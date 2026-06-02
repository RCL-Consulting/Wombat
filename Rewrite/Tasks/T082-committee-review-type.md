# T082 — Review-type field on committee reviews (F-4B-1 d)

**Status:** ✅ Shipped 2026-06-02 (Opus). Closes the last deferred Act 4 follow-up.

## Problem (F-4B-1 d)

The committee-review schedule form had no "review type" field. The Paediatrics scenario (Step 4.4)
distinguishes an **Annual progression review** (years 1–3) from a **Pre-graduation review** (the final
year-4 review, e.g. Dr Molefe), but the implementation scheduled all reviews identically. The
distinction is descriptive — no engine logic branches on it — so it had been deferred until a real need
surfaced. Closing it now for record fidelity and report clarity, alongside the F-3E-2 / F-3F-NOTE
data corrections.

This is orthogonal to the existing `IsFormative` flag: `IsFormative` controls whether a review issues a
binding decision (interim check-in vs summative); `ReviewType` records *why* the review is held.

## Fix

- **Domain:** new enum `CommitteeReviewType { AnnualProgression = 1, PreGraduation = 2 }`
  (`src/Wombat.Domain/CommitteeDecisions/CommitteeReviewType.cs`); `CommitteeReview.ReviewType`
  property (defaults to `AnnualProgression`).
- **Persistence:** stored as `integer` (mirrors `State` / `IsFormative` — no explicit conversion).
  Migration `20260602110316_CommitteeReviewType` (dotnet-ef scaffolded; Designer + snapshot updated)
  adds the column, **backfilling existing reviews to `AnnualProgression` (1)** — the enum has no `0`
  member, so the scaffolder's `defaultValue: 0` was changed to `1`.
- **Application:** `ScheduleCommitteeReviewCommand` gains a `ReviewType` parameter (defaults to
  `AnnualProgression`, so existing callers/tests are unaffected); handler persists it and returns it.
  Both DTOs (`CommitteeReviewListItemDto`, `CommitteeReviewDetailDto`) carry `ReviewType` (trailing
  optional param); `ToDetailDto` maps it; all three list projections (`ListReviewsForPanel`,
  `ListReviewsForChair`, `ListReviewsForTrainee`) select it.
- **Web:** `ReviewsSchedule` schedule form has a Review-type `<select>` and the list a **Type** column;
  `ReviewDetail` shows a **Type** row; `MyReviews` (trainee view) shows a **Type** column.

## Tests

`CommitteeReviewTypeTests` (+2 Application): scheduling with `PreGraduation` persists and returns it;
scheduling without a type defaults to `AnnualProgression`. Application 268→270. All other suites green
(Domain 49, Infrastructure 10, Architecture 19, Web 42); Integration not run (Docker).

## Note

No data backfill beyond the migration default is needed for the play-through DB. If a future Act 4/5
replay wants Molefe's final review tagged `PreGraduation`, set it at schedule time (the picker now
offers it); the existing five reviews remain `AnnualProgression` after the migration.
