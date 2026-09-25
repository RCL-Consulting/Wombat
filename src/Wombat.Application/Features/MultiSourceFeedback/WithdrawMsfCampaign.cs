using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.MultiSourceFeedback;

namespace Wombat.Application.Features.MultiSourceFeedback;

/// <summary>
/// No validator: carries a non-nullable int ID, the state the caller confirmed, and the caller; the handler authorises
/// the caller against the campaign's subject (T113), refuses a campaign already released or withdrawn (T206 review,
/// T199), and refuses one that has closed since the caller's page showed it (T199 review).
/// </summary>
/// <param name="ConfirmedState">
/// The state the caller's page showed the campaign in when the withdraw was confirmed, which its dialog was worded by
/// (<see cref="MsfCampaignRules.WithdrawingForgoesRelease" />). Written to the audit row with the id.
/// </param>
[NoValidator]
public sealed record WithdrawMsfCampaignCommand(int CampaignId, MsfCampaignState ConfirmedState, ClaimsPrincipal Principal)
    : IRequest;

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

        // Any campaign not yet released, as the campaign list and the campaign page offer it (MsfCampaignRules.IsWithdrawable).
        // One closed and under review too since T199: withdrawing it is the decision never to release its report, which
        // whoever may release it may take, by the same caller rule. Asked of the campaign as it is now, not as the caller's
        // page showed it: a page loaded before another tab released the campaign still offers Withdraw (T206 review).
        if (!MsfCampaignRules.IsWithdrawable(campaign.State))
        {
            throw new InvalidOperationException(NotWithdrawable(campaign.State));
        }

        // The withdraw the caller confirmed, not another one (T199 review). A page loaded while the campaign was open asks
        // in an open campaign's words: its links stop, its addresses go. Once the auto-close job or another tab has closed
        // it, withdrawing it is the decision never to release its report, which that dialog never put to anyone. Refused
        // before anything changes; both pages read the campaign again after a refusal, so the next dialog asks in the
        // closed campaign's words. The other way round cannot happen (a campaign does not reopen), and a caller who
        // confirmed a closed campaign's withdraw may withdraw an open one by the same rule, so it is not asked.
        if (MsfCampaignRules.WithdrawingForgoesRelease(campaign.State) &&
            !MsfCampaignRules.WithdrawingForgoesRelease(request.ConfirmedState))
        {
            throw new InvalidOperationException(ClosedSinceShown);
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
    /// closed, released or withdrawn elsewhere. Nothing is stored. (T206 review) Shown on the campaign list and on the
    /// campaign page, both of which read the campaign again and show its state, so it names neither (T217 review).
    /// </summary>
    public const string CampaignChanged =
        "The campaign changed while it was being withdrawn: an invitee was added, or it was opened, closed, released or " +
        "withdrawn elsewhere. It has not been withdrawn. If it has not been released or withdrawn, withdraw it again.";

    /// <summary>The refusal of a campaign that can no longer be withdrawn, saying what it is now. (T206 review, T199)</summary>
    public static string NotWithdrawable(MsfCampaignState state)
        => state == MsfCampaignState.Withdrawn
            ? MsfCampaign.AlreadyWithdrawn
            : ReleasedNotWithdrawable;

    /// <summary>
    /// The refusal of a released campaign, the one state besides Withdrawn that cannot be withdrawn: its report is the
    /// trainee's, and its evidence is on their record. (T199)
    /// </summary>
    public const string ReleasedNotWithdrawable =
        "This campaign has been released to the trainee, so it can no longer be withdrawn.";

    /// <summary>
    /// The refusal of a withdraw confirmed while the caller's page showed the campaign as a draft or open, of a campaign
    /// that has closed since: withdrawing it now means its report is never released, which is not what the caller was
    /// asked. Nothing is stored. (T199 review)
    /// </summary>
    public const string ClosedSinceShown =
        "This campaign has closed since your page showed it, and its report has not been released. Withdrawing it now " +
        "would mean its report is never released to the trainee. It has not been withdrawn; if that is what you want, " +
        "withdraw it again.";
}
