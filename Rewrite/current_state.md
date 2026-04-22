# Current state

This file is the live handoff between sessions. Every session ends by editing this file. Keep it short and accurate.

## Active task

**T039 — Committee flow (GUI review cluster 3).** Model: Sonnet.

Browser-verify and polish the committee surface (densest recent work, T031–T033 all landed here): `Committee/PanelsList`, `PanelEdit`, `MyReviews`, `ReviewsSchedule`, `ReviewDetail`, plus `CommitteeMemberDashboard`. Apply the rubric in `Rewrite/gui-review-plan.md`. Dev server is unblocked and `committee@wombat.local` / `ChangeThisCommittee123!` is now seeded in Development for committee-role verification (see DevUserSeeder under "This session at a glance"). `pwd_DO_NOT_COMMIT.txt` carries the credentials.

## This session at a glance

Practical plan closed. **T036 deferred indefinitely** (no accreditor format spec forthcoming). New plan: `Rewrite/gui-review-plan.md` — six clusters T037–T042, ~8 working days.

Four pieces of work shipped this session:

- **T037** — NavMenu's bespoke `.bi-*-nav-menu` background-image CSS replaced with `Icon.razor`. Added Lucide shield/clock/key SVGs. Browser-verified under Administrator: 13 nav icons render and follow `currentColor`.
- **T028 hotfix** — `RenameStarReflection` migration's SQL referenced a non-existent `ActivityTypes."Title"` column; corrected to `"Name"` in place (data-only migration, never ran successfully anywhere before). This unblocked the dev server, which had been silently broken since T028 landed.
- **T038** — Trainee-surface polish. `ExportPortfolio` and `VerifyExport`: raw `<div class="alert alert-...">` Bootstrap markup swapped for `<Alert Kind="...">`; 4-space indentation normalized to 2-space. `MyProgress` and `MyAuthorisations`: manual loading text + empty cards refactored to `StatePanel` for consistency with `TraineeDashboard`/`MyActivities`. `MyAuthorisations`: split `_error` into `_loadError` (drives StatePanel) and `_downloadError` (above-the-fold Alert) so a download failure no longer blanks the loaded list. `TraineeDashboard` and `MyActivities` already passed the rubric — no changes. **All 6 pages browser-verified** (3 as Administrator, 3 as the seeded Trainee).
- **DevUserSeeder** (commit `e132765`) — new `Wombat.Infrastructure.Identity.DevUserSeeder`, run after `DataSeeder` only when `app.Environment.IsDevelopment()`. Idempotent. Creates `trainee@wombat.local` (Trainee role + TraineeProfile linked to the Demo curriculum) and `committee@wombat.local` (CommitteeMember role). Hardcoded `ChangeThisTrainee123!` / `ChangeThisCommittee123!` passwords — obvious in source so a careless production deploy is loud. Resolves the "non-admin role" gap that T038 surfaced; T039 onwards can browser-verify role-gated pages without rebuilding the invitation flow each cluster.
- **chore** — `.gitignore` hardened against artifacts this session generated: `.playwright-mcp/`, `/*.png`, `pwd_DO_NOT_COMMIT.txt`. The pwd file now also carries trainee + committee credentials alongside the existing admin entry.

## Possible follow-ups (not opened as tasks)

- The h1 on every page (e.g. "Welcome, admin@wombat.local", "My activities") shows a black focus-ring rectangle on initial render — Blazor likely focuses the h1 after navigation for screen-reader announcement. Worth verifying intent before suppressing; if intentional, `h1:focus { outline: none; }` or `h1:focus-visible { outline: none; }` on the inert case.
- Nav-link text shows the default `<a>` underline. Pre-existing in `NavMenu.razor.css`. Decide during a NavMenu follow-up or T040 whether to add `text-decoration: none` to `.nav-item ::deep .nav-link`.
- `TraineeDashboard.razor` uses ~8 inline `style="..."` attributes for flex layouts; each references design tokens (`var(--space-xs)` etc.). CLAUDE.md doesn't ban inline `style="..."`, only `<style>` blocks, so they're not a violation — flagged for a future utility-class pass if the same patterns surface in other dashboards.
- `GetTraineeDashboardSummaryQuery` returns `CurriculumProgress.Count == 0` for the seeded Trainee even though `TraineeProfile.CurriculumId` points to "IM Core Curriculum 2026.1" which has one CurriculumItem. The query is stricter than just the profile-curriculum link — likely needs evidence (Activities) or institutional scope. Out of GUI-review scope; flag for whoever owns the dashboard query.

## Last completed

**T038 — Trainee surface polish** (commit `88f5cf4`) **+ DevUserSeeder** (commit `e132765`).

T038 fixes:
- `ExportPortfolio.razor` and `VerifyExport.razor` — raw `<div class="alert alert-...">` Bootstrap markup swapped for `<Alert Kind="...">`; 4-space indentation normalized to 2-space.
- `MyProgress.razor` and `MyAuthorisations.razor` — manual "Loading…" + empty cards refactored to `StatePanel` for consistency with `TraineeDashboard`/`MyActivities`.
- `MyAuthorisations.razor` — split `_error` into `_loadError` (drives StatePanel) and `_downloadError` (rendered above-the-fold via Alert) so a download failure no longer blanks the loaded list.
- `TraineeDashboard.razor` and `MyActivities.razor` — no changes; both already pass the rubric.

DevUserSeeder closed the role-gap that initially blocked T038 verification:
- Idempotent dev-only seeder. Runs after `DataSeeder` only when `app.Environment.IsDevelopment()`.
- Creates `trainee@wombat.local` (Trainee + TraineeProfile linked to the Demo curriculum) and `committee@wombat.local` (CommitteeMember).
- Hardcoded `ChangeThisTrainee123!` / `ChangeThisCommittee123!` — obvious in source so a careless production deploy is loud.

Verification:
- All 6 T038 pages browser-rendered cleanly: `/activities/mine`, `/portfolio/export`, `/portfolio/verify?hash=…` (warning Alert) under Administrator; `/`, `/portfolio/progress`, `/portfolio/authorisations` under the seeded Trainee.
- Build clean, 270/270 tests pass (Domain 45, Application 168, Architecture 19, Web 38).

Earlier in the same session:
- **T037** (commit `1d25995`) — NavMenu icons consolidated to `Icon.razor`. Browser-verified.
- **T028 migration fix** (commit `a413ddc`) — `ActivityTypes."Title"` → `"Name"` in `RenameStarReflection`. Unblocked the dev server.

## Plan this session works against

`Rewrite/gui-review-plan.md` — design-system audit of ~65 pages + 15 shared components. T037 and T038 done; T039 (committee flow) active.

`Rewrite/practical-plan.md` — closed: T035 done, T036 deferred indefinitely.

## GUI review sequence

1. ✅ T037 — Consolidate NavMenu icons to Icon.razor (browser-verified Administrator)
2. ✅ T038 — Trainee surface (all 6 pages browser-verified, including via the seeded Trainee)
3. T039 — Committee flow (active — 1.5 d; CommitteeMember login seeded)
4. T040 — Admin hierarchy (2 d)
5. T041 — Activity platform (2 d)
6. T042 — Account & auth shell (1 d)

## Block 4 / practical-plan sequence (closed)

1. ✅ T035 — Assessor training status field
2. 🚫 T036 — Accreditor-specific export template (deferred — WBA new locally, no accreditor spec)

## Test status at handoff

- `dotnet build Wombat.sln -c Release` — zero errors, zero warnings
- Domain tests — 45/45 pass
- Application tests — 168/168 pass
- Architecture tests — 19/19 pass
- Web tests — 38/38 pass
- Infrastructure tests — `SeedParseTests` pre-existing parallel-run flakiness; passes in isolation
- Integration tests — Docker-gated; not run locally
- Browser verification — T037 (Administrator nav) and T038 (all 6 pages, mixed Administrator + seeded Trainee) at `http://localhost:5080/`

## Known T035 compromises

- **No validation that the date is plausible.** The date picker accepts any `DateOnly?` — a far-future date would be stored as-is. The field is information-only and admin-only, so the risk is cosmetic; add a sanity guard if a programme starts filling in future placeholder dates.
- **No browser-level verification this session.** List/edit wiring is a direct column-through, identical in shape to existing profile fields; trusted from the Application-layer round-trip tests.

## What remains (operational, not code — carried forward from T016)

- Execute `deploy/README.md` first-boot checklist against a real Linode server
- Configure DNS, TLS certificate (Caddy auto-provisions via ACME)
- Set production secrets in `/opt/wombat/config/wombat.env`
- Run `--seed` to provision the admin user and seeded activity types
- Revoke UPDATE/DELETE on AuditEntries table after first migration

## Companion reference docs

- `EPA Book/evaluation.md` — 92-requirement book scorecard (reference, not todo list)
- `EPA Book/critique.md` — literature-backed reasoning for practical-plan compromises
- `Rewrite/book-fidelity-plan.md` — superseded; kept only because `critique.md` cites it

## Last verified commits

- `e132765` — chore: add DevUserSeeder for non-admin browser verification
- `cde9ee1` — docs: record T038 commit hash + open trainee-account question
- `e7e9abb` — chore: gitignore browser-verification artifacts and pwd file
- `88f5cf4` — T038 (trainee surface polish — Alert + StatePanel consolidation)
- `2b82f7e` — docs: record T037 browser verification + T028 fix in handoff
- `a413ddc` — fix(T028): correct ActivityTypes column name in RenameStarReflection migration
- `ba8d20b` — docs: record T037 commit hash in handoff
- `1d25995` — T037 (consolidate NavMenu icons to Icon.razor; browser-verified Administrator role)
- `ba7c7d8` — docs: defer T036, open GUI review plan (T037–T042)
- `9d60cd7` — T035 (assessor training status field)
- `dd18b66` — T034 (EPA core/elective + stage-indexed supervision levels)
- `9910ba1` — T033 (per-trainee per-EPA trajectory chart)
- `2e02a1e` — T032 (sampling-concentration warning on review detail)
- `ac4fdb9` — T031 (formative-only committee review mode)
- `c9b00d0` — docs: record T030 commit hash
- `10f7e55` — T030 (STAR certificate PDF + authorisations UI)
- `21f7959` — docs: record T029 commit hash in current_state handoff
- `91ff841` — T029 (EntrustmentDecision aggregate / STAR)
- `dc506d1` — T028 (rename `star_reflection` → `reflective_note`)
- `bf583ee` — MailKit 4.16.0 security bump (closes GHSA-9j88-vvj5-vhgr)
- `864ad3b` — T016 (rewrite-complete baseline)
