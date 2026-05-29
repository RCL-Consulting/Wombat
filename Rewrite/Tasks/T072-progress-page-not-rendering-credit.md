# T072 — Trainee progress page does not render an existing CurriculumItemProgress row

**Status:** ✅ SHIPPED 2026-05-29 (Opus). See "Resolution" below.
**Severity:** High — credit is invisible to the trainee even when correctly persisted.
**Surface:** `/portfolio/progress` page + its query/projection (Trainee progress feature).

## Resolution (2026-05-29)

The original premise was partly wrong: the credit **was** visible — on the trainee **dashboard**
(`/`), whose "Curriculum progress" card correctly renders `1 / 30 · reached 0 / 30` by EPA title
(live-verified). The real defect was a **surface mismatch**: that dashboard card links to
`/portfolio/progress`, but that page was a *rating-trajectory* chart that never showed curriculum
credit — and it was empty anyway because the trajectory query (a) allow-listed only literal keys
`mini_cex`/`dops`/`cbd`/`acat` (the built type is `mini_cex_paed`), and (b) parsed the rating from a
field named `overall` while the schema-driven Mini-CEX stores `overall_level`.

Fixes (chosen: lead with curriculum credit + fix trajectory; pragmatic field fallbacks):
1. **New query** `GetCurriculumProgressForTraineeQuery` (`Features/Curricula/GetCurriculumProgressForTrainee.cs`)
   — lists **every** curriculum item in the trainee's active-profile curriculum (including 0-credit
   items as "0 of N"), left-joined to `CurriculumItemProgress`, with effective stage-minimum level,
   level-reached count, and last-credited date. Reuses `ComputeTraineeStage`.
2. **`/portfolio/progress` rebuilt** (`MyProgress.razor`) — leads with a **Curriculum credit**
   section (EPA code + title, count/required, progress bar, min-level + reached, last-credited
   date), then keeps the **Rating trajectory** section (only rendered when points exist).
3. **Trajectory parser fixed** (`GetEpaTrajectoryForTraineeQuery.cs`) — matches an assessment family
   by exact key **or** `"<base>_…"` prefix (so `mini_cex_paed` resolves), and reads the overall
   rating from `overall` **or** `overall_level` (schema-driven debt; a fully schema-aware field
   resolution belongs with T069).

**Live-verified** as Dlamini on snapshot `act3-credit-semantics-T071`: `/portfolio/progress` shows
`PAED-001 — 1 / 30 — Minimum level 4 (year 3) · reached 0 / 30 · last credited …`, all 15 PAED items
listed, and the trajectory charts the level-3 observation. Tests: +5 (Application 249/249); Domain 45,
Architecture 19, Web 39 unchanged.

---

## Original report

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
