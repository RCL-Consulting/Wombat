using System.Security.Claims;
using FluentAssertions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Application.Features.Programme.Trainees;
using Wombat.Application.Features.Reporting;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;
using static Wombat.Application.Tests.Features.Programme.ProgrammeCast;

namespace Wombat.Application.Tests.Features.Reporting;

/// <summary>
/// T379 (flow 13, lane E): whose portfolio Export portfolio exports, read before its form (<see cref="GetExportSubjectQuery" />;
/// M4, F6, Q19), and <c>MayExport</c> on the committee review and the registrar's programme page (F1, M1): the export
/// command's own check, so no page offers what the command refuses, and an Administrator is never refused.
/// </summary>
public sealed class ExportSubjectQueryTests
{
    // 2026-10-04 10:00 SAST, the cast's day.
    private static readonly TimeProvider Clock = new FixedClock(new DateTimeOffset(2026, 10, 4, 8, 0, 0, TimeSpan.Zero));

    [Fact]
    public async Task HerOwnExport_ReadsHerProgramme()
    {
        var (db, users) = Build();
        await using var _ = db;

        var subject = await SubjectAsync(db, users, null, TestPrincipals.Trainee(Molefe, Kgk));

        subject.Should().Be(new ExportSubjectDto(
            Molefe, "Lerato Molefe", "Kgosi Kgari Teaching Hospital", "Paediatrics", "11.1", 4,
            new DateOnly(2023, 1, 15), null, MolefeProfile));
    }

    [Fact]
    public async Task TheOwnExport_OfACallerWithNoProfile_IsNull()
    {
        // F6, R3-E19: David Naidoo, acting as Assessor, types /portfolio/export.
        var (db, users) = Build();
        await using var _ = db;
        users.With(new UserIdentityDetails("naidoo", "naidoo@kgk.wombat.local", "David", "Naidoo", Kgk, [], [], [WombatRoles.Assessor]));

        (await SubjectAsync(db, users, null, TestPrincipals.InRole(WombatRoles.Assessor, "naidoo", Kgk))).Should().BeNull();
    }

    public static TheoryData<string> StaffWhoMayExport() =>
    [
        "coordinator", "committee member", "speciality admin", "sub-speciality admin", "institutional admin", "administrator",
    ];

    [Theory]
    [MemberData(nameof(StaffWhoMayExport))]
    public async Task AMemberOfStaffWhoOverseesHer_ReadsTheSubject(string who)
    {
        var (db, users) = Build();
        await using var _ = db;

        var subject = await SubjectAsync(db, users, Molefe, Staff(who));

        subject.Should().NotBeNull(who);
        subject!.Name.Should().Be("Lerato Molefe");
        subject.ProfileId.Should().Be(MolefeProfile);
    }

    [Fact]
    public async Task EveryRefusedCause_IsNull()
    {
        var (db, users) = Build();
        await using var _ = db;
        db.TraineeProfiles.Find(NdlovuProfile)!.Erase("erased-pseudonym", D);
        db.SaveChanges();

        (await SubjectAsync(db, users, Elsewhere, Coordinator())).Should().BeNull("another institution's registrar");
        (await SubjectAsync(db, users, Molefe, Coordinator(institutionId: OtherHospital))).Should().BeNull("out of scope");
        (await SubjectAsync(db, users, Molefe, TestPrincipals.Trainee(Dlamini, Kgk))).Should().BeNull("a peer's portfolio");
        (await SubjectAsync(db, users, Molefe, TestPrincipals.InRoles([WombatRoles.Trainee, WombatRoles.Coordinator], "smit", Kgk)))
            .Should().BeNull("a holder of Trainee reads only their own (T185), as the command does");
        (await SubjectAsync(db, users, "no-such-user", TestPrincipals.Administrator())).Should().BeNull("an unknown id");
        (await SubjectAsync(db, users, "erased-pseudonym", TestPrincipals.Administrator()))
            .Should().BeNull("an erased person's pseudonym no account holds");
    }

    [Fact]
    public async Task AnEndedProgramme_GivesItsLastDay_AndTheYearItEndedIn()
    {
        var (db, users) = Build();
        await using var _ = db;
        db.TraineeProfiles.Find(MolefeProfile)!.Complete(new DateOnly(2026, 10, 2), D);
        db.SaveChanges();

        var own = await SubjectAsync(db, users, null, TestPrincipals.InRoles([], Molefe, Kgk));
        var staff = await SubjectAsync(db, users, Molefe, Coordinator());

        own.Should().NotBeNull("a former trainee holding no role exports her own record");
        own!.LastDay.Should().Be(new DateOnly(2026, 10, 2));
        own.TrainingYear.Should().Be(4);
        own.ProfileId.Should().Be(MolefeProfile);
        staff.Should().Be(own);
    }

    [Fact]
    public async Task TheProfileId_IsThePreferredProfiles()
    {
        // An older, ended programme beside the running one: the running one is the preferred profile.
        var (db, users) = Build();
        await using var _ = db;
        var older = new TraineeProfile
        {
            Id = 90, UserId = Molefe, InstitutionId = Kgk, CurriculumId = PaediatricsCurriculum,
            ProgrammeStartDate = new DateOnly(2019, 1, 15), AdmittedOn = new DateOnly(2019, 1, 15),
            ExpectedCompletionDate = new DateOnly(2023, 1, 15)
        };
        older.Deactivate(new DateOnly(2020, 6, 30), D);
        db.TraineeProfiles.Add(older);
        db.SaveChanges();

        var subject = await SubjectAsync(db, users, Molefe, Coordinator());

        subject!.ProfileId.Should().Be(MolefeProfile);
        subject.LastDay.Should().BeNull();
    }

    [Fact]
    public async Task APersonWithNoProfile_TheAdministratorMayExport_HasNoProfileId()
    {
        // Check build note 6: the staff trail falls back to Home › Export portfolio.
        var (db, users) = Build();
        await using var _ = db;
        users.With(new UserIdentityDetails("naidoo", "naidoo@kgk.wombat.local", "David", "Naidoo", Kgk, [], [], [WombatRoles.Assessor]));

        var subject = await SubjectAsync(db, users, "naidoo", TestPrincipals.Administrator());

        subject.Should().Be(new ExportSubjectDto("naidoo", "David Naidoo", null, null, null, null, null, null, null));
    }

    [Fact]
    public void FromAfterTo_IsRefusedOnFrom_InTheBuiltWords()
    {
        // S6, R3-E11: the page maps the refusal to its field by its property, never by its words.
        var result = new ExportPortfolioCommandValidator().Validate(new ExportPortfolioCommand(
            Molefe, new DateOnly(2026, 11, 2), new DateOnly(2026, 10, 26), TestPrincipals.Trainee(Molefe, Kgk)));

        result.Errors.Should().ContainSingle().Which.Should().Match<FluentValidation.Results.ValidationFailure>(failure =>
            failure.PropertyName == nameof(ExportPortfolioCommand.FromDate) &&
            failure.ErrorMessage == "The start date must be before or equal to the end date.");
    }

    // ─── MayExport (F1, M1) ─────────────────────────────────────────────────

    [Fact]
    public async Task TheProgrammePage_MayExport_WhereTheCommandWouldNotRefuse()
    {
        var (db, users) = Build();
        await using var _ = db;
        var handler = new GetProgrammeTraineeQueryHandler(db, users, Clock);

        (await handler.Handle(new GetProgrammeTraineeQuery(CommitteeMember(), WombatRoles.CommitteeMember, MolefeProfile, D), default))!
            .MayExport.Should().BeTrue();
        (await handler.Handle(new GetProgrammeTraineeQuery(Coordinator(), WombatRoles.Coordinator, MolefeProfile, D), default))!
            .MayExport.Should().BeTrue();

        // After the end too (A4, R3-X04): an ended record stays exportable by the staff who oversaw it.
        db.TraineeProfiles.Find(MolefeProfile)!.Complete(new DateOnly(2026, 10, 2), D);
        db.SaveChanges();
        (await handler.Handle(new GetProgrammeTraineeQuery(Coordinator(), WombatRoles.Coordinator, MolefeProfile, D), default))!
            .MayExport.Should().BeTrue();
    }

    [Fact]
    public async Task TheReviewPage_MayExport_ForTheAdministrator_AndStaffWhoOverseeTheTrainee()
    {
        var (db, users) = Build();
        await using var _ = db;
        AddPanel(db, 1, Kgk, "zulu");
        AddReview(db, 1, 1, Molefe, D.AddDays(7));
        db.SaveChanges();
        var handler = new GetCommitteeReviewByIdQueryHandler(db, users);

        (await handler.Handle(new GetCommitteeReviewByIdQuery(1, TestPrincipals.InstitutionalAdmin(Kgk)), default))
            .MayExport.Should().BeTrue();
        (await handler.Handle(new GetCommitteeReviewByIdQuery(1, Coordinator()), default)).MayExport.Should().BeTrue();
        (await handler.Handle(new GetCommitteeReviewByIdQuery(1, TestPrincipals.Administrator()), default))
            .MayExport.Should().BeTrue();
    }

    [Fact]
    public async Task TheReviewOfATraineeElsewhere_MayNotExport_ButTheAdministratorIsNeverRefused()
    {
        // R3-X08: the trainee has moved to another institution; the panel's staff read the review but no longer oversee her.
        var (db, users) = Build();
        await using var _ = db;
        AddPanel(db, 1, Kgk, "zulu");
        AddReview(db, 1, 1, Molefe, D.AddDays(7));
        db.TraineeProfiles.Find(MolefeProfile)!.InstitutionId = OtherHospital;
        db.SaveChanges();
        var handler = new GetCommitteeReviewByIdQueryHandler(db, users);

        var coordinator = await handler.Handle(new GetCommitteeReviewByIdQuery(1, Coordinator()), default);
        coordinator.TraineeElsewhere.Should().NotBeNull("guard: the review's trainee is elsewhere");
        coordinator.MayExport.Should().BeFalse();
        (await handler.Handle(new GetCommitteeReviewByIdQuery(1, TestPrincipals.InstitutionalAdmin(Kgk)), default))
            .MayExport.Should().BeFalse("an Institutional admin of another institution than hers");
        (await handler.Handle(new GetCommitteeReviewByIdQuery(1, TestPrincipals.Administrator()), default))
            .MayExport.Should().BeTrue();
    }

    [Fact]
    public async Task TheReviewOfAnErasedTrainee_OffersNoExport_SinceTheExportWouldReadNoSubject()
    {
        // The T379 replay's A.1.14: the erased registrar's review offered "Export deleted_user_…'s portfolio", which led only
        // to Portfolio unavailable. Her profile stays; her account does not.
        var (db, users) = Build();
        await using var _ = db;
        AddRegistrar(db, new Wombat.Tests.Shared.FakeUserDirectory(), 99, "deleted_user_d5fe50c8", "", "", trainingYear: 1);
        AddPanel(db, 1, Kgk, "zulu");
        AddReview(db, 1, 1, "deleted_user_d5fe50c8", D.AddDays(7));
        db.SaveChanges();
        var handler = new GetCommitteeReviewByIdQueryHandler(db, users);

        (await handler.Handle(new GetCommitteeReviewByIdQuery(1, Coordinator()), default)).MayExport.Should().BeFalse();
        (await handler.Handle(new GetCommitteeReviewByIdQuery(1, TestPrincipals.Administrator()), default))
            .MayExport.Should().BeFalse("no account, so no subject to export, for anyone");
    }

    private static ClaimsPrincipal Staff(string who) => who switch
    {
        "coordinator" => Coordinator(),
        "committee member" => CommitteeMember(),
        "speciality admin" => SpecialityAdmin(),
        "sub-speciality admin" => SubSpecialityAdmin(),
        "institutional admin" => TestPrincipals.InstitutionalAdmin(Kgk),
        _ => TestPrincipals.Administrator(),
    };

    private static Task<ExportSubjectDto?> SubjectAsync(
        ApplicationDbContext db, IUserAdministrationService users, string? traineeUserId, ClaimsPrincipal principal)
        => new GetExportSubjectQueryHandler(db, users, Clock).Handle(
            new GetExportSubjectQuery(traineeUserId, principal), CancellationToken.None);

    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
