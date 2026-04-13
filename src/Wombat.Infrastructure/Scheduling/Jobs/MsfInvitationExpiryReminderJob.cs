using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Wombat.Application.Common.Email.Templates;
using Wombat.Application.Common.Interfaces;
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

            var responseUrl = $"/msf/respond?token={Uri.EscapeDataString(invitation.TokenHash)}";
            var email = MsfExpiryReminderEmail.Build(invitation.RespondentEmail, responseUrl, invitation.ExpiresOn);
            await emailSender.SendAsync(email, cancellationToken);
            sentCount++;
        }

        context.Logger.LogInformation("MsfInvitationExpiryReminderJob: sent {Count} expiry reminders.", sentCount);
    }
}
