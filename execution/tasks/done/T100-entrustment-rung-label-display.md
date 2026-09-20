---
id: T100
title: "An ordinal is printed where a rung is meant, and on the CPSA ladder the two are different numbers"
status: done
priority: P3
created: 2026-09-17
---
# T100 — An ordinal is printed where a rung is meant, and on the CPSA ladder the two are different numbers

**Status:** tiers 1, 2a and 3 **DONE** 2026-09-19. The admin editor is split out as [T125]; the
trajectory chart is [T123] defect 1.
**Surfaced:** 2026-09-17 (anticipated by T098's trap list, then enumerated site by site).
**Premise rewritten:** 2026-09-19 from the [T118] evidence run and a check against the dev database. The
original framing was wrong in two ways that changed what the fix and its regression test should be — see
"What the first version of this task got wrong" below.
**Severity:** **Medium-High.** Not cosmetic. An assessor choosing a supervision level is shown two
different numbers per row with nothing saying which is the rung, and a trainee is told their target is a
rung that does not exist on their ladder.

## Symptom

The CPSA v11.1 ladder has six rungs at `Order` 1-6 labelled `1, 2, 3a, 3b, 4, 5`. Ordinal and rung are
**different numbers for four of the six rungs**, and the product prints the ordinal in places where the
reader can only mean the rung.

The entrustment picker, in full, as an assessor sees it today ([T118] finding 1):

```
1. 1      2. 2      3. 3a      4. 3b      5. 4      6. 5
```

| Stored Order | College rung | Rendered today | Reads as |
|---|---|---|---|
| 1 | 1  | `1. 1`  | a repetition with no explanation |
| 2 | 2  | `2. 2`  | a repetition with no explanation |
| 3 | 3a | `3. 3a` | plausible — "3" and "3a" look related |
| 4 | 3b | `4. 3b` | two unrelated numbers |
| 5 | 4  | `5. 4`  | **actively wrong** — the College's rung 4 labelled "5" |
| 6 | 5  | `6. 5`  | **actively wrong** — the College's rung 5 labelled "6" |

The second surface is worse than the picker, because there is no adjacent list to disambiguate it
([T118] finding 2). The progress page reads:

> Minimum level **3** (year 1) · reached 1 / 24

There is **no rung "3"** on the CPSA ladder. The rungs are 1, 2, 3a, 3b, 4, 5. "Level 3" is ambiguous
between rung 3a (whose ordinal is 3) and a rung that does not exist, and the reader has nothing to resolve
it against.

The third surface is the rating trajectory, whose y-axis prints bare ordinals against the same ladder.
That one is [T123] defect 1, which also has to fix the axis *length*; it is the same defect and the two
should land together.

## What the first version of this task got wrong

**1. "The ordinal was a useful prefix while `Order` and `Label` agreed (the old 5-rung O-R Scale)."**
They never agreed. Verified against dev, 2026-09-19:

| Scale | Rungs | Labels |
|---|---|---|
| 1 — O-R Scale | 5 | `Observe only`, `Direct supervision`, `Indirect supervision`, `Independent`, `Supervises others` |
| 2 — Paed General Entrustment Scale | 5 | `Observation only`, `Direct supervision`, `Indirect supervision`, `Unsupervised`, `Can supervise others` |
| 3 — CPSA Paediatric Entrustment Scale v11.1 | 6 | `1`, `2`, `3a`, `3b`, `4`, `5` |

Both legacy ladders carry **word** labels. The ordinal was never a redundant prefix on them; it was a
numbering of prose. So "`Order` and `Label` agreed" was never true of any scale in the product, and the
v11.1 ladder is not the exception that broke a working convention — it is the first ladder on which the
concatenation produces a *contradiction* rather than a *decoration*.

**2. The stated regression risk is backwards, and the test it proposes would assert the wrong thing.**
The old Verification section read: *"The 5-rung O-R Scale still reads correctly everywhere (labels there
are `1`..`5`, so the change is invisible on the old scale — which is the regression risk worth a test)."*
Both halves are false. The labels are words, and the change is therefore **highly visible** on the legacy
scales — which is where all the existing data is. Every `EntrustmentDecision` on dev is on scale 2:

| Today | After |
|---|---|
| `4. Unsupervised` | `Unsupervised` |
| `3. Indirect supervision` | `Indirect supervision` |

That is still the right rendering — it is what the reader means — but a test asserting *invariance* on the
old scale would fail on correct behaviour. **Assert the new string on both ladders, not invariance on one.**

## Sites

Line numbers below were re-verified 2026-09-19; several had drifted since the task was filed. The sweep
is now exhaustive for `src` (excluding `bin`/`obj`/`Migrations`) — no further concatenation or
bare-ordinal display site exists.

### ✅ Tier 1 — both halves already in hand, delete the ordinal (DONE 2026-09-19)

| Site | Filed as | Actually at |
|---|---|---|
| `ActivityReferenceDataService.GetEntrustmentScaleLevelOptionsAsync` — the WBA rating picker | `:166-168` | **`:314-316`** |
| `CommitteeDecisions/ReviewDetail.razor` — STAR authorised-level picker | `:165` | **`:187`** |
| `Admin/EntrustmentDecisions/Index.razor` | `:73` | `:73` |
| `Portfolio/MyAuthorisations.razor` | `:28` | `:28` |
| `Reporting/EntrustmentCertificatePdfService.cs` — the STAR certificate | `:105` | `:105` |
| `Reporting/EntrustmentSummaryComponent.cs` — the portfolio PDF | `:54` | `:54` |
| **`IActivityReferenceDataService.cs:41`** — the XML doc contracting `"order. label"` in prose | *not filed* | `:41` |

`ReviewDetail.razor:220` (filed as `:198`) already printed the label alone and was left alone — it is
correct **by construction**, not by discipline: `PendingEntrustmentDecisionDto` carries no ordinal at all.

⚠️ **`ActivityCatalogueOption.Value` stays `level.Order.ToString()`.** That is the value written into
`Activity.DataJson`, and `CreditApplier.cs:235` carries a comment stating it mirrors this method exactly.
Only the display half moved. `ActivityReferenceDataServiceTests.cs:111` guards the value and was not
touched; `:112` guarded the format and was inverted.

### ✅ Tier 2a — thread a label to the two "Minimum level N" sites (DONE 2026-09-19)

This is [T118] finding 2, and it was the worst surface: no adjacent list to disambiguate "Minimum level 3"
against a ladder with no rung 3.

- `Dashboards/TraineeDashboard.razor:41` and `Portfolio/MyProgress.razor:35` print the same sentence from
  **two different DTOs built by two different handlers** — `CurriculumProgressItem`
  (`TraineeDashboardSummaryDto.cs:10-17`) and `TraineeCurriculumProgressDto`
  (`GetCurriculumProgressForTrainee.cs:29-39`). Both gained `EffectiveMinimumLevelLabel`; both EF
  projections gained `CurriculumItem.ScaleId`, which neither selected.
- The shared resolver is `Wombat.Application/Features/Epas/EntrustmentRungLabels.cs` — **public**, because
  `EntrustmentScaleReferences` next door is `internal` and there is no `InternalsVisibleTo` from
  Application to Infrastructure, so an internal one could not be called from the PDF or the picker.

### ✅ Tier 3 — the portfolio PDF printing a raw `DataJson` integer (DONE 2026-09-19)

`Reporting/ActivitiesSectionComponent.cs` (filed at `:115-125`/`:161-168`, actually `:120-132`/`:168-176`)
printed *"Supervision required for this encounter: 5"*. It had the best data position of any site — the
parsed `FormSchema` is in hand and `FormField` carries both `Type` and `ScaleKey` — and needed only an
ordinal→label map. `PortfolioPdfService` now resolves every `scale_key` its loaded schema versions declare
and hands the lookup to the composer, which has no database of its own.

### ⏸ Split out — the admin curriculum-item editor

`Admin/Curricula/CurriculumItemsEdit.razor` was filed here as two bare-ordinal displays at `:74`/`:113`.
Re-reading it, **only one of those is a display** (`:86`, drifted from `:74`). `:60` and `:124` are
free-text **number inputs**, and `:70`/`:139` are raw stage-map **JSON textareas**. An administrator types
a raw integer with no indication what it means on the ladder the item is pinned to — an unguided write,
not a mislabelled read, and the fix is a rung `<select>` rather than a formatter. The page already loads
the scales (`:166`, `:195`) and already renders a scale `<select>`, so the rungs are one query away.
**Filed as [T125].**

### Out of scope, confirmed

- `Admin/EntrustmentScales/EntrustmentScaleEdit.razor:74` — correct by construction, own column under its
  own `<th>Order</th>`. This is the one place the ordinal is legitimately the datum being read, because it
  is the rank being edited. D32 keeps it.
- `MsfAggregationService.cs:50-53` (filed as `:41-42`) — MSF answers are not `EntrustmentLevel`-backed, so
  there is no ladder to resolve labels from. Correctly excluded.

### The trajectory chart

`TrajectoryChart.razor` `:153-155` (y-axis), `:45` (tooltip), `:69` (screen-reader table) are the third
surface and are **[T123] defect 1**, which also has to fix the axis *length*. Note `ChartPoint.Label`
(`:161`) is **not** a rung label — both call sites pass `point.Source` ("Direct observation"), and the
screen-reader column above it is headed "Source". A rung label needs a second field.

## Verification

- The rating picker on a CPSA Mini-CEX lists `1, 2, 3a, 3b, 4, 5` — six entries, no ordinal prefixes, no
  `5. 4`.
- A curriculum-3 trainee's progress page shows the target as `3b`, never `4` and never `4. 3b`, and the
  year-1 minimum as `3a`, never `3`.
- The STAR certificate and the portfolio PDF print rung labels.
- **On the legacy 5-rung ladders the rendering changes too, and that is correct.** `4. Unsupervised`
  becomes `Unsupervised` on every existing `EntrustmentDecision`. Assert the new string; do **not** write
  the invariance test the previous version of this file asked for — it would fail on correct behaviour.
- The admin scale editor still shows the ordinal, in its own column (D32).

## Related

Lands on the form that [T070] rebuilt. [T123] defect 1 is the same defect on the trajectory chart and
additionally fixes the axis length; **the two should land together** — they are one change to what a rung
looks like, on three surfaces. D32 settles the rendering rule (label alone); D29 settles the narrower
question of what fits on a 40px chart axis. Premise evidence in [T118] findings 1 and 2. Listed in
`T098-epa-v11-adoption.md` trap 5 and phase 2.
