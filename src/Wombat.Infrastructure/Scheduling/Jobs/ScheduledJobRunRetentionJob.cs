using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Scheduling;
using Wombat.Domain.Email;
using Wombat.Domain.Scheduling;

namespace Wombat.Infrastructure.Scheduling.Jobs;

public sealed class ScheduledJobRunRetentionJob : IScheduledJob
{
    private readonly IServiceScopeFactory _scopeFactory;

    public ScheduledJobRunRetentionJob(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    /// <summary>How long a run record and a mail outcome are kept (Run history's "kept for 90 days.", K18; Q18).</summary>
    public const int RetentionDays = 90;

    public string Key => "scheduled-job-run-retention";
    public string CronExpression => "0 4 * * *";
    public string Description => "Deletes job run records and mail outcomes older than 90 days.";

    /// <summary>
    /// Deletes the run records and the mail outcomes (Q18) that started or happened more than 90 days ago. Home's Mail line
    /// reads "since ‹time›" from the same 90 days, so the table holds what Home says it does (T400, flow 18). Each table is
    /// deleted from by one statement, with no row loaded (the build review's N-7): 90 days of outcomes is a few thousand
    /// rows a day at most. The two are independent; a failure between them leaves the outcomes for the next day's run.
    /// </summary>
    public async Task ExecuteAsync(ScheduledJobContext context, CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        var cutoff = context.UtcNow.AddDays(-RetentionDays);

        var runCount = await dbContext.Set<ScheduledJobRun>()
            .Where(r => r.StartedAt < cutoff)
            .ExecuteDeleteAsync(cancellationToken);

        // By OccurredAt, the indexed column (T400, § 0's migration).
        var outcomeCount = await dbContext.Set<EmailOutcome>()
            .Where(outcome => outcome.OccurredAt < cutoff)
            .ExecuteDeleteAsync(cancellationToken);

        if (runCount == 0 && outcomeCount == 0)
        {
            context.Logger.LogInformation("ScheduledJobRunRetentionJob: no old runs or mail outcomes to delete.");
            return;
        }

        context.Logger.LogInformation(
            "ScheduledJobRunRetentionJob: deleted {RunCount} old job run records and {OutcomeCount} old mail outcomes.",
            runCount,
            outcomeCount);
    }
}
