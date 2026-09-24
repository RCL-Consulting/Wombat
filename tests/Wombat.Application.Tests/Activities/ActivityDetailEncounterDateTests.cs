using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Domain.Activities;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Activities;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.Activities;

/// <summary>
/// T161, D28: the activity page's DTO carries the stamped encounter date and whether anyone stated it, so the page can
/// mark a filing day as one. Created and read back through <c>ActivityService</c>, as <c>/activities/{id}</c> reads it.
/// </summary>
/// <remarks>
/// The undated type is the shipped <c>reflective_note</c> seed, which declares no <c>observation_date_field</c>. The
/// dated one is the same form with a date field and the pointer at it, so the two differ only in that.
/// </remarks>
public sealed class ActivityDetailEncounterDateTests
{
    private const string TraineeId = "trainee-1";
    private const int InstitutionId = 10;
    private const int UndatedTypeId = 1;
    private const int DatedTypeId = 2;

    [Fact]
    public async Task AnActivityOfATypeWithNoDatePointer_CarriesItsFilingDay_MarkedAsUndated()
    {
        var options = await SeededAsync();

        var detail = await CreateAndReadAsync(options, UndatedTypeId, """{ "situation": "A long night on the ward." }""");

        detail.ObservedOnDeclared.Should().BeFalse("nobody stated when it happened");
        detail.ObservedOn.Should().Be(ProgrammeCalendar.DateOf(detail.CreatedOn), "the fallback is the South African filing day");
    }

    [Fact]
    public async Task AnActivityWithAStatedEncounterDate_CarriesThatDate_AsDeclared()
    {
        var options = await SeededAsync();

        var detail = await CreateAndReadAsync(
            options, DatedTypeId, """{ "situation": "A long night on the ward.", "observed_on": "2026-03-10" }""");

        detail.ObservedOnDeclared.Should().BeTrue();
        detail.ObservedOn.Should().Be(new DateOnly(2026, 3, 10));
    }

    private static async Task<ActivityDto> CreateAndReadAsync(
        DbContextOptions<ApplicationDbContext> options, int typeId, string dataJson)
    {
        int id;
        await using (var db = new ApplicationDbContext(options))
        {
            id = (await Service(db).CreateDraftAsync(
                new CreateActivityInput(typeId, TraineeId, TraineeId, dataJson, Principal(TraineeId)))).Id;
        }

        await using var read = new ApplicationDbContext(options);
        var detail = await Service(read).GetDetailAsync(id, Principal(TraineeId));
        return detail!.Activity;
    }

    private static async Task<DbContextOptions<ApplicationDbContext>> SeededAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var db = new ApplicationDbContext(options);

        NomineeSeed.AddUser(db, TraineeId, InstitutionId, WombatRoles.Trainee);
        db.Set<TraineeProfile>().Add(new TraineeProfile
        {
            Id = 1,
            UserId = TraineeId,
            InstitutionId = InstitutionId,
            CurriculumId = 1,
            ProgrammeStartDate = new DateOnly(2026, 1, 12),
            ExpectedCompletionDate = new DateOnly(2030, 1, 12),
            IsActive = true
        });

        string Read(string file) => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Activities", "Seeds", "reflective_note", file));
        var schemaJson = Read("schema.json");
        schemaJson.Should().NotContain("observation_date_field", "this fixture is the seed that declares no date");

        var datedSchemaJson = schemaJson
            .Replace("\"version\": 1,", "\"version\": 1,\n  \"observation_date_field\": \"observed_on\",", StringComparison.Ordinal)
            .Replace(
                "{ \"key\": \"situation\",",
                "{ \"key\": \"observed_on\", \"type\": \"date\", \"label\": \"Encounter date\" },\n        { \"key\": \"situation\",",
                StringComparison.Ordinal);

        db.ActivityTypes.AddRange(
            Type(UndatedTypeId, "reflective_note", schemaJson, Read("workflow.json"), Read("credit.json")),
            Type(DatedTypeId, "dated_reflection", datedSchemaJson, Read("workflow.json"), Read("credit.json")));

        await db.SaveChangesAsync();
        return options;
    }

    private static ActivityType Type(int id, string key, string schemaJson, string workflowJson, string creditJson)
    {
        var type = new ActivityType
        {
            Id = id,
            Key = key,
            Name = key,
            Scope = ActivityScope.Global,
            Version = 1,
            SchemaJson = schemaJson,
            WorkflowJson = workflowJson,
            CreditRulesJson = creditJson,
            DisplayFieldsJson = "[]",
            OwnerUserId = "system",
            CreatedOn = DateTime.UtcNow
        };
        type.Versions.Add(new ActivityTypeVersion
        {
            ActivityTypeId = id,
            Version = 1,
            SchemaJson = schemaJson,
            WorkflowJson = workflowJson,
            CreditRulesJson = creditJson,
            DisplayFieldsJson = "[]",
            PublishedByUserId = "system",
            PublishedOn = DateTime.UtcNow
        });
        return type;
    }

    private static ActivityService Service(ApplicationDbContext db)
        => new(db, new SchemaValidator(), new WorkflowEvaluator(), new CreditApplier(db), new FieldPermissionEvaluator());

    private static ClaimsPrincipal Principal(string userId)
        => new(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId)], "test"));
}
