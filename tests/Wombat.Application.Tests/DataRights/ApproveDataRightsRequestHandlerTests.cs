using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Wombat.Application.Common.Email.Templates;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Options;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.DataRights;
using Wombat.Application.Features.DataRights.Commands;
using Wombat.Domain.DataRights;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.DataRights;

/// <summary>
/// Approve (T387, lane D): the refusals in their order, each before anything is staged (T201); a save that lost a race to
/// a withdrawal or another decision, refused in the request's words once the row is read again (F8, S13); the erasure's
/// two refusals (N1, F9); and the notice, sent after the commit and never on a refusal, the erasure's to the address read
/// before it ran (F4, M3).
/// </summary>
public sealed class ApproveDataRightsRequestHandlerTests
{
    private const string RequesterId = "user-1";
    private const string RequesterEmail = "user.one@test.local";
    private const string Note = "Identity confirmed by telephone.";

    private static ApplicationDbContext CreateDb(string? name = null, params IInterceptor[] interceptors)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(name ?? Guid.NewGuid().ToString())
            .AddInterceptors(interceptors)
            .Options;
        return new ApplicationDbContext(options);
    }

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
        DataRightsRequestType type,
        int? institutionId = null,
        DataRightsRequestStatus status = DataRightsRequestStatus.Submitted,
        string requesterId = RequesterId)
    {
        var entity = DataRightsRequest.Create(requesterId, "User One", type, "reason", DateTime.UtcNow, institutionId);
        entity.Status = status;
        db.Set<DataRightsRequest>().Add(entity);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        return entity.Id;
    }

    private static UserIdentityDetails Account(string userId, string email, bool deactivated = false, params string[] roles)
        => new(userId, email, "User", "One", 1, [], [], roles, IsDeactivated: deactivated);

    /// <summary>The requester, an ordinary trainee, and the approving Administrator.</summary>
    private static FakeUserDirectory Directory()
        => new FakeUserDirectory()
            .With(Account(RequesterId, RequesterEmail, roles: WombatRoles.Trainee))
            .With(Account("admin-1", "admin@test.local", roles: WombatRoles.Administrator));

    private sealed class RecordingErasureExecutor : IErasureExecutor
    {
        public int Calls { get; private set; }

        /// <summary>Runs inside ExecuteAsync, before it answers: what happens elsewhere while the person is erased.</summary>
        public Func<Task>? During { get; init; }

        public Exception? Throw { get; init; }

        public async Task<ErasureOutcome> ExecuteAsync(DataRightsRequest request, string pseudonymSalt, CancellationToken cancellationToken)
        {
            Calls++;
            if (During is not null)
            {
                await During();
            }

            if (Throw is not null)
            {
                throw Throw;
            }

            return new ErasureOutcome("deleted_user_x", 1, 0, 0, 1);
        }
    }

    private sealed class Harness
    {
        public required ApproveDataRightsRequestCommandHandler Handler { get; init; }
        public required TestNotices Notices { get; init; }
        public required RecordingErasureExecutor Executor { get; init; }
        public required ListLogger Log { get; init; }
    }

    private static Harness CreateHandler(
        ApplicationDbContext db,
        string? salt = "pepper",
        IUserAdministrationService? users = null,
        RecordingErasureExecutor? executor = null)
    {
        users ??= Directory();
        executor ??= new RecordingErasureExecutor();
        var notices = new TestNotices(users);
        var log = new ListLogger();
        return new Harness
        {
            Handler = new ApproveDataRightsRequestCommandHandler(
                db, executor, users, notices.Notices, Options.Create(new WombatOptions { PseudonymSalt = salt }), log),
            Notices = notices,
            Executor = executor,
            Log = log
        };
    }

    private static Task<ApproveDataRightsResult> Approve(Harness harness, Guid id, ClaimsPrincipal? caller = null)
        => harness.Handler.Handle(new ApproveDataRightsRequestCommand(id, Note, caller ?? Admin()), CancellationToken.None);

    /// <summary>What the audit pipeline would commit from its catch: nothing, since nothing was staged (T201).</summary>
    private static async Task ShouldHaveStagedNothingAsync(ApplicationDbContext db, Guid id, DataRightsRequestStatus expected)
    {
        db.ChangeTracker.Entries().Should().OnlyContain(entry => entry.State == EntityState.Unchanged || entry.State == EntityState.Detached);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        (await db.Set<DataRightsRequest>().AsNoTracking().SingleAsync(r => r.Id == id)).Status.Should().Be(expected);
    }

    [Fact]
    public async Task Approve_Erasure_WithoutSalt_IsRefusedTyped_LogsTheSetting_AndStaysSubmitted()
    {
        await using var db = CreateDb();
        var id = await SeedAsync(db, DataRightsRequestType.Erasure);
        var harness = CreateHandler(db, salt: null);

        var refusal = (await FluentActions.Invoking(() => Approve(harness, id))
            .Should().ThrowAsync<DataRightsRefusedException>()).Which;

        refusal.Reason.Should().Be(DataRightsRefusal.ErasureNotConfigured);
        refusal.Message.Should().NotContain("PseudonymSalt", "the setting is the operator's, named in the log only (N1)");
        harness.Log.Entries.Should().ContainSingle(entry => entry.Contains("Wombat__PseudonymSalt"));
        harness.Executor.Calls.Should().Be(0);
        harness.Notices.Sender.Sent.Should().BeEmpty();
        await ShouldHaveStagedNothingAsync(db, id, DataRightsRequestStatus.Submitted);
    }

    [Fact]
    public async Task Approve_Erasure_WithSalt_Executes_Completes_AndMailsTheAddressReadBefore()
    {
        await using var db = CreateDb();
        var id = await SeedAsync(db, DataRightsRequestType.Erasure);
        var users = new ErasingDirectory(Account(RequesterId, RequesterEmail, roles: WombatRoles.Trainee));
        var executor = new RecordingErasureExecutor { During = () => { users.Erase(); return Task.CompletedTask; } };
        var harness = CreateHandler(db, users: users, executor: executor);

        var result = await Approve(harness, id);

        executor.Calls.Should().Be(1);
        result.Request.Status.Should().Be(DataRightsRequestStatus.Completed);
        result.Erasure.Should().Be(new ErasureOutcome("deleted_user_x", 1, 0, 0, 1),
            "the result carries what the erasure ended, for its sentence (T387, S6)");

        var mail = harness.Notices.Sender.Sent.Should().ContainSingle(
            "the erased account has no address, so the confirmation goes to the one read before (F4)").Which;
        mail.To.Should().Be(RequesterEmail);
        mail.Subject.Should().Be(DataRightsDecisionEmail.ErasedSubject);
        mail.TextBody.Should().NotContain("https://", "the account is gone: the erasure's notice carries no link");
        mail.TextBody.Should().NotContain(Note, "the note is never mailed (Q5)").And.Contain("erased from Wombat");
    }

    [Fact]
    public async Task Approve_Export_Completes_AndMailsTheApprovedNotice_AfterTheCommit()
    {
        await using var db = CreateDb();
        var id = await SeedAsync(db, DataRightsRequestType.Export);
        var harness = CreateHandler(db, salt: null);

        var result = await Approve(harness, id);

        result.Request.Status.Should().Be(DataRightsRequestStatus.Completed);
        result.Erasure.Should().BeNull();
        harness.Executor.Calls.Should().Be(0);

        var mail = harness.Notices.Sender.Sent.Should().ContainSingle().Which;
        mail.To.Should().Be(RequesterEmail);
        mail.Subject.Should().Be(DataRightsDecisionEmail.DecidedSubject);
        mail.TextBody.Should().Contain("Your Export request was approved on")
            .And.Contain("https://wombat.test/account/data-rights")
            .And.NotContain("admin", "the notice names no decider (Q5)")
            .And.NotContain(Note);
        (await db.Set<DataRightsRequest>().AsNoTracking().SingleAsync(r => r.Id == id)).Status
            .Should().Be(DataRightsRequestStatus.Completed, "the mail is sent once the approval is saved");
    }

    [Fact]
    public async Task Approve_Rectification_StaysApproved_AndSaysItIsBeingCorrected()
    {
        await using var db = CreateDb();
        var id = await SeedAsync(db, DataRightsRequestType.Rectification);
        var harness = CreateHandler(db);

        var result = await Approve(harness, id);

        result.Request.Status.Should().Be(DataRightsRequestStatus.Approved);
        harness.Notices.Sender.Sent.Should().ContainSingle().Which.TextBody.Should().Contain("It is being corrected.");
    }

    [Fact]
    public async Task Approve_AWithdrawnRequest_IsRefusedWithdrawn_BeforeTheSalt_AndStagesNothing()
    {
        await using var db = CreateDb();
        var id = await SeedAsync(db, DataRightsRequestType.Erasure, status: DataRightsRequestStatus.Withdrawn);
        var harness = CreateHandler(db, salt: null);

        var refusal = (await FluentActions.Invoking(() => Approve(harness, id))
            .Should().ThrowAsync<DataRightsRefusedException>()).Which;

        refusal.Reason.Should().Be(DataRightsRefusal.Withdrawn, "Withdrawn is checked before anything the erasure needs (F8)");
        harness.Executor.Calls.Should().Be(0);
        harness.Notices.Sender.Sent.Should().BeEmpty();
        await ShouldHaveStagedNothingAsync(db, id, DataRightsRequestStatus.Withdrawn);
    }

    [Theory]
    [InlineData(DataRightsRequestStatus.Approved)]
    [InlineData(DataRightsRequestStatus.Rejected)]
    [InlineData(DataRightsRequestStatus.Completed)]
    public async Task Approve_ARequestDecidedAlready_IsRefusedDecidedMeanwhile(DataRightsRequestStatus status)
    {
        await using var db = CreateDb();
        var id = await SeedAsync(db, DataRightsRequestType.Rectification, status: status);
        var harness = CreateHandler(db);

        var refusal = (await FluentActions.Invoking(() => Approve(harness, id))
            .Should().ThrowAsync<DataRightsRefusedException>()).Which;

        refusal.Reason.Should().Be(DataRightsRefusal.DecidedMeanwhile);
        refusal.Message.Should().Be(DataRightsDecisionRefusals.DecidedMeanwhileMessage);
        harness.Notices.Sender.Sent.Should().BeEmpty();
        await ShouldHaveStagedNothingAsync(db, id, status);
    }

    /// <summary>
    /// The scope rule comes first (T387, the lane's deviation from the plan's order, CLAUDE.md T056): a caller out of
    /// scope is refused as for any request, and learns nothing of whether it was withdrawn.
    /// </summary>
    [Fact]
    public async Task Approve_AWithdrawnRequest_OutOfScope_IsRefusedForScope_NotWithdrawn()
    {
        await using var db = CreateDb();
        var id = await SeedAsync(db, DataRightsRequestType.Erasure, institutionId: 1, status: DataRightsRequestStatus.Withdrawn);
        var harness = CreateHandler(db);

        await FluentActions.Invoking(() => Approve(harness, id, Coordinator(institutionId: 2)))
            .Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Approve_TheRequestersOwn_IsRefused_TheAdministratorIncluded()
    {
        await using var db = CreateDb();
        var id = await SeedAsync(db, DataRightsRequestType.Export, requesterId: "admin-1");
        var harness = CreateHandler(db);

        await FluentActions.Invoking(() => Approve(harness, id, Admin("admin-1")))
            .Should().ThrowAsync<UnauthorizedAccessException>("nobody decides their own request (Q21, F9)");
        harness.Notices.Sender.Sent.Should().BeEmpty();
        await ShouldHaveStagedNothingAsync(db, id, DataRightsRequestStatus.Submitted);
    }

    [Fact]
    public async Task Approve_TheErasureOfTheOnlyActiveAdministrator_IsRefused_AndErasesNothing()
    {
        await using var db = CreateDb();
        var id = await SeedAsync(db, DataRightsRequestType.Erasure, institutionId: 1, requesterId: "admin-only");
        var users = new FakeUserDirectory()
            .With(Account("admin-only", "only@test.local", roles: WombatRoles.Administrator))
            .With(Account("admin-locked", "locked@test.local", deactivated: true, roles: WombatRoles.Administrator));
        var harness = CreateHandler(db, users: users);

        var refusal = (await FluentActions.Invoking(() => Approve(harness, id, Coordinator(institutionId: 1)))
            .Should().ThrowAsync<DataRightsRefusedException>()).Which;

        refusal.Reason.Should().Be(DataRightsRefusal.LastAdministrator, "a deactivated Administrator runs nothing");
        refusal.Message.Should().NotContain("only@test.local").And.NotContain("admin-only");
        harness.Executor.Calls.Should().Be(0);
        harness.Notices.Sender.Sent.Should().BeEmpty();
        await ShouldHaveStagedNothingAsync(db, id, DataRightsRequestStatus.Submitted);
    }

    [Fact]
    public async Task Approve_TheErasureOfAnAdministrator_WithAnotherActive_Proceeds()
    {
        await using var db = CreateDb();
        var id = await SeedAsync(db, DataRightsRequestType.Erasure, requesterId: "admin-2");
        var users = new FakeUserDirectory()
            .With(Account("admin-1", "one@test.local", roles: WombatRoles.Administrator))
            .With(Account("admin-2", "two@test.local", roles: WombatRoles.Administrator));
        var harness = CreateHandler(db, users: users);

        var result = await Approve(harness, id);

        result.Request.Status.Should().Be(DataRightsRequestStatus.Completed);
        harness.Executor.Calls.Should().Be(1);
    }

    [Fact]
    public async Task Approve_TheErasureOfTheOnlyAdministrator_WithoutSalt_IsRefusedForTheSaltFirst()
    {
        await using var db = CreateDb();
        var id = await SeedAsync(db, DataRightsRequestType.Erasure, institutionId: 1, requesterId: "admin-only");
        var users = new FakeUserDirectory().With(Account("admin-only", "only@test.local", roles: WombatRoles.Administrator));
        var harness = CreateHandler(db, salt: null, users: users);

        var refusal = (await FluentActions.Invoking(() => Approve(harness, id, Coordinator(institutionId: 1)))
            .Should().ThrowAsync<DataRightsRefusedException>()).Which;

        refusal.Reason.Should().Be(DataRightsRefusal.ErasureNotConfigured);
    }

    /// <summary>
    /// F8: the requester withdrew between the approver's read and save. The save is refused on the request's token
    /// (simulated here by the interceptor writing the withdrawal through another context first), and the approver is told
    /// Withdrawn, nothing left staged, no mail.
    /// </summary>
    [Fact]
    public async Task Approve_LosingTheRaceToAWithdrawal_IsRefusedWithdrawn_AndMailsNobody()
    {
        var name = Guid.NewGuid().ToString();
        await using (var seed = CreateDb(name))
        {
            await SeedAsync(seed, DataRightsRequestType.Export);
        }

        var interceptor = new ConcurrentWrite(name, request => request.Withdraw());
        await using var db = CreateDb(name, interceptor);
        var id = await db.Set<DataRightsRequest>().Select(r => r.Id).SingleAsync();
        var harness = CreateHandler(db);

        var refusal = (await FluentActions.Invoking(() => Approve(harness, id))
            .Should().ThrowAsync<DataRightsRefusedException>()).Which;

        refusal.Reason.Should().Be(DataRightsRefusal.Withdrawn);
        refusal.InnerException.Should().BeOfType<DbUpdateConcurrencyException>("the audit pipeline sees a refused save (T201)");
        interceptor.Fired.Should().BeTrue();
        harness.Notices.Sender.Sent.Should().BeEmpty();
        await ShouldHaveStagedNothingAsync(db, id, DataRightsRequestStatus.Withdrawn);
    }

    [Fact]
    public async Task Approve_LosingTheRaceToAnotherDecision_IsRefusedDecidedMeanwhile()
    {
        var name = Guid.NewGuid().ToString();
        await using (var seed = CreateDb(name))
        {
            await SeedAsync(seed, DataRightsRequestType.Rectification);
        }

        var interceptor = new ConcurrentWrite(name, request => request.Reject("admin-2", "No.", DateTime.UtcNow));
        await using var db = CreateDb(name, interceptor);
        var id = await db.Set<DataRightsRequest>().Select(r => r.Id).SingleAsync();
        var harness = CreateHandler(db);

        var refusal = (await FluentActions.Invoking(() => Approve(harness, id))
            .Should().ThrowAsync<DataRightsRefusedException>()).Which;

        refusal.Reason.Should().Be(DataRightsRefusal.DecidedMeanwhile);
        harness.Notices.Sender.Sent.Should().BeEmpty();
        await ShouldHaveStagedNothingAsync(db, id, DataRightsRequestStatus.Rejected);
    }

    /// <summary>
    /// The erasure saves several times, and a withdrawal saved meanwhile arrives as the executor's PersonChanged. The row,
    /// read again, says it was the request: Withdrawn, not "something about this person changed".
    /// </summary>
    [Fact]
    public async Task Approve_Erasure_PersonChangedBecauseTheRequestWasWithdrawn_IsRefusedWithdrawn()
    {
        var name = Guid.NewGuid().ToString();
        await using (var seed = CreateDb(name))
        {
            await SeedAsync(seed, DataRightsRequestType.Erasure);
        }

        await using var db = CreateDb(name);
        var id = await db.Set<DataRightsRequest>().Select(r => r.Id).SingleAsync();
        var executor = new RecordingErasureExecutor
        {
            During = () => ConcurrentWrite.WriteAsync(name, request => request.Withdraw()),
            Throw = new DataRightsRefusedException(DataRightsRefusal.PersonChanged, "changed")
        };
        var harness = CreateHandler(db, executor: executor);

        var refusal = (await FluentActions.Invoking(() => Approve(harness, id))
            .Should().ThrowAsync<DataRightsRefusedException>()).Which;

        refusal.Reason.Should().Be(DataRightsRefusal.Withdrawn);
        harness.Notices.Sender.Sent.Should().BeEmpty();
    }

    [Fact]
    public async Task Approve_Erasure_PersonChangedElsewhere_StaysPersonChanged()
    {
        await using var db = CreateDb();
        var id = await SeedAsync(db, DataRightsRequestType.Erasure);
        var thrown = new DataRightsRefusedException(DataRightsRefusal.PersonChanged, "changed");
        var harness = CreateHandler(db, executor: new RecordingErasureExecutor { Throw = thrown });

        var refusal = (await FluentActions.Invoking(() => Approve(harness, id))
            .Should().ThrowAsync<DataRightsRefusedException>()).Which;

        refusal.Should().BeSameAs(thrown, "the request is still Submitted: something else about the person changed");
        harness.Notices.Sender.Sent.Should().BeEmpty();
    }

    /// <summary>
    /// T112, at the handler rather than the helper. Approving an Erasure runs
    /// <see cref="IErasureExecutor"/>, which irreversibly pseudonymises the subject's record — so the
    /// institution check has to be wired into THIS path, not merely available in a static class. The
    /// executor call count is the assertion that matters: a refusal that still erased would be worse
    /// than no gate at all.
    /// </summary>
    [Fact]
    public async Task Approve_Erasure_ByACoordinatorFromAnotherInstitution_IsRefusedAndErasesNothing()
    {
        await using var db = CreateDb();
        var id = await SeedAsync(db, DataRightsRequestType.Erasure, institutionId: 1);
        var harness = CreateHandler(db);

        await FluentActions.Invoking(() => Approve(harness, id, Coordinator(institutionId: 2)))
            .Should().ThrowAsync<UnauthorizedAccessException>();
        harness.Executor.Calls.Should().Be(0);
        await ShouldHaveStagedNothingAsync(db, id, DataRightsRequestStatus.Submitted);
    }

    [Fact]
    public async Task Approve_Erasure_ByACoordinatorInTheSameInstitution_Proceeds()
    {
        await using var db = CreateDb();
        var id = await SeedAsync(db, DataRightsRequestType.Erasure, institutionId: 1);
        var harness = CreateHandler(db);

        var result = await Approve(harness, id, Coordinator(institutionId: 1));

        harness.Executor.Calls.Should().Be(1);
        result.Request.Status.Should().Be(DataRightsRequestStatus.Completed);
    }

    /// <summary>An unstamped request is Administrator-only; erasure is irreversible.</summary>
    [Fact]
    public async Task Approve_Erasure_OfAnUnstampedRequest_IsRefusedToAScopedCoordinator()
    {
        await using var db = CreateDb();
        var id = await SeedAsync(db, DataRightsRequestType.Erasure);
        var harness = CreateHandler(db);

        await FluentActions.Invoking(() => Approve(harness, id, Coordinator(institutionId: 1)))
            .Should().ThrowAsync<UnauthorizedAccessException>();
        harness.Executor.Calls.Should().Be(0);
    }

    /// <summary>
    /// Writes a change to the one request through a second context on the same in-memory store, and moves its token, as a
    /// concurrent save on Postgres moves xmin. As an interceptor it fires once, just before the handler's own save.
    /// </summary>
    internal sealed class ConcurrentWrite(string databaseName, Action<DataRightsRequest> change) : SaveChangesInterceptor
    {
        public bool Fired { get; private set; }

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            if (!Fired)
            {
                Fired = true;
                await WriteAsync(databaseName, change);
            }

            return result;
        }

        public static async Task WriteAsync(string databaseName, Action<DataRightsRequest> change)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(databaseName).Options;
            await using var other = new ApplicationDbContext(options);
            var request = await other.Set<DataRightsRequest>().SingleAsync();
            change(request);
            var token = other.Entry(request).Property<uint>("xmin");
            token.CurrentValue = token.CurrentValue + 1;
            await other.SaveChangesAsync();
        }
    }

    /// <summary>
    /// One account that an erasure takes away: after <see cref="Erase" /> it answers as an erased account does (no
    /// address, deactivated). Every member a decision does not call throws.
    /// </summary>
    private sealed class ErasingDirectory(UserIdentityDetails account) : IUserAdministrationService
    {
        private UserIdentityDetails _account = account;

        public void Erase() => _account = _account with { Email = string.Empty, IsDeactivated = true, Roles = [] };

        public Task<UserIdentityDetails?> GetByIdAsync(string userId, CancellationToken cancellationToken = default)
            => Task.FromResult<UserIdentityDetails?>(userId == _account.UserId ? _account : null);

        public Task<IReadOnlyList<UserIdentityDetails>> ListUsersInRoleAsync(string role, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<UserIdentityDetails>>(_account.Roles.Contains(role) ? [_account] : []);

        public Task<IReadOnlyList<UserIdentityDetails>> ListAllUsersAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<UserIdentityDetails>>([_account]);

        public Task UpdateNamesAsync(string userId, string firstName, string lastName, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task UpdateScopeAsync(
            string userId, int institutionId, IReadOnlyCollection<int> specialityIds, IReadOnlyCollection<int> subSpecialityIds,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task PromotePendingTraineeAsync(string userId, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task AddRoleAsync(string userId, string role, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task RemoveRoleAsync(string userId, string role, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task ResetPasswordAsync(string userId, string newPassword, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task SetLockoutAsync(string userId, bool locked, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }

    /// <summary>A logger that keeps each line it is given.</summary>
    private sealed class ListLogger : ILogger<ApproveDataRightsRequestCommandHandler>
    {
        public List<string> Entries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => NullLogger.Instance.BeginScope(state);

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            => Entries.Add(formatter(state, exception));
    }
}
