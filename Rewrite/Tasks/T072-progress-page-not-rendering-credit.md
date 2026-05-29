# T072 — Trainee progress page does not render an existing CurriculumItemProgress row

**Status:** open (found during Act 3 play-through, 2026-05-29)
**Severity:** High — credit is invisible to the trainee even when correctly persisted.
**Surface:** `/portfolio/progress` page + its query/projection (Trainee progress feature).

## Symptom

After a Mini-CEX completed with `overall_level = 4` (meets PAED-001 min 4), the
`CurriculumItemProgresses` table has a correct row (verified in DB):
`CurriculumItemId=2 (PAED-001), TraineeUserId=Dlamini, CountsSoFar=1, MinimumLevelReachedCount=1,
LastActivityId=2, CreditedActivityKeysJson=["2:complete"]`.

But signed in as that trainee, **`/portfolio/progress` shows no PAED-001 entry** — the string
"PAED-001" does not appear on the page (h1 "My progress" renders, no error). The credit is
persisted but not surfaced to the trainee.

## Likely cause (to confirm)

The progress page's query probably joins/filters differently from how `CreditApplier` writes the
row. Candidates to check:
- The page may key progress by `TraineeProfileId` while `CreditApplier` writes
  `CurriculumItemProgress.TraineeUserId` (a user id string) — a join mismatch would hide the row.
- The page may only list curriculum items for the trainee's *current stage* and PAED-001's
  stage/visibility filter excludes it.
- The page may require the progress row to also satisfy a min-level/visibility predicate.

Start at the `/portfolio/progress` Razor page → its MediatR query (something like
`GetTraineeProgress*` / `GetCurriculumProgress*`) and compare its filter against
`CurriculumItemProgress.TraineeUserId` (string user id) written by
`CreditApplier.ApplyAsync` (`src/Wombat.Infrastructure/Activities/CreditApplier.cs`).

## Verification

From snapshot `act3-minicex-credited` (already has the row), signing in as Dlamini and opening
`/portfolio/progress` shows PAED-001 with its count (per the T071 decision, "1 of 30" or the
agreed display). Add/extend a test for the progress projection against a written progress row.

## Related
- [[T071]] — credit minimum-level semantics (whether below-min completions count at all).
