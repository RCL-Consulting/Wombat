using System.Net;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Infrastructure.Identity;
using Wombat.Integration.Tests.MultiSourceFeedback;

namespace Wombat.Integration.Tests.Hosting;

/// <summary>
/// My account's name, end to end (T335, flow 01; the review of the t335 branch): the form posts to an endpoint that saves
/// the name and issues the sign-in cookie again in one request, and the page it comes back to, a full load, shows the new
/// name in the account row.
/// </summary>
/// <remarks>
/// <para>
/// The shell names the person from the cookie's <c>display_name</c> claim, so it reads no database. Until the review the
/// page saved the name in its circuit, which cannot write the cookie: the account row went on showing the old name in that
/// circuit and on every page after it, until the stamp validator next rebuilt the cookie. As change password does (T265),
/// the endpoint checks the session first, and a refusal comes back as a code.
/// </para>
/// <para>
/// The whole of <c>Wombat.Web</c>'s <c>Program.cs</c> on a PostgreSQL schema of its own, hosted by the MSF respondent
/// page's fixture (<see cref="MsfRespondPageFlowTests.WebHost" />). Pages are read as a full load delivers them.
/// </para>
/// </remarks>
public sealed class ProfileFlowTests : IClassFixture<MsfRespondPageFlowTests.WebHost>
{
    private static int _nextSubnet;

    private readonly MsfRespondPageFlowTests.WebHost _host;

    public ProfileFlowTests(MsfRespondPageFlowTests.WebHost host)
    {
        _host = host;
    }

    [Fact]
    public async Task ASavedName_IsTheAccountRowsName_OnThePageTheSaveComesBackTo_AndOnEveryPageAfter()
    {
        var account = await _host.CreateAssessorAsync(NewEmail());
        using var browser = NewBrowser();
        await SignInAsync(browser, account.Email!);
        var before = await LoadAsync(browser, "/account/profile");
        AccountRowName(before).Should().Be("Signed Assessor", "guard: the name the account was made with");

        using var save = await PostAsync(browser, before, " Thandeka ", "Zulu-Mokoena");

        save.StatusCode.Should().Be(HttpStatusCode.Redirect);
        save.Headers.Location!.ToString().Should().Be("/account/profile?status=saved");
        SetCookies(save).Should().Contain(cookie => cookie.StartsWith(".AspNetCore.Identity.Application=", StringComparison.Ordinal)
                && !cookie.StartsWith(".AspNetCore.Identity.Application=;", StringComparison.Ordinal),
            "the cookie is issued again, with the new name's claim, in the same request");

        var after = await LoadAsync(browser, "/account/profile?status=saved");
        AccountRowName(after).Should().Be("Thandeka Zulu-Mokoena", "the account row reads the new cookie's claim");
        after.QuerySelector(".account-row a.account-link")!.GetAttribute("title").Should().Be("Thandeka Zulu-Mokoena");
        after.QuerySelector(".action-result .alert-success")!.TextContent.Trim().Should().Be("Name saved.", "T339, E10");
        after.QuerySelector("#profile-first-name")!.GetAttribute("value").Should().Be("Thandeka", "stored trimmed");

        AccountRowName(await LoadAsync(browser, "/")).Should().Be("Thandeka Zulu-Mokoena", "and on every page after it");
        (await StoredAsync(account.Id)).Should().Be(("Thandeka", "Zulu-Mokoena"));
    }

    // T339 (flow 02): the blank field by name, so the page marks it alone and keeps the other; both blank, both.
    [Theory]
    [InlineData("", "Zulu", "FirstNameMissing", "Your name was not saved. Enter your first name.")]
    [InlineData(null, "Zulu", "FirstNameMissing", "Your name was not saved. Enter your first name.")]
    [InlineData("Thandeka", "   ", "LastNameMissing", "Your name was not saved. Enter your last name.")]
    [InlineData("Thandeka", null, "LastNameMissing", "Your name was not saved. Enter your last name.")]
    [InlineData(" ", null, "NameMissing", "Your name was not saved. Enter your first name and your last name.")]
    public async Task ANameLeftBlank_IsRefused_AsACode_AndNothingIsSaved(string? firstName, string? lastName, string code, string words)
    {
        var account = await _host.CreateAssessorAsync(NewEmail());
        using var browser = NewBrowser();
        await SignInAsync(browser, account.Email!);

        using var save = await PostAsync(browser, await LoadAsync(browser, "/account/profile"), firstName, lastName);

        save.Headers.Location!.ToString().Should().Be($"/account/profile?error={code}");
        var page = await LoadAsync(browser, save.Headers.Location.ToString());
        page.QuerySelector(".action-result .alert-danger")!.TextContent.Trim().Should().Be(words);
        (await StoredAsync(account.Id)).Should().Be(("Signed", "Assessor"));
    }

    // Change password redirects here with ?status=password-updated (T339: lane A's ChangePasswordOutcome.UpdatedUrl);
    // the page says so under its header, in its own words, and the result is focused by the circuit, not autofocused.
    [Fact]
    public async Task PasswordUpdated_IsSaidUnderTheHeader()
    {
        var account = await _host.CreateAssessorAsync(NewEmail());
        using var browser = NewBrowser();
        await SignInAsync(browser, account.Email!);

        var page = await LoadAsync(browser, "/account/profile?status=password-updated");

        var done = page.QuerySelector(".my-account > .action-result .alert-success");
        done.Should().NotBeNull("under the header, above the cards");
        done!.TextContent.Trim().Should().Be("Password updated.");
        done.ParentElement!.GetAttribute("tabindex").Should().Be("-1", "the circuit moves the focus to it once it has drawn the page");
    }

    [Fact]
    public async Task ANameTooLong_IsRefused_AsACode_AndNothingIsSaved()
    {
        var account = await _host.CreateAssessorAsync(NewEmail());
        using var browser = NewBrowser();
        await SignInAsync(browser, account.Email!);

        using var save = await PostAsync(browser, await LoadAsync(browser, "/account/profile"), new string('a', 101), "Zulu");

        save.Headers.Location!.ToString().Should().Be("/account/profile?error=NameTooLong");
        (await StoredAsync(account.Id)).Should().Be(("Signed", "Assessor"));
    }

    // As change password (the T265 review): a session the account no longer accepts reaches the endpoint within the stamp
    // validator's minute. It is signed out, not issued a cookie with the account's current stamp, and nothing is saved.
    [Fact]
    public async Task ASessionThatHasEnded_IsSignedOut_NotIssuedACookie_AndNothingIsSaved()
    {
        var account = await _host.CreateAssessorAsync(NewEmail());
        using var browser = NewBrowser();
        await SignInAsync(browser, account.Email!);
        var page = await LoadAsync(browser, "/account/profile");

        await using (var scope = _host.Factory.Services.CreateAsyncScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<WombatIdentityUser>>();
            (await users.UpdateSecurityStampAsync((await users.FindByIdAsync(account.Id))!)).Succeeded.Should().BeTrue(
                "guard: an administrator's lock, or a change of roles, changes the stamp");
        }

        using var save = await PostAsync(browser, page, "Thandeka", "Zulu");

        save.Headers.Location!.ToString().Should().Be("/account/login?error=SessionEnded&returnUrl=%2Faccount%2Fprofile");
        SetCookies(save).Should().Contain(cookie => cookie.StartsWith(".AspNetCore.Identity.Application=;", StringComparison.Ordinal),
            "the session is ended here, not renewed");
        (await StoredAsync(account.Id)).Should().Be(("Signed", "Assessor"));
    }

    private HttpClient NewBrowser() => _host.NewBrowser($"10.235.{Interlocked.Increment(ref _nextSubnet)}.7");

    private static string NewEmail() => $"profile-{Guid.NewGuid():N}@example.test";

    private static string? AccountRowName(IHtmlDocument page) => page.QuerySelector(".account-row .account-name")?.TextContent.Trim();

    private static IReadOnlyList<string> SetCookies(HttpResponseMessage response)
        => response.Headers.TryGetValues("Set-Cookie", out var cookies) ? cookies.ToList() : [];

    private static async Task<IHtmlDocument> LoadAsync(HttpClient browser, string address)
    {
        using var response = await browser.GetAsync(address);
        response.StatusCode.Should().Be(HttpStatusCode.OK, $"guard: {address} loads signed in ({response.Headers.Location})");
        return new HtmlParser().ParseDocument(await response.Content.ReadAsStringAsync());
    }

    /// <summary>Posts My account's form as the browser does: its token, and the two names as given (a null one left out).</summary>
    private static Task<HttpResponseMessage> PostAsync(HttpClient browser, IHtmlDocument page, string? firstName, string? lastName)
    {
        var form = page.QuerySelector("form[action='/account/profile/submit']");
        form.Should().NotBeNull("guard: the page's form posts to the endpoint");

        var fields = new List<KeyValuePair<string, string>>
        {
            new("__RequestVerificationToken", form!.QuerySelector("input[name=__RequestVerificationToken]")!.GetAttribute("value")!)
        };
        if (firstName is not null)
        {
            fields.Add(new("FirstName", firstName));
        }

        if (lastName is not null)
        {
            fields.Add(new("LastName", lastName));
        }

        return browser.PostAsync("/account/profile/submit", new FormUrlEncodedContent(fields));
    }

    private static async Task SignInAsync(HttpClient browser, string email)
    {
        using var loginPage = await browser.GetAsync("/account/login");
        var form = new HtmlParser().ParseDocument(await loginPage.Content.ReadAsStringAsync())
            .QuerySelector("form[action='/account/login/submit']")!;

        using var signIn = await browser.PostAsync("/account/login/submit", new FormUrlEncodedContent(
        [
            new("__RequestVerificationToken", form.QuerySelector("input[name=__RequestVerificationToken]")!.GetAttribute("value")!),
            new("Email", email),
            new("Password", MsfRespondPageFlowTests.WebHost.SignInPassword)
        ]));

        signIn.StatusCode.Should().Be(HttpStatusCode.Redirect);
        signIn.Headers.Location!.ToString().Should().NotContain("/account/login", "guard: the sign-in succeeded");
    }

    private async Task<(string? FirstName, string? LastName)> StoredAsync(string userId)
    {
        await using var scope = _host.Factory.Services.CreateAsyncScope();
        var user = await scope.ServiceProvider.GetRequiredService<UserManager<WombatIdentityUser>>().FindByIdAsync(userId);
        return (user!.FirstName, user.LastName);
    }
}
