# Button

A `.btn`, at least 36px with a 1px edge on every variant, in one of four variants: `btn-primary` for the page's primary action, `btn-outline` (filled with the surface) for everything else, `btn-danger` only in a ConfirmDialog footer, and `btn-success` for Publish.

There is no Button component: a page writes the classes on a `<button>` or an `<a>`. Class order is `.btn .btn-sm .btn-{variant}`, sizing before variant.

## What the consumer provides

- The element: a `<button type="button">` for an action, `type="submit"` in a form, or an `<a>` for navigation. Both are border-box and stand the same height (T328 measured them apart; flow 01 closed it).
- The variant: `btn-primary`, `btn-outline`, `btn-success` or `btn-danger`. Size: none (36px), `btn-sm` (28px: row actions, the pager, the top bar's Sign out, a form page's "Back to EPAs") or `btn-xs` (28px: the Alert's ×).
- Optionally a 16px `Icon` before the label ("Create curriculum" with plus, "Log an activity" with circle-plus, "Try again" with refresh-cw), spaced by the button's `space-sm` gap.
- For a row action: an `aria-label` that starts with the visible label and names the row ("Edit PAED-001 — Providing paediatric emergency care to children").
- While its own action runs: `aria-disabled="@InFlight.AriaDisabled(running)"`, never `disabled`.

## Look (DESIGN.md § Button system)

- `.btn`: inline-flex, centred, `radius-md`, 0.95rem/600, padding `space-xs` `space-md`, min-height 2.25rem, a 1px transparent edge; hover dims to `brightness(0.95)` over `motion-fast`.
- `.btn-primary`: `secondary-color` fill and edge, `on-fill` words. `.btn-success`: `success-color`. `.btn-danger`: `danger-color`. Filled words are always `on-fill`, never `surface-color`.
- `.btn-outline`: `surface-color` fill, `secondary-color` edge and words: never transparent, so its label holds 4.86:1 on a tint or `header-bg` (S16).
- `.btn-sm`: 0.85rem, min-height 1.75rem, padding 0.125rem 0.625rem. `.btn-xs`: 0.7rem, min-height 1.75rem.
- Focus: the one `:focus-visible` rule, a 2px `focus-ring` outline at a 2px offset. On dark chrome (the phone menu's Sign out) `nav-focus-ring`.
- Disabled and in-flight: 65% opacity; in-flight shows the progress cursor and keeps its ring.

## Rules

- The primary page action is `.btn .btn-primary` and lives in the PageHeader's action slot. Home offers one per role at most.
- Row actions (Edit, Remove, View) are `.btn .btn-sm .btn-outline` inside `<div class="actions-cell">`, never `td.actions-cell`.
- Never Bootstrap's `.btn-outline-primary` and kin.
- Destructive actions open a ConfirmDialog first; the red `.btn-danger` appears only in its footer. (Pages flow 01 did not reach still break this: `.btn-danger` stands on the page in the activity type builder, the scale editor and the scales list, a College's, a speciality's and a sub-speciality's Deactivate, the entrustment decisions, invitations, SSO mappings and a user's page. The activity page's workflow row breaks the next two rules: every move, and Apply, is `.btn-primary`, so the row holds several primaries, and while a move runs every one of them is `disabled` (`disabled="@Busy"`, ActivityWorkflowActions), the pressed one included, so it drops the focus. Flows 03 and 04 redesign it.)
- A button is never disabled by its own action while it runs: it keeps the focus and says so with `aria-disabled="true"`. Every other button that action would race is disabled.
- A workflow move is labelled from its key in title case (`WorkflowTransition.LabelFor`: "Sign Off", "Return"), not sentence case, until flows 03 and 04 decide; quote it as it ships.
- A workflow action the actor may take but cannot complete is disabled, never hidden, and its reason is visible text in a `.workflow-action-reasons` list under the row, starting with the action's name, which the button names with `aria-describedby`.

## Contrast

`on-fill` 4.86:1 on `secondary-color`, 5.88:1 on `success-color`, 5.95:1 on `danger-color`; `.btn-outline` 4.86:1 wherever it sits. Until T335 the success and danger labels were 2.87 and 3.82:1.
