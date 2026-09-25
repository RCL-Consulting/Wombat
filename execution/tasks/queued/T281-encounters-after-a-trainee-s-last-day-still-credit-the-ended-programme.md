---
id: T281
title: Encounters after a trainee's last day still credit the ended programme
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
---

# T281 — Encounters after a trainee's last day still credit the ended programme

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. An ended programme's last period can count work done after it ended.
**Surfaced:** 2026-09-25, the lifecycle browser check (T252). With the dev trainee's last day set to 20 August,
PAED-001's Semester 2 showed "10 recorded", the latest observed on 25 September. The MSF line counted a campaign that
closed on 25 September.

## Decision (adopted 2026-09-25; the operator may overrule)

An encounter observed after the programme's last day (`CompletedOn` or `DeactivatedOn`) credits nothing on that
profile, just as an encounter before the programme start credits nothing (T160). This is the end-side mirror of D15
and D49.

## What to build

`EncounterDateGate` and `CreditApplier` refuse credit for an observed date after the profile's end, and the rebuild
agrees. The ended progress page and the PDF then count only encounters up to the last day. MSF coverage counts only
campaigns closed by then.

## Verification

- [ ] An encounter after the last day credits nothing, and the ended page does not count it. Tests.

## Related

T252, T209, D49, T160.
