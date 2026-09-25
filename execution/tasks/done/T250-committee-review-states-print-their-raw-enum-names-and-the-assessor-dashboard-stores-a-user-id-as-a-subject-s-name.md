---
id: T250
title: Committee review states print their raw enum names, and the assessor dashboard stores a user id as a subject's name
status: done
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
completed: 2026-09-25
---

# T250 — Committee review states print their raw enum names, and the assessor dashboard stores a user id as a subject's name

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. "InProgress" and "UnderAppeal" read as code.
**Surfaced:** 2026-09-25, the T220 review.

## Symptom

- Committee review states print `State.ToString()` in:
  - `MyReviews.razor` (two places);
  - `ReviewDetail.razor`;
  - `ReviewsSchedule.razor`;
  - `CommitteeSectionComponent` (the PDF).
- `GetAssessorDashboardSummaryQuery` passes `SubjectUserId` into `SubjectName` on `AcceptedActivityItem` and
  `RecentDecisionItem`. No page prints it today, but the DTO misleads, and it breaks T142's rule.

## What to build

One label per committee review state ("In progress", "Under appeal", …), in one place, used by every surface above.
Resolve the subject's display name in the assessor query, or drop the field.

## Verification

- [x] Every surface prints the label. bUnit, and a PDF text test.
- [x] The assessor DTO carries a name or no name field. Handler test.

## Related

T220, T142, T189.

---

## As built — 2026-09-25 (`ea38d73`)

One label per committee review state (`CommitteeDecisionWording.StateLabel`, Withdrawn included), used by the schedule
list, the review page, My reviews and the portfolio PDF. Each assessor-dashboard row carries the trainee's name, and its
link is named by the instrument and trainee. bUnit, handler and PDF text tests.

Browser on dev (scripted Chrome, master `b4fd566`):
- **The State columns** read Scheduled, In progress, Ratified, Under appeal, Closed and Withdrawn, with no enum name
  anywhere.
- **The PDF** reads Ratified 11 times and Closed twice, with no "Final".
- **The assessor dashboard** shows no user ids.

**Filed from the review:** [T280] (the PDF leaves out a review under appeal; confirmed on dev: review 6 is absent).
