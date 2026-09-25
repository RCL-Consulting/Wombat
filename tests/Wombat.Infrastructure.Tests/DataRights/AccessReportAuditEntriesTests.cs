using System.IO.Compression;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Reporting;
using Wombat.Domain.Audit;
using Wombat.Infrastructure.DataRights;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Infrastructure.Tests.DataRights;

/// <summary>
/// T262: the access report lists the newest 500 audit entries its subject made, and when the cut at 500 falls inside a
/// tie, the entries kept are the tie's highest ids, the same on every run.
/// </summary>
/// <remarks>
/// Ordered by the timestamp alone, which entries of a tie made the cut was the server's sort's to choose. EF InMemory's
/// sort is stable, so here the untie-broken query keeps the tie in the order the entries were added, lowest id first,
/// and leaves out the highest. The same on PostgreSQL is <c>AuditEntryOrderPostgresTests</c>.
/// </remarks>
public sealed class AccessReportAuditEntriesTests
{
    private const string Subject = "t262-subject";

    [Fact]
    public async Task TheCutAt500_InsideATie_KeepsTheTiesHighestIds_NewestFirst()
    {
        await using var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

        var tie = new DateTime(2026, 9, 20, 10, 0, 0, DateTimeKind.Utc);

        // Three newer entries with the lowest ids, then 500 on one timestamp, added lowest id first. 503 in all, so the
        // cut leaves out three of the tie.
        db.AuditEntries.Add(Entry(1, tie.AddSeconds(3)));
        db.AuditEntries.Add(Entry(2, tie.AddSeconds(2)));
        db.AuditEntries.Add(Entry(3, tie.AddSeconds(1)));
        db.AuditEntries.AddRange(Enumerable.Range(4, 500).Select(n => Entry(n, tie)));

        // Someone else's, newer than everything: not in the subject's report.
        var other = AuditEntry.Create(tie.AddMinutes(1), AuditCategory.Command, "Other", true, actorUserId: "someone-else");
        other.Id = new Guid("00000000-0000-0000-0000-999999999999");
        db.AuditEntries.Add(other);
        await db.SaveChangesAsync();

        var export = await new AccessReportBuilder(db, new NoPortfolio()).BuildAsync(Subject, CancellationToken.None);

        using var zip = new ZipArchive(new MemoryStream(export.ZipBytes), ZipArchiveMode.Read);
        await using var json = zip.GetEntry("data-export.json")!.Open();
        using var document = await JsonDocument.ParseAsync(json);
        var numbers = document.RootElement.GetProperty("auditEntries").EnumerateArray()
            .Select(entry => int.Parse(entry.GetProperty("id").GetString()![^12..]))
            .ToList();

        numbers.Should().Equal(
            new[] { 1, 2, 3 }.Concat(Enumerable.Range(7, 497).Reverse()),
            "the three newer entries come first, then the tie from its highest id down, and 4, 5 and 6 are cut");
    }

    /// <summary>An entry by the subject whose id is <paramref name="number" /> in its last group, so the ids sort as the numbers do.</summary>
    private static AuditEntry Entry(int number, DateTime occurredAt)
    {
        var entry = AuditEntry.Create(occurredAt, AuditCategory.Command, $"Cmd{number}", true, actorUserId: Subject);
        entry.Id = new Guid($"00000000-0000-0000-0000-{number:D12}");
        return entry;
    }

    /// <summary>The PDF is best-effort in the report, and not what this test reads.</summary>
    private sealed class NoPortfolio : IPortfolioPdfService
    {
        public Task<PortfolioExportResult> GenerateAsync(PortfolioExportRequest request, CancellationToken cancellationToken)
            => throw new NotSupportedException("The PDF is not part of this test.");
    }
}
