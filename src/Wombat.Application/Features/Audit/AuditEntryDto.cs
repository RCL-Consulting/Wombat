using Wombat.Domain.Audit;

namespace Wombat.Application.Features.Audit;

public sealed record AuditEntryDto(
    Guid Id,
    DateTime OccurredAt,
    string? ActorUserId,
    string? ActorDisplay,
    string? ActorIpAddress,
    AuditCategory Category,
    string Action,
    string? SubjectType,
    Guid? SubjectId,
    int? InstitutionId,
    int? SpecialityId,
    string SummaryJson,
    bool Success,
    string? ErrorMessage);
