using MediatR;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.Assessors;

public sealed record AssessorUserDto(
    string UserId,
    string Email,
    string FirstName,
    string LastName);

public sealed record ListAssessorUsersQuery() : IRequest<IReadOnlyList<AssessorUserDto>>;

public sealed class ListAssessorUsersQueryHandler : IRequestHandler<ListAssessorUsersQuery, IReadOnlyList<AssessorUserDto>>
{
    private readonly IUserAdministrationService _userAdministrationService;

    public ListAssessorUsersQueryHandler(IUserAdministrationService userAdministrationService)
    {
        _userAdministrationService = userAdministrationService;
    }

    public async Task<IReadOnlyList<AssessorUserDto>> Handle(ListAssessorUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _userAdministrationService.ListUsersInRoleAsync(WombatRoles.Assessor, cancellationToken);
        return users
            .Select(user => new AssessorUserDto(user.UserId, user.Email, user.FirstName, user.LastName))
            .ToArray();
    }
}
