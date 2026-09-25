---
id: T238
title: An erased trainee's pseudonymised profile stays in committee scope, and the schedule command accepts it
status: done
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
completed: 2026-09-25
---

# T238 — An erased trainee's pseudonymised profile stays in committee scope, and the schedule command accepts it

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. It needs a crafted request, and the pseudonym names no one.
**Surfaced:** 2026-09-25, the T194 and T216 reviews.

## Symptom

`ErasureExecutor` replaces an erased trainee's profile user id with a pseudonym that matches no account. The profile
lookup includes inactive profiles, so the pseudonym stays in scope. The schedule command accepts it through a crafted
request (the picker and, since T194, the panel list leave it out). A profile that outlives its user's Trainee role has
the same shape.

## What to build

One rule for "a trainee this caller may act on": an active profile whose user exists and still holds Trainee. Used by
`CommitteeTraineeScope` (scheduling and its commands) and by the trainee lists, which drop stale and erased profiles.
Check T113's other trainee lists against the same rule.

## Verification

- [x] Scheduling an erased trainee's pseudonym is refused before any write. Handler test.
- [x] The trainee lists omit erased and stale profiles. Tests.

## Related

T194, T216, T113, T026 (erasure).

---

## As built — 2026-09-25 (`248c402`)

One rule for a trainee a caller may act on: `TraineeScopeResolver.KeepCurrentAsync`, meaning an active profile whose
user exists and still holds Trainee.
- **Refusals.** Committee scheduling and its commands refuse anyone else before any write, and so does MSF create. The
  MSF picker (`ListMsfCampaignSubjectsQuery`) offers exactly whom create accepts.
- **Dashboards.** Every staff dashboard counts the same trainees.

A Postgres test erases a trainee with the real `ErasureExecutor`, then tries both ids.

Browser on dev (scripted Chrome, master `810236c`):
- **Pseudonym ignored.** With a pseudonymised profile added by SQL, the scheduling picker, decisions-due, `/msf/coverage`,
  the MSF picker and the committee dashboard never showed it.
- **Trainee role removed.** Scheduling and MSF create were refused with the exact texts, and every list emptied. With the
  role restored, all came back.
- **Clean-up.** The pseudonym profile was deleted.

**Behaviour change for the operator:** an Administrator can no longer create an MSF campaign for someone who is not a
current trainee, and a graduate or withdrawn trainee gets no new campaign. A locked (not erased) account still counts
as current.

**Filed from the review:** [T258] (P2: erasure leaves open records running).
