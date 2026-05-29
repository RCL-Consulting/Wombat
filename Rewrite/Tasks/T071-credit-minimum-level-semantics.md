# T071 — Credit `minimum_level_field` is an all-or-nothing gate (two-counter model is dead)

**Status:** open — **domain decision required** (found during Act 3 play-through, 2026-05-29)
**Severity:** High (affects whether trainees get curriculum credit) but needs a product call first.
**Surface:** `src/Wombat.Infrastructure/Activities/CreditApplier.cs` (+ the scenario expectation).

## Observation

`CurriculumItemProgress` has two counters: `CountsSoFar` and `MinimumLevelReachedCount`. The intent
(and the scenario, Step 3.6: "PAED-001 … 1 of 30" with the level *below* the stage target) is that
**volume accrues regardless of level, while level-attainment is tracked separately.**

The implementation does not do this. In `CreditApplier.ApplyAsync`:

```csharp
var minimumLevelReached = MeetsMinimumLevel(curriculumItem, directive, data);
if (!minimumLevelReached && !string.IsNullOrWhiteSpace(directive.MinimumLevelField))
{
    continue;                              // <-- skips the whole row, no count at all
}
...
progress.CountsSoFar += directive.Amount;
if (minimumLevelReached) progress.MinimumLevelReachedCount += directive.Amount;
```

When a directive has `minimum_level_field` and the activity's level is below the curriculum item's
`MinimumLevelOrder`, **nothing is credited** — not even `CountsSoFar`. And because of the `continue`,
whenever a row *is* written `minimumLevelReached` is necessarily true, so
`MinimumLevelReachedCount` can never differ from `CountsSoFar`. **The two-counter design is
effectively dead.**

## Evidence (Act 3 play-through)

PAED-001 `MinimumLevelOrder = 4`. Mini-CEX credit rule: `minimum_level_field = overall_level`.
- Activity 1 completed with `overall_level = 3` → **zero progress rows** (gated out).
- Activity 2 completed with `overall_level = 4` → one row, `CountsSoFar = 1`,
  `MinimumLevelReachedCount = 1`.

## Decision needed

- **Option A (matches scenario + revives the second counter):** always increment `CountsSoFar` by
  `Amount` when the curriculum item matches; increment `MinimumLevelReachedCount` only when the
  level is met; drop the early `continue`. Trainees get volume credit for sub-threshold work, and
  the level-reached counter becomes meaningful.
- **Option B (keep current gate):** below-min completions genuinely don't count; then fix the
  scenario (Step 3.6) expectation and document that `MinimumLevelReachedCount` is redundant.

Recommend **A** on EPA/CBME grounds (a trainee's repeated below-entrustment attempts are still
evidence/volume; entrustment level is the separate progression signal) — but this is a product
call, so flagged rather than changed unilaterally.

## Verification (if A is chosen)

Re-run Act 3: activity 1 (level 3) writes a row with `CountsSoFar=1, MinimumLevelReachedCount=0`;
activity 2 (level 4) bumps to `CountsSoFar=2, MinimumLevelReachedCount=1`; dashboard shows "2 of
30". Update/extend `CreditApplier` unit tests for the split-counter behaviour.
