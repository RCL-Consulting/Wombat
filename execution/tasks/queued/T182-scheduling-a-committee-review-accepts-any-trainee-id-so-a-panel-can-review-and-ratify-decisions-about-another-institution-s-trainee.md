---
id: T182
title: Scheduling a committee review accepts any trainee id, so a panel can review, and ratify decisions about, another institution's trainee
status: queued
priority: P1
owner: agent
depends_on: []
created: 2026-09-24
---

# <id> — <one line that states the defect or the goal, not the solution>

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** High. A cross-institution review freezes another institution's evidence into a snapshot, and its
ratified decisions supersede that trainee's current entrustment decision. Nothing is live, but the design is wrong
(CLAUDE.md § Nothing is live: severity is not discounted).
**Surfaced:** 2026-09-24, the T113 security review (finding 1, confirmed in code, outside T113's scope).

## Symptom

`ScheduleCommitteeReview.cs:43-77`: `TraineeUserId` is free text. It is never checked against the panel's
institution or the caller's. The panel-institution check runs only for an InstitutionalAdmin, so a Coordinator, a
SpecialityAdmin or a SubSpecialityAdmin skips it. From there the review's snapshot (`StartCommitteeReview`) and its
ratification (`RatifyCommitteeDecision`) act on whoever was named.

## What to build

Before any mutation, and for every role but Administrator: the trainee's preferred profile
(`TraineeScopeResolver`, T113) must be at the panel's institution, and the caller must be able to act for that
institution. Return the same refusal for an unknown and an out-of-scope trainee (no existence leak, T056). The trainee
picker on the scheduling page must offer only trainees the handler would accept (picker = gate). Check
`StartCommitteeReview` and `RatifyCommitteeDecision` for the same gap and close it there too: a review scheduled before
the fix must not be the way in.

## Verification

- [ ] A Coordinator, a SpecialityAdmin and an InstitutionalAdmin of institution A cannot schedule a review of B's
      trainee. Handler tests; nothing is written (save, then clear the tracker, as `AuditPipelineBehavior` does).
- [ ] Start and Ratify refuse a review whose trainee is outside the panel's institution. Handler tests.
- [ ] The scheduling page's picker offers only in-scope trainees. bUnit test.

## Related

T113 (the same shape, for queries and MSF), T101, T056, T131 (committee governance), T165.
