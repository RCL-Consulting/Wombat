using System.Security.Claims;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Activities;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.Features.MultiSourceFeedback;

/// <summary>
/// Per programme, EPA and semester, how many of the programme's trainees a released MSF campaign covered (T210): the
/// trainees' own cards (<see cref="GetMsfCoverageForTraineeQuery" />, T168), counted, for the trainees the caller may read
/// about (T113's ladder, trainee rung first).
/// </summary>
/// <remarks>
/// The day read as today is 1 September 2026, so the semesters are 2026's first (ended) and second (running). The
/// fixture gives each way a trainee can be covered or not a trainee of its own: covered in one semester, in the other, by
/// a campaign under review, by a learner-feedback campaign, by a declared EPA with no evidence row, by a campaign closed
/// before the span; a trainee who started in the running semester, one who starts next year, one who has completed, one
/// on another curriculum and one at another institution. And one on each side of each semester's last day (30 June and
/// 1 July, 31 December and 1 January), since whether a trainee counts in a semester is decided there
/// (<c>MsfSemesterCoverage.CountsIn</c>), for their own card as for the programme.
/// </remarks>
public sealed class GetMsfProgrammeCoverageTests
{
    private const int Host = 1;
    private const int Other = 2;
    private const int Paediatrics = 10;
    private const int InternalMedicine = 20;
    private const int MsfTemplateId = 40;
    private const int LearnerFeedbackTemplateId = 41;
    private const int MsfTypeId = 60;

    private const int Paed001 = 1;
    private const int Paed002 = 2;
    private const int Paed007 = 7;
    private const int HostLocal = 30;
    private const int OtherLocal = 31;
    private const int Retired = 32;
    private const int Im001 = 50;

    private static readonly DateOnly AsOf = new(2026, 9, 1);

    // ─── The counts are the trainees' cards ──────────────────────────────────

    [Theory]
    [InlineData("coordinator")]
    [InlineData("administrator")]
    public async Task EveryCount_IsTheTraineesOwnCards_Counted(string reader)
    {
        await using var db = await SeedAsync();
        var principal = reader == "coordinator" ? TestPrincipals.Coordinator(Host) : TestPrincipals.Administrator();

        var coverage = await ReadAsync(db, principal);
        var programmes = coverage.Programmes;

        programmes.Should().NotBeEmpty();
        var cellsWithSomeCovered = 0;
        var cellsWithSomeNot = 0;

        using var scope = new AssertionScope();
        foreach (var programme in programmes)
        {
            // Each trainee's own card, as their progress page reads it: the span is the semester before today's and
            // today's, the earlier one left out when their programme had not started by its last day.
            var cards = new Dictionary<string, MsfCoverageDto>();
            foreach (var trainee in programme.Trainees)
            {
                cards[trainee.UserId] = (await new GetMsfCoverageForTraineeQueryHandler(db).Handle(
                    new GetMsfCoverageForTraineeQuery(trainee.UserId, TestPrincipals.Administrator(), AsOf: AsOf),
                    CancellationToken.None))!;
            }

            // Whether a trainee counts in the earlier semester is whether their own card lists it: one rule decides both.
            var earlier = coverage.Periods[0];
            foreach (var trainee in programme.Trainees)
            {
                trainee.For(earlier.Year, earlier.Semester)!.HadStarted.Should().Be(
                    cards[trainee.UserId].Periods.Any(entry => entry.Year == earlier.Year && entry.Semester == earlier.Semester),
                    $"{trainee.UserId} {earlier.Name}");
            }

            foreach (var period in coverage.Periods)
            {
                var counted = programme.Trainees
                    .Where(trainee => trainee.For(period.Year, period.Semester)!.HadStarted)
                    .ToList();
                programme.For(period.Year, period.Semester)!.Trainees.Should().Be(counted.Count);

                foreach (var epa in programme.Epas)
                {
                    var coveredOnTheirCards = counted.Count(trainee =>
                        cards[trainee.UserId].Epas.Single(row => row.CurriculumItemId == epa.CurriculumItemId)
                            .For(period.Year, period.Semester)!.IsCovered);

                    var cell = epa.For(period.Year, period.Semester)!;
                    cell.TraineesCovered.Should().Be(coveredOnTheirCards, $"{programme.CurriculumName} {epa.EpaCode} {period.Name}");
                    cell.Trainees.Should().Be(counted.Count);

                    cellsWithSomeCovered += cell.TraineesCovered > 0 ? 1 : 0;
                    cellsWithSomeNot += cell.TraineesCovered < cell.Trainees ? 1 : 0;
                }

                foreach (var trainee in counted)
                {
                    // The card lists the semester, and says what the trainee's row says.
                    var card = cards[trainee.UserId];
                    card.Periods.Should().Contain(entry => entry.Year == period.Year && entry.Semester == period.Semester);
                    trainee.For(period.Year, period.Semester)!.EpasCovered.Should().Be(
                        card.Periods.Single(entry => entry.Year == period.Year && entry.Semester == period.Semester).EpasCovered,
                        $"{trainee.UserId} {period.Name}");
                    card.Epas.Select(row => row.CurriculumItemId).Should().Equal(programme.Epas.Select(row => row.CurriculumItemId));
                }
            }
        }

        // Guards: the comparison is not between two answers that say nothing.
        cellsWithSomeCovered.Should().BeGreaterThan(0);
        cellsWithSomeNot.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task TheHostPaediatricProgramme_CountsEachEpaAndSemester()
    {
        await using var db = await SeedAsync();

        var coverage = await ReadAsync(db, TestPrincipals.Coordinator(Host));

        coverage.Periods.Select(period => (period.Name, period.HasEnded)).Should().Equal(
            ("Semester 1, 2026", true), ("Semester 2, 2026", false));

        var paediatrics = coverage.Programmes.Single(programme => programme.CurriculumId == Paediatrics);
        paediatrics.InstitutionName.Should().Be("Host Hospital");
        paediatrics.CurriculumName.Should().Be("Paediatric EPA Curriculum");
        paediatrics.CurriculumVersion.Should().Be("11.1");

        // Ada, Ben, Cara and Hal (on 30 June itself) had started by 30 June. Dan, Jo (on 1 July) and Kim (on 31 December
        // itself) had started by 31 December. Fay starts on 1 January and counts in neither.
        paediatrics.Periods.Select(period => period.Trainees).Should().Equal(4, 7);

        Counts(paediatrics, "PAED-001").Should().Equal(
            [(1, 4), (1, 7)],
            "Dan's May campaign closed before his programme started, so he is not one of semester 1's trainees");
        Counts(paediatrics, "PAED-002").Should().Equal(
            [(0, 4), (1, 7)],
            "Cara's campaign is under review, Ada's learner feedback is not MSF, and Ada's December campaign is before the span");
        Counts(paediatrics, "PAED-007").Should().Equal(
            [(2, 4), (0, 7)],
            "Hal, who started on 30 June, is one of semester 1's trainees and covered; Jo, who started on 1 July, is not, " +
            "so his June campaign counts nowhere; and Dan's campaign declared PAED-007 but recorded no evidence for it");
        Counts(paediatrics, "HOST-030").Should().Equal((1, 4), (0, 7));

        paediatrics.Epas.Select(epa => (epa.EpaCode, epa.IsLocal)).Should().Equal(
            [("HOST-030", true), ("PAED-001", false), ("PAED-002", false), ("PAED-007", false)],
            "another institution's own item and a deactivated EPA are not on the programme's list");
    }

    [Fact]
    public async Task EachTraineeIsARow_ByName_WithTheirCountOrNotStarted()
    {
        await using var db = await SeedAsync();

        var paediatrics = (await ReadAsync(db, TestPrincipals.Coordinator(Host)))
            .Programmes.Single(programme => programme.CurriculumId == Paediatrics);

        paediatrics.Trainees.Select(trainee => trainee.Name).Should().Equal(
            ["Ada Adams", "Ben Botha", "Cara Cele", "Dan Dube", "Fay Fourie", "Hal Hendricks", "Jo Jacobs", "Kim Khumalo"],
            "by name, and Gus, who has completed, is on no programme");

        // Dan had not started by 30 June, so his May campaign counts for nothing in semester 1, on his row as in the EPAs'.
        // Hal started on 30 June and counts in it; Jo started on 1 July and does not; Kim started on 31 December and counts
        // in semester 2; Fay, on 1 January, counts in neither.
        paediatrics.Trainees.Select(trainee => trainee.Periods.Select(period => (period.HadStarted, period.EpasCovered)).ToArray())
            .Should().BeEquivalentTo(
                new[]
                {
                    new[] { (true, 3), (true, 0) },
                    new[] { (true, 0), (true, 1) },
                    new[] { (true, 0), (true, 0) },
                    new[] { (false, 0), (true, 1) },
                    new[] { (false, 0), (false, 0) },
                    new[] { (true, 1), (true, 0) },
                    new[] { (false, 0), (true, 0) },
                    new[] { (false, 0), (true, 0) }
                },
                options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task ACurriculumAtAnInstitution_IsItsOwnProgramme()
    {
        await using var db = await SeedAsync();

        var coordinator = await ReadAsync(db, TestPrincipals.Coordinator(Host));
        var administrator = await ReadAsync(db, TestPrincipals.Administrator());

        coordinator.Programmes.Select(programme => (programme.InstitutionName, programme.CurriculumName)).Should().Equal(
            ("Host Hospital", "Internal Medicine Curriculum"), ("Host Hospital", "Paediatric EPA Curriculum"));

        administrator.Programmes.Select(programme => (programme.InstitutionName, programme.CurriculumName)).Should().Equal(
            ("Host Hospital", "Internal Medicine Curriculum"),
            ("Host Hospital", "Paediatric EPA Curriculum"),
            ("Other Hospital", "Paediatric EPA Curriculum"));

        // The other institution's trainees follow the same curriculum row with that institution's own item on it.
        var elsewhere = administrator.Programmes.Single(programme => programme.InstitutionId == Other);
        elsewhere.Epas.Select(epa => epa.EpaCode).Should().Equal("OTHER-031", "PAED-001", "PAED-002", "PAED-007");
        elsewhere.Trainees.Select(trainee => trainee.Name).Should().Equal("Eve Essop");
        Counts(elsewhere, "PAED-001").Should().Equal((1, 1), (0, 1));

        var medicine = coordinator.Programmes.Single(programme => programme.CurriculumId == InternalMedicine);
        Counts(medicine, "IM-050").Should().Equal((1, 1), (0, 1));
    }

    // ─── Who is counted ──────────────────────────────────────────────────────

    [Fact]
    public async Task AnErasedTraineesPseudonym_AndAProfileThatOutlivedItsTrainee_AreNotCounted()
    {
        // T238. An erasure leaves the profile active under a pseudonym no account holds (ErasureExecutor), and lia's
        // profile outlived her Trainee role. Neither is a trainee on a programme now: counted, each would be a row, the
        // first by its bare pseudonym, and every "n of m" would be out by two.
        await using var db = await SeedAsync();
        var directory = Directory().WithTrainees("ada", "ben", "cara", "dan", "eve", "fay", "gus", "hal", "ivy", "jo", "kim");
        var before = await PaediatricsAsync(db, directory);

        Profile(db, 12, "deleted_user_3b4c5d6e", Host, Paediatrics, new DateOnly(2025, 1, 1));
        Profile(db, 13, "lia", Host, Paediatrics, new DateOnly(2025, 1, 1));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var paediatrics = await PaediatricsAsync(db, directory);

        paediatrics.Trainees.Select(trainee => trainee.UserId)
            .Should().Equal("ada", "ben", "cara", "dan", "fay", "hal", "jo", "kim");
        foreach (var epa in paediatrics.Epas)
        {
            Counts(paediatrics, epa.EpaCode).Should().Equal(Counts(before, epa.EpaCode), epa.EpaCode);
        }

        // As without them: both started in 2025, so counted they would have made these 6 and 9.
        Counts(paediatrics, "PAED-001").Select(count => count.Trainees).Should().Equal(4, 7);

        static async Task<MsfProgrammeDto> PaediatricsAsync(ApplicationDbContext db, FakeUserDirectory directory)
            => (await new GetMsfProgrammeCoverageQueryHandler(db, directory).Handle(
                    new GetMsfProgrammeCoverageQuery(TestPrincipals.Coordinator(Host), AsOf), CancellationToken.None))
                .Programmes.Single(programme => programme.CurriculumId == Paediatrics);
    }

    [Fact]
    public async Task ACoordinator_CountsOnlyTheirOwnInstitutionsTrainees()
    {
        await using var db = await SeedAsync();

        var other = await ReadAsync(db, TestPrincipals.Coordinator(Other));

        other.Programmes.Should().ContainSingle().Which.Trainees.Select(trainee => trainee.UserId).Should().Equal("eve");
        (await ReadAsync(db, TestPrincipals.InRole(WombatRoles.Coordinator, "coordinator-nowhere", institutionId: null)))
            .Programmes.Should().BeEmpty("a coordinator with no institution coordinates nobody");
    }

    [Fact]
    public async Task SomeoneWhoHoldsTrainee_IsShownNoProgramme_WhateverElseTheyHold()
    {
        await using var db = await SeedAsync();

        ClaimsPrincipal[] trainees =
        [
            TestPrincipals.Trainee("ada", Host),
            TestPrincipals.InRoles([WombatRoles.Trainee, WombatRoles.Coordinator], "ada", Host),
            TestPrincipals.InRoles([WombatRoles.Trainee, WombatRoles.Administrator], "ada", Host),
            TestPrincipals.InRoles([WombatRoles.Trainee, WombatRoles.Coordinator], "registrar-who-coordinates", Host)
        ];

        foreach (var principal in trainees)
        {
            GetMsfProgrammeCoverageQuery.ShowsNoProgrammeTo(principal).Should().BeTrue();
            var coverage = await ReadAsync(db, principal);
            coverage.Programmes.Should().BeEmpty("a trainee reads no other trainee's record (T185), and their own is on My progress");
            coverage.Periods.Should().HaveCount(2);
        }

        GetMsfProgrammeCoverageQuery.ShowsNoProgrammeTo(TestPrincipals.Coordinator(Host)).Should().BeFalse();
    }

    [Fact]
    public async Task SomeoneWhoOverseesNobody_IsShownNoProgramme()
    {
        await using var db = await SeedAsync();

        (await ReadAsync(db, TestPrincipals.InRole(WombatRoles.Assessor, "assessor-1", Host))).Programmes.Should().BeEmpty();
        (await ReadAsync(db, TestPrincipals.Anonymous())).Programmes.Should().BeEmpty();
    }

    [Fact]
    public async Task WithNoTraineeOnAnyProgramme_ThereIsNoProgramme()
    {
        await using var db = CreateDb();
        db.Institutions.Add(new Institution { Id = Host, Name = "Host Hospital" });
        await db.SaveChangesAsync();

        var coverage = await ReadAsync(db, TestPrincipals.Coordinator(Host));

        coverage.Programmes.Should().BeEmpty();
        coverage.AsOf.Should().Be(AsOf);
    }

    // ─── Which semesters ─────────────────────────────────────────────────────

    [Fact]
    public async Task InSemesterOne_TheSpanIsTheSecondSemesterBeforeIt_AndDecemberIsInIt()
    {
        await using var db = await SeedAsync();

        var coverage = await ReadAsync(db, TestPrincipals.Coordinator(Host), new DateOnly(2026, 2, 1));

        coverage.Periods.Select(period => (period.Name, period.End, period.HasEnded)).Should().Equal(
            ("Semester 2, 2025", new DateOnly(2025, 12, 31), true),
            ("Semester 1, 2026", new DateOnly(2026, 6, 30), false));

        // Ada's campaign that closed on 10 December 2025 covers PAED-002 in semester 2, 2025 (D40's December fold).
        var paediatrics = coverage.Programmes.Single(programme => programme.CurriculumId == Paediatrics);
        Counts(paediatrics, "PAED-002").Should().Equal((1, 2), (0, 4));
    }

    [Fact]
    public void TheValidator_RefusesNoPrincipal()
    {
        var validator = new GetMsfProgrammeCoverageQueryValidator();

        validator.Validate(new GetMsfProgrammeCoverageQuery(TestPrincipals.Coordinator(Host))).IsValid.Should().BeTrue();
        validator.Validate(new GetMsfProgrammeCoverageQuery(null!)).IsValid.Should().BeFalse();
    }

    // ─── Fixture ─────────────────────────────────────────────────────────────

    /// <summary>An EPA's (covered, trainees) in each semester, oldest first.</summary>
    private static IEnumerable<(int Covered, int Trainees)> Counts(MsfProgrammeDto programme, string epaCode)
        => programme.Epas.Single(epa => epa.EpaCode == epaCode).Periods
            .Select(period => (period.TraineesCovered, period.Trainees))
            .ToArray();

    private static Task<MsfProgrammeCoverageDto> ReadAsync(ApplicationDbContext db, ClaimsPrincipal principal, DateOnly? asOf = null)
        => new GetMsfProgrammeCoverageQueryHandler(db, Directory().WithTraineesOf(db)).Handle(
            new GetMsfProgrammeCoverageQuery(principal, asOf ?? AsOf),
            CancellationToken.None);

    private static FakeUserDirectory Directory()
        => new(
            ("ada", "Ada Adams"),
            ("ben", "Ben Botha"),
            ("cara", "Cara Cele"),
            ("dan", "Dan Dube"),
            ("eve", "Eve Essop"),
            ("fay", "Fay Fourie"),
            ("gus", "Gus Gumede"),
            ("hal", "Hal Hendricks"),
            ("ivy", "Ivy Isaacs"),
            ("jo", "Jo Jacobs"),
            ("kim", "Kim Khumalo"));

    private static async Task<ApplicationDbContext> SeedAsync()
    {
        var db = CreateDb();

        db.Institutions.Add(new Institution { Id = Host, Name = "Host Hospital" });
        db.Institutions.Add(new Institution { Id = Other, Name = "Other Hospital" });
        db.Specialities.Add(new Speciality { Id = 1, CollegeId = 1, Name = "Paediatrics" });
        db.SubSpecialities.Add(new SubSpeciality { Id = 1, SpecialityId = 1, Name = "General Paediatrics" });
        db.Curricula.Add(new Curriculum
        {
            Id = Paediatrics, SubSpecialityId = 1, Name = "Paediatric EPA Curriculum", Version = "11.1",
            EffectiveFrom = new DateOnly(2025, 1, 1), IsActive = true
        });
        db.Curricula.Add(new Curriculum
        {
            Id = InternalMedicine, SubSpecialityId = 1, Name = "Internal Medicine Curriculum", Version = "2026.1",
            EffectiveFrom = new DateOnly(2025, 1, 1), IsActive = true
        });

        db.CurriculumItems.AddRange(
            Item(Paed001, "PAED-001"),
            Item(Paed002, "PAED-002"),
            Item(Paed007, "PAED-007"),
            Item(HostLocal, "HOST-030", owningInstitutionId: Host),
            Item(OtherLocal, "OTHER-031", owningInstitutionId: Other),
            Item(Retired, "PAED-032"),
            Item(Im001, "IM-050", curriculumId: InternalMedicine));

        // Host paediatrics.
        Profile(db, 1, "ada", Host, Paediatrics, new DateOnly(2025, 1, 1));
        Profile(db, 2, "ben", Host, Paediatrics, new DateOnly(2025, 1, 1));
        Profile(db, 3, "cara", Host, Paediatrics, new DateOnly(2026, 2, 1));
        Profile(db, 4, "dan", Host, Paediatrics, new DateOnly(2026, 8, 1));
        Profile(db, 5, "fay", Host, Paediatrics, new DateOnly(2027, 1, 1));
        Profile(db, 6, "gus", Host, Paediatrics, new DateOnly(2022, 1, 1), isActive: false);
        // Elsewhere, and another curriculum.
        Profile(db, 7, "eve", Other, Paediatrics, new DateOnly(2025, 1, 1));
        Profile(db, 8, "ivy", Host, InternalMedicine, new DateOnly(2025, 1, 1));
        // Each side of each semester's last day: 30 June and 1 July, and 31 December (Fay's 1 January is above).
        Profile(db, 9, "hal", Host, Paediatrics, new DateOnly(2026, 6, 30));
        Profile(db, 10, "jo", Host, Paediatrics, new DateOnly(2026, 7, 1));
        Profile(db, 11, "kim", Host, Paediatrics, new DateOnly(2026, 12, 31));

        db.MsfTemplates.Add(new MsfTemplate { Id = MsfTemplateId, Name = "Annual MSF" });
        db.MsfTemplates.Add(new MsfTemplate { Id = LearnerFeedbackTemplateId, Name = "Learner feedback", Kind = MsfTemplateKind.LearnerFeedback });
        db.ActivityTypes.Add(new ActivityType
        {
            Id = MsfTypeId,
            Key = MsfEvidenceKinds.MsfActivityTypeKey,
            Name = "Multi-Source Feedback (Paediatrics)",
            Version = 1,
            WorkflowJson = File.ReadAllText(
                Path.Combine(AppContext.BaseDirectory, "Activities", "Seeds", "msf_cpsa", "workflow.json")),
            OwnerUserId = "seed-system",
            CreatedOn = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });

        // Semester 1, 2026: Ada covers PAED-001, PAED-007 and the host's own item.
        Campaign(db, 50, "ada", MsfCampaignState.Released, Utc(2026, 3, 10), (Paed001, true), (Paed007, true), (HostLocal, true));
        // Semester 2, 2026: Ben covers PAED-001.
        Campaign(db, 51, "ben", MsfCampaignState.Released, Utc(2026, 8, 10), (Paed001, true));
        // Cara's is under review: nothing yet, though its row names it.
        Campaign(db, 52, "cara", MsfCampaignState.UnderReview, Utc(2026, 3, 10), (Paed002, true));
        // Dan covers PAED-002; PAED-007 was declared and recorded nothing.
        Campaign(db, 53, "dan", MsfCampaignState.Released, Utc(2026, 8, 15), (Paed002, true), (Paed007, false));
        // Ada's learner feedback is not MSF, whatever row names it.
        Campaign(db, 54, "ada", MsfCampaignState.Released, Utc(2026, 4, 1), (Paed002, true)).TemplateId = LearnerFeedbackTemplateId;
        // Eve, at the other institution, covers PAED-001 in semester 1.
        Campaign(db, 55, "eve", MsfCampaignState.Released, Utc(2026, 3, 10), (Paed001, true));
        // Ivy, on Internal Medicine, covers its EPA in semester 1.
        Campaign(db, 56, "ivy", MsfCampaignState.Released, Utc(2026, 3, 10), (Im001, true));
        // Ada's December campaign: semester 2, 2025, before the span read on 1 September 2026.
        Campaign(db, 57, "ada", MsfCampaignState.Released, Utc(2025, 12, 10), (Paed002, true));
        // Gus has completed: his campaign counts nowhere, since he is on no programme.
        Campaign(db, 58, "gus", MsfCampaignState.Released, Utc(2026, 3, 10), (Paed001, true));
        // The retired EPA's evidence, which no list shows.
        Campaign(db, 59, "ben", MsfCampaignState.Released, Utc(2026, 3, 10), (Retired, true));
        // Dan's campaign run in May, before his programme started in August: he counts in no semester-1 figure.
        Campaign(db, 60, "dan", MsfCampaignState.Released, Utc(2026, 5, 10), (Paed001, true));
        // Hal's campaign on his first day, 30 June: semester 1's, and he counts in it.
        Campaign(db, 61, "hal", MsfCampaignState.Released, Utc(2026, 6, 30), (Paed007, true));
        // Jo's campaign in June, before his programme started on 1 July: he counts in no semester-1 figure.
        Campaign(db, 62, "jo", MsfCampaignState.Released, Utc(2026, 6, 15), (Paed007, true));

        await db.SaveChangesAsync();
        (await db.Epas.SingleAsync(epa => epa.Id == Retired)).Deactivate(DateTime.MinValue);
        await db.SaveChangesAsync();
        return db;
    }

    private static DateTime Utc(int year, int month, int day) => new(year, month, day, 9, 30, 0, DateTimeKind.Utc);

    private static void Profile(
        ApplicationDbContext db, int id, string userId, int institutionId, int curriculumId, DateOnly start, bool isActive = true)
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

    /// <summary>
    /// A campaign as a release leaves it: each EPA declared, and for each one marked <c>Evidence</c> an <c>msf_cpsa</c>
    /// row naming the campaign, as <c>GetMsfCoverageForTraineeTests</c> writes them.
    /// </summary>
    private static MsfCampaign Campaign(
        ApplicationDbContext db,
        int id,
        string subjectUserId,
        MsfCampaignState state,
        DateTime closedOn,
        params (int EpaId, bool Evidence)[] epas)
    {
        var campaign = new MsfCampaign
        {
            Id = id,
            SubjectUserId = subjectUserId,
            TemplateId = MsfTemplateId,
            CreatedByUserId = "coord-1",
            CreatedOn = closedOn.AddDays(-30),
            OpensOn = DateOnly.FromDateTime(closedOn.AddDays(-30)),
            ClosesOn = DateOnly.FromDateTime(closedOn),
            State = state,
            OpenedOn = closedOn.AddDays(-30),
            ClosedOn = closedOn,
            ReleasedOn = state == MsfCampaignState.Released ? closedOn.AddDays(3) : null
        };

        foreach (var (epaId, evidence) in epas)
        {
            campaign.CoveredEpas.Add(new MsfCampaignEpa { EpaId = epaId });
            if (evidence)
            {
                db.Activities.Add(new Activity
                {
                    ActivityTypeId = MsfTypeId,
                    SchemaVersion = 1,
                    SubjectUserId = subjectUserId,
                    CreatedByUserId = "coord-1",
                    CurrentState = "recorded",
                    DataJson = $$"""{ "epa_id": {{epaId}}, "campaign_id": {{id}}, "respondent_count": 8 }""",
                    EpaId = epaId,
                    CreatedOn = closedOn.AddDays(3),
                    UpdatedOn = closedOn.AddDays(3),
                    ObservedOn = DateOnly.FromDateTime(closedOn),
                    ObservedOnSource = ObservationDateSource.Declared
                });
            }
        }

        db.MsfCampaigns.Add(campaign);
        return campaign;
    }

    private static CurriculumItem Item(int epaId, string code, int? owningInstitutionId = null, int curriculumId = Paediatrics)
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

    private static ApplicationDbContext CreateDb()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
}
