using Cronos;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Scheduling;

namespace Wombat.Application.Features.Scheduling.Queries.GetScheduledJobStatus;

public sealed class GetScheduledJobStatusQueryHandler : IRequestHandler<GetScheduledJobStatusQuery, IReadOnlyList<ScheduledJobDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetScheduledJobStatusQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ScheduledJobDto>> Handle(GetScheduledJobStatusQuery request, CancellationToken cancellationToken)
    {
        var definitions = await _dbContext.Set<ScheduledJobDefinition>()
            .OrderBy(d => d.Key)
            .ToListAsync(cancellationToken);

        var keys = definitions.Select(d => d.Key).ToList();

        var lastRuns = await _dbContext.Set<ScheduledJobRun>()
            .Where(r => keys.Contains(r.Key))
            .GroupBy(r => r.Key)
            .Select(g => g.OrderByDescending(r => r.StartedAt).First())
            .ToListAsync(cancellationToken);

        var lastRunByKey = lastRuns.ToDictionary(r => r.Key);

        return definitions.Select(d =>
        {
            lastRunByKey.TryGetValue(d.Key, out var lastRun);
            DateTime? nextRun = null;
            if (d.IsEnabled)
            {
                try
                {
                    var cron = CronExpression.Parse(d.CronExpression);
                    nextRun = cron.GetNextOccurrence(DateTime.UtcNow, inclusive: false);
                }
                catch
                {
                    // Invalid cron expression — leave nextRun null
                }
            }

            return new ScheduledJobDto(
                d.Id,
                d.Key,
                d.CronExpression,
                d.IsEnabled,
                d.Description,
                lastRun?.StartedAt,
                lastRun?.Status.ToString(),
                nextRun);
        }).ToList();
    }
}
