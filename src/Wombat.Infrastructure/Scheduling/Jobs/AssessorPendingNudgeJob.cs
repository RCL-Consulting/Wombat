using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Wombat.Application.Common.Email.Templates;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Scheduling;
using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Workflow;
using Wombat.Infrastructure.Identity;

namespace Wombat.Infrastructure.Scheduling.Jobs;

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
            outcome.Skipped(NudgeSkipReason.UnknownUser),
            outcome.Skipped(NudgeSkipReason.Deactivated),
            outcome.Skipped(NudgeSkipReason.OptedOut),
            outcome.Skipped(NudgeSkipReason.NoEmail));
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

        var users = await dbContext.Set<WombatIdentityUser>()
            .AsNoTracking()
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new NudgePerson(u.Id, u.Email, u.FirstName, u.LastName, u.LockoutEnd, u.OptOutOfDigestEmails))
            .ToDictionaryAsync(u => u.Id, StringComparer.Ordinal, cancellationToken);

        foreach (var group in grouped)
        {
            var nominee = users.GetValueOrDefault(group.Key);
            if (SkipReasonFor(nominee) is { } reason)
            {
                outcome.Skip(reason);
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

    /// <summary>
    /// Why a nominee is not written to (D50, T151), or null when they are. One reason each, the first that applies in
    /// this order, so an erased account (deactivated, opted out and without an email) is counted once, as deactivated.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A deactivated account (<see cref="UserDeactivation" />: an administrator's lock or an erasure) is not written to,
    /// and neither is a user who opted out of digest emails: the nudge is a digest. A brute-force lockout is not a
    /// deactivation and lifts itself, so that nominee is still nudged.
    /// </para>
    /// <para>
    /// A nominee who has since lost the Assessor role or moved institution IS still nudged. The nominee gate (T102)
    /// judges eligibility when the activity is handed to them, and lets them complete it afterwards; so this job
    /// deliberately does not re-read <c>NomineeDirectory</c>, which would silence the one person able to act.
    /// </para>
    /// </remarks>
    internal static NudgeSkipReason? SkipReasonFor(NudgePerson? nominee) => nominee switch
    {
        null => NudgeSkipReason.UnknownUser,
        _ when UserDeactivation.IsDeactivated(nominee.LockoutEnd) => NudgeSkipReason.Deactivated,
        { OptOutOfDigestEmails: true } => NudgeSkipReason.OptedOut,
        _ when string.IsNullOrWhiteSpace(nominee.Email) => NudgeSkipReason.NoEmail,
        _ => null
    };

    internal sealed record NudgePerson(
        string Id,
        string? Email,
        string FirstName,
        string LastName,
        DateTimeOffset? LockoutEnd,
        bool OptOutOfDigestEmails);

    internal enum NudgeSkipReason
    {
        UnknownUser,
        Deactivated,
        OptedOut,
        NoEmail
    }

    private sealed class NudgeOutcome
    {
        private readonly Dictionary<NudgeSkipReason, int> _skipped = [];

        public int Nudged { get; set; }

        public int NudgedActivities { get; set; }

        public void Skip(NudgeSkipReason reason) => _skipped[reason] = Skipped(reason) + 1;

        public int Skipped(NudgeSkipReason reason) => _skipped.GetValueOrDefault(reason);
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
