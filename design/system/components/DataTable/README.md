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

`Items`, `HeaderRow` and `Row` are required; `Caption` and `Empty` are optional. It renders `div.table-container.shadow > table.clinic-table`. Most pages wrap it in a `StatePanel` for loading, error and empty.

## The list page shape (DESIGN.md § Table system; § Page-level patterns)

PageHeader (title, subtitle, primary action) → `.search-container` with a `.search-grid` of labelled `.search-input` fields → the table → `PagerControls`.

## Column and row classes

- `.clinic-table--compact`: half the cell padding (`space-sm`), for eight or more columns or a grid of inputs.
- `.col-wrap` (and `--wide` on its `th`): a long-text column that wraps first, down to a 4rem (5rem) floor, and asks for 14% (20%) of the table.
- `.col-fit`: as narrow as its content, on one line (an ordinal, a code). Not for buttons.
- `.col-actions`: a row's buttons, 12rem for three small buttons.
- `.clinic-table--inputs`: every row is a set of inputs; cells centre, no hover tint; under 44rem of container its buttons stack.
- `tr.is-editing`: a row open for editing and its spanning form row below, tied by `editing-stripe`.
- A table grouped by one column puts each group in its own `<tbody>`, opened by `<th scope="rowgroup" rowspan="n">`.

## Rules

- Every `<table>` is a `.clinic-table` in a `.table-container`; never Bootstrap's `.table`, and no inline column widths.
- An actions column's header is a `.visually-hidden` "Actions", never an empty `<th>`. A column no row offers an action in is not rendered.
- Every row's button carries an `aria-label` that contains its visible label and names the row in the page's words ("View Mini-CEX, PAED-003, encounter date 2026-09-01"); `RowNames.Distinct` adds tie-breakers where names would repeat.
- A row with no action says why in a `.muted` span ("Set by the College", "Staged below"); it is never blank.
- An item open for editing keeps its row read-only ("Editing below") and puts its controls in a second row spanning the table.
- Measure a table at 1280px against its longest real values before adding a column.

## Appearance

Header on `header-bg`, weight 600; rows ruled with `border-color`; cells padded `space-md`; hover `hover-bg`; the container is `surface-color` with a 1px border, `radius-12` and `shadow-utility`. All text pairs pass.
