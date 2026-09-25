using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Wombat.Application.Common.Email.Templates;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Scheduling;
using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Workflow;

namespace Wombat.Infrastructure.Scheduling.Jobs;

/// <summary>
/// Nudges each assessor, daily, about the activities that have waited on them for more than five days: one email per
/// assessor listing them all.
/// </summary>
/// <remarks>
/// <para>
/// Whom the nudge does not write to (D50, T151) is <see cref="ReminderRecipientPolicy" />, shared with the other
/// periodic reminders since T240: not an id naming no account, not a deactivated account, not a user who opted out of
/// digest emails (the nudge is a digest), and not one with no address.
/// </para>
/// <para>
/// A nominee who has since lost the Assessor role or moved institution IS still nudged. The nominee gate (T102)
/// judges eligibility when the activity is handed to them, and lets them complete it afterwards; so this job
/// deliberately does not re-read <c>NomineeDirectory</c>, which would silence the one person able to act.
/// </para>
/// </remarks>
public sealed class AssessorPendingNudgeJob : IScheduledJob
{
    private readonly IServiceScopeFactory _scopeFactory;

    public AssessorPendingNudgeJob(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public string Key => "assessor-pending-nudge";
    public string CronExpression => "0 9 * * *";
    public string Description => "Nudges assessors about activities waiting for their assessment for more than 5 days (daily at 09:00 UTC).";

    public async Task ExecuteAsync(ScheduledJobContext context, CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();

        var cutoff = context.UtcNow.AddDays(-5);

        var activities = await dbContext.Set<Activity>()
            .Include(a => a.ActivityType)
            .ThenInclude(t => t.Versions)
            .Where(a => a.UpdatedOn < cutoff)
            .ToListAsync(cancellationToken);

        var pendingItems = new List<(string AssessorUserId, string ActivityTypeName, string TraineeUserId, int DaysWaiting)>();

        foreach (var activity in activities)
        {
            // The version the activity is pinned to, as the inbox reads it (T102): the nominee gate judged the
            // pinned version's field: rules, so a later version's rules must not decide whom this activity emails.
            var pinnedVersion = activity.ActivityType.Versions.SingleOrDefault(v => v.Version == activity.SchemaVersion);
            if (pinnedVersion is null || string.IsNullOrWhiteSpace(pinnedVersion.WorkflowJson))
                continue;

            Workflow workflow;
            try
            {
                workflow = WorkflowParser.Parse(pinnedVersion.WorkflowJson);
            }
            catch
            {
                continue;
            }

            var currentState = workflow.States.FirstOrDefault(s => s.Key == activity.CurrentState);
            if (currentState is null || currentState.Terminal)
                continue;

            var outgoingTransitions = workflow.Transitions
                .Where(t => t.From.Contains(activity.CurrentState))
                .ToList();

            var assessorFieldTransition = outgoingTransitions
                .FirstOrDefault(t => HasFieldUserActor(t.Actor));

            if (assessorFieldTransition is null)
                continue;

            var fieldName = GetFieldName(assessorFieldTransition.Actor);
            if (fieldName is null)
                continue;

            var assessorUserId = ExtractFieldValue(activity.DataJson, fieldName);
            if (string.IsNullOrWhiteSpace(assessorUserId))
                continue;

            var daysWaiting = (int)(context.UtcNow - activity.UpdatedOn).TotalDays;
            pendingItems.Add((assessorUserId, activity.ActivityType.Name, activity.SubjectUserId, daysWaiting));
        }

        var outcome = await NudgeAsync(dbContext, emailSender, pendingItems, cancellationToken);

        // One line per run, whatever happened, so a quiet day and a day where every nominee was skipped read apart. Each
        // count follows its label, so the line reads right at 1 as well as at 0 or 2.
        context.Logger.LogInformation(
            "AssessorPendingNudgeJob: assessors nudged {NudgedCount} (activities {NudgedActivityCount}); nominees " +
            "skipped: no such account {UnknownUserCount}, deactivated {DeactivatedCount}, opted out of digest emails " +
            "{OptedOutCount}, no email address {NoEmailCount}.",
            outcome.Nudged,
            outcome.NudgedActivities,
            outcome.Skipped[ReminderSkipReason.UnknownUser],
            outcome.Skipped[ReminderSkipReason.Deactivated],
            outcome.Skipped[ReminderSkipReason.OptedOut],
            outcome.Skipped[ReminderSkipReason.NoEmail]);
    }

    private static async Task<NudgeOutcome> NudgeAsync(
        IApplicationDbContext dbContext,
        IEmailSender emailSender,
        List<(string AssessorUserId, string ActivityTypeName, string TraineeUserId, int DaysWaiting)> pendingItems,
        CancellationToken cancellationToken)
    {
        var outcome = new NudgeOutcome();
        if (pendingItems.Count == 0)
            return outcome;

        var grouped = pendingItems.GroupBy(p => p.AssessorUserId, StringComparer.Ordinal).ToList();
        var userIds = grouped.Select(g => g.Key)
            .Concat(pendingItems.Select(p => p.TraineeUserId))
            .Distinct()
            .ToList();

        var users = await ReminderRecipientPolicy.LoadAsync(dbContext, userIds, cancellationToken);

        foreach (var group in grouped)
        {
            // The shared reminder policy (T240), and nothing more: see the class remarks on who is NOT skipped.
            var nominee = users.GetValueOrDefault(group.Key);
            if (ReminderRecipientPolicy.SkipReasonFor(nominee) is { } reason)
            {
                outcome.Skipped.Add(reason);
                continue;
            }

            var items = group.Select(p =>
            {
                var traineeName = users.TryGetValue(p.TraineeUserId, out var trainee)
                    ? $"{trainee.FirstName} {trainee.LastName}"
                    : "Unknown trainee";
                return (p.ActivityTypeName, TraineeName: traineeName, p.DaysWaiting);
            }).ToList();

            var email = AssessorPendingNudgeEmail.Build(nominee!.Email!, nominee.FirstName, items);
            await emailSender.SendAsync(email, cancellationToken);
            outcome.Nudged++;
            outcome.NudgedActivities += items.Count;
        }

        return outcome;
    }

    /// <summary>What a run did, for its one log line.</summary>
    private sealed class NudgeOutcome
    {
        public int Nudged { get; set; }

        public int NudgedActivities { get; set; }

        public SkipTally<ReminderSkipReason> Skipped { get; } = new();
    }

    private static bool HasFieldUserActor(ActorRule rule) => rule switch
    {
        FieldUserActorRule => true,
        CombinedActorRule combined => combined.Rules.Any(HasFieldUserActor),
        _ => false
    };

    private static string? GetFieldName(ActorRule rule) => rule switch
    {
        FieldUserActorRule field => field.Field,
        CombinedActorRule combined => combined.Rules.OfType<FieldUserActorRule>().FirstOrDefault()?.Field,
        _ => null
    };

    private static string? ExtractFieldValue(string dataJson, string fieldName)
    {
        try
        {
            using var doc = JsonDocument.Parse(dataJson);
            return doc.RootElement.TryGetProperty(fieldName, out var value) ? value.GetString() : null;
        }
        catch
        {
            return null;
        }
    }
}
