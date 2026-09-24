---
id: T180
title: The browser tab title does not change when the user moves between pages inside the app
status: done
priority: P3
owner: agent
depends_on: []
created: 2026-09-24
completed: 2026-09-24
---

# T180 — The browser tab title does not change when the user moves between pages inside the app

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. Browser history, tabs and screen readers announce the wrong page name after any in-app
navigation.
**Surfaced:** 2026-09-24, independently by three browser checks (T127, T125, T141).

## Symptom

After clicking from the dashboard to My Progress, `document.title` stays "Dashboard — Wombat". It changes only on a
full load. Every page declares `<PageTitle>`, so the defect is in the host, not the pages.

## Root cause (inferred)

`App.razor` renders `<HeadOutlet />` statically while `<Routes @rendermode="InteractiveServer" />` is interactive. An
interactive page's `PageTitle` cannot reach a static `HeadOutlet`. The usual fix is
`<HeadOutlet @rendermode="InteractiveServer" />`. Check it against the CSP nonce handling (T097) and prerendering.

## Verification

- [x] Clicking between three pages updates the tab title each time. Browser.
- [x] A full load still renders the right title (prerender). Browser.

## Related

T097 (CSP), T127 (first noticed).

---

## As built — 2026-09-24 (with [T175])

`<HeadOutlet>` renders interactively, so `<PageTitle>` reaches it after in-app navigation. Browser on dev, as the
trainee, with no page reloads: Dashboard, then My Activities, My progress, New Activity, and back to Dashboard. Each
title followed. A full reload and the prerendered HTML both carry the page's title. The console shows no CSP violation.
Titles follow no common style ("— Wombat", "- Wombat" or no suffix; mixed capitals); filed separately.
