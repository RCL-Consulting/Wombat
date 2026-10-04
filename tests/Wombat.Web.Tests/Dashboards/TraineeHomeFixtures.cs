using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Application.Features.EntrustmentDecisions;
using Wombat.Domain.Curricula;

namespace Wombat.Web.Tests.Dashboards;

/// <summary>
/// The Trainee's Home as <c>GetTraineeDashboardSummaryQuery</c> returns it for flow 05's cast (T355, R1; C6): the
/// decisions on her requests as <c>DecidedOnYours</c> fills them, and the standing in summary mode (no latest rating).
/// </summary>
internal static class TraineeHomeFixtures
{
    public static readonly DateTime DecidedAt = new(2026, 10, 3, 15, 15, 0, DateTimeKind.Utc);

    /// <summary>
    /// A decision on her request, as <c>DecidedOnYours</c> fills it: "Mini-CEX (Paediatrics) · PAED-001 · 2026-09-23",
    /// "to David Naidoo", completed on 2026-10-03, crediting one item, with its count line.
    /// </summary>
    public static ActivitySummaryDto Decision(
        int id,
        string typeName = "Mini-CEX (Paediatrics)",
        string epaCode = "PAED-001",
        int epaId = 2,
        DateOnly? observedOn = null,
        string nominee = "David Naidoo",
        string state = "completed",
        string stateLabel = "Completed",
        int? credited = 1,
        bool epaInForce = true,
        EpaCountLineDto? countLine = null,
        string? displayName = null)
    {
        var observed = observedOn ?? new DateOnly(2026, 9, 23);
        return new ActivitySummaryDto(
            id, 2, "mini_cex_cpsa", typeName, "trainee-1", state, stateLabel, DecidedAt.AddDays(-5), DecidedAt,
            epaId, epaCode, "Providing paediatric emergency care to children", epaInForce, observed, true, credited)
        {
            Holder = new ActivityHolderDto(ActivityHolderKind.Done, null, null, false, DecidedAt),
            NomineeName = nominee,
            DisplayName = displayName ?? $"{typeName} · {epaCode} · {observed:yyyy-MM-dd}",
            DecidedOn = DecidedAt,
            IsFinished = state != "declined",
            // As DecidedOnYours reads the seeded Mini-CEX: its decline is the assessor's, and it credits (T355, R4).
            Declined = state == "declined",
            CanCredit = true,
            CountLine = countLine
        };
    }

    /// <summary>A count line in the current window: "PAED-001: 1 of 3 this semester." (or met, at 3).</summary>
    public static EpaCountLineDto CurrentLine(string epaCode = "PAED-001", int epaId = 2, int count = 1, int target = 3)
        => new(epaId, epaCode, QuotaPeriod.Semester, Window("Semester 2, 2026", new(2026, 7, 1), new(2026, 11, 30), count, target), IsCurrentWindow: true);

    /// <summary>A count line in an older window: "PAED-001, Semester 1, 2026: 3 of 3, met."</summary>
    public static EpaCountLineDto OlderLine(string epaCode = "PAED-001", int epaId = 2, int count = 3, int target = 3)
        => new(epaId, epaCode, QuotaPeriod.Semester, Window("Semester 1, 2026", new(2026, 1, 1), new(2026, 6, 30), count, target), IsCurrentWindow: false);

    public static QuotaWindowDto Window(string name, DateOnly start, DateOnly end, int count, int target)
        => new(
            name, "July to November", start, end, QuotaWindowStatus.Counting, count, target,
            IsMet: count >= target, Shortfall: Math.Max(0, target - count), PercentOfTarget: Math.Min(100, count * 100 / target),
            MinimumLevelReachedCount: count, LastObservedOn: null, LastObservedOnDeclared: false,
            FirstCountedName: null, FirstCountedOn: null);

    /// <summary>
    /// The standing in summary mode for training year <paramref name="targetYear" />, its EPAs as given, as on
    /// <paramref name="asOf" /> (2026-10-03 by default).
    /// </summary>
    public static EntrustmentStandingDto Standing(int targetYear, IReadOnlyList<EpaStandingDto> epas, DateOnly? asOf = null)
        => new(
            asOf ?? new DateOnly(2026, 10, 3), new DateOnly(2023, 1, 14), targetYear, ProgrammeNotStarted: false, epas,
            new ExitRuleReadinessDto(epas.Count, 0, [], epas.Select(epa => epa.EpaCode).ToList()));

    /// <summary>An EPA's standing: no STAR by default; with <paramref name="star" />, a STAR at that level.</summary>
    public static EpaStandingDto Epa(
        int epaId,
        string code,
        string title,
        string? star = null,
        EntrustmentStandingStatus status = EntrustmentStandingStatus.NoDecision,
        DateOnly? expires = null,
        string yearTarget = "5")
        => new(
            epaId, epaId, code, title, "CPSA Paediatric Entrustment Scale v11.1", IsLocal: false,
            YearTargetOrder: 6, yearTarget, YearTargetIsExitLevel: false, ExitLevelOrder: 6, ExitLevelLabel: "5",
            star is null ? null : new StandingDecisionDto(epaId, 5, star, null, new DateOnly(2026, 4, 2), expires),
            status, EntrustmentStandingStatus.NoDecision, LatestRating: null);
}
