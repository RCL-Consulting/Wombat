using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Wombat.Application.Common.Options;
using Wombat.Application.Features.Dashboards.Assessor;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Activities;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Dashboards;

/// <summary>
/// The assessor dashboard reads "finished" as a terminal state of each activity's PINNED workflow (T203, D44), not as
/// the literal <c>completed</c>.
/// </summary>
public sealed class AssessorDashboardQueryTests
{
    private const string AssessorId = "assessor-1";

    private const int ReflectiveTypeId = 1;
    private const int TeachingTypeId = 2;
    private const int WbaTypeId = 3;
    private const int MsfTypeId = 4;
    private const int ProcedureLogTypeId = 5;
    private const int RelabelledWbaTypeId = 6;

    [Fact]
    public async Task AnActivityInATerminalStateOfItsWorkflow_IsADecision_AndNeverWorkNeedingAction()
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
        // literal "completed" saw none of them.
        result.RecentDecisions.Select(item => item.ActivityId).Should().BeEquivalentTo([1, 2, 3, 4, 5, 6]);
        // The teaching session finishes in "accepted", so it is not an assessment still needing action.
        result.AcceptedActivities.Select(item => item.ActivityId).Should().Equal(7);
        result.PendingRequestCount.Should().Be(1);
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
    }

    [Fact]
    public async Task AnAssessorWhoIsAlsoATrainee_SeesNoneOfTheirOwnPortfolio_OnlyWhatTheyAssessOrRaisedForOthers()
    {
        await using var db = CreateDb();
        SeedTypes(db);
        // The caller's own portfolio: they created each row and made its "create" move. A procedure log is born in its
        // terminal "logged", so without the subject exclusion it would read as the caller's decision the moment it is
        // logged; the requested and accepted Mini-CEX are the caller's own requests, not assessments for them to do.
        AddOwn(db, 11, ProcedureLogTypeId, "logged");
        AddOwn(db, 12, ReflectiveTypeId, "discussed");
        AddOwn(db, 13, WbaTypeId, "requested");
        AddOwn(db, 14, WbaTypeId, "accepted");
        AddOwn(db, 15, WbaTypeId, "completed");
        // Assessor work: a Mini-CEX the caller completed, and one the caller raised for a trainee (the creator arm).
        AddActedOn(db, 1, WbaTypeId, version: 1, "completed");
        db.Activities.Add(new Activity
        {
            Id = 16, ActivityTypeId = WbaTypeId, SchemaVersion = 1,
            SubjectUserId = "trainee-1", CreatedByUserId = AssessorId, CurrentState = "completed", DataJson = "{}",
            CreatedOn = DateTime.UtcNow.AddDays(-2), UpdatedOn = DateTime.UtcNow.AddMinutes(-16)
        });
        await db.SaveChangesAsync();

        var result = await Handle(db, CreatePrincipal(AssessorId, "Assessor", "Trainee"));

        result.RecentDecisions.Select(item => item.ActivityId).Should().BeEquivalentTo([1, 16]);
        result.AcceptedActivities.Should().BeEmpty();
        result.PendingRequestCount.Should().Be(0);
    }

    /// <summary>
    /// T220 review: the "Accepted, needing action" card printed the key <c>accepted</c>. Each item carries the state's
    /// label in the version the activity is pinned to: activity 1's version 1 renames it, where the type's current
    /// version does not.
    /// </summary>
    [Fact]
    public async Task AnAcceptedAssessment_CarriesItsStateLabel_FromThePinnedVersion()
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
                Id = 3, ActivityTypeId = RelabelledWbaTypeId, Version = 1,
                WorkflowJson = FinishingWorkflows.Wba.Replace(
                    "\"label\": \"Accepted\"", "\"label\": \"Accepted for observation\"", StringComparison.Ordinal)
            },
            new ActivityTypeVersion
            {
                Id = 4, ActivityTypeId = RelabelledWbaTypeId, Version = 2, WorkflowJson = FinishingWorkflows.Wba
            });
        AddActedOn(db, 1, RelabelledWbaTypeId, version: 1, "accepted");
        AddActedOn(db, 2, WbaTypeId, version: 1, "accepted");
        await db.SaveChangesAsync();

        var result = await Handle(db);

        result.AcceptedActivities.Select(item => (item.ActivityId, item.CurrentStateLabel))
            .Should().BeEquivalentTo([(1, "Accepted for observation"), (2, "Accepted")]);
    }

    private static async Task<AssessorDashboardSummaryDto> Handle(ApplicationDbContext db, ClaimsPrincipal? principal = null)
        => await new GetAssessorDashboardSummaryQueryHandler(db, Options.Create(new DashboardThresholds()))
            .Handle(new GetAssessorDashboardSummaryQuery(principal ?? CreatePrincipal(AssessorId)), CancellationToken.None);

    /// <summary>An activity of the caller's own, created by them, as <c>ActivityService</c> writes one.</summary>
    private static void AddOwn(ApplicationDbContext db, int id, int typeId, string state)
    {
        var now = DateTime.UtcNow;
        db.Activities.Add(new Activity
        {
            Id = id, ActivityTypeId = typeId, SchemaVersion = 1,
            SubjectUserId = AssessorId, CreatedByUserId = AssessorId, CurrentState = state, DataJson = "{}",
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
            });
    }

    /// <summary>An activity of another user's that the assessor has made a move on.</summary>
    private static void AddActedOn(ApplicationDbContext db, int id, int typeId, int version, string state)
    {
        var now = DateTime.UtcNow;
        db.Activities.Add(new Activity
        {
            Id = id, ActivityTypeId = typeId, SchemaVersion = version,
            SubjectUserId = "trainee-1", CreatedByUserId = "trainee-1", CurrentState = state, DataJson = "{}",
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
