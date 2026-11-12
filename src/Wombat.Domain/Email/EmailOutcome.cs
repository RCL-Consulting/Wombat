namespace Wombat.Domain.Email;

/// <summary>
/// One mail the worker finished with: when, and whether it was sent or dropped (T400, flow 18, Q18, K3). Home's Mail line
/// reads the last 24 hours from these rows (the drops, the last drop and its refusal, the last sent), so either host's
/// mail is counted and nothing is kept in memory.
/// </summary>
/// <remarks>
/// No address, subject, body, reference or tag is a member, and none may become one (Q18): the row says that a mail
/// went or did not, never to whom or what. The refusal is the mail server's words as the worker redacted them
/// (<c>EmailLog.Redact</c>); null where there were none (a mail abandoned as the host stopped). Rows older than 90 days
/// are deleted by the run-record retention job.
/// </remarks>
public sealed class EmailOutcome
{
    /// <summary>The longest refusal kept; the rest is cut (the column's length).</summary>
    public const int RefusalMaxLength = 2000;

    private EmailOutcome()
    {
    }

    public long Id { get; private set; }

    /// <summary>When the worker finished with the mail, UTC.</summary>
    public DateTime OccurredAt { get; private set; }

    public EmailOutcomeKind Kind { get; private set; }

    /// <summary>The redacted refusal of a dropped mail, at most <see cref="RefusalMaxLength" /> characters; null when none.</summary>
    public string? Refusal { get; private set; }

    /// <summary>A mail the server took.</summary>
    public static EmailOutcome Sent(DateTime occurredAtUtc)
        => new() { OccurredAt = occurredAtUtc, Kind = EmailOutcomeKind.Sent };

    /// <summary>
    /// A mail given up on, with the server's refusal as the caller redacted it: trimmed, cut to
    /// <see cref="RefusalMaxLength" /> characters, and null when empty or white space.
    /// </summary>
    public static EmailOutcome Dropped(DateTime occurredAtUtc, string? refusal)
        => new() { OccurredAt = occurredAtUtc, Kind = EmailOutcomeKind.Dropped, Refusal = Kept(refusal) };

    private static string? Kept(string? refusal)
    {
        var trimmed = refusal?.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return null;
        }

        return trimmed.Length <= RefusalMaxLength ? trimmed : trimmed[..RefusalMaxLength];
    }
}
