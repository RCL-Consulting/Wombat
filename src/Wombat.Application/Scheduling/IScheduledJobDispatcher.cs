namespace Wombat.Application.Scheduling;

public interface IScheduledJobDispatcher
{
    Task DispatchNowAsync(IScheduledJob job, string? triggeredByUserId, CancellationToken cancellationToken);
}
