using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Users;
using Wombat.Domain.CommitteeDecisions;

namespace Wombat.Application.Features.CommitteeDecisions;

/// <summary>A trainee the scheduling page may offer for a review on one panel. (T182)</summary>
public sealed record SchedulableTraineeDto(string UserId, string DisplayName);

/// <summary>
/// The trainees the caller may schedule a review of on this panel: exactly the ones
/// <see cref="ScheduleCommitteeReviewCommand" /> would accept, by the same predicate
/// (<see cref="CommitteeTraineeScope.MayScheduleFor" />) over the same current trainees
/// (<see cref="Wombat.Application.Common.Security.TraineeScopeResolver.ResolveAllCurrentAsync" />, T238). Empty for a
/// panel that does not exist. (T182)
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
            _dbContext, _userAdministrationService, request.Principal, panel, cancellationToken);

        if (traineeUserIds.Count == 0)
        {
            return [];
        }

        // Every trainee the rule keeps is a current trainee, whose account exists (T238), so each has a name; the id is
        // shown only for one with no name on record. An erased trainee's pseudonym is not among them: the rule leaves it
        // out, for the handler as here, where until T238 the picker left it out by a filter the handler did not apply.
        var names = await UserDisplayNames.ResolveAsync(_userAdministrationService, traineeUserIds, cancellationToken);

        return traineeUserIds
            .Select(userId => new SchedulableTraineeDto(userId, names.NameOf(userId)))
            .OrderBy(trainee => trainee.DisplayName, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(trainee => trainee.UserId, StringComparer.Ordinal)
            .ToArray();
    }
}
