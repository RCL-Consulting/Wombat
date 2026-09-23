using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Activities.Commands.SaveActivityTypeDraft;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Queries.GetActivityTypeEditor;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Workflow;
using Wombat.Domain.Epas;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Activities;

/// <summary>
/// T122 — the activity-type builder's "This tool is" choice: <see cref="SaveActivityTypeDraftCommand" /> writes
/// <see cref="ActivityType.WbaToolKey" />, normalised, validated against the vocabulary, and never half-written.
/// </summary>
/// <remarks>
/// <para>
/// The key is the half of the allow-list an activity type carries. A key the vocabulary does not hold could never
/// match any curriculum item's list, so a type carrying one would be refused against every restricted EPA with
/// nothing saying why; a key that is silently dropped on an unchanged re-save is worse, because null is
/// unrestricted (D21) and nothing would ever say the restriction had gone.
/// </para>
/// <para>
/// The other half of these tests is the audit trap. <c>AuditPipelineBehavior</c>'s catch saves the request's
/// DbContext, so whatever a refused command leaves dirty is COMMITTED. Before T122 this handler <c>Add</c>ed a new
/// type before the scope guard ran and wrote the metadata before the DSL parse. Every refusal here is therefore
/// checked the way <c>CreditPlannedBeforeTransitionTests</c> checks one: nothing Added, Modified or Deleted in the
/// tracker, then the audit pipeline's save, then a fresh context that must still hold the old row.
/// </para>
/// </remarks>
public sealed class SaveActivityTypeDraftWbaToolKeyTests
{
    private const string ActorUserId = "admin-user";

    [Fact]
    public async Task CreatingANewType_StoresTheNormalisedKey()
    {
        // The builder's select sends the key it was given, but an API caller or a pasted value may not. Every reader
        // compares normalised keys, so a stored " Mini_CEX " would match nothing and refuse every restricted EPA.
        var fixture = await Fixture.CreateAsync();
        var defaults = await NewTypeDefaultsAsync(fixture);

        ActivityTypeEditorDto saved;
        await using (var db = fixture.NewContext())
        {
            saved = await new SaveActivityTypeDraftCommandHandler(db).Handle(
                Command(null, "mini_cex_local", "Local Mini-CEX", ActivityScope.Global, null, " Mini_CEX ", defaults, TestPrincipals.Administrator()),
                CancellationToken.None);
        }

        saved.WbaToolKey.Should().Be("mini_cex");

        // Read back through a fresh context: the returned DTO is mapped from the tracked entity, so it would echo the
        // value even if nothing reached the store.
        await using var verify = fixture.NewContext();
        var stored = await verify.Set<ActivityType>().SingleAsync(type => type.Key == "mini_cex_local");
        stored.WbaToolKey.Should().Be("mini_cex");
    }

    [Fact]
    public async Task SavingAnExistingType_ReplacesItsKeyWithTheNormalisedOne()
    {
        var fixture = await Fixture.CreateAsync();
        var defaults = await NewTypeDefaultsAsync(fixture);

        await using (var db = fixture.NewContext())
        {
            await new SaveActivityTypeDraftCommandHandler(db).Handle(
                Command(fixture.ExistingTypeId, Fixture.ExistingKey, Fixture.ExistingName, ActivityScope.Global, null, "  DOPS", defaults, TestPrincipals.Administrator()),
                CancellationToken.None);
        }

        await using var verify = fixture.NewContext();
        var stored = await verify.Set<ActivityType>().SingleAsync(type => type.Id == fixture.ExistingTypeId);
        stored.WbaToolKey.Should().Be("dops", "the existing 'cbd' is replaced, and the replacement is normalised");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ANullOrBlankKey_WithdrawsTheTypesInstrument(string? requestedKey)
    {
        // "Not a WBA instrument" is the builder's empty option. It must store null, not an empty string: a blank key
        // is recognised as a key by nothing and would read as an unknown instrument rather than as no instrument.
        var fixture = await Fixture.CreateAsync();
        var defaults = await NewTypeDefaultsAsync(fixture);

        await using (var db = fixture.NewContext())
        {
            var saved = await new SaveActivityTypeDraftCommandHandler(db).Handle(
                Command(fixture.ExistingTypeId, Fixture.ExistingKey, Fixture.ExistingName, ActivityScope.Global, null, requestedKey, defaults, TestPrincipals.Administrator()),
                CancellationToken.None);

            saved.WbaToolKey.Should().BeNull();
        }

        await using var verify = fixture.NewContext();
        var stored = await verify.Set<ActivityType>().SingleAsync(type => type.Id == fixture.ExistingTypeId);
        stored.WbaToolKey.Should().BeNull("guard: the fixture's type starts keyed 'cbd', so null here is a withdrawal");
    }

    [Fact]
    public async Task AnUnknownKeyOnAnExistingType_IsRefusedBeforeAnythingIsWritten()
    {
        // The rename rides along so a refusal that fired AFTER the metadata writes would show: the tracked entity
        // would be Modified, and the audit pipeline's save would commit the new name under a failed command.
        var fixture = await Fixture.CreateAsync();
        var defaults = await NewTypeDefaultsAsync(fixture);

        await using (var db = fixture.NewContext())
        {
            var act = () => new SaveActivityTypeDraftCommandHandler(db).Handle(
                Command(fixture.ExistingTypeId, Fixture.ExistingKey, "Renamed by a refused save", ActivityScope.Global, null, " OSCE ", defaults, TestPrincipals.Administrator()),
                CancellationToken.None);

            (await act.Should().ThrowAsync<InvalidOperationException>())
                .WithMessage("*'osce'*", "the refusal names the key, normalised, so the administrator can see what was not recognised");

            var tracked = db.ChangeTracker.Entries<ActivityType>().Single(entry => entry.Entity.Id == fixture.ExistingTypeId);
            tracked.State.Should().Be(EntityState.Unchanged, "guard: the handler did load the type, so a mutation would show here");
            tracked.Entity.Name.Should().Be(Fixture.ExistingName);
            tracked.Entity.WbaToolKey.Should().Be("cbd");
            tracked.Entity.StagingSchemaJson.Should().BeNull("the draft is parsed only after the key is accepted");

            await AssertNothingForTheAuditSaveToCommitAsync(db);
        }

        await using var verify = fixture.NewContext();
        var stored = await verify.Set<ActivityType>().SingleAsync(type => type.Id == fixture.ExistingTypeId);
        stored.Name.Should().Be(Fixture.ExistingName);
        stored.WbaToolKey.Should().Be("cbd");
        stored.StagingSchemaJson.Should().BeNull();
    }

    [Fact]
    public async Task AnUnknownKeyOnANewType_IsRefusedWithNothingAdded()
    {
        var fixture = await Fixture.CreateAsync();
        var defaults = await NewTypeDefaultsAsync(fixture);

        await using (var db = fixture.NewContext())
        {
            var act = () => new SaveActivityTypeDraftCommandHandler(db).Handle(
                Command(null, "viva_local", "Viva", ActivityScope.Global, null, "viva", defaults, TestPrincipals.Administrator()),
                CancellationToken.None);

            (await act.Should().ThrowAsync<InvalidOperationException>()).WithMessage("*'viva'*");

            db.ChangeTracker.Entries<ActivityType>()
                .Should().NotContain(entry => entry.State == EntityState.Added, "the new type is Added only after every check has passed");
            await AssertNothingForTheAuditSaveToCommitAsync(db);
        }

        await using var verify = fixture.NewContext();
        (await verify.Set<ActivityType>().AnyAsync(type => type.Key == "viva_local")).Should().BeFalse();
    }

    [Fact]
    public async Task TheEditorsOwnValues_SavedUnchanged_KeepTheKey()
    {
        // This is what the builder does: load the editor DTO, bind it to the form, save it. If the DTO dropped the
        // key, or the command defaulted it, an administrator who only opened and saved mini_cex_cpsa would silently
        // make it unrestricted, and under D21 nothing would ever say so.
        var fixture = await Fixture.CreateAsync(publishExistingType: true);

        ActivityTypeEditorDto loaded;
        await using (var db = fixture.NewContext())
        {
            loaded = await new GetActivityTypeEditorQueryHandler(db).Handle(
                new GetActivityTypeEditorQuery(fixture.ExistingTypeId, TestPrincipals.Administrator()),
                CancellationToken.None);
        }

        loaded.WbaToolKey.Should().Be("cbd", "guard: the editor DTO must carry the key for the round trip to mean anything");
        loaded.PublishedVersion.Should().Be(1, "guard: a published type, so the key-change check is live");

        await using (var db = fixture.NewContext())
        {
            await new SaveActivityTypeDraftCommandHandler(db).Handle(
                new SaveActivityTypeDraftCommand(
                    loaded.Id,
                    loaded.Key,
                    loaded.Name,
                    loaded.Description,
                    loaded.Scope,
                    loaded.ScopeId,
                    loaded.IsActive,
                    loaded.WbaToolKey,
                    loaded.DraftSchemaJson,
                    loaded.DraftWorkflowJson,
                    loaded.DraftCreditRulesJson,
                    loaded.DraftDisplayFieldsJson,
                    ActorUserId,
                    TestPrincipals.Administrator()),
                CancellationToken.None);
        }

        await using var verify = fixture.NewContext();
        var stored = await verify.Set<ActivityType>().SingleAsync(type => type.Id == fixture.ExistingTypeId);
        stored.WbaToolKey.Should().Be("cbd");
        stored.Name.Should().Be(Fixture.ExistingName);
        stored.Description.Should().Be(Fixture.ExistingDescription);
        stored.Scope.Should().Be(ActivityScope.Global);
        stored.Version.Should().Be(1, "saving a draft never publishes");
    }

    [Fact]
    public async Task AnInstitutionalAdminCreatingAGlobalType_IsRefused_AndNoTypeIsLeftForTheAuditSaveToCommit()
    {
        // REGRESSION (T122 adjacent fix). Before T122 the handler Add()ed the new type and only then ran the scope
        // guard on the requested scope. The refusal was correct, but the audit pipeline's catch then saved the
        // context, committing a blank-named, Version-0 Global type that squatted the key. A seeder skips a key that
        // already exists, so the squat would also have blocked any later seed of that key.
        var fixture = await Fixture.CreateAsync();
        var defaults = await NewTypeDefaultsAsync(fixture);

        await using (var db = fixture.NewContext())
        {
            var act = () => new SaveActivityTypeDraftCommandHandler(db).Handle(
                Command(null, "squatter", "Squatter", ActivityScope.Global, null, "mini_cex", defaults, TestPrincipals.InstitutionalAdmin(fixture.InstitutionId)),
                CancellationToken.None);

            await act.Should().ThrowAsync<UnauthorizedAccessException>();

            db.ChangeTracker.Entries<ActivityType>()
                .Should().NotContain(entry => entry.State == EntityState.Added, "a refused create must not leave the new type tracked");
            await AssertNothingForTheAuditSaveToCommitAsync(db);
        }

        await using var verify = fixture.NewContext();
        (await verify.Set<ActivityType>().AnyAsync(type => type.Key == "squatter"))
            .Should().BeFalse("the key must still be free after the refused create");
    }

    [Fact]
    public async Task AnInstitutionalAdminCreatingATypeInTheirOwnInstitution_MayKeyIt()
    {
        // The control for the refusal above: the same caller, the same key, their own scope. It shows the refusal is
        // the scope guard and not the key, and that an institution may declare which instrument its own type is (the
        // D21 trust boundary: institution-declared keys are trusted).
        var fixture = await Fixture.CreateAsync();
        var defaults = await NewTypeDefaultsAsync(fixture);

        await using (var db = fixture.NewContext())
        {
            await new SaveActivityTypeDraftCommandHandler(db).Handle(
                Command(null, "local_mini_cex", "Our Mini-CEX", ActivityScope.Institution, fixture.InstitutionId, "Mini_Cex", defaults, TestPrincipals.InstitutionalAdmin(fixture.InstitutionId)),
                CancellationToken.None);
        }

        await using var verify = fixture.NewContext();
        var stored = await verify.Set<ActivityType>().SingleAsync(type => type.Key == "local_mini_cex");
        stored.WbaToolKey.Should().Be("mini_cex");
        stored.Scope.Should().Be(ActivityScope.Institution);
        stored.ScopeId.Should().Be(fixture.InstitutionId);
    }

    [Fact]
    public async Task AnOutOfScopeCaller_IsRefusedAsUnauthorised_BeforeTheKeyIsChecked()
    {
        // The scope guards run before the vocabulary lookup, so an out-of-scope caller learns nothing about which
        // instruments exist: an unknown key must not turn "you may not do this" into "that is not an instrument".
        var fixture = await Fixture.CreateAsync();
        var defaults = await NewTypeDefaultsAsync(fixture);

        await using var db = fixture.NewContext();
        var act = () => new SaveActivityTypeDraftCommandHandler(db).Handle(
            Command(null, "probe", "Probe", ActivityScope.Global, null, "not_an_instrument", defaults, TestPrincipals.InstitutionalAdmin(fixture.InstitutionId)),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        await AssertNothingForTheAuditSaveToCommitAsync(db);
    }

    [Fact]
    public async Task AnInstitutionalAdminEditingAGlobalType_IsRefused_AndTheTypeIsUntouched()
    {
        // ActivityTypeScopeGuardTests pins the exception. This pins the audit half: the type is loaded WITH tracking,
        // so any write before the guard would be Modified and committed by the audit save.
        var fixture = await Fixture.CreateAsync();
        var defaults = await NewTypeDefaultsAsync(fixture);

        await using (var db = fixture.NewContext())
        {
            var act = () => new SaveActivityTypeDraftCommandHandler(db).Handle(
                Command(fixture.ExistingTypeId, Fixture.ExistingKey, "Renamed by an IA", ActivityScope.Global, null, null, defaults, TestPrincipals.InstitutionalAdmin(fixture.InstitutionId)),
                CancellationToken.None);

            await act.Should().ThrowAsync<UnauthorizedAccessException>();
            await AssertNothingForTheAuditSaveToCommitAsync(db);
        }

        await using var verify = fixture.NewContext();
        var stored = await verify.Set<ActivityType>().SingleAsync(type => type.Id == fixture.ExistingTypeId);
        stored.Name.Should().Be(Fixture.ExistingName);
        stored.WbaToolKey.Should().Be("cbd", "a refused edit must not withdraw the instrument");
    }

    [Fact]
    public async Task AMalformedWorkflow_WithAChangedNameKeyAndTool_LeavesTheExistingTypeExactlyAsItWas()
    {
        // REGRESSION (T122 adjacent fix). Before T122 the metadata was written before SaveDraft parsed the payloads,
        // so a draft with a malformed workflow threw with the rename (and now the new tool key) already on the
        // tracked entity, and the audit pipeline's save committed them under a failed command. Every value this
        // save tries to change is a valid change on its own, so only the ordering can keep them out.
        var fixture = await Fixture.CreateAsync();
        var defaults = await NewTypeDefaultsAsync(fixture);

        await using (var db = fixture.NewContext())
        {
            var act = () => new SaveActivityTypeDraftCommandHandler(db).Handle(
                new SaveActivityTypeDraftCommand(
                    fixture.ExistingTypeId,
                    "renamed_key",
                    "Renamed by a failed save",
                    "A new description",
                    ActivityScope.Global,
                    null,
                    false,
                    "dops",
                    defaults.DraftSchemaJson,
                    """{ "version": 1, "initial_state": """,
                    defaults.DraftCreditRulesJson,
                    defaults.DraftDisplayFieldsJson,
                    ActorUserId,
                    TestPrincipals.Administrator()),
                CancellationToken.None);

            await act.Should().ThrowAsync<WorkflowParseException>();

            db.ChangeTracker.Entries()
                .Where(entry => entry.State == EntityState.Modified)
                .Should().BeEmpty("a failed parse must leave the loaded type exactly as it was read");

            var tracked = db.ChangeTracker.Entries<ActivityType>().Single(entry => entry.Entity.Id == fixture.ExistingTypeId).Entity;
            tracked.Key.Should().Be(Fixture.ExistingKey);
            tracked.Name.Should().Be(Fixture.ExistingName);
            tracked.Description.Should().Be(Fixture.ExistingDescription);
            tracked.IsActive.Should().BeTrue();
            tracked.WbaToolKey.Should().Be("cbd");
            tracked.StagingSchemaJson.Should().BeNull("the valid schema must not be staged when the workflow beside it is malformed");

            await AssertNothingForTheAuditSaveToCommitAsync(db);
        }

        await using var verify = fixture.NewContext();
        var stored = await verify.Set<ActivityType>().SingleAsync(type => type.Id == fixture.ExistingTypeId);
        stored.Key.Should().Be(Fixture.ExistingKey);
        stored.Name.Should().Be(Fixture.ExistingName);
        stored.WbaToolKey.Should().Be("cbd");
        stored.IsActive.Should().BeTrue();
        stored.StagingSchemaJson.Should().BeNull();
        stored.StagingWorkflowJson.Should().BeNull();
    }

    [Fact]
    public async Task AMalformedDisplayFieldList_LeavesAnEarlierDraftStaged_NotHalfReplaced()
    {
        // SaveDraft's own atomicity (the second half of the same fix). The display fields are the LAST payload it
        // canonicalises, so a SaveDraft that assigned each staging column as it parsed would have replaced the
        // schema, workflow and credit rules before throwing here. The earlier draft must survive whole.
        var fixture = await Fixture.CreateAsync(stageExistingDraft: true);
        var defaults = await NewTypeDefaultsAsync(fixture);

        string priorStagedSchema;
        await using (var seeded = fixture.NewContext())
        {
            priorStagedSchema = (await seeded.Set<ActivityType>().SingleAsync(type => type.Id == fixture.ExistingTypeId)).StagingSchemaJson!;
        }

        var differentSchema = defaults.DraftSchemaJson.Replace("\"Title\"", "\"A different title\"", StringComparison.Ordinal);
        differentSchema.Should().NotBe(defaults.DraftSchemaJson, "guard: the new schema must differ, or a half-write would be invisible");

        await using (var db = fixture.NewContext())
        {
            var act = () => new SaveActivityTypeDraftCommandHandler(db).Handle(
                new SaveActivityTypeDraftCommand(
                    fixture.ExistingTypeId,
                    Fixture.ExistingKey,
                    "Renamed by a failed save",
                    Fixture.ExistingDescription,
                    ActivityScope.Global,
                    null,
                    true,
                    "mini_cex",
                    differentSchema,
                    defaults.DraftWorkflowJson,
                    defaults.DraftCreditRulesJson,
                    "[1]",
                    ActorUserId,
                    TestPrincipals.Administrator()),
                CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Display field*");

            var tracked = db.ChangeTracker.Entries<ActivityType>().Single(entry => entry.Entity.Id == fixture.ExistingTypeId);
            tracked.State.Should().Be(EntityState.Unchanged);
            tracked.Entity.StagingSchemaJson.Should().Be(priorStagedSchema);
            tracked.Entity.WbaToolKey.Should().Be("cbd");
            tracked.Entity.Name.Should().Be(Fixture.ExistingName);

            await AssertNothingForTheAuditSaveToCommitAsync(db);
        }

        await using var verify = fixture.NewContext();
        var stored = await verify.Set<ActivityType>().SingleAsync(type => type.Id == fixture.ExistingTypeId);
        stored.StagingSchemaJson.Should().Be(priorStagedSchema);
        stored.WbaToolKey.Should().Be("cbd");
        stored.Name.Should().Be(Fixture.ExistingName);
    }

    /// <summary>
    /// The audit pipeline's catch saves this request's context. Nothing dirty may be left for it, and the save it
    /// performs must write nothing.
    /// </summary>
    private static async Task AssertNothingForTheAuditSaveToCommitAsync(ApplicationDbContext db)
    {
        db.ChangeTracker.Entries()
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Should().BeEmpty("the audit pipeline's catch saves this context, so anything dirty here would be committed");

        (await db.SaveChangesAsync()).Should().Be(0);
    }

    /// <summary>The builder's own defaults for a new type: valid DSL payloads the parsers accept.</summary>
    private static async Task<ActivityTypeEditorDto> NewTypeDefaultsAsync(Fixture fixture)
    {
        await using var db = fixture.NewContext();
        return await new GetActivityTypeEditorQueryHandler(db).Handle(
            new GetActivityTypeEditorQuery(null, TestPrincipals.Administrator()),
            CancellationToken.None);
    }

    private static SaveActivityTypeDraftCommand Command(
        int? activityTypeId,
        string key,
        string name,
        ActivityScope scope,
        int? scopeId,
        string? wbaToolKey,
        ActivityTypeEditorDto payloads,
        System.Security.Claims.ClaimsPrincipal principal)
        => new(
            activityTypeId,
            key,
            name,
            activityTypeId is null ? null : Fixture.ExistingDescription,
            scope,
            scopeId,
            true,
            wbaToolKey,
            payloads.DraftSchemaJson,
            payloads.DraftWorkflowJson,
            payloads.DraftCreditRulesJson,
            payloads.DraftDisplayFieldsJson,
            ActorUserId,
            principal);

    /// <summary>
    /// One InMemory store shared by several contexts, so each handler call starts with a tracker holding only what
    /// it loaded, and each read-back sees only what was saved.
    /// </summary>
    private sealed class Fixture
    {
        public const string ExistingKey = "wba_under_test";
        public const string ExistingName = "WBA under test";
        public const string ExistingDescription = "A Global type keyed 'cbd'.";

        private readonly string _databaseName = Guid.NewGuid().ToString();

        public int ExistingTypeId { get; private set; }
        public int InstitutionId { get; private set; }

        public ApplicationDbContext NewContext()
            => new(new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(_databaseName)
                .Options);

        public static async Task<Fixture> CreateAsync(bool publishExistingType = false, bool stageExistingDraft = false)
        {
            var fixture = new Fixture();
            await using var db = fixture.NewContext();

            var institution = new Institution { Name = "Institution A", ShortCode = "IA", IsActive = true, CreatedOn = DateTime.UtcNow };
            db.Institutions.Add(institution);

            // The three instruments the generic rated seeds are keyed to. Descriptions do not matter here.
            db.WbaTools.AddRange(
                new WbaTool { Key = "cbd", Name = "CBD" },
                new WbaTool { Key = "dops", Name = "DOPS" },
                new WbaTool { Key = "mini_cex", Name = "Mini-CEX" });

            var existing = new ActivityType
            {
                Key = ExistingKey,
                Name = ExistingName,
                Description = ExistingDescription,
                Scope = ActivityScope.Global,
                IsActive = true,
                OwnerUserId = "seed-system",
                CreatedOn = DateTime.UtcNow,
                WbaToolKey = "cbd"
            };

            if (publishExistingType || stageExistingDraft)
            {
                var defaults = await new GetActivityTypeEditorQueryHandler(db).Handle(
                    new GetActivityTypeEditorQuery(null, TestPrincipals.Administrator()),
                    CancellationToken.None);
                existing.SaveDraft(defaults.DraftSchemaJson, defaults.DraftWorkflowJson, defaults.DraftCreditRulesJson, defaults.DraftDisplayFieldsJson, "seed-system");

                if (publishExistingType)
                {
                    existing.PublishDraft("seed-system");
                }
            }

            db.Set<ActivityType>().Add(existing);
            await db.SaveChangesAsync();

            fixture.ExistingTypeId = existing.Id;
            fixture.InstitutionId = institution.Id;
            return fixture;
        }
    }
}
