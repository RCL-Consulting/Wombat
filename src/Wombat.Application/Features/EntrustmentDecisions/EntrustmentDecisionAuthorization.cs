using System.Security.Claims;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Domain.EntrustmentDecisions;

namespace Wombat.Application.Features.EntrustmentDecisions;

internal static class EntrustmentDecisionAuthorization
{
    /// <summary>
    /// The one refusal for a decision id the caller may not revoke, whether or not it exists. (T183)
    /// </summary>
    /// <remarks>
    /// The admin page prints a command's refusal. "Could not be found" for a missing id beside a scope refusal for a
    /// real one would let an administrator walk the ids and learn which decisions other institutions hold, so both are
    /// this. A caller who may revoke every decision (<see cref="MayRevokeEveryDecision" />) is told plainly, because for
    /// them "not found" says only that.
    /// </remarks>
    internal const string DecisionNotRevocableByCaller =
        "The entrustment decision could not be found among the decisions you may revoke.";

    /// <summary>
    /// A global Administrator who is not also a trainee: the one caller for whom every decision is revocable, so the
    /// one caller told plainly that an id names nothing. (T183, T185)
    /// </summary>
    /// <remarks>
    /// An Administrator who also holds Trainee revokes nothing but what they issued
    /// (<see cref="TraineeScopeResolver.ActsAsTrainee" />), so a plain "not found" beside the scope refusal would tell
    /// them which ids exist.
    /// </remarks>
    public static bool MayRevokeEveryDecision(ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);
        return principal.IsAdministrator() && !TraineeScopeResolver.ActsAsTrainee(principal);
    }

    public static string GetRequiredUserId(ClaimsPrincipal principal)
        => principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("The current user identifier is missing.");

    /// <summary>
    /// Refuses to revoke a decision unless the caller is the chair who issued it, a global Administrator, or an
    /// InstitutionalAdmin, SpecialityAdmin or SubSpecialityAdmin who administers the decision's trainee
    /// (<see cref="TraineeScopeResolver.IsAdministeredBy" />); and, but for the issuing chair, never a caller who holds
    /// Trainee. (T183, T185)
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
    /// The trainee rung (<see cref="TraineeScopeResolver.ActsAsTrainee" />) stands before every admin arm, the
    /// Administrator's included, as it does on the certificate (<see cref="TraineeScopeResolver.MayReadAsync" />):
    /// someone who holds Trainee beside an admin role administers no trainee's decisions, their own included. Without it
    /// the admin list offered such a caller Revoke on decisions whose certificate it refused them (T185 review). It comes
    /// after the issuing chair's arm, which the certificate also reads first, so the two stay one answer.
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

        if (IsIssuer(principal, decision.IssuedByChairUserId))
        {
            return;
        }

        if (TraineeScopeResolver.ActsAsTrainee(principal))
        {
            throw new UnauthorizedAccessException(DecisionNotRevocableByCaller);
        }

        if (principal.IsAdministrator())
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
