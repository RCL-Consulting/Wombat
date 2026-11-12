using System.Security.Claims;
using MediatR;

namespace Wombat.Application.Features.Audit.Queries.GetAuditEntryById;

/// <summary>One audit entry (T400, flow 18, Q9); null for an id that is unknown or out of the caller's scope (404, T101).</summary>
public sealed record GetAuditEntryByIdQuery(Guid Id, ClaimsPrincipal Principal) : IRequest<AuditEntryDetailDto?>;
