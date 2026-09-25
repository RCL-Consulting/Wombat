---
id: T258
title: Erasure rewrites only a trainee's first profile, leaves it active, and leaves their open reviews and MSF campaigns running under the pseudonym
status: queued
priority: P2
owner: agent
depends_on: []
created: 2026-09-25
---

# T258 — Erasure rewrites only a trainee's first profile, leaves it active, and leaves their open reviews and MSF campaigns running under the pseudonym

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium. An erased person's records stay live: a panel can still ratify their review, and a release writes
activities for the pseudonym.
**Surfaced:** 2026-09-25, the T238 review (out of scope). Read from the code, not run.

## Symptom

`ErasureExecutor`:
- rewrites only the **first** trainee profile and the **first** assessor profile, so a trainee's other profiles stay
  under their original id;
- leaves the rewritten profile **active**;
- moves the trainee's open committee reviews to the pseudonym, where the panel can still start, record and ratify them
  (`DemandTraineeAtPanelInstitutionAsync` finds the pseudonym's profile);
- moves their MSF campaigns to the pseudonym, where the coordinator can still invite, open, close and release them
  (`IsSubjectInScopeAsync`). A release then writes activities for the pseudonym.

T238 keeps a pseudonym out of new scope. It does not end what was already open.

## What to build

In erasure itself:
- Rewrite every profile of the user, and deactivate them (T209's `DeactivatedOn` = the erasure day).
- End what is open: withdraw open or draft MSF campaigns, anonymising their invitations as a withdraw does. Close or
  withdraw open committee reviews with a recorded reason. Refuse a pending STAR stage.
- Keep what is settled: ratified STARs and released reports, under the pseudonym.

Record the rule in CUSTOMIZATION.md or DOMAIN.md § data rights.

## Verification

- [ ] After erasure, no profile under the original id remains, every pseudonymised profile is inactive, and no open
      review or campaign remains. Postgres test with the real `ErasureExecutor`.

## Related

T238, T026 (data rights), T207, T209.
