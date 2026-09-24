using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;

namespace Wombat.Infrastructure.Tests.Reporting;

/// <summary>
/// The text a reader of a PDF gets back from it (search, copy, a screen reader): every glyph a page's content streams
/// show, mapped through its font's <c>/ToUnicode</c> CMap. (T200)
/// </summary>
/// <remarks>
/// <para>
/// A PDF draws glyph ids, not characters. The characters come only from the ToUnicode map, so a PDF whose maps are wrong
/// prints perfectly and reads as nothing. That is the failure T200 is about: in a render that overlapped another, every
/// Lato glyph but one mapped to U+0000. Rendering the same document again as SVG, as the T161 section tests do, says
/// nothing about the maps in a PDF already produced; the PDF itself has to be read.
/// </para>
/// <para>
/// Only what QuestPDF (Skia's PDF backend) writes is read: plain objects, Flate streams with a direct <c>/Length</c>,
/// <c>Tf</c>/<c>Tj</c>/<c>TJ</c> text, form XObjects. Anything else throws rather than reading as empty, so a change in
/// the writer fails a test loudly instead of passing it vacuously. A glyph mapped to U+0000 reads as <c>\0</c>; a glyph
/// with no mapping reads as U+FFFD.
/// </para>
/// </remarks>
internal static partial class PdfTextLayer
{
    /// <summary>The text of each page, in page order; each <c>BT</c>…<c>ET</c> block ends a line.</summary>
    public static IReadOnlyList<string> Pages(byte[] pdf)
    {
        var objects = ReadObjects(pdf);
        var catalog = objects.Values.Single(o => TypeIs(o.Dictionary, "Catalog"));
        var pages = new List<PdfObject>();
        CollectPages(objects, objects[Reference(catalog.Dictionary, "Pages")], pages);

        return pages.Select(page => PageText(objects, page)).ToList();
    }

    private static string PageText(Dictionary<int, PdfObject> objects, PdfObject page)
    {
        var text = new StringBuilder();
        var resources = Resolve(objects, page.Dictionary, "Resources");
        foreach (var contents in References(page.Dictionary, "Contents"))
        {
            ShowText(objects, objects[contents].Stream(), resources, text);
        }

        return text.ToString();
    }

    private static void ShowText(Dictionary<int, PdfObject> objects, byte[] content, string resources, StringBuilder text)
    {
        var fonts = NamedReferences(objects, resources, "Font")
            .ToDictionary(pair => pair.Key, pair => Font.Read(objects, objects[pair.Value]));
        var xObjects = NamedReferences(objects, resources, "XObject");

        Font? font = null;
        var operands = new List<object>();
        foreach (var token in Tokens(content))
        {
            if (token is not Operator op)
            {
                operands.Add(token);
                continue;
            }

            switch (op.Name)
            {
                case "Tf":
                    var name = ((Name)operands[^2]).Value;
                    font = fonts.TryGetValue(name, out var named)
                        ? named
                        : throw new InvalidDataException($"Font /{name} is not in the resources.");
                    break;
                case "Tj" or "'" or "\"":
                    Show(font, (byte[])operands[^1], text);
                    break;
                case "TJ":
                    foreach (var shown in ((List<object>)operands[^1]).OfType<byte[]>())
                    {
                        Show(font, shown, text);
                    }

                    break;
                case "ET":
                    text.Append('\n');
                    break;
                case "Do" when xObjects.TryGetValue(((Name)operands[^1]).Value, out var xObjectId)
                    && TypeIs(objects[xObjectId].Dictionary, "Form", key: "Subtype"):
                    var form = objects[xObjectId];
                    var formResources = form.Dictionary.Contains("/Resources", StringComparison.Ordinal)
                        ? Resolve(objects, form.Dictionary, "Resources")
                        : resources;
                    ShowText(objects, form.Stream(), formResources, text);
                    break;
            }

            operands.Clear();
        }
    }

    private static void Show(Font? font, byte[] codes, StringBuilder text)
    {
        if (font is null)
        {
            throw new InvalidDataException("Text is shown before a font is selected.");
        }

        for (var i = 0; i < codes.Length; i += font.CodeLength)
        {
            var code = font.CodeLength == 2 ? (codes[i] << 8) | codes[i + 1] : codes[i];
            text.Append(font.ToUnicode.TryGetValue(code, out var mapped) ? mapped : "\uFFFD");
        }
    }

    private sealed record Font(int CodeLength, IReadOnlyDictionary<int, string> ToUnicode)
    {
        public static Font Read(Dictionary<int, PdfObject> objects, PdfObject font)
            => new(
                // A composite (Type0) font shows two-byte codes (Identity-H); a simple or Type3 font one-byte ones.
                TypeIs(font.Dictionary, "Type0", key: "Subtype") ? 2 : 1,
                font.Dictionary.Contains("/ToUnicode", StringComparison.Ordinal)
                    ? ParseCMap(Encoding.Latin1.GetString(objects[Reference(font.Dictionary, "ToUnicode")].Stream()))
                    : new Dictionary<int, string>());
    }

    // ─── ToUnicode CMap ─────────────────────────────────────────────────────────

    private static Dictionary<int, string> ParseCMap(string cmap)
    {
        var map = new Dictionary<int, string>();
        foreach (Match section in BfCharSection().Matches(cmap))
        {
            foreach (Match pair in HexPair().Matches(section.Groups[1].Value))
            {
                map[HexNumber(pair.Groups[1].Value)] = Utf16(pair.Groups[2].Value);
            }
        }

        foreach (Match section in BfRangeSection().Matches(cmap))
        {
            foreach (Match range in HexRange().Matches(section.Groups[1].Value))
            {
                var low = HexNumber(range.Groups[1].Value);
                var high = HexNumber(range.Groups[2].Value);
                if (range.Groups[3].Success)
                {
                    // <lo> <hi> <dst>: consecutive codes, consecutive destinations (the last code unit increments).
                    var destination = Utf16(range.Groups[3].Value).ToCharArray();
                    for (var code = low; code <= high; code++)
                    {
                        map[code] = new string(destination);
                        destination[^1]++;
                    }
                }
                else
                {
                    // <lo> <hi> [<dst> <dst> ...]: one destination per code.
                    var destinations = HexString().Matches(range.Groups[4].Value)
                        .Select(m => Utf16(m.Groups[1].Value))
                        .ToList();
                    for (var code = low; code <= high; code++)
                    {
                        map[code] = destinations[code - low];
                    }
                }
            }
        }

        return map;
    }

    private static int HexNumber(string hex) => int.Parse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture);

    private static string Utf16(string hex) => Encoding.BigEndianUnicode.GetString(Convert.FromHexString(hex));

    [GeneratedRegex(@"beginbfchar(.*?)endbfchar", RegexOptions.Singleline)]
    private static partial Regex BfCharSection();

    [GeneratedRegex(@"beginbfrange(.*?)endbfrange", RegexOptions.Singleline)]
    private static partial Regex BfRangeSection();

    [GeneratedRegex(@"<([0-9A-Fa-f]+)>\s*<([0-9A-Fa-f]+)>")]
    private static partial Regex HexPair();

    [GeneratedRegex(@"<([0-9A-Fa-f]+)>\s*<([0-9A-Fa-f]+)>\s*(?:<([0-9A-Fa-f]+)>|\[([^\]]*)\])")]
    private static partial Regex HexRange();

    [GeneratedRegex(@"<([0-9A-Fa-f]+)>")]
    private static partial Regex HexString();

    // ─── Content-stream tokens ──────────────────────────────────────────────────

    private sealed record Operator(string Name);

    private sealed record Name(string Value);

    /// <summary>Operands (numbers, names, strings as bytes, arrays as lists) and operators, in stream order.</summary>
    private static IEnumerable<object> Tokens(byte[] stream)
    {
        // Latin-1 maps each byte to the char of the same value, so the string indexes as the bytes do.
        var content = Encoding.Latin1.GetString(stream);
        var position = 0;
        var arrays = new Stack<List<object>>();
        while (NextToken(content, ref position) is { } token)
        {
            switch (token)
            {
                case Operator { Name: "[" }:
                    arrays.Push([]);
                    continue;
                case Operator { Name: "]" }:
                    token = arrays.Pop();
                    break;
            }

            if (arrays.Count > 0)
            {
                arrays.Peek().Add(token);
            }
            else
            {
                yield return token;
            }
        }
    }

    private static object? NextToken(string content, ref int position)
    {
        while (position < content.Length)
        {
            if (IsWhitespace(content[position]))
            {
                position++;
            }
            else if (content[position] == '%')
            {
                while (position < content.Length && content[position] is not ('\n' or '\r'))
                {
                    position++;
                }
            }
            else
            {
                break;
            }
        }

        if (position >= content.Length)
        {
            return null;
        }

        var start = position;
        switch (content[position])
        {
            case '[' or ']':
                position++;
                return new Operator(content[start].ToString());
            case '<' when position + 1 < content.Length && content[position + 1] == '<':
                // A marked-content property list (QuestPDF tags layout artifacts: /Artifact <</Type /Layout>> BDC).
                // It shows no text, unless it replaces the glyphs' text, which this reader does not model.
                position = DictionaryEnd(content, start);
                var properties = content[start..position];
                return properties.Contains("/ActualText", StringComparison.Ordinal)
                    ? throw new InvalidDataException("/ActualText replaces the glyphs' text, which T200's reader does not model.")
                    : new Name(properties);
            case '<':
                var close = content.IndexOf('>', position);
                var hex = new string(content[(position + 1)..close].Where(Uri.IsHexDigit).ToArray());
                position = close + 1;
                return Convert.FromHexString(hex.Length % 2 == 0 ? hex : hex + "0");
            case '(':
                return LiteralString(content, ref position);
            case '/':
                position++;
                while (position < content.Length && IsRegular(content[position]))
                {
                    position++;
                }

                return new Name(content[(start + 1)..position]);
            default:
                while (position < content.Length && IsRegular(content[position]))
                {
                    position++;
                }

                var word = content[start..position];
                return double.TryParse(word, NumberStyles.Float, CultureInfo.InvariantCulture, out var number)
                    ? number
                    : new Operator(word);
        }
    }

    /// <summary>The bytes of the literal string opening at <paramref name="position" />, which it leaves past the close.</summary>
    private static byte[] LiteralString(string content, ref int position)
    {
        var bytes = new List<byte>();
        var depth = 0;
        position++;
        while (true)
        {
            var c = content[position++];
            if (c == '\\')
            {
                var escaped = content[position++];
                if (escaped is >= '0' and <= '7')
                {
                    var octal = escaped - '0';
                    for (var digits = 1; digits < 3 && content[position] is >= '0' and <= '7'; digits++)
                    {
                        octal = (octal * 8) + (content[position++] - '0');
                    }

                    bytes.Add((byte)octal);
                }
                else
                {
                    bytes.Add((byte)(escaped switch
                    {
                        'n' => '\n',
                        'r' => '\r',
                        't' => '\t',
                        'b' => '\b',
                        'f' => '\f',
                        _ => escaped
                    }));
                }

                continue;
            }

            if (c == ')' && depth-- == 0)
            {
                return bytes.ToArray();
            }

            if (c == '(')
            {
                depth++;
            }

            bytes.Add((byte)c);
        }
    }

    private static bool IsWhitespace(char c) => c is '\0' or '\t' or '\n' or '\f' or '\r' or ' ';

    private static bool IsRegular(char c)
        => !IsWhitespace(c) && c is not ('(' or ')' or '<' or '>' or '[' or ']' or '{' or '}' or '/' or '%');

    // ─── Objects ────────────────────────────────────────────────────────────────

    private sealed record PdfObject(string Dictionary, byte[]? RawStream)
    {
        public byte[] Stream()
        {
            if (RawStream is null)
            {
                throw new InvalidDataException("The object is not a stream.");
            }

            if (!Dictionary.Contains("/Filter", StringComparison.Ordinal))
            {
                return RawStream;
            }

            if (!FlateDecode().IsMatch(Dictionary))
            {
                throw new InvalidDataException($"Only /FlateDecode streams are read (T200's reader): {Dictionary}");
            }

            using var inflated = new MemoryStream();
            using (var zlib = new ZLibStream(new MemoryStream(RawStream), CompressionMode.Decompress))
            {
                zlib.CopyTo(inflated);
            }

            return inflated.ToArray();
        }
    }

    private static Dictionary<int, PdfObject> ReadObjects(byte[] pdf)
    {
        var text = Encoding.Latin1.GetString(pdf);
        if (text.Contains("/ObjStm", StringComparison.Ordinal))
        {
            throw new InvalidDataException("Object streams are not read (T200's reader).");
        }

        var objects = new Dictionary<int, PdfObject>();
        var resumeAt = 0;
        foreach (Match header in ObjectHeader().Matches(text))
        {
            if (header.Index < resumeAt)
            {
                // Inside the previous object's stream: compressed bytes that happen to read as a header.
                continue;
            }

            var position = header.Index + header.Length;
            if (!text.AsSpan(position).StartsWith("<<"))
            {
                // Not a dictionary (a number, an array, a string): nothing the text layer reads.
                continue;
            }

            var dictionaryEnd = DictionaryEnd(text, position);
            var dictionary = text[position..dictionaryEnd];
            byte[]? stream = null;

            var keyword = StreamKeyword().Match(text, dictionaryEnd);
            if (keyword.Success && keyword.Index == dictionaryEnd)
            {
                var length = DirectLength().Match(dictionary);
                if (!length.Success)
                {
                    throw new InvalidDataException($"A stream without a direct /Length is not read (T200's reader): {dictionary}");
                }

                var start = keyword.Index + keyword.Length;
                resumeAt = start + int.Parse(length.Groups[1].Value, CultureInfo.InvariantCulture);
                stream = pdf[start..resumeAt];
            }

            objects[int.Parse(header.Groups[1].Value, CultureInfo.InvariantCulture)] = new PdfObject(dictionary, stream);
        }

        return objects;
    }

    /// <summary>The index just past the <c>&gt;&gt;</c> that closes the dictionary opening at <paramref name="start" />.</summary>
    private static int DictionaryEnd(string text, int start)
    {
        var depth = 0;
        var i = start;
        while (i < text.Length)
        {
            if (text[i] == '(')
            {
                // A literal string (a title, a date) may hold angle brackets; skip it whole.
                LiteralString(text, ref i);
            }
            else if (text[i] == '<' && i + 1 < text.Length && text[i + 1] == '<')
            {
                depth++;
                i += 2;
            }
            else if (text[i] == '>' && i + 1 < text.Length && text[i + 1] == '>')
            {
                depth--;
                i += 2;
                if (depth == 0)
                {
                    return i;
                }
            }
            else
            {
                i++;
            }
        }

        throw new InvalidDataException("An unterminated dictionary.");
    }

    private static void CollectPages(Dictionary<int, PdfObject> objects, PdfObject node, List<PdfObject> pages)
    {
        if (TypeIs(node.Dictionary, "Page"))
        {
            pages.Add(node);
            return;
        }

        foreach (var kid in References(node.Dictionary, "Kids"))
        {
            CollectPages(objects, objects[kid], pages);
        }
    }

    private static bool TypeIs(string dictionary, string type, string key = "Type")
        => Regex.IsMatch(dictionary, $@"/{key}\s*/{type}(?![A-Za-z0-9])");

    private static int Reference(string dictionary, string key) => References(dictionary, key).Single();

    /// <summary>The object ids <paramref name="key" /> refers to, whether one reference or an array of them.</summary>
    private static IReadOnlyList<int> References(string dictionary, string key)
    {
        var match = Regex.Match(dictionary, $@"/{key}(?![A-Za-z0-9])\s*(\[[^\]]*\]|\d+\s+\d+\s+R)");
        if (!match.Success)
        {
            throw new InvalidDataException($"/{key} is not a reference: {dictionary}");
        }

        return IndirectReference().Matches(match.Groups[1].Value)
            .Select(m => int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture))
            .ToList();
    }

    /// <summary>The value of <paramref name="key" />: an inline dictionary, or the dictionary of the object it refers to.</summary>
    private static string Resolve(Dictionary<int, PdfObject> objects, string dictionary, string key)
    {
        var match = Regex.Match(dictionary, $@"/{key}(?![A-Za-z0-9])\s*(<<|\d+\s+\d+\s+R)");
        if (!match.Success)
        {
            return "<<>>";
        }

        var value = match.Groups[1];
        return value.Value == "<<"
            ? dictionary[value.Index..DictionaryEnd(dictionary, value.Index)]
            : objects[int.Parse(IndirectReference().Match(value.Value).Groups[1].Value, CultureInfo.InvariantCulture)].Dictionary;
    }

    /// <summary>The <c>/Name n 0 R</c> entries of the sub-dictionary <paramref name="key" /> (the fonts, the XObjects).</summary>
    private static Dictionary<string, int> NamedReferences(Dictionary<int, PdfObject> objects, string resources, string key)
        => NamedReference().Matches(Resolve(objects, resources, key))
            .ToDictionary(m => m.Groups[1].Value, m => int.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture));

    [GeneratedRegex(@"(?<![0-9])(\d+)\s+0\s+obj\s*")]
    private static partial Regex ObjectHeader();

    [GeneratedRegex(@"\s*stream\r?\n")]
    private static partial Regex StreamKeyword();

    [GeneratedRegex(@"/Length\s+(\d+)(?![0-9])(?!\s+\d+\s+R)")]
    private static partial Regex DirectLength();

    [GeneratedRegex(@"/Filter\s*/FlateDecode(?![A-Za-z0-9])")]
    private static partial Regex FlateDecode();

    [GeneratedRegex(@"(\d+)\s+\d+\s+R")]
    private static partial Regex IndirectReference();

    [GeneratedRegex(@"/([^\s/<>\[\]()]+)\s+(\d+)\s+\d+\s+R")]
    private static partial Regex NamedReference();
}
