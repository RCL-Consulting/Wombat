# T048 — Suppress programmatic-focus ring on page h1

Follow-up item #2 from the post-T047 backlog: "h1 focus-ring rectangle on initial render. Pre-existing cosmetic issue noted since T037. Decide intent (screen-reader announcement vs unwanted styling) before suppressing."

## Investigation

`Routes.razor:17` has `<FocusOnNavigate RouteData="routeData" Selector="h1" />`. Blazor's `FocusOnNavigate` component adds `tabindex="-1"` to the first matching element after every navigation and programmatically focuses it so screen readers announce the new page. This is correct accessibility behavior and must stay.

The cosmetic bug: Chrome's `:focus-visible` heuristic matches on this programmatically-focused `tabindex="-1"` h1 (confirmed by `h1.matches(':focus-visible')` returning `true` after login). The browser draws its default focus outline, rendering a black rectangle around the welcome text of every dashboard and around every page's h1 after navigation.

First attempt: `h1[tabindex="-1"]:focus:not(:focus-visible) { outline: none }`. Didn't work — the browser's own heuristics decided `:focus-visible` should match after form submission (click Sign in → redirect + programmatic focus is seen as keyboard-path-adjacent). Rule guard inverted the intent.

## Decision

**Suppress the outline unconditionally on `h1[tabindex="-1"]:focus`.** Since `tabindex="-1"` explicitly removes the element from tab order, there is no legitimate keyboard path by which a user can focus an h1. Every focus event on that element is programmatic, and sighted users don't need a visual indicator for a screen-reader announcement. Blazor's `.focus()` call still fires the accessibility event — we're only silencing the browser-default outline.

## Change

`src/Wombat.Web/wwwroot/app.css` — added one rule immediately after the existing `:focus-visible` block:

```css
h1[tabindex="-1"]:focus {
  outline: none;
}
```

Comment explains the FocusOnNavigate context so a future contributor doesn't "fix" it back.

## Verification

Browser-confirmed on three pages after the CSS change and dev server restart:

- `/` (Home, Administrator dashboard) — "Welcome, admin@wombat.local" now renders cleanly.
- `/admin/audit` — "Audit log" h1 clean.
- `/admin/institutions` — "Institutions" h1 clean.

`getComputedStyle` on the focused h1 shows `outline-style: none` (confirming the rule applied) while `matches(':focus')` remains `true` (confirming Blazor still focuses it).

## Out of scope

- Any change to `Routes.razor` / `FocusOnNavigate`. The accessibility behavior is intentional.
- Altering `:focus-visible` behavior for interactive elements. The existing `.btn:focus-visible` / `.form-control:focus-visible` / `a:focus-visible` rules stay.

## Definition of done

- No black rectangle on h1 after navigation.
- Screen-reader announcement path (Blazor programmatic focus) unchanged.
- Build clean, Web tests pass.

## Files touched

- `src/Wombat.Web/wwwroot/app.css`
- `Rewrite/Tasks/T048-suppress-h1-focus-ring.md` (this file)
- `Rewrite/current_state.md`
