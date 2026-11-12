using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.DataRights.Queries;
using Wombat.Domain.Audit;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.Audit.Queries.ListAuditEntries;

public sealed class ListAuditEntriesQueryHandler : IRequestHandler<ListAuditEntriesQuery, PagedAuditResult>
{
    /// <summary>The window on arrival: the last 24 hours (T21).</summary>
    public static readonly TimeSpan DefaultWindow = TimeSpan.FromHours(24);

    /// <summary>PagerControls' largest page.</summary>
    public const int MaxPageSize = 100;

    private readonly IApplicationDbContext _dbContext;
    private readonly IUserAdministrationService _users;
    private readonly TimeProvider _clock;

    public ListAuditEntriesQueryHandler(IApplicationDbContext dbContext, IUserAdministrationService users, TimeProvider clock)
    {
        _dbContext = dbContext;
        _users = users;
        _clock = clock;
    }

    public async Task<PagedAuditResult> Handle(ListAuditEntriesQuery request, CancellationToken cancellationToken)
    {
        // The window as the caller fixed it, returned as read (T400, T21, T277): until T400 an absent window was "the last
        // 24 hours" from each request's own clock, so page 2 was read in a later window than page 1.
        var to = AsUtc(request.To) ?? _clock.GetUtcNow().UtcDateTime;
        var from = AsUtc(request.From) ?? to - DefaultWindow;
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, MaxPageSize);
        var everyInstitution = request.Principal.IsAdministrator();

        var query = _dbContext.Set<AuditEntry>()
            .Where(e => e.OccurredAt >= from && e.OccurredAt <= to);
        string? scopeName = null;

        if (!everyInstitution)
        {
            var scopedInstitutionId = request.Principal.GetInstitutionId();
            if (!scopedInstitutionId.HasValue)
            {
                return new PagedAuditResult([], 0, page, pageSize, from, to, false, []);
            }
            // A null InstitutionId is NOT "a global event everyone may read" — it is a row whose scope
            // is unknown, written by an actor with no institution claim (a global Administrator, a
            // background job). Admitting those to a scoped admin turned the audit log into a
            // cross-institution read channel: the pipeline stamped no institution on ANY command row,
            // so every InstitutionalAdmin could read every institution's command SummaryJson — raw
            // activity form data included — through this page. Unknown scope fails closed to
            // Administrator only; known scope is stamped by AuditPipelineBehavior. (T101)
            //
            // The Institution filter is the Administrator's alone (T400, K15): for an institution's administrator it is
            // ignored, never combined with this predicate, so no filter can widen it.
            var institutionId = scopedInstitutionId.Value;
            query = query.Where(e => e.InstitutionId == institutionId);

            // The subtitle names their own institution (T12): "Every consequential action at …".
            scopeName = await _dbContext.Set<Institution>()
                .AsNoTracking()
                .Where(i => i.Id == institutionId)
                .Select(i => i.Name)
                .FirstOrDefaultAsync(cancellationToken);
        }
        else if (request.Institution is { } institution)
        {
            query = institution.Kind switch
            {
                AuditInstitutionFilterKind.None => query.Where(e => e.InstitutionId == null),
                AuditInstitutionFilterKind.One when institution.InstitutionId is int one => query.Where(e => e.InstitutionId == one),
                _ => query,
            };
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

        // A page past the last (a hand-made address) reads the last, as Run history does; clamped upward too, so no page
        // number overflows the offset (the build review's N-4, N8).
        var lastPage = Math.Max(1, (int)Math.Ceiling(totalCount / (double)pageSize));
        page = Math.Clamp(page, 1, lastPage);

        // Newest first, ties settled by id, so a page boundary inside a tie neither repeats nor skips an entry (T262).
        var rows = await query
            .NewestFirst()
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
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
                e.InstitutionId,
            })
            .ToListAsync(cancellationToken);

        // Names and institutions are read for the rows on this page alone, which the caller may already read (T101): a
        // person by the one reader run history shares (M3, Q15), an institution by its name now (T19).
        var userIds = rows
            .Where(row => row.ActorUserId is not null)
            .Select(row => row.ActorUserId!)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var names = await DataRightsPeople.NamesOfAsync(_dbContext, _users, userIds, cancellationToken);

        var institutionIds = rows
            .Where(row => row.InstitutionId is not null)
            .Select(row => row.InstitutionId!.Value)
            .Distinct()
            .ToArray();
        var institutionNames = institutionIds.Length == 0
            ? new Dictionary<int, string>()
            : await _dbContext.Set<Institution>()
                .AsNoTracking()
                .Where(i => institutionIds.Contains(i.Id))
                .ToDictionaryAsync(i => i.Id, i => i.Name, cancellationToken);

        var items = rows
            .Select(row =>
            {
                var kind = AuditActors.KindOf(row.ActorUserId, row.Action);
                return new AuditLogRowDto(
                    row.Id,
                    row.OccurredAt,
                    row.Category,
                    row.Action,
                    row.Success,
                    row.ErrorMessage,
                    kind,
                    AuditActors.NameOf(kind, row.ActorUserId, row.ActorDisplay, names),
                    row.ActorUserId,
                    row.InstitutionId,
                    row.InstitutionId is int id && institutionNames.TryGetValue(id, out var name) ? name : null);
            })
            .ToList();

        IReadOnlyList<AuditInstitutionOptionDto> options = everyInstitution
            ? await _dbContext.Set<Institution>()
                .AsNoTracking()
                .OrderBy(i => i.Name)
                .Select(i => new AuditInstitutionOptionDto(i.Id, i.Name))
                .ToListAsync(cancellationToken)
            : [];

        return new PagedAuditResult(items, totalCount, page, pageSize, from, to, everyInstitution, options, scopeName);
    }

    // The column is timestamptz, which Npgsql writes only from a UTC DateTime: a moment of another kind is read as UTC.
    private static DateTime? AsUtc(DateTime? moment)
        => moment is { } value ? DateTime.SpecifyKind(value, DateTimeKind.Utc) : null;
}
