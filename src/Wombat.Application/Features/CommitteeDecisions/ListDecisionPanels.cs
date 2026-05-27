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

    public async Task<IReadOnlyList<DecisionPanelSummaryDto>> Handle(ListDecisionPanelsQuery request, CancellationToken cancellationToken)
    {
        // T063: InstitutionalAdmin sees only panels in their institution. Filter resolves
        // Speciality-scoped panels via their Speciality.InstitutionId.
        if (request.Principal.IsInstitutionalAdmin() && !request.Principal.IsAdministrator())
        {
            var scopedInstitutionId = request.Principal.GetInstitutionId();
            if (!scopedInstitutionId.HasValue)
            {
                return Array.Empty<DecisionPanelSummaryDto>();
            }

            return await _dbContext.Set<DecisionPanel>()
                .AsNoTracking()
                .Include(panel => panel.Members)
                .Where(panel =>
                    (panel.Scope == DecisionPanelScope.Institution && panel.InstitutionId == scopedInstitutionId.Value) ||
                    (panel.Scope == DecisionPanelScope.Speciality && panel.SpecialityId.HasValue &&
                     _dbContext.Set<Speciality>().Any(speciality =>
                        speciality.Id == panel.SpecialityId.Value &&
                        speciality.InstitutionId == scopedInstitutionId.Value)))
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

        return await _dbContext.Set<DecisionPanel>()
            .AsNoTracking()
            .Include(panel => panel.Members)
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
