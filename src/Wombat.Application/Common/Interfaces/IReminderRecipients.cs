namespace Wombat.Application.Common.Interfaces;

/// <summary>
/// The accounts a staff member's reminder about one waiting request could be written to, as the periodic reminders read
/// them (T358, flow 06; review 2): the Application side of Infrastructure's <c>ReminderRecipientPolicy</c> projection,
/// which stays internal there with the digests' rule unchanged.
/// </summary>
/// <remarks>
/// What may be sent is <c>ReminderRecipientRules.RefusalFor</c>: a missing account, a deactivated one (an administrator's
/// lock or an erasure, never a brute-force lockout) and a missing address refuse a reminder; opting out of digest emails
/// does not, since a reminder is "email about one particular thing" (E1).
/// </remarks>
public interface IReminderRecipients
{
    /// <summary>The accounts <paramref name="userIds" /> name, keyed by id. An id that names no account is absent.</summary>
    Task<IReadOnlyDictionary<string, ReminderRecipientDto>> LoadAsync(
        IEnumerable<string> userIds,
        CancellationToken cancellationToken);
}

/// <summary>An account as a reminder judges and greets it (T358).</summary>
/// <param name="IsDeactivated">An administrator's lock or an erasure (<c>UserDeactivation</c>); not a brute-force lockout.</param>
public sealed record ReminderRecipientDto(
    string UserId,
    string? Email,
    string FirstName,
    string LastName,
    bool IsDeactivated);
