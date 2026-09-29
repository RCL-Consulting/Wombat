using FluentAssertions;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Web.Components.Pages.CommitteeDecisions;
using Wombat.Web.Components.Shared;
using Wombat.Web.Components.Shared.Activities;

namespace Wombat.Web.Tests.Accessibility;

/// <summary>
/// T239: a list's row actions name their rows, and no two on one list read the same. <see cref="RowNames" /> keeps a
/// row's own name where no other row has it, and adds a tie-breaker only where two rows would otherwise share one.
/// </summary>
public sealed class RowNamesTests
{
    [Fact]
    public void ANameNoOtherRowHas_IsKeptPlain_AndASharedOne_TakesTheTieBreakers_InTurn()
    {
        Row[] rows = [new(1, "Thandi Nkosi", "a@x"), new(2, "Thandi Nkosi", "b@x"), new(3, "Sam Smit", "c@x")];

        var names = RowNames.Distinct(rows, row => row.Id, row => row.Name, row => $" ({row.Detail})", row => $" #{row.Id}");

        names.Should().BeEquivalentTo(new Dictionary<int, string>
        {
            [1] = "Thandi Nkosi (a@x)",
            [2] = "Thandi Nkosi (b@x)",
            [3] = "Sam Smit"
        }, "the second tie-breaker is not needed once the first has made every name different");
    }

    [Fact]
    public void ANameTheFirstTieBreakerLeavesShared_TakesTheNext()
    {
        Row[] rows = [new(1, "Edit", "d1"), new(2, "Edit", "d1"), new(3, "Edit", "d2")];

        var names = RowNames.Distinct(rows, row => row.Id, row => row.Name, row => $", {row.Detail}", row => $", #{row.Id}");

        names.Values.Should().OnlyHaveUniqueItems();
        names.Should().BeEquivalentTo(new Dictionary<int, string>
        {
            [1] = "Edit, d1, #1",
            [2] = "Edit, d1, #2",
            [3] = "Edit, d2"
        });
    }

    [Fact]
    public void ATieBreakerThatSaysTheSameOfEveryRowSharingAName_IsNotAdded()
    {
        // Rows 1 and 2 share "Edit" and a detail; rows 3 and 4 share "Open" and differ in it. The detail tells 1 and 2
        // nothing, so they go straight to the next tie-breaker.
        Row[] rows = [new(1, "Edit", "d1"), new(2, "Edit", "d1"), new(3, "Open", "d1"), new(4, "Open", "d2")];

        var names = RowNames.Distinct(rows, row => row.Id, row => row.Name, row => $", {row.Detail}", row => $", #{row.Id}");

        names.Should().BeEquivalentTo(new Dictionary<int, string>
        {
            [1] = "Edit, #1",
            [2] = "Edit, #2",
            [3] = "Open, d1",
            [4] = "Open, d2"
        });
    }

    [Fact]
    public void RowsNoTieBreakerTellsApart_AreNumberedInListOrder_SoNoTwoNamesAreTheSame()
    {
        // T239 review: a last tie-breaker that is not a key (a panel's scope, a time to the minute) can leave rows alike.
        Row[] rows = [new(1, "CCC", "d1"), new(2, "Other", "d1"), new(3, "CCC", "d1"), new(4, "CCC", "d2"), new(5, "CCC", "d1")];

        var names = RowNames.Distinct(rows, row => row.Id, row => row.Name, row => $" ({row.Detail})");

        names.Values.Should().OnlyHaveUniqueItems();
        names.Should().BeEquivalentTo(new Dictionary<int, string>
        {
            [1] = "CCC (d1) (1 of 3)",
            [2] = "Other",
            [3] = "CCC (d1) (2 of 3)",
            [4] = "CCC (d2)",
            [5] = "CCC (d1) (3 of 3)"
        });
    }

    [Fact]
    public void AReview_IsNamedByItsPeriodAndPanel_AndByItsTrainee_OnAListOfMany()
    {
        var mine = ReviewRowNames.For([Review(1, "General CCC", semester: 2)], withTrainee: false);
        var schedule = ReviewRowNames.For([Review(1, "General CCC", semester: 2) with { TraineeName = "Thandi Nkosi" }], withTrainee: true);

        mine[1].Should().Be("the 2026 S2 review before General CCC");
        schedule[1].Should().Be("the 2026 S2 review of Thandi Nkosi before General CCC");
    }

    [Fact]
    public void ReviewsThatReadTheSame_AreToldApart_ByFormative_ThenTheDayScheduled_ThenTheirNumber()
    {
        // A formative review beside the binding one for the same period and panel; a second binding review scheduled once
        // the first was ratified; and two formative reviews scheduled for the same day, whose day tells them nothing.
        var names = ReviewRowNames.For(
        [
            Review(10, "General CCC", semester: 2, scheduledOn: new DateOnly(2026, 11, 2)),
            Review(11, "General CCC", semester: 2, scheduledOn: new DateOnly(2026, 12, 7)),
            Review(12, "General CCC", semester: 2, scheduledOn: new DateOnly(2026, 9, 1), formative: true),
            Review(13, "General CCC", semester: 2, scheduledOn: new DateOnly(2026, 9, 1), formative: true),
            Review(14, "Neonatal CCC", semester: 2)
        ], withTrainee: false);

        names.Values.Should().OnlyHaveUniqueItems();
        names[10].Should().Be("the 2026 S2 review before General CCC, scheduled 2026-11-02");
        names[11].Should().Be("the 2026 S2 review before General CCC, scheduled 2026-12-07");
        names[12].Should().Be("the 2026 S2 formative review before General CCC, review #12");
        names[13].Should().Be("the 2026 S2 formative review before General CCC, review #13");
        names[14].Should().Be("the 2026 S2 review before Neonatal CCC");
    }

    [Fact]
    public void ReviewsOfOnePeriodBeforeOnePanel_AreToldApart_FirstByTheTypeTheirRowShows()
    {
        // T239 review: an entrustment-only review scheduled once the annual one was ratified. The Type column tells the two
        // apart on the trainee's list, which shows no Scheduled column; so the name does the same.
        var names = ReviewRowNames.For(
        [
            Review(20, "General CCC", semester: 2, scheduledOn: new DateOnly(2026, 11, 2)),
            Review(21, "General CCC", semester: 2, scheduledOn: new DateOnly(2027, 1, 8)) with
            {
                ReviewType = CommitteeReviewType.EntrustmentOnly
            }
        ], withTrainee: false);

        names[20].Should().Be("the 2026 S2 review before General CCC, Annual progression");
        names[21].Should().Be("the 2026 S2 review before General CCC, Entrustment only");
    }

    [Fact]
    public void AnActivity_IsNamedByItsType_ItsSubjectInAnInbox_ItsEpa_AndItsEncounterDate()
    {
        var declared = Activity(1, "PAED-003", new DateOnly(2026, 9, 1), declared: true);
        var undeclared = Activity(2, null, new DateOnly(2026, 9, 2), declared: false);

        var inbox = ActivityRowNames.For([declared, undeclared], withSubject: true);
        var mine = ActivityRowNames.For([declared, undeclared], withSubject: false);

        inbox[1].Should().Be("Mini-CEX for Thandi Nkosi, PAED-003, encounter date 2026-09-01");
        inbox[2].Should().Be("Mini-CEX for Thandi Nkosi, encounter date not recorded (created 2026-09-02)");
        mine[1].Should().Be("Mini-CEX, PAED-003, encounter date 2026-09-01");
    }

    [Fact]
    public void ActivitiesThatReadTheSame_AreToldApart_ByTheirState_AsTheStateColumnSaysIt()
    {
        var first = Activity(1, "PAED-003", new DateOnly(2026, 9, 1), declared: true);
        var second = first with { Id = 2, CurrentState = "draft", CurrentStateLabel = "Draft" };

        var mine = ActivityRowNames.For([first, second], withSubject: false);
        var inbox = ActivityRowNames.For([first, second], withSubject: true);

        mine[1].Should().Be("Mini-CEX, PAED-003, encounter date 2026-09-01, Submitted");
        mine[2].Should().Be("Mini-CEX, PAED-003, encounter date 2026-09-01, Draft");
        inbox[2].Should().Be("Mini-CEX for Thandi Nkosi, PAED-003, encounter date 2026-09-01, Draft");
    }

    [Fact]
    public void InAnInbox_ActivitiesAlikeInState_AreToldApart_ByWhenEachWasUpdated()
    {
        var first = Activity(1, "PAED-003", new DateOnly(2026, 9, 1), declared: true) with
        {
            UpdatedOn = new DateTime(2026, 9, 1, 8, 0, 0, DateTimeKind.Utc)
        };
        var second = first with { Id = 2, UpdatedOn = new DateTime(2026, 9, 1, 9, 30, 0, DateTimeKind.Utc) };

        var names = ActivityRowNames.For([first, second], withSubject: true);

        names.Values.Should().OnlyHaveUniqueItems();
        names[1].Should().StartWith("Mini-CEX for Thandi Nkosi, PAED-003, encounter date 2026-09-01, updated ");
    }

    [Fact]
    public void ActivitiesAlikeInEveryColumn_AreNumbered_AndTheSubjectsOwnListNamesNoUpdatedColumn()
    {
        // T239 review: two drafts saved in the same minute read the same in every column, the inbox's Updated too. The
        // subject's own list has no Updated column, so it names none, however the times differ.
        var first = Activity(1, "PAED-003", new DateOnly(2026, 9, 1), declared: true);
        var sameMinute = first with { Id = 2, UpdatedOn = first.UpdatedOn.AddSeconds(20) };
        var later = first with { Id = 3, UpdatedOn = first.UpdatedOn.AddHours(2) };

        var inbox = ActivityRowNames.For([first, sameMinute], withSubject: true);
        var mine = ActivityRowNames.For([first, later], withSubject: false);

        inbox.Values.Should().OnlyHaveUniqueItems();
        inbox[1].Should().StartWith("Mini-CEX for Thandi Nkosi, PAED-003, encounter date 2026-09-01").And.EndWith(" (1 of 2)");
        inbox[2].Should().EndWith(" (2 of 2)");
        mine[1].Should().Be("Mini-CEX, PAED-003, encounter date 2026-09-01 (1 of 2)");
        mine[3].Should().Be("Mini-CEX, PAED-003, encounter date 2026-09-01 (2 of 2)");
    }

    /// <summary>
    /// T342 (flow 03; T280, A16): an activity link's name is its words, the name, ", " and its second line; two alike add
    /// their state, then a number. One activity listed twice on a page, in Needs you and in All activities, has one name.
    /// </summary>
    [Fact]
    public void ActivityLinks_AreNamedByTheirWords_AndTwoAlikeAreToldApart()
    {
        var draft = TestSupport.ActivityRows.Row(1);
        var cancelled = TestSupport.ActivityRows.Row(2, "cancelled", "Cancelled");
        var reflection = TestSupport.ActivityRows.Returned(3);
        var twin = TestSupport.ActivityRows.Row(4);

        var names = ActivityRowNames.Links([draft, reflection, draft, cancelled, reflection, twin]);

        names.Should().HaveCount(4, "an activity in both lists is one link, one name");
        names[3].Should().Be("Reflective Exercise (Paediatrics) · PAED-001 · 2026-09-09, with Sarah Botha");
        ActivityRowNames.LinkWords(reflection).Should().Be(names[3], "a name no other link has is the link's own words");
        names[2].Should().Be("Mini-CEX (Paediatrics) · PAED-003 · 2026-09-25, to David Naidoo, Cancelled");
        names[1].Should().Be("Mini-CEX (Paediatrics) · PAED-003 · 2026-09-25, to David Naidoo, Draft (1 of 2)");
        names[4].Should().Be("Mini-CEX (Paediatrics) · PAED-003 · 2026-09-25, to David Naidoo, Draft (2 of 2)");
    }

    private sealed record Row(int Id, string Name, string Detail);

    private static CommitteeReviewListItemDto Review(
        int id, string panel, int semester, DateOnly? scheduledOn = null, bool formative = false)
        => new(
            id, "trainee-1", 20, panel, new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31),
            scheduledOn ?? new DateOnly(2026, 11, 2), CommitteeReviewState.Ratified, null, null, formative)
        {
            AcademicYear = 2026,
            Semester = semester
        };

    private static ActivitySummaryDto Activity(int id, string? epaCode, DateOnly observedOn, bool declared)
        => new(
            id, 5, "mini_cex_cpsa", "Mini-CEX", "trainee-1", "submitted", "Submitted",
            new DateTime(2026, 9, 1, 8, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 1, 8, 0, 0, DateTimeKind.Utc),
            epaCode is null ? null : 3, epaCode, epaCode is null ? null : "Title", epaCode is null ? null : true,
            observedOn, declared, null)
        {
            SubjectName = "Thandi Nkosi"
        };
}
