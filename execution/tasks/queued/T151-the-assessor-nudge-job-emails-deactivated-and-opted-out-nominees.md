---
id: T151
title: The assessor nudge job emails deactivated and opted-out nominees
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-24
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

- [ ] A deactivated or opted-out nominee gets no email — tests.

## Related

T102, T026 (data-subject rights, which added the opt-out flags).

**Decision, 2026-09-25:** D50 adopted as a default (EPA-PROGRAMME § 3D). Build to it.
