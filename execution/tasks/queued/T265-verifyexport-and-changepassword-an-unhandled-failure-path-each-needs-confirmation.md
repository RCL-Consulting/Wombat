---
id: T265
title: Changing a password crashes the page (confirmed), and VerifyExport has an unhandled failure path
status: queued
priority: P2
owner: agent
depends_on: []
created: 2026-09-25
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

- [ ] Each failure path shows a message, not the error page. bUnit or integration test.

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
