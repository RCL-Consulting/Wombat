using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Programme;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Activities;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Programme;

/// <summary>
/// T358 (flow 06, lane A0): the one scope a programme page reads by. It is the scope of the role the page reads as (E4),
/// never the union of the roles held: the institution for a Committee member or a Coordinator, the institution and the
/// speciality's sub-specialities for a Speciality admin, the institution and the sub-specialities for a Sub-speciality
/// admin. Null for someone in the programme as a trainee (T185), for a role not held or not one of the four, and for a
/// caller with no institution.
/// </summary>
public sealed class ProgrammeScopeTests
{
    private const int Kgk = 10;
    private const int OtherHospital = 11;
    private const int Paediatrics = 100;
    private const int Surgery = 101;
    private const int GeneralPaediatrics = 1000;
    private const int Neonatology = 1001;
    private const int GeneralSurgery = 1010;

    [Theory]
    [InlineData(WombatRoles.CommitteeMember)]
    [InlineData(WombatRoles.Coordinator)]
    public async Task ACommitteeMemberOrACoordinator_ReadsTheInstitution_NamedForIt(string role)
    {
        await using var db = Seeded();

        var scope = await ProgrammeScope.ResolveAsync(
            db, TestPrincipals.InRole(role, "staff-1", Kgk), role, CancellationToken.None);

        scope.Should().NotBeNull();
        scope!.ActingRole.Should().Be(role);
        scope.Kind.Should().Be(ProgrammeScopeKind.Institution);
        scope.Name.Should().Be("Kgosi Kgari Teaching Hospital");
        scope.InstitutionId.Should().Be(Kgk);
        scope.SpecialityIds.Should().BeEmpty();
        scope.SubSpecialityIds.Should().BeEmpty();
    }

    [Fact]
    public async Task ASpecialityAdmin_ReadsTheInstitutionAndTheSpecialitysSubSpecialities_NamedForTheSpeciality()
    {
        await using var db = Seeded();

        var scope = await ProgrammeScope.ResolveAsync(
            db,
            TestPrincipals.InRole(WombatRoles.SpecialityAdmin, "mokoena", Kgk, specialityId: Paediatrics),
            WombatRoles.SpecialityAdmin,
            CancellationToken.None);

        scope.Should().NotBeNull();
        scope!.Kind.Should().Be(ProgrammeScopeKind.Speciality);
        scope.Name.Should().Be("Paediatrics");
        scope.InstitutionId.Should().Be(Kgk);
        scope.SpecialityIds.Should().Equal(Paediatrics);
        scope.SubSpecialityIds.Should().BeEquivalentTo([GeneralPaediatrics, Neonatology]);
    }

    [Fact]
    public async Task ASubSpecialityAdmin_ReadsTheInstitutionAndTheSubSpeciality_NamedForIt()
    {
        await using var db = Seeded();

        var scope = await ProgrammeScope.ResolveAsync(
            db,
            TestPrincipals.InRole(WombatRoles.SubSpecialityAdmin, "sithole", Kgk, subSpecialityId: GeneralPaediatrics),
            WombatRoles.SubSpecialityAdmin,
            CancellationToken.None);

        scope.Should().NotBeNull();
        scope!.Kind.Should().Be(ProgrammeScopeKind.SubSpeciality);
        scope.Name.Should().Be("General Paediatrics");
        scope.InstitutionId.Should().Be(Kgk);
        scope.SpecialityIds.Should().BeEmpty();
        scope.SubSpecialityIds.Should().Equal(GeneralPaediatrics);
    }

    [Fact]
    public async Task SeveralSpecialities_AreNamedTogether_InNameOrder()
    {
        await using var db = Seeded();
        var principal = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity(
            [
                new(System.Security.Claims.ClaimTypes.NameIdentifier, "two-hats"),
                new(System.Security.Claims.ClaimTypes.Role, WombatRoles.SpecialityAdmin),
                new(Wombat.Application.Common.Security.WombatClaimTypes.InstitutionId, Kgk.ToString()),
                new(Wombat.Application.Common.Security.WombatClaimTypes.SpecialityId, Surgery.ToString()),
                new(Wombat.Application.Common.Security.WombatClaimTypes.SpecialityId, Paediatrics.ToString()),
            ],
            "test"));

        var scope = await ProgrammeScope.ResolveAsync(db, principal, WombatRoles.SpecialityAdmin, CancellationToken.None);

        scope!.Name.Should().Be("Paediatrics, Surgery");
        scope.SubSpecialityIds.Should().BeEquivalentTo([GeneralPaediatrics, Neonatology, GeneralSurgery]);
    }

    /// <summary>E4: the role read as decides the scope, whichever other role is held beside it.</summary>
    [Fact]
    public async Task ASpecialityAdminWhoAlsoSitsOnTheCommittee_ReadsTheSpecialityAsSpecialityAdmin_AndTheInstitutionAsCommitteeMember()
    {
        await using var db = Seeded();
        var principal = TestPrincipals.InRoles(
            [WombatRoles.SpecialityAdmin, WombatRoles.CommitteeMember], "mokoena", Kgk, specialityId: Paediatrics);

        var asAdmin = await ProgrammeScope.ResolveAsync(db, principal, WombatRoles.SpecialityAdmin, CancellationToken.None);
        var asMember = await ProgrammeScope.ResolveAsync(db, principal, WombatRoles.CommitteeMember, CancellationToken.None);

        asAdmin!.Kind.Should().Be(ProgrammeScopeKind.Speciality);
        asAdmin.Name.Should().Be("Paediatrics");
        asMember!.Kind.Should().Be(ProgrammeScopeKind.Institution);
        asMember.Name.Should().Be("Kgosi Kgari Teaching Hospital");
        asMember.ActingRole.Should().Be(WombatRoles.CommitteeMember);
    }

    [Fact]
    public async Task ARoleNotHeld_IsNull()
    {
        await using var db = Seeded();

        (await ProgrammeScope.ResolveAsync(
                db, TestPrincipals.InRole(WombatRoles.CommitteeMember, "zulu", Kgk), WombatRoles.Coordinator, CancellationToken.None))
            .Should().BeNull("the caller does not hold the role the page would read as");
    }

    [Theory]
    [InlineData(WombatRoles.Administrator)]
    [InlineData(WombatRoles.InstitutionalAdmin)]
    [InlineData(WombatRoles.Assessor)]
    [InlineData(WombatRoles.Trainee)]
    [InlineData("NoSuchRole")]
    public async Task ARoleThatIsNotOneOfTheFour_IsNull_EvenWhenHeld(string role)
    {
        await using var db = Seeded();

        (await ProgrammeScope.ResolveAsync(db, TestPrincipals.InRole(role, "someone", Kgk), role, CancellationToken.None))
            .Should().BeNull();
    }

    /// <summary>T185's trainee rung: a registrar who also holds an oversight seat reads no peer's record.</summary>
    [Fact]
    public async Task SomeoneInTheProgrammeAsATrainee_IsNull_WhateverRoleTheyReadAs()
    {
        await using var db = Seeded();
        var principal = TestPrincipals.InRoles([WombatRoles.Trainee, WombatRoles.CommitteeMember], "registrar-rep", Kgk);

        (await ProgrammeScope.ResolveAsync(db, principal, WombatRoles.CommitteeMember, CancellationToken.None))
            .Should().BeNull();
    }

    [Fact]
    public async Task ACallerWithNoInstitution_IsNull()
    {
        await using var db = Seeded();

        (await ProgrammeScope.ResolveAsync(
                db, TestPrincipals.InRole(WombatRoles.Coordinator, "smit", institutionId: null), WombatRoles.Coordinator, CancellationToken.None))
            .Should().BeNull();
    }

    /// <summary>
    /// An institution claim that names no institution, and a speciality admin who administers no speciality, have no
    /// programme to read: null, so the page reads as not found rather than naming a scope that reads nobody.
    /// </summary>
    [Fact]
    public async Task AnInstitutionThatDoesNotExist_OrAnAdminWithNothingInScope_IsNull()
    {
        await using var db = Seeded();

        (await ProgrammeScope.ResolveAsync(
                db, TestPrincipals.InRole(WombatRoles.Coordinator, "smit", 999), WombatRoles.Coordinator, CancellationToken.None))
            .Should().BeNull();
        (await ProgrammeScope.ResolveAsync(
                db, TestPrincipals.InRole(WombatRoles.SpecialityAdmin, "nobody", Kgk), WombatRoles.SpecialityAdmin, CancellationToken.None))
            .Should().BeNull();
        (await ProgrammeScope.ResolveAsync(
                db, TestPrincipals.InRole(WombatRoles.SubSpecialityAdmin, "nobody", Kgk, subSpecialityId: 4242), WombatRoles.SubSpecialityAdmin, CancellationToken.None))
            .Should().BeNull();
    }

    [Fact]
    public void TheRoles_AreTheFourThePagesAdmit_AndTheThreeThatMayRemind()
    {
        ProgrammeScope.RosterRoles.Should().Equal(
            WombatRoles.CommitteeMember, WombatRoles.SpecialityAdmin, WombatRoles.SubSpecialityAdmin, WombatRoles.Coordinator);
        ProgrammeScope.WaitingRoles.Should().Equal(
            WombatRoles.SpecialityAdmin, WombatRoles.SubSpecialityAdmin, WombatRoles.Coordinator);
    }

    // ---- the narrowings ----

    [Fact]
    public async Task Profiles_NeverReachAnotherInstitution_OrAnotherSpeciality_AndKeepTheEndedOnes()
    {
        await using var db = Seeded();
        db.TraineeProfiles.AddRange(
            Profile(1, "kgk-paed", Kgk, curriculumId: 3000),
            Profile(2, "kgk-neo", Kgk, curriculumId: 3001),
            Profile(3, "kgk-surg", Kgk, curriculumId: 3010),
            Profile(4, "other-paed", OtherHospital, curriculumId: 3000),
            Profile(5, "kgk-ended", Kgk, curriculumId: 3000, isActive: false));
        await db.SaveChangesAsync();

        async Task<int[]> Read(System.Security.Claims.ClaimsPrincipal principal, string role)
        {
            var scope = await ProgrammeScope.ResolveAsync(db, principal, role, CancellationToken.None);
            return await ProgrammeScope.Profiles(db, scope!).Select(profile => profile.Id).OrderBy(id => id).ToArrayAsync();
        }

        (await Read(TestPrincipals.InRole(WombatRoles.CommitteeMember, "zulu", Kgk), WombatRoles.CommitteeMember))
            .Should().Equal([1, 2, 3, 5], "the whole institution, ended included; never another hospital's");
        (await Read(TestPrincipals.InRole(WombatRoles.SpecialityAdmin, "mokoena", Kgk, specialityId: Paediatrics), WombatRoles.SpecialityAdmin))
            .Should().Equal([1, 2, 5], "the speciality's sub-specialities at the institution");
        (await Read(TestPrincipals.InRole(WombatRoles.SubSpecialityAdmin, "sithole", Kgk, subSpecialityId: GeneralPaediatrics), WombatRoles.SubSpecialityAdmin))
            .Should().Equal([1, 5], "the sub-speciality alone");
    }

    [Fact]
    public async Task Activities_NeverReachAnotherInstitution_AnUnstampedActivity_OrAnotherSpeciality()
    {
        await using var db = Seeded();
        var activities = new[]
        {
            Stamped(1, Kgk, Paediatrics, GeneralPaediatrics),
            Stamped(2, Kgk, Paediatrics, Neonatology),
            Stamped(3, Kgk, Surgery, GeneralSurgery),
            Stamped(4, OtherHospital, Paediatrics, GeneralPaediatrics),
            Stamped(5, null, Paediatrics, GeneralPaediatrics),
            Stamped(6, Kgk, null, null),
        }.AsQueryable();

        async Task<int[]> Read(System.Security.Claims.ClaimsPrincipal principal, string role)
        {
            var scope = await ProgrammeScope.ResolveAsync(db, principal, role, CancellationToken.None);
            return ProgrammeScope.Activities(activities, scope!).Select(activity => activity.Id).OrderBy(id => id).ToArray();
        }

        (await Read(TestPrincipals.InRole(WombatRoles.Coordinator, "smit", Kgk), WombatRoles.Coordinator))
            .Should().Equal([1, 2, 3, 6], "the institution's stamp; an unstamped activity is nobody's");
        (await Read(TestPrincipals.InRole(WombatRoles.SpecialityAdmin, "mokoena", Kgk, specialityId: Paediatrics), WombatRoles.SpecialityAdmin))
            .Should().Equal([1, 2], "the speciality's stamp at the institution; a null stamp is nobody's");
        (await Read(TestPrincipals.InRole(WombatRoles.SubSpecialityAdmin, "sithole", Kgk, subSpecialityId: GeneralPaediatrics), WombatRoles.SubSpecialityAdmin))
            .Should().Equal([1], "the sub-speciality's stamp at the institution");
    }

    private static Activity Stamped(int id, int? institutionId, int? specialityId, int? subSpecialityId) => new()
    {
        Id = id,
        InstitutionId = institutionId,
        SpecialityId = specialityId,
        SubSpecialityId = subSpecialityId,
        SubjectUserId = "registrar",
        CreatedByUserId = "registrar",
        CurrentState = "requested",
        DataJson = "{}"
    };

    private static TraineeProfile Profile(int id, string userId, int institutionId, int curriculumId, bool isActive = true) => new()
    {
        Id = id,
        UserId = userId,
        InstitutionId = institutionId,
        CurriculumId = curriculumId,
        ProgrammeStartDate = new DateOnly(2026, 1, 15),
        AdmittedOn = new DateOnly(2026, 1, 15),
        ExpectedCompletionDate = new DateOnly(2030, 1, 15),
        IsActive = isActive
    };

    private static ApplicationDbContext Seeded()
    {
        var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

        db.Institutions.AddRange(
            new Institution { Id = Kgk, Name = "Kgosi Kgari Teaching Hospital", ShortCode = "KGK" },
            new Institution { Id = OtherHospital, Name = "Other Hospital", ShortCode = "OTH" });
        db.Specialities.AddRange(
            new Speciality { Id = Paediatrics, CollegeId = 1, Name = "Paediatrics" },
            new Speciality { Id = Surgery, CollegeId = 1, Name = "Surgery" });
        db.SubSpecialities.AddRange(
            new SubSpeciality { Id = GeneralPaediatrics, SpecialityId = Paediatrics, Name = "General Paediatrics" },
            new SubSpeciality { Id = Neonatology, SpecialityId = Paediatrics, Name = "Neonatology" },
            new SubSpeciality { Id = GeneralSurgery, SpecialityId = Surgery, Name = "General Surgery" });
        db.Curricula.AddRange(
            new Curriculum { Id = 3000, SubSpecialityId = GeneralPaediatrics, Name = "Paed", Version = "1", EffectiveFrom = new DateOnly(2026, 1, 1) },
            new Curriculum { Id = 3001, SubSpecialityId = Neonatology, Name = "Neo", Version = "1", EffectiveFrom = new DateOnly(2026, 1, 1) },
            new Curriculum { Id = 3010, SubSpecialityId = GeneralSurgery, Name = "Surg", Version = "1", EffectiveFrom = new DateOnly(2026, 1, 1) });
        db.SaveChanges();
        return db;
    }
}
