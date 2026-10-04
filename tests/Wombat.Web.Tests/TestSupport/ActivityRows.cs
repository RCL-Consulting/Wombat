using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Dashboards.Assessor;

namespace Wombat.Web.Tests.TestSupport;

/// <summary>
/// An activity row as My activities, Needs you and Home's Needs you card receive it (T342, flow 03): the columns, and what
/// <c>ActivityRowDetails</c> adds, a holder, a nominee, a return and a name, filled as the queries fill them.
/// </summary>
public static class ActivityRows
{
    public static readonly DateTime When = new(2026, 9, 29, 7, 30, 0, DateTimeKind.Utc);

    /// <summary>
    /// A Mini-CEX on PAED-003 observed on 2026-09-25, held by <paramref name="holder" /> (the viewer, its author, by
    /// default), named "Mini-CEX (Paediatrics) · PAED-003 · 2026-09-25".
    /// </summary>
    public static ActivitySummaryDto Row(
        int id,
        string state = "draft",
        string stateLabel = "Draft",
        string typeName = "Mini-CEX (Paediatrics)",
        string? nominee = "David Naidoo",
        ActivityTypeShape shape = ActivityTypeShape.Rated,
        ActivityHolderDto? holder = null,
        int? creditedItemCount = null,
        string epaCode = "PAED-003",
        DateOnly? observedOn = null)
    {
        var observed = observedOn ?? new DateOnly(2026, 9, 25);
        return new ActivitySummaryDto(
            id,
            2,
            "mini_cex_cpsa",
            typeName,
            "trainee-1",
            state,
            stateLabel,
            When,
            When,
            5000,
            epaCode,
            "Take a history",
            true,
            observed,
            true,
            creditedItemCount)
        {
            Holder = holder ?? new ActivityHolderDto(ActivityHolderKind.Author, "trainee-1", "Sipho Ndlovu", true, When),
            NomineeName = nominee,
            DisplayName = $"{typeName} · {epaCode} · {observed:yyyy-MM-dd}",
            Shape = shape,
            // Her own list, its EPA an item of her curriculum (T355, build review G4).
            EpaPageOpens = true
        };
    }

    /// <summary>
    /// A row of an assessor's "Waiting for you" (T350, note 5), as <c>WaitingForYou</c> fills it: the registrar's name, the
    /// name without a nominee, how long it has waited since <paramref name="since" /> (<see cref="When" /> by default) and
    /// whether that is overdue.
    /// </summary>
    public static ActivitySummaryDto Waiting(
        int id,
        string typeName = "Mini-CEX (Paediatrics)",
        string subjectName = "Anele Dlamini",
        string state = "requested",
        string stateLabel = "Requested",
        int waitedDays = 0,
        bool overdue = false,
        DateTime? since = null,
        string epaCode = "PAED-001",
        DateOnly? observedOn = null)
    {
        var observed = observedOn ?? new DateOnly(2026, 9, 20);
        var updated = since ?? When;
        return new ActivitySummaryDto(
            id, 2, "mini_cex_cpsa", typeName, $"trainee-{id}", state, stateLabel, updated.AddDays(-1), updated,
            5000, epaCode, "Take a history", true, observed, true, null)
        {
            SubjectName = subjectName,
            DisplayName = $"{typeName} · {epaCode} · {observed:yyyy-MM-dd}",
            WaitedDays = waitedDays,
            IsOverdue = overdue
        };
    }

    /// <summary>
    /// A row of an assessor's "Decided by you" (T350, note 6), as <c>DecidedByYou</c> fills it: the state the decision left
    /// it in, whether that is finished, and when (<see cref="When" /> by default).
    /// </summary>
    public static ActivitySummaryDto Decided(
        int id,
        string typeName = "Mini-CEX (Paediatrics)",
        string subjectName = "Anele Dlamini",
        string state = "completed",
        string stateLabel = "Completed",
        bool isFinished = true,
        DateTime? decidedOn = null,
        int? creditedItemCount = 1)
    {
        var decided = decidedOn ?? When;
        return new ActivitySummaryDto(
            id, 2, "mini_cex_cpsa", typeName, $"trainee-{id}", state, stateLabel, decided.AddDays(-1), decided,
            5000, "PAED-001", "Take a history", true, new DateOnly(2026, 9, 20), true, creditedItemCount)
        {
            SubjectName = subjectName,
            DisplayName = $"{typeName} · PAED-001 · 2026-09-20",
            DecidedOn = decided,
            IsFinished = isFinished,
            // As DecidedOnYours reads the seeded Mini-CEX: its decline is the assessor's, and it credits (T355, R4).
            Declined = state == "declined",
            CanCredit = true
        };
    }

    /// <summary>
    /// The Assessor's Home as <c>GetAssessorDashboardSummaryQuery</c> returns it (T350): every waiting row, and the decided
    /// read's first page of five with its total.
    /// </summary>
    public static AssessorDashboardSummaryDto AssessorHome(
        IReadOnlyList<ActivitySummaryDto> waiting, IReadOnlyList<ActivitySummaryDto> decided, int? decidedTotal = null)
        => new(
            new WaitingForYouDto(waiting, waiting.Count(row => row.IsOverdue), 7),
            new ActivityListPageDto(decided, 1, 5, decidedTotal ?? decided.Count));

    /// <summary>A reflection returned to its author by Sarah Botha on 2026-09-29 (SAST).</summary>
    public static ActivitySummaryDto Returned(int id) =>
        Row(id, typeName: "Reflective Exercise (Paediatrics)", nominee: "Sarah Botha", shape: ActivityTypeShape.DiscussedOrReviewed,
                epaCode: "PAED-001", observedOn: new DateOnly(2026, 9, 9)) with
        {
            Returned = new ActivityReturnDto("sarah", "Sarah Botha", new DateTime(2026, 9, 29, 13, 30, 0, DateTimeKind.Utc), "Say more.")
        };
}
