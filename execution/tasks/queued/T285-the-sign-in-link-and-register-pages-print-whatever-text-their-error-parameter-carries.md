---
id: T285
title: The sign-in, link and register pages print whatever text their ?error= parameter carries
status: queued
priority: P2
owner: agent
depends_on: []
created: 2026-09-25
---

# T285 — The sign-in, link and register pages print whatever text their ?error= parameter carries

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium. A crafted link can put any words, for example a phone number or instructions, on Wombat's own
sign-in page. The text is HTML-encoded, so it is text injection, not script.
**Surfaced:** 2026-09-25, the T265 and T155 reviews. It predates both.

## Symptom

- `Login.razor` and `LinkExternalLogin.razor` show the `error` query text as it arrives.
- The endpoints build those URLs with free text: `BuildLoginUrl` and `BuildRegisterUrl`.
- The register endpoint's catch puts `exception.Message` into the URL.

T265 already made `/account/change-password` map codes to fixed sentences. `?error=Call%20012` there shows only "The
password could not be changed".

## What to build

Redirect with error **codes** only, mapped to fixed sentences on each page (T265's `ChangePasswordOutcome` pattern).
Any unknown code gets one generic sentence. Log the exception, and never put its message in a URL.

## Verification

- [ ] `/account/login?error=Call%20012` shows only a generic sentence, and each real refusal still reads right.
      Integration tests (WebApplicationFactory) and bUnit.

## Related

T265, T155, T156, T181.
