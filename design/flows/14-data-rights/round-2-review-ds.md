# Flow 14 round 2: the design-system review

Reviewed 2026-10-30 from the design-system side: the 138 boards in `round-2/` (R2-M01-M18 My data rights, R2-Q01-Q13 the
queue, R2-D01-D19 a request and its decision, R2-C01-C10 the correction, R2-K01 the kept request, R2-U01-U02 Request
unavailable; 1280 and 390 twins, 768 for eight, Q13 at 390 only = 133; plus R2-0-spec, -spec-2, -spec-3, R2-0-steps,
-steps-2 = 5) and `canvas.json` against `design/system/components/bundle.css` (8,071 lines), `src/Wombat.Web/wwwroot/app.css`,
`execution/architecture/DESIGN.md`, the shipped `DataTable.razor`, `ConfirmDialog.razor`, `NavMenu.razor` and BRIEF § 5.1.
Model: flow 13's `round-2-review-ds.md`; the ask is `round-2-ask.txt`.

Method, and what it could not do. Static, by script over all 133 board files: classes against `bundle.css`, `app.css`
and the Spec's NEW table; raw colours; inline `style=`; Bootstrap-like classes; ids, `aria-*` and `for` references;
headings; the brand-mark image; tables (caption, `scope`, roles, `data-label`); badge class by word; focus marks
(`pv-focus`) against each chip; every GUID and address for a wrapping class; every time for a zone; gendered pronouns;
the 1280/390/768 twins' text sets; each role's menu; titles and h1; `canvas.json` against the files. **Nothing was
rendered.** The two NEW stylesheets (`r2-new.css`, `r2-canvas.css`) are linked as `/_blob/b7f92335...` and
`/_blob/d4da56c4...` and are not in the folder (D6), so a 44 px or sideways-scroll claim that rests on a NEW rule
(`.data-rights-page`, `.request-page`, `.correction-records`, `.answer-cell`, `.request-id`) is read from the Spec's
description, not measured; every claim that rests on a shipped rule is checked against `bundle.css` by line. Cast and
code are other reviewers' remit. Board text is read as data. Severity: M must-fix, S should-fix, N nit.

## The ask's checklist (CHECK BEFORE YOU STOP)

| # | Check | Result | Evidence |
|---|---|---|---|
| 1 | One h1 a board | Done | 133 of 133 have one; h1, h2 sections, no skipped level; the dialog's h2 is the dialog's own |
| 2 | No inline style | Done | 0 `style=`; every board's `<style>` is `body{margin:0}` |
| 3 | No undefined class | Done | every class is in `bundle.css` or in the Spec's NEW table (`.request-id`, `.data-rights-table`, `.answer-cell`, `.request-page`, `.correction-values`, `.correction-records`, `.data-rights-page`, `.dialog-body--scroll`) or canvas-only (`.anno`, `.pv-focus`, `.pv-overlay`); but see D6: the NEW rules' CSS is not in the folder |
| 4 | No raw colour | Done | 0 hex/rgb/hsl in markup; no NEW token (Spec 2) |
| 5 | Every 390 control 44 px | Partly | covered by shipped rules: `.user-page` (bundle.css:6505-6528), `.alert-row .btn` and `.pager .btn` (:2729), `.clinic-table--stack-wide .btn`, `.filter-actions .btn` (:3952), `.dialog-card .form-actions` (:6180). **Not covered: the queue's Type and Status selects and the pager's page-size select (D1)** |
| 6 | Nothing scrolls sideways at 1280, 768, 390 | Unverified (static pass) | every GUID is in `.request-id` (0 bare), addresses carry `<wbr>`, tables stack from 900 px so 768 and 390 are blocks; the Spec says "by construction, not rendered"; the NEW rules are not in the folder (D6) |
| 7 | No gendered pronoun for a person (M4) | Done | 0 hits for he/she/his/her/him/himself/herself in 133 boards' text |
| 8 | Every bracket still a bracket, A.1.8a above all | Done | R2-M08, M09, C01, C02, C03, C05, C06: time, request id, user id, reason, note, "Completed on ‹date›" are all ‹…›; the only real values are "Mahlngu" and "Mahlangu", which the ask gives |
| 9 | 390 and 768 twins carry the 1280 words; the Spec says whether the captures arrived | Done | twin script: 0 differences over 133 boards (text sets); Spec 1 opens with "They did not arrive as attachments", read from the folder, 10 opened, 8 staged and not opened, the rest not staged |

Tally: **7 done, 1 partly (5), 1 unverified (6)**, none not done.

States and widths: every state in "STATES TO DRAW" has its boards: M01-M18, Q01-Q12, Q13 at 390 only, D01-D19, C01-C10,
K01, U01, U02, each at 1280 and 390; 768 on M01, M04, Q04, Q09, D01, C02, C05, C09 (the eight tables). 133 boards,
all in `canvas.json` on the "Round 2" page, none missing, none extra, widths match their names. The 20 Illustrations
(M13, M16-M18, Q09, Q10, D10-D16, D19, C04, C07-C10, U02) carry "Illustration" in the canvas title and in the chip,
and the canvas titles match the Spec's list exactly.

## The ask's 25 corrections

| # | Result | Boards / evidence |
|---|---|---|
| M1 | Done | U01, U02: h1 "Request unavailable", "This data rights request does not exist, or you cannot open it.", Go to Data rights requests, Data rights requests lit (`aria-current="true"`), no alert; the download's 404 untouched |
| M2 | Done | the digest help in Q27's words on all 16 boards that hold it (M01-M13, M16-M18); no "request to assess", "committee or entrustment decision" or "password reset" anywhere |
| M3 | Done | M03, M05, M07, M10 keep "Request submitted. You will be notified when it is reviewed."; D03, D05, C01, C06 each say the requester is emailed; the answer column names no decider; Spec 1 (Q5 row) names AccountNotices' exception |
| M4 | Done | no gendered pronoun on any board (check 7) |
| M5 | Done | C02-C10: four radios from Q26's list with M5's lines (no line offers an email), "Recorded" = "Pieter Smit · ‹time› SAST"; Spec 1 writes each TargetType and the recorder column |
| M6 | Done | Steps 1 and 2: A.1.8a as Role/Route/Do/Expect, the moved Expects (A.1.9, A.1.12, A.7.5, the Goal, the outcome check), A.2.4 and A.2.6 "likely unchanged" with the appendix named as unread, the word search with each hit's step |
| S1 | Done | devadmin ("Demo Administrator") under the grouped menu on C07, D06-D14, Q05-Q07, Q09, Q10 (31 boards); Data rights requests lit under Platform |
| S2 | Done | 0 inline style; stacking by `.dialog-card .form-actions`, `.user-page`, StackWide, `.pager`; 1280 DOM order kept (safe button first in the dialog; D01 Reject then Approve). See D5 for the NEW scope that repeats `.user-page` |
| S3 | Done | Spec 1 (BadgeFor row) and Spec 3: Approved green to amber, Withdrawn's class renamed, "never moves from red"; the boards use `badge-accepted` and `badge-withdrawn` |
| S4 | Done | every Requester cell is `th scope="row"` on Q01-Q13 |
| S5 | Done | Mahlangu's rejection 00:03 SAST (D05, M08, M09, Q04); du Plessis's id 01a17f63-7af4-7ac5-a6d1-d01726b57bc0 (D04, Q); addresses are the cast's |
| S6 | Done | C03: danger Alert in the Correction section's `.action-result`, focus on Complete request |
| S7 | Done | "You cannot download it yourself." on D01, D02, D16 (Coordinator) and not on devadmin's boards; D12 has no time |
| S8 | Done | Spec 1 and Spec 2 (DATA table): institution name, the result's counts, the recorder, the decider's name; T276's residue dropped |
| S9 | Done | Spec 3 PROPOSED list: counter, Withdraw narrowed, kept notice, Decide's per-type help, Approved amber, Request id column |
| S10 | Done | K01: trail Home > Data rights requests > the id, `breadcrumb-parent` folds at 390, the notice an info Alert with no role under the header above the sections |
| S11 | Done | D13: "The roles ‹roles› and their scopes." in It ends, Illustration |
| S12 | Done | each requester's own menu: Trainee, du Plessis (Home / My progress, Export portfolio, My data rights: matches NavItems.cs:213), Coordinator, Administrator, no-role Illustration |
| N1 | Done | the counter "0 of 4,000 characters" is on the PROPOSED list (Spec 2 messages table, Spec 3) |
| N2 | Done | Steps 2: one counting rule stated ("from the menu link; 390: Menu one more press") |
| N3 | **Partly** | Clear filters is 44 px by `.filter-actions .btn`; the page-size select is not covered (D1); Q13-390 draws Clear filters with no filter applied (D8) |
| N4 | Done | `.request-id` NEW in Spec 1; `.muted` and `.person-cell` used, no `.pv-muted` on a board (it survives in Spec 3's text only) |
| N5 | Done | `breadcrumb-parent` at 390 on every request page |
| N6 | Done | Spec 3 names the variations V1/V2 |
| N7 | **Partly** | Download is named for the Export request ("Download the export of the Export request made ..."); no wording is given for an Access request (D12) |

Also required, all met: the request page borrows `.user-page`/`.user-section` and says so (Spec 1); `badge-withdrawn`'s
comment widens (Spec 2); the radios get 44 px rows by a rule scoped to the page and named (`.data-rights-page`); the
erasure dialog scrolls in itself (`.dialog-body--scroll`); the muted line's contrast on a hovered row (4.8:1, correct
by computation); PagerControls 10, 20, 50, 100.

**Tally: 23 done, 2 partly (N3, N7), 0 not done.**

## What holds

- **Shell.** Sidebar, top bar (`account-row` with name and Sign out), phone bar (`phone-role`, CSS-only toggle) and the
  brand mark are flow 01's on every board: the mark is the `/_blob/616538c681cf4475edf7c466864e408d` image in
  `img.brand-mark` on 133 of 133, never a drawn disc, and the markup skeleton matches flow 13's board for board. The
  menus match the fixtures role by role; `aria-current` is "page" on list pages and "true" on owner rows of request pages.
- **ConfirmDialog (flow 12's).** D07, D13, D14 (1280 and 390): native `<dialog>` as `ConfirmDialog.razor` writes it
  (`dialog-scrim dialog-card`, `form-container`, `aria-labelledby`/`aria-describedby`, body in `.dialog-body`), Cancel
  first in the DOM and focused, the danger "Erase and approve" the only filled danger button in the flow, the trigger
  "Approve and erase..." an outline `btn` with `aria-haspopup="dialog"`, the action on top at 390 by `.dialog-card .form-actions`
  (bundle.css:6180). Title by name; "This cannot be undone." first.
- **Results and refusals.** Every result and failure sits in an `.action-result` (`tabindex="-1"`) with an Alert
  (`role="status"` for done, `role="alert"` for refusals and failures); the chips' focus targets are focusable on every
  board that draws a focus (a result region, a field, a button, an `h2[tabindex=-1]`); a field's refusal is a
  `.validation-message` under it with `aria-invalid` and the invalid border (M02, M13, D02, C04). The kept notice is an
  Alert with no role. Loading is a `role="status"` `.loading-line` plus `.skeleton`; the load errors are `.alert-row`
  with Try again (flow 11's D5 not repeated).
- **Badges.** Submitted `badge-submitted` (blue), Approved `badge-accepted` (amber), Completed `badge-completed`,
  Rejected `badge-declined`, Withdrawn `badge-withdrawn`: 5 words, 5 classes, no exception on any of 133 boards. No Under
  review, no Objection anywhere (Q8). Approved's amber text is `--text-color` on `--warning-bg` (see D11).
- **Names and words.** Row actions are named for their row and none alike (Withdraw/Download/Review start with their
  visible word, WCAG 2.5.3; M17's two Withdraws are told apart by "the later/earlier of two that minute"). Every time has
  a date and "SAST" to the minute (0 bare times), ids and the pseudonym are in `.request-id` or a wrapping cell, every
  email has `<wbr>`. Page titles `<Type> request from <name> · Wombat`, "Request unavailable · Wombat", "Data rights
  request · Wombat" for loading and the load error, "My data rights · Wombat" and "Data rights requests · Wombat"; the
  h1 equals the title; nav labels are the owner rows'.
- **Forms.** Fieldsets with legends for the two radio sets, each radio's line a `.page-subtitle.choice-help` named by
  `aria-describedby` (flow 11 and `UserDetail.razor:489`), help outside labels, required marks (aria-hidden star plus
  visually hidden "required") on every text field and textarea, no dangling `for`/`aria-*`, no duplicate id.
- **Icons.** Inline Lucide paths only (30 distinct, all Lucide shapes: shield-check on Data rights requests, lock on My
  data rights); no `bi`, no emoji, no drawn glyph.

## Findings

| Id | Sev | Boards | What is wrong | Evidence | Fix |
|---|---|---|---|---|---|
| D1 | **M** | Q01-Q13 at 390 (Q04, Q09 at 768 are stacked blocks, unaffected) | **The queue's Type and Status selects are 38 px and the pager's page-size select 28 px at 390; nothing in the Spec's 44 px scope covers them.** The checklist's "every 390 control 44 px" and the ask's N3 ("Clear filters and the page-size select at 390") are not met for the selects; Spec 3 claims N3 done by listing `.filter-actions .btn` and `.pager .btn` only. The same filter bar on Entrustment decisions and Audit has the same 38 px. | `bundle.css:533-550` `.form-select{min-height:2.375rem}`, `:551` `.form-select-sm{min-height:1.75rem}`; the 390 rules that raise a select are scoped to `.committee-calendar` (:4657), `.msf-page` (:5812), `.people-page`/`.user-page` (:6507-6509), `.activity-form` (:1674); none to `.search-container` or `.pager`; Spec 1's NEW table has no such rule; every Q board has `select.form-select` (Type, Status) and `select.pager-page-size-select` | Add one NEW rule at max-width 640.98px: `.search-container .form-select, .pager .form-select {min-height: 2.75rem}`, list it in the NEW table and in Spec 3's accessibility line; or put the queue's filter on flow 12's `.people-page .people-filter` scope (bundle.css:6592, 6629-6645, 6734), which already gives 44 px. Pre-existing for every list's pager (flow 13's D8); this flow draws the pager on 12 boards, so it should state the rule once for all. |
| D2 | **M** | every board with a table: M03-M17 (Your requests), Q01-Q13 (the queue), C05, C06, C09, C10 and the other correction boards (Corrections recorded); 70+ boards, 8 tables at 768 | **The stacked tables do not keep DataTable's roles, and drop its base stack class.** The ask's ARIA lesson is "a stacked table keeps DataTable's roles". DataTable with `StackWide` emits `class="clinic-table clinic-table--stack clinic-table--stack-wide"`, `role="table"`, `role="rowgroup"` on thead and tbody and `role="row"`, `role="columnheader"`, `role="rowheader"`, `role="cell"` on the caller's cells, because `display:grid` on a row drops a table's semantics in Safari. The boards write `<table class="clinic-table clinic-table--stack-wide data-rights-table">` with bare `<tr>`, `<td>`, `<th scope=...>`. Flow 13 drew them (`R2-C02-1280` in flow 13: `role="table"`, `<td role="cell">`, `<th scope="row" role="rowheader">`). | `DataTable.razor:10-24, 54-56`; `bundle.css:2556-2562` ("the table carries explicit roles, which a row laid out as a grid would otherwise lose"); `R2-Q04-1280` `<table class="clinic-table clinic-table--stack-wide data-rights-table">`, 0 `role=` attributes on any table, row or cell | On all three tables: `class="clinic-table clinic-table--stack clinic-table--stack-wide data-rights-table" role="table"`, `role="rowgroup"` on thead and tbody, `role="row"` on tr, `role="columnheader"` on th scope col, `role="rowheader"` on th scope row, `role="cell"` on td. Say in Spec 3's accessibility line that the roles are DataTable's. |
| D3 | **M** | M03-M17 (Your requests), Q01-Q13 (the queue): every Status cell, 70 cells | **The Status cell has no `data-label`, so at 390 and 768 (stacked from 900 px) the badge stands in the block with no "Status:" before it.** The ask says "StackWide, each cell labelled at 390". Type, Answer, Request id, Institution, Value before/after and Recorded are labelled; Status is the only cell with text that is not (the actions cell and the th row are the exceptions by design). Shipped tables label it (`data-label="Status"`), and flow 13 labels its badge cell ("Outcome"). | `R2-Q04-1280`: `<td><span class="badge badge-submitted">` (70 `<td>` with a badge, 0 with `data-label`); `bundle.css:4067-4072` shows the label only on `td[data-label]`; `EntrustmentDecisions/Index.razor:138` `<td role="cell" data-label="Status">` | `data-label="Status"` on the badge cell in all three tables. |
| D4 | S | M01-M18 ("Request type"), C01-C10 ("Record") at 1280 and 390 | **The two required radio groups do not tell a screen reader they are required.** The legend has the star (aria-hidden) and no visually hidden "required"; every text field on the same boards has both. DESIGN.md: "A required group carries the mark a required `<FormField>` does, in its legend (`*` hidden from a screen reader, then a visually hidden 'required')." | `<legend>Request type<span class="required-mark" aria-hidden="true"> *</span></legend>`; `DESIGN.md:1299-1302`; `InviteAPerson.razor:92`, `NewCampaign.razor:266` (`<span class="visually-hidden"> required</span>`) | Add `<span class="visually-hidden"> required</span>` after the star in both legends. |
| D5 | S | M01-M18, C01-C10, D01-D19 (the NEW table) | **Three NEW rules repeat what flow 12's `.user-page` already does.** `.data-rights-page` "My data rights' column (the .user-page measure) and its 390 scope: every .btn and .form-control 44 px, radio and checkbox labels 44 px rows, .form-actions stacked primary on top" is `.user-page` (flex column, `max-width:60rem` at bundle.css:6423; 44 px at :6505-6511; `.form-actions` column-reverse at :6513-6522; choice-label rows at :6524-6528). `.correction-records` ("its labels 44 px below 641 px") is :6524. `.request-page` ("detail lists get minmax(0, 1fr) values, so a 60-character address wraps in its dd") is :6435 (`.user-page .detail-list dd{min-width:0; overflow-wrap:anywhere}`), and `.request-id` already wraps. Flow 12's D4 and flow 13's D4/D5 were the same finding. The page already uses `.detail-card.user-section`, which expects a `.user-page` parent. | Spec 1 NEW rows; `bundle.css:6423-6440, 6505-6528`; M-board markup `div.data-rights-page > section.detail-card.user-section` | Use `class="user-page"` on My data rights and on the correction and request pages; keep only what is missing (`.answer-cell` wrap, `.correction-values` two columns, `.dialog-body--scroll`, a full-width Download/Withdraw that StackWide's `.btn` rule already gives). Drop `.request-page` and `.correction-records` from the NEW table, or say why `.user-page` is not enough. |
| D6 | S | all 133 boards | **The two NEW stylesheets are not in the folder.** Every board links `/_blob/b7f92335...` (`r2-new.css`) and `/_blob/d4da56c4...` (`r2-canvas.css`); neither is under `design/`. A reviewer cannot diff the NEW rules, a build cannot take them, and the Spec's 44 px and no-sideways-scroll claims for `.data-rights-page`, `.request-page`, `.answer-cell`, `.correction-*` cannot be measured (this review could not). Flow 12 shipped `f12r2.css` beside its boards; the Spec says "NEW classes ... r2-new.css, proposed for app.css". | `grep '_blob/b7f92335'` on 133 files; `find design -name 'r2-*.css'` finds nothing | Save `r2-new.css` (NEW rules, each commented NEW) and `r2-canvas.css` (`.anno`, `.pv-focus`, `.pv-overlay`) in `round-2/`, and say in Spec 1 which file holds which. Then measure the boards at 1280, 1100, 768 and 390 (the ask's own list) before the build. |
| D7 | S | Q13-390 (Q07 is filtered and fine) | **Clear filters is drawn with no filter applied.** Q13's selects read "All types" and "All statuses", the caption is unfiltered, and Clear filters is there; its 1280 twin Q08 (same five rows) has none. DESIGN.md: "Show (primary) and, once any is set, Clear filters". The ask's A.7.5 wording ("the filters above Show and Clear filters") did not decide this. | `R2-Q13-390`: `<a class="btn btn-outline" href="#">Clear filters</a>` and `<option selected>All types`; `DESIGN.md:3534`; `EntrustmentDecisions/Index.razor:17` | Either draw A.7.5 with a filter applied (then the rows change), or drop Clear filters from Q13 and state N3's Clear filters 44 px on Q02/Q03/Q06/Q07, which do have a filter. |
| D8 | S | Q01, Q03, Q04, Q06, Q07, Q08, Q13 (1280, 390; 768 on Q04) | **The pager is drawn on lists of 1, 3 and 5 rows**, with disabled Previous and Next and "Showing 1-1 of 1". Flow 13's closed list and the People list draw it only past one page ("`PagerControls` only past 20"; "The pager is drawn only when the matches run past one page"). The ask says PagerControls at 20 and is silent on when. Not wrong (`PagerControls` is "the one component ... on every list that can grow"), but this flow decides it the other way from the two lists it sits beside, and a disabled pager on one row is noise on a 390 screen. | `R2-Q01-1280` `nav.pager` with `aria-disabled="true"` ends; `DESIGN.md:1143, 3584-3585` | Decide once: draw it only when `MatchCount > PageSize` (as the People list), keep it on Q09/Q10 only, and say so in Spec 2 (§ Table system text). |
| D9 | S | R2-M17 | **The 4,000-character reason is not drawn.** The ask: "R2-M17 a reason of 4,000 characters and a long note on its row". M17's Reason textarea is empty ("0 of 4,000 characters"); only the note on the row is long. Spec 3 lists M17 for "A reason of 4,000 characters (its counter)". D19 draws a long reason on the request page, but not the form's counter at its limit. | `R2-M17-1280`: `<textarea id="reason" ...></textarea>` empty, counter "0 of 4,000 characters"; Spec 3 edge-case table | Fill the textarea with a bracket value ("‹a reason of 4,000 characters, wrapping in the box›") and the counter "4,000 of 4,000 characters"; at 390 check the box does not widen. |
| D10 | N | Spec 2 (Tokens and contrast) | **Two contrast lines describe the colours wrongly.** "badge-accepted's text: warning-color rgb(154 84 0) on warning-bg": the badge's text is `--text-color`; `--warning-color` is its border (`bundle.css:1963-1966`, `.badge{color:var(--text-color)}`). The border still passes (5.4:1 by computation on the tint). "Focus ring secondary blue on white ≈ 5.2:1": `--focus-ring` is rgb(45 108 223), 4.86:1 on white and 4.61:1 on the page background (`app.css:49, 59`). | `app.css:49, 59, 66-67, 2020-2032` | Reword: the badge's text is the text colour on the tint, its edge `--warning-color`; the ring is 4.86:1 on white. Both pass. |
| D11 | N | Spec 3 N7; M04 | **Download is named for the Export request only.** N7: "named for the type, never 'your export' on an Access request". Only the Export request has a Download board; the Spec says "Download named for the export", so an Access request's name ("Download the data held about you of the Access request"?) is unstated. | `R2-M04-1280` `aria-label="Download the export of the Export request made ..."`; Spec 3 N7 row | Write the name per type in Spec 2's messages table (Access and Export), starting with the visible word. |
| D12 | N | D09, K01, Q07, Q08 | **Q11 reads "the queue and the page read deleted_user_0ae1d64b over 'Erased 2026-10-28'".** The queue does (Q07, Q08); the request page's Requester dd shows the pseudonym alone, and the date is only in the notice. | `R2-K01-1280` `<dd>deleted_user_0ae1d64b</dd>` | Put "Erased 2026-10-28" under the pseudonym in the Requester dd (the `.person-cell` second line), or note in the Spec that the notice carries it. |
| D13 | N | D17, D18 (1280, 390) | **The loading and the failed request draw the trail's ancestors and no id crumb**, though the route has the id; every loaded request page ends in it. Flow 13's D11 was the same. | `R2-D17-1280`: `breadcrumb-ancestor`, `breadcrumb-parent`, no `breadcrumb-current` | Draw Home > Data rights requests > the id on D17 and D18, as K01 does (the id is in the URL). |
| D14 | N | D18, M15, Q12 | **The chip says focus returns to "the reloaded page's h1" but no h1 on any board carries `tabindex="-1"`** (flow 13's R01 draws it). Static markup only; the shipped page focuses on navigate. | `<h1>My data rights</h1>` on all 133 | Draw `tabindex="-1"` on the h1 of the three boards, or leave and say the focus comes from FocusOnNavigate. |
| D15 | N | all textarea/input boards (M01-M18, D01-D19, C01-C10) | **No field has the `required` attribute**, only the visible mark; flow 13's boards set both. The built forms are server-validated (no `required` in `RequestDetail.razor`), so this may be right. | `<textarea id="reason" class="form-control" rows="4" aria-describedby="reason-count">` | State which: either add `required` or say in Spec 3 that the pages validate on press. |
| D16 | N | D02, M02, M13 | **A refusal is named before the help in `aria-describedby`** (`note-err note-h`, `reason-err reason-count`; ids `-err`). DESIGN.md § Form system: a control names "its help, its message, any notice, then the summary", the message id `<key>-msg`; § Button system names the last-day refusal "first". DESIGN.md contradicts itself; flow 13 chose help first, the boards chose refusal first. Same finding as flow 13's D14. | `DESIGN.md:765, 1276-1278` | Decide once in DESIGN.md; if refusal first, say so in Spec 2. |
| D17 | N | K01, C06, D03, D05, D09 | `h2#s-dec` and `h2#s-cor` carry `tabindex="-1"` on boards where focus goes to the result region or nowhere (flow 13's D13). Harmless. | `<h2 id="s-dec" tabindex="-1">Decision</h2>` on K01 | Keep it only where the build focuses the heading. |

## Counts

17 findings: **3 must-fix** (D1 to D3), **6 should-fix** (D4 to D9), **8 nits** (D10 to D17).

Must-fixes:

- D1: the queue's Type and Status selects (38 px) and the pager's page-size select (28 px) are under 44 px at 390, and no rule in the Spec covers them (N3 partly done).
- D2: all three stacked tables drop DataTable's roles and its base `clinic-table--stack` class, against the ask's "a stacked table keeps DataTable's roles".
- D3: the Status cell of every stacked table has no `data-label`, against the ask's "each cell labelled at 390".

The rest of the markup is clean on what the lens checks: one h1 a board, no inline style, no raw colour, no unmarked
class, no dangling id or `aria-*`, the brand mark and shell are flow 01's on all 133, badges are Q22's, the dialogs
are flow 12's, results and refusals are in `ActionResult` and move the focus, times carry SAST, ids wrap, no gendered
pronoun, no bracket filled from memory.

Held, not findings: Approve and erase is an outline trigger and the only filled danger button is the dialog's confirm
(Q3, decided); the Requester `th scope="row"` is the second column with Submitted a plain cell (S4, decided); the
request page's crumb is its id, not the h1 (Q12, decided); the kept request's notice is a standing info Alert with no
role (S10); Illustration boards' invented menus (M18) and the unread appendix are disclosed on the Spec; the board
`<title>`s are the pages' tab titles and the canvas titles are the board names; the digest help's "14 days" and "5 days"
come from round 1's and the flow brief's wording (`14-data-rights.md`), not from the ask, and are the cast reviewer's to
weigh.

Not done in this review (needs the two NEW stylesheets, D6): rendered measurements at 1280, 1100, 768 and 390 (page and
table-container overflow, control boxes at 390, a long address or a 4,000-character reason in a dd). Nothing under
`design/` or `src/` was changed except this file.
