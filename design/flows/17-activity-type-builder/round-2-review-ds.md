# Flow 17 round 2: the design-system review

Reviewed 2026-11-10 from the design-system side: the 145 files in `round-2/` (141 boards: A01-A12 with A09-create and
-nocreate, B01-B11, C01-C21 with C08-list and -new and C15-before and -after, D01-D05, E01-E15, each with its `-390` twin and
the `-768` twins; plus R2-0-spec, -spec-2, -spec-3, -steps) and `f17r2.css`, against `src/Wombat.Web/wwwroot/app.css`,
the shell's `.razor.css` files, `execution/architecture/DESIGN.md`, and the built `ConfirmDialog`, `DataTable`,
`PageHeader`, `Breadcrumbs`, `Alert`, `ActionResult`, `StatePanel`, `Skeleton`, `FormField`, `FormActions`,
`RefusalSummary` and `ActivityTypeEdit`/`ActivityTypesList`. There is no `TabBar` component: the tabs are inline markup in
`ActivityTypeEdit.razor:82-87`. Model: `design/flows/16-college-catalogue/round-2-review-ds.md`.

Method. Static, by script over every board's `<main>` (classes against app.css, the shell CSS and `f17r2.css`; inline
style; hex and `rgb(`; h1 count and heading order; every `id`, `for`, `aria-describedby`, `aria-labelledby`,
`aria-controls`; the tabs pattern; unlabelled controls; table captions, `scope`, `data-label` against the header text;
dialog markup against `ConfirmDialog.razor`; alert roles; required marks; accessible names against visible words), plus
reading Spec 1-3 and `f17r2.css`. Contrast figures are computed from the `:root` tokens in app.css (the board folder carries
no `tokens.css`). Nothing was rendered; a width claim rests on a rule in app.css (cited by line) or says "not measured".
Severity: must, should, nit.

## Tally: 2 must, 10 should, 6 nit

## Must

- **M1. Required fields: the visual mark and a hidden "required" are drawn; `aria-required` and the line are not (39
  boards, 89 controls).** DESIGN.md:1329 and :1510 (T373's rule) want all three: the mark (`aria-hidden`), `aria-required="true"` on the
  control, and "* marks a field you must fill in." above the first field. What the boards draw:
  - **The Details tab (C01-C07, C15-before, E02, E11, E12, with twins: 22 files):** Key and Name are starred, 0
    `aria-required`, and no line anywhere on the board.
  - **The editors in place (C10-C14, C15-after, C17, C19, with twins: 17 files; 3 controls each: Field key, Label, Type,
    and the Choice's options):** the editor's controls have the mark and no `aria-required`; the only
    `aria-required="true"` on the board is on the *preview's* `pv-*` controls, and the only line is the preview's own,
    inside the other form. The editor, which is the form being filled, has neither.
  Built `FormField.razor` adds the hidden "required" itself and the caller adds `aria-required`; these boards do not show the caller
  doing it. Draw the line once at the head of the Details panel and once at the head of the editor, and put
  `aria-required="true"` on the 89 controls, so the build copies the whole pattern. (Flow 16's M1 was the same gap.)
- **M2. Two kinds of control have an accessible name that does not contain the visible words (WCAG 2.5.3, Level A;
  DESIGN.md:960-963, `RowActionMarkupTests`).**
  - **A tab with a problem count (C14, C15-before/after, C20, D01, D04, with twins: 12 files).** The tab reads "Form 1 problem"
    (the chip is visible text inside the button, `Form<span class="tab-problem">1 problem</span>`), but `aria-label="Form"`
    replaces the content, so the name is "Form" and the chip is only a description. Someone who says "click Form 1 problem"
    is not matched. Also, in the sighted text there is no space between the word and the chip, which a screen reader joining the
    content would read as "Form1 problem". Either drop the `aria-label` and let the content be the name with a visible-or-
    hidden space before the chip ("Form 1 problem"), or move the chip outside the button's text. Dropping `aria-label` also
    lets the Spec's `aria-describedby` go.
  - **The fold buttons of C19 (3 files, 6 buttons each):** visible "Hide fields" / "Show fields", names "Hide the fields of
    section X" / "Show the fields of section X". "Hide fields" is not a substring of "Hide the fields of". Name them "Hide
    fields of section X" and "Show fields of section X". They also change both the name and `aria-expanded` for one
    state: use one stable name ("Fields of section X") with `aria-expanded` alone, or keep the words and drop
    `aria-expanded`, not both; and add `aria-controls` naming the fields' container.
  Every other row action (Edit/Up/Down/Delete field/section, Done, View/Edit on the list) starts with its visible
  word: 0 failures in 141 boards.

## Should

- **S1. `.tab-problem` (Spec S1, NEW) is `.badge.badge-declined` under another name.** app.css:2039 `.badge` is a pill, 0.75rem,
  600, text-color, 1px edge; :2079 `.badge-declined` is `--danger-bg` with a `--danger-color` edge. `.tab-problem` is the
  same rule but for the padding (`0 6px`, line-height 1.4 against `.badge`'s 1px 10px, 1.5). The Spec's "no new badge" is
  true in name only. Use `<span class="badge badge-declined">` and, if the 390 width needs it, one scoped rule
  (`.tab-bar-tab .badge { padding-inline: 6px; }`), not a new component class. The colours are the same either way
  (11.81:1 text, 5.56:1 edge, computed).
- **S2. The tab chip's width at 390 is drawn for one count only.** The tab track is 72px (Spec) and the tab's padding 2px
  each side, so 68px; "1 problem" at 0.75rem is about 52px of text plus 14px of chip padding and edge, 66px: it fits with 2px to spare (inferred, not measured). Every board
  shows "1 problem" (12 files); "2 problems" is about 5px wider and "12 problems" about 20px, and `.tab-bar-tab` is `white-space: nowrap`, so a count past
  one spills into its neighbour's track. A form can have two problems. Draw "2 problems" at 390 or say that the chip is
  the number alone at 390 ("2") with "problems" in the visually hidden description.
- **S3. The tab bar moves out of the card, and nothing lists it.** `ActivityTypeEdit.razor:81-87` puts `.tab-bar` inside the
  surrounding `.form-container` and the panel's content inside the same card; DESIGN.md:1811 says "`.tab-bar` sits directly
  below `PageHeader` inside the surrounding `.form-container`". Every builder board draws the bar as a sibling of the card on the
  page ground, and the card itself *is* the `role="tabpanel"`. That is a good structure for the pattern, but Spec 2's "Code
  changes" row for `ActivityTypeEdit` says "the tabs pattern on `.tab-bar`" and does not name the move, and the DESIGN.md list
  (Builder layout rewritten) does not either. It also changes the tab's ground: the Spec's tab contrast figures are on `--surface-color`
  (4.86:1, 10.98:1), but the bar stands on `--background-color`: 4.61:1 for the underline, 10.42:1 for the selected label,
  11.99:1 unselected (all pass; the Spec's numbers are the wrong ground). Say it in Spec and DESIGN.md. `ContrastTests`
  `TheActiveTab_TakesOnFill` reads `.tab-bar-tab.is-active` and will throw when C1 removes that rule; it must be rewritten
  to `[aria-selected="true"]` in the same change.
- **S4. The tab hover is invisible.** `.tab-bar-tab:hover { background: var(--hover-bg) }` paints `rgb(248 249 250)`,
  exactly `--background-color`, which is what the bar stands on. The `--hover-bg` token is for a row on a white surface (app.css:350). Use
  `--header-bg` (241 243 245; muted text on it 4.57:1, text 11.36:1) or leave the hover to the underline.
- **S5. C3's block repeats nine rules app.css already holds for a selector list.** app.css:7539-7600 is "The catalogue tables stack from
  1024px down" for `.epas-page`, `.colleges-page`, `.college-tree-page`, one rule per declaration group. `f17r2.css` writes the
  same nine rules again under `.activity-types-page`. Flow 16's S4 asked for exactly this fold: add `.activity-types-page`
  to the lists (a new page class is the only genuinely new thing, and the boards carry it on all 31 list files). The arithmetic
  holds: the container is the viewport minus 308 (250 sidebar, 56 gutters, 2 border), so at 950px it is 642 and the
  table (682, the Administrator's 687) overflows by 40-45px; the table fits from about 990px, so the 1024px stack
  matches the precedent with room. At 1000px the table would be 692 in 692, no slack.
- **S6. T4's claim that a DataTable fails in the 363px rail is half-arithmetic.** Spec 2: "a DataTable per section leaves its
  label column about 93px beside four btn-sm buttons (204px) and two cells' padding". The sum is 363 - 204 - 64 (two cells'
  16px padding each side) = 95, close to 93. But it treats the actions cell as unable to wrap, and
  `.actions-cell` (app.css:461) is `flex-wrap: wrap`: in an auto-layout table the browser gives the label column its
  content and lets the buttons wrap two by two (a cell of about 110px, a label column of about 190px). Also, four
  `btn-sm` buttons at 0.85rem with 10px padding and 1px edges are 46+40+54+56 = 196px of button plus 24px gaps = 220px, not
  204. Not measured here. The conclusion still holds, for a different reason: a row of Edit/Up/Down/Delete under a label never leaves a
  readable measure in 363px, with or without wrapping, and `DataTable` stacks by a *viewport* query (app.css:4144 at 900px,
  :6278), which at 1280px does not fire in a rail. The honest reasons for the outline are those two; say them, and
  state that the measure was derived, not rendered. (A DataTable with Field and Edit only, 363 - 64 - 48 = 251px for the
  label, would work if Up, Down and Delete moved into the editor. The Spec rejects it implicitly: Up/Down are the keyboard
  reorder path. That is a design decision to record, not a CSS one.)
- **S7. The outline has no list semantics, and `.field-row-actions` duplicates `.actions-cell`.** 50 boards draw a section as
  `<section aria-labelledby>` holding flat `div.field-row`s of `<p>`s, so a screen reader hears no count ("list, 6 items"),
  and the section's name is its `h3` with the key `<code>` in it ("Teaching session session"). Make the rows a `<ul class="form-outline-fields">` of
  `li.field-row` (the class names stay). And `.field-row-actions` is `display: flex; flex-wrap: wrap; gap: xs`, which is
  `.actions-cell` (flex, wrap, gap sm) with a smaller gap; the boards also use it for the "Add section" button alone, which
  is not a row. Reuse `.actions-cell` (and keep the 4px gap only if the 363px rail needs it, measured), or name the one difference.
  `.field-row-actions .btn { min-height: 2.75rem }` at 390 (f17r2:149) is redundant: `.user-page .btn` (app.css:6616) already
  gives 44px to every button, `btn-sm` included, by specificity (0,2,0 over 0,1,0), and the Spec's own list of the
  44px scope says `.user-page` on the builder. Keep only `flex: 1 1 auto`.
- **S8. The Publish dialog's confirm is `btn-success`; the built `ConfirmDialog` has two kinds.** E05 (and 390): `<button class="btn btn-success">Publish version 2</button>`.
  `ConfirmDialog.razor` renders `btn-danger` or `btn-primary` only (`DangerAction`). All four
  other dialogs (Discard x2, Deactivate x2) match the component exactly (below). Either the Publish dialog is `btn-primary`,
  or a ConfirmDialog parameter is added and listed in Spec 2's code changes; as drawn, it is a component change nobody asked for.
- **S9. Stripe states vanish in a contrast theme.** `.field-row.is-editing`, `.is-refused` and `.field-editor-inplace` mark their state
  with `box-shadow: inset 4px 0 0` alone; forced-colors drops `box-shadow`. app.css handles this each time it uses the stripe
  (:1387, :4738: a `border-left` fallback). `f17r2.css` has one forced-colors rule, for the tab. The words under a refused
  row stay, and an open editor is told by `aria-expanded`, so nothing becomes unreadable, but the editor's link to its row (the point of T176's
  stripe) is lost. Add the fallbacks. The tab's `border-bottom: 3px solid transparent` is, in a contrast theme, drawn
  (a known behaviour of forced colours: borders keep a system colour), so all tabs would show the line, the selected one in
  `Highlight` (inferred, not tested).
- **S10. The heavy case (C19, 3 files) is drawn in placeholders that break its ids.** 20 boards carry a `‹…›` (A09-nocreate, C08-list,
  C19: Spec 4 allows brackets only "where FIXTURES has none"; the other two are the empty states). C19's ids contain them:
  `id="sec-‹section key 1›"`, `for="fe-‹key›"`, `aria-describedby="…‹…›-options-help"`. A space inside an id splits the
  token list, so 114 `aria-labelledby`, 282 `for`/`id` pairs and 6 `aria-describedby` references on C19 resolve to
  nothing by the HTML rules. The build makes real ids, but the board is the test fixture's source; use `sec-section-1`.

## Nit

- **N1. 62 boards print `aria-describedby=""`** (175 occurrences, on the editor's, the Details' and the settings' controls with no help). An empty
  token list is harmless but is not what the built markup should emit; `FieldHelp.DescribedBy` returning null prints nothing.
- **N2. Every tab is `aria-label` equal to its text** (102 files): the attribute is redundant except where M2 applies, and is
  then wrong. Drop it everywhere.
- **N3. The tabpanel is `tabindex="0"` on all 102 tab boards, with form controls inside.** The APG puts it there only when the
  panel's content has no focusable element; here it is one extra tab stop in front of the first field.
- **N4. The Edit buttons carry `aria-expanded` and no `aria-controls`** (50 boards); the editor is the next sibling `div#...`, so
  `aria-controls="fe-topic-h"`'s group id would link them. Not a failure.
- **N5. The Show choice's state is CSS-only and per tab render.** Blazor re-rendering the tab panel keeps the DOM radio's `checked`, but
  switching tab and back drops it to "Edit the form". Fine if intended; say so in C2's comment.
- **N6. `.field-row-meta` and `.outline-section-head h3 code` use `--muted-text` on `--header-bg`** (4.57:1, passing by 0.07); the Spec lists the
  figures on surface (5.09:1) and background (4.83:1) only. Add the third.

## The ask's checks

| Check | Result | Evidence |
|---|---|---|
| Classes in app.css, the shell, or f17r2 | Done | 0 undefined classes in 141 boards. New and used: `.activity-types-page` (31 files), `.form-outline` (50), `.outline-section` (50), `.outline-section-head`, `.field-row`, `-label`, `-meta`, `-actions` (50), `.field-editor-inplace` (19), `.tab-problem` (12), `.is-refused` (6); `.tab-bar` and `.tab-bar-tab` (102) and `.is-editing`, `.builder-two-col`, `.choice-group` exist and are CHANGED or reused. |
| `.cv-*` canvas-only | Done | `.cv-chip` (52 files) and `.cv-modal-stage` (8); `.cv-doc`, `.cv-cols`, `.cv-first` on the Spec boards only. None inside an article's components; all five are Part 1 of `f17r2.css` and never ship. |
| Shipped, by Spec row | C1 changed, justified; S1 duplicates a badge (S1); C2 justified; C3 justified, written as a copy (S5); S2 justified for reasons other than T4's arithmetic (S6), with S7 and S9 | |
| No inline style | Done | 0 `style=` in any article (141 boards); the head's `<style>` is `body{margin:0}`. |
| No raw hex or `rgb(` | Done | 0 in boards and in `f17r2.css`; every `var(--...)` in `f17r2.css` (all of them) is defined in app.css; `Highlight` in the forced-colors rule is a system colour. |
| One h1; heading order | Done | exactly one h1 per board; h2 for each panel's parts, h3 an outline section, h4 the editor; 0 skips; no panel without an h2. |
| ids and references | Done bar S10 | 0 duplicate ids, 0 dangling `for`, `aria-labelledby`, `aria-describedby` or `aria-controls` outside C19. |
| Tabs pattern | Done | 102 boards, 86 with five tabs and 16 with a reader's four; `role=tablist` with a label, `role=tab`, exactly one `aria-selected="true"`, `tabindex` 0 on it and -1 on the others, `aria-controls` on the selected tab only (the Spec's choice, S2), one `role=tabpanel` with `aria-labelledby` its tab. M2 and N2-N3 are the exceptions. Arrow-key behaviour is not a static check. |
| Labelled controls | Done | 0 unlabelled input, select or textarea in any article (label `for`, or `aria-label`). |
| Required marks | **No** | M1. |
| 44px at 390 | Done by shipped rules | `.user-page` buttons, inputs and selects (app.css:6616) covers the builder and the outline's `btn-sm`; `.people-page` the list; `.filter-actions`; `.clinic-table--stack-wide .btn` (:6281); the dialog's actions; the radio label (:6652); C1's tab 2.75rem. The shell's Menu toggle and the breadcrumb's parent link: not measured. The tab chip's width: S2. |
| ConfirmDialog | Done bar S8 | 5 dialogs (E05, E07, E09, E11 and twins) are `dialog.dialog-scrim.dialog-card[aria-labelledby][aria-describedby]` > `div.form-container` > `h2#dlg-title`, `div#dlg-body.dialog-body`, `div.form-actions` with Cancel `autofocus` first and the action second (`btn-danger` for Discard and Deactivate): the component's markup. |
| DataTable stack | Done | the list: `table.clinic-table.clinic-table--stack.clinic-table--stack-wide` with caption, `scope="col"`, row `th scope="row"`, `data-label` equal to the header text on every cell (0 mismatches), the actions header `.visually-hidden`; the moves and versions tables the same. |
| Alerts, summary, loading | Done | 0 danger alert without `role="alert"`, 0 summary without it, 0 nested `.action-result`, 0 `.loading-line` without `role="status"`. |
| Contrast | Done | below. |
| DESIGN.md amendments | Partly | Spec 2 lists Builder layout, Button, Form, Table, Page-level text; missing: the tab bar's move out of the card (S3), the `ContrastTests` rewrite, and a ConfirmDialog kind (S8). DESIGN.md:1804-1811 and :237 (`--radius-pill` "badges, tabs") are the lines it moves. |

## Contrast, computed from the tokens in app.css (`:root`, lines 48-69)

| Pair | Spec | Computed | Note |
|---|---|---|---|
| secondary-color on surface (selected underline) | 4.86 | 4.86 | on the page ground, where the bar stands: 4.61 |
| primary-color on surface (selected label) | 10.98 | 10.98 | on the page ground: 10.42 |
| text-color on surface (unselected) | 12.63 | 12.63 | on the page ground: 11.99 |
| text-color on danger-bg (chip text) | 11.81 | 11.81 | exact |
| danger-color on danger-bg (chip edge) | 5.56 | 5.56 | exact; non-text needs 3 |
| muted-text on surface / on background | 5.09 / 4.83 | 5.09 / 4.83 | exact; on header-bg 4.57 (N6) |
| white on btn-success | 5.88 | 5.88 | exact |
| danger-color on surface (refusal stripe) | 5.95 | 5.95 | exact |
| secondary-color on surface / on page (editing stripe, focus ring) | 4.86 / 4.61 | 4.86 / 4.61 | exact |

Every figure the Spec states is right; S3's point is only that the bar's own ground is the page.

## Words, not colour alone

A refused row says why under its label (`p.validation-message`) as well as the red stripe; the tab says "1 problem"; the selected tab is told by weight
and `aria-selected` as well as the underline; an open editor by "Done" and `aria-expanded`. Nothing is colour alone.
