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

public sealed class MsfInvitationExpiryReminderJob : IScheduledJob
{
    private readonly IServiceScopeFactory _scopeFactory;

    public MsfInvitationExpiryReminderJob(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public string Key => "msf-invitation-expiry-reminder";
    public string CronExpression => "0 8 * * *";
    public string Description => "Reminds MSF respondents whose invitation tokens expire within 48 hours (daily at 08:00 UTC).";

    public async Task ExecuteAsync(ScheduledJobContext context, CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();
        var tokenService = scope.ServiceProvider.GetRequiredService<IInvitationTokenService>();
        var options = scope.ServiceProvider.GetRequiredService<IOptions<WombatOptions>>().Value;

        // Resolved before the query so a misconfigured deployment fails loudly and sends nothing,
        // rather than re-issuing every expiring token and then discovering it has nowhere to send them.
        var respondUrl = options.RequireMsfRespondUrl();

        var today = DateOnly.FromDateTime(context.UtcNow);
        var expiryWindow = today.AddDays(2);

        var expiringInvitations = await dbContext.Set<MsfInvitation>()
            .Include(i => i.Campaign)
            .Where(i =>
                i.ExpiresOn <= expiryWindow &&
                i.ExpiresOn >= today &&
                i.RespondedOn == null &&
                i.RevokedOn == null &&
                i.AnonymizedOn == null &&
                !string.IsNullOrEmpty(i.RespondentEmail) &&
                i.Campaign.State == MsfCampaignState.Open)
            .ToListAsync(cancellationToken);

        if (expiringInvitations.Count == 0)
        {
            context.Logger.LogInformation("MsfInvitationExpiryReminderJob: no expiring invitations found.");
            return;
        }

        var sentCount = 0;
        foreach (var invitation in expiringInvitations)
        {
            if (string.IsNullOrWhiteSpace(invitation.RespondentEmail))
                continue;

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
            invitation.TokenHash = tokenService.HashToken(token);

            var responseUrl = $"{respondUrl}?token={Uri.EscapeDataString(token)}";
            var email = MsfExpiryReminderEmail.Build(invitation.RespondentEmail, responseUrl, invitation.ExpiresOn);
            await emailSender.SendAsync(email, cancellationToken);
            sentCount++;
        }

        // The re-issued hashes are only useful if they outlive the job.
        await dbContext.SaveChangesAsync(cancellationToken);

        context.Logger.LogInformation("MsfInvitationExpiryReminderJob: sent {Count} expiry reminders.", sentCount);
    }
}
