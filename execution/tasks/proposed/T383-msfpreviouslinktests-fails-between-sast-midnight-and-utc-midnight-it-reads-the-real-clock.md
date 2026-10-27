---
id: T383
title: MsfPreviousLinkTests fails between SAST midnight and UTC midnight (it reads the real clock)
status: proposed
priority:
owner: agent
depends_on: []
created: 2026-10-28
---

# T383 — MsfPreviousLinkTests fails between SAST midnight and UTC midnight

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. A test that fails only in a two-hour window each night.
**Surfaced:** 2026-10-28, T379 (flow 13).

## Why

2026-10-28 00:xx SAST, T379's fix commit: `MsfPreviousLinkTests.ThePreviousLink_OpensTheQuestionnaire_UntilTheLastDayToRespond(closesInDays: 0, opens: True)` failed with "This feedback link has expired" (`MsfCampaignRules.cs:767`), and passed earlier the same evening.

## Symptom

As above.

## Root cause

Inferred: the test or `GetActiveInvitationByTokenAsync` takes today from the real clock, so between 00:00 SAST and 00:00 UTC the closing day reads differently. Not yet read.

## What to build

Pin the clock in the test (a FixedClock, as other tests do), and check whether the rule itself judges the closing day on the SAST calendar.

## Verification

- [ ] The test passes with the clock pinned at 22:30 UTC (already tomorrow in SAST) and at 12:00 UTC.

## Related

T379, T325.

## Notes
