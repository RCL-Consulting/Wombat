using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.CommitteeDecisions;

public sealed record GetDecisionPanelByIdQuery(int PanelId, ClaimsPrincipal Principal) : IRequest<DecisionPanelDetailDto?>;

public sealed class GetDecisionPanelByIdQueryHandler : IRequestHandler<GetDecisionPanelByIdQuery, DecisionPanelDetailDto?>
{
    private readonly IApplicationDbContext _dbContext;

    public GetDecisionPanelByIdQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DecisionPanelDetailDto?> Handle(GetDecisionPanelByIdQuery request, CancellationToken cancellationToken)
    {
        var panel = await _dbContext.Set<DecisionPanel>()
            .AsNoTracking()
            .Include(entity => entity.Members)
            .SingleOrDefaultAsync(entity => entity.Id == request.PanelId, cancellationToken);

        if (panel is null)
        {
            return null;
        }

        // T063: scope guard — InstitutionalAdmin can only see panels in their institution.
        // Out-of-scope id returns null (404, not 403) to avoid leaking other-institution ids.
        if (request.Principal.IsInstitutionalAdmin() && !request.Principal.IsAdministrator())
        {
            var resolvedInstitutionId = await ResolveInstitutionIdAsync(panel, cancellationToken);
            if (resolvedInstitutionId.HasValue && !request.Principal.CanAccessInstitution(resolvedInstitutionId.Value))
            {
                return null;
            }
        }

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

    private async Task<int?> ResolveInstitutionIdAsync(DecisionPanel panel, CancellationToken cancellationToken)
    {
        return panel.Scope switch
        {
            DecisionPanelScope.Institution => panel.InstitutionId,
            DecisionPanelScope.Speciality when panel.SpecialityId.HasValue => await _dbContext.Set<Speciality>()
                .Where(speciality => speciality.Id == panel.SpecialityId.Value)
                .Select(speciality => (int?)speciality.InstitutionId)
                .SingleOrDefaultAsync(cancellationToken),
            _ => null
        };
    }
}
