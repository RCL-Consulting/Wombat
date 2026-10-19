---
id: T372
title: The MSF campaign page holds an empty band above its status card when there is no result message
status: proposed
priority:
owner: agent
depends_on: []
created: 2026-10-19
---

# T372 — The MSF campaign page holds an empty band above its status card when there is no result message

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. Cosmetic: about 40 px of empty space between the header rule and the status card.
**Surfaced:** 2026-10-19, T369's states pass, in states/campaign-edit--resend-refused.png and
campaign-edit--address-refused.png.

## Symptom

On /msf/campaigns/{id}, with no result message showing, an empty band of about 40 px sits between the
header rule and the status card.

## Root cause

Inferred: the result region (role=status) is rendered empty but keeps its margins or a min-height.

## What to build

Let the empty result region take no space (no margin while empty), as flow 09's pages do.

## Verification

- [ ] The gap is gone at 1280 and 390 with no message, and a message still shows in place — a capture of each.

## Related

T369 (flow 10); DESIGN.md's result-region pattern.

## Notes

- Observed by T369's states pass; not yet checked in the code.
