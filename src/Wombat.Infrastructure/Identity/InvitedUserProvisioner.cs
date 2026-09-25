using Microsoft.AspNetCore.Identity;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Invitations;
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

    /// <summary>
    /// The account created here takes the address as its email and as its user name, so an account holds the address when
    /// it has it as either, and Identity must accept the address as a user name, whose characters its rules limit. The user
    /// name is judged by Identity's own user validators, the ones <c>CreateAsync</c> runs, on the account that would be
    /// created, which is never added to the context. (T285)
    /// </summary>
    public async Task<InvitedAddressStatus> GetAddressStatusAsync(string email, CancellationToken cancellationToken = default)
    {
        if (await _userManager.FindByEmailAsync(email) is not null)
        {
            return InvitedAddressStatus.Taken;
        }

        var prospective = new WombatIdentityUser { UserName = email, Email = email };
        var codes = new List<string>();
        foreach (var validator in _userManager.UserValidators)
        {
            var result = await validator.ValidateAsync(_userManager, prospective);
            codes.AddRange(result.Errors.Select(error => error.Code));
        }

        return StatusOf(codes);
    }

    public async Task<ProvisionedInvitationUser> ProvisionAsync(
        string email,
        string password,
        string firstName,
        string lastName,
        string targetRole,
        int? institutionId,
        int? collegeId,
        int? specialityId,
        int? subSpecialityId,
        CancellationToken cancellationToken = default)
    {
        // The test the invitation's preview asks, so a submit is refused as the page was (T285).
        if (InvitationRefusals.For(await GetAddressStatusAsync(email, cancellationToken)) is { } addressRefusal)
        {
            throw new InvitationRefusedException(addressRefusal);
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
            InstitutionId = institutionId,
            CollegeId = collegeId
        };

        // A refusal travels as Identity's codes, never its descriptions: the register page describes each code in words of
        // its own, and a description can quote the address ("Username '...' is already taken."), which the audit row this
        // message becomes would keep. A refusal of the address itself, which the check above makes first unless another
        // registration took the address in between, is refused as that check refuses it (T285).
        var createResult = await _userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
        {
            var codes = createResult.Errors.Select(error => error.Code).ToList();
            if (InvitationRefusals.For(StatusOf(codes)) is { } refusal)
            {
                throw new InvitationRefusedException(refusal);
            }

            throw new InvitationRefusedException(
                InvitationRefusal.AccountNotCreated,
                $"Identity refused the new account ({string.Join(", ", codes)}).",
                codes);
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

    /// <summary>What Identity's refusal of an account says about its address; <see cref="InvitedAddressStatus.Available" /> when nothing.</summary>
    private static InvitedAddressStatus StatusOf(IReadOnlyCollection<string> codes)
    {
        if (codes.Any(code => code is nameof(IdentityErrorDescriber.DuplicateUserName)
                or nameof(IdentityErrorDescriber.DuplicateEmail)))
        {
            return InvitedAddressStatus.Taken;
        }

        return codes.Any(code => code is nameof(IdentityErrorDescriber.InvalidUserName)
                or nameof(IdentityErrorDescriber.InvalidEmail))
            ? InvitedAddressStatus.NotAccepted
            : InvitedAddressStatus.Available;
    }
}
