using System.Security.Claims;
using MediatR;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Services;
using Microsoft.EntityFrameworkCore;
using Wombat.Domain.Activities;
using Wombat.Domain.Epas;

namespace Wombat.Application.Features.Activities.Queries.GetActivityById;

/// <summary>
/// Loads one activity together with the calling actor's writable field set and available
/// transitions (T070), the name of everyone in its history (T142), and what its status card says: who has it now,
/// whether it was returned, its nominee and its name (T342, B6, B7, E7).
/// </summary>
/// <remarks>
/// <c>Principal</c> is both the read gate and the input to the writable set. Returns null when the
/// activity does not exist OR the caller may not read it — one answer for both, so the id space
/// cannot be walked (T101).
/// </remarks>
public sealed record GetActivityByIdQuery(int ActivityId, ClaimsPrincipal Principal) : IRequest<ActivityDetailDto?>;

public sealed class GetActivityByIdQueryHandler : IRequestHandler<GetActivityByIdQuery, ActivityDetailDto?>
{
    private readonly IActivityService _activityService;
    private readonly IUserAdministrationService _users;
    private readonly IApplicationDbContext _dbContext;

    public GetActivityByIdQueryHandler(
        IActivityService activityService,
        IUserAdministrationService users,
        IApplicationDbContext dbContext)
    {
        _activityService = activityService;
        _users = users;
        _dbContext = dbContext;
    }

    public async Task<ActivityDetailDto?> Handle(GetActivityByIdQuery request, CancellationToken cancellationToken)
    {
        var detail = await _activityService.GetDetailAsync(request.ActivityId, request.Principal, cancellationToken);
        if (detail is null)
        {
            // Nothing is looked up for an activity the caller may not read: the lookup is behind the read gate.
            return null;
        }

        var activity = detail.Activity;
        var transitions = activity.Transitions;

        // T342 (B6, B7): the status card, read from the pinned workflow and form the detail already carries, by the code
        // My activities reads its rows with, so the card and the row cannot disagree. The newest move is the history's
        // newest by the order the lists read it.
        var last = transitions
            .OrderByDescending(transition => transition.OccurredOn)
            .ThenByDescending(transition => transition.Id)
            .FirstOrDefault();
        var facts = new ActivityRowFacts(
            activity.Id,
            activity.ActivityTypeId,
            activity.SchemaVersion,
            activity.ActivityTypeName,
            activity.SubjectUserId,
            activity.CreatedByUserId,
            activity.CurrentState,
            activity.EpaId,
            EpaCode: null,
            activity.ObservedOn,
            activity.ObservedOnDeclared,
            activity.DataJson,
            last is null ? null : new ActivityLastMove(
                last.FromState, last.ToState, last.ActorUserId, last.OccurredOn, last.Note, last.TransitionKey));

        // E7: whether another of the subject's activities that the caller may read shares its type, EPA and date. Read
        // through the read rule, so an activity the caller cannot open never changes the name of one they can.
        var readable = _dbContext.Set<Activity>().WhereReadableBy(request.Principal);
        var shared = await ActivityRowDetails.SharedKeysAsync(readable, [facts], cancellationToken);
        // The EPA's title and whether it is in force ride with its code, for the page's About card (T342, C13).
        var epa = activity.EpaId is null
            ? null
            : await _dbContext.Set<Epa>()
                .AsNoTracking()
                .Where(candidate => candidate.Id == activity.EpaId.Value)
                .Select(candidate => new { candidate.Code, candidate.Title, candidate.IsActive })
                .FirstOrDefaultAsync(cancellationToken);
        var epaCode = epa?.Code;

        var form = PinnedForm.Parse(activity.SchemaJson, activity.WorkflowJson);
        // The history's Actor column, by name, in the same one lookup as the card's names. Here rather than in
        // ActivityService.Map, which create and transition share and whose results no page shows as history.
        var (statuses, names) = await ActivityRowDetails.ResolveAsync(
            [facts with { EpaCode = epaCode }],
            _ => form,
            fact => shared.Contains(fact.CollisionKey),
            request.Principal,
            _users,
            cancellationToken,
            alsoNamed: transitions.Select(transition => transition.ActorUserId).Append(activity.SubjectUserId));
        var status = statuses[activity.Id];

        return detail with
        {
            Activity = activity with
            {
                Transitions = transitions
                    .Select(transition => transition with { ActorName = names.NameOf(transition.ActorUserId) })
                    .ToList()
            },
            Holder = status.Holder,
            Returned = status.Returned,
            NomineeName = status.NomineeName,
            DisplayName = status.DisplayName,
            SubjectName = names.NameOf(activity.SubjectUserId),
            EpaCode = epa?.Code,
            EpaTitle = epa?.Title,
            EpaInForce = epa?.IsActive
        };
    }
}
