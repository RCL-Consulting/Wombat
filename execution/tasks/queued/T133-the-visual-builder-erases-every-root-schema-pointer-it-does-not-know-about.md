---
id: T133
title: "The visual builder erases every root schema pointer it does not know about"
status: queued
priority: P1
owner: agent
depends_on: []
created: 2026-09-20
---

# T133 — Saving a draft from the Form tab silently drops `observation_date_field` and `rated_level_field`

**Severity:** High. It is silent, it is on the only schema-authoring path the product has, and it
un-does two shipped features for any type an operator touches.
**Surfaced:** 2026-09-20, during T126's design pass. Nobody was looking for it — it was found by
tracing T119's pointer end to end to copy its shape.

## Symptom

`BuilderSchemaModel.ToJson()` builds its schema with the **two-argument** constructor:

```csharp
var schema = new FormSchema(
    1,
    Sections.Select(...)
```

`FormSchema`'s root pointers are trailing optional parameters, so both default to null. And
`BuilderSchemaModel.Parse` never reads them in the first place — it copies section and field
properties (including `ScaleKey` and `EditableBy`) and nothing else.

The Form tab is a **structured builder only**: `ActivityTypeEdit.razor` has raw JSON textareas for
workflow and credit, but the schema comes from `CurrentDraftSchemaJson => _builderSchema.ToJson()`.
So `BuilderSchemaModel` is the sole schema-authoring path, and any operator who opens a type and
saves a draft erases:

- `observation_date_field` (T119) — the type silently reverts to dating activities by the audit
  clock, which is the exact defect T119 exists to remove.
- `rated_level_field` (T126) — the type can no longer say which ladder its ratings sit on.

Neither failure is visible. The JSON parses, the builder renders, the publish succeeds.

## Root cause

`BuilderSectionModel.EditableBy` carries this convention in writing:

> *"Carried through the builder round-trip verbatim. The builder has no editor for it yet ... so
> preserving the parsed rule is what stops a save from silently dropping an `editable_by` authored in
> the raw JSON."*

Root-level properties were never given the same treatment. T119 added the first one and did not
extend `BuilderModels`; T126 added the second knowing this was outstanding.

## The trap — carry-through alone is NOT enough

**Do not fix only `Parse`/`ToJson`.** Once the builder carries a pointer through, an admin who
deletes the pointed-at field produces a schema the parser refuses
(`"rated_level_field 'x' does not match any field in the schema."`). That surfaces as `_actionError`
on the page — and with no control to clear the pointer, **the type becomes uneditable through the
only path that exists.** A silent drop would be traded for a dead end.

So the two halves ship together:

1. `Parse` reads both pointers; `ToJson` emits them.
2. A Form-tab control sets them — two selects, each listing the eligible fields (`date`-typed for the
   observation date, `scale`-typed for the rating) plus a **None** option.

`execution/architecture/DESIGN.md` is mandatory before the Razor half.

## Why this is P1

It is the only route by which an operator-built type can ever declare either pointer, and
`ActivityTypeSeedRefresher` can never reach an operator-published type. The four legacy `*_paed`
types are operator data that exists in no seeder — `scenario-paediatrics.md` step 1.14 has the
operator build all ten through this builder. Until this lands, T126's pointer is reachable only by
editing a seed folder.

## Verification

- [ ] A draft saved from the Form tab preserves both root pointers — checked by a bUnit or
      round-trip test over `BuilderSchemaModel.Parse` + `ToJson`, which **nothing currently covers**
      (no test in `tests/` references `BuilderSchemaModel`)
- [ ] Both pointers are settable and clearable from the Form tab — checked in the browser
- [ ] Deleting the pointed-at field does not strand the type — checked in the browser: set a
      pointer, delete its field, confirm the page lets you clear the pointer rather than refusing
- [ ] `GetPublishWarnings` mentions a pointer that was added, removed or re-pointed — it warns about
      removed sections, removed fields, type changes and newly-required fields today, and would say
      nothing about this
- [ ] Full suite green, no `--no-build`

## Related

Regression of [T119]; blocks the operator half of [T126]. `GetActivityTypeEditorQuery.DefaultSchemaJson`
is the template every builder-built type starts from and may want a pointer too.

## Notes

- **Observed, code-read, not browser-confirmed:** the claim that no control exists rests on
  `ActivityTypeEdit.razor` having no raw schema editor. Confirm in the browser before designing.
- **Unverified:** whether any seeded type in the dev or production database has *already* lost
  `observation_date_field` to a builder save. One query over `ActivityTypeVersions` settles it and
  should be run first — if any has, the loss is already in the data, not just latent.
