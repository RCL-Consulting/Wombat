# RatedLevelPicker

The rater's rung picker (`Components/Shared/Activities/RatedLevelPicker.razor`): the writer of the rated level field chooses on the rung row itself, one native radio per rung, the row as wide as its ladder, the chosen rung filled and its descriptor under the row, and the College's descriptors under "What each rung means". It is RungRow made writable. Flow 04 (T350, 2026-09-30, 06aa51d7; Q5; round 2, E3 and C7; R4) made it; until then the rater chose a level from a select of the ladder's labels. Named for the rated level, not "RungPicker", which is the curriculum item form's own component.

## What the consumer provides

ActivityForm draws it; a page does not. It does so only on the schema's `rated_level_field`, only for a reader who may write it, and only when the page's ladder loaded (`Rungs`). Every other scale field (the demo types carry up to six), a ladder that failed to load, Log an activity and the builder's preview keep the select (E3).

```razor
<RatedLevelPicker FieldKey="@field.Key" Label="@field.Label" Rungs="Rungs!" Value="@GetString(field.Key)"
                  Required="@field.Required" HelpText="@field.HelpText" DescribedBy="@DescribedBy(field)"
                  Message="@FieldMessageFor(field.Key)" Invalid="@IsMarkedInvalid(field.Key)"
                  OnChoose="value => UpdateValue(field.Key, value)" />
```

- `FieldKey`, `Label` (required): the field's key (the radios' `name`, the root of every id) and label (the group's legend).
- `Rungs` (required): the ladder, lowest first, with the College's descriptors, as RungRow takes it.
- `Value`: the chosen rung's **Order**, as text: 5 is the rung labelled "4". Empty for none.
- `Required`, `HelpText`, `DescribedBy`, `Message`, `Invalid`: as the form gives every field.
- `OnChoose`: a rung was chosen. A press is one round trip through the form's update; the descriptor under the row is the server's render.

## Markup

A `div.form-group.full-width` holding:

- **The group:** `fieldset.form-group.rung-picker` (`min-width: 0`, so six cells share whatever width the section has, at 390 too), `aria-describedby` the help first. Its `legend` is the field's label with the `.required-mark` " *" and a visually hidden " required" (a group cannot carry `aria-required`). The help under it is `small.page-subtitle` with id `<key>-help`.
- **The row:** RungRow's `ol.rung-row`, each `li` keyed by its rung's Order, holding `label.rung.rung-choice` round one native `input type="radio"` (`name` the key, `value` the Order) above the rung's label: a 3.5rem cell, at every width. The chosen cell adds `.is-chosen` (filled `secondary-color`, words `on-fill`) and `span.rung-check` "chosen", `aria-hidden` (the browser says "checked"). The radio stays native, coloured by `accent-color`: `secondary-color`, and `on-fill` on the chosen rung. A cell hovers to `hover-bg`; the focus ring is on the cell (`:focus-within`).
- **Ids:** the first radio carries the field's input id (`<key>-in`), so a refusal summary's link lands on the field; the rest `<key>-in-<Order>`. Each radio is described by its rung's descriptor, a visually hidden `span#rung-desc-<Order>` inside the fieldset and outside both folds (a rung with no descriptor names none), and while the field is refused by its message (`<key>-msg`) too.
- **Refused:** the group, always `role="radiogroup"`, takes `aria-invalid="true"` (ARIA permits it on the group, not on a radio), and `p.validation-message#<key>-msg` sits under the row. Only those two are the refusal's.
- **The chosen rung's descriptor** under the group: `p.rung-descriptor`, as RungRow's: "**3b** Indirect supervision, reviewed on the trainee's initiative. …".
- **"What each rung means":** `details.rung-legend` (a hairline frame, `radius-md`), its `summary` 44px at every width: a 16px `chevron-right` (`.rung-legend-marker`) that turns down while open, "What each rung means", and, pushed right, Show while shut and Hide while open (`span.section-fold-show` holding `.fold-show` and `.fold-hide`). Its body is `dl.rung-legend-list`, a row a rung: the label in bold in a 2rem column, the College's words beside it. **With no rung chosen it is drawn twice**, read-only and with no ids: `.only-wide`, open, from 641px, and `.only-narrow`, shut, below it; CSS shows one, never a script (E2). **Once one is chosen**, it is drawn once, shut.

The browser's radio group does the keys: arrows move and choose, Tab leaves the group, and with none chosen Tab lands on the first rung.

## Rules (DESIGN.md § Form system, "The activity form", R4)

- The rung is shown by the College's label and stored as its Order.
- Only the rated level field, only on a loaded ladder; anything else keeps the select.
- What is open at 1280 and shut at 390 is two renderings toggled at 641px by CSS (`.only-wide`, `.only-narrow`), never script.

## Contrast

A rung's label `text-color` on the surface 12.63:1, its edge `input-border` 3.80:1; the chosen rung `on-fill` on `secondary-color` 4.86:1; the unchecked radio's ring is the browser's (Chromium's #767676, 4.54:1 on the surface), not a token; the descriptor `text-color` on `info-bg` 11.84:1; Show and Hide `link-color` 6.70:1.

## Known gaps

- The descriptors are `visually-hidden` spans, so a screen reader reading down the page hears each after the row and again in "What each rung means"; `hidden` would keep them for `aria-describedby` alone (T352).
- Three quick arrow presses replay three renders in order, so the "chosen" mark and the descriptor trail the focus for a moment (T352, mention only).
- The help under the legend is `small.page-subtitle`, where every other field on the activity form has `p.field-help` (flow 03). Both are muted 0.9rem, so they look alike; the element differs.
