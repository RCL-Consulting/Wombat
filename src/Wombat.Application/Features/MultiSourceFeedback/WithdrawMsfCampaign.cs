using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.MultiSourceFeedback;

namespace Wombat.Application.Features.MultiSourceFeedback;

/// <summary>
/// No validator: carries a non-nullable int ID and the caller; the handler authorises the caller against the
/// campaign's subject (T113) and refuses a campaign that is not a draft or open (T206 review).
/// </summary>
[NoValidator]
public sealed record WithdrawMsfCampaignCommand(int CampaignId, ClaimsPrincipal Principal) : IRequest;

public sealed class WithdrawMsfCampaignCommandHandler : IRequestHandler<WithdrawMsfCampaignCommand>
{
    private readonly IApplicationDbContext _dbContext;

    public WithdrawMsfCampaignCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(WithdrawMsfCampaignCommand request, CancellationToken cancellationToken)
    {
        // Before anything is loaded to be touched. (T113)
        await MsfCampaignRules.EnsureCampaignIsInScopeAsync(
            _dbContext, request.Principal, request.CampaignId, cancellationToken);

        // The graph carries the invitations, which withdrawing anonymises (MsfCampaign.Withdraw), as closing does. (T202)
        var campaign = await MsfCampaignRules.GetCampaignGraphAsync(_dbContext, request.CampaignId, cancellationToken);

        // Only a draft or an open campaign, the two the campaign list offers Withdraw on (T206). The aggregate would also
        // withdraw one under review, which is a decision never to release its report, and nobody has decided who may make
        // it. Asked of the campaign as it is now, not as the caller's list showed it: a list loaded before the auto-close
        // job ran still shows the campaign open (T206 review).
        if (campaign.State is not (MsfCampaignState.Draft or MsfCampaignState.Open))
        {
            throw new InvalidOperationException(NotWithdrawable(campaign.State));
        }

        campaign.Withdraw(DateTime.UtcNow);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            // Carried as the inner exception, so the audit pipeline still sees a refused save and writes its row alone
            // (T201). EF's own message names row counts; the coordinator needs to know what happened (T206 review).
            throw new InvalidOperationException(CampaignChanged, exception);
        }
    }

    /// <summary>
    /// The refusal when the campaign changed between being read and the save: an invitee was added, or it was opened,
    /// closed or withdrawn elsewhere. Nothing is stored. (T206 review)
    /// </summary>
    public const string CampaignChanged =
        "The campaign changed while it was being withdrawn: an invitee was added, or it was opened, closed or withdrawn " +
        "elsewhere. It has not been withdrawn. If the campaigns list still shows it as a draft or open, withdraw it again.";

    /// <summary>The refusal of a campaign that is no longer a draft or open, saying what it is now. (T206 review)</summary>
    public static string NotWithdrawable(MsfCampaignState state)
        => state == MsfCampaignState.Withdrawn
            ? MsfCampaign.AlreadyWithdrawn
            : "Only a draft or open campaign can be withdrawn, and this one " + state switch
            {
                MsfCampaignState.UnderReview => "has closed and is under review.",
                MsfCampaignState.Released => "has been released to the trainee.",
                _ => $"is {state}."
            };
}
