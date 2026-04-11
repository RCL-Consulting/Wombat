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

        if (scopeUser.InstitutionId.HasValue)
        {
            identity.AddClaim(new Claim(WombatClaims.InstitutionId, scopeUser.InstitutionId.Value.ToString()));
        }

        foreach (var specialityId in scopeUser.SpecialityScopes.Select(scope => scope.SpecialityId).Distinct())
        {
            identity.AddClaim(new Claim(WombatClaims.SpecialityId, specialityId.ToString()));
        }

        foreach (var subSpecialityId in scopeUser.SubSpecialityScopes.Select(scope => scope.SubSpecialityId).Distinct())
        {
            identity.AddClaim(new Claim(WombatClaims.SubSpecialityId, subSpecialityId.ToString()));
        }

        return identity;
    }
}
