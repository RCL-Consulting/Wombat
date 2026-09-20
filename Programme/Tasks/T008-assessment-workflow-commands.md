# T008 — Assessment workflow commands & UI

> **⚠ Superseded by the Activity platform.** See `../CUSTOMIZATION.md`. Generic activity commands (`CreateActivityCommand`, `TransitionActivityCommand`) and a generic dynamic form renderer replace per-type handlers and pages. Build T017–T019 instead; the Mini-CEX seed in T020 demonstrates the pattern.

**Phase:** 2 — Assessment workflow
**Depends on:** T007
**Blocks:** T011, T013

## Goal

Wire the `Assessment` aggregate into EF, add CQRS commands and queries around it, and build the Blazor pages for the request/accept/decline/cancel/complete flow.

## What to do

1. EF configuration for `Assessment`, `CriterionResponse`, `AssessmentEvent`. Include indexes on `TraineeUserId`, `AssessorUserId`, and `(State, ScheduledFor)` for list queries.
2. EF migration: `Assessments`. Include `Designer.cs`.
3. CQRS handlers in `Wombat.Application/Features/Assessments/`:
   - **Commands:**
     - `RequestAssessmentCommand` — trainee requests. Validates the assessor is in the same speciality, the form links the EPA, and (if `CurriculumItemId` is supplied) the item belongs to the trainee's curriculum.
     - `AcceptAssessmentCommand`
     - `DeclineAssessmentCommand`
     - `CancelAssessmentCommand` — callable by either the trainee or the assessor while in `Requested` or `Accepted`.
     - `CompleteAssessmentCommand` — takes a list of criterion responses.
   - **Queries:**
     - `GetAssessmentByIdQuery` (scoped by actor — trainees see their own, assessors see their own, admins see everything in their scope).
     - `ListAssessmentsForTraineeQuery`
     - `ListAssessmentsForAssessorQuery`
     - `ListPendingRequestsForAssessorQuery`
     - `ListAssessmentsForSpecialityQuery` (admin view)
4. Authorisation checks in each handler. Do not trust the UI; the handler is the authority:
   - `RequestAssessmentCommand` requires the actor to be the trainee named in the command.
   - `AcceptAssessmentCommand` requires the actor to be the assessor on the request.
   - `CompleteAssessmentCommand` requires the actor to be the assessor and the state to be `Accepted`.
   - `CancelAssessmentCommand` requires the actor to be either the trainee or the assessor.
5. Blazor pages under `Wombat.Web/Components/Pages/Assessments/`:
   - `NewAssessmentRequest.razor` — trainee chooses EPA, form, assessor, writes context note, submits.
   - `MyAssessments.razor` — trainee's list with filters by state.
   - `PendingInbox.razor` — assessor's inbox of `Requested` assessments.
   - `AssessmentDetail.razor` — read-only view with appropriate action buttons based on state and role.
   - `CompleteAssessment.razor` — assessor fills in the form, submits.
6. The "complete" page dynamically builds form inputs from the `AssessmentForm`'s criteria: scale criteria render as a level picker, free-text criteria render as a textarea. Validation: all `IsRequired` criteria must be answered before submit is enabled.
7. Write at least one end-to-end handler test (in `Wombat.Application.Tests`) that covers request → accept → complete happy path with an in-memory DbContext.

## Files created

- `src/Wombat.Infrastructure/Persistence/Configurations/{Assessment,CriterionResponse,AssessmentEvent}Configuration.cs`
- `src/Wombat.Infrastructure/Persistence/Migrations/*Assessments.cs`
- `src/Wombat.Application/Features/Assessments/Commands/**`
- `src/Wombat.Application/Features/Assessments/Queries/**`
- `src/Wombat.Application/Features/Assessments/Dtos/**`
- `src/Wombat.Web/Components/Pages/Assessments/**`
- `tests/Wombat.Application.Tests/Features/Assessments/**`

## Verification

- [ ] `dotnet build` clean.
- [ ] `dotnet test` — happy path passes, every illegal-transition test from T007 still passes, authorisation tests pass.
- [ ] Manual: as a Trainee, request an assessment; log in as the Assessor and accept it; fill in the form; complete; log back in as the Trainee and see the completed assessment on their dashboard.
- [ ] A trainee cannot cancel an assessment belonging to another trainee (authorisation check).
- [ ] An assessor cannot complete an assessment that is not in `Accepted` state.

## Notes & gotchas

- Load the aggregate and its children in one `Include`-heavy query; do not do N+1.
- Completing an assessment writes all criterion responses in one transaction with the state transition. If either fails, the whole thing rolls back.
- Trainees must be able to see their own assessments even after they are deactivated (e.g. after graduating). Scope the query by user ID, not by active-trainee status.
- The "assessor list" when a trainee is creating a request should only show active assessors in the trainee's speciality. If cross-speciality assessment is needed later, add a feature flag.
