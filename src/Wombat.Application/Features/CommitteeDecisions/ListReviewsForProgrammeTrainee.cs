using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Domain.CommitteeDecisions;

namespace Wombat.Application.Features.CommitteeDecisions;

/// <summary>
/// One registrar's committee reviews, in any state, that the caller may open, newest scheduled first: the registrar page's
/// Committee reviews section (T358, flow 06, C9). Each is a link to the review, so each is listed exactly when the review's
/// own read ladder opens it (<see cref="CommitteeDecisionAuthorization.ReadableReviewsAsync" />): a committee member reads
/// the reviews of the panels they sit on, a Coordinator their institution's.
/// </summary>
/// <remarks>
/// Unlike <see cref="ListReviewsForTraineeQuery" />, which shows a trainee only what has been decided and told to them, this
/// is the staff's view and lists scheduled and in-progress reviews too. Someone in the programme as a trainee reads no
/// peer's reviews here (T185), as on the committee reviews page; empty, never a refusal.
/// </remarks>
public sealed record ListReviewsForProgrammeTraineeQuery(ClaimsPrincipal Principal, string TraineeUserId)
    : IRequest<IReadOnlyList<CommitteeReviewListItemDto>>;

public sealed class ListReviewsForProgrammeTraineeQueryValidator : AbstractValidator<ListReviewsForProgrammeTraineeQuery>
{
    public ListReviewsForProgrammeTraineeQueryValidator()
    {
        RuleFor(query => query.Principal).NotNull();
    }
}

public sealed class ListReviewsForProgrammeTraineeQueryHandler
    : IRequestHandler<ListReviewsForProgrammeTraineeQuery, IReadOnlyList<CommitteeReviewListItemDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserAdministrationService _users;

    public ListReviewsForProgrammeTraineeQueryHandler(IApplicationDbContext dbContext, IUserAdministrationService users)
    {
        _dbContext = dbContext;
        _users = users;
    }

    public async Task<IReadOnlyList<CommitteeReviewListItemDto>> Handle(
        ListReviewsForProgrammeTraineeQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.TraineeUserId) || TraineeScopeResolver.ActsAsTrainee(request.Principal))
        {
            return [];
        }

        // One registrar's reviews are few, so every one is loaded and the ladder judges each, the Administrator's too.
        var candidates = await _dbContext.Set<CommitteeReview>()
            .AsNoTracking()
            .Include(review => review.Panel)
                .ThenInclude(panel => panel.Members)
            .Where(review => review.TraineeUserId == request.TraineeUserId)
            .ToListAsync(cancellationToken);
        if (candidates.Count == 0)
        {
            return [];
        }

        var readable = (await CommitteeDecisionAuthorization.ReadableReviewsAsync(
                _dbContext, _users, request.Principal, candidates, cancellationToken))
            .Select(review => review.Id)
            .ToArray();
        if (readable.Length == 0)
        {
            return [];
        }

        return await _dbContext.Set<CommitteeReview>()
            .AsNoTracking()
            .Where(review => readable.Contains(review.Id))
            .OrderByDescending(review => review.ScheduledOn)
            .ThenByDescending(review => review.Id)
            .Select(review => new CommitteeReviewListItemDto(
                review.Id,
                review.TraineeUserId,
                review.PanelId,
                review.Panel.Name,
                review.ReviewPeriodFrom,
                review.ReviewPeriodTo,
                review.ScheduledOn,
                review.State,
                review.Decisions.OrderByDescending(decision => decision.DecidedOn).Select(decision => (CommitteeDecisionCategory?)decision.Category).FirstOrDefault(),
                review.RatifiedOn,
                review.IsFormative,
                review.ReviewType)
            {
                AcademicYear = review.AcademicYear,
                Semester = review.Semester
            })
            .ToListAsync(cancellationToken);
    }
}
