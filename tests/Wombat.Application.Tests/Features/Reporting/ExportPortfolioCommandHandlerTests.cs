using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Wombat.Application.Features.Reporting;
using Wombat.Domain.Identity;
using Wombat.Domain.Reporting;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Reporting;

public sealed class ExportPortfolioCommandHandlerTests
{
    [Fact]
    public async Task Handle_PersistsExportRecordWithCorrectHash()
    {
        await using var db = CreateDb();

        var expectedHash = "abc123def456";
        var expectedFileName = "portfolio-abc123def456.pdf";
        var pdfBytes = new byte[] { 1, 2, 3 };

        var pdfService = new Mock<IPortfolioPdfService>();
        pdfService
            .Setup(s => s.GenerateAsync(It.IsAny<PortfolioExportRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PortfolioExportResult(pdfBytes, expectedFileName, expectedHash));

        var principal = CreatePrincipal("user-1", WombatRoles.Administrator);
        var handler = new ExportPortfolioCommandHandler(db, pdfService.Object);

        var result = await handler.Handle(
            new ExportPortfolioCommand("trainee-1", null, null, principal),
            CancellationToken.None);

        result.ContentHash.Should().Be(expectedHash);
        result.FileName.Should().Be(expectedFileName);
        result.PdfBytes.Should().BeSameAs(pdfBytes);

        var record = await db.Set<PortfolioExport>().FirstOrDefaultAsync();
        record.Should().NotBeNull();
        record!.TraineeUserId.Should().Be("trainee-1");
        record.ExportedByUserId.Should().Be("user-1");
        record.ContentHash.Should().Be(expectedHash);
        record.FileName.Should().Be(expectedFileName);
    }

    [Fact]
    public async Task Handle_TraineeCanExportOwnPortfolio()
    {
        await using var db = CreateDb();

        var pdfService = new Mock<IPortfolioPdfService>();
        pdfService
            .Setup(s => s.GenerateAsync(It.IsAny<PortfolioExportRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PortfolioExportResult([], "test.pdf", "hash"));

        var principal = CreatePrincipal("trainee-1", WombatRoles.Trainee);
        var handler = new ExportPortfolioCommandHandler(db, pdfService.Object);

        var act = () => handler.Handle(
            new ExportPortfolioCommand("trainee-1", null, null, principal),
            CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Handle_CoordinatorCanExportTraineePortfolio()
    {
        await using var db = CreateDb();

        var pdfService = new Mock<IPortfolioPdfService>();
        pdfService
            .Setup(s => s.GenerateAsync(It.IsAny<PortfolioExportRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PortfolioExportResult([], "test.pdf", "hash"));

        var principal = CreatePrincipal("coord-1", WombatRoles.Coordinator);
        var handler = new ExportPortfolioCommandHandler(db, pdfService.Object);

        var act = () => handler.Handle(
            new ExportPortfolioCommand("trainee-1", null, null, principal),
            CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Handle_UnauthorizedUserCannotExportOtherPortfolio()
    {
        await using var db = CreateDb();

        var pdfService = new Mock<IPortfolioPdfService>();
        var principal = CreatePrincipal("other-user", WombatRoles.Trainee);
        var handler = new ExportPortfolioCommandHandler(db, pdfService.Object);

        var act = () => handler.Handle(
            new ExportPortfolioCommand("trainee-1", null, null, principal),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_PassesDateFiltersToService()
    {
        await using var db = CreateDb();

        PortfolioExportRequest? captured = null;
        var pdfService = new Mock<IPortfolioPdfService>();
        pdfService
            .Setup(s => s.GenerateAsync(It.IsAny<PortfolioExportRequest>(), It.IsAny<CancellationToken>()))
            .Callback<PortfolioExportRequest, CancellationToken>((req, _) => captured = req)
            .ReturnsAsync(new PortfolioExportResult([], "test.pdf", "hash"));

        var from = new DateOnly(2025, 1, 1);
        var to = new DateOnly(2025, 12, 31);
        var principal = CreatePrincipal("admin-1", WombatRoles.Administrator);
        var handler = new ExportPortfolioCommandHandler(db, pdfService.Object);

        await handler.Handle(
            new ExportPortfolioCommand("trainee-1", from, to, principal),
            CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.TraineeUserId.Should().Be("trainee-1");
        captured.FromDate.Should().Be(from);
        captured.ToDate.Should().Be(to);
    }

    private static ApplicationDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static ClaimsPrincipal CreatePrincipal(string userId, string role) =>
        new(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Role, role)
        ], "Test"));
}
