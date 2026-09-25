using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Audit.Queries.ListAuditEntries;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Audit;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Audit;

public sealed class ListAuditEntriesQueryHandlerTests
{
    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task Handle_DefaultQuery_ReturnsLast24Hours()
    {
        await using var db = CreateDb();

        var old = AuditEntry.Create(DateTime.UtcNow.AddHours(-48), AuditCategory.Command, "OldCommand", true);
        var recent = AuditEntry.Create(DateTime.UtcNow.AddMinutes(-30), AuditCategory.Command, "RecentCommand", true);

        db.Set<AuditEntry>().AddRange(old, recent);
        await db.SaveChangesAsync();

        var handler = new ListAuditEntriesQueryHandler(db);
        var result = await handler.Handle(new ListAuditEntriesQuery(TestPrincipals.Administrator()), CancellationToken.None);

        result.Items.Should().ContainSingle();
        result.Items[0].Action.Should().Be("RecentCommand");
    }

    [Fact]
    public async Task Handle_FilterByCategory_ReturnsCategoryOnly()
    {
        await using var db = CreateDb();

        var cmd = AuditEntry.Create(DateTime.UtcNow.AddMinutes(-1), AuditCategory.Command, "SomeCommand", true);
        var auth = AuditEntry.Create(DateTime.UtcNow.AddMinutes(-1), AuditCategory.Authentication, "Login", true);

        db.Set<AuditEntry>().AddRange(cmd, auth);
        await db.SaveChangesAsync();

        var handler = new ListAuditEntriesQueryHandler(db);
        var result = await handler.Handle(new ListAuditEntriesQuery(TestPrincipals.Administrator(), Category: AuditCategory.Authentication), CancellationToken.None);

        result.Items.Should().ContainSingle();
        result.Items[0].Action.Should().Be("Login");
    }

    [Fact]
    public async Task Handle_FilterBySuccessOnly_ExcludesFailures()
    {
        await using var db = CreateDb();

        var success = AuditEntry.Create(DateTime.UtcNow.AddMinutes(-1), AuditCategory.Command, "GoodCommand", true);
        var failure = AuditEntry.Create(DateTime.UtcNow.AddMinutes(-1), AuditCategory.Command, "BadCommand", false);

        db.Set<AuditEntry>().AddRange(success, failure);
        await db.SaveChangesAsync();

        var handler = new ListAuditEntriesQueryHandler(db);
        var result = await handler.Handle(new ListAuditEntriesQuery(TestPrincipals.Administrator(), SuccessOnly: true), CancellationToken.None);

        result.Items.Should().ContainSingle();
        result.Items[0].Action.Should().Be("GoodCommand");
    }

    [Fact]
    public async Task Handle_Pagination_ReturnsCorrectPage()
    {
        await using var db = CreateDb();

        var entries = Enumerable.Range(1, 10).Select(i =>
            AuditEntry.Create(DateTime.UtcNow.AddMinutes(-i), AuditCategory.Command, $"Cmd{i}", true))
            .ToList();

        db.Set<AuditEntry>().AddRange(entries);
        await db.SaveChangesAsync();

        var handler = new ListAuditEntriesQueryHandler(db);
        var result = await handler.Handle(new ListAuditEntriesQuery(TestPrincipals.Administrator(), Page: 2, PageSize: 3), CancellationToken.None);

        result.TotalCount.Should().Be(10);
        result.Items.Should().HaveCount(3);
        result.Page.Should().Be(2);
    }

    /// <summary>
    /// T262: eight entries on one timestamp, straddling two page boundaries, between a newer and an older one. Every
    /// entry is on exactly one page, and a tie is settled by id, highest first.
    /// </summary>
    /// <remarks>
    /// Ordered by the timestamp alone, a tie comes back in whatever order the server's sort leaves it, and PostgreSQL's can
    /// differ from one page's query to the next, so a page could repeat an entry and another page miss one. EF InMemory's
    /// sort is stable, so here the untie-broken query returns the tie in the order the entries were added: lowest id first,
    /// which is not the expected pages. On PostgreSQL, where the repeat and the skip are real, it is
    /// <c>AuditEntryOrderPostgresTests</c>.
    /// </remarks>
    [Fact]
    public async Task Handle_EntriesOnOneTimestamp_AcrossPageBoundaries_AreEachOnOnePage_TiesSettledById()
    {
        await using var db = CreateDb();

        var tie = new DateTime(2026, 9, 20, 10, 0, 0, DateTimeKind.Utc);

        // The newest entry has the lowest id and the oldest the highest, so the timestamp, not the id, puts them first
        // and last. The tie is added lowest id first.
        db.Set<AuditEntry>().Add(Entry(1, tie.AddSeconds(1)));
        db.Set<AuditEntry>().AddRange(Enumerable.Range(2, 8).Select(n => Entry(n, tie)));
        db.Set<AuditEntry>().Add(Entry(10, tie.AddSeconds(-1)));
        await db.SaveChangesAsync();

        var handler = new ListAuditEntriesQueryHandler(db);
        var pages = new List<IReadOnlyList<int>>();
        for (var page = 1; page <= 4; page++)
        {
            var result = await handler.Handle(
                new ListAuditEntriesQuery(
                    TestPrincipals.Administrator(), From: tie.AddHours(-1), To: tie.AddHours(1), Page: page, PageSize: 3),
                CancellationToken.None);

            result.TotalCount.Should().Be(10);
            pages.Add(result.Items.Select(item => Number(item.Id)).ToList());
        }

        pages.Should().BeEquivalentTo(
            new[] { new[] { 1, 9, 8 }, new[] { 7, 6, 5 }, new[] { 4, 3, 2 }, new[] { 10 } },
            options => options.WithStrictOrdering());
        pages.SelectMany(page => page).Should().OnlyHaveUniqueItems().And.BeEquivalentTo(Enumerable.Range(1, 10));
    }

    /// <summary>An entry whose id is <paramref name="number" /> in its last group, so the ids sort as the numbers do.</summary>
    private static AuditEntry Entry(int number, DateTime occurredAt)
    {
        var entry = AuditEntry.Create(occurredAt, AuditCategory.Command, $"Cmd{number}", true);
        entry.Id = new Guid($"00000000-0000-0000-0000-{number:D12}");
        return entry;
    }

    private static int Number(Guid id) => int.Parse(id.ToString()[^12..]);
}
