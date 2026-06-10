using System.Security.Claims;
using FluentValidation;
using MediatR;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.Users.Commands.RemoveRoleFromUser;

public sealed record RemoveRoleFromUserCommand(string UserId, string Role, ClaimsPrincipal Principal) : IRequest;

public sealed class RemoveRoleFromUserCommandValidator : AbstractValidator<RemoveRoleFromUserCommand>
{
    public RemoveRoleFromUserCommandValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.Role).NotEmpty().MaximumLength(64);
    }
}

public sealed class RemoveRoleFromUserCommandHandler : IRequestHandler<RemoveRoleFromUserCommand>
{
    private readonly IUserAdministrationService _userAdministrationService;

    public RemoveRoleFromUserCommandHandler(IUserAdministrationService userAdministrationService)
    {
        _userAdministrationService = userAdministrationService;
    }

    public async Task Handle(RemoveRoleFromUserCommand request, CancellationToken cancellationToken)
    {
        if (!UserAdministrationRules.IsAssignableRole(request.Role))
        {
            throw new InvalidOperationException(
                $"The role '{request.Role}' cannot be removed via the Users admin surface.");
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

        await _userAdministrationService.RemoveRoleAsync(request.UserId, request.Role, cancellationToken);
    }
}
