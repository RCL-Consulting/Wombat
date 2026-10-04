# EpaLabel

An EPA by name (`Components/Shared/EpaLabel.razor`): "Code — Title", the form every EPA surface uses, and the paused mark, "(no longer in use)", after it when the EPA is not in force now (T231, T255; D48). It renders no element of its own, so it sits in a table cell, a heading, a link or a sentence, and the heading's or link's accessible name carries the mark. Flow 05 (T355, 2026-10-04, b020c942; C5, C13) gave the mark its own class, `span.paused-mark`, so every row, heading and chart heading marks a paused EPA the same way; until then it was a plain `span.muted`.

## What the consumer provides

`<EpaLabel Code="@epa.EpaCode" Title="@epa.EpaTitle" InForce="@epa.EpaInForce" />`

- `Code` (required): the EPA's code, "PAED-012".
- `Title`: its title. A title that did not load leaves the code alone, never a dangling dash.
- `InForce` (required, so a page that names an EPA cannot leave out that it is paused): the flag a DTO carries for it (`ActivitySummaryDto.EpaInForce`, `EpaTrajectoryDto.EpaInForce`, `EntrustmentDecisionDto.EpaInForce`), the one the activity's own EPA picker reads (`Epa.IsActive`), so every page marks the same EPA the same way.

The words are the picker's (`EpaOptionLabel.For`, `EpaOptionLabel.NoLongerInUse`), so the label and the option cannot drift apart. `ActivityEpaLabel` wraps it for an activity's EPA cell (the Activity inbox's EPA column), with a dash when the activity is about no EPA.

## Markup

- In force: the text alone, "PAED-001 — Providing paediatric emergency care to children".
- Paused: the text, a space, then `span.paused-mark` "(no longer in use)".

`.paused-mark` is `muted-text` at weight 400: inside a heading it is never bold, so it reads as a mark on the name rather than part of it.

## Where it shows (flow 05)

- **The EPA page's h1** (through PageHeader's `TitleContent`): "PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams (no longer in use)"; the tab is the same words, mark included, " · Wombat" after them.
- **The rating trajectory's heading** (TrajectoryChart): on the EPA page "Rating trajectory (no longer in use)" (the chart writes the same `span.paused-mark` after its own title); on the committee page the EPA's name, through EpaLabel.
- **My progress's "No longer in use" group** (EpaProgressTable): each paused EPA's name as its row's link, on the paused row's `header-bg` ground.
- The standing panel and the index's own groups read only EPAs in force (D48), so they pass `InForce="true"`.
- Elsewhere: an activity's EPA cell (ActivityEpaLabel), My authorisations, and the Institutional admin's entrustment decisions.

## Rules (DESIGN.md § Table system, "the paused mark"; § Voice: "An EPA is 'Code — Title'")

- Name an EPA through EpaLabel wherever it may be paused; never write "(no longer in use)" in a page's own markup.
- The mark is words, muted, never a colour or a badge alone.

## Contrast

`muted-text` 5.09:1 on a card, 4.83:1 in an h1 on the page's ground, 4.57:1 on a paused row's `header-bg` (Spec § 4).

## Known gaps

- Two other forms of the mark ship beside it, under an activity's link rather than after an EPA's name: My activities' "PAED-006 (no longer in use)" (`.muted .text-sm`, T231) and the EPA page's Activities on this EPA (`span.activity-cell-paused`, muted 0.9rem). Neither is `.paused-mark` (T357).
