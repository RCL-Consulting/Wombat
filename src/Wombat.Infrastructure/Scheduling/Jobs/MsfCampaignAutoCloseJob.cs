using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Scheduling;
using Wombat.Domain.MultiSourceFeedback;

namespace Wombat.Infrastructure.Scheduling.Jobs;

public sealed class MsfCampaignAutoCloseJob : IScheduledJob
{
    private readonly IServiceScopeFactory _scopeFactory;

    public MsfCampaignAutoCloseJob(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public string Key => "msf-campaign-auto-close";
    public string CronExpression => "0 * * * *";
    public string Description => "Closes open MSF campaigns past their ClosesOn date and runs aggregation (hourly).";

    public async Task ExecuteAsync(ScheduledJobContext context, CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        var today = DateOnly.FromDateTime(context.UtcNow);

        var expiredCampaigns = await dbContext.Set<MsfCampaign>()
            .Include(c => c.Invitations)
            .Where(c => c.State == MsfCampaignState.Open && c.ClosesOn < today)
            .ToListAsync(cancellationToken);

        if (expiredCampaigns.Count == 0)
        {
            context.Logger.LogInformation("MsfCampaignAutoCloseJob: no expired campaigns found.");
            return;
        }

        foreach (var campaign in expiredCampaigns)
        {
            campaign.Close(context.UtcNow);
            AnonymizeInvitations(campaign.Invitations);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        context.Logger.LogInformation("MsfCampaignAutoCloseJob: auto-closed {Count} campaigns.", expiredCampaigns.Count);
    }

    private static void AnonymizeInvitations(IEnumerable<MsfInvitation> invitations)
    {
        var utcNow = DateTime.UtcNow;
        foreach (var invitation in invitations.Where(i => !string.IsNullOrWhiteSpace(i.RespondentEmail)))
        {
            invitation.RespondentEmailHash = Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes(invitation.RespondentEmail!.Trim().ToUpperInvariant())));
            invitation.RespondentEmail = null;
            invitation.AnonymizedOn = utcNow;
        }
    }
}
