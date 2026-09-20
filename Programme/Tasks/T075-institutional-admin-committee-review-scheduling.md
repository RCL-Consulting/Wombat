# T075 — InstitutionalAdmin committee-review scheduling (F-4A-1)

**Status:** ✅ Shipped 2026-06-01 (Opus). Found while starting Act 4 Phase 4.A.

## Problem (F-4A-1)

Act 4 casts **Prof Mbatha (InstitutionalAdmin)** as the scheduler of the annual
committee reviews. But the committee-review surface excluded `InstitutionalAdmin`
at both layers, so Mbatha hit `/access-denied`:

- **Page gates** — `ReviewsSchedule.razor` (`/committee/reviews`) and
  `ReviewDetail.razor` (`/committee/reviews/{id}`) allowed
  `Coordinator, Administrator, SpecialityAdmin, SubSpecialityAdmin, CommitteeMember`.
- **Handler gate** — `CommitteeDecisionAuthorization.DemandReviewScheduling` admitted
  `Administrator, Coordinator, SpecialityAdmin, SubSpecialityAdmin`.

This was an inconsistency, not a deliberate boundary: the sibling method
`DemandPanelAdministration` **already** admits `InstitutionalAdmin` (T063), so Mbatha
could create the panel and set its chair/members but could not schedule a single review
on it — while a Coordinator (a lesser admin role) could. The reviews list also filtered
strictly to panels the caller sits on (`ListReviewsForPanelQuery`), so even after the
gate opened an admin would see an empty list.

## Fix (mirrors the T063 panel-admin scope migration)

Scope-aware: an `InstitutionalAdmin` may schedule/view only on panels within their own
institution. A panel's effective institution is its `InstitutionId` (Institution-scoped)
or its `Speciality.InstitutionId` (Speciality-scoped). Global `Administrator` is unchanged.

1. `CommitteeDecisionAuthorization.DemandReviewScheduling` — admit `InstitutionalAdmin`.
2. `ScheduleCommitteeReviewCommandHandler` — after loading the panel, if the caller is
   InstitutionalAdmin (and not Administrator), resolve the panel's institution and reject
   with `UnauthorizedAccessException("…panels in your institution")` on mismatch.
3. `ListReviewsForPanelQuery` — Administrator sees all reviews; InstitutionalAdmin sees
   reviews for panels in their institution; everyone else keeps the member-based filter.
4. `GetCommitteeReviewByIdQuery` — InstitutionalAdmin may view (read-only) any review for
   a panel in their institution, scope-checked; out-of-institution throws. Conduct actions
   (start / record / ratify / resolve-appeal) remain **chair-gated** in their own handlers.
5. `ReviewsSchedule.razor` + `ReviewDetail.razor` — add `InstitutionalAdmin` to the gate.

**Deliberately NOT changed:** ratification (`DemandChairAccess`) and appeal resolution
(`DemandAppealResolverAccess`) stay reserved to the panel chair / external / global
Administrator. Whether an InstitutionalAdmin should ratify or resolve appeals is a separate
governance decision (the committee does the committee's work) — see F-4A-2 below.

## Tests

`tests/Wombat.Application.Tests/Features/CommitteeDecisions/ReviewSchedulingScopeGuardTests.cs`
(+6): schedule on own Institution/Speciality panel; reject other-institution panel; list
scoped to own institution; get-by-id allowed in-scope and rejected out-of-scope.
Application 252 → 258. Full solution green (Domain 45, Infrastructure 8, Architecture 19,
Web 42); Integration not run (Docker).

## Live verification

Restored `act3R-final-t065`, drove Act 4 as Mbatha: all 5 reviews scheduled, visible in her
scoped list, and the post-schedule redirect to `/committee/reviews/{id}` rendered for her.

## Follow-ups raised (deferred)

- **F-4A-2 (governance):** the scenario casts Mbatha for ratification (4.8) and appeal
  resolution (4.10), but those are chair/Administrator acts. Decide whether the institutional
  lead should be an allowed ratifier / appeal body, or amend the scenario casting.
- **Nav:** no NavMenu link to `/committee/reviews` for InstitutionalAdmin (URL-only).
