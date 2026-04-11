# T009 — STAR reflection aggregate & commands

> **⚠ Superseded by the Activity platform.** See `../CUSTOMIZATION.md`. STAR becomes a seeded activity type (T020) with four longtext sections and a `draft → submitted → approved/declined` workflow expressed as JSON. No dedicated aggregate.

**Phase:** 2 — Assessment workflow
**Depends on:** T004, T006
**Blocks:** T011, T013

## Goal

Model STAR reflections (Situation-Task-Action-Result) as a separate aggregate with its own lifecycle: Draft → Submitted → Approved / Declined (with feedback). Ship CQRS + Blazor UI.

## What to do

1. In `Wombat.Domain/StarReflections/`:
   - `StarReflection` — aggregate root. Fields: Id, TraineeUserId, EpaId, State (enum), CreatedOn (UTC), SubmittedOn (UTC?), DecisionOn (UTC?), DecidedByUserId?, Feedback.
   - `StarReflectionState` — enum: `Draft`, `Submitted`, `Approved`, `Declined`.
   - `StarSection` — Id, StarReflectionId, Kind (enum: `Situation`, `Task`, `Action`, `Result`), Body (text), Order.
2. State transition methods on `StarReflection`:
   - `static Draft(Guid traineeUserId, int epaId)` — factory; produces a Draft with empty sections pre-created in order S-T-A-R.
   - `UpdateSection(StarSectionKind kind, string body)` — allowed only in `Draft` or `Declined`. After `Declined`, updating a section moves the whole reflection back to `Draft` (so "resubmit" is just "edit + submit").
   - `Submit(Guid actorUserId)` — requires actor to be the trainee. Requires all sections to be non-empty. Transitions to `Submitted`.
   - `Approve(Guid actorUserId, string? feedback)` — from `Submitted`. Writes DecidedOn, DecidedByUserId, Feedback.
   - `Decline(Guid actorUserId, string feedback)` — feedback is required when declining.
3. EF configurations + migration `StarReflections` (include `Designer.cs`).
4. CQRS in `Wombat.Application/Features/StarReflections/`:
   - `CreateStarDraftCommand`
   - `UpdateStarSectionCommand`
   - `SubmitStarCommand`
   - `ApproveStarCommand`
   - `DeclineStarCommand`
   - Queries: `GetStarReflectionById`, `ListStarForTrainee`, `ListPendingStarForSpeciality` (for admins).
5. Authorisation: only the trainee on the reflection can edit/submit. Only SpecialityAdmins / SubSpecialityAdmins in the trainee's scope can approve/decline.
6. Blazor pages under `Wombat.Web/Components/Pages/StarReflections/`:
   - `MyStarReflections.razor` — trainee list
   - `NewStarDraft.razor` — create + pick EPA
   - `StarEditor.razor` — four sections, per-section autosave optional, submit button.
   - `PendingStarInbox.razor` — admin list
   - `StarReview.razor` — admin view with approve / decline + feedback box.
7. Unit tests for all state transitions in `tests/Wombat.Domain.Tests/StarReflections/`.

## Files created

- `src/Wombat.Domain/StarReflections/{StarReflection,StarSection,StarReflectionState,StarSectionKind}.cs`
- `src/Wombat.Infrastructure/Persistence/Configurations/{StarReflection,StarSection}Configuration.cs`
- `src/Wombat.Infrastructure/Persistence/Migrations/*StarReflections.cs`
- `src/Wombat.Application/Features/StarReflections/**`
- `src/Wombat.Web/Components/Pages/StarReflections/**`
- `tests/Wombat.Domain.Tests/StarReflections/**`

## Verification

- [ ] `dotnet build` clean.
- [ ] `dotnet test` — state machine tests pass, authorisation tests pass.
- [ ] Manual: trainee creates a draft, fills in all four sections, submits. Admin sees it in the pending inbox, declines with feedback. Trainee edits and resubmits. Admin approves. Trainee sees it as approved.
- [ ] Cannot submit a reflection with any section empty.

## Notes & gotchas

- Resubmission = the `Declined` → edit → `Submitted` path. Do **not** model "resubmit" as a separate command; the `UpdateSection` from `Declined` bumps the state to `Draft` and then `Submit` re-runs. This keeps the state machine small.
- A reflection belongs to **one** EPA. If a trainee needs to reflect on a multi-EPA experience, they create one reflection per EPA. This matches how portfolio systems in real programmes work.
- Declined feedback is required and should be visible to the trainee permanently, including after approval on the next round.
- The DOMAIN.md document explicitly renames the old `STARApplication` → `StarReflection`. Do not keep the old name around, even for backward compatibility; there is nothing to be backward-compatible with.
