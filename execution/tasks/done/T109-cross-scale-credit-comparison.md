---
id: T109
title: "A rating on one entrustment scale is compared against a minimum on another, and silently credits"
status: done
priority: P1
created: 2026-09-17
---
# T109 — A rating on one entrustment scale is compared against a minimum on another, and silently credits

**Status:** **SHIPPED 2026-09-18**, commit `d5025e6` — verified by test, **not yet in a browser** (see below).
**Surfaced:** 2026-09-17, adversarial review of T108.
**Severity:** High — this is silently **wrong** credit, which is worse than T108's silently **absent**
credit. A trainee can be recorded as having met a supervision minimum they have not met.

## Symptom

A KGK trainee is pinned to curriculum 2 (`FCPaed(SA) Part 1`), whose items carry minima expressed as
orders on the **five-rung** "Paed General Entrustment Scale" — `MinimumLevelOrder = 4` means
*"Independent"*.

They file a `mini_cex_cpsa`, which binds `overall_level` to the **six-rung** CPSA v11.1 ladder, where
order 4 is the rung labelled **"3b"** — *"call me if you're worried"*, still requiring indirect
supervision.

The assessor rates order 4. `CreditApplier.MeetsMinimumLevel` evaluates `4 >= 4` and records
`MinimumLevelReachedCount++`. **The trainee is credited with independent practice on the strength of an
assessment that said the opposite.**

## Why T108 makes this matter now

The mis-comparison was always reachable — nothing ever stopped that trainee picking a curriculum-2 EPA on a
CPSA tool. What T108 changed is that it is now the **only** path: the picker narrows to the subject's
curriculum, so the CPSA EPAs (which would have credited nothing, visibly wrong after T108's second half)
are gone, and the curriculum-2 EPAs that mis-credit are all that remain.

T108 did not create this. It removed the escape hatch, which is the right thing to have done — an
uncreditable option is not a safety feature — but it means this can no longer be deferred.

## Root cause — the hazard T098 identified and left open

> "Nothing binds a stored entrustment level to a scale. `CurriculumItem.MinimumLevelOrder`,
> `MinimumLevelByStageJson` values, activity `DataJson` scale values and `MsfResponseAnswer.ScaleValue`
> all persist a **bare integer** with no `ScaleId` and no version pin."
> — `T098-epa-v11-adoption.md`

`CreditApplier.MeetsMinimumLevel` compares two bare integers that come from different scales and have no
way of knowing it. The activity's scale comes from the schema's `scale_key`; the curriculum item's comes
from its sub-speciality's `DefaultEntrustmentScaleId`. Nothing checks they agree.

## Options

1. **Pin the ordinal to a scale** (T098's preferred option): add `ScaleId` to `CurriculumItem`, record the
   scale on the activity value or its schema version, and make `MeetsMinimumLevel` refuse — loudly — to
   compare across scales. The real fix; it also unblocks T104's re-pinning hazard.
2. **Refuse the combination at source.** Do not offer an activity type whose `scale_key` resolves to a
   different scale from the subject's curriculum's. Cheaper, and it is the same predicate T108 applied to
   EPAs, applied one level up to `ListActivityTypesQuery`. It does not fix the underlying comparison, but
   it makes the mismatch unreachable through the product.
3. **Detect and flag at credit time**, reusing T108's `CreditedItemCount` stamp — a fourth state meaning
   "credited, but across scales". Weakest: it records the problem after the fact.

Recommendation: **2 now** (it is small, and it closes the live hole created by having two scales in one
institution), then **1** as the durable fix, before any institution runs two catalogues concurrently for
real.

## Verification

- A trainee pinned to a five-rung curriculum is not offered a six-rung activity type.
- After option 1: a cross-scale comparison raises rather than silently passing, and a
  `CurriculumItemProgress` row can state which scale its levels are on.
- The legitimate case is unaffected: a CPSA-curriculum trainee using a CPSA tool credits exactly as now.

## Related

The concrete instance of `T098`'s scale-pinning hazard. Made unavoidable (not caused) by `T108`. Blocks
`T104`, which cannot re-pin a trainee from curriculum 2 to curriculum 3 without exactly this remap.

---

## Chosen design — option 1, agreed 2026-09-18

Option 1 ("pin the ordinal to a scale"). Designed three ways and adversarially judged; no design survived
both a correctness and a production-safety lens unamended, so what follows is the synthesis, with the
amendments the judges forced written in.

### The shape

| Side | Where the scale comes from |
|------|----------------------------|
| Curriculum | **A real column.** `CurriculumItem.ScaleId` (nullable FK to `EntrustmentScales`). Governs `MinimumLevelOrder` *and* every value in `MinimumLevelByStageJson`. |
| Activity | **Derived, not stored.** The directive's `minimum_level_field` names a field in the activity's **pinned** `ActivityTypeVersion.SchemaJson`; that field's existing `scale_key` resolves to a scale id. |

Deriving the activity side from the pinned version means the answer is immutable for the life of the
activity, needs no backfill, and replays identically under `RebuildCurriculumProgress`. It costs one thing:
`CreditApplier` cannot see the schema today, because both call sites hand it a synthetic
`new ActivityType { CreditRulesJson = … }` (`ActivityService.cs:218-223`,
`RebuildCurriculumProgressCommand.cs:60-65`). Both now pass `SchemaJson` as well.

### The comparison

A pure Domain function, `EntrustmentLevelComparer.Compare`, returning `(bool MinimumMet, LevelComparisonBasis Basis)`:

| Basis | When | `MinimumMet` |
|-------|------|--------------|
| `NoGate` | the directive names neither `minimum_level_field` nor `minimum_level_fixed` | `true` — unchanged |
| `ValueMissing` | gated, but the activity carries no readable ordinal | `false` — unchanged |
| `SameScale` | both sides resolve to the **same** scale | `provided >= required` |
| `ScaleMismatch` | both sides resolve and **differ** | **`false` — this is T109** |
| `Unpinned` | either side is unknown | `provided >= required` — **exactly as today** |

`Unpinned` is the load-bearing default. A type with no scale field, an unresolvable `scale_key`, a
`minimum_level_fixed` literal, and every curriculum item not yet pinned all land there and behave precisely
as they do now. **Nothing that credits today stops crediting because of this change.**

### It never throws

`CreditApplier.ApplyAsync` runs between `activity.ApplyTransition` (state and data already mutated) and the
single `SaveChangesAsync` at `ActivityService.cs:229`. Throwing there rolls back a completed, schema-valid
assessment and shows the assessor an error about a curriculum-item scale they cannot fix. So the refusal
declines the *minimum* and keeps the *volume*: `CountsSoFar` increments in every case, because the encounter
did happen. Only `MinimumLevelReachedCount` is withheld.

### What gets recorded

- `CurriculumItemProgress.MinimumLevelScaleId` — the scale the stored tally was actually computed on.
- `CurriculumItemProgress.ScaleMismatchCount` — counted for volume, refused the minimum.
- `CurriculumItemProgress.UnverifiedLevelCount` — compared without scale proof. This is the exposure meter:
  it is how an operator knows how much of the estate is still unpinned.
- `ActivityTransition.CreditScaleMismatchCount` — sibling of T108's `CreditedItemCount`, same three-valued
  contract (`null` = credit never evaluated).

### The migration backfills NOTHING — deliberately

Every design proposed backfilling `CurriculumItem.ScaleId` from `Curriculum.SubSpeciality.DefaultEntrustmentScaleId`,
and every production-safety judge killed it. Two independent reasons, both verified:

1. **That field is force-overwritten on every boot.** `PaediatricCatalogueSeeder.EnsureDefaultScaleAsync:223-236`
   sets the Paediatrics sub-speciality's default to the **six-rung CPSA** scale unconditionally, clobbering
   whatever an admin chose in `SubSpecialityEdit.razor:47`. On any database where the paediatric catalogue has
   booted, reading it back yields the wrong ladder for a five-rung curriculum — so the backfill would pin
   curriculum 2's five-rung minima to the six-rung scale and **certify the very defect this task repairs**.
2. **It cannot express the case anyway.** `Curriculum.CloneAsNewVersion:22` copies `SubSpecialityId`, so every
   version of a curriculum necessarily shares one sub-speciality and therefore one default. T109's scenario is
   two curriculum versions on two ladders — precisely what a per-sub-speciality key cannot represent.

And a wrong pin is **not** symmetric with a null one: `ScaleMismatch` refuses unconditionally, so a single bad
pin silently converts a trainee's legitimately earned `MinimumLevelReachedCount` into a refusal.

So: the migration adds nullable columns and writes no data. **Deploying this changes no trainee's progress.**
Pins arrive by provenance only, from the seeders that own those rows and know the answer:

- `PaediatricCatalogueSeeder` pins its own curriculum items to the CPSA v11.1 scale it seeds.
- `DataSeeder` pins the IM demo curriculum items to the O-R Scale it seeds.

Everything else — including the operator-built curriculum 2 that this task was raised for — stays `NULL` and
behaves as today until pinned explicitly. `UnverifiedLevelCount` is what makes that state visible rather than
silent.

### The explicit pin (added after adversarial review)

The first cut of this shipped the engine with no way to feed it: `CurriculumItem.ScaleId` was writable only
by the two seeders, so the curriculum this task was raised against could never have been pinned through the
product, and the task's own verification criterion was unsatisfiable. That is now closed:

- `ScaleId` flows through `CurriculumItemDto`, `AddCurriculumItemCommand`, `UpdateCurriculumItemCommand`,
  both handlers, `CurriculumMappings` and both `GetCurricula` projections.
- `CurriculumItemsEdit.razor` gains a **Scale** column and a picker whose default option is *Not pinned*.
- `CurriculumMappings.EnsureScaleCanExpressMinimaAsync` refuses a pin to a ladder that cannot express the
  item's ordinals — the flat `MinimumLevelOrder` **and** every value in `MinimumLevelByStageJson`. It catches
  the obvious half (a minimum of 6 pinned to a five-rung scale); it cannot catch a pin that is wrong but
  arithmetically plausible, which is exactly why a human chooses it and nothing infers it.
- Unpinning is allowed. An administrator who becomes unsure must be able to withdraw an assertion.

### Three guards protect the pin, because nothing in the database does

Two of the ways a scale is depended on are invisible to the schema: a `scale_key` is a bare string, not a
foreign key, and a pinned item's *rungs* are not protected by the FK that protects the scale's existence.
`EntrustmentScaleReferences` holds both checks so the rename path and the delete path cannot disagree — a
guard on one door is the same hole with an extra step.

### Renaming or deleting a scale is now blocked when a schema binds to it

`scale_key` binds by **exact name**, so renaming a scale silently unbinds every schema that names it: the key
stops resolving, the engine falls back to comparing bare ordinals, and this protection disappears with no
error anywhere — while the rung picker empties at the same moment. Pinned versions make it worse, not better:
an in-flight activity keeps pointing at the old version, whose schema still carries the old name.

`UpdateEntrustmentScaleCommandHandler` now refuses such a rename and names the activity type and version that
binds to it. **`DeleteEntrustmentScaleCommandHandler` refuses the same thing**, because deleting the scale
breaks the binding exactly as renaming it does — and adversarial review found that door open after the first
one was shut. Note the asymmetry that made it easy to miss: the FK-backed checks guard the *required* side of
the comparison (the curriculum item's pin), while `scale_key` is the only thing that feeds the *achieved*
side, and nothing FK-backed touches it at all.

That is the blunt fix. The durable one is to resolve `scale_key` to an id at publish time so the binding stops
depending on a mutable string — a DSL change, and backlog.

### Removing a rung out from under a pinned item is blocked

`EnsureScaleCanExpressMinimaAsync` establishes "every ordinal a pinned item uses is a real rung on its scale",
but it only runs when the **item** changes. The scale can move too. Because the level validator forces the
incoming set to be contiguous from 1, a removal always removes the **top** rung — precisely the one an
"Independent" or "Supervises others" minimum names. `ThrowIfPinnedItemNeedsARemovedRungAsync` refuses that
and names the curriculum item, rather than leaving a minimum that no assessment could ever meet again.

### Deleting a scale explains itself

The two new foreign keys are `ON DELETE RESTRICT`, so `DeleteEntrustmentScaleCommandHandler` gained the two
matching pre-flight checks as well. Without them an administrator deleting a pinned scale got a raw
`DbUpdateException` where a sentence belonged.

### Explicitly out of scope, with reasons

- **`MsfResponseAnswer.ScaleValue` needs no scale column.** `MsfQuestion.ScaleId` already exists
  (`MsfQuestion.cs:10`) and `MsfAggregationService.cs:34-36` aggregates strictly per `question.Id`, so MSF
  values are never compared across scales. T098's hazard note overstated this one.
- **The `or_scale` / `"O-R Scale"` key mismatch is NOT fixed here.** Today `scale_key: "or_scale"` on
  `mini_cex`/`cbd`/`dops`/`acat` resolves to nothing, so those land in `Unpinned` and are unchanged. Making it
  resolve would bind them to the O-R Scale — a five-rung ten-Cate ladder semantically identical to the
  operator-made "Paed General Entrustment Scale", i.e. the same ladder duplicated — and mass-refuse legitimate
  historic credit the moment a curriculum is pinned to the other copy. That is the largest blast radius in the
  whole change and it buys nothing for this defect. Filed separately.
- **`EntrustmentScales.Key` is not added.** `CreateEntrustmentScaleCommandHandler:34-47` never sets it, so a
  `NOT NULL UNIQUE` key would throw a raw unique-index violation on the second scale an admin creates.

### What is still not closed

- **The refusal is surfaced on the activity, not on the progress page.** `ActivityView` warns when a
  transition counted volume but refused the level. `CurriculumItemProgress.ScaleMismatchCount` and
  `UnverifiedLevelCount` are stored and are not yet read by any dashboard or progress surface.
- **`RebuildCurriculumProgressCommand` still stamps nothing** (T106 item 12) and still has no caller, so
  there is no in-product way to re-score history after a pin is corrected.
- **Existing mis-credited rows are not repaired.** Pinning a curriculum item changes what future completions
  do; it does not revisit `MinimumLevelReachedCount` already awarded. There is no remediation path short of
  a rebuild, which is the previous bullet.

### Verification

All three criteria are covered by automated tests, in
`tests/Wombat.Application.Tests/Activities/CreditApplierScalePinningTests.cs` and
`tests/Wombat.Domain.Tests/Epas/EntrustmentLevelComparerTests.cs`:

- ✅ A trainee pinned to a five-rung curriculum, filing a six-rung CPSA tool, counts for volume and **not**
  for the minimum, and the refusal is visible on the progress row and the transition.
- ✅ The legitimate case is unaffected: a CPSA-curriculum trainee using a CPSA tool credits exactly as now.
- ✅ Every currently-crediting combination still credits: no scale field, unresolvable `scale_key`,
  `minimum_level_fixed`, unpinned curriculum item.

**Browser verification is deliberately outstanding, and it is blocked on a data step rather than on code.**
No curriculum in the dev database is pinned to a scale — the migration backfills nothing by design — so the
mis-crediting scenario cannot be reproduced through the UI until curriculum 2 (`FCPaed(SA) Part 1`) is
pinned to the five-rung ladder via Admin → Curricula → items → **Scale**. Pinning it *is* the first item of
the next session, so the browser check belongs there: pin, file a CPSA Mini-CEX as a curriculum-2 trainee,
and confirm the activity shows the cross-scale banner while `CountsSoFar` still increments.

Before pinning, check which sub-speciality curriculum 2 actually sits under. The repository cannot answer
that, and it decides whether `PaediatricCatalogueSeeder.EnsureDefaultScaleAsync` has already overwritten
that sub-speciality's default entrustment scale.
