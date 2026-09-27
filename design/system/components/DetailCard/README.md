# DetailCard

The `.detail-card` surface that groups fields inside a page, the building block of detail pages and dashboards, with variants for density, emphasis, warning, links and empty states.

There is no DetailCard component: a page writes `div.detail-card` (or `aside`, `section`, `article`) and a variant class. DashboardCard wraps it with a title.

## What the consumer provides

The element and its content: usually an `h2` or `h3`, then a `.details-list` (`<dl>` of `<div><dt>…</dt><dd>…</dd></div>` rows, labels in `muted-text`), a table, or text.

## Variants (DESIGN.md § Card system)

| Class | Use |
|---|---|
| `.detail-card` | `surface-color`, 1px `border-color`, `radius-lg`, `shadow-raised`, padding `space-lg` |
| `--compact` | padding `space-md`: a card inside a card, a per-EPA snapshot |
| `--header` | a padded header strip only |
| `--emphasis` | a 4px `secondary-color` stripe down the left: the card the page is for |
| `--warning` | a 4px `warning-color` stripe |
| `--interactive` | the whole card is a link: pointer, and on hover a 2px lift to `0 4px 12px` of `shadow-color`, over `motion-fast` |
| `--empty`, `--empty-compact` | dashed border, centred `muted-text`: "nothing yet" |

## Layout

- A detail page is `.details-grid`: a summary card beside the main card, 1fr 2fr, one column at 900px and below. `.details-grid--lone-spans` lets a lone card take the row, opt-in.
- `.stack-list` stacks cards with a `space-md` gap.

## Rules

- Anywhere fields group inside a page, reach for a card.
- A form with nothing left to offer is replaced by its empty state: a `div.detail-card.detail-card--empty-compact[tabindex=-1]` saying nothing is left and what would change that, which takes the focus when the last action removed the form.
- The empty-state card's dashed border reads as "nothing yet" at a glance.

## Known gaps

- DESIGN.md says `.detail-card h3` has a bottom border and the primary colour; app.css has no such rule, so a card title is a plain `h3`.
- `--interactive` keeps the card's `border-color` hairline (1.30:1), so a clickable card is told by its lift and pointer cursor, not its edge.
- DashboardCard writes its linked card's `<a>` with an inline `style="text-decoration:none;color:inherit;"`, which this preview copies; a class should carry it (DashboardCard, Known gaps).
