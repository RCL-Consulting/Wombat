---
id: T110
title: "`scale_key: 'or_scale'` resolves to nothing, and four seeded tools quietly don't"
status: queued
priority: P2
created: 2026-09-18
---
# T110 — `scale_key: "or_scale"` resolves to nothing, and four seeded tools quietly don't

**Status:** open
**Surfaced:** 2026-09-18, while scoping T109.
**Severity:** Medium — latent today, load-bearing the moment a curriculum is pinned.

## Symptom

`mini_cex`, `cbd`, `dops` and `acat` each declare `"scale_key": "or_scale"` on their rating fields
(`mini_cex/schema.json:20-25`, `cbd/schema.json:18-22`, `dops/schema.json:20-25`, `acat/schema.json:18-23`).

The only scale `DataSeeder` creates is **named `"O-R Scale"`** (`DataSeeder.cs:113,119`).

`ActivityReferenceDataService.GetEntrustmentScaleLevelOptionsAsync:299-303` resolves a `scale_key` by numeric
id or **exact name**. `"or_scale"` is neither. It resolves to nothing, for all four tools.

## Why nobody has noticed

Those same fields also declare inline `options: ["1".."5"]`, and `ActivityForm.GetOptions`
(`ActivityForm.razor:236-241`) returns `field.Options` first. So the rung picker renders from the inline list
and looks perfectly healthy, while the scale binding behind it has never once resolved.

The four `_cpsa` tools do not have this problem: they name the scale exactly
(`"CPSA Paediatric Entrustment Scale v11.1"`), and `CpsaWbaSeedTests.cs:53-58` pins that. **No equivalent
guard exists for the four generic seeds, which is why this drifted unnoticed.**

## Why it matters now

T109 made `scale_key` load-bearing for credit, not just for rendering. An unresolvable key lands in
`LevelComparisonBasis.Unpinned`, which compares bare ordinals exactly as before — so today this is harmless.

It stops being harmless the moment an administrator pins a curriculum item to the O-R Scale. The tools that
should match it still resolve to nothing, so their completions are recorded as `UnverifiedLevelCount` rather
than as verified credit, for ever.

## Why it was deliberately NOT fixed inside T109

Because the obvious fix is the single largest blast radius in that change, and it buys nothing for the defect
T109 repairs. Giving the O-R Scale the key `or_scale` (or renaming the seeds to `"O-R Scale"`) binds all four
generic tools to scale 1. Meanwhile the operator-built five-rung `"Paed General Entrustment Scale"` is
**the same ten-Cate ladder, duplicated** — `DataSeeder.cs:123-127` seeds O-R as Observe only / Direct
supervision / Indirect supervision / Independent / Supervises others, and the browser-made scale in
`Programme/scenario-paediatrics.md:160-166` is the same five rungs. Two ids, one ladder. Binding the tools to
one copy while a curriculum is pinned to the other mass-refuses legitimate historic credit.

**So the duplicate ladder has to be reconciled first, or at the same time.** That is a data decision about
the live database, not a code change.

## Work

1. Decide whether the five-rung `"Paed General Entrustment Scale"` and `"O-R Scale"` are the same ladder. If
   they are, merge them: re-point every reference and delete one. Do it before pinning anything to either.
2. Fix the binding — either rename the four seeds' `scale_key` to `"O-R Scale"`, or give `EntrustmentScale` a
   stable slug and make the resolver accept it. A slug is the better answer but note that
   `CreateEntrustmentScaleCommandHandler:34-47` never sets one, so a `NOT NULL UNIQUE` slug column would
   throw a raw unique-index violation on the second admin-created scale. Handle the create path first.
3. Extend `CpsaWbaSeedTests` (or add the equivalent) so **every** seed folder's `scale_key` is asserted to
   resolve against the seeded scales. The absence of that assertion is the actual root cause.

## Related

Found while scoping `T109`, which documents why it stayed out of scope. Interacts with `T104` (the legacy
paediatric data this duplicate ladder is part of).
