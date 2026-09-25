---
id: T218
title: A Coordinator's committee review list is empty, and a ratified review still shows empty decision inputs
status: done
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
completed: 2026-09-25
---

# T218 — A Coordinator's committee review list is empty, and a ratified review still shows empty decision inputs

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. The Coordinator can schedule a review and then cannot find it.
**Surfaced:** 2026-09-25, the T131 slices 5–6 browser check. Both predate T131.

## Symptom

- As the Coordinator, `/committee/reviews` reads "No reviews yet", even for a review they scheduled.
  `ListReviewsForPanelQuery` lists only reviews on panels the caller sits on, unless the caller is an Administrator or
  InstitutionalAdmin.
- A ratified review's Decision card still shows empty Rationale and Conditions inputs, with no Record button.

## What to build

List, for a scheduling role, the reviews they may open at their institution (T182's scope, trainee first per T216), and
render a decided review's decision read-only.

## Verification

- [x] The Coordinator's list shows review 5. bUnit and browser.
- [x] A ratified review shows no decision inputs. bUnit.

## Related

T131, T182, T212.

## Progress, 2026-09-25

- **Item 2 is done by T213** (`2c6c3a8`): the decision form is offered only to the chair while the review is in
  progress. Browser: ratified reviews 1–5 and 7 show no decision inputs to the chair.
- **Item 1 is still open.** As coordinator, `/committee/reviews` reads "No reviews yet – Schedule the first committee
  review…" with eight reviews at institution 1. `ListReviewsForPanel` lists only panels the caller sits on unless they
  are an Administrator or InstitutionalAdmin. It must also stop telling a coordinator to schedule the first review
  when reviews exist.

---

## As built — 2026-09-25 (`c92c55d`)

A scheduling role's `/committee/reviews` lists the reviews they may open at their institution, by one read rule
(`CommitteeReviewReadAccess`) shared with the review page, trainee first. The empty card never says "schedule the first
review" when reviews exist that the caller cannot see. Item 2 (a ratified review's decision inputs) was done by T213.

Browser on dev (scripted Chrome, master `b4fd566`):
- **coordinator:** 19 rows, equal to the database count, each opening. A newly scheduled review 20 appeared at once.
- **An assessor given SpecialityAdmin:** the same 20 rows.
- **committee:** its panels' reviews, and no Schedule button.
- **A trainee given Coordinator:** "No reviews to show" with the Trainee-role note. My committee reviews listed their 14,
  and review 15 (a peer's scope) was refused.
