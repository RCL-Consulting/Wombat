using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Reporting;
using Wombat.Domain.Reporting;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Reporting;

public sealed class VerifyExportQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsRecord_WhenHashMatches()
    {
        await using var db = CreateDb();

        db.Set<PortfolioExport>().Add(new PortfolioExport
        {
            TraineeUserId = "trainee-1",
            ExportedByUserId = "admin-1",
            ExportedOn = new DateTime(2025, 6, 15, 10, 0, 0, DateTimeKind.Utc),
            ContentHash = "abc123",
            FileName = "portfolio-abc123.pdf"
        });
        await db.SaveChangesAsync();

        var handler = new VerifyExportQueryHandler(db);

        var result = await handler.Handle(new VerifyExportQuery("abc123"), CancellationToken.None);

        result.Should().NotBeNull();
        result!.TraineeUserId.Should().Be("trainee-1");
        result.ContentHash.Should().Be("abc123");
        result.FileName.Should().Be("portfolio-abc123.pdf");
    }

    [Fact]
    public async Task Handle_ReturnsNull_WhenHashNotFound()
    {
        await using var db = CreateDb();

        var handler = new VerifyExportQueryHandler(db);

        var result = await handler.Handle(new VerifyExportQuery("nonexistent"), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ReturnsNull_WhenHashIsEmpty()
    {
        await using var db = CreateDb();

        var handler = new VerifyExportQueryHandler(db);

        var result = await handler.Handle(new VerifyExportQuery(""), CancellationToken.None);

        result.Should().BeNull();
    }

    private static ApplicationDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
}
