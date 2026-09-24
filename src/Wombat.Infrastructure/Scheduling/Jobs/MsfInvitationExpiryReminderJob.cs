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

            // Asked again before each reminder (T214 review). A store refused below refreshes the campaign, and a campaign
            // closed or withdrawn while this run was sending is neither mailed about nor stored onto any further. Stored
            // onto it would be: the refreshed xmin lets the store through, while its other invitations are still held as
            // they were read, before the close anonymised them.
            if (!invitation.IsReminderDue(campaign, context.UtcNow))
            {
                continue;
            }

            // Skipped rather than sent unnamed: a feedback request that does not say whom it is about cannot be
            // answered (T202). The link the invitation carried is left working, and nothing is re-issued.
            if (!names.TryGetValue(campaign.SubjectUserId, out var traineeName) || string.IsNullOrWhiteSpace(traineeName))
            {
                context.Logger.LogWarning(
                    "MsfInvitationExpiryReminderJob: campaign {CampaignId}'s trainee has no name on record, so its respondents were not reminded.",
                    campaign.Id);
                continue;
            }

            // The token this invitation was opened with cannot be recovered: only its selector and a
            // one-way hash of it are stored (InvitationTokenService.GenerateSelectorToken). So the
            // reminder RE-ISSUES — exactly as OpenMsfCampaign does — rather than trying to reconstruct
            // the original. Before T132 the job mailed `invitation.TokenHash` itself as a relative URL,
            // so every reminder was both unclickable and unusable, and the job still logged success.
            //
            // The link mailed when the campaign opened is kept as the previous link, and still takes
            // the respondent's one response until their last day to respond (T214). Until T214 the
            // reminder retired it outright, so a respondent part-way through the questionnaire on it
            // lost what they had typed when they submitted.
            var token = tokenService.GenerateSelectorToken();
            var responseUrl = $"{respondUrl}?token={Uri.EscapeDataString(token.Token)}";

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
            // links work. The other order, store then send, would store a link nobody was mailed, and since a link is
            // replaced once, the respondent would never be reminded. Not cancellable: the mail has been handed over.
            invitation.ReplaceLink(token.Selector, token.Hash, context.UtcNow);

            // Only onto the invitation as it was read (T214 review). The send takes seconds, and in them the respondent
            // may answer, or the coordinator close or withdraw the campaign, which anonymises it. The store would then
            // put the replaced link back on a row that has retired it: EF writes only the columns this job changed. So
            // the campaign row is written too, unchanged, under its xmin token (MsfCampaignConfiguration), which a close
            // or withdrawal moves; and an answer is refused by the server (CK_MsfInvitations_PreviousLinkUnanswered).
            // A close or withdrawal that read the campaign before this store commits is refused at its own save, by the
            // same token, and closing again retires the link kept here (MsfLinkSelectorPostgresTests).
            var invitationEntry = dbContext.Set<MsfInvitation>().Entry(invitation);
            var campaignEntry = dbContext.Set<MsfCampaign>().Entry(campaign);
            campaignEntry.Property(candidate => candidate.State).IsModified = true;

            try
            {
                await dbContext.SaveChangesAsync(CancellationToken.None);
                sentCount++;
            }
            catch (DbUpdateException)
            {
                // Both read again, which also puts back what this attempt changed, so the next respondent's save does
                // not carry it. The link just mailed is then stored nowhere and opens nothing: its respondent has
                // answered, or their campaign has been closed or withdrawn, and their first link says which.
                await invitationEntry.ReloadAsync(CancellationToken.None);
                await campaignEntry.ReloadAsync(CancellationToken.None);

                // A refusal the respondent is still due a reminder after is not one of those, and is a fault: the run
                // stops, as any failed store did before T214, since a later respondent's link must not be mailed if it may
                // not be stored.
                if (invitation.IsReminderDue(campaign, context.UtcNow))
                {
                    throw;
                }

                context.Logger.LogWarning(
                    "MsfInvitationExpiryReminderJob: invitation {InvitationId} was answered, or campaign {CampaignId} was closed or withdrawn, while its reminder was being sent, so the reminder's link was not stored.",
                    invitation.Id,
                    campaign.Id);
            }
        }

        context.Logger.LogInformation("MsfInvitationExpiryReminderJob: sent {Count} reminders.", sentCount);
    }
}
