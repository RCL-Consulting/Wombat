# Button

A `.btn`, at least 36px with a 1px edge on every variant, in one of five variants: `btn-primary` for the page's primary action, `btn-outline` (filled with the surface) for everything else, `btn-danger` only in a ConfirmDialog footer (or a note panel's move that ends an activity), `btn-success` for Publish, and `btn-quiet` (flow 03) for words on no ground.

There is no Button component: a page writes the classes on a `<button>` or an `<a>`. Class order is `.btn .btn-sm .btn-{variant}`, sizing before variant.

## What the consumer provides

- The element: a `<button type="button">` for an action, `type="submit"` in a form, or an `<a>` for navigation. Both are border-box and stand the same height (T328 measured them apart; flow 01 closed it).
- The variant: `btn-primary`, `btn-outline`, `btn-success`, `btn-danger` or `btn-quiet` (with `btn-quiet--danger` for Cancel). Size: none (36px), `btn-sm` (28px: row actions, the pager, the top bar's Sign out, a form page's "Back to EPAs") or `btn-xs` (28px: the Alert's ×).
- Optionally a 16px `Icon` before the label ("Create curriculum" with plus, "Log an activity" with circle-plus, "Try again" with refresh-cw), spaced by the button's `space-sm` gap.
- For a row action: an `aria-label` that starts with the visible label and names the row ("Edit PAED-001 — Providing paediatric emergency care to children").
- While its own action runs: `aria-disabled="@InFlight.AriaDisabled(running)"`, never `disabled`.

## Look (DESIGN.md § Button system)

- `.btn`: inline-flex, centred, `radius-md`, 0.95rem/600, padding `space-xs` `space-md`, min-height 2.25rem, a 1px transparent edge; hover dims to `brightness(0.95)` over `motion-fast`.
- `.btn-primary`: `secondary-color` fill and edge, `on-fill` words. `.btn-success`: `success-color`. `.btn-danger`: `danger-color`. Filled words are always `on-fill`, never `surface-color`.
- `.btn-outline`: `surface-color` fill, `secondary-color` edge and words: never transparent, so its label holds 4.86:1 on a tint or `header-bg` (S16).
- `.btn-sm`: 0.85rem, min-height 1.75rem, padding 0.125rem 0.625rem. `.btn-xs`: 0.7rem, min-height 1.75rem.
- `.btn-quiet` (flow 03, T342): words on no ground, underlined 0.2em below, `link-color`, with a transparent fill and edge. It is the quiet way out of a note panel ("Keep the request", "Keep the reflection", "Keep the review"; flow 04). `.btn-quiet--danger` turns its words `danger-color`: an activity's Cancel ("Cancel this draft…", "Cancel request…"), last in an action bar and pushed to its far end, or the status card's one action.
- `.btn.is-unavailable` (flow 03): a move the actor cannot complete from here, `aria-disabled="true"` and never natively disabled, with the not-allowed cursor. `.btn[aria-disabled="true"].is-running`: a move running, the progress cursor.
- Focus: the one `:focus-visible` rule, a 2px `focus-ring` outline at a 2px offset. On dark chrome (the phone menu's Sign out) `nav-focus-ring`.
- Disabled and in-flight: 65% opacity; in-flight shows the progress cursor and keeps its ring.

## Rules

- The primary page action is `.btn .btn-primary` and lives in the PageHeader's action slot. Home offers one per role at most.
- Row actions (Edit, Remove, View) are `.btn .btn-sm .btn-outline` inside `<div class="actions-cell">`, never `td.actions-cell`.
- Never Bootstrap's `.btn-outline-primary` and kin.
- Destructive actions open a ConfirmDialog first; the red `.btn-danger` appears only in its footer. (Pages flow 01 did not reach still break this: `.btn-danger` stands on the page in the activity type builder, the scale editor and the scales list, a College's, a speciality's and a sub-speciality's Deactivate, the entrustment decisions, invitations, SSO mappings and a user's page.) One more place, since flow 03: a note panel's move that ends the activity ("Decline with this note"), which the note already confirms.
- **An action bar with workflow moves** (`.form-actions--moves`, flow 03; ActivityWorkflowActions): the move that leads on first and filled, Save draft and Discard changes outlined, any other move outlined, Cancel last as `.btn-quiet .btn-quiet--danger`. One primary per bar. No top rule (the check line leads into it); below 641px it stacks, each button 44px, the move on top.
- A button is never disabled by its own action while it runs: it keeps the focus and says so with `aria-disabled="true"`. Every other button that action would race is disabled.
- A workflow move is labelled from its key in sentence case (`WorkflowTransition.LabelFor`: "Submit", "Record discussion", "Sign off"; a word written wholly in capitals, "MSF", is kept), since flow 04 (T350, round 1, E1); flow 03 had title case. Each move keeps its own label (flow 03, E4), adding " to <name>" only where the move hands the activity to the person a filled field names ("Submit to David Naidoo"). Quote it as it ships.
- A workflow move the actor may see but cannot complete is shown, never hidden, and **never natively disabled** (flow 03, A10): `.is-unavailable` with `aria-disabled="true"`, so it stays in the tab order, and its reason is visible text in a `.move-reasons` list under the bar, starting with the move's name, which the button names with `aria-describedby`. Discard changes with nothing to discard is the same, its reason "Nothing to discard yet." beside it in the bar's row (`.move-reasons--beside`, flow 04). The committee review and the MSF campaign pages (flows 07 and 10) still disable theirs natively over a `.workflow-action-reasons` list.

## Contrast

`on-fill` 4.86:1 on `secondary-color`, 5.88:1 on `success-color`, 5.95:1 on `danger-color`; `.btn-outline` 4.86:1 wherever it sits. `.btn-quiet` 6.70:1 on the surface and 6.36:1 on the page; `.btn-quiet--danger` 5.95:1 and 5.65:1. Until T335 the success and danger labels were 2.87 and 3.82:1.
