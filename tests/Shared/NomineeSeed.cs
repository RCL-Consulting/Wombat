using Microsoft.AspNetCore.Identity;
using Wombat.Infrastructure.Identity;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Tests.Shared;

/// <summary>
/// Seeds the Identity rows a nominee field is judged against (T102): a user with an institution, their roles, and
/// optionally a lockout. Linked into every test project that drives <c>ActivityService</c>.
/// </summary>
/// <remarks>
/// A fixture whose activities name an assessor must seed that assessor as ELIGIBLE, so its assertions keep proving what
/// they proved before T102. Never make a nominee pass by loosening an assertion.
/// Roles carry <c>NormalizedName</c>, because that is what the directory matches on, as Identity does on PostgreSQL.
/// Adds only; the caller saves.
/// </remarks>
public static class NomineeSeed
{
    public static WombatIdentityUser AddUser(
        ApplicationDbContext db,
        string userId,
        int? institutionId,
        params string[] roles)
        => AddUser(db, userId, institutionId, lockoutEnd: null, roles);

    public static WombatIdentityUser AddUser(
        ApplicationDbContext db,
        string userId,
        int? institutionId,
        DateTimeOffset? lockoutEnd,
        params string[] roles)
    {
        var user = new WombatIdentityUser
        {
            Id = userId,
            UserName = $"{userId}@test.local",
            NormalizedUserName = $"{userId}@TEST.LOCAL".ToUpperInvariant(),
            Email = $"{userId}@test.local",
            NormalizedEmail = $"{userId}@TEST.LOCAL".ToUpperInvariant(),
            FirstName = "First",
            LastName = userId,
            InstitutionId = institutionId,
            LockoutEnd = lockoutEnd,
            LockoutEnabled = true
        };
        db.Users.Add(user);

        foreach (var role in roles)
        {
            db.UserRoles.Add(new IdentityUserRole<string> { UserId = userId, RoleId = EnsureRole(db, role).Id });
        }

        return user;
    }

    /// <summary>Gives an already-seeded user another role.</summary>
    public static void AddRole(ApplicationDbContext db, string userId, string role)
        => db.UserRoles.Add(new IdentityUserRole<string> { UserId = userId, RoleId = EnsureRole(db, role).Id });

    private static IdentityRole EnsureRole(ApplicationDbContext db, string role)
    {
        var id = $"role-{role.ToLowerInvariant()}";
        var existing = db.Roles.Local.FirstOrDefault(entity => entity.Id == id) ?? db.Roles.Find(id);
        if (existing is not null)
        {
            return existing;
        }

        var created = new IdentityRole(role) { Id = id, NormalizedName = role.ToUpperInvariant() };
        db.Roles.Add(created);
        return created;
    }
}
