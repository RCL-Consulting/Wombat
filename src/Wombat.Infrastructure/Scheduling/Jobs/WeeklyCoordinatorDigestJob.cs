using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Wombat.Application.Common.Email.Templates;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Application.Scheduling;
using Wombat.Domain.Activities;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Identity;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Infrastructure.Identity;

namespace Wombat.Infrastructure.Scheduling.Jobs;

/// <summary>
/// Mails each Coordinator, on Monday morning, the trainees at their institution who have logged nothing in 30 days, the
/// feedback campaigns there waiting on a review, and the committee reviews there scheduled this week.
/// </summary>
/// <remarks>
/// <para>
/// <b>The boundary is the recipient, not a request.</b> A push has no request principal, so no read gate runs on the way
/// out; until T117 the job built one body over every institution and mailed it to every coordinator in the country. Each
/// recipient is now judged as they would be signed in: their principal is built by the app's own claims factory
/// (<see cref="IUserClaimsPrincipalFactory{TUser}" />, <see cref="WombatUserClaimsPrincipalFactory" />) from their stored
/// roles, institution and speciality/sub-speciality scopes, and each list is narrowed by the rule the page showing the
/// same rows applies to that principal. No rule is restated here:
/// </para>
/// <list type="bullet">
/// <item>trainees: those whose record the recipient may read (<see cref="TraineeScopeResolver.ReadableAsync" />, T113),
/// keyed by where each trains (<c>TraineeProfile.InstitutionId</c> on the preferred profile);</item>
/// <item>feedback campaigns: those the recipient runs (<see cref="MsfCampaignRules.WhereRunBy" />, the campaign list's
/// rule, which also keeps a recipient off campaigns about themselves);</item>
/// <item>committee reviews: those the recipient may open (<see cref="CommitteeReviewReadAccess.MayRead" />,
/// the review page's ladder), on a panel run by their institution (<c>DecisionPanel.InstitutionId</c>).</item>
/// </list>
/// <para>
/// Every list is then held to the recipient's own institution, whatever wider reach another role lends them: this is an
/// institution's digest, so an Administrator who also coordinates is mailed their institution's roster, not the
/// country's. A Coordinator's reach under those rules is the whole institution (T182: the Coordinator arm of
/// <see cref="TraineeScopeResolver.IsAdministeredOrCoordinatedBy" /> does not read speciality claims), so a coordinator's
/// speciality or sub-speciality scopes neither narrow nor widen what they are sent; if that rule ever narrows, the digest
/// follows it.
/// </para>
/// <para>
/// Some coordinators are sent nothing, counted by reason in the run's one log line. First, whoever the shared reminder
/// policy skips (<see cref="ReminderRecipientPolicy" />, T240): a deactivated account, one who opted out of digest emails
/// (this is the email that flag is named after), and one with no address. Then two of the digest's own: one with no
/// institution, who oversees nobody, rather than the national roster the job used to send; and one who holds Trainee
/// (<see cref="TraineeScopeResolver.ActsAsTrainee" />, T185), who reads no peer's record, and whose own is on their own
/// pages. An empty digest to either would say "no items requiring attention", which is not what the job knows.
/// </para>
/// <para>
/// The activity table is read directly, outside <c>ActivityReadScope.WhereReadableBy</c> (an Application extension over
/// a principal's stamps), and only to learn which trainees filed anything in 30 days. No activity reaches a mail; what a
/// recipient is told is decided by the roster above, so the per-recipient boundary is the roster, not a per-activity gate.
/// </para>
/// </remarks>
public sealed class WeeklyCoordinatorDigestJob : IScheduledJob
{
    private readonly IServiceScopeFactory _scopeFactory;

    public WeeklyCoordinatorDigestJob(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public string Key => "weekly-coordinator-digest";
    public string CronExpression => "0 8 * * 1";
    public string Description => "Sends weekly digest emails to Coordinators on Mondays at 08:00 UTC.";

    public async Task ExecuteAsync(ScheduledJobContext context, CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<WombatIdentityUser>>();
        var claimsFactory = scope.ServiceProvider.GetRequiredService<IUserClaimsPrincipalFactory<WombatIdentityUser>>();

        var outcome = new DigestOutcome();
        var coordinators = await userManager.GetUsersInRoleAsync(WombatRoles.Coordinator);

        if (coordinators.Count > 0)
        {
            var facts = await ReadFactsAsync(dbContext, userManager, context.UtcNow, cancellationToken);

            foreach (var coordinator in coordinators)
            {
                // The account first, so a locked or opted-out coordinator's principal is never even built.
                if (ReminderRecipientPolicy.SkipReasonFor(ReminderRecipientPolicy.From(coordinator)) is { } accountReason)
                {
                    outcome.SkippedAccounts.Add(accountReason);
                    continue;
                }

                var recipient = await claimsFactory.CreateAsync(coordinator);
                if (SkipReasonFor(recipient) is { } reason)
                {
                    outcome.SkippedRecipients.Add(reason);
                    continue;
                }

                var institutionId = recipient.GetInstitutionId()!.Value;
                var digest = await DigestForAsync(dbContext, facts, recipient, institutionId, cancellationToken);

                var email = CoordinatorDigestEmail.Build(
                    coordinator.Email!,
                    coordinator.FirstName,
                    digest.InactiveTrainees,
                    digest.MsfCampaignsNeedingReview,
                    digest.CommitteeReviewsThisWeek);

                await emailSender.SendAsync(email, cancellationToken);
                outcome.Sent++;
            }
        }

        // One line per run, whatever happened, as the assessor nudge logs (T151). The reasons are in the order asked.
        context.Logger.LogInformation(
            "WeeklyCoordinatorDigestJob: digests sent {SentCount}; coordinators skipped: deactivated {DeactivatedCount}, " +
            "opted out of digest emails {OptedOutCount}, no email address {NoEmailCount}, holds Trainee " +
            "{HoldsTraineeCount}, no institution {NoInstitutionCount}.",
            outcome.Sent,
            outcome.SkippedAccounts[ReminderSkipReason.Deactivated],
            outcome.SkippedAccounts[ReminderSkipReason.OptedOut],
            outcome.SkippedAccounts[ReminderSkipReason.NoEmail],
            outcome.SkippedRecipients[DigestSkipReason.HoldsTrainee],
            outcome.SkippedRecipients[DigestSkipReason.NoInstitution]);
    }

    /// <summary>
    /// Why a coordinator the reminder policy lets through is still sent no digest, or null when they are sent one. One
    /// reason each, the first that applies in this order. Asked of the principal the app's claims factory builds for
    /// them, so it sees the roles and institution a sign-in would.
    /// </summary>
    internal static DigestSkipReason? SkipReasonFor(ClaimsPrincipal recipient)
    {
        if (TraineeScopeResolver.ActsAsTrainee(recipient))
        {
            return DigestSkipReason.HoldsTrainee;
        }

        return recipient.GetInstitutionId() is null ? DigestSkipReason.NoInstitution : null;
    }

    /// <summary>
    /// What is read once per run, for every institution. None of it reaches a mail until <see cref="DigestForAsync" />
    /// narrows it to one recipient.
    /// </summary>
    private static async Task<DigestFacts> ReadFactsAsync(
        IApplicationDbContext dbContext,
        UserManager<WombatIdentityUser> userManager,
        DateTime utcNow,
        CancellationToken cancellationToken)
    {
        var inactivityCutoff = utcNow.AddDays(-30);
        var weekStart = DateOnly.FromDateTime(utcNow);
        var weekEnd = weekStart.AddDays(7);

        var trainees = await userManager.GetUsersInRoleAsync(WombatRoles.Trainee);
        var traineeNames = trainees.ToDictionary(
            trainee => trainee.Id,
            trainee => $"{trainee.FirstName} {trainee.LastName}",
            StringComparer.Ordinal);
        var traineeIds = traineeNames.Keys.ToArray();

        // Whether each trainee filed anything, and nothing else about the activities: see the class remarks.
        var activeTraineeIds = await dbContext.Set<Activity>()
            .AsNoTracking()
            .Where(activity => activity.CreatedOn >= inactivityCutoff && traineeIds.Contains(activity.SubjectUserId))
            .Select(activity => activity.SubjectUserId)
            .Distinct()
            .ToListAsync(cancellationToken);

        // Panel and members loaded, as the review read ladder requires.
        var reviewsThisWeek = await dbContext.Set<CommitteeReview>()
            .AsNoTracking()
            .Include(review => review.Panel)
                .ThenInclude(panel => panel.Members)
            .Where(review => review.ScheduledOn >= weekStart && review.ScheduledOn <= weekEnd &&
                             review.State == CommitteeReviewState.Scheduled)
            .ToListAsync(cancellationToken);

        var reviewTraineeIds = reviewsThisWeek.Select(review => review.TraineeUserId).Distinct().ToArray();
        var reviewTraineeNames = await dbContext.Set<WombatIdentityUser>()
            .AsNoTracking()
            .Where(user => reviewTraineeIds.Contains(user.Id))
            .ToDictionaryAsync(user => user.Id, user => $"{user.FirstName} {user.LastName}", cancellationToken);

        return new DigestFacts(
            traineeNames,
            activeTraineeIds.ToHashSet(StringComparer.Ordinal),
            reviewsThisWeek,
            reviewTraineeNames);
    }

    /// <summary>
    /// One recipient's three lists: the per-recipient boundary the class remarks describe. Every row is admitted by the
    /// rule its own page applies to <paramref name="recipient" />, and is at <paramref name="institutionId" />, theirs.
    /// </summary>
    private static async Task<Digest> DigestForAsync(
        IApplicationDbContext dbContext,
        DigestFacts facts,
        ClaimsPrincipal recipient,
        int institutionId,
        CancellationToken cancellationToken)
    {
        // The trainees this recipient may read who train at their institution: T113's ladder, then the digest's own
        // institution bound, which is a narrowing only.
        var readable = await TraineeScopeResolver.ReadableAsync(dbContext, recipient, cancellationToken);
        var roster = readable
            .Where(entry => entry.Value.InstitutionId == institutionId)
            .Select(entry => entry.Key)
            .ToArray();

        var inactiveTrainees = roster
            .Where(userId => !facts.ActiveTraineeIds.Contains(userId))
            .Select(userId => facts.TraineeNames.GetValueOrDefault(userId))
            .OfType<string>()
            .Order(StringComparer.Ordinal)
            .ToList();

        var msfCampaignsNeedingReview = await dbContext.Set<MsfCampaign>()
            .AsNoTracking()
            .Where(campaign => campaign.State == MsfCampaignState.UnderReview)
            .WhereRunBy(dbContext, recipient)
            .Where(campaign => roster.Contains(campaign.SubjectUserId))
            .OrderBy(campaign => campaign.Id)
            .Select(campaign => $"{campaign.Template.Name} (campaign #{campaign.Id})")
            .ToListAsync(cancellationToken);

        var committeeReviewsThisWeek = facts.ReviewsThisWeek
            .Where(review => review.Panel.InstitutionId == institutionId &&
                             CommitteeReviewReadAccess.MayRead(recipient, review))
            .OrderBy(review => review.ScheduledOn)
            .ThenBy(review => review.Id)
            .Select(review =>
            {
                var name = facts.ReviewTraineeNames.GetValueOrDefault(review.TraineeUserId) ?? "Unknown trainee";
                return $"{name} on {review.ScheduledOn:yyyy-MM-dd}";
            })
            .ToList();

        return new Digest(inactiveTrainees, msfCampaignsNeedingReview, committeeReviewsThisWeek);
    }

    /// <summary>The digest's own reasons, asked after <see cref="ReminderSkipReason" />'s.</summary>
    internal enum DigestSkipReason
    {
        HoldsTrainee,
        NoInstitution
    }

    private sealed record DigestFacts(
        IReadOnlyDictionary<string, string> TraineeNames,
        IReadOnlySet<string> ActiveTraineeIds,
        IReadOnlyList<CommitteeReview> ReviewsThisWeek,
        IReadOnlyDictionary<string, string> ReviewTraineeNames);

    private sealed record Digest(
        IReadOnlyList<string> InactiveTrainees,
        IReadOnlyList<string> MsfCampaignsNeedingReview,
        IReadOnlyList<string> CommitteeReviewsThisWeek);

    private sealed class DigestOutcome
    {
        public int Sent { get; set; }

        public SkipTally<ReminderSkipReason> SkippedAccounts { get; } = new();

        public SkipTally<DigestSkipReason> SkippedRecipients { get; } = new();
    }
}
