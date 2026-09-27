using System.Security.Claims;
using System.Text.Json;
using FluentAssertions;
using Wombat.Application.Common.Security;
using Wombat.Domain.Identity;
using Wombat.Web.Navigation;

namespace Wombat.Web.Tests.Navigation;

/// <summary>
/// T335, flow 01 (D1, R2-Rules § 1–2): the acting role is the one stored with the account while the person holds it, else
/// the first role they hold in the precedence, else none. It chooses only what the frame shows; access is the union of the
/// roles held. One resolver, read from the sign-in cookie's claims, so the shell reads no database.
/// </summary>
public sealed class ActingRoleResolverTests
{
    [Fact]
    public void TheStoredRole_IsTheActingRole_WhenThePersonHoldsIt()
    {
        var acting = ActingRoleResolver.Resolve(WombatRoles.Assessor, [WombatRoles.CommitteeMember, WombatRoles.Assessor]);

        acting.Role.Should().Be(WombatRoles.Assessor, "a choice is honoured over the precedence");
    }

    // A stored role the person no longer holds (an administrator removed it, or a forged value) is never shown: the frame
    // falls back to the precedence.
    [Theory]
    [InlineData(WombatRoles.Administrator)]
    [InlineData("Registrar")]
    [InlineData("assessor")]
    [InlineData("")]
    [InlineData(null)]
    public void AStoredRoleThePersonDoesNotHold_IsIgnored_AndThePrecedenceDecides(string? stored)
    {
        var acting = ActingRoleResolver.Resolve(stored, [WombatRoles.Assessor, WombatRoles.CommitteeMember]);

        acting.Role.Should().Be(WombatRoles.CommitteeMember, "Committee member comes before Assessor");
    }

    [Fact]
    public void ThePrecedence_IsDashboardPriorityOrder()
    {
        ActingRoleResolver.Resolve(null, WombatRoles.All).Role.Should().Be(WombatRoles.Administrator);
        ActingRoleResolver.Resolve(null, [WombatRoles.PendingTrainee, WombatRoles.Trainee]).Role.Should().Be(WombatRoles.Trainee);
        ActingRoleResolver.Resolve(null, [WombatRoles.Coordinator, WombatRoles.CommitteeMember]).Role
            .Should().Be(WombatRoles.CommitteeMember);
    }

    [Fact]
    public void SomeoneWhoHoldsNoRole_HasNoActingRole()
    {
        var acting = ActingRoleResolver.Resolve(WombatRoles.Trainee, []);

        acting.Role.Should().BeNull("a former trainee holds no role, and a stored one they no longer hold is not shown");
        acting.HeldRoles.Should().BeEmpty();
        acting.OtherRoles.Should().BeEmpty();
    }

    [Fact]
    public void TheHeldRoles_AreListedInThePrecedence_KnownRolesOnly_OnceEach()
    {
        var acting = ActingRoleResolver.Resolve(
            WombatRoles.Assessor,
            [WombatRoles.Trainee, WombatRoles.Assessor, "Registrar", WombatRoles.SpecialityAdmin, WombatRoles.Assessor]);

        acting.HeldRoles.Should().Equal(WombatRoles.SpecialityAdmin, WombatRoles.Assessor, WombatRoles.Trainee);
        acting.OtherRoles.Should().Equal(WombatRoles.SpecialityAdmin, WombatRoles.Trainee);
    }

    [Fact]
    public void FromClaims_TheStoredRoleIsTheActingRoleClaim_AndTheHeldRolesAreTheRoleClaims()
    {
        var user = Principal(WombatRoles.Assessor, WombatRoles.CommitteeMember, WombatRoles.Assessor);

        var acting = ActingRoleResolver.Resolve(user);

        acting.Role.Should().Be(WombatRoles.Assessor);
        acting.HeldRoles.Should().Equal(WombatRoles.CommitteeMember, WombatRoles.Assessor);
    }

    [Fact]
    public void FromClaims_AStoredRoleNotHeld_IsIgnored()
    {
        ActingRoleResolver.Resolve(Principal(WombatRoles.Administrator, WombatRoles.Trainee)).Role.Should().Be(WombatRoles.Trainee);
        ActingRoleResolver.Resolve(Principal(null, WombatRoles.Coordinator)).Role.Should().Be(WombatRoles.Coordinator);
    }

    [Fact]
    public void AVisitorWhoHasNotSignedIn_HasNoActingRole()
    {
        ActingRoleResolver.Resolve(new ClaimsPrincipal(new ClaimsIdentity())).Should().Be(ActingRole.None);
    }

    // Routes is an interactive root: what App passes it is written into the page and read back by the circuit as JSON
    // (the Web defaults Blazor serialises root parameters with). The acting role and the switch's result, its nonce
    // included (a resumed circuit is known by it), must survive it.
    [Fact]
    public void WhatAppPassesRoutes_SurvivesTheCircuitsJson()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        var acting = ActingRoleResolver.Resolve(WombatRoles.Assessor, [WombatRoles.CommitteeMember, WombatRoles.Assessor]);
        var result = new ActingRoleSwitchResult(WombatRoles.CommitteeMember, WombatRoles.Assessor, Nonce: "3F2A9C01B7D84E6F9A0B1C2D3E4F5061");

        var actingBack = (ActingRole)JsonSerializer.Deserialize(JsonSerializer.Serialize<object>(acting, options), typeof(ActingRole), options)!;
        var resultBack = (ActingRoleSwitchResult)JsonSerializer.Deserialize(
            JsonSerializer.Serialize<object>(result, options), typeof(ActingRoleSwitchResult), options)!;

        actingBack.Role.Should().Be(WombatRoles.Assessor);
        actingBack.HeldRoles.Should().Equal(WombatRoles.CommitteeMember, WombatRoles.Assessor);
        actingBack.OtherRoles.Should().Equal(WombatRoles.CommitteeMember);
        resultBack.Should().Be(result);
    }

    private static ClaimsPrincipal Principal(string? stored, params string[] roles)
    {
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, "user-1"), new(ClaimTypes.Name, "t.zulu@kgk.test") };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        if (stored is not null)
        {
            claims.Add(new Claim(WombatClaimTypes.ActingRole, stored));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "Identity.Application"));
    }
}
