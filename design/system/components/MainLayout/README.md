# MainLayout

The signed-in shell: one `<header>` banner that is the sidebar from 641px (the brand cell, the role head, the scrolling nav list) with the account row fixed across the top of the page beside it, and `main > article` holding the page; one DOM at every width.

Flow 01 of the restructure (T335, b347e11c) rebuilt it. Below 641px the same markup is the phone bar and its menu: see MainLayoutPhone.

## What the consumer provides

Nothing: it is the default layout, and a page renders into `article`. It reads no database. The name in the account row is the sign-in cookie's display-name claim (`GetDisplayName()`: "FirstName LastName", or the email when the account has no name), and the role is the acting role that `App.razor` resolved from the cookie and `Routes` cascades. So a page drawn in a database outage, the error page's, still has its shell: signed out, when the rerun could not check the sign-in (ErrorPage).

## Structure (DESIGN.md § Layout grid)

```
div.page
  input#nav-toggle.nav-toggle            (the phone menu's CSS-only toggle; no tab stop from 641px)
  header.sidebar                         (the banner, at both widths)
    a.brand > img.brand-mark (32px) + span.brand-word "Wombat"
    p.phone-role                         (the phone bar's "Acting as"; hidden from 641px)
    label.nav-toggle-label               (the phone bar's Menu / Close; hidden from 641px)
    nav.nav-panel[aria-label=Main]       (NavMenu)
    div.account-row                      (the name, a link to My account, and Sign out)
  main > article                         (the page)
div#blazor-error-ui                      (the error bar: see ErrorBar)
```

- **From 641px** `.page` is a grid: `var(--sidebar-width)` (250px) beside `minmax(0, 1fr)`, a first row of 56px for the top bar, then the page.
- **The sidebar** is a sticky, full-height flex column (`height: 100vh`) painted with the gradient (180deg, `sidebar-gradient-start` to `sidebar-gradient-end` at 70%), so the gradient runs the window's height whatever the list's length. In it: the brand cell (56px, on `nav-brand-bg`, the whole cell the link home; the mark at 32px as an `<img alt="">` and the wordmark at 1.6rem, which names the link), the role head, and the list, which alone scrolls (NavMenu).
- **The top bar** is `.account-row`, the header's last child, `position: fixed` from `var(--sidebar-width)` to the window's right edge: 56px, `surface-color`, a `border-color` rule under it. Right-aligned: the person's name, a link to My account (`.account-link`, at least 32px, a 16px `user` icon, weight 600), then one Sign out, `.btn .btn-outline .btn-sm .sign-out-button` with its `log-out` icon, in a `form.sign-out` that posts to `/account/logout`.
- **The name** is the whole display name, in the link (so it is its accessible name) and its `title`; the bar cuts it at 28 characters (`max-width: 28ch`) with an ellipsis. On My account it is the current page: `aria-current="page"` and a 3px `secondary-color` underline (`inset 0 -3px 0`); in the phone menu's foot the underline is `nav-text-strong`.
- **On the error page** Sign out is a link to `/account/logout-confirm` with the same classes, not a form: the page is drawn in the failed request's rerun, whose antiforgery token could be refused.
- **The page** is `main > article`, padded 24px from the top bar and by the side gutter: 32px left and 24px right from 641px, 16px each side below it.

## Signed out

The static pages a visitor reaches (Access denied, Page not found, the error page, `/portfolio/verify`) get `div.page.page--signed-out`: a `header.signed-out-bar` (the same gradient at 90deg, 56px) holding the brand (the 32px mark and the 1.6rem wordmark, 10px apart) and a Sign in link (`.sign-in`, 0.95rem/600, a `log-in` icon, a `nav-group-label` edge, 32px), and no navigation. **Below 641px** the mark drops to 28px while the wordmark stays 1.6rem (the 28px rule is not scoped to `.sidebar`; probably a slip, see Logos), and Sign in grows to 44px with 12px sides. The sign-in, register, forgotten-password, link-account, sign-out and MSF respondent pages use AuthLayout instead.

## Rules

- **Every focusable control on dark chrome takes `nav-focus-ring`**, 2px at a 2px offset: the page's `focus-ring` is only 2.90:1 on the gradient's start. The brand cell's ring is inset (`outline-offset: -4px`), since the cell runs to the window's edge.
- **One Sign out.** The nav has no Sign out and no My account; the account row carries both, at every width.
- **Landmarks:** one `<header>` (the banner) holding the brand, the navigation and the account row, then `<main>`. No landmark relies on `display: contents`.
- **In a Windows contrast theme** My account's underline, a box-shadow, is drawn again as a 3px `CanvasText` text underline.
- **The page has a side gutter at every width** (T226), and a page's content never scrolls sideways; a wide table scrolls inside its `.table-container`.

## Contrast

`text-color` on the top bar 12.63:1. On the sidebar (the gradient's start, its worst point): `nav-text` 9.78:1, `nav-text-strong` 14.07:1, `nav-group-label` 8.00:1; the brand cell 17.52:1. `nav-focus-ring` 14.07:1 on the gradient. `Design/ContrastTests` measures every pair the shell's stylesheets paint, on the ground each sits on.

## Notes

- The Administrator's list (17 links) is longer than a short window's sidebar, so it scrolls; the lit item is scrolled into view as the page loads (`wombat.revealCurrentNavItem`, in `wombat.js`).
- Dark mode is later (W-011): `color-scheme` is `light`, and the tokens are shaped for a second theme (`on-fill`, the shadow tokens).
