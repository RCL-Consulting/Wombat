using FluentAssertions;
using Wombat.Application.Features.Programme;
using Wombat.Domain.Identity;
using Wombat.Web.Navigation;

namespace Wombat.Web.Tests.Navigation;

/// <summary>
/// T358 (flow 06, D2, E4): the role a programme page reads as. The acting role when the page admits it; else the first
/// role it admits that the person holds, in <see cref="DashboardPriority.Order" />; never the union of the roles held.
/// </summary>
public sealed class ProgrammeReadAsTests
{
    [Theory]
    [InlineData(WombatRoles.CommitteeMember)]
    [InlineData(WombatRoles.SpecialityAdmin)]
    [InlineData(WombatRoles.SubSpecialityAdmin)]
    [InlineData(WombatRoles.Coordinator)]
    public void TheActingRole_WhenThePageAdmitsIt(string role)
        => ProgrammeReadAs.RoleFor(Acting(role, role), ProgrammeScope.RosterRoles).Should().Be(role);

    /// <summary>
    /// A Speciality admin who sits on the committee reads as the role in hand, even where another held role ranks above it.
    /// </summary>
    [Fact]
    public void TheActingRole_WinsOverAHigherRankingRoleHeld()
        => ProgrammeReadAs.RoleFor(
                Acting(WombatRoles.CommitteeMember, WombatRoles.SpecialityAdmin, WombatRoles.CommitteeMember),
                ProgrammeScope.RosterRoles)
            .Should().Be(WombatRoles.CommitteeMember);

    [Fact]
    public void ACommitteeMemberActingAsAssessor_WhoTypesTheAddress_ReadsAsCommitteeMember()
        => ProgrammeReadAs.RoleFor(
                Acting(WombatRoles.Assessor, WombatRoles.CommitteeMember, WombatRoles.Assessor),
                ProgrammeScope.RosterRoles)
            .Should().Be(WombatRoles.CommitteeMember);

    /// <summary>Otherwise the first admitted role held, in the dashboards' precedence: Speciality admin before Committee member.</summary>
    [Fact]
    public void OtherwiseTheFirstAdmittedRoleHeld_InTheDashboardsPrecedence()
        => ProgrammeReadAs.RoleFor(
                Acting(
                    WombatRoles.Assessor,
                    WombatRoles.SpecialityAdmin, WombatRoles.CommitteeMember, WombatRoles.Coordinator, WombatRoles.Assessor),
                ProgrammeScope.RosterRoles)
            .Should().Be(WombatRoles.SpecialityAdmin);

    /// <summary>Waiting for assessors does not admit the Committee member: a member who is also Coordinator reads as Coordinator.</summary>
    [Fact]
    public void ARoleThePageDoesNotAdmit_IsPassedOver_ForOneItDoes()
        => ProgrammeReadAs.RoleFor(
                Acting(WombatRoles.CommitteeMember, WombatRoles.CommitteeMember, WombatRoles.Coordinator),
                ProgrammeScope.WaitingRoles)
            .Should().Be(WombatRoles.Coordinator);

    [Fact]
    public void NoAdmittedRoleHeld_IsNull()
    {
        ProgrammeReadAs.RoleFor(Acting(WombatRoles.Assessor, WombatRoles.Assessor), ProgrammeScope.RosterRoles).Should().BeNull();
        ProgrammeReadAs.RoleFor(Acting(WombatRoles.CommitteeMember, WombatRoles.CommitteeMember), ProgrammeScope.WaitingRoles)
            .Should().BeNull();
        ProgrammeReadAs.RoleFor(ActingRole.None, ProgrammeScope.RosterRoles).Should().BeNull();
    }

    /// <summary>The held roles in <see cref="DashboardPriority.Order" />, as <c>ActingRoleResolver</c> gives them.</summary>
    private static ActingRole Acting(string role, params string[] held)
        => new(role, DashboardPriority.Order.Where(held.Contains).ToList());
}
