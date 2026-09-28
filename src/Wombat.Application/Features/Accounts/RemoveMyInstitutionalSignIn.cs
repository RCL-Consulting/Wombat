using System.Security.Claims;
using FluentValidation;
using MediatR;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;

namespace Wombat.Application.Features.Accounts;

/// <summary>
/// Removes one of the signed-in user's own institutional sign-ins: whoever <paramref name="Principal" /> names, and nobody
/// else (T339, flow 02; T286's removal). A command, so the audit pipeline writes its row, stamped with the caller's
/// institution; the row names the provider, never a password: the endpoint checks the password before it sends this.
/// </summary>
/// <param name="Provider">The provider's key. The handler finds the account's login through it; no form names a subject.</param>
public sealed record RemoveMyInstitutionalSignInCommand(ClaimsPrincipal Principal, string Provider) : IRequest;

public sealed class RemoveMyInstitutionalSignInCommandValidator : AbstractValidator<RemoveMyInstitutionalSignInCommand>
{
    public RemoveMyInstitutionalSignInCommandValidator()
    {
        RuleFor(command => command.Principal).NotNull();
        RuleFor(command => command.Provider).NotEmpty().MaximumLength(128);
    }
}

public sealed class RemoveMyInstitutionalSignInCommandHandler : IRequestHandler<RemoveMyInstitutionalSignInCommand>
{
    private readonly IUserAdministrationService _userAdministrationService;

    public RemoveMyInstitutionalSignInCommandHandler(IUserAdministrationService userAdministrationService)
    {
        _userAdministrationService = userAdministrationService;
    }

    public async Task Handle(RemoveMyInstitutionalSignInCommand request, CancellationToken cancellationToken)
    {
        var userId = request.Principal.GetRequiredUserId();

        // The service checks the last way in before it removes anything, and leaves nothing staged when it refuses, so the
        // failure row the audit pipeline writes for the throw below commits no removal (the audit trap, T201).
        var removal = await _userAdministrationService.RemoveInstitutionalSignInAsync(userId, request.Provider, cancellationToken);
        if (removal != InstitutionalSignInRemoval.Removed)
        {
            throw new InstitutionalSignInRemovalRefusedException(removal);
        }
    }
}

/// <summary>
/// A removal of an institutional sign-in that did not happen, and why (T339, flow 02). The endpoint reads
/// <see cref="Reason" /> into a code, and My account chooses the words; the message is the audit row's.
/// </summary>
public sealed class InstitutionalSignInRemovalRefusedException : InvalidOperationException
{
    public InstitutionalSignInRemovalRefusedException(InstitutionalSignInRemoval reason)
        : base(reason switch
        {
            InstitutionalSignInRemoval.NotLinked => "The account has no institutional sign-in through that provider.",
            InstitutionalSignInRemoval.LastWayIn => "The institutional sign-in is the account's only way in.",
            InstitutionalSignInRemoval.Conflict => "The account changed while the institutional sign-in was being removed.",
            _ => throw new ArgumentOutOfRangeException(nameof(reason), reason, "Not a refusal.")
        })
    {
        Reason = reason;
    }

    public InstitutionalSignInRemoval Reason { get; }
}
