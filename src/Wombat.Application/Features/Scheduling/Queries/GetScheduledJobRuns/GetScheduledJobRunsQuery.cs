using MediatR;

namespace Wombat.Application.Features.Scheduling.Queries.GetScheduledJobRuns;

/// <summary>
/// One page of Run history (T400, flow 18, T1).
/// </summary>
/// <param name="Key">One job by its whole key (the Job select cannot miss), or null for every job.</param>
/// <param name="Status">A run status by name (Running, Succeeded, Failed), or null for every status; an unknown name is ignored.</param>
/// <param name="From">The first South African day of runs to read (from 00:00 SAST), or null.</param>
/// <param name="To">The last South African day of runs to read (to the next day's 00:00 SAST), or null.</param>
/// <param name="Page">The page, from 1.</param>
/// <param name="PageSize">Runs a page: 50 by default (R3-R01).</param>
public sealed record GetScheduledJobRunsQuery(
    string? Key = null,
    string? Status = null,
    DateOnly? From = null,
    DateOnly? To = null,
    int Page = 1,
    int PageSize = GetScheduledJobRunsQuery.DefaultPageSize) : IRequest<ScheduledJobRunsPageDto>
{
    /// <summary>Fifty runs a page (R3-R01, M2).</summary>
    public const int DefaultPageSize = 50;
}
