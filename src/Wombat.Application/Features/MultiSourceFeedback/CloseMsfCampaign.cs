using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
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

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            // The campaign's xmin token (MsfCampaignConfiguration) refused the save: it was withdrawn or closed elsewhere,
            // or the reminder job stored a respondent's new link, which writes the campaign row so that a close cannot
            // leave that link on an anonymised invitation (T214 review). Carried as the inner exception, so the audit
            // pipeline still sees a refused save and writes its row alone (T201); EF's own message names row counts.
            throw new InvalidOperationException(CampaignChanged, exception);
        }

        // Closed, not released: nothing has been recorded yet. For the coordinator who closed it, who typed the teaching
        // contexts and is told them (T164).
        return _aggregationService.BuildReport(campaign, [], nameTeachingContexts: true);
    }

    /// <summary>
    /// The refusal when the campaign changed between being read and the save: it was closed or withdrawn elsewhere, or a
    /// respondent was sent a reminder. Nothing this attempt did is stored. (T214 review) Shown on the report page, which
    /// reads the campaign again and shows its state, so it names no other page (T225, as T217 worded open and withdraw).
    /// It says "this attempt", not "it has not been closed": closed elsewhere, it has been.
    /// </summary>
    public const string CampaignChanged =
        "The campaign changed while it was being closed: it was closed or withdrawn elsewhere, or a respondent was sent " +
        "a reminder. This attempt did not close it. If it is still open, close it again.";
}
