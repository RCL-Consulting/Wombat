using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Application.Common.Users;
using Wombat.Domain.CommitteeDecisions;

namespace Wombat.Application.Features.CommitteeDecisions;

/// <summary>
/// The committee reviews page's list: each review the caller may open, with its trainee's name, period, state and
/// outcome. Exactly the reviews the review's own read ladder admits them to
/// (<see cref="CommitteeDecisionAuthorization.ReadableReviewsAsync" />), so the list shows a review exactly when its Open
/// link will open it. (T218)
/// </summary>
/// <remarks>
/// <para>
/// So a Coordinator or an InstitutionalAdmin lists every review a panel of their institution holds, a SpecialityAdmin or
/// SubSpecialityAdmin the reviews of their own speciality's or sub-speciality's trainees at their institution (T182's
/// scope, read back over the reviews already scheduled), a committee member the reviews of the panels they sit on, and a
/// global Administrator every review. Until T218 the list had its own rule, which listed a Coordinator, a
/// SpecialityAdmin and a SubSpecialityAdmin only the reviews of panels they sat on: a Coordinator scheduled a review and
/// could not find it.
/// </para>
/// <para>
/// Someone who holds Trainee is listed no one's review, whatever other role they hold, the Administrator's included
/// (<see cref="TraineeScopeResolver.ActsAsTrainee" />, T185, T216). Each row names a trainee and the committee's outcome
/// for them, and the review itself refuses such a caller every review but their own once ratified
/// (<see cref="CommitteeDecisionAuthorization.DemandReviewAccessAsync" />). Their own are on My committee reviews
/// (<see cref="ListReviewsForTraineeQuery" />): the one place the list is narrower than the ladder.
/// </para>
/// </remarks>
public sealed record ListReviewsForPanelQuery(ClaimsPrincipal Principal) : IRequest<IReadOnlyList<CommitteeReviewListItemDto>>;

public sealed class ListReviewsForPanelQueryHandler : IRequestHandler<ListReviewsForPanelQuery, IReadOnlyList<CommitteeReviewListItemDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserAdministrationService _users;

    public ListReviewsForPanelQueryHandler(IApplicationDbContext dbContext, IUserAdministrationService users)
    {
        _dbContext = dbContext;
        _users = users;
    }

    public async Task<IReadOnlyList<CommitteeReviewListItemDto>> Handle(ListReviewsForPanelQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request.Principal);

        // The trainee rung first (T185, T216): a registrar who also administers, coordinates or sits on a committee lists
        // no peer's review, which the ladder refuses them anyway, and no own one either, which the ladder opens for them
        // once it is ratified. Their own are on My committee reviews; this is the one place the list is narrower than the
        // ladder.
        if (TraineeScopeResolver.ActsAsTrainee(request.Principal))
        {
            return [];
        }

        // The ladder decides each row, and a global Administrator's rung admits every review (the trainee rung, the only
        // one above it, has answered), so theirs are not loaded to be judged one by one. For anyone else the store is
        // asked only for the reviews the ladder could admit, and each is judged.
        var listed = _dbContext.Set<CommitteeReview>().AsNoTracking();
        if (!request.Principal.IsAdministrator())
        {
            var candidates = await Candidates(request.Principal)
                .AsNoTracking()
                .Include(review => review.Panel)
                    .ThenInclude(panel => panel.Members)
                .ToListAsync(cancellationToken);

            var readable = (await CommitteeDecisionAuthorization.ReadableReviewsAsync(
                    _dbContext, request.Principal, candidates, cancellationToken))
                .Select(review => review.Id)
                .ToArray();
            if (readable.Length == 0)
            {
                return [];
            }

            listed = listed.Where(review => readable.Contains(review.Id));
        }

        var reviews = await listed
            .OrderByDescending(review => review.ScheduledOn)
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

        // T142. The Trainee column by name, in one lookup for the reviews this caller may list.
        var names = await UserDisplayNames.ResolveAsync(
            _users, reviews.Select(review => review.TraineeUserId), cancellationToken);

        return reviews
            .Select(review => review with { TraineeName = names.NameOf(review.TraineeUserId) })
            .ToList();
    }

    /// <summary>
    /// The reviews the read ladder could admit this caller to, who is neither a global Administrator nor someone who holds
    /// Trainee: those of a panel they sit on, wherever it is, and, for a role the ladder reaches through the panel's
    /// institution (<see cref="CommitteeDecisionAuthorization.ReadsReviewsByInstitution" />), every review a panel of
    /// their institution holds. A superset of what the ladder admits, never the rule: the ladder judges each one. So a
    /// committee member alone is sent their panels' reviews, not every review at their hospital.
    /// </summary>
    private IQueryable<CommitteeReview> Candidates(ClaimsPrincipal principal)
    {
        var institutionId = CommitteeDecisionAuthorization.ReadsReviewsByInstitution(principal)
            ? principal.GetInstitutionId()
            : null;
        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return _dbContext.Set<CommitteeReview>().Where(review =>
            (institutionId != null && review.Panel.InstitutionId == institutionId) ||
            (userId != null && review.Panel.Members.Any(member => member.UserId == userId)));
    }
}
