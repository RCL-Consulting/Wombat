using FluentAssertions;
using Wombat.Domain.Identity;
using Wombat.Web.Navigation;

namespace Wombat.Web.Tests.Dashboards;

public sealed class DashboardPriorityTests
{
    [Fact]
    public void PriorityOrder_ContainsAllNineRoles()
    {
        DashboardPriority.Order.Should().HaveCount(9);
        foreach (var role in WombatRoles.All)
        {
            DashboardPriority.Order.Should().Contain(role);
        }
    }

    [Fact]
    public void Administrator_IsHighestPriority()
    {
        DashboardPriority.Order[0].Should().Be(WombatRoles.Administrator);
    }

    [Fact]
    public void PendingTrainee_IsLowestPriority()
    {
        DashboardPriority.Order[^1].Should().Be(WombatRoles.PendingTrainee);
    }

    [Fact]
    public void ValidRoles_MatchesPriorityOrder()
    {
        foreach (var role in DashboardPriority.Order)
        {
            DashboardPriority.ValidRoles.Should().Contain(role);
        }
    }

    [Fact]
    public void CookieName_IsNotEmpty()
    {
        DashboardPriority.CookieName.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Assessor_HasHigherPriorityThanTrainee()
    {
        var assessorIndex = DashboardPriority.Order.ToList().IndexOf(WombatRoles.Assessor);
        var traineeIndex = DashboardPriority.Order.ToList().IndexOf(WombatRoles.Trainee);
        assessorIndex.Should().BeLessThan(traineeIndex);
    }
}
