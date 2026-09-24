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
public sealed record CloseMsfCampaignCommand(int CampaignId, ClaimsPrincipal Principal) : IRequest<MsfCampaignAggregateReportDto>;

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
        // Before anything is loaded to be touched: a close anonymises every respondent for good, and the report it
        // returns is the unreleased one. (T113)
        await MsfCampaignRules.EnsureCampaignIsInScopeAsync(
            _dbContext, request.Principal, request.CampaignId, cancellationToken);

        var campaign = await MsfCampaignRules.GetCampaignGraphAsync(_dbContext, request.CampaignId, cancellationToken);

        // Closing anonymises every respondent (MsfCampaign.Close), the same routine the auto-close job reaches. (T184)
        campaign.Close(DateTime.UtcNow);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return _aggregationService.BuildReport(campaign);
    }
}
