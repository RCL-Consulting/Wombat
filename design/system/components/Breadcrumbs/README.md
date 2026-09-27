# Breadcrumbs

A trail of links to the pages above this one, set in `muted-text` above the page header; the component exists, but no page renders it today.

## What the consumer provides

`<Breadcrumbs Items="@([new("Curricula", "/admin/curricula"), new("Paediatric EPA Curriculum 11.1", "/admin/curricula/3"), new("Items")])" />`: a list of `BreadcrumbItem(Label, Href)`, the last without an `Href`. It renders `nav[aria-label="Breadcrumb"] > ol.breadcrumbs > li`, each a link or, for the current page, a `span`. Nothing renders for an empty list.

## Rules

- The trail is separated by a `space-sm` gap only; there is no separator glyph.
- The current page is plain text, not a link.

## Known gaps

No page uses it, and several pages are reached only by address (design/BRIEF.md § 4). The restructure (W-008) may put it to work or drop it.
