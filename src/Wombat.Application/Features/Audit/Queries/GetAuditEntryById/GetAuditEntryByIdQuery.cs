using MediatR;

namespace Wombat.Application.Features.Audit.Queries.GetAuditEntryById;

public sealed record GetAuditEntryByIdQuery(Guid Id) : IRequest<AuditEntryDto?>;
