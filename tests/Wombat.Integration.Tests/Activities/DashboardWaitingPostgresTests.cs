using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Options;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Queries.ListDecidedByYou;
using Wombat.Application.Features.Activities.Queries.ListWaitingForYou;
using Wombat.Application.Features.Activities.Queries.ListNeedsYou;
using Wombat.Application.Features.Activities.Services;
using Wombat.Application.Features.Dashboards.Assessor;
using Wombat.Application.Features.Dashboards.Coordinator;
using Wombat.Application.Features.Dashboards.SpecialityAdmin;
using Wombat.Application.Features.Dashboards.SubSpecialityAdmin;
using Wombat.Application.Features.Dashboards.Trainee;
using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Workflow;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Activities;
using Wombat.Infrastructure.Persistence;
using Wombat.Integration.Tests.TestSupport;
using Wombat.Tests.Shared;

namespace Wombat.Integration.Tests.Activities;

/// <summary>
/// T297 on a real PostgreSQL server: the inbox and the four waiting cards, read through <c>ActivityWaiting</c>.
/// </summary>
/// <remarks>
/// <para>
/// The unit suites run on EF InMemory, which evaluates each query in memory. Here each must translate: the narrowing to
/// the waiting types and states (two array <c>Contains</c>, <c>= ANY</c>), the assessor's decisions (a correlated
/// subquery for the latest move's actor, ordered by time then id, and the latest move's time), and the coordinator's
/// narrowing beneath the read rule. The figures are Steps 3.24, 3.30 and 3.53 in small: a Mini-CEX a trainee filed and
/// submitted naming the assessor, stalled eight days; a portfolio review awaiting him; a draft; a request he declined;
/// and a request at another institution.
/// </para>
/// <para>
/// Isolated the way <c>NomineeDirectoryPostgresTests</c> is: a migrated schema of its own (<c>it_&lt;guid&gt;</c>),
/// registered before it is created and dropped in a finally and again on dispose.
/// </para>
/// </remarks>
public sealed class DashboardWaitingPostgresTests : IAsyncLifetime
{
    private const int InstitutionId = 10;
    private const int OtherInstitutionId = 20;
    private const int SpecialityId = 5;
    private const int SubSpecialityId = 6;

    private const string TraineeId = "t297-trainee";
    private const string OtherTraineeId = "t297-other-trainee";
    private const string AssessorId = "t297-assessor";

    private readonly TestSchemas _schemas = new();

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _schemas.DropAllAsync();

    [Fact]
    public async Task TheInboxAndEveryWaitingCard_OnPostgres_ReadWhatWaitsFromThePinnedWorkflow()
    {
        try
        {
            var schema = await _schemas.CreateAsync();
            await using (var db = NewContext(schema))
            {
                await db.Database.MigrateAsync();
            }

            var now = DateTime.UtcNow;
            int miniCex, portfolioReview, draft, declined, elsewhere;
            await using (var db = NewContext(schema))
            {
                var miniCexType = AddType(db, "mini_cex_cpsa", "Mini-CEX (Paediatrics)");
                var portfolioType = AddType(db, "portfolio_review_cpsa", "Portfolio and Logbook Review (Paediatrics)");
                await db.SaveChangesAsync();

                var namesHim = $$"""{ "assessor_user_id": "{{AssessorId}}" }""";
                var requested = Add(db, miniCexType, "requested", TraineeId, InstitutionId, namesHim, now.AddDays(-8),
                    Move("create", "draft", "draft", TraineeId, now.AddDays(-9)),
                    Move("submit", "draft", "requested", TraineeId, now.AddDays(-8)));
                var awaiting = Add(db, portfolioType, "submitted", TraineeId, InstitutionId, namesHim, now.AddDays(-2),
                    Move("submit", "draft", "submitted", TraineeId, now.AddDays(-2)));
                var unsent = Add(db, miniCexType, "draft", TraineeId, InstitutionId, namesHim, now.AddDays(-20),
                    Move("create", "draft", "draft", TraineeId, now.AddDays(-20)));
                // Two moves at one instant: a tie goes to the later row, so his decline, inserted after her submit, is the
                // latest move, and the decision is his. The SQL must break the tie by id, as the in-memory read does.
                var refused = Add(db, miniCexType, "declined", TraineeId, InstitutionId, namesHim, now.AddDays(-1),
                    Move("submit", "draft", "requested", TraineeId, now.AddDays(-1)),
                    Move("decline", "requested", "declined", AssessorId, now.AddDays(-1)));
                var otherInstitution = Add(db, miniCexType, "requested", OtherTraineeId, OtherInstitutionId, namesHim, now.AddDays(-9),
                    Move("submit", "draft", "requested", OtherTraineeId, now.AddDays(-9)));
                await db.SaveChangesAsync();

                (miniCex, portfolioReview, draft, declined, elsewhere) = (requested.Id, awaiting.Id, unsent.Id, refused.Id, otherInstitution.Id);
            }

            var names = new FakeUserDirectory((TraineeId, "Nomsa Mahlangu"), (OtherTraineeId, "Other Trainee"))
                .With(new UserIdentityDetails(TraineeId, "nomsa@test.local", "Nomsa", "Mahlangu", InstitutionId, [], [], [WombatRoles.Trainee]));
            var assessor = Principal(AssessorId, [WombatRoles.Assessor], InstitutionId);

            await using (var db = NewContext(schema))
            {
                // T350 (note 5): one waiting read, oldest first, for the inbox and Home alike.
                var inbox = await new ListWaitingForYouQueryHandler(
                        db, new WorkflowEvaluator(), names, Options.Create(new DashboardThresholds()), TimeProvider.System)
                    .Handle(new ListWaitingForYouQuery(assessor), CancellationToken.None);
                inbox.Items.Select(row => (row.Id, row.CurrentStateLabel, row.IsOverdue, row.WaitedDays)).Should().Equal(
                    [(elsewhere, "Requested", true, 9), (miniCex, "Requested", true, 8), (portfolioReview, "Awaiting review", false, 2)],
                    "what he can move, oldest first");
                inbox.OverdueCount.Should().Be(2);
                inbox.Items.Select(row => row.SubjectName).Should().Equal("Other Trainee", "Nomsa Mahlangu", "Nomsa Mahlangu");

                var card = await new GetAssessorDashboardSummaryQueryHandler(
                        db, new WorkflowEvaluator(), names, Options.Create(new DashboardThresholds()), TimeProvider.System)
                    .Handle(new GetAssessorDashboardSummaryQuery(assessor), CancellationToken.None);
                card.Waiting.Items.Select(item => item.Id).Should().Equal(inbox.Items.Select(row => row.Id));
                card.Decisions.Items.Select(item => (item.Id, item.CurrentStateLabel, item.IsFinished))
                    .Should().Equal((declined, "Declined", false));

                // T350 (note 6): "Decided by you", paged, on PostgreSQL: the latest move's actor by time then id, its time,
                // and a page past the end served as the last.
                var decided = await new ListDecidedByYouQueryHandler(db, names)
                    .Handle(new ListDecidedByYouQuery(assessor, Page: 4, PageSize: 1), CancellationToken.None);
                (decided.Page, decided.TotalCount).Should().Be((1, 1));
                var decision = decided.Items.Should().ContainSingle().Subject;
                decision.Id.Should().Be(declined);
                decision.DecidedOn.Should().BeCloseTo(now.AddDays(-1), TimeSpan.FromSeconds(1));
                decision.SubjectName.Should().Be("Nomsa Mahlangu");

                // T342 (B6, E8): the two waiting on him she may only cancel, so they are with him; her draft is hers to
                // submit, so it is Needs you, and Home's card is Needs you, on Postgres as in memory; the decline has no
                // move left.
                var trainee = await new GetTraineeDashboardSummaryQueryHandler(db, new WorkflowEvaluator(), names)
                    .Handle(new GetTraineeDashboardSummaryQuery(Principal(TraineeId, [WombatRoles.Trainee], InstitutionId)), CancellationToken.None);
                trainee.NeedsYou.Select(row => row.Id).Should().Equal(draft);
                var needsYou = await new ListNeedsYouQueryHandler(db, new WorkflowEvaluator(), names)
                    .Handle(new ListNeedsYouQuery(Principal(TraineeId, [WombatRoles.Trainee], InstitutionId)), CancellationToken.None);
                needsYou.Select(row => (row.Id, row.Holder!.Kind, row.NomineeName)).Should().ContainSingle()
                    .Which.Should().Be((draft, ActivityHolderKind.Author, AssessorId), "the assessor has no name on record, so his id is shown");

                var coordinator = await new GetCoordinatorDashboardSummaryQueryHandler(
                        db, Options.Create(new DashboardThresholds { CoordinatorStallDays = 7 }), names)
                    .Handle(new GetCoordinatorDashboardSummaryQuery(Principal("t297-coordinator", [WombatRoles.Coordinator], InstitutionId)), CancellationToken.None);
                coordinator.StalledRequests.Select(item => (item.ActivityId, item.SubjectName))
                    .Should().Equal((miniCex, "Nomsa Mahlangu"));

                var speciality = await new GetSpecialityAdminDashboardSummaryQueryHandler(db, names)
                    .Handle(new GetSpecialityAdminDashboardSummaryQuery(
                        Principal("t297-speciality-admin", [WombatRoles.SpecialityAdmin], InstitutionId, specialityId: SpecialityId),
                        new DateOnly(2026, 9, 23)), CancellationToken.None);
                speciality.PendingReviewCount.Should().Be(2, "the requested Mini-CEX and the portfolio review, at her institution");

                var subSpeciality = await new GetSubSpecialityAdminDashboardSummaryQueryHandler(db, names)
                    .Handle(new GetSubSpecialityAdminDashboardSummaryQuery(
                        Principal("t297-sub-speciality-admin", [WombatRoles.SubSpecialityAdmin], InstitutionId, subSpecialityId: SubSpecialityId),
                        new DateOnly(2026, 9, 23)), CancellationToken.None);
                subSpeciality.PendingReviewCount.Should().Be(2);
            }
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    /// <summary>
    /// T342 (B6, G12) on PostgreSQL: a trainee holding no other role has nothing in her Activity inbox while she holds a
    /// draft, a reflection returned to her and a request she may only cancel. The first two are hers by the author's arm,
    /// so they are Needs you; the third is with its assessor. The inbox reads only the arms that are not the author's.
    /// </summary>
    [Fact]
    public async Task ATraineesInbox_OnPostgres_IsEmpty_BesideHerDraftHerReturnedReflectionAndHerCancellableRequest()
    {
        try
        {
            var schema = await _schemas.CreateAsync();
            await using (var db = NewContext(schema))
            {
                await db.Database.MigrateAsync();
            }

            var now = DateTime.UtcNow;
            int draft, returned;
            await using (var db = NewContext(schema))
            {
                var miniCexType = AddType(db, "mini_cex_cpsa", "Mini-CEX (Paediatrics)");
                var reflectionType = AddType(db, "reflective_exercise_cpsa", "Reflective Exercise (Paediatrics)");

                var namesHim = $$"""{ "assessor_user_id": "{{AssessorId}}" }""";
                var unsent = Add(db, miniCexType, "draft", TraineeId, InstitutionId, namesHim, now.AddDays(-3),
                    Move("create", "draft", "draft", TraineeId, now.AddDays(-3)));
                var sentBack = Add(db, reflectionType, "draft", TraineeId, InstitutionId, namesHim, now.AddDays(-1),
                    Move("create", "draft", "draft", TraineeId, now.AddDays(-4)),
                    Move("submit", "draft", "submitted", TraineeId, now.AddDays(-2)),
                    Move("return", "submitted", "draft", AssessorId, now.AddDays(-1)));
                Add(db, miniCexType, "requested", TraineeId, InstitutionId, namesHim, now.AddDays(-2),
                    Move("create", "draft", "draft", TraineeId, now.AddDays(-5)),
                    Move("submit", "draft", "requested", TraineeId, now.AddDays(-2)));
                await db.SaveChangesAsync();
                (draft, returned) = (unsent.Id, sentBack.Id);
            }

            var names = new FakeUserDirectory((TraineeId, "Nomsa Mahlangu"));
            var trainee = Principal(TraineeId, [WombatRoles.Trainee], InstitutionId);

            await using (var db = NewContext(schema))
            {
                var inbox = await new ListWaitingForYouQueryHandler(
                        db, new WorkflowEvaluator(), names, Options.Create(new DashboardThresholds()), TimeProvider.System)
                    .Handle(new ListWaitingForYouQuery(trainee), CancellationToken.None);
                inbox.Items.Should().BeEmpty("nothing waits on her as anyone's assessor or reviewer");

                // Guard: the rows are there and hers to move, so the empty inbox is not an empty database.
                var needsYou = await new ListNeedsYouQueryHandler(db, new WorkflowEvaluator(), names)
                    .Handle(new ListNeedsYouQuery(trainee), CancellationToken.None);
                needsYou.Select(row => row.Id).Should().BeEquivalentTo([draft, returned]);
            }
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    /// <summary>
    /// The T297 review's narrowing (<c>WaitingStates.NarrowToCaller</c>) on PostgreSQL, one arm at a time: a <c>field:</c>
    /// arm is a text search of the <c>jsonb</c> data (<c>Convert.ToString</c>, which must translate to <c>::text</c>), a
    /// <c>scope:</c> arm the institution stamp, a held role every row, and <c>subject</c> the caller's own. Each caller's
    /// narrowed read is compared with the act gate run on every row, as the inbox read them before T297.
    /// </summary>
    [Fact]
    public async Task TheCallerNarrowing_OnPostgres_KeepsWhatTheActGateAdmits_AndLeavesTheRest()
    {
        try
        {
            var schema = await _schemas.CreateAsync();
            await using (var db = NewContext(schema))
            {
                await db.Database.MigrateAsync();
            }

            var now = DateTime.UtcNow;
            await using (var db = NewContext(schema))
            {
                var miniCexType = AddType(db, "mini_cex_cpsa", "Mini-CEX (Paediatrics)");
                var teachingType = AddType(db, "teaching_session", "Teaching session");
                var feedbackType = AddType(db, "learner_feedback_cpsa", "Learner feedback");

                var namesHim = $$"""{ "assessor_user_id": "{{AssessorId}}" }""";
                const string namesAnother = """{ "assessor_user_id": "t297-another-assessor" }""";
                Add(db, miniCexType, "requested", TraineeId, InstitutionId, namesHim, now.AddDays(-1));
                Add(db, miniCexType, "requested", TraineeId, InstitutionId, namesAnother, now.AddDays(-2));
                Add(db, miniCexType, "draft", TraineeId, InstitutionId, namesHim, now.AddDays(-3));
                Add(db, teachingType, "submitted", TraineeId, InstitutionId, "{}", now.AddDays(-4));
                Add(db, teachingType, "submitted", OtherTraineeId, OtherInstitutionId, "{}", now.AddDays(-5));
                Add(db, feedbackType, "draft", OtherTraineeId, OtherInstitutionId, "{}", now.AddDays(-6));
                await db.SaveChangesAsync();
            }

            var callers = new Dictionary<string, ClaimsPrincipal>
            {
                ["assessor"] = Principal(AssessorId, [WombatRoles.Assessor], InstitutionId),
                ["trainee"] = Principal(TraineeId, [WombatRoles.Trainee], InstitutionId),
                ["speciality admin"] = Principal("t297-speciality-admin", [WombatRoles.SpecialityAdmin], InstitutionId, specialityId: SpecialityId),
                ["coordinator"] = Principal("t297-coordinator", [WombatRoles.Coordinator], InstitutionId)
            };

            await using (var db = NewContext(schema))
            {
                var evaluator = new WorkflowEvaluator();
                var everyRow = await db.Activities.AsNoTracking().Include(activity => activity.ActivityType).ToListAsync();
                var published = await db.Set<ActivityTypeVersion>().AsNoTracking().ToListAsync();
                var movable = await ActivityWaiting.LoadMovableAsync(db);

                foreach (var (who, principal) in callers)
                {
                    var admitted = everyRow
                        .Where(activity => ActivityWaiting.IsActionableBy(
                            WorkflowParser.Parse(published.Single(version =>
                                version.ActivityTypeId == activity.ActivityTypeId && version.Version == activity.SchemaVersion).WorkflowJson),
                            activity, principal, evaluator))
                        .Select(activity => activity.Id)
                        .ToList();
                    var narrowed = await movable.NarrowToCaller(db.Activities, principal).Select(activity => activity.Id).ToListAsync();
                    var read = await ActivityWaiting.LoadActionableAsync(db.Activities, db, evaluator, principal);

                    admitted.Should().NotBeEmpty($"the {who} has something to move, or the comparison below is vacuous");
                    narrowed.Should().Contain(admitted, $"the {who}'s narrowing keeps every row the gate admits");
                    narrowed.Should().HaveCountLessThan(everyRow.Count, $"the {who}'s narrowing leaves rows in the database");
                    read.Select(row => row.Activity.Id).Should().BeEquivalentTo(admitted, $"the {who}'s read is the gate's");
                }
            }
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    private static int AddType(ApplicationDbContext db, string key, string name)
    {
        var workflowJson = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Activities", "Seeds", key, "workflow.json"));
        var publishedOn = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var type = new ActivityType
        {
            Key = $"t297_{key}",
            Name = name,
            Scope = ActivityScope.Global,
            Version = 1,
            IsActive = true,
            SchemaJson = "{}",
            WorkflowJson = workflowJson,
            CreditRulesJson = """{ "counts_for": [] }""",
            OwnerUserId = "system",
            CreatedOn = publishedOn
        };
        type.Versions.Add(new ActivityTypeVersion
        {
            Version = 1,
            SchemaJson = "{}",
            WorkflowJson = workflowJson,
            CreditRulesJson = """{ "counts_for": [] }""",
            PublishedByUserId = "system",
            PublishedOn = publishedOn
        });
        db.ActivityTypes.Add(type);
        db.SaveChanges();
        return type.Id;
    }

    private static Activity Add(
        ApplicationDbContext db,
        int typeId,
        string state,
        string subject,
        int institutionId,
        string dataJson,
        DateTime updatedOn,
        params ActivityTransition[] moves)
    {
        var activity = new Activity
        {
            ActivityTypeId = typeId,
            SchemaVersion = 1,
            SubjectUserId = subject,
            CreatedByUserId = subject,
            CurrentState = state,
            DataJson = dataJson,
            InstitutionId = institutionId,
            SpecialityId = SpecialityId,
            SubSpecialityId = SubSpecialityId,
            CreatedOn = updatedOn.AddDays(-1),
            UpdatedOn = updatedOn,
            ObservedOn = DateOnly.FromDateTime(updatedOn)
        };
        foreach (var move in moves)
        {
            activity.Transitions.Add(move);
        }

        db.Activities.Add(activity);
        return activity;
    }

    private static ActivityTransition Move(string key, string from, string to, string actor, DateTime occurredOn) => new()
    {
        FromState = from, ToState = to, TransitionKey = key, ActorUserId = actor, OccurredOn = occurredOn, SnapshotJson = "{}"
    };

    private static ClaimsPrincipal Principal(
        string userId, string[] roles, int institutionId, int? specialityId = null, int? subSpecialityId = null)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(WombatClaimTypes.InstitutionId, institutionId.ToString(System.Globalization.CultureInfo.InvariantCulture))
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        if (specialityId is int speciality)
        {
            claims.Add(new Claim(WombatClaimTypes.SpecialityId, speciality.ToString(System.Globalization.CultureInfo.InvariantCulture)));
        }

        if (subSpecialityId is int subSpeciality)
        {
            claims.Add(new Claim(WombatClaimTypes.SubSpecialityId, subSpeciality.ToString(System.Globalization.CultureInfo.InvariantCulture)));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }

    private static ApplicationDbContext NewContext(string schema)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(TestDatabase.SchemaConnectionString(schema))
            .Options);
}
