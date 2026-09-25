---
id: T287
title: Sign-in hardening, second pass: the lockout reply tells a locked account apart, the app cookies lack __Host-, and the API host has no forwarded headers
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
---

# T287 — Sign-in hardening, second pass: the lockout reply tells a locked account apart, the app cookies lack __Host-, and the API host has no forwarded headers

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low.
**Surfaced:** 2026-09-25, the T156 review.

## Symptom and what to build

- **The lockout reply.** "Too many failed sign-in attempts…" answers on the first try for an account an admin
  deactivated, and after 5 guesses for an active local account, but never for an unknown or SSO-only address. Decide
  whether a locked-out person is told. **Recommendation:** the same generic refusal for all, with the lockout explained
  by email to the account's own address.
- **Cookies.** The application and antiforgery cookies could take the `__Host-` prefix, as the external cookie now does.
- **Timing.** Refusing a real local account takes longer (its password is hashed) than refusing an unknown address.
  Hash a dummy password for unknown addresses.
- **An Identity description.** A failed `AddLoginAsync` in `LinkAndSignInAsync` still shows Identity's text. Only a race
  can cause it.
- **The API host** has no forwarded-headers setup, so behind Caddy its MSF limits count Caddy as the client.
- **VerifyExport's placeholder** says "Enter the hash from the PDF footer", but the footer prints no hash. Reword it, or
  print the hash.

## Verification

- [ ] Each item fixed has a test, or its decision is recorded.

## Related

T156, T149, T265.
