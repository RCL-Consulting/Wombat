using System.Security.Claims;
using MediatR;
using Microsoft.Extensions.Options;
using Wombat.Application.Common;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Options;
using Wombat.Application.Features.Institutions.Queries.GetAdoptedDisciplinesForInstitution;
using Wombat.Application.Features.Users;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.Sso;

/// <summary>
/// No validator: the handler enforces the provider's institution and the roles a mapping may give. The group's id and
/// display name are required by the page before dispatch.
/// </summary>
[NoValidator]
public sealed record CreateSsoGroupMappingCommand(
    string ProviderKey,
    string ExternalGroupId,
    string ExternalGroupDisplayName,
    string WombatRole,
    int InstitutionId,
    int? SpecialityId,
    int? SubSpecialityId,
    ClaimsPrincipal Principal) : IRequest<int>;

public sealed class CreateSsoGroupMappingCommandHandler : IRequestHandler<CreateSsoGroupMappingCommand, int>
{
    /// <summary>The refusal of a mapping to Trainee or PendingTrainee, which only admission gives. (T333)</summary>
    public const string AdmissionRoleNotMappable =
        "Trainee and Pending trainee cannot be given by an SSO group mapping. A registrar becomes a trainee when admitted " +
        "to a curriculum, from Trainees.";

    /// <summary>The refusal of a mapping whose provider is not configured, or serves another institution. (T288)</summary>
    public const string ProviderNotTheInstitutions =
        "That sign-in provider does not serve the institution chosen. A mapping is made for its provider's own institution.";

    /// <summary>The refusal of a mapping to Administrator, which always needs a manual assignment.</summary>
    public const string AdministratorNotMappable = "The Administrator role cannot be assigned via SSO group mapping.";

    /// <summary>
    /// The refusal of a mapping to Institutional admin by anyone but the Administrator (T376, Q19, PROPOSED; O06): the page's
    /// select never offers it to her, so these words meet only a posted role, beside Wombat role.
    /// </summary>
    public const string InstitutionalAdminNotMappable = "Only a Wombat administrator can map a group to Institutional admin.";

    /// <summary>The refusal of a speciality or sub-speciality on a mapping to Institutional admin, which is kept to an institution (T376 build review, B3).</summary>
    public const string InstitutionalAdminNotScopable = "Institutional administrators may only be scoped to an institution.";

    /// <summary>The refusal of a sub-speciality on a role kept to no more than a speciality (T376 build review, B3).</summary>
    public const string SubSpecialityNotScopable = "The selected role may not be scoped to a sub-speciality.";

    /// <summary>The built refusal of a Speciality admin mapping with no speciality (the form's own words, Spec 3).</summary>
    public const string SpecialityAdminNeedsSpeciality = "Speciality administrators must be scoped to a speciality.";

    /// <summary>The built refusal of an Assessor or Sub-speciality admin mapping with no speciality and sub-speciality.</summary>
    public const string NeedsSpecialityAndSubSpeciality = "The selected role requires speciality and sub-speciality scope.";

    /// <summary>The refusal of a speciality the institution has not adopted (T376 build review, B3; S13's rule).</summary>
    public const string SpecialityNotAdopted = "Choose a speciality the institution has adopted.";

    /// <summary>The refusal of a sub-speciality the institution has not adopted, or of another speciality (T376 build review, B3).</summary>
    public const string SubSpecialityNotAdopted = "Choose a sub-speciality of that speciality the institution has adopted.";

    /// <summary>The refusal of a role no mapping gives at an institution: College admin, or a key that is no role (T376).</summary>
    public const string RoleNotMappable = "That role cannot be given by an SSO group mapping.";

    /// <summary>
    /// The roles a mapping may give at an institution (T376, S4): the staff roles. Administrator is never mapped, Trainee
    /// and Pending trainee are admission's (T333), and College admin is a College's, which no institution's group can give.
    /// Institutional admin is the Administrator's alone to map (Q19).
    /// </summary>
    public static readonly IReadOnlyList<string> MappableRoles =
    [
        WombatRoles.InstitutionalAdmin,
        WombatRoles.SpecialityAdmin,
        WombatRoles.SubSpecialityAdmin,
        WombatRoles.Coordinator,
        WombatRoles.CommitteeMember,
        WombatRoles.Assessor
    ];

    private readonly IApplicationDbContext _dbContext;
    private readonly IOptions<SsoOptions> _ssoOptions;

    public CreateSsoGroupMappingCommandHandler(IApplicationDbContext dbContext, IOptions<SsoOptions> ssoOptions)
    {
        _dbContext = dbContext;
        _ssoOptions = ssoOptions;
    }

    /// <remarks>
    /// Every refusal comes before the write (T288). <b>A trainee first</b>
    /// (<see cref="UserAdministrationRules.DemandUserAdministration" />, T185, T278): a mapping grants roles at each
    /// sign-in, so a registrar who also administers makes none. Then the institution, which must be the caller's and the
    /// provider's own: the mapper applies a provider's mappings to the users who sign in through it, all of them at the
    /// provider's institution, so until T288 an InstitutionalAdmin at A could map B's provider's groups and grant B's
    /// users roles. Then the role: only a staff role (<see cref="MappableRoles" />), and Institutional admin only for the
    /// Administrator (T376, Q19). Then the scope it gives (<see cref="DemandScopeAsync" />, T376 build review, B3). Each refusal is a <see cref="SsoMappingRefusedException" /> naming its field, so the page
    /// puts it beside Wombat role (O06) by its reason, never by its text.
    /// </remarks>
    public async Task<int> Handle(CreateSsoGroupMappingCommand request, CancellationToken cancellationToken)
    {
        UserAdministrationRules.DemandUserAdministration(request.Principal);

        if (!request.Principal.CanAccessInstitution(request.InstitutionId))
        {
            throw new UnauthorizedAccessException("You do not have permission to create SSO mappings for that institution.");
        }

        if (!SsoMappingProviders.Serves(_ssoOptions.Value, request.ProviderKey, request.InstitutionId))
        {
            throw new SsoMappingRefusedException(
                SsoMappingRefusal.ProviderNotTheInstitutions, SsoMappingField.Provider, ProviderNotTheInstitutions);
        }

        if (string.Equals(request.WombatRole, WombatRoles.Administrator, StringComparison.Ordinal))
        {
            throw new SsoMappingRefusedException(
                SsoMappingRefusal.AdministratorNotMappable, SsoMappingField.Role, AdministratorNotMappable);
        }

        // Admission's roles (T333): a mapping would grant Trainee with no profile, give a graduate it back, and take it
        // from a running trainee whenever their group changed.
        if (WombatRoles.AdmissionManaged.Contains(request.WombatRole, StringComparer.Ordinal))
        {
            throw new SsoMappingRefusedException(
                SsoMappingRefusal.AdmissionRoleNotMappable, SsoMappingField.Role, AdmissionRoleNotMappable);
        }

        if (!MappableRoles.Contains(request.WombatRole, StringComparer.Ordinal))
        {
            throw new SsoMappingRefusedException(SsoMappingRefusal.RoleNotMappable, SsoMappingField.Role, RoleNotMappable);
        }

        // Q19 (T376, PROPOSED): a group that gave Institutional admin would make each of its members an administrator of
        // the institution at their next sign-in, a power the Institutional admin who made it could pass on to a group she
        // does not control. Only the Administrator maps it.
        if (string.Equals(request.WombatRole, WombatRoles.InstitutionalAdmin, StringComparison.Ordinal)
            && !request.Principal.IsAdministrator())
        {
            throw new SsoMappingRefusedException(
                SsoMappingRefusal.InstitutionalAdminNotMappable, SsoMappingField.Role, InstitutionalAdminNotMappable);
        }

        await DemandScopeAsync(request, cancellationToken);

        var mapping = new SsoGroupRoleMapping
        {
            ProviderKey = request.ProviderKey,
            ExternalGroupId = request.ExternalGroupId,
            ExternalGroupDisplayName = request.ExternalGroupDisplayName,
            WombatRole = request.WombatRole,
            InstitutionId = request.InstitutionId,
            SpecialityId = request.SpecialityId,
            SubSpecialityId = request.SubSpecialityId
        };

        _dbContext.Set<SsoGroupRoleMapping>().Add(mapping);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return mapping.Id;
    }

    /// <summary>
    /// The scope the mapping gives, checked as Add a role… checks a person's (T376 build review, B3): provisioning grants it
    /// at each member's sign-in, so a posted speciality the institution has not adopted, a sub-speciality of another
    /// speciality, or an Assessor with none would give every member a scope no gate reads. The page offers only valid
    /// pairs, so these words meet only a posted request; each sits under its field. The role's need is in the built words
    /// the form's own checks use (<see cref="UserAdministrationRules.RoleNeedsSpeciality" />,
    /// <see cref="UserAdministrationRules.RoleTakesSubSpeciality" />); what may be named is <see cref="AdoptedDisciplines" />.
    /// </summary>
    private async Task DemandScopeAsync(CreateSsoGroupMappingCommand request, CancellationToken cancellationToken)
    {
        var role = request.WombatRole;

        if (role == WombatRoles.InstitutionalAdmin)
        {
            if (request.SpecialityId.HasValue || request.SubSpecialityId.HasValue)
            {
                throw new SsoMappingRefusedException(
                    SsoMappingRefusal.NotScopable, SsoMappingField.Speciality, InstitutionalAdminNotScopable);
            }

            return;
        }

        if (request.SubSpecialityId.HasValue && !UserAdministrationRules.RoleTakesSubSpeciality(role))
        {
            throw new SsoMappingRefusedException(
                SsoMappingRefusal.NotScopable, SsoMappingField.SubSpeciality, SubSpecialityNotScopable);
        }

        if (UserAdministrationRules.RoleNeedsSpeciality(role) && request.SpecialityId is null)
        {
            throw new SsoMappingRefusedException(
                SsoMappingRefusal.ScopeNeeded,
                SsoMappingField.Speciality,
                role == WombatRoles.SpecialityAdmin ? SpecialityAdminNeedsSpeciality : NeedsSpecialityAndSubSpeciality);
        }

        if (UserAdministrationRules.RoleTakesSubSpeciality(role) && request.SubSpecialityId is null)
        {
            throw new SsoMappingRefusedException(
                SsoMappingRefusal.ScopeNeeded, SsoMappingField.SubSpeciality, NeedsSpecialityAndSubSpeciality);
        }

        if (request.SpecialityId is null && request.SubSpecialityId is null)
        {
            return;
        }

        var adopted = await AdoptedDisciplines.LoadAsync(_dbContext, request.InstitutionId, cancellationToken);

        if (request.SpecialityId is int specialityId && adopted.Specialities.All(entry => entry.Id != specialityId))
        {
            throw new SsoMappingRefusedException(
                SsoMappingRefusal.NotAdopted, SsoMappingField.Speciality, SpecialityNotAdopted);
        }

        if (request.SubSpecialityId is int subSpecialityId
            && !adopted.SubSpecialities.Any(entry => entry.Id == subSpecialityId && entry.SpecialityId == request.SpecialityId))
        {
            throw new SsoMappingRefusedException(
                SsoMappingRefusal.NotAdopted, SsoMappingField.SubSpeciality, SubSpecialityNotAdopted);
        }
    }
}
