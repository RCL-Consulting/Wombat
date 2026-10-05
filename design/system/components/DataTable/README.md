# DataTable

The list page's table: a `.clinic-table` in a scrolling `.table-container`, with a header row, one row per item, and row actions named by their row.

## What the consumer provides

```razor
<DataTable Items="_items" Caption="…">
  <HeaderRow><tr><th>Type</th>…<th><span class="visually-hidden">Actions</span></th></tr></HeaderRow>
  <Row Context="item"><tr><td>…</td><td><div class="actions-cell"><a class="btn btn-outline btn-sm" aria-label="View …">View</a></div></td></tr></Row>
  <Empty>…</Empty>
</DataTable>
```

`Items`, `HeaderRow` and `Row` are required; `Caption`, `Empty` and `Stack` are optional. It renders `div.table-container.shadow > table.clinic-table`. Most pages wrap it in a `StatePanel` for loading, error and empty.

`Stack` (flow 03, T342, A8) makes it **the stacked table**, `table.clinic-table.clinic-table--stack`: below 641px each row is a block of its cells, one under another, instead of a table that scrolls sideways. See below.

## The list page shape (DESIGN.md § Table system; § Page-level patterns)

PageHeader (title, subtitle, primary action) → `.search-container` with a `.search-grid` of labelled `.search-input` fields → the table → `PagerControls`. Since flow 06 a list's filters are applied with Show, its heading counting the answer (FilterBar).

## Column and row classes

- `.clinic-table--compact`: half the cell padding (`space-sm`), for eight or more columns or a grid of inputs.
- `.col-wrap` (and `--wide` on its `th`): a long-text column that wraps first, down to a 4rem (5rem) floor, and asks for 14% (20%) of the table.
- `.col-fit`: as narrow as its content, on one line (an ordinal, a code). Not for buttons.
- `.col-actions`: a row's buttons, 12rem for three small buttons.
- `.clinic-table--inputs`: every row is a set of inputs; cells centre, no hover tint; under 44rem of container its buttons stack.
- `tr.is-editing`: a row open for editing and its spanning form row below, tied by a 4px `secondary-color` stripe (`inset 4px 0 0`) on the first cell of each.
- A table grouped by one column puts each group in its own `<tbody>`, opened by `<th scope="rowgroup" rowspan="n">`.

## The stacked table (DESIGN.md § Page-level patterns, "List page")

My activities' All activities was the first: the columns Activity, Who has it now, State and Credit. Flow 04 stacked both of the Activity inbox's tables, and flow 05 writes the same markup out by hand (DataTable cannot caption a group or head a row) for My progress's index (EpaProgressTable), the standing panel (EntrustmentStandingPanel), the trajectory's table (TrajectoryChart) and the EPA page's Activities on this EPA (EpaPage); those make the EPA, or the encounter, each row's header (`th scope="row"`, `display: block` when stacked), where My activities has none.

- **Below 641px** each `tbody tr` is a grid of its cells, 4px apart, padded 8px by 16px, ruled from the next; the cells lose their padding and rules.
- **The header row stays in the table**, visually hidden (never `display: none`), so a screen reader still has each cell's column.
- **The roles are explicit**: DataTable writes `role="table"` on the table and `role="rowgroup"` on its head and body, and the caller's rows carry `role="row"`, `role="columnheader"` and `role="cell"`, since a row laid out as a grid loses its table semantics in some browsers.
- **A cell whose value does not say what it is** carries `data-label`, its column's name, drawn before the value at weight 600 in `muted-text` as `content: attr(data-label) ": " / ""`: the empty alternative text keeps the name from being read twice. A cell that says what it is (the activity's link, the state's badge) has none.
- The Activity cell is an `ActivityLink` (NeedsYouList describes it): the activity's name, "Type · EPA · date", and on a second line whom it goes to. An EPA not in force now is marked under the link, "PAED-006 (no longer in use)" (`.muted .text-sm`, T231).
- **Who has it now** reads "You", the holder's name, "Waiting for <state label>." (a move a role holds, nobody named), "Done" or "Closed" (`ActivityListWords.WhoHasIt`). **Credit** is `CreditOutcome.Label`: "1 item", "None", "—". Since flow 05 (C7, Q7) a credit to an EPA whose page opens is a link to it, `a.credit-link` "1 item", named "1 item to PAED-001, in My progress" (`ProgressLinks.CreditTo`), a 44px block below 641px; "None", "—", and a credit to an EPA of no curriculum she now holds stay text.
- The list is headed by an `h2.list-section-title`, "All activities (12)", which takes the focus after a page turn (`tabindex="-1"`): the pager's button may be gone from under it. `PagerControls` under it, 20 a page.

**Stacked from 900px** (flow 06, T358, 94edf2b7; A.7.8a): `.clinic-table--stack-wide`, added beside `.clinic-table--stack`, stacks the same way **at 900px and below**, where the dashboard grid goes to one column too. From 641 to 900px the sidebar leaves the main column too narrow for Programme trainees' six columns and Waiting for assessors' four, so the names split mid-word. The two lists write it out by hand, as flow 05's tables do; a row's header (`th scope="row"`, the registrar) is `display: block`, and a cell carries `data-label` as below 641px.

## Rules

- Every `<table>` is a `.clinic-table` in a `.table-container`; never Bootstrap's `.table`, and no inline column widths.
- An actions column's header is a `.visually-hidden` "Actions", never an empty `<th>`. A column no row offers an action in is not rendered.
- Every row's button carries an `aria-label` that contains its visible label and names the row in the page's words ("View Mini-CEX, PAED-003, encounter date 2026-09-01"); `RowNames.Distinct` adds tie-breakers where names would repeat.
- A row with no action says why in a `.muted` span ("Set by the College", "Staged below"); it is never blank.
- An item open for editing keeps its row read-only ("Editing below") and puts its controls in a second row spanning the table.
- Measure a table at 1280px against its longest real values before adding a column.

## Known gaps

- My activities, the Activity inbox, flow 05's index, standing panel, trajectory table and Activities on this EPA, and flow 06's Programme trainees and Waiting for assessors (from 900px) stack. Every other list at 390px still scrolls sideways inside its container.
- A filtered list's form above it is FilterBar's (flow 06); the older lists' `.search-container` with `.search-input` fields acts as each page wires it.

## Appearance

Header on `header-bg`, weight 600; rows ruled with `border-color`; cells padded `space-md`; hover `hover-bg`; figures in columns (`tabular-nums`, which Source Sans 3 draws already); the container is `surface-color` with a 1px border, `radius-xl` and, through `.shadow`, `shadow-raised`. All text pairs pass; a focusable scroll container (`.table-container[tabindex="0"]`) shows the page's focus ring.
