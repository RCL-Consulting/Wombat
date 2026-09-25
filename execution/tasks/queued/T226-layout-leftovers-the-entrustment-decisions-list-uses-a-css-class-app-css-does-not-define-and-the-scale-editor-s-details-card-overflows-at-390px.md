---
id: T226
title: Layout leftovers: the entrustment decisions list uses a CSS class app.css does not define, and the scale editor's details card overflows at 390px
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
---

# T226 — Layout leftovers: the entrustment decisions list uses a CSS class app.css does not define, and the scale editor's details card overflows at 390px

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. Cosmetic.
**Surfaced:** 2026-09-25, the T198 review.

## Symptom

- `Admin/EntrustmentDecisions/Index.razor` uses `class="data-table"`, which `app.css` does not define. The table is
  unstyled.
- On the scale editor, the "Scale details" card's `form-grid--wide` needs at least 350px. At 390px the Name input sticks
  out of the card and the page scrolls sideways.
- Between about 641 and 800px, and 941 and 1060px, the scale editor's Label and Description inputs are about 39px and
  83px wide, because the buttons' column claims its width first. It needs a container query.
- From 641 to about 668px the scale table is up to about 20px wider than its card (compact cell padding).

## Verification

- [ ] The decisions list is a `clinic-table`. The scale editor has no horizontal scroll at 390, 700 and 1000px. Browser.

## Related

T198, T188.
