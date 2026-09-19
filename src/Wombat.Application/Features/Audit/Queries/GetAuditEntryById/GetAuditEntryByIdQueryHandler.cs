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

        // A row with no institution has unknown scope, not global scope. Letting a scoped admin open
        // it was the single worst leak in the product: command rows were all unstamped, and SummaryJson
        // is rendered raw on AuditDetail, so /admin/audit/{id} handed an InstitutionalAdmin in
        // institution A the clinical form submissions of institution B's trainees. Unknown scope is now
        // Administrator-only. Null (404), not an exception (403), so the id's existence is not confirmed.
        // See ListAuditEntriesQueryHandler for the matching list-side predicate. (T101)
        return entry.InstitutionId is int scopedInstitutionId
            && request.Principal.CanAccessInstitution(scopedInstitutionId)
                ? entry
                : null;
    }
}
