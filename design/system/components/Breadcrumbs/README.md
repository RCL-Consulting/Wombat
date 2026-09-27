# Breadcrumbs

A page's trail above its header, Home › the list it is under › the page, drawn by PageHeader from the owner table, the last crumb the page itself; below 641px it folds to one 44px link back to its parent.

Until flow 01 (T335, b347e11c) the component existed and no page rendered it.

## What the consumer provides

Nothing, usually: PageHeader draws it from `NavOwners.TrailTo(page, acting role, menu)`, plus the page's `Trail` and its own crumb. Direct use: `<Breadcrumbs Items="@crumbs" />`, a list of `Crumb(Label, Href)`, Home first and the page itself last with no `Href`. Nothing renders for fewer than two crumbs.

## Markup

```html
<nav aria-label="Breadcrumb">
  <ol class="breadcrumbs">
    <li class="breadcrumb-ancestor">
      <a href="/"><svg class="icon breadcrumb-back">chevron-left 18px</svg>Home</a>
      <svg class="icon breadcrumb-separator">chevron-right 14px</svg>
    </li>
    <li class="breadcrumb-parent">
      <a href="/activities/inbox"><svg class="icon breadcrumb-back">…</svg>Activity inbox</a>
      <svg class="icon breadcrumb-separator">…</svg>
    </li>
    <li class="breadcrumb-current"><span aria-current="page">Mini-CEX (Paediatrics)</span></li>
  </ol>
</nav>
```

The crumb before the page's own is `.breadcrumb-parent`; any before it `.breadcrumb-ancestor`.

## Look

- `.breadcrumbs`: 0.9rem, `muted-text` for the chevrons, links in `link-color`, each link at least 24px tall; `space-xs` rows and 6px between crumbs; at the top of the article, 16px above the header.
- The page's own crumb is `text-color` at weight 600, not a link.
- **Below 641px** only `.breadcrumb-parent` shows, as one 44px link at weight 600 led by its `.breadcrumb-back` chevron; the ancestors, the current crumb and the separators are hidden.

## Rules (DESIGN.md § Breadcrumbs)

- The trail follows the owning list for the acting role (D5), not the page the person came from.
- A page drawn in place of another names itself (Access denied), so it draws no trail of the refused page's.
- Its words are the pages' own: a nav label, the h1 and the tab share them. The exception is a record's own crumb, which the page may set through PageHeader's `CurrentCrumb`: a data-rights request's crumb is its id, and the College admin on a College's specialities reads Home › Specialities › {College name} under the h1 "Specialities".
- No trail on Home, on a list the acting role's menu offers, on My account and the sign-in pages, or on the system pages. Change password is the one account page with one: Home › My account › Change password.
