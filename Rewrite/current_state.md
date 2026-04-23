# Current state

This file is the live handoff between sessions. Every session ends by editing this file. Keep it short and accurate.

## Active task

**None. GUI review sequence (T037–T042) complete.**

The six-cluster design-system audit of ~65 Blazor pages is done. Recommended next items, in rough priority order — pick one and open a new task file in `Rewrite/Tasks/` before starting:

1. **Orphan list/dl helper classes in `app.css`.** `details-list`, `detail-list`, `stack-list`, `stack-card` referenced in committee, portfolio, and admin pages but never defined. Either define minimal rules in `app.css` or swap each usage for a defined utility. Cross-cluster — touches ReviewDetail, MyAuthorisations, AuditDetail, RequestDetail. **Suggested model:** Sonnet, ~half day.
2. **Dashboard design decision.** All seven role dashboards (Administrator, InstitutionalAdmin, SpecialityAdmin, SubSpecialityAdmin, CommitteeMember, Coordinator, Assessor, Trainee) lack `<PageTitle>`/`<PageHeader>` and use inline `style="..."` for flex layouts. Consistent but undocumented. Decide: document the pattern in `DESIGN.md`, or retrofit PageHeader + utility classes globally. **Suggested model:** Opus for the decision, Sonnet for the refactor if chosen.
3. **Operational deployment (carried from T016).** Execute `deploy/README.md` against a real Linode server, configure DNS + TLS, set production secrets, seed. **Suggested model:** Opus — first-time infra work with no playbook yet.
4. **Populated `ReviewDetail` and `ActivityView` browser verification.** T039 and T041 both deferred the populated-data rendering check (requires seeding a review-on-a-panel and submitting an activity respectively). Low-value unless a bug is actually suspected — the mechanical dual-error split is well-tested by now.
5. **h1 focus-ring rectangle on initial render.** Pre-existing cosmetic issue noted since T037. Decide intent (screen-reader announcement vs unwanted styling) before suppressing.

`Rewrite/PLAN.md` is otherwise complete. The practical-plan and gui-review-plan are both closed. The next session should start by picking one of the above, or by the user raising something new.

## This session at a glance

**T042 — Account & auth shell done** (commit `b109e7c`). 12 pages audited, 2 real findings:

- **Profile.razor** — `_errorMessage` drove both an above-fold Alert and `StatePanel.LoadError` (via a ternary that only checked `_loading`). On load failure both surfaces rendered the same message. Split into `_loadError` + `_actionError` — matches the T038/T039/T040/T041 pattern across every dual-error bug the review surfaced.
- **MainLayout.razor** — `class="top-row px-4 auth"` and `class="content px-4"` both referenced `px-4`, which was **never defined** anywhere in `app.css` or any `.razor.css` isolation file. Pure Bootstrap utility leftover. The actual horizontal padding is applied by `MainLayout.razor.css` at `(min-width: 641px)` via `padding-left: 2rem !important; padding-right: 1.5rem !important;` on `.top-row` and `article`. Removed both `px-4` references; browser-verified the padding didn't regress.

**10 of 12 pages rubric-clean on static audit.** Every referenced class (`account-form-container`, `sso-divider`, `sso-providers`, `sso-button`, `password-wrapper`, `state-panel-title`, `state-panel-copy`, `form-group`, `form-actions`, `auth-page-shell`, `auth-page-main`, `validation-summary-errors`) is defined in `app.css`. ReconnectModal uses Blazor's built-in `components-*` classes tied to the server-side reconnection JS — not Wombat CSS, out of scope.

**Not fixed (noted only):**
- `#blazor-error-ui` in MainLayout uses the `🗙` emoji and raw `lightyellow`/`rgba(0,0,0,0.2)` colors. Standard Blazor template scaffolding; pre-existing.
- `ChangePassword.razor` uses raw `<div class="form-group"><label>...<input>` instead of the `FormField` component used elsewhere. Functional; cosmetic consistency follow-up.

## Browser verification

Eight of 12 pages verified on `http://localhost:5080/`. Screenshots in `.playwright-mcp/` (gitignored):

| Page | Context | Result |
|---|---|---|
| `/account/login` | AuthLayout, anonymous | Centered `account-form-container` card with Email / Password / Remember me / Sign in |
| `/account/forgot-password` | AuthLayout, anonymous | Info Alert "Password reset is not wired yet" + Back to sign in |
| `/not-found` | MainLayout, anonymous | PageHeader + "Nothing to show" detail-card, padding preserved after px-4 removal |
| `/` | MainLayout, Administrator | Dashboard renders with correct 2rem horizontal padding — px-4 removal verified |
| `/account/profile` | MainLayout, Administrator | **Changed page** — PageHeader + Account summary + Profile details rendered; no dual-error (happy path) |
| `/account/change-password` | MainLayout, Administrator | PageHeader + 3 password fields with Show buttons + Cancel/Change actions |
| `/access-denied` | MainLayout, any auth | PageHeader + warning Alert + Back to home — `actions-cell` class used for single button, minor style-choice note |
| `/placeholder/users` | MainLayout, Administrator | PageHeader + 3 detail-cards (Planned surface / Next task / Coming soon empty card) |

### T042 known compromises

- **4 of 12 pages not browser-verified.** Register (needs an invitation token to preview), Error (only triggered via unhandled exception path), AuthLayout (only verified indirectly via Login/ForgotPassword), ReconnectModal (only shown on circuit disconnect). All are rubric-clean on static audit and the ones exercised via their hosting pages rendered correctly.

## GUI review sequence — closed

1. ✅ T037 — Consolidate NavMenu icons to Icon.razor (`1d25995`)
2. ✅ T038 — Trainee surface (`88f5cf4`)
3. ✅ T039 — Committee flow (`dd9f892`)
4. ✅ T040 — Admin hierarchy (`2094d9a`)
5. ✅ T041 — Activity platform (`ae1a316`)
6. ✅ T042 — Account & auth shell (`b109e7c`)

Across six clusters:
- **~65 Blazor pages audited** against the DESIGN.md rubric
- **13 shipped fixes** — the recurring theme was the dual-error bug (`_error` driving both Alert and StatePanel.LoadError). Split into `_loadError`/`_actionError` on 8 pages across the clusters. Remaining fixes: NavMenu icon consolidation, orphan class swap (`plain-list` → `list-unstyled`), redundant Alert collapse (AssessorsList), Bootstrap utility removal (MainLayout `px-4`).
- **Clean build, 270/270 tests pass** throughout
- **One systemic finding opened as a follow-up**: orphan list/dl helpers (`details-list`, `stack-list`, `stack-card`, `detail-list`, `plain-list`) referenced but never defined. Touches committee, portfolio, and admin surfaces. See the top of this file for the suggested next task.

## Systemic follow-ups (not opened as tasks yet)

- **Orphan list/dl helper classes in `app.css`.** See item 1 above.
- **Dashboards lack `<PageTitle>` / `<PageHeader>`.** Uniform across 7 role dashboards. See item 2 above.
- **Dashboard inline `style="..."` for flex layouts.** Not a rubric violation but a utility-class pass would be tidier.
- **h1 focus-ring rectangle on initial render.** Pre-existing since T037. See item 5 above.
- **Blazor default `#blazor-error-ui`** uses emoji and raw colors (standard template).
- **`ChangePassword.razor`** uses raw form markup instead of `FormField`. Consistency follow-up.

## Last completed

**T042 — Account & auth shell** (commit `b109e7c`).

Two fixes:
- `Account/Profile.razor` — split `_errorMessage` into `_loadError` (StatePanel) + `_actionError` (Alert) across OnInitializedAsync + SaveAsync.
- `Layout/MainLayout.razor` — remove undefined `px-4` class from the top-row and article wrappers.

Verification:
- 8 of 12 pages browser-rendered cleanly (admin + anonymous). Horizontal padding preserved after px-4 removal.
- Build clean, 270/270 tests pass (Domain 45, Application 168, Architecture 19, Web 38).

## Test status at handoff

- `dotnet build Wombat.sln -c Release` — zero errors, zero warnings
- Domain tests — 45/45 pass
- Application tests — 168/168 pass
- Architecture tests — 19/19 pass
- Web tests — 38/38 pass
- Infrastructure tests — `SeedParseTests` pre-existing parallel-run flakiness; passes in isolation
- Integration tests — Docker-gated; not run locally
- Browser verification — T037 / T038 / T039 / T040 / T041 / T042 (8/12 account+auth+layout pages)

## Plans status

- `Rewrite/gui-review-plan.md` — **closed**. All six clusters complete.
- `Rewrite/practical-plan.md` — **closed**: T035 done, T036 deferred indefinitely.
- `Rewrite/PLAN.md` — complete for the rewrite baseline. Remaining items are the operational deployment block carried from T016.

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

- `b109e7c` — T042 (account & auth shell polish — Profile dual-error split + MainLayout px-4 removal)
- `e6c8ad7` — docs: record T041 commit hash, T042 handoff
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
