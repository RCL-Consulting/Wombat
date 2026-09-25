---
id: T259
title: Decisions-due names the panel an EPA routes to, not the panel the holding review sits before
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
---

# T259 — Decisions-due names the panel an EPA routes to, not the panel the holding review sits before

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. The coordinator is sent to the wrong panel.
**Surfaced:** 2026-09-25, the committee chain 2 browser check. It predates that chain: `HoldingSentence` in
`DecisionsDueText.cs`, from T131 slice 6.

## Symptom

A "Revoked: re-decide" row read "Review #13 is open for 2027 S2 before CC T194 Paediatrics panel", but review #13 sits
before panel 1. The sentence uses `SchedulePanelName`, the panel the EPA routes to, not the holding review's own panel.
Panels 1 and 3 count as the same seat, because both have a null decision-body key.

## What to build

Carry the holding review's own panel name on the DTO, and print it. bUnit, and a handler test with two panels in one
seat.

## Verification

- [ ] The row names the holding review's panel. Tests.

## Related

T131 (slice 6), T235.
