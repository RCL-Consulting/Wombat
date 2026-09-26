---
id: T277
title: The audit log's default window moves between pages, and the access report cuts at 500 without saying so
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
---

# T277 — The audit log's default window moves between pages, and the access report cuts at 500 without saying so

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. An auditor paging the default view can see an entry twice or miss it. A subject-access answer can be
silently incomplete.
**Surfaced:** 2026-09-25, the T262 review.

## Symptom

- **The window moves.** `ListAuditEntriesQueryHandler` recomputes `from = UtcNow - 24h` and `to = UtcNow` on every
  request when no dates are typed, and `AuditList` re-queries on Next. New rows push entries down, old ones drop out,
  and the total changes between pages.
- **The access report is cut.** `AccessReportBuilder` takes 500 with no total and no flag, and never reads
  `AuditEntryArchive`.

## What to build

- Fix the window when the page loads or filters apply, and send it on every page. Better, page by the key of the last
  row shown (`OccurredAt`, `Id`).
- The access report includes archived rows. It is complete, or it says "newest 500 of N".

## Verification

- [ ] Paging the default view while rows are written repeats and misses nothing. Test.
- [ ] The access report states its total and includes archived rows. Test.

## Related

T262, T026.

## Notes

- **T295 replay, 2026-09-26 (C65).** Note, 2026-09-26 (T295 replay, step 3.56): 'Back to log' on an audit entry drops the list's filters. The replay filtered on From 10:13 UTC and Result Failures only, opened an entry, and pressed Back to log. It landed on /admin/audit with From empty and Result All: 50 rows from the last 24 hours (design/baseline/act-3/3.56-1-audit-failure-detail.png). The cause is that AuditDetail.razor:10 links to a bare `/admin/audit`, and AuditList.razor:141-150 keeps its filters and page in component fields, so nothing survives the navigation. This widens the task to the detail page, and it gives the window fix a home. Put the filters, the resolved From/To window and the page number in the list's query string (`[SupplyParameterFromQuery]`). Have each Detail link carry that URL as its return, and make Back to log go back to it. The list then pages a fixed window, and the reader returns to the rows they left. Verify by replaying 3.55-3.56: filter, open an entry, press Back to log, and the same rows and filters are shown. Add a bUnit test that the Detail link carries the filtered URL.
