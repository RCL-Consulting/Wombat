# Icon

An inline SVG line icon from Wombat's Lucide set, drawn in the colour of the text around it and hidden from screen readers.

## What the consumer provides

`<Icon Name="plus" Size="16" Class="…" />`

- `Name` (required): a file name in `wwwroot/icons/` without `.svg`, which is the icon's Lucide name. The set is the Icons asset group: 25 icons, 20 in use.
- `Size`: pixels, default 16. DashboardCard titles use 18; the nav sizes its icons to 1.25rem in CSS.
- `Class`: extra classes after `icon`.

It renders `<svg class="icon" width="16" height="16" aria-hidden="true"><use href="/icons/plus.svg#i"></use></svg>`. The `.icon` class does the drawing: `fill: none; stroke: currentColor; stroke-width: 2; stroke-linecap: round; stroke-linejoin: round`. (The previews here inline each icon's paths, since a static preview cannot load the sprite files.)

## Rules (DESIGN.md § Icons; CLAUDE.md)

- Icons are Lucide (MIT), one SVG per icon whose root has `id="i"`, copied from Lucide and never redrawn.
- The Bootstrap Icons font is not loaded: `<i class="bi bi-*">` renders nothing. No icon fonts, emoji or pictures as icons.
- An icon is decorative: the words beside it carry the meaning, so it is always `aria-hidden` next to a label.
- The nav uses the same component; NavMenu.razor.css only sizes and spaces it.
- The mark is not an icon: it is multi-colour and shown as an `<img>`.

## Contrast

An icon takes its text's colour, so it meets 3:1 wherever that text meets 4.5:1.
