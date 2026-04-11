using Microsoft.AspNetCore.Identity;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Infrastructure.Identity;

public sealed class InvitedUserProvisioner : IInvitedUserProvisioner
{
    private readonly UserManager<WombatIdentityUser> _userManager;
    private readonly ApplicationDbContext _dbContext;

    public InvitedUserProvisioner(UserManager<WombatIdentityUser> userManager, ApplicationDbContext dbContext)
    {
        _userManager = userManager;
        _dbContext = dbContext;
    }

    public async Task<ProvisionedInvitationUser> ProvisionAsync(
        string email,
        string password,
        string firstName,
        string lastName,
        string targetRole,
        int institutionId,
        int? specialityId,
        int? subSpecialityId,
        CancellationToken cancellationToken = default)
    {
        var existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser is not null)
        {
            throw new InvalidOperationException("A user with this email address already exists.");
        }

        var assignedRole = string.Equals(targetRole, WombatRoles.Trainee, StringComparison.Ordinal)
            ? WombatRoles.PendingTrainee
            : targetRole;

        var user = new WombatIdentityUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            InstitutionId = institutionId
        };

        var createResult = await _userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", createResult.Errors.Select(error => error.Description)));
        }

        var roleResult = await _userManager.AddToRoleAsync(user, assignedRole);
        if (!roleResult.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", roleResult.Errors.Select(error => error.Description)));
        }

        if (specialityId.HasValue)
        {
            _dbContext.UserSpecialityScopes.Add(new WombatIdentityUserSpecialityScope
            {
                UserId = user.Id,
                SpecialityId = specialityId.Value
            });
        }

        if (subSpecialityId.HasValue)
        {
            _dbContext.UserSubSpecialityScopes.Add(new WombatIdentityUserSubSpecialityScope
            {
                UserId = user.Id,
                SubSpecialityId = subSpecialityId.Value
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return new ProvisionedInvitationUser(user.Id, assignedRole);
    }
}
