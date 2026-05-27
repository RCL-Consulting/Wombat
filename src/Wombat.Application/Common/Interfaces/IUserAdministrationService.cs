namespace Wombat.Application.Common.Interfaces;

public interface IUserAdministrationService
{
    Task<UserIdentityDetails?> GetByIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserIdentityDetails>> ListUsersInRoleAsync(string role, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserIdentityDetails>> ListAllUsersAsync(CancellationToken cancellationToken = default);
    Task UpdateNamesAsync(string userId, string firstName, string lastName, CancellationToken cancellationToken = default);
    Task UpdateScopeAsync(
        string userId,
        int institutionId,
        IReadOnlyCollection<int> specialityIds,
        IReadOnlyCollection<int> subSpecialityIds,
        CancellationToken cancellationToken = default);
    Task PromotePendingTraineeAsync(string userId, CancellationToken cancellationToken = default);
    Task AddRoleAsync(string userId, string role, CancellationToken cancellationToken = default);
    Task RemoveRoleAsync(string userId, string role, CancellationToken cancellationToken = default);
    Task ResetPasswordAsync(string userId, string newPassword, CancellationToken cancellationToken = default);
    Task SetLockoutAsync(string userId, bool locked, CancellationToken cancellationToken = default);
}

public sealed record UserIdentityDetails(
    string UserId,
    string Email,
    string FirstName,
    string LastName,
    int? InstitutionId,
    IReadOnlyCollection<int> SpecialityIds,
    IReadOnlyCollection<int> SubSpecialityIds,
    IReadOnlyCollection<string> Roles,
    bool IsLockedOut = false);
