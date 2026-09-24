---
id: T175
title: app.css is served without a version in its URL, so a deploy can show a new page with an old stylesheet
status: queued
priority: P2
owner: agent
depends_on: []
created: 2026-09-24
---

# <id> — <one line that states the defect or the goal, not the solution>

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium: a deploy risk. Every CSS change ships broken to anyone holding a cached copy until their cache
expires, and T157 will ship many CSS changes at once.
**Surfaced:** 2026-09-24, T125's browser check. On first load the browser used a cached `app.css` without the new
`.stage-minima` rule. The per-year editor rendered with selects about 1000px wide until a forced re-fetch.

## Symptom

`src/Wombat.Web/Components/App.razor:17` links `<link rel="stylesheet" href="/app.css" />`, a bare path. The response
carries an ETag and Last-Modified but no Cache-Control, so browsers cache it heuristically (often for hours to days,
based on the Last-Modified age). `Program.cs` already calls `MapStaticAssets()` (.NET 9+), which fingerprints assets
referenced through `@Assets["…"]`, but `app.css` is not referenced that way.

## What to build

Reference the stylesheet through `@Assets["app.css"]` (and any other first-party CSS or JS linked by a bare path; grep
`App.razor` and the layouts), so the URL changes whenever the content does and the asset gets a long immutable cache.
Check that the CSP (nonce-backed `script-src`, T097) and the `MapStaticAssets().AllowAnonymous()` comment at
`Program.cs:156-158` still hold. Confirm in the browser that a changed `app.css` is fetched on the next load without a
forced refresh.

## Verification

- [ ] The rendered `<link>` for app.css carries a content hash. bUnit or a response check.
- [ ] After a CSS edit and a restart, a normal reload picks up the new rule. Browser.
- [ ] The CSP check in `deploy/verify/` still passes.

## Related

T125 (where it bit), T097 (CSP), T157 (the deploy that would ship it).
