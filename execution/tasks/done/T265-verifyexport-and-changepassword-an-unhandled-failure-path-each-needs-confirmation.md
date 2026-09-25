---
id: T265
title: Changing a password crashes the page (confirmed), and VerifyExport has an unhandled failure path
status: done
priority: P2
owner: agent
depends_on: []
created: 2026-09-25
completed: 2026-09-25
---

# T265 — Changing a password crashes the page (confirmed), and VerifyExport has an unhandled failure path

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low, and it needs confirmation first.
**Surfaced:** 2026-09-25, the T234 review. Both predate it.

## Symptom (inferred)

- **VerifyExport:** `VerifyAsync` catches nothing, so a failed check is an unhandled exception. The page is
  `[AllowAnonymous]`, and a signed-out visitor's Verify button does nothing; only a `?hash=` link works.
- **ChangePassword:** `SignInManager.RefreshSignInAsync` runs inside the circuit with no catch.

## What to build

Reproduce each. Then catch and show a refusal, and make the anonymous verify work signed out, or say that it needs the
link.

## Verification

- [x] Each failure path shows a message, not the error page. bUnit or integration test.

## Related

T234, T023, T010.

## Confirmed, 2026-09-25 (the H1 browser check); raised to P2

As trainee, open `/account/change-password`, enter the current password and a new one, and press "Change password".
The password **is** changed. But the button stays on "Saving..." and Blazor's "An unhandled error has occurred.
Reload" appears, and "Password updated." never shows. The log reads:
`InvalidOperationException: Headers are read-only, response has already started`, from
`SignInManager.RefreshSignInCoreAsync` / `CookieAuthenticationHandler.HandleSignInAsync`, called at
`ChangePassword.razor:107` inside the interactive circuit.

A cookie cannot be reissued from a circuit. Change the password in the circuit, then refresh the sign-in through a
real HTTP request: post a form to an endpoint, as the login page does (T181's `[ExcludeFromInteractiveRouting]`
pattern), or redirect to an endpoint that refreshes and returns. Verify in the browser that the page says "Password
updated." and the user stays signed in.

---

## As built — 2026-09-25 (`88bb05c`)

Changing a password posts to an HTTP endpoint (the page is `[ExcludeFromInteractiveRouting]`). The endpoint:
- validates the session's security stamp first;
- checks the current password with lockout;
- is throttled like sign-in, and refuses an SSO-only account;
- changes the password and reissues the cookie over HTTP.

Faults and a post-expiry return land on the page. VerifyExport catches its failures and works signed out.
WebApplicationFactory flow tests (14) and bUnit.

Browser on dev (scripted Chrome, master `3f08b26`; `pg_dump -n public` first, at `recovery/pre-t251-migration.dump`):
- **assessor changed the password and changed it back:** "Password updated.", focused, with no "Headers are read-only".
  The new password signed in and the old one was refused. The refusals read as designed, and `?error=Call%20012` showed
  only the generic sentence.
- **collegeadmin's lockout:** the fifth wrong password locked the account, and the session stayed signed in.
- **committee2** locked by instadmin mid-session was sent to sign in, with the hash unchanged.
- **An expired session's** return was a GET to the page, not a 405.
- **VerifyExport:** a random hash gave "No matching export found", a real one "Export verified", and a blank press asked
  for a hash, all as a static page.

assessor's password is the dev constant again.

**Filed from the review:** [T286] (audit rows, external logins, unique emails) and [T285] (P2: free text on the sign-in
page).
