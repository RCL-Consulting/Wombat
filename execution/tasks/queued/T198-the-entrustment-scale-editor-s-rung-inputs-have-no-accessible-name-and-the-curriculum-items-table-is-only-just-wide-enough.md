---
id: T198
title: The entrustment scale editor's rung inputs have no accessible name, and the curriculum items table is only just wide enough
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-24
---

# <id> — <one line that states the defect or the goal, not the solution>

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. Accessibility, and layout headroom.
**Surfaced:** 2026-09-24, the T176 review.

## Symptom

- `EntrustmentScaleEdit.razor:75-76`: each rung's Label and Description inputs have no label and no `aria-label`. The
  same table has inline `style="width:…"` on two `<th>` elements, which DESIGN.md forbids.
- The curriculum items table fits at 1280px with 33px to spare (3px at 1250). A longer EPA title or tool name, or
  another column, brings the sideways scroll back.

## What to build

Name each rung input (e.g. "Label, rung 3") and move the widths into `app.css`. Let the EPA and Tools columns wrap, then
re-measure at 1280px.

## Verification

- [ ] Every input on the scale editor has a name. bUnit, using `AccessibleNames.cs`.
- [ ] The items table has at least 100px of headroom at 1280px. Browser.

## Related

T176, T125.
