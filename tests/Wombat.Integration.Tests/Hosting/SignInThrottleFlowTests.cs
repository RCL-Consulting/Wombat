extern alias WombatWeb;

using System.Net;
using System.Security.Claims;
using System.Text;
using AngleSharp.Html.Parser;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Infrastructure.Identity;
using Wombat.Infrastructure.Persistence;
using Wombat.Integration.Tests.MultiSourceFeedback;
using WombatWeb::Wombat.Web.Security;

namespace Wombat.Integration.Tests.Hosting;

/// <summary>
/// T156 end to end, through the sign-in page's own form and the whole of <c>Wombat.Web</c>'s <c>Program.cs</c> on a
/// PostgreSQL schema of its own: the sign-in throttle counts failures from one address, and nothing else; a refused
/// sign-in says the same whatever the account; and <c>X-Forwarded-For</c> is believed from the loopback proxy only.
/// </summary>
/// <remarks>
/// A browser is a client with its own cookies whose requests come from an address of its own, as Caddy forwards it
/// (<see cref="MsfRespondPageFlowTests.WebHost.NewBrowser" />): the test server's own connections have no address, which
/// the forwarded-headers middleware lets a proxy's header fill.
/// </remarks>
public sealed class SignInThrottleFlowTests : IClassFixture<MsfRespondPageFlowTests.WebHost>
{
    private const string WrongPassword = "Not-the-Pa55word!";
    private const string LinkProvider = "kgk";

    private static int _nextSubnet;

    private readonly MsfRespondPageFlowTests.WebHost _host;

    public SignInThrottleFlowTests(MsfRespondPageFlowTests.WebHost host)
    {
        _host = host;
    }

    private static string Refused(string message) => "/account/login?error=" + Uri.EscapeDataString(message);

    [Fact]
    public async Task TenFailuresFromOneAddress_RefuseTheEleventh_EvenWithTheRightPassword_ButNotANeighbourOnTheSameSlash24()
    {
        var email = NewEmail();
        await _host.CreateAssessorAsync(email);
        var subnet = NextSubnet();

        using (var guesser = _host.NewBrowser($"10.237.{subnet}.7"))
        {
            // A spray: one guess at each of ten addresses, so no account's own lockout is what stops it.
            for (var guess = 1; guess <= 10; guess++)
            {
                using var refused = await SignInAsync(guesser, NewEmail(), $"Guess-{guess}-Pa55word!");
                refused.Headers.Location!.ToString().Should().Be(Refused(SignInMessages.InvalidCredentials),
                    $"guess {guess} of ten is checked");
            }

            using var throttled = await SignInAsync(guesser, email, MsfRespondPageFlowTests.WebHost.SignInPassword);
            throttled.Headers.Location!.ToString().Should().Be(Refused(SignInMessages.TooManyFailedAttempts),
                "the eleventh is refused before any password is checked");
            int.Parse(throttled.Headers.RetryAfter!.ToString()).Should().BeInRange(1, 300);
        }

        // Until T156 the throttle counted the /24, so the neighbour behind the same NAT block was refused too.
        using var neighbour = _host.NewBrowser($"10.237.{subnet}.8");
        using var signedIn = await SignInAsync(neighbour, email, MsfRespondPageFlowTests.WebHost.SignInPassword);
        signedIn.Headers.Location!.ToString().Should().Be("/", "a neighbour's failures are not theirs");
    }

    [Fact]
    public async Task SignInsThatSucceed_CostNothing_SoTenFailuresAreStillTen()
    {
        var email = NewEmail();
        await _host.CreateAssessorAsync(email);
        var address = $"10.237.{NextSubnet()}.7";

        // A morning's sign-ins behind one hospital's address: until T156 the eleventh was refused.
        for (var signIn = 1; signIn <= 12; signIn++)
        {
            using var browser = _host.NewBrowser(address);
            using var signedIn = await SignInAsync(browser, email, MsfRespondPageFlowTests.WebHost.SignInPassword);
            signedIn.Headers.Location!.ToString().Should().Be("/", $"sign-in {signIn} succeeds");
        }

        using var guesser = _host.NewBrowser(address);
        for (var guess = 1; guess <= 10; guess++)
        {
            using var refused = await SignInAsync(guesser, NewEmail(), WrongPassword);
            refused.Headers.Location!.ToString().Should().Be(Refused(SignInMessages.InvalidCredentials),
                $"failure {guess} of ten is checked");
        }

        using var throttled = await SignInAsync(guesser, NewEmail(), WrongPassword);
        throttled.Headers.Location!.ToString().Should().Be(Refused(SignInMessages.TooManyFailedAttempts));
    }

    /// <summary>
    /// Until T156 an account that signs in only through its institution was told "This account uses institutional sign-in",
    /// and an address no account has was told "Invalid email or password.", so the page said which addresses had accounts.
    /// </summary>
    [Fact]
    public async Task AnAddressNoAccountHas_AndAnAccountThatSignsInThroughItsInstitution_AreRefusedAlike()
    {
        var institutional = NewEmail();
        await _host.CreateAssessorAsync(institutional);
        await SignsInThroughItsInstitutionAsync(institutional);
        var unknown = NewEmail();
        var userAgent = $"EnumerationProbe/{Guid.NewGuid():N}";

        using var browser = _host.NewBrowser($"10.237.{NextSubnet()}.7");
        browser.DefaultRequestHeaders.UserAgent.Clear();
        browser.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);

        using var forUnknown = await SignInAsync(browser, unknown, WrongPassword);
        using var forInstitutional = await SignInAsync(browser, institutional, WrongPassword);

        forInstitutional.Headers.Location.Should().Be(forUnknown.Headers.Location);
        forUnknown.Headers.Location!.ToString().Should().Be(Refused(SignInMessages.InvalidCredentials));

        await using var scope = _host.Factory.Services.CreateAsyncScope();
        var rows = await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().AuditEntries.AsNoTracking()
            .Where(entry => entry.ActorUserAgent == userAgent)
            .OrderBy(entry => entry.Id)
            .ToListAsync();
        rows.Should().HaveCount(2, "each refusal is audited, the institutional account's too");
        rows.Should().OnlyContain(row => row.Action == "LoginFailed"
                                         && !row.Success
                                         && row.ActorUserId == null
                                         && row.InstitutionId == null
                                         && row.ErrorMessage == SignInMessages.InvalidCredentials,
            "the rows are alike: neither names an account or an institution");

        var users = scope.ServiceProvider.GetRequiredService<UserManager<WombatIdentityUser>>();
        (await users.FindByEmailAsync(institutional))!.AccessFailedCount.Should().Be(0,
            "no password is checked for an account that has none, so nothing counts towards a lockout of its SSO sign-in");
    }

    /// <summary>
    /// Where the page offers institutional sign-in, every refusal points at its button: the same words for a wrong
    /// password, an unknown address and an institutional account, so the hint says nothing about any of them.
    /// </summary>
    [Fact]
    public async Task WithInstitutionalSignInOffered_EveryRefusalPointsAtItsButton_InTheSameWords()
    {
        await using var app = _host.Factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("Sso:Providers:0:Key", "kgk");
            builder.UseSetting("Sso:Providers:0:DisplayName", "KGK");
            builder.UseSetting("Sso:Providers:0:InstitutionId", "1");
            builder.UseSetting("Sso:Providers:0:Authority", "https://idp.kgk.test");
            builder.UseSetting("Sso:Providers:0:ClientId", "wombat");
            builder.UseSetting("Sso:Providers:0:ClientSecret", "not-a-secret");
        });

        var local = NewEmail();
        await _host.CreateAssessorAsync(local);
        var institutional = NewEmail();
        await _host.CreateAssessorAsync(institutional);
        await SignsInThroughItsInstitutionAsync(institutional);

        using var browser = app.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        browser.DefaultRequestHeaders.Add("X-Forwarded-For", $"10.237.{NextSubnet()}.7");

        var locations = new List<string>();
        foreach (var email in new[] { local, NewEmail(), institutional })
        {
            using var refused = await SignInAsync(browser, email, WrongPassword);
            locations.Add(refused.Headers.Location!.ToString());
        }

        locations.Should().AllBe(Refused(SignInMessages.InvalidCredentialsOrInstitutional));
    }

    /// <summary>
    /// The link-your-account page checks a password too, and shares the throttle (T149, T156). Its refusal ends the
    /// institutional sign-in and goes to the sign-in page: the external cookie ends five minutes after the callback, and
    /// the client's window can outlast it, so waiting on the link page would only lead to a sign-in that had expired
    /// (T156 review). Over HTTPS, as behind Caddy: the external cookie is a <c>__Host-</c> cookie, Secure always, so this
    /// is also the check that it is issued so, and read back by the page and its submit.
    /// </summary>
    [Fact]
    public async Task TheLinkPage_SharesTheThrottle_AndItsRefusalEndsTheInstitutionalSignIn()
    {
        var email = NewEmail();
        await _host.CreateAssessorAsync(email);
        int institutionId;
        await using (var scope = _host.Factory.Services.CreateAsyncScope())
        {
            institutionId = (await scope.ServiceProvider.GetRequiredService<UserManager<WombatIdentityUser>>().FindByEmailAsync(email))!
                .InstitutionId!.Value;
        }

        await using var app = _host.Factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("Sso:Providers:0:Key", LinkProvider);
            builder.UseSetting("Sso:Providers:0:DisplayName", "KGK");
            builder.UseSetting("Sso:Providers:0:InstitutionId", institutionId.ToString(System.Globalization.CultureInfo.InvariantCulture));
            builder.UseSetting("Sso:Providers:0:Authority", "https://idp.kgk.test");
            builder.UseSetting("Sso:Providers:0:ClientId", "wombat");
            builder.UseSetting("Sso:Providers:0:ClientSecret", "not-a-secret");
            builder.ConfigureTestServices(services => services.AddSingleton<IStartupFilter>(new ProviderCallback(email)));
        });

        using var browser = app.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });
        browser.DefaultRequestHeaders.Add("X-Forwarded-For", $"10.237.{NextSubnet()}.7");

        using (var callback = await browser.GetAsync(ProviderCallback.Path))
        {
            callback.Headers.GetValues("Set-Cookie").Should().ContainSingle(cookie => cookie.StartsWith("__Host-Identity.External=", StringComparison.Ordinal))
                .Which.ToLowerInvariant().Should().Contain("; secure").And.Contain("; path=/").And.NotContain("domain=");
        }

        var locations = new List<string>();
        var externalCookieEnded = new List<bool>();
        for (var guess = 1; guess <= 11; guess++)
        {
            using var page = await browser.GetAsync("/account/link-external");
            page.StatusCode.Should().Be(HttpStatusCode.OK);
            var form = new HtmlParser().ParseDocument(await page.Content.ReadAsStringAsync())
                .QuerySelector("form[action='/account/link-external/submit']");
            form.Should().NotBeNull($"guess {guess}: the institutional sign-in is still held, so the page offers the link");

            using var answer = await browser.PostAsync("/account/link-external/submit", new FormUrlEncodedContent(
            [
                new("__RequestVerificationToken", form!.QuerySelector("input[name=__RequestVerificationToken]")!.GetAttribute("value")!),
                new("Password", $"Guess-{guess}-Pa55word!")
            ]));
            answer.StatusCode.Should().Be(HttpStatusCode.Redirect);
            locations.Add(answer.Headers.Location!.ToString());
            externalCookieEnded.Add(answer.Headers.TryGetValues("Set-Cookie", out var cookies)
                && cookies.Any(cookie => cookie.StartsWith("__Host-Identity.External=;", StringComparison.Ordinal)));
        }

        var throttled = Uri.EscapeDataString(SignInMessages.TooManyFailedAttempts);
        locations.Take(10).Should().NotContain(location => location.Contains(throttled), "guard: ten are checked");
        externalCookieEnded.Take(10).Should().AllBeEquivalentTo(false, "guard: a refused password keeps the sign-in in progress");
        locations[10].Should().Be(Refused(SignInMessages.TooManyFailedAttempts), "the sign-in page says to wait and try again");
        externalCookieEnded[10].Should().BeTrue("the institutional sign-in in progress is ended with the refusal");

        using var after = await browser.GetAsync("/account/link-external");
        new HtmlParser().ParseDocument(await after.Content.ReadAsStringAsync())
            .QuerySelector("form[action='/account/link-external/submit']")
            .Should().BeNull("the link page has no sign-in in progress to link");
    }

    /// <summary>
    /// Caddy reaches Kestrel over loopback and names the client in <c>X-Forwarded-For</c>. Until T156 the header was
    /// believed from any peer; now from the loopback proxy only, and any other peer is taken as the client it is. The
    /// failed sign-in's audit row records the address the app believed (its /24).
    /// </summary>
    [Theory]
    [InlineData("127.0.0.1", "198.51.100.0/24")]
    [InlineData("::1", "198.51.100.0/24")]
    [InlineData("::ffff:127.0.0.1", "198.51.100.0/24")]
    [InlineData("203.0.113.9", "203.0.113.0/24")]
    [InlineData("10.0.0.5", "10.0.0.0/24")]
    public async Task XForwardedFor_IsBelievedFromTheLoopbackProxyOnly(string peer, string recorded)
    {
        var userAgent = $"ProxyProbe/{Guid.NewGuid():N}";

        // The form's antiforgery token and cookie, as a browser holds them after loading the page.
        using var browser = _host.NewBrowser($"10.237.{NextSubnet()}.7");
        using var page = await browser.GetAsync("/account/login");
        var token = new HtmlParser().ParseDocument(await page.Content.ReadAsStringAsync())
            .QuerySelector("form[action='/account/login/submit'] input[name=__RequestVerificationToken]")!
            .GetAttribute("value")!;
        var antiforgeryCookie = string.Join("; ", page.Headers.GetValues("Set-Cookie").Select(cookie => cookie.Split(';')[0]));

        var body = Encoding.UTF8.GetBytes(
            $"__RequestVerificationToken={Uri.EscapeDataString(token)}&Email={Uri.EscapeDataString(NewEmail())}" +
            $"&Password={Uri.EscapeDataString(WrongPassword)}");
        var answered = await _host.Factory.Server.SendAsync(context =>
        {
            context.Request.Method = HttpMethods.Post;
            context.Request.Path = "/account/login/submit";
            context.Connection.RemoteIpAddress = IPAddress.Parse(peer);
            context.Request.Headers["X-Forwarded-For"] = "198.51.100.7";
            context.Request.Headers.UserAgent = userAgent;
            context.Request.Headers.Cookie = antiforgeryCookie;
            context.Request.ContentType = "application/x-www-form-urlencoded";
            context.Request.ContentLength = body.Length;
            context.Request.Body = new MemoryStream(body);
        });

        answered.Response.StatusCode.Should().Be(StatusCodes.Status302Found);
        answered.Response.Headers.Location.ToString().Should().Be(Refused(SignInMessages.InvalidCredentials));

        await using var scope = _host.Factory.Services.CreateAsyncScope();
        var row = await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().AuditEntries.AsNoTracking()
            .SingleAsync(entry => entry.ActorUserAgent == userAgent);
        row.Action.Should().Be("LoginFailed");
        row.ActorIpAddress.Should().Be(recorded);
    }

    /// <summary>Loads the sign-in page and posts its form, as the browser does; the answer is not followed.</summary>
    private static async Task<HttpResponseMessage> SignInAsync(HttpClient browser, string email, string password)
    {
        using var loginPage = await browser.GetAsync("/account/login");
        var form = new HtmlParser().ParseDocument(await loginPage.Content.ReadAsStringAsync())
            .QuerySelector("form[action='/account/login/submit']")!;

        var answer = await browser.PostAsync("/account/login/submit", new FormUrlEncodedContent(
        [
            new("__RequestVerificationToken", form.QuerySelector("input[name=__RequestVerificationToken]")!.GetAttribute("value")!),
            new("Email", email),
            new("Password", password)
        ]));

        answer.StatusCode.Should().Be(HttpStatusCode.Redirect, "guard: the form was accepted and answered");
        return answer;
    }

    /// <summary>Moves an account to institutional sign-in: its password no longer signs it in.</summary>
    private async Task SignsInThroughItsInstitutionAsync(string email)
    {
        await using var scope = _host.Factory.Services.CreateAsyncScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<WombatIdentityUser>>();
        var user = await users.FindByEmailAsync(email);
        user!.AllowLocalPassword = false;
        (await users.UpdateAsync(user)).Succeeded.Should().BeTrue();
    }

    private static int NextSubnet() => Interlocked.Increment(ref _nextSubnet);

    /// <summary>
    /// What the provider's handler does at its callback, without a provider: puts an institutional sign-in, which asserts
    /// <paramref name="email" /> as verified, into the external cookie. Ahead of the app's own pipeline.
    /// </summary>
    private sealed class ProviderCallback(string email) : IStartupFilter
    {
        public const string Path = "/test/provider-callback";

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
        {
            app.Map(Path, branch => branch.Run(context =>
            {
                var principal = new ClaimsPrincipal(new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.NameIdentifier, "idp-subject-link"),
                    new Claim(ClaimTypes.Email, email),
                    new Claim("email_verified", "true")
                ], LinkProvider));
                var properties = new AuthenticationProperties();
                properties.Items["LoginProvider"] = LinkProvider;
                return context.SignInAsync(IdentityConstants.ExternalScheme, principal, properties);
            }));
            next(app);
        };
    }

    private static string NewEmail() => $"sign-in-{Guid.NewGuid():N}@example.test";
}
