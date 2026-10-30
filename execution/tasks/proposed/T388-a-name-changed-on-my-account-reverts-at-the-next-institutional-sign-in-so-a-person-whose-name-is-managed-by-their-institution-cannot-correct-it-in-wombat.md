---
id: T388
title: A name changed on My account reverts at the next institutional sign-in, so a person whose name is managed by their institution cannot correct it in Wombat
status: proposed
priority:
owner: agent
depends_on: []
created: 2026-10-30
---

# <id> — <one line that states the defect or the goal, not the solution>

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** _High / Medium / Low, and why — this is what `priority` above is derived from._
**Surfaced:** _When and how it was found. "While fixing T100" is worth more than a date alone._

## Symptom

A person who signs in through their institution and corrects their own first or last name on My account sees it revert the next time they sign in. Flow 14's correction step (A.1.8a) depends on a person correcting their own name; for an institutional sign-in it cannot hold.

## Root cause

Observed: `ExternalLoginHandler` rewrites first and last name from the provider's `name` claim whenever they differ (`src/Wombat.Infrastructure/Identity/ExternalLoginHandler.cs:540-547`, per flow 14's round-2 review S7/K9).

## What to build

Decide whose name wins for an institutional account: the provider's (then My account says so and refuses the edit), or the person's (then the handler stops overwriting a name the person set). Flow 14 (F6) only says so on the correction form.

## Verification

_This repo's completion signal. Each item names how it will be checked — a command, a test,
an observation. "Code exists" is not a completion signal._

- [ ] _criterion_ — checked by _command or observation_

## Related

T387 (flow 14, F6), T155, `design/flows/14-data-rights/round-2-review.md` S7.

## Notes

## Why

Surfaced 2026-10-30 by flow 14's round-2 code review; the operator took the review's recommendation to propose it as its own task ("as recommended").
