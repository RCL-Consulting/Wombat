# Current state

This file is the live handoff between sessions. Every session ends by editing this file. Keep it short and accurate.

## Active task

**T013 — Architecture tests** (next task on the critical path after T027) — **Model: Sonnet**

T027 is now implemented. Next session: start `Tasks/T013-architecture-tests.md`.

## Critical-path reminder (post-pivot)

The plan has been restructured around a **schema-driven Activity platform** so institutions can add new activity types without code. The old per-type tasks (T007 Assessment, T008 Workflow, T009 STAR) are **superseded** — read their banners. The new critical path after the core domain is:

> T001 → T002 → T003 → T004 → T005 → T006 → **T017 → T018 → T019 → T020** → T021 → T022 → T010 → ~~T011~~ → ~~T012~~ → ~~T023~~ → ~~T024~~ → ~~T025~~ → ~~T026~~ → ~~T027~~ → T013 → T014 → T015 → T016

See `PLAN.md` for the full phase/dependency graph and `CUSTOMIZATION.md` for the no-code model.

## Last session notes

T027 completed:
- Domain layer:
  - `SsoGroupRoleMapping` entity (ProviderKey, ExternalGroupId, ExternalGroupDisplayName, WombatRole, InstitutionId, SpecialityId?, SubSpecialityId?)
  - `UserRoleAssignment` entity (tracks SSO vs manual role assignment source per user+role)
  - `RoleAssignmentSource` enum (Manual, Sso)
- Application layer:
  - `SsoOptions` + `SsoProviderOptions` config model (Sso:Providers section in appsettings)
  - `CreateSsoGroupMappingCommand` (blocks Administrator role mapping)
  - `DeleteSsoGroupMappingCommand`
  - `ListSsoGroupMappingsQuery` (filterable by provider, joins institution/speciality names)
  - `SsoGroupMappingDto`
- Infrastructure:
  - `SsoGroupMapper` — syncs Wombat roles from IdP group claims on each SSO login. Adds roles for matching groups, removes SSO-assigned roles that no longer match, never touches manually-assigned roles. Enforces Administrator guard (logs warning, skips). Also syncs speciality/sub-speciality scopes from matched mappings.
  - `ExternalLoginHandler` — full OIDC callback handler: lookup by external login → lookup by email (offer linking) → provision new user. Provisions SSO users with AllowLocalPassword=false, EmailConfirmed=true, InstitutionId from provider config. Falls back to PendingTrainee if no group mappings match. Audit entries for SsoLogin, SsoFirstLogin, SsoAccountLinked.
  - `LinkAndSignInAsync` — account linking flow where user confirms ownership via local password
  - EF configurations for SsoGroupRoleMapping and UserRoleAssignment
  - Migration `20260414140355_Sso` (SsoGroupRoleMappings table, UserRoleAssignments table, AllowLocalPassword column on AspNetUsers with backfill to true for existing users)
  - DbSets added to ApplicationDbContext
  - All services registered in Infrastructure DI
  - ErasureExecutor updated to remove UserRoleAssignment records during erasure
- Web (Blazor):
  - `Program.cs`: config-driven OIDC provider registration loop (AddOpenIdConnect per provider), SSO challenge endpoint (`GET /account/sso-challenge/{providerKey}`), SSO callback endpoint (`GET /account/sso-callback`), account linking endpoint (`POST /account/link-external/submit`), AllowLocalPassword enforcement on local login
  - `Login.razor`: SSO provider buttons with "or" divider when providers are configured
  - `LinkExternalLogin.razor` at `/account/link-external` — "is this you?" page for linking existing account to SSO
  - `Admin/Sso/GroupMappings.razor` at `/admin/sso/group-mappings` — CRUD for group-to-role mappings with institution/speciality/sub-speciality scope
  - NavMenu: "SSO Mappings" link for Administrator
  - `app.css`: SSO divider and button styles
- Tests: 11 new tests (122 total Application, was 111)
  - 7 `SsoGroupMapperTests` (matching group assigns role, overlapping groups assign multiple, no matching groups, Administrator mapping skipped, removed from group removes SSO role, manually-assigned role preserved, different provider doesn't interfere)
  - 4 `SsoGroupMappingCommandTests` (create valid, create Administrator throws, delete existing, delete non-existent throws)
- NuGet: `Microsoft.AspNetCore.Authentication.OpenIdConnect 10.0.3` added to Web project
- Verification:
  - `dotnet build Wombat.sln -c Release` — clean (0 warnings, 0 errors)
  - `dotnet test Application.Tests` — 122 passed (was 111)
  - `dotnet test Web.Tests` — 33 passed
  - `dotnet test Domain.Tests` — 17 passed
- Verification caveats:
  - No real OIDC IdP tested — requires a stub OpenIddict server or a test Azure AD tenant
  - Login page SSO buttons not tested against running app
  - Admin group mappings page not tested against running app
  - Account linking flow not end-to-end tested
  - Federated logout not yet wired (RP-initiated OIDC logout is per-provider opt-in, documented but not connected)
