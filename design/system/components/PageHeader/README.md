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
- `TitleContent` (flow 05, T355, C5): a heading that holds more than words, drawn in the h1 in place of `Title`: an EPA's page, "PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams" with "(no longer in use)" in `span.paused-mark` (EpaLabel) and an institution's own EPA's badge. `Title` still names the crumb unless `CurrentCrumb` does, and must read as the h1 does, mark included, since the tab is the same words.
- `SubtitleContent` (flow 03, T342): a subtitle that holds more than words, drawn in the same `p.page-subtitle` in place of `Subtitle`: Log an activity's "Mini-CEX (Paediatrics) · Choose another type", whose last part is a link back to the picker.
- **An activity's page** (flow 03): the title is the activity's name, "Mini-CEX (Paediatrics) · PAED-001 · 2026-09-20" (" · <nominee>" where two share the rest, E7), which is also its tab's stem and its last crumb; the subtitle names its people, "Anele Dlamini's request to David Naidoo", "Sipho Ndlovu's reflection, for discussion with Sarah Botha", "Pieter du Plessis's draft, cancelled" (`ActivityPageModel.Subtitle`). No "State:" line: the status card says who has it. One header heads every state, its title changing, so the h1 FocusOnNavigate focused on arrival is the loaded page's: "Loading the activity" (the crumb "Loading…") while it loads, "Activity" when the read fails, "Activity unavailable" for one it cannot show.
- `Actions`: the primary action as `.btn-primary`, a companion as `.btn-outline`; on a form page a small outline way back ("Back to EPAs").
- `Icon` and `IconTone` (the system pages): a 24px Lucide icon before the heading, aria-hidden. `IconTone` is `info` (the default, `secondary-color`), `warning` or `danger`.
- `Trail`: crumbs between the owner and the page, where the owner table names one (the College on a College's specialities, for the Administrator).
- `CurrentCrumb`: the page's own crumb when it is not its title (a data-rights request's id; an EPA's page, its code: "PAED-012", "Loading…" while it loads, "EPA" when the read fails).
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

- **Under an owner:** Home › the owner › the page: the owner is the nav item the page lights for the acting role, so the trail follows the owning list, not the page the person came from. A page under a personal link (flow 05, E3: an EPA's page under My progress) has that owner whatever the acting role, none included: Home › My progress › PAED-001.
- **No owner for the acting role:** Home › the page.
- **None** on Home, on a list the acting role's menu offers, on My account and the sign-in pages, and on the system pages. Change password: Home › My account › Change password.

## Rules

- **One `<h1>` per page, from PageHeader.** Blazor's `FocusOnNavigate` focuses it after every navigation so a screen reader announces the page; that focus shows no ring. Never render a lone `<h1>`.
- A page's actions are offered only to those the page's commands accept. A dashboard adds no header: Home's is the only one.
