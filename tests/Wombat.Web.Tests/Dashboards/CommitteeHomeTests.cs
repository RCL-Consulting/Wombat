using Bunit;
using FluentAssertions;
using Wombat.Application.Features.Programme.Trainees;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Dashboards;
using static Wombat.Web.Tests.Dashboards.OversightHomeFixtures;

namespace Wombat.Web.Tests.Dashboards;

/// <summary>
/// The Committee member's Home (T358, flow 06; R2-Home c1–c11): Registrars and Targets by EPA, each a section named by its
/// title and spanning three, every row its own link, no card a link around them, and none of the old words.
/// </summary>
public sealed class CommitteeHomeTests : OversightHomeContext
{
    [Fact]
    public void Typical_TwoSections_RegistrarsThenTargetsByEpa_EachSpanningThree()
    {
        // c1 (Step 3.52).
        var cut = RenderDashboard<CommitteeMemberDashboard>(WombatRoles.CommitteeMember, Committee());

        Titles(cut).Should().Equal("Registrars", "Targets by EPA");
        var registrars = Section(cut, "card-registrars");
        registrars.ClassList.Should().Contain("dashboard-span-3");
        registrars.QuerySelector("h2#card-registrars .badge")!.TextContent.Should().Be("5 registrars");
        registrars.QuerySelector("h2 .badge")!.ClassList.Should().Contain("badge-draft", "a count of people (D10)");
        Text(registrars.QuerySelector("p.needs-you-rule")!).Should().Be("Fewest met first, then by surname.");
        Section(cut, "card-epa-targets").ClassList.Should().Contain("dashboard-span-3");
        cut.FindAll(".detail-card--interactive").Should().BeEmpty("a card is never one link around its rows (T280)");
    }

    [Fact]
    public void EachRegistrar_IsALinkToTheRegistrarsPage_OverTheTrainingYear_WithMyProgresssTwoFigures()
    {
        var cut = RenderDashboard<CommitteeMemberDashboard>(WombatRoles.CommitteeMember, Committee());

        var rows = Section(cut, "card-registrars").QuerySelectorAll("li.roster-row").ToList();
        rows.Select(row => Text(row.QuerySelector("a.progress-row-link")!)).Should().Equal(
            "Pieter du Plessis", "Nomsa Mahlangu", "Sipho Ndlovu", "Anele Dlamini", "Lerato Molefe");
        rows[0].QuerySelector("a")!.GetAttribute("href").Should().Be("/programme/trainees/101");
        Text(rows[0].QuerySelector(".progress-row-meta")!).Should().Be("Training year 2");

        var figures = rows[3].QuerySelectorAll(".dashboard-metric").ToList();
        figures.Select(figure => Text(figure.QuerySelector(".count-figure")!)).Should().Equal("1 of 10", "0 of 5");
        figures.Select(figure => Text(figure.QuerySelector(".dashboard-metric-label")!))
            .Should().Equal("EPAs met this semester", "EPAs met in 2026");
        Section(cut, "card-registrars").QuerySelector(".dashboard-card-footer a")!.GetAttribute("href").Should().Be("/programme/trainees");
        Text(Section(cut, "card-registrars").QuerySelector(".dashboard-card-footer a")!).Should().Be("Open Programme trainees");
    }

    [Fact]
    public void TargetsByEpa_EachEpaALinkToTheRegistrarsShortOnIt_FewestMetFirst_AndNoFoot()
    {
        var cut = RenderDashboard<CommitteeMemberDashboard>(WombatRoles.CommitteeMember, Committee());

        var card = Section(cut, "card-epa-targets");
        Text(card.QuerySelector("p.needs-you-rule")!).Should().Be("Fewest registrars met first. Semester 2, 2026 ends on 2026-11-30.");
        card.QuerySelectorAll("li.coverage-row a").Select(link => link.GetAttribute("href"))
            .Should().Equal("/programme/trainees?short=2", "/programme/trainees?short=8", "/programme/trainees?short=1");
        card.QuerySelectorAll(".count-figure").Select(Text).Should().Equal("0 of 5", "0 of 5", "2 of 5");
        card.QuerySelectorAll(".dashboard-card-footer").Should().BeEmpty("Targets by EPA has no foot");
    }

    [Fact]
    public void AllZeros_InSurnameOrder_EveryEpaNoughtOfFive()
    {
        // c4 (Step 2.33) and c5 (Step 2.37, the external member, after T290).
        var cut = RenderDashboard<CommitteeMemberDashboard>(WombatRoles.CommitteeMember, Committee(
            Card(AllZeros()),
            Coverage(0, Epa(1, "PAED-001", "Providing paediatric emergency care to children", 0, 5),
                Epa(2, "PAED-002", "Managing common paediatric presentations", 0, 5))));

        Section(cut, "card-registrars").QuerySelectorAll("a.progress-row-link").Select(Text).Should().Equal(
            "Anele Dlamini", "Pieter du Plessis", "Nomsa Mahlangu", "Lerato Molefe", "Sipho Ndlovu");
        Section(cut, "card-epa-targets").QuerySelectorAll(".count-figure").Select(Text).Should().OnlyContain(figure => figure == "0 of 5");
    }

    [Fact]
    public void AnExemptRegistrar_SaysWhyInWords_AndHasNoFigure()
    {
        // c6: the exempt registrar listed last; the EPA card counts four, in the one wording.
        var exempt = Registrar("Sipho Ndlovu", 1, exemption: ProgrammeExemption.StartedPartWay);
        var cut = RenderDashboard<CommitteeMemberDashboard>(WombatRoles.CommitteeMember, Committee(
            Card([.. Typical().Where(row => row.Name != "Sipho Ndlovu"), exempt]),
            Coverage(1, Epa(1, "PAED-001", "Providing paediatric emergency care to children", 2, 4))));

        var last = Section(cut, "card-registrars").QuerySelectorAll("li.roster-row").Last();
        Text(last.QuerySelector("a")!).Should().Be("Sipho Ndlovu");
        last.QuerySelectorAll(".dashboard-metric").Should().BeEmpty();
        var why = last.QuerySelector(".roster-exempt")!;
        Text(why.QuerySelector(".badge")!).Should().Be("Exempt this period");
        why.QuerySelector(".badge")!.ClassList.Should().Contain("badge-draft");
        Text(why.QuerySelector(".progress-row-meta")!).Should().Be("Started part-way through the period");
        Text(Section(cut, "card-epa-targets").QuerySelector("p.needs-you-rule")!).Should().StartWith("1 registrar exempt this period, not counted. ");
    }

    [Fact]
    public void NobodyToShow_EachCardSaysSo_AndRegistrarsKeepsItsFoot()
    {
        // c7: Programme trainees is in the menu, so the card keeps its foot, empty included (round 3 item 27).
        var cut = RenderDashboard<CommitteeMemberDashboard>(WombatRoles.CommitteeMember, Committee(
            Wombat.Application.Features.Dashboards.Oversight.RegistrarsCardDto.Empty, NoCoverage()));

        var registrars = Section(cut, "card-registrars");
        registrars.QuerySelectorAll(".badge").Should().BeEmpty("no \"0\" badge");
        Text(registrars.QuerySelector("p.card-empty")!).Should().Be(
            "No current registrars. They appear here once they are admitted to the programme.");
        registrars.QuerySelector(".dashboard-card-footer a")!.GetAttribute("href").Should().Be("/programme/trainees");
        Text(Section(cut, "card-epa-targets").QuerySelector("p.card-empty")!).Should().Be("No targets this period: there is no current registrar.");
    }

    [Fact]
    public void TheStorysEnd_KgkFirst_NamedAsTheHospitalsOwn()
    {
        // c8 (A.7.7): two current registrars, KGK-001 nobody has met first.
        var cut = RenderDashboard<CommitteeMemberDashboard>(WombatRoles.CommitteeMember, Committee(
            Card([Registrar("Anele Dlamini", 3), Registrar("Nomsa Mahlangu", 1)]),
            Coverage(0,
                Epa(21, "KGK-001", "Running a paediatric outreach clinic at a district hospital", 0, 2, QuotaPeriod.AcademicYear, 1,
                    owner: "Kgosi Kgari Teaching Hospital"),
                Epa(1, "PAED-001", "Providing paediatric emergency care to children", 1, 2))));

        var first = Section(cut, "card-epa-targets").QuerySelector("li.coverage-row")!;
        Text(first.QuerySelector("a")!).Should().StartWith("KGK-001 — Running a paediatric outreach clinic at a district hospital");
        Text(first.QuerySelector(".progress-row-meta")!).Should().Be("1 per academic year · Kgosi Kgari Teaching Hospital's own");
        Section(cut, "card-registrars").QuerySelector("h2 .badge")!.TextContent.Should().Be("2 registrars");
    }

    [Fact]
    public void Heavy_FiveRows_ThenHowManyMore_AndAnEpaEveryRegistrarHasMetIsText()
    {
        // c11.
        var cut = RenderDashboard<CommitteeMemberDashboard>(WombatRoles.CommitteeMember, Committee(
            Card(Typical(), total: 8),
            Coverage(0,
                Epa(1, "PAED-001", "Providing paediatric emergency care to children", 3, 8),
                Epa(7, "PAED-007", "Maintaining and promoting the health and well-being of children", 8, 8, target: 1))));

        var registrars = Section(cut, "card-registrars");
        registrars.QuerySelectorAll("li.roster-row").Should().HaveCount(5);
        Text(registrars.QuerySelector("p.waiting-more")!).Should().Be("3 more in Programme trainees.");
        registrars.QuerySelector("h2 .badge")!.TextContent.Should().Be("8 registrars");

        var met = Section(cut, "card-epa-targets").QuerySelectorAll("li.coverage-row").Last();
        met.QuerySelectorAll("a").Should().BeEmpty("nobody is short on it");
        Text(met.QuerySelector(".progress-row-meta")!).Should().EndWith(" · every registrar has met it");
    }

    [Fact]
    public void NoOldWord_NoSlashFigure_NoPercentage_NoPronoun()
    {
        var cut = RenderDashboard<CommitteeMemberDashboard>(WombatRoles.CommitteeMember, Committee());

        var text = Text(cut.Find(".dashboard-grid"));
        text.Should().NotContainAny("inactive", "Trainees in programme", "Pending reviews", "Targets this period", "Targets met by EPA",
            "semester 0/", "yearly ", "%");
        text.Should().NotMatchRegex(@"\d+ ?/ ?\d+");
        Programme.ProgrammeWordsFixtures.NamesAPersonByPronoun(text).Should().BeFalse();
    }

    [Fact]
    public void Loading_TwoTitlesOverSkeletons_NothingOffered()
    {
        // c9.
        var cut = RenderDashboard<CommitteeMemberDashboard>(WombatRoles.CommitteeMember, null, Reads.Hang);

        Titles(cut).Should().Equal("Registrars", "Targets by EPA");
        cut.FindAll(".skeleton").Should().NotBeEmpty();
        cut.FindAll(".dashboard-grid a, .dashboard-grid .badge").Should().BeEmpty();
    }

    [Fact]
    public void ALoadError_IsTheFramesOneAlert_AndNoCard()
    {
        // c10.
        var cut = RenderDashboard<CommitteeMemberDashboard>(WombatRoles.CommitteeMember, null, Reads.Throw);

        Text(cut.Find(".alert-danger .alert-row-text")).Should().Be(
            "Could not load your Home. Nothing has changed. Try again, or come back in a few minutes.");
        cut.FindAll(".detail-card").Should().BeEmpty();
    }
}
