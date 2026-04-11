# T010 — Web layout, navigation, and auth

**Phase:** 3 — Web
**Depends on:** T002 (can start in parallel with the Phase 2 tasks)
**Blocks:** T011

## Goal

Deliver the Blazor chrome: layout, navigation, auth pages (login, logout, change password), role-gated routing, and a shared component library. No feature pages — those come in T011.

## What to do

1. Decide the icon strategy (see Open Questions in `current_state.md`). Recommendation: copy a small set of inline SVGs from Lucide into `Wombat.Web/wwwroot/icons/` and reference them via a `<Icon Name="check" />` component.
2. Build `MainLayout.razor` with a sidebar (role-aware) and a top bar (user menu, sign-out).
3. `NavMenu.razor` — renders navigation items based on the current user's roles. Each `NavItem` declares which roles can see it. Copy the pattern from ClinicAssist.
4. Auth pages under `Wombat.Web/Components/Pages/Account/`:
   - `Login.razor` — POST to Identity sign-in manager, redirect on success.
   - `Logout.razor`
   - `ChangePassword.razor`
   - `Register.razor` already exists from T005.
5. Error pages: `Error.razor`, `AccessDenied.razor`, `NotFound.razor`. Style them consistently.
6. Shared components under `Wombat.Web/Components/Shared/`:
   - `Icon.razor`
   - `ConfirmDialog.razor`
   - `PageHeader.razor`
   - `Breadcrumbs.razor`
   - `DataTable.razor` — generic list rendering with sort + filter. Borrow from ClinicAssist.
   - `FormField.razor` — a label + input + validation wrapper.
7. Configure `Program.cs` in `Wombat.Web`:
   - Add Blazor Interactive Server.
   - Add Identity + cookies.
   - Register `IScopedSender`.
   - Register `ApplicationDbContext` with Npgsql.
   - Configure authentication middleware order correctly (this is a common footgun).
   - Register authorization policies from T002.
   - Configure `FallbackPolicy` to require an authenticated user — everything is protected by default, pages opt out of auth explicitly.
8. CSS strategy: one top-level `app.css` plus component-scoped `.razor.css` files where a component has non-trivial styles. Do not pull in a UI framework (no MudBlazor, no Radzen). Same as ClinicAssist.
9. Smoke page: `Components/Pages/Home.razor` — a stub that says "Welcome, {User.Name}. Your role is {Role}." Good enough to verify auth works end-to-end.

## Files created

- `src/Wombat.Web/wwwroot/icons/*.svg` (lucide subset)
- `src/Wombat.Web/wwwroot/app.css`
- `src/Wombat.Web/Components/Layout/MainLayout.razor` + `.razor.css`
- `src/Wombat.Web/Components/Layout/NavMenu.razor` + `.razor.css`
- `src/Wombat.Web/Components/Shared/{Icon,ConfirmDialog,PageHeader,Breadcrumbs,DataTable,FormField}.razor`
- `src/Wombat.Web/Components/Pages/Account/{Login,Logout,ChangePassword}.razor`
- `src/Wombat.Web/Components/Pages/{Error,AccessDenied,NotFound,Home}.razor`

## Verification

- [ ] `dotnet build` clean.
- [ ] Manual: unauthenticated user is redirected to `/Account/Login`. Seeded admin logs in, lands on `/`, sees their name and role.
- [ ] Attempting to visit an admin page as a Trainee redirects to `/AccessDenied`.
- [ ] Sign-out clears the cookie and redirects to `/Account/Login`.
- [ ] `NavMenu` does not render admin items for non-admin users.

## Notes & gotchas

- Inject `IScopedSender`, not `ISender`, in every Blazor component. `ISender` captures the top-level scope and causes DbContext lifecycle bugs on subsequent calls within the same circuit. This is the single most common mistake in ClinicAssist's history; do not repeat it.
- Blazor's authentication state provider needs to be wired correctly; copy ClinicAssist's `Program.cs` pattern for `AddAuthenticationStateProvider` and friends.
- Do not use the default Identity UI scaffolding. It does not play nicely with Blazor Interactive Server and it brings in a lot of stuff you don't need. Hand-roll the auth pages.
- The `FallbackPolicy` = require-auth rule means every new page is protected by default. To mark a page anonymous, use `[AllowAnonymous]` on the `@attribute` line.
- `flush_interval -1` in Caddy (see INFRASTRUCTURE.md) is required for Blazor Server. Add a note to T015 to remember.
