using System.Security.Claims;
using FluentValidation;
using MediatR;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.Users.Commands.ResetUserPassword;

public sealed record ResetUserPasswordCommand(string UserId, string NewPassword, ClaimsPrincipal Principal) : IRequest;

public sealed class ResetUserPasswordCommandValidator : AbstractValidator<ResetUserPasswordCommand>
{
    public ResetUserPasswordCommandValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.NewPassword).NotEmpty().MinimumLength(8).MaximumLength(256);
    }
}

public sealed class ResetUserPasswordCommandHandler : IRequestHandler<ResetUserPasswordCommand>
{
    private readonly IUserAdministrationService _userAdministrationService;

    public ResetUserPasswordCommandHandler(IUserAdministrationService userAdministrationService)
    {
        _userAdministrationService = userAdministrationService;
    }

    public async Task Handle(ResetUserPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userAdministrationService.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new InvalidOperationException("The user could not be found.");

        if (user.Roles.Contains(WombatRoles.Administrator, StringComparer.Ordinal)
            && !request.Principal.IsAdministrator())
        {
            throw new UnauthorizedAccessException("You do not have permission to reset a global administrator's password.");
        }

        if (user.InstitutionId.HasValue && !request.Principal.CanAccessInstitution(user.InstitutionId.Value))
        {
            throw new UnauthorizedAccessException("You do not have permission to reset this user's password.");
        }

        await _userAdministrationService.ResetPasswordAsync(request.UserId, request.NewPassword, cancellationToken);
    }
}
