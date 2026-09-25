using System.Globalization;
using System.Security.Claims;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Audit;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Users;
using Wombat.Application.Features.Users.Commands.RemoveRoleFromUser;
using Wombat.Application.Features.Users.Commands.ResetUserPassword;
using Wombat.Application.Features.Users.Commands.SetUserLockout;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Audit;
using Wombat.Infrastructure.Identity;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Integration.Tests.Identity;

/// <summary>
/// T278 on a real PostgreSQL server, through the real user store (<see cref="UserAdministrationService" />) and the real
/// audit pipeline: a Trainee who also administers their institution removes no role, their own Trainee role included, and
/// locks out nobody, their panel's chair included; an administrator changes none of their own roles, lockout or password.
/// Each refusal leaves its failure row and changes nothing.
/// </summary>
/// <remarks>
/// <para>
/// The audit trap, as the store has it: <c>UserManager</c> saves each change at once, on the request's context, the one
/// the pipeline's catch writes the failure row through. A refusal that came after the store's write would leave the change
/// committed under a row that says the command failed. Every refusal here comes before the user is looked up, and the
/// fresh read after each proves it.
/// </para>
/// <para>
/// Isolated as <c>DecisionPanelTraineeSeatPostgresTests</c> is: a migrated schema of its own, dropped in a finally and
/// again on dispose.
/// </para>
/// </remarks>
public sealed class UserAdministrationSelfAndTraineePostgresTests : IAsyncLifetime
{
    private const string Registrar = "registrar-a";
    private const string Chair = "chair-a";
    private const string Admin = "inst-admin-a";

    private readonly TestSchemas _schemas = new();

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _schemas.DropAllAsync();

    [Fact]
    public async Task OnPostgres_EveryRefusedSelfOrTraineeChange_LeavesItsFailureRow_AndChangesNothing()
    {
        var schema = await _schemas.CreateAsync();

        try
        {
            await using var root = await MigratedServicesAsync(schema);

            int institutionId;
            await using (var arrange = root.CreateAsyncScope())
            {
                var db = arrange.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var institution = new Institution { Name = "A", ShortCode = "A", IsActive = true, CreatedOn = DateTime.UtcNow };
                db.Institutions.Add(institution);
                await db.SaveChangesAsync();
                institutionId = institution.Id;

                NomineeSeed.AddUser(db, Registrar, institutionId, WombatRoles.Trainee, WombatRoles.InstitutionalAdmin);
                NomineeSeed.AddUser(db, Chair, institutionId, WombatRoles.CommitteeMember);
                NomineeSeed.AddUser(db, Admin, institutionId, WombatRoles.InstitutionalAdmin);
                await db.SaveChangesAsync();
            }

            var registrar = Caller(Registrar, institutionId, WombatRoles.Trainee, WombatRoles.InstitutionalAdmin);
            var admin = Caller(Admin, institutionId, WombatRoles.InstitutionalAdmin);

            // The Trainee who administers their institution: not their own Trainee role, not their panel's chair.
            (await RemoveRoleAsync(root, new RemoveRoleFromUserCommand(Registrar, WombatRoles.Trainee, registrar)))
                .Should().Be(UserAdministrationRules.TraineeAdministersNoUser);
            (await RemoveRoleAsync(root, new RemoveRoleFromUserCommand(Chair, WombatRoles.CommitteeMember, registrar)))
                .Should().Be(UserAdministrationRules.TraineeAdministersNoUser);
            (await SetLockoutAsync(root, new SetUserLockoutCommand(Chair, true, registrar)))
                .Should().Be(UserAdministrationRules.TraineeAdministersNoUser);

            // An administrator who is not a trainee: none of their own roles, lockout or password.
            (await RemoveRoleAsync(root, new RemoveRoleFromUserCommand(Admin, WombatRoles.InstitutionalAdmin, admin)))
                .Should().Be(UserAdministrationRules.OwnRolesNotChangeable);
            (await SetLockoutAsync(root, new SetUserLockoutCommand(Admin, true, admin)))
                .Should().Be(UserAdministrationRules.OwnLockoutNotChangeable);
            (await ResetPasswordAsync(root, new ResetUserPasswordCommand(Admin, "A-new-password-1", admin)))
                .Should().Be(UserAdministrationRules.OwnPasswordNotResettable);

            await using (var read = root.CreateAsyncScope())
            {
                var db = read.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                (await RolesOf(db, Registrar)).Should().BeEquivalentTo([WombatRoles.Trainee, WombatRoles.InstitutionalAdmin]);
                (await RolesOf(db, Chair)).Should().BeEquivalentTo([WombatRoles.CommitteeMember]);
                (await RolesOf(db, Admin)).Should().BeEquivalentTo([WombatRoles.InstitutionalAdmin]);

                var accounts = await db.Users.AsNoTracking().ToListAsync();
                accounts.Should().OnlyContain(user => user.LockoutEnd == null, "nobody was locked out");
                accounts.Single(user => user.Id == Admin).PasswordHash.Should().BeNull("the password was not reset");

                var rows = await db.AuditEntries.AsNoTracking().OrderBy(entry => entry.OccurredAt).ToListAsync();
                rows.Should().HaveCount(6).And.OnlyContain(entry => !entry.Success, "each refusal leaves its failure row");
                rows.Select(entry => entry.ErrorMessage).Should().Equal(
                    UserAdministrationRules.TraineeAdministersNoUser,
                    UserAdministrationRules.TraineeAdministersNoUser,
                    UserAdministrationRules.TraineeAdministersNoUser,
                    UserAdministrationRules.OwnRolesNotChangeable,
                    UserAdministrationRules.OwnLockoutNotChangeable,
                    UserAdministrationRules.OwnPasswordNotResettable);
            }

            // The control: the administrator who is not a trainee takes CommitteeMember from the chair, through the same
            // real store and pipeline.
            (await RemoveRoleAsync(root, new RemoveRoleFromUserCommand(Chair, WombatRoles.CommitteeMember, admin))).Should().BeNull();

            await using (var read = root.CreateAsyncScope())
            {
                var db = read.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                (await RolesOf(db, Chair)).Should().BeEmpty();
                (await db.AuditEntries.AsNoTracking().CountAsync(entry => entry.Success)).Should().Be(1);
            }
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    // ─── The commands, as a request runs them ────────────────────────────────

    private static Task<string?> RemoveRoleAsync(ServiceProvider root, RemoveRoleFromUserCommand command)
        => RunAsync(root, command, (users, request) => new RemoveRoleFromUserCommandHandler(users).Handle(request, CancellationToken.None));

    private static Task<string?> SetLockoutAsync(ServiceProvider root, SetUserLockoutCommand command)
        => RunAsync(root, command, (users, request) => new SetUserLockoutCommandHandler(users).Handle(request, CancellationToken.None));

    private static Task<string?> ResetPasswordAsync(ServiceProvider root, ResetUserPasswordCommand command)
        => RunAsync(root, command, (users, request) => new ResetUserPasswordCommandHandler(users).Handle(request, CancellationToken.None));

    /// <summary>
    /// Runs one command in a request scope of its own, inside the real audit pipeline writing through the real
    /// <see cref="AuditWriter" /> on the request's context, the one the real store saves through. Returns the refusal's
    /// words, or null when it was done.
    /// </summary>
    private static async Task<string?> RunAsync<TCommand>(
        ServiceProvider root,
        TCommand command,
        Func<IUserAdministrationService, TCommand, Task> handle)
        where TCommand : notnull
    {
        await using var request = root.CreateAsyncScope();
        var db = request.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var users = new UserAdministrationService(request.ServiceProvider.GetRequiredService<UserManager<WombatIdentityUser>>(), db);

        try
        {
            await new AuditPipelineBehavior<TCommand, Unit>(new AuditWriter(db), new FixedAuditContext()).Handle(
                command,
                async () =>
                {
                    await handle(users, command);
                    return Unit.Value;
                },
                CancellationToken.None);
            return null;
        }
        catch (UnauthorizedAccessException refusal)
        {
            return refusal.Message;
        }
    }

    private static async Task<IReadOnlyList<string>> RolesOf(ApplicationDbContext db, string userId)
        => await db.UserRoles.AsNoTracking()
            .Where(link => link.UserId == userId)
            .Join(db.Roles.AsNoTracking(), link => link.RoleId, role => role.Id, (_, role) => role.Name!)
            .ToListAsync();

    private static ClaimsPrincipal Caller(string userId, int institutionId, params string[] roles)
        => new(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(WombatClaimTypes.InstitutionId, institutionId.ToString(CultureInfo.InvariantCulture)),
                .. roles.Select(role => new Claim(ClaimTypes.Role, role))
            ],
            "test"));

    private sealed class FixedAuditContext : IAuditContextProvider
    {
        public string? UserId => "caller";
        public string? UserDisplay => "Caller";
        public string? IpAddress => "10.0.0.0/24";
        public string? UserAgent => "Test/1.0";
        public int? InstitutionId => null;
        public void DeclareInstitution(int institutionId) { }
        public void DeclareActor(string userId, string display) { }
    }

    private static async Task<ServiceProvider> MigratedServicesAsync(string schema)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(TestDatabase.SchemaConnectionString(schema)));
        services.AddIdentity<WombatIdentityUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        var root = services.BuildServiceProvider();

        await using (var migrationScope = root.CreateAsyncScope())
        {
            await migrationScope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.MigrateAsync();
        }

        return root;
    }
}
