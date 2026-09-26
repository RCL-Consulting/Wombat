using System.Security.Claims;
using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Users;
using Wombat.Application.Features.Activities.Services;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Domain.Activities;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.Dashboards.Trainee;

/// <param name="AsOf">The day to read curriculum progress for. Defaults to today in South Africa; tests pin it.</param>
public sealed record GetTraineeDashboardSummaryQuery(ClaimsPrincipal Principal, DateOnly? AsOf = null) : IRequest<TraineeDashboardSummaryDto>;

public sealed class GetTraineeDashboardSummaryQueryHandler
    : IRequestHandler<GetTraineeDashboardSummaryQuery, TraineeDashboardSummaryDto>
{
    /// <summary>How many of the inbox's rows the Activity inbox card lists: its first, the newest.</summary>
    public const int InboxListed = 5;

    private readonly IApplicationDbContext _dbContext;
    private readonly IWorkflowEvaluator _workflowEvaluator;
    private readonly IUserAdministrationService _users;

    public GetTraineeDashboardSummaryQueryHandler(
        IApplicationDbContext dbContext,
        IWorkflowEvaluator workflowEvaluator,
        IUserAdministrationService users)
    {
        _dbContext = dbContext;
        _workflowEvaluator = workflowEvaluator;
        _users = users;
    }

    public async Task<TraineeDashboardSummaryDto> Handle(
        GetTraineeDashboardSummaryQuery request, CancellationToken cancellationToken)
    {
        var userId = request.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        var isPending = request.Principal.IsInRole("PendingTrainee") &&
                        !request.Principal.IsInRole("Trainee");

        if (isPending)
        {
            return new TraineeDashboardSummaryDto(null, [], [], [], IsPendingTrainee: true);
        }

        // The same read model as the progress page (T130), so the card and the page cannot disagree. It is
        // item-driven: a period that has only just begun reads "0 of 3" rather than showing nothing.
        var curriculumTargets = await TraineeQuotaProgressReader.ReadAsync(
            _dbContext, userId, request.AsOf ?? QuotaCalendar.Today(), cancellationToken);

        // T297: the Activity inbox card lists what /activities/inbox lists, read by the same code: the activities the
        // caller can move now, newest first. Until T297 it listed the states keyed requested, accepted, declined or draft,
        // so it kept a declined CPSA request, which has no move left, and dropped a reflection awaiting discussion, which
        // the trainee may still cancel (Steps 3.12, 3.16). A declined request is shown, with its badge, on Recent
        // activities (while it is among the five newest) and on My Activities. No mail announces it:
        // AssessmentDeclinedEmail has no sender (T320).
        var actionable = (await ActivityWaiting.LoadActionableAsync(
                _dbContext.Set<Activity>(), _dbContext, _workflowEvaluator, request.Principal, cancellationToken: cancellationToken))
            .Take(InboxListed)
            .ToList();

        // For a caller who is also an assessor the inbox holds other trainees' work, which the card names, as the inbox
        // and the Assessor's card do (T250); the caller's own rows need no name. One lookup, and none for a trainee alone.
        var names = await UserDisplayNames.ResolveAsync(
            _users,
            actionable.Select(row => row.Activity.SubjectUserId).Where(subject => subject != userId),
            cancellationToken);

        var inbox = actionable
            .Select(row => new ActivityInboxItem(
                row.Activity.Id,
                row.Activity.ActivityType.Name,
                row.Activity.CurrentState,
                PinnedWorkflows.StateLabel(row.Workflow, row.Activity.CurrentState),
                row.Activity.UpdatedOn,
                row.Activity.SubjectUserId == userId ? null : names.NameOf(row.Activity.SubjectUserId)))
            .ToList();

        // T203: an activity is finished in a terminal state of its PINNED workflow (D44, ActivityCompletion), not in the
        // literal "completed". A discussed reflective exercise, a recorded MSF row, a logged procedure and an accepted
        // teaching session are all done, so none of them has a deadline still to meet. Read once, from the pins and states
        // alone.
        var activityStates = await _dbContext.Set<Activity>()
            .AsNoTracking()
            .Where(a => a.SubjectUserId == userId)
            .Select(a => new
            {
                a.Id,
                a.ActivityTypeId,
                a.SchemaVersion,
                a.CurrentState
            })
            .ToListAsync(cancellationToken);

        var recent = await _dbContext.Set<Activity>()
            .AsNoTracking()
            .Where(a => a.SubjectUserId == userId)
            .OrderByDescending(a => a.CreatedOn)
            .Take(5)
            .Select(a => new
            {
                a.Id,
                a.ActivityTypeId,
                a.SchemaVersion,
                TypeName = a.ActivityType.Name,
                a.CurrentState,
                a.CreatedOn
            })
            .ToListAsync(cancellationToken);

        // Each pin's workflow, read once for Recent activities and the deadlines: it says which states are finished (T203),
        // which have a move left, and what each state is called, so a badge names the state as the activity's own page
        // does (T220).
        var workflows = await PinnedWorkflows.LoadAsync(
            _dbContext,
            activityStates.Select(a => (a.ActivityTypeId, a.SchemaVersion))
                .Concat(recent.Select(a => (a.ActivityTypeId, a.SchemaVersion))),
            cancellationToken);
        var finishedStates = workflows.ToDictionary(pair => pair.Key, pair => ActivityCompletion.FinishedStates(pair.Value));
        string StateLabel(int activityTypeId, int version, string state)
            => PinnedWorkflows.StateLabel(workflows[(activityTypeId, version)], state);

        var recentActivities = recent
            .Select(a => new RecentActivityItem(
                a.Id,
                a.TypeName,
                a.CurrentState,
                StateLabel(a.ActivityTypeId, a.SchemaVersion, a.CurrentState),
                finishedStates[(a.ActivityTypeId, a.SchemaVersion)].Contains(a.CurrentState),
                a.CreatedOn))
            .ToList();

        var cutoff = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(14));
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Upcoming deadlines: scan DataJson for fields with a "due_date" key
        // This is done client-side since jsonb path queries vary by provider
        // Only work still open has a deadline to meet: not finished, and with a move left in its pinned workflow. A
        // cancelled or declined request is a dead end (T297, where this read the literal "cancelled"). With no workflow
        // there is no way to tell, so only the finished test applies.
        var deadlineCandidateIds = activityStates
            .Where(a =>
            {
                var workflow = workflows[(a.ActivityTypeId, a.SchemaVersion)];
                return !finishedStates[(a.ActivityTypeId, a.SchemaVersion)].Contains(a.CurrentState) &&
                       (workflow is null || workflow.HasOutgoingTransition(a.CurrentState));
            })
            .Select(a => a.Id)
            .ToList();
        var candidateActivities = await _dbContext.Set<Activity>()
            .AsNoTracking()
            .Where(a => a.SubjectUserId == userId && deadlineCandidateIds.Contains(a.Id))
            .Select(a => new { a.Id, TypeName = a.ActivityType.Name, a.DataJson })
            .ToListAsync(cancellationToken);

        var upcomingDeadlines = new List<UpcomingDeadlineItem>();
        foreach (var activity in candidateActivities)
        {
            try
            {
                using var doc = JsonDocument.Parse(activity.DataJson);
                foreach (var prop in doc.RootElement.EnumerateObject())
                {
                    if (prop.Name.Contains("due_date", StringComparison.OrdinalIgnoreCase) &&
                        prop.Value.ValueKind == JsonValueKind.String &&
                        DateOnly.TryParse(prop.Value.GetString(), out var dueDate) &&
                        dueDate >= today && dueDate <= cutoff)
                    {
                        upcomingDeadlines.Add(new UpcomingDeadlineItem(
                            activity.Id, activity.TypeName, prop.Name, dueDate));
                    }
                }
            }
            catch (JsonException)
            {
                // Skip activities with invalid JSON
            }
        }

        return new TraineeDashboardSummaryDto(
            curriculumTargets,
            inbox,
            recentActivities,
            upcomingDeadlines.OrderBy(d => d.DueDate).Take(5).ToList(),
            IsPendingTrainee: false);
    }

    public static int? ComputeTraineeStage(DateOnly programmeStart, DateOnly today)
        => new TraineeProfile { ProgrammeStartDate = programmeStart }.GetStage(today);
}
