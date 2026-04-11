# T011 — Role dashboards

> **⚠ Partially superseded by the Activity platform.** See `../CUSTOMIZATION.md`. Dashboards still exist but their widgets query generically across `Activity` rather than per-type. Depends on T019 (generic list views) rather than on T008/T009. Re-read before executing.

**Phase:** 3 — Web
**Depends on:** T003, T004, T005, T006, T008, T009, T010
**Blocks:** T016

## Goal

Build the landing page each role sees immediately after login. A dashboard gives an at-a-glance view of what the user should do next. This is the first thing a real user touches; treat it as the top of the funnel for every workflow.

## What to do

Build one dashboard per non-admin role (admins use the admin pages from T003/T004 directly). Each dashboard lives under `Wombat.Web/Components/Pages/Dashboards/` and is routed based on the user's primary role at login.

1. `TraineeDashboard.razor` (roles: `Trainee`, `PendingTrainee`):
   - For `PendingTrainee`: a welcome card explaining they are waiting to be admitted to a curriculum, plus a "Complete your profile" link.
   - For `Trainee`: progress cards showing curriculum completion (X of Y required assessments per EPA, colour-coded). Recent assessments, pending requests, draft STAR reflections, declined STAR reflections needing attention.
2. `AssessorDashboard.razor`:
   - Pending requests count with "Review inbox" CTA.
   - Assessments in `Accepted` state that need completing (overdue highlighted).
   - Recent completed assessments (last 10).
3. `CoordinatorDashboard.razor`:
   - List of stalled assessment requests (in `Requested` > N days).
   - List of pending invitations nearing expiry.
   - Quick action: "Issue invitation".
4. `SpecialityAdminDashboard.razor` / `SubSpecialityAdminDashboard.razor`:
   - Pending STAR reviews count.
   - Trainees in programme.
   - Curriculum coverage summary.
5. `InstitutionalAdminDashboard.razor`:
   - Users in institution (broken down by role).
   - Specialities + sub-specialities count.
   - Quick links to admin pages.
6. `CommitteeMemberDashboard.razor`:
   - Read-only progress view for the speciality.
   - Trainees approaching completion.
7. `AdministratorDashboard.razor`:
   - System health (DB connection, email queue depth).
   - Users across all institutions.
   - Quick links to seed / maintenance tools.
8. A router component (`DashboardRouter.razor`) mounted at `/` that inspects the user's roles and renders the highest-priority dashboard. Priority order is the table in DOMAIN.md, top-down.
9. Each dashboard runs its data load via MediatR queries; no direct DbContext from the component. Queries may need to be added if they don't already exist from earlier tasks.
10. Skeleton loaders for every card while data is loading. Blazor Server is fast but not instant.

## Files created

- `src/Wombat.Web/Components/Pages/Dashboards/DashboardRouter.razor`
- `src/Wombat.Web/Components/Pages/Dashboards/{Trainee,Assessor,Coordinator,SpecialityAdmin,SubSpecialityAdmin,InstitutionalAdmin,CommitteeMember,Administrator}Dashboard.razor`
- Possibly new queries under `src/Wombat.Application/Features/Dashboards/` for aggregated counts.

## Verification

- [ ] `dotnet build` clean.
- [ ] Manual: log in as each role (create test users via invitation), land on `/`, confirm the correct dashboard renders and all cards populate.
- [ ] A user with two roles lands on the higher-priority one.
- [ ] Curriculum progress calculation matches reality when a trainee has completed some assessments.

## Notes & gotchas

- Curriculum progress is the trickiest piece. For each `CurriculumItem`, count the trainee's **Completed** assessments on that EPA within the item's `WindowMonths`, only counting ones where at least one scale criterion response is at or above `MinimumLevelOrder`. Spec this out as a method on `Curriculum` (or a service) so tests can cover it.
- Dashboards aggregate — a lot of queries per load. Keep the initial render under ~200ms by projecting efficiently. If it starts to drag, introduce a `DashboardSummaryQuery` per role that returns a single DTO.
- Do not put any business logic in the razor components. Components render what the query returns and call commands on user action.
- Skeleton placeholders prevent the "everything shifts around" feel. Use `<div class="skeleton" />` with a subtle pulse animation.
