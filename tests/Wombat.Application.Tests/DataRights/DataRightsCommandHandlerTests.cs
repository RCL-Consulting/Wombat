using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.DataRights.Commands;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.DataRights;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.DataRights;

public sealed class DataRightsCommandHandlerTests
{
    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static ClaimsPrincipal UserPrincipal(string userId = "user-1", string name = "Test User", params string[] roles)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Name, name)
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }

    [Fact]
    public async Task Submit_AccessRequest_CreatesSubmittedRequest()
    {
        await using var db = CreateDb();
        var handler = new SubmitDataRightsRequestCommandHandler(db);
        var principal = UserPrincipal();

        var result = await handler.Handle(
            new SubmitDataRightsRequestCommand(DataRightsRequestType.Access, "I want my data.", principal),
            CancellationToken.None);

        result.Type.Should().Be(DataRightsRequestType.Access);
        result.Status.Should().Be(DataRightsRequestStatus.Submitted);
        result.RequesterUserId.Should().Be("user-1");
        result.Reason.Should().Be("I want my data.");
    }

    [Fact]
    public async Task Submit_ErasureRequest_BlockedByActiveCommitteeReview()
    {
        await using var db = CreateDb();

        // Create an active committee review for this user
        db.Set<DecisionPanel>().Add(new DecisionPanel
        {
            Name = "Test Panel",
            Scope = DecisionPanelScope.Institution,
            InstitutionId = 1,
            CreatedOn = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        db.Set<CommitteeReview>().Add(new CommitteeReview
        {
            TraineeUserId = "user-1",
            PanelId = 1,
            ReviewPeriodFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-6)),
            ReviewPeriodTo = DateOnly.FromDateTime(DateTime.UtcNow),
            ScheduledOn = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7))
        });
        await db.SaveChangesAsync();

        var handler = new SubmitDataRightsRequestCommandHandler(db);
        var principal = UserPrincipal();

        var act = () => handler.Handle(
            new SubmitDataRightsRequestCommand(DataRightsRequestType.Erasure, "Please erase my data.", principal),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*active committee review*");
    }

    [Fact]
    public async Task Withdraw_OwnRequest_Succeeds()
    {
        await using var db = CreateDb();
        var principal = UserPrincipal();

        // Submit a request first
        var submitHandler = new SubmitDataRightsRequestCommandHandler(db);
        var submitted = await submitHandler.Handle(
            new SubmitDataRightsRequestCommand(DataRightsRequestType.Access, "I want my data.", principal),
            CancellationToken.None);

        // Withdraw it
        var withdrawHandler = new WithdrawDataRightsRequestCommandHandler(db);
        await withdrawHandler.Handle(
            new WithdrawDataRightsRequestCommand(submitted.Id, principal),
            CancellationToken.None);

        var entity = await db.Set<DataRightsRequest>().FindAsync(submitted.Id);
        entity!.Status.Should().Be(DataRightsRequestStatus.Withdrawn);
    }

    [Fact]
    public async Task Withdraw_OtherUsersRequest_ThrowsUnauthorized()
    {
        await using var db = CreateDb();
        var ownerPrincipal = UserPrincipal("user-1");
        var otherPrincipal = UserPrincipal("user-2");

        var submitHandler = new SubmitDataRightsRequestCommandHandler(db);
        var submitted = await submitHandler.Handle(
            new SubmitDataRightsRequestCommand(DataRightsRequestType.Access, "My data.", ownerPrincipal),
            CancellationToken.None);

        var withdrawHandler = new WithdrawDataRightsRequestCommandHandler(db);
        var act = () => withdrawHandler.Handle(
            new WithdrawDataRightsRequestCommand(submitted.Id, otherPrincipal),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Reject_AsAdministrator_SetsRejectedStatus()
    {
        await using var db = CreateDb();
        var userPrincipal = UserPrincipal("user-1");
        var adminPrincipal = UserPrincipal("admin-1", "Admin User", "Administrator");

        var submitHandler = new SubmitDataRightsRequestCommandHandler(db);
        var submitted = await submitHandler.Handle(
            new SubmitDataRightsRequestCommand(DataRightsRequestType.Access, "My data.", userPrincipal),
            CancellationToken.None);

        var rejectHandler = new RejectDataRightsRequestCommandHandler(db);
        var result = await rejectHandler.Handle(
            new RejectDataRightsRequestCommand(submitted.Id, "Insufficient justification.", adminPrincipal),
            CancellationToken.None);

        result.Status.Should().Be(DataRightsRequestStatus.Rejected);
        result.DecisionNote.Should().Be("Insufficient justification.");
        result.DecidedByUserId.Should().Be("admin-1");
    }

    [Fact]
    public async Task Reject_AsNonAdmin_ThrowsUnauthorized()
    {
        await using var db = CreateDb();
        var userPrincipal = UserPrincipal("user-1");

        var submitHandler = new SubmitDataRightsRequestCommandHandler(db);
        var submitted = await submitHandler.Handle(
            new SubmitDataRightsRequestCommand(DataRightsRequestType.Access, "My data.", userPrincipal),
            CancellationToken.None);

        var rejectHandler = new RejectDataRightsRequestCommandHandler(db);
        var act = () => rejectHandler.Handle(
            new RejectDataRightsRequestCommand(submitted.Id, "No.", UserPrincipal("user-2", "Regular User")),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
