using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Application.Common.Users;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.CommitteeDecisions;

/// <summary>A trainee the scheduling page may offer for a review on one panel. (T182)</summary>
/// <param name="TrainingYear">
/// The trainee's training year today, on the programme they are current in, as the picker shows it beside the name
/// ("Lerato Molefe · Training year 4", T364, 5.29); null before the programme starts.
/// </param>
public sealed record SchedulableTraineeDto(string UserId, string DisplayName, int? TrainingYear = null);

/// <summary>
/// The trainees the caller may schedule a review of on this panel: exactly the ones
/// <see cref="ScheduleCommitteeReviewCommand" /> would accept, by the same predicate
/// (<see cref="CommitteeTraineeScope.MayScheduleFor" />) over the same current trainees
/// (<see cref="Wombat.Application.Common.Security.TraineeScopeResolver.ResolveAllCurrentAsync" />, T238). Empty for a
/// panel that does not exist. (T182)
/// </summary>
/// <remarks>
/// By surname, then first name (T364, the schedule page's picker): "Anele Dlamini", "Pieter du Plessis", "Nomsa
/// Mahlangu". Each with the training year today.
/// </remarks>
/// <param name="Today">The day the training year is read on; the programme's today when null.</param>
public sealed record ListSchedulableTraineesQuery(int PanelId, ClaimsPrincipal Principal, DateOnly? Today = null)
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

        // The surname to sort by (T364): the name as the directory holds it, one lookup for them all.
        var contacts = await _userAdministrationService.GetContactsAsync(traineeUserIds, cancellationToken);

        // The training year on the active profile, the one the rule resolved each trainee by (T238), as the review page
        // reads it (GetStage).
        var today = request.Today ?? ProgrammeCalendar.DateOf(DateTime.UtcNow);
        var starts = await TraineeScopeResolver.ActiveProfiles(_dbContext)
            .AsNoTracking()
            .Where(profile => traineeUserIds.Contains(profile.UserId))
            .Select(profile => new { profile.UserId, profile.ProgrammeStartDate })
            .ToListAsync(cancellationToken);
        var yearOf = starts
            .GroupBy(start => start.UserId, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => TraineeProfile.StageOn(group.First().ProgrammeStartDate, today), StringComparer.Ordinal);

        // The one surname rule Decisions due sorts by, blank surnames included (T364, build review R3).
        return traineeUserIds
            .Select(userId => new SchedulableTraineeDto(userId, names.NameOf(userId), yearOf.GetValueOrDefault(userId)))
            .OrderBy(_ => 0)
            .ThenBySurname(trainee => trainee.UserId, contacts)
            .ToArray();
    }
}
