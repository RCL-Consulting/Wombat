using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Domain.Invitations;

namespace Wombat.Application.Features.Invitations;

internal static class InvitationRules
{
    private static readonly HashSet<string> AllowedTargetRoles =
    [
        WombatRoles.CollegeAdmin,
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

    public static string? ValidateScope(string targetRole, int? institutionId, int? collegeId, int? specialityId, int? subSpecialityId)
        => CheckScope(targetRole, institutionId, collegeId, specialityId, subSpecialityId)?.Message;

    /// <summary>
    /// A scope refusal: its sentence, as <see cref="ValidateScope" /> gives it, and the field of Invite a person it sits under
    /// (T373, flow 11, Q3: N02's under College, N10's under Speciality, N11's under Sub-speciality).
    /// </summary>
    public sealed record ScopeRefusal(string Message, InvitationIssueField Field);

    /// <summary>
    /// <see cref="ValidateScope" />'s rule with the field each sentence is about (T373). The words are unchanged
    /// (build-contracts § 4).
    /// </summary>
    public static ScopeRefusal? CheckScope(string targetRole, int? institutionId, int? collegeId, int? specialityId, int? subSpecialityId)
    {
        if (!IsAllowedTargetRole(targetRole))
        {
            return new("The selected target role cannot be issued by invitation.", InvitationIssueField.Role);
        }

        // A CollegeAdmin is scoped to a national College, not an institution/discipline. (T093)
        if (targetRole == WombatRoles.CollegeAdmin)
        {
            if (!collegeId.HasValue || collegeId.Value <= 0)
            {
                return new("A college is required for a college administrator.", InvitationIssueField.College);
            }

            if (institutionId.HasValue || specialityId.HasValue || subSpecialityId.HasValue)
            {
                return new("College administrators may only be scoped to a college.", InvitationIssueField.College);
            }

            return null;
        }

        if (collegeId.HasValue)
        {
            return new("Only college administrators may be scoped to a college.", InvitationIssueField.College);
        }

        if (!institutionId.HasValue || institutionId.Value <= 0)
        {
            return new("An institution is required.", InvitationIssueField.Institution);
        }

        // The one sentence of a role that needs both disciplines sits under whichever is missing: Speciality first (T373).
        return targetRole switch
        {
            WombatRoles.InstitutionalAdmin when specialityId.HasValue || subSpecialityId.HasValue
                => new("Institutional administrators may only be scoped to an institution.", InvitationIssueField.Speciality),
            WombatRoles.SpecialityAdmin when !specialityId.HasValue
                => new("Speciality administrators must be scoped to a speciality.", InvitationIssueField.Speciality),
            WombatRoles.SpecialityAdmin or WombatRoles.Coordinator or WombatRoles.CommitteeMember when subSpecialityId.HasValue
                => new("The selected role may not be scoped to a sub-speciality.", InvitationIssueField.SubSpeciality),
            WombatRoles.SubSpecialityAdmin or WombatRoles.Assessor or WombatRoles.Trainee when !specialityId.HasValue
                => new("The selected role requires speciality and sub-speciality scope.", InvitationIssueField.Speciality),
            WombatRoles.SubSpecialityAdmin or WombatRoles.Assessor or WombatRoles.Trainee when !subSpecialityId.HasValue
                => new("The selected role requires speciality and sub-speciality scope.", InvitationIssueField.SubSpeciality),
            _ => null
        };
    }

    public static async Task<string?> ValidateScopeEntitiesAsync(
        IApplicationDbContext dbContext,
        int? institutionId,
        int? collegeId,
        int? specialityId,
        int? subSpecialityId,
        CancellationToken cancellationToken)
        => (await CheckScopeEntitiesAsync(dbContext, institutionId, collegeId, specialityId, subSpecialityId, cancellationToken))?.Message;

    /// <summary><see cref="ValidateScopeEntitiesAsync" />'s rule with the field each sentence is about (T373).</summary>
    public static async Task<ScopeRefusal?> CheckScopeEntitiesAsync(
        IApplicationDbContext dbContext,
        int? institutionId,
        int? collegeId,
        int? specialityId,
        int? subSpecialityId,
        CancellationToken cancellationToken)
    {
        if (institutionId.HasValue &&
            !await dbContext.Set<Institution>().AnyAsync(entity => entity.Id == institutionId.Value && entity.IsActive, cancellationToken))
        {
            return new("The selected institution was not found.", InvitationIssueField.Institution);
        }

        if (collegeId.HasValue &&
            !await dbContext.Set<College>().AnyAsync(entity => entity.Id == collegeId.Value && entity.IsActive, cancellationToken))
        {
            return new("The selected college was not found.", InvitationIssueField.College);
        }

        if (specialityId.HasValue)
        {
            // Specialities are national now (T091); verify existence only — the discipline is independent
            // of the invitee's institution.
            var specialityExists = await dbContext.Set<Speciality>()
                .AnyAsync(entity => entity.Id == specialityId.Value && entity.IsActive, cancellationToken);

            if (!specialityExists)
            {
                return new("The selected speciality was not found.", InvitationIssueField.Speciality);
            }
        }

        if (subSpecialityId.HasValue)
        {
            var subSpecialityMatchesSpeciality = await dbContext.Set<SubSpeciality>()
                .AnyAsync(entity => entity.Id == subSpecialityId.Value && entity.SpecialityId == specialityId && entity.IsActive, cancellationToken);

            if (!subSpecialityMatchesSpeciality)
            {
                return new("The selected sub-speciality does not belong to the selected speciality.", InvitationIssueField.SubSpeciality);
            }
        }

        return null;
    }

    /// <summary>
    /// An invitation's role and scope in the pages' words (<see cref="InvitationScopeWords.RoleAndScope" />), its names read
    /// by id: what the invitation's mail says it is for (T373, S12).
    /// </summary>
    public static async Task<string> RoleAndScopeAsync(
        IApplicationDbContext dbContext,
        string targetRole,
        int? institutionId,
        int? collegeId,
        int? specialityId,
        int? subSpecialityId,
        CancellationToken cancellationToken)
    {
        var institutionName = institutionId is int institution
            ? await dbContext.Set<Institution>().Where(entity => entity.Id == institution).Select(entity => entity.Name).FirstOrDefaultAsync(cancellationToken)
            : null;
        var collegeName = collegeId is int college
            ? await dbContext.Set<College>().Where(entity => entity.Id == college).Select(entity => entity.Name).FirstOrDefaultAsync(cancellationToken)
            : null;
        var specialityName = specialityId is int speciality
            ? await dbContext.Set<Speciality>().Where(entity => entity.Id == speciality).Select(entity => entity.Name).FirstOrDefaultAsync(cancellationToken)
            : null;
        var subSpecialityName = subSpecialityId is int subSpeciality
            ? await dbContext.Set<SubSpeciality>().Where(entity => entity.Id == subSpeciality).Select(entity => entity.Name).FirstOrDefaultAsync(cancellationToken)
            : null;

        return InvitationScopeWords.RoleAndScope(targetRole, institutionName, collegeName, specialityName, subSpecialityName);
    }

    /// <summary>
    /// Whether <paramref name="principal" /> may see an invitation scoped so: an Administrator every one; anyone else one at an
    /// institution they can access, never a College's (T093). Q10's in-scope judgement, the expired list and the prefill read
    /// it (T373).
    /// </summary>
    public static bool IsInScope(ClaimsPrincipal principal, int? institutionId, int? collegeId)
    {
        ArgumentNullException.ThrowIfNull(principal);
        if (principal.IsAdministrator())
        {
            return true;
        }

        return !collegeId.HasValue && institutionId is int institution && principal.CanAccessInstitution(institution);
    }

    /// <summary>
    /// The usable invitation <paramref name="rawToken" /> names, or its refusal. Its last day is judged on the South African
    /// calendar at <paramref name="utcNow" /> (<see cref="Invitation.HasPassedLastDay" />, T373, F7), from the caller's
    /// clock, so a test can pin 22:30 UTC.
    /// </summary>
    public static Invitation GetActiveInvitationOrThrow(
        IEnumerable<Invitation> invitations,
        string rawToken,
        Common.Security.IInvitationTokenService tokenService,
        DateTime utcNow)
    {
        foreach (var invitation in invitations)
        {
            if (!tokenService.VerifyToken(rawToken, invitation.TokenHash))
            {
                continue;
            }

            if (invitation.RevokedOn.HasValue)
            {
                throw new InvitationRefusedException(InvitationRefusal.Revoked);
            }

            if (invitation.UsedOn.HasValue)
            {
                throw new InvitationRefusedException(InvitationRefusal.Used);
            }

            if (Invitation.HasPassedLastDay(invitation.ExpiresOn, utcNow))
            {
                throw new InvitationRefusedException(InvitationRefusal.Expired);
            }

            return invitation;
        }

        throw new InvitationRefusedException(InvitationRefusal.Invalid);
    }
}
