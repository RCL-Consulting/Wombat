---
id: T200
title: PDFs rendered at the same time can lose their text layer (QuestPDF), so an export may be unsearchable and unreadable to a screen reader
status: done
priority: P2
owner: agent
depends_on: []
created: 2026-09-24
completed: 2026-09-24
---

# T200 — PDFs rendered at the same time can lose their text layer (QuestPDF), so an export may be unsearchable and unreadable to a screen reader

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium. A portfolio, certificate or access-report PDF could print correctly but carry no text. That
breaks search, screen readers and the export's reproducible hash. The failure is intermittent.
**Surfaced:** 2026-09-24, the T169 review. A determinism test failed about once in 25 full runs. The two PDFs differed
only in the fonts' ToUnicode maps: in one, every Lato glyph mapped to U+0000. The cause is two QuestPDF renders running
at once in one process. The test classes now share a non-parallel collection (60 of 60 passed), but production renders
are not serialised.

## What to build

Serialise PDF rendering process-wide (a `SemaphoreSlim(1)` around `GeneratePdf` in one shared renderer used by
`PortfolioPdfService`, the certificate and the access report), or find the upstream QuestPDF/SkiaSharp fix. Check
whether one bad render poisons later renders in the same process.

## Verification

- [x] Two parallel exports both carry a full text layer, repeatedly. Stress test without the test collection's
      serialisation.

## Related

T169, T023 (the portfolio PDF), T026 (the access report).

---

## As built — 2026-09-24

One shared `QuestPdfRenderer` serialises every QuestPDF render process-wide (`SemaphoreSlim(1)`). It waits with
`ConfigureAwait(false)`, so a queued render never holds the gate on a busy circuit. An export whose caller has gone is
never rendered. The portfolio, certificate and access-report PDFs all go through it, and an architecture test
(`PdfRenderingTests`) refuses any other QuestPDF render call. A stress test renders in parallel without the test
collection's serialisation, and every document keeps its text layer. Mutation-checked.

Out of scope, noted: `AccessReportBuilder`'s bare `catch` would also swallow a cancellation. Nothing passes a token
today.
