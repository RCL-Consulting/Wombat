using System.Security.Claims;
using MediatR;

namespace Wombat.Application.Features.Audit.Queries.GetAuditEntryById;

public sealed record GetAuditEntryByIdQuery(Guid Id, ClaimsPrincipal Principal) : IRequest<AuditEntryDto?>;
