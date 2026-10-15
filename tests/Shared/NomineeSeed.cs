using Microsoft.AspNetCore.Identity;
using Wombat.Domain.Identity;
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

    /// <summary>
    /// Gives a subject the active trainee profile D55 asks of anyone an activity is filed about: their programme is
    /// running. Before D55 a fixture could file for a subject with no profile, whose stamp came from their Identity row.
    /// </summary>
    /// <remarks>
    /// The default curriculum id is one no fixture seeds, so the profile changes nothing but the filing rule: the stamp is
    /// the institution alone, as the Identity row of a user with no speciality scopes gave; no curriculum item is credited;
    /// and the default start bounds no encounter date a fixture uses. Pass a curriculum when the fixture's scope or credit
    /// needs one.
    /// </remarks>
    public static TraineeProfile AddCurrentProgramme(
        ApplicationDbContext db,
        string userId,
        int institutionId,
        int curriculumId = NoCurriculumId,
        DateOnly? programmeStartDate = null)
    {
        var start = programmeStartDate ?? new DateOnly(2000, 1, 1);
        var profile = new TraineeProfile
        {
            UserId = userId,
            InstitutionId = institutionId,
            CurriculumId = curriculumId,
            ProgrammeStartDate = start,
            AdmittedOn = start,
            ExpectedCompletionDate = start.AddYears(40),
            IsActive = true
        };
        db.TraineeProfiles.Add(profile);
        return profile;
    }

    /// <summary>The curriculum id <see cref="AddCurrentProgramme" /> uses when none is given: one no fixture seeds.</summary>
    public const int NoCurriculumId = -1;

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
