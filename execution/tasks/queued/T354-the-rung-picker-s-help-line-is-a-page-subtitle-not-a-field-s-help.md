---
id: T354
title: The rung picker's help line is a page subtitle, not a field's help
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-10-03
---

# T354 — The rung picker's help line is a page subtitle, not a field's help

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. It looks nearly the same (both muted 0.9rem; the subtitle adds a 4px top margin), but it breaks
DESIGN.md's activity-form rule and the one class every other field's help uses.
**Surfaced:** 2026-10-03, T350 step 8 (the design system's re-sync, read against DESIGN.md).

## Symptom

`RatedLevelPicker.razor:24` renders the field's help as `<small class="page-subtitle">`. DESIGN.md (§ the activity
form, "Help under the control") gives every field `p.field-help` with id `<key>-help`, named first in the control's
`aria-describedby`.

## Root cause

Flow 04 built the picker apart from `FormField`; its help line took the nearest muted style.

## What to build

Render it as `<p class="field-help" id="@HelpId">`, keeping the id and the `aria-describedby` order; update the bUnit
test that pins the markup and the design system's RatedLevelPicker README and preview.

## Verification

- [ ] The picker's help is `p.field-help` — a bUnit assertion; the rated states re-taken look unchanged.

## Related

T350 (flow 04), DESIGN.md § the activity form, `design/system/components/RatedLevelPicker/`.
