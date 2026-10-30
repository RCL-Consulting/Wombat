using System.Security.Claims;
using FluentValidation;
using MediatR;
using Wombat.Application.Common.Interfaces;

namespace Wombat.Application.Features.DataRights.Commands;

/// <summary>
/// Stops, or starts again, the regular reminder emails (My data rights' Reminder emails; ReminderRecipientPolicy). Until
/// T387 it also carried an "optional processing" opt-out that nothing but the erasure read (T318, Q6): gone, with its
/// column.
/// </summary>
public sealed record UpdateObjectionFlagsCommand(
    bool OptOutOfDigestEmails,
    ClaimsPrincipal Principal) : IRequest<ReminderEmailsSaved>;

/// <summary>
/// What the save left (T387, N3): the state saved, and whether it changed it, so the page says "Reminder emails stopped."
/// or "Nothing changed: reminder emails are still stopped.".
/// </summary>
public sealed record ReminderEmailsSaved(bool OptOutOfDigestEmails, bool Changed);

public sealed class UpdateObjectionFlagsCommandValidator : AbstractValidator<UpdateObjectionFlagsCommand>
{
    public UpdateObjectionFlagsCommandValidator()
    {
        RuleFor(command => command.Principal).NotNull();
    }
}

/// <summary>
/// Handler lives in Infrastructure because it needs direct access to WombatIdentityUser
/// (an Infrastructure type). The Application layer defines the command and validator only.
/// </summary>
public interface IObjectionFlagUpdater
{
    /// <summary>Saves the flag and answers with the state saved and whether it differed from the stored one.</summary>
    Task<ReminderEmailsSaved> UpdateAsync(string userId, bool optOutOfDigestEmails, CancellationToken cancellationToken);
}

public sealed class UpdateObjectionFlagsCommandHandler : IRequestHandler<UpdateObjectionFlagsCommand, ReminderEmailsSaved>
{
    private readonly IObjectionFlagUpdater _flagUpdater;

    public UpdateObjectionFlagsCommandHandler(IObjectionFlagUpdater flagUpdater)
    {
        _flagUpdater = flagUpdater;
    }

    public async Task<ReminderEmailsSaved> Handle(UpdateObjectionFlagsCommand request, CancellationToken cancellationToken)
    {
        var userId = request.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("User identity is required.");

        return await _flagUpdater.UpdateAsync(userId, request.OptOutOfDigestEmails, cancellationToken);
    }
}
