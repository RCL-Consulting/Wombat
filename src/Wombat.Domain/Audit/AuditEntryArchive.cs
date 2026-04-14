namespace Wombat.Domain.Audit;

/// <summary>
/// Archival copy of an AuditEntry after the active retention window (2 years).
/// Same schema as AuditEntry but without the append-only database trigger.
/// Cold storage after 7 years is handled outside the application (see INFRASTRUCTURE.md).
/// </summary>
public sealed class AuditEntryArchive
{
    public Guid Id { get; set; }
    public DateTime OccurredAt { get; set; }
    public string? ActorUserId { get; set; }
    public string? ActorDisplay { get; set; }
    public string? ActorIpAddress { get; set; }
    public string? ActorUserAgent { get; set; }
    public AuditCategory Category { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? SubjectType { get; set; }
    public Guid? SubjectId { get; set; }
    public int? InstitutionId { get; set; }
    public int? SpecialityId { get; set; }
    public string SummaryJson { get; set; } = "{}";
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime ArchivedAt { get; set; }
}
