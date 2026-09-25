using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.CommitteeDecisions;

/// <param name="ForScheduling">
/// True for the scheduling page's panel picker: only the panels this caller may put a trainee before
/// (<see cref="CommitteeTraineeScope.ListSchedulablePanelsAsync" />, T194). False for the panels page, which lists every
/// panel the caller may read.
/// </param>
public sealed record ListDecisionPanelsQuery(ClaimsPrincipal Principal, bool ForScheduling = false)
    : IRequest<IReadOnlyList<DecisionPanelSummaryDto>>;

public sealed class ListDecisionPanelsQueryHandler : IRequestHandler<ListDecisionPanelsQuery, IReadOnlyList<DecisionPanelSummaryDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserAdministrationService _users;

    public ListDecisionPanelsQueryHandler(IApplicationDbContext dbContext, IUserAdministrationService users)
    {
        _dbContext = dbContext;
        _users = users;
    }

    /// <remarks>
    /// <para>
    /// A global Administrator sees every panel. Everyone else sees the panels their own institution runs, and any
    /// panel they sit on (an External member may come from elsewhere). Until T182 only an InstitutionalAdmin was
    /// filtered (T063), so a Coordinator, SpecialityAdmin or CommitteeMember at one hospital listed every panel in the
    /// country, and the scheduling page offered them panels its handler would refuse.
    /// </para>
    /// <para>
    /// The scheduling page asks for <see cref="ListDecisionPanelsQuery.ForScheduling" />, which keeps only the panels on
    /// which the scheduling rule accepts at least one trainee from this caller that the trainee picker can name: the panel
    /// picker offers a panel exactly when its trainee picker would offer someone. Until T194 it read the whole list, so an
    /// External member from another institution was offered that panel with no trainee to choose, and a SpecialityAdmin
    /// another speciality's panel.
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

        if (request.ForScheduling)
        {
            var candidates = await panels.ToListAsync(cancellationToken);
            var schedulableIds = (await CommitteeTraineeScope.ListSchedulablePanelsAsync(
                    _dbContext, _users, request.Principal, candidates, cancellationToken))
                .Select(panel => panel.Id)
                .ToArray();

            if (schedulableIds.Length == 0)
            {
                return [];
            }

            panels = panels.Where(panel => schedulableIds.Contains(panel.Id));
        }

        return await panels
            .OrderBy(panel => panel.Name)
            .Select(panel => new DecisionPanelSummaryDto(
                panel.Id,
                panel.Name,
                panel.Scope,
                panel.InstitutionId,
                panel.SpecialityId,
                panel.Members.Count,
                panel.DecisionBodyKey,
                panel.DecisionBody == null ? null : panel.DecisionBody.Name))
            .ToListAsync(cancellationToken);
    }
}
