---
id: T337
title: The replay tool cannot age activities or start outside Development, so three steps and one state go unplayed
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-27
---

# T337 — The replay tool cannot age activities or start outside Development, so three steps and one state go unplayed

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. The replay is the redesign's acceptance check (BRIEF § 9). Three steps and one state are left
unplayed each time.
**Surfaced:** 2026-09-27, T335's step G replay (BRIEF § 11).

## Symptom

- Steps 3.30, 3.32 and 3.33's Overdue badge need Act 3's activities aged by five days. The step's Note prescribes an
  `UPDATE "Activities"` run by hand. The replay agent's permission rules refused it as a change to a shared resource,
  so nothing was stalled, no nudge was sent, and the badge read Requested.
- states.md's "/Error, after a failure" is served outside Development only. `tools/scenario-replay.ps1 start` always
  starts in Development, whose developer exception page answers instead. The replay cannot reach the page. Outside
  Development the user secrets are not loaded, and the connection string's password lives only there.

## What to build

- A `tools/scenario-replay.ps1 age <db> <days> [activity ids]` command. It runs the runbook's ageing statement on a
  `wombat_scenario*` database only, prints what it changed, and never touches the dev database. Steps 3.30 and 3.33 then
  name the command instead of raw SQL.
- A `start` option that runs the published app outside Development (for example `env=Staging`). It passes the
  connection string from the user secrets the way `sql` and `dump` already read them, without printing it. Add a states.md
  recipe for forcing a failure (the lock hold the Home lane used) on such a start.

## Verification

- [ ] `scenario-replay.ps1 age` refuses a database whose name does not start `wombat_scenario`; on a scratch copy it ages
  the named activities, and Steps 3.30–3.33 replay with it — the replay's Actual lines.
- [ ] A non-Development start serves the error page after a forced failure; `states/error--failed.png` is captured at
  1280 and 390 — the captures.
