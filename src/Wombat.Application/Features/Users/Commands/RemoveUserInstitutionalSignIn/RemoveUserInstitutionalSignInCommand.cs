using System.Security.Claims;
using FluentValidation;
using MediatR;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Accounts;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.Users.Commands.RemoveUserInstitutionalSignIn;

/// <summary>
/// Removes another person's institutional sign-in from their page (T376, flow 12, F7, Q10; T286's external-logins half).
/// The caller's own is flow 02's My account, which asks for their password first.
/// </summary>
/// <param name="Provider">The provider's key, as the page lists it.</param>
public sealed record RemoveUserInstitutionalSignInCommand(string UserId, string Provider, ClaimsPrincipal Principal) : IRequest;

public sealed class RemoveUserInstitutionalSignInCommandValidator : AbstractValidator<RemoveUserInstitutionalSignInCommand>
{
    public RemoveUserInstitutionalSignInCommandValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.Provider).NotEmpty().MaximumLength(128);
    }
}

public sealed class RemoveUserInstitutionalSignInCommandHandler : IRequestHandler<RemoveUserInstitutionalSignInCommand>
{
    private readonly IUserAdministrationService _userAdministrationService;

    public RemoveUserInstitutionalSignInCommandHandler(IUserAdministrationService userAdministrationService)
    {
        _userAdministrationService = userAdministrationService;
    }

    /// <remarks>
    /// Who the caller is before anything is looked up (<see cref="UserAdministrationRules.DemandUserAdministration" />,
    /// then <see cref="UserAdministrationRules.DemandNotCaller" />), then the person's scope, then the global
    /// administrator's account, as the other Users commands ask. The removal is the directory's, which checks the last way
    /// in before it removes anything and leaves nothing staged when it refuses (flow 02's guard, reused): the failure row
    /// the audit pipeline writes for the throw commits no removal (T201). Its refusal is flow 02's typed one, so the page
    /// words each reason without reading the message.
    /// </remarks>
    public async Task Handle(RemoveUserInstitutionalSignInCommand request, CancellationToken cancellationToken)
    {
        UserAdministrationRules.DemandUserAdministration(request.Principal);
        UserAdministrationRules.DemandNotCaller(request.Principal, request.UserId, UserAdministrationRules.OwnSignInNotRemovable);

        var user = UserAdministrationRules.DemandInScope(
            request.Principal,
            await _userAdministrationService.GetByIdAsync(request.UserId, cancellationToken),
            "You do not have permission to modify this user.");

        if (user.Roles.Contains(WombatRoles.Administrator, StringComparer.Ordinal)
            && !request.Principal.IsAdministrator())
        {
            throw new UnauthorizedAccessException("You do not have permission to modify a global administrator.");
        }

        var removal = await _userAdministrationService.RemoveInstitutionalSignInAsync(user.UserId, request.Provider, cancellationToken);
        if (removal != InstitutionalSignInRemoval.Removed)
        {
            throw new InstitutionalSignInRemovalRefusedException(removal);
        }
    }
}
