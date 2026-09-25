---
id: T151
title: The assessor nudge job emails deactivated and opted-out nominees
status: done
priority: P3
owner: agent
depends_on: []
created: 2026-09-24
completed: 2026-09-25
---

# T151 — The assessor nudge job emails deactivated and opted-out nominees

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. Mail to people who should not receive it; the email names the trainee.
**Surfaced:** 2026-09-24, T102 mapping (consumers lens).

## Symptom

`AssessorPendingNudgeJob` emails every resolved nominee with an email address: including a deactivated account
(`UserDeactivation.IsDeactivated`), a user who opted out of digest emails (`OptOutOfDigestEmails`), and a nominee who
has since lost the Assessor role or moved institution. T102 made it read the pinned workflow, but not filter who it
writes to. It also silently skips unknown ids and cannot say how many it skipped.

## What to build

Skip deactivated and opted-out recipients; log counts of skipped recipients by reason. Decide whether a nominee who is
no longer eligible should be nudged, or the trainee told instead.

## Verification

- [x] A deactivated or opted-out nominee gets no email — tests.

## Related

T102, T026 (data-subject rights, which added the opt-out flags).

**Decision, 2026-09-25:** D50 adopted as a default (EPA-PROGRAMME § 3D). Build to it.

---

## As built — 2026-09-25 (`2eaf949`, D50)

`AssessorPendingNudgeJob` skips a deactivated account and a user who opted out of digest emails. A nominee who lost the
role or moved institution is still nudged. One run line counts the nudges and each skip reason ("assessors nudged N
(activities N); nominees skipped: no such account N, deactivated N, opted out of digest emails N, no email address N").
Job tests use a fake sender.

Browser on dev (scripted Chrome, master `c6efc04`; `pg_dump` first, at `recovery/pre-g3-migration.dump`): Activity 39 was a Mini-CEX awaiting the assessor, aged by SQL. Each run was triggered by moving
`ScheduledJobRuns."StartedAt"` back, and restored after.
- Baseline: "assessors nudged 1 (activities 1)", and the mail reached the assessor.
- **Opted out:** "nudged 0 … opted out of digest emails 1", and no mail.
- **Locked by instadmin:** "nudged 0 … deactivated 1", and no mail. The lock was undone.
- **Assessor role removed (D50):** still nudged, and the mail was sent. The role was restored.

**Filed from the review:** [T240]. The weekly digest and the draft reminder still mail deactivated and opted-out
accounts.
