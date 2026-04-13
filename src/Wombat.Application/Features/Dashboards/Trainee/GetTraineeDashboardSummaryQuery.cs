using System.Security.Claims;
using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Activities;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.Dashboards.Trainee;

public sealed record GetTraineeDashboardSummaryQuery(ClaimsPrincipal Principal) : IRequest<TraineeDashboardSummaryDto>;

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
            return new TraineeDashboardSummaryDto([], [], [], [], IsPendingTrainee: true);
        }

        var profile = await _dbContext.Set<TraineeProfile>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId && p.IsActive, cancellationToken);

        var curriculumProgress = new List<CurriculumProgressItem>();
        if (profile is not null)
        {
            curriculumProgress = await _dbContext.Set<CurriculumItemProgress>()
                .AsNoTracking()
                .Where(p => p.TraineeUserId == userId &&
                            p.CurriculumItem.CurriculumId == profile.CurriculumId)
                .Select(p => new CurriculumProgressItem(
                    p.CurriculumItem.Epa.Title,
                    p.CountsSoFar,
                    p.CurriculumItem.RequiredCount,
                    p.CountsSoFar >= p.CurriculumItem.RequiredCount))
                .ToListAsync(cancellationToken);
        }

        var inbox = await _dbContext.Set<Activity>()
            .AsNoTracking()
            .Include(a => a.ActivityType)
            .Where(a => a.SubjectUserId == userId &&
                        (a.CurrentState == "requested" || a.CurrentState == "accepted" ||
                         a.CurrentState == "declined" || a.CurrentState == "draft"))
            .OrderByDescending(a => a.UpdatedOn)
            .Take(5)
            .Select(a => new ActivityInboxItem(
                a.Id, a.ActivityType.Name, a.CurrentState, a.UpdatedOn))
            .ToListAsync(cancellationToken);

        var recentActivities = await _dbContext.Set<Activity>()
            .AsNoTracking()
            .Include(a => a.ActivityType)
            .Where(a => a.SubjectUserId == userId)
            .OrderByDescending(a => a.CreatedOn)
            .Take(5)
            .Select(a => new RecentActivityItem(
                a.Id, a.ActivityType.Name, a.CurrentState, a.CreatedOn))
            .ToListAsync(cancellationToken);

        var cutoff = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(14));
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Upcoming deadlines: scan DataJson for fields with a "due_date" key
        // This is done client-side since jsonb path queries vary by provider
        var candidateActivities = await _dbContext.Set<Activity>()
            .AsNoTracking()
            .Include(a => a.ActivityType)
            .Where(a => a.SubjectUserId == userId &&
                        a.CurrentState != "completed" && a.CurrentState != "cancelled")
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
            curriculumProgress,
            inbox,
            recentActivities,
            upcomingDeadlines.OrderBy(d => d.DueDate).Take(5).ToList(),
            IsPendingTrainee: false);
    }
}
