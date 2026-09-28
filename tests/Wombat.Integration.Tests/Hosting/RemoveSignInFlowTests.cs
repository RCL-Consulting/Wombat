extern alias WombatWeb;

using System.Net;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Accounts;
using Wombat.Domain.Audit;
using Wombat.Infrastructure.Identity;
using Wombat.Infrastructure.Persistence;
using Wombat.Integration.Tests.MultiSourceFeedback;
using WombatWeb::Wombat.Web.Navigation;
using WombatWeb::Wombat.Web.Security;

namespace Wombat.Integration.Tests.Hosting;

/// <summary>
/// My account's Remove of an institutional sign-in, end to end (T339, flow 02; T286's removal; the round 2 review's A8,
/// B-D6, E2, E3): the dialog's form posts to <c>/account/external-logins/remove</c>, which checks the session, the password
/// with lockout, and the last way in, removes the sign-in through the audited command, issues the cookie again, and sends
/// the browser back to My account with a code.
/// </summary>
/// <remarks>
/// The whole of <c>Wombat.Web</c>'s <c>Program.cs</c> on a PostgreSQL schema of its own, hosted by the MSF respondent
/// page's fixture (<see cref="MsfRespondPageFlowTests.WebHost" />). The sign-ins are written straight into the store: no
/// identity provider runs here, and the removal reads only the store.
/// </remarks>
public sealed class RemoveSignInFlowTests : IClassFixture<MsfRespondPageFlowTests.WebHost>
{
    private const string RemovePath = "/account/external-logins/remove";

    private static int _nextSubnet;

    private readonly MsfRespondPageFlowTests.WebHost _host;

    public RemoveSignInFlowTests(MsfRespondPageFlowTests.WebHost host)
    {
        _host = host;
    }

    [Fact]
    public async Task WithTheRightPassword_TheSignInIsRemoved_ThroughTheAuditedCommand_AndTheSessionGoesOnWithANewCookie()
    {
        var account = await _host.CreateAssessorAsync(NewEmail());
        await LinkAsync(account.Id, ("kgk", "subject-1", "Kgosi Kgari Test Hospital"));
        using var browser = NewBrowser();
        await _host.SignInAsync(browser, account.Email!);
        var page = await LoadAsync(browser, "/account/profile");
        page.QuerySelector($"button[aria-label='Remove your Kgosi Kgari Test Hospital sign-in']").Should().NotBeNull("guard: a live Remove");

        using var remove = await PostAsync(browser, page, "kgk", MsfRespondPageFlowTests.WebHost.SignInPassword);

        remove.StatusCode.Should().Be(HttpStatusCode.Redirect);
        remove.Headers.Location!.ToString().Should().Be("/account/profile?status=sign-in-removed&provider=kgk");
        SetCookies(remove).Should().Contain(cookie => cookie.StartsWith(".AspNetCore.Identity.Application=", StringComparison.Ordinal)
                && !cookie.StartsWith(".AspNetCore.Identity.Application=;", StringComparison.Ordinal),
            "the removal changed the stamp, so the cookie is issued again in the same request");
        (await LoginsAsync(account.Id)).Should().BeEmpty();

        // The new cookie signs the browser in: the old one's stamp is stale.
        var after = await LoadAsync(browser, remove.Headers.Location.ToString());
        after.QuerySelector(".my-account-card .action-result .alert-success").Should().NotBeNull("the page says the sign-in was removed");

        var row = await AuditRowAsync(account.Id);
        row.Success.Should().BeTrue();
        row.InstitutionId.Should().Be(account.InstitutionId, "the row is stamped with the caller's institution (T101)");
        row.SummaryJson.Should().Contain("kgk").And.NotContain(MsfRespondPageFlowTests.WebHost.SignInPassword, "never the password");
    }

    [Fact]
    public async Task AWrongPassword_RemovesNothing_ComesBackAsACode_AndCountsTowardsTheLockout()
    {
        var account = await _host.CreateAssessorAsync(NewEmail());
        await LinkAsync(account.Id, ("kgk", "subject-1", null));
        using var browser = NewBrowser();
        await _host.SignInAsync(browser, account.Email!);

        using var remove = await PostAsync(browser, await LoadAsync(browser, "/account/profile"), "kgk", "Not-the-Pa55word!");

        remove.Headers.Location!.ToString().Should().Be("/account/profile?error=RemoveWrongPassword&provider=kgk");
        (await LoginsAsync(account.Id)).Should().Equal("kgk");
        (await AccountAsync(account.Id)).AccessFailedCount.Should().Be(1, "E3: checked with lockout, as the sign-in page checks it");
        (await AuditRowsAsync(account.Id)).Should().BeEmpty("the command is never sent for a refused password");
    }

    [Fact]
    public async Task TheFifthWrongPassword_LocksTheAccount_AndSignsTheSessionOut()
    {
        var account = await _host.CreateAssessorAsync(NewEmail());
        await LinkAsync(account.Id, ("kgk", "subject-1", null));
        using var browser = NewBrowser();
        await _host.SignInAsync(browser, account.Email!);
        var page = await LoadAsync(browser, "/account/profile");
        var stampBefore = (await AccountAsync(account.Id)).SecurityStamp;

        for (var attempt = 1; attempt <= 4; attempt++)
        {
            using var wrong = await PostAsync(browser, page, "kgk", $"Wrong-Pa55word-{attempt}!");
            wrong.Headers.Location!.ToString().Should().Be("/account/profile?error=RemoveWrongPassword&provider=kgk");
        }

        // With a switch's word not yet shown, which the sign-out takes too (T317; the review of the t339 branch).
        using var fifth = await PostAsync(browser, page, "kgk", "Wrong-Pa55word-5!",
            cookie: $"{ActingRoleSwitchResults.CookieName}=a-word-not-yet-shown");

        fifth.Headers.Location!.ToString().Should().Be("/account/login?error=LockedSignedOut", "E2, as change password");
        SetCookies(fifth).Should().Contain(cookie => cookie.StartsWith(".AspNetCore.Identity.Application=;", StringComparison.Ordinal),
            "the session is signed out");
        SetCookies(fifth).Should().Contain(cookie => cookie.StartsWith(ActingRoleSwitchResults.CookieName + "=;", StringComparison.Ordinal),
            "the acting role's word goes with the sign-out");
        (await LoginsAsync(account.Id)).Should().Equal("kgk");
        var locked = await AccountAsync(account.Id);
        locked.LockoutEnd.Should().NotBeNull();
        locked.SecurityStamp.Should().NotBe(stampBefore,
            "the session is ended everywhere, not only in this browser: a copied cookie stops working too (E2, E3)");

        // Recorded, as the sign-in page's lockout is: stamped with the account's institution, never the password (the review
        // of the t339 branch).
        var row = (await AuditRowsAsync(account.Id, "RemoveSignInLockedOut")).Should().ContainSingle().Subject;
        row.Success.Should().BeFalse();
        row.Category.Should().Be(AuditCategory.Authentication);
        row.InstitutionId.Should().Be(account.InstitutionId);
        (row.SummaryJson + row.ErrorMessage).Should().NotContain("Wrong-Pa55word-5!", "never the password");
    }

    /// <summary>
    /// The review of the t339 branch: an account locked already, by a stranger's five wrong passwords at the sign-in page,
    /// is refused inside the dialog with the wait, and nothing else changes. Identity answers a locked account's check with
    /// a lockout without checking the password, so until the review the owner's Remove, with the right password, ended
    /// every session of the account and blamed the owner.
    /// </summary>
    [Fact]
    public async Task AnAccountLockedAlready_IsRefusedInTheDialogWithTheWait_AndTheSessionGoesOn()
    {
        var account = await _host.CreateAssessorAsync(NewEmail());
        await LinkAsync(account.Id, ("kgk", "subject-1", "Kgosi Kgari Test Hospital"));
        using var browser = NewBrowser();
        await _host.SignInAsync(browser, account.Email!);
        var page = await LoadAsync(browser, "/account/profile");

        using var stranger = NewBrowser();
        for (var guess = 1; guess <= 5; guess++)
        {
            await FailedSignInAsync(stranger, account.Email!, $"Stranger-{guess}-Pa55word!");
        }

        var stampBefore = (await AccountAsync(account.Id)).SecurityStamp;

        using var remove = await PostAsync(browser, page, "kgk", MsfRespondPageFlowTests.WebHost.SignInPassword);

        remove.Headers.Location!.ToString().Should().Be("/account/profile?error=RemoveAccountLocked&provider=kgk&minutes=15");
        SetCookies(remove).Should().NotContain(cookie => cookie.StartsWith(".AspNetCore.Identity.Application", StringComparison.Ordinal),
            "the session is neither signed out nor issued a cookie");
        (await LoginsAsync(account.Id)).Should().Equal("kgk");
        (await AccountAsync(account.Id)).SecurityStamp.Should().Be(stampBefore, "no session of the account was ended");
        (await AuditRowsAsync(account.Id, "RemoveSignInLockedOut")).Should().BeEmpty("this post tripped no lock");

        // Still signed in; the dialog opens again for the sign-in, saying why.
        var after = await LoadAsync(browser, remove.Headers.Location.ToString());
        after.Body!.TextContent.Should().Contain(
            "Your sign-in was not removed. Your account is locked. Wait 15 minutes, then try again.");
    }

    /// <summary>
    /// The Remove dialog's password check shares the sign-in throttle, as change password's does (T156, T265; the review of
    /// the t339 branch): ten failed checks from one address in five minutes, counted across the pages, and the refusal
    /// comes back into the dialog.
    /// </summary>
    [Fact]
    public async Task ThePasswordCheck_SharesTheSignInThrottle_AndItsRefusalComesBackToTheDialog()
    {
        var account = await _host.CreateAssessorAsync(NewEmail());
        await LinkAsync(account.Id, ("kgk", "subject-1", null));
        var address = NextAddress();
        using var browser = _host.NewBrowser(address);
        await _host.SignInAsync(browser, account.Email!);
        var page = await LoadAsync(browser, "/account/profile");

        // Six failed sign-ins from this address, to addresses no account has, in another tab, then four wrong passwords
        // here: ten failed checks, one short of the account's lockout.
        using var signedOut = _host.NewBrowser(address);
        for (var attempt = 1; attempt <= 6; attempt++)
        {
            await FailedSignInAsync(signedOut, NewEmail(), "Not-the-Pa55word!");
        }

        var codes = new List<string>();
        string? retryAfter = null;
        for (var attempt = 1; attempt <= 5; attempt++)
        {
            using var guess = await PostAsync(browser, page, "kgk", $"Wrong-Pa55word-{attempt}!");
            codes.Add(guess.Headers.Location!.ToString());
            retryAfter = guess.Headers.RetryAfter?.ToString();
        }

        codes.Take(4).Should().OnlyContain(code => code == "/account/profile?error=RemoveWrongPassword&provider=kgk",
            "guard: four are checked");
        codes[4].Should().Be($"/account/profile?error={ProfileOutcome.RemoveTooManyAttempts}&provider=kgk",
            "a signed-in user is sent back to the dialog, not to the sign-in page");
        int.Parse(retryAfter!).Should().BeInRange(1, 300, "the seconds left of the address's five minutes");
        (await AccountAsync(account.Id)).AccessFailedCount.Should().Be(4, "the fifth was refused before its password was checked");
        (await LoginsAsync(account.Id)).Should().Equal("kgk");

        var after = await LoadAsync(browser, codes[4]);
        after.Body!.TextContent.Should().Contain(
            "Your sign-in was not removed. Too many attempts from this network. Wait a few minutes and try again.");
    }

    [Fact]
    public async Task TheLastWayIn_IsRefusedByTheServer_EvenWhenThePageOffersNoRemove()
    {
        var account = await _host.CreateAssessorAsync(NewEmail());
        await LinkAsync(account.Id, ("kgk", "subject-1", null));
        using var browser = NewBrowser();
        await _host.SignInAsync(browser, account.Email!);
        await MakeInstitutionOnlyAsync(account.Id);
        var page = await LoadAsync(browser, "/account/profile");
        page.QuerySelector(".signin-method button[disabled]").Should().NotBeNull("guard: the page offers no live Remove");

        // A crafted post: the page's own token, and the provider.
        using var remove = await PostAsync(browser, page, "kgk", password: null);

        remove.Headers.Location!.ToString().Should().Be("/account/profile?error=RemoveLastSignIn&provider=kgk");
        (await LoginsAsync(account.Id)).Should().Equal("kgk");
    }

    [Fact]
    public async Task ASecondTab_CannotRemoveTheOtherSignIn_WhenTheFirstLeftItTheLastWayIn()
    {
        var account = await _host.CreateAssessorAsync(NewEmail());
        await LinkAsync(account.Id, ("kgk", "subject-1", null), ("marula", "subject-2", null));
        using var browser = NewBrowser();
        await _host.SignInAsync(browser, account.Email!);
        await MakeInstitutionOnlyAsync(account.Id);
        var firstTab = await LoadAsync(browser, "/account/profile");
        var secondTab = await LoadAsync(browser, "/account/profile");
        secondTab.QuerySelectorAll(".signin-method button:not([disabled])").Should().HaveCount(2, "guard: both tabs offer both");

        using var first = await PostAsync(browser, firstTab, "marula", password: null);
        first.Headers.Location!.ToString().Should().Be("/account/profile?status=sign-in-removed&provider=marula",
            "an account with no password removes one of two with no password asked (E3)");

        using var second = await PostAsync(browser, secondTab, "kgk", password: null);

        second.Headers.Location!.ToString().Should().Be("/account/profile?error=RemoveLastSignIn&provider=kgk");
        (await LoginsAsync(account.Id)).Should().Equal("kgk");
    }

    [Fact]
    public async Task TheRemoval_IsTheSignedInAccounts_AnotherAccountsSignInThroughTheSameProviderStays()
    {
        var other = await _host.CreateAssessorAsync(NewEmail());
        await LinkAsync(other.Id, ("kgk", "other-subject", null));
        var account = await _host.CreateAssessorAsync(NewEmail());
        await LinkAsync(account.Id, ("kgk", "subject-1", null));
        using var browser = NewBrowser();
        await _host.SignInAsync(browser, account.Email!);

        using var remove = await PostAsync(browser, await LoadAsync(browser, "/account/profile"), "kgk",
            MsfRespondPageFlowTests.WebHost.SignInPassword, extra: [new("UserId", other.Id), new("ProviderKey", "other-subject")]);

        remove.Headers.Location!.ToString().Should().StartWith("/account/profile?status=sign-in-removed");
        (await LoginsAsync(account.Id)).Should().BeEmpty();
        (await LoginsAsync(other.Id)).Should().Equal(["kgk"], "the form names no account and no subject; the cookie's account is the one");
    }

    [Fact]
    public async Task APostWithoutTheAntiforgeryToken_IsRefused_AndNothingIsRemoved()
    {
        var account = await _host.CreateAssessorAsync(NewEmail());
        await LinkAsync(account.Id, ("kgk", "subject-1", null));
        using var browser = NewBrowser();
        await _host.SignInAsync(browser, account.Email!);

        using var remove = await browser.PostAsync(RemovePath, new FormUrlEncodedContent(
        [
            new("Provider", "kgk"),
            new("Password", MsfRespondPageFlowTests.WebHost.SignInPassword)
        ]));

        remove.StatusCode.Should().Be(HttpStatusCode.BadRequest, "a cross-site post cannot remove a sign-in");
        (await LoginsAsync(account.Id)).Should().Equal("kgk");
    }

    // As the name and password endpoints (the T265 review): a session the account no longer accepts reaches the endpoint
    // within the stamp validator's minute. It is signed out, not issued a cookie, and nothing is removed.
    [Fact]
    public async Task ASessionThatHasEnded_IsSignedOut_NotIssuedACookie_AndNothingIsRemoved()
    {
        var account = await _host.CreateAssessorAsync(NewEmail());
        await LinkAsync(account.Id, ("kgk", "subject-1", null));
        using var browser = NewBrowser();
        await _host.SignInAsync(browser, account.Email!);
        var page = await LoadAsync(browser, "/account/profile");

        await using (var scope = _host.Factory.Services.CreateAsyncScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<WombatIdentityUser>>();
            (await users.UpdateSecurityStampAsync((await users.FindByIdAsync(account.Id))!)).Succeeded.Should().BeTrue();
        }

        using var remove = await PostAsync(browser, page, "kgk", MsfRespondPageFlowTests.WebHost.SignInPassword);

        remove.Headers.Location!.ToString().Should().Be("/account/login?error=SessionEnded&returnUrl=%2Faccount%2Fprofile");
        SetCookies(remove).Should().Contain(cookie => cookie.StartsWith(".AspNetCore.Identity.Application=;", StringComparison.Ordinal));
        (await LoginsAsync(account.Id)).Should().Equal("kgk");
    }

    [Fact]
    public async Task AProviderTheAccountHasNoSignInThrough_IsRefused_AsAFailure()
    {
        var account = await _host.CreateAssessorAsync(NewEmail());
        await LinkAsync(account.Id, ("kgk", "subject-1", null));
        using var browser = NewBrowser();
        await _host.SignInAsync(browser, account.Email!);

        using var remove = await PostAsync(browser, await LoadAsync(browser, "/account/profile"), "marula",
            MsfRespondPageFlowTests.WebHost.SignInPassword);

        remove.Headers.Location!.ToString().Should().Be("/account/profile?error=RemoveFailed&provider=marula");
        (await LoginsAsync(account.Id)).Should().Equal("kgk");
        (await AccountAsync(account.Id)).AccessFailedCount.Should().Be(0, "no password was checked");
    }

    private HttpClient NewBrowser() => _host.NewBrowser(NextAddress());

    private static string NextAddress() => $"10.236.{Interlocked.Increment(ref _nextSubnet)}.7";

    /// <summary>A sign-in through the sign-in page's own form that is refused: a failed password check the throttle counts.</summary>
    private static async Task FailedSignInAsync(HttpClient browser, string email, string password)
    {
        using var loginPage = await browser.GetAsync("/account/login");
        var form = new HtmlParser().ParseDocument(await loginPage.Content.ReadAsStringAsync())
            .QuerySelector("form[action='/account/login/submit']")!;

        using var signIn = await browser.PostAsync("/account/login/submit", new FormUrlEncodedContent(
        [
            new("__RequestVerificationToken", form.QuerySelector("input[name=__RequestVerificationToken]")!.GetAttribute("value")!),
            new("Email", email),
            new("Password", password)
        ]));

        signIn.Headers.Location!.ToString().Should().Be("/account/login?error=Refused", "guard: a failed check");
    }

    private static string NewEmail() => $"remove-{Guid.NewGuid():N}@example.test";

    private static IReadOnlyList<string> SetCookies(HttpResponseMessage response)
        => response.Headers.TryGetValues("Set-Cookie", out var cookies) ? cookies.ToList() : [];

    private static async Task<IHtmlDocument> LoadAsync(HttpClient browser, string address)
    {
        using var response = await browser.GetAsync(address);
        response.StatusCode.Should().Be(HttpStatusCode.OK, $"guard: {address} loads signed in ({response.Headers.Location})");
        return new HtmlParser().ParseDocument(await response.Content.ReadAsStringAsync());
    }

    /// <summary>
    /// Posts the Remove dialog's form as the browser does: its token, the provider in its hidden field, and the password
    /// when given. The dialog is drawn only once Remove is pressed, so the token is the page's (the name form's): one
    /// token per page and user.
    /// </summary>
    private static Task<HttpResponseMessage> PostAsync(
        HttpClient browser,
        IHtmlDocument page,
        string provider,
        string? password,
        IReadOnlyList<KeyValuePair<string, string>>? extra = null,
        string? cookie = null)
    {
        var token = page.QuerySelector("input[name=__RequestVerificationToken]")!.GetAttribute("value")!;
        var fields = new List<KeyValuePair<string, string>> { new("__RequestVerificationToken", token), new("Provider", provider) };
        if (password is not null)
        {
            fields.Add(new("Password", password));
        }

        fields.AddRange(extra ?? []);
        var request = new HttpRequestMessage(HttpMethod.Post, RemovePath) { Content = new FormUrlEncodedContent(fields) };
        if (cookie is not null)
        {
            // Beside the browser's own cookies, which its handler adds.
            request.Headers.Add("Cookie", cookie);
        }

        return browser.SendAsync(request);
    }

    private async Task LinkAsync(string userId, params (string Provider, string Subject, string? DisplayName)[] logins)
    {
        await using var scope = _host.Factory.Services.CreateAsyncScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<WombatIdentityUser>>();
        var user = (await users.FindByIdAsync(userId))!;
        foreach (var login in logins)
        {
            // The subject made unique: the fixture's database is shared by every test here, and a provider's subject is
            // one account's.
            var added = await users.AddLoginAsync(user, new UserLoginInfo(login.Provider, $"{login.Subject}-{Guid.NewGuid():N}", login.DisplayName));
            added.Succeeded.Should().BeTrue("guard: the sign-in is linked ({0})", string.Join("; ", added.Errors.Select(error => error.Code)));
        }
    }

    /// <summary>
    /// The account as its institution's sign-in provisions one (no password it may use), written straight to the row so
    /// the session signed in with the password goes on: the security stamp is left as it is.
    /// </summary>
    private async Task MakeInstitutionOnlyAsync(string userId)
    {
        await using var scope = _host.Factory.Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Users
            .Where(user => user.Id == userId)
            .ExecuteUpdateAsync(update => update
                .SetProperty(user => user.AllowLocalPassword, false)
                .SetProperty(user => user.PasswordHash, (string?)null));
    }

    private async Task<List<string>> LoginsAsync(string userId)
    {
        await using var scope = _host.Factory.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().UserLogins
            .Where(login => login.UserId == userId)
            .Select(login => login.LoginProvider)
            .OrderBy(provider => provider)
            .ToListAsync();
    }

    private async Task<WombatIdentityUser> AccountAsync(string userId)
    {
        await using var scope = _host.Factory.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Users.AsNoTracking().SingleAsync(user => user.Id == userId);
    }

    private async Task<List<AuditEntry>> AuditRowsAsync(string userId, string action = nameof(RemoveMyInstitutionalSignInCommand))
    {
        await using var scope = _host.Factory.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Set<AuditEntry>()
            .AsNoTracking()
            .Where(entry => entry.ActorUserId == userId && entry.Action == action)
            .ToListAsync();
    }

    private async Task<AuditEntry> AuditRowAsync(string userId)
        => (await AuditRowsAsync(userId)).Should().ContainSingle("the removal writes one audit row").Subject;
}
