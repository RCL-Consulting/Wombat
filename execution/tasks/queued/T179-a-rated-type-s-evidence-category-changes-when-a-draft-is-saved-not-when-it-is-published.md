---
id: T179
title: A rated type's evidence category changes when a draft is saved, not when it is published
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-24
---

# T179 — A rated type's evidence category changes when a draft is saved, not when it is published

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. Only a committee's source count can move, and only while a builder draft is in flight.
**Surfaced:** 2026-09-24, the T144 review (minor, confirmed, documented rather than fixed).

## Symptom

Since T144, `RatedActivityTypes.Classify` reads a rated source's category from `ActivityType.WbaToolKey`.
`SaveActivityTypeDraftCommand` writes `WbaToolKey` on **save**, and discarding the draft does not undo it. Saving a
draft that picks another instrument therefore relabels every activity of the type, completed ones included. That can
move a committee sampling report's distinct-source count before anything is published. What counts as *rated* still
follows the published schema.

## What to build

Stage `WbaToolKey` with the draft (a draft column, applied at `PublishDraft`), so the instrument changes only on
publish, as the schema does. This touches T122's write path, where the tool gate reads the live key. Decide whether
the gate should also read only the published key, and record the decision.

## Verification

- [ ] Saving a draft with a different instrument changes no classification and no gate verdict until it is
      published. Test.
- [ ] Publishing it does. Test.

## Related

T144, T122 (the `WbaToolKey` write path and the gate).
