using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Curricula;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.CommitteeDecisions;

public sealed record GetDecisionPanelByIdQuery(int PanelId, ClaimsPrincipal Principal) : IRequest<DecisionPanelDetailDto?>;

public sealed class GetDecisionPanelByIdQueryHandler : IRequestHandler<GetDecisionPanelByIdQuery, DecisionPanelDetailDto?>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserAdministrationService _users;

    public GetDecisionPanelByIdQueryHandler(IApplicationDbContext dbContext, IUserAdministrationService users)
    {
        _dbContext = dbContext;
        _users = users;
    }

    public async Task<DecisionPanelDetailDto?> Handle(GetDecisionPanelByIdQuery request, CancellationToken cancellationToken)
    {
        var panel = await _dbContext.Set<DecisionPanel>()
            .AsNoTracking()
            .Include(entity => entity.Members)
            .Include(entity => entity.DecisionBody)
            .SingleOrDefaultAsync(entity => entity.Id == request.PanelId, cancellationToken);

        if (panel is null)
        {
            return null;
        }

        // The panel form's read: the panels the caller may change (T063, T182). Out of scope is null (404, not 403),
        // so the id's existence is not confirmed. Only an InstitutionalAdmin used to be checked; a SpecialityAdmin
        // could open any panel in the country and read its members, and until T131 slice 3 any speciality's at their
        // own hospital. Someone who holds Trainee opens none (T256): the reach is asked of the trainee rung first, and the
        // form says why from its options (DecisionPanelFormOptionsDto.TraineeNote).
        if (!await CommitteeDecisionAuthorization.MayAdministerPanelAsync(
                _dbContext, request.Principal, panel.InstitutionId, panel.Scope, panel.SpecialityId, cancellationToken))
        {
            return null;
        }

        var detail = DecisionPanelBodies.ToDetailDto(panel, panel.DecisionBody?.Name);

        // A panel's page names the panel and what it covers above its forms (T364, Q6; T260), and every member by name as
        // stored, so a member who can no longer sit is named in the warning, not counted (round 3 M6).
        var names = await _users.GetDisplayNamesAsync(
            panel.Members.Select(member => member.UserId).Distinct(StringComparer.Ordinal).ToArray(), cancellationToken);
        var institutionName = await _dbContext.Set<Institution>()
            .AsNoTracking()
            .Where(institution => institution.Id == panel.InstitutionId)
            .Select(institution => institution.Name)
            .FirstOrDefaultAsync(cancellationToken);
        var specialityName = panel.SpecialityId is int specialityId
            ? await _dbContext.Set<Speciality>()
                .AsNoTracking()
                .Where(speciality => speciality.Id == specialityId)
                .Select(speciality => speciality.Name)
                .FirstOrDefaultAsync(cancellationToken)
            : null;

        var openReviews = await OpenReviewsAsync(panel.Id, request.Principal, cancellationToken);
        var first = openReviews.FirstOrDefault();

        return detail with
        {
            Members = detail.Members
                .Select(member => member with { Name = names.TryGetValue(member.UserId, out var name) ? name : null })
                .ToArray(),
            InstitutionName = institutionName,
            SpecialityName = specialityName,
            OpenReviews = openReviews.Select(review => review.Dto).ToArray(),
            // The refusal Save committee would meet, said first (T364, F5): the same sentence, naming the same review.
            DecisionBodyLock = first is null
                ? null
                : SetDecisionPanelBodyCommandHandler.ReviewStillOpen(first.Dto.Id, first.Period, first.Dto.State)
        };
    }

    /// <summary>
    /// The reviews still open before the panel (<see cref="SetDecisionPanelBodyCommandHandler.OpenStates" />), oldest
    /// first, each marked with whether this reader may open it, by the review read ladder (T364, Q6).
    /// </summary>
    private async Task<IReadOnlyList<OpenReview>> OpenReviewsAsync(
        int panelId,
        ClaimsPrincipal principal,
        CancellationToken cancellationToken)
    {
        var reviews = await _dbContext.Set<CommitteeReview>()
            .AsNoTracking()
            .Include(review => review.Panel)
            .ThenInclude(reviewPanel => reviewPanel.Members)
            .Where(review => review.PanelId == panelId &&
                             SetDecisionPanelBodyCommandHandler.OpenStates.Contains(review.State))
            .OrderBy(review => review.Id)
            .ToListAsync(cancellationToken);

        if (reviews.Count == 0)
        {
            return [];
        }

        var readable = (await CommitteeReviewReadAccess.ReadableAsync(_dbContext, _users, principal, reviews, cancellationToken))
            .Select(review => review.Id)
            .ToHashSet();

        return reviews
            .Select(review =>
            {
                var period = review.Period;
                return new OpenReview(
                    new PanelOpenReviewDto(
                        review.Id,
                        period.ToString(),
                        review.State,
                        CommitteeDecisionWording.StateInSentence(review.State),
                        readable.Contains(review.Id)),
                    period);
            })
            .ToArray();
    }

    private sealed record OpenReview(PanelOpenReviewDto Dto, AcademicPeriod Period);
}
