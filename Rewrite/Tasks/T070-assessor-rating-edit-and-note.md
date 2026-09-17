# T070 — No assessor rating-edit / assessor-note surface

**Status:** DONE (2026-09-17, Opus) — code + tests, build clean, 563 tests green. Not yet exercised in a
browser; see "What is still not proven" below.
**Originally filed:** 2026-05-29, during the Act 3 play-through.
**Re-scoped:** 2026-09-17, after verifying the whole path against the source. It is larger than filed.
**Model:** Opus — it adds a DSL property, a permission engine and a new editable surface across five layers.

## Symptom

The assessor cannot enter a rating **at all**. Not "cannot adjust one" — there is no path by which
assessment data ever reaches an activity after creation.

An assessor opens a submitted WBA, sees a **Complete** button, clicks it, and gets a red alert reading
`overall_level: A value is required; strengths: A value is required; improvements: …`. There is no form.
The only data an activity will ever hold is what the **creator** typed at creation.

`decline` is the only assessor transition that is satisfiable end to end today.

## Correction to the original filing

The task text named a **`Rated`** state. No such state exists. The CPSA workflows run
`draft -> requested -> completed` (plus dead-end `declined` / `cancelled`); the legacy seeds run
`requested -> accepted -> completed`. **The gate is data, not a state name** — the bound assessor is
whoever `DataJson["assessor_user_id"]` names, per `actor: "field:assessor_user_id"`. Everything below is
written against that.

## Verified ground truth

Established by reading the source and querying the dev database, 2026-09-17.

### The Web layer never sends a data patch on a transition

- `TransitionActivityCommand.cs:14` already declares `string? DataPatchJson = null`, and
  `ActivityService.cs:129-131` already merges it. **The backend is not the gap.**
- `ActivityView.razor:81-87` dispatches `(_activity.Id, request.TransitionKey, userId, authState.User, null, request.Note)`
  — a literal `null` in the 5th (patch) position. This is the assessor's only path.
- `NewActivity.razor:130-136` is the **one** Web dispatch that does pass a patch, because that page just
  collected the form.
- `ActivityDetail.razor:1` is a one-line component: `<ActivityForm … ReadOnly="true" />`. A string literal,
  no branch, no parameter to override it. Every field renders `disabled` and `ActivityForm.UpdateValue`
  hard-returns (`ActivityForm.razor:225-228`).
- `ActivityWorkflowActions.razor:35` emits only `(TransitionKey, Note)`, and collects the note only when
  `requires_note` — which for the CPSA tools is true for `decline` and **false** for `complete`.
- There is no `/activities/{id}/edit` route. The four activity routes are `inbox`, `mine`, `new`, `{id:int}`.

### `ActivityForm` needs no work to be editable

`ActivityForm.razor:117-121` already exposes `SchemaJson`, `DataJson`, `ReadOnly` and
`EventCallback<string> DataJsonChanged` — so `@bind-DataJson` works. Two callers already edit through it:
`NewActivity.razor:40` and the builder preview at `ActivityTypeEdit.razor:230`. It only lacks a caller on
the detail page.

### `requires_fields` is a validation list, not an ownership list

This is the finding that kills the obvious narrow fix.

- `WorkflowEvaluator` **never reads** `RequiresFields` (`WorkflowEvaluator.cs:13-37`). Its only consumer is
  `ActivityService.cs:137`, which hands it to `SchemaValidator` as `additionallyRequiredFieldKeys`.
- `SchemaValidator.cs:58-59`: `isRequired = (mode == Submit && field.Required) || additionallyRequiredFieldKeys.Contains(field.Key)`.
  It **widens** the already-full Submit pass; it never scopes it.

So "the assessor may edit what `complete` requires" only works on the four `_cpsa` tools, which T098 phase
2a happened to author that way. On the legacy `mini_cex`, `schema.json:19-24` marks **14 of 14** fields
`required: true` including all six assessment scales, while `complete` lists four — the assessor would get
4 of 10 assessor-owned fields and `complete` would still throw. And in `requested` the assessor's only
transition (`accept`) declares no `requires_fields` at all, so the editable set would be **empty**.

### No field-level write authorization exists anywhere

`MergeJsonObjects` (`ActivityService.cs:207-227`) is a shallow top-level merge: the patch overwrites any key
it names. Nothing restricts an assessor's patch to their own fields, so a `complete` patch can silently
rewrite `epa_id`, `assessor_user_id` or `observed_on` and **redirect which curriculum item gets credited**
(`CreditApplier.cs:144-150`). Opening the patch channel without a write gate creates that escalation.

### Nothing stops the trainee filling the assessor's fields either

`NewActivity.razor:40` renders the whole schema — Request, Entrustment and Feedback alike — to the creator,
and `CreateDraftAsync` validates in **Draft** mode only. A trainee can self-rate at creation, at which point
`complete` is already satisfiable and the assessor's click changes nothing. The schema gives the UI nothing
to key a decision off: `FormSchemaParser.cs:184` whitelists exactly
`key, type, label, help_text, required, options, catalogue, scale_key, validation, show_if` — there is no
role or ownership concept in the field DSL at all.

### Ordering constraints that fix the design

- **Authorization runs before the merge.** `ActivityService.cs:118` evaluates the actor rule against the
  *pre-patch* `DataJson`; the merge is at `:129-131`. A patch cannot authorise its own transition. Good —
  keep it that way.
- **Credit fires after the merge**, only on a terminal target state (`:141-151`), reading the post-merge
  data (`CreditApplier.cs:37`). So a rating supplied in the patch *does* reach credit through the existing
  path — no credit changes are needed.
- `CreditApplier` does not hard-code `overall_level`; it reads `minimum_level_field` from the pinned
  version's credit rules (`CreditApplier.cs:171-175`). The seeds name it.

### There is no assessor-note field, and that is fine

No `note` / `assessor_note` / `comment` field exists in any of the four CPSA schemas. The assessor's
narrative surface is the three unrequired longtext fields in the `feedback` section — `strengths`,
`improvements`, `plan`. The separate `ActivityTransition.Note` is a **workflow annotation**: it is mandatory
on `decline`, invisible to credit and to every report, and **nothing in Web renders it today**, so a decline
note is currently write-only.

Keep the two distinct. Do not add a hard-coded assessor-note field — assessor narrative is a schema field
the admin marks assessor-owned.

## Approach: declare ownership in the schema, don't infer it

Add an `editable_by` actor rule to the schema section, the schema field, and the workflow state. The
effective writable set is the conjunction: *the state allows this actor to write* **and** *the field (or its
section) allows this actor to write*. Defaults reproduce today's behaviour exactly, so every
already-published `ActivityTypeVersion` keeps working without a republish.

This is the model-correct choice rather than the cheap one, and the reason is the platform premise in
`CLAUDE.md`: institutions add activity types **without developer involvement**. A code-side gate would work
for the four seeded tools and silently mis-scope every admin-built one, including each of the ten v11.1
tools still to be seeded.

## Implementation plan

### 1. Domain DSL — `editable_by` on section, field and state

Add `ActorRule? EditableBy` as the **last** positional member of `FormSection`, `FormField` and
`WorkflowState`. (`FormSection` is `(Key, Title, ShowIf, Fields)` — it must go after `Fields`, or
`FormSchemaParser.cs:176` and `BuilderModels.cs:53` stop compiling.)

Whitelists to extend: `FormSchemaParser.ParseSection` (`:159`), `ParseField` (`:184`),
`WorkflowParser.ParseState` (`:177`). Parse with `ActorRuleParser.Parse`, wrapping `WorkflowParseException`
in `SchemaParseException` on the schema side.

> **The one silent failure mode.** Emit it in **both** `Serialize` bodies — `FormSchemaParser.Serialize`
> (section block ~`:36`, field block ~`:115`, beside the existing `show_if` writes) and
> `WorkflowParser.Serialize`'s state loop (~`:36`, beside `terminal`). `ActivityType.SaveDraft`
> (`ActivityType.cs:51-52`) round-trips Parse+Serialize, so anything `Serialize` omits is **dropped at
> publish with no error**: the JSON parses, the builder shows the setting, and the feature quietly dies.
> **Write the round-trip test before the enforcement code.**

Construction sites to fix: `FormSchemaParser.cs:176,187`; `WorkflowParser.cs:180`; `BuilderModels.cs:53,57`;
`WorkflowEvaluatorTests.cs:81`.

Do **not** add a `nobody` token or an `editable_in` state list in this task — both are purely additive later
(a third optional property that ANDs in), so nothing built here gets rewritten.

### 2. Extract `ActorRuleMatcher`, add `IFieldPermissionEvaluator`

Lift `WorkflowEvaluator.cs:39-107` (`EvaluateRule`, `IsInActivityScope`, `HasNameIdentifier`,
`GetStringFieldValue`) verbatim into an internal static `ActorRuleMatcher.Matches(ActorRule, Activity, ClaimsPrincipal)`
in `Wombat.Infrastructure.Activities`. It needs `Wombat.Application.Common.Extensions`
(`GetInstitutionId` / `IsInSpeciality` / `IsInSubSpeciality`, used at `WorkflowEvaluator.cs:67-73`), which
is why it cannot live in Domain. `WorkflowEvaluator.EvaluateRule` becomes a one-line delegation — no
behaviour change, so `WorkflowEvaluatorTests.cs:64-75` must pass **untouched**. That is the whole acceptance
criterion for this step.

Add to Application:
`IFieldPermissionEvaluator.GetWritableFieldKeys(FormSchema, Workflow, Activity, ClaimsPrincipal, bool ignoreStateGate = false)`.
`IWorkflowEvaluator` already takes Domain `Workflow` + `Activity` from the same folder, so no architecture
test is at risk.

Rules:
- terminal state => empty (unless `ignoreStateGate`);
- state rule = `state.EditableBy ?? ActorRuleParser.Parse("subject|creator")`;
- field rule = `field.EditableBy ?? section.EditableBy ?? default`;
- effective = state ∧ field.

`ignoreStateGate: true` is **only** for `CreateDraftAsync` — `procedure_log` and `journal_club` have a
terminal initial state and would otherwise be uncreatable.

Register `AddScoped<IFieldPermissionEvaluator, FieldPermissionEvaluator>()` at `DependencyInjection.cs:103`.

### 3. `ActivityService` — create-time filter, permission-aware merge, `GetDetailAsync`

Fifth ctor dependency `IFieldPermissionEvaluator` (ripples to `ActivityHandlersTests.cs:29`).

**(a) `CreateDraftAsync` (`:31`)** — after parsing and normalising, drop every key outside the writable set
(`ignoreStateGate: true`) **before** the Draft-mode validate at `:46`. This is what closes "the trainee
pre-fills the assessor's rating".

**(b) Replace `MergeJsonObjects` (`:207`) with `MergeWritableKeys(current, patch, writable)`** — writable
keys apply; a non-writable key is **ignored when `JsonElement.DeepEquals`-equal** to the stored value and
otherwise throws
`InvalidOperationException($"Field '{key}' cannot be written in state '{state}' by the current actor.")`.
The DeepEquals carve-out is load-bearing: full-form post-backs echo unchanged locked fields, and without it
the rule is a landmine. (Confirm `JsonElement.DeepEquals` — .NET 9+; this repo is `net10.0`.)

**(c) `TransitionAsync`** — compute the writable set **after** the evaluator gate at `:118` and **before**
the merge at `:129`, against the **pre-transition** state. Everything downstream (Submit-mode validate with
`transition.RequiresFields` at `:133-137`, `ApplyTransition`, terminal `CreditApplier`) is untouched, so the
assessor's `overall_level` reaches credit through the existing path.

**(d) Add `GetDetailAsync(int, ClaimsPrincipal, CancellationToken)`** returning
`ActivityDetailDto(ActivityDto Activity, IReadOnlyList<string> EditableFieldKeys, IReadOnlyList<ActivityActionDto> AvailableActions)`,
with the actions evaluated against the **real** loaded `ActivityType` (`LoadActivityAsync` already Includes
it at `:163`) — which is what finally makes `scope:` rules agree between the button list and the server.
Leave `ActivityDto` alone; the inbox and list paths stay principal-free.

### 4. MediatR surface + audit redaction

`GetActivityByIdQuery` becomes `(int ActivityId, ClaimsPrincipal Principal) : IRequest<ActivityDetailDto>`
calling `GetDetailAsync`. `ActivityView.razor:54` is its only consumer, so the return-type change is
contained.

> Add an explicit code comment that the principal is used **only** to compute the writable set and that this
> query still performs **no read authorization** — see T101. Otherwise the next maintainer reads the
> `ClaimsPrincipal` as a gate that isn't there.

Add `[property: Redact]` to `DataPatchJson` and `Note` on `TransitionActivityCommand` (`:14-15`).
`AuditPipelineBehavior.cs:76-77` audits every type whose name ends in `Command` and `AuditPayloadSerializer`
writes its properties into `SummaryJson`; from this task onward that patch carries the assessor's
`strengths` / `improvements` / `plan` narrative on **every** completion. `RedactAttribute` already exists at
`Wombat.Application/Audit/RedactAttribute.cs`. No validator change needed —
`TransitionActivityCommandValidator` exists, so `NamingTests.cs:64-92` stays green.

### 5. `ActivityForm` — per-field locking

Add `[Parameter] public IReadOnlySet<string>? EditableFieldKeys { get; set; }` beside the existing four
(`:118-121`) and
`private bool IsFieldLocked(string key) => ReadOnly || (EditableFieldKeys is not null && !EditableFieldKeys.Contains(key));`

Replace `disabled="@ReadOnly"` at the **eight** sites — lines 40, 43, 48, 51, 56, 68, 78, 91 — with
`disabled="@IsFieldLocked(field.Key)"`. Swap the `if (ReadOnly) return;` guards in `UpdateValue` (`:225`)
and `ToggleMultiChoice` (`:244`) for `if (IsFieldLocked(key)) return;` — **the C# guard, not the `disabled`
attribute, is what stops a forged event.**

Null keeps today's semantics exactly, so `ActivityDetail.razor`, `ActivityTypeEdit.razor:230`,
`RuntimeRendererTests` and `BuilderPreviewParityTests` are unaffected.

> Do **not** touch `OnParametersSetAsync` — the `_loadedSchemaJson` ordinal guard at `:140-145` exists to
> stop concurrent `DbContext` use on rapid edits and is regression-tested at `RuntimeRendererTests.cs:107-136`.

### 6. `ActivityWorkflowActions` becomes presentational

Replace `[Parameter] ActivityDto Activity` (`:34`) with
`[Parameter, EditorRequired] IReadOnlyList<ActivityActionDto> Actions`; delete `@inject IWorkflowEvaluator`
(`:7`), the `@using Wombat.Domain.Activities` / `.Workflow` (`:4-5`) and the whole `OnParametersSetAsync`
block (`:41-63`) including the `new Activity { ActivityType = new ActivityType() }` candidate at `:49` —
that synthetic type defaults `Scope` to `Global` and `ScopeId` to null, so every `scope:` rule evaluates
false in the UI while the server may allow it.

Two further wins: it stops re-parsing the workflow and re-evaluating every transition on every parent render
(which, once the parent re-renders per keystroke, would fire on each character), and it removes this file's
direct Domain-entity construction, which `CLAUDE.md` forbids and no architecture test catches.

Keep `ToLabel` (`:94-99`), the `.form-actions` button row and the two-step `requires_note` panel
(`:20-30`, `:65-86`) exactly as they are. Keep the callback as `(string TransitionKey, string? Note)` — the
data patch rides on the page, not here. `ActivityView.razor:27` is the only caller.

### 7. `ActivityView` — the editable surface, the patch, save, and a history card

Hold `ActivityDetailDto _detail`, `string _workingDataJson`, `IReadOnlySet<string> _editable`.

When `_editable.Count == 0`, keep the existing read-only `<ActivityDetail …/>`. Otherwise render an
`<Alert Kind="info">` explaining what the actor may fill in, then
`<ActivityForm SchemaJson=… DataJson="@_workingDataJson" EditableFieldKeys="_editable" DataJsonChanged="OnDataChanged" />`
plus a `<FormActions>` row with `.btn .btn-outline` Discard and `.btn .btn-primary` Save.

Build the patch with `ActivityDataPatch.Build(storedJson, workingJson, writableKeys)` in a **plain .cs
file** — a diff emitting only writable keys whose value differs. Unit-testable without bUnit, and it is what
keeps step 3's reject-on-unowned-change rule from firing on untouched fields.

Pass that patch into the 5th positional slot of `TransitionActivityCommand` where the literal `null` sits at
`:86`, so **Complete carries the rating atomically with the state change** and `CreditApplier` grades the
assessor's value. Re-issue `GetActivityByIdQuery` after every save and every transition so the writable set
and the action list refresh.

Add a third `<section class="detail-card">` history card over `_detail.Activity.Transitions` (key,
from -> to, actor, `OccurredOn`, `Note`). `ActivityTransitionDto` already carries all of it, and nothing in
Web renders a transition note today — so a decline note is currently write-only.

> `DataTable` is `@typeparam TItem` with `[Parameter, EditorRequired] RenderFragment HeaderRow` and
> `RenderFragment<TItem> Row`; `<DataTable Items="…">` alone will not compile.
> `.page-subtitle` (`app.css:110`), `.details-grid` (`:756`) and `.detail-card` (`:499`) all exist.

### 8. `NewActivity` — lock assessor fields at creation

Pass `EditableFieldKeys` into the `ActivityForm` at `:40` so a trainee creating a Mini-CEX sees Request
editable and Entrustment/Feedback locked.

Cheapest slice: keep `GetActivityTypeEditorQuery` and compute the writable set from the same schema. Fuller
option: a `GetActivityCreationContextQuery` returning server-resolved schema + writable set +
`InitialTransitionKey`, which also deletes the second `new Activity` / `new ActivityType()` at `:163-170`
and the `@inject IWorkflowEvaluator`.

Change `:135` from `_dataJson` to `null` with a comment that `CreateActivityCommand` already persisted it
(`ActivityService.cs:45-57` normalises and stores that exact string). Under step 3's merge it is a
DeepEquals no-op anyway, but removing it deletes a class of future failure.

Leave `:111-116` alone — an assessor creating **for** a trainee needs a subject picker and a "trainees I may
assess" query that does not exist (T106 item 2).

### 9. Seeds — declare ownership

For each of the four `_cpsa` folders: `"editable_by": "field:assessor_user_id"` on the `requested` state in
`workflow.json` and on the `assessment` and `feedback` sections in `schema.json`. **The `request` section
keeps the default** — that is what stops an authorised assessor rewriting `epa_id` / `assessor_user_id` /
`observed_on` and redirecting credit.

Same treatment for the legacy `mini_cex` / `dops` / `cbd` / `acat` seeds on their `accepted` state, **and
drop `required: true` from their assessor-owned fields** — `mini_cex/schema.json` has 14 of 14 fields
required, and since every transition validates the whole schema in Submit mode (`ActivityService.cs:136`),
`accept` and `cancel` are unsatisfiable on that type today. (The general fix is T105.)

> **Be honest in the handoff:** `PaediatricCatalogueSeeder.cs:113` skips keys that already exist, so these
> edits reach a **fresh database only**. Landing them on dev and production needs T103.

### 10. Tests

Domain: `editable_by` round-trips through Parse+Serialize for a section, a field and a state; a state
without it emits no property; `"editable_by": "assessor"` throws `SchemaParseException`.

Application — the backwards-compatibility contract first: a schema and workflow declaring no `editable_by`
anywhere gives subject and creator every field in a non-terminal state and an unrelated user none. Then:
assessor owns exactly `{overall_level, strengths, improvements, plan}` in `requested` and neither `epa_id`
nor `assessor_user_id`; trainee owns the request keys in `draft` and nothing in `requested`; terminal state
writable by nobody; create ignores the state gate (the `procedure_log` / `journal_club` regression).

**The verification criterion proper** is a pair: seed a trainee having pre-filled `overall_level` 2 against
a `CurriculumItem` with `MinimumLevelOrder` 4; the assessor patches 5 on `complete`; assert
`CountsSoFar == 1` **and** `MinimumLevelReachedCount == 1`. Mirror at level 2: `CountsSoFar == 1`,
`MinimumLevelReachedCount == 0` (`CreditApplier.cs:99-103`).

Plus: a `complete` patch that also changes `epa_id` throws naming `epa_id`, the activity stays in
`requested`, and no progress row exists; the same patch echoing `epa_id` **unchanged** succeeds (the
DeepEquals carve-out); `CreateActivityCommand` carrying `overall_level` returns a DTO whose `DataJson` has
none; a fully valid patch from the **subject** is denied before any merge (proving `:118` precedes `:129`);
the persisted `ActivityTransition` snapshot carries the merged data and the note.

Infrastructure: each `_cpsa` seed binds assessor ownership on the `requested` state and the
assessment/feedback sections, and the request section's rule is null — extend the convention test at
`CpsaWbaSeedTests.cs:150`. Legacy guard: assessor fields no longer `required: true`.

Web: `EditableFieldKeys {overall_level, strengths}` renders exactly two enabled inputs; driving `@oninput`
on a locked `#epa_id` leaves `DataJsonChanged` uninvoked (proves the C# guard, not the DOM hint); null +
`ReadOnly` still disables everything. `ActivityDataPatch` emits only changed writable keys. Bound assessor
sees an enabled `#overall_level` and a Save button and the captured command carries a patch of the
assessor's keys only; a **non-bound user gets an empty writable set, every field disabled, and no Save**
(T070's "non-bound users stay read-only", asserted directly); the history card renders a recorded note.

> Test conventions verified: Application tests hand-wire
> `new ActivityService(dbContext, new SchemaValidator(), new WorkflowEvaluator(), new CreditApplier(dbContext), new FieldPermissionEvaluator())`
> over `UseInMemoryDatabase` and call `IRequestHandler.Handle` directly (`ActivityHandlersTests.cs:29-70`);
> `SeedRuntimeData` must append a matching `ActivityTypeVersion` or `GetPinnedVersion` throws. Web tests
> inherit `Bunit.TestContext`, use `this.AddTestAuthorization().SetAuthorized(...)` and register
> `StubActivityReferenceDataService`; the only `FakeSender : IScopedSender` to copy is
> `Admin/UsersListSmokeTests.cs:47-67`. `Wombat.Web.csproj` already references `Wombat.Infrastructure`, so
> no csproj change is needed.
>
> **Never pass `--no-build` to `dotnet test` in this repo.**

### 11. Docs

Document `editable_by` in `CUSTOMIZATION.md` beside the ActorRule section: the two declarations, the
`subject|creator` default (so every already-published `ActivityTypeVersion` keeps today's `CanEditDraft`
behaviour without a republish), the conjunction rule, the create-time state-gate exception, and the explicit
statement that there is **no hard-coded assessor-note field** — assessor narrative is a schema field the
admin marks assessor-owned, while `ActivityTransition.Note` stays a workflow annotation.

## Suggested commit slicing

The repo rule is one commit per completed task; this is large enough to warrant two:

1. **Steps 1-4 + their tests** — DSL, permission engine, service, MediatR surface. No UI change, all
   defaults backwards-compatible, suites green.
2. **Steps 5-11** — the Web surface, seeds and docs.

## Verification

As the bound assessor, in `requested`: change a rating, add feedback, Complete. The stored `DataJson` shows
the assessor's values, the trainee's request fields are unchanged, and the `CurriculumItemProgress` row
reflects the **assessor-adjusted** `overall_level`. As a non-bound user the same page is fully read-only
with no Save button. Re-run Act 3 Step 3.5 as written.

**End-to-end verification needs T099 first** — the four `_cpsa` types are Speciality-scoped to Paediatrics
(ScopeId 3) and `UserSpecialityScopes` holds nobody on Speciality 3, so `ListActivityTypesQuery.cs:36`
filters them out of `/activities/new` for every non-admin user.

## Open decisions

1. **Do the seed edits ship with T070?** They reach a fresh database only until T103 lands. Recommended:
   **yes, ship them** — the defaults are backwards-compatible so nothing regresses, and T103 is T098 phase
   2b's problem.
2. **Is a `SaveActivityDataCommand` in scope?** It is the fix for "nobody can edit an existing activity from
   the UI" (`UpdateActivityDraftCommand` has zero dispatch sites — T106 item 1) and is cheap once the
   permission engine exists, but it renames an audited command and changes `IActivityService`. **The
   cleanest slice to defer** if T070 runs long.
3. **Should the submitted `assessor_user_id` be validated at all?** Making ownership data-driven off that
   field raises the stakes on T102. Minimum viable fix: reject `subject == assessor_user_id` at
   `CreateDraftAsync`. Decide before the seeds harden.
4. **Default field ownership is `subject|creator`.** Right for a coordinator filing on a trainee's behalf?
   The conservative default matches today's `CanEditDraft` exactly, but it means every assessor-owned field
   must be declared explicitly, including in each of the ten v11.1 tools still to be seeded.

## Related

Blocks T098 phase 2c onward. Prerequisite: T099. Interacts with T101, T102, T103, T105, T106.

---

## Resolution — 2026-09-17

Implemented as planned, then adversarially reviewed by three independent reviewers (authorization,
backwards-compatibility, ordinary correctness) reading the finished diff. **The review found two
regressions this work introduced and one pre-existing escalation it made fully reachable.** All three are
fixed below; each fix carries a regression test that was verified to fail against the pre-fix code.

**Build clean (0 warnings, warnings-as-errors on). 563 tests green** — Domain 59 (+9), Application 349
(+19), Infrastructure 59 (+9), Architecture 19, Web 77 (+32).

### Landed as planned

Steps 1-9 and 11. `editable_by` parses, serialises and round-trips on schema sections, schema fields and
workflow states; `ActorRuleMatcher` is extracted and `WorkflowEvaluatorTests` passes untouched;
`IFieldPermissionEvaluator` computes the state ∧ field conjunction with the `subject|creator` default;
`ActivityService` filters at create, merges only writable keys, and exposes `GetDetailAsync`;
`GetActivityByIdQuery` returns `ActivityDetailDto`; `TransitionActivityCommand.DataPatchJson` and `Note`
are `[Redact]`ed; `ActivityForm` locks per field on the C# guard rather than the `disabled` attribute;
`ActivityWorkflowActions` is presentational; `ActivityView` renders the editable surface and a history
card; `NewActivity` locks assessor fields at creation; the four `_cpsa` and four legacy WBA seeds declare
ownership; `CUSTOMIZATION.md` documents the DSL.

### Fixed after review

**1. The create-time permission check resolved against caller-controlled data (HIGH, introduced here).**
`CreateDraftAsync` assigned `activity.DataJson = submittedDataJson` *before* calling
`GetWritableFieldKeys`. A `field:` rule reads its answer out of `DataJson`, so a creator naming themself
in `assessor_user_id` unlocked the assessor-owned sections and their self-rating survived the filter —
defeating the exact hole step 3(a) exists to close. The activity is now built with **empty** data while
the writable set is computed; at creation nobody is bound by a data field yet, so only column-backed
rules (`subject`, `creator`), roles and scope can match.
*Test:* `Create_CreatorNamesThemselfAsTheAssessor_StillDropsTheAssessorOwnedFields`.

**2. Cancelling or declining a legacy WBA awarded curriculum credit (HIGH, introduced here).**
Step 9 dropped `required: true` from the legacy seeds' assessor fields, which made `cancel` and `decline`
satisfiable for the first time. In `mini_cex` / `dops` / `cbd` / `acat` the `declined` and `cancelled`
states were `terminal: true`, and `CreditApplier` fires on **any** terminal target state — so a withdrawn
request counted toward the trainee's observation volume. Reproduced by execution before the fix:
`cancel OK -> state=cancelled`, `CurriculumItemProgress.CountsSoFar = 1`.

Those states are now non-terminal, matching the CPSA design T098 phase 2a established. `research_output`'s
terminal `rejected` was the same latent defect and is fixed with them — it is harmless today
(`"counts_for": []`) and it is a dead end either way, but leaving one seed violating the new invariant
would be arbitrary. **This is the one change in this commit outside T070's stated scope.**
*Test:* `SeedParseTests.NoSeededWorkflow_MarksAnAbandonmentStateTerminal`, across every seed.

**3. A trainee could name themself as assessor and self-award credit (HIGH, pre-existing — T102).**
The subject legitimately owns `assessor_user_id` while the request is still theirs, so they could point
it at themself in `draft`, and from `requested` on they matched every `field:assessor_user_id` rule —
their own `complete` included. Pre-existing, but T070 makes that field decide **write ownership** as well
as transition rights, so the narrow half of T102 could not wait: `ActivityService` now refuses, at create
and on the patch merge, any value that makes an actor-binding field name the activity's subject.
The general fix — validating a `user` value against the users the caller may legitimately nominate —
remains T102.
*Tests:* `Create_SubjectNamesThemselfAsTheAssessor_IsRejected`,
`Transition_SubjectRetargetsTheAssessorFieldToThemself_IsRejected`.

**4. A dead-end state handed out a live form with no way to save it (MEDIUM).**
The CPSA workflows mark `declined` / `cancelled` non-terminal *on purpose* (see fix 2), so the terminal
flag alone did not close the write surface: the subject got enabled inputs on a refused activity.
`IsStateWritable` now also denies any state with no outgoing transitions — a dead end is as final as a
terminal state. `ActivityView` additionally falls back to read-only whenever no action is available,
because a transition is the only save channel there is and an editable form with no Save silently loses
work on navigation.
*Test:* `GetWritableFieldKeys_InANonTerminalStateWithNoWayOut_IsWritableByNobody`.

### Deliberately not fixed here

- **On an existing database this task changes nothing for the seeded types.** `PaediatricCatalogueSeeder`
  and `DataSeeder` skip keys that already exist, so the `editable_by` declarations reach a **fresh**
  database only. Dev and production keep their pinned v1 schemas, where no `editable_by` exists, the
  `subject|creator` default applies to every field, and the assessor's writable set is empty — i.e. the
  symptom persists there until **T103** lands. The engine is correct and inert. This is the single most
  important thing to know about this commit.
- **A consequence of that:** on the live database, legacy types in a non-terminal state give the *subject*
  the new editable surface (the default rule matches them), including over assessor-owned fields. They
  cannot save it — `cancel` still fails the old all-required validation — but it looks wrong. T103 closes
  it by landing the seeds' explicit rules.
- `UpdateDraftAsync` is the one write path the field gate does not cover. It has zero dispatch sites, so
  it is latent — but it is where a `SaveActivityDataCommand` would land. Recorded as T106 item 1.
- No builder UI for `editable_by`. It round-trips through the visual builder unharmed (the implementer
  correctly threaded it through `BuilderModels` rather than passing `null`, which would have dropped it on
  the first admin save), but it must be authored as raw JSON. Recorded in T106.

### What is still not proven

The automated suites cover the contract end to end at the service and component level. **Nobody has
clicked through it.** The browser verification — Act 3 Step 3.5, as an assessor on a CPSA Mini-CEX —
needs a **fresh database** (see above) or T103, plus the T099 scope rows (done on dev, 2026-09-17).
Until that run happens, treat "the assessor can enter a rating" as green-by-test, not green-by-use.
