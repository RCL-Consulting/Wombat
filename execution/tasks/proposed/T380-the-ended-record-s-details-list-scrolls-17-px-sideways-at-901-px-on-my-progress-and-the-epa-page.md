---
id: T380
title: The ended record's details list scrolls 17 px sideways at 901 px on My progress and the EPA page
status: proposed
priority:
owner: agent
depends_on: []
created: 2026-10-27
---

# T380 — The ended record's details list scrolls 17 px sideways at 901 px

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. A narrow-desktop overflow on two read-only pages.
**Surfaced:** 2026-10-27, flow 13's round 3 (T379): the canvas's Spec measured 17 px sideways at 901 px on R3-R03 and
R3-R06 (the ended EPA page and My progress), naming flow 05's shipped `.details-list` as the cause.

## Why

The round-3 check (`design/flows/13-graduation-and-exit/round-3-check.md`, A4): "it reports 17 px sideways at 901 on
R3-R03 and R3-R06. The cause is flow 05's shipped `.details-list`, named on the Spec and proposed as a system task."
Unreplayed on the built pages: measure them at 901 px first.

## Symptom

Inferred from the boards, not yet measured on the app: at 901 px the details list on My progress's ended record and the
EPA page is wider than its container.

## Root cause

Not yet known; the Spec names `.details-list` (app.css, flow 05).

## What to build

Measure My progress and the EPA page at 901, 1000 and 1100 px on the replay; if they overflow, let `.details-list` wrap
or shrink so nothing scrolls sideways.

## Verification

- [ ] Nothing scrolls sideways at 901, 1000 and 1100 px on My progress and an EPA page — a browser measurement.

## Related

T379 (flow 13), flow 05 (T355), `design/flows/13-graduation-and-exit/round-3/project/R3-0-spec-3.dc.html`.

## Notes
