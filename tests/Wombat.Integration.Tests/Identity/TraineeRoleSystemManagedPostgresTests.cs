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
using Wombat.Application.Features.Trainees;
using Wombat.Application.Features.Users;
using Wombat.Application.Features.Users.Commands.AddRoleToUser;
using Wombat.Application.Features.Users.Commands.RemoveRoleFromUser;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Audit;
using Wombat.Infrastructure.Identity;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Integration.Tests.Identity;

/// <summary>
/// T303 on a real PostgreSQL server, through the real user store (<see cref="UserAdministrationService" />) and the real
/// audit pipeline: neither an Administrator nor an InstitutionalAdmin adds Trainee to a pending registrar or a graduate,
/// or removes it from a trainee whose programme is running. Each refusal leaves its failure row and changes no role.
/// </summary>
/// <remarks>
/// <para>
/// The audit trap, as the store has it: <c>UserManager</c> saves each change at once, on the request's context, the one
/// the pipeline's catch writes the failure row through. A refusal that came after the store's write would leave the role
/// changed under a row that says the command failed. The refusal comes before the user is looked up, and the fresh read
/// after the pipeline's saves proves it.
/// </para>
/// <para>
/// Isolated as <c>UserAdministrationSelfAndTraineePostgresTests</c> is: a migrated schema of its own, dropped in a finally
/// and again on dispose.
/// </para>
/// </remarks>
public sealed class TraineeRoleSystemManagedPostgresTests : IAsyncLifetime
{
    private const string PendingRegistrar = "molefe";
    private const string Graduate = "graduate";
    private const string RunningTrainee = "trainee";
    private const string InstitutionalAdmin = "inst-admin-a";
    private const string GlobalAdmin = "global-admin";

    private readonly TestSchemas _schemas = new();

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _schemas.DropAllAsync();

    [Fact]
    public async Task OnPostgres_NeitherAdministratorAddsOrRemovesTrainee_EachRefusalLeavesItsFailureRow_AndNoRoleChanges()
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

                NomineeSeed.AddUser(db, PendingRegistrar, institutionId, WombatRoles.PendingTrainee);
                NomineeSeed.AddUser(db, Graduate, institutionId);
                NomineeSeed.AddUser(db, RunningTrainee, institutionId, WombatRoles.Trainee);
                NomineeSeed.AddUser(db, InstitutionalAdmin, institutionId, WombatRoles.InstitutionalAdmin);
                NomineeSeed.AddUser(db, GlobalAdmin, institutionId: null, WombatRoles.Administrator);

                // An assessor, so the Assessor role the control adds exists, as it does on a seeded database.
                NomineeSeed.AddUser(db, "assessor-a", institutionId, WombatRoles.Assessor);
                await db.SaveChangesAsync();
            }

            var callers = new[]
            {
                Caller(GlobalAdmin, institutionId: null, WombatRoles.Administrator),
                Caller(InstitutionalAdmin, institutionId, WombatRoles.InstitutionalAdmin)
            };

            foreach (var caller in callers)
            {
                (await AddRoleAsync(root, new AddRoleToUserCommand(PendingRegistrar, WombatRoles.Trainee, caller)))
                    .Should().Be(UserAdministrationRules.TraineeRoleByAdmissionOnly);
                (await AddRoleAsync(root, new AddRoleToUserCommand(Graduate, WombatRoles.Trainee, caller)))
                    .Should().Be(UserAdministrationRules.TraineeRoleByAdmissionOnly);
                (await RemoveRoleAsync(root, new RemoveRoleFromUserCommand(RunningTrainee, WombatRoles.Trainee, caller)))
                    .Should().Be(UserAdministrationRules.TraineeRoleByAdmissionOnly);
            }

            await using (var read = root.CreateAsyncScope())
            {
                var db = read.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                (await RolesOf(db, PendingRegistrar)).Should().BeEquivalentTo([WombatRoles.PendingTrainee]);
                (await RolesOf(db, Graduate)).Should().BeEmpty();
                (await RolesOf(db, RunningTrainee)).Should().BeEquivalentTo([WombatRoles.Trainee]);

                var rows = await db.AuditEntries.AsNoTracking().OrderBy(entry => entry.OccurredAt).ToListAsync();
                rows.Should().HaveCount(6).And.OnlyContain(entry => !entry.Success, "each refusal leaves its failure row");
                rows.Should().OnlyContain(entry => entry.ErrorMessage == UserAdministrationRules.TraineeRoleByAdmissionOnly);
            }

            // The control: the same InstitutionalAdmin adds a role the surface manages to the same pending registrar,
            // through the same real store and pipeline.
            (await AddRoleAsync(root, new AddRoleToUserCommand(PendingRegistrar, WombatRoles.Assessor, callers[1]))).Should().BeNull();

            await using (var read = root.CreateAsyncScope())
            {
                var db = read.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                (await RolesOf(db, PendingRegistrar)).Should().BeEquivalentTo([WombatRoles.PendingTrainee, WombatRoles.Assessor]);
                (await db.AuditEntries.AsNoTracking().CountAsync(entry => entry.Success)).Should().Be(1);
            }
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    /// <summary>
    /// T303 review: admission refuses a registrar who already holds Trainee beside PendingTrainee (the state an SSO group
    /// mapping to Trainee still makes) before anything is written. Until then it saved the profile and the scope, took
    /// PendingTrainee away, and then failed on "User already in role 'Trainee'": the admission committed under a failure
    /// row. The control admits a plain pending registrar at the same institution into the same curriculum.
    /// </summary>
    [Fact]
    public async Task OnPostgres_AdmissionRefusesARegistrarWhoAlreadyHoldsTrainee_BeforeAnyWrite_AndTheControlIsAdmitted()
    {
        const string MappedRegistrar = "sso-mapped";
        var schema = await _schemas.CreateAsync();

        try
        {
            await using var root = await MigratedServicesAsync(schema);

            int institutionId, curriculumId, adoptionId;
            await using (var arrange = root.CreateAsyncScope())
            {
                var db = arrange.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var institution = new Institution { Name = "A", ShortCode = "A", IsActive = true, CreatedOn = DateTime.UtcNow };
                var college = new College { Name = "CMSA", ShortCode = "CMSA", IsActive = true };
                var paediatrics = new Speciality { College = college, Name = "Paediatrics", IsActive = true };
                var curriculum = new Curriculum
                {
                    SubSpeciality = new SubSpeciality { Speciality = paediatrics, Name = "General Paediatrics", IsActive = true },
                    Name = "General Paediatrics",
                    Version = "1",
                    EffectiveFrom = new DateOnly(2024, 1, 1)
                };
                db.AddRange(institution, college, paediatrics, curriculum);
                await db.SaveChangesAsync();

                var adoption = new InstitutionCurriculumAdoption
                {
                    InstitutionId = institution.Id,
                    CurriculumId = curriculum.Id,
                    SubSpecialityId = curriculum.SubSpecialityId,
                    AdoptedOn = new DateOnly(2024, 1, 1),
                    IsActive = true
                };
                db.Add(adoption);

                NomineeSeed.AddUser(db, MappedRegistrar, institution.Id, WombatRoles.PendingTrainee, WombatRoles.Trainee);
                NomineeSeed.AddUser(db, PendingRegistrar, institution.Id, WombatRoles.PendingTrainee);
                await db.SaveChangesAsync();

                (institutionId, curriculumId, adoptionId) = (institution.Id, curriculum.Id, adoption.Id);
            }

            var callers = new[]
            {
                Caller(GlobalAdmin, institutionId: null, WombatRoles.Administrator),
                Caller(InstitutionalAdmin, institutionId, WombatRoles.InstitutionalAdmin)
            };

            foreach (var caller in callers)
            {
                (await AdmitAsync(root, new AdmitTraineeCommand(MappedRegistrar, curriculumId, new DateOnly(2026, 1, 15), null, caller)))
                    .Should().Be(AdmitTraineeCommandHandler.AlreadyHoldsTrainee);
            }

            await using (var read = root.CreateAsyncScope())
            {
                var db = read.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                (await RolesOf(db, MappedRegistrar)).Should().BeEquivalentTo([WombatRoles.PendingTrainee, WombatRoles.Trainee]);
                (await db.TraineeProfiles.AsNoTracking().AnyAsync()).Should().BeFalse("no profile is saved before the refusal");
                (await db.UserSpecialityScopes.AsNoTracking().AnyAsync(scope => scope.UserId == MappedRegistrar)).Should().BeFalse();
                (await db.UserSubSpecialityScopes.AsNoTracking().AnyAsync(scope => scope.UserId == MappedRegistrar)).Should().BeFalse();

                var rows = await db.AuditEntries.AsNoTracking().ToListAsync();
                rows.Should().HaveCount(2).And.OnlyContain(entry => !entry.Success, "each refusal leaves its failure row");
                rows.Should().OnlyContain(entry => entry.ErrorMessage == AdmitTraineeCommandHandler.AlreadyHoldsTrainee);
            }

            // The control: the same InstitutionalAdmin admits a plain pending registrar into the same curriculum, through
            // the same real store and pipeline.
            (await AdmitAsync(root, new AdmitTraineeCommand(PendingRegistrar, curriculumId, new DateOnly(2026, 1, 15), null, callers[1])))
                .Should().BeNull();

            await using (var read = root.CreateAsyncScope())
            {
                var db = read.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                (await RolesOf(db, PendingRegistrar)).Should().BeEquivalentTo([WombatRoles.Trainee]);
                var profile = await db.TraineeProfiles.AsNoTracking().SingleAsync();
                profile.UserId.Should().Be(PendingRegistrar);
                profile.AdoptionId.Should().Be(adoptionId);
                (await db.AuditEntries.AsNoTracking().CountAsync(entry => entry.Success)).Should().Be(1);
            }
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    // ─── The commands, as a request runs them ────────────────────────────────

    private static Task<string?> AddRoleAsync(ServiceProvider root, AddRoleToUserCommand command)
        => RunAsync(root, command, (users, request) => new AddRoleToUserCommandHandler(users).Handle(request, CancellationToken.None));

    private static Task<string?> RemoveRoleAsync(ServiceProvider root, RemoveRoleFromUserCommand command)
        => RunAsync(root, command, (users, request) => new RemoveRoleFromUserCommandHandler(users).Handle(request, CancellationToken.None));

    /// <summary>
    /// Runs one admission as <see cref="RunAsync{TCommand}" /> runs a role command: its own request scope, the real audit
    /// pipeline, and the real store on the request's context. Returns the refusal's words, or null when it was done.
    /// </summary>
    private static async Task<string?> AdmitAsync(ServiceProvider root, AdmitTraineeCommand command)
    {
        await using var request = root.CreateAsyncScope();
        var db = request.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var users = new UserAdministrationService(request.ServiceProvider.GetRequiredService<UserManager<WombatIdentityUser>>(), db);

        try
        {
            await new AuditPipelineBehavior<AdmitTraineeCommand, TraineeProfileDto>(new AuditWriter(db), new FixedAuditContext()).Handle(
                command,
                () => new AdmitTraineeCommandHandler(db, users).Handle(command, CancellationToken.None),
                CancellationToken.None);
            return null;
        }
        catch (InvalidOperationException refusal)
        {
            return refusal.Message;
        }
    }

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
        catch (InvalidOperationException refusal)
        {
            return refusal.Message;
        }
    }

    private static async Task<IReadOnlyList<string>> RolesOf(ApplicationDbContext db, string userId)
        => await db.UserRoles.AsNoTracking()
            .Where(link => link.UserId == userId)
            .Join(db.Roles.AsNoTracking(), link => link.RoleId, role => role.Id, (_, role) => role.Name!)
            .ToListAsync();

    /// <summary>The caller in <paramref name="roles" />; a global Administrator carries no institution.</summary>
    private static ClaimsPrincipal Caller(string userId, int? institutionId, params string[] roles)
    {
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId) };
        if (institutionId is { } id)
        {
            claims.Add(new Claim(WombatClaimTypes.InstitutionId, id.ToString(CultureInfo.InvariantCulture)));
        }

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }

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
