using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Email.Templates;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.DataRights;
using Wombat.Application.Features.DataRights.Commands;
using Wombat.Domain.DataRights;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.DataRights;

/// <summary>
/// Reject (T387, lane D): the refusals Approve makes, in the same order and before anything is staged (F8, S13), the same
/// race handling, and the Rejected notice after the commit, with no note and no decider (Q5, M3).
/// </summary>
public sealed class RejectDataRightsRequestHandlerTests
{
    private const string RequesterId = "user-1";
    private const string Note = "We keep this record by law.";

    private static ApplicationDbContext CreateDb(string? name = null, params Microsoft.EntityFrameworkCore.Diagnostics.IInterceptor[] interceptors)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(name ?? Guid.NewGuid().ToString())
            .AddInterceptors(interceptors)
            .Options);

    private static ClaimsPrincipal Admin(string userId = "admin-1")
        => new(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, userId), new Claim(ClaimTypes.Role, WombatRoles.Administrator)],
            "test"));

    private static ClaimsPrincipal Coordinator(int institutionId)
        => new(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, "coord-1"),
                new Claim(WombatClaimTypes.InstitutionId, institutionId.ToString()),
                new Claim(ClaimTypes.Role, WombatRoles.Coordinator)
            ],
            "test"));

    private static async Task<Guid> SeedAsync(
        ApplicationDbContext db,
        DataRightsRequestStatus status = DataRightsRequestStatus.Submitted,
        int? institutionId = null,
        string requesterId = RequesterId)
    {
        var entity = DataRightsRequest.Create(
            requesterId, "User One", DataRightsRequestType.Rectification, "reason", DateTime.UtcNow, institutionId);
        entity.Status = status;
        db.Set<DataRightsRequest>().Add(entity);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        return entity.Id;
    }

    private static (RejectDataRightsRequestCommandHandler Handler, TestNotices Notices) CreateHandler(ApplicationDbContext db)
    {
        var users = new FakeUserDirectory().With(new UserIdentityDetails(
            RequesterId, "user.one@test.local", "User", "One", 1, [], [], [WombatRoles.Trainee]));
        var notices = new TestNotices(users);
        return (new RejectDataRightsRequestCommandHandler(db, notices.Notices), notices);
    }

    private static Task<DataRightsRequestDto> Reject(
        RejectDataRightsRequestCommandHandler handler, Guid id, ClaimsPrincipal? caller = null)
        => handler.Handle(new RejectDataRightsRequestCommand(id, Note, caller ?? Admin()), CancellationToken.None);

    private static async Task ShouldHaveStagedNothingAsync(ApplicationDbContext db, Guid id, DataRightsRequestStatus expected)
    {
        db.ChangeTracker.Entries().Should().OnlyContain(entry => entry.State == EntityState.Unchanged);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        (await db.Set<DataRightsRequest>().AsNoTracking().SingleAsync(r => r.Id == id)).Status.Should().Be(expected);
    }

    [Fact]
    public async Task Reject_Rejects_AndMailsTheRejectedNotice_WithNoNoteAndNoDecider()
    {
        await using var db = CreateDb();
        var id = await SeedAsync(db);
        var (handler, notices) = CreateHandler(db);

        var result = await Reject(handler, id);

        result.Status.Should().Be(DataRightsRequestStatus.Rejected);
        var mail = notices.Sender.Sent.Should().ContainSingle().Which;
        mail.To.Should().Be("user.one@test.local");
        mail.Subject.Should().Be(DataRightsDecisionEmail.DecidedSubject);
        mail.TextBody.Should().Contain("Your Rectification request was rejected on")
            .And.Contain("https://wombat.test/account/data-rights")
            .And.NotContain(Note, "the note is read on My data rights, never mailed (Q5)")
            .And.NotContain("admin-1");
    }

    [Fact]
    public async Task Reject_AWithdrawnRequest_IsRefusedWithdrawn_AndStagesNothing()
    {
        await using var db = CreateDb();
        var id = await SeedAsync(db, DataRightsRequestStatus.Withdrawn);
        var (handler, notices) = CreateHandler(db);

        var refusal = (await FluentActions.Invoking(() => Reject(handler, id))
            .Should().ThrowAsync<DataRightsRefusedException>()).Which;

        refusal.Reason.Should().Be(DataRightsRefusal.Withdrawn);
        refusal.Message.Should().Be(DataRightsDecisionRefusals.WithdrawnMessage);
        notices.Sender.Sent.Should().BeEmpty();
        await ShouldHaveStagedNothingAsync(db, id, DataRightsRequestStatus.Withdrawn);
    }

    [Theory]
    [InlineData(DataRightsRequestStatus.Approved)]
    [InlineData(DataRightsRequestStatus.Rejected)]
    [InlineData(DataRightsRequestStatus.Completed)]
    public async Task Reject_ARequestDecidedAlready_IsRefusedDecidedMeanwhile(DataRightsRequestStatus status)
    {
        await using var db = CreateDb();
        var id = await SeedAsync(db, status);
        var (handler, notices) = CreateHandler(db);

        var refusal = (await FluentActions.Invoking(() => Reject(handler, id))
            .Should().ThrowAsync<DataRightsRefusedException>()).Which;

        refusal.Reason.Should().Be(DataRightsRefusal.DecidedMeanwhile);
        notices.Sender.Sent.Should().BeEmpty();
        await ShouldHaveStagedNothingAsync(db, id, status);
    }

    [Fact]
    public async Task Reject_OutOfScope_IsRefusedForScope_BeforeItsState()
    {
        await using var db = CreateDb();
        var id = await SeedAsync(db, DataRightsRequestStatus.Withdrawn, institutionId: 1);
        var (handler, _) = CreateHandler(db);

        await FluentActions.Invoking(() => Reject(handler, id, Coordinator(institutionId: 2)))
            .Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Reject_TheRequestersOwn_IsRefused()
    {
        await using var db = CreateDb();
        var id = await SeedAsync(db, requesterId: "admin-1");
        var (handler, notices) = CreateHandler(db);

        await FluentActions.Invoking(() => Reject(handler, id, Admin("admin-1")))
            .Should().ThrowAsync<UnauthorizedAccessException>();
        notices.Sender.Sent.Should().BeEmpty();
        await ShouldHaveStagedNothingAsync(db, id, DataRightsRequestStatus.Submitted);
    }

    [Fact]
    public async Task Reject_LosingTheRaceToAWithdrawal_IsRefusedWithdrawn_AndMailsNobody()
    {
        var name = Guid.NewGuid().ToString();
        await using (var seed = CreateDb(name))
        {
            await SeedAsync(seed);
        }

        var interceptor = new ApproveDataRightsRequestHandlerTests.ConcurrentWrite(name, request => request.Withdraw());
        await using var db = CreateDb(name, interceptor);
        var id = await db.Set<DataRightsRequest>().Select(r => r.Id).SingleAsync();
        var (handler, notices) = CreateHandler(db);

        var refusal = (await FluentActions.Invoking(() => Reject(handler, id))
            .Should().ThrowAsync<DataRightsRefusedException>()).Which;

        refusal.Reason.Should().Be(DataRightsRefusal.Withdrawn);
        refusal.InnerException.Should().BeOfType<DbUpdateConcurrencyException>();
        notices.Sender.Sent.Should().BeEmpty();
        await ShouldHaveStagedNothingAsync(db, id, DataRightsRequestStatus.Withdrawn);
    }
}
