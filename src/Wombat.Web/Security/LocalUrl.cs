using System.Buffers;
using System.Text;

namespace Wombat.Web.Security;

/// <summary>
/// Whether an address a request carries is a path on this site, judged as <c>LocalRedirect</c> judges one, so a caller can
/// drop one that is not rather than have <c>LocalRedirect</c> throw on it; and, when it is, the address in a form a
/// response header can carry.
/// </summary>
/// <remarks>
/// <para>
/// <c>Uri.TryCreate(url, UriKind.Relative, …)</c> is not this test: it accepts <c>//evil.example</c>, which a browser
/// follows to another host, and <c>/\evil.example</c>, which some browsers read the same way. (T335, flow 01; first
/// written for <see cref="SessionEnd" />.)
/// </para>
/// <para>
/// <b>A character above U+007E is percent-encoded</b>, as the UTF-8 bytes a browser itself sends for it (the review of the
/// t335 branch). A return address arrives decoded from its query or form field, so <c>?returnUrl=%2F%C3%A9</c> is
/// <c>/é</c> here. <c>LocalRedirect</c> put that in the <c>Location</c> header as it was, and Kestrel refuses any header
/// value outside printable ASCII: the answer was a 500, and at sign-in it came after the password had been checked and
/// the sign-in audited. Encoded rather than refused, because such an address is an ordinary one (a list filtered by
/// "José"): the browser is sent to the same page it asked for, and nothing about which site it is changes, since the
/// characters that decide that, the first two, are ASCII already. Every caller shares it: the sign-in, the institutional
/// sign-in's callback, the link page, the acting role's switch, the session's end, the error page's Try again and
/// Access denied's sign-in link.
/// </para>
/// </remarks>
public static class LocalUrl
{
    /// <summary>
    /// <paramref name="url" /> when it is a path on this site: it starts with one slash, not two and not a slash and a
    /// backslash, and carries no control character; with every character above U+007E percent-encoded as UTF-8. Null for
    /// anything else, an absolute address and a broken character (half a surrogate pair) included.
    /// </summary>
    public static string? OrNull(string? url)
    {
        if (string.IsNullOrEmpty(url) || url[0] != '/')
        {
            return null;
        }

        if (url.Length > 1 && (url[1] == '/' || url[1] == '\\'))
        {
            return null;
        }

        return url.Any(char.IsControl) ? null : Ascii(url);
    }

    /// <summary><paramref name="url" /> with each character above U+007E percent-encoded as UTF-8; null for a broken one.</summary>
    private static string? Ascii(string url)
    {
        if (url.All(character => character <= '~'))
        {
            return url;
        }

        var ascii = new StringBuilder(url.Length + 16);
        Span<byte> bytes = stackalloc byte[4];
        var index = 0;
        while (index < url.Length)
        {
            if (url[index] <= '~')
            {
                ascii.Append(url[index]);
                index++;
                continue;
            }

            if (Rune.DecodeFromUtf16(url.AsSpan(index), out var rune, out var consumed) != OperationStatus.Done)
            {
                return null;
            }

            var written = rune.EncodeToUtf8(bytes);
            foreach (var octet in bytes[..written])
            {
                ascii.Append('%').Append(octet.ToString("X2", System.Globalization.CultureInfo.InvariantCulture));
            }

            index += consumed;
        }

        return ascii.ToString();
    }
}
