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

    public async Task Handle(AddRoleToUserCommand request, CancellationToken cancellationToken)
    {
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
