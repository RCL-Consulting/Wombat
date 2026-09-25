---
id: T117
title: "The weekly coordinator digest emails every coordinator the same national roster"
status: done
priority: P2
created: 2026-09-19
completed: 2026-09-25
---
# T117 — The weekly coordinator digest emails every coordinator the same national roster

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

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

---

## As built — 2026-09-25 (`62ff084`)

`WeeklyCoordinatorDigestJob` builds each digest for its recipient.
- **Scope.** A digest holds the coordinator's institution's inactive trainees (T113's read rule, trainee first), the
  under-review MSF campaigns about them, and the Scheduled reviews on their institution's panels in the next 7 days.
- **Who gets none.** A coordinator who holds Trainee, has no institution, or has no email address gets no digest.
- **The log.** One line per run counts the digests and each skip reason.

Job tests (two institutions) and a Postgres test run the real claims factory.

On dev (master `aa66ee3`; jobs triggered by moving `ScheduledJobRuns."StartedAt"` back, and restored after; mail read from the SMTP sink):
- **Run 74:** "digests sent 2".
- **coordinator@'s digest:** Demo Trainee (at risk), campaigns #4 and #5, and review #16 on 2026-09-28. This matches the
  database exactly.
- **coordinator.t113b's digest** (institution 2): "No items requiring attention this week", with none of institution 1's
  names or numbers.
- **Not exercised on dev:** the Trainee, no-institution and no-email skips (no such coordinator). Covered by tests.
