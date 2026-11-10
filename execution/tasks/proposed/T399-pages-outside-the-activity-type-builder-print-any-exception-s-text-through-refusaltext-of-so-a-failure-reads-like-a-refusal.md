---
id: T399
title: Pages outside the activity-type builder print any exception's text through RefusalText.Of, so a failure reads like a refusal
status: proposed
priority:
owner: agent
depends_on: []
created: 2026-11-10
---

# T399 — Pages outside the activity-type builder print any exception's text, so a failure reads like a refusal

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** _High / Medium / Low, and why — this is what `priority` above is derived from._
**Surfaced:** _When and how it was found. "While fixing T100" is worth more than a date alone._

## Symptom

`RefusalText.Of` (30 calls in 19 files: flows 03, 04, 07–12 beside the builder) prints whatever a handler throws, so an
unexpected failure shows its exception text as if it were a refusal the user can act on.

## Why

Flow 17's build plan (T398, `design/flows/17-activity-type-builder/build-lanes.md` P1): the operator took the
recommendation, 2026-11-10, that only the two activity-type pages tell a refusal from a failure in this build, and that
the other pages be done page by page, each with its own fixed sentence, as a separate task.

## Root cause

_Why it happens. If this is not yet known, say so rather than guessing._

## What to build

_The intended shape of the work. Enough that someone else could start it._

## Verification

_This repo's completion signal. Each item names how it will be checked — a command, a test,
an observation. "Code exists" is not a completion signal._

- [ ] _criterion_ — checked by _command or observation_

## Related

_Task ids, decisions (`D<n>` for product, `W-<n>` for process), files, commits._

## Notes

_Observed facts, inferences, and unknowns. Label them when the difference matters._
