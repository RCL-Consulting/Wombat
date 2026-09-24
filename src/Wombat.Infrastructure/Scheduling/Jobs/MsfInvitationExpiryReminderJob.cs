using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Wombat.Application.Common.Email.Templates;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Options;
using Wombat.Application.Common.Security;
using Wombat.Application.Scheduling;
using Wombat.Domain.MultiSourceFeedback;

namespace Wombat.Infrastructure.Scheduling.Jobs;

/// <summary>
/// Reminds each respondent of an open campaign who has not answered, once, from two days before their last day to
/// respond (<see cref="MsfInvitation.IsReminderDue" />). (T132, T206)
/// </summary>
/// <remarks>
/// Until T206 it keyed on the invitation's own expiry, which the product writes a week after the window closes, while
/// <see cref="MsfCampaignAutoCloseJob" /> closes the campaign the day after the window does. So it could only ever have
/// found a campaign that was already closed, and it never sent anything. The key is still named for the expiry: it is
/// the scheduled job's stored identity.
/// </remarks>
public sealed class MsfInvitationExpiryReminderJob : IScheduledJob
{
    private readonly IServiceScopeFactory _scopeFactory;

    public MsfInvitationExpiryReminderJob(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public string Key => "msf-invitation-expiry-reminder";
    public string CronExpression => "0 8 * * *";
    public string Description =>
        "Reminds MSF respondents who have not responded, once, two days before their last day to respond (daily at 08:00 UTC).";

    public async Task ExecuteAsync(ScheduledJobContext context, CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();
        var tokenService = scope.ServiceProvider.GetRequiredService<IInvitationTokenService>();
        var users = scope.ServiceProvider.GetRequiredService<IUserAdministrationService>();
        var options = scope.ServiceProvider.GetRequiredService<IOptions<WombatOptions>>().Value;

        // Resolved before the query so a misconfigured deployment fails loudly and sends nothing,
        // rather than re-issuing every expiring token and then discovering it has nowhere to send them.
        var respondUrl = options.RequireMsfRespondUrl();

        // The query narrows to an open campaign's unanswered, unrevoked, addressed invitations; whether one is due today
        // is MsfInvitation.IsReminderDue, the one statement of the rule, which asks the campaign's state again. An open
        // campaign is one whose window has not closed (the auto-close job sees to that), so this is never more than the
        // invitations of the campaigns running now.
        var candidates = await dbContext.Set<MsfInvitation>()
            .Include(invitation => invitation.Campaign)
                .ThenInclude(campaign => campaign.Template)
            .Where(invitation =>
                invitation.Campaign.State == MsfCampaignState.Open &&
                invitation.RespondedOn == null &&
                invitation.RevokedOn == null &&
                invitation.AnonymizedOn == null &&
                invitation.RespondentEmail != null &&
                invitation.RespondentEmail != string.Empty)
            .ToListAsync(cancellationToken);

        var due = candidates
            .Where(invitation => invitation.IsReminderDue(invitation.Campaign, context.UtcNow))
            .ToList();

        if (due.Count == 0)
        {
            context.Logger.LogInformation("MsfInvitationExpiryReminderJob: no respondent is due a reminder.");
            return;
        }

        // Whom each reminder is about, as the invitation named them (OpenMsfCampaignCommandHandler, T202).
        var names = await users.GetDisplayNamesAsync(
            due.Select(invitation => invitation.Campaign.SubjectUserId).Distinct(StringComparer.Ordinal).ToList(),
            cancellationToken);

        var sentCount = 0;
        foreach (var invitation in due)
        {
            var campaign = invitation.Campaign;

            // Skipped rather than sent unnamed: a feedback request that does not say whom it is about cannot be
            // answered (T202). The link the invitation carried is left working, and nothing is re-issued.
            if (!names.TryGetValue(campaign.SubjectUserId, out var traineeName) || string.IsNullOrWhiteSpace(traineeName))
            {
                context.Logger.LogWarning(
                    "MsfInvitationExpiryReminderJob: campaign {CampaignId}'s trainee has no name on record, so its respondents were not reminded.",
                    campaign.Id);
                continue;
            }

            // The token this invitation was opened with cannot be recovered: only a one-way hash is
            // stored (InvitationTokenService.HashToken). So the reminder RE-ISSUES — exactly as
            // OpenMsfCampaign does — rather than trying to reconstruct the original.
            //
            // This invalidates the link mailed when the campaign opened. That is the accepted cost
            // (T132): a respondent holding both emails and clicking the older one gets an invalid
            // token, so MsfExpiryReminderEmail says in as many words that the new link replaces any
            // earlier one. Before this, the job mailed `invitation.TokenHash` itself as a relative
            // URL, so every reminder was both unclickable and unusable, and the job still logged
            // success.
            var token = tokenService.GenerateToken();
            var responseUrl = $"{respondUrl}?token={Uri.EscapeDataString(token)}";

            await emailSender.SendAsync(
                MsfExpiryReminderEmail.Build(new MsfInvitationEmailContent(
                    campaign.Id,
                    invitation.RespondentEmail!,
                    traineeName.Trim(),
                    campaign.Template.Name,
                    campaign.OpensOn,
                    campaign.ClosesOn,
                    invitation.ExpiresOn,
                    responseUrl,
                    campaign.Template.Kind)),
                cancellationToken);

            // Stored after the send, and one respondent at a time (T206). A send that throws leaves this respondent's
            // original link working and still due, for the next run; the reminders already sent are stored, so their
            // links work. The other order, store then send, would retire a link and mail nothing to replace it. Not
            // cancellable: the mail has been handed over.
            invitation.IssueLink(tokenService.HashToken(token), context.UtcNow);
            await dbContext.SaveChangesAsync(CancellationToken.None);
            sentCount++;
        }

        context.Logger.LogInformation("MsfInvitationExpiryReminderJob: sent {Count} reminders.", sentCount);
    }
}
