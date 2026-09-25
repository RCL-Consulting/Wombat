---
id: T220
title: Activity pages, lists and dashboards show workflow state and move keys, not their labels
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
---

# T220 — Activity pages, lists and dashboards show workflow state and move keys, not their labels

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. Three seed states differ from their key: `submitted` is "Awaiting supervisor", "Awaiting review"
or "Awaiting discussion". Elsewhere the key and label differ only in case.
**Surfaced:** 2026-09-25, the T189 review (the display half of its finding 1).

## Symptom

- `ActivityView.razor` prints `CurrentState` in its header ("State: {key}") and its summary. Its history prints
  `TransitionKey` and "FromState → ToState".
- `MyActivities`, `ActivityInbox` and `TraineeDashboard` print the state key.

T189 made refusals and notices use labels, so the pages disagree with their own notices.

## What to build

Carry the state and move labels (`Workflow.StateLabel` and the move labels, from the pinned version) in the activity DTOs,
and print them everywhere a key is shown today.

## Verification

- [ ] Every surface above shows labels. bUnit, and a browser check on a clinical audit ("Awaiting supervisor").

## Related

T189, T172.
