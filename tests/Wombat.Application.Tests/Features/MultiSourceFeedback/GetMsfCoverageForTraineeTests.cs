using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.MultiSourceFeedback;

/// <summary>
/// Per EPA and per semester, whether a released MSF campaign covering the EPA closed in the semester (T168). D9: "MSF
/// completed for EPA 7" means a campaign covering EPA 7 was released this period. Released only, and only the EPAs the
/// release recorded evidence for, bucketed by the UTC day the campaign closed.
/// </summary>
public sealed class GetMsfCoverageForTraineeTests
{
    private const string TraineeUserId = "trainee-1";
    private const int HostInstitution = 1;
    private const int OtherInstitution = 2;
    private const int CurriculumId = 10;
    private const int OtherCurriculumId = 11;
    private const int TemplateId = 40;

    private static readonly DateOnly Year2026From = new(2026, 1, 1);
    private static readonly DateOnly Year2026To = new(2026, 12, 31);

    // ─── Which campaigns ─────────────────────────────────────────────────────

    [Fact]
    public async Task AReleasedCampaignCoveringTwoEpas_CoversBothInItsSemester_AndNeitherInTheNext()
    {
        await using var db = CreateDb();
        await SeedAsync(db, [Item(1, "PAED-001"), Item(2, "PAED-002"), Item(7, "PAED-007")]);
        db.MsfCampaigns.Add(Campaign(50, MsfCampaignState.Released, Utc(2026, 3, 10, 9), (1, true), (7, true)));
        await db.SaveChangesAsync();

        var coverage = await ReadAsync(db, Self(), Year2026From, Year2026To);

        coverage!.Periods.Select(period => period.Name).Should().Equal("Semester 1, 2026", "Semester 2, 2026");
        Covered(coverage, "PAED-001").Should().Equal((2026, 1, 50));
        Covered(coverage, "PAED-007").Should().Equal((2026, 1, 50));
        Covered(coverage, "PAED-002").Should().BeEmpty("the campaign did not cover it");

        var paed001 = coverage.Epas.Single(epa => epa.EpaCode == "PAED-001");
        paed001.For(2026, 1)!.Latest.Should().Be(new MsfCoveringCampaignDto(50, "Annual MSF", new DateOnly(2026, 3, 10), new DateOnly(2026, 3, 13)));
        paed001.For(2026, 2)!.IsCovered.Should().BeFalse();
        coverage.Periods.Select(period => period.EpasCovered).Should().Equal(2, 0);
    }

    [Theory]
    [InlineData(MsfCampaignState.Draft)]
    [InlineData(MsfCampaignState.Open)]
    [InlineData(MsfCampaignState.Closed)]
    [InlineData(MsfCampaignState.UnderReview)]
    [InlineData(MsfCampaignState.Withdrawn)]
    public async Task ACampaignThatIsNotReleased_CoversNothing(MsfCampaignState state)
    {
        await using var db = CreateDb();
        await SeedAsync(db, [Item(1, "PAED-001")]);
        // Closed in the span and marked recorded, so only the state can be what leaves it out.
        db.MsfCampaigns.Add(Campaign(50, state, Utc(2026, 3, 10, 9), (1, true)));
        await db.SaveChangesAsync();

        var coverage = await ReadAsync(db, Self(), Year2026From, Year2026To);

        Covered(coverage!, "PAED-001").Should().BeEmpty("before release the coordinator may still withdraw it, and a withdrawn one was retracted");
    }

    [Fact]
    public async Task ADeclaredEpaTheReleaseRecordedNoEvidenceFor_IsNotCovered()
    {
        await using var db = CreateDb();
        await SeedAsync(db, [Item(1, "PAED-001"), Item(3, "PAED-003")]);
        // PAED-003 was declared but had left the curriculum at release, so the release dropped it.
        db.MsfCampaigns.Add(Campaign(50, MsfCampaignState.Released, Utc(2026, 3, 10, 9), (1, true), (3, false)));
        await db.SaveChangesAsync();

        var coverage = await ReadAsync(db, Self(), Year2026From, Year2026To);

        Covered(coverage!, "PAED-001").Should().Equal((2026, 1, 50));
        Covered(coverage!, "PAED-003").Should().BeEmpty("no msf_cpsa evidence row was written for it");
    }

    [Fact]
    public async Task TheSemesterIsTheUtcDayTheCampaignClosed_NotTheDayItWasScheduledToOrReleased()
    {
        await using var db = CreateDb();
        await SeedAsync(db, [Item(1, "PAED-001"), Item(2, "PAED-002")]);

        // Scheduled to close in July, closed early on the last UTC day of June (already 1 July in South Africa), and
        // released in July: semester 1, the day its evidence is dated.
        var closedEarly = Campaign(50, MsfCampaignState.Released, Utc(2026, 6, 30, 23), (1, true));
        closedEarly.ClosesOn = new DateOnly(2026, 7, 10);
        closedEarly.ReleasedOn = Utc(2026, 7, 3, 9);

        // Scheduled for the last day of June, auto-closed just after midnight UTC: semester 2.
        var closedLate = Campaign(51, MsfCampaignState.Released, Utc(2026, 7, 1, 0), (2, true));
        closedLate.ClosesOn = new DateOnly(2026, 6, 30);

        db.MsfCampaigns.AddRange(closedEarly, closedLate);
        await db.SaveChangesAsync();

        var coverage = await ReadAsync(db, Self(), Year2026From, Year2026To);

        Covered(coverage!, "PAED-001").Should().Equal((2026, 1, 50));
        Covered(coverage!, "PAED-002").Should().Equal((2026, 2, 51));
        coverage!.Epas.Single(epa => epa.EpaCode == "PAED-001").For(2026, 1)!.Latest!.ClosedOn.Should().Be(new DateOnly(2026, 6, 30));
    }

    [Fact]
    public async Task AnotherTraineesCampaign_CoversNothingForThisOne()
    {
        await using var db = CreateDb();
        await SeedAsync(db, [Item(1, "PAED-001")]);
        db.MsfCampaigns.Add(Campaign(50, MsfCampaignState.Released, Utc(2026, 3, 10, 9), subjectUserId: "trainee-2", epas: (1, true)));
        await db.SaveChangesAsync();

        var coverage = await ReadAsync(db, Self(), Year2026From, Year2026To);

        Covered(coverage!, "PAED-001").Should().BeEmpty();
    }

    [Fact]
    public async Task TwoCampaignsInOneSemester_AreBothListed_TheLatestFirst()
    {
        await using var db = CreateDb();
        await SeedAsync(db, [Item(1, "PAED-001")]);
        db.MsfCampaigns.AddRange(
            Campaign(50, MsfCampaignState.Released, Utc(2026, 2, 10, 9), (1, true)),
            Campaign(51, MsfCampaignState.Released, Utc(2026, 5, 20, 9), (1, true)));
        await db.SaveChangesAsync();

        var cell = (await ReadAsync(db, Self(), Year2026From, Year2026To))!.Epas.Single().For(2026, 1)!;

        cell.Campaigns.Select(campaign => campaign.CampaignId).Should().Equal(51, 50);
        cell.Latest!.ClosedOn.Should().Be(new DateOnly(2026, 5, 20));
    }

    // ─── Which semesters ─────────────────────────────────────────────────────

    [Fact]
    public async Task AReviewPeriodAcrossTwoAcademicYears_ReadsEachSemesterItTouches_AndDecemberIsSemesterTwo()
    {
        await using var db = CreateDb();
        await SeedAsync(db, [Item(1, "PAED-001"), Item(2, "PAED-002"), Item(3, "PAED-003"), Item(4, "PAED-004")]);
        db.MsfCampaigns.AddRange(
            Campaign(50, MsfCampaignState.Released, Utc(2025, 12, 5, 9), (1, true)),
            Campaign(51, MsfCampaignState.Released, Utc(2026, 4, 1, 9), (2, true)),
            // The day after the span's last semester: outside it.
            Campaign(52, MsfCampaignState.Released, Utc(2026, 7, 1, 9), (3, true)),
            // The day before its first: outside it.
            Campaign(53, MsfCampaignState.Released, Utc(2025, 6, 30, 9), (3, true)),
            // The first and the last half hour the span's semesters hold: inside it, though the review period itself
            // starts a month later.
            Campaign(54, MsfCampaignState.Released, Utc(2025, 7, 1, 0), (4, true)),
            Campaign(55, MsfCampaignState.Released, Utc(2026, 6, 30, 23), (4, true)));
        await db.SaveChangesAsync();

        var coverage = await ReadAsync(db, Self(), new DateOnly(2025, 8, 1), new DateOnly(2026, 6, 30));

        coverage!.Periods.Select(period => (period.Year, period.Semester)).Should().Equal((2025, 2), (2026, 1));
        Covered(coverage, "PAED-001").Should().Equal((2025, 2, 50));
        Covered(coverage, "PAED-002").Should().Equal((2026, 1, 51));
        Covered(coverage, "PAED-003").Should().BeEmpty("both of its campaigns closed outside the semesters read");
        Covered(coverage, "PAED-004").Should().Equal([(2025, 2, 54), (2026, 1, 55)], "a semester is read whole, to its edges");
        coverage.Periods.Select(period => period.HasEnded).Should().Equal(true, true);
    }

    [Fact]
    public async Task WithNoSpanGiven_ItReadsTheSemesterBeforeAndTheCurrentOne()
    {
        await using var db = CreateDb();
        await SeedAsync(db, [Item(1, "PAED-001")]);
        db.MsfCampaigns.AddRange(
            Campaign(50, MsfCampaignState.Released, Utc(2025, 11, 15, 9), (1, true)),
            Campaign(51, MsfCampaignState.Released, Utc(2026, 3, 10, 9), (1, true)));
        await db.SaveChangesAsync();

        var coverage = await ReadAsync(db, Self(), from: null, to: new DateOnly(2026, 9, 1));

        coverage!.To.Should().Be(new DateOnly(2026, 9, 1));
        coverage.Periods.Select(period => period.Name).Should().Equal("Semester 1, 2026", "Semester 2, 2026");
        Covered(coverage, "PAED-001").Should().Equal((2026, 1, 51));
    }

    [Fact]
    public async Task WithNoSpanGiven_ASemesterThatEndedBeforeTheProgrammeStarted_IsLeftOut()
    {
        await using var db = CreateDb();
        await SeedAsync(db, [Item(1, "PAED-001")], programmeStart: new DateOnly(2026, 2, 1));
        await db.SaveChangesAsync();

        var coverage = await ReadAsync(db, Self(), from: null, to: new DateOnly(2026, 3, 1));

        coverage!.Periods.Select(period => period.Name).Should().Equal("Semester 1, 2026");
    }

    [Fact]
    public async Task ASemesterStillRunning_HasNotEnded()
    {
        await using var db = CreateDb();
        await SeedAsync(db, [Item(1, "PAED-001")]);
        await db.SaveChangesAsync();

        var coverage = await ReadAsync(db, Self(), new DateOnly(2099, 1, 1), new DateOnly(2099, 3, 1));

        coverage!.Periods.Should().ContainSingle().Which.HasEnded.Should().BeFalse();
    }

    [Fact]
    public async Task ASpanEndingInSemesterTwo_ReadsItTo31December_TheDecemberFold()
    {
        await using var db = CreateDb();
        await SeedAsync(db, [Item(1, "PAED-001"), Item(2, "PAED-002"), Item(3, "PAED-003")]);
        db.MsfCampaigns.AddRange(
            // After the College's 30 November, inside the bucket (D40): semester 2.
            Campaign(50, MsfCampaignState.Released, Utc(2026, 12, 15, 9), (1, true)),
            // The bucket's last half minute.
            Campaign(51, MsfCampaignState.Released, new DateTime(2026, 12, 31, 23, 59, 30, DateTimeKind.Utc), (2, true)),
            // The next year's first instant: outside it.
            Campaign(52, MsfCampaignState.Released, new DateTime(2027, 1, 1, 0, 0, 0, DateTimeKind.Utc), (3, true)));
        await db.SaveChangesAsync();

        // The committee's span, a year of whole semesters; and the progress page's, which ends on the day read as today.
        var spanned = await ReadAsync(db, Self(), Year2026From, Year2026To);
        var progressPage = await ReadAsync(db, Self(), from: null, to: null, asOf: new DateOnly(2026, 12, 31));

        foreach (var coverage in new[] { spanned!, progressPage! })
        {
            coverage.Periods[^1].Name.Should().Be("Semester 2, 2026");
            Covered(coverage, "PAED-001").Should().Equal((2026, 2, 50));
            Covered(coverage, "PAED-002").Should().Equal((2026, 2, 51));
            Covered(coverage, "PAED-003").Should().BeEmpty("it closed in 2027");
            coverage.Periods[^1].EpasCovered.Should().Be(2);
        }
    }

    [Theory]
    [InlineData("2026-06-30", false, false)]
    [InlineData("2026-07-01", true, false)]
    [InlineData("2026-11-30", true, false)]
    [InlineData("2026-12-15", true, false)]
    [InlineData("2026-12-31", true, false)]
    [InlineData("2027-01-01", true, true)]
    public async Task ASemesterHasEnded_OnlyOnceItsLastDay_31DecemberForSemesterTwo_IsBehindTheDayReadAsToday(
        string asOf, bool firstEnded, bool secondEnded)
    {
        await using var db = CreateDb();
        await SeedAsync(db, [Item(1, "PAED-001")]);
        await db.SaveChangesAsync();

        var coverage = await ReadAsync(db, Self(), Year2026From, Year2026To, DateOnly.Parse(asOf, System.Globalization.CultureInfo.InvariantCulture));

        // A campaign can still close in semester 2 in December, so it is still running then: "yet" still holds.
        coverage!.Periods.Select(period => period.HasEnded).Should().Equal(firstEnded, secondEnded);
    }

    [Fact]
    public async Task WithNoSpanGiven_TheSpanEndsOnTheDayReadAsToday()
    {
        await using var db = CreateDb();
        await SeedAsync(db, [Item(1, "PAED-001")]);
        db.MsfCampaigns.Add(Campaign(50, MsfCampaignState.Released, Utc(2026, 12, 20, 9), (1, true)));
        await db.SaveChangesAsync();

        var coverage = await ReadAsync(db, Self(), from: null, to: null, asOf: new DateOnly(2027, 2, 1));

        coverage!.To.Should().Be(new DateOnly(2027, 2, 1));
        coverage.Periods.Select(period => (period.Name, period.HasEnded)).Should().Equal(("Semester 2, 2026", true), ("Semester 1, 2027", false));
        Covered(coverage, "PAED-001").Should().Equal((2026, 2, 50));
    }

    // ─── Which EPAs ──────────────────────────────────────────────────────────

    [Fact]
    public async Task TheRowsAreTheTraineesCurriculumInForce_WithOnlyTheirOwnInstitutionsLocalItems()
    {
        await using var db = CreateDb();
        await SeedAsync(
            db,
            [
                Item(1, "PAED-001"),
                Item(2, "HOST-002", owningInstitutionId: HostInstitution),
                Item(3, "ELSE-003", owningInstitutionId: OtherInstitution),
                Item(4, "PAED-004"),
                Item(5, "PAED-005", curriculumId: OtherCurriculumId)
            ]);
        (await db.Epas.SingleAsync(epa => epa.Id == 4)).IsActive = false;
        await db.SaveChangesAsync();

        // An Administrator signed in at the other institution: the trainee's institution decides the local items.
        var administrator = TestPrincipals.InRole(WombatRoles.Administrator, "admin-elsewhere", OtherInstitution);
        var coverage = await ReadAsync(db, administrator, Year2026From, Year2026To);

        coverage!.Epas.Select(epa => (epa.EpaCode, epa.IsLocal)).Should().Equal(
            [("HOST-002", true), ("PAED-001", false)],
            "another institution's local item, a deactivated EPA (T158) and another curriculum's item are not this trainee's");
    }

    // ─── Who may ask ─────────────────────────────────────────────────────────

    [Fact]
    public async Task SomeoneWhoMayNotReadAboutTheTrainee_GetsNull()
    {
        await using var db = CreateDb();
        await SeedAsync(db, [Item(1, "PAED-001")]);
        db.MsfCampaigns.Add(Campaign(50, MsfCampaignState.Released, Utc(2026, 3, 10, 9), (1, true)));
        await db.SaveChangesAsync();

        (await ReadAsync(db, TestPrincipals.Coordinator(OtherInstitution), Year2026From, Year2026To)).Should().BeNull();
        (await ReadAsync(db, TestPrincipals.InRole(WombatRoles.CommitteeMember, "cm-elsewhere", OtherInstitution), Year2026From, Year2026To))
            .Should().BeNull();
        (await ReadAsync(db, TestPrincipals.Trainee("trainee-2", HostInstitution), Year2026From, Year2026To)).Should().BeNull();
        (await ReadAsync(db, TestPrincipals.InRole(WombatRoles.Assessor, "assessor-1", HostInstitution), Year2026From, Year2026To))
            .Should().BeNull("an assessor at the trainee's institution does not oversee their programme");
    }

    [Fact]
    public async Task TheTraineeAndThoseWhoOverseeTheirProgramme_ReadIt()
    {
        await using var db = CreateDb();
        await SeedAsync(db, [Item(1, "PAED-001")]);
        db.MsfCampaigns.Add(Campaign(50, MsfCampaignState.Released, Utc(2026, 3, 10, 9), (1, true)));
        await db.SaveChangesAsync();

        ClaimsPrincipal[] readers =
        [
            Self(),
            TestPrincipals.Coordinator(HostInstitution),
            TestPrincipals.InRole(WombatRoles.CommitteeMember, "cm-host", HostInstitution),
            TestPrincipals.Administrator()
        ];

        foreach (var reader in readers)
        {
            Covered((await ReadAsync(db, reader, Year2026From, Year2026To))!, "PAED-001").Should().Equal((2026, 1, 50));
        }
    }

    [Fact]
    public async Task ATraineeWithNoProfile_GetsNull()
    {
        await using var db = CreateDb();
        await SeedAsync(db, [Item(1, "PAED-001")]);
        await db.SaveChangesAsync();

        var coverage = await new GetMsfCoverageForTraineeQueryHandler(db).Handle(
            new GetMsfCoverageForTraineeQuery("nobody", TestPrincipals.Administrator(), Year2026From, Year2026To),
            CancellationToken.None);

        coverage.Should().BeNull();
    }

    // ─── The request ─────────────────────────────────────────────────────────

    [Fact]
    public void TheValidator_RefusesASpanWithNoLastDay_OneTheWrongWayRound_AndOneOverTenYears()
    {
        var validator = new GetMsfCoverageForTraineeQueryValidator();
        var principal = Self();

        validator.Validate(new GetMsfCoverageForTraineeQuery(TraineeUserId, principal)).IsValid.Should().BeTrue();
        validator.Validate(new GetMsfCoverageForTraineeQuery(TraineeUserId, principal, To: new DateOnly(2026, 9, 1))).IsValid.Should().BeTrue();
        validator.Validate(new GetMsfCoverageForTraineeQuery(TraineeUserId, principal, Year2026From, Year2026To)).IsValid.Should().BeTrue();

        validator.Validate(new GetMsfCoverageForTraineeQuery(TraineeUserId, principal, From: Year2026From)).IsValid.Should().BeFalse();
        validator.Validate(new GetMsfCoverageForTraineeQuery(TraineeUserId, principal, Year2026To, Year2026From)).IsValid.Should().BeFalse();
        validator.Validate(new GetMsfCoverageForTraineeQuery(TraineeUserId, principal, new DateOnly(2016, 1, 1), Year2026To)).IsValid.Should().BeFalse();
    }

    // ─── Fixtures ────────────────────────────────────────────────────────────

    /// <summary>Each semester an EPA is covered in, as (year, semester, latest campaign id).</summary>
    private static IEnumerable<(int Year, int Semester, int CampaignId)> Covered(MsfCoverageDto coverage, string epaCode)
        => coverage.Epas.Single(epa => epa.EpaCode == epaCode).Periods
            .Where(period => period.IsCovered)
            .Select(period => (period.Year, period.Semester, period.Latest!.CampaignId))
            .ToArray();

    private static Task<MsfCoverageDto?> ReadAsync(
        ApplicationDbContext db, ClaimsPrincipal principal, DateOnly? from, DateOnly? to, DateOnly? asOf = null)
        => new GetMsfCoverageForTraineeQueryHandler(db).Handle(
            new GetMsfCoverageForTraineeQuery(TraineeUserId, principal, from, to, asOf),
            CancellationToken.None);

    private static ClaimsPrincipal Self() => TestPrincipals.Trainee(TraineeUserId, HostInstitution);

    private static DateTime Utc(int year, int month, int day, int hour)
        => new(year, month, day, hour, 30, 0, DateTimeKind.Utc);

    private static MsfCampaign Campaign(
        int id,
        MsfCampaignState state,
        DateTime closedOn,
        params (int EpaId, bool Recorded)[] epas)
        => Campaign(id, state, closedOn, TraineeUserId, epas);

    private static MsfCampaign Campaign(
        int id,
        MsfCampaignState state,
        DateTime closedOn,
        string subjectUserId,
        params (int EpaId, bool Recorded)[] epas)
    {
        var releasedOn = closedOn.AddDays(3);
        var campaign = new MsfCampaign
        {
            Id = id,
            SubjectUserId = subjectUserId,
            TemplateId = TemplateId,
            CreatedByUserId = "coord-1",
            CreatedOn = closedOn.AddDays(-30),
            OpensOn = DateOnly.FromDateTime(closedOn.AddDays(-30)),
            ClosesOn = DateOnly.FromDateTime(closedOn),
            State = state,
            OpenedOn = closedOn.AddDays(-30),
            ClosedOn = closedOn,
            ReleasedOn = state == MsfCampaignState.Released ? releasedOn : null,
            WithdrawnOn = state == MsfCampaignState.Withdrawn ? releasedOn : null
        };

        foreach (var (epaId, recorded) in epas)
        {
            campaign.CoveredEpas.Add(new MsfCampaignEpa { EpaId = epaId, RecordedOn = recorded ? releasedOn : null });
        }

        return campaign;
    }

    private static CurriculumItem Item(
        int epaId,
        string code,
        int? owningInstitutionId = null,
        int curriculumId = CurriculumId)
        => new()
        {
            Id = 100 + epaId,
            CurriculumId = curriculumId,
            EpaId = epaId,
            Epa = new Epa { Id = epaId, SubSpecialityId = 1, Code = code, Title = $"{code} title", IsActive = true },
            RequiredCount = 3,
            QuotaPeriod = QuotaPeriod.Semester,
            MinimumLevelOrder = 6,
            WindowMonths = 12,
            OwningInstitutionId = owningInstitutionId
        };

    private static async Task SeedAsync(ApplicationDbContext db, CurriculumItem[] items, DateOnly? programmeStart = null)
    {
        db.Institutions.Add(new Institution { Id = HostInstitution, Name = "Host" });
        db.Institutions.Add(new Institution { Id = OtherInstitution, Name = "Elsewhere" });
        db.Specialities.Add(new Speciality { Id = 1, CollegeId = 1, Name = "Paediatrics" });
        db.SubSpecialities.Add(new SubSpeciality { Id = 1, SpecialityId = 1, Name = "General Paediatrics" });

        db.Curricula.Add(new Curriculum
        {
            Id = CurriculumId, SubSpecialityId = 1, Name = "Paediatric EPA Curriculum", Version = "11.1",
            EffectiveFrom = new DateOnly(2025, 1, 1), IsActive = true
        });
        db.Curricula.Add(new Curriculum
        {
            Id = OtherCurriculumId, SubSpecialityId = 1, Name = "Paediatric EPA Curriculum", Version = "10",
            EffectiveFrom = new DateOnly(2020, 1, 1), IsActive = false
        });
        db.CurriculumItems.AddRange(items);

        db.Set<TraineeProfile>().Add(new TraineeProfile
        {
            Id = 1, UserId = TraineeUserId, InstitutionId = HostInstitution, CurriculumId = CurriculumId,
            ProgrammeStartDate = programmeStart ?? new DateOnly(2025, 1, 1),
            ExpectedCompletionDate = new DateOnly(2029, 1, 1),
            IsActive = true
        });

        db.MsfTemplates.Add(new MsfTemplate { Id = TemplateId, Name = "Annual MSF" });

        await db.SaveChangesAsync();
    }

    private static ApplicationDbContext CreateDb()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
}
