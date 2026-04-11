# T002 — Identity and role model

**Phase:** 0 — Ground truth
**Depends on:** T001
**Blocks:** T003, T005, T010

## Goal

Stand up ASP.NET Core Identity with the nine Wombat roles, claim-based scope claims, and the invitation-only registration flag. No UI yet — this task delivers the Identity plumbing and seeding only.

## What to do

1. In `Wombat.Domain`, add a `WombatUser` class that contains only the pure domain fields (no EF attributes, no Identity base class). It will be composed into the Identity user in Infrastructure.
2. In `Wombat.Infrastructure/Identity/`, add `WombatIdentityUser : IdentityUser` that holds Identity concerns plus a 1:1 reference to `WombatUser` or inlines the fields — mirror ClinicAssist's approach.
3. Add `ApplicationDbContext : IdentityDbContext<WombatIdentityUser>` in `Wombat.Infrastructure/Persistence/`. It implements `IApplicationDbContext` defined in `Wombat.Application`.
4. Register Identity in `Wombat.Web/Program.cs`:
   - `AddIdentity<WombatIdentityUser, IdentityRole>()`
   - `.AddEntityFrameworkStores<ApplicationDbContext>()`
   - `.AddDefaultTokenProviders()`
   - Disable self-registration (configure `SignInManager` options and/or remove the Register page from Identity scaffolding).
5. Seed the nine roles via a `RoleSeeder` class called from `Program.cs` at startup. Roles match `DOMAIN.md`:
   - `Administrator`
   - `InstitutionalAdmin`
   - `SpecialityAdmin`
   - `SubSpecialityAdmin`
   - `Coordinator`
   - `CommitteeMember`
   - `Assessor`
   - `Trainee`
   - `PendingTrainee`
6. Define authorization policies in `Wombat.Infrastructure/Identity/AuthorizationPolicies.cs`. One policy per role plus policies for scoped checks (e.g. `RequireSpecialityAdminForCurrentInstitution`). Use claims-based requirements.
7. Define the scope claims: `institution_id`, `speciality_id`, `sub_speciality_id`. Add a `ClaimsPrincipalExtensions` helper in Application with methods like `GetInstitutionId()`, `IsInSpeciality(int id)`, `IsInRole(string role)`.
8. Seed one `Administrator` user from configuration (`Wombat:SeedAdminEmail` and `Wombat:SeedAdminPassword`). If the user already exists, no-op. This is how the fresh-install bootstrap works.
9. Add an initial EF migration: `IdentityInitial`. Include the `Designer.cs` file.
10. Run `dotnet ef database update` against a local Postgres (or document how to run it) and confirm the schema is created.

## Files created

- `src/Wombat.Domain/Identity/WombatUser.cs`
- `src/Wombat.Application/Common/Interfaces/IApplicationDbContext.cs`
- `src/Wombat.Application/Common/Extensions/ClaimsPrincipalExtensions.cs`
- `src/Wombat.Infrastructure/Persistence/ApplicationDbContext.cs`
- `src/Wombat.Infrastructure/Persistence/Migrations/*IdentityInitial.cs` (+ Designer.cs)
- `src/Wombat.Infrastructure/Identity/WombatIdentityUser.cs`
- `src/Wombat.Infrastructure/Identity/RoleSeeder.cs`
- `src/Wombat.Infrastructure/Identity/AdminSeeder.cs`
- `src/Wombat.Infrastructure/Identity/AuthorizationPolicies.cs`
- `src/Wombat.Infrastructure/Identity/WombatClaims.cs` (constants for claim type names)

## Verification

- [ ] `dotnet build` clean.
- [ ] `dotnet ef database update --project src/Wombat.Infrastructure --startup-project src/Wombat.Web` succeeds.
- [ ] Running the app inserts all nine roles if they don't exist (check `AspNetRoles` table).
- [ ] Running the app creates the seeded Administrator user (check `AspNetUsers` + `AspNetUserRoles`).
- [ ] Unit test (in `Wombat.Application.Tests`): `ClaimsPrincipalExtensions.GetInstitutionId()` returns the expected value given a mocked principal.

## Notes & gotchas

- Seed the roles **before** attempting to create the admin user — ordering matters.
- Use the non-generic `IdentityRole` unless you have a reason not to. ClinicAssist does the simple thing here; copy that.
- Scope claims must be re-emitted on every login by a `IUserClaimsPrincipalFactory<WombatIdentityUser>` implementation; don't write them once at registration time. Match ClinicAssist's approach.
- Do not enable `RequireConfirmedAccount`. Registration is invitation-only, which is its own proof of identity. Flag this explicitly as a decision so it isn't re-litigated later.
