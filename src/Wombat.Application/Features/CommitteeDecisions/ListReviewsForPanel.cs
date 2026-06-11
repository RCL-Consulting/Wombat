using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.CommitteeDecisions;

public sealed record ListReviewsForPanelQuery(ClaimsPrincipal Principal) : IRequest<IReadOnlyList<CommitteeReviewListItemDto>>;

public sealed class ListReviewsForPanelQueryHandler : IRequestHandler<ListReviewsForPanelQuery, IReadOnlyList<CommitteeReviewListItemDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public ListReviewsForPanelQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<CommitteeReviewListItemDto>> Handle(ListReviewsForPanelQuery request, CancellationToken cancellationToken)
    {
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

        return await query
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
                review.ReviewType))
            .ToListAsync(cancellationToken);
    }
}
