using System.Security.Claims;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Dashboards.InstitutionalAdmin;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Dashboards;
using Wombat.Web.Components.Shared;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.Dashboards;

/// <summary>
/// Her Home (T376, flow 12, lane U; boards R3-H01–H05): People's counts as links to Users filtered by each role (S19), the
/// account count, Curriculum's adopted counts and line, Your institution with Status set by a Wombat administrator and a
/// long address that wraps (M7), the waiting line only while someone waits (Q8, S23), and no Quick links (Q7).
/// </summary>
public sealed class InstitutionalAdminHomeTests : TestContext
{
    private const string Kgk = "Kgosi Kgari Teaching Hospital";

    public InstitutionalAdminHomeTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("mbatha@kgk.wombat.local");
        auth.SetRoles(WombatRoles.InstitutionalAdmin);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "mbatha"));
    }

    [Fact]
    public void H01_People_EachRoleItsCountAndLabelAsOneLinkToUsersFilteredByIt_ThenTheAccounts()
    {
        var cut = Render(At244());

        var people = Card(cut, "home-card-people");
        Text(people.QuerySelector("h2")!).Should().Be("People");
        var links = people.QuerySelectorAll(".dashboard-metric-row a.dashboard-metric");
        links.Select(Text).Should().Equal(
            "1 Institutional admin", "1 Speciality admin", "1 Sub-speciality admin", "1 Coordinator", "4 Committee member",
            "5 Assessor", "5 Trainee");
        links.Select(link => link.GetAttribute("href")).Should().Equal(
            WombatRoles.All
                .Where(role => role is not (WombatRoles.Administrator or WombatRoles.CollegeAdmin or WombatRoles.PendingTrainee))
                .Select(role => PeopleLinks.UsersFiltered(role: role)));
        Text(people.QuerySelector(".dashboard-card-footer .field-help")!).Should().Be("15 accounts.");
        var open = people.QuerySelector(".dashboard-card-footer a.btn")!;
        Text(open).Should().Be("Open Users");
        open.GetAttribute("href").Should().Be(PeopleLinks.Users);
    }

    [Fact]
    public void H01_Curriculum_WhatSheHasAdopted_AndItsLine()
    {
        var cut = Render(At244());

        var curriculum = Card(cut, "home-card-curriculum");
        curriculum.QuerySelectorAll(".dashboard-metric").Select(Text).Should().Equal("1 speciality adopted", "1 sub-speciality adopted");
        curriculum.QuerySelectorAll("a.dashboard-metric").Should().BeEmpty("these figures are not links");
        Text(curriculum.QuerySelector("p.field-help")!).Should().Be(
            "Paediatrics / Paediatrics, from the CPSA's Paediatric EPA Curriculum (11.1).");
        curriculum.QuerySelector(".dashboard-card-footer a")!.GetAttribute("href").Should().Be(OnboardingLinks.CurriculumAdoptions);
    }

    [Fact]
    public void H01_YourInstitution_ItsRecord_StatusSetByAWombatAdministrator_AndItsLink()
    {
        var cut = Render(At244());

        var card = Card(cut, "home-card-institution");
        card.QuerySelectorAll("dl.detail-list dt").Select(Text).Should().Equal("Name", "Short code", "Contact email", "Status");
        card.QuerySelectorAll("dl.detail-list dd").Select(Text).Should().Equal(
            Kgk, "KGK", "paeds-admin@kgk.wombat.local", "Active Set by a Wombat administrator.");
        card.QuerySelectorAll("dl.detail-list dd")[2].InnerHtml.Should().Contain("paeds-admin@<wbr>kgk.wombat.local");
        var open = card.QuerySelector(".dashboard-card-footer a")!;
        Text(open).Should().Be($"Open {Kgk}");
        open.GetAttribute("href").Should().Be(PeopleLinks.Institution(1));
    }

    [Fact]
    public void H01_NoQuickLinks_NoWaitingLine_AndNoInlineStyle()
    {
        var cut = Render(At244());

        cut.FindAll(".dashboard-card-title").Select(Text).Should().Equal("People", "Curriculum", "Your institution");
        cut.FindAll("#home-waiting-line").Should().BeEmpty("nobody waits at 2.44");
        cut.Markup.Should().NotContain("Quick links");
        cut.FindAll("[style]").Should().BeEmpty("no inline style (S18)");
    }

    [Fact]
    public void H01b_ALongContactAddress_BreaksAfterItsAt_InTheCardsWrappingList()
    {
        var address = new string('a', 47) + "@kgk.wombat.local";
        var cut = Render(At244() with { Institution = At244().Institution! with { ContactEmail = address } });

        Card(cut, "home-card-institution").QuerySelectorAll("dl.detail-list dd")[2].InnerHtml.Should().Be($"{new string('a', 47)}@<wbr>kgk.wombat.local");
    }

    [Fact]
    public void H02_HerFirstSignIn_OneAccountHers_AndNothingAdopted()
    {
        var cut = Render(At244() with
        {
            UsersByRole = [new RoleCountItem(WombatRoles.InstitutionalAdmin, 1)],
            AccountCount = 1,
            OnlyAccountIsYours = true,
            AdoptedSpecialityCount = 0,
            AdoptedSubSpecialityCount = 0,
            Adoptions = []
        });

        Text(Card(cut, "home-card-people").QuerySelector(".dashboard-card-footer .field-help")!).Should().Be("1 account: yours.");
        var curriculum = Card(cut, "home-card-curriculum");
        curriculum.QuerySelectorAll(".dashboard-metric").Select(Text).Should().Equal("0 specialities adopted", "0 sub-specialities adopted");
        Text(curriculum.QuerySelector("p.field-help")!).Should().Be(
            "Nothing is adopted yet. Adopt the College's curriculum to set up a programme.");
    }

    [Fact]
    public void H04_FiveWaiting_TheLineAboveTheCards_NotALiveRegion_WithOpenTrainees()
    {
        var cut = Render(At244() with { WaitingCount = 5 });

        var line = cut.Find("#home-waiting-line");
        line.ClassList.Should().Contain(["alert", "alert-info"]);
        line.GetAttribute("role").Should().BeNull("a standing notice is not a live region (S23)");
        Text(line.QuerySelector("strong")!).Should().Be("5 registrars are waiting to be admitted.");
        Text(line.QuerySelector(".alert-row-text")!).Should().Be(
            "5 registrars are waiting to be admitted. Nothing can be filed about them until then.");
        var open = line.QuerySelector("a.btn")!;
        Text(open).Should().Be("Open Trainees");
        open.GetAttribute("href").Should().Be(OnboardingLinks.Trainees);
        line.CompareDocumentPosition(cut.Find(".dashboard-grid")).HasFlag(AngleSharp.Dom.DocumentPositions.Following)
            .Should().BeTrue("the line is above the cards");
    }

    [Fact]
    public void OneWaiting_HasItsSingular()
    {
        var cut = Render(At244() with { WaitingCount = 1 });

        Text(cut.Find("#home-waiting-line strong")).Should().Be("1 registrar is waiting to be admitted.");
    }

    private IRenderedComponent<InstitutionalAdminDashboard> Render(InstitutionalAdminDashboardSummaryDto summary)
    {
        Services.AddSingleton<IScopedSender>(new Sender(summary));
        var cut = RenderComponent<InstitutionalAdminDashboard>();
        cut.WaitForState(() => cut.FindAll(".skeleton").Count == 0);
        return cut;
    }

    private static IElement Card(IRenderedFragment cut, string headingId)
        => cut.Find($"section[aria-labelledby={headingId}]");

    private static string Text(IElement element)
        => string.Join(' ', element.TextContent.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    /// <summary>2.44's Home (H01).</summary>
    private static InstitutionalAdminDashboardSummaryDto At244()
        => new(
            [
                new RoleCountItem(WombatRoles.InstitutionalAdmin, 1),
                new RoleCountItem(WombatRoles.SpecialityAdmin, 1),
                new RoleCountItem(WombatRoles.SubSpecialityAdmin, 1),
                new RoleCountItem(WombatRoles.Coordinator, 1),
                new RoleCountItem(WombatRoles.CommitteeMember, 4),
                new RoleCountItem(WombatRoles.Assessor, 5),
                new RoleCountItem(WombatRoles.Trainee, 5)
            ],
            AccountCount: 15,
            OnlyAccountIsYours: false,
            WaitingCount: 0,
            AdoptedSpecialityCount: 1,
            AdoptedSubSpecialityCount: 1,
            Adoptions: [new AdoptedCurriculumItem("Paediatrics", "Paediatrics", "CPSA", "Paediatric EPA Curriculum", "11.1")],
            Institution: new InstitutionCardItem(1, Kgk, "KGK", "paeds-admin@kgk.wombat.local", IsActive: true));

    private sealed class Sender(InstitutionalAdminDashboardSummaryDto summary) : IScopedSender
    {
        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
            => request is GetInstitutionalAdminDashboardSummaryQuery
                ? Task.FromResult((TResponse)(object)summary)
                : throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
