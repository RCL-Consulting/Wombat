using System.Security.Claims;
using MediatR;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Scheduling;

namespace Wombat.Application.Features.Scheduling.Commands.RunScheduledJobNow;

public sealed class RunScheduledJobNowCommandHandler : IRequestHandler<RunScheduledJobNowCommand, ScheduledJobRunOutcome>
{
    private readonly IScheduledJobRegistry _registry;
    private readonly IScheduledJobDispatcher _dispatcher;

    public RunScheduledJobNowCommandHandler(IScheduledJobRegistry registry, IScheduledJobDispatcher dispatcher)
    {
        _registry = registry;
        _dispatcher = dispatcher;
    }

    public async Task<ScheduledJobRunOutcome> Handle(RunScheduledJobNowCommand request, CancellationToken cancellationToken)
    {
        // The page is the Administrator's alone; the handler is the second lock (the build review's N-3).
        if (!request.Principal.IsAdministrator())
        {
            throw new UnauthorizedAccessException("Only a Wombat administrator may run a scheduled job.");
        }

        // Who ran it is the caller (T400, K6): a principal with no id is refused, never run as "unknown". Read before
        // anything is staged, as every refusal is (AuditPipelineBehavior, T201).
        var userId = request.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new UnauthorizedAccessException("Run now needs a signed-in person.");
        }

        // A key not in the registry is a failure, not a refusal: the page shows its fixed words, the cause to the log.
        var job = _registry.GetByKey(request.Key)
            ?? throw new InvalidOperationException($"Scheduled job '{request.Key}' not found in registry.");

        // The dispatcher refuses an already-running or a disabled job under the job's lock (ScheduledJobRefusal).
        return await _dispatcher.DispatchNowAsync(job, userId, cancellationToken);
    }
}
