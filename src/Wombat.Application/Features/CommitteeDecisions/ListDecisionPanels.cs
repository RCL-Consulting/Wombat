using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.CommitteeDecisions;

namespace Wombat.Application.Features.CommitteeDecisions;

public sealed record ListDecisionPanelsQuery() : IRequest<IReadOnlyList<DecisionPanelSummaryDto>>;

public sealed class ListDecisionPanelsQueryHandler : IRequestHandler<ListDecisionPanelsQuery, IReadOnlyList<DecisionPanelSummaryDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public ListDecisionPanelsQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<DecisionPanelSummaryDto>> Handle(ListDecisionPanelsQuery request, CancellationToken cancellationToken)
    {
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
