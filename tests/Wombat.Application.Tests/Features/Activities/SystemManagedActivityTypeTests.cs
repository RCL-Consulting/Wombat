using Wombat.Tests.Shared;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Activities.Commands.SaveActivityTypeDraft;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Queries.GetActivityTypeEditor;
using Wombat.Application.Features.Activities.Queries.ListActivityTypes;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Activities;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Activities;

/// <summary>
/// T162. A type only the system writes (<see cref="ActivityType.SystemManaged" />, <c>msf_cpsa</c>) is not on the type
/// picker, and the builder can neither set nor clear the flag.
/// </summary>
/// <remarks>
/// The create refusal is in <c>MsfEvidenceFanOutTests</c>, against the real <c>msf_cpsa</c> seed, beside the release it
/// must not stop.
/// </remarks>
public sealed class SystemManagedActivityTypeTests
{
    private const int InstitutionId = 2;
    private const int CollegeId = 10;
    private const int SpecialityId = 20;

    /// <summary>
    /// The picker leaves a system-managed type out, and nothing else changes: its twin, identical but for the flag, is
    /// still offered. Both with a subject (the ladder narrowing runs) and without (the builder's preview).
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("trainee-1")]
    public async Task ThePicker_LeavesOutASystemManagedType_AndOnlyThat(string? subjectUserId)
    {
        await using var db = CreateDb();
        AddType(db, 1, "msf_cpsa", systemManaged: true);
        AddType(db, 2, "msf_twin", systemManaged: false);
        AddType(db, 3, "reflective_note", systemManaged: false);
        await db.SaveChangesAsync();

        var offered = await new ListActivityTypesQueryHandler(db).Handle(
            new ListActivityTypesQuery(TestPrincipals.Trainee("trainee-1", InstitutionId), subjectUserId),
            CancellationToken.None);

        offered.Select(type => type.Key).Should().BeEquivalentTo("msf_twin", "reflective_note");
    }

    /// <summary>
    /// The builder has no way to say "system-managed": the command carries no such value, so a type made in the builder is
    /// filed by people, as every builder-made type before T162 was. The editor reads the flag out (T398, Q9: the release's
    /// notice, read-only) but nothing sends it back.
    /// </summary>
    [Fact]
    public async Task ATypeMadeInTheBuilder_IsNotSystemManaged_AndTheBuilderCannotSayOtherwise()
    {
        typeof(SaveActivityTypeDraftCommand).GetProperties().Select(property => property.Name)
            .Should().NotContain(name => name.Contains("SystemManaged"), "the builder does not send the flag");
        typeof(ActivityTypeEditorDto).GetProperty(nameof(ActivityTypeEditorDto.SystemManaged))!.SetMethod!
            .ReturnParameter.GetRequiredCustomModifiers()
            .Should().Contain(typeof(System.Runtime.CompilerServices.IsExternalInit), "the editor only reads it out (T398, Q9)");

        var databaseName = Guid.NewGuid().ToString();
        var defaults = await EditorAsync(databaseName, activityTypeId: null);

        await using (var db = CreateDb(databaseName))
        {
            await new SaveActivityTypeDraftCommandHandler(db, FakeUserDirectory.Empty).Handle(
                SaveCommand(defaults, activityTypeId: null, key: "local_audit", name: "Local audit"),
                CancellationToken.None);
        }

        await using var verify = CreateDb(databaseName);
        (await verify.ActivityTypes.SingleAsync(type => type.Key == "local_audit")).SystemManaged.Should().BeFalse();
    }

    /// <summary>
    /// T398 (flow 17, Q9, T334): a system-managed type is written by its release alone, so the builder's save and publish
    /// are refused on msf_cpsa to an Administrator and to the College that owns its discipline, and nothing is staged.
    /// Inverted on purpose: until T334 this test saved and published msf_cpsa as an Administrator and checked only that
    /// the flag survived, and a save in the builder handed a release's type over. A draft it already holds may still be
    /// discarded (the operator's F1, below).
    /// </summary>
    [Fact]
    public async Task SavingAndPublishingASystemManagedTypeInTheBuilder_IsRefused_ToEveryone()
    {
        const string refusal =
            "Nothing was saved. Multi-Source Feedback (Paediatrics) is written by the MSF release, so it cannot be changed here.";
        var databaseName = Guid.NewGuid().ToString();
        var typeId = await AddMsfWithADraftAsync(databaseName);

        foreach (var principal in new[] { TestPrincipals.Administrator(), TestPrincipals.CollegeAdmin(CollegeId) })
        {
            var loaded = await EditorAsync(databaseName, typeId, principal);
            loaded.CanWrite.Should().BeFalse("the builder offers nothing to save on a release's type");

            await using var db = CreateDb(databaseName);
            var save = () => new SaveActivityTypeDraftCommandHandler(db, FakeUserDirectory.Empty).Handle(
                SaveCommand(loaded, typeId, loaded.Key, "Multi-Source Feedback (renamed)", principal, ActivityScope.Speciality, SpecialityId),
                CancellationToken.None);
            await save.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage(refusal);

            var publish = () => new Wombat.Application.Features.Activities.Commands.PublishActivityTypeDraft.PublishActivityTypeDraftCommandHandler(db, FakeUserDirectory.Empty)
                .Handle(
                    new Wombat.Application.Features.Activities.Commands.PublishActivityTypeDraft.PublishActivityTypeDraftCommand(
                        typeId, "admin-user", principal),
                    CancellationToken.None);
            await publish.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage(refusal);

            db.ChangeTracker.Entries().Where(entry => entry.State != EntityState.Unchanged)
                .Should().BeEmpty("a refusal stages nothing for the audit pipeline's save to commit");
        }

        await using var verify = CreateDb(databaseName);
        var stored = await verify.ActivityTypes.SingleAsync(type => type.Id == typeId);
        stored.Name.Should().Be("Multi-Source Feedback (Paediatrics)", "the save was refused");
        stored.Version.Should().Be(1, "the publish was refused");
        stored.HasDraft.Should().BeTrue();
        stored.SystemManaged.Should().BeTrue();
    }

    /// <summary>
    /// T398, the operator's F1 (2026-11-10): a draft a system-managed type holds from a builder save before T334 may be
    /// discarded by whoever the scope admits, an Administrator or the College that owns its discipline, since the seed
    /// refresher skips a type with a draft in flight and nothing else could clear it. The discard drops staged work only:
    /// the published version and the flag are unchanged. An institution's admin, whom the scope does not admit, is refused,
    /// and the editor offers the discard to exactly those it would accept.
    /// </summary>
    [Theory]
    [InlineData("administrator")]
    [InlineData("college-admin")]
    public async Task ADraftASystemManagedTypeHolds_MayBeDiscarded_ByWhoeverItsScopeAdmits(string caller)
    {
        var databaseName = Guid.NewGuid().ToString();
        var typeId = await AddMsfWithADraftAsync(databaseName);
        var principal = caller == "administrator" ? TestPrincipals.Administrator() : TestPrincipals.CollegeAdmin(CollegeId);
        var outsider = TestPrincipals.InstitutionalAdmin(InstitutionId);

        (await EditorAsync(databaseName, typeId, principal)).CanDiscardDraft.Should().BeTrue();
        (await EditorAsync(databaseName, typeId, outsider)).CanDiscardDraft.Should().BeFalse();

        await using (var db = CreateDb(databaseName))
        {
            var refused = () => new Wombat.Application.Features.Activities.Commands.DiscardActivityTypeDraft.DiscardActivityTypeDraftCommandHandler(db, FakeUserDirectory.Empty)
                .Handle(
                    new Wombat.Application.Features.Activities.Commands.DiscardActivityTypeDraft.DiscardActivityTypeDraftCommand(typeId, outsider),
                    CancellationToken.None);
            await refused.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("You do not have permission to modify activity types in that speciality.");
        }

        await using (var db = CreateDb(databaseName))
        {
            var editor = await new Wombat.Application.Features.Activities.Commands.DiscardActivityTypeDraft.DiscardActivityTypeDraftCommandHandler(db, FakeUserDirectory.Empty)
                .Handle(
                    new Wombat.Application.Features.Activities.Commands.DiscardActivityTypeDraft.DiscardActivityTypeDraftCommand(typeId, principal),
                    CancellationToken.None);
            editor.HasDraft.Should().BeFalse();
            editor.CanDiscardDraft.Should().BeFalse("there is nothing left to discard");
            editor.CanWrite.Should().BeFalse("the discard opens nothing else");
        }

        await using var verify = CreateDb(databaseName);
        var stored = await verify.ActivityTypes.SingleAsync(type => type.Id == typeId);
        stored.HasDraft.Should().BeFalse("the draft was discarded");
        stored.Version.Should().Be(1, "what is published is unchanged");
        stored.SystemManaged.Should().BeTrue();
    }

    /// <summary>msf_cpsa, published by the release, with a draft staged on it as a builder save before T334 left one.</summary>
    private static async Task<int> AddMsfWithADraftAsync(string databaseName)
    {
        await using var db = CreateDb(databaseName);
        db.Colleges.Add(new College { Id = CollegeId, Name = "College of Paediatricians", ShortCode = "CPSA" });
        db.Specialities.Add(new Speciality { Id = SpecialityId, CollegeId = CollegeId, Name = "Paediatrics" });
        var type = AddType(db, 0, "msf_cpsa", systemManaged: true, ActivityScope.Speciality, SpecialityId);
        type.Name = "Multi-Source Feedback (Paediatrics)";
        type.SaveDraft(type.SchemaJson!, type.WorkflowJson!, type.CreditRulesJson!, "[]", "admin-user");
        await db.SaveChangesAsync();
        return type.Id;
    }

    private static async Task<ActivityTypeEditorDto> EditorAsync(string databaseName, int? activityTypeId, ClaimsPrincipal? principal = null)
    {
        await using var db = CreateDb(databaseName);
        return (await new GetActivityTypeEditorQueryHandler(db, FakeUserDirectory.Empty).Handle(
            new GetActivityTypeEditorQuery(activityTypeId, principal ?? TestPrincipals.Administrator()),
            CancellationToken.None))!;
    }

    private static SaveActivityTypeDraftCommand SaveCommand(
        ActivityTypeEditorDto payloads,
        int? activityTypeId,
        string key,
        string name,
        ClaimsPrincipal? principal = null,
        ActivityScope scope = ActivityScope.Institution,
        int? scopeId = InstitutionId)
        => new(
            activityTypeId,
            key,
            name,
            payloads.Description,
            scope,
            scopeId,
            true,
            WbaToolKey: null,
            payloads.DraftSchemaJson,
            payloads.DraftWorkflowJson,
            payloads.DraftCreditRulesJson,
            payloads.DraftDisplayFieldsJson,
            "admin-user",
            principal ?? TestPrincipals.Administrator());

    private static ActivityType AddType(
        ApplicationDbContext db,
        int id,
        string key,
        bool systemManaged,
        ActivityScope scope = ActivityScope.Institution,
        int? scopeId = InstitutionId)
    {
        const string schemaJson = """
            {"version":1,"sections":[{"key":"d","title":"D","fields":[{"key":"note","type":"longtext","label":"Note"}]}]}
            """;
        const string workflowJson = """
            {"version":1,"initial_state":"draft","states":[{"key":"draft","label":"Draft"},{"key":"done","label":"Done","terminal":true}],"transitions":[{"key":"submit","from":"draft","to":"done","actor":"subject","validation":"all"}]}
            """;
        const string creditRulesJson = """{"counts_for":[]}""";

        var type = new ActivityType
        {
            Id = id,
            Key = key,
            Name = key,
            IsActive = true,
            OwnerUserId = "seed-system",
            CreatedOn = DateTime.UtcNow,
            Scope = scope,
            ScopeId = scopeId,
            SystemManaged = systemManaged
        };

        type.SaveDraft(schemaJson, workflowJson, creditRulesJson, "[]", "seed-system");
        type.PublishDraft("seed-system");

        db.ActivityTypes.Add(type);
        return type;
    }

    private static ApplicationDbContext CreateDb(string? databaseName = null)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString())
            .Options);
}
