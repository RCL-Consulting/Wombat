using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Activities.Services;
using Wombat.Domain.Activities;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;

namespace Wombat.Application.Features.Epas;

/// <summary>
/// Reactivating an EPA credits what was filed while it was inactive (T196, D48). Planned with every read before the
/// reactivation touches anything, and applied without a single await, so the audit pipeline's catch can never commit
/// half of it.
/// </summary>
/// <remarks>
/// <para>
/// Deactivating pauses credit: a completion while the EPA is inactive credits none of its items and is stamped zero
/// (<c>CurriculumItemsInForce.InForceAt</c>). This is the other half. The paused completions are those credited at or
/// after <see cref="Epa.DeactivatedOn" /> (the current pause; every earlier one was closed by a reactivation like this).
/// Each is planned through the live path's own <see cref="ICreditApplier" /> with <see cref="CreditSubject.ResumedEpaId" />
/// set, which credits this EPA's items alone: the completion's other items were judged when it completed, and credit
/// never re-litigates them. The applier's dedupe makes a completion that already holds this EPA's credit a no-op.
/// </para>
/// <para>
/// The result is the credit a rebuild would give, because the rebuild judges each completion at its own moment and, once
/// the EPA is active, every moment is in force. Applying in filing order, before any later completion can land, writes
/// <c>LastActivityId</c> as the rebuild would, so no Administrator rebuild is needed after a reactivation, and the admin
/// who reactivates (a CollegeAdmin for a national EPA, the owning InstitutionalAdmin for a local one) need not hold the
/// Administrator role.
/// </para>
/// <para>
/// It crosses institutions. A national EPA's items are on every adopting institution's curriculum, so reactivating it
/// credits trainees there too, as deactivating it paused them. Nothing about any activity leaves this class: the caller
/// gets a count.
/// </para>
/// <para>
/// <b>Two races it does not close</b> (T196 review), each one request wide. A completion that reads the EPA as inactive,
/// and saves after this reactivation has read its candidates, is stamped zero and is in no plan: it stays uncredited
/// until an Administrator rebuild, which after the reactivation finds every moment in force. And a completion that read
/// the EPA as active just before a deactivation committed, but whose moment falls after <see cref="Epa.DeactivatedOn" />,
/// keeps live credit a rebuild while the EPA is inactive takes away; the reactivation that ends the pause restores it,
/// either way. Closing them needs the completion's save to conflict with the EPA's, which neither writes; no lock is
/// taken for a window this narrow.
/// </para>
/// </remarks>
internal sealed class ResumedEpaCredit
{
    private readonly ICreditApplier _creditApplier;
    private readonly IReadOnlyList<(Activity Activity, CreditPlan Plan)> _plans;

    private ResumedEpaCredit(ICreditApplier creditApplier, IReadOnlyList<(Activity Activity, CreditPlan Plan)> plans)
    {
        _creditApplier = creditApplier;
        _plans = plans;
    }

    /// <summary>
    /// The activities that may have been paused, with what a replay needs loaded: those pinned to a version whose rules
    /// credit, in a state such a version ends in, with a transition at or after the pause began. All such activities when
    /// the pause has no recorded start, which only a fixture that constructs an inactive EPA directly can produce, and
    /// which the credit rule reads as "never in force".
    /// </summary>
    /// <remarks>
    /// Narrowed in the query, before anything is tracked (T196 review). Only a crediting completion can hold paused
    /// credit, and <see cref="PlanAsync" /> still asks <see cref="CreditReplay.PinnedCreditingType" /> of each one, so
    /// the narrowing changes what is read, never what is credited. Without it, every draft, reflection and research output
    /// touched since the pause was loaded with its type's versions and its transitions into an interactive save, and an
    /// EPA inactive since before T196, whose pause the migration dates from its creation, loaded the whole table.
    /// </remarks>
    /// <param name="activities">
    /// <c>Set&lt;Activity&gt;()</c>, passed by the handler so the read stays in its body, where
    /// <c>ActivityReadBoundaryTests</c> sees it and its scope exemption states why it is confined.
    /// </param>
    public static async Task<List<Activity>> LoadCandidatesAsync(
        IQueryable<Activity> activities,
        IQueryable<ActivityTypeVersion> versions,
        DateTime? pausedSince,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(activities);
        ArgumentNullException.ThrowIfNull(versions);

        var versionRows = await versions
            .AsNoTracking()
            .Select(version => new { version.Id, version.WorkflowJson, version.CreditRulesJson })
            .ToListAsync(cancellationToken);

        var crediting = CreditReplay.CreditingVersions(
            versionRows.Select(version => (version.Id, version.WorkflowJson, version.CreditRulesJson)));

        if (crediting.VersionIds.Count == 0)
        {
            return [];
        }

        var versionIds = crediting.VersionIds;
        var terminalStates = crediting.TerminalStates;

        var candidates = activities
            .Where(activity => terminalStates.Contains(activity.CurrentState)
                && activity.ActivityType.Versions.Any(version =>
                    version.Version == activity.SchemaVersion && versionIds.Contains(version.Id)));

        if (pausedSince is { } since)
        {
            candidates = candidates.Where(activity => activity.Transitions.Any(transition => transition.OccurredOn >= since));
        }

        return await candidates
            .Include(activity => activity.ActivityType)
                .ThenInclude(activityType => activityType.Versions)
            .Include(activity => activity.Transitions)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Every read: which of the candidates completed during the pause under rules that credit, and what each would credit
    /// on this EPA. Mutates nothing.
    /// </summary>
    /// <param name="epa">The EPA about to be reactivated, still inactive: its <see cref="Epa.DeactivatedOn" /> is read here.</param>
    public static async Task<ResumedEpaCredit> PlanAsync(
        ICreditApplier creditApplier,
        Epa epa,
        IEnumerable<Activity> candidates,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(creditApplier);
        ArgumentNullException.ThrowIfNull(epa);
        ArgumentNullException.ThrowIfNull(candidates);

        var pausedSince = epa.DeactivatedOn;
        var plans = new List<(Activity, CreditPlan)>();

        foreach (var activity in CreditReplay.InFilingOrder(candidates))
        {
            var pinnedType = CreditReplay.PinnedCreditingType(activity);
            if (pinnedType is null)
            {
                continue;
            }

            var subject = CreditSubject.Of(activity) with { ResumedEpaId = epa.Id };

            // Completed before the pause: judged while the EPA was in force, so already credited (or refused for its own
            // reasons, which a reactivation does not revisit).
            if (pausedSince is { } since && subject.CreditedAt < since)
            {
                continue;
            }

            var plan = await creditApplier.PlanAsync(subject, pinnedType, cancellationToken);
            if (plan.Credits.Count > 0)
            {
                plans.Add((activity, plan));
            }
        }

        return new ResumedEpaCredit(creditApplier, plans);
    }

    /// <summary>
    /// Writes the planned credit and adds it to each completion's T108 stamp. Synchronous and read-free: it runs between
    /// the reactivation and the save, where nothing may fail.
    /// </summary>
    /// <returns>How many completions it credited.</returns>
    public int Apply()
    {
        // A row one completion's apply adds is not in a later plan, which read the table before it existed. Handing it on
        // is what stops two paused completions in one semester opening two rows for it. Stored rows need no help: every
        // plan's tracking query returned the same instances, so an earlier increment is already on them.
        var added = new List<CurriculumItemProgress>();
        var completionsCredited = 0;

        foreach (var (activity, plan) in _plans)
        {
            var existing = plan.ExistingRows
                .Concat(added)
                .Distinct<CurriculumItemProgress>(ReferenceEqualityComparer.Instance)
                .ToList();

            var credited = _creditApplier.Apply(plan with { ExistingRows = existing }, activity);
            if (credited.UpdatedRows.Count == 0)
            {
                continue;
            }

            added.AddRange(credited.UpdatedRows.Where(row => !existing.Contains(row, ReferenceEqualityComparer.Instance)));

            // Added to, not replaced: the stamp already counts the items the completion credited when it happened, and
            // these are the ones its EPA's pause held back. The sum is what a rebuild stamps.
            if (CreditReplay.CreditedTransition(activity) is { } transition)
            {
                transition.CreditedItemCount = (transition.CreditedItemCount ?? 0) + credited.UpdatedRows.Count;
                transition.CreditScaleMismatchCount = (transition.CreditScaleMismatchCount ?? 0) + credited.ScaleMismatchCount;
            }

            completionsCredited++;
        }

        return completionsCredited;
    }
}
