---
id: T279
title: Starting and reading a committee review still trust the sign-in claims, so a member who lost the role or moved can freeze a trainee's evidence
status: done
priority: P2
owner: agent
depends_on: []
created: 2026-09-25
completed: 2026-09-25
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

- [x] A member who lost the role or moved is refused Start and cannot read the panel's reviews. Handler tests.

## Related

T256, T237, T194.

Note, 2026-09-25 (the lifecycle browser check): an erased user's open session outlived the erasure by about 30 minutes.
No `SecurityStampValidatorOptions.ValidationInterval` is set, so Identity's default applies, and the old cookie still
opened `/`, `/account/data-rights` and `/portfolio/progress` with the full Trainee nav. Set a short app-wide interval
(1–5 minutes), or revalidate on each circuit start. That also closes most of this task's stale-claims window.

---

## As built — 2026-09-25 (`6d53688`)

- **Seats.** Start and the panel arm of review reads use `PanelSeat.SittingAt`, so a member who lost the role, moved or
  was deactivated neither starts nor reads the panel's reviews.
- **Revalidation.** Sessions revalidate within minutes: the security stamp is checked on a short interval, and a circuit
  whose session has ended leaves by a full load of `/account/session-ended` and signs in again.
- **SSO.** An SSO role change ends the user's other sessions.

Web, integration and handler tests.

Browser on dev (scripted Chrome, master `e22d58b`; `pg_dump -n public` first, at `recovery/pre-t283-t281-migrations.dump`):
- **Ended sessions.** Each ended session (a role lost, a lock, another browser) reached the sign-in page with
  "SessionEnded" in about 60 s. Signing in answered 302, not 400.
- **The seat rule.** committee2 given Trainee saw an empty list, and reviews 5 and 16 "could not be found", with no Start.
  Restored, Start on review 16 worked.
- **An open tab** reloaded by itself back to its review, still signed in.

**Found:** log noise from a gone circuit, noted on [T290].
