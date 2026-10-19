---
id: T371
title: An MSF report lists a group's comments in the order they were submitted
status: proposed
priority:
owner: agent
depends_on: []
created: 2026-10-19
---

# T371 — An MSF report lists a group's comments in the order they were submitted

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium (needs confirmation). Anonymity is flow 10's first rule. The order names no one, but it follows
submission time, and the coordinator watches per-group responded counts while the campaign is open
(R7), so the coordinator may be able to tie a comment to the moment someone answered.
**Surfaced:** 2026-10-19, T369's states pass: the Nurse card lists Mokwena's, Tau's and Nkosi's comments in the
order they answered at Steps 3.39 and 3.42.

## Symptom

Within a respondent group, the released report (coordinator's /msf/reports/{id} and the registrar's
/msf/my-reports/{id}) lists the free-text comments in submission order.

## Root cause

Not yet traced. Inferred: the report reads the responses in insertion or id order.

## What to build

Decide with the operator whether order counts as identifying. If it does, list comments in an order that
carries nothing about time (sorted by text, or shuffled once and stored at close), on every report
surface and in the portfolio PDF.

## Verification

- [ ] The operator's decision recorded — the task's notes.
- [ ] If changed: a test that two campaigns with the same comments submitted in different orders show
  them in the same order — the test name.

## Related

T369 (flow 10), R7 (round 2 review), D9.

## Notes

- Observed by T369's states pass; not yet checked in the code.
