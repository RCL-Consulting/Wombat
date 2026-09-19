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

    public static void DemandReviewScheduling(ClaimsPrincipal principal)
    {
        // InstitutionalAdmin can schedule reviews on panels in their own institution
        // (the handler applies the scope check). This mirrors DemandPanelAdministration,
        // which already admits InstitutionalAdmin — scheduling a review on a panel you can
        // administer should not require a lesser Coordinator role. (T075 / F-4A-1)
        if (principal.IsInRole(WombatRoles.Administrator) ||
            principal.IsInRole(WombatRoles.InstitutionalAdmin) ||
            principal.IsInRole(WombatRoles.Coordinator) ||
            principal.IsInRole(WombatRoles.SpecialityAdmin) ||
            principal.IsInRole(WombatRoles.SubSpecialityAdmin))
        {
            return;
        }

        throw new UnauthorizedAccessException("You are not allowed to schedule committee reviews.");
    }

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
        // A panel with no institution stamp matches nobody here, for the same reason a null scope
        // stamp matches nobody in ActivityReadScope: an unstamped row cannot be claimed by scope,
        // only by membership, which is the next rung down.
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
    /// ReviewDetail.razor is fed by three sibling queries — the review itself, the sampling
    /// concentration report, and the entrustment decisions staged against it — and each used to
    /// carry its own idea of who may read a review, or none at all. One ladder, called by all
    /// three, is the only arrangement in which they cannot drift apart again. The review's
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
            if (review.Panel.InstitutionId.HasValue &&
                !principal.CanAccessInstitution(review.Panel.InstitutionId.Value))
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
    private static bool CoordinatesInstitution(ClaimsPrincipal principal, int? institutionId)
    {
        if (!institutionId.HasValue)
        {
            return false;
        }

        var scopedInstitutionId = principal.GetInstitutionId();
        return scopedInstitutionId.HasValue && scopedInstitutionId.Value == institutionId.Value;
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
