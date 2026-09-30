# RungRow

The entrustment ladder, read-only (`Components/Shared/Activities/RungRow.razor`): where the reader cannot write a scale field, its value is shown on its ladder, every rung abreast, the chosen one filled and named, and its descriptor under the row. Before the section is filled in, the same ladder is pending. Flow 03 (T342, 2026-09-29, 725237ee; B12, A12) made it; until then a rating the reader could not change was a disabled select.

## What the consumer provides

`<RungRow Label="@field.Label" Rungs="rungs" Value="@stored" />`, or `Pending="true"` for a section not filled in yet. ActivityForm draws it; a page does not.

- `Label` (required): the scale field's label, which names the list ("Supervision required for this encounter").
- `Rungs` (required): the ladder, lowest first, as `EntrustmentRung(Order, Label, Description)` from `IActivityReferenceDataService.GetEntrustmentScaleRungsAsync`. The College's v11.1 ladder is six rungs: 1, 2, 3a, 3b, 4, 5.
- `Value`: the stored value, **the chosen rung's Order**, not its label: 5 is the rung labelled "4".
- `Pending`: the ladder with nothing chosen.

## Markup

- `ol.rung-row[aria-label]`: a grid of equal columns, as many as the ladder has, 8px apart (4px below 641px). Each `li.rung` is at least 44px (2.75rem), its label at 1.1rem/700, centred, on the surface in a 1px `input-border` edge, `radius-md`.
- **Chosen:** `li.rung.is-chosen`, filled `secondary-color` with `on-fill` words, and named in words, not by colour alone: its label, a visually hidden ", ", then `span.rung-check` "chosen" (0.75rem/600), so it reads "4, chosen".
- **The descriptor**, when the chosen rung has one: `p.rung-descriptor` under the row, body text on `info-bg` with a 4px `secondary-color` stripe, `radius-sm`, 8px by 16px: the rung's label in bold on its own line, then the College's words. "**4** Unsupervised practice. The trainee carries the responsibility for the activity."
- **Pending:** `li.rung.rung--pending`, dashed, transparent on the locked section's ground, `muted-text` at weight 600, each `aria-hidden`: a screen reader hears the ladder once, from the list's name, "Supervision required for this encounter: 1, 2, 3a, 3b, 4, 5", instead of six empty items. No descriptor.

## Rules (DESIGN.md § Form system, "The activity form")

- The rung is shown by the College's label, found by its Order.
- A writer of the scale keeps the select; rating by the rungs themselves (radios) is flow 04's.

## Contrast

The chosen rung `on-fill` on `secondary-color`, 4.86:1; a rung's words `text-color` on the surface, 12.63:1, its edge `input-border` 3.80:1; a pending rung `muted-text` on the page, 4.83:1, its dashed edge 3.60:1; the descriptor `text-color` on `info-bg`, 11.84:1, its stripe 4.55:1.
