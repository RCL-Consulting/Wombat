# T090 — InstitutionalAdmin must not see or manage unscoped (global) users

**Status:** DONE (code + tests; uncommitted)
**Surfaced:** 2026-06-10, during T089 logo review — user logged in as Mbatha
(InstitutionalAdmin, KGK) and noticed `admin@wombat.local` (global Administrator) listed
and "Manage"-able at `/admin/users`.

## Problem

The admin Users surface (T061) deliberately showed unscoped (no-institution) accounts —
including global Administrators — to a tenant-level `InstitutionalAdmin`. The original
`ListUsersQuery` comment justified this as "visible but cannot be mutated from this
surface." Two issues:

1. **Wrong default (information disclosure).** A tenant-level admin could enumerate the
   platform's privileged operator accounts. The conservative multi-tenant default is
   need-to-know: an InstitutionalAdmin's world is their own institution.
2. **Incomplete enforcement (latent write gap).** The mutation guards keyed on the
   *Administrator role* (`user.Roles.Contains(Administrator) && !IsAdministrator()`) plus
   an institution-scope check that only fired when `InstitutionId.HasValue`. An **unscoped
   account that did NOT hold the Administrator role** (none exist today, but a future
   "global Coordinator" would) slipped *both* guards and was mutable by any
   InstitutionalAdmin. The detail page (`GetUserByIdQuery`) had the same hole — its scope
   guard only fired for scoped users, so a global account was viewable by GUID.

## Invariant established

> An `InstitutionalAdmin`'s authority is confined to users scoped to their own
> institution. Unscoped (global) accounts are visible and mutable only by a global
> `Administrator`.

Applied uniformly at every read and write path, using the same shape:
`!Principal.IsAdministrator() && (!user.InstitutionId.HasValue || !Principal.CanAccessInstitution(user.InstitutionId.Value))`.

## Changes

- `ListUsersQuery` — filter to own-institution only (drop the `!HasValue ||` branch).
- `GetUserByIdQuery` — return null (404) for unscoped users unless caller is Administrator.
- `AddRoleToUser`, `RemoveRoleFromUser`, `ResetUserPassword`, `SetUserLockout` — scope
  guard now also rejects unscoped targets for non-Administrator callers. (The existing
  Administrator-role guard stays as the specific, friendlier message for that case.)

## Tests (Wombat.Application.Tests, all green — 284 total)

- `ListUsers_AsInstitutionalAdmin_OnlyReturnsOwnInstitution` (was `...AndGlobal`, flipped).
- `ListUsers_AsAdministrator_ReturnsAllIncludingUnscoped` (new — Administrator still sees all).
- `GetUserById_UnscopedGlobalUser_AsInstitutionalAdmin_ReturnsNull` (new).
- `GetUserById_UnscopedGlobalUser_AsAdministrator_IsVisible` (new).
- `ResetPassword_UnscopedNonAdministrator_AsInstitutionalAdmin_IsRejected` (new — closes the latent write gap).

## Notes

- No DB / migration changes; pure authorization logic.
- Web detail page (`/admin/users/{guid}`) needs no change — it already 404s on a null
  `GetUserByIdQuery` result.
