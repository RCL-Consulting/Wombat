---
id: T245
title: A SpecialityAdmin's panel form offers specialities their institution has not adopted
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
---

# T245 — A SpecialityAdmin's panel form offers specialities their institution has not adopted

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low, and it needs confirmation first. It may only offer a panel that can never have a trainee.
**Surfaced:** 2026-09-25, the committee chain's browser check (T194 step 5).

## Symptom

A coordinator given SpecialityAdmin is offered General Medicine and Paediatrics on `/committee/panels/new`, because the
offer follows the user's speciality claims. Institution 1 has adopted no General Medicine curriculum, and the
InstitutionalAdmin's form offers only Paediatrics. It was not tested whether a General Medicine panel is accepted on
create.

## What to build

First establish whether create accepts it. Then offer a speciality admin only the specialities their claims name that
their institution has adopted, which is the InstitutionalAdmin's rule, and have create refuse the rest with the same
predicate.

## Verification

- [ ] The form and create agree: a speciality with no adoption is neither offered nor accepted. Handler and bUnit tests.

## Related

T194, T182.
