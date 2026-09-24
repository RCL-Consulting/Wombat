---
id: T155
title: SSO sign-in rewrites an account's email from an unverified claim, and can re-key it onto another person's address
status: queued
priority: P2
owner: agent
depends_on: []
created: 2026-09-24
---

# T155 — SSO sign-in rewrites an account's email from an unverified claim, and can re-key it onto another person's address


> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium, possibly High depending on the identity provider. Found by the T149 security review
(2026-09-24); not introduced by T149.
**Surfaced:** 2026-09-24, T149 adversarial review, section (b)1. Observed in code, not exercised.

## Symptom

`ExternalLoginHandler.SignInExistingUserAsync` (the profile-sync block) copies `Email` and `UserName` from the
provider's email claim on every SSO sign-in. It never checks `email_verified`, and it ignores the result of
`UserManager.UpdateAsync`. If the provider lets a user change their email claim:

- **The new address belongs to user V:** Identity's validation refuses the update (duplicate user name), but the
  modified entity stays tracked. `SsoGroupMapper.ApplyAsync` then calls `SaveChangesAsync`, which commits Email and
  UserName set to V's address, with stale normalised columns. A's mail goes to V, and A appears under V's address in
  admin lists and pickers. The next `UpdateAsync` on A throws on the `NormalizedUserName` unique index.
- **The address is unclaimed:** the change succeeds and squats it, and a later invitation to that address is refused,
  because `InvitedUserProvisioner` refuses an existing email. First-login provisioning squats the same way
  (`EmailConfirmed = true` on an unverified claim).

## What to build

Stop syncing the email on sign-in, or sync it only when the provider asserts `email_verified`, through
`SetEmailAsync`/`SetUserNameAsync` with the result checked, before any other save. Decide whether first-login
provisioning should require `email_verified` too.

## Verification

- [ ] An SSO sign-in whose email claim is another user's address changes nothing on either account — test.
- [ ] An unverified email claim is never written — test.

## Related

T149, T027 (SSO), T102 (nominees are found by the stored identity).
