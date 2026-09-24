namespace Wombat.Web.Components.Shared.Activities;

/// <summary>
/// The one wording of a T108 credit outcome, shared by an activity's history table and the activity lists (T137).
/// </summary>
/// <remarks>
/// Three-valued, and the three must never read alike: null is "credit was never evaluated" (in flight, or a type that
/// credits nothing by design), zero is "evaluated, and it counted towards nothing", and a positive count is what it
/// counted towards. A list that printed "0" for the first case would report every open request and every reflection as
/// a failed completion.
/// </remarks>
public static class CreditOutcome
{
    public static string Label(int? creditedItemCount) => creditedItemCount switch
    {
        null => "—",
        0 => "None",
        1 => "1 item",
        _ => $"{creditedItemCount} items"
    };
}
