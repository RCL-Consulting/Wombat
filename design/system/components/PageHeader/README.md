# PageHeader

The top of every page: its one `<h1>`, a subtitle, and a right-hand slot for the page's actions, over a 2px `secondary-color` rule.

## What the consumer provides

```razor
<PageHeader Title="MSF campaigns" Subtitle="Create and review multi-source feedback campaigns.">
  <Actions><a class="btn btn-primary" href="/msf/campaigns/new">New campaign</a></Actions>
</PageHeader>
```

- `Title` (required): the page's name, sentence case.
- `Subtitle`: one line saying what the page is for, as a `.page-subtitle` (`muted-text`, 0.9rem).
- `Actions`: the primary action as `.btn-primary`, with any companion as `.btn-outline`; on a form page, a small outline way back ("Back to EPAs").

It renders `div.header-container` (flex, wrapping, space-between, a `secondary-color` bottom rule, `space-xl` below) holding the heading block and a `div.actions-cell`.

## Rules (DESIGN.md § Typography; § Page-level patterns)

- One `<h1>` per page, and it comes from PageHeader: Blazor's `FocusOnNavigate` focuses it after every navigation so a screen reader announces the page; that focus shows no ring. Never render a lone `<h1>`.
- The `h1` is 1.5rem, never larger.
- Dashboards add none: Home's header ("Welcome, {name}" / "Viewing as {role}") is the only one.
- A page's actions are offered only to those the page's commands accept.
