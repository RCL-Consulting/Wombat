using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common;
using Wombat.Application.Features.Activities.Commands.SaveActivityTypeDraft;
using Wombat.Application.Features.Activities.Queries.GetActivityTypeEditor;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Activities;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.Features.Activities;

/// <summary>
/// Details' Key and Name (T398, flow 17, S10): the validator's four words under their fields, and the key taken as a
/// refusal on Key, before anything is staged.
/// </summary>
public sealed class SaveActivityTypeDraftValidatorTests
{
    [Theory]
    [InlineData("", "A name", "Key", "Enter a key.")]
    [InlineData("   ", "A name", "Key", "Enter a key.")]
    [InlineData("a_key", "", "Name", "Enter a name.")]
    [InlineData("a_key", "  ", "Name", "Enter a name.")]
    public void ABlankKeyOrName_IsRefusedUnderItsField(string key, string name, string field, string words)
    {
        var result = new SaveActivityTypeDraftCommandValidator().Validate(Command(key, name));

        result.Errors.Should().ContainSingle().Which.Should().BeEquivalentTo(new { PropertyName = field, ErrorMessage = words });
    }

    [Fact]
    public void AKeyOver100_OrANameOver200_IsRefusedUnderItsField()
    {
        var result = new SaveActivityTypeDraftCommandValidator().Validate(Command(new string('k', 101), new string('n', 201)));

        result.Errors.Select(error => (error.PropertyName, error.ErrorMessage)).Should().BeEquivalentTo(
        [
            ("Key", "A key is at most 100 characters."),
            ("Name", "A name is at most 200 characters.")
        ]);
    }

    /// <summary>The fix pass (N-1): a description past its 2,000-character column is refused under Description.</summary>
    [Fact]
    public void ADescriptionOver2000_IsRefusedUnderItsField()
    {
        var validator = new SaveActivityTypeDraftCommandValidator();

        validator.Validate(Command("a_key", "A name") with { Description = new string('d', 2001) }).Errors
            .Should().ContainSingle().Which.Should().BeEquivalentTo(
                new { PropertyName = "Description", ErrorMessage = "A description is at most 2,000 characters." });
        validator.Validate(Command("a_key", "A name") with { Description = $" {new string('d', 2000)} " })
            .IsValid.Should().BeTrue();
    }

    /// <summary>Judged as stored, trimmed: 100 characters with spaces round them is a key of 100.</summary>
    [Fact]
    public void TheLongest_AreAccepted_AsTheHandlerStoresThem()
        => new SaveActivityTypeDraftCommandValidator()
            .Validate(Command($"  {new string('k', 100)}  ", $" {new string('n', 200)} "))
            .IsValid.Should().BeTrue();

    /// <summary>R3-C22: the key taken is refused on Key, in the built sentence, and nothing is staged.</summary>
    [Fact]
    public async Task TheKeyTaken_IsARefusalOnKey_AndNothingIsStaged()
    {
        await using var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        db.ActivityTypes.Add(new ActivityType
        {
            Key = "kgk_teaching_log", Name = "Taken", Scope = ActivityScope.Global, OwnerUserId = "someone", CreatedOn = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var save = () => new SaveActivityTypeDraftCommandHandler(db, FakeUserDirectory.Empty).Handle(
            Command(" kgk_teaching_log ", "KGK Teaching Session Log"), CancellationToken.None);

        var refusal = (await save.Should().ThrowAsync<FieldRefusalException>()).Which;
        refusal.Field.Should().Be("Key");
        refusal.Message.Should().Be("An activity type with key 'kgk_teaching_log' already exists.");
        db.ChangeTracker.Entries().Should().OnlyContain(entry => entry.State == EntityState.Unchanged);
        (await db.ActivityTypes.CountAsync()).Should().Be(1);
    }

    private static SaveActivityTypeDraftCommand Command(string key, string name) => new(
        null,
        key,
        name,
        null,
        ActivityScope.Global,
        null,
        true,
        null,
        StartSchemaJson,
        StartWorkflowJson,
        StartCreditRulesJson,
        "[]",
        "admin-user",
        TestPrincipals.Administrator());

    private const string StartSchemaJson = """{ "version": 1, "sections": [ { "key": "details", "title": "Details", "fields": [ { "key": "title", "type": "text", "label": "Title", "required": true } ] } ] }""";

    private const string StartWorkflowJson = """{ "version": 1, "initial_state": "draft", "states": [ { "key": "draft", "label": "Draft" }, { "key": "submitted", "label": "Submitted", "terminal": true } ], "transitions": [ { "key": "submit", "from": "draft", "to": "submitted", "actor": "subject", "validation": "all" } ] }""";

    private const string StartCreditRulesJson = """{ "counts_for": [] }""";
}
