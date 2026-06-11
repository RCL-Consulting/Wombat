using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Domain.Invitations;

namespace Wombat.Application.Features.Invitations;

internal static class InvitationRules
{
    private static readonly HashSet<string> AllowedTargetRoles =
    [
        WombatRoles.InstitutionalAdmin,
        WombatRoles.SpecialityAdmin,
        WombatRoles.SubSpecialityAdmin,
        WombatRoles.Coordinator,
        WombatRoles.CommitteeMember,
        WombatRoles.Assessor,
        WombatRoles.Trainee
    ];

    public static bool IsAllowedTargetRole(string targetRole)
        => AllowedTargetRoles.Contains(targetRole);

    public static string? ValidateScope(string targetRole, int institutionId, int? specialityId, int? subSpecialityId)
    {
        if (!IsAllowedTargetRole(targetRole))
        {
            return "The selected target role cannot be issued by invitation.";
        }

        if (institutionId <= 0)
        {
            return "An institution is required.";
        }

        return targetRole switch
        {
            WombatRoles.InstitutionalAdmin when specialityId.HasValue || subSpecialityId.HasValue
                => "Institutional administrators may only be scoped to an institution.",
            WombatRoles.SpecialityAdmin when !specialityId.HasValue
                => "Speciality administrators must be scoped to a speciality.",
            WombatRoles.SpecialityAdmin or WombatRoles.Coordinator or WombatRoles.CommitteeMember when subSpecialityId.HasValue
                => "The selected role may not be scoped to a sub-speciality.",
            WombatRoles.SubSpecialityAdmin or WombatRoles.Assessor or WombatRoles.Trainee when !specialityId.HasValue || !subSpecialityId.HasValue
                => "The selected role requires speciality and sub-speciality scope.",
            _ => null
        };
    }

    public static async Task<string?> ValidateScopeEntitiesAsync(
        IApplicationDbContext dbContext,
        int institutionId,
        int? specialityId,
        int? subSpecialityId,
        CancellationToken cancellationToken)
    {
        if (!await dbContext.Set<Institution>().AnyAsync(entity => entity.Id == institutionId && entity.IsActive, cancellationToken))
        {
            return "The selected institution was not found.";
        }

        if (specialityId.HasValue)
        {
            // Specialities are national now (T091); verify existence only — the discipline is independent
            // of the invitee's institution.
            var specialityExists = await dbContext.Set<Speciality>()
                .AnyAsync(entity => entity.Id == specialityId.Value && entity.IsActive, cancellationToken);

            if (!specialityExists)
            {
                return "The selected speciality was not found.";
            }
        }

        if (subSpecialityId.HasValue)
        {
            var subSpecialityMatchesSpeciality = await dbContext.Set<SubSpeciality>()
                .AnyAsync(entity => entity.Id == subSpecialityId.Value && entity.SpecialityId == specialityId && entity.IsActive, cancellationToken);

            if (!subSpecialityMatchesSpeciality)
            {
                return "The selected sub-speciality does not belong to the selected speciality.";
            }
        }

        return null;
    }

    public static Invitation GetActiveInvitationOrThrow(IEnumerable<Invitation> invitations, string rawToken, Common.Security.IInvitationTokenService tokenService)
    {
        var now = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(now);

        foreach (var invitation in invitations)
        {
            if (!tokenService.VerifyToken(rawToken, invitation.TokenHash))
            {
                continue;
            }

            if (invitation.RevokedOn.HasValue)
            {
                throw new InvalidOperationException("This invitation has been revoked.");
            }

            if (invitation.UsedOn.HasValue)
            {
                throw new InvalidOperationException("This invitation has already been used.");
            }

            if (invitation.ExpiresOn < today)
            {
                throw new InvalidOperationException("This invitation has expired.");
            }

            return invitation;
        }

        throw new InvalidOperationException("This invitation is invalid.");
    }
}
