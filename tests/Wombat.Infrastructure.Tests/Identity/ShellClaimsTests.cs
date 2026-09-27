using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Common.Extensions;
using Wombat.Infrastructure.Identity;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Infrastructure.Tests.Identity;

/// <summary>
/// T335, flow 01: the shell reads no database (review S7), so what it shows of the person comes from the sign-in cookie.
/// Sign-in issues their name as a claim, falling back to their email. Built on the real Identity stack over the in-memory
/// store, with the app's own claims factory.
/// </summary>
public sealed class ShellClaimsTests : IDisposable
{
    private readonly ServiceProvider _root;
    private readonly IServiceScope _scope;

    public ShellClaimsTests()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        services.AddIdentity<WombatIdentityUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddClaimsPrincipalFactory<WombatUserClaimsPrincipalFactory>();

        _root = services.BuildServiceProvider();
        _scope = _root.CreateScope();
    }

    public void Dispose()
    {
        _scope.Dispose();
        _root.Dispose();
    }

    private UserManager<WombatIdentityUser> Users => _scope.ServiceProvider.GetRequiredService<UserManager<WombatIdentityUser>>();

    [Fact]
    public async Task TheDisplayName_IsFirstNameAndLastName()
    {
        var principal = await SignInPrincipalAsync(await CreateUserAsync("t.zulu@kgk.test", "Thandi", "Zulu"));

        principal.FindAll(WombatClaims.DisplayName).Select(claim => claim.Value).Should().Equal("Thandi Zulu");
        principal.GetDisplayName().Should().Be("Thandi Zulu");
    }

    [Theory]
    [InlineData("  Thandi ", " Zulu  ", "Thandi Zulu")]
    [InlineData("Thandi", "", "Thandi")]
    [InlineData(" ", "Zulu", "Zulu")]
    public void TheDisplayName_IsTrimmed_AndSkipsABlankPart(string first, string last, string expected)
    {
        WombatUserClaimsPrincipalFactory.DisplayNameOf(new WombatIdentityUser
        {
            UserName = "someone@kgk.test", Email = "someone@kgk.test", FirstName = first, LastName = last
        }).Should().Be(expected);
    }

    // The bootstrap administrator and an account whose names were erased have none: the shell shows the email.
    [Fact]
    public async Task AnAccountWithNoName_IsShownByItsEmail()
    {
        var principal = await SignInPrincipalAsync(await CreateUserAsync("admin@wombat.test", " ", ""));

        principal.FindAll(WombatClaims.DisplayName).Select(claim => claim.Value).Should().Equal("admin@wombat.test");
        principal.GetDisplayName().Should().Be("admin@wombat.test");
    }

    // A principal from before the claim existed (a cookie issued before this change) is still named: by its sign-in name.
    [Fact]
    public void APrincipalWithoutTheClaim_IsNamedByItsSignInName()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, "old.cookie@kgk.test")], "Identity.Application"));

        principal.GetDisplayName().Should().Be("old.cookie@kgk.test");
        new ClaimsPrincipal(new ClaimsIdentity()).GetDisplayName().Should().BeNull();
    }

    // The acting role is stored with the account (D1, W-010) and carried into the cookie, so the shell resolves it from
    // claims alone. The claim is what is stored: whether the person still holds it is the resolver's question.
    [Fact]
    public async Task TheStoredActingRole_IsIssuedAsAClaim()
    {
        var user = await CreateUserAsync("t.zulu@kgk.test", "Thandi", "Zulu", actingRole: "Assessor");

        var principal = await SignInPrincipalAsync(user);

        principal.FindAll(WombatClaims.ActingRole).Select(claim => claim.Value).Should().Equal("Assessor");
    }

    [Fact]
    public async Task NoStoredActingRole_IssuesNoClaim()
    {
        var principal = await SignInPrincipalAsync(await CreateUserAsync("d.naidoo@kgk.test", "David", "Naidoo"));

        principal.HasClaim(claim => claim.Type == WombatClaims.ActingRole).Should().BeFalse();
    }

    // A change is carried by the next cookie issued from the account, as the switch endpoint's RefreshSignInAsync issues it.
    [Fact]
    public async Task AChangedActingRole_IsWhatTheNextPrincipalCarries()
    {
        var user = await CreateUserAsync("t.zulu@kgk.test", "Thandi", "Zulu", actingRole: "CommitteeMember");

        user.ActingRole = "Assessor";
        (await Users.UpdateAsync(user)).Succeeded.Should().BeTrue();

        (await SignInPrincipalAsync(user)).FindFirst(WombatClaims.ActingRole)!.Value.Should().Be("Assessor");
    }

    private async Task<WombatIdentityUser> CreateUserAsync(string email, string firstName, string lastName, string? actingRole = null)
    {
        var user = new WombatIdentityUser
        {
            UserName = email, Email = email, EmailConfirmed = true, FirstName = firstName, LastName = lastName,
            ActingRole = actingRole
        };
        (await Users.CreateAsync(user)).Succeeded.Should().BeTrue();
        return user;
    }

    /// <summary>The principal sign-in builds, through the factory the app registers (<c>AddClaimsPrincipalFactory</c>).</summary>
    private Task<ClaimsPrincipal> SignInPrincipalAsync(WombatIdentityUser user)
        => _scope.ServiceProvider.GetRequiredService<IUserClaimsPrincipalFactory<WombatIdentityUser>>().CreateAsync(user);
}
