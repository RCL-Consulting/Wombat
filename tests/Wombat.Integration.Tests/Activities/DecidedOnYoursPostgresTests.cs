using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Activities.Queries.GetActivityCountLine;
using Wombat.Application.Features.Activities.Queries.ListActivitiesBySubject;
using Wombat.Application.Features.Activities.Services;
using Wombat.Domain.Activities;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Persistence;
using Wombat.Integration.Tests.TestSupport;
using Wombat.Tests.Shared;

namespace Wombat.Integration.Tests.Activities;

/// <summary>
/// T355 (flow 05, lane A2) on a real PostgreSQL server: Recent decisions' read (<c>DecidedOnYours</c>), the count a
/// decision made (<c>EpaCountLines</c>, and the completed card's <c>GetActivityCountLineQuery</c>), and My activities'
/// EPA filter, beside <c>DashboardWaitingPostgresTests</c>.
/// </summary>
/// <remarks>
/// The unit suites run on EF InMemory. Here each must translate: the latest move's actor and key (correlated subqueries
/// ordered by time then id), the system-managed type's join, the items in force on her curriculum (the EPA's join), and
/// the subject's whole list the E7 names are counted over. On the seeded v11.1 catalogue, in a migrated schema of its own.
/// </remarks>
public sealed class DecidedOnYoursPostgresTests : IAsyncLifetime
{
    private const string TraineeId = "t355-trainee";
    private const string AssessorId = "t355-assessor";
    private const string OtherAssessorId = "t355-assessor-2";

    private static readonly DateOnly Today = new(2026, 10, 3);

    private readonly TestSchemas _schemas = new();

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _schemas.DropAllAsync();

    [Fact]
    public async Task RecentDecisions_TheirCounts_AndTheEpaFilter_ReadOnPostgres()
    {
        try
        {
            var schema = await _schemas.CreateAsync();
            await using (var db = NewContext(schema))
            {
                await db.Database.MigrateAsync();
            }

            await using (var db = NewContext(schema))
            {
                await new DataSeeder(db).SeedAsync();
            }

            await using (var db = NewContext(schema))
            {
                await new PaediatricCatalogueSeeder(db).SeedAsync();
            }

            var now = new DateTime(2026, 10, 3, 6, 0, 0, DateTimeKind.Utc);
            int institutionId, paed001, paed002;
            int completed, declined, returned, msf, inHand, otherEpa;
            await using (var db = NewContext(schema))
            {
                institutionId = await db.Institutions.Where(entity => entity.ShortCode == "DEMO").Select(entity => entity.Id).SingleAsync();
                var curriculumId = await db.Curricula
                    .Where(entity => entity.Name == "Paediatric EPA Curriculum")
                    .Select(entity => entity.Id)
                    .SingleAsync();
                var items = await db.CurriculumItems
                    .Where(item => item.CurriculumId == curriculumId && item.OwningInstitutionId == null)
                    .Select(item => new { item.Id, item.EpaId, item.Epa.Code })
                    .ToListAsync();
                var item001 = items.Single(item => item.Code == "PAED-001");
                paed001 = item001.EpaId;
                paed002 = items.Single(item => item.Code == "PAED-002").EpaId;

                db.TraineeProfiles.Add(new TraineeProfile
                {
                    UserId = TraineeId, InstitutionId = institutionId, CurriculumId = curriculumId,
                    ProgrammeStartDate = new DateOnly(2023, 1, 15), ExpectedCompletionDate = new DateOnly(2027, 1, 14),
                    IsActive = true
                });
                db.CurriculumItemProgresses.Add(new CurriculumItemProgress
                {
                    CurriculumItemId = item001.Id, TraineeUserId = TraineeId, AcademicYear = 2026, Semester = 2,
                    CountsSoFar = 1, MinimumLevelReachedCount = 1, LastUpdated = now
                });

                var miniCexType = AddType(db, "mini_cex_cpsa", "Mini-CEX (Paediatrics)");
                var reflectionType = AddType(db, "reflective_exercise_cpsa", "Reflective Exercise (Paediatrics)");
                var msfType = AddType(db, "msf_cpsa", "Multi-Source Feedback (Paediatrics)", systemManaged: true);
                await db.SaveChangesAsync();

                var namesHim = $$"""{ "assessor_user_id": "{{AssessorId}}" }""";
                var encounter = new DateOnly(2026, 9, 23);
                var completedOne = Add(db, miniCexType, "completed", institutionId, namesHim, paed001, encounter,
                    Move("submit", "draft", "requested", TraineeId, now.AddDays(-3)),
                    Credited(Move("complete", "requested", "completed", AssessorId, now.AddHours(-2)), 1));
                // Two moves at one instant: the tie goes to the later row, his decline.
                var declinedOne = Add(db, miniCexType, "declined", institutionId, namesHim, paed002, new DateOnly(2026, 9, 13),
                    Move("submit", "draft", "requested", TraineeId, now.AddHours(-3)),
                    Move("decline", "requested", "declined", AssessorId, now.AddHours(-3)));
                var returnedOne = Add(db, reflectionType, "draft", institutionId, namesHim, paed001, new DateOnly(2026, 9, 20),
                    Move("submit", "draft", "submitted", TraineeId, now.AddDays(-2)),
                    Move("return", "submitted", "draft", AssessorId, now.AddHours(-1)));
                var msfOne = Add(db, msfType, "recorded", institutionId, "{}", paed001, new DateOnly(2026, 10, 2),
                    Move("record", "draft", "recorded", "t355-coordinator", now.AddHours(-4)));
                // Shares the completed one's type, EPA and date, and is still with its assessor: E7 names both by nominee.
                var inHandOne = Add(db, miniCexType, "requested", institutionId, $$"""{ "assessor_user_id": "{{OtherAssessorId}}" }""",
                    paed001, encounter, Move("submit", "draft", "requested", TraineeId, now.AddDays(-1)));
                var otherEpaOne = Add(db, miniCexType, "completed", institutionId, namesHim, paed002, new DateOnly(2026, 9, 10),
                    Move("submit", "draft", "requested", TraineeId, now.AddDays(-5)),
                    Move("complete", "requested", "completed", AssessorId, now.AddHours(-5)));
                await db.SaveChangesAsync();

                (completed, declined, returned, msf, inHand, otherEpa) =
                    (completedOne.Id, declinedOne.Id, returnedOne.Id, msfOne.Id, inHandOne.Id, otherEpaOne.Id);
            }

            var names = new FakeUserDirectory((AssessorId, "David Naidoo"), (OtherAssessorId, "Fatima Khumalo"));
            var trainee = Principal(TraineeId, [WombatRoles.Trainee], institutionId);

            await using (var db = NewContext(schema))
            {
                var decisions = await DecidedOnYours.ReadAsync(db, names, trainee, take: 5, Today, CancellationToken.None);

                decisions.Select(item => (item.Id, item.CurrentStateLabel, item.IsFinished)).Should().Equal(
                    [(completed, "Completed", true), (declined, "Declined", false), (otherEpa, "Completed", true)],
                    "returned to her draft is not decided; MSF is not listed; her request in hand is not decided");
                decisions[0].DecidedOn.Should().BeCloseTo(now.AddHours(-2), TimeSpan.FromSeconds(1));
                decisions[0].CreditedItemCount.Should().Be(1);
                decisions[0].DisplayName.Should().EndWith(" · David Naidoo", "E7 over her whole list");
                var line = decisions[0].CountLine!;
                (line.EpaCode, line.IsCurrentWindow, line.Window.Name, line.Window.Count, line.Window.Target)
                    .Should().Be(("PAED-001", true, "Semester 2, 2026", 1, 3));
                decisions[1].CountLine!.Window.Count.Should().Be(0, "PAED-002 has no credit yet");

                var card = await new GetActivityCountLineQueryHandler(db, new FixedClock(new DateTime(2026, 10, 3, 6, 0, 0, DateTimeKind.Utc)))
                    .Handle(new GetActivityCountLineQuery(trainee, completed), CancellationToken.None);
                card.Should().BeEquivalentTo(line, "the card and Home read one reader");

                var onPaed001 = await new ListActivitiesBySubjectQueryHandler(db, names)
                    .Handle(new ListActivitiesBySubjectQuery(TraineeId, trainee) { EpaId = paed001 }, CancellationToken.None);
                onPaed001.Items.Select(item => item.Id).Should().Equal(
                    [msf, completed], "finished on PAED-001, newest encounter first: the MSF row is listed, the rest are not");

                var all = await new ListActivitiesBySubjectQueryHandler(db, names)
                    .Handle(new ListActivitiesBySubjectQuery(TraineeId, trainee), CancellationToken.None);
                all.TotalCount.Should().Be(6);
                all.Items.Single(item => item.Id == completed).DisplayName
                    .Should().Be(onPaed001.Items.Single(item => item.Id == completed).DisplayName, "named alike, filter or none");
                all.Items.Select(item => item.Id).Should().Contain([declined, returned, inHand]);
            }
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    private static int AddType(ApplicationDbContext db, string key, string name, bool systemManaged = false)
    {
        var workflowJson = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Activities", "Seeds", key, "workflow.json"));
        var publishedOn = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var type = new ActivityType
        {
            Key = $"t355_{key}",
            Name = name,
            Scope = ActivityScope.Global,
            Version = 1,
            IsActive = true,
            SystemManaged = systemManaged,
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
        int institutionId,
        string dataJson,
        int epaId,
        DateOnly observedOn,
        params ActivityTransition[] moves)
    {
        var at = moves.Max(move => move.OccurredOn);
        var activity = new Activity
        {
            ActivityTypeId = typeId,
            SchemaVersion = 1,
            SubjectUserId = TraineeId,
            CreatedByUserId = TraineeId,
            CurrentState = state,
            DataJson = dataJson,
            InstitutionId = institutionId,
            EpaId = epaId,
            CreatedOn = at.AddDays(-1),
            UpdatedOn = at,
            ObservedOn = observedOn,
            ObservedOnSource = ObservationDateSource.Declared
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

    private static ActivityTransition Credited(ActivityTransition move, int count)
    {
        move.CreditedItemCount = count;
        return move;
    }

    private static ClaimsPrincipal Principal(string userId, string[] roles, int institutionId)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(WombatClaimTypes.InstitutionId, institutionId.ToString(System.Globalization.CultureInfo.InvariantCulture))
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }

    private static ApplicationDbContext NewContext(string schema)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(TestDatabase.SchemaConnectionString(schema))
            .Options);

    /// <summary>A clock that always reads <paramref name="now" /> (UTC).</summary>
    private sealed class FixedClock(DateTime now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(DateTime.SpecifyKind(now, DateTimeKind.Utc));
    }
}
