using System.Security.Claims;
using FluentValidation;
using MediatR;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.Users.Commands.AddRoleToUser;

public sealed record AddRoleToUserCommand(string UserId, string Role, ClaimsPrincipal Principal) : IRequest;

public sealed class AddRoleToUserCommandValidator : AbstractValidator<AddRoleToUserCommand>
{
    public AddRoleToUserCommandValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.Role).NotEmpty().MaximumLength(64);
    }
}

public sealed class AddRoleToUserCommandHandler : IRequestHandler<AddRoleToUserCommand>
{
    private readonly IUserAdministrationService _userAdministrationService;

    public AddRoleToUserCommandHandler(IUserAdministrationService userAdministrationService)
    {
        _userAdministrationService = userAdministrationService;
    }

    /// <remarks>
    /// Every refusal comes before the role is written (T278): who the caller is (<see cref="UserAdministrationRules.DemandUserAdministration" />,
    /// then <see cref="UserAdministrationRules.DemandNotCaller" />) before anything is looked up, then the role, then the
    /// user's scope. The write is the service's own save, and a refused command leaves nothing for the audit pipeline's
    /// save to commit.
    /// </remarks>
    public async Task Handle(AddRoleToUserCommand request, CancellationToken cancellationToken)
    {
        UserAdministrationRules.DemandUserAdministration(request.Principal);
        UserAdministrationRules.DemandNotCaller(request.Principal, request.UserId, UserAdministrationRules.OwnRolesNotChangeable);

        if (!UserAdministrationRules.IsAssignableRole(request.Role))
        {
            throw new InvalidOperationException(
                $"The role '{request.Role}' cannot be assigned via the Users admin surface.");
        }

        var user = await _userAdministrationService.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new InvalidOperationException("The user could not be found.");

        if (user.Roles.Contains(WombatRoles.Administrator, StringComparer.Ordinal)
            && !request.Principal.IsAdministrator())
        {
            throw new UnauthorizedAccessException("You do not have permission to modify a global administrator.");
        }

        if (!request.Principal.IsAdministrator()
            && (!user.InstitutionId.HasValue || !request.Principal.CanAccessInstitution(user.InstitutionId.Value)))
        {
            throw new UnauthorizedAccessException("You do not have permission to modify this user.");
        }

        await _userAdministrationService.AddRoleAsync(request.UserId, request.Role, cancellationToken);
    }
}
