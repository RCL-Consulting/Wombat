using System.Security.Claims;
using MediatR;
using Wombat.Application.Common;
using Wombat.Application.Common.Interfaces;

namespace Wombat.Application.Features.MultiSourceFeedback;

/// <summary>
/// No validator: carries a non-nullable int ID and the caller; the handler authorises the caller against the
/// campaign's subject (T113) and the aggregate validates the state transition.
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

        var campaign = await MsfCampaignRules.GetCampaignGraphAsync(_dbContext, request.CampaignId, cancellationToken);

        campaign.Withdraw(DateTime.UtcNow);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
