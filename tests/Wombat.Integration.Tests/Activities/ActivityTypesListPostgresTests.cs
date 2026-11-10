using System.Globalization;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Queries.ListActivityTypesAdmin;
using Wombat.Domain.Activities;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;
using Wombat.Integration.Tests.TestSupport;

namespace Wombat.Integration.Tests.Activities;

/// <summary>
/// T398 (flow 17, lane B) on a real PostgreSQL server: the activity-type list's owner words and the College admin's
/// flags (Q7, N12), each of which must translate. The owner is the College's short code read through a speciality's and
/// a sub-speciality's College (a join the unit suite's InMemory evaluates in memory), the institution's name, or the
/// platform; a system-managed row names its release and is writable to nobody (Q9, T334); a College admin is told the
/// College's name and whether it holds any speciality.
/// </summary>
/// <remarks>Isolated as <c>ActivityTypeAdminScopePostgresTests</c> is: a migrated schema of its own, dropped after.</remarks>
public sealed class ActivityTypesListPostgresTests : IAsyncLifetime
{
    private readonly TestSchemas _schemas = new();

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _schemas.DropAllAsync();

    [Fact]
    public async Task TheListsOwnersAndFlags_OnPostgres()
    {
        try
        {
            var schema = await _schemas.CreateAsync();
            await using (var db = NewContext(schema))
            {
                await db.Database.MigrateAsync();
            }

            int kalafong, cpsa, surgeons;
            int globalType, kalafongType, cpsaType, cpsaSubType, msfType;
            await using (var db = NewContext(schema))
            {
                var kgk = new Institution { Name = "T398 Kalafong", ShortCode = "T398KGK", IsActive = true, CreatedOn = DateTime.UtcNow };
                var paediatricians = new College { Name = "T398 College of Paediatricians", ShortCode = "T398CPSA", IsActive = true };
                var noSpeciality = new College { Name = "T398 College of Surgeons", ShortCode = "T398COS", IsActive = true };
                var paeds = new Speciality { College = paediatricians, Name = "T398 Paediatrics", IsActive = true };
                var neonates = new SubSpeciality { Speciality = paeds, Name = "Neonatology", IsActive = true };
                db.AddRange(kgk, paediatricians, noSpeciality, paeds, neonates);
                await db.SaveChangesAsync();
                (kalafong, cpsa, surgeons) = (kgk.Id, paediatricians.Id, noSpeciality.Id);

                globalType = AddType(db, "t398_global", ActivityScope.Global, null);
                kalafongType = AddType(db, "t398_kgk", ActivityScope.Institution, kalafong);
                cpsaType = AddType(db, "t398_mini_cex_cpsa", ActivityScope.Speciality, paeds.Id);
                cpsaSubType = AddType(db, "t398_neonatal_cpsa", ActivityScope.SubSpeciality, neonates.Id);
                msfType = AddType(db, "msf_cpsa", ActivityScope.Speciality, paeds.Id, systemManaged: true);
            }

            await using (var db = NewContext(schema))
            {
                var administrator = await ListAsync(db, Principal([WombatRoles.Administrator], kalafong));
                var owners = administrator.Items
                    .Where(item => new[] { globalType, kalafongType, cpsaType, cpsaSubType, msfType }.Contains(item.Id))
                    .ToDictionary(item => item.Id, item => item.Owner);
                owners.Should().Equal(new Dictionary<int, ActivityTypeOwnerDto?>
                {
                    [globalType] = new(ActivityTypeOwnerKind.Platform, null),
                    [kalafongType] = new(ActivityTypeOwnerKind.Institution, "T398 Kalafong"),
                    [cpsaType] = new(ActivityTypeOwnerKind.College, "T398CPSA"),
                    [cpsaSubType] = new(ActivityTypeOwnerKind.College, "T398CPSA"),
                    [msfType] = new(ActivityTypeOwnerKind.College, "T398CPSA")
                });
                var msf = administrator.Items.Single(item => item.Id == msfType);
                msf.WrittenBy.Should().Be("the MSF release");
                msf.CanWrite.Should().BeFalse("T334: the Administrator included");
                administrator.CallerCollegeName.Should().BeNull();

                var college = await ListAsync(db, Principal([WombatRoles.CollegeAdmin], kalafong, cpsa));
                college.CallerCollegeName.Should().Be("T398 College of Paediatricians");
                college.CallerCollegeHasSpeciality.Should().BeTrue();

                var empty = await ListAsync(db, Principal([WombatRoles.CollegeAdmin], kalafong, surgeons));
                empty.CallerCollegeName.Should().Be("T398 College of Surgeons");
                empty.CallerCollegeHasSpeciality.Should().BeFalse("N12: a College with no speciality");
                empty.CanCreate.Should().BeFalse();
            }
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    private static Task<ActivityTypeAdminListDto> ListAsync(ApplicationDbContext db, ClaimsPrincipal principal)
        => new ListActivityTypesAdminQueryHandler(db).Handle(new ListActivityTypesAdminQuery(principal), CancellationToken.None);

    private static int AddType(ApplicationDbContext db, string key, ActivityScope scope, int? scopeId, bool systemManaged = false)
    {
        var type = new ActivityType
        {
            Key = key,
            Name = key,
            Scope = scope,
            ScopeId = scopeId,
            Version = 1,
            IsActive = true,
            SystemManaged = systemManaged,
            SchemaJson = "{}",
            WorkflowJson = "{}",
            CreditRulesJson = """{ "counts_for": [] }""",
            OwnerUserId = "system",
            CreatedOn = DateTime.UtcNow
        };
        db.ActivityTypes.Add(type);
        db.SaveChanges();
        return type.Id;
    }

    private static ClaimsPrincipal Principal(string[] roles, int institutionId, int? collegeId = null)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "t398-caller"),
            new(WombatClaimTypes.InstitutionId, institutionId.ToString(CultureInfo.InvariantCulture))
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        if (collegeId is int college)
        {
            claims.Add(new Claim(WombatClaimTypes.CollegeId, college.ToString(CultureInfo.InvariantCulture)));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }

    private static ApplicationDbContext NewContext(string schema)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(TestDatabase.SchemaConnectionString(schema))
            .Options);
}
