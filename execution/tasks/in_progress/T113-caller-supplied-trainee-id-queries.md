---
id: T113
title: "Two more queries trust a caller-supplied trainee id"
status: in_progress
priority: P2
created: 2026-09-19
started: 2026-09-24
---
# T113 — Two more queries trust a caller-supplied trainee id

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Status:** open
**Surfaced:** 2026-09-19, closing the read boundary for T101.
**Severity:** Medium — progress and committee history about a named trainee, readable by anyone who can
name them.

## Symptom

T101 added a `ClaimsPrincipal` and scope filtering to `ListActivitiesBySubjectQuery`,
`GetEpaTrajectoryForTraineeQuery`, the coordinator dashboard's stalled-activity query and
`GetSamplingConcentrationWarnings`. Two queries of exactly the same shape were left, because they read a
different aggregate and `ActivityReadScope.WhereReadableBy` does not apply to them:

- `src/Wombat.Application/Features/Curricula/GetCurriculumProgressForTrainee.cs:17` —
  `GetCurriculumProgressForTraineeQuery(string TraineeUserId)`, no principal. *(Since [T130], 2026-09-23, it is
  `GetCurriculumProgressForTraineeQuery(string TraineeUserId, DateOnly? AsOf = null)` and returns a
  `TraineeCurriculumProgressSummaryDto?`. Still no principal: adding one is this task.)* Called from
  `MyProgress.razor:81`, directly beside the trajectory query that T101 *did* scope.
- `src/Wombat.Application/Features/CommitteeDecisions/ListReviewsForTrainee.cs:8` —
  `ListReviewsForTraineeQuery(string TraineeUserId)`, no principal.

Both filter on the string the caller passes, so passing another trainee's id returns their data:
curriculum progress (counts, minimum-level achievement, credited items) and committee review history.

## Why they were not done with T101

They read `CurriculumItemProgress` and `CommitteeReview`, neither of which carries the T101 scope stamps.
Scoping them means either resolving the trainee's scope through `TraineeProfile` (the
`ExportPortfolio.ResolveTraineeScopeAsync` pattern) or stamping those aggregates too.

Prefer the resolver: unlike `Activity`, these rows are *about a trainee by definition*, so there is no
case where the subject has no profile and the row still means something.

## Also in scope

`GetSamplingConcentrationWarnings` was given a principal by T101, but it scopes the **rows** it reads,
not the **review** it is asked about. Reaching another institution's review id returns an empty report
rather than a refusal. The proper review-level ladder already exists in `GetCommitteeReviewById.cs:35-63`
(trainee self + state gate / InstitutionalAdmin institution / panel membership). Duplicating it would be
a third copy — extract it into one shared helper and call it from both.

## A third site, added 2026-09-20 by [T132]

`ListMsfCampaignsForCoordinator.cs` is the same defect without even the fig leaf of a caller-supplied
id: `ListMsfCampaignsForCoordinatorQuery()` **takes no parameters at all**, the handler filters on
nothing, and every coordinator in the country sees every MSF campaign in every institution — each
row carrying `SubjectUserId` (who is being assessed), response counts and release state.

It was found while fixing [T132] and deliberately **not** fixed there. `MsfCampaign` carries no
institution, so scoping it needs the `TraineeProfile` resolver — and
`ExportPortfolio.ResolveTraineeScopeAsync` is a deliberate three-step resolver with a comment
explaining why it is not one join. Copying it into the MSF handler would have been the third copy,
which is exactly what this task already says to avoid. Do it here, with the shared helper.

**Also verified 2026-09-20:** no MSF command or query anywhere takes a `ClaimsPrincipal` — grep
across `src/Wombat.Application/Features/MultiSourceFeedback/`, zero files. So the read boundary is
not the only gap in that feature; the write side has no caller identity either.

## Related

[T101] closed the activity surface. [T112] is the data-rights equivalent.

## Update 2026-09-24 — EPA-stream survey

**Widened to the MSF write side and three more queries of the same shape.** The title now undercounts. The scope is
below.

Observed at `431e69e`:

- **The MSF commands and report query take an id and no principal:**
  - `OpenMsfCampaignCommand` (`OpenMsfCampaign.cs:16`)
  - `CloseMsfCampaignCommand` (`CloseMsfCampaign.cs:9`)
  - `WithdrawMsfCampaignCommand` (`WithdrawMsfCampaign.cs:10`)
  - `AddMsfInvitationCommand` (`AddMsfInvitation.cs:10-13`)
  - `GetCampaignAggregateReportQuery` (`GetCampaignAggregateReport.cs:6`; no scope at `:19-23`)

  The only gate is page-level `[Authorize]` (`CampaignEdit.razor:6`, `CampaignReport.razor:4`). So a Coordinator at any
  institution can open, close, add respondents to, or read the report of another institution's campaign by route id.
  The exploit path is inferred from the signatures and the page gates, not exercised.
- **"No MSF command takes a `ClaimsPrincipal`" is now partly false.** Since [T121], create and release check the
  subject's institution through `MsfCampaignRules.EnsureSubjectIsInScopeAsync` (`MsfCampaignRules.cs:56-88`; called at
  `ReleaseMsfCampaign.cs:86`).
- **Three unlisted queries have this task's shape:**
  - `GetActiveDecisionsForTraineeQuery` (`GetActiveDecisionsForTrainee.cs:9`; caller `MyAuthorisations.razor:78-80`)
  - `GetDecisionHistoryForEpaQuery` (`GetDecisionHistoryForEpa.cs:9`; no caller)
  - `ListMsfCampaignsForTraineeQuery` (`ListMsfCampaignsForTrainee.cs:8`). Its caller, `MyMsfReports.razor:96`,
    authorizes the report in the page (`:100-104`), not in the handler.
- **"Also in scope" is already fixed; strike it.** `GetSamplingConcentrationWarnings.cs:103` calls
  `CommitteeDecisionAuthorization.DemandReviewAccess` (`CommitteeDecisionAuthorization.cs:83`). It landed in [T101]'s
  own commit `518d990`.
- **The trainee-scope resolver has four copies, not two.** `SubjectScopeResolver` is canonical but `internal` to
  Infrastructure (`SubjectScopeResolver.cs:15,41-46`). `ExportPortfolio.cs:173-212` and `PortfolioPdfService.cs:119-125`
  are the other two. All three break ties by `IsActive`, then `Id`. `MsfCampaignRules.cs:71-77` breaks ties by
  `IsActive`, then `ProgrammeStartDate`, which is drift.

**Scope now:**

1. **Six queries take a `ClaimsPrincipal` and filter through one shared resolver.** They are
   `GetCurriculumProgressForTrainee` and `ListReviewsForTrainee` (the original two), [T132]'s
   `ListMsfCampaignsForCoordinator`, and the three above.
2. **Open, Close, Withdraw, AddInvitation and the aggregate report take a principal** and run `MsfCampaignRules`'
   subject-institution check before any mutation. The audit pipeline commits a failed handler's mutation, so the check
   must come first. Do not use `CanAccessInstitution`: it returns false for Coordinators (`MsfCampaignRules.cs:44-50`).
3. Every Razor call site passes `authState.User`.

**Implementer decisions (recommended; record the final choice here):**

- **The resolver lives in Application**, profile-based. `SubjectScopeResolver`'s identity fallback layers on top of it
  in Infrastructure. Retire the ExportPortfolio, PDF and MSF copies.
- **Tie-break by `Id` descending**, as three of the four copies and the activity scope stamp do, so MSF agrees with the
  scope stamped on activities. This changes which institution a trainee with several profiles resolves to for MSF;
  see [T153].
- **Who may read another trainee's data:** mirror [T101]'s read ladder (self, Administrator, in-scope overseer;
  `ActivityService.IsScopedOverseerOf`). Ask the operator only if committee members should be added.
- **An out-of-scope query returns empty or null, not a refusal**, per CLAUDE.md's T056 convention. Commands throw
  `UnauthorizedAccessException`.

**Verification (this file had none):**

- [ ] Each of the six queries returns nothing to an out-of-scope caller and the data to an in-scope one. Handler tests.
- [ ] Open, Close, Withdraw and AddInvitation refuse a Coordinator from another institution and change nothing. The
      aggregate report refuses the same Coordinator. Handler tests.
- [ ] One resolver remains. Its tie-break is tested with a trainee who holds two profiles.
- [ ] Browser: `/msf/campaigns` and `/msf/reports/{id}` as a Coordinator from the other institution. First check that
      the scenario coordinators carry an institution claim.
- [ ] The MSF scenario is re-run. Rows that newly disappear from lists are acceptable (W-007).
- [ ] Full suite green, no `--no-build`.
