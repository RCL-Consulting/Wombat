# T019-g — Schema templates, copy-from-existing, and advanced signature capture

**Phase:** Phase-2 follow-up
**Depends on:** T019
**Blocks:** nothing

## Goal

Three small enhancements that, together, make starting a new activity type dramatically faster: the ability to start a draft from an existing type, an institution-scoped template library, and a proper signature field. Each is independent; ship them in any order.

## What to do

### 1. Copy-from-existing

- "New from existing" button in `ActivityTypesList.razor`.
- Opens a picker of published activity types across the current scope and its parents. Selecting one creates a new draft whose schema/workflow/credit rules are copies of the source's **published** version (not staging).
- The new draft starts at version 1 with a new `Key` the admin must provide. Admin can edit everything before publishing.
- Audit log (T025) records the copy with the source type's id.

### 2. Institution template library

- Add `ActivityTypeTemplate` — a separate entity, not an `ActivityType`. Fields: `Id`, `Name`, `Description`, `Scope` (institution / speciality / sub-speciality), `FormSchemaJson`, `WorkflowJson`, `CreditRulesJson`, `CreatedByUserId`, `CreatedOn`. Not versioned.
- Admin UI under `Admin/ActivityTemplates/`: list, create, edit, delete. Templates are editable and deletable freely because no activities reference them.
- "New from template" button in `ActivityTypesList.razor`.
- "Save as template" button in `ActivityTypeEdit.razor` — captures the current draft or published version as a template.
- Seed a small starter library: one Mini-CEX template, one DOPS, one STAR, one research output. Institutions can extend or delete.

### 3. Advanced signature capture

- `signature` field from T019 is the simple version — just records a name and timestamp.
- This task adds optional canvas drawing: a drawing surface with pointer events, touch support, and undo. The signature image is saved as a PNG blob linked by the field value's `image_id`. The underlying metadata (name, role, timestamp) is still captured.
- Pure HTML canvas, no libraries. Draw on pointerdown+move, store stroke points, render on resize.
- Opt-in at the field level: the signature field gains a `capture_drawing` boolean in its type options. Default false — most institutions don't need it and drawn signatures are a storage and legal-review cost.

## Files created

- `src/Wombat.Web/Components/Pages/Admin/ActivityTypes/NewFromExistingDialog.razor`
- `src/Wombat.Web/Components/Pages/Admin/ActivityTemplates/**`
- `src/Wombat.Domain/Activities/ActivityTypeTemplate.cs`
- `src/Wombat.Infrastructure/Persistence/Configurations/Activities/ActivityTypeTemplateConfiguration.cs`
- `src/Wombat.Infrastructure/Persistence/Migrations/*ActivityTypeTemplate.cs`
- `src/Wombat.Web/Components/Shared/Activities/FieldRenderers/SignatureCanvasRenderer.razor`
- `src/Wombat.Web/wwwroot/js/signature-canvas.js`
- `tests/Wombat.Application.Tests/Activities/ActivityTypeTemplateTests.cs`

## Verification

- [ ] Copy an existing Mini-CEX type into a new `mini_cex_surgical` draft, tweak two fields, publish. Confirm the source type is unchanged.
- [ ] Save a published type as a template. Use the template to create a new type in a different sub-speciality. Confirm the new type has independent storage and does not update when the template is edited later.
- [ ] Signature capture: draw a signature on a tablet, submit the activity, reload the detail view, confirm the rendered signature matches.
- [ ] Signature capture in PDF export (T023): the drawn signature is included as an image on the relevant page.

## Notes & gotchas

- Templates are **snapshots**, not references. An activity type created from a template has no ongoing link to the template. This is deliberate; templates that live on would be a version-management nightmare.
- Storage for signature blobs: same path as file uploads (`/opt/wombat/data/uploads/activities/`), same integrity rules. Do not put them in jsonb.
- Legal weight of drawn signatures varies by jurisdiction. Document in `CUSTOMIZATION.md` that Wombat's signature capture is not a legally-binding e-signature solution and should not be used for consent forms or regulatory submissions without an external e-signature provider. It is for "the assessor signed off this activity" only.
- The starter template library is intentionally small. Institutions quickly learn that their needs differ from the defaults and delete them; shipping 30 templates is waste.
