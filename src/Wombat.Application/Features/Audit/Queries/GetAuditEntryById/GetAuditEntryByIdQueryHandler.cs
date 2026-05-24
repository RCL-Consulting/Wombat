using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Audit;
using Wombat.Domain.Audit;

namespace Wombat.Application.Features.Audit.Queries.GetAuditEntryById;

public sealed class GetAuditEntryByIdQueryHandler : IRequestHandler<GetAuditEntryByIdQuery, AuditEntryDto?>
{
    private readonly IApplicationDbContext _dbContext;

    public GetAuditEntryByIdQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AuditEntryDto?> Handle(GetAuditEntryByIdQuery request, CancellationToken cancellationToken)
    {
        var entry = await _dbContext.Set<AuditEntry>()
            .Where(e => e.Id == request.Id)
            .Select(e => new AuditEntryDto(
                e.Id,
                e.OccurredAt,
                e.ActorUserId,
                e.ActorDisplay,
                e.ActorIpAddress,
                e.Category,
                e.Action,
                e.SubjectType,
                e.SubjectId,
                e.InstitutionId,
                e.SpecialityId,
                e.SummaryJson,
                e.Success,
                e.ErrorMessage))
            .FirstOrDefaultAsync(cancellationToken);

        if (entry is null)
        {
            return null;
        }

        if (request.Principal.IsAdministrator())
        {
            return entry;
        }

        // InstitutionalAdmin: visible if no institution (global event) or matches their scope.
        if (entry.InstitutionId is null)
        {
            return entry;
        }
        return request.Principal.CanAccessInstitution(entry.InstitutionId.Value) ? entry : null;
    }
}
