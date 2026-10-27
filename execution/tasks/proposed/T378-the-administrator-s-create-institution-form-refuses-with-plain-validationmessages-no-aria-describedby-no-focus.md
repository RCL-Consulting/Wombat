---
id: T378
title: The Administrator's Create institution form refuses with plain ValidationMessages: no aria-describedby, no focus
status: proposed
priority:
owner: agent
depends_on: []
created: 2026-10-27
---

# T378 — The Administrator's Create institution form refuses in a way a screen reader cannot follow

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. The Administrator's page only, and the words are right; the refusal is not announced or focused.
**Surfaced:** 2026-10-27, T376 step 7's states pass (`states/institution-edit--invalid.png`), on a post-act1 copy.

## Why

T376's states agent: "/admin/institutions/new, the Administrator's create form, Save with the form empty: the field
messages are plain `div.validation-message`, no `role="alert"`, the inputs `aria-invalid` but no `aria-describedby`;
focus stays on Save instead of moving to the first invalid field. Mbatha's edit form does this correctly."

## Symptom

On `/admin/institutions/new`, Save with Name and Short code empty shows each message under its field, but the inputs
are not described by the messages and the focus stays on Save. `/admin/institutions/{Id}` refuses the same fields with
`aria-describedby` and focuses the first invalid one.

## Root cause

Checked: `InstitutionCreate.razor` is its own page, an `EditForm` with `OnValidSubmit` and `DataAnnotationsValidator` /
`<ValidationMessage>` (lines 30-46). `InstitutionEdit.razor` has the page's own checks (`CheckFields`, `FieldMessage`,
`MessageIdIf`, `_focusFieldId`). Flow 12 changed the create page's words (R2) but not its pattern; it is flow 18's page.

## What to build

The create page refuses as the edit page does (or the two become one page, flow 18's call): `novalidate`, the page's
own checks from `UpdateInstitutionCommandValidator`'s words, each message `aria-describedby` from its input, the first
invalid field focused, nothing sent.

## Verification

- [ ] An empty create focuses Name, each input names its message by `aria-describedby` — a bUnit test, and a browser
  check on the replay's `states/institution-edit--invalid.png`.

## Related

T376 (flow 12), flow 18 (`design/flows/18-platform-operations.md`), T324, `InstitutionCreate.razor`,
`InstitutionEdit.razor`.

## Notes
