using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Wombat.Application.Common.Options;
using Wombat.Application.Features.DataRights;
using Wombat.Application.Features.DataRights.Commands;
using Wombat.Domain.DataRights;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.DataRights;

public sealed class ApproveDataRightsRequestHandlerTests
{
    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static ClaimsPrincipal Admin()
        => new(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, "admin-1"), new Claim(ClaimTypes.Role, WombatRoles.Administrator)],
            "test"));

    private static async Task<Guid> SeedSubmittedAsync(ApplicationDbContext db, DataRightsRequestType type)
    {
        var entity = DataRightsRequest.Create("user-1", "User One", type, "reason", DateTime.UtcNow);
        db.Set<DataRightsRequest>().Add(entity);
        await db.SaveChangesAsync();
        return entity.Id;
    }

    private sealed class RecordingErasureExecutor : IErasureExecutor
    {
        public int Calls { get; private set; }

        public Task<DataRightsErasureRecord> ExecuteAsync(DataRightsRequest request, string pseudonymSalt, CancellationToken cancellationToken)
        {
            Calls++;
            return Task.FromResult(DataRightsErasureRecord.Create(request.Id, request.RequesterUserId, "deleted_user_x", DateTime.UtcNow, "[]"));
        }
    }

    private sealed class StubReportBuilder : IAccessReportBuilder
    {
        public Task<AccessExportResult> BuildAsync(string userId, CancellationToken cancellationToken)
            => Task.FromResult(new AccessExportResult([], "x.zip"));
    }

    private static ApproveDataRightsRequestCommandHandler CreateHandler(
        ApplicationDbContext db, IErasureExecutor executor, string? salt)
        => new(db, executor, new StubReportBuilder(), Options.Create(new WombatOptions { PseudonymSalt = salt }));

    [Fact]
    public async Task Approve_Erasure_WithoutSalt_StaysSubmitted_AndDoesNotExecute()
    {
        await using var db = CreateDb();
        var id = await SeedSubmittedAsync(db, DataRightsRequestType.Erasure);
        var executor = new RecordingErasureExecutor();
        var handler = CreateHandler(db, executor, salt: null);

        var act = () => handler.Handle(new ApproveDataRightsRequestCommand(id, "approved", Admin()), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*PseudonymSalt*");
        executor.Calls.Should().Be(0);
        var stored = await db.Set<DataRightsRequest>().AsNoTracking().FirstAsync(r => r.Id == id);
        stored.Status.Should().Be(DataRightsRequestStatus.Submitted, "a missing-salt failure must leave the request actionable");
    }

    [Fact]
    public async Task Approve_Erasure_WithSalt_Executes_AndCompletes()
    {
        await using var db = CreateDb();
        var id = await SeedSubmittedAsync(db, DataRightsRequestType.Erasure);
        var executor = new RecordingErasureExecutor();
        var handler = CreateHandler(db, executor, salt: "pepper");

        var result = await handler.Handle(new ApproveDataRightsRequestCommand(id, "approved", Admin()), CancellationToken.None);

        executor.Calls.Should().Be(1);
        result.Status.Should().Be(DataRightsRequestStatus.Completed);
    }

    [Fact]
    public async Task Approve_Export_Completes_WithoutErasure()
    {
        await using var db = CreateDb();
        var id = await SeedSubmittedAsync(db, DataRightsRequestType.Export);
        var executor = new RecordingErasureExecutor();
        var handler = CreateHandler(db, executor, salt: null);

        var result = await handler.Handle(new ApproveDataRightsRequestCommand(id, "approved", Admin()), CancellationToken.None);

        result.Status.Should().Be(DataRightsRequestStatus.Completed);
        executor.Calls.Should().Be(0);
    }
}
