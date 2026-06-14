namespace Wombat.Application.Features.Users;

public sealed record UserSummaryDto(
    string UserId,
    string Email,
    string FirstName,
    string LastName,
    int? InstitutionId,
    string? InstitutionName,
    IReadOnlyCollection<string> Roles,
    bool IsLockedOut);

public sealed record UserDetailDto(
    string UserId,
    string Email,
    string FirstName,
    string LastName,
    int? InstitutionId,
    string? InstitutionName,
    IReadOnlyCollection<int> SpecialityIds,
    IReadOnlyCollection<int> SubSpecialityIds,
    IReadOnlyCollection<string> Roles,
    bool IsLockedOut,
    IReadOnlyList<UserPendingInvitationDto> PendingInvitations);

public sealed record UserPendingInvitationDto(
    int InvitationId,
    string TargetRole,
    int? InstitutionId,
    // Institution name for institution-scoped invitations, or the College name for a CollegeAdmin
    // invitation (which has no institution). (T093)
    string ScopeName,
    int? SpecialityId,
    string? SpecialityName,
    DateTime IssuedOn,
    DateOnly ExpiresOn);
