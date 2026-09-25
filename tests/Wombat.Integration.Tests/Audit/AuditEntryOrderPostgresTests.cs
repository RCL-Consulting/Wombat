using System.IO.Compression;
using System.Security.Claims;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Audit.Queries.ListAuditEntries;
using Wombat.Application.Features.DataRights;
using Wombat.Application.Features.Reporting;
using Wombat.Domain.Audit;
using Wombat.Domain.Identity;
using Wombat.Infrastructure;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Integration.Tests.Audit;

/// <summary>
/// T262 on a real PostgreSQL server: the audit log's pages and the access report's cut at 500 settle a tie of timestamps
/// by id, so a page boundary or the cut inside a tie neither repeats nor skips an entry.
/// </summary>
/// <remarks>
/// <para>
/// Before T262 both ordered by <see cref="AuditEntry.OccurredAt" /> alone. PostgreSQL's sort is not stable, and a query
/// with a different <c>OFFSET</c> or <c>LIMIT</c> can be sorted by a different method, so the order within a tie was
/// the server's to choose on each query. <c>ListAuditEntriesQueryHandlerTests</c> and
/// <c>AccessReportAuditEntriesTests</c> hold the rule on EF InMemory, whose sort is stable and so never skips or repeats;
/// only a server can show that. Here the <c>ORDER BY</c> is the one Npgsql sends, with the id compared as PostgreSQL
/// compares a <c>uuid</c>.
/// </para>
/// <para>
/// Isolated as <c>AuditOverlongActorPostgresTests</c> is: a migrated schema of its own, dropped in a finally and again
/// on dispose. The access report is resolved through <c>AddInfrastructure</c>, as the host resolves it, because the
/// builder is internal to Infrastructure.
/// </para>
/// </remarks>
public sealed class AuditEntryOrderPostgresTests : IAsyncLifetime
{
    private const string Actor = "t262-actor";

    private static readonly DateTime Tie = new(2026, 9, 20, 10, 0, 0, DateTimeKind.Utc);

    private readonly TestSchemas _schemas = new();

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _schemas.DropAllAsync();

    /// <summary>
    /// 120 entries on one timestamp, with ten newer and two older, paged 50 at a time as the audit page pages them: once as
    /// the server plans the query, and once with the table read and sorted rather than read through an index.
    /// </summary>
    /// <remarks>
    /// <para>
    /// On a table this small the planner reads the tie backwards through the <c>(ActorUserId, OccurredAt)</c> index,
    /// which returns it in the reverse of the order it was written, the same on every page. The tie is written in neither
    /// its ids' order nor their reverse, so that order is not the id's.
    /// </para>
    /// <para>
    /// Read and sorted, which is what the server does whenever it does not use the index, page 1 (<c>LIMIT 50</c>) is a
    /// top-N heapsort, and pages 2 and 3 (<c>OFFSET 50</c>, <c>OFFSET 100</c>) are a quicksort of every row. The two leave
    /// the tie in different orders. Without the id, on PostgreSQL 16.10, page 2 repeated ten entries page 1 had shown,
    /// and ten others were on no page (observed 2026-09-25). The layout below is the one that showed it: the newer entries
    /// written among the tie, not before it.
    /// </para>
    /// </remarks>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task TheAuditLog_Pages120EntriesOnOneTimestamp_EachOnOnePage_TiesSettledById(bool readAndSorted)
    {
        try
        {
            var schema = await MigratedSchemaAsync();

            await using (var arrange = NewContext(schema))
            {
                // Written in this order, each entry in a save of its own, because EF sorts one save's inserts by key:
                // 30 of the tie; the newer entries 1 to 10, entry 1 the newest; the other 90 of the tie; and the older
                // entries 131 and 132, 132 the oldest. The tie's ids, 11 to 130, go in the order 11, 58, 105, 32, ...
                // (47 steps, wrapping at 120).
                async Task WriteAsync(int number, DateTime occurredAt)
                {
                    arrange.AuditEntries.Add(Entry(number, occurredAt));
                    await arrange.SaveChangesAsync();
                }

                var tieIds = Enumerable.Range(0, 120).Select(k => 11 + k * 47 % 120).ToList();

                foreach (var number in tieIds.Take(30))
                {
                    await WriteAsync(number, Tie);
                }

                foreach (var number in Enumerable.Range(1, 10))
                {
                    await WriteAsync(number, Tie.AddSeconds(11 - number));
                }

                foreach (var number in tieIds.Skip(30))
                {
                    await WriteAsync(number, Tie);
                }

                await WriteAsync(131, Tie.AddSeconds(-1));
                await WriteAsync(132, Tie.AddSeconds(-2));
            }

            var pages = new List<IReadOnlyList<int>>();
            for (var page = 1; page <= 3; page++)
            {
                await using var db = NewContext(schema);
                if (readAndSorted)
                {
                    // Held open, so the handler's queries run on this session. Npgsql resets the settings when the
                    // connection goes back to its pool.
                    await db.Database.OpenConnectionAsync();
                    await db.Database.ExecuteSqlRawAsync(
                        "SET enable_indexscan = off; SET enable_indexonlyscan = off; SET enable_bitmapscan = off;");
                }

                var result = await new ListAuditEntriesQueryHandler(db).Handle(
                    new ListAuditEntriesQuery(
                        Administrator(), ActorUserId: Actor, From: Tie.AddHours(-1), To: Tie.AddHours(1), Page: page, PageSize: 50),
                    CancellationToken.None);

                result.TotalCount.Should().Be(132);
                pages.Add(result.Items.Select(item => Number(item.Id)).ToList());
            }

            pages.SelectMany(page => page).Should().OnlyHaveUniqueItems("no page repeats an entry another page showed")
                .And.BeEquivalentTo(Enumerable.Range(1, 132), "every entry is on a page");

            // Newest first; the tie from its highest id down.
            var expected = Enumerable.Range(1, 10)
                .Concat(Enumerable.Range(11, 120).Reverse())
                .Concat([131, 132])
                .Chunk(50)
                .ToList();
            pages.Should().BeEquivalentTo(expected, options => options.WithStrictOrdering());
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    [Fact]
    public async Task TheAccessReport_CutsAt500InsideATie_KeepingTheTiesHighestIds_NewestFirst()
    {
        try
        {
            var schema = await MigratedSchemaAsync();

            await using (var arrange = NewContext(schema))
            {
                // As AccessReportAuditEntriesTests: three newer entries with the lowest ids, then 500 on one timestamp,
                // added lowest id first. 503 in all, so the cut leaves out three of the tie.
                arrange.AuditEntries.Add(Entry(1, Tie.AddSeconds(3)));
                arrange.AuditEntries.Add(Entry(2, Tie.AddSeconds(2)));
                arrange.AuditEntries.Add(Entry(3, Tie.AddSeconds(1)));
                arrange.AuditEntries.AddRange(Enumerable.Range(4, 500).Select(n => Entry(n, Tie)));
                await arrange.SaveChangesAsync();
            }

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = TestDatabase.SchemaConnectionString(schema)
                })
                .Build();

            var services = new ServiceCollection()
                .AddLogging()
                .AddInfrastructure(configuration);

            // The PDF is best-effort in the report, and not what this test reads.
            services.AddScoped<IPortfolioPdfService, NoPortfolio>();

            await using var provider = services.BuildServiceProvider();
            await using var scope = provider.CreateAsyncScope();
            var export = await scope.ServiceProvider.GetRequiredService<IAccessReportBuilder>()
                .BuildAsync(Actor, CancellationToken.None);

            using var zip = new ZipArchive(new MemoryStream(export.ZipBytes), ZipArchiveMode.Read);
            await using var json = zip.GetEntry("data-export.json")!.Open();
            using var document = await JsonDocument.ParseAsync(json);
            var numbers = document.RootElement.GetProperty("auditEntries").EnumerateArray()
                .Select(entry => Number(entry.GetProperty("id").GetGuid()))
                .ToList();

            numbers.Should().Equal(
                new[] { 1, 2, 3 }.Concat(Enumerable.Range(7, 497).Reverse()),
                "the three newer entries come first, then the tie from its highest id down, and 4, 5 and 6 are cut");
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    /// <summary>An entry by the actor whose id is <paramref name="number" /> in its last group, so the ids sort as the numbers do.</summary>
    private static AuditEntry Entry(int number, DateTime occurredAt)
    {
        var entry = AuditEntry.Create(occurredAt, AuditCategory.Command, $"Cmd{number}", true, actorUserId: Actor);
        entry.Id = new Guid($"00000000-0000-0000-0000-{number:D12}");
        return entry;
    }

    private static int Number(Guid id) => int.Parse(id.ToString()[^12..]);

    private static ClaimsPrincipal Administrator()
        => new(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, "admin-t262"),
                new Claim(ClaimTypes.Role, WombatRoles.Administrator)
            ],
            "IntegrationTest",
            ClaimTypes.Name,
            ClaimTypes.Role));

    private sealed class NoPortfolio : IPortfolioPdfService
    {
        public Task<PortfolioExportResult> GenerateAsync(PortfolioExportRequest request, CancellationToken cancellationToken)
            => throw new NotSupportedException("The PDF is not part of this test.");
    }

    // ─── Schema helpers (as AuditOverlongActorPostgresTests) ─────────────────

    private async Task<string> MigratedSchemaAsync()
    {
        var schema = await _schemas.CreateAsync();

        await using var db = NewContext(schema);
        await db.Database.MigrateAsync();

        return schema;
    }

    private static ApplicationDbContext NewContext(string schema)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(TestDatabase.SchemaConnectionString(schema)).Options);
}
