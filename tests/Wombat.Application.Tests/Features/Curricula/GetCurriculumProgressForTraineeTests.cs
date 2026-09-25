using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Curricula;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Curricula;

/// <summary>
/// The progress page's read model (T130): every item read against its target for the window containing "today",
/// with the College's D14 exemption and the previous window. "Today" is pinned by <c>AsOf</c>, so no figure here
/// depends on the date the suite runs.
/// </summary>
public sealed class GetCurriculumProgressForTraineeTests
{
    /// <summary>Semester 2 of 2026.</summary>
    private static readonly DateOnly AsOf = new(2026, 9, 23);

    private const int SemesterItemId = 1;   // PAED-001, three per semester
    private const int YearItemId = 2;       // PAED-002, one per academic year

    [Fact]
    public async Task ASemesterItemReadsTheCurrentSemesterOnly_AndKeepsLastSemestersResult()
    {
        // Rows in three semesters for one item. Before T130 there was one row per item, and the handler built a
        // dictionary keyed on the item, which would throw here on the duplicate key.
        await using var db = CreateDb();
        SeedCurriculum(db);
        AddRow(db, SemesterItemId, 2025, 2, counts: 1);
        AddRow(db, SemesterItemId, 2026, 1, counts: 2, reached: 2, lastObservedOn: new DateOnly(2026, 6, 30));
        AddRow(db, SemesterItemId, 2026, 2, counts: 1, reached: 0, lastObservedOn: new DateOnly(2026, 8, 12));
        db.SaveChanges();

        var summary = await Read(db);
        var item = summary.Items.Single(entry => entry.EpaCode == "PAED-001");

        item.IsPerSemester.Should().BeTrue();
        item.Target.Should().Be(3);
        item.Current.Name.Should().Be("Semester 2, 2026");
        item.Current.Months.Should().Be("July to November");
        item.Current.NominalEnd.Should().Be(new DateOnly(2026, 11, 30));
        item.Current.Status.Should().Be(QuotaWindowStatus.Counting);
        item.Current.Count.Should().Be(1);
        item.Current.IsMet.Should().BeFalse();
        item.Current.Shortfall.Should().Be(2);
        item.Current.PercentOfTarget.Should().Be(33);
        item.Current.MinimumLevelReachedCount.Should().Be(0);
        item.Current.LastObservedOn.Should().Be(new DateOnly(2026, 8, 12));

        var previous = item.Previous.Should().NotBeNull().And.Subject.As<QuotaWindowDto>();
        previous.Name.Should().Be("Semester 1, 2026");
        previous.Count.Should().Be(2);
        previous.Shortfall.Should().Be(1);
    }

    [Fact]
    public async Task AnAcademicYearItemAddsBothSemestersOfTheYear_AndIgnoresOtherYears()
    {
        await using var db = CreateDb();
        SeedCurriculum(db);
        AddRow(db, YearItemId, 2025, 2, counts: 4);
        AddRow(db, YearItemId, 2026, 1, counts: 1, lastObservedOn: new DateOnly(2026, 3, 10));
        AddRow(db, YearItemId, 2026, 2, counts: 1, lastObservedOn: new DateOnly(2026, 7, 2));
        AddRow(db, YearItemId, 2027, 1, counts: 9);   // future-dated: nothing bounds ObservedOn
        db.SaveChanges();

        var item = (await Read(db)).Items.Single(entry => entry.EpaCode == "PAED-002");

        item.IsPerSemester.Should().BeFalse();
        item.Current.Name.Should().Be("2026 academic year");
        item.Current.Months.Should().Be("January to November");
        item.Current.Count.Should().Be(2);
        item.Current.IsMet.Should().BeTrue("the target is one per academic year");
        item.Current.LastObservedOn.Should().Be(new DateOnly(2026, 7, 2));
        item.Previous!.Name.Should().Be("2025 academic year");
        item.Previous.Count.Should().Be(4);
    }

    [Fact]
    public async Task EachWindowSaysWhetherItsLastEncounterDateWasStated()
    {
        // T219. The page marks an undated last encounter, so the read model says which it is, per window. This
        // semester's row is undated; last semester's is stated.
        await using var db = CreateDb();
        SeedCurriculum(db);
        AddRow(db, SemesterItemId, 2026, 1, counts: 2, lastObservedOn: new DateOnly(2026, 6, 30));
        AddRow(db, SemesterItemId, 2026, 2, counts: 1, lastObservedOn: new DateOnly(2026, 8, 12), declared: false);
        db.SaveChanges();

        var item = (await Read(db)).Items.Single(entry => entry.EpaCode == "PAED-001");

        (item.Current.LastObservedOn, item.Current.LastObservedOnDeclared).Should().Be(((DateOnly?)new DateOnly(2026, 8, 12), false));
        (item.Previous!.LastObservedOn, item.Previous.LastObservedOnDeclared).Should().Be(((DateOnly?)new DateOnly(2026, 6, 30), true));
    }

    [Theory]
    [InlineData(true, false, false)]
    [InlineData(false, true, true)]
    public async Task AnAcademicYearsLastEncounter_IsStatedOnlyIfTheLaterSemestersIs(
        bool firstSemesterDeclared, bool secondSemesterDeclared, bool expected)
    {
        // T219. A year is two semester rows, and its last encounter is the later row's. So whether it was stated is
        // that row's flag, not the earlier row's, and not "either row's".
        await using var db = CreateDb();
        SeedCurriculum(db);
        AddRow(db, YearItemId, 2026, 1, counts: 1, lastObservedOn: new DateOnly(2026, 3, 10), declared: firstSemesterDeclared);
        AddRow(db, YearItemId, 2026, 2, counts: 1, lastObservedOn: new DateOnly(2026, 7, 2), declared: secondSemesterDeclared);
        db.SaveChanges();

        var item = (await Read(db)).Items.Single(entry => entry.EpaCode == "PAED-002");

        item.Current.Name.Should().Be("2026 academic year", "guard");
        (item.Current.LastObservedOn, item.Current.LastObservedOnDeclared).Should().Be(((DateOnly?)new DateOnly(2026, 7, 2), expected));
    }

    [Fact]
    public async Task AWindowWithNothingCredited_HasNoLastEncounter_AndSoNothingStated()
    {
        await using var db = CreateDb();
        SeedCurriculum(db);
        db.SaveChanges();

        var item = (await Read(db)).Items.Single(entry => entry.EpaCode == "PAED-001");

        (item.Current.LastObservedOn, item.Current.LastObservedOnDeclared).Should().Be(((DateOnly?)null, false));
    }

    [Fact]
    public async Task ChangingAnItemsWindowRereadsTheSameRows_WithNoRebuild()
    {
        // D41: storage is per semester whatever the item says, so an administrator switching an item from
        // semester to academic year changes the reading and needs nothing re-bucketed.
        await using var db = CreateDb();
        SeedCurriculum(db);
        AddRow(db, SemesterItemId, 2026, 1, counts: 2);
        AddRow(db, SemesterItemId, 2026, 2, counts: 1);
        db.SaveChanges();

        (await Read(db)).Items.Single(entry => entry.EpaCode == "PAED-001").Current.Count.Should().Be(1);

        db.CurriculumItems.Single(entry => entry.Id == SemesterItemId).QuotaPeriod = QuotaPeriod.AcademicYear;
        db.SaveChanges();

        (await Read(db)).Items.Single(entry => entry.EpaCode == "PAED-001").Current.Count.Should().Be(3);
    }

    [Fact]
    public async Task ListsEveryCurriculumItem_IncludingThoseWithoutCredit()
    {
        // Item-driven, not row-driven: a period that has just begun has no rows, and it must read "0 of 3".
        await using var db = CreateDb();
        SeedCurriculum(db);
        db.SaveChanges();

        var summary = await Read(db);

        summary.Items.Select(entry => entry.EpaCode).Should().Equal("PAED-001", "PAED-002");
        summary.Items.Should().OnlyContain(entry => entry.Current.Count == 0 && !entry.Current.IsMet && entry.Current.LastObservedOn == null);
    }

    [Fact]
    public async Task AnotherInstitutionsLocalItemIsNotATargetForThisTrainee()
    {
        // A curriculum row is shared by every adopting institution. Institution 2's local extra must not appear
        // on an institution-1 trainee's page as "0 of 1", a target they could never meet.
        await using var db = CreateDb();
        SeedCurriculum(db);
        db.Epas.Add(new Epa { Id = 3, SubSpecialityId = 1, OwningInstitutionId = 2, Code = "LOCAL-2", Title = "Someone else's extra" });
        db.CurriculumItems.Add(new CurriculumItem { Id = 3, CurriculumId = 1, EpaId = 3, OwningInstitutionId = 2, RequiredCount = 1, MinimumLevelOrder = 3, WindowMonths = 12 });
        db.Epas.Add(new Epa { Id = 4, SubSpecialityId = 1, OwningInstitutionId = 1, Code = "LOCAL-1", Title = "Our own extra" });
        db.CurriculumItems.Add(new CurriculumItem { Id = 4, CurriculumId = 1, EpaId = 4, OwningInstitutionId = 1, RequiredCount = 1, MinimumLevelOrder = 3, WindowMonths = 12 });
        db.SaveChanges();

        (await Read(db)).Items.Select(entry => entry.EpaCode).Should().Equal("LOCAL-1", "PAED-001", "PAED-002");
    }

    /// <summary>
    /// T158. A deactivated EPA leaves the picker, so its item is a target nobody can file against: it leaves the page
    /// and every figure the summary counts. Deactivating deletes nothing, so reactivating brings it back with the
    /// credit it had.
    /// </summary>
    [Fact]
    public async Task AnItemWhoseEpaIsDeactivated_LeavesTheSummary_AndReturnsWithItsCreditWhenReactivated()
    {
        await using var db = CreateDb();
        SeedCurriculum(db);
        AddRow(db, SemesterItemId, 2026, 2, counts: 3, reached: 3, lastObservedOn: new DateOnly(2026, 8, 12));
        db.SaveChanges();

        db.Epas.Single(epa => epa.Id == 1).Deactivate(DateTime.MinValue);
        db.SaveChanges();

        var retired = await Read(db);

        retired.Items.Select(entry => entry.EpaCode).Should().Equal(["PAED-002"],
            "PAED-001's EPA is deactivated: it cannot be filed against, so it is no target");
        retired.HasSemesterItems.Should().BeFalse("the only per-semester item is retired");
        retired.SemesterTargetsMet.Should().Be(0, "its met target leaves the count with it");
        retired.SemesterTargetsApplying.Should().Be(0);
        retired.HasYearItems.Should().BeTrue();
        retired.YearTargetsApplying.Should().Be(1);

        db.Epas.Single(epa => epa.Id == 1).Reactivate();
        db.SaveChanges();

        var restored = await Read(db);

        restored.Items.Select(entry => entry.EpaCode).Should().Equal("PAED-001", "PAED-002");
        var item = restored.Items.Single(entry => entry.EpaCode == "PAED-001");
        item.Current.Count.Should().Be(3, "deactivating deleted nothing");
        item.Current.IsMet.Should().BeTrue();
        restored.SemesterTargetsMet.Should().Be(1);
        restored.SemesterTargetsApplying.Should().Be(1);
    }

    [Fact]
    public async Task SummaryCountsTargetsMetByKind()
    {
        await using var db = CreateDb();
        SeedCurriculum(db);
        AddRow(db, SemesterItemId, 2026, 2, counts: 3);
        db.SaveChanges();

        var summary = await Read(db);

        summary.CurrentSemesterName.Should().Be("Semester 2, 2026");
        summary.CurrentSemesterMonths.Should().Be("July to November");
        summary.SemesterTargetsMet.Should().Be(1);
        summary.SemesterTargetsApplying.Should().Be(1);
        summary.YearTargetsMet.Should().Be(0);
        summary.YearTargetsApplying.Should().Be(1);
        summary.SemesterTargetsStart.Should().BeNull();
        summary.YearTargetsStart.Should().BeNull();
        summary.IsAfterTeachingYear.Should().BeFalse();
        summary.TraineeStage.Should().Be(3, "the programme started 2024-01-01");
    }

    [Fact]
    public async Task AMidPeriodStartIsExempt_CountsStillShow_AndTheDateTargetsStartIsGiven()
    {
        // D14 with D42's reading. A start on 15 August is after semester 2's first month, and in the second half of
        // the year, so neither target applies until 1 January. The encounter already recorded still shows: D14
        // waives the target, not the evidence.
        await using var db = CreateDb();
        SeedCurriculum(db, programmeStart: new DateOnly(2026, 8, 15));
        AddRow(db, SemesterItemId, 2026, 2, counts: 1);
        db.SaveChanges();

        var summary = await Read(db);
        var semesterItem = summary.Items.Single(entry => entry.EpaCode == "PAED-001");

        semesterItem.Current.Status.Should().Be(QuotaWindowStatus.ExemptPartialPeriod);
        semesterItem.Current.Count.Should().Be(1);
        semesterItem.Current.IsMet.Should().BeFalse();
        semesterItem.Current.Shortfall.Should().Be(0, "nothing is owed while exempt");
        semesterItem.Current.PercentOfTarget.Should().Be(0);
        semesterItem.Current.FirstCountedName.Should().Be("semester 1, 2027");
        semesterItem.Current.FirstCountedOn.Should().Be(new DateOnly(2027, 1, 1));
        semesterItem.Previous.Should().BeNull("the trainee had not started in semester 1");

        summary.SemesterTargetsApplying.Should().Be(0);
        summary.YearTargetsApplying.Should().Be(0);
        summary.SemesterTargetsStart.Should().Be(new QuotaStartDto("semester 1, 2027", new DateOnly(2027, 1, 1)));
        summary.YearTargetsStart.Should().Be(new QuotaStartDto("the 2027 academic year", new DateOnly(2027, 1, 1)));
    }

    [Fact]
    public async Task AStartInTheFirstMonthOfASemesterCounts()
    {
        // D42: 5 July is inside semester 2's first month, so the semester target applies at once. The academic
        // year began in January, so the yearly target is waived until next year.
        await using var db = CreateDb();
        SeedCurriculum(db, programmeStart: new DateOnly(2026, 7, 5));
        db.SaveChanges();

        var summary = await Read(db);

        summary.Items.Single(entry => entry.EpaCode == "PAED-001").Current.Applies.Should().BeTrue();
        summary.Items.Single(entry => entry.EpaCode == "PAED-002").Current.IsExempt.Should().BeTrue();
        summary.SemesterTargetsStart.Should().BeNull();
        summary.YearTargetsStart!.StartsOn.Should().Be(new DateOnly(2027, 1, 1));
    }

    [Fact]
    public async Task BeforeTheProgrammeStartsNothingIsOwed()
    {
        await using var db = CreateDb();
        SeedCurriculum(db, programmeStart: new DateOnly(2027, 1, 1));
        db.SaveChanges();

        var summary = await Read(db);

        summary.ProgrammeNotStarted.Should().BeTrue();
        summary.Items.Should().OnlyContain(entry => entry.Current.NotStarted);
        summary.SemesterTargetsStart.Should().Be(new QuotaStartDto("semester 1, 2027", new DateOnly(2027, 1, 1)));
        summary.TraineeStage.Should().BeNull();
    }

    [Fact]
    public async Task InDecemberTheSecondSemesterIsStillOpen_AndThePageKnowsTheTeachingYearHasEnded()
    {
        await using var db = CreateDb();
        SeedCurriculum(db);
        AddRow(db, SemesterItemId, 2026, 2, counts: 2);
        db.SaveChanges();

        var summary = await Read(db, new DateOnly(2026, 12, 15));

        summary.IsAfterTeachingYear.Should().BeTrue();
        summary.CurrentSemesterName.Should().Be("Semester 2, 2026");
        summary.Items.Single(entry => entry.EpaCode == "PAED-001").Current.Count.Should().Be(2);
    }

    [Fact]
    public async Task ReportsWhenTheTrainingYearChangedInsideTheWindow()
    {
        // D17: a start on 1 March 2025 moves to training year 2 on 1 March 2026, inside semester 1 of 2026.
        // Encounters either side were judged against different minima, and the page says so.
        await using var db = CreateDb();
        SeedCurriculum(db, programmeStart: new DateOnly(2025, 3, 1));
        db.SaveChanges();

        var summary = await Read(db, new DateOnly(2026, 5, 1));
        var item = summary.Items.Single(entry => entry.EpaCode == "PAED-001");

        item.TrainingYearChangedOn.Should().Be(new DateOnly(2026, 3, 1));
        summary.TraineeStage.Should().Be(2);
        (await Read(db, new DateOnly(2026, 9, 1))).Items.Single(entry => entry.EpaCode == "PAED-001")
            .TrainingYearChangedOn.Should().BeNull("no training year boundary falls in semester 2 of 2026 for this trainee");
    }

    [Fact]
    public async Task NoActiveProfile_ReturnsNull()
    {
        await using var db = CreateDb();
        SeedCurriculum(db);
        db.SaveChanges();

        var handler = new GetCurriculumProgressForTraineeQueryHandler(db);
        var result = await handler.Handle(
            new GetCurriculumProgressForTraineeQuery("trainee-without-profile", TestPrincipals.Administrator(), AsOf), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task PinnedItem_RendersTheRungLabelTheCollegePrints_NotTheOrdinal()
    {
        // T100/T118 finding 2. On the CPSA v11.1 ladder the ordinal and the rung are different
        // numbers: ordinal 4 is rung "3b". The progress page said "Minimum level 4", naming a rung
        // that is on the ladder but is NOT the one required.
        await using var db = CreateDb();
        SeedCurriculum(db);
        SeedCpsaLadderAndPinItem1(db);
        db.SaveChanges();

        var summary = await Read(db);

        var paed001 = summary.Items.Single(r => r.EpaCode == "PAED-001");
        paed001.EffectiveMinimumLevelOrder.Should().Be(4, "the stored ordinal is the comparison key and does not move");
        paed001.EffectiveMinimumLevelLabel.Should().Be("3b");

        // PAED-002 is on the same curriculum but was left unpinned, so it still degrades to the ordinal.
        summary.Items.Single(r => r.EpaCode == "PAED-002").EffectiveMinimumLevelLabel.Should().Be("3");
    }

    [Fact]
    public async Task PinnedItem_WhoseOrdinalIsNotARungOnItsScale_FallsBackToTheOrdinal()
    {
        // A pin can outlive the rung it names (T109: nothing protects the rungs from being removed
        // underneath a pinned item). Print the number rather than nothing.
        await using var db = CreateDb();
        SeedCurriculum(db);
        SeedCpsaLadderAndPinItem1(db);
        db.Set<EntrustmentLevel>().Remove(db.Set<EntrustmentLevel>().Local.Single(l => l.Order == 4));
        db.SaveChanges();

        (await Read(db)).Items.Single(r => r.EpaCode == "PAED-001").EffectiveMinimumLevelLabel.Should().Be("4");
    }

    [Fact]
    public async Task ASpanListsEveryWindowBackToItsFirstDay_NewestFirst_OpeningWithTheCurrentAndPrevious()
    {
        // T169: the portfolio export prints every period it covers. The first two are exactly what the progress page
        // shows, read by the same tally; the span reaches back to the window CONTAINING its first day.
        await using var db = CreateDb();
        SeedCurriculum(db);
        AddRow(db, SemesterItemId, 2024, 2, counts: 9);   // before the span: not listed
        AddRow(db, SemesterItemId, 2025, 1, counts: 1, reached: 1);
        AddRow(db, SemesterItemId, 2025, 2, counts: 3, reached: 2);
        AddRow(db, SemesterItemId, 2026, 1, counts: 2, reached: 2);
        AddRow(db, SemesterItemId, 2026, 2, counts: 1);
        AddRow(db, YearItemId, 2025, 2, counts: 1);
        db.SaveChanges();
        db.ChangeTracker.Clear();

        var summary = await ReadSpan(db, periodsFrom: new DateOnly(2025, 3, 1));

        var semester = summary.Items.Single(entry => entry.EpaCode == "PAED-001");
        semester.EpaId.Should().Be(1);
        var periods = semester.Periods.Should().NotBeNull().And.Subject.As<IReadOnlyList<QuotaWindowDto>>();
        periods.Select(period => period.Name).Should().Equal(
            "Semester 2, 2026", "Semester 1, 2026", "Semester 2, 2025", "Semester 1, 2025");
        periods.Select(period => period.Count).Should().Equal(1, 2, 3, 1);
        periods.Select(period => period.IsMet).Should().Equal(false, false, true, false);
        periods.Select(period => period.MinimumLevelReachedCount).Should().Equal(0, 2, 2, 1);
        periods[0].Should().Be(semester.Current, "the span opens with the window the progress page calls current");
        periods[1].Should().Be(semester.Previous, "and then the one it calls previous");

        var year = summary.Items.Single(entry => entry.EpaCode == "PAED-002");
        year.Periods!.Select(period => period.Name).Should().Equal("2026 academic year", "2025 academic year");
        year.Periods!.Select(period => period.Count).Should().Equal(0, 1);
    }

    [Fact]
    public async Task ASpanStopsAtTheProgrammeStart_AndKeepsTheWaivedFirstPeriod()
    {
        // A start on 15 August 2025 waives semester 2 of 2025 and the 2025 year (D14, D42). Those windows are listed,
        // because encounters in them are evidence; nothing before them is, whatever the span asks for.
        await using var db = CreateDb();
        SeedCurriculum(db, programmeStart: new DateOnly(2025, 8, 15));
        AddRow(db, SemesterItemId, 2025, 2, counts: 2);
        db.SaveChanges();
        db.ChangeTracker.Clear();

        var summary = await ReadSpan(db, periodsFrom: DateOnly.MinValue);

        var semester = summary.Items.Single(entry => entry.EpaCode == "PAED-001");
        semester.Periods!.Select(period => (period.Name, period.Status)).Should().Equal(
            ("Semester 2, 2026", QuotaWindowStatus.Counting),
            ("Semester 1, 2026", QuotaWindowStatus.Counting),
            ("Semester 2, 2025", QuotaWindowStatus.ExemptPartialPeriod));
        semester.Periods![2].Count.Should().Be(2, "a waived period still shows what was recorded in it");

        summary.Items.Single(entry => entry.EpaCode == "PAED-002").Periods!.Select(period => (period.Name, period.Status))
            .Should().Equal(
                ("2026 academic year", QuotaWindowStatus.Counting),
                ("2025 academic year", QuotaWindowStatus.ExemptPartialPeriod));
    }

    [Fact]
    public async Task WithoutASpan_NoPeriodsAreRead()
    {
        // The progress page and the dashboard read the current and previous windows only.
        await using var db = CreateDb();
        SeedCurriculum(db);
        AddRow(db, SemesterItemId, 2026, 1, counts: 2);
        db.SaveChanges();

        (await Read(db)).Items.Should().OnlyContain(entry => entry.Periods == null);
    }

    [Fact]
    public async Task ASpanThatStartsAfterTheDaysWindow_ListsNoWindow()
    {
        // T169 review: an open-ended export from 1 January 2027, read on 23 September 2026, covers no period yet. Listing
        // the window containing the day would print semester 2 of 2026, which the span does not cover.
        await using var db = CreateDb();
        SeedCurriculum(db);
        AddRow(db, SemesterItemId, 2026, 2, counts: 1);
        db.SaveChanges();
        db.ChangeTracker.Clear();

        var summary = await ReadSpan(db, periodsFrom: new DateOnly(2027, 1, 1));

        summary.Items.Should().OnlyContain(entry => entry.Periods != null && entry.Periods.Count == 0);
        summary.Items.Single(entry => entry.EpaCode == "PAED-001").Current.Name
            .Should().Be("Semester 2, 2026", "the day's own window is still read; only the span's list is empty");

        // A span starting on the window's last counted day still lists it.
        (await ReadSpan(db, periodsFrom: new DateOnly(2026, 12, 31))).Items.Single(entry => entry.EpaCode == "PAED-001")
            .Periods!.Select(period => period.Name).Should().Equal("Semester 2, 2026");
    }

    [Fact]
    public async Task AResolvedProfileIsReadWhateverItsState_WhileTheProgressPageReadsOnlyAnActiveOne()
    {
        // T169 review: the portfolio export reads the programme its cover names, which for a graduate is a completed,
        // inactive profile. The progress page still has nothing to show for one.
        await using var db = CreateDb();
        SeedCurriculum(db);
        AddRow(db, SemesterItemId, 2026, 1, counts: 2);
        db.SaveChanges();
        db.Set<TraineeProfile>().Local.Single().Complete(new DateOnly(2026, 6, 30), today: new DateOnly(2026, 6, 30));
        db.SaveChanges();
        db.ChangeTracker.Clear();

        var page = await new GetCurriculumProgressForTraineeQueryHandler(db).Handle(
            new GetCurriculumProgressForTraineeQuery("trainee-1", TestPrincipals.Trainee("trainee-1"), AsOf),
            CancellationToken.None);
        page.Should().BeNull();

        var summary = await ReadSpan(db, periodsFrom: DateOnly.MinValue, asOf: new DateOnly(2026, 6, 30));
        summary.Items.Single(entry => entry.EpaCode == "PAED-001").Current
            .Should().Match<QuotaWindowDto>(window => window.Name == "Semester 1, 2026" && window.Count == 2);
    }

    [Fact]
    public async Task ADeactivatedProgramme_IsReadWithItsEnd_SoThePeriodItCutShortIsExempt_AndNoStartIsAnnounced()
    {
        // T209, D49. Deactivated on 15 October 2026, before November, the last month of semester 2 and of the academic
        // year: neither holds a target, and both still show what was credited. The summary must not announce when targets
        // "start" either: that is D14's sentence for a late start, and this trainee's first counted window is 2024's.
        await using var db = CreateDb();
        SeedCurriculum(db);
        AddRow(db, SemesterItemId, 2026, 1, counts: 1);
        AddRow(db, SemesterItemId, 2026, 2, counts: 2);
        db.SaveChanges();
        db.Set<TraineeProfile>().Local.Single().Deactivate(new DateOnly(2026, 10, 15), today: new DateOnly(2026, 10, 15));
        db.SaveChanges();
        db.ChangeTracker.Clear();

        var summary = await ReadSpan(db, periodsFrom: new DateOnly(2026, 1, 1), asOf: new DateOnly(2026, 10, 15));

        var semester = summary.Items.Single(entry => entry.EpaCode == "PAED-001");
        semester.Current.Status.Should().Be(QuotaWindowStatus.ExemptProgrammeEnded);
        semester.Current.Count.Should().Be(2);
        semester.Current.Shortfall.Should().Be(0);
        semester.Previous!.Status.Should().Be(QuotaWindowStatus.Counting);
        semester.Previous.Shortfall.Should().Be(2, "semester 1 was served in full, so it is still short");
        semester.Periods!.Select(period => (period.Name, period.Status)).Should().Equal(
            ("Semester 2, 2026", QuotaWindowStatus.ExemptProgrammeEnded),
            ("Semester 1, 2026", QuotaWindowStatus.Counting));

        summary.Items.Single(entry => entry.EpaCode == "PAED-002").Current.Status
            .Should().Be(QuotaWindowStatus.ExemptProgrammeEnded);
        summary.SemesterTargetsApplying.Should().Be(0);
        summary.YearTargetsApplying.Should().Be(0);
        summary.SemesterTargetsStart.Should().BeNull();
        summary.YearTargetsStart.Should().BeNull();
    }

    [Fact]
    public async Task AGraduationInTheLastMonth_HoldsThatPeriodToItsFullTarget()
    {
        // D49: an end in November leaves semester 2 and the academic year whole.
        await using var db = CreateDb();
        SeedCurriculum(db);
        AddRow(db, SemesterItemId, 2026, 2, counts: 2);
        db.SaveChanges();
        db.Set<TraineeProfile>().Local.Single().Complete(new DateOnly(2026, 11, 20), today: new DateOnly(2026, 11, 20));
        db.SaveChanges();
        db.ChangeTracker.Clear();

        var summary = await ReadSpan(db, periodsFrom: new DateOnly(2026, 7, 1), asOf: new DateOnly(2026, 11, 20));

        var semester = summary.Items.Single(entry => entry.EpaCode == "PAED-001");
        semester.Current.Status.Should().Be(QuotaWindowStatus.Counting);
        semester.Current.Shortfall.Should().Be(1);
        summary.Items.Single(entry => entry.EpaCode == "PAED-002").Current.Status.Should().Be(QuotaWindowStatus.Counting);
        summary.SemesterTargetsApplying.Should().Be(1);
    }

    [Fact]
    public async Task TheProgressPage_ReadsThePreferredProfile_TheOneCreditLandsOn()
    {
        // T185. Two active profiles, which only a store without Postgres's one-active-profile index can hold, and the one
        // place the progress page's pick could part from credit's. The page took the latest programme start (profile 1);
        // credit, the activity scope stamp and the export take the preferred profile, the highest id (profile 2). The
        // page now reads profile 2's programme, so the targets it shows are the ones credit counts against.
        await using var db = CreateDb();
        SeedCurriculum(db);
        db.Curricula.Add(new Curriculum
        {
            Id = 2, SubSpecialityId = 1, Name = "Earlier programme",
            Version = "2020.1", EffectiveFrom = new DateOnly(2020, 1, 1), IsActive = true
        });
        db.CurriculumItems.Add(new CurriculumItem
        {
            Id = 20, CurriculumId = 2, EpaId = 2, RequiredCount = 2, QuotaPeriod = QuotaPeriod.Semester,
            MinimumLevelOrder = 3, WindowMonths = 36
        });
        db.Set<TraineeProfile>().Add(new TraineeProfile
        {
            Id = 2, UserId = "trainee-1", InstitutionId = 1, CurriculumId = 2,
            ProgrammeStartDate = new DateOnly(2020, 1, 1), ExpectedCompletionDate = new DateOnly(2027, 1, 1),
            IsActive = true
        });
        db.SaveChanges();

        var summary = await Read(db);

        summary.ProgrammeStartDate.Should().Be(new DateOnly(2020, 1, 1));
        summary.Items.Select(item => item.CurriculumItemId).Should().Equal(20);
    }

    private static async Task<TraineeCurriculumProgressSummaryDto> ReadSpan(
        ApplicationDbContext db, DateOnly periodsFrom, DateOnly? asOf = null)
    {
        var profile = await db.Set<TraineeProfile>().AsNoTracking().SingleAsync(entity => entity.UserId == "trainee-1");
        return await TraineeQuotaProgressReader.ReadForProfileAsync(
            db, profile, asOf ?? AsOf, periodsFrom, CancellationToken.None);
    }

    private static async Task<TraineeCurriculumProgressSummaryDto> Read(ApplicationDbContext db, DateOnly? asOf = null)
    {
        var handler = new GetCurriculumProgressForTraineeQueryHandler(db);
        var result = await handler.Handle(
            new GetCurriculumProgressForTraineeQuery("trainee-1", TestPrincipals.Trainee("trainee-1"), asOf ?? AsOf), CancellationToken.None);
        return result.Should().NotBeNull().And.Subject.As<TraineeCurriculumProgressSummaryDto>();
    }

    private static void AddRow(
        ApplicationDbContext db,
        int curriculumItemId,
        int year,
        int semester,
        int counts,
        int reached = 0,
        DateOnly? lastObservedOn = null,
        bool declared = true)
        => db.CurriculumItemProgresses.Add(new CurriculumItemProgress
        {
            CurriculumItemId = curriculumItemId,
            TraineeUserId = "trainee-1",
            AcademicYear = year,
            Semester = semester,
            CountsSoFar = counts,
            MinimumLevelReachedCount = reached,
            LastObservedOn = lastObservedOn,
            LastObservedOnDeclared = lastObservedOn is not null && declared,
            LastUpdated = new DateTime(2026, 9, 1, 8, 0, 0, DateTimeKind.Utc)
        });

    private static void SeedCpsaLadderAndPinItem1(ApplicationDbContext db)
    {
        db.Set<EntrustmentScale>().Add(new EntrustmentScale
        {
            Id = 7, Name = "CPSA Paediatric Entrustment Scale v11.1"
        });
        var labels = new[] { "1", "2", "3a", "3b", "4", "5" };
        for (var order = 1; order <= labels.Length; order++)
        {
            db.Set<EntrustmentLevel>().Add(new EntrustmentLevel
            {
                Id = 100 + order, ScaleId = 7, Order = order, Label = labels[order - 1]
            });
        }

        // SeedCurriculum has added but not saved, so reach for the tracked entity.
        db.CurriculumItems.Local.Single(i => i.Id == SemesterItemId).ScaleId = 7;
    }

    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static void SeedCurriculum(ApplicationDbContext db, DateOnly? programmeStart = null)
    {
        db.Institutions.Add(new Institution { Id = 1, Name = "KGK" });
        db.Specialities.Add(new Speciality { Id = 1, CollegeId = 1, Name = "Paediatrics" });
        db.SubSpecialities.Add(new SubSpeciality { Id = 1, SpecialityId = 1, Name = "General Paediatrics" });

        db.Epas.Add(new Epa { Id = 1, SubSpecialityId = 1, Code = "PAED-001", Title = "Clerk an acute admission" });
        db.Epas.Add(new Epa { Id = 2, SubSpecialityId = 1, Code = "PAED-002", Title = "Manage a ward" });

        db.Curricula.Add(new Curriculum
        {
            Id = 1, SubSpecialityId = 1, Name = "FCPaed Part 1",
            Version = "2026.1", EffectiveFrom = new DateOnly(2025, 1, 1), IsActive = true
        });

        db.CurriculumItems.Add(new CurriculumItem
        {
            Id = SemesterItemId, CurriculumId = 1, EpaId = 1, RequiredCount = 3, QuotaPeriod = QuotaPeriod.Semester,
            MinimumLevelOrder = 4, WindowMonths = 36
        });
        db.CurriculumItems.Add(new CurriculumItem
        {
            Id = YearItemId, CurriculumId = 1, EpaId = 2, RequiredCount = 1, QuotaPeriod = QuotaPeriod.AcademicYear,
            MinimumLevelOrder = 3, WindowMonths = 36
        });

        db.Set<TraineeProfile>().Add(new TraineeProfile
        {
            Id = 1, UserId = "trainee-1", InstitutionId = 1, CurriculumId = 1,
            ProgrammeStartDate = programmeStart ?? new DateOnly(2024, 1, 1),
            ExpectedCompletionDate = new DateOnly(2028, 1, 1),
            IsActive = true
        });
    }
}
