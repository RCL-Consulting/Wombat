using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Wombat.Infrastructure.Identity;

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

        var (_, _, document) = await host.LoadAsync($"{LinkPath}?error=The%20password%20is%20not%20right.");

        var alert = document.QuerySelector(".alert.alert-danger")!;
        alert.TextContent.Should().Contain("The password is not right.");
        alert.GetAttribute("role").Should().Be("alert");
        alert.Id.Should().Be("link-error");

        var password = document.GetElementById("link-password")!;
        password.HasAttribute("autofocus").Should().BeTrue();
        password.GetAttribute("aria-describedby").Should().Be("link-error");
        password.GetAttribute("autocomplete").Should().Be("current-password");
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
