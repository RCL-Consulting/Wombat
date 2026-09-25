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

Three entities replace most of the per-feature aggregates:

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
- `WbaToolKey` — which College-named instrument the type *is* (`mini_cex`, `dops`, `cbd`, `msf`, …), a key into the
  `WbaTools` vocabulary, or null for "not a recognised instrument" (T122). **Live and unversioned**, like `Name` and
  `Scope`: not part of the four versioned payloads, not bumped by `Version`, never touched by the seed refresher. The
  builder writes it on save, not on publish, and discarding a draft does not undo it. On a seeded type, that save
  parks a draft, and the seed refresher skips a type with a draft in flight until it is published or discarded. See
  "Which instruments may credit an EPA" below.
- `SystemManaged` — only the system writes this type's activities (T162). `msf_cpsa` is the one such type: a released
  MSF campaign writes one row per EPA it covers (`ActivityService.StageCompletedAsync`). A system-managed type is **not
  on the type picker** (`ListActivityTypesQuery`) and **a hand-made create is refused** (`ActivityService.CreateDraftAsync`,
  before anything is written); the system's own path does not read the flag. It is owned by the seed catalogue, like
  `WbaToolKey`: declared on the `ActivityTypeSeedEntry` (required, so every seed says), written on create and never
  refreshed, and stamped on existing databases once by the T162 migration. So a catalogue change to a type an existing
  database already holds reaches it only through a **new** migration (a new seed needs none: it is created flagged);
  until then the seeders warn at startup and never repair. T162's migration and its key list are frozen. The builder
  neither shows nor writes it, so a builder-made type is never system-managed and a builder save keeps the flag a type
  already has.

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

### Who may do what

There is no permission table. Who may take a transition, who may write a field and who may be named in a field are
actor rules (`subject`, `creator`, `role:`, `scope:`, `field:`) declared in the type's own workflow and schema JSON, and
enforced by one matcher (`ActorRuleMatcher`); see "Field ownership" below. The `ActivityPermissionRule` entity planned
here was mapped but never read, and T162 dropped its table. Whether a person may create the type at all is the one
flag above, `SystemManaged`.

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
        { "key": "history", "type": "scale", "label": "History taking", "scale_key": "seed:demo:scale:o-r", "required": true },
        { "key": "exam", "type": "scale", "label": "Physical examination", "scale_key": "seed:demo:scale:o-r", "required": true },
        { "key": "reasoning", "type": "scale", "label": "Clinical reasoning", "scale_key": "seed:demo:scale:o-r", "required": true }
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

Types added since v1 include `epa`, `scale`, `rating`, `markdown`, `datetime`, `checkbox` and `user`. The one with
rules of its own is **`user`**, which names a person (T102). It offers, and the server accepts, only active users at the
activity's institution who hold its `role` (default `Assessor`; or InstitutionalAdmin, Coordinator, CommitteeMember or
Trainee — the roles whose authority is not bounded by a speciality), and never the subject. Only the institution is
matched, never the nominee's speciality: an assessor from another discipline at the same institution can be named (D23). It takes no `options` or `catalogue`: its people come from the directory. See
rule 3 under field permissions.

**`file` is not wired**: it renders a placeholder and nothing stores an upload. Until attachments are their own task, a
document is a `text` field holding a link, held to `http(s)` by
`"validation": { "regex": "^(?i:https?)://\\S+\\z" }` (D34; `clinical_audit_cpsa.report_link`, T154). The
regex refuses a note in place of a link and a `javascript:` URL. It anchors with `\z`, not `$`: the validator is .NET's
`Regex.IsMatch`, where `$` also matches before a final newline. The link is shown as text, not as a clickable link.

**Root pointers** name, at the schema's top level, which field plays a role the platform reads without knowing the
form:

- `observed_on_field`: the encounter date (T119).
- `rated_level_field`: the entrustment rating (T126).
- `evidence_epa_field`: the EPA this activity is evidence for (T137). It must name an `epa`-typed field.

`evidence_epa_field` is what stamps `Activity.EpaId`, at create, on every transition, and on the system-written MSF
path, by `EvidenceEpaResolver`. The lists and the committee read that stamp.

So the list and credit can never name different EPAs, publish enforces exactly one of two shapes
(`EvidenceEpa.EnsureCreditAgrees`, run by `SaveDraft` and `PublishDraft` before anything is assigned):

- **one EPA:** the pointer is declared, every credit directive's `epa_field` equals it, and no directive targets a
  curriculum item directly (`curriculum_item_id` or `curriculum_item_field`); or
- **no single EPA:** no pointer, and no directive reads an `epa_field`. Crediting a fixed item or an item field still
  works (journal club).

Every pointer needs both a Parse half and a Serialize half, plus a `SeedRoundTripTests` fixture (CLAUDE.md § Editing a
seed folder).

**`scale_key`** binds a `scale` field to the entrustment ladder it is rated on (T109, T253). It is one string in one of
three forms, told apart by the string alone (`ScaleBinding`, Domain):

- `seed:` and the scale's `SeedKey`: what every seed writes (`seed:cpsa:scale:v11.1`, `seed:demo:scale:o-r`). No
  command writes a seed key, so an administrator's rename of the ladder unbinds nothing. `SeedScaleKeyTests` holds
  every seed to this form and to a key a seeder stamps.
- digits alone: the scale's id, what the activity-type builder writes.
- anything else: the scale's exact name. No seed and no builder writes one, and the T253 migration rewrote every stored
  one (in published versions, each type's current copy and its draft) to the same scale's seed key, or its id when it
  has none. A name that bound no scale was left as it was. So only a hand-made schema can still bind by name.

A scale's name may be neither of the first two forms: the create and update validators refuse digits alone and a
`seed:` prefix, so a key binds at most one scale. Every reader goes through one resolver,
`EntrustmentScaleBindings.ResolveAsync`: `CreditApplier`, the rung picker, the rung labels on every page and the PDF,
and the type picker's ladder filter. A key that binds nothing is read as "no ladder": credit compares bare ordinals, and
the form offers the field's own options, or asks for a bare number. Three rules keep a published version from binding
nothing:

- The scale's **delete** is refused while any published version, current or not, binds it in any form, naming each type
  and its versions: a published version never changes and an activity stays on its version, so no later publish can
  take a binding back. The refusal says to leave the scale in place, not to rename it, because a rename unbinds a form
  that binds by name.
- The activity-type **publish** is refused while a field of the draft has a `scale_key` that binds no scale, naming each
  field (`EntrustmentScaleBindings.ThrowIfAFieldBindsNoScaleAsync`). A draft may bind a scale deleted since it was
  saved, because the delete asks only about published versions. A field with no `scale_key` is not asked about. The
  seeders publish without it; `SeedScaleKeyTests` holds the seeds to keys a seeder stamps.
- The scale's **rename** refuses nothing, and warns, naming each type, when a published version binds the old name.
  The save asks every check before it assigns anything (the audit trap), so a refused save commits no rename.

A delete and a publish that race are not serialised: each checks, then writes, with no lock between them.

The references the database does hold, a sub-speciality's default, a curriculum item's pin, a scored progress row and a
decision on one of the scale's levels, are ON DELETE RESTRICT, so a delete that races one of them is refused by the
foreign key rather than committed. The delete then asks its checks again on a fresh read and throws the first that now
applies, in the words it would have used before the save; a scale another delete removed first is "not found". The
checks ask about the levels stored now, so a level another save added after the load is asked about too. A level
another save removed after the load makes EF refuse the whole save as a concurrency conflict; with no check applying,
that is reported as the scale having changed, and a second attempt reads it afresh. The database's refusal stays
underneath, so the audit pipeline discards the refused removal (T201). Any other refusal no check explains, and any
refusal whose read-back itself fails, is rethrown as the database gave it, never replaced by the read-back's exception,
which would not carry it (T254).

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
    { "key": "accepted", "label": "Accepted", "editable_by": "field:assessor_user_id" },
    { "key": "declined", "label": "Declined" },
    { "key": "cancelled", "label": "Cancelled" },
    { "key": "completed", "label": "Completed", "terminal": true }
  ],
  "transitions": [
    { "key": "accept",   "from": "requested", "to": "accepted",  "actor": "field:assessor_user_id", "validation": "draft" },
    { "key": "decline",  "from": "requested", "to": "declined",  "actor": "field:assessor_user_id", "requires_note": true, "validation": "draft" },
    { "key": "cancel",   "from": ["requested","accepted"], "to": "cancelled", "actor": "subject|field:assessor_user_id", "validation": "draft" },
    { "key": "complete", "from": "accepted",  "to": "completed", "actor": "field:assessor_user_id", "validation": "all" }
  ]
}
```

Workflows can also be simpler — a Research Output might just be `draft → submitted → approved` with the subject trainee submitting and a SpecialityAdmin approving. Each activity type picks its own shape.

### Keys are for logic, labels are for people (T189, T220)

A state's `label` is the only name a person sees for it, read from the activity's **pinned** version: a submitted
`clinical_audit_cpsa` is "Awaiting supervisor" on its page, in the lists, on the dashboards, in a committee's evidence
snapshot (frozen there as `CommitteeEvidence.SourceStateLabel`) and in the portfolio PDF, as it is in every refusal and
notice. A transition declares no label; it is named from its key in words (`WorkflowTransition.LabelFor`: `sign_off` is
"Sign Off"), on its button, in a refusal and in the history. The history's create row, which no workflow declares, is
"Create". The DTOs carry both (`CurrentState` and `CurrentStateLabel`, and each history row's `FromStateLabel`,
`ToStateLabel` and `TransitionLabel`); `PinnedWorkflows` resolves them. The stored key is shown only where the pinned
version cannot name it: a state or move it does not declare, or a type with no workflow that parses. A key never reaches
a page otherwise, except as a badge's colour class. The subject-access report's JSON keeps the stored key, as it keeps
`DataJson` raw.

### `validation` — how much of the form a move insists on (T105)

Every transition checks formats. Which `required` fields count is the transition's `validation`:

| Value | Counts | Use it for |
|---|---|---|
| `all` (default) | every visible `required` field in the schema | a move into a terminal state: completion must find the whole form |
| `owned` | only the `required` fields the mover may write in the state they are leaving | a hand-on: the trainee's submit must not wait for fields only the assessor can write |
| `draft` | none | a way out or back: cancel, decline, recall. A half-filled draft must be disposable |

`requires_fields` adds fields under every value; it means "additionally required for this step", never "the only
place requiredness lives". So the schema says what is mandatory — the CPSA seeds mark the assessor's rating and three
feedback fields `required` — and the workflow says when each move checks. The default is the strict value, so a
transition that declares nothing is checked more, never less; the serialiser always writes the value out. Before T105
every transition validated the whole schema, which made cancel and decline unusable on a half-filled form and forced
the seeds to hide the assessor's fields from the validator.

### `terminal` means "credit fires here"

Only mark a state terminal if reaching it should **count**. `CreditApplier` runs on any transition into
a terminal state, and an abandoned request still carries a filled-in `epa_id` — so a terminal `declined`
or `cancelled` awards curriculum credit for an assessment that was refused or withdrawn. Dead-end states
that are not achievements simply declare no outgoing transitions; that is enough to end the activity.
`SeedParseTests.NoSeededWorkflow_MarksAnAbandonmentStateTerminal` enforces this across every seed.

## Field ownership — `editable_by` (T070)

Who may *take* a transition is `actor`. Who may *write* a given field is `editable_by`, and it is declared
in three places, all optional:

```json
// workflow.json — the state gate
{ "key": "requested", "label": "Requested", "editable_by": "field:assessor_user_id" }
```

```json
// schema.json — the section gate, with a per-field override
{
  "key": "assessment",
  "title": "Entrustment",
  "editable_by": "field:assessor_user_id",
  "fields": [
    { "key": "overall_level", "type": "scale", "label": "Overall level" },
    { "key": "trainee_comment", "type": "longtext", "label": "Trainee's reflection", "editable_by": "subject" }
  ]
}
```

Both take the same actor grammar as `actor` (`subject`, `creator`, `role:X`, `scope:X`,
`field:<key>`, combined with `|` for any and `+` for all).

**The effective rule is a conjunction:** the actor must satisfy the **state** rule *and* the field's own
rule, where a field falls back to its section's rule. Field beats section beats the default.

**The default is `subject|creator`**, which reproduces the pre-T070 `CanEditDraft` test exactly — so every
`ActivityTypeVersion` already published keeps its existing behaviour without a republish. Declaring nothing
changes nothing.

Four rules worth knowing before you author a type:

1. **A terminal state, or any state with no outgoing transitions, is writable by nobody.** A transition is
   the only save channel, so a form nobody can submit is a form that loses work.
2. **At creation the state gate is ignored** (`procedure_log` and `journal_club` have a terminal initial
   state and would otherwise be uncreatable), and the field rules are evaluated against **empty** data. A
   `field:` rule reads its answer out of `DataJson`; evaluating it against what the caller just submitted
   would let the caller name themself and unlock the fields the rule protects.
3. **A nominee field may name only an eligible person (T102).** A nominee field is every `user` field plus every
   field a `field:` rule names. Its value must be the exact id of a user who holds every role the field requires
   (`role`, default `Assessor`), belongs to the activity's stamped institution (the subject's, never the caller's;
   there is no Administrator bypass), is not deactivated (an administrator's lock or an erasure; a brute-force lockout
   does not count), and is not the activity's subject. A **changed** value is judged on every write, whoever makes it,
   including a withdrawal. An **unchanged** one is judged only when the author hands the activity on while still able
   to change it — the D20 clause the EPA→tool gate uses (T122) — so an assessor's own completion is never refused
   because they have since lost the role. The picker lists exactly the accepted set (`NomineeDirectory`). Enforced in
   `ActivityService` by `ThrowIfActorFieldNamesSubject` (the subject case, with its own message) and `NomineeGate`.
4. **Save and publish refuse an ambiguous nominee** (`ActorFieldRules.EnsurePublishable`): a section or field key
   declared twice, a `field:` rule naming no field or a field that is not `user`, and `options` or a `catalogue` on a
   `user` field. The check is not in the parser, so a stored version that predates it still loads.

**There is no hard-coded assessor-note field.** Assessor narrative is an ordinary schema field the admin
marks assessor-owned (`strengths` / `improvements` / `plan` on the CPSA seeds). That is distinct from
`ActivityTransition.Note`, which is a workflow annotation: it is what `requires_note` demands, it is shown
in the activity history, and it is invisible to credit and to reports.

**The builder has no editor for `editable_by` yet.** It round-trips through the visual builder unharmed. A state's
`editable_by` can be written in the Workflow tab's JSON; a section's or field's cannot be written by an operator at
all today, because the Form tab has no raw editor, so it comes only from a seed.

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

### Which instruments may credit an EPA (T122)

A credit rule says *how* an activity credits; the curriculum item says *which instruments may*. Each
`CurriculumItem.PermittedToolsJson` is a sorted JSON array of `WbaTools` keys (Annexure A's tools column for the CPSA
catalogue), or null for "any instrument". It is checked against the activity type's `WbaToolKey`:

- **Where:** on the write path, never at credit (D20), and per credit directive. Every target is checked at create. A
  changed target (a draft update or a transition patch that alters it) is checked wherever credit can still follow. An
  unchanged target is re-checked only when the author hands it on while still able to correct it: the mover is the
  subject or the creator, nobody else has acted yet, the mover can write that directive's field in the current state,
  and the move hands it on (credit can follow without coming back through where it started, `Workflow.CanReachTerminal`,
  or the mover loses write access to the field). A move from which credit cannot be reached (`cancel`, `decline` into
  a state nothing leaves) is never checked. So an assessor's completion (even one allowed to correct the EPA), a sign-off
  after assessment and a resubmission after a decline are never refused for an unchanged target. `CreditApplier` and
  the rebuild never read the list, so editing one never takes back credit already earned.
- **The picker agrees:** the EPA picker drops EPAs whose item forbids this type's instrument, using the same
  predicate (`ToolPermission.Evaluate`). If a keyed instrument may credit none of the subject's EPAs, the picker falls
  back to the unrestricted creditable set rather than emptying, and the write path's refusal names the instruments
  the curriculum accepts.
- **An unrated instrument is gated by its evidence EPA (T154):** a type that credits nothing (`"counts_for": []`) is
  judged on the field its schema names as `evidence_epa_field`, as if that were an `epa_field` directive
  (`ToolPermissionGate.GatedTargets`). The reflective exercise, the clinical audit and the portfolio review credit
  nothing (D7), but each is stamped as evidence for its EPA (T137) and the committee reads that, and a list names the
  instruments that are evidence for its EPA. So each is held to the lists by the same predicate, at the same moments
  (create, a changed target, the author's hand-on) and with the same message as a rated tool, and its picker narrows
  that field (`CreditRuleFields.ResolveNarrowedEpaFieldKeys`). A type that credits is judged on its directives alone:
  one crediting by EPA must credit through the pointer (`EvidenceEpa.EnsureCreditAgrees`), so the two are one target.
  Only an instrument is narrowed: a type with no key (a reflective note) keeps the claims filter (D21). The MSF
  release writes `msf_cpsa` rows through the staged path, which is not gated: a release covers every EPA the
  campaign declared (T121), and every v11.1 list names MSF anyway.
- **Permissive by default (D21):** a type with no key, an item with no list (or an unparseable one), a subject with no
  trainee profile and an EPA with no item are all unrestricted. An institution's own types are therefore unrestricted
  until someone picks an instrument for them in the builder ("This tool is").
- **One resolver:** the gate asks `CreditTargetResolver`, the credit engine's own item resolution, so it can never
  refuse or pass a different item from the one credit lands on. It asks for every item a target names, in force or not
  (D48, T196): a deactivated EPA's credit is paused, not cancelled, so its item is where the credit lands once the EPA
  is reactivated. Not being in force is never itself a refusal.
- **Seeds:** the catalogue stamps lists and seeded types' keys on CREATE only. An existing database got them from the
  T122 migration; afterwards, a difference from the catalogue is logged as a startup warning, never written.

### A deactivated EPA pauses credit (T158, T196, D48)

Deactivating an EPA takes its curriculum items out of force: the pickers stop offering it and no progress page lists it
as a target (`CurriculumItemsInForce.InForce`). It does not refuse filing, and it does not cancel credit:

- **Credit is judged at the moment of credit**, the time of the transition credit is recorded against (the newest, the
  one `CreditApplier` builds its dedupe key from). An item is in force at that moment when its EPA is active, or the
  moment falls before `Epa.DeactivatedOn` (`CurriculumItemsInForce.InForceAt`, `Epa.InForceAt`). A completion while the
  EPA is inactive credits nothing and is stamped `CreditedItemCount = 0`, so the T108 warning explains it.
- **The rebuild judges each completion at its own moment** (`CreditSubject.Of`), so a rebuild while an EPA is inactive
  keeps the credit earned while it was in force and credits nothing completed since. It is the completion's moment, not
  the encounter's, because that is what the live path judged: a rebuild must reproduce it. Only the pause is judged as
  of that moment: which items the curriculum holds, their targets and their scale pins are today's, which is what makes
  the rebuild the repair after a curriculum edit.
- **Reactivating credits what was filed during the pause** (`UpdateEpaCommandHandler`, `ResumedEpaCredit`), for whoever
  may reactivate the EPA (a CollegeAdmin for a national EPA, the owning InstitutionalAdmin for a local one), with no
  Administrator rebuild. It credits the reactivated EPA's items only, adds to each completion's stamp, and writes what a
  rebuild would. Every read happens before the first mutation. It reads only completions that could credit: pinned to a
  version whose rules declare `counts_for`, in a state such a version ends in, and moved since the pause began
  (`ResumedEpaCredit.LoadCandidatesAsync`). A unique-index refusal at its save is told apart by reading back: a duplicate
  code says so, and anything else is a progress row another save opened first ("Save again").
- **A completion and its EPA's deactivation or reactivation are serialised on the EPA's row** (T230, `IEpaCreditLock`,
  `EpaCreditLock`), so each race ends with the credit a rebuild would write. The live completion
  (`ActivityService.TransitionAsync`) holds the EPAs its credit judges `FOR SHARE`, from before the plan reads whether
  they are in force until its save commits (`CreditTargetResolver.EpasJudgedAsync` names them: every item the
  directives match, in force or not). `DeactivateEpaCommandHandler` and `UpdateEpaCommandHandler` hold the EPA
  `FOR NO KEY UPDATE` before they read it, take the deactivation's moment under it, and commit with it. Whichever comes
  second waits and then runs on what the first committed. A reactivation that waited for a completion finds it among its
  candidates; a completion that waited for a reactivation reads the EPA as active; a deactivation that waited takes a
  moment after the completion's; a completion that waited for a deactivation reads the pause, and judges its own moment
  against it (a moment taken before the deactivation's still credits). Completions never wait for one another (shared
  locks). An optimistic token on the EPA row was rejected: a completion would have had to write the row, so every two
  completions on one national EPA would have conflicted. Not `FOR UPDATE` either: it also conflicts with the
  `FOR KEY SHARE` a foreign-key check takes, so every insert naming the EPA (a curriculum item, an entrustment decision,
  a pending one, an MSF campaign's EPA) would wait while a reactivation plans its paused completions. The hold is a
  `READ COMMITTED` transaction on the request's own context, and does nothing on any provider but PostgreSQL.
  `EpaCreditRacePostgresTests` drives each race from two connections.
- **A wait is bounded by the command timeout** (Npgsql's 30 seconds; nothing in Wombat sets another, nor a
  `lock_timeout`). A hold that gives up refuses the request with a busy message (`EpaCreditLock.CreditBusy` on a
  completion, `ChangeBusy` on the EPA's edit page) instead of Npgsql's "Exception while reading from stream". Nothing
  was changed, because every caller takes its hold before its first mutation, so the audit pipeline's catch commits only
  its own row. A server `lock_timeout` or `statement_timeout`, if an operator sets one, is refused the same way.
- **What the hold does not cover.** The Administrator rebuild takes no hold: one racing a reactivation meets it on the
  progress rows' `xmin` token or unique index, and one of the two is refused ("run again" / "Save again"). An item
  added to the curriculum between `EpasJudgedAsync` and the plan is judged without a hold; a curriculum edit already
  calls for a rebuild. A deactivation or reactivation waits for every completion on its EPA that is in flight when it
  arrives, and longer if more keep arriving, up to the command timeout above. And the ordering rests on one clock: a
  completion takes its moment before it holds the EPA and a deactivation after, so two application servers would need
  clocks that agree to within a request.
- **One timestamp is the whole history.** Every earlier pause was closed by a reactivation that credited it, so only the
  current pause holds anything back. Deactivating an EPA that is already inactive keeps the moment its pause began.
  `CK_Epas_DeactivatedOn` keeps `IsActive` and `DeactivatedOn` in step; change them through `Epa.Deactivate` and
  `Epa.Reactivate`, and only under `IEpaCreditLock.HoldForChangeAsync`, taken before the EPA is read and committed with
  the save, with the deactivation's moment read once it is held (`EpaPauseWritePathTests` fails a caller that takes no
  hold). `IsActive` is init-only, so a stored EPA's flag cannot be set any other way.
- **An activity already filed against it keeps its EPA**, shown as "(no longer in use)" in its option list
  (`EpaOptionLabel`), because the pickers offer only EPAs in force. My activities and the Inbox mark it the same way
  (T231): `ActivitySummaryDto.EpaInForce` carries the flag the picker reads (`Epa.IsActive`, judged now), and
  `ActivityEpaLabel` prints `EpaOptionLabel.NoLongerInUse` after the title, muted. So do the rating trajectory headings
  on My progress and the committee review page (`EpaTrajectoryDto.EpaInForce`), which still chart the EPA's ratings,
  and the admin's entrustment decisions list and My authorisations (`EntrustmentDecisionDto.EpaInForce`, filled by the
  shared `ToDto`), whose STARs deactivation leaves standing (T255). Each prints it through the shared `EpaLabel`. The
  entrustment standing panel needs no mark: it reads only items in force (`.InForce()`).

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
2. **Multi-source feedback.** Anonymity requires specific implementation: respondent tokens, aggregated views that don't allow de-anonymisation by counting, carefully audited viewing. Cannot be expressed as "a form with a workflow". Response links and respondent emails are short-lived operational data; on close, withdrawal or auto-close the implementation nulls the raw address and keeps nothing derived from it (T207 dropped the unsalted hash), so the released report cannot be traced back to a person through the application data model. A respondent is never named on an audit row (T205). Whoever runs a campaign sees its invitees' addresses only while it is a draft, listed so that one added by mistake can be removed (T247): a draft's invitees hold no working link and have given no response, so the list says nothing about anyone's answers. "Whoever runs it" is any coordinator at the trainee's institution and any Administrator, not only the one who typed the addresses; that is accepted, since each of them may add to the campaign, open it and read its report, but it means a second coordinator also knows who a one-person group is (T247 review). Removing one is refused once the campaign has opened, and its audit row names the invitation by id, never by address. From the open on, the campaign page counts invitees by respondent group only (T217).
3. **Committee decisions.** These are legally consequential (appeals, regulatory scrutiny) and their shape is determined by external rules, not the institution. They need dedicated domain code with strict invariants.
4. **Audit log.** The audit log must be append-only, tamper-evident, and cannot itself be editable by admins. Making it customizable would defeat its purpose. Important interactions with other hardcoded features:
   - **Data-subject deletion (T026) must not delete audit entries.** An audit entry records that an *action occurred*; the subject's identity is a foreign key but the record of the action is legally and operationally independent. POPIA/GDPR allows retention of audit data for accountability purposes even after a deletion request. T026's deletion path must null or pseudonymise the `ActorDisplayName` field but leave the entry itself intact.
   - **Retention window is configurable per deployment.** The `AuditLogRetentionJob` uses a 2-year active window (entries older than 2 years are moved to `AuditEntryArchives`). An institution whose governing body specifies a different active period can adjust this window via configuration; the 7-year total retention (2 active + 5 archive) before cold-storage export is the default, not a hard limit. See `INFRASTRUCTURE.md` for the full lifecycle table.
5. **Data subject rights.** POPIA/GDPR compliance is a regulatory obligation; it must be predictable and testable. Hardcoded.
   - **Erasure ends what is open about the person and keeps what is settled (T258).** `ErasureExecutor` moves every
     reference to the person to a pseudonym no account holds, and then:
     - **Profiles.** Every trainee profile moves, not only the first. One still active is deactivated with the erasure day
       as its last day (`TraineeProfile.Erase`: `DeactivatedOn`, `IsActive` false; D49 reads it). One that had already
       ended keeps its recorded end. Every assessor profile moves too. An assessor profile has no active state: it is a
       directory entry, and the assessor list drops one whose account is gone.
     - **Committee reviews.** A review still open (Scheduled, InProgress or Decided: `CommitteeReview.OpenStates`) is
       withdrawn with a recorded reason (`CommitteeReview.Withdraw`, state `Withdrawn`, `WithdrawnOn` and
       `WithdrawalReason`, which the review page shows, the day on the South African calendar). The decisions staged at it
       are removed, since only ratifying issues one. Its snapshot, agenda and any unratified decision stay as the record
       of what happened. The pages read it as withdrawn: its list outcome is "None: the review was withdrawn" (formative
       too), a decision it recorded is said never to have been ratified, nothing is said to be coming or staged, and the
       MSF notices beside its snapshot say nothing, since nothing more is weighed at it.
     - **Multi-source feedback.** Every campaign about the person not yet released is withdrawn (`MsfCampaign.Withdraw`),
       which anonymises its invitations as any withdrawal does: every state `Withdraw` accepts, which is all but Released
       and Withdrawn (Draft, Open, UnderReview, and Closed, which nothing produces today). So no invitee is added, and no
       release writes activities about someone who has left. Under review is included: its report could only be released
       to the person. A campaign withdrawn by an erasure records no reason, as no campaign withdrawal does.
     - **Kept, under the pseudonym.** Ratified reviews, and a review under appeal (its decision is ratified; the appeal
       body can still answer the appeal, which reads no evidence and issues no STAR). Issued STARs, released feedback
       reports, activities, progress and audit entries.
   - **No committee review refuses an erasure request (T258 review).** T026 refused a request while a review of the
     requester was Scheduled, InProgress, Decided or UnderAppeal, because an erasure then left the review running under
     the pseudonym. Approval now ends the open ones and keeps the one under appeal answerable, so the refusal only
     contradicted it: a request made before a review was scheduled withdrew it at approval, while one made a day after
     was refused for the same review. Whether the person may be erased is the approver's decision, taken on the request.
   - **All of it or none of it.** The erasure runs in one transaction, and a failure leaves nothing tracked. The audit
     pipeline saves the same context from its catch, so anything still tracked would otherwise commit without the rest,
     along with the request's approval. The request's approval and its completion are written by the erasure's own save,
     inside the transaction (the approving handler marks both before the erasure runs), so no erasure stands under a
     request left merely Approved. A refused erasure leaves the request as it was, ready to approve again. A row about the
     person that changed while the erasure ran (a campaign closed by the auto-close job, a review ratified) fails the save
     on its concurrency token, and the approver is told so in words (`ErasureExecutor.PersonChanged`), not EF's.
   - **Not ended by an erasure, yet:** an activity about the person still in its workflow runs on under the pseudonym and
     can still be completed and credited; a user id held in an activity's `DataJson` (a nominee field) is not rewritten;
     the person's address as a respondent on someone else's open campaign stays until that campaign closes; and a review,
     campaign or profile created about the person while the erasure runs (it reads under READ COMMITTED and locks
     nothing) lands under the original id. Approving an erasure asks for no confirmation and does not list what it will
     withdraw.
   - T238 keeps a pseudonym out of every new scope. T258 ends what was already open when the erasure ran.
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
- "Curriculum progress for trainee X" — reads the trainee's `CurriculumItemProgress` rows, one per (item, semester), which `CreditApplier` writes at each terminal transition, and reads them against each item's per-window target through one shared read model (`QuotaProgressCalculator`, T130). It never re-queries Activities: the tally is the materialisation, and `RebuildCurriculumProgressCommand` is the one thing that recomputes it from the activities. So a row also carries what a reader needs about its last encounter: the date (`LastObservedOn`) and whether anybody stated it (`LastObservedOnDeclared`, T219), because an undated activity's date is only the day its form was created and My progress marks it as such (`EncounterDate.Label`). On a tie the date is stated if either encounter stated it, so a rebuild reproduces the flag whatever order it replays in. Which windows hold a target is `QuotaWindow.For`, judged with both ends of the programme: the start (D14, D42) and the actual end, `TraineeProfile.EndedOn` (completion or deactivation, never the expected completion date). A window the programme ended in before its last month is exempt, and one after the end is outside the programme (D49, T209). The end is recorded when the profile ends, so it is never after today on the South African calendar (a later day would hold the trainee to every window up to an end that has not happened), never before the start, and the start may not later move past it. Credit itself is unchanged: it lands in the encounter's semester whatever the trainee's dates, and only the target is waived.

Reports that aggregate across all activities in an institution will be the slow case. Acceptable for now; accelerate with materialised views if needed.

## Versioning and migration

An `ActivityType` can be edited. When it is, its `Version` increments. New activities use the new version. Old activities keep their pinned version and can still be read, edited (within their workflow rules), and displayed — the renderer takes the activity's `SchemaVersion` and fetches that version of the schema.

If a schema change is incompatible with existing data (field removed, field type changed), the admin UI warns and requires a migration step: either leave old activities alone (safe default) or run a small transformation (a tiny DSL) over the old data.

No automatic schema rewrites on existing activities. Ever. Old data is sacred.

**One recorded exception while nothing is live (W-007).** T137's migration (`20260924123852_T137_ActivityEvidenceEpa`)
added `evidence_epa_field` to stored schemas, published versions included, wherever the version's own credit rules or
its only `epa` field decide it. It then stamped `Activities.EpaId` from the data. The pointer is inert: nothing
rendered, validated or credited changes. Without it, an in-flight activity pinned to a pre-T137 version would have its
backfilled `EpaId` reset to null on its next transition. When Wombat takes on real users this exception closes with
W-007: a later pointer of this kind needs a resolver fallback for old versions, not a rewrite.

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
