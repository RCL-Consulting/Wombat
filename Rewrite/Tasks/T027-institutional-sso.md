# T027 — Institutional SSO (OIDC)

**Phase:** 6 — Cross-cutting operations
**Depends on:** T005 (identity + roles), T006 (institution scope)
**Blocks:** T016

## Goal

Let users log in with their institutional identity (Microsoft Entra ID, Shibboleth via an OIDC bridge, Google Workspace for some private programmes) instead of — or alongside — a local Wombat password. On first login, provision a Wombat user record, map institutional group memberships to Wombat roles per a rule table, and land the user on their dashboard. Local password login remains available for non-SSO users and break-glass access.

## What to do

1. **Identity configuration**:
   - Add `Microsoft.AspNetCore.Authentication.OpenIdConnect` to `Wombat.Web`.
   - Add `AddOpenIdConnect` for each configured institution provider in `Program.cs`, reading provider config from `appsettings`:
     ```
     Sso:
       Providers:
         - Key: "uct"
           DisplayName: "University of Cape Town"
           InstitutionId: "..."
           Authority: "https://login.microsoftonline.com/<tenant>/v2.0"
           ClientId: "..."
           ClientSecret: "<from env>"
           Scopes: [ "openid", "profile", "email" ]
           GroupsClaim: "groups"
     ```
   - Providers are registered at startup from this config. Adding a new provider is a config edit + app restart (not a redeploy). Document in `INFRASTRUCTURE.md`.
2. **Login page** under `Wombat.Web/Components/Pages/Account/Login.razor`:
   - Shows local-password form.
   - Shows one "Sign in with <Institution>" button per configured provider.
   - A "challenge" action on the button triggers the standard ASP.NET Core OIDC challenge. The callback is handled by `OnTicketReceived` / `OnTokenValidated` events in the provider configuration.
3. **Claim handling** in `Wombat.Infrastructure/Identity/ExternalLoginHandler.cs`:
   - On successful OIDC callback, extract: `sub` (external id), `email`, `name`, `preferred_username`, groups claim.
   - Look up the Wombat user by `(ProviderKey, ExternalSubjectId)` in `AspNetUserLogins`.
   - If found: sign in, update profile from the claim (email, display name).
   - If not found: look up by email within the provider's `InstitutionId`. If found, **offer to link** (do not auto-link across an unverified email match — show a "is this you?" page where the user enters their local password or a provisioning token to confirm).
   - If still not found: provision a new user (see below).
4. **First-login provisioning**:
   - Create `AspNetUsers` row with `Email`, `UserName` = email, `EmailConfirmed = true` (the provider asserted it), no password.
   - Add the `AspNetUserLogins` row for the OIDC provider.
   - Assign the `InstitutionId` scope claim from the provider config.
   - Run the group-to-role mapper (see below).
   - If the mapper assigns no roles, create the user as `PendingTrainee` and email a Coordinator/Administrator to review.
5. **Group-to-role mapping**:
   - `SsoGroupRoleMapping` table — Id, ProviderKey, ExternalGroupName, WombatRole (from the role enum), InstitutionId, SpecialityId?. Seeded per institution at deployment.
   - On each login, compare the user's current groups against the mapping table. Add roles that match and are missing. Remove roles that were previously assigned by SSO and no longer match (do not remove roles that were assigned manually by an admin — track the `AssignedBy` source on `UserRole`).
   - Admins manage mappings in `Admin/Sso/GroupMappings.razor`. Changes take effect on the next login for each user.
6. **Break-glass local accounts**:
   - Retain a small number of local-password Administrator accounts (at least two) independent of SSO. Even if the IdP is down, the system must be administrable.
   - `AllowLocalPassword` flag on `AspNetUsers` — default `false` for SSO-provisioned users. A user with `AllowLocalPassword = false` cannot set a password, and the local login form refuses their username.
7. **Logout**:
   - Local logout only, by default. RP-initiated OIDC logout (federating logout back to the IdP) is optional and controlled by a per-provider flag `EnableFederatedLogout`. Default off; some IdPs misbehave with it.
8. **Security considerations**:
   - `state` parameter enforced (stock ASP.NET Core OIDC does this).
   - `nonce` enforced.
   - Valid issuer list strictly from configured authorities.
   - Client secrets from environment variables, never in `appsettings.Production.json`.
   - Clock skew: 2 minutes tolerance.
   - Token replay: use the built-in ASP.NET Core nonce cache, backed by the data-protection key ring (shared if Wombat ever runs multi-node — for now, single node, default key ring).
9. **Audit** (T025): every SSO login, first-time provisioning, role mapping change, and role grant/revoke via SSO writes an audit entry in category `Authentication` or `Permission`.
10. **Testing**:
    - Integration test uses a stub OIDC provider (`OpenIddict` in-memory) to exercise the callback handlers without a real IdP.
    - Unit tests for the group-to-role mapper: overlapping groups, no groups, removal of a previously-SSO-assigned role, preservation of a manually-assigned role.

## Files created

- `src/Wombat.Web/Program.cs` (edits — OIDC registration loop)
- `src/Wombat.Web/Components/Pages/Account/Login.razor` (edits — provider buttons)
- `src/Wombat.Web/Components/Pages/Account/LinkExternalLogin.razor`
- `src/Wombat.Infrastructure/Identity/ExternalLoginHandler.cs`
- `src/Wombat.Infrastructure/Identity/SsoGroupMapper.cs`
- `src/Wombat.Domain/Identity/SsoGroupRoleMapping.cs`
- `src/Wombat.Infrastructure/Persistence/Configurations/Identity/SsoGroupRoleMappingConfiguration.cs`
- `src/Wombat.Infrastructure/Persistence/Migrations/*Sso.cs`
- `src/Wombat.Web/Components/Pages/Admin/Sso/GroupMappings.razor`
- `tests/Wombat.Infrastructure.Tests/Identity/SsoGroupMapperTests.cs`
- `tests/Wombat.Web.Tests/Sso/ExternalLoginFlowTests.cs`

## Verification

- [ ] `dotnet build` clean.
- [ ] `dotnet test` — SSO mapper tests and external-login flow tests green.
- [ ] Manual with a stub IdP (OpenIddict in a console app, or a test Azure AD tenant):
  - First login provisions a new user.
  - Assigned roles match the group mappings.
  - Re-login honours group changes (add a group → new role on next login; remove a group → role removed next login, but manually-assigned roles remain).
  - A user with no matching groups lands in `PendingTrainee` and triggers a review email to coordinators.
  - Local break-glass login still works when the IdP is misconfigured (simulate by setting an invalid authority — SSO button errors, local login unaffected).
- [ ] Audit log shows authentication and permission entries for every step.

## Notes & gotchas

- Do not use `AddMicrosoftIdentityWebApp` from `Microsoft.Identity.Web`. It is heavier than needed and adds a Microsoft-specific abstraction over what is already plain OIDC. Stock `AddOpenIdConnect` works for Entra, Google, and Shibboleth bridges alike.
- Shibboleth is SAML at the wire level. To integrate, the institution points an OIDC bridge (Keycloak, Auth0, or a dedicated SAML-to-OIDC adapter) at their Shibboleth IdP, and Wombat talks to the bridge over OIDC. Do not implement SAML directly.
- Groups claim format varies by provider. Entra emits object IDs by default, not display names; map by object ID and keep display names in the mapping table for admin UX. Document per-provider quirks in `CUSTOMIZATION.md`.
- **Never** create a user from an SSO login with Administrator role from the group mapping alone. Administrator assignment requires an explicit manual step by an existing Administrator. Document this invariant and enforce in `SsoGroupMapper.Apply` — if the mapping table says "this group → Administrator", log a warning and skip. Administrator cannot be assigned by SSO.
- Institution scope is inferred from the configured provider's `InstitutionId`. A user cannot belong to two institutions via SSO; if cross-institution login is needed, the user gets two separate accounts. Document.
- Do not sync avatars from the IdP. It seems nice, it is a parade of edge cases (size, format, licensing). Let users upload their own.
- Time-limit the provisioning grace: if `PendingTrainee` lingers for 14 days without a Coordinator completing setup, nudge the user and the Coordinator (via T024).
- "Just in time" vs "just once" provisioning: Wombat does just-in-time (create on first login), not just-once (pre-create from a nightly SCIM feed). SCIM is a separate future task if an institution asks for it — list it as a follow-up but not in this round.
