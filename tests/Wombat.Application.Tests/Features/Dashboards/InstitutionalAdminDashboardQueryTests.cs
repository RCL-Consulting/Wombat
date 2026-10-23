using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Dashboards.InstitutionalAdmin;
using Wombat.Application.Tests.Features.Users;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Dashboards;

/// <summary>
/// Her Home (T376, flow 12, Q7, Q8, K24; T291 items 4 and 8): every count her institution's, at the runbook's moments
/// 1.10, 1.21, 2.28 and 2.44 (boards H02, H03, H04, H01).
/// </summary>
public sealed class InstitutionalAdminDashboardQueryTests
{
    private const int Kgk = 1;
    private const int Other = 2;

    [Fact]
    public async Task At110_HerFirstSignIn_OneAccountHers_AndNothingAdopted()
    {
        await using var db = Catalogue();
        var users = new RecordingUserAdministrationService();
        users.Add(Person("mbatha", Kgk, WombatRoles.InstitutionalAdmin));
        users.Add(Person("elsewhere", Other, WombatRoles.InstitutionalAdmin));

        var home = await Handle(db, users);

        home.UsersByRole.Should().Equal(new RoleCountItem(WombatRoles.InstitutionalAdmin, 1));
        home.AccountCount.Should().Be(1);
        home.OnlyAccountIsYours.Should().BeTrue("1 account: yours.");
        home.WaitingCount.Should().Be(0);
        home.AdoptedSpecialityCount.Should().Be(0, "the College's catalogue is not hers (T291 item 4)");
        home.AdoptedSubSpecialityCount.Should().Be(0);
        home.Adoptions.Should().BeEmpty();
        home.Institution.Should().Be(new InstitutionCardItem(Kgk, "Kgosi Kgari Teaching Hospital", "KGK", "paeds-admin@kgk.wombat.local", true));
    }

    [Fact]
    public async Task At121_AdoptedBeforeOnboarding_OneAndOne_WithItsLine()
    {
        await using var db = Catalogue(adopted: true);
        var users = new RecordingUserAdministrationService();
        users.Add(Person("mbatha", Kgk, WombatRoles.InstitutionalAdmin));

        var home = await Handle(db, users);

        home.AdoptedSpecialityCount.Should().Be(1);
        home.AdoptedSubSpecialityCount.Should().Be(1);
        home.Adoptions.Should().Equal(new AdoptedCurriculumItem("Paediatrics", "Paediatrics", "CPSA", "Paediatric EPA Curriculum", "11.1"));
    }

    [Fact]
    public async Task At228_FivePendingTrainees_AreTheWaitingCount_AndAPeopleCount()
    {
        await using var db = Catalogue(adopted: true);
        var users = Staff();
        foreach (var n in Enumerable.Range(1, 5))
        {
            users.Add(Person($"pending-{n}", Kgk, WombatRoles.PendingTrainee));
        }

        var home = await Handle(db, users);

        home.WaitingCount.Should().Be(5);
        home.UsersByRole.Should().Contain(new RoleCountItem(WombatRoles.PendingTrainee, 5));
        home.AccountCount.Should().Be(15);
        home.OnlyAccountIsYours.Should().BeFalse();
    }

    [Fact]
    public async Task At244_EveryRoleHeld_InTheRolesOrder_AndTheDistinctAccountCount_WithNoRoleAccountsIncluded()
    {
        await using var db = Catalogue(adopted: true);
        var users = Staff();
        foreach (var n in Enumerable.Range(1, 5))
        {
            users.Add(Person($"trainee-{n}", Kgk, WombatRoles.Trainee));
        }

        users.Add(Person("duplessis", Kgk));
        users.Add(Person("elsewhere-assessor", Other, WombatRoles.Assessor));

        var home = await Handle(db, users);

        home.UsersByRole.Should().Equal(
            new RoleCountItem(WombatRoles.InstitutionalAdmin, 1),
            new RoleCountItem(WombatRoles.SpecialityAdmin, 1),
            new RoleCountItem(WombatRoles.SubSpecialityAdmin, 1),
            new RoleCountItem(WombatRoles.Coordinator, 1),
            new RoleCountItem(WombatRoles.CommitteeMember, 4),
            new RoleCountItem(WombatRoles.Assessor, 5),
            new RoleCountItem(WombatRoles.Trainee, 5));
        home.AccountCount.Should().Be(16, "fifteen with roles and du Plessis with none; the roles sum past it (Zulu holds two)");
        home.WaitingCount.Should().Be(0, "no line above the cards once everyone is admitted");
    }

    [Fact]
    public async Task ACallerWithNoInstitution_ReadsNothing()
    {
        await using var db = Catalogue(adopted: true);
        var users = Staff();

        var home = await new GetInstitutionalAdminDashboardSummaryQueryHandler(db, users).Handle(
            new GetInstitutionalAdminDashboardSummaryQuery(TestPrincipals.InRoles([WombatRoles.InstitutionalAdmin], "nobody", null)),
            CancellationToken.None);

        home.AccountCount.Should().Be(0);
        home.Institution.Should().BeNull();
        users.Lookups.Should().BeEmpty();
    }

    private static Task<InstitutionalAdminDashboardSummaryDto> Handle(ApplicationDbContext db, IUserAdministrationService users)
        => new GetInstitutionalAdminDashboardSummaryQueryHandler(db, users).Handle(
            new GetInstitutionalAdminDashboardSummaryQuery(TestPrincipals.InstitutionalAdmin(Kgk, userId: "mbatha")),
            CancellationToken.None);

    /// <summary>The ten of 2.12 after 2.13: Zulu Committee member and Assessor, four Committee members, five Assessors.</summary>
    private static RecordingUserAdministrationService Staff()
    {
        var users = new RecordingUserAdministrationService();
        users.Add(Person("mbatha", Kgk, WombatRoles.InstitutionalAdmin));
        users.Add(Person("mokoena", Kgk, WombatRoles.SpecialityAdmin));
        users.Add(Person("sithole", Kgk, WombatRoles.SubSpecialityAdmin));
        users.Add(Person("smit", Kgk, WombatRoles.Coordinator));
        users.Add(Person("zulu", Kgk, WombatRoles.CommitteeMember, WombatRoles.Assessor));
        users.Add(Person("botha", Kgk, WombatRoles.CommitteeMember, WombatRoles.Assessor));
        users.Add(Person("naidoo", Kgk, WombatRoles.CommitteeMember, WombatRoles.Assessor));
        users.Add(Person("vanrensburg", Kgk, WombatRoles.CommitteeMember));
        users.Add(Person("khumalo", Kgk, WombatRoles.Assessor));
        users.Add(Person("patel", Kgk, WombatRoles.Assessor));
        return users;
    }

    private static UserIdentityDetails Person(string id, int institutionId, params string[] roles)
        => new(id, $"{id}@kgk.wombat.local", id, id, institutionId, [], [], roles);

    /// <summary>Two specialities in the College's catalogue; with <paramref name="adopted" />, KGK holds one active adoption.</summary>
    private static ApplicationDbContext Catalogue(bool adopted = false)
    {
        var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
        db.Institutions.Add(new Institution { Id = Kgk, Name = "Kgosi Kgari Teaching Hospital", ShortCode = "KGK", ContactEmail = "paeds-admin@kgk.wombat.local" });
        db.Institutions.Add(new Institution { Id = Other, Name = "Elsewhere", ShortCode = "ELS" });
        db.Colleges.Add(new College { Id = 1, Name = "College of Paediatricians of South Africa", ShortCode = "CPSA" });
        db.Specialities.Add(new Speciality { Id = 1, CollegeId = 1, Name = "Paediatrics" });
        db.Specialities.Add(new Speciality { Id = 2, CollegeId = 1, Name = "Neonatology" });
        db.SubSpecialities.Add(new SubSpeciality { Id = 1, SpecialityId = 1, Name = "Paediatrics" });
        db.SubSpecialities.Add(new SubSpeciality { Id = 2, SpecialityId = 2, Name = "Neonatology" });
        db.Curricula.Add(new Curriculum { Id = 1, SubSpecialityId = 1, Name = "Paediatric EPA Curriculum", Version = "11.1" });
        db.Curricula.Add(new Curriculum { Id = 2, SubSpecialityId = 2, Name = "Neonatal Curriculum", Version = "1" });
        // Another institution's adoption, and an ended one of hers, never count.
        db.InstitutionCurriculumAdoptions.Add(new InstitutionCurriculumAdoption { Id = 1, InstitutionId = Other, CurriculumId = 2, SubSpecialityId = 2, IsActive = true });
        db.InstitutionCurriculumAdoptions.Add(new InstitutionCurriculumAdoption { Id = 2, InstitutionId = Kgk, CurriculumId = 2, SubSpecialityId = 2, IsActive = false });
        if (adopted)
        {
            db.InstitutionCurriculumAdoptions.Add(new InstitutionCurriculumAdoption { Id = 3, InstitutionId = Kgk, CurriculumId = 1, SubSpecialityId = 1, IsActive = true });
        }

        db.SaveChanges();
        return db;
    }
}
