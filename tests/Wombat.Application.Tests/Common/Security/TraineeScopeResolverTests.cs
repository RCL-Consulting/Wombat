using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Security;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Common.Security;

/// <summary>
/// <see cref="TraineeScopeResolver" />: the one answer to "where does this trainee train, and may this caller read about
/// them?" (T113). Four copies of the first half existed; the MSF one broke ties by programme start date and the other
/// three by id.
/// </summary>
public sealed class TraineeScopeResolverTests
{
    private const int HostInstitution = 1;
    private const int OtherInstitution = 2;
    private const int PaediatricsSpeciality = 1;
    private const int GeneralPaediatrics = 11;
    private const int SurgerySpeciality = 2;
    private const int GeneralSurgery = 21;

    // ─── The tie-break ───────────────────────────────────────────────────────

    [Fact]
    public async Task TheActiveProfile_WinsOverANewerInactiveOne()
    {
        await using var db = CreateDb();
        AddProfile(db, id: 1, "trainee-1", HostInstitution, isActive: true, start: new DateOnly(2023, 1, 1));
        AddProfile(db, id: 2, "trainee-1", OtherInstitution, isActive: false, start: new DateOnly(2025, 1, 1));
        await db.SaveChangesAsync();

        (await TraineeScopeResolver.ResolveAsync(db, "trainee-1", CancellationToken.None))!
            .InstitutionId.Should().Be(HostInstitution);
    }

    [Fact]
    public async Task WhenNoProfileIsActive_TheHighestIdWins_NotTheLatestProgrammeStart()
    {
        // The case the MSF copy got wrong, and the only one the id decides: the database allows one ACTIVE profile per
        // trainee (a unique index filtered on IsActive), so two profiles tie only when neither is current. Profile 5
        // started later, profile 7 was created later. The activity scope stamp has always said profile 7, so a
        // campaign about this trainee must say profile 7 too, or the coordinator who may run the campaign is not the
        // coordinator who may read the evidence it records.
        await using var db = CreateDb();
        AddProfile(db, id: 5, "trainee-1", OtherInstitution, isActive: false, start: new DateOnly(2025, 1, 1));
        AddProfile(db, id: 7, "trainee-1", HostInstitution, isActive: false, start: new DateOnly(2020, 1, 1));
        await db.SaveChangesAsync();

        (await TraineeScopeResolver.ResolveAsync(db, "trainee-1", CancellationToken.None))!
            .InstitutionId.Should().Be(HostInstitution);
    }

    [Fact]
    public async Task PreferredProfiles_HoldsExactlyOneProfilePerTrainee_TheOneResolveReads()
    {
        // The set form is what a list filters through in SQL; it must agree with the single lookup row for row.
        await using var db = CreateDb();
        AddProfile(db, id: 1, "trainee-1", HostInstitution, isActive: false, start: new DateOnly(2022, 1, 1));
        AddProfile(db, id: 2, "trainee-1", OtherInstitution, isActive: true, start: new DateOnly(2023, 1, 1));
        AddProfile(db, id: 3, "trainee-1", HostInstitution, isActive: false, start: new DateOnly(2024, 1, 1));
        AddProfile(db, id: 4, "trainee-2", HostInstitution, isActive: false, start: new DateOnly(2024, 1, 1));
        AddProfile(db, id: 5, "trainee-2", OtherInstitution, isActive: false, start: new DateOnly(2021, 1, 1));
        await db.SaveChangesAsync();

        var preferred = await TraineeScopeResolver.PreferredProfiles(db)
            .OrderBy(profile => profile.UserId)
            .Select(profile => new { profile.UserId, profile.Id })
            .ToListAsync();

        preferred.Should().Equal(new { UserId = "trainee-1", Id = 2 }, new { UserId = "trainee-2", Id = 5 });
    }

    [Fact]
    public async Task AProfileWhoseCurriculumIsMissing_StillResolvesItsInstitution()
    {
        await using var db = CreateDb();
        AddProfile(db, id: 1, "trainee-1", HostInstitution, isActive: true, start: new DateOnly(2024, 1, 1), curriculumId: 999);
        await db.SaveChangesAsync();

        (await TraineeScopeResolver.ResolveAsync(db, "trainee-1", CancellationToken.None))
            .Should().Be(new TraineeScope(HostInstitution, null, null));
    }

    [Fact]
    public async Task ATraineeWithNoProfile_ResolvesToNothing()
    {
        await using var db = CreateDb();
        SeedTree(db);
        await db.SaveChangesAsync();

        (await TraineeScopeResolver.ResolveAsync(db, "nobody", CancellationToken.None)).Should().BeNull();
    }

    // ─── The set forms ───────────────────────────────────────────────────────

    [Fact]
    public async Task ResolveAll_AgreesWithResolve_TraineeByTrainee_AndHoldsOnlyTheInstitutionAskedFor()
    {
        // The committee scheduling picker reads the set form and its handler the single one (T182); if the two ever
        // disagreed about a trainee, the picker would offer someone the handler refuses, or hide someone it accepts.
        // The rows cover each way a scope degrades, and the tie-break.
        await using var db = CreateDb();
        SeedTree(db);
        db.SubSpecialities.Add(new SubSpeciality { Id = 12, SpecialityId = 404, Name = "Orphaned sub-speciality" });
        db.Curricula.Add(new Curriculum { Id = 101, SubSpecialityId = 12, Name = "Orphaned", Version = "1" });
        db.Curricula.Add(new Curriculum { Id = 102, SubSpecialityId = 505, Name = "Dangling", Version = "1" });
        AddProfile(db, id: 1, "whole", HostInstitution, isActive: true, start: new DateOnly(2024, 1, 1), curriculumId: 100);
        AddProfile(db, id: 2, "no-curriculum", HostInstitution, isActive: true, start: new DateOnly(2024, 1, 1), curriculumId: 999);
        AddProfile(db, id: 3, "no-speciality", HostInstitution, isActive: true, start: new DateOnly(2024, 1, 1), curriculumId: 101);
        AddProfile(db, id: 4, "no-sub-speciality", HostInstitution, isActive: true, start: new DateOnly(2024, 1, 1), curriculumId: 102);
        AddProfile(db, id: 5, "moved", HostInstitution, isActive: false, start: new DateOnly(2022, 1, 1));
        AddProfile(db, id: 6, "moved", OtherInstitution, isActive: true, start: new DateOnly(2021, 1, 1));
        AddProfile(db, id: 7, "elsewhere", OtherInstitution, isActive: true, start: new DateOnly(2024, 1, 1));
        await db.SaveChangesAsync();

        var everyone = await TraineeScopeResolver.ResolveAllAsync(db, null, CancellationToken.None);
        everyone.Keys.Should().BeEquivalentTo("whole", "no-curriculum", "no-speciality", "no-sub-speciality", "moved", "elsewhere");
        foreach (var (userId, scope) in everyone)
        {
            scope.Should().Be(await TraineeScopeResolver.ResolveAsync(db, userId, CancellationToken.None), userId);
        }

        var host = await TraineeScopeResolver.ResolveAllAsync(db, HostInstitution, CancellationToken.None);
        host.Keys.Should().BeEquivalentTo(["whole", "no-curriculum", "no-speciality", "no-sub-speciality"],
            "a trainee is at the institution of their preferred profile only, so the one who moved is not here");
        host["whole"].Should().Be(new TraineeScope(HostInstitution, PaediatricsSpeciality, GeneralPaediatrics));
        host["no-curriculum"].Should().Be(new TraineeScope(HostInstitution, null, null));
    }

    [Fact]
    public async Task ResolveMany_GivesEachTraineeTheirOwnScope_AndDegradesEachLevelForThoseWhoNameIt()
    {
        // The list form (T183): one call over several trainees, each answered as ResolveAsync answers them alone. A
        // missing curriculum or sub-speciality nulls that level for the trainee who names it, and nobody else.
        await using var db = CreateDb();
        SeedTree(db);
        db.Curricula.Add(new Curriculum { Id = 101, SubSpecialityId = 999, Name = "Orphaned", Version = "1" });
        AddProfile(db, id: 1, "trainee-1", HostInstitution, isActive: true, start: new DateOnly(2024, 1, 1), curriculumId: 100);
        AddProfile(db, id: 2, "trainee-2", OtherInstitution, isActive: true, start: new DateOnly(2024, 1, 1), curriculumId: 999);
        AddProfile(db, id: 3, "trainee-3", OtherInstitution, isActive: true, start: new DateOnly(2024, 1, 1), curriculumId: 101);
        AddProfile(db, id: 4, "trainee-4", HostInstitution, isActive: false, start: new DateOnly(2025, 1, 1), curriculumId: 100);
        AddProfile(db, id: 5, "trainee-4", OtherInstitution, isActive: false, start: new DateOnly(2020, 1, 1), curriculumId: 100);
        await db.SaveChangesAsync();

        var scopes = await TraineeScopeResolver.ResolveManyAsync(
            db, ["trainee-1", "trainee-2", "trainee-3", "trainee-4", "nobody", "trainee-1", " "], CancellationToken.None);

        scopes.Should().BeEquivalentTo(new Dictionary<string, TraineeScope>
        {
            ["trainee-1"] = new(HostInstitution, PaediatricsSpeciality, GeneralPaediatrics),
            ["trainee-2"] = new(OtherInstitution, null, null),
            ["trainee-3"] = new(OtherInstitution, null, 999),
            ["trainee-4"] = new(OtherInstitution, PaediatricsSpeciality, GeneralPaediatrics)
        });

        foreach (var (userId, scope) in scopes)
        {
            (await TraineeScopeResolver.ResolveAsync(db, userId, CancellationToken.None)).Should().Be(scope, userId);
        }

        (await TraineeScopeResolver.ResolveManyAsync(db, [], CancellationToken.None)).Should().BeEmpty();
    }

    // ─── The read ladder ─────────────────────────────────────────────────────

    public static TheoryData<string, bool> Callers => new()
    {
        { "the trainee themselves", true },
        { "a global Administrator", true },
        { "a Coordinator at the trainee's institution", true },
        { "an InstitutionalAdmin of the trainee's institution", true },
        { "a CommitteeMember at the trainee's institution", true },
        { "a SpecialityAdmin of the trainee's speciality at their institution", true },
        { "a SubSpecialityAdmin of the trainee's sub-speciality at their institution", true },
        { "a Coordinator at another institution", false },
        { "an InstitutionalAdmin of another institution", false },
        { "a SpecialityAdmin of the trainee's speciality at another institution", false },
        { "a SpecialityAdmin of another speciality at the trainee's institution", false },
        { "a classmate: another Trainee at the same institution", false },
        { "an Assessor at the same institution", false },
        { "a principal with no claims at all", false },
        // T185: the trainee rung comes first, as on the committee review. A Trainee holding an oversight role beside it
        // reads their own record and nobody else's.
        { "a classmate who also sits on the committee at the trainee's institution", false },
        { "a classmate who also administers the trainee's institution", false },
        { "a classmate who is also a global Administrator", false }
    };

    [Theory]
    [MemberData(nameof(Callers))]
    public async Task MayRead_IsT101sLadder(string caller, bool expected)
    {
        await using var db = CreateDb();
        SeedTree(db);
        AddProfile(db, id: 1, "trainee-1", HostInstitution, isActive: true, start: new DateOnly(2024, 1, 1), curriculumId: 100);
        await db.SaveChangesAsync();

        (await TraineeScopeResolver.MayReadAsync(db, Caller(caller), "trainee-1", CancellationToken.None))
            .Should().Be(expected, caller);
    }

    [Fact]
    public async Task ATraineeWhoAlsoHoldsAnOversightRole_StillReadsTheirOwnRecord()
    {
        // The trainee rung that refuses them their classmates' records must not refuse them their own. (T185)
        await using var db = CreateDb();
        SeedTree(db);
        AddProfile(db, id: 1, "trainee-2", HostInstitution, isActive: true, start: new DateOnly(2024, 1, 1), curriculumId: 100);
        await db.SaveChangesAsync();

        foreach (var caller in new[]
                 {
                     "a classmate who also sits on the committee at the trainee's institution",
                     "a classmate who also administers the trainee's institution",
                     "a classmate who is also a global Administrator"
                 })
        {
            (await TraineeScopeResolver.MayReadAsync(db, Caller(caller), "trainee-2", CancellationToken.None))
                .Should().BeTrue(caller);
        }
    }

    [Fact]
    public async Task ATraineeWithNoProfile_IsReadableOnlyByThemselvesAndAnAdministrator()
    {
        await using var db = CreateDb();
        SeedTree(db);
        await db.SaveChangesAsync();

        (await TraineeScopeResolver.MayReadAsync(db, TestPrincipals.Trainee("trainee-1", HostInstitution), "trainee-1", CancellationToken.None))
            .Should().BeTrue();
        (await TraineeScopeResolver.MayReadAsync(db, TestPrincipals.Administrator(), "trainee-1", CancellationToken.None))
            .Should().BeTrue();
        (await TraineeScopeResolver.MayReadAsync(db, TestPrincipals.Coordinator(HostInstitution), "trainee-1", CancellationToken.None))
            .Should().BeFalse("no profile, no organisational home, so nobody oversees them");
    }

    [Fact]
    public async Task MayRead_FollowsTheTieBreak_ForATraineeWithTwoPastProfiles()
    {
        await using var db = CreateDb();
        AddProfile(db, id: 1, "trainee-1", HostInstitution, isActive: false, start: new DateOnly(2025, 1, 1));
        AddProfile(db, id: 2, "trainee-1", OtherInstitution, isActive: false, start: new DateOnly(2024, 1, 1));
        await db.SaveChangesAsync();

        (await TraineeScopeResolver.MayReadAsync(db, TestPrincipals.Coordinator(OtherInstitution), "trainee-1", CancellationToken.None))
            .Should().BeTrue();
        (await TraineeScopeResolver.MayReadAsync(db, TestPrincipals.Coordinator(HostInstitution), "trainee-1", CancellationToken.None))
            .Should().BeFalse();
    }

    // ─── Administration, the admin half of oversight ─────────────────────────

    public static TheoryData<string, bool, bool> OversightAndAdministration => new()
    {
        // caller, oversees, administers
        { "an InstitutionalAdmin of the trainee's institution", true, true },
        { "a SpecialityAdmin of the trainee's speciality at their institution", true, true },
        { "a SubSpecialityAdmin of the trainee's sub-speciality at their institution", true, true },
        { "a Coordinator at the trainee's institution", true, false },
        { "a CommitteeMember at the trainee's institution", true, false },
        // Users hold several roles: the oversight role reads, and does not lend its institution-wide reach to the
        // admin role beside it.
        { "a SpecialityAdmin of another speciality at the trainee's institution who also sits on its committee", true, false },
        { "a SubSpecialityAdmin of another sub-speciality at the trainee's institution who also coordinates there", true, false },
        { "a SpecialityAdmin of another speciality at the trainee's institution", false, false },
        { "an InstitutionalAdmin of another institution", false, false },
        { "a SpecialityAdmin of the trainee's speciality at another institution", false, false },
        { "a classmate: another Trainee at the same institution", false, false }
    };

    [Theory]
    [MemberData(nameof(OversightAndAdministration))]
    public void IsAdministeredBy_IsTheAdminHalfOfIsOverseenBy_EachRoleAtItsOwnLevel(string caller, bool oversees, bool administers)
    {
        // What only an administrator may do about a trainee (revoke their entrustment, T183) asks for the admin role and
        // its scope together; "holds an admin role" and IsOverseenBy asked separately let a committee seat stand in for
        // the scope.
        var scope = new TraineeScope(HostInstitution, PaediatricsSpeciality, GeneralPaediatrics);

        TraineeScopeResolver.IsOverseenBy(scope, Caller(caller)).Should().Be(oversees, caller);
        TraineeScopeResolver.IsAdministeredBy(scope, Caller(caller)).Should().Be(administers, caller);
    }

    private static ClaimsPrincipal Caller(string caller) => caller switch
    {
        "the trainee themselves" => TestPrincipals.Trainee("trainee-1", HostInstitution),
        "a global Administrator" => TestPrincipals.Administrator(),
        "a Coordinator at the trainee's institution" => TestPrincipals.Coordinator(HostInstitution),
        "an InstitutionalAdmin of the trainee's institution" => TestPrincipals.InstitutionalAdmin(HostInstitution),
        "a CommitteeMember at the trainee's institution" =>
            TestPrincipals.InRole(WombatRoles.CommitteeMember, "member-1", HostInstitution),
        "a SpecialityAdmin of the trainee's speciality at their institution" =>
            TestPrincipals.InRole(WombatRoles.SpecialityAdmin, "spec-1", HostInstitution, specialityId: PaediatricsSpeciality),
        "a SubSpecialityAdmin of the trainee's sub-speciality at their institution" =>
            TestPrincipals.InRole(WombatRoles.SubSpecialityAdmin, "sub-1", HostInstitution, subSpecialityId: GeneralPaediatrics),
        "a Coordinator at another institution" => TestPrincipals.Coordinator(OtherInstitution),
        "an InstitutionalAdmin of another institution" => TestPrincipals.InstitutionalAdmin(OtherInstitution),
        "a SpecialityAdmin of the trainee's speciality at another institution" =>
            TestPrincipals.InRole(WombatRoles.SpecialityAdmin, "spec-2", OtherInstitution, specialityId: PaediatricsSpeciality),
        "a SpecialityAdmin of another speciality at the trainee's institution" =>
            TestPrincipals.InRole(WombatRoles.SpecialityAdmin, "spec-3", HostInstitution, specialityId: SurgerySpeciality),
        "a SpecialityAdmin of another speciality at the trainee's institution who also sits on its committee" =>
            TestPrincipals.InRoles(
                [WombatRoles.SpecialityAdmin, WombatRoles.CommitteeMember], "spec-4", HostInstitution, specialityId: SurgerySpeciality),
        "a SubSpecialityAdmin of another sub-speciality at the trainee's institution who also coordinates there" =>
            TestPrincipals.InRoles(
                [WombatRoles.SubSpecialityAdmin, WombatRoles.Coordinator], "sub-2", HostInstitution, subSpecialityId: GeneralSurgery),
        "a classmate: another Trainee at the same institution" => TestPrincipals.Trainee("trainee-2", HostInstitution),
        "a classmate who also sits on the committee at the trainee's institution" =>
            TestPrincipals.InRoles([WombatRoles.Trainee, WombatRoles.CommitteeMember], "trainee-2", HostInstitution),
        "a classmate who also administers the trainee's institution" =>
            TestPrincipals.InRoles([WombatRoles.Trainee, WombatRoles.InstitutionalAdmin], "trainee-2", HostInstitution),
        "a classmate who is also a global Administrator" =>
            TestPrincipals.InRoles([WombatRoles.Trainee, WombatRoles.Administrator], "trainee-2", HostInstitution),
        "an Assessor at the same institution" => TestPrincipals.InRole(WombatRoles.Assessor, "assessor-1", HostInstitution),
        "a principal with no claims at all" => TestPrincipals.Anonymous(),
        _ => throw new ArgumentOutOfRangeException(nameof(caller), caller, null)
    };

    private static void SeedTree(ApplicationDbContext db)
    {
        db.Specialities.Add(new Speciality { Id = PaediatricsSpeciality, CollegeId = 1, Name = "Paediatrics" });
        db.Specialities.Add(new Speciality { Id = SurgerySpeciality, CollegeId = 1, Name = "Surgery" });
        db.SubSpecialities.Add(new SubSpeciality { Id = GeneralPaediatrics, SpecialityId = PaediatricsSpeciality, Name = "General Paediatrics" });
        db.Curricula.Add(new Curriculum { Id = 100, SubSpecialityId = GeneralPaediatrics, Name = "CPSA Paediatrics", Version = "11.1" });
    }

    private static void AddProfile(
        ApplicationDbContext db,
        int id,
        string userId,
        int institutionId,
        bool isActive,
        DateOnly start,
        int curriculumId = 100)
        => db.Set<TraineeProfile>().Add(new TraineeProfile
        {
            Id = id,
            UserId = userId,
            InstitutionId = institutionId,
            CurriculumId = curriculumId,
            ProgrammeStartDate = start,
            ExpectedCompletionDate = start.AddYears(4),
            IsActive = isActive
        });

    private static ApplicationDbContext CreateDb()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
}
