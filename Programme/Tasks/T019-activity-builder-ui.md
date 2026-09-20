# T019 — Activity builder UI + dynamic form renderer (v1)

**Phase:** 2 — Activity platform
**Depends on:** T018, T010 (web chrome + design system)
**Blocks:** T020, T011
**Follow-ups:** T019-b (drag-drop), T019-c (nested + repeatable sections), T019-d (visual workflow editor), T019-e (visual credit editor), T019-f (multi-condition visibility), T019-g (schema templates & copy)

> **Read `../DESIGN.md` before touching any Razor file.** T019 consumes (and does not silently extend) the design system defined there and built in T010: `PageHeader`, `.form-container`, `.form-grid`, `.form-actions`, `.detail-card` (+ `--interactive` for hoverable add-field targets, `--empty` for the "no fields yet" state), `.clinic-table`, `.btn`/`.btn-primary`/`.btn-outline`/`.btn-sm`, `.validation-summary-errors`, `.alert`, `StatePanel`, `ConfirmDialog`, `FormField`, `FormActions`. If the builder truly needs a new shared class (likely: `.tab-bar`, `.tab-bar-tab`, `.builder-two-col`, `.field-type-icon`, `.state-diagram` — enumerate before starting), add it to `DESIGN.md` in the same commit and land the rule in `app.css` before using it anywhere. **Never** inline a `<style>` block inside a builder page. The builder is the most complex UI in Wombat; it is also the loudest test of whether the design system holds — so it hurts first and most when the contract breaks.

## Goal

Deliver the two halves of the no-code UX for phase 1: an admin builder that creates and edits `ActivityType` schemas/workflows/credit rules through a visual form editor plus validated JSON editors for the two less-touched tabs, and a runtime renderer that takes any schema and any activity and produces a working form for the user. With this task done, adding a new activity type is a matter of clicking through the builder — good enough to meet or slightly exceed the old Wombat builder, not good enough to compete with MedHub. The polish lives in the T019-b … T019-g follow-ups.

The guiding principle for this task is **honest scoping**. Everything in the "Deferred to follow-ups" section below is deliberately *not* here. Do not sneak them in mid-task. If a real user need surfaces during T019 that pulls toward a deferred feature, write it down in `current_state.md` as a note for the relevant follow-up task and keep going.

## What to do

### Builder (admin side)

1. **Pages** under `Wombat.Web/Components/Pages/Admin/ActivityTypes/`:
   - `ActivityTypesList.razor` — lists types in the current scope, filter by active/scope/key, version badge, "Edit draft" and "View published" actions. Uses `PageHeader` + `.clinic-table` inside `.table-container` + `PagerControls` — do not hand-roll a `<table class="table">`.
   - `ActivityTypeEdit.razor` — three-tab editor: **Form**, **Workflow**, **Credit**. Page shell is `PageHeader` followed by a `.tab-bar` (new class in `DESIGN.md`) followed by the active tab's body wrapped in `.form-container`. Save/Discard/Publish/Preview live in a sticky `.form-actions` bar at the bottom of the page.
   - Header shows the published version, the draft version (if one exists), and buttons: **Save draft**, **Discard draft**, **Publish new version**, **Preview as trainee**. Buttons use `.btn .btn-primary`, `.btn .btn-outline`, and `.btn .btn-danger` — never `.btn-outline-primary`.

2. **Draft/publish lifecycle**:
   - The published `ActivityType` row is immutable. Editing means creating a draft.
   - "Edit" on a published type creates a draft row (or loads the existing draft) in `ActivityType.Draft*` staging columns — `StagingSchemaJson`, `StagingWorkflowJson`, `StagingCreditRulesJson`, `StagingUpdatedByUserId`, `StagingUpdatedOn`.
   - Only one draft exists at a time per type. Collaborative editing is out of scope for v1 — last-writer-wins with a stale-read warning if two admins open the same draft.
   - **Publish** validates the whole draft (schema + workflow + credit rules), bumps `Version`, atomically copies staging into the canonical columns, clears staging, writes an audit entry (T025), and increments the version history.
   - **Discard** clears the staging columns with a confirmation dialog.
   - Existing activities stay pinned to the version they were created under. They are never rewritten by a publish.

3. **Form tab — the visual builder**:
   - Two-column layout using the shared `.builder-two-col` grid (add to `DESIGN.md` — a `display: grid; grid-template-columns: minmax(320px, 1fr) minmax(400px, 1.4fr); gap: var(--space-6);` container, collapsing to a single column below 1100px).
   - Left column: a `.detail-card` titled "Sections" containing a list of section cards. Each section is itself a nested `.detail-card` with its own fields rendered as `.detail-card--interactive` rows. Each field row shows its label, type icon (from the `.field-type-icon` sprite — Lucide SVGs sized via `--space-5`), and a "required" dot if applicable.
   - Right column: a `.detail-card` titled "Live preview" containing the runtime `ActivityForm.razor` component rendered against the current staging schema with dummy data. The preview card is the same visual treatment a trainee sees at submission time, so any drift shows up immediately.
   - Section controls on each section card: **Edit title**, **Delete section**, **Move section up**, **Move section down**, **Add field**.
   - Field controls on each field row: **Edit**, **Delete**, **Move field up**, **Move field down** (within the section). Moving a field between sections in v1 is done via Delete + Add in the target section; cross-section moves are deferred to T019-b.
   - **Add field** opens an inline panel (not a modal) with a field-type dropdown of the ten supported types. Selecting a type reveals the type-specific edit panel below it, with sensible defaults.
   - **Edit field** reuses the same inline panel in-place.
   - The edit panel saves on blur / on explicit "Apply" click. The preview updates reactively. Blazor Interactive Server handles the state sync for free — do not hand-roll a diff mechanism.

4. **Field types — exactly ten, no more**:
   - `text` — short single-line. Options: max length, regex validation, placeholder.
   - `longtext` — multi-line textarea. Options: max length, rows hint.
   - `number` — numeric. Options: min, max, step, integer-only flag, unit suffix.
   - `date` — date picker (no time). Options: min date, max date (literal or `today`).
   - `choice` — single-select dropdown or radio group. Options: list of `{ value, label }`, display-as (`dropdown` or `radio`), allow-other flag.
   - `multichoice` — checkbox group. Options: list of `{ value, label }`, min selections, max selections.
   - `likert` — rating scale. Options: scale reference (key into `EntrustmentScale` or a named scale in the schema), display-as (`buttons` or `slider`).
   - `procedure_ref` — picker tied to the procedure catalogue (see T020). Options: catalogue filter by speciality.
   - `file` — file upload. Options: mime allowlist, max size (bytes), multiple-files flag (capped at 5).
   - `signature` — the submitter's name, role, and UTC timestamp captured at submit time. No fancy canvas drawing in v1 — T019-g or later can add that if anyone asks.

   Every field has these common properties regardless of type: `key` (stable snake_case identifier, auto-generated from the label on first entry, editable until the field is saved for the first time in a published version, then **locked**), `label`, `help_text`, `required`, `show_if` (a single condition — see below).

5. **Sections — flat, one level deep**:
   - A section has a `key`, `title`, `description` (optional), and a flat list of fields.
   - No nested sections in v1. One section contains fields, not other sections.
   - No repeatable sections in v1. QI PDSA cycles in T020 are handled by three pre-numbered sub-sections (`pdsa_1`, `pdsa_2`, `pdsa_3`) rather than a repeatable group. Document in T020.

6. **Conditional visibility — one condition per field**:
   - Each field (and each section) may have a single `show_if` condition expressed in the UI as three dropdowns: **Field** (any previously declared field in the same schema), **Operator** (`equals`, `not equals`, `is set`, `is not set`, `greater than`, `less than`), **Value** (free text, type-coerced against the referenced field).
   - Conditions are evaluated live in the renderer. A hidden field's value is omitted from submission payloads.
   - Multi-condition visibility (AND/OR, grouping) is deferred to T019-f.

7. **Workflow tab — JSON editor with validation**:
   - Monaco-style code editor (via the Blazor Monaco interop, or a plain textarea with a monospace font and a JSON syntax validator if Monaco proves awkward in Interactive Server). Prefer the textarea path in v1 to avoid a heavy client dependency — the editor can be upgraded later without changing the task contract.
   - Runs the T018 workflow validator on every change (debounced 300ms), surfacing errors inline: unreachable states, transitions referencing unknown states, actor rules referencing unknown roles, field references pointing at fields not present in the form tab.
   - A "Test transition" button lets the admin pick a starting state and a user context and shows which transitions would be enabled. Reuses the T018 evaluator directly.
   - A small read-only state diagram is rendered below the editor using a simple DOT→SVG generator. One box per state, one arrow per transition. No layout engine; arrange states in a horizontal row and accept the result. Good enough for a 5-to-10-state diagram.
   - Visual workflow editor is T019-d.

8. **Credit tab — JSON editor with validation**:
   - Same approach as the Workflow tab: validated JSON editor with inline errors and a "Test against sample activity" button that runs the T018 credit applier on a dummy activity built from the current form schema.
   - Errors must include "this directive references field `foo` which does not exist in the form schema" — the validator cross-checks against the form tab's current state.
   - Visual credit editor is T019-e.

9. **Schema change warnings on publish**:
   - Before committing a draft, diff the draft schema against the published schema.
   - Warn on: field removed, field type changed, field key changed, required flag added to an existing field, section removed.
   - Warnings are informational — the admin can publish anyway because old activities are pinned to their original schema version and not affected. The warning exists to stop accidents, not to block upgrades.

10. **"Preview as trainee"**:
    - Renders the draft schema in the runtime `ActivityForm.razor` with fresh empty data, in a full-width panel. Useful for "does the whole form make sense in context".

11. **Activity type metadata tab** (call it a small fourth tab, or put it above the three editor tabs — pick what fits):
    - `Key` (immutable once published), `Name`, `Description`, `Scope` (global / institution / speciality / sub-speciality), `IsActive` toggle, `OwnerUserId` (read-only), creation/version history.
    - `display_fields` editor — pick up to three field keys from the form schema to show as columns in `MyActivities` and the admin list. Default: the first two fields in the first section.

### Runtime renderer (everyone)

12. **Components** under `Wombat.Web/Components/Shared/Activities/`:
    - `ActivityForm.razor` — takes a `FormSchema` and a `Data` dictionary, renders the form. Handles all ten field types. Emits `OnDataChanged` events. Honours `show_if`. Shows inline validation from the schema validator. **One component, three callers**: builder preview, trainee submission, admin read-only detail.
    - `ActivityFieldRenderer.razor` — internal; dispatches to a per-type sub-component (`TextFieldRenderer`, `LikertFieldRenderer`, `ProcedureRefRenderer`, etc.). One component per field type keeps the dispatch legible.
    - `ActivityDetail.razor` — read-only variant of `ActivityForm.razor` for viewing submitted activities against their pinned schema version.
    - `ActivityWorkflowActions.razor` — given the T018 evaluator's available-transitions result, renders the buttons available to the current user (Submit, Accept, Decline, Complete, Cancel, …). Clicking dispatches a `TransitionActivityCommand`. Note field shown inline when the selected transition has `requires_note`.

13. **Pages** under `Wombat.Web/Components/Pages/Activities/`:
    - `NewActivity.razor` — pick activity type from a scope-filtered list, pick subject (defaults to current user if Trainee), render the form, save draft or submit.
    - `MyActivities.razor` — list the current user's activities across all types. Columns adapt to the type's `display_fields`.
    - `ActivityInbox.razor` — what needs the current user's attention. Queries `ListActivitiesByActorInboxQuery` from T018.
    - `ActivityView.razor` — detail + workflow actions.

14. **Real-time preview invariant**: the builder's preview panel and the trainee's submission page render via the exact same `ActivityForm.razor` component. Any visible difference between them is a bug. Write at least one test that asserts this by rendering the same schema+data in both contexts and comparing the resulting DOM.

## Files created

- `src/Wombat.Web/Components/Pages/Admin/ActivityTypes/ActivityTypesList.razor`
- `src/Wombat.Web/Components/Pages/Admin/ActivityTypes/ActivityTypeEdit.razor`
- `src/Wombat.Web/Components/Pages/Admin/ActivityTypes/FormTab.razor`
- `src/Wombat.Web/Components/Pages/Admin/ActivityTypes/WorkflowTab.razor`
- `src/Wombat.Web/Components/Pages/Admin/ActivityTypes/CreditTab.razor`
- `src/Wombat.Web/Components/Pages/Admin/ActivityTypes/MetadataTab.razor`
- `src/Wombat.Web/Components/Pages/Admin/ActivityTypes/FieldEditor.razor` (inline panel, dispatches to per-type editors)
- `src/Wombat.Web/Components/Pages/Admin/ActivityTypes/FieldEditors/**` (one per field type)
- `src/Wombat.Web/Components/Pages/Admin/ActivityTypes/StateDiagram.razor`
- `src/Wombat.Web/Components/Shared/Activities/ActivityForm.razor`
- `src/Wombat.Web/Components/Shared/Activities/ActivityDetail.razor`
- `src/Wombat.Web/Components/Shared/Activities/ActivityFieldRenderer.razor`
- `src/Wombat.Web/Components/Shared/Activities/FieldRenderers/**` (one per field type)
- `src/Wombat.Web/Components/Shared/Activities/ActivityWorkflowActions.razor`
- `src/Wombat.Web/Components/Pages/Activities/NewActivity.razor`
- `src/Wombat.Web/Components/Pages/Activities/MyActivities.razor`
- `src/Wombat.Web/Components/Pages/Activities/ActivityInbox.razor`
- `src/Wombat.Web/Components/Pages/Activities/ActivityView.razor`
- `src/Wombat.Infrastructure/Persistence/Migrations/*ActivityTypeStaging.cs` — adds `Staging*` columns to `ActivityType`.
- `tests/Wombat.Web.Tests/Activities/BuilderPreviewParityTests.cs`
- `tests/Wombat.Web.Tests/Activities/RuntimeRendererTests.cs`
- `tests/Wombat.Web.Tests/Activities/DraftPublishLifecycleTests.cs`

## Verification

- [ ] `dotnet build` clean.
- [ ] `dotnet test` green, including:
  - Builder preview / runtime render parity test.
  - Draft/publish lifecycle: editing a published type creates a draft; publish bumps version; existing activities keep their pinned version.
  - Validation: publishing a draft with an unreachable workflow state is rejected; publishing with a credit rule referencing a missing field is rejected.
- [ ] **Design-system compliance greps (must return zero matches):**
  - `grep -R '<style' src/Wombat.Web/Components/Pages/Admin/ActivityTypes src/Wombat.Web/Components/Shared/Activities` — no inline `<style>` blocks in builder or renderer files.
  - `grep -R 'class="table"' src/Wombat.Web/Components/Pages/Admin/ActivityTypes src/Wombat.Web/Components/Shared/Activities` — no primitive Bootstrap tables; everything is `.clinic-table`.
  - `grep -R 'btn-outline-primary\|btn-outline-secondary\|btn-outline-danger' src/Wombat.Web/Components/Pages/Admin/ActivityTypes src/Wombat.Web/Components/Shared/Activities` — Wombat uses `.btn-outline`, not the Bootstrap variants.
  - `grep -R '<i class="bi bi-' src/Wombat.Web/Components/Pages/Admin/ActivityTypes src/Wombat.Web/Components/Shared/Activities` — no Bootstrap Icons (the font isn't loaded — use the shared `Icon` component).
  - `grep -REn '#[0-9a-fA-F]{3,6}' src/Wombat.Web/Components/Pages/Admin/ActivityTypes src/Wombat.Web/Components/Shared/Activities` — no raw hex colors; everything routes through CSS custom properties.
- [ ] Manual walkthrough — **the T019 acceptance script**:
  1. Log in as an Administrator. Go to Admin → Activity Types → New.
  2. Create a type with Key `hello_world`, Name `Hello World`, global scope.
  3. On the Form tab, add a section `Greeting`, add a `text` field `message` (label "Your message", required).
  4. On the Workflow tab, paste a workflow with `draft → submitted` and one transition `submit` from draft to submitted, actor `subject`, requires field `message`.
  5. Credit tab: empty rules.
  6. Publish. Confirm version 1 exists and the type appears in the list as Active.
  7. Log out, log in as a Trainee. Go to Activities → New. Pick `Hello World`. Fill in the message. Click Submit. Confirm the activity lands in `MyActivities` as `submitted`.
  8. Log in as Administrator. Open the type again. Click Edit. Add a required `longtext` field `details`. Observe the incompatible-change warning. Publish anyway.
  9. Trainee: the old activity is still visible as-is (pinned to v1, no `details` field). A new activity shows the new field.
- [ ] Performance: a form with 30 fields and 5 sections renders in the builder preview in under 100ms after each edit (Blazor Interactive Server measured via the server's component render time; this is easy to meet but check once because drag-drop-free reactive re-renders should fly).
- [ ] Field key locking: a published field's key cannot be edited. Attempting to edit it in the draft editor is prevented in the UI and in the publisher.

## Notes & gotchas

- **The ten field types are the contract for v1.** New field types need a new task; do not add an eleventh "just while I'm here". Every field type is a renderer, a builder editor, a validator, a JSON serialization, and a PDF renderer in T023.
- **Up/down buttons, not drag-drop.** Drag-drop is T019-b. The time you save by skipping SortableJS and touch-event handling is the time you spend on the ten field editors instead.
- **Monaco is optional.** A plain `<textarea class="font-mono">` plus live validation covers the v1 need for the Workflow and Credit tabs. Monaco via Blazor interop has real pain points with Interactive Server (disposal, event re-registration on circuit reconnect). Ship with textarea. Upgrade later.
- **Schema change diffing** is a small, pure function over two `FormSchema` instances. Write it carefully and unit test it exhaustively — every real-world schema edit will run through it.
- **Field key locking** is the most important invariant in the whole builder. Once a field key is published, it is referenced by (a) stored activity data in jsonb, (b) credit rules, (c) workflow transition `required_fields`, (d) `display_fields`, (e) `show_if` conditions. Renaming it silently would corrupt all of the above. The UI must lock the input, and the publisher must refuse to publish a draft whose published-before field keys have changed. Deletion is allowed with the warning — renames are not.
- **No raw JSON in the Form tab.** The Form tab is visual-only. A developer-only "Advanced: view JSON" toggle is fine for debugging, behind the Administrator role check. Admins do not see it by default.
- **Live preview must not save.** It renders an in-memory draft only. Reload-safe — the draft persists in `StagingSchemaJson` after Save draft, not after every keystroke.
- **One generic renderer, many callers.** Resist the urge to fork `ActivityForm.razor` for any reason. Special behaviours belong in the schema, not in the component tree.
- **`display_fields` default fallback**: first two fields of the first section, if the admin doesn't set them. The list view must still render something useful on a type that nobody has customized.
- **Accessibility**: label-for associations, ARIA required indicators, keyboard navigation for the section-and-field reorder buttons. This is cheaper to do correctly in v1 than to retrofit.

## Deferred to follow-up tasks (explicitly out of scope for T019)

- **T019-b — Drag-drop reordering.** Sections and fields. Cross-section moves. SortableJS interop or equivalent. Up/down buttons remain as a keyboard-accessible fallback.
- **T019-c — Nested sections and repeatable sections.** PDSA cycles, structured logbook entries, anything where a group of fields recurs N times per activity. Requires schema shape changes, renderer changes, and credit-rule grammar changes.
- **T019-d — Visual workflow editor.** Replaces the JSON editor with a drag-and-connect state machine designer. Requires the engine to stay unchanged.
- **T019-e — Visual credit-rules editor.** Replaces the JSON editor with a guided form: "count this activity once toward the CurriculumItem matching field X when field Y is at level Z or above".
- **T019-f — Multi-condition visibility.** AND/OR combinations, expression grouping, "show when any of" / "show when all of".
- **T019-g — Schema templates, copy-from-existing, and advanced signature capture.** Start a new type from an existing one. Library of institution templates. Signature canvas if anyone actually asks for it.

Write each follow-up as its own task file under `Tasks/` when its time comes. Do not expand T019 in place.
