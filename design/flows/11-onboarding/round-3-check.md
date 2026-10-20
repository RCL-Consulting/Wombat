# Flow 11 round 3: the check

Round 3 (canvas "Wombat 11", saved in `round-3/project/`: 203 boards `R3-*.dc.html`, all on page `r3` of `canvas.json`, plus
`r3.css`, `r2.css`, `f11.css` and `ds/wombat/tokens.css` + `components/bundle.css`) was checked item by item against
`round-3-ask.txt` by a Sonnet agent for the main session, 2026-10-20. Method: every board's HTML with style, script and svg
stripped (class names and attributes read, text read); the Spec, Spec 2 and Steps boards read in full; round 2's boards diffed
for names, addresses, dates, times and counts; the built code (`InvitationText.cs`, `InvitationsList.razor`, `MsfCampaign`-style
rule files, `UserAdministrationRules.cs`, `TraineeAdministrationRules.cs`, `RegisterOutcome.cs`, `InvitationRefusedException.cs`,
`DashboardFrame.razor`, `TraineeProfileEdit.razor`, `ConfirmDialog.razor`, `wwwroot/icons`) opened where the ask says "as built".
**Rendered:** the folder was served with `python -m http.server` and drawn in Chrome through Playwright (the boards render without
`support.js`; the canvas pills `.anno` hidden for measuring). Measured: page sideways scroll (`scrollWidth - clientWidth`) for L04,
L12, L13, T03, T06, L05, L14, T01 at 1280, 1100, 1000, 901, 900, 768 and 390; the 390, 768 and 1280 boards of L04, L12, L13, T03
and N21 at their own width; **every** `R3-*` board at its own width (200 boards); the revoke dialog's buttons; every `.btn` on every
390 board. Screenshots were read for L04 (901), L13 (1280, 1000), T03 (901), N21 (390) and L06 (390).

## Verdict

Tally: **F 9 done, 1 partly. M 10 done, 1 partly. S 22 done, 2 partly. Steps board: 7 of 8 done, 1 partly. Nits: 12 done, 1 partly.**
Nothing not done. The M6 target as worded (0 px sideways) is met everywhere, and M7, M8, M9 and the Illustration list check out when
rendered. Two things need a line before the build starts (see "Must fix before build"): the Invitee row header is drawn
`.col-wrap`, which splits every address mid-word at every width from 901 to 1280, and M12's heading names a registrar before any read.
No invented data; every name, address, date, time and count matches round 2 or follows from it.

### Measurements (rendered)

- Sideways scroll, page and table container, px: **0 at all seven widths for L04, L05, L12, L13, L14, T01, T03, T06.** The 390 boards of
  L04, L12, L13, T03, N21 and the 768 boards of L04, L13, T03: 0. A sweep of all 200 `R3-*` boards at their own width (1280, 768 or
  390): **0 on every one**. The Spec's table and its "all 199" are true.
- Revoke dialog at 390 (L06-390): Revoke invitation 44 px, Keep the invitation 44 px, both 244 px wide, Revoke on top and Keep last on
  screen; DOM order is Keep first, so first in tab order. At 1280 both are 36 px (the rule is below 641 only). The rule in `r3.css` is
  the general `.dialog-card .form-actions` one asked for, not a page wrapper, and `ConfirmDialog.razor` does carry `dialog-card`.
- Every `.btn` outside the sidebar on every 390 board is at least 44 px high (header "Invite a person", "Back to trainees", "Try
  again", row buttons, form actions). None under 43.5.
- N21-390 with the 64-character address (47 x's + `@kgk.wombat.local` = 64): the result alert wraps (`overflow-wrap:anywhere`), the
  link label breaks at the `<wbr>`, no sideways scroll. L13-390 carries it in a row, 0 px.
- Words splitting inside themselves while the page does not scroll (the cost of "no nowrap, no min-width"): see Must fix 1 and Build notes 1 and 2.

## F1-F10

- **F1 (partly).** M01-M03, M06, M07, M09, M13, M15 read the curriculum as `dl.form-group` / `dd.readonly-value` with "Adopted at Kgosi
  Kgari Teaching Hospital for Paediatrics / Paediatrics, the invited sub-speciality."; M14 is the one `select` (Curriculum, starred,
  Illustration, "Admitting sets the registrar's scope from it."); no M05 or P06 board exists; P04's address is `‹a registrar's address›`
  (the Spec's F1 row says "P04 now ‹a registrar's name›": it is the address, a wording slip on the Spec). **Partly:** M04, the board for
  "none adopted for the invited sub-speciality", reads the code's old words, "This institution has adopted no curriculum yet, so no trainee
  can be admitted." (`TraineeProfileEdit.razor:293`). After F1 the refusal also fires when the institution has adopted other curricula
  but none for her sub-speciality, and those words are then false. The ask named M04's NothingAdopted without new words; the build needs a
  sentence that names the sub-speciality ("... for Paediatrics / Paediatrics"). See Build note 3.
- **F2 (done).** No "Resent" and no ResentOn on any board (the word survives only in the Spec's F2 row and in Steps 2.26's "built Resent
  words", which means `InvitationText.Resent`'s sentence). L10's row: 2026-10-19 over 10:23 SAST, Being sent, Revoke… only.
- **F3 (done).** P01-P03 "Nolwazi Mbatha, Institutional admin, on 2026-10-19" (DATA: IssuedAsRole, UsedOn); P04 and P07 have no Invited by
  line; P07 is the issuer-gone board, Illustration. Note: the date is `UsedOn` (the registration day), under the label "Invited by ... on".
  See Build note 4.
- **F4 (done).** M15: Molefe, Programme start 2023-01-15 (`input#start`), Expected completion empty, `aria-invalid`, `aria-describedby="end-help
  end-err"`, help "Left empty, it would be 2024-01-15, which has passed. Enter the expected completion." (today 2026-10-19, so true), the
  refusal beside the field. The refusal repeats the hint in different words ("Enter the expected completion: left empty, it would be
  2024-01-15, which has passed."), and that second wording is not in the Spec's changed-words list (nit).
- **F5 (done).** G18 holds `<input type="hidden" name="returnTo" value="/account/register">` in the sign-out form, button "Sign out and
  continue"; the Spec's build rule says the endpoint accepts only that path and touches flow 02's sign-out.
- **F6 (done, checked both ways).** Every board the Spec's Illustration list names has the `anno--ill` chip on its face AND "Illustration"
  in its `canvas.json` title, at 1280 and 390 (L11, L12, L13, L14, L16, L17, N06b, N13, N14, N15, N16, N17, N18, N20, N21, N23, N24,
  N25, N26, G06, G10-G17, P04, P05, P07, P08, P09, T06, T07, T09, M04, M04b, M06, M07, M09, M11, M12, M13, M14, M15). No board outside the
  list carries the chip or the word. G10-G17, N13, N14 and T06 (the ask's minimum) are all in it.
- **F7 (done).** "works until 2026-11-02" in Register's note (G01-G06, G17), the OneTimeLink (N03, N06, N06b, N09, N21, L10), N17 and the
  mail section; the column is "Last day" on every list board (L03-L14); the word "Expires" is on no board (only Spec 2's old-words
  table). G10 reads "its link worked until 2026-11-02" (past tense, correct for an expired link). Expired's 30 days "on the South African
  calendar" (L14).
- **F8 (done).** P01-P05 and P07 are `section.detail-card.wait-card.dashboard-span-3` inside `.dashboard-grid`, title "Awaiting admission"
  with the hourglass path, no `detail-card--emphasis` anywhere (0 matches on all P boards). Build: `hourglass.svg` is not in
  `src/Wombat.Web/wwwroot/icons` (nor is `arrow-right.svg`, which P's links use); add both.
- **F9 (done).** "(optional)" and `.inv-optional` are on no board; N05, N07 (Speciality) and M01-M15 (Expected completion) carry the help
  that says what empty does ("Leave it at Any speciality for the whole institution." / "Leave this empty to derive it from the
  curriculum window.").
- **F10 (done).** Spec only: build rule F10, G14 unchanged ("Registration could not be completed. Nothing was saved.").

## M1-M11

- **M1 (partly).** L17 and T07 hold the page header, the T290 note and nothing else: no table, no `.btn`, no input in `main`
  (counted). N24 reads the user-accounts note ("... view or change user accounts, including your own.", `UserAdministrationRules.cs:83`,
  which `InvitationsList.razor:285` uses) and M12 the trainee-profiles one (`TraineeAdministrationRules.cs:20`), each at 1280 and 390.
  **Partly:** M12's `h1` and breadcrumb read "Admit Lerato Molefe", the caption says "before any read". A name needs a read of the
  profile or user, which is exactly what this caller is refused. See Must fix 2.
- **M2 (done).** P01-P03 read "Paediatrics / Paediatrics, on the Paediatric EPA Curriculum (11.1)"; "only while exactly one version" is on
  no board; P04 and P07 keep the Programme line, P05 drops it.
- **M3, M4, M5 (done, Spec only).** The build rules are on Spec 1 in the asked words. M4: G18's title says it is reached by the page and by
  a stale tab's submit. M5: N10, N11 and L07 show no link (N10/N11 have no `.otl`; L07 is a result with the "Invite another address" link
  only).
- **M6 (done as asked, with a defect found).** `.inv-when span{display:block}`, `.col-wrap` on Delivery, `.col-actions`, `width:1%` and
  `nowrap` gone, Invitee's `min-width` gone; `.col-actions--single` (NEW) for Current trainees' one button. The Current trainees table was
  restructured as the Spec says (Programme start over "Training year N"; curriculum over sub-speciality as `person-cell`; the
  Administrator's institution under the name). Measured table in "Measurements": 0 everywhere. Defect: Must fix 1.
- **M7 (done).** Every `@` in text on every G, L, N, M, P, T board is followed by `<wbr>` (scripted over all boards, attributes excluded:
  zero misses). `.action-result .alert,.validation-message,.alert{overflow-wrap:anywhere}` (`r3.css:55`). N21-390 and L13 carry the
  64-character address; N14-N20's notices and refusals have the `<wbr>`.
- **M8 (done).** Rendered; see Measurements. The Spec's ConfirmDialog text and "flows 08's and 10's scoped copies fold into it" are on
  Spec 2 (the copies exist: `app.css:5445`, `.msf-page dialog .form-actions`).
- **M9 (done).** `.header-container .actions-cell .btn` and `.inv-page .btn` below 641 px, general rules, in `r3.css`; every 390 board's
  `.btn` measured at 44 px or more.
- **M10 (done).** L16, M11, N23, T09 (and P09) are `div.action-result.canvas-focus[tabindex=-1] > div.alert.alert-danger.alert-row[role=alert]`
  with a `<strong>` first sentence naming what failed, "Nothing has changed. Try again, or come back in a few minutes.", and an outline
  `btn-sm` "Try again" with an icon; no empty-state or panel class on any of them, no inline style. L16, T09, M11 and P09 say "focus on
  it, after a failed retry too" or "focus on it" in their titles; N23's title omits the focus (caption nit).
- **M11 (done, all Invite and Admit states).** Counted per board at 1280 and 390: "* marks a field you must fill in." above the first field
  on N01-N26 (the forms) and M01-M03, M06, M07, M09, M13-M15; `required-mark` span `aria-hidden` on Email, Role (legend), and where a control,
  College (N02), Institution (N04), Speciality and Sub-speciality as the role needs them (starred N08, N08a, N10-N12, N14, N17-N20,
  N25 has none, N26 none); `aria-required="true"` on each control; the visually hidden " required" on Role's legend on all 26 Invite
  boards and on no other. Programme start starred on every Admit form, M14's Curriculum select starred and `aria-required`; Expected
  completion has no mark. G01-G06, G17 carry none (Q6). N05, N07, N13, N15, N16 have Speciality unstarred (Coordinator, Committee
  member), correct.

## S1-S24

- **S1, S2, S4, S8, S9, S12, S15, S16 (done, Spec only).** Each is on Spec 1 or 2 in the asked words (S12's mail: "as Institutional admin
  at Kgosi Kgari Teaching Hospital", the scope in the page's words, "it works until 2026-11-02", link to `/account/register/start?token=...`).
  S16: the boards already use `clinic-table--stack clinic-table--stack-wide` and the note row is a second `<tr>`.
- **S3 (done).** Every time on every board reads "SAST" (scripted: the only bare `HH:MM` is the L13 title "‹a time after 10:22› SAST");
  Registered and the expired dates are dates. Spec says so.
- **S5 (partly).** P05 has no Programme line and no Invited by line, and M14 draws Invited as "—" and the select. But P05's lead sentence
  still reads "waiting to be admitted to the Paediatrics programme at Kgosi Kgari Teaching Hospital", which names a speciality this
  registrar was never invited to (round 2's P05 had the same sentence next to a Programme line). And "Waiting's Invited as reads '—'" is
  drawn on no T board. See Build note 5.
- **S6 (done).** Waiting's caption on T01-T06: "A deactivated account is not listed."; M13: "**Not admitted.** Lerato Molefe's account is
  deactivated. Reactivate it under Users first." (bold first two words, `role="alert"`, "Users" the one link, Illustration).
- **S7 (done).** G13 title "a GET of /account/register with neither the cookie nor a token (S7)", G16 "a POST of the form after the cookie's
  30 minutes (S7)"; the two boards' words differ ("The invitation token is missing." vs "This page held the invitation for 30 minutes").
- **S10 (done).** `section.action-result.otl`: a success `alert` (role status, first sentence bold), `label[for=otl-link]`, `input#otl-link
  readonly` (value "…/account/register/start?token=•••"), "Copy link", `p#otl-said[aria-live=polite]`, help; N06 "Copied" + "Link copied.";
  N06b "Could not copy. Select the link and copy it yourself." NEW, Illustration; no inline ring.
- **S11 (done).** Results are `.action-result` on L07, L10, T02-T04, M09, M13; `.result-block` is on no board;
  `.inv-page .action-result .alert{margin:0}` in `r3.css:42`.
- **S13 (done).** N26: under Speciality "Nothing is adopted at Kgosi Kgari Teaching Hospital yet, so roles that need a speciality cannot
  be invited. Adopt a curriculum under Curriculum adoptions." with the last two words a link; Issue invitation stays.
- **S14 (done).** P08 (loading) and P09 (error, DashboardFrame's own words "Could not load your Home.") exist; N25 "Choose a role." under the
  legend, first radio `canvas-focus`, `fieldset[aria-describedby=role-err]`, `.choice-group.is-invalid`; N08a "Then choose its
  sub-speciality." with Sub-speciality not yet drawn (Steps 2.4 says the hint stays); M15 is F4's board. P08 draws the skeleton and
  `aria-busy` but not DashboardFrame's visually hidden status "Loading your Home." (nit; the code adds it).
- **S17 (done).** Each radio is `input` + `label` naming it alone, the description a `p.review-type-help.field-help` outside the label
  and in `aria-describedby`; `fieldset.form-group.choice-row`; no `.inv-radio*` class on any board.
- **S18 (done).** Institution (N01-N26) and Curriculum (M01-M13, M15) are `dl.form-group > dd.readonly-value`; Training year is `<output
  for="start">` under a `<label for="ty">`, its help a sibling `p`. Nit: the output's text is "Training year 1" under the label "Training
  year" (read twice); "1" alone would do.
- **S19 (done).** None of `inv-has-note`, `inv-note`, `consequence`, `inv-section`, `admit-facts`, `wait-facts`, `wait-steps`, `inv-role`,
  `inv-scope`, `field-label`, `inv-readback`, `inv-radio`, `result-block`, `inv-optional`, `reg-card-icon`, `ty-value` is on any board.
  Of the 141 classes the page boards use, every one is defined in `r3.css`, `bundle.css` or `tokens.css`; the ones only in `r3.css` are the
  canvas's (`anno*`, `canvas-focus`, `stage`, `note-board`) plus the NEW ones, and every NEW one has a Spec 2 row (`history-note--warning`,
  `delivery*`, `otl*`, `inv-page*`, `inv-count`, `wait-card`, `wait-links`, `ty-output`, `col-actions--single`, `skel-*`, `loading-line`,
  `inv-when`).
- **S20 (done).** `grep style=` over all 203 boards: **0**. G01-G06, G17: the submit is `div.form-actions > button.btn.btn-primary`. M08 is
  `class="system-panel"`.
- **S21 (done).** G06: no `maxlength` on any G board; `#last` holds "‹a last name of 101 characters›" with `aria-invalid` and
  `aria-describedby="last-err"`: "Last name: enter 100 characters or fewer." NEW.
- **S22 (done).** M09 "**Not admitted.** Lerato Molefe already has an active trainee profile. Open Trainees"; M04's warning is
  `alert alert-warning` with no `role`; the four loading lines (L15, N22, T08, M10) are `role="status"` "Loading the invitations." /
  "Loading what you can invite." / "Loading the trainees." / "Loading this registrar.".
- **S23 (done).** L13 is "read at ‹a time after 10:22› SAST": ndlvou@ (Check the address) and duplessis@ (Not delivered) at 09:22, two further
  Not delivered rows at ‹time›, three Being sent rows at ‹time within the hour› (one is the 64-character address), then mahlangu@,
  dlamini@, molefe@ (Sent, 09:22) and the nine 09:19 rows (Sent). 19 active = 4 + 3 + 3 + 9. No 09:19 or 09:22 row is Being sent. (Nit:
  the two further Not delivered rows carry "‹time›" and sit below the 09:22 rows, which is only "newest first" if they are no later; the
  placeholder cannot say.)
- **S24 (partly).** See the Steps board.

## Steps board (7 of 8 bullets done, 1 partly)

- 1.10 and 2.44: both say Home's "Invite a person" opens `/admin/invitations/new` (PROPOSED, Q1; X01). Done.
- 2.28-2.30: today's words quoted beside the new ("Pending admission", "Active profiles", "Admit to curriculum: ‹name›", "No active
  trainee profiles found.", "Trainee profile" / "Admit a pending trainee into a curriculum.", the landing on the profile (?id=) with
  Deactivate and Mark complete; 2.30 "No pending trainees found."). Done.
- The scope phrase's rule once, in 2.8. Done.
- "Invite the next person" is the reset form's heading, not a press; the journey recomputed as 40 + 9. Done.
- 2.9 names G05's focus (Password). Done. 2.26 has no "Resent" in the Issued clause. Done.
- 2.19's card names mahlangu@. **Partly:** the last line reads "Changed: 25 steps. Unchanged: 1.11, 2.31, 2.32 (and 2.19's menu ...)" as
  the ask said, but the table has 29 rows (counted), three of them Unchanged, so 26 changed; 25 only if 2.19 is counted unchanged, which
  its own row ("the card names mahlangu@") contradicts. Also 1.5 cites L02 for "Row: College admin over College of Paediatricians ...",
  and L02 has no rows (the two rows are on L03). Nits; fix the count and the board list.
- F7's "Last day" and "works until" in 1.5, 1.8, 2.2, 2.10, 2.26. Done.
- The counts on the boards agree with the Steps: 9 invitees; 5 waiting; Molefe's 2023-01-15 = training year 4, completion 2027-01-14;
  Ndlovu 2026-01-15 = year 1, derived 2027-01-15; Mahlangu's list entry 2030-01-14; T03 shows year 3, 2, 1, 4 for Dlamini 2024-01-15,
  du Plessis 2025-01-15, Mahlangu 2026-01-15, Molefe 2023-01-15 (all true on 2026-10-19).

## Nits (12 done, 1 partly)

- Done: training years "turn every 365 days from the programme start" on Admit's help and the Current trainees caption; M04b "This registrar
  belongs to no institution, so no curriculum can be offered." (no nav item lit, breadcrumb Home > Trainees > Admit ‹a registrar's name›
  with Trainees a link); the owner table (Spec 2) and the Administrator's crumb; the cookie's Secure follows the request's scheme (Spec 1);
  N14 `value="‹a malformed address›"`; N15 one link, "open Pieter Smit under Users"; inline links at 390 as the accepted exception (Spec
  2); "Cancel" on Invite a person and Admit; T04's "DATA: flow 13’s list, unchanged" and X01's "DATA: flow 12’s card" are `anno--data`
  pills; no "→" on any board (the one in Steps 2.2 is prose); no `form[aria-label]` on any board (forms use `aria-labelledby` an h2 that
  differs from the h1); `ds/wombat/tokens.css` and `components/bundle.css` are saved (`round-3/project/ds/`).
- Partly: T06. The ask wants "an Institution column, as Waiting has". The board has none: Waiting's header is also "Name and
  institution" with the institution under the name (round 2's Waiting had its own column; round 3 merged both). The Spec says why for
  Current trainees (a column ran 8 px over at 901), not that Waiting lost its column too. Acceptable; say it.

## Must fix before build (two lines on the Spec; no round needed)

1. **The Invitee row header must not be `.col-wrap`.** Every list board draws `th scope="row" class="col-wrap"` for the address. `.col-wrap`
   is `overflow-wrap:anywhere` with a 14% column, so the address breaks inside its domain wherever the column is narrow, and it is narrow
   at every desktop width: measured on L04 (nine rows), **9 mid-word breaks at 1280, 1100, 1000 and 901** ("sithole@ / kgk.wo / mbat.loc / al",
   "kgk.wombat.l / ocal" at 1280 in the screenshot). With `.col-wrap` removed from the row header only: 0 mid-word breaks on L04 at all four
   widths, and 0 px sideways at all of them (also 0 at L13's 1280). The ask asked for `.col-wrap` on Delivery only; the row header's
   `<wbr>` after the @ already gives the address its break. The build must not copy the board's class here. (Rendered, observed.)
2. **M12 names a registrar before any read.** The h1 and breadcrumb of the Trainee-holding admin's Admit board read "Admit Lerato Molefe",
   while the board is "before any read" and the caller is refused every trainee-profile read. The build cannot know the name without the
   read it must not make, and writing it would leak a registrar's name to someone who may not see trainee profiles. The note board should
   read "Admit a registrar" (h1, breadcrumb and title), as N24 reads "Invite a person". Say so on the Spec, and in Steps if M12 appears.

## Carried to the build (the build can do it right)

1. **Narrow-desktop word splits.** Even with Invitee fixed, `.col-wrap` Delivery and the stacked-wide tables split words between 901 and
   1100: L13 at 1000/901 "Not delive / red" and "Check the addre / ss"; T03 at 1000/901 the Curriculum column "Curricul / um",
   "Paediatri / c EPA", and dates "2026- / 10-19" on L04 at 901. No sideways scroll, so M6 passes, but the system's own comment (`.col-wrap`
   "No word breaks at 1280px, but with little to spare") says this is the floor. Raise StackWide's threshold from 900 toward 1100, or give
   Delivery and Curriculum a floor; confirm at replay at 901, 1000 and 1100.
2. **Dates.** "2026-10-19" and "Training year 4" should not split on the hyphen; `white-space:nowrap` on the date line only (a short token,
   cannot widen a column past ~6.5rem).
3. **M04's words** (F1): name the sub-speciality ("...no curriculum adopted for Paediatrics / Paediatrics...") and keep the institution-wide
   sentence for the case where none is adopted at all.
4. **"Invited by ‹name›, Institutional admin, on ‹date›"** uses `UsedOn`, the registration day, under the label "Invited". Use the
   invitation's issue day, or drop "on ‹date›" (P03's resent link makes them differ in principle).
5. **P05's lead sentence** must not name a speciality for a registrar with no invited scope ("waiting to be admitted at Kgosi Kgari Teaching
   Hospital"); and Waiting's Invited as "—" (S5) needs a T row at replay.
6. **Icons.** Add `hourglass.svg` and `arrow-right.svg` to `src/Wombat.Web/wwwroot/icons` (neither exists; paths are in the board markup).
   `clock`, `circle-alert`, `refresh-cw`, `copy`, `user-plus` and `info` exist.
7. **M15's refusal** is a second wording of the hint (not on the Spec's changed-words list); pick one sentence for both.
8. **Counter/loading/clipboard.** P08 draws no "Loading your Home." status (the code does); `wombat.js` has no clipboard call yet (Spec lists it).
9. Nits: Steps' count (26) and 1.5's board list; Training year's `output` reads "Training year 1" under the label "Training year";
   N23's title omits the focus; the Spec's F1 row "P04 now ‹a registrar's name›" should read address; L13's two ‹time› Not delivered rows
   are unsortable placeholders.

## New in round 3: names, addresses, dates, times, counts

Against round 2 (scripted, per board) and the code: nothing invented. The new boards use the cast only (N06b Smit, N15 Smit, N24, N25,
N26 Kgosi Kgari; M04b, M14 use ‹…› for the registrar; M12, M13, M15 Molefe; P07-P09 no data; T06 now 1 waiting + 4 current, taken from
T03's four rows, which is why its count changed from round 2's 5 waiting). The ask's wording for NEW sentences is word for word on
N06b, N13, N14 ("Enter a valid email address, for example name@example.org." matches `AddMsfInvitation.Malformed`), N25, N26, M13, M04b,
G06. Strings the ask said are "as built" match the code: `InvitationText.Issued`, `.Resent`, `.ResendRetiresTheLink`, `.CheckAddress`;
`InvitationRefusedException` (invalid, revoked, account exists, address not accepted); `RegisterOutcome` (token missing, user not loaded);
`DashboardFrame`'s "Could not load your Home."; both T290 notes. G14's "Registration could not be completed." is `RegisterOutcome.GeneralRefusal`
without its "Try again." tail (the button says it).

## For the build

- Spec 1 and 2's tables and build rules (M3-M5, S1, S2, S7-S9, F5, F10), "DATA the build must add", and `r3.css` minus the canvas-only
  rules (`.anno*`, `.canvas-focus`, `.stage`, `.note-board`, `.doc`).
- Replay at 901, 1000, 1100 and 390 for the lists and Current trainees (Must fix 1 and Carried 1 are about layout the zero-scroll
  measurement cannot see), the dialog's stacked order at 390, and L17/T07/N24/M12 as a Trainee-holding admin (no read before the note).

## Taken into the build (the main session, 2026-10-20)

Round 3 is final; no fourth round. Both must-fixes were confirmed on the boards (`R3-L04-nine-1280`: nine
`th scope="row" class="col-wrap"`; `R3-M12-t290-1280`: `<h1>Admit Lerato Molefe</h1>`). The build does, in place of
the boards:
1. The Invitee row header without `.col-wrap` (the address breaks only after its `<wbr>`); `.col-wrap` on Delivery only.
2. The T290 note on Admit reads "Admit a registrar" in its h1, tab title and trail: no name before the refused read.
3. M04 says "No curriculum is adopted at ‹institution› for ‹sub-speciality›, so ‹name› cannot be admitted." when other
   curricula are adopted (F1); the institution-wide sentence only when none is.
4. P05 names no discipline; Waiting's Invited as reads "—" for a registrar with no invited scope (S5).
5. The Steps count is 26 changed; 1.5's rows are L03's.
6. T06's Waiting keeps its Institution column (round 2's nit); Current trainees keeps the institution under the name.
7. The carried notes above (`hourglass.svg`, `arrow-right.svg` added to `wwwroot/icons`; "Invited by … on" the
   invitation's issue day, not `UsedOn`; M15's refusal worded once).
