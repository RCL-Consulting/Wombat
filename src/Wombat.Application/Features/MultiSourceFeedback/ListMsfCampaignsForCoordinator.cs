using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.MultiSourceFeedback;

namespace Wombat.Application.Features.MultiSourceFeedback;

/// <summary>
/// The campaigns this caller runs: every campaign for an Administrator, the campaigns about trainees at their own
/// institution for a Coordinator, and none for anyone else. (T113)
/// </summary>
/// <remarks>
/// This took no parameters at all until T113, and every coordinator in the country saw every campaign in every
/// institution: who was being assessed, how many had responded, and whether it had been released.
/// </remarks>
public sealed record ListMsfCampaignsForCoordinatorQuery(ClaimsPrincipal Principal)
    : IRequest<IReadOnlyList<MsfCampaignSummaryDto>>;

public sealed class ListMsfCampaignsForCoordinatorQueryHandler : IRequestHandler<ListMsfCampaignsForCoordinatorQuery, IReadOnlyList<MsfCampaignSummaryDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public ListMsfCampaignsForCoordinatorQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<MsfCampaignSummaryDto>> Handle(ListMsfCampaignsForCoordinatorQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request.Principal);

        return await _dbContext.Set<MsfCampaign>()
            .AsNoTracking()
            .WhereRunBy(_dbContext, request.Principal)
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
