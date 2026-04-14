using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Audit.Queries.ListAuditEntries;
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
        var result = await handler.Handle(new ListAuditEntriesQuery(), CancellationToken.None);

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
        var result = await handler.Handle(new ListAuditEntriesQuery(Category: AuditCategory.Authentication), CancellationToken.None);

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
        var result = await handler.Handle(new ListAuditEntriesQuery(SuccessOnly: true), CancellationToken.None);

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
        var result = await handler.Handle(new ListAuditEntriesQuery(Page: 2, PageSize: 3), CancellationToken.None);

        result.TotalCount.Should().Be(10);
        result.Items.Should().HaveCount(3);
        result.Page.Should().Be(2);
    }
}
