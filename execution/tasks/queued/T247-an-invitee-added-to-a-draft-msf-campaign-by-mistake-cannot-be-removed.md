---
id: T247
title: An invitee added to a draft MSF campaign by mistake cannot be removed
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
---

# T247 — An invitee added to a draft MSF campaign by mistake cannot be removed

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. The only remedy is to withdraw the campaign and create it again.
**Surfaced:** 2026-09-25, the T228 review (finding 2).

## What to build

A Remove on each draft invitee row, and a command to go with it:
- the T113 scope check before any change;
- only while the campaign is a draft (no link sent, no response);
- an audit row with the address redacted (T205);
- a ConfirmDialog and focus handling per DESIGN.md.

The invitee table shows counts per group (T217), so the control needs a way to reach one address while the campaign is a
draft. Decide how: a draft-only list of the addresses the coordinator typed.

## Verification

- [ ] A draft invitee can be removed, and an open campaign's cannot. Handler and bUnit tests.

## Related

T228, T217, T205, T113.
