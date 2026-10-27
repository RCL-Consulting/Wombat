---
id: T384
title: Scheduled jobs' run history reads its From date as a UTC day
status: proposed
priority:
owner: agent
depends_on: []
created: 2026-10-28
---

# T384 — Scheduled jobs' run history reads its From date as a UTC day

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. A filter shows the wrong day's runs for two hours each night.
**Surfaced:** 2026-10-28, T379 (flow 13).

## Why

T379's replay, A.2.10 (2026-10-28 00:xx SAST): "the Run-history From date is read as a UTC day. At 00:xx SAST, 'From today' (2026-10-28) gives 'No runs found', and 2026-10-27 shows today's runs." Kin to T325.

## Symptom

As above.

## Root cause

Inferred from the replay: the From date is turned into a UTC midnight, not the start of the SAST day. Not yet read.

## What to build

Read From as the start of the day on the South African calendar, as the other day filters do.

## Verification

- [ ] An Application test: with the clock at 22:30 UTC, From = the SAST today lists the run made a minute ago.

## Related

T379, T325.

## Notes
