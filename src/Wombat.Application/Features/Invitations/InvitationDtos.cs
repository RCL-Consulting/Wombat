using Wombat.Domain.Invitations;

namespace Wombat.Application.Features.Invitations;

/// <summary>What Register says about the invitation its link holds (T373, flow 11, Q6; build-contracts § 2).</summary>
/// <param name="Email">The invited address, as stored.</param>
/// <param name="TargetRole">The role's key; the page writes its label (<c>InvitationScopeWords.RoleAndScope</c>).</param>
/// <param name="InstitutionName">The institution invited to; null for a College admin's invitation.</param>
/// <param name="CollegeName">The College, for a College admin's invitation; otherwise null.</param>
/// <param name="SpecialityName">The speciality the invitation is scoped to, if any.</param>
/// <param name="SubSpecialityName">Its sub-speciality, if any.</param>
/// <param name="ExpiresOn">The link's last day, on the South African calendar (F7): "its link works until ‹date›".</param>
public sealed record InvitationPreviewDto(
    string Email,
    string TargetRole,
    string? InstitutionName,
    string? CollegeName,
    string? SpecialityName,
    string? SubSpecialityName,
    DateOnly ExpiresOn);

/// <param name="IssuedOn">When the current link was issued: the issue, or the last resend (T283).</param>
/// <param name="Delivery">What became of the current link's mail (T283).</param>
/// <param name="DeliveryFailures">How many of the invitation's mails were given up on, across its links (T283).</param>
/// <param name="SuggestCheckingAddress">
/// The current link was not delivered, and enough mails have failed that the address may be what is wrong
/// (<c>Invitation.SuggestsCheckingAddress</c>, T283).
/// </param>
public sealed record ActiveInvitationDto(
    int Id,
    string Email,
    string TargetRole,
    int? InstitutionId,
    string? InstitutionName,
    int? CollegeId,
    string? CollegeName,
    int? SpecialityId,
    string? SpecialityName,
    int? SubSpecialityId,
    string? SubSpecialityName,
    DateTime IssuedOn,
    DateOnly ExpiresOn,
    InvitationDelivery Delivery,
    int DeliveryFailures,
    bool SuggestCheckingAddress);

/// <summary>
/// An expired invitation that was never used (T373, flow 11, Q9): the active row's facts without its delivery, which no
/// longer matters. Listed under "Expired, not used" for 30 days after its last day, with Invite again.
/// </summary>
/// <param name="IssuedOn">When its last link was issued.</param>
/// <param name="ExpiresOn">Its last day, on the South African calendar.</param>
public sealed record ExpiredInvitationDto(
    int Id,
    string Email,
    string TargetRole,
    int? InstitutionId,
    string? InstitutionName,
    int? CollegeId,
    string? CollegeName,
    int? SpecialityId,
    string? SpecialityName,
    int? SubSpecialityId,
    string? SubSpecialityName,
    DateTime IssuedOn,
    DateOnly ExpiresOn);

/// <summary>What Invite a person is filled from (T373, flow 11, N19, N20).</summary>
public enum InvitationPrefillSource
{
    /// <summary>A revoked invitation: its role and scope, never its address, which was the wrong one (N19).</summary>
    Revoked,

    /// <summary>An invitation that passed its last day unused: its address, role and scope (N20, Q9).</summary>
    Expired
}

/// <summary>
/// Invite a person filled from an earlier invitation in the caller's scope (T373, N19, N20). <paramref name="Email" /> is the
/// earlier invitation's address, which the page fills only from an expired one and names in its notice either way.
/// </summary>
public sealed record InvitationPrefillDto(
    int InvitationId,
    InvitationPrefillSource Source,
    string Email,
    string TargetRole,
    int? InstitutionId,
    int? CollegeId,
    int? SpecialityId,
    int? SubSpecialityId,
    DateOnly ExpiresOn);

/// <param name="Token">The new link's token: shown once, never stored (T283).</param>
public sealed record IssuedInvitationResult(int InvitationId, string Token)
{
    /// <summary>
    /// The link's last day on the South African calendar (T373, F7): the OneTimeLink's "It works until ‹last day›".
    /// </summary>
    public DateOnly ExpiresOn { get; init; }
}

public sealed record AcceptedInvitationResult(string UserId, string AssignedRole);
