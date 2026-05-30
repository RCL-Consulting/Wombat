# Current state

This file is the live handoff between sessions. Every session ends by editing this file. Keep it short and accurate.

## Active task

**✅ Act 3 Phase 3.D played + a HIGH credit bug (T073) fixed 2026-05-30 (Opus).** Heading into 3.D
(procedure-log stage-minimum credit gating) I found `CreditApplier` gated "level reached" on the
curriculum item's **flat** `MinimumLevelOrder`, ignoring `MinimumLevelByStageJson` — while the dashboard/
progress page display the **stage-aware** minimum. Fixed (T073, `4b5cad8`): added
`TraineeProfile.GetStage(today)` (domain single-source; dashboard's `ComputeTraineeStage` delegates to
it) and `CreditApplier` now gates on `GetMinimumLevelForStage(stage)`. +1 unit test (Application 250),
no regressions. Then played 3.D live: built+published **`procedure_log_paed` v2** via the builder (EPA,
Procedure, Supervision-level Scale bound to scale 2; workflow draft→logged; credit epa_id +
minimum_level_field=supervision_level). du Plessis (stage 2) logged 5 IV-access entries on PAED-011
(stage-2 min 3, flat 4): 2×level-2 + 3×level-3. Result (DB + `/portfolio/progress`): **PAED-011 = 5 / 30
· reached 3 / 30 · "Minimum level 3 (year 2)"** — the three level-3 entries credited toward reached (would
have been 0 under the old flat-gate). Snapshot `act3-D-verified`.

**Act 3 status:** 3.A–3.D done (clean, post-fix). **Remaining: 3.E DOPS, 3.F MSF, 3.G stalled-assessment
triage, 3.I dashboard sweep** (each 3.E/3.F needs that type's schema built via the builder first). All on
master; nothing pushed.

---

**✅ Wart fixed + second clean replay done 2026-05-30 (Opus).** The builder EPA-preview wart (empty for
InstitutionalAdmins) is fixed in `63a5605`: `GetEpaOptionsAsync` now also returns every EPA whose
sub-speciality rolls up to an InstitutionalAdmin's institution (Administrator still all; trainee/
speciality scoping unchanged; +1 test, Infrastructure 8). Re-ran the **whole replay from
`after-act-2-replay`**: rebuilt the 12-field Mini-CEX via the builder as **Mbatha** — the EPA preview
now lists all 15 PAED EPAs ✓ — published v2, drove Dlamini→Naidoo two Mini-CEX (level 3 + 4). Same green
result: DB `PAED-001 CountsSoFar=2, MinimumLevelReachedCount=1`; `/portfolio/progress` shows the credit
+ trajectory (ratings 3 and 4). Builder commit-race note: set field **label before key** and verify the
staged schema (`ActivityTypes.StagingSchemaJson`) before publishing — a key occasionally fails to commit
when a Type `<select>` change immediately follows the key fill.

---

**✅ Clean post-fix replay of Act 3.A–3.C done 2026-05-30 (Opus) — all five fixes hold from a fresh
Act-2 state, and the replay caught + fixed one new bug.** Restored `after-act-2-replay`, rebuilt the
full 12-field Mini-CEX schema via the builder (3 sections; 6 Scale fields each bound to "Paed General
Entrustment Scale" via the new T069 picker), set workflow + credit JSON, published v2. Drove Dlamini →
two Mini-CEX (overall_level 3 and 4) → Naidoo accept+complete each. Results, all live + DB verified:
- **T067** builder Add-field used ~12× with **no crash**. **T069 builder** scale-picker bound 6 fields;
  preview rendered level dropdowns. **T069 runtime** trainee form showed EPA dropdown (15 scoped PAED
  EPAs), Assessor dropdown (5 KGK), Scale dropdowns (5 levels). **T068** trainee created activities.
  Actor-DSL: submit=subject, accept/complete=`field:assessor_user_id` (Naidoo saw Accept).
- **T071** credit (DB): PAED-001 `CountsSoFar=2, MinimumLevelReachedCount=1` (both volume; only the
  level-4 met stage-min 4), keys `["1:complete","2:complete"]`. **T072** `/portfolio/progress` shows
  `PAED-001 — 2 / 30 · reached 1 / 30 · last credited 30 May 2026`, all 15 items, trajectory charts
  both ratings (3 and 4).
- **NEW BUG found + fixed (`cd0ca07`):** `ActivityForm` reloaded EPA/assessor/scale options from the DB
  in `OnParametersSetAsync` on **every** field edit; rapid edits overlapped queries on the scoped
  DbContext → "A second operation was started on this context instance." Fixed to load options only
  when `SchemaJson` changes (claim before await); +1 regression test. Web 42/42.
- Minor wart (not fixed): builder **EPA preview is empty for an InstitutionalAdmin** (Mbatha) because
  `GetEpaOptionsAsync` scopes by speciality/sub-speciality claims, which she lacks; trainees see EPAs
  fine. Consider letting InstitutionalAdmins see their institution's EPAs in the builder preview.
- Snapshot `act3-replay-verified` captures this clean post-fix credited state.

---

**✅ Branch `fix/T067-activity-builder-addfield-crash` MERGED to master 2026-05-30 (ff to `0a88cc8`).**
It carried five clean tested fixes — **T067, T068, T071, T072, T069** — plus the Act 3 play-through
docs. Act 3 Phases 3.A–3.C + 3.H ran (credit engine proven end-to-end, DB-verified); **3.D–3.G + 3.I
remain deferred** and are the next substantive work. Detail in `Rewrite/act3-findings-scratch.md`.

> **Merge caveat:** full-solution `dotnet test` could not include the Integration project (Docker not
> available in this env). All other suites green at merge: Domain 45, Application 249, Architecture 19,
> Web 41, Infrastructure 7 = 361. Run the Integration suite (needs Docker) when next on a Docker host.
> Local master is ~132 commits ahead of `origin/master` (repo not pushed) — push only when you intend to.
>
> **Container engine for Integration tests (dev-env note):** `Wombat.Integration.Tests` uses
> Testcontainers, which needs a Docker-API-compatible engine running. None is installed on this Windows
> machine, so that suite can't run locally yet. Everything else works on native PostgreSQL 16
> (`127.0.0.1:5432`) — Docker is *not* needed for app dev, running the app, the other test suites, or
> `tools/db-snapshot.ps1`. To unlock integration tests locally/CI, install Rancher Desktop or Podman
> (lighter, no Docker Desktop licensing) or Docker Desktop (WSL2 backend). Defer-able if integration is
> only run in CI later.

**Landed (now on master):**
- **T067** (`2b732cf`) — builder crashed the circuit on the first **Add field** click (loop-variable
  capture in `ActivityTypeEdit.razor`). Blocked building any multi-field schema.
- **T068** (`6281eae`) — no trainee could create any activity (`GetActivityTypeEditorQuery` read
  guard rejected non-admins → circuit crash on `/activities/new`). Fixed.
- Full 12-field Mini-CEX schema built via the builder, **published v2**; lifecycle driven
  trainee→assessor→complete twice. **Activity 2 (overall_level 4) credited PAED-001** —
  `CurriculumItemProgresses` row verified in DB (counts=1, minReached=1, key `2:complete`). Actor-DSL
  `field:assessor_user_id` verified both ways; audit log clean.

**T071 (HIGH) ✅ SHIPPED 2026-05-29 (Option A).** Credit `minimum_level_field` was all-or-nothing
(below-level completions credited nothing). Chose Option A: volume (`CountsSoFar`) always counts on a
match; `MinimumLevelReachedCount` only when the level is met. Removed the early `continue` in
`CreditApplier.ApplyAsync`; `RebuildCurriculumProgress` inherits it (delegates to CreditApplier).
Tests updated/added (Application 245/245). Live-verified: a level-3 Mini-CEX now writes
`CountsSoFar=1, MinimumLevelReachedCount=0` (was: no row). Snapshot `act3-credit-semantics-T071`.

**Open follow-ups raised:**
- **T070 (MEDIUM)** — no assessor rating-edit/note in Rated state (Step 3.5 unperformable).
- Doc fixes: Step 1.11.b 13→12 fields; Step 3.6 vs T071; "Format JSON" button refs.

**T069 (HIGH) ✅ SHIPPED 2026-05-30 (Opus).** Runtime `ActivityForm` now renders rich pickers instead
of raw inputs. `IActivityReferenceDataService` gained scoped `GetEpaOptionsAsync` (sub-speciality
scope; admin all), `GetAssessorOptionsAsync` (institution scope, via `IUserAdministrationService`) and
`GetEntrustmentScaleOptionsAsync`/`GetEntrustmentScaleLevelOptionsAsync(scaleKey)`. `ActivityForm`
injects `AuthenticationStateProvider`, loads options per field type, and renders Epa/User (and Choice)
as `<select>`; Scale renders a level `<select>` when its `scale_key` resolves, else **falls back to the
number input** (existing schemas with no scale binding still work). The builder field editor
(`ActivityTypeEdit`) now shows an **Entrustment scale** picker for Scale fields (writes `scale_key`).
Stored primitives unchanged → CreditApplier/actor-DSL unaffected. Live-verified: Dlamini's
`/activities/new` Mini-CEX shows an EPA dropdown (15 scoped PAED EPAs) + Assessor dropdown (5 KGK
assessors); builder Scale-picker binds a scale and the live preview shows the 5 levels. Tests +6
(Infrastructure 7, Web 41; Application 249, Domain 45, Architecture 19). **Debt:** existing published
schemas need re-binding (set each Scale field's scale, republish) for labelled level dropdowns; the
T072 trajectory parser still reads the rating by literal field name (not schema-aware) — small
follow-up if wanted.

**T072 (HIGH) ✅ SHIPPED 2026-05-29 (Opus).** Premise was partly wrong — credit **was** visible on the
trainee **dashboard** (`/`) all along (live-verified: "Curriculum progress" card shows `1 / 30 · reached
0/30` by EPA title; the earlier "dashboard doesn't show it" note was itself the wrong one). Real defect:
a surface mismatch — that dashboard card links to `/portfolio/progress`, which was a *rating-trajectory*
page showing no curriculum credit, and empty anyway (trajectory query allow-listed only literal keys —
`mini_cex`, not the built `mini_cex_paed` — and read field `overall`, but the schema uses
`overall_level`). Fix: new `GetCurriculumProgressForTraineeQuery` (lists every curriculum item incl.
0-credit "0 of N"); `/portfolio/progress` rebuilt to **lead with a Curriculum credit section** (EPA
code+title, count/required, bar, min-level+reached, last-credited date) then the trajectory; trajectory
parser now matches `<base>_…` key prefixes and reads `overall`|`overall_level`. Live-verified as Dlamini
on `act3-credit-semantics-T071`: `/portfolio/progress` shows `PAED-001 — 1 / 30 …`, all 15 PAED items,
trajectory charts the level-3 obs. Tests +5 (Application 249/249; Domain 45, Architecture 19, Web 39).
Trajectory field-name fallback is schema-aware debt → fold proper resolution into **T069**.

**DB snapshots this session:** `act3-schema-built` (Mini-CEX v2 published, pre-lifecycle),
`act3-minicex-credited` (activities 1+2 completed, PAED-001 credited; a stray activity 3 sits in
`submitted` — harmless), and `act3-credit-semantics-T071` (clean post-T071 verification: one level-3
Mini-CEX → `CountsSoFar=1, MinimumLevelReachedCount=0`). `tools\db-snapshot.ps1 restore <name>`.

**Session commits (branch `fix/T067-activity-builder-addfield-crash`, chronological):**
`2b732cf` T067 builder Add-field fix · `6281eae` T068 trainee schema read · `c4b9e73` docs Act 3
play-through + T069/T070/T071 · `79d124d` docs Act 3 correction + T072 · `08be04b` docs handoff fix ·
`860a33d` T071 credit volume-always (+tests, Application 245/245) · _this commit_ finalize log.
Build clean; **full-solution `dotnet test` NOT run — do so before merging the branch.** Server stopped.

**Recommended next pickup: merge the branch, then continue Act 3 (3.D–3.I).** T067/T068/T071/T072/T069
are all clean, isolated, tested fixes on this branch; the schema-driven loop now has real pickers
(T069), real credit display (T072), and real credit semantics (T071). **Opus.**

**Next session — pick one:**

1. **Merge the branch + continue Act 3 (Recommended).** First `git checkout master; dotnet test`
   (full solution — not yet run this branch), then squash/ff-merge
   `fix/T067-activity-builder-addfield-crash`. Then restore `act3-minicex-credited` and continue Phases
   **3.D** (procedure-log stage-minimum credit gating — most valuable distinct test; needs
   `procedure_log_paed` schema built first), 3.E DOPS, 3.F MSF, 3.G stalled triage, 3.I dashboards.
   **Opus.** Each of 3.D–3.F needs that type's full schema built via the builder first (~15 steps each).
   Note: thanks to T069, building those schemas can now bind a scale per Scale field, and the forms are
   fillable without hand-typing ids/GUIDs.
2. **T070 / T064 / T065 / T066** — smaller follow-ups. **Sonnet.**
3. **Trajectory schema-awareness (small).** Make `GetEpaTrajectoryForTraineeQuery` read the rating/EPA
   field roles from the activity-type schema/credit config rather than T072's literal
   `overall`|`overall_level` fallback. **Sonnet.**

**Strong recommendation:** option 1 — the Act 3 prerequisites (pickers, credit display, semantics) are
all in place now, so 3.D–3.I can be driven realistically.

> **Tooling notes for next session (important):** heavy Playwright result-batching latency this
> session — drive the UI in **small batches (≤6 stateful steps), verify each before the next**; large
> batches caused cascading silent failures (a wrong `<select>` value or stale element ref no-ops, then
> later steps run against the wrong state). The activity-type `<select>` options use **numeric ids**
> as values (e.g. `"11"`), not the string key. **Save draft on `/activities/new` keeps the URL at
> `/activities/new`** — reach the created activity via `/activities/{id}` (or My Activities) to submit
> it; don't Submit on the new page. Keep **`psql` calls solo** — a non-zero psql exit cancels sibling
> tool calls in the same message.

## This session at a glance

**Session 2026-05-29 — Acts 1 + 2 replayed clean against a freshly reset DB; recovery-point helper added.** No code changes to product surfaces; one tooling commit + scenario doc + handoff updates.

**Recovery-point helper (`tools/db-snapshot.ps1`, commit `3b370db`).** PowerShell wrapper around `pg_dump --format=custom` + PostgreSQL template-database cloning. Subcommands: `take <name>`, `restore <name>`, `list`, `drop <name>`. Reads the dev connection string from `Wombat.Web` user-secrets at runtime so it tracks the canonical DB without duplicating config. Each `take` writes both a `.dump` file under `recovery/` (gitignored) and a `wombat_snapshot_<name>` template DB; `restore` prefers the template clone (seconds) and falls back to the dump file. `recovery/` added to `.gitignore`.

**Snapshots taken this session:**
- `before-replay` — dirty post-2026-05-27 state (safety net before drop). Can be discarded once we're sure Act 2 stayed clean — kept until next session for safety.
- `after-act-1-replay` — Act 1 outcome state (1 institution + speciality + sub-speciality, 2 entrustment scales, 15 EPAs, 1 curriculum + 15 items, 10 published activity types, admin + Mbatha).
- `after-act-2-replay` — Act 2 outcome state (above + 7 consultants registered + 5 assessor profiles + 5 trainee profiles + 1 decision panel).

**Replay coverage:** every Act 1 step from 1.1 through 1.13 driven via Playwright as bootstrap admin (Steps 1.1-1.7) and Mbatha (Steps 1.8-1.13). Phase 1.F kept replay-3's minimal-schema scope reduction (default `title`-only form + default workflow + default credit; just metadata + publish per type). Act 2 all 8 phases driven as Mbatha + the seven invitee registrations + Smit's negative panel-create check.

**T060/T061/T062/T063 verification under re-run** (all closures hold; see `scenario-paediatrics.md` § "Act 2 findings summary → Replay 2 (2026-05-29)" for the granular per-finding result):
- **A2-1** (T060): Coordinator + external CommitteeMember invitations accepted with blank Speciality; SpecialityAdmin still rejected with new clearer message; Trainee/Assessor still require both Speciality + Sub-speciality.
- **A2-2 / A2-3 / A2-4 / A2-pwd** (T061): `/admin/users` lists 9 KGK + 1 global Admin for Mbatha (Demo Institution users hidden via T056.d). Added Assessor to Zulu/Naidoo/Botha via the Add-role picker; banners + DB confirm. No dev CLI flags used.
- **A2-7** (T059): `/admin/assessors` and `/admin/trainees` render cleanly under Mbatha's view; no DbContext concurrency crash.
- **A2-9 / A2-11** (T063): Mbatha reaches `/committee/panels/new`; Smit (Coordinator) gets `/access-denied?ReturnUrl=...` on the same route. Smit retains read-only `/committee/panels`.
- **A2-10** (T062): All four scope-aware pickers in place (Speciality + Chair single-select + Members/External multi-select). Created `Paed Annual Review Panel 2026` with correct chair/member/external role flags persisted.

**Open findings unchanged** (confirmed open under re-run, not regressions):
- **A2-5** (T065) — Assessor training surface is still a date, not enum.
- **A2-6** (T064) — `AssessorProfileEdit` post-save URL stays at `/admin/assessors/edit` (does not flip to `?id={id}`). Verified by saving 5 profiles back-to-back; URL never changes.
- **A2-8** (T066) — Admit-trainee form has no Stage field; Active profiles list doesn't surface Stage either.

**One new observation (non-blocking):** Phase 2.H Zulu's dashboard now uses T044's `/dashboard/switch/{role}` mechanism — "Viewing as CommitteeMember / You also act as Assessor. Switch view: Assessor" — instead of the stacked-merged layout the 2026-05-27 play-through saw. NavMenu still shows the union. Nicer UX; worth noting in case future Acts depend on the layout assumption.

**Test status:** unchanged from 2026-05-27 — Domain 45, Application 243, Architecture 19, Web 39 = 346/346 pass. No code changes to product surfaces this session.

**Session commits (this session, chronological, master branch):**
- `3b370db` — tools: add `db-snapshot.ps1` for local recovery points (2 files; 165 insertions).
- `37be4ec` — docs: record 2026-05-29 Act 1 + Act 2 replay; T060-T063 fixes hold (2 files; 60 insertions).
- `519286c` — docs: record 37be4ec commit hash in handoff (1 file; 1 insertion / 1 deletion).
- _docs: finalize 2026-05-29 session log_ — this commit.

**Memory file unchanged this session.** Existing `feedback_record_session_secrets` memory was honoured: scenario users' shared password (`Act2Pass!123`) and Mbatha's password (`Mbatha@KGK2026!`) re-recorded in `pwd_DO_NOT_COMMIT.txt` with a note that they correspond to the `after-act-1-replay` / `after-act-2-replay` snapshots, so any session restoring those snapshots can sign in without re-driving registration.

**Session finalized.** Handoff ready for the next session — recommended pickup is **Play Act 3** with **Opus**, starting from `tools\db-snapshot.ps1 restore after-act-2-replay`.

---

**Session 2026-05-27 (continued) — T062 + T063 shipped: Decision Panel surface usable by Mbatha.** **T063:** widened `DemandPanelAdministration` to accept `InstitutionalAdmin`; CreateDecisionPanel + UpdateDecisionPanel now resolve the panel's effective institution (InstitutionId directly for Institution-scoped, or via Speciality.InstitutionId for Speciality-scoped) and reject with `UnauthorizedAccessException` when the caller isn't authorized for it. GetDecisionPanelById + ListDecisionPanels now take `ClaimsPrincipal`; out-of-scope GetById returns null (404, not 403). PanelEdit page authorize tightened to `Administrator,InstitutionalAdmin,SpecialityAdmin,SubSpecialityAdmin` (Coordinator dropped — its actual privilege is `DemandReviewScheduling`). PanelsList retains Coordinator for read-only viewing. **T062:** swapped numeric Institution/Speciality InputNumbers and Chair/Members/External GUID textareas for scope-aware `<select>` widgets + native `<select multiple>` pickers backed by a new `ListPanelMemberCandidatesQuery(Principal)`. The query lists `CommitteeMember` users filtered by caller's institution (Administrator sees all). Single chair `<select>`, two `<select multiple>` for Members and Externals, with the chair excluded from both and members excluded from externals. Post-save now uses `forceLoad: true` (mirrors T057's pattern) so the document.title updates after the SPA nav from `/new` to `/{id}`. 7 new scope-guard tests in `PanelScopeGuardTests`.

**Browser-verified end-to-end (Playwright):**
- As Mbatha (InstitutionalAdmin): `/committee/panels` lists 1 KGK panel; `/committee/panels/new` Speciality picker shows only "Kgosi Kgari Teaching Hospital / Paediatrics"; Chair picker shows 4 CommitteeMembers (Botha, Naidoo, van Rensburg, Zulu — Demo Committee correctly excluded). Created "T062 Test Panel" with chair Zulu + Members Botha+Naidoo + no External → saved to `/committee/panels/2` with all pickers showing the round-tripped selection.
- As Smit (Coordinator): `/committee/panels` shows panels (read-only); `/committee/panels/new` redirects to `/access-denied`.
- As Administrator: `/committee/panels/new` Speciality picker shows both institutions ("Demo Institution / General Medicine" + "Kgosi Kgari Teaching Hospital / Paediatrics"); Chair picker shows all 5 CommitteeMembers including Demo Committee.

**Session 2026-05-27 (continued earlier) — T061 shipped: admin Users surface.** Replaced the `/placeholder/users` stub with `/admin/users` (list, scope-filtered, client-side filter) + `/admin/users/{userId}` (account summary, role add/remove, password reset, lockout, pending-invitation cleanup). Both pages gated on `AdministratorOrInstitutionalAdmin` and pass `ClaimsPrincipal` through to the handlers, which scope-filter via the standard T056 helpers. Added 6 MediatR records under `Features/Users/`: `ListUsersQuery`, `GetUserByIdQuery`, `AddRoleToUserCommand`, `RemoveRoleFromUserCommand`, `ResetUserPasswordCommand`, `SetUserLockoutCommand`, `RevokePendingInvitationsForEmailCommand`. Extended `IUserAdministrationService` with `ListAllUsersAsync`, `AddRoleAsync`, `RemoveRoleAsync`, `ResetPasswordAsync`, `SetLockoutAsync`; `UserIdentityDetails` gained a positional `IsLockedOut` (default false → existing call sites unaffected). Modified `AcceptInvitationCommandHandler` to sweep all other Active same-email invitations to Revoked on a successful registration. Removed both dev-CLI flags from `Wombat.Web/Program.cs`. NavMenu's Administrator block + InstitutionalAdmin block both point at `/admin/users`.

**Role-mutation guardrails:** `UserAdministrationRules.AssignableRoles` excludes `Administrator` and `PendingTrainee` (must remain DB-direct / system-managed). Handler also rejects: cross-institution targets (UnauthorizedAccessException), self-lockout (InvalidOperationException), lockout of any Administrator (InvalidOperationException), password reset against an Administrator unless caller is Administrator.

**Test additions (16 new):** `UserAdministrationTests` (12 — scope guards, role-mutation rejections, password reset forwarding, lockout self-refusal + admin-refusal, invitation sweep + cross-institution rejection, AssignableRoles assertion); `AcceptInvitationAutoRevokeTests` (1 — auto-revokes other Active same-email invitations on registration); `UsersListSmokeTests` (1 bUnit — 2 rows render + filter narrows to one). The Application stub `StubUserAdministrationService` was updated for the new interface methods.

**Browser-verified end-to-end (Playwright):**
- As Administrator: `/admin/users` lists 16 seeded users; opened Patel's detail, reset password to `PatelT061!2026`, signed out, signed back in as `patel@kgk.wombat.local` with the new password → dashboard reachable.
- As Mbatha (InstitutionalAdmin): `/admin/users` shows 14 rows — all KGK users + the global Administrator. Demo Institution users hidden. Direct nav to a Demo user's detail URL renders "User unavailable" (out-of-scope = 404, not 403). On Zulu (CommitteeMember + Assessor): add-role picker correctly excludes Administrator, PendingTrainee, CommitteeMember, Assessor. Added Coordinator → success Alert. Removed Coordinator → success Alert. Lockout → status flips to "Locked out". Reactivate → status back to "Active".

**Known UX wart noted, not blocking (deferred):** On first nav to `/admin/users/{userId}` immediately after clicking a list row, the document.title sometimes lags at "Users" while the h1 updates correctly. Same family as T057's `<PageTitle>` re-eval issue. Hard reload of the detail URL renders the title correctly. Doesn't affect functionality.

**Session 2026-05-27 (earlier) — T059 + T060 also shipped.** 11 findings raised during Act 2 play-through; T059 fixed DbContext concurrency in ListAssessors/Trainees, T060 relaxed the invitation validator for Coordinator + external CommitteeMember.

**Memory addition:** `feedback_record_session_secrets` — when a session sets a password during a play-through, record it to disk so the next session can resume. Previous session's AI lost Mbatha's password by not recording it; cost ~10 minutes this session to recover.

**Session commits (this session, in chronological order — all on `master`):**

- `9114244` — T059: fix DbContext concurrency in list handlers + dev CLI ops tools.
- `c0072a9` — docs: Act 2 play-through findings + handoff update.
- `e6cdc03` — docs: record session commits in handoff.
- `bc9776c` — T060: relax invitation validator for Coordinator + external CommitteeMember + create T061–T066 task files.
- `3b652fd` — docs: record T060 in handoff + recommend T061 next.
- `f5fabf3` — docs: finalize 2026-05-27 session log.
- `7610ac5` — T061: admin Users surface + auto-revoke of stale invitations on register + dev-CLI flag removal. **(+16 tests, 339/339 pass.)**
- `2565137` — docs: record T061 commit hash in handoff + scenario findings.
- `852f410` — T062 + T063: Decision Panel scope-aware pickers + InstitutionalAdmin admin policy + Coordinator dropped from edit. **(+7 tests, 346/346 pass.)**

**Test status at session end:** build clean, **346/346 pass** (Domain 45, Application 243 (+7 from T063), Architecture 19, Web 39).

## Previous session

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

**T060 — Invitation validator relaxation + 7 task files for the remaining Act 2 findings** (commit `bc9776c`). Split `InvitationRules.ValidateScope`'s combined SpecialityAdmin/Coordinator/CommitteeMember `requires speciality scope` rule so only SpecialityAdmin still requires speciality (with a clearer message). Coordinator and CommitteeMember can now be issued with just an institution — closes A2-1. Sub-speciality still forbidden for all three. 5 new tests in `InvitationValidatorTests.cs` exercise the validator through `IssueInvitationCommandHandler`: Coordinator-null-speciality accepted, CommitteeMember-null-speciality accepted, SpecialityAdmin-null-speciality rejected with new message, Coordinator/CommitteeMember-with-sub-speciality still rejected. Browser-verified end-to-end as Mbatha. Scenario doc Steps 2.1 + 2.2 reverted to leave-Speciality-blank prescription; Act 2 findings summary marks A2-1 closed. Also created `Rewrite/Tasks/T061-...md` through `T066-...md` covering the remaining 9 Act 2 findings. Tests: Domain 45, Application 221 (+5), Architecture 19, Web 38 = 323/323.

**T059 — DbContext concurrency fix on ListAssessors + ListTrainees + dev CLI ops tools + Act 2 play-through** (commit `9114244`). Surfaced during the Act 2 play-through when `/admin/trainees` and `/admin/assessors` crashed with `A second operation was started on this context instance before a previous operation completed.` Two handlers (`ListAssessorsForSpecialityQueryHandler` + `ListTraineesForSpecialityQueryHandler`) fired `Task.WhenAll(profiles.Select(GetByIdAsync))` on a shared `ApplicationDbContext`. EF Core forbids parallel queries on a single context. Fix: replaced with sequential foreach loops; profile count is small (N=5 in this scenario, plausibly <50 in production), so the sequential cost is negligible. Same session: added two dev-only CLI flags to `Wombat.Web/Program.cs` — `--dev-reset-password <email> <new>` (Identity-backed PasswordReset token + ResetPassword) and `--dev-add-role <email> <role>` (UserManager.AddToRoleAsync) — both guarded by `app.Environment.IsDevelopment()` so they're inert in Production. Used to recover Mbatha's lost password and to stamp Assessor onto Zulu/Naidoo/Botha respectively. Act 2 then played end-to-end Phases 2.A–2.H; 11 findings recorded in `Rewrite/scenario-paediatrics.md`.

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

- `37be4ec` — docs: record 2026-05-29 Act 1 + Act 2 replay; T060-T063 fixes hold (2 files: `Rewrite/scenario-paediatrics.md` + `Rewrite/current_state.md`; +60 insertions; new "Replay 2 (2026-05-29)" subsection under Act 2 findings summary; doc-only).
- `3b370db` — tools: add `tools/db-snapshot.ps1` for local recovery points (2 files; .gitignore adds `recovery/`; PowerShell helper wraps `pg_dump --format=custom` + PostgreSQL template-DB cloning; `take` / `restore` / `list` / `drop` subcommands; pulls connection from Wombat.Web user-secrets at runtime; verified round-trip against the dev DB).
- `7c9e3a9` — docs: record T062+T063 commit hash in handoff + scenario findings (doc-only).
- `852f410` — T062 + T063 (Decision Panel scope-aware pickers + InstitutionalAdmin admin policy widened + Coordinator dropped from page authorize; 7 new scope-guard tests; Application 236 → 243; build clean; browser-verified end-to-end as Mbatha + Smit + Administrator).
- `2565137` — docs: record T061 commit hash in handoff + scenario findings (doc-only).
- `7610ac5` — T061 (admin Users surface at `/admin/users` + `/admin/users/{userId}`; 6 new MediatR records; `IUserAdministrationService` extended with 5 methods; `AcceptInvitationCommandHandler` auto-revokes stale same-email invitations on registration; both dev-CLI flags removed from `Wombat.Web/Program.cs`; 16 new tests; Application 227 → 236, Web 38 → 39; 339/339 → 339/339 pass; browser-verified as Administrator + Mbatha).
- `bc9776c` — T060 (relax invitation validator for Coordinator + external CommitteeMember; SpecialityAdmin still requires speciality with clearer message; 5 new tests in InvitationValidatorTests.cs; 7 task files added — T060 + T061 + T062 + T063 + T064 + T065 + T066; scenario doc Step 2.1 + 2.2 reverted; A2-1 closed; build clean; Domain 45/45, Application 221/221, Architecture 19/19, Web 38/38; browser-verified end-to-end as Mbatha).
- `c0072a9` — docs: Act 2 play-through findings + handoff update (Rewrite/scenario-paediatrics.md Act 2 Actual/Gap + 11-finding summary; Rewrite/current_state.md session log; doc-only).
- `9114244` — T059 (DbContext concurrency fix in ListAssessors + ListTrainees; +2 dev CLI flags `--dev-reset-password` and `--dev-add-role` in `Wombat.Web/Program.cs`; 3 files; +96/-8 lines; build clean; Domain 45/45, Application 216/216, Architecture 19/19, Web 38/38).
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
