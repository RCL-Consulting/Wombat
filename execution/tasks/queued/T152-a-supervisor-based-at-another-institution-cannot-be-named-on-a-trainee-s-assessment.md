---
id: T152
title: A supervisor based at another institution cannot be named on a trainee's assessment
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-24
---

# T152 — A supervisor based at another institution cannot be named on a trainee's assessment

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low today (no corpus step needs it); a real gap once rotations are recorded.
**Surfaced:** 2026-09-24, T102 design critique (upheld 3/3).

## Symptom

Since T102 a nominee must belong to the activity's institution. A visiting or outreach consultant based elsewhere
cannot be named. The workaround T006/T027 imply — a second account at the trainee's institution — needs a second email
address, because `UserName` is the email and is unique, invitation acceptance refuses an existing email, and SSO
provisioning fails on a duplicate `UserName`.

## What to build

A decision first: model cross-institution supervision (an assessor affiliation list, or an institution-granted
"may assess here" record) and let the nominee directory read it. Picker and gate must keep sharing one predicate
(`NomineeDirectory`).

## Verification

- [ ] An affiliated assessor from another institution is offered and accepted; an unaffiliated one is neither — tests.

## Related

T102, T006, T027.
