using MediatR;
using Wombat.Application.Common.Interfaces;

namespace Wombat.Application.Features.MultiSourceFeedback;

public sealed record GetCampaignAggregateReportQuery(int CampaignId) : IRequest<MsfCampaignAggregateReportDto>;

public sealed class GetCampaignAggregateReportQueryHandler : IRequestHandler<GetCampaignAggregateReportQuery, MsfCampaignAggregateReportDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IMsfAggregationService _aggregationService;

    public GetCampaignAggregateReportQueryHandler(IApplicationDbContext dbContext, IMsfAggregationService aggregationService)
    {
        _dbContext = dbContext;
        _aggregationService = aggregationService;
    }

    public async Task<MsfCampaignAggregateReportDto> Handle(GetCampaignAggregateReportQuery request, CancellationToken cancellationToken)
    {
        var campaign = await MsfCampaignRules.GetCampaignGraphAsync(_dbContext, request.CampaignId, cancellationToken);
        return _aggregationService.BuildReport(campaign);
    }
}
