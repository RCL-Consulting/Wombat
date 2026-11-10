using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Queries.ListActivityTypesAdmin;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Activities;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Activities;

/// <summary>
/// T398 (flow 17, lane B): what the activity-type list reads beyond T300's rule. Whose each type is (Q7: the College by its
/// short code, the institution by its name, the platform), a system-managed type's release with CanWrite false to
/// everyone (Q9, T334), and, for a College admin, the College's name and whether it holds any speciality (N12). The read
/// rule itself is unchanged (Q19): <c>ActivityTypeScopeGuardTests</c> holds it.
/// </summary>
public sealed class ListActivityTypesAdminQueryTests : IAsyncLifetime
{
    private const int Kgk = 1;
    private const int Cpsa = 10;
    private const int Surgeons = 11;
    private const int Paediatrics = 20;
    private const int Neonatology = 30;

    private ApplicationDbContext _db = null!;

    public async Task InitializeAsync()
    {
        _db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        _db.Institutions.Add(new Institution { Id = Kgk, Name = "Kgosi Kgari Teaching Hospital", ShortCode = "KGK", IsActive = true, CreatedOn = DateTime.UtcNow });
        _db.Colleges.AddRange(
            new College { Id = Cpsa, Name = "College of Paediatricians of South Africa", ShortCode = "CPSA" },
            new College { Id = Surgeons, Name = "College of Surgeons", ShortCode = "CoS" });
        _db.Specialities.Add(new Speciality { Id = Paediatrics, CollegeId = Cpsa, Name = "Paediatrics" });
        _db.SubSpecialities.Add(new SubSpeciality { Id = Neonatology, SpecialityId = Paediatrics, Name = "Neonatology" });

        _db.ActivityTypes.AddRange(
            Type("reflection", "Reflection", ActivityScope.Global, null),
            Type("kgk_teaching_log", "KGK Teaching Session Log", ActivityScope.Institution, Kgk),
            Type("mini_cex_cpsa", "Mini-CEX (Paediatrics)", ActivityScope.Speciality, Paediatrics),
            Type("neonatal_log", "Neonatal log", ActivityScope.SubSpeciality, Neonatology),
            Type("msf_cpsa", "Multi-Source Feedback (Paediatrics)", ActivityScope.Speciality, Paediatrics, systemManaged: true),
            Type("learner_feedback_cpsa", "Learner Feedback (Paediatrics)", ActivityScope.Speciality, Paediatrics, systemManaged: true),
            Type("future_release_cpsa", "Some future release", ActivityScope.Speciality, Paediatrics, systemManaged: true));
        await _db.SaveChangesAsync();
    }

    public Task DisposeAsync()
    {
        _db.Dispose();
        return Task.CompletedTask;
    }

    [Theory]
    [InlineData("reflection", ActivityTypeOwnerKind.Platform, null)]
    [InlineData("kgk_teaching_log", ActivityTypeOwnerKind.Institution, "Kgosi Kgari Teaching Hospital")]
    [InlineData("mini_cex_cpsa", ActivityTypeOwnerKind.College, "CPSA")]
    [InlineData("neonatal_log", ActivityTypeOwnerKind.College, "CPSA")]
    public async Task EachRow_SaysWhoseItIs(string key, ActivityTypeOwnerKind kind, string? name)
    {
        var list = await ListAsync(TestPrincipals.Administrator());

        list.Items.Single(item => item.Key == key).Owner.Should().Be(new ActivityTypeOwnerDto(kind, name));
    }

    [Fact]
    public async Task ATypeWhoseTargetNoLongerExists_HasAnOwnerKind_AndNoName()
    {
        _db.ActivityTypes.Add(Type("orphan", "Orphan", ActivityScope.Speciality, 99));
        await _db.SaveChangesAsync();

        var list = await ListAsync(TestPrincipals.Administrator());

        list.Items.Single(item => item.Key == "orphan").Owner.Should().Be(new ActivityTypeOwnerDto(ActivityTypeOwnerKind.College, null));
    }

    [Theory]
    [MemberData(nameof(EveryCaller))]
    public async Task ASystemManagedRow_NamesItsRelease_AndIsWritableToNobody(string caller)
    {
        var list = await ListAsync(PrincipalOf(caller));

        var msf = list.Items.Single(item => item.Key == "msf_cpsa");
        msf.SystemManaged.Should().BeTrue();
        msf.WrittenBy.Should().Be("the MSF release");
        msf.CanWrite.Should().BeFalse("T334: a release writes it, the Administrator included");

        list.Items.Single(item => item.Key == "learner_feedback_cpsa").WrittenBy.Should().Be("the learner-feedback release");
        list.Items.Single(item => item.Key == "future_release_cpsa").WrittenBy
            .Should().Be("a release of Wombat", "a key the table does not know reads as the guard names it (N10)");

        var ordinary = list.Items.Single(item => item.Key == "mini_cex_cpsa");
        ordinary.SystemManaged.Should().BeFalse();
        ordinary.WrittenBy.Should().BeNull();
    }

    [Fact]
    public async Task ACollegeAdmin_IsToldTheCollegesName_AndThatItHoldsASpeciality()
    {
        var list = await ListAsync(CollegeAdmin(Cpsa));

        list.CallerCollegeName.Should().Be("College of Paediatricians of South Africa");
        list.CallerCollegeHasSpeciality.Should().BeTrue();
        list.Items.Where(item => item.Key is "mini_cex_cpsa" or "neonatal_log").Should().OnlyContain(item => item.CanWrite);
    }

    /// <summary>N12 (R3-C08-list): a College with no speciality has no type for its admin to change, and none to create.</summary>
    [Fact]
    public async Task ACollegeAdminWhoseCollegeHoldsNoSpeciality_IsToldSo()
    {
        var list = await ListAsync(CollegeAdmin(Surgeons));

        list.CallerCollegeName.Should().Be("College of Surgeons");
        list.CallerCollegeHasSpeciality.Should().BeFalse();
        list.CanCreate.Should().BeFalse();
        list.Items.Should().NotBeEmpty("every published type is still read").And.OnlyContain(item => !item.CanWrite);
    }

    [Theory]
    [InlineData("Administrator")]
    [InlineData("InstitutionalAdmin")]
    public async Task AnyoneElse_IsToldNoCollege(string caller)
    {
        var list = await ListAsync(PrincipalOf(caller));

        list.CallerCollegeName.Should().BeNull();
        list.CallerCollegeHasSpeciality.Should().BeTrue();
    }

    [Fact]
    public async Task ACollegeAdminWhoseAccountNamesNoCollege_IsToldNoCollege()
    {
        var list = await ListAsync(TestPrincipals.InRoles([WombatRoles.CollegeAdmin], "college-admin-user", Kgk));

        list.CallerCollegeName.Should().BeNull();
        list.CallerCollegeHasSpeciality.Should().BeTrue();
    }

    [Fact]
    public async Task TheQuery_StillOrdersByName()
    {
        var list = await ListAsync(TestPrincipals.Administrator());

        list.Items.Select(item => item.Name).Should().BeInAscendingOrder(StringComparer.Ordinal);
    }

    public static TheoryData<string> EveryCaller() => new() { "Administrator", "InstitutionalAdmin", "CollegeAdmin" };

    private async Task<ActivityTypeAdminListDto> ListAsync(ClaimsPrincipal principal)
        => await new ListActivityTypesAdminQueryHandler(_db).Handle(new ListActivityTypesAdminQuery(principal), CancellationToken.None);

    private static ClaimsPrincipal PrincipalOf(string caller) => caller switch
    {
        "Administrator" => TestPrincipals.Administrator(),
        "InstitutionalAdmin" => TestPrincipals.InstitutionalAdmin(Kgk),
        "CollegeAdmin" => CollegeAdmin(Cpsa),
        _ => throw new ArgumentOutOfRangeException(nameof(caller), caller, null)
    };

    private static ClaimsPrincipal CollegeAdmin(int collegeId)
        => TestPrincipals.InRoles([WombatRoles.CollegeAdmin], "college-admin-user", Kgk).WithCollege(collegeId);

    private static ActivityType Type(string key, string name, ActivityScope scope, int? scopeId, bool systemManaged = false) => new()
    {
        Key = key,
        Name = name,
        Scope = scope,
        ScopeId = scopeId,
        IsActive = true,
        Version = 1,
        SystemManaged = systemManaged,
        SchemaJson = "{}",
        WorkflowJson = "{}",
        CreditRulesJson = "{}",
        OwnerUserId = "seeder",
        CreatedOn = DateTime.UtcNow
    };
}
