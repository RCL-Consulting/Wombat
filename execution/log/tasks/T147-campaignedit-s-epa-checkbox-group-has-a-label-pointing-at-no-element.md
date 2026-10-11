---
id: T147
title: CampaignEdit's EPA checkbox group has a label pointing at no element
status: done
priority: P3
owner: agent
model: sonnet
depends_on: []
created: 2026-09-23
started: 2026-09-24
completed: 2026-09-24
---

# T147 — The MSF campaign's "Evidence for these EPAs" label is for an id nothing has

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. It is an accessibility defect. A screen reader announces the group without its label, and
clicking the label does nothing.
**Surfaced:** 2026-09-23, by [T122]'s design critique, while choosing the pattern for the curriculum tool list.

## Symptom

`CampaignEdit.razor` wraps its EPA checkboxes in `<FormField Label="Evidence for these EPAs" InputId="msf-epas">`.
`FormField` renders `<label for="msf-epas">`, but no element has `id="msf-epas"`: each checkbox has its own
`msf-epa-{id}`. DESIGN.md § Accessibility requires every field's `<label for>` to match its input's id.

## Root cause

`FormField` is for a single input. A group of checkboxes has no single input to point at.

## What to build

Use the pattern [T122] used for the curriculum tool list: a `<fieldset>` with a `<legend>`, the checkboxes in a
`.check-grid` of `.form-check`s, and the help text as a paragraph inside the fieldset.

## Verification

- [x] No `label[for]` on the page points at a missing id — bUnit test over the rendered markup
- [x] The group is announced by its legend — browser check

## Related

[T122] (`CurriculumItemsEdit.razor`, `.check-grid` in `app.css`), DESIGN.md § Form system and § Accessibility.

---

## As built — 2026-09-24

- The campaign editor's EPA checkboxes are a `<fieldset>` with a `<legend>` ("Evidence for these EPAs required"), and
  each checkbox is named by its EPA, following DESIGN.md's checkbox-group rule. The fieldset is described by its help
  text. A shared test helper, `tests/Wombat.Web.Tests/Accessibility/IdReferences.cs`, asserts that every `label[for]`
  and `aria-describedby` on the rendered page names an existing element. The other MSF pages were checked and needed no
  change.
- Browser on dev (admin, `/msf/campaigns/new`, trainee chosen). The accessibility snapshot shows the named group with
  15 checkboxes, PAED-001 to PAED-015. A DOM check found 25 labels, every `label[for]` resolving and every
  `label.control` set, with no duplicate ids. With no trainee chosen, the group shows "Select a trainee first." and all
  10 labels still resolve. No campaign was saved.
- Filed: [T177] (ActivityForm's multi-choice and file fields have the same defect).
