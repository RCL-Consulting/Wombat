namespace Wombat.Infrastructure.Tests.Reporting;

/// <summary>
/// Every test class that renders a document through QuestPDF runs in this collection: one at a time, and never
/// alongside another collection.
/// </summary>
/// <remarks>
/// <para>
/// Two renders running at once can each lose their text layer. Observed on the T169 branch:
/// <c>PortfolioPdfServiceTests.Generate_IsByteForByteDeterministic</c> failed about once in 25 runs of this suite, and
/// the two PDFs differed only in their fonts' ToUnicode maps. In the second export, every Lato glyph mapped to U+0000.
/// The glyphs and the layout were identical. That is library state shared across the process (the typeface's
/// glyph-to-Unicode map), not anything the document composes.
/// </para>
/// <para>
/// Since T200 every production render goes through <c>QuestPdfRenderer</c>, one at a time in the process, and
/// <see cref="ConcurrentPdfRenderingTests" /> starts exports together, outside this collection, to prove it. This
/// collection stays for the renders the tests make themselves: the SVG text assertions (T161) call QuestPDF directly,
/// past the gate, so they must not overlap each other or an export.
/// </para>
/// </remarks>
[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class QuestPdfRenderingCollection
{
    public const string Name = "QuestPDF rendering";
}
