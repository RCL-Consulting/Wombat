using System.Security.Claims;
using MediatR;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.Assessors;

public sealed record AssessorUserDto(
    string UserId,
    string Email,
    string FirstName,
    string LastName);

public sealed record ListAssessorUsersQuery(ClaimsPrincipal Principal) : IRequest<IReadOnlyList<AssessorUserDto>>;

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
        var query = users.AsEnumerable();

        if (!request.Principal.IsAdministrator())
        {
            var scopedInstitutionId = request.Principal.GetInstitutionId();
            if (!scopedInstitutionId.HasValue)
            {
                return Array.Empty<AssessorUserDto>();
            }
            query = query.Where(user => user.InstitutionId == scopedInstitutionId.Value);
        }

        return query
            .Select(user => new AssessorUserDto(user.UserId, user.Email, user.FirstName, user.LastName))
            .ToArray();
    }
}
