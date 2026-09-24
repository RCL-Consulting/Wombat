---
id: T118
title: "What the first real v11.1 assessment showed"
status: queued
priority: P3
created: 2026-09-19
---
# T118 — What the first real v11.1 assessment showed

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

> **This is a holding file, not a task.** It is a container for findings that have not been
> split out yet. **Do not "do" T118** — pick an item from the list below, file it with
> `harness.py task new`, and strike it through here. T118 closes when the list is empty.
> Rated P3 on 2026-09-20 so it stops ranking alongside actionable defects.

**Status:** open (holding file — each finding needs its own fix or an existing task updated)
**Surfaced:** 2026-09-19, the Wave-0 evidence run.
**Severity:** mixed; two items below are clinician-facing and wrong, not merely ugly.

## What was done

The Paediatric EPA v11.1 catalogue had **never been used**: no trainee on curriculum 3, no adoption row,
and it had never credited anything. Every severity judgement about it was therefore a guess. So, through
the UI on dev:

1. Institution 2 adopted `Paediatric EPA Curriculum 11.1` (`/admin/adoptions`).
2. Trainee Ndlovu was moved from curriculum 2 to curriculum 3 (`/admin/trainees/edit?id=6`).
3. Ndlovu filed a `mini_cex_cpsa` against PAED-001, `observed_on` **2026-03-10**, and submitted it.
4. Assessor Naidoo completed it with a rating of **3a** (ordinal 3).

## ✅ What works

- **The catalogue credits.** `CreditedItemCount = 1`, `CreditScaleMismatchCount = 0`. A progress row
  appeared for curriculum item 17 (PAED-001): `CountsSoFar = 1`, `MinimumLevelReachedCount = 1`. This is
  the first credit the v11.1 catalogue has ever produced.
- **T109 holds** — same-scale comparison, no mismatch, because both sides resolve to the six-rung CPSA
  ladder.
- **T073's per-stage minimum works** — the item's `MinimumLevelOrder` is 6 (the final-year target) but
  the year-1 minimum of 3 was applied, so a 3a rating correctly counted.
- **T101's stamping followed the curriculum move** — the new activity is stamped speciality 3, where
  Ndlovu's earlier ones were speciality 2.
- **T070 field ownership works** — the assessor's fields were disabled for the trainee and enabled for
  the assessor.
- **T108's EPA narrowing works** — the picker offered exactly the 15 v11.1 EPAs with their v11.1 titles.

## 🔴 Findings

### 1. The entrustment picker is genuinely confusing, not just ugly (T100 is understated)

The dropdown renders:

```
1. 1      2. 2      3. 3a      4. 3b      5. 4      6. 5
```

Rungs 1 and 2 read as "1. 1" and "2. 2"; rung 4 reads as **"5. 4"** and rung 5 as **"6. 5"**. An assessor
choosing a supervision level sees two different numbers per row and no indication which is the rung.
T100's task file describes this as "3. 3a" and assumes elsewhere that labels are numeric and the ordinal
is the useful part — on this ladder the ordinal is actively misleading. **Update T100 before fixing it.**

### 2. The progress page says "Minimum level 3" for a rung called "3a"

`Minimum level 3 (year 1) · reached 1 / 24`. There is no rung "3" on the CPSA ladder — the rungs are
1, 2, 3a, 3b, 4, 5 — so "level 3" is ambiguous between rung 3a (ordinal 3) and a rung that does not
exist. Second surface for T100, and worse than the picker because there is no list to disambiguate it.

### 3. The quota is visibly wrong: "1 / 24"

> **Closed by [T130], 2026-09-23.** The page now reads each EPA against its published target for the current
> period — "1 of 3 this semester" for PAED-001 — with the College's D14 exemption and the previous period's result.

PAED-001 requires **six per annum**. The page says `1 / 24`, because `PaediatricCatalogueSeeder.cs:356-361`
multiplies the annual quota by four programme years. The other EPAs read `0 / 4`, `0 / 8`, `0 / 16`,
`0 / 24`. A registrar cannot tell from this page what is expected of them **this year**, which is the only
question they actually have. This is T098 phase 3 made concrete and it is the single most visible gap.

### 4. `observed_on` being ignored is now visible on a clinician-facing chart

The encounter was recorded as **2026-03-10**. The rating trajectory plots it at **2026-09-19** and the
table beneath reads `2026-09-19`, because `CreditApplier.ResolveObservationDate` uses `Activity.CreatedOn`.
The form's own helper text says *"When the encounter happened, not when this form is completed."* The
product asks for the date, promises it matters, and then discards it.

This run did **not** exercise the credit consequence — both dates fall in stage 1, so the stage minimum
was the same either way. The display defect is proven; the crediting defect remains latent and will bite
the moment a date crosses a year boundary. Still unfiled as its own task.

### 5. NEW — the trajectory chart has a five-point axis for a six-rung ladder

The y-axis renders `1 2 3 4 5`. The CPSA scale has **six** rungs. A rating of ordinal 6 (rung "5") has
nowhere to plot. Not in any task file. Needs checking against `TrajectoryChart` before anyone reads a
paediatric trajectory as meaningful.

### 6. NEW — two indistinguishable "Mini-CEX (Paediatrics)" in the picker

`/activities/new` offers eight paediatric types, of which **two are called "Mini-CEX (Paediatrics)"** (ids
11 `mini_cex_paed` and 17 `mini_cex_cpsa`) and **two "DOPS (Paediatrics)"** (12 `dops_paed`, 18
`dops_cpsa`). Nothing on screen distinguishes them. A trainee picking the wrong one files against the
wrong world and gets no credit. This is [T104]'s legacy world colliding with the new one, and it is a
reason to bring T104 forward rather than leave it last.

### 7. NEW — the picker offers a v11.1 trainee the four legacy institution-scoped types

Ndlovu is on curriculum 3 (speciality 3). The picker still offers `mini_cex_paed`, `dops_paed`,
`procedure_log_paed` and `msf_paed` — all `Scope = Institution, ScopeId = 2`. Offering is by institution
scope, with no reference to the trainee's curriculum, so the two paediatric worlds are both on the menu.
Related to finding 6 and to [T108], which narrowed the EPA picker but not the type picker.

### 8. To verify — the trajectory labels a Mini-CEX as "Direct observation"

The trajectory table's Source column reads `Direct observation` for a `mini_cex_cpsa`. May be a
deliberate category rather than the tool name; confirm against `GetEpaTrajectoryForTraineeQuery`'s source
mapping before treating it as a defect.

## Evidence left in the dev database

Adoption row 2 (institution 2 → curriculum 3); Ndlovu moved to curriculum 3; activity 12
(`mini_cex_cpsa`, completed, rated 3a, `observed_on` 2026-03-10); one progress row on item 17. **Ndlovu
was moved off curriculum 2 and has not been moved back** — that is the only destructive part of this run
and it is trivially reversible at `/admin/trainees/edit?id=6`.
