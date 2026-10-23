using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Institutions.Queries.GetAdoptedDisciplinesForInstitution;
using Wombat.Application.Features.Users.Commands.AddRoleToUser;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Users;

/// <summary>
/// Add role's write (T376, flow 12, round 3 M2, F1, F2, S13): the role and the union of the scope held with what it adds,
/// in one transaction; the role's need judged on the resulting scope, so the order two roles came in never changes the
/// scope; a person at Any speciality narrowed by a speciality; and only what the PERSON's institution has adopted.
/// </summary>
public sealed class AddRoleToUserScopeTests
{
    private const int Kgk = 1;
    private const int Demo = 2;
    private const int Paediatrics = 10;
    private const int Surgery = 11;
    private const int GeneralPaediatrics = 20;
    private const int GeneralSurgery = 21;
    private const int NeonatalPaediatrics = 22;

    [Fact]
    public async Task M2_TheRoleAndTheUnionOfTheScope_AreWrittenInOneTransaction()
    {
        // Zulu at 2.13: a Committee member kept to Paediatrics, given Assessor in Paediatrics / Paediatrics.
        await using var db = Db();
        var users = Store(new UserIdentityDetails("zulu", "zulu@kgk", "Thandi", "Zulu", Kgk, [Paediatrics], [], [WombatRoles.CommitteeMember]));

        await Handler(users, db).Handle(
            new AddRoleToUserCommand("zulu", WombatRoles.Assessor, null, GeneralPaediatrics, TestPrincipals.InstitutionalAdmin(Kgk)),
            CancellationToken.None);

        users.Transactions.Should().Be(1);
        users.AddRoleCalls.Should().Equal(("zulu", WombatRoles.Assessor));
        users.UpdateScopeCalls.Should().ContainSingle();
        var (_, institutionId, specialities, subSpecialities) = users.UpdateScopeCalls[0];
        institutionId.Should().Be(Kgk);
        specialities.Should().Equal([Paediatrics], "the Committee member's speciality is kept: a union, never a replacement");
        subSpecialities.Should().Equal(GeneralPaediatrics);
    }

    [Fact]
    public async Task M2_AFaultAfterTheRole_LeavesTheTransactionToRollBack_AndThePageIsToldNothing()
    {
        await using var db = Db();
        var users = Store(new UserIdentityDetails("zulu", "zulu@kgk", "Thandi", "Zulu", Kgk, [Paediatrics], [], [WombatRoles.CommitteeMember]));
        users.UpdateScopeFault = new InvalidOperationException("the scope's save failed");

        var act = () => Handler(users, db).Handle(
            new AddRoleToUserCommand("zulu", WombatRoles.Assessor, null, GeneralPaediatrics, TestPrincipals.InstitutionalAdmin(Kgk)),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("the scope's save failed");
        users.Transactions.Should().Be(1, "both writes ran inside the one transaction, which the fault leaves uncommitted (the Postgres test holds the rollback)");
    }

    [Fact]
    public async Task B2_AScopeChangedBetweenTheReadAndTheWrite_IsKept_AndTheAddedOneJoinsIt()
    {
        // Two admins open Add a role for Smit, held at Any speciality. B commits Coordinator kept to Surgery first; A's add of
        // Assessor in Paediatrics / Paediatrics then writes the union of what is held NOW, not of what A first read
        // (T376 build review, B2), so neither change is lost.
        await using var db = Db();
        var users = Store(new UserIdentityDetails("smit", "s@kgk", "Pieter", "Smit", Kgk, [], [], [WombatRoles.CommitteeMember]));
        users.OnTransactionStart = store => store.Add(
            new UserIdentityDetails("smit", "s@kgk", "Pieter", "Smit", Kgk, [Surgery], [GeneralSurgery], [WombatRoles.CommitteeMember, WombatRoles.Coordinator]));

        await Handler(users, db).Handle(
            new AddRoleToUserCommand("smit", WombatRoles.Assessor, Paediatrics, GeneralPaediatrics, TestPrincipals.InstitutionalAdmin(Kgk)),
            CancellationToken.None);

        var (_, _, specialities, subSpecialities) = users.UpdateScopeCalls.Should().ContainSingle().Which;
        specialities.Should().Equal(Paediatrics, Surgery);
        subSpecialities.Should().Equal(GeneralPaediatrics, GeneralSurgery);
    }

    [Fact]
    public async Task B2_ARoleWhoseNeedTheChangedScopeNoLongerMeets_IsRefused_InsideTheTransaction()
    {
        // Patel held Paediatrics / Paediatrics when the form opened; before A's add of Sub-speciality admin (nothing added,
        // the held sub-speciality enough) another admin moved him to no scope. Judged on the read inside the transaction,
        // the add is refused, and the role written before it is rolled back with the transaction.
        await using var db = Db();
        var users = Store(new UserIdentityDetails("patel", "p@kgk", "Mohammed", "Patel", Kgk, [Paediatrics], [GeneralPaediatrics], [WombatRoles.CommitteeMember]));
        users.OnTransactionStart = store => store.Add(
            new UserIdentityDetails("patel", "p@kgk", "Mohammed", "Patel", Kgk, [], [], [WombatRoles.CommitteeMember]));

        var act = () => Handler(users, db).Handle(
            new AddRoleToUserCommand("patel", WombatRoles.SubSpecialityAdmin, null, null, TestPrincipals.InstitutionalAdmin(Kgk)),
            CancellationToken.None);

        (await act.Should().ThrowAsync<RoleAddRefusedException>()).Which.Reason.Should().Be(RoleAddRefusal.ScopeNeeded);
        users.UpdateScopeCalls.Should().BeEmpty();
    }

    [Fact]
    public async Task F1_EitherOrder_GivesTheSameScope_AndNeitherIsRefused()
    {
        // Patel, an Assessor in Paediatrics / Paediatrics, given Committee member: nothing to add, the scope unchanged.
        await using var db = Db();
        var assessorFirst = Store(new UserIdentityDetails("patel", "p@kgk", "Mohammed", "Patel", Kgk, [Paediatrics], [GeneralPaediatrics], [WombatRoles.Assessor]));
        await Handler(assessorFirst, db).Handle(
            new AddRoleToUserCommand("patel", WombatRoles.CommitteeMember, null, null, TestPrincipals.InstitutionalAdmin(Kgk)),
            CancellationToken.None);

        // The other order: a Committee member kept to Paediatrics, given Assessor with the sub-speciality it needs.
        var committeeFirst = Store(new UserIdentityDetails("patel", "p@kgk", "Mohammed", "Patel", Kgk, [Paediatrics], [], [WombatRoles.CommitteeMember]));
        await Handler(committeeFirst, db).Handle(
            new AddRoleToUserCommand("patel", WombatRoles.Assessor, null, GeneralPaediatrics, TestPrincipals.InstitutionalAdmin(Kgk)),
            CancellationToken.None);

        var one = assessorFirst.UpdateScopeCalls.Single();
        var other = committeeFirst.UpdateScopeCalls.Single();
        one.SpecialityIds.Should().Equal(other.SpecialityIds).And.Equal(Paediatrics);
        one.SubSpecialityIds.Should().Equal(other.SubSpecialityIds).And.Equal(GeneralPaediatrics);
    }

    [Fact]
    public async Task F1_TheInvitationsSubSpecialityRule_IsNeverAppliedToAScopeAlreadyHeld()
    {
        // A Coordinator may not be invited with a sub-speciality; one who holds one keeps it when given Committee member.
        await using var db = Db();
        var users = Store(new UserIdentityDetails("x", "x@kgk", "X", "Person", Kgk, [Paediatrics], [GeneralPaediatrics], [WombatRoles.Assessor]));

        await Handler(users, db).Handle(
            new AddRoleToUserCommand("x", WombatRoles.Coordinator, null, null, TestPrincipals.InstitutionalAdmin(Kgk)),
            CancellationToken.None);

        users.UpdateScopeCalls.Single().SubSpecialityIds.Should().Equal(GeneralPaediatrics);
    }

    [Fact]
    public async Task F2_ASpecialityGivenToSomeoneAtAnySpeciality_NarrowsTheirScope_ForEveryRole()
    {
        // Smit, a Coordinator at Any speciality, given Speciality admin in Paediatrics.
        await using var db = Db();
        var users = Store(new UserIdentityDetails("smit", "s@kgk", "Pieter", "Smit", Kgk, [], [], [WombatRoles.Coordinator]));

        await Handler(users, db).Handle(
            new AddRoleToUserCommand("smit", WombatRoles.SpecialityAdmin, Paediatrics, null, TestPrincipals.InstitutionalAdmin(Kgk)),
            CancellationToken.None);

        var scope = users.UpdateScopeCalls.Single();
        scope.SpecialityIds.Should().Equal(Paediatrics);
        scope.SubSpecialityIds.Should().BeEmpty();
    }

    [Theory]
    [InlineData(WombatRoles.SpecialityAdmin, null, null, RoleAddField.Speciality, "Speciality administrators must be scoped to a speciality.")]
    [InlineData(WombatRoles.Assessor, null, null, RoleAddField.Speciality, "The selected role requires speciality and sub-speciality scope.")]
    [InlineData(WombatRoles.Assessor, Paediatrics, null, RoleAddField.SubSpeciality, "The selected role requires speciality and sub-speciality scope.")]
    [InlineData(WombatRoles.SubSpecialityAdmin, Paediatrics, null, RoleAddField.SubSpeciality, "The selected role requires speciality and sub-speciality scope.")]
    public async Task TheRolesNeed_JudgedOnTheResultingScope_IsRefusedBesideItsField_AndNothingIsWritten(
        string role, int? specialityId, int? subSpecialityId, RoleAddField field, string message)
    {
        await using var db = Db();
        var users = Store(new UserIdentityDetails("smit", "s@kgk", "Pieter", "Smit", Kgk, [], [], [WombatRoles.Coordinator]));

        var act = () => Handler(users, db).Handle(
            new AddRoleToUserCommand("smit", role, specialityId, subSpecialityId, TestPrincipals.InstitutionalAdmin(Kgk)),
            CancellationToken.None);

        var refusal = (await act.Should().ThrowAsync<RoleAddRefusedException>()).Which;
        refusal.Reason.Should().Be(RoleAddRefusal.ScopeNeeded);
        refusal.Field.Should().Be(field);
        refusal.Message.Should().Be(message);
        users.Changes.Should().Be(0);
        users.Transactions.Should().Be(0);
    }

    [Fact]
    public async Task S13_OnlyWhatThePersonsInstitutionAdopted_MayBeAdded()
    {
        // KGK adopted Paediatrics only: Surgery (the Demo Institution's) is refused under Speciality.
        await using var db = Db();
        var users = Store(new UserIdentityDetails("smit", "s@kgk", "Pieter", "Smit", Kgk, [], [], [WombatRoles.Coordinator]));

        var act = () => Handler(users, db).Handle(
            new AddRoleToUserCommand("smit", WombatRoles.SpecialityAdmin, Surgery, null, TestPrincipals.InstitutionalAdmin(Kgk)),
            CancellationToken.None);

        var refusal = (await act.Should().ThrowAsync<RoleAddRefusedException>()).Which;
        refusal.Reason.Should().Be(RoleAddRefusal.NotAdopted);
        refusal.Field.Should().Be(RoleAddField.Speciality);
        users.Changes.Should().Be(0);
    }

    [Fact]
    public async Task S13_TheAdministrator_IsOfferedAndJudgedByThePersonsInstitution_NotHisOwn()
    {
        // P27: the Administrator (no institution) adding to the Demo trainee is held to the Demo Institution's set.
        await using var db = Db();
        var users = Store(new UserIdentityDetails("dev-trainee", "trainee@wombat.local", "Dev", "Trainee", Demo, [], [], [WombatRoles.Trainee]));

        var offered = await new GetAdoptedDisciplinesForInstitutionQueryHandler(db).Handle(
            new GetAdoptedDisciplinesForInstitutionQuery(Demo, TestPrincipals.Administrator()), CancellationToken.None);
        offered.Specialities.Select(entry => entry.Id).Should().Equal(Surgery);
        offered.SubSpecialities.Select(entry => entry.Id).Should().Equal(GeneralSurgery);

        var refused = () => Handler(users, db).Handle(
            new AddRoleToUserCommand("dev-trainee", WombatRoles.SpecialityAdmin, Paediatrics, null, TestPrincipals.Administrator()),
            CancellationToken.None);
        (await refused.Should().ThrowAsync<RoleAddRefusedException>()).Which.Reason.Should().Be(RoleAddRefusal.NotAdopted);

        await Handler(users, db).Handle(
            new AddRoleToUserCommand("dev-trainee", WombatRoles.SpecialityAdmin, Surgery, null, TestPrincipals.Administrator()),
            CancellationToken.None);
        users.UpdateScopeCalls.Single().SpecialityIds.Should().Equal(Surgery);
    }

    [Fact]
    public async Task S13_AnotherInstitutionsSet_ReadsAsNothing_ToAnInstitutionalAdmin()
    {
        await using var db = Db();

        var offered = await new GetAdoptedDisciplinesForInstitutionQueryHandler(db).Handle(
            new GetAdoptedDisciplinesForInstitutionQuery(Demo, TestPrincipals.InstitutionalAdmin(Kgk)), CancellationToken.None);

        offered.Specialities.Should().BeEmpty();
        offered.SubSpecialities.Should().BeEmpty();
    }

    [Fact]
    public async Task ASubSpecialityOfASpecialityThePersonWillNotHold_IsRefused()
    {
        // KGK adopted Neonatal under Paediatrics too; Surgery's General Surgery is not under the person's speciality.
        await using var db = Db();
        db.Set<InstitutionCurriculumAdoption>().Add(new InstitutionCurriculumAdoption
            { Id = 9, InstitutionId = Kgk, CurriculumId = 9, SubSpecialityId = GeneralSurgery, AdoptedOn = new DateOnly(2026, 1, 1) });
        db.Set<InstitutionCurriculumAdoption>().Add(new InstitutionCurriculumAdoption
            { Id = 10, InstitutionId = Kgk, CurriculumId = 10, SubSpecialityId = NeonatalPaediatrics, AdoptedOn = new DateOnly(2026, 1, 1) });
        await db.SaveChangesAsync();
        var users = Store(new UserIdentityDetails("zulu", "zulu@kgk", "Thandi", "Zulu", Kgk, [Paediatrics], [], [WombatRoles.CommitteeMember]));

        var act = () => Handler(users, db).Handle(
            new AddRoleToUserCommand("zulu", WombatRoles.Assessor, null, GeneralSurgery, TestPrincipals.InstitutionalAdmin(Kgk)),
            CancellationToken.None);

        (await act.Should().ThrowAsync<RoleAddRefusedException>()).Which.Field.Should().Be(RoleAddField.SubSpeciality);
        users.Changes.Should().Be(0);
    }

    [Fact]
    public async Task APersonWithNoInstitution_IsGivenNoRole_AndNothingIsWritten()
    {
        // U14's College admin: every role offered here works at an institution.
        await using var db = Db();
        var users = Store(new UserIdentityDetails("college", "c@college", "College", "Admin", null, [], [], [WombatRoles.CollegeAdmin], CollegeId: 1));

        var act = () => Handler(users, db).Handle(
            new AddRoleToUserCommand("college", WombatRoles.Coordinator, null, null, TestPrincipals.Administrator()),
            CancellationToken.None);

        (await act.Should().ThrowAsync<RoleAddRefusedException>()).Which.Reason.Should().Be(RoleAddRefusal.NoInstitution);
        users.Changes.Should().Be(0);
    }

    [Fact]
    public async Task ARoleAlreadyHeld_IsRefused()
    {
        await using var db = Db();
        var users = Store(new UserIdentityDetails("zulu", "zulu@kgk", "Thandi", "Zulu", Kgk, [Paediatrics], [], [WombatRoles.CommitteeMember]));

        var act = () => Handler(users, db).Handle(
            new AddRoleToUserCommand("zulu", WombatRoles.CommitteeMember, null, null, TestPrincipals.InstitutionalAdmin(Kgk)),
            CancellationToken.None);

        (await act.Should().ThrowAsync<RoleAddRefusedException>()).Which.Reason.Should().Be(RoleAddRefusal.AlreadyHeld);
        users.Changes.Should().Be(0);
    }

    [Fact]
    public async Task InstitutionalAdmin_TakesNoSpeciality()
    {
        await using var db = Db();
        var users = Store(new UserIdentityDetails("smit", "s@kgk", "Pieter", "Smit", Kgk, [], [], [WombatRoles.Coordinator]));

        var act = () => Handler(users, db).Handle(
            new AddRoleToUserCommand("smit", WombatRoles.InstitutionalAdmin, Paediatrics, null, TestPrincipals.InstitutionalAdmin(Kgk)),
            CancellationToken.None);

        (await act.Should().ThrowAsync<RoleAddRefusedException>()).Which.Reason.Should().Be(RoleAddRefusal.NotScopable);
        users.Changes.Should().Be(0);
    }

    private static AddRoleToUserCommandHandler Handler(RecordingUserAdministrationService users, ApplicationDbContext db)
        => new(users, db);

    private static RecordingUserAdministrationService Store(UserIdentityDetails person)
    {
        var users = new RecordingUserAdministrationService();
        users.Add(person);
        return users;
    }

    /// <summary>KGK adopted General Paediatrics; the Demo Institution General Surgery.</summary>
    private static ApplicationDbContext Db()
    {
        var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
        db.Institutions.AddRange(
            new Institution { Id = Kgk, Name = "Kgosi Kgari Teaching Hospital", ShortCode = "KGK", IsActive = true, CreatedOn = DateTime.UtcNow },
            new Institution { Id = Demo, Name = "Demo Institution", ShortCode = "DEMO", IsActive = true, CreatedOn = DateTime.UtcNow });
        db.Set<College>().Add(new College { Id = 1, Name = "College", ShortCode = "C" });
        db.Set<Speciality>().AddRange(
            new Speciality { Id = Paediatrics, CollegeId = 1, Name = "Paediatrics" },
            new Speciality { Id = Surgery, CollegeId = 1, Name = "Surgery" });
        db.Set<SubSpeciality>().AddRange(
            new SubSpeciality { Id = GeneralPaediatrics, SpecialityId = Paediatrics, Name = "Paediatrics" },
            new SubSpeciality { Id = NeonatalPaediatrics, SpecialityId = Paediatrics, Name = "Neonatology" },
            new SubSpeciality { Id = GeneralSurgery, SpecialityId = Surgery, Name = "General Surgery" });
        db.Set<InstitutionCurriculumAdoption>().AddRange(
            new InstitutionCurriculumAdoption { Id = 1, InstitutionId = Kgk, CurriculumId = 1, SubSpecialityId = GeneralPaediatrics, AdoptedOn = new DateOnly(2026, 1, 1) },
            new InstitutionCurriculumAdoption { Id = 2, InstitutionId = Demo, CurriculumId = 2, SubSpecialityId = GeneralSurgery, AdoptedOn = new DateOnly(2026, 1, 1) });
        db.SaveChanges();
        return db;
    }
}
