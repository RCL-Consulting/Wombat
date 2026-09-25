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

    /// <remarks>
    /// Every refusal comes before the role is removed (T278): who the caller is (<see cref="UserAdministrationRules.DemandUserAdministration" />,
    /// then <see cref="UserAdministrationRules.DemandNotCaller" />) before anything is looked up, then the role, then the
    /// user's scope. Until T278 a Trainee who also held InstitutionalAdmin could remove their own Trainee role here, and
    /// with it every trainee-first refusal (T185, T256).
    /// </remarks>
    public async Task Handle(RemoveRoleFromUserCommand request, CancellationToken cancellationToken)
    {
        UserAdministrationRules.DemandUserAdministration(request.Principal);
        UserAdministrationRules.DemandNotCaller(request.Principal, request.UserId, UserAdministrationRules.OwnRolesNotChangeable);

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
