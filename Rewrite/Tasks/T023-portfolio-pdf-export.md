# T023 — Portfolio PDF export

**Phase:** 6 — Cross-cutting operations
**Depends on:** T020, T021, T022
**Blocks:** T016

## Goal

A trainee (or admin acting for them) can export a complete portfolio PDF: all activities, reflections, MSF aggregates, committee decisions, with institution branding. This is table stakes for regulatory submission and for trainees moving between programmes.

## What to do

1. Add QuestPDF to `Directory.Packages.props`. Accept the Community License (it's free for non-commercial and for small commercial; check terms match Wombat's licensing).
2. PDF generation service in `Wombat.Infrastructure/Reporting/PortfolioPdfService.cs`. It takes a `PortfolioExportRequest` (trainee id, from/to dates, optional filters) and returns a byte array.
3. The PDF is structured as:
   - **Cover page**: institution logo, trainee name, programme, date range, date generated, system signature (hash of the contents).
   - **Summary page**: curriculum progress chart (textual), counts of activities by type, list of completed EPAs.
   - **Committee decisions section**: each ratified decision on its own page.
   - **Activities section**: grouped by type, each activity rendered from its pinned schema version. One per page for formal instruments (Mini-CEX, DOPS), compact list for logbook entries.
   - **STAR reflections section**: each reflection with its four sections + feedback.
   - **MSF section**: released aggregate reports with the coordinator's narrative. **No individual responses** ever appear in the PDF — the anonymity guarantee from T021 extends here.
   - **Appendix — audit trail**: a compact list of state transitions for every included activity, showing who did what when.
4. **Activity rendering**: the PDF service uses the pinned `FormSchema` for each activity to render its fields. This is the same rendering concept as the Blazor component, but in QuestPDF primitives. A small helper maps schema field types to QuestPDF elements.
5. **Institution branding**: the PDF reads logo and colour palette from `InstitutionBrand` (a simple configuration entity per institution). Default branding if none set.
6. **Request handling**: a CQRS command `ExportPortfolioCommand` that runs synchronously for small portfolios and enqueues a background job for large ones (threshold: ~200 activities). Large-job status surfaced via a progress indicator.
7. **Download endpoint**: Blazor page triggers the export and streams the result to the client as a file download. For background-generated PDFs, the file is stored in `/opt/wombat/data/exports/` with a signed URL that expires in 24 hours.
8. **Integrity signature**: the last page of the PDF shows a SHA-256 hash of the content stream and a note "Verify at `/portfolio/verify?hash=...`". The verify page checks the hash against a stored record of exports. This does not prove authenticity against tampering but does prove "this is the same PDF Wombat produced on date X", which is useful for disputes.
9. **Audit**: every export is recorded in the admin audit log (T025) with trainee id, exporter user id, filter parameters, hash, timestamp.

## Files created

**Domain:**
- `src/Wombat.Domain/Institutions/InstitutionBrand.cs`
- `src/Wombat.Domain/Reporting/PortfolioExport.cs`

**Application:**
- `src/Wombat.Application/Features/Reporting/IPortfolioPdfService.cs`
- `src/Wombat.Application/Features/Reporting/PortfolioExportDtos.cs`
- `src/Wombat.Application/Features/Reporting/ExportPortfolio.cs`
- `src/Wombat.Application/Features/Reporting/VerifyExport.cs`

**Infrastructure:**
- `src/Wombat.Infrastructure/Reporting/PortfolioPdfService.cs`
- `src/Wombat.Infrastructure/Reporting/CoverPageComponent.cs`
- `src/Wombat.Infrastructure/Reporting/SummaryPageComponent.cs`
- `src/Wombat.Infrastructure/Reporting/CommitteeSectionComponent.cs`
- `src/Wombat.Infrastructure/Reporting/ActivitiesSectionComponent.cs`
- `src/Wombat.Infrastructure/Reporting/MsfSectionComponent.cs`
- `src/Wombat.Infrastructure/Reporting/AuditAppendixComponent.cs`
- `src/Wombat.Infrastructure/Reporting/IntegrityFooterComponent.cs`
- `src/Wombat.Infrastructure/Persistence/Configurations/InstitutionBrandConfiguration.cs`
- `src/Wombat.Infrastructure/Persistence/Configurations/PortfolioExportConfiguration.cs`
- `src/Wombat.Infrastructure/Persistence/Migrations/20260413120000_PortfolioExport.cs` (+ Designer)

**Web:**
- `src/Wombat.Web/Components/Pages/Portfolio/ExportPortfolio.razor`
- `src/Wombat.Web/Components/Pages/Portfolio/VerifyExport.razor`
- `src/Wombat.Web/wwwroot/js/file-download.js`

**Tests:**
- `tests/Wombat.Application.Tests/Features/Reporting/ExportPortfolioCommandHandlerTests.cs`
- `tests/Wombat.Application.Tests/Features/Reporting/VerifyExportQueryHandlerTests.cs`

## Verification

- [x] `dotnet build` clean.
- [ ] Manual: export a portfolio for the demo trainee. Open the PDF. Every section renders, no blank pages, institution branding appears.
- [ ] The PDF passes a PDF/A conformance check (use a free validator). This is often required by regulators. *(Deferred — task file allows fallback to regular PDF.)*
- [ ] The hash shown on the last page matches the verify page's recorded hash.
- [ ] MSF section contains aggregate narrative only, no individual responses (verify by exporting a trainee with MSF data and inspecting the output).

## Notes & gotchas

- QuestPDF is lovely but has a learning curve. Budget for it.
- Do not put PII in the filename. Use a hash or a short opaque ID.
- Large portfolios (thousands of activities) will produce huge PDFs. Cap the filter date range in the UI (e.g., default 12 months, max 5 years) and force explicit opt-in for full-programme exports.
- Font embedding: pick one widely-licensed font (Inter, Source Sans 3) and embed it. Avoid Arial/Helvetica licensing headaches.
- The "integrity signature" is a weak guarantee — it proves content equality against a server-side record, not against a cryptographic chain of trust. Don't oversell it; document its limits.
- PDF/A conformance is non-trivial; if it blocks the task for too long, fall back to regular PDF for now and log PDF/A as a follow-up task.
