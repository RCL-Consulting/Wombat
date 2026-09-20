---
id: T110
title: "`scale_key: 'or_scale'` resolves to nothing, and four seeded tools quietly don't"
status: done
priority: P1
created: 2026-09-18
started: 2026-09-20
completed: 2026-09-20
---
# T110 — `scale_key: "or_scale"` resolves to nothing, and four seeded tools quietly don't

**Status:** done 2026-09-20 — binding and guard shipped; D25's merge still open
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
`execution/knowledge/scenario-paediatrics.md:160-166` is the same five rungs. Two ids, one ladder. Binding the tools to
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

---

## Outcome — 2026-09-20

### What was built

1. **The binding.** The four generic seeds now declare `"scale_key": "O-R Scale"` — the scale's exact
   name — instead of `"or_scale"`, which named nothing. **23 fields across four tools**: `acat` 6,
   `cbd` 5, `dops` 6, `mini_cex` 6.
2. **The duplication that allowed it.** `DataSeeder` now exposes `public const string OrScaleName`,
   used at both its own sites, so the test asserts against the same string the seeder writes rather
   than a third copy of it.
3. **The guard, which is the actual root cause.** `SeedScaleKeyTests` — a theory over every seed
   folder asserting that every `scale_key` names a seeded scale, plus a second over the rated field
   specifically, since a component scale binding to nothing is a display bug while the rated field
   binding to nothing is a silent credit failure. The seeded names come from the two places that
   seed one: `DataSeeder.OrScaleName` and `paediatric-epa-v11.1.json`.

**Names only, never a raw numeric id.** The resolver accepts an id, but an id is a fact about one
database — a seed shipping `"2"` binds to whatever scale happens to be second wherever it is restored.
The four operator-built `*_paed` types do exactly that; they are not seeds and are not covered.

### D25's merge is NOT done, and this lands safely without it

D25 recommends merging `"O-R Scale"` and `"Paed General Entrustment Scale"` **before** fixing the
binding, because binding the tools to one copy while a curriculum is pinned to the other would
mass-refuse legitimate credit. That hazard was checked in the browser rather than assumed:

| Curriculum | Items | Pinned to |
|---|---|---|
| 1 | 1 item pinned | **O-R Scale** |
| 2 | 15 | **nothing** — 15/15 unpinned |
| 3 | 15 | CPSA Paediatric Entrustment Scale v11.1 |

**Nothing anywhere pins `"Paed General Entrustment Scale"`.** So there is no curriculum against which
binding to the O-R copy could refuse anything, and the merge stays a tidy-up rather than a
prerequisite. It remains open under D25 and is still worth doing before anyone pins to either.

### The ⚠ in D25 about charts does not fire

D25 warns that *"fixing T110 makes every legacy chart gain a five-rung labelled axis at that moment"*
and that T110 and [T123] d1 change the same pictures. Checked: **no chart changes.** A chart's axis
comes from the trainee's pinned `CurriculumItem.ScaleId`, not from the activity's `scale_key`.
Curriculum 2 — the legacy paediatric world — is 15/15 unpinned, so its charts draw the numeric axis
they always drew and nothing about this change touches them. Curriculum 1's one pinned item was
already labelled from the O-R Scale before this.

What *does* change is invisible on a chart and is the whole point: a generic-tool completion under a
curriculum pinned to the O-R Scale now resolves its ladder and can be credited as verified, where
before it resolved to nothing and landed in `UnverifiedLevelCount`. Note that
`RebuildCurriculumProgressCommand` has no production caller, so **nothing recomputes existing
progress** — the improvement applies to completions from here.

### Verification

- [x] Every seed's `scale_key` names a seeded scale — `EveryScaleKeyNamesASeededScale`, a theory over
      all 14 folders
- [x] Every rated field binds to a seeded scale — `TheRatedFieldBindsToASeededScale`
- [x] **Verified to fail against the unfixed corpus.** Reverting `dops` to `or_scale` fails both
      theories for that seed, naming the field and listing the seeded scales:
      *"'dops' field 'preparation' declares scale_key 'or_scale', which is not the exact name of any
      seeded scale"*
- [x] The guard itself is honest — `TheGuardRejectsAKeyThatNamesNoSeededScale`
- [x] **Booted against the dev database.** `ActivityTypeSeedRefresher` republished **exactly the four
      generic seeds**, `v4 → v5 (schema)`, and no others — the CPSA seeds were already correct. That
      is the expected blast radius, observed rather than predicted.
- [x] `dotnet build Wombat.sln -c Release` — 0 warnings, 0 errors
- [x] Suites green, no `--no-build`: Domain **78**, Application **544**, Infrastructure **236**,
      Architecture **23**, Web **111** — **992 total**, up from 963

### Still open

- **D25's merge** — whether `"O-R Scale"` and `"Paed General Entrustment Scale"` are one ladder, and
  if so which survives. Operator data; the repo cannot confirm it. Not blocking, per the table above.
- **A stable slug** on `EntrustmentScale`, which D25 calls the better long-term answer. Not taken:
  `CreateEntrustmentScaleCommandHandler` never sets one, so a `NOT NULL UNIQUE` column would throw a
  raw index violation on the second admin-created scale. The create path has to be handled first.
- **No rebuild was run**, so existing completions keep whatever verification state they had.
