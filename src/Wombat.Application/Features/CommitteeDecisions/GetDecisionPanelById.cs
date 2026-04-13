using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.CommitteeDecisions;

namespace Wombat.Application.Features.CommitteeDecisions;

public sealed record GetDecisionPanelByIdQuery(int PanelId) : IRequest<DecisionPanelDetailDto>;

public sealed class GetDecisionPanelByIdQueryHandler : IRequestHandler<GetDecisionPanelByIdQuery, DecisionPanelDetailDto>
{
    private readonly IApplicationDbContext _dbContext;

    public GetDecisionPanelByIdQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DecisionPanelDetailDto> Handle(GetDecisionPanelByIdQuery request, CancellationToken cancellationToken)
    {
        var panel = await _dbContext.Set<DecisionPanel>()
            .AsNoTracking()
            .Include(entity => entity.Members)
            .SingleOrDefaultAsync(entity => entity.Id == request.PanelId, cancellationToken)
            ?? throw new InvalidOperationException("The decision panel could not be found.");

        return new DecisionPanelDetailDto(
            panel.Id,
            panel.Name,
            panel.Scope,
            panel.InstitutionId,
            panel.SpecialityId,
            panel.Members
                .OrderBy(member => member.Role)
                .ThenBy(member => member.UserId)
                .Select(member => new DecisionPanelMemberDto(member.Id, member.UserId, member.Role))
                .ToArray());
    }
}
