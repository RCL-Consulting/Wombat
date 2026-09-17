# T109 — A rating on one entrustment scale is compared against a minimum on another, and silently credits

**Status:** open — **the most serious open defect in the credit path**
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
