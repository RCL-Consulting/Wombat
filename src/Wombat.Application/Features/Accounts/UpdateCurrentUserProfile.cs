using FluentValidation;
using MediatR;
using Wombat.Application.Common.Interfaces;

namespace Wombat.Application.Features.Accounts;

public sealed record UpdateCurrentUserProfileCommand(
    string UserId,
    string FirstName,
    string LastName) : IRequest<UserProfileDto>;

public sealed class UpdateCurrentUserProfileCommandValidator : AbstractValidator<UpdateCurrentUserProfileCommand>
{
    public UpdateCurrentUserProfileCommandValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();
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
        await _userAdministrationService.UpdateNamesAsync(request.UserId, request.FirstName, request.LastName, cancellationToken);

        var user = await _userAdministrationService.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new InvalidOperationException("The user profile could not be found.");

        return new UserProfileDto(user.UserId, user.Email, user.FirstName, user.LastName, user.Roles);
    }
}
