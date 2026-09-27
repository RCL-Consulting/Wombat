---
id: T190
title: Page titles follow no common pattern
status: done
priority: P3
owner: agent
depends_on: []
created: 2026-09-24
completed: 2026-09-27
---

# T190 — Page titles follow no common pattern

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. Tabs and history read inconsistently.
**Surfaced:** 2026-09-24, the T180 browser check.

## Symptom

"Dashboard — Wombat", "Sign in - Wombat", and most pages with no suffix. Capitals are mixed ("My Activities", "My
progress"), and every activity page is titled just "Activity".

## What to build

One rule in DESIGN.md (e.g. "<page> — Wombat", sentence case), and a `PageTitle` wrapper or a test that walks every
`@page` component. An activity page's title names the activity type.

## Verification

- [x] Every routable page's title matches the rule. bUnit or reflection test. — `b347e11c`: `Hosting/PageTitleTests` finds every page by reflection and reads its source. It holds each title to "<Stem> · Wombat", sentence case, and stem = heading. It failed on 64 pages before the change.

## As built in T335 (`b347e11c`, 2026-09-27)

- D10 applied: every routable page's tab is "<Stem> · Wombat", the stem in sentence case and the same words as its
  heading. The rule is in DESIGN.md § Page-level patterns.

## Related

T180.

## Notes

- **T295 replay, 2026-09-26 (C16).** Note, 2026-09-26 (the T295 replay, Step 1.26): on `/admin/activity-types/new`, `document.title` is empty. `ActivityTypeEdit.razor:14` is `<PageTitle>@(_editor?.Name ?? "Activity Type")</PageTitle>`, and `GetActivityTypeEditorQuery.cs:67-69` returns a new type's Name as `string.Empty`, so the `??` applies only while the page loads. The title also comes from the stored name, not from what the header says (`PageTitleText`, `:434`: "New activity type" or "Edit {name}"), and it is in Title Case. Under this task's rule, the title should follow the header. The every-page title test should fail on an empty title after the load, not only on the rendered markup before it, or this page passes it. Step 1.26's Expect ("the browser tab reading 'Activity Type'") needs updating to the rule when it lands.
