using FluentAssertions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Application.Features.Programme.Trainees;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Persistence;
using static Wombat.Application.Tests.Features.Programme.ProgrammeCast;

namespace Wombat.Application.Tests.Features.Programme;

/// <summary>
/// T358 (flow 06, C2; review 6): one registrar by trainee-profile id, for the registrar page. Only the user's preferred
/// profile, only in the reading role's scope, only while an account holds the user id; anything else is null ("Page not
/// found"), so an id confirms nothing. An ended programme opens, read-only (r6).
/// </summary>
public sealed class GetProgrammeTraineeQueryTests
{
    [Fact]
    public async Task TheRegistrarsCurrentProfile_Opens_WithItsHeader()
    {
        var (db, users) = Build();
        await using var _ = db;

        var registrar = await GetAsync(db, users, CommitteeMember(), WombatRoles.CommitteeMember, MahlanguProfile);

        registrar.Should().NotBeNull();
        registrar!.ProfileId.Should().Be(MahlanguProfile);
        registrar.TraineeUserId.Should().Be(Mahlangu);
        registrar.Name.Should().Be("Nomsa Mahlangu");
        registrar.TrainingYear.Should().Be(1);
        registrar.InstitutionName.Should().Be("Kgosi Kgari Teaching Hospital");
        registrar.SubSpecialityName.Should().Be("Paediatrics");
        registrar.CurrentSemesterName.Should().Be("Semester 2, 2026");
        registrar.Ended.Should().BeNull();
        registrar.Scope.ActingRole.Should().Be(WombatRoles.CommitteeMember);
    }

    [Fact]
    public async Task AnOlderProfilesId_IsNull()
    {
        // Review 6: flow 05's reads take a user id and read the preferred profile, so an old programme's address would
        // show the newer one.
        var (db, users) = Build();
        await using var _ = db;
        var older = new TraineeProfile
        {
            Id = 90, UserId = Mahlangu, InstitutionId = Kgk, CurriculumId = PaediatricsCurriculum,
            ProgrammeStartDate = new DateOnly(2022, 1, 15), AdmittedOn = new DateOnly(2022, 1, 15),
            ExpectedCompletionDate = new DateOnly(2026, 1, 15)
        };
        older.Complete(new DateOnly(2025, 12, 1), D);
        db.TraineeProfiles.Add(older);
        db.SaveChanges();

        (await GetAsync(db, users, CommitteeMember(), WombatRoles.CommitteeMember, 90)).Should().BeNull();
        (await GetAsync(db, users, CommitteeMember(), WombatRoles.CommitteeMember, MahlanguProfile)).Should().NotBeNull();
    }

    [Fact]
    public async Task OutOfScope_Unknown_AndErased_AreNull()
    {
        var (db, users) = Build();
        await using var _ = db;
        db.TraineeProfiles.Find(NdlovuProfile)!.Erase("erased-pseudonym", D);
        db.SaveChanges();

        (await GetAsync(db, users, SpecialityAdmin(), WombatRoles.SpecialityAdmin, SurgeonProfile))
            .Should().BeNull("the surgical registrar is outside Paediatrics");
        (await GetAsync(db, users, CommitteeMember(), WombatRoles.CommitteeMember, ElsewhereProfile))
            .Should().BeNull("another institution's registrar");
        (await GetAsync(db, users, CommitteeMember(), WombatRoles.CommitteeMember, 9999)).Should().BeNull();
        (await GetAsync(db, users, CommitteeMember(), WombatRoles.CommitteeMember, NdlovuProfile))
            .Should().BeNull("an erased profile is a pseudonym no account holds");
        (await GetAsync(db, users, CommitteeMember(), WombatRoles.Coordinator, MahlanguProfile))
            .Should().BeNull("the role read as is not held");
    }

    [Fact]
    public async Task AnEndedProgramme_Opens_AsOnItsLastDay()
    {
        var (db, users) = Build();
        await using var _ = db;
        db.TraineeProfiles.Find(DuPlessisProfile)!.Deactivate(new DateOnly(2026, 10, 2), D);
        db.SaveChanges();

        var registrar = await GetAsync(db, users, SpecialityAdmin(), WombatRoles.SpecialityAdmin, DuPlessisProfile);

        registrar.Should().NotBeNull();
        registrar!.Ended.Should().Be(new ProgrammeEndDto(Completed: false, new DateOnly(2026, 10, 2), D));
        registrar.TrainingYear.Should().Be(2);
        registrar.CurrentSemesterName.Should().Be("Semester 2, 2026");
    }

    [Fact]
    public async Task ALockedAccount_StillOpens()
    {
        var (db, _) = Build();
        await using var __ = db;
        var users = new Wombat.Tests.Shared.FakeUserDirectory().With(new UserIdentityDetails(
            Mahlangu, "m@test", "Nomsa", "Mahlangu", Kgk, [], [], [WombatRoles.Trainee]) { IsLockedOut = true, IsDeactivated = true });

        (await GetAsync(db, users, Coordinator(), WombatRoles.Coordinator, MahlanguProfile)).Should().NotBeNull(
            "a lock takes a registrar off the lists of current registrars (T268), not out of the staff's reach");
    }

    private static Task<ProgrammeTraineeDto?> GetAsync(
        ApplicationDbContext db,
        IUserAdministrationService users,
        System.Security.Claims.ClaimsPrincipal principal,
        string actingRole,
        int profileId)
        => new GetProgrammeTraineeQueryHandler(db, users, TimeProvider.System).Handle(
            new GetProgrammeTraineeQuery(principal, actingRole, profileId, D), CancellationToken.None);
}
