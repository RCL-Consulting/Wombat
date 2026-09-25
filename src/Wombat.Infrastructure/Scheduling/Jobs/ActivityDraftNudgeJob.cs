using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Wombat.Application.Common.Email.Templates;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Scheduling;
using Wombat.Domain.Activities;

namespace Wombat.Infrastructure.Scheduling.Jobs;

/// <summary>
/// Reminds each trainee, daily, of their drafts untouched for 14 days: one email per trainee listing them all.
/// </summary>
/// <remarks>
/// <para>
/// Written to each draft's subject, through <see cref="ReminderRecipientPolicy" /> (T240). The reminder is a periodic,
/// unsolicited summary, so it counts as a digest: a trainee who opted out of digest emails is not sent it, and neither is
/// an account an administrator locked.
/// </para>
/// <para>
/// An erased trainee is never reached at all. Erasure keeps their drafts on the record but writes a pseudonym into each
/// one's subject (<see cref="Wombat.Infrastructure.DataRights.ErasureExecutor" />), and a pseudonym names no account, so
/// those drafts are counted under "no such account", not "deactivated".
/// </para>
/// </remarks>
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
            .AsNoTracking()
            .Where(a => a.CurrentState == "draft" && a.UpdatedOn < cutoff)
            .Select(a => new
            {
                a.SubjectUserId,
                ActivityTypeName = a.ActivityType.Name,
                DaysOld = (int)(context.UtcNow - a.UpdatedOn).TotalDays
            })
            .ToListAsync(cancellationToken);

        var bySubject = staleActivities.GroupBy(a => a.SubjectUserId, StringComparer.Ordinal).ToList();
        var recipients = await ReminderRecipientPolicy.LoadAsync(dbContext, bySubject.Select(g => g.Key), cancellationToken);

        var reminded = 0;
        var remindedDrafts = 0;
        var skipped = new SkipTally<ReminderSkipReason>();

        foreach (var drafts in bySubject)
        {
            var recipient = recipients.GetValueOrDefault(drafts.Key);
            if (ReminderRecipientPolicy.SkipReasonFor(recipient) is { } reason)
            {
                skipped.Add(reason);
                continue;
            }

            var items = drafts.Select(a => (a.ActivityTypeName, a.DaysOld)).ToList();
            var email = DraftNudgeEmail.Build(recipient!.Email!, recipient.FirstName, items);
            await emailSender.SendAsync(email, cancellationToken);
            reminded++;
            remindedDrafts += items.Count;
        }

        // One line per run, whatever happened, as the assessor nudge logs (T151): a quiet day reads apart from a day on
        // which every trainee was skipped.
        context.Logger.LogInformation(
            "ActivityDraftNudgeJob: trainees reminded {RemindedCount} (drafts {RemindedDraftCount}); trainees skipped: " +
            "no such account {UnknownUserCount}, deactivated {DeactivatedCount}, opted out of digest emails " +
            "{OptedOutCount}, no email address {NoEmailCount}.",
            reminded,
            remindedDrafts,
            skipped[ReminderSkipReason.UnknownUser],
            skipped[ReminderSkipReason.Deactivated],
            skipped[ReminderSkipReason.OptedOut],
            skipped[ReminderSkipReason.NoEmail]);
    }
}
