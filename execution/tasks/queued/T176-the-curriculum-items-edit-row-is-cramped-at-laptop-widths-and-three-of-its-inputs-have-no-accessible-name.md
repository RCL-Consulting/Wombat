---
id: T176
title: The curriculum items edit row is cramped at laptop widths, and three of its inputs have no accessible name
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

**Severity:** Low. Usable, but awkward on a common laptop width, and three inputs fail WCAG's name requirement.
**Surfaced:** 2026-09-24, T125's browser check at a viewport about 1250px wide. Mostly not introduced by T125.

## Symptom

- In the edit row on `/admin/curricula/{id}/items` (`CurriculumItemsEdit.razor`), the Scale select shows "CPSA" or
  "O-R S", the minimum picker shows "Cho…", and "per semester" shows "per". O-R labels such as "Indirect supervision"
  are cut off.
- The table scrolls sideways inside its container (about 1018 against 876px), so Save and Cancel are off-screen until
  the admin scrolls.
- The edit row's EPA select, Completion window and Weight inputs have no accessible name.
- The status that explains a disabled Save sits in the full-width sub-row, away from the button.

## What to build

Move the edit-row controls into the sub-row form (as T125 already did for the per-year editor and tools), or give the
row a stacked layout below a breakpoint, per DESIGN.md's table and form rules. Keep Save and Cancel visible without
horizontal scrolling at 1280px. Give every input a label (`<label for>` or `aria-label`).

## Verification

- [ ] At 1280×800 the edit row's controls read in full and Save is visible without scrolling. Browser.
- [ ] Every input in the edit row and the Add form has an accessible name. bUnit test over the rendered markup.

## Related

T125, T139 (edits the same page; sequence with it).
