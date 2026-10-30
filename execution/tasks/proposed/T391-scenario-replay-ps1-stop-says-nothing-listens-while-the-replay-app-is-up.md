---
id: T391
title: scenario-replay.ps1 stop says nothing listens while the replay app is up
status: proposed
priority:
owner: agent
depends_on: []
created: 2026-10-30
---

# T391 — scenario-replay.ps1 stop says nothing listens while the replay app is up

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. Tooling only: a replay or states agent believes the app stopped, and the next publish fails on
locked files (`MSB3026`, "being used by another process").
**Surfaced:** 2026-10-30, T387 step 7 (Act 6, the appendix, the states pass and the integrator's republish).

## Why

`tools\scenario-replay.ps1 stop 5180` (and `5181`) printed "Nothing listens on 5180" while the app on that port
answered HTTP 200; `Get-NetTCPConnection` did not see the listener. Every agent then stopped the process by id after
reading its command line (`.scenario-appin\Wombat.Web.dll --Wombat:BaseUrl=http://localhost:5180`).

## Symptom

`stop <port>` reports nothing listening; the app keeps running and locks `.scenario-appin`.

## Root cause

Not confirmed. The script's listener lookup misses the process (inferred: `Get-NetTCPConnection` under the shell the
tool runs in, or a listener bound to IPv6 only).

## What to build

Find the listener another way when the first finds none (`netstat -ano`, or the `dotnet` process whose command line
names `.scenario-appin\Wombat.Web.dll` and `--Wombat:BaseUrl=http://localhost:<port>`), and stop only that one.

## Verification

_This repo's completion signal. Each item names how it will be checked — a command, a test,
an observation. "Code exists" is not a completion signal._

- [ ] _criterion_ — checked by _command or observation_

## Related

_Task ids, decisions (`D<n>` for product, `W-<n>` for process), files, commits._

## Notes

_Observed facts, inferences, and unknowns. Label them when the difference matters._
