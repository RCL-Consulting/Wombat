---
id: T117
title: "The weekly coordinator digest emails every coordinator the same national roster"
status: queued
priority: P2
created: 2026-09-19
---
# T117 — The weekly coordinator digest emails every coordinator the same national roster

**Status:** open
**Surfaced:** 2026-09-19, adversarial review of T101.
**Severity:** Medium (confidentiality) — pushed, not pulled, so no read gate applies and the recipient
does not have to go looking.

## Symptom

`src/Wombat.Infrastructure/Scheduling/Jobs/WeeklyCoordinatorDigestJob.cs`:

- `:36` — `GetUsersInRoleAsync(Coordinator)` returns **every** coordinator in the product.
- `:47-61` — `inactiveTrainees` is computed **once**, over an unscoped `Set<Activity>()` read, for all
  trainees.
- `:67-88` — `reviewDescriptions` (trainee name + review date) likewise, once, globally.
- `:90-102` — the **identical body** is mailed to every coordinator.

So a coordinator at institution A is emailed, by name, which of institution B's trainees have logged
nothing in 30 days and which of B's trainees have a committee review this week.

## Why T101 did not close it

T101 gated **reads**. This is a push: nothing evaluates a principal, because there is no request. The
job builds one message and fans it out.

## Fix

Resolve each recipient's institution and compute both lists per recipient — join
`TraineeProfile.InstitutionId` for the inactive list and `DecisionPanel.InstitutionId` for the reviews.
A coordinator with no institution should get no roster rather than the global one.

While there: the job reads `Set<Activity>()` directly in Infrastructure, which is one of the surfaces
outside the reach of `ActivityReadScope.WhereReadableBy` (an Application extension). Worth a comment
recording that the boundary is per-recipient here, not per-principal.

## Related

Same confidentiality class as [T101]; independent mechanism. See also [T112] (data rights) and [T113].
