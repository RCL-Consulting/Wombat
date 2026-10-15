using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Users;
using Wombat.Application.Features.Activities.Services;
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
/// It says it is the trainee's (<see cref="MsfCampaignAggregateReportDto.IsSubjectsCopy" />), so the coordinator's
/// report page, which a trainee who also coordinates can open, sends them to their own copy instead (T269).
/// </para>
/// <para>
/// It is the report page's one read (T369, build-contracts § 1): the trainee's name, the window, the invited count, who
/// closed it and when it was released, the trainee's standing (<see cref="MsfCampaignRules.SubjectStandingAsync" />), and
/// before release which named EPAs the release would record (<see cref="MsfCoveredEpaDto.WillBeRecorded" />, from the
/// release's own predicate). No group's results while the campaign has not closed (F1), for every reader.
/// </para>
/// </remarks>
public sealed record GetCampaignAggregateReportQuery(int CampaignId, ClaimsPrincipal Principal)
    : IRequest<MsfCampaignAggregateReportDto?>;

public sealed class GetCampaignAggregateReportQueryHandler
    : IRequestHandler<GetCampaignAggregateReportQuery, MsfCampaignAggregateReportDto?>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IMsfAggregationService _aggregationService;
    private readonly IUserAdministrationService _users;
    private readonly IActivityReferenceDataService _referenceData;

    public GetCampaignAggregateReportQueryHandler(
        IApplicationDbContext dbContext,
        IMsfAggregationService aggregationService,
        IUserAdministrationService users,
        IActivityReferenceDataService referenceData)
    {
        _dbContext = dbContext;
        _aggregationService = aggregationService;
        _users = users;
        _referenceData = referenceData;
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
        // subject is (T224 review). The same answer marks the copy as theirs, which the coordinator's report page reads
        // to send them to their own (T269).
        var callerIsSubject = MsfCampaignRules.IsCaller(request.Principal, campaign.SubjectUserId);

        var report = _aggregationService.BuildReport(campaign, recorded[campaign.Id], nameTeachingContexts: !callerIsSubject);

        // Before release, which named EPAs the release would record: the predicate it drops an EPA by, asked now (T369,
        // M6). Released, BuildReport has already set each to what was recorded.
        var coveredEpas = report.CoveredEpas;
        if (campaign.State != MsfCampaignState.Released && coveredEpas.Count > 0)
        {
            var coverable = await MsfCampaignRules.CoverableEpaIdsAsync(
                _referenceData, campaign.SubjectUserId, campaign.Template.Kind, cancellationToken);
            coveredEpas = coveredEpas.Select(epa => epa with { WillBeRecorded = coverable.Contains(epa.EpaId) }).ToList();
        }

        var standing = await MsfCampaignRules.SubjectStandingAsync(_dbContext, _users, campaign.SubjectUserId, cancellationToken);
        var names = await UserDisplayNames.ResolveAsync(_users, [campaign.SubjectUserId], cancellationToken);

        report = report with
        {
            IsSubjectsCopy = callerIsSubject,
            CoveredEpas = coveredEpas,
            SubjectName = names.NameOf(campaign.SubjectUserId),
            SubjectIsCurrent = standing.IsCurrent,
            SubjectEndedOn = standing.EndedOn
        };

        return callerIsSubject ? TraineesCopy(report) : report;
    }

    /// <summary>
    /// The trainee's copy carries only what the trainee's page may show (T369, build review R6; F3): no suppressed group, no
    /// group's or question's count, no total, no invited count, no count of groups reporting, and not who closed it. Until
    /// R6 only the page kept these out, so the copy itself still held a suppressed group's one answer counted. A question
    /// nobody in a group rated carries no scale, so "answered" no longer needs its count.
    /// </summary>
    internal static MsfCampaignAggregateReportDto TraineesCopy(MsfCampaignAggregateReportDto report)
        => report with
        {
            Categories = report.Categories
                .Where(category => !category.IsSuppressed)
                .Select(category => category with
                {
                    ResponseCount = 0,
                    Questions = category.Questions
                        .Select(question => question with
                        {
                            Scale = question.Scale is { ResponseCount: > 0 } scale ? scale with { ResponseCount = 0 } : null
                        })
                        .ToList()
                })
                .ToList(),
            TotalResponses = 0,
            SurvivingCategoryCount = 0,
            InvitedTotal = 0,
            ClosedByUserId = null
        };
}
