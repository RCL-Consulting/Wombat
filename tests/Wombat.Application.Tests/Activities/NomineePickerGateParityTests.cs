using System.Security.Claims;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Services;
using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Schema;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Activities;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.Activities;

/// <summary>
/// T102: the picker offers exactly who the write path accepts. Over a matrix of seeded users, each failing one
/// eligibility condition or failing none, every id <see cref="ActivityReferenceDataService.GetNomineeOptionsAsync" />
/// lists is accepted by the write path, and every seeded id it does not list is refused.
/// </summary>
/// <remarks>
/// <para>
/// Parity is the reason the directory is one query shared by both sides. Two drifts would each be a real defect:
/// a picker that offers someone the server refuses turns a correct choice into an error the user cannot explain, and a
/// server that accepts someone the picker hides means the check is not the check the design describes. The picker's
/// inputs are built the way <c>ActivityForm</c> builds them: roles from
/// <see cref="ActorFieldRules.RequiredRolesForUserField" />, the create page resolving the institution from the subject,
/// an existing activity passing its stamped institution from the DTO.
/// </para>
/// <para>
/// Each call runs on its own DbContext over one InMemory store, as each request does. Every refusal is checked against
/// the audit trap: the change tracker holds nothing dirty, and the audit pipeline's save writes nothing. The Npgsql
/// half of the same parity is <c>NomineeDirectoryPostgresTests</c> in the integration suite.
/// </para>
/// </remarks>
public sealed class NomineePickerGateParityTests
{
    private const string TraineeId = "trainee-1";

    private const int InstitutionId = 10;
    private const int OtherInstitutionId = 20;

    private const int AssessorFieldTypeId = 100;
    private const int CommitteeFieldTypeId = 101;

    /// <summary>The fragment every nominee-gate refusal carries, so a test can tell it apart from any other refusal.</summary>
    private const string GateRefusal = "cannot be named here";

    /// <summary>
    /// One row per seeded user. Each decoy fails exactly one condition for an <c>Assessor</c> field; the eligible rows
    /// differ from each other only in things that must not matter (an extra role, a lockout that lifts by itself).
    /// </summary>
    private static readonly (string Id, int? Institution, DateTimeOffset? LockoutEnd, string[] Roles)[] Matrix =
    [
        ("eligible", InstitutionId, null, [WombatRoles.Assessor]),
        ("eligible-extra-role", InstitutionId, null, [WombatRoles.Assessor, WombatRoles.Coordinator]),
        ("briefly-locked", InstitutionId, DateTimeOffset.UtcNow.AddMinutes(15), [WombatRoles.Assessor]),
        ("lockout-expired", InstitutionId, DateTimeOffset.UtcNow.AddDays(-1), [WombatRoles.Assessor]),
        ("assessor-and-committee", InstitutionId, null, [WombatRoles.Assessor, WombatRoles.CommitteeMember]),
        ("committee-only", InstitutionId, null, [WombatRoles.CommitteeMember]),
        ("coordinator-only", InstitutionId, null, [WombatRoles.Coordinator]),
        ("pending-trainee", InstitutionId, null, [WombatRoles.PendingTrainee]),
        ("no-roles", InstitutionId, null, []),
        ("other-institution", OtherInstitutionId, null, [WombatRoles.Assessor]),
        ("no-institution", null, null, [WombatRoles.Assessor]),
        ("deactivated", InstitutionId, DateTimeOffset.MaxValue, [WombatRoles.Assessor]),
        // What an erasure leaves: roles removed and locked for good.
        ("erased", InstitutionId, DateTimeOffset.MaxValue, []),
    ];

    /// <summary>Ids that name nobody: a re-cased, a padded and an unknown id. Never listed, so never accepted.</summary>
    private static readonly string[] Variants = ["ELIGIBLE", "eligible ", " eligible", "nobody-at-all"];

    /// <summary>Who each field's picker must list: the guard that keeps parity from holding over an empty list.</summary>
    public static TheoryData<int, string[]> Fields => new()
    {
        { AssessorFieldTypeId, ["eligible", "eligible-extra-role", "briefly-locked", "lockout-expired", "assessor-and-committee"] },
        // An explicit role replaces the default: an Assessor who is not on the committee is not a committee nominee.
        { CommitteeFieldTypeId, ["assessor-and-committee", "committee-only"] },
    };

    // ---- 1 ----------------------------------------------------------------------------------------------------

    [Theory]
    [MemberData(nameof(Fields))]
    public async Task OnTheCreatePage_EveryListedIdIsAcceptedByTheCreate_AndEverySeededIdNotListedIsRefused(int activityTypeId, string[] expected)
    {
        var options = NewDatabase();
        await SeedAsync(options);

        var listed = await ListAsync(options, new NomineeOptionScope(
            TraineeId, await RequiredRolesAsync(options, activityTypeId), ForExistingActivity: false, ActivityInstitutionId: null, StoredValue: null));

        listed.Should().BeEquivalentTo(expected, "guard: parity over the wrong list would prove nothing");

        foreach (var nominee in listed)
        {
            var created = await CreateAsync(options, activityTypeId, nominee);
            ReadString(created.DataJson, "nominee_user_id").Should().Be(nominee, "a listed nominee must be accepted as written");
        }

        foreach (var nominee in SeededIds().Except(listed).Concat(Variants))
        {
            var message = await ShouldBeRefusedAsync(options, service => service.CreateDraftAsync(CreateInput(activityTypeId, nominee)));
            if (nominee != TraineeId)
            {
                // The subject is refused earlier, by the rule that an actor field never names the subject.
                message.Should().Contain(GateRefusal, $"'{nominee}' is not listed, so the nominee gate must refuse it");
            }
        }

        await using var verify = new ApplicationDbContext(options);
        (await verify.Activities.CountAsync()).Should().Be(listed.Count, "only the listed nominees' creates were saved");
    }

    // ---- 2 ----------------------------------------------------------------------------------------------------

    [Theory]
    [MemberData(nameof(Fields))]
    public async Task OnAnExistingActivity_EveryListedIdIsAcceptedWhenTheAuthorChangesToIt_AndEverySeededIdNotListedIsRefused(
        int activityTypeId,
        string[] expected)
    {
        // The existing-activity picker is keyed on the DTO's stamped institution, not on the subject's profile as it is
        // now. A changed nominee is judged on every move, so a submit that re-picks is the write path it must match.
        var options = NewDatabase();
        await SeedAsync(options);

        var first = expected[0];
        var probe = await CreateAsync(options, activityTypeId, first);
        probe.InstitutionId.Should().Be(InstitutionId, "guard: the create stamps the profile's institution, not the Identity row's");

        var listed = await ListAsync(options, new NomineeOptionScope(
            TraineeId, await RequiredRolesAsync(options, activityTypeId), ForExistingActivity: true, probe.InstitutionId, StoredValue: first));

        listed.Should().BeEquivalentTo(expected, "guard: the stored value is eligible, so nothing is appended");

        foreach (var nominee in listed)
        {
            var draft = await CreateAsync(options, activityTypeId, first);
            var submitted = await TransitionAsync(options, draft.Id, "submit", Patch(nominee));

            submitted.CurrentState.Should().Be("requested");
            ReadString(submitted.DataJson, "nominee_user_id").Should().Be(nominee);
        }

        foreach (var nominee in SeededIds().Except(listed).Concat(Variants))
        {
            var draft = await CreateAsync(options, activityTypeId, first);

            var message = await ShouldBeRefusedAsync(options, service => service.TransitionAsync(
                new TransitionActivityInput(draft.Id, "submit", TraineeId, Principal(TraineeId), Patch(nominee), null)));
            if (nominee != TraineeId)
            {
                message.Should().Contain(GateRefusal, $"'{nominee}' is not listed, so the nominee gate must refuse it");
            }

            var stored = await StoredAsync(options, draft.Id);
            stored.CurrentState.Should().Be("draft");
            ReadString(stored.DataJson, "nominee_user_id").Should().Be(first, "a refused submit must not merge its patch");
        }
    }

    // ---- 3 ----------------------------------------------------------------------------------------------------

    [Fact]
    public async Task AStoredNomineeKeptOnThePickerOnlyBecauseItIsStored_IsTheOneEntryTheAuthorsHandOnRefuses()
    {
        // The one deliberate gap in parity: a stored value that has fallen off the list stays selectable, labelled
        // neutrally, so a filled field never renders as "Select…". It is safe only because the author's hand-on still
        // judges it. This pins both halves: it is listed, it is refused at the submit, and every other entry passes.
        var options = NewDatabase();
        await SeedAsync(options);

        var draft = await CreateAsync(options, AssessorFieldTypeId, "eligible");
        await DeactivateAsync(options, "eligible");

        var picker = await ListOptionsAsync(options, new NomineeOptionScope(
            TraineeId, [WombatRoles.Assessor], ForExistingActivity: true, draft.InstitutionId, StoredValue: "eligible"));

        picker.Select(option => option.Value).Should().BeEquivalentTo(
            ["eligible", "eligible-extra-role", "briefly-locked", "lockout-expired", "assessor-and-committee"],
            "guard: the directory's four remaining nominees, plus the stored one kept on");
        var kept = picker.Should().ContainSingle(option => option.Value == "eligible").Subject;
        kept.Label.Should().EndWith("(not on the current list)");

        var message = await ShouldBeRefusedAsync(options, service => service.TransitionAsync(
            new TransitionActivityInput(draft.Id, "submit", TraineeId, Principal(TraineeId), null, null)));
        message.Should().StartWith("Nominee: First eligible cannot be named here.");
        (await StoredAsync(options, draft.Id)).CurrentState.Should().Be("draft");

        foreach (var option in picker.Where(option => option.Value != "eligible"))
        {
            option.Label.Should().NotEndWith("(not on the current list)");

            var other = await CreateAsync(options, AssessorFieldTypeId, option.Value);
            (await TransitionAsync(options, other.Id, "submit", null)).CurrentState.Should().Be("requested",
                $"'{option.Value}' is a directory entry, so the author's hand-on must accept it");
        }

        // Nor does the kept entry trap the draft: a withdrawal into a dead end never judges an unchanged nominee.
        (await TransitionAsync(options, draft.Id, "cancel", null)).CurrentState.Should().Be("cancelled");
    }

    // ---- helpers ----------------------------------------------------------------------------------------------

    private static IEnumerable<string> SeededIds() => Matrix.Select(row => row.Id).Append(TraineeId);

    private static async Task<IReadOnlyList<string>> ListAsync(
        DbContextOptions<ApplicationDbContext> options,
        NomineeOptionScope scope)
        => (await ListOptionsAsync(options, scope)).Select(option => option.Value).ToList();

    private static async Task<IReadOnlyList<ActivityCatalogueOption>> ListOptionsAsync(
        DbContextOptions<ApplicationDbContext> options,
        NomineeOptionScope scope)
    {
        await using var db = new ApplicationDbContext(options);
        return await new ActivityReferenceDataService(db).GetNomineeOptionsAsync(scope);
    }

    /// <summary>The roles the picker asks for, read off the published schema exactly as <c>ActivityForm</c> reads them.</summary>
    private static async Task<IReadOnlyList<string>> RequiredRolesAsync(DbContextOptions<ApplicationDbContext> options, int activityTypeId)
    {
        await using var db = new ApplicationDbContext(options);
        var schemaJson = await db.ActivityTypes.Where(type => type.Id == activityTypeId).Select(type => type.SchemaJson).SingleAsync();
        return ActorFieldRules.RequiredRolesForUserField(FormSchemaParser.Parse(schemaJson!), "nominee_user_id");
    }

    /// <summary>What an administrator's lock does to one user, in its own request.</summary>
    private static async Task DeactivateAsync(DbContextOptions<ApplicationDbContext> options, string userId)
    {
        await using var db = new ApplicationDbContext(options);
        var user = await db.Users.SingleAsync(entity => entity.Id == userId);
        user.LockoutEnd = DateTimeOffset.MaxValue;
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Runs one refused request on its own context, asserts it left nothing for the audit save to commit, performs
    /// that save, and returns the refusal message.
    /// </summary>
    private static async Task<string> ShouldBeRefusedAsync(
        DbContextOptions<ApplicationDbContext> options,
        Func<ActivityService, Task> act)
    {
        await using var db = new ApplicationDbContext(options);
        var service = Service(db);

        var attempt = async () => await act(service);
        var thrown = await attempt.Should().ThrowAsync<InvalidOperationException>();

        db.ChangeTracker.Entries()
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Select(entry => $"{entry.Metadata.ClrType.Name}: {entry.State}")
            .Should().BeEmpty("the audit pipeline's catch saves this context, so anything dirty here would be committed");

        (await db.SaveChangesAsync()).Should().Be(0);

        return thrown.Which.Message;
    }

    private static async Task<ActivityDto> CreateAsync(DbContextOptions<ApplicationDbContext> options, int activityTypeId, string nominee)
    {
        await using var db = new ApplicationDbContext(options);
        return await Service(db).CreateDraftAsync(CreateInput(activityTypeId, nominee));
    }

    private static async Task<ActivityDto> TransitionAsync(
        DbContextOptions<ApplicationDbContext> options,
        int activityId,
        string transitionKey,
        string? patch)
    {
        await using var db = new ApplicationDbContext(options);
        return await Service(db).TransitionAsync(
            new TransitionActivityInput(activityId, transitionKey, TraineeId, Principal(TraineeId), patch, null));
    }

    private static async Task<Activity> StoredAsync(DbContextOptions<ApplicationDbContext> options, int activityId)
    {
        await using var db = new ApplicationDbContext(options);
        return await db.Activities.AsNoTracking().SingleAsync(entity => entity.Id == activityId);
    }

    private static CreateActivityInput CreateInput(int activityTypeId, string nominee)
        => new(
            activityTypeId,
            TraineeId,
            TraineeId,
            JsonSerializer.Serialize(new Dictionary<string, string> { ["nominee_user_id"] = nominee, ["observed_on"] = "2026-03-10" }),
            Principal(TraineeId));

    private static string Patch(string nominee)
        => JsonSerializer.Serialize(new Dictionary<string, string> { ["nominee_user_id"] = nominee });

    private static ActivityService Service(ApplicationDbContext db)
        => new(db, new SchemaValidator(), new WorkflowEvaluator(), new CreditApplier(db), new FieldPermissionEvaluator());

    private static string? ReadString(string dataJson, string key)
    {
        using var document = JsonDocument.Parse(dataJson);
        return document.RootElement.TryGetProperty(key, out var value) ? value.GetString() : null;
    }

    private static DbContextOptions<ApplicationDbContext> NewDatabase()
        => new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

    private static ClaimsPrincipal Principal(string userId)
        => new(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId)], "test"));

    // ---- fixture ----------------------------------------------------------------------------------------------

    /// <summary>A request form whose nominee must hold the default role: no <c>role</c> property.</summary>
    private const string AssessorFieldSchemaJson = """
        {
          "version": 1,
          "observation_date_field": "observed_on",
          "sections": [
            {
              "key": "request",
              "title": "Request",
              "fields": [
                { "key": "nominee_user_id", "type": "user", "label": "Nominee", "required": true },
                { "key": "observed_on", "type": "date", "label": "Date observed", "required": true }
              ]
            },
            {
              "key": "assessment",
              "title": "Assessment",
              "editable_by": "field:nominee_user_id",
              "fields": [
                { "key": "overall_level", "type": "number", "label": "Supervision required" }
              ]
            }
          ]
        }
        """;

    /// <summary>The same form with the nominee's role declared on the field.</summary>
    private static readonly string CommitteeFieldSchemaJson = AssessorFieldSchemaJson.Replace(
        """{ "key": "nominee_user_id", "type": "user", "label": "Nominee", "required": true }""",
        """{ "key": "nominee_user_id", "type": "user", "label": "Nominee", "required": true, "role": "CommitteeMember" }""",
        StringComparison.Ordinal);

    /// <summary>The CPSA seeds' shape, with the nominee field as the assessor.</summary>
    private const string WorkflowJson = """
        {
          "version": 1,
          "initial_state": "draft",
          "states": [
            { "key": "draft", "label": "Draft" },
            { "key": "requested", "label": "Requested", "editable_by": "field:nominee_user_id" },
            { "key": "completed", "label": "Completed", "terminal": true },
            { "key": "cancelled", "label": "Cancelled" }
          ],
          "transitions": [
            { "key": "submit", "from": "draft", "to": "requested", "actor": "subject|creator" },
            { "key": "complete", "from": "requested", "to": "completed", "actor": "field:nominee_user_id", "requires_fields": ["overall_level"] },
            { "key": "cancel", "from": ["draft", "requested"], "to": "cancelled", "actor": "subject|creator" }
          ]
        }
        """;

    private static async Task SeedAsync(DbContextOptions<ApplicationDbContext> options)
    {
        await using var db = new ApplicationDbContext(options);

        foreach (var (id, institution, lockoutEnd, roles) in Matrix)
        {
            NomineeSeed.AddUser(db, id, institution, lockoutEnd, roles);
        }

        // The subject holds every role a nominee could need, so only the subject condition excludes them. Their Identity
        // row sits at the OTHER institution and their profile at this one: the create stamps from the profile, and so
        // must the create page's picker, or "other-institution" would be listed on one side only.
        NomineeSeed.AddUser(db, TraineeId, OtherInstitutionId, WombatRoles.Trainee, WombatRoles.Assessor, WombatRoles.CommitteeMember);
        db.TraineeProfiles.Add(new TraineeProfile
        {
            Id = 1,
            UserId = TraineeId,
            InstitutionId = InstitutionId,
            CurriculumId = 3000,
            ProgrammeStartDate = new DateOnly(2025, 4, 14),
            ExpectedCompletionDate = new DateOnly(2029, 4, 13),
            IsActive = true
        });

        db.ActivityTypes.AddRange(
            Type(AssessorFieldTypeId, "assessor_field_under_test", AssessorFieldSchemaJson),
            Type(CommitteeFieldTypeId, "committee_field_under_test", CommitteeFieldSchemaJson));

        await db.SaveChangesAsync();
    }

    /// <summary>No instrument key and no credit, so the T122 gate never runs and cannot be the reason for a refusal.</summary>
    private static ActivityType Type(int id, string key, string schemaJson)
    {
        var publishedOn = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        const string creditRulesJson = """{ "counts_for": [] }""";
        const string displayFieldsJson = """["nominee_user_id"]""";

        var activityType = new ActivityType
        {
            Id = id,
            Key = key,
            Name = key,
            Scope = ActivityScope.Institution,
            ScopeId = InstitutionId,
            Version = 1,
            SchemaJson = schemaJson,
            WorkflowJson = WorkflowJson,
            CreditRulesJson = creditRulesJson,
            DisplayFieldsJson = displayFieldsJson,
            WbaToolKey = null,
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
            DisplayFieldsJson = displayFieldsJson,
            PublishedByUserId = "admin-1",
            PublishedOn = publishedOn
        });

        return activityType;
    }
}
