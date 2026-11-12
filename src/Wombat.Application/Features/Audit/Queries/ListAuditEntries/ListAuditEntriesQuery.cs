using System.Security.Claims;
using MediatR;
using Wombat.Domain.Audit;

namespace Wombat.Application.Features.Audit.Queries.ListAuditEntries;

/// <summary>
/// The audit log's page (T400, flow 18, Q8). <paramref name="From" /> and <paramref name="To" /> are the window the caller
/// fixed (T21: on arrival the last 24 hours, or as Show resolved it) and are returned as read, so every page of one window
/// is read between the same two moments; either left null is resolved here from the clock, To as now and From as To less
/// 24 hours.
/// </summary>
/// <param name="ActorUserId">The Person filter: a user id, exactly (T3).</param>
/// <param name="Action">Part of an action's name (Contains).</param>
/// <param name="Institution">The Administrator's Institution filter (K15); ignored for anyone else. Null is every row.</param>
public sealed record ListAuditEntriesQuery(
    ClaimsPrincipal Principal,
    string? ActorUserId = null,
    AuditCategory? Category = null,
    string? Action = null,
    string? SubjectType = null,
    DateTime? From = null,
    DateTime? To = null,
    bool? SuccessOnly = null,
    int Page = 1,
    int PageSize = 50,
    AuditInstitutionFilter? Institution = null) : IRequest<PagedAuditResult>;

/// <summary>
/// One page of the log, with the window it was read in (T21) and, for the Administrator, the institutions the Institution
/// filter offers (T19).
/// </summary>
/// <param name="From">The window's start, UTC, as read.</param>
/// <param name="To">The window's end, UTC, as read.</param>
/// <param name="EveryInstitution">
/// The caller reads every institution's rows and those with none (the Administrator): the rows name their institution, and
/// the Institution filter is offered.
/// </param>
/// <param name="Institutions">The Institution filter's institutions, by name; empty for anyone but the Administrator.</param>
/// <param name="ScopeName">
/// An institution's administrator's institution, by name, for the subtitle (T12); null for the Administrator.
/// </param>
public sealed record PagedAuditResult(
    IReadOnlyList<AuditLogRowDto> Items,
    int TotalCount,
    int Page,
    int PageSize,
    DateTime From,
    DateTime To,
    bool EveryInstitution,
    IReadOnlyList<AuditInstitutionOptionDto> Institutions,
    string? ScopeName = null);
