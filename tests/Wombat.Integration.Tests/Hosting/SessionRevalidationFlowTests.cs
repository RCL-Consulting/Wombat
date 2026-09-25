extern alias WombatWeb;

using System.Net;
using System.Text.RegularExpressions;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Mvc.Testing.Handlers;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.DataRights;
using Wombat.Domain.DataRights;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Identity;
using Wombat.Infrastructure.Persistence;
using Wombat.Integration.Tests.MultiSourceFeedback;
using WombatWeb::Wombat.Web.Security;

namespace Wombat.Integration.Tests.Hosting;

/// <summary>
/// T279 end to end: a sign-in cookie is checked against its account within a minute of being issued, so a lock, a lost
/// role, a move to another institution or an erasure signs an existing session out within that minute. Until T279 no interval was
/// set, and Identity's thirty minutes applied: an erased trainee's open session loaded their pages for about that long.
/// </summary>
/// <remarks>
/// <para>
/// The whole of <c>Wombat.Web</c>'s <c>Program.cs</c> on a PostgreSQL schema of its own, hosted by the MSF respondent
/// page's fixture (<see cref="MsfRespondPageFlowTests.WebHost" />), with the app's own interval: only the stamp
/// validator's clock is the test's, so a minute passes without waiting for one. The cookie is issued on the real clock,
/// and the validator measures from that.
/// </para>
/// <para>
/// A circuit is checked on the same interval by <see cref="SessionRevalidatingAuthenticationStateProvider" />, which
/// waits on the real clock; its loop and what it asks are held by <c>SessionRevalidationTests</c> in the web suite. Here
/// the app is shown to register it, and a tab whose circuit's sign-in ended is shown the way back in through
/// <see cref="SessionEnd" /> (the T279 review).
/// </para>
/// </remarks>
public sealed partial class SessionRevalidationFlowTests : IClassFixture<MsfRespondPageFlowTests.WebHost>
{
    private const string SignedInPage = "/account/change-password";

    /// <summary>The page a tab was on when its circuit's sign-in ended: where signing in again comes back to.</summary>
    private const string TabPage = "/committee/reviews?panel=1";

    private static readonly Uri Origin = new("http://localhost");

    private static int _nextSubnet;

    private readonly MsfRespondPageFlowTests.WebHost _host;

    public SessionRevalidationFlowTests(MsfRespondPageFlowTests.WebHost host)
    {
        _host = host;
    }

    [Fact]
    public async Task TheApp_ChecksACookieAndACircuit_WithinAMinute()
    {
        await using var scope = _host.Factory.Services.CreateAsyncScope();

        scope.ServiceProvider.GetRequiredService<IOptions<SecurityStampValidatorOptions>>().Value.ValidationInterval
            .Should().Be(TimeSpan.FromMinutes(1)).And.Be(SessionRevalidation.Interval);
        scope.ServiceProvider.GetRequiredService<AuthenticationStateProvider>()
            .Should().BeOfType<SessionRevalidatingAuthenticationStateProvider>("a circuit's sign-in is checked too");
    }

    public static TheoryData<string> ChangesThatEndASession => new()
    {
        "an administrator locks the account",
        "an administrator removes its role",
        "it moves to another institution",
        "it is erased"
    };

    [Theory]
    [MemberData(nameof(ChangesThatEndASession))]
    public async Task ASessionAlreadySignedIn_IsSignedOutWithinAMinuteOfAChangeToItsAccount(string change)
    {
        var clock = new MovableClock();
        await using var app = _host.Factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
            services.Configure<SecurityStampValidatorOptions>(options => options.TimeProvider = clock)));

        var email = NewEmail();
        var user = await _host.CreateAssessorAsync(email);
        var (browser, _) = NewBrowser(app);
        using (browser)
        {
            await _host.SignInAsync(browser, email);
            (await SignedInAsync(browser)).Should().BeTrue("guard: signed in");

            await ChangeAsync(app, user.Id, change);

            // Half a minute on, the cookie has not been checked since it was issued, so it still signs the browser in: the
            // interval is what ends the session, not the change itself.
            clock.MoveForward(TimeSpan.FromSeconds(30));
            (await SignedInAsync(browser)).Should().BeTrue($"{change}: the cookie is checked a minute after it was issued");

            // Past the minute the cookie is checked, its stamp is no longer the account's, and the browser is sent to sign in.
            clock.MoveForward(TimeSpan.FromSeconds(35));
            (await SignedInAsync(browser)).Should().BeFalse($"{change}: the session ends within a minute");
        }
    }

    [Fact]
    public async Task TheControl_AnAccountNobodyChanged_StaysSignedIn_PastTheCheck()
    {
        // The same browser and clock, and nothing changed: the minute's check passes, and the cookie is issued again.
        var clock = new MovableClock();
        await using var app = _host.Factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
            services.Configure<SecurityStampValidatorOptions>(options => options.TimeProvider = clock)));

        var email = NewEmail();
        await _host.CreateAssessorAsync(email);
        var (browser, _) = NewBrowser(app);
        using (browser)
        {
            await _host.SignInAsync(browser, email);

            clock.MoveForward(TimeSpan.FromSeconds(65));
            (await SignedInAsync(browser)).Should().BeTrue("the account's stamp is unchanged");

            clock.MoveForward(TimeSpan.FromMinutes(10));
            (await SignedInAsync(browser)).Should().BeTrue("still unchanged, however often it is checked");
        }
    }

    // ---- a tab whose circuit's sign-in ended signs in again (the T279 review) -----------------------------------------
    //
    // A circuit found its sign-in ended and became anonymous, and the router sent the tab to the sign-in page inside the
    // circuit. That page's form carried the antiforgery token the circuit was given on its first page, naming the old user,
    // and the browser still held the old user's cookie. So the post was refused with a bare 400, and nobody the check
    // signed out could sign back in without reloading. The tab now leaves the circuit by a full load of SessionEnd. The
    // first test is the failure itself, the reason the tab leaves; the rest are the way back in.

    [Fact]
    public async Task TheSignInFormACircuitWasGiven_IsRefused_OnceTheCookieIs()
    {
        var clock = new MovableClock();
        await using var app = AppOn(clock);

        var email = NewEmail();
        var user = await _host.CreateAssessorAsync(email);
        var (browser, _) = NewBrowser(app);
        using (browser)
        {
            await _host.SignInAsync(browser, email);

            // A token minted for the signed-in user, as the one a circuit is given on its first page.
            using var circuitsFirstPage = await browser.GetAsync(SignedInPage);
            var circuitsToken = TokenOf(Parse(await circuitsFirstPage.Content.ReadAsStringAsync()));

            await ChangeAsync(app, user.Id, "an administrator removes its role");
            clock.MoveForward(TimeSpan.FromSeconds(65));

            using var post = await PostSignInAsync(browser, circuitsToken, email, returnUrl: TabPage);
            post.StatusCode.Should().Be(
                HttpStatusCode.BadRequest,
                "the cookie is refused, so the post is anonymous, and the token names the user it no longer signs in");
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ATabWhoseSessionEnded_LeavesThroughSessionEnded_AndSignsInAgain_BackWhereItWas(bool cookieInsideItsMinute)
    {
        var clock = new MovableClock();
        await using var app = AppOn(clock);

        var email = NewEmail();
        var user = await _host.CreateAssessorAsync(email);
        var (browser, _) = NewBrowser(app);
        using (browser)
        {
            await _host.SignInAsync(browser, email);
            await ChangeAsync(app, user.Id, "an administrator removes its role");

            if (cookieInsideItsMinute)
            {
                // Another tab renewed the cookie less than a minute ago, so the cookie handler does not check it yet: a plain
                // load of the sign-in page would still be signed in as the old user, interactive, its form's token naming
                // them again. Only the endpoint's own check stands between the tab and the same 400.
                using var plainLoad = await browser.GetAsync(SignInOutcome.PagePath);
                ServerComponentMarker().IsMatch(await plainLoad.Content.ReadAsStringAsync()).Should().BeTrue(
                    "guard: the cookie still signs the browser in");
            }
            else
            {
                clock.MoveForward(TimeSpan.FromSeconds(65));
            }

            using var leave = await browser.GetAsync(SessionEnd.Url(TabPage));
            leave.StatusCode.Should().Be(HttpStatusCode.Redirect);
            leave.Headers.Location!.AbsolutePathOf().Should().Be(SignInOutcome.PagePath);
            QueryValue(leave.Headers.Location!, "error").Should().Be(SignInOutcome.SessionEnded);
            QueryValue(leave.Headers.Location!, "returnUrl").Should().Be(TabPage);

            // The sign-in page that follows loads signed out: static, with a form and a token of its own, saying why.
            using var page = await browser.GetAsync(leave.Headers.Location);
            page.StatusCode.Should().Be(HttpStatusCode.OK);
            var html = await page.Content.ReadAsStringAsync();
            ServerComponentMarker().IsMatch(html).Should().BeFalse("the browser's session ended with the circuit's");
            var document = Parse(html);
            document.QuerySelector(".alert-danger")!.TextContent.Trim().Should().Be(ChangePasswordOutcome.SessionEndedMessage);
            document.QuerySelector("input[name=ReturnUrl]")!.GetAttribute("value").Should().Be(TabPage);

            using var signIn = await PostSignInAsync(
                browser, TokenOf(document), email, returnUrl: document.QuerySelector("input[name=ReturnUrl]")!.GetAttribute("value"));
            signIn.StatusCode.Should().Be(HttpStatusCode.Redirect, "signed in again, not a 400");
            signIn.Headers.Location!.ToString().Should().Be(TabPage, "back to the page the tab was on");
            (await SignedInAsync(browser)).Should().BeTrue();
        }
    }

    [Fact]
    public async Task ALockedAccountsTab_ComesToASignInPage_ThatRefusesItInItsOwnWords()
    {
        // Before the review a locked user got the bare 400 at this point, never the words that say why.
        var clock = new MovableClock();
        await using var app = AppOn(clock);

        var email = NewEmail();
        var user = await _host.CreateAssessorAsync(email);
        var (browser, _) = NewBrowser(app);
        using (browser)
        {
            await _host.SignInAsync(browser, email);
            await ChangeAsync(app, user.Id, "an administrator locks the account");
            clock.MoveForward(TimeSpan.FromSeconds(65));

            using var leave = await browser.GetAsync(SessionEnd.Url(TabPage));
            using var page = await browser.GetAsync(leave.Headers.Location);
            var document = Parse(await page.Content.ReadAsStringAsync());

            using var signIn = await PostSignInAsync(browser, TokenOf(document), email, returnUrl: TabPage);
            signIn.StatusCode.Should().Be(HttpStatusCode.Redirect);
            QueryValue(signIn.Headers.Location!, "error").Should().Be(SignInOutcome.LockedOut);

            using var refusal = await browser.GetAsync(signIn.Headers.Location);
            Parse(await refusal.Content.ReadAsStringAsync()).QuerySelector(".alert-danger")!.TextContent.Trim()
                .Should().Be(SignInOutcome.LockedOutMessage);
        }
    }

    [Fact]
    public async Task ASessionTheAccountStillAccepts_GoesBackToItsPage_StillSignedIn()
    {
        // A GET of the endpoint never signs a good session out: the user signed in again in another tab, or the circuit
        // ended on faults alone. Past the minute, so the cookie is checked on the way in, and passes.
        var clock = new MovableClock();
        await using var app = AppOn(clock);

        var email = NewEmail();
        await _host.CreateAssessorAsync(email);
        var (browser, _) = NewBrowser(app);
        using (browser)
        {
            await _host.SignInAsync(browser, email);
            clock.MoveForward(TimeSpan.FromSeconds(65));

            using var leave = await browser.GetAsync(SessionEnd.Url(TabPage));

            leave.StatusCode.Should().Be(HttpStatusCode.Redirect);
            leave.Headers.Location!.ToString().Should().Be(TabPage);
            (await SignedInAsync(browser)).Should().BeTrue();
        }
    }

    [Fact]
    public async Task TheEndpoint_FollowsNoAddressOffThisSite()
    {
        var clock = new MovableClock();
        await using var app = AppOn(clock);

        var email = NewEmail();
        await _host.CreateAssessorAsync(email);
        var (browser, _) = NewBrowser(app);
        using (browser)
        {
            await _host.SignInAsync(browser, email);

            using var signedIn = await browser.GetAsync($"{SessionEnd.Path}?returnUrl={Uri.EscapeDataString("//evil.example/x")}");
            signedIn.Headers.Location!.ToString().Should().Be("/");

            var (stranger, _) = NewBrowser(app);
            using (stranger)
            {
                using var signedOut = await stranger.GetAsync(
                    $"{SessionEnd.Path}?returnUrl={Uri.EscapeDataString("https://evil.example/")}");
                signedOut.Headers.Location!.ToString().Should().Be(SignInOutcome.Url(SignInOutcome.SessionEnded));
            }
        }
    }

    private WebApplicationFactory<WombatWeb::Program> AppOn(MovableClock clock)
        => _host.Factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
            services.Configure<SecurityStampValidatorOptions>(options => options.TimeProvider = clock)));

    /// <summary>Posts the sign-in form as the browser does, with <paramref name="token" /> as its antiforgery token.</summary>
    private static Task<HttpResponseMessage> PostSignInAsync(HttpClient browser, string token, string email, string? returnUrl)
        => browser.PostAsync("/account/login/submit", new FormUrlEncodedContent(
        [
            new("__RequestVerificationToken", token),
            new("Email", email),
            new("Password", MsfRespondPageFlowTests.WebHost.SignInPassword),
            new("ReturnUrl", returnUrl ?? string.Empty)
        ]));

    private static string TokenOf(IHtmlDocument page)
        => page.QuerySelector("input[name=__RequestVerificationToken]")!.GetAttribute("value")!;

    private static string? QueryValue(Uri address, string name)
    {
        var query = (address.IsAbsoluteUri ? address : new Uri(Origin, address)).Query;
        return query.TrimStart('?').Split('&')
            .Select(pair => pair.Split('=', 2))
            .Where(pair => string.Equals(pair[0], name, StringComparison.OrdinalIgnoreCase))
            .Select(pair => Uri.UnescapeDataString(pair.Length > 1 ? pair[1] : string.Empty))
            .SingleOrDefault();
    }

    private static IHtmlDocument Parse(string html) => new HtmlParser().ParseDocument(html);

    /// <summary>An interactive server component's marker: a page that opens a circuit carries one.</summary>
    [GeneratedRegex("""<!--Blazor:\{[^>]*"type":"server""")]
    private static partial Regex ServerComponentMarker();

    private static async Task ChangeAsync(WebApplicationFactory<WombatWeb::Program> app, string userId, string change)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var users = scope.ServiceProvider.GetRequiredService<IUserAdministrationService>();

        switch (change)
        {
            case "an administrator locks the account":
                await users.SetLockoutAsync(userId, locked: true);
                break;
            case "an administrator removes its role":
                await users.RemoveRoleAsync(userId, WombatRoles.Assessor);
                break;
            case "it moves to another institution":
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var elsewhere = new Institution
                {
                    Name = $"Elsewhere {Guid.NewGuid():N}",
                    ShortCode = $"E{Guid.NewGuid():N}"[..8],
                    IsActive = true,
                    CreatedOn = DateTime.UtcNow
                };
                db.Institutions.Add(elsewhere);
                await db.SaveChangesAsync();
                await users.UpdateScopeAsync(userId, elsewhere.Id, [], []);
                break;
            }
            case "it is erased":
            {
                // The erasure itself, as an approved data-rights request runs it: the account is pseudonymised, its roles and
                // password go, and its security stamp changes (ErasureExecutor). Without the new stamp the check would pass,
                // build a principal with no roles, and keep the browser signed in (the T279 review).
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var request = DataRightsRequest.Create(
                    userId, "Signed Assessor", DataRightsRequestType.Erasure, "Leaving the programme.", DateTime.UtcNow);
                db.Set<DataRightsRequest>().Add(request);
                await db.SaveChangesAsync();
                await scope.ServiceProvider.GetRequiredService<IErasureExecutor>()
                    .ExecuteAsync(request, "salt-for-tests", CancellationToken.None);
                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(change), change, null);
        }
    }

    /// <summary>Whether the browser's cookie, as it holds it now, loads a signed-in page.</summary>
    private static async Task<bool> SignedInAsync(HttpClient browser)
    {
        using var load = await browser.GetAsync(SignedInPage);
        if (load.StatusCode == HttpStatusCode.OK)
        {
            return true;
        }

        load.StatusCode.Should().Be(HttpStatusCode.Redirect);
        load.Headers.Location!.AbsolutePathOf().Should().Be("/account/login", "guard: refused by being sent to sign in");
        return false;
    }

    /// <summary>
    /// A browser of its own: its own cookies, and its own address, so the sign-in throttle it meets is its own. Redirects
    /// are not followed.
    /// </summary>
    private static (HttpClient Browser, CookieContainerHandler Cookies) NewBrowser<TEntryPoint>(WebApplicationFactory<TEntryPoint> factory)
        where TEntryPoint : class
    {
        var cookies = new CookieContainerHandler();
        var browser = factory.CreateDefaultClient(Origin, cookies);
        browser.DefaultRequestHeaders.Add("X-Forwarded-For", $"10.236.{Interlocked.Increment(ref _nextSubnet)}.7");
        return (browser, cookies);
    }

    private static string NewEmail() => $"session-{Guid.NewGuid():N}@example.test";

    /// <summary>The system clock, moved forward by as much as a test says.</summary>
    private sealed class MovableClock : TimeProvider
    {
        private TimeSpan _ahead;

        public void MoveForward(TimeSpan by) => _ahead += by;

        public override DateTimeOffset GetUtcNow() => base.GetUtcNow() + _ahead;
    }
}
