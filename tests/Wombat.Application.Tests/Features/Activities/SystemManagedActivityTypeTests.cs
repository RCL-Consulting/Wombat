using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Activities.Commands.SaveActivityTypeDraft;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Queries.GetActivityTypeEditor;
using Wombat.Application.Features.Activities.Queries.ListActivityTypes;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Activities;
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
    /// The builder has no way to say "system-managed": the command carries no such value and the editor shows none, so a
    /// type made in the builder is filed by people, as every builder-made type before T162 was.
    /// </summary>
    [Fact]
    public async Task ATypeMadeInTheBuilder_IsNotSystemManaged_AndTheBuilderCannotSayOtherwise()
    {
        typeof(SaveActivityTypeDraftCommand).GetProperties().Select(property => property.Name)
            .Should().NotContain(name => name.Contains("SystemManaged"), "the builder does not expose the flag");
        typeof(ActivityTypeEditorDto).GetProperties().Select(property => property.Name)
            .Should().NotContain(name => name.Contains("SystemManaged"));

        var databaseName = Guid.NewGuid().ToString();
        var defaults = await EditorAsync(databaseName, activityTypeId: null);

        await using (var db = CreateDb(databaseName))
        {
            await new SaveActivityTypeDraftCommandHandler(db).Handle(
                SaveCommand(defaults, activityTypeId: null, key: "local_audit", name: "Local audit"),
                CancellationToken.None);
        }

        await using var verify = CreateDb(databaseName);
        (await verify.ActivityTypes.SingleAsync(type => type.Key == "local_audit")).SystemManaged.Should().BeFalse();
    }

    /// <summary>
    /// What an administrator does to msf_cpsa in the builder (open, edit, save, publish) keeps it system-managed. The
    /// save writes every metadata column it carries, so a flag it carried would be reset by every save.
    /// </summary>
    [Fact]
    public async Task SavingAndPublishingASystemManagedTypeInTheBuilder_KeepsItSystemManaged()
    {
        var databaseName = Guid.NewGuid().ToString();
        int typeId;
        await using (var db = CreateDb(databaseName))
        {
            var type = AddType(db, 0, "msf_cpsa", systemManaged: true);
            await db.SaveChangesAsync();
            typeId = type.Id;
        }

        var loaded = await EditorAsync(databaseName, typeId);

        await using (var db = CreateDb(databaseName))
        {
            await new SaveActivityTypeDraftCommandHandler(db).Handle(
                SaveCommand(loaded, typeId, loaded.Key, "Multi-Source Feedback (renamed)"),
                CancellationToken.None);
        }

        await using (var db = CreateDb(databaseName))
        {
            await new Wombat.Application.Features.Activities.Commands.PublishActivityTypeDraft.PublishActivityTypeDraftCommandHandler(db)
                .Handle(
                    new Wombat.Application.Features.Activities.Commands.PublishActivityTypeDraft.PublishActivityTypeDraftCommand(
                        typeId, "admin-user", TestPrincipals.Administrator()),
                    CancellationToken.None);
        }

        await using var verify = CreateDb(databaseName);
        var stored = await verify.ActivityTypes.SingleAsync(type => type.Id == typeId);
        stored.Name.Should().Be("Multi-Source Feedback (renamed)", "guard: the save reached the store");
        stored.Version.Should().Be(2, "guard: the publish reached the store");
        stored.SystemManaged.Should().BeTrue();
    }

    private static async Task<ActivityTypeEditorDto> EditorAsync(string databaseName, int? activityTypeId)
    {
        await using var db = CreateDb(databaseName);
        return await new GetActivityTypeEditorQueryHandler(db).Handle(
            new GetActivityTypeEditorQuery(activityTypeId, TestPrincipals.Administrator()),
            CancellationToken.None);
    }

    private static SaveActivityTypeDraftCommand SaveCommand(ActivityTypeEditorDto payloads, int? activityTypeId, string key, string name)
        => new(
            activityTypeId,
            key,
            name,
            payloads.Description,
            ActivityScope.Institution,
            InstitutionId,
            true,
            WbaToolKey: null,
            payloads.DraftSchemaJson,
            payloads.DraftWorkflowJson,
            payloads.DraftCreditRulesJson,
            payloads.DraftDisplayFieldsJson,
            "admin-user",
            TestPrincipals.Administrator());

    private static ActivityType AddType(ApplicationDbContext db, int id, string key, bool systemManaged)
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
            Scope = ActivityScope.Institution,
            ScopeId = InstitutionId,
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
