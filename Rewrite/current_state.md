# Current state

This file is the live handoff between sessions. Every session ends by editing this file. Keep it short and accurate.

## Active task

**T040 — Admin hierarchy (GUI review cluster 4).** Model: Sonnet.

Sweep the admin list/edit pairs + role-specific admin dashboards against the rubric in `Rewrite/gui-review-plan.md`:

- Institutions (list, edit)
- Specialities, SubSpecialities (4 pages)
- Assessors (list, profile edit)
- Curricula (list, edit, items edit)
- EPAs (list, edit)
- Dashboards: `InstitutionalAdminDashboard`, `SpecialityAdminDashboard`, `SubSpecialityAdminDashboard`, `AdministratorDashboard`

Dev server is working, `admin@wombat.local` / `ChangeThisAdmin123!` can access all admin surfaces. Run it as one sweep — pages are structurally similar. `pwd_DO_NOT_COMMIT.txt` has all seeded credentials.

## This session at a glance

**T039 — Committee flow (GUI review cluster 3) done** (commit `dd9f892`). Three real findings, three mechanical fixes:

- **MyReviews.razor** — single `_error` field drove both an above-fold `<Alert Kind="danger">` and `StatePanel`'s `LoadError`, so a load failure rendered twice. Split into `_loadError` (StatePanel only) and `_actionError` (Alert only), mirroring T038's `MyAuthorisations` fix.
- **ReviewsSchedule.razor** — identical dual-error bug, identical split. "Schedule review" submit failures now surface as Alert above the table; list-load failures go through StatePanel.
- **ReviewDetail.razor** — `plain-list` (5 occurrences) was an orphan class name not defined in `app.css`. Swapped to `list-unstyled` (defined at `app.css:776`). `stack-list`/`stack-card`/`details-list` are also orphan but left as-is — see "Systemic follow-ups" below.

Rubric-clean on audit, no changes needed: `PanelsList.razor` (PageHeader + StatePanel + DataTable), `PanelEdit.razor` (PageHeader + Alert + FormField + FormActions), `CommitteeMemberDashboard.razor` (consistent with other role dashboards).

## Browser verification

Five of six pages verified on `http://localhost:5080/` with screenshots captured (all `.playwright-mcp/` / `.png` are gitignored):

| Page | Role | Result |
|---|---|---|
| `/` (CommitteeMemberDashboard) | committee@wombat.local | Empty states on both DashboardCards render cleanly |
| `/committee/panels` | committee@wombat.local | StatePanel "No decision panels" empty state, PageHeader + New panel action |
| `/committee/reviews` | committee@wombat.local | StatePanel empty state; "Schedule review" toggle opens form with all FormFields |
| `/committee/panels/new` | admin@wombat.local | Full form renders: Panel name, Scope, Institution id, Speciality id, Chair user id, Member user ids, External user ids, Save panel |
| `/committee/my-reviews` | admin@wombat.local | "No decisions yet" StatePanel empty state |
| `/committee/reviews/1` | admin@wombat.local | Error path: single `Alert Kind="danger"` with "The committee review could not be found." — no duplicate rendering, confirming the dual-error split |

### T039 known compromise

- **Populated `ReviewDetail.razor` unverified.** The page renders correctly in the error state, but the decision/pending-entrustment/evidence/trajectory/appeal sections require a seeded review-on-a-panel flow (5+ steps: create panel, schedule review, start, record decision, optionally stage pending decisions, optionally lodge+resolve appeal). Fixes were mechanical (one `class="plain-list"` → `class="list-unstyled"` swap, no markup structure change), and the Web test suite exercises the component; risk of regression from the swap alone is near zero. Flag for manual exercise during T040+ if a committee review is created incidentally.

## Systemic follow-ups (not T039 scope)

Some committee drift turned out to be global, not cluster-specific. Track as cross-cluster follow-ups:

- **Orphan list/dl helper classes in `app.css`.** `details-list`, `detail-list`, `stack-list`, `stack-card` are referenced across committee, portfolio (`MyAuthorisations`), and admin (`AuditDetail`, `RequestDetail`) pages but not defined anywhere in `app.css` or `.razor.css` files. They render as default browser styling (dls and divs with no visual separation). Either add definitions to `app.css` in a dedicated "design tokens top-up" task, or swap each usage for a defined utility class. Out of scope for GUI review clusters since the rubric forbids touching `app.css` tokens.
- **Dashboards have no `<PageTitle>` or `<PageHeader>`.** Uniform across TraineeDashboard, AssessorDashboard, CommitteeMemberDashboard, InstitutionalAdminDashboard, CoordinatorDashboard. The welcome strip in MainLayout provides role context, so this may be deliberate. Decide during T040 whether to make the pattern explicit (add to `DESIGN.md`) or retrofit headers.
- **h1 focus-ring rectangle on initial render.** Pre-existing; noted last session. Still unresolved.

## Possible follow-ups (not opened as tasks)

- Nav-link underline on default `<a>` state — pre-existing in `NavMenu.razor.css`. Decide during T040/T042.
- `TraineeDashboard.razor` and `CommitteeMemberDashboard.razor` inline `style="..."` for flex layouts — reference design tokens, not a violation. Track for a utility-class pass if the pattern appears on more dashboards in T040.
- `GetTraineeDashboardSummaryQuery` returns `CurriculumProgress.Count == 0` for the seeded Trainee despite a linked curriculum. Out of GUI scope; flag for whoever owns the dashboard query.

## Last completed

**T039 — Committee flow** (commit `dd9f892`).

Three fixes:
- `CommitteeDecisions/MyReviews.razor` — split `_error` into `_loadError` (StatePanel) and `_actionError` (Alert).
- `CommitteeDecisions/ReviewsSchedule.razor` — same split.
- `CommitteeDecisions/ReviewDetail.razor` — `plain-list` → `list-unstyled` (5 occurrences).

Verification:
- 5/6 committee pages browser-rendered cleanly (screenshots above).
- Build clean, 270/270 tests pass (Domain 45, Application 168, Architecture 19, Web 38).

## Plan this session works against

`Rewrite/gui-review-plan.md` — design-system audit of ~65 pages + 15 shared components. T037, T038, T039 done; T040 (admin hierarchy) active.

`Rewrite/practical-plan.md` — closed: T035 done, T036 deferred indefinitely.

## GUI review sequence

1. ✅ T037 — Consolidate NavMenu icons to Icon.razor (browser-verified Administrator)
2. ✅ T038 — Trainee surface (all 6 pages browser-verified, including seeded Trainee)
3. ✅ T039 — Committee flow (5/6 pages browser-verified; populated ReviewDetail deferred)
4. T040 — Admin hierarchy (active — 2 d; admin login available)
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
- Browser verification — T037 (Administrator nav), T038 (all 6 trainee-surface pages), T039 (5/6 committee pages)

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
