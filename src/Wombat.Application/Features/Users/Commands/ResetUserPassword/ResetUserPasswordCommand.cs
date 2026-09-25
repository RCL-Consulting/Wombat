using System.Security.Claims;
using FluentValidation;
using MediatR;
using Wombat.Application.Audit;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.Users.Commands.ResetUserPassword;

/// <remarks>
/// <c>NewPassword</c> is redacted from the audit summary. The AuditPipelineBehavior audits every
/// request whose type name ends in "Command" and AuditPayloadSerializer writes its properties into
/// SummaryJson; without this marker the admin-chosen password was stored in the audit table in
/// plaintext, next to the UserId it belongs to, and rendered raw on AuditDetail — undoing the point
/// of hashing it in Identity. The UserId stays in the clear: who was reset, and by whom, is exactly
/// what this row exists to record. (T101)
/// </remarks>
public sealed record ResetUserPasswordCommand(
    string UserId,
    [property: Redact] string NewPassword,
    ClaimsPrincipal Principal) : IRequest;

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

    /// <remarks>
    /// Every refusal comes before the password is written (T278): who the caller is before anything is looked up, then the
    /// user. The caller's own password is refused: a reset here asks for no current password, so an administrator's
    /// session left open was enough to take the account. They change it on the Change password page, which asks for it.
    /// </remarks>
    public async Task Handle(ResetUserPasswordCommand request, CancellationToken cancellationToken)
    {
        UserAdministrationRules.DemandUserAdministration(request.Principal);
        UserAdministrationRules.DemandNotCaller(request.Principal, request.UserId, UserAdministrationRules.OwnPasswordNotResettable);

        var user = await _userAdministrationService.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new InvalidOperationException("The user could not be found.");

        if (user.Roles.Contains(WombatRoles.Administrator, StringComparer.Ordinal)
            && !request.Principal.IsAdministrator())
        {
            throw new UnauthorizedAccessException("You do not have permission to reset a global administrator's password.");
        }

        if (!request.Principal.IsAdministrator()
            && (!user.InstitutionId.HasValue || !request.Principal.CanAccessInstitution(user.InstitutionId.Value)))
        {
            throw new UnauthorizedAccessException("You do not have permission to reset this user's password.");
        }

        await _userAdministrationService.ResetPasswordAsync(request.UserId, request.NewPassword, cancellationToken);
    }
}
