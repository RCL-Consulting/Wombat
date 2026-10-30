---
id: T389
title: A locked trainee is still emailed by the entrustment decision expiry job
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

An administrator locks a trainee's account; the entrustment decision expiry job still emails them that a decision on their record has expired or is about to.

## Root cause

Observed: `ReminderRecipientPolicy.cs` documents it as "A known gap, not a decision": the expiry job checks only for a missing address. T240 covered only the three digest jobs. The opt-out is rightly not honoured there (the mail is about one decision).

## What to build

Skip a deactivated account in `EntrustmentDecisionExpiryJob`, without honouring the digest opt-out; a test that a locked trainee gets nothing.

## Verification

_This repo's completion signal. Each item names how it will be checked — a command, a test,
an observation. "Code exists" is not a completion signal._

- [ ] _criterion_ — checked by _command or observation_

## Related

T387 (flow 14, F7), T240, `design/flows/14-data-rights/round-2-review.md` S9.

## Notes

## Why

Surfaced 2026-10-30 by flow 14's round-2 code review; the operator took the review's recommendation to propose it as its own task ("as recommended").
