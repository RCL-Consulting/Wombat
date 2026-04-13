using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Wombat.Application.Common.Email.Templates;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Scheduling;
using Wombat.Domain.Activities;
using Wombat.Infrastructure.Identity;

namespace Wombat.Infrastructure.Scheduling.Jobs;

public sealed class ActivityDraftNudgeJob : IScheduledJob
{
    private readonly IServiceScopeFactory _scopeFactory;

    public ActivityDraftNudgeJob(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public string Key => "activity-draft-nudge";
    public string CronExpression => "0 7 * * *";
    public string Description => "Reminds trainees about draft activities older than 14 days (daily at 07:00 UTC).";

    public async Task ExecuteAsync(ScheduledJobContext context, CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();

        var cutoff = context.UtcNow.AddDays(-14);

        var staleActivities = await dbContext.Set<Activity>()
            .Include(a => a.ActivityType)
            .Where(a => a.CurrentState == "draft" && a.UpdatedOn < cutoff)
            .Select(a => new
            {
                a.SubjectUserId,
                ActivityTypeName = a.ActivityType.Name,
                DaysOld = (int)(context.UtcNow - a.UpdatedOn).TotalDays
            })
            .ToListAsync(cancellationToken);

        if (staleActivities.Count == 0)
        {
            context.Logger.LogInformation("ActivityDraftNudgeJob: no stale drafts found.");
            return;
        }

        var grouped = staleActivities.GroupBy(a => a.SubjectUserId);

        foreach (var group in grouped)
        {
            var user = await dbContext.Set<WombatIdentityUser>()
                .Where(u => u.Id == group.Key)
                .Select(u => new { u.Email, u.FirstName })
                .FirstOrDefaultAsync(cancellationToken);

            if (user is null || string.IsNullOrWhiteSpace(user.Email))
                continue;

            var items = group.Select(a => (a.ActivityTypeName, a.DaysOld)).ToList();
            var email = DraftNudgeEmail.Build(user.Email, user.FirstName, items);
            await emailSender.SendAsync(email, cancellationToken);
        }

        context.Logger.LogInformation("ActivityDraftNudgeJob: sent nudges to {Count} trainees.", grouped.Count());
    }
}
