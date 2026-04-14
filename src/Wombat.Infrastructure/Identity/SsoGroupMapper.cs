using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Infrastructure.Identity;

/// <summary>
/// Synchronises Wombat roles from IdP group claims on each SSO login.
/// Adds roles for matching groups, removes SSO-assigned roles that no longer match,
/// and never touches manually-assigned roles.
/// </summary>
public sealed class SsoGroupMapper
{
    private readonly UserManager<WombatIdentityUser> _userManager;
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<SsoGroupMapper> _logger;

    public SsoGroupMapper(
        UserManager<WombatIdentityUser> userManager,
        ApplicationDbContext dbContext,
        ILogger<SsoGroupMapper> logger)
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Apply group-to-role mappings for a user after SSO login.
    /// Returns the list of Wombat roles the user currently holds after sync.
    /// </summary>
    public async Task<IReadOnlyList<string>> ApplyAsync(
        WombatIdentityUser user,
        string providerKey,
        IReadOnlyList<string> externalGroupIds,
        CancellationToken cancellationToken = default)
    {
        // Load the mapping rules for this provider
        var mappings = await _dbContext.SsoGroupRoleMappings
            .AsNoTracking()
            .Where(m => m.ProviderKey == providerKey)
            .ToListAsync(cancellationToken);

        // Determine which roles should be active based on current group membership
        var desiredRoles = new HashSet<string>(StringComparer.Ordinal);
        foreach (var mapping in mappings)
        {
            if (externalGroupIds.Contains(mapping.ExternalGroupId))
            {
                // INVARIANT: Administrator cannot be assigned via SSO
                if (string.Equals(mapping.WombatRole, WombatRoles.Administrator, StringComparison.Ordinal))
                {
                    _logger.LogWarning(
                        "SSO group mapping {MappingId} for provider '{ProviderKey}' maps group '{GroupId}' to Administrator — skipping. " +
                        "Administrator role cannot be assigned via SSO.",
                        mapping.Id, providerKey, mapping.ExternalGroupId);
                    continue;
                }

                desiredRoles.Add(mapping.WombatRole);
            }
        }

        // Load existing SSO-assigned role tracking records for this user and provider
        var existingSsoAssignments = await _dbContext.UserRoleAssignments
            .Where(a => a.UserId == user.Id && a.Source == RoleAssignmentSource.Sso && a.ProviderKey == providerKey)
            .ToListAsync(cancellationToken);

        var currentSsoRoles = existingSsoAssignments
            .Select(a => a.Role)
            .ToHashSet(StringComparer.Ordinal);

        // Roles to add: desired but not currently SSO-assigned
        var rolesToAdd = desiredRoles.Except(currentSsoRoles, StringComparer.Ordinal).ToList();

        // Roles to remove: currently SSO-assigned but no longer desired
        var rolesToRemove = currentSsoRoles.Except(desiredRoles, StringComparer.Ordinal).ToList();

        // Add new roles
        foreach (var role in rolesToAdd)
        {
            var currentRoles = await _userManager.GetRolesAsync(user);
            if (!currentRoles.Contains(role))
            {
                var result = await _userManager.AddToRoleAsync(user, role);
                if (!result.Succeeded)
                {
                    _logger.LogWarning("Failed to add role '{Role}' to user '{UserId}': {Errors}",
                        role, user.Id, string.Join("; ", result.Errors.Select(e => e.Description)));
                    continue;
                }
            }

            _dbContext.UserRoleAssignments.Add(new UserRoleAssignment
            {
                UserId = user.Id,
                Role = role,
                Source = RoleAssignmentSource.Sso,
                ProviderKey = providerKey,
                AssignedOn = DateTime.UtcNow
            });
        }

        // Remove roles that were SSO-assigned and no longer match
        foreach (var role in rolesToRemove)
        {
            // Only remove the Identity role if there is no manual assignment keeping it
            var hasManualAssignment = await _dbContext.UserRoleAssignments
                .AnyAsync(a => a.UserId == user.Id && a.Role == role && a.Source == RoleAssignmentSource.Manual,
                    cancellationToken);

            if (!hasManualAssignment)
            {
                await _userManager.RemoveFromRoleAsync(user, role);
            }

            // Remove the SSO tracking record regardless
            var assignment = existingSsoAssignments.First(a => a.Role == role);
            _dbContext.UserRoleAssignments.Remove(assignment);
        }

        // Apply speciality/sub-speciality scopes from matched mappings
        await ApplyScopesAsync(user, providerKey, externalGroupIds, mappings, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return (await _userManager.GetRolesAsync(user)).ToList();
    }

    private async Task ApplyScopesAsync(
        WombatIdentityUser user,
        string providerKey,
        IReadOnlyList<string> externalGroupIds,
        List<SsoGroupRoleMapping> mappings,
        CancellationToken cancellationToken)
    {
        var matchedMappings = mappings.Where(m => externalGroupIds.Contains(m.ExternalGroupId)).ToList();

        // Speciality scopes
        var desiredSpecialityIds = matchedMappings
            .Where(m => m.SpecialityId.HasValue)
            .Select(m => m.SpecialityId!.Value)
            .Distinct()
            .ToList();

        var existingSpecialityIds = await _dbContext.UserSpecialityScopes
            .Where(s => s.UserId == user.Id)
            .Select(s => s.SpecialityId)
            .ToListAsync(cancellationToken);

        foreach (var specialityId in desiredSpecialityIds.Except(existingSpecialityIds))
        {
            _dbContext.UserSpecialityScopes.Add(new WombatIdentityUserSpecialityScope
            {
                UserId = user.Id,
                SpecialityId = specialityId
            });
        }

        // Sub-speciality scopes
        var desiredSubSpecialityIds = matchedMappings
            .Where(m => m.SubSpecialityId.HasValue)
            .Select(m => m.SubSpecialityId!.Value)
            .Distinct()
            .ToList();

        var existingSubSpecialityIds = await _dbContext.UserSubSpecialityScopes
            .Where(s => s.UserId == user.Id)
            .Select(s => s.SubSpecialityId)
            .ToListAsync(cancellationToken);

        foreach (var subSpecialityId in desiredSubSpecialityIds.Except(existingSubSpecialityIds))
        {
            _dbContext.UserSubSpecialityScopes.Add(new WombatIdentityUserSubSpecialityScope
            {
                UserId = user.Id,
                SubSpecialityId = subSpecialityId
            });
        }
    }
}
