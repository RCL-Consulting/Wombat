# T044 — Document dashboard composition pattern in DESIGN.md

Follow-up item #1 from the post-T042 backlog: "Dashboard design decision. All seven role dashboards lack `<PageTitle>`/`<PageHeader>` and use inline `style="..."` for flex layouts. Consistent but undocumented. Decide: document the pattern in `DESIGN.md`, or retrofit PageHeader + utility classes globally."

## Decision

**Document, don't retrofit.** The "dashboards lack PageHeader" observation from T041/T042 was a category error. Dashboards are not standalone routed pages; they are child components composed by `Home.razor`. `Home.razor` is the one route at `/`, and it renders a `<PageHeader Title="Welcome, {name}" Subtitle="Viewing as {role}" />` directly above the role-appropriate dashboard component. The `/dashboard/switch/{role}` endpoint in `Program.cs` is a minimal-API cookie-setter that 302s back to `/` — it never renders a dashboard directly.

So every rendered dashboard page has a PageHeader. It just isn't in the dashboard `.razor` file because those files are children. Adding one there would duplicate Home's header.

Retrofit was the wrong question. The right action is to update `DESIGN.md` so future contributors understand the composition and don't add a redundant header to a new dashboard.

## Changes

Update `Rewrite/DESIGN.md`:

- **Replace the "Dashboard page" pattern section** (previously a flat `<PageHeader>` + `<div class="dashboard-grid">` example). Call out explicitly that dashboards are a *composition*, that Home owns the PageHeader, that role dashboards must not have `@page` / `<PageTitle>` / `<PageHeader>`, and show a representative dashboard file shape using `<StatePanel>` + `<div class="dashboard-grid">` + `<DashboardCard>`.
- **Clarify inline style policy for dashboard items.** Token-backed inline `style="..."` on list items, progress-bar fills, and similar per-instance layouts is acceptable. Promote to a utility in `app.css` only once the same pattern appears in four or more dashboards. This resolves the "Dashboard inline style for flex layouts" backlog note without opening a separate refactor task.
- **Add `DashboardCard.razor`** to the "Files that own the design system" tree.
- **Update the "Dashboard layout grid" section** to reference `<DashboardCard>` as the primary card building block (previously said "each dashboard card is a `.detail-card`"). DashboardCard wraps `.detail-card` and adds `Title` / `Icon` / `Href` / `Emphasis` / `Warning` / `Span` knobs.

## Out of scope

- No Blazor source changes. Every rendered dashboard page is already correct.
- No `app.css` changes. The `.detail-card`, `.dashboard-grid`, and dashboard-metric primitives are already defined and exercised.
- Inline-style consolidation into utility classes remains a valid future task — but only if a pattern genuinely surfaces in 4+ dashboards, per the new rule.

## Definition of done

- DESIGN.md reflects the composition pattern.
- Follow-up backlog item #1 in `current_state.md` dropped.
- Dashboard inline-style note downgraded (policy now documented, no separate follow-up).

## Files touched

- `Rewrite/DESIGN.md` (+file-list entry, +replaced Dashboard page section, updated Dashboard layout grid paragraph)
- `Rewrite/Tasks/T044-document-dashboard-composition.md` (this file)
- `Rewrite/current_state.md` (backlog + commits list)
