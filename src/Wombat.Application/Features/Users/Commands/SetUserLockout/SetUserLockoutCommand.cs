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

    /// <remarks>
    /// Every refusal comes before the lockout is written (T278): who the caller is before anything is looked up, then the
    /// user. The caller's own account is refused either way, a lock or a reactivation: until T278 only a lock was, and
    /// only after the lookup.
    /// </remarks>
    public async Task Handle(SetUserLockoutCommand request, CancellationToken cancellationToken)
    {
        UserAdministrationRules.DemandUserAdministration(request.Principal);
        UserAdministrationRules.DemandNotCaller(request.Principal, request.UserId, UserAdministrationRules.OwnLockoutNotChangeable);

        var user = await _userAdministrationService.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new InvalidOperationException("The user could not be found.");

        if (user.Roles.Contains(WombatRoles.Administrator, StringComparer.Ordinal))
        {
            throw new InvalidOperationException("A global administrator cannot be locked out from this surface.");
        }

        if (!request.Principal.IsAdministrator()
            && (!user.InstitutionId.HasValue || !request.Principal.CanAccessInstitution(user.InstitutionId.Value)))
        {
            throw new UnauthorizedAccessException("You do not have permission to modify this user.");
        }

        await _userAdministrationService.SetLockoutAsync(request.UserId, request.Locked, cancellationToken);
    }
}
