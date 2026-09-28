---
id: T341
title: Account outcomes ride in the address, so a crafted link can show a fixed notice or reopen the Remove dialog
status: queued
priority: P2
owner: agent
depends_on: []
created: 2026-09-28
---

# T341 — Account outcomes ride in the address, so a crafted link can show a fixed notice or reopen the Remove dialog

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low to medium. Only fixed words can be shown, never an attacker's text, but one link opens a password
prompt. P2: before real users.
**Surfaced:** 2026-09-28, the security review of T339's integration branch (finding 5).

## Symptom

Every account outcome is a code in the query string, turned into fixed words by the page:
- `/account/profile?status=sign-in-removed&provider=<linked key>` says "<X> sign-in removed." while the sign-in is still
  listed; `?status=password-updated` says "Password updated.".
- `/account/login?error=LockedSignedOut` tells a signed-out visitor their account is locked for 15 minutes.
- `/account/profile?error=RemoveWrongPassword&provider=<linked key>` opens the Remove dialog as the page loads, reading
  "Incorrect password", the password focused. A "confirm it is you" link can get a person to type their password and
  press Remove.

## Root cause

The endpoints redirect with the outcome in the address (a pattern since T265), and the pages trust it.

## What to build

Carry one-time outcomes in a short-lived, data-protected cookie, as `ActingRoleSwitchResults` already does, read once
and deleted; the address carries nothing the page acts on. At the least, never auto-open a password prompt from the
address alone. Covers sign-in notices, change password, My account and Remove.

## Verification

- [ ] Each crafted address above shows the plain page — tests.
- [ ] Each real outcome still shows once, and not on reload — tests.

## Related

T339 (flow 02; `design/flows/02-sign-in-and-account/`), T265, T285, `ActingRoleSwitchResults`.

## Notes

- Observed in review, not exploited. The words are fixed, so no text injection (T285 holds).
