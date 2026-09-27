extern alias WombatWeb;

using System.Net;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Mvc.Testing.Handlers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Identity;
using Wombat.Infrastructure.Persistence;
using Wombat.Integration.Tests.MultiSourceFeedback;
using WombatWeb::Wombat.Web.Navigation;

namespace Wombat.Integration.Tests.Hosting;

/// <summary>
/// T335, flow 01 (D1, W-010) and T317's remembered view, end to end: the acting role is switched by one GET address,
/// stored with the account, carried into the sign-in cookie, and said once on the page the switch lands on.
/// </summary>
/// <remarks>
/// <para>
/// The whole of <c>Wombat.Web</c>'s <c>Program.cs</c> on a PostgreSQL schema of its own, hosted by the MSF respondent
/// page's fixture (<see cref="MsfRespondPageFlowTests.WebHost" />). A browser is a client with its own cookies and its own
/// address. Pages are read as a full load delivers them: prerendered for an interactive page, rendered for a static one.
/// </para>
/// <para>
/// Until T335 the choice was a 30-day browser cookie, <c>wombat_preferred_dashboard_role</c>, tied to no account: it
/// survived sign-out, and the next person to sign in on the browser landed on the last one's choice (T317, F-2.34a).
/// </para>
/// </remarks>
public sealed class ActingRoleFlowTests : IClassFixture<MsfRespondPageFlowTests.WebHost>
{
    private const string Password = MsfRespondPageFlowTests.WebHost.SignInPassword;
    private const string SignInCookie = ".AspNetCore.Identity.Application";
    private const string OldCookie = "wombat_preferred_dashboard_role";

    private static readonly Uri Origin = new("http://localhost");

    private static int _nextSubnet;

    private readonly MsfRespondPageFlowTests.WebHost _host;

    public ActingRoleFlowTests(MsfRespondPageFlowTests.WebHost host)
    {
        _host = host;
    }

    [Fact]
    public async Task ASwitchToAHeldRole_IsStoredWithTheAccount_AndSurvivesSignOutAndSignIn()
    {
        var zulu = await CreateUserAsync("Thandi", "Zulu", WombatRoles.CommitteeMember, WombatRoles.Assessor);
        var (browser, cookies) = NewBrowser();
        using (browser)
        {
            await SignInAsync(browser, zulu.Email!);
            (await HomeAsync(browser)).ActingAs.Should().Be("Committee member", "guard: the precedence, before any choice");

            using var switched = await browser.GetAsync("/dashboard/switch/Assessor");

            switched.StatusCode.Should().Be(HttpStatusCode.Redirect);
            switched.Headers.Location!.ToString().Should().Be("/", "a switch with no return address lands on the new role's Home");
            (await StoredActingRoleAsync(zulu.Id)).Should().Be(WombatRoles.Assessor);
            var reissued = SetCookies(switched).Single(cookie => cookie.StartsWith(SignInCookie + "=", StringComparison.Ordinal));
            reissued.Should().NotContainEquivalentOf("expires=",
                "issued again as the sign-in issued it: a session cookie, since she did not ask to be remembered");
            SetCookies(switched).Should().NotContain(cookie => cookie.StartsWith(OldCookie, StringComparison.Ordinal));

            var home = await HomeAsync(browser);
            home.ActingAs.Should().Be("Assessor");
            home.Alert.Should().Be("You are now acting as Assessor.");
            home.SwitchBack.Should().BeNull("on Home the sidebar's switch is the way back (R2-Landing-Assessor)");

            await SignOutAsync(browser);
            cookies.Container.GetCookies(Origin).Cast<Cookie>().Select(cookie => cookie.Name)
                .Should().NotContain([OldCookie, ActingRoleSwitchResults.CookieName], "nothing of her choice is left in the browser");

            await SignInAsync(browser, zulu.Email!);
            var again = await HomeAsync(browser);
            again.ActingAs.Should().Be("Assessor", "the choice is the account's, and follows her across sign-ins");
            again.Alert.Should().BeNull("a sign-in is not a switch");
        }
    }

    // T317 (F-2.34a): Naidoo signed in after Zulu on the same browser and landed on her choice.
    [Fact]
    public async Task AnotherAccount_SigningInOnTheSameBrowser_LandsByThePrecedence()
    {
        var zulu = await CreateUserAsync("Thandi", "Zulu", WombatRoles.CommitteeMember, WombatRoles.Assessor);
        var naidoo = await CreateUserAsync("David", "Naidoo", WombatRoles.CommitteeMember, WombatRoles.Assessor);
        var (browser, cookies) = NewBrowser();
        using (browser)
        {
            await SignInAsync(browser, zulu.Email!);
            using (var switched = await browser.GetAsync("/dashboard/switch/Assessor"))
            {
                switched.StatusCode.Should().Be(HttpStatusCode.Redirect);
            }

            // Signed out before the page the switch left its word for was loaded: sign-out takes the word away (T317).
            await SignOutAsync(browser);
            cookies.Container.GetCookies(Origin).Cast<Cookie>().Select(cookie => cookie.Name)
                .Should().NotContain(ActingRoleSwitchResults.CookieName, "nothing of her choice outlives her sign-out here");
            await SignInAsync(browser, naidoo.Email!);

            var home = await HomeAsync(browser);
            home.ActingAs.Should().Be("Committee member");
            home.Alert.Should().BeNull();
            (await StoredActingRoleAsync(naidoo.Id)).Should().BeNull();
            (await StoredActingRoleAsync(zulu.Id)).Should().Be(WombatRoles.Assessor, "hers is kept for her next sign-in");
        }
    }

    [Theory]
    [InlineData(WombatRoles.Administrator)]
    [InlineData(WombatRoles.Trainee)]
    [InlineData("Registrar")]
    [InlineData("assessor")]
    public async Task ARoleNotHeld_WritesNothing_SaysNothing_AndStillOpensTheReturnAddress(string role)
    {
        var zulu = await CreateUserAsync("Thandi", "Zulu", WombatRoles.CommitteeMember, WombatRoles.Assessor);
        var (browser, _) = NewBrowser();
        using (browser)
        {
            await SignInAsync(browser, zulu.Email!);

            using var refused = await browser.GetAsync($"/dashboard/switch/{Uri.EscapeDataString(role)}?returnUrl=%2Faccount%2Fprofile");

            refused.StatusCode.Should().Be(HttpStatusCode.Redirect);
            refused.Headers.Location!.ToString().Should().Be("/account/profile");
            SetCookies(refused).Should().BeEmpty("nothing is stored, no cookie is issued again, and no word is left");
            (await StoredActingRoleAsync(zulu.Id)).Should().BeNull();

            (await PageAsync(browser, "/account/profile")).Alert.Should().BeNull();
            (await HomeAsync(browser)).ActingAs.Should().Be("Committee member");
        }
    }

    // The return address is followed only when it is a path on this site. Uri.TryCreate(…, Relative) accepts the second and
    // fourth; a browser follows them to another host. A dropped address still switches, and lands on the new role's Home.
    // One outside ASCII is followed percent-encoded: as it was, Kestrel refused the Location header and the switch answered
    // 500 (the review of the t335 branch). SignInReturnAddressFlowTests.Location holds the header to Kestrel's rule.
    [Theory]
    [InlineData("/account/profile", "/account/profile")]
    [InlineData("/activities/inbox?page=2", "/activities/inbox?page=2")]
    [InlineData("//evil.example", "/")]
    [InlineData("https://evil.example", "/")]
    [InlineData("/\\evil.example", "/")]
    [InlineData("/%09/evil.example", "/")]
    [InlineData("", "/")]
    [InlineData("/%C3%A9", "/%C3%A9")]
    [InlineData("/admin/users?search=Jos%C3%A9", "/admin/users?search=Jos%C3%A9")]
    public async Task TheReturnAddress_IsFollowedOnlyWhenItIsLocal(string returnUrl, string landsOn)
    {
        var zulu = await CreateUserAsync("Thandi", "Zulu", WombatRoles.CommitteeMember, WombatRoles.Assessor);
        var (browser, _) = NewBrowser();
        using (browser)
        {
            await SignInAsync(browser, zulu.Email!);

            using var switched = await browser.GetAsync(
                $"/dashboard/switch/Assessor?returnUrl={Uri.EscapeDataString(Uri.UnescapeDataString(returnUrl))}");

            switched.StatusCode.Should().Be(HttpStatusCode.Redirect);
            SignInReturnAddressFlowTests.Location(switched).Should().Be(landsOn);
            (await StoredActingRoleAsync(zulu.Id)).Should().Be(WombatRoles.Assessor, "the switch is made whatever the return address");
        }
    }

    // R2-Detail-Email: an email's link switches her and opens the page. An interactive page, as its prerender delivers it
    // (the circuit is given the same word as a Routes parameter), and a static one (the export check, excluded from
    // interactive routing), each say it once.
    [Theory]
    [InlineData("/account/profile")]
    [InlineData("/portfolio/verify")]
    public async Task TheResult_IsSaidOnThePageItLandsOn_ExactlyOnce(string page)
    {
        var zulu = await CreateUserAsync("Thandi", "Zulu", WombatRoles.CommitteeMember, WombatRoles.Assessor);
        var (browser, _) = NewBrowser();
        using (browser)
        {
            await SignInAsync(browser, zulu.Email!);

            using (var switched = await browser.GetAsync(ActingRoleSwitch.Url(WombatRoles.Assessor, page)))
            {
                switched.Headers.Location!.ToString().Should().Be(page);
            }

            var landed = await PageAsync(browser, page);
            landed.Alert.Should().Be("You are now acting as Assessor.");
            landed.Focused.Should().BeTrue("the result takes the focus as the page loads");
            landed.SwitchBack.Should().Be(
                ("Switch back to Committee member", $"/dashboard/switch/CommitteeMember?returnUrl={Uri.EscapeDataString(page)}"),
                "the same page, in the role she was acting as");

            (await PageAsync(browser, page)).Alert.Should().BeNull("said once: a reload says nothing");

            // Switch back, as the alert's link does: the same page again, in the other frame, and it says so.
            using (var back = await browser.GetAsync(landed.SwitchBack!.Value.Href))
            {
                back.Headers.Location!.ToString().Should().Be(page);
            }

            var returned = await PageAsync(browser, page);
            returned.Alert.Should().Be("You are now acting as Committee member.");
            returned.SwitchBack!.Value.Text.Should().Be("Switch back to Assessor");
            (await StoredActingRoleAsync(zulu.Id)).Should().Be(WombatRoles.CommitteeMember);
        }
    }

    // A link naming the role already shown switches nothing, so it says nothing.
    [Fact]
    public async Task ASwitchToTheRoleAlreadyShown_SaysNothing()
    {
        var zulu = await CreateUserAsync("Thandi", "Zulu", WombatRoles.CommitteeMember, WombatRoles.Assessor);
        var (browser, _) = NewBrowser();
        using (browser)
        {
            await SignInAsync(browser, zulu.Email!);

            using (var switched = await browser.GetAsync("/dashboard/switch/CommitteeMember"))
            {
                SetCookies(switched).Should().NotContain(cookie => cookie.StartsWith(ActingRoleSwitchResults.CookieName, StringComparison.Ordinal));
            }

            (await HomeAsync(browser)).Alert.Should().BeNull();
        }
    }

    [Fact]
    public async Task AForgedWord_AnotherAccountsWord_AndAReplayedWord_SayNothing()
    {
        var zulu = await CreateUserAsync("Thandi", "Zulu", WombatRoles.CommitteeMember, WombatRoles.Assessor);
        var (browser, cookies) = NewBrowser();
        using (browser)
        {
            await SignInAsync(browser, zulu.Email!);

            // Forged: a value written by hand. It is deleted, and says nothing.
            cookies.Container.Add(Origin, new Cookie(ActingRoleSwitchResults.CookieName, "CfDJ8forged-value"));
            using (var forged = await browser.GetAsync("/"))
            {
                SetCookies(forged).Should().Contain(cookie => cookie.StartsWith(ActingRoleSwitchResults.CookieName + "=;", StringComparison.Ordinal));
                Parse(await forged.Content.ReadAsStringAsync()).QuerySelector(".action-result .alert-info").Should().BeNull();
            }

            // Another account's: a genuine word, protected by the app, for someone else who switched to Committee member.
            cookies.Container.Add(Origin, new Cookie(ActingRoleSwitchResults.CookieName, WordFor("someone-else", WombatRoles.Assessor, WombatRoles.CommitteeMember)));
            (await HomeAsync(browser)).Alert.Should().BeNull("the word is bound to the account that switched");

            // Replayed: her own genuine word, shown once, then sent again.
            string word;
            using (var switched = await browser.GetAsync("/dashboard/switch/Assessor"))
            {
                var issued = SetCookies(switched).Single(cookie => cookie.StartsWith(ActingRoleSwitchResults.CookieName + "=", StringComparison.Ordinal));
                word = issued[(ActingRoleSwitchResults.CookieName.Length + 1)..issued.IndexOf(';')];
            }

            (await HomeAsync(browser)).Alert.Should().Be("You are now acting as Assessor.", "guard: the genuine word, once");
            cookies.Container.Add(Origin, new Cookie(ActingRoleSwitchResults.CookieName, word));
            (await HomeAsync(browser)).Alert.Should().BeNull("a word already taken says nothing again");
        }
    }

    // An email's link opened while signed out: sign-in comes back to the switch, which switches her and opens the page.
    [Fact]
    public async Task SignedOut_TheSwitchSendsToSignIn_AndSignInComesBackToIt()
    {
        var zulu = await CreateUserAsync("Thandi", "Zulu", WombatRoles.CommitteeMember, WombatRoles.Assessor);
        var (browser, _) = NewBrowser();
        using (browser)
        {
            var link = ActingRoleSwitch.Url(WombatRoles.Assessor, "/account/profile");

            using var challenged = await browser.GetAsync(link);

            challenged.StatusCode.Should().Be(HttpStatusCode.Redirect);
            challenged.Headers.Location!.AbsolutePathOf().Should().Be("/account/login");
            (await StoredActingRoleAsync(zulu.Id)).Should().BeNull("nothing is written for a visitor who has not signed in");

            var afterSignIn = await SignInAsync(browser, zulu.Email!, returnUrl: link);
            afterSignIn.Should().Be(link);
            using (var switched = await browser.GetAsync(afterSignIn))
            {
                switched.Headers.Location!.ToString().Should().Be("/account/profile");
            }

            (await PageAsync(browser, "/account/profile")).Alert.Should().Be("You are now acting as Assessor.");
        }
    }

    // A session another change has ended (here, the account's security stamp changed elsewhere) is signed out, not issued
    // a cookie carrying the account's current stamp, which would outlive the change (the change-password endpoint's rule).
    [Fact]
    public async Task ASessionThatHasEnded_IsSignedOut_NotRenewedByASwitch()
    {
        var zulu = await CreateUserAsync("Thandi", "Zulu", WombatRoles.CommitteeMember, WombatRoles.Assessor);
        var (browser, _) = NewBrowser();
        using (browser)
        {
            await SignInAsync(browser, zulu.Email!);
            await using (var scope = _host.Factory.Services.CreateAsyncScope())
            {
                var users = scope.ServiceProvider.GetRequiredService<UserManager<WombatIdentityUser>>();
                (await users.UpdateSecurityStampAsync((await users.FindByIdAsync(zulu.Id))!)).Succeeded.Should().BeTrue();
            }

            using var switched = await browser.GetAsync("/dashboard/switch/Assessor");

            switched.StatusCode.Should().Be(HttpStatusCode.Redirect);
            switched.Headers.Location!.ToString().Should().Be(
                "/account/login?error=SessionEnded&returnUrl=" + Uri.EscapeDataString("/dashboard/switch/Assessor"));
            SetCookies(switched).Where(cookie => cookie.StartsWith(SignInCookie + "=", StringComparison.Ordinal))
                .Should().OnlyContain(cookie => cookie.StartsWith(SignInCookie + "=;", StringComparison.Ordinal), "only deleted");
            (await StoredActingRoleAsync(zulu.Id)).Should().BeNull();
        }
    }

    /// <summary>A word the app itself protects, for <paramref name="userId" />: genuine, but not this browser's user's.</summary>
    private string WordFor(string userId, string from, string to)
    {
        var context = new DefaultHttpContext();
        _host.Factory.Services.GetRequiredService<ActingRoleSwitchResults>().Issue(context, userId, from, to);
        var header = context.Response.Headers.SetCookie.ToString();
        return header[(ActingRoleSwitchResults.CookieName.Length + 1)..header.IndexOf(';')];
    }

    /// <summary>A user at the host's institution who holds <paramref name="roles" /> and signs in with a password.</summary>
    private async Task<WombatIdentityUser> CreateUserAsync(string firstName, string lastName, params string[] roles)
    {
        var email = $"{firstName.ToLowerInvariant()}.{lastName.ToLowerInvariant()}-{Guid.NewGuid():N}@example.test";
        var user = await _host.CreateUserAsync(email, roles[0]);

        await using var scope = _host.Factory.Services.CreateAsyncScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<WombatIdentityUser>>();
        var stored = (await users.FindByIdAsync(user.Id))!;
        stored.FirstName = firstName;
        stored.LastName = lastName;
        (await users.UpdateAsync(stored)).Succeeded.Should().BeTrue();
        (await users.AddToRolesAsync(stored, roles.Skip(1))).Succeeded.Should().BeTrue();
        return stored;
    }

    private async Task<string?> StoredActingRoleAsync(string userId)
    {
        await using var scope = _host.Factory.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Users.AsNoTracking()
            .Where(user => user.Id == userId)
            .Select(user => user.ActingRole)
            .SingleAsync();
    }

    private static async Task<Landing> HomeAsync(HttpClient browser) => await PageAsync(browser, "/");

    /// <summary>A page as a full load delivers it, and what its result alert says.</summary>
    private static async Task<Landing> PageAsync(HttpClient browser, string path)
    {
        using var load = await browser.GetAsync(path);
        load.StatusCode.Should().Be(HttpStatusCode.OK, $"guard: {path} loads signed in ({load.Headers.Location})");
        var page = Parse(await load.Content.ReadAsStringAsync());

        var region = page.QuerySelector(".header-container + .action-result");
        var alert = region?.QuerySelector(".alert-info[role=status]");
        var back = alert?.QuerySelector("a");
        // Home's subtitle is "{role} · Semester N, YYYY" (T335, flow 01; HomeFrame): the role is what a switch changes.
        return new Landing(
            page.QuerySelector(".page-subtitle")?.TextContent.Trim().Split(" · ")[0],
            alert?.QuerySelector("span")?.TextContent.Trim(),
            region?.HasAttribute("autofocus") ?? false,
            back is null ? null : (back.TextContent.Trim(), back.GetAttribute("href")!));
    }

    /// <summary>Signs a browser in through the sign-in page's own form, and returns where it was sent.</summary>
    private static async Task<string> SignInAsync(HttpClient browser, string email, string? returnUrl = null)
    {
        using var loginPage = await browser.GetAsync(
            returnUrl is null ? "/account/login" : $"/account/login?ReturnUrl={Uri.EscapeDataString(returnUrl)}");
        var form = Parse(await loginPage.Content.ReadAsStringAsync()).QuerySelector("form[action='/account/login/submit']")!;

        using var signIn = await browser.PostAsync("/account/login/submit", new FormUrlEncodedContent(
        [
            new("__RequestVerificationToken", form.QuerySelector("input[name=__RequestVerificationToken]")!.GetAttribute("value")!),
            new("Email", email),
            new("Password", Password),
            new("ReturnUrl", form.QuerySelector("input[name=ReturnUrl]")?.GetAttribute("value") ?? string.Empty)
        ]));

        signIn.StatusCode.Should().Be(HttpStatusCode.Redirect);
        var location = signIn.Headers.Location!.ToString();
        location.Should().NotContain("/account/login", "guard: the sign-in succeeded");
        return location;
    }

    /// <summary>Signs out through the shell's own Sign out form.</summary>
    private static async Task SignOutAsync(HttpClient browser)
    {
        using var page = await browser.GetAsync("/account/profile");
        var form = Parse(await page.Content.ReadAsStringAsync()).QuerySelector("form[action='/account/logout']")!;

        using var signOut = await browser.PostAsync("/account/logout", new FormUrlEncodedContent(
        [
            new("__RequestVerificationToken", form.QuerySelector("input[name=__RequestVerificationToken]")!.GetAttribute("value")!)
        ]));

        signOut.StatusCode.Should().Be(HttpStatusCode.Redirect);
        signOut.Headers.Location!.ToString().Should().Be("/account/login", "guard: signed out");
    }

    private static IReadOnlyList<string> SetCookies(HttpResponseMessage response)
        => response.Headers.TryGetValues("Set-Cookie", out var cookies) ? cookies.ToList() : [];

    /// <summary>
    /// A browser of its own: its own cookies, which the test can read and add to, and its own address, so the sign-in
    /// throttle it meets is its own. Redirects are not followed.
    /// </summary>
    private (HttpClient Browser, CookieContainerHandler Cookies) NewBrowser()
    {
        var cookies = new CookieContainerHandler();
        var browser = _host.Factory.CreateDefaultClient(Origin, cookies);
        browser.DefaultRequestHeaders.Add("X-Forwarded-For", $"10.236.{Interlocked.Increment(ref _nextSubnet)}.7");
        return (browser, cookies);
    }

    private static IHtmlDocument Parse(string html) => new HtmlParser().ParseDocument(html);

    /// <summary>What a page shows of the acting role: the role Home's subtitle names, and the switch's result alert.</summary>
    private sealed record Landing(string? ActingAs, string? Alert, bool Focused, (string Text, string Href)? SwitchBack);
}
