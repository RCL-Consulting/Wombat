namespace Wombat.Domain.Audit;

/// <summary>
/// Append-only record of a consequential action taken in the system.
/// Order entries by OccurredAt. Id is no substitute: it is a Guid.CreateVersion7(), ordered only to the millisecond with
/// the rest random, so two entries written in the same millisecond sort by Id either way (T244). OccurredAt can tie as
/// well: two requests can start in the same clock tick, and Postgres keeps only microseconds. So a reader that pages, or
/// stops at a count, must order by OccurredAt then Id. The order within a tie is then arbitrary, but the same on every read.
/// </summary>
public sealed class AuditEntry
{
    private AuditEntry() { }

    public Guid Id { get; set; }
    public DateTime OccurredAt { get; set; }

    /// <summary>ASP.NET Core Identity user ID of the actor, or null for system actions.</summary>
    public string? ActorUserId { get; set; }

    /// <summary>The <see cref="ActorDisplay" /> column's width. <see cref="Create" /> truncates to it.</summary>
    public const int MaxActorDisplayLength = 200;

    /// <summary>Denormalized display name at write time; users may be renamed later.</summary>
    public string? ActorDisplay { get; set; }

    /// <summary>Truncated after 90 days: /24 for IPv4, /48 for IPv6.</summary>
    public string? ActorIpAddress { get; set; }

    /// <summary>The <see cref="ActorUserAgent" /> column's width. <see cref="Create" /> truncates to it.</summary>
    public const int MaxActorUserAgentLength = 500;

    /// <summary>The request's User-Agent header, as long as the client chooses to send it.</summary>
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
        // Bounded here, not by the caller (T122, T208). Each column is varchar of its constant's width, and one value
        // longer than its column fails the whole save the row rides on.
        //
        // The audit pipeline writes a failed command's row from its catch: an overlong exception message made THAT save
        // fail too, so the caller saw a DbUpdateException in place of the real refusal, and nothing was audited (T122).
        // A User-Agent header or a display name over its column did the same to any row it was on. After a command that
        // had succeeded, the work had committed, the caller saw EF's error, and there was no row. The header's length is
        // the client's to choose, and a display name is joined from a user's names or taken from a claim (T208).
        return new AuditEntry
        {
            Id = Guid.CreateVersion7(occurredAt),
            OccurredAt = occurredAt,
            ActorUserId = actorUserId,
            ActorDisplay = Truncate(actorDisplay, MaxActorDisplayLength),
            ActorIpAddress = actorIpAddress,
            ActorUserAgent = Truncate(actorUserAgent, MaxActorUserAgentLength),
            Category = category,
            Action = action,
            SubjectType = subjectType,
            SubjectId = subjectId,
            InstitutionId = institutionId,
            SpecialityId = specialityId,
            SummaryJson = summaryJson,
            Success = success,
            ErrorMessage = Truncate(errorMessage, MaxErrorMessageLength)
        };
    }

    /// <summary>
    /// <paramref name="value" /> as it is when it fits in <paramref name="maxLength" /> characters; otherwise cut, and
    /// ended with an ellipsis, to fit. The cut never falls inside a surrogate pair: Npgsql cannot encode the lone half it
    /// would leave as UTF-8, and refuses the save for that instead of the length. A width of one keeps only the ellipsis.
    /// Internal, not private, so a width no column has today can be tested.
    /// </summary>
    internal static string? Truncate(string? value, int maxLength)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxLength, 1);

        if (value is null || value.Length <= maxLength)
        {
            return value;
        }

        var kept = maxLength - 1;
        if (kept > 0 && char.IsHighSurrogate(value[kept - 1]))
        {
            kept--;
        }

        return string.Concat(value.AsSpan(0, kept), "…");
    }
}
