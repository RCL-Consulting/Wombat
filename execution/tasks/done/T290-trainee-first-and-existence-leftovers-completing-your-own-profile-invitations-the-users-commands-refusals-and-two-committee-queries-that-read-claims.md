---
id: T290
title: Trainee-first and existence leftovers: completing your own profile, invitations, the Users commands' refusals, and two committee queries that read claims
status: done
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
started: 2026-10-05
completed: 2026-10-15
---

# T290 — Trainee-first and existence leftovers: completing your own profile, invitations, the Users commands' refusals, and two committee queries that read claims

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low.
**Surfaced:** 2026-09-25, the T278, T279 and T281 reviews.

## Items

- `CompleteTraineeProfile` removes the caller's own Trainee role, and `/admin/trainees` has no trainee-first check.
- Issuing an invitation, revoking one by id, and T283's ResendInvitation have no trainee-first check (T278 guards only the
  Users feature).
- The Users commands reveal whether an id exists. Add role, remove role and password reset answer "The user could not be
  found." where scope would refuse. Add and remove role, and `SetUserLockout`, give the global-administrator refusal
  before the scope check. Use one refusal before any state check (T194's rule).
- `ListReviewsForChairQuery` is unused and ignores the review access rules. Delete it.
- `ListDecisionPanels` still lists panels from the caller's claims, not the seat rule.
- **Withdrawn trainees and the Trainee role.** Nothing warns while an after-end encounter date is typed (T281 refuses
  credit, but the form does not say so), and a withdrawn trainee keeps the Trainee role. Decide whether they should.

## Verification

- [x] Each item fixed has a test, or its decision is recorded (2026-10-15; see the closing note).

## Related

T278, T279, T281, T194, T185.

Note, 2026-09-25 (the final browser check): a circuit whose tab has gone still runs T279's revalidation after a role
change, and logs "Navigation failed … TaskCanceledException" and "Unhandled exception in circuit" at error level. It is
harmless, but it is noise: treat a cancelled navigation on a gone circuit as expected.

## Notes

- **T295 replay, 2026-09-26 (C02).** Note, 2026-09-26 (the T295 replay, Step 2.37, F-2.37a): a third query that reads claims, and the most visible one. `GetCommitteeMemberDashboardSummaryQuery.cs:34,55` lists only trainees whose curriculum is in the caller's sub-speciality claims. A CommitteeMember invitation may not carry a sub-speciality (`InvitationRules.cs:65-66`), and T113's rule (`TraineeScopeResolver.IsOverseenBy`, :725-727) gives a committee member every current trainee at their institution. So Dr van Rensburg, the panel's external member, who has no scope rows, saw 'No trainees have targets this period.' and 'No curriculum targets for these trainees.' (design/baseline/act-2/2.37-1-vanrensburg-home.png; states/home--committee-member-empty.png). Zulu, Naidoo and Botha saw the five registrars only because saving their assessor profiles wrote a Paediatrics scope onto them (Step 2.14; `CreateOrUpdateAssessorProfile.cs:125`), and even they see only that sub-speciality's trainees, not the institution's. This widens the task: read the committee card's trainees by the overseer rule (the institution's current trainees, through `KeepCurrentAsync` as now; an Administrator sees all), with a query form of `IsOverseenBy` beside `AdministeredProfiles` if `ListDecisionPanels` needs the same. Step 2.37's Expect records the empty card as expected and must change with it. On its own this item is Medium: the external member is the one panel seat that can never see the card, so consider taking it ahead of the rest of this P3 bundle. Verify with a handler test (a CommitteeMember with no speciality or sub-speciality claims sees the institution's current trainees and none from another institution) and by replaying Steps 2.33 and 2.37.
- **T295 replay, 2026-09-26 (C63).** Note, 2026-09-26 (T295 replay, steps 2.13 and 2.29): seen again, four times. `.scenario-app/wombat_scenario.log` holds three pairs, around lines 19512-19545, of 'fail: RemoteNavigationManager … Navigation failed when changing the location to /account/session-ended?returnUrl=%2F' and 'fail: CircuitHost[111] Unhandled exception in circuit' (TaskCanceledException). There is one pair each for Dr Zulu, Dr Naidoo and Dr Botha, whose Assessor role Prof Mbatha added 1 to 2 minutes after they signed out. A fourth pair (line 29253) came at the admissions in 2.29, probably du Plessis's. The repro is therefore wider than a closed tab. Signing out leaves the old circuit disconnected but retained (Program.cs sets no DisconnectedCircuitRetentionPeriod, so the 3-minute default applies; inferred). A role change within that time fails its revalidation, and `EndedSessionExit.Leave` (EndedSessionExit.cs:37) then navigates a circuit that has no client. The item's scope is unchanged: when the circuit has no client, skip that navigation or treat the cancellation as expected. Test that a role change on a disconnected circuit writes nothing at fail level.
- **Question from T303, 2026-09-26 (not decided).** A graduate cannot enter a second programme at all. `AdmitTrainee` requires PendingTrainee (`AdmitTrainee.cs:44-47`); Add role never offered PendingTrainee and, since T303 (`4824d62`), does not offer Trainee; and an invitation to an address an account already holds cannot be completed (T285: the provisioner reports the address Taken). A second programme needs an admission path of its own (for example admitting a graduate directly, or an admin-only "return to pending admission"), never Add role.
- **2026-10-05: the committee item is done** (with T298, before flow 06's build, the operator's Q8). `GetCommitteeMemberDashboardSummaryQuery` reads every current trainee at the caller's institution (`IsOverseenBy`'s rule), not the sub-speciality claims; handler tests `CommitteeMemberDashboard_ForAMemberWithNoSubSpeciality_ListsTheInstitutionsCurrentTrainees` and the updated institution-wide ones; Step 2.37's Expect and states.md's empty row rewritten; browser: Dr van Rensburg reads the five registrars on `wombat_scenario_t290`. **Every other item above is still open**; the task stays in progress only because the harness cannot requeue it.

- **2026-10-15: closed (Opus).** Every remaining item, one session, at the operator's "close T290":
  - **Trainee-first on Trainees and Invitations.** `TraineeAdministrationRules` (the Users rule, its own words) is asked
    first by every Trainees handler (admit, update, withdraw, complete, read, both lists, both curriculum-choice reads),
    so nobody marks their own programme complete; `UserAdministrationRules.DemandUserAdministration` first by issue,
    list, resend and revoke. `/admin/trainees` and `/admin/invitations` show an info note in its place, as Users does.
    Tests: `CompleteTraineeProfileCommandHandlerTests.Handle_RefusesATraineeWhoAdministersTheirInstitution_…`,
    `InvitationTraineeFirstTests`.
  - **Existence.** `UserAdministrationRules.DemandInScope`: add role, remove role, lockout and password reset give a
    non-Administrator one refusal for an unknown id, an id at another institution and a global administrator, before
    any check of what the user holds. Revoke and resend by id likewise. Tests: `UserCommandsExistenceTests`,
    `InvitationTraineeFirstTests`.
  - **`ListReviewsForChairQuery`** deleted (no caller).
  - **`ListDecisionPanels`**: the "sits on it" arm is the seat rule (`PanelSeat.MaySit`, from the user store), not the
    member list. Two tests that pinned an External member from another institution listing the panel were rewritten
    (`DecisionPanelScopeTests`, `CommitteeRequestsAuthoriseFirstTests`).
  - **The disconnected circuit.** `CircuitConnection` (a scoped `CircuitHandler`); `EndedSessionExit` does not navigate a
    circuit with no browser and leaves on reconnection. Test: `EndedSessionExitConnectionTests`. Not yet seen in a log:
    the next replay that changes a role soon after a sign-out shows it.
  - **Withdrawn trainees: D54** (the operator): withdrawal takes the Trainee role away, as completion does, so there is no
    one to warn about an after-end date. Runbook Steps 5.27, 5.28, 6.41, A.1.6, A.4.6, the Act 5 outcome and the
    appendix cast rewritten; **their Actuals are from T366 and wait for the next replay.**
  - Not done here (T303's question, unchanged): a graduate's or withdrawn registrar's second programme.
