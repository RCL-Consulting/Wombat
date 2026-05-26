# Current state

This file is the live handoff between sessions. Every session ends by editing this file. Keep it short and accurate.

## Active task

**Act 1 replay run + T057 (post-save tab-title + EntrustmentScale write-gate) shipped this session.** Replay (commit `ac53e2f`) re-ran every Phase 1.A–1.F step against the T051/T055/T056-fixed code; closed 4 of 6 previous findings, partially closed #5 (titles), surfaced #7 (UX wart for InstitutionalAdmin on EntrustmentScale create). T057 (commit `d7f695c`) then closed both #5 and #7. Remaining:

1. **T052** — Invitation form: allow `Administrator` role with null institution (~3h, **Opus**). Requires making `Invitation.InstitutionId` nullable — hand-written migration + Designer + snapshot update + handler + provisioner changes + tests. Plan doc in `scenario-act1-fixes-plan.md` spells out the exact file list.
2. **T051.b** — Invitation form: First/Last name capture (entity migration + form + accept pre-fill). Same migration overhead as T052. ~2h, **Sonnet**. Could be bundled with T052 to share the migration cost.
3. **Operational deployment (carried from T016).** Execute `deploy/README.md` against a real Linode server, configure DNS + TLS, set production secrets, seed. **Suggested model:** Opus — first-time infra work with no playbook yet.

**Suggestion:** bundle T051.b + T052 in one session — they touch the same Invitation entity and would share the migration / Designer / snapshot cost. Net effort ~3.5h.

## This session at a glance

**Session 2026-05-26 — Act 1 replay + T057 + T058 fixes shipped, then replay-3 confirmed clean, then Acts 2-5 + Appendix drafted.** Replayed the scenario end-to-end against a freshly reset dev DB now that T051/T055/T056 had landed. 4 of 6 previous findings closed by those task shipments; the replay surfaced 2 still-open findings (#5 stale tab title, #7 EntrustmentScale UX wart for InstitutionalAdmin), which T057 then closed. Finding #6 (Scope column ambiguity) closed by T058. **All 7 findings from the 2026-05-24 / 2026-05-26 sweep now closed.** A third replay was then run to confirm the fixes stay closed under re-run — clean, no new findings, no regressions, plus exercised the Papercut SMTP delivery path (previous replays used the inline URL only). One tiny doc-fix surfaced: scenario inline note said Papercut emails land in an `Incoming` subfolder; they actually land in the parent dir. Finally, Acts 2 (Day-1 onboarding, 248 lines), 3 (Months 1-6 operational rhythm, 234 lines), 4 (annual review + STARs + appeal, 166 lines), 5 (graduation + portfolio PDF, 120 lines), and the Appendix (data rights / scheduled jobs / SSO / mobile + a11y, 122 lines) were drafted as a single doc-only commit — each act ready to play in a future session with the same six-line step convention Act 1 uses.

**Session 2026-05-24 (continued) — T056 complete + T051 (URL+SMTP+message portion) shipped.** Started T056 (InstitutionalAdmin role-power audit, Option A). Mid-implementation realized the full 12–16h sweep wouldn't fit one session cleanly, so split into five clusters and landed all five plus T051's UI/config portion. The Paediatrics scenario Act 1 now plays end-to-end as Prof Mbatha (InstitutionalAdmin) per its original intent.

**Session commits in chronological order:**
- `41def8a` — T056.a: foundations + Institutions/Speciality/SubSpec scope guards
- `4232d22` — docs: record T056.a hash
- `9e3bc0a` — T056.b: EPAs + Curricula scope guards
- `5b06def` — docs: record T056.b hash
- `e1d3737` — T056.c: ActivityTypes + Forms scope guards
- `d08db42` — docs: record T056.c hash
- `8ad0788` — T056.d: Trainees + Assessors + Invitations + EntrustmentScales scope guards
- `3a016d2` — docs: record T056.d hash
- `3c60a71` — docs: update scenario-act1-fixes-plan
- `ec6d6d1` — T056.e: Audit + SSO + NavMenu refresh + scenario-doc revert (T056 complete)
- `18fcf97` — docs: record T056.e hash
- `799cc1a` — T051: invitation registration-URL surface + dev SMTP tidy + status fix
- `4487240` — docs: record T051 hash
- `c8ff215` — docs: detail T052 schema-change plan + suggest bundling with T051.b

**Test status at session end:** Domain 45, Application 216, Architecture 19, Web 38 — all unchanged from 2026-05-24. T057 is UI-only (no handler changes) so no new tests needed. Build clean.

**Session 2026-05-26 commits in chronological order:**
- `ac53e2f` — docs: re-record Act 1 play-through after T051/T055/T056 (Actual/Gap rewrite + findings-summary rewrite)
- `d7f695c` — T057: post-save tab-title fix + EntrustmentScale write-gate (7 files; 5 forceLoad swaps; 1 EntrustmentScaleEdit policy swap; 1 EntrustmentScalesList button-conditional refactor)
- `a60ed2a` — docs: mark findings 5 + 7 closed by T057; update handoff
- `02a167f` — T058: resolve activity-types list Scope column to full path (closes finding #6; 1 file; +52/-1 lines; global Administrator now sees IM and Paed types disambiguated by their full path)
- _replay 3 (no new commit yet; doc update inbound)_ — third Act 1 replay confirmed all 7 fixes stay closed under re-run. Exercised the Papercut SMTP path; surfaced one tiny doc-fix (Papercut Incoming-subfolder claim).
- `43878de` — docs: record Act 1 replay 3 (clean re-run, all 7 fixes stay closed)
- _Acts 2-5 + Appendix draft (commit inbound)_ — doc-only, +890 lines to `scenario-paediatrics.md`. Each act ready to play with the same six-line step convention as Act 1.

**Session 2026-05-24 commits in chronological order (kept for reference):**

**T056.b — EPAs + Curricula cluster** (commit `9e3bc0a`). 13 handlers updated. EPAs scoped via `SubSpeciality.Speciality.InstitutionId`; curricula via `Curriculum.SubSpeciality.Speciality.InstitutionId`. `EpaScopeGuardTests` + `CurriculumScopeGuardTests` (5+5). Razor pages: `EpasList`, `EpaEdit`, `CurriculaList`, `CurriculumEdit`, `CurriculumItemsEdit` swapped to combined policy; call-site updates in `FormEdit`, `ReviewDetail`, `TraineeProfileEdit`. Application 183→193.

**T056.c — ActivityTypes + Forms cluster** (commit `e1d3737`). 13 handlers. ActivityType scope rules use a new `ActivityTypeScopeGuard` static helper (Global types readable, writes Administrator-only; Institution/Speciality/SubSpec-scoped types follow caller's hierarchy). Form scope resolves via `FormMappings.EnsureCallerCanWriteAsync` helper handling all three nullable scope columns. `ActivityTypeScopeGuardTests` + `AssessmentFormScopeGuardTests` (5+5). Razor pages: `ActivityTypesList`, `ActivityTypeEdit`, `FormsList`, `FormEdit`. Application 193→203.

**T056.d — Trainees + Assessors + Invitations + EntrustmentScales cluster** (commit `8ad0788`). 15 handlers. Trainee scope via `TraineeProfile.Curriculum.SubSpeciality.Speciality.InstitutionId`; assessors via `AssessorProfile.InstitutionId`; invitations via `Invitation.InstitutionId`. EntrustmentScales remain Administrator-only for writes (global resource, no institution column). 7 Razor pages swapped. `TraineeScopeGuardTests` + `InvitationScopeGuardTests` (3+3) + 1 EntrustmentScale rejection test. Application 203→210.

**T056.e — Audit + SSO + NavMenu refresh + scenario-doc revert** (commit `ec6d6d1`). 5 handlers. Audit filters via `AuditEntry.InstitutionId` (InstitutionalAdmin sees own + global no-institution events). SSO filters via `SsoGroupRoleMapping.InstitutionId`. NavMenu InstitutionalAdmin block expanded from 3 placeholder links to 11 real routes. New `/admin/specialities` redirect page resolves caller's institution. Scenario doc reverted: Phase 1.B warning replaced with "Resolved by T056", Step 1.8 role no longer says bootstrap, finding #1 marked closed. `AuditScopeGuardTests` + `SsoScopeGuardTests` (3+2) + 1 added rejection test. Application 210→216.

**T051 — invitation URL surface + dev SMTP + status fix** (commit `799cc1a`). Pure UI/config: `InvitationsList.IssueAsync` captures the just-issued token and renders a one-shot info Alert with the full `/account/register?token=…` URL below the form (copy-friendly `<code>`, "shown only once" warning). Status text replaced. `appsettings.Development.json` SMTP port 1025 → 25 (Papercut default). No schema change.

**T051.b deferred** (FirstName/LastName columns on Invitation — needs a hand-written migration + Designer + snapshot update; only enables pre-fill on accept-invitation form, nice-to-have). Recommended to bundle with T052 which has the same migration overhead.

---

### T056.a session detail (kept for reference)

**T056.a — Foundations + Institutions/Speciality/SubSpec cluster** (commit `41def8a`).

Foundations:
- New `AdministratorOrInstitutionalAdmin` policy in `AuthorizationPolicies.cs`.
- Helpers on `ClaimsPrincipalExtensions`: `IsAdministrator()`, `IsInstitutionalAdmin()`, `CanAccessInstitution(int)`.

Handlers fully guarded (14 total — every query+command in the three entity types):
- Institutions: `GetInstitutionsListQuery`, `GetInstitutionByIdQuery`, `CreateInstitutionCommand`, `UpdateInstitutionCommand`, `DeactivateInstitutionCommand`.
- Specialities: `GetSpecialitiesListQuery`, `GetSpecialitiesForInstitutionQuery`, `CreateSpecialityCommand`, `UpdateSpecialityCommand`, `DeactivateSpecialityCommand`.
- SubSpecialities: `GetSubSpecialitiesListQuery`, `GetSubSpecialitiesForSpecialityQuery`, `CreateSubSpecialityCommand`, `UpdateSubSpecialityCommand`, `DeactivateSubSpecialityCommand`.

Every query record carries `ClaimsPrincipal Principal`. Lists filter by institution when caller is not Administrator. Get-by-id returns null on out-of-scope id (404 not 403, avoids leaking other-institution ids). Commands throw `UnauthorizedAccessException` on scope mismatch. `CreateInstitution` / `DeactivateInstitution` are Administrator-only outright (creating new institutions is a global act).

Razor pages updated to pass `authState.User`:
- 6 pages in the Institutions feature itself: `InstitutionEdit`, `InstitutionsList`, `SpecialityEdit`, `SpecialitiesList`, `SubSpecialityEdit`, `SubSpecialitiesList`.
- 8 picker-using pages outside the feature whose handlers haven't been guarded yet: `ActivityTypeEdit`, `Sso/GroupMappings`, `Curricula/CurriculaList`, `Curricula/CurriculumEdit`, `Invitations/InvitationsList`, `Epas/EpasList`, `Epas/EpaEdit`, `Forms/FormEdit`, `Assessors/AssessorProfileEdit`, `Trainees/PendingTraineesList`. These pages still `[Authorize(Policy = "Administrator")]` — the swap to the combined policy happens when their feature cluster lands.

Auth swaps: only the 5 Institutions/Speciality/SubSpec pages move to `AdministratorOrInstitutionalAdmin`. `InstitutionsList` stays Administrator-only by design (listing/creating institutions is a global act; InstitutionalAdmin gets to edit own via the edit page only).

Tests:
- New `tests/Wombat.Application.Tests/TestHelpers/TestPrincipals.cs` — Administrator / InstitutionalAdmin principal builders.
- New `tests/Wombat.Application.Tests/Features/Institutions/InstitutionalAdminScopeTests.cs` — 8 scope-guard tests.
- `CreateInstitutionCommandHandlerTests` gained a second test asserting InstitutionalAdmin cannot create institutions.
- Application count: 174 → 183. Domain 45, Architecture 19, Web 38 unchanged. Build clean, zero warnings.

NavMenu: deferred to T056.e — exposing nav links to half-guarded pages would mislead Mbatha during dev play-through.

Browser verification: skipped at the request-flow level. T056 has no UI/UX changes for the Administrator role; the InstitutionalAdmin path requires T056.b/c/d/e to be testable end-to-end. Scope-guard tests assert the handler behavior at the unit-test layer.

---

## Previous session (2026-05-24 — Act 1 fix-up)

**T054 — Admin CRUD for `EntrustmentScale` + `EntrustmentLevel`** (commit `ef02268`). New `/admin/entrustment-scales` list + `/admin/entrustment-scales/{new|id}` edit pages. Three MediatR commands (Create/Update/Delete) + one new query (`GetEntrustmentScaleById`). Delete enforces referential integrity across four reference paths (`AssessmentForm`, `MsfQuestion`, `PendingEntrustmentDecision`, `EntrustmentDecision`) — no soft-delete needed. Update diffs incoming levels against existing: insert new, update matched-by-id, delete removed (only if no entrustment-decision refs). Nav entry between Activity Types and Scheduled Jobs (`award` icon). 5 new Application tests cover create/dup-reject/update-with-add-rename-remove/delete-unused/delete-rejects-referenced. Build 0 warnings 0 errors; Application 169→174, Architecture 19/19, Web 38/38. Browser-verified end-to-end: created a "Paed General Entrustment Scale" with 5 ten-Cate levels, renamed a level, deleted the scale cleanly. Closes the "only true feature gap" from the Act 1 audit; Step 1.7's workaround can now be reverted to the canonical create-scale prescription as a follow-up.

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

**T058 — Activity-types list Scope column path resolution** (commit `02a167f`). Finding #6 from the 2026-05-26 replay: global Administrator sees activity types from multiple specialities, all rendered as bare "Speciality" — visually indistinguishable except by key suffix. Replaced `@item.Scope` with `@FormatScope(item)` which resolves the scope-entity name via the same scope-aware GetInstitutionsListQuery / GetSpecialitiesListQuery / GetSubSpecialitiesListQuery lookups the T053 picker uses. Format: "Global", "Institution · X", "Speciality · I / S", "Sub-speciality · I / S / Sub". If a lookup row is missing (e.g. soft-deleted institution), helpers fall back to "Institution · #42" with the raw id so rows stay disambiguable. Verified as global Administrator: 20 rows split cleanly between "Speciality · Demo Institution / General Medicine" and "Speciality · Kgosi Kgari Teaching Hospital / Paediatrics". 1 file; +52/-1 lines; UI-only; Architecture 19/19, Web 38/38 pass.

**T057 — Post-save tab-title fix + EntrustmentScale write-gate** (commit `d7f695c`). Two fixes from the 2026-05-26 replay's open findings. Finding #5: Blazor's `<PageTitle>` does not re-evaluate when the same route handler is re-rendered after a same-component SPA NavigateTo (h1 updates correctly via PageHeader's parameter, but document.title stayed on "Create X"). Changed `forceLoad: false` → `forceLoad: true` on the IsNew → /{id} transition on all five affected edit pages (Institution, Speciality, SubSpeciality, EntrustmentScale, Epa, Curriculum). Finding #7: `EntrustmentScalesList` now hides Create/Edit/Delete buttons behind an explicit `_isAdministrator` field check (AuthorizeView Roles= surprisingly did not gate in this page context — fell back to ClaimsPrincipalExtensions.IsAdministrator() field check loaded in OnInitializedAsync); `EntrustmentScaleEdit` page policy tightened to Administrator. Verified as Mbatha (Create/Edit/Delete hidden, direct nav to /new → /access-denied) and admin (EPA saved at /admin/epas/21 with tab title flipping cleanly to "Edit EPA"). 7 files; UI-only; no new tests needed; 318/318 pass.

**Act 1 replay** (commit `ac53e2f`). Re-ran the scenario end-to-end against a freshly reset dev DB now that T051/T055/T056 had landed. Updated every Phase 1.A-1.F step's Actual/Gap lines per scenario convention and rewrote the findings summary. Confirmed 4 closures (T051 SMTP + token, T055 URL stickiness, T056 InstitutionalAdmin lockout). Surfaced 2 still-open findings (#5 tab-title, #7 EntrustmentScale UX wart) which T057 then addressed in the same session. Replay used the inline-URL invitation path exclusively; Mbatha drove Phases 1.D–1.F end-to-end with handler-level scope filtering correctly excluding seeded Demo Institution data from her pickers.

**T051 — Invitation registration-URL surface + dev SMTP tidy + status-message fix** (commit `799cc1a`). `InvitationsList.IssueAsync` now captures the `IssuedInvitationResult.Token` and renders the registration URL in a one-shot info Alert below the form (with copy-friendly `<code>` styling and a clear "shown only on this page-load" warning). The misleading "The stub sender logged the registration link" status text replaced with "Copy the link below — it is shown only once." `appsettings.Development.json` dev SMTP port aligned to Papercut's default (25 instead of 1025). FirstName/LastName columns deferred as T051.b — they require a migration + Designer + snapshot update and are nice-to-have, not blocking.

**T056.e — Audit + SSO + NavMenu refresh + scenario-doc revert** (commit `ec6d6d1`). Audit handlers (`ListAuditEntriesQuery`, `GetAuditEntryByIdQuery`) and SSO handlers (`ListSsoGroupMappings`, `CreateSsoGroupMapping`, `DeleteSsoGroupMapping`) all principal-aware. Audit filters by `AuditEntry.InstitutionId` (InstitutionalAdmin sees own institution + global no-institution events). SSO filters by `SsoGroupRoleMapping.InstitutionId`. NavMenu InstitutionalAdmin block expanded from 3 placeholder links to 11 real routes. New `/admin/specialities` redirect page resolves the caller's institution from claims. Scenario doc: Phase 1.B warning replaced with "Resolved by T056" note, Step 1.8 role no longer says "bootstrap Administrator", finding #1 marked closed. 6 new scope tests. Application 210→216.

**T056.d — Trainees/Assessors/Invitations/EntrustmentScales cluster** (commit `8ad0788`). Trainee handlers (ListPendingTrainees, ListTraineesForSpeciality, GetTraineeProfileById, AdmitTrainee, UpdateTraineeProfile, DeactivateTraineeProfile) all principal-aware and scope-filter via `TraineeProfile.Curriculum.SubSpeciality.Speciality.InstitutionId`. Assessor handlers (ListAssessorUsers, ListAssessorsForSpeciality, GetAssessorProfileById, CreateOrUpdateAssessorProfile) filter via `AssessorProfile.InstitutionId`. Invitation handlers (ListActiveInvitations, IssueInvitation, RevokeInvitation) filter via `Invitation.InstitutionId`. EntrustmentScale write commands (Create/Update/Delete) now require Administrator (global entities). 7 admin pages on AdministratorOrInstitutionalAdmin policy. 7 new scope tests. Application 203→210.

**T056.c — ActivityTypes + Forms cluster** (commit `e1d3737`). ActivityType handlers (ListActivityTypesAdmin, GetActivityTypeEditor, SaveActivityTypeDraft, PublishActivityTypeDraft, DiscardActivityTypeDraft) all principal-aware. Form handlers (GetAssessmentFormsList, GetAssessmentFormById, Create, Update, Deactivate, AddCriterion, RemoveCriterion, Link/UnlinkEpa) all principal-aware. ActivityType scope rules: InstitutionalAdmin sees Global+own-institution types (read-only Global), writes blocked at handler. Form scope rules: forms scoped via InstitutionId or SpecialityId or SubSpecialityId; Global forms (all-null) are Administrator-only for writes but readable. New shared `ActivityTypeScopeGuard` static helper in `PublishActivityTypeDraft`. New `FormMappings.EnsureCallerCanWriteAsync` helper. Razor pages updated: `ActivityTypesList`, `ActivityTypeEdit`, `NewActivity` (picker callsite), `FormsList`, `FormEdit`. 4 admin pages moved to `AdministratorOrInstitutionalAdmin` policy. New scope tests: `ActivityTypeScopeGuardTests` (5) + `AssessmentFormScopeGuardTests` (5). Application 193→203. Build clean, all suites pass.

**T056.b — EPAs + Curricula cluster** (commit `9e3bc0a`). EPA handlers (Create, Update, Deactivate, ListEpasForSubSpeciality, GetEpaById) and Curricula handlers (GetCurriculaList, GetCurriculumById, CreateCurriculum, UpdateCurriculum, CloneCurriculumAsNewVersion, AddCurriculumItem, UpdateCurriculumItem, RemoveCurriculumItem) all take `ClaimsPrincipal Principal` and filter/reject by scope. EPAs scoped via `SubSpeciality.Speciality.InstitutionId`; curricula via `Curriculum.SubSpeciality.Speciality.InstitutionId`. Razor pages updated: `EpasList`, `EpaEdit`, `CurriculaList`, `CurriculumEdit`, `CurriculumItemsEdit` (all five now use `AdministratorOrInstitutionalAdmin` policy), plus call-site updates in `FormEdit`, `ReviewDetail`, `TraineeProfileEdit`. New scope tests: `EpaScopeGuardTests` (5) + `CurriculumScopeGuardTests` (5). Application 183→193. Build clean, all suites pass.

**T056.a — InstitutionalAdmin role-power foundations + Institutions/Speciality/SubSpec cluster** (commit `41def8a`). New `AdministratorOrInstitutionalAdmin` policy + `CanAccessInstitution` helper. 14 handlers in Institutions feature now principal-aware; 14 razor pages updated to pass `authState.User`; 9 new tests cover the scope guards. Application 174→183, Domain 45, Architecture 19, Web 38; build clean. T056.b–e remaining (EPAs+Curricula, ActivityTypes+Forms, Trainees+Assessors+Invitations+EntrustmentScales, Audit+SSO+NavMenu).

**T054 — Admin CRUD for `EntrustmentScale` + `EntrustmentLevel`** (commit `ef02268`). 12 files added, 3 modified. Full Application + Web layer for create/edit/delete of entrustment scales with their nested levels. Delete enforces referential-integrity across `AssessmentForm` / `MsfQuestion` / `PendingEntrustmentDecision` / `EntrustmentDecision`. 5 new Application tests. Browser-verified end-to-end (create Paed scale → rename level → delete). Closes the only true feature gap from Act 1 audit. Build clean, 174 + 19 + 38 tests pass.

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

- `02a167f` — T058 (activity-types list Scope column path resolution; 1 file; Architecture 19/19, Web 38/38; browser-verified as global Administrator)
- `a60ed2a` — docs: mark findings 5 + 7 closed by T057; update handoff (doc-only)
- `d7f695c` — T057 (post-save tab-title fix + EntrustmentScale write-gate; 7 files; 5 forceLoad swaps on Institution/Speciality/SubSpeciality/Epa/Curriculum/EntrustmentScale edit pages + 1 EntrustmentScaleEdit policy swap + 1 EntrustmentScalesList button-conditional refactor with `_isAdministrator` field check; browser-verified as both Mbatha and admin; build clean, all 318 tests pass)
- `ac53e2f` — docs: re-record Act 1 play-through after T051/T055/T056 (doc-only Actual/Gap + findings-summary rewrite of `Rewrite/scenario-paediatrics.md`)
- `799cc1a` — T051 (invitation registration-URL surface + dev SMTP port fix + status-message cleanup; 4 files; UI + config only, no schema change; build clean, all suites pass)
- `ec6d6d1` — T056.e (Audit + SSO + NavMenu refresh + scenario-doc revert; 20 files; 5 handlers + 3 razor pages + new MySpecialitiesRedirect page + 6 new scope tests + NavMenu expansion + scenario doc revert; Application 210/210 → 216/216, Architecture 19/19, Web 38/38, Domain 45/45)
- `8ad0788` — T056.d (Trainees + Assessors + Invitations + EntrustmentScales scope guards; 32 files; 15 handlers + 7 razor pages + 7 new scope tests; Application 203/203 → 210/210, Architecture 19/19, Web 38/38, Domain 45/45)
- `e1d3737` — T056.c (ActivityTypes + Forms scope guards; 20 files; 5 ActivityType + 8 Form handlers + 10 new scope tests + new ActivityTypeScopeGuard and FormMappings helpers; Application 193/193 → 203/203, Architecture 19/19, Web 38/38, Domain 45/45)
- `9e3bc0a` — T056.b (EPAs + Curricula scope guards; 24 files; 8 EPA + 8 curriculum handlers + 10 new scope tests; Application 183/183 → 193/193, Architecture 19/19, Web 38/38, Domain 45/45)
- `41def8a` — T056.a (foundations + Institutions/Speciality/SubSpec scope guards; 56 files; new AdministratorOrInstitutionalAdmin policy, 14 principal-aware handlers, 14 razor call sites, 9 scope tests; Application 183/183, Architecture 19/19, Web 38/38, Domain 45/45)
- `ef02268` — T054 (admin CRUD for EntrustmentScale + EntrustmentLevel; 12 new files, 3 modified; browser-verified; Application 174/174, Architecture 19/19, Web 38/38)
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
