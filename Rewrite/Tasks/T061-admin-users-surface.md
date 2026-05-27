# T061 — Admin Users surface (replaces `/placeholder/users`)

Wombat has no admin Users page. The placeholder route `/placeholder/users` renders the "Planned surface / Coming soon / T011 or later" stub. `/admin/users` returns HTTP 404. This blocks four common ops tasks:

- **Add a role to an existing user.** Multi-role onboarding via two invitations is broken — the second invitation registration rejects with "A user with this email address already exists." Finding A2-3 / A2-4 in the 2026-05-27 Act 2 play-through.
- **Reset a user's password.** Finding A2-pwd. Mbatha's password from a prior session was unrecoverable; the dev-only `--dev-reset-password` CLI flag was added as a stop-gap.
- **Revoke stale invitations.** After a same-email duplicate-invitation attempt, the second invitation sits forever in the Active invitations table because no auto-revoke happens when registration completes (Finding A2-2).
- **Deactivate / re-activate a user.** Departure handling — no surface today.

This task replaces the placeholder with a real Users admin page. It is **the single most valuable Act-2-derived task** because closing it removes the need for both dev-CLI workarounds added on 2026-05-27 and unblocks future scenario replays.

## Surface

| Route | Purpose | Auth |
|---|---|---|
| `/admin/users` | List users (scope-aware: Administrator sees all, InstitutionalAdmin sees own institution + global) with name / email / roles / last-login / status | `AdministratorOrInstitutionalAdmin` |
| `/admin/users/{userId:guid}` | Detail + edit: roles add/remove, password reset, deactivate/reactivate, pending invitation cleanup | `AdministratorOrInstitutionalAdmin` |
| _(remove)_ `/placeholder/users` | Delete the placeholder page after the real one ships | n/a |

The NavMenu `Users` link should point at `/admin/users` (currently `/placeholder/users`).

## Application layer

Add or extend handlers in `Wombat.Application/Features/Users/`:

- `ListUsersQuery(Principal)` returning `UserSummaryDto`: filter to own institution if caller is InstitutionalAdmin; emit users with their roles, last-login, status. Scope guard via `principal.IsAdministrator()` / `principal.GetInstitutionId()` / `principal.CanAccessInstitution(...)`. Use `IUserAdministrationService.ListUsersInRoleAsync` plus a new method or compose queries.
- `GetUserByIdQuery(userId, Principal)` returning `UserDetailDto` with roles + scopes + active invitations for the same email. Return null (=> 404) when out-of-scope, NOT 403 (matches T056 pattern in `Institutions` / `Specialities` to avoid leaking other-institution id existence).
- `AddRoleToUserCommand(userId, role, Principal)` and `RemoveRoleFromUserCommand(userId, role, Principal)`. Reject if caller cannot administer the target (`Administrator` cannot be added or removed via this surface — explicit rule per CLAUDE.md "Administrator role cannot be assigned via SSO" — extend that rule to "cannot be assigned via the Users page either; must be DB-direct"). Same for the `PendingTrainee` role which is system-managed.
- `ResetUserPasswordCommand(userId, newPassword, Principal)` — Identity-backed token + reset. Audit-log the event.
- `DeactivateUserCommand(userId, Principal)` — sets `IsActive=false` on `WombatIdentityUser` if such a column exists; else use Identity's lockout: `LockoutEnd = DateTimeOffset.MaxValue`. Check what's available first.
- `RevokeAllPendingInvitationsForEmailCommand(email, Principal)` — cleanup for the stale-secondary-invitation papercut. Also invoke from the `RegisterUserCommand` happy path so stale invitations auto-revoke after registration completes.

All commands write audit entries (`UserRoleAdded`, `UserPasswordReset`, `UserDeactivated`, etc.) — pattern from `T040` / `T045`.

## Web layer

`src/Wombat.Web/Components/Pages/Admin/Users/`:

- `UsersList.razor` (`/admin/users`) — DataTable per the existing admin-list pattern (`AssessorsList.razor` is the closest analogue). Search box for email / name. Status pill (Active / Locked-out).
- `UserDetail.razor` (`/admin/users/{userId:guid}`) — sections:
  - Profile summary (immutable email + name + last login).
  - Roles list with `[x] remove` per row + "Add role" picker (excludes Administrator and PendingTrainee).
  - Speciality / sub-speciality scope rows (read-only display; future task could add edit, but out of scope here).
  - Password reset form (single field + confirm; admin sets directly, no email loop).
  - Pending invitations panel — lists any active invitations for the user's email; "Revoke" button per row.
  - Deactivate / reactivate button.
- Both pages use `IScopedSender` per CLAUDE.md.

Delete `src/Wombat.Web/Components/Pages/Placeholders/PlaceholderUsers.razor` (if that's where the stub lives — check `/placeholder/users` first). Update `NavMenu.razor` to point at `/admin/users`.

## Replaces dev-CLI stop-gaps

Once shipped:
- Remove `--dev-reset-password` from `Wombat.Web/Program.cs`. Keep `--dev-add-role` only if it remains useful for headless seed scripts; otherwise drop it too.
- Update `pwd_DO_NOT_COMMIT.txt` note about session-set passwords to point at the new Users page.

## Tests

- New `Features/Users` test class with scope-guard tests (Administrator vs InstitutionalAdmin, cross-institution rejection) following the T056 cluster pattern.
- Test that adding Administrator or PendingTrainee role is rejected.
- Test that password reset produces a working hash (sign in flow assertion via Identity).
- Test that registering a user revokes all pending same-email invitations.
- bUnit smoke test for `UsersList.razor` (1–2 rows render + filter works).

## Browser verification

As Administrator: visit `/admin/users`, see all users including Demo Institution's seeded users. Open Mbatha's detail, add Coordinator role, verify her dashboard shows the merged surface on next login. Reset Patel's password to a known value, sign in as Patel with that password. Deactivate Ndlovu, verify he cannot log in.

As Mbatha (InstitutionalAdmin): repeat the above; verify Demo Institution users are NOT visible.

## Out of scope

- SSO group mapping changes (T027 territory).
- User profile self-edit beyond the existing `/account/profile`.
- Cascading speciality / sub-speciality scope editing — flag as a future task once the list page surfaces the gap.
- Audit-log inspection page for a specific user — `/admin/audit` already exists; surface a "show events for this user" link from the detail page if cheap, defer otherwise.

## Definition of done

- Build clean, all suites pass.
- New tests cover scope guards + the four commands (add role / remove role / reset password / revoke invitations).
- bUnit smoke test passes.
- Browser-verified the three scenarios above.
- `--dev-reset-password` removed from `Program.cs`; `--dev-add-role` removed or kept per call.
- `pwd_DO_NOT_COMMIT.txt` updated.
- Scenario doc Act 2 findings A2-2 / A2-3 / A2-4 / A2-pwd marked closed.

## Files touched (estimate)

- `src/Wombat.Application/Features/Users/*` — ~6 new handler/command/query files.
- `src/Wombat.Application/Common/Interfaces/IUserAdministrationService.cs` + impl — possibly extend.
- `src/Wombat.Web/Components/Pages/Admin/Users/UsersList.razor` (new).
- `src/Wombat.Web/Components/Pages/Admin/Users/UserDetail.razor` (new).
- `src/Wombat.Web/Components/Layout/NavMenu.razor` — link update.
- `src/Wombat.Web/Components/Pages/Placeholders/PlaceholderUsers.razor` — delete.
- `src/Wombat.Web/Program.cs` — remove `--dev-reset-password` (and maybe `--dev-add-role`).
- `tests/Wombat.Application.Tests/Features/Users/*` — new test class(es).
- `tests/Wombat.Web.Tests/*` — bUnit smoke.
- `pwd_DO_NOT_COMMIT.txt` — note update.
- `Rewrite/scenario-paediatrics.md` — Act 2 close-out lines.
- `Rewrite/Tasks/T061-admin-users-surface.md` (this file).
- `Rewrite/current_state.md`.

## Estimate

~4–6 hours. **Opus** — touches auth, audit, two new pages, ~5 commands, tests.
