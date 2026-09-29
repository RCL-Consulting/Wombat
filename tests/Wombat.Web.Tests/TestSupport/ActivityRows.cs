using Wombat.Application.Features.Activities.Dtos;

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
            Shape = shape
        };
    }

    /// <summary>A reflection returned to its author by Sarah Botha on 2026-09-29 (SAST).</summary>
    public static ActivitySummaryDto Returned(int id) =>
        Row(id, typeName: "Reflective Exercise (Paediatrics)", nominee: "Sarah Botha", shape: ActivityTypeShape.DiscussedOrReviewed,
                epaCode: "PAED-001", observedOn: new DateOnly(2026, 9, 9)) with
        {
            Returned = new ActivityReturnDto("sarah", "Sarah Botha", new DateTime(2026, 9, 29, 13, 30, 0, DateTimeKind.Utc), "Say more.")
        };
}
