# T018 — Activity engine: generic commands, workflow runtime, credit rules

**Phase:** 2 — Activity platform
**Depends on:** T017
**Blocks:** T019, T020, T021, T022

## Goal

Build the runtime that takes an activity type's schema + workflow + credit rules and turns them into behaviour: creating activities, validating submissions, enforcing transitions, applying credit on completion. One set of MediatR handlers serves every activity type. No per-type code.

## What to do

1. **Services** in `Wombat.Application/Features/Activities/Services/`:
   - `IActivityService` — high-level façade. Methods: `CreateDraftAsync(CreateActivityInput)`, `UpdateDraftAsync(UpdateActivityDraftInput)`, `TransitionAsync(TransitionActivityInput)`, `GetAsync(activityId)`.
   - `ISchemaValidator` — given a parsed `FormSchema` and a `DataJson`, return a list of validation errors. Errors are structured (field key + message + rule).
   - `IWorkflowEvaluator` — given a parsed `Workflow`, an `Activity`, a transition key, and a `ClaimsPrincipal`, return `Allow` or `Deny(reason)`. Same evaluator used by UI rendering and command handlers — single source of truth.
   - `ICreditApplier` — given a completed `Activity` and its `ActivityType`, apply credit rules and return the list of updated `CurriculumItemProgress` rows. Called as part of the transition handler when the target state is terminal.
2. **Implementations** in `Wombat.Infrastructure/Activities/`:
   - `ActivityService` — orchestrates parse + validate + apply. Uses `IApplicationDbContext` via the Application interface.
   - `SchemaValidator` — walks fields, checks required, regex, min/max, scale validity, etc. Supports the `show_if` visibility rules (a field is only required if visible).
   - `WorkflowEvaluator` — evaluates the actor rule against the principal and the activity. Pure logic, no I/O. Fully unit-tested.
   - `CreditApplier` — walks the credit rules, matches `CurriculumItem`s, upserts progress rows.
3. **CQRS handlers** in `Wombat.Application/Features/Activities/`:
   - `CreateActivityCommand` — inputs: activityTypeId, subjectUserId, initialData. Loads type, parses schema, validates initial data (partial allowed — drafts), persists with `CurrentState` = workflow initial state, records initial `ActivityTransition`.
   - `UpdateActivityDraftCommand` — inputs: activityId, newData. Only valid while in a non-terminal state that the actor may edit. Validates and saves.
   - `TransitionActivityCommand` — inputs: activityId, transitionKey, optional data patch, optional note. Loads activity + type, evaluates workflow, validates data against schema (all fields required by the transition must be present and valid), applies the transition, saves, records a transition event with a snapshot, applies credit rules if entering a terminal state.
   - Queries: `GetActivityByIdQuery`, `ListActivitiesBySubjectQuery`, `ListActivitiesByActorInboxQuery` (generic "what needs my attention"), `ListActivityTypesQuery` (scoped to the user's institution).
4. **Progress aggregate** (related but kept simple):
   - `CurriculumItemProgress` — materialised progress per trainee per curriculum item. Fields: Id, CurriculumItemId, TraineeUserId, CountsSoFar, LastActivityId, LastUpdated, MinimumLevelReachedCount. Written only by `CreditApplier`. Readable by dashboards.
   - A background job (or explicit command) to rebuild progress from scratch — for recovery and for after a schema change. Skeleton only; real scheduling is in T024.
5. **Unit tests** in `tests/Wombat.Application.Tests/Activities/`:
   - Schema validator: required-field missing, min/max violations, hidden-field exclusion, scale value outside declared scale.
   - Workflow evaluator: every actor rule path (subject, role, scope, combinations).
   - Credit applier: a completed activity increments the matching progress row; non-matching does nothing; a field below the minimum level does not count.
   - End-to-end handler test: create draft → update → transition → verify state, event, progress.

## Files created

- `src/Wombat.Application/Features/Activities/Services/{IActivityService,ISchemaValidator,IWorkflowEvaluator,ICreditApplier}.cs`
- `src/Wombat.Application/Features/Activities/Commands/{CreateActivity,UpdateActivityDraft,TransitionActivity}/**`
- `src/Wombat.Application/Features/Activities/Queries/{GetActivityById,ListActivitiesBySubject,ListActivitiesByActorInbox,ListActivityTypes}/**`
- `src/Wombat.Application/Features/Activities/Dtos/**`
- `src/Wombat.Infrastructure/Activities/{ActivityService,SchemaValidator,WorkflowEvaluator,CreditApplier}.cs`
- `src/Wombat.Domain/Curricula/CurriculumItemProgress.cs` (+ EF config in Infrastructure)
- `src/Wombat.Infrastructure/Persistence/Migrations/*CurriculumProgress.cs`
- `tests/Wombat.Application.Tests/Activities/**`

## Verification

- [ ] `dotnet build` clean.
- [ ] `dotnet test` — all new tests green.
- [ ] A programmatic test seeds a tiny activity type ("hello_world" with one text field and a one-state workflow), creates an activity, completes it, verifies the transition record exists.
- [ ] The workflow evaluator returns the same decision for identical inputs in a property test (1000 randomised runs, no exceptions).
- [ ] `CreditApplier` is idempotent: applying credit twice for the same activity does not double-count.

## Notes & gotchas

- The evaluator is pure logic. Do not inject DbContext into it. If it needs data, the handler fetches and passes it in.
- `CreditApplier` idempotency is critical — transitions may be retried under connection failures and must not double-credit. Use the activity id + transition key as an idempotency key on the progress row.
- Drafts are allowed to have incomplete data; only transitions to non-draft states require full validation. The schema DSL should mark fields as "required to submit" rather than "required to save a draft".
- When a terminal state is reached, apply credit **inside** the same transaction as the state change. Otherwise a crash between the two leaves the system in a state where the activity is complete but progress is not updated.
- No magic strings for transition keys in handlers. Handlers take a string from the caller; validation happens against the parsed workflow.
- Do not cache parsed schemas/workflows across requests yet. Premature optimisation; add a cache in a later task if profiling justifies it.
