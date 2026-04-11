using MediatR;
using Wombat.Application.Common.Interfaces;

namespace Wombat.Application.Features.Accounts;

public sealed record GetCurrentUserProfileQuery(string UserId) : IRequest<UserProfileDto>;

public sealed class GetCurrentUserProfileQueryHandler : IRequestHandler<GetCurrentUserProfileQuery, UserProfileDto>
{
    private readonly IUserAdministrationService _userAdministrationService;

    public GetCurrentUserProfileQueryHandler(IUserAdministrationService userAdministrationService)
    {
        _userAdministrationService = userAdministrationService;
    }

    public async Task<UserProfileDto> Handle(GetCurrentUserProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await _userAdministrationService.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new InvalidOperationException("The user profile could not be found.");

        return new UserProfileDto(user.UserId, user.Email, user.FirstName, user.LastName, user.Roles);
    }
}
