# Current state

This file is the live handoff between sessions. Every session ends by editing this file. Keep it short and accurate.

## ⭐ SESSION 2026-06-14 (Opus) — T091 fresh-DB replay (**Act 1 + Act-2 adoption gate VALIDATED**) + both invitation findings **FIXED** (T092 + T093)

**Both findings surfaced by the replay are now fixed, tested, and live-verified — no longer just tickets.**

- **T092 (blocker) FIXED** — `GetSpecialitiesListQuery`/`GetSubSpecialitiesListQuery` gained an
  InstitutionalAdmin branch scoping to the institution's **active adoptions**. Mbatha's invitation Speciality
  dropdown now shows KGK's adopted discipline; she issued an Assessor invitation (Patel) live. +4 tests.
- **T093 (CollegeAdmin provisioning) FIXED** — the finding grew: T091 P1 never wired CollegeAdmin *scoping*
  (no `WombatIdentityUser.CollegeId`, no `CollegeId` claim emission). Built end-to-end: `Invitation.CollegeId`
  + nullable `InstitutionId`, user `CollegeId`, claims-factory emits it, provisioner sets it, `InvitationRules`
  allows CollegeAdmin (Administrator-only, college-scoped), invitation form gains a College picker. Migration
  **`T093_CollegeAdminProvisioning`** (CLI-generated, applied on boot). +5 tests. **Live-verified:** issued a
  CollegeAdmin invite for **Dr Kruger** (College of Paediatricians) → he registered → `/admin/epas` shows
  exactly his college's 15 PAED EPAs. DB: user `CollegeId=2`.

**All suites green after the fixes:** Domain 50, **Application 313** (+9), Architecture 19, Web 43; full
solution **Release build clean (0 warnings)**. Snapshot **`t091-act2-with-collegeadmin`** banks the fixed
state (T093 migration applied + Kruger). New secret `Kruger@CMSA2026!` recorded in `pwd_DO_NOT_COMMIT.txt`.
**Dev server STOPPED; browser open.** Task files `Tasks/T092-*.md` (DONE) + `Tasks/T093-*.md` (new, DONE).

---

## (earlier same session) 2026-06-14 — T091 fresh-DB replay: Act 1 + Act-2 adoption gate VALIDATED; 2 findings raised

**Ran the rewritten `scenario-paediatrics.md` against a truly fresh DB (dropped + recreated empty → startup
migrated the full 29-migration chain incl. all 4 T091 migrations + seeded Demo College/Institution). Drove Act 1
end-to-end UI-first (Playwright) + DB-verified at every step, then validated the key Act-2 T091 change (adoption-gated
admission). The entire T091 hinge works from scratch.** No product code changed; 2 real integration regressions found
(invitation surface). **Dev server STOPPED; browser left open.** Snapshots banked: **`t091-act1-replay`**,
**`t091-act2-molefe-admitted`**.

**Act 1 — DONE + DB-verified (ids in parens):** College of Paediatricians (2) → Paediatrics speciality (2) →
General Paediatrics sub-speciality (2, DefaultEntrustmentScaleId→Paed scale (2)); Paed General Entrustment Scale (2,
5 ten-Cate levels); **15 national EPAs** (ids 2–16, all `OwningInstitutionId` null, 2 elective); **FCPaed(SA) Part 1
v2026.1** curriculum (2) + **15 items** (counts/min-levels/per-stage-JSON/windows/weights all match the runbook table
byte-for-byte, incl. PAED-010 yr1-min-2 F-3E-2 fix); **KGK institution** (2); **Mbatha** InstitutionalAdmin (invite→
register→dashboard, nav shows Curriculum Adoptions); **KGK adopted** FCPaed v2026.1 via `/admin/adoptions`
(InstitutionCurriculumAdoptions id 1, active, DB-verified — no institution picker, resolved from her claim);
**Mini-CEX activity type published** (id 11, Scope=Institution→KGK; T053/T055/T056 scope-guard + publish path all
work post-T091). **Scope reduction:** built only the Mini-CEX worked example (default schema), not all 10 types — the
builder is platform mechanics already proven in prior replays + P6; full 10-type build still pending for Act 3.

**Act 2 — adoption gate VALIDATED (the core T091 Act-2 change):** admit-form (`/admin/trainees/edit`) curriculum
picker shows **only the adopted** `FCPaed(SA) Part 1 (2026.1)` (nothing else); Molefe admitted → **TraineeProfile id 2
pinned: CurriculumId 2, InstitutionId 2, AdoptionId 1**, start 2023-01-15. Version-pinning works end-to-end. Full Act-2
team onboarding (other 4 registrars, 6 consultants, coordinator, assessor profiles, committee panel) **NOT done** —
blocked by the finding below.

**🐞 TWO FINDINGS (invitation surface, post-T091) → new task `T092`:**
1. **F-A2-T091-1 (blocking) — InstitutionalAdmin cannot issue Assessor/Trainee invitations.** On `/admin/invitations`
   as Mbatha the **Speciality dropdown is empty**, but Assessor/Trainee require speciality+sub-speciality scope
   (enforced by T088 validation + `InvitationRules`) → "The selected role requires speciality and sub-speciality
   scope." **Root cause:** `InvitationsList.razor` populates specialities via `GetSpecialitiesListQuery`, whose handler
   (`GetSpecialitiesListQueryHandler.cs:22-30`) returns `[]` when caller isn't Administrator and has no college claim —
   an InstitutionalAdmin has none (specialities are College-owned post-T091). Bootstrap Administrator is unaffected
   (sees all national specialities). This **blocks Mbatha onboarding her own clinical team** — the whole Act-2 workflow.
   **Workaround used:** issued Molefe's Trainee invitation as the bootstrap Administrator. **Fix:** give the invitation
   form an InstitutionalAdmin speciality source (national specialities the institution has adopted, or all national).
2. **Step 1.3 — invitation form exposes no `CollegeAdmin` role / College picker.** Role dropdown lacks CollegeAdmin;
   the national catalogue was therefore authored by the bootstrap Administrator (scenario's documented fallback). The
   CollegeAdmin role/claim/policy exist (T091 P1) but the invitation-surface wiring doesn't.

**▶ Next:** (a) **fix T092** (the F-A2-T091-1 blocker is the priority — it stops realistic InstitutionalAdmin-driven
onboarding), then optionally the CollegeAdmin invitation wiring; (b) **resume the replay** from
`t091-act2-molefe-admitted` (or rebuild from `t091-act1-replay`): finish Act-2 team onboarding (needs T092 fixed or the
admin workaround), build the remaining 9 activity types, then Acts 3–5 (activities/credit, committee/STARs, graduation).
**Opus** recommended (T092 auth fix + scenario correctness). Old `act*-v2-*` snapshots remain invalid under the new schema.

**Tests:** not run this session (no product code changed) — last green = Domain 50, Application 304, Architecture 19,
Web 43, Infrastructure 10. **⚠️ Tooling:** dev server via the **PowerShell** tool (background); **stop it before**
`dotnet test`/`build`/`db-snapshot take|restore`; `psql` at `C:\Program Files\PostgreSQL\16\bin\psql.exe` — use ASCII
**here-strings** piped to psql (the `-c "...\"...\""` double-quote escaping fails in PowerShell). Reused secrets
`Mbatha@KGK2026!` / `Act2Pass!123` (already in `pwd_DO_NOT_COMMIT.txt`).

---

## ⭐ SESSION FINALIZED — 2026-06-13→14 (Opus) — T091 P4 + P5 + P6-validation DONE, scenario rewritten, **master PUSHED**

**Very long session, now shipped.** Delivered T091 **Phase 4** (adoption + versioning), all of **Phase 5** (Web
surfaces a–e), a **P6 fresh-DB validation pass** proving the whole redesign works from scratch in the running app,
the **T091 rewrite of `scenario-paediatrics.md`**, and finally **pushed `master` to origin** (the long-standing
backlog). **Remaining for T091:** only the full realistic Acts 1–5 narrative replay on the new schema.

**✅ PUSHED 2026-06-14.** `git push origin master` shipped **206 commits** (the entire backlog — this session + all
prior unpushed work) to `origin/master` (`…→9f2d5b7`, clean fast-forward). `origin/master` now == local `master`
(0/0). The long-standing "push master to origin" item is **done**; HEAD verified green in Release (0 warnings)
before pushing. (Older session blocks below still say "nothing pushed" — true when written, now superseded.)

**P6 validation (this session's finale) — fresh DB, UI-driven, DB-verified:**
- Dropped + recreated `wombat_t002_verify` empty → startup applied the **entire migration chain cleanly** (incl.
  all T091 migrations: AddCollege, ReparentSpecialityToCollege, LocalExtraDiscriminators, CurriculumAdoption) and seeded.
- **Fix shipped (`220a909`):** `DevUserSeeder` never set the new `TraineeProfile.InstitutionId` (P2 FK) → fresh
  boot crashed (exit 82, FK violation). Now set from the DEMO institution; `AdoptionId` left null (dev seed bypasses
  the admit gate; CreditApplier scopes by CurriculumId + InstitutionId).
- DB-verified seed: Demo College → General Medicine (CollegeId set) → SubSpeciality → IM curriculum + trainee profile.
- **UI-validated (Playwright, admin@wombat.local / ChangeThisAdmin123!):** `/admin/colleges` lists the College;
  `/admin/colleges/1/specialities` (P5d) shows "College: Demo College"; **`/admin/adoptions`** (Administrator picks
  Demo Institution → adoptable-curricula picker → **adopted IM Core Curriculum**) → "Curriculum adopted." + active
  row, **DB-verified** (`InstitutionCurriculumAdoptions` Id 1, active); `/admin/curricula` shows the new **College**
  column (P5e). **Dev server STOPPED; browser left open.**
- Snapshot **`t091-fresh-setup`** banked (clean validated state; one adoption present). Old `act*-v2-*` snapshots
  remain invalid under the new schema.

**`scenario-paediatrics.md` rewritten for T091 (`f47a076`):** the canonical runbook now prescribes the national
model end-to-end (College of Paediatricians authors the catalogue; KGK adopts; admission adoption-gated; Act 1
restructured + steps renumbered 1.1–1.16; Acts 3–5 credit/STAR notes). Prior `Actual:`/`Gap:` + per-act findings
are marked **superseded (pre-T091)** and retained for history. It's ready to drive the replay below.

**▶ Next: the full realistic Acts 1–5 narrative replay** on the new schema (its own focused Opus session), following
the rewritten `scenario-paediatrics.md`. Build the
national **College of Paediatricians** → Paediatrics → General Paed → 15 EPAs + curriculum (as Administrator/CollegeAdmin),
create **KGK** institution + Mbatha (InstitutionalAdmin), **KGK adopts** the curriculum via `/admin/adoptions`, then
onboard trainees (now **gated on adoption** — admit only into the adopted version), run activities/credit, committee/STARs,
graduation; re-bank per-act snapshots. Restore `t091-fresh-setup` (or drop/recreate empty) to start. **Watch:** trainee
admission requires an active adoption; the InstitutionalAdmin curriculum dropdown shows only adopted versions.

**Tests:** Domain 50, **Application 304**, Architecture 19, Web 43, Infrastructure 10 — all green. Integration NOT
run (needs Docker). Full solution builds clean in Release (0 warnings).

**⚠️ Tooling reminders:** dev server via the **PowerShell** tool (`$env:ASPNETCORE_ENVIRONMENT='Development'; dotnet run …`,
run in background); **stop it before** `dotnet test`/`build`/`db-snapshot take|restore`; `psql` at
`C:\Program Files\PostgreSQL\16\bin\psql.exe` (solo calls, ASCII here-strings); `AuditEntries` append-only. **EF CLI:**
run `dotnet ef migrations add` **without `--no-build`** (stale-assembly empty/wrong-migration hazard).

---

## (superseded) 2026-06-13 — T091 **P4 + P5 (a–e) DONE**; P6 next

**Long session — shipped T091 Phase 4 (adoption + versioning) and all of Phase 5 (Web surfaces, a–e).** Eleven
code commits + docs on `master` (**nothing pushed**; master is far ahead of origin — pushing is the standing item).
All unit suites green; **full solution builds clean in Release** (0 warnings).

**Phase 5 is complete.** The full national-catalogue + adoption workflow is now wired end-to-end in the app:
- **P5a** (`28a3cec`) — College admin CRUD + `/admin/colleges` pages (Administrator).
- **P5b** (`889f8ba`) — `/admin/adoptions` (InstitutionalAdmin adopts/re-adopts; Administrator via institution
  picker). Closes the gap where an InstitutionalAdmin couldn't admit trainees.
- **P5c** (`daeadb8`) — `NationalCatalogueAccess` policy on EPA/curriculum pages + CollegeAdmin/Administrator nav.
- **P5d** (`bfa4ca4`) — Speciality/SubSpeciality re-routed under College (`/admin/colleges/{id}/specialities`);
  new `GetSpecialityByIdQuery`; fixed the broken create-speciality flow (was passing InstitutionId as CollegeId).
- **P5e** (`5103553`) — College display columns: `EpaDto`/`CurriculumDto` carry `CollegeName`; EpasList/CurriculaList
  show a real College column (was a "—" placeholder).

**Phase 4 (earlier, `c592556`):** `InstitutionCurriculumAdoption` (one active per institution+sub-speciality),
`TraineeProfile.AdoptionId`, adopt/re-adopt, admission **hard-gate**, `CreditApplier` scoped to the adopted version
+ own-institution local extras, narrowed catalogue views, migration `T091_CurriculumAdoption`. Plus `b9da54c`
(pre-existing CS8604 Release fix).

**Tests:** Domain 50, **Application 304**, Architecture 19, Web 43, Infrastructure 10 — all green. Integration NOT
run (Docker). ~17 tests added this session.

**End-to-end flow now in the app:** Administrator creates a College → its Specialities/SubSpecialities → a
CollegeAdmin (or Administrator) authors national EPAs + a curriculum → an InstitutionalAdmin adopts that curriculum
version at `/admin/adoptions` → trainees are admitted into the adopted version (hard-gated) → activities credit only
against the adopted version + own-institution local extras.

**▶ Next: P6 — scenario rebuild on a FRESH DB.** Old `act*-v2-*` snapshots are **invalid** under the new schema
(College layer, re-parented Speciality, adoption, version-pinned trainees). Rebuild Act-1 setup as: national CMSA
**College of Paediatricians** → Speciality (Paediatrics) → SubSpeciality (General Paed) → 15 EPAs + curriculum
(authored as Administrator/CollegeAdmin), then **KGK adopts** the curriculum via `/admin/adoptions`, then admit the
trainees (now gated on adoption) and replay Acts 1–5; re-bank snapshots. Drive via Playwright + verify in DB.
Watch: trainee admission now REQUIRES an active adoption for the discipline; the InstitutionalAdmin's curriculum
dropdown shows only adopted versions. **Opus** recommended (migration/scenario correctness). Integration tests
need Docker (not on this box).

**⚠️ EF CLI gotcha (still relevant):** run `dotnet ef migrations add` **without `--no-build`** (stale-assembly
empty migration + wrong-migration deletion bit me this session; recovered via `git checkout`).

---

## (superseded) 2026-06-13 — T091 **P4 + P5a–d DONE**

**Long session — shipped T091 Phase 4 (adoption + versioning) and all of Phase 5 (Web surfaces, a–d).** Eight
code commits + docs on `master` (**nothing pushed**; master is far ahead of origin — pushing is the standing item).
All unit suites green; **full solution builds clean in Release** (0 warnings).

**Latest add — `bfa4ca4` P5d (Speciality/SubSpeciality re-routed under College):** specialities are College-owned
(P2) but the admin UI was still institution-routed and the **create-speciality flow was broken** (passed
InstitutionId where CollegeId is expected). Now: new `GetSpecialityByIdQuery` (clean lookup, CanAccessCollege,
404-not-403) replacing the institution-scanning hack; Speciality pages at `/admin/colleges/{CollegeId}/specialities`;
pages → `AdministratorOrCollegeAdmin`; `/admin/specialities` redirect resolves the CollegeAdmin's college;
CollegesList Specialities drill-in + CollegeAdmin Specialities nav; dead institution→speciality links removed.

**Earlier this session:** `c592556` **P4** (adoption + versioning — `InstitutionCurriculumAdoption`,
`TraineeProfile.AdoptionId`, adopt/re-adopt, admission hard-gate, CreditApplier scoping, narrowed catalogue views,
migration `T091_CurriculumAdoption`); `b9da54c` fixed a pre-existing CS8604 (Release build); `28a3cec` **P5a**
(College admin CRUD + pages); `889f8ba` **P5b** (adoption page — closes the InstitutionalAdmin-can't-admit gap);
`daeadb8` **P5c** (`NationalCatalogueAccess` policy + CollegeAdmin/Administrator catalogue nav).

**Tests:** Domain 50, **Application 304**, Architecture 19, Web 43, Infrastructure 10 — all green. Integration NOT
run (Docker). ~16 tests added this session.

**▶ Next: optional P5e polish, then P6.**
- **P5e (optional, deferred):** College display columns on the admin EPA + curriculum lists — add `CollegeName`
  to `EpaDto`/`CurriculumDto` and update every construction site (GetEpas x2, CreateEpa, UpdateEpa, GetCurricula
  x2, `CurriculumMappings.ToDto`, CloneCurriculum, ManageCurriculumItems). Pure display; wide blast radius —
  skip or fold into P6.
- **P6 — scenario rebuild on a fresh DB:** national CMSA Paediatrics catalogue (College → Speciality →
  SubSpeciality → EPAs + curriculum, authored as CollegeAdmin/Administrator), **KGK adopts it** via
  `/admin/adoptions`, then admit trainees (now gated on adoption) and replay Acts 1–5. **Old `act*-v2-*` snapshots
  are invalid** under the new schema — DB must be rebuilt fresh. **Opus** recommended.

**End-to-end flow now wired in the app:** Administrator creates a College → its Specialities/SubSpecialities → a
CollegeAdmin (or Administrator) authors national EPAs + a curriculum → an InstitutionalAdmin adopts that curriculum
version at `/admin/adoptions` → trainees are admitted into the adopted version (hard-gated) → activities credit only
against the adopted version + own-institution local extras.

**⚠️ EF CLI gotcha (still relevant):** run `dotnet ef migrations add` **without `--no-build`** (stale-assembly
empty migration + wrong-migration deletion bit me this session; recovered via `git checkout`).

---

## (superseded) 2026-06-13 — T091 **P4 + P5a–c DONE**

**Long session — shipped T091 Phase 4 (adoption + versioning) and Phase 5a–c (Web surfaces).** Six commits
on `master` (**nothing pushed**; master is far ahead of origin — pushing is still the standing item). All unit
suites green and the **full solution builds clean in Release** (0 warnings — the pre-existing `Tags` nit is fixed).

**Commits this session (master, unpushed):**
- **`c592556` — P4 (adoption + versioning).** `InstitutionCurriculumAdoption` (one active per institution+
  sub-speciality, partial unique index); `TraineeProfile.AdoptionId`; `Curriculum.CloneAsNewVersion` clones only
  national-core items + preserves stage JSON. `AdoptCurriculum`/`ListAdoptions`. `AdmitTrainee`/`UpdateTraineeProfile`
  **hard-gate** on the institution's active adoption. `CreditApplier` scopes credit to the trainee's adopted version
  + own-institution local extras. `GetCurricula`/`GetEpas` narrowed for InstitutionalAdmin to adopted-only. Migration
  `T091_CurriculumAdoption`. (Full detail in the superseded section below.)
- **`b9da54c`** — fixed a pre-existing CS8604 (`EmailMessage.Tags` null) that broke `dotnet build -c Release`.
- **`28a3cec` — P5a (College admin).** `Features/Colleges` CRUD (Administrator-only create/update/deactivate;
  list/by-id allow CollegeAdmin own); `/admin/colleges` list + edit pages; Administrator nav link.
- **`889f8ba` — P5b (adoption page).** `GetAdoptableCurricula` (shared active national catalogue) + `/admin/adoptions`
  (InstitutionalAdmin adopts/re-adopts via their claim; Administrator picks an institution). **This closes the gap
  where InstitutionalAdmins couldn't admit trainees** (no UI to create an adoption). InstitutionalAdmin nav now has
  Curriculum Adoptions (the dead Specialities link was removed).
- **`daeadb8` — P5c (CollegeAdmin authoring surface).** New `NationalCatalogueAccess` policy
  (Admin+CollegeAdmin+InstitutionalAdmin) on the EPA/curriculum list+edit pages; new CollegeAdmin nav section
  (EPAs, Curricula); Administrator EPAs/Curricula links.

**Tests:** Domain 50, **Application 303**, Architecture 19, Web 43, Infrastructure 10 — all green. Integration NOT
run (Docker). +14 tests this session (adoption, College admin, credit-scoping, admit-gate, adoptable-list).

**▶ Next: P5d (finish Web surfaces), then P6.**
- **P5d** — the **Speciality/SubSpeciality admin UI is still institution-routed** (`/admin/institutions/{id}/specialities`,
  `GetSpecialitiesForInstitutionQuery`) — semantically wrong post-P2 (specialities are College-owned). Re-route under
  College (`/admin/colleges/{id}/specialities`), add a **College picker** to `SpecialityEdit`, a CollegeAdmin
  **Specialities** nav link, and a `GetSpecialitiesForCollege` query. Then **College display columns** on the admin
  EPA + curriculum lists (add `CollegeName` to `EpaDto`/`CurriculumDto` + update every projection/`ToDto` call site —
  several: GetEpas x2, GetCurricula x2, CurriculumMappings.ToDto, CloneCurriculum, ManageCurriculumItems).
- **P6** — scenario rebuild on a **fresh DB**: national CMSA Paediatrics catalogue (College → Speciality →
  SubSpeciality → EPAs + curriculum), **KGK adopts it** (via `/admin/adoptions`), then admit trainees (now gated on
  adoption) and replay. Old `act*-v2-*` snapshots are **invalid** under the new schema — DB must be rebuilt. Also fold
  in any test additions. **Opus** recommended for P5d/P6 (route rework + migration/scenario correctness).

**⚠️ EF CLI gotcha (still relevant):** run `dotnet ef migrations add` **without `--no-build`** — `--no-build` used a
stale assembly this session, produced an EMPTY migration, and a follow-up `remove --no-build` deleted the wrong
(committed) migration + reverted the snapshot (recovered via `git checkout`).

---

## (superseded) 2026-06-13 — T091 **P4 (adoption + versioning) DONE**

**Shipped T091 Phase 4** — the version-pinned adoption layer. Institutions now adopt national
(College-owned) curriculum *versions* explicitly; trainees and curriculum credit are pinned to the
adopted version (the CMSA model). One code commit on `master` (**nothing pushed**).

**Committed (`c592556`, master, unpushed):**
- **Domain** — new `InstitutionCurriculumAdoption` (Institution adopts a national `Curriculum` version
  per discipline; **partial unique index = one active adoption per institution+sub-speciality**; all FKs
  Restrict). `TraineeProfile.AdoptionId` pins the trainee to the adopted version (nullable; new admissions
  always set it). `Curriculum.CloneAsNewVersion` now clones **only national-core items** (skips local
  extras, which stay pinned to their version) and **preserves stage-level JSON** (was dropping it — latent bug).
- **Application** — `Features/Adoptions`: `AdoptCurriculum` (adopt/re-adopt; re-adoption supersedes the
  current active record for that discipline) + `ListAdoptions` (Administrator any/all; InstitutionalAdmin
  own-institution only) + DTO/mappings. `AdmitTrainee`/`UpdateTraineeProfile` resolve & **require** the
  institution's active adoption for the chosen version (**hard gate** via `TraineeAdoptionResolver` —
  admission into a non-adopted version/discipline is rejected). `GetCurricula`/`GetEpas` narrowed for
  InstitutionalAdmin to **adopted-only** (this also fixes the **previously-empty** trainee-admit curriculum
  dropdown for InstitutionalAdmins — they had no College claim so saw nothing).
- **Infrastructure** — `CreditApplier` now resolves the active trainee profile and **scopes every
  curriculum-item match** to that trainee's adopted curriculum version **+ own-institution local extras**
  (no credit leak across versions or onto another institution's local items sharing an EPA); returns nothing
  when there is no active trainee profile. EF config + DbSet + TraineeProfile FK; migration
  `T091_CurriculumAdoption` (fresh-DB).
- **+9 tests.** Unit suites green: **Domain 50, Application 298, Architecture 19, Web 43, Infrastructure 10.**
  Integration NOT run (Docker). Production code (`Wombat.Web`, transitively all src) builds **clean in Release**.

**⚠️ Pre-existing, NOT mine, blocks only `dotnet build Wombat.sln -c Release`:** a nullable warning-as-error
in `tests/.../CompleteTraineeProfileCommandHandlerTests.cs:43` (`m.Tags.Contains(...)`, `Tags` possibly null).
Untouched by P4; all unit suites pass in Debug and production src is clean in Release. Trivial one-char fix
(`m.Tags!.Contains` / `?.Contains(...) == true`) — sweep it whenever, or fold into P6 test work.

**⚠️ EF CLI gotcha learned this session:** `dotnet ef migrations add/remove --no-build` used a **stale**
assembly → produced an EMPTY migration, and the follow-up `remove --no-build` then deleted the **wrong**
(committed P3) migration + reverted the snapshot. Recovered via `git checkout` of the committed migration
files + snapshot. **Always run `dotnet ef migrations add` WITHOUT `--no-build`** (let it rebuild) so the model
reflects current code.

**▶ Next: P5 — Web surfaces.** College admin pages (Administrator manages Colleges); national EPA/curriculum
authoring surfaced to CollegeAdmin; **institution adoption pages** (InstitutionalAdmin adopts/re-adopts via
`AdoptCurriculum`/`ListAdoptions`, manages local extras); add College display columns to the admin
EPA/curriculum/speciality lists; nav updates. NOTE: until P5 ships the adoption UI, an InstitutionalAdmin has
no way to create an adoption, so their trainee-admit dropdown stays empty in the live app (Administrator sees
all) — P5 closes that. Then **P6** scenario rebuild on a fresh DB (national CMSA Paediatrics catalogue + KGK
adoption) — old `act*-v2-*` snapshots are invalid under the new schema. **Opus** recommended for P5/P6 (auth +
UI wiring + migration/scenario correctness). DB must be rebuilt fresh.

---

## (superseded) 2026-06-13 earlier — T091 P1–P3 DONE; T090 + T089 shipped

**Long, productive session.** Shipped T090 (user-admin scope-leak fix) + T089 (branding/Fraunces wordmark),
then drove the **T091 national EPA/curriculum redesign through Phases 1–3** (College ownership + local extras),
all green and committed. **Nothing pushed** — `master` is far ahead of origin (T090, T089, T091 P1–P3 + this
session's earlier work). Pushing `master` is the standing item.

**Big domain redesign in progress.** EPAs/curricula must be nationally owned by the **Colleges of Medicine of
SA (CMSA)**; institutions **adopt** them (don't author). Decided design (4 forks) + 6-phase plan in
`Rewrite/Tasks/T091-national-epa-curriculum-catalogue.md`. Decisions: College layer; national core + local
extras; per-College `CollegeAdmin` role; explicit version-pinned adoption.

**Committed (master, unpushed):**
- **P1 (`1c18bda`)** — College entity + CollegeAdmin role/claim/policy/scope helpers (additive).
- **P2 (`f63d993`)** — re-parented `Speciality` InstitutionId→CollegeId; added direct `TraineeProfile.InstitutionId`;
  re-scoped ~35 Application handlers (catalogue→`CanAccessCollege`; trainees/forms/panels/activity-types→direct
  institution); Infra seeders/PDF/reference-data; Web admin pages compile; migration
  `T091_ReparentSpecialityToCollege` (fresh-DB). Scope-guard tests rewritten to college-scoping; `CollegeAdmin`
  test principal added.
- **P3 (`03fad97` additive + `2714f11` logic)** — local extras: nullable `OwningInstitutionId` on `Epa` +
  `CurriculumItem` (null=national core, set=institution-local); partial unique indexes (national codes per
  sub-speciality, local per sub-speciality+institution); migration `T091_LocalExtraDiscriminators`. Auth wired:
  Create EPA / Add curriculum item — CollegeAdmin/Admin author national, InstitutionalAdmin adds local;
  Update/Deactivate/Remove branch national→`CanAccessCollege` / local→`CanAccessInstitution(owner)` (+ coarse
  curriculum gate so item existence isn't leaked); list shows national + own-local. +5 tests.
  **All unit suites green: Domain 50, Application 289, Architecture 19, Web 43, Infrastructure 10.**

**⚠️ Provisional / deferred to later phases (flagged inline with T091 comments):** InstitutionalAdmins see the
WHOLE national EPA/curriculum catalogue (no adoption narrowing until P4); Web admin pages show no College column
and don't filter specialities (P5 rework); speciality-scoped DecisionPanels must carry their own InstitutionId.
**DB must be rebuilt fresh** — old `act*-v2-*` snapshots are invalid under the new schema.

**▶ Next: P4** — adoption + versioning: new `InstitutionCurriculumAdoption` entity (Institution adopts a
national Curriculum *version*; AdoptedOn/IsActive; unique active per institution+discipline); adopt/re-adopt
flows; trainee linkage (TraineeProfile pins to adopted version); CreditApplier honours adoption + local items;
then narrow the "InstitutionalAdmin sees whole catalogue" provisional bits to adopted-only. Then **P5** Web
surfaces (College admin + adoption + College display columns), **P6** scenario rebuild on fresh DB.
**Opus** throughout. Integration tests NOT run (need Docker).

---

## ✅ SESSION 2026-06-10 (Opus) — T090 + T089 committed; T091 opened

- **T090 (`f800b99`)** — confined InstitutionalAdmin user administration to own-institution scope. User spotted
  (during T089 logo review) that an InstitutionalAdmin could see/“Manage” the global `admin@wombat.local` at
  `/admin/users`. Fixed: `ListUsersQuery`/`GetUserByIdQuery` own-institution only; 4 mutation handlers reject
  unscoped targets for non-Administrators (closed a latent write gap). +4 tests. Application **284** green.
  Task file `Tasks/T090-institutional-admin-user-scope-leak.md`.
- **T089 (`fa9928e`)** — branding & polish committed. Adds the self-hosted **variable Fraunces** wordmark
  (`wwwroot/fonts/fraunces-var.woff2`; OFL/GPLv3) for the brand lockup only — weight 500, WONK on, SOFT 0,
  `font-optical-sizing: auto`; new `--font-display`/`--font-display-settings` tokens. Nav mark 40px + centered
  in the top bar; login mark 56px. Palette `#2d6cdf`/`#3498db`, logo, favicons/manifest, arrow-left.svg,
  nav-toggler a11y. Web bUnit **43/43** green. User reviewed the wordmark live and approved (matched their
  Google Fonts specimen at weight ~500).
- **`master` is well ahead of origin and still unpushed** (now incl. T090 + T089). Standing item.

**⚠️ Tooling reminders (unchanged):** dev server via the **PowerShell** tool
(`$env:ASPNETCORE_ENVIRONMENT='Development'; dotnet run …`), not Bash; **stop the dev server before
`dotnet test`/`dotnet build`/`db-snapshot.ps1 restore`**; keep `psql` calls solo. `AuditEntries` is
append-only (DB trigger blocks DELETE/UPDATE).

---

## ⭐ SESSION FINALIZED — 2026-06-07 (Opus) — Appendix cross-cutting spot-checks re-run in v2 (all PASS; T083–T086 fixes hold)

**Re-ran the entire Appendix (A.1–A.4) from `act5-v2-final`, UI-driven via Playwright, DB-verified.** Goal was
to confirm the four 2026-06-04 findings (T083–T086, all since fixed) hold from the clean v2 state, plus
re-confirm the things that passed. **No product code changed. Nothing committed** (this handoff only).
DB artifacts (1 export request, 1 erasure, 1 manual job run) were **rolled back by restoring `act5-v2-final`**;
verified clean afterward (Ndlovu restored, 0 DataRightsRequests). **Dev server STOPPED.** No new secrets
(reused existing pw + the persistent `Wombat:PseudonymSalt` dev user-secret).

**Fix verifications (all HOLD in v2):**
- **T083 (export download) ✓** — `/account/data-rights/download/{id}` returns **HTTP 200, `application/zip`,
  85,643-byte "PK" ZIP** (was the 404 bug). Realistic non-existent GUID → **404** (no leak).
- **T084 (atomic erasure) ✓** — Ndlovu erasure = **pseudonymisation**: Email/First/Last/NormalizedEmail
  cleared, `UserName`→`deleted_user_446aa09f`, account locked (`LockoutEnd=infinity`), **Id retained**,
  request→**Completed**, **3 audit entries retained**. The T084 **Retry/Resume** recovery buttons are
  present on the request page.
- **T085 (chart SR fallback) ✓** — Mahlangu's review (id 4) renders TrajectoryCharts with `role=img` +
  `aria-label` **and** the `<table class="visually-hidden">` fallback (caption=aria-label, Date/Rating/Source,
  populated). (Molefe's reviews have no chart — his STARs came via committee staging, not rated activities.)
- **T086 (contrast + tap-target) ✓** — `--muted-text`=`rgb(104 111 119)` → **4.83:1** on page bg (≥AA 4.5);
  `.btn` min-height **28px** (≥WCAG 2.5.8 24px).

**Re-confirmed passes:** A.2 jobs (9 jobs listed; manual Run-now on assessor-pending-nudge recorded a
`ScheduledJobRun` Succeeded/91ms with Triggered-by = admin id, vs `scheduler`); A.3 SSO degrades gracefully
(no local IdP banner + form gated; login paths still deferred, invariants unit-tested); A.4.1 mobile @375px
(no horizontal overflow, hamburger present). A.4.2 keyboard not separately re-driven (covered by T048, unchanged).

**3 minor new observations (none block anything; not ticketed):**
- **Guid.Empty download → 500** instead of 404/400. A **T088 side-effect**: the `RequestId must not be empty`
  validator now throws `ValidationException`, which the minimal-API download endpoint doesn't catch. No real
  download link carries an empty GUID — edge-case robustness only. (Fix: catch ValidationException → 400/404.)
- **`/icons/arrow-left.svg` 404** on the "Back to jobs" / "Back to list" link icon (missing asset; cosmetic).
- **Mobile nav toggler is an unlabeled `<input class="navbar-toggler">`** (no `aria-label`/`aria-expanded`) —
  minor a11y nit, pre-existing.

**Tests:** not re-run this session (no product code changed); last green = the T087/T088 session + this
session's verify pass (Domain 50, Application 280, Architecture 19, Web 43). **DB:** `act5-v2-final` (restored,
clean). Snapshots unchanged.

**▶ Recommended next:** the scenario + Appendix are now fully validated in v2 and every finding is closed.
The remaining real item is **pushing `master` to origin** (local master is well ahead, never pushed) — **Opus**
for release judgement; review the unpushed history first. The 3 minor observations above could optionally be
swept into a tiny polish task. Start any play-through from `tools\db-snapshot.ps1 restore act5-v2-final`.

**⚠️ Tooling reminders (unchanged):** dev server via the **PowerShell** tool
(`$env:ASPNETCORE_ENVIRONMENT='Development'; dotnet run …`), not Bash; **stop the dev server before
`dotnet test`/`dotnet build`/`db-snapshot.ps1 restore`**; keep `psql` calls solo (it lives at
`C:\Program Files\PostgreSQL\16\bin\psql.exe`, not on PATH); psql here-strings must be **ASCII**.
`AuditEntries` is append-only (a DB trigger blocks DELETE/UPDATE).

---

## ⭐ SESSION FINALIZED — 2026-06-07 (Opus) — clean v2 replay (Acts 1–5) DONE + 2 code fixes shipped

**Two-part session.** (1) Replayed the **entire linear Paediatrics scenario (Acts 1–5)** clean from a
fresh DB, UI-driven, DB-verified at every step — six per-act snapshots banked, ending at **`act5-v2-final`**
(Molefe graduated with 15/15 STARs; 16 users / 10 activities / 6 reviews / 1 resolved appeal). (2) Fixed
the real findings the replay surfaced, each a task-commit on `master` (**nothing pushed**):

- **T087 (`0cc9bf9`)** — atomic committee-appeal resolution (**F-4F-1**): moved the Remitted
  replacement-decision guard before `appeal.Resolve()` so a bad request throws with zero mutation
  (no more stranded `UnderAppeal`). +1 domain test.
- **T088 (`d054b6c`)** — wired FluentValidation into the MediatR pipeline (`ValidationBehavior` inside the
  audit behaviour): ~75 validators were registered but **never executed** (the root cause that let the bad
  appeal request reach the domain). +2 tests. Live-verified valid commands still pass.

**Non-bugs:** **F-5-DATE retracted** (completion-date input already exists); **F-4D-1** is a setup config
gap (`SubSpecialities.DefaultEntrustmentScaleId`), not a defect.

**Tests (all green):** Domain **50**, Application **280**, Architecture **19**, Web **43**. Integration NOT
run (no Docker on this box). Full solution builds clean. **Dev server STOPPED; live DB restored to
`act5-v2-final`.** No new secrets.

**▶ Recommended next:** optionally re-run the **Appendix** cross-cutting spot-checks in v2 (already played +
fixed pre-v2 as T083–T086), or begin **pushing** `master` to origin (local `master` is now well ahead,
never pushed). **Sonnet** fine for the Appendix grind; **Opus** for release/push judgement. Start any
play-through from `tools\db-snapshot.ps1 restore act5-v2-final` (or an earlier per-act snapshot).

**⚠️ Tooling reminders (unchanged):** dev server via the **PowerShell** tool
(`$env:ASPNETCORE_ENVIRONMENT='Development'; dotnet run …`), not Bash; **stop the dev server before
`dotnet test`/`dotnet build`/`db-snapshot.ps1 restore`**; keep `psql` calls solo; psql here-strings must be
**ASCII** (em-dashes trip UTF-8). `AuditEntries` is append-only (a DB trigger blocks DELETE/UPDATE).

---

## ✅ CLEAN FULL REPLAY COMPLETE — 2026-06-07 (Opus) — Acts 1–5 ALL DONE (linear scenario fully replayed)

**The entire Paediatrics scenario (Acts 1–5) has been replayed clean from a fresh DB, end-to-end, UI-driven,
and DB-verified at every step.** Final snapshot **`act5-v2-final`** is the full end-state. Per-act snapshots:
`after-act-1-replay-v2`, `after-act-2-replay-v2`, `act3-v2-AD`, `act3-v2-final`, `act4-v2-final`, `act5-v2-final`.
**Dev server is STOPPED.** No product code changed across the whole replay; no new secrets (reused
`Mbatha@KGK2026!` / `Act2Pass!123` / `ChangeThisAdmin123!`).

**Final state (DB-verified):** 16 users; 5 active trainee profiles + **1 completed (Molefe, graduated:
Trainee role removed, profile inactive)**; 10 activities; 6 committee reviews; **15 EntrustmentDecisions
(Molefe's full STAR set across all 15 EPAs)**; 1 resolved appeal.

**Act 5 (graduation) — all 5 goals met:** final PreGraduation review (id 6) → **Graduate** decision (T081);
chair staged 12 remaining STARs (2 via UI, 10 bulk-loaded via SQL — staging proven in Act 4) → ratify
**atomically issued all → Molefe 15/15 EPAs**; portfolio PDF generated (`portfolio-d058b3974b9c.pdf`, 96,831 B)
and **reproduced byte-for-byte by Coordinator Smit** (T078 deterministic + T079 Coordinator access); Mbatha
**Mark-complete** removed the Trainee role + sent graduation email (T080). ~~F-5-DATE~~ **RETRACTED** —
the Mark-complete form *does* have a completion-date input (`#completion-date`, defaulting to today);
the replay just left it at the default. Not a product gap.

**▶ Next (optional):** the only unplayed part is the **Appendix** cross-cutting spot-checks (data rights,
scheduled jobs, SSO, mobile/a11y) — `scenario-paediatrics.md` Appendix. Those were already played + fixed
in the pre-v2 sessions (T083–T086); a v2 re-run is optional. Otherwise the v2 clean replay is **done** —
consider this the canonical from-scratch validation that the rewrite works end-to-end. Restore any per-act
snapshot to revisit. **Sonnet** fine for the Appendix grind.

**Findings follow-up (2026-06-07, Opus):** the one real code bug surfaced by the replay is fixed —
**T087 (`0cc9bf9`, master)** makes `CommitteeReview.ResolveAppeal` atomic (F-4F-1). Domain 49→50,
Application 278, all green. **F-5-DATE retracted** (the completion-date input exists). **F-4D-1** is a
play-through config gap (set `SubSpecialities.DefaultEntrustmentScaleId` during setup), not a product
bug. **T088 (`d054b6c`, master)** wired FluentValidation into the MediatR pipeline (a new
`ValidationBehavior` inside the audit behaviour) — ~75 validators that were registered but never executed
now run before their handlers; live-verified valid commands still pass. Tests after both fixes: Domain 50,
Application 280, Architecture 19, Web 43 — all green (Integration not run, no Docker).

---

## (superseded) Act 4 detail — kept for reference

**Act 4 (annual review + STARs + appeal) fully replayed from the Act-3 end-state and DB-verified.** All 6
Act-4 goals met. Snapshot **`act4-v2-final`**.

**Act-4 end-state (DB-verified):** 5 CommitteeReviews (id 1–5; Molefe=1/PreGraduation, rest AnnualProgression,
all panel 1) — reviews 1/2/3/5 **Ratified** (State 4), review 4 (Mahlangu) **Final** (State 6, appealed+resolved).
**6 CommitteeDecisions** (5 original + 1 replacement): Molefe SatisfactoryProgress, Dlamini SatisfactoryProgress,
du Plessis SatisfactoryWithObservations, Mahlangu InadequateProgressAdditionalTraining (+ replacement decision id 6,
**SupersedesDecisionId=4**), Ndlovu OutcomeDeferred. **3 EntrustmentDecisions (Molefe STARs):** PAED-001 & PAED-006
Unsupervised (level id 9), PAED-013 Indirect supervision (level id 8) — all Active, issued atomically on ratify.
**1 CommitteeAppeal** (Mahlangu, Remitted). Verified: T075 InstAdmin scheduling, T081 Graduate category present,
evidence freezes on Start, STAR-issuance atomic on ratify (3 pending → 3 issued, 0 left).

**▶ Next: Act 5 (graduation + STAR augmentation + portfolio PDF)** — `scenario-paediatrics.md` Act 5.
Molefe (review id 1, PreGraduation, currently 3 STARs) is the graduand: schedule a final pre-graduation
review, chair stages STARs for the remaining 12 EPAs → ratify issues all → 15 EntrustmentDecisions; then
Graduate decision (T081) + Mark-complete lifecycle (T080, removes Trainee role); portfolio PDF export
(T077/T078 deterministic, T079 Coordinator export). Prior Act-5 fixes (T077–T081) are already in code.
**Opus** recommended (entrustment augmentation + QuestPDF). Restore `act4-v2-final` to start.

**Two v2 findings this Act:**
- **F-4F-1 (NEW, real bug — non-atomic appeal resolution) — ✅ FIXED (T087, on master):** submitting a **Remitted** appeal *without* the
  replacement decision (the `appeal-category`/`appeal-rationale` fields, which only render after selecting
  Remitted) persisted the appeal's `Outcome`+`ResolvedOn` but did NOT create the replacement decision or
  transition the review — leaving the appeal marked resolved, the review stuck `UnderAppeal`, and the UI
  rejecting any retry ("There is no open appeal to resolve."). Mirrors T084 (non-atomic erasure approve).
  Worked around by clearing `ResolvedOn`/`ResolvedByUserId` on the `CommitteeAppeals` row via psql, then
  re-resolving correctly (→ review Final + replacement decision). **Ticket: the Remitted replacement-decision
  guard must run before/inside the same transaction as `appeal.Resolve()`.**
- **F-4D-1 recurrence in v2 (config gap, patched):** the STAR `pending-level` picker showed **all** scales'
  levels because `SubSpecialities.DefaultEntrustmentScaleId` was **NULL** — the T076 programme-default-scale
  was never set during Act-1 v2 setup. Patched via psql (`SET DefaultEntrustmentScaleId=2 WHERE Id=2`); the
  picker then correctly filtered to the Paed scale. **Act-1 setup (or a future task) should set the
  sub-speciality default scale through the admin UI.** (Not in git — runtime config, captured in the snapshot.)

---

## (superseded) Act 3 detail — kept for reference

**Act 3 (operational rhythm) fully replayed from the Act-2 end-state and DB-verified.** All 7 Act-3 goals
met. Snapshot **`act3-v2-final`** is the Act-3 end-state. Intermediate snapshot `act3-v2-AD` (= through 3.D).

**Act-3 end-state (DB-verified):** 4 activity types built+published **v2** via the visual builder
(mini_cex_paed 11, dops_paed 14, procedure_log_paed 15, msf_paed 16; types 12/13/17/18/19/20 stay v1 —
not exercised, matching scenario scope). **10 activities:** 3 completed (2 Mini-CEX + 1 DOPS), 5 logged
(procedure log), 1 closed (MSF), 1 submitted (the deliberately-stale Mini-CEX). **Credit (CurriculumItemProgresses):**
PAED-001 (item 2) 2/30 reached 1; PAED-010 (item 11) 1/10 reached **1**; PAED-011 (item 12) 5/30 reached 3;
PAED-013 (item 14) 1/8 reached 1.

**Three prior Act-3 findings CONFIRMED FIXED in this v2 replay:**
- **F-3E-2** ✓ — PAED-010 carries stage-1 min = 2 (`{"1":2,...}`), so Mahlangu's year-1 level-2 DOPS now
  **reaches** (1/1), where the old play-through got reached=0.
- **F-3F-NOTE** ✓ — MSF credit rule targets `curriculum_item_id:14` → credit lands on **PAED-013**
  (not PAED-012 as before).
- **F-3G-1 / T074** ✓ — Smit's Coordinator "Stalled requests" panel now **surfaces** the stale submitted
  Mini-CEX (backdated 15d); it was dead ("No stalled requests") in the original play-through.
Also verified: schema-driven form pickers (EPA/User/Scale), workflow actor-DSL (`role:Trainee` submit,
`field:assessor_user_id` accept/complete, `creator` for MSF close), stage-aware CreditApplier, in-training
assessor (Patel) flagged-not-blocked, audit log lifecycle entries with real principal names (no JsonException,
no raw `[PRINCIPAL]`).

**Act-3 fidelity note (intentional):** schemas were built with the **essential field set** that drives the
workflow + credit chain (mini_cex/dops: epa_id, assessor_user_id, overall_level; procedure_log: epa_id,
procedure_code[Text], supervision_level; msf: self_rating, feedback_summary) — not the full 12-field
realistic forms. `procedure_code` is **Text** not Choice (the builder's Choice "Options" textarea format
was unverified and procedure_code isn't needed for credit). All platform mechanics are exercised; only form
richness is reduced. Canonical workflow/credit JSON is in `act3-rebuild-scratch.md` and was used verbatim.

**Builder mechanics learned (for Act 4+/future schema work):** the Form tab is a **live-bound visual
builder** — `Add field` creates a blank field selected in the editor; set `#field-label` → `#field-key` →
`#field-type` (a Scale type reveals `#field-scale`, bind to `2`) → `#field-required`; no per-field save.
Workflow/Credit are pasted into `#workflow-json` / `#credit-json` textareas. **Save draft** (real Playwright
click) persists Staging*; verify Staging* in DB; then **Publish** (bumps Version, promotes Staging→main).
Credit DSL supports both `{"epa_field":"epa_id"}` and `{"curriculum_item_id":N}` in `curriculum_item_match`.

**▶ Next: Act 4 (annual review + STARs + appeal)** — `scenario-paediatrics.md` Act 4. DecisionPanel id 1
exists (chair Zulu). See the Act-4 findings summary / prior detail below; key prior fixes (T075 InstAdmin
scheduling, T076 programme default scale, T081 Graduate category) are already in code. **Opus** recommended
(entrustment-decision augmentation + committee flow). Restore `act3-v2-final` to start.

---

## (superseded) Act 2 detail — kept for reference

**Act 2 (onboarding) fully replayed from the Act-1 end-state and DB-verified.** All 6 Act-2 goals met.
Snapshot **`after-act-2-replay-v2`** is the Act-2 end-state. This replay ran
notably cleaner than the original Act-2 play-through — every prior workaround is now obsolete (see below).

**Act-2 end-state (DB-verified counts):** 16 users (14 KGK-relevant + 2 Demo-institution seed users),
**5 AssessorProfiles**, **6 TraineeProfiles** (5 KGK + 1 demo), **1 DecisionPanel** (4 members),
12 invitations (all Accepted, **no stale duplicates**), 0 activities / 0 reviews / 0 STARs.
- **People (all pw `Act2Pass!123`):** Smit=Coordinator; Zulu/Naidoo/Botha=CommitteeMember+Assessor;
  Patel/Khumalo=Assessor; van Rensburg=external CommitteeMember; Molefe/Dlamini/du Plessis/Mahlangu/
  Ndlovu=Trainee. (Mbatha stays `Mbatha@KGK2026!`.)
- **AssessorProfiles (TrainingStatus enum, T065):** Zulu/Naidoo/Botha/Khumalo=Trained(3),
  Patel=InTraining(1); all SpecialityId 2. van Rensburg has none (committee-only). Qualifications is
  **[Required]** — filled with a generic FCPaed(SA) line.
- **TraineeProfiles (ids 2–6, curriculum 2):** start dates set so the **computed stage** lands right
  (machine clock = 2026-06-06): Molefe 2023-01-15→2029-12-15 (stage 4), Dlamini 2024-01-15→2027-12-15
  (3), du Plessis 2025-01-15→2028-12-15 (2), Mahlangu & Ndlovu 2026-01-15→2029-12-15 (1). No Stage
  field on the form — stage is derived from `ProgrammeStartDate` (A2-8 still stands, non-blocking).
- **DecisionPanel id 1:** "Paed Annual Review Panel 2026", Scope=Speciality (Paediatrics), Chair=Zulu(1),
  Members=Botha+Naidoo(2), External=van Rensburg(3).

**Prior Act-2 findings now RESOLVED in code (this replay used the proper UI throughout — no hacks):**
- **A2-1** (T060): Coordinator/CommitteeMember invitations issue with Speciality blank. ✓
- **A2-2/A2-3/A2-4** (T061): real admin **Users surface** at `/admin/users/{guid}` (role add/remove,
  password reset, lockout, invitation revoke). Added the secondary **Assessor** role to Zulu/Naidoo/Botha
  here via the UI — the old `--dev-add-role` CLI hack is gone and no longer needed.
- **A2-9/A2-10/A2-11** (T062): `/committee/panels/new` now admits **InstitutionalAdmin** (Mbatha created
  the panel herself) and uses real **name pickers** (Scope/Speciality selects + Chair/Members/External
  candidate selects from the CommitteeMember pool) — no more raw int/GUID textareas.
- **A2-5** (T065): assessor **TrainingStatus enum** (NotStarted/InTraining/Provisional/Trained) replaces
  the bare date field.

**Act-2 automation notes (for Act 3):** invitation registration tokens are **hashed** (`TokenHash`) and
NOT recoverable from the DB — the inline registration URL must be captured at issue time. The naive
"grab first `<code>` with register?token=" races a **stale** URL from the previous issue → I cleared the
Invitations table and re-issued all 12 with a helper that tracks a **seen-set** and waits for a genuinely
new URL. The login/register pages are **server-rendered POST forms** (set `#id` values via native setter,
then a **real Playwright click** on the submit button so the full-page POST fires). Blazor `EditForm`s
(assessor profile, panel) need a **real Playwright click** on the submit button too — a JS `.click()`
does NOT trigger `OnValidSubmit`; and **`Qualifications` is [Required]** so a blank textarea silently
fails validation with no save. Trainee admit + panel use native-setter on `#id` + real click.

**Open Act-2 papercuts (non-blocking, carry forward):** A2-6 (assessor edit URL doesn't flip to `?id=`;
user dropdown doesn't narrow after save) and A2-8 (no Stage field on admit; stage derived from start
date, not surfaced in the Active list). Both cosmetic; documented in `scenario-paediatrics.md` Act-2.

**▶ Next: Act 3 (operational rhythm)** — `scenario-paediatrics.md` Act 3 (3.A–3.I). Build the full
`*_paed` form schemas via the visual builder (mini_cex_paed v2, procedure_log_paed v2, dops_paed v2,
msf_paed v2 at minimum — see `act3-rebuild-scratch.md` for the canonical workflow/credit JSON and the
per-schema build recipe; the JSON format is easy to get wrong), then have trainees submit activities and
assessors rate them, verifying curriculum credit. **EPA ids 2–16, activity-type ids 11–20, scale id 2,
curriculum id 2** (+ items, with PAED-010 stage-1=2). **Sonnet** is fine for the grind; escalate to Opus
for findings/domain calls. Act 3 is a large, multi-phase, builder-heavy effort — best run as its own
focused session (it consumed whole Opus sessions before). The dev server was left **running** on 5080.

> **⚠️ `act3-rebuild-scratch.md` cast UserIds are STALE for the v2 DB.** Use these **v2** ids:
> Trainees (curriculum 2): Molefe `23c47fa0-2402-4ba3-bf0b-54de220ca928` (stage 4), Dlamini
> `7d82d81e-4758-40c1-8584-537334462397` (3), du Plessis `00c3efb1-4e64-4339-8284-3c7633309ca9` (2),
> Mahlangu `a6a75057-3ad7-477a-90e1-3d72a146de0a` (1), Ndlovu `ea7ceb10-72af-4a67-a206-d43f1bb17522` (1).
> Assessors: Naidoo `1e2a5fce-f4fd-4b4b-bc92-cbec8f6c4891`, Patel `f161448f-7f14-41ed-8a8b-cd64a4512636`
> (InTraining), Khumalo `dc7e8c0e-7301-454f-b5d1-85317d093d16`, Botha `7614c1db-38c6-4413-a934-6f1316229f3e`,
> Zulu `76577c95-8833-4b66-9ba7-e349fdc084b6`. TraineeProfile ids: Molefe 2, Dlamini 3, du Plessis 4,
> Mahlangu 5, Ndlovu 6. DecisionPanel id 1. The **Form tab is a visual builder only** (no raw-JSON paste);
> only the **Workflow** and **Credit** tabs accept pasted JSON. Patel is already InTraining via the enum
> (no need to NULL a date as the old scratch doc says — that predates T065).

**Act-1 finding (still open):** fresh-install seeding logs a non-fatal **"AuditEntries is append-only"**
EF exception (DevUserSeeder user upsert) — users/roles/Demo still seed. Ticket if it recurs.

---

## ✅ Act 1 replay detail (2026-06-05/06) — superseded by the summary above

**User asked for a full from-scratch replay** of the Paediatrics scenario (Acts 1–5 + Appendix) via the
UI. DB was **dropped + recreated empty** (`DROP/CREATE DATABASE wombat_t002_verify`), app re-migrated +
seeded on startup. Driving everything through Playwright per the user's choice ("grind everything through
the UI"). **No product code changed; nothing committed except this handoff.**

**Act 1 progress (all UI-driven, DB-verified):**
- **1.A ✓** institution **KGK** (id 2), speciality **Paediatrics** (id 2), sub-speciality **General
  Paediatrics** (id 2) — bootstrap admin.
- **1.B ✓** **Mbatha** invited (InstitutionalAdmin) + registered via the inline registration URL (no SMTP
  needed; T051). Password `Mbatha@KGK2026!` (already in pwd file). Logged-in flows work.
- **1.C ✓** **Paed General Entrustment Scale** (id 2), 5 ten-Cate levels — bootstrap admin (scales are
  Administrator-only, T057).
- **1.D ✓** **15 EPAs** PAED-001…015 (EPA ids **2–16**; Core 1–13, Elective 14–15) — as Mbatha.
- **1.E ✓** curriculum **FCPaed(SA) Part 1 v2026.1** (id 2) + **15 items**, all counts/levels/windows/
  weights/stage-JSON DB-verified. **PAED-010 entered with stage-1=2** (`{"1":2,"2":2,"3":3,"4":4}`) to
  reflect the **F-3E-2 resolution**, not the original table's dash.
- **1.F ◑ 1/10 DONE** — the **10 activity types** (minimal: metadata + default schema/workflow/credit +
  publish each, per the scenario's own reduced Act-1 scope; full *_paed schemas are built in Act 3).
  **`mini_cex_paed` built + published (id 11, v1, ScopeId 2)** — builder save-draft→publish flow
  verified on the fresh DB (T055 URL-flip works). **9 remaining:** cbd_paed, acat_paed, dops_paed,
  procedure_log_paed, msf_paed, reflective_note_paed, journal_club_paed, research_output_paed,
  teaching_session_paed. (10 seeded IM types occupy ids 1–10, so the Paed types get ids 11+.)

**Activity-type builder flow (verified):** `/admin/activity-types/new` → click **Metadata** tab →
set `#type-key`,`#type-name`,`#type-description`, `#type-scope`=`Speciality` (wait) →
`#type-scope-id`=`2` (KGK/Paediatrics), ensure `#type-is-active` checked → **Save draft** (URL flips to
`/admin/activity-types/{id}`) → click **Publish**. ~3 calls/type.

**Snapshots:** **`act1-v2-minicex-published`** (latest, = 1.A–1.E + scale + mini_cex_paed published) and
`act1-v2-pre-activitytypes` (= through 1.E + scale). Restore the latest to resume Phase 1.F at type 2/10.

**Finding so far (Act 1):** fresh-install seeding logs a non-fatal **"AuditEntries is append-only: UPDATE
and DELETE are not permitted"** EF exception (during DevUserSeeder user upsert) — users/roles/Demo still
seed correctly. Worth a task if it recurs (likely the audit interceptor attempting an UPDATE on an
existing dev user). Not yet ticketed.

**▶ Resume:** restore `act1-v2-pre-activitytypes`, log in as **Mbatha** (`Mbatha@KGK2026!`), build the 10
activity types via `/admin/activity-types/new` (minimal), then **Act 2** (onboarding). **Sonnet** is fine
for the grind. Useful automation pattern this session: drive Blazor forms with one `browser_evaluate`
that sets `#id` values, dispatches `input`+`change`, waits ~400ms, then clicks the submit button — works
reliably (use explicit element `#id`s, not positional indexing — positional got the scale name wrong once).

---


## ⭐ SESSION FINALIZED — 2026-06-05 (Opus) — all 4 Appendix findings fixed (T083–T086)

**Fixed every finding the Appendix surfaced**, each a self-contained task-commit on `master`
(**nothing pushed**). With this, the Paediatrics scenario is fully played **and** all its findings —
linear acts + appendix — are closed.

**Commits (chronological, `master`):**
- `0d437e3` **T083 (HIGH)** — mapped `GET /account/data-rights/download/{id:guid}` in `Program.cs`
  (dispatches `DownloadAccessReportQuery`, returns the ZIP via `Results.File`; auth/not-found → 404, no
  existence leak). The Export/Access download worked end-to-end (live: 200, `application/zip`, 90,704-byte
  `PK` ZIP). +5 Application tests.
- `f1a352e` **T084 (MED)** — `ApproveDataRightsRequest` validates `PseudonymSalt` **before**
  `entity.Approve()`, so a missing salt throws while still `Submitted` (no stranded Approved-but-not-erased).
  +3 Application tests.
- `bf2f9a4` **T085 (MOD)** — `TrajectoryChart` renders a `visually-hidden` data table (date/rating/source,
  caption = aria-label) for screen readers (WCAG 1.1.1). +1 bUnit test.
- `88aa0d3` **T086 (LOW)** — darkened `--muted-text` (→ `.page-subtitle` 4.45→**4.83** AA) and added
  `min-height:1.75rem` to `.btn` (small controls 23→**28px**, WCAG 2.5.8). Live-verified.

**Tests (all green):** Domain 49, **Application 278** (+8), Architecture 19, **Web 43** (+1),
Infrastructure 10. Integration not run (Docker).

**DB:** `followups-complete` is the current restored state — clean (the T083 live-verification seeded one
export row, deleted afterward). The dev `Wombat:PseudonymSalt` user-secret persists (in
`pwd_DO_NOT_COMMIT.txt`); **production must set it.** No snapshot changes this session.

**▶ Recommended next:** scenario + findings are all closed. Options: (1) a fresh end-to-end **replay**
to confirm the fixes hold from a clean state; (2) start **pushing** to `origin` (local `master` is now
~140 commits ahead, never pushed) once you're ready; (3) pick up any remaining `Rewrite/practical-plan.md`
items. **Sonnet** is fine for a replay; **Opus** if pushing/release prep involves judgement. Start any
play-through from `tools\db-snapshot.ps1 restore followups-complete`.

**⚠️ Tooling reminders (unchanged):** dev server via the **PowerShell** tool
(`$env:ASPNETCORE_ENVIRONMENT='Development'; dotnet run …`), not Bash; **stop the dev server before
`dotnet test`/`dotnet build`** (it locks the build outputs → MSB3027); restore needs the server stopped;
keep `psql` solo.

---

## ⭐ SESSION FINALIZED — 2026-06-04 (Opus) — Appendix cross-cutting spot-checks played

**Played the whole Appendix** (data rights, scheduled jobs, SSO, mobile/a11y) from snapshot
`followups-complete`. **No product code changed this session** — it's a find-and-triage pass. Found
**2 real data-rights bugs (1 HIGH)** + 3 a11y polish items, all ticketed as task files. One **docs
commit** on `master` (nothing pushed). DB spot-check artifacts were rolled back by restoring
`followups-complete`. **With this, the entire Paediatrics scenario (Acts 1–5 + Appendix) is played.**

**New findings → task files (all OPEN, none block the linear acts):**
- **T083 (HIGH)** — data-rights Export/Access **"Download" 404s**: `/account/data-rights/download/{id}`
  is never mapped in `Program.cs`; `DownloadAccessReportQuery` is never dispatched from `Wombat.Web`
  (grep: 0 refs). Export approves → `Completed` but the artifact is undeliverable. Live-verified (fetch +
  click both 404).
- **T084 (MED)** — **erasure approve is non-atomic**: if the executor throws (dev was missing
  `Wombat:PseudonymSalt`), the request is left `Approved` but data un-erased, with no UI retry. On a
  clean run erasure = **pseudonymisation** (name/email cleared, `UserName`→`deleted_user_<hash>`, account
  locked, Id retained), request → `Completed`.
- **T085 (MOD)** — TrajectoryChart has `aria-label` + per-point `<title>` but **no text/tabular
  fallback** (WCAG 1.1.1).
- **T086 (LOW)** — muted `#6c757d` on page bg `#f8f9fa` = 4.45:1 (just under AA 4.5); some controls
  21–23px tall (< WCAG 2.5.8 24px). Plus a tiny note: list `?key=`/`?status=` query params don't
  pre-filter on load.

**Passed cleanly:** A.2 scheduled jobs (9 jobs, all Succeeded; manual Run-now records the triggering
user); A.3 SSO degrades gracefully (no dev IdP) with invariants unit-tested (Administrator-blocked,
group→role, PendingTrainee fallback, scope guards); A.4 mobile (no h-overflow, hamburger works), keyboard
(logical focus order, T048 h1 ring holds). Full Actual/Gap in `scenario-paediatrics.md` §"Appendix
findings summary".

**Dev-env change (persists, not in git):** set `Wombat:PseudonymSalt` via `dotnet user-secrets` on
`Wombat.Web` so erasure runs locally — value recorded in `pwd_DO_NOT_COMMIT.txt`. Production must set it.

**Tests:** not re-run this session (no product code changed); last green at the T082 session (Domain 49,
Infrastructure 10, Application 270, Architecture 19, Web 42). **DB:** `followups-complete` is the current
restored state (unchanged by the appendix). Snapshots unchanged.

**▶ Recommended next:** fix the triaged findings, **starting with T083 (HIGH)** — the export download is
a real GDPR-deliverable gap and is a small, self-contained Program.cs endpoint + test. Then T084
(erasure atomicity), then T085/T086 (a11y). **Opus** for T083/T084 (endpoint + transactional/handler
correctness); **Sonnet** is fine for T085/T086 (a11y polish). Start from
`tools\db-snapshot.ps1 restore followups-complete`.

**⚠️ Tooling reminders (unchanged):** dev server via the **PowerShell** tool
(`$env:ASPNETCORE_ENVIRONMENT='Development'; dotnet run …`), not Bash; don't `db-snapshot.ps1 take`
during a live browser request; keep `psql` solo; restore needs the dev server stopped (it holds DB
connections).

---

## ⭐ SESSION FINALIZED — 2026-06-02 (Opus) — all three deferred follow-ups completed

**Cleared the last three deferred follow-ups** (F-4B-1 d, F-3E-2, F-3F-NOTE). One code task-commit
on `master` (**nothing pushed**) + two runtime-data patches captured in a new DB snapshot. With this,
**every finding from the linear Acts 1–5 is closed** — only the cross-cutting Appendix spot-checks
remain unplayed.

**Code (1 commit, `master`):**
- `59a004d` **T082** — review-type field on committee reviews (**F-4B-1 d**).
  `CommitteeReviewType { AnnualProgression=1, PreGraduation=2 }`: domain enum + `CommitteeReview.ReviewType`,
  integer column via migration **`20260602110316_CommitteeReviewType`** (dotnet-ef scaffolded; backfills
  existing reviews to AnnualProgression — enum has no 0, so the scaffolder's `defaultValue:0` was changed
  to `1`), command/DTO/mapping plumbing, all three list projections, schedule-form dropdown + detail/list
  "Type" display. Orthogonal to `IsFormative`. +2 Application tests. Task file `Tasks/T082-committee-review-type.md`.
  **Live-verified** (admin browser): list Type column, schedule-form Review-type dropdown, detail Type row.

**Runtime-data patches (NOT in git — curriculum items & credit rules are builder-authored, never seeded
in code; captured in snapshot `followups-complete`):**
- **F-3E-2 RESOLVED** — PAED-010 (curriculum item 11) `MinimumLevelByStageJson` `{"2":2,"3":3,"4":4}` →
  `{"1":2,"2":2,"3":3,"4":4}` (year-1 min = level 2 per scenario step 3.E). Mahlangu's level-2 DOPS now
  meets it → progress row 3 reached 0→1 (**PAED-010 1/10 reached 1/10**).
- **F-3F-NOTE RESOLVED** — MSF credit rule (`ActivityTypes` 16 + published `ActivityTypeVersions` 24)
  `curriculum_item_id` 13→14; Molefe's MSF credit (progress row 4) moved PAED-012 → **PAED-013** (1/6
  reached 1/6; rule has no level gate). All five updates ran in one transaction.
- Also tagged Molefe's Act 5 final review (review 6) **PreGraduation** so the snapshot exercises the new
  T082 field; the other five reviews stay AnnualProgression.

**Tests (all green):** Domain 49, Infrastructure 10, **Application 270** (+2), Architecture 19, Web 42.
Integration NOT run (Testcontainers needs Docker — none on this box).

**DB snapshots:** new **`followups-complete`** = `act5-complete` + the T082 migration + the two data
fixes + Molefe-review tagged PreGraduation. Restore with `tools\db-snapshot.ps1 restore followups-complete`.
The pure `act5-complete` is kept unchanged as the Act 5 play-through record. The T082 migration was
applied to the live dev DB via `dotnet ef database update` before snapshotting.

**No new secrets** — reused `admin@wombat.local` / `ChangeThisAdmin123!` for the browser verification;
all already in `pwd_DO_NOT_COMMIT.txt`.

**▶ Recommended next: the Appendix** cross-cutting spot-checks (data rights, scheduled jobs, SSO,
mobile/a11y) in `scenario-paediatrics.md` — the only outstanding work; the linear acts are all played
and every finding is closed. **Sonnet** is fine for that grind. Start from
`tools\db-snapshot.ps1 restore followups-complete`.

**⚠️ Tooling reminders (unchanged):** start the dev server via the **PowerShell** tool
(`$env:ASPNETCORE_ENVIRONMENT='Development'; dotnet run …`), NOT Bash (bash mangles `$env:` → silently
runs Production on 5080). Don't `db-snapshot.ps1 take` while a browser request is in flight (template
clone drops DB connections → 500). Keep `psql` calls solo. `dotnet ef` 10.0.3 works here for scaffolding
migrations against the live DB.

---

## ⭐ SESSION FINALIZED — 2026-06-01 (Opus) — Acts 4 & 5 played; all findings fixed

**Big session.** Played **Act 4** (annual review + STARs + appeal) and **Act 5** (graduation + portfolio
PDF) end-to-end via Playwright, and fixed **every** finding raised — 9 task-commits, all on `master`,
**nothing pushed**. The linear scenario (Acts 1–5) is now fully played with all findings closed.

**Commits this session (chronological, `master`):**
- `46126bf` **T075** — InstitutionalAdmin committee-review scheduling (F-4A-1, scope-aware + tests).
- `a2c786f` docs (T075 hash).
- `b01920f` **F-4A-2** resolved A1 (docs-only) — committee ratify/appeals stay with the chair.
- `3186a85` **T076** — programme default entrustment scale (F-4D-1; SubSpeciality.DefaultEntrustmentScaleId
  + migration; STAR level picker filters to it).
- `282aab3` **F-4B-1** — committee nav links for InstAdmin/Administrator + doc reconcile (sampling
  warning + evidence already exist; review-type field deferred).
- `7949b8f` docs (Act 5 play-through + the 4 then-open findings).
- `af46b5a` **T077+T078** — portfolio STAR section (F-5-2) + deterministic PDFs (F-5-3).
- `b0bff7d` docs (T077/T078 hash).
- `4cc1207` **T079+T080** — Coordinator portfolio export (F-5-5) + graduation/completion lifecycle
  (F-5-4: `TraineeProfile.CompletedOn` + Mark-complete + role removal + graduation email + migration).
- `0a0a7d8` **T081** — `Graduate` committee decision category (F-5-1).
- _this commit_ — session finalization.

**Two EF migrations added this session** (both auto-applied at startup; both dotnet-ef scaffolded):
`20260601161846_ProgrammeDefaultEntrustmentScale`, `20260601172756_TraineeCompletion`.

**Tests (all green):** Domain 49, Infrastructure 10, Application 268, Architecture 19, Web 42.
**Integration NOT run** (Testcontainers needs a Docker engine — none on this box). One *flaky*
Infrastructure failure recurs only on full-solution `dotnet test` when it races the dev-server kill;
isolated `dotnet test` of the Infrastructure project is a clean 10/10.

**DB snapshots (latest):** `act5-complete` (Molefe graduated: 15 STARs, profile completed via
Mark-complete, Trainee role removed; both new migrations applied). Earlier checkpoints kept:
`act4-A-scheduled`, `act4-molefe-ratified`, `act4-complete`, `act4-complete-t076`, `act5-complete`.
Restore with `tools\db-snapshot.ps1 restore <name>`.

**No new secrets** — reused `Mbatha@KGK2026!` (Mbatha) and the shared `Act2Pass!123`
(Zulu / Mahlangu / Smit), already in `pwd_DO_NOT_COMMIT.txt`.

**Open follow-ups (deferred, documented):** review-type field on committee reviews (F-4B-1 d — Annual
vs Pre-graduation; descriptive-only, nothing depends on it); F-3E-2 / F-3F-NOTE seed-config notes from
Act 3.

**▶ Recommended next: the Appendix** cross-cutting spot-checks (data rights, scheduled jobs, SSO,
mobile/a11y) in `scenario-paediatrics.md` — the linear acts are done. **Sonnet** is fine for that grind.

**⚠️ Tooling reminders:** start the dev server via the **PowerShell** tool (`$env:ASPNETCORE_ENVIRONMENT=
'Development'; dotnet run …`), NOT the Bash tool (bash mangles `$env:` — it silently runs `dotnet run`
in *Production* after the failed env-set, grabbing port 5080). Do **not** run `db-snapshot.ps1 take`
while a browser request is in flight (the template-clone drops the app's DB connections and 500s the
request) — snapshot while idle. Keep `psql` calls solo.

---

## ⭐ Act 5 played in full — 2026-06-01 (Opus) — graduation; findings fixed (T077–T081)

**Act 5 (graduation + STAR augmentation + portfolio PDF) played end-to-end from
`act4-complete-t076`. No code changes — all four findings are design/feature decisions left OPEN for
the user (same find-then-decide rhythm as Act 4). Snapshot `act5-complete`.**

**Worked (DB/PDF-verified):** Mbatha scheduled Molefe's final review (T075); chair Zulu staged STARs
for the remaining 12 EPAs (the **T076 scale filter applied** — only Paed levels offered); ratify
issued them atomically → **Molefe has 15 EntrustmentDecisions covering all 15 EPAs** (12 Unsupervised
+ 3 Indirect). Portfolio PDF generated cleanly (cover/summary/committee-reviews/activities/MSF/audit).
*(2 STARs staged via UI to verify; the other 10 bulk-loaded via SQL into `PendingEntrustmentDecisions`
since staging was already proven in Act 4 — ratify issued all 12 identically.)*

**Findings (all OPEN — detail + repro in `scenario-paediatrics.md` § "Act 5 findings summary"):**
- **F-5-1:** no `Graduate`/`Complete` committee decision category (recorded graduation as
  `SatisfactoryProgress`).
- **F-5-2 — RESOLVED (T077, `af46b5a`):** portfolio PDF now renders a "Statements of Awarded
  Responsibility (STARs)" table (all active `EntrustmentDecision`s). Live-verified: Molefe's portfolio
  lists all 15 STARs.
- **F-5-3 — RESOLVED (T078, `af46b5a`):** removed the wall-clock `Generated:` line + set fixed
  QuestPDF metadata → byte-for-byte reproducible. Live-verified: two exports → identical hash filename.
  +2 Infrastructure tests (8→10).
- **F-5-4 — RESOLVED (T080):** added a graduation lifecycle —
  `TraineeProfile.CompletedOn` + `Complete()`, `CompleteTraineeProfileCommand` (records date, deactivates,
  **removes Trainee role**, sends graduation email), "Mark complete" edit action, "Completed & closed"
  list section. Migration `20260601172756_TraineeCompletion`; +5 tests. Live-verified (Molefe → roles
  none, listed completed). F-5-1 (no graduation decision category) still open.
- **F-5-5 — RESOLVED (T079):** added Coordinator to `ExportPortfolioCommand`. Live-verified: Smit
  reproduced Mbatha's exact PDF (identical hash) — Step 5.5 now passes on authz + determinism.

**All five Act 5 findings now fixed (T077/T078, T079/T080, T081).** F-5-1 closed: added a `Graduate`
committee decision category (T081) — the committee can formally record graduation; the T080 lifecycle
then archives the trainee.

**Tests:** Domain 49, Infrastructure 10, Application 268, Architecture 19, Web 42 — all green. Integration
not run (Docker). DB snapshot **`act5-complete`** reflects the proper graduation (Molefe completed via
Mark-complete: `CompletedOn` set, Trainee role removed) + the `TraineeCompletion` migration.

**▶ Recommended next:** the **Appendix** cross-cutting spot-checks (data rights, scheduled jobs, SSO,
mobile/a11y) in `scenario-paediatrics.md`. **Sonnet** is fine for the Appendix grind. The linear acts
(1–5) are all played and **all their findings fixed** (F-3*, F-4*, F-5* all resolved).

## ⭐ Session finalized — 2026-06-01 (Opus) — Act 4 played in full + F-4A-1 fixed (T075)

**Act 4 (annual review + STARs + appeal) played end-to-end and DB-verified, after fixing one real
blocking bug. On `master`, nothing committed yet at time of writing — see "commit" note below.**
Started from `tools\db-snapshot.ps1 restore act3R-final-t065`.

**Bug found + fixed before 4.A could run — F-4A-1 / T075 (scope-aware, mirrors T063):**
InstitutionalAdmin (Prof Mbatha, the scenario's scheduler) was excluded from the committee-review
surface at both the page gate (`ReviewsSchedule`/`ReviewDetail`) and `DemandReviewScheduling`, even
though `DemandPanelAdministration` already admits them — so she could build the panel but not
schedule on it. Fixed: admit InstAdmin to `DemandReviewScheduling` + a panel-institution scope guard
in `ScheduleCommitteeReviewCommandHandler`; broadened `ListReviewsForPanelQuery` (Admin=all,
InstAdmin=own-institution, else member-based) and `GetCommitteeReviewByIdQuery` (InstAdmin view,
scope-checked); added InstAdmin to both page gates. Conduct actions (start/record/ratify/resolve)
stay chair-gated by design. **+6 tests** (`ReviewSchedulingScopeGuardTests`), Application 252→258.
Full task write-up: `Rewrite/Tasks/T075-institutional-admin-committee-review-scheduling.md`.

**Play-through result (all DB-verified; per-phase detail in `scenario-paediatrics.md` § "Act 4
findings summary"):** 5 reviews scheduled (Mbatha) → evidence frozen on Start (MSF/Mini-CEX/DOPS
captured; T033 trajectory renders for rated WBAs) → 5 decisions recorded (chair Zulu) → 3 STARs
staged for Molefe → all 5 ratified (**Molefe's ratify atomically issued 3 `EntrustmentDecision`
rows**) → Mahlangu lodged an appeal → chair resolved it `Remitted` (referral upheld, 6mo→3mo),
review `Final`, replacement decision supersedes the original. Decisions: Molefe/Dlamini
`SatisfactoryProgress`, du Plessis `SatisfactoryWithObservations`, Mahlangu
`InadequateProgressAdditionalTraining`, Ndlovu `OutcomeDeferred`.

**Tests:** Domain 45, Infrastructure 8, Application 258, Architecture 19, Web 42 — all green.
Integration not run (Docker). (One flaky Infrastructure failure on a full-solution run while the dev
server was being killed; re-ran isolated = 8/8.)

**DB snapshots:** `act4-A-scheduled`, `act4-molefe-ratified`, **`act4-complete`** (final, all
phases). Restore the final state with `tools\db-snapshot.ps1 restore act4-complete`.

**Findings still open (deferred, documented in the Act 4 findings summary):**
- **F-4A-2 (governance) — RESOLVED A1 (docs-only):** keep the code; ratification + appeals stay with
  the committee (chair), InstAdmin schedules/oversees only. Scenario steps 4.8/4.10 recast to the
  chair + a governance note added (separation of duties; appeals independent of the program overseer).
  Multi-role is supported, so an institutional lead granted `CommitteeMember` + seated as chair could
  legitimately ratify/resolve via membership — this scenario deliberately keeps them separate.
- **F-4D-1 (UI correctness) — RESOLVED (T076):** STAR "Authorised level" picker offered every scale's
  levels (duplicate 1–5 sets) and didn't narrow. Fixed via **Option A** — a programme default scale on
  `SubSpeciality` (`DefaultEntrustmentScaleId`): picker filters to it, stage handler rejects off-scale
  levels, admin edit-page picker added. Migration `20260601161846_ProgrammeDefaultEntrustmentScale`;
  +7 tests (Application 258→265). Live-verified; Paediatrics seeded to scale 2 in `act4-complete-t076`.
- **F-4B-1 (doc):** scenario imagines a tabbed evidence bundle with a T032 sampling-concentration
  warning + dashboard snapshot; the implemented `ReviewDetail` is single-column with neither (T032
  not exercisable here). Also: no review-type field; no NavMenu link to `/committee/reviews` for
  InstAdmin (URL-only).

**⚠️ Tooling note:** do NOT run `db-snapshot.ps1 take` concurrently with live browser requests — the
template-clone briefly drops the app's DB connections and 500s the in-flight request. Snapshot idle.
Also: the **Bash tool runs bash, not PowerShell** — start the dev server via the PowerShell tool
(`$env:ASPNETCORE_ENVIRONMENT='Development'; dotnet run …`) or it silently no-ops.

**No new secrets** — reused Mbatha `Mbatha@KGK2026!` and scenario shared `Act2Pass!123` (Zulu,
Mahlangu), already in `pwd_DO_NOT_COMMIT.txt`.

**Commit:** T075 (code + tests) + doc updates landed in **`46126bf`** on `master` (10 files,
+447/-7). Nothing pushed. Build clean, all non-Integration suites green.

**Post-Act-4 findings follow-up (2026-06-01, Opus) — ALL THREE handled:**
- **F-4A-2 RESOLVED (A1, docs-only, `b01920f`)** — committee ratify/appeals stay with the chair.
- **F-4D-1 RESOLVED (T076, `3186a85`)** — programme default entrustment scale.
- **F-4B-1 mostly RECONCILED + nav fix (this commit):** re-examined `ReviewDetail` — the T032
  sampling-concentration warning **does** exist/render (conditional on `AnyWarning`; just didn't fire
  on the thin scenario data), and the single-column page **does** surface the evidence (frozen
  activity snapshot + trajectory + warning). The scenario over-imagined a tabbed bundle / dashboard
  snapshot — accepted the implemented design (no code). **Fixed (e):** added "Decision Panels" +
  "Committee Reviews" nav links for InstitutionalAdmin + Administrator (were missing; verified live).
  **Deferred (d):** review-type field (Annual vs Pre-graduation) — descriptive-only, nothing uses it.

**▶ Recommended next: Act 5** (graduation + STAR augmentation to all 15 EPAs + portfolio PDF export).
**Opus** (entrustment-decision augmentation + QuestPDF). Start Act 5 from
**`tools\db-snapshot.ps1 restore act4-complete-t076`** (= act4-complete + the T076 migration + Paed
default scale; the pure `act4-complete` is kept as the play-through record).

## ⭐ Session finalized — 2026-05-30 (Opus) — Act 3 played in full + 2 findings fixed

**Act 3 fully played (3.A–3.I, Sonnet), then its two real code findings fixed (Opus). On `master`,
nothing pushed.** Act 3 was rebuilt from `after-act-2-replay` (the older `act3-*` snapshots were
corrupted by another project sharing this Postgres instance). Schemas built via the visual builder:
mini_cex_paed v2, procedure_log_paed v2, dops_paed v2, msf_paed v2. Per-phase Actual/Gap lines are in
`scenario-paediatrics.md`; build details in `act3-rebuild-scratch.md`.

**Findings fixed this session:**
- **F-3G-1 FIXED (`361fc6b`, task `T074`):** Coordinator "Stalled requests" panel was dead — the query
  filtered `CurrentState == "requested"` (a state no workflow defines; the model is draft→submitted→
  rated→completed) and compared `CreatedOn` not `UpdatedOn`. Now `CurrentState == "submitted" &&
  UpdatedOn < stallCutoff`; `StalledRequestItem.RequestedOn`→`SubmittedOn`; +1 regression test. (Also
  fixed a pre-existing committed compile error in CreditApplierTests: `PublishedVersion`→`Version`.)
- **T065 SHIPPED (`2562c2a`):** assessor `TrainingStatus` enum (NotStarted/InTraining/Provisional/
  Trained) on `AssessorProfile`, alongside the existing `TrainingCompletedOn` date — closes **F-3E-1**
  (in-training now representable). Migration `20260530104328_AssessorTrainingStatusEnum` (dotnet-ef
  scaffolded) + backfill: recorded date→Trained(3), else NotStarted(0); verified applied. Edit-page
  status picker (date gated to Provisional/Trained) + list column; full browser round-trip verified.
  NOTE: models + surfaces status (flagging); **hard-blocking** an in-training assessor from completing
  is a separate product decision, deliberately NOT done.

**Tests:** Domain 45, Application 252, Architecture 19, Web 42 — all green. Integration not run (Docker).

**DB snapshots:** latest is **`act3R-final-t065`** (Act 3 complete + T065 migration applied & backfilled;
Patel=InTraining demo, Botha/Naidoo/Zulu=Trained, Khumalo=NotStarted). Restore with
`tools\db-snapshot.ps1 restore act3R-final-t065`. Older `act3R-*` predate the T065 migration (restoring
one re-applies it on app start). Earlier act3R milestones: `act3R-minicex-published`, `-A-C`, `-D`,
`-E`, `-F`, `-final`.

**Findings still open (deferred, documented):**
- **F-3E-2 (not a code bug):** PAED-010 has no stage-1 key in `MinimumLevelByStageJson` → credit engine
  correctly falls back to the flat min (4); a year-1 level-2 DOPS counts volume but not reached. This is
  curriculum **seed-config**, not a defect. Fold into T066 if a stage-1 minimum for PAED-010 is wanted.
- **F-3F-NOTE (doc):** the MSF credit rule built used `curriculum_item_id: 13` = PAED-012 (not PAED-013).
  Just a play-through choice; revisit when seeding the real MSF credit rule.

**⚠️ Env note:** during this session an external process intermittently corrupted files under `Rewrite/`
and stale `act3-*` DB snapshots (same other-project tooling). If a doc looks mangled, check `git`.

**▶ Recommended next: Act 4** (annual review + STARs + appeal). Opus.
Start from `tools\db-snapshot.ps1 restore act3R-final-t065`.

## ⭐ Session finalized — 2026-05-30 (Opus)

**Shipped this session (all merged to `master`, nothing pushed):**
- **T072** (`b045fee`) — `/portfolio/progress` now leads with curriculum credit; trajectory parser
  handles schema-driven `*_paed` keys + `overall_level`.
- **T069** (`0a88cc8`) — runtime `ActivityForm` EPA/User/Scale pickers + builder Scale-binding picker.
- Merged the `fix/T067-…` branch to master (`edf9bba`); container-engine note (`7ef04bc`).
- **ActivityForm reload fix** (`cd0ca07`) — load reference options once per schema (not per edit);
  fixes a DbContext-concurrency crash on rapid form edits. Found by the clean replay.
- **InstitutionalAdmin EPA-preview fix** (`63a5605`) — builder EPA picker now populated for InstAdmins.
- **T073** (`4b5cad8`) — credit engine gates on the **stage-aware** minimum, not the flat target.
  Found heading into 3.D; `TraineeProfile.GetStage` is the new single source of truth.

**Play-through this session (from `after-act-2-replay`):** Act 3 **3.A–3.D** all driven clean post-fix
and DB+UI verified. Mini-CEX credit `PAED-001 2/30 reached 1/30`; procedure-log credit
`PAED-011 5/30 reached 3/30` (stage-2 gating). Builder validated end-to-end three times.

**Tests:** Domain 45, Application 250, Architecture 19, Web 42, Infrastructure 8 — all green.
**Integration NOT run** (Testcontainers needs a Docker-API engine; none on this Windows box — install
Rancher Desktop / Podman / Docker Desktop to run it, or run in CI).

**DB snapshots added:** `act3-replay-verified`, `act3-D-verified` (latest; PAED-001 + PAED-011 credited,
Mini-CEX v2 + procedure_log_paed v2 published). `tools\db-snapshot.ps1 restore act3-D-verified`.

**No new secrets created** (reused `Mbatha@KGK2026!` / `Act2Pass!123`, already in pwd_DO_NOT_COMMIT.txt).

### ⚠️ 2026-05-30 (later, Opus): act3-* snapshots CORRUPTED — Act 3 being rebuilt from clean Act-2 state
**Do NOT restore `act3-D-verified` / any `act3-*` snapshot** — they were polluted by **another
project** sharing the Postgres instance (`ActivityTypes` blown up to 429 cols incl. duplicate
column names + a stray `ActivityTypes_BackupPre` table, main table 0 rows; the app can't query it).
User confirmed the corruption is foreign and chose: **restore `after-act-2-replay` (verified clean)
and rebuild ALL of Act 3 (3.A–3.I) via the visual builder.** That rebuild is IN PROGRESS.

**▶ Resume from `Rewrite/act3-rebuild-scratch.md` → "⏯ RESUME HERE".** It has the clean-DB facts
(real schema column names, cast UserIds, stages, EPA/curriculum stage-minimums, scale id 2), the
canonical workflow/credit JSON (the exact format the parser accepts — easy to get wrong), and a
per-phase checklist. Current point: mini_cex_paed FORM schema built (snapshot `act3R-minicex-built`),
but its Workflow/Credit JSON was pasted in the WRONG format and must be re-pasted + republished
before any lifecycle credits. **Model: Sonnet for the play-through grind; escalate to Opus for
findings / domain calls / handoff.**

Dev server: `$env:ASPNETCORE_ENVIRONMENT='Development'; dotnet run --project src/Wombat.Web/Wombat.Web.csproj`
(NOT `--no-launch-profile`). DB `wombat_t002_verify` user `wombat` pw `3Uca!yptus#12`; psql at
`C:\Program Files\PostgreSQL\16\bin\psql.exe`. Keep psql calls SOLO (a failing call cancels its batch).

Smaller alt tasks if preferred: **T070** (assessor rating-edit/note in Rated state — Sonnet) or the
trajectory schema-awareness cleanup (Sonnet).
Open follow-up still: **T070** (MEDIUM). Builder note: set a field's **label before its key** and verify
`ActivityTypes.StagingSchemaJson` before publishing (a `@bind` commit can drop a key when a Type
`<select>` change immediately follows the key fill).

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
