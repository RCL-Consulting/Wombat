---
id: T262
title: The audit log and the access report page with no tie-break, so entries with the same timestamp can be skipped or repeated
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
---

# T262 — The audit log and the access report page with no tie-break, so entries with the same timestamp can be skipped or repeated

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. An auditor paging the log can miss an entry.
**Surfaced:** 2026-09-25, the T244 review.

## What to build

- `ListAuditEntriesQueryHandler`: `.OrderByDescending(e => e.OccurredAt).ThenByDescending(e => e.Id)` before
  `Skip`/`Take`.
- `AccessReportBuilder`: the same before `Take(500)`.
- A test with several entries on one timestamp across a page boundary.

## Verification

- [ ] Every entry appears exactly once across pages. Test.

## Related

T244, T026.
