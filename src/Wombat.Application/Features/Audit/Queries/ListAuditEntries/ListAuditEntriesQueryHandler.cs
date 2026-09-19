using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Audit;

namespace Wombat.Application.Features.Audit.Queries.ListAuditEntries;

public sealed class ListAuditEntriesQueryHandler : IRequestHandler<ListAuditEntriesQuery, PagedAuditResult>
{
    private readonly IApplicationDbContext _dbContext;

    public ListAuditEntriesQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedAuditResult> Handle(ListAuditEntriesQuery request, CancellationToken cancellationToken)
    {
        var from = request.From ?? DateTime.UtcNow.AddHours(-24);
        var to = request.To ?? DateTime.UtcNow;

        var query = _dbContext.Set<AuditEntry>()
            .Where(e => e.OccurredAt >= from && e.OccurredAt <= to);

        if (!request.Principal.IsAdministrator())
        {
            var scopedInstitutionId = request.Principal.GetInstitutionId();
            if (!scopedInstitutionId.HasValue)
            {
                return new PagedAuditResult(Array.Empty<AuditEntryDto>(), 0, request.Page, request.PageSize);
            }
            // A null InstitutionId is NOT "a global event everyone may read" — it is a row whose scope
            // is unknown, written by an actor with no institution claim (a global Administrator, a
            // background job). Admitting those to a scoped admin turned the audit log into a
            // cross-institution read channel: the pipeline stamped no institution on ANY command row,
            // so every InstitutionalAdmin could read every institution's command SummaryJson — raw
            // activity form data included — through this page. Unknown scope fails closed to
            // Administrator only; known scope is stamped by AuditPipelineBehavior. (T101)
            var institutionId = scopedInstitutionId.Value;
            query = query.Where(e => e.InstitutionId == institutionId);
        }

        if (!string.IsNullOrWhiteSpace(request.ActorUserId))
            query = query.Where(e => e.ActorUserId == request.ActorUserId);

        if (request.Category is not null)
            query = query.Where(e => e.Category == request.Category.Value);

        if (!string.IsNullOrWhiteSpace(request.Action))
            query = query.Where(e => e.Action.Contains(request.Action));

        if (!string.IsNullOrWhiteSpace(request.SubjectType))
            query = query.Where(e => e.SubjectType == request.SubjectType);

        if (request.SuccessOnly is true)
            query = query.Where(e => e.Success);
        else if (request.SuccessOnly is false)
            query = query.Where(e => !e.Success);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(e => e.OccurredAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
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
            .ToListAsync(cancellationToken);

        return new PagedAuditResult(items, totalCount, request.Page, request.PageSize);
    }
}
