namespace Wombat.Application.Features.Accounts;

public sealed record UserProfileDto(
    string UserId,
    string Email,
    string FirstName,
    string LastName,
    IReadOnlyCollection<string> Roles);
