---
id: T240
title: The weekly coordinator digest and the draft reminder mail deactivated accounts and ignore the digest opt-out
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
---

# T240 — The weekly coordinator digest and the draft reminder mail deactivated accounts and ignore the digest opt-out

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. It sends mail to people who should not get it. An admin-locked or erased account still gets reminders.
**Surfaced:** 2026-09-25, the T151 review (finding 1). T151 fixed only the assessor nudge (D50).

## Symptom

- `WeeklyCoordinatorDigestJob` fetches every Coordinator and skips only those with no email. An opted-out or admin-locked
  coordinator still gets the Monday digest, the very email `OptOutOfDigestEmails` is named after.
- `ActivityDraftNudgeJob` checks only for a missing email, so it writes to admin-locked and erased trainees.
- After T151, the assessor nudge is the only job that reads the opt-out.

## What to build

Move T151's `AssessorPendingNudgeJob.SkipReasonFor` into one shared recipient policy that all three jobs call. It skips
a deactivated account (`UserDeactivation.IsDeactivated`) always, and a user who opted out of digest emails for every
periodic reminder. **Recommendation:** the draft reminder counts as a digest under the opt-out. It is a periodic,
unsolicited summary, like the others. Each job logs its skips as counts by reason, as T151's does.

## Verification

- [ ] Each job skips deactivated and opted-out recipients. Job tests with a fake sender.

## Related

T151, D50, T117, T026.
