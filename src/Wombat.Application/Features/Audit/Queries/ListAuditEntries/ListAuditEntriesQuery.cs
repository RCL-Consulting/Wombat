using System.Security.Claims;
using MediatR;
using Wombat.Domain.Audit;

namespace Wombat.Application.Features.Audit.Queries.ListAuditEntries;

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
    int PageSize = 50) : IRequest<PagedAuditResult>;

public sealed record PagedAuditResult(
    IReadOnlyList<AuditEntryDto> Items,
    int TotalCount,
    int Page,
    int PageSize);
