# Flow 18 round 2: the design-system review

Reviewed 2026-11-11 from the design-system side: the 173 files in `round-2/` (170 boards: E01-E11, H01-H11, I01-I22, J01-J12,
L01-L15, R01-R11, each with its `-390` twin, the `-768` twins of E03, H01, I02, I12, J01, L01, L05 and R01, and R2-I03 and
R2-R05 at 390 only; plus R2-0-spec, -spec-2 and -steps) and `f18r2.css`, against `src/Wombat.Web/wwwroot/app.css`, the shell's
`.razor.css` files, `execution/architecture/DESIGN.md`, and the built `ConfirmDialog`, `DataTable`, `PageHeader`,
`Breadcrumbs`, `Alert`, `ActionResult`, `StatePanel`, `PagerControls`, `DashboardCard`, `BadgeFor`, `FormField`, `FormActions`
and `RefusalSummary`. Model: `design/flows/17-activity-type-builder/round-2-review-ds.md`.

Method. Two passes. **Static**, by script over every board's `<main>`: classes against app.css, the shell CSS and `f18r2.css`;
inline style; hex and `rgb(`; h1 count and heading order; every `id`, `for`, `aria-describedby`, `aria-labelledby`,
`aria-controls`; unlabelled controls; empty badges; table captions, `scope`, `data-label` against the header text; dialog
markup against `ConfirmDialog.razor`; alert and loading roles; every `var(--...)` in `f18r2.css` against `:root`. **Rendered**
(this round, unlike flow 17's): the 170 boards served from a scratch copy with `app.css` and the three layout stylesheets
substituted for the missing `ds/wombat/` links, in Chromium at 1280, 768 and 390 (each board at its own width): page
overflow, every button, input, select and textarea against 44px at 390, every `.table-container` against its scrollWidth, a
900-1280 sweep of the four lists, the Home grid and the health list's container query across 390-1280, a dialog redrawn in the
component's markup, the pager's DOM order, the payload region. The shell is the layout CSS, not the live shell and not the
Source Sans face (the `/fonts/` files were not served, so text fell back to Segoe UI): a width below is therefore an upper
bound on what the real face needs. Contrast is computed from the `:root` tokens. Severity: must, should, nit.

## Tally: 2 must, 10 should, 7 nit

## Must

- **M1. The two dialogs (I13, I15, with their 390 twins) are not ConfirmDialog's action-mode markup.** The component (action
  mode, `ConfirmDialog.razor:55-80`) draws `dialog.dialog-scrim.dialog-card` > `div.form-container` > `h2`, `p#body`,
  `div.form-actions` (FormActions) with Cancel carrying `autofocus` and the action `btn-danger`/`btn-primary`. The boards draw
  `div.dialog-form` > `h2`, `p`, `div.dialog-actions`, which are the **form mode's** classes (C7, app.css:1072-1100, for a
  dialog holding a real `<form method="post">`; neither dialog here posts), and Cancel has no `autofocus` (the canvas only
  marks it `cv-focus`). Rendered, the board's card is 448 x 270 and the same words in the component's markup are 448 x 305: 35px
  shorter, because `.dialog-actions`' rule and the form-mode padding differ from `.form-container` + `.form-actions`. The replay
  would show a taller dialog than the boards. Redraw both in the action-mode markup (flow 17's five dialogs did) and give Cancel
  `autofocus`; Spec 2 § 12's "`.dialog-card .dialog-actions` stacks both dialogs' buttons" then becomes
  `.dialog-card .form-actions` (app.css:6375), the rule the component already has. The Reactivate and Deactivate words are
  otherwise fine.
- **M2. The pager on all 40 R and L boards that draw one is not `PagerControls`' order or elements.** The component renders
  `div.pager-info`, `div.pager-actions` (Previous, Next), then `div.pager-page-size` ("Per page:" + select). Every board draws
  `span.pager-info`, `span.pager-page-size`, then `div.pager-actions`. Rendered at 1280 (R01): info x=282, page size x=424,
  actions x=574; the component's order puts Previous/Next second and the page size last, and the tab order follows the DOM.
  Spec § 13 names "the Pager" as the existing component ("Run history and the Audit log on PagerControls"), so this is a
  drawing error, not a change request; redraw in the component's order. (The `span`s are only a nit; the order is what a reader
  tabs through.)

## Should

- **S1. Home's two links are bare `<a>` in a `<p>`, 21px tall at 390 (rendered), where every other dashboard card puts its
  links in `.dashboard-card-footer`.** Spec 2 § 12 limits the 44px scope to the people and record pages and the dialogs, and the
  rendered sweep of all 170 boards at 390 found **0 buttons, inputs, selects or textareas under 44px**, so the Spec's claim holds
  for controls. But "Scheduled jobs" (H01-H10) and "Users" are the only actions on Home, and Flow 16's Home card made
  its links 44px through the footer (app.css:4127ff, :1957; `AssessorDashboard.razor:39`, `CollegeAdminDashboard.razor:13`).
  Draw them as `div.dashboard-card-footer` with the links as outline buttons, or say that Home's two links are inline text and
  exempt (WCAG 2.5.8 is met at 21px only if the 24px spacing exception applies, which a lone link in its own paragraph does not
  claim). Same card: the boards put `h2.dashboard-card-title` straight above the content; `DashboardCard` wraps the content in
  `div.dashboard-card-body` (app.css:1952, `margin-top: sm`) and the title in a `<span>`. If the build uses DashboardCard
  (it should), the card is 8px taller than drawn; draw the wrapper.
- **S2. Spec 2 § 12 says "At 768 ... Home is the 2-column grid with System health spanning both". It is one column.**
  app.css:2288 sets `.dashboard-grid` to one track and resets the spans at 900px and below; two tracks run only from 901 to
  1099px. Rendered: grid tracks 1 at 390, 640, 768 and 900; 2 at 901 and 1099; 3 from 1100. The H01-768 board is drawn
  correctly (one column); the sentence is wrong and will be copied into DESIGN.md § Dashboard page. (Related, right but
  unstated: between 1100 and 1114px the three-track grid gives System health a list 471-478px wide, under the 30rem query, so
  the lines take the narrow layout until 1115px; the container query behaves as designed.)
- **S3. `.code-block--region:focus-visible` (R2) restates the global rule.** app.css:512 gives every `:focus-visible` element
  `outline: 2px solid var(--focus-ring); outline-offset: 2px`, and says why (T328: a focusable scroll region had shown the
  browser's own ring). R2's second declaration block is the same two properties. Keep `max-height: 28rem; overflow: auto` only;
  the Spec's "its ring is the page's" is then literally true. The ring's 4.37:1 on `--header-bg` is the global ring's too.
- **S4. The payload region and its section carry the same landmark name (E02, E03, E06, E07, E09 and twins, 11 files).**
  `<section class="user-section" aria-labelledby="e-pl">` is a region named "Payload", and the `<pre role="region"
  aria-labelledby="e-pl">` inside it is a second region with the same name, so a landmark list reads "Payload, Payload".
  Name the `pre` by what it is ("Payload, as stored", `aria-label`), or leave the section unnamed (it has no landmark job; the
  other sections' `aria-labelledby` is the user page's pattern and stays). Keep `tabindex="0"`: the region scrolls (rendered:
  E06's 1,250px of text in a 482px box), and nothing focusable is inside it.
- **S5. Retiring the status dots leaves four things the Spec does not list (§ 13 names only DESIGN.md § Status dots).**
  `.status-dot` and `.status-dot.ok/.warn/.err` (app.css:2111-2131) are used only by `AdministratorDashboard.razor:16-22`;
  `ContrastTests.cs:319` resolves `.status-dot.{state}` and will throw when the rule goes; DESIGN.md:324 carries a contrast-table row,
  "Status dots on surface: success · warning · danger, 3 | 5.88 · 5.77 · 5.95"; and `DefinedClassTests` may scan the class.
  List the rule removal, the test rewrite or deletion, and the row in the same change, as flow 17 did for `.is-active`.
- **S6. `required=""` on 46 controls (23 I-boards) where DESIGN.md:1345 and :1535 want `aria-required="true"`.** The star is
  `aria-hidden` and the line "* marks a field you must fill in." is present on every form (good), but 0 controls on 170
  boards carry `aria-required`; they carry the native `required`, which the built pages do not (they are `novalidate` and
  write `aria-required="true"`, e.g. `InstitutionEdit.razor:64,72`). Copying the board would add the native attribute and its
  `:invalid` styling. Draw `aria-required="true"` and drop `required`.
- **S7. Rows have no row header on Scheduled jobs and Run history (35 board files with a table).** Institutions' Name is
  `th scope="row"` (the Spec says so, "painted as a header as on Users and EPAs"); Scheduled jobs' Job and Run history's Job are
  plain `td`s (checked per row), so a screen reader reading "Succeeded" in a run row hears no row name, and in the stacked
  layout nothing differs visually. The Job is the row's identity in both; make it `th scope="row"` (and add
  `.jobs-page`/`.job-runs-page` to the existing `th[scope="row"] .person-cell` painting if the build needs it). The audit
  rows have no natural name; their Detail link carries "Login at 2026-11-10 20:24:10" for that, which is enough.
- **S8. The 1150px stack for Institutions, Scheduled jobs and the Audit log is a readability choice the Spec words as an
  overflow fix.** Spec 2 § 10 sweeps each table against its container and shows them stacked to 1150 and as tables from 1151, then
  says "swept 900-1100 against its container" (S8). Rendered (Segoe UI fallback; the real face is narrower): with the stack
  switched off, the natural table first overflows its container at about **985px viewport** for Institutions (1022 with the
  200-character name), 862-885 for Scheduled jobs, 967 for the devadmin Audit log (926 for Mbatha's), and 899-904 for Run
  history. So the 1024px stack of Run history is 120px early and the 1150px stack for the other three is 130-290px early. Both
  are defensible (the Spec's cell lines, "Contact email three lines at 1151", are a real cost) but the precedent rule in
  app.css:7622-7628 and DESIGN.md:1147 is "stack where the columns overflow". Either say the reason is lines per cell, or use the
  1024px stack for Institutions and Audit like the EPA list. (The measured widths themselves agree with the Spec: 972 at 1280 for
  Institutions; the other three read 15px less here, the page scrollbar on a taller page.)
- **S9. C1 is justified as a fold, but four new page classes grow a selector list that is already 4 and 3 long.** The f18r2.css C1 block
  reproduces app.css:7633-7700 and 7708-7790 exactly (same eight rule groups, same declarations, same order; the only
  differences are the added selectors), and none of `.institutions-page`, `.jobs-page`, `.audit-page`, `.job-runs-page` exists in
  app.css or any `.razor`. Folding into the two existing lists is right (flow 16's S4 asked for it). The alternative worth a line:
  `DataTable` already takes a `Class` parameter (`DataTable.razor`, the data-rights tables use it) so one table modifier
  (`clinic-table--stack-1150`) would stop the lists growing by a page class a flow. Not required. And `.institutions-page` is also
  put on the record boards (31 files: `record-page institutions-page`), where there is no table; it does nothing there. Put it on
  the list only.
- **S10. The table container's `shadow` and the DataTable's roles are not on the boards.** `DataTable` draws `div.table-container.shadow`
  and `role="table"`/`rowgroup` (caller `row`/`columnheader`/`cell`) on stacked tables; the boards draw `table-container` alone and no
  roles (170 boards; flow 16 and 17's boards did the same, so it is a convention, not a regression). The roles are why a stacked
  grid row keeps its table semantics (DataTable's own comment). State once in the Spec that the build adds both, so
  the replay compares a 1px shadow, not a defect.

## Nit

- **N1. `h2` "Runs" / "Actions" (R, L boards) are bare `<h2 tabindex="-1">`**; the built focus-target headings are
  `h2.list-section-title` (`ActivityInbox.razor:40-50`; `.people-page .list-section-title` margin 0). Same look today; the class is
  what a test finds the heading by.
- **N2. The count line is `p role="status" tabindex="-1"` and the focus target (L01).** Focus on a live region announces it and the
  region announces the change: a double read. R06/R08 put the focus on the h2 instead (the Spec § 13 says the h2). Pick the
  h2 for both, or drop `role` from the one that takes focus (the ActionResult pattern, whose region is not itself live).
- **N3. Badge tone for Disabled and Not configured is `badge-accepted` (amber, "taken on and in hand, or wanting attention again",
  `BadgeFor.cs:23`).** A job an administrator switched off, and mail that was never set up, are neither in hand nor newly
  needing attention; `badge-draft` (grey, "no longer in force") reads as true. The word carries it either way, and Spec § 11
  records the choice as decided; I would not hold the build on it. Overdue and Running share one tint (`.badge-overdue` is
  `.badge-accepted`); fine, words differ.
- **N4. The badge edges' contrast is quoted against white; they sit on their tints.** Spec § 14: success 5.88, warning 5.77, danger
  5.95. Drawn on `--success-bg`, `--warning-bg`, `--danger-bg` they are 5.23, 5.43, 5.56 (and the neutral edge on `--header-bg`
  is 3.42, not 3.80). All pass 3:1; say the ground.
- **N5. Muted text on the success tint is 4.52:1**, 0.02 above the line (Spec § 14 states it). Fine today; a darker muted token
  or a tint change would break it, and `ContrastTests` should pin this pair.
- **N6. 66 boards carry `‹...›` placeholders** (32 board groups: the times of the database-down and window lines, "‹key›" and
  the crash error). None sits inside an `id`, `for` or `aria-*` value (checked), so every reference resolves. Spec 4 allows
  them where FIXTURES holds none; the cast review owns whether that holds here.
- **N7. `.health-line-name`'s "Database<span visually-hidden>:</span><badge>"** reads "Database: OK". Good; the same pattern
  would help the `Detail` links (already done). Nothing to fix; recorded because it is the one place a hidden colon
  stands in for a label.

## The ask's checks

| Check | Result | Evidence |
|---|---|---|
| Every class exists | Done | 0 undefined in 170 boards' articles and 0 in their shell. Not in app.css or the shell, found in `f18r2.css`: `.health-list`, `.health-line`, `.health-line-name`, `.health-line-text` (21 files), `.code-block--region` (11), `.institutions-page` (43), `.jobs-page` (25), `.audit-page` (32), `.job-runs-page` (22), and the `.cv-*` set. |
| `.cv-*` canvas only | Done | `cv-chips`, `cv-chip` (+ `--illus`, `--new`, `--proposed`, `--data`), `cv-note`, `cv-focus`, `cv-scrim`, `cv-dialog` appear only as the board's chips, notes aside, focus mark and the open dialog's layer; all are Part 1 of `f18r2.css`. `.cv-sheet`, `.cv-cols` on the Spec boards only. |
| NEW / CHANGED rules, shipped | R1 justified; R2 half (S3); C1 justified as a fold (S8, S9) | R1: `.detail-list` is a viewport-query dl with a muted dt; the health card needs the card's width (a 30rem container query: narrow at 1100-1114 and at 768) and a name-plus-badge cell, so it earns its rule. R2: the ceiling and `overflow` are new; the ring is not (S3). C1: declaration-for-declaration the existing stack. |
| C1 reproduces app.css exactly | Done | the 1150px block (thead clip, row grid, last-child, cell reset, `th[scope=row]` block, `td[data-label]::before` with the `/ ""` fallback line, `.btn` 2.75rem) and the 1024px block match app.css:7633-7700 and :7708-7790. The four page classes exist nowhere else. |
| No inline style | Done | 0 `style=` in any of the 170 articles; the head's `<style>` is `body{margin:0}`. |
| No raw hex or `rgb(` | Done | 0 in the articles, 0 in `f18r2.css`; every `var(--...)` in `f18r2.css` is defined in app.css. |
| One h1; heading order | Done | 170 boards, exactly one `h1`, 0 skipped levels. R2-I21 (Access denied) has an `h1` without `tabindex`, as the shipped `.system-panel` page does. |
| ids and references | Done | 0 duplicate ids; 0 dangling `for`, `aria-labelledby`, `aria-describedby`, `aria-controls` (including the dialog's `dlg-t`/`dlg-b`). |
| Labelled controls | Done | 0 unlabelled `input`, `select`, `textarea`: the Job select (`f-job`), Status, the From/To days, every audit filter, and the pager's page size (`label.pager-page-size-label` for `per-page`) all have a label. The filter forms are `role="search"` with a name. |
| Words, not colour alone | Done | 0 empty badges. Badges: Success 312, Succeeded 280, Failed 36, OK 32, Active 25, Not configured 15, Failing 8, Inactive 6, Running 4, Disabled 2, Overdue 2, Unreachable 2, Not read 2; every one a word. 0 `status-dot` on a board. |
| 44px at 390 | Done for controls; S1 for Home's links | rendered, 170 boards: 0 controls under 44px (pager Previous/Next, page-size select, filter selects and date inputs, the Detail buttons, Run now, the dialog's two, Try again). Breadcrumb links 44. Home's "Scheduled jobs" and "Users": 21px. No page overflows sideways at its own width (0 of 170); no table scrolls inside its container (0). |
| ConfirmDialog markup | **No** | M1. |
| DataTable stack markup | Done bar S7, S10 | 69 tables: `clinic-table clinic-table--stack clinic-table--stack-wide`, caption, `scope="col"`, every non-first, non-action cell has `data-label` equal to its header (0 mismatches); the actions header is `visually-hidden`. Institutions' Name is `th scope="row"`; the others are not (S7). |
| Payload region | Done bar S4 | `pre.code-block.code-block--region[tabindex=0][role=region][aria-labelledby]` on all 11 files (E02, E03 with its 768, E06, E07, E09 and twins): named by its h2; rendered E06 scrolls (1,250px in 482px). |
| Alerts, load, results | Done | 0 `alert-danger`/`alert-warning` without `role="alert"`; 12 `.loading-line`, all `role="status"`; results sit in `div.action-result[tabindex=-1]` above the list, the shape `ActionResult` renders; load errors are `alert alert-danger alert-row` with the Try again button, nothing under it. |
| Drift from DESIGN.md | See S5, S6, S2 | the dots, `aria-required`, the 768 grid. |

## Contrast, computed from the tokens in app.css (`:root`, lines 48-69)

| Pair | Spec § 14 | Computed | Note |
|---|---|---|---|
| text on success / warning / danger / info tint | 11.23-11.89 | 11.23 / 11.89 / 11.81 / 11.84 | exact |
| muted on white / background | 5.09 / - | 5.09 / 4.83 | exact |
| muted on `--header-bg` | 4.57 | 4.57 | exact |
| muted on success / warning / danger / info tint | 4.52 / 4.79 / 4.75 / 4.77 | 4.52 / 4.79 / 4.75 / 4.77 | exact; success has 0.02 to spare (N5) |
| success / warning (and overdue) / danger edge on white | 5.88 / 5.77 / 5.95 | 5.88 / 5.77 / 5.95 | on the tints: 5.23 / 5.43 / 5.56 (N4) |
| `--input-border` on white | 3.80 | 3.80 | on `--header-bg` 3.42 |
| focus ring on white / `--header-bg` | 4.86 / 4.37 | 4.86 / 4.37 | also 4.61 on the page ground |
| link on white | 6.70 | 6.70 | on `--header-bg` 6.03 |

Every figure the Spec states is right.

## Widths, rendered

The 900-1280 sweep (table `scrollWidth` / container `clientWidth`; S for stacked, T for table) agrees with Spec 2 § 10: Institutions
S 592 / 642 / 692 / 716 / 717 / 742 / 792 / 842 at 900 / 950 / 1000 / 1024 / 1025 / 1050 / 1100 / 1150, T 843 / 892 / 972 at 1151 / 1200 /
1280, no overflow at any step; Scheduled jobs and the Audit log (Mbatha's and devadmin's, as drawn) the same switch points, 15px
narrower because their pages carry a scrollbar; Run history stacked to 1024, a table from 1025. The boards' stacks do their
job; S8 is only whether they are needed that wide.
