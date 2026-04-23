# Current state

This file is the live handoff between sessions. Every session ends by editing this file. Keep it short and accurate.

## Active task

**T041 — Activity platform (GUI review cluster 5).** Model: Sonnet.

Polish the schema-driven activity platform's admin-facing surface plus the trainee/assessor runtime. Per `Rewrite/gui-review-plan.md`:

- `Components/Pages/Admin/ActivityTypes/ActivityTypesList.razor`, `ActivityTypeEdit.razor`
- `Components/Pages/Admin/Forms/FormsList.razor`, `FormEdit.razor`
- `Components/Pages/Activities/NewActivity.razor`, `ActivityView.razor`, `ActivityInbox.razor`
- `Components/Pages/AssessorDashboard.razor`, `CoordinatorDashboard.razor`

`admin@wombat.local` covers admin-side surfaces; use the trainee/assessor seeded accounts for runtime pages. `pwd_DO_NOT_COMMIT.txt` has all credentials.

**Estimate:** 2 days. This is the builder-facing cluster so design drift is possible — the schema/workflow/credit JSON editors were T019-series and may have their own conventions.

## This session at a glance

**T040 — Admin hierarchy done** (commit `2094d9a`). 17 pages audited, 3 real findings, 3 mechanical fixes:

- **AssessorsList.razor** — `LoadError="@(null)"` hardcoded while a separate Alert displayed `_errorMessage`. Inconsistent with the other 5 admin list pages (InstitutionsList, SpecialitiesList, SubSpecialitiesList, CurriculaList, EpasList) which all route error through `StatePanel.LoadError` with no above-fold Alert. Collapsed the Alert, routed error through StatePanel to match.
- **AssessorProfileEdit.razor** — on load failure, both the Alert (`_errorMessage`) AND the "Profile unavailable" empty state rendered. Applied the T038/T039 split: `_loadError` drives StatePanel (suppresses empty state on load failure); `_actionError` drives Alert from SaveAsync failures.
- **CurriculumItemsEdit.razor** — classic dual-error: same `_errorMessage` drove both Alert and StatePanel's LoadError, so a load failure rendered the message twice. Same `_loadError`/`_actionError` split applied.

**14 of 17 pages rubric-clean on static audit.** Form edit pages (InstitutionEdit, SpecialityEdit, SubSpecialityEdit, CurriculumEdit, EpaEdit) use `<StatePanel IsLoading=_loading IsEmpty=false>` with no `LoadError` prop, which is correct for their single-above-fold-Alert pattern. List pages (all 5 non-assessor) use `LoadError="@_errorMessage"` with no separate Alert. Dashboards (Administrator, InstitutionalAdmin, SpecialityAdmin, SubSpecialityAdmin) follow the shared dashboard convention (no PageHeader, inline `style="..."` for flex layouts) — this is the pre-existing cross-cluster follow-up, not a T040 violation.

## Browser verification

Seven of 17 pages verified on `http://localhost:5080/` as `admin@wombat.local`, with screenshots captured (gitignored under `.playwright-mcp/`):

| Page | Result |
|---|---|
| `/admin/assessors` | Clean empty state through StatePanel; no stray Alert above |
| `/admin/assessors/edit` (new) | 2-column form, FormFields render, Cancel + Save actions |
| `/admin/institutions` | DataTable with seeded "Demo Institution" row, Edit + Specialities actions |
| `/admin/curricula` | DataTable with "IM Core Curriculum 2026.1" row populated |
| `/admin/curricula/1/items` | **Dual-error fix verified with seeded data** — Existing items card with EPA-001 populated, Add item form below, no Alert duplication |
| `/admin/epas` | DataTable with seeded "EPA-001 — Clerk, assess, and present a general medical admission" |
| `/admin/institutions/1/specialities` | Dynamic subtitle "Institution: Demo Institution (DEMO)", DataTable with seeded "General Medicine" |

### T040 known compromise

- **10 of 17 pages not browser-verified.** Remaining pages (InstitutionEdit / SpecialityEdit / SubSpecialityEdit / CurriculumEdit / EpaEdit / SubSpecialitiesList / 4 admin dashboards) share exact structural patterns with pages that were verified — same shared components, same state-handling shape, no changes applied. Risk of regression is near zero since the 3 fixes were scoped to individual files and no shared components were touched.

## Systemic follow-ups (carried forward, not T040 scope)

- **Orphan list/dl helper classes in `app.css`.** `details-list`, `detail-list`, `stack-list`, `stack-card` referenced across committee, portfolio, and admin pages but never defined. Render as browser-default styling. Track as a dedicated "design tokens top-up" task.
- **Dashboards have no `<PageTitle>` or `<PageHeader>`.** Uniform across TraineeDashboard, AssessorDashboard, CommitteeMemberDashboard, and now all four admin dashboards (Administrator, Institutional, Speciality, SubSpeciality). Consistent pattern — document in `DESIGN.md` or retrofit headers globally. Not a per-cluster fix.
- **Dashboard inline `style="..."` for flex/margin layouts.** Surfaces across every role dashboard reviewed so far. CLAUDE.md permits inline `style="..."` (only `<style>` blocks are banned), so this is a utility-class pass, not a compliance fix. Defer.
- **h1 focus-ring rectangle on initial render.** Pre-existing since T037/T038; still unresolved.

## Last completed

**T040 — Admin hierarchy** (commit `2094d9a`).

Three fixes:
- `Admin/Assessors/AssessorsList.razor` — route `_errorMessage` through `StatePanel.LoadError`, drop the duplicate Alert.
- `Admin/Assessors/AssessorProfileEdit.razor` — split `_errorMessage` into `_loadError` (StatePanel) + `_actionError` (Alert).
- `Admin/Curricula/CurriculumItemsEdit.razor` — same split.

Verification:
- 7 of 17 pages browser-rendered cleanly as admin with populated seed data; dual-error fix visibly verified on `/admin/curricula/1/items`.
- Build clean, 270/270 tests pass (Domain 45, Application 168, Architecture 19, Web 38).

## Plan this session works against

`Rewrite/gui-review-plan.md` — design-system audit of ~65 pages + 15 shared components. T037, T038, T039, T040 done; T041 (activity platform) active.

`Rewrite/practical-plan.md` — closed: T035 done, T036 deferred indefinitely.

## GUI review sequence

1. ✅ T037 — Consolidate NavMenu icons to Icon.razor (browser-verified Administrator)
2. ✅ T038 — Trainee surface (all 6 pages browser-verified, including seeded Trainee)
3. ✅ T039 — Committee flow (5/6 pages browser-verified; populated ReviewDetail deferred)
4. ✅ T040 — Admin hierarchy (7/17 pages browser-verified; 3 dual-error fixes shipped)
5. T041 — Activity platform (active — 2 d)
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
- Browser verification — T037 / T038 / T039 / T040 (7/17 admin pages)

## Known T035 compromises

- **No validation that the date is plausible.** The date picker accepts any `DateOnly?` — a far-future date would be stored as-is. The field is information-only and admin-only, so the risk is cosmetic; add a sanity guard if a programme starts filling in future placeholder dates.
- **No browser-level verification that session.** List/edit wiring is a direct column-through, identical in shape to existing profile fields; trusted from the Application-layer round-trip tests.

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

- `2094d9a` — T040 (admin hierarchy polish — 3 dual-error splits)
- `930081f` — docs: record T039 commit hash, systemic follow-ups, T040 handoff
- `dd9f892` — T039 (committee flow polish — dual-error split + list-unstyled swap)
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
