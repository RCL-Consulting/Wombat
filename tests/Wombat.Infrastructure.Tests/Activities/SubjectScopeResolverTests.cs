using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Services;
using Wombat.Domain.Activities;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Activities;
using Wombat.Infrastructure.Identity;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Infrastructure.Tests.Activities;

/// <summary>
/// <see cref="SubjectScopeResolver" />: "where does this subject train?", moved out of <see cref="ActivityService" /> by
/// T102 so the create page's nominee picker and the stamp the create writes are one answer.
/// </summary>
/// <remarks>
/// <para>
/// The rules are T101's and did not change with the move: the active trainee profile, else the most recent one (highest
/// id); each level of profile → curriculum → sub-speciality → speciality degrading on its own, so the institution survives
/// a missing curriculum; and, for a subject with no profile at all, their own identity row, with a speciality or
/// sub-speciality only when they hold exactly one.
/// </para>
/// <para>
/// Every case is asserted twice: against the resolver, and against the stamp <see cref="ActivityService.CreateDraftAsync" />
/// writes. The second is what "the same answer ActivityService stamped before" means in practice, and the first is what
/// the picker reads.
/// </para>
/// </remarks>
public sealed class SubjectScopeResolverTests
{
    private const string SubjectId = "subject-1";
    private const int ScopeTypeId = 900;

    // Speciality 1 → sub-speciality 11 → curriculum 100; speciality 2 → sub-speciality 21 → curriculum 200. Curriculum 300
    // points at a sub-speciality that does not exist, and curriculum 999 does not exist at all.
    private const int PaediatricsCurriculumId = 100;
    private const int SurgeryCurriculumId = 200;
    private const int OrphanedCurriculumId = 300;
    private const int MissingCurriculumId = 999;

    public static TheoryData<string, int?, int?, int?> Cases => new()
    {
        { "the active profile, over a newer inactive one", 10, 1, 11 },
        { "the newest profile, when none is active", 20, 2, 21 },
        { "the newest active profile, when several are active", 20, 2, 21 },
        { "a profile, over the subject's own identity row", 10, 1, 11 },
        { "a profile whose curriculum is missing keeps its institution", 10, null, null },
        { "a profile whose sub-speciality is missing keeps the sub-speciality id", 10, null, 31 },
        { "no profile: the identity row, with its one speciality and one sub-speciality", 30, 1, 11 },
        { "no profile: two specialities are no answer, the one sub-speciality still is", 30, null, 11 },
        { "no profile: two sub-specialities are no answer, the one speciality still is", 30, 1, null },
        { "no profile: an identity row with no institution and no scopes", null, null, null },
        { "no profile and no identity row", null, null, null }
    };

    [Theory]
    [MemberData(nameof(Cases))]
    public async Task TheResolverGivesT101sAnswer(string @case, int? institutionId, int? specialityId, int? subSpecialityId)
    {
        await using var db = CreateDb();
        await SeedAsync(db, @case);

        var resolved = await SubjectScopeResolver.ResolveAsync(db, SubjectId, CancellationToken.None);

        resolved.Should().Be((institutionId, specialityId, subSpecialityId), @case);
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public async Task TheCreateStampsExactlyWhatTheResolverAnswers(string @case, int? institutionId, int? specialityId, int? subSpecialityId)
    {
        await using var db = CreateDb();
        await SeedAsync(db, @case);

        var created = await Service(db).CreateDraftAsync(
            new CreateActivityInput(ScopeTypeId, SubjectId, SubjectId, """{ "title": "Stamped" }""", Principal(SubjectId)));

        var stored = await db.Activities.AsNoTracking().SingleAsync(activity => activity.Id == created.Id);
        (stored.InstitutionId, stored.SpecialityId, stored.SubSpecialityId)
            .Should().Be((institutionId, specialityId, subSpecialityId), @case);
    }

    [Fact]
    public async Task TheCreateTrimsTheSubjectBeforeResolving_SoAPaddedIdGetsTheSameStamp()
    {
        // ActivityService trims the subject id before it resolves; the picker does too. A padded id must not fall
        // through to the identity fallback, or to nothing.
        await using var db = CreateDb();
        await SeedAsync(db, "the active profile, over a newer inactive one");

        var created = await Service(db).CreateDraftAsync(
            new CreateActivityInput(ScopeTypeId, $"  {SubjectId} ", SubjectId, """{ "title": "Stamped" }""", Principal(SubjectId)));

        var stored = await db.Activities.AsNoTracking().SingleAsync(activity => activity.Id == created.Id);
        stored.SubjectUserId.Should().Be(SubjectId);
        (stored.InstitutionId, stored.SpecialityId, stored.SubSpecialityId).Should().Be(((int?)10, (int?)1, (int?)11));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ABlankSubjectResolvesToNothing(string subjectUserId)
    {
        await using var db = CreateDb();
        await SeedAsync(db, "no profile: the identity row, with its one speciality and one sub-speciality");

        var resolved = await SubjectScopeResolver.ResolveAsync(db, subjectUserId, CancellationToken.None);

        resolved.Should().Be(((int?)null, (int?)null, (int?)null));
    }

    [Fact]
    public async Task TheCreatePagePickerAndTheCreateGate_FollowTheSameProfile_NotTheIdentityRow()
    {
        // Why the resolver was extracted. The subject's identity row says institution 20 and a newer, inactive profile
        // says 20 too, but the ACTIVE profile is at 10. The picker must offer institution 10's assessors, the create must
        // stamp 10, and the gate must accept the one the picker offered and refuse the one it did not.
        await using var db = CreateDb();
        await SeedAsync(db, "the active profile, over a newer inactive one");
        db.Users.Single(user => user.Id == SubjectId).InstitutionId = 20;
        NomineeSeed.AddUser(db, "assessor-at-10", 10, WombatRoles.Assessor);
        NomineeSeed.AddUser(db, "assessor-at-20", 20, WombatRoles.Assessor);
        await db.SaveChangesAsync();

        var offered = await new ActivityReferenceDataService(db).GetNomineeOptionsAsync(
            new NomineeOptionScope(SubjectId, [WombatRoles.Assessor], ForExistingActivity: false, ActivityInstitutionId: null, StoredValue: null));

        offered.Select(option => option.Value).Should().Equal("assessor-at-10");

        var created = await Service(db).CreateDraftAsync(new CreateActivityInput(
            NomineeTypeId, SubjectId, SubjectId, """{ "assessor_user_id": "assessor-at-10" }""", Principal(SubjectId)));
        created.DataJson.Should().Contain("assessor-at-10");
        (await db.Activities.AsNoTracking().SingleAsync(activity => activity.Id == created.Id)).InstitutionId.Should().Be(10);

        var refused = () => Service(db).CreateDraftAsync(new CreateActivityInput(
            NomineeTypeId, SubjectId, SubjectId, """{ "assessor_user_id": "assessor-at-20" }""", Principal(SubjectId)));
        await refused.Should().ThrowAsync<InvalidOperationException>().WithMessage("*cannot be named here*");
    }

    // ---- fixture --------------------------------------------------------------------------------------------------

    private const int NomineeTypeId = 901;

    private static async Task SeedAsync(ApplicationDbContext db, string @case)
    {
        db.Set<Speciality>().AddRange(
            new Speciality { Id = 1, CollegeId = 1, Name = "Paediatrics" },
            new Speciality { Id = 2, CollegeId = 1, Name = "Surgery" });
        db.Set<SubSpeciality>().AddRange(
            new SubSpeciality { Id = 11, SpecialityId = 1, Name = "General Paediatrics" },
            new SubSpeciality { Id = 21, SpecialityId = 2, Name = "General Surgery" });
        db.Set<Curriculum>().AddRange(
            new Curriculum { Id = PaediatricsCurriculumId, SubSpecialityId = 11, Name = "Paediatrics", Version = "1" },
            new Curriculum { Id = SurgeryCurriculumId, SubSpecialityId = 21, Name = "Surgery", Version = "1" },
            new Curriculum { Id = OrphanedCurriculumId, SubSpecialityId = 31, Name = "Orphaned", Version = "1" });

        db.ActivityTypes.AddRange(
            Type(ScopeTypeId, "scope_under_test", TitleSchemaJson),
            Type(NomineeTypeId, "nominee_scope_under_test", NomineeSchemaJson));

        switch (@case)
        {
            case "the active profile, over a newer inactive one":
                AddIdentity(db, institutionId: 10);
                AddProfile(db, 5, 10, PaediatricsCurriculumId, isActive: true);
                AddProfile(db, 9, 20, SurgeryCurriculumId, isActive: false);
                break;

            case "the newest profile, when none is active":
                AddProfile(db, 5, 10, PaediatricsCurriculumId, isActive: false);
                AddProfile(db, 9, 20, SurgeryCurriculumId, isActive: false);
                break;

            case "the newest active profile, when several are active":
                AddProfile(db, 5, 10, PaediatricsCurriculumId, isActive: true);
                AddProfile(db, 9, 20, SurgeryCurriculumId, isActive: true);
                AddProfile(db, 12, 30, PaediatricsCurriculumId, isActive: false);
                break;

            case "a profile, over the subject's own identity row":
                AddIdentity(db, institutionId: 20, specialityIds: [2], subSpecialityIds: [21]);
                AddProfile(db, 5, 10, PaediatricsCurriculumId, isActive: true);
                break;

            case "a profile whose curriculum is missing keeps its institution":
                AddIdentity(db, institutionId: 20, specialityIds: [2], subSpecialityIds: [21]);
                AddProfile(db, 5, 10, MissingCurriculumId, isActive: true);
                break;

            case "a profile whose sub-speciality is missing keeps the sub-speciality id":
                AddProfile(db, 5, 10, OrphanedCurriculumId, isActive: true);
                break;

            case "no profile: the identity row, with its one speciality and one sub-speciality":
                AddIdentity(db, institutionId: 30, specialityIds: [1], subSpecialityIds: [11]);
                break;

            case "no profile: two specialities are no answer, the one sub-speciality still is":
                AddIdentity(db, institutionId: 30, specialityIds: [1, 2], subSpecialityIds: [11]);
                break;

            case "no profile: two sub-specialities are no answer, the one speciality still is":
                AddIdentity(db, institutionId: 30, specialityIds: [1], subSpecialityIds: [11, 21]);
                break;

            case "no profile: an identity row with no institution and no scopes":
                AddIdentity(db, institutionId: null);
                break;

            case "no profile and no identity row":
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(@case), @case, "No such case.");
        }

        await db.SaveChangesAsync();
    }

    private static void AddIdentity(
        ApplicationDbContext db,
        int? institutionId,
        int[]? specialityIds = null,
        int[]? subSpecialityIds = null)
    {
        NomineeSeed.AddUser(db, SubjectId, institutionId, WombatRoles.Trainee);

        foreach (var specialityId in specialityIds ?? [])
        {
            db.UserSpecialityScopes.Add(new WombatIdentityUserSpecialityScope { UserId = SubjectId, SpecialityId = specialityId });
        }

        foreach (var subSpecialityId in subSpecialityIds ?? [])
        {
            db.UserSubSpecialityScopes.Add(new WombatIdentityUserSubSpecialityScope { UserId = SubjectId, SubSpecialityId = subSpecialityId });
        }
    }

    private static void AddProfile(ApplicationDbContext db, int id, int institutionId, int curriculumId, bool isActive)
        => db.TraineeProfiles.Add(new TraineeProfile
        {
            Id = id,
            UserId = SubjectId,
            InstitutionId = institutionId,
            CurriculumId = curriculumId,
            ProgrammeStartDate = new DateOnly(2025, 4, 14),
            ExpectedCompletionDate = new DateOnly(2029, 4, 13),
            IsActive = isActive
        });

    private static ActivityService Service(ApplicationDbContext db)
        => new(db, new SchemaValidator(), new WorkflowEvaluator(), new CreditApplier(db), new FieldPermissionEvaluator());

    private static ApplicationDbContext CreateDb()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static ClaimsPrincipal Principal(string userId)
        => new(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId)], "test"));

    private const string TitleSchemaJson = """
        {
          "version": 1,
          "sections": [
            { "key": "main", "title": "Main", "fields": [ { "key": "title", "type": "text", "label": "Title" } ] }
          ]
        }
        """;

    private const string NomineeSchemaJson = """
        {
          "version": 1,
          "sections": [
            { "key": "main", "title": "Main", "fields": [ { "key": "assessor_user_id", "type": "user", "label": "Assessor" } ] }
          ]
        }
        """;

    private const string WorkflowJson = """
        {
          "version": 1,
          "initial_state": "draft",
          "states": [
            { "key": "draft", "label": "Draft" },
            { "key": "done", "label": "Done", "terminal": true }
          ],
          "transitions": [
            { "key": "finish", "from": "draft", "to": "done", "actor": "subject" }
          ]
        }
        """;

    /// <summary>Global, so the create never asks whether the subject is in the type's scope: only the stamp is under test.</summary>
    private static ActivityType Type(int id, string key, string schemaJson)
    {
        var publishedOn = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        const string creditRulesJson = """{ "counts_for": [] }""";

        var activityType = new ActivityType
        {
            Id = id,
            Key = key,
            Name = key,
            Scope = ActivityScope.Global,
            Version = 1,
            SchemaJson = schemaJson,
            WorkflowJson = WorkflowJson,
            CreditRulesJson = creditRulesJson,
            DisplayFieldsJson = "[]",
            OwnerUserId = "admin-1",
            CreatedOn = publishedOn
        };

        activityType.Versions.Add(new ActivityTypeVersion
        {
            ActivityTypeId = id,
            Version = 1,
            SchemaJson = schemaJson,
            WorkflowJson = WorkflowJson,
            CreditRulesJson = creditRulesJson,
            DisplayFieldsJson = "[]",
            PublishedByUserId = "admin-1",
            PublishedOn = publishedOn
        });

        return activityType;
    }
}
