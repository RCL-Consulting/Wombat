using System.Security.Claims;
using FluentValidation;
using MediatR;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Institutions.Queries.GetAdoptedDisciplinesForInstitution;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.Users.Commands.AddRoleToUser;

/// <summary>
/// Gives a person a role, and the speciality or sub-speciality it needs, as one write (T376, flow 12, round 3 M2).
/// </summary>
/// <param name="SpecialityId">
/// A speciality to add to the person's scope, when the role needs one they do not hold, or (Coordinator, Committee member)
/// to keep them to it; null when the scope they hold is enough (Q4: one scope per person, Add a role adds to it).
/// </param>
/// <param name="SubSpecialityId">A sub-speciality to add, for a role that needs one they do not hold (Assessor, Sub-speciality admin).</param>
public sealed record AddRoleToUserCommand(
    string UserId,
    string Role,
    int? SpecialityId,
    int? SubSpecialityId,
    ClaimsPrincipal Principal) : IRequest;

public sealed class AddRoleToUserCommandValidator : AbstractValidator<AddRoleToUserCommand>
{
    public AddRoleToUserCommandValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.Role).NotEmpty().WithMessage("Choose a role.").MaximumLength(64);
    }
}

public sealed class AddRoleToUserCommandHandler : IRequestHandler<AddRoleToUserCommand>
{
    private readonly IUserAdministrationService _userAdministrationService;
    private readonly IApplicationDbContext _dbContext;

    public AddRoleToUserCommandHandler(IUserAdministrationService userAdministrationService, IApplicationDbContext dbContext)
    {
        _userAdministrationService = userAdministrationService;
        _dbContext = dbContext;
    }

    /// <remarks>
    /// <para>
    /// Every refusal comes before the role is written (T278): who the caller is (<see cref="UserAdministrationRules.DemandUserAdministration" />,
    /// then <see cref="UserAdministrationRules.DemandNotCaller" />) before anything is looked up, then the role, then the
    /// user's scope, then what the role needs. A refused command leaves nothing for the audit pipeline's save to commit.
    /// The role is refused before the lookup too: Trainee, which only admission grants (T303), and any other role the
    /// surface does not manage.
    /// </para>
    /// <para>
    /// <b>The scope</b> (T376, M2, F1, F2, S13). A person holds one scope for every role (Q4), so the role's need is judged
    /// against the scope the person will hold, the one they hold with what this adds (F1): Patel, an Assessor in Paediatrics /
    /// Paediatrics, is given Committee member with nothing to add, and the order the two roles came in does not change the
    /// scope. Until T376 the page sent the role alone, so an Assessor given without a sub-speciality could not be named on
    /// any request. A speciality given to someone at Any speciality narrows them, for every role they hold (F2); the page
    /// says so before and after. The invitation's "may not be scoped to a sub-speciality" is a rule about what an
    /// invitation asks, never applied to a scope already held. What may be added is what the PERSON's institution has
    /// adopted (<see cref="AdoptedDisciplines" />), read once, as the form reads it.
    /// </para>
    /// <para>
    /// <b>One write.</b> The role and the scope are saved in one transaction (<see cref="IUserAdministrationService.InOneTransactionAsync{T}" />):
    /// a fault after the role leaves neither, so a retry adds both. Inside it the scope is read again after the role's write
    /// and the union recomputed (T376 build review, B2), so a scope another admin changed since the first read is kept; a
    /// refusal only that second reading finds is rolled back with the role.
    /// </para>
    /// </remarks>
    public async Task Handle(AddRoleToUserCommand request, CancellationToken cancellationToken)
    {
        UserAdministrationRules.DemandUserAdministration(request.Principal);
        UserAdministrationRules.DemandNotCaller(request.Principal, request.UserId, UserAdministrationRules.OwnRolesNotChangeable);
        UserAdministrationRules.DemandAssignableRole(
            request.Role, role => $"The role '{role}' cannot be assigned via the Users admin surface.");

        var user = UserAdministrationRules.DemandInScope(
            request.Principal,
            await _userAdministrationService.GetByIdAsync(request.UserId, cancellationToken),
            "You do not have permission to modify this user.");

        if (user.Roles.Contains(WombatRoles.Administrator, StringComparer.Ordinal)
            && !request.Principal.IsAdministrator())
        {
            throw new UnauthorizedAccessException("You do not have permission to modify a global administrator.");
        }

        if (user.Roles.Contains(request.Role, StringComparer.Ordinal))
        {
            throw new RoleAddRefusedException(
                RoleAddRefusal.AlreadyHeld, RoleAddField.Role, UserAdministrationRules.RoleAlreadyHeld(request.Role));
        }

        // Every role the surface gives works at an institution (U14's College admin has none): nothing is written.
        if (user.InstitutionId is not int institutionId)
        {
            throw new RoleAddRefusedException(
                RoleAddRefusal.NoInstitution, RoleAddField.Role, UserAdministrationRules.RoleNeedsAnInstitution);
        }

        var adopted = await AdoptedDisciplines.LoadAsync(_dbContext, institutionId, cancellationToken);

        // Judged first on the scope as read, so a refusal the page can foresee comes before anything is written.
        _ = ResultingScope(request, user, adopted);

        await _userAdministrationService.InOneTransactionAsync(
            async token =>
            {
                // The role's write updates the account's row, which holds it until the commit: another Add a role, or any
                // scope write, waits behind it. The scope is then read again and the union computed from what is held NOW
                // (T376 build review, B2): two admins adding at once never write a scope that drops the other's, or leaves
                // a role whose need the scope no longer meets. A refusal here rolls the role back with the transaction.
                await _userAdministrationService.AddRoleAsync(request.UserId, request.Role, token);

                var held = UserAdministrationRules.DemandInScope(
                    request.Principal,
                    await _userAdministrationService.GetByIdAsync(request.UserId, token),
                    "You do not have permission to modify this user.");
                if (held.InstitutionId != institutionId)
                {
                    throw new InvalidOperationException(
                        "This person's institution changed while the role was being added. Nothing was saved.");
                }

                var (specialityIds, subSpecialityIds) = ResultingScope(request, held, adopted);
                await _userAdministrationService.UpdateScopeAsync(request.UserId, institutionId, specialityIds, subSpecialityIds, token);
                return true;
            },
            cancellationToken);
    }

    /// <summary>
    /// The scope the person will hold (held ∪ what is added), once what is added is one the institution adopted and the
    /// role's need is met by the result; otherwise the refusal, under the field it is about.
    /// </summary>
    private static (IReadOnlyCollection<int> Specialities, IReadOnlyCollection<int> SubSpecialities) ResultingScope(
        AddRoleToUserCommand request,
        UserIdentityDetails user,
        AdoptedDisciplinesDto adopted)
    {
        var role = request.Role;

        if (role == WombatRoles.InstitutionalAdmin && (request.SpecialityId.HasValue || request.SubSpecialityId.HasValue))
        {
            throw new RoleAddRefusedException(
                RoleAddRefusal.NotScopable, RoleAddField.Speciality, "Institutional administrators may only be scoped to an institution.");
        }

        if (request.SubSpecialityId.HasValue && !UserAdministrationRules.RoleTakesSubSpeciality(role))
        {
            throw new RoleAddRefusedException(
                RoleAddRefusal.NotScopable, RoleAddField.SubSpeciality, "The selected role may not be scoped to a sub-speciality.");
        }

        if (request.SpecialityId is int specialityId && adopted.Specialities.All(entry => entry.Id != specialityId))
        {
            throw new RoleAddRefusedException(
                RoleAddRefusal.NotAdopted, RoleAddField.Speciality, UserAdministrationRules.SpecialityNotAdopted);
        }

        var specialities = user.SpecialityIds.Concat(request.SpecialityId is int added ? [added] : []).Distinct().Order().ToArray();

        if (request.SubSpecialityId is int subSpecialityId)
        {
            var sub = adopted.SubSpecialities.FirstOrDefault(entry => entry.Id == subSpecialityId)
                ?? throw new RoleAddRefusedException(
                    RoleAddRefusal.NotAdopted, RoleAddField.SubSpeciality, UserAdministrationRules.SubSpecialityNotAdopted);

            // A sub-speciality of a speciality the person will not hold would be a scope no gate reads.
            if (!specialities.Contains(sub.SpecialityId))
            {
                throw new RoleAddRefusedException(
                    RoleAddRefusal.NotAdopted, RoleAddField.SubSpeciality, UserAdministrationRules.SubSpecialityNotUnderScope);
            }
        }

        var subSpecialities = user.SubSpecialityIds
            .Concat(request.SubSpecialityId is int addedSub ? [addedSub] : [])
            .Distinct()
            .Order()
            .ToArray();

        // The role's need, judged on the resulting scope (F1), in the built words (Spec 3: built refusals).
        if (role == WombatRoles.SpecialityAdmin && specialities.Length == 0)
        {
            throw new RoleAddRefusedException(
                RoleAddRefusal.ScopeNeeded, RoleAddField.Speciality, "Speciality administrators must be scoped to a speciality.");
        }

        if (UserAdministrationRules.RoleTakesSubSpeciality(role))
        {
            if (specialities.Length == 0)
            {
                throw new RoleAddRefusedException(
                    RoleAddRefusal.ScopeNeeded, RoleAddField.Speciality, "The selected role requires speciality and sub-speciality scope.");
            }

            if (subSpecialities.Length == 0)
            {
                throw new RoleAddRefusedException(
                    RoleAddRefusal.ScopeNeeded, RoleAddField.SubSpeciality, "The selected role requires speciality and sub-speciality scope.");
            }
        }

        return (specialities, subSpecialities);
    }
}

/// <summary>Why Add a role… was refused (T376, flow 12), so the page puts the words beside the field they are about.</summary>
public enum RoleAddRefusal
{
    /// <summary>The person already holds the role.</summary>
    AlreadyHeld,

    /// <summary>The person has no institution, and every role offered here works at one.</summary>
    NoInstitution,

    /// <summary>A speciality or sub-speciality the person's institution has not adopted (S13).</summary>
    NotAdopted,

    /// <summary>A scope the role does not take (a speciality for Institutional admin, a sub-speciality for a role with none).</summary>
    NotScopable,

    /// <summary>The scope the person would hold does not meet the role's need (F1).</summary>
    ScopeNeeded
}

/// <summary>The field of Add a role… a refusal is about.</summary>
public enum RoleAddField
{
    Role,
    Speciality,
    SubSpeciality
}

/// <summary>
/// Add a role…'s refusal with its reason and its field (T376, flow 12; on the pattern of
/// <c>InvitationIssueRefusedException</c>): the page maps the field, never the message's text. The message names nobody
/// outside the caller's scope: the person is already in it.
/// </summary>
public sealed class RoleAddRefusedException : InvalidOperationException
{
    public RoleAddRefusedException(RoleAddRefusal reason, RoleAddField field, string message)
        : base(message)
    {
        Reason = reason;
        Field = field;
    }

    public RoleAddRefusal Reason { get; }

    public RoleAddField Field { get; }
}
