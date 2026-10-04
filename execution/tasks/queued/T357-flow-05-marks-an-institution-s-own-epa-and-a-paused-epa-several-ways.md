---
id: T357
title: Flow 05 marks an institution's own EPA and a paused EPA several ways
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-10-04
---

# T357 — Flow 05 marks an institution's own EPA and a paused EPA several ways

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. Each surface is legible on its own; together they say one fact several ways, and one form breaks a
DESIGN.md rule. Found by the design system's re-sync to flow 05 (T355 step 8), not by the replay.
**Surfaced:** 2026-10-04, T355 step 8.

## Symptom

- **An institution's own EPA (KGK-001):** My progress's index and the EPA page's h1 show a `badge-draft` reading
  "Kgosi Kgari Teaching Hospital's own". DESIGN.md § Badges keeps badges for states, never categories, and DESIGN.md's
  curriculum-list rule says "Your institution's own item" to a reader whose reads do not span institutions. The
  standing panel marks the same EPA as text under its name.
- **A paused EPA:** `span.paused-mark` (EpaLabel); `.muted .text-sm` under the link in My activities;
  `span.activity-cell-paused` in the EPA page's Activities on this EPA.
- **December's alert** says "towards semester 2, 2026" in lower case, where every other window reads "Semester 2, 2026"
  (not reachable in the October replay).
- **DashboardCard's `Href`** has no consumer since flow 05 but is still built, inline style included.

## What to build

One mark for an institution's own EPA (text, as the standing panel writes it, in the curriculum list's words), one for
a paused EPA (`span.paused-mark` everywhere), December's window name capitalised as the others, and `Href` removed
with its style. Update DESIGN.md's R2 text and the design system's records.

## Verification

- [ ] bUnit: each surface renders the one mark; no `badge-*` names a category; December's line reads "Semester 2, …".

## Related

T355 (flow 05), `design/system/components/EpaLabel`, `EpaProgressTable`, `EpaPage`, `DashboardCard`.
