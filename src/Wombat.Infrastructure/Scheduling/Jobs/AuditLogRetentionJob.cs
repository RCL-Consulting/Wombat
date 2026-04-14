using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Scheduling;
using Wombat.Domain.Audit;

namespace Wombat.Infrastructure.Scheduling.Jobs;

/// <summary>
/// Archives AuditEntry rows older than 2 years into AuditEntryArchives.
/// Runs daily at 03:00 UTC. Processes in batches of 1000 to keep transactions small.
/// </summary>
public sealed class AuditLogRetentionJob : IScheduledJob
{
    private readonly IServiceScopeFactory _scopeFactory;

    public AuditLogRetentionJob(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public string Key => "audit-log-retention";
    public string CronExpression => "0 3 * * *";
    public string Description => "Archives audit log entries older than 2 years into the archive table (daily at 03:00 UTC).";

    public async Task ExecuteAsync(ScheduledJobContext context, CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        var cutoff = context.UtcNow.AddYears(-2);
        const int batchSize = 1000;
        int totalArchived = 0;

        while (true)
        {
            var batch = await dbContext.Set<AuditEntry>()
                .Where(e => e.OccurredAt < cutoff)
                .OrderBy(e => e.OccurredAt)
                .Take(batchSize)
                .ToListAsync(cancellationToken);

            if (batch.Count == 0) break;

            var archives = batch.Select(e => new AuditEntryArchive
            {
                Id = e.Id,
                OccurredAt = e.OccurredAt,
                ActorUserId = e.ActorUserId,
                ActorDisplay = e.ActorDisplay,
                ActorIpAddress = e.ActorIpAddress,
                ActorUserAgent = e.ActorUserAgent,
                Category = e.Category,
                Action = e.Action,
                SubjectType = e.SubjectType,
                SubjectId = e.SubjectId,
                InstitutionId = e.InstitutionId,
                SpecialityId = e.SpecialityId,
                SummaryJson = e.SummaryJson,
                Success = e.Success,
                ErrorMessage = e.ErrorMessage,
                ArchivedAt = context.UtcNow
            }).ToList();

            dbContext.Set<AuditEntryArchive>().AddRange(archives);
            dbContext.Set<AuditEntry>().RemoveRange(batch);
            await dbContext.SaveChangesAsync(cancellationToken);

            totalArchived += batch.Count;
            context.Logger.LogInformation(
                "AuditLogRetentionJob: archived {Count} entries (total so far: {Total}).",
                batch.Count, totalArchived);

            if (batch.Count < batchSize) break;
        }

        context.Logger.LogInformation(
            "AuditLogRetentionJob: complete. Total archived: {Total}.", totalArchived);
    }
}
