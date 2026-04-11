# T007 — Assessment aggregate & state machine (domain only)

> **⚠ Superseded by the Activity platform.** See `../CUSTOMIZATION.md`. This task was written before the no-code customization requirement was surfaced. The `Assessment` aggregate is replaced by Mini-CEX / DOPS / CbD / ACAT activity types seeded in **T020** on top of the platform built in **T017–T019**. Do not execute this task as written. Keep this file for history; it still documents the state-machine shape that the activity type's workflow JSON must express.

**Phase:** 2 — Assessment workflow
**Depends on:** T004, T006
**Blocks:** T008

## Goal

Model the `Assessment` aggregate as a proper state machine on the domain side. No handlers, no DB migrations, no UI — just the domain types and tests for them. Getting this right in isolation makes everything downstream easy.

## What to do

1. In `Wombat.Domain/Assessments/`:
   - `Assessment` — the aggregate root. Fields: Id, TraineeUserId, AssessorUserId, EpaId, FormId, CurriculumItemId?, State (enum), RequestedOn (UTC), ContextNote (trainee's description of the clinical encounter), ScheduledFor (`DateOnly?`), DecisionOn (UTC?), CompletedOn (UTC?), CancelledOn (UTC?), IsPublic.
   - `AssessmentState` — enum: `Requested`, `Accepted`, `Declined`, `Cancelled`, `Completed`.
   - `CriterionResponse` — Id, AssessmentId, FormCriterionId, EntrustmentLevelId (for scale criteria), FreeText (optional).
   - `AssessmentEvent` — Id, AssessmentId, EventType (`Requested`, `Accepted`, `Declined`, `Cancelled`, `Completed`, `Reopened`), OccurredOn (UTC), ActorUserId, Note.
2. State transitions as methods on `Assessment`:
   - `static Assessment Request(...)` — factory, produces a `Requested` assessment and records a `Requested` event.
   - `Accept(Guid actorUserId, DateOnly scheduledFor)` — only valid from `Requested`.
   - `Decline(Guid actorUserId, string reason)` — only valid from `Requested`.
   - `Cancel(Guid actorUserId, string reason)` — valid from `Requested` or `Accepted`.
   - `Complete(Guid actorUserId, IEnumerable<CriterionResponse> responses)` — valid only from `Accepted`. Records the responses, sets `CompletedOn`, pushes a `Completed` event.
   - Each method validates the current state and throws a domain exception on illegal transitions.
3. Define the transition table explicitly as a `static IReadOnlyDictionary<AssessmentState, HashSet<AssessmentState>> AllowedTransitions` in the aggregate. Unit test against this table.
4. `Assessment` is sealed. Its constructor is private. All creation goes through the factory.
5. No EF yet — the aggregate is pure C# in `Wombat.Domain`. EF configurations and migrations come in T008 where the handlers need them.
6. Unit tests under `tests/Wombat.Domain.Tests/Assessments/`:
   - Illegal transitions throw.
   - Legal transitions succeed and produce the right event.
   - `Complete` requires all required criteria to have responses; raises if any are missing.
   - `CriterionResponse` with a scale criterion requires a level; with a free-text criterion, level is null and free text is required.

## Files created

- `src/Wombat.Domain/Assessments/Assessment.cs`
- `src/Wombat.Domain/Assessments/AssessmentState.cs`
- `src/Wombat.Domain/Assessments/CriterionResponse.cs`
- `src/Wombat.Domain/Assessments/AssessmentEvent.cs`
- `src/Wombat.Domain/Assessments/AssessmentStateException.cs`
- `tests/Wombat.Domain.Tests/Assessments/AssessmentStateMachineTests.cs`
- `tests/Wombat.Domain.Tests/Assessments/AssessmentCompletionTests.cs`

## Verification

- [ ] `dotnet build` clean.
- [ ] `dotnet test tests/Wombat.Domain.Tests` — all new tests pass, coverage for every transition in the table.
- [ ] Every illegal transition is covered by at least one negative test.

## Notes & gotchas

- Domain methods take **actor user IDs** as parameters, not the whole principal. The handler supplies the ID after authorisation has passed.
- `AssessmentEvent`s are inside the aggregate; they are produced by the state transition methods and persisted together with the aggregate. Do not expose a way to add events from outside the aggregate.
- The aggregate does not know about Identity, roles, or EF. It knows about domain types only. A handler checks "is this actor the assessor on this assessment?" before calling `Accept`.
- Do not conflate `Declined` with `Cancelled`. `Declined` = assessor said no before starting. `Cancelled` = either party backed out after acceptance. They are different data and different events.
- `IsPublic` is preserved from the old model — it means "visible to other trainees as an example". Default false.
- `CurriculumItemId` is **nullable** because not every assessment maps to a curriculum requirement (an assessor might do an impromptu observation).
