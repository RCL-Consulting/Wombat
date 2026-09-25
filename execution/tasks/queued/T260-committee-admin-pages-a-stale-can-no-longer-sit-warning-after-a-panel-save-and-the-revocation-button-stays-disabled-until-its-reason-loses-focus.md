---
id: T260
title: Committee admin pages: a stale can-no-longer-sit warning after a panel save, and the revocation button stays disabled until its reason loses focus
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
---

# T260 — Committee admin pages: a stale can-no-longer-sit warning after a panel save, and the revocation button stays disabled until its reason loses focus

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low.
**Surfaced:** 2026-09-25, the committee chain 2 browser check.

## Symptom

1. **After a panel save.** Save removes a member who can no longer sit and shows "Panel members updated.", but the
   yellow "1 member … can no longer sit on it… Saving takes that member off the panel." warning stays until a reload
   (panels 1 and 3 on dev). This is from T237.
2. **Revocation.** On `/admin/entrustment-decisions`, "Confirm revocation" stays disabled until the reason box loses
   focus: the binding updates on change, not on input. A click straight after typing hits a disabled button. This
   predates the chain.

## What to build

1. Re-read the panel after a save, so the warning reflects it.
2. Bind the reason with `:event="oninput"`, or enable the button on input. bUnit for both.

## Verification

- [ ] Both, with bUnit tests.

## Related

T237, T182.
