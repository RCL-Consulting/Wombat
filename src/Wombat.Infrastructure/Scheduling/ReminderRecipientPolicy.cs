using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Infrastructure.Identity;
using Wombat.Infrastructure.Scheduling.Jobs;

namespace Wombat.Infrastructure.Scheduling;

/// <summary>
/// Whom a periodic reminder may be written to: the one policy every scheduled job that mails an account holder a
/// summary applies before it writes (D50, T151, T240).
/// </summary>
/// <remarks>
/// <para>
/// Three jobs call it: the assessor nudge (<see cref="AssessorPendingNudgeJob" />), the weekly coordinator digest
/// (<see cref="WeeklyCoordinatorDigestJob" />) and the draft reminder (<see cref="ActivityDraftNudgeJob" />). Each is a
/// periodic, unsolicited summary, so each is a digest email in the sense of the T026 objection flag
/// (<see cref="WombatIdentityUser.OptOutOfDigestEmails" />), whatever its subject line says. A user who ticked "Opt out
/// of digest emails" is sent none of them.
/// </para>
/// <para>
/// A deactivated account (<see cref="UserDeactivation" />: an administrator's lock or an erasure) is never written to. A
/// brute-force lockout is not a deactivation and lifts itself, so that person is still written to: treating it as one
/// would let anyone silence someone's reminders by typing five wrong passwords at their account.
/// </para>
/// <para>
/// One reason each, the first that applies in <see cref="ReminderSkipReason" />'s order, so a person is counted once:
/// a locked account that had also opted out is counted as deactivated. An erased account is deactivated, opted out and
/// without an address all at once. Where a job still reaches it by its real id (the assessor nudge, because an
/// activity's data keeps the nominee's id) it is counted once, as deactivated. Erasure writes a pseudonym into an
/// activity's subject, though (<see cref="Wombat.Infrastructure.DataRights.ErasureExecutor" />), and a pseudonym names
/// no account, so the draft reminder counts an erased trainee's drafts as <see cref="ReminderSkipReason.UnknownUser" />.
/// A job with reasons of its own asks them after this, so a reason about the account always wins over one about what
/// the job would say. Each job logs one line per run counting its skips by reason, under the same property names:
/// <c>DeactivatedCount</c>, <c>OptedOutCount</c>, <c>NoEmailCount</c>, and <c>UnknownUserCount</c> where a stored id
/// can name no account.
/// </para>
/// <para>
/// Mail that is not a summary to an account holder does not call it. The MSF invitation reminder writes to respondents
/// by address, not to accounts.
/// </para>
/// <para>
/// <b>A known gap, not a decision:</b> the entrustment decision expiry job (<see cref="EntrustmentDecisionExpiryJob" />)
/// also writes to trainees, and it checks only for a missing address, so a trainee an administrator locked is still told
/// that a decision on their record has expired or is about to. T240 named only the three jobs above and left it. That
/// email is about one decision, not a digest, so closing the gap means skipping a deactivated account there without
/// honouring the opt-out. An erased trainee is not written to, because erasure writes a pseudonym into the decision's
/// trainee id.
/// </para>
/// </remarks>
internal static class ReminderRecipientPolicy
{
    /// <summary>What the policy and a reminder's greeting read of an account, as one query projects it.</summary>
    public static readonly Expression<Func<WombatIdentityUser, ReminderRecipient>> Projection =
        user => new ReminderRecipient(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.LockoutEnd,
            user.OptOutOfDigestEmails);

    private static readonly Func<WombatIdentityUser, ReminderRecipient> Project = Projection.Compile();

    /// <summary>The same projection, of an account already loaded (as <c>UserManager</c> hands them back).</summary>
    public static ReminderRecipient From(WombatIdentityUser user) => Project(user);

    /// <summary>The accounts <paramref name="userIds" /> name, keyed by id. An id that names no account is absent.</summary>
    public static async Task<IReadOnlyDictionary<string, ReminderRecipient>> LoadAsync(
        IApplicationDbContext dbContext,
        IEnumerable<string> userIds,
        CancellationToken cancellationToken)
    {
        var ids = userIds.Distinct(StringComparer.Ordinal).ToArray();
        if (ids.Length == 0)
        {
            return new Dictionary<string, ReminderRecipient>(StringComparer.Ordinal);
        }

        return await dbContext.Set<WombatIdentityUser>()
            .AsNoTracking()
            .Where(user => ids.Contains(user.Id))
            .Select(Projection)
            .ToDictionaryAsync(recipient => recipient.Id, StringComparer.Ordinal, cancellationToken);
    }

    /// <summary>
    /// Why <paramref name="recipient" /> is not written to, or null when they may be. Null means the account exists and
    /// has an address, so a caller may read <see cref="ReminderRecipient.Email" /> as set.
    /// </summary>
    public static ReminderSkipReason? SkipReasonFor(ReminderRecipient? recipient) => recipient switch
    {
        null => ReminderSkipReason.UnknownUser,
        _ when UserDeactivation.IsDeactivated(recipient.LockoutEnd) => ReminderSkipReason.Deactivated,
        { OptOutOfDigestEmails: true } => ReminderSkipReason.OptedOut,
        _ when string.IsNullOrWhiteSpace(recipient.Email) => ReminderSkipReason.NoEmail,
        _ => null
    };
}

/// <summary>An account as <see cref="ReminderRecipientPolicy" /> judges it and a reminder greets it.</summary>
internal sealed record ReminderRecipient(
    string Id,
    string? Email,
    string FirstName,
    string LastName,
    DateTimeOffset? LockoutEnd,
    bool OptOutOfDigestEmails);

/// <summary>Why <see cref="ReminderRecipientPolicy" /> skips an account, in the order it asks.</summary>
internal enum ReminderSkipReason
{
    /// <summary>The stored id names no account.</summary>
    UnknownUser,

    /// <summary>An administrator's lock or an erasure (<see cref="UserDeactivation" />).</summary>
    Deactivated,

    /// <summary>The user opted out of digest emails (T026).</summary>
    OptedOut,

    /// <summary>Nothing to write to.</summary>
    NoEmail
}

/// <summary>A run's skips, counted by reason, for its one log line.</summary>
internal sealed class SkipTally<TReason>
    where TReason : struct, Enum
{
    private readonly Dictionary<TReason, int> _counts = [];

    public int this[TReason reason] => _counts.GetValueOrDefault(reason);

    public void Add(TReason reason) => _counts[reason] = this[reason] + 1;
}
