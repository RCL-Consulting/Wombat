using System.Security.Cryptography;
using System.Text.RegularExpressions;
using MimeKit;
using Wombat.Application.Common.Email;

namespace Wombat.Infrastructure.Email;

/// <summary>
/// How a log line names a mail: by its tags and a reference, never by an address it is sent to (ARCHITECTURE.md
/// § Logging & diagnostics; T282).
/// </summary>
/// <remarks>
/// <para>
/// Until T282 the mail worker logged "Email to {To}" on every send, retry and drop, beside the mail's tags. On an MSF
/// invitation or reminder the tags carry <c>campaign:N</c>, so for as long as logs were kept they named each respondent of
/// each campaign, and the reminders, which go only to those who have not answered, said who had not answered by when.
/// That undoes T207, which keeps a respondent's address away from everything that outlives the campaign.
/// </para>
/// <para>
/// A reference is drawn at random for each mail (<see cref="NewReference" />), so it says nothing about whom the mail went
/// to and there is nothing to reverse. It is the same on every line about one mail and on no line about another. A
/// reference derived from the address, even a keyed hash, would instead be the same on every mail to one person, and would
/// join a respondent's invitation and reminders to the mail the same person gets as a user of the app, which names them
/// no less than the address did. What kind of mail it was is in its tags.
/// </para>
/// <para>
/// A mail's subject is not logged either: an MSF mail's names the trainee, and a subject is free text that a template may
/// fill with anything.
/// </para>
/// </remarks>
public static class EmailLog
{
    /// <summary>What an exception's text says in place of an address the mail was sent to.</summary>
    public const string RedactedAddress = "[recipient]";

    /// <summary>
    /// A new mail's reference: twelve hexadecimal digits (48 random bits), enough to follow one mail through the log.
    /// </summary>
    public static string NewReference() => Convert.ToHexStringLower(RandomNumberGenerator.GetBytes(6));

    /// <summary>The mail's tags as one log value, or <c>(none)</c>.</summary>
    public static string DescribeTags(EmailMessage message)
        => message.Tags is { Count: > 0 } ? string.Join(", ", message.Tags) : "(none)";

    /// <summary>
    /// The shortest local part (<c>nurse-a</c> of <c>nurse-a@example.test</c>) redacted where it stands alone. A shorter
    /// one names nobody, and would take ordinary words out of the server's reason and the stack trace.
    /// </summary>
    internal const int ShortestRedactedLocalPart = 3;

    /// <summary>
    /// <paramref name="exception" /> as a log line may carry it about <paramref name="message" />: its whole text, inner
    /// exceptions and stack trace included, with every address the mail was sent to replaced by
    /// <see cref="RedactedAddress" />.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A mail server that refuses a recipient says so in words that usually quote the address ("5.1.1
    /// &lt;someone@example.com&gt;: Recipient address rejected"), and MailKit makes that reply the exception's message. So
    /// the worker never hands the logger an exception about a mail as it was thrown. What it logs keeps the server's
    /// reason, the exception's type and where it was thrown, which is what a failure is diagnosed by.
    /// </para>
    /// <para>
    /// A server quotes the address as it was sent, which for an international domain is its ASCII form
    /// (<c>xn--bcher-kva.example</c>), since MailKit encodes the domain for a server that does not take UTF-8; and some
    /// quote only the local part ("&lt;nurse-a&gt;... User unknown"), which beside <c>campaign:N</c> says as much. So each
    /// address is redacted as given, in both forms of its domain, and by its local part where that stands alone (T282
    /// review).
    /// </para>
    /// </remarks>
    public static Exception Redact(Exception exception, EmailMessage message)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentNullException.ThrowIfNull(message);

        var pattern = PatternFor(message);
        return new RedactedException(
            Redact(exception.Message, pattern),
            Redact(exception.ToString(), pattern),
            exception.StackTrace);
    }

    /// <summary>
    /// One pattern for every spelling of an address the mail was sent to that a text about it might quote: each header
    /// value as given, each mailbox it parses to with its domain in either form, and each mailbox's local part where no
    /// other character of an address adjoins it. Matched in one pass, longest first, so a display name's form is replaced
    /// before the address inside it, and nothing already replaced is matched again. Null when the mail names no address.
    /// Literals and fixed look-arounds only, so it cannot backtrack without bound, and it has no timeout to throw from the
    /// worker's failure path.
    /// </summary>
    private static Regex? PatternFor(EmailMessage message)
    {
        var addresses = new List<string>();
        var localParts = new List<string>();
        foreach (var header in new[] { message.To, message.Cc })
        {
            if (string.IsNullOrWhiteSpace(header))
            {
                continue;
            }

            addresses.Add(header.Trim());
            if (!InternetAddressList.TryParse(header, out var parsed))
            {
                continue;
            }

            foreach (var mailbox in parsed.Mailboxes)
            {
                addresses.Add(mailbox.Address);
                addresses.Add(Spelled(mailbox, idnEncode: true));
                addresses.Add(Spelled(mailbox, idnEncode: false));
                if (mailbox.LocalPart is { Length: >= ShortestRedactedLocalPart } localPart)
                {
                    localParts.Add(localPart);
                }
            }
        }

        var whole = Alternation(addresses);
        if (whole is null)
        {
            return null;
        }

        // Not inside a longer local part or a dotted name (Wombat.Infrastructure.Email), but a sentence's full stop, or
        // sendmail's "...", may follow it.
        var standingAlone = Alternation(localParts) is { } parts
            ? $"|(?<![A-Za-z0-9._%+@-])(?:{parts})(?![A-Za-z0-9_%+@-]|\\.[A-Za-z0-9_%+-])"
            : string.Empty;
        return new Regex(
            $"(?:{whole}){standingAlone}",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    }

    /// <summary>The mailbox's address with its domain in the given form, or as given if it has no other.</summary>
    private static string Spelled(MailboxAddress mailbox, bool idnEncode)
    {
        try
        {
            return mailbox.GetAddress(idnEncode);
        }
        catch (ArgumentException)
        {
            return mailbox.Address;
        }
    }

    private static string? Alternation(IEnumerable<string> values)
    {
        var escaped = values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(value => value.Length)
            .Select(Regex.Escape)
            .ToList();
        return escaped.Count == 0 ? null : string.Join('|', escaped);
    }

    private static string Redact(string text, Regex? pattern)
        => pattern is null ? text : pattern.Replace(text, RedactedAddress);

    /// <summary>
    /// What the log is handed in place of an exception about a mail. It renders as the original did, less the addresses,
    /// and chains nothing: the original, as an inner exception, would be rendered in full.
    /// </summary>
    private sealed class RedactedException(string message, string text, string? stackTrace) : Exception(message)
    {
        public override string? StackTrace => stackTrace;

        public override string ToString() => text;
    }
}
