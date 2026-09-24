using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.CommitteeDecisions;

public sealed record ListDecisionPanelsQuery(ClaimsPrincipal Principal) : IRequest<IReadOnlyList<DecisionPanelSummaryDto>>;

public sealed class ListDecisionPanelsQueryHandler : IRequestHandler<ListDecisionPanelsQuery, IReadOnlyList<DecisionPanelSummaryDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public ListDecisionPanelsQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <remarks>
    /// <para>
    /// A global Administrator sees every panel. Everyone else sees the panels their own institution runs, and any
    /// panel they sit on (an External member may come from elsewhere). Until T182 only an InstitutionalAdmin was
    /// filtered (T063), so a Coordinator, SpecialityAdmin or CommitteeMember at one hospital listed every panel in the
    /// country, and the scheduling page offered them panels its handler would refuse.
    /// </para>
    /// <para>
    /// The scheduling page reads this list for its panel picker. A panel at the caller's institution is one the
    /// scheduling rule can accept, since it demands that institution of the panel, the trainee and the caller alike; a
    /// panel the caller only sits on elsewhere lists no trainee for them.
    /// </para>
    /// </remarks>
    public async Task<IReadOnlyList<DecisionPanelSummaryDto>> Handle(ListDecisionPanelsQuery request, CancellationToken cancellationToken)
    {
        var panels = _dbContext.Set<DecisionPanel>().AsNoTracking();

        if (!request.Principal.IsAdministrator())
        {
            var institutionId = request.Principal.GetInstitutionId();
            var userId = request.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            panels = panels.Where(panel =>
                (institutionId != null && panel.InstitutionId == institutionId) ||
                (userId != null && panel.Members.Any(member => member.UserId == userId)));
        }

        return await panels
            .OrderBy(panel => panel.Name)
            .Select(panel => new DecisionPanelSummaryDto(
                panel.Id,
                panel.Name,
                panel.Scope,
                panel.InstitutionId,
                panel.SpecialityId,
                panel.Members.Count))
            .ToListAsync(cancellationToken);
    }
}
