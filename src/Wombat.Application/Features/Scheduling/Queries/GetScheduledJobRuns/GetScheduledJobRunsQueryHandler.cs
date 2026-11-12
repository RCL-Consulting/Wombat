using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.DataRights.Queries;
using Wombat.Domain.Curricula;
using Wombat.Domain.Scheduling;

namespace Wombat.Application.Features.Scheduling.Queries.GetScheduledJobRuns;

/// <summary>
/// Run history (T400, flow 18; R3-R01–R09): a page of runs, newest first, with a total in place of the 200 it once cut at
/// (R3-R06: about 2,800 held at once); From and To as South African days (K7); Ran by by name (M3).
/// </summary>
public sealed class GetScheduledJobRunsQueryHandler : IRequestHandler<GetScheduledJobRunsQuery, ScheduledJobRunsPageDto>
{
    /// <summary>The sizes the pager offers; any other is read as the default.</summary>
    private static readonly int[] PageSizes = [10, 20, 50, 100];

    private readonly IApplicationDbContext _dbContext;
    private readonly IUserAdministrationService _users;

    public GetScheduledJobRunsQueryHandler(IApplicationDbContext dbContext, IUserAdministrationService users)
    {
        _dbContext = dbContext;
        _users = users;
    }

    public async Task<ScheduledJobRunsPageDto> Handle(GetScheduledJobRunsQuery request, CancellationToken cancellationToken)
    {
        var runs = _dbContext.Set<ScheduledJobRun>().AsNoTracking();
        var allRunsCount = await runs.CountAsync(cancellationToken);

        var query = runs;
        if (!string.IsNullOrWhiteSpace(request.Key))
        {
            // The whole key, from the select: no partial match (T1).
            query = query.Where(r => r.Key == request.Key);
        }

        if (StatusOf(request.Status) is { } status)
        {
            query = query.Where(r => r.Status == status);
        }

        // A South African day runs from its 00:00 SAST to the next day's (K7): 2026-11-10 is [2026-11-09 22:00 UTC,
        // 2026-11-10 22:00 UTC). Until T400 these were UTC days, two hours off. The calendar's first and last days bound
        // nothing (the build review's N-1): every run is after the one and before the other, and their edges overflow.
        if (request.From is { } from && from > DateOnly.MinValue)
        {
            var fromUtc = ProgrammeCalendar.EndOf(from.AddDays(-1));
            query = query.Where(r => r.StartedAt >= fromUtc);
        }

        if (request.To is { } to && to < DateOnly.MaxValue)
        {
            var toUtc = ProgrammeCalendar.EndOf(to);
            query = query.Where(r => r.StartedAt < toUtc);
        }

        var total = await query.CountAsync(cancellationToken);
        var pageSize = PageSizes.Contains(request.PageSize) ? request.PageSize : GetScheduledJobRunsQuery.DefaultPageSize;
        var lastPage = Math.Max(1, (int)Math.Ceiling(total / (double)pageSize));
        var page = Math.Clamp(request.Page, 1, lastPage);

        var rows = await query
            .OrderByDescending(r => r.StartedAt)
            .ThenByDescending(r => r.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new { r.Id, r.Key, r.StartedAt, r.FinishedAt, r.Status, r.ErrorMessage, r.TriggeredBy })
            .ToListAsync(cancellationToken);

        // Ran by: the live name, an erased person's pseudonym (M3); the scheduler's runs carry no id.
        var ids = rows.Select(r => r.TriggeredBy).OfType<string>().Distinct(StringComparer.Ordinal).ToList();
        var names = await DataRightsPeople.NamesOfAsync(_dbContext, _users, ids, cancellationToken);

        var keys = await _dbContext.Set<ScheduledJobDefinition>()
            .AsNoTracking()
            .OrderBy(d => d.Key)
            .Select(d => d.Key)
            .ToListAsync(cancellationToken);

        var items = rows
            .Select(r => new ScheduledJobRunDto(
                r.Id,
                r.Key,
                r.StartedAt,
                r.FinishedAt,
                r.Status.ToString(),
                r.ErrorMessage,
                r.TriggeredBy is { } id ? names[id] : null))
            .ToList();

        return new ScheduledJobRunsPageDto(items, page, pageSize, total, allRunsCount, keys);
    }

    /// <summary>A status by its name exactly; anything else (a number, an unknown word) filters nothing.</summary>
    private static ScheduledJobRunStatus? StatusOf(string? name)
        => Enum.GetNames<ScheduledJobRunStatus>().Contains(name, StringComparer.Ordinal)
            ? Enum.Parse<ScheduledJobRunStatus>(name!)
            : null;
}
