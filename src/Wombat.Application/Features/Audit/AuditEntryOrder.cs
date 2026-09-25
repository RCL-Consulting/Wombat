using Wombat.Domain.Audit;

namespace Wombat.Application.Features.Audit;

/// <summary>
/// The one order for a reader of audit entries that pages or stops at a count: newest first, and within one timestamp
/// by id (T262).
/// </summary>
/// <remarks>
/// Two entries can share an <see cref="AuditEntry.OccurredAt" />: two requests can start in the same clock tick, and
/// PostgreSQL keeps only microseconds. Ordered by the timestamp alone, the entries in a tie come back in whatever order
/// the server's sort leaves them, and that order can differ from one query to the next. So the audit log's page 2 could
/// repeat entries page 1 had shown and leave out others, as <c>AuditEntryOrderPostgresTests</c> saw on PostgreSQL, and
/// the access report's cut at 500 could keep a different 500 on each run. The id settles every tie the same way on
/// every read. It is no order of its own: it is a version-7 GUID, ordered only to the millisecond, so within a tie its
/// order is arbitrary, but fixed (see <see cref="AuditEntry" />). Used by <c>ListAuditEntriesQueryHandler</c> and
/// <c>AccessReportBuilder</c>.
/// </remarks>
public static class AuditEntryOrder
{
    public static IOrderedQueryable<AuditEntry> NewestFirst(this IQueryable<AuditEntry> entries)
        => entries
            .OrderByDescending(entry => entry.OccurredAt)
            .ThenByDescending(entry => entry.Id);
}
