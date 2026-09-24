---
id: T159
title: Retarget the paediatric scenario runbook (scenario-paediatrics.md Acts 1-2) onto the seeded v11.1 catalogue
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-24
---

# T159 — The paediatric scenario runbook still builds the FCPaed world that W-006 removed

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low for the product, Medium for the test corpus. The runbook is the scenario corpus other tasks verify
against. Replaying Acts 1-2 as written rebuilds the second paediatric world: a five-rung ladder, an operator-built
curriculum and ten `*_paed` types, one of which shares a display name with a seeded type. The scenario also never
exercises the seeded v11.1 catalogue, which is what the EPA stream built.
**Surfaced:** 2026-09-24, the EPA-stream survey, closing [T104] and [T118]. Neither task's scope covered the runbook.

## Symptom

Observed in `execution/knowledge/scenario-paediatrics.md` at `431e69e`:

| Line | What the runbook builds by hand |
|---|---|
| `:141` (Step 1.2) | College "College of Paediatricians", short code `FCPaed` |
| `:179` (Step 1.6) | "Paed General Entrustment Scale", five levels |
| `:202` (Step 1.8) | 15 General Paediatrics EPAs |
| `:235` (Step 1.9) | curriculum `FCPaed(SA) Part 1` v2026.1, with lifetime totals in Step 1.10 |
| `:299` (Step 1.13) | KGK adopts that curriculum |
| `:319` (Step 1.14) | `mini_cex_paed`, named "Mini-CEX (Paediatrics)", which is the seeded `mini_cex_cpsa`'s name too (`ActivityTypeSeedCatalogue.cs:116`) |
| Step 1.15 | nine more `*_paed` types |
| `:529` | Act 2's precondition restates all of the above |
| `:688` | every registrar is admitted to `FCPaed(SA) Part 1` |
| `:889` (Act 3) | expects the picker to show the 10 `*_paed` types |

The seeders already create the College, the Paediatrics speciality and sub-speciality, the 15 v11.1 EPAs, the six-rung
CPSA ladder, the `Paediatric EPA Curriculum` 11.1 and the nine `*_cpsa` types (`ActivityTypeSeedCatalogue.cs:94-157`).
W-006 (DECISIONS.md, 2026-09-20) removed the hand-built world from dev. The runbook has no W-006 banner. Its [T130]
banner (`:18-26`) already says Act 3's expected figures must be restated as per-period targets and that the restatement
"wants a replay, not an edit".

Inferred: replaying the runbook unchanged brings back [T118] findings 6 and 7. `ListActivityTypesQuery.cs:183` lets
through any type whose ladder cannot be resolved, and [T123] records that a v11.1 trainee would still see unrated
legacy types.

## What to build

1. **Act 1.** Replace the authoring steps for the College, scale, EPAs and curriculum (Phases 1.A-1.C) with checks
   that the seeded catalogue is present: its names, the 15 EPAs, the six rungs 1/2/3a/3b/4/5, and the 15 items with
   their per-period targets. Keep the KGK institution, Prof Mbatha and the adoption (Step 1.13), now of the seeded
   curriculum. Replace Phase 1.F (building ten types) with a check that the seeded `*_cpsa` types are offered. If the
   runbook should still exercise the builder, build one institution-scoped type whose key and name cannot collide
   with a seed.
2. **Act 2.** Admit the registrars to the v11.1 curriculum (`:688`) and restate the precondition (`:529`).
3. **Expected-state blocks.** Restate the Act 1 outcome state (`:428-440`), the Act 2 precondition and outcome state
   (`:529`, `:745-755`), and the Act 3 lines that name `*_paed` types (`:889`). Restating all of Acts 3-5 is out of
   scope. File it separately if the replay shows it is needed.
4. **A dated banner at the top**, beside the [T130] and [T102] banners, saying what changed and why. Per the
   runbook's own convention, recorded `Actual:`/`Gap:` history lines are not rewritten.
5. **Replay Acts 1-2 on a fresh dev database** (the W-006 procedure: `pg_dump` first, then drop, migrate and seed).
   Snapshot the database after Act 2 for the later acts.

## Verification

- [ ] A grep of Acts 1-2 for `FCPaed`, `Paed General Entrustment` and `_paed` matches only history lines and the
      banner. Command output recorded here.
- [ ] Acts 1-2 replayed on a fresh dev database. The Act 1 and Act 2 outcome-state blocks match a query of that
      database, recorded as `Actual:` lines in the runbook.
- [ ] After the replay, `/activities/new` for a KGK registrar offers no two types with the same display name, and only
      types on the CPSA ladder. Browser check.
- [ ] The post-Act-2 snapshot is named in the runbook.

## Related

[T104] (closed 2026-09-24; this is the corpus half it never scoped), [T118] findings 6-7, W-006, W-007, the [T130]
banner, D31 and [T123] d2 (moot once the runbook stops building the duplicate).

## Note, 2026-09-24 (the T131 slices 3–4 merge)

Act 4 of `scenario-paediatrics.md` records the committee decision (Step 4.5) before staging STARs (Step 4.6). Under
T165 and T131 the order is now:
1. Start the review; its agenda is planned.
2. Stage a STAR (naming snapshot evidence), or defer each closing agenda line with a reason.
3. Record the decision with a quorate attendance (the chair plus one).
4. Ratify.

Reorder Act 4 when the runbook is retargeted.

