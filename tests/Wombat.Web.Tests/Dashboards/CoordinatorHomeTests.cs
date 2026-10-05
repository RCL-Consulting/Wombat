using Bunit;
using FluentAssertions;
using Wombat.Application.Features.Dashboards.Oversight;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Dashboards;
using static Wombat.Web.Tests.Dashboards.OversightHomeFixtures;

namespace Wombat.Web.Tests.Dashboards;

/// <summary>
/// The Coordinator's Home (T358, flow 06; R2-Home k1–k6; Q3, Q6, E3, E5): Waiting for assessors spanning two, Nothing
/// filed in 30 days and Invitations nearing expiry; no "Quick action" card (its action is Home's header's, HomeFrameTests).
/// </summary>
public sealed class CoordinatorHomeTests : OversightHomeContext
{
    [Fact]
    public void Typical_ThreeWaitingTwoOverdue_OldestFirst()
    {
        // k1 (Step 3.30).
        var cut = RenderDashboard<CoordinatorDashboard>(WombatRoles.Coordinator, Coordinator());

        Titles(cut).Should().Equal("Waiting for assessors", "Nothing filed in 30 days", "Invitations nearing expiry");
        var waiting = Section(cut, "card-waiting-assessors");
        waiting.ClassList.Should().Contain(["dashboard-span-2", "detail-card--warning"]);
        waiting.QuerySelector("h2 .badge")!.TextContent.Should().Be("3 waiting, 2 overdue");
        waiting.QuerySelectorAll("li.needs-you-row a").Select(link => link.GetAttribute("href"))
            .Should().Equal("/activities/10", "/activities/21", "/activities/30");
        waiting.QuerySelectorAll("li.needs-you-row").Last().QuerySelectorAll("p.needs-you-why").Select(Text)
            .Should().Equal("With Fatima Khumalo", "Waiting less than a day");
        cut.Markup.Should().NotContainAny(["Quick action", "Stalled requests", "Start an MSF campaign"]);
    }

    [Fact]
    public void NothingFiled_Empty_KeepsItsRuleLineAndItsFoot()
    {
        // k1, k2 (Step 2.32: admitted this week, so nobody yet, E5).
        var cut = RenderDashboard<CoordinatorDashboard>(WombatRoles.Coordinator, Coordinator());

        var filed = Section(cut, "card-nothing-filed");
        filed.QuerySelectorAll(".badge").Should().BeEmpty();
        Text(filed.QuerySelector("p.needs-you-rule")!).Should().Be(
            "Current registrars with nothing filed (a draft is not filed) in the last 30 days. A registrar admitted less than 30 " +
            "days ago is not listed.");
        Text(filed.QuerySelector("p.card-empty")!).Should().Be("Every current registrar has filed something in the last 30 days.");
        var foot = filed.QuerySelector(".dashboard-card-footer a")!;
        foot.GetAttribute("href").Should().Be("/programme/trainees?filed=true");
        Text(foot).Should().Be("Open in Programme trainees");
    }

    [Fact]
    public void NothingFiled_WithRows_EachALinkToTheRegistrar_OverWhenLastFiled()
    {
        // k4: the count badge in words, in the draft tone.
        var cut = RenderDashboard<CoordinatorDashboard>(WombatRoles.Coordinator, Coordinator(
            nothingFiled: Card([Registrar("Lerato Molefe", 4, lastFiled: new DateOnly(2026, 9, 2), profileId: 103)])));

        var filed = Section(cut, "card-nothing-filed");
        var badge = filed.QuerySelector("h2 .badge")!;
        badge.TextContent.Should().Be("1 registrar");
        badge.ClassList.Should().Contain("badge-draft");
        var row = filed.QuerySelector("li")!;
        row.QuerySelector("a.progress-row-link")!.GetAttribute("href").Should().Be("/programme/trainees/103");
        Text(row.QuerySelector(".progress-row-meta")!).Should().Be("Last filed 2026-09-02 · Training year 4");
        row.QuerySelectorAll(".dashboard-metric").Should().BeEmpty("the card is one column: no figures");
    }

    [Fact]
    public void NothingFiled_PastFive_SaysHowManyMore()
    {
        var cut = RenderDashboard<CoordinatorDashboard>(WombatRoles.Coordinator, Coordinator(nothingFiled: Card(Typical(), total: 6)));

        Text(Section(cut, "card-nothing-filed").QuerySelector("p.waiting-more")!).Should().Be("1 more in Programme trainees.");
    }

    [Fact]
    public void AnInvitationNearingExpiry_ItsRoleByItsLabel_ItsDayIso_AndTheCardSaysWhyItOffersNothing()
    {
        // k4: "expiring@kgk.wombat.local · Trainee", "expires 2026-10-07" (D+3).
        var cut = RenderDashboard<CoordinatorDashboard>(WombatRoles.Coordinator, Coordinator(
            invitations: [Invitation("member@kgk.wombat.local", WombatRoles.CommitteeMember, "Committee member")]));

        var invitations = Section(cut, "card-invitations");
        invitations.ClassList.Should().Contain("dashboard-span-3");
        Text(invitations.QuerySelector("p.needs-you-rule")!).Should().Be(
            "Expiring in the next 3 days. Only an institutional admin can resend an invitation.");
        var row = invitations.QuerySelector("li.list-row")!;
        Text(row).Should().Be("member@kgk.wombat.local · Committee member expires 2026-10-07");
        row.QuerySelectorAll("a, button").Should().BeEmpty();
        invitations.QuerySelectorAll(".dashboard-card-footer").Should().BeEmpty();
        Text(invitations).Should().NotContain("CommitteeMember", "a role's label, never its key (Q10)");
    }

    [Fact]
    public void NoInvitation_TheCardSaysSo_WithNoRuleLine()
    {
        var cut = RenderDashboard<CoordinatorDashboard>(WombatRoles.Coordinator, Coordinator());

        var invitations = Section(cut, "card-invitations");
        invitations.ClassList.Should().NotContain("dashboard-span-3");
        Text(invitations.QuerySelector("p.card-empty")!).Should().Be("No invitations expiring soon.");
        invitations.QuerySelectorAll("p.needs-you-rule").Should().BeEmpty();
    }

    [Fact]
    public void NothingWaiting_TheEmphasisStripe_AndTheFootKept()
    {
        // k2.
        var cut = RenderDashboard<CoordinatorDashboard>(WombatRoles.Coordinator, Coordinator(
            waiting: Waiting(Kgk, []), nothingFiled: RegistrarsCardDto.Empty));

        var waiting = Section(cut, "card-waiting-assessors");
        waiting.ClassList.Should().Contain("detail-card--emphasis");
        Text(waiting.QuerySelector("p.card-empty")!).Should().Be("Nothing is waiting for an assessor.");
        waiting.QuerySelector(".dashboard-card-footer a")!.GetAttribute("href").Should().Be("/programme/waiting");
    }

    [Fact]
    public void NoOldWord_NoDayMonthDate_NoPronoun()
    {
        var cut = RenderDashboard<CoordinatorDashboard>(WombatRoles.Coordinator, Coordinator(
            nothingFiled: Card([Registrar("Lerato Molefe", 4, lastFiled: new DateOnly(2026, 9, 2))]),
            invitations: [Invitation()]));

        var text = Text(cut.Find(".dashboard-grid"));
        text.Should().NotContainAny("inactive", "Stalled", "(Trainee)", " Oct", " Sep");
        Programme.ProgrammeWordsFixtures.NamesAPersonByPronoun(text).Should().BeFalse();
    }

    [Fact]
    public void Loading_ThreeTitlesOverSkeletons()
    {
        // k5 (the header action is Home's, there from the start: HomeFrameTests).
        var cut = RenderDashboard<CoordinatorDashboard>(WombatRoles.Coordinator, null, Reads.Hang);

        Titles(cut).Should().Equal("Waiting for assessors", "Nothing filed in 30 days", "Invitations nearing expiry");
        cut.FindAll(".dashboard-grid a, .dashboard-grid .badge").Should().BeEmpty();
    }

    [Fact]
    public void ALoadError_IsTheFramesOneAlert()
    {
        // k6.
        var cut = RenderDashboard<CoordinatorDashboard>(WombatRoles.Coordinator, null, Reads.Throw);

        cut.FindAll(".alert-danger").Should().ContainSingle();
        cut.FindAll(".detail-card").Should().BeEmpty();
    }
}
