using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.DataRights.Queries;
using Wombat.Domain.Audit;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.Audit.Queries.GetAuditEntryById;

public sealed class GetAuditEntryByIdQueryHandler : IRequestHandler<GetAuditEntryByIdQuery, AuditEntryDetailDto?>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserAdministrationService _users;

    public GetAuditEntryByIdQueryHandler(IApplicationDbContext dbContext, IUserAdministrationService users)
    {
        _dbContext = dbContext;
        _users = users;
    }

    public async Task<AuditEntryDetailDto?> Handle(GetAuditEntryByIdQuery request, CancellationToken cancellationToken)
    {
        var entry = await _dbContext.Set<AuditEntry>()
            .AsNoTracking()
            .Where(e => e.Id == request.Id)
            .Select(e => new
            {
                e.Id,
                e.OccurredAt,
                e.Category,
                e.Action,
                e.Success,
                e.ErrorMessage,
                e.ActorUserId,
                e.ActorDisplay,
                e.ActorIpAddress,
                e.InstitutionId,
                e.SubjectType,
                e.SubjectId,
                e.SpecialityId,
                e.SummaryJson,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (entry is null)
        {
            return null;
        }

        var administrator = request.Principal.IsAdministrator();

        // A row with no institution has unknown scope, not global scope. Letting a scoped admin open
        // it was the single worst leak in the product: command rows were all unstamped, and SummaryJson
        // is rendered raw on AuditDetail, so /admin/audit/{id} handed an InstitutionalAdmin in
        // institution A the clinical form submissions of institution B's trainees. Unknown scope is now
        // Administrator-only. Null (404), not an exception (403), so the id's existence is not confirmed.
        // See ListAuditEntriesQueryHandler for the matching list-side predicate. (T101)
        //
        // Decided before any name is read (T400): a person's or an institution's name is read only for a row the caller
        // may already read.
        if (!administrator &&
            !(entry.InstitutionId is int scopedInstitutionId && request.Principal.CanAccessInstitution(scopedInstitutionId)))
        {
            return null;
        }

        // Who by the one reader the log and run history share (M3, Q15): an erased person by their pseudonym, never by the
        // stored display, which keeps the name they had.
        var kind = AuditActors.KindOf(entry.ActorUserId, entry.Action);
        var names = await DataRightsPeople.NamesOfAsync(
            _dbContext,
            _users,
            entry.ActorUserId is null ? [] : [entry.ActorUserId],
            cancellationToken);

        var institutionName = entry.InstitutionId is int institutionId
            ? await _dbContext.Set<Institution>().AsNoTracking()
                .Where(i => i.Id == institutionId).Select(i => i.Name).FirstOrDefaultAsync(cancellationToken)
            : null;
        var specialityName = entry.SpecialityId is int specialityId
            ? await _dbContext.Set<Speciality>().AsNoTracking()
                .Where(s => s.Id == specialityId).Select(s => s.Name).FirstOrDefaultAsync(cancellationToken)
            : null;

        return new AuditEntryDetailDto(
            entry.Id,
            entry.OccurredAt,
            entry.Category,
            entry.Action,
            entry.Success,
            entry.ErrorMessage,
            kind,
            AuditActors.NameOf(kind, entry.ActorUserId, entry.ActorDisplay, names),
            entry.ActorUserId,
            entry.ActorIpAddress,
            entry.InstitutionId,
            institutionName,
            entry.SubjectType,
            entry.SubjectId,
            entry.SpecialityId,
            specialityName,
            // The payload is the Administrator's alone (R2-E01): it may carry what an institution's administrator is not
            // shown, a decision's rationale among it (R2-E04). Withheld by the query, not only left unrendered by the page.
            administrator ? entry.SummaryJson : null);
    }
}
