using Wombat.Application.Common.Interfaces;
using Wombat.Infrastructure.Identity;

namespace Wombat.Infrastructure.Scheduling;

/// <summary>
/// <see cref="IReminderRecipients" /> over <see cref="ReminderRecipientPolicy" />'s projection (T358, flow 06; review 2):
/// one query for the accounts a waiting list names, read the way the periodic reminders read them, so a staff member's
/// reminder and the nightly nudge cannot disagree about who an account is or whether it is deactivated
/// (<see cref="UserDeactivation" />).
/// </summary>
/// <remarks>
/// It reports the account; it does not judge it. The digests' rule (<see cref="ReminderRecipientPolicy.SkipReasonFor" />)
/// also skips an opt-out, which a reminder about one request does not (E1), so the rule a reminder applies is the
/// Application's <c>ReminderRecipientRules</c>, and the opt-out is not carried across at all.
/// </remarks>
internal sealed class ReminderRecipients : IReminderRecipients
{
    private readonly IApplicationDbContext _dbContext;

    public ReminderRecipients(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyDictionary<string, ReminderRecipientDto>> LoadAsync(
        IEnumerable<string> userIds,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(userIds);

        var loaded = await ReminderRecipientPolicy.LoadAsync(_dbContext, userIds, cancellationToken);
        return loaded.Values.ToDictionary(
            recipient => recipient.Id,
            recipient => new ReminderRecipientDto(
                recipient.Id,
                recipient.Email,
                recipient.FirstName,
                recipient.LastName,
                UserDeactivation.IsDeactivated(recipient.LockoutEnd)),
            StringComparer.Ordinal);
    }
}
