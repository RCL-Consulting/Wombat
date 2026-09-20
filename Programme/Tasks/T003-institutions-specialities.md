# T003 — Institutions, Specialities, SubSpecialities

**Phase:** 0 — Ground truth
**Depends on:** T002
**Blocks:** T004, T005, T011
**Status:** Implemented on 2026-04-11 in commit `0caee6a` (follow-up manual auth walkthrough still pending)

## Goal

Model the organisational reference hierarchy: an Institution has many Specialities; a Speciality has many SubSpecialities. CRUD handlers for each, plus admin-only pages to maintain them. This is the skeleton everything else hangs off.

## What to do

1. In `Wombat.Domain/Institutions/`:
   - `Institution` entity — Id, Name, ShortCode, ContactEmail, IsActive, CreatedOn (UTC).
   - `Speciality` entity — Id, InstitutionId, Name, Description, IsActive.
   - `SubSpeciality` entity — Id, SpecialityId, Name, Description, IsActive.
2. EF configurations for each under `Wombat.Infrastructure/Persistence/Configurations/`. Keys, indexes (unique on `(InstitutionId, Name)` for Speciality, `(SpecialityId, Name)` for SubSpeciality), cascade rules (deleting a Speciality cascades to SubSpecialities; deleting an Institution with Specialities is restricted).
3. CQRS handlers in `Wombat.Application/Features/Institutions/`:
   - Queries: `GetInstitutionsList`, `GetInstitutionById`, `GetSpecialitiesForInstitution`, `GetSubSpecialitiesForSpeciality`
   - Commands: `CreateInstitution`, `UpdateInstitution`, `DeactivateInstitution`, `CreateSpeciality`, `UpdateSpeciality`, `DeactivateSpeciality`, `CreateSubSpeciality`, `UpdateSubSpeciality`, `DeactivateSubSpeciality`
4. FluentValidation validators for each command. Name must be non-empty, length bounds, uniqueness check at the DB level (catch the unique constraint violation and return a domain error).
5. Blazor pages under `Wombat.Web/Components/Pages/Admin/Institutions/`:
   - `InstitutionsList.razor`
   - `InstitutionEdit.razor` (create + edit on the same page)
   - `SpecialitiesList.razor` (scoped to an institution)
   - `SpecialityEdit.razor`
   - `SubSpecialitiesList.razor`
   - `SubSpecialityEdit.razor`
6. Gate all admin pages with `[Authorize(Policy = "Administrator")]` or the institutional-admin-scoped policies from T002.
7. EF migration: `InstitutionsInitial`. Include `Designer.cs`. Apply to local DB.
8. Seed one demo institution, one demo speciality, one demo sub-speciality in `DataSeeder` — guarded by `if (!_db.Institutions.Any())` so it runs once.

## Files created

- `src/Wombat.Domain/Institutions/{Institution,Speciality,SubSpeciality}.cs`
- `src/Wombat.Infrastructure/Persistence/Configurations/{Institution,Speciality,SubSpeciality}Configuration.cs`
- `src/Wombat.Infrastructure/Persistence/Migrations/*InstitutionsInitial.cs`
- `src/Wombat.Application/Features/Institutions/**` (handlers, DTOs, validators)
- `src/Wombat.Web/Components/Pages/Admin/Institutions/**`

## Verification

- [x] `dotnet build` clean.
- [x] `dotnet test` — new unit tests cover the validators and a sample handler using an in-memory context.
- [ ] Manual: log in as the seeded admin, create a second institution, create a speciality under it, create a sub-speciality. Reload and confirm they persist.
- [ ] Deleting an institution with children is refused with a clear error.

## Outcome

- Domain entities, EF configuration, CQRS handlers, validators, migration, startup seeding, and admin maintenance pages were added.
- `InstitutionsInitial` was applied successfully to the local verification database `wombat_t002_verify`.
- The web host still starts successfully after the migration.
- Remaining gap: authenticated manual browser verification is deferred until the login/bootstrap path is exercised again.

## Notes & gotchas

- "Deactivate" not "delete". Deactivation preserves referential integrity. Only the Administrator role can hard-delete, and only via a separate command that refuses if there are dependent assessments.
- The cascade rule for Speciality → SubSpeciality cascade-deletes in the EF model but is only exercised when an Institution is deleted outright. In normal operation everything is deactivated.
- Queries return flat DTOs; do not return entities from handlers.
