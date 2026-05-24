using System.Security.Claims;
using MediatR;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.Trainees;

public sealed record ListPendingTraineesQuery(ClaimsPrincipal Principal) : IRequest<IReadOnlyList<PendingTraineeDto>>;

public sealed class ListPendingTraineesQueryHandler : IRequestHandler<ListPendingTraineesQuery, IReadOnlyList<PendingTraineeDto>>
{
    private readonly IUserAdministrationService _userAdministrationService;

    public ListPendingTraineesQueryHandler(IUserAdministrationService userAdministrationService)
    {
        _userAdministrationService = userAdministrationService;
    }

    public async Task<IReadOnlyList<PendingTraineeDto>> Handle(ListPendingTraineesQuery request, CancellationToken cancellationToken)
    {
        var users = await _userAdministrationService.ListUsersInRoleAsync(WombatRoles.PendingTrainee, cancellationToken);
        var query = users.AsEnumerable();

        if (!request.Principal.IsAdministrator())
        {
            var scopedInstitutionId = request.Principal.GetInstitutionId();
            if (!scopedInstitutionId.HasValue)
            {
                return Array.Empty<PendingTraineeDto>();
            }
            query = query.Where(user => user.InstitutionId == scopedInstitutionId.Value);
        }

        return query
            .Select(user => new PendingTraineeDto(
                user.UserId,
                user.Email,
                user.FirstName,
                user.LastName,
                user.InstitutionId,
                user.SpecialityIds,
                user.SubSpecialityIds))
            .ToArray();
    }
}
