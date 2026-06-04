using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.DataRights;
using Wombat.Application.Features.DataRights.Queries;
using Wombat.Domain.DataRights;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.DataRights;

public sealed class DownloadAccessReportQueryHandlerTests
{
    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static ClaimsPrincipal UserPrincipal(string userId, params string[] roles)
    {
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId) };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }

    private static async Task<Guid> SeedCompletedExportAsync(ApplicationDbContext db, string ownerUserId)
    {
        var entity = DataRightsRequest.Create(ownerUserId, "Owner", DataRightsRequestType.Export, "export please", DateTime.UtcNow);
        entity.Approve("admin-1", "ok", DateTime.UtcNow);
        entity.Complete(DateTime.UtcNow);
        db.Set<DataRightsRequest>().Add(entity);
        await db.SaveChangesAsync();
        return entity.Id;
    }

    private sealed class StubReportBuilder : IAccessReportBuilder
    {
        public Task<AccessExportResult> BuildAsync(string userId, CancellationToken cancellationToken)
            => Task.FromResult(new AccessExportResult([1, 2, 3, 4], $"data-export-{userId}.zip"));
    }

    [Fact]
    public async Task Download_AsOwnerOfCompletedExport_ReturnsReport()
    {
        await using var db = CreateDb();
        var id = await SeedCompletedExportAsync(db, "user-1");
        var handler = new DownloadAccessReportQueryHandler(db, new StubReportBuilder());

        var result = await handler.Handle(new DownloadAccessReportQuery(id, UserPrincipal("user-1")), CancellationToken.None);

        result.ZipBytes.Should().Equal(1, 2, 3, 4);
        result.FileName.Should().Be("data-export-user-1.zip");
    }

    [Fact]
    public async Task Download_AsAdministrator_ReturnsReport()
    {
        await using var db = CreateDb();
        var id = await SeedCompletedExportAsync(db, "user-1");
        var handler = new DownloadAccessReportQueryHandler(db, new StubReportBuilder());

        var result = await handler.Handle(
            new DownloadAccessReportQuery(id, UserPrincipal("admin-1", WombatRoles.Administrator)),
            CancellationToken.None);

        result.ZipBytes.Should().HaveCount(4);
    }

    [Fact]
    public async Task Download_AsOtherUser_ThrowsUnauthorized()
    {
        await using var db = CreateDb();
        var id = await SeedCompletedExportAsync(db, "user-1");
        var handler = new DownloadAccessReportQueryHandler(db, new StubReportBuilder());

        var act = () => handler.Handle(new DownloadAccessReportQuery(id, UserPrincipal("user-2")), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Download_WhenNotCompleted_Throws()
    {
        await using var db = CreateDb();
        var entity = DataRightsRequest.Create("user-1", "Owner", DataRightsRequestType.Export, "export please", DateTime.UtcNow);
        db.Set<DataRightsRequest>().Add(entity); // still Submitted
        await db.SaveChangesAsync();
        var handler = new DownloadAccessReportQueryHandler(db, new StubReportBuilder());

        var act = () => handler.Handle(new DownloadAccessReportQuery(entity.Id, UserPrincipal("user-1")), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*completed*");
    }

    [Fact]
    public async Task Download_WhenRequestMissing_Throws()
    {
        await using var db = CreateDb();
        var handler = new DownloadAccessReportQueryHandler(db, new StubReportBuilder());

        var act = () => handler.Handle(new DownloadAccessReportQuery(Guid.NewGuid(), UserPrincipal("user-1")), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*not found*");
    }
}
