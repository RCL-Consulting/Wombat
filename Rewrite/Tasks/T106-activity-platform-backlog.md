# T106 — Activity platform backlog: smaller gaps found while mapping the transition pipeline

**Status:** open (a holding file — split any item into its own task when it is picked up)
**Surfaced:** 2026-09-17, verifying the T070 / T098 handoff against the source.

Each item below was verified in the code. None is large enough to warrant its own task file yet, but each
is a real gap and several will bite during T098 phases 2c-3.

## 1. No post-creation edit path exists for anyone

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
