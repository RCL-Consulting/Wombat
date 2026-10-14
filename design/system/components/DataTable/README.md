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

Flow 08 (T364, round 3 M8) stacks its lists the same way from 900px: Decision panels and its Who decides each EPA (DecisionPanels), Decisions due's By EPA (DecisionsDue) and Committee reviews (CommitteeReviews); there a stack-wide row's button is 44px (`.committee-calendar .clinic-table--stack-wide .btn`). Decisions due's list and the seat picker (SeatPicker) stack below 641px only. A stack-wide row header keeps the header cell's tint; only the seat picker's drops it below 641px, so the name sits over its radios (build review D5).

**Both stack classes on a hand-written table** (flow 09, T366, S8; DESIGN.md § Table system): a list that must stack at 900px writes `.clinic-table--stack .clinic-table--stack-wide`, as flows 06 and 08 write theirs; DataTable's `Stack` is the 641px one and gains no wider mode. Flow 09's three are written so: My committee reviews (MyCommitteeReviews, its Period the row's header, D11), My authorisations' No longer in force (MyAuthorisations) and the STAR register (StarRegister).

**A row's line** (flow 03's note row, generalised by flow 09, T366, S4, S9, C22): a row that needs a sentence under it is `tr.has-note`, followed by a row whose one cell spans every column, `td.history-note` (`pre-line`, no top padding). Since flow 09 the pair works on **any `.clinic-table`**, not only the activity's `.history-table`: every cell of the noted row drops its bottom rule (`.clinic-table .has-note > *`), so the two share the row's edge; stacked at 900px the noted row drops its rule and bottom padding and the line joins its block, unpadded (D4), wrapping, never cut. The STAR register's revoked and superseded lines are the first outside the history: "Revoked 2026-10-14 at 16:50 SAST by Refilwe Mokoena: Staged on evidence about PAED-001, not PAED-002; to be decided again.", "Superseded by STAR #18, issued 2026-10-13.".

## Rules

- Every `<table>` is a `.clinic-table` in a `.table-container`; never Bootstrap's `.table`, and no inline column widths.
- An actions column's header is a `.visually-hidden` "Actions", never an empty `<th>`. A column no row offers an action in is not rendered.
- Every row's button carries an `aria-label` that contains its visible label and names the row in the page's words ("View Mini-CEX, PAED-003, encounter date 2026-09-01"); `RowNames.Distinct` adds tie-breakers where names would repeat.
- A row with no action says why in a `.muted` span ("Set by the College", "Staged below"); it is never blank.
- **One action a row** (flow 08, T364, C3; DESIGN.md § Table system): a row offers the one thing to do next, never two that compete. Decisions due's held row offers "Open review #1" and no Schedule, a row that can be scheduled only Schedule, a decided one "Decided: nothing to schedule".
- **A status cell** (flow 08, `.status-cell`): the badge over a sentence (0.9rem) saying what it means, a review it names a link whose line is padded to a 24px target (`.status-cell p a`, 1ec7a352).
- **A period over its window** (flow 08, `.period-window`): "2026 S2" over "2026-01-01 to 2026-12-31" in `muted-text` at 0.9rem, each date unbroken (Committee reviews).
- **A person over the address** (flow 08, `.person-cell`): the name over the whole address in `muted-text`, which wraps anywhere (the seat picker).
- **On flow 08's pages** (`.committee-calendar`) every row action of a stacked table is 44px and the row's width below 641px; **on flow 09's** two STAR pages (`.my-authorisations`, `.entrustment-list`) every row button, a failed download's Try again and the register's Issued link are 44px below 641px. My committee reviews' list is wrapped in `.my-reviews-list`, so its View is 44px below 641px.
- **A cell's link that is a second line** (flow 09, `.issued-review`): the register's "review #2" under its Issued date, an inline box 24px tall at 0.9rem, never a 21.6px line (M9), and only where the reader may open that review; plain text otherwise (S7).
- An item open for editing keeps its row read-only ("Editing below") and puts its controls in a second row spanning the table.
- Measure a table at 1280px against its longest real values before adding a column.

## Known gaps

- My activities, the Activity inbox, flow 05's index, standing panel, trajectory table and Activities on this EPA, flow 06's Programme trainees and Waiting for assessors (from 900px), flow 08's Decision panels, Who decides each EPA, By EPA and Committee reviews (from 900px), Decisions due's list and the seat picker, and flow 09's My committee reviews, No longer in force and the STAR register (from 900px) stack. Every other list at 390px still scrolls sideways inside its container.
- A filtered list's form above it is FilterBar's (flow 06); the older lists' `.search-container` with `.search-input` fields acts as each page wires it.

## Appearance

Header on `header-bg`, weight 600; rows ruled with `border-color`; cells padded `space-md`; hover `hover-bg`; figures in columns (`tabular-nums`, which Source Sans 3 draws already); the container is `surface-color` with a 1px border, `radius-xl` and, through `.shadow`, `shadow-raised`. All text pairs pass; a focusable scroll container (`.table-container[tabindex="0"]`) shows the page's focus ring.
