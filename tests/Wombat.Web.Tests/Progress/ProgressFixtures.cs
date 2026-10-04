using Wombat.Application.Features.Curricula.Quota;
using Wombat.Domain.Curricula;

namespace Wombat.Web.Tests.Progress;

/// <summary>
/// Hand-built read models for flow 05's words (T355): the windows and items the reader would build for the cast on the
/// day given. 3 October 2026 (D) is in semester 2 of the 2026 academic year, whose College end is 30 November.
/// </summary>
internal static class ProgressFixtures
{
    public static readonly DateOnly D = new(2026, 10, 3);

    public static readonly DateOnly December = new(2026, 12, 7);

    public static readonly Window Semester1Of2026 = new("Semester 1, 2026", "January to June", new(2026, 1, 1), new(2026, 6, 30));
    public static readonly Window Semester2Of2026 = new("Semester 2, 2026", "July to November", new(2026, 7, 1), new(2026, 11, 30));
    public static readonly Window Year2026 = new("2026 academic year", "January to November", new(2026, 1, 1), new(2026, 11, 30));
    public static readonly Window Year2025 = new("2025 academic year", "January to November", new(2025, 1, 1), new(2025, 11, 30));

    public static QuotaWindowDto Counting(
        Window window, int count, int target, int minimumReached = 0, DateOnly? lastObservedOn = null, bool declared = true)
        => new(
            window.Name, window.Months, window.Start, window.NominalEnd, QuotaWindowStatus.Counting, count, target,
            IsMet: count >= target,
            Shortfall: Math.Max(0, target - count),
            PercentOfTarget: Math.Min(100, count * 100 / target),
            minimumReached,
            lastObservedOn,
            LastObservedOnDeclared: lastObservedOn is not null && declared,
            FirstCountedName: null,
            FirstCountedOn: null);

    public static QuotaWindowDto Waived(Window window, QuotaWindowStatus status, int count, int target, string? firstCountedName = null)
        => new(
            window.Name, window.Months, window.Start, window.NominalEnd, status, count, target,
            IsMet: false, Shortfall: 0, PercentOfTarget: 0, MinimumLevelReachedCount: count,
            LastObservedOn: null, LastObservedOnDeclared: false,
            firstCountedName, FirstCountedOn: firstCountedName is null ? null : new DateOnly(2027, 1, 1));

    public static TraineeCurriculumProgressDto Item(
        string code,
        QuotaPeriod period,
        int target,
        QuotaWindowDto current,
        QuotaWindowDto? previous = null,
        IReadOnlyList<QuotaWindowDto>? periods = null,
        string title = "Providing paediatric emergency care to children",
        string minimum = "5")
        => new(1, EpaId: 2, code, title, period, target, current, previous,
            EffectiveMinimumLevelOrder: 6, EffectiveMinimumLevelLabel: minimum, TrainingYearChangedOn: null, periods)
        {
            ExitLevelOrder = 6,
            ExitLevelLabel = "5",
            MinimumByTrainingYear = true
        };

    public static TraineeCurriculumProgressSummaryDto Summary(
        DateOnly asOf,
        DateOnly programmeStart,
        int? stage,
        IReadOnlyList<TraineeCurriculumProgressDto> items,
        QuotaStartDto? semesterStart = null,
        QuotaStartDto? yearStart = null)
    {
        var semesterItems = items.Where(item => item.IsPerSemester).ToList();
        var yearItems = items.Where(item => !item.IsPerSemester).ToList();
        var nominalEnd = new DateOnly(asOf.Year, 11, 30);

        return new TraineeCurriculumProgressSummaryDto(
            asOf,
            programmeStart,
            stage,
            CurrentSemesterName: $"Semester 2, {asOf.Year}",
            CurrentSemesterMonths: "July to November",
            CurrentSemesterNominalEnd: nominalEnd,
            IsAfterTeachingYear: asOf > nominalEnd,
            SemesterTargetsMet: semesterItems.Count(item => item.Current.IsMet),
            SemesterTargetsApplying: semesterItems.Count(item => item.Current.Applies),
            YearTargetsMet: yearItems.Count(item => item.Current.IsMet),
            YearTargetsApplying: yearItems.Count(item => item.Current.Applies),
            HasSemesterItems: semesterItems.Count > 0,
            HasYearItems: yearItems.Count > 0,
            ProgrammeNotStarted: asOf < programmeStart,
            SemesterTargetsStart: semesterItems.Count > 0 ? semesterStart : null,
            YearTargetsStart: yearItems.Count > 0 ? yearStart : null,
            Items: items);
    }

    public sealed record Window(string Name, string Months, DateOnly Start, DateOnly NominalEnd);
}
