using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Wombat.Application.Common.Options;
using Wombat.Application.Features.Dashboards.Assessor;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Activities;
using Wombat.Infrastructure.Activities;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.Features.Dashboards;

/// <summary>
/// The assessor dashboard reads what waits on the caller, and what the caller decided, from each activity's PINNED
/// workflow (T297), as it reads "finished" (T203, D44): never from a state's key.
/// </summary>
/// <remarks>
/// Until T297 "Pending requests" counted only activities in a state keyed <c>requested</c> that the assessor had created
/// or already moved. The trainee makes both the create and the submit, so a request naming him never counted, and a
/// portfolio review waits in <c>submitted</c>: Dr Patel read "0 assessments awaiting review" beside an inbox of two.
/// "Accepted, needing action" read a state only the Demo legacy seeds have.
/// </remarks>
public sealed class AssessorDashboardQueryTests
{
    private const string AssessorId = "assessor-1";
    private const string TraineeId = "trainee-1";

    private const int ReflectiveTypeId = 1;
    private const int TeachingTypeId = 2;
    private const int WbaTypeId = 3;
    private const int MsfTypeId = 4;
    private const int ProcedureLogTypeId = 5;
    private const int RelabelledWbaTypeId = 6;

    private const int CpsaMiniCexTypeId = 21;
    private const int CpsaPortfolioReviewTypeId = 22;
    private const int CpsaReflectiveTypeId = 23;

    private static readonly string NamesTheAssessor = $$"""{ "assessor_user_id": "{{AssessorId}}" }""";

    // ---- Pending requests: the inbox's rows, less the caller's own portfolio ---------------------------------------

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
        // His own draft (he is also a trainee): he may submit it, so it is in his inbox, but it is his own portfolio.
        AddFiled(db, 33, CpsaMiniCexTypeId, "draft", AssessorId, """{ "assessor_user_id": "assessor-2" }""", now.AddHours(-5),
            Move("create", "draft", "draft", AssessorId, now.AddHours(-5)));
        // A trainee's draft naming him: nothing he can move until she submits it.
        AddFiled(db, 34, CpsaMiniCexTypeId, "draft", TraineeId, NamesTheAssessor, now.AddHours(-4),
            Move("create", "draft", "draft", TraineeId, now.AddHours(-4)));
        await db.SaveChangesAsync();

        var result = await Handle(db, CreatePrincipal(AssessorId, "Assessor", "Trainee"));

        result.PendingRequestCount.Should().Be(2);
        result.AwaitingReview.Select(item => item.ActivityId).Should().Equal([31, 32], "oldest first");
        result.RecentDecisions.Should().BeEmpty("he has moved none of them");
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

        result.AwaitingReview.Select(item => (item.ActivityId, item.SubjectName, item.CurrentState, item.CurrentStateLabel, item.IsOverdue))
            .Should().Equal(
                (42, "Nomsa Mahlangu", "requested", "Requested", true),
                (43, "Pieter du Plessis", "submitted", "Awaiting review", false),
                (41, "Pieter du Plessis", "submitted", "Awaiting discussion", false));
        result.AwaitingReview[0].WaitingSince.Should().BeCloseTo(now.AddDays(-10), TimeSpan.FromSeconds(1));
        result.PendingRequestCount.Should().Be(3);
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

        // A discussed reflective exercise, an accepted teaching session and a recorded MSF row are all finished; the
        // literal "completed" saw none of them. The declined and cancelled requests have no move left.
        result.RecentDecisions.Select(item => item.ActivityId).Should().BeEquivalentTo([1, 2, 3, 4, 5, 6]);
        // Each says whether it is finished, by the same test, for its badge (T266 review): the declined and cancelled
        // requests are decisions but not finished work, and the teaching session's "accepted" is.
        result.RecentDecisions.Where(item => item.IsFinished).Select(item => item.ActivityId).Should().BeEquivalentTo([1, 2, 3, 4]);
        // The teaching session finishes in "accepted", so it is not an assessment waiting on him; the legacy Mini-CEX's
        // "accepted" and "requested" and the reflection awaiting discussion are, oldest first.
        result.AwaitingReview.Select(item => item.ActivityId).Should().Equal(9, 8, 7);
        result.PendingRequestCount.Should().Be(3);
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

        result.RecentDecisions.Select(item => item.ActivityId).Should().Equal(1);
        result.AwaitingReview.Select(item => item.ActivityId).Should().Equal([2], "on version 2 the sign-off is his to make");
    }

    /// <summary>
    /// A decision is an activity the caller moved last and that is finished or has no move left (T297). One he returned
    /// is back with its author, so it is not his decision; nor is one the trainee withdrew after he had moved it.
    /// </summary>
    [Fact]
    public async Task ARecentDecision_IsWhatHeMovedLast_ThatIsFinishedOrHasNoMoveLeft()
    {
        await using var db = CreateDb();
        SeedCpsaTypes(db);
        var now = DateTime.UtcNow;
        AddFiled(db, 51, CpsaMiniCexTypeId, "completed", TraineeId, NamesTheAssessor, now.AddDays(-1),
            Move("submit", "draft", "requested", TraineeId, now.AddDays(-2)),
            Move("complete", "requested", "completed", AssessorId, now.AddDays(-1)));
        AddFiled(db, 52, CpsaMiniCexTypeId, "declined", TraineeId, NamesTheAssessor, now.AddHours(-3),
            Move("submit", "draft", "requested", TraineeId, now.AddDays(-2)),
            Move("decline", "requested", "declined", AssessorId, now.AddHours(-3)));
        // Returned by him: back in draft with the trainee, a move left.
        AddFiled(db, 53, CpsaReflectiveTypeId, "draft", TraineeId, NamesTheAssessor, now.AddHours(-2),
            Move("submit", "draft", "submitted", TraineeId, now.AddDays(-2)),
            Move("return", "submitted", "draft", AssessorId, now.AddHours(-2)));
        // Returned by him, then withdrawn by the trainee: the last move is hers.
        AddFiled(db, 54, CpsaReflectiveTypeId, "cancelled", TraineeId, NamesTheAssessor, now.AddHours(-1),
            Move("submit", "draft", "submitted", TraineeId, now.AddDays(-2)),
            Move("return", "submitted", "draft", AssessorId, now.AddHours(-2)),
            Move("cancel", "draft", "cancelled", TraineeId, now.AddHours(-1)));
        await db.SaveChangesAsync();

        var result = await Handle(db);

        result.RecentDecisions.Select(item => (item.ActivityId, item.FinalStateLabel, item.IsFinished))
            .Should().Equal((52, "Declined", false), (51, "Completed", true));
        result.RecentDecisions[0].DecidedOn.Should().BeCloseTo(now.AddHours(-3), TimeSpan.FromSeconds(1));
    }

    /// <summary>
    /// The T297 review: the decisions were found among the fifty activities he moved last, so fifty portfolio reviews he
    /// returned to their trainees, each with a move left, crowded his older decisions off the card. They are read a page
    /// at a time, newest move first, until the card is full or nothing is left.
    /// </summary>
    [Fact]
    public async Task OlderDecisions_AreNotCrowdedOff_ByMoreRecentMovesThatLeftAMoveToMake()
    {
        await using var db = CreateDb();
        SeedCpsaTypes(db);
        var now = DateTime.UtcNow;
        var returned = GetAssessorDashboardSummaryQueryHandler.LastMovedRead + 5;
        for (var id = 1; id <= returned; id++)
        {
            AddFiled(db, id, CpsaPortfolioReviewTypeId, "draft", TraineeId, NamesTheAssessor, now.AddMinutes(-id),
                Move("submit", "draft", "submitted", TraineeId, now.AddDays(-3)),
                Move("return", "submitted", "draft", AssessorId, now.AddMinutes(-id)));
        }

        AddFiled(db, 1001, CpsaMiniCexTypeId, "declined", TraineeId, NamesTheAssessor, now.AddDays(-1),
            Move("submit", "draft", "requested", TraineeId, now.AddDays(-2)),
            Move("decline", "requested", "declined", AssessorId, now.AddDays(-1)));
        AddFiled(db, 1002, CpsaMiniCexTypeId, "completed", TraineeId, NamesTheAssessor, now.AddDays(-2),
            Move("submit", "draft", "requested", TraineeId, now.AddDays(-3)),
            Move("complete", "requested", "completed", AssessorId, now.AddDays(-2)));
        await db.SaveChangesAsync();

        var result = await Handle(db);

        result.RecentDecisions.Select(item => item.ActivityId).Should().Equal(
            [1001, 1002], "a returned review has a move left, so it is not a decision, however recent");
    }

    [Fact]
    public async Task AnAssessorWhoIsAlsoATrainee_SeesNoneOfTheirOwnPortfolio_OnlyWhatTheyAssessOrRaisedForOthers()
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
        // Assessor work: a Mini-CEX the caller completed, and one the caller raised, finished, for a trainee.
        AddActedOn(db, 1, WbaTypeId, version: 1, "completed");
        var now = DateTime.UtcNow;
        db.Activities.Add(new Activity
        {
            Id = 16, ActivityTypeId = WbaTypeId, SchemaVersion = 1,
            SubjectUserId = TraineeId, CreatedByUserId = AssessorId, CurrentState = "completed", DataJson = "{}",
            CreatedOn = now.AddDays(-2), UpdatedOn = now.AddMinutes(-16),
            Transitions =
            [
                new ActivityTransition
                {
                    Id = 16, ActivityId = 16, FromState = "completed", ToState = "completed", TransitionKey = "create",
                    ActorUserId = AssessorId, OccurredOn = now.AddMinutes(-16)
                }
            ]
        });
        await db.SaveChangesAsync();

        var result = await Handle(db, CreatePrincipal(AssessorId, "Assessor", "Trainee"));

        result.RecentDecisions.Select(item => item.ActivityId).Should().BeEquivalentTo([1, 16]);
        result.AwaitingReview.Should().BeEmpty();
        result.PendingRequestCount.Should().Be(0);
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

        result.AwaitingReview.Select(item => (item.ActivityId, item.CurrentStateLabel))
            .Should().BeEquivalentTo([(1, "Accepted for observation"), (2, "Accepted")]);
    }

    /// <summary>
    /// Each row says whose it is by name, looked up once for the page (T142's rule), and by the id only when that user has
    /// no name on record. Until T250 both lists carried the subject's user id in <c>SubjectName</c>.
    /// </summary>
    [Fact]
    public async Task EachRow_NamesItsSubject_InOneLookup_AndByTheIdOnlyWhenNoNameIsOnRecord()
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

        result.AwaitingReview.Select(item => (item.ActivityId, item.SubjectName))
            .Should().Equal((1, "Thandi Nkosi"));
        result.RecentDecisions.Select(item => (item.ActivityId, item.SubjectName))
            .Should().BeEquivalentTo([(2, "Sipho Dlamini"), (3, "Thandi Nkosi"), (4, "trainee-gone")]);
        directory.Lookups.Should().ContainSingle()
            .Which.Should().BeEquivalentTo([TraineeId, "trainee-2", "trainee-gone"]);
    }

    private static async Task<AssessorDashboardSummaryDto> Handle(
        ApplicationDbContext db, ClaimsPrincipal? principal = null, FakeUserDirectory? directory = null)
        => await new GetAssessorDashboardSummaryQueryHandler(
                db, new WorkflowEvaluator(), directory ?? FakeUserDirectory.Empty, Options.Create(new DashboardThresholds()))
            .Handle(new GetAssessorDashboardSummaryQuery(principal ?? CreatePrincipal(AssessorId)), CancellationToken.None);

    /// <summary>An activity of the caller's own, created by them, as <c>ActivityService</c> writes one.</summary>
    private static void AddOwn(ApplicationDbContext db, int id, int typeId, string state)
    {
        var now = DateTime.UtcNow;
        db.Activities.Add(new Activity
        {
            Id = id, ActivityTypeId = typeId, SchemaVersion = 1,
            SubjectUserId = AssessorId, CreatedByUserId = AssessorId, CurrentState = state,
            DataJson = """{ "assessor_user_id": "assessor-2" }""",
            CreatedOn = now.AddDays(-1), UpdatedOn = now.AddMinutes(-id),
            Transitions =
            [
                new ActivityTransition
                {
                    Id = id, ActivityId = id, FromState = state, ToState = state, TransitionKey = "create",
                    ActorUserId = AssessorId, OccurredOn = now.AddMinutes(-id)
                }
            ]
        });
    }

    /// <summary>
    /// The shapes <see cref="FinishingWorkflows" /> copies, each with the version row a publish writes, so a move on an
    /// activity pinned to it is judged against that row; the reflective exercise has a second version.
    /// </summary>
    private static void SeedTypes(ApplicationDbContext db)
    {
        db.ActivityTypes.AddRange(
            new ActivityType
            {
                Id = ReflectiveTypeId, Key = "reflective_exercise", Name = "Reflective exercise",
                Scope = ActivityScope.Global, Version = 2,
                WorkflowJson = FinishingWorkflows.ReflectiveExerciseWithSignOff
            },
            new ActivityType
            {
                Id = TeachingTypeId, Key = "teaching_session", Name = "Teaching session",
                Scope = ActivityScope.Global, Version = 1, WorkflowJson = FinishingWorkflows.TeachingSession
            },
            new ActivityType
            {
                Id = WbaTypeId, Key = "mini_cex", Name = "Mini-CEX",
                Scope = ActivityScope.Global, Version = 1, WorkflowJson = FinishingWorkflows.Wba
            },
            new ActivityType
            {
                Id = MsfTypeId, Key = "msf", Name = "MSF",
                Scope = ActivityScope.Global, Version = 1, WorkflowJson = FinishingWorkflows.Msf
            },
            new ActivityType
            {
                Id = ProcedureLogTypeId, Key = "procedure_log", Name = "Procedure log",
                Scope = ActivityScope.Global, Version = 1, WorkflowJson = FinishingWorkflows.ProcedureLog
            });

        db.Set<ActivityTypeVersion>().AddRange(
            new ActivityTypeVersion
            {
                Id = 1, ActivityTypeId = ReflectiveTypeId, Version = 1, WorkflowJson = FinishingWorkflows.ReflectiveExercise
            },
            new ActivityTypeVersion
            {
                Id = 2, ActivityTypeId = ReflectiveTypeId, Version = 2,
                WorkflowJson = FinishingWorkflows.ReflectiveExerciseWithSignOff
            },
            new ActivityTypeVersion { Id = 3, ActivityTypeId = TeachingTypeId, Version = 1, WorkflowJson = FinishingWorkflows.TeachingSession },
            new ActivityTypeVersion { Id = 4, ActivityTypeId = WbaTypeId, Version = 1, WorkflowJson = FinishingWorkflows.Wba },
            new ActivityTypeVersion { Id = 5, ActivityTypeId = MsfTypeId, Version = 1, WorkflowJson = FinishingWorkflows.Msf },
            new ActivityTypeVersion { Id = 6, ActivityTypeId = ProcedureLogTypeId, Version = 1, WorkflowJson = FinishingWorkflows.ProcedureLog });
    }

    /// <summary>The shipped CPSA seeds KGK runs: a rated Mini-CEX, a portfolio review and a reflective exercise.</summary>
    private static void SeedCpsaTypes(ApplicationDbContext db)
    {
        ShippedSeeds.AddType(db, CpsaMiniCexTypeId, "mini_cex_cpsa", "Mini-CEX (Paediatrics)");
        ShippedSeeds.AddType(db, CpsaPortfolioReviewTypeId, "portfolio_review_cpsa", "Portfolio and Logbook Review (Paediatrics)");
        ShippedSeeds.AddType(db, CpsaReflectiveTypeId, "reflective_exercise_cpsa", "Reflective Exercise (Paediatrics)");
    }

    /// <summary>An activity of another user's that the assessor has made a move on, naming him as its assessor.</summary>
    private static void AddActedOn(
        ApplicationDbContext db, int id, int typeId, int version, string state, string subject = TraineeId)
    {
        var now = DateTime.UtcNow;
        db.Activities.Add(new Activity
        {
            Id = id, ActivityTypeId = typeId, SchemaVersion = version,
            SubjectUserId = subject, CreatedByUserId = subject, CurrentState = state, DataJson = NamesTheAssessor,
            CreatedOn = now.AddDays(-2), UpdatedOn = now.AddMinutes(-id),
            Transitions =
            [
                new ActivityTransition
                {
                    Id = id, ActivityId = id, FromState = "draft", ToState = state, TransitionKey = "move",
                    ActorUserId = AssessorId, OccurredOn = now.AddMinutes(-id)
                }
            ]
        });
    }

    /// <summary>An activity filed by <paramref name="subject" /> about themselves, with the moves given.</summary>
    private static void AddFiled(
        ApplicationDbContext db,
        int id,
        int typeId,
        string state,
        string subject,
        string dataJson,
        DateTime updatedOn,
        params ActivityTransition[] moves)
    {
        var activity = new Activity
        {
            Id = id, ActivityTypeId = typeId, SchemaVersion = 1,
            SubjectUserId = subject, CreatedByUserId = subject, CurrentState = state, DataJson = dataJson,
            CreatedOn = updatedOn.AddDays(-1), UpdatedOn = updatedOn
        };
        foreach (var move in moves)
        {
            activity.Transitions.Add(move);
        }

        db.Activities.Add(activity);
    }

    private static ActivityTransition Move(string key, string from, string to, string actor, DateTime occurredOn) => new()
    {
        FromState = from, ToState = to, TransitionKey = key, ActorUserId = actor, OccurredOn = occurredOn
    };

    private static ClaimsPrincipal CreatePrincipal(string userId, params string[] roles)
        => new(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, userId),
                .. (roles.Length == 0 ? ["Assessor"] : roles).Select(role => new Claim(ClaimTypes.Role, role))
            ],
            "test"));

    private static ApplicationDbContext CreateDb()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
}
