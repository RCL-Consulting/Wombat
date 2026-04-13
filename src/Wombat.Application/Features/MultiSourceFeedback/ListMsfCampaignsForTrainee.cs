using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.MultiSourceFeedback;

namespace Wombat.Application.Features.MultiSourceFeedback;

public sealed record ListMsfCampaignsForTraineeQuery(string SubjectUserId) : IRequest<IReadOnlyList<MsfCampaignSummaryDto>>;

public sealed class ListMsfCampaignsForTraineeQueryHandler : IRequestHandler<ListMsfCampaignsForTraineeQuery, IReadOnlyList<MsfCampaignSummaryDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public ListMsfCampaignsForTraineeQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<MsfCampaignSummaryDto>> Handle(ListMsfCampaignsForTraineeQuery request, CancellationToken cancellationToken)
    {
        return await _dbContext.Set<MsfCampaign>()
            .AsNoTracking()
            .Include(campaign => campaign.Template)
            .Include(campaign => campaign.Invitations)
            .Include(campaign => campaign.Responses)
            .Where(campaign => campaign.SubjectUserId == request.SubjectUserId && campaign.State == MsfCampaignState.Released)
            .OrderByDescending(campaign => campaign.ReleasedOn)
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
