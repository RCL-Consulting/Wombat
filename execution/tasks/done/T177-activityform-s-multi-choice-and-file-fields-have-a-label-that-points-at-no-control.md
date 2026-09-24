---
id: T177
title: ActivityForm's multi-choice and file fields have a label that points at no control
status: done
priority: P3
owner: agent
depends_on: []
created: 2026-09-24
completed: 2026-09-24
---

# <id> — <one line that states the defect or the goal, not the solution>

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. An accessibility defect on the most-used form in the product: a screen reader announces no name
for the group.
**Surfaced:** 2026-09-24, the T147 review.

## Symptom

`src/Wombat.Web/Components/Shared/Activities/ActivityForm.razor` (around :35) wraps a MultiChoice field's checkboxes in
`<FormField InputId="@field.Key">`, but each checkbox's id is `{key}-{value}` (around :88), so the `<label for="{key}">`
names no element. The File placeholder (around :99) has the same problem.

## What to build

Follow DESIGN.md § Form system's checkbox-group rule, as T122 and T147 did: a `<fieldset>` with a `<legend>` for a
MultiChoice field, each checkbox labelled by its option, and no `<FormField>` wrapper. For the File placeholder, drop
the dangling `for` or render a real control. Reuse `tests/Wombat.Web.Tests/Accessibility/IdReferences.cs` (T147) to
assert that every `label[for]` resolves, on an ActivityForm rendered from each seeded schema.

## Verification

- [x] Every `label[for]` on an ActivityForm rendered from each seed schema names an existing element. bUnit.
- [x] Browser: an accessibility snapshot of a form with a multi-choice field shows a named group.

## Related

T147, T122, T154 (the File field and D34).

---

## As built — 2026-09-24

A MultiChoice field (and File, DateTime, Checkbox, Markdown) renders as a named `fieldset.form-group` with a legend.
Each checkbox is named by its option. Every other type is exactly one keyed control named by one `label[for]`.
`ActivityFormLabelTests` pins this per field type, and checks every seed schema for orphaned references. DESIGN.md's
rule is scoped to ActivityForm.

Browser on dev:
- The CCA form's "Documentation reviewed" is a named group, one checkbox per option.
- Orphan-label count was 0 on the Mini-CEX, QI Project and CCA new forms, and on activities 26 and 19.

The CCA options display as raw keys (`admission_notes`), because the seed gives no option labels: filed.
