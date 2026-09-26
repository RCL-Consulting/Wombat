---
id: T297
title: Five dashboard cards read literal state keys, so an assessor's Pending requests reads 0 beside a full inbox and the coordinator's Stalled requests never lists a stalled CPSA request
status: queued
priority: P2
owner: agent
depends_on: []
created: 2026-09-26
---

# T297 — Five dashboard cards read literal state keys, so an assessor's Pending requests reads 0 beside a full inbox and the coordinator's Stalled requests never lists a stalled CPSA request

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** High. The Assessor's and the Coordinator's main cards misreport the work waiting on them for every rated CPSA instrument, which is the catalogue KGK uses. An assessor told '0 assessments awaiting review' has no reason to open his inbox. The coordinator's chase-up card exists for the request nobody has acted on, and it never shows one. T074 rated this same card being dead as P2.
**Surfaced:** 2026-09-26, the T295 replay of the rewritten scenario runbook (findings F-3.24a, F-3.33a, F-3.51a, F-A.6.8a, F-3.12a, F-3.16a, F-3.30a, F-3.53a, F-3.54a).

## Symptom

On the fresh replay database (wombat_scenario), each card disagrees with the page it links to.
- Assessor, 'Pending requests': Dr Patel's Home read '0 assessments awaiting review' while his Activity Inbox held Nomsa Mahlangu's DOPS (Requested) and Pieter du Plessis's Portfolio and Logbook Review (Awaiting review) (Step 3.24; design/baseline/act-3/3.24-1-patel-home.png, 3.24-2-patel-inbox-two.png). Dr Zulu read 0 beside Dr Mahlangu's Mini-CEX in her inbox (3.33; act-3/3.33-2-zulu-home-assessor-view.png, states/home--assessor-pending.png). Dr Khumalo read 0 beside du Plessis's CBD (3.51; act-3/3.51-1-khumalo-dashboard.png, 3.51-2-khumalo-inbox.png). Patel read 0 beside the portfolio review again (A.6.8; act-A/A.6.8-2-assessor-dashboard.png). 'Accepted, needing action' is always empty, because no CPSA or KGK workflow has a state called accepted (3.51; states.md § States no local replay reaches).
- Coordinator, 'Stalled requests': after the runbook's ageing SQL, Mr Smit's card listed only du Plessis's Portfolio and Logbook Review. Dr Mahlangu's Mini-CEX (activity 21, Requested, untouched for 8 days) was missing, although the same run's assessor-pending-nudge mailed Dr Zulu about it (3.30; act-3/3.30-1-smit-stalled-requests.png; mail 20260926-124322-036.eml). The rows are plain text, not links.
- SpecialityAdmin and SubSpecialityAdmin, 'Pending reviews': '1 activities in review' counted du Plessis's portfolio review but not his Requested CBD, and 'Review queue →' opened /activities/inbox, which read 'Inbox clear' (3.53, 3.54; act-3/3.53-1-mokoena-dashboard.png, 3.53-2-review-queue-inbox-clear.png, 3.54-1-sithole-dashboard.png).
- Trainee, 'Activity inbox': Dr Ndlovu's card listed his Declined Mini-CEX, which has no move left, and /activities/inbox did not (3.12; act-3/3.12-1-ndlovu-home-declined.png). After his reflective exercise was re-submitted, the card dropped it (Awaiting discussion, which he may still cancel) while the inbox listed it (3.16; act-3/3.16-4-resubmitted.png).

## Root cause

Each card selects activities by literal state keys. The inbox and the nudge job instead read the workflow the activity is pinned to: ListActivitiesByActorInboxQuery.cs:58-63 keeps an activity when a transition is available to the caller, and AssessorPendingNudgeJob.cs:80-103 keeps a non-terminal state with a field: actor. T203 fixed the same class for 'done' only.
- GetAssessorDashboardSummaryQuery.cs:57-66 keeps only activities the assessor created or has already moved. The trainee makes both the create and the submit, so a request she filed naming him is never a candidate. :80-82 then keeps only CurrentState == "requested", which misses a portfolio review or reflective exercise waiting in 'submitted'. :84-86 reads "accepted", a state only the Demo legacy mini_cex, dops, cbd and acat seeds have. :88 reads "declined" and "cancelled" for Recent decisions. pendingCount (:38-51) is computed and never used.
- GetTraineeDashboardSummaryQuery.cs:93-94 lists 'requested', 'accepted', 'declined' or 'draft'. A declined CPSA request is a dead end, since the seed's workflow has no move out of it, and 'submitted' (reflective_exercise_cpsa, portfolio_review_cpsa, clinical_audit_cpsa) is left out.
- GetCoordinatorDashboardSummaryQuery.cs:44 reads CurrentState == "submitted". T074 set that for the old draft→submitted shape. Every rated *_cpsa WBA (mini_cex, dops, cbd, cca, rca, chart_stimulated_recall, direct_observation) waits for its assessor in 'requested'. CoordinatorDashboard.razor:22 renders each row as text.
- GetSpecialityAdminDashboardSummaryQuery.cs:58 and GetSubSpecialityAdminDashboardSummaryQuery.cs:51 read "submitted" or "in_review", and no seed has in_review. SpecialityAdminDashboard.razor:14-17 and SubSpecialityAdminDashboard.razor:14-17 label the count 'activities in review', with no singular, and link it to /activities/inbox. That page lists only what the caller can move, not the programme backlog the tile counts.

## What to build

Every 'waiting' card is read from the activity's pinned workflow, as T203 did for 'done', and every count agrees with the page its link opens.
- One shared reading in Application, beside ActivityCompletion and PinnedWorkflows in Features/Activities/Services:
  (a) actionable by the caller: the inbox's rule, moved out of ListActivitiesByActorInboxQuery so the inbox and the cards call the same code;
  (b) awaiting a reviewer: a non-terminal state with an outgoing transition whose actor has a field: or role: arm, meaning someone other than the author. AssessorPendingNudgeJob uses it too, and keeps its own extra need for a named nominee to mail.
- Assessor: 'Pending requests' counts what the caller can act on in the inbox, leaving out their own portfolio (T203's rule). Replace 'Accepted, needing action' with those items, oldest first, marked overdue past DashboardThresholds.AssessorDueDays. Recent decisions are the activities the caller last moved that are finished or have no move left, not the literal 'declined' and 'cancelled'. Delete pendingCount.
- Trainee: the 'Activity inbox' card lists what /activities/inbox lists for the trainee. A declined request is already announced by AssessmentDeclinedEmail, and it appears on Recent activities with its badge. Update Step 3.12's Expect, which asks for it on this card.
- Coordinator: 'Stalled requests' lists the activities awaiting a reviewer that have been untouched for CoordinatorStallDays and that the caller may read (T101), oldest first. Each row links to /activities/{id} and names the trainee (T250). A request the nudge mails about appears on the card.
- Speciality and SubSpeciality admins: 'Pending reviews' counts the programme's backlog awaiting a reviewer, on the stamps T185 conjoins (speciality or sub-speciality, and the institution). Give it a singular label ('1 activity awaiting review'). Do not link it to /activities/inbox: point it at a page that lists those rows, or drop the link until one exists (the nav's Stalled Activities is still a placeholder).
- Add a guard test that no query under Features/Dashboards compares CurrentState with a string literal. This class has now come back three times (T074, T203, here).
- ListActivitiesByActorInboxQuery.cs:34-40 loads every activity in the database before evaluating. If the cards share it, narrow in SQL first: the caller's readable scope and non-terminal states.
- Update the runbook Expect lines the fix changes: 2.38, 3.12, 3.24, 3.30, 3.33, 3.51, 3.53, 3.54 and A.6.8.

## Verification

- [ ] AssessorDashboardQueryTests: a mini_cex_cpsa that a trainee filed and submitted naming the assessor, with no transition by him, counts 1. A portfolio_review_cpsa in 'submitted' naming him counts. His own draft does not.
- [ ] Parity test: for each seeded workflow and caller, the Assessor and Trainee cards' rows equal ListActivitiesByActorInboxQuery's rows, less the assessor's own portfolio.
- [ ] TraineeDashboardQueryTests: a declined mini_cex_cpsa is not on the Activity inbox card, and a submitted reflective_exercise_cpsa is.
- [ ] CoordinatorDashboardQueryTests: a mini_cex_cpsa in 'requested' untouched for 8 days is listed, and a draft and a declined request are not. A shared-predicate test shows that an activity AssessorPendingNudgeJob would mail about is on the card.
- [ ] Speciality and SubSpeciality admin handler tests: a requested CPSA WBA in the caller's programme and institution is counted, and one at another institution is not. The label reads '1 activity'.
- [ ] The guard test fails when a literal state comparison is put back under Features/Dashboards (mutation-checked by reintroducing Coordinator :44).
- [ ] bUnit: the Coordinator's stalled rows link to /activities/{id}, and DashboardLinkAuthorizationTests stays green.
- [ ] Browser: replay Steps 3.12, 3.16, 3.24, 3.30, 3.33, 3.51, 3.53, 3.54 and A.6.8 on a fresh database. Each card's figure matches the page it opens, and the 3.30 card lists the Mini-CEX the nudge mailed about. Re-capture the baseline screenshots.

## Related

T203 (the same class, for 'done'), T074 (the coordinator card, fixed for the old shape), T101 and T185 (the tiles' scope), T220 (state labels), T250 (names on rows), T261 (dashboard links admit the role), T280 (trainee dashboard link names), D44. Runbook steps 2.38, 3.12, 3.16, 3.24, 3.30, 3.33, 3.51, 3.53, 3.54, A.5.10 and A.6.8. ActivityDraftNudgeJob.cs:72 reads the literal 'draft'. That is right for every seed today; derive it from the workflow's initial state if a builder-made type can start elsewhere.
