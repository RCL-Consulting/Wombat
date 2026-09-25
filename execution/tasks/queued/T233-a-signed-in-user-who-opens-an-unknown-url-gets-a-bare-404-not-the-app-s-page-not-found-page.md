---
id: T233
title: A signed-in user who opens an unknown URL gets a bare 404, not the app's "Page not found" page
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
---

# T233 — A signed-in user who opens an unknown URL gets a bare 404, not the app's "Page not found" page

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. The browser shows its own error page, with no way back into the app but the back button.
**Surfaced:** 2026-09-25, the G1 browser check (T145's retired `/admin/forms`). App-wide, and older than T145.

## Symptom

For a signed-in user, `/admin/forms`, `/no-such-page` or any unknown URL answers HTTP 404 with an empty body, so Chrome
shows "This localhost page can't be found". The app's own page appears only at `/not-found`. A signed-out visitor is
redirected to the login page instead.

## Cause (inferred)

`Routes.razor` sets `NotFoundPage`, but `Wombat.Web/Program.cs` never calls `UseStatusCodePages*`. Only `Wombat.Api`
does, since T202.

## What to build

Render the app's not-found page for a **404 only**, for a request that accepts HTML: for example
`UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true)`, limited to 404. Leave a 401 on
`/_blazor/negotiate` (T181), a 400 or 405, a static file's 404 and any response that already has a body alone. The MSF
respondent page's own 404 ("Feedback link not recognised", T163) must keep its text.

## Verification

- [ ] Signed in, `/no-such-page` shows the app's "Page not found" page with status 404. Browser, and an integration
      test with WebApplicationFactory.
- [ ] Negotiate still answers 401 signed out, and a tampered MSF link still reads "Feedback link not recognised".

## Related

T145, T181, T163, T202.
