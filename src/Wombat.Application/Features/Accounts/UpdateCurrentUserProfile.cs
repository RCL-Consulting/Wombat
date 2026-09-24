using System.Security.Claims;
using FluentValidation;
using MediatR;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;

namespace Wombat.Application.Features.Accounts;

/// <summary>
/// Renames the signed-in user: whoever <paramref name="Principal" /> names, and nobody else. (T185)
/// </summary>
/// <remarks>
/// Until T185 the user to rename was an id in the request, so the handler would rename whoever it was handed. The id is
/// now read from the caller, before anything is written.
/// </remarks>
public sealed record UpdateCurrentUserProfileCommand(
    ClaimsPrincipal Principal,
    string FirstName,
    string LastName) : IRequest<UserProfileDto>;

public sealed class UpdateCurrentUserProfileCommandValidator : AbstractValidator<UpdateCurrentUserProfileCommand>
{
    public UpdateCurrentUserProfileCommandValidator()
    {
        RuleFor(command => command.Principal).NotNull();
        RuleFor(command => command.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(command => command.LastName).NotEmpty().MaximumLength(100);
    }
}

public sealed class UpdateCurrentUserProfileCommandHandler : IRequestHandler<UpdateCurrentUserProfileCommand, UserProfileDto>
{
    private readonly IUserAdministrationService _userAdministrationService;

    public UpdateCurrentUserProfileCommandHandler(IUserAdministrationService userAdministrationService)
    {
        _userAdministrationService = userAdministrationService;
    }

    public async Task<UserProfileDto> Handle(UpdateCurrentUserProfileCommand request, CancellationToken cancellationToken)
    {
        var userId = request.Principal.GetRequiredUserId();

        await _userAdministrationService.UpdateNamesAsync(userId, request.FirstName, request.LastName, cancellationToken);

        var user = await _userAdministrationService.GetByIdAsync(userId, cancellationToken)
            ?? throw new InvalidOperationException("The user profile could not be found.");

        return new UserProfileDto(user.UserId, user.Email, user.FirstName, user.LastName, user.Roles);
    }
}
