using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Wombat.Infrastructure.Identity;
using Wombat.Web.Security;

namespace Wombat.Web.Tests.Hosting;

/// <summary>
/// The link-your-account page as a browser receives it (T193). A refused link reloads it with the reason in the query,
/// and the password field takes focus at once, so the field names the refusal: an alert already there when the page
/// loads is not reliably announced.
/// </summary>
public sealed class LinkExternalLoginPageTests
{
    private const string LinkPath = "/account/link-external";

    [Fact]
    public async Task ARefusedLink_IsReadWithThePasswordField()
    {
        await using var host = await StartAsync();

        var (_, _, document) = await host.LoadAsync($"{LinkPath}?error=SsoLinkRefused");

        var alert = document.QuerySelector(".alert.alert-danger")!;
        alert.TextContent.Trim().Should().Be("The account could not be linked. Check your password and try again.");
        alert.GetAttribute("role").Should().Be("alert");
        alert.Id.Should().Be("link-error");

        var password = document.GetElementById("link-password")!;
        password.HasAttribute("autofocus").Should().BeTrue();
        password.GetAttribute("aria-describedby").Should().Be("link-error");
        password.GetAttribute("autocomplete").Should().Be("current-password");
    }

    /// <summary>
    /// Each code the link endpoint sends reads as the refusal did before T285, when the endpoint sent the words. Written
    /// out, so a changed word fails here.
    /// </summary>
    [Theory]
    [InlineData("PasswordRequired", "Your password is required.")]
    [InlineData("SsoLinkRefused", "The account could not be linked. Check your password and try again.")]
    [InlineData("SsoLinkLockedOut", "Too many failed attempts. Please try again later or reset your password.")]
    [InlineData("SsoAlreadyLinked", "This institutional sign-in is already linked to an account.")]
    [InlineData("SsoLinkFailed", "Linking failed.")]
    [InlineData("SsoUnknownProvider", "Unknown SSO provider.")]
    [InlineData("SsoNoEmail", "The identity provider did not supply an email address.")]
    [InlineData("SsoEmailNotVerified",
        "Your institution's sign-in did not confirm your email address, so Wombat cannot use it to find your account or " +
        "create one. Sign in with your password if you have one, or ask your administrator for an invitation.")]
    [InlineData("SsoAdministrator", "An administrator account signs in with its password, not through institutional sign-in.")]
    [InlineData("SsoAccountLocked", "This account is locked. Contact your administrator.")]
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
            .And.Be("The account could not be linked. Please try again.");
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

    private static Task<AppTestHost> StartAsync()
        => AppTestHost.StartAsync(services => services.AddScoped<SignInManager<WombatIdentityUser>>(provider =>
            new ExternalSignInInProgress(provider.GetRequiredService<IHttpContextAccessor>())));

    /// <summary>
    /// A sign-in manager whose external sign-in is in progress, for the one call the page makes. The host has no
    /// database, so the user manager behind it has a store that answers nothing.
    /// </summary>
    private sealed class ExternalSignInInProgress(IHttpContextAccessor accessor) : SignInManager<WombatIdentityUser>(
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
            => Task.FromResult<ExternalLoginInfo?>(new ExternalLoginInfo(
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
