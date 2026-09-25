using Wombat.Domain.Curricula;

namespace Wombat.Application.Features.Curricula;

/// <summary>
/// What deactivating an EPA means (T158, T196, D48): its curriculum items are not in force while it is inactive. The one
/// definition of that, shared by every reader that treats an item as a target or a place credit can land.
/// </summary>
/// <remarks>
/// <para>
/// An item whose EPA is inactive is not offered by the EPA picker (<c>ActivityReferenceDataService</c>), and no tool
/// is offered for its ladder alone (<c>ListActivityTypesQuery</c>). No progress surface lists it as a target
/// (<c>TraineeQuotaProgressReader</c>, <c>CurriculumCoverageReader</c>). Before T158 only the picker filtered: a
/// deactivated EPA left the picker but stayed on every progress page, still owed, and an activity already filed
/// against it still credited it.
/// </para>
/// <para>
/// <b>Deactivating pauses credit; it does not cancel it (D48).</b> Credit is judged at the moment of credit, which is
/// the crediting transition's time, by <see cref="InForceAt" />: the EPA is active, or the moment falls before
/// <c>Epa.DeactivatedOn</c>. So a completion while the EPA is inactive credits nothing and stamps
/// <c>CreditedItemCount = 0</c>, and the T108 warning on the activity explains it. <c>RebuildCurriculumProgress</c>
/// judges each completion at its own moment by the same rule, so a rebuild while the EPA is inactive keeps the credit
/// earned while it was in force, and credits nothing filed since (before T196 it judged every EPA as active or not on the
/// day the rebuild ran, and dropped the lot). Reactivating the EPA credits what was filed during the pause
/// (<c>UpdateEpaCommandHandler</c>, via <c>ResumedEpaCredit</c>), which is what makes one timestamp enough history.
/// </para>
/// <para>
/// The moment is the completion's, not the encounter's. It is the moment the live path judges, so a rebuild reproduces
/// the live outcome: an encounter observed before the deactivation but completed after it was paused live, and a rebuild
/// judged on the encounter date would credit it.
/// </para>
/// <para>
/// The tool gate reads neither predicate (D48). It judges the instrument against every item a directive names, in force
/// or not: a paused completion is credited when its EPA is reactivated, so the list it will be credited under is the one
/// that must pass it. Refusing an EPA for being out of force would make the gate a curriculum-membership check, which
/// D48 rejected.
/// </para>
/// <para>
/// Not applied where an item is read as a record rather than as a target: the curriculum editor, the checks that
/// stop an entrustment scale in use from being deleted, and the trajectory chart's ladder for evidence already
/// recorded. Those still see every item.
/// </para>
/// </remarks>
public static class CurriculumItemsInForce
{
    /// <summary>The items whose EPA is active now: what the pickers offer and the progress pages list.</summary>
    public static IQueryable<CurriculumItem> InForce(this IQueryable<CurriculumItem> items)
        => items.Where(item => item.Epa.IsActive);

    /// <summary>
    /// The items a completion credited at <paramref name="moment" /> may credit: their EPA is active, or its current
    /// pause began after the moment. The query form of <c>Epa.InForceAt</c>; for any moment up to now it agrees with
    /// <see cref="InForce" /> about every EPA that is active now.
    /// </summary>
    public static IQueryable<CurriculumItem> InForceAt(this IQueryable<CurriculumItem> items, DateTime moment)
        => items.Where(item => item.Epa.IsActive
            || (item.Epa.DeactivatedOn != null && moment < item.Epa.DeactivatedOn));

    /// <summary>
    /// The items whose EPA is deactivated: the complement of <see cref="InForce" />, for a reader that must say why an
    /// EPA is no target rather than silently leave it out (the portfolio export's per-EPA section, T169).
    /// </summary>
    public static IQueryable<CurriculumItem> NotInForce(this IQueryable<CurriculumItem> items)
        => items.Where(item => !item.Epa.IsActive);
}
