---
id: T353
title: Step 3.30's ageing leaves a request filed before its encounter
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-10-03
---

# T353 — Step 3.30's ageing leaves a request filed before its encounter

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. A runbook artefact, not a product defect: no user can file before an encounter. But the baseline then
shows an impossible record, and a design brief that cites it would carry the impossibility.
**Surfaced:** 2026-10-03, T350's states capture (the act-3 agent on `wombat_scenario_t350_s5184`).

## Symptom

After Step 3.30's Note ages Dr Mahlangu's Mini-CEX by eight days, its About card reads "Encounter 2026-09-30" and "Filed
2026-09-25": filed five days before the encounter it records. The states `home--committee-line-overdue.png`,
`activity-view--from-the-line.png` and `home--assessor-pending.png` show that record.

## Root cause

The Note moves `UpdatedOn` and the history rows' `OccurredOn` back eight days, so the request has waited long enough to
be overdue; the encounter is typed as `D−3` in Step 3.2x and is not moved. Eight days back from D lands before D−3.

## What to build

Make the aged record possible. Either the Mini-CEX's encounter is typed at least eight days back (`D−10`, say, still
inside the 14-day late-filing window so no warning is added, and still after the programme start), or the Note also
moves the encounter date (`DataJson`'s observation date field) by the same eight days. Prefer the first: it changes a
typed value, not stored JSON. Then follow the date through every Actual and Expect line that quotes it (3.33's line on
Home, A-steps that name it).

## Verification

- [ ] After 3.30 on a replay, the activity's About reads an encounter on or before its Filed day — the act-3 replay's
  Actual line and the re-taken states.

## Related

T350 (flow 04's replay and states), Step 3.30, Step 3.33.
