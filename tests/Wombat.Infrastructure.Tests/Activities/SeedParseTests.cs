using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Credit;
using Wombat.Domain.Activities.Schema;
using Wombat.Domain.Activities.Workflow;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Infrastructure.Tests.Activities;

public sealed class SeedParseTests
{
    [Fact]
    public void AllSeedJsonFiles_ParseCleanly()
    {
        foreach (var directory in EnumerateSeedDirectories())
        {
            var schemaJson = File.ReadAllText(Path.Combine(directory, "schema.json"));
            var workflowJson = File.ReadAllText(Path.Combine(directory, "workflow.json"));
            var creditJson = File.ReadAllText(Path.Combine(directory, "credit.json"));

            var schema = FormSchemaParser.Parse(schemaJson);
            var workflow = WorkflowParser.Parse(workflowJson);
            var creditRules = CreditRulesParser.Parse(creditJson);

            schema.Sections.Should().NotBeEmpty(directory);
            workflow.States.Should().NotBeEmpty(directory);
            creditRules.CountsFor.Should().NotBeNull(directory);
        }
    }

    [Fact]
    public async Task DataSeeder_SeedsStarterActivityTypesAndProcedureCatalogue()
    {
        await using var dbContext = CreateDbContext();
        var seeder = new DataSeeder(dbContext);

        await seeder.SeedAsync();

        dbContext.ProcedureCatalogueEntries.Count().Should().BeGreaterThanOrEqualTo(20);
        dbContext.ActivityTypes.Should().HaveCount(10);
        dbContext.ActivityTypes.Should().OnlyContain(entity => entity.Version == 1 && entity.IsActive);
        dbContext.Set<ActivityTypeVersion>().Should().HaveCount(10);
        dbContext.ActivityTypes.Select(entity => entity.Key).Should().Contain(["mini_cex", "procedure_log", "journal_club"]);
    }

    [Fact]
    public async Task DataSeeder_DoesNotOverwriteExistingActivityType()
    {
        await using var dbContext = CreateDbContext();
        dbContext.ActivityTypes.Add(new ActivityType
        {
            Key = "mini_cex",
            Name = "Custom Mini-CEX",
            Description = "Customized",
            Scope = ActivityScope.Global,
            OwnerUserId = "tester",
            CreatedOn = DateTime.UtcNow,
            SchemaJson = """{"version":1,"sections":[{"key":"a","title":"A","fields":[{"key":"b","type":"text","label":"B"}]}]}""",
            WorkflowJson = """{"version":1,"initial_state":"logged","states":[{"key":"logged","label":"Logged","terminal":true}],"transitions":[]}""",
            CreditRulesJson = """{"counts_for":[]}""",
            DisplayFieldsJson = """["b"]""",
            Version = 1,
            IsActive = true
        });
        await dbContext.SaveChangesAsync();

        var seeder = new DataSeeder(dbContext);
        await seeder.SeedAsync();

        var activityType = await dbContext.ActivityTypes.SingleAsync(entity => entity.Key == "mini_cex");
        activityType.Name.Should().Be("Custom Mini-CEX");
        dbContext.ActivityTypes.Should().HaveCount(10);
    }

    /// <summary>
    /// Credit fires on any transition into a terminal state (ActivityService), and an abandoned
    /// request still carries a filled-in epa_id — so a terminal `declined` or `cancelled` counts a
    /// refused or withdrawn assessment toward the trainee's observation volume.
    /// </summary>
    /// <remarks>
    /// This was latent until T070 dropped `required: true` from the legacy seeds' assessor fields:
    /// before that, `decline` and `cancel` could not satisfy the full Submit-mode validation pass
    /// and so were unreachable. Making them usable made the credit path reachable with them.
    /// </remarks>
    [Fact]
    public void NoSeededWorkflow_MarksAnAbandonmentStateTerminal()
    {
        string[] abandonmentStates = ["declined", "cancelled", "rejected", "withdrawn"];

        foreach (var directory in EnumerateSeedDirectories())
        {
            var workflow = WorkflowParser.Parse(File.ReadAllText(Path.Combine(directory, "workflow.json")));

            foreach (var state in workflow.States.Where(state => state.Terminal))
            {
                abandonmentStates.Should().NotContain(
                    state.Key,
                    "a terminal '{0}' in {1} would award curriculum credit for an assessment that was refused or withdrawn",
                    state.Key,
                    Path.GetFileName(directory));
            }
        }
    }

    private static IEnumerable<string> EnumerateSeedDirectories()
        => Directory.EnumerateDirectories(Path.Combine(AppContext.BaseDirectory, "Activities", "Seeds"))
            .Where(path => !string.Equals(Path.GetFileName(path), "README.md", StringComparison.OrdinalIgnoreCase));

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }
}
