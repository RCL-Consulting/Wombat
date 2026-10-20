using System.Security.Claims;
using MediatR;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.Trainees;

/// <summary>
/// Trainees' Waiting to be admitted (T373, flow 11, T01–T06): the registrars who hold PendingTrainee in the caller's scope,
/// by surname, each with their institution, invited scope and registration day by name (build-contracts § 3). A
/// deactivated account is left off (S6): nobody can sign in to it, and admitting it is refused.
/// </summary>
public sealed record ListPendingTraineesQuery(ClaimsPrincipal Principal) : IRequest<IReadOnlyList<PendingTraineeDto>>;

public sealed class ListPendingTraineesQueryHandler : IRequestHandler<ListPendingTraineesQuery, IReadOnlyList<PendingTraineeDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserAdministrationService _userAdministrationService;

    public ListPendingTraineesQueryHandler(IApplicationDbContext dbContext, IUserAdministrationService userAdministrationService)
    {
        _dbContext = dbContext;
        _userAdministrationService = userAdministrationService;
    }

    public async Task<IReadOnlyList<PendingTraineeDto>> Handle(ListPendingTraineesQuery request, CancellationToken cancellationToken)
    {
        TraineeAdministrationRules.DemandTraineeAdministration(request.Principal);

        var users = await _userAdministrationService.ListUsersInRoleAsync(WombatRoles.PendingTrainee, cancellationToken);
        var waiting = users
            .Where(user => !user.IsDeactivated)
            .Where(user => AdmissionScope.IsInScope(user, request.Principal))
            .OrderBy(user => user.LastName, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(user => user.FirstName, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(user => user.Email, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (waiting.Length == 0)
        {
            return [];
        }

        var names = await RegistrarFacts.ScopeNamesAsync(_dbContext, waiting, cancellationToken);
        var rows = new List<PendingTraineeDto>(waiting.Length);
        foreach (var user in waiting)
        {
            // One read per registrar, through the address rule every other reader uses (S4): the list is the waiting few.
            var used = await RegistrarFacts.UsedInvitationAsync(_dbContext, user.Email, cancellationToken);
            rows.Add(new PendingTraineeDto(
                user.UserId,
                user.Email,
                user.FirstName,
                user.LastName,
                user.InstitutionId,
                user.SpecialityIds,
                user.SubSpecialityIds)
            {
                InstitutionName = user.InstitutionId is { } institutionId ? names.Institutions.GetValueOrDefault(institutionId) : null,
                InvitedAs = names.InvitedAs(user),
                RegisteredOn = used?.UsedOn is { } usedOn ? ProgrammeCalendar.DateOf(usedOn) : null
            });
        }

        return rows;
    }
}
