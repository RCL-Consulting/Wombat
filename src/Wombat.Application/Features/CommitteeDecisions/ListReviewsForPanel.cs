using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Application.Common.Users;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.CommitteeDecisions;

/// <summary>
/// The committee reviews page's list: each review the caller's panels, institution or global role let them see, with its
/// trainee's name, period, state and outcome.
/// </summary>
/// <remarks>
/// Someone who holds Trainee is listed no one's review, whatever other role they hold, the Administrator's included
/// (<see cref="TraineeScopeResolver.ActsAsTrainee" />, T185, T216). Each row names a trainee and the committee's outcome
/// for them, and the review itself refuses such a caller every review but their own once ratified
/// (<see cref="CommitteeDecisionAuthorization.DemandReviewAccess" />). Their own are on My committee reviews
/// (<see cref="ListReviewsForTraineeQuery" />).
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

        // The trainee rung first (T185, T216): a registrar who also administers, coordinates or sits on a committee
        // lists no peer's review, and no own one either, which a panel's list would show before it is ratified.
        if (TraineeScopeResolver.ActsAsTrainee(request.Principal))
        {
            return [];
        }

        var query = _dbContext.Set<CommitteeReview>()
            .AsNoTracking()
            .Include(review => review.Panel)
                .ThenInclude(panel => panel.Members)
            .Include(review => review.Decisions)
            .AsQueryable();

        // Administrator sees every panel's reviews; an InstitutionalAdmin sees reviews for
        // panels in their institution (Institution-scoped via InstitutionId, Speciality-scoped
        // via the Speciality's InstitutionId); committee members see panels they sit on. (T075)
        if (request.Principal.IsAdministrator())
        {
            // No filter — global view.
        }
        else if (request.Principal.IsInstitutionalAdmin())
        {
            var scopedInstitutionId = request.Principal.GetInstitutionId();
            if (!scopedInstitutionId.HasValue)
            {
                return Array.Empty<CommitteeReviewListItemDto>();
            }

            // Panels carry their own institution now; the speciality they cover is national (T091).
            query = query.Where(review => review.Panel.InstitutionId == scopedInstitutionId.Value);
        }
        else
        {
            var userId = CommitteeDecisionAuthorization.GetRequiredUserId(request.Principal);
            query = query.Where(review => review.Panel.Members.Any(member => member.UserId == userId));
        }

        var reviews = await query
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
}
