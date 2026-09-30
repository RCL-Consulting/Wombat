using FluentAssertions;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Web.Components.Shared.Activities;
using Wombat.Web.Tests.TestSupport;

namespace Wombat.Web.Tests.Activities;

/// <summary>
/// T350, flow 04 (R3-Spec § 1, § 7): every phrase that says what waits on an assessor, in one place, as the boards word
/// it.
/// </summary>
public sealed class WaitingWordsTests
{
    [Theory]
    [InlineData(0, "Waiting less than a day", "Less than a day")]
    [InlineData(1, "Waiting 1 day", "1 day")]
    [InlineData(8, "Waiting 8 days", "8 days")]
    public void AWait_IsWholeDays_AndUnderADayIsSaid(int days, string waited, string cell)
    {
        var row = ActivityRows.Waiting(1, waitedDays: days);

        WaitingWords.Waited(row).Should().Be(waited);
        WaitingWords.WaitedCell(row).Should().Be(cell);
    }

    [Fact]
    public void Since_IsSouthAfricanTime_WithTheZone()
    {
        // 06:06 UTC is 08:06 in South Africa; 22:30 UTC on the 21st is already the 22nd.
        var row = ActivityRows.Waiting(1, waitedDays: 8, since: new DateTime(2026, 9, 22, 6, 6, 0, DateTimeKind.Utc));

        WaitingWords.Since(row).Should().Be("since 2026-09-22 08:06 SAST");
        WaitingWords.WaitedSince(row).Should().Be("Waiting 8 days, since 2026-09-22 08:06 SAST.");
        WaitingWords.Since(ActivityRows.Waiting(2, since: new DateTime(2026, 9, 21, 22, 30, 0, DateTimeKind.Utc)))
            .Should().Be("since 2026-09-22 00:30 SAST");
    }

    [Fact]
    public void TheCount_SaysTheOverdueOnlyWhenAnyIs()
    {
        WaitingWords.Count(Waiting(ActivityRows.Waiting(1), ActivityRows.Waiting(2))).Should().Be("2 waiting");
        WaitingWords.Count(Waiting(ActivityRows.Waiting(1, waitedDays: 8, overdue: true), ActivityRows.Waiting(2)))
            .Should().Be("2 waiting, 1 overdue");
    }

    [Fact]
    public void TheRuleLines_AndTheDecidedCount()
    {
        WaitingWords.RuleLine(7).Should().Be("Oldest first. Overdue once it has waited 7 days.");
        WaitingWords.RuleLine(1).Should().Be("Oldest first. Overdue once it has waited 1 day.");
        WaitingWords.DecidedRule.Should().Be("Newest first. Everything you completed, declined, discussed or signed off.");
        WaitingWords.DecidedCount(1).Should().Be("1 decision");
        WaitingWords.DecidedCount(45).Should().Be("45 decisions");
    }

    [Fact]
    public void TheOverflow_AndTheResultsTail()
    {
        WaitingWords.More(1).Should().Be("1 more waits in the Activity inbox.");
        WaitingWords.More(20).Should().Be("20 more wait in the Activity inbox.");
        WaitingWords.MoreForYou(1).Should().Be("1 more waits for you.");
        WaitingWords.MoreForYou(2).Should().Be("2 more wait for you.");
        WaitingWords.MoreForYou(0).Should().Be("Nothing else waits for you.");
    }

    /// <summary>E4: the three wordings, without the role; the name and registrar from the oldest, its wait lower-cased.</summary>
    [Fact]
    public void TheOtherRoleLine_HasItsThreeWordings_AndNoneWhenNothingWaits()
    {
        var zulu = ActivityRows.Waiting(
            1, subjectName: "Nomsa Mahlangu", waitedDays: 8, overdue: true, epaCode: "PAED-004", observedOn: new DateOnly(2026, 9, 27));
        var today = ActivityRows.Waiting(
            2, subjectName: "Anele Dlamini", epaCode: "PAED-002", observedOn: new DateOnly(2026, 9, 30));

        WaitingWords.OtherRoleLine(Waiting(zulu)).Should().Be(
            "1 activity waits for you in the Activity inbox, and it is overdue: Mini-CEX (Paediatrics) · PAED-004 · " +
            "2026-09-27, from Nomsa Mahlangu, waiting 8 days.");
        WaitingWords.OtherRoleLine(Waiting(zulu, today, ActivityRows.Waiting(3))).Should().Be(
            "3 activities wait for you in the Activity inbox; 1 is overdue. The oldest: Mini-CEX (Paediatrics) · PAED-004 · " +
            "2026-09-27, from Nomsa Mahlangu, waiting 8 days.");
        WaitingWords.OtherRoleLine(Waiting(today)).Should().Be(
            "1 activity waits for you in the Activity inbox: Mini-CEX (Paediatrics) · PAED-002 · 2026-09-30, from Anele " +
            "Dlamini, waiting less than a day.");
        WaitingWords.OtherRoleLine(Waiting(today, ActivityRows.Waiting(3))).Should().Be(
            "2 activities wait for you in the Activity inbox. The oldest: Mini-CEX (Paediatrics) · PAED-002 · 2026-09-30, " +
            "from Anele Dlamini, waiting less than a day.");
        WaitingWords.OtherRoleLine(Waiting(zulu, zulu with { Id = 4 })).Should().Contain("; 2 are overdue.");
        WaitingWords.OtherRoleLine(Waiting()).Should().BeNull();

        WaitingWords.OtherRoleAction(Waiting(zulu)).Should().Be("Open it");
        WaitingWords.OtherRoleAction(Waiting(zulu, today)).Should().Be("Open the oldest");
    }

    [Fact]
    public void AnAssessorsSecondLine_SaysWhoseItIs()
    {
        ActivityListWords.FromLine(ActivityRows.Waiting(1, subjectName: "Anele Dlamini")).Should().Be("from Anele Dlamini");
        ActivityListWords.FromLine(ActivityRows.Waiting(1) with { SubjectName = null }).Should().BeNull();
    }

    private static WaitingForYouDto Waiting(params ActivitySummaryDto[] rows)
        => new(rows, rows.Count(row => row.IsOverdue), 7);
}
