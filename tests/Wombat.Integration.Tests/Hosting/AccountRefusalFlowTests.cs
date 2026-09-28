extern alias WombatWeb;

using System.Collections.Concurrent;
using System.Globalization;
using System.Net;
using System.Security.Claims;
using AngleSharp.Html.Dom;
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
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Domain.Identity;
using Wombat.Domain.Invitations;
using Wombat.Infrastructure.Identity;
using Wombat.Infrastructure.Persistence;
using Wombat.Integration.Tests.MultiSourceFeedback;
using WombatWeb::Wombat.Web.Security;

namespace Wombat.Integration.Tests.Hosting;

/// <summary>
/// T285 end to end, through the whole of <c>Wombat.Web</c>'s <c>Program.cs</c> on a PostgreSQL schema of its own: the
/// sign-in, link and register pages show fixed sentences for the codes their endpoints send, never text from the address,
/// and every real refusal still reads as it did before.
/// </summary>
/// <remarks>
/// <para>
/// Until T285 each endpoint redirected with the sentence in <c>?error=</c> and each page printed it as it arrived, so a
/// crafted link could put any words on Wombat's own sign-in page, and the register endpoint's catch put any exception's
/// message into the address. The expected sentences below are written out, not read from the classes that now hold them,
/// so a changed word fails here.
/// </para>
/// <para>
/// A browser is a client with its own cookies and its own address (<see cref="MsfRespondPageFlowTests.WebHost.NewBrowser" />),
/// so the sign-in throttle each one meets is its own.
/// </para>
/// </remarks>
public sealed class AccountRefusalFlowTests : IClassFixture<MsfRespondPageFlowTests.WebHost>
{
    private const string Crafted = "Your account is locked. Call 012 345 6789";
    private const string WrongPassword = "Not-the-Pa55word!";
    private const string NewUserPassword = "Registered-Pa55word!";
    private const string ProviderKey = "kgk";

    private static int _nextSubnet;

    private readonly MsfRespondPageFlowTests.WebHost _host;

    public AccountRefusalFlowTests(MsfRespondPageFlowTests.WebHost host)
    {
        _host = host;
    }

    // ─── Sign-in ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task TheSignInPage_ShowsOnlyTheGeneralSentence_ForWordsOfTheAddressesOwn()
    {
        using var browser = NewBrowser();

        var page = await LoadAsync(browser, "/account/login?error=Call%20012");

        Refusal(page).Should().Be("Sign-in could not be completed. Try again.");
        page.DocumentElement.OuterHtml.Should().NotContain("Call 012");

        var longer = await LoadAsync(browser, "/account/login?error=" + Uri.EscapeDataString(Crafted));
        Refusal(longer).Should().Be("Sign-in could not be completed. Try again.");
        longer.DocumentElement.OuterHtml.Should().NotContain("012 345 6789");
    }

    [Fact]
    public async Task ARefusedSignIn_TravelsAsACode_AndALockoutAsAWrongPassword()
    {
        var email = NewEmail();
        await _host.CreateAssessorAsync(email);
        using var browser = NewBrowser();

        var blank = await SignInAsync(browser, email, string.Empty);
        blank.Should().Be("/account/login?error=FieldsMissing");
        Refusal(await LoadAsync(browser, blank)).Should().Be("Enter your email and your password.");

        var wrong = await SignInAsync(browser, email, WrongPassword);
        wrong.Should().Be("/account/login?error=Refused");
        Refusal(await LoadAsync(browser, wrong)).Should().Be("Invalid email or password.");

        var unknown = await SignInAsync(browser, NewEmail(), WrongPassword);
        unknown.Should().Be(wrong, "an address no account has is refused as a wrong password is (T156)");

        // Identity's lockout trips at the account's fifth wrong password (one above, four here), and the lock refuses even
        // the right one. Each is sent back as a wrong password is, byte for byte (T287, T339, flow 02): until T339 the
        // lockout had a code of its own, which said the address had an account.
        var answers = new List<string>();
        for (var guess = 2; guess <= 5; guess++)
        {
            answers.Add(await SignInAsync(browser, email, WrongPassword));
        }

        answers.Add(await SignInAsync(browser, email, MsfRespondPageFlowTests.WebHost.SignInPassword));

        (await LockedOutAsync(email)).Should().BeTrue("guard: the fifth wrong password locked the account");
        answers.Should().AllBe(unknown);
        Refusal(await LoadAsync(browser, answers[^1])).Should().Be("Invalid email or password.");
    }

    [Fact]
    public async Task TheInstitutionalSignInsOwnRefusals_TravelAsCodes_AndReadAsTheSpecHasThem()
    {
        using var browser = NewBrowser();

        using var challenge = await browser.GetAsync("/account/sso-challenge/no-such-provider");
        challenge.Headers.Location!.ToString().Should().Be("/account/login?error=SsoUnknownProvider");
        Refusal(await LoadAsync(browser, challenge.Headers.Location.ToString()))
            .Should().Be("That institution's sign-in is not set up in Wombat. Sign in with your email and password.");

        // No institutional sign-in in progress: nothing to read at the callback, and nothing to link at the link submit.
        using var callback = await browser.GetAsync("/account/sso-callback?returnUrl=%2Fprogress");
        callback.Headers.Location!.ToString().Should().Be("/account/login?error=ExternalLoginUnavailable&returnUrl=%2Fprogress");
        Refusal(await LoadAsync(browser, callback.Headers.Location.ToString()))
            .Should().Be("Your institution's sign-in did not complete. Sign in with your email and password.",
                "this host offers no institution's button, so the words point at the email and password");

        var signInPage = await LoadAsync(browser, "/account/login");
        using var link = await browser.PostAsync("/account/link-external/submit", new FormUrlEncodedContent(
        [
            new("__RequestVerificationToken", AntiforgeryToken(signInPage)),
            new("Password", WrongPassword)
        ]));
        link.Headers.Location!.ToString().Should().Be("/account/login?error=ExternalSessionExpired");
        Refusal(await LoadAsync(browser, link.Headers.Location.ToString()))
            .Should().Be("Your institution's sign-in took too long and has expired. Sign in with your email and password.");
    }

    [Fact]
    public async Task ARefusedInstitutionalSignIn_TravelsAsItsCode_AndReadsAsItDidBefore()
    {
        // The provider does not verify the email, so no account is found by it or created from it (T155).
        await using var app = WithProvider(new ProviderCallback(NewEmail(), EmailVerified: false), await DemoInstitutionIdAsync());
        using var browser = NewHttpsBrowser(app);

        using (await browser.GetAsync(ProviderCallback.Path))
        {
        }

        using var callback = await browser.GetAsync("/account/sso-callback?returnUrl=%2F");
        callback.Headers.Location!.ToString().Should().Be("/account/login?error=SsoEmailNotVerified&returnUrl=%2F");
        Refusal(await LoadAsync(browser, callback.Headers.Location.ToString())).Should().Be(
            "Your institution's sign-in did not confirm your email address, so Wombat cannot use it to find your account or " +
            "create one. Sign in with your password if you have one, or ask your administrator for an invitation.");
    }

    // ─── Link ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task TheLinkPage_ShowsOnlyFixedSentences_AndARefusedLinkReadsAsItDidBefore()
    {
        var email = NewEmail();
        await _host.CreateAssessorAsync(email);
        await using var app = WithProvider(new ProviderCallback(email, EmailVerified: true), await InstitutionOfAsync(email));
        using var browser = NewHttpsBrowser(app);

        using (await browser.GetAsync(ProviderCallback.Path))
        {
        }

        using var callback = await browser.GetAsync("/account/sso-callback?returnUrl=%2F");
        callback.Headers.Location!.ToString().Should().Be("/account/link-external?returnUrl=%2F",
            "guard: the account is offered a link, with no refusal");

        var crafted = await LoadAsync(browser, "/account/link-external?returnUrl=%2F&error=" + Uri.EscapeDataString(Crafted));
        Refusal(crafted).Should().Be("The account could not be linked. Try again.");
        crafted.DocumentElement.OuterHtml.Should().NotContain("012 345 6789");

        var blank = await PostLinkAsync(browser, string.Empty);
        blank.Should().Be("/account/link-external?returnUrl=%2F&error=PasswordRequired");
        Refusal(await LoadAsync(browser, blank)).Should().Be("Enter your password.");

        var wrong = await PostLinkAsync(browser, WrongPassword);
        wrong.Should().Be("/account/link-external?returnUrl=%2F&error=SsoLinkRefused");
        Refusal(await LoadAsync(browser, wrong)).Should().Be("The account could not be linked. Check your password and try again.");
    }

    // ─── Register ────────────────────────────────────────────────────────────

    [Fact]
    public async Task TheRegisterPage_ShowsOnlyTheGeneralSentence_ForWordsOfTheAddressesOwn_AndStillOffersTheForm()
    {
        var (token, _) = await InviteAsync();
        using var browser = NewBrowser();

        var page = await LoadAsync(browser, $"/account/register?token={Uri.EscapeDataString(token)}&error=Call%20012");

        Refusal(page).Should().Be("Registration could not be completed. Try again.");
        page.DocumentElement.OuterHtml.Should().NotContain("Call 012");
        page.QuerySelector("form[action='/account/register/submit']").Should().NotBeNull("the invitation can still be used");
    }

    [Fact]
    public async Task ARefusedRegistration_TravelsAsCodes_AndReadsAsItDidBefore_ThenTheRightOneRegisters()
    {
        var (token, email) = await InviteAsync();
        using var browser = NewBrowser();
        var registerPage = $"/account/register?token={Uri.EscapeDataString(token)}";

        var mismatch = await RegisterAsync(browser, registerPage, NewUserPassword, "Something-else-1!");
        mismatch.Should().Be($"{registerPage}&error=ConfirmationMismatch");
        var afterMismatch = await LoadAsync(browser, mismatch);
        Refusal(afterMismatch).Should().Be("The password confirmation does not match.");
        afterMismatch.QuerySelector("form[action='/account/register/submit']").Should().NotBeNull(
            "the person puts it right on the same page; until T285 the refusal replaced the form");

        // Identity's rules, as codes: until T285 the address carried Identity's descriptions, joined with "; ".
        var weak = await RegisterAsync(browser, registerPage, "short", "short");
        weak.Should().StartWith($"{registerPage}&error=PasswordTooShort").And.NotContain("%20").And.NotContain("Passwords");
        var afterWeak = await LoadAsync(browser, weak);
        Refusal(afterWeak).Should().Be(
            "The new password needs: At least 12 characters. A digit (0 to 9). An upper-case letter. A symbol, such as ! or #.",
            "the rules broken, in the one order the rules are listed in (T339, flow 02, E11)");

        var registered = await RegisterAsync(browser, registerPage, NewUserPassword, NewUserPassword);
        registered.Should().Be("/", "guard: the right details register the account and sign it in");
        (await FindUserAsync(email)).Should().NotBeNull();

        // The same invitation again: refused as used, on the page and in the address.
        using var again = NewBrowser();
        var formPage = await LoadAsync(again, "/account/login");
        var used = await PostRegisterAsync(again, AntiforgeryToken(formPage), token, NewUserPassword, NewUserPassword);
        used.Should().Be($"{registerPage}&error=InvitationUsed");
        var afterUsed = await LoadAsync(again, used);
        Refusal(afterUsed).Should().Be("This invitation has already been used.");
        afterUsed.QuerySelector("form[action='/account/register/submit']").Should().BeNull(
            "an invitation that cannot be used offers no form");
    }

    [Fact]
    public async Task ARevokedInvitation_AndAMissingToken_ReadAsTheyDidBefore()
    {
        var (token, _) = await InviteAsync(revoked: true);
        using var browser = NewBrowser();
        var formPage = await LoadAsync(browser, "/account/login");

        var revoked = await PostRegisterAsync(browser, AntiforgeryToken(formPage), token, NewUserPassword, NewUserPassword);
        revoked.Should().Be($"/account/register?token={Uri.EscapeDataString(token)}&error=InvitationRevoked");
        Refusal(await LoadAsync(browser, revoked)).Should().Be("This invitation has been revoked.");

        var missing = await PostRegisterAsync(browser, AntiforgeryToken(formPage), token: null, NewUserPassword, NewUserPassword);
        missing.Should().Be("/account/register?error=TokenMissing");
        Refusal(await LoadAsync(browser, missing)).Should().Be("The invitation token is missing.");
    }

    /// <summary>
    /// T285 review: an invitation to an address an account already holds. Issuing does not check for one, and the register
    /// page keeps its form under a refusal the person can put right, so until the review every submit came back to the form
    /// refused as AccountExists again, with the invitation still unused. The preview now refuses the address as the submit
    /// does, so the page says so and offers no form, before a submit and after one.
    /// </summary>
    [Fact]
    public async Task AnInvitationToAnAddressAnAccountHolds_SaysSo_AndOffersNoForm_BeforeOrAfterASubmit()
    {
        var email = NewEmail();
        await _host.CreateAssessorAsync(email);
        var (token, _) = await InviteAsync(email: email);
        using var browser = NewBrowser();
        var registerPage = $"/account/register?token={Uri.EscapeDataString(token)}";

        var page = await LoadAsync(browser, registerPage);
        Refusal(page).Should().Be("A user with this email address already exists.");
        page.QuerySelector("form[action='/account/register/submit']").Should().BeNull("no submit could succeed");

        // A form posted anyway (one loaded before the account was created, say) is refused as the page was.
        var formPage = await LoadAsync(browser, "/account/login");
        var refused = await PostRegisterAsync(browser, AntiforgeryToken(formPage), token, NewUserPassword, NewUserPassword);
        refused.Should().Be($"{registerPage}&error=AccountExists");

        var afterSubmit = await LoadAsync(browser, refused);
        afterSubmit.QuerySelectorAll(".alert-danger").Should().ContainSingle()
            .Which.TextContent.Trim().Should().Be("A user with this email address already exists.");
        afterSubmit.QuerySelector("form[action='/account/register/submit']").Should().BeNull(
            "until the T285 review the form came back here, to be refused the same way on every submit");
        (await InvitationUsedOnAsync(token)).Should().BeNull("nothing was registered");
    }

    /// <summary>
    /// An invitation to an address Identity will not take as a user name can never be registered. Until T285 the refusal
    /// was Identity's own words, which quoted the address, and the form was hidden; T285 sent Identity's code, which read as
    /// the general sentence under a form that could never succeed. The preview now refuses it in a sentence of its own.
    /// </summary>
    [Fact]
    public async Task AnInvitationToAnAddressIdentityWillNotTake_SaysSo_AndOffersNoForm()
    {
        var address = $"o'brien-{Guid.NewGuid():N}@example.test";
        var (token, _) = await InviteAsync(email: address);
        using var browser = NewBrowser();
        var registerPage = $"/account/register?token={Uri.EscapeDataString(token)}";

        var page = await LoadAsync(browser, registerPage);
        Refusal(page).Should().Be(
            "This email address cannot be used for an account. Ask your administrator for an invitation to another address.");
        page.QuerySelector("form[action='/account/register/submit']").Should().BeNull();

        var formPage = await LoadAsync(browser, "/account/login");
        var refused = await PostRegisterAsync(browser, AntiforgeryToken(formPage), token, NewUserPassword, NewUserPassword);
        refused.Should().Be($"{registerPage}&error=AddressNotAccepted");
        (await FindUserAsync(address)).Should().BeNull();
    }

    /// <summary>
    /// A name of spaces passes the browser's <c>required</c>, and the command's own validator refuses it through the real
    /// pipeline: until T285 the page showed the validator's log text ("Validation failed: -- FirstName: ...").
    /// </summary>
    [Fact]
    public async Task ABlankName_IsRefusedByTheValidator_AsDetailsInvalid_AndKeepsTheForm()
    {
        var (token, email) = await InviteAsync();
        using var browser = NewBrowser();
        var registerPage = $"/account/register?token={Uri.EscapeDataString(token)}";

        var refused = await RegisterAsync(browser, registerPage, NewUserPassword, NewUserPassword, firstName: "   ");

        refused.Should().Be($"{registerPage}&error=DetailsInvalid");
        var page = await LoadAsync(browser, refused);
        Refusal(page).Should().Be("Enter your first name and last name, each of at most 100 characters, and a password.");
        page.QuerySelector("form[action='/account/register/submit']").Should().NotBeNull("the person puts it right here");
        (await FindUserAsync(email)).Should().BeNull();
    }

    [Fact]
    public async Task AFaultInRegistration_IsLogged_AndTheAddressCarriesOnlyACode()
    {
        var (token, _) = await InviteAsync();
        var logs = new CapturingLoggerProvider();
        await using var app = _host.Factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IInvitedUserProvisioner>();
            services.AddScoped<IInvitedUserProvisioner, FaultingProvisioner>();
            services.AddSingleton<ILoggerProvider>(logs);
        }));
        using var browser = app.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        browser.DefaultRequestHeaders.Add("X-Forwarded-For", NextAddress());
        var registerPage = $"/account/register?token={Uri.EscapeDataString(token)}";

        var failed = await RegisterAsync(browser, registerPage, NewUserPassword, NewUserPassword);

        failed.Should().Be($"{registerPage}&error=Failed", "an exception's message is never put in an address");
        Refusal(await LoadAsync(browser, failed)).Should().Be("Registration could not be completed. Try again.");
        logs.Entries.Should().Contain(entry => entry.Category == RegisterOutcome.LogCategory
                                               && entry.Level == LogLevel.Error
                                               && entry.Exception != null
                                               && entry.Exception.Message == FaultingProvisioner.Message,
            "the fault is logged, where an operator can read it");
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private HttpClient NewBrowser() => _host.NewBrowser(NextAddress());

    private static string NextAddress() => $"10.238.{Interlocked.Increment(ref _nextSubnet)}.7";

    private static string NewEmail() => $"refusal-{Guid.NewGuid():N}@example.test";

    private static async Task<IHtmlDocument> LoadAsync(HttpClient browser, string address)
    {
        using var response = await browser.GetAsync(address);
        response.StatusCode.Should().Be(HttpStatusCode.OK, $"guard: {address} loads");
        return new HtmlParser().ParseDocument(await response.Content.ReadAsStringAsync());
    }

    /// <summary>What the page's refusal says; null when it shows none.</summary>
    private static string? Refusal(IHtmlDocument page) => page.QuerySelector(".alert-danger")?.TextContent.Trim();

    private static string AntiforgeryToken(IHtmlDocument page)
        => page.QuerySelector("input[name=__RequestVerificationToken]")!.GetAttribute("value")!;

    /// <summary>Loads the sign-in page and posts its form, as the browser does; returns where the answer sends it.</summary>
    private static async Task<string> SignInAsync(HttpClient browser, string email, string password)
    {
        var page = await LoadAsync(browser, "/account/login");
        using var answer = await browser.PostAsync("/account/login/submit", new FormUrlEncodedContent(
        [
            new("__RequestVerificationToken", AntiforgeryToken(page)),
            new("Email", email),
            new("Password", password)
        ]));

        answer.StatusCode.Should().Be(HttpStatusCode.Redirect, "guard: the form was accepted and answered");
        return answer.Headers.Location!.ToString();
    }

    /// <summary>Loads the link page, which holds the institutional sign-in in progress, and posts its form.</summary>
    private static async Task<string> PostLinkAsync(HttpClient browser, string password)
    {
        var page = await LoadAsync(browser, "/account/link-external?returnUrl=%2F");
        var form = page.QuerySelector("form[action='/account/link-external/submit']");
        form.Should().NotBeNull("guard: the institutional sign-in is still held, so the page offers the link");

        using var answer = await browser.PostAsync("/account/link-external/submit", new FormUrlEncodedContent(
        [
            new("__RequestVerificationToken", AntiforgeryToken(page)),
            new("ReturnUrl", form!.QuerySelector("input[name=ReturnUrl]")!.GetAttribute("value")!),
            new("Password", password)
        ]));

        answer.StatusCode.Should().Be(HttpStatusCode.Redirect);
        return answer.Headers.Location!.ToString();
    }

    /// <summary>Loads the register page for an invitation and posts its form, as the browser does.</summary>
    private static async Task<string> RegisterAsync(
        HttpClient browser,
        string registerPage,
        string password,
        string confirmation,
        string firstName = "Refilwe")
    {
        var page = await LoadAsync(browser, registerPage);
        var form = page.QuerySelector("form[action='/account/register/submit']");
        form.Should().NotBeNull("guard: the invitation can be used, so the page offers the form");

        return await PostRegisterAsync(
            browser,
            AntiforgeryToken(page),
            form!.QuerySelector("input[name=Token]")!.GetAttribute("value"),
            password,
            confirmation,
            firstName);
    }

    private static async Task<string> PostRegisterAsync(
        HttpClient browser,
        string antiforgeryToken,
        string? token,
        string password,
        string confirmation,
        string firstName = "Refilwe")
    {
        var fields = new List<KeyValuePair<string, string>>
        {
            new("__RequestVerificationToken", antiforgeryToken),
            new("FirstName", firstName),
            new("LastName", "Dlamini"),
            new("Password", password),
            new("ConfirmPassword", confirmation)
        };
        if (token is not null)
        {
            fields.Add(new("Token", token));
        }

        using var answer = await browser.PostAsync("/account/register/submit", new FormUrlEncodedContent(fields));
        answer.StatusCode.Should().Be(HttpStatusCode.Redirect, "guard: the form was accepted and answered");
        return answer.Headers.Location!.ToString();
    }

    /// <summary>
    /// A Coordinator invitation to the demo institution, for <paramref name="email" /> or a new address, stored as issuing one
    /// stores it; returns its token.
    /// </summary>
    private async Task<(string Token, string Email)> InviteAsync(bool revoked = false, string? email = null)
    {
        var institutionId = await DemoInstitutionIdAsync();
        await using var scope = _host.Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var tokens = scope.ServiceProvider.GetRequiredService<IInvitationTokenService>();

        var token = tokens.GenerateToken();
        email ??= NewEmail();
        dbContext.Invitations.Add(new Invitation
        {
            Email = email,
            TokenHash = tokens.HashToken(token),
            TargetRole = WombatRoles.Coordinator,
            InstitutionId = institutionId,
            IssuedByUserId = "coordinator-web-1",
            IssuedOn = DateTime.UtcNow,
            ExpiresOn = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            RevokedOn = revoked ? DateTime.UtcNow : null
        });
        await dbContext.SaveChangesAsync();
        return (token, email);
    }

    /// <summary>When the invitation <paramref name="token" /> names was accepted; null while it has not been.</summary>
    private async Task<DateTime?> InvitationUsedOnAsync(string token)
    {
        await using var scope = _host.Factory.Services.CreateAsyncScope();
        var tokens = scope.ServiceProvider.GetRequiredService<IInvitationTokenService>();
        var invitations = await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Invitations
            .AsNoTracking()
            .ToListAsync();
        return invitations.Single(invitation => tokens.VerifyToken(token, invitation.TokenHash)).UsedOn;
    }

    private async Task<WombatIdentityUser?> FindUserAsync(string email)
    {
        await using var scope = _host.Factory.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<UserManager<WombatIdentityUser>>().FindByEmailAsync(email);
    }

    private async Task<bool> LockedOutAsync(string email)
    {
        await using var scope = _host.Factory.Services.CreateAsyncScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<WombatIdentityUser>>();
        return await users.IsLockedOutAsync((await users.FindByEmailAsync(email))!);
    }

    private async Task<int> InstitutionOfAsync(string email) => (await FindUserAsync(email))!.InstitutionId!.Value;

    private async Task<int> DemoInstitutionIdAsync()
    {
        await using var scope = _host.Factory.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Institutions
            .Where(entity => entity.ShortCode == "DEMO")
            .Select(entity => entity.Id)
            .SingleAsync();
    }

    /// <summary>The web host with one institutional sign-in configured, and <paramref name="callback" /> ahead of it.</summary>
    private WebApplicationFactory<WombatWeb::Program> WithProvider(ProviderCallback callback, int institutionId)
        => _host.Factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("Sso:Providers:0:Key", ProviderKey);
            builder.UseSetting("Sso:Providers:0:DisplayName", "KGK");
            builder.UseSetting("Sso:Providers:0:InstitutionId", institutionId.ToString(CultureInfo.InvariantCulture));
            builder.UseSetting("Sso:Providers:0:Authority", "https://idp.kgk.test");
            builder.UseSetting("Sso:Providers:0:ClientId", "wombat");
            builder.UseSetting("Sso:Providers:0:ClientSecret", "not-a-secret");
            builder.ConfigureTestServices(services => services.AddSingleton<IStartupFilter>(callback));
        });

    /// <summary>
    /// A browser over HTTPS, as behind Caddy: the external cookie is a <c>__Host-</c> cookie, which is Secure always.
    /// </summary>
    private static HttpClient NewHttpsBrowser(WebApplicationFactory<WombatWeb::Program> app)
    {
        var browser = app.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });
        browser.DefaultRequestHeaders.Add("X-Forwarded-For", NextAddress());
        return browser;
    }

    /// <summary>
    /// What the provider's handler does at its callback, without a provider: puts an institutional sign-in that asserts
    /// <paramref name="Email" />, verified or not, into the external cookie. Ahead of the app's own pipeline.
    /// </summary>
    private sealed record ProviderCallback(string Email, bool EmailVerified) : IStartupFilter
    {
        public const string Path = "/test/provider-callback";

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
        {
            app.Map(Path, branch => branch.Run(context =>
            {
                List<Claim> claims =
                [
                    new(ClaimTypes.NameIdentifier, $"idp-subject-{Guid.NewGuid():N}"),
                    new(ClaimTypes.Email, Email)
                ];
                if (EmailVerified)
                {
                    claims.Add(new Claim("email_verified", "true"));
                }

                var properties = new AuthenticationProperties();
                properties.Items["LoginProvider"] = ProviderKey;
                return context.SignInAsync(
                    IdentityConstants.ExternalScheme,
                    new ClaimsPrincipal(new ClaimsIdentity(claims, ProviderKey)),
                    properties);
            }));
            next(app);
        };
    }

    /// <summary>A provisioner that fails as a fault does: with a message no one should ever read in an address.</summary>
    private sealed class FaultingProvisioner : IInvitedUserProvisioner
    {
        public const string Message = "Npgsql: connection to 10.0.0.5 refused. Call 012 345 6789";

        /// <summary>The address is free, so the page offers the form, and the fault comes at the submit.</summary>
        public Task<InvitedAddressStatus> GetAddressStatusAsync(string email, CancellationToken cancellationToken = default)
            => Task.FromResult(InvitedAddressStatus.Available);

        public Task<ProvisionedInvitationUser> ProvisionAsync(
            string email,
            string password,
            string firstName,
            string lastName,
            string targetRole,
            int? institutionId,
            int? collegeId,
            int? specialityId,
            int? subSpecialityId,
            CancellationToken cancellationToken = default)
            => throw new InvalidOperationException(Message);
    }

    private sealed record LogEntry(string Category, LogLevel Level, Exception? Exception);

    private sealed class CapturingLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentQueue<LogEntry> _entries = new();

        public IReadOnlyList<LogEntry> Entries => _entries.ToList();

        public ILogger CreateLogger(string categoryName) => new Logger(categoryName, _entries);

        public void Dispose()
        {
        }

        private sealed class Logger(string category, ConcurrentQueue<LogEntry> entries) : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(
                LogLevel logLevel,
                EventId eventId,
                TState state,
                Exception? exception,
                Func<TState, Exception?, string> formatter)
                => entries.Enqueue(new LogEntry(category, logLevel, exception));
        }
    }
}
