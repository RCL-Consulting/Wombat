using Bunit;
using FluentAssertions;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Dashboards;
using static Wombat.Web.Tests.Dashboards.OversightHomeFixtures;

namespace Wombat.Web.Tests.Dashboards;

/// <summary>
/// The Speciality and Sub-speciality admin's Homes (T358, flow 06; R2-Home a1–a7; Q3, Q10, E3, E6): Waiting for assessors,
/// Registrars and Targets by EPA, each spanning three; "Pending reviews", "Trainees in programme" and "inactive" gone.
/// </summary>
public sealed class ProgrammeAdminHomeTests : OversightHomeContext
{
    [Fact]
    public void Typical_ThreeCards_WaitingFirst_TwoWaitingOneOverdue()
    {
        // a1 (Step 3.53): the portfolio review overdue, the Case-Based Discussion waiting less than a day.
        var cut = RenderDashboard<SpecialityAdminDashboard>(WombatRoles.SpecialityAdmin, SpecialityAdmin());

        Titles(cut).Should().Equal("Waiting for assessors", "Registrars", "Targets by EPA");
        var waiting = Section(cut, "card-waiting-assessors");
        waiting.ClassList.Should().Contain(["dashboard-span-3", "detail-card--warning"]).And.NotContain("detail-card--emphasis");
        var badge = waiting.QuerySelector("h2 .badge")!;
        badge.TextContent.Should().Be("2 waiting, 1 overdue");
        badge.ClassList.Should().Contain("badge-submitted");
        Text(waiting.QuerySelector("p.needs-you-rule")!).Should().Be(
            "Oldest first. Overdue once it has waited 7 days. Its assessor is emailed after 5.");
    }

    [Fact]
    public void EachWaitingRow_IsItsOwnLink_WithWhomItWaits_ThenHowLong()
    {
        // E6: "With Mohammed Patel" on its own line, then flow 04's "Waiting 8 days".
        var cut = RenderDashboard<SpecialityAdminDashboard>(WombatRoles.SpecialityAdmin, SpecialityAdmin());

        var rows = Section(cut, "card-waiting-assessors").QuerySelectorAll("li.needs-you-row").ToList();
        rows.Should().HaveCount(2);
        rows[0].ClassList.Should().Contain("needs-you-row--overdue");
        rows[0].QuerySelector("a")!.GetAttribute("href").Should().Be("/activities/10");
        Text(rows[0].QuerySelector("a")!).Should().Be(
            "Portfolio and Logbook Review (Paediatrics) · PAED-015 · 2026-10-03, from Pieter du Plessis");
        rows[0].QuerySelectorAll(".needs-you-badges .badge").Select(Text).Should().Equal("Awaiting review", "Overdue");
        rows[0].QuerySelectorAll("p.needs-you-why").Select(Text).Should().Equal("With Mohammed Patel", "Waiting 8 days");
        rows[1].QuerySelectorAll("p.needs-you-why").Select(Text).Should().Equal("With Fatima Khumalo", "Waiting less than a day");
        rows.Should().OnlyContain(row => row.QuerySelector("button") == null, "a reminder is the page's, not Home's");
        var foot = Section(cut, "card-waiting-assessors").QuerySelector(".dashboard-card-footer a")!;
        foot.GetAttribute("href").Should().Be("/programme/waiting");
        Text(foot).Should().Be("Open Waiting for assessors");
    }

    [Fact]
    public void NothingWaiting_NoBadge_TheEmphasisStripe_AndTheFootKept()
    {
        // a2 (Step 2.38), a4.
        var cut = RenderDashboard<SpecialityAdminDashboard>(WombatRoles.SpecialityAdmin, SpecialityAdmin(
            waiting: Waiting(Paediatrics, []), card: Card(AllZeros())));

        var waiting = Section(cut, "card-waiting-assessors");
        waiting.ClassList.Should().Contain("detail-card--emphasis").And.NotContain("detail-card--warning");
        waiting.QuerySelectorAll(".badge").Should().BeEmpty("no \"0\"");
        Text(waiting.QuerySelector("p.card-empty")!).Should().Be("Nothing is waiting for an assessor.");
        waiting.QuerySelectorAll("p.needs-you-rule").Should().BeEmpty();
        waiting.QuerySelector(".dashboard-card-footer a")!.GetAttribute("href").Should().Be("/programme/waiting");
    }

    [Fact]
    public void Heavy_FiveRows_ThenHowManyMoreWait()
    {
        // a3: 14 waiting, 11 overdue.
        var items = Enumerable.Range(1, 5)
            .Select(i => Request(100 + i, "Mini-CEX (Paediatrics)", "PAED-004", $"2026-09-0{i}", "Nomsa Mahlangu", "Thandi Zulu", "Requested", 10, true))
            .ToList();
        var cut = RenderDashboard<SpecialityAdminDashboard>(WombatRoles.SpecialityAdmin, SpecialityAdmin(
            waiting: Waiting(Paediatrics, items, total: 14, overdue: 11)));

        var waiting = Section(cut, "card-waiting-assessors");
        waiting.QuerySelectorAll("li.needs-you-row").Should().HaveCount(5);
        Text(waiting.QuerySelector("p.waiting-more")!).Should().Be("9 more wait in Waiting for assessors.");
        waiting.QuerySelector("h2 .badge")!.TextContent.Should().Be("14 waiting, 11 overdue");
    }

    [Fact]
    public void TheSubSpecialityAdminsHome_IsTheSameThreeCards()
    {
        // a5 (Step 3.54).
        var cut = RenderDashboard<SubSpecialityAdminDashboard>(WombatRoles.SubSpecialityAdmin, SubSpecialityAdmin());

        Titles(cut).Should().Equal("Waiting for assessors", "Registrars", "Targets by EPA");
        Section(cut, "card-waiting-assessors").QuerySelector("h2 .badge")!.TextContent.Should().Be("2 waiting, 1 overdue");
        Section(cut, "card-registrars").QuerySelectorAll("li.roster-row").Should().HaveCount(5);
    }

    [Fact]
    public void NoOldWord_NoInactive_NoSlashFigure_NoPronoun()
    {
        foreach (var cut in new[]
                 {
                     (Bunit.IRenderedFragment)RenderDashboard<SpecialityAdminDashboard>(WombatRoles.SpecialityAdmin, SpecialityAdmin()),
                 })
        {
            var text = Text(cut.Find(".dashboard-grid"));
            text.Should().NotContainAny("inactive", "Trainees in programme", "Pending reviews", "Curriculum coverage", "awaiting review ", "%");
            text.Should().NotMatchRegex(@"\d+ ?/ ?\d+");
            Programme.ProgrammeWordsFixtures.NamesAPersonByPronoun(text).Should().BeFalse();
            cut.FindAll(".detail-card--interactive").Should().BeEmpty("a card is never one link around its rows (T280)");
        }
    }

    [Fact]
    public void Loading_ThreeTitlesOverSkeletons()
    {
        // a6.
        var cut = RenderDashboard<SpecialityAdminDashboard>(WombatRoles.SpecialityAdmin, null, Reads.Hang);

        Titles(cut).Should().Equal("Waiting for assessors", "Registrars", "Targets by EPA");
        cut.FindAll(".dashboard-grid a, .dashboard-grid .badge, .detail-card--warning").Should().BeEmpty();
    }

    [Fact]
    public void ALoadError_IsTheFramesOneAlert()
    {
        // a7.
        var cut = RenderDashboard<SubSpecialityAdminDashboard>(WombatRoles.SubSpecialityAdmin, null, Reads.Throw);

        cut.FindAll(".alert-danger").Should().ContainSingle();
        cut.FindAll(".detail-card").Should().BeEmpty();
    }
}
