using System.Security.Claims;
using FluentValidation;
using MediatR;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.Users.Commands.SetUserLockout;

public sealed record SetUserLockoutCommand(string UserId, bool Locked, ClaimsPrincipal Principal) : IRequest;

public sealed class SetUserLockoutCommandValidator : AbstractValidator<SetUserLockoutCommand>
{
    public SetUserLockoutCommandValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();
    }
}

public sealed class SetUserLockoutCommandHandler : IRequestHandler<SetUserLockoutCommand>
{
    private readonly IUserAdministrationService _userAdministrationService;

    public SetUserLockoutCommandHandler(IUserAdministrationService userAdministrationService)
    {
        _userAdministrationService = userAdministrationService;
    }

    public async Task Handle(SetUserLockoutCommand request, CancellationToken cancellationToken)
    {
        var user = await _userAdministrationService.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new InvalidOperationException("The user could not be found.");

        if (user.Roles.Contains(WombatRoles.Administrator, StringComparer.Ordinal))
        {
            throw new InvalidOperationException("A global administrator cannot be locked out from this surface.");
        }

        if (user.InstitutionId.HasValue && !request.Principal.CanAccessInstitution(user.InstitutionId.Value))
        {
            throw new UnauthorizedAccessException("You do not have permission to modify this user.");
        }

        var callerUserId = request.Principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (request.Locked && string.Equals(callerUserId, request.UserId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("You cannot lock out your own account.");
        }

        await _userAdministrationService.SetLockoutAsync(request.UserId, request.Locked, cancellationToken);
    }
}
