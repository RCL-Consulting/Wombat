using System.Globalization;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Assessors;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Identity;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Integration.Tests.Identity;

/// <summary>
/// T376 (flow 12, lane T, M1; Spec 2's test) on a real PostgreSQL server, through the real user store: saving assessor
/// training for a Committee member who is also an Assessor leaves her speciality and sub-speciality scope and her security
/// stamp as they were, so she keeps her committee scope and no session of hers ends. Until T376 the save passed the form's
/// scope to <c>UpdateScopeAsync</c>, which replaced the whole scope and changed the stamp.
/// </summary>
/// <remarks>Isolated as <see cref="UserAdministrationSelfAndTraineePostgresTests" /> is: a migrated schema of its own.</remarks>
public sealed class AssessorTrainingScopePostgresTests : IAsyncLifetime
{
    private const string Zulu = "zulu-a";
    private const string Admin = "inst-admin-a";

    private readonly TestSchemas _schemas = new();

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _schemas.DropAllAsync();

    [Fact]
    public async Task OnPostgres_SavingTraining_LeavesTheScopeAndTheSecurityStampUnchanged()
    {
        var schema = await _schemas.CreateAsync();

        try
        {
            await using var root = await MigratedServicesAsync(schema);

            int institutionId;
            int specialityId;
            int subSpecialityId;
            string stampBefore;
            await using (var arrange = root.CreateAsyncScope())
            {
                var db = arrange.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var institution = new Institution { Name = "A", ShortCode = "A", IsActive = true, CreatedOn = DateTime.UtcNow };
                var college = new College { Name = "College A", ShortCode = "CA" };
                db.Institutions.Add(institution);
                db.Colleges.Add(college);
                await db.SaveChangesAsync();
                var speciality = new Speciality { CollegeId = college.Id, Name = "Paediatrics" };
                db.Specialities.Add(speciality);
                await db.SaveChangesAsync();
                var subSpeciality = new SubSpeciality { SpecialityId = speciality.Id, Name = "Paediatrics" };
                db.SubSpecialities.Add(subSpeciality);
                await db.SaveChangesAsync();
                (institutionId, specialityId, subSpecialityId) = (institution.Id, speciality.Id, subSpeciality.Id);

                var zulu = NomineeSeed.AddUser(db, Zulu, institutionId, WombatRoles.CommitteeMember, WombatRoles.Assessor);
                zulu.SecurityStamp = "stamp-before";
                NomineeSeed.AddUser(db, Admin, institutionId, WombatRoles.InstitutionalAdmin);
                db.UserSpecialityScopes.Add(new WombatIdentityUserSpecialityScope { UserId = Zulu, SpecialityId = specialityId });
                db.UserSubSpecialityScopes.Add(new WombatIdentityUserSubSpecialityScope { UserId = Zulu, SubSpecialityId = subSpecialityId });
                await db.SaveChangesAsync();
                stampBefore = zulu.SecurityStamp;
            }

            await using (var request = root.CreateAsyncScope())
            {
                var db = request.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var users = new UserAdministrationService(request.ServiceProvider.GetRequiredService<UserManager<WombatIdentityUser>>(), db);

                var saved = await new CreateOrUpdateAssessorProfileCommandHandler(db, users).Handle(
                    new CreateOrUpdateAssessorProfileCommand(
                        Zulu, AssessorTrainingStatus.Trained, new DateOnly(2018, 10, 20), "MBChB", Caller(Admin, institutionId)),
                    CancellationToken.None);

                saved.InstitutionId.Should().Be(institutionId);
            }

            await using (var read = root.CreateAsyncScope())
            {
                var db = read.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                (await db.UserSpecialityScopes.AsNoTracking().Where(scope => scope.UserId == Zulu).Select(scope => scope.SpecialityId).ToListAsync())
                    .Should().Equal(specialityId);
                (await db.UserSubSpecialityScopes.AsNoTracking().Where(scope => scope.UserId == Zulu).Select(scope => scope.SubSpecialityId).ToListAsync())
                    .Should().Equal(subSpecialityId);
                (await db.Users.AsNoTracking().SingleAsync(user => user.Id == Zulu)).SecurityStamp
                    .Should().Be(stampBefore, "M1: no scope is written, so no session of hers ends");
                var profile = await db.Set<AssessorProfile>().AsNoTracking().SingleAsync();
                (profile.UserId, profile.InstitutionId, profile.TrainingStatus, profile.TrainingCompletedOn)
                    .Should().Be((Zulu, institutionId, AssessorTrainingStatus.Trained, (DateOnly?)new DateOnly(2018, 10, 20)));
            }
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    private static ClaimsPrincipal Caller(string userId, int institutionId)
        => new(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(WombatClaimTypes.InstitutionId, institutionId.ToString(CultureInfo.InvariantCulture)),
                new Claim(ClaimTypes.Role, WombatRoles.InstitutionalAdmin)
            ],
            "test"));

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
