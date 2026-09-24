namespace Wombat.Infrastructure.Tests.Reporting;

/// <summary>
/// Every test class that renders a document through QuestPDF runs in this collection: one at a time, and never
/// alongside another collection.
/// </summary>
/// <remarks>
/// Rendering on two threads at once can change what a later document in the same process contains. Observed on the T169
/// branch: <c>PortfolioPdfServiceTests.Generate_IsByteForByteDeterministic</c> failed about once in 25 runs of this
/// suite, and the two PDFs differed only in their fonts' ToUnicode maps. In the second export, every Lato glyph mapped
/// to U+0000. The glyphs and the layout were identical. That is library state shared across the process (the font's
/// glyph-to-Unicode map), not anything the document composes, so the byte-for-byte assertions (T078) and the SVG text
/// assertions (T161) can only be trusted with renders serialised.
/// </remarks>
[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class QuestPdfRenderingCollection
{
    public const string Name = "QuestPDF rendering";
}
