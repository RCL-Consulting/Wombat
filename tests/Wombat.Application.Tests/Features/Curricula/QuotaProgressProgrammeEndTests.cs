using FluentAssertions;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Domain.Curricula;

namespace Wombat.Application.Tests.Features.Curricula;

/// <summary>
/// D49 (T209) through <see cref="QuotaProgressCalculator" />, the one place stored buckets are read against a target: a
/// period the programme ended in before its last month, and a period after the end, are never met, never short, and
/// still show what was credited in them.
/// </summary>
public sealed class QuotaProgressProgrammeEndTests
{
    private const int ItemId = 1;
    private static readonly DateOnly Start = new(2025, 1, 1);

    private static readonly QuotaProgressRow[] Rows =
    [
        new(ItemId, 2025, 1, CountsSoFar: 3, MinimumLevelReachedCount: 3, LastObservedOn: new DateOnly(2025, 5, 1), LastObservedOnDeclared: true),
        new(ItemId, 2025, 2, CountsSoFar: 1, MinimumLevelReachedCount: 1, LastObservedOn: new DateOnly(2025, 9, 1), LastObservedOnDeclared: true),
        new(ItemId, 2026, 1, CountsSoFar: 1, MinimumLevelReachedCount: 0, LastObservedOn: new DateOnly(2026, 3, 1), LastObservedOnDeclared: true),
        new(ItemId, 2026, 2, CountsSoFar: 2, MinimumLevelReachedCount: 2, LastObservedOn: new DateOnly(2026, 8, 1), LastObservedOnDeclared: true)
    ];

    [Fact]
    public void AGraduatesCutShortLastPeriod_IsExempt_AndStillShowsItsCount()
    {
        // Completed on 15 May 2026, before June: semester 1 of 2026 was the last, and it was cut short.
        var end = new DateOnly(2026, 5, 15);

        var periods = QuotaProgressCalculator.Since(ItemId, QuotaPeriod.Semester, 3, Rows, Start, end, DateOnly.MinValue, asOf: end);

        periods.Select(period => (period.Window.Semesters[0], period.Window.Status, period.Count, period.Shortfall, period.IsMet))
            .Should().Equal(
                (new AcademicPeriod(2026, 1), QuotaWindowStatus.ExemptProgrammeEnded, 1, 0, false),
                (new AcademicPeriod(2025, 2), QuotaWindowStatus.Counting, 1, 2, false),
                (new AcademicPeriod(2025, 1), QuotaWindowStatus.Counting, 3, 0, true));
        periods[0].Applies.Should().BeFalse();
        periods[0].PercentOfTarget.Should().Be(0);
    }

    [Fact]
    public void AnEndInTheLastMonth_HoldsTheFullTarget()
    {
        var end = new DateOnly(2026, 6, 10);

        var last = QuotaProgressCalculator.For(ItemId, QuotaPeriod.Semester, 3, Rows, Start, end, asOf: end).Current;

        last.Window.Status.Should().Be(QuotaWindowStatus.Counting);
        last.Shortfall.Should().Be(2, "an end in June leaves semester 1 whole, so it is held to all three");
    }

    [Fact]
    public void APeriodAfterTheEnd_IsOutsideTheProgramme_NotShort()
    {
        // A reader that asks about a day after a deactivation (the export clamps to the end; this guards any that does not).
        var end = new DateOnly(2026, 5, 15);

        var progress = QuotaProgressCalculator.For(ItemId, QuotaPeriod.Semester, 3, Rows, Start, end, asOf: new DateOnly(2026, 9, 23));

        progress.Current.Window.Status.Should().Be(QuotaWindowStatus.AfterProgrammeEnd);
        progress.Current.Count.Should().Be(2, "what was credited after the end is still shown");
        progress.Current.Shortfall.Should().Be(0);
        progress.Current.IsMet.Should().BeFalse();
        progress.Previous!.Window.Status.Should().Be(QuotaWindowStatus.ExemptProgrammeEnded);
    }

    [Fact]
    public void AnAcademicYearTheProgrammeEndsInBeforeNovember_IsExempt()
    {
        var end = new DateOnly(2026, 10, 31);

        var year = QuotaProgressCalculator.For(ItemId, QuotaPeriod.AcademicYear, 1, [], Start, end, asOf: end);

        year.Current.Window.Status.Should().Be(QuotaWindowStatus.ExemptProgrammeEnded);
        year.Current.Shortfall.Should().Be(0);
        year.Previous!.Window.Status.Should().Be(QuotaWindowStatus.Counting);
        year.Previous.Shortfall.Should().Be(1);
    }

    [Fact]
    public void AnEndedWindow_NeverSaysWhenTargetsStart()
    {
        // "Targets start with" is D14's sentence for a late start. The first counted window of a trainee who started on
        // time is long past, and an ended programme's targets did not start later: they stopped.
        var end = new DateOnly(2026, 5, 15);

        var ended = QuotaWindowDto.From(QuotaProgressCalculator.For(ItemId, QuotaPeriod.Semester, 3, Rows, Start, end, asOf: end).Current);
        var after = QuotaWindowDto.From(
            QuotaProgressCalculator.For(ItemId, QuotaPeriod.Semester, 3, Rows, Start, end, asOf: new DateOnly(2026, 9, 1)).Current);

        ended.EndedPartWay.Should().BeTrue();
        ended.IsExempt.Should().BeFalse("IsExempt is the late start's, which the page words as \"you started part-way through\"");
        ended.FirstCountedName.Should().BeNull();
        ended.FirstCountedOn.Should().BeNull();
        after.IsAfterProgrammeEnd.Should().BeTrue();
        after.FirstCountedName.Should().BeNull();

        // A late start still says it.
        var late = QuotaWindowDto.From(
            QuotaProgressCalculator.For(ItemId, QuotaPeriod.Semester, 3, [], new DateOnly(2026, 3, 1), null, asOf: new DateOnly(2026, 4, 1)).Current);
        late.FirstCountedName.Should().Be("semester 2, 2026");
    }
}
