using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Wombat.Application.Features.Epas;
using Wombat.Application.Features.Epas.Commands.UpdateEntrustmentScale;
using Wombat.Domain.Activities.Schema;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Activities;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Infrastructure.Tests.Persistence;

/// <summary>
/// T253: an administrator renames a seeded ladder on the scale editor, and every field the seeds bind to it stays bound.
/// </summary>
/// <remarks>
/// Before T253 every <c>*_cpsa</c> seed bound "CPSA Paediatric Entrustment Scale v11.1" by name, and the four generic seeds
/// bound "O-R Scale", so this rename emptied every rung picker and dropped every rating's ladder at once (the rename was
/// refused instead, so the ladder could not be renamed at all). The seeds now bind by seed key, which no command writes.
/// The whole seeded corpus is booted, as a startup does, and the rename goes through the command the scale editor sends.
/// </remarks>
public sealed class SeededScaleRenameTests
{
    public static TheoryData<string, string, string[], string[]> SeededLadders => new()
    {
        {
            "cpsa:scale:v11.1",
            "Paediatric ladder",
            ["1", "2", "3a", "3b", "4", "5"],
            [
                "cbd_cpsa", "cca_cpsa", "chart_stimulated_recall_cpsa", "direct_observation_cpsa", "dops_cpsa",
                "mini_cex_cpsa", "msf_cpsa", "rca_cpsa"
            ]
        },
        {
            "demo:scale:o-r",
            "Observation to entrustment",
            ["Observe only", "Direct supervision", "Indirect supervision", "Independent", "Supervises others"],
            ["acat", "cbd", "dops", "mini_cex"]
        }
    };

    [Theory]
    [MemberData(nameof(SeededLadders))]
    public async Task RenamingASeededLadder_LeavesEveryFieldTheSeedsBindToItBound_AndWarnsOfNothing(
        string seedKey, string newName, string[] rungs, string[] bindingTypes)
    {
        var database = new SeedDatabase();
        await database.BootAsync();

        int scaleId;
        await using (var dbContext = database.NewContext())
        {
            var scale = await dbContext.EntrustmentScales
                .AsNoTracking()
                .Include(entity => entity.Levels)
                .SingleAsync(entity => entity.SeedKey == seedKey);
            scaleId = scale.Id;

            var saved = await new UpdateEntrustmentScaleCommandHandler(dbContext).Handle(
                new UpdateEntrustmentScaleCommand(
                    scale.Id,
                    newName,
                    scale.Description,
                    scale.Levels
                        .OrderBy(level => level.Order)
                        .Select(level => new EntrustmentLevelUpdate(level.Id, level.Order, level.Label, level.Description))
                        .ToList(),
                    Administrator()),
                CancellationToken.None);

            saved.Scale.Name.Should().Be(newName);
            saved.RenameWarning.Should().BeNull("no seeded schema binds the ladder by its name");
        }

        await using var reader = database.NewContext();
        (await reader.EntrustmentScales.SingleAsync(entity => entity.Id == scaleId)).Name.Should().Be(newName);

        // Every scale field of every seeded type's current published version.
        var current = await reader.ActivityTypes
            .AsNoTracking()
            .SelectMany(type => type.Versions
                .Where(version => version.Version == type.Version)
                .Select(version => new { type.Key, version.SchemaJson }))
            .ToListAsync();
        var scaleFields = current
            .SelectMany(type => FormSchemaParser.Parse(type.SchemaJson).Sections
                .SelectMany(section => section.Fields)
                .Where(field => !string.IsNullOrWhiteSpace(field.ScaleKey))
                .Select(field => (TypeKey: type.Key, FieldKey: field.Key, ScaleKey: field.ScaleKey!)))
            .ToList();

        var resolved = await EntrustmentScaleBindings.ResolveAsync(reader, scaleFields.Select(field => field.ScaleKey));
        var boundHere = scaleFields.Where(field => resolved.GetValueOrDefault(field.ScaleKey) == scaleId).ToList();

        boundHere.Select(field => field.TypeKey).Distinct().Order(StringComparer.Ordinal)
            .Should().Equal(bindingTypes, "every seeded type that rates on this ladder is still bound to it");
        boundHere.Should().OnlyContain(field => field.ScaleKey == ScaleBinding.ForSeedKey(seedKey));
        scaleFields.Should().OnlyContain(
            field => resolved.ContainsKey(field.ScaleKey),
            "no seeded scale field binds nothing, after either rename");

        // What the assessor sees: the rung picker the form renders for each field still offers the ladder's rungs.
        var picker = new ActivityReferenceDataService(reader);
        foreach (var field in boundHere)
        {
            (await picker.GetEntrustmentScaleLevelOptionsAsync(field.ScaleKey)).Select(option => option.Label)
                .Should().Equal(rungs, "{0}.{1} still offers the ladder's rungs", field.TypeKey, field.FieldKey);
        }
    }

    private static ClaimsPrincipal Administrator()
        => new(new ClaimsIdentity([new Claim(ClaimTypes.Role, WombatRoles.Administrator)], "test"));

    private sealed class SeedDatabase
    {
        private readonly InMemoryDatabaseRoot _root = new();
        private readonly string _name = Guid.NewGuid().ToString();

        public ApplicationDbContext NewContext()
            => new(new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(_name, _root)
                .Options);

        /// <summary>The seeding half of one startup, in the order <c>Program.cs</c> runs it.</summary>
        public async Task BootAsync()
        {
            await using var dbContext = NewContext();
            await new DataSeeder(dbContext).SeedAsync();
            await new PaediatricCatalogueSeeder(dbContext).SeedAsync();
        }
    }
}
