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
/// One trainee's activity list, a page at a time: My activities' All activities table.
/// </summary>
/// <remarks>
/// <see cref="SubjectUserId" /> is whose list is being asked for; <see cref="Principal" /> is who is
/// asking. Both are needed: the query used to carry only the first, and since the subject id arrives
/// from the caller, anyone who could reach the handler could read any trainee's list by naming them.
/// (T101)
/// </remarks>
/// <param name="Page">The page to serve, from 1; brought within the list's pages (T342, B7).</param>
/// <param name="PageSize">Rows per page, 20 by default as the page offers; brought within 1 and <see cref="MaxPageSize" />.</param>
public sealed record ListActivitiesBySubjectQuery(
    string SubjectUserId,
    ClaimsPrincipal Principal,
    int Page = 1,
    int PageSize = ListActivitiesBySubjectQuery.DefaultPageSize)
    : IRequest<ActivityListPageDto>
{
    /// <summary>The page size My activities opens with.</summary>
    public const int DefaultPageSize = 20;

    /// <summary>The largest page served.</summary>
    public const int MaxPageSize = 100;
}

public sealed class ListActivitiesBySubjectQueryHandler : IRequestHandler<ListActivitiesBySubjectQuery, ActivityListPageDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserAdministrationService _users;

    public ListActivitiesBySubjectQueryHandler(IApplicationDbContext dbContext, IUserAdministrationService users)
    {
        _dbContext = dbContext;
        _users = users;
    }

    public async Task<ActivityListPageDto> Handle(ListActivitiesBySubjectQuery request, CancellationToken cancellationToken)
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
        // T342 (B7): with the data, the creator and the newest move, which say who has each row now, whom it names and
        // whether it came back. Every row is read, and the page cut from them in memory: a name needs its nominee when
        // ANY of the subject's activities shares its type, EPA and date (E7), and one registrar's list is a programme's
        // worth of rows, not a table's.
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
                    activity.CreatedByUserId,
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
                    activity.DataJson,
                    // Null when no transition evaluated credit, which is the honest answer for an activity still in
                    // flight and for a type that credits nothing by design (T108).
                    CreditedItemCount = activity.Transitions
                        .Where(transition => transition.CreditedItemCount != null)
                        .OrderByDescending(transition => transition.OccurredOn)
                        .ThenByDescending(transition => transition.Id)
                        .Select(transition => transition.CreditedItemCount)
                        .FirstOrDefault(),
                    // The newest move, by the order the credit column reads: when it entered its state, and whether that
                    // was a return (T342, B6).
                    LastMove = activity.Transitions
                        .OrderByDescending(transition => transition.OccurredOn)
                        .ThenByDescending(transition => transition.Id)
                        .Select(transition => new
                        {
                            transition.FromState,
                            transition.ToState,
                            transition.ActorUserId,
                            transition.OccurredOn,
                            transition.Note,
                            transition.TransitionKey
                        })
                        .FirstOrDefault()
                })
            .ToListAsync(cancellationToken);

        var facts = rows
            .Select(row => new ActivityRowFacts(
                row.Id,
                row.ActivityTypeId,
                row.SchemaVersion,
                row.TypeName,
                row.SubjectUserId,
                row.CreatedByUserId,
                row.CurrentState,
                row.EpaId,
                row.EpaCode,
                row.ObservedOn,
                row.ObservedOnDeclared,
                row.DataJson,
                row.LastMove is { } last
                    ? new ActivityLastMove(
                        last.FromState, last.ToState, last.ActorUserId, last.OccurredOn, last.Note, last.TransitionKey)
                    : null))
            .ToList();

        // E7: the keys more than one of these rows share, counted over the whole list, not the page.
        var shared = facts
            .GroupBy(fact => fact.CollisionKey)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToHashSet();

        var pageSize = Math.Clamp(request.PageSize, 1, ListActivitiesBySubjectQuery.MaxPageSize);
        var pageCount = Math.Max(1, (rows.Count + pageSize - 1) / pageSize);
        var page = Math.Clamp(request.Page, 1, pageCount);
        var pageFacts = facts.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        // T220: each state in the words of the workflow the activity is pinned to, read once per pin; T342: with the pinned
        // form, whose EPA and date fields decide the name's segments (E9).
        var forms = await PinnedForms.LoadAsync(
            _dbContext, pageFacts.Select(fact => (fact.ActivityTypeId, fact.SchemaVersion)), cancellationToken);
        var (details, _) = await ActivityRowDetails.ResolveAsync(
            pageFacts,
            fact => forms[(fact.ActivityTypeId, fact.SchemaVersion)],
            fact => shared.Contains(fact.CollisionKey),
            request.Principal,
            _users,
            cancellationToken);

        var byId = rows.ToDictionary(row => row.Id);
        var items = pageFacts
            .Select(fact =>
            {
                var row = byId[fact.Id];
                var detail = details[fact.Id];
                return new ActivitySummaryDto(
                    row.Id,
                    row.ActivityTypeId,
                    row.TypeKey,
                    row.TypeName,
                    row.SubjectUserId,
                    row.CurrentState,
                    PinnedWorkflows.StateLabel(forms[(row.ActivityTypeId, row.SchemaVersion)].Workflow, row.CurrentState),
                    row.CreatedOn,
                    row.UpdatedOn,
                    row.EpaId,
                    row.EpaCode,
                    row.EpaTitle,
                    row.EpaInForce,
                    row.ObservedOn,
                    row.ObservedOnDeclared,
                    row.CreditedItemCount)
                {
                    Holder = detail.Holder,
                    NomineeName = detail.NomineeName,
                    Returned = detail.Returned,
                    DisplayName = detail.DisplayName,
                    DisplayNameHasNominee = detail.DisplayNameHasNominee,
                    Shape = detail.Shape
                };
            })
            .ToList();

        return new ActivityListPageDto(items, page, pageSize, rows.Count);
    }
}
