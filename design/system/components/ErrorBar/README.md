# ErrorBar

The in-app error bar, `#blazor-error-ui`: a warning-tinted bar fixed at the foot of the page, beside the sidebar, that the Blazor runtime shows when a page's circuit fails, with Reload and Dismiss.

Flow 01 (T335, b347e11c) rebuilt it from the template's `lightyellow` bar. Source: the foot of `Components/Layout/MainLayout.razor`, outside its `AuthorizeView`, so on every page; its rules are in `MainLayout.razor.css`.

## What the consumer provides

Nothing. The runtime shows it by setting an inline `display: block` on `#blazor-error-ui`, and wires its buttons by class: `.reload` reloads the page, `.dismiss` hides the bar. Nothing styles those two classes; the buttons look like buttons by `.btn`.

## Markup

```html
<div id="blazor-error-ui" role="alert" data-nosnippet>
  <div class="error-bar-row">
    <svg class="icon error-bar-icon" …>triangle-alert, 20px</svg>
    <p class="error-bar-text">This page no longer responds; copy anything you need, then reload.</p>
    <button type="button" class="btn btn-sm btn-primary reload">(refresh-cw, .error-bar-button-icon) Reload</button>
    <button type="button" class="btn btn-sm btn-outline dismiss">(x, .error-bar-button-icon) Dismiss</button>
  </div>
</div>
```

The row is on the inner `.error-bar-row`, because the runtime's inline `display: block` on the outer element would beat a flex row there.

## Look (DESIGN.md § The error bar)

- `warning-bg`, a 3px `warning-color` top edge, `shadow-bar`, `text-color`; the icon in `warning-color`.
- From 641px it starts at `var(--sidebar-width)`, beside the sidebar, and pads by the page's own gutters (32px left, 24px right); its buttons are small, with icons.
- Below 641px: the sentence first, then Reload and Dismiss side by side, each half the row at 44px, at a full button's 0.95rem and without their icons (R2-ErrorBar-Narrow).

## Rules

- No reference on the bar (D7): a per-circuit reference is later work. The error page has one (ErrorPage).
- `.reload` and `.dismiss` are the runtime's names: keep them, and style nothing by them.

## Contrast

Text 11.89:1 on `warning-bg`; the edge and icon 5.43:1; Reload's `on-fill` 4.86:1; Dismiss, filled with the surface, 4.86:1.
