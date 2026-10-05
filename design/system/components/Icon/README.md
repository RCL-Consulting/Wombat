# Icon

An inline SVG line icon from Wombat's Lucide set, drawn in the colour of the text around it and hidden from screen readers.

## What the consumer provides

`<Icon Name="plus" Size="16" Class="…" />`

- `Name` (required): a file name in `wwwroot/icons/` without `.svg`, which is the icon's Lucide name. The set is the Icons asset group: 60 icons, 53 named by a page (flow 06 retired the last use of `alert-triangle`). Flow 02 (T339) added four, all Lucide's own: `eye` and `eye-off` (PasswordField's Show toggle, at rest and pressed), `building-2` (an institution: the sign-in page's institution buttons and My account's institutional sign-in rows) and `key-round` (My account's Password row). Flow 03 (T342) added one, Lucide's own: `copy` (the status card's "File it again, to someone else"), and named `lock` before a locked section's owner line and `chevron-right` on each instrument link and as the history's fold marker. Flow 04 (T350) added none: it names `history` on the Assessor's Recent decisions card (which had `check`, now named by no page) and `chevron-right` as the marker of "What each rung means". Flow 05 (T355) added one, Lucide's own: `target` (the Trainee's Your targets card, three circles), and names `history` on the Trainee's Recent decisions card and `award` on My authorisations, now a section rather than a linked card. The Trainee's Home no longer names `book` (Curriculum targets), `file-text` (Recent activities) or `calendar` (Upcoming deadlines); each is still named elsewhere.
- `Size`: pixels, default 16. A dashboard card's title and the phone trail's back chevron use 18, the chevron between crumbs 14, a nav row, the Menu toggle, the error bar and My account's sign-in rows 20, a system page's heading 24.
- `Class`: extra classes after `icon` (`page-title-icon`, `breadcrumb-back`, `breadcrumb-separator`, `error-bar-icon`, `error-bar-button-icon`, `password-toggle-eye`, `password-toggle-eye-off`, `signin-method-icon`, `history-details-marker`). The history's marker turns 90deg while its `<details>` is open, at `motion-fast` (none under reduced motion). The Show toggle renders both eyes and the stylesheet hides one from `aria-pressed`.

It renders `<svg class="icon" width="16" height="16" aria-hidden="true"><use href="/icons/plus.svg#i"></use></svg>`. The `.icon` class does the drawing: `fill: none; stroke: currentColor; stroke-width: 2; stroke-linecap: round; stroke-linejoin: round`. (The previews here inline each icon's paths, since a static preview cannot load the sprite files.)

## Where the names come from

- The navigation names each item's icon in `Navigation/NavItems.cs`; Home's header action in `Navigation/HomeFrame.cs`; the rest in the Razor that draws them.
- The alerts' and the validation message's icons are not `Icon`s: app.css draws them as a `::before` masked with an inline Lucide glyph (circle-check, info, triangle-alert, circle-alert), filled with the kind's colour.
- The reconnect dialog inlines its own Lucide paths (loader-circle, clock, circle-alert, pause, refresh-cw): it shows when the server cannot be reached, so it cannot fetch an `/icons/` file then.

## Rules (DESIGN.md § Icons; CLAUDE.md)

- Icons are Lucide (MIT), one SVG per icon whose root has `id="i"`: a new one is copied from Lucide, never redrawn. Thirteen older files are redrawings (the Icons group's README names them: `home`, `user`, `inbox`, `search`, `info`, `settings`, `calendar`, `users`, `book`, `file-text`, `pencil`, `trash`, `alert-triangle`).
- The Bootstrap Icons font is not loaded: `<i class="bi bi-*">` renders nothing. No icon fonts, emoji or pictures as icons.
- `building` (the older nav icon for institutions) and `building-2` (an institution one signs in through) both ship; use `building-2` for an institution's sign-in.
- **An icon after its words** is new with flow 03: the instrument link's `chevron-right` stands at the link's far end (the link is a flex row, space-between), saying the link opens a form; and the history's fold marker comes before its summary's words. Elsewhere an icon comes before its words.
- An icon is decorative: the words beside it carry the meaning, so it is `aria-hidden` next to a label. On a system page's heading it is too: the heading is named by its words.
- The mark is not an icon: it is multi-colour and shown as an `<img>`.

## Contrast

An icon takes its text's colour, so it meets 3:1 wherever that text meets 4.5:1. A coloured one is a meaningful mark held to 3:1: the system pages' `secondary-color` 4.61:1, `warning-color` 5.47:1 and `danger-color` 5.65:1 on the page. A sign-in row's icon on My account is `muted-text`, 5.09:1 on the surface; the pressed toggle's `eye-off` is `on-fill` on `secondary-color`, 4.86:1. A locked section's `lock` is `text-color` on `header-bg`, 11.36:1; an instrument link's chevron `link-color` on the surface, 6.70:1; File it again's `copy` `on-fill` on `secondary-color`, 4.86:1.
