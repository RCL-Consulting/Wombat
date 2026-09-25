---
id: T198
title: The entrustment scale editor's rung inputs have no accessible name, and the curriculum items table is only just wide enough
status: done
priority: P3
owner: agent
depends_on: []
created: 2026-09-24
completed: 2026-09-25
---

# T198 — The entrustment scale editor's rung inputs have no accessible name, and the curriculum items table is only just wide enough

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

- [x] Every input on the scale editor has a name. bUnit, using `AccessibleNames.cs`.
- [x] The items table has at least 100px of headroom at 1280px. Browser.

## Related

T176, T125.

---

## As built — 2026-09-25 (`ef81430`)

- **The scale editor.** Every rung input is named by its column and row ("Label, level 3"), and the row's buttons too.
  The table is `clinic-table--compact clinic-table--inputs`, with no hover tint. The buttons column is `.col-actions`,
  and its buttons wrap before the table overflows its card. `AccessibleNames` bUnit test.
- **The curriculum items table.** Its EPA and Tools columns wrap, with rem minimum widths (`.col-wrap`,
  `.col-wrap--wide`). `TableColumnClassTests` holds the rules.
- DESIGN.md § Table system states the trade-off with measured figures.

Browser on dev (scripted Chrome, master `95b65f4`): as collegeadmin on `/admin/curricula/2/items`:
- **1280×800:** the container has no scroll (922 = 922). At `min-content` the table measures 794px, 128px under the
  container. 762 words were checked with no mid-word breaks (11 breaks, all after a hyphen).
- **1024px:** EPA 96px and Tools 80px. The table scrolls inside its container and the page does not.
- **Not run:** the scale editor in a browser. It is Administrator-only, and no session may use the admin credential. It
  is covered by the bUnit accessible-name test and the fixer's static renders (no overflow at 13 widths from 700px, and
  none at 390px).

**Filed:** [T226] (layout leftovers from the review).
