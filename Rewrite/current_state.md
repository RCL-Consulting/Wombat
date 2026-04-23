# Current state

This file is the live handoff between sessions. Every session ends by editing this file. Keep it short and accurate.

## Active task

**T042 — Account & auth shell (GUI review cluster 6).** Model: Sonnet.

Final GUI review cluster. Low-churn, small surface — wrap-up pass per `Rewrite/gui-review-plan.md`:

- `Components/Pages/Account/Login.razor`, `Register.razor`, `ForgotPassword.razor`, `ChangePassword.razor`, `Profile.razor`
- `Components/Pages/AccessDenied.razor`, `Error.razor`, `NotFound.razor`
- `Components/Pages/Home.razor`, `PlaceholderPage.razor`
- Layout: `Components/Layout/MainLayout.razor`, `AuthLayout.razor`, `ReconnectModal.razor` (focus on the reconnect affordance)

**Estimate:** 1 day. Anonymous / lightly-authenticated pages plus the global layout affordances. After this, the full GUI review sequence (T037–T042) is closed.

## This session at a glance

**T041 — Activity platform done** (commit `ae1a316`). 9 pages audited, 2 real findings, both the same dual-error pattern:

- **ActivityTypeEdit.razor** — `_error` drove both an above-fold Alert and `StatePanel.LoadError`. On load failure both the Alert and the "Editor unavailable" empty state rendered. Split into `_loadError` (StatePanel) + `_actionError` (Alert) across all three action methods (SaveDraft, Publish, Discard) plus the OnParametersSetAsync load.
- **ActivityView.razor** — same dual-error; split across OnParametersSetAsync + HandleTransitionAsync.

**7 of 9 pages rubric-clean on static audit.** Every builder class referenced (`tab-bar`, `tab-bar-tab`, `builder-two-col`, `font-mono`, `search-container`, `search-grid`, `search-field`, `search-input`, `header-container`, `page-subtitle`, `detail-card--interactive`, `detail-card--empty-compact`) is defined in `app.css`. `FormEdit.razor` uses `<StatePanel IsLoading=_loading IsEmpty=false>` with no `LoadError` prop — single-Alert pattern, no dual-error. `ActivityInbox.razor` uses LoadError-only, no separate Alert — clean. `NewActivity.razor` has no StatePanel LoadError, only Alerts — clean.

`AssessorDashboard.razor` and `CoordinatorDashboard.razor` follow the shared dashboard convention (no PageHeader, inline `style="..."` for flex item layouts) — pre-existing cross-cluster pattern, not a T041 violation.

## Browser verification

Six of 9 pages verified on `http://localhost:5080/` as `admin@wombat.local`. Screenshots in `.playwright-mcp/` (gitignored):

| Page | Result |
|---|---|
| `/admin/activity-types` | 10 seeded activity types in DataTable (ACAT, CBD, DOPS, Journal Club, Mini-CEX, Procedure Log, QI Project, Reflective Note, Research Output, Teaching Session); search filter renders |
| `/admin/activity-types/1` | **Builder UI fully populated** — Mini-CEX loaded with Request / EPA / Assessment / Document / Feedback sections; Live preview pane renders the schema-driven form; Section + Field editor panels at bottom |
| `/admin/forms` | Clean "No assessment forms yet" StatePanel empty state |
| `/activities/inbox` | "Inbox clear" empty state |
| `/activities/new` | PageHeader + Activity type select renders |
| `/activities/1` | **Dual-error fix verified** — load failure shows a single message via `StatePanel.LoadError`; no duplicate Alert above |

### T041 known compromises

- **3 of 9 pages not browser-verified.** FormEdit and the two dashboards (AssessorDashboard, CoordinatorDashboard) render rubric-clean on static audit but weren't exercised in the browser. Low risk: FormEdit matches other admin edit pages (no changes applied), and the dashboards follow the shared pattern that's been browser-verified in T037/T040.
- **Populated `ActivityView` with real activity unverified.** Would need to seed or submit an activity first. The changed file's happy path uses the same `details-grid` + `detail-card` shell as ReviewDetail (verified in T039) and ActivityTypeEdit's preview pane (verified in this cluster). Risk of regression from the mechanical `_error` → `_loadError`/`_actionError` split is near zero.

## Systemic follow-ups (carried forward, not T041 scope)

- **Orphan list/dl helper classes in `app.css`.** `details-list`, `detail-list`, `stack-list`, `stack-card` referenced in committee, portfolio, and admin pages but never defined. Track as a dedicated "design tokens top-up" task.
- **Dashboards have no `<PageTitle>` or `<PageHeader>`.** Uniform across TraineeDashboard, AssessorDashboard, CommitteeMemberDashboard, CoordinatorDashboard, and all four admin dashboards. Consistent pattern; decide during T042 or after whether to document in `DESIGN.md` or retrofit.
- **Dashboard inline `style="..."` for flex/margin layouts.** Not a rubric violation (CLAUDE.md bans `<style>` blocks only), but a cosmetic utility-class pass would be worth opening as a follow-up task.
- **h1 focus-ring rectangle on initial render.** Pre-existing since T037/T038; still unresolved.

## Last completed

**T041 — Activity platform** (commit `ae1a316`).

Two fixes:
- `Admin/ActivityTypes/ActivityTypeEdit.razor` — split `_error` into `_loadError` (StatePanel) + `_actionError` (Alert) across OnParametersSetAsync, SaveDraft, Publish, Discard.
- `Activities/ActivityView.razor` — same split across OnParametersSetAsync + HandleTransitionAsync.

Verification:
- 6 of 9 pages browser-rendered cleanly as admin with populated seed data; dual-error fix visibly verified on `/activities/1` (single "could not be found" message, no duplicate).
- Build clean, 270/270 tests pass (Domain 45, Application 168, Architecture 19, Web 38).

## Plan this session works against

`Rewrite/gui-review-plan.md` — design-system audit of ~65 pages + 15 shared components. T037 / T038 / T039 / T040 / T041 done; T042 (account & auth shell) active.

`Rewrite/practical-plan.md` — closed: T035 done, T036 deferred indefinitely.

## GUI review sequence

1. ✅ T037 — Consolidate NavMenu icons to Icon.razor (browser-verified Administrator)
2. ✅ T038 — Trainee surface (all 6 pages browser-verified, including seeded Trainee)
3. ✅ T039 — Committee flow (5/6 pages browser-verified; populated ReviewDetail deferred)
4. ✅ T040 — Admin hierarchy (7/17 pages browser-verified; 3 dual-error fixes shipped)
5. ✅ T041 — Activity platform (6/9 pages browser-verified; 2 dual-error fixes shipped)
6. T042 — Account & auth shell (active — 1 d)

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
- Browser verification — T037 / T038 / T039 / T040 / T041 (6/9 activity platform pages)

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

- `ae1a316` — T041 (activity platform polish — dual-error split on ActivityTypeEdit + ActivityView)
- `17fe16d` — docs: record T040 commit hash, T041 handoff
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
