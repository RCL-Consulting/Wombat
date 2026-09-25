---
id: T262
title: The audit log and the access report page with no tie-break, so entries with the same timestamp can be skipped or repeated
status: done
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
completed: 2026-09-25
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

- [x] Every entry appears exactly once across pages. Test.

## Related

T244, T026.

---

## As built — 2026-09-25 (`16f0a72`)

The audit log and the access report order by `OccurredAt` then `Id`, both descending, before paging (`AuditEntryOrder`).
Tests cover tied timestamps across a page boundary.

Browser on dev (scripted Chrome, master `f19417d`; `pg_dump -n public` first, at `recovery/pre-t258-migration.dump`): 120 audit rows on one timestamp, filtered as instadmin:
- "1–50 of 120" ran from 120 down to 071, "51–100" from 070 down to 021, and "101–120" from 020 down to 001.
- All 120 were distinct and in order, with none missing, and Previous gave the same pages again.

**Filed from the review:** [T277] (the default window moves; the access report cuts at 500).
