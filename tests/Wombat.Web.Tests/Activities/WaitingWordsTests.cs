using FluentAssertions;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Programme;
using Wombat.Application.Features.Programme.Waiting;
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

    // ---- T358 (flow 06; E3, E4, E6): the staff reading, Waiting for assessors ------------------------------------------

    [Fact]
    public void TheStaffRow_SaysWhomItWaitsWith_OnALineOfItsOwn()
    {
        var row = ActivityRows.Waiting(1, waitedDays: 8) with
        {
            Holder = new ActivityHolderDto(ActivityHolderKind.Person, "patel", "Mohammed Patel", false, null)
        };

        WaitingWords.With(row).Should().Be("With Mohammed Patel");
        WaitingWords.Waited(row).Should().Be("Waiting 8 days", "flow 04's words stand (E6)");
    }

    [Fact]
    public void TheStaffRules_AddTheNudge_AndThePageAddsWhatRestartsTheWait()
    {
        WaitingWords.StaffRuleLine(7, 5).Should().Be(
            "Oldest first. Overdue once it has waited 7 days. Its assessor is emailed after 5.");
        WaitingWords.PageRuleLine(7, 5).Should().Be(
            "Oldest first. Overdue once it has waited 7 days. Its assessor is emailed after 5. Waiting counts from the last " +
            "move: any save restarts it.");
        WaitingWords.RuleLine(7).Should().Be("Oldest first. Overdue once it has waited 7 days.", "flow 04's rule is unchanged");
    }

    [Fact]
    public void TheStaffCount_IsTheMatch_InFlow04sWords_WithTheNomineeWhenOneIsAsked()
    {
        WaitingWords.StaffCount(Staff(match: 3, matchOverdue: 2)).Should().Be("3 waiting, 2 overdue");
        WaitingWords.StaffCount(Staff(match: 2, matchOverdue: 0)).Should().Be("2 waiting");
        WaitingWords.StaffCount(Staff(match: 1, matchOverdue: 1, withUserId: "patel")).Should().Be("1 waiting, 1 overdue, with Mohammed Patel");
        WaitingWords.NoMatchHeading(Staff(match: 0, matchOverdue: 0, total: 2)).Should().Be("0 of 2 waiting");
    }

    [Theory]
    [InlineData(1, "1 more waits in Waiting for assessors.")]
    [InlineData(9, "9 more wait in Waiting for assessors.")]
    public void HomesOverflow_HasItsSingular(int beyond, string words) => WaitingWords.StaffMore(beyond).Should().Be(words);

    /// <summary>E4: the subtitle says what was read, as which role.</summary>
    [Fact]
    public void TheSubtitle_AndTheEmptyWords_NameTheScopeReadAndTheRole()
    {
        var hospital = new ProgrammeScopeDto("Coordinator", ProgrammeScopeKind.Institution, "Kgosi Kgari Teaching Hospital", 10, [], []);
        var paediatrics = new ProgrammeScopeDto("SpecialityAdmin", ProgrammeScopeKind.Speciality, "Paediatrics", 10, [100], [1000]);
        var sub = new ProgrammeScopeDto("SubSpecialityAdmin", ProgrammeScopeKind.SubSpeciality, "Paediatrics", 10, [], [1000]);

        WaitingWords.StaffSubtitle(hospital).Should().Be(
            "Requests at Kgosi Kgari Teaching Hospital whose next move names an assessor, supervisor or reviewer, read as " +
            "Coordinator. Your own requests are not listed.");
        WaitingWords.StaffSubtitle(paediatrics).Should().Be(
            "Requests in Paediatrics whose next move names an assessor, supervisor or reviewer, read as Speciality admin. " +
            "Your own requests are not listed.");
        WaitingWords.StaffSubtitle(sub).Should().Contain("read as Sub-speciality admin.");

        WaitingWords.NothingWaitingCard.Should().Be("Nothing is waiting for an assessor.");
        WaitingWords.NothingWaitingTitle.Should().Be("Nothing is waiting");
        WaitingWords.NothingWaitingBody(hospital).Should().Be(
            "Nothing at Kgosi Kgari Teaching Hospital is waiting for an assessor, supervisor or reviewer.");
        WaitingWords.NothingWaitingBody(paediatrics).Should().Be(
            "Nothing in Paediatrics is waiting for an assessor, supervisor or reviewer.");
    }

    [Fact]
    public void TheFiltersWords_NoMatch_WhatWasAsked_AndTheCaption()
    {
        WaitingWords.NoMatch.Should().Be("No request matches these filters.");
        WaitingWords.Asked(true, "Fatima Khumalo").Should().Be("Overdue only, with Fatima Khumalo.");
        WaitingWords.Asked(true, null).Should().Be("Overdue only.");
        WaitingWords.Asked(false, "Fatima Khumalo").Should().Be("With Fatima Khumalo.");
        WaitingWords.Asked(false, null).Should().BeEmpty();
        WaitingWords.TableCaption(null).Should().Be("Requests waiting for a named assessor, oldest first");
        WaitingWords.TableCaption("Mohammed Patel").Should().Be("Requests waiting for Mohammed Patel, oldest first");
        (WaitingWords.ShowLabel, WaitingWords.ShowAll, WaitingWords.ShowOverdue, WaitingWords.WithLabel, WaitingWords.WithAnyone)
            .Should().Be(("Waiting", "All", "Overdue only", "With", "Anyone"));
        WaitingWords.PageLoading.Should().Be("Loading Waiting for assessors.");
        WaitingWords.PageLoadFailed.Should().Be(
            "Could not load Waiting for assessors. Nothing has changed. Try again, or come back in a few minutes.");
        WaitingWords.OpenPage.Should().Be("Open Waiting for assessors");
        WaitingWords.NomineeNameOf(Staff(1, 0), "patel").Should().Be("Mohammed Patel");
        WaitingWords.NomineeNameOf(Staff(1, 0), "nobody").Should().BeNull();
    }

    private static WaitingForAssessorsDto Staff(int match, int matchOverdue, int? total = null, string? withUserId = null)
        => new(
            new ProgrammeScopeDto("Coordinator", ProgrammeScopeKind.Institution, "Kgosi Kgari Teaching Hospital", 10, [], []),
            [],
            match,
            matchOverdue,
            total ?? match,
            matchOverdue,
            [new NomineeOptionDto("khumalo", "Fatima Khumalo"), new NomineeOptionDto("patel", "Mohammed Patel")],
            7,
            5,
            1,
            20,
            true)
        {
            Filter = new WaitingForAssessorsFilter(WithUserId: withUserId)
        };

    private static WaitingForYouDto Waiting(params ActivitySummaryDto[] rows)
        => new(rows, rows.Count(row => row.IsOverdue), 7);
}
