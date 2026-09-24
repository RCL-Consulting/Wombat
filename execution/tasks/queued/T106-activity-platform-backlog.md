---
id: T106
title: "Activity platform backlog: smaller gaps found while mapping the transition pipeline"
status: queued
priority: P3
created: 2026-09-17
---
# T106 — Activity platform backlog: smaller gaps found while mapping the transition pipeline

> **This is a holding file, not a task.** It is a container for findings that have not been
> split out yet. **Do not "do" T106** — pick an item from the list below, file it with
> `harness.py task new`, and strike it through here. T106 closes when the list is empty.
> Rated P3 on 2026-09-20 so it stops ranking alongside actionable defects.

**Status:** open (a holding file — split any item into its own task when it is picked up)
**Surfaced:** 2026-09-17, verifying the T070 / T098 handoff against the source.

Each item below was verified in the code. None is large enough to warrant its own task file yet, but each
is a real gap and several will bite during T098 phases 2c-3.

## 1. No post-creation edit path exists for anyone

> **T102, 2026-09-24:** `UpdateActivityDraftCommand`, `UpdateActivityDraftInput` and `IActivityService.UpdateDraftAsync`
> are deleted. They had no caller and skipped fix 1, T070's writable-key filter, the state gate and the nominee gate. A
> post-creation save must be a transition (a self-transition if the state should not move) through `TransitionAsync`,
> or a new path that runs, before its first mutation: the actor gate, `MergeWritableKeys` against the pre-move state,
> `ThrowIfActorFieldNamesSubject`, schema validation, `DirectivesToJudge` → `ToolPermissionGate`, and
> `NomineeFieldsToJudge` → `NomineeGate`. The text below predates the deletion.

`UpdateActivityDraftCommand` exists (`Features/Activities/Commands/UpdateActivityDraft/`, implemented at
`ActivityService.cs:78-104`) but has **zero** Razor callers — a grep over `src/Wombat.Web` finds only
`CreateActivityCommand` and `TransitionActivityCommand`. Once an activity is created, its data can only
change through a transition's `DataPatchJson`.

Consequence: a trainee who saves a draft with a required field missing cannot fill it in later, and cannot
cancel it either (see T105). Their only route is to complete it or abandon it in place.

Its semantics if it is wired up: full replacement, not a patch; rejects terminal states (`:85-88`);
authorises via `CanEditDraft` = subject **or** creator only (`:90-93`, `:177-179`), so an assessor can never
use it; validates in Draft mode (`:97`); and is **not** limited to the initial state — a trainee can still
edit while the activity sits in `requested`, i.e. after the assessor has been asked but before they act.
That last one is arguably a defect in its own right.

## 2. An assessor cannot create an activity on a trainee's behalf

`NewActivity.razor:111-116` sends `new CreateActivityCommand(typeId, userId, userId, ...)` — the caller in
both the `SubjectUserId` and `CreatedByUserId` positions — and the page has no subject picker. The command
itself takes them as independent parameters (`CreateActivityCommand.cs:9-14`), so the domain supports it;
the UI does not.

Doing this properly needs a "trainees I may assess" query, which does not exist.

## 3. `ActivityWorkflowActions` evaluates the workflow client-side against a fabricated `ActivityType`

`ActivityWorkflowActions.razor:49` constructs `new Activity { ActivityType = new ActivityType() }` to probe
which transitions to render. `ActivityScope` defaults to `Global` and `ScopeId` to null
(`ActivityScope.cs:5`), so **any `scope:` actor rule evaluates false in the UI** while the server — which
has the real `ActivityType` via the Include at `ActivityService.cs:163` — may allow it. The button list and
the server verdict can disagree.

The same file constructs Domain types directly in a `.razor`, which `CLAUDE.md` lists as a layering rule.

T070's plan makes this component presentational (server-computed actions passed in), which fixes both. If
T070 is descoped, this stands alone.

## 4. Raw user ids are rendered where names belong

`ActivityInbox.razor:24` shows a bare `SubjectUserId` GUID. T095 fixed the same class of bug on the
Coordinator dashboard by injecting `IUserAdministrationService` and resolving display names — the same
pattern applies here and to any transition-history surface that shows an actor id.

## 5. `MergeJsonObjects` is shallow and cannot delete keys

`ActivityService.cs:207-227` merges top-level keys only: a nested object in the patch replaces the whole
stored subtree, and a JSON `null` sets the key to null rather than removing it. Fine for today's flat
schemas; relevant the moment T019-c (repeatable/nested sections) lands.

**No test covers `DataPatchJson` at all** — a grep for `DataPatch` across `tests/` returns nothing.
`ActivityHandlersTests.cs:63-70` exercises `complete` with a null patch only.

## 6. Authorization runs *before* the patch merge

`ActivityService.cs:118` evaluates the actor rule against the pre-patch `DataJson`; the merge is at
`:129-131`. Any future flow that expects the patch to supply the field a `field:` actor rule points at will
fail authorization. Benign today (the trainee sets `assessor_user_id` at draft time) but it fixes an
ordering constraint worth knowing about. It is also what stops a patch escalating its own authorisation.

## 7. `ActivityForm` writes every value as a JSON string

`_data[key] = value` — so `overall_level` lands as `"6"`, not `6`. Every downstream reader tolerates it
(`SchemaValidator.TryGetDecimal`, `CreditApplier.TryGetInt32`, the trajectory's `TryGetInt32` all accept
numeric strings), so this is not urgent. But anything writing `assessor_user_id` as a non-string silently
breaks the actor binding (`WorkflowEvaluator.cs:84-107` requires `ValueKind == String`).

## 8. Three field types render nothing useful

`ActivityForm.razor:98-102` renders a placeholder for `File`. Check `Checkbox`, `DateTime` and `Markdown`
against the builder's advertised type list before the remaining ten v11.1 tools are authored against them.

## 9. The trajectory chart does not filter by workflow state

`GetEpaTrajectoryForTraineeQuery` charts any activity carrying `epa_id` + a rating + `assessor_user_id`,
including one still in `draft` or `requested`. Reachable today only via trainee self-fill (see T102), and it
would become reachable through a half-finished assessor form if T070 ever persists partial ratings before
`complete`. Whatever T070 does, it must not create charted observations that no assessor has signed.

## 10. `CurriculumItem.WindowMonths` is stored, validated and editable — and read by nothing in the credit path

Validated `GreaterThan(0)` at `ManageCurriculumItems.cs:41,57`, carried through clone, editable in the admin
UI. Its sole consumer is `AdmitTrainee.cs:126` (`GetDefaultCompletionMonths`, taking the max across items to
derive an expected completion date). `CreditApplier` never reads it. Anyone assuming "the currency window is
already enforced" would be wrong — relevant to T098 phase 3.

## 11. The visual builder has no editor for `editable_by` (added 2026-09-17)

T070 added `editable_by` to the schema section, schema field and workflow state DSL. It **round-trips**
through the visual builder unharmed — `BuilderSectionModel` / `BuilderFieldModel` carry it through
`Parse` and `ToJson`, so an admin saving from the builder no longer silently discards a rule authored in
raw JSON — but there is no input for it. An institution building its own rated tool must hand-edit the
schema/workflow JSON to say who owns which field.

That is a direct dent in the platform premise. An actor-rule input on the section and field editors in
`ActivityTypeEdit.razor` (and on the state editor, once T019-d's visual workflow editor exists) is the
fix. Worth its own task when picked up.

## 12. ✅ DONE 2026-09-19 (with T119) — `RebuildCurriculumProgress` did not stamp `CreditedItemCount`, so a stale zero was never cleared (added 2026-09-17)

T108 stamps the credit outcome on the transition, and `ActivityView` warns when it is `0`. But the stamp
happens only on the transition path. `RebuildCurriculumProgressCommand` deletes every progress row and
replays, so it re-credits properly — and leaves the old `0` in place.

The consequence is precise and bad: an administrator fixes the curriculum, runs a rebuild, the activity
now credits, and **the warning banner telling them to fix it stays on for ever.** The remediation T108
points the reader at is the one thing that cannot clear T108's own warning.

Fix: stamp from the rebuild path too. It is the same three-valued logic against the same
`ICreditApplier` return, and it is what makes the signal trustworthy rather than sticky.

### Resolution

Picked up with [T119], which is what finally makes a rebuild something anyone actually runs: wiring
`Activity.ObservedOn` re-dates every completion, and a rebuild is the only thing in the product that can
move a stored tally.

`RebuildCurriculumProgressCommandHandler` now stamps `CreditedItemCount` **and**
`CreditScaleMismatchCount` (T109's sibling, which would otherwise have been left stale by the same
omission) onto the transition it credited against. Three things make the stamp mean what T108 says it
means:

- **The `counts_for` gate is checked before the applier is called**, exactly as
  `ActivityService.TransitionAsync` checks it — not read back out of a zero in the result. Reading the
  result instead would stamp `0` on every reflective note, journal club, procedure log, QI project,
  research output and teaching session, which credit nothing by design and must stay `null`.
- **The transition stamped is selected with the same expression `CreditApplier.GetCreditKey` uses**
  (newest by `OccurredOn`), so the stamp and the dedupe key describe the same move rather than two
  different ones.
- **The stamps are applied after the replay finishes**, so the only writes outstanding while the replay
  can still throw are the progress rows.

Two further defects in the same handler were fixed alongside, because a stamp is no use on a command
nobody dares run:

- **Atomicity.** It used to delete every `CurriculumItemProgress` row and *save*, then replay and save
  again — a failure in between left every trainee in the system on zero progress. It now zeroes the
  pre-existing rows **in place**, replays into them, and commits once; EF wraps one `SaveChangesAsync`
  in one transaction, so this needs no `BeginTransaction` seam on `IApplicationDbContext` (which exposes
  none) and stays exercisable on the in-memory provider (which has none). A replay that throws restores
  the rows it zeroed and detaches the ones it added, so a failed rebuild cannot leave a poisoned context
  for the next `SaveChanges` in the same scope either.
- **Reach.** It takes a `ClaimsPrincipal` and refuses anyone who is not a global `Administrator`, and an
  optional `TraineeUserId` that confines both the zeroing and the replay to one trainee — which is what
  [T119] wants, a rebuild aimed at the rows whose dates actually moved rather than a global wipe. It
  returns counts of what it moved instead of a bare `int`.

Still **no entry point**, deliberately: where an Administrator-only button lives, what its confirmation
says and how the result is shown are UI questions with a DESIGN.md pass attached. What changed is that
wiring one is now a presentation job with no remaining hazard behind it.

Tests: `tests/Wombat.Application.Tests/Activities/RebuildCurriculumProgressTests.cs`.

## 13. `AssessorPendingNudgeJob` reads the live workflow, not the pinned version (added 2026-09-17)

Noted while building T103. The job reads `ActivityType.WorkflowJson` — the *current* published version —
while each activity validates against the version it was created under. None of T103's nine republishes
renamed or removed a state key, so nudges are unaffected today.

A future seed edit that renamed a state would silently stop nudging every activity pinned to the old
version: no error, no log, just assessors who stop being chased. Audit every reader of the live
`WorkflowJson`/`SchemaJson` for the same assumption.

## 14. A zero-credit completion is only visible one activity at a time (added 2026-09-17)

T108's warning is per-activity. A registrar has to open each record to discover the pattern, which is
exactly the failure mode the task was filed against — "fifty-five encounters logged, nothing counted" is a
statement about a *year*, not about one form.

The surface that closes it is a column on `MyActivities`: widen `ActivitySummaryDto` and have
`ListActivitiesBySubjectQuery` correlate the last transition's `CreditedItemCount`. Cheap now that the
data exists.
