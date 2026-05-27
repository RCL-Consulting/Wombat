# T066 — Trainee admission: surface Stage on form + list

Finding **A2-8** from the 2026-05-27 Act 2 play-through: the admit-trainee form (`/admin/trainees/edit?userId={...}`) collects only Curriculum + ProgrammeStartDate + ExpectedCompletionDate. There is no `Stage` field — neither a numeric input nor a derived display. The Active profiles list at `/admin/trainees` also omits Stage. The scenario expected explicit Stage assignment per registrar (year 1–4) since the curriculum's per-stage-minimum-level rules depend on it.

Stage is computed elsewhere in the codebase (presumably `(today - ProgrammeStartDate).Years` plus a cap at the curriculum's nominal duration). Two questions for this task:

1. Is the computed Stage already used by the curriculum-progress-applier (CreditApplier)? If yes, the data side is fine — the gap is purely display.
2. Are there programmes where Stage diverges from calendar years (e.g. parental leave, deferral, advanced standing)? If yes, Stage should be persisted and editable, not computed.

Verify both before deciding form-only vs persisted-column.

## Investigation

Look at `src/Wombat.Domain/Trainees/TraineeProfile.cs` + `src/Wombat.Infrastructure/Activities/CreditApplier.cs` for how Stage is resolved. Search for `Stage` references across the codebase. Pull `ProgrammeStartDate` use-sites too.

Findings will drive the choice between Option A (computed display) and Option B (persisted column).

## Option A — Computed Stage display

Cheaper. Add a `Stage` getter on `TraineeProfileDto` that returns `Math.Clamp((today - ProgrammeStartDate).Years + 1, 1, Curriculum.NominalYears)` (or whatever the actual rule is). Surface in:

- `AdmitTrainee` / `UpdateTraineeProfile` view: read-only display below ProgrammeStartDate, e.g. `Stage: 4 (computed from start date)`.
- Active profiles list: new column `Stage`.
- Trainee dashboard's curriculum-progress panel (already exists per T044): verify Stage is rendered there too.

Effort: ~30 min if the computation rule is already implemented; ~1 hour if I need to add it.

## Option B — Persisted Stage column

Required if the codebase has scenarios where stage ≠ calendar years. New nullable `int? Stage` column on `TraineeProfile`. Hand-written migration + Designer + snapshot. Form input + list column + dashboard display. ~2–3 hours.

## Recommendation

Start with Option A unless investigation surfaces a hard requirement for divergence. The scenario's Year-1 / Year-2 / Year-3 / Year-4 assignments are all calendar-aligned and would map cleanly onto computed-from-start. Add Option B later if a real programme needs it.

## Tests

(Option A)
- Unit test on the `TraineeProfileDto.Stage` getter for the boundary cases (year 1 day 1, exact-year anniversary, beyond nominal years).
- bUnit test on the Active profiles list: a seeded trainee with start `2023-01-15` and today `2026-05-27` renders `Stage = 4`.
- bUnit test on the admit form: read-only Stage chip appears with the correct value when ProgrammeStartDate is filled in.

## Browser verification

Visit `/admin/trainees`. The Active profiles table shows Stage 4/3/2/1/1 for Molefe/Dlamini/du Plessis/Mahlangu/Ndlovu — matching the cast.

Open Molefe's edit page: Stage displays as `4 (computed)` below the start-date input.

Sign in as Molefe: her TraineeDashboard shows her stage in the curriculum-progress panel header (e.g. `Stage 4 — Final year`). Curriculum-item progress bars use the per-stage-minimum-level data from `MinimumLevelByStageJson` for stage 4.

## Out of scope

- Stage transitions (programme advancement, deferrals). Phase 4 of the scenario does an "annual review" that conceptually advances stage but currently does nothing to the profile — that's a separate task (the committee-review decision lifecycle).
- Custom curriculum durations (3-year, 5-year, etc.). Existing curriculum has a NominalYears column; check if it's used for the clamp. If not, defer.

## Definition of done

- Build clean, all suites pass.
- Stage column visible in `/admin/trainees` list + admit form + Trainee dashboard.
- Computed-stage formula verified at the boundary cases.
- Scenario doc Act 2 Step 2.7 Actual/Gap updated.

## Files touched (Option A)

- `src/Wombat.Application/Features/Trainees/TraineeProfileDto.cs` — add Stage getter.
- `src/Wombat.Application/Features/Trainees/*` — possibly extend AdmitTraineeCommandHandler / UpdateTraineeProfileCommandHandler to surface Stage in their responses.
- `src/Wombat.Web/Components/Pages/Admin/Trainees/TraineesList.razor` — new column.
- `src/Wombat.Web/Components/Pages/Admin/Trainees/TraineeProfileEdit.razor` — Stage display.
- `src/Wombat.Web/Components/Pages/Dashboards/TraineeDashboard.razor` — Stage in curriculum-progress header (if not already shown).
- `tests/Wombat.Application.Tests/Features/Trainees/*` and `tests/Wombat.Web.Tests/*`.
- `Rewrite/Tasks/T066-trainee-admit-stage-display.md` (this file).
- `Rewrite/scenario-paediatrics.md`.
- `Rewrite/current_state.md`.

## Estimate

~30 min–1 hour for Option A; ~2–3 hours for Option B. **Sonnet.**
