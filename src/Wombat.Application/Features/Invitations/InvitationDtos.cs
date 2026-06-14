namespace Wombat.Application.Features.Invitations;

public sealed record InvitationPreviewDto(string Email, string TargetRole);

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
    DateOnly ExpiresOn);

public sealed record IssuedInvitationResult(int InvitationId, string Token);

public sealed record AcceptedInvitationResult(string UserId, string AssignedRole);
