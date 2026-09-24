---
id: T181
title: The sign-in page logs a JSON parse error on every load
status: done
priority: P3
owner: agent
depends_on: []
created: 2026-09-24
completed: 2026-09-25
---

# T181 — The sign-in page logs a JSON parse error on every load

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

- [x] The sign-in page loads with a clean console. Browser.

## Related

T097 (CSP), T175 (static assets).

## Cause found, 2026-09-24 (the T175 browser check)

On `/account/login` and `/account/forgot-password`, `GET /_blazor/initializers` (a fetch by `blazor.web.js`, not an
asset) is answered with a **302 to `/account/login?ReturnUrl=%2F_blazor%2Finitializers`**. The fallback authorization
policy covers that endpoint, so the script parses the login page's HTML as JSON. T097 recorded it as "Filed, NOT fixed"
(its § 7), but no open task carried it until this one. The fix is to exempt the Blazor framework endpoints from the
fallback policy (as `MapWombatStaticAssets()` does for assets, T175), or to map them `AllowAnonymous`. Check that no
circuit endpoint becomes reachable unauthenticated beyond what Blazor needs.

---

## As built — 2026-09-25 (`705bf2c`)

The Blazor initializers endpoint answers signed out (`[]`, 200 JSON), so the sign-in pages no longer parse the login
page as JSON. The circuit (`/_blazor/negotiate`) still refuses a signed-out visitor, and opens for a signed-in one
(tested). DESIGN.md: a self-posting anonymous page must also carry `[ExcludeFromInteractiveRouting]`.

Browser/SQL on dev (scripted Chrome, `150417e`):  `/account/login` signed out logs no error, and initializers answers 200 `[]` with no redirect. Negotiate answers
401 signed out, and after sign-in the WebSocket connects with no console error.
