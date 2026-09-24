namespace Wombat.Infrastructure.Identity;

/// <summary>
/// What "deactivated" means for an account: locked indefinitely, as an administrator's lock and an erasure leave it,
/// as opposed to a brute-force lockout that lifts itself after minutes. (T102)
/// </summary>
/// <remarks>
/// <para>
/// There is no active flag on a user. <see cref="UserAdministrationService" />'s lock and the erasure executor both
/// write <see cref="IndefiniteLockoutEnd" />, and Identity's failed-password lockout writes the same column with a time
/// a few minutes out, so "locked out right now" cannot tell them apart. Treating any current lockout as deactivation
/// would let anyone make an assessor un-nominable for a quarter of an hour by typing five wrong passwords at their
/// account.
/// </para>
/// <para>
/// The test is a threshold rather than equality with <see cref="DateTimeOffset.MaxValue" />, so it holds whether the
/// stored value comes back exactly, truncated to PostgreSQL's microseconds, or as <c>infinity</c>. It is a constant
/// rather than "now plus a margin", so it translates to the same SQL every time and does not depend on the configured
/// lockout duration.
/// </para>
/// </remarks>
internal static class UserDeactivation
{
    /// <summary>What an administrator's lock and an erasure write to <c>LockoutEnd</c>.</summary>
    public static readonly DateTimeOffset IndefiniteLockoutEnd = DateTimeOffset.MaxValue;

    /// <summary>A <c>LockoutEnd</c> at or after this is a deactivation; anything earlier lifts by itself.</summary>
    public static readonly DateTimeOffset Threshold = new(9000, 1, 1, 0, 0, 0, TimeSpan.Zero);

    public static bool IsDeactivated(DateTimeOffset? lockoutEnd) => lockoutEnd is { } end && end >= Threshold;
}
