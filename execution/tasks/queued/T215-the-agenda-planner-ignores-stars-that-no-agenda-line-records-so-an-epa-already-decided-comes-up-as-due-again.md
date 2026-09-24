---
id: T215
title: The agenda planner ignores STARs that no agenda line records, so an EPA already decided comes up as due again
status: queued
priority: P2
owner: agent
depends_on: []
created: 2026-09-24
---

# T215 — The agenda planner ignores STARs that no agenda line records, so an EPA already decided comes up as due again

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium. The committee is asked to decide again an EPA that already has an active STAR in the window.
**Surfaced:** 2026-09-24, the T131 slices 3–4 browser check (review 4 on dev).

## Symptom

On review 4 (2026 S2), PAED-001 and PAED-006 were planned as closing **Due** lines, although STAR #3 (PAED-001, 3a) and
STAR #2 (PAED-006, 3b) are Active and were issued inside the window. The page's own standing card (T166) shows both as
issued. `AgendaPlanner` counts only agenda **lines** marked Decided on some review. Reviews 2 and 3 were ratified before
agendas existed, so they have none.

## What to build

Make the planner, and T131 slice 6's decisions-due page, count an EPA as decided in its window when an Active or
Superseded STAR on that EPA was issued for this trainee inside the window, from any review. Agenda lines stay the
record of what a sitting did. The STAR is the fact.

Rejected: backfilling Decided lines for pre-agenda STARs. It fixes today's rows only; any STAR without a line (a future
path, a restored backup) would repeat the defect.

## Verification

- [ ] A trainee with an Active STAR issued in the window has no Due line for that EPA, and the preview names it
      "Already decided in this window". Handler test.
- [ ] The decisions-due page shows it as decided. Test.
- [ ] Browser: a new 2026 S2 review for the dev trainee skips PAED-001, 002 and 006.

## Related

T131 (slices 4 and 6), T166.
