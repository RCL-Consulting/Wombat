---
id: T286
title: Account leftovers: a password change writes no audit row, institutional sign-in links cannot be listed or removed, and emails are unique only by check
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
---

# T286 — Account leftovers: a password change writes no audit row, institutional sign-in links cannot be listed or removed, and emails are unique only by check

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low.
**Surfaced:** 2026-09-25, the T265 and T155 reviews.

## Symptom and what to build

- **Audit.** A self-service password change writes no audit row, for success, refusal or lockout. Sign-in writes
  `Login`, `LoginFailed` and `LoginLockedOut`; write the same for the password change.
- **External logins.** No page lists them, and only an institution move or erasure removes one. List them to the account
  holder and an admin, allow removal, and decide whether a password reset drops them.
- **Unique emails.** `RequireUniqueEmail` is false and the `NormalizedEmail` index is not unique, so only the handler's
  check keeps emails unique. Add a unique partial index on `NormalizedEmail` (`WHERE "NormalizedEmail" IS NOT NULL`,
  because erasure nulls it) in a migration. Do not use `RequireUniqueEmail`: its validator would refuse erasure's null
  update.
- **Profile.** It shows "Change password" to accounts with `AllowLocalPassword == false`. Hide it.

## Verification

- [ ] Each, with tests. The index refuses a concurrent duplicate (Postgres test).

## Related

T265, T155, T258.

Notes, 2026-09-25 (the T285 review):
- **A link race answers 500.** In `ExternalLoginHandler.LinkAndSignInAsync`, two link requests that both pass
  `AddLoginAsync`'s internal check break the `AspNetUserLogins` key, and nothing catches the `DbUpdateException`. Catch
  it as `ProvisionNewUserAsync` does, discard, reload, and return `AlreadyLinked`.
- **A failed role leaves an account.** `InvitedUserProvisioner.ProvisionAsync` throws a bare exception if
  `AddToRoleAsync` fails after `CreateAsync` has saved the account, so the account stays without a role and the
  invitation stays unused.
- **Issuing never checks the address.** `IssueInvitation` should refuse an address that already has an account or that
  Identity will not accept, through T285's `GetAddressStatusAsync`.

Notes, 2026-09-25 (the mail chain's reviews):
- **Identity descriptions in exceptions.** Identity's error descriptions quote the address and are put into exception
  text that reaches audit rows and logs: `UserAdministrationService` (two places) and `InvitedUserProvisioner` (two
  places). `DevUserSeeder` does the same (dev only). Map them to fixed messages.
- **The audit display name.** `HttpAuditContextProvider` falls back to `ClaimTypes.Name`, which is the address.
- **A throwing logger** on the mail worker's retry or outcome path stops the worker.
- **`RevokeInvitation`'s check.** Through the command, a CollegeAdmin can revoke their College's CollegeAdmin
  invitation, although the comment says Administrator-only (T093). The page does not reach it. Apply T283's
  `IsAdministrator()` rule.
