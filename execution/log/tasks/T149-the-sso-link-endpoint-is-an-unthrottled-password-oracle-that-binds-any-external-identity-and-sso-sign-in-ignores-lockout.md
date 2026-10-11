---
id: T149
title: The SSO link endpoint is an unthrottled password oracle that binds any external identity, and SSO sign-in ignores lockout
status: done
priority: P1
owner: agent
depends_on: []
created: 2026-09-24
started: 2026-09-24
completed: 2026-09-24
---

# T149 — The SSO link endpoint is an unthrottled password oracle that binds any external identity, and SSO sign-in ignores lockout

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

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

- [x] **The link binds the cookie's login, never the form's.** The form no longer carries a provider, subject or email
  (`LinkExternalRequest` is password and return URL); `ALinkWithTheRightPassword_BindsTheExternalLoginFromTheCookie_…`
  and `ALinkTargetsTheAccountTheProvidersEmailNames_SoAnotherAccountCannotBeNamed` (`SsoLinkAndSignInTests`).
- [x] **Five bad link passwords lock the account; a locked account cannot link or SSO-sign-in.**
  `FiveWrongPasswords_LockTheAccount_AndALockedAccountCannotLinkEvenWithTheRightOne`,
  `AnAdministratorsLock_IsHonouredBySsoSignIn`; mutation-checked (`lockoutOnFailure: false` and the removed sign-in
  refusal each fail one test).
- [x] **The endpoint is rate-limited.** Dev, curl: ten tokenless posts got 400 (antiforgery), the eleventh a 302 to
  `/account/login?error=Too many sign-in attempts…` — the limiter runs before antiforgery, so every attempt counts.
- [x] **An erased user has no external logins.** `SsoErasurePostgresTests` (real PostgreSQL: the executor runs raw
  SQL); mutation-checked.
- [x] Also: every link failure but the lockout reads the same; an already-linked subject cannot be re-linked; an
  account of another institution cannot sign in through this provider (mutation-checked).
- [x] From the review: a brute-force lockout does **not** block SSO sign-in (only a deactivation does); an SSO-only
  account cannot be linked and the attempt counts nothing against it; an Administrator can neither link nor sign in
  through SSO; a deactivated account is not offered a link; moving a user to another institution drops their external
  logins. 14 tests in `SsoLinkAndSignInTests`, each new condition mutation-checked.

## Related

T102 (the nominee rule's trust boundary), T097 (login throttling), T027 (SSO), T061 (lock-out).

---

## As built — 2026-09-24

- **Link** (`ExternalLoginHandler.LinkAndSignInAsync`, `POST /account/link-external/submit`): the provider, subject and
  email come from the external cookie; the form carries only the password and the return URL. The account must be the
  provider's asserted email inside the provider's institution, not already linked, not SSO-only and not an
  Administrator. The password is checked with `CheckPasswordSignInAsync(lockoutOnFailure: true)`; every failure but the
  lockout reads the same. The endpoint uses the login rate limit. The link page shows the email from the external
  cookie, not the query string, and the URL no longer carries the provider, subject or email.
- **Sign-in** (`SignInExistingUserAsync`): refuses, before touching the account, a **deactivated** account
  (`UserDeactivation`: an admin's lock or an erasure — deliberately not a brute-force lockout, which protects a
  password SSO never checks and would let anyone lock an SSO user out every fifteen minutes), an Administrator, and an
  account outside the provider's institution. The callback does not offer a link to a deactivated account, and clears
  the external cookie on every final outcome.
- **Erasure** removes every external login; **`UpdateScopeAsync`** drops them when a user moves institution, so the
  institution refusal does not strand a moved user.
- **Not done here** (pre-existing, filed): [T155] SSO rewrites an account's email from an unverified claim;
  [T156] the /24 throttle, enumeration, the sliding external cookie, non-transactional erasure.

**Review:** one adversarial security review (read-only): no major defect. Its minor findings are the second half of
the list above; its pre-existing findings are T155 and T156.

