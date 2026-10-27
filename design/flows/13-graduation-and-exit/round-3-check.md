# Flow 13 round 3: the check

Round 3 (canvas "Wombat 13", saved in `round-3/project/`: 192 boards on page `r3` of `canvas.json`, which agrees: 192 boards carry `page: r3`;
five Spec/Steps boards `R3-0-*`, 187 screens: 89 at 1280, 90 at 390, 8 at 768, plus `f13r3.css`) was checked item by item against
`round-3-ask.txt` (F1-F11, M1-M5, S1-S24, N1-N16, T1-T12, A1-A5: 73 items), by a Sonnet agent for the main session, 2026-10-27.

Method: every board's HTML with script, style and svg stripped and reduced to class names, roles, aria attributes, ids and text (script
`dig.py`, scratchpad); the five Spec/Steps boards and `f13r3.css` read whole; every H, X, E, V, P, C and R board read at 1280. The 390
twins were compared to the 1280 boards by script: text and markup identical on all 89 pairs (R3-H08 has no 1280 twin by design), so the
width differences are CSS only; the eight 768 twins are the same boards under a different caption. Scripted over all 192 files: link order
in `<head>` (`ds/wombat/tokens.css`, `ds/wombat/components/bundle.css`, `f13r3.css`: all 192 correct), `<style>` content (`body{margin:0}`
alone on all 192), `style=` attributes (0), raw hex in the markup or in `f13r3.css` (0), every class used against `bundle.css` and
`f13r3.css` (0 unknown; `breadcrumb-ancestor`, `breadcrumb-separator`, `page--signed-out` are in `bundle.css` inside compound selectors),
retired classes (`verify-answer`, `verify-details`, `closed-when`, `end-help`, `end-decision`, `badge-draft`: 0 on any board; they occur
only as words on the Spec), pronouns in the captions and annotations (`she/he/her/his/him`: 0), dialogs (6 boards x 2 widths: the safe
button is first in the DOM on every one), Illustration word in the title against the chip (24 of 24, none missing, none extra). Each R3 board
was diffed against its R2 twin (every family; see "Regressions"). **Not done: the rendered measurements.** `ds/wombat/tokens.css` is not
in `round-3/project/` (nor in round 2's) and `bundle.css` is only in `design/system/components/`, so the boards cannot be drawn faithfully
here; the Spec's A3 and A4 tables are taken as reported, not re-measured. Per the instruction, the captures did not arrive (R3-0-spec
says so, first section, and lists the 19 it could not open); brackets only a capture holds are not failures and are listed under Build notes.

## Verdict

Tally (rows below): **73 rows. Done 70, Partly 3, Not done 0, Regressed 0.**

**Round 3 is final and fit to build from.** All 73 items are drawn or written; every Partly is a measurement or a bookkeeping gap, not a
design question. Nothing round 2 had right is now wrong (see "Regressions": 1 nit). No invented person, address, id, date, count or order:
the new Illustrations bracket everything not in FIXTURES. No further round is needed; the build notes below carry what the boards leave.

## F1-F11

| Item | Board(s) | Status | Evidence |
|---|---|---|---|
| F1 export gate, MayExport | X01-X10, P01, P05, P11; Spec, Spec 2 | Done | Spec 2 "MayExport bool on CommitteeReviewDetailDto, ProgrammeTraineeDto and TraineeProfileDto: MayReadAsync(caller, trainee user id)"; the action is `div.actions-cell > a.btn.btn-outline` inside `.header-container`, a sibling of the title block, on X01, X02, X03, X04, X05, X06, X07, X09, X10 and every P profile board; absent from X08. |
| F2 take-back count | P03, P05, P09, P11, P20, P21; Spec 2 | Done | Spec 2 "IRequest<ProgrammeEndResult> with TakenBack (int) ... nothing when 0. The dialog's count is GetTakeBackForecastQuery"; P09 "1 activity, observed 2026-10-25, will stop counting."; P11 "1 activity observed after that day no longer counts."; P03, P05 no clause. |
| F3 T312 grown | R05, R06, P18; Spec Prerequisites | Done | Prerequisites block "T312 lands first ... grown by F3 (recorded in T312 on 2026-10-27)"; R05/R06 carry the `chip--t312` pill "after T312"; P18's annotation says what T312 changes. |
| F4 no trail on personal export | E01, E07, E08 (and E02, E11, E12, E16) | Done | No `nav[aria-label=Breadcrumb]` on any of the seven; exactly one `aria-current="page"` each (the menu's); Spec 3 DESIGN.md "Export portfolio ... lights itself and draws no trail (F4)". |
| F5 account sentence | P05, P11, P18, P21, P24, P25; Spec 2 | Done | P05 "holds no role now, and can still sign in to read the record, export the portfolio and download the STAR certificates."; P11 (no STAR) "... read the record and export the portfolio."; P24 "‹registrar› now holds Assessor. They can still read the record and export the portfolio."; P25 only `p.account-open > a` "Open ‹registrar›'s user page"; erased: Spec 2 S5 "no sentence, and the name is the pseudonym". |
| F6 holds a STAR | H01, H02, R01, R02, R09, E19 | Done | H01 `a` My authorisations "Your 15 STARs in force, each with its certificate to download."; H02 `span.record-link-plain` "STARs" + "No STAR was issued to you ..."; R02 record line "My activities" alone; R09 reachable "by its address only"; E19 David Naidoo "Portfolio unavailable". |
| F7 closed list for the Administrator | C01-C07; Spec 2 | Done | C06 `th` "Name and institution" and `span.person-cell` name over "Kgosi Kgari Teaching Hospital" in each row; Spec 3 "‹n› ended (AdmissionText)"; C07 `‹deleted_user_…›` Withdrawn ‹its day›. |
| F8 Mark complete's address | P03, P19, P20; Spec 2/3 | Done | P03 "a graduation email is sent to molefe@kgk.wombat.local."; P19, P20 "‹address›"; Spec 2 S12 "NEW, from TraineeProfileDto.Email". |
| F9 Verify's answer container | V02, V03, V07, V09, V10 | Done | `div.action-result[tabindex=-1] > div.alert.alert-success[role=status] > p > strong "Export verified."` then the sentence, then `dl.detail-list`; replaces R2's `section.verify-answer` + h2 (diff). |
| F10 badges | C02, C04-C07, P05, P11, P21 | Done | `badge-completed` / `badge-withdrawn`; `.badge-withdrawn` in `f13r3.css` ("Row badge-withdrawn") and Spec 3 class table; `badge-draft` on 0 boards. |
| F11 "last day" nouns, P22 | P02, P07, P22; Spec 1 | Done | Spec F11 row "EnsureCanEnd and its tests say 'last day'; the before-start refusal a NEW row with its own board"; P22 "The last day cannot be before the programme start date (2023-01-15)." |

## M1-M5

| Item | Board(s) | Status | Evidence |
|---|---|---|---|
| M1 gate drawn | X01, X03, X05, X08, X09, X10; Spec | Done | X08 Illustration, `‹registrar› · ‹period›`, no progress link, no action, annotation "MayExport is false: the export would be refused, so the header offers nothing."; X09 "Demo Administrator" (top bar) and X10 Nolwazi Mbatha: no `.review-progress-link`, action in the header; X01/X03/X05 keep "Lerato Molefe's progress" in the subtitle and the action in `.actions-cell`. No board says an Administrator is refused (grep). Spec 2 writes MayExport and says what the Administrator and Institutional admin see. |
| M2 take-back | Spec 2; P03, P05, P09, P20, P21 | Done | Spec 2 command row; P20 dialog "‹n› activities, observed after ‹day›, will stop counting."; P21 result "... graduation email sent. ‹n› activities observed after that day no longer count." |
| M3 T312's ended EPA page | R05, R06; Steps 6.21 | Done | R06 has no `PausedRow` alert, h1 and trajectory heading "(no longer in use)" (`span.paused-mark`), Observations lines, "Training year 4: level 5 ...", Entrustment "5", badge "At or above", Issued 2026-10-26, "5 · reached", `a.epa-auth-link` "Open My authorisations"; R05 PAED-012 card "(no longer in use)", standing "15 of 15 EPAs"; both `chip--t312`. Steps 6.21 "PAED-012's page reads as R3-R06". |
| M4 staff trail | E04, E05, E13, E17, E18 (Trainees); E06, E20 (Programme trainees); Spec 2 | Done | `Home > Trainees > Lerato Molefe's programme (href /admin/trainees/edit?id=2) > Export portfolio`; E06/E20 `Home > Programme trainees > Lerato Molefe (href /programme/trainees/2) > Export portfolio`; E17/E18 bracketed names; Spec 2 "GetExportSubjectQuery ... ProfileId (the preferred profile from PreferredProfiles; null when none)". |
| M5 A.7.12's board | V02 (1280, 390, 768), V07-390; Steps | Done | V02 title "5.13 verified, lower case, three exports" at all three widths; V07-390 "5.23 the second file (at 390, A.7.12)"; Steps row A.7.12 "R3-V07 at 390". |

## S1-S24

| Item | Board(s) | Status | Evidence |
|---|---|---|---|
| S1 one trail rule | E01, E07, E08 | Done | No breadcrumb `nav`; one `aria-current="page"` (counted by script at 1280 and 390). |
| S2 lit item by route | E09, E10, E19; Spec 2 build rule, Spec 3 edge list | Done | E09, E10, E19 menus have no lit item; Spec 2 "S2: /portfolio/export/{id} lights its staff owner by route ... the Unavailable page passes Page=, so nothing is lit"; edge list "S2: a staff member who is also a former trainee ...". |
| S3 the refusals | P02, P07, P22, P23; Spec 2 | Done | P02/P07 "The last day cannot be after today (2026-10-26)."; P22 value "‹a day before 2023-01-15›", refusal above, no `dialog`; P23 `value=""`, "Choose the last day.", no dialog; Spec 2 S3 "EndDate becomes nullable on the page, so an empty field reaches the press check". |
| S4 the profile's read | P12-P15; Spec 2 | Done | P12/P13 identical body ("This trainee profile does not exist, or you cannot open it."); P15 fixed words + Try again; Spec 2 "GetTraineeProfileByIdQuery: Null for unknown and out of scope; the DTO adds EndKind, EndDay, TrainingYearAtEnd, LastRatifiedDecision, MayExport, Email"; "StatePanel gets LoadError and OnRetry, Try again focusing the h1. File: TraineeProfileEdit.razor". |
| S5 account sentence | P05, P11, P24, P25; Spec 3 DATA | Done | As F5; DATA row "roles now, locked, erased and the STAR count for the account sentence". |
| S6 Export's changes | E11, E12, E13, E19, E20; Spec 2 | Done | E11 refusal under From; E12 "Choose the from date."; E13 fixed words; Spec 2 "ValidationException mapped to its field; every other exception shows E13's fixed words"; "'returns the file name' struck (PortfolioExportResult.FileName exists)"; E19 Naidoo "Portfolio unavailable", nothing lit; E20 To hint "Lerato Molefe's last day in the programme.", From "Lerato Molefe's programme started on 2023-01-15." |
| S7 built subtitles | R01, R02, R05, R10, P01-P04, P06-P10, P16, P17, P19, P20, P22, P23, R07 | Done | R01/R02/R05/R10 "Your record of the programme you ended: what your curriculum held you to in each period."; the P boards "Update curriculum and completion details for this trainee." (P18/P21/P05/P11, the ended ones, keep "The record of a programme that has ended."); R07 "Everything you have filed or logged, newest encounter first. Read-only: your programme ended on 2026-10-24, so nothing new can be filed." Spec 3 messages list each. |
| S8 record line, Home's STAR rule | R01, H01, H02; Spec 2 | Done | Spec 2 "The 'Your record:' line is MyProgress.razor's, around EndedRecord (its golden-markup test unchanged), never on the staff page; if the standing read fails it shows My activities only"; H01 "Your 15 STARs in force, each with its certificate to download."; GetFormerTraineeHomeQuery row (preferred profile, kind, year, STAR count, null for a current Trainee); "None of your STARs is in force now; My authorisations lists them." (Spec 3, PROPOSED, no board). |
| S9 Where column | Spec 2 | Done | Rows for Features/Reporting, Features/CommitteeDecisions, Features/Trainees/ProgrammeEndCredit (internal), Features/Dashboards (+ `FormerTraineeRecordCard` under DashboardFrame, rendered by DashboardLinkAuthorizationTests), Infrastructure/Scheduling/Jobs + DependencyInjection.cs + ScheduledJobDefinition row, TraineeProfileEdit.razor, EpaProgress.razor, MyAuthorisations.razor "certificate download handler needs no change". |
| S10 not-a-hash family | V06; Spec 3 | Done | Three sentences in Spec 3 (wrong length with n, non-hex with c, the 12-digit prefix with "The 12 in a file's name are only its start."), NEW PROPOSED; V06 draws the third only. |
| S11 closed list paged | C04, C06, C07; Spec 2/3 | Done | Spec 2 "ListEndedProfilesQuery Paged in SQL, 20 a page, the latest end first ... PagerControls new on this page"; C04 20 rows + `nav.pager` "Showing 1-20 of 40", select "Ended programmes per page"; count word "40 ended". |
| S12 Mark complete's address | P03, P19, P20 | Done | As F8; Spec marks NEW. |
| S13 plain Verify link | E03, E05, E06, E08 (E20 before export) | Done | `<a href="/portfolio/verify">Verify portfolio export.` in the result sentence on all four; Spec 2 build rule S13. |
| S14 P18 | P18 | Done | Curriculum read-out `‹curriculum›` drawn as built; DATA list has no P18 row ("nothing for P18's label"); `chip--t312` only on the annotation "What T312 changes here: the record's EPA list and targets come from the curriculum the programme ended on." |
| S15 R10 | R10 | Done | "‹m› at or above · ‹…› below · ‹…› with no decision, of 17 EPAs", "‹m› of 17 EPAs ... (level 5: ‹…› · level 4: ‹…›)"; 17 cards, ‹PAED-016›, ‹PAED-017›. |
| S16 Steps board | R3-0-steps, -steps-2 | Done | See T1-T12 below. |
| S17 the brackets | R01-R08; X08, X09 | Partly | Every FIXTURES value under "The record" that a capture does not hold is drawn (R01, R05 semesters back to 2023; R03; R06; R04 "No STAR yet."; R07's rows; standing sentence and "15 at or above ..." line); the "did not arrive" notes are gone from R03, R04, R06; X08's ‹name as stored› is gone and X09 reads "Demo Administrator". But R08 keeps `‹title, as seeded›` on eleven cards (PAED-003 to -009, -011, -013 to -015), though FIXTURES says "titles from the seed" and the seed is in the repo, not a capture. Spec says "the seed is not in hand". A build note, not a design gap. |
| S18 R07 | R07 (1280, 768, 390) | Done | Trail `Home > My progress > My activities` on all three widths; `<caption>` "Every activity filed by you or about you, with who has it now and its credit."; Spec 2 S18 "the shipped table has none and the build adds it". |
| S19 refusal before a dialog | P02, P22, P23, P07; Spec 3 | Done | `div.validation-message[role=alert]` in the field's group, input `aria-invalid`, `anno-focus` on the input; Spec 3 § Button system "A refusal found before any dialog stays beside its field with the focus in it, role="alert" so it is read at once". |
| S20 Verify's answer markup | V02, V03, V07, V09, V10 | Done | See F9; V10 `action-result` without `anno-focus`, caption "Focus: left alone, from a link"; no `section`, no `.verify-answer`; only "Export verified." is `strong`. |
| S21 detail list | V02-V10 | Done | `dl.detail-list` on the five answers; `f13r3.css` `.verify-page .detail-list{grid-template-columns:minmax(8rem,max-content) minmax(0,1fr)}` and `dd{min-width:0;overflow-wrap:anywhere}`; `.verify-details` on 0 boards. |
| S22 Programme cell | C01-C07 | Done | `span.person-cell` in th/td; `.closed-when` on 0 boards. |
| S23 badges | C02, C04-C07, P05, P11 and the Active boards | Done | "Withdrawn" and "Inactive" `.badge-withdrawn`; "Completed" `.badge-completed`; Status "Active" is plain `<dd>Active` on P01-P04, P06-P10, P22, P23 (and P08-P10, P16, P17, P19, P20 by identical markup); `badge-draft` 0. |
| S24 C04 | C04 | Done | 18 bracketed rows each `Withdrawn ‹day›`; Programme `th.col-wrap` and `td.col-wrap`; `.closed-list td, .closed-list th{overflow-wrap:anywhere}`; Spec 3 A4 901 row lists only R03 and R06 as nonzero, so C04 is 0 there. |

## N1-N16

| Item | Board(s) | Status | Evidence |
|---|---|---|---|
| N1 E17's annotation | E17 | Done | "The header reads from GetExportSubjectQuery (M4, N1) ... whichever institution the caller also serves." |
| N2 activity opened from My activities | R3-0-steps-2 | Done | T11 "N2: an activity opened from My activities by a former trainee has the trail 'Home > the activity'." |
| N3 Trainee acting as another role | Spec 3 edge list | Done | "N3: a holder of Trainee acting as another role is offered My progress and My data rights only (Export portfolio is in the Trainee menu)." |
| N4 autofocus | V05, V06; Spec 2 | Done | `input#hash ... autofocus` on both; Spec 2 build rule N4 "when the page renders a refusal (static SSR, no script)". |
| N5 header slot | X01-X10; Spec 2 | Done | Spec 2 "PageHeader's Actions on the review page (moved out of SubtitleContent), a new Actions slot on the programme page"; boards X01-X10 carry it in their captions and markup. |
| N6 one Status row | P05, P11, P21 | Done | `dd` = `span.badge.badge-completed "Completed"` + " 2026-10-26"; P11 `badge-withdrawn "Inactive"` + " Left the programme: 2026-10-24"; Spec 2 N6 says which words move into the badge. |
| N7 R07's Mini-CEX | R07 | Done | `span.activity-link-to` "to David Naidoo" (visually hidden ", " before it) under the title. |
| N8 states.md list | R3-0-steps-2 | Done | T11 lists E09-E15, E19, H05, H06, P06, P12-P15, P22, P23, V08-V10, X03-X07: exactly the ask's list. |
| N9 bracket what belongs to a bracketed person | E16, E17, E18, P19, C04 | Done | E17 `from value=""` + "started on ‹start date›"; E18 same + "Training year ‹n›"; P19 "‹address›"; E16 title "‹about 200›"; C04 ‹curriculum› on the bracketed rows. |
| N10 no pronoun in a note | all captions | Done | Scripted: 0 `she/her/he/his/him` in any board-title, board-sub or `.anno` on 187 boards ("their" only). |
| N11 one stylesheet | all 192 | Done | Link order and `<style>body{margin:0}</style>` verified by script; `f13r3.css` section 1 canvas, section 2 each NEW rule under a `/* Row "..." */` comment; no NEW rule on any board. |
| N12 E14, E15 | E14, E15 | Done | `Home > Trainees > Export portfolio`, no name, h1 "Export portfolio". |
| N13 describedby | P01-P04, P06-P10, P16, P17, P19, P22, P23 (+P20) | Done | Scripted on 1280: both triggers carry `aria-describedby="end-mc-note"` / `"end-dc-note"` and the notes carry those ids on all 15. |
| N14 | all P | Done | `h2#end-h` without `tabindex` on all 15 (script); `.end-help`, `.end-decision` 0; decision line a plain `<p>`; help a `.field-help`. |
| N15 | V05, V06; Spec 3 | Done | `aria-describedby="hash-e hash-h hash-cmds"`; Spec 3 DESIGN.md "A refusal is named first by the field (aria-describedby, N15)". |
| N16 pager select at 390 | C04-390; Spec 2/3 | Done | Spec 2 N16 row (shared PagerControls, "proposed as a system task ... .pager .form-select{min-height:2.75rem} below 641px, not in f13r3.css"); Spec 3 A3 lists "R3-C04-390: select ... (28)" as the exception. |

## T1-T12

| Item | Board(s) | Status | Evidence |
|---|---|---|---|
| T1 5.8 | Steps 2 | Done | "5.8 Expect unchanged; Note rewritten: the 'last point ... to download' Note is false after Q3. F-5.22a closes." |
| T2 profile-opening steps | Steps 2 | Done | 2.30 (Ndlovu, edit?id=6), 6.34 (Dlamini, id 3), 6.36, A.7.9 gain the h1, no Back to trainees, End this programme, header action; 2.39 and 2.40 unchanged. |
| T3 A.7.9 | Steps 2 | Done | "the closed list stacked, View 44 px (not 'scrolls in its own container, each Edit')"; "three fields plus the last day and two triggers". |
| T4 A.2.1, A.2.9 | Steps 2 | Done | nine-job list becomes eight; "All nine" to "All eight"; title "the other six jobs" to "the other five"; cleanup bullet and 90-day Note go; F-A.2.9a closes with T313. |
| T5 6.21, 6.40, 6.41 | Steps 1, 2 | Done | 6.21 "a PAED-012 card ... '15 of 15 · level 5: 9 of 9'; PAED-012's page reads as R3-R06; PAED-006's card 'PAED-006 - Managing long-term health conditions (LTHCs) in children'. F-6.21a closes"; 6.40 "15 EPAs, no KGK-001. F-6.40a closes."; 6.41 "no KGK-001; '15 with no decision, of 15'". |
| T6 summary and order | Steps 2 | Done | "no 'Name:' and no 'Trainee summary' heading; the closed list the latest end first at A.1.14, 5.27 and 6.34; A.1.14 gains the closed list's line with the erased row (R3-C07)." (Boards agree: the summary is `dl[aria-label="Trainee summary"]` with Status and Email only, no visible heading.) |
| T7 5.10, 5.12, 5.23 | Steps 1, 2 | Done | "the action in the page header (F1); 5.23: no trail (F4), Export portfolio lit." |
| T8 A.7.12 | Steps 1, 2 | Done | "R3-V07 at 390". |
| T9 counts | Steps 2 | Done | "5.17's result built from 0 (nothing added); 5.27's from 1; 5.27's dialog 'will stop counting'." |
| T10 the rest of the cast review's list | Steps 1, 2 | Partly | Rows (what changes, which Gap closes) exist for 5.9, 5.10, 5.12, 5.13, 5.14, 5.16, 5.18, 5.20, 5.21, 5.22, 5.23, 5.28, A.7.4 and the unchanged list is right. A.1.6 and A.4.6 have no row of their own (one clause in steps 2: "Pieter du Plessis's Home: R3-H02's card, his menu with Export portfolio"), and the coverage.md rows at lines 143, 223, 323, 385, 494 are said to be "each re-pointed to these R3 boards (their text not in hand here: check)": not verified by the board. |
| T11 N2, N8, A4 | Steps 2 | Done | All three lines present; A4 "Pieter Smit reaches the export from the review page (R3-X03), or the programme page by its address, /programme/trainees/2 (R3-X04)". |
| T12 journeys | Steps 2 | Done | Seven journeys, columns Today / 1280 / 390 / "Changed by F1, F4, M4?", each answered (e.g. Prof Mbatha's 5.10: 4 + 3 today, 4 + 0 at 1280, 5 + 0 at 390, "no (F1 moves no action on the profile; M4 adds a link back)"); presses recomputed correctly (Trainees > Edit > Export > Export PDF = 4). |

## A1-A5

| Item | Board(s) | Status | Evidence |
|---|---|---|---|
| A1 primary on top | all 390 forms and dialogs | Done | DOM order is secondary first on every form and dialog (Cancel then Save profile; Cancel then Mark complete / Deactivate); `bundle.css` carries `.dialog-card .form-actions{flex-direction:column-reverse}` (line 6159) and `.user-page .form-actions` column-reverse (line 6492); Export and Verify have one button (`width:100%` at 390 in `f13r3.css`). Spec 2 A1 states it. |
| A2 form parts | Spec 2; E*, V*, P* | Done | Spec 2 A2 names each holder: Export `form.form-container` child of `.export-page`; Verify `form.form-container` child of `.verify-page`; Profile details the form inside `section.user-section`; End this programme "the section itself (no form element)". The markup agrees (P01: no `form` in the End section). |
| A3 44 px at 390 | Spec 3 | Done | "90 boards measured, 583 controls"; exceptions each named: R3-C04-390 select (28, N16), E05/E06-390 inline Verify link (20), R07-390 four activity links (20, flow 03's shipped table). 90 is the number of 390 boards. Reported, not re-measured. |
| A4 re-measure at seven widths | Spec 3 | Partly | Table drawn (1280 / 1100 / 1000 / 901 / 900 / 768 / 390; 89, 89, 89, 89, 89, 97, 90 boards; counts agree with the files). 0 px at six of the seven; at 901 "R3-R03-1280: 17 px, R3-R06-1280: 17 px" (flow 05's `.details-list` 128 px term column in `.details-grid--reverse`, between 901 and about 940 px), named as flow 05's and proposed as a system task, not fixed. The ask says 0 px at each. Reported, not re-measured. |
| A5 no secret, no impossible pair | all | Done | No password or token drawn; V04's hash is the bracketed `5c171ec5‹…›`. No board holds two states that cannot hold at once (a dialog over its page holds; P22/P23 draw the refusal with no dialog; P07 closes the dialog before showing the command's refusal). |

## Regressions against round 2

Every R3 board was diffed (tag-stream diff, head and menu compared apart) against its R2 twin. Every change is traced to an item; nothing
round 2 had right was lost.

- **H01-H07:** hrefs added; H01/H07 "Your 15 STARs in force" (S8); H03 pronoun out of the bracket (N10). Menus unchanged.
- **X01, X03, X05:** the export link moved from the subtitle into `.actions-cell` (F1, N5); progress link kept with an href. X02, X04, X06, X07 unchanged. X08 redrawn (M1); its menu is now the Coordinator's, correct for Pieter Smit (R2 drew the Administrator's menu with ‹name as stored›).
- **E01, E02, E09-E12, E16:** no change. E03, E05, E06, E08: Verify link made a real href (S13). E04, E05, E06, E13, E17, E18: crumb hrefs (M4). E07, E08: breadcrumb removed (F4). E14, E15: trail added (N12). E17, E18: start date and training year bracketed (N9).
- **V01, V04, V08:** no change. V02, V03, V07, V09, V10: answer container and `dl.detail-list` (F9, S20, S21). V05, V06: describedby order and autofocus (N15, N4).
- **P01-P04, P06, P08, P10, P16, P17, P19:** subtitle (S7), `h2#end-h` loses tabindex, `.end-decision` goes, describedby on both triggers (N13, N14). P02, P07: `role="alert"`, describedby order (S19). P09, P16, P17: forecast clause (M2). P05, P11, P18: badge (F10, N6), `.account-open`. P19: address bracketed (F8). P12-P15 unchanged.
- **C01-C05:** `closed-list`, `col-wrap`, `person-cell` for `closed-when` (S22, S24), `badge-withdrawn` for `badge-draft` (F10). C03 unchanged.
- **R01-R08, R10:** FIXTURES values filled, subtitles (S7), hrefs on the links, "did not arrive" notes removed (S17); R07 trail, caption, rows (S18, N7). R09 unchanged.
- **One nit (not counted):** P16 ("the end on the programme's first day") had `‹start date›` in R2 for the day field and the dialog; R3 has the less specific `‹day›` (the ask's own bracket list names ‹day› for P16-P18). The build reads the real value; a replay-time caption could say "the first day".
- **No regression found** in: headers and titles (all `R3-` with width, none missing a twin), the 768 and 390 twins (identical to 1280 markup), nav menus (only X08 differs, see above), dialog titles and buttons, sentence wording.

## Build notes

1. **ds/ not saved** (the same housekeeping as flow 12's round 3). Every board links `ds/wombat/tokens.css` and `ds/wombat/components/bundle.css`; `round-3/project/` holds neither (and the repo's `design/system/` has `components/bundle.css` only). Copy both from the design system into `round-3/project/ds/wombat/`. Not a defect of the design.
2. **A3 and A4 are reported, not re-measured** here. The one real finding is the 901 px overflow on R03 and R06 (flow 05's `.details-list`, a shipped component, not changed in this flow): the build either lands the proposed system task (`minmax(0, 1fr)` on `.details-list`'s value column) or states the exception in the handoff. The two other proposed system tasks are N16's `.pager .form-select{min-height:2.75rem}` below 641 px and flow 03's one-line `.activity-link` height at 390.
3. **Brackets only a capture holds (expected, not failures):** R02 and R04 2025 semesters ("as 5.28-2 records it") and R02's standing; R03's two MSF lines, trajectory chart and activities table; R04 and R06 trajectory line, chart and activities table; R05 PAED-006's target line and Semester 2, 2026; R07's six other rows (EPA, date, who, credit, and the Teaching Session Logs' state); R08 titles other than PAED-001, -002, -010 and -012 (also derivable from the seed in the repo: the build reads them, `PaediatricCatalogueSeeder`); R10's ‹title›, ‹n›, ‹as it was on the last day›; V04's `5c171ec5‹…›`; H03's recent decisions and the 60-character names (H07, E18, P19); E17's ‹institution› and ‹registrar›; C04's further people; C05's second person; C07's `‹deleted_user_…›` and day.
4. **Values the boards derive rather than quote from FIXTURES** (the build confirms them against the replay): R02/R04 du Plessis "Semester 2, 2026 ... 0 recorded" (the take-back of the Mini-CEX observed 2026-10-25), "Minimum when your programme ended: 3b" / "Training year 2: level 3b" (carried from round 2), the wording of R07's note row ("Observed after your last day in the programme, 2026-10-24, so it counts towards nothing."; Spec 3 lists "My activities' note row" as NEW with its reason DATA).
5. **C07's order is a drawing, not a rule.** The erased row is drawn first with the Spec line "‹its place if its day is earlier›"; the build orders by end day, latest first (S11), so the pseudonym's place follows the erasure day.
6. **GetExportSubjectQuery with a null ProfileId** (a caller whose subject has no preferred profile): the middle crumb is not drawn for that case. E14 and E15 (no name known) show the three-crumb trail; the build uses it, and a null ProfileId falls back to it. Say so in the handoff.
7. **Inputs drawn as `type="text"`** on P16, P20 and P22 hold brackets (`‹day›`, `‹a day before 2023-01-15›`); the build input is the date field (S3: EndDate nullable on the page).
8. **states.md rows (N8):** E09-E15, E19, H05, H06, P06, P12-P15, P22, P23, V08-V10, X03-X07, as T11 lists.
9. **coverage.md rows 143, 223, 323, 385, 494** (T10) are not verified by the boards; re-point them at the build, with A.1.6 and A.4.6 which have no Steps row of their own.
10. **Pre-existing, not flow 13's:** `body{background}` and the canvas layout rules in `f13r3.css` section 1 are canvas-only and never ship; the NEW rules in section 2 are the ones to move into `app.css` with their class-table rows (`record-links` .. `account-open`, `badge-withdrawn`, `closed-list`, `outcome-cell`, `export-page`, `verify-page`, `hash-value`, `cmd-list`, and the 641 px block scoped to `.export-page`/`.verify-page`/`.record-links`/`.record-line`/`.account-open`).
