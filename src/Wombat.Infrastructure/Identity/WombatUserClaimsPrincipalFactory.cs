using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Infrastructure.Identity;

public sealed class WombatUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<WombatIdentityUser, IdentityRole>
{
    private readonly ApplicationDbContext _dbContext;

    public WombatUserClaimsPrincipalFactory(
        UserManager<WombatIdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IOptions<IdentityOptions> optionsAccessor,
        ApplicationDbContext dbContext)
        : base(userManager, roleManager, optionsAccessor)
    {
        _dbContext = dbContext;
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(WombatIdentityUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);
        var scopeUser = await _dbContext.Users
            .AsNoTracking()
            .Include(entity => entity.SpecialityScopes)
            .Include(entity => entity.SubSpecialityScopes)
            .SingleAsync(entity => entity.Id == user.Id);

        // What the shell shows of the person, so it reads no database (T335, flow 01).
        if (DisplayNameOf(scopeUser) is { Length: > 0 } displayName)
        {
            identity.AddClaim(new Claim(WombatClaims.DisplayName, displayName));
        }

        // The acting role the person last chose, as stored (D1): the shell resolves it from claims, and a stored role
        // they no longer hold is ignored there, not here.
        if (!string.IsNullOrWhiteSpace(scopeUser.ActingRole))
        {
            identity.AddClaim(new Claim(WombatClaims.ActingRole, scopeUser.ActingRole));
        }

        if (scopeUser.InstitutionId.HasValue)
        {
            identity.AddClaim(new Claim(WombatClaims.InstitutionId, scopeUser.InstitutionId.Value.ToString()));
        }

        if (scopeUser.CollegeId.HasValue)
        {
            identity.AddClaim(new Claim(WombatClaims.CollegeId, scopeUser.CollegeId.Value.ToString()));
        }

        foreach (var specialityId in scopeUser.SpecialityScopes.Select(scope => scope.SpecialityId).Distinct())
        {
            identity.AddClaim(new Claim(WombatClaims.SpecialityId, specialityId.ToString()));
        }

        foreach (var subSpecialityId in scopeUser.SubSpecialityScopes.Select(scope => scope.SubSpecialityId).Distinct())
        {
            identity.AddClaim(new Claim(WombatClaims.SubSpecialityId, subSpecialityId.ToString()));
        }

        // T252: a graduate keeps their own record after completion takes the Trainee role away. Any profile, current or
        // ended: the reader shows the current one while there is one, and the one they ended on after.
        if (await _dbContext.TraineeProfiles.AsNoTracking().AnyAsync(profile => profile.UserId == user.Id))
        {
            identity.AddClaim(new Claim(WombatClaims.TraineeRecord, "true"));
        }

        return identity;
    }

    /// <summary>
    /// "FirstName LastName", each part trimmed and a blank one left out; the email when both are blank, and the user name
    /// when there is no email either. Empty only for an account with none of the three.
    /// </summary>
    public static string DisplayNameOf(WombatIdentityUser user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var name = string.Join(" ", new[] { user.FirstName, user.LastName }
            .Where(part => !string.IsNullOrWhiteSpace(part))
            .Select(part => part.Trim()));

        if (name.Length > 0)
        {
            return name;
        }

        return new[] { user.Email, user.UserName }.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim()
            ?? string.Empty;
    }
}
