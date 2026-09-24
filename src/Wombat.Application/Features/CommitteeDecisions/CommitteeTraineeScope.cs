using System.Security.Claims;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Domain.CommitteeDecisions;

namespace Wombat.Application.Features.CommitteeDecisions;

/// <summary>
/// Which trainees a committee panel may review: those who train at the panel's institution. (T182)
/// </summary>
/// <remarks>
/// <para>
/// A review freezes its trainee's evidence into a snapshot when it starts, and its ratified entrustment decisions
/// supersede the trainee's current ones. A panel at one institution holding a review of another institution's trainee
/// therefore reads that institution's evidence and overwrites that institution's decisions. Until T182 the trainee id
/// was free text on the scheduling form, checked against nothing, and only an InstitutionalAdmin's PANEL was checked:
/// a Coordinator, SpecialityAdmin or SubSpecialityAdmin skipped even that.
/// </para>
/// <list type="bullet">
/// <item>
/// To schedule (<see cref="DemandSchedulableAsync" />), the trainee's preferred profile must be at the panel's
/// institution, whoever schedules, a global Administrator included: a panel's members may act only on reviews of its
/// own institution's trainees, so a review of anyone else would be one only an Administrator could run. Anyone but an
/// Administrator must also oversee that trainee through a role that schedules reviews, which is T113's rule without its
/// CommitteeMember arm (<see cref="TraineeScopeResolver.IsAdministeredOrCoordinatedBy" />): same institution, and for a
/// SpecialityAdmin or SubSpecialityAdmin the trainee's own speciality or sub-speciality. An unknown trainee, a trainee
/// with no profile and a trainee at another institution are refused alike, and so is an unknown panel, so the refusal
/// never confirms that an id names someone. The scheduling page's picker (<see cref="ListSchedulableTraineesQuery" />)
/// offers exactly the trainees this accepts, through <see cref="MayScheduleFor" />.
/// </item>
/// <item>
/// To act on a review already scheduled (<see cref="DemandTraineeAtPanelInstitutionAsync" />), its trainee must
/// still train at the panel's institution: start, record, close, ratify, and stage, remove or issue an entrustment
/// decision against it. A trainee can move institution after being scheduled, and the panel must not then read their
/// new institution's evidence or supersede its decisions. Who may act is still the command's own panel ladder (a
/// member, the chair); this adds only the trainee. A global Administrator is not refused here, so a review stranded by
/// a move can still be finished or closed.
/// </item>
/// </list>
/// <para>
/// An appeal is checked on neither side. It is lodged against a review already ratified, which passed the check above
/// when it was. Resolving it reads no evidence and supersedes no entrustment decision; it records the appeal body's
/// answer on the panel's own review. A trainee who moves after their review is ratified keeps their recourse, and the
/// appeal body can still answer it.
/// </para>
/// <para>
/// Every check here runs before the handler's first mutation. The audit pipeline saves the request's DbContext from its
/// catch, so a check that ran after a mutation would have the refusal commit it.
/// </para>
/// </remarks>
internal static class CommitteeTraineeScope
{
    /// <summary>The one refusal to schedule, for an unknown trainee or panel as for an out-of-scope one.</summary>
    internal const string NotSchedulable =
        "A review can only be scheduled on a panel of your institution, for a trainee at that institution whose " +
        "programme you oversee.";

    /// <summary>
    /// The refusal a global Administrator gets for a trainee who does not train at the panel's institution. An
    /// Administrator may see who trains where, so it says what is wrong.
    /// </summary>
    internal const string NotAtPanelInstitution =
        "A panel reviews only trainees at its own institution, and this trainee does not train there.";

    /// <summary>The refusal to act on a review whose trainee does not train at the panel's institution.</summary>
    internal const string TraineeNotAtPanelInstitution =
        "This review's trainee does not train at the panel's institution, so the panel cannot act on it.";

    /// <summary>
    /// Whether this caller may put this trainee before this panel. The one predicate the handler and the picker share.
    /// </summary>
    /// <param name="trainee">Where the trainee trains, or null when they hold no profile, or the id names nobody.</param>
    public static bool MayScheduleFor(ClaimsPrincipal principal, DecisionPanel panel, TraineeScope? trainee)
    {
        ArgumentNullException.ThrowIfNull(principal);
        ArgumentNullException.ThrowIfNull(panel);

        if (trainee is null || panel.InstitutionId != trainee.InstitutionId)
        {
            return false;
        }

        // Not IsOverseenBy: its CommitteeMember arm would let someone who also sits on a committee borrow that role's
        // reach for a right it does not grant. The role check stays as the statement of who schedules; the arms of
        // IsAdministeredOrCoordinatedBy are those same roles.
        return principal.IsAdministrator() ||
               (CommitteeDecisionAuthorization.MayScheduleReviews(principal) &&
                TraineeScopeResolver.IsAdministeredOrCoordinatedBy(trainee, principal));
    }

    /// <summary>
    /// Refuses to schedule a review of this trainee on this panel unless <see cref="MayScheduleFor" /> allows it.
    /// </summary>
    public static async Task DemandSchedulableAsync(
        IApplicationDbContext dbContext,
        ClaimsPrincipal principal,
        DecisionPanel panel,
        string traineeUserId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(principal);

        var trainee = await TraineeScopeResolver.ResolveAsync(dbContext, traineeUserId, cancellationToken);
        if (!MayScheduleFor(principal, panel, trainee))
        {
            throw new UnauthorizedAccessException(principal.IsAdministrator() ? NotAtPanelInstitution : NotSchedulable);
        }
    }

    /// <summary>
    /// The trainees this caller may put before this panel: the picker half of <see cref="DemandSchedulableAsync" />.
    /// </summary>
    /// <remarks>
    /// Candidates are the trainees at the panel's institution, each resolved by the set form of the handler's own
    /// resolver and kept only if <see cref="MayScheduleFor" /> keeps them. The candidate set narrows nothing the
    /// predicate would keep, since the predicate demands the panel's institution of every trainee, whoever asks.
    /// </remarks>
    public static async Task<IReadOnlyList<string>> ListSchedulableAsync(
        IApplicationDbContext dbContext,
        ClaimsPrincipal principal,
        DecisionPanel panel,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(principal);
        ArgumentNullException.ThrowIfNull(panel);

        var trainees = await TraineeScopeResolver.ResolveAllAsync(dbContext, panel.InstitutionId, cancellationToken);

        return trainees
            .Where(trainee => MayScheduleFor(principal, panel, trainee.Value))
            .Select(trainee => trainee.Key)
            .ToArray();
    }

    /// <summary>
    /// Refuses to act on a review unless its trainee's preferred profile is at the panel's institution.
    /// </summary>
    /// <remarks>
    /// The review's <see cref="CommitteeReview.Panel" /> must be loaded. Run it after the command's own panel ladder,
    /// so that someone with no business on the panel is told that first, and before the first mutation.
    /// </remarks>
    public static async Task DemandTraineeAtPanelInstitutionAsync(
        IApplicationDbContext dbContext,
        ClaimsPrincipal principal,
        CommitteeReview review,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(principal);
        ArgumentNullException.ThrowIfNull(review);

        if (principal.IsAdministrator())
        {
            return;
        }

        var trainee = await TraineeScopeResolver.ResolveAsync(dbContext, review.TraineeUserId, cancellationToken);
        if (trainee is null || review.Panel.InstitutionId != trainee.InstitutionId)
        {
            throw new UnauthorizedAccessException(TraineeNotAtPanelInstitution);
        }
    }
}
