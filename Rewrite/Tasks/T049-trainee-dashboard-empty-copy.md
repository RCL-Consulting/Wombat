# T049 — Clarify trainee-dashboard curriculum-progress empty-state copy

Follow-up item #3 from the post-T047 backlog (now item #2 after T048): "Trainee dashboard 'No curriculum items assigned yet'. The claims fix in T046 did not populate this."

## Investigation

Not a bug. The rendering is correct — the copy is misleading.

`GetTraineeDashboardSummaryQuery.cs` line 47–60 reads `CurriculumItemProgress` rows where `TraineeUserId == userId && CurriculumItem.CurriculumId == profile.CurriculumId`. Those rows are **lazily created by `CreditApplier.ApplyAsync`** (`src/Wombat.Infrastructure/Activities/CreditApplier.cs:58-68`) only when a terminal-state activity's data credits a curriculum item. A brand-new trainee with zero completed activities correctly has zero progress rows.

The dashboard then showed "No curriculum items assigned yet." — which is wrong in two ways:

1. Curriculum items ARE assigned to the trainee via `TraineeProfile.CurriculumId` → `Curriculum` → `CurriculumItems`. The Demo curriculum ("IM Core Curriculum 2026.1") has 1 CurriculumItem (EPA-001) attached to the seeded trainee.
2. The empty-state actually reflects "no progress data yet", not "no items exist".

No backend fix needed. Behavior is correct by design: progress rows are computed, not pre-seeded.

## Change

`src/Wombat.Web/Components/Pages/Dashboards/TraineeDashboard.razor` line 25:

```
- <p class="muted">No curriculum items assigned yet.</p>
+ <p class="muted">No curriculum progress yet. Complete and submit activities to start tracking.</p>
```

New copy:
- Describes the actual state (no progress, not no items).
- Gives a clear next action (complete and submit activities).
- Fits the rest of the dashboard's tone.

## Verification

Browser-reloaded `/` as `trainee@wombat.local`. Curriculum progress card now reads:

> No curriculum progress yet. Complete and submit activities to start tracking.

The other dashboard cards also populated this session:
- Activity inbox: 2 Reflective Note drafts (from T046 verification).
- Recent activities: same 2 drafts listed.
- Upcoming deadlines: "No deadlines in the next 14 days."
- My authorisations: placeholder (no STAR granted yet).
- Actions: Log an activity / Request an assessment.

h1 focus-ring gone (T048 composing with this change cleanly).

## Out of scope

- Changing the query to left-join CurriculumItems and surface them at zero. That's a product/UX decision — "should empty progress be visible or hidden?" — that should be raised with whoever owns the trainee dashboard. Not attempted here.
- Eagerly seeding CurriculumItemProgress rows per trainee on admission. Same reason: a product/UX decision, not a bug.

## Definition of done

- Empty-state copy is accurate.
- Build clean, Web tests pass (38/38).

## Files touched

- `src/Wombat.Web/Components/Pages/Dashboards/TraineeDashboard.razor`
- `Rewrite/Tasks/T049-trainee-dashboard-empty-copy.md` (this file)
- `Rewrite/current_state.md`
