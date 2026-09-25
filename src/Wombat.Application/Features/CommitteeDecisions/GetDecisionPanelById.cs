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

        return DecisionPanelBodies.ToDetailDto(panel, panel.DecisionBody?.Name);
    }
}
