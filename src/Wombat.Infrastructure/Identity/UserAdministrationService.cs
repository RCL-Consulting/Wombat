using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Infrastructure.Identity;

public sealed class UserAdministrationService : IUserAdministrationService
{
    private readonly UserManager<WombatIdentityUser> _userManager;
    private readonly ApplicationDbContext _dbContext;

    public UserAdministrationService(UserManager<WombatIdentityUser> userManager, ApplicationDbContext dbContext)
    {
        _userManager = userManager;
        _dbContext = dbContext;
    }

    public async Task<UserIdentityDetails?> GetByIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await LoadUsersQuery()
            .SingleOrDefaultAsync(entity => entity.Id == userId, cancellationToken);

        if (user is null)
        {
            return null;
        }

        var roles = await _userManager.GetRolesAsync(user);
        return Map(user, roles.ToArray());
    }

    public async Task<IReadOnlyList<UserIdentityDetails>> ListUsersInRoleAsync(string role, CancellationToken cancellationToken = default)
    {
        var usersInRole = await _userManager.GetUsersInRoleAsync(role);
        var userIds = usersInRole.Select(user => user.Id).ToArray();

        if (userIds.Length == 0)
        {
            return [];
        }

        var users = await LoadUsersQuery()
            .Where(entity => userIds.Contains(entity.Id))
            .OrderBy(entity => entity.LastName)
            .ThenBy(entity => entity.FirstName)
            .ToListAsync(cancellationToken);

        var usersById = users.ToDictionary(entity => entity.Id, entity => entity);

        return usersInRole
            .Where(user => usersById.ContainsKey(user.Id))
            .Select(user => Map(usersById[user.Id], [role]))
            .OrderBy(user => user.LastName)
            .ThenBy(user => user.FirstName)
            .ToArray();
    }

    public async Task<IReadOnlyList<UserIdentityDetails>> ListAllUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await LoadUsersQuery()
            .OrderBy(entity => entity.LastName)
            .ThenBy(entity => entity.FirstName)
            .ToListAsync(cancellationToken);

        var results = new List<UserIdentityDetails>(users.Count);
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            results.Add(Map(user, roles.ToArray()));
        }
        return results;
    }

    public async Task UpdateNamesAsync(string userId, string firstName, string lastName, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users.SingleOrDefaultAsync(entity => entity.Id == userId, cancellationToken)
            ?? throw new InvalidOperationException("The user could not be found.");

        user.FirstName = firstName.Trim();
        user.LastName = lastName.Trim();

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(error => error.Description)));
        }
    }

    public async Task UpdateScopeAsync(
        string userId,
        int institutionId,
        IReadOnlyCollection<int> specialityIds,
        IReadOnlyCollection<int> subSpecialityIds,
        CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users
            .Include(entity => entity.SpecialityScopes)
            .Include(entity => entity.SubSpecialityScopes)
            .SingleOrDefaultAsync(entity => entity.Id == userId, cancellationToken)
            ?? throw new InvalidOperationException("The user could not be found.");

        user.InstitutionId = institutionId;

        var desiredSpecialityIds = specialityIds.Distinct().OrderBy(id => id).ToArray();
        var desiredSubSpecialityIds = subSpecialityIds.Distinct().OrderBy(id => id).ToArray();

        SyncScopes(
            user.SpecialityScopes,
            desiredSpecialityIds,
            existing => existing.SpecialityId,
            value => new WombatIdentityUserSpecialityScope
            {
                UserId = userId,
                SpecialityId = value
            });

        SyncScopes(
            user.SubSpecialityScopes,
            desiredSubSpecialityIds,
            existing => existing.SubSpecialityId,
            value => new WombatIdentityUserSubSpecialityScope
            {
                UserId = userId,
                SubSpecialityId = value
            });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task PromotePendingTraineeAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new InvalidOperationException("The user could not be found.");

        if (!await _userManager.IsInRoleAsync(user, WombatRoles.PendingTrainee))
        {
            throw new InvalidOperationException("Only users in the PendingTrainee role can be admitted.");
        }

        var removeResult = await _userManager.RemoveFromRoleAsync(user, WombatRoles.PendingTrainee);
        if (!removeResult.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", removeResult.Errors.Select(error => error.Description)));
        }

        var addResult = await _userManager.AddToRoleAsync(user, WombatRoles.Trainee);
        if (!addResult.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", addResult.Errors.Select(error => error.Description)));
        }

        await _userManager.UpdateSecurityStampAsync(user);
    }

    public async Task AddRoleAsync(string userId, string role, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new InvalidOperationException("The user could not be found.");

        if (await _userManager.IsInRoleAsync(user, role))
        {
            return;
        }

        var result = await _userManager.AddToRoleAsync(user, role);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(error => error.Description)));
        }

        await _userManager.UpdateSecurityStampAsync(user);
    }

    public async Task RemoveRoleAsync(string userId, string role, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new InvalidOperationException("The user could not be found.");

        if (!await _userManager.IsInRoleAsync(user, role))
        {
            return;
        }

        var result = await _userManager.RemoveFromRoleAsync(user, role);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(error => error.Description)));
        }

        await _userManager.UpdateSecurityStampAsync(user);
    }

    public async Task ResetPasswordAsync(string userId, string newPassword, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new InvalidOperationException("The user could not be found.");

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(error => error.Description)));
        }

        await _userManager.UpdateSecurityStampAsync(user);
    }

    public async Task SetLockoutAsync(string userId, bool locked, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new InvalidOperationException("The user could not be found.");

        // Lockout uses DateTimeOffset.MaxValue as the "indefinite" sentinel. Clearing the lockout
        // requires setting LockoutEnd to null (UserManager.SetLockoutEndDateAsync only writes the
        // column; we also need lockout enabled to be true to honour it).
        if (locked)
        {
            await _userManager.SetLockoutEnabledAsync(user, true);
        }

        var lockoutEnd = locked ? DateTimeOffset.MaxValue : (DateTimeOffset?)null;
        var result = await _userManager.SetLockoutEndDateAsync(user, lockoutEnd);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(error => error.Description)));
        }

        await _userManager.UpdateSecurityStampAsync(user);
    }

    private IQueryable<WombatIdentityUser> LoadUsersQuery()
        => _dbContext.Users
            .AsNoTracking()
            .Include(entity => entity.SpecialityScopes)
            .Include(entity => entity.SubSpecialityScopes);

    private static UserIdentityDetails Map(WombatIdentityUser user, IReadOnlyCollection<string> roles)
        => new(
            user.Id,
            user.Email ?? string.Empty,
            user.FirstName,
            user.LastName,
            user.InstitutionId,
            user.SpecialityScopes.Select(scope => scope.SpecialityId).Distinct().ToArray(),
            user.SubSpecialityScopes.Select(scope => scope.SubSpecialityId).Distinct().ToArray(),
            roles,
            IsLockedOut: user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow);

    private static void SyncScopes<TScope>(
        ICollection<TScope> currentScopes,
        IReadOnlyCollection<int> desiredValues,
        Func<TScope, int> getValue,
        Func<int, TScope> createScope)
        where TScope : class
    {
        var toRemove = currentScopes
            .Where(existing => !desiredValues.Contains(getValue(existing)))
            .ToArray();

        foreach (var scope in toRemove)
        {
            currentScopes.Remove(scope);
        }

        var existingValues = currentScopes.Select(getValue).ToHashSet();
        foreach (var value in desiredValues)
        {
            if (!existingValues.Contains(value))
            {
                currentScopes.Add(createScope(value));
            }
        }
    }
}
