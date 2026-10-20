using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Dashboards.Trainee;
using Wombat.Application.Features.Trainees;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Domain.Invitations;
using Wombat.Infrastructure.Activities;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.Features.Trainees;

/// <summary>
/// T373 (flow 11, lane A): the admission and what the onboarding pages read about a registrar. F1 (only the curriculum
/// adopted for the invited sub-speciality; the invited scope kept), F4 (an empty completion whose derived date has passed
/// is refused), S6 (a deactivated account refused and left off Waiting), S9 (typed refusals; not pending, out of scope and
/// unknown read alike), Q7 (the admission's mail), F3 and check rule 7 (Invited by on the pending Home), T306 (the
/// training year), S4 (the used invitation found whatever the address's spelling).
/// </summary>
public sealed class AdmissionFlow11Tests
{
    private const int Kgk = 10;
    private const int Other = 11;
    private const int Paediatrics = 1;
    private const int GeneralPaediatrics = 1;
    private const int Neonatology = 2;
    private const int PaedCurriculum = 3000;
    private const int NeoCurriculum = 3001;

    // 08:00 UTC on 2026-10-19 is 10:00 on the same day in South Africa.
    private static readonly FixedClock Today = new(new DateTimeOffset(2026, 10, 19, 8, 0, 0, TimeSpan.Zero));

    // ── The admission ────────────────────────────────────────────────────────────

    [Fact]
    public async Task F1_TheInvitedSubSpecialitysCurriculum_IsAdmitted_AndTheInvitedScopeIsKept()
    {
        await using var db = Seeded();
        var users = Directory(Molefe());
        var notices = new TestNotices(users.Object);

        var result = await Handler(db, users, notices).Handle(
            Admit(PaedCurriculum, new DateOnly(2023, 1, 15), new DateOnly(2027, 1, 14)), CancellationToken.None);

        result.CurriculumId.Should().Be(PaedCurriculum);
        result.TrainingYear.Should().Be(4);
        (await db.TraineeProfiles.SingleAsync()).ExpectedCompletionDate.Should().Be(new DateOnly(2027, 1, 14));

        // F1: no rewrite of the invited scope (AdmitTrainee.cs:129–134 at 69a7df20 wrote the chosen curriculum's).
        users.Verify(service => service.UpdateScopeAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<IReadOnlyCollection<int>>(), It.IsAny<IReadOnlyCollection<int>>(), It.IsAny<CancellationToken>()), Times.Never);
        users.Verify(service => service.PromotePendingTraineeAsync("molefe", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task F1_AnotherAdoptedCurriculum_IsRefused_BeforeAnythingIsStaged()
    {
        await using var db = Seeded();
        var users = Directory(Molefe());

        var act = () => Handler(db, users).Handle(
            Admit(NeoCurriculum, new DateOnly(2026, 1, 15), null), CancellationToken.None);

        (await act.Should().ThrowAsync<TraineeAdmissionRefusedException>())
            .Which.Reason.Should().Be(TraineeAdmissionRefusal.CurriculumNotOffered);
        db.ChangeTracker.HasChanges().Should().BeFalse("the audit pipeline's save would commit anything staged");
        db.TraineeProfiles.Should().BeEmpty();
    }

    [Fact]
    public async Task F1_ARegistrarInvitedToNoSubSpeciality_ChoosesAnAdoptedCurriculum_AndTakesItsScope()
    {
        await using var db = Seeded();
        var users = Directory(Molefe() with { SpecialityIds = [], SubSpecialityIds = [] });

        await Handler(db, users).Handle(Admit(NeoCurriculum, new DateOnly(2026, 1, 15), null), CancellationToken.None);

        users.Verify(service => service.UpdateScopeAsync(
            "molefe",
            Kgk,
            It.Is<IReadOnlyCollection<int>>(ids => ids.SequenceEqual(new[] { Paediatrics })),
            It.Is<IReadOnlyCollection<int>>(ids => ids.SequenceEqual(new[] { Neonatology })),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task NothingAdoptedForTheInvitedSubSpeciality_IsRefused()
    {
        await using var db = Seeded(adoptPaediatrics: false);
        var users = Directory(Molefe());

        var act = () => Handler(db, users).Handle(Admit(PaedCurriculum, new DateOnly(2026, 1, 15), null), CancellationToken.None);

        (await act.Should().ThrowAsync<TraineeAdmissionRefusedException>())
            .Which.Reason.Should().Be(TraineeAdmissionRefusal.NothingAdopted);
    }

    [Theory]
    [InlineData(2023, 1, 15, true)]    // 12 months on: 2024-01-15, passed on 2026-10-19.
    [InlineData(2026, 1, 15, false)]   // 2027-01-15, still to come.
    public async Task F4_AnEmptyCompletion_WhoseDerivedDateHasPassed_IsRefused(int year, int month, int day, bool refused)
    {
        await using var db = Seeded();
        var users = Directory(Molefe());

        var act = () => Handler(db, users).Handle(Admit(PaedCurriculum, new DateOnly(year, month, day), null), CancellationToken.None);

        if (refused)
        {
            var refusal = (await act.Should().ThrowAsync<TraineeAdmissionRefusedException>()).Which;
            refusal.Reason.Should().Be(TraineeAdmissionRefusal.CompletionPassed);
            refusal.DerivedCompletion.Should().Be(new DateOnly(2024, 1, 15));
            refusal.Message.Should().Be("Left empty, it would be 2024-01-15, which has passed. Enter the expected completion.");
            db.TraineeProfiles.Should().BeEmpty();
        }
        else
        {
            await act.Should().NotThrowAsync();
            (await db.TraineeProfiles.SingleAsync()).ExpectedCompletionDate.Should().Be(new DateOnly(2027, 1, 15));
        }
    }

    [Fact]
    public async Task F4_APastStart_WithTheCompletionGiven_IsAdmitted()
    {
        await using var db = Seeded();
        var users = Directory(Molefe());

        await Handler(db, users).Handle(Admit(PaedCurriculum, new DateOnly(2023, 1, 15), new DateOnly(2027, 1, 14)), CancellationToken.None);

        db.TraineeProfiles.Should().ContainSingle();
    }

    [Fact]
    public async Task S6_ADeactivatedAccount_IsRefused()
    {
        await using var db = Seeded();
        var users = Directory(Molefe() with { IsDeactivated = true });

        var act = () => Handler(db, users).Handle(Admit(PaedCurriculum, new DateOnly(2026, 1, 15), null), CancellationToken.None);

        (await act.Should().ThrowAsync<TraineeAdmissionRefusedException>())
            .Which.Reason.Should().Be(TraineeAdmissionRefusal.Deactivated);
        users.Verify(service => service.PromotePendingTraineeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>M08, S9: an unknown id, an account not waiting and one of another institution are one refusal, in words that name nobody.</summary>
    [Theory]
    [InlineData("nobody")]
    [InlineData("assessor")]
    [InlineData("other-registrar")]
    public async Task NotPending_OutOfScope_AndUnknown_AreOneRefusal(string userId)
    {
        await using var db = Seeded();
        var users = Directory(
            Molefe(),
            new UserIdentityDetails("assessor", "assessor@kgk", "Thabo", "Assessor", Kgk, [], [], [WombatRoles.Assessor]),
            new UserIdentityDetails("other-registrar", "x@other", "Nandi", "Other", Other, [Paediatrics], [GeneralPaediatrics], [WombatRoles.PendingTrainee]));

        var act = () => Handler(db, users).Handle(
            new AdmitTraineeCommand(userId, PaedCurriculum, new DateOnly(2026, 1, 15), null, TestPrincipals.InstitutionalAdmin(Kgk)),
            CancellationToken.None);

        var refusal = (await act.Should().ThrowAsync<TraineeAdmissionRefusedException>()).Which;
        refusal.Reason.Should().Be(TraineeAdmissionRefusal.NotPending);
        refusal.Message.Should().Be("No registrar waiting for admission has this link.");
    }

    [Fact]
    public async Task M09_ASecondAdmitFromAStaleTab_IsToldTheRegistrarIsAdmitted()
    {
        await using var db = Seeded();
        var users = Directory(Molefe());
        await Handler(db, users).Handle(Admit(PaedCurriculum, new DateOnly(2026, 1, 15), null), CancellationToken.None);

        // Promoted: the account no longer holds PendingTrainee.
        users.Setup(service => service.GetByIdAsync("molefe", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Molefe() with { Roles = [WombatRoles.Trainee] });
        var act = () => Handler(db, users).Handle(Admit(PaedCurriculum, new DateOnly(2026, 1, 15), null), CancellationToken.None);

        (await act.Should().ThrowAsync<TraineeAdmissionRefusedException>())
            .Which.Reason.Should().Be(TraineeAdmissionRefusal.AlreadyAdmitted);
    }

    [Fact]
    public async Task M04b_ARegistrarOfNoInstitution_IsRefusedToTheAdministrator_AndNotWaitingToAnInstitutionalAdmin()
    {
        await using var db = Seeded();
        var users = Directory(Molefe() with { InstitutionId = null });

        var asAdministrator = () => Handler(db, users).Handle(
            new AdmitTraineeCommand("molefe", PaedCurriculum, new DateOnly(2026, 1, 15), null, TestPrincipals.Administrator()),
            CancellationToken.None);
        var asInstitutionalAdmin = () => Handler(db, users).Handle(Admit(PaedCurriculum, new DateOnly(2026, 1, 15), null), CancellationToken.None);

        (await asAdministrator.Should().ThrowAsync<TraineeAdmissionRefusedException>()).Which.Reason.Should().Be(TraineeAdmissionRefusal.NoInstitution);
        (await asInstitutionalAdmin.Should().ThrowAsync<TraineeAdmissionRefusedException>()).Which.Reason.Should().Be(TraineeAdmissionRefusal.NotPending);
    }

    [Fact]
    public async Task TheRefusals_AreTheAdmissionsOwn()
    {
        TraineeAdmissionRefusals.IsRefusal(new TraineeAdmissionRefusedException(TraineeAdmissionRefusal.NotPending)).Should().BeTrue();
        TraineeAdmissionRefusals.IsRefusal(new UnauthorizedAccessException()).Should().BeTrue();
        TraineeAdmissionRefusals.IsRefusal(new InvalidOperationException("Sequence contains no elements")).Should().BeFalse();
        await Task.CompletedTask;
    }

    // ── Q7: the admission's mail ─────────────────────────────────────────────────

    [Fact]
    public async Task Q7_TheMail_IsSentOnce_AfterTheSave_InItsWords()
    {
        await using var db = Seeded();
        var users = Directory(Molefe());
        var notices = new TestNotices(users.Object);

        await Handler(db, users, notices).Handle(
            Admit(PaedCurriculum, new DateOnly(2023, 1, 15), new DateOnly(2027, 1, 14)), CancellationToken.None);

        var mail = notices.Sender.Sent.Should().ContainSingle().Subject;
        mail.To.Should().Be("molefe@kgk.wombat.local");
        mail.Subject.Should().Be("You are admitted in Wombat");
        mail.TextBody.Should().Contain("You have been admitted to the Paediatric EPA Curriculum (11.1), in training year 4.")
            .And.Contain("If you were signed in to Wombat, you have been signed out once: sign in again to log activities and follow your progress.")
            .And.Contain("https://wombat.test/account/login");
        mail.TextBody.Should().NotContain("Kgosi Kgari").And.NotContain("2027-01-14", "no other data");
    }

    [Fact]
    public async Task Q7_BeforeTheProgrammeStarts_TheMailSaysTheDay()
    {
        await using var db = Seeded();
        var users = Directory(Molefe());
        var notices = new TestNotices(users.Object);

        await Handler(db, users, notices).Handle(Admit(PaedCurriculum, new DateOnly(2027, 1, 15), null), CancellationToken.None);

        notices.Sender.Sent.Single().TextBody.Should().Contain("Your programme starts on 2027-01-15.");
    }

    [Fact]
    public async Task Q7_AFailedSend_DoesNotFailTheAdmission()
    {
        await using var db = Seeded();
        var users = Directory(Molefe());
        var notices = new TestNotices(users.Object, failSendWith: new InvalidOperationException("SMTP down"));

        var act = () => Handler(db, users, notices).Handle(Admit(PaedCurriculum, new DateOnly(2026, 1, 15), null), CancellationToken.None);

        await act.Should().NotThrowAsync();
        db.TraineeProfiles.Should().ContainSingle();
    }

    [Fact]
    public async Task Q7_ARefusedAdmission_MailsNobody()
    {
        await using var db = Seeded();
        var users = Directory(Molefe());
        var notices = new TestNotices(users.Object);

        var act = () => Handler(db, users, notices).Handle(Admit(NeoCurriculum, new DateOnly(2026, 1, 15), null), CancellationToken.None);

        await act.Should().ThrowAsync<TraineeAdmissionRefusedException>();
        notices.Sender.Sent.Should().BeEmpty();
    }

    // ── The Admit page's reads ───────────────────────────────────────────────────

    [Fact]
    public async Task TheCandidate_NamesTheInvitedScope_TheRegistrationDay_AndTheOneCurriculum()
    {
        await using var db = Seeded();
        // Registered at 22:30 UTC on 2026-10-18: already 2026-10-19 in South Africa. The invitation's address in another case (S4).
        db.Set<Invitation>().Add(UsedInvitation("Molefe@KGK.wombat.local", usedOn: new DateTime(2026, 10, 18, 22, 30, 0, DateTimeKind.Utc)));
        await db.SaveChangesAsync();
        var users = Directory(Molefe());

        var candidate = await new GetAdmissionCandidateQueryHandler(db, users.Object, Today)
            .Handle(new GetAdmissionCandidateQuery("molefe", TestPrincipals.InstitutionalAdmin(Kgk)), CancellationToken.None);

        candidate.Should().NotBeNull();
        candidate!.FullName.Should().Be("Lerato Molefe");
        candidate.InstitutionName.Should().Be("Kgosi Kgari Teaching Hospital");
        candidate.InvitedAs.Should().Be("Paediatrics / Paediatrics");
        candidate.RegisteredOn.Should().Be(new DateOnly(2026, 10, 19));
        candidate.HasInvitedScope.Should().BeTrue();
        candidate.Curricula.Select(curriculum => curriculum.Id).Should().Equal(PaedCurriculum);
        candidate.Standing.Should().Be(AdmissionStanding.Ready);
        candidate.Today.Should().Be(new DateOnly(2026, 10, 19));
    }

    [Fact]
    public async Task TheCandidate_InvitedToNone_IsOfferedTheInstitutionsAdoptions()
    {
        await using var db = Seeded();
        var users = Directory(Molefe() with { SpecialityIds = [], SubSpecialityIds = [] });

        var candidate = await new GetAdmissionCandidateQueryHandler(db, users.Object, Today)
            .Handle(new GetAdmissionCandidateQuery("molefe", TestPrincipals.Administrator()), CancellationToken.None);

        candidate!.InvitedAs.Should().BeNull();
        candidate.RegisteredOn.Should().BeNull("an SSO registrar registered from no invitation");
        candidate.HasInvitedScope.Should().BeFalse();
        candidate.Curricula.Select(curriculum => curriculum.Id).Should().BeEquivalentTo([PaedCurriculum, NeoCurriculum]);
    }

    [Theory]
    [InlineData(true, AdmissionStanding.NothingAdoptedForInvitedScope)]
    [InlineData(false, AdmissionStanding.NothingAdopted)]
    public async Task TheCandidate_SaysWhyNothingIsOffered(bool othersAdopted, AdmissionStanding standing)
    {
        await using var db = Seeded(adoptPaediatrics: false, adoptNeonatology: othersAdopted);
        var users = Directory(Molefe());

        var candidate = await new GetAdmissionCandidateQueryHandler(db, users.Object, Today)
            .Handle(new GetAdmissionCandidateQuery("molefe", TestPrincipals.InstitutionalAdmin(Kgk)), CancellationToken.None);

        candidate!.Standing.Should().Be(standing);
        candidate.Curricula.Should().BeEmpty();
    }

    [Theory]
    [InlineData("nobody")]
    [InlineData("assessor")]
    [InlineData("other-registrar")]
    public async Task TheCandidate_NotPending_OutOfScope_AndUnknown_ReadAlike(string userId)
    {
        await using var db = Seeded();
        var users = Directory(
            Molefe(),
            new UserIdentityDetails("assessor", "assessor@kgk", "Thabo", "Assessor", Kgk, [], [], [WombatRoles.Assessor]),
            new UserIdentityDetails("other-registrar", "x@other", "Nandi", "Other", Other, [Paediatrics], [GeneralPaediatrics], [WombatRoles.PendingTrainee]));

        var candidate = await new GetAdmissionCandidateQueryHandler(db, users.Object, Today)
            .Handle(new GetAdmissionCandidateQuery(userId, TestPrincipals.InstitutionalAdmin(Kgk)), CancellationToken.None);

        candidate.Should().BeNull();
    }

    [Theory]
    [InlineData(2026, 10, 19, 1, false)]
    [InlineData(2023, 1, 15, 4, true)]
    [InlineData(2022, 1, 15, 5, true)]
    public async Task TheDates_TheTrainingYearAndTheDerivedCompletion(int year, int month, int day, int trainingYear, bool passed)
    {
        await using var db = Seeded();

        var dates = await new GetAdmissionDatesQueryHandler(db, Directory(Molefe()).Object, Today).Handle(
            new GetAdmissionDatesQuery("molefe", PaedCurriculum, new DateOnly(year, month, day), TestPrincipals.InstitutionalAdmin(Kgk)),
            CancellationToken.None);

        dates.Today.Should().Be(new DateOnly(2026, 10, 19));
        dates.TrainingYear.Should().Be(trainingYear);
        dates.DerivedCompletion.Should().Be(new DateOnly(year, month, day).AddMonths(12));
        dates.DerivedHasPassed.Should().Be(passed);
        dates.StartsMoreThanFourYearsAgo.Should().Be(year == 2022, "M07: more than four years back");
    }

    [Fact]
    public async Task TheDates_AFutureStart_HasNoTrainingYear_AndNoCurriculum_DerivesNothing()
    {
        await using var db = Seeded();

        var dates = await new GetAdmissionDatesQueryHandler(db, Directory(Molefe()).Object, Today).Handle(
            new GetAdmissionDatesQuery("molefe", null, new DateOnly(2027, 1, 15), TestPrincipals.InstitutionalAdmin(Kgk)), CancellationToken.None);

        dates.TrainingYear.Should().BeNull();
        dates.DerivedCompletion.Should().BeNull();
        dates.DerivedHasPassed.Should().BeFalse();
    }

    /// <summary>
    /// T373 fix pass, B12: the derived completion is answered only for a curriculum the registrar is offered (F1), and only
    /// for a registrar waiting in the caller's scope; any other id derives nothing, as the command refuses it.
    /// </summary>
    [Theory]
    [InlineData("molefe", NeoCurriculum)]
    [InlineData("other-registrar", PaedCurriculum)]
    [InlineData("nobody", PaedCurriculum)]
    public async Task TheDates_DeriveNothing_ForACurriculumNotOffered_OrARegistrarOutOfScope(string userId, int curriculumId)
    {
        await using var db = Seeded();
        var users = Directory(
            Molefe(),
            new UserIdentityDetails("other-registrar", "x@other", "Nandi", "Other", Other, [Paediatrics], [GeneralPaediatrics], [WombatRoles.PendingTrainee]));

        var dates = await new GetAdmissionDatesQueryHandler(db, users.Object, Today).Handle(
            new GetAdmissionDatesQuery(userId, curriculumId, new DateOnly(2026, 1, 15), TestPrincipals.InstitutionalAdmin(Kgk)),
            CancellationToken.None);

        dates.DerivedCompletion.Should().BeNull();
        dates.DerivedHasPassed.Should().BeFalse();
        dates.TrainingYear.Should().Be(1, "the training year is the start's alone");
    }

    /// <summary>The training year is counted to the South African day: 22:30 UTC on 2027-01-14 is already 2027-01-15 there.</summary>
    [Fact]
    public async Task TheDates_AreCountedOnTheSouthAfricanDay()
    {
        await using var db = Seeded();
        var lateEvening = new FixedClock(new DateTimeOffset(2027, 1, 14, 22, 30, 0, TimeSpan.Zero));

        var dates = await new GetAdmissionDatesQueryHandler(db, Directory(Molefe()).Object, lateEvening).Handle(
            new GetAdmissionDatesQuery("molefe", PaedCurriculum, new DateOnly(2026, 1, 15), TestPrincipals.InstitutionalAdmin(Kgk)), CancellationToken.None);

        dates.Today.Should().Be(new DateOnly(2027, 1, 15));
        dates.TrainingYear.Should().Be(2);
    }

    [Fact]
    public async Task TheResult_IsReadAgainInScope_AndSaysWhetherTheCompletionWasDerived()
    {
        await using var db = Seeded();
        var users = Directory(Molefe());
        var admitted = await Handler(db, users).Handle(Admit(PaedCurriculum, new DateOnly(2026, 1, 15), null), CancellationToken.None);
        var names = new FakeUserDirectory(("molefe", "Lerato Molefe"));

        var result = await new GetAdmissionResultQueryHandler(db, names, Today)
            .Handle(new GetAdmissionResultQuery(admitted.Id, TestPrincipals.InstitutionalAdmin(Kgk)), CancellationToken.None);
        var outOfScope = await new GetAdmissionResultQueryHandler(db, names, Today)
            .Handle(new GetAdmissionResultQuery(admitted.Id, TestPrincipals.InstitutionalAdmin(Other)), CancellationToken.None);
        var unknown = await new GetAdmissionResultQueryHandler(db, names, Today)
            .Handle(new GetAdmissionResultQuery(admitted.Id + 100, TestPrincipals.InstitutionalAdmin(Kgk)), CancellationToken.None);

        result.Should().BeEquivalentTo(new
        {
            FullName = "Lerato Molefe",
            CurriculumName = "Paediatric EPA Curriculum",
            CurriculumVersion = "11.1",
            TrainingYear = (int?)1,
            ProgrammeStart = new DateOnly(2026, 1, 15),
            ExpectedCompletion = new DateOnly(2027, 1, 15),
            CompletionWasDerived = true
        });
        outOfScope.Should().BeNull();
        unknown.Should().BeNull();
    }

    // ── Waiting and Current trainees ─────────────────────────────────────────────

    [Fact]
    public async Task Waiting_BySurname_WithTheScopeAndTheRegistrationDay_LeavesOffADeactivatedAccount()
    {
        await using var db = Seeded();
        db.Set<Invitation>().Add(UsedInvitation("molefe@kgk.wombat.local", usedOn: new DateTime(2026, 10, 19, 7, 0, 0, DateTimeKind.Utc)));
        await db.SaveChangesAsync();
        var directory = new FakeUserDirectory()
            .With(Molefe())
            .With(new UserIdentityDetails("dlamini", "dlamini@kgk.wombat.local", "Anele", "Dlamini", Kgk, [], [], [WombatRoles.PendingTrainee]))
            .With(new UserIdentityDetails("gone", "gone@kgk.wombat.local", "Zanele", "Abrahams", Kgk, [], [], [WombatRoles.PendingTrainee], IsDeactivated: true))
            .With(new UserIdentityDetails("other", "x@other", "Nandi", "Aaron", Other, [], [], [WombatRoles.PendingTrainee]));

        var rows = await new ListPendingTraineesQueryHandler(db, directory)
            .Handle(new ListPendingTraineesQuery(TestPrincipals.InstitutionalAdmin(Kgk)), CancellationToken.None);

        rows.Select(row => row.UserId).Should().Equal("dlamini", "molefe");
        rows[0].InvitedAs.Should().BeNull("S5: invited to none reads as a dash on the page");
        rows[0].RegisteredOn.Should().BeNull();
        rows[1].InvitedAs.Should().Be("Paediatrics / Paediatrics");
        rows[1].RegisteredOn.Should().Be(new DateOnly(2026, 10, 19));
        rows[1].InstitutionName.Should().Be("Kgosi Kgari Teaching Hospital");
    }

    /// <summary>T306's admission half: Current trainees' training year, on the South African day (22:30 UTC is tomorrow there).</summary>
    [Fact]
    public async Task CurrentTrainees_CarryTheTrainingYear_OnTheSouthAfricanDay()
    {
        await using var db = Seeded();
        db.TraineeProfiles.Add(new TraineeProfile
        {
            Id = 1, UserId = "molefe", InstitutionId = Kgk, CurriculumId = PaedCurriculum, AdoptionId = 1,
            ProgrammeStartDate = new DateOnly(2026, 1, 15), ExpectedCompletionDate = new DateOnly(2027, 1, 15), IsActive = true
        });
        await db.SaveChangesAsync();
        var users = Directory(Molefe() with { Roles = [WombatRoles.Trainee] });
        var lateEvening = new FixedClock(new DateTimeOffset(2027, 1, 14, 22, 30, 0, TimeSpan.Zero));

        var profiles = await new ListTraineesForSpecialityQueryHandler(db, users.Object, lateEvening)
            .Handle(new ListTraineesForSpecialityQuery(TestPrincipals.Administrator()), CancellationToken.None);

        profiles.Single().TrainingYear.Should().Be(2);
        profiles.Single().InstitutionName.Should().Be("Kgosi Kgari Teaching Hospital");
    }

    // ── The pending registrar's Home (P01–P07) ───────────────────────────────────

    [Fact]
    public async Task PendingHome_NamesTheInstitutionalAdminWhoInvitedHer_OnTheIssueDay()
    {
        await using var db = Seeded();
        // Issued 2026-10-12 (a resend's link keeps its own issue time, F2); she registered on 2026-10-19: the issue day is
        // named (check rule 7), not UsedOn. The stored address in another case (S4).
        db.Set<Invitation>().Add(UsedInvitation(
            "MOLEFE@kgk.wombat.local",
            usedOn: new DateTime(2026, 10, 19, 7, 0, 0, DateTimeKind.Utc),
            issuedOn: new DateTime(2026, 10, 11, 22, 30, 0, DateTimeKind.Utc),
            issuedAs: WombatRoles.InstitutionalAdmin));
        await db.SaveChangesAsync();

        var pending = await PendingHome(db, Mbatha());

        pending.Should().BeEquivalentTo(new PendingAdmissionDto(
            "molefe@kgk.wombat.local",
            "Kgosi Kgari Teaching Hospital",
            "Paediatrics",
            "Paediatrics / Paediatrics",
            "Paediatric EPA Curriculum",
            "11.1",
            "Nolwazi Mbatha",
            new DateOnly(2026, 10, 12)));
    }

    [Fact]
    public async Task PendingHome_AnAdministratorsIssue_NamesNobody()
    {
        await using var db = Seeded();
        db.Set<Invitation>().Add(UsedInvitation("molefe@kgk.wombat.local", usedOn: DateTime.UtcNow, issuedAs: WombatRoles.Administrator));
        await db.SaveChangesAsync();

        var pending = await PendingHome(db, Mbatha());

        pending!.InvitedByName.Should().BeNull();
        pending.InvitedOn.Should().BeNull();
        pending.CurriculumName.Should().Be("Paediatric EPA Curriculum");
    }

    [Fact]
    public async Task PendingHome_ADeactivatedOrErasedIssuer_IsNotNamed()
    {
        await using var db = Seeded();
        db.Set<Invitation>().Add(UsedInvitation("molefe@kgk.wombat.local", usedOn: DateTime.UtcNow, issuedAs: WombatRoles.InstitutionalAdmin));
        await db.SaveChangesAsync();

        (await PendingHome(db, Mbatha() with { IsDeactivated = true }))!.InvitedByName.Should().BeNull();
        (await PendingHome(db, issuer: null))!.InvitedByName.Should().BeNull();
    }

    [Fact]
    public async Task PendingHome_InvitedToNone_HasNoProgramme_AndNamesNoDiscipline()
    {
        await using var db = Seeded();

        var pending = await PendingHome(db, Mbatha(), Molefe() with { SpecialityIds = [], SubSpecialityIds = [] });

        pending!.SpecialityName.Should().BeNull();
        pending.InvitedAs.Should().BeNull();
        pending.CurriculumName.Should().BeNull();
        pending.InstitutionName.Should().Be("Kgosi Kgari Teaching Hospital");
    }

    [Fact]
    public async Task PendingHome_NothingAdoptedForHerSubSpeciality_NamesNoCurriculum()
    {
        await using var db = Seeded(adoptPaediatrics: false);

        var pending = await PendingHome(db, Mbatha());

        pending!.InvitedAs.Should().Be("Paediatrics / Paediatrics");
        pending.CurriculumName.Should().BeNull();
    }

    // ── Fixtures ─────────────────────────────────────────────────────────────────

    private static async Task<PendingAdmissionDto?> PendingHome(ApplicationDbContext db, UserIdentityDetails? issuer, UserIdentityDetails? registrar = null)
    {
        var directory = new FakeUserDirectory().With(registrar ?? Molefe());
        if (issuer is not null)
        {
            directory.With(issuer);
        }

        var summary = await new GetTraineeDashboardSummaryQueryHandler(db, new WorkflowEvaluator(), directory, Today).Handle(
            new GetTraineeDashboardSummaryQuery(TestPrincipals.InRoles([WombatRoles.PendingTrainee], "molefe", Kgk)),
            CancellationToken.None);

        summary.IsPendingTrainee.Should().BeTrue();
        return summary.Pending;
    }

    private static AdmitTraineeCommandHandler Handler(ApplicationDbContext db, Mock<IUserAdministrationService> users, TestNotices? notices = null)
        => new(db, users.Object, (notices ?? new TestNotices(users.Object)).Notices, Today);

    private static AdmitTraineeCommand Admit(int curriculumId, DateOnly start, DateOnly? completion)
        => new("molefe", curriculumId, start, completion, TestPrincipals.InstitutionalAdmin(Kgk));

    private static UserIdentityDetails Molefe() => new(
        "molefe", "molefe@kgk.wombat.local", "Lerato", "Molefe", Kgk, [Paediatrics], [GeneralPaediatrics], [WombatRoles.PendingTrainee]);

    private static UserIdentityDetails Mbatha() => new(
        "mbatha", "mbatha@kgk.wombat.local", "Nolwazi", "Mbatha", Kgk, [], [], [WombatRoles.InstitutionalAdmin]);

    private static Mock<IUserAdministrationService> Directory(params UserIdentityDetails[] users)
    {
        var directory = new Mock<IUserAdministrationService>().RunsTransactions();
        foreach (var user in users)
        {
            directory.Setup(service => service.GetByIdAsync(user.UserId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        }

        return directory;
    }

    private static Invitation UsedInvitation(string email, DateTime usedOn, DateTime? issuedOn = null, string? issuedAs = null) => new()
    {
        Email = email,
        TokenHash = Guid.NewGuid().ToString("N"),
        TargetRole = WombatRoles.Trainee,
        InstitutionId = Kgk,
        SpecialityId = Paediatrics,
        SubSpecialityId = GeneralPaediatrics,
        IssuedByUserId = "mbatha",
        IssuedAsRole = issuedAs,
        IssuedOn = issuedOn ?? usedOn.AddHours(-1),
        ExpiresOn = new DateOnly(2026, 11, 2),
        UsedOn = usedOn
    };

    private static ApplicationDbContext Seeded(bool adoptPaediatrics = true, bool adoptNeonatology = true)
    {
        var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

        db.Set<Institution>().AddRange(
            new Institution { Id = Kgk, Name = "Kgosi Kgari Teaching Hospital", ShortCode = "KGK", IsActive = true, CreatedOn = DateTime.UtcNow },
            new Institution { Id = Other, Name = "Other Hospital", ShortCode = "OTH", IsActive = true, CreatedOn = DateTime.UtcNow });
        db.Set<Speciality>().Add(new Speciality { Id = Paediatrics, CollegeId = 1, Name = "Paediatrics", IsActive = true });
        db.Set<SubSpeciality>().AddRange(
            new SubSpeciality { Id = GeneralPaediatrics, SpecialityId = Paediatrics, Name = "Paediatrics", IsActive = true },
            new SubSpeciality { Id = Neonatology, SpecialityId = Paediatrics, Name = "Neonatology", IsActive = true });
        db.Set<Curriculum>().AddRange(
            new Curriculum
            {
                Id = PaedCurriculum, SubSpecialityId = GeneralPaediatrics, Name = "Paediatric EPA Curriculum", Version = "11.1",
                Items = [new CurriculumItem { Id = 4000, WindowMonths = 12, RequiredCount = 1, MinimumLevelOrder = 3, EpaId = 5000 }]
            },
            new Curriculum { Id = NeoCurriculum, SubSpecialityId = Neonatology, Name = "Neonatal Curriculum", Version = "1.0" });
        if (adoptPaediatrics)
        {
            db.Set<InstitutionCurriculumAdoption>().Add(new InstitutionCurriculumAdoption
            {
                Id = 1, InstitutionId = Kgk, CurriculumId = PaedCurriculum, SubSpecialityId = GeneralPaediatrics, AdoptedOn = new DateOnly(2026, 1, 1), IsActive = true
            });
        }

        if (adoptNeonatology)
        {
            db.Set<InstitutionCurriculumAdoption>().Add(new InstitutionCurriculumAdoption
            {
                Id = 2, InstitutionId = Kgk, CurriculumId = NeoCurriculum, SubSpecialityId = Neonatology, AdoptedOn = new DateOnly(2026, 1, 1), IsActive = true
            });
        }

        db.SaveChanges();
        return db;
    }

    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
