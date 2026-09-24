---
id: T149
title: The SSO link endpoint is an unthrottled password oracle that binds any external identity, and SSO sign-in ignores lockout
status: queued
priority: P1
owner: agent
depends_on: []
created: 2026-09-24
---

# T149 — The SSO link endpoint is an unthrottled password oracle that binds any external identity, and SSO sign-in ignores lockout

**Severity:** High. Account takeover by password guessing, around every control the local login has. Found while
designing T102, whose nominee rule trusts the stored role rows, `InstitutionId` and `LockoutEnd` — all three of
which this path can get past.
**Surfaced:** 2026-09-24, T102 design critique (escalation lens, upheld 3/3), confirmed by reading the code.

## Symptom

Observed in code, not yet exercised in a browser:

- `POST /account/link-external/submit` (`Program.cs:358-398`) is `AllowAnonymous` and has **no rate limiting**. The
  local login is `.RequireRateLimiting(LoginRateLimitPolicy)` (`Program.cs:254`).
- `ExternalLoginHandler.LinkAndSignInAsync` (`ExternalLoginHandler.cs:123-178`) calls `UserManager.CheckPasswordAsync`,
  which neither counts failures nor checks lockout, so the brute-force lockout never engages. It answers "User not
  found." and "Invalid password." differently, which enumerates accounts.
- It binds `new UserLoginInfo(providerKey, externalSubjectId, providerKey)` taken from the **form**, not from
  `loginInfo`, the external login actually in the cookie. Anyone holding any external session can therefore bind an
  arbitrary provider subject to the victim, then sign in as them through SSO from then on.
- No SSO sign-in path checks `LockoutEnd`, so an account an administrator locked (or erased) but which has a linked
  external login can still sign in. (Needs confirmation for the erased case: erasure removes roles and nulls the
  institution, but `ErasureExecutor` does not remove external logins.)

## Root cause

The link flow was written as a convenience for first SSO sign-in and never got the local login's defences.

## What to build

1. Bind the link to `loginInfo.LoginProvider` / `loginInfo.ProviderKey`; ignore the form's copies.
2. Use `SignInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true)` so failures count and a
   locked account is refused. One message for unknown user and bad password.
3. Rate-limit the endpoint with the login policy.
4. Refuse SSO sign-in for a locked-out account in every path (`SignInExistingUserAsync` and first login).
5. Remove external logins at erasure.
6. Consider refusing SSO role sync when the provider's institution differs from the user's.

## Verification

- [ ] A link attempt whose form provider/subject differ from the external cookie binds the cookie's — test.
- [ ] Five bad link passwords lock the account; a locked account cannot link or SSO-sign-in — tests.
- [ ] The endpoint is rate-limited — test or browser check.
- [ ] An erased user has no external logins — test.

## Related

T102 (the nominee rule's trust boundary), T097 (login throttling), T027 (SSO), T061 (lock-out).
