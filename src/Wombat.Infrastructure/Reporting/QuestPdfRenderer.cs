using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace Wombat.Infrastructure.Reporting;

/// <summary>
/// Where every PDF Wombat produces is rendered: one render at a time in the process. (T200)
/// </summary>
/// <remarks>
/// <para>
/// <b>Why.</b> Two QuestPDF renders that overlap can each come out with fonts whose <c>/ToUnicode</c> map sends every
/// glyph but one to U+0000. The page prints perfectly and reads as nothing: search, copy and a screen reader get no text,
/// and the content hash (T078) is not the one the same data gives on its own. Measured on Windows, where QuestPDF's
/// native Skia uses DirectWrite: eight exports started together, 30 times, gave 19 to 28 bad PDFs out of 240 in each of
/// three runs, portfolios and certificates alike. The T169 determinism test caught the same thing about once in 25 suite runs. The glyphs and the
/// layout come out right and only the glyph-to-Unicode maps are wrong, which points (inferred, not traced) at the native
/// library's typefaces, shared by every document in the process. Production runs FreeType on Linux; whether that path
/// races too was not measured, and with nothing overlapping it does not matter.
/// </para>
/// <para>
/// <b>Why static.</b> The shared state is per process, so the gate is too. A DI singleton is one per container, and a
/// process can hold more than one (the integration tests' hosts); a per-container gate would not serialise them.
/// </para>
/// <para>
/// <b>A bad render does not poison later ones.</b> After each of 29 corrupted rounds, five portfolios and five
/// certificates rendered one at a time were all byte-identical to a reference render, with their whole text (290 of
/// 290). Stopping the overlap is the whole fix; there is nothing to reset.
/// </para>
/// <para>
/// Only the wait can be cancelled: a render that has started runs to its end. A render that had to wait runs on the
/// thread pool, not back on the caller's context; the caller's own <c>await</c> still resumes there. The access report
/// renders its PDF through <see cref="PortfolioPdfService" />, so it takes this gate too.
/// <c>PdfRenderingTests</c> (Architecture) fails if anything else calls a QuestPDF render method.
/// </para>
/// </remarks>
internal static class QuestPdfRenderer
{
    private static readonly SemaphoreSlim Gate = new(1, 1);

    public static async Task<byte[]> GeneratePdfAsync(IDocument document, CancellationToken cancellationToken)
    {
        // Not back onto the caller's context: on Blazor Server that is the circuit's dispatcher, and a render that has
        // the gate must not then wait for one circuit while every export in the process waits for it.
        await Gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return document.GeneratePdf();
        }
        finally
        {
            Gate.Release();
        }
    }
}
