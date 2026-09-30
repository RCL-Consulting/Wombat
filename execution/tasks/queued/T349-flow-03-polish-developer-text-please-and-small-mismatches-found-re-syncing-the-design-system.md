---
id: T349
title: Flow 03 polish: developer text, Please, and small mismatches found re-syncing the design system
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-30
---

# T349 — Flow 03 polish: developer text, "Please", and small mismatches found re-syncing the design system

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. Copy and consistency on flow 03's pages; nothing refuses or loses work.
**Surfaced:** 2026-09-30, T342 step 8 (re-syncing `design/system/` to flow 03 as built). Observed in the code at `c92f578a`.

## Symptom

1. `ActivityPageModel.AcrossScalesWarning` says "Please raise it…" (the voice drops "Please").
2. `ActivityForm` can show developer text to a user: "File uploads are represented in the schema, but storage wiring
   lands in a later pass." and "Unsupported field type: …".
3. The note panel's required `*` is a bare `aria-hidden` span, not `.required-mark`, so it is not red like the form's.
4. Discard changes is `aria-disabled` with nothing to discard, and says no reason (the "unavailable action says why" rule).
5. DESIGN.md gives the Activity inbox subtitle as "Work waiting for an assessor's rating or review."; the code says
   "Work waiting for you to rate, review or record."
6. `ActivityStatus.razor`'s comment and `ActivityStatusTone.Yours`'s doc name `.activity-status--yours`, which does not
   exist; the code uses `.detail-card--emphasis`.
7. File it again's notice ends "Nothing is saved until you do." (submit), though Save draft on the same form saves.
8. "Waiting for Awaiting review." reads doubled in the fallback headline and the Who has it now cell (as specified).
9. Home's Needs you footer is a plain link "Open My activities", Recent activities' a `.btn-sm .btn-outline`
   "All activities →", to the same page (flow 05 may own this).

## What to build

Fix 1–7 directly (copy, one class, one doc line each); reword 8 with the design system's voice; leave 9 to flow 05 and
note it there.

## Verification

- [ ] Each item's new text or class asserted in a bUnit test where the page has one; DESIGN.md and comments by diff.

## Related

T342 (flow 03), T343–T348; `design/system/README.md` § Flow 03.
