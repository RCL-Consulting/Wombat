using Wombat.Application.Features.Dashboards.Oversight;
using Wombat.Application.Features.Programme.Waiting;

namespace Wombat.Application.Features.Dashboards.Coordinator;

/// <summary>
/// The Coordinator's Home (T358, flow 06; R2-Home k1–k6): Waiting for assessors, Nothing filed in 30 days and Invitations
/// nearing expiry, read as Coordinator, in the institution's scope.
/// </summary>
/// <remarks>
/// Until T358 its first card was "Stalled requests": ten rows untouched for the stall days, any activity awaiting a
/// reviewer, a role-held review included, each dated "d MMM". It is now Waiting for assessors' first five (E3: only a
/// request whose next move names a person), and the stall setting is gone (review 11).
/// </remarks>
/// <param name="Waiting">Waiting for assessors' first five and its counts; null when the role gives no scope.</param>
/// <param name="NothingFiled">Programme trainees filtered Nothing filed in 30 days (E5): its first five and its count.</param>
/// <param name="ExpiringInvitations">The institution's unused invitations expiring in the next three days.</param>
public sealed record CoordinatorDashboardSummaryDto(
    WaitingForAssessorsDto? Waiting,
    RegistrarsCardDto NothingFiled,
    IReadOnlyList<ExpiringInvitationItem> ExpiringInvitations);

/// <summary>An unused invitation expiring soon (k4: "expiring@kgk.wombat.local · Trainee", "expires 2026-10-07").</summary>
/// <param name="TargetRole">The role's key, as stored.</param>
/// <param name="ExpiresOn">Its last day.</param>
public sealed record ExpiringInvitationItem(
    int InvitationId,
    string Email,
    string TargetRole,
    DateOnly ExpiresOn)
{
    /// <summary>The role by its label, never its key (T358, Q10): "Trainee", "Committee member".</summary>
    public string TargetRoleLabel { get; init; } = TargetRole;
}
