using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.MultiSourceFeedback;

namespace Wombat.Application.Features.MultiSourceFeedback;

public sealed record ListMsfCampaignsForCoordinatorQuery() : IRequest<IReadOnlyList<MsfCampaignSummaryDto>>;

public sealed class ListMsfCampaignsForCoordinatorQueryHandler : IRequestHandler<ListMsfCampaignsForCoordinatorQuery, IReadOnlyList<MsfCampaignSummaryDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public ListMsfCampaignsForCoordinatorQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<MsfCampaignSummaryDto>> Handle(ListMsfCampaignsForCoordinatorQuery request, CancellationToken cancellationToken)
    {
        return await _dbContext.Set<MsfCampaign>()
            .AsNoTracking()
            .Include(campaign => campaign.Template)
            .Include(campaign => campaign.Invitations)
            .Include(campaign => campaign.Responses)
            .OrderByDescending(campaign => campaign.CreatedOn)
            .Select(campaign => new MsfCampaignSummaryDto(
                campaign.Id,
                campaign.SubjectUserId,
                campaign.Template.Name,
                campaign.OpensOn,
                campaign.ClosesOn,
                campaign.MinimumResponses,
                campaign.MinimumCategoryResponses,
                campaign.State,
                campaign.Invitations.Count,
                campaign.Responses.Count,
                campaign.ReleasedOn))
            .ToListAsync(cancellationToken);
    }
}
