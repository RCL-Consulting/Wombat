# MainLayout

The signed-in shell: the NavMenu in a sticky 250px sidebar beside a main column whose sticky top row holds the user's name and Sign out, and whose `article` holds the page.

## What the consumer provides

Nothing: it is the default layout, and a page renders into `@Body`. The top row shows the signed-in user's name (their username, an email address) linking to `/account/profile`, and a Sign out form (`.btn .btn-outline .btn-sm`); signed out, a "Sign in" link.

## Structure (DESIGN.md § Layout grid)

`div.page > div.sidebar (NavMenu) + main > div.top-row.auth + article`, then the fixed `#blazor-error-ui` bar.

- From 641px (`breakpoint-wide`): a row; the sidebar is `sidebar-width` (250px), sticky, full height, painted with the sidebar gradient; the top row is sticky at 3.5rem on `background-color` with a `border-color` rule; the gutter is 2rem left and 1.5rem right.
- At 640.98px and below: a column; the sidebar stacks on top, the nav folds behind its toggler, the top row spreads its items apart, and the gutter is `space-md` (16px) each side.
- The page has a side gutter at every width (T226).

## Framework states to design

- `#blazor-error-ui`: "An unhandled error has occurred." with a Reload link and a dismiss glyph (U+1F5D9), fixed to the bottom on `lightyellow` with `error-bar-shadow`; `.reload` is read by blazor.web.js.
- The reconnect dialog (`#components-reconnect-modal`: rejoining, retrying, failed, paused, resume failed), styled in ReconnectModal.razor.css, not part of this system yet.

## Known gaps (design/BRIEF.md § 4, § 5.5)

- Sign out appears twice: in the top row and as the nav's Logout.
- The error bar's `lightyellow` and the sidebar's values in NavMenu.razor.css are raw colours outside :root.
- The shell is the first flow the restructure (W-008) redesigns.
