using MediatR;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.Trainees;

public sealed record ListPendingTraineesQuery() : IRequest<IReadOnlyList<PendingTraineeDto>>;

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
        return users
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
