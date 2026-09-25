using System.Globalization;
using Wombat.Domain.MultiSourceFeedback;

namespace Wombat.Domain.Invitations;

public sealed class Invitation
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// The hash of the link's token: the only thing stored of it. A resend replaces it (T283), so the link it replaces
    /// matches nothing and is refused as invalid.
    /// </summary>
    public string TokenHash { get; set; } = string.Empty;

    public string TargetRole { get; set; } = string.Empty;

    /// <summary>
    /// The institution this invitation scopes the invitee to. Null only for a
    /// <c>CollegeAdmin</c> invitation, which is scoped to a <see cref="CollegeId"/> instead. (T093)
    /// </summary>
    public int? InstitutionId { get; set; }

    /// <summary>
    /// The national College this invitation scopes the invitee to. Set only for a
    /// <c>CollegeAdmin</c> invitation; null for every institution-scoped role. (T093)
    /// </summary>
    public int? CollegeId { get; set; }

    public int? SpecialityId { get; set; }
    public int? SubSpecialityId { get; set; }
    public string IssuedByUserId { get; set; } = string.Empty;

    /// <summary>
    /// When the current link was issued: when the invitation was issued, then when a resend replaced its link (T283). The
    /// hour a link's mail may go unreported is counted from it (<see cref="DeliveryReportDeadline" />).
    /// </summary>
    public DateTime IssuedOn { get; set; }

    public DateOnly ExpiresOn { get; set; }
    public DateTime? UsedOn { get; set; }
    public DateTime? RevokedOn { get; set; }

    /// <summary>
    /// When the mail carrying the current link was accepted by the mail server; null when none has been reported sent.
    /// (T283)
    /// </summary>
    /// <remarks>
    /// A send is a hand-off to an in-process queue, and the mail worker delivers it after the request, retrying three
    /// times before it gives up (T251). Until T283 nothing recorded what became of an account invitation's mail, so an
    /// invitation issued while the mail server was down read as issued and nothing more, and its invitee never heard of
    /// it. Written only by the worker's report (<c>AccountInvitationDeliveryRecorder</c>), which names the link it is about
    /// by its hash, so a report about a link a resend has replaced changes nothing. The link is always stored before its
    /// mail is handed over, so its report never arrives first.
    /// </remarks>
    public DateTime? SentOn { get; set; }

    /// <summary>
    /// When the mail carrying the current link was given up on: the worker's last retry failed, or the app stopped with it
    /// still queued. Null otherwise; never set with <see cref="SentOn" />. A resend clears it. (T283)
    /// </summary>
    public DateTime? DeliveryFailedOn { get; set; }

    /// <summary>
    /// How many of this invitation's mails the mail server was offered and the worker then gave up on, across every link it
    /// has held; a resend keeps the count. (T283)
    /// </summary>
    /// <remarks>
    /// An address the mail server always refuses (a mistyped domain) stays not delivered however often it is resent, and
    /// the product cannot tell that from a mail server that was down. So once the count reaches
    /// <see cref="FailuresBeforeAddressCheck" />, the invitations list says to check the address. A mail that was never
    /// reported (the app crashed), and one dropped still queued as the app stopped, before any attempt, are not counted:
    /// nothing says the address was at fault. Each still reads as not delivered. (T283 review)
    /// </remarks>
    public int DeliveryFailures { get; set; }

    /// <summary>How long a link works: an issued or resent link expires this many days after it was issued.</summary>
    public const int LinkLifetimeDays = 14;

    /// <summary>
    /// How many failed mails make the invitations list suggest the address may be wrong: the second, since one failure is
    /// as likely the mail server's. (T283)
    /// </summary>
    public const int FailuresBeforeAddressCheck = 2;

    /// <summary>
    /// How long a link's mail may go unreported before it counts as not delivered: the app stopped without reporting it,
    /// so it is not coming. The same hour as an MSF link's, for the same reasons (<see cref="MsfInvitation.DeliveryReportDeadline" />).
    /// (T283)
    /// </summary>
    public static TimeSpan DeliveryReportDeadline => MsfInvitation.DeliveryReportDeadline;

    /// <summary>The last day a link issued at <paramref name="utcNow" /> works.</summary>
    public static DateOnly LinkExpiresOn(DateTime utcNow) => DateOnly.FromDateTime(utcNow.Date.AddDays(LinkLifetimeDays));

    /// <summary>What became of the current link's mail, as of <paramref name="utcNow" />. (T283)</summary>
    public InvitationDelivery DeliveryAt(DateTime utcNow) => DeliveryOf(SentOn, DeliveryFailedOn, IssuedOn, utcNow);

    /// <summary>
    /// What became of a link's mail: sent once the worker said so; not delivered once it said it gave up, or once nothing
    /// was heard of it for <see cref="DeliveryReportDeadline" />; otherwise still being sent. (T283)
    /// </summary>
    /// <remarks>
    /// Whether the invitation can still be used is asked apart from this: the list shows only those that can, and the
    /// resend refuses the rest before it asks this.
    /// </remarks>
    public static InvitationDelivery DeliveryOf(DateTime? sentOn, DateTime? deliveryFailedOn, DateTime issuedOn, DateTime utcNow)
    {
        if (sentOn is not null)
        {
            return InvitationDelivery.Sent;
        }

        return deliveryFailedOn is not null || issuedOn < utcNow - DeliveryReportDeadline
            ? InvitationDelivery.NotDelivered
            : InvitationDelivery.BeingSent;
    }

    /// <summary>
    /// Whether the invitations list should say to check the address: the current link was not delivered, and at least
    /// <see cref="FailuresBeforeAddressCheck" /> of the invitation's mails were given up on. (T283)
    /// </summary>
    public static bool SuggestsCheckingAddress(InvitationDelivery delivery, int deliveryFailures)
        => delivery == InvitationDelivery.NotDelivered && deliveryFailures >= FailuresBeforeAddressCheck;

    // ─── What became of a link's mail (T283) ────────────────────────────────

    private const string DeliveryKeyPrefix = "account-invitation:";

    /// <summary>The length of a <see cref="TokenHash" />: a SHA-256 in hex.</summary>
    private const int TokenHashLength = 64;

    /// <summary>
    /// The key an account invitation's mail carries (<c>EmailMessage.DeliveryKey</c>), which the mail worker hands back
    /// with the mail's outcome: this invitation, and the link the mail carries, by its hash. (T283)
    /// </summary>
    /// <remarks>
    /// The hash, not a number the invitation counts: two resends that race each read the same count, and would name two
    /// links alike. The hash is stored already, and the mail the key travels with holds the token itself. The key is never
    /// a tag and never logged.
    /// </remarks>
    public static string DeliveryKey(int invitationId, string tokenHash)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(invitationId);
        if (!IsTokenHash(tokenHash))
        {
            throw new ArgumentException("A token hash is 64 hexadecimal digits.", nameof(tokenHash));
        }

        return string.Create(CultureInfo.InvariantCulture, $"{DeliveryKeyPrefix}{invitationId}:{tokenHash}");
    }

    /// <summary>The invitation and link a <see cref="DeliveryKey" /> names; false for any other key, or none. (T283)</summary>
    public static bool TryReadDeliveryKey(string? key, out int invitationId, out string tokenHash)
    {
        invitationId = 0;
        tokenHash = string.Empty;

        if (key is null || !key.StartsWith(DeliveryKeyPrefix, StringComparison.Ordinal))
        {
            return false;
        }

        var parts = key[DeliveryKeyPrefix.Length..].Split(':');
        if (parts.Length != 2 ||
            !int.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out var id) ||
            id <= 0 ||
            !IsTokenHash(parts[1]))
        {
            return false;
        }

        invitationId = id;
        tokenHash = parts[1];
        return true;
    }

    private static bool IsTokenHash(string? value)
        => value is { Length: TokenHashLength } && value.All(char.IsAsciiHexDigit);
}

/// <summary>What became of the mail carrying an account invitation's current link. (T283)</summary>
public enum InvitationDelivery
{
    /// <summary>Handed over and not yet reported on, within the hour.</summary>
    BeingSent,

    /// <summary>The mail server accepted it.</summary>
    Sent,

    /// <summary>The worker gave up on it, or nothing was heard of it within the hour.</summary>
    NotDelivered
}
