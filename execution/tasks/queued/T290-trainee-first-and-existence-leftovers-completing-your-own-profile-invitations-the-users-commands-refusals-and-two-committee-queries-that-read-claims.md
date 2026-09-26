---
id: T290
title: Trainee-first and existence leftovers: completing your own profile, invitations, the Users commands' refusals, and two committee queries that read claims
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
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

- [ ] Each item fixed has a test, or its decision is recorded.

## Related

T278, T279, T281, T194, T185.

Note, 2026-09-25 (the final browser check): a circuit whose tab has gone still runs T279's revalidation after a role
change, and logs "Navigation failed … TaskCanceledException" and "Unhandled exception in circuit" at error level. It is
harmless, but it is noise: treat a cancelled navigation on a gone circuit as expected.

## Notes

- **T295 replay, 2026-09-26 (C02).** Note, 2026-09-26 (the T295 replay, Step 2.37, F-2.37a): a third query that reads claims, and the most visible one. `GetCommitteeMemberDashboardSummaryQuery.cs:34,55` lists only trainees whose curriculum is in the caller's sub-speciality claims. A CommitteeMember invitation may not carry a sub-speciality (`InvitationRules.cs:65-66`), and T113's rule (`TraineeScopeResolver.IsOverseenBy`, :725-727) gives a committee member every current trainee at their institution. So Dr van Rensburg, the panel's external member, who has no scope rows, saw 'No trainees have targets this period.' and 'No curriculum targets for these trainees.' (design/baseline/act-2/2.37-1-vanrensburg-home.png; states/home--committee-member-empty.png). Zulu, Naidoo and Botha saw the five registrars only because saving their assessor profiles wrote a Paediatrics scope onto them (Step 2.14; `CreateOrUpdateAssessorProfile.cs:125`), and even they see only that sub-speciality's trainees, not the institution's. This widens the task: read the committee card's trainees by the overseer rule (the institution's current trainees, through `KeepCurrentAsync` as now; an Administrator sees all), with a query form of `IsOverseenBy` beside `AdministeredProfiles` if `ListDecisionPanels` needs the same. Step 2.37's Expect records the empty card as expected and must change with it. On its own this item is Medium: the external member is the one panel seat that can never see the card, so consider taking it ahead of the rest of this P3 bundle. Verify with a handler test (a CommitteeMember with no speciality or sub-speciality claims sees the institution's current trainees and none from another institution) and by replaying Steps 2.33 and 2.37.
- **T295 replay, 2026-09-26 (C63).** Note, 2026-09-26 (T295 replay, steps 2.13 and 2.29): seen again, four times. `.scenario-app/wombat_scenario.log` holds three pairs, around lines 19512-19545, of 'fail: RemoteNavigationManager … Navigation failed when changing the location to /account/session-ended?returnUrl=%2F' and 'fail: CircuitHost[111] Unhandled exception in circuit' (TaskCanceledException). There is one pair each for Dr Zulu, Dr Naidoo and Dr Botha, whose Assessor role Prof Mbatha added 1 to 2 minutes after they signed out. A fourth pair (line 29253) came at the admissions in 2.29, probably du Plessis's. The repro is therefore wider than a closed tab. Signing out leaves the old circuit disconnected but retained (Program.cs sets no DisconnectedCircuitRetentionPeriod, so the 3-minute default applies; inferred). A role change within that time fails its revalidation, and `EndedSessionExit.Leave` (EndedSessionExit.cs:37) then navigates a circuit that has no client. The item's scope is unchanged: when the circuit has no client, skip that navigation or treat the cancellation as expected. Test that a role change on a disconnected circuit writes nothing at fail level.
- **Question from T303, 2026-09-26 (not decided).** A graduate cannot enter a second programme at all. `AdmitTrainee` requires PendingTrainee (`AdmitTrainee.cs:44-47`); Add role never offered PendingTrainee and, since T303 (`4824d62`), does not offer Trainee; and an invitation to an address an account already holds cannot be completed (T285: the provisioner reports the address Taken). A second programme needs an admission path of its own (for example admitting a graduate directly, or an admin-only "return to pending admission"), never Add role.
