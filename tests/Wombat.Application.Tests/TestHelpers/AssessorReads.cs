using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Wombat.Application.Common.Options;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Queries.ListDecidedByYou;
using Wombat.Application.Features.Activities.Queries.ListWaitingForYou;
using Wombat.Domain.Activities;
using Wombat.Infrastructure.Activities;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.TestHelpers;

/// <summary>
/// The fixtures of the assessor's two reads, "Waiting for you" and "Decided by you" (T350, notes 5 and 6), and of the
/// Home that makes both (<c>AssessorDashboardQueryTests</c>): the types, the activities an assessor is named on, their
/// moves, and a clock the waiting read can be pinned to.
/// </summary>
internal static class AssessorReads
{
    public const string AssessorId = "assessor-1";
    public const string TraineeId = "trainee-1";

    public const int ReflectiveTypeId = 1;
    public const int TeachingTypeId = 2;
    public const int WbaTypeId = 3;
    public const int MsfTypeId = 4;
    public const int ProcedureLogTypeId = 5;
    public const int RelabelledWbaTypeId = 6;

    public const int CpsaMiniCexTypeId = 21;
    public const int CpsaPortfolioReviewTypeId = 22;
    public const int CpsaReflectiveTypeId = 23;

    public static readonly string NamesTheAssessor = $$"""{ "assessor_user_id": "{{AssessorId}}" }""";

    public static ApplicationDbContext CreateDb()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    public static ClaimsPrincipal CreatePrincipal(string userId, params string[] roles)
        => new(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, userId),
                .. (roles.Length == 0 ? ["Assessor"] : roles).Select(role => new Claim(ClaimTypes.Role, role))
            ],
            "test"));

    public static async Task<WaitingForYouDto> WaitingAsync(
        ApplicationDbContext db,
        ClaimsPrincipal? principal = null,
        FakeUserDirectory? directory = null,
        DateTime? now = null,
        DashboardThresholds? thresholds = null)
        => await new ListWaitingForYouQueryHandler(
                db,
                new WorkflowEvaluator(),
                directory ?? FakeUserDirectory.Empty,
                Options.Create(thresholds ?? new DashboardThresholds()),
                now is { } at ? new FixedClock(at) : TimeProvider.System)
            .Handle(new ListWaitingForYouQuery(principal ?? CreatePrincipal(AssessorId)), CancellationToken.None);

    public static async Task<ActivityListPageDto> DecidedAsync(
        ApplicationDbContext db,
        ClaimsPrincipal? principal = null,
        FakeUserDirectory? directory = null,
        int page = 1,
        int pageSize = 20)
        => await new ListDecidedByYouQueryHandler(db, directory ?? FakeUserDirectory.Empty)
            .Handle(new ListDecidedByYouQuery(principal ?? CreatePrincipal(AssessorId), page, pageSize), CancellationToken.None);

    /// <summary>
    /// The shapes <see cref="FinishingWorkflows" /> copies, each with the version row a publish writes, so a move on an
    /// activity pinned to it is judged against that row; the reflective exercise has a second version.
    /// </summary>
    public static void SeedTypes(ApplicationDbContext db)
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
    public static void SeedCpsaTypes(ApplicationDbContext db)
    {
        ShippedSeeds.AddType(db, CpsaMiniCexTypeId, "mini_cex_cpsa", "Mini-CEX (Paediatrics)");
        ShippedSeeds.AddType(db, CpsaPortfolioReviewTypeId, "portfolio_review_cpsa", "Portfolio and Logbook Review (Paediatrics)");
        ShippedSeeds.AddType(db, CpsaReflectiveTypeId, "reflective_exercise_cpsa", "Reflective Exercise (Paediatrics)");
    }

    /// <summary>An activity of the caller's own, created by them, as <c>ActivityService</c> writes one.</summary>
    public static void AddOwn(ApplicationDbContext db, int id, int typeId, string state)
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

    /// <summary>An activity of another user's that the assessor has made a move on, naming him as its assessor.</summary>
    public static void AddActedOn(
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
    public static Activity AddFiled(
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
        return activity;
    }

    public static ActivityTransition Move(string key, string from, string to, string actor, DateTime occurredOn) => new()
    {
        FromState = from, ToState = to, TransitionKey = key, ActorUserId = actor, OccurredOn = occurredOn
    };

    /// <summary>A clock that always reads <paramref name="now" /> (UTC).</summary>
    public sealed class FixedClock(DateTime now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(DateTime.SpecifyKind(now, DateTimeKind.Utc));
    }
}
