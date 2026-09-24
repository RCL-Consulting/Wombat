---
id: T166
title: The committee cannot see where a trainee stands against Annexure A's target level for their year, or against the exit rule
status: queued
priority: P2
owner: agent
depends_on: []
created: 2026-09-24
---

# T166 — Nothing compares a trainee's entrustment decisions with Annexure A's year targets or the exit rule

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium. Annual progression and pre-graduation reviews exist to judge a registrar against Annexure A's
year targets and the programme exit rule. Wombat holds both numbers but never compares a trainee's entrustment decisions
with either, so the committee cannot see who is behind.
**Surfaced:** 2026-09-24, the EPA-stream survey, gaps "Annexure A target per year" and "the programme exit rule". [T124]
§ 5c recorded the exit rule as "Not filed anywhere; noted here for phase 4". [T131] (phase 4) never picked it up.

## Symptom

Observed at `431e69e`:

- Annexure A gives a target level per training year for every EPA (`tasks/done/T098-data/annexure-a.json`, `y1`-`y4`
  per entry). The catalogue stores them as `MinimumLevelByStageJson`. They are read only as a **per-encounter credit
  minimum**: `TraineeQuotaProgress.cs:206-210` via `CurriculumItem.GetMinimumLevelForStage`.
- The exit rule, "Level 5 in 9 EPAs and Level 4 in the remaining 6" (`tasks/done/T124-page-8-extraction-findings.md:161-164`),
  exists only as per-item final minima: `minimumLevelOrder` 6 (rung 5) for PAED-001 to 007, 010 and 012, and 5 (rung 4)
  for 008, 009, 011 and 013 to 015 (`paediatric-epa-v11.1.json`). Nothing evaluates it across the set.
- `GetActiveDecisionsForTrainee` has one consumer, `MyAuthorisations.razor:80`, which shows no target. The committee
  review page loads no targets (`ReviewDetail.razor:351-357`).
- A `Graduate` decision (`CommitteeDecisionCategory.cs:13-17`) is recorded with no check against any EPA, and
  `CompleteTraineeProfile.cs:44-58` completes the profile with no entrustment check.

## What to build

1. **A per-trainee standing query.** For each curriculum item (scoped on `OwningInstitutionId`), return the active
   entrustment decision's rung, the target for the trainee's current year, the final (exit) level, and a status: at or
   above, below, no decision, or not comparable. Compare only on the item's pinned ladder ([T109]); a decision on
   another ladder is "not comparable", never coerced. The query takes a `ClaimsPrincipal` and follows [T113]'s read
   ladder.
2. **Target source (decide and record).** Recommendation: read the per-stage map as the year target. It is Annexure A's
   `y1`-`y4`, and a second copy would drift. Add a separate column only if the College distinguishes the two.
3. **The exit rule.** Inferred: "every item's active decision at or above its final `MinimumLevelOrder`" is equivalent
   to "Level 5 in 9, Level 4 in 6", because the seed pins which nine and which six. Show it as "n of 15 at exit level",
   naming the EPAs that are short.
4. **Surfaces.** A per-EPA standing table on the committee review page, and targets on the trainee's
   `MyAuthorisations`. Follow DESIGN.md.
5. **Graduation (operator decision).** When a `Graduate` decision is recorded, or the profile is completed, with the
   exit rule unmet: refuse, or warn and require a recorded reason. Recommendation: warn and require a reason. The
   committee holds the authority; the product makes the gap visible and recorded.

## Verification

- [ ] A year-2 trainee with an active 3a on an EPA whose year-2 target is 3b reads "below". With 3b it reads "at". With
      no decision it reads "no decision". Query tests.
- [ ] A trainee at every final minimum reads "meets the exit rule"; one EPA short names that EPA. Query test.
- [ ] A decision on a different ladder reads "not comparable". Test.
- [ ] Recording `Graduate` with the rule unmet behaves as decided. Application test.
- [ ] The review page shows the standing table for the dev trainee. Browser.
- [ ] Full suite green, no `--no-build`.

## Related

[T131] (sibling, not scope), [T165] (who takes the decision), [T167] (the evidence view), [T124] § 5c, [T113] (scope of
`GetActiveDecisionsForTrainee`), [T109] (same-ladder comparison), [T073] (per-stage minimum), [T139] (`GetStage` is
uncapped and there is no programme length).
