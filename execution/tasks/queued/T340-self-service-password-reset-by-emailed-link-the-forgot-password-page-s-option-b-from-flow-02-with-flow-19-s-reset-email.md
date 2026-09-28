---
id: T340
title: Self-service password reset by emailed link: the forgot-password page's option B from flow 02, with flow 19's reset email
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-28
---

# T340 — A person who forgets their password cannot reset it themselves

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. An administrator's reset (Step A.4.5) covers it; this removes a wait on an administrator.
**Surfaced:** 2026-09-28, flow 02 round 1 (T339): the operator chose option A for now, plain words and an
administrator's reset, and kept option B as future work.

## Symptom

`/account/forgot-password` says what to do (after T339), but offers no reset. Nothing sends the `PasswordResetEmail`
template (`coverage.md` § Flows and states not played).

## Root cause

Not built: no reset request endpoint, no single-use token, no reset form.

## What to build

Flow 02's round 1 option B (`design/flows/02-sign-in-and-account/round-1/Forgot-B.dc.html`): four screens.
- Ask: an email field, sent as a form post; throttled per network, as sign-in is (T156).
- Check your email: the same words whether or not the address has an account (T287).
- Choose a new password, from the emailed link: a single-use, expiring token; the rules as T339 words them.
- Link expired or used; then Sign in with a notice.
The email is flow 19's to design. The token stays out of the address bar after the page loads (T315's lesson).

## Verification

- [ ] A reset by emailed link works end to end on the scenario data, and a runbook step plays it — the step's Actual.
- [ ] An unknown address and a known one get the same page and the same timing — a test.
- [ ] A used or expired token is refused — a test.

## Related

T339 (flow 02, Q2), flow 19 (emails), T287, T315, T156, BRIEF § 7 B10.

## Notes

- Observed: `design/flows/02-sign-in-and-account/round-1-review.md` records the decision.
