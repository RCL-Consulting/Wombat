namespace Wombat.Domain.Audit;

/// <summary>
/// Append-only record of a consequential action taken in the system.
/// Id uses Guid.CreateVersion7() — time-sortable, eliminates the need for
/// secondary ordering on OccurredAt when paginating by PK.
/// </summary>
public sealed class AuditEntry
{
    private AuditEntry() { }

    public Guid Id { get; set; }
    public DateTime OccurredAt { get; set; }

    /// <summary>ASP.NET Core Identity user ID of the actor, or null for system actions.</summary>
    public string? ActorUserId { get; set; }

    /// <summary>Denormalized display name at write time; users may be renamed later.</summary>
    public string? ActorDisplay { get; set; }

    /// <summary>Truncated after 90 days: /24 for IPv4, /48 for IPv6.</summary>
    public string? ActorIpAddress { get; set; }

    public string? ActorUserAgent { get; set; }
    public AuditCategory Category { get; set; }

    /// <summary>Short action name, e.g. "RecordCommitteeDecisionCommand" or "Login".</summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>Domain aggregate type name, e.g. "CommitteeReview".</summary>
    public string? SubjectType { get; set; }

    public Guid? SubjectId { get; set; }
    public int? InstitutionId { get; set; }
    public int? SpecialityId { get; set; }

    /// <summary>
    /// Small redacted JSON payload describing the change (&lt;2 KB typical).
    /// Stored as jsonb. Sensitive fields are replaced with "[REDACTED]".
    /// </summary>
    public string SummaryJson { get; set; } = "{}";

    public bool Success { get; set; }

    /// <summary>The <see cref="ErrorMessage" /> column's width. <see cref="Create" /> truncates to it.</summary>
    public const int MaxErrorMessageLength = 2000;

    public string? ErrorMessage { get; set; }

    public static AuditEntry Create(
        DateTime occurredAt,
        AuditCategory category,
        string action,
        bool success,
        string? actorUserId = null,
        string? actorDisplay = null,
        string? actorIpAddress = null,
        string? actorUserAgent = null,
        string? subjectType = null,
        Guid? subjectId = null,
        int? institutionId = null,
        int? specialityId = null,
        string summaryJson = "{}",
        string? errorMessage = null)
    {
        // Bounded here, not by the caller (T122). The column is varchar(MaxErrorMessageLength), and the audit pipeline
        // writes this row from a failed command's catch: an overlong exception message made THAT save fail too, so the
        // caller saw a DbUpdateException in place of the real refusal, and no audit row was written at all.
        if (errorMessage is { Length: > MaxErrorMessageLength })
        {
            errorMessage = string.Concat(errorMessage.AsSpan(0, MaxErrorMessageLength - 1), "…");
        }

        return new AuditEntry
        {
            Id = Guid.CreateVersion7(occurredAt),
            OccurredAt = occurredAt,
            ActorUserId = actorUserId,
            ActorDisplay = actorDisplay,
            ActorIpAddress = actorIpAddress,
            ActorUserAgent = actorUserAgent,
            Category = category,
            Action = action,
            SubjectType = subjectType,
            SubjectId = subjectId,
            InstitutionId = institutionId,
            SpecialityId = specialityId,
            SummaryJson = summaryJson,
            Success = success,
            ErrorMessage = errorMessage
        };
    }
}
