using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Epas;
using Wombat.Domain.Epas;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Epas;

/// <summary>
/// T122 — the instrument vocabulary the two admin pickers read: the builder's "This tool is" select and the
/// curriculum item editor's tool checkboxes.
/// </summary>
/// <remarks>
/// Both pickers bind the <see cref="WbaToolDto.Key" /> and print the <see cref="WbaToolDto.Name" />, so a projection
/// that swapped the two would store display names as keys, and every one of them would then be refused as an
/// unknown instrument. The order is by name because that is what an administrator scans, not the key or the id.
/// </remarks>
public sealed class GetWbaToolsQueryTests
{
    [Fact]
    public async Task ReturnsEveryInstrumentsKeyNameAndDescription_OrderedByName()
    {
        var databaseName = Guid.NewGuid().ToString();
        await using (var seed = CreateDbContext(databaseName))
        {
            // Inserted so that neither id order nor key order is name order: "a_test_instrument" sorts first by key
            // and last by name, and the ids run backwards through the names.
            seed.WbaTools.AddRange(
                new WbaTool { Id = 6, Key = "a_test_instrument", Name = "Zeta instrument (test only)", Description = null },
                new WbaTool { Id = 5, Key = "reflective_exercise", Name = "Reflective exercise", Description = "A written or verbal reflection on a challenging case." },
                new WbaTool { Id = 4, Key = "portfolio_review", Name = "Portfolio and logbook review", Description = null },
                new WbaTool { Id = 3, Key = "mini_cex", Name = "Mini-CEX", Description = "Mini-Clinical Evaluation Exercise." },
                new WbaTool { Id = 2, Key = "direct_observation", Name = "Direct observation", Description = "Observation of the trainee in routine clinical practice." },
                new WbaTool { Id = 1, Key = "cbd", Name = "CBD", Description = "Case-Based Discussion." });
            await seed.SaveChangesAsync();
        }

        await using var dbContext = CreateDbContext(databaseName);
        var tools = await new GetWbaToolsQueryHandler(dbContext).Handle(new GetWbaToolsQuery(), CancellationToken.None);

        tools.Select(tool => tool.Name).Should().Equal(
            "CBD",
            "Direct observation",
            "Mini-CEX",
            "Portfolio and logbook review",
            "Reflective exercise",
            "Zeta instrument (test only)");

        tools.Should().Equal(
            new WbaToolDto("cbd", "CBD", "Case-Based Discussion."),
            new WbaToolDto("direct_observation", "Direct observation", "Observation of the trainee in routine clinical practice."),
            new WbaToolDto("mini_cex", "Mini-CEX", "Mini-Clinical Evaluation Exercise."),
            new WbaToolDto("portfolio_review", "Portfolio and logbook review", null),
            new WbaToolDto("reflective_exercise", "Reflective exercise", "A written or verbal reflection on a challenging case."),
            new WbaToolDto("a_test_instrument", "Zeta instrument (test only)", null));
    }

    [Fact]
    public async Task AnEmptyVocabulary_ReturnsAnEmptyList()
    {
        // Before PaediatricCatalogueSeeder has run (a fresh database, a test host) the pickers must render with only
        // their "Not a WBA instrument" / "Any instrument" option, not fail.
        await using var dbContext = CreateDbContext(Guid.NewGuid().ToString());

        var tools = await new GetWbaToolsQueryHandler(dbContext).Handle(new GetWbaToolsQuery(), CancellationToken.None);

        tools.Should().BeEmpty();
    }

    private static ApplicationDbContext CreateDbContext(string databaseName)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options);
}
