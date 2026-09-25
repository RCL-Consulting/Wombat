---
id: T215
title: The agenda planner ignores STARs that no agenda line records, so an EPA already decided comes up as due again
status: done
priority: P2
owner: agent
depends_on: []
created: 2026-09-24
completed: 2026-09-25
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

- [x] A trainee with an Active STAR issued in the window has no Due line for that EPA, and the preview names it
      "Already decided in this window". Handler test.
- [x] The decisions-due page shows it as decided. Test.
- [x] Browser: a new 2026 S2 review for the dev trainee skips PAED-001, 002 and 006.

## Related

T131 (slices 4 and 6), T166.

---

## As built — 2026-09-25 (`69da24d`)

The agenda planner, its preview and Start count an EPA as decided in its window by the decisions-due page's rule (an
Active or Superseded STAR issued in the window, from any review), which is one predicate in `DecisionWindowRecords`. An EPA
decided that way is not planned. The preview and the agenda card say "Already decided in this window, so not on the
agenda: …". An agenda whose due EPAs are all decided says so. Handler tests cover a pre-agenda STAR, a Superseded one,
a Revoked one, an annual EPA decided in S1, and a STAR from a previous institution. bUnit covers the wording.

Browser on dev (scripted Chrome, master `225763e`):
- **The coordinator's preview** for panel 1, 2026 S2: "10 EPAs will be on the agenda", and "Already decided in this
  window, so not on the agenda: PAED-001, PAED-002 and PAED-006."
- **Review 6** has 10 lines, none for those three, and after Start they are still absent.
- **Decisions-due** shows the three Decided (STARs #3, #4 and #2).
- **Review 7's preview** also named PAED-007, decided by review 6's STAR #6.

**Filed from the review:** [T235] (P2): an agenda line is never reconciled after Start.
