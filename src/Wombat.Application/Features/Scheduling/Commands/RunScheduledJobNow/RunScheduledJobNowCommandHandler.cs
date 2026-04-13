using MediatR;
using Wombat.Application.Scheduling;

namespace Wombat.Application.Features.Scheduling.Commands.RunScheduledJobNow;

public sealed class RunScheduledJobNowCommandHandler : IRequestHandler<RunScheduledJobNowCommand>
{
    private readonly IScheduledJobRegistry _registry;
    private readonly IScheduledJobDispatcher _dispatcher;

    public RunScheduledJobNowCommandHandler(IScheduledJobRegistry registry, IScheduledJobDispatcher dispatcher)
    {
        _registry = registry;
        _dispatcher = dispatcher;
    }

    public async Task Handle(RunScheduledJobNowCommand request, CancellationToken cancellationToken)
    {
        var job = _registry.GetByKey(request.Key)
            ?? throw new InvalidOperationException($"Scheduled job '{request.Key}' not found in registry.");

        await _dispatcher.DispatchNowAsync(job, request.TriggeredByUserId, cancellationToken);
    }
}
