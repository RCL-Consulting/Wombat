using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Scheduling;

namespace Wombat.Application.Features.Scheduling.Queries.GetScheduledJobRuns;

public sealed class GetScheduledJobRunsQueryHandler : IRequestHandler<GetScheduledJobRunsQuery, IReadOnlyList<ScheduledJobRunDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetScheduledJobRunsQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ScheduledJobRunDto>> Handle(GetScheduledJobRunsQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Set<ScheduledJobRun>().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Key))
        {
            query = query.Where(r => r.Key == request.Key);
        }

        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<ScheduledJobRunStatus>(request.Status, out var status))
        {
            query = query.Where(r => r.Status == status);
        }

        if (request.From is not null)
        {
            var fromDate = request.From.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            query = query.Where(r => r.StartedAt >= fromDate);
        }

        if (request.To is not null)
        {
            var toDate = request.To.Value.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            query = query.Where(r => r.StartedAt < toDate);
        }

        return await query
            .OrderByDescending(r => r.StartedAt)
            .Take(200)
            .Select(r => new ScheduledJobRunDto(
                r.Id,
                r.Key,
                r.StartedAt,
                r.FinishedAt,
                r.Status.ToString(),
                r.ErrorMessage,
                r.TriggeredBy))
            .ToListAsync(cancellationToken);
    }
}
