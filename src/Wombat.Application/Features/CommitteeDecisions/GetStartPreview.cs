using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.CommitteeDecisions;

namespace Wombat.Application.Features.CommitteeDecisions;

/// <summary>
/// What Start would freeze for a scheduled review, for the Start dialog's count (T361, round 2 review 11; round 3 item 11):
/// "the 9 items observed from 2026-01-01 to 2026-12-31 (8 activities and the MSF campaign report)", and the MSF campaigns
/// that closed in the window and are not yet released, which Start leaves out (T173's sentence).
/// </summary>
/// <remarks>
/// <para>
/// Counted by building the snapshot Start would build (<see cref="CommitteeEvidenceSnapshot.BuildAsync" />), the one code
/// both run, and never by a second query of its own: a count that drifted from what Start freezes would tell the chair one
/// thing and freeze another. Nothing is staged. The count is as of the read; a form filed between the dialog and the press
/// is frozen too, as it would have been without the dialog.
/// </para>
/// <para>
/// Null, so the page draws no count, when the caller may not read the review (an unknown id alike, T194 item 1) or it is
/// not scheduled, the one state Start takes. Read by the review ladder (<see cref="CommitteeDecisionAuthorization.ReadableReviewsAsync" />),
/// not by Start's own rule: the page offers Start by <see cref="CommitteeReviewDetailDto.CallerMayStart" />, and asks for
/// this only where it does.
/// </para>
/// </remarks>
public sealed record GetStartPreviewQuery(int ReviewId, ClaimsPrincipal Principal) : IRequest<StartPreviewDto?>;

/// <summary>What Start would freeze, and what it would leave out (T361).</summary>
/// <param name="ItemCount">Every line Start would freeze: the activities and the campaign reports.</param>
/// <param name="ActivityCount">The trainee's activities observed in the window, in any state.</param>
/// <param name="CampaignReportCount">The MSF campaigns released and closed in the window, one report line each.</param>
/// <param name="WindowFrom">The window's first day, the review period's.</param>
/// <param name="WindowTo">The window's last day.</param>
/// <param name="CampaignsLeftOut">
/// The campaigns that closed in the window and await release, which Start leaves out and no later snapshot takes (T173).
/// </param>
public sealed record StartPreviewDto(
    int ItemCount,
    int ActivityCount,
    int CampaignReportCount,
    DateOnly WindowFrom,
    DateOnly WindowTo,
    int CampaignsLeftOut);

public sealed class GetStartPreviewQueryValidator : AbstractValidator<GetStartPreviewQuery>
{
    public GetStartPreviewQueryValidator()
    {
        RuleFor(query => query.ReviewId).GreaterThan(0);
        RuleFor(query => query.Principal).NotNull();
    }
}

public sealed class GetStartPreviewQueryHandler : IRequestHandler<GetStartPreviewQuery, StartPreviewDto?>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserAdministrationService _users;

    public GetStartPreviewQueryHandler(IApplicationDbContext dbContext, IUserAdministrationService users)
    {
        _dbContext = dbContext;
        _users = users;
    }

    public async Task<StartPreviewDto?> Handle(GetStartPreviewQuery request, CancellationToken cancellationToken)
    {
        var review = await _dbContext.Set<CommitteeReview>()
            .AsNoTracking()
            .Include(entity => entity.Panel)
                .ThenInclude(panel => panel.Members)
            .SingleOrDefaultAsync(entity => entity.Id == request.ReviewId, cancellationToken);

        if (review is null ||
            (await CommitteeDecisionAuthorization.ReadableReviewsAsync(
                _dbContext, _users, request.Principal, [review], cancellationToken)).Count == 0 ||
            review.State != CommitteeReviewState.Scheduled)
        {
            return null;
        }

        var items = await CommitteeEvidenceSnapshot.BuildAsync(_dbContext, review, cancellationToken);
        var activities = items.Count(item => item.SourceType == CommitteeEvidenceSourceType.Activity);
        var campaigns = items.Count(item => item.SourceType == CommitteeEvidenceSourceType.MsfCampaign);
        var leftOut = await CountMsfCampaignsOutsideSnapshotQueryHandler.CountOutsideAsync(_dbContext, review, cancellationToken);

        return new StartPreviewDto(
            items.Count,
            activities,
            campaigns,
            review.ReviewPeriodFrom,
            review.ReviewPeriodTo,
            leftOut.Total);
    }
}
