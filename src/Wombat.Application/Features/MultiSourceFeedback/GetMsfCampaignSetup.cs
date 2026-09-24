using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.MultiSourceFeedback;

namespace Wombat.Application.Features.MultiSourceFeedback;

/// <summary>
/// What the campaign form needs to add invitees to one campaign: its questionnaire's kind and whom it may invite; or
/// null when there is no such campaign or the caller does not run it. (T164)
/// </summary>
/// <remarks>
/// Null for both, as <see cref="GetCampaignAggregateReportQuery" /> answers (T113): the form is reached by a campaign id
/// in the address bar. Nothing a respondent gave is read: no address, no response.
/// </remarks>
public sealed record GetMsfCampaignSetupQuery(int CampaignId, ClaimsPrincipal Principal) : IRequest<MsfCampaignSetupDto?>;

/// <param name="AcceptedCategories">
/// The respondent categories this campaign's questionnaire accepts (<c>MsfTemplate.Accepts</c>), which the invite command
/// holds it to: Learner alone for learner feedback, every other category for multi-source feedback.
/// </param>
public sealed record MsfCampaignSetupDto(
    int CampaignId,
    string TemplateName,
    MsfTemplateKind Kind,
    MsfCampaignState State,
    IReadOnlyList<MsfRespondentCategory> AcceptedCategories);

public sealed class GetMsfCampaignSetupQueryHandler : IRequestHandler<GetMsfCampaignSetupQuery, MsfCampaignSetupDto?>
{
    private readonly IApplicationDbContext _dbContext;

    public GetMsfCampaignSetupQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<MsfCampaignSetupDto?> Handle(GetMsfCampaignSetupQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request.Principal);

        var campaign = await _dbContext.Set<MsfCampaign>()
            .AsNoTracking()
            .Include(entity => entity.Template)
            .Where(entity => entity.Id == request.CampaignId)
            .FirstOrDefaultAsync(cancellationToken);

        if (campaign is null ||
            !await MsfCampaignRules.IsSubjectInScopeAsync(_dbContext, request.Principal, campaign.SubjectUserId, cancellationToken))
        {
            return null;
        }

        return new MsfCampaignSetupDto(
            campaign.Id,
            campaign.Template.Name,
            campaign.Template.Kind,
            campaign.State,
            campaign.Template.AcceptedCategories());
    }
}
