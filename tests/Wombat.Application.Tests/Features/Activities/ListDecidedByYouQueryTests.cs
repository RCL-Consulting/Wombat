using FluentAssertions;
using Wombat.Application.Features.Activities.Services;
using Wombat.Domain.Activities;
using Wombat.Tests.Shared;
using static Wombat.Application.Tests.TestHelpers.AssessorReads;

namespace Wombat.Application.Tests.Features.Activities;

/// <summary>
/// "Decided by you" (T350, round 1, Q1; note 6): what the caller moved last that is finished or has no move left, by each
/// pinned workflow (T297), newest first, a page at a time, the total counted over every decision before the page is cut.
/// The Assessor's Home "Recent decisions" is its first page at five (<c>AssessorDashboardQueryTests</c>).
/// </summary>
public sealed class ListDecidedByYouQueryTests
{
    private static readonly DateTime Now = DateTime.UtcNow;

    /// <summary>
    /// A decision is an activity the caller moved last and that is finished or has no move left (T297). One he returned
    /// is back with its author, so it is not his decision; nor is one the trainee withdrew after he had moved it. A dead
    /// end of any name is.
    /// </summary>
    [Fact]
    public async Task AReturnToDraftIsNotADecision_ADeadEndOfAnyNameIs_AndTheLastMoveMustBeHis()
    {
        await using var db = CreateDb();
        SeedCpsaTypes(db);
        AddFiled(db, 51, CpsaMiniCexTypeId, "completed", TraineeId, NamesTheAssessor, Now.AddDays(-1),
            Move("submit", "draft", "requested", TraineeId, Now.AddDays(-2)),
            Move("complete", "requested", "completed", AssessorId, Now.AddDays(-1)));
        AddFiled(db, 52, CpsaMiniCexTypeId, "declined", TraineeId, NamesTheAssessor, Now.AddHours(-3),
            Move("submit", "draft", "requested", TraineeId, Now.AddDays(-2)),
            Move("decline", "requested", "declined", AssessorId, Now.AddHours(-3)));
        // Returned by him: back in draft with the trainee, a move left.
        AddFiled(db, 53, CpsaReflectiveTypeId, "draft", TraineeId, NamesTheAssessor, Now.AddHours(-2),
            Move("submit", "draft", "submitted", TraineeId, Now.AddDays(-2)),
            Move("return", "submitted", "draft", AssessorId, Now.AddHours(-2)));
        // Returned by him, then withdrawn by the trainee: the last move is hers.
        AddFiled(db, 54, CpsaReflectiveTypeId, "cancelled", TraineeId, NamesTheAssessor, Now.AddHours(-1),
            Move("submit", "draft", "submitted", TraineeId, Now.AddDays(-2)),
            Move("return", "submitted", "draft", AssessorId, Now.AddHours(-2)),
            Move("cancel", "draft", "cancelled", TraineeId, Now.AddHours(-1)));
        await db.SaveChangesAsync();

        var result = await DecidedAsync(db);

        result.Items.Select(item => (item.Id, item.CurrentStateLabel, item.IsFinished))
            .Should().Equal((52, "Declined", false), (51, "Completed", true));
        result.Items[0].DecidedOn.Should().BeCloseTo(Now.AddHours(-3), TimeSpan.FromSeconds(1));
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task EveryFinishedState_IsADecision_WhateverItIsCalled()
    {
        await using var db = CreateDb();
        SeedTypes(db);
        AddActedOn(db, 1, ReflectiveTypeId, version: 1, "discussed");
        AddActedOn(db, 2, TeachingTypeId, version: 1, "accepted");
        AddActedOn(db, 3, MsfTypeId, version: 1, "recorded");
        AddActedOn(db, 4, WbaTypeId, version: 1, "completed");
        AddActedOn(db, 5, WbaTypeId, version: 1, "declined");
        AddActedOn(db, 6, WbaTypeId, version: 1, "cancelled");
        AddActedOn(db, 7, WbaTypeId, version: 1, "accepted");
        await db.SaveChangesAsync();

        var result = await DecidedAsync(db);

        // A discussed reflective exercise, an accepted teaching session and a recorded MSF row are all finished; the
        // literal "completed" saw none of them. The declined and cancelled requests have no move left; the legacy
        // Mini-CEX's "accepted" has, so it is not a decision.
        result.Items.Select(item => item.Id).Should().Equal(1, 2, 3, 4, 5, 6);
        result.Items.Where(item => item.IsFinished).Select(item => item.Id).Should().Equal(1, 2, 3, 4);
    }

    [Fact]
    public async Task TheDecisions_AreNewestFirst_ThenByTheHigherId()
    {
        await using var db = CreateDb();
        SeedTypes(db);
        var at = Now.AddHours(-5);
        AddDecision(db, 1, Now.AddDays(-2));
        AddDecision(db, 2, at);
        AddDecision(db, 3, at);
        AddDecision(db, 4, Now.AddHours(-1));
        await db.SaveChangesAsync();

        var result = await DecidedAsync(db);

        result.Items.Select(item => item.Id).Should().Equal(4, 3, 2, 1);
        result.Items.Select(item => item.DecidedOn).Should().BeInDescendingOrder();
    }

    /// <summary>
    /// Note 6: the total reads every activity the caller moved last before the page is cut, and the page and its size are
    /// brought within bounds as My activities brings them; a page past the end serves the last.
    /// </summary>
    [Fact]
    public async Task TheTotalCountsEveryDecision_ThePageAndSizeAreClamped_AndAPagePastTheEndIsTheLast()
    {
        await using var db = CreateDb();
        SeedTypes(db);
        for (var id = 1; id <= 25; id++)
        {
            AddDecision(db, id, Now.AddMinutes(-id));
        }

        await db.SaveChangesAsync();

        var second = await DecidedAsync(db, page: 2, pageSize: 10);
        second.Items.Select(item => item.Id).Should().Equal(Enumerable.Range(11, 10));
        (second.Page, second.PageSize, second.TotalCount, second.PageCount).Should().Be((2, 10, 25, 3));

        var past = await DecidedAsync(db, page: 9, pageSize: 10);
        past.Page.Should().Be(3);
        past.Items.Select(item => item.Id).Should().Equal(Enumerable.Range(21, 5));

        var first = await DecidedAsync(db, page: 0, pageSize: 0);
        (first.Page, first.PageSize).Should().Be((1, 1));
        first.Items.Select(item => item.Id).Should().Equal(1);

        (await DecidedAsync(db, pageSize: 1000)).PageSize.Should().Be(DecidedByYou.MaxPageSize);
    }

    /// <summary>
    /// The T297 review: the decisions were found among the fifty activities he moved last, so fifty portfolio reviews he
    /// returned to their trainees, each with a move left, crowded his older decisions off the card. Every activity he
    /// moved last is judged before the page is cut.
    /// </summary>
    [Fact]
    public async Task OlderDecisions_AreNotCrowdedOff_ByMoreRecentMovesThatLeftAMoveToMake()
    {
        await using var db = CreateDb();
        SeedCpsaTypes(db);
        for (var id = 1; id <= 55; id++)
        {
            AddFiled(db, id, CpsaPortfolioReviewTypeId, "draft", TraineeId, NamesTheAssessor, Now.AddMinutes(-id),
                Move("submit", "draft", "submitted", TraineeId, Now.AddDays(-3)),
                Move("return", "submitted", "draft", AssessorId, Now.AddMinutes(-id)));
        }

        AddFiled(db, 1001, CpsaMiniCexTypeId, "declined", TraineeId, NamesTheAssessor, Now.AddDays(-1),
            Move("submit", "draft", "requested", TraineeId, Now.AddDays(-2)),
            Move("decline", "requested", "declined", AssessorId, Now.AddDays(-1)));
        AddFiled(db, 1002, CpsaMiniCexTypeId, "completed", TraineeId, NamesTheAssessor, Now.AddDays(-2),
            Move("submit", "draft", "requested", TraineeId, Now.AddDays(-3)),
            Move("complete", "requested", "completed", AssessorId, Now.AddDays(-2)));
        await db.SaveChangesAsync();

        var result = await DecidedAsync(db, pageSize: 5);

        result.Items.Select(item => item.Id).Should().Equal(
            [1001, 1002], "a returned review has a move left, so it is not a decision, however recent");
        result.TotalCount.Should().Be(2);
    }

    /// <summary>
    /// The credit column is the latest transition that evaluated credit, ties to the later row (T108): the rule every list
    /// reads it by.
    /// </summary>
    [Fact]
    public async Task EachDecision_CarriesTheLatestEvaluatedCredit()
    {
        await using var db = CreateDb();
        SeedTypes(db);
        var credited = Move("complete", "accepted", "completed", AssessorId, Now.AddHours(-1));
        credited.CreditedItemCount = 2;
        var earlier = Move("accept", "requested", "accepted", AssessorId, Now.AddHours(-2));
        earlier.CreditedItemCount = 0;
        AddFiled(db, 1, WbaTypeId, "completed", TraineeId, NamesTheAssessor, Now.AddHours(-1), earlier, credited);
        AddFiled(db, 2, WbaTypeId, "declined", TraineeId, NamesTheAssessor, Now.AddHours(-3),
            Move("decline", "requested", "declined", AssessorId, Now.AddHours(-3)));
        await db.SaveChangesAsync();

        var result = await DecidedAsync(db);

        result.Items.Select(item => (item.Id, item.CreditedItemCount)).Should().Equal((1, 2), (2, (int?)null));
    }

    /// <summary>
    /// The caller's own subject rows are left out (T203): he made the create move on every activity of his own, and a
    /// procedure log is born in its terminal state. Each decision says whose it is, by name, and is named without E7's
    /// nominee, who would be the reader.
    /// </summary>
    [Fact]
    public async Task HisOwnPortfolioIsLeftOut_AndEachDecisionSaysWhoseItIs()
    {
        await using var db = CreateDb();
        SeedTypes(db);
        AddOwn(db, 11, ProcedureLogTypeId, "logged");
        AddOwn(db, 12, WbaTypeId, "completed");
        AddActedOn(db, 1, WbaTypeId, version: 1, "completed");
        AddActedOn(db, 2, WbaTypeId, version: 1, "completed");
        await db.SaveChangesAsync();
        var directory = new FakeUserDirectory((AssessorId, "Mohammed Patel"), (TraineeId, "Thandi Nkosi"));

        var result = await DecidedAsync(db, CreatePrincipal(AssessorId, "Assessor", "Trainee"), directory);

        result.Items.Select(item => item.Id).Should().Equal(1, 2);
        result.Items.Should().AllSatisfy(item =>
        {
            item.SubjectName.Should().Be("Thandi Nkosi");
            item.DisplayName.Should().StartWith("Mini-CEX").And.NotContain("Mohammed Patel");
            item.WaitedDays.Should().BeNull("only the waiting read counts a wait");
        });
        directory.Lookups.Should().ContainSingle("the page's names are read in one lookup");
    }

    /// <summary>
    /// A create is not a decision, whoever makes it (T350 build review, R1): a supervisor who logs a procedure on a
    /// registrar's behalf made the create move, the last move, on a type born terminal, exactly as the registrar would
    /// have (T203). What he moved after the create still counts.
    /// </summary>
    [Fact]
    public async Task ACreateOnSomeoneElsesBehalf_IsNotADecision_EvenOnATypeBornTerminal()
    {
        await using var db = CreateDb();
        SeedTypes(db);
        var logged = AddFiled(db, 61, ProcedureLogTypeId, "logged", TraineeId, NamesTheAssessor, Now.AddHours(-1),
            Move(Wombat.Domain.Activities.Workflow.Workflow.CreateTransitionKey, "logged", "logged", AssessorId, Now.AddHours(-1)));
        logged.CreatedByUserId = AssessorId;
        AddDecision(db, 62, Now.AddHours(-2));
        await db.SaveChangesAsync();

        var result = await DecidedAsync(db);

        result.Items.Select(item => item.Id).Should().Equal(62);
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task NothingDecided_IsAnEmptyFirstPage()
    {
        await using var db = CreateDb();
        SeedTypes(db);
        await db.SaveChangesAsync();

        var result = await DecidedAsync(db);

        result.Items.Should().BeEmpty();
        (result.Page, result.TotalCount, result.PageCount).Should().Be((1, 0, 1));
    }

    /// <summary>A legacy Mini-CEX the assessor completed at <paramref name="decidedOn" />.</summary>
    private static void AddDecision(Wombat.Infrastructure.Persistence.ApplicationDbContext db, int id, DateTime decidedOn)
        => AddFiled(db, id, WbaTypeId, "completed", TraineeId, NamesTheAssessor, decidedOn,
            Move("complete", "accepted", "completed", AssessorId, decidedOn));
}
