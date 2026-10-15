# Flow 10 round 3: the check

Round 3 (canvas "Wombat 10" version 1792071007-7a95, saved in `round-3/project/`: 188 boards `R3-*.dc.html`, `R3-msf.css`,
`canvas.json`) was checked item by item against `round-3-ask.txt` by a Sonnet agent for the main session, 2026-10-15,
from the boards' text and markup (the HTML with its style and script blocks stripped, class names and attributes read)
and from `R3-msf.css`, with the built code (`MsfCampaignText.cs`, `MsfDtos.cs`, `CampaignReport.razor`) opened where the
ask says "as built". All 188 boards are on the canvas page "Round 3" (`page: r3`); every 1280, 768 and 390 board carries
the matching `$preview` width. **Not checked: rendered layout.** The boards link `ds/wombat/tokens.css` and
`ds/wombat/components/bundle.css`, which are not in the saved folder, so nothing could be drawn here; "laid out for 390"
below means the 390 boards' markup and the `@media (max-width:640.98px)` rules in `R3-msf.css`, not a screenshot. The
`R3-0-spec` board's Round 3 table was read in full; round 2's boards were not diffed.

## Verdict

Tally: **44 done, 8 partly, 1 not done.** No anonymity leak and nothing that needs another round.

- **F1-F11: 10 done, 1 partly (F10).**
  - F1: R1 (1280, 768, 390), R2 and R16 hold the status card and the checklist only. "The responses are read once the
    campaign has closed and every address has been removed." sits under the Open badge; there is no `group-card`, no
    "By respondent group" and no "Suppressed" on any Open report. R3 (Under review) is the first board with cards.
    3.41's Expect: "no group card, no suppressed line; Release not offered."
  - F2: the form says "At least 3 for multi-source feedback."; C8 refuses with "Enter a number from 3 to 100." (summary
    link to `#c-mincat`). K2 draws Peer doctor 2, Consultant 3, Nurse 3, Allied health professional 1, All groups 9, with
    "‹the third consultant's address›" and "‹the third nurse's address›" in the list. R3 and M3 show the Consultant and
    Nurse cards "from 3 responses", with "‹the third consultant's comment›" and "‹the third nurse's comment›". Responded
    is 1 / 3 / 3 / 1 = 8 of 9 (K7, K8, K9, K22). No board draws a Distribution or a per-point count.
  - F3: M2 at 1280, 768 and 390 reads "Report", "Released", "Actions": Default MSF / Multi-source feedback / 2026-10-14
    16:34 SAST / View. No Responses column.
  - F4: R6d and R10 read "No email is sent: she can no longer open MSF reports."; no PROPOSED chip on any board.
  - F5: R4 reads "Closed by itself on ‹date›, once 2026-10-28 had passed. DATA: ClosedByUserId null"; its note says "the
    hourly job closed it"; "nightly" is on no board. The Spec adds `ClosedByUserId` to both queries.
  - F6: C1-C8 carry 11 `required-mark` spans (Template, the fold's Template name, Scale question and Comment question,
    Trainee, Opens on, Closes on, the three minimums, the EPA legend), 10 `aria-required="true"` and the
    "* marks a field you must fill in." line, at 1280 and 390. K1, K2, K5-K5c, K13 and K20 carry one (Respondent email) and
    the line; K15 carries two (email, Teaching context). The respondent group select has none on any board.
  - F7: every K and R board's `status-card` has `detail-card--emphasis` (Draft, Under review), `activity-status--others`
    (Open), `activity-status--done` (Released, K11, R7, R7b) or `activity-status--declined` (Withdrawn, K12).
  - F8: Close campaign is `btn-danger` in R2's dialog, Release `btn-primary` in R6a-R6d's; the Spec's F8 row says why.
    Unchanged from round 2, as asked ("keep as drawn").
  - F9: "Open its campaign page" is its own `<p class="page-subtitle"><a class="review-progress-link">` under the subtitle
    on every R board; `R3-msf.css` gives it `min-height:2.75rem` below 641 px.
  - F10 (partly): R8a and R9 are "MSF report for Anele Dlamini", one Consultant invitee, 1 response, PAED-012 (R8a) or
    no EPA (R9), no Illustration. But the thresholds are drawn 5 / 3 / 2, not ‹n› as the ask said; the strip and the
    Spec say it was read off the old-frame captures and ask "say if ‹n› was meant". A question, not a defect.
  - F11: K2 reads "Invitee added." alone.
- **M1-M10: all done.**
  - M1: Pieter du Plessis's row above Lerato Molefe's on L2-L6 at 1280, 768 and 390, and in L7's semester-2 rows.
  - M2: Consultant card above Nurse on R3-R11, R17, M3, M4, M8; "Suppressed: Peer doctor, Allied health professional."; the
    select reads Peer doctor, Consultant, Nurse, Allied health professional, Other with Peer doctor chosen on K1, K2, K3,
    K4, K5b, K5c, K13 and K20 (Nurse on K5, the duplicate's re-post).
  - M3: K4 reads "Opening emails each of the 8 invitees …". The ask's "6" is superseded by F2: nine, less Palesa Tau's
    first address; K2 and K3 are recounted (K3: Nurse 2, Allied 2, All 9). The Spec says so.
  - M4: K15 as asked (Thresholds "‹n› responses; ‹n› in a respondent group; 1 respondent group reporting", the fixed-at-1
    note, PAED-015 with its whole title, "‹n› invitees", "‹closes›"). The disabled minimum "with its help" on a create
    form is Spec only; the ask did not require a learner create board.
  - M5: R10 and R6d: "As Lerato Molefe would have read it." and "She can no longer open MSF reports; the report is kept
    with the evidence on her record."
  - M6: the Spec's DTO table lists `EvidenceRecordedOn`, each EPA's `Recorded`, the list's `ReleasedOn` and
    `TeachingContextCount` under "Exists today" (all four are in `MsfDtos.cs`), and adds the per-EPA "will be recorded"
    flag, read by the release section's records line and by R6a-R6b ("DATA: will be recorded").
  - M7: labels end in `<span class="required-mark" aria-hidden="true"> *</span>`, controls carry `aria-required`, the
    visually hidden "required" is on the EPA legend only.
  - M8: no inline colour on any board (the only inline style in 188 boards is Skeleton's `width:100%;height:3rem`); K11 and
    R7 both read "The report has been released to the trainee." under one `h2.status-line`.
  - M9: C4's summary is `div.validation-summary-errors.error-summary` with `role="alert" tabindex="-1"`, a bold "Not
    created." then "Fix the field below and create the campaign again.", and a link to `#e1`; the fieldset's own message
    stays under the legend; the Spec says PageFocus and the summary taking the focus.
  - M10: the dialog rules are `.msf-page dialog .form-actions{flex-direction:column-reverse}` and
    `.msf-page dialog .btn{min-height:2.75rem;width:100%}` in the 390 media block, not `.dlg-overlay`; the DOM order is
    the safe button then the action; the Spec says each ConfirmDialog sits inside the page's `.msf-page` column and lists the
    stacked order as a rule it changes. All 16 dialog boards are inside `.msf-page`.
- **S1-S17: 13 done, 4 partly (S1, S2, S10, S17).**
  - S1 (partly): R2 sits over 3.44's page and, by F1, shows the checklist with no cards, not the cards S1 listed; it
    reads 8 responses, not 6, by F2. R6b, R6c and R6d carry Illustration. R6c's backdrop is Lerato Molefe's page with no
    EPA named, not R9's (R9 is now Anele Dlamini's and blocked, so no dialog opens there); the strip explains it. Sound.
  - S2 (partly): as F10, thresholds drawn 5 / 3 / 2.
  - S3: NEW chips on the hour line (K7, K8, K22), "Go to MSF campaigns" (K16, R12), "Today, on the South African
    calendar. NEW DATA"; C1-C8 trainee help is "Current trainees you can run a campaign for."; K2 as F11.
  - S4: K5c "Enter a valid email address, for example name@example.org. NEW"; C8 "Enter a number from 3 to 100."; the
    Spec says `type="email"` with `novalidate`.
  - S5: Spec only: the fold is outside the campaign's EditForm, Add template is `type="button"`, the Template select
    uses `form="campaign-form"` (the markup agrees: no `<form>` around the fold).
  - S6: the Spec names `MyMsfReport` for `/msf/my-reports/{id}` with its h1, tab and trail, and keeps the not-found alert
    (M5) apart from the load error (M10).
  - S7: Spec only, as asked.
  - S8: K19, R15 and M8 draw fifteen EPAs at 390 with PAED-015's full title; M9, M10, M11 are her report loading, load
    error and unavailable; K20, K21, K22, R16 and R17 are the refused Open, Withdraw, Resend, Close and Release, each with
    an `alert-danger` and the `ring` on the button pressed; Close's result is R3's; Q9 is the refused post (tab "Error:
    Feedback on Lerato Molefe · Wombat", `#msf-error` `tabindex="-1"`, `aria-invalid` on the comment); 768 exists for C3,
    K7, V1 and R6a (and for K2, L2, L5, M2, M3, R1, R3, R7, R8a).
  - S9: Q3-Q8 keep the state's Alert under the h1 (success, info, warning x3, danger); Q9 danger.
  - S10 (partly): `div.actions-cell` and `.submit-check` are used; no `.row-actions`, `.check-line` or
    `.epa-fieldset` anywhere. Every class the ask lists has a Spec row. Not every class a board uses does: see
    "Fix in the build" 4.
  - S11: the DESIGN.md table has all seven asked rows plus the title-rule and anonymous-page rows.
  - S12: the EPA card opens with `h2 "Evidence"`, the legend inside; invalid state is `fieldset.choice-group.is-invalid`.
  - S13: V1 at 768 exists; the Spec names the scroll regions as R9's one exception and says why count tables stay
    unstacked.
  - S14: `@media (max-width:900px)` stacks `.add-invitee`; `.add-invitee > .btn{align-self:end}`; K2-768 exists.
  - S15: "withdraw it on its campaign page" is `a.msf-inline-link` (24 px, 44 px at 390) in R3-R7, R8a, R9, R17 and in the
    disabled Release's reason; not on R8b (see below).
  - S16: no inline style but Skeleton's; `.msf-page--measure`, `.section-title--h3`, `.msf-stack-card` are classes.
  - S17 (partly): K12's sentence is `CampaignEdit.StateNote`'s word for word; R12's is the campaign's and the Spec says
    the source names only the title (`CampaignReport.razor:30` confirms); R11's 4,000-character comment is drawn at
    length. **K10 does not match `MsfCampaignText.WithdrawBody`** (see "Fix in the build" 1).
- **Nits: 9 done, 3 partly, 1 not done** (13 lines).
  - Done: R11's count (17 responses = 12 + 3 + 1 + 1); Q1-Q9's mark at `width="44"`; PAED-012 as the dropped EPA on R6b,
    R7b and M4; K13's strip ("A draft campaign for a registrar whose programme has since ended (‹date›)."); C1 and C2's
    `aria-describedby` targets all exist (checked every `aria-describedby`, `-labelledby` and `-controls` in all 188
    boards: none dangles); L2-L7's "Campaigns (n)" is a plain count; C3-C5's "Mr Smit typed 5; 3 and 2 are the form's
    defaults (8 / 3 / 2)"; R7, R7b and K11 share one status heading; the 390 boards keep the closed menu's list and the
    account row (the nine Q boards are anonymous pages and have none).
  - Partly: R8a keeps the link, R8b lost it; the counter is static text in every board ("106 of 4,000 characters"; no
    input handler in any file, as in flow 09's boards), so "counts in the browser" is the Spec's word only, while
    "Up to 4,000 characters" is on Q1, Q2 and Q9; the "no `.shadow` in a card" and "K13's natively disabled controls" notes
    are on the Spec, the fold summary's text colour is drawn (`color:var(--text-color)`) but not said.
  - Not done: the canvas's `ds/` (tokens.css, bundle.css) is not in `round-3/project/`, though the Spec's last note
    says it is saved with the round. Flow 09's and round 2's folders do not hold it either.
- **Steps board: done.** All 18 steps are present (3.34-3.49, A.4.7, A.7.5), each "as today, and" the new clause. The
  counts agree with the boards: 9 invitees; groups 2 / 3 / 3 / 1; 8 of 9 responded, 1 / 3 / 3 / 1; "3 responses, of the
  5 needed" and "0 respondent groups reporting, of the 2 needed" on R1; "1 unused link, 8 responses kept" on R2; 106
  characters (the narrative counts 106); the status line on R7; list row Released, 9, 8 on L5. 3.36 says a default counts
  no field and lists the select in M2's order; 3.38 and A.7.5 put Pieter first; 3.44 has the Consultant card first and
  the level options; the journeys table starts at Home's Start an MSF campaign, counts "5 + 2" for Create, and gives
  12 presses + 18 fields for the nine adds, the remove and Palesa's re-add (9 + 1 + 1 + 1 presses; 9 emails, 7 groups,
  then 1 email, 1 group), which adds up.
- **Spec board: done** for F1-F11, M1-M10, S1-S17 (what changed, where), the class table, the DESIGN.md list and "DTO
  additions, by query" (the "will be recorded" flag, `ClosedByUserId`, the South African default, `HasOpened`, the setup's
  and the report's additions). One gap in the DTO table: see "Fix in the build" 3.
- **Anonymity: no leak.** A respondent's address appears only on the draft pages (K2-K5c, K3, K4, K20), the campaign's
  own pre-open state, as built. No board shows a respondent's name or address to the trainee (M2-M5, M8-M11 hold no `@`,
  no cast name, no Suppressed line, no group named except the two shown). Suppressed groups appear on the coordinator's
  boards by name only ("Suppressed: Peer doctor, Allied health professional. Fewer than 3 responses each"), never with a
  rating or a comment; the trainee-facing text says they are not shown "not even that they answered". On R8a and R9 the
  one Consultant response is not shown. R11 shows the Nurse group's 12 comments only because 12 is at or above 3.

## Still wrong

### Fix in the build (small; no round needed)

1. **K10's Withdraw dialog body is not the built one.** The board reads "… A withdrawn campaign is never released to the
   trainee, and withdrawing cannot be undone." (the draft's tail). `MsfCampaignText.WithdrawBody` for a closed or
   under-review campaign reads "A withdrawn campaign is never released to the trainee and is never evidence on their
   record, and withdrawing cannot be undone." Build from the source; S17 asked for exactly this check.
2. **R8b's disabled Release reason has no "withdraw it on its campaign page" link** (R3-R8b-blocked-groups-1280 and -390
   end at "…is not multi-source feedback."). R8a, R9, R17 and the rest have it; the nit named R8b. Add the link to the
   reason, as `CampaignReport.razor:105` already does.
3. **`MsfScaleAggregateDto.Distribution` must go.** F2 says the per-point counts are dropped from the data, and the Spec's
   F2 row says it, but the DTO table lists nothing removed; the DTO still carries `Distribution`
   (`MsfDtos.cs`, `MsfScaleAggregateDto`). The boards draw no distribution, so the build should drop the field and its
   handler code.
4. **Class rows missing from the Spec** (S10 asked for every class a board uses): `is-met`, `is-short`, `is-rule`,
   `narrative`, `scale-choices`, `sep`, `alert-row` and `dialog-body` are in `R3-msf.css` or the boards with no Spec row.
   Some may be design-system classes; the Spec should say "reuses .x" or NEW for each.
5. **The `ds/` folder** the boards link is not saved with the round (nit). Save it, or drop the Spec's last note.
6. **The counter** on the report's narrative and on `/msf/respond` needs the browser handler (flow 09's); the boards are
   static.
7. **F10's thresholds.** The Spec asks whether ‹n› was meant. If so, change R8a and R9 to ‹n› (two boards, no layout
   change); otherwise leave 5 / 3 / 2.
8. **Low, needs-confirmation:** K13 draws a Default MSF (multi-source) campaign for PAED-015. D35 says learner feedback is
   written only on EPAs whose list names it (PAED-015); whether PAED-015's tool list also names multi-source feedback was
   not checked. If it does not, change K13's EPA to one that does.

### Needs another round

None. The wrong-behaviour and missing-state tests found nothing: every state the ask names is drawn, the anonymity rules
hold on every board read, and the deviations (M3's 8, S1's R6c backdrop, F10's 5 / 3 / 2) are stated on the Spec and
follow from F1, F2 or the F10 data.

## For the build

- The Spec's "DTO additions, by query" table, plus round 2's (`round-2-review.md`), plus item 3 above.
- The proposed app.css section is `R3-msf.css`: the canvas-only rules (`.anno`, `.board-strip`, `.doc-board`, `.ring`,
  `.browser-prompt`, `.dlg-overlay`) are not shipped.
- Replay checks the layout this check could not: 390 widths for the campaign page, the report and the add form, and the
  dialogs' stacked order.
