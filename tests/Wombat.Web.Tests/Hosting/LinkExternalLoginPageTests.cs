using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Wombat.Infrastructure.Identity;
using Wombat.Web.Security;
using Wombat.Web.Tests.TestSupport;

namespace Wombat.Web.Tests.Hosting;

/// <summary>
/// The link-your-institutional-sign-in page as a browser receives it (T193; T339, flow 02, R3-LK-*). A refused link
/// reloads it with the reason in the query; the refusal stands in the slot above Password and takes the focus itself, and
/// Password names it: an alert already there when the page loads is not reliably announced.
/// </summary>
/// <remarks>
/// Changed deliberately by T339 (flow 02): the heading is the page's &lt;h1&gt;, "Link your institutional sign-in"; the
/// refusal takes the focus, where Password took it and named the refusal (C6); the expired state is a page of its own
/// ("Institutional sign-in expired"), not a danger alert; the button says "Link and sign in".
/// </remarks>
public sealed class LinkExternalLoginPageTests
{
    private const string LinkPath = "/account/link-external";

    [Fact]
    public async Task TheLivePage_AsksForThePassword_WhichTakesTheFocus()
    {
        await using var host = await StartAsync();

        var (_, _, document) = await host.LoadAsync($"{LinkPath}?returnUrl=%2F");

        document.QuerySelectorAll("h1").Should().ContainSingle().Which.TextContent.Trim().Should().Be("Link your institutional sign-in");
        document.Title.Should().Be("Link your institutional sign-in · Wombat");
        document.QuerySelector(".account-email")!.TextContent.Should().Be("registrar@hospital.test");

        var password = document.GetElementById("link-password")!;
        password.HasAttribute("autofocus").Should().BeTrue();
        password.GetAttribute("autocomplete").Should().Be("current-password");
        PasswordToggleMarkup.ShouldBeTheToggle(document, "link-password", "Show password", PasswordToggleMarkup.Driven.ByScript);

        document.QuerySelectorAll(".form-actions .btn").Select(button => button.TextContent.Trim())
            .Should().Equal("Cancel", "Link and sign in");
    }

    [Fact]
    public async Task ARefusedLink_StandsAbovePassword_TakesTheFocus_AndPasswordNamesIt()
    {
        await using var host = await StartAsync();

        var (_, _, document) = await host.LoadAsync($"{LinkPath}?error=SsoLinkRefused");

        var alert = document.QuerySelector(".alert.alert-danger")!;
        alert.TextContent.Trim().Should().Be("The account could not be linked. Check your password and try again.");
        alert.GetAttribute("role").Should().Be("alert");
        alert.Id.Should().Be("link-error");
        alert.GetAttribute("tabindex").Should().Be("-1");
        alert.HasAttribute("autofocus").Should().BeTrue();
        alert.NextElementSibling!.QuerySelector("#link-password").Should().NotBeNull("the slot is above Password");

        var password = document.GetElementById("link-password")!;
        password.HasAttribute("autofocus").Should().BeFalse("the refusal's autofocus replaces the field's");
        password.GetAttribute("aria-describedby").Should().Be("link-error");
        document.Title.Should().Be("Error: Link your institutional sign-in · Wombat");
    }

    [Fact]
    public async Task WhenTheSignInInProgressHasGone_ThePageSaysSo_WithNoField_AndTheWayBack()
    {
        await using var host = await StartAsync(expired: true);

        var (_, _, document) = await host.LoadAsync($"{LinkPath}?returnUrl=%2F");

        document.QuerySelectorAll("h1").Should().ContainSingle().Which.TextContent.Trim().Should().Be("Institutional sign-in expired");
        document.Title.Should().Be("Institutional sign-in expired · Wombat");
        document.QuerySelector(".account-form-container p")!.TextContent.Trim()
            .Should().Be("Your institutional sign-in has expired. Start again from the sign-in page.");
        document.QuerySelectorAll("input:not([type=hidden])").Should().BeEmpty("there is nothing left to link");
        document.QuerySelector(".alert").Should().BeNull("not a refusal: nothing the person did was refused");
        var back = document.QuerySelector(".form-actions a.btn")!;
        back.TextContent.Trim().Should().Be("Back to sign in");
        back.GetAttribute("href").Should().Be("/account/login");
    }

    /// <summary>
    /// Each code the link endpoint sends reads as round 3's Spec has it (T339, flow 02; T285 before it). Written out, so a
    /// changed word fails here. A lockout reads as any other refused link (T287).
    /// </summary>
    [Theory]
    [InlineData("PasswordRequired", "Enter your password.")]
    [InlineData("SsoLinkRefused", "The account could not be linked. Check your password and try again.")]
    [InlineData("SsoLinkLockedOut", "The account could not be linked. Check your password and try again.")]
    [InlineData("SsoAlreadyLinked", "This institutional sign-in is already linked to an account.")]
    [InlineData("SsoLinkFailed", "Your institutional sign-in could not be linked this time. Try again later.")]
    [InlineData("SsoUnknownProvider", "That institution's sign-in is not set up in Wombat. Sign in with your email and password.")]
    [InlineData("SsoNoEmail",
        "Your institution's sign-in did not give Wombat your email address, so Wombat cannot find your account. Sign in " +
        "with your email and password, or ask your administrator for help.")]
    [InlineData("SsoEmailNotVerified",
        "Your institution's sign-in did not confirm your email address, so Wombat cannot use it to find your account or " +
        "create one. Sign in with your password if you have one, or ask your administrator for an invitation.")]
    [InlineData("SsoAdministrator", "An administrator account signs in with its password, not through institutional sign-in.")]
    [InlineData("SsoAccountLocked", "Wombat could not sign you in through your institution. Contact your administrator.")]
    [InlineData("SsoWrongInstitution",
        "This account is not registered at the institution this sign-in belongs to. Contact your administrator.")]
    public async Task EachRefusal_ReadsAsItDidBefore(string code, string expected)
    {
        await using var host = await StartAsync();

        var (_, _, document) = await host.LoadAsync($"{LinkPath}?returnUrl=%2F&error={code}");

        document.QuerySelector(".alert.alert-danger")!.TextContent.Trim().Should().Be(expected);
    }

    /// <summary>
    /// Until T285 the page printed its <c>?error=</c> as it arrived, so a crafted link could put words of its choosing on
    /// it. Now any code the page does not know reads as one general sentence.
    /// </summary>
    [Theory]
    [InlineData("Your%20account%20is%20locked.%20Call%20012%20345%206789", "012 345 6789")]
    [InlineData("Call%20012", "Call 012")]
    [InlineData("%3Cb%3Enot%20a%20code%3C%2Fb%3E", "not a code")]
    public async Task WordsOfTheAddressesOwn_NeverReachThePage(string error, string words)
    {
        await using var host = await StartAsync();

        var (_, html, document) = await host.LoadAsync($"{LinkPath}?error={error}");

        document.QuerySelector(".alert.alert-danger")!.TextContent.Trim().Should().Be(LinkExternalOutcome.GeneralRefusal)
            .And.Be("The account could not be linked. Try again.");
        html.Should().NotContain(words);
        document.GetElementById("link-password")!.GetAttribute("aria-describedby").Should().Be("link-error",
            "the general sentence is a refusal like any other, read with the field");
    }

    [Fact]
    public async Task AFirstVisit_NamesNoRefusal()
    {
        await using var host = await StartAsync();

        var (_, _, document) = await host.LoadAsync(LinkPath);

        document.QuerySelector(".alert").Should().BeNull();
        document.GetElementById("link-password")!.HasAttribute("aria-describedby").Should().BeFalse(
            "there is no refusal for it to name");
    }

    private static Task<AppTestHost> StartAsync(bool expired = false)
        => AppTestHost.StartAsync(services => services.AddScoped<SignInManager<WombatIdentityUser>>(provider =>
            new ExternalSignInInProgress(provider.GetRequiredService<IHttpContextAccessor>(), expired)));

    /// <summary>
    /// A sign-in manager whose external sign-in is in progress, for the one call the page makes. The host has no
    /// database, so the user manager behind it has a store that answers nothing.
    /// </summary>
    private sealed class ExternalSignInInProgress(IHttpContextAccessor accessor, bool expired) : SignInManager<WombatIdentityUser>(
        Users,
        accessor,
        new UserClaimsPrincipalFactory<WombatIdentityUser>(Users, Microsoft.Extensions.Options.Options.Create(new IdentityOptions())),
        Microsoft.Extensions.Options.Options.Create(new IdentityOptions()),
        NullLogger<SignInManager<WombatIdentityUser>>.Instance,
        new AuthenticationSchemeProvider(Microsoft.Extensions.Options.Options.Create(new AuthenticationOptions())),
        new DefaultUserConfirmation<WombatIdentityUser>())
    {
        private static readonly UserManager<WombatIdentityUser> Users =
            new(new NoStore(), null!, null!, null!, null!, null!, null!, null!, NullLogger<UserManager<WombatIdentityUser>>.Instance);

        public override Task<ExternalLoginInfo?> GetExternalLoginInfoAsync(string? expectedXsrf = null)
            => Task.FromResult<ExternalLoginInfo?>(expired ? null : new ExternalLoginInfo(
                new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Email, "registrar@hospital.test")], "oidc")),
                "oidc",
                "subject-1",
                "Hospital sign-in"));
    }

    private sealed class NoStore : IUserStore<WombatIdentityUser>
    {
        public void Dispose()
        {
        }

        public Task<string> GetUserIdAsync(WombatIdentityUser user, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<string?> GetUserNameAsync(WombatIdentityUser user, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task SetUserNameAsync(WombatIdentityUser user, string? userName, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<string?> GetNormalizedUserNameAsync(WombatIdentityUser user, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task SetNormalizedUserNameAsync(WombatIdentityUser user, string? normalizedName, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<IdentityResult> CreateAsync(WombatIdentityUser user, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<IdentityResult> UpdateAsync(WombatIdentityUser user, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<IdentityResult> DeleteAsync(WombatIdentityUser user, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<WombatIdentityUser?> FindByIdAsync(string userId, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<WombatIdentityUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken) => throw new NotSupportedException();
    }
}
