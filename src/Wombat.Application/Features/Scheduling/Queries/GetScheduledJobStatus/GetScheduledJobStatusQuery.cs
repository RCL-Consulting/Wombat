using MediatR;

namespace Wombat.Application.Features.Scheduling.Queries.GetScheduledJobStatus;

public sealed record GetScheduledJobStatusQuery() : IRequest<IReadOnlyList<ScheduledJobDto>>;
