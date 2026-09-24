---
id: T212
title: Review page leftovers: the pending list stays after ratify, picker labels lack a space, and certificate glyphs extract as U+FFFD
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-24
---

# <id> — <one line that states the defect or the goal, not the solution>

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low.
**Surfaced:** 2026-09-24, the T131 slices 1–2 browser check.

## Items

1. After "Decision ratified." `ReviewDetail` still lists the staged decisions under "Pending entrustment decisions"
   (without Remove buttons) until a reload. `ExecuteAsync` reloads the standing but not `_pendingDecisions`. This
   predates T131. Sequence the fix after T131 slice 4 and T165, which also edit the page.
2. The evidence picker's labels read "Clinical Case Analysis (Paediatrics) #19· 3a · …", with no space before the first
   "·". This is Razor's dropped space again (T166 met it).
3. The STAR certificate's "•" and "—" draw correctly but extract as U+FFFD (pdftotext, PyMuPDF), so the certificate's
   text layer is lossy for search and screen readers. Check the font's glyph coverage and ToUnicode map (T200 is
   related).

4. The agenda's deferral form blocks an empty reason (`aria-invalid`) but never shows "Say why the committee is
   deferring the decision.": it has no ValidationMessage.
5. My reviews lists several ratified reviews that read the same, because its Period column shows the window
   ("2026-01-01 to 2026-12-31") and not the period label ("2026 S2").

## Verification

- [ ] Each item fixed: a bUnit test for 1 and 2, a text-extraction test for 3.

## Related

T131, T166, T200.
