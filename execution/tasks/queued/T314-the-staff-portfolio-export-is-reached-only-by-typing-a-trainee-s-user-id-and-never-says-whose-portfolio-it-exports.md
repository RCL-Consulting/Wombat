---
id: T314
title: The staff portfolio export is reached only by typing a trainee's user id, and never says whose portfolio it exports
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-26
---

# T314 — The staff portfolio export is reached only by typing a trainee's user id, and never says whose portfolio it exports

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. The export's scope is correct (T101). The defects are how staff reach it, and that a wrong id stays invisible until the PDF is opened.
**Surfaced:** 2026-09-26, the T295 replay of the rewritten scenario runbook (findings F-5.10a).

## Symptom

In Steps 5.10 and 5.12, Prof Mbatha and then Mr Smit typed /portfolio/export/59ba7d01-561f-4831-9f35-b85305e31354 (design/baseline/act-5/5.10-1-mbatha-staff-export.png, 5.12-1-smit-reproduces-export.png, design/baseline/states/export-portfolio--staff.png). The page reads 'Export portfolio / Generate a PDF export of the trainee's portfolio.' before and after the export, and never names Dr Molefe. Mr Smit found her id only in the trainee= part of the Schedule link he followed in Step 5.2. No page a Coordinator can open shows or links it.

## Root cause

- ExportPortfolio.razor:10 is a fixed PageHeader. OnInitializedAsync (:62-67) fills in only the caller's own id and never resolves a name.
- Nothing links to /portfolio/export/{TraineeUserId}. The only link to the page is the NavMenu's own-export item (NavMenu.razor:106). The committee review page (/committee/reviews/{id}), which a Coordinator opens for a named trainee, and the trainee profile page (/admin/trainees/edit) offer no export.
- DESIGN.md:1692-1701 (T142): a person is shown by name, and 'No input takes a raw user id'. The route id is such an input.

## What to build

- **The page names whose portfolio it exports:** 'Export Lerato Molefe's portfolio' for staff, 'Export your portfolio' for the trainee herself. The name comes from a query gated like ExportPortfolioCommandHandler.DemandExportAccessAsync (TraineeScopeResolver.MayReadAsync). It gives the one refusal for an unknown id and an out-of-scope id alike, so no name is disclosed and no trainee's existence is confirmed. On a refusal the form is not shown.
- **Staff reach it from a named trainee.** Add an 'Export portfolio' link on the committee review page for the trainee under review, and on the trainee profile page. Offer it only to a caller MayReadAsync admits, and name the link for the person (DESIGN's row-action rule).
- **Docs.** DESIGN records the staff route and where it is linked.

## Verification

- [ ] bUnit: the staff route shows the trainee's name in the header. An out-of-scope id and an unknown id show the same refusal and no form. The own route says 'your portfolio'.
- [ ] Authorization test: the export link on the review page and on the profile page is offered only to roles whose export the handler admits.
- [ ] Browser, replay Steps 5.10 and 5.12. Prof Mbatha reaches the export from Dr Molefe's profile page and Mr Smit from her review, and each page names her. The file is byte-identical to Step 5.9's.

## Related

T023, T101 (export scope), T142 (names, not ids), T154 (the trainee hands the PDF to the reviewer), T182 (a picker, not an id). Runbook Steps 5.2, 5.9, 5.10 and 5.12. Finding F-5.10a. The suspect about ExportPortfolio's default dates (Step 5.9, DateTime.Today rather than the South African calendar) is on the same page.
