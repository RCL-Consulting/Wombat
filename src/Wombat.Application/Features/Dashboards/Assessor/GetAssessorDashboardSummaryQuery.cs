using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Options;
using Wombat.Application.Common.Users;
using Wombat.Application.Features.Activities.Services;
using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Workflow;

namespace Wombat.Application.Features.Dashboards.Assessor;

public sealed record GetAssessorDashboardSummaryQuery(ClaimsPrincipal Principal) : IRequest<AssessorDashboardSummaryDto>;

/// <summary>
/// The Assessor's dashboard: what waits on the caller, and what the caller recently decided, each read from the
/// activity's PINNED workflow (T297; "finished" since T203), never from a state's key.
/// </summary>
/// <remarks>
/// Neither list holds the caller's own portfolio. A user who is also a Trainee created, and made the "create" move on,
/// every activity of their own, and may submit or cancel it; a nominee is never the subject (NomineeGate), so none of those
/// is assessor work. Without the exclusion a logged procedure, which is born in its terminal state, would read as the
/// caller's decision (T203).
/// </remarks>
public sealed class GetAssessorDashboardSummaryQueryHandler
    : IRequestHandler<GetAssessorDashboardSummaryQuery, AssessorDashboardSummaryDto>
{
    /// <summary>How many of the activities waiting on the caller the card lists, oldest first; the count is of them all.</summary>
    public const int AwaitingReviewListed = 10;

    /// <summary>How many recent decisions the card lists.</summary>
    public const int RecentDecisionsListed = 10;

    /// <summary>
    /// How many of the activities the caller moved last are read at a time, newest move first, until the decisions among
    /// them fill the card or none is left.
    /// </summary>
    public const int LastMovedRead = 50;

    private readonly IApplicationDbContext _dbContext;
    private readonly IWorkflowEvaluator _workflowEvaluator;
    private readonly IUserAdministrationService _users;
    private readonly DashboardThresholds _thresholds;

    public GetAssessorDashboardSummaryQueryHandler(
        IApplicationDbContext dbContext,
        IWorkflowEvaluator workflowEvaluator,
        IUserAdministrationService users,
        IOptions<DashboardThresholds> thresholds)
    {
        _dbContext = dbContext;
        _workflowEvaluator = workflowEvaluator;
        _users = users;
        _thresholds = thresholds.Value;
    }

    public async Task<AssessorDashboardSummaryDto> Handle(
        GetAssessorDashboardSummaryQuery request, CancellationToken cancellationToken)
    {
        var userId = request.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        var dueCutoff = DateTime.UtcNow.AddDays(-_thresholds.AssessorDueDays);

        // T297: what waits on the caller is what their Activity Inbox lists, less their own portfolio, read by the same
        // code. Until T297 it was the activities in a state keyed "requested" that the caller had created or already
        // moved: the trainee makes both the create and the submit, so a request naming the assessor never counted, and a
        // portfolio review waits in "submitted". "Accepted, needing action" read a state no CPSA workflow has.
        var waiting = (await ActivityWaiting.LoadActionableAsync(
                _dbContext.Set<Activity>(), _dbContext, _workflowEvaluator, request.Principal, cancellationToken: cancellationToken,
                arms: ActorArms.NotAuthor))
            .Where(row => row.Activity.SubjectUserId != userId)
            .OrderBy(row => row.Activity.UpdatedOn)
            .ThenBy(row => row.Activity.Id)
            .ToList();
        var listed = waiting.Take(AwaitingReviewListed).ToList();

        // A decision is an activity the caller moved last that is finished or has no move left (T297): not the literal
        // "declined" and "cancelled", which included a request the trainee withdrew after the caller had moved it, and left
        // out a dead end of any other name, such as a rejected research output.
        // The caller's move is the latest by time, ties to the later row, as the inbox reads the latest credit (T108).
        // Newest move first, which is the card's own order, read a page at a time until ten decisions are found: a page of
        // moves that each left a move to make (portfolio reviews the caller returned to draft) must not crowd older
        // decisions off the card, as one fixed read of the last fifty did (T297 review).
        var lastMovedQuery = _dbContext.Set<Activity>()
            .AsNoTracking()
            .Where(activity => activity.SubjectUserId != userId)
            // Implied by the next test, and cheaper: only the activities the caller ever moved are asked who moved last.
            .Where(activity => activity.Transitions.Any(transition => transition.ActorUserId == userId))
            .Where(activity => activity.Transitions
                .OrderByDescending(transition => transition.OccurredOn)
                .ThenByDescending(transition => transition.Id)
                .Select(transition => transition.ActorUserId)
                .FirstOrDefault() == userId)
            .OrderByDescending(activity => activity.Transitions.Max(transition => transition.OccurredOn))
            .ThenByDescending(activity => activity.Id)
            .Select(activity => new LastMoved(
                activity.Id,
                activity.ActivityTypeId,
                activity.SchemaVersion,
                activity.CurrentState,
                activity.SubjectUserId,
                activity.ActivityType.Name,
                activity.Transitions.Max(transition => transition.OccurredOn)));

        // T203: "finished" is a terminal state of the activity's PINNED workflow (D44, ActivityCompletion), not the literal
        // "completed": a discussed reflective exercise, a recorded MSF row and an accepted teaching session are finished.
        // The same pinned workflow names the state each decision left the activity in (T220), and says whether any move is
        // left from it. With no workflow nothing can move it, so it is where the caller left it.
        var decided = new List<(LastMoved Activity, Workflow? Workflow, bool IsFinished)>();
        for (var read = 0; decided.Count < RecentDecisionsListed; read += LastMovedRead)
        {
            var page = await lastMovedQuery.Skip(read).Take(LastMovedRead).ToListAsync(cancellationToken);
            var workflows = await PinnedWorkflows.LoadAsync(
                _dbContext,
                page.Select(activity => (activity.ActivityTypeId, activity.SchemaVersion)),
                cancellationToken);
            foreach (var activity in page)
            {
                var workflow = workflows[(activity.ActivityTypeId, activity.SchemaVersion)];
                var isFinished = ActivityCompletion.FinishedStates(workflow).Contains(activity.CurrentState);
                var hasMoveLeft = workflow?.HasOutgoingTransition(activity.CurrentState) ?? false;
                if (isFinished || !hasMoveLeft)
                {
                    decided.Add((activity, workflow, isFinished));
                }
            }

            if (page.Count < LastMovedRead)
            {
                break;
            }
        }

        decided = decided.Take(RecentDecisionsListed).ToList();

        // Whose each row is, by name, in one lookup for the rows the page lists (T142's rule). Until T250 each row carried
        // the subject's user id in SubjectName.
        var names = await UserDisplayNames.ResolveAsync(
            _users,
            listed.Select(row => row.Activity.SubjectUserId).Concat(decided.Select(row => row.Activity.SubjectUserId)),
            cancellationToken);

        var awaitingReview = listed
            .Select(row => new AwaitingReviewItem(
                row.Activity.Id,
                row.Activity.ActivityType.Name,
                names.NameOf(row.Activity.SubjectUserId),
                row.Activity.CurrentState,
                PinnedWorkflows.StateLabel(row.Workflow, row.Activity.CurrentState),
                row.Activity.UpdatedOn,
                row.Activity.UpdatedOn < dueCutoff))
            .ToList();

        var recentDecisions = decided
            .Select(row => new RecentDecisionItem(
                row.Activity.Id,
                row.Activity.TypeName,
                names.NameOf(row.Activity.SubjectUserId),
                row.Activity.CurrentState,
                PinnedWorkflows.StateLabel(row.Workflow, row.Activity.CurrentState),
                row.IsFinished,
                row.Activity.DecidedOn))
            .ToList();

        return new AssessorDashboardSummaryDto(waiting.Count, awaitingReview, recentDecisions);
    }

    /// <summary>An activity the caller moved last, as the decisions read it: <paramref name="DecidedOn" /> is that move's time.</summary>
    private sealed record LastMoved(
        int Id,
        int ActivityTypeId,
        int SchemaVersion,
        string CurrentState,
        string SubjectUserId,
        string TypeName,
        DateTime DecidedOn);
}
