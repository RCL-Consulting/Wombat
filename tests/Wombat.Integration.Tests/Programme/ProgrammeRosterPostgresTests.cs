using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Programme.Filing;
using Wombat.Application.Features.Programme.Trainees;
using Wombat.Domain.Activities;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Persistence;
using Wombat.Integration.Tests.TestSupport;
using Wombat.Tests.Shared;

namespace Wombat.Integration.Tests.Programme;

/// <summary>
/// T358 (flow 06, lane A2) on a real PostgreSQL server: Programme trainees' roster read and the filing rule (E5), which
/// the unit suites run on EF InMemory. Here each query must translate: the scope's profiles (the sub-speciality through the
/// curriculum), the preferred-profile test, the coverage reader's items and rows, and the filing moments' projection of
/// each activity's moves.
/// </summary>
/// <remarks>
/// A migrated schema of its own (<c>it_&lt;guid&gt;</c>), seeded the way startup seeds it (DataSeeder, then the paediatric
/// catalogue), as <c>AcademicPeriodQuotaPostgresTests</c> does; dropped on dispose.
/// </remarks>
public sealed class ProgrammeRosterPostgresTests : IAsyncLifetime
{
    private const string PaediatricCurriculumName = "Paediatric EPA Curriculum";
    private static readonly DateOnly D = new(2026, 10, 4);
    private static readonly DateTime DAt10 = new(2026, 10, 4, 8, 0, 0, DateTimeKind.Utc);

    private readonly TestSchemas _schemas = new();

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _schemas.DropAllAsync();

    [Fact]
    public async Task TheRoster_AndNothingFiled_OnPostgres()
    {
        var schema = await SeededSchemaAsync();

        int institutionId, subSpecialityId, paed002, mahlanguProfile;
        await using (var db = NewContext(schema))
        {
            institutionId = await db.Institutions.Where(entity => entity.ShortCode == "DEMO").Select(entity => entity.Id).SingleAsync();
            var curriculum = await db.Curricula
                .Where(entity => entity.Name == PaediatricCurriculumName)
                .Select(entity => new { entity.Id, entity.SubSpecialityId })
                .SingleAsync();
            subSpecialityId = curriculum.SubSpecialityId;
            var items = await db.CurriculumItems
                .Where(item => item.CurriculumId == curriculum.Id && item.OwningInstitutionId == null)
                .Select(item => new { item.Id, item.EpaId, item.Epa.Code })
                .ToListAsync();
            paed002 = items.Single(item => item.Code == "PAED-002").EpaId;

            var mahlangu = Profile("t358-mahlangu", institutionId, curriculum.Id, admittedOn: new DateOnly(2026, 1, 15));
            db.TraineeProfiles.AddRange(
                mahlangu,
                Profile("t358-ndlovu", institutionId, curriculum.Id, admittedOn: new DateOnly(2026, 1, 15)),
                // Admitted ten days ago: not yet 30 days to file anything (E5, D1).
                Profile("t358-new", institutionId, curriculum.Id, admittedOn: D.AddDays(-10)));

            db.CurriculumItemProgresses.Add(new CurriculumItemProgress
            {
                CurriculumItemId = items.Single(item => item.Code == "PAED-002").Id,
                TraineeUserId = "t358-mahlangu",
                AcademicYear = 2026,
                Semester = 2,
                CountsSoFar = 1,
                MinimumLevelReachedCount = 1,
                LastObservedOn = new DateOnly(2026, 9, 30),
                LastUpdated = DAt10,
                CreditedActivityKeysJson = "[]"
            });

            var miniCex = AddType(db, "mini_cex_cpsa");
            var journalClub = AddType(db, "journal_club");
            await db.SaveChangesAsync();

            // Mahlangu: a Mini-CEX submitted three days ago. Ndlovu: a journal club logged 40 days ago, and a draft this week.
            Add(db, miniCex, "t358-mahlangu", "requested", institutionId, DAt10.AddDays(-4),
                Move("create", "draft", "draft", "t358-mahlangu", DAt10.AddDays(-4)),
                Move("submit", "draft", "requested", "t358-mahlangu", DAt10.AddDays(-3)));
            Add(db, journalClub, "t358-ndlovu", "logged", institutionId, DAt10.AddDays(-40),
                Move("create", "logged", "logged", "t358-ndlovu", DAt10.AddDays(-40)));
            Add(db, miniCex, "t358-ndlovu", "draft", institutionId, DAt10.AddDays(-1),
                Move("create", "draft", "draft", "t358-ndlovu", DAt10.AddDays(-1)));
            await db.SaveChangesAsync();
            mahlanguProfile = mahlangu.Id;
        }

        var users = new FakeUserDirectory()
            .With(Trainee("t358-mahlangu", "Nomsa", "Mahlangu", institutionId))
            .With(Trainee("t358-ndlovu", "Sipho", "Ndlovu", institutionId))
            .With(Trainee("t358-new", "Thando", "Newman", institutionId));

        await using (var db = NewContext(schema))
        {
            var filed = await FilingMoments.LastFiledAsync(db, ["t358-mahlangu", "t358-ndlovu", "t358-new"], CancellationToken.None);
            filed.Keys.Should().BeEquivalentTo(["t358-mahlangu", "t358-ndlovu"]);
            filed["t358-mahlangu"].Should().BeCloseTo(DAt10.AddDays(-3), TimeSpan.FromSeconds(1));
            filed["t358-ndlovu"].Should().BeCloseTo(DAt10.AddDays(-40), TimeSpan.FromSeconds(1), "a draft is not a filing");

            var coordinator = Principal("t358-coordinator", WombatRoles.Coordinator, institutionId);
            var roster = await new ListProgrammeTraineesQueryHandler(db, users, TimeProvider.System).Handle(
                new ListProgrammeTraineesQuery(coordinator, WombatRoles.Coordinator, AsOf: D), CancellationToken.None);

            roster!.Read.Rows.Select(row => row.Name).Should().Equal("Nomsa Mahlangu", "Sipho Ndlovu", "Thando Newman");
            roster.Read.CurrentCount.Should().Be(3);
            roster.Read.Rows.Should().OnlyContain(row => row.SemesterApplying == 10 && row.YearApplying == 5);

            var nothingFiled = await new ListProgrammeTraineesQueryHandler(db, users, TimeProvider.System).Handle(
                new ListProgrammeTraineesQuery(coordinator, WombatRoles.Coordinator, NothingFiled: true, AsOf: D), CancellationToken.None);
            nothingFiled!.Read.Rows.Select(row => row.Name).Should().Equal("Sipho Ndlovu");

            var subSpecialityAdmin = Principal("t358-ssa", WombatRoles.SubSpecialityAdmin, institutionId, subSpecialityId);
            var shortOn = await new ListProgrammeTraineesQueryHandler(db, users, TimeProvider.System).Handle(
                new ListProgrammeTraineesQuery(subSpecialityAdmin, WombatRoles.SubSpecialityAdmin, ShortOnEpaId: paed002, AsOf: D),
                CancellationToken.None);
            shortOn!.Read.Rows.Select(row => (row.Name, row.ShortOn!.Count)).Should().Equal(
                ("Sipho Ndlovu", 0), ("Thando Newman", 0), ("Nomsa Mahlangu", 1));

            var registrar = await new GetProgrammeTraineeQueryHandler(db, users, TimeProvider.System).Handle(
                new GetProgrammeTraineeQuery(coordinator, WombatRoles.Coordinator, mahlanguProfile, D), CancellationToken.None);
            registrar!.Name.Should().Be("Nomsa Mahlangu");
            registrar.SubSpecialityName.Should().NotBeEmpty();
        }
    }

    private static TraineeProfile Profile(string userId, int institutionId, int curriculumId, DateOnly admittedOn) => new()
    {
        UserId = userId,
        InstitutionId = institutionId,
        CurriculumId = curriculumId,
        ProgrammeStartDate = new DateOnly(2026, 1, 15),
        AdmittedOn = admittedOn,
        ExpectedCompletionDate = new DateOnly(2030, 1, 14),
        IsActive = true
    };

    private static UserIdentityDetails Trainee(string userId, string firstName, string lastName, int institutionId)
        => new(userId, $"{userId}@test.local", firstName, lastName, institutionId, [], [], [WombatRoles.Trainee]);

    private static int AddType(ApplicationDbContext db, string key)
    {
        var workflowJson = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Activities", "Seeds", key, "workflow.json"));
        var publishedOn = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var type = new ActivityType
        {
            Key = $"t358_{key}",
            Name = key,
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

    private static void Add(
        ApplicationDbContext db, int typeId, string subject, string state, int institutionId, DateTime createdOn,
        params ActivityTransition[] moves)
    {
        var activity = new Activity
        {
            ActivityTypeId = typeId,
            SchemaVersion = 1,
            SubjectUserId = subject,
            CreatedByUserId = subject,
            CurrentState = state,
            DataJson = """{ "assessor_user_id": "t358-assessor" }""",
            InstitutionId = institutionId,
            CreatedOn = createdOn,
            UpdatedOn = moves[^1].OccurredOn,
            ObservedOn = DateOnly.FromDateTime(createdOn)
        };
        foreach (var move in moves)
        {
            activity.Transitions.Add(move);
        }

        db.Activities.Add(activity);
    }

    private static ActivityTransition Move(string key, string from, string to, string actor, DateTime occurredOn) => new()
    {
        FromState = from, ToState = to, TransitionKey = key, ActorUserId = actor, OccurredOn = occurredOn, SnapshotJson = "{}"
    };

    private static ClaimsPrincipal Principal(string userId, string role, int institutionId, int? subSpecialityId = null)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Role, role),
            new(WombatClaimTypes.InstitutionId, institutionId.ToString(System.Globalization.CultureInfo.InvariantCulture))
        };
        if (subSpecialityId is int subSpeciality)
        {
            claims.Add(new Claim(WombatClaimTypes.SubSpecialityId, subSpeciality.ToString(System.Globalization.CultureInfo.InvariantCulture)));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }

    private async Task<string> SeededSchemaAsync()
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

        return schema;
    }

    private static ApplicationDbContext NewContext(string schema)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(TestDatabase.SchemaConnectionString(schema))
            .Options);
}
