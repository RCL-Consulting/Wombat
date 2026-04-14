using MediatR;
using Wombat.Application.Common;
using Wombat.Application.Common.Interfaces;

namespace Wombat.Application.Features.MultiSourceFeedback;

/// <summary>No validator: carries a single non-nullable int ID; EF lookup enforces existence.</summary>
[NoValidator]
public sealed record CloseMsfCampaignCommand(int CampaignId) : IRequest<MsfCampaignAggregateReportDto>;

public sealed class CloseMsfCampaignCommandHandler : IRequestHandler<CloseMsfCampaignCommand, MsfCampaignAggregateReportDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IMsfAggregationService _aggregationService;

    public CloseMsfCampaignCommandHandler(IApplicationDbContext dbContext, IMsfAggregationService aggregationService)
    {
        _dbContext = dbContext;
        _aggregationService = aggregationService;
    }

    public async Task<MsfCampaignAggregateReportDto> Handle(CloseMsfCampaignCommand request, CancellationToken cancellationToken)
    {
        var campaign = await MsfCampaignRules.GetCampaignGraphAsync(_dbContext, request.CampaignId, cancellationToken);
        campaign.Close(DateTime.UtcNow);
        MsfCampaignRules.AnonymizeInvitations(campaign.Invitations);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return _aggregationService.BuildReport(campaign);
    }
}
