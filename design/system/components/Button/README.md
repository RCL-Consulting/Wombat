# Button

A `.btn` with one variant: `btn-primary` for the page's primary action, `btn-outline` for everything else, `btn-danger` only in a ConfirmDialog footer, and `btn-success` for Publish.

There is no Button component: a page writes the classes on a `<button>` or an `<a>`. Class order is `.btn .btn-sm .btn-{variant}`, sizing before variant.

## What the consumer provides

- The element: a `<button type="button">` for an action, `type="submit"` in a form, or an `<a>` for navigation. An `<a class="btn">` renders taller and bolder than a `<button class="btn">` (it takes the body's font and line height); both are in use.
- The variant: `btn-primary`, `btn-outline`, `btn-success` or `btn-danger`. Size: none, `btn-sm` (row actions, the pager, a form page's "Back to EPAs") or `btn-xs` (the Alert's ×).
- Optionally an `Icon` before the label ("+ Create curriculum"), spaced by the button's `space-sm` gap.
- For a row action: an `aria-label` that starts with the visible label and names the row ("Edit PAED-001 — Providing paediatric emergency care to children").
- While its own action runs: `aria-disabled="@InFlight.AriaDisabled(running)"`, never `disabled`.

## Rules (DESIGN.md § Button system)

- The primary page action is `.btn .btn-primary` and lives in the PageHeader's action slot.
- Row actions (Edit, Remove, View) are `.btn .btn-sm .btn-outline` inside `<div class="actions-cell">`, never `td.actions-cell`.
- Never Bootstrap's `.btn-outline-primary` and kin; `.btn-outline` takes its colour from `secondary-color`.
- Destructive actions open a ConfirmDialog first; the red `.btn-danger` appears only in its footer. (The source breaks this in 15 places: row Delete and Remove buttons in the builder, the scale editor and the scales list.)
- A button is never disabled by its own action while it runs: it keeps the focus and says so with `aria-disabled="true"`, which the stylesheet dims (opacity 0.65, progress cursor) while keeping its focus ring. Every other button that action would race is disabled.
- A workflow action the actor may take but cannot complete is disabled, never hidden, and its reason is visible text in a `.workflow-action-reasons` list under the row, starting with the action's name; the button names it with `aria-describedby`.
- Minimum height is `target-min` (1.75rem), above the 24px WCAG target.

## States

Hover dims to brightness 0.95 over 0.15s. Focus is the 2px `focus-ring` outline, offset 2px. Disabled and in-flight are 65% opacity.

## Contrast

`btn-primary`: `surface-color` on `secondary-color`, 4.86:1, passes. `btn-outline`: `secondary-color` on white, 4.86:1. **Fails, kept as the source has it (T322):** the `btn-success` label, 2.87:1, and the `btn-danger` label, 3.82:1.
