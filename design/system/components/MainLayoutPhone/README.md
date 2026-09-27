# MainLayoutPhone

MainLayout below 641px: the header is the phone bar (the brand, "Acting as" and the role, and a Menu toggle), and the menu it opens takes the page's place, with the account row pinned at its foot; it is the same DOM as MainLayout, laid out by the same stylesheet.

## What the consumer provides

Nothing: see MainLayout. This card exists because a preview's width is its viewport, so the phone layout needs a 390px card of its own.

## Structure (DESIGN.md § Layout grid, R2-Phone-*)

- **The phone bar**, on `sidebar-gradient-start` as a solid, a grid of three: the brand (the mark at 28px, the wordmark at 1.4rem, at least 44px tall), then `p.phone-role` ("Acting as" in `nav-group-label` at 0.75rem/600, over the role's label in `nav-text-strong` at 0.9rem, right-aligned, at most 120px wide: "Sub-speciality admin" takes two lines and the bar grows, never cut), then the toggle. Someone with no role has no `phone-role`.
- **The toggle is CSS only.** `input#nav-toggle.nav-toggle` is a visually hidden checkbox, the first child of `.page`, so every part of the shell is its later sibling. Its `label.nav-toggle-label` in the bar is the 44px target: `menu` icon and "Menu", or `x` and "Close" while open, with a `nav-group-label` edge. The checkbox is the control a screen reader hears, checked or not, so nothing carries `aria-expanded`; its focus ring is drawn on the label (`.nav-toggle:focus-visible ~ .sidebar .nav-toggle-label`). It is keyed by a count of the circuit's navigations, so every navigation replaces it, unchecked, and the next page never opens under the menu.
- **Open** (`.nav-toggle:checked`): the bar keeps the brand and Close (its ground `nav-hover-bg`), the role leaves the bar for the panel's head, `main` is hidden, and the header takes the window's height (`100dvh`): the panel (the role head, the switch at 44px, the list with 44px rows, which scrolls), then **the account row pinned at its foot** on `sidebar-gradient-end`, under a `nav-divider` rule: the name whole and wrapping (`nav-text-strong`; on My account a 3px `nav-text-strong` underline), and Sign out at 44px with a transparent ground and a `nav-group-label` edge. It is the same element and the same Sign out form as the desktop top bar.
- **Home's header action** (the Trainee's "Log an activity", the Institutional admin's "Invite a person") is its own 44px row the width of the page, under the header's rule (`.home-action`): the rule is drawn under the heading block instead.
- **A trail folds** to one 44px link to its parent, led by a back chevron (`.breadcrumb-parent`; the ancestors, the current crumb and the separators are hidden).
- **The gutter** is `space-md` (16px) each side, and the page starts 16px under the bar. The h1 is 1.375rem.

## Rules

- Every control on the bar and in the menu takes `nav-focus-ring`; every target in the open menu is 44px.
- Only the list scrolls between the fixed head and the pinned foot; without `:has()` the page keeps its `100vh` behind the menu.

## Contrast

On the bar (the gradient's start): the brand and role 14.07:1, "Acting as" and the toggle's edge 8.00:1, the ring 14.07:1. On the menu's foot (the gradient's end): the name 16.19:1, Sign out's edge 9.21:1, the ring 16.19:1.
