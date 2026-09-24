using Wombat.Domain.Curricula;

namespace Wombat.Application.Features.Curricula;

/// <summary>
/// What deactivating an EPA means (T158): its curriculum items are no longer in force. The one definition of that,
/// shared by every reader that treats an item as a target or a place credit can land.
/// </summary>
/// <remarks>
/// <para>
/// An item whose EPA is inactive is not offered by the EPA picker (<c>ActivityReferenceDataService</c>), and no tool
/// is offered for its ladder alone (<c>ListActivityTypesQuery</c>). Credit does not land on it
/// (<c>CreditTargetResolver</c>, and so the <c>ToolPermissionGate</c> that shares it). No progress surface lists it
/// as a target (<c>TraineeQuotaProgressReader</c>, <c>CurriculumCoverageReader</c>). Before T158 only the picker
/// filtered: a deactivated EPA left the picker but stayed on every progress page, still owed, and an activity already
/// filed against it still credited it.
/// </para>
/// <para>
/// Credit is judged at the moment of credit. A completion while the EPA is inactive credits nothing and stamps
/// <c>CreditedItemCount = 0</c>, so the T108 warning on the activity explains it. Deactivating deletes nothing.
/// Progress rows already credited stay, hidden with their item, and reactivating the EPA brings the item back with
/// them. <c>RebuildCurriculumProgress</c> replays through the same resolver under the rule as it stands. A rebuild
/// while the EPA is inactive therefore drops its rows, and one after reactivation restores them, together with any
/// completion that credited nothing while the EPA was inactive.
/// </para>
/// <para>
/// Not applied where an item is read as a record rather than as a target: the curriculum editor, the checks that
/// stop an entrustment scale in use from being deleted, and the trajectory chart's ladder for evidence already
/// recorded. Those still see every item.
/// </para>
/// </remarks>
public static class CurriculumItemsInForce
{
    /// <summary>The items whose EPA is active.</summary>
    public static IQueryable<CurriculumItem> InForce(this IQueryable<CurriculumItem> items)
        => items.Where(item => item.Epa.IsActive);
}
