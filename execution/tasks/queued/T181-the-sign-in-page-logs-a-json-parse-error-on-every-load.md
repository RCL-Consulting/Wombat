---
id: T181
title: The sign-in page logs a JSON parse error on every load
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-24
---

# <id> — <one line that states the defect or the goal, not the solution>

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. Nothing visibly breaks, but an error on every sign-in hides real ones, and something is fetching a
resource it does not get.
**Surfaced:** 2026-09-24, noticed by the T141 and T137 browser checks. Not investigated.

## Symptom

The console on `/account/login` logs `Unexpected token '<', "<!DOCTYPE "... is not valid JSON` once per load. A script
on the page fetches something it expects to be JSON and gets an HTML page back, probably a fallback route or a 404
rendered as HTML.

## What to build

Find the request in the network log (Playwright `browser_network_requests`), then fix its URL or stop the fetch. Likely
suspects are the auth layout, a manifest, or a Blazor boot resource under the CSP (T097).

## Verification

- [ ] The sign-in page loads with a clean console. Browser.

## Related

T097 (CSP), T175 (static assets).
