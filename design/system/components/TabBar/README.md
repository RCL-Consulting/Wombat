# TabBar

A row of pill-shaped tab buttons over a hairline, used by the activity type builder to switch between Metadata, Form, Workflow and Credit.

There is no TabBar component: the builder writes `div.tab-bar` and a `button.tab-bar-tab` per tab, adding `.is-active` to the current one.

## What the consumer provides

The tabs' labels and which is active. Each tab is a `<button type="button">`; the active one is filled with `secondary-color` and labelled in `on-fill` (4.86:1); the others are `surface-color` with a 1px `border-color` edge and `text-color`, `radius-pill`, padding 0.5rem 1rem.

## Rules (DESIGN.md § Builder layout)

- The tab bar sits directly below the PageHeader, inside the builder's `.form-container`.
- The builder does not get its own design language: its panels reuse the card, form, button, alert and validation classes, and its two columns (`.builder-two-col`) stack when the card is under 46.5rem wide.

## Known gaps

- The tabs are plain buttons, not an ARIA tab list: no `role="tablist"`, `role="tab"` or `aria-selected`, so the active tab is shown by colour alone.
- An unselected tab is a `<button>` edged in `border-color` (1.30:1): the one control edged in a hairline. It is pre-restructure (flow 17); a new control takes `input-border`.
