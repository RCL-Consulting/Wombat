using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.EntrustmentDecisions;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.EntrustmentDecisions;

public sealed record ListEntrustmentDecisionsForAdminQuery(
    string? TraineeUserIdFilter,
    EntrustmentDecisionStatus? StatusFilter,
    ClaimsPrincipal Principal) : IRequest<IReadOnlyList<EntrustmentDecisionDto>>;

public sealed class ListEntrustmentDecisionsForAdminQueryValidator : AbstractValidator<ListEntrustmentDecisionsForAdminQuery>
{
    public ListEntrustmentDecisionsForAdminQueryValidator()
    {
        RuleFor(query => query.Principal).NotNull();
    }
}

public sealed class ListEntrustmentDecisionsForAdminQueryHandler
    : IRequestHandler<ListEntrustmentDecisionsForAdminQuery, IReadOnlyList<EntrustmentDecisionDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public ListEntrustmentDecisionsForAdminQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<EntrustmentDecisionDto>> Handle(ListEntrustmentDecisionsForAdminQuery request, CancellationToken cancellationToken)
    {
        DemandAdminAccess(request.Principal);

        var query = _dbContext.Set<EntrustmentDecision>()
            .AsNoTracking()
            .Include(d => d.Epa)
            .Include(d => d.AuthorisedLevel)
            .Include(d => d.EvidenceLinks)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.TraineeUserIdFilter))
        {
            query = query.Where(d => d.TraineeUserId == request.TraineeUserIdFilter);
        }

        if (request.StatusFilter.HasValue)
        {
            query = query.Where(d => d.Status == request.StatusFilter.Value);
        }

        var decisions = await query
            .OrderByDescending(d => d.IssuedOn)
            .ThenByDescending(d => d.Id)
            .ToListAsync(cancellationToken);

        return decisions.Select(d => d.ToDto()).ToArray();
    }

    private static void DemandAdminAccess(ClaimsPrincipal principal)
    {
        if (principal.IsInRole(WombatRoles.Administrator) ||
            principal.IsInRole(WombatRoles.InstitutionalAdmin) ||
            principal.IsInRole(WombatRoles.SpecialityAdmin) ||
            principal.IsInRole(WombatRoles.SubSpecialityAdmin))
        {
            return;
        }

        throw new UnauthorizedAccessException("Only institutional administrators can list entrustment decisions.");
    }
}
