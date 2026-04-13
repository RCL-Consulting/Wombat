using MediatR;

namespace Wombat.Application.Features.Scheduling.Queries.GetScheduledJobRuns;

public sealed record GetScheduledJobRunsQuery(
    string? Key = null,
    string? Status = null,
    DateOnly? From = null,
    DateOnly? To = null) : IRequest<IReadOnlyList<ScheduledJobRunDto>>;
