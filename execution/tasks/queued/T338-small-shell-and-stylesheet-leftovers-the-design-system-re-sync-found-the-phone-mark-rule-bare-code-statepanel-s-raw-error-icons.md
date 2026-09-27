---
id: T338
title: Small shell and stylesheet leftovers the design-system re-sync found: the phone mark rule, bare code, StatePanel's raw error, icons
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-27
---

# T338 — Small shell and stylesheet leftovers the design-system re-sync found

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. They are cosmetic and accessibility leftovers. None blocks a flow.
**Surfaced:** 2026-09-27, the re-sync of `design/system/` to flow 01 as built (`064cde00`). Two reviewers checked the
design system against the code.

## Symptom

1. The 28 px `.brand-mark` rule in `MainLayout.razor.css` (about lines 282–285) is not scoped to the sidebar. So the
   signed-out phone bar shows a 28 px mark, where DESIGN.md says the signed-out bar keeps 32 px.
2. `app.css` has no `code` rule, so about 20 bare `<code>` elements render in the browser's monospace, not `--font-mono`.
3. `StatePanel` prints `LoadError`, often an exception's message, with no Try again. This is T329's family.
4. `DashboardCard.razor:20` puts an inline style on the linked card.
5. The unselected `.tab-bar-tab` is edged in `--border-color`, below 3:1 for a control edge. This is T322's family.
6. 13 icons in `wwwroot/icons` are redrawn, not Lucide's paths: home, user, inbox, search, info, settings, calendar,
   users, book, file-text, pencil, trash and alert-triangle.

## What to build

- Scope the rule to `.sidebar`.
- Add a `code` rule on `--font-mono`.
- Give StatePanel a fixed error sentence with Try again (log the exception).
- Move the inline style to a class.
- Edge the tab in `--input-border`.
- Replace the 13 icons with Lucide's own paths, keeping `id="i"`.

Then re-sync `design/system/` for what changes.

## Verification

- [ ] Design tests: the brand-mark sizes per bar, the `code` rule, no inline style under `Components/Shared`, and the
  tab edge in ContrastTests — test names.
- [ ] Each of the 13 icons matches Lucide's path — a test or a diff.
