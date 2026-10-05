using System.Reflection;
using System.Security.Claims;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Wombat.Application.Features.Programme;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Programme;
using Wombat.Web.Navigation;
using Wombat.Web.Tests.Navigation;
using Wombat.Web.Tests.TestSupport;

namespace Wombat.Web.Tests.Programme;

/// <summary>
/// T358 (flow 06, lane A0): the three programme pages' addresses and who they admit (D5), fixed before the pages are
/// built, so the menus and the owner table can name them. Programme trainees and the registrar page admit the four
/// roles that read the programme's registrars; Waiting for assessors the three that may remind. Not the Administrator or
/// the Institutional admin (no menu offers them the pages; flows 12 and 18 decide).
/// </summary>
/// <remarks>
/// The last test holds the wave-0 shells: each draws its title and its h1. Lanes C and D replace it when they build the
/// pages, whose own tests then hold the headings; lane B replaces the owner-table test when it names the owners.
/// </remarks>
public sealed class ProgrammePageShellTests : TestContext
{
    private static readonly Type[] ThePages = [typeof(ProgrammeTrainees), typeof(ProgrammeTraineeDetail), typeof(WaitingForAssessors)];

    public static TheoryData<Type, string> Routes => new()
    {
        { typeof(ProgrammeTrainees), "/programme/trainees" },
        { typeof(ProgrammeTraineeDetail), "/programme/trainees/{ProfileId:int}" },
        { typeof(WaitingForAssessors), "/programme/waiting" },
    };

    [Theory]
    [MemberData(nameof(Routes))]
    public void EachPage_AnswersItsAddress(Type page, string template)
    {
        page.GetCustomAttributes<RouteAttribute>().Select(route => route.Template).Should().Equal(template);
        PageAccess.PageFor(template.Replace("{ProfileId:int}", "7", StringComparison.Ordinal)).Should().Be(page);
    }

    [Fact]
    public void TheRegistrarPage_TakesTheProfileFromItsAddress()
        => typeof(ProgrammeTraineeDetail).GetProperty(nameof(ProgrammeTraineeDetail.ProfileId))!
            .GetCustomAttribute<ParameterAttribute>().Should().NotBeNull();

    [Theory]
    [InlineData(typeof(ProgrammeTrainees))]
    [InlineData(typeof(ProgrammeTraineeDetail))]
    public Task TheRosterPages_AdmitTheFourRosterRoles_AndRefuseEveryOther(Type page)
        => AdmitsExactly(page, ProgrammeScope.RosterRoles);

    [Fact]
    public Task WaitingForAssessors_AdmitsTheThreeThatMayRemind_AndRefusesEveryOther()
        => AdmitsExactly(typeof(WaitingForAssessors), ProgrammeScope.WaitingRoles);

    // Lane B (T358): Programme trainees and Waiting for assessors are menu items, each its own page's; the registrar page is
    // under Programme trainees for the four roles that read it (ActiveNavItemTests lights them).
    [Fact]
    public void TheListsAreMenuItems_AndTheRegistrarPageIsUnderProgrammeTrainees()
    {
        NavItems.ProgrammeTrainees.Page.Should().Be(typeof(ProgrammeTrainees));
        NavItems.WaitingForAssessors.Page.Should().Be(typeof(WaitingForAssessors));
        NavOwners.Table.Should().NotContainKey(typeof(ProgrammeTrainees)).And.NotContainKey(typeof(WaitingForAssessors));
        NavOwners.Table[typeof(ProgrammeTraineeDetail)].Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new NavOwners.Owner(ProgrammeScope.RosterRoles, NavItems.ProgrammeTrainees));
    }

    // Each page's title and heading are held by its own tests now: ProgrammeTraineesPageTests,
    // ProgrammeTraineeDetailTests and WaitingForAssessorsPageTests (T358).

    private static async Task AdmitsExactly(Type page, IReadOnlyList<string> admitted)
    {
        foreach (var role in WombatRoles.All)
        {
            var refusal = await PageAccess.RefusalOf(page, role);
            if (admitted.Contains(role))
            {
                refusal.Should().BeNull($"{page.Name} admits a {role}");
            }
            else
            {
                refusal.Should().NotBeNull($"{page.Name} refuses a {role}");
            }
        }
    }
}
