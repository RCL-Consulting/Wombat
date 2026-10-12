---
id: T362
title: Committee review: members' pages follow the chair's changes live during a sitting
status: proposed
priority:
owner: agent
depends_on: []
created: 2026-10-12
---

# T362 — Committee review: members' pages follow the chair's changes live during a sitting

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. Members can refresh; nothing is wrong, only stale.
**Surfaced:** 2026-10-12, flow 07's round 1 (T361): all three variations drew a member's review page that follows the
chair's staging live ("Updated 14:02 SAST · follows the chair's changes", PROPOSED).

## Why

The operator chose, on recommendation (2026-10-12), not to build it in flow 07: flow 07 draws "As of 14:02 · Refresh"
instead, and live following is filed here.

## Symptom

A panel member's `/committee/reviews/{id}` shows what it read when opened; the chair's stages, removals and the record
reach it only on a reload.

## What to build

Not designed. A cross-circuit notification (one Blazor Server circuit per reader) keyed by review id, raised after each
committed change to the review, on which an open page re-reads. Decide whether it also reaches the registrar's and the
administrator's views.

## Verification

- [ ] A member's open page shows the chair's staged decision without a reload — checked in two browsers on the replay.

## Related

T361 (flow 07), `design/flows/07-review-sitting/round-2-ask.txt` (C4).
