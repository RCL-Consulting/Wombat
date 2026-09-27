# Design system

> **2026-09-27: the structural lock is lifted for the Claude Design restructure (W-008).** The operator chose a
> restructure with UX in scope. The shell, the navigation, the role dashboards, the page shapes and the task flows may
> change, not only the colours (`execution/DECISIONS.md` W-008; `design/BRIEF.md` § 4). So the lock this file states
> no longer binds the redesign. That lock is three passages:
> - the opening's "ported structurally from ClinicAssist … Only the colours differ";
> - § Non-negotiables' "Ask before widening the contract … silently deleting or renaming existing ones is not";
> - § Historical context's "Change the colours in `:root`, keep everything else".
>
> - **Everything below still binds whatever has not been redesigned.** A page that no redesigned flow has reached keeps
>   this contract, its class names and the tests that pin them.
> - **Each redesigned flow amends this file deliberately,** in the same task as its Razor, its `app.css` and the tests
>   that pin what it changes (`design/BRIEF.md` § 9). It then adds itself to the list below, with its date and commit.
> - **The invariants in `design/BRIEF.md` § 4.4 bind every redesign.** For example, every nav link opens a page that
>   admits the role, and there is one `<h1>` per page.
> - **Redesigned so far:** flow 01, the shell (2026-09-27, T335, `b347e11c`).

This file is the visual contract for the Wombat rewrite. It exists because the first pass at T010 said "copy ClinicAssist" without enumerating what that actually means, and the current `Wombat.Web/wwwroot/app.css` is still the 37-line Blazor default — raw `<h1>` + `<table class="table">` — which is nowhere near the reference.

The design system is **ported structurally from ClinicAssist** (same token names, same component class names, same spacing scale, same layout grid) with a **new Wombat palette**. That means `t010` ships the card system, the button system, the form system, the pager, the validation summary, etc. with the same class names ClinicAssist uses, so muscle memory transfers and the Razor pages look identical in shape. Only the colours differ.

Read this once at the start of any task that renders HTML (T010, T011, T019, and any new UI task). When a task says "use `.detail-card`" it means the class defined here.

## Files that own the design system

```
src/Wombat.Web/
├── wwwroot/
│   ├── app.css                                   ← global design system (sections below)
│   ├── fonts/                                    ← the two self-hosted faces, each beside its OFL text (W-011)
│   │   ├── SourceSans3VF-Upright.ttf.woff2       ← body face, Adobe's release 3.052R, unmodified
│   │   ├── SourceSans3VF-Italic.ttf.woff2
│   │   ├── SourceSans3-OFL.txt                   ← Adobe's LICENSE.md at 3.052R, whole
│   │   ├── fraunces-var.woff2                    ← the wordmark's display face (T089)
│   │   └── Fraunces-OFL.txt                      ← the Fraunces project's OFL.txt
│   ├── icons/                                    ← inline-SVG sprite files, one per icon
│   └── lib/…                                     ← bootstrap grid only, if needed; no Bootstrap components
└── Components/
    ├── Layout/
    │   ├── MainLayout.razor                      ← the shell: toggle, one <header> (brand, NavMenu, account row), main (T335)
    │   ├── MainLayout.razor.css                  ← the shell's grid at both widths, the phone bar and menu, the error bar
    │   ├── NavMenu.razor                         ← the role head and switch, the acting role's links, the personal links
    │   ├── NavMenu.razor.css                     ← the panel, the role head, the scrolling list, the group headings
    │   ├── NavItemLink.razor                     ← one nav row; lit (class active, aria-current) as NavOwners says
    │   └── NavItemLink.razor.css                 ← the row: 28px, 44px on a phone; hover, current, the nav ring
    └── Shared/
        ├── Icon.razor                            ← <Icon Name="check" /> renders inline SVG
        ├── PageHeader.razor                      ← trail + <h1> (an Icon before it, optional) + subtitle + action slot; renders ActingRoleSwitchAlert
        ├── ActingRoleSwitchAlert.razor           ← "You are now acting as …" once, after a switch (§ Acting role, T335)
        ├── Breadcrumbs.razor                     ← the trail PageHeader draws (§ Page-level patterns, T335)
        ├── DataTable.razor                       ← generic list shell using .clinic-table
        ├── FormField.razor                       ← <label> + input slot + validation message
        ├── ConfirmDialog.razor
        ├── ActionResult.razor                    ← .action-result region; takes the focus when an action is done (T234)
        ├── InFlight.cs                           ← aria-disabled for a button whose own action runs (T234 review)
        ├── EpaLabel.razor                        ← "Code — Title", "(no longer in use)" muted beside it when not in force (T255)
        ├── PagerControls.razor                   ← .pager .pager-actions .pager-page-size
        ├── StatePanel.razor                      ← empty / loading / error state shells
        ├── DashboardCard.razor                   ← <DashboardCard Title=…> wraps .detail-card; its title an <h2>, IsLoading, Count
        ├── DashboardFrame.razor                  ← a dashboard's grid, busy while it reads; one alert + Try again if it fails (T335)
        ├── RoleDashboard.cs                      ← base of a dashboard that reads one summary: the read, the retry, the log (T335)
        ├── ReferenceBlock.razor                  ← "Reference" + the trace id + the time in SAST, on the error page (T335)
        └── Skeleton.razor                        ← <div class="skeleton" />
```

`app.css` is the one global stylesheet. Component-scoped `.razor.css` files exist where a component has styles that do not belong in `app.css` (`MainLayout.razor.css`, `NavMenu.razor.css`, `NavItemLink.razor.css`, `ReconnectModal.razor.css`).

**The navigation is written in C#, not in Razor** (2026-09-27, T335, flow 01). `src/Wombat.Web/Navigation/NavItems.cs`
declares every nav item and which the acting role is offered (§ The NavMenu); `Navigation/NavOwners.cs` is the owner
table, the item a page is under, which lights the nav and draws the trail; `Navigation/ActingRole*.cs` is the acting
role (§ Acting role). `wwwroot/wombat.js` scrolls the lit item into the sidebar's view. **No other CSS framework.** No MudBlazor, no Radzen, no Bootstrap components (the grid file is optional — `.form-grid` below is native CSS grid and does not need it).

`Components/App.razor` links every first-party stylesheet and script through `@Assets["…"]` (`href="@Assets["app.css"]"`), which resolves to the content-hashed URL (`app.<hash>.css`) that `MapStaticAssets` serves with an immutable year-long cache. An edit changes the URL, so a deploy never pairs a new page with a cached old stylesheet. A new stylesheet or script is linked the same way, never by a bare path; `Hosting/AppAssetUrlTests` fails on a bare one (T175).

**The fonts (2026-09-27, T335, flow 01).** `app.css`'s `@font-face` rules load them from `/fonts/…`, which the CSP's
`font-src 'self'` admits; nothing is fetched from a font host. Both faces are under the SIL Open Font License 1.1 and are
served as files beside the application, never compiled into it: W-011 accepts that as aggregation. The rules for
vendoring them:
- **Source Sans 3** is Adobe's release 3.052R (github.com/adobe-fonts/source-sans, the latest release on 2026-09-27),
  the variable upright and italic files from `WOFF2/VF/` in its WOFF2 zip, byte for byte. They are identical to the
  repository's `WOFF2/VF/` files at the tag. They carry weights 200 to 900. The TrueType-flavoured ones are used, not
  the CFF2 ones.
- **Its licence reserves a name.** Upstream's `LICENSE.md` reads "Copyright 2010-2022 Adobe (http://www.adobe.com/), with
  Reserved Font Name 'Source'". Under the OFL's Reserved Font Name clause, a subset, converted or otherwise changed file
  is a Modified Version and may not be served as "Source Sans 3". So never trim or convert these files; vendor a newer
  release whole. `Design/TypographyTests` pins each file's SHA-256 to the release's. `SourceSans3-OFL.txt` is that
  `LICENSE.md`, whole.
- **Fraunces** (T089's wordmark face) reserves no name. `Fraunces-OFL.txt` is the project's own `OFL.txt`
  (github.com/undercasetype/Fraunces), identical at its master branch and its 1.000 release tag.
- A new face comes with its licence file in the same folder. `Design/TypographyTests` fails on a `.woff2` with no
  licence beside it.

## Design tokens

> **Amended 2026-09-27, T335, flow 01:** the round-3 token sheet (`design/flows/01-shell/round-3/R2-Tokens.dc.html`),
> which also closes T322's contrast faults. Changed: the success, warning and danger colours, the input border and the
> focus ring. New: `color-scheme`, `--on-fill`, `--link-hover`, `--scrim`, the three shadows, the `--nav-*` set, the
> radii, `--motion-fast`, `--font-body` and `--font-mono`. Removed: `--accent-color` and `--info-color`, which nothing
> read. Every colour is now written in one notation (N3).

Defined at `:root` in `app.css`. These are the **only** colours, radii, shadows and durations allowed: no colour is
written anywhere else in `app.css`, except inside a quoted SVG data URI (the select's chevron, the alert icons' masks)
and a system colour under `@media (forced-colors: active)`. `Design/StylesheetRuleTests` fails on any other.

```css
:root {
  color-scheme: light;                              /* dark mode is later (W-011) */

  --font-body: "Source Sans 3", "Segoe UI", system-ui, sans-serif;
  --font-mono: Consolas, "Courier New", monospace;
  --font-display: "Fraunces", Georgia, "Times New Roman", serif;  /* the wordmark only */
  --font-display-settings: "SOFT" 0, "WONK" 1;

  /* Page */
  --primary-color:    rgb(44 62 80);                /* #2C3E50 emphasis ink, a dashboard's figure */
  --secondary-color:  rgb(45 108 223);              /* #2D6CDF the one action colour; the header rule; info's edge */
  --on-fill:          rgb(255 255 255);             /* text and icons on a filled button */
  --background-color: rgb(248 249 250);             /* #F8F9FA the page */
  --surface-color:    rgb(255 255 255);             /* cards, tables, dialogs, and .btn-outline's fill */
  --text-color:       rgb(51 51 51);                /* #333333 body, and the words on every tint */
  --muted-text:       rgb(104 111 119);             /* #686F77 help, labels, subtitles */
  --link-color:       rgb(11 92 171);               /* #0B5CAB links, breadcrumbs */
  --link-hover:       rgb(8 74 138);                /* #084A8A a link under the pointer */
  --border-color:     rgb(222 226 230);             /* #DEE2E6 hairlines only, never a control's edge */
  --input-border:     rgb(123 132 141);             /* #7B848D a control's edge (was #CED4DA) */
  --focus-ring:       rgb(45 108 223);              /* #2D6CDF the ring on a light ground (was #3498DB) */
  --header-bg:        rgb(241 243 245);             /* #F1F3F5 table heads, skeleton, code, the reference block */
  --hover-bg:         rgb(248 249 250);             /* row hover, the progress track */

  /* Semantic: an edge, an icon, a dot, a fill or a button, never words on its own tint */
  --success-color:    rgb(26 115 64);               /* #1A7340 (was #27AE60) */
  --success-bg:       rgb(232 245 233);             /* #E8F5E9 */
  --warning-color:    rgb(154 84 0);                /* #9A5400 (was #FD7E14); the error bar's edge */
  --warning-bg:       rgb(255 248 225);             /* #FFF8E1 */
  --danger-color:     rgb(184 50 42);               /* #B8322A (was #E74C3C); validation text too */
  --danger-bg:        rgb(255 245 245);             /* #FFF5F5 */
  --info-bg:          rgb(243 248 255);             /* #F3F8FF; its edge is --secondary-color */

  /* Overlay and shadows */
  --scrim:            rgb(0 0 0 / 0.35);            /* a dialog's backdrop */
  --shadow-color:     rgb(0 0 0 / 0.1);
  --shadow-raised:    0 1px 3px var(--shadow-color);   /* cards, tables, forms, the signed-out cards */
  --shadow-dialog:    0 10px 25px rgb(0 0 0 / 0.25);   /* the reconnect and confirm dialogs */
  --shadow-bar:       0 -1px 2px rgb(0 0 0 / 0.2);     /* the error bar */

  /* Dark chrome: the sidebar, the phone bar, the signed-out bar */
  --sidebar-gradient-start: rgb(5 39 103);          /* #052767 */
  --sidebar-gradient-end:   rgb(58 6 71);           /* #3A0647 */
  --nav-text:         rgb(215 215 215);             /* #D7D7D7 an item at rest */
  --nav-text-strong:  rgb(255 255 255);             /* the current and hovered item, the role's name, the brand, the bar's controls */
  --nav-group-label:  rgb(185 195 224);             /* #B9C3E0 group headings, "Acting as", the switch's and toggle's edges */
  --nav-active-bg:    rgb(255 255 255 / 0.32);      /* the current item; #556C98 at the gradient's start */
  --nav-hover-bg:     rgb(255 255 255 / 0.1);       /* a hovered item */
  --nav-brand-bg:     rgb(0 0 0 / 0.4);             /* the brand cell at the sidebar's head */
  --nav-divider:      rgb(255 255 255 / 0.18);      /* the rules under the role head, above the personal links and the menu's foot */
  --nav-focus-ring:   rgb(255 255 255);             /* every focusable control on dark chrome */

  /* Shape and motion */
  --radius-sm: 4px;     /* nav rows, skeleton, a field's warning */
  --radius-md: 6px;     /* controls, buttons, alerts */
  --radius-lg: 8px;     /* cards */
  --radius-xl: 12px;    /* tables, forms, dialogs */
  --radius-pill: 999px; /* badges, tabs */
  --motion-fast: 150ms; /* a button's dim, a card's lift, a nav item's hover */

  /* Spacing scale: use these, not raw rem values */
  --space-xs:  0.25rem;
  --space-sm:  0.5rem;
  --space-md:  1rem;
  --space-lg:  1.5rem;
  --space-xl:  2rem;
  --space-2xl: 3rem;
}
```

**Rules:**

- Add a new token before hard-coding a colour, a radius, a shadow or a duration anywhere else.
- `primary-color` is ink / heading accent. `secondary-color` is for actions: buttons, links, the focus ring. Do not mix
  them up.
- **A semantic colour is never words on its own tint.** An alert's and a badge's words are `--text-color` on the tint,
  and the kind's colour is the edge, the icon or the dot (T322). The one semantic colour used as text is
  `--danger-color`, for a refusal or an error on a light ground (`.validation-message`, `.text-danger`).
- **Text on a filled button is `--on-fill`**, never `--surface-color`. Dark mode will change one and not the other.
- **A translucent token is valid only over the ground it was measured on.** `--nav-active-bg`, `--nav-hover-bg`,
  `--nav-brand-bg` and `--nav-divider` are for the sidebar's gradient; `--scrim` is for a dialog's backdrop.
- **`--nav-text` on `--nav-active-bg` is forbidden** (3.66:1 at the gradient's start). The current item's words are
  `--nav-text-strong`, and its cue is the 3px white bar and weight 600, not the fill (D4): at 0.32 the fill is 2.67:1
  against a plain item.
- **The two rings.** `--focus-ring` is 2.90:1 on the gradient's start, so every focusable control on dark chrome (the
  brand link, the menu toggle, nav links, the switch, the account link and Sign out at 390px, the signed-out bar's Sign
  in) takes `--nav-focus-ring`, set in that chrome's own stylesheet.
- The sidebar gradient tokens and the `--nav-*` set are read by the shell's stylesheets (`NavMenu.razor.css`,
  `MainLayout.razor.css`) only.
- `--hover-bg` equals `--background-color`, so a row's hover shows only on a surface (N3).
- The invalid-field stripe is not a token: `inset 4px 0 0 var(--danger-color)` (§ Form system).
- Radii are the five tokens, and a status dot's 50%. Shadows are the three tokens, except `.detail-card--interactive`'s
  lift under the pointer, `0 4px 12px var(--shadow-color)`, which the sheet does not name.
- If the palette changes, only `:root` changes, and the tables below are recomputed.
- Not tokens, never shipped: the sheet's canvas-only colours (#5B6068 and #E6E8EB behind the reconnect boards, the
  `.slot` stripes of a placeholder card, the phone list's scroll fade and drawn scrollbar, the disc drawn for the mark).
  The mark is `/brand/wombat-mark.svg`, an `<img>`, never recoloured.

### Contrast

Every pair the design system paints meets WCAG 2.1 AA: text 4.5:1; a control's edge, the focus ring and a meaningful
mark 3:1 (1.4.3, 1.4.11). `Design/ContrastTests` computes every figure below from `app.css`'s `:root`, and fails when a
pair falls short or a figure here differs from its computation. So change a token, then correct this table from the
test's message.

**The method.** WCAG 2.1 relative luminance on the token values. A translucent colour is blended over the ground it
actually sits on, unrounded, before it is measured. The sidebar's gradient is interpolated in sRGB, as a browser
interpolates a gradient of `rgb()` colours, and sampled at 21 points from its start (#052767) to its end (#3A0647); the
worst sample is the one that must pass, and the table prints the start, the middle and the end. The worst is the start
in every row.

**Where these differ from the token sheet.** The sheet rounded each blend, and the gradient's middle, to whole channels
(#03173E, #201657) before measuring; this table does not. So the brand cell is 17.52 (sheet 17.56), a hovered item at
the start 10.62 (10.58) and `--nav-text` at the middle 11.09 (11.11): the three the round-2 review named (N2). Several
other middle and end figures move in the second decimal. Every verdict is the same.

Page (each pair on a solid ground):

| Foreground / ground | Needs | Ratio |
|---|---|---|
| `--text-color` on surface · background · header-bg | 4.5 | 12.63 · 11.99 · 11.36 |
| `--text-color` on the success · warning · danger · info tints | 4.5 | 11.23 · 11.89 · 11.81 · 11.84 |
| `--muted-text` on surface · background · header-bg | 4.5 | 5.09 · 4.83 · 4.57 |
| `--muted-text` on the success · warning · danger · info tints | 4.5 | 4.52 · 4.79 · 4.75 · 4.77 |
| `--link-color` on surface · background · header-bg | 4.5 | 6.70 · 6.36 · 6.03 |
| `--link-color` on the success · warning · danger · info tints | 4.5 | 5.96 · 6.31 · 6.27 · 6.28 |
| `--link-hover` on surface | 4.5 | 8.91 |
| `--primary-color` on surface | 4.5 | 10.98 |
| `--on-fill` on secondary · success · danger (the filled buttons) | 4.5 | 4.86 · 5.88 · 5.95 |
| `--secondary-color` (an outline button's label and edge) on surface · background | 4.5 | 4.86 · 4.61 |
| `--danger-color` (validation text) on surface · background | 4.5 | 5.95 · 5.65 |
| `--input-border` on surface · background · header-bg | 3 | 3.80 · 3.60 · 3.42 |
| `--input-border` on the success · warning · danger · info tints | 3 | 3.38 · 3.58 · 3.55 · 3.56 |
| `--focus-ring` on surface · background · header-bg | 3 | 4.86 · 4.61 · 4.37 |
| `--focus-ring` on the success · warning · danger · info tints | 3 | 4.32 · 4.57 · 4.54 · 4.55 |
| Tint edges and icons: success · warning · danger · info (`--secondary-color`) on their tints | 3 | 5.23 · 5.43 · 5.56 · 4.55 |
| Neutral badge: text · edge on header-bg | 4.5 · 3 | 11.36 · 3.42 |
| Status dots on surface: success · warning · danger | 3 | 5.88 · 5.77 · 5.95 |
| Progress fill on its hover-bg track: secondary · success (complete) | 3 | 4.61 · 5.58 |
| Reconnect bar: secondary on its header-bg track | 3 | 4.37 |
| Error bar: text · the warning edge and icon on warning-bg | 4.5 · 3 | 11.89 · 5.43 |
| `--border-color` on surface (a hairline, never a control's edge) | none | 1.30 |

Before T322: the semantic colours as text on their tints were 2.55 (success), 2.42 (warning) and 3.57 (danger); white
on the success and danger buttons 2.87 and 3.82; validation text 3.82 on white; the ring 3.15 on white and 2.99 on the
page; an input's edge 1.49 on white; the ok and warn dots 2.87 and 2.57; a complete progress bar 2.73.

Dark chrome (the gradient's start · middle · end; a translucent token blended over it first). The phone bar is the
gradient's start as a solid, the phone menu's foot its end, and the signed-out bar the same gradient laid at 90deg, so
the gradient rows hold for it too:

| Foreground / ground | Needs | Ratio |
|---|---|---|
| `--nav-text` on the gradient | 4.5 | 9.78 · 11.09 · 11.25 |
| `--nav-text-strong` on the gradient (the brand, the role name, the signed-out bar's text) | 4.5 | 14.07 · 15.96 · 16.19 |
| `--nav-group-label` on the gradient (group headings, "Acting as", the switch's and the toggle's edges) | 4.5 | 8.00 · 9.08 · 9.21 |
| `--nav-text-strong` on `--nav-active-bg` over the gradient (the current item) | 4.5 | 5.27 · 5.73 · 6.08 |
| `--nav-text` on `--nav-active-bg` over the gradient | 4.5 | 3.66 · 3.98 · 4.22 (forbidden) |
| `--nav-text-strong` on `--nav-hover-bg` over the gradient (a hovered item) | 4.5 | 10.62 · 12.06 · 12.64 |
| `--nav-focus-ring` on the gradient | 3 | 14.07 · 15.96 · 16.19 |
| `--nav-focus-ring` on the current item (`--nav-active-bg` over the gradient) | 3 | 5.27 · 5.73 · 6.08 |
| `--nav-text-strong` on `--nav-brand-bg` over the gradient's start (the brand cell) | 4.5 | 17.52 |
| Phone bar (the gradient's start, solid): `--nav-text-strong` · `--nav-group-label` · `--nav-focus-ring` | 4.5 · 4.5 · 3 | 14.07 · 8.00 · 14.07 |
| Phone menu foot (the gradient's end, solid): `--nav-text-strong` · `--nav-group-label` · `--nav-focus-ring` | 4.5 · 3 · 3 | 16.19 · 9.21 · 16.19 |

The tables measure the tokens. `Design/ContrastTests` also reads the rules the shell's scoped stylesheets paint with
(`NavItemLink.razor.css`, `NavMenu.razor.css`, `MainLayout.razor.css`) and measures each pair on the ground it sits on:
the current item's words and bar on its fill, an item at rest and hovered, over the whole gradient; the switch, its edge,
Change role's options and the role head; the phone bar's brand, role and toggle on the gradient's start; the open menu's
name, Sign out and its edge on the gradient's end. So a rule that pairs the wrong tokens fails, not only a token that
changes (2026-09-27, T335, flow 01; the review of the t335 branch).

## Logo & brand assets (T089)

The Wombat mark is a stylised wombat face (white, on a navy→blue gradient disc) emerging from a darker
"burrow" mound. Source-of-truth SVGs live in `wwwroot/brand/`:

- `wombat-mark.svg` — the round mark, transparent corners. Used in the nav brand lockup, the login card,
  and as `favicon.svg`. This is the canonical vector logo; render it as an `<img>` (it is multi-colour, so
  it is **not** an `Icon.razor` glyph and does not use `currentColor`).
- `wombat-tile.svg` — a full-bleed, opaque square variant (same face, no transparent corners) used only to
  rasterise the OS app-icon tiles.

Raster fallbacks (generated from `wombat-tile.svg`, in `wwwroot/`): `favicon.ico` (16/32/48),
`apple-touch-icon.png` (180), `icon-192.png` / `icon-512.png` (PWA, `purpose: any maskable`). The head links
+ `site.webmanifest` + `theme-color` (`#2c3e50`) are wired in `Components/App.razor`.

**Lockup:** mark + `Wombat` wordmark in Fraunces 500 (§ Typography). White on dark chrome (`.brand`, in
`MainLayout.razor`); `--primary-color` on light (`.account-brand`). To re-skin, regenerate the four raster files from the
tile SVG and keep the mark/tile colours in sync with `:root`.

**The brand cell** (2026-09-27, T335, flow 01; D3, R2-Shell-*). The sidebar's head is a 56px cell on `--nav-brand-bg`,
the full sidebar's width, that is itself the link home: the mark as a 32px `<img alt="">` and the wordmark at 1.6rem,
which names the link. On a phone the same link starts the phone bar, the mark at 28px and the wordmark at 1.4rem, and on
the signed-out bar it is the mark at 32px. The mark is never recoloured and never an `Icon.razor` glyph.

## Typography

> **Amended 2026-09-27, T335, flow 01:** the body face is Source Sans 3 (W-011, § Files that own the design system),
> with line heights, inherited control fonts (T328) and tabular figures in tables.

```css
body {
  background-color: var(--background-color);
  color: var(--text-color);
  font-family: var(--font-body);   /* "Source Sans 3", "Segoe UI", system-ui, sans-serif */
  line-height: 1.5;
}

button, input, select, textarea { font: inherit; }   /* T328 */

h1 { font-size: 1.5rem;  font-weight: 600; line-height: 1.25; margin-bottom: var(--space-md); }
h2 { font-size: 1.25rem; font-weight: 600; line-height: 1.25; margin-bottom: var(--space-sm); }
h3 { font-size: 1.1rem;  font-weight: 600; line-height: 1.25; margin-bottom: var(--space-sm); }
h4 { font-size: 1rem;    font-weight: 600; line-height: 1.25; margin-bottom: var(--space-xs); }
h5 { font-size: 0.9rem;  font-weight: 600; line-height: 1.25; margin-bottom: var(--space-xs); }
@media (max-width: 640.98px) { h1 { font-size: 1.375rem; } }

.header-container    { …; margin-bottom: var(--space-lg); padding-bottom: 0.75rem; }  /* the page header, below */
.header-container h1 { margin: 0; }
.page-subtitle { font-size: 0.9rem; color: var(--muted-text); margin: var(--space-xs) 0 0; }
.dashboard-card-title { …; margin-top: 0; margin-bottom: var(--space-sm); }
.clinic-table  { font-variant-numeric: tabular-nums; }
.font-mono, .code-block { font-family: var(--font-mono); }
```

Page `<h1>` is `1.5rem`, not bigger, and `1.375rem` below 641px, as every 390px board sets it. Pages use `PageHeader` (section below) to keep the subtitle + action slot consistent. Do not render a lone `<h1>` in a page — reach for `PageHeader`.

**The page header is spaced as the boards draw it** (2026-09-27, T335, flow 01; the review of the t335 branch; R2-Shell-*,
R2-Landing-*, R2-Detail-*, R2-Error-*). The heading has no margin of its own in the header, the subtitle keeps its own
0.25rem under it, the rule (2px `--secondary-color`) is 0.75rem under them, and the page begins `--space-lg` (24px) under
the rule. A dashboard card's title (`.dashboard-card-title`, an `<h2>`) has no top margin. Until the review the heading
kept the browser's top margin (0.67em) and 1rem under it, the rule sat 1rem under the subtitle, and the page began 2rem
under the rule. **This reflows every page**: every header loses the heading's two margins and 4px of padding, and the
page begins 8px closer to the rule.
`Design/StylesheetRuleTests` and `Design/NarrowLayoutTests` hold it. The phone boards draw the rule 10px under the
heading and the page 16px under it; the build keeps 12px and 24px at every width.

The scale, from the token sheet:

| Use | Size / weight |
|---|---|
| The wordmark | Fraunces 500, 1.6rem in the sidebar (2.4rem on the sign-in card) |
| h1 · h2 · h3 (card titles) · h4 · h5 | 1.5 · 1.25 · 1.1 · 1 · 0.9rem, all 600; h1 1.375rem below 641px |
| Body | 1rem, line height 1.5 |
| A nav label | 0.9375rem; 600 when it is the current item |
| A nav group's label | 0.8125rem, 600, sentence case |
| A subtitle | 0.9rem, `--muted-text` |
| Small text | 0.875rem |
| A badge | 0.75rem, 600 |
| A button | 0.95rem, 600; `.btn-sm` 0.85rem |
| A dashboard's figure | 2rem, 700, `--primary-color`, line height 1 |
| Code | `--font-mono`, 0.875rem |
| A time | always with its zone: "2026-09-26 15:14 SAST" |

- **Sentence case throughout.** A nav label, its page's `<h1>`, its breadcrumb and the stem of its tab title are the same
  words ("Activity inbox · Wombat").
- **Controls take the body's font** (T328). Before, a textarea rendered in the browser's monospace and a select in
  Arial, and a `<button class="btn">` stood shorter than an `<a class="btn">` beside it. `font: inherit` carries the
  family, the size and the line height, and a class that sets its own size (`.form-control`, `.btn`) still does. The
  builder's JSON textareas keep their own monospace.
- **Tables set their figures in columns.** Source Sans 3's default figures are tabular already (every digit is 472
  units wide in 3.052R); `tabular-nums` holds the fallback faces to the same.
- **A control is 38px tall**: a 24px line between 6px of padding and its 1px edge (`.form-control`, `.form-select`,
  `.search-input`; `min-height: 2.375rem`). With the old 12px padding, the body's 1.5 line height would have made it
  50px; before T335 it was about 44px.
- **The type change reflows every page** (the round-2 review, S22(d)). Source Sans 3 sets narrower than Segoe UI, and
  the 1.5 line height is taller than `normal`: a table row, a card and a list grow a few pixels, and a label may fit on
  one line where it wrapped.

## Layout grid (the shell)

> **Rewritten 2026-09-27, T335, flow 01** (R2-Shell-*, R2-Phone-*, R2-Sidebar-Scroll, R2-Tokens' shell figures; review
> S11, S12, D3, D9). Until then the shell was ClinicAssist's: a `.top-row` over the page and a `.navbar-toggler` menu.

`MainLayout.razor` is the whole frame, one DOM at every width, and `MainLayout.razor.css` lays it out. The shell reads no
database: the name is the `display_name` claim and the role the cascaded acting role, so a page drawn in a database
outage, the error page's, still has its frame (S7); signed out, when the rerun could not check the sign-in (§ System
pages). `Navigation/MainLayoutTests` holds it, and reads every component the rendered shell holds for what it has
injected, `@inject` and `[Inject]` alike: only `NavigationManager` and `IJSRuntime` (the review of the t335 branch; the
scan had read the three files' `@inject` lines alone).

**The chrome is one `<header>`, the page's banner** (2026-09-27, T335, flow 01; the review of the t335 branch): the brand,
the phone bar's acting role and Menu toggle, the navigation (`<nav aria-label="Main">`) and the account row. Until the
review the brand and the account row sat outside every landmark. The header is `.sidebar`, a real box at both widths, so
no landmark relies on `display: contents`, which some screen readers drop a landmark for; `<main>` is its sibling.

```
from 641px                                          below 641px, folded        below 641px, open
+--------------+-----------------------------+      +------------------------+ +------------------------+
| .brand 56px  | .account-row, the top bar   |      | brand  Acting as [Menu]| | brand           [Close]|
+--------------+   name  [Sign out]  56px    |      |        Trainee         | +------------------------+
| role head    +-----------------------------+      +------------------------+ | role head, switch      |
+--------------+ main > article              |      | main > article         | | .nav-list (scrolls)    |
| .nav-list    |   trail, PageHeader, @Body  |      |   trail folded to its  | |                        |
| (scrolls)    |                             |      |   parent, PageHeader,  | +------------------------+
| 250px, full  |                             |      |   @Body                | | .account-row: name,    |
| height       |                             |      |                        | | Sign out (pinned foot) |
+--------------+-----------------------------+      +------------------------+ +------------------------+
```

- **From 641px** `.page` is a grid: `var(--sidebar-width)` (250px, written once in `:root`) beside `minmax(0, 1fr)`, and
  a first row of 56px, the top bar's, over the page.
  - **The sidebar** (the header) is a sticky, full-height flex column (`height: 100vh`), so its gradient (180deg,
    `--sidebar-gradient-start` to `-end` at 70%) runs the window's height whatever the list's length (S12). In it: the
    brand cell (§ Logo & brand assets), the role head, and the list, which alone scrolls (§ The NavMenu).
  - **The top bar** is the account row, the header's last child, `position: fixed` over the grid's first row, from
    `var(--sidebar-width)` to the window's right edge: in the banner, and where it was when it was a sticky row of the
    grid (until the review of the t335 branch). Light (`--surface-color`), 56px, over the page. Right-aligned: the
    person's name, a link to My account at least 32px tall (T328: it was 21px), then one Sign out, an outline small
    button with its icon. The name is the display-name claim, the whole of it in the link, so it is the link's
    accessible name and its title; the bar cuts it at 28 characters (`max-width: 28ch`) with an ellipsis. An account
    with no name shows its email, cut the same way. On My account the name is the current page: `aria-current="page"`
    and a 3px `--secondary-color` underline (R2-Rules § 3).
- **Below 641px** the header is a grid of its own: the bar's row (brand, role, toggle), the panel's and the foot's.
  - **The phone bar**, on `--sidebar-gradient-start`, solid (the header's ground): the brand, "Acting as" over the role's
    label (right-aligned, at most 120px wide, two lines at most, never cut: the bar grows), and the Menu toggle.
  - **The toggle is CSS only** (S11). A visually hidden checkbox, `#nav-toggle`, is the first child of `.page`, so every
    part of the shell is its later sibling or inside one. It is keyed by a count of the circuit's navigations (`@key`,
    bumped on every `LocationChanged`): every navigation replaces it, unchecked, so the next page does not open under the
    menu, a navigation to the page shown included (the lit item tapped, the brand tapped on Home), which a key by path
    kept open (the review of the t335 branch). Its `<label>`, in the header, is the 44px target, "Menu", or "Close" with
    the menu open. It carries no `aria-expanded`: the checkbox is the control, and its checked state is what a screen
    reader hears. The ring goes on the label, through `#nav-toggle:focus-visible ~ .sidebar .nav-toggle-label`. From
    641px both are `display: none`, so the checkbox is no tab stop.
  - **Open**, the menu takes the page's place: the bar shows the brand and Close, then the panel (the role head, the
    switch at 44px, the list with 44px rows, which scrolls), then **the account row pinned at its foot** on
    `--sidebar-gradient-end` (D9), the name whole and wrapping, and Sign out at 44px. It is the same element and the
    same Sign out form as the desktop top bar, placed by CSS: there is one Sign out in the DOM. The header takes the
    window's height (`100dvh`), so the list scrolls between the head and the foot, and `:has()` lets the page shrink to
    it (without `:has()` the page keeps its `100vh`, a little more on a phone whose address bar shows).
- **Signed out** (the static pages a visitor reaches): a gradient bar only (90deg), with the brand and a Sign in link
  (32px, 44px on a phone), and no navigation.
- **The page has a side gutter at every width** (T226): the account row and `article` pad their sides by `--space-md`
  (16px) outside any media query, and from 641px by `--space-xl` (32px) on the left and `--space-lg` (24px) on the right.
  Until T226 the phone width had none, and a page's header and cards ran from the screen's edge to its edge.
  `Design/NarrowLayoutTests` pins it.
- **The page begins under the bar by the article's own padding** (2026-09-27, T335, flow 01; the review of the t335
  branch), `main > article`: 16px, 24px from 641px, as every board's content column. Until then the heading's browser top
  margin was the only space above a page; the heading has none in the header now (§ Typography). A trail stands at the
  article's top, 16px above the header (§ Breadcrumbs), and the signed-out system card keeps its 80px under the bar
  (`.system-card-page` pads 56px and the article 24px; below 641px the article's 16px alone).
- **Every focusable control on dark chrome takes `--nav-focus-ring`** (S13): the brand, the toggle's label, the nav
  rows, the switch and Change role, the phone menu's name and Sign out, and Sign in. The page's ring is 2.90:1 on the
  gradient's start. The brand cell's ring is inset (`outline-offset: -4px`), since the cell runs to the window's edge.
- Below the layout, outside its `AuthorizeView`, a fixed `#blazor-error-ui` bar shows when a page's circuit fails, with
  Reload and Dismiss; from 641px it starts at `var(--sidebar-width)`, beside the sidebar, and below it spans the width.
  It is § The reconnect dialog and the error bar (2026-09-27, T335, flow 01); until then it was ClinicAssist's, copied
  verbatim.
- **In a Windows contrast theme** (`@media (forced-colors: active)`; the review of the t335 branch) a box-shadow is not
  painted, and both of the shell's current-page cues were box-shadows. The lit nav item's 3px bar is a 3px `CanvasText`
  left border, the row's left padding giving back its width (`NavItemLink.razor.css`), and My account's underline is a
  3px `CanvasText` text underline (`MainLayout.razor.css`). `Navigation/MainLayoutTests` holds both.

## The NavMenu

> **Rewritten 2026-09-27, T335, flow 01** (the pick, variation A with C's breadcrumbs; R2-Rules; A-Spec; review S7–S12,
> S21, D3–D5, D8; T331). Until then the nav was the union of every role held, in Title Case, with My Account, Data Rights
> and Logout written out and five `/placeholder/` stubs.

`NavMenu.razor` renders one `<nav aria-label="Main">`: the role head, then the acting role's links, then the personal
links under a rule. What it offers is `Navigation/NavItems.For(acting role, claims)`, which `MainLayout` builds and
cascades; it is declared once, in `NavItems.cs`, so a page has one label and one icon wherever it is offered (T178).

**One acting role at a time, never the union.** The links are the acting role's (§ Acting role). Access is the union of
the roles held and never changes: a page outside the acting role's links still opens, with nothing lit, and following a
link never switches the role (R2-Rules § 1).

**The role head** (R2-Rules § 4, R2-Phone-Bars). "Acting as" and the role's label (`WombatRoleLabels`), one layout that
wraps: the label and the role on one line where they fit, the role under it where not, never cut.
- **Two roles held:** one "Switch to {label}" link under it.
- **Three or more:** a native `<details>` "Change role" that lists "Switch to {label}" for each other role held, in
  `DashboardPriority.Order`. No script, so it works on a static page too.
- Every switch is `ActingRoleSwitch.Url(role)` with no return address, so it lands on the new role's Home, and carries
  `data-enhance-nav="false"`: a full page load (§ Acting role). Only held roles are offered.
- **No role** (a former trainee, D8): no head and no switch; the list starts under the brand. A Pending trainee reads
  "Acting as Pending trainee", like any role.
- The phone bar shows "Acting as" and the role too, while the menu is folded (§ Layout grid).
- `RoleSwitchTests` holds the head.

**The links.** Home first. A label is its page's `<h1>` and the stem of its tab, in sentence case (D10);
`NavMenuAuthorizationTests` holds each label to its page's `<PageTitle>`. Up to 8 links, Home and the personal links
counted, the list is flat; more than 8, it is grouped under headings that are not links. A heading is a `<p>`, not a
heading element (the page's `<h1>` is its first heading), and names its `<ul>` through `aria-labelledby`. Only the
Administrator's (17) and the Institutional admin's (16) are grouped.

Each row is what that acting role is offered, in menu order: "Heading: links" for a group, `;` between groups.
`NavMenuAuthorizationTests` builds the menu for every row and fails when the two differ, so a change to the nav is a
change to this table.

| Acting role | Links |
|---|---|
| Administrator | Home; Platform: Scheduled jobs, Audit log, SSO mappings, Data rights requests; Organisations: Institutions, Colleges; People: Users, Invitations; Catalogue: EPAs, Curricula, Activity types, Entrustment scales; Reviews: Decisions due, Committee reviews, Decision panels |
| InstitutionalAdmin | Home; People: Invitations, Trainees, Assessors, Users; Curriculum: Curriculum adoptions, Curricula, EPAs, Activity types, Entrustment scales; Reviews: Decisions due, Committee reviews, Decision panels; Access and audit: SSO mappings, Audit log |
| CollegeAdmin | Home, Specialities, EPAs, Curricula, Activity types |
| SpecialityAdmin | Home, Decisions due, Committee reviews, Decision panels |
| SubSpecialityAdmin | Home, Decisions due, Committee reviews, Decision panels |
| CommitteeMember | Home, Committee reviews, Decision panels |
| Coordinator | Home, Decisions due, MSF campaigns, Committee reviews, Data rights requests |
| Assessor | Home, Activity inbox |
| Trainee | Home, Log an activity, My activities, MSF reports, My committee reviews, Export portfolio |
| PendingTrainee | Home, Log an activity, My activities |
| No role | Home |

**The personal links**, last, under a rule, are the person's rather than the role's, so they are offered whatever the
acting role (D8):

| Personal link | Offered to |
|---|---|
| My progress | a holder of the Trainee role or of a trainee record (the `trainee_record` claim, current or ended, T252): exactly whom its page's policy, `TraineeOrFormerTrainee`, admits. A graduate acting as Assessor sees it. |
| My data rights | everyone signed in |

**What the nav never holds.**
- **No link to an unbuilt page.** The placeholder page and its five stubs are gone (the review's S22e): Recent
  activities and System were dropped, Stalled activities and Programme trainees are flow 06's, and the STAR review queue
  is flow 09's. `NavMenuAuthorizationTests` fails on a `/placeholder/` link and on a page that answers one.
- **No Sign out and no My account:** the account row carries both (§ Layout grid).
- **No link the acting role cannot open.** `NavMenuAuthorizationTests` judges every rendered link against its page's
  `[Authorize]`, through the app's own policies, for every acting role. So the Coordinator is not offered Invitations,
  the speciality admins are not offered Curricula, a Committee member is not offered Decisions due (T131 slice 6), and a
  Pending trainee is offered only what admits one (T141).
- **No page twice, and no two links with one name,** for any acting role, personal links included (T178). That is why the
  trainee's own reviews are "My committee reviews" beside the staff's "Committee reviews", and the queue of other
  people's requests is "Data rights requests" beside everyone's own "My data rights".

**The current item** (R2-Rules § 3; S9; T331). At most one item is lit. `NavItemLink` is a plain link, not `NavLink`, so
no address prefix lights anything: `Navigation/NavOwners.Lit` decides, from the page `Routes` cascades (its `RouteData`),
the acting role and the menu.
- **A list lights itself**, with `aria-current="page"`: the page shown is the page of an item the menu offers.
- **A page under a list lights its owner** for the acting role, with `aria-current="true"`: the first row of the owner
  table below naming the role, when the menu offers that item.
- **Where the acting role has no owner, nothing is lit.** "Lights Home" is retired.
- **My account, Change password, the sign-in pages, the anonymous static pages and the failure pages light nothing**
  (`NavOwners.Outside`). On My account the account row's name carries `aria-current="page"`.
- Lit, the row takes the class `active`: the `--nav-active-bg` fill (.32, D4), `--nav-text-strong`, weight 600 and a 3px
  inset bar on its left. The bar and the weight are the state's cue; the fill is 2.67:1 against a plain row.
- **An owner is named only for a role its page admits.** So Access denied, drawn at the address of a page that refused
  (D6), has no owner for the acting role, and the refusal lights nothing. `ActiveNavItemTests` holds every row to its
  page's `[Authorize]` and to the role's menu, renders every routable page for every acting role, and fails on a page
  that is neither an item's page, in the table, nor outside the rule. It also renders the real `Routes`, signed in,
  across navigations in the circuit (Home lit; Change password lighting nothing under Home › My account; Home lit again),
  which fails without the `RouteData` cascade `Routes.razor` wraps the page in (the review of the t335 branch).

The owner table (`NavOwners.Table`): a page, and the item it is under for each acting role. A role not named has no owner
there, and its trail is Home › the page (§ Page-level patterns).

| Page | Lit item, by acting role |
|---|---|
| An activity (`/activities/{id}`) | Assessor: Activity inbox · Trainee, Pending trainee: My activities |
| A committee review | Committee member, Coordinator, Speciality and Sub-speciality admin, Institutional admin, Administrator: Committee reviews (the trainee reads theirs on My committee reviews; the page does not admit them) |
| A decision panel | Administrator, Institutional admin, Speciality and Sub-speciality admin: Decision panels |
| An MSF campaign, `/msf/reports/{id}`, `/msf/coverage` | Coordinator: MSF campaigns (T331) |
| A data-rights request | Coordinator, Administrator: Data rights requests. Its crumb is the request's id once loaded (R2-Rules § 3). R2-Rules also names the requester, under My data rights: that row was dropped, since the page admits only Administrator and Coordinator, so the owner table may not name the requester's role (the review of the t335 branch) |
| A College's specialities and sub-specialities, and their edit pages | College admin: Specialities · Administrator: Colleges (T331) |
| A College | Administrator: Colleges |
| An institution | Administrator: Institutions |
| An EPA, an activity type, a curriculum's items | Administrator, College admin, Institutional admin: EPAs, Activity types, Curricula |
| A curriculum | Administrator, College admin: Curricula |
| `/admin/curriculum-progress` | Administrator: Curricula (a link on Curricula, now that Home's Maintenance card is dropped) |
| An entrustment scale | Administrator: Entrustment scales |
| A user | Administrator, Institutional admin: Users |
| A trainee's profile, an assessor's profile | Institutional admin: Trainees, Assessors |
| An audit entry | Administrator, Institutional admin: Audit log |
| The job run history | Administrator: Scheduled jobs |
| My authorisations | Trainee: My progress |
| Entrustment decisions | none: under no list until flow 09 places it |

MSF coverage (`/msf/coverage`, T210) is reached from the MSF campaign list's header, an outline "MSF coverage" link
(`#msf-coverage-link`) beside New campaign. The link is not offered to someone who holds Trainee, whom the page shows no
programme (`GetMsfProgrammeCoverageQuery.ShowsNoProgrammeTo`).

**The list scrolls** (S12, R2-Sidebar-Scroll): `.nav-list { flex: 1 1 auto; min-height: 0; overflow-y: auto; padding: 4px
8px 8px }` under the fixed brand cell and role head, rows 28px (44px on a phone) and 4px apart, so a 2px ring at a 2px
offset never meets the next row, and the padding keeps the first and last rings inside the scroll edge. It takes no
tabindex: Tab walks its links and the browser brings each into view. The lit item is scrolled into view
(`wombat.revealCurrentNavItem`, in `wombat.js`, keeping 8px of the list around it, the rows' `scroll-margin-block`): as a
static page loads, after an enhanced navigation, when the phone menu opens, and from `NavMenu` after the circuit's first
render, which replaces the prerendered list, or when a navigation lights another item. Only the list scrolls, never the
page. The scroll is decoration: `NavMenu` catches its call failing whatever the cause (the circuit gone, a script error,
the call cancelled), since a failure left to the renderer would end the circuit (2026-09-27, T335, flow 01; the review
of the t335 branch; `ActiveNavItemTests`).

**Styles.** Every colour is a `--nav-*` token (R2-Tokens: text at rest `--nav-text`; the current and hovered item, the
role's name and the bar's controls `--nav-text-strong`; headings, "Acting as" and the switch's edge `--nav-group-label`;
rules `--nav-divider`), and every control's ring is `--nav-focus-ring`. Each item's icon is the shared
`<Icon Name="…" />` (§ Icons), a Lucide glyph named in `NavItems.cs`. Every class in `NavMenu.razor` and
`NavItemLink.razor` is theirs or app.css's (T266).

New items go in `NavItems.cs` and in this table, and a new page that is not an item's goes in the owner table
(`NavOwners`), not anywhere else.

## Button system

> **Amended 2026-09-27, T335, flow 01:** every button is at least 36px with a 1px edge, filled ones take `--on-fill`,
> and `.btn-outline` is filled with the surface (S16).

Class order is **`.btn .btn-sm .btn-{variant} [spacing utilities]`**. The sizing comes before the variant.

```css
.btn            /* border-box, 1px solid transparent edge, min-height 2.25rem (36px), padding --space-xs --space-md,
                   radius --radius-md, 0.95rem / 600, hover filter:brightness(.95) over --motion-fast */
:focus-visible  /* 2px --focus-ring outline, offset 2px: every focusable element's (§ Accessibility) */

.btn-primary    /* bg and edge --secondary-color, text --on-fill (4.86:1) */
.btn-success    /* bg and edge --success-color, text --on-fill (5.88:1) */
.btn-danger     /* bg and edge --danger-color, text --on-fill (5.95:1) */
.btn-outline    /* bg --surface-color, edge and text --secondary-color (4.86:1) */

.btn-sm         /* min-height 1.75rem (28px), padding .125rem .625rem, font .85rem */
.btn-xs         /* min-height 1.75rem (28px), padding .15rem .4rem, font .7rem */
```

- **One height for the header's actions.** Every `.btn` carries a 1px edge, transparent unless its variant colours it,
  and is at least 36px, so a filled button and an outline one stand the same height, whether each is a `<button>` or an
  `<a>`. T328 measured them apart: the `<button>` kept the browser's control font and its `normal` line height, the
  outline button's 1px border made it 2px taller than a filled one, and an `<a class="btn">` sized its content box, not
  its border box as a `<button>` does, so its padding and edge stood on top of the minimum height. A long label may wrap; the padding keeps it clear of
  the edge.
- **`.btn-outline` is filled with `--surface-color`, never transparent.** Transparent, its label fell to 4.37:1 on
  header-bg and 4.32:1 on a success tint, and passed by 0.07 on the error bar's warning tint. With its own ground it is
  4.86:1 wherever it sits (the round-2 review, S16). `Design/ContrastTests` pins the fill.
- **Text and icons on a filled button are `--on-fill`**, never `--surface-color` (T322 made the success and danger
  fills dark enough for it: they were 2.87 and 3.82:1 under white).
- A small button (`.btn-sm`, `.btn-xs`) is at least 28px, over WCAG 2.5.8's 24px (T086).

**Rules:**

- Never use Bootstrap's `.btn-outline-primary` / `.btn-outline-success` / `.btn-outline-danger`. Use `.btn-outline` and let the tokens drive the colour.
- Row-level list actions (Edit, Delete) are `.btn .btn-sm .btn-outline`, wrapped in `<div class="actions-cell">` for a flex-gap cluster.
- The primary page action is `.btn .btn-primary` and lives in the `PageHeader` action slot.
- Destructive actions open a `ConfirmDialog` first; the red `.btn-danger` only appears inside the dialog's footer.
- A destructive row action on a list (Withdraw on the MSF campaign list, T206) is one `ConfirmDialog` for the page,
  whose body names the row it was opened from and says what cannot be undone. The row's button carries an `aria-label`
  that starts with its visible label and names the row ("Withdraw the campaign for …"): a column of identical
  "Withdraw" buttons is otherwise indistinguishable to a screen reader. It is disabled while the action is in flight.
  Its result is an `Alert` with `Role="status"` (done) or `Role="alert"` (refused) inside a `.action-result` region
  (`tabindex="-1"`), which takes the focus in `OnAfterRenderAsync`, after the dialog has closed: the row's button may be
  gone, and while the modal is open nothing outside it can take the focus (T206 review).
- The same holds on a form page whose confirmed action removes the button that opened the dialog: Deactivate on the EPA
  page is offered only on an active EPA, so once it deactivates, its result takes the focus (T196 review). A refused
  deactivation leaves its button, and the dialog hands the focus back to it.
- **A button is never disabled by its own action while that action runs** (T234; T225 review). It has the focus, and a
  browser drops the focus of a button it disables, to the page body: until T234, Save on the EPA page and the action
  buttons of 25 other pages left `document.activeElement` on BODY. The action's in-flight flag, set before its first
  `await`, makes a second press send nothing (T202); the button may say what it is doing ("Saving..."). While it runs
  the button carries `aria-disabled="true"` (`InFlight.AriaDisabled(running)`, Components/Shared; T234 review), so a
  screen reader hears that it is unavailable: a changed label on the focused button is often not read.
  `.btn[aria-disabled]` looks unavailable and keeps its focus ring. Every other button the action would race is disabled
  while it runs. Where one flag serves several buttons, the page records which one was pressed (`OtherInFlight(action)`
  and `Running(action)` on the user page, the activity type builder, the jobs list and the committee review page; the
  row's id on the invitations, SSO mappings and data-rights pages). A `ConfirmDialog`'s confirm button is held to the
  same rule while its `OnConfirm` runs, and its Cancel is disabled then.
- **An action that is done moves the focus to its result** (T234), an `Alert` in an `.action-result` region
  (`tabindex="-1"`) where the page reports the result (at the top of a form page; under the button on the rebuild page,
  under the form on the export check, under the table on the jobs list), once the page has rendered it. A status that
  arrives on its own is often not read, and what the action changed may have taken its button away (a cleared choice, a
  closed row, a form rebuilt with what was stored, a swapped button). The shared `ActionResult` component
  (Components/Shared) is the region: the page puts its result alerts inside and calls `FocusAfterRender()` when the
  action is done. The older pages (EPA, trainee profile, curriculum items, the MSF campaign pages) hand-roll the same
  region with `OnAfterRenderAsync`. An action that leaves the page (a create, a clone) moves no focus: the next page has
  it.
- **An action that is a page load has its result arrive with the page** (T265): a form the browser sends itself, whose
  answer is the page again. Change password posts to an endpoint that sends the browser back with what happened; Verify
  on the export check is a GET form. The region is still `ActionResult`, given `FocusOnLoad` when the page is that
  answer, done or refused: it renders `autofocus`, which a static page needs, and focuses itself once a circuit has
  rendered, since that render replaces the HTML the browser focused (after the router's `FocusOnNavigate` has put it on
  the heading). The export check sends `check=1` with the hash, so a check opened from a link, which nobody pressed for,
  moves no focus.
- **A refused action leaves the focus where it was**: on its button, still there and enabled, or in the field Enter was
  pressed in. The refusal is a `danger` `Alert`, `role="alert"`, read at once, and the operator's next step is usually to
  correct a field and press again. Where the refusal itself leaves the pressed button gone or disabled, its result takes
  the focus too (the user page's Reset password, whose password is cleared either way). A refusal shown beside its button
  (the curriculum item editor's Add and Save, the panel page's Save committee) is read there.
- The exceptions are deliberate: the trainee profile's Deactivate and Mark complete move the focus to their result done
  or refused (below); the MSF campaign page keeps it on Open after a refused open, for the retry, and puts it in the email
  field after an invitee is added ("The MSF campaign page", below); the curriculum item editor moves it to the Add form's
  empty state after the Add that took the last EPA (§ Card system); and a certificate download (My authorisations) keeps
  it on the button pressed: the page shows no result of its own, and the browser's download says what it did.
  `ActionFocusTests` (bUnit) holds each action in its `Scenarios` table to the rule (running, done, refused), and the
  pages with their own tests (EPA, trainee profile, curriculum items, panel, new activity) are held there. A new action
  gets a scenario. A page-load action is not one, having no button whose action runs in a circuit: its page's own tests
  hold it (`Account/ChangePasswordPageTests`, `Portfolio/VerifyExportPageTests`).
- A destructive action on a form page that acts on a field (the trainee profile's Deactivate and Mark complete, which
  each record the "Last day in the programme", T209 review) is an `.btn-outline` in the form's actions row whose
  `ConfirmDialog` names the value it will record, what it does that cannot be undone (encounters observed after the day
  count towards nothing, including any already counted: T281 review), and says it cannot be changed afterwards. Its result goes to the page's
  `.action-result` region, which takes the focus once it has answered, done or refused. A refusal's `Alert` has an `Id`
  that the field names with `aria-describedby` (after its help, `FieldHelp.DescribedBy`) while the refusal stands; a
  refusal of anything else on the page (Save) carries no such id, and the field does not name it. A date field bounded by
  "today" defaults to today on the South African calendar (`QuotaCalendar.Today(TimeProvider)`), never the server's
  `DateTime.Today`. It carries no `max`: the field sits inside the page's `EditForm`, and the browser would then refuse
  Save over a field Save does not send. The server's refusal is the rule.

## Table system

Two classes, two uses:

- `.clinic-table` — the canonical list table. Header uses `--header-bg`, rows separate with `--border-color`, hover uses `--hover-bg`. Wrap every table in `<div class="table-container shadow">` for the rounded surface + horizontal scroll on narrow screens.
  Every `<table>` is one, bar a data table only a screen reader reads (the trajectory chart's `.visually-hidden`), and
  carries no class app.css does not define: the entrustment decisions list was `class="data-table"` until T226, unstyled
  and 133px wider than the screen at 390px. `Design/NarrowLayoutTests` scans every page.
- `.table` — do not use. It renders as Bootstrap defaults. Delete every existing occurrence.

A list page is always:

```
PageHeader (title + subtitle + primary action slot)
search-container (search-grid with labelled .search-input fields)
table-container
  clinic-table
    thead (sticky, header-bg)
    tbody (rows)
PagerControls
```

`DataTable.razor` wraps this and takes column definitions via a `RenderFragment<TItem>`-per-column pattern. T010 builds the shell; T011 and T019 consume it.

**Editing a row in place** (T176, the curriculum items page). An item of a list, opened for editing, does not put its
controls in its own row's cells. A select squeezed into a column shows only a fragment of its label ("CPSA", "Cho…").
Instead:

- The item's row stays read-only, so what is stored stays in view. Its actions cell says "Editing below".
- Every control goes in a second row whose one cell spans every column (`colspan` = the header count). The cell holds
  a `<fieldset>` whose `<legend>` names the item ("Edit PAED-001"). Inside it: a `.form-grid` of `<FormField>`s, the
  same fields in the same order as the page's Add form, each with its `<label for>` and the same help text. Then a
  refusal `Alert` with `role="alert"`, and a `.form-actions` row holding the status that explains a disabled Save,
  Cancel, and Save. On the curriculum items page, `CurriculumItemsEditLayoutTests` holds the two forms to each other.
- Both rows carry `.is-editing`: a `--secondary-color` stripe down the left ties them together. They take no hover
  tint: neither is a list row while the edit is open, and tinting a whole form as the pointer crosses it is noise.
- The form row is as wide as the table, so Save sits at the table's right edge. **The read-only table must fit its
  container**, or Save is off-screen however the form is laid out.

**A row the caller cannot change** (T211). A list offers a row action only where the command behind it would accept it,
and a link only to a page that admits the caller and lets them act there. The page reads that from the query, never
from the caller's roles: each row carries the flag the command's own rule sets (`CurriculumItemDto.CanEdit`,
`CurriculumDto.CanEditCurriculum`, both from `CurriculumAdminScope`), and a page-level link is shown to those the target
page's policy admits (`IAuthorizationService` with that policy's name). A row with no action is not left blank: its
actions cell says why in a `.muted` span ("Set by the College"). Where whole groups of rows are read-only to the
caller, a standing `Alert` (`Role=""`) at the top says what they may and may not change there. An institution's own
item on a shared national list is marked under its title, `.muted .text-sm`, as the entrustment standing panel marks an
institution's own EPA. It names the owner to a caller whose reads span institutions ("Groote Schuur Hospital's own
item": an Administrator, T222), and says "Your institution's own item" to anyone else, every local item they read being
their own. The query decides which (`CurriculumItemDto.OwningInstitutionName`, cut by
`CurriculumAdminScope.NamesItemOwners`), never the page from the caller's roles.

**A column of actions that no row offers is not rendered** (T226). The committee agenda's Action column is the chair's
(T213), and it is shown while some line offers Stage, Defer or Reinstate (`ActionOn(line)`, the predicates the commands
demand). A decided or ratified review's lines offer nothing, bar a Defer on a line that keeps it from being ratified, so
the column was a row of blank cells there; now it is left out. A line with no action beside one that has is not left
blank either, as above: its cell says why in a `.muted` span. In progress that is a staged line, "Staged below", where
its decision is removed; on a decided review, "Fixed with the recorded decision" (T226 review). A cell's buttons sit in a
`div.actions-cell` inside the `<td>`, never `td.actions-cell`: a table cell made a flex box is no longer a table cell,
and its border no longer meets its row's. Every list page's buttons are in one since T266 (nine had `td.actions-cell`), and
every actions column's header is the `.visually-hidden` "Actions" below (26 headers on 24 pages were an empty `<th>`).
`Design/TableColumnClassTests` fails on either.

**Row actions on the curriculum items page** (T222) follow § Button system: Edit and Remove are `.btn .btn-sm
.btn-outline`, named by the item's EPA (`aria-label="Edit PAED-001"`, `"Remove PAED-001"`), and, where the row names its
owner, by whose item it is (`"Edit PAED-099 (Groote Schuur Hospital's own item)"`). Since T223 two institutions may each
hold an item of their own on one EPA, and an Administrator reads both rows, so the EPA alone no longer names one button;
the edit row's legend and the Remove result name the item the same way. Remove opens the page's one
`ConfirmDialog`, whose result takes the focus in an `.action-result` region once the dialog has closed. The dialog says
whose trainees the item stops measuring: a national item's in every institution that has adopted the curriculum, an
institution's own item only that institution's, named as the row names its owner (T222 review). **An actions
column's header is a `.visually-hidden` "Actions", never an empty `<th>`**: a header names the cells under it. A column of
buttons that fits in its content's width needs no `.col-actions`; the curriculum items table has no room for its 12rem.

**Every list's row actions name their rows** (T239). Until T239, 24 pages headed their actions column with an empty
`<th>`, and most offered a column of identical "Edit", "View" or "Open" buttons. Each row's button, and each link styled
as one, carries an `aria-label` that contains its visible label, and starts with it where the sentence allows, and names
the row in the page's words: "View the 2026 S2 review before General CCC", "Open Mini-CEX for Thandi Nkosi, PAED-003,
encounter date 2026-09-01", "Edit PAED-001 — Take a history", "Run now: msf-campaign-auto-close"; an "Up" reads "Move
field Notes up" (WCAG 2.5.3 asks that the name contain the label, so a speech-input user can say what they see). A plain
link whose own text is the row's words ("Mini-CEX") needs none. Where the list's key column is unique by rule (an
institution's, a College's or a scale's name, a job's key), the page names the row by it. Where it is not (a person's
name, a panel's, a review's period), `RowNames.Distinct` (Components/Shared) adds tie-breakers only to the rows that
would otherwise share a name, each repeating a column the row shows where it can: the email, a review's type, an
activity's state. A tie-breaker that says the same of every row sharing a name is not added to them, and rows that every
tie-breaker leaves alike are numbered in list order, "(1 of 2)", so no two names on one list are ever the same (T239
review: two drafts saved in one minute, or two panels of one name, read the same until then). `ReviewRowNames` and
`ActivityRowNames` are the committee's and the activity lists' wordings. The same holds outside tables: a user's roles
("Remove the Assessor role"), a review's staged decisions ("Remove the staged decision on PAED-003"), the builder's
sections and fields ("Move field Notes up", "Add field to section Request"), and each STAR's "Download certificate for
PAED-003". The edit row's Cancel and Save on the curriculum items page name the item too. `Design/RowActionMarkupTests`
scans every page: no empty `<th>`, no `td.actions-cell`, and no button or `.btn` link in a `Row` fragment or a `tbody`
without a name of its own, which is an `aria-label` computed from the row (a fixed "Detail" names every row alike) that
contains the visible label. A column no row offers an action in is not rendered: the entrustment scales list shows its
Edit and Delete column to an Administrator only, and the decision panels list its Edit column only to a caller who may
manage one of its panels.

**A picker offers exactly what the command it feeds accepts, and is asked again after every command that changes that**
(T195, T222). The curriculum item editor's EPA pickers leave out every EPA the curriculum already holds for the same
trainees, apart from the edited item's own, because the Add and Update commands refuse it (T223): a national item holds
its EPA for every institution, and an institution's own item holds it for that institution alone, so institution B's
picker still offers an EPA that institution A has as its own item, and the College's does not. The EPA field's help says
so, and so does the College's empty state: an EPA can be missing from its picker because an institution has it as its own
item, which the College does not read, and only that institution frees it. It names no institution and no EPA. After
each Add, Save and Remove the page asks for both pickers again (the edit row's while one is open). A choice the new
answer no longer offers moves to one it does: the Add form's to the first EPA on offer, the edit row's back to the item's
stored EPA. **A refusal reads the page again too**
(T222 review): the likeliest one comes from a page that is out of date (an EPA added, or an item removed, in another tab),
so the page reads the curriculum and both pickers again and keeps the refusal. An edit row whose item has gone closes, and
its refusal moves to the `.action-result` region, which takes the focus; a refused Add that leaves nothing to add puts its
refusal at the head of the empty state (§ Card system).

**An EPA that is not in force** is marked beside its title in a `.muted` span, on the same line: "(inactive: not in
force)" on the curriculum editor, which admins read, and "(no longer in use)", the words the activity's own EPA
picker uses (`EpaOptionLabel`), on the surfaces listed here. The shared `EpaLabel` prints it: an activity's EPA cell on
My activities and the Inbox (through `ActivityEpaLabel`, T231), the rating trajectory headings on My progress and the
committee review page, and the EPA on the admin's entrustment decisions list, its revoke confirmation and My
authorisations (T255). It renders no element of its own, so a heading's text, and its accessible name, carries the
mark. A page that names an EPA passes the flag its DTO carries (`EpaInForce`); the parameter is required. No other
surface marks it yet: the committee review page's sampling concentration list, its staged STARs (which say "No longer
fits" instead) and its evidence snapshot headings (frozen at Start) still print a bare "Code — Title".

A grid whose every row is always a set of inputs, such as a scale's levels on `EntrustmentScaleEdit`, is a different
pattern and keeps its controls in the cells. Each of them still needs an accessible name; a column header does not
give one. Name each by its column and its row, in the words the page uses: `aria-label="Label, level 3"`, and a row's
buttons `aria-label="Move level 3 up"` and `"Remove level 3"` (T198). The table is
`clinic-table clinic-table--compact clinic-table--inputs`: the inputs' own padding spaces the row, and the row's text and
buttons centre on its inputs. Its rows take no hover tint, for the reason `.is-editing` rows take none. Its ordinal is
`.col-fit`, as narrow as its content. Its buttons are `.col-actions`, whose header is a `.visually-hidden` "Actions": the
column asks for 12rem, three small buttons on one line, and gets it before the inputs grow. Where the card is narrow the
buttons wrap, and the table scrolls only once they are stacked. The inputs share the rest. Not `.col-fit` for buttons:
at 1% the column shrinks to its widest button and stacks them. And not buttons held on one line: the scale editor then
scrolled sideways, with Remove out of view, at 390px and near 700 and 1000px (T198 review).

**A narrow grid of inputs stacks its buttons** (T226). The 12rem is claimed before the inputs grow, so in a narrow table
the inputs were left their headers' width: Label 39px and Description 83px on the scale editor at 390px, from 641 to
700px and from 941 to 1000px. So the grid's `.table-container` is a size container (`container: inputs-grid /
inline-size`, selected by `:has(> .clinic-table--inputs)`), and under 44rem its buttons' column is `width: 1%` after
all, the buttons stacked, and its cells' sides pad by `--space-xs`, since the inputs' own padding spaces them. A container
query, not a media query: the card's width is not the viewport's (two thirds of the row at 1000px, all of it at 900px).

**Why 44rem.** The buttons go on one line only where each input then still has the buttons' own 12rem: Label is 201px
in a 705px table, and 191px in a 688px one. The first threshold, 36rem, put them on one line at 1280px (a 581px table)
with Label 131px, too narrow for "Indirect supervision", and narrower than stacking gives at 1100px (152px). It was also
5px from the edge: a classic 15px scrollbar made the table 571px and stacked the buttons again (T226 review). At 44rem,
1280, 1366 and 1440px stack with or without a scrollbar (tables 571 to 688px, 16px or more under), and 1536 and 1920px
keep one line (742px and up, 38px or more over).

The scale editor's Label and Description inputs, measured in headless Chrome on a static copy of the page. Headless
Chrome draws no scrollbar; with a classic 15px one the table is about 10px narrower at two-column widths and 15px at
one-column widths, and each input about 5 to 9px.

| Width | Table | Buttons | Label / Description |
|---|---|---|---|
| 390px | 306px | stacked | 63 / 102 |
| 641px | 283px | stacked | 50 / 92 |
| 700px | 342px | stacked | 84 / 117 |
| 800px | 442px | stacked | 141 / 160 |
| 1000px | 395px | stacked | 114 / 140 |
| 1280px | 581px | stacked | 220 / 220 |
| 1440px | 688px | stacked | 274 / 274 |
| 1536px | 752px | one line | 227 / 227 |
| 1920px | 1008px | one line | 355 / 355 |

The rows are three buttons high below 44rem. At 641px, the narrowest two-column width, Label is still only 50px (42px
with a scrollbar): the table's narrowest is its headers' words and one button, which now fits the 283px the card has
there, where it was 11px wider. Anything wider needs a stacked layout per level, which is a redesign.

**Many columns.** A table's narrowest width is its columns' longest words plus 2 × `--space-md` of padding per column.
Nine columns spend 288px on padding alone. The curriculum items table needed 1018px against the 907px its container has
at 1280px, and nothing in it was too wide. `.clinic-table--compact` halves the cell padding (a spanning cell keeps the
full padding, because it holds a form). That brought the table to 874px. Reach for it on a table of eight or more
columns, and on a grid of inputs.

A column of long text (a title, a list of names) is `.col-wrap`, on its header and every one of its cells. When the table
is short of room it gives way first: its lines wrap, then a long word breaks inside itself (`overflow-wrap: anywhere`),
down to a floor, and only then does the table scroll sideways. So its longest word no longer sets the table's narrowest
width; the floor does. The header sets the floor, 4rem of text (5rem with `.col-wrap--wide`). Without one the column
fell to a letter a line, 30px wide at 1024px (T198 review). The header also asks for a share of the table, 14% (20%
with `--wide`). A share, not a length: a length would hold the column there on a wide screen while every other column
grew.

That is a trade, not a free gain. A static render of the curriculum items table (the fifteen v11.1 items; T198), whose
EPA column is `--wide` and whose Tools column is not:
- Its narrowest width is 794px, down from 883px, so it has 113px to spare at 1280px (a 907px container), up from 24 in
  the same render (T176 measured 33 on the live page).
- No word breaks at 1280px, but with 5px to spare: the EPA column is 168px, and "neurodevelopmental" needs 163. Below
  about 1272px that word breaks, where the old table fitted down to about 1257px and then scrolled.
- By about 1168px both columns are at their floors (96px and 80px with padding), and many words are broken. Below that
  the table scrolls.
- At 1440px every column is within 10px of its old width. At 1920px the EPA and Tools columns are narrower than they
  were, 309px and 216px against 374px and 276px: they hold their share, and the other columns take the rest.

Measure a table at 1280px against the longest real values before adding a column.

```css
.clinic-table--compact /* cell padding --space-sm; a td[colspan] keeps --space-md (T176) */
.clinic-table--inputs  /* a grid whose every row is a set of inputs: cells centre vertically (T198) */
.is-editing            /* on a clinic-table row: the item open for editing and its form row below (T176) */
.col-wrap              /* th and td of a long-text column: overflow-wrap anywhere; the th asks for 14%, floor 4rem (T198) */
.col-wrap--wide        /* with .col-wrap on the th: asks for 20%, floor 5rem (T198) */
.col-fit               /* th and td of a column as narrow as its content, on one line: an ordinal, a code (T198) */
.col-actions           /* th and td of a row's buttons: asks for 12rem, one line of three; they wrap when short (T198) */
@container inputs-grid /* under 44rem, a .clinic-table--inputs stacks its buttons (width 1%) and pads cells by xs (T226) */
```

No inline `style="width:…"` on a table's cells. A column's width is one of the classes above.

A table **grouped by one column** puts each group in its own `<tbody>`, and the group's first row opens with
`<th scope="rowgroup" rowspan="n">` naming it; `app.css` aligns that label to the top of its group. No new class. The
committee review's evidence snapshot is one table per EPA, grouped by instrument (T167). A card that holds such a table
is an `article.detail-card--compact` whose `<h4>` it names with `aria-labelledby`.

## Form system

```css
.form-container   /* surface card with padding, border, shadow */
.form-grid        /* grid-template-columns: repeat(auto-fit, minmax(min(250px, 100%), 1fr)); gap 1.5rem */
.form-grid--wide  /* minmax(min(350px, 100%), 1fr) — wide sections that must not truncate */
.form-group       /* flex-column, gap .5rem, label on top of input; a field-level fieldset too (T188) */
.form-group > p   /* no margin: the group's gap spaces a group's help and status lines (T188) */
.form-group > .btn /* keeps its own width in the column: Add year (T188) */
dl.form-group     /* no margin on it or its <dd>s: a field shown as text to a caller who may not change it (T302) */
.full-width       /* grid-column: 1 / -1 */

.form-control     /* 38px: min-height 2.375rem, padding .375rem .75rem, border --input-border, radius --radius-md (T335) */
.form-select      /* native <select> styled with a chevron data-URI */
.form-select-sm   /* compact variant */
.form-control.invalid, .form-select.invalid, textarea.invalid, …[aria-invalid="true"]
                  /* an invalid control: --danger-color border + a 5px left stripe (T236, T335; with .input-validation-error) */

.form-actions     /* flex, justify-end, gap .75rem, padded-top, top border */
.scale-choices    /* one radio per line for a rating scale's points, lowest first (T205) */
.workflow-action-reasons /* list under a workflow action row: why a disabled action cannot be taken (T107) */
.stage-minima     /* grid: training year | rung picker (up to 18rem, floor 0, T266) | Remove, one row per year (T125) */
```

**Rules:**

- Every form is inside a `.form-container`. Every form's submit/cancel cluster is a `.form-actions` row at the bottom.
- **An auto-fit grid's column asks for its width or the whole container, whichever is less**: `minmax(min(350px,
  100%), 1fr)`, never a bare `minmax(350px, 1fr)` (T226). A bare one is 350px in a container with less room: the scale
  editor's Name input stuck out of its card at 390px, and out of the one-third column of `.details-grid` at every width
  that has two columns, and the EPA page's fields ran 48px past its form's card at 641px. The same holds for
  `.search-grid`, `.check-grid` and `.dashboard-grid`; `Design/NarrowLayoutTests` scans app.css for any other.
- `<FormField>` wraps a `<label for="…">` + input slot + `.validation-message` target. Its help text carries the id
  `FieldHelp.Id(InputId)` (`{InputId}-help`). The input in the slot is the caller's markup, which a component cannot
  change, so **a caller that gives a field `HelpText` names it on the input**:
  `aria-describedby="@FieldHelp.DescribedBy("x", helpText)"`, or `@FieldHelp.Id("x")` when the help is constant.
  `DescribedBy` lists the help first, then any other region that describes the input (a warning, a refusal), and is
  null when there is nothing, so the attribute is left off. `FieldHelp` is its own class because every page also imports
  the schema's `FormField`. A literal id passed to it is the field's own `InputId`. `FieldHelp.Id` keeps only
  characters an id list can carry (`FieldHelp.IdPart`), because an activity form's input id is a free-text field key;
  build any other id that is named in an id list from a key the same way. `FormFieldHelpTextLinkTests` scans every
  caller (T193; T205 did the MSF comment boxes).
- **Help goes in `HelpText`, never into the slot.** A bare `<small class="muted">` or `<p class="form-hint">` under the
  input is read by no screen reader. A line that does belong in the slot (the curriculum editor's "Suggested because …")
  has an id, and the control names it after its help. A group's help (a `<fieldset>` of checkboxes or rows) has an id
  the fieldset names. The same scan fails on help written into a slot (T193).
- Inputs default to `.form-control`. Selects use `.form-select` (never native unstyled).
- **An option of a `<select multiple>` rendered from data is keyed by what it offers**: `<option @key="candidate.UserId"
  value="@candidate.UserId" selected="@isSelected">` (T257). Unkeyed, Blazor reuses the option elements by position when
  the list changes (the panel form's Members list leaves out the chair), and a browser ignores a change to the `selected`
  attribute of an option the user has clicked, so the element reused for the next person shows them selected when the
  form does not hold them, and the next click sends them. `Design/MultiSelectOptionKeyTests` scans every page.
- Validation summaries render as `.validation-summary-errors` (red panel) at the top of the form. Per-field errors render as `.validation-message` under the field.
- **An invalid field shows it on the control**, not only in the message under it (T236). Blazor's inputs carry `.invalid`
  and `aria-invalid="true"` while the `EditContext` holds a message for them. A control a page marks by hand, where the
  page can tell the server will refuse what is typed (T192's encounter date, T125's rung picker, T205's MSF comment),
  carries `.input-validation-error` and `aria-invalid="true"`. On a `.form-control`, a `.form-select` or a `textarea`,
  `app.css` gives every one of them a `--danger-color` border and a 5px stripe down the left edge: the 1px border and a
  4px inset shadow, `inset 4px 0 0 var(--danger-color)` (the round-3 token sheet's; 3px until 2026-09-27, T335, flow
  01). The stripe is the cue that is not colour (WCAG 1.4.1); a shadow, not a wider border, so the text does
  not move as a field turns invalid while it is typed in. In a contrast theme (`@media (forced-colors: active)`), which
  drops shadows and paints every border one colour, the stripe is a 5px left border instead. The rule comes after
  `.form-control` and `.form-select`, which are as specific as `.input-validation-error`. A checkbox or radio is not
  marked, since a native one takes no border; its message says it. There is no field CSS class provider: Blazor's own
  class names are the ones styled, so a page marks a field by giving it a validation message, and a hand-made control by
  the two markers above, never by a style of its own. `Design/InvalidFieldStyleTests` pins the rule, and its
  `ShowsInvalid(element)` matches a rendered control against the rule's own selectors, which a page test uses to show a
  refused field is one the rule styles (the agenda deferral reason, the activity form's encounter date).
- **A field the server refused is marked, not only named** (T263). An activity form's refusal names each field by its
  label in words, and carries the fields' schema keys beside the message (`ActivityFieldsRefusedException.FieldKeys`;
  CUSTOMIZATION.md § Keys are for logic). The page hands them to `ActivityForm` as `RefusedFieldKeys`, with the id of the
  `Alert` that shows the refusal as `RefusalId`. Each control named takes the two markers above and names the alert in
  its `aria-describedby`, after its help text and any notice (`FieldHelp.DescribedBy`). A multi-choice group's fieldset
  names the alert, and its checkboxes are not marked. A locked field the refusal names is marked too, so the actor sees
  which field stopped the move. The page holds the keys with the alert and clears both on its next action, so a field
  is marked exactly while the alert that names it is shown. The three places are `/activities/new`'s refused create
  (`NewActivity.RefusalAlertId`), a refused move on the activity's page (`ActivityView.ActionRefusalAlertId`) and the
  notice of a submit refused straight after the create (`ActivityView.NoticeAlertId`, the keys riding on
  `ActivityNotice.RefusedFieldKeys`). `Activities/RefusedFieldMarkingTests` holds all three.
- Multi-step forms get `<fieldset>` with a styled `<legend>` — both reset in the CSS.
- Checkbox: `<div class="form-check">` wrapping a `.form-check-input` + `<label>`.
- A group of checkboxes is a `<fieldset>` with a `<legend>`, the checkboxes inside a `.check-grid` (columns of
  `.form-check`), every checkbox with its own unique id. **Not** a `<FormField>`: its `<label for>` would point at
  no single input. When two groups share a page (an inline edit row and an Add form), prefix their ids differently
  (`edit-tool-{key}` / `add-tool-{key}`, T122's curriculum tool list).
- **A fieldset that is one field is a `fieldset.form-group`, wherever it is** (T177 in `ActivityForm`, every page since
  T188): a group of checkboxes or radios, and a field with no control to name (the activity form's file placeholder,
  an unsupported type). `.form-group` makes its legend read as that field's label, like the labels beside it, not as a
  section title. It sits straight in the `.form-grid` among the `<FormField>`s, `full-width` when its options need the
  row, never inside a wrapper: the MSF campaign form's EPAs (T147), the curriculum item forms' Minimum by training year
  (`StageMinimaEditor`, whose own fieldset is the grid item, T125) and Tools (T122), the committee review's Evidence it
  rests on (T131) and Present (T165), the activity form's multi-choice, and the MSF questionnaire's scale (T205). A
  required group carries the mark a required `<FormField>` does, in its legend (`*` hidden from a screen reader, then
  a visually hidden "required"). Help text goes inside the fieldset, which names it with `aria-describedby`, and is a
  `.page-subtitle`, the size of a `<FormField>`'s help: a `<p class="page-subtitle">` under the legend, or the activity
  form's `<small class="page-subtitle">` after the options. A status line in the group ("Select a trainee first.") is
  one too; a `.field-warning` stays a warning. The group's gap spaces its parts, so a paragraph in it takes no margin,
  and a button in it keeps its own width. A group inside such a field (the evidence picker's lines by EPA) is a form
  group too, and is set in by a 2px rule down its left, because its legend is the same size as the field's. Its legend
  is floated, so it is laid out inside the group and the rule runs its full height (a rendered legend straddles the
  top border, and the rule began halfway down it); a floated legend still names its fieldset. A fieldset that
  **holds** fields (a `.form-grid` or `<FormField>`s: an activity form's section, the curriculum item's edit row, a
  deferral) is a cluster, and keeps the section-size legend. `Design/FieldGroupTests` reads every `<fieldset` in every
  `.razor` file, the render fragments in `@code` included, and fails on a field whose class does not write
  `form-group` out (one an expression adds is sometimes not there), and on a paragraph in a field that does not write
  `page-subtitle` (or `field-warning`) out.
- An option is shown by its label, never by the key it stores (T191): `<option value="picu">PICU</option>`, and a
  checkbox's id is built from the key, checking it stores the key, and its `<label>` shows the words. A schema option
  is a bare string only when the string reads as words ("1"); a key is written
  `{ "value": "admission_notes", "label": "Admission notes" }`, and the builder's Options box takes it as
  `admission_notes | Admission notes`. The box holds one option per line, and a comma is part of the option, never a
  separator. The portfolio export prints the label too, except on a scale, where the rung the College prints comes
  first (T100). A field that stores an id prints as what the id names, never as the id (T199,
  `PortfolioFieldReferences`): an `epa` field as its picker labels it ("PAED-003 — Title", "(no longer in use)" after
  one not in force), a `user` field as the person's name (never their address; "Unknown person" when the account is
  gone), and a campaign's evidence record's `campaign_id` as the MSF section of the same export heads that campaign,
  "Annual MSF (Campaign #3)".
- A rating scale answered by choosing one point is a `fieldset.form-group` whose `<legend>` is the question (with the
  required `*`), holding a `.scale-choices` list of `.form-check` rows: one radio each, lowest point first, each with its
  own id and `<label for>`, a point's description as a `<small>` in its label (T205, the MSF questionnaire). A list, not
  a `<select>`: every point's label stays in view. Not a `.check-grid`: an ordered scale reads down, not in columns.
- Sensitive inputs (password, passphrase): wrap in `.password-wrapper` and use `PasswordToggleButton.razor` to show/hide.
  The toggle is a 28px target inside the field's right end, placed from the wrapper's foot, so it is centred on the 38px
  field even where the wrapper also holds the label (the sign-in, registration and link pages); the wrapper is as tall
  as what it holds, even as a grid's item. So the field must be the last thing in the wrapper (2026-09-27, T335, flow 01;
  T328 found the toggle 23px and on the field's top edge).
- **A field the caller may read but not change is text, not a control** (T302). Where the command behind a field
  refuses the caller, the form does not offer its control (§ Table system, T211: read from the policy the command's
  rule is, with `IAuthorizationService`). The field stays in the grid as a `dl.form-group`: its `<dt>` where a label
  stands, the stored value in a `<dd>`, and a `.muted .text-sm` `<dd>` saying who sets it ("Set by a global
  administrator."). The institution page shows an InstitutionalAdmin its Status so; an Administrator gets the Active box,
  and Deactivate behind a `ConfirmDialog` (§ Button system).
- A workflow action the actor may take but cannot complete (T107) is a **disabled** button, never a hidden one. Its
  reason is visible text in a `.workflow-action-reasons` list below the row, starting with the action's name, and the
  button points at it with `aria-describedby`. A tooltip alone is not enough.
- An action that can be taken but has a consequence worth knowing first (Start review while MSF campaigns from the
  window are unreleased, T173) says so in a `<span class="muted">` beside the button in its `.form-actions` row, and
  the button points at it with `aria-describedby`. Say it at the button even when a notice elsewhere on the page says
  it too: on a narrow screen the notice can be several cards away.

## Card system

```css
.detail-card                  /* surface, padding var(--space-lg), border+shadow */
.detail-card--compact         /* padding var(--space-md) */
.detail-card--header          /* padded header strip only */
.detail-card--interactive     /* hover: translateY(-2px), bigger shadow, cursor pointer */
.detail-card--empty           /* dashed border, centred muted text, var(--space-xl) */
.detail-card--empty-compact   /* dashed border, centred, var(--space-md) */
.detail-card--empty-compact[tabindex]:focus-visible /* focus ring, for an empty state the page moves the focus to (T222) */
.detail-card--emphasis        /* left 4px solid accent stripe, --secondary-color */
.detail-card--warning         /* left 4px solid accent stripe, --warning-color */
```

- `.detail-card h3` has a bottom border and primary colour — gives the card a titled strip.
- Cards are the building block of dashboards and detail pages. Anywhere you want to group fields inside a page, reach for a card.
- The empty-state card uses a dashed border so it reads as "nothing yet" at a glance.
- **A form with nothing left to offer is replaced by its empty state** (T222). When a form's one required picker would
  be empty (the curriculum item editor's Add form, once every EPA the item could name is already on the curriculum), the
  form is not rendered with an empty select and a live submit button. Under the form's heading, a
  `div.detail-card.detail-card--empty-compact` says that nothing is left, in the words of what the form would have made,
  and what would have to change for there to be something. It has `tabindex="-1"`: when the action that emptied the
  picker took its own button away with the form (the last Add), the page moves the focus to it in `OnAfterRenderAsync`,
  and it starts with that action's result ("Curriculum item added.", or a refusal in `.text-danger`), so the focus reads
  the result out where the operator is. A status alert that arrives already filled just as the focus moves may not be
  read at all (T222 review). The page's usual result alert still shows at the top.

## Dashboard layout grid

> **Amended 2026-09-27, T335, flow 01:** three counted tracks, not auto-fit (the round-2 review, S22(b)); a card's title is
> an `<h2>`, and its rows are `.list-row` (T328).

Dashboards use one shared grid so every role page looks like the same product.

```css
.dashboard-grid {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));   /* three tracks from 1100px */
  gap: var(--space-lg);
  align-items: start;
}
.dashboard-grid > * { min-width: 0; }                  /* a card's content never widens its track */

.dashboard-span-2 { grid-column: span 2; }
.dashboard-span-3 { grid-column: 1 / -1; }             /* the whole row, at every width */

@media (max-width: 1099.98px) { .dashboard-grid { grid-template-columns: repeat(2, minmax(0, 1fr)); } }
@media (max-width: 900px)     { .dashboard-grid { grid-template-columns: minmax(0, 1fr); }
                                .dashboard-span-2, .dashboard-span-3 { grid-column: auto; } }
```

**Three tracks at 1280px.** Until T335 the grid was `repeat(auto-fit, minmax(min(320px, 100%), 1fr))`. Three 320px
tracks and two gaps need 1,008px, and at 1280px the content column is 974px (1280 less the 250px sidebar and the article's
32px and 24px sides): Home had two tracks there, and a card spanning three added a third the grid had no room for. So the
tracks are counted: three from 1100px (a 794px column, 249px tracks), two from 901px to 1099px, and one at 900px and
below. A card spanning three takes the whole row at every width, so it never adds a track. `Design/NarrowLayoutTests`
pins the three rules.

Dashboard widget classes (added in T011):

```css
.dashboard-metric         /* flex column, gap xs */
.dashboard-metric-value   /* 2rem bold, --primary-color */
.dashboard-metric-label   /* 0.9rem, --muted-text */

.progress-bar             /* 0.5rem tall, rounded, --hover-bg background */
.progress-bar-fill        /* fills parent height, --secondary-color */
.progress-bar-fill.is-complete  /* --success-color */

.progress-row             /* one titled figure + bar + meta line; margin-bottom sm (T130) */
.progress-row-head        /* flex, space-between, baseline: label left, "n of m" right */
.progress-row-meta        /* muted, 0.85rem, margin-top xs: the explanatory line under a figure */
.progress-group-title     /* heading that splits a page of progress cards into groups ("Each semester", "Once a year") */
```

`.progress-row-*` was promoted in T130 from the inline `display:flex;justify-content:space-between` row that had spread
across six progress surfaces, past the four-dashboard threshold below. Use it for any "label · n of m · bar" figure.
Give every `.progress-bar` `role="progressbar"` with `aria-valuemin`, `aria-valuemax`, `aria-valuenow` and an
`aria-label` that states the figure in words. A bar is decoration without them.

Curriculum progress figures (T130) are always **a count against a target for a named window**: "2 of 3 this semester",
"1 of 1 in 2026", "4 of 9 trainees met". Never a lifetime total, and never a mean percentage across trainees. Say
"training year N" for `TraineeProfile.GetStage` and "Semester S, YYYY" / "YYYY academic year" for a quota window:
the two are different concepts (D17), and the bare word "year" beside both is ambiguous. When a target is waived
under the College's D14 rule, show the count and the date targets start, never a fraction and bar. When the programme's
end waives it (D49, T209: it ended in the window before the window's last month, or the window is after the end), show
the count and why ("the programme ended part-way through", "after the programme ended"), never a fraction, a bar, "short"
or "targets start with": the targets did not start later, they stopped.

The three staff dashboards' target cards (CommitteeMember, SpecialityAdmin and SubSpecialityAdmin, one
`EpaTargetCoverageList`) count the same trainees: the current trainees in the caller's scope
(`TraineeScopeResolver.KeepCurrentAsync`, T238), an active profile on an account that still holds Trainee and that an
administrator has not locked (T268). So an erased trainee's pseudonym, a profile that outlived its Trainee role and a
locked trainee are in no "n of m", and the admins' "Trainees in programme" tile counts as active the trainees their card
reads. None of them is counted as inactive either; the admin trainees list is where such a profile is seen, and ended.

Each dashboard card is a `<DashboardCard>` — a shared component that wraps `.detail-card` and adds `Title`, `Icon` (Lucide name), `Href` (turns it into `.detail-card--interactive`), `Emphasis` / `Warning` (left stripe variants), and `Span` (1/2/3, the `.dashboard-span-*` modifiers). Reach for `<DashboardCard>` first; drop to raw `<div class="detail-card">` only when the card does not have a titled strip. At 900px and below the `.dashboard-grid` is a single column.

- **Its title is an `<h2 class="dashboard-card-title">`** (2026-09-27, T335, flow 01): the icon, the words and, where the
  card counts something, the count as a `badge-submitted` badge (`Count`: "Waiting for your rating 2",
  R2-Landing-Assessor). Home's `<h1>` is the page's one; until T335 the cards' `<h3>` skipped a level.
- **`IsLoading`** draws the card as its title and a skeleton (`.dashboard-card-skeleton`, three lines, `aria-hidden`),
  and not its content. A card that links is not a link while it loads, and a count is not shown: nothing is offered
  before the read returns (R2-Landing-Loading).
- **A row of a card's list is `li.list-row`**: flex, space-between, baseline, a small gap and `--space-xs` above and below;
  a badge keeps its pill. A row of buttons is `.actions-cell`, and figures side by side `.dashboard-metric-row`. Until
  T335 (T328) each dashboard wrote its rows as inline `display:flex` styles; `WaitingCardsTests` finds none left.

T011 mandates this grid for every role dashboard — do not hand-roll a different one per role.

## Builder layout

T019 introduces a small builder-specific extension to the shared system:

```css
.tab-bar          /* horizontal tab strip with pill-like tab buttons */
.tab-bar-tab      /* inactive tab button */
.tab-bar-tab.is-active

.builder-two-col  /* admin builder split view: editor rail + live preview */
```

- `.tab-bar` sits directly below `PageHeader` inside the surrounding `.form-container`.
- `.builder-two-col` is `grid-template-columns: minmax(320px, 1fr) minmax(400px, 1.4fr)` with the normal `var(--space-lg)`
  gap, and collapses to one column by the width of **its card**, not the viewport's (T266). The card, the `.form-container`
  whose direct child it is, is a size container (`container: builder / inline-size`, selected by
  `:has(> .builder-two-col)`), and under 46.5rem of it, the two columns' own minimum (320 + 400 + 24px), the columns stack.
  The sidebar, the gutter and the card's padding take 372px of the viewport, so the viewport's 900px said nothing: until
  T266 the page scrolled sideways by 158px at 901px, 109px at 950px and 59px at 1000px, and up to 1115px the columns ran
  into the card's padding. Now they stack up to 1115px and stand side by side from 1116px (a 744px card). Each column's
  content keeps to its track (`min-width: 0`). `Design/NarrowLayoutTests` pins the rules, and
  `ActivityTypeBuilderLayoutTests` the columns' place as the card's child.
- The left column uses stacked `.detail-card` sections and field rows. The right column is always the live preview rendered by the shared `ActivityForm.razor`.
- New builder affordances still reuse the existing button, card, form, alert, and validation classes. The builder does not get its own parallel design language.
- **The builder follows the rule its commands refuse by** (T300, § Table system's T211 paragraph). The editor carries
  `CanWrite` and `WritableScopes` (`ActivityTypeAdminScope`: an Administrator writes every type, a Global type is
  theirs alone, an Institution type its InstitutionalAdmin's, a Speciality or SubSpeciality type the owning College's
  CollegeAdmin's). The Scope picker lists exactly `WritableScopes` with no empty target, and a new type starts on the
  first of them (Institution with her own institution, for an InstitutionalAdmin). Where `CanWrite` is false the page
  is a reader's: no Save draft, Discard draft or Publish, no Add, Up, Down or Delete; the metadata, the form settings,
  a section's and a field's settings, the workflow and the credit rules are text (`dl.form-group`, the JSON in a
  `pre.code-block`); each section and field offers View ("View field Overall level"), so a field can still be read; and a
  standing `Alert` (`Kind="info"`, `Role=""`, `#activity-type-read-only`) at the top says whose type it is ("Set by the
  College that owns Paediatrics.") and that the reader can read it but not change it. The header offers nothing to save
  until the editor has loaded, and nothing after it failed to; its title comes from the address, so a page with an id
  never reads "New activity type". The list offers Edit where `CanWrite`, View elsewhere, both to the builder, and New
  activity type only where `ActivityTypeAdminListDto.CanCreate`. `ActivityTypeBuilderAccessTests` and
  `ActivityTypesListAccessTests` hold both pages to it.

## Alerts, validation, empty states

> **Amended 2026-09-27, T335, flow 01:** an alert's words are body text on its tint, with the kind's colour on the edge
> and the icon (T322); a field's refusal carries the danger icon, and the validation summary reads as a danger alert. An
> alert may carry its own action (`.alert-row`), and a load error offers the read again.

```css
.alert                       /* body text (--text-color) on the tint; 1px edge in the kind's colour, 4px down the left;
                                padding .75rem 1rem .75rem 3rem, radius --radius-md, margin-bottom 1.5rem */
.alert::before               /* the kind's Lucide icon, 20px on the first line, 1rem in: a mask filled with the kind's colour */
.alert-success               /* --success-bg, edge and icon --success-color, circle-check */
.alert-info                  /* --info-bg, edge and icon --secondary-color, info */
.alert-warning               /* --warning-bg, edge and icon --warning-color, triangle-alert */
.alert-danger                /* --danger-bg, edge and icon --danger-color, circle-alert */
.alert-row                   /* inside an alert: its words, then its own action at the right (flex, wrap, centred) */
.alert-row-text              /* the words: flex 1 1 16rem, so the action wraps under them where they need the width */

.validation-message          /* a field's refusal: --danger-color (5.95:1 on white), .875rem, the circle-alert icon
                                (16px, in the text's own colour) hung in a 1.375rem left padding */
.validation-summary-errors   /* Blazor's <ValidationSummary>: read as a danger alert (body text, the stripe, the icon);
                                its <li class="validation-message">s take its colour and no icon of their own */
.input-validation-error      /* a hand-marked invalid input: --danger-color border + 5px left stripe, as Blazor's .invalid (§ Form system, T236) */
.field-warning               /* inline NON-blocking warning under a field, input accepted as typed: --warning-bg, --warning-color left stripe, body text, .85rem (T160 late filing) */
```

- **A semantic colour is never an alert's words** (T322). Until 2026-09-27 each alert was its kind's colour on its own
  tint: success 2.55:1, warning 2.42:1, danger 3.57:1. Now every alert is `--text-color` on the tint (11.2:1 or more),
  and the kind is told three ways: the tint, the edge (3:1 or more, a 4px stripe down the left) and the icon.
  `Design/ContrastTests` holds each kind's rule to that.
- **The icon is the stylesheet's, not the markup's.** `app.css` draws it as a `::before` masked with an inline Lucide
  glyph (a quoted SVG data URI, § Icons) and filled with the kind's colour, so every `<Alert>` has it and no page writes
  one. The words keep a 3rem left padding, clear of it. In a contrast theme the mask is filled with `CanvasText`, as the
  theme would otherwise paint it the colour of the ground.
- **A field's refusal has the icon too**, so a refusal is not told by colour alone. It hangs in the message's left
  padding, so a message that wraps keeps its edge.

- `<Alert>` is announced by its kind unless the caller names a `Role` (T193). `danger` is `role="alert"`, so a refusal
  that appears after Submit is read at once; `warning` and `success` are `role="status"`, because nearly every one
  reports what an action did; `info` has none. Name one where the kind's default is wrong:
  - a refusal shown as a warning is `Role="alert"` (the MSF page's refused link; the activity page's refused-submit
    notice, which arrives with the page already filled, where a `status` is often not read);
  - a warning or success that is **standing page content**, there on every visit (a banner, "credited nothing", a
    suppressed category), is `Role=""`, which renders none, or it is read out each time the page finishes loading.
  - Never hand-write `<div class="alert …">`: it skips the default.
- **A refusal is shown in its own words** (T213). A page puts a caught exception in its error `Alert` through
  `RefusalText.Of(exception)` (Components/Shared), never `exception.Message` directly: a command's validator refuses
  with a FluentValidation `ValidationException`, whose message is written for a log ("Validation failed: -- Members: …
  Severity: Error"), and the helper shows each failure's message once instead. Every other refusal's message is a
  sentence already and is shown as it is. The committee pages use it throughout; other pages still print
  `exception.Message` and move to it when next touched.
- **An alert already on the page when it loads is not reliably announced**, whatever its role. When a page reloads with
  a refusal and puts focus in a field (sign-in, link account), give the `Alert` an `Id` and have the field name it with
  `aria-describedby`, so the refusal is read with the field. A refusal that names fields gives its `Alert` an `Id` the
  same way, and every field it names points at it (the activity pages, T263, § Form system).
- **An alert that carries its own action** lays its words and the action out in `.alert-row`: the words
  (`.alert-row-text`) take the room, and the button or link sits at the right, wrapping under the words where they need
  the width (2026-09-27, T335, flow 01). Home's load error and its Try again, and the switch's result and its Switch back
  (R2-Landing-Error, R2-Landing-Assessor, R2-Detail-Email).
- **A load error says that nothing changed, and offers the read again** (2026-09-27, T335, flow 01; R2-Landing-Error;
  T329). When the read a page is built on fails, the page shows one `danger` alert with fixed words, the failure's own
  text going to the log, and a Try again that reads again, and draws nothing the read would have filled: no empty state,
  no skeleton, no card. Home's is "**Could not load your Home.** Nothing has changed. Try again, or come back in a few
  minutes." (§ Dashboard page). The wording of a database failure on other pages is T272's.
- `StatePanel.razor` renders three canonical states: loading (skeletons), error (`.alert .alert-danger`), empty (`.detail-card--empty` + optional CTA).
- Every list page handles all three states explicitly. **No more "Loading…" plain text** — that pattern is dead. The
  entrustment decisions list and an audit entry were its last uses, until T266; `PageShapeSmokeTests` scans every page
  for it.
- A field's warning and its predicted refusal never show together. When the page can tell the server will refuse what is
  typed (an encounter date before the trainee's programme started, T192), the field says so as a `.validation-message`
  in place of any `.field-warning`, whose "can still be filed" would contradict it. Both sit in one `role="status"`
  region under the field, present before anything is typed, which the input names with `aria-describedby`, after its
  help text (`FieldHelp.DescribedBy`, § Form system). While the refusal is predicted the input also carries
  `aria-invalid="true"` and `.input-validation-error`; a warning alone marks nothing, since what it warns of is
  accepted. It is a hint: the server's refusal stays the rule. A refusal the page did not predict counts the same
  (T263): while a refusal on show names the field, the `.field-warning` is left out, since the page's start date was
  missing or out of date and the alert says why the date was not accepted.

## Skeleton loaders

```css
.skeleton {
  background: linear-gradient(90deg, var(--hover-bg), var(--border-color), var(--hover-bg));
  background-size: 200% 100%;
  animation: skeleton-pulse 1.2s ease-in-out infinite;
  border-radius: var(--radius-sm);
  min-height: 1rem;
}

@media (prefers-reduced-motion: reduce) {
  .skeleton { animation: none; background: var(--header-bg); }
}

@keyframes skeleton-pulse {
  0%   { background-position: 200% 0; }
  100% { background-position: -200% 0; }
}
```

`Skeleton.razor` renders `<div class="skeleton" style="width:@Width;height:@Height">`. Dashboards and list pages render a handful of skeletons while `IScopedSender.Send(...)` resolves. The viewport should not shift when real data lands.

**Reduced motion** (2026-09-27, T335, flow 01; the round-2 review, S17): with the reader's "reduce motion" setting the
skeleton does not pulse; it is a still `--header-bg` block, the same size (§ Accessibility).

## Badges

> **Amended 2026-09-27, T335, flow 01:** five tints, body text on each and the tint's colour on the edge (T322); a pill
> keeps its shape in a flex row (T328).

```css
.badge           /* inline-flex pill: 0.75rem / 600, line height 1.5, padding 1px 10px, --radius-pill, 1px edge,
                    --text-color words, white-space nowrap; flex: none and align-self: center in a flex row */
.badge-draft     /* neutral: --header-bg ground, --input-border edge */
.badge-submitted /* info: --info-bg ground, --secondary-color edge */
.badge-accepted  /* warning: --warning-bg ground, --warning-color edge */
.badge-completed /* success: --success-bg ground, --success-color edge */
.badge-declined  /* danger: --danger-bg ground, --danger-color edge */
```

- **A badge's words are body text on its tint**, 11.2:1 or more (T322). Until 2026-09-27 a state badge was its state's
  colour on its own tint: Completed 2.55:1, Accepted 2.42:1, Declined 3.57:1. T166's standing badges had already taken
  body text for that reason; now every badge does, and a standing badge is the state badge of its tint.
- **A pill keeps its shape in a flex row** (T328). On the dashboards' Recent activities and Recent decisions, a badge
  beside a link that wrapped took the row's height, a tall pill: a flex item stretches by default. `align-self: center`
  and `flex: none` keep it one line tall and its own width, and `white-space: nowrap` keeps its words on one line.

Used on activity state indicators in dashboard list cards and activity tables. The state picks the class and the text is
its label (T220, T266): `<span class="badge @BadgeFor.ActivityState(item.CurrentState, item.IsFinished)">@item.CurrentStateLabel</span>`
renders `<span class="badge badge-submitted">Awaiting supervisor</span>`.

**One helper names every badge class** (T266): `BadgeFor` (Components/Shared). A page writes
`class="badge @BadgeFor.…(…)"` and never a `badge-` class of its own; `Design/DefinedClassTests` fails on one named
anywhere else (written out, glued to an expression, or interpolated: `$"badge-{key}"`), and holds every value each
`BadgeFor` method can return to app.css; `Design/BadgeForStatusTableTests` pins each status's tint below, one row per
enum member, so a swapped tint or a new status fails. Until T266 eight pages wore `badge-success`, `-danger`,
`-warning`, `-info` and `-primary`, none of them defined, and the dashboards `badge-{state key}`, tinted for five of the
fifteen keys the seeded workflows use.

- `BadgeFor.State(BadgeState)` is the five: `Draft` grey, `Submitted` blue, `Accepted` amber, `Completed` green,
  `Declined` red.
- `BadgeFor.ActivityState(key, isFinished)` is the dashboards' activity states. Done is green, and done is a terminal
  state of the activity's **pinned** workflow (`ActivityCompletion`, D44), which the dashboard queries send as
  `IsFinished`; never a key's name. `teaching_session` finishes in `accepted`, which on a Mini-CEX is a supervisor's work
  in hand: by the key alone a finished teaching session was amber on the assessor's Recent decisions (T266 review).
  `declined`, `rejected` and `cancelled` are red, even where a version makes them terminal. Of the rest, `submitted` and
  `requested` (waiting on a supervisor) are blue and `accepted` amber. Any other key, a draft or a state an institution's
  own workflow names, is grey: the badge's words, the state's label, say what it is, and grey claims nothing. The trainee's
  inbox holds no finished work, so it passes `false`.
- `AgendaLine`, `AgendaElsewhere`, `DecisionDue`, `MsfCampaign` and `EntrustmentDecision` are the tables in this file.
- `DataRightsRequest`: submitted blue, under review amber, approved and completed green, rejected red, withdrawn grey.
  `JobRun`: running amber, succeeded green, failed red. `AuditResult`: done green, failed red.
- `Standing` is the comparison badges below.
- A category or a type (the audit log's category, a data rights request's type) is not a state, so it is not a badge: its
  words stand alone, as on the requester's own data rights page.

A status that is not an activity state is mapped onto these five in C#, never written as `badge-{status}`: app.css
defines no other. The entrustment decisions list did that, and all four of its STAR statuses were untinted pills (T226
review). There, Active is `badge-completed`, Expired `badge-accepted` (the EPA wants deciding again), Revoked
`badge-declined` and Superseded `badge-draft`.

```css
.badge-standing-met    /* success: as .badge-completed (T166) */
.badge-standing-below  /* warning: as .badge-accepted */
.badge-standing-none   /* neutral: as .badge-draft; no decision, or not comparable */
```

A verdict of a level against a target: `EntrustmentStandingPanel`'s "At or above", "Below", "No decision" and "Not
comparable" (T166). The words are the verdict and the tint repeats it. Every badge above is a state; these are the only
ones that are a comparison. They keep their own class names, so a verdict and a state stay apart in the markup, though
since T335 each is painted as the state badge of its tint.

**Entrustment standing** (T166). `Components/Shared/EntrustmentStandingPanel.razor` is the one rendering of
`GetEntrustmentStandingForTraineeQuery`. The committee review page shows it as a full-width card (`.detail-card
.full-width`) after its first pair of cards. The trainee's My progress page shows it as a section with its own `<h2>`,
and says it to "you" (`Self`). It opens with a sentence naming the training year and a `details-list` of two lines:
the year-target counts, and the exit rule counted by exit level with the EPAs short of it named. Then a
`.clinic-table--compact` of six columns: EPA, year target, STAR decision, against target, exit level, latest rating.
A level on another ladder, or on an unpinned item, is "Not comparable" with the reason under it, never a verdict. The
panel says the exit rule gates nothing, directly under the rule. Each page loads the panel in its own `try`, so a
failure shows in the panel and never replaces the page.

What the panel must not claim. On the review page the year is read for the review period's last day once that has
passed (`ReviewPeriodTo`), and the opening sentence says so and that decisions are today's: a review held after its
period judges that period's year, not the next one. On the trainee's own page once their programme has ended, the year is
read for its last day (`ProgrammeEnd`), and the sentence says the programme ended, or was completed, then, in that year,
rather than "you are in" a year the start date would put them in today (T252). A profile ended before Wombat recorded the
day (T209) is read on today, and the sentence says the day was not recorded and the year is counted from the start date
to today, never "you are in" it. A target the curriculum sets no level for (no per-stage map, or a
year past the ones it names) is the exit level standing in, and its cell says "Exit level; no year N level set" rather
than passing it off as Annexure A's. An institution's own EPA is a row, marked under its title, but the exit rule is
the College's and counts only the College's EPAs. For someone else, "nothing to show" does not say "no curriculum":
the same null is what a caller outside the trainee's oversight gets. The exit rule is one sentence built in C#, because
Razor drops a space standing alone before an expression and ran the sentences together.

**Multi-source feedback coverage** (T168). `Components/Shared/MsfCoveragePanel.razor` renders
`GetMsfCoverageForTraineeQuery` on the committee review page. It is a full-width card directly after the standing card,
wherever that sits, so the pair of cards below keeps its pair. It has three parts:

- An opening built as one C# string. It says what covered means, that MSF counts towards no target, and that it is read
  live beside a frozen snapshot, in the review's own terms: on a Scheduled review, that nothing is frozen yet; in
  progress, that a campaign released since Start is not in the snapshot; once decided (or a formative review closed),
  that the card may differ from what the panel saw. Only when the review period starts or ends inside a semester does
  it add that a semester is read whole.
- A `details-list` giving each semester's "n of m EPAs covered".
- A `.clinic-table--compact` with each EPA as a row header (`th scope="row"`) and one column per semester. A cell says
  "Covered", with the campaign and the day it closed, or "None released" in muted text ("None released yet" while the
  semester runs).

It takes no badge and no warning tint. Coverage is not a verdict against a target, and there is no shortfall to show
until Annexure B's cadence is confirmed. The trainee's My progress page says the same thing as one line per quota card,
newest semester first, and gives this semester's count in the "This period" card. Both pages use the words
`MsfCoverageText` builds. Every sentence says a released campaign *covering* the EPA closed in the semester, never a
campaign "about" it (D9). An uncovered semester that has ended is never worded as final ("…that closed in the semester
has been released"): a campaign is placed by the day it closed, so one closed in June and released in July covers
semester 1 only from its release.

**My progress once the programme has ended** (T252). A trainee with no current programme, completed or withdrawn, sees
the one they ended on, read as on its last day, never "No curriculum items assigned yet". The page opens with an
`<Alert Kind="info">` (standing content, so no role) saying that the programme ended and on which day, "You completed
your programme on …" or "Your programme ended on …", that the page is their record and read-only, and D49's rule, as the
PDF states it. The "This period" card becomes "Your programme": started, completed or ended, and the training year it
ended in. Each quota card keeps its heading and target line, and lists every period in a `details-list`, newest first,
from the one it ended in back to the start: "Semester 2, 2026: no target (your programme ended part-way through) · 1
recorded", "Semester 1, 2026: 3 of 3, met; 2 at the minimum level when observed". A period not yet closed by today
(`QuotaWindowDto.HasClosedBy`, through 31 December for semester 2, D40) is "2 of 3 so far", or "3 of 3 so far, met",
never "more by": nothing is owed. There is no bar, no D14 alert and no December notice. The period lines are the
portfolio PDF's (`PeriodLine`, which reads the same closing rule), in the page's "your", and each card keeps its MSF
line; a coverage read that fails says so above the cards, as on the running view. The opening sentence ("You completed
your programme on …") is `QuotaText.ProgrammeEnded`, which the standing panel and the trainee dashboard's Curriculum
targets card also use; that card says the programme ended and points here.

**The programme's MSF coverage** (T210, `/msf/coverage`, Coordinator and Administrator, as the campaign pages). Per
programme, EPA and semester, how many of the programme's trainees were covered: "1 of 3 trainees covered". A programme
is a curriculum as one institution follows it (`MsfProgrammeCoverageText.ProgrammeName`, "Paediatric EPA Curriculum 11.1
at Demo Institution"), so an Administrator sees one per institution. The trainees are those the caller may read about
(`TraineeScopeResolver.ReadableAsync`, the set form of T113's ladder) who are on the programme now: current trainees
(`TraineeScopeResolver.WhichAreCurrentAsync`, T238), an active profile on an account that still holds Trainee and that
an administrator has not locked (T268), so an erased trainee's pseudonym, a profile that outlived its Trainee role and a
locked trainee are not counted. The semesters are today's and the one before, as on the trainee's progress page. Each
trainee is counted from their own card: the counts are read by `MsfSemesterCoverage`, the one rule
`GetMsfCoverageForTraineeQuery` reads too. The page holds:

- A `.muted` opening (`#msf-coverage-intro`, one C# string): what covered means in D9's words, that each trainee is
  counted from their own progress page, that MSF counts towards no target so an uncovered trainee is no shortfall, that a
  trainee counts in a semester once their programme has started by its last day, and that an ended semester can still
  gain a campaign released later.
- One `section.detail-card` a programme, `aria-labelledby` its `<h2>`. A `details-list` gives each semester's trainees
  ("3 trainees, whose programme had started by 30 June 2026"). Then two tables, each under an `<h3>` ("By EPA", "By
  trainee"), each in a `.table-container` that is `tabindex="0"` and `role="region"`, as the decisions-due summary is,
  since neither holds anything focusable. Its `aria-labelledby` names its `<h3>` and then the programme's `<h2>`, so that
  where several programmes render (an Administrator's view, or an institution running two curricula) each region is
  named for whose it is ("By EPA Paediatric EPA Curriculum 11.1 at Demo Institution"), not one of several "By EPA"s.
  Both are `.clinic-table--compact` with a row header per row and a column per semester.
  - **By EPA**: "n of m trainees covered", bold when n is above 0 and muted when it is 0; "No trainee had started" where
    m is 0, never "0 of 0". An institution's own EPA is marked under its title, as on the trainee's grid.
  - **By trainee**: "6 of 15 EPAs covered", the trainee's own count for the semester (for the current one, the count
    their My progress "This period" line gives), or "Not started".
- Like the trainee's grid it takes no badge, no warning tint and no bar: nothing here is a verdict against a target.
- Someone who holds Trainee (T185's rung) reads no other trainee's record: the page opens with a standing `Alert`
  (`Kind="warning"`, `Role=""`, `#msf-coverage-trainee`) saying so and that their own coverage is on My progress, and asks
  for nothing. Nobody to show is the `StatePanel`'s empty card; a failed read is its error.

**The MSF campaign page** (T217, `/msf/campaigns/{id}`). Its campaign card shows the campaign as it is now and offers
only what its state allows. It is read again after every action on the page, refused or not: a refusal is likeliest
because the campaign changed elsewhere. The Quick template card is only on the create page, `/msf/campaigns/new`: a
campaign that exists already has its questionnaire, and its page reads no template list (T225). The card holds:

- A `details-list`: Trainee (by name), Template (with its kind), State (a badge) and Response window. Then one sentence
  saying what the state means and what comes next: an open campaign names its respondents' last day to respond and says
  that Close campaign is on its report. It says each respondent "is emailed" a link, never "has been emailed": the mail
  leaves after the open, and whether it arrived is what the next lines say (T251).
- **What became of the links, on an open campaign, counted and never whose** (T251). The mail worker reports each link's
  mail sent or dropped onto its invitation, and a link nothing was heard of for an hour (the host crashed, or stopped
  mid-send) counts as dropped (`MsfInvitation.LinkNotDelivered`, `LinkBeingSent`). Links not delivered are a `warning` `Alert`
  (`#msf-links-not-delivered`, `Role=""`: standing content, there on every visit until they are resent) reading "2 links
  were not delivered. Resend sends each of these respondents a new link; this page never says who they are."
  (`MsfCampaignText.LinksNotDelivered`). Links still being sent are a `.muted` line (`#msf-links-being-sent`) that says
  how many and to reload the page to see whether they arrived. A number, never a row or an address, for the reason the
  invitees are counted by group: an undelivered respondent is one who has not answered. Nothing is shown when every link
  was sent, and nothing on any other state.
- **Invitees, counted by respondent group, never listed once the campaign has opened.** An `<h4>`, a `.muted` line
  saying the page never lists who was invited or which of them responded ("Once the campaign opens, …" on a draft), and
  promising nothing more: a campaign whose category threshold is one shows a one-person group's answers on its report.
  Then a `.clinic-table` with a row header per group (by its label, "Peer doctor", never the key) and Invited, then
  Responded once the campaign has opened, with an "All groups" `tfoot` row when there are two groups or more. Not a row
  per invitee, even without its address: the coordinator added the rows and knows which is whom, so a responded mark on
  one row names the author of the comment that has just appeared on the report. A group's count adds little to the
  report, which already shows the total and a card for each group that has responded. Until release it does add the
  exact count of a group below the category threshold, which the report hides; that names nobody, and with one such group
  it is already the total less the others. (T217 review)
- **A draft's addresses, for Remove** (T247). On a draft that invites anyone, an `<h4>` "Addresses invited", a `.muted`
  line saying they are listed only while the campaign is a draft, and a `.clinic-table` with a row per invitee in the
  order they were added: the address as the row header (`.col-wrap`, so a long one wraps rather than widening the
  table), the group by its label, the teaching context on learner feedback, and a `.visually-hidden` "Actions" header
  over each row's Remove (`.btn .btn-sm .btn-outline`, `aria-label` "Remove peer-1@example.test (Peer doctor)"). This is
  not the row per invitee the counts refuse to be: a draft's invitees hold no working link and have given no response,
  so a row says nothing about anyone's answers. It does show the addresses to whoever runs the campaign, any coordinator
  at the trainee's institution, not only the one who typed them; that is accepted (T247 review). The query lists them
  only while the campaign row is a draft (`MsfCampaignSetupDto.DraftInvitees`), and the page asks the state too; once
  the campaign opens, the list is gone. Remove opens the page's second `ConfirmDialog`, which names the invitee, says
  that they hold no working link and will not be sent one, and that they can be added again while the campaign is a
  draft. "No working link", never "no link": an open that failed or was refused at its save may have mailed one. Its result, done
  (`Role="status"`) or refused (`Role="alert"`), takes the focus in the `.action-result` region once the dialog has
  closed: the row's button is gone after a remove, and after a refusal the campaign is read again and may have opened
  elsewhere. The counts and the list are read again with it, so a group loses the invitee at once, and removing the only
  invitee disables Open campaign with its reason. While a remove is in flight, Open, Withdraw campaign, Add invitee and
  every Remove are disabled (the focus is in the dialog), and a second confirm sends nothing.
- The invitee form, only on a draft.
- One `.form-actions` row, with nothing a state does not allow: a draft has Withdraw campaign and Open campaign; an
  open campaign has Withdraw campaign and a "View report" link, where it is closed, and, first, "Resend 2 links"
  (`.btn .btn-outline`, `#msf-resend-links`) while any link was not delivered, whose `aria-describedby` names the
  warning above (T251); a closed one under review has Withdraw campaign and a "Review and release" link; a released one
  "View report"; a withdrawn one has no row. Withdraw is where the campaign list offers it (T206): on every campaign not
  yet released (`MsfCampaignRules.IsWithdrawable`, which the command asks too). Withdrawing a closed campaign is the
  decision never to release its report, which whoever may release it may take (T199; until then a campaign nobody would
  release stayed under review for good). Both pages use the same `ConfirmDialog` wording (`MsfCampaignText`). For a
  closed campaign, the dialog and the result say that its report will never be released. They do not mention links or
  addresses, which closing already removed, and its state note ends "If it should never be released, withdraw it here."
  The command is sent the state the dialog was worded by (`WithdrawMsfCampaignCommand.ConfirmedState`), and refuses a
  withdraw confirmed while the page showed the campaign as a draft or open once it has closed
  (`MsfCampaignRules.WithdrawingForgoesRelease`, `WithdrawMsfCampaignCommandHandler.ClosedSinceShown`): a page loaded
  before the auto-close job ran would otherwise withdraw, never to be released, a campaign whose coordinator was asked
  only to stop its links (T199 review). Both pages read the campaign again after the refusal, so the next dialog asks in
  the closed campaign's words. On a draft that invites nobody,
  Open campaign is shown **disabled** with its reason (the T107 pattern above, T225): no click handler, and
  `aria-describedby="msf-open-reason"`, a `.workflow-action-reasons` list below the row reading "Open campaign: add at
  least one invitee first…" (`MsfCampaignText.OpenNeedsInvitees`). The handler's refusal of a campaign with no invitee
  stays the rule; until T225 the button was enabled and the refusal came only after the press.
- An action's result is an `Alert` in an `.action-result` region that takes the focus once it has answered, whenever
  the button that sent it is gone: after an open or a withdraw, and after an invitee add refused because the campaign
  was opened or withdrawn elsewhere (its form is gone). A refused open on a campaign that is still a draft leaves the
  focus on Open, for the retry; an add on a draft leaves it in the form. An open reports "Campaign opened; links are
  being sent." (`MsfCampaignText.CampaignOpened`, T251): the mail server answers after the request, so the open cannot say
  the links arrived, and its "could not all be sent" refusal is only for a mail the queue would not take. A resend reports
  "2 new links are being sent." and moves the focus to its result (its button is gone once no link awaits resending); a
  refused resend that leaves the button (a hand-off that failed) leaves the focus on it for the retry, and one that finds
  the campaign closed elsewhere moves the focus to the refusal, beside the campaign read again. Resend is never disabled
  by its own resend (T234): it carries `aria-disabled` and reads "Resending links…" while it runs, and a second press
  sends nothing; Withdraw campaign is disabled then. An address the campaign already invites, in any
  capitals, is refused the same way (T228): a `role="alert"` refusal naming no address, with the address left typed to
  correct and nobody counted. While an open is in flight, Withdraw campaign and Add invitee are disabled, and Open reads
  "Opening campaign…" but stays enabled and a second click on it sends nothing (T225 review): it holds the focus, and a
  browser drops the focus of a button it disables, to the page (observed in Chrome), so a refused open would otherwise
  leave the focus nowhere. While a withdraw is in flight, Open, Withdraw campaign and Add invitee are disabled (the focus
  is in the dialog). While an invitee add is in flight,
  Open and Withdraw campaign are disabled and a second Enter or click on Add invitee sends nothing (T228 review); Add
  invitee itself stays enabled, because it may hold the focus that a refusal on a draft leaves in place. A refusal is kept
  when the campaign cannot then be read again: the card says it could not be loaded, and the refusal says why the action
  was not taken.

Nobody runs a campaign about themselves, and nobody who holds Trainee runs one at all (T224, T185's trainee rung;
`MsfCampaignRules.IsKeptFromCampaignsAbout`). The create form's Trainee picker never offers the caller. It offers
exactly the trainees the create accepts (`ListMsfCampaignSubjectsQuery`, T238): the current trainees at the caller's
institution, each once, by the rule committee scheduling reads (an active profile on an account that still holds Trainee
and that an administrator has not locked, T268). So it offers no graduate, no trainee who has withdrawn or moved away,
no profile that outlived its Trainee role, no trainee whose account an administrator has locked, and no erased trainee's
pseudonym, and a crafted create for any of them is refused before anything is written, in the words a trainee elsewhere
gets (an Administrator is told "…only be run for a trainee in a programme now…"). Adding an invitee to a draft and
opening it are new work too, so each asks the same of the campaign's trainee (`MsfCampaignRules.EnsureCampaignTakesNewWorkAsync`,
T284): once the trainee is no longer current, Add invitee and Open campaign are refused, after the scope check and before
anything is written or mailed, with the "…only be run for a trainee in a programme now…" refusal, said plainly to a
coordinator too since they already know whom the campaign is about. A draft about them can still lose an invitee or be
withdrawn. A campaign already open stays one its coordinator can resend, close, release or withdraw after its trainee's
programme ends, and its respondents are still reminded. The picker and the create ask one
predicate (`MsfCampaignRules.MayStartCampaignAbout`, T248), and the page lists no trainee on a campaign's own page,
which has no create form. The campaign page answers a campaign about the caller as it answers an id that names nothing
(its "Campaign unavailable" card), the campaign list leaves those campaigns out, and the report is theirs only as the
released trainee's copy, on their own page (T269, below).

Someone who holds Trainee is told so, whatever role brought them there (`MsfCampaignRules.RunsNoCampaigns`; § A row
the caller cannot change). The campaign list and the campaign page open with a standing `Alert` (`Kind="warning"`,
`Role=""`, `#msf-runs-no-campaigns`) in the words a create refusal gives (`MsfCampaignRules.TraineeRunsNoCampaigns`).
The list then offers no "New campaign" and no empty card, and asks for nothing. The campaign page shows no create card
and lists no trainee; on a campaign's own page the alert sits above its "Campaign unavailable" card. The create page
shows them no Quick template card either, and reads no template list: creating a template is running campaigns, and the
template command refuses them in the alert's words (`MsfCampaignRules.EnsureRunsCampaigns`, T248; until then the card
stayed, since a template is about no trainee, and the command took no caller to refuse). A Coordinator at no
institution (an Administrator can add the role to such an account) runs no campaign either
(`MsfCampaignRules.RunsCampaigns`): the campaign page gives them the same standing alert in the words the create and
template commands refuse them in (`MsfCampaignRules.RunsCampaignsRoles`), and neither form (T248 review). The MSF
campaigns nav link stays, as Committee reviews and Decisions due stay for the same people (T185); the page it opens
now says why nothing is there. (T224 review)

**The invitations list** (T283, `/admin/invitations`). Each active invitation's row says what became of the email
carrying its current link, in a `.col-wrap` "Delivery" column before the actions: "Sent"; "Being sent" (a `.muted`
span: nothing is wrong yet); or "**Not delivered.**", when the mail worker gave up on it or nothing was heard of it for
an hour (`Invitation.DeliveryOf`, the same hour as an MSF link's). From the second failed email of one invitation
(`Invitation.FailuresBeforeAddressCheck`) the cell goes on to say that the mail server may be refusing the address, to
check it, and to revoke and issue again if it is wrong: an address is never corrected in place, since the invitation
names who may register (`InvitationText.CheckAddress`). A row not delivered then ends "Resend emails a new link in place
of the current one, which then stops working." (`InvitationText.ResendRetiresTheLink`, T283 review): the link alert
tells whoever issued it to share the link another way, and the row goes on offering Resend, so anyone about to press it
is told first that it would stop a shared link working. The cell's text is a `span` with an id
(`#invitation-delivery-{id}`), and it is the text, not a badge: the words are the state, and nothing on the row is a
verdict to tint.
- A row not delivered offers **Resend** (`.btn .btn-sm .btn-outline`) before Revoke, both in a `div.actions-cell`. It is
  named by the row as Revoke is ("Resend the Coordinator invitation to a@example.test", T239), and its `aria-describedby`
  names the row's Delivery text, so a screen reader hears why it is offered and what it will do. No other row offers it.
- Resend emails a new link and retires the old one (`ResendInvitationCommand`). Its result is the issue's: a `success`
  `Alert`, "A new invitation link is being emailed to …. The link it replaces no longer works. Copy the new link below —
  it is shown only once.", and the `info` `Alert` holding the link, both in the page's `ActionResult`, which takes the
  focus (the row now reads "Being sent" and offers no Resend). The link's alert says the link is also being emailed and
  that the Delivery column says whether it arrived (`InvitationText.LinkIsBeingEmailed`); until T283 it said email
  delivery was "configured separately".
- The list is read again after every resend, refused or not: a refusal is likeliest because the invitation changed
  elsewhere (resent, used, revoked, or delivered after all), and its refusal says so. A refused resend leaves the focus
  on Resend while the row still offers it, for the retry, and moves it to the refusal once the row no longer does.
- Resend is never disabled by its own resend (T234): it carries `aria-disabled` while it runs, and a second press sends
  nothing. The issue form's submit and every other row's Resend and Revoke are disabled then, as they are while a
  revoke runs. Every refusal on the page is shown through `RefusalText.Of`.

**The MSF campaign list's row links** (T225, `/msf/campaigns`) are named by what their page lets the coordinator do in
the row's state, in the campaign page's words (`MsfCampaignText.CampaignLinkLabel`, `ReportLinkLabel`), never "Edit" in
every state:

| Campaign state | Campaign page link | Report link |
|---|---|---|
| Draft | Manage | none |
| Open | Manage | View report |
| Closed, Under review | View campaign | Review and release |
| Released | View campaign | View report |
| Withdrawn | View campaign | none |

A draft has no responses to report and a withdrawn campaign is never released, so neither row links to a report, as
the campaign page offers none there. Each link's `aria-label` starts with its visible label and names the row
("Manage: the campaign for Sipho Dlamini (Annual MSF, closing 2029-03-21)", `MsfCampaignText.RowLinkName`), as the row's
Withdraw button does (T206).

**The MSF report page** (`/msf/reports/{id}`) names each respondent group by its label ("Peer doctor", never
"PeerDoctor"), as its card headings, as the trainee's copy (`/msf/my-reports`) and as the portfolio PDF print it
(`MsfRespondentCategories.Describe`, T225). Close campaign and Release to trainee put their result in an
`.action-result` region above the report, which takes the focus when the button that acted is gone, as on the campaign
page; a load failure is the `StatePanel`'s, including a read that fails after an action that was taken, and a refusal
never replaces the report. Neither can be sent twice at once, and neither is disabled while it runs (T225 review, as Add
invitee): the page refuses the second press, and the button pressed keeps the focus, so a refusal that leaves it (a
close refused on a campaign still open, a release refused on one still ready) leaves the focus on it for the retry.
Release to trainee is disabled only on a report not ready for release; a refused release that finds it no longer ready
moves the focus to the result. The warning that says why (a campaign under review short of responses or groups) is
standing content (`Role=""`) and ends "If it should never be released, withdraw it on its campaign page.", the last
words a link to `/msf/campaigns/{id}` (`#msf-withdraw-elsewhere`), where Withdraw is (T199 review). A release whose save
a withdraw from another tab beat is refused in `ReleaseMsfCampaignCommandHandler.CampaignChanged`'s words, never EF's
row counts; the report read again shows the campaign withdrawn, with no form (T199 review). After a refused close or release the report is read again, keeping the narrative and
level typed, so a refusal names no other page: "If it is still open, close it again."
(`CloseMsfCampaignCommandHandler.CampaignChanged`, worded as T217's open and withdraw refusals). Only an open campaign
closes (T246): a report loaded while the campaign was open still offers Close after another tab or the auto-close job
has closed it, and that close is refused in the campaign's words ("Only open campaigns can be closed.",
`MsfCampaign.OnlyOpenCanBeClosed`), as the campaign page's refused open is (T217). Its close date stands, since it
places the campaign's semester, committee window and evidence date. The report read again shows it as it is now, Close
is gone, and the refusal takes the focus. A refused close or release is shown through `RefusalText.Of`. The Coordinator
actions card holds the narrative and level fields only while the campaign can still be closed or released (Open, Under
review). Otherwise (released, withdrawn, a draft) it shows them as stored, as `<p><strong>Narrative:</strong> …</p>` and
the level by its rung's label ("Not given", "Not stated" when empty), with no form (T246 review): a tab that typed a
narrative and then found the campaign released elsewhere kept its own text in an editable box beside "State: Released",
where the narrative the trainee was given belonged.

The campaign's own subject never reads the report on this page (T269). A trainee who also coordinates, or an
Administrator who is the subject, is admitted to it by role, and the handler admits the subject to the released report.
So the page sends them to their own copy, `/msf/my-reports/{id}`, replacing its address in the history so that Back does
not return to it, and renders nothing of the report meanwhile, only the `StatePanel`'s skeleton. That is the navigation
inside a circuit. A page load (the address bar, a bookmark, a link from outside the app) is the page's prerender, where
the same `NavigateTo` is the server's redirect, a 302 to `/msf/my-reports/{id}` with nothing of the report in it, because
`Wombat.Web.csproj` sets `BlazorDisableThrowNavigationException`. No page links the subject here (the campaign list and
the campaign page leave out a campaign about the caller), so the page load is the usual way in; `MsfReportPageFlowTests`
covers it and `SubjectReadsOwnReportTests` the circuit. Whether a copy is the caller's is the handler's answer
(`MsfCampaignAggregateReportDto.IsSubjectsCopy`), the same answer that leaves the teaching contexts unnamed (T164). The
trainee's page shows only a copy marked that way, so neither page names a context to the subject. The flag says which
page shows a copy, never what a copy may name: the portfolio PDF's copy is not marked, and the trainee reads it. The
trainee's page admits Coordinator as well as Trainee and Administrator, so that every role this page admits can open the
page it is sent to. The nav still offers MSF reports to Trainee alone.

The state's words are `MsfCampaignText.State` ("Under review", never the enum's "UnderReview"), which the campaign list
and the report print too. Its badge is `BadgeFor.MsfCampaign`:

| Campaign state | Badge |
|---|---|
| Draft | `badge-draft` |
| Open | `badge-submitted` |
| Closed, Under review | `badge-accepted` |
| Released | `badge-completed` |
| Withdrawn | `badge-declined` |

**The trainee's copies of a released report** (`/msf/my-reports` and the portfolio PDF's feedback section,
`MsfSectionComponent`) print nothing of a respondent group below the category threshold: not its label, its answers or
its own count (T249). The PDF used to print "Nurse: 1 responses" above "Below minimum threshold", an exact count under a
group's name, which is what the threshold hides. When the PDF leaves any group out, it prints one line, however many
were left out: "Respondent groups with fewer than N responses are not shown, to protect the respondents' anonymity." N
is the campaign's category threshold. The line explains why the printed groups add up to less than the total, and it
names none of them. A count of one is singular: "1 response", "(1 rating)", and every number prints the same on any
host (invariant culture, so an export is the same bytes, T078).

The two copies differ in one respect. The web copy prints the total (in its list) and no group's count. The PDF prints
the total and each printed group's count ("Consultant: 3 responses"), and a required rating's "(3 ratings)" is the same
figure. So the total less the printed counts is the hidden groups' count together, and with one hidden group it is that
group's own; if the template allows only one other group, it also says which. That says at most who took part, never
what anyone answered, and it is accepted, as the campaign page accepts it for the coordinator (T217 review above) (T249
review). Dropping the group and rating counts from the PDF would match the web copy exactly; that is the operator's
call, not made here, since T249 asked for the counts to be kept and pluralised. The coordinator's report page is not
changed: it still shows a suppressed group's card, without its answers.

**Committee agenda** (T131 slice 4). The committee review page's Agenda card is full width, directly above the
"Pending entrustment decisions" card, so a chair reads what is due and stages it in one place. A formative review has
no Agenda card. The card holds:

- A muted sentence naming the period ("2026 S1") and saying that a line marked Due must be staged or deferred before
  ratify.
- On a review in progress, the EPAs routed to this panel and due whose window lost its decision while the review sat,
  and which the agenda does not hold (T235): an EPA Start left off because a STAR decided it, whose STAR has since been
  revoked. They are named in one sentence of their own in a warning `Alert` (`#agenda-no-longer-decided`) directly
  under the opening sentence, above the table: "PAED-003 is no longer decided in its window: the STAR that decided it
  has been revoked. It is not on this agenda; stage a decision on it to decide it at this sitting." (plural: "PAED-003
  and PAED-006 are no longer decided in their windows: the STARs that decided them have been revoked. They are not on
  this agenda; stage a decision on each to decide it at this sitting."). The alert stands on the page until the chair
  acts, so it takes `Role=""` and is not read out on every load. The chair gets one `.btn-outline.btn-sm` per EPA in an
  `.actions-cell` row under the sentence, "Stage PAED-003", named by its own text (no `aria-label`, so the name holds the
  visible words), which chooses the EPA in the staging form and moves the focus there, as a line's Stage does; staging it adds the chair's line, and
  the EPA is named no longer. Anyone else reads the sentence alone. The EPA is never added to the agenda by itself: the
  agenda was planned at Start and the decision recorded at this sitting settles it (D46). The sentence is not shown
  before Start, which plans the EPA, nor once the decision is recorded, when nothing more can be staged and the
  decisions-due page reads it as "Revoked: re-decide".
- A `.clinic-table` with five columns: EPA (the row header), window ("2026 S1" for a semester, "2026" for an annual EPA),
  state, evidence count, and action. The action column is the chair's alone (T213): anyone else reads four columns, not
  an empty fifth. The caption counts the lines and the ones still to stage or defer. The state cell
  holds the badge and, under it, a `.muted` line saying what the state means for this sitting: the deferral's reason,
  the STAR a decided line names, or that an optional line is optional and why.
- A line whose window another sitting has decided since it was planned reads "Decided elsewhere" (T235): a late
  semester-1 sitting ratifying an annual EPA that the semester-2 review holds as closing. Whether the window is decided is
  `CommitteeAgendaStatus.IsDecided`, the decisions-due page's and the planner's one predicate, read on the line's own
  window whenever the agenda is read and when the decision is recorded and ratified. Such a line is optional: it is not
  counted in the caption, and neither Record nor Ratify is disabled for it. While it is still due its muted line says
  "Another sitting has decided it in this window, so it need not be decided here." On a review in progress, the one
  state that takes a staged decision, it adds "A decision staged on it decides it again.", and the line offers Stage and
  Defer as any line still due does. On a decided review awaiting ratify it adds "Ratifying the review records that.",
  and on a scheduled review nothing. A review whose trainee has moved to another institution reads none of its lines
  this way, since that would say what the new institution decided (T182); a speciality panel that no longer covers the
  trainee at the same institution still does. Recording the decision settles it
  (`DecidedElsewhere` is stored, D46), and so does ratify for one decided since; its muted line then says "Another sitting
  had decided it in this window when this review settled its agenda." A settled line stays so if the other sitting's
  STAR is later revoked: the decisions-due page shows that, and the next sitting plans the EPA again. The trainee's own
  page says "Decided at another sitting in this window."
- Each line offers only what its state allows. A Due line that is not staged offers Stage and Defer. Stage chooses the
  EPA in the staging form below and moves the focus there. A deferred line offers Reinstate. The others offer nothing.
  All of it only while the review is in progress: recording the decision fixes the agenda with the staged STARs (T165).
  On a decided review the one action left is Defer on a closing line that blocks Ratify, which happens only when the
  STAR staged on it was removed afterwards because it no longer fits the trainee's curriculum.
- A deferral opens a `<fieldset>` form under the table, with its `<legend>` naming the EPA ("Defer PAED-002"), a
  required reason, and Cancel and Defer in a `.form-actions` row. The focus moves to the reason once the form is
  rendered, as Stage moves it to the EPA. The trainee sees the reason once the review is ratified (below). An empty
  reason is refused in the form with "Say why the committee is deferring the decision." as a `.validation-message` under
  the box (T212). Its region (`#deferral-reason-message`) is there before anything is refused, and the box names it with
  `aria-describedby` after its help. A refused submit moves the focus back to the box, so the message is read with it.
- The EPAs routed to this panel that a STAR already decided in their window, so the planner left them off, as one
  `.muted` sentence (`#agenda-decided-in-window`) in the scheduling preview's words: "Already decided in this window, so
  not on the agenda: PAED-001 and PAED-006." (T215). A STAR decides its window whether or not an agenda line records it,
  by the decisions-due page's rule. An EPA the review holds a line for is not named: its line says what this sitting did
  with it, and the decisions-due page says where the decision stands now. An empty agenda on a scheduled review says
  that Start adds every EPA due in its period and not already decided in its window.
- The EPAs another panel sitting as a College committee decides for the period ("Decided by another panel"), as a
  read-only list: the panel, the window, and a badge. While one is not yet decided, the progression Decision card shows
  a `.field-warning` above Record decision. That is a warning, never a refusal (O8).

The badges. Each says the state in words, and the three kinds of "still due" share one badge and differ by label:

| State | Badge |
|---|---|
| Due, Due by year end, Partial period, As opportunity allows | `badge-draft` |
| Staged | `badge-submitted` |
| Decided, Decided elsewhere | `badge-completed` |
| Deferred | `badge-accepted` |
| Not decided | `badge-declined` |

Another panel's decision uses the same badges: Not yet decided is `badge-draft`, On the agenda of an open review is
`badge-submitted`, Decided is `badge-completed`, Deferred is `badge-accepted`, and Missed is `badge-declined`. "Missed"
is computed when the page is read, never stored. It means the window has ended with nothing decided, deferred, or on an
open review's agenda. An EPA decided as opportunity allows, or in a partial period, is never missed.

Record decision is a disabled button while a closing line is neither staged, deferred nor decided elsewhere (the T107
pattern above), and so is Ratify. Each one's `.workflow-action-reasons` line names the lines, in the refusal's own words
(`CommitteeAgendaDto.RatifyBlockedReason`). Where more than one reason stands, the line gives the first the handler
would refuse with: the panel's seats before the agenda for Record, the decision's quorum before the agenda for Ratify.
The agenda is read again after every action on the page. A failure is a warning `Alert` in the card, never red beside
the action's success, and the agenda last read stays in view. Every sentence is built in C# (`CommitteeAgendaText`),
which the scheduling form's preview shares.

The "Pending entrustment decisions" list is read again after every action that can change it (T212): Stage, Remove, and
the review's own actions (Start, Record, Ratify, Close, Resolve). Ratifying issues the staged decisions as STARs and
clears the list, which the page used to go on showing until a reload. Defer and Reinstate do not read it: a line with a
decision staged on it cannot be deferred, and neither touches a staged row. A failed read is a warning `Alert` in that
card, as the agenda's is, and the list last read stays in view. An empty list says why it is empty
(`#no-pending-note`). Before the review is ratified it says that nothing has been staged. Once it is ratified, the
sentence depends on the agenda. With a line ratifying decided, it says that ratifying issued what was staged as STARs
and the agenda names each one; staging always leaves the EPA a line, so that is exact. With none, which ratify allows
when every line was optional or deferred (or a progression review's agenda is empty), it says that nothing was staged when the review was ratified, so it issued no
STAR. Where the agenda could not be read again straight after Ratify, the lines staged in the agenda last read count as
issued, since ratifying issues every staged decision.

**The chair's controls** (T213). The review page offers each control to exactly the people its handler lets use it,
in exactly the states its handler takes it, by what `GetCommitteeReviewByIdQuery` says the caller may do:
`CallerChairs`, `CallerResolvesAppeals` and `CallerMayStart`, computed by the predicates the handlers demand
(`CommitteeDecisionAuthorization.Chairs`, `ResolvesAppeals` and `WorksOnReview`), and `TraineeElsewhere`, the trainee check
every chair's action and Start also demand (`CommitteeTraineeScope`). Start is offered on a scheduled review, and only
then, to a member of its panel who may sit on it now (`PanelSeat.SittingAt`, T279: an active committee member at the
panel's institution who is not a trainee, and never the trainee under review), a coordinator of the panel's institution
or an Administrator (T194); an institutional, speciality or sub-speciality administrator who reads the review without a
seat (T218), or through their role while their seat no longer counts, is not offered it, and `#chair-actions-note` reads
"Only the coordinators of the panel's institution, and those of its members who are active committee members there, can
start this review." A member who may no longer sit (lost the CommitteeMember role, moved institution, deactivated, given
Trainee) reads the review through their seat no more than they start it (T279): the page, its sibling reads and the
committee reviews list refuse or leave it out, with the one refusal. None of the three
flags is ever true for someone who holds Trainee, whatever seat or role they hold beside it (T185's rung, asked by
`WorksOnReview` and by the seat predicates since the T194 review): a trainee seated on the panel that reviews them before
they held Trainee reads their own review once it is ratified, and is offered neither the chair's controls nor the
resolve form on it. Since T237 no one who holds Trainee is seated at all (see **The panel form**). The chair alone is
offered the decision form and Record decision (while the review is in progress), Ratify and its reason (once decided),
Close review, the staging form, Remove, and the agenda's Stage, Defer and Reinstate, and only while the chair may sit at
the review now (`PanelSeat.SittingAt`, T237's one rule, which `CommitteeDecisionAuthorization.Chairs` asks and every
chair's action demands, T256): an active committee member at the panel's institution who is not a trainee, and never the
trainee under review. A chair who has since lost the CommitteeMember role, moved institution, been deactivated or been
given Trainee is offered none of them. In their place, while a chair's action is open or to come (scheduled, in progress
or decided), a standing warning `Alert` with no
role (`#chair-cannot-act-note`, the query's `ChairCannotAct`) says why: to the chair, the sentence each click would be
refused with ("You chair this panel but cannot take the chair's actions now: only an active committee member at the
panel's institution who is not a trainee can, and never the trainee under review. A panel administrator must seat a
chair who can."); to every other reader, the same naming the chair ("The panel's chair, Thandi Zulu, cannot take the
chair's actions now: …"). `#chair-actions-note` then says nothing about the chair's actions, which it would name the
chair as able to take; on a scheduled review it still says who can start it. The appeal body alone (the chair or an external member) is offered the
resolve-appeal form, and only while the review is under appeal: a form is never shown with empty fields and no button.
It is offered only to a member of the appeal body who may sit on the panel now (`PanelSeat.AppealBodyAt`, the list the
note below names, T237), which the resolve handler demands too; a chair or external member who has since lost the
CommitteeMember role, moved institution or been deactivated reads the note instead, which does not name them.
These are hidden from everyone else, not disabled: T107's disabled button is for an action the reader may take but
cannot complete yet, and nobody else may take these. Instead a `.muted` sentence in the Review card
(`#chair-actions-note`) names the chair and what only the chair can do in the review's state ("Only the panel's chair,
Thandi Zulu, can ratify the committee's decision."; on a decided review it also names removing a staged decision that no
longer fits and deferring a line that keeps the review from being ratified, where either is open), and under appeal the
Appeals card names who resolves it (`#appeal-body-note`: "Only the appeal body can resolve the appeal: the panel's
chair, Thandi Zulu, and its external member, Anna Botha."). It names only those who can act, the query's `AppealBody`
(T237): the chair and external members who may sit on the panel now (`PanelSeat`), never someone who holds Trainee, and
the same for every reader, the trainee on their own appeal included. A chair who cannot act is left out ("…: the
panel's external member, Anna Botha."), and where no one can, it says so: "Only the appeal body, the panel's chair or an
external member, can resolve the appeal, and none of them can act now. A panel administrator must seat a chair who
can." A Decision card with no decision and no form says "No
decision has been recorded yet." (`#no-decision-note`) rather than stand as a bare heading. When the review's trainee no
longer trains at the panel's institution, Start and the chair's controls are offered to nobody (a global Administrator
excepted, whom the check does not refuse), no `#chair-actions-note` is shown, and a warning `Alert` with no role
(`#trainee-elsewhere-note`) says what those actions would refuse with. What the review holds (its agenda, staged
decisions, evidence and decisions) is shown to every reader as before. The remit form ticks and locks the chair and the
caller, who is the resolver because only the appeal body is offered the form. A staged decision that names no evidence
(only a row written some other way can) says, while the review is in progress, that the chair can remove it and stage it
again; once the decision is recorded it says what the ratify refusal says, that it is fixed and an administrator must
look into it (D46).

**A refusal never says whether an id exists** (T194). Every committee command and every read of a review authorises
before it looks at anything else, and gives an id that names nothing the same sentence as one out of the caller's reach,
before any state check. Each gate has one sentence, which the page prints as it is (`RefusalText.Of`): "The committee
review could not be found among the reviews you can view." (the review page and its sibling reads, a trainee's own review
before ratification included, and since T279 a member whose seat no longer counts), "…you can start." (the same
member, T279), "…you chair." (every chair's action), "…whose appeals you resolve.",
and "…among your own ratified reviews." (lodging an appeal). A panel id gets its own gate's sentence ("You can only
manage panels in your institution.", or the scheduling refusal). Only who may act at all is said before the lookup,
because it says nothing about the id: "Only trainees can lodge appeals.", "You are not allowed to manage committee
panels.", "You hold the Trainee role, so you cannot create or change a decision panel, including one that reviews you."
(T256, to someone who holds Trainee beside a role that manages panels), "Only an institutional administrator can say
which College committee a panel sits as.", or T216's scheduling refusals. The seat refusals come after the one refusal,
and only to the seat's holder who can read the review: "You sit on this panel's appeal body but cannot resolve its
appeals now: …" (T237) and "You chair this panel but cannot take the chair's actions now: …" (T256). Since T279 a seat
whose holder may not sit no longer lets them read the review, so such a holder who reads it through no other role (a
coordinator of the panel's institution, say) is given the one refusal: the seat's sentence would tell them that the id
names a review of their panel. The review page's `#chair-cannot-act-note` is said to the chair only where they can read
it.

**The trainee's own reviews** (`/committee/my-reviews`). The list's Period column names the period the review sat for,
then its evidence window in a `.muted` span, as the schedule does: "2026 S2 · 2026-01-01 to 2026-12-31" (T212). The
window alone read the same for a year's two sittings, since a semester-2 sitting's window is the whole year. The detail
says the same on two lines, "Sits for" and "Evidence window", as the committee's review page does. A ratified review's
detail shows its agenda read-only, as an
`article.detail-card--compact` headed "Agenda": a muted sentence naming the period, then a `.clinic-table` of three
columns, EPA (the row header), window and outcome. The outcome cell holds the same badge as the chair's table and, under
it, a `.muted` line: the committee's reason for a deferral (O6), the STAR a decided line names, or that it was not
decided at this review. No actions. Nothing is shown before ratify; the read ladder gives a trainee nothing sooner.

**Scheduling a review.** The form asks for the period the review sits for: a select of semesters around today, labelled
with the period itself, "2026 S2 · 1 Jul to 31 Dec 2026". Choosing one fills the evidence window, which stays editable,
from the start of the period's academic year to the period's end: 2026-01-01 to 2026-12-31 for 2026 S2, because the
annual EPAs a semester-2 sitting closes are judged on the whole year. Under the fields, a full-width section previews
the agenda. It says how many EPAs will be on it and which must be decided, lists the optional ones with their status,
says which EPAs another panel decides ("schedule them separately"), and says which are already decided in the window.
Only the opening sentences sit in a `role="status"` region (`#agenda-preview-summary`), so a screen reader hears what a
new choice changed, not every line again. Where a STAR already decided every EPA due, the opening sentence says so
itself ("Every EPA this panel decides that is due for 2026 S2 is already decided in its window…"), not that none is due,
since the note naming them is outside the live region (T215). The Trainee select offers exactly the trainees the
scheduling handler would accept on the chosen panel (T182), and the Panel select offers exactly the panels on which that
list is not empty (`ListDecisionPanelsQuery` with `ForScheduling`, T194): not a panel the caller sits on at another
institution, not a speciality panel of a speciality they do not administer, and not a panel whose only trainee in reach
is not a current trainee. The Trainee select, the Panel select and the handler all read one rule for who that is
(`TraineeScopeResolver.ResolveCurrentAsync` and its set form, T238): an active profile on an account that still holds
Trainee and that an administrator has not locked (T268; a lockout after wrong passwords, which lifts itself, does not
count). So an erased trainee's pseudonym, a trainee whose programme has ended, one who no longer holds Trainee and one
whose account an administrator has locked are offered nowhere, and a typed or crafted request for one gets the
scheduling refusal before anything is written (an Administrator is told "Only a trainee in a programme now can be put
before a panel…"). When it offers none, its help text says so: "No panel has a trainee you can schedule a review for. A
panel is listed once a trainee it reviews is in a programme you oversee."

**The panel form** (`/committee/panels/new`, T194). A new panel's Scope and Speciality selects offer what creating the
panel would accept from the caller, read from `GetDecisionPanelFormOptionsQuery`, never from roles: an institutional
administrator both scopes and the specialities their institution has adopted a curriculum in; a speciality or
sub-speciality administrator the Speciality scope and only those of their own specialities their institution has adopted
(T245); an Administrator both scopes and, once they have chosen the institution above, the specialities it has adopted
(the query asked again with `InstitutionId` whenever the institution changes, T245 review). A speciality chosen before the
institution changed that the institution now chosen has not adopted is unchosen. Adopted is one predicate
(`AdoptedSpecialities.IdsAt`), the one the institutional administrator's speciality list reads, asked at the panel's
institution, and panel create refuses any other speciality from everyone: "Your institution has adopted no curriculum in
this speciality, so a panel for it would have no trainee to review.", or to an Administrator "The institution you chose
has adopted no curriculum in this speciality, so a panel for it would have no trainee to review.", which the form,
offering none, never draws. So that a speciality is not simply missing, the Speciality select's help text says why: "The
panel runs at your institution. Only the specialities it has adopted a curriculum in are listed."; to an Administrator
"Choose the institution first: only the specialities it has adopted a curriculum in are listed." until they have, then
"Only the specialities the chosen institution has adopted a curriculum in are listed." Speciality is selected first where
it is offered. Where nothing is, the page shows no form: a `detail-card--empty` card (`#panel-none-creatable`) headed "Create
panel" says in a `.muted` paragraph that they cannot create a panel, and who can: "…A speciality or sub-speciality
administrator creates panels for their own speciality at their own institution, once the institution has adopted a
curriculum in it. An institutional administrator creates the institution's panels." Every seat's picker (Chair, Members, External members) offers
exactly the people the panel may seat, `ListPanelMemberCandidatesQuery` by `PanelSeat`, the rule panel create and
update enforce: active committee members at the panel's institution who do not hold Trainee (T165, T237). The Members
help text ends "Only active committee members at the panel's institution who are not trainees are listed.", and the
External members help text says the same of its list and that external members sit with the chair on the appeal body. A
stored
member who can no longer sit is not listed and not sent back: a standing warning `Alert` (`Role=""`) above the form
says so before the save ("1 member of this panel can no longer sit on it, so is not listed below: only an active
committee member at its institution who is not a trainee can. Saving takes that member off the panel."). A stored chair
who cannot sit leaves the Chair select on "Select chair", so the form cannot be saved until another chair is chosen:
Save shows "Choose a chair." in the validation summary. The Members and External members pickers leave the chair out,
and choosing as chair someone already selected in either takes them off that selection too (T257): until then the
selection kept them, unseen, and the save was refused with "A panel member is listed more than once.". A polite line
under the Chair select says so ("Priya Naidoo is the chair now, so is no longer selected under Members."): a
`role="status"` region (`#panel-chair-note`, `.muted`) present before any chair is chosen, which the select names with
`aria-describedby` after its help while the line says something. Choosing another chair, or none, puts the chair before
back in the selection choosing them took them from, and the line says that too, after what the new choice took, if
anything ("Priya Naidoo is selected under Members again."). So a chair chosen by mistake takes nobody off, nor does
moving down the Chair select with the arrow keys, which in Chrome and Edge on Windows chooses each person passed. The
line is emptied when a choice takes nobody off and puts nobody back, when the institution changes, when the panel is
read again, and when the save succeeds; after any of these nobody is put back, so a stored chair is not moved to Members
when another is chosen: they are offered there again, unselected. Each option in the Members and External members
selects is keyed by the person it offers (§ Form system).

**Nobody who holds Trainee administers a panel** (T256). Panel create and update refuse someone who holds Trainee beside
a role that manages panels (the Administrator's included, T185's rung) before anything is read, the panel's read gives
them nothing, and the pickers offer them nobody: a trainee does not choose who sits on their review. So the panel form,
for a new panel (`/committee/panels/new`) and an existing one (`/committee/panels/{id}`) alike, reads
`GetDecisionPanelFormOptionsQuery` first and, where its `TraineeNote` is set, shows no form, no "Decides for" card and
nothing about the panel: a `detail-card--empty` card (`#panel-trainee-note`) headed "Create panel" or "Update members"
says in a `.muted` paragraph "You hold the Trainee role, so you cannot create or change a decision panel, including one
that reviews you.". The panel list (`/committee/panels`) offers **New panel** only when creating a panel would be
accepted (`MayCreateAny`) and **Edit** only on a row whose form would open (`DecisionPanelSummaryDto.CallerMayManage`,
the rule panel update and the form's read demand), so a committee member, a coordinator, or a speciality administrator
looking at another speciality's panel is offered neither either. To someone who holds Trainee beside a role that manages
panels it also says why, in a standing info `Alert` (no role) above the list (`#panels-trainee-note`), with the same
sentence; the list of panels and who decides each EPA stay in view. Its empty card says "Create a panel before
scheduling reviews." only to someone who may create one; otherwise "No decision panel runs at your institution yet.", or
to a global Administrator, who belongs to no institution and lists every panel, "No decision panel has been created yet."

**Nobody who holds Trainee administers a user, and nobody changes their own account** (T278). Every Users read and
command refuses someone who holds Trainee beside a role that administers users (the Administrator's included, T185's
rung) before anything is looked up (`UserAdministrationRules.DemandUserAdministration`), and both Users pages ask the
same rule before they send anything (`TraineeNoteOnUserPages`, read off `MayAdministerUsers`, so page and handler cannot
drift). Such a caller is shown no table, filter, name or button, only a standing info `Alert` (no role) saying "You hold
the Trainee role, so you cannot view or change user accounts, including your own.": `#users-trainee-note` on the list
(`/admin/users`), `#user-trainee-note` on the user page (`/admin/users/{id}`), which is titled "User". Neither page then
has a subtitle: theirs promise roles, passwords, lockout and access, which is what the page withholds. Nobody, the
Administrator included, changes their own roles, lockout or password there (`UserAdministrationRules.IsCaller`, refused
by `DemandNotCaller` before the lookup). On the caller's own account the user page lists their roles with no Remove (and
no "System-managed", which means something else), shows no Add role, Reset password or Lockout card, and says why in a
standing info `Alert` (`#user-own-account-note`) that ends in a link, "Change your password", to
`/account/change-password`, which asks for the current password. Its subtitle reads "Your own account's summary, roles
and pending invitations.". Revoking the pending invitations to their own email is still offered: an invitation to an
email that has an account cannot be accepted. Both pages show a refusal or a failed load through `RefusalText.Of`.

**Trainee is system-managed on the user page, as PendingTrainee is** (T303). Admission (`AdmitTrainee`) grants it, with
the profile and the adoption pin, and Mark complete (`CompleteTraineeProfile`) takes it away, so Add role never offers
it (`UserAdministrationRules.AssignableRoles`: InstitutionalAdmin to Assessor) and a held Trainee reads "System-managed"
with no Remove. The Add role field's help text (`TraineeRoleNotOffered`, named by the select's `aria-describedby`) says
where a registrar is made a trainee: Trainees, "Admit to curriculum". Add role and Remove role refuse Trainee before the
user is looked up (`DemandAssignableRole`, `TraineeRoleByAdmissionOnly`).

**Who is offered scheduling** (T216). The page reads it from `GetCommitteeReviewsAccessQuery`, never from the caller's
roles: `MaySchedule` is the rule the scheduling command and the agenda preview demand first. A caller it refuses, a
committee member or anyone who also holds Trainee, is not offered "Schedule review", and a link that names a panel
opens no form (only a typed or saved one: Decisions due gives such a caller no Schedule link). The subtitle then reads "Open existing
committee reviews.". Someone who holds Trainee, whatever other role admits them, the Administrator's included (T185's
trainee rung), is also listed no review, their own included, so the list's empty card says why: its title is "No
reviews to show", its body is the query's `TraineeNote` ("You hold the Trainee role, so you cannot schedule a committee
review or preview its agenda, and this page lists no one's reviews. Your own are on My committee reviews once they are
ratified.", or, beside a committee member's role alone, "…so this page lists no one's committee reviews…"), and its
action is a `.btn-outline` link, "Open My committee reviews". A committee member who holds no Trainee role keeps the
list of their panels' reviews; their empty card says those appear once they are scheduled.

**Who is listed which reviews** (T218). The list is exactly the reviews whose Open link will open, by the review's own
read ladder in its set form (`CommitteeDecisionAuthorization.ReadableReviewsAsync`, which the review page and every
sibling read demand through `DemandReviewAccessAsync`): a coordinator or institutional administrator every review a panel
of their institution holds; a speciality or sub-speciality administrator the reviews of their own speciality's or
sub-speciality's trainees at their institution (T182's scope, read back); a committee member the reviews of the panels
they sit on, wherever; a global Administrator every review. Someone who holds Trainee is listed none (above), not even
their own ratified review, which opens for them and is on My committee reviews: the one place the list is narrower than
the review. So a scheduler finds the review they have just scheduled, and the Decisions due page links a review exactly
where it would open. A speciality administrator's reach follows the trainee's programme now, as every read about the
trainee does, not the programme they were reviewed in: when a trainee moves from Surgery to Paediatrics at the same
institution, their earlier reviews there pass to the Paediatrics administrator and leave the Surgery one
(`CommitteeDecisionAuthorization.InSchedulingReach` says why the review is not stamped instead). The list's empty card
never says that no review exists, since reviews out of the caller's reach may, and does not say which reviews are in
reach, which differs by role: to a caller who may schedule, its title is "No reviews yet" and its body "The reviews you
can open appear here once they are scheduled. Use Schedule review to put a trainee before a panel." It never says
"Schedule the first committee review".

**Entrustment-only reviews** (T131 slice 5). A review before a panel sitting as a College committee (the neonatal CCC)
decides entrustment only: its decision is the STARs staged at it, and it records no progression category. A general
panel's semester-1 sitting may be one too. Every page says so in the same words, built in C#
(`CommitteeDecisionWording`, which the portfolio PDF shares):

- **The type** reads "Annual progression review", "Pre-graduation review" or "Entrustment-only review" on the review
  card, and "Annual progression", "Pre-graduation" or "Entrustment only" in a list column.
- **The outcome** of a recorded decision is its category in words ("Satisfactory with Observations"), never the enum's
  name. An entrustment-only review's reads "Entrustment decisions only": as the decision's heading on the review page,
  in the Decision column of the schedule and of the trainee's reviews, as the heading of the trainee's current decision,
  and on the portfolio PDF's Decision line. It never reads "Pending" once the decision is recorded.
- **The Decision card** of an entrustment-only review has no Category field. In its place, above the form, a `.muted`
  paragraph (`#entrustment-only-note`) says the decision is the entrustment decisions staged below and records no
  progression category; once the review is ratified it says instead that the decision is the entrustment decisions the
  review issued, each named on its agenda. Rationale, conditions and who was present are asked as on any review. O8's
  sitting-order warning is not shown: it is about a progression decision, and this review takes none. While nothing is
  on its agenda, Record decision is shown disabled with the reason (the T107 pattern, `#record-reason`): its decision is
  what the agenda holds, and staging a decision puts the EPA there.
- **Every category select opens on "Select a category…"**, the first-time decision's as well as a remitted appeal's
  replacement, so the page never shows a category the chair did not choose. Leaving it there on a progression review is
  refused by the handler, whose reason the page shows ("… records a progression category. Choose one.").
- **The appeal's Outcome select opens on "Select an outcome…"** by the same rule (T307): resolving closes the review for
  good, so the form never offers an outcome nobody chose. Resolve appeal with none chosen is refused on the page, with
  "Choose an outcome." under the select (`#appeal-outcome-message`, which the select names after its help), the focus
  moves to the select, and nothing is sent. Each option is the outcome in words with what it does to the decision,
  "Dismissed: the decision stands" and "Remitted: the appeal body replaces the decision"
  (`CommitteeDecisionWording.AppealOutcomeLabel`), never the enum's name, and the help text says what each does and that
  either closes the review (D51: there is no Upheld).
- **A remitted appeal's replacement records conditions** (T307), as the Decision form's does: an optional "Replacement
  conditions" box (`#appeal-conditions`) after Replacement rationale, on a progression and an entrustment-only review
  alike, shown on the replacement's card as "Conditions: …". What was typed for a replacement is sent only with
  Remitted: a resolver who turns back to Dismissed sends no category, rationale, conditions or attendance.
- **A remitted appeal** on an entrustment-only review has no Replacement category either. A `.muted` line
  (`#remit-entrustment-only-note`) says why, and says what the remit does not do: the replacement changes no entrustment
  decision, the STARs the review issued stand, and one is changed by revoking it and re-deciding the EPA at a new review.
- **The trainee's reviews.** Under the heading of an entrustment-only review's decision, a `.muted` line says "This
  review decided entrustment only, so it records no progression outcome."; adds, when an appeal remitted the decision,
  that the decision is the appeal body's and the review's STARs stand unless one is revoked; and adds "What it decided on
  each EPA is on its agenda below." when the agenda is shown. The detail names the type.
- **Scheduling.** The Review type select offers only the types the handler accepts (`CommitteeReviewTypes.Allowed`):
  before a College committee, only "Entrustment-only review", with help text naming the committee; before a general
  panel, all three in semester 1 and the two progression types in semester 2. The semester-1 help text says when to
  choose entrustment-only (the sitting decides STARs, not progression) and that the semester-2 sitting always decides
  progression; it does not say semester 1 never does. Choosing a panel takes its default type; choosing a period keeps
  the chosen type while the period allows it, and takes the default otherwise. Where the panel decides no EPA on the
  trainee's curriculum, the agenda preview says so first, since an entrustment-only review there is refused.
- **The panel's College committee** ("Decides for" on the panel form) cannot change while a review before the panel is
  scheduled, in progress or awaiting ratification: what the review decides was fixed from it. The select's help text
  says so, and a refused save names the open review in the card's `Alert`.

**Decisions due** (T131 slice 6). `/committee/decisions-due` is a list page for the roles that schedule reviews
(Coordinator, InstitutionalAdmin, SpecialityAdmin, SubSpecialityAdmin, Administrator). It lists, per current trainee the
caller administers or coordinates at their institution (an active profile on an account that still holds Trainee and
that an administrator has not locked, the rule scheduling reads, T238, T268), each EPA due for an entrustment decision
in a period and where the decision stands, so every Schedule link it offers is one the scheduling handler accepts. Every
sentence is built in C# (`DecisionsDueText`). A caller who also holds Trainee is shown nobody's decisions, whatever
other role admits them (T185's trainee rung): the page shows its empty card.

- **Filters**, in the `search-container`: Institution (an Administrator only, who belongs to none: nothing is read until
  one is chosen, and `#due-choose-institution` says so), Period (the scheduling form's semesters, the current one by
  default), Status ("Outstanding: not yet decided" by default, then "Every status", then each status), and EPA.
- **The opening**: a `.muted` sentence (`#due-intro`) naming the period, how many trainees and where, and what Missed
  means.
- **By EPA**: a `.detail-card` holding a `.clinic-table--compact` of eight columns: EPA (the row header), Due, Decided,
  Scheduled, Deferred, To schedule, Missed and Optional. Its caption says what the last three count. The filters do not
  narrow it: it is the whole period at a glance. Its `.table-container` (`#due-summary-scroll`) is `tabindex="0"`,
  `role="region"` and labelled by the card's heading, so a keyboard can scroll it at 390px: it holds nothing focusable.
- **The count**: a `.muted` line (`#due-count`, `role="status"`), "4 of 15 decisions due in 2026 S2 shown.", which a
  screen reader hears again whenever the Status or EPA filter changes it.
- **The list**: a `.clinic-table` of five columns: Trainee (the row header, by name), EPA, window ("2026 S1", or "2026"
  for an annual EPA), status, and action. The status cell holds the badge and, under it, a `.muted` line saying what it
  means: the STAR and review for a decision, the open review holding it, the review that deferred it, or why it is
  optional. `PagerControls` pages it, 20 rows to start. When the filters leave nothing, a `.detail-card--empty` says so.
- **Actions.** "Open review" where the status names a review and the caller passes that review's read ladder. "Schedule"
  on every row neither decided nor scheduled: it opens `/committee/reviews` with the panel the EPA routes to for the
  trainee, the trainee and the period a sitting that decides the window sits for, the window's last semester
  (`?panel=…&trainee=…&period=2026-2`): semester 2 for an annual EPA, whichever semester the page shows (Decision 5).
  The scheduling form takes each value only where it offers it, so the link authorizes nothing. Where no panel the
  caller can schedule before decides the EPA, the cell says so. Each link's `aria-label` names the trainee and the EPA.
- **An open review holding the seat.** A trainee has one open binding review per period in each seat (the general
  committee, or a College committee), and scheduling a second is refused. Where one is already open for the row's
  schedule period before the panel's seat, the row offers "Open review #N" (where the caller may open it) instead of
  Schedule, and its `.muted` line says "Review #N is open for 2026 S2 before General CCC: decide it there, or ratify that
  review before scheduling another." A line deferred on that same open review reads "Deferred at review #N, which is
  still open: its chair can reinstate it there", beside its one "Open review" link.
- **States**: `StatePanel`'s skeletons while loading, its danger `Alert` on a failure, and its empty card ("Nothing due
  for 2026 S2") when no trainee owes a decision.

| Status | Badge |
|---|---|
| Decided | `badge-completed` |
| Scheduled | `badge-submitted` |
| Deferred | `badge-accepted` |
| Not scheduled, Due by year end, Partial period, As opportunity allows | `badge-draft` |
| Missed, Revoked: re-decide | `badge-declined` |

Decided means a STAR from a sitting for a period in the window that still decides it, whether or not an agenda line
records it: Active; Expired, since expiry is informational (Decision 10); or Superseded, by a STAR from another window or
by one that decides this window in turn. A revoked one reads "Revoked: re-decide", and so does one superseded inside the
window by a STAR since revoked. Deferred holds only while no later sitting has decided the EPA since, in sitting order
(the period a review sat for, then the order reviews were scheduled in), so a deferral that a later, since-revoked
decision overtook reads "Revoked: re-decide". Missed is computed when the page is read, as on the agenda: the window has
ended with nothing decided, deferred or on an open review's agenda, and an EPA decided as opportunity allows, or in a
partial period, is never missed. The agenda planner, its "routed elsewhere" list and its "Already decided in this
window" sentence read the same rule (`CommitteeAgendaStatus.IsDecided`), so a sitting never plans as due an EPA this page
calls decided (T215). An open review's own lines are read against it too, so a line this page calls decided reads
"Decided elsewhere" on the review and never holds back its Record or Ratify; and "Revoked: re-decide" is
`CommitteeAgendaStatus.HasLostItsDecision`, the predicate a review in progress names an EPA off its agenda by (T235).

## Status dots

```css
.status-dot      /* 0.6rem circle, inline-block, margin-right sm */
.status-dot.ok   /* --success-color, 5.88:1 on white */
.status-dot.warn /* --warning-color, 5.77:1 */
.status-dot.err  /* --danger-color, 5.95:1 */
```

Used in the Administrator dashboard system-health card to show service status at a glance. A dot is a meaningful mark,
so it needs 3:1 against its ground; the ok and warn dots were 2.87 and 2.57:1 until the semantic colours darkened
(2026-09-27, T335, flow 01; T322).

## Pager

```css
.pager                 /* flex, gap 1rem, margin-top 1rem */
.pager-info            /* muted, .9rem — "Showing 1–20 of 137" */
.pager-actions         /* flex, gap .5rem — prev/next buttons */
.pager-page-size       /* inline-flex, gap .4rem — "Per page: [20]" */
.pager-page-size-label /* muted, .85rem */
.pager-page-size-select/* width auto, compact */
```

`PagerControls.razor` is the one component for pagination. Use it on every list that can grow.

## The reconnect dialog and the error bar

2026-09-27, T335, flow 01: round 3's `R2-Reconnect`, `R2-Reconnect-Narrow`, `R2-ErrorBar` and `R2-ErrorBar-Narrow`
(`design/flows/01-shell/round-3/`), the round-2 review's S18, S19 and D7, and T330. What the Blazor runtime does was read
in the source of the `blazor.web.js` the app serves (10.0.12: `UserSpecifiedDisplay`, `DefaultReconnectionHandler`,
`BootErrors`). `Design/ReconnectModalTests` and `Design/ErrorBarTests` hold that file to the names both are built on, so a
runtime that changes them fails the suite.

### The reconnect dialog

`Components/Layout/ReconnectModal.razor`, `.razor.css` and `.razor.js`; `App.razor` renders it, statically, on every page.
The runtime finds it by its id, `components-reconnect-modal`. As the connection drops it puts a class on it for the state
and raises `components-reconnect-state-changed`: `show`, then `retrying` once a second while it counts down to its next
attempt (`detail.secondsToNextAttempt`) and once with 0 as the attempt starts, and at the end `hide`, `failed` or
`rejected`; or `show` and at once `paused`. By default it makes 30 attempts: ten at once, ten 5 s apart, ten 30 s apart.
It opens nothing and, with this dialog in the page, never reloads by itself; the script does both.

| State | When | Heading and icon | Sentence | Button | Role |
|---|---|---|---|---|---|
| Rejoining | `-show`; also the runtime's first ten attempts, made at once | Reconnecting, loader-circle turning | "The connection to Wombat dropped. Reconnecting now.", and the bar | none | dialog |
| Retrying | `-show -retrying`: from the second attempt that has to wait its turn | Could not reconnect, clock | One line, which `data-attempt` picks: `"waiting"`, the count to the next attempt, "Trying again in N seconds." (the script writes N a second at a time; "1 second"); `"started"`, "Trying again now.", and the bar | none | dialog |
| Failed | `-failed`: the attempts are spent | Connection lost, circle-alert | "Wombat cannot be reached. Try again when your connection is back. If the page cannot be restored, it reloads, and anything not yet saved on it is lost." | Try again (refresh-cw): the template's `retry()` | alertdialog |
| Paused | `-paused`: `Blazor.pauseCircuit()` | Page paused, pause | "This page is paused. Resume to carry on." | Resume: `resume()` | dialog |
| Resume failed | `-resume-failed`, put on by the script when `resume()` cannot reach the server | Could not resume, circle-alert | Failed's sentence | Try again (refresh-cw): `resume()` | alertdialog |
| Rejected | `-rejected`: the server answered but holds neither the circuit nor a state to resume it from | Reloading, loader-circle turning | "Reloading the page…" | none | dialog |

- **One state at a time.** Each state is its own element, carrying one `-visible` class, and the stylesheet shows it under
  one rule. The runtime adds `-retrying` beside `-show` and never takes `-show` away while it retries, so a rule under
  `-retrying` hides the first attempt's state (T330: before it, "Rejoining the server..." stood above the retry's line).
  `ReconnectModalTests` evaluates the rules on the served markup for every class the runtime sets, down to the line and
  the bar each shows.
- **Could not reconnect is one state, its line replaced** (2026-09-27, T335, flow 01; R2-Reconnect's states 2 and 3,
  "The same line, replaced when the attempt starts"). The script sets `data-attempt` on every `retrying` event:
  `"waiting"` while the runtime counts down, `"started"` as the attempt starts. It picks the state's line
  (`data-reconnect-line`), not the state: the count, or "Trying again now." and the bar, in the one sentence the dialog is
  described by. Until then waiting and started were two elements, each with its own "Could not reconnect", and since the
  focus follows the shown state's heading (below), in an outage it moved between the two on every attempt and a screen
  reader read the heading again every few seconds. Now the heading keeps the focus from the first retry to the last;
  `ReconnectModalTests` holds that waiting, started and waiting again show one and the same element.
- **No "Try now".** The runtime has no call that skips its countdown, and `Blazor.reconnect()` from a button would race
  the attempt it makes itself. `Blazor.reconnect()` runs only under Connection lost's Try again.
- **What the buttons call.** `Blazor.reconnect()` resolves true once reconnected, false when the server answers but no
  longer holds the circuit, and throws when the server cannot be reached. `Blazor.resumeCircuit()` resolves true once it
  has started a new circuit from the state the server kept, and false when there is none; it throws when the server
  cannot be reached. So Try again reconnects, else resumes, else reloads, and on a throw stays on Connection lost and
  tries again when the tab is next shown, as the template does. Resume resumes, else reloads, and on a throw moves to
  Could not resume. A reload shows Reloading first.
- **The buttons** are `.btn .btn-primary`, found by `data-reconnect-action` (`retry` or `resume`). While an attempt runs
  the button carries `aria-disabled="true"` and a second press sends nothing (§ Button system). If its state has gone
  when the attempt ends, the focus moves to the button of the state that took its place (Resume, then Could not
  resume's Try again).
- **Focus, name and role.** The heading (`tabindex="-1"`, no ring) takes the focus when the dialog opens, and again
  whenever the shown state does not hold the focus (2026-09-27, T335, flow 01; the review of the t335 branch): as the
  runtime begins to retry, Reconnecting hides, and a hidden element's focus falls to the body inside the modal, where
  Connection lost, an alertdialog, never took it. So each move to a new state's element moves the focus to its heading,
  and its sentence is still said by the live region, since a move inside the dialog reads the heading alone. A new
  attempt is not a new state: Could not reconnect's line changes under a heading that keeps the focus. The dialog
  is named by the shown state's heading and described by its sentence, and its role is `alertdialog` for Connection lost
  and Could not resume, the `<dialog>`'s own elsewhere. The script sets all three after each event and any the runtime
  raises with it, before it moves the focus, so Paused, which follows `show` at once, is the state that takes the focus.
  A button's own state going while its attempt runs still hands the focus on to the button that took its place (below).
- **The live region.** One `aria-live="polite"` region, inside the dialog: while it is modal the page behind it is inert,
  and so is any live region there. It is written only when the state changes, with that state's sentence
  (`data-announce`) and never the count; Could not reconnect's two lines are one sentence to it, "Could not reconnect.
  Trying again.", so each attempt does not repeat it. As
  the dialog opens it stays silent, since the focused heading reads the name and the description.
- **It cannot be dismissed.** Esc is refused: nothing behind it answers until the connection is back.
- **The look.** A native `<dialog>` opened modal, on `--scrim`. 400px wide (`25rem`), and never closer than
  `--space-md` to the screen's sides, so 358px at 390. `--radius-xl`, `--shadow-dialog`, padding `--space-lg` (1.25rem
  below 641px), and 12.5rem high at least from 641px, so it does not jump between states. The heading is 1.25rem, with a
  20px Lucide icon: loader-circle and pause in `--secondary-color`, clock in `--warning-color`, circle-alert in
  `--danger-color`. The bar is 4px, `--secondary-color` running on a `--header-bg` track. The button sits at the right;
  below 641px it fills the dialog's width at 44px. The icons are inlined (§ Icons).
- **Motion.** It arrives after 0.3s, so a connection that drops and comes straight back shows nothing, then slides up and
  fades in. Under `prefers-reduced-motion: reduce` the spinner, the bar, the slide and the fade stop, and the words carry
  the state. The dialog is opaque at rest: only its animation starts it transparent, so with the animations off it is
  still there (the template's started at opacity 0).

### The error bar

`#blazor-error-ui`, at the foot of `MainLayout.razor`; its rules are in `MainLayout.razor.css`. The runtime shows it when a
page's circuit fails (an exception in a component, or a connection that cannot start) by setting an inline
`display: block` on it, and wires its buttons by their classes: `.reload` reloads the page and `.dismiss` hides the bar.
Both are `onclick` properties set from the runtime's own script, so the CSP has nothing to refuse, and they work on a
`<button>` as on the template's link.

- **Markup.** `<div id="blazor-error-ui" role="alert" data-nosnippet>`, outside the layout's `AuthorizeView`, so on every
  page, holding one `.error-bar-row`: a 20px triangle-alert icon, `<p class="error-bar-text">` "This page no longer
  responds; copy anything you need, then reload.", `<button class="btn btn-sm btn-primary reload">` Reload with the
  refresh-cw icon and `<button class="btn btn-sm btn-outline dismiss">` Dismiss with the x icon (R2-ErrorBar; the icons
  carry `.error-bar-button-icon`; 2026-09-27, T335, flow 01, the review of the t335 branch). The row is on the wrapper
  because the runtime's inline `display: block` on the outer element would beat a flex row there. No reference (D7): a
  per-circuit reference is new code, for a later task.
- **The look.** Fixed at the foot, `--warning-bg` with a 3px `--warning-color` top edge, `--shadow-bar`, `--text-color`,
  the icon in `--warning-color`. From 641px it starts beside the 250px sidebar, with the page's own gutters (32px left,
  24px right), its buttons small. Below 641px the sentence comes first, then Reload and Dismiss side by side, each half
  the row at 44px, at a full button's 0.95rem and without their icons (R2-ErrorBar-Narrow).
- `Design/ErrorBarTests` renders `MainLayout`, signed in and signed out, and reads the bar from it (until the review it
  read the source, on the claim that the layout could not be rendered there).
- `.reload` and `.dismiss` are the runtime's classes and nothing styles them (§ Non-negotiables); the buttons look like
  buttons by `.btn`.

## Accessibility

- Every form field has a `<label for="…">` tying to the input's `id`.
- A field's help text has an id, and its input names it with `aria-describedby`, help first (§ Form system, T193).
- A sign-in field says what it holds: the email is `autocomplete="username"` and the password
  `autocomplete="current-password"`. A password being set is `autocomplete="new-password"`: register, change password,
  and an administrator's reset, where the browser would otherwise offer the administrator's own password (T193).
- A refused sign-in (`?error=`) is named by the email and password fields' `aria-describedby`, and a refused link by
  the password field's: the page reloads with focus in the field, and the alert alone is not announced (§ Alerts,
  validation, empty states). A refused password change is named by each of its three fields; the page reloads with the
  focus on the refusal (T265). A refused registration is named by each of its four fields, and the page reloads with the
  focus in the first name (T285).
- Required fields show a visual `*` plus `aria-required="true"`.
- `.visually-hidden` is available for screen-reader-only copy.
- **The focus ring** (2026-09-27, T335, flow 01; T322, T328): every element a keyboard focuses shows a 2px solid
  `--focus-ring` outline at a 2px offset, from one `:focus-visible` rule for every element. Until T328 the rule was a list
  of classes, so a focusable scroll region (Decisions due's summary, `.table-container[tabindex="0"]`), a tab and a
  checkbox showed the browser's own 1px ring. The ring is 4.86:1 on white and 4.61:1 on the page (T322: the old one was
  2.99:1 there). On dark chrome it is `--nav-focus-ring`, set by that chrome's own stylesheet, since `--focus-ring` is
  2.90:1 on the gradient's start (§ Design tokens). Never remove a focus outline without replacing it: the only rule
  that does is `h1[tabindex="-1"]:focus`, the heading `FocusOnNavigate` focuses for the screen reader, which no keyboard
  reaches. `Design/StylesheetRuleTests` holds both.
- **Reduced motion** (2026-09-27, T335, flow 01; S17). Under `@media (prefers-reduced-motion: reduce)` nothing slides,
  fades or pulses: every transition's duration and delay go to 0, and every animation runs for 0.01ms, once, with no
  delay, so it jumps to its last frame. That `app.css` rule is a general safety, for any animation that has no
  reduced-motion rule of its own. The reconnect dialog has one: the dialog is opaque at rest (only its fade keyframes
  change its opacity), and `ReconnectModal.razor.css` sets `animation: none` and `transition: none` on it, its backdrop,
  the spinner and the bar's fill. So it shows at once and still, the bar's fill standing where the design draws it, and
  its words carry the state. The skeleton is a still `--header-bg` block (§ Skeleton loaders). Every transition in
  `app.css` runs at `--motion-fast` (150ms), which `Design/StylesheetRuleTests` holds.
- **The shell** (2026-09-27, T335, flow 01; § Layout grid, § The NavMenu).
  - **The nav ring:** every focusable control on dark chrome (the sidebar, the phone bar, the open menu's foot, the
    signed-out bar) takes `--nav-focus-ring`, 2px at a 2px offset, set by `MainLayout.razor.css`, `NavMenu.razor.css`
    and `NavItemLink.razor.css` over the page's ring. `Navigation/MainLayoutTests` holds each.
  - **One current item:** at most one nav item is lit, and only it carries `aria-current`: `"page"` on a list, `"true"`
    on the owner of a page under it. On My account the account row's name carries `aria-current="page"`, and the trail's
    last crumb does on a page that has one. `Navigation/ActiveNavItemTests` renders every routable page for every acting
    role.
  - **Target sizes:** a nav row is 28px at a pointer's width and 44px on a phone, 4px apart; the account link 32px, 44px
    on a phone; the switch 32px, 44px on a phone; the Menu toggle, Sign in on a phone, the phone menu's Sign out and the
    folded trail's link 44px.
  - **The scroll list:** only the nav list scrolls, and it takes no tabindex, so it adds no tab stop to every page; the
    browser brings each focused link into view, and the lit item is scrolled into view as the page loads.
  - **The phone toggle** is a real checkbox, visually hidden and focusable, named by its label ("Menu" or "Close"); its
    checked state is what a screen reader hears, so nothing carries `aria-expanded`. Every navigation closes it.
  - **A contrast theme** keeps the current-page cues: the lit item's bar and My account's underline are drawn again as a
    border and a text underline in `CanvasText` (§ Layout grid).
  - **Landmarks:** one `<header>`, the banner, holding the brand, the navigation and the account row (the review of the
    t335 branch: the brand and the account row were outside every landmark); in it one `<nav aria-label="Main">`; one
    `<nav aria-label="Breadcrumb">` for a trail; and the page in `<main>`. A nav group's heading is a `<p>` that names its
    list, so the page's `<h1>` stays its first heading.
- Never leave the focus on the page body. A button is not disabled by its own action, and says it is unavailable with
  `aria-disabled` while that action runs; an action that is done moves the focus to its result, and one whose button is
  gone does too (§ Button system, T234).
- An invalid field is marked by a stripe as well as the danger colour, and stays marked in a contrast theme (WCAG 1.4.1,
  § Form system, T236). Its focus ring is the usual outline, outside the stripe.
- Up/down reorder buttons (T019) are keyboard-focusable `<button type="button">` with `aria-label="Move field up"`.
- `<fieldset>` is reset to no border/padding and its `<legend>` styled as a heading — this is the semantic grouping for multi-field clusters. A fieldset that is one field (a group of checkboxes) is a `.form-group`, whose legend reads as a label (§ Form system, T188).

## Icons

- One `Icon.razor` wraps `<svg>` + `<use href="/icons/{name}.svg#i" />` or inline path data.
- The reconnect dialog inlines its Lucide paths instead (`ReconnectModal.razor`): it shows when the server cannot be
  reached, so it cannot fetch an `/icons/` file then (2026-09-27, T335, flow 01; § The reconnect dialog and the error
  bar).
- Icons live under `src/Wombat.Web/wwwroot/icons/` as individual SVG files, copied from Lucide (MIT licensed, compatible with AGPL-3.0).
- The nav uses the same `Icon.razor`, 20px, each item naming its glyph in `Navigation/NavItems.cs`;
  `NavItemLink.razor.css` only spaces it (§ The NavMenu). Every name must be a file under `wwwroot/icons/`, which
  `NavMenuAuthorizationTests` checks.
- **Do not load a Bootstrap Icons font.** ClinicAssist tried and the `<i class="bi bi-*">` approach renders nothing without the font, silently. Repeating that mistake is not on the table.

## Page-level patterns

Every page in `Wombat.Web` follows one of these shapes. Pick one at the top of the file and stick to it.

**A page is named once, in the same words, wherever it is named** (T190; D10 of the flow 01 review; 2026-09-27, T335,
flow 01). Its nav label, its heading, its breadcrumb and its tab are the same words, in sentence case.

- **The tab is "<Stem> · Wombat"** on every routable page: `<PageTitle>Audit log · Wombat</PageTitle>`, the separator
  U+00B7 with a space each side. Until T190 the tabs read "Dashboard — Wombat", "Sign in - Wombat", or the page alone.
- **Sentence case.** The first word is capitalised, and after it only acronyms and proper nouns: EPA, EPAs, MSF, SSO,
  STAR, CPSA, PDF, Mini-CEX, DOPS, CbD, and College where it names the College. "Activity inbox", "Sub-specialities",
  "Create college". A name that comes from data (an activity type's, a person's) is printed as it is stored.
- **The stem is the page's heading**: its `PageHeader` Title, or on an account page the card's `<h2>`. A page the nav
  opens is headed by its nav label: "Log an activity" (`/activities/new`), "MSF reports", "My data rights", "SSO
  mappings", "My account" (`/account/profile`).
- **A heading that changes is one expression, which the title and the header share**: a member
  (`<PageTitle>@Heading · Wombat</PageTitle>` beside `<PageHeader Title="@Heading" …>`) or a choice of two literals
  (`@(IsNew ? "Create EPA" : "Edit EPA")`). Until it has loaded the member says what the page is ("Activity",
  "Activity type"), never nothing. An activity's page is named for its type ("Mini-CEX (Paediatrics) · Wombat").
- **After a refused post the title starts "Error: "** (WCAG technique G88): the change password page, the MSF respondent
  page.

`Hosting/PageTitleTests` reads every routable component's source, found by reflection over `[Route]`, and fails on a
page with no title, a title without the suffix, a stem not in sentence case, or a stem that is not the heading (a
literal is compared with a literal, an expression with the same expression). A page whose heading it cannot read says
why in the test: the specialities redirect has none, and the MSF respondent page's states are held by
`MsfRespondPageHostingTests`. `TestSupport/TabTitle` reads a rendered page's tab in bUnit, which renders no
`HeadOutlet`; a page whose title is computed checks it after its load there, as the builder's tests do (its tab was empty
once the editor had loaded, T190's note).

**A person is shown by name, never by user id** (T142). The name is a field on the page's DTO, filled by the query
that serves the page with one `UserDisplayNames.ResolveAsync` call for every id it lists. Razor never looks a name up,
nothing looks one up per row, and a mapper that other handlers share stays lookup-free. The id is shown only where no
user by that id exists, or the user has no name on record. Plain text, not `<code>`: a name is not an identifier. A
filter that narrows a list to one person takes what the list shows, a name, not an id.

Five pages keep ids on purpose. The audit log and an audit entry (`/admin/audit`), and the data-rights request list and
a request (`/admin/data-rights`), because the id is the record there. The anonymous portfolio verification page
(`/portfolio/verify`), because it must not disclose a name. No input takes a raw user id: the committee review schedule
form's trainee is a picker since T182.

**A workflow state or move is shown by its label, never by its key** (T220). The activity page's header, summary and
history, the activity lists, the dashboards' badges, the committee's evidence snapshot and the portfolio PDF all print
the label the query carried from the activity's pinned workflow (`CurrentStateLabel`, `FromStateLabel`, `ToStateLabel`,
`TransitionLabel`, `FinalStateLabel`, `SourceStateLabel`), so a page names a state as the refusals and notices on it do
(T189): "Awaiting supervisor", not `submitted`. A move is named as its button is ("Sign Off"). Razor never works a label
out. The key appears only as what picks a badge's colour (`BadgeFor.ActivityState(item.CurrentState, item.IsFinished)`,
§ Badges), and as text only where the pinned workflow cannot name it.

**A committee review's state is shown by its label, never by the enum's name** (T250). The schedule, the review page,
the trainee's own reviews and the portfolio PDF print one label per state, `CommitteeDecisionWording.StateLabel`, which
the review DTOs carry as `StateLabel`: "Scheduled", "In progress", "Decided", "Ratified", "Under appeal" and "Closed".
`Final` reads "Closed", since a review reaches it when a formative review is closed or when an appeal is resolved. A
refusal that names a review's state mid-sentence uses `StateInSentence`, which is built from the same labels ("decided,
not yet ratified"). The data-rights JSON export keeps the stored value, because it is the record.

### Breadcrumbs

(2026-09-27, T335, flow 01; D5; R2-Rules § 3, R2-Detail-*.) `PageHeader` draws the page's trail above its header, from
the owner table (`NavOwners.TrailTo`), so a page gets it by having a header; `Breadcrumbs.razor` renders it.

- **Under an owner:** Home › the owner › the page. The owner is the nav item the page lights for the acting role (§ The
  NavMenu), so the trail follows the owning list, not the page the person came from (D5: an activity opened from a
  committee review is under the inbox for an Assessor; flow 07 decides whether evidence gets a context trail).
- **With no owner for the acting role**, on a page in the owner table or on a list the acting role's menu does not
  offer: Home › the page.
- **None** on Home, on a list the acting role's menu offers (its own "Home › the list" is the trail its pages inherit,
  R2-ErrorBar), on My account and the sign-in pages, and on the failure pages.
- **Change password:** Home › My account › Change password.
- **The page's own crumb** is its title, a `<span aria-current="page">` in body text at weight 600; the others are links,
  separated by a 14px chevron. A header whose crumb is not its title passes `CurrentCrumb`: a data-rights request's is
  its id, once it has loaded (the review of the t335 branch; `ActiveNavItemTests`).
- **A crumb between the owner and the page**, where the owner table names one, is passed by the page as `Trail`: the
  College on a College's specialities, for the Administrator (Home › Colleges › the College › Specialities). For the
  College admin the College is the page's own crumb (Home › Specialities › the College).
- **A page drawn in place of another** names itself: Access denied, drawn at the address of the page that refused (D6),
  passes `Page="typeof(AccessDenied)"`, so its trail is its own (none), not the refused page's.
- **Below 641px** the trail folds to one 44px link to its parent, the crumb before the page's own, led by a back chevron
  (R2-Detail-Phone): `.breadcrumb-ancestor`, `.breadcrumb-current` and the separators are hidden there.
- **Its place:** at the top of the article, whose padding spaces it from the bar, and 16px above the header
  (`.breadcrumbs { margin: 0 0 var(--space-md) }`, R2-Detail-*). Until the review of the t335 branch it stood 16px under
  the bar, and the heading's browser margin kept the header off it.

`Navigation/ActiveNavItemTests` holds the trail.

### List page

```
<PageTitle>… · Wombat</PageTitle>

<PageHeader Title="…" Subtitle="…">
  <Actions>
    <a class="btn btn-primary" href="/…">Create …</a>
  </Actions>
</PageHeader>

<div class="search-container shadow-sm mb-4">
  <div class="search-grid">
    <div class="search-field">…</div>
  </div>
</div>

@if (IsLoading)            { <Skeleton … /> × N }
else if (LoadError is not null) { <Alert Kind="danger" /> }
else if (Items.Count == 0)      { <DetailCard Empty CTA /> }
else
{
  <div class="table-container shadow">
    <table class="clinic-table"> … </table>
  </div>
  <PagerControls … />
}
```

### Detail page

```
<PageHeader Title="@item.Title">
  <Actions>
    <a class="btn btn-outline btn-sm" href="…">Edit</a>
    <button class="btn btn-danger btn-sm" @onclick="…">Delete</button>
  </Actions>
</PageHeader>

<div class="details-grid">                        <!-- 1fr 2fr @ >= 900px, 1fr @ narrow -->
  <aside class="detail-card"> summary </aside>
  <section class="detail-card"> main body </section>
</div>
```

A details grid whose second card comes and goes with the page's state can give a lone card the whole row:
`.details-grid--lone-spans` (`> .detail-card:only-child` spans `1 / -1`, T266). The grid's first column is its narrow
third, so a lone card sat there with the rest of the row empty: the MSF campaign page, whose Quick template card is only
on the create page, showed a campaign's card 223px wide at 1000px, in a 694px row, with its invitees table scrolling 143px
inside it. It is the only page that asks. It is opt-in (T266 review): on a grid of like cards (My authorisations' STARs,
My progress's trajectories) one card should look like each of several, and a lone trajectory chart, which scales with
its width, would stand three times as tall; on My MSF reports the list would jump from the whole row to a third of it
when a report is selected.

### Form page

```
<PageHeader Title="…" />

<ActionResult @ref="_result">          @* what Save did; takes the focus once it is done (§ Button system, T234) *@
  <Alert Kind="success">…</Alert>
  <Alert Kind="danger">…</Alert>
</ActionResult>

<div class="form-container">
  <EditForm … >
    <ValidationSummary class="validation-summary-errors" />

    <div class="form-grid">
      <FormField Label="…"> <InputText class="form-control" /> </FormField>
      …
      <FormField Label="…" FullWidth="true"> <InputTextArea class="form-control" /> </FormField>
    </div>

    <div class="form-actions">
      <a class="btn btn-outline" href="…">Cancel</a>
      <button type="submit" class="btn btn-primary">Save</button>
    </div>
  </EditForm>
</div>
```

### Dashboard page

> **Amended 2026-09-27, T335, flow 01** (R2-Landing-Assessor, -CM, -Loading, -Error, R2-Shell-Admin, -Assessor,
> R2-Phone-Folded; the round-2 review, S20 and S22(c); T329's Home half): Home is headed "Home", with no "Welcome" and no
> "Viewing as"; it has one header action per role, a loading frame and a load error; the switch line is gone to the
> sidebar. The cards are kept as they were, less the duplicates the boards drop, and belong to later flows.

Dashboards are a composition, not a standalone page pattern.

- **`Home.razor`** is the one routed page at `/`: the frame, then the acting role's dashboard (the `ActingRole` Routes
  cascades, § Acting role).
  - **Its heading is "Home"**, its nav label, and its tab "Home · Wombat" (D10). Until T335 it was "Welcome, {email}",
    under "Dashboard — Wombat": the top bar now names the person.
  - **Its subtitle is "{acting role's label} · Semester N, YYYY"** (`Navigation/HomeFrame.Subtitle`): "Assessor ·
    Semester 2, 2026". The semester is `AcademicPeriod.Containing` of today on the South African calendar
    (`QuotaCalendar.Today`, from the injected `TimeProvider`), the same "Semester 2, 2026" the progress pages and the
    committee's card name that day. It replaced "Viewing as {role key}". Someone with no role has no subtitle (D8).
  - **One header action for a role whose main job starts from Home** (A-Spec § e; `HomeFrame.ActionFor`), a primary
    button with its icon: the Trainee's "Log an activity" (`/activities/new`) and the Institutional admin's "Invite a
    person" (`/admin/invitations`, where the invitation form is). Every other role has none: its main job is a card's row
    or a nav link, and a pending trainee can file nothing yet. The action is a link Home offers, so it too opens a page
    that admits the role. At phone width it is its own row **below the header's rule**, 44px tall and the page's width
    (`.home-action`), as R2-Phone-Folded draws it: the rule is drawn under the heading block instead of under the header
    (`.header-container:has(> .actions-cell > .home-action)`), so the action falls below it. Until the review of the t335
    branch it sat under the heading but above the rule, which this passage attributed to the same board.
  - **A header's actions stand on the heading's baseline** (`.header-container`'s `align-items: baseline`, every page):
    at `flex-start` they sat at the header's top, 16px above the heading, which kept the browser's own top margin (the
    heading has none in the header since the review of the t335 branch, § Typography).
  - **No switch.** The "You also act as … Switch view" line is gone: the sidebar's head offers the switch on every page
    (§ Acting role). A switch's one-time result still shows under Home's header (`ActingRoleSwitchAlert`, through
    `PageHeader`).
- **Role dashboards** live under `Components/Pages/Dashboards/` (`AdministratorDashboard`, `CollegeAdminDashboard`, `InstitutionalAdminDashboard`, `SpecialityAdminDashboard`, `SubSpecialityAdminDashboard`, `CommitteeMemberDashboard`, `CoordinatorDashboard`, `AssessorDashboard`, `TraineeDashboard`). Each one is a child component — **no `@page` directive**, **no `<PageTitle>`**, **no `<PageHeader>`**. Adding any of those would duplicate Home's header.
- **The frame while a dashboard reads, and when its read fails** (R2-Landing-Loading, R2-Landing-Error; T329). A
  dashboard that reads inherits `RoleDashboard<TSummary>` and draws its cards inside a `DashboardFrame`:
  - **Loading:** the header, the grid (`aria-busy="true"`) and every card's title render at once, each card a skeleton
    (`DashboardCard`'s `IsLoading`). Nothing is offered until the read returns: no card's content, no card that links.
    Until T335 the page showed a column of bare skeletons. A dashboard with two frames (the trainee's, pending or not)
    draws the acting role's until its summary says which.
  - **Failed:** one `danger` `Alert`, "**Could not load your Home.** Nothing has changed. Try again, or come back in a
    few minutes.", with a Try again button (outline, small, the `refresh-cw` icon) that reads again, and **no cards**: a
    card drawn empty says there is nothing, and a skeleton says it is still coming. The exception is logged, never shown
    (until T335 each dashboard printed its message, which for a database failure is EF's text, T272). Try again's button
    goes with the alert, so the answer (the cards, or the alert again) takes the focus once it has come: the frame is an
    `ActionResult` (§ Button system, T234).
  - **Guard the eager reads.** The frame renders before the summary, so a card's parameters are read while it is null: a
    title, stripe or count that reads it is written `Summary?.…` (the Coordinator's warning stripe, the admins' coverage
    title, which is "Curriculum coverage" until it knows the semester), and a card's content only
    `@if (Summary is { } summary)`. `HomeFrameTests` renders every role's Home with a read that never returns, and with
    one that fails.
- **The cards** belong to later flows (S20); today's are kept, less the duplicates the boards drop:
  - the Assessor's work waiting is one card, "Waiting for your rating", its count (`PendingRequestCount`) as its badge,
    its rows under it and "Open inbox →" at its foot. "Pending requests", "Awaiting your review" and the Actions card,
    three ways to one inbox, are gone;
  - the Administrator's job status is in System health only (spanning two), beside Users across institutions. The
    Maintenance card, four nav links over again, is gone; its fifth, Curriculum progress, is a header link on Curricula,
    the page that owns it, for the Administrator its page admits;
  - the Trainee's Actions card is gone: its Log an activity and Request an assessment both opened `/activities/new`,
    which the header now offers.
- **Every role has its own case in Home's switch**, and the dashboard it picks admits the role (T261). Until then a
  CollegeAdmin fell through to the trainee's dashboard: "No curriculum assigned yet", beside links to pages that refuse
  a CollegeAdmin. PendingTrainee shares the trainee's dashboard, which branches on it. A signed-in user who holds **no
  role** is not given a role's dashboard either, and Home shows no subtitle. A graduate (the `trainee_record` claim, T252)
  gets one card, "Your training record", pointing to My progress, which `TraineeOrFormerTrainee` admits (A-Spec § e:
  "her record, not No role assigned"). Anyone else with no role (an administrator removed the last one) gets one "No role
  assigned" card with no link, since no page is theirs to open.
- **Each dashboard names the roles it is for in `[Authorize(Roles = …)]`.** Blazor enforces `[Authorize]` on a routed
  page only, so on a dashboard it is a statement the test reads, not a gate. A dashboard with none, or a bare
  `[Authorize]`, would be judged as admitting every signed-in user; the test refuses both.
- **Every link on a dashboard opens a page that admits the dashboard's role** (T261), the nav's rule (§ The NavMenu).
  `DashboardLinkAuthorizationTests` renders Home for a holder of each role and judges every link on it with the nav
  test's `PageAccess.PageFor` and `RefusalOf`. It also checks that each dashboard's renders draw every `href` it can
  draw: those its own source declares, and those declared by every Wombat.Web component it names and by the components
  those name (the shared `EpaTargetCoverageList`, say). So a link in a branch the test does not reach, in the dashboard
  or in a component it uses, fails until the test's `DashboardSender` fills that branch; the sender fills every list a
  dashboard shows. An `href` that only passes on the component's own parameter (`DashboardCard`'s `href="@Href"`) is
  judged where the caller sets `Href`, and an SVG `<use href>` is no link. A
  link to a page the role cannot open is removed or pointed at one it can: the Coordinator starts an MSF campaign rather
  than issuing an invitation, and the InstitutionalAdmin's quick links are Users, Invitations, Curriculum adoptions and
  Entrustment decisions, not the Administrator's Institutions or the College's Specialities.
- **`/dashboard/switch/{role}`** (`ActingRoleSwitch`, mapped in `Program.cs`) stores the acting role with the account and 302s to its return address or `/` (§ Acting role). It never renders UI directly. Every way a user reaches a dashboard goes through Home.

So a dashboard `.razor` file looks like this:

```razor
@using System.Security.Claims
@using MediatR
@using Wombat.Application.Features.Dashboards.{RoleName}
@attribute [Authorize(Roles = "{RoleName}")]
@rendermode InteractiveServer
@inherits RoleDashboard<{RoleName}DashboardSummaryDto>

<DashboardFrame IsLoading="IsLoading" Failed="Failed" OnRetry="LoadAsync">
  <DashboardCard Title="…" Icon="…" IsLoading="IsLoading">
    @if (Summary is { } summary) { … }
  </DashboardCard>
  <DashboardCard Title="…" Icon="…" Emphasis="true" IsLoading="IsLoading" Count="@(Summary?.…)"> … </DashboardCard>
  <DashboardCard Title="…" Icon="…" Span="2" IsLoading="IsLoading"> … </DashboardCard>
</DashboardFrame>

@code {
  protected override IRequest<{RoleName}DashboardSummaryDto> QueryFor(ClaimsPrincipal user) => new Get{RoleName}DashboardSummaryQuery(user);
}
```

Inline `style="..."` is acceptable inside a dashboard for a per-instance value — `style="width:@percent%"` for a progress
bar fill — never for layout a class names: a row is `.list-row`, a row of buttons `.actions-cell` and a row of figures
`.dashboard-metric-row` (T328; 2026-09-27, T335, flow 01, which promoted the badge-row that had spread to four
dashboards).

Every dashboard uses `.dashboard-grid` + `DashboardCard` + the `.dashboard-metric` / `.progress-bar` / `.status-dot` primitives. Role-specific content lives inside the cards; the grid and card shapes do not.

A dashboard that reads nothing (the CollegeAdmin's, a card of links to the national catalogue it authors) has no query,
no `DashboardFrame` and no `@rendermode`: the grid and its cards are the whole file.

### Acting role

(2026-09-27, T335, flow 01; R2-Rules § 1–2; D1, W-010; T317.) A person who holds several roles acts as one of them at a
time. **The acting role chooses only what the frame shows**: the navigation, the landing (Home's dashboard) and the head.
Access is the union of the roles held and never changes with it: a page any held role admits opens whatever the acting
role, and following a link inside Wombat never switches it.

- **The rule.** The role stored with the account (`WombatIdentityUser.ActingRole`) while the person holds it; else the
  first role they hold in `DashboardPriority.Order` (Administrator, College admin, Institutional admin, Speciality admin,
  Sub-speciality admin, Committee member, Coordinator, Assessor, Trainee, Pending trainee); else none, and a person with
  none gets no "Acting as" head and no switch. It is **remembered per account, across sign-ins**, never in the browser:
  the next person to sign in on the same browser lands by the precedence (T317).
- **The resolver.** `Navigation/ActingRoleResolver` is the one implementation, a pure function of the stored choice and
  the roles held, which returns an `ActingRole` (the role, and every role held in precedence order). The choice reaches
  it as the sign-in cookie's `acting_role` claim (`WombatClaimTypes.ActingRole`, issued by
  `WombatUserClaimsPrincipalFactory`), so **the shell reads no database**. `App.razor` resolves it once per request from
  `HttpContext.User` and passes it to `Routes` as a parameter; `Routes` cascades it with `IsFixed`. Components take
  `[CascadingParameter] ActingRole`; nothing reads a cookie or `IHttpContextAccessor` for it inside a circuit.
- **The endpoint.** `GET /dashboard/switch/{role}?returnUrl=<local path>` (`Navigation/ActingRoleSwitch`), a GET so an
  email's link can use it (W-010), for a signed-in user only. A role the person holds is written through `UserManager`
  and the cookie issued again (`RefreshSignInAsync`); an unknown or unheld role writes nothing and says nothing. The
  return address is followed only when `LocalUrl.OrNull` passes it (one slash, not `//` or `/\`, no control
  character); anything else lands on `/`, the new role's Home. A character above U+007E is percent-encoded as UTF-8, as a
  browser sends it: as it arrived, decoded from the query, it went into the `Location` header, which Kestrel refuses
  outside printable ASCII, and the answer was a 500 (2026-09-27, T335, flow 01; the review of the t335 branch). Every
  caller of `LocalUrl` shares it: the sign-in, the institutional sign-in's callback, the link page, the session's end.
  The sidebar's switch is the same link without a return address. A session whose security stamp has changed is signed out, never given a new cookie. Not audited: it grants
  nothing.
- **Every switch link is a full page load:** `data-enhance-nav="false"`, `ActingRoleSwitch.Url(role, returnUrl)` for its
  address. On a static page (`/portfolio/verify`, `/msf/respond`, `/Error`) enhanced navigation would otherwise fetch
  the landing page and patch it in place, where `autofocus` does not apply, `FocusOnNavigate` takes the h1, and no fresh
  circuit starts. In a circuit the router cannot route the address, so it loads in full anyway.
- **The result.** After a switch that changes the frame, the page it lands on says so once, under its header:
  `ActingRoleSwitchAlert`, which `PageHeader` renders, shows an `info` `Alert` with `role="status"` inside an
  `ActionResult` with `FocusOnLoad`: "You are now acting as Assessor." Away from Home it adds an outline small link,
  "Switch back to {previous role}", to the same endpoint with this page (path and query) as the return address. The
  words and the link are an `.alert-row`: the words grow, so the link sits at the right (R2-Detail-Email).
  - The word travels as a short-lived HttpOnly, SameSite=Lax cookie, protected with Data Protection, bound to the
    account and taken once (`ActingRoleSwitchResults`). `App.razor` takes it and passes it to `Routes`, which cascades
    it; a forged, expired, replayed or someone else's word shows nothing, and so does one whose new role is not the
    acting role. Sign-out deletes it.
  - The first in-circuit navigation leaves the page, and the word with it.
  - Its focus wins over `FocusOnNavigate`'s h1: `FocusOnNavigate` sits beside `AuthorizeRouteView`, so it renders, and
    sends its focus, before any component of the page (`ActingRoleSwitchAlertTests`).
  - It works on an interactive page (the prerender and the circuit each show it) and on a static one.
  - **Said by one circuit** (2026-09-27, T335, flow 01; the review of the t335 branch). `Blazor.resumeCircuit()` starts a
    new circuit from the page's own descriptor, `Routes`' parameters and the word included, so a resumed page said it
    again. The word carries the switch's nonce, and the first circuit to render it records it with
    `ActingRoleSwitchResults.SayInCircuit` (for eight hours, longer than a circuit's state is kept to be resumed); a later
    circuit given the same word says nothing (`ActingRoleSwitchAlertTests`).
- **Names.** A role is shown by its sentence-case label, `WombatRoleLabels.For` (Domain): "Committee member", never
  "CommitteeMember". The signed-in person is shown by `ClaimsPrincipal.GetDisplayName()`, the `display_name` claim
  ("FirstName LastName", or the email when the account has no name), never read from the database by the shell.

### Account / auth page

```
<div class="account-form-container">
  <h2>Sign in</h2>
  @if (Refusal is not null) { <Alert Kind="danger" Id="login-error">@Refusal</Alert> }   @* role="alert" by default *@
  <form method="post" action="/account/login/submit">
    <div class="mb-3"> label + .form-control with its autocomplete token, aria-describedby="login-error" on a refusal </div>
    …
    <button type="submit" class="btn btn-primary">Sign in</button>
  </form>
</div>
```

`.account-form-container` is a 400px centred card with a wide top margin — the shape ClinicAssist uses for its login/register/change-password pages.

**A refusal travels as a code, and the page chooses the words** (T265, T285). Every endpoint that sends the browser back
to an account page with a refusal puts a code in `?error=`, never a sentence, and never an exception's message: sign-in
(`SignInOutcome`), link account (`LinkExternalOutcome`), register (`RegisterOutcome`) and change password
(`ChangePasswordOutcome`), each in `Wombat.Web/Security`. The page shows the sentence it holds for each code it knows,
and one general sentence for any other, so a crafted link (`/account/login?error=Call%20012`) cannot put words of its
choosing on Wombat's own page. The institutional sign-in's refusals are `ExternalLoginRefusal` codes, which the sign-in
and link pages read alike. An exception that is not a refusal is logged, and the page says the action could not be
completed. Until T285 the sign-in, link and register pages printed their `?error=` as it arrived. A refused registration
keeps the form under its refusal while the invitation can still be used, the first field taking the focus and every field
naming the refusal; an invitation that cannot be used says why and offers no form. Whether it can be used is the
invitation preview's answer, never the code's: the preview refuses whatever no input could put right, an invitation
revoked, used, expired or unknown and an address no account can be created for (one an account already holds, or one
Identity will not take as a user name), by the provisioner's own test. So the form never comes back under the same
refusal on every submit.

**A visitor who has not signed in gets static pages** (T181). `App.razor` gives them no render mode, so no page they
reach opens a circuit: the Blazor hub stays behind the fallback policy, because inside a circuit navigation never meets
an endpoint's policy. So everything on an `[AllowAnonymous]` page must work as plain HTML: links, form posts to a
minimal-API endpoint (as sign-in, register and forgot-password do), and `wombat.js` for any behaviour. `@onclick`,
`@bind`, `OnAfterRenderAsync` and JS interop do nothing there.

**A circuit whose account has changed is signed out within a minute** (T279). The circuit's sign-in is checked every
`SessionRevalidation.Interval` (`SessionRevalidatingAuthenticationStateProvider`); after a lock, an erasure, or a change
of roles, institution or scope, the circuit becomes anonymous and **the tab leaves it by a full page load** of
`/account/session-ended` (`SessionEnd`), never by navigation inside it (the T279 review). Two components send it, once
between them (`EndedSessionExit`, scoped to the circuit): `LeaveEndedSession`, rendered in `Routes.razor` outside the
router, the moment the sign-in ends on any page, and `RedirectToLogin` when it is rendered interactively; rendered
statically, `RedirectToLogin` still redirects to the sign-in page. The endpoint asks the account itself, whatever the
cookie's age: a session it no longer accepts, or cannot check, is signed out and sent to
`/account/login?error=SessionEnded&returnUrl=…`, which loads signed out, static, with a form and an antiforgery token of
its own, and says "Your session has ended. Please sign in again." in its danger `Alert`, named by both fields as every
sign-in refusal is. Signing in comes back to the page the tab was on. A session the account still accepts (signed in
again in another tab) goes straight back there. Until the review the sign-in page rendered inside the old circuit, and
its form posted the antiforgery token the circuit was given on its first page, naming the old user; the cookie was
refused at the post, so antiforgery refused the token and the endpoint answered a bare 400. A form inside a circuit
cannot outlive the sign-in it was rendered for, so a page must never keep a circuit going once it is anonymous.

A fault in the minute's check (the database restarting) is logged and asked again at the next interval; only the third
in a row signs the circuit out, so a restart does not send every open tab to the sign-in page at once.

The same page is **interactive for a signed-in visitor** unless it carries `[ExcludeFromInteractiveRouting]`. So a page
that posts back to itself (`@formname` + `[SupplyParameterFromForm]`) runs two ways. Signed out, the submit is an HTTP
post that the pipeline sees. Signed in, it is an `@onsubmit` event in the circuit: there is no post, so
`[SupplyParameterFromForm]` binds nothing, the cascaded `HttpContext` is null, and no per-request middleware (a rate
limit) or response status applies to it. Such a page carries `[ExcludeFromInteractiveRouting]` as well, as `/msf/respond`
does, or it posts to an endpoint instead.

Two predate the rule and are dead signed out: `PasswordToggleButton` (sign-in, register, link) and the register page's
`OnAfterRenderAsync` that clears the invitation token from the address. Neither ever worked signed out: the hub has
always refused an anonymous circuit. `/portfolio/verify`'s Verify button was the third. Since T265 that page carries
`[ExcludeFromInteractiveRouting]` and its form is a GET: Verify loads `/portfolio/verify?hash=…&check=1`, and the check
is made as the page renders, the same for every visitor. A check that fails says so on the page.

**The sign-in cookie is written only in an HTTP request** (T265). A circuit's response started when the page first
loaded, so `SignInManager` there throws "Headers are read-only". Sign-in, register, link-account, sign-out, change
password and My account's name each post a form to an endpoint in `Program.cs`, which writes the cookie and redirects. Change password stays
an interactive page, for its Show buttons. Its form, a plain `<form method="post" action="/account/change-password/submit">`
with `<AntiforgeryToken />`, is posted by the browser, which the circuit does not intercept. The endpoint changes the
password and issues the cookie again with the new security stamp, in one request. A separate "refresh the cookie"
endpoint would hand the new stamp to any cookie still inside the stamp validator's interval, a stolen one included. The
redirect back carries `?status=updated`, or a code for each refusal (`?error=PasswordMismatch`), never the words.
The page chooses the words (`ChangePasswordOutcome`), so a crafted link cannot put its own text in the page's alert.

**My account's name is saved the same way** (2026-09-27, T335, flow 01; the review of the t335 branch). The shell names
the person from the cookie's `display_name` claim and reads no database, so a name saved in the page's circuit left the
account row on the old name, in that circuit and in the cookie, until the stamp validator next rebuilt it. The page's form,
a plain `<form method="post" action="/account/profile/submit">` with `<AntiforgeryToken />`, the two names `required` and
`maxlength="100"`, posts to an endpoint that checks the session, saves the name (`UpdateCurrentUserProfileCommand`, for
the request's caller) and issues the cookie again in one request, then redirects to `?status=saved`, or `?error=` with a
code (`ProfileOutcome`: `NameMissing`, `NameTooLong`, `Failed`); the page chooses the words, takes the focus on the result
as it loads, and its title starts "Error: " after a refusal. The page loads in full, and its circuit starts from the new
cookie. `Account/ProfilePageTests` holds the page, `Hosting/ProfileFlowTests` the post, to the account row's new name.

An endpoint that issues the cookie again checks the session first (T265 review). The stamp validator looks at a cookie
once a minute (`SessionRevalidation`, T279; thirty minutes before it), so until then a session that has already ended
(an administrator's lock, a password changed in another browser, a change of roles) still reaches the endpoint. `SignInManager.ValidateSecurityStampAsync` refuses it,
and the endpoint signs it out rather than hand it the new stamp, which would keep it alive past the lock. A password
such an endpoint checks is checked as the sign-in page checks one: `CheckPasswordSignInAsync` with
`lockoutOnFailure: true`, under the sign-in throttle (`LoginRateLimitPolicy`), whose refusal goes back to the form the
post came from. An account that signs in through its institution (`AllowLocalPassword` false) has no password to
change. A fault before the change says the password could not be changed; a fault after it, in issuing the cookie,
signs the user out and says the password was changed.

### Anonymous static page (the MSF respondent page, T205)

A page for a stranger holding an emailed link, who never signs in: `/msf/respond`, where an MSF respondent answers. It
is the account page's shape, widened for a questionnaire, and it is the one page that is **never interactive**, signed
in or not:

```razor
@page "/msf/respond"
@attribute [AllowAnonymous]
@attribute [ExcludeFromInteractiveRouting]              @* static HTML: App.razor gives it no render mode *@
@attribute [EnableRateLimiting(MsfRespondRateLimit.PolicyName)]
@attribute [RequireAntiforgeryToken(required: false)]  @* the link is the post's authority *@
@layout Layout.AuthLayout

<PageTitle>@PageTitleText · Wombat</PageTitle>          @* one per state, its <h2>'s words; "Error: …" after a refused post *@
<div class="account-form-container account-form-container--wide shadow">
  brand lockup, <h2>, the state's Alert
  <div id="msf-error" class="error-summary" tabindex="-1" autofocus>  @* only after a refused post *@
    <Alert Kind="danger" Role="alert">…</Alert>
  </div>
  <form method="post" @formname="…" @onsubmit="…" data-submit-once>
    … fields, only while the questionnaire is shown …
  </form>
</div>
```

- `App.razor` renders `Routes` and `HeadOutlet` with no render mode for a page marked `[ExcludeFromInteractiveRouting]`
  and for a visitor who has not signed in (T181), and `InteractiveServer` for a signed-in user's every other page. The
  attribute is what keeps the page static for a respondent who happens to be signed in. Static, the page opens no
  circuit, the link's rate limit sees the submit (an HTTP post, not a SignalR message), and it works without WebSockets
  or script. `Hosting/MsfRespondPageHostingTests` loads it signed in for that reason.
- Fields are plain inputs named `Prefix.Key[id]`, read by `[SupplyParameterFromForm]`. Give them `@onchange`/`@oninput`
  too: bUnit renders the page interactively, and the handlers are how its tests fill the form.
- The `<form>` stays in the tree when it holds nothing. Blazor answers a post whose named form is not on the page with a
  bare 400, and a link can die between loading the page and posting it.
- `data-submit-once` (`wombat.js`) drops a second submit while the first is on its way: the link takes one response.
- A refusal is the page's own state, with the status the Api would answer (`MsfResponseRefusals.Describe`), set as the
  response starts: .NET 10 writes no body for a page that ends its render at 404.
- Every state has its own `<PageTitle>`, its `<h2>` in the same words ("Feedback link expired · Wombat", "Thank you ·
  Wombat", "Feedback on Nomsa Mahlangu · Wombat"; T190), and a refused post's starts "Error:". A refused post
  reloads the whole page, so its summary is an `.error-summary` that takes the focus as the page loads
  (`tabindex="-1"` + `autofocus`), and the question the refusal names points at it: the rating's
  fieldset with `aria-describedby`, a comment box with `aria-invalid`, `.input-validation-error` and
  `aria-describedby` (its help text, then the summary). Only that question.
- No antiforgery check and no token in the form: whoever holds the link can post without a browser, so the check
  protected nothing, and a browser that lost the cookie had its answers answered with a bare 400.
- The page sets `Referrer-Policy: no-referrer` (the app's own policy sent its address, token and all, as the Referer of
  every asset it loads), `X-Robots-Tag: noindex, nofollow` and `Cache-Control: no-cache, no-store`, in every state.
- Declare what the post can leave out. The form mapper sets a dictionary the post carried no key for to null, whatever
  it was initialised to: a radio group with nothing chosen posts nothing. Reset each to empty before the page reads it.
- It says what the product guarantees and no more. MSF anonymity runs one way: the respondent sees their own
  questionnaire and nothing of anyone else's answers, counts or thresholds. The comment boxes' help text says a comment
  may reach the trainee word for word, which the promise that name and email are hidden does not cover.

```css
.account-form-container--wide  /* 44rem card for a questionnaire; less padding at ≤640px */
.error-summary                 /* a refused post's summary, focused on load; a focus ring when focused (T205) */
```

### System pages

(2026-09-27, T335, flow 01: R2-Denied-*, R2-NotFound-*, R2-Error-*; the round-2 review, S1 to S6 and D6; T321.) Access
denied, Page not found and the error page tell a person that the page they wanted is not theirs, not there, or failed,
and give them the way on. They share one shape, and none of them, nor its shell, reads the database: each is what a
person sees when something else has gone wrong. The one read on their way is the pipeline's, not theirs: the sign-in
cookie's check reads the account once the cookie is over a minute old (T279). In the error page's rerun a check that
throws draws the page signed out (below; the review of the t335 branch, 2026-09-27, T335, flow 01).

- **Signed in:** a `PageHeader` with its icon (`Icon`, `IconTone`), and one `.system-panel` under it, at most 40rem
  wide: what happened, what to do, and the way on (`.actions-cell`, the first button primary). **Signed out:** the same
  header, in a `.system-card` centred under the signed-out bar, its icon above the heading and no rule. At 390px the
  buttons stand one under another, each the panel's width and 44px tall.
- **The icon** says the kind of page: `ban` in the danger colour for a refusal, `lock` for "sign in", `search` for not
  found, `triangle-alert` in the warning colour for a failure, `info` for "nothing went wrong". It is aria-hidden: the
  heading is named by its words.
- **"Go to Home"** is the way back, everywhere (until T335 "Back to home"). The tab and the heading are the same words.
- **No page names more than the person may know.** Access denied never names the page or the roles that would open it,
  and Page not found never echoes the address (a crafted link would put its words on a Wombat page, T285's class) or
  says anything about another institution's records, which are answered 404 so as not to be known to exist.

| Page | Heading | What it says | The way on |
|---|---|---|---|
| Access denied, signed in (`/access-denied`, or in place) | You cannot open this page | "Your role (Trainee) does not open this page." or "None of your roles (Committee member, Assessor) opens this page." (`SystemPageText.Refusal`, from the claims); then "If you need it for your work, ask your institution's Wombat administrator." ("… ask the platform administrator." for an Institutional admin, a College admin or an Administrator: their institution's administrator is themselves or nobody, S1) | Go to Home |
| Access denied, signed out | Sign in to open this page | "After you sign in, Wombat brings you back to the page you asked for." when `ReturnUrl` is a path on this site (`LocalUrl`) other than `/access-denied`; else "Sign in to carry on." | Sign in: `/account/login?ReturnUrl=<it>`, or `/account/login` |
| Page not found, signed in | Page not found | "There is no page at this address." "Check the address, or start again from Home." | Go to Home |
| Page not found, signed out | Page not found | "There is no page at this address." | Go to Home |
| The error page, after a failure (500) | Something went wrong | "Wombat could not finish this request. Try again. If it keeps happening, send this reference to your institution's Wombat administrator." (signed out: "… to whoever sent you the link, or to your Wombat administrator.", since an MSF respondent has no account); the reference block | Try again (the failed address), Go to Home |
| The error page, typed as `/Error` | Nothing went wrong | "This is Wombat's error page, opened directly. No request failed, so there is nothing to report." | Go to Home |

- **Access denied is drawn in place, once** (D6, T321). A full load of a page the person may not open is sent to
  `/access-denied` by the sign-in cookie's `AccessDeniedPath`; in-app navigation to one renders `<AccessDenied />` at
  the page's own address, from `AuthorizeRouteView`'s `NotAuthorized`, which is already drawn in the default layout.
  Until T335 `Routes.razor` wrapped it in a `LayoutView` of `MainLayout` as well: two sidebars, two top rows and two Sign
  out buttons. No switch of role is offered: access is the union of the roles held, so none could open it (S1).
- **The error page** (`Pages/Error.razor`) is static (`[ExcludeFromInteractiveRouting]`) and anonymous: it renders once,
  inside the failed request, so its reference is that request's, and a visitor who has not signed in sees it, not a
  sign-in form. `Navigation/ErrorPages` wires `UseExceptionHandler("/Error")`, with a scope of its own for the rerun,
  outside Development, for a browser's page load only (a GET that accepts HTML): a failed post or `fetch` keeps its bare
  500. The handler sits **before** `SecurityHeadersMiddleware` and the 404 rerun: it clears the failed response, headers
  and all, so behind them the page lost its CSP header (`ErrorPageFlowTests` holds the nonce).
  - **The reference** is the request's W3C trace id, 32 hexadecimal characters (`Activity.TraceId`), never the
    55-character traceparent, which wrapped at 390px and which no one searches a journal for; the request's own id where
    there is no trace. `ErrorPages.ReferenceLog` logs the failure once, with the exception and that reference, before the
    page renders; the middleware's own line, which names none, is suppressed when the page answered.
  - **The reference block** (`ReferenceBlock.razor`, `dl.reference-block`): the label "Reference" and "Time" muted, the
    trace id in `--font-mono`, wrapping anywhere, and the time with its zone, "2026-09-26 15:14 SAST", on `--header-bg`.
  - **Try again** is the failed address, its path and query from `IExceptionHandlerPathFeature`, when `LocalUrl` passes
    it, and a full page load (`data-enhance-nav="false"`). With none, Go to Home is the primary button. Signed in, the
    buttons carry their icons; signed out, in both states, none, as the signed-out Page not found and Access denied
    (R2-Error-Out).
  - **In a database outage** (2026-09-27, T335, flow 01; the review of the t335 branch). The rerun runs authentication
    again, and with the database down the cookie's check throws there as it did in the failed pass. Until the review
    nothing caught the second, so a signed-in person got the server's bare 500, T321's defect.
    `ErrorPages.GuardTheSignInCheck` wraps the check (`ConfigureApplicationCookie` in `Program.cs`): in the rerun only
    (`IExceptionHandlerFeature` present), a check that throws rejects the principal, so the page is drawn signed out, and
    it signs nothing out: the cookie is left as it was, as `SessionEnd` leaves a session it could not check, and is good
    again once the account can be read. The fault is logged as a warning; the failure itself is the request's, logged
    once with its reference.
  - **Sign out is a link on the error page** (the same review). The page is drawn in the rerun, where a form's token would
    be the failed pass's, whose antiforgery cookie the handler cleared with that pass's response, so the post could be
    refused. `MainLayout` draws Sign out there as a link to `/account/logout-confirm`, which draws its own form and
    token, with the button's classes.
- **Tests:** `Navigation/SystemPagesTests` (the words from the claims, one role, several, an administrator of anything,
  none; signed out with and without a page to return to; Not found signed in and out; the error page's states, and its
  icons signed in and none signed out; Access denied in the real `Routes`, drawn once), `Navigation/MainLayoutTests`
  (Sign out a link on the error page), `Hosting/ErrorPageFlowTests` in the integration suite (the 500, the page, its
  nonce, the reference logged once, counting every error line that carries the exception; signed out; the sign-in check
  throwing in the rerun, drawn signed out with the cookie kept; no page for a post or a non-HTML request; the typed
  state), and
  `Hosting/SignInReturnAddressFlowTests` (the sign-in's return address is followed only when `LocalUrl` passes it, and
  one outside ASCII percent-encoded, at the sign-in, the institutional sign-in's callback and the link page, the raw
  `Location` held to Kestrel's rule; `ActingRoleFlowTests` the switch's, `Security/LocalUrlTests` the rule itself).

```css
.page-title-with-icon          /* the h1 with its icon: flex, centred, a 12px gap */
.page-title-icon(--warning|--danger)  /* the icon's colour: the action blue, or the kind's */
.system-panel                  /* the signed-in panel: a card, a 12px column gap, at most 40rem */
.system-card-page, .system-card  /* the signed-out card, centred 80px under the bar, 580px, --radius-xl */
.reference-block               /* dl: label | value, --header-bg, the id in --font-mono; below 641px one column, label
                                  over value, padded 0.75rem, in a panel padded 16px (R2-Error-Narrow) */
```

## app.css section order

`app.css` is one file, but it has mandatory sections and section headers. Keep them in this order so two sessions don't re-sort and conflict.

```css
/* ── Fonts ─────────────────────────────────────────── */
@font-face (Source Sans 3 upright and italic, Fraunces)

/* ── Design tokens ─────────────────────────────────── */
:root { … }

body, button/input/select/textarea (font: inherit), .auth-page-shell, a, a:where(:hover), main

/* ── Base ──────────────────────────────────────────── */
h1..h5, .page-subtitle

/* ── Header + search ──────────────────────────────── */
.header-container (actions on the heading's baseline), .header-container h1 (no margin), .page-title-with-icon, .page-title-icon(--warning|--danger), .search-container, .search-input, .search-grid, .search-field, .search-hint

/* ── Tables ────────────────────────────────────────── */
.table-container, .clinic-table, .clinic-table tr.is-editing, .clinic-table--compact, .col-wrap, .col-fit, .col-actions, .clinic-table--inputs (+ @container inputs-grid), .actions-cell

/* ── Buttons ───────────────────────────────────────── */
.btn, :focus-visible (every element's ring), h1[tabindex="-1"]:focus, .btn-{variant}, .btn-outline, .btn-sm, .btn-xs

/* ── Forms ─────────────────────────────────────────── */
.form-container, .form-grid, .form-group (+ > p, > .btn), dl.form-group (+ > dd), .full-width, .form-control, .form-select, .form-select-sm, .form-check, .check-grid, .scale-choices, .form-actions, .account-form-container(--wide)

/* ── Alerts ────────────────────────────────────────── */
.alert, .alert-{kind}, .alert-row, .alert-row-text, .error-summary

/* ── Validation ────────────────────────────────────── */
.validation-message, .validation-summary-errors, .input-validation-error (+ .invalid, [aria-invalid="true"], forced colours), .field-warning

/* ── Cards ─────────────────────────────────────────── */
.detail-card, .detail-card--{variant}

/* ── Dashboard grid ───────────────────────────────── */
.dashboard-grid (+ > *, three/two/one tracks), .dashboard-span-{N}, .dashboard-card-title, .dashboard-card-skeleton, .list-row, .home-action (≤640px)

/* ── Details grid ─────────────────────────────────── */
.details-grid (+ responsive)

/* ── System pages ─────────────────────────────────── */
.system-panel, .system-card-page, .system-card, .reference-block (+ ≤640px)

/* ── Pager ─────────────────────────────────────────── */
.pager, .pager-info, .pager-actions, .pager-page-size, .pager-page-size-label, .pager-page-size-select

/* ── Password toggle ──────────────────────────────── */
.password-wrapper, .password-toggle-btn

/* ── State panels / skeletons ─────────────────────── */
.state-panel-title, .state-panel-copy, .skeleton, @keyframes skeleton-pulse

/* ── Utilities ─────────────────────────────────────── */
.shadow, .text-center, .mb-3, .font-mono, .code-block (a stored text block shown verbatim, T266), .visually-hidden

/* ── Accessibility ────────────────────────────────── */
fieldset, fieldset legend, fieldset.form-group > legend, fieldset.form-group fieldset.form-group (+ > legend),
@media (prefers-reduced-motion: reduce)
```

When a new section is needed (say `/* ── Badges ── */`), add its heading in alphabetical-ish order inside the existing block and keep the rest of the file untouched.

## Non-negotiables

- One `app.css`. One design system. Component-scoped `.razor.css` only for the layout shell and NavMenu.
- **Every class a page names is defined** (T266): by app.css, or by the component's own `.razor.css`, or it is `reload`
  or `dismiss` on the error bar's buttons, which blazor.web.js reads and nothing styles (2026-09-27, T335, flow 01:
  `dismiss` joined `reload` when the bar's 🗙 became a Dismiss button styled by `.btn` alone; `Design/ErrorBarTests`
  holds that no rule names either). A class the framework puts on at run time
  (an input's `invalid`) is not in a page's markup; a page that wrote one itself would be styling by it, so it must be
  defined. `NavItemLink` writes `active` itself, so `NavItemLink.razor.css` defines it. A class nothing defines styles
  nothing, silently. `Design/DefinedClassTests` reads every `class="…"` in every `.razor` file as Razor writes it (the
  words written out, the literals an `@(…)` can put there, and a
  word glued to an expression or an interpolation hole as the start of a class) and fails on any other. The start of a
  class passes only when it is a whole class itself (`form-select{InvalidClass}`), or in the one file whose expression a
  test holds to app.css (`Alert.razor`'s `alert-@Kind`, by the Alert-kind test, which reads a written-out `Kind` and the
  literals of a `Kind="@(…)"`): never because some defined class starts with it, which let `$"badge badge-{key}"` through
  (T266 review). An allowance the scan does not need fails it too. A class a method returns is out of its sight, which is
  why badges come from `BadgeFor` alone.
- No raw hex colours outside `:root`. No raw spacing outside the `--space-*` scale.
- No `<table class="table">`. Use `.clinic-table` wrapped in `.table-container`.
- Every list page renders loading / empty / error explicitly via `StatePanel` or equivalent.
- Every form page uses `.form-container` + `.form-grid` + `.form-actions`.
- Every dashboard page uses `.dashboard-grid`.
- `IScopedSender` only in interactive components.
- No Bootstrap Icons font. Inline SVG or CSS background-image data URIs.
- No MudBlazor, no Radzen, no jQuery. If a component needs JS, write a 10-line module under `wwwroot/js/` and import it with `IJSRuntime`.
  - **One exemption: the reconnect dialog's script** (`Components/Layout/ReconnectModal.razor.js`, 2026-09-27, T335,
    flow 01). It is the .NET template's collocated module, rewritten, and it must run when the circuit is gone, so it
    cannot be imported through `IJSRuntime`: `ReconnectModal.razor` loads it with a `<script type="module">` through
    `@Assets`. It is as long as the runtime's states make it (§ The reconnect dialog and the error bar).
- **Ask before widening the contract.** New component classes and new tokens are fine; silently deleting or renaming existing ones is not.

## Where this lives in the task graph

- **T010** ships the shell, `app.css` (every section above), `MainLayout`, `NavMenu`, and every `Components/Shared/*` component referenced here. Definition of Done for T010 includes "the list, detail, form, dashboard, and account page patterns above each render cleanly with stub data". Until T010 is done, the rest of the web surface has nothing to lean on.
- **T011** consumes `dashboard-grid` and `detail-card` only. No new global classes.
- **T019** consumes `detail-card`, `form-container`, `form-grid`, `form-actions`, `.btn`, `.clinic-table`, `.validation-summary-errors`. The builder's "Form tab" uses `.detail-card` for each section card and `.detail-card--interactive` for the hoverable add-field affordance. The JSON tabs use a `<textarea class="form-control" style="font-family: monospace">` inside a `.form-container`.
- Future UI tasks: open this file first. If a pattern you need isn't here, add it here in the same PR as the feature and reference the new section from the task file.

## Reference material

Two read-only reference trees informed this design system. Both were deleted from the worktree on 2026-09-20, long after T010 landed and `app.css` outgrew them. Where they went:

- **ClinicAssist.NET** — the gold standard the token names, class names, spacing scale and layout grid were ported from. Live working copy at `C:\Users\Renier\ClinicAssist.NET`; the files that mattered were `src/ClinicAssist.Web/wwwroot/app.css`, `Components/Layout/MainLayout.razor.css` and `Components/Layout/NavMenu.razor.css`. Copy structure, not palette.
- **The old Wombat** — its GUI was never a gold standard (Bootstrap-coupled tables, ad-hoc inline styles, no design-token discipline) but it **did ship and work**, and its `Views/` folder is the record of which pages users actually navigated for the Wombat-specific workflows ClinicAssist has no equivalent of (EPA management, curriculum items, activity builder, committee views). Recover it from this repo's history: `git worktree add ../wombat-old 55a92c6`.

Neither is needed to work on the design system now — `app.css` is the contract, and this file describes it in full.

## Historical context

The original ClinicAssist `app.css` was ~570 lines by the time its product shipped. Wombat's current one is 37. Every UI task that doesn't explicitly think about the design system leaks hand-rolled styles into its own `.razor` file, and six months later nothing matches. Porting the ClinicAssist skeleton in one go under T010 is cheaper than fighting that drift task by task.

The palette is still TBD. Everything else — token names, class names, spacing scale, layout grid — is locked to the ClinicAssist shape on purpose. Change the colours in `:root`, keep everything else.
