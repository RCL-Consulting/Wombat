---
id: T309
title: Scheduling a review: the agenda preview does not warn that the trainee already holds the period's binding seat, and a formative check-in is typed and shown as an annual progression review
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-26
---

# T309 — Scheduling a review: the agenda preview does not warn that the trainee already holds the period's binding seat, and a formative check-in is typed and shown as an annual progression review

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. Both mislead, but nothing wrong is stored or wrongly allowed. Create still refuses the second binding review (Step 4.8), and a formative review decides nothing whatever its type.
**Surfaced:** 2026-09-26, the T295 replay of the rewritten scenario runbook (findings F-4.8a, F-4.49a).

## Symptom

1. **The preview (Step 4.8),** as Mr Smit on /committee/reviews, scheduling a second Annual progression review of Dr Molefe before the same panel for 2026 S2. The agenda preview reads "15 EPAs will be on the agenda for 2026 S2." with no warning. Only Create review is refused: "Review #1 already puts this trainee before Paed Annual Review Panel for 2026 S2 …" (design/baseline/states/reviews-schedule--refused.png, design/baseline/act-4/4.8-1-smit-second-review-refused.png).
2. **The formative type (Step 4.49).** With Formative only ticked, the Review type select still offers Annual progression and Pre-graduation, and the check-in is stored with the choice. Review #6 reads Type "Annual progression review", Mode Formative (design/baseline/states/review-detail--formative.png).
3. **Step 4.51.** Dr Ndlovu's list has no Mode column and reads "Annual progression · Closed · No binding decision" for the check-in, beside his real annual review (design/baseline/states/my-reviews--two.png).
4. **By code, not observed:** the portfolio PDF lists a closed check-in as "Type: Annual progression review". In semester 1, or before the neonatal CCC, a formative review can be typed "Entrustment-only review" while it "cannot issue entrustment decisions".

## Root cause

**The preview:** `PreviewCommitteeAgendaQueryHandler.Handle` (`PreviewCommitteeAgenda.cs`) runs only the scope check and `AgendaPlanner`. The seat check is private to `ScheduleCommitteeReviewCommandHandler.OpenBindingReviewAsync` (`ScheduleCommitteeReview.cs:205-222`, called at `:126`; refusal text `AlreadyScheduled`, `:233-237`). `CommitteeAgendaPreviewDto` (`CommitteeAgendaDtos.cs:193-208`) carries `PanelDecidesAnything` so the preview says the entrustment-only refusal first (DESIGN.md § Entrustment-only reviews, Scheduling), but it carries nothing about the seat. Decisions-due already reads the seat through `CommitteeReviewSeats` (`GetEntrustmentDecisionsDue.cs:301-308`).

**The formative type:** `CommitteeReview.ReviewType` defaults to `AnnualProgression` (`CommitteeReview.cs:61`), and `CommitteeReviewType.cs` documents it as independent of `IsFormative`. `ScheduleCommitteeReview.cs:115` stores `request.ReviewType ?? DefaultFor(...)` for a formative review too. `ReviewsSchedule.razor:74-81` keeps the select live while `#review-formative` (`:83-90`) is ticked. Every label reads the type alone (`CommitteeDecisionWording.ReviewTypeLabel` and `ReviewTypeShortLabel`, `:120-135`), used by `MyReviews.razor:39` and `:62`, `ReviewDetail.razor:123`, `ReviewsSchedule.razor:120`, `ReviewRowNames.cs:29` and `CommitteeSectionComponent.cs:94`. The PDF includes Final reviews (`PortfolioPdfService.cs:293`).

## What to build

1. **The preview warns first.** Move the open-seat lookup into `CommitteeReviewSeats` as one query that both the scheduling handler and `PreviewCommitteeAgendaQuery` call. Return the holding review (id, panel name, state, scheduled on) on `CommitteeAgendaPreviewDto`. The preview then says, first and in the create's own words (`AlreadyScheduled`), which review holds the seat, with an Open review link. Create review is shown disabled with that reason (the T107 pattern, as Record decision is on an empty agenda). If the preview failed, the button stays live and the handler refuses as now. A formative request takes no seat, so it gets no warning.
2. **A formative review reads as one.** Give it one label, read from both fields: `CommitteeDecisionWording.ReviewTypeLabel(review)` returns "Formative check-in" (short: "Formative") when `IsFormative`, else the type's label. Use it on every surface listed in the root cause, the PDF and `ReviewRowNames` included.
3. **The form stops asking.** Hide the Review type select while Formative only is ticked. The handler ignores `ReviewType` for a formative review and stores the panel and period default.
4. **Record the rule** in DESIGN.md's "The type" bullet. Weigh the larger alternative, a `Formative` member of `CommitteeReviewType` replacing `IsFormative` (84 references), and record which was chosen. If storage changes, update the runbook's outcome SQL, which expects ReviewType 1 on the check-in.

## Verification

- [ ] Application test (CommitteeAgendaHandlerTests): the preview for a trainee with an open binding review in the seat returns that review. It returns none for a formative request, for a panel in another seat (a neonatal CCC panel), or once the review is ratified.
- [ ] bUnit (ReviewsScheduleAgendaPreviewTests): the preview shows the holding-review sentence and Open review link, and Create review is disabled with the reason.
- [ ] bUnit (ReviewsScheduleTraineeFirstTests or a new test): ticking Formative only hides #review-type.
- [ ] Application or bUnit test: a formative review reads "Formative check-in" or "Formative" on ReviewDetail, both MyReviews cells and the schedule list.
- [ ] PDF text test (PortfolioPdfServiceTests): a closed check-in prints "Type: Formative check-in".
- [ ] Browser: Step 4.8's preview warns before Create is pressed; Steps 4.49 and 4.51's check-in reads Formative on review #6, on Mr Smit's list and on Dr Ndlovu's list.

## Related

T131 slice 4 (one open binding review per seat and period), T131 slice 5 (review types; the preview already says the entrustment-only refusal first), T107 (disabled with the reason), T259 (decisions-due seat and panel naming, neighbouring code). Runbook Steps 4.8, 4.49, 4.50 and 4.51. Findings F-4.8a and F-4.49a.
