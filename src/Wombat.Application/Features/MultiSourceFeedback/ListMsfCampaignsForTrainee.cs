using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Domain.MultiSourceFeedback;

namespace Wombat.Application.Features.MultiSourceFeedback;

/// <summary>
/// One trainee's released feedback campaigns, for the trainee themselves or anyone who may read about them
/// (<see cref="TraineeScopeResolver.MayReadAsync" />); empty for anyone else, never a refusal. (T113)
/// </summary>
/// <remarks>
/// Newest release first, released only, and no count (T369, F3): <see cref="MsfReleasedReportDto" /> carries none, so no
/// count can reach her list.
/// </remarks>
public sealed record ListMsfCampaignsForTraineeQuery(string SubjectUserId, ClaimsPrincipal Principal)
    : IRequest<IReadOnlyList<MsfReleasedReportDto>>;

public sealed class ListMsfCampaignsForTraineeQueryHandler : IRequestHandler<ListMsfCampaignsForTraineeQuery, IReadOnlyList<MsfReleasedReportDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public ListMsfCampaignsForTraineeQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<MsfReleasedReportDto>> Handle(ListMsfCampaignsForTraineeQuery request, CancellationToken cancellationToken)
    {
        if (!await TraineeScopeResolver.MayReadAsync(_dbContext, request.Principal, request.SubjectUserId, cancellationToken))
        {
            return [];
        }

        return await _dbContext.Set<MsfCampaign>()
            .AsNoTracking()
            .Where(campaign => campaign.SubjectUserId == request.SubjectUserId &&
                               campaign.State == MsfCampaignState.Released &&
                               campaign.ReleasedOn != null)
            .OrderByDescending(campaign => campaign.ReleasedOn)
            .ThenByDescending(campaign => campaign.Id)
            .Select(campaign => new MsfReleasedReportDto(
                campaign.Id,
                campaign.Template.Name,
                campaign.Template.Kind,
                campaign.ReleasedOn!.Value))
            .ToListAsync(cancellationToken);
    }
}
