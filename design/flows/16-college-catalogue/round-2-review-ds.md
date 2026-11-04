# Flow 16 round 2: the design-system review

Reviewed 2026-11-04 from the design-system side: the 306 board files in `round-2/` (A01-A22 with A13a/b, A14a/b, A16a/b,
A18a-c, A19a/b, A20a/b; B01-B17 with B11a/b, B14a/b, B15a/b, B17a/b; C01-C21 with C03a/b, C15a-c, C18a/b, C19a/b, C21a/b;
D01-D19 with D09a/b; E01-E20 with the -EV1 and -EV2 pairs; F01-F20 with F20a-f; the 1280 boards plus their 390 and 768
twins; plus R2-0-spec, -spec-2, -steps) and `f16r2.css`, against `src/Wombat.Web/wwwroot/app.css`,
`execution/architecture/DESIGN.md`, and the built `ConfirmDialog`, `DataTable`, `PageHeader`, `Breadcrumbs`, `Alert`,
`ActionResult`, `StatePanel`, `Skeleton`, `FormField`, `FormActions`, `PagerControls` and `DashboardCard`. Model:
`design/flows/15-institution-adopts-catalogue/round-2-review-ds.md`.

Method. Static, by script over every board file (classes against app.css and the shell's `.razor.css`, inline style, hex
and `rgb(`, h1 and heading order, ids and `aria-describedby`/`aria-labelledby`/`for` references, unlabelled controls,
table captions, scopes and `data-label` against the header text, row-action names against their visible labels, dialog
markup against `ConfirmDialog.razor`, alert roles, loading lines), plus reading Spec § 3-9 and `f16r2.css`. Nothing was
rendered. Where a width or a 44 px claim rests on a rule that exists in app.css it is checked by line; where it rests on
layout arithmetic it says "not measured". Severity: must, should, nit.

## Tally: 2 must, 5 should, 4 nit

## Must

- **M1. Required fields carry a mark only a sighted reader gets (95 files: every form board and its twins).** Boards draw
  `<span class="required-mark" aria-hidden="true">*</span>` on a label and nothing else: 0 `aria-required="true"` on any
  control, no visually hidden "required", and no "* marks a field you must fill in." line above the first field (grep:
  0 of 306). DESIGN.md:1292-1296 (T373, M11) and :1473-1477 (A15) require all three; built `FormField.razor:7` adds the
  hidden "required" itself. As drawn, a screen reader hears an optional field. Flow 15's boards left it out too, so the
  build copied the gap; draw it once here (the line, and `aria-required` on Name, Short code, Label, Code, Title,
  Version, Sub-speciality and the other starred controls) and the build has the pattern.
- **M2. Four row and header links have names that do not contain their visible words (WCAG 2.5.3; DESIGN.md:960-963,
  RowActionMarkupTests).** The names, with the boards that draw them:
  - "Items (15)" / "Items (16)" is named "Items of Paediatric EPA Curriculum (11.1)": the count is missing (D05-D14, D18);
  - "Create sub-speciality" is named "Create a sub-speciality of Paediatrics": "a" breaks the phrase (A02, A04, A07, A12,
    A13b, A14b, A16b, A22);
  - "Edit this version" is named "Edit Paediatric EPA Curriculum (11.2)" (F05-F09, F11, F13, F15, F18);
  - "Open Adult Neurology" is named "Open the sub-speciality Adult Neurology" (C08).
  Name them "Items (15) of …", "Create sub-speciality of Paediatrics", "Edit this version of Paediatric EPA Curriculum
  (11.2)", "Open Adult Neurology, the sub-speciality". Every other row button and link passes: it starts with
  its visible words, minus the trailing "…" (no failures for Remove…, Delete…, Deactivate…, Hold back…).

## Should

- **S1. Every result region is drawn twice (71 files: the success and the rung-refusal results, A04, A07, A09, A12, A13b,
  A14b, A16a, B06, B08, B11a, B12, C04, C05, C08, C09, C11 ... and their twins).** Markup:
  `<div class="action-result" tabindex="-1"><div class="action-result" tabindex="-1"><div class="alert ...">`. Built has
  one `ActionResult` (`ActionResult.razor`; `StatePanel` wraps once). Two nested focus targets mean `FocusAfterRender`
  would have two candidates and the layout rules that match `.action-result:empty` and `.user-page .action-result .alert`
  see a wrapper that is never empty. The empty-region boards (A10) and the five load errors are right (one region).
- **S2. The 900-1100 px sweep has holes exactly where this flow's lesson bit (BRIEF § 11 "Flow 15"). Not measured by me;
  by layout arithmetic.** The container is the viewport minus 250 (sidebar) minus 56 (article gutters) minus 2 (border):
  vw - 308. Stack-wide tables stack at 900 and below (app.css:4125), so 900 in the Spec's sweep measures a stacked table
  and tells nothing. The sweep samples 900, 950, 1000, 1050, 1100.
  - **Colleges (B01, B02, B11b, B16, B17a)**: the Spec's own measure, a 667 px table, agrees with the arithmetic
    (`.people-page .clinic-table th.col-wrap` min 7rem, app.css:6751, plus 32 px padding gives 144 px for Name and
    Description). It overflows by 667 - (vw - 308): 74 px at 901, 25 px at 950, none from about 975. The Spec leaves the fix
    to the operator and the boards draw the overflow. Decide before build: stack the Colleges list to 1024 px as the EPA
    list does (one more rule), or move Created into the record. The container has no `tabindex="0"`/`role="region"`
    (DESIGN.md:2201, :3116), so a keyboard user cannot scroll it.
  - **Curricula (F01-F04, six columns)**: two `.col-wrap` columns at 144 px (288), Effective about 98, Items about 72,
    Status about 80, Actions (Edit and Items, wrapping) about 92: roughly 630 px. Against a container of 593 at 901 px
    this overflows by about 37 px and fits from about 940. The sweep's 950 shows it fitting; 901-939 is unsampled.
  - **The tree's sub-speciality tables (A02, A04, A07, A12, A13b, A14b, A16b, A22, five columns)**: three `.col-wrap`
    at 144 px, Status about 80, Edit about 84: roughly 596 px, 3 px over a 593 px container at 901. Borderline; it clears
    by about 905.
  - **The items table (F05-F13, F15, F18, six columns, `--compact`)**: the Spec's measure (744 px, as built by flow 15)
    gives overflow below a viewport of 1052 px, but it stacks at 1024 (app.css, the 1024 block at about :7395), so the
    unstacked range 1025-1051 overflows by up to 27 px (22 px at 1030). The sweep's 1000 is stacked and 1050 is the
    first width that passes. The app.css comment above that block sized the table at 901 only.
  - Fits by the same arithmetic: scales (four columns, about 490 px), the EPA list (stacks to 1024; at 1025 a 717 px
    container holds about 430 px), the levels table (stacks).
  Ask: sweep 901-939, 975 and 1025-1051 in 5 px steps before the build, or fix the three tables above in this round.
- **S3. The header actions wrap under the subtitle, as drawn, on four pages (Spec § 5, "Header actions beside the
  title").** Colleges, the College's EPA list and Curricula at 1280, and those plus the tree at 1100: Create college,
  Create EPA, Create curriculum and Create speciality drop below the long subtitle. The Spec calls it a fix for the
  operator that needs a third shipped rule (`flex: 1 1 20rem; min-width: 0` on the title block in `.header-container`).
  It is a defect of existing app.css, not of the boards, and it touches every PageHeader with actions: take it as one
  decision, not four boards' worth of redraws; until then the boards at 1280 do not show the intended layout.
- **S4. `f16r2.css` S1 duplicates three shipped rules.** `.user-page.catalogue-record .form-check` and its input and
  label are, declaration for declaration, app.css:7462-7477 (`.user-page.curriculum-item-page`) and :7515-7529
  (`.user-page.epa-page`), and the data-rights page's (`.data-rights-page .form-check`, about :7242). It would be the
  fourth copy of one rule. Every board that holds a checkbox sits in `.user-page` (57 `catalogue-record`, 24 `epa-page`,
  7 `curriculum-item-page`), so write one rule, `.user-page .form-check` (and `-input`, `label`) below 641 px, and retire
  the per-page copies, rather than add `.catalogue-record` and a fifth later. `.catalogue-record` is then not needed
  on the five pages. The Spec's note that "the build may write both selectors in one rule" is the smaller version of this.
- **S5. The levels table has no row header (28 files: C02-C05, C11-C16, C20, C03a/b and twins).** The first cell of each
  rung is `<td data-label="Order">`; the rung is named only by a visually hidden label on its input. Every other table in
  the round has `th scope="row"`. Make Order the `<th scope="row">` (it is the rung's rank, unique per row), or the
  Label input's row will be read as "3, edit text" with no name for the row.

## Nit

- **N1. Breadcrumb markup differs from `Breadcrumbs.razor`, rendering the same (232 files).** Boards use separate
  `li.breadcrumb-separator` items and `aria-current` on the `li`; built nests the separator in the crumb's `li` and puts
  `aria-current="page"` on the `span`, and draws the back arrow on every non-current crumb, not only the parent. Flows 14
  and 15 drew it the same way.
- **N2. 56 files print a bracketed placeholder as a field value ("‹description›" in the rung and item textareas; "‹code A›",
  "‹College name A›" on B16; "‹a description of 2,000 characters›").** Spec § 4 says "no bracket where a value is given".
  The textarea's own text is a value to a reader and to the build's markup test; use the empty textarea, or the
  fixture's words.
- **N3. Unavailable boards (22 files) use `<p class="state-panel-copy">` and a bare `<div>` around "Go to …".** Built
  (`RequestDetail.razor:74-79`) is `div.state-panel-copy` and `div.form-actions`. The 44 px at 390 holds either way
  (`.detail-card--empty .btn`, app.css:3663).
- **N4. D56's "Save with Active unticked asking the same" has no drawn trigger.** Save is a plain submit on A15, A17 and
  B09, which show Active ticked. DESIGN.md:777 gives a submit that opens a dialog `aria-haspopup="dialog"` "like every
  trigger"; say in the Spec that the build adds it when Active is unticked. (The Deactivate…, Hold back…, Delete… and
  Remove… triggers all carry it: 0 failures, an improvement on flow 15's N5.)

## The ask's checks

| Check | Result | Evidence |
|---|---|---|
| Classes in app.css, NEW, or `cv-*` | Done | 1 undefined class: `.catalogue-record` (NEW; S4). The shell classes live in `NavMenu.razor.css`/`MainLayout.razor.css`. 0 `cv-*` used and not defined in `f16r2.css`. `.curriculum-item-page` and `.epa-page` already shipped (app.css:7451, :7482) |
| New rules duplicating existing ones | One | S4; Part 1 (`.cv-*`) is canvas only |
| Components vs markup | Mostly | PageHeader, DataTable (StackWide, captions, `scope`, `data-label` equal to the header text on every cell), Alert, Skeleton, FormActions, DashboardCard, `StatePanel` (empty and load error with `alert-row` + Try again inside one `ActionResult`) and `ConfirmDialog` match. Not: Breadcrumbs (N1), the doubled `ActionResult` (S1). `PagerControls`: none drawn, correct (no list passes 20) |
| ConfirmDialog | Done | 18 of 18 dialogs (9 boards and twins) are `dialog.dialog-scrim.dialog-card[aria-labelledby][aria-describedby]` > `div.form-container` > `h2#id`, `div#id.dialog-body`, `FormActions`, Cancel `autofocus`, danger confirm: the shape flow 15's S1 asked for |
| No inline style | Done | 0 `style=` in any screen markup (306 files); each board's `<style>` is `body{margin:0}` |
| No raw hex | Done | 0 hex or `rgb(` in boards or `f16r2.css`; every `var(--...)` in `f16r2.css` is defined in app.css |
| One h1; heading order | Done | exactly one h1 per board, with `tabindex="-1"`; h2 sections, no skip; dialog h2s are their own |
| `for`/`id`, `aria-describedby` | Done | 0 dangling `for`, `aria-describedby`, `aria-labelledby` or `aria-controls`; 0 duplicate ids; 0 unlabelled controls in the article or a dialog; refusals are `.validation-message role="alert"` and `aria-invalid` is on every invalid control |
| Required marks | **No** | M1 |
| Table captions, scope, labels | Done bar S5 | every table has a `<caption>`, `th scope="col"`, `th scope="row"` (except the levels table), and `data-label` on every non-action cell; the actions header is `.visually-hidden` |
| Row actions in `div.actions-cell` | Done | 0 `td.actions-cell` (flow 15's M1 is fixed) |
| Named row actions, none alike | Partly | M2; the only repeated names are C03b and C16, which draw two rungs with the same label on purpose (the duplicate-label refusal) |
| Alerts | Done | every danger alert has `role="alert"`, every success `role="status"`; the info (standing) alerts carry no role (no `role=""` is printed) |
| Loading and load errors | Done | `p.loading-line[role="status"]` on every loading board; each load error is one `ActionResult` with the Try again row (flow 15's S3, S8 fixed) |
| 44 px at 390 | Done by shipped rules | `.user-page` buttons and inputs, `.dialog-card .form-actions`, `.detail-card--empty .btn` (:3663), stack-wide `.btn` (:6262), DashboardCard footer for Home's four links; checkbox rows by the new rule (S4). The shell's Menu toggle: not measured |
| Tables overflow, 900-1100 | **No** | S2 |
| Header beside the title | Spec admits | S3 |
| Words, not colour alone | Done | Active, Open, Held back, Inactive are words everywhere |
| DESIGN.md amendments | Done | Spec § 8 covers the Button, Form, Table, Page-level and NavMenu text. DESIGN.md:594's CollegeAdmin menu row and :4024's `ValidationSummary` page template are the lines it moves; the Form system text at :4148 already says there is none |
