extern alias WombatWeb;

using System.Globalization;
using System.Net;
using System.Security.Claims;
using AngleSharp.Html.Parser;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Infrastructure.Persistence;
using Wombat.Integration.Tests.MultiSourceFeedback;

namespace Wombat.Integration.Tests.Hosting;

/// <summary>
/// T335, flow 01: a sign-in goes on to the address it was asked to come back to only when that is a path on this site
/// (<c>LocalUrl</c>), and to Home otherwise. The signed-out Access denied now builds such a link, <c>/account/login?ReturnUrl=…</c>.
/// </summary>
/// <remarks>
/// <para>
/// Until T335 the sign-in judged the address with <c>Uri.TryCreate(…, UriKind.Relative)</c>, which passes
/// <c>//evil.example</c> and <c>/\evil.example</c>: a browser follows either to another host. <c>LocalRedirect</c> then
/// refused it by throwing, so a correct password answered 500. The institutional sign-in's callback and the link page's
/// submit follow their return address by the same rule.
/// </para>
/// <para>
/// The review of the t335 branch: an address carrying a character above U+007E (<c>?returnUrl=%2F%C3%A9</c>) passed
/// <c>LocalUrl</c> and went into the <c>Location</c> header as it was. Kestrel refuses such a header value, so the answer
/// was a 500, at sign-in after the password had been checked and the row audited. <c>LocalUrl</c> now percent-encodes such
/// a character as UTF-8, the form a browser itself sends, so the person lands where they asked. The test server does not
/// refuse the header as Kestrel does, so each answer's raw <c>Location</c> is held to Kestrel's rule: printable ASCII only.
/// Every caller is tried in the shape it takes the address: the sign-in and the link page from a form's field, the
/// institutional sign-in's callback from its query (here), and the acting role's switch from its query
/// (<c>ActingRoleFlowTests</c>).
/// </para>
/// </remarks>
public sealed class SignInReturnAddressFlowTests : IClassFixture<MsfRespondPageFlowTests.WebHost>
{
    private const string ProviderKey = "kgk-return";

    private static int _nextSubnet;

    private readonly MsfRespondPageFlowTests.WebHost _host;

    public SignInReturnAddressFlowTests(MsfRespondPageFlowTests.WebHost host)
    {
        _host = host;
    }

    [Theory]
    [InlineData("/activities/inbox?page=2", "/activities/inbox?page=2")]
    [InlineData("/account/profile", "/account/profile")]
    [InlineData("//evil.example", "/")]
    [InlineData("/\\evil.example", "/")]
    [InlineData("https://evil.example/", "/")]
    [InlineData("", "/")]
    [InlineData("/é", "/%C3%A9")]
    [InlineData("/admin/users?search=José%20Ndö", "/admin/users?search=Jos%C3%A9%20Nd%C3%B6")]
    [InlineData("/activities/mine?q=\U0001F989", "/activities/mine?q=%F0%9F%A6%89")]
    public async Task ASignIn_GoesOnToItsReturnAddress_OnlyWhenItIsAPathOnThisSite(string returnUrl, string landsOn)
    {
        var email = $"return-address-{Guid.NewGuid():N}@example.test";
        await _host.CreateAssessorAsync(email);
        using var browser = _host.NewBrowser($"10.238.{Interlocked.Increment(ref _nextSubnet)}.7");

        using var loginPage = await browser.GetAsync("/account/login");
        var form = new HtmlParser().ParseDocument(await loginPage.Content.ReadAsStringAsync())
            .QuerySelector("form[action='/account/login/submit']")!;
        using var signIn = await browser.PostAsync("/account/login/submit", new FormUrlEncodedContent(
        [
            new("__RequestVerificationToken", form.QuerySelector("input[name=__RequestVerificationToken]")!.GetAttribute("value")!),
            new("Email", email),
            new("Password", MsfRespondPageFlowTests.WebHost.SignInPassword),
            new("ReturnUrl", returnUrl)
        ]));

        signIn.StatusCode.Should().Be(HttpStatusCode.Redirect, "the password is right, whatever the return address");
        Location(signIn).Should().Be(landsOn);
    }

    // The institutional sign-in's callback takes its return address from the query. A first sign-in with a verified email
    // no account holds is given an account, and goes on to the address.
    [Fact]
    public async Task AnInstitutionalSignIn_GoesOnToAReturnAddressOutsideAscii_PercentEncoded()
    {
        await using var app = WithProvider($"return-sso-{Guid.NewGuid():N}@example.test", await DemoInstitutionIdAsync());
        using var browser = NewHttpsBrowser(app);
        using (await browser.GetAsync(ProviderCallback.Path))
        {
        }

        using var callback = await browser.GetAsync("/account/sso-callback?returnUrl=%2Factivities%2Fmine%3Fq%3D%C3%A9");

        callback.StatusCode.Should().Be(HttpStatusCode.Redirect);
        Location(callback).Should().Be("/activities/mine?q=%C3%A9", "the account is made and signed in, and goes on there");
    }

    // The link page's form carries the return address the callback gave it, and its submit follows it once linked.
    [Fact]
    public async Task ALinkedInstitutionalSignIn_GoesOnToAReturnAddressOutsideAscii_PercentEncoded()
    {
        var email = $"return-link-{Guid.NewGuid():N}@example.test";
        var account = await _host.CreateAssessorAsync(email);
        await using var app = WithProvider(email, account.InstitutionId!.Value);
        using var browser = NewHttpsBrowser(app);
        using (await browser.GetAsync(ProviderCallback.Path))
        {
        }

        using (var callback = await browser.GetAsync("/account/sso-callback?returnUrl=%2F%C3%A9"))
        {
            Location(callback).Should().Be("/account/link-external?returnUrl=%2F%C3%A9", "guard: the account is offered a link");
        }

        using var page = await browser.GetAsync("/account/link-external?returnUrl=%2F%C3%A9");
        var form = new HtmlParser().ParseDocument(await page.Content.ReadAsStringAsync())
            .QuerySelector("form[action='/account/link-external/submit']")!;
        using var link = await browser.PostAsync("/account/link-external/submit", new FormUrlEncodedContent(
        [
            new("__RequestVerificationToken", form.QuerySelector("input[name=__RequestVerificationToken]")!.GetAttribute("value")!),
            new("ReturnUrl", form.QuerySelector("input[name=ReturnUrl]")!.GetAttribute("value")!),
            new("Password", MsfRespondPageFlowTests.WebHost.SignInPassword)
        ]));

        link.StatusCode.Should().Be(HttpStatusCode.Redirect);
        Location(link).Should().Be("/%C3%A9", "the password is right, so the login is linked and the person goes on");
    }

    /// <summary>
    /// The answer's <c>Location</c> as the server wrote it, held to what Kestrel lets through in a header value: printable
    /// ASCII only, or it answers 500 instead.
    /// </summary>
    /// <remarks>
    /// Read unvalidated: <c>Headers.GetValues</c> and <c>Headers.Location</c> parse the value as a URI and hand it back
    /// escaped, which hid the very character Kestrel refuses.
    /// </remarks>
    internal static string Location(HttpResponseMessage response)
    {
        response.Headers.NonValidated.TryGetValues("Location", out var values).Should().BeTrue("the answer is a redirect");
        var location = values.Should().ContainSingle().Which;
        location.Should().MatchRegex("^[\\x20-\\x7E]*$", "Kestrel refuses a header value holding any other character");
        return location;
    }

    private async Task<int> DemoInstitutionIdAsync()
    {
        await using var scope = _host.Factory.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Institutions
            .Where(entity => entity.ShortCode == "DEMO")
            .Select(entity => entity.Id)
            .SingleAsync();
    }

    /// <summary>The web host with one institutional sign-in configured, and a callback asserting <paramref name="email" /> ahead of it.</summary>
    private WebApplicationFactory<WombatWeb::Program> WithProvider(string email, int institutionId)
        => _host.Factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("Sso:Providers:0:Key", ProviderKey);
            builder.UseSetting("Sso:Providers:0:DisplayName", "KGK");
            builder.UseSetting("Sso:Providers:0:InstitutionId", institutionId.ToString(CultureInfo.InvariantCulture));
            builder.UseSetting("Sso:Providers:0:Authority", "https://idp.kgk.test");
            builder.UseSetting("Sso:Providers:0:ClientId", "wombat");
            builder.UseSetting("Sso:Providers:0:ClientSecret", "not-a-secret");
            builder.ConfigureTestServices(services => services.AddSingleton<IStartupFilter>(new ProviderCallback(email)));
        });

    /// <summary>A browser over HTTPS, as behind Caddy: the external cookie is a <c>__Host-</c> cookie, which is Secure always.</summary>
    private static HttpClient NewHttpsBrowser(WebApplicationFactory<WombatWeb::Program> app)
    {
        var browser = app.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });
        browser.DefaultRequestHeaders.Add("X-Forwarded-For", $"10.238.{Interlocked.Increment(ref _nextSubnet)}.7");
        return browser;
    }

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
                    new Claim(ClaimTypes.NameIdentifier, $"idp-subject-{Guid.NewGuid():N}"),
                    new Claim(ClaimTypes.Email, email),
                    new Claim("email_verified", "true")
                ], ProviderKey));
                var properties = new AuthenticationProperties();
                properties.Items["LoginProvider"] = ProviderKey;
                return context.SignInAsync(IdentityConstants.ExternalScheme, principal, properties);
            }));
            next(app);
        };
    }
}
