# TabBar

The tabs pattern: a row of tabs on the page ground, the selected one underlined, over the card that is its panel. Wombat's one tab bar is the activity type builder's (ActivityTypeBuilder): Details, Form, Workflow, Credit and, for a writer, Publish. Flow 17 (T398, 2026-11-10, 84bcffeb, with c9742a18; Q1, T280, S2, S21, S22, S24, R1; app.css's Builder section, C1) rebuilt it as a real tabs pattern. Until then it was a row of pill buttons (Metadata, Form, Workflow, Credit) inside the builder's card, the active one filled in `secondary-color` (`.is-active`), the others edged in `border-color` (1.30:1), with no tab semantics, so the active tab was told by colour alone.

There is no TabBar component: the builder writes the markup.

## What the consumer provides

```html
<div class="tab-bar" role="tablist" aria-label="Parts of this activity type">
  <button type="button" role="tab" id="tab-details" class="tab-bar-tab" aria-selected="false" tabindex="-1">Details</button>
  <button type="button" role="tab" id="tab-form" class="tab-bar-tab" aria-selected="true" tabindex="0" aria-controls="panel-form">Form <span class="badge badge-declined">1<span> problem</span></span></button>
  …
</div>
<div class="form-container" role="tabpanel" id="panel-form" aria-labelledby="tab-form">…</div>
```

- The bar names what the tabs are parts of (`aria-label`). Each tab is a `button[role="tab"]` with `aria-selected`, a roving `tabindex` (0 on the selected, -1 on the rest) and `aria-controls` on the selected only (the other panels are not in the page). No tab carries an `aria-label` or `aria-describedby`: its name is its words.
- **The panel** follows the bar: `role="tabpanel"`, `aria-labelledby` its tab, and `tabindex="0"` only when it holds no control, so the keyboard reaches a read-only panel.
- **The keys** (manual activation): Left and Right move the focus to the previous and next tab, wrapping; Home and End to the first and last; Enter or Space selects. The page reads the keys in C#; `wombat.js` keeps these four alone from scrolling the page (never Tab, so Tab leaves the bar for the panel and Shift+Tab comes back).
- **A count in a tab's name** (R1, M2): a tab whose part holds refusals carries `span.badge.badge-declined` after its words, the number then a `span` with " problem" or " problems", so the name is "Form 1 problem". The badge's `gap` (0.25em, c9742a18) stands for the word's leading space, which an inline-flex box drops ("1problem" until then).

## Look (app.css's Builder section)

- `.tab-bar`: a one-row grid (`grid-auto-flow: column`, tracks `max-content`), `space-xs` apart, one 1px `input-border` rule under it, on the page ground above the card.
- `.tab-bar-tab`: no fill and no edge; 44px high (`min-height: 2.75rem`), padding 0 `space-md`, `text-color` at 500, one line, a 3px transparent bottom border sitting on the bar's rule (`margin-bottom: -1px`). Hover: `header-bg` (the page ground is `hover-bg`'s value, so a hover there would not show).
- **Selected** (`[aria-selected="true"]`): a 3px `secondary-color` underline, `primary-color` words at 700. Told by the underline and the weight, never by a fill. In a contrast theme the underline is `Highlight`.
- The count's badge: `padding-inline: 6px`.

## Widths

The tracks go by the **column's** width, not the viewport's (T398's sweep: from 641px the sidebar leaves a 641px window 335px for the writer's five tabs' 416px). The builder's page is the size container (`.activity-type-page { container: builder-page / inline-size }`):

- **Under 34rem** of column the tabs take equal tracks (`minmax(0, 1fr)`), no gap, 0.9375rem, the label over its count, padding 0 2px: one row of five 72px tabs at 390.
- **Under 46rem** a count's word is visually hidden (`.visually-hidden`'s declarations on the badge's inner span), so the number alone shows and the name keeps "1 problem"; four "2 problems" counts with their word took the bar to 710px.
- The bar is **one row at every width**; the page never scrolls sideways for it (swept 641 to 1150px with counts standing).

## Rules (DESIGN.md § Builder layout)

- A tab press is not an action: it neither saves nor clears the page's result region (E02b).
- A new type opens on its first tab, an existing one on the tab most used (the builder's Form); a save keeps the tab; an action pressed from a tab lands on it (Q20).

## Contrast

The selected tab's words `primary-color` 10.42:1 on the page ground, its underline `secondary-color` 4.61:1; an unselected tab's words `text-color` 11.99:1, 11.36:1 on its hover; the bar's rule `input-border` 3.60:1 on the page ground. The count: `text-color` on `danger-bg` 11.81:1, its edge `danger-color` 5.56:1.

## Known gaps

- None found re-syncing. (Flow 17 closed both of the old record's gaps: the bar is a tab list, and no tab is edged in a hairline.)
