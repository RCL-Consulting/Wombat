---
id: T374
title: An existing account cannot be invited or admitted as a trainee
status: proposed
priority:
owner: agent
depends_on: []
created: 2026-10-19
---

# T374 — An existing account cannot be invited or admitted as a trainee

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium (needs the operator). A registrar who already holds a Wombat account (a former trainee after D54,
or staff starting a programme) has no way onto a new programme.
**Surfaced:** 2026-10-19, writing flow 11's round 2 ask (T373): Q12 refuses an invitation to an address that has an
account and says to add the role instead, but no path adds Trainee.

## Symptom

An invitation to an address that already has an account is refused (flow 11's Q12, T286). For staff roles the refusal
says to add the role under Users. For Trainee there is no such path: Add role never offers Trainee (T303), and Trainees
admits only a Pending trainee. So a graduate or a withdrawn registrar (D54: no role left) cannot start a new programme,
and nor can a consultant who becomes a registrar.

## Root cause

By design so far: Trainee is system-managed (T303) and reached only through an invitation and an admission.

## What to build

A decision first: how an existing account enters a programme (an admission offered for an account with no active
profile; or a Trainee invitation that, used by a signed-in account holder, attaches to that account). Then the build.

## Verification

- [ ] The operator's decision recorded — a D-number in EPA-PROGRAMME § 3.
- [ ] A runbook step plays a former trainee admitted to a second programme — its Actual.

## Related

T373 (flow 11), T286, T303, D54, D55, T305.

## Notes

- Inferred from the code paths named above; not replayed. Flow 11's round 2 ask marks the Trainee case PROPOSED.
