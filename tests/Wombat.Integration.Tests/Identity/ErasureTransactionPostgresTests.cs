using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.DataRights;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.DataRights;
using Wombat.Infrastructure.Identity;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Integration.Tests.Identity;

/// <summary>
/// T156: an erasure is all or nothing, on real PostgreSQL with the real <see cref="ErasureExecutor" />. It writes in many
/// steps: raw updates that commit as they run, and Identity calls that each save the whole context. A failure part-way
/// used to leave a half-erased person: committee rows under a pseudonym, beside an account that still carried their name,
/// email, roles and institutional sign-in, and no erasure record to say so. T258 put the erasure in one transaction; T156
/// checks each Identity write's result, which the user store reports as a result rather than an exception, so a refused
/// one fails the erasure too instead of being stepped past and committed.
/// </summary>
/// <remarks>
/// <para>
/// The person erased chaired a committee review (raw updates: who started it, who ratified it, who decided) and sat on
/// its panel and at its decision (tracked updates), and holds a password, a role and an institutional sign-in. A failure
/// is put in at three places through the user manager the executor calls: before the account is touched, when a
/// concurrent edit makes Identity refuse the account's update, and after the account is cleared and its roles removed.
/// </para>
/// <para>
/// After each failure the test saves the same context, as the audit pipeline's failure row does (the audit trap), and
/// only then looks. Isolated the way <c>SsoErasurePostgresTests</c> is: a migrated schema of its own.
/// </para>
/// </remarks>
public sealed class ErasureTransactionPostgresTests : IAsyncLifetime
{
    private const string Email = "naidoo@kgk.test";
    private const string Provider = "kgk";
    private const string ProviderSubject = "idp-subject-1";

    private readonly TestSchemas _schemas = new();

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _schemas.DropAllAsync();

    [Fact]
    public async Task AFaultBeforeTheAccountIsTouched_ErasesNothing_AndLeavesNothingForTheAuditRowsSaveToCommit()
    {
        await RunAsync(
            fault => fault.ThrowAtFindUser = true,
            (thrown, _) => thrown.Should().BeOfType<InvalidOperationException>().Which.Message.Should().Contain("the test put here"));
    }

    [Fact]
    public async Task AnAccountChangedDuringTheErasure_IsRefusedAsThePersonChanged_AndErasesNothing()
    {
        // An administrator's edit lands between the erasure reading the account and writing it. The user store reports the
        // conflict as a failed result, not an exception, and until T156 the erasure went on past it. It is refused as a
        // conflict on any other row about the person is (T258), in the same words (T156 review).
        await RunAsync(
            fault => fault.ConcurrentEditBeforeUpdate = true,
            (thrown, _) => thrown.Should().BeOfType<InvalidOperationException>()
                .Which.Message.Should().Be(ErasureExecutor.PersonChanged, "it stops at the refusal, not at a later step"),
            concurrentEditStands: true);
    }

    /// <summary>
    /// An Identity write refused as a result: the account's own update, or a later write after the account has been
    /// cleared. Until T156 the erasure went on past it and committed: an erased account that kept its role or its
    /// institutional sign-in, recorded as erased, or, past a refused update, one cleared by the next write's save anyway.
    /// A refusal that is not a conflict names the step and Identity's code.
    /// </summary>
    [Theory]
    [InlineData(IdentityStep.Update, "could not clear the account (DefaultError)")]
    [InlineData(IdentityStep.RemoveRoles, "could not remove the account's roles (DefaultError)")]
    [InlineData(IdentityStep.RemoveLogin, "could not remove the account's institutional sign-in (DefaultError)")]
    public async Task AnIdentityWriteRefused_FailsTheErasure_NamingTheStep_AndErasesNothing(IdentityStep step, string named)
    {
        await RunAsync(
            fault => fault.RefuseAt = step,
            (thrown, _) => thrown.Should().BeOfType<InvalidOperationException>()
                .Which.Message.Should().Contain(named).And.NotContain(Email));
    }

    [Fact]
    public async Task AFaultAfterTheAccountIsClearedAndItsRolesRemoved_PutsThemBack()
    {
        await RunAsync(
            fault => fault.ThrowAtRemoveLogin = true,
            (thrown, _) => thrown.Should().BeOfType<InvalidOperationException>().Which.Message.Should().Contain("the test put here"));
    }

    [Fact]
    public async Task WithoutAFault_TheErasureCommits()
    {
        // The control: the same arrangement, erased.
        var schema = await _schemas.CreateAsync();
        try
        {
            await using var root = await ServicesAsync(schema);
            var arranged = await ArrangeAsync(root);

            await using (var act = root.CreateAsyncScope())
            {
                var db = act.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var request = await db.DataRightsRequests.SingleAsync(entity => entity.Id == arranged.RequestId);
                await act.ServiceProvider.GetRequiredService<ErasureExecutor>().ExecuteAsync(request, "salt-for-tests", CancellationToken.None);
            }

            await using var read = root.CreateAsyncScope();
            var readDb = read.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var account = await readDb.Users.AsNoTracking().SingleAsync(user => user.Id == arranged.UserId);
            account.Email.Should().BeNull();
            (await readDb.UserRoles.AnyAsync(role => role.UserId == arranged.UserId)).Should().BeFalse();
            (await readDb.UserLogins.AnyAsync(login => login.UserId == arranged.UserId)).Should().BeFalse();
            var review = await readDb.CommitteeReviews.AsNoTracking().SingleAsync(entity => entity.Id == arranged.ReviewId);
            review.StartedByUserId.Should().StartWith("deleted_user_");
            (await readDb.DataRightsErasureRecords.AnyAsync(record => record.UserId == arranged.UserId)).Should().BeTrue();
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    private async Task RunAsync(
        Action<ErasureFault> arm,
        Action<Exception, Arranged> assertThrown,
        bool concurrentEditStands = false)
    {
        var schema = await _schemas.CreateAsync();
        try
        {
            await using var root = await ServicesAsync(schema);
            var arranged = await ArrangeAsync(root);
            var fault = root.GetRequiredService<ErasureFault>();
            fault.EditConcurrently = async userId =>
            {
                await using var other = root.CreateAsyncScope();
                await other.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.ExecuteSqlInterpolatedAsync(
                    $"UPDATE \"AspNetUsers\" SET \"FirstName\" = 'Edited', \"ConcurrencyStamp\" = 'edited' WHERE \"Id\" = {userId}");
            };

            await using (var act = root.CreateAsyncScope())
            {
                var db = act.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var request = await db.DataRightsRequests.SingleAsync(entity => entity.Id == arranged.RequestId);

                // As the approval handler does: the request is approved in this context before the erasure runs.
                request.Approve("admin-1", "Approved.", DateTime.UtcNow);

                arm(fault);
                var erase = () => act.ServiceProvider.GetRequiredService<ErasureExecutor>()
                    .ExecuteAsync(request, "salt-for-tests", CancellationToken.None);
                var thrown = (await erase.Should().ThrowAsync<Exception>()).Which;
                fault.Disarm();
                assertThrown(thrown, arranged);

                // What the audit pipeline does next: its failure row is saved through this same context.
                await db.SaveChangesAsync();
            }

            await using var read = root.CreateAsyncScope();
            var readDb = read.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var account = await readDb.Users.AsNoTracking().SingleAsync(user => user.Id == arranged.UserId);
            account.Email.Should().Be(Email, "the account keeps its email");
            account.UserName.Should().Be(Email);
            account.LastName.Should().Be("Naidoo");
            account.FirstName.Should().Be(concurrentEditStands ? "Edited" : "Priya",
                concurrentEditStands ? "the concurrent edit is the only change" : "the account keeps its name");
            account.PasswordHash.Should().NotBeNull("the account keeps its password");
            account.InstitutionId.Should().Be(arranged.InstitutionId);
            account.LockoutEnd.Should().BeNull("the account is not deactivated");

            (await readDb.UserRoles.CountAsync(role => role.UserId == arranged.UserId)).Should().Be(1, "the role is kept");
            (await readDb.UserLogins.SingleAsync(login => login.UserId == arranged.UserId)).ProviderKey
                .Should().Be(ProviderSubject, "the institutional sign-in is kept");

            var review = await readDb.CommitteeReviews.AsNoTracking()
                .Include(entity => entity.Decisions)
                .SingleAsync(entity => entity.Id == arranged.ReviewId);
            review.StartedByUserId.Should().Be(arranged.UserId, "the raw updates were rolled back");
            review.RatifiedByUserId.Should().Be(arranged.UserId);
            review.Decisions.Should().OnlyContain(decision => decision.DecidedByChairUserId == arranged.UserId);

            (await readDb.DecisionPanelMembers.AsNoTracking().CountAsync(member => member.UserId == arranged.UserId))
                .Should().Be(1, "the tracked updates were not committed, by the erasure or by the audit row's save");
            (await readDb.CommitteeDecisionAttendees.AsNoTracking().CountAsync(attendee => attendee.UserId == arranged.UserId))
                .Should().Be(1);

            (await readDb.DataRightsErasureRecords.AnyAsync()).Should().BeFalse("no erasure is recorded");
            (await readDb.DataRightsRequests.AsNoTracking().SingleAsync(entity => entity.Id == arranged.RequestId)).Status
                .Should().Be(DataRightsRequestStatus.Submitted, "the approval went with the erasure, so the request can be approved again");
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    private static async Task<ServiceProvider> ServicesAsync(string schema)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(TestDatabase.SchemaConnectionString(schema)));
        services.AddIdentity<WombatIdentityUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();
        services.AddSingleton<ErasureFault>();
        services.AddScoped<UserManager<WombatIdentityUser>, FaultingUserManager>();
        services.AddScoped<ErasureExecutor>();

        var root = services.BuildServiceProvider();
        await using var migrationScope = root.CreateAsyncScope();
        await migrationScope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.MigrateAsync();
        return root;
    }

    private static async Task<Arranged> ArrangeAsync(ServiceProvider root)
    {
        await using var arrange = root.CreateAsyncScope();
        var db = arrange.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var institution = new Institution { Name = "Kgosi Kgari", ShortCode = "KGK", IsActive = true, CreatedOn = DateTime.UtcNow };
        db.Institutions.Add(institution);
        await db.SaveChangesAsync();

        var users = arrange.ServiceProvider.GetRequiredService<UserManager<WombatIdentityUser>>();
        var roles = arrange.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        (await roles.CreateAsync(new IdentityRole("CommitteeMember"))).Succeeded.Should().BeTrue();
        var chair = new WombatIdentityUser
        {
            UserName = Email,
            Email = Email,
            FirstName = "Priya",
            LastName = "Naidoo",
            InstitutionId = institution.Id
        };
        (await users.CreateAsync(chair, "Correct-Horse-Battery-9!")).Succeeded.Should().BeTrue();
        (await users.AddToRoleAsync(chair, "CommitteeMember")).Succeeded.Should().BeTrue();
        (await users.AddLoginAsync(chair, new UserLoginInfo(Provider, ProviderSubject, "KGK sign-in"))).Succeeded.Should().BeTrue();

        var members = new[]
        {
            new DecisionPanelMember { UserId = chair.Id, Role = DecisionPanelMemberRole.Chair },
            new DecisionPanelMember { UserId = "member-1", Role = DecisionPanelMemberRole.Member }
        };
        var panel = new DecisionPanel
        {
            Name = "Paediatrics CCC",
            Scope = DecisionPanelScope.Institution,
            InstitutionId = institution.Id,
            CreatedOn = DateTime.UtcNow,
            Members = members
        };
        var sitting = new DateTime(2026, 7, 2, 9, 0, 0, DateTimeKind.Utc);
        var review = new CommitteeReview
        {
            AcademicYear = 2026,
            Semester = 1,
            Panel = panel,
            TraineeUserId = "trainee-1",
            ReviewPeriodFrom = new DateOnly(2026, 1, 1),
            ReviewPeriodTo = new DateOnly(2026, 6, 30),
            ScheduledOn = new DateOnly(2026, 7, 2)
        };
        review.Start([], chair.Id, sitting);
        review.RecordDecision(
            CommitteeDecisionCategory.SatisfactoryProgress, "On track.", null, chair.Id, sitting, members, [], []);
        review.Ratify(chair.Id, sitting);
        db.DecisionPanels.Add(panel);
        db.CommitteeReviews.Add(review);

        var request = DataRightsRequest.Create(
            chair.Id, "Priya Naidoo", DataRightsRequestType.Erasure, "Leaving the programme.", DateTime.UtcNow);
        db.DataRightsRequests.Add(request);
        await db.SaveChangesAsync();

        return new Arranged(chair.Id, institution.Id, review.Id, request.Id);
    }

    private sealed record Arranged(string UserId, int InstitutionId, int ReviewId, Guid RequestId);

    /// <summary>An Identity write the erasure makes: the account's own update, and those after it.</summary>
    public enum IdentityStep
    {
        None,
        Update,
        RemoveRoles,
        RemoveLogin
    }

    /// <summary>Where the user manager fails, set by each test just before the erasure and cleared after it.</summary>
    private sealed class ErasureFault
    {
        public bool ThrowAtFindUser { get; set; }

        public bool ConcurrentEditBeforeUpdate { get; set; }

        public bool ThrowAtRemoveLogin { get; set; }

        /// <summary>The write the user store refuses, as it reports a refusal: a failed result, not an exception.</summary>
        public IdentityStep RefuseAt { get; set; }

        /// <summary>An edit of the account through another connection, committed at once.</summary>
        public Func<string, Task>? EditConcurrently { get; set; }

        public void Disarm()
        {
            ThrowAtFindUser = false;
            ConcurrentEditBeforeUpdate = false;
            ThrowAtRemoveLogin = false;
            RefuseAt = IdentityStep.None;
        }

        public static IdentityResult Refused() => IdentityResult.Failed(new IdentityErrorDescriber().DefaultError());

        public static InvalidOperationException Thrown() => new("A fault the test put here: the database has gone.");
    }

    /// <summary>Identity's user manager, failing where <see cref="ErasureFault" /> says.</summary>
    private sealed class FaultingUserManager(
        IUserStore<WombatIdentityUser> store,
        IOptions<IdentityOptions> optionsAccessor,
        IPasswordHasher<WombatIdentityUser> passwordHasher,
        IEnumerable<IUserValidator<WombatIdentityUser>> userValidators,
        IEnumerable<IPasswordValidator<WombatIdentityUser>> passwordValidators,
        ILookupNormalizer keyNormalizer,
        IdentityErrorDescriber errors,
        IServiceProvider services,
        ILogger<UserManager<WombatIdentityUser>> logger,
        ErasureFault fault)
        : UserManager<WombatIdentityUser>(
            store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
    {
        public override Task<WombatIdentityUser?> FindByIdAsync(string userId)
            => fault.ThrowAtFindUser ? throw ErasureFault.Thrown() : base.FindByIdAsync(userId);

        public override async Task<IdentityResult> UpdateAsync(WombatIdentityUser user)
        {
            if (fault.ConcurrentEditBeforeUpdate)
            {
                await fault.EditConcurrently!(user.Id);
            }

            return fault.RefuseAt == IdentityStep.Update ? ErasureFault.Refused() : await base.UpdateAsync(user);
        }

        public override Task<IdentityResult> RemoveFromRolesAsync(WombatIdentityUser user, IEnumerable<string> roles)
            => fault.RefuseAt == IdentityStep.RemoveRoles
                ? Task.FromResult(ErasureFault.Refused())
                : base.RemoveFromRolesAsync(user, roles);

        public override Task<IdentityResult> RemoveLoginAsync(WombatIdentityUser user, string loginProvider, string providerKey)
        {
            if (fault.ThrowAtRemoveLogin)
            {
                throw ErasureFault.Thrown();
            }

            return fault.RefuseAt == IdentityStep.RemoveLogin
                ? Task.FromResult(ErasureFault.Refused())
                : base.RemoveLoginAsync(user, loginProvider, providerKey);
        }
    }
}
