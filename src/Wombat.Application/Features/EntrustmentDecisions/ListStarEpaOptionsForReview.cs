using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Domain.CommitteeDecisions;

namespace Wombat.Application.Features.EntrustmentDecisions;

/// <summary>
/// The EPAs a STAR may be staged on at this review, each with the ladder its level must be a rung of: the committee
/// page's STAR picker. (T167)
/// </summary>
/// <remarks>
/// The list is <see cref="StarCurriculum.ListAsync" /> for the review's trainee, the same call
/// <c>StagePendingEntrustmentDecisionCommand</c> and <c>IssueEntrustmentDecisionCommand</c> enforce, so the picker
/// offers exactly what the gate accepts. It replaces two queries: <c>ListEpasForSubSpecialityQuery</c>, which listed
/// the EPAs in the CHAIR's scope rather than the trainee's curriculum, and <c>GetProgramScaleIdForReviewQuery</c>,
/// whose one programme scale is now each option's fallback. It climbs the review read ladder, like every other query on
/// the page (T101 finding E): it names a trainee's curriculum.
/// </remarks>
public sealed record ListStarEpaOptionsForReviewQuery(int ReviewId, ClaimsPrincipal Principal)
    : IRequest<IReadOnlyList<StarEpaOptionDto>>;

public sealed class ListStarEpaOptionsForReviewQueryValidator : AbstractValidator<ListStarEpaOptionsForReviewQuery>
{
    public ListStarEpaOptionsForReviewQueryValidator()
    {
        RuleFor(query => query.ReviewId).GreaterThan(0);
        RuleFor(query => query.Principal).NotNull();
    }
}

public sealed class ListStarEpaOptionsForReviewQueryHandler
    : IRequestHandler<ListStarEpaOptionsForReviewQuery, IReadOnlyList<StarEpaOptionDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public ListStarEpaOptionsForReviewQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<StarEpaOptionDto>> Handle(
        ListStarEpaOptionsForReviewQuery request,
        CancellationToken cancellationToken)
    {
        var review = await _dbContext.Set<CommitteeReview>()
            .AsNoTracking()
            .Include(entity => entity.Panel)
                .ThenInclude(panel => panel.Members)
            .SingleOrDefaultAsync(entity => entity.Id == request.ReviewId, cancellationToken)
            ?? throw new InvalidOperationException("The committee review could not be found.");

        CommitteeDecisionAuthorization.DemandReviewAccess(request.Principal, review);

        return await StarCurriculum.ListAsync(_dbContext, review.TraineeUserId, cancellationToken);
    }
}
