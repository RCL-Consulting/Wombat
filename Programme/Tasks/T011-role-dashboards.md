# T011 — Role dashboards

> **⚠ Partially superseded by the Activity platform.** See `../CUSTOMIZATION.md`. Dashboards still exist but their widgets query generically across `Activity` rather than per-type. Depends on T019 (generic list views) rather than on the superseded T008/T009. Re-read before executing.

**Phase:** 3 — Web
**Depends on:** T003, T004, T005, T006, T010 (design system), T019 (activity inbox/list queries), T021, T022
**Blocks:** T016

## Goal

Build the landing page each role sees immediately after login. A dashboard gives an at-a-glance view of what the user should do next. This is the first thing a real user touches; treat it as the top of the funnel for every workflow. Every dashboard uses the same grid and the same card vocabulary from `../DESIGN.md § Dashboard layout grid` — no per-role custom HTML.

Read `../DESIGN.md` before starting. This task consumes `.dashboard-grid`, `.detail-card`, `.detail-card--interactive`, `.detail-card--empty`, `.skeleton`, `PageHeader`, `StatePanel`, `Alert`, and `PagerControls`. It **does not** introduce any new global CSS classes silently. If a widget needs something `app.css` does not provide, add the class to `DESIGN.md` + `app.css` in the same commit instead of inlining a `<style>` block.

## What to build — the shared shape

Every dashboard page lives under `Wombat.Web/Components/Pages/Dashboards/` and looks like:

```razor
@page "/dashboard/trainee"    @* or assessor, coordinator, etc. — see router below *@
@attribute [Authorize(Roles = "Trainee, PendingTrainee")]
@rendermode InteractiveServer
@inject IScopedSender Sender

<PageTitle>Dashboard — Wombat</PageTitle>

<PageHeader Title="Welcome, @_name" Subtitle="@_roleLabel" />

<StatePanel IsLoading="@(_vm is null)" LoadError="@_error">
  <div class="dashboard-grid">
    <DashboardCard Title="Curriculum progress"
                   Icon="book"
                   Href="/curriculum"
                   Emphasis="true">
      @* body *@
    </DashboardCard>
    …
  </div>
</StatePanel>
```

`StatePanel` (from T010) handles the skeleton rendering while `Sender.Send(new TraineeDashboardSummaryQuery(...))` is in flight. **Do not** roll a "Loading…" text node.

### `DashboardCard.razor` — new shared component

Goes under `Components/Shared/DashboardCard.razor`. Every dashboard card is one of these. It wraps a `.detail-card` with a card-header / card-body / card-footer contract so the grid renders predictably.

Props:

- `Title` — `<h3>` inside the card header strip.
- `Icon?` — name of an icon in `wwwroot/icons/` (`book`, `inbox`, `alert-triangle`, `users`, `calendar`, `file-text`).
- `Href?` — if present, the whole card becomes `.detail-card .detail-card--interactive` and navigates on click / Enter.
- `Emphasis?` — `bool`, renders a left accent stripe using `--secondary-color` (via new `.detail-card--emphasis` class).
- `Span?` — `int`, maps to `.dashboard-span-2` / `.dashboard-span-3` (no span by default).
- `RenderFragment ChildContent` — the body.
- `RenderFragment? Footer` — optional footer strip with action buttons (`.btn .btn-sm .btn-outline`, right-aligned).

Semantic variants through child content:

- **Number card** — a big number + label. Wrap in `<div class="dashboard-metric">`. Add `.dashboard-metric` / `.dashboard-metric-value` / `.dashboard-metric-label` to `app.css`.
- **List card** — a short `<ul class="list-unstyled">` of up to 5 items, each a small inline icon + label + link. Sixth item is "View all →" linking to a full list page (from T010/T019 list pages).
- **Progress card** — a stacked bar of per-EPA completion using `.progress-bar` + `.progress-bar-fill` (add these too, CSS only, no animation). Each row: EPA title + bar + `<span class="muted">x / y</span>`.
- **Empty card** — if the widget's query returns nothing, pass `.detail-card--empty` through the card header instead of the default. Render as `"Nothing to review yet."` plus an optional CTA.

Classes to add to `app.css` under "Cards" / "Dashboard grid":

```css
.dashboard-metric { display: flex; flex-direction: column; gap: var(--space-xs); }
.dashboard-metric-value { font-size: 2rem; font-weight: 700; color: var(--primary-color); line-height: 1; }
.dashboard-metric-label { font-size: 0.9rem; color: var(--muted-text); }

.progress-bar { height: 0.5rem; background: var(--hover-bg); border-radius: 4px; overflow: hidden; }
.progress-bar-fill { height: 100%; background: var(--secondary-color); }
.progress-bar-fill.is-complete { background: var(--success-color); }

.detail-card--emphasis { border-left: 4px solid var(--secondary-color); }
.detail-card--warning  { border-left: 4px solid var(--warning-color); }
```

Update `../DESIGN.md § Cards` in the same commit to list these.

## What to do — dashboards by role

Every dashboard sends **one** `*DashboardSummaryQuery` via MediatR and renders the resulting DTO into `DashboardCard`s. The query handler does the aggregation; the Razor component does no business logic. Every widget below is one card in the grid.

### 1. `TraineeDashboard.razor` (`Trainee`, `PendingTrainee`)

For `PendingTrainee`:

- Emphasis card, full-width (`Span=3`), title "Awaiting admission". Body: "You are registered and waiting to be admitted to a curriculum by your programme administrator." Footer: link to `Profile` — "Complete your profile →". Icon: `info`.

For `Trainee`:

- **Curriculum progress** (`Span=2`, emphasis) — progress card with one `.progress-bar` per required EPA in the active curriculum. Colour flips to `.is-complete` when the count hits the window.
- **Activity inbox** — list card showing the first 5 activities in the trainee's `ActivityInbox` from T019 (requested/accepted reviews, draft STAR, declined STAR, etc.). Footer CTA: "Open inbox →" linking to `/activities/inbox`.
- **Recent activities** — list card of the last 5 activities the trainee created, with their current state as a trailing badge (`.badge .badge-@state`). Footer: "All activities →".
- **Upcoming deadlines** — list card of items with a `due_date` field in their schema that falls within 14 days. Uses a jsonb-path projection in the query handler.
- **Actions** — empty-state card with two primary buttons: "Log an activity" (`/activities/new`), "Request an assessment" (shortcut to `/activities/new?type=mini_cex` once T020 seeds the canonical type).

Add `.badge` + state variants to `app.css` (section "Utilities" or a new "Badges" block — add the heading in `DESIGN.md`):

```css
.badge { display: inline-block; padding: 0.15rem 0.5rem; border-radius: 999px; font-size: 0.75rem; font-weight: 600; }
.badge-draft     { background: var(--hover-bg);   color: var(--muted-text); }
.badge-submitted { background: var(--info-bg);    color: var(--secondary-color); }
.badge-accepted  { background: var(--warning-bg); color: var(--warning-color); }
.badge-completed { background: var(--success-bg); color: var(--success-color); }
.badge-declined  { background: var(--danger-bg);  color: var(--danger-color); }
```

### 2. `AssessorDashboard.razor` (`Assessor`)

- **Pending requests** — metric card. Number = count of activities in `requested` state naming the assessor. Primary CTA: "Review inbox →".
- **Accepted, needing action** — list card of activities in `accepted` state for this assessor. Overdue items (older than N days, configurable in `appsettings.json` under `DashboardThresholds:AssessorDueDays`, default 7) render with `.detail-card--warning` on the card + a `.badge-accepted` on the row.
- **Recent decisions** — list card of the last 10 activities this assessor has transitioned to a terminal state, with their decision summary.
- **Actions** — empty card with "Open my inbox →" button.

### 3. `CoordinatorDashboard.razor` (`Coordinator`)

- **Stalled requests** — list card of activities in `requested` state older than `DashboardThresholds:CoordinatorStallDays` (default 7). Each row: subject name + requested date + "Nudge" button (dispatches `SendActivityNudgeCommand` from T024 when that task lands; until then, the button is disabled with a tooltip).
- **Invitations nearing expiry** — list card of invitations within 3 days of expiring. Row action: "Resend invitation".
- **Quick action** — empty card with a primary "Issue invitation" button routing to `/admin/invitations/new`.

### 4. `SpecialityAdminDashboard.razor` / `SubSpecialityAdminDashboard.razor`

- **Pending reviews** — metric card with count of STAR / reflection activities in a review state. CTA: "Review queue →".
- **Trainees in programme** — metric card. Subtitle: "active / inactive" split.
- **Curriculum coverage summary** — progress card aggregated across all trainees: per-EPA average completion.
- **Recent publications** — list card of `ActivityType`s published in the last 30 days (from T025 audit log).

### 5. `InstitutionalAdminDashboard.razor` (`InstitutionalAdmin`)

- **Users** — metric card, count by role in a small stacked list.
- **Specialities + sub-specialities** — metric card with two numbers.
- **Quick links** — empty card, buttons linking to Admin → Institutions / Users / Specialities.

### 6. `CommitteeMemberDashboard.razor` (`CommitteeMember`)

- **Trainees approaching completion** — list card of trainees whose curriculum progress crosses a configurable threshold (default 80%).
- **Programme overview** — progress card aggregated across all trainees (read-only, no edit affordances anywhere).

### 7. `AdministratorDashboard.razor` (`Administrator`)

- **System health** — list card. Rows: DB connection status (from a cheap `SELECT 1` via the health-check service), email queue depth (from T012), last nightly job time (from T024). Each row has a `.status-dot`.
- **Users across institutions** — metric card.
- **Maintenance** — list card with links to `/admin/seed`, `/admin/audit`, `/admin/activity-types`.

Add `.status-dot` + variants to `app.css`:

```css
.status-dot { display: inline-block; width: 0.6rem; height: 0.6rem; border-radius: 50%; margin-right: var(--space-sm); vertical-align: middle; }
.status-dot.ok   { background: var(--success-color); }
.status-dot.warn { background: var(--warning-color); }
.status-dot.err  { background: var(--danger-color); }
```

## Router

`DashboardRouter.razor` is mounted at `/` and replaces the T010 `Home.razor` stub. It inspects the current user's roles and renders the highest-priority dashboard component inline (not via navigation — same URL, different body). Priority order matches the role hierarchy table in `../DOMAIN.md`, top-down:

```
Administrator → InstitutionalAdmin → SpecialityAdmin → SubSpecialityAdmin
→ CommitteeMember → Coordinator → Assessor → Trainee → PendingTrainee
```

A user who holds two roles (e.g. Trainee + Assessor) sees the higher-priority dashboard by default, with a small banner at the top of `PageHeader.Subtitle`: "You also act as {other role}. Switch view →". That link flips a cookie `wombat_preferred_dashboard_role` and reloads. Store the cookie name and accepted values in a constant `Wombat.Web.Navigation.DashboardPriority` — do not spread them.

## Queries

Each dashboard sends exactly one query. Add the query and handler under `Wombat.Application/Features/Dashboards/{Trainee,Assessor,Coordinator,SpecialityAdmin,SubSpecialityAdmin,InstitutionalAdmin,CommitteeMember,Administrator}/Get{Role}DashboardSummaryQuery{,Handler}.cs`.

Each query returns a flat DTO with one field per widget (e.g. `TraineeDashboardSummaryDto { CurriculumProgress, Inbox, RecentActivities, UpcomingDeadlines }`). Do **not** split into N queries per dashboard — the initial render must hit the DB at most twice (one for the summary DTO, one for the identity / role resolution). Keep it under 200ms on a seeded dataset.

Projections read the `Activity.Data` jsonb column via the helpers in T018. **Do not** materialise the whole activity into a typed object — pull exactly the fields the widget needs. Follow the pattern in `src/Wombat.Application/Features/Activities/Queries/*` from T018.

## Files created

- `src/Wombat.Web/Components/Shared/DashboardCard.razor` (+ bUnit test)
- `src/Wombat.Web/Components/Pages/Dashboards/DashboardRouter.razor`
- `src/Wombat.Web/Components/Pages/Dashboards/TraineeDashboard.razor`
- `src/Wombat.Web/Components/Pages/Dashboards/AssessorDashboard.razor`
- `src/Wombat.Web/Components/Pages/Dashboards/CoordinatorDashboard.razor`
- `src/Wombat.Web/Components/Pages/Dashboards/SpecialityAdminDashboard.razor`
- `src/Wombat.Web/Components/Pages/Dashboards/SubSpecialityAdminDashboard.razor`
- `src/Wombat.Web/Components/Pages/Dashboards/InstitutionalAdminDashboard.razor`
- `src/Wombat.Web/Components/Pages/Dashboards/CommitteeMemberDashboard.razor`
- `src/Wombat.Web/Components/Pages/Dashboards/AdministratorDashboard.razor`
- `src/Wombat.Application/Features/Dashboards/**/Get{Role}DashboardSummaryQuery.cs` + `Handler.cs` + `Dto.cs` (8 of each)
- `tests/Wombat.Application.Tests/Dashboards/**` — one test class per query handler, covering empty / partial / full data
- `tests/Wombat.Web.Tests/Dashboards/DashboardCardTests.cs` — bUnit test asserting `.detail-card`, emphasis stripe, span, and footer render correctly
- `tests/Wombat.Web.Tests/Dashboards/DashboardRouterTests.cs` — asserts priority resolution for single-role and multi-role users
- `src/Wombat.Web/wwwroot/app.css` — additions: `.dashboard-metric*`, `.progress-bar*`, `.badge*`, `.status-dot*`, `.detail-card--emphasis`, `.detail-card--warning`
- `../DESIGN.md` — updated in the same commit to document all new classes

## Verification

- [ ] `dotnet build` clean.
- [ ] `dotnet test` green, including the new query-handler tests and the bUnit card/router tests.
- [ ] `../DESIGN.md` is updated with the new dashboard-specific classes and reviewed.
- [ ] `grep -R '<style>' src/Wombat.Web/Components/Pages/Dashboards` returns nothing.
- [ ] Manual walkthrough — **the T011 acceptance script**:
  1. Log in as seeded Trainee. Land on `/`. See `TraineeDashboard` inside `.dashboard-grid` with the five cards above. Curriculum-progress card shows real progress for the seeded trainee's activities. Skeletons appear for <100ms then real data lands with no layout shift.
  2. Resize to 800px wide. Cards collapse to a single column. Emphasis card is still visible at the top.
  3. Log in as seeded Assessor. Dashboard flips to `AssessorDashboard`. Pending requests metric matches the count of seeded `requested` activities naming them.
  4. Log in as seeded Administrator. Dashboard flips to `AdministratorDashboard`. System-health card shows all three rows with green `.status-dot.ok`.
  5. Create a test user with both `Trainee` and `Assessor` roles. Log in. See `AssessorDashboard` by default (higher priority). Click the "switch view" link in the subtitle — page reloads, now shows `TraineeDashboard`. Cookie `wombat_preferred_dashboard_role` is set.
  6. Open the network tab. Each dashboard initial render issues at most two queries: the auth state resolution and the summary query. No N+1.
- [ ] Performance: `GetTraineeDashboardSummaryQuery` returns in under 200ms on a seeded dataset with 100 trainees, 500 activities, 20 curriculum items. Measured via a cold-start `dotnet run` + a warm request.
- [ ] Every dashboard card root element has `class="detail-card …"` — bUnit test catches any drift.

## Notes & gotchas

- **Curriculum progress is the trickiest piece.** For each `CurriculumItem`, count the trainee's completed activities (activities whose type is tagged in the item's `activity_type_keys` list and whose terminal state matches the credit rule) within the item's `WindowMonths`, with the additional constraint that any required scale response meets the `MinimumLevelOrder`. Spec this as a method on `Curriculum` (or a `CurriculumProgressService` under `Wombat.Application/Features/Curricula/Services/`) so tests can cover it without a DB.
- **Dashboards aggregate.** One query per dashboard is the rule. If you find yourself wanting two queries, collapse them. If the DTO grows too wide, split along widget boundaries but still hit the DB once.
- **No business logic in Razor.** Components render what the query returns and dispatch commands on user action. Any conditional rendering logic beyond trivial `@if (x)` lives in the DTO as a pre-computed bool.
- **Skeleton placeholders prevent content-shift.** The `StatePanel` from T010 handles this; use it everywhere rather than open-coding `@if (IsLoading) { "Loading…" }`.
- **Every card must handle its own empty state.** "No pending requests" is not the same as "query failed". The former renders the card's `.detail-card--empty` variant with a friendly message. The latter propagates to `StatePanel.LoadError` and shows an `Alert Kind="danger"`.
- **The activity inbox card is the single hand-off between T019 and T011.** It must consume `ListActivitiesByActorInboxQuery` verbatim. If the shape of that query changes, update T011 (and the card rendering) instead of adding a parallel query.
- **Priority routing**: put the priority order in the `DashboardPriority` constant so tests can assert it without string-matching.
- **Role switch cookie**: `SameSite=Lax`, `Secure` in production, 30-day expiry. No PII — just the role enum value.
- **`IScopedSender`** as always. Never `ISender` in an interactive component.
- **Dashboards are the highest-visibility surface for T010 drift.** If a dashboard card looks "off" compared to the ClinicAssist reference, the problem is almost certainly in T010's `app.css` rather than here. Fix the design system, not the dashboard.
