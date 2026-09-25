using System.Security.Claims;
using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
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
    private readonly IApplicationDbContext _dbContext;

    public GetTraineeDashboardSummaryQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
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

        // T203: an activity is finished in a terminal state of its PINNED workflow (D44, ActivityCompletion), not in the
        // literal "completed". A discussed reflective exercise, a recorded MSF row, a logged procedure and an accepted
        // teaching session are all done, so none of them is waiting in the inbox or has a deadline still to meet. Read
        // once, from the pins and states alone.
        var activityStates = await _dbContext.Set<Activity>()
            .AsNoTracking()
            .Where(a => a.SubjectUserId == userId)
            .Select(a => new
            {
                a.Id,
                a.ActivityTypeId,
                a.SchemaVersion,
                a.CurrentState,
                a.UpdatedOn,
                TypeName = a.ActivityType.Name
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

        // Each pin's workflow, read once for both lists: it says which states are finished (T203) and what each state is
        // called, so a badge names the state as the activity's own page does (T220).
        var workflows = await PinnedWorkflows.LoadAsync(
            _dbContext,
            activityStates.Select(a => (a.ActivityTypeId, a.SchemaVersion))
                .Concat(recent.Select(a => (a.ActivityTypeId, a.SchemaVersion))),
            cancellationToken);
        var finishedStates = workflows.ToDictionary(pair => pair.Key, pair => ActivityCompletion.FinishedStates(pair.Value));
        string StateLabel(int activityTypeId, int version, string state)
            => PinnedWorkflows.StateLabel(workflows[(activityTypeId, version)], state);

        var unfinished = activityStates
            .Where(a => !finishedStates[(a.ActivityTypeId, a.SchemaVersion)].Contains(a.CurrentState))
            .ToList();

        var inbox = unfinished
            .Where(a => a.CurrentState is "requested" or "accepted" or "declined" or "draft")
            .OrderByDescending(a => a.UpdatedOn)
            .Take(5)
            .Select(a => new ActivityInboxItem(
                a.Id, a.TypeName, a.CurrentState, StateLabel(a.ActivityTypeId, a.SchemaVersion, a.CurrentState), a.UpdatedOn))
            .ToList();

        var recentActivities = recent
            .Select(a => new RecentActivityItem(
                a.Id, a.TypeName, a.CurrentState, StateLabel(a.ActivityTypeId, a.SchemaVersion, a.CurrentState), a.CreatedOn))
            .ToList();

        var cutoff = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(14));
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Upcoming deadlines: scan DataJson for fields with a "due_date" key
        // This is done client-side since jsonb path queries vary by provider
        var deadlineCandidateIds = unfinished
            .Where(a => a.CurrentState != "cancelled")
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
