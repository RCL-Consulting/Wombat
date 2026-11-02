# Flow 15 round 2: the design-system review

Reviewed 2026-11-02 from the design-system side: the 168 board files in `round-2/` (A01-A15 with A06a/b, B01-B20, C01-C19
with C18a/b, D01-D07a/b, E01-E05, F01-F05, G01; 1280 boards plus their 390 and 768 twins; plus R2-0-spec, -spec-2,
-steps) and `f15r2.css`, against `src/Wombat.Web/wwwroot/app.css`, `execution/architecture/DESIGN.md`, and the built
`ConfirmDialog`, `DataTable`, `PageHeader`, `Breadcrumbs`, `Alert`, `ActionResult`, `StatePanel`, `Skeleton`,
`FormActions` and `PagerControls`. Model: `design/flows/14-data-rights/round-2-review-ds.md`.

Method. Static, by script over every board file (classes against app.css, inline style, hex, h1 and heading order, ids and
`aria-describedby` and `for` references, table captions, scopes and `data-label`, row-action names, dialogs, alerts),
plus reading Spec § 3-6 and 8-13 and the stylesheet. Nothing was rendered. Where a width or a 44 px claim rests on a
rule that exists in app.css it is checked by line; where it rests on layout arithmetic it says "not measured".
Severity: must, should, nit.

## Tally: 1 must, 8 should, 7 nit

## Must

- **M1. Every row-action cell is `<td class="actions-cell">` (242 cells; A01-A13, B06, B08, B10, B11, B13, B17, B20, C09,
  C15).** DESIGN.md:919-922 (T266) says a cell's buttons sit in a `div.actions-cell` inside the `<td>`, never
  `td.actions-cell` (a flex `td` stops being a table cell and its border no longer meets the row's; app.css:442-446 makes
  `.actions-cell` a flex box), and `RowActionMarkupTests.cs:35,185` fails a page that does it. Flow 14's boards drew
  `<td><div class="actions-cell">`. Spec § 3 does not declare the change. Draw the cells as the rule says.

## Should

- **S1. Dialogs are not the built action-mode markup (A03, A08, A11, B13, C12, D03, F02 and their 390 twins).** Boards:
  `<dialog aria-labelledby>` with `<div class="dialog-form">` and a body with no id and no `aria-describedby` on the
  dialog. Built (`ConfirmDialog.razor`, action mode): `<div class="form-container">`, the body as `<p id=BodyId>` or
  `<div id=BodyId class="dialog-body">`, and `aria-describedby=BodyId` on the dialog; `.dialog-form` is the form mode's
  wrapper. As drawn, a screen reader is not given the body on open. `.dialog-card .form-actions` (app.css:6275) applies
  either way.
- **S2. C15's pager is hand-drawn, not `PagerControls`.** Board: `<nav class="pager">` holding Previous, a span
  "Page 1 of ‹n›", Next. Built (`PagerControls.razor`): `.pager-info` summary, `.pager-actions`, and the page-size
  `<select>` named by its label (44 px at 390 by app.css:4045). The board drops the select and leaves a bracket for a
  count the Spec gives (40 EPAs).
- **S3. The five load-error boards draw a bare danger alert (A15, B19, C17, C18b, E04).** Built `StatePanel` with
  `OnRetry` wraps it in `ActionResult` (`tabindex="-1"`, takes the focus once Try again has answered). None of the five
  has a `div.action-result`, so the board has no focus target for the answer; the label strips say nothing of focus.
- **S4. Two links with one name on one page (A10, A11 and twins).** "Items of Paediatric EPA Curriculum (11.1)" is the
  `aria-label` of a link in "All curricula" and of another in "Curricula KGK trains against" (A10 repeats 11.2 too).
  DESIGN.md § Table system (T239): no two row names alike. Name the second table's links for the institution.
- **S5. The items table's width is asserted, not shown (B01-B20 at 1280, and 901-1100).** Spec § 4 says nothing scrolls
  because Scale folded into Minimum level. The table is seven columns on the default padding (`clinic-table--stack`,
  no `--compact`): 224 px of padding alone, plus "neurodevelopmental" (163 px) in the EPA column and an Edit/Remove…
  cell. DESIGN.md:1038-1062 records the old table at 874 px compact against a 907 px container (5 px to spare at the
  EPA column) and recommends `--compact` from eight columns. Not measured; by that arithmetic I expect the
  `.table-container` to scroll sideways at 1280 and certainly between 901 and 1100 px (stacking starts at 900,
  app.css:4125). The scroll is inside the container, not the page, but the container has no `tabindex`. Measure it, or
  add `--compact`.
- **S6. DESIGN.md § 5 misses text T323 makes false.** It lists six changes. Still describing the removed inline edit row:
  "Editing a row in place" (DESIGN.md:893-910: `.is-editing`, the colspan form, `CurriculumItemsEditLayoutTests`), the
  row-actions paragraph's "the edit row's legend" and "Cancel and Save on the curriculum items page" (:930-945), and the
  picker paragraph's "After each Add, Save and Remove the page asks for both pickers again (the edit row's while one is
  open)" (:973-980). "Curriculum adoptions" is also named outside the NavMenu table, at :3985 (Invite a person's link)
  and :4441 (the InstitutionalAdmin's quick links); Spec § 9 re-points the words but § 5 does not list those lines.
  And § 5's "The picker rule" names no section that exists (nearest: the picker paragraph in § Table system, :965).
- **S7. § 5's "words, never a badge" contradicts DESIGN.md as it stands.** DESIGN.md:3777 and `PageHeader.TitleContent`
  put an institution's own EPA's badge in the EPA page's h1 (EpaProgress), and G01 (My progress) shows
  "Kgosi Kgari Teaching Hospital's own" as `.badge-draft`. The Spec admits the exception for My progress; the amendment
  should say "on the admin lists and pages; the trainee's h1 and My progress keep the badge", or flow 15 changes them.
- **S8. Loading boards omit the status role (A14, B18, C16, C18a, E03).** `<p class="loading-line">Loading.</p>` has no
  `role="status"`; built pages have it (AssessorsList.razor:34, RequestDetail.razor:35). A14 repeats "Loading." under two
  h2s.

## Nit

- **N1. `.curriculum-item-page` (B04, B05, B09, B12 and 390 twins) is needed, but could be narrower.** No existing rule
  gives the item form's checkbox rows 44 px: `.activity-form` (app.css:2434-2446) and `.review-page` (:4626-4645) are
  scoped to their own pages (`.activity-form` also carries a whole form layout, :1468); `.user-page` has
  `.choice-group .form-check label` for radios only (:6616); the Opportunistic checkbox sits in a `fieldset`, outside
  `.check-grid`. Flow 14 made the same call (`.data-rights-page .form-check`, :7242-7255). Two cleanups: the rule's
  first selector (`.check-grid .form-check`) is redundant with its second (`.form-check`); and a general
  `.user-page .form-check` rule would give EPA, profile and future user-page forms the same rows without a class per
  page. src has no `curriculum-item-page` yet; the root `user-page curriculum-item-page` is on exactly the four form
  boards that hold a checkbox.
- **N2. Breadcrumb markup differs from `Breadcrumbs.razor`, rendering the same (46 boards).** Boards use separate
  `li.breadcrumb-separator` items and `aria-current` on the `li`; built nests the separator in the crumb's `li` and puts
  `aria-current="page"` on the `span`. Both are hidden at 390 by app.css:3313-3317. Flow 14 drew it the same way.
- **N3. `role=""` is written on standing alerts (B01, B02, B03, B06 ...).** `Alert` with `Role=""` renders no role
  attribute; the board prints an empty one.
- **N4. Empty states use `<p class="state-panel-title">` (A01, C01, E05); `StatePanel` renders a `<div>`.**
- **N5. Dialog triggers carry no `aria-haspopup="dialog"` (Remove…, Deactivate…, Adopt…, Save profile and move…).**
  DESIGN.md:757 requires it on the user page's outline triggers (T376); flow 14 omitted it too.
- **N6. A10's column header "For your institution" is read by the Administrator** for a chosen institution; "For this
  institution" would be true for both.
- **N7. D02's `.submit-check` changes on selection and is not a live region.** It is the select's `aria-describedby`
  target, which many readers do not re-read on change. Consider `role="status"` if the build confirms it is silent.

## The ask's checks

| Check | Result | Evidence |
|---|---|---|
| Classes in app.css, NEW, or `cv-*` | Done, bar the shell | 124 classes. Only `.curriculum-item-page` (NEW) is undefined in app.css; the 25 shell classes (`.sidebar`, `.nav-*`, `.account-*`, `.brand*`, `.role-*`, `.phone-role*`, `.sign-out*`) live in `NavMenu.razor.css` and `MainLayout.razor.css`, as in flow 14. `.state-panel-title`, `.skel-*`, `.loading-line`, `.submit-check` (app.css:1629), `.dialog-body--scroll` exist |
| `.curriculum-item-page` needed? | Yes, see N1 | `.activity-form` and `.review-page` are page-scoped; `.user-page` covers radios only |
| Class used with the meaning given | Done | `.muted .text-sm` owner mark, `.form-section-head`, `.submit-check`, `.user-page`; `.badge` only on G01 |
| Components vs markup | Partly | PageHeader, DataTable (StackWide, captions, scope, `data-label`), Alert, Skeleton, FormField, FormActions match. Not: ConfirmDialog (S1), PagerControls (S2), StatePanel load error (S3), Breadcrumbs (N2) |
| Owner mark is words, never a badge (M4) | Done for A-F | no `.badge` on any list or form board; G01 keeps flow 05's badge (S7) |
| No inline style | Done | 0 `style=` in any screen markup (168 files); each board's `<style>` is `body{margin:0}` |
| No raw hex outside `:root` | Done | 0 hex or `rgb(` in f15r2.css or the boards; every `var(--...)` in f15r2.css is a token in app.css |
| One h1; heading order | Done | every screen has one h1, first; h2 sections; h3 only under A10-A11's "An institution's curricula"; the dialog's h2 is its own |
| Table captions, scope, labels | Done | every table has a `<caption>`, `th scope="col"`, a `th scope="row"` row header and `data-label` on each non-action cell; the actions header is `.visually-hidden` |
| Named row actions, none alike | Partly | every row button or link has a distinct `aria-label` containing its visible label, except S4 |
| Refusals first in `aria-describedby` | Done | C05 (`code-err`), C07 (`sub-err sub-help`), B05/B12 (`epa-err epa-help`), D06 (`cur-err cur-line`); no dangling id on any board |
| Focus targets | Partly | `h1 tabindex="-1"` on every page; `.action-result tabindex="-1"` on the result boards; dialogs `autofocus` on Cancel (7 of 7); the five load errors have none (S3) |
| 44 px at 390 | Done by shipped rules | stack-wide `.btn` (app.css:6262, to 900 px), `.user-page` controls and actions (:6597-6616), `.dialog-card .form-actions` (:6275), `.search-container .form-select` and `.pager .form-select` (:4045), `.alert-row .btn` and `.pager .btn` (:2815), `.breadcrumb-parent a` (:3319); checkboxes by the NEW rule. The shell's Menu toggle: not measured |
| >= 24 px at 1280 | Done | `.btn` has a 36 px floor (app.css:452 ff); breadcrumb links `min-height: 24px` (:3296); inline links in cells (C02) are exempt text links |
| Nothing sideways at 390, 768, 1280 | Not measured | tables stack from 900 so 768 and 390 are blocks; long values are in `.col-wrap` (`overflow-wrap: anywhere`); the items table at 1280 and 901-1100 is the open risk (S5) |
| Words, not colour alone | Done | state is words everywhere (Active, Open, Held back, Adopted on, Superseded on, "(inactive: not in force)"); a refusal has `.invalid` plus `.validation-message` text |
| DESIGN.md amendments | Partly | the § 5 rows agree with § Form system (no `ValidationSummary` matches :4083, flow 12 S22), § Page-level patterns (Unavailable, :1891-1901), the dialog's Cancel focus (:771, :790) and flows 13 and 14. Gaps: S6, S7. The Form page template (:3959) still draws a `ValidationSummary`; the amendment should name itself as scoped to the EPA, item and profile pages |
| Contrast (Spec § 6) | Done | recomputed from the tokens: text on success tint 11.23, on surface 12.63; muted on surface 5.09, background and hover-bg 4.83, header-bg 4.57, info-bg 4.77, warning-bg 4.79; link on surface 6.70; danger on surface 5.95; input-border 3.80; focus-ring 4.86, and 4.55 on info-bg. Every figure matches the Spec and DESIGN.md's Contrast table. The Spec omits muted on success-bg (4.52); no board draws it |
