using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Wombat.Application.Common.Email.Templates;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Scheduling;
using Wombat.Domain.EntrustmentDecisions;
using Wombat.Infrastructure.Identity;

namespace Wombat.Infrastructure.Scheduling.Jobs;

public sealed class EntrustmentDecisionExpiryJob : IScheduledJob
{
    private const int ReminderWindowDays = 30;

    private readonly IServiceScopeFactory _scopeFactory;

    public EntrustmentDecisionExpiryJob(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public string Key => "entrustment-decision-expiry";
    public string CronExpression => "30 3 * * *";
    public string Description => "Expires entrustment decisions past their ExpiresOn date and sends 30-day expiry reminders (daily at 03:30 UTC).";

    public async Task ExecuteAsync(ScheduledJobContext context, CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();

        var today = DateOnly.FromDateTime(context.UtcNow);
        var reminderThreshold = today.AddDays(ReminderWindowDays);

        var expired = await dbContext.Set<EntrustmentDecision>()
            .Include(d => d.Epa)
            .Where(d => d.Status == EntrustmentDecisionStatus.Active
                && d.ExpiresOn.HasValue
                && d.ExpiresOn.Value < today)
            .ToListAsync(cancellationToken);

        foreach (var decision in expired)
        {
            decision.MarkExpired(context.UtcNow);
        }

        var expiringSoon = await dbContext.Set<EntrustmentDecision>()
            .Include(d => d.Epa)
            .Where(d => d.Status == EntrustmentDecisionStatus.Active
                && d.ExpiresOn.HasValue
                && d.ExpiresOn.Value >= today
                && d.ExpiresOn.Value <= reminderThreshold
                && (d.LastExpiryReminderSentOn == null || d.LastExpiryReminderSentOn.Value < today))
            .ToListAsync(cancellationToken);

        var traineeUserIds = expired.Select(d => d.TraineeUserId)
            .Concat(expiringSoon.Select(d => d.TraineeUserId))
            .Distinct()
            .ToList();

        var trainees = await dbContext.Set<WombatIdentityUser>()
            .Where(u => traineeUserIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, cancellationToken);

        var expiredSent = 0;
        foreach (var decision in expired)
        {
            if (!trainees.TryGetValue(decision.TraineeUserId, out var trainee) || string.IsNullOrWhiteSpace(trainee.Email))
            {
                continue;
            }

            var email = EntrustmentDecisionExpiredEmail.Build(
                trainee.Email,
                decision.Epa?.Code ?? string.Empty,
                decision.Epa?.Title ?? string.Empty,
                decision.ExpiresOn!.Value);

            await emailSender.SendAsync(email, cancellationToken);
            expiredSent++;
        }

        var reminderSent = 0;
        foreach (var decision in expiringSoon)
        {
            if (!trainees.TryGetValue(decision.TraineeUserId, out var trainee) || string.IsNullOrWhiteSpace(trainee.Email))
            {
                continue;
            }

            var daysRemaining = decision.ExpiresOn!.Value.DayNumber - today.DayNumber;
            var email = EntrustmentDecisionExpiringReminderEmail.Build(
                trainee.Email,
                decision.Epa?.Code ?? string.Empty,
                decision.Epa?.Title ?? string.Empty,
                decision.ExpiresOn.Value,
                daysRemaining);

            await emailSender.SendAsync(email, cancellationToken);
            decision.RecordExpiryReminderSent(today);
            reminderSent++;
        }

        if (expired.Count > 0 || expiringSoon.Count > 0)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        context.Logger.LogInformation(
            "EntrustmentDecisionExpiryJob: expired {ExpiredCount} decisions (sent {ExpiredEmailCount}), reminded {ReminderCount} expiring soon (sent {ReminderEmailCount}).",
            expired.Count, expiredSent, expiringSoon.Count, reminderSent);
    }
}
