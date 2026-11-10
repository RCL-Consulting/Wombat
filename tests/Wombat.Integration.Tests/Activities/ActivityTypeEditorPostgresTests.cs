using System.Globalization;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Queries.GetActivityTypeEditor;
using Wombat.Domain.Activities;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;
using Wombat.Integration.Tests.TestSupport;
using Wombat.Tests.Shared;

namespace Wombat.Integration.Tests.Activities;

/// <summary>
/// The builder's read of a type on a real PostgreSQL server (T398, flow 17, lane A; BRIEF § 11, flow 16): the owning
/// College's short code, the people named in one lookup, and the preview's EPAs by the type's scope, each of whose
/// queries must translate (a navigation to the speciality; two nested <c>Contains</c> subqueries for an institution's
/// adopted curricula). The unit suite (<c>ActivityTypeEditorReadTests</c>) runs on EF InMemory, which translates nothing.
/// </summary>
public sealed class ActivityTypeEditorPostgresTests : IAsyncLifetime
{
    private readonly TestSchemas _schemas = new();

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _schemas.DropAllAsync();

    [Fact]
    public async Task TheOwnerTheNamesAndThePreviewsEpas_ReadOnPostgres()
    {
        try
        {
            var schema = await _schemas.CreateAsync();
            await using (var db = NewContext(schema))
            {
                await db.Database.MigrateAsync();
            }

            int kgk, specialityType, subSpecialityType, institutionType;
            await using (var db = NewContext(schema))
            {
                var hospital = new Institution { Name = "T398 Kgosi Kgari", ShortCode = "T398KGK", IsActive = true, CreatedOn = DateTime.UtcNow };
                var other = new Institution { Name = "T398 Other", ShortCode = "T398OTH", IsActive = true, CreatedOn = DateTime.UtcNow };
                var college = new College { Name = "T398 College of Paediatricians", ShortCode = "T398CPSA", IsActive = true };
                var demo = new College { Name = "T398 Demo College", ShortCode = "T398DEMO", IsActive = true };
                var paeds = new Speciality { College = college, Name = "T398 Paediatrics", IsActive = true };
                var general = new SubSpeciality { Speciality = paeds, Name = "General", IsActive = true };
                var medicine = new Speciality { College = demo, Name = "T398 Medicine", IsActive = true };
                var medicineSub = new SubSpeciality { Speciality = medicine, Name = "General", IsActive = true };
                db.AddRange(hospital, other, college, demo, paeds, general, medicine, medicineSub);
                await db.SaveChangesAsync();
                kgk = hospital.Id;

                var paed002 = new Epa { SubSpecialityId = general.Id, Code = "T398-P002", Title = "Second" };
                var paed001 = new Epa { SubSpecialityId = general.Id, Code = "T398-P001", Title = "First" };
                var paused = new Epa { SubSpecialityId = general.Id, Code = "T398-P003", Title = "Paused" };
                paused.Deactivate(DateTime.UtcNow);
                var own = new Epa { SubSpecialityId = general.Id, OwningInstitutionId = hospital.Id, Code = "T398-K001", Title = "KGK's own" };
                var another = new Epa { SubSpecialityId = general.Id, OwningInstitutionId = other.Id, Code = "T398-O001", Title = "Another's own" };
                var demoEpa = new Epa { SubSpecialityId = medicineSub.Id, Code = "T398-D001", Title = "Demo" };
                db.Epas.AddRange(paed002, paed001, paused, own, another, demoEpa);

                var curriculum = new Curriculum { SubSpecialityId = general.Id, Name = "T398 Paediatrics", Version = "1", EffectiveFrom = new DateOnly(2026, 1, 1) };
                db.Curricula.Add(curriculum);
                await db.SaveChangesAsync();

                db.CurriculumItems.AddRange(
                    Item(curriculum.Id, paed001.Id, null),
                    Item(curriculum.Id, paused.Id, null),
                    Item(curriculum.Id, own.Id, hospital.Id),
                    Item(curriculum.Id, another.Id, other.Id));
                db.InstitutionCurriculumAdoptions.Add(new InstitutionCurriculumAdoption
                {
                    InstitutionId = hospital.Id, CurriculumId = curriculum.Id, SubSpecialityId = general.Id,
                    AdoptedOn = new DateOnly(2026, 1, 1), IsActive = true
                });

                specialityType = AddType(db, "t398_speciality", ActivityScope.Speciality, paeds.Id, ActivityTypeSeedOwnership.SeederUserId);
                subSpecialityType = AddType(db, "t398_sub", ActivityScope.SubSpeciality, general.Id, "t398-nolwazi");
                institutionType = AddType(db, "t398_institution", ActivityScope.Institution, hospital.Id, "t398-nolwazi");
                await db.SaveChangesAsync();
            }

            var users = new FakeUserDirectory(("t398-nolwazi", "Nolwazi Mbatha"));
            var administrator = Principal([WombatRoles.Administrator], kgk);

            await using (var db = NewContext(schema))
            {
                var speciality = await ReadAsync(db, users, specialityType, administrator);
                speciality.Owner.Should().Be(new ActivityTypeOwnerDto(ActivityTypeOwnerKind.College, "T398CPSA"));
                speciality.PreviewEpaOptions!.Select(option => option.Label).Should().Equal("T398-P001 — First", "T398-P002 — Second");
                speciality.Versions.Single().PublishedByName.Should().Be("Wombat's catalogue");
                speciality.IsSeedOwned.Should().BeTrue();

                var subSpeciality = await ReadAsync(db, users, subSpecialityType, administrator);
                subSpeciality.Owner.Should().Be(new ActivityTypeOwnerDto(ActivityTypeOwnerKind.College, "T398CPSA"));
                subSpeciality.PreviewEpaOptions!.Select(option => option.Label).Should().Equal("T398-P001 — First", "T398-P002 — Second");
                subSpeciality.Versions.Single().PublishedByName.Should().Be("Nolwazi Mbatha");

                var institution = await ReadAsync(db, users, institutionType, Principal([WombatRoles.InstitutionalAdmin], kgk));
                institution.Owner.Should().Be(new ActivityTypeOwnerDto(ActivityTypeOwnerKind.Institution, "T398 Kgosi Kgari"));
                institution.PreviewEpaOptions!.Select(option => option.Label).Should().Equal("T398-K001 — KGK's own", "T398-P001 — First");
                institution.WorkflowRead!.Moves.Single().Who.Should().Be("The registrar");
                institution.DraftWorkflowJson.Should().StartWith("{\n  \"version\": 1,");
            }
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    private static async Task<ActivityTypeEditorDto> ReadAsync(ApplicationDbContext db, FakeUserDirectory users, int id, ClaimsPrincipal principal)
        => (await new GetActivityTypeEditorQueryHandler(db, users).Handle(new GetActivityTypeEditorQuery(id, principal), CancellationToken.None))!;

    private static CurriculumItem Item(int curriculumId, int epaId, int? owningInstitutionId) => new()
    {
        CurriculumId = curriculumId, EpaId = epaId, OwningInstitutionId = owningInstitutionId, RequiredCount = 1,
        QuotaPeriod = QuotaPeriod.AcademicYear, MinimumLevelOrder = 3, WindowMonths = 12
    };

    private static int AddType(ApplicationDbContext db, string key, ActivityScope scope, int scopeId, string publisher)
    {
        var type = new ActivityType
        {
            Key = key,
            Name = key,
            Scope = scope,
            ScopeId = scopeId,
            OwnerUserId = publisher,
            CreatedOn = DateTime.UtcNow
        };
        type.SaveDraft(SchemaJson, WorkflowJson, CreditRulesJson, "[]", publisher);
        type.PublishDraft(publisher);
        db.ActivityTypes.Add(type);
        db.SaveChanges();
        return type.Id;
    }

    private static ClaimsPrincipal Principal(string[] roles, int institutionId)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "t398-caller"),
            new(WombatClaimTypes.InstitutionId, institutionId.ToString(CultureInfo.InvariantCulture))
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }

    private static ApplicationDbContext NewContext(string schema)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(TestDatabase.SchemaConnectionString(schema))
            .Options);

    private const string SchemaJson = """{ "version": 1, "sections": [ { "key": "details", "title": "Details", "fields": [ { "key": "title", "type": "text", "label": "Title", "required": true } ] } ] }""";

    private const string WorkflowJson = """{ "version": 1, "initial_state": "draft", "states": [ { "key": "draft", "label": "Draft" }, { "key": "submitted", "label": "Submitted", "terminal": true } ], "transitions": [ { "key": "submit", "from": "draft", "to": "submitted", "actor": "subject", "validation": "all" } ] }""";

    private const string CreditRulesJson = """{ "counts_for": [] }""";
}
