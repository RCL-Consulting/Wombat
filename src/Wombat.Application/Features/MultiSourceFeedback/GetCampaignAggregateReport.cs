using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.MultiSourceFeedback;

namespace Wombat.Application.Features.MultiSourceFeedback;

/// <summary>
/// One campaign's aggregate report, or null when there is no such campaign or the caller may not read it. (T113)
/// </summary>
/// <remarks>
/// Null for both, per the T056 convention for a read by id: a refusal that differed from "not found" would let a
/// campaign id in the address bar be incremented into a census of other institutions' campaigns. Who may read it is
/// <see cref="MsfCampaignRules.CanReadReportAsync" />: the trainee it is about once it is released, and whoever runs
/// campaigns for that trainee.
/// <para>
/// The trainee's copy counts a learner-feedback campaign's teaching contexts and does not name them; whoever runs the
/// campaign typed them and is told them (<see cref="MsfCampaignAggregateReportDto.TeachingContextsResponded" />, T164).
/// </para>
/// </remarks>
public sealed record GetCampaignAggregateReportQuery(int CampaignId, ClaimsPrincipal Principal)
    : IRequest<MsfCampaignAggregateReportDto?>;

public sealed class GetCampaignAggregateReportQueryHandler
    : IRequestHandler<GetCampaignAggregateReportQuery, MsfCampaignAggregateReportDto?>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IMsfAggregationService _aggregationService;

    public GetCampaignAggregateReportQueryHandler(IApplicationDbContext dbContext, IMsfAggregationService aggregationService)
    {
        _dbContext = dbContext;
        _aggregationService = aggregationService;
    }

    public async Task<MsfCampaignAggregateReportDto?> Handle(GetCampaignAggregateReportQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request.Principal);

        // Authorised from the campaign's subject and state alone, before the graph - every response and every
        // verbatim comment - is loaded at all.
        var header = await _dbContext.Set<MsfCampaign>()
            .AsNoTracking()
            .Where(campaign => campaign.Id == request.CampaignId)
            .Select(campaign => new { campaign.SubjectUserId, campaign.State })
            .FirstOrDefaultAsync(cancellationToken);

        if (header is null ||
            !await MsfCampaignRules.CanReadReportAsync(
                _dbContext, request.Principal, header.SubjectUserId, header.State, cancellationToken))
        {
            return null;
        }

        var campaign = await MsfCampaignRules.GetCampaignGraphAsync(_dbContext, request.CampaignId, cancellationToken);

        // Which declared EPAs were recorded, from the evidence rows, as the committee snapshot and the coverage grid
        // read it (T186), from the type this campaign's kind writes (T164). Nothing is read for a campaign that is not
        // released.
        var recorded = await MsfCampaignCoverage.RecordedEpasAsync(
            _dbContext,
            campaign.SubjectUserId,
            [(campaign.Id, campaign.State, campaign.Template.Kind)],
            cancellationToken);

        // Only the trainee reads the report about themselves; anyone else CanReadReportAsync admitted runs the campaign.
        // Asked as CanReadReportAsync asks it (MsfCampaignRules.IsCaller), so the two cannot disagree about who the
        // subject is. (T224 review)
        var callerIsSubject = MsfCampaignRules.IsCaller(request.Principal, campaign.SubjectUserId);

        return _aggregationService.BuildReport(campaign, recorded[campaign.Id], nameTeachingContexts: !callerIsSubject);
    }
}
