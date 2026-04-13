using MediatR;
using Wombat.Application.Common.Interfaces;

namespace Wombat.Application.Features.MultiSourceFeedback;

public sealed record WithdrawMsfCampaignCommand(int CampaignId) : IRequest;

public sealed class WithdrawMsfCampaignCommandHandler : IRequestHandler<WithdrawMsfCampaignCommand>
{
    private readonly IApplicationDbContext _dbContext;

    public WithdrawMsfCampaignCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(WithdrawMsfCampaignCommand request, CancellationToken cancellationToken)
    {
        var campaign = await MsfCampaignRules.GetCampaignGraphAsync(_dbContext, request.CampaignId, cancellationToken);
        campaign.Withdraw(DateTime.UtcNow);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
