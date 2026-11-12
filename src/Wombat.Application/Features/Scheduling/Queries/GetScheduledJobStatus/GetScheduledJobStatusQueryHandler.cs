using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.DataRights.Queries;
using Wombat.Application.Scheduling;
using Wombat.Domain.Scheduling;

namespace Wombat.Application.Features.Scheduling.Queries.GetScheduledJobStatus;

/// <summary>
/// Scheduled jobs (T400, flow 18; R3-J01–J10): each job by key, with its last run (status, start, finish, the first line of
/// a failed run's error, and who ran it) and its next run by the host's own rule (K5).
/// </summary>
public sealed class GetScheduledJobStatusQueryHandler : IRequestHandler<GetScheduledJobStatusQuery, IReadOnlyList<ScheduledJobDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserAdministrationService _users;
    private readonly TimeProvider _timeProvider;

    public GetScheduledJobStatusQueryHandler(
        IApplicationDbContext dbContext, IUserAdministrationService users, TimeProvider timeProvider)
    {
        _dbContext = dbContext;
        _users = users;
        _timeProvider = timeProvider;
    }

    public async Task<IReadOnlyList<ScheduledJobDto>> Handle(GetScheduledJobStatusQuery request, CancellationToken cancellationToken)
    {
        var now = _timeProvider.GetUtcNow().UtcDateTime;

        var definitions = await _dbContext.Set<ScheduledJobDefinition>()
            .AsNoTracking()
            .OrderBy(d => d.Key)
            .ToListAsync(cancellationToken);

        var keys = definitions.Select(d => d.Key).ToList();

        var lastRuns = await _dbContext.Set<ScheduledJobRun>()
            .AsNoTracking()
            .Where(r => keys.Contains(r.Key))
            .GroupBy(r => r.Key)
            .Select(g => g.OrderByDescending(r => r.StartedAt).First())
            .ToListAsync(cancellationToken);

        var lastRunByKey = lastRuns.ToDictionary(r => r.Key);

        var ids = lastRuns.Select(r => r.TriggeredBy).OfType<string>().Distinct(StringComparer.Ordinal).ToList();
        var names = await DataRightsPeople.NamesOfAsync(_dbContext, _users, ids, cancellationToken);

        return definitions.Select(d =>
        {
            lastRunByKey.TryGetValue(d.Key, out var lastRun);
            var readable = JobSchedule.TryParse(d.CronExpression, out var cron);

            // Next run by the host's rule (K5): the cron's next occurrence after the last run's start, not after now, so a
            // job disabled past its time and enabled again is due at once, and the page says so. None while disabled.
            DateTime? nextRun = null;
            var isDue = false;
            if (d.IsEnabled && readable)
            {
                nextRun = JobSchedule.NextAfter(cron!, lastRun?.StartedAt, now);
                isDue = nextRun is { } next && next <= now;
            }

            return new ScheduledJobDto(
                d.Id,
                d.Key,
                d.CronExpression,
                d.IsEnabled,
                d.Description,
                lastRun?.StartedAt,
                lastRun?.Status.ToString(),
                lastRun?.FinishedAt,
                lastRun?.Status == ScheduledJobRunStatus.Failed ? FirstLine(lastRun.ErrorMessage) : null,
                lastRun?.TriggeredBy is { } id ? names[id] : null,
                nextRun,
                isDue,
                readable);
        }).ToList();
    }

    /// <summary>The first line of an error as stored, trimmed; null for none.</summary>
    private static string? FirstLine(string? error)
    {
        if (string.IsNullOrWhiteSpace(error))
        {
            return null;
        }

        var trimmed = error.Trim();
        var end = trimmed.IndexOfAny(['\r', '\n']);
        return end < 0 ? trimmed : trimmed[..end].TrimEnd();
    }
}
