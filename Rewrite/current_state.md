# Current state

This file is the live handoff between sessions. Every session ends by editing this file. Keep it short and accurate.

## Active task

**None.** T053 just landed. Remaining triage:

1. **T054** — Admin CRUD for `EntrustmentScale` + `EntrustmentLevel` (~6–8h, **Opus**, only true feature gap; independent of T056). **Recommended next** — independent of T056 and closes Step 1.7's workaround.
2. **T056** — InstitutionalAdmin role-power audit, **Option A**: grant institution-scoped admin powers across ~25 pages + handlers + tests (~12–16h, **Opus**). Blocks T051's intended scope.
3. **T051** — Invitation form: First/Last name capture + surface registration URL + dev SMTP tidy (~3h, **Sonnet**, scope bumped; after T056 to avoid double-touching the invitation surface).
4. **T052** — Invitation form: allow `Administrator` role with null institution (~3h, **Opus**, after T056).
5. **Operational deployment (carried from T016).** Execute `deploy/README.md` against a real Linode server, configure DNS + TLS, set production secrets, seed. **Suggested model:** Opus — first-time infra work with no playbook yet.

## This session at a glance

**T053 — Activity-type Metadata: context-aware Scope Id picker** (commit `4aeaa3d`). Replaced the raw numeric `Scope Id` spinbutton with a `<select>` whose options come from the relevant lookup list (institutions / specialities / sub-specialities) based on the selected Scope. Triple-path labels match the EPA + Curriculum edit convention. Scope=Global hides the field entirely; changing scope clears the stale id so an institution-id can't carry over into a speciality picker. Only `ActivityTypeEdit.razor` touched (markup + 3 new lookup lists + projection helpers + scope-change handler). Round-trip verified on the existing `mini_cex_paed` (Scope=Speciality, ScopeId=2) — the picker pre-selects `Kgosi Kgari Teaching Hospital / Paediatrics` cleanly. Build clean, 38/38 Web tests pass.

**T055 — Publish button + post-save redirect on ActivityType edit** (commit `6eaef56`). Two of three originally-bundled items shipped; the third turned out to be a Playwright snapshot-timing false alarm. Touched only `ActivityTypeEdit.razor`. Browser-verified end-to-end.

- `Publish` now renders unconditionally on `/admin/activity-types/{new|id}`, with `disabled` + tooltip "Save a draft to publish." until a draft exists. `Discard draft` still gates on having a draft.
- First `Save draft` on a brand-new type now navigates to `/admin/activity-types/{id}` (SPA-style, `forceLoad: false`). Previously the URL stuck at `/new`, so a refresh wiped the just-saved type from view.
- The "Create X" page-title finding was a false alarm — 5 of 6 admin edit pages already have the correct `IsNew ? "Create X" : "Edit X"` pattern; the play-through screenshots captured a pre-render snapshot. Verified by direct navigation showing "Edit Institution" in the tab title.

Build clean, 38/38 Web tests pass.

**2026-05-24 Act 1 play-through** (commit `d8a7557`). End-to-end Playwright run of every Phase 1.A–1.F step against the T050-corrected scenario. All step `Actual:` / `Gap:` lines populated.

End state in the dev DB: 1 institution (KGK), 1 speciality (Paediatrics), 1 sub-speciality (General Paediatrics), 1 InstitutionalAdmin user (mbatha@kgk), 15 PAED EPAs, 1 curriculum (FCPaed(SA) Part 1 v2026.1) with 15 items, 10 published activity types (Mini-CEX in full; 9 others with minimal valid metadata/workflow).

Six new findings surfaced, two of them hard:

- **Hard:** `InstitutionalAdmin` is locked out of every admin route except `/admin/entrustment-decisions`. Mbatha provisions cleanly but cannot run Phases 1.D–1.F. Bootstrap admin had to take over from Step 1.8 onward. New task **T056** opened (Option A: grant scope-aware admin powers; Option B: revise scenario so bootstrap admin runs the whole thing). Decision blocks T051 + T052.
- **Hard-ish:** dev SMTP config sends to localhost:1025 but Papercut listens on 25 — every invitation email silently dropped. Worked around by `$env:Email__SmtpPort=25`. Fold fix into T051 (preferred: surface the registration URL in UI so SMTP becomes optional).
- **Bug:** `InvitationsList.IssueAsync` discards the raw token returned by `IssueInvitationCommand`. Status message says "The stub sender logged the registration link" — no stub sender exists. Fold into T051.
- **Cosmetic:** Save draft on a new activity type keeps URL at `/admin/activity-types/new` (refresh loses context). Fold into T055.
- **Cosmetic:** page-title bar reads "Create X" after entity is saved on 6 edit pages. Fold into T055.
- **Cosmetic:** activity-types list `Scope` column shows "Speciality" without identifying which one. Standalone follow-up; not pressing.

T051 and T055 scopes bumped to absorb the play-through findings. `scenario-act1-fixes-plan.md` updated with T056 + new estimates (~19–34h total range; T056 dominates).

## Previous session

**T050 — Scenario doc corrections** commit `96104a1`. Absorbed the 2026-05-24 Playwright route-and-surface audit findings into `Rewrite/scenario-paediatrics.md` so Act 1 plays as-written. Phases 1.A and 1.B swap (institution before invitation); Prof Mbatha demotes from global Administrator to `InstitutionalAdmin` (the invitation surface does not expose the Administrator role); Step 1.7 becomes a workaround pointing at T054; Step 1.11.c gets the parser-accepted workflow JSON inline (incl. a one-block reference to the actor DSL grammar); small wording fixes across Steps 1.1, 1.3, 1.8, 1.11.a/b/e, 1.13. Cast row + Phase 1.A preamble + Act 1 goals + outcome-state + time-estimate + handoff sections all updated to match. Companion plan `Rewrite/scenario-act1-fixes-plan.md` (committed earlier in `c07b71a`) carries the code-side gaps as T051–T055.

Zero code changes; tests unaffected. Doc-only.

**Audit pre-commit** (commit `c07b71a`). Recorded the raw Playwright findings inline in each step's Actual:/Gap: lines + wrote the fixes plan. The current `scenario-paediatrics.md` overwrites the Actual:/Gap: text per scenario convention; that audit content lives in git history at `c07b71a` if needed for reference.

**T049 — Clarify trainee dashboard curriculum-progress empty copy** (commit `ec649d5`). Closed the last cosmetic follow-up from the GUI review sweep.

Investigated the "No curriculum items assigned yet" message the seeded trainee sees. Root cause is correct-by-design behavior, not a bug: `GetTraineeDashboardSummaryQuery` reads `CurriculumItemProgress` rows, and those are lazily created by `CreditApplier.ApplyAsync` when a terminal-state activity credits a curriculum item. A brand-new trainee with zero completed activities correctly has zero progress rows. The message was misleading — items ARE assigned via the profile-curriculum link, just no progress accrued yet.

Fixed the copy only: "No curriculum items assigned yet." → "No curriculum progress yet. Complete and submit activities to start tracking." No query change; that's a product/UX call for another time.

**T048 — h1 programmatic-focus ring suppression** (commit `dcf76bb`). Closed the "h1 focus-ring rectangle on initial render" backlog item that had been open since T037.

Root cause: `Routes.razor:17` has `<FocusOnNavigate Selector="h1" />`, which adds `tabindex="-1"` and programmatically focuses the page h1 after every navigation for screen-reader announcement. Chrome's `:focus-visible` heuristic matched on the programmatic focus after a form submit (`h1.matches(':focus-visible')` returned true), so the browser drew its default outline — the black rectangle seen since T037.

First-attempt rule `h1[tabindex="-1"]:focus:not(:focus-visible)` didn't work because `:focus-visible` was matching. Unconditional `outline: none` is correct here: `tabindex="-1"` explicitly removes the element from keyboard tab order, so there is no legitimate keyboard path to focus the h1. The screen-reader announcement is fired by the `.focus()` call itself; the visual ring was never serving any user.

Browser-verified ring-free on `/`, `/admin/audit`, `/admin/institutions`.

**T047 — Utility-class backfill in `app.css`** (commit `f38a880`). Closed the "Bootstrap utility drift in AuditDetail + RequestDetail" backlog item. Discovered the drift was wider than the item implied: `text-sm`, `text-muted`, `mt-1`, `mt-4`, and one orphan `text-danger` were referenced across 7 razor files (DataRights, Admin/DataRights × 2, Admin/Audit × 2, Admin/Jobs × 2). Added 5 one-line rules to the existing Utilities section; `.muted` and `.text-muted` share one rule via a combined selector so the dashboard `.muted` pattern keeps working unchanged. Spacing follows the existing literal-rem convention (matches `.mt-3`, `.mb-3` already in the section); color uses tokens (`var(--muted-text)`, `var(--danger-color)`). Browser-verified on a pre-existing failed audit entry — error message ("text-sm text-muted mt-1") now renders smaller + muted + with small top offset, and the "Payload (raw JSON)" detail-card has the expected 1.5rem top margin. Bonus: the Payload JSON on that entry shows `"principal": "[PRINCIPAL]"`, confirming T045's audit-serializer fix is visible in persisted audit data.

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
- **~~Remaining Bootstrap utility drift~~** (`text-sm`, `text-muted`, `mt-4`, `mt-1`, `text-danger`). Fixed in T047 (`f38a880`). Turned out to span 7 files not 2.
- **~~h1 focus-ring rectangle on initial render.~~** Fixed in T048 (`dcf76bb`).
- **~~Trainee dashboard curriculum progress stays empty.~~** T049 clarified the empty-state copy (`ec649d5`). Behavior is correct — progress rows are lazy; copy was misleading.
- **Blazor default `#blazor-error-ui`** uses emoji and raw colors (standard template).
- **`ChangePassword.razor`** uses raw form markup instead of `FormField`. Consistency follow-up.

## Last completed

**T053 — Activity-type Metadata: context-aware Scope Id picker** (commit `4aeaa3d`). Single-file change in `ActivityTypeEdit.razor`. Numeric Scope Id spinbutton replaced with cascading-context `<select>`: hidden when Scope=Global, single-level picker for Institution, joined "Institution / Speciality" labels for Speciality, triple-path labels for SubSpeciality. Scope-change handler clears stale id. Round-trip on existing entity verified. Build clean, 38/38 Web tests pass.

**T055 — Publish button + post-save redirect on ActivityType edit** (commit `6eaef56`).

One Razor file touched. Publish renders unconditionally with disabled+tooltip until a draft exists; first Save draft on a brand-new type SPA-redirects to `/admin/activity-types/{id}`. Browser-verified end-to-end. The originally-bundled "Create X" page-title fix was dropped — 5/6 admin edit pages already have the correct conditional `<PageTitle>` and direct navigation shows "Edit Institution" correctly; the play-through screenshots had been a Playwright snapshot-timing race. Build clean, 38/38 Web tests pass.

**2026-05-24 Act 1 play-through** (commit `d8a7557`).

End-to-end Playwright execution of every Phase 1.A–1.F step against the T050-corrected scenario. All step `Actual:` / `Gap:` lines populated. Two hard findings (T056 role-scope; dev SMTP port mismatch), four cosmetic. T051 and T055 scopes bumped to absorb new fixes; new task T056 added to `scenario-act1-fixes-plan.md` with Option A/B for the role-power audit. Zero code; doc-only.

**T050 — Scenario doc corrections** commit `96104a1`.

Doc-only rewrite of `Rewrite/scenario-paediatrics.md` to absorb the 2026-05-24 Playwright audit findings. Phase swap (1.A ↔ 1.B), role demotion (Administrator → InstitutionalAdmin), Step 1.7 workaround, Step 1.11.c JSON correction + actor DSL reference, plus small wording fixes across 9 other steps. Zero code; tests unaffected.

## Previous session

**T048 — h1 programmatic-focus ring suppression** (commit `dcf76bb`).

One CSS rule (`h1[tabindex="-1"]:focus { outline: none; }`) with a comment explaining the Blazor `FocusOnNavigate` context. No code path changes; screen-reader announcement still works. Closes the pre-existing cosmetic issue that has been noted in handoffs since T037.

## Previous session

**T047 — Utility-class backfill** (commit `f38a880`).

Added 5 one-line utilities (`mt-1`, `mt-4`, `text-muted` as alias of existing `.muted`, `text-sm`, `text-danger`) to the existing Utilities section of `app.css`. Zero code/tests impact; pure CSS addition. Closes the last remaining class-drift backlog item from the post-GUI-review survey.

## Previous session

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

- `4aeaa3d` — T053 (context-aware Scope Id picker on activity-type Metadata tab; one Razor file; round-trip verified; build clean, 38/38 Web tests pass)
- `6eaef56` — T055 (always-visible Publish button + post-save URL redirect on ActivityType edit; one Razor file; build clean, 38/38 Web tests pass)
- `1d76c3c` — docs: record d8a7557 commit hash in handoff + plan
- `d8a7557` — docs: Act 1 Playwright play-through findings + T056 raised (every step's Actual/Gap populated; 6 findings; T051/T055 scope bumped; new T056 = InstitutionalAdmin role-power audit)
- `96104a1` — T050 (scenario doc corrections — Phase swap, role demotion, Step 1.7 workaround, Step 1.11.c JSON correction + actor DSL, plus 9 small wording fixes; docs-only)
- `c07b71a` — docs: record Act 1 Playwright audit findings + scenario-act1-fixes-plan.md
- `ec649d5` — T049 (clarify trainee dashboard curriculum-progress empty copy)
- `87b3fdf` — docs: record T048 h1-focus-ring fix, shrink backlog to 2
- `dcf76bb` — T048 (suppress programmatic-focus outline on page h1)
- `448f230` — docs: record T047 utility-class backfill, reprioritise backlog
- `f38a880` — T047 (backfill mt-1/mt-4/text-muted/text-sm/text-danger utilities in app.css)
- `9f7d6f8` — docs: record T046 findings, update backlog
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
