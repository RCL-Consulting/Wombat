using System.Security.Claims;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.EntrustmentDecisions;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.EntrustmentDecisions;

internal static class EntrustmentDecisionAuthorization
{
    /// <summary>
    /// The one refusal for a decision id the caller may not revoke, whether or not it exists. (T183)
    /// </summary>
    /// <remarks>
    /// The admin page prints a command's refusal. "Could not be found" for a missing id beside a scope refusal for a
    /// real one would let an administrator walk the ids and learn which decisions other institutions hold, so both are
    /// this. An Administrator may revoke every decision, so for them "not found" says only that.
    /// </remarks>
    internal const string DecisionNotRevocableByCaller =
        "The entrustment decision could not be found among the decisions you may revoke.";

    public static string GetRequiredUserId(ClaimsPrincipal principal)
        => principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("The current user identifier is missing.");

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

        throw new UnauthorizedAccessException("Only panel chairs can issue entrustment decisions.");
    }

    /// <summary>
    /// Refuses to revoke a decision unless the caller is a global Administrator, the chair who issued it, or an
    /// InstitutionalAdmin, SpecialityAdmin or SubSpecialityAdmin who administers the decision's trainee
    /// (<see cref="TraineeScopeResolver.IsAdministeredBy" />). (T183)
    /// </summary>
    /// <remarks>
    /// <para>
    /// Until T183 the admin arm was the role alone, so any hospital's admin could withdraw any trainee's entrustment in
    /// the country. It is <see cref="TraineeScopeResolver.IsAdministeredBy" />, which asks for the role and its scope
    /// together, not "an admin role" and <see cref="TraineeScopeResolver.IsOverseenBy" /> separately: a Coordinator or
    /// CommitteeMember oversees every trainee at their institution, so a user who is also a SpecialityAdmin for one
    /// speciality there would pass the pair for the trainees of every speciality. Revoking is an administrator's act or
    /// the issuing chair's, as it was.
    /// </para>
    /// <para>
    /// The issuing chair's arm is about this decision, not about a role: they issued it, and may withdraw it, wherever the
    /// trainee has since moved. It reads <see cref="EntrustmentDecision.IssuedByChairUserId" />, which is written when the
    /// decision is issued and never again, NOT the issuing panel's current members: a panel's members can be replaced
    /// after the fact, so "the chair of the issuing panel" would admit whoever was made chair since.
    /// </para>
    /// <para>
    /// Call it before anything is changed: the audit pipeline saves the request's DbContext from its catch, so a
    /// mutation staged before a refusal would be committed by the refusal itself.
    /// </para>
    /// </remarks>
    public static async Task DemandRevocationAccessAsync(
        IApplicationDbContext dbContext,
        ClaimsPrincipal principal,
        EntrustmentDecision decision,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(principal);
        ArgumentNullException.ThrowIfNull(decision);

        if (principal.IsAdministrator() || IsIssuer(principal, decision.IssuedByChairUserId))
        {
            return;
        }

        var scope = await TraineeScopeResolver.ResolveAsync(dbContext, decision.TraineeUserId, cancellationToken);
        if (scope is not null && TraineeScopeResolver.IsAdministeredBy(scope, principal))
        {
            return;
        }

        throw new UnauthorizedAccessException(DecisionNotRevocableByCaller);
    }

    /// <summary>
    /// Whether the caller is the chair who issued a decision, as the decision itself records it
    /// (<see cref="EntrustmentDecision.IssuedByChairUserId" />), never as the issuing panel's members now read. (T183)
    /// </summary>
    public static bool IsIssuer(ClaimsPrincipal principal, string issuedByChairUserId)
    {
        ArgumentNullException.ThrowIfNull(principal);

        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return !string.IsNullOrEmpty(userId) &&
               string.Equals(userId, issuedByChairUserId, StringComparison.Ordinal);
    }
}
