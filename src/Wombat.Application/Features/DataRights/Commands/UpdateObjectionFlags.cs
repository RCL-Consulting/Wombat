using System.Security.Claims;
using FluentValidation;
using MediatR;
using Wombat.Application.Common.Interfaces;

namespace Wombat.Application.Features.DataRights.Commands;

public sealed record UpdateObjectionFlagsCommand(
    bool OptOutOfOptionalProcessing,
    bool OptOutOfDigestEmails,
    ClaimsPrincipal Principal) : IRequest;

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
    Task UpdateAsync(string userId, bool optOutOfOptionalProcessing, bool optOutOfDigestEmails, CancellationToken cancellationToken);
}

public sealed class UpdateObjectionFlagsCommandHandler : IRequestHandler<UpdateObjectionFlagsCommand>
{
    private readonly IObjectionFlagUpdater _flagUpdater;

    public UpdateObjectionFlagsCommandHandler(IObjectionFlagUpdater flagUpdater)
    {
        _flagUpdater = flagUpdater;
    }

    public async Task Handle(UpdateObjectionFlagsCommand request, CancellationToken cancellationToken)
    {
        var userId = request.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("User identity is required.");

        await _flagUpdater.UpdateAsync(
            userId,
            request.OptOutOfOptionalProcessing,
            request.OptOutOfDigestEmails,
            cancellationToken);
    }
}
