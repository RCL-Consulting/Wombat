# T099 — The CPSA paediatric catalogue is seeded but unreachable by every non-admin user

**Status:** open
**Surfaced:** 2026-09-17, verifying the T098 phase-2a handoff against the dev database.
**Severity:** High — blocks end-to-end verification of T070 and makes the whole v11.1 catalogue invisible
in the product. Cheap to fix.

## Symptom

T098 phases 1 and 2a are correct in code and in the database, but **no trainee or assessor can see any of
it.** The four CPSA WBA tools do not appear at `/activities/new`, and the 15 national EPAs do not appear in
the EPA picker, for any user other than a global `Administrator` or an `InstitutionalAdmin`.

## Evidence (dev DB, queried 2026-09-17)

```
SELECT DISTINCT "SpecialityId" FROM "UserSpecialityScopes";   ->  1, 2      (no row for 3)
SELECT count(*) FROM "UserSpecialityScopes" WHERE "SpecialityId" = 3;  ->  0

Id | Key                      | Scope          | ScopeId
17 | mini_cex_cpsa            | Speciality (2) | 3
18 | dops_cpsa                | Speciality (2) | 3
19 | cbd_cpsa                 | Speciality (2) | 3
20 | direct_observation_cpsa  | Speciality (2) | 3
```

Colleges: 1 = DEMO-C, 2 = FCPaed -> Speciality 2 -> SubSpeciality 2 "General Paediatrics",
**3 = CPSA -> Speciality 3 -> SubSpeciality 3 "Paediatrics"**. The new catalogue lives entirely under
Speciality/SubSpeciality **3**, and nobody is scoped there.

## Root cause

Two independent scope filters, both reading claims baked into the auth cookie:

1. `ListActivityTypesQuery.cs:36` —
   `activityType.Scope == ActivityScope.Speciality && specialityIds.Contains(activityType.ScopeId ?? 0)`.
   `specialityIds` comes from `principal.GetSpecialityIds()`, i.e. the `UserSpecialityScopes` rows
   materialised as claims at sign-in. With no row for Speciality 3, all four `_cpsa` types are filtered out.
2. `ActivityReferenceDataService.cs:88-93` — the EPA catalogue options are institution/speciality filtered
   for callers who are neither `Administrator` nor `InstitutionalAdmin`, so the 15 new EPAs are hidden from
   trainees and assessors by the same mechanism.

T098's own task file notes the scope requirement (`T098-epa-v11-adoption.md:384-386`); the session handoff
headline ("EPA v11.1 catalogue live in code") does not, which is how it was missed.

## Decision required

The catalogue sits under a **new** hierarchy (CPSA / Speciality 3), while all five existing trainees and
their assessors are scoped to the **FCPaed** hierarchy (Speciality 2). Two options:

1. **Scope the existing paediatric users to Speciality 3 as well** — additive, non-destructive, leaves the
   old FCPaed world intact. Users hold both scopes and see both tool sets until T104 retires the old one.
   *Recommended for unblocking T070 now.*
2. **Migrate them off Speciality 2 onto Speciality 3** — this is T104, a live-data migration with the
   5-rung/6-rung scale re-point hazard. Not a prerequisite for T070.

## Fix

Option 1: insert `UserSpecialityScopes` rows for `SpecialityId = 3` for the paediatric trainees and
assessors used in verification. **A fresh sign-in is required** — the filter reads cookie claims, not the
database, so an existing session keeps the old scope set.

Consider whether `PaediatricCatalogueSeeder` should also provision scope rows for a nominated verification
cohort on a dev database, so this is not re-discovered on the next fresh DB.

## Verification

Sign in as a paediatric trainee (fresh sign-in, not a resumed session) and confirm:
- `/activities/new` lists Mini-CEX (CPSA), CBD (CPSA), DOPS (CPSA) and Direct observation (CPSA).
- The EPA picker inside those forms lists the 15 `PAED-001..015` EPAs of the **CPSA** sub-speciality.
- The `overall_level` field renders the six-rung ladder, not a bare number box (a bare box means the
  `scale_key` did not resolve — `ActivityReferenceDataService.cs:151-160` matches on exact scale Name).

## Related

Prerequisite for verifying [T070]. Interacts with [T104] (retiring the old FCPaed paediatric world).

---

## Progress — 2026-09-17: dev database done, production outstanding

Option 1 (additive) applied to the dev database:

```sql
insert into "UserSpecialityScopes" ("UserId","SpecialityId")
select distinct "UserId", 3 from "UserSpecialityScopes" where "SpecialityId" = 2
on conflict ("UserId","SpecialityId") do nothing;   -- INSERT 0 10
```

All ten KGK paediatric users — 4 trainees (dlamini, duplessis, mahlangu, ndlovu), 5 assessors (botha,
khumalo, naidoo, patel, zulu) and molefe — now hold **both** Speciality 2 and Speciality 3. The legacy
FCPaed world is untouched, so nothing that worked before stops working.

Row counts after: Speciality 1 = 2 users, Speciality 2 = 10, Speciality 3 = 10.

**A fresh sign-in is required** before it takes effect — the filter reads claims baked into the auth
cookie, not the database.

### Still open

- **Production has the identical problem.** `PaediatricCatalogueSeeder` runs at startup there too, so the
  catalogue exists and no production user is scoped to the new speciality. Nothing has been changed on
  production. This needs to be an explicit deployment step, not a discovery.
- **A fresh database has it too.** Nothing provisions these rows; the next developer to drop and recreate
  the dev DB will hit the same invisible-catalogue symptom. Decide whether
  `PaediatricCatalogueSeeder` should scope a nominated verification cohort, or whether this belongs in the
  admin UI as an ordinary operator action (which is what it is, in production terms).
- **Noticed en route, unrelated:** `molefe@kgk.wombat.local` holds **no roles at all**, so that account
  can do nothing in the product. Pre-existing; worth a look when next in `/admin/users`.
