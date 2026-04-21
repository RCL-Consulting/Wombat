using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.EntrustmentDecisions;

namespace Wombat.Application.Features.EntrustmentDecisions;

public sealed record ListExpiringDecisionsQuery(int WithinDays, DateOnly? AsOf = null) : IRequest<IReadOnlyList<EntrustmentDecisionDto>>;

public sealed class ListExpiringDecisionsQueryValidator : AbstractValidator<ListExpiringDecisionsQuery>
{
    public ListExpiringDecisionsQueryValidator()
    {
        RuleFor(query => query.WithinDays).GreaterThan(0).LessThanOrEqualTo(365);
    }
}

public sealed class ListExpiringDecisionsQueryHandler
    : IRequestHandler<ListExpiringDecisionsQuery, IReadOnlyList<EntrustmentDecisionDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public ListExpiringDecisionsQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<EntrustmentDecisionDto>> Handle(ListExpiringDecisionsQuery request, CancellationToken cancellationToken)
    {
        var today = request.AsOf ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var threshold = today.AddDays(request.WithinDays);

        var decisions = await _dbContext.Set<EntrustmentDecision>()
            .AsNoTracking()
            .Include(d => d.Epa)
            .Include(d => d.AuthorisedLevel)
            .Include(d => d.EvidenceLinks)
            .Where(d => d.Status == EntrustmentDecisionStatus.Active
                && d.ExpiresOn.HasValue
                && d.ExpiresOn.Value <= threshold
                && d.ExpiresOn.Value >= today)
            .OrderBy(d => d.ExpiresOn)
            .ToListAsync(cancellationToken);

        return decisions.Select(d => d.ToDto()).ToArray();
    }
}
