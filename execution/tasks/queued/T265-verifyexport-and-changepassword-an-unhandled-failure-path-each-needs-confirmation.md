---
id: T265
title: VerifyExport and ChangePassword: an unhandled failure path each (needs confirmation)
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
---

# T265 — VerifyExport and ChangePassword: an unhandled failure path each (needs confirmation)

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
