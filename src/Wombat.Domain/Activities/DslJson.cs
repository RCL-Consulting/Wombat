using System.Text.Encodings.Web;
using System.Text.Json;

namespace Wombat.Domain.Activities;

/// <summary>What the three DSL parsers share in how they write and refuse JSON (T398, flow 17).</summary>
internal static class DslJson
{
    /// <summary>
    /// The writer a <c>Serialize</c> writes through. Compact, as every stored payload has always been written; or laid out
    /// for the builder's JSON editor (Q13, S11): one property a line, two spaces, a line feed, in the same key order, and
    /// with an apostrophe or an accent written as itself rather than escaped, since a person reads it. Indented text is
    /// never stored: <c>ActivityType.SaveDraft</c> writes the compact form, and the two parse to the same thing.
    /// </summary>
    public static Utf8JsonWriter Writer(Stream stream, bool indented)
        => indented
            ? new Utf8JsonWriter(stream, new JsonWriterOptions
            {
                Indented = true,
                IndentSize = 2,
                IndentCharacter = ' ',
                NewLine = "\n",
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            })
            : new Utf8JsonWriter(stream);

    /// <summary>
    /// A payload that is not JSON at all, at the line the reader stopped on, counted from 1 (T398, Q6, S11:
    /// <see cref="JsonException.LineNumber" /> counts from 0); no line when the reader gave none. The sentence is the
    /// parser's message, unchanged.
    /// </summary>
    public static RefusalPlace Malformed(RefusalTab tab, string message, JsonException exception)
        => new(tab, RefusalKind.MalformedJson, message)
        {
            JsonLine = exception.LineNumber is long line ? line + 1 : null
        };
}
