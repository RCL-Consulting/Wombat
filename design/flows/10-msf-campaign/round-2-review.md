# Flow 10 round 2: the review

Round 2 (the boards saved in `round-2/project/`, not yet committed: 159 boards, `R2-0-spec`, `R2-0-steps` and
`R2-msf.css`) drew P, RA and MA at fidelity: the campaign list (L), New MSF campaign (C), the campaign page (K), the
report (R), `/msf/respond` (Q), MSF reports and her report (M) and MSF coverage (V), at 1280 and 390, with 768 where the
ask marked it. Reviewed 2026-10-15 from three sides, each a Sonnet agent reading the boards' text:
**the code** (`round-2-review-code.md`: `src/` and the ask), **the cast** (`round-2-review-cast.md`: the runbook, the
seed and `states.md`) and **the design system** (`round-2-review-ds.md`: `app.css`, DESIGN.md, BRIEF.md, accessibility).
The three source files are kept as they are; this file merges them. Every must-fix below was checked against the board
and the code or the runbook before it was carried; what did not hold is said where it was dropped or downgraded.
Severity: M must-fix, S should-fix, N nit. Code is cited at `58126bdd`.

**Round 2 went without screenshots.** The Spec board says so: "The screenshots did not arrive. Nothing was attached to
round 2's message in this session, and reading them from C:\dev\Wombat\design\upload was refused … Nothing on round 2's
boards is drawn from memory of them." What it cost (inferred from the findings, not measured): the words held, since
every one came from the ask's FIXTURES (the cast checked the names, addresses, counts, ratings, comments, the 106-character
narrative, 16:34 SAST and all fifteen EPA titles, and found them true). What slipped is what only a capture shows: **orders**
(the list's rows, M1; the respondent groups on the report and in the add form's select, M2) and **values the ask left
open**, filled by invention on Illustration boards (K15, M4). `3.46-2-list-released.png`, `3.44-1-report-under-review.png`
and `3.36-2-seven-invitees.png` would each have shown the built order. Round 3's message should attach them again, and
the Spec should say if they did not arrive.

## Decisions for the operator

| | Question | Recommendation | Alternatives |
|---|---|---|---|
| F1 | **Should an Open campaign's report show a group's average and comments as they arrive?** Today it does (`CampaignReport.razor:144–172` renders every category in every state) and R1 draws it as "So far". While Open the campaign page also shows Responded per group (Q5), and the coordinator listed every invitee while it was a draft, so a reload after one more response tells them which comment and which rating that response brought, and from the per-group count, which group it came from. CUSTOMIZATION.md § "What stays in code" (:583) asks for "aggregated views that don't allow de-anonymisation by counting"; the code itself declines a per-row "responded" mark because it "would name the author of the comment that has just appeared on the report" (`CampaignEdit.razor:200–203`). | **No.** While Open, the report shows the status card and the threshold checklist only (counts, as the campaign page has them); the group cards and the suppressed line appear once the campaign is closed and its addresses removed ("closed and anonymised for review"). Nothing is lost: Release is offered only after the close. Cost: a build change to `CampaignReport`, R1 redrawn, 3.41's Expect rewritten (no Nurse card while Open). | Keep live results (today's); show them while Open only once every group in the campaign has passed its threshold. |
| F2 | **Should a respondent group report at 2 responses (or 1)?** At 2, a group's average gives both ratings away: 4.50 on the 1-to-5 scale is a 4 and a 5, 5.00 two 5s, and each of the two respondents learns the other's exactly. The form allows a group minimum of 1 (`[Range(1, 100)]`, `CampaignEdit.razor:1209`), where one person's rating and comment are shown alone. The suppression threshold is the only anonymity guard on the report (D11 builds the release gate on it), and D9's per-period campaigns (about 16 questionnaires a year) make every group small. The code's own default is 3 (`MsfCampaign.cs:13`, the form's 8 / 3 / 2); Mr Smit typed 2 at 3.35. | **A floor of 3 for multi-source feedback**: create refuses a group minimum below 3 (learner feedback's one group starts at 3 too), and `MsfScaleAggregateDto.Distribution` (`MsfDtos.cs:41`) is dropped from the DTO, since Q9 shows it to nobody. Cost: the replay's groups of two report nothing, so the cast gains a third Nurse and a third Consultant (and their ratings and comments), and every board's figures are recomputed. | Accept 2 (the boards as drawn), the ratings of a pair known; keep 2 but show a group's average only from 3 responses (its comments from 2). |
| F3 | **Her MSF reports list's Responses column**: it shows the campaign's total, suppressed groups included (`ListMsfCampaignsForTrainee.cs:47`, `Responses.Count`), so "6" beside a report showing two groups of two tells her that two others answered, against the Release dialog's "not even that they answered" (T249). | **Drop the column** from her list; the count stays on the coordinator's side. | Show the count of the shown groups' responses only; keep the total. |
| F4 | **The release mail when her programme has ended** (PROPOSED in round 2's ask): an ended registrar has lost the Trainee role (D54), so a link to MSF reports lands on a page she cannot open (`MyMsfReports.razor:2`, T311). | **Send none**, as R6d draws it: "No email is sent: she can no longer open MSF reports." The release section and the dialog say the report "is kept with the evidence on her record" (M5). Revisit with T311. | A notice that says a report was released and links to nothing; a mail as for a current registrar. |
| F5 | **"Closed by itself on ‹date›"** (PROPOSED, R5f): read from `ClosedOn`'s South African date falling after `ClosesOn`. The job runs hourly (`"0 * * * *"`, not nightly) on the UTC date, so a hand close at 00:30 SAST on the 29th (22:30 UTC on the 28th) reads as "by itself". | **Store how it closed**: a nullable `ClosedByUserId` (null when the job closed it), one small migration; the board's sentence then reads from it and "nightly" becomes "hourly". | Keep the date reading with the edge noted on the Spec; drop "by itself" and say only "Closed to responses on ‹date›". |
| F6 | **Which fields carry the required mark** (PROPOSED, R12): the boards mark Template, Trainee, Opens on, Closes on and the EPAs, not the three minimums (`Required="true"` today, `CampaignEdit.razor:115–121`), the invitee's email (refused blank, K5b), the fold's Template name, Scale question and Comment question, or K15's Teaching context. | **Every required control, at every width,** with DESIGN.md's "* marks a field you must fill in." line above the create form's first card (and the campaign page's add form); a select that always holds a value (the respondent group) carries none. | Mark none and rely on the refusals; mark only the create form's. |
| F7 | **The status card's stripe.** R4a asked for `.detail-card--emphasis` in every state; DESIGN.md (:3364) gives that blue the meaning "the viewer's move", and R7 alone overrides it inline with green while K11, the same Released state, stays blue. | **The activity page's rule**: blue where the coordinator has the move (Draft; Under review, to release), the control edge while Open (the respondents' move), green when Released, red when Withdrawn; as classes, listed on the Spec. | Blue in every state (R4a as asked), R7's override dropped. |
| F8 | **Close campaign's confirm is danger, Release's is primary**, though both "cannot be undone". | **Keep**: Close destroys (it ends every unused link and removes every address); Release completes the flow and destroys nothing. The Spec says which reading it takes. | Both danger; both primary. |
| F9 | **Where "Open its campaign page" sits on the report**: today the tail of the dot-separated subtitle, an unpadded 19 px link. | **Alone on its own line under the subtitle** (`PageHeader`'s `SubtitleContent`, flow 07's `.review-progress-link`: 24 px, 44 px at 390). | An action in the status card's foot. |
| F10 | **R8a (blocked by responses) and R9 (names no EPA)**: drawn as Illustrations for Dr Molefe, though `states.md:627–628` reaches both on a scratch campaign for Dr Anele Dlamini (PAED-012, one invitee, answered and closed). | **Redraw for Dr Dlamini** as `states.md` reaches them ("MSF report for Anele Dlamini", PAED-012, 1 response, thresholds ‹n›), and drop the Illustration label. | Keep them as labelled illustrations for Dr Molefe, R8a's "1 response" bracketed. |
| F11 | **The add result**: K2 reads "Invitee added. palesa.tau@kgk.wombat.local, Nurse."; the ask kept "Invitee added." and 3.36's Expect checks it after each of eight adds. | **"Invitee added." alone**, as built: the cleared address takes the focus and the list below shows the new row. | The address and group added, marked NEW, and 3.36's Expect rewritten. |

Settled from the evidence (no decision needed):

- **The list's order is `CreatedOn` descending** (`ListMsfCampaignsForCoordinator.cs:40`): Pieter du Plessis's campaign
  (#2) above Lerato Molefe's (#1), as 3.38's and A.7.5's Actuals record (`act-3-operations.md:1012`,
  `appendix-cross-cutting.md:1267`). M1.
- **The respondent groups' order is the enum's** (`MsfRespondentCategory`: Peer doctor, Consultant, Nurse, Allied health
  professional, Other; `MsfAggregationService.cs:44` and `GetMsfCampaignSetup.cs:230` `OrderBy(group => group.Key)`), as
  3.36's, 3.44's and 3.47's Actuals read. The ask's "Nurse 4.50, Consultant 4.00" was a fixture list, not an order. M2.
- **Coverage's scroll region stays** (the design review's question 2): round 2's ask decided it (R9, "T323's one
  exception"). The Spec lists it as the deliberate exception and V1 is drawn at 768 (S13).
- **Palesa Tau's group on K2–K4 holds** (the design review's M1, in part): she was added second as an Allied health
  professional (K3, before the remove: Nurse 1, Allied 2), removed (K4: six), then re-added last as a Nurse (K2: seven).
  `act-3-operations.md:957–975`. Only K4's "7" is wrong (M3).
- **Learner feedback is evidence for PAED-015 only**: the seed's only tool list naming `learner_feedback` is EPA 15's
  (`paediatric-epa-v11.1.json`, D35), and create narrows by it (`CreateMsfCampaign.cs:87–88`). M4.
- **What holds** (all three reviews): every name, address, group, count, rating, comment, the narrative (106 characters),
  16:34 SAST = 14:34 UTC, the coverage figures, the five `/msf/respond` refusals, the withdraw, remove and duplicate
  wordings, the badges (`BadgeFor.MsfCampaign`), the menus and trails, the dialogs' structure, the loading and load-error
  states, colours (no raw colour, no new token; the contrast table recomputes), no duplicate id, no `tabindex` on an h1.

**Data the build must add** (restated from the DTOs as they stand; M6):

| Board shows | Gap | Size |
|---|---|---|
| Responses "—" on a campaign that never opened | `HasOpened` on `MsfCampaignSummaryDto` (the entity's `OpenedOn`) | S |
| About this campaign: thresholds, EPAs, training year, Closed to responses; the not-current and ended warnings | `GetMsfCampaignSetup`: MinimumResponses, MinimumCategoryResponses, MinimumRespondentCategories, CoveredEpas, TrainingYear, ClosedOn, SubjectIsCurrent, SubjectEndedOn | S |
| The report's h1, subtitle, Close dialog's unused links, Closed and Released lines, the warnings | `GetCampaignAggregateReport`: SubjectName, OpensOn, ClosesOn, InvitedTotal, ClosedOn, ReleasedOn, SubjectIsCurrent, SubjectEndedOn. `EvidenceRecordedOn` and each EPA's `Recorded` **exist** (`MsfDtos.cs:123`, `:63`) | S |
| Before release: which named EPAs will be recorded (R6b's "not recorded, no longer on her curriculum"; the release section's "2 evidence records … for PAED-010 and PAED-012") | **New:** a per-EPA "will be recorded" flag from `MsfCampaignRules.CoverableEpaIdsAsync` (`MsfCampaignRules.cs:39`), the predicate the release drops by (`ReleaseMsfCampaign.cs:211–222`). `Recorded` is false for every EPA until release and cannot serve | M |
| "Closed by itself" | F5: `ClosedByUserId`, or nothing | S |
| Released time on her list | Nothing: `MsfCampaignSummaryDto.ReleasedOn` is already a `DateTime?` (`:28`); shown in SAST | none |
| Learner feedback's "Teaching contexts that responded" | Nothing: `TeachingContextCount` is on the report DTO (`:126`), which her page reads | none |
| Opens on defaulting to today on the South African calendar | `ProgrammeCalendar.DateOf` from the `TimeProvider` in place of `DateTime.Today` (`CampaignEdit.razor:1194, 1197`) | S |
| The release mail (decided, T367) | `AccountNotices`' one-off notice | S |

## Must fix

| # | Side | Finding | Fix |
|---|---|---|---|
| M1 | cast | The campaign list draws Lerato Molefe above Pieter du Plessis under "newest first" (L2–L6 and their 390s; L7's semester-2 rows). The list sorts by `CreatedOn` descending, and du Plessis's campaign was created after hers; 3.38's and A.7.5's Actuals read du Plessis first | Du Plessis's row first wherever both are drawn; L7 likewise |
| M2 | cast (upgraded from S) | The report's groups are drawn Nurse then Consultant (R3–R11, M3, M4) and the add form's select Nurse first with Nurse chosen (K1–K5b, K13). The built order is the enum's, and the select's default is Peer doctor (`CampaignEdit.razor:1229`). Upgraded: the Steps board's 3.41–3.47 Expects name an order the replay will not show | The built order everywhere: Peer doctor, Consultant, Nurse, Allied health professional, Other; Consultant's card above Nurse's; the select defaulting to Peer doctor; the Steps Expects follow |
| M3 | cast, design | K4 (after the remove) reads "Opening emails each of the 7 invitees" above "6 addresses" and All groups 6. The design review's further claim (Palesa Tau in one group on K2–K4) does not hold: the fixtures move her from Allied health professional to Nurse | "each of the 6 invitees"; K2 and K3 unchanged |
| M4 | code, cast | K15 (learner feedback's draft) reads "2 respondent groups reporting", which `CreateMsfCampaign.cs:76–80` refuses and the form disables at 1 (`CampaignEdit.razor:123, 687`): drawn so, it could never release. It also names PAED-010 and PAED-012, which learner feedback cannot cover (only PAED-015's list names it, D35), and invents 3 invitees, 5 / 2 and 2026-10-28 | "1 respondent group reporting", the field shown disabled at 1 with its help; Evidence for PAED-015 only; ‹n›, ‹thresholds› and ‹closes› where the ask gives nothing. The Spec's learner note says the minimum is fixed at 1 |
| M5 | code | R10 (an ended programme) warns that she "can no longer open MSF reports", then says "As Lerato Molefe will read it once released" and, in the release section, "Lerato Molefe can read your narrative and the groups shown above on MSF reports"; the mail line is dropped without a word. R6d's dialog list item 1 is the same | When `SubjectIsCurrent` is false: "As Lerato Molefe would have read it"; the release section's first line "She can no longer open MSF reports; the report is kept with the evidence on her record."; the same in R6d; the mail line as F4 |
| M6 | code | The Spec's DTO table lists as additions members that exist (`EvidenceRecordedOn`, `Recorded`, `ReleasedOn`, the teaching contexts) and omits the one R6b and the release section need: which EPAs will be recorded before release | Restate it as above ("Data the build must add"); the release section's records line and R6a–R6b read the new flag |
| M7 | design | The required mark is not DESIGN.md's (§ Form system, :1315): the boards write `<span aria-hidden="true">*</span><span class="visually-hidden">required</span>` on each label, with no `.required-mark` (so the star is not `--danger-color`), no `aria-required="true"` on the control, and no explaining line; the hidden word belongs on a group's legend only | `<span class="required-mark" aria-hidden="true"> *</span>` and `aria-required="true"` on every control F6 names; the hidden "required" on the EPA legend only; "* marks a field you must fill in." above the first card. The Spec says R12 changes the markup, not only which fields carry a star |
| M8 | design | The status card's stripe: R7 and R7b override `.detail-card--emphasis` inline (`border-left-color:var(--success-color)`) while K11, the same Released state, and K12 (Withdrawn) stay blue, the colour DESIGN.md gives "the viewer's move" | As F7, as classes (no inline colour), the same on K and R for each state, listed in the Spec's NEW and changed table |
| M9 | design | C4's refused create is `<ul class="validation-summary-errors" tabindex="-1">` with no `role="alert"` (not announced), no "what to do" line, and its link targets `#epa-h`, a `<legend>`, which cannot take the focus. DESIGN.md § Alerts (:1692) | Build it as `RefusalSummary` does: `validation-summary-errors error-summary`, `role="alert"`, `tabindex="-1"`, a bold "Not created." and a line saying what to do, the field's link moving the focus by script (`PageFocus.FocusByIdAsync`) to the first EPA checkbox. The Spec says which of the summary and the fieldset's own message takes the focus |
| M10 | design | The dialogs' 44 px buttons and stacked actions at 390 come from `.dlg-overlay .btn` and `.dlg-overlay .form-actions` (`R2-msf.css:189–190`), the canvas's own selectors, "never shipped". `app.css` has no phone rule for `.dialog-card .btn` (`.btn` is 36 px; `.dialog-actions` stacks only in form mode, `:1135`). BRIEF § 11 "Flow 09": a page with no wrapper class got none of its flow's phone rules | The rule written against what ships (`.msf-page dialog .btn`, or `.dialog-card .form-actions .btn` below 641 px), and the Spec says where the `ConfirmDialog`s sit so it reaches them; the `column-reverse` order (safe button last on screen, first in tab order) listed as a rule the Spec changes |

## Should fix

- **The dialogs' backdrops and labels** (cast S5–S8): R2 (Close) is drawn over 3.41's page; Close is pressed at 3.44, so the
  page behind reads 6 responses, of the 5 needed, 2 respondent groups reporting, the Consultant and Nurse cards and
  "Suppressed: Peer doctor, Allied health professional.". R6b, R6c and R6d carry the "Illustration" chip (R6d also
  PROPOSED). R6c sits over R9's page (no Evidence list, no records line); R6d over R10's (the warning, no mail line).
- **R8a and R9** as F10.
- **Unmarked wording** (code S5, S12; cast N12, N13): each new sentence is chipped NEW, or dropped. K2's add result as F11.
  K7 and K8's "A link nothing was heard of for an hour counts as not delivered." (true: `MsfInvitation.cs:378`) is NEW.
  C1–C5's "Kgosi Kgari Teaching Hospital's current trainees." names an institution no DTO carries
  (`TraineeProfileDto.cs:34` has `InstitutionId` only) and is wrong for an Administrator: "Current trainees you can run a
  campaign for." or DATA. "Today, on the South African calendar." is NEW and DATA (the default from the `TimeProvider`
  through `ProgrammeCalendar`). K16 and R12's "Go to MSF campaigns" is NEW.
- **A malformed address and the thresholds' range** (code S6): as built they name a property ("The Email field is not a
  valid e-mail address.", "The field MinimumResponses must be between 1 and 100."), which R11 forbids (T324). Draw K5b's
  malformed case as "Enter a valid email address, for example name@example.org." (NEW) and one range refusal ("Enter a
  number from 1 to 100." or F2's floor), and say whether the input is `type="email"` with `novalidate` (so the server's
  words show) or plain text.
- **Nested forms** (code S4): the template fold's `<form>` is drawn inside the campaign form's card (C1–C3), and Blazor's
  `EditForm` cannot nest. The Spec says which: the fold outside any `EditForm` with Add template a `type="button"`, or
  the campaign's `EditForm` closing before the fold.
- **Her report page needs a component of its own** (code S9): `NavOwners` keys by page type (`NavOwners.cs:130–132`) and
  `MyMsfReports` serves both routes (`MyMsfReports.razor:1–2`), so the id route cannot have its own h1, tab title and
  trail. The Spec names a second page component for `/msf/my-reports/{id}`, and M5's not-found alert is kept apart from
  the load error (today another trainee's id throws into `_error` and hides the list, `:127`, `:26`).
- **A group with zero or several scale questions** (code S10): the h3 row "Nurse · 4.50 average rating" assumes exactly
  one (`MsfDtos.cs:41–50` carries one aggregate per question). The Spec says: with none, the header names the group only;
  with several, each average under its own question.
- **Boards the rules call for are missing** (code S11, N18; design S14, S16): R10's fifteen EPAs in About and Evidence for
  on a K or R board, and PAED-015 in the report, the campaign facts and her "On your portfolio" (one board each, 390);
  her report page (M3) loading, load error and unavailable; a refused Open, Withdraw, Resend, Close and Release (the danger
  Alert, the focus left on the button) and Close's result; `/msf/respond`'s refused post (the "Error: …" tab title, the
  `.error-summary` with `tabindex="-1"` that takes the focus, `aria-invalid` on the comment); 768 for C, K7/K8
  (`.status-split`), V1 and one dialog.
- **`/msf/respond` keeps "the state's Alert"** (design S14): DESIGN.md § Anonymous static page (:4118) has one per state;
  Q3–Q8 draw a plain `<p>`. Keep an Alert per state (info for already used, warning for expired and closed, danger for
  the fault) or list its removal on the Spec.
- **CSS reuse** (design S6–S8): `div.actions-cell`, not `.row-actions` (the same declarations, `app.css:442`, held by
  `TableColumnClassTests`); `.submit-check`, not `.check-line` (`app.css:1634`); and every class the boards use gets a
  Spec row, NEW with its reason or "reuses `.x`": `.msf-header`, `.section-title` (against `.list-section-title`),
  `.count-table`/`.count-box`/`.num`, `.nowrap`, `.kind-line`/`.trainee-cell` (against `.person-cell`), `.stored`/
  `.msf-facts` (against `.details-list`), `.groups-intro`/`.group-avg`, `.skeleton-stack` (Skeleton's `Count`),
  `.choice-stack`, the three coverage classes, the three `.respond-*`.
- **DESIGN.md text the Spec omits** (design S9): § The MSF campaign page's h4s become h2 and h3; the campaign card's
  `details-list` gives way to the status card and About (State and Response window move to the badge and the subtitle);
  the Quick template card becomes the Questionnaire card with its fold; the required mark (M7); which tables do not stack
  (S13); `/msf/respond`'s Alert (above); the status stripe (M8); the draft's Remove now in a list item, outside § Table
  system's column-header rule.
- **The create page's EPA card** (design S10): give the card an h2 like the other three and keep the legend a label inside;
  reuse `.choice-group.is-invalid`, which has the `forced-colors` mark `.epa-fieldset.is-invalid` lacks (WCAG 1.4.1, T236).
- **Coverage** (design S11): V1 drawn at 768; the Spec lists the named scroll region as R9's deliberate exception; the
  count tables (K, R) stay unstacked and say why.
- **The add form at 768** (design S12): three columns until 641 px leave the email about 155 px and the select about
  100 px at 768. Stack it below 900 px (DESIGN.md :1007) and align Add invitee with `align-self:end`, not a
  `margin-top:1.95rem`.
- **Links in sentences** (design S13): "withdraw it on its campaign page" (in `.field-help` and the disabled Release's
  reason) gets the inline-block padding flow 08 settled (BRIEF § 11); "Open its campaign page" as F9.
- **Inline styles to classes, one measure** (design S15, code N19): `max-width:60rem` on R and none on K, though both draw
  the same status and About cards; 58 inline grids, 17 inline h3 sizes, R7's colour, the skeletons' sizes. One measure
  for K and R as a class, `.section-title--h3`, Skeleton's `Width`/`Height`.
- **Placeholders filled from what the code gives** (design S17, code N14, cast N14): K10's Withdraw body and K12's
  withdrawn sentence from `MsfCampaignText.WithdrawBody` / `StateNote`; R12's sentence; K5b's refusal (above); R11's
  4,000-character comment drawn at length, not as a placeholder. ‹date› stays where the replay has none.
- **The Steps board** (cast S3, N16, N17; code N17): 3.44 keeps the narrative and "Supervision level this feedback supports
  (Not stated, 1, 2, 3a, 3b, 4, 5)" beside Release; 3.36 says whether an add that keeps the select's default counts a
  field (M2 makes it Peer doctor); the Create journey says it starts at Home's Start an MSF campaign (3.34's route is Home,
  the list, New campaign: one press more) and counts "5 + 2" (two minimums typed, not three); the Spec says L5 stands for
  A.4.7; 3.41's Expect as F1 decides; the figures as F2 decides.

## Nits

- R11 (a dozen comments): the Nurse card "from 12 responses" against the status card's "6 responses" and Consultant
  "from 2": make the status 14 (code N15).
- R8a, R8b: the disabled Release's reason keeps "withdraw it on its campaign page" a link, as today
  (`CampaignReport.razor:105`) (code N16).
- Q1–Q8: the brand mark is 44 px as `MsfRespond.razor` renders `/brand/wombat-mark.svg`, not 56 (code N18).
- The Spec's counter: say it counts in the browser as flow 09's does (DESIGN.md :1318), in UTF-16 units as
  `[MaxLength(4000)]` counts (code N20); "Up to 4000 characters" on `/msf/respond` against "4,000" on the report (design
  S14).
- One convention for a dropped EPA across R6b, R7b and M4 (PAED-012 throughout, since the campaign names PAED-010 and
  PAED-012), and R6b's backdrop to match (cast N11).
- K13's strip: "a draft campaign for a registrar whose programme has since ended", not du Plessis's withdrawn #2 (cast N15).
- C1, C2: `aria-describedby="epa-help"` names no element (design 18).
- L2–L7: the count chips use `badge-draft`, the Draft state's class: say it is on purpose or use a plain count (design 19).
- C1–C2 show 8 / 3 / 2 and C3–C5 5 / 2 / 2 with nothing saying Mr Smit typed them; K12 keeps 8 / 3 / 2 (design 20).
- R7, R7b head the status card with a hidden h2 "Status" and K11 with a visible sentence: one way (design 21).
- The 390 boards omit the closed menu's list and the account row that flow 09's keep (design 22).
- Save the canvas's `ds/` (tokens.css, bundle.css) with the round, so a board can be re-rendered (design 23).
- Count tables in `.table-container` without `.shadow` inside a card: say so; the template fold's summary in
  `--link-color` reads as a link; K13 disables its email and select natively (allowed for controls; say so) (design 24).

## Who raised what

| Here | Code review | Cast review | Design review |
|---|---|---|---|
| M1 | | 1 | |
| M2 | | 3, 4 (S) | |
| M3 | | 2 | 1 (in part; the rest dropped) |
| M4 | 1 | 10 (S) | |
| M5 | 3 | | |
| M6 | 2 | | |
| M7 | E | | 2 |
| M8 | | | 3 |
| M9 | | | 4 |
| M10 | | | 5 |
| F1 | 7, A | | |
| F2 | C | | |
| F3 | 8, B | | |
| F4 | D | | |
| F5 | 13 | | |
| F6 | E | | 2, Q3 |
| F7 | | | 3, Q1 |
| F8 | | | 25, Q4 |
| F9 | | | 13, Q5 |
| F10 | | 9, Q3 | |
| F11 | 12 | 12, Q4 | |
| Settled | | Q1, Q2 | Q2 |
