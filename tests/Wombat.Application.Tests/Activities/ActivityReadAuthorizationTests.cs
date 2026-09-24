using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Domain.Activities;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Activities;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.Activities;

/// <summary>
/// T101 — who may read an activity.
/// </summary>
/// <remarks>
/// Before T101 the entire read gate was a null check on the principal, so any authenticated user —
/// including a PendingTrainee holding no programme role — could read any activity's clinical data
/// and full transition history by putting an integer in the URL. These tests pin both halves: the
/// people who must be able to read, and the people who must not.
/// </remarks>
public sealed class ActivityReadAuthorizationTests
{
    private const int ActivityId = 700;
    private const int InstitutionId = 10;
    private const int SpecialityId = 20;
    private const int SubSpecialityId = 30;

    // ─── Admitted ────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("trainee-1")]     // the subject
    [InlineData("coordinator-1")] // the creator, who filed it on the trainee's behalf
    [InlineData("assessor-1")]    // the bound assessor, named by a field: rule
    public async Task TheNamedParticipantsCanRead(string userId)
    {
        await using var dbContext = CreateContext();
        SeedStampedActivity(dbContext);

        var view = await CreateService(dbContext).GetDetailAsync(ActivityId, Principal(userId));

        view.Should().NotBeNull();
        view!.Activity.Id.Should().Be(ActivityId);
    }

    /// <summary>
    /// The bound assessor is named by a <c>field:</c> rule, i.e. by DATA. If the trainee re-points
    /// that field at someone else, the assessor who already declined must not lose the record of
    /// their own decision — so a prior transition actor keeps their read.
    /// </summary>
    [Fact]
    public async Task APriorTransitionActorKeepsTheirReadAfterTheAssessorFieldMovesOn()
    {
        await using var dbContext = CreateContext();
        SeedStampedActivity(dbContext);

        var activity = dbContext.Activities.Include(entity => entity.Transitions).Single();
        activity.Transitions.Add(new ActivityTransition
        {
            FromState = "requested",
            ToState = "declined",
            TransitionKey = "decline",
            ActorUserId = "assessor-previous",
            OccurredOn = DateTime.UtcNow,
            SnapshotJson = activity.DataJson,
            Note = "Not my patient."
        });
        dbContext.SaveChanges();

        var view = await CreateService(dbContext).GetDetailAsync(ActivityId, Principal("assessor-previous"));

        view.Should().NotBeNull();
    }

    [Fact]
    public async Task AGlobalAdministratorCanReadAnything()
    {
        await using var dbContext = CreateContext();
        SeedStampedActivity(dbContext);

        var view = await CreateService(dbContext).GetDetailAsync(
            ActivityId,
            Principal("admin-1", institutionId: 999, roles: WombatRoles.Administrator));

        view.Should().NotBeNull();
    }

    [Theory]
    [InlineData(WombatRoles.InstitutionalAdmin)]
    [InlineData(WombatRoles.Coordinator)]
    [InlineData(WombatRoles.CommitteeMember)]
    public async Task InstitutionScopedOversightCanReadWithinItsOwnInstitution(string role)
    {
        await using var dbContext = CreateContext();
        SeedStampedActivity(dbContext);

        var view = await CreateService(dbContext).GetDetailAsync(
            ActivityId,
            Principal("overseer-1", institutionId: InstitutionId, roles: role));

        view.Should().NotBeNull();
    }

    [Fact]
    public async Task ASpecialityAdminOfTheSubjectsSpecialityAndInstitutionCanRead()
    {
        await using var dbContext = CreateContext();
        SeedStampedActivity(dbContext);

        var view = await CreateService(dbContext).GetDetailAsync(
            ActivityId,
            Principal("spec-admin-1", institutionId: InstitutionId, roles: WombatRoles.SpecialityAdmin, specialityId: SpecialityId));

        view.Should().NotBeNull();
    }

    [Fact]
    public async Task ASubSpecialityAdminOfTheSubjectsSubSpecialityAndInstitutionCanRead()
    {
        await using var dbContext = CreateContext();
        SeedStampedActivity(dbContext);

        var view = await CreateService(dbContext).GetDetailAsync(
            ActivityId,
            Principal("sub-admin-1", institutionId: InstitutionId, roles: WombatRoles.SubSpecialityAdmin, subSpecialityId: SubSpecialityId));

        view.Should().NotBeNull();
    }

    /// <summary>
    /// A <c>Speciality</c> is owned by a College, so its id is NATIONAL. Matching on the speciality
    /// claim alone would let one hospital's SpecialityAdmin read every paediatric trainee in the
    /// country — wider than the hole T101 was filed to close. CLAUDE.md scopes the role to one
    /// speciality *within an institution*, and it takes both claims to say that.
    /// </summary>
    [Fact]
    public async Task ASpecialityAdminOfTheRightSpecialityAtAnotherInstitutionIsRefused()
    {
        await using var dbContext = CreateContext();
        SeedStampedActivity(dbContext);

        var view = await CreateService(dbContext).GetDetailAsync(
            ActivityId,
            Principal("spec-admin-elsewhere", institutionId: InstitutionId + 1, roles: WombatRoles.SpecialityAdmin, specialityId: SpecialityId));

        view.Should().BeNull();
    }

    /// <inheritdoc cref="ASpecialityAdminOfTheRightSpecialityAtAnotherInstitutionIsRefused" />
    [Fact]
    public async Task ASubSpecialityAdminOfTheRightSubSpecialityAtAnotherInstitutionIsRefused()
    {
        await using var dbContext = CreateContext();
        SeedStampedActivity(dbContext);

        var view = await CreateService(dbContext).GetDetailAsync(
            ActivityId,
            Principal("sub-admin-elsewhere", institutionId: InstitutionId + 1, roles: WombatRoles.SubSpecialityAdmin, subSpecialityId: SubSpecialityId));

        view.Should().BeNull();
    }

    // ─── Refused ─────────────────────────────────────────────────────────────

    /// <summary>
    /// The headline hole. A signed-in user with no relationship to the activity and no programme
    /// role — the shape a PendingTrainee has — walking the id space.
    /// </summary>
    [Fact]
    public async Task AnUnrelatedUserIsRefused()
    {
        await using var dbContext = CreateContext();
        SeedStampedActivity(dbContext);

        var view = await CreateService(dbContext).GetDetailAsync(ActivityId, Principal("stranger-1"));

        view.Should().BeNull();
    }

    [Theory]
    [InlineData(WombatRoles.InstitutionalAdmin)]
    [InlineData(WombatRoles.Coordinator)]
    [InlineData(WombatRoles.CommitteeMember)]
    public async Task InstitutionScopedOversightIsRefusedInAnotherInstitution(string role)
    {
        await using var dbContext = CreateContext();
        SeedStampedActivity(dbContext);

        var view = await CreateService(dbContext).GetDetailAsync(
            ActivityId,
            Principal("overseer-2", institutionId: InstitutionId + 1, roles: role));

        view.Should().BeNull();
    }

    [Fact]
    public async Task ASpecialityAdminOfAnotherSpecialityIsRefused()
    {
        await using var dbContext = CreateContext();
        SeedStampedActivity(dbContext);

        var view = await CreateService(dbContext).GetDetailAsync(
            ActivityId,
            Principal("spec-admin-2", institutionId: 999, roles: WombatRoles.SpecialityAdmin, specialityId: SpecialityId + 1));

        view.Should().BeNull();
    }

    /// <summary>
    /// Fail closed. An activity whose subject had no trainee profile carries no stamp, and an
    /// unstamped activity must satisfy no scoped arm — withholding oversight rather than granting
    /// it to everyone.
    /// </summary>
    [Fact]
    public async Task AnUnstampedActivityIsRefusedToScopedOversight()
    {
        await using var dbContext = CreateContext();
        SeedStampedActivity(dbContext, stamp: false);

        var view = await CreateService(dbContext).GetDetailAsync(
            ActivityId,
            Principal("overseer-1", institutionId: InstitutionId, roles: WombatRoles.InstitutionalAdmin));

        view.Should().BeNull();
    }

    /// <summary>
    /// The anti-enumeration property: a refusal must be byte-identical to an id that was never
    /// issued. If this ever diverges, walking the id space maps the database again.
    /// </summary>
    [Fact]
    public async Task ARefusalIsIndistinguishableFromAnIdThatWasNeverIssued()
    {
        await using var dbContext = CreateContext();
        SeedStampedActivity(dbContext);
        var activityService = CreateService(dbContext);

        var refused = await activityService.GetDetailAsync(ActivityId, Principal("stranger-1"));
        var neverIssued = await activityService.GetDetailAsync(999_999, Principal("stranger-1"));

        refused.Should().BeNull();
        neverIssued.Should().BeNull();
    }

    // ─── Invariants ──────────────────────────────────────────────────────────

    /// <summary>
    /// THE GOVERNING INVARIANT: the read set must be a superset of the act set. A read gate narrower
    /// than the write gate produces inbox rows that cannot be opened and buttons that 404. Anyone
    /// the workflow would let act must therefore be able to read.
    /// </summary>
    [Theory]
    [InlineData("trainee-1")]  // may cancel
    [InlineData("assessor-1")] // may complete or decline
    public async Task AnyoneWhoMayActMayAlsoRead(string userId)
    {
        await using var dbContext = CreateContext();
        SeedStampedActivity(dbContext);

        var view = await CreateService(dbContext).GetDetailAsync(ActivityId, Principal(userId));

        view.Should().NotBeNull();
        view!.AvailableActions.Should().NotBeEmpty(
            "this test is only meaningful while the actor genuinely has an action available");
    }

    /// <summary>
    /// The scope stamp is written at creation from the subject's trainee profile, not derived on
    /// read and not taken from the activity type (whose scope says who may OFFER the tool).
    /// </summary>
    [Fact]
    public async Task CreatingAnActivityStampsTheSubjectsScope()
    {
        await using var dbContext = CreateContext();
        SeedActivityType(dbContext);
        SeedSubjectProfile(dbContext);
        // T102: the assessor named below is judged against the stamp this test checks, so they are seeded eligible there.
        NomineeSeed.AddUser(dbContext, "assessor-1", InstitutionId, WombatRoles.Assessor);
        dbContext.SaveChanges();

        await CreateService(dbContext).CreateDraftAsync(new CreateActivityInput(
            600,
            "trainee-1",
            "trainee-1",
            """{ "epa_id": 5000, "assessor_user_id": "assessor-1" }""",
            Principal("trainee-1")));

        var created = await dbContext.Activities.SingleAsync();
        created.InstitutionId.Should().Be(InstitutionId);
        created.SpecialityId.Should().Be(SpecialityId);
        created.SubSpecialityId.Should().Be(SubSpecialityId);
    }

    /// <summary>
    /// A subject with no trainee profile leaves the stamp null rather than guessing. The activity is
    /// then readable only by the people named on it.
    /// </summary>
    [Fact]
    public async Task CreatingAnActivityForASubjectWithNoProfileLeavesTheStampNull()
    {
        await using var dbContext = CreateContext();
        SeedActivityType(dbContext);
        dbContext.SaveChanges();

        // No assessor named: against a null stamp the nominee gate accepts nobody (T102; NomineeGateTests covers it),
        // so naming one would make this a nominee-gate test. The stamp is what this test is about.
        await CreateService(dbContext).CreateDraftAsync(new CreateActivityInput(
            600,
            "unenrolled-1",
            "unenrolled-1",
            """{ "epa_id": 5000 }""",
            Principal("unenrolled-1")));

        var created = await dbContext.Activities.SingleAsync();
        created.InstitutionId.Should().BeNull();
        created.SpecialityId.Should().BeNull();
        created.SubSpecialityId.Should().BeNull();
    }

    /// <summary>
    /// T101, the alternation case. <c>field:assessor_user_id|role:Coordinator</c> parses as ONE
    /// <c>Any</c> node. An earlier guard asked "does this rule contain an unqualified role?" and threw
    /// the whole rule away if so — taking the bound arm with it, so the named assessor could act (the
    /// row appeared in their inbox) but opening it said "Activity unavailable". The rewrite must drop
    /// only the role arm.
    /// </summary>
    [Fact]
    public async Task AnAssessorNamedAlongsideABareRoleKeepsTheirRead()
    {
        await using var dbContext = CreateContext();
        SeedStampedActivity(dbContext, actorRule: "field:assessor_user_id|role:Coordinator");

        var view = await CreateService(dbContext).GetDetailAsync(ActivityId, Principal("assessor-1"));

        view.Should().NotBeNull();
    }

    /// <summary>
    /// The other direction: a conjunction of nothing but roles binds to no particular activity, so it
    /// must not become a read grant over every activity of that type in every institution.
    /// </summary>
    [Fact]
    public async Task AConjunctionOfBareRolesGrantsNoRead()
    {
        await using var dbContext = CreateContext();
        SeedStampedActivity(dbContext, actorRule: "role:Coordinator+role:CommitteeMember");

        var holder = new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, "outsider-1"),
                new Claim(WombatClaimTypes.InstitutionId, (InstitutionId + 1).ToString()),
                new Claim(ClaimTypes.Role, WombatRoles.Coordinator),
                new Claim(ClaimTypes.Role, WombatRoles.CommitteeMember)
            ],
            "test"));

        var view = await CreateService(dbContext).GetDetailAsync(ActivityId, holder);

        view.Should().BeNull();
    }

    // ─── Fixtures ────────────────────────────────────────────────────────────

    private static ApplicationDbContext CreateContext()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static ActivityService CreateService(ApplicationDbContext dbContext)
        => new(
            dbContext,
            new SchemaValidator(),
            new WorkflowEvaluator(),
            new CreditApplier(dbContext),
            new FieldPermissionEvaluator());

    private static ClaimsPrincipal Principal(
        string userId,
        int? institutionId = InstitutionId,
        string? roles = null,
        int? specialityId = null,
        int? subSpecialityId = null)
    {
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId) };

        if (institutionId.HasValue)
        {
            claims.Add(new Claim(WombatClaimTypes.InstitutionId, institutionId.Value.ToString()));
        }

        if (specialityId.HasValue)
        {
            claims.Add(new Claim(WombatClaimTypes.SpecialityId, specialityId.Value.ToString()));
        }

        if (subSpecialityId.HasValue)
        {
            claims.Add(new Claim(WombatClaimTypes.SubSpecialityId, subSpecialityId.Value.ToString()));
        }

        if (roles is not null)
        {
            claims.Add(new Claim(ClaimTypes.Role, roles));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }

    private static void SeedSubjectProfile(ApplicationDbContext dbContext)
    {
        dbContext.Set<Wombat.Domain.Institutions.Speciality>().Add(new Wombat.Domain.Institutions.Speciality
        {
            Id = SpecialityId,
            Name = "Paediatrics"
        });

        dbContext.Set<Wombat.Domain.Institutions.SubSpeciality>().Add(new Wombat.Domain.Institutions.SubSpeciality
        {
            Id = SubSpecialityId,
            SpecialityId = SpecialityId,
            Name = "General Paediatrics"
        });

        dbContext.Set<Wombat.Domain.Curricula.Curriculum>().Add(new Wombat.Domain.Curricula.Curriculum
        {
            Id = 3000,
            SubSpecialityId = SubSpecialityId,
            Name = "Paediatric EPA Curriculum",
            Version = "11.1"
        });

        dbContext.Set<TraineeProfile>().Add(new TraineeProfile
        {
            Id = 1,
            UserId = "trainee-1",
            InstitutionId = InstitutionId,
            CurriculumId = 3000,
            ProgrammeStartDate = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-1),
            ExpectedCompletionDate = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(2),
            IsActive = true
        });
    }

    /// <summary>
    /// A requested Mini-CEX about trainee-1, filed by coordinator-1, with assessor-1 bound by the
    /// <c>field:assessor_user_id</c> rule. The activity type is scoped to a DIFFERENT speciality
    /// from the subject's on purpose: before T101 a <c>scope:</c> rule resolved against the type, so
    /// this is the shape that made oversight land on the wrong administrator.
    /// </summary>
    private static void SeedStampedActivity(
        ApplicationDbContext dbContext,
        bool stamp = true,
        string? actorRule = null)
    {
        SeedActivityType(dbContext, actorRule);
        SeedSubjectProfile(dbContext);

        var utcNow = DateTime.UtcNow;
        dbContext.Activities.Add(new Activity
        {
            Id = ActivityId,
            ActivityTypeId = 600,
            SchemaVersion = 1,
            SubjectUserId = "trainee-1",
            CreatedByUserId = "coordinator-1",
            CurrentState = "requested",
            DataJson = """
                {
                  "epa_id": 5000,
                  "assessor_user_id": "assessor-1",
                  "case_summary": "Febrile infant in the emergency unit.",
                  "overall_level": 4,
                  "strengths": "Systematic.",
                  "improvements": "Slow to escalate.",
                  "plan": "Repeat next month."
                }
                """,
            InstitutionId = stamp ? InstitutionId : null,
            SpecialityId = stamp ? SpecialityId : null,
            SubSpecialityId = stamp ? SubSpecialityId : null,
            CreatedOn = utcNow,
            UpdatedOn = utcNow
        });

        dbContext.SaveChanges();
    }

    private static void SeedActivityType(ApplicationDbContext dbContext, string? actorRule = null)
    {
        // Swaps the `complete` transition's actor rule so a test can exercise a rule SHAPE — an
        // alternation containing a bare role, or a conjunction of nothing but roles — without a
        // second fixture.
        const string BoundActor = "\"actor\": \"field:assessor_user_id\",";
        var workflowJson = actorRule is null
            ? WorkflowJson
            : WorkflowJson.Replace(
                BoundActor,
                $"\"actor\": \"{actorRule}\",",
                StringComparison.Ordinal);

        var activityType = new ActivityType
        {
            Id = 600,
            Key = "mini_cex_cpsa",
            Name = "Mini-CEX (CPSA)",
            // Deliberately a speciality the subject is NOT in. See SeedStampedActivity.
            Scope = ActivityScope.Speciality,
            ScopeId = SpecialityId + 1,
            Version = 1,
            SchemaJson = SchemaJson,
            WorkflowJson = workflowJson,
            CreditRulesJson = "{}",
            DisplayFieldsJson = """["epa_id","overall_level"]""",
            OwnerUserId = "admin-1",
            CreatedOn = DateTime.UtcNow
        };

        activityType.Versions.Add(new ActivityTypeVersion
        {
            ActivityTypeId = 600,
            Version = 1,
            SchemaJson = SchemaJson,
            WorkflowJson = workflowJson,
            CreditRulesJson = "{}",
            DisplayFieldsJson = activityType.DisplayFieldsJson,
            PublishedByUserId = "admin-1",
            PublishedOn = DateTime.UtcNow
        });

        dbContext.ActivityTypes.Add(activityType);
    }

    private const string SchemaJson = """
        {
          "version": 1,
          "sections": [
            {
              "key": "request",
              "title": "Request",
              "fields": [
                { "key": "epa_id", "type": "epa", "label": "EPA", "required": true },
                { "key": "assessor_user_id", "type": "user", "label": "Assessor", "required": true },
                { "key": "case_summary", "type": "longtext", "label": "Case summary" }
              ]
            },
            {
              "key": "assessment",
              "title": "Entrustment",
              "editable_by": "field:assessor_user_id",
              "fields": [
                { "key": "overall_level", "type": "number", "label": "Overall level" },
                { "key": "strengths", "type": "longtext", "label": "Strengths" },
                { "key": "improvements", "type": "longtext", "label": "Improvements" },
                { "key": "plan", "type": "longtext", "label": "Plan" }
              ]
            }
          ]
        }
        """;

    private const string WorkflowJson = """
        {
          "version": 1,
          "initial_state": "draft",
          "states": [
            { "key": "draft", "label": "Draft" },
            { "key": "requested", "label": "Requested", "editable_by": "field:assessor_user_id" },
            { "key": "completed", "label": "Completed", "terminal": true },
            { "key": "declined", "label": "Declined" },
            { "key": "cancelled", "label": "Cancelled" }
          ],
          "transitions": [
            { "key": "submit", "from": "draft", "to": "requested", "actor": "subject|creator" },
            {
              "key": "complete",
              "from": "requested",
              "to": "completed",
              "actor": "field:assessor_user_id",
              "requires_fields": ["overall_level", "strengths", "improvements", "plan"]
            },
            { "key": "decline", "from": "requested", "to": "declined", "actor": "field:assessor_user_id", "requires_note": true },
            { "key": "cancel", "from": ["draft", "requested"], "to": "cancelled", "actor": "subject|creator" },
            { "key": "override", "from": "requested", "to": "completed", "actor": "role:SpecialityAdmin+scope:speciality" }
          ]
        }
        """;
}
