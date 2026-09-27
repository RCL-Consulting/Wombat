# PageHeader

The top of every page: its trail (from the owner table), its one `<h1>` with an optional icon, a subtitle, and a slot for the page's actions, over a 2px `secondary-color` rule; under it, once, the result of a switch of the acting role.

## What the consumer provides

```razor
<PageTitle>MSF campaigns · Wombat</PageTitle>
<PageHeader Title="MSF campaigns" Subtitle="Create and review multi-source feedback campaigns.">
  <Actions>
    <a id="msf-coverage-link" class="btn btn-outline" href="/msf/coverage">MSF coverage</a>
    <a class="btn btn-primary" href="/msf/campaigns/new">New campaign</a>
  </Actions>
</PageHeader>
```

- `Title` (required): the page's name, sentence case, the same words as its nav label, its breadcrumb and the stem of its tab ("<Title> · Wombat").
- `Subtitle`: one line under it in `.page-subtitle` (`muted-text`, 0.9rem). Home's is "{acting role} · Semester N, YYYY": "Assessor · Semester 2, 2026"; for someone with no role, none (DashboardCard).
- `Actions`: the primary action as `.btn-primary`, a companion as `.btn-outline`; on a form page a small outline way back ("Back to EPAs").
- `Icon` and `IconTone` (the system pages): a 24px Lucide icon before the heading, aria-hidden. `IconTone` is `info` (the default, `secondary-color`), `warning` or `danger`.
- `Trail`: crumbs between the owner and the page, where the owner table names one (the College on a College's specialities, for the Administrator).
- `CurrentCrumb`: the page's own crumb when it is not its title (a data-rights request's id).
- `Page`: the page this header heads when it is not the routed one (Access denied, drawn in place of the page that refused, draws no trail).

## Markup

```html
<nav aria-label="Breadcrumb">…</nav>                 (Breadcrumbs, when the page has a trail)
<div class="header-container">
  <div>
    <h1 class="page-title-with-icon">(icon: svg.icon.page-title-icon[--warning|--danger]) Title</h1>
    <p class="page-subtitle">Subtitle</p>
  </div>
  <div class="actions-cell">…Actions…</div>
</div>
(ActingRoleSwitchAlert, after a switch)
```

## Layout (DESIGN.md § Typography, § Page-level patterns)

- `.header-container`: flex, wrapping, space-between, the actions on the heading's **baseline**; the h1 has no margin, the subtitle sits 0.25rem under it, the rule 0.75rem under that, and the page begins `space-lg` (24px) under the rule.
- The h1 is 1.5rem/600 (1.375rem below 641px), never larger; line height 1.25.
- **Home's one header action** (`.home-action`: the Trainee's "Log an activity", the Institutional admin's "Invite a person") is, below 641px, its own 44px row the page's width under the rule.
- **With an icon**, `.page-title-with-icon` lays the icon and the words out in a row with a 12px gap; in a signed-out system card the icon stands above the heading and the rule is gone.

## The trail (Breadcrumbs; NavOwners.TrailTo)

- **Under an owner:** Home › the owner › the page: the owner is the nav item the page lights for the acting role, so the trail follows the owning list, not the page the person came from.
- **No owner for the acting role:** Home › the page.
- **None** on Home, on a list the acting role's menu offers, on My account and the sign-in pages, and on the system pages. Change password: Home › My account › Change password.

## Rules

- **One `<h1>` per page, from PageHeader.** Blazor's `FocusOnNavigate` focuses it after every navigation so a screen reader announces the page; that focus shows no ring. Never render a lone `<h1>`.
- A page's actions are offered only to those the page's commands accept. A dashboard adds no header: Home's is the only one.
