using System.Security.Claims;
using Wombat.Application.Common.Extensions;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.CommitteeDecisions;

internal static class CommitteeDecisionAuthorization
{
    public static string GetRequiredUserId(ClaimsPrincipal principal)
        => principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("The current user identifier is missing.");

    public static void DemandPanelAdministration(ClaimsPrincipal principal)
    {
        if (principal.IsInRole(WombatRoles.Administrator) ||
            principal.IsInRole(WombatRoles.InstitutionalAdmin) ||
            principal.IsInRole(WombatRoles.SpecialityAdmin) ||
            principal.IsInRole(WombatRoles.SubSpecialityAdmin))
        {
            return;
        }

        throw new UnauthorizedAccessException("You are not allowed to manage committee panels.");
    }

    /// <summary>The one refusal to create, change or open a panel outside the caller's scope.</summary>
    internal const string PanelOutOfScope = "You can only manage panels in your institution.";

    /// <summary>
    /// Whether this caller may create, change or open a panel of this scope at this institution. (T182)
    /// </summary>
    /// <remarks>
    /// <para>
    /// A global Administrator may manage any panel. Everyone else manages only panels at their own institution, and
    /// only through a panel-administration role: an InstitutionalAdmin any of them, a SpecialityAdmin or
    /// SubSpecialityAdmin only a Speciality-scoped one, never the institution-wide panel.
    /// </para>
    /// <para>
    /// Before T182 the panel's institution was stamped only when an InstitutionalAdmin created it, and the checks
    /// that read it (<c>CanAccessInstitution</c>, which is true for an InstitutionalAdmin alone) were skipped when it
    /// was null. So a SpecialityAdmin's panel carried no institution, could be rewritten by any panel administrator in
    /// the country, and once T182 compared the panel's institution with the trainee's, could review nobody. Every
    /// panel now carries its institution, and this is the one rule that reads it for administration.
    /// </para>
    /// <para>
    /// Which speciality a SpecialityAdmin's panel covers is not checked here: a panel's speciality is not enforced
    /// against its trainees either, and the two are one question for later.
    /// </para>
    /// </remarks>
    public static bool MayAdministerPanel(ClaimsPrincipal principal, int institutionId, DecisionPanelScope scope)
    {
        ArgumentNullException.ThrowIfNull(principal);

        if (principal.IsAdministrator())
        {
            return true;
        }

        if (principal.GetInstitutionId() != institutionId)
        {
            return false;
        }

        if (principal.IsInstitutionalAdmin())
        {
            return true;
        }

        return scope == DecisionPanelScope.Speciality &&
               (principal.IsInRole(WombatRoles.SpecialityAdmin) || principal.IsInRole(WombatRoles.SubSpecialityAdmin));
    }

    public static void DemandReviewScheduling(ClaimsPrincipal principal)
    {
        if (MayScheduleReviews(principal))
        {
            return;
        }

        throw new UnauthorizedAccessException("You are not allowed to schedule committee reviews.");
    }

    /// <summary>
    /// The roles that put trainees before a panel. Which trainees, and on which panels, is
    /// <see cref="CommitteeTraineeScope" />'s question. (T182)
    /// </summary>
    /// <remarks>
    /// InstitutionalAdmin can schedule reviews on panels in their own institution. This mirrors
    /// DemandPanelAdministration, which already admits InstitutionalAdmin — scheduling a review on a panel you can
    /// administer should not require a lesser Coordinator role. (T075 / F-4A-1)
    /// </remarks>
    public static bool MayScheduleReviews(ClaimsPrincipal principal)
        => principal.IsInRole(WombatRoles.Administrator) ||
           principal.IsInRole(WombatRoles.InstitutionalAdmin) ||
           principal.IsInRole(WombatRoles.Coordinator) ||
           principal.IsInRole(WombatRoles.SpecialityAdmin) ||
           principal.IsInRole(WombatRoles.SubSpecialityAdmin);

    public static void DemandPanelAccess(ClaimsPrincipal principal, DecisionPanel panel)
    {
        if (principal.IsInRole(WombatRoles.Administrator))
        {
            return;
        }

        // A Coordinator supports the programmes of ONE institution, so they reach the panels their
        // own institution runs — not every institution's. The waiver used to be role-only, which
        // made any Coordinator anywhere a reader of every panel's reviews and, through the queries
        // that hang off a review, of every trainee's evidence. (T101 finding E)
        if (principal.IsInRole(WombatRoles.Coordinator) && CoordinatesInstitution(principal, panel.InstitutionId))
        {
            return;
        }

        var userId = GetRequiredUserId(principal);
        if (panel.Members.Any(member => string.Equals(member.UserId, userId, StringComparison.Ordinal)))
        {
            return;
        }

        throw new UnauthorizedAccessException("You are not a member of this decision panel.");
    }

    /// <summary>
    /// The one read ladder for a committee review and for everything computed from it.
    /// </summary>
    /// <remarks>
    /// ReviewDetail.razor is fed by sibling queries — the review itself, the sampling
    /// concentration report, and the entrustment decisions staged against it — and each used to
    /// carry its own idea of who may read a review, or none at all. One ladder, called by all
    /// of them, is the only arrangement in which they cannot drift apart again; the count of the
    /// window's MSF campaigns missing from the snapshot climbs it too (T173). The review's
    /// <see cref="CommitteeReview.Panel" /> and its members must be loaded. (T101 finding E)
    /// </remarks>
    public static void DemandReviewAccess(ClaimsPrincipal principal, CommitteeReview review)
    {
        // The trainee arm comes first deliberately: someone holding Trainee alongside an oversight
        // role is still a trainee about their own record, and must not read a panel's working notes
        // on someone else through the wider role.
        if (principal.IsInRole(WombatRoles.Trainee))
        {
            var userId = GetRequiredUserId(principal);
            if (!string.Equals(review.TraineeUserId, userId, StringComparison.Ordinal))
            {
                throw new UnauthorizedAccessException("You can only view your own committee reviews.");
            }

            if (review.State is not (CommitteeReviewState.Ratified or CommitteeReviewState.UnderAppeal or CommitteeReviewState.Final))
            {
                throw new UnauthorizedAccessException("This review is not yet visible to the trainee.");
            }

            return;
        }

        if (principal.IsAdministrator())
        {
            return;
        }

        if (principal.IsInstitutionalAdmin())
        {
            // An InstitutionalAdmin can view (read-only) any review for a panel in their
            // institution, even without panel membership. Conduct actions (start/record/
            // ratify) remain chair-gated in their respective handlers. (T075 / F-4A-1)
            if (!principal.CanAccessInstitution(review.Panel.InstitutionId))
            {
                throw new UnauthorizedAccessException("You can only view committee reviews for panels in your institution.");
            }

            return;
        }

        DemandPanelAccess(principal, review.Panel);
    }

    /// <summary>
    /// True when the caller's institution claim names the panel's institution. Not
    /// <c>CanAccessInstitution</c>, which answers the narrower question of whether an
    /// InstitutionalAdmin owns an institution; a Coordinator is scoped by the same claim without
    /// being an admin of it.
    /// </summary>
    private static bool CoordinatesInstitution(ClaimsPrincipal principal, int institutionId)
        => principal.GetInstitutionId() == institutionId;

    /// <summary>
    /// The one refusal for a review id the caller may not conduct as its chair, whether or not the id names a review.
    /// (T131, T194 item 1)
    /// </summary>
    internal const string ReviewNotChairedByCaller =
        "The committee review could not be found among the reviews you chair.";

    /// <summary>
    /// Refuses, before anything else is looked at, unless <paramref name="review" /> exists and the caller chairs its
    /// panel or is a global Administrator. An unknown id and a review of another panel get the one refusal. (T131)
    /// </summary>
    /// <remarks>
    /// <para>
    /// The page prints a command's refusal. Before T131 the entrustment commands said "could not be found" for an unknown
    /// id and only then checked the formative flag, the review's state and the chair, each with its own message, so a
    /// committee member of one panel could try review ids and learn which exist, which are formative and how far each
    /// has got (T194 item 1). The state checks now come after this one.
    /// </para>
    /// <para>
    /// A global Administrator may conduct every review, so for them an unknown id is only that. The review's
    /// <see cref="CommitteeReview.Panel" /> and its members must be loaded.
    /// </para>
    /// </remarks>
    public static CommitteeReview DemandChairedReview(ClaimsPrincipal principal, CommitteeReview? review)
    {
        ArgumentNullException.ThrowIfNull(principal);

        if (principal.IsAdministrator())
        {
            return review ?? throw new InvalidOperationException("The committee review could not be found.");
        }

        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (review is null ||
            string.IsNullOrEmpty(userId) ||
            !review.Panel.Members.Any(member =>
                string.Equals(member.UserId, userId, StringComparison.Ordinal) &&
                member.Role == DecisionPanelMemberRole.Chair))
        {
            throw new UnauthorizedAccessException(ReviewNotChairedByCaller);
        }

        return review;
    }

    public static void DemandChairAccess(ClaimsPrincipal principal, DecisionPanel panel)
    {
        if (principal.IsInRole(WombatRoles.Administrator))
        {
            return;
        }

        var userId = GetRequiredUserId(principal);
        if (panel.Members.Any(member =>
            string.Equals(member.UserId, userId, StringComparison.Ordinal) &&
            member.Role == DecisionPanelMemberRole.Chair))
        {
            return;
        }

        throw new UnauthorizedAccessException("Only panel chairs can complete this action.");
    }

    public static void DemandAppealResolverAccess(ClaimsPrincipal principal, DecisionPanel panel)
    {
        if (principal.IsInRole(WombatRoles.Administrator))
        {
            return;
        }

        var userId = GetRequiredUserId(principal);
        if (panel.Members.Any(member =>
            string.Equals(member.UserId, userId, StringComparison.Ordinal) &&
            member.Role is DecisionPanelMemberRole.Chair or DecisionPanelMemberRole.External))
        {
            return;
        }

        throw new UnauthorizedAccessException("Only the appeal body can resolve committee appeals.");
    }

    public static void DemandTraineeSelfAccess(ClaimsPrincipal principal, string traineeUserId)
    {
        if (principal.IsInRole(WombatRoles.Administrator))
        {
            return;
        }

        if (!principal.IsInRole(WombatRoles.Trainee))
        {
            throw new UnauthorizedAccessException("Only trainees can lodge appeals.");
        }

        var userId = GetRequiredUserId(principal);
        if (!string.Equals(userId, traineeUserId, StringComparison.Ordinal))
        {
            throw new UnauthorizedAccessException("You can only appeal your own committee reviews.");
        }
    }
}
