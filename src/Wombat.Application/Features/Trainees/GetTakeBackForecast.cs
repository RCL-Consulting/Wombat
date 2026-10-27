using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Activities;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.Trainees;

/// <summary>
/// What recording <see cref="LastDay" /> as the programme's last day would take back (T379, flow 13, M2): read by the
/// trainee profile when Mark complete… or Deactivate… opens its dialog, for the day typed, so the dialog says "…: 1
/// activity, observed 2026-10-25, will stop counting." before anything is done. The count is
/// <see cref="ProgrammeEndCredit.CountTakenBackAsync" />'s, the rule the end commands answer with
/// (<see cref="ProgrammeEndResult" />), so the forecast and the result cannot disagree for the same data. A forecast only:
/// a completion that credits between the dialog and the confirm is taken back too, and the result says so.
/// </summary>
public sealed record GetTakeBackForecastQuery(int ProfileId, DateOnly LastDay, ClaimsPrincipal Principal) : IRequest<TakeBackForecastDto>;

/// <param name="Count">How many activities observed after the day have credit the end would take back; zero says nothing.</param>
/// <param name="ObservedOn">The one activity's day when <paramref name="Count" /> is 1; null otherwise.</param>
public sealed record TakeBackForecastDto(int Count, DateOnly? ObservedOn);

public sealed class GetTakeBackForecastQueryHandler : IRequestHandler<GetTakeBackForecastQuery, TakeBackForecastDto>
{
    private readonly IApplicationDbContext _dbContext;

    public GetTakeBackForecastQueryHandler(IApplicationDbContext dbContext) => _dbContext = dbContext;

    public async Task<TakeBackForecastDto> Handle(GetTakeBackForecastQuery request, CancellationToken cancellationToken)
    {
        // As the end commands: a Trainee-holding admin ends nothing (T290), and another institution's profile is not found.
        TraineeAdministrationRules.DemandTraineeAdministration(request.Principal);

        var profile = await _dbContext.Set<TraineeProfile>()
            .AsNoTracking()
            .Where(entity => entity.Id == request.ProfileId)
            .Select(entity => new { entity.UserId, entity.InstitutionId })
            .SingleOrDefaultAsync(cancellationToken);

        if (profile is null || !request.Principal.CanAccessInstitution(profile.InstitutionId))
        {
            throw new InvalidOperationException("The trainee profile could not be found.");
        }

        var observed = await ProgrammeEndCredit.ObservedTakenBackAsync(
            _dbContext.Set<Activity>(), profile.UserId, request.LastDay, cancellationToken);

        return new TakeBackForecastDto(observed.Count, observed.Count == 1 ? observed[0] : null);
    }
}
