using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Services;
using Wombat.Domain.Activities;
using Wombat.Domain.Epas;

namespace Wombat.Application.Features.Activities.Queries.ListActivitiesBySubject;

/// <summary>
/// One trainee's activity list.
/// </summary>
/// <remarks>
/// <see cref="SubjectUserId" /> is whose list is being asked for; <see cref="Principal" /> is who is
/// asking. Both are needed: the query used to carry only the first, and since the subject id arrives
/// from the caller, anyone who could reach the handler could read any trainee's list by naming them.
/// (T101)
/// </remarks>
public sealed record ListActivitiesBySubjectQuery(string SubjectUserId, ClaimsPrincipal Principal)
    : IRequest<IReadOnlyList<ActivitySummaryDto>>;

public sealed class ListActivitiesBySubjectQueryHandler : IRequestHandler<ListActivitiesBySubjectQuery, IReadOnlyList<ActivitySummaryDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public ListActivitiesBySubjectQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ActivitySummaryDto>> Handle(ListActivitiesBySubjectQuery request, CancellationToken cancellationToken)
    {
        var readable = _dbContext.Set<Activity>()
            .AsNoTracking()
            .Where(activity => activity.SubjectUserId == request.SubjectUserId)
            // Applied to the rows rather than to the subject, so that the answer to "may I see this
            // trainee?" is decided per activity by the stamps on it. A trainee who moved institution
            // keeps their old assessments out of their new overseers' sight, which is what the stamps
            // are for. Out of scope yields an empty list, not an error: this is a list, and a refusal
            // would confirm the trainee exists. (T101)
            .WhereReadableBy(request.Principal);

        // T137. The EPA comes from the stamped column, joined LEFT: an activity about no EPA is still listed. Before
        // the stamp existed the EPA lived only inside DataJson, so a released MSF campaign covering eight EPAs listed
        // as eight identical rows. An EPA that is not in force now is marked on the row (T231). The credit column is
        // T106 item 14: the latest transition that evaluated credit.
        // Ordered by the encounter date, the column the list shows in place of the audit clock, so an encounter filed
        // late sits at its own date rather than at the top of a list that would then look unsorted. Ties fall back to
        // the most recently updated, then the id, so the order is total.
        var rows = await (
                from activity in readable
                join epa in _dbContext.Set<Epa>() on activity.EpaId equals (int?)epa.Id into epas
                from epa in epas.DefaultIfEmpty()
                orderby activity.ObservedOn descending, activity.UpdatedOn descending, activity.Id descending
                select new
                {
                    activity.Id,
                    activity.ActivityTypeId,
                    activity.SchemaVersion,
                    TypeKey = activity.ActivityType.Key,
                    TypeName = activity.ActivityType.Name,
                    activity.SubjectUserId,
                    activity.CurrentState,
                    activity.CreatedOn,
                    activity.UpdatedOn,
                    activity.EpaId,
                    EpaCode = epa == null ? null : epa.Code,
                    EpaTitle = epa == null ? null : epa.Title,
                    // T231. In force now, by the rule the activity's own picker labels by (EpaOptionLabel).
                    EpaInForce = epa == null ? null : (bool?)epa.IsActive,
                    activity.ObservedOn,
                    ObservedOnDeclared = activity.ObservedOnSource == ObservationDateSource.Declared,
                    // Null when no transition evaluated credit, which is the honest answer for an activity still in
                    // flight and for a type that credits nothing by design (T108).
                    CreditedItemCount = activity.Transitions
                        .Where(transition => transition.CreditedItemCount != null)
                        .OrderByDescending(transition => transition.OccurredOn)
                        .ThenByDescending(transition => transition.Id)
                        .Select(transition => transition.CreditedItemCount)
                        .FirstOrDefault()
                })
            .ToListAsync(cancellationToken);

        // T220: each state in the words of the workflow the activity is pinned to, read once per pin.
        var workflows = await PinnedWorkflows.LoadAsync(
            _dbContext, rows.Select(row => (row.ActivityTypeId, row.SchemaVersion)), cancellationToken);

        return rows
            .Select(row => new ActivitySummaryDto(
                row.Id,
                row.ActivityTypeId,
                row.TypeKey,
                row.TypeName,
                row.SubjectUserId,
                row.CurrentState,
                PinnedWorkflows.StateLabel(workflows[(row.ActivityTypeId, row.SchemaVersion)], row.CurrentState),
                row.CreatedOn,
                row.UpdatedOn,
                row.EpaId,
                row.EpaCode,
                row.EpaTitle,
                row.EpaInForce,
                row.ObservedOn,
                row.ObservedOnDeclared,
                row.CreditedItemCount))
            .ToList();
    }
}
