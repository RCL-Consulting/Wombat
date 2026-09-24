using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Activities.Services;
using Wombat.Infrastructure.Identity;

namespace Wombat.Infrastructure.Activities;

/// <summary>
/// The people a nominee field may name on an activity: the one query behind both the picker and the server's check
/// (T102), so the form can never offer someone the server refuses, or the reverse.
/// </summary>
/// <remarks>
/// <para>
/// Eligible means: holds every required role; belongs to the activity's institution (the subject's, which is also
/// what <c>scope:</c> rules read, never the caller's); is not deactivated (<see cref="UserDeactivation" />); and is not
/// the subject. A null institution admits nobody. There is no Administrator bypass: the answer depends on the
/// activity, not on who is asking.
/// </para>
/// <para>
/// Everything is read from the database, never from claims. A caller's claims are frozen for the life of a Blazor
/// circuit, and a nominee's are not available at all. Roles are matched on <c>NormalizedName</c>, as Identity does,
/// so the answer is the same on PostgreSQL and in the in-memory test store.
/// </para>
/// </remarks>
internal static class NomineeDirectory
{
    public static IQueryable<WombatIdentityUser> Eligible(
        IApplicationDbContext dbContext,
        int? institutionId,
        IReadOnlyCollection<string> requiredRoles,
        string subjectUserId)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(requiredRoles);

        if (institutionId is null || requiredRoles.Count == 0)
        {
            return dbContext.Set<WombatIdentityUser>().Where(_ => false);
        }

        var users = dbContext.Set<WombatIdentityUser>()
            .AsNoTracking()
            .Where(user => user.InstitutionId == institutionId.Value &&
                           user.Id != subjectUserId &&
                           (user.LockoutEnd == null || user.LockoutEnd < UserDeactivation.Threshold));

        var userRoles = dbContext.Set<IdentityUserRole<string>>();
        var roles = dbContext.Set<IdentityRole>();
        foreach (var normalizedRole in requiredRoles.Select(role => role.ToUpperInvariant()).Distinct(StringComparer.Ordinal))
        {
            users = users.Where(user => userRoles.Any(userRole =>
                userRole.UserId == user.Id &&
                roles.Any(role => role.Id == userRole.RoleId && role.NormalizedName == normalizedRole)));
        }

        return users;
    }

    public static async Task<IReadOnlyList<ActivityCatalogueOption>> ListAsync(
        IApplicationDbContext dbContext,
        int? institutionId,
        IReadOnlyCollection<string> requiredRoles,
        string subjectUserId,
        CancellationToken cancellationToken)
    {
        var users = await Eligible(dbContext, institutionId, requiredRoles, subjectUserId)
            .Select(user => new { user.Id, user.FirstName, user.LastName, user.Email })
            .ToListAsync(cancellationToken);

        return users
            .OrderBy(user => user.LastName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(user => user.FirstName, StringComparer.OrdinalIgnoreCase)
            .Select(user => new ActivityCatalogueOption(user.Id, FormatLabel(user.FirstName, user.LastName, user.Email)))
            .ToArray();
    }

    /// <summary>
    /// Whether <paramref name="userId" /> is eligible. Exact and ordinal, as the actor grammar matches: a padded or
    /// re-cased id names nobody the grammar would ever match, so it is not eligible either.
    /// </summary>
    public static Task<bool> IsEligibleAsync(
        IApplicationDbContext dbContext,
        string userId,
        int? institutionId,
        IReadOnlyCollection<string> requiredRoles,
        string subjectUserId,
        CancellationToken cancellationToken)
        => Eligible(dbContext, institutionId, requiredRoles, subjectUserId)
            .AnyAsync(user => user.Id == userId, cancellationToken);

    public static string FormatLabel(string? firstName, string? lastName, string? email)
    {
        var name = string.Join(" ", new[] { firstName, lastName }.Where(part => !string.IsNullOrWhiteSpace(part)));
        if (string.IsNullOrWhiteSpace(name))
        {
            return email ?? string.Empty;
        }

        return string.IsNullOrWhiteSpace(email) ? name : $"{name} ({email})";
    }
}
