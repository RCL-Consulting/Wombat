# Current state

This file is the live handoff between sessions. Every session ends by editing this file. Keep it short and accurate.

## Active task

**None. T046 closed seed-claims + a second bug surfaced during verification.**

Recommended next items, in rough priority order — pick one and open a new task file in `Rewrite/Tasks/` before starting:

1. **Operational deployment (carried from T016).** Execute `deploy/README.md` against a real Linode server, configure DNS + TLS, set production secrets, seed. **Suggested model:** Opus — first-time infra work with no playbook yet.
2. **Remaining Bootstrap utility drift in AuditDetail + RequestDetail.** `text-sm`, `text-muted`, `mt-4`, `mt-1` still undefined. Cosmetic — doesn't break structural rendering. Either define the utilities (trivial — one-line rules each) or strip them from the razor files. Deferred from T043. **Suggested model:** Sonnet, ~1 hour.
3. **h1 focus-ring rectangle on initial render.** Pre-existing cosmetic issue noted since T037. Decide intent (screen-reader announcement vs unwanted styling) before suppressing.
4. **Trainee dashboard "No curriculum items assigned yet".** The claims fix in T046 did not populate this. `GetTraineeDashboardSummaryQuery` likely joins to activities/evidence beyond the profile-curriculum link; probably needs at least one activity rated against a curriculum item before it counts as progress. Worth a quick look the next time someone's in the dashboard query area. Not urgent.

`Rewrite/PLAN.md` is otherwise complete. The practical-plan and gui-review-plan are both closed.

## This session at a glance

**T046 — Seed-claims gap + ActivityService draft-create bug** (commit `cef4efc`). Closed backlog item #2 and unblocked trainee activity creation end-to-end. Two related fixes:

- **DevUserSeeder** now writes `WombatIdentityUserSpecialityScope` + `WombatIdentityUserSubSpecialityScope` rows for the seeded trainee and committee users. Previously only `InstitutionId` was set, so `WombatUserClaimsPrincipalFactory` produced principals with zero speciality/sub-speciality claims → every scope-filtered query returned empty for those users. The idempotent ensure-scopes helper tops up existing users on next startup too.
- **`ActivityService.CreateDraftAsync`** was missing `.Include(Versions)` on the ActivityType query. `Map(activity)` calls `GetPinnedVersion` which reads `ActivityType.Versions`; without the Include the collection was empty and Map threw "published activity type version '1' could not be found". One-line fix; the Update/Transition/Get paths already did this via `LoadActivityAsync`.

Both bugs surfaced during verification of backlog item #2. First DevUserSeeder fix unblocked the `/activities/new` type selector. Then the trainee's Save draft revealed the ActivityService bug. Second fix unblocked the persist. Then populated `/activities/2` rendered cleanly (PageHeader + Summary aside + Activity details with schema-driven data).

**Populated ActivityView verification: complete.** Together with T045 (ReviewDetail), this closes both halves of the original "populated rendering" backlog item.

Standing follow-up noted: trainee dashboard "No curriculum items assigned yet" — needs evidence beyond the profile-curriculum link. Cosmetic; flagged in backlog item #4.

**T045 — Populated rendering verification** (commit `d97eb9a`). Closed backlog item #2 and surfaced one real bug + one new upstream follow-up.

- **Bug fixed:** `AuditPayloadSerializer` reflected over MediatR command properties and called `JsonSerializer.Serialize` on each value. Committee commands carry an `Actor: ClaimsPrincipal`, and `ClaimsPrincipal.Claims[].Subject` is a self-referencing graph → `System.Text.Json` cycle exception aborted every committee write path (CreateDecisionPanel, ScheduleCommitteeReview, StartCommitteeReview, etc.). Fix: type-check for `ClaimsPrincipal`/`ClaimsIdentity` and substitute `[PRINCIPAL]`. Actor identity is already captured on `AuditEntry` separately. Regression test added. This bug had been in place since the committee feature shipped — it's exactly why T039 deferred populated ReviewDetail verification ("seeding fails").
- **Populated ReviewDetail verified:** Seeded panel "T045 Test Panel" → scheduled review against the trainee → opened `/committee/reviews/1`. All 6 sections render correctly in Scheduled state (Review summary with T043's `details-list` applied, Decision form inactive, Pending entrustments empty, Evidence snapshot empty, Rating trajectory empty, Appeals form). Clicked **Start review** → success Alert routed through `_status`, state flipped to InProgress, Record decision button appeared on the Decision card, Stage pending decision form appeared. Conditional block rendering confirmed across two states.
- **Populated ActivityView blocked** by a separate upstream issue — `/activities/new`'s type selector is empty for every seeded user because `ListActivityTypesQuery` filters by `specialityIds.Contains(...)` and the seeded users have no such claims. Not a T041 regression; a seed-pipeline claims gap. Opened as new backlog item #2.

**T044 — Dashboard composition documented** (commit `5a4491f`). Closed backlog item #1 by updating `Rewrite/DESIGN.md`:

- Replaced the "Dashboard page" pattern section with a composition description: `Home.razor` is the one routed page at `/` and owns the `<PageHeader>`; role dashboards are child components under `Components/Pages/Dashboards/` and must NOT declare their own `@page` / `<PageTitle>` / `<PageHeader>`; `/dashboard/switch/{role}` is a cookie-setting redirect, not a routed surface.
- Added the dashboard file skeleton (StatePanel + dashboard-grid + DashboardCard).
- Documented the inline-style policy: token-backed `style="..."` on per-instance list items and progress-bar fills is acceptable; promote to a named utility only when the pattern surfaces in 4+ dashboards. This downgrades the secondary "dashboard inline-style" follow-up into standing policy.
- Added `DashboardCard.razor` to the shared-files tree and updated the "Dashboard layout grid" section to reference `<DashboardCard>` as the primary card building block (previously said raw `.detail-card`).

Zero Blazor source changes. Zero `app.css` changes. The "dashboards lack PageHeader" observation from the T041/T042 audit was a category error — every rendered dashboard page already has a header via Home; the file-level absence is the correct composition.

**T043 — Orphan CSS classes done** (commit `3b87eee`). Three new utilities defined + three swaps to existing ones:

- **Defined in `app.css`**: `.details-list` (div-wrapped dt/dd rows for key-value display in committee + portfolio), `.detail-list` (flat dt/dd siblings for admin detail pages), `.stack-list` (vertical card stack primitive). Each has a responsive breakpoint that collapses to single-column at ≤640.98px.
- **Swapped to existing utilities**: `stack-card` → `detail-card detail-card--compact` (3 usages, committee flow); `detail-grid` → `details-grid` (4 usages, admin detail pages); `detail-section` → `detail-card` (5 usages, same files).
- **Task file**: `Rewrite/Tasks/T043-define-orphan-css-classes.md` records scope and rationale.

**Browser verification:** AuditDetail with 14 seeded audit entries renders cleanly — 2-column `details-grid`, bordered `detail-card` wrappers, `detail-list` dt labels aligned left in muted color with dd values in the adjacent grid column. All three swapped classes now apply correctly with real data. RequestDetail uses identical markup structure, so the proof carries.

**Not fixed (flagged for item 4 above):** `text-sm`, `text-muted`, `mt-4`, `mt-1` still undefined in AuditDetail + RequestDetail. Cosmetic leftover from copy-pasted Bootstrap markup; structural rendering is unaffected.

## Previous session

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

## Systemic follow-ups (carried forward)

- **~~Orphan list/dl helper classes in `app.css`.~~** Fixed in T043 (`3b87eee`).
- **~~Dashboards lack `<PageTitle>` / `<PageHeader>`.~~** Documented in T044 as the intended composition pattern (Home.razor owns the header).
- **~~Dashboard inline `style="..."` for flex layouts.~~** Standing policy documented in DESIGN.md by T044: token-backed per-instance inline styles are fine; consolidate only when a pattern surfaces in 4+ dashboards.
- **~~Seed-pipeline claims gap.~~** Fixed in T046 (`cef4efc`).
- **Remaining Bootstrap utility drift** (`text-sm`, `text-muted`, `mt-4`, `mt-1`) in AuditDetail + RequestDetail. See item 2 above.
- **h1 focus-ring rectangle on initial render.** Pre-existing since T037. See item 3 above.
- **Trainee dashboard curriculum progress stays empty.** T046 unblocked claims but `GetTraineeDashboardSummaryQuery` needs more than the profile-curriculum link. See item 4 above.
- **Blazor default `#blazor-error-ui`** uses emoji and raw colors (standard template).
- **`ChangePassword.razor`** uses raw form markup instead of `FormField`. Consistency follow-up.

## Last completed

**T046 — Seed-claims gap + ActivityService Versions include fix** (commit `cef4efc`).

Two fixes that together unblocked trainee activity creation end-to-end. DevUserSeeder now stamps `WombatIdentityUserSpecialityScope` + `WombatIdentityUserSubSpecialityScope` rows (idempotent — tops up existing users). ActivityService.CreateDraftAsync now `.Include(Versions)` — the Map/GetPinnedVersion path was throwing otherwise. Populated `ActivityView` verified in the browser.

271/271 tests pass (unchanged count — no new tests added).

## Previous session

**T045 — Populated rendering verification + audit-serializer fix** (commit `d97eb9a`).

One bug found and fixed (audit-summary JSON cycle on `ClaimsPrincipal`), one seeding blocker verified (seeded Review → populated ReviewDetail renders cleanly in both Scheduled and InProgress states), one new upstream finding surfaced (seed-claims gap blocks `ActivityView` populated verification via a separate code path).

Tests: 270/270 → 271/271 pass (new regression test added for the serializer).

## Previous session

**T044 — Dashboard composition pattern documented** (commit `5a4491f`).

Doc-only change to `Rewrite/DESIGN.md`:
- "Dashboard page" pattern section rewritten as composition: Home owns the PageHeader; role dashboards are child components without their own header.
- Inline-style policy documented (token-backed + per-instance is fine; consolidate at 4+ repetitions).
- `DashboardCard.razor` added to the design-system file tree; "Dashboard layout grid" section updated to reference it.

Zero code changes; tests unaffected.

**T043 — Orphan CSS helper classes** (commit `3b87eee`).

Defined 3 new utilities in `app.css`:
- `.details-list` — `<dl>` with div-wrapped `<dt>` + `<dd>` rows (used in committee + portfolio).
- `.detail-list` — `<dl>` with flat `<dt>` / `<dd>` siblings (used in admin detail pages).
- `.stack-list` — vertical card stack primitive.

Swapped 3 more orphans to existing defined utilities:
- `stack-card` → `detail-card detail-card--compact` (3 usages across ReviewDetail + MyReviews)
- `detail-grid` → `details-grid` (4 usages across AuditDetail + RequestDetail)
- `detail-section` → `detail-card` (5 usages across AuditDetail + RequestDetail)

Verification:
- AuditDetail browser-verified with populated seed data (14 audit entries). The 2-column details-grid, bordered detail-card wrappers, and detail-list dt/dd alignment all render correctly. RequestDetail uses identical markup — proof carries.
- Build clean, 270/270 tests pass (Domain 45, Application 168, Architecture 19, Web 38).

## Test status at handoff

- `dotnet build Wombat.sln -c Release` — zero errors, zero warnings
- Domain tests — 45/45 pass
- Application tests — 169/169 pass
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

- `cef4efc` — T046 (fix seed-claims gap in DevUserSeeder + Versions include in ActivityService.CreateDraftAsync; populated ActivityView verified)
- `e886d10` — docs: record T045 findings, update backlog
- `d97eb9a` — T045 (fix ClaimsPrincipal cycle in audit-summary serializer; populated ReviewDetail verified)
- `e434ec2` — docs: record T044 commit hash
- `5a4491f` — T044 (document dashboard composition pattern in DESIGN.md — docs-only)
- `65b48d7` — docs: record T043 orphan-CSS fix + drop item from backlog
- `3b87eee` — T043 (define orphan CSS helpers — details-list/detail-list/stack-list + 3 swaps to existing utilities)
- `5169466` — docs: close T037-T042 GUI review, record follow-up backlog
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
