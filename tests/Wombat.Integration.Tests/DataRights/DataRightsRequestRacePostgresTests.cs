using System.Globalization;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Options;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.DataRights;
using Wombat.Application.Features.DataRights.Commands;
using Wombat.Domain.DataRights;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.DataRights;
using Wombat.Infrastructure.Identity;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Integration.Tests.DataRights;

/// <summary>
/// T387, F8, on a real PostgreSQL server: an approval and a withdrawal of one request, each in its own context, each reading
/// it Submitted. The request's xmin refuses whichever saves second. When the approval is second it is told Withdrawn (the
/// row read again, Withdrawn checked first), nothing is erased and nobody is mailed; when the withdrawal is second its save
/// is refused and the approval stands. Until T387 the request had no token, so an erasure approved on a status read a
/// moment before erased a person who had just taken the request back.
/// </summary>
/// <remarks>
/// The concurrent writer runs inside the approving handler, at the moment the race needs: an Erasure's after the handler
/// has read the request and before the executor's first save (the user directory's address read, which comes between);
/// an Export's from a save interceptor, just before the handler's own save. Isolated as
/// <c>ErasureEndsOpenRecordsPostgresTests</c> is: a migrated schema of its own, dropped in a finally and on dispose.
/// </remarks>
public sealed class DataRightsRequestRacePostgresTests : IAsyncLifetime
{
    private const string RequesterId = "requester-t387-race";
    private const string CoordinatorUserId = "coordinator-t387-race";
    private const string Salt = "salt-for-tests";

    private readonly TestSchemas _schemas = new();

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _schemas.DropAllAsync();

    [Fact]
    public async Task OnPostgres_AnErasureApprovedAsTheRequesterWithdraws_IsRefusedWithdrawn_AndErasesNothing()
    {
        try
        {
            var schema = await _schemas.CreateAsync();
            await using var root = await ServicesAsync(schema);
            var (hostId, requestId) = await ArrangeAsync(root, DataRightsRequestType.Erasure);

            DataRightsRefusedException refusal;
            TestNotices notices;
            await using (var act = root.CreateAsyncScope())
            {
                var context = act.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var users = new WithdrawingDirectory(Users(act.ServiceProvider), () => WithdrawAsync(schema, requestId));
                notices = new TestNotices(users);
                var handler = new ApproveDataRightsRequestCommandHandler(
                    context,
                    act.ServiceProvider.GetRequiredService<ErasureExecutor>(),
                    users,
                    notices.Notices,
                    Options.Create(new WombatOptions { PseudonymSalt = Salt }),
                    NullLogger<ApproveDataRightsRequestCommandHandler>.Instance);

                refusal = (await FluentActions.Invoking(() => handler.Handle(
                        new ApproveDataRightsRequestCommand(requestId, "Identity confirmed.", Coordinator(hostId)),
                        CancellationToken.None))
                    .Should().ThrowAsync<DataRightsRefusedException>()).Which;

                users.Withdrew.Should().BeTrue("the withdrawal ran between the approval's read and its save");

                // As the audit pipeline would from its catch: whatever is still tracked is saved. Nothing is.
                context.ChangeTracker.Entries().Should().BeEmpty();
                await context.SaveChangesAsync();
            }

            refusal.Reason.Should().Be(DataRightsRefusal.Withdrawn,
                "the erasure's save is refused on the request's token, and the row read again says it was withdrawn");
            notices.Sender.Sent.Should().BeEmpty();

            await using var read = root.CreateAsyncScope();
            var db = read.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var stored = await db.DataRightsRequests.SingleAsync(request => request.Id == requestId);
            stored.Status.Should().Be(DataRightsRequestStatus.Withdrawn);
            stored.DecidedByUserId.Should().BeNull();
            stored.RequesterDisplayName.Should().Be("Requester Race");
            (await db.DataRightsErasureRecords.AnyAsync()).Should().BeFalse();
            var account = await db.Users.SingleAsync(user => user.Id == RequesterId);
            account.Email.Should().Be($"{RequesterId}@test.local", "nothing was erased");
            (await db.UserRoles.AnyAsync(link => link.UserId == RequesterId)).Should().BeTrue();
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    [Fact]
    public async Task OnPostgres_AnExportApprovedAsTheRequesterWithdraws_IsRefusedWithdrawn()
    {
        try
        {
            var schema = await _schemas.CreateAsync();
            await using var root = await ServicesAsync(schema);
            var (hostId, requestId) = await ArrangeAsync(root, DataRightsRequestType.Export);

            var interceptor = new BeforeFirstSave(() => WithdrawAsync(schema, requestId));
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseNpgsql(TestDatabase.SchemaConnectionString(schema))
                .AddInterceptors(interceptor)
                .Options;

            await using (var context = new ApplicationDbContext(options))
            await using (var scope = root.CreateAsyncScope())
            {
                var users = Users(scope.ServiceProvider);
                var notices = new TestNotices(users);
                var handler = new ApproveDataRightsRequestCommandHandler(
                    context,
                    scope.ServiceProvider.GetRequiredService<ErasureExecutor>(),
                    users,
                    notices.Notices,
                    Options.Create(new WombatOptions { PseudonymSalt = Salt }),
                    NullLogger<ApproveDataRightsRequestCommandHandler>.Instance);

                var refusal = (await FluentActions.Invoking(() => handler.Handle(
                        new ApproveDataRightsRequestCommand(requestId, "Here it is.", Coordinator(hostId)),
                        CancellationToken.None))
                    .Should().ThrowAsync<DataRightsRefusedException>()).Which;

                interceptor.Fired.Should().BeTrue();
                refusal.Reason.Should().Be(DataRightsRefusal.Withdrawn);
                refusal.InnerException.Should().BeOfType<DbUpdateConcurrencyException>();
                notices.Sender.Sent.Should().BeEmpty();
                context.ChangeTracker.Entries().Should().BeEmpty("the refused approval is not left for the audit write");
            }

            await using var read = root.CreateAsyncScope();
            var stored = await read.ServiceProvider.GetRequiredService<ApplicationDbContext>().DataRightsRequests
                .SingleAsync(request => request.Id == requestId);
            stored.Status.Should().Be(DataRightsRequestStatus.Withdrawn);
            stored.DecidedByUserId.Should().BeNull();
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    /// <summary>The other order: the withdrawal read the request Submitted, the approval saved first, the withdrawal is refused.</summary>
    [Fact]
    public async Task OnPostgres_AWithdrawalSavedAfterAnApproval_IsRefusedOnTheToken_AndTheApprovalStands()
    {
        try
        {
            var schema = await _schemas.CreateAsync();
            await using var root = await ServicesAsync(schema);
            var (hostId, requestId) = await ArrangeAsync(root, DataRightsRequestType.Export);

            await using var withdrawing = new ApplicationDbContext(
                new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(TestDatabase.SchemaConnectionString(schema)).Options);
            var stale = await withdrawing.DataRightsRequests.SingleAsync(request => request.Id == requestId);

            await using (var act = root.CreateAsyncScope())
            {
                var users = Users(act.ServiceProvider);
                await new ApproveDataRightsRequestCommandHandler(
                        act.ServiceProvider.GetRequiredService<ApplicationDbContext>(),
                        act.ServiceProvider.GetRequiredService<ErasureExecutor>(),
                        users,
                        new TestNotices(users).Notices,
                        Options.Create(new WombatOptions { PseudonymSalt = Salt }),
                        NullLogger<ApproveDataRightsRequestCommandHandler>.Instance)
                    .Handle(new ApproveDataRightsRequestCommand(requestId, "Here it is.", Coordinator(hostId)), CancellationToken.None);
            }

            stale.Withdraw();
            await FluentActions.Invoking(() => withdrawing.SaveChangesAsync())
                .Should().ThrowAsync<DbUpdateConcurrencyException>();

            await using var read = root.CreateAsyncScope();
            (await read.ServiceProvider.GetRequiredService<ApplicationDbContext>().DataRightsRequests
                    .SingleAsync(request => request.Id == requestId)).Status
                .Should().Be(DataRightsRequestStatus.Completed);
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    /// <summary>
    /// Review S2: a correction recorded on a page read while the Rectification was approved, saved after another tab completed
    /// it, is refused on the request's token as no longer an approved Rectification, and records nothing.
    /// </summary>
    [Fact]
    public async Task OnPostgres_ACorrectionSavedAfterTheRequestWasCompleted_IsRefused_AndRecordsNothing()
    {
        try
        {
            var schema = await _schemas.CreateAsync();
            await using var root = await ServicesAsync(schema);
            var (hostId, requestId) = await ArrangeAsync(root, DataRightsRequestType.Rectification);
            await using (var approve = root.CreateAsyncScope())
            {
                var db = approve.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                (await db.DataRightsRequests.SingleAsync(request => request.Id == requestId))
                    .Approve(CoordinatorUserId, "Will correct.", DateTime.UtcNow);
                await db.SaveChangesAsync();
            }

            var interceptor = new BeforeFirstSave(() => CompleteAsync(schema, requestId));
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseNpgsql(TestDatabase.SchemaConnectionString(schema))
                .AddInterceptors(interceptor)
                .Options;

            await using (var context = new ApplicationDbContext(options))
            await using (var scope = root.CreateAsyncScope())
            {
                var handler = new ApplyRectificationCommandHandler(context, Users(scope.ServiceProvider));

                var refusal = (await FluentActions.Invoking(() => handler.Handle(
                        new ApplyRectificationCommand(
                            requestId, DataRightsRecords.AccountLastName, "Mahlngu", "Mahlangu", Coordinator(hostId)),
                        CancellationToken.None))
                    .Should().ThrowAsync<DataRightsRefusedException>()).Which;

                interceptor.Fired.Should().BeTrue();
                refusal.Reason.Should().Be(DataRightsRefusal.NotAnApprovedRectification);
                refusal.InnerException.Should().BeOfType<DbUpdateConcurrencyException>();
            }

            await using var read = root.CreateAsyncScope();
            var stored = read.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            (await stored.DataRightsRequests.SingleAsync(request => request.Id == requestId)).Status
                .Should().Be(DataRightsRequestStatus.Completed);
            (await stored.DataRightsRectifications.AnyAsync(correction => correction.RequestId == requestId)).Should().BeFalse();
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    /// <summary>Another tab completes the request, through a context of its own.</summary>
    private static async Task CompleteAsync(string schema, Guid requestId)
    {
        await using var db = new ApplicationDbContext(
            new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(TestDatabase.SchemaConnectionString(schema)).Options);
        var request = await db.DataRightsRequests.SingleAsync(entity => entity.Id == requestId);
        request.Complete(DateTime.UtcNow);
        await db.SaveChangesAsync();
    }

    /// <summary>The requester withdraws through a context of its own, as their own page's command would.</summary>
    private static async Task WithdrawAsync(string schema, Guid requestId)
    {
        await using var db = new ApplicationDbContext(
            new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(TestDatabase.SchemaConnectionString(schema)).Options);
        var request = await db.DataRightsRequests.SingleAsync(entity => entity.Id == requestId);
        request.Withdraw();
        await db.SaveChangesAsync();
    }

    private static async Task<(int HostId, Guid RequestId)> ArrangeAsync(ServiceProvider root, DataRightsRequestType type)
    {
        await using var arrange = root.CreateAsyncScope();
        var db = arrange.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var hostId = await db.Institutions.Where(entity => entity.ShortCode == "DEMO").Select(entity => entity.Id).SingleAsync();
        NomineeSeed.AddUser(db, RequesterId, hostId, WombatRoles.Trainee);
        var request = DataRightsRequest.Create(
            RequesterId, "Requester Race", type, "Leaving the programme.", DateTime.UtcNow, hostId);
        db.DataRightsRequests.Add(request);
        await db.SaveChangesAsync();
        return (hostId, request.Id);
    }

    private static ClaimsPrincipal Coordinator(int institutionId)
        => new(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, CoordinatorUserId),
                new Claim(ClaimTypes.Role, WombatRoles.Coordinator),
                new Claim(WombatClaimTypes.InstitutionId, institutionId.ToString(CultureInfo.InvariantCulture))
            ],
            "IntegrationTest",
            ClaimTypes.Name,
            ClaimTypes.Role));

    private static UserAdministrationService Users(IServiceProvider services)
        => new(services.GetRequiredService<UserManager<WombatIdentityUser>>(), services.GetRequiredService<ApplicationDbContext>());

    private static async Task<ServiceProvider> ServicesAsync(string schema)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(TestDatabase.SchemaConnectionString(schema)));
        services.AddIdentity<WombatIdentityUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();
        services.AddScoped<ErasureExecutor>();

        var root = services.BuildServiceProvider();

        await using (var scope = root.CreateAsyncScope())
        {
            await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.MigrateAsync();
        }

        await using (var scope = root.CreateAsyncScope())
        {
            await new DataSeeder(scope.ServiceProvider.GetRequiredService<ApplicationDbContext>()).SeedAsync();
        }

        return root;
    }

    /// <summary>
    /// The real directory, whose address read (the approval's last read before the erasure's first save) first lets the
    /// requester withdraw.
    /// </summary>
    private sealed class WithdrawingDirectory(IUserAdministrationService inner, Func<Task> withdraw) : IUserAdministrationService
    {
        public bool Withdrew { get; private set; }

        public async Task<UserIdentityDetails?> GetByIdAsync(string userId, CancellationToken cancellationToken = default)
        {
            if (!Withdrew)
            {
                Withdrew = true;
                await withdraw();
            }

            return await inner.GetByIdAsync(userId, cancellationToken);
        }

        public Task<IReadOnlyList<UserIdentityDetails>> ListUsersInRoleAsync(string role, CancellationToken cancellationToken = default)
            => inner.ListUsersInRoleAsync(role, cancellationToken);

        public Task<IReadOnlyList<UserIdentityDetails>> ListAllUsersAsync(CancellationToken cancellationToken = default)
            => inner.ListAllUsersAsync(cancellationToken);

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

    /// <summary>Runs the concurrent writer once, just before the context's first save.</summary>
    private sealed class BeforeFirstSave(Func<Task> write) : SaveChangesInterceptor
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
                await write();
            }

            return result;
        }
    }
}
