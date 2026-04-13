using Microsoft.Extensions.Logging;
using Wombat.Application.Scheduling;

namespace Wombat.Infrastructure.Scheduling.Jobs;

public sealed class AuditLogRetentionJob : IScheduledJob
{
    public string Key => "audit-log-retention";
    public string CronExpression => "0 3 * * *";
    public string Description => "Archives audit log entries older than the retention window (daily at 03:00 UTC). Placeholder until T025 implements the audit log.";

    public Task ExecuteAsync(ScheduledJobContext context, CancellationToken cancellationToken)
    {
        context.Logger.LogInformation("AuditLogRetentionJob: no-op — audit log not yet implemented (see T025).");
        return Task.CompletedTask;
    }
}
