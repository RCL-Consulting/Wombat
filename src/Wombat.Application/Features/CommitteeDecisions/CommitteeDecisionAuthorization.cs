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
        if (principal.IsInRole(WombatRoles.Administrator) || principal.IsInRole(WombatRoles.Coordinator))
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
