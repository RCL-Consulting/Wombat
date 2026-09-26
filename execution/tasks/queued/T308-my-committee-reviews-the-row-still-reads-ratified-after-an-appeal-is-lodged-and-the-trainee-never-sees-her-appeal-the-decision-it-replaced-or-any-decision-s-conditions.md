---
id: T308
title: My Committee Reviews: the row still reads Ratified after an appeal is lodged, and the trainee never sees her appeal, the decision it replaced, or any decision's conditions
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-26
---

# T308 — My Committee Reviews: the row still reads Ratified after an appeal is lodged, and the trainee never sees her appeal, the decision it replaced, or any decision's conditions

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium. The page contradicts itself straight after the trainee's own action. It never shows the appellant her appeal, its outcome or the conditions set for her, although its subtitle promises "appeal status" and the data is already in the DTO it reads. Nothing wrong is stored.
**Surfaced:** 2026-09-26, the T295 replay of the rewritten scenario runbook (findings F-4.43a, F-4.48a).

## Symptom

1. **Step 4.43**, as Dr Mahlangu on /committee/my-reviews. After Lodge appeal the page says "Appeal lodged." and the detail reads "State: Under appeal", but the list row above still reads State Ratified until a reload (design/baseline/states/my-reviews--appealed.png, design/baseline/act-4/4.43-1-mahlangu-appeal-lodged.png). While the appeal is open, the page shows only the state: not her appeal's date or reason.
2. **Step 4.48**, after the chair remits in Step 4.47. The detail shows only the replacement, "Satisfactory with Observations", State Closed. It shows no appeal, no "Remitted", no first decision and no "Replaced on appeal" (design/baseline/states/my-reviews--remitted.png, design/baseline/act-4/4.48-1-mahlangu-remitted-outcome.png). The committee's page shows all of it (Step 4.47).
3. **Found in code, not by the replay:** the trainee's detail never prints a decision's Conditions. The committee's review page and the portfolio PDF do, so a trainee reads the remediation plan set for her only in her exported PDF.

## Root cause

- `MyReviews.razor:187-214`: `LodgeAppealAsync` replaces `_selectedReview` (`:206`) but not the row in `_reviews` (`:143`), which is loaded once in `OnInitializedAsync` (`:151-169`). The row's State and Decision cells (`:40`, `:42`) read the stale item.
- `MyReviews.razor:65-86` renders only `Decisions[0]` (newest first, `CommitteeDecisionMappings.cs:24-25`) with Rationale and Attendees. It has no Conditions line, no loop over earlier decisions and no "Replaced on appeal" marker, and never reads `_selectedReview.Appeals`. `GetCommitteeReviewByIdQuery` includes Appeals for every reader who passes the ladder (`GetCommitteeReviewById.cs:34`; mapped at `CommitteeDecisionMappings.cs:44-45`).
- The committee page's version is `ReviewDetail.razor:306-345` (every decision, replaced marker, Conditions, Present) plus the Appeals list at `:802-813`. The PDF's is `CommitteeSectionComponent.cs:118-124`.
- No decision says what the appellant sees after resolution. DESIGN.md § Entrustment-only reviews, "The trainee's reviews", covers only the entrustment-only remit sentence (`EntrustmentOnlyNote`, `MyReviews.razor:226-241`).

## What to build

1. After a lodge, update the matching row in `_reviews` from the command's answer, or re-read the list, so that row and detail agree. Do the same for any later action the page adds.
2. The detail shows every decision as the committee's page does: newest first, a replaced one marked "Replaced on appeal by the decision above.", and each with its rationale, its Conditions when set, and who was present. Share one component or `CommitteeDecisionWording` helper with `ReviewDetail`, so the two cannot drift.
3. Add an Appeals block listing her appeals: date lodged, reason, outcome in words (Open, or the resolved outcome from the appeal-form task's `AppealOutcomeLabel`), and date resolved.
4. Record the default in EPA-PROGRAMME § 3 as adopted on recommendation (the operator may overrule): the appellant sees her appeal, its outcome and the decision it replaced, as the committee does. Record it in DESIGN.md's "The trainee's reviews" bullet too.
5. Update runbook Steps 4.43 and 4.48's Expect once fixed.

## Verification

- [ ] bUnit (new MyReviewsAppealTests): after LodgeAppealCommand answers with UnderAppeal, the row's State cell reads "Under appeal" without a reload.
- [ ] bUnit: a Final review with a remitted appeal renders both decisions, "Replaced on appeal by the decision above." on the first, and the appeal with its outcome.
- [ ] bUnit (beside MyReviewsAttendanceTests): a decision with Conditions shows "Conditions: …" in the trainee's detail.
- [ ] Browser, replaying Steps 4.43 and 4.48 as Dr Mahlangu: 4.43's row reads Under appeal at once; 4.48 shows her appeal as Remitted and the replaced Inadequate Progress decision. New baseline screenshots replace my-reviews--appealed.png and my-reviews--remitted.png.
- [ ] The default is recorded in EPA-PROGRAMME § 3 and DESIGN.md.

## Related

T280 (the portfolio PDF leaves out a review under appeal: the same appeal on another surface), the appeal-form task drafted alongside (outcome wording and remit conditions), T165, T250 (state labels), T131 slice 5 (the entrustment-only remit note). Runbook Steps 4.43, 4.47 and 4.48. Findings F-4.43a and F-4.48a.
