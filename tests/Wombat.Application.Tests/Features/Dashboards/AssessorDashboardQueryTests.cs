using System.Security.Claims;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Wombat.Application.Common.Options;
using Wombat.Application.Features.Dashboards.Assessor;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Activities;
using Wombat.Infrastructure.Activities;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;
using static Wombat.Application.Tests.TestHelpers.AssessorReads;

namespace Wombat.Application.Tests.Features.Dashboards;

/// <summary>
/// The assessor dashboard reads what waits on the caller, and what the caller decided, from each activity's PINNED
/// workflow (T297), as it reads "finished" (T203, D44): never from a state's key. Since T350 (notes 5 and 6) both are the
/// Activity inbox's own reads: every waiting row (<c>WaitingForYou</c>), and "Decided by you"'s first page of five
/// (<c>DecidedByYou</c>). The reads' own rules are <c>ListWaitingForYouQueryTests</c>' and
/// <c>ListDecidedByYouQueryTests</c>'; these hold what Home carries of them.
/// </summary>
/// <remarks>
/// Until T297 "Pending requests" counted only activities in a state keyed <c>requested</c> that the assessor had created
/// or already moved. The trainee makes both the create and the submit, so a request naming him never counted, and a
/// portfolio review waits in <c>submitted</c>: Dr Patel read "0 assessments awaiting review" beside an inbox of two.
/// "Accepted, needing action" read a state only the Demo legacy seeds have.
/// </remarks>
public sealed class AssessorDashboardQueryTests
{
    // ---- Waiting for you: the inbox's rows, less the caller's own portfolio ----------------------------------------

    /// <summary>
    /// The Verification's case: a Mini-CEX the trainee filed and submitted naming the assessor, with no move of his, is
    /// waiting on him; so is a portfolio review in <c>submitted</c>; his own draft is not, and neither is a trainee's draft
    /// that names him, which only its author can move.
    /// </summary>
    [Fact]
    public async Task ARequestATraineeFiledAndSubmittedNamingHim_Counts_AsDoesAPortfolioReviewAwaitingHim_ButNotHisOwnDraft()
    {
        await using var db = CreateDb();
        SeedCpsaTypes(db);
        var now = DateTime.UtcNow;
        AddFiled(db, 31, CpsaMiniCexTypeId, "requested", TraineeId, NamesTheAssessor, now.AddDays(-2),
            Move("create", "draft", "draft", TraineeId, now.AddDays(-3)),
            Move("submit", "draft", "requested", TraineeId, now.AddDays(-2)));
        AddFiled(db, 32, CpsaPortfolioReviewTypeId, "submitted", TraineeId, NamesTheAssessor, now.AddDays(-1),
            Move("create", "draft", "draft", TraineeId, now.AddDays(-2)),
            Move("submit", "draft", "submitted", TraineeId, now.AddDays(-1)));
        // His own draft (he is also a trainee): he may submit it, but it is his own portfolio.
        AddFiled(db, 33, CpsaMiniCexTypeId, "draft", AssessorId, """{ "assessor_user_id": "assessor-2" }""", now.AddHours(-5),
            Move("create", "draft", "draft", AssessorId, now.AddHours(-5)));
        // A trainee's draft naming him: nothing he can move until she submits it.
        AddFiled(db, 34, CpsaMiniCexTypeId, "draft", TraineeId, NamesTheAssessor, now.AddHours(-4),
            Move("create", "draft", "draft", TraineeId, now.AddHours(-4)));
        await db.SaveChangesAsync();

        var result = await Handle(db, CreatePrincipal(AssessorId, "Assessor", "Trainee"));

        result.Waiting.Count.Should().Be(2);
        result.Waiting.Items.Select(item => item.Id).Should().Equal([31, 32], "oldest first");
        result.Decisions.Items.Should().BeEmpty("he has moved none of them");
        result.Decisions.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task WhatAwaitsHim_IsListedOldestFirst_ByItsPinnedLabel_AndOverduePastTheDueDays()
    {
        await using var db = CreateDb();
        SeedCpsaTypes(db);
        var now = DateTime.UtcNow;
        AddFiled(db, 41, CpsaReflectiveTypeId, "submitted", TraineeId, NamesTheAssessor, now.AddDays(-1));
        AddFiled(db, 42, CpsaMiniCexTypeId, "requested", "trainee-2", NamesTheAssessor, now.AddDays(-10));
        AddFiled(db, 43, CpsaPortfolioReviewTypeId, "submitted", TraineeId, NamesTheAssessor, now.AddDays(-3));
        await db.SaveChangesAsync();
        var directory = new FakeUserDirectory((TraineeId, "Pieter du Plessis"), ("trainee-2", "Nomsa Mahlangu"));

        var result = await Handle(db, directory: directory);

        result.Waiting.Items.Select(item => (item.Id, item.SubjectName, item.CurrentState, item.CurrentStateLabel, item.IsOverdue))
            .Should().Equal(
                (42, "Nomsa Mahlangu", "requested", "Requested", true),
                (43, "Pieter du Plessis", "submitted", "Awaiting review", false),
                (41, "Pieter du Plessis", "submitted", "Awaiting discussion", false));
        result.Waiting.Items[0].UpdatedOn.Should().BeCloseTo(now.AddDays(-10), TimeSpan.FromSeconds(1));
        result.Waiting.OverdueCount.Should().Be(1);
        result.Waiting.DueDays.Should().Be(7, "the rule line's number is AssessorDueDays (E1)");
    }

    /// <summary>
    /// Home carries every waiting row, not the first ten as the card used to: it lists five and says how many more wait in
    /// the inbox (E5). And "Recent decisions" is the decided read's first page, at five, with the total.
    /// </summary>
    [Fact]
    public async Task Home_CarriesEveryWaitingRow_AndTheDecidedReadsFirstPageOfFive()
    {
        await using var db = CreateDb();
        SeedTypes(db);
        // Twelve requests waiting on him, and seven decisions.
        for (var id = 1; id <= 12; id++)
        {
            AddActedOn(db, id, WbaTypeId, version: 1, "requested");
        }

        for (var id = 21; id <= 27; id++)
        {
            AddActedOn(db, id, WbaTypeId, version: 1, "completed");
        }

        await db.SaveChangesAsync();

        var result = await Handle(db);

        result.Waiting.Count.Should().Be(12);
        result.Decisions.TotalCount.Should().Be(7);
        result.Decisions.PageSize.Should().Be(GetAssessorDashboardSummaryQueryHandler.DecisionsListed);
        result.Decisions.Items.Select(item => item.Id).Should().Equal([21, 22, 23, 24, 25], "newest move first");
    }

    [Fact]
    public async Task AnActivityInATerminalStateOfItsWorkflow_IsADecision_AndNeverWorkWaitingOnHim()
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
        AddActedOn(db, 8, WbaTypeId, version: 1, "requested");
        AddActedOn(db, 9, ReflectiveTypeId, version: 1, "submitted");
        await db.SaveChangesAsync();

        var result = await Handle(db);

        // Six decisions, of which Home shows the newest five (ListDecidedByYouQueryTests holds all six).
        result.Decisions.TotalCount.Should().Be(6);
        result.Decisions.Items.Select(item => item.Id).Should().Equal(1, 2, 3, 4, 5);
        // Each says whether it is finished, by the same test, for its badge (T266 review): the declined request is a
        // decision but not finished work, and the teaching session's "accepted" is.
        result.Decisions.Items.Where(item => item.IsFinished).Select(item => item.Id).Should().BeEquivalentTo([1, 2, 3, 4]);
        // The teaching session finishes in "accepted", so it is not an assessment waiting on him; the legacy Mini-CEX's
        // "accepted" and "requested" and the reflection awaiting discussion are, oldest first.
        result.Waiting.Items.Select(item => item.Id).Should().Equal(9, 8, 7);
    }

    [Fact]
    public async Task FinishedIsReadFromThePinnedVersion_NotTheTypesCurrentWorkflow()
    {
        await using var db = CreateDb();
        SeedTypes(db);
        // Version 1 finishes in "discussed"; version 2, the type's current one, adds a sign-off after it.
        AddActedOn(db, 1, ReflectiveTypeId, version: 1, "discussed");
        AddActedOn(db, 2, ReflectiveTypeId, version: 2, "discussed");
        await db.SaveChangesAsync();

        var result = await Handle(db);

        result.Decisions.Items.Select(item => item.Id).Should().Equal(1);
        result.Waiting.Items.Select(item => item.Id).Should().Equal([2], "on version 2 the sign-off is his to make");
    }

    [Fact]
    public async Task AnAssessorWhoIsAlsoATrainee_SeesNoneOfTheirOwnPortfolio_OnlyWhatTheyDecidedForOthers()
    {
        await using var db = CreateDb();
        SeedTypes(db);
        // The caller's own portfolio: they created each row and made its "create" move. A procedure log is born in its
        // terminal "logged", so without the subject exclusion it would read as the caller's decision the moment it is
        // logged; the requested and accepted Mini-CEX are the caller's own requests, which they may cancel, not
        // assessments for them to do.
        AddOwn(db, 11, ProcedureLogTypeId, "logged");
        AddOwn(db, 12, ReflectiveTypeId, "discussed");
        AddOwn(db, 13, WbaTypeId, "requested");
        AddOwn(db, 14, WbaTypeId, "accepted");
        AddOwn(db, 15, WbaTypeId, "completed");
        // Assessor work: a Mini-CEX the caller completed. And a procedure the caller logged for a trainee, born in its
        // terminal state: a create is not a decision, whoever makes it (T350 build review, R1; until then a create for
        // someone else counted, on a Mini-CEX drawn born "completed", which its workflow cannot do).
        AddActedOn(db, 1, WbaTypeId, version: 1, "completed");
        var now = DateTime.UtcNow;
        db.Activities.Add(new Activity
        {
            Id = 16, ActivityTypeId = ProcedureLogTypeId, SchemaVersion = 1,
            SubjectUserId = TraineeId, CreatedByUserId = AssessorId, CurrentState = "logged", DataJson = "{}",
            CreatedOn = now.AddDays(-2), UpdatedOn = now.AddMinutes(-16),
            Transitions =
            [
                new ActivityTransition
                {
                    Id = 16, ActivityId = 16, FromState = "logged", ToState = "logged", TransitionKey = "create",
                    ActorUserId = AssessorId, OccurredOn = now.AddMinutes(-16)
                }
            ]
        });
        await db.SaveChangesAsync();

        var result = await Handle(db, CreatePrincipal(AssessorId, "Assessor", "Trainee"));

        result.Decisions.Items.Select(item => item.Id).Should().BeEquivalentTo([1]);
        result.Waiting.Items.Should().BeEmpty();
        result.Waiting.Count.Should().Be(0);
    }

    /// <summary>
    /// T220 review: the card printed the key <c>accepted</c>. Each item carries the state's label in the version the
    /// activity is pinned to: activity 1's version 1 renames it, where the type's current version does not.
    /// </summary>
    [Fact]
    public async Task AWaitingAssessment_CarriesItsStateLabel_FromThePinnedVersion()
    {
        await using var db = CreateDb();
        SeedTypes(db);
        db.ActivityTypes.Add(new ActivityType
        {
            Id = RelabelledWbaTypeId, Key = "mini_cex_relabelled", Name = "Mini-CEX (relabelled)",
            Scope = ActivityScope.Global, Version = 2, WorkflowJson = FinishingWorkflows.Wba
        });
        db.Set<ActivityTypeVersion>().AddRange(
            new ActivityTypeVersion
            {
                Id = 13, ActivityTypeId = RelabelledWbaTypeId, Version = 1,
                WorkflowJson = FinishingWorkflows.Wba.Replace(
                    "\"label\": \"Accepted\"", "\"label\": \"Accepted for observation\"", StringComparison.Ordinal)
            },
            new ActivityTypeVersion
            {
                Id = 14, ActivityTypeId = RelabelledWbaTypeId, Version = 2, WorkflowJson = FinishingWorkflows.Wba
            });
        AddActedOn(db, 1, RelabelledWbaTypeId, version: 1, "accepted");
        AddActedOn(db, 2, WbaTypeId, version: 1, "accepted");
        await db.SaveChangesAsync();

        var result = await Handle(db);

        result.Waiting.Items.Select(item => (item.Id, item.CurrentStateLabel))
            .Should().BeEquivalentTo([(1, "Accepted for observation"), (2, "Accepted")]);
    }

    /// <summary>
    /// Each row says whose it is by name, looked up once per read (T142's rule), and by the id only when that user has no
    /// name on record. Until T250 both lists carried the subject's user id in <c>SubjectName</c>.
    /// </summary>
    [Fact]
    public async Task EachRow_NamesItsSubject_InOneLookupPerRead_AndByTheIdOnlyWhenNoNameIsOnRecord()
    {
        await using var db = CreateDb();
        SeedTypes(db);
        AddActedOn(db, 1, WbaTypeId, version: 1, "accepted", subject: TraineeId);
        AddActedOn(db, 2, WbaTypeId, version: 1, "completed", subject: "trainee-2");
        AddActedOn(db, 3, WbaTypeId, version: 1, "completed", subject: TraineeId);
        AddActedOn(db, 4, WbaTypeId, version: 1, "declined", subject: "trainee-gone");
        await db.SaveChangesAsync();
        var directory = new FakeUserDirectory((TraineeId, "Thandi Nkosi"), ("trainee-2", "Sipho Dlamini"));

        var result = await Handle(db, directory: directory);

        result.Waiting.Items.Select(item => (item.Id, item.SubjectName))
            .Should().Equal((1, "Thandi Nkosi"));
        result.Decisions.Items.Select(item => (item.Id, item.SubjectName))
            .Should().BeEquivalentTo([(2, "Sipho Dlamini"), (3, "Thandi Nkosi"), (4, "trainee-gone")]);
        directory.Lookups.Should().HaveCount(2, "one for the waiting rows, one for the decisions");
        directory.Lookups[0].Should().Contain(TraineeId).And.NotContain(["trainee-2", "trainee-gone"]);
        directory.Lookups[1].Should().Contain([TraineeId, "trainee-2", "trainee-gone"]);
    }

    private static async Task<AssessorDashboardSummaryDto> Handle(
        ApplicationDbContext db, ClaimsPrincipal? principal = null, FakeUserDirectory? directory = null)
        => await new GetAssessorDashboardSummaryQueryHandler(
                db, new WorkflowEvaluator(), directory ?? FakeUserDirectory.Empty, Options.Create(new DashboardThresholds()),
                TimeProvider.System)
            .Handle(new GetAssessorDashboardSummaryQuery(principal ?? CreatePrincipal(AssessorId)), CancellationToken.None);
}
