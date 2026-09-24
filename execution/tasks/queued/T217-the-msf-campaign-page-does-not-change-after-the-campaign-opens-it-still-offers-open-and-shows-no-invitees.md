---
id: T217
title: The MSF campaign page does not change after the campaign opens: it still offers Open and shows no invitees
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
---

# T217 — The MSF campaign page does not change after the campaign opens: it still offers Open and shows no invitees

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. A coordinator sees stale controls; the handlers refuse a second open.
**Surfaced:** 2026-09-25, the T163 browser check (not new; identical in a 2026-09-24 screenshot).

## Symptom

After "Campaign opened…", `/msf/campaigns/{id}` still reads "Add invitees, then open the campaign to send anonymous
response links.", with "Open campaign" enabled and no invitee list.

## What to build

Show the campaign's state and its invitees (counts or anonymised rows, per MSF anonymity) once opened. Hide Open, and
offer only what the state allows (Close, Withdraw).

## Verification

- [ ] An open campaign's page shows its state and invitees, and no Open. bUnit and browser.

## Related

T205, T206, T202.
