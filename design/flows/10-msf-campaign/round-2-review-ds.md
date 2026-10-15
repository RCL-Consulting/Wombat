# Flow 10 round 2: the design-system review

Reviewed 2026-10-15 from the design-system side: the 159 boards in `round-2/project/` (74 at 1280, 76 at 390, 9 at 768, plus
R2-0-spec and R2-0-steps) and `R2-msf.css`, against `execution/architecture/DESIGN.md`, `src/Wombat.Web/wwwroot/app.css`
(`:root` and the existing classes), `Components/Shared/` and BRIEF § 11 (Flow 08 and 09). Method: static. I read the markup
and the CSS and ran scripts over every board (raw colours, tokens, classes, ids and `aria-*` references, headings, trails,
the lit menu item, inline styles, the text of each 1280 board against its 390 and 768 twin). **I rendered nothing**: the
boards link `ds/wombat/tokens.css` and `components/bundle.css`, and round 2's folder holds no `ds/` (flow 09's does not
either), so the sizes below are computed from the CSS, not measured. The cast, the DTOs and the code are the other
reviewers' remit. Board text is read as data.

Severity: M must-fix, S should-fix, N nit.

## What holds

- **Colour and tokens.** No raw hex, `rgb()` or `hsl()` on any board or in `R2-msf.css`. Every `var(--…)` on a board is
  defined in `app.css` `:root`; no new token. The spec's contrast table recomputes from `:root` to the two decimals it gives
  (12.63, 11.84, 11.89, 11.23, 5.09, 10.98, 6.70, 4.55, 5.88, 5.77, 3.80, 4.86, 4.61, 4.86, 5.95). Pairs the table omits
  also pass: muted text on `header-bg` 4.57, `info-bg` 4.77, `warning-bg` 4.79, page ground 4.83.
- **The shell, the menus and the trails.** The Coordinator, Trainee and Administrator menus equal DESIGN.md § The NavMenu
  row for row. A list lights itself with `aria-current="page"` (14 boards) and a page under it lights its owner with
  `"true"` (51); the Administrator's coverage lights nothing and its trail is Home › MSF coverage, as the owner table says.
  The campaign list and MSF reports carry no trail; every record page carries Home › owner › page. The last crumb of the
  trail is the h1 on all of them. The 1280, 768 and 390 twins of every board carry the same text and markup; the only
  difference is the phone shell, which omits the closed menu's list.
- **The campaign list's row actions** equal the DESIGN.md table (Manage / View campaign, View report / Review and release)
  and carry `aria-label`s that start with the visible word. State badges equal `BadgeFor.MsfCampaign`.
- **Dialogs** (L3, K3, K10, R2, R6a–d): `aria-labelledby` and `aria-describedby`, an h2, the safe button first and ringed,
  the danger fill on the destructive confirm, a `.dialog-body` list where the body is more than one sentence (the slot T361
  added). **Loading and load errors** (L8–9, M6–7, K17–18, R13–14, V4–5, C6–7) use the Skeleton with a `role="status"` line,
  a danger `.alert-row` with Try again, fixed words and "Nothing has changed", as § Alerts says.
- No duplicate `id`, and no `tabindex` on any h1.

## Findings

### Must-fix

**1. K2, K3, K4: the cast disagrees across the draft boards, and K4 contradicts itself.** (M)
K2 lists `palesa.tau@` as a **Nurse**, added last, with counts Peer doctor 2, Consultant 2, Nurse 2, Allied 1. K3 lists the
same address as an **Allied health professional**, second in the list, with counts Nurse 1, Allied 2, and its dialog names
her Allied. K4, the result of that remove, lists six rows (Nurse 1, Allied 1) and says "Opening emails each of the **7**
invitees" under a table whose All groups row is **6** (the heading above the rows says "6 addresses"). The build copies
what it sees; step 3.36's Expects count "2, 2, 2, 1, All groups 7" before the remove. *Rule:* § The MSF campaign page,
invitee counts and "the counts and the list are read again", and the runbook's step 3.36. *Fix:* palesa is one group in all
three; K4's sentence reads 6; and K3's list keeps K2's order ("in the order they were added", DESIGN.md § The MSF campaign
page).

**2. The required mark is not DESIGN.md's, and the forms disagree on which fields carry it.** (M)
Every board writes `Label <span aria-hidden="true">*</span><span class="visually-hidden">required</span>` with no
`.required-mark` class (so the star is body text, not `--danger-color`) and no `aria-required="true"` on the control
(C1–C5 Template, Trainee, Opens on, Closes on; the EPA legend). DESIGN.md § Form system, "The required mark" (line 1315):
"The label ends in `<span class="required-mark" aria-hidden="true"> *</span>`, and the control carries
`aria-required="true"`. One line above the first section explains it: '* marks a field you must fill in.' … A required
group keeps its legend's visually hidden 'required'." So: the hidden word belongs to the group (the EPA legend), not to a
single control's label (a screen reader hears "required" twice once `aria-required` is added); the explaining line is
missing from the create page (only `/msf/respond` has its own, "Questions marked * must be answered."). And R12's claim, "the
form has one rule for every field", is not true on the boards: the Add an invitee email (K1–K15) and, in the fold, Template
name, Scale question and Comment question carry no mark, though the first is required (K5b refuses it blank) and Create is
refused without a template. *Fix:* the markup above on every required control, the one explaining line on C1–C5, and decide
the fold's and the invitee form's fields (operator question 3). Also state in the spec that R12 changes the markup, not
only which fields carry a star.

**3. The status card's stripe: the spec says one thing, R7 does another, and K11 contradicts R7.** (M)
Spec: `.status-card` "on `.detail-card--emphasis` in every state" and "a warning is an Alert inside it, never a tint on the
card". R7 and R7b override it inline, `style="border-left-color:var(--success-color)"`, while K11 (the campaign page of the
same Released state) and K12 (Withdrawn, nothing left to do) keep the blue stripe. `.detail-card--emphasis` is, in
`app.css` and DESIGN.md, the action blue that says **the viewer's move** (the activity page's status card, line ~2206 of
app.css: "the viewer's is `.detail-card--emphasis`, the action blue; someone else's the control edge; done green; declined
red"). So blue on Withdrawn and Released is wrong by the system's own meaning, and green on R7 alone is an unlisted variant
with an inline colour. *Fix:* choose one rule (operator question 1) and write it as a class, not an inline style: either
blue only where the viewer has a move (Draft, Open on the report with Close, Under review) and the control edge / green /
red elsewhere as the activity card does, or blue throughout with R7's override dropped. Either way list it in the spec's
"NEW and changed" row for the status card.

**4. C4: the refused create is not the system's refusal summary, and its link goes nowhere.** (M)
C4 draws `<ul class="validation-summary-errors" tabindex="-1"><li class="validation-message"><strong>Not created.</strong>
<a href="#epa-h">…</a>`. DESIGN.md § Alerts, "A refusal that names fields" (line 1692): `class="validation-summary-errors
error-summary"`, **`role="alert"`**, `tabindex="-1"`, a bold title saying what did not happen and what state the record is
in, a line saying what to do, then one line per field, "each a link to the field's input"; the `href` is only for a page
with no circuit, because `<base href="/">` makes a bare fragment navigate to Home and lose what was typed, so a press moves
the focus by script (`PageFocus.FocusByIdAsync`) and "every target can take the focus". Here: no `role="alert"` (so the
refusal is not announced), no second line, and the target `#epa-h` is a `<legend>`, which cannot take the focus. *Fix:*
build C4's summary as `RefusalSummary` does, link it to the first EPA checkbox (`#e1`), and say in the spec which of the two
refusals (summary or the fieldset's own `.validation-message` under the legend, which C4 also draws) is the one that takes
the focus.

**5. The 44 px target for dialog buttons at 390 rests on a board-only selector.** (M)
`R2-msf.css` sets 44 px on dialog buttons with `.dlg-overlay .btn` and `.dlg-overlay .form-actions`, and the file's own
header says `.dlg-overlay` is "the canvas's own … never shipped". `app.css` has no phone rule for `.dialog-card .btn` (`.btn`
is 36 px; `.dialog-actions` has its own phone rule only in form mode). In the app the dialog is a `<dialog>` inside the page
component; whether it sits inside `.msf-page` decides whether `.msf-page .btn{min-height:2.75rem}` reaches it. This is
BRIEF § 11 "Flow 09" again ("a page with no wrapper class gets none of its flow's scoped phone rules": My committee reviews'
View stayed 28 px). *Fix:* write the dialog rule against what ships (`.msf-page dialog .btn`, and say where the
`ConfirmDialog` instances are placed so they are inside `.msf-page`), and list the stacked, reversed action order at 390
(`column-reverse`: safe button last on screen but first in the tab order) as a rule the spec changes; the DESIGN.md dialog
paragraphs do not say it.

### Should-fix

**6. `.row-actions` is `.actions-cell` again.** (S) Spec "Row actions below 641px"; `R2-msf.css` `.row-actions{display:flex;
flex-wrap:wrap;gap:var(--space-sm)}` is, declaration for declaration, `app.css` `.actions-cell` (line 442), which DESIGN.md
§ Table system (line 855) already makes the one container for a row's buttons ("a cell's buttons sit in a `div.actions-cell`
inside the `<td>`", held by `TableColumnClassTests`). A second class for it is a NEW component with no justification. *Fix:*
`div.actions-cell` on L1–L7 and M2, with the phone rule scoped as `.msf-page .actions-cell`.

**7. The release section's check line is `.check-line`, which does not exist; the component is `.submit-check`.** (S) The
spec's components table names SubmitCheck and gives its class as `.check-line`. `app.css` (line 1634) and
`SubmitCheck.razor` use `.submit-check` (same tint and stripe; padding `0.75rem var(--space-md)`, not `--space-md`).
`.check-line` is redefined in `R2-msf.css`. *Fix:* use `.submit-check` and delete the redefinition; where the padding should
differ, say so.

**8. The spec's NEW table is not the list of what the boards use.** (S)
The task's rule is that a NEW component is marked NEW and justified. `R2-msf.css` defines 15 classes the boards use that no
spec row names, and several have an existing equivalent:

| Class (boards) | Where | Existing equivalent or the gap |
|---|---|---|
| `.msf-header` | every page | the page header; needed only for the phone rules, so name it as such |
| `.section-title` | 43 boards | `.list-section-title` (list sections) and the card titles; a third h2/h3 style |
| `.count-table`, `.count-box`, `.num` | K, R, 13 boards | `.clinic-table` plus `td` alignment; `.num` is a utility with no precedent |
| `.nowrap`, `.kind-line`, `.trainee-cell` | L, M | `.person-cell` (name over a muted line) covers `.kind-line` and the address/group pair of `.invitee-address`/`.invitee-group` |
| `.stored`, `.msf-facts` | R7, K | `.details-list` (a `dl` of `div`s, with `--stacked`) and `.details-grid` |
| `.groups-intro`, `.group-avg` | R, M | muted line, a span in an h3: fold into `.group-card` |
| `.skeleton-stack` | six loading boards | `Skeleton` with `Count` and no wrapper (the system draws siblings; the wrapper's gap is not DESIGN.md's) |
| `.choice-stack`, `.epa-fieldset.is-invalid` | C | `.choice-row` + `.choice-group(.is-invalid)` (finding 10) |
| `.coverage-windows`, `.coverage-programme`, `.is-covered` | V | spec says coverage uses "unchanged components"; these three are NEW |
| `.respond-label`, `.respond-required`, `.respond-meta` | Q | NEW in the respondent card; only `.respond-card`/`.respond-privacy` are listed |

*Fix:* every class the boards use gets a row (NEW, or "reuses `.x`"), with the justification where it is NEW, and
`.num`/`.nowrap`/`.section-title` either dropped for existing classes or justified once.

**9. DESIGN.md text the boards change that the spec does not list.** (S) The spec's "NEW and changed" table is the checklist
for the DESIGN.md rewrite; these are missing: (a) § The MSF campaign page's invitee counts, addresses and invitee form are
`<h4>`s in DESIGN.md; the boards use h2 and h3 (R11 mentions headings, but not that the text changes); (b) the campaign card's
`details-list` (Trainee, Template, State, Response window) is replaced by the status card and "About this campaign", which the
spec lists, but the State row and the Response window moving into the subtitle is not said; (c) the "Quick template card"
becomes the Questionnaire card with a fold; (d) the required mark's markup (finding 2); (e) which tables do not stack
(finding 11); (f) the `/msf/respond` states drop "the state's Alert" (finding 14); (g) the status stripe (finding 3);
(h) the draft Remove's `aria-label` ("Remove grace.mokwena@… (Nurse)") is kept but its container changes from a table row's
cell to a list item, which § Table system's column-header rule (line 855) no longer covers.

**10. C: the EPA group is a card title that is not a heading, and its invalid state has no contrast-theme mark.** (S)
(a) The other three create cards open with an h2 (`msf-card-title`); the fourth, "Evidence for these EPAs", is a `<legend
class="msf-card-title">` in the same size, so the page has three h2s for four equal-looking sections and a heading list that
skips the one the refusal names. DESIGN.md line 1194 (T188): a group of checkboxes is a `.form-group` "whose legend reads as
a label". Either give the card an h2 and keep the legend a label inside, or make all four cards legends (they are not: the
other three hold several fields). (b) `.epa-fieldset.is-invalid{box-shadow:inset 4px 0 0 …}` has no `forced-colors` rule, but
`.choice-group.is-invalid`, which is the same thing for a group of radios, does (a `CanvasText` border; DESIGN.md line 2983:
an invalid field "stays marked in a contrast theme", WCAG 1.4.1, T236). *Fix:* reuse `.choice-group.is-invalid`.

**11. Coverage (V1, V3) and the count tables do not stack, and nothing says why.** (S)
DESIGN.md stacks a list table below 641 px (`.clinic-table--stack`; line 1007 "A list that must stack at 900px"; the
progress table and the ratings table stack too, lines 1517 and 1877). V1's two tables are `.clinic-table.coverage-table`
with `td{white-space:nowrap}`, a row header at `min-width:16rem` (12rem at 390), each cell "0 of 5 trainees covered", inside
a focusable `role="region"`. At 390 that is a sideways scroll region on the page's main content; at 768 (no board) the main
column is ~518 px (768 less the 250 px sidebar), against about 40 rem of table, so it scrolls there too. The scroll region
with a name is DESIGN.md's answer for Decisions due's summary (a wide, dense table), not for a two-column table of sentences.
The count tables (K, R: three columns, short labels) fit without scrolling and may stay unstacked. *Fix:* stack V1 as
`EpaProgressTable` does (EPA as the row header, the semester a labelled cell), or list the scroll region as a deliberate
exception in the spec; draw V1 at 768. (Operator question 2.)

**12. The Add an invitee row at 768 is cramped; the stack point is 641.** (S)
`.add-invitee` is three columns (`2fr 1.3fr auto`) until 640.98 px. At 768 with the 250 px sidebar the card's content is
roughly 400 px; after the button (about 120 px) and two gaps the email gets about 155 px and the group select about 100 px:
"grace.mokwena@kgk.wombat.local" and "Allied health professional" both truncate in a field the person types into. K2 is drawn
at 768 (the only add form that is), so the board shows it; I could not measure it. `.add-invitee > .btn{margin-top:1.95rem}`
also hard-codes the label's height to line the button up (a wrapped label, or a larger text size, breaks it). *Fix:* stack
the form below the width a stacked list uses (900 px, as DESIGN.md line 1007), and align the button with `align-self:end` on
a grid that puts it beside the controls rather than a margin.

**13. A link in a sentence is a small target at 390 on R, and the "Open its campaign page" link breaks flow 07's pattern.** (S)
"Open its campaign page" ends the `.page-subtitle` of R1–R11 ("Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28
· Open its campaign page"), and "withdraw it on its campaign page" sits in `.field-help` and in the disabled Release's reason
(R3, R5, R8a …). Neither is padded: `R2-msf.css` pads `.portfolio-links a` only. BRIEF § 11 Flow 08: "pad a sentence's link,
never make it a block" (the 19 px links, 27 px after inline block padding). DESIGN.md line 3019–3023 is the precedent for a
link in a header: "a link alone on its line (`.review-progress-link`, `PageHeader`'s `SubtitleContent`)", 24 px, 44 px below
641 px. A link at the tail of a dot-separated subtitle is neither. *Fix:* the campaign-page link alone on a line under the
subtitle (or in the status card), and an inline-block padded rule for the two help-text links.

**14. `/msf/respond`: the states lose "the state's Alert", and no board draws the refused post.** (S)
DESIGN.md § Anonymous static page (line 4118): "brand lockup, `<h2>`, the state's Alert". Q3–Q8 draw `<p>` text under the h1
and no Alert (Q3 Thank you as well). The spec lists only h2 → h1 as the change. Either the Alert stays (an Alert Kind per
state: info for "already used", warning for "expired", danger for the fault) or the spec lists its removal. Separately: the
page's refused post ("Error: …" tab title, `.error-summary` that takes the focus on load, `aria-invalid` on the comment, the
rating's fieldset naming the summary) is a stated rule and has no board, though the h1 change touches it and the rule "no
`tabindex` on any h1" must sit beside `tabindex="-1"` on the summary. Q2 draws the browser's own prompt only. Also: the
help says "Up to 4000 characters" while the release form's counter says "4,000" (N below).

**15. Inline `style=` stands where a class should, and the measure differs between sibling pages.** (S)
Counts over the 159 boards: `max-width:60rem` on `.msf-page` in 42 (all the report pages R1–R14 and none of the campaign
pages K1–K18, which draw the same status card and the same About card at the full column), `max-width:52rem` on M2/M3 and
`.msf-form-column`, `display:grid;gap:var(--space-md)` 58, `font-size:1rem;margin:0` on 17 h3s, `grid-column:1/-1` 33,
`border-left-color:var(--success-color)` 5, `width:100%;height:3rem` on every skeleton. DESIGN.md forbids inline `<style>`
blocks, not `style` attributes, but a build that copies these writes 40 inline widths into Razor, and "K shows the report's
card at 1030 px, R at 960 px" is not a decision anyone made. *Fix:* one measure for the status and about cards on K and R
(or say why they differ), as a class on `.msf-page` modifiers; `.section-title--h3` for the 1rem h3s; `Skeleton`'s own
`Width`/`Height` parameters, which exist, for the bars.

**16. Boards the spec's own rules call for are missing.** (S)
(a) Her report page (M3) has no loading, no load-error and no unavailable board; M5 draws "not found" on the list only (a
page-level "unavailable" for a record that is not hers is DESIGN.md's C7 pattern, line ~1713, and the title rule, line
3019, wants an h1 "MSF report" while it loads). (b) No board shows a refused Open, Withdraw, Resend, Close or Release (danger
`Alert`, focus left on the button for the retry, DESIGN.md lines ~2036–2060 and the report-page paragraphs), nor the result
of a Close (R3's opening banner is the only trace), though § Alerts' "A refusal … takes the focus" is a rule for each.
(c) Of the new pages only the lists, K2, R1, R3, R7 and R8a have a 768 board; C (the 52 rem form column with a two-column EPA
grid), K7/K8 (`.status-split` with a table beside the Links part), V1 and the dialogs have none. BRIEF § 11 asks for 768 on
every new table or grid.

**17. The boards still carry 33 files of ‹placeholders›.** (S)
K10 ("‹The rest as built.›" for the Withdraw dialog's second half), K12 (the Withdrawn status line ends "‹the rest as
built›"), R12 ("‹Its sentence, as built.›"), K5b (the malformed address's refusal), ‹date› ×17 (K14, R4, R6d, R10), and
`PAED-0nn` in R6b. These are words `MsfCampaignText` and the DTO give, so the blanks are checkable now. A placeholder
cannot be checked for wrapping or contrast, and a 4,000-character comment (R11) is drawn as the placeholder, not as the long
text the board exists to test (`overflow-wrap:anywhere; white-space:pre-line` on `.group-comments li` is untested at 390).

### Nits

**18.** (N) C1 and C2 aria-describedby="epa-help" on the EPA fieldset, but "Select a trainee first." has no `id="epa-help"`:
a dangling reference (C3–C5 have the id).
**19.** (N) The "2 campaigns" and "10 campaigns" count chips on L2–L7 use `badge-draft`, the Draft state's class (grey); a count
in a heading is a badge to the eye and words to a screen reader (§ List page, line ~3159), so a state class on a number can be
read as the state. No other count class exists; if grey is wanted, say it is `badge-draft` on purpose.
**20.** (N) C1 and C2 show the thresholds as 8 / 3 / 2; C3 to C5 (the same page, after choosing a trainee) show 5 / 2 / 2, with no
chip saying the person edited them; K12 carries 8 / 3 / 2. Which is the form's default?
**21.** (N) R7 and R7b head the status card with a visually hidden h2 "Status" and a one-line `.report-line`; K11 (same state) and
R3 give the card a visible sentence h2. Fine for the headings list (both exist) but the two Released cards read differently.
**22.** (N) 390 boards omit the closed phone menu's list and the account row that flow 09's 390 boards keep, so the Trainee's MSF
reports item and the Coordinator's MSF campaigns item are never shown at phone width, though the spec's components table
lists the menus as used there.
**23.** (N) `round-2/project/` has no `ds/` (the boards' two stylesheets) and flow 09's has none either, so no round can be
re-rendered from the repo; BRIEF § 11's note about writing `tokens.css` by hand applies. Save the canvas's `ds/` with the round.
**24.** (N) Count tables (K, R) are in `.table-container` without `.shadow` (DESIGN.md: "wrap every table in `table-container
shadow`"); fine inside a card, but say so. `.template-fold > summary` is `--link-color` and 24 px tall at 1280, exactly the floor;
it is not a link, so the colour reads as one. K13 disables the email and group controls natively while the button is
`aria-disabled`: DESIGN.md's "never disable natively" is about buttons, so this is allowed, but say it.
**25.** (N) The Release dialog's confirm is `btn-primary` ("Release to Lerato Molefe") and Close campaign's is `btn-danger`;
both are irreversible ("cannot be undone"). Close removes addresses, Release writes records, so either can be argued;
the spec should say which reading it takes (operator question 4).

## Does the spec list every DESIGN.md rule it changes?

No. Findings 2, 3, 5, 9 and 14 are rules the boards change that the spec's table does not name; finding 8 is the classes
that table omits. What it lists correctly: the status card, Links part, address rows, add form, anonymity caption, About,
threshold list, group card, suppressed line, release section, narrative counter, the create page, `/msf/respond`'s h1, her
report page's title rule and owner-table row, coverage's subtitle, and the 641 px row actions.

## Operator questions

1. **The status card's stripe.** Blue (`--secondary-color`) on every state as the spec says, or the activity page's rule
   (blue = your move, the control edge = someone else's, green = done, red = withdrawn)? R7 and K11 disagree today.
   Recommendation: the activity page's rule, so a Released or Withdrawn card is not blue; list the colours in the spec.
2. **Coverage at 390.** Stack V1 as the progress table does, or keep a named scroll region? Recommendation: stack.
3. **Required marks.** Mark the Add invitee email, the fold's three fields and the Teaching context (K15), and add the one
   explaining line on the create page? Recommendation: yes; R12 already says one rule, and the line costs nothing.
4. **Danger on Close campaign, primary on Release.** Keep, or both danger (each says "cannot be undone")?
5. **"Open its campaign page".** Alone under the subtitle (flow 07's pattern), or an action in the status card?
