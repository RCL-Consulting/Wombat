# T077 + T078 — Portfolio PDF: STAR section (F-5-2) + determinism (F-5-3)

**Status:** ✅ Shipped 2026-06-01 (Opus). Two related portfolio-PDF fixes from the Act 5 play-through.

## T077 — Portfolio omits the STAR/authorisations section (F-5-2)

`PortfolioPdfService` never queried `EntrustmentDecisions`, so the exported portfolio showed
committee *reviews* but **none of the trainee's awarded STARs** — the centerpiece of a graduation
portfolio was absent. (STARs existed only as separate per-EPA certificates via
`EntrustmentCertificatePdfService`.)

**Fix:** load the trainee's **active** `EntrustmentDecision`s (with `Epa` + `AuthorisedLevel`,
ordered by EPA code, **not** date-filtered — they are current standing authorisations) and render a new
`EntrustmentSummaryComponent` — a "Statements of Awarded Responsibility (STARs)" table (EPA code,
title, authorised level, issued, expires) placed after the Summary, before Committee Decisions.

**Verified live:** Molefe's portfolio now lists all 15 STARs (12 Unsupervised, 3 Indirect supervision)
with issue dates.

## T078 — Portfolio + STAR-certificate PDFs are non-deterministic (F-5-3)

Both PDFs rendered `Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC` on every page, and QuestPDF
stamped `DateTime.Now` document metadata — so two exports a minute apart produced different bytes. The
content hash is used by the file name (`portfolio-{hash}.pdf`) and persisted to `PortfolioExport` for
the `/portfolio/verify` surface, so byte-for-byte reproducibility is a real functional requirement, not
cosmetic.

**Fix:** the PDF is now a pure function of its data —
- removed the wall-clock `Generated:` line from `CoverPageComponent` and the certificate footer
  (provenance is the content hash itself: the file name + `/portfolio/verify`);
- set deterministic `DocumentMetadata` (fixed `CreationDate`/`ModifiedDate` =
  `PortfolioPdfService.DeterministicTimestamp` = `2000-01-01Z`) on both services via `.WithMetadata(...)`.

**Verified live:** two exports of Molefe's portfolio produced the **identical** file
`portfolio-176a91aec2bc.pdf` (before the fix: `…46a3959b1e92` ≠ `…82cb272b2eb3`).

## Tests

`PortfolioPdfServiceTests` (+2, Infrastructure 8→10): `Generate_IsByteForByteDeterministic` (two
generations identical) and `Generate_StarsAffectOutput` (removing the active STAR changes the bytes —
proving the section renders). Added `InternalsVisibleTo("Wombat.Infrastructure.Tests")`. All suites
green: Domain 45, Infrastructure 10, Application 265, Architecture 19, Web 42. Integration not run (Docker).

## Related finding NOT fixed here

- **F-5-5 (noted, deferred):** `ExportPortfolioCommand.DemandExportAccess` excludes **Coordinator**, so
  the scenario's Step 5.5 ("Coordinator reproduces the PDF") fails on authorisation. Decide whether
  Coordinators should have portfolio read/export access.
