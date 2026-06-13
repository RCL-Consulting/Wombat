# T091 — EPAs/curricula are nationally owned (CMSA), institutions adopt them

**Status:** IN PROGRESS — Phases 1–4 DONE & committed; Phases 5–6 remain.

**Progress:**
- ✅ **P1** (`1c18bda`) — College entity + CollegeAdmin role/claim/policy/scope helpers (additive).
- ✅ **P2** — re-parented `Speciality` (InstitutionId→CollegeId); added direct `TraineeProfile.InstitutionId`;
  re-scoped ~35 Application handlers (national catalogue → `CanAccessCollege`; trainees/forms/panels/
  activity-types → direct institution); Infra seeders/PDF services/reference-data fixed; Web admin pages
  compile (College display + adoption-aware filtering deferred to P5, marked with T091 comments); migration
  `T091_ReparentSpecialityToCollege` (fresh-DB, column rename + new FKs); scope-guard tests rewritten to
  college-scoping; `CollegeAdmin` test principal added. All unit suites green (Domain 50, Application 284,
  Architecture 19, Web 43, Infrastructure 10). Provisional/Phase-4-deferred bits flagged inline:
  InstitutionalAdmin sees the whole national EPA/curriculum catalogue (no adoption narrowing yet);
  speciality-scoped DecisionPanels now carry their own InstitutionId.
- ✅ **P3** (`03fad97` additive + `2714f11` logic) — local extras: nullable `OwningInstitutionId` on `Epa`
  + `CurriculumItem`; partial unique indexes; national-vs-local authorization.
- ✅ **P4** (`c592556`) — adoption + versioning. New `InstitutionCurriculumAdoption` (Institution adopts a
  national `Curriculum` version per discipline; partial unique index = one active per institution+sub-speciality;
  all FKs Restrict). `TraineeProfile.AdoptionId` pins the trainee to the adopted version.
  `Curriculum.CloneAsNewVersion` now clones only national-core items (skips local extras) + preserves stage JSON.
  `AdoptCurriculum` (adopt/re-adopt — re-adoption supersedes the active record) + `ListAdoptions`
  (Administrator any/all; InstitutionalAdmin own-only). `AdmitTrainee`/`UpdateTraineeProfile` resolve & **require**
  the institution's active adoption for the chosen version (hard gate via `TraineeAdoptionResolver`).
  `CreditApplier` scopes every curriculum-item match to the trainee's adopted curriculum version + own-institution
  local extras (no cross-version/cross-institution leak; nothing without an active profile). `GetCurricula`/`GetEpas`
  narrowed for InstitutionalAdmin to adopted-only (also fixes the previously-empty admit dropdown). Migration
  `T091_CurriculumAdoption` (fresh-DB). +9 tests. Unit suites green: Domain 50, Application 298, Architecture 19,
  Web 43, Infrastructure 10.
- ⏳ **P5** Web surfaces (College admin + adoption pages + College display), **P6** tests + scenario rebuild — still to do.
**Surfaced:** 2026-06-10. User confirmed the current ownership model is a **mistake**:
in South Africa, EPAs and the discipline curriculum are defined by the **Colleges of
Medicine of South Africa (CMSA)** (e.g. the College of Paediatricians → FCPaed), and
**training institutions must adopt** them — institutions do not author their own EPAs.

## Current (incorrect) model

EPAs are owned by an institution, transitively:

```
Epa.SubSpecialityId → SubSpeciality.SpecialityId → Speciality.InstitutionId → Institution
```

(`Wombat.Domain/Epas/Epa.cs`, `Institutions/SubSpeciality.cs`, `Institutions/Speciality.cs`.)
`Curriculum` and `CurriculumItem` hang off the same institution-scoped `SubSpeciality`.

**Consequences of the mistake:**
- Each institution must re-enter the entire national EPA set + curriculum by hand
  (duplication, drift, no single source of truth).
- No cross-institution comparability — KGK's "PAED-001" and another site's "PAED-001"
  are unrelated rows.
- A national curriculum revision must be replicated into every institution separately.
- Contradicts the SA regulatory reality (CMSA owns the standard; HPCSA accredits sites).

## DECIDED design (2026-06-10, user — domain expert)

Four forks resolved:
1. **College layer** — introduce `College` (a CMSA constituent, e.g. College of Paediatricians)
   that owns the national discipline + its EPAs/curricula. Institution becomes a training site
   that adopts.
2. **National core + local extras** — institutions adopt the national set verbatim and MAY add
   institution-local supplementary EPAs / curriculum items, but can NEVER edit the national core.
   ⇒ EPA and CurriculumItem need an owner discriminator (`OwningInstitutionId`, null = national).
3. **Per-College admin role** — new `CollegeAdmin` role scoped to one College (mirrors
   InstitutionalAdmin/InstitutionId): manages that College's EPAs/curricula.
4. **Explicit, version-pinned adoption** — an institution adopts a curriculum *version* via an
   adoption record; national publishes new versions institutions opt into; trainees pin to the
   adopted version.

### Target ownership chains

```
National catalogue:  College → Speciality → SubSpeciality → { Epa (national), Curriculum (national, versioned) → CurriculumItem (national) }
Institution-local:   Institution → InstitutionCurriculumAdoption → (pins a national Curriculum version)
                     Institution → Epa (OwningInstitutionId set)            ← local extra
                     Curriculum → CurriculumItem (OwningInstitutionId set)  ← local extra on a national curriculum
Trainee:             TraineeProfile.CurriculumId (national version) [+ AdoptionId]
```

### Schema changes

- **NEW `College`**: Id, Name, ShortCode, Description?, IsActive, CreatedOn. Owns Specialities.
- **`Speciality`**: replace `InstitutionId` FK → **`CollegeId`** FK (the re-parent). Unique
  index (CollegeId, Name). *This is the breaking change* — Speciality is now national.
- **`SubSpeciality`**: unchanged parent (SpecialityId); now transitively College-owned.
- **`Epa`**: add nullable **`OwningInstitutionId`** (null = national/College, set = local extra).
  Unique index → (SubSpecialityId, OwningInstitutionId, Code).
- **`Curriculum`**: stays national (via SubSpeciality); existing Version + CloneAsNewVersion reused.
- **`CurriculumItem`**: add nullable **`OwningInstitutionId`** (null = national core, set = local
  addition applied only for that institution's adopters).
- **NEW `InstitutionCurriculumAdoption`**: Id, InstitutionId, CurriculumId (pinned national
  version), AdoptedOn, IsActive. Unique active per (InstitutionId, SubSpeciality-of-curriculum).
- **`TraineeProfile`**: keep `CurriculumId` (national version); optionally add `AdoptionId`.
- **`AssessorProfile`**: keeps its own `InstitutionId` (unaffected); its optional Speciality/
  SubSpeciality now point at national rows — fine.
- **`EntrustmentScale`**: already global — leave as-is (could later move under College; out of scope).

### Auth

- `WombatRoles.CollegeAdmin`; `WombatClaims.CollegeId`.
- `ClaimsPrincipalExtensions`: `GetCollegeId()`, `IsCollegeAdmin()`, `CanAccessCollege(int)`.
- Policy `AdministratorOrCollegeAdmin` for national-catalogue pages.
- National EPA/curriculum authoring flips from `AdministratorOrInstitutionalAdmin` →
  `AdministratorOrCollegeAdmin`. InstitutionalAdmin retains authoring **only** for local extras
  (OwningInstitutionId = own institution) + adoption management. Administrator cannot be CollegeAdmin
  via SSO (same rule as Administrator today).

## Phased plan (each phase builds green; commit per phase)

- **P1 — Foundation (additive, safe):** `College` entity + EF config + DbSet + migration;
  `CollegeAdmin` role + `CollegeId` claim + extension methods + `AdministratorOrCollegeAdmin`
  policy. Nothing consumes them yet. ← STARTED.
- **P2 — Re-parent Speciality (breaking):** Speciality `InstitutionId`→`CollegeId`; migration with
  data backfill (create a College per existing institution-speciality owner, or a single CMSA
  College); fix every consumer of `Speciality.InstitutionId` (scope guards, handlers, AssessorProfile
  display, DecisionPanel). Rework `EpaScopeGuardTests`/`CurriculumScopeGuardTests` to college scope.
- **P3 — Local-extra discriminators:** `OwningInstitutionId` on Epa + CurriculumItem; uniqueness;
  list queries union national + own-institution-local; guards (national needs CollegeAdmin, local
  needs matching InstitutionalAdmin).
- **P4 — Adoption + versioning:** ✅ DONE (`c592556`). `InstitutionCurriculumAdoption` entity/config/migration;
  adopt / re-adopt flows; trainee linkage (`TraineeProfile.AdoptionId`, hard gate at admission); CreditApplier
  respects adoption + local items; InstitutionalAdmin catalogue views narrowed to adopted-only.
- **P5 — Web surfaces:** College admin (Administrator); national EPA/curriculum authoring
  (CollegeAdmin); institution adoption + local-extras pages (InstitutionalAdmin); nav updates.
- **P6 — Tests + scenario rebuild:** update/extend Application + architecture + Web tests; rebuild
  `scenario-paediatrics.md` Act 1 setup on a fresh DB (national CMSA Paediatrics catalogue + KGK
  adoption); re-bank snapshots. Old `act*-v2-*` snapshots become invalid for the new schema.

**Run on a fresh DB.** `dotnet ef` 10.0.3 works here against the live dev DB (needs the
user-secrets connection string) — prefer scaffolded migrations, hand-write + Designer only if EF CLI
can't run. **Opus** throughout (domain + migration correctness).
