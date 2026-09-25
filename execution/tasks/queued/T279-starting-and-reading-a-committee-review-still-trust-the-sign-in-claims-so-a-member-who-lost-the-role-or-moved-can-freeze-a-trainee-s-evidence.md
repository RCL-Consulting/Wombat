---
id: T279
title: Starting and reading a committee review still trust the sign-in claims, so a member who lost the role or moved can freeze a trainee's evidence
status: queued
priority: P2
owner: agent
depends_on: []
created: 2026-09-25
---

# T279 — Starting and reading a committee review still trust the sign-in claims, so a member who lost the role or moved can freeze a trainee's evidence

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium. Starting a review freezes the trainee's evidence snapshot.
**Surfaced:** 2026-09-25, the T256 review (out of scope 2).

## Symptom

`WorksOnPanel` (`CommitteeDecisionAuthorization`), used by `MayReadReview` and `DemandStartableReview`, reads only the
sign-in claims. A member who moved institution, or lost CommitteeMember but whose cookie has not been revalidated (30
minutes by default), can still read that panel's reviews and frozen evidence, and can start a review.

## What to build

Apply `PanelSeat.SittingAt` (T237's one rule) to Start and to the panel arm of `MayReadReview`, as T256 did for the
chair's actions. Consider shortening the security-stamp revalidation interval, or revalidating on committee pages.

## Verification

- [ ] A member who lost the role or moved is refused Start and cannot read the panel's reviews. Handler tests.

## Related

T256, T237, T194.
