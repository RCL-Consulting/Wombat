using Wombat.Domain.Invitations;

namespace Wombat.Application.Features.Invitations;

public sealed record InvitationPreviewDto(string Email, string TargetRole);

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

public sealed record IssuedInvitationResult(int InvitationId, string Token);

public sealed record AcceptedInvitationResult(string UserId, string AssignedRole);
