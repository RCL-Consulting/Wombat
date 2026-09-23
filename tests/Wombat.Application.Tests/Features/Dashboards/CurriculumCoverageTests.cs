using System.Security.Claims;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;
using Moq;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Application.Features.Dashboards.CommitteeMember;
using Wombat.Application.Features.Dashboards.SpecialityAdmin;
using Wombat.Application.Features.Dashboards.SubSpecialityAdmin;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Dashboards;

/// <summary>
/// The committee, speciality-admin and sub-speciality-admin views of curriculum targets (T130), all read through
/// <see cref="CurriculumCoverageReader" />. Every figure is a count, "n of m trainees met this window's target",
/// and "today" is pinned to 23 September 2026 (semester 2), so nothing here depends on the date the suite runs.
/// </summary>
/// <remarks>
/// None of the three handlers had a test before T130. Each held its own copy of a lifetime percentage which, once
/// progress was stored per semester, added every semester's row together and could pass 100%, counted exempt
/// trainees as 0%, and divided by trainees whose curriculum did not hold the EPA at all.
/// </remarks>
public sealed class CurriculumCoverageTests
{
    /// <summary>Semester 2 of 2026, which the College calls July to November.</summary>
    private static readonly DateOnly AsOf = new(2026, 9, 23);

    /// <summary>Long before any window read here, so every target applies.</summary>
    private static readonly DateOnly OnTime = new(2024, 1, 1);

    /// <summary>D42: inside semester 2's first month, so its semester targets apply; after 30 June, so the year's are waived.</summary>
    private static readonly DateOnly JulyStart = new(2026, 7, 5);

    /// <summary>D14: after semester 2's first month and in the second half of the year, so every target is waived until 2027.</summary>
    private static readonly DateOnly MidSemesterStart = new(2026, 8, 15);

    /// <summary>After "today": the programme has not started, so nothing is owed.</summary>
    private static readonly DateOnly FutureStart = new(2027, 1, 1);

    private const int OurInstitution = 1;
    private const int OtherInstitution = 2;
    private const int ThirdInstitution = 3;

    // ---------------------------------------------------------------------------------------------------------
    // The reader
    // ---------------------------------------------------------------------------------------------------------

    [Fact]
    public async Task ATraineeCountsForAnEpaOnlyWhenTheirCurriculumHoldsIt_AndItIsNationalOrTheirOwnInstitutions()
    {
        // The denominator every coverage bar is drawn against. A curriculum row is shared by every adopting
        // institution, and each local extra belongs to one of them: institution 2's extra is no target for an
        // institution-1 trainee, and an extra nobody in scope may use is not listed at all. A trainee with no
        // credit is in the denominator, because they are the ones the view exists to show.
        await using var db = CreateDb();
        SeedScope(db);
        AddCurriculum(db, 1, subSpecialityId: 1);
        AddCurriculum(db, 2, subSpecialityId: 1);
        AddEpa(db, 1, "PAED-001");
        AddEpa(db, 2, "PAED-002");
        AddEpa(db, 21, "LOCAL-1", OurInstitution);
        AddEpa(db, 22, "LOCAL-2", OtherInstitution);
        AddEpa(db, 23, "LOCAL-3", ThirdInstitution);
        AddItem(db, 1, curriculumId: 1, epaId: 1, QuotaPeriod.Semester, target: 3);
        AddItem(db, 21, curriculumId: 1, epaId: 21, QuotaPeriod.AcademicYear, target: 1, OurInstitution);
        AddItem(db, 22, curriculumId: 1, epaId: 22, QuotaPeriod.AcademicYear, target: 1, OtherInstitution);
        AddItem(db, 23, curriculumId: 1, epaId: 23, QuotaPeriod.AcademicYear, target: 1, ThirdInstitution);
        AddItem(db, 2, curriculumId: 2, epaId: 2, QuotaPeriod.Semester, target: 2);

        AddTrainee(db, "ours-met", curriculumId: 1, OnTime);
        AddTrainee(db, "ours-zero", curriculumId: 1, OnTime);
        AddTrainee(db, "theirs", curriculumId: 1, OnTime, OtherInstitution);
        AddTrainee(db, "other-curriculum", curriculumId: 2, OnTime);

        AddRow(db, 1, "ours-met", 2026, 2, counts: 3);
        AddRow(db, 21, "ours-met", 2026, 1, counts: 1);
        AddRow(db, 1, "theirs", 2026, 2, counts: 1);
        AddRow(db, 22, "theirs", 2026, 2, counts: 1);
        AddRow(db, 2, "other-curriculum", 2026, 2, counts: 2);
        // A row left on an item outside the trainee's current curriculum. T121 moved dev's trainee between
        // curricula, so this is a real state. It must not put them in PAED-001's denominator, nor its numerator.
        AddRow(db, 1, "other-curriculum", 2026, 2, counts: 5);
        Commit(db);

        var coverage = await ReadAsync(db);

        coverage.Epas.Should().Equal(
            new EpaTargetCoverage(21, "LOCAL-1", TitleOf("LOCAL-1"), QuotaPeriod.AcademicYear, 1, TraineesMet: 1, TraineesApplying: 2, TraineesExempt: 0),
            new EpaTargetCoverage(22, "LOCAL-2", TitleOf("LOCAL-2"), QuotaPeriod.AcademicYear, 1, TraineesMet: 1, TraineesApplying: 1, TraineesExempt: 0),
            new EpaTargetCoverage(1, "PAED-001", TitleOf("PAED-001"), QuotaPeriod.Semester, 3, TraineesMet: 1, TraineesApplying: 3, TraineesExempt: 0),
            new EpaTargetCoverage(2, "PAED-002", TitleOf("PAED-002"), QuotaPeriod.Semester, 2, TraineesMet: 1, TraineesApplying: 1, TraineesExempt: 0));
        coverage.Epas.Select(epa => epa.PercentMet).Should().Equal(50, 100, 33, 100);
        coverage.Epas.Select(epa => epa.IsPerSemester).Should().Equal(false, false, true, true);

        // Per trainee, by kind: "theirs" is held to LOCAL-2 and not LOCAL-1; "ours-zero" is held to LOCAL-1.
        coverage.Trainees.Should().Equal(
            new TraineeTargetCoverage("ours-zero", SemesterTargetsMet: 0, SemesterTargetsApplying: 1, YearTargetsMet: 0, YearTargetsApplying: 1),
            new TraineeTargetCoverage("theirs", SemesterTargetsMet: 0, SemesterTargetsApplying: 1, YearTargetsMet: 1, YearTargetsApplying: 1),
            new TraineeTargetCoverage("other-curriculum", SemesterTargetsMet: 1, SemesterTargetsApplying: 1, YearTargetsMet: 0, YearTargetsApplying: 0),
            new TraineeTargetCoverage("ours-met", SemesterTargetsMet: 1, SemesterTargetsApplying: 1, YearTargetsMet: 1, YearTargetsApplying: 1));
        coverage.ExemptTraineeCount.Should().Be(0);
        coverage.AsOf.Should().Be(AsOf);
        coverage.CurrentSemesterName.Should().Be("Semester 2, 2026");
        coverage.CurrentSemesterMonths.Should().Be("July to November");
    }

    [Fact]
    public async Task ExemptPairsLeaveTheDenominator_AndAreCountedApart()
    {
        // D14 waives a target for a registrar who starts part-way through its window. The old arithmetic counted
        // such a trainee as 0% and dragged every EPA down. Now the pair leaves the denominator and is counted in
        // TraineesExempt instead. A July starter owes this semester's targets but not this year's (D42), so they
        // stay on the list with semester targets only. The two trainees owing nothing are counted, not listed.
        await using var db = CreateDb();
        SeedScope(db);
        SeedSemesterAndYearCurriculum(db, semesterTarget: 3, yearTarget: 1);

        AddTrainee(db, "on-time", curriculumId: 1, OnTime);
        AddTrainee(db, "july", curriculumId: 1, JulyStart);
        AddTrainee(db, "mid-semester", curriculumId: 1, MidSemesterStart);
        AddTrainee(db, "not-started", curriculumId: 1, FutureStart);

        AddRow(db, SemesterItem, "on-time", 2026, 2, counts: 3);
        // Evidence recorded while exempt is still credited, but the waived target is not thereby "met".
        AddRow(db, SemesterItem, "mid-semester", 2026, 2, counts: 3);
        AddRow(db, YearItem, "mid-semester", 2026, 2, counts: 1);
        Commit(db);

        var coverage = await ReadAsync(db);

        coverage.Epas.Should().Equal(
            new EpaTargetCoverage(SemesterEpa, "PAED-001", TitleOf("PAED-001"), QuotaPeriod.Semester, 3, TraineesMet: 1, TraineesApplying: 2, TraineesExempt: 2),
            new EpaTargetCoverage(YearEpa, "PAED-011", TitleOf("PAED-011"), QuotaPeriod.AcademicYear, 1, TraineesMet: 0, TraineesApplying: 1, TraineesExempt: 3));
        coverage.Epas.Select(epa => epa.PercentMet).Should().Equal(50, 0);

        coverage.Trainees.Should().Equal(
            new TraineeTargetCoverage("july", SemesterTargetsMet: 0, SemesterTargetsApplying: 1, YearTargetsMet: 0, YearTargetsApplying: 0),
            new TraineeTargetCoverage("on-time", SemesterTargetsMet: 1, SemesterTargetsApplying: 1, YearTargetsMet: 0, YearTargetsApplying: 1));
        coverage.ExemptTraineeCount.Should().Be(2);
    }

    [Fact]
    public async Task WhenEveryPairIsExempt_EachEpaHasNoneApplyingAndZeroPercent_WithoutDividingByZero()
    {
        // Dev's state when T130 landed: its only trainee started on 20 September 2026, mid-semester, so every pair
        // is exempt and both denominators are nought. A NaN or a DivideByZeroException would reach the tile. The
        // exempt trainee's three encounters reach the number, but a waived target is never "met".
        await using var db = CreateDb();
        SeedScope(db);
        SeedSemesterAndYearCurriculum(db, semesterTarget: 3, yearTarget: 1);

        AddTrainee(db, "dev-trainee", curriculumId: 1, new DateOnly(2026, 9, 20));
        AddTrainee(db, "not-started", curriculumId: 1, FutureStart);
        AddRow(db, SemesterItem, "dev-trainee", 2026, 2, counts: 3);
        Commit(db);

        var coverage = await ReadAsync(db);

        coverage.Epas.Should().Equal(
            new EpaTargetCoverage(SemesterEpa, "PAED-001", TitleOf("PAED-001"), QuotaPeriod.Semester, 3, TraineesMet: 0, TraineesApplying: 0, TraineesExempt: 2),
            new EpaTargetCoverage(YearEpa, "PAED-011", TitleOf("PAED-011"), QuotaPeriod.AcademicYear, 1, TraineesMet: 0, TraineesApplying: 0, TraineesExempt: 2));
        coverage.Epas.Select(epa => epa.PercentMet).Should().Equal(0, 0);
        coverage.Trainees.Should().BeEmpty();
        coverage.ExemptTraineeCount.Should().Be(2);
    }

    [Fact]
    public async Task TwoSemesterRowsForOnePairCountOnlyTheCurrentWindow()
    {
        // Progress is stored per semester. The old dashboards added every row of a pair together, so a trainee with
        // five last semester and one this semester read as having met a three-per-semester target, and an EPA's
        // figure could pass 100%. A semester item reads this semester's row alone; a year item reads both of this
        // year's semesters and nothing from 2025.
        await using var db = CreateDb();
        SeedScope(db);
        SeedSemesterAndYearCurriculum(db, semesterTarget: 3, yearTarget: 2);

        AddTrainee(db, "trainee-a", curriculumId: 1, OnTime);
        AddTrainee(db, "trainee-b", curriculumId: 1, OnTime);

        AddRow(db, SemesterItem, "trainee-a", 2025, 2, counts: 4);
        AddRow(db, SemesterItem, "trainee-a", 2026, 1, counts: 5);
        AddRow(db, SemesterItem, "trainee-a", 2026, 2, counts: 1);   // 1 of 3: not met
        AddRow(db, YearItem, "trainee-a", 2025, 2, counts: 7);
        AddRow(db, YearItem, "trainee-a", 2026, 1, counts: 1);
        AddRow(db, YearItem, "trainee-a", 2026, 2, counts: 1);       // 1 + 1 of 2: met, only because both semesters add

        AddRow(db, SemesterItem, "trainee-b", 2026, 1, counts: 3);
        AddRow(db, SemesterItem, "trainee-b", 2026, 2, counts: 3);   // 3 of 3: met, and not "6 of 3"
        AddRow(db, YearItem, "trainee-b", 2025, 1, counts: 5);       // 0 of 2 this year: last year's five do not carry
        Commit(db);

        var coverage = await ReadAsync(db);

        coverage.Epas.Should().Equal(
            new EpaTargetCoverage(SemesterEpa, "PAED-001", TitleOf("PAED-001"), QuotaPeriod.Semester, 3, TraineesMet: 1, TraineesApplying: 2, TraineesExempt: 0),
            new EpaTargetCoverage(YearEpa, "PAED-011", TitleOf("PAED-011"), QuotaPeriod.AcademicYear, 2, TraineesMet: 1, TraineesApplying: 2, TraineesExempt: 0));
        coverage.Epas.Select(epa => epa.PercentMet).Should().Equal(50, 50);

        coverage.Trainees.Should().Equal(
            new TraineeTargetCoverage("trainee-a", SemesterTargetsMet: 0, SemesterTargetsApplying: 1, YearTargetsMet: 1, YearTargetsApplying: 1),
            new TraineeTargetCoverage("trainee-b", SemesterTargetsMet: 1, SemesterTargetsApplying: 1, YearTargetsMet: 0, YearTargetsApplying: 1));
    }

    [Fact]
    public async Task TraineesAreCountedByKind_AndOrderedFewestMetFirstAsAShareOfWhatApplies()
    {
        // A committee reads this list top-down, so the trainees furthest behind come first. "Furthest behind" is a
        // share, not a count: a July starter owes three semester targets and no yearly ones, so two of three puts
        // them ahead of a trainee on three of five, although three is more than two. The ids run the other way
        // alphabetically, so an ordering by id, or by the raw count, cannot pass by accident. Equal shares fall
        // back to the id.
        await using var db = CreateDb();
        SeedScope(db);
        AddCurriculum(db, 1, subSpecialityId: 1);
        AddEpa(db, 1, "PAED-001");
        AddEpa(db, 2, "PAED-002");
        AddEpa(db, 3, "PAED-003");
        AddEpa(db, 8, "PAED-008");
        AddEpa(db, 9, "PAED-009");
        AddItem(db, 1, curriculumId: 1, epaId: 1, QuotaPeriod.Semester, target: 1);
        AddItem(db, 2, curriculumId: 1, epaId: 2, QuotaPeriod.Semester, target: 1);
        AddItem(db, 3, curriculumId: 1, epaId: 3, QuotaPeriod.Semester, target: 1);
        AddItem(db, 8, curriculumId: 1, epaId: 8, QuotaPeriod.AcademicYear, target: 1);
        AddItem(db, 9, curriculumId: 1, epaId: 9, QuotaPeriod.AcademicYear, target: 1);

        AddTrainee(db, "trainee-a", curriculumId: 1, OnTime);      // 4 of 5
        AddTrainee(db, "trainee-b", curriculumId: 1, JulyStart);   // 2 of 3
        AddTrainee(db, "trainee-c", curriculumId: 1, OnTime);      // 3 of 5
        AddTrainee(db, "trainee-d", curriculumId: 1, OnTime);      // 0 of 5
        AddTrainee(db, "trainee-e", curriculumId: 1, OnTime);      // 0 of 5, ties with trainee-d

        foreach (var item in new[] { 1, 2, 3, 8 })
        {
            AddRow(db, item, "trainee-a", 2026, item == 8 ? 1 : 2, counts: 1);
        }

        AddRow(db, 1, "trainee-b", 2026, 2, counts: 1);
        AddRow(db, 2, "trainee-b", 2026, 2, counts: 1);
        AddRow(db, 1, "trainee-c", 2026, 2, counts: 1);
        AddRow(db, 2, "trainee-c", 2026, 2, counts: 1);
        AddRow(db, 8, "trainee-c", 2026, 1, counts: 1);
        Commit(db);

        var coverage = await ReadAsync(db);

        coverage.Trainees.Should().Equal(
            new TraineeTargetCoverage("trainee-d", SemesterTargetsMet: 0, SemesterTargetsApplying: 3, YearTargetsMet: 0, YearTargetsApplying: 2),
            new TraineeTargetCoverage("trainee-e", SemesterTargetsMet: 0, SemesterTargetsApplying: 3, YearTargetsMet: 0, YearTargetsApplying: 2),
            new TraineeTargetCoverage("trainee-c", SemesterTargetsMet: 2, SemesterTargetsApplying: 3, YearTargetsMet: 1, YearTargetsApplying: 2),
            new TraineeTargetCoverage("trainee-b", SemesterTargetsMet: 2, SemesterTargetsApplying: 3, YearTargetsMet: 0, YearTargetsApplying: 0),
            new TraineeTargetCoverage("trainee-a", SemesterTargetsMet: 3, SemesterTargetsApplying: 3, YearTargetsMet: 1, YearTargetsApplying: 2));
        coverage.Trainees.Select(trainee => (trainee.TargetsMet, trainee.TargetsApplying))
            .Should().Equal((0, 5), (0, 5), (3, 5), (2, 3), (4, 5));
        coverage.ExemptTraineeCount.Should().Be(0);
    }

    [Fact]
    public async Task WithNoActiveTraineesTheCoverageIsEmpty_ButStillNamesTheCurrentSemester()
    {
        await using var db = CreateDb();

        var coverage = await CurriculumCoverageReader.ReadAsync(db, [], AsOf, CancellationToken.None);

        coverage.Should().BeEquivalentTo(
            new CurriculumCoverage(AsOf, "Semester 2, 2026", "July to November", [], [], 0),
            options => options.ComparingRecordsByMembers());
    }

    [Fact]
    public async Task AnEpaRowStatesOnlyTheTargetOfTheTraineesItCounts()
    {
        // Two versions of one curriculum can hold the same EPA against different targets: an administrator edits
        // the new version's item, and trainees admitted earlier stay pinned to the old version. Both versions sit in
        // one sub-speciality, so one committee sees both. A row reads "PAED-001 (3 per semester): n of m met", so
        // every trainee counted under it must be held to the target and window it states. Otherwise the row says a
        // trainee met "3 per semester" on the strength of a single encounter against a one-a-year target. Whatever
        // shape the rows take, the trainees under a stated target must be exactly those held to it.
        await using var db = CreateDb();
        SeedScope(db);
        AddCurriculum(db, 1, subSpecialityId: 1);
        AddCurriculum(db, 2, subSpecialityId: 1);
        AddEpa(db, 1, "PAED-001");
        AddItem(db, 101, curriculumId: 1, epaId: 1, QuotaPeriod.AcademicYear, target: 1);
        AddItem(db, 201, curriculumId: 2, epaId: 1, QuotaPeriod.Semester, target: 3);

        AddTrainee(db, "on-version-1", curriculumId: 1, OnTime);
        AddTrainee(db, "on-version-2", curriculumId: 2, OnTime);
        AddRow(db, 101, "on-version-1", 2026, 2, counts: 1);   // 1 of 1 a year: met
        AddRow(db, 201, "on-version-2", 2026, 2, counts: 1);   // 1 of 3 a semester: not met
        Commit(db);

        var coverage = await ReadAsync(db);

        var perSemester = coverage.Epas.Where(epa => epa.EpaCode == "PAED-001" && epa.QuotaPeriod == QuotaPeriod.Semester && epa.Target == 3).ToList();
        var perYear = coverage.Epas.Where(epa => epa.EpaCode == "PAED-001" && epa.QuotaPeriod == QuotaPeriod.AcademicYear && epa.Target == 1).ToList();

        using (new AssertionScope())
        {
            perSemester.Sum(epa => epa.TraineesApplying).Should().Be(1, "only on-version-2 is held to three per semester");
            perSemester.Sum(epa => epa.TraineesMet).Should().Be(0, "on-version-2 has one of three");
            perYear.Sum(epa => epa.TraineesApplying).Should().Be(1, "only on-version-1 is held to one per academic year");
            perYear.Sum(epa => epa.TraineesMet).Should().Be(1, "on-version-1 has one of one");
        }

        // The per-trainee figures are right either way: each trainee is read against their own item.
        coverage.Trainees.Should().Equal(
            new TraineeTargetCoverage("on-version-2", SemesterTargetsMet: 0, SemesterTargetsApplying: 1, YearTargetsMet: 0, YearTargetsApplying: 0),
            new TraineeTargetCoverage("on-version-1", SemesterTargetsMet: 0, SemesterTargetsApplying: 0, YearTargetsMet: 1, YearTargetsApplying: 1));
    }

    // ---------------------------------------------------------------------------------------------------------
    // The three handlers
    // ---------------------------------------------------------------------------------------------------------

    [Fact]
    public async Task CommitteeMemberDashboard_ListsTheirSubSpecialitiesActiveTraineesByName_AndEachEpasCount()
    {
        // The committee card printed the user id in the name column before T130. The member's scope is their
        // sub-speciality claims, 1 and 2 here: sub-speciality 3's trainee, and the inactive one, would each move
        // PAED-001 to "2 of 3" if they leaked in.
        await using var db = CreateDb();
        SeedProgramme(db);
        var handler = new GetCommitteeMemberDashboardSummaryQueryHandler(db, TraineeNames().Object);

        var result = await handler.Handle(
            new GetCommitteeMemberDashboardSummaryQuery(CommitteeMember(subSpecialityIds: [1, 2]), AsOf),
            CancellationToken.None);

        result.CurrentSemesterName.Should().Be("Semester 2, 2026");
        result.CurrentSemesterMonths.Should().Be("July to November");
        result.TraineeTargets.Should().Equal(
            new TraineeTargetsItem("bongani", "Bongani Dlamini", SemesterTargetsMet: 0, SemesterTargetsApplying: 1, YearTargetsMet: 0, YearTargetsApplying: 1),
            new TraineeTargetsItem("amara", "Amara Okafor", SemesterTargetsMet: 1, SemesterTargetsApplying: 1, YearTargetsMet: 0, YearTargetsApplying: 1),
            new TraineeTargetsItem("chen", "Chen Wei", SemesterTargetsMet: 1, SemesterTargetsApplying: 1, YearTargetsMet: 0, YearTargetsApplying: 0));
        result.EpaTargets.Should().Equal(ExpectedEpasForSubSpecialitiesOneAndTwo);
        result.ExemptTraineeCount.Should().Be(1, "dineo started mid-semester");
    }

    [Fact]
    public async Task EveryStaffDashboard_KeepsToTheCallersInstitution_ThoughTheCurriculumIsShared()
    {
        // A sub-speciality id is national (College-owned), so on its own it matches every adopting institution's
        // trainees. The review of T130 found the committee card listing trainees from every institution by name.
        // gugu trains at another institution on the SAME curriculum and has met PAED-001: if she leaked in, the
        // committee would name her and every card would read PAED-001 "2 of 3".
        await using var db = CreateDb();
        SeedProgramme(db);
        AddTrainee(db, "gugu", curriculumId: 1, OnTime, institutionId: OtherInstitution);
        AddRow(db, 1, "gugu", 2026, 2, counts: 3);
        Commit(db);
        var users = TraineeNames();

        var committee = await new GetCommitteeMemberDashboardSummaryQueryHandler(db, users.Object).Handle(
            new GetCommitteeMemberDashboardSummaryQuery(CommitteeMember(subSpecialityIds: [1, 2]), AsOf), CancellationToken.None);
        var speciality = await new GetSpecialityAdminDashboardSummaryQueryHandler(db).Handle(
            new GetSpecialityAdminDashboardSummaryQuery(SpecialityAdmin(1), AsOf), CancellationToken.None);
        var subSpeciality = await new GetSubSpecialityAdminDashboardSummaryQueryHandler(db).Handle(
            new GetSubSpecialityAdminDashboardSummaryQuery(SubSpecialityAdmin(1), AsOf), CancellationToken.None);

        committee.TraineeTargets.Select(trainee => trainee.TraineeUserId).Should().NotContain("gugu");
        committee.EpaTargets.Should().Equal(ExpectedEpasForSubSpecialitiesOneAndTwo);
        speciality.CurriculumCoverage.Epas.Should().Equal(ExpectedEpasForSubSpecialitiesOneAndTwo);
        speciality.ActiveTraineeCount.Should().Be(4, "amara, bongani, chen and dineo; not gugu at the other institution");
        subSpeciality.CurriculumCoverage.Epas.Single(epa => epa.EpaCode == "PAED-001").TraineesApplying.Should().Be(2);
        users.Verify(
            service => service.GetDisplayNamesAsync(
                It.Is<IReadOnlyCollection<string>>(ids => !ids.Contains("gugu")), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task AStaffMemberWithNoInstitutionSeesNoTrainees_AndAnAdministratorSeesThemAll()
    {
        await using var db = CreateDb();
        SeedProgramme(db);
        AddTrainee(db, "gugu", curriculumId: 1, OnTime, institutionId: OtherInstitution);
        Commit(db);

        var unscoped = new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, "nobody"),
                new Claim(ClaimTypes.Role, WombatRoles.CommitteeMember),
                new Claim(WombatClaimTypes.SubSpecialityId, "1")
            ],
            "test"));
        var noInstitution = await new GetCommitteeMemberDashboardSummaryQueryHandler(db, TraineeNames().Object).Handle(
            new GetCommitteeMemberDashboardSummaryQuery(unscoped, AsOf), CancellationToken.None);
        noInstitution.TraineeTargets.Should().BeEmpty();
        noInstitution.EpaTargets.Should().BeEmpty();

        var administrator = new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, "admin"),
                new Claim(ClaimTypes.Role, WombatRoles.Administrator),
                new Claim(WombatClaimTypes.SubSpecialityId, "1")
            ],
            "test"));
        var global = await new GetCommitteeMemberDashboardSummaryQueryHandler(db, TraineeNames().Object).Handle(
            new GetCommitteeMemberDashboardSummaryQuery(administrator, AsOf), CancellationToken.None);
        global.TraineeTargets.Select(trainee => trainee.TraineeUserId).Should().Contain("gugu");
    }

    [Fact]
    public async Task ATraineeWithNoTargetsIsNotCalledExempt()
    {
        // D14 waives a target a trainee has. A trainee whose curriculum holds no item for them (a new version with no
        // items yet, or only another institution's local extras) has no target to waive, and "1 exempt this period"
        // would be D14 wording for something D14 never did.
        await using var db = CreateDb();
        SeedScope(db);
        AddCurriculum(db, 1, subSpecialityId: 1);
        AddCurriculum(db, 2, subSpecialityId: 1);
        AddEpa(db, 1, "PAED-001");
        AddEpa(db, 9, "LOCAL-9", owningInstitutionId: OtherInstitution);
        AddItem(db, 1, curriculumId: 1, epaId: 1, QuotaPeriod.Semester, target: 3);
        AddItem(db, 9, curriculumId: 2, epaId: 9, QuotaPeriod.Semester, target: 1, owningInstitutionId: OtherInstitution);
        AddTrainee(db, "amara", curriculumId: 1, OnTime);
        AddTrainee(db, "bongani", curriculumId: 2, OnTime);
        Commit(db);

        var coverage = await ReadAsync(db);

        coverage.ExemptTraineeCount.Should().Be(0);
        coverage.Trainees.Select(trainee => trainee.TraineeUserId).Should().Equal("amara");
    }

    [Fact]
    public async Task CommitteeMemberDashboard_WhenEveryTraineeIsExempt_ShowsNoTraineesAndNoneApplying()
    {
        // The all-exempt state the committee card is in on dev. It must render counts of nought, not throw.
        await using var db = CreateDb();
        SeedScope(db);
        SeedSemesterAndYearCurriculum(db, semesterTarget: 3, yearTarget: 1);
        AddTrainee(db, "dineo", curriculumId: 1, MidSemesterStart);
        AddRow(db, SemesterItem, "dineo", 2026, 2, counts: 3);
        Commit(db);
        var handler = new GetCommitteeMemberDashboardSummaryQueryHandler(db, TraineeNames().Object);

        var result = await handler.Handle(
            new GetCommitteeMemberDashboardSummaryQuery(CommitteeMember(subSpecialityIds: [1]), AsOf),
            CancellationToken.None);

        result.TraineeTargets.Should().BeEmpty();
        result.EpaTargets.Should().Equal(
            new EpaTargetCoverage(SemesterEpa, "PAED-001", TitleOf("PAED-001"), QuotaPeriod.Semester, 3, TraineesMet: 0, TraineesApplying: 0, TraineesExempt: 1),
            new EpaTargetCoverage(YearEpa, "PAED-011", TitleOf("PAED-011"), QuotaPeriod.AcademicYear, 1, TraineesMet: 0, TraineesApplying: 0, TraineesExempt: 1));
        result.EpaTargets.Select(epa => epa.PercentMet).Should().Equal(0, 0);
        result.ExemptTraineeCount.Should().Be(1);
    }

    [Fact]
    public async Task SpecialityAdminDashboard_ReadsCoverageForItsSpecialitysActiveTraineesOnly()
    {
        // Speciality 1 holds sub-specialities 1 and 2. The inactive trainee is counted in the tile but kept out of
        // the coverage, and sub-speciality 3 belongs to speciality 2, so its trainee is in neither.
        await using var db = CreateDb();
        SeedProgramme(db);
        var handler = new GetSpecialityAdminDashboardSummaryQueryHandler(db);

        var result = await handler.Handle(
            new GetSpecialityAdminDashboardSummaryQuery(SpecialityAdmin(specialityId: 1), AsOf),
            CancellationToken.None);

        result.ActiveTraineeCount.Should().Be(4);
        result.InactiveTraineeCount.Should().Be(1);
        var coverage = result.CurriculumCoverage;
        coverage.AsOf.Should().Be(AsOf);
        coverage.CurrentSemesterName.Should().Be("Semester 2, 2026");
        coverage.CurrentSemesterMonths.Should().Be("July to November");
        coverage.Trainees.Should().Equal(
            new TraineeTargetCoverage("bongani", SemesterTargetsMet: 0, SemesterTargetsApplying: 1, YearTargetsMet: 0, YearTargetsApplying: 1),
            new TraineeTargetCoverage("amara", SemesterTargetsMet: 1, SemesterTargetsApplying: 1, YearTargetsMet: 0, YearTargetsApplying: 1),
            new TraineeTargetCoverage("chen", SemesterTargetsMet: 1, SemesterTargetsApplying: 1, YearTargetsMet: 0, YearTargetsApplying: 0));
        coverage.Epas.Should().Equal(ExpectedEpasForSubSpecialitiesOneAndTwo);
        coverage.ExemptTraineeCount.Should().Be(1);

        // One reader behind all three dashboards: the handler adds nothing to what it returns.
        var direct = await CurriculumCoverageReader.ReadAsync(db, await ActiveProfilesIn(db, 1, 2), AsOf, CancellationToken.None);
        coverage.Should().BeEquivalentTo(direct, options => options.ComparingRecordsByMembers().WithStrictOrdering());
    }

    [Fact]
    public async Task SubSpecialityAdminDashboard_ReadsCoverageForItsSubSpecialitysActiveTraineesOnly()
    {
        // Sub-speciality 1 alone: chen (sub-speciality 2) and PAED-006 drop out, and the inactive trainee's met
        // PAED-001 and PAED-011 are not counted.
        await using var db = CreateDb();
        SeedProgramme(db);
        var handler = new GetSubSpecialityAdminDashboardSummaryQueryHandler(db);

        var result = await handler.Handle(
            new GetSubSpecialityAdminDashboardSummaryQuery(SubSpecialityAdmin(subSpecialityId: 1), AsOf),
            CancellationToken.None);

        result.ActiveTraineeCount.Should().Be(3);
        result.InactiveTraineeCount.Should().Be(1);
        var coverage = result.CurriculumCoverage;
        coverage.AsOf.Should().Be(AsOf);
        coverage.CurrentSemesterName.Should().Be("Semester 2, 2026");
        coverage.Trainees.Should().Equal(
            new TraineeTargetCoverage("bongani", SemesterTargetsMet: 0, SemesterTargetsApplying: 1, YearTargetsMet: 0, YearTargetsApplying: 1),
            new TraineeTargetCoverage("amara", SemesterTargetsMet: 1, SemesterTargetsApplying: 1, YearTargetsMet: 0, YearTargetsApplying: 1));
        coverage.Epas.Should().Equal(
            new EpaTargetCoverage(1, "PAED-001", TitleOf("PAED-001"), QuotaPeriod.Semester, 3, TraineesMet: 1, TraineesApplying: 2, TraineesExempt: 1),
            new EpaTargetCoverage(11, "PAED-011", TitleOf("PAED-011"), QuotaPeriod.AcademicYear, 1, TraineesMet: 0, TraineesApplying: 2, TraineesExempt: 1));
        coverage.ExemptTraineeCount.Should().Be(1);

        var direct = await CurriculumCoverageReader.ReadAsync(db, await ActiveProfilesIn(db, 1), AsOf, CancellationToken.None);
        coverage.Should().BeEquivalentTo(direct, options => options.ComparingRecordsByMembers().WithStrictOrdering());
    }

    // ---------------------------------------------------------------------------------------------------------
    // Fixture
    // ---------------------------------------------------------------------------------------------------------

    private const int SemesterEpa = 1;
    private const int YearEpa = 11;
    private const int SemesterItem = 1;
    private const int YearItem = 11;

    /// <summary>What sub-specialities 1 and 2 of <see cref="SeedProgramme" /> read, for the committee and speciality views.</summary>
    private static readonly EpaTargetCoverage[] ExpectedEpasForSubSpecialitiesOneAndTwo =
    [
        new(1, "PAED-001", TitleOf("PAED-001"), QuotaPeriod.Semester, 3, TraineesMet: 1, TraineesApplying: 2, TraineesExempt: 1),
        new(6, "PAED-006", TitleOf("PAED-006"), QuotaPeriod.Semester, 2, TraineesMet: 1, TraineesApplying: 1, TraineesExempt: 0),
        new(11, "PAED-011", TitleOf("PAED-011"), QuotaPeriod.AcademicYear, 1, TraineesMet: 0, TraineesApplying: 2, TraineesExempt: 1)
    ];

    /// <summary>
    /// Three sub-specialities with a curriculum each. Sub-speciality 1: amara (met PAED-001), bongani (no credit),
    /// dineo (started mid-semester, exempt), and emeka (inactive, met both). Sub-speciality 2: chen (met PAED-006).
    /// Sub-speciality 3, in speciality 2: farai (met PAED-001 on that curriculum's own item for the same EPA).
    /// </summary>
    private static void SeedProgramme(ApplicationDbContext db)
    {
        SeedScope(db);
        AddCurriculum(db, 1, subSpecialityId: 1);
        AddCurriculum(db, 2, subSpecialityId: 2);
        AddCurriculum(db, 3, subSpecialityId: 3);
        AddEpa(db, 1, "PAED-001");
        AddEpa(db, 6, "PAED-006");
        AddEpa(db, 11, "PAED-011");
        AddItem(db, 1, curriculumId: 1, epaId: 1, QuotaPeriod.Semester, target: 3);
        AddItem(db, 11, curriculumId: 1, epaId: 11, QuotaPeriod.AcademicYear, target: 1);
        AddItem(db, 6, curriculumId: 2, epaId: 6, QuotaPeriod.Semester, target: 2);
        AddItem(db, 31, curriculumId: 3, epaId: 1, QuotaPeriod.Semester, target: 3);

        AddTrainee(db, "amara", curriculumId: 1, OnTime);
        AddTrainee(db, "bongani", curriculumId: 1, OnTime);
        AddTrainee(db, "chen", curriculumId: 2, OnTime);
        AddTrainee(db, "dineo", curriculumId: 1, MidSemesterStart);
        AddTrainee(db, "emeka", curriculumId: 1, OnTime, isActive: false);
        AddTrainee(db, "farai", curriculumId: 3, OnTime);

        AddRow(db, 1, "amara", 2026, 2, counts: 3);
        AddRow(db, 6, "chen", 2026, 2, counts: 2);
        AddRow(db, 1, "dineo", 2026, 2, counts: 1);
        AddRow(db, 1, "emeka", 2026, 2, counts: 3);
        AddRow(db, 11, "emeka", 2026, 1, counts: 1);
        AddRow(db, 31, "farai", 2026, 2, counts: 3);
        Commit(db);
    }

    /// <summary>Curriculum 1 in sub-speciality 1: PAED-001 per semester and PAED-011 per academic year.</summary>
    private static void SeedSemesterAndYearCurriculum(ApplicationDbContext db, int semesterTarget, int yearTarget)
    {
        AddCurriculum(db, 1, subSpecialityId: 1);
        AddEpa(db, SemesterEpa, "PAED-001");
        AddEpa(db, YearEpa, "PAED-011");
        AddItem(db, SemesterItem, curriculumId: 1, epaId: SemesterEpa, QuotaPeriod.Semester, semesterTarget);
        AddItem(db, YearItem, curriculumId: 1, epaId: YearEpa, QuotaPeriod.AcademicYear, yearTarget);
    }

    /// <summary>Three institutions; speciality 1 holds sub-specialities 1 and 2, speciality 2 holds sub-speciality 3.</summary>
    private static void SeedScope(ApplicationDbContext db)
    {
        db.Institutions.Add(new Institution { Id = OurInstitution, Name = "KGK" });
        db.Institutions.Add(new Institution { Id = OtherInstitution, Name = "Other hospital" });
        db.Institutions.Add(new Institution { Id = ThirdInstitution, Name = "Third hospital" });
        db.Specialities.Add(new Speciality { Id = 1, CollegeId = 1, Name = "Paediatrics" });
        db.Specialities.Add(new Speciality { Id = 2, CollegeId = 1, Name = "Internal Medicine" });
        db.SubSpecialities.Add(new SubSpeciality { Id = 1, SpecialityId = 1, Name = "General Paediatrics" });
        db.SubSpecialities.Add(new SubSpeciality { Id = 2, SpecialityId = 1, Name = "Neonatology" });
        db.SubSpecialities.Add(new SubSpeciality { Id = 3, SpecialityId = 2, Name = "General Medicine" });
    }

    private static void AddCurriculum(ApplicationDbContext db, int id, int subSpecialityId)
        => db.Curricula.Add(new Curriculum
        {
            Id = id, SubSpecialityId = subSpecialityId, Name = $"Curriculum {id}",
            Version = "2026.1", EffectiveFrom = new DateOnly(2025, 1, 1), IsActive = true
        });

    private static string TitleOf(string code) => $"Title of {code}";

    private static void AddEpa(ApplicationDbContext db, int id, string code, int? owningInstitutionId = null)
        => db.Epas.Add(new Epa
        {
            Id = id, SubSpecialityId = 1, OwningInstitutionId = owningInstitutionId, Code = code, Title = TitleOf(code)
        });

    private static void AddItem(
        ApplicationDbContext db,
        int id,
        int curriculumId,
        int epaId,
        QuotaPeriod quotaPeriod,
        int target,
        int? owningInstitutionId = null)
        => db.CurriculumItems.Add(new CurriculumItem
        {
            Id = id, CurriculumId = curriculumId, EpaId = epaId, OwningInstitutionId = owningInstitutionId,
            RequiredCount = target, QuotaPeriod = quotaPeriod, MinimumLevelOrder = 3, WindowMonths = 12
        });

    private static void AddTrainee(
        ApplicationDbContext db,
        string userId,
        int curriculumId,
        DateOnly programmeStart,
        int institutionId = OurInstitution,
        bool isActive = true)
        => db.TraineeProfiles.Add(new TraineeProfile
        {
            UserId = userId, InstitutionId = institutionId, CurriculumId = curriculumId,
            ProgrammeStartDate = programmeStart, ExpectedCompletionDate = programmeStart.AddYears(4),
            IsActive = isActive
        });

    private static void AddRow(ApplicationDbContext db, int curriculumItemId, string traineeUserId, int year, int semester, int counts)
        => db.CurriculumItemProgresses.Add(new CurriculumItemProgress
        {
            CurriculumItemId = curriculumItemId,
            TraineeUserId = traineeUserId,
            AcademicYear = year,
            Semester = semester,
            CountsSoFar = counts,
            MinimumLevelReachedCount = counts,
            LastObservedOn = new AcademicPeriod(year, semester).Start,
            LastUpdated = new DateTime(2026, 9, 1, 8, 0, 0, DateTimeKind.Utc)
        });

    /// <summary>
    /// Saves, then checks by hand that every (item, trainee, year, semester) bucket holds one row. The in-memory
    /// provider enforces no unique index, so a fixture could hold a state Postgres refuses, and a figure resting on
    /// it would prove nothing.
    /// </summary>
    private static void Commit(ApplicationDbContext db)
    {
        db.SaveChanges();
        db.CurriculumItemProgresses.AsNoTracking().AsEnumerable()
            .GroupBy(CurriculumItemProgressKey.Of)
            .Where(bucket => bucket.Count() > 1)
            .Select(bucket => bucket.Key)
            .Should().BeEmpty("the database holds one row per (item, trainee, year, semester)");
    }

    private static async Task<CurriculumCoverage> ReadAsync(ApplicationDbContext db)
    {
        var active = await db.TraineeProfiles.AsNoTracking().Where(profile => profile.IsActive).ToListAsync();
        return await CurriculumCoverageReader.ReadAsync(db, active, AsOf, CancellationToken.None);
    }

    private static async Task<List<TraineeProfile>> ActiveProfilesIn(ApplicationDbContext db, params int[] subSpecialityIds)
    {
        var curricula = await db.Curricula.AsNoTracking()
            .Where(curriculum => subSpecialityIds.Contains(curriculum.SubSpecialityId))
            .Select(curriculum => curriculum.Id)
            .ToListAsync();
        return await db.TraineeProfiles.AsNoTracking()
            .Where(profile => profile.IsActive && curricula.Contains(profile.CurriculumId))
            .ToListAsync();
    }

    private static Mock<IUserAdministrationService> TraineeNames()
    {
        var people = new List<UserIdentityDetails>
        {
            Person("amara", "Amara", "Okafor"),
            Person("bongani", "Bongani", "Dlamini"),
            Person("chen", "Chen", "Wei"),
            Person("dineo", "Dineo", "Mokoena"),
            Person("emeka", "Emeka", "Obi"),
            Person("farai", "Farai", "Moyo"),
            Person("gugu", "Gugu", "Zulu")
        };

        // Names are looked up for exactly the trainees listed, whatever roles they hold (the review of T130: the
        // first version loaded every Trainee-role user in the country and fell back to the id for anyone else).
        var users = new Mock<IUserAdministrationService>();
        users
            .Setup(service => service.GetDisplayNamesAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<string> ids, CancellationToken _) =>
                (IReadOnlyDictionary<string, string>)people
                    .Where(person => ids.Contains(person.UserId))
                    .ToDictionary(person => person.UserId, person => $"{person.FirstName} {person.LastName}", StringComparer.Ordinal));
        return users;
    }

    private static UserIdentityDetails Person(string userId, string firstName, string lastName)
        => new(userId, $"{userId}@example.test", firstName, lastName, OurInstitution, [], [], [WombatRoles.Trainee]);

    private static ClaimsPrincipal CommitteeMember(IReadOnlyCollection<int> subSpecialityIds)
        => Principal("committee-member", WombatRoles.CommitteeMember, WombatClaimTypes.SubSpecialityId, subSpecialityIds);

    private static ClaimsPrincipal SpecialityAdmin(int specialityId)
        => Principal("speciality-admin", WombatRoles.SpecialityAdmin, WombatClaimTypes.SpecialityId, [specialityId]);

    private static ClaimsPrincipal SubSpecialityAdmin(int subSpecialityId)
        => Principal("sub-speciality-admin", WombatRoles.SubSpecialityAdmin, WombatClaimTypes.SubSpecialityId, [subSpecialityId]);

    private static ClaimsPrincipal Principal(string userId, string role, string scopeClaimType, IEnumerable<int> scopeIds)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Role, role),
            new(WombatClaimTypes.InstitutionId, OurInstitution.ToString())
        };
        claims.AddRange(scopeIds.Select(id => new Claim(scopeClaimType, id.ToString())));
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }

    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }
}
