using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.CommitteeDecisions;

namespace Wombat.Application.Features.CommitteeDecisions;

/// <summary>A trainee the scheduling page may offer for a review on one panel. (T182)</summary>
public sealed record SchedulableTraineeDto(string UserId, string DisplayName);

/// <summary>
/// The trainees the caller may schedule a review of on this panel: exactly the ones
/// <see cref="ScheduleCommitteeReviewCommand" /> would accept, by the same predicate
/// (<see cref="CommitteeTraineeScope.MayScheduleFor" />). Empty for a panel that does not exist. (T182)
/// </summary>
public sealed record ListSchedulableTraineesQuery(int PanelId, ClaimsPrincipal Principal)
    : IRequest<IReadOnlyList<SchedulableTraineeDto>>;

public sealed class ListSchedulableTraineesQueryHandler
    : IRequestHandler<ListSchedulableTraineesQuery, IReadOnlyList<SchedulableTraineeDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserAdministrationService _userAdministrationService;

    public ListSchedulableTraineesQueryHandler(
        IApplicationDbContext dbContext,
        IUserAdministrationService userAdministrationService)
    {
        _dbContext = dbContext;
        _userAdministrationService = userAdministrationService;
    }

    public async Task<IReadOnlyList<SchedulableTraineeDto>> Handle(
        ListSchedulableTraineesQuery request,
        CancellationToken cancellationToken)
    {
        var panel = await _dbContext.Set<DecisionPanel>()
            .AsNoTracking()
            .SingleOrDefaultAsync(entity => entity.Id == request.PanelId, cancellationToken);

        if (panel is null)
        {
            return [];
        }

        var traineeUserIds = await CommitteeTraineeScope.ListSchedulableAsync(
            _dbContext, request.Principal, panel, cancellationToken);

        if (traineeUserIds.Count == 0)
        {
            return [];
        }

        // A profile whose identity row is gone has no one to name; it is left out rather than offered as a bare id.
        var names = await _userAdministrationService.GetDisplayNamesAsync(traineeUserIds, cancellationToken);

        return traineeUserIds
            .Where(names.ContainsKey)
            .Select(userId => new SchedulableTraineeDto(
                userId,
                string.IsNullOrWhiteSpace(names[userId]) ? userId : names[userId]))
            .OrderBy(trainee => trainee.DisplayName, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(trainee => trainee.UserId, StringComparer.Ordinal)
            .ToArray();
    }
}
