# Flow 13 round 2: the design-system review

Reviewed 2026-10-27 from the design-system side: the 168 boards in `round-2/project/` (H01-H08 Home, X01-X08 the staff
export's entry points, E01-E18 Export, V01-V10 Verify, P01-P19 the trainee profile and the end of a programme, C01-C05
Completed and closed, R01-R10 the ended record pages; 1280 and 390 twins, 768 for eight; plus R2-0-spec, -spec-2, -spec-3,
R2-0-steps, -steps-2) against `design/system/components/bundle.css` (v33), `src/Wombat.Web/wwwroot/app.css` and
`execution/architecture/DESIGN.md`. Model: flow 12's `round-2-review-ds.md`.

Method. Static first, by script over every board: classes against `bundle.css`, `app.css` and the boards' own `<style>`
(one block, byte-identical on all 168 boards: there is no `f13r2.css`); raw colours; ids and `aria-*`/`for` references;
heading order; table captions, `scope`, roles; inline styles; `maxlength`; accessible names. Then rendered: the boards
link `ds/wombat/tokens.css` and `components/bundle.css`, which the folder does not hold, so I served a scratch copy
(tokens and fonts from `app.css`, `bundle.css`, the boards) and measured in Chromium (Playwright, channel chrome,
headless): page overflow and table containers for every 1280 board at 1280, 1100, 1000, 901, 900 and 768 (its width freed),
every 390 board at 390, every 768 board at 768 (163 boards, 400+ renders); control boxes at 390; screenshots of H01, P01,
P03, V02, C04 at their width. Sizes are measured unless marked "computed". Cast, DTOs and code are other reviewers'
remit. Board text is read as data. Severity: M must-fix, S should-fix, N nit.

## What holds

- **Colour, ids, inline style.** No raw hex, `rgb()` or `hsl()` in any board's markup; no inline `style=` on any board
  (flow 12's D7 and flow 11's D17 are not repeated); no `maxlength`; no duplicate id; no dangling `aria-labelledby`,
  `aria-describedby`, `aria-controls` or `for` (flow 12's D5 is not repeated). Every field's help and refusal is named by
  `aria-describedby` (help first, refusal after, as DESIGN.md § Button system says).
- **Classes.** Not one class is in no stylesheet (flow 12's D3 is not repeated). Every class that is not in `bundle.css` is
  declared in the boards' `<style>` and is in the spec's NEW table or its canvas-only list: nothing is unmarked. Only
  `.end-help` is listed and used nowhere (N5).
- **Overflow.** No page scrolls sideways at any width on any board (document scroll width equals the viewport on all 163
  boards at every width tried). No table container scrolls at 1280, 1100, 1000, 900, 768 or 390. The one exception at
  901 is a placeholder artefact (D7). The only boxes past the right edge are the clipped, visually hidden column headers
  of stacked tables, as the spec says.
- **44 px at 390.** Every button, field, select and row action in `main`, in the dialogs and in the menu is at least 44 px
  high at 390 (Export PDF, Verify, Download certificate, View, the two end triggers, dialog buttons, the header action).
  The only boxes under 44 are inline links inside a sentence ("Verify portfolio export", 21 px; the activity link in a
  table cell), which WCAG 2.5.8 exempts, and the pager's size select (D8). The scoped rules never touch a bare `.btn`
  (flow 12's D1 is not repeated): `.password-toggle` does not occur here.
- **Dialogs.** Mark complete and Deactivate follow flow 12's banner: safe button first in the DOM, the action on top at 390
  (`column-reverse`, measured on P03-390), Cancel focused on opening, `aria-labelledby`/`aria-describedby` on the dialog,
  the title and body naming the person and the day, "this cannot be undone" in both. The filled danger button occurs once
  in the flow, in P09's and P16's Deactivate dialog; the end's two triggers are `btn-outline btn-sm` with
  `aria-haspopup="dialog"`.
- **Results and failures.** Every done result (E03, E05, E06, E08, P05, P11) is a success `Alert` with `role="status"` in
  an `.action-result` (`tabindex="-1"`) at the head of the page, first sentence bold, naming the file or the person;
  every refusal that is not a field's and every failure is `role="alert"` in the same region; the loading states are a
  visually hidden `role="status"` line plus `.skeleton`; the load failures are `.alert-row` with a Try again outline
  `btn-sm` (flow 11's D5 is not repeated). The standing notices (R01, R02, R05, R10, the Home no-role card) are Alerts
  with no role (flow 12's D13 is not repeated).
- **Tables.** C01-C05's table is captioned (the caption does not repeat the heading), has `th scope="col"`, a visually
  hidden "Actions" header, `th scope="row"` on the name, `data-label` on every cell that needs one, DataTable's explicit
  roles, both stack classes, and a View named for its row ("View Lerato Molefe's record"). The pager is drawn only on C04
  (an Illustration of 40 ended, 20 a page, the select defaulting to 20) and not on the 1, 2 or 3 row boards.
- **Headings and landmarks.** One h1 per board, no skipped level (h1, h2 sections, h3 cards); the dialog stages have none,
  as stages; trails are Home > owner > page with the last crumb = h1; the owner rows are aria-current="true" (Trainees,
  Programme trainees, My progress) with the page itself "page". No two navs share a name.
- **Forms.** Required marks (`aria-hidden` star plus `visually-hidden` "required"), `required`, `aria-invalid` plus the
  invalid border and stripe on E11, E12, P02, V05, V06; Verify is a GET form with `name="check" value="1"` and no hidden
  input; help outside the labels.
- **Phone layout of the new rules.** The 390 screenshots of H01, P01, P03, V02 and C04 show the stacked rows, the Save row
  (Save on top), the end section's note above its button, Verify's one-column details with the ids wrapping, and the
  stacked closed list as DESIGN.md's stacked-row rules describe them; no board CSS breaks one (flow 11's BRIEF § 11 trap
  is not repeated).

## Findings

| Id | Sev | Boards | What is wrong | Evidence | Fix |
|---|---|---|---|---|---|
| D1 | S | R07 (1280, 390, 768) | **My activities has no trail and its table has no caption.** The spec's route table says `/activities/mine` is owned by My progress with the trail "Home > My progress > the page" (and R08, R09 draw it), but R07 draws none (the nav lights My progress `aria-current="true"` and no breadcrumb exists). Its table is the only board table with no `<caption>`; the spec's accessibility line says "Every table captioned". | R2-R07-1280: no `nav[aria-label=Breadcrumb]`, `<table ... role="table">` with no `<caption>` (1 table, 0 captions over the whole flow's boards) | Draw the trail Home > My progress > My activities on R07 at 1280, 768 and 390; add a caption that does not repeat the h2 ("Every activity filed by you or about you, with who has it now and its credit"), as flow 09's STAR register does. If the shipped My activities has none, say so in the build note and add it there. |
| D2 | S | P02, spec 2 "Refusal" | **The last-day refusal is drawn as a field message with the focus in the field; DESIGN.md says the trainee profile's two actions move the focus to their result, done or refused.** § Button system: "Its result goes to the page's `.action-result` region, which takes the focus once it has answered, done or refused. A refusal's `Alert` has an `Id` that the field names with `aria-describedby`". P02 draws a bare `div.validation-message` (no role, not an Alert, not in `.action-result`) and the chip says the focus is the field. Q21 moves the check to before the dialog but does not say it changes where the refusal lives. The spec's "DESIGN.md text each change needs" does not list it. | R2-P02-1280 `#ld-e` `div.validation-message`; DESIGN.md § Button system (the form-page destructive-action bullet) | Either draw the refusal as the danger Alert in `.action-result` (focus there; the field names it by `aria-describedby`), or add the exception to the DESIGN.md text: "a refusal that names the field and is found before any dialog stays beside it with the focus in it, read at once" and give the message `role="alert"` so it is announced when drawn. |
| D3 | S | V02, V03, V07, V09, V10 | **Verify's answer is a hand-built success card, outside `ActionResult`.** DESIGN.md (the page-load bullet): the region is `ActionResult`, given `FocusOnLoad`, done or refused; here the failures use it (V04, V08) but the success is `section.detail-card.verify-answer[tabindex=-1]` with its own 4 px success stripe (`border-left` in the board's CSS) and no region. Two shapes for one result; the answer is also a named `section` (a region landmark) that exists only after a press. `.verify-answer` reproduces `.alert-success`'s stripe by hand. | R2-V02-1280 `section.verify-answer`; V04 `div.action-result > div.alert-danger` | Put the answer in the page's `ActionResult` (FocusOnLoad) as one container: the success Alert's first sentence ("Export verified.") as the heading line, the `dl` beneath it; or keep the card and say in the DESIGN.md text it is the page-load result's one exception, with `.verify-answer` as the stripe's only class. Decide once. |
| D4 | S | V02-V10, spec 1 NEW table | **`.verify-details` is a second `.detail-list`.** `bundle.css:1895` is a `dl` grid, muted `dt` weight 500, `dd` margin 0; `.verify-details` is the same with `max-content` for the first column, `minmax(0,1fr)` for the second and a 390 stack. Flow 12's D8 asked for the same reuse. The only real difference is the wrap of a 64-digit hash, which `.hash-value` already does. | `helmet` `.verify-details`; `bundle.css:1895-1910` | Use `.detail-list` (give it `dd{min-width:0}`/`minmax(0,1fr)` in the system if a long value needs it) and drop `.verify-details`. If kept, the NEW table must say why `.detail-list` is not enough. |
| D5 | S | C01-C05, spec 1 NEW table | **`.closed-when` is `.person-cell`.** A column with a muted second line (`bundle.css:4615`: `display:flex; flex-direction:column`, muted `span`, 0.9rem, wrap anywhere), the same as `.closed-when` (flex column, muted `span` .9rem). The Curriculum cell of the same table already uses `.person-cell` for exactly this. | `helmet` `.closed-when`; `bundle.css:4615-4624`; C02 row 1, two adjacent cells | Use `.person-cell` for the Programme cell and drop `.closed-when` from the NEW table. |
| D6 | S | C02, C04, C05, P11 | **Withdrawn and Inactive are drawn with `badge-draft`.** The class means "draft"; DESIGN.md's Badges list (data rights: "withdrawn grey") has no Withdrawn or Inactive tone for a profile. P11's Status is a grey badge "Inactive" while P01's "Active" is a plain word (N9), so the two states of one field look unlike. The spec says "v33 ships no Active badge tint": it does not say it picked `badge-draft` for the other two, nor that Completed uses `badge-completed`. | `R2-C05-1280` `span.badge.badge-draft` "Withdrawn"; `R2-P11-1280` `dd > span.badge.badge-draft` "Inactive"; spec-3 N9 | Add the two words to DESIGN.md § Badges (Completed green, Withdrawn grey) and name the class (`badge-ended`/`badge-withdrawn`, an alias of the grey tone), so a build is not a "draft" badge; draw Active, Inactive and Completed alike in the summary (all plain words or all badges). |
| D7 | S | C04 (901 only), spec-3 | **The "no sideways scroll at all widths" claim is false at 901 on C04, by a placeholder.** C04's `.table-container` is 593 px at 901 and its table 622 px (29 px scroll); C01, C02 and C05 fit. The cause is the 18 `‹Completed or Withdrawn›` badges, whose `.badge` is `white-space:nowrap` (a 190 px Outcome column). The real words ("Completed", "Withdrawn") do fit (C02, C05 at 901: scroll width equals client width), but they fit with no slack: the columns sum to the container, so a longer name or a two-word outcome would scroll the closed list between 900 and 1000 px. Not measured with a real 20-row set: the rows are placeholders. | measured `R2-C04-1280` at 901: `table-container 622>593`; at 950: fits; cols at 901 on C01: 90, 152, 137, 131, 82 = 592 | Replace the bracket badge with a real word in the Illustration, and give `.closed-when`/`.person-cell` cells `overflow-wrap:anywhere` (already on `.person-cell span`) plus `.col-wrap` on the Programme column, as flow 11 did for Users, so the table has slack below 1000. |
| D8 | N | C04 (390, 768) | **The pager's page-size select is 28 px high at 390** (72x28), where every other control is 44 px. The shipped phone rule raises only `.pager .btn` (`bundle.css:2719`). Pre-existing in every flow's pager; this flow draws the pager once. | measured C04-390 `SELECT.pager-page-size-select` | Add `.pager .form-select{min-height:2.75rem}` at 390 to the system, or state the exception. Not this flow's to fix alone. |
| D9 | N | all boards | **The NEW rules exist only inside each board's `<style>`, mixed with the canvas-only chrome.** Flow 12 shipped `f12r2.css` with each rule marked NEW; here 168 copies of one block hold 33 NEW rules and the `.sheet`, `.chip`, `.anno` canvas rules together. Nothing is wrong on a board, but a build must diff 168 blocks, and a rule changed on one board silently differs. | md5 of the `<style>` block: one hash on all 168 files | Move the NEW rules to one `f13r2.css` (as flow 12) and keep the canvas rules apart. |
| D10 | N | E07, E08 | **Two `aria-current="page"` on one page**: the menu's Export portfolio (lit itself) and the crumb "Export portfolio" in the trail "Home > Export portfolio". The spec marks the trail PROPOSED ("a personal link's own page draws no trail elsewhere: your call"). Valid markup; but the page names itself twice and the one trail is the only one for a personal link's own page. | `R2-E07-1280` two `[aria-current=page]` | Decide the trail once (spec 2, Routes); if kept, say in DESIGN.md that a personal link's own page may draw a one-step trail. |
| D11 | N | E14, E15 | **The loading and failed export drop the trail that the loaded export draws.** E04 draws "Home > Trainees > Lerato Molefe's programme > Export portfolio"; E14 and E15 (the same page loading or failed) draw none, though the nav owner is still lit; P14 and P15 (the profile loading or failed) do keep theirs with "Trainee profile" as the last crumb. | `R2-E14-1280` and `R2-E15-1280`: no `nav[aria-label=Breadcrumb]` | Draw Home > Trainees > Export portfolio on E14 and E15, as P14 does. |
| D12 | N | P06, P08 (end section) | **The two end triggers are named by their visible word only.** "Mark complete…" and "Deactivate…" have no `aria-label` naming the row, and the sentence that says what each does is a sibling `span.role-row-note`, not tied to the button; flow 12's rule is "an outline trigger named for its row" ("Remove Assessor from Mohammed Patel"). Here there are only two distinct names on a page, so it is understandable, but the note is not read with the button. | `R2-P06-1280` `li.role-row > span.role-row-note + button[aria-haspopup]` | `aria-describedby` from each button to its note (ids on the notes), or `aria-label="Mark Lerato Molefe's programme complete…"` starting with the visible word. |
| D13 | N | all P with the end section | **`h2#end-h` carries `tabindex="-1"` with no board saying why** (the focus goes to the field on a refusal, to `.action-result` on a result). Harmless; a tab stop never, a focus target nobody uses. And `.end-decision` and `.end-help` are no-op classes (`margin:0`; `.end-help` is used nowhere). | `R2-P01-1280`; `helmet` | Drop `tabindex` unless the build moves the focus there; drop `.end-help`, and `.end-decision` if `p` is left alone. |
| D14 | N | V01-V10 | **The hash field's description is long**, 59 words of help plus a three-line command list in `aria-describedby`, and the refusal is read after all of it (V06: `hash-h hash-cmds hash-e`). Consistent with DESIGN.md (the refusal follows the help), but for this field the refusal is the news. | `R2-V06-1280` `aria-describedby` | Put the refusal id first on V05 and V06 (`hash-e hash-h hash-cmds`) and say so in the DESIGN.md text. Optional. |
| D15 | N | H01, H02, R01-R02 (1280) | **Count and size of targets at 1280**: `.record-links a` and `.record-line a` are 24 px high at 1280 (44 only at 390 by the scoped rule): acceptable (WCAG 2.5.8) and as flow 12's D11. The H02 plain-text STARs row has no relation to its sentence (`.record-link-none` is not `aria-describedby`'d): read in order, so fine. | measured | None; noted as accepted. |

## Counts

15 findings: **0 must-fix**, **7 should-fix** (D1 to D7), **8 nits** (D8 to D15).

Must-fixes: none. The flow's markup is clean on the rules the lens checks (every id and reference resolves, no inline style,
no unmarked class, 44 px at 390, no overflow at any width, dialog and result patterns as flow 12 banked them). The should-fixes
are three pattern splits from the system (D3 the verify answer, D4 `.verify-details`, D5 `.closed-when`), one DESIGN.md
conflict to decide (D2 the last-day refusal), a missing trail and caption (D1), the Withdrawn/Inactive badge (D6) and a
placeholder that hides a 901 px risk (D7).

Held, not findings: Mark complete's confirm is `btn-primary`, not danger (Q24, decided; both dialogs say it cannot be
undone); the `‹…›` brackets for values the fixtures do not give; the board `<title>`s are the board names, not "Page · Wombat"
(canvas); the brand mark renders as a broken image only because the scratch copy has no `/_blob/` (not a board defect); the
spec's claim that the captures did not arrive is another lens's.

Scratch render used for the measurements (not part of the repo): a copy of the boards with `tokens.css` and the fonts from
`app.css`, `bundle.css` v33, served locally under the session scratchpad (`g376/r2ds`); nothing under `design/` or `src/`
was changed except this file.
