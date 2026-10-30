# Flow 14 round 3: the check

Round 3 (canvas "Wombat 14", page `r3`, saved flat in `round-3/`: 149 boards, which `canvas.json` agrees with: 149 carry `page: r3`;
six Spec/Steps boards `R3-0-spec`, `-spec-2`, `-spec-3`, `-spec-4`, `R3-0-steps`, `-steps-2`, and 143 screens: 67 at 1280, 68 at 390, 8 at 768,
plus `f14r3.css`) was checked item by item against `round-3-ask.txt` (M1-M6, S1-S24, N1-N18, T1-T8, A1-A5: 61 items; F1-F10 are the
decisions behind them and are read with the items), by a Sonnet agent for the main session, 2026-10-30.

Method: all 149 boards downloaded from the canvas and read as HTML; the six Spec/Steps boards and `f14r3.css` read whole; every screen's
`<main>` reduced to text, classes, roles, aria attributes and ids by script and read for the boards each item names. Scripted over all 143
screens: link order in `<head>` (`ds/wombat/tokens.css`, `ds/wombat/components/bundle.css`, `f14r3.css`: 149 of 149 correct), `<style>` content
(`body{margin:0}` alone on 149), `style=` attributes (0), raw hex (0), `/_blob/` references (143 screens carry the mark
`/_blob/616538c681cf4475edf7c466864e408d` once; no other blob; the two bare `/_blob/` strings are words on the Spec), every class against
`design/system/components/bundle.css` (byte-for-byte the canvas's `bundle.css`, 232,840 bytes, v37) and `f14r3.css` (0 unknown;
`request-page` and `correction-records` on 0 boards), gendered pronouns in the whole text of every board (0), tables (50 tables on 23 board
families: all carry `clinic-table clinic-table--stack clinic-table--stack-wide data-rights-table`, `role=table`, `rowgroup` on thead and
tbody, `row` on each tr, `columnheader`, `rowheader`, `cell`; every td with text has its `data-label`; no actions cell has one), legends
(every `<legend>` has the aria-hidden star then the visually-hidden " required"), `required` attributes (0), `tabindex` on `h2#s-dec` and
`h2#s-cor` (0). The 390 and 768 twins were compared to the 1280 boards after removing ids: markup and words identical on every pair (R3-Q13 has
no 1280 twin by design). Illustration word in the canvas title against the chip: agree on all 143 (52 screens: 25 families, with C09 and Q09 at
768 too). Each R3 board was diffed against its R2 twin (see "Regressions"). **Not done: the rendered measurements.** The captures did not
arrive (the Spec says so, first section); A3 and A4 are taken as the Spec reports them, not re-measured here.

## Verdict

Tally (rows below): **61 rows. Done 59, Partly 2, Not done 0, Regressed 0.**

The five new states (R3-C11, D20, D21, D22, K02) exist at 1280 and 390 and carry the same words at both; f14r3.css fixes M4 to M6 as the ask
gave it; the stacked tables keep DataTable's roles and every stacked cell is labelled; no gendered pronoun, no inline style, no raw hex; the mark
is the given one; and the Spec reports the render-check: 143 boards, 277 renders, 408 controls at 390, 0 under 44 px, no sideways scroll at any
width. **Round 3 is final and fit to build from.** The two Partly are a wording claim and the form of one table, not design questions. No
invented person, address, id, date, count or order: what no cast member holds is bracketed and marked Illustration.

## M1-M6

| Item | Board(s) | Status | Evidence |
|---|---|---|---|
| M1 requester's name (F1, F2) | C01-C04, C07, M08; C05, C06, C10, M09, Q05, Q08, Q13; Spec | Partly | "Nomsa Mahlngu" on C01, C02, C03, C04, C07 (tab, h1, Requester, the three record lines, C07's "Changed on Nomsa Mahlngu's programme page." and link "Open Nomsa Mahlngu's programme" with `href="/admin/trainees/edit?id=5"`, the closing "… on Nomsa Mahlngu's My data rights.", C01's result "Request approved. Nomsa Mahlngu is emailed. Record each correction below, then complete the request.") and M08 (top bar; chip "read between the approval and the corrective save: top bar Nomsa Mahlngu"). "Nomsa Mahlangu" on C05, C06, C10, M09, D05, M07, Q04, Q05, Q08, Q13 (two rows on Q05, Q08, Q13). C05 and C06 keep "Mahlngu" once, in Value before, correctly. The Spec writes the name rule (R3-0-spec M1; code row "The name rule"; "submission stores First Last"). Gap: the ask wants every C board's annotation to say before or after the corrective save and the Spec says it does; C01-C07 and C10 do, but C08, C09 and C11 (bracketed people, not this request) say neither. |
| M2 erasure's reach (F3) | D13, D07, K02; Spec 2 | Done | D13 It ends carries "‹n› other open data rights requests: ‹requests›. Each is withdrawn."; D07 has no such line. K02 (Illustration, new): Pieter Smit's queue, three rows `deleted_user_‹…›` over "Erased ‹date›": Erasure Completed, Access Withdrawn, ‹type› Completed; no address; newest first. Spec 2 ErasureExecutor row: RequesterDisplayName on every request of the person, DecidedByUserId and AppliedByUserId to the pseudonym, other open requests withdrawn in the same save, "The completeness test covers the three columns". |
| M3 erasure's mail (F4) | Spec 2, steps-2 T2 | Done | Spec 2: `AccountNotices.SendToAddressAsync(address, kind, build)` (NEW), the handler reading the address with GetByIdAsync before ExecuteAsync, the class comment's exception, `DataRightsDecisionEmail` a NEW template under Common/Email/Templates, no link for an erasure. T2: A.1.12 one stub mail logged after the commit; A.1.13 reworded. |
| M4 390 selects | f14r3.css; Q01-Q13 at 390, K02 | Done | The "list selects at 390" rule is in `f14r3.css` as given (`.search-container .form-select, .pager .form-select {min-height:2.75rem}` below 641px). Every Q and K02 board at 390 that has selects (2 each; 3 on Q09, Q10 with the pager's) is under `.search-container` or `.pager`; the page-size select is `form-select form-select-sm pager-page-size-select`, and `.pager .form-select` outranks `.form-select-sm`. The class table lists it as a system rule; the Spec's render-check names it ("M4's rule is what brings the queue's Type and Status and Requests per page to 44px"). Not re-measured. |
| M5 DataTable's roles | M03-M10, M17, Q01, Q03-Q10, Q13, K02, C05, C06, C09 | Done | Scripted: all 50 tables, every role as asked, at every width. Spec: "The roles are DataTable's: a row laid out as a grid below 900px loses a table's semantics without them." |
| M6 every stacked cell labelled | same boards | Done | `data-label` counts: Submitted 148, Type 175, Status 175, Request id 148, Institution 114, Answer 27, Value before 20, Value after 20, Recorded 20; the badge cell is labelled "Status"; no row header or actions cell carries one. |

## F1-F10 (decisions behind the items)

| Item | Board(s) | Status | Evidence |
|---|---|---|---|
| F1 live name | M1's boards; Q; D; Spec 2 | Done | See M1 and S1; "First Last" at submission; the column only when erased. |
| F2 Mahlngu played | C01-C07, M08; steps-2 T1 | Done | T1: Dr Mahlangu saves Last name "Mahlngu", Mr Smit approves (C01), reads the empty section (C02), is refused (C03), M08, the corrective save, C05, C06, M09; route as the ask gave it. |
| F3, F4, F5 | K02, D13, D09; Spec | Done | See M2, M3, S5. |
| F6 institutional sign-in | C11 | Done | See S7. |
| F7 expiry notice | Spec 2 (Q27) | Done | Spec only, as asked; the row names T389. |
| F8 Withdrawn first, xmin | D12; Spec 2 | Done | See S13. |
| F9 Administrator's own; last Administrator | D20, D21 | Done | See S8. |
| F10 pager past one page | Q01, Q03-Q08, Q13, K02; Q09, Q10 | Done | See S23. |

## S1-S24

| Item | Board(s) | Status | Evidence |
|---|---|---|---|
| S1 email | K01, K02, D09, D22; Spec 3, 4 | Done | None of the four draws an Email row or a queue second line; the others read the live address. DATA row "The live name and email from GetContactsAsync". |
| S2 Programme start reads | C07, C08; Spec 2 | Done | C07 link `/admin/trainees/edit?id=5`; C08 "This programme has ended, so its record is archived and cannot be changed." with no link and no task id. Spec 2 row: preferred profile id (an int), ended, may-open, exists; "TargetId the requester's user id (a Guid) … does not link back to the profile". |
| S3 MyDataRightsDto | M01, M11, M18; Spec 2 | Done | Code row (InstitutionName, null for none; Requests). M01 and M11 name Kgosi Kgari Teaching Hospital with no request; M18 "The Wombat administrator decides it". |
| S4 queue's institution | Q01, Q02, Q11, Q12; Spec 2 | Done | Subtitle, caption and empty sentence name it; the no-institution Coordinator's subtitle and empty card are in the ListDataRightsRequestsQuery row (NEW, "no board"). |
| S5 forecast's scope | D13, D07, D14; Spec 2 | Done | D13 (Illustration): Pieter Smit, Coordinator menu, KGK; account line, "The roles ‹roles› and their scopes.", "‹n› open committee reviews: ‹reviews at Kgosi Kgari Teaching Hospital›, and 1 open review at another institution. Each is withdrawn, and nothing more is decided at it.", campaigns, other requests, programme. D07 and D14 stay devadmin's. Spec 2: the query gated as the command, names at the caller's institution, counts elsewhere. |
| S6 ErasureOutcome | D09, D22; Spec 2 | Done | D09: "The personal data is erased. The request is approved and completed. 1 committee review was withdrawn and the programme ended today. The account is now deleted_user_0ae1d64b; its open session ends within a minute." D22 (new, Illustration): "… Nothing else was open to end." Code row: ErasureOutcome (Pseudonym, ReviewsWithdrawn, CampaignsWithdrawn, RequestsWithdrawn, ProfilesEnded). |
| S7 institutional sign-in | C11 | Done | Illustration; the First name and Last name radios `disabled`, each "This name comes from ‹institution›'s sign-in; the institution corrects it."; Programme start and Other as C02's. Spec 2 row names AccountSignInMethods.InstitutionalSignIns and T388; Spec 3 says the disabled reading is the round's inference. |
| S8 Administrator's own; last Administrator | D20, D21; Spec 2 | Done | D20: top bar Demo Administrator, Requester Demo Administrator, no Decide section, "You cannot decide your own request: another Administrator decides it.", subtitle "Submitted ‹date› ‹time› SAST" with no institution. D21: refusal in the Decide ActionResult (`action-result pv-focus`, `alert-danger`), "Nothing has been erased." bold then the sentence as asked, the note kept in the textarea, buttons still offered. Spec 2: CanReview and CanRectify, CanAct refuses the requester, the refusal before ExecuteAsync. |
| S9 expiry notice | Spec 2, 3 | Done | Q27 row, T389 named, no board. |
| S10 MyDataRightsRequestDto | Spec 2 | Done | Fields as asked; "never DecidedByUserId"; CorrectedRecords by one grouped read. |
| S11 record's label and values | C04, C09; Spec 2, 3 | Done | Spec 2 ApplyRectificationCommand row (encoding, canonical-to-canonical, TargetType forms, 194, the page's two checks). "Name the record in the field under it." on every C board with the form; C09 draws Other chosen (`checked`) with `input#rec-name` value ‹the record's name› under "Record's name", recorded rows "Other › ‹record›" over ‹before›. Spec 3 and 4 carry the two refusals and the M09 line. |
| S12 queue's handlers and words | Q02, Q03, Q06, Q07; Spec 2, 4 | Done | Code rows (PagerControls 20 of 10/20/50/100, PageSizeLabel "Requests per page", Show and Clear filters, the search-container pattern, StatePanel LoadError and OnRetry). Words: caption tails ": Export, Submitted" (Q03), "No Erasure request at Kgosi Kgari Teaching Hospital. Clear filters to see every request." (Q02), the other forms in the messages table. |
| S13 Withdrawn check, token | D12; Spec 2 | Done | Chip names Withdrawn-first, the domain sentence never used, the row read again and drawn Withdrawn, the name live; alert "Sipho Ndlovu withdrew this request. Nothing was decided or erased." Spec 2 DataRightsRequest row: xmin, Withdraw() refuses all but Submitted, the decided-meanwhile sentence (NEW, no board). |
| S14 load outcomes | D17, D18, U01, U02 | Done | Chips: D17 "the read not yet back (not null, no exception)"; D18 "drawn when the read throws; never ex.Message"; U01 "the read returned null (unknown id)"; U02 "(foreign id)". Spec 2 GetDataRightsRequestByIdQuery row. |
| S15-S17, S19 Steps | steps, steps-2 | Done | See T1-T8. |
| S18 Clear filters once set | Q13; Q02, Q03, Q06, Q07 | Done | Q13 (390 only): no Clear filters, no pager, chip says so. The four others draw `a.btn.btn-outline` "Clear filters" after Show inside `.filter-actions`, whose phone rule (`.filter-actions .btn {min-height:2.75rem; width:100%}`, bundle.css) stacks them with Show on top. |
| S20 required groups | all M boards with the form, all C boards with the form (C11 too) | Done | Scripted: every `<legend>` reads the aria-hidden star then `<span class="visually-hidden"> required</span>`. |
| S21 NEW table cut | M, C, D boards; Spec 2 | Done | 18 M boards `user-page data-rights-page`; 34 C/D/K01 boards `user-page`; `request-page` and `correction-records` on 0 boards. Class table rows as asked, with "OUT (S21)". |
| S22 stylesheet | all; Spec | Done | See "Verdict". Spec, section 2: "a project file at project/f14r3.css … unchanged: the render needed no change to it"; the render-check is there. `f14r3.css` text matches the ask's, rule for rule. |
| S23 pager past one page | Q01, Q03-Q08, Q13, K02; Q09, Q10 | Done | `class="pager` on Q09 and Q10 only (each with "Showing 1–20 of 50" / "21–40 of 50", Previous, Next, "Requests per page"); 0 on the others. |
| S24 long reason | M17 | Done | Reason holds "‹a reason of 4,000 characters, wrapping in the box›", counter "4,000 of 4,000 characters"; the Spec says it was measured in its column at 390. |

## N1-N18

| Item | Board(s) | Status | Evidence |
|---|---|---|---|
| N1 PseudonymSalt | D10; Spec 2 | Done | "Nothing has been erased." (bold) "Erasure is not set up on this server. The request is still Submitted. Ask whoever runs the server to set it up, then approve again." ApproveDataRightsRequest row: the exception names Wombat__PseudonymSalt, in the log. |
| N2 may-download flag | Spec 2 | Done | DataRightsAuthorization row: "may-download's only consumer is Decide's 'You cannot download it yourself.' … the Administrator gets no Download". D01 shows the sentence. |
| N3 reminder save | Spec 2, 3, 4 | Done | Both "Nothing changed: …" sentences (NEW, PROPOSED); T318 row names UpdateObjectionFlagsCommand, IObjectionFlagUpdater, GetObjectionFlagsQuery, ObjectionFlagsDto, ObjectionFlagService and the executor's write. |
| N4 two in one minute | M17; Q (Review names) | Done | M17's two Withdraws `aria-label="Withdraw the Export request made 2026-10-28 ‹hh:mm:ss› SAST"`; Spec: the queue's Review names the same way (T6). The two labels read alike only because both seconds are bracketed. |
| N5 SAST date | Spec 4 | Done | DATA: "The pseudonym and ErasedOn's SAST date". |
| N6 user ids | D04, D05, C01-C07, C10; C08, C09 | Done | D04 6b844c8a-2a79-4f85-a118-28a321709844; D05 and C01-C07, C10 d04f4dfb-f855-4345-9efa-edd168d5a2b6 (one each, scripted); C08, C09 ‹user id›. |
| N7 Illustrations not the cast's | M17, C09, Q09, Q10, M16, C04, C10, D19 | Done | M17 top bar ‹name›; C09 requester ‹name›; Q09, Q10 ‹name›@‹domain› under ‹institution›; chips: M16 "fails A.1.2's submit", C04 "refuses A.1.8a's record, before the corrective save", C10 "fails A.1.8a's record, after the corrective save", D19 "Pieter Smit as the decider of ‹name›'s request". |
| N8 ratified review's number | D07 | Done | "the ratified review #5, closed check-in #6, the activities, progress and audit entries, and this request." |
| N9 Dr and Mr | steps | Done | Kept; the steps boards say they are the runbook's titles. |
| N10 captures | Spec | Done | First section: 8 opened, listed by name, with the contradictions; "all 18 staged captures are now read". |
| N11 contrast | Spec 3 | Done | 5.4:1, 4.86:1 and 4.61:1 as asked. |
| N12 download names | M04; Spec 3, 4 | Done | M04 `aria-label="Download the export of the Export request made 2026-10-28 00:01 SAST"`; Access form (NEW, PROPOSED) in the Spec; each starts with the visible word. |
| N13 pseudonym's second line | K01, D09, D22 | Done | Requester dd is `.person-cell` "deleted_user_0ae1d64b" over "Erased 2026-10-28" (D22 ‹pseudonym› over "Erased ‹date›"). |
| N14 id crumb | D17, D18 | Done | Home › Data rights requests › 01a17f61-f16f-77c1-860c-661586d5b733 at both widths; the 390 twins carry the parent link. |
| N15 h1 tabindex | M15, Q12, D18 | Done | `<h1 tabindex="-1">` on all three (and not on D17, U01). |
| N16 validated on press | Spec 3 | Done | "No field carries required"; 0 `required` attributes. |
| N17 describedby order | M02, M13, D02; Spec 3, 4 | Done | "reason-err reason-count", "note-err note-h" (the Reason has no help of its own); Spec 4 amends § Form page (:1277) and § Form system's counter line (:1435), PROPOSED. |
| N18 headings nothing focuses | all D, C | Done | `h2#s-dec` and `h2#s-cor` carry no tabindex on any board; results are `action-result` with `tabindex="-1"`. |

## T1-T8

| Item | Board(s) | Status | Evidence |
|---|---|---|---|
| T1 A.1.8a rewritten | steps-2 | Done | Route, Before, Do, Expect as the ask gave them, naming C01, C02, C03, M08, C05, C06, M09. |
| T2 the mail | steps-2 | Done | A.1.3, A.1.5, A.1.8, A.1.8a (two), A.1.9, A.1.12, A.1.13; "6 Stub emails" becomes 11 (10 if Complete request sends none: still open). |
| T3 word search | steps-2 | Done | Every phrase and line number of the ask, with the Gap each closes (F-A.1.1a, F-A.1.2a, F-A.1.9a, F-A.1.2c, F-A.1.3b, F-A.1.3c, F-A.1.12a, F-A.5.7a, F-A.7.5b); A.1.2's (86) stays. |
| T4 Outcome state | steps-2 | Done | "four" to five; tuples (0,5,1) (1,3,1) (1,4,1) (2,4,1) (4,4,1); audit Submit 5, Withdraw 1, Approve 3, Reject 1 and the two commands added; the SELECT loses OptOutOfOptionalProcessing; the two Handoff and shared-state changes; states.md and coverage.md "the build's". |
| T5 rest of the cast review's list | steps | Done | A row each for A.1.1, A.1.2, A.1.3, A.1.5, A.1.8, A.1.9, A.1.12, A.1.13, A.2.3, A.2.5, A.5.7, A.7.3, A.7.5; A.2.4, A.2.6, A.1.4, A.1.10, A.1.14 unchanged. |
| T6 re-pinning | steps-2 | Done | One line, including the Review-name rule (N4). |
| T7 journey counts | steps-2 | Done | Seven journeys, Today / 1280 / 390 / "Changed by F2 or F10?" each answered; the correction 5 + 2 and the name correction 2 + 1 as inferred. |
| T8 states.md rows | steps-2 | Done | M13-M18, Q02, Q09-Q12, D10-D22, C04, C07-C11, K02, U02: exactly the ask's list. |

## A1-A5

| Item | Board(s) | Status | Evidence |
|---|---|---|---|
| A1 primary on top | D, C, Q at 390; dialogs | Done | DOM order is secondary first in every `.form-actions` (Reject, then Approve; Cancel, then Erase and approve); `.user-page .form-actions` and `.dialog-card .form-actions` are `column-reverse` in bundle.css; the queue's Show precedes Clear filters inside `.filter-actions`. Spec 3 A1 states it. |
| A2 form parts | Spec 2 | Done | "Where each form's parts are held": Make a request, Reminder emails, Decide, Record a correction, each by its element. |
| A3 44 px at 390 | Spec | Done | "408 measured … 0 fell short of 44px." Reported, not re-measured. |
| A4 nothing scrolls sideways | Spec | Partly | The Spec reports 277 renders (201 + 8 + 68; the counts agree with the files: 67 / 8 / 68) and "scrollWidth equals its clientWidth, and no .table-container scrolls", the long ids, pseudonym, 60-character address and 4,000-character reason wrapping. The ask says "a table on the Spec"; the render-check is a bullet list, with no width-by-width table of the 1280, 1100, 768 and 390 results. Reported, not re-measured. |
| A5 no secret, no impossible pair | all | Done | No password or token drawn; D21 draws the refusal with the note kept and Submitted, D12 the Withdrawn page without Decide; no board draws two states that cannot hold at once. |

## Regressions against round 2

Every R3 board was diffed (markup tag stream and text, head and menu apart) against its R2 twin. Every change traces to an item. Nothing
round 2 had right was lost.

- **M (M01-M18):** `user-page data-rights-page` for `data-rights-page` (S21); the visually-hidden " required" (S20); roles, `clinic-table--stack`, `data-label` (M5, M6); M08/M09 names (M1); M15 `h1 tabindex` (N15); M17 reason and Withdraw names (S24, N4). **One unlisted change (not counted):** `h2#yr-h` ("Your requests") lost its `tabindex="-1"` on the M boards; the item list (N18) names only `s-dec` and `s-cor`. Harmless: the chips say the focus is "the result in Your requests", which is an `action-result` with its own tabindex.
- **Q (Q01-Q13):** pager gone below two pages (F10, S23); roles and labels (M5, M6); selects 44 px (M4); Q09/Q10 ‹name›@‹domain› (N7); Q05, Q08, Q13 names (M1); Q13 loses Clear filters (S18); Q02, Q11, U01, U02 unchanged in markup.
- **D (D01-D19):** `request-page` and the `s-dec` tabindex out (S21, N18); user ids (N6); D09 and K01 `.person-cell` (N13); D10 words (N1); D13's subtitle names KGK where R2 had ‹institution›, with the forecast lines (S5, M2); D17, D18 crumb and `h1` (N14, N15).
- **C (C01-C10):** names (M1); `correction-records` out; legend required (S20); "Name the record in the field under it." (S11); C08's line (S2); C09 rebuilt (S11); user ids (N6).
- **K01:** pseudonym second line (N13).
- **No regression found** in: titles and widths (all `R3-` with width, none missing a twin), the 390 and 768 twins, nav menus, dialog titles and buttons, sentence wording that no item touched, the Illustration chips.

## Build notes

1. **ds/ not saved** (as flow 13's). Every board links `ds/wombat/tokens.css` and `ds/wombat/components/bundle.css`; `round-3/` holds neither (it holds `canvas.json` and `f14r3.css`; the canvas keeps both under `project/ds/wombat/`). The repo's `design/system/components/bundle.css` is the same 232,840 bytes. Copy `tokens.css` and `bundle.css` into `round-3/ds/wombat/` if the round must render offline. `support.js` is the canvas runtime's and is not a project file.
2. **A3 and A4 are reported, not re-measured.** The Spec says the brand mark did not load in the render (a fixed 32×32 box, moves nothing). Re-measure on the built pages in the replay.
3. **`f14r3.css` section 2 to `app.css`**, each with its class-table row: `.request-id`; `.data-rights-table tbody th[scope="row"]`; `.answer-cell`; `.correction-values` (and its one-column rule below 641px); `.dialog-body--scroll`; the two `.data-rights-page` phone rules (the reminder checkbox's 44px label row via `:not(.choice-group .form-check)`; a stacked row's `.btn` at the row's width); and the system rule `.search-container .form-select, .pager .form-select {min-height:2.75rem}` below 641px, for every list. Section 1 (`.anno`, `.pv-focus`, `.pv-overlay`) is canvas only and never ships.
4. **Brackets the build fills from the replay:** A.1.8a's ‹reason›, ‹note›, ‹request id›, ‹time›, ‹date› (on C01-C03, C05, C06, C07, C10, M08, M09 and the A.1.8a rows of Q05 and Q13); M17's ‹hh:mm›, ‹hh:mm:ss›, ‹date›, long note; C09's ‹before›, ‹after›, ‹recorder›; Q09/Q10's ‹name›, ‹domain›, ‹institution›; K02's ‹date›, ‹time›, ‹type›, ‹request id›; D13/D14/D19/D20/D21/D22's ‹name›, ‹roles›, ‹n›, ‹reviews›, ‹campaigns›, ‹requests›, ‹curriculum›, ‹address›, ‹pseudonym›; C11's ‹institution›; devadmin's ‹address› (no step prints it); ‹user id› on C08, C09.
5. **Code and data the boards need** (all PROPOSED on Spec 2 and 4): `GetContactsAsync` for the live name and email; `RequesterDisplayName` stored as "First Last"; `MyDataRightsDto`/`MyDataRightsRequestDto`; the queue's institution names and a Coordinator with no institution; the Programme start reads; `ApplyRectificationCommand` encoding, `Other:‹name›` (at most 194), a migration for `AppliedByUserId`; a migration for the `xmin` token on `DataRightsRequestConfiguration`; `ErasureExecutor` rewrites and `ErasureOutcome`; the forecast by scope; the last-Administrator refusal; `AccountNotices.SendToAddressAsync` and a new `DataRightsDecisionEmail` template; the T318 removals and the `OptOutOfOptionalProcessing` column (a migration); may-decide, may-record and may-download from `CanReview` and `CanRectify`; PseudonymSalt's words; ErasedOn's SAST date. Tasks named: T388 (My account's name form for an institutional sign-in), T389 (the expiry job's lock gap).
6. **Still open:** whether Complete request sends a mail (T2's 11 or 10 stub emails; T316); the disabled name radios on C11 are the round's inference (S7), the operator may correct them.
7. **DESIGN.md amendments (PROPOSED, Spec 4):** § Form page (:1277) and § Form system's counter line (:1435), refusal first in `aria-describedby`; § Table system (the Answer column, `th scope="row"` with DataTable's roles, every stacked cell labelled, the pager only past one page, captions with the filter's tail); § Button system's destructive action with F5's counted line; § Alerts and § Detail page's live requester name; § Badges (Approved amber, Withdrawn `badge-withdrawn`); § Page-level patterns' Unavailable for a null read only.
8. **states.md rows (T8)** and the cast review's coverage.md rows are the build's; `badge-accepted` and `BadgeFor.DataRightsRequest` (Approved to `badge-accepted`, Withdrawn to `badge-withdrawn`, UnderReview gone) are a rename plus one removal.
9. **Two small things the build may settle:** M17's two Withdraw labels differ only by the seconds, which are bracketed; and `h2#yr-h` has no tabindex on the M boards (see Regressions), which suits a result that focuses its own `action-result`.
10. **Layout:** the round is saved flat in `round-3/` (flow 13 used `round-3/project/`); the files are as published, with `canvas.json` for the page `r3` titles.
