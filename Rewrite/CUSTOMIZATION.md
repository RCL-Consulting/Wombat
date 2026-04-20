# No-code customization — the Activity platform

This document exists because the rewrite plan initially drifted away from a core Wombat intent: that admins should be able to add new activity types (research outputs, teaching sessions, quality improvement projects, audits, course attendance, journal club, anything) without involving a developer. The original Wombat's `Option`/`OptionSet`/`OptionCriterion`/`OptionCriterionResponse` abstraction was reaching for a generic form builder. The first draft of the rewrite replaced that with hardcoded aggregates per activity type (`Assessment`, `StarReflection`, etc.), which is the opposite of what's wanted.

This document defines the schema-driven model that puts the customization back. It supersedes the "one aggregate per feature" assumption that `DOMAIN.md` and T007–T009 were built on.

## What "no-code customizable" actually means

It does **not** mean every part of the system can be changed by an admin. It does mean:

- Admins can define new **activity types** (Mini-CEX, DOPS, Research Publication, Teaching Session, QI Project, Audit, Morbidity & Mortality discussion, External Course — whatever).
- For each activity type, admins define the **form schema**: which fields, what types, validation, help text, layout.
- Admins define the **workflow**: who creates, who approves, what states exist, who can move between them.
- Admins define how the activity contributes to **curriculum progress**: counts toward a CurriculumItem, evidences an EPA at a minimum level, etc.
- Admins do **not** change: identity, role definitions, scope claims, audit log format, or legally sensitive workflows (MSF anonymity, committee decisions, data subject rights).

The dividing line: if a feature could be confused with "code", it stays in code. If it's a form and a workflow on top of a form, it's data.

## The Activity model

Four entities replace most of the per-feature aggregates:

### `ActivityType`

Admin-defined catalogue entry.

- `Id`
- `Key` — stable short code (`mini_cex`, `dops`, `reflective_note`, `research_output`). Immutable once used.
- `Name`, `Description`
- `Scope` — global / institution / speciality / sub-speciality. Determines who sees it in their pickers.
- `FormSchema` — jsonb. The form definition (see below).
- `Workflow` — jsonb. The state machine definition (see below).
- `CreditRules` — jsonb. How a completed activity contributes to curriculum progress.
- `Version` — integer, bumped on every schema change. Old activities keep their original schema version so historical data stays readable.
- `IsActive`
- `OwnerUserId` — who created this type, for audit.

### `Activity`

One instance of an activity type. This is what trainees, assessors, and admins actually create.

- `Id`
- `ActivityTypeId`, `SchemaVersion` — pinned at creation; a later schema change does not retroactively invalidate old activities.
- `SubjectUserId` — the trainee the activity is about.
- `CreatedByUserId` — the person who initiated it.
- `CurrentState` — one of the states defined in the workflow.
- `Data` — jsonb. All field values. Keyed by field key from the schema.
- `CreatedOn`, `UpdatedOn` — UTC.
- Optional `EpaId`, `CurriculumItemId` when the activity type requires linking to an EPA or curriculum.

### `ActivityTransition`

An audit of every state change on an activity.

- `Id`
- `ActivityId`
- `FromState`, `ToState`
- `TransitionKey` — the command name from the workflow (`submit`, `accept`, `decline`, `complete`).
- `ActorUserId`
- `OccurredOn` — UTC.
- `Note` — free text.
- `Snapshot` — jsonb. The Data field at the moment of the transition. Enables "show me what the form looked like when they submitted" queries.

### `ActivityPermissionRule`

How the system decides who can do what on which activities. Stored as data, not code.

- `Id`
- `ActivityTypeId`
- `TransitionKey` — which transition this rule governs.
- `ActorRule` — which users may perform it (`subject`, `role:Assessor`, `role:SpecialityAdmin`, `scope:same_speciality`, combinations).
- `FieldRequirement` — which schema fields must be non-empty to perform this transition.

## Schema format

Form schemas are JSON documents with a stable shape. An admin edits them through the Activity Builder UI; the JSON is the underlying storage. A minimal schema:

```json
{
  "version": 1,
  "sections": [
    {
      "key": "context",
      "title": "Clinical context",
      "fields": [
        { "key": "setting", "type": "choice", "label": "Setting", "options": ["Ward", "Clinic", "ED", "Theatre"], "required": true },
        { "key": "presenting_complaint", "type": "text", "label": "Presenting complaint", "required": true },
        { "key": "case_complexity", "type": "choice", "label": "Complexity", "options": ["Low", "Medium", "High"], "required": true }
      ]
    },
    {
      "key": "rating",
      "title": "Assessment",
      "fields": [
        { "key": "history", "type": "scale", "label": "History taking", "scale_key": "or_scale", "required": true },
        { "key": "exam", "type": "scale", "label": "Physical examination", "scale_key": "or_scale", "required": true },
        { "key": "reasoning", "type": "scale", "label": "Clinical reasoning", "scale_key": "or_scale", "required": true }
      ]
    },
    {
      "key": "feedback",
      "title": "Feedback",
      "fields": [
        { "key": "strengths", "type": "longtext", "label": "What went well", "required": true },
        { "key": "improvements", "type": "longtext", "label": "What to improve", "required": true },
        { "key": "plan", "type": "longtext", "label": "Plan for next time", "required": false }
      ]
    }
  ]
}
```

Supported field types in v1 (exactly ten — the T019 contract):

- `text` — short single-line.
- `longtext` — multi-line.
- `number` — numeric with min / max / step / integer-only / unit suffix.
- `date` — date picker.
- `choice` — single-select (dropdown or radio).
- `multichoice` — checkbox group with min/max selections.
- `likert` — rating scale referencing an `EntrustmentScale` or a named scale defined in the schema.
- `procedure_ref` — picker tied to the procedure catalogue seeded in T020.
- `file` — upload with mime allowlist and size limit; up to 5 files.
- `signature` — captures the submitter's name, role, and UTC timestamp at submit time.

New field types are new tasks, not T019 drive-bys. Every field type is a renderer, a builder editor, a validator, a JSON serialization, and a PDF renderer in T023 — the marginal cost is real.

Conditional visibility (`show_if`) is supported on sections and fields as a **single** condition per element in v1: one field, one operator (`equals` / `not_equals` / `is_set` / `is_not_set` / `greater_than` / `less_than`), one value. Multi-condition visibility with ANDs/ORs is T019-f. Covers roughly 90% of the real cases — "show site when procedure = central line", "show escalation note when complication = yes".

Sections are one level deep in v1. No nested sections. No repeatable sections (for PDSA cycles in T020, use three pre-numbered sub-sections; for structured logbooks, wait for T019-c). Validation rules beyond `required` live inside the field type options (`max`, `min`, `regex`, `length`) rather than as a separate expression language.

## Workflow format

A workflow is a state machine defined as data. Minimum viable shape:

```json
{
  "version": 1,
  "initial_state": "requested",
  "states": [
    { "key": "requested", "label": "Requested" },
    { "key": "accepted", "label": "Accepted" },
    { "key": "declined", "label": "Declined", "terminal": true },
    { "key": "cancelled", "label": "Cancelled", "terminal": true },
    { "key": "completed", "label": "Completed", "terminal": true }
  ],
  "transitions": [
    { "key": "accept",   "from": "requested", "to": "accepted",  "actor": "role:Assessor+subject_assessor" },
    { "key": "decline",  "from": "requested", "to": "declined",  "actor": "role:Assessor+subject_assessor", "requires_note": true },
    { "key": "cancel",   "from": ["requested","accepted"], "to": "cancelled", "actor": "subject_or_assessor" },
    { "key": "complete", "from": "accepted",  "to": "completed", "actor": "role:Assessor+subject_assessor", "requires_fields": ["history","exam","reasoning","strengths","improvements"] }
  ]
}
```

Workflows can also be simpler — a Research Output might just be `draft → submitted → approved` with the subject trainee submitting and a SpecialityAdmin approving. Each activity type picks its own shape.

## Credit rules

How a completed activity contributes toward curriculum progress is also data:

```json
{
  "counts_for": [
    { "curriculum_item_match": { "epa_field": "epa_id" }, "amount": 1, "minimum_level_field": "reasoning" }
  ]
}
```

That reads as: "when this activity is completed, it counts once toward the CurriculumItem matching the EPA chosen in the `epa_id` field, provided the `reasoning` field is at or above the CurriculumItem's minimum level." Different activity types have different credit rules; research outputs might not count toward EPAs at all but toward a separate "research portfolio" requirement.

## Rendering

One generic Blazor component tree takes a schema and an `Activity` and produces a form. One generic list view takes an `ActivityType` and a query, returns a table. One generic detail view renders the activity's data against its pinned schema version.

This means the Blazor code for "a Mini-CEX form" and "a research output form" is the *same* code. The only per-type code is the small amount of workflow-specific UI (buttons, state badges), which is also driven by the schema.

A trainee creating a new activity: picker of `ActivityType` → dynamic form from its schema → submit → landed in workflow initial state. No per-type Blazor pages.

An admin defining a new activity type: a visual form builder → a JSON-validated workflow editor → a JSON-validated credit-rules editor → publish. No developer involvement. See "Builder scope — v1 and beyond" below for what's visual and what isn't.

## Builder scope — v1 and beyond

The old Wombat shipped a builder that could limp through real institutional use. The replacement needs to do at least as well from day one, but deliberately not chase polish that would never ship. T019 builds a visual form editor plus JSON-validated editors for workflow and credit rules; T019-b through T019-g are staged follow-ups that layer in the nice-to-haves once real schemas exist to inform the UX.

**In T019 (v1):**

- Visual form editor with a section list, a field list per section, an inline field-edit panel, and a live preview.
- Section reorder via up/down buttons. Field reorder via up/down buttons within a section. Cross-section field moves via delete + add.
- Ten field types (see above).
- One-condition `show_if` per field/section.
- JSON editor with inline validation for the Workflow tab. A small state diagram rendered from the JSON for orientation. "Test transition" button that runs the engine against a sample actor context.
- JSON editor with inline validation for the Credit tab. "Test against sample activity" button.
- Draft/publish lifecycle. Published versions immutable. Existing activities pinned to the version they were created under. Incompatible-change warnings on publish.
- `display_fields` picker in the metadata tab.
- One generic runtime renderer shared between the builder preview, the trainee submission form, and the admin detail view.

**In T019-b (drag-drop):**

- SortableJS-style drag-and-drop reordering for sections and fields. Cross-section drag moves. Up/down buttons remain as the keyboard-accessible fallback.

**In T019-c (nested & repeatable sections):**

- Sections inside sections (one additional level — no tree).
- Repeatable sections for PDSA cycles, structured logbook entries, signed off-service rotations. Schema shape changes; credit-rule grammar extended to address fields inside repeatable groups.

**In T019-d (visual workflow editor):**

- Drag-and-connect state machine designer. Same underlying workflow JSON; this is a UI over it.
- State node properties in a side panel.
- Transition arrows with inline actor-rule editing.
- The T018 workflow engine does not change.

**In T019-e (visual credit-rules editor):**

- Guided form: "count this activity once toward the CurriculumItem matching field X when field Y is at or above value Z". Generates the same credit-rules JSON T019 writes by hand.

**In T019-f (multi-condition visibility):**

- AND/OR combinations and expression grouping for `show_if`. Backwards-compatible — single-condition rules from v1 still parse.

**In T019-g (schema templates and copy):**

- "Start from an existing schema" — duplicate a published type into a new draft.
- Institution-scoped template library.
- Advanced signature capture (canvas, touch, timestamped) if anyone actually asks.

The staging is deliberate. Each follow-up ships as an independent task, each measurable, each low-risk relative to the core engine that T017 and T018 establish. The builder gets better every release; it does not block phase-1 launch.

## What stays in code (and why)

Not everything survives the pivot to schema-driven. These stay hardcoded because the cost of making them data-driven exceeds the benefit:

1. **Identity, roles, and scope claims.** These are the foundation everything else checks against. Making them data means "who can edit the data that decides who can edit" — recursive and dangerous.
2. **Multi-source feedback.** Anonymity requires specific implementation: respondent tokens, aggregated views that don't allow de-anonymisation by counting, carefully audited viewing. Cannot be expressed as "a form with a workflow". Response links and respondent emails are short-lived operational data; after campaign close and aggregation, the implementation hashes the respondent email and nulls the raw address so the released report cannot be traced back to a person through the application data model.
3. **Committee decisions.** These are legally consequential (appeals, regulatory scrutiny) and their shape is determined by external rules, not the institution. They need dedicated domain code with strict invariants.
4. **Audit log.** The audit log must be append-only, tamper-evident, and cannot itself be editable by admins. Making it customizable would defeat its purpose. Important interactions with other hardcoded features:
   - **Data-subject deletion (T026) must not delete audit entries.** An audit entry records that an *action occurred*; the subject's identity is a foreign key but the record of the action is legally and operationally independent. POPIA/GDPR allows retention of audit data for accountability purposes even after a deletion request. T026's deletion path must null or pseudonymise the `ActorDisplayName` field but leave the entry itself intact.
   - **Retention window is configurable per deployment.** The `AuditLogRetentionJob` uses a 2-year active window (entries older than 2 years are moved to `AuditEntryArchives`). An institution whose governing body specifies a different active period can adjust this window via configuration; the 7-year total retention (2 active + 5 archive) before cold-storage export is the default, not a hard limit. See `INFRASTRUCTURE.md` for the full lifecycle table.
5. **Data subject rights.** POPIA/GDPR compliance is a regulatory obligation; it must be predictable and testable. Hardcoded.
6. **Institutional SSO.** Protocol-level code.
7. **Notification primitives.** The queue, worker, templates live in code. *Which* events trigger notifications can be data (part of the workflow definition).

A useful mental model: **the platform is code; the content is data.** Identity/roles/audit/committee decisions are platform. Activity forms and workflows are content.

## Querying the jsonb store

Postgres `jsonb` is the right storage for `Activity.Data`. Indexes:

- GIN index on `Data` for containment queries (`WHERE Data @> '{"history": 4}'`).
- Expression indexes on specific frequently-queried paths (`Data->>'epa_id'`).
- Strongly-typed columns for the few fields every activity has: `SubjectUserId`, `ActivityTypeId`, `CurrentState`, `CreatedOn`. Don't put those in jsonb.

Query patterns:

- "All activities for trainee X of type Y" — indexed.
- "All activities of type Y with field `reasoning` at or above 4" — GIN-indexed.
- "Curriculum progress for trainee X" — walks the trainee's CurriculumItems and queries Activities by type + EPA field + minimum level; fast because it's bounded by the trainee's curriculum.

Reports that aggregate across all activities in an institution will be the slow case. Acceptable for now; accelerate with materialised views if needed.

## Versioning and migration

An `ActivityType` can be edited. When it is, its `Version` increments. New activities use the new version. Old activities keep their pinned version and can still be read, edited (within their workflow rules), and displayed — the renderer takes the activity's `SchemaVersion` and fetches that version of the schema.

If a schema change is incompatible with existing data (field removed, field type changed), the admin UI warns and requires a migration step: either leave old activities alone (safe default) or run a small transformation (a tiny DSL) over the old data.

No automatic schema rewrites on existing activities. Ever. Old data is sacred.

## Trade-offs, honestly

**What this buys you**

- A new activity type is an admin task, not a developer task.
- Regulatory or institutional changes ("add a reflection on patient safety to every Mini-CEX") are a schema edit.
- The same platform serves WBAs, reflections, research, teaching, QI, journal club, course attendance, and anything new the RCP decides to require next year.
- The Wombat rewrite matches the original Wombat's intent.

**What it costs you**

- Type safety drops for the activity data. The compiler does not know what fields exist; tests have to cover shape.
- Queries over activity data are slightly slower than over typed columns, and considerably slower if you neglect indexes.
- Debugging "why didn't this transition fire" is harder when the answer is in a JSON document somewhere.
- The admin UI for building schemas and workflows is itself a non-trivial amount of work. That's T017–T019.
- "No-code" for the admin still means "learn the form builder, understand schema versioning, understand credit rules". It is dramatically less work than a PR, but it is not zero work.
- The v1 builder uses JSON editors for the workflow and credit tabs. Admins who build those tabs will see and edit JSON. The editors validate live and surface errors inline, so this is tractable for anyone willing to follow a short guide — but it is the area the follow-up tasks target for improvement first (T019-d, T019-e).

**The 80/20 line**

Roughly 80% of the things admins want to add — forms with workflows — fit cleanly into the platform. The other 20% (anonymous MSF, committee decisions, novel authentication, audit) will always be developer work. Make peace with that ratio; don't try to build a platform that covers 100%, because that's how you end up writing Salesforce instead of Wombat.

## Relationship to the original plan

The following original tasks change shape:

- **T007 (Assessment aggregate)** becomes **seed Mini-CEX, DOPS, CbD, ACAT as activity types** on top of the platform. The state machine is expressed in each activity type's workflow JSON, not in C# methods on an aggregate.
- **T008 (Assessment workflow commands & UI)** becomes **generic activity commands & generic activity renderer**. One set of handlers for all activity types. One set of Blazor components.
- **T009 (STAR reflection)** becomes **seed STAR reflection as an activity type** with a schema that has four longtext fields.
- **T011 (Role dashboards)** still builds per-role dashboards, but the dashboard widgets query activities generically ("pending activities where I'm the assessor", "my recent completed activities") rather than per-type.
- **T013 (Architecture tests)** adds tests that the activity platform enforces its invariants: every activity type has a valid schema, every transition references valid states, every activity's `Data` conforms to its pinned schema version.

The tasks that do not change shape and remain required:

- T001–T006 (scaffold, identity, org hierarchy, curriculum structure, invitation flow, profiles)
- T010 (web layout & auth)
- T012 (email infrastructure)
- T014 (seeding)
- T015 (Linode deployment)
- T016 (smoke test & handover)

The new tasks T017–T027 introduce the platform, the specific hardcoded workflows that cannot be data, and the real-world features from the previous evaluation.

See `PLAN.md` for the re-sequenced list.
