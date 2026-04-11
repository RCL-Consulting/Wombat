# T022 — Committee decisions (ARCP equivalent)

**Phase:** 4 — Hardcoded domain features
**Depends on:** T006, T020
**Blocks:** T011, T016

## Goal

Model formal committee decisions on trainee progression: panel composition, evidence under review, decision category, rationale, appeals. This is the regulatory record of "we reviewed trainee X and decided Y" that survives audits and supports appeals. Dedicated domain, not a customizable activity type.

## Why this is not an activity type

Committee decisions carry legal weight: they must be immutable after issue, appealable through a defined process, defensible to a regulator, and produced by a named panel of named individuals. Letting an admin change the "decision" form in the builder would undermine all of that. Hardcoded.

## What to do

1. **Domain** in `Wombat.Domain/CommitteeDecisions/`:
   - `DecisionPanel` — Id, Name, Scope (Institution or Speciality), CreatedOn. Reusable group of people.
   - `DecisionPanelMember` — Id, PanelId, UserId, Role (`Chair`, `Member`, `External`).
   - `CommitteeReview` — Aggregate root. Id, TraineeUserId, PanelId, ReviewPeriodFrom (`DateOnly`), ReviewPeriodTo (`DateOnly`), ScheduledOn (`DateOnly`), State (`Scheduled`, `InProgress`, `Decided`, `Ratified`, `UnderAppeal`, `Final`). Has domain methods: `Start()`, `RecordDecision(category, rationale, conditions, actorUserId)`, `Ratify(actorUserId)`, `LodgeAppeal(reason, actorUserId)`, `ResolveAppeal(outcome, actorUserId)`.
   - `CommitteeDecision` — child of `CommitteeReview`. Id, ReviewId, Category (enum: `SatisfactoryProgress`, `SatisfactoryWithObservations`, `InadequateProgressAdditionalTraining`, `InadequateProgressRepeat`, `ReleaseFromTraining`, `OutcomeDeferred`), Rationale (longtext), Conditions (longtext, optional — remediation plan), DecidedOn, DecidedByChairUserId.
   - `CommitteeAppeal` — child. Id, ReviewId, LodgedOn, LodgedByUserId, Reason, ResolvedOn?, ResolvedByUserId?, Outcome (enum: `Upheld`, `Dismissed`, `Remitted`).
   - `CommitteeEvidence` — child of `CommitteeReview`. Snapshot list of everything the panel considered: activity ids, STAR reflection ids, MSF campaign ids, supervisor report ids. Frozen at the moment of `Start()` so later changes to activities don't rewrite history.
2. **Immutability**:
   - Once a `CommitteeDecision` is recorded, it cannot be mutated. Edits are blocked at the aggregate level.
   - Appeal outcomes are additive: a new record, not an edit.
   - Even admins cannot bypass this. The only way to "change" a decision is to record a new decision via appeal resolution.
3. **Evidence bundle**:
   - When a review is `Started`, the system snapshots the evidence pointers into `CommitteeEvidence`. Accessing the review later always pulls the snapshot, not the live state, so the panel always sees what they saw at the time.
   - Snapshot is by reference (IDs + counts + computed summaries), not by value. The underlying activities still exist; the snapshot freezes *which ones* were considered.
4. **CQRS** in `Wombat.Application/Features/CommitteeDecisions/`:
   - `CreateDecisionPanelCommand`, `UpdateDecisionPanelCommand` (member changes only)
   - `ScheduleCommitteeReviewCommand`
   - `StartCommitteeReviewCommand` (snapshots evidence)
   - `RecordCommitteeDecisionCommand`
   - `RatifyCommitteeDecisionCommand` (chair sign-off)
   - `LodgeAppealCommand`
   - `ResolveAppealCommand`
   - Queries: `GetCommitteeReviewById`, `ListReviewsForTrainee`, `ListReviewsForPanel`, `ListReviewsForChair`.
5. **Blazor pages** under `Wombat.Web/Components/Pages/CommitteeDecisions/`:
   - `PanelsList.razor`, `PanelEdit.razor`
   - `ReviewsSchedule.razor` (coordinator view — schedule a review for a trainee)
   - `ReviewDetail.razor` (panel view — shows evidence bundle, decision form)
   - `MyReviews.razor` (trainee view — shows their ratified decisions, supports lodging appeals)
6. **PDF export**:
   - A ratified decision can be exported as a PDF (via QuestPDF — see T023). The PDF shows panel members, decision category, rationale, conditions, evidence summary, ratification signature. This is often required for regulatory submission.
7. **Migration**: `CommitteeDecisions`. Include `Designer.cs`.
8. **Tests**:
   - Immutability: attempting to edit a `CommitteeDecision` throws.
   - State machine: every transition (Scheduled → InProgress → Decided → Ratified → UnderAppeal → Final) tested.
   - Evidence snapshot: starting a review captures a fixed set; later activity edits do not change the snapshot.
   - Authorisation: only panel chairs can ratify; only the trainee can lodge appeals; only the appeal body (defined by panel role) can resolve.

## Files created

- `src/Wombat.Domain/CommitteeDecisions/**`
- `src/Wombat.Infrastructure/Persistence/Configurations/CommitteeDecisions/**`
- `src/Wombat.Infrastructure/Persistence/Migrations/*CommitteeDecisions.cs`
- `src/Wombat.Application/Features/CommitteeDecisions/**`
- `src/Wombat.Web/Components/Pages/CommitteeDecisions/**`

## Verification

- [ ] `dotnet build` clean.
- [ ] `dotnet test` — immutability and state machine tests green.
- [ ] Manual: create a panel, schedule a review for a trainee, start it (confirm evidence snapshot saved), record a decision, ratify. Log in as trainee, see the decision. Lodge an appeal. Log in as appeal resolver, resolve it. Confirm the full history is preserved.
- [ ] Attempting to edit a ratified decision in any way results in an error.

## Notes & gotchas

- `CommitteeDecision` is the most sensitive record in the system. Treat it like a legal document.
- The `CommitteeMember` role from DOMAIN.md finally has a real job here. Wire it up.
- Panel members from different institutions (`External`) can review trainees outside their home institution — this is how "external examiners" work in many programmes. Support it explicitly via the `External` member role.
- Do not allow mass-decide operations. Every review is individual. A coordinator who wants to record decisions for 40 trainees clicks through 40 reviews. The annoyance is a feature — it prevents accidental bulk errors.
- Evidence pointers survive even if an underlying activity is later deleted for data-rights reasons (T026). Define the interaction clearly: data-rights deletion of an activity does not remove the committee evidence pointer, but does remove the ability to view the underlying content. Document this in T026.
