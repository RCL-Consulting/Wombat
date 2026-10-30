# Flow 14 round 3: the text of every board

Generated from `round-3/R3-*.dc.html` (T387, build-contracts § 0) by a script, as flow 13's was: style, script, svg, head and helmet blocks stripped; headings, labels, aria-labels, placeholders, input values, alt text and annotations kept. Board order is the canvas's (`canvas.json`, page `r3`, top to bottom, then left to right). Each board's head (its canvas title, then the `anno` line under it) is the designer's annotation, quoted after `>`. Design data: a caption, a chip or a `DATA:` note is never a shipped word. A table row is one `| a | b |` line; a `dt` opens with `**`.

######## BOARD R3-0-spec.dc.html — R3-0-spec · the screenshots, the stylesheet, the render-check, F1–F10, M1–M6
### R3-0-spec · flow 14 · round 3
The screenshots, the stylesheet and its render-check, the items F1–F10 and M1–M6.
#### The screenshots (first)
- Not attached: nothing came with the message (the uploads folder held only round 2's staged copies). Read from the folder C:\dev\Wombat\design\upload\baseline\act-A, granted to the session in round 2.
- Opened this round (8): A.1.2-1-export-no-reason-refused, A.1.3-3-request-detail-1280, A.1.5-1-dlamini-completed-download, A.1.6-2-duplessis-withdrawn, A.1.8-2-mahlangu-rectification-rejected, A.1.12-3-queue-after-erasure, A.1.13-1-ndlovu-session-ended, A.1.14-4-checkin-withdrawn. With round 2's ten, all 18 staged captures are now read.
- Words a capture shows that a board contradicts: none that the decisions do not intend. Each difference is a decided change, listed so it is seen: A.1.2-1 puts "Please provide a reason for your request." in a banner at the page's top over "No requests / You have not submitted any data rights requests yet." (M02 puts it under Reason; M01 has round 1's empty words). A.1.3-3 and A.1.8-2 title the page "Data rights request" with Back to list, Requester the address, times in UTC ("2026-10-27 22:01:54") and Decided by the bare id (D01–D03, D05: the h1 by type and person, SAST, the name over the id). A.1.5-1 has Date/Type/Status and Download, no answer (M04 adds the Answer). A.1.6-2: "Request withdrawn." at the top, the row Withdrawn with nothing offered (M06 agrees; the result now heads Your requests). A.1.12-3: the erased row still reads ndlovu@kgk.wombat.local, Completed (Q07: deleted_user_0ae1d64b over "Erased 2026-10-28", F3).
- Confirmed by a capture: Nomsa Mahlangu's user id d04f4dfb-f855-4345-9efa-edd168d5a2b6 (A.1.8-2), as FIXTURES now gives it (N6); Withdrawn already grey; A.1.12-3 shows four rows before A.1.8a, so five follows it. A.1.13-1 and A.1.14-4 are other flows' pages (session ended; the withdrawn check-in): nothing drawn from them.
#### The stylesheet (S22)
f14r3.css is a project file at project/f14r3.css, written with the text given, unchanged: the render needed no change to it. Every R3 board's head links ds/wombat/tokens.css, ds/wombat/components/bundle.css, then f14r3.css, and no /_blob/ stylesheet; each board's <style> is body{margin:0} alone. Section 1 (canvas only): .anno, .pv-focus, .pv-overlay. Section 2: the NEW rules of the class table (R3-0-spec-2). Round 2's two /_blob/ sheets are not linked by any R3 board.
#### Render-check (A3, A4)
- How: every R3 board rendered in Chromium (Playwright) from the project's files (tokens.css, bundle.css v37 with its fonts, f14r3.css), outside the canvas's dc runtime: the HTML as written. The brand mark is a canvas asset and did not load there; it is a fixed 32×32 box and moves nothing.
- 143 boards, 277 renders: the 67 boards at 1280 each at 1280, 1100 and 768 (201); the 8 boards at 768 at 768; the 68 boards at 390 at 390.
- Controls at 390: 408 measured (every visible .btn, .form-control, .form-select, radio and checkbox label row, the breadcrumb's parent link, the pager's Previous, Next and page-size select, the dialog's buttons). 0 fell short of 44px. M4's rule is what brings the queue's Type and Status and Requests per page to 44px.
- Nothing scrolls sideways at any width: in all 277 renders the page's scrollWidth equals its clientWidth, and no .table-container scrolls; the request ids, user ids, deleted_user_0ae1d64b, the 60-character addresses (Q09, Q10, D19) and the 4,000-character reason (M17, D19) wrap in their cells and dd's.
#### F1–F10, where each is drawn
The operator's decisions of 2026-10-30
| Decision | Drawn |
| F1 the live name | M1's boards; the queue's and request's names; Spec code rows (R3-0-spec-2) |
| F2 Mahlngu played | R3-C01–C04, C07, M08 "Nomsa Mahlngu"; C05, C06, C10, M09 and the queue "Nomsa Mahlangu"; T1 |
| F3 every request rewritten | R3-K02 (new), R3-D13's other-requests line; code rows |
| F4 the erasure's mail | Spec only (code rows, T2) |
| F5 the forecast's scope | R3-D13 (Pieter Smit, counted line); D07, D14 devadmin's |
| F6 an institutional sign-in | R3-C11 (new) |
| F7 the expiry notice | Spec only (S9); no board changes |
| F8 Withdrawn first, xmin | R3-D12's chip; code rows |
| F9 the Administrator's own; the last Administrator | R3-D20, D21 (new) |
| F10 the pager past one page | only R3-Q09, Q10 draw it |
#### M1–M6
Must fix
| Item | Done, where |
| M1 the name | Done. "Nomsa Mahlngu" on R3-C01, C02, C03, C04, C07 (tab, h1, Requester, the three record lines, C07's "Changed on Nomsa Mahlngu's programme page." and link "Open Nomsa Mahlngu's programme", the closing "… on Nomsa Mahlngu's My data rights.", C01's result) and R3-M08 (top bar). "Nomsa Mahlangu" on C05, C06, C10, M09 and the queue's Mahlangu rows (Q05, Q08, Q13). Each C board's chip says before or after the corrective save. The rule: the queue's Requester, the request's h1, tab, Requester and every sentence naming the requester read GetContactsAsync's FirstName and LastName at load; RequesterDisplayName only when the account is erased (then the pseudonym); submission stores "First Last". |
| M2 the erasure's reach | Done. R3-D13's "‹n› other open data rights requests: ‹requests›. Each is withdrawn." (NEW); D07 has no such line (Sipho Ndlovu had none). R3-K02 new. Code rows. |
| M3 the mail | Spec only: code rows (R3-0-spec-2); T2 (R3-0-steps-2). |
| M4 390 selects | Done by f14r3.css's "list selects at 390"; measured 44px on every R3-Q 390 board that draws them (Q13 included); class table row; accessibility line. |
| M5 DataTable's roles | Done: Your requests, the queue, K02 and Corrections recorded write clinic-table--stack clinic-table--stack-wide data-rights-table, role=table/rowgroup/row/columnheader/rowheader/cell, at every width. The roles are DataTable's: a row laid out as a grid below 900px loses a table's semantics without them. |
| M6 every stacked cell labelled | Done: data-label on Status's badge cell and every text td (Submitted, Type, Status, Answer, Institution, Request id, Value before, Value after, Recorded); the row header and the actions cell carry none. |

######## BOARD R3-0-spec-2.dc.html — R3-0-spec-2 · code changes, the class table, form holders
### R3-0-spec-2 · the code changes, the class table
Every change written as code, each PROPOSED; restated whole.
#### Code changes (each PROPOSED)
Code changes the boards need
| Where | Change | Items |
| SubmitDataRightsRequest | Store RequesterDisplayName as "First Last" (today ClaimTypes.Name, the address: SubmitDataRightsRequest.cs:49-51). The validator refuses Objection. | F1, Q8 |
| The name rule (queue, request page) | Read IUserAdministrationService.GetContactsAsync (FirstName, LastName, Email) for the requester ids on the page at load; RequesterDisplayName only when the account is erased (it then holds the pseudonym). Every sentence that names the requester uses the live name. | F1, M1, S1 |
| GetMyDataRightsRequestsQuery | Returns MyDataRightsDto (InstitutionName, null for none; Requests: MyDataRightsRequestDto[]). MyDataRightsRequestDto (Id, RequestedOn, Type, Status, DecidedOn, DecisionNote, CompletedOn, CorrectedRecords), for My data rights only; never DecidedByUserId. CorrectedRecords by one grouped read of DataRightsRectifications. DataRightsRequestSummaryDto (the queue's) unchanged in what it carries of a decision. | S3, S10, T319 |
| ListDataRightsRequestsQuery | The live name and email; the institution's name for the Administrator ("None" without one); the caller's institution name for the subtitle, caption and empty sentence; PagerControls at 20 of 10/20/50/100, PageSizeLabel "Requests per page", shown only when MatchCount > PageSize; statuses without UnderReview, types without Objection. A Coordinator with no institution: subtitle "Requests about personal data: decide each, and carry an approved correction through.", empty card "No requests" / "Your account is not set to an institution, so no request is yours to decide." (NEW, PROPOSED; no board). | S4, S12, F10, Q8 |
| RequestsList.razor | The constant 25 and the hand-built pager go; Show and Clear filters are new handlers; the filter bar is the search-container / search-field filter-actions pattern (DecisionsDue.razor:44), not a component; Clear filters drawn only once a filter is set. | S12, S18 |
| GetDataRightsRequestByIdQuery | Returns null for an unknown and a foreign id alike (no throw); the page draws Request unavailable only for null; the page's catch maps every exception to "Could not load this request." with Try again (StatePanel LoadError and OnRetry, new here; FocusAfterRetry to the h1), never ex.Message. Carries the decider's name, the requester's live name, email and institution, the corrections with their recorder, may-decide, may-record and may-download, the pseudonym and ErasedOn's SAST date. | S14, N2, N5, F9 |
| DataRightsAuthorization | RectificationRoles folds into ReviewRoles (the Coordinator at the request's institution and the Administrator; Speciality admin dropped). CanAct refuses the requester, the Administrator included (Q21). The page's may-decide and may-record come from CanReview and CanRectify as changed, never from roles. may-download's only consumer is Decide's "You cannot download it yourself." (shown when false); the Administrator gets no Download on the request page. | Q1, Q21, F9, N2 |
| ApplyRectificationCommand(RequestId, TargetType, TargetId, FromValueJson, ToValueJson, Principal) | TargetType Account.FirstName, Account.LastName, TraineeProfile.ProgrammeStart, or Other:‹name› (at most 200, so the name at most 194). TargetId the requester's user id (a Guid) for every record: a recorded Programme start does not link back to the profile (its id is an int). Values JsonSerializer.Serialize(string), read back with Deserialize<string>, compared canonical-to-canonical (CLAUDE.md's jsonb rule). "Enter the value before." and "Enter the value after." are the page's checks: the validator's NotEmpty() passes the JSON of an empty string. A recorder column, AppliedByUserId, on DataRightsRectification (a migration). | S11, M5 |
| The Programme start reads | The requester's preferred profile id (an int, TraineeScopeResolver.PreferredProfiles), whether it has ended, whether the caller may open it (the Administrator, or an Institutional admin of its institution), whether a profile exists (none: the radio is not offered). C07 links /admin/trainees/edit?id=5; C08 reads flow 13's TraineeProfileWords.EndedLine. | S2 |
| The sign-in methods | GetSignInMethodsAsync: a requester with an institutional sign-in gets the name radios disabled with "This name comes from ‹institution›'s sign-in; the institution corrects it."; ‹institution› is the provider's DisplayName (AccountSignInMethods.InstitutionalSignIns); such a correction records as Other. ExternalLoginHandler rewrites both names at every sign-in (:540-547). My account's name form for such a person: T388. | F6, S7 |
| CompleteRectificationRequestCommand | "Record at least one correction before completing the request." for "At least one rectification must be applied before completing the request."; whether it sends Q5's notice (still open: R3-C06 says it does). | T316 |
| DataRightsRequest | A concurrency token (xmin, as CommitteeReviewConfiguration.cs:41) on DataRightsRequestConfiguration. Approve and Reject check Withdrawn first and say "‹name› withdrew this request. Nothing was decided or erased." (never "Only submitted or under-review requests can be approved."); a lost race reads the row again. Decided meanwhile: "This request was decided while this page was open. Nothing more was done." (NEW, PROPOSED; no board). Withdraw() refuses all but Submitted. | F8, S13, Q14 |
| The erasure forecast | A query gated as the command: open committee reviews at any institution or panel (at the caller's institution named, elsewhere counted: "and 1 open review at another institution"; the Administrator's names all), unreleased MSF campaigns, the person's other open data-rights requests, an active profile and its curriculum, a staff role's roles and scopes. | F5, S5, M2, S11 |
| ErasureExecutor | Rewrites RequesterDisplayName to the pseudonym on every request whose RequesterUserId is the person's; DecidedByUserId and AppliedByUserId to the pseudonym where the person decided or recorded; withdraws the person's other open requests in the same save. RequesterUserId stays (DataRightsErasureRecord pairs UserId, RequestId, Pseudonym). The completeness test covers the three columns. ExecuteAsync returns ErasureOutcome (Pseudonym, ReviewsWithdrawn, CampaignsWithdrawn, RequestsWithdrawn, ProfilesEnded; NEW), and ApproveDataRightsRequestCommand returns the request with it. | F3, M2, S6 |
| ApproveDataRightsRequest | Refuses the erasure of the last Administrator before ExecuteAsync. Reads the requester's address with GetByIdAsync before ExecuteAsync, and after the commit sends DataRightsDecisionEmail with no link through AccountNotices.SendToAddressAsync(address, kind, build) (NEW): SendAsync(userId) skips a deactivated or address-less account (AccountNotices.cs:60-66), which an erased one is. The exception at ApproveDataRightsRequest.cs:76 names Wombat__PseudonymSalt for the operator, in the log; the page maps it to N1's words. | F9, S8, F4, M3, N1 |
| AccountNotices | The class comment ("what happened, when and by whom") names the data-rights exception: the decision mail names no decider. DataRightsDecisionEmail is a NEW template file under Common/Email/Templates (type, date, outcome, a link to My data rights; none for an erasure). | F4, M3, Q5 |
| T318 | UpdateObjectionFlagsCommand, IObjectionFlagUpdater, GetObjectionFlagsQuery, ObjectionFlagsDto and ObjectionFlagService lose OptOutOfOptionalProcessing; the column goes (a migration) and the executor's write of it. The reminder save's result names the state saved from the state before (N3). | Q6, N3 |
| Q27 | The digest help's sentence stays; the build adds the expiry notice to its examples when it checks the list against EntrustmentDecisionExpiredEmail and EntrustmentDecisionExpiringReminderEmail. The expiry job's lock gap (ReminderRecipientPolicy, "A known gap"): T389. | F7, S9 |
| BadgeFor.DataRightsRequest | Approved → badge-accepted; Withdrawn → badge-withdrawn (grey already: a rename); UnderReview gone. | Q22 |
#### Class table (every class not in bundle.css v37)
The class table
| Class | Kind | What it adds |
| request-id | NEW | An id in its own cell or dd: monospace, breaks anywhere. |
| data-rights-table | NEW | The three tables' th scope="row" reads as a cell (transparent, 600). |
| answer-cell | NEW | The requester's Answer wraps a 4,000-character note. |
| correction-values | NEW | Value before and after side by side; one column below 641px. |
| dialog-body--scroll | NEW | The erasure dialog's body scrolls inside the dialog. |
| data-rights-page | NEW (two phone rules only) | On <div class="user-page data-rights-page">: the reminder checkbox's 44px label row (.user-page gives one to a radio only); a stacked row's Download or Withdraw the row's width. |
| list selects at 390 | NEW, a system rule | Every list's filter selects and PagerControls' page-size select 44px below 641px (.search-container .form-select, .pager .form-select), beside .filter-actions' phone rule. |
| request-page, correction-records, the old .data-rights-page column | OUT (S21) | The request and correction pages are <div class="user-page">: .user-page's own 44px and stacked actions serve them. |
| user-page, user-section, person-cell, choice-group, clinic-table--stack, clinic-table--stack-wide | reused as shipped | Never redeclared. |
| anno, pv-focus, pv-overlay | canvas only (f14r3.css § 1) | The chip, the frozen focus ring, the painted modal stage. |
#### Where each form's parts are held (A2)
- Make a request: its .user-section card (flex column, gap 16px): the intro p, fieldset.choice-group, the Reason .form-group, the next-step p, .form-actions.
- Reminder emails: its .user-section card: the intro p, a .form-group holding the .form-check and its help, .form-actions.
- Decide: its .user-section card: the help p, the note's .form-group, .form-actions.
- Record a correction: the Correction .user-section card: fieldset.choice-group, .correction-values (its own grid gap), .form-actions; Complete request its own p and .form-actions under it.

######## BOARD R3-0-spec-3.dc.html — R3-0-spec-3 · S1–S24, N1–N18, A1–A5, Illustrations, brackets
### R3-0-spec-3 · S1–S24, N1–N18, A1–A5
Each item, done or Spec only, with where.
#### Should fix
Should fix
| Item | Done, where |
| S1 email | The request page's Email and the queue's second line read GetContactsAsync's Email; none once erased: R3-K01, K02, D09, D22 draw no address. DATA row. |
| S2 programme start | R3-C07's link href="/admin/trainees/edit?id=5"; R3-C08 "This programme has ended, so its record is archived and cannot be changed." (no task id, no link). DATA row; TargetId stays the user id. |
| S3 MyDataRightsDto | Code row; the institution's name stands on M01, M11, M18 with no request. |
| S4 the queue's institution | Code row; the no-institution Coordinator's words, no board. |
| S5 the forecast's scope | R3-D13 redrawn: Pieter Smit's menu and top bar; It ends: account, roles, "‹n› open committee reviews: ‹reviews at Kgosi Kgari Teaching Hospital›, and 1 open review at another institution. Each is withdrawn, and nothing more is decided at it." (NEW), campaigns, other requests, programme. D07, D14 stay devadmin's. |
| S6 ErasureOutcome | Code row; R3-D09's sentence kept; R3-D22 new: "The request is approved and completed. Nothing else was open to end." (NEW). |
| S7 institutional sign-in | R3-C11 new: First name and Last name disabled, each "This name comes from ‹institution›'s sign-in; the institution corrects it."; reading them as disabled is this round's inference. |
| S8 the Administrator's own; the last Administrator | R3-D20 new ("You cannot decide your own request: another Administrator decides it."; subtitle without the institution). R3-D21 new (the refusal in Decide's ActionResult, focused, note kept, Submitted). |
| S9 expiry notice | Spec only (code row Q27); Q27's sentence stays on every M board; T389. |
| S10 MyDataRightsRequestDto | Code row. |
| S11 the record's encoding; Other's name | Code row; Other's line "Name the record in the field under it." on every C board with the form; R3-C09 draws Other chosen with "Record's name" holding ‹the record's name›, its rows "Other › ‹record›" over "‹before›". M09's answer names an Other record "Other › ‹name›" the same way (the cast's names Account › Last name). |
| S12 the queue's handlers and words | Code rows; the caption tails and empty sentences in the messages table. |
| S13 Withdrawn first | R3-D12's chip; code row. |
| S14 load outcomes | R3-D17 (loading: the read not back), D18 (the read threw), U01, U02 (the read returned null): each chip says which. |
| S15–S17, S19 | R3-0-steps, R3-0-steps-2 (T1–T8). |
| S18 Clear filters once set | R3-Q13 has no Clear filters (defaults) and no pager; Q02, Q03, Q06, Q07 draw it, a 44px block under Show at 390. |
| S20 required groups | Both legends read the aria-hidden star then <span class="visually-hidden"> required</span> (M boards with the form; C boards with the form, C11 included). |
| S21 the NEW table cut | My data rights is user-page data-rights-page; the request and correction pages user-page; .request-page, .correction-records out. |
| S22 the stylesheet | R3-0-spec, and the render-check there. |
| S23 pager past one page | Only R3-Q09, Q10 draw a pager. |
| S24 the long reason | R3-M17's Reason holds ‹a reason of 4,000 characters, wrapping in the box›, counter "4,000 of 4,000 characters"; measured in its column at 390. |
#### Nits
Nits
| Item | Done, where |
| N1 | R3-D10: "Nothing has been erased." (bold) "Erasure is not set up on this server. The request is still Submitted. Ask whoever runs the server to set it up, then approve again." |
| N2 | Spec only (code row DataRightsAuthorization). |
| N3 | Spec only: "Nothing changed: reminder emails are still stopped." / "Nothing changed: reminder emails are still sent." (NEW, PROPOSED). |
| N4 | R3-M17's two Withdraws "Withdraw the Export request made 2026-10-28 ‹hh:mm:ss› SAST", in place of "the later / the earlier of two that minute"; the queue's Review names the same way (T6). |
| N5 | DATA row: "Erased 2026-10-28" is ErasedOn's SAST date (ProgrammeCalendar.DateOf). |
| N6 | R3-D04 6b844c8a-2a79-4f85-a118-28a321709844; R3-D05, C01–C07, C10 d04f4dfb-f855-4345-9efa-edd168d5a2b6; C08, C09 ‹user id›. |
| N7 | R3-M17 (top bar ‹name›) and C09 (the requester ‹name›) bracket the person; Q09, Q10 ‹name›@‹domain› under ‹institution›; M16, C04, C10, D19's chips name the step's state they fail, refuse or show. |
| N8 | R3-D07: "the ratified review #5, closed check-in #6, the activities, progress and audit entries, and this request." |
| N9 | Kept: "Dr Mahlangu", "Mr Smit" on the Steps boards are the runbook's titles. |
| N10 | R3-0-spec, first section. |
| N11 | badge-accepted's text is --text-color on --warning-bg, its border --warning-color (5.4:1 on the tint); the focus ring rgb(45 108 223) is 4.86:1 on white and 4.61:1 on the page background. Both pass. |
| N12 | Download names: Export "Download the export of the Export request made ‹date› ‹time› SAST" (built); Access "Download the copy of your data for the Access request made ‹date› ‹time› SAST" (NEW, PROPOSED). Each starts with the visible word. |
| N13 | R3-K01, D09 (and D22): Requester is the .person-cell "deleted_user_0ae1d64b" over "Erased 2026-10-28". |
| N14 | R3-D17, D18: Home › Data rights requests › 01a17f61-f16f-77c1-860c-661586d5b733; at 390 the parent link. |
| N15 | R3-M15, Q12, D18: the h1 carries tabindex="-1". |
| N16 | No field carries required: the pages validate on press, as built. |
| N17 | aria-describedby: refusal first, then help, then counter ("reason-err reason-count", "note-err note-h"); DESIGN.md § Form system's counter line (:1435) and § Form page (:1277) amended to say so, PROPOSED. |
| N18 | h2#s-dec and h2#s-cor carry no tabindex on any board; results focus their ActionResult. |
#### Also (A1–A5)
- A1: stacked actions primary on top (.user-page .form-actions column-reverse; the queue's Show over Clear filters); the dialog at 390 the action on top (.dialog-card .form-actions).
- A2: R3-0-spec-2, the form holders.
- A3, A4: the render-check, R3-0-spec.
- A5: no password or secret drawn; each board one state.
#### Illustrations
M13, M16, M17, M18; Q09, Q10; D10, D11, D12, D13, D14, D15, D16, D19, D20, D21, D22; C04, C07, C08, C09, C10, C11; K02; U02. Each says so in its canvas title and as the bold lead of its chip.
#### Brackets kept, and why
- A.1.8a's ‹reason›, ‹note›, ‹request id›, ‹time›, ‹date›: the step is NEW and not yet played; the replay that plays it pins them (T6).
- The Illustrations' ‹name›, ‹institution›, ‹roles›, ‹n›, ‹reviews›, ‹campaigns›, ‹requests›, ‹curriculum›, ‹pseudonym›, ‹address›, ‹domain›, ‹type›, ‹hh:mm:ss›: no cast member is in those states.
- ‹user id› on C08, C09 (bracketed people); devadmin's ‹address› (no step prints it).

######## BOARD R3-0-spec-4.dc.html — R3-0-spec-4 · DESIGN.md, messages, DATA, edge cases
### R3-0-spec-4 · DESIGN.md, messages, DATA
The text each change touches, every message by kind, the reads the build must add.
#### DESIGN.md text each change touches
- § The NavMenu's owner table: unchanged.
- § Form page: described radios with the visually-hidden " required" on the legend; the reminder section's own result (N3's two unchanged-state words); refusal first in aria-describedby (N17), PROPOSED (:1277).
- § Form system: the counter line (:1435) reads refusal, then help, then counter (N17), PROPOSED.
- § Detail page and the title rule: a request by type and person, the live name (F1); one column of h2 sections on .user-page; Decided by the name over the id; the pseudonym's .person-cell (N13).
- § Table system: the Answer column; the Request id column; th scope="row" with DataTable's roles (M5); every stacked text cell labelled (M6); the Administrator's Institution column; the pager only past one page (F10); captions with the filter's tail.
- § Button system's destructive action: Approve and erase… and its dialog, with F5's counted line for another institution.
- § Alerts: the kept notice standing; the live requester name in every result (F1).
- § Badges: DataRightsRequest Approved amber, Withdrawn badge-withdrawn.
- § Page-level patterns' Unavailable: the request's, for a null read only (S14).
- § Content's ids rule: ids on the queue and the request, never on My data rights.
#### Messages by kind
Every message by kind
| Kind | Words | Source; NEW beside what it replaces |
| Result | "Request submitted. You will be notified when it is reviewed." · "Request withdrawn." · "Reminder emails stopped." / "Reminder emails will be sent again." | built; round 1's (for "Processing preferences saved.") |
| Result | "Nothing changed: reminder emails are still stopped." / "Nothing changed: reminder emails are still sent." | NEW, PROPOSED (N3); replaces "Processing preferences saved." on an unchanged save |
| Result | "Request approved." + "Anele Dlamini is emailed, and can download the export from My data rights." · "Request rejected." + "Nomsa Mahlangu is emailed, and reads your note on My data rights." · "Request approved. Nomsa Mahlngu is emailed. Record each correction below, then complete the request." · "Correction recorded: Account › Last name." · "Request completed. Nomsa Mahlangu is emailed, and reads it as Completed on My data rights." | round 2's; M1's spelling before the save |
| Result | "The personal data is erased." (bold) "The request is approved and completed." then the clauses present ("‹n› committee review(s) was/were withdrawn", "‹n› MSF campaign(s) …", "‹n› other request(s) …", "the programme ended today"), then "The account is now ‹pseudonym›; its open session ends within a minute."; with none "Nothing else was open to end." | round 2's sentence; NEW (S6) the clause rule and the none sentence |
| Refusal | "Please provide a reason for your request." · "A decision note is required." · "Record at least one correction before completing the request." · "Choose the record." / "Enter the value before." / "Enter the value after." · "Name the record." / "Name the record in 194 characters or fewer." | built; round 1's; round 2's; NEW, PROPOSED (S11) |
| Refusal | PseudonymSalt: "Nothing has been erased. Erasure is not set up on this server. The request is still Submitted. Ask whoever runs the server to set it up, then approve again." | NEW (N1), replacing round 2's "… its pseudonym key (PseudonymSalt) is not configured …" |
| Refusal | "‹name› withdrew this request. Nothing was decided or erased." · "This request was decided while this page was open. Nothing more was done." · the concurrent-change refusal | round 2's with the live name; NEW, PROPOSED (S13); built |
| Refusal | "Nothing has been erased. ‹name› is Wombat's only Administrator, and erasing the account would leave nobody to run it. Add another Administrator first, then approve again." | NEW, PROPOSED (S8) |
| Line | "You cannot decide your own request: another Coordinator at Kgosi Kgari Teaching Hospital, or the Wombat administrator, decides it." · "You cannot decide your own request: another Administrator decides it." | round 2's; NEW (F9) |
| Dialog | "‹n› other open data rights requests: ‹requests›. Each is withdrawn." · "‹n› open committee reviews: ‹reviews at Kgosi Kgari Teaching Hospital›, and 1 open review at another institution. Each is withdrawn, and nothing more is decided at it." · "the ratified review #5" | NEW (M2, S5); round 2's "the ratified review" numbered (N8) |
| Hint | Record lines: "Changed by ‹live name› on My account. No staff page changes a name." · "Changed on ‹live name›'s programme page, which an Institutional admin at Kgosi Kgari Teaching Hospital opens; you cannot." · "Changed on ‹live name›'s programme page." + "Open ‹live name›'s programme" · "This programme has ended, so its record is archived and cannot be changed." · "This name comes from ‹institution›'s sign-in; the institution corrects it." · "Name the record in the field under it." + the field "Record's name" | round 2's with the live name; flow 13's built words (S2) for round 2's "(T305)" line; NEW (F6); NEW (S11) for "Name the record in Value before." |
| Empty | "No ‹Type› request at ‹institution›." · "No ‹Type› request is ‹Status› at ‹institution›." · "No request is ‹Status› at ‹institution›." · the Administrator's "… at any institution.", each + "Clear filters to see every request." · the no-institution Coordinator's "No requests" / "Your account is not set to an institution, so no request is yours to decide." | NEW (S12, S4) |
| Name | Caption tails ": Export, Submitted", ": Erasure", ": Completed" · "Withdraw the Export request made 2026-10-28 ‹hh:mm:ss› SAST" (two in a minute) · Download per type (N12) | NEW (S12, N4, N12), N4 for "the later / the earlier of two that minute" |
| Subtitle | The Coordinator with no institution: "Requests about personal data: decide each, and carry an approved correction through." · a request with no institution: "Submitted ‹date› ‹time› SAST" | NEW (S4); build rule (S8) |
| Failure, load error | As round 2 (each "Nothing has changed. Try again, or come back in a few minutes.") | round 2's |
#### DATA the build must add
- The live name and email from GetContactsAsync (F1, S1); "First Last" stored at submission.
- The institution names: My data rights (MyDataRightsDto.InstitutionName), the queue's caller, the Administrator's column (S3, S4).
- The Programme start reads: preferred profile id, ended, may-open, exists (S2).
- The forecast by scope, with the person's other open requests (F5, F3).
- The erasure's ErasureOutcome (S6).
- The recorder: AppliedByUserId (a migration), read with its name.
- may-decide, may-record, may-download from CanReview, CanRectify and the download policy (F9, N2).
- The pseudonym and ErasedOn's SAST date (N5).
- MyDataRightsRequestDto's fields (S10).
#### Edge cases
- A requester with no institution: R3-M18; the Administrator's "None" (Q09).
- A reviewer's own request: R3-D15; the Administrator's own: R3-D20.
- The last Administrator's erasure: R3-D21.
- A review under appeal: R3-D14. A staff role, a review elsewhere: R3-D13.
- The person's earlier requests: R3-K02. Nothing else open: R3-D22.
- An institutional sign-in: R3-C11 (T388 for My account's form).
- The two erasure refusals: R3-D10, D11. Withdrawn meanwhile: R3-D12. Decided meanwhile: Spec only.
- An export whose PDF fails: flow 20's, no board. An ended programme: R3-C08. Two in one minute: R3-M17. A 60-character address: R3-Q09, D19. A 4,000-character reason: R3-M17, D19. An approved erasure never read by its requester: no board.

######## BOARD R3-0-steps.dc.html — R3-0-steps · the 19 steps
### R3-0-steps · the 19 steps
Round 2's rows, with T2, T3 and T5 applied. Values from FIXTURES; "Dr"/"Mr" are the runbook's titles (N9).
#### The steps
The 19 steps and A.1.8a
| Step | Board | Changes (Expect, Do, Note, Actual or Gap) |
| A.1.1 Dr Dlamini | R3-M01 | Do: read the page; save nothing (Q28). Expect: Your requests ("No requests yet."), Make a request (four described radios), Reminder emails ("Stop the regular reminder emails", Q27's help); no optional box; the 23:59 note goes. Closes F-A.1.1a (T318). |
| A.1.2 Dr Dlamini | R3-M02, M03 | The refusal under Reason, Export kept; "Request submitted. You will be notified when it is reviewed."; the row 2026-10-28 00:01 SAST, "Waiting for a decision.", Withdraw named in SAST; the submission's "No mail" (86) stays. Closes F-A.1.2a (T319), F-A.1.2c (T325). |
| A.1.3 Mr Smit | R3-Q01–Q03, D01–D03 | Show after the filters (Apply filters goes, 107); the requester by name over the address (99); h1 "Export request from Anele Dlamini"; the refusal under the note; Decided by Pieter Smit over 43ad0cb7-…; a stub decision mail is logged (Actual's "No mail", 113, goes). Closes F-A.1.3b (T324: UnderReview, 113, 116), F-A.1.3c (T323). |
| A.1.4 | — | unchanged |
| A.1.5 Dr Dlamini | R3-M04 | The answer on the row; "No email is sent" (140) becomes "An email told Anele Dlamini of the decision at A.1.3; none is sent on download". |
| A.1.6 Dr du Plessis | R3-M05, M06 | SAST; "You withdrew it before a decision."; Withdrawn grey. |
| A.1.7 Dr Mahlangu | R3-M07 | SAST. |
| A.1.8 Mr Smit | R3-Q04, D04, D05 | Q22's badges ("badge-draft", 195, becomes badge-withdrawn); the rejected request with no Correction section; a stub mail for the rejection (198). |
| A.1.8a (NEW) | R3-C01–C06, M08, M09 | See R3-0-steps-2, T1. |
| A.1.9 Dr Mahlangu | R3-M09 | Two Rectifications; "no email told …" (206, 210) becomes "an email told Nomsa Mahlangu of each decision". Closes F-A.1.9a (T319). |
| A.1.10 | — | unchanged |
| A.1.11 Dr Ndlovu | R3-M10 | SAST. |
| A.1.12 devadmin | R3-Q05–Q07, D06–D09 | Five requests; the dialog with #5; "No confirmation is asked" (259) goes; "by the address" (260) becomes deleted_user_0ae1d64b over "Erased 2026-10-28"; one stub mail, the erasure's confirmation, no link, after the commit; the UTC time (267) in SAST. Closes F-A.1.12a (T276, T264). |
| A.1.13 | — | "No email is sent to …" (280, 284) becomes "The erasure's confirmation was sent at A.1.12; nothing more is sent." (T2) |
| A.1.14 | — | unchanged |
| A.2.3 Mr Smit | R3-M11 | Do: tick "Stop the regular reminder emails" (378, 380); Expect "Reminder emails stopped." (379, 381). |
| A.2.4, A.2.6 | — | unchanged: they read the log (the cast review checked them). |
| A.2.5 Mr Smit | R3-M12 | Do: untick it (402); Expect "Reminder emails will be sent again." (403, 405). |
| A.5.7 Mr Smit | R3-U01 | "Request unavailable", no alert; "Not found" (854, 859) and "Back to list" (859) go. Closes F-A.5.7a. |
| A.7.3 Dr Dlamini | R3-M04 at 390 | Your requests stacks, no inner scroll. |
| A.7.5 Mr Smit | R3-Q13 | The selects 44px; no Clear filters at the defaults; five KGK rows stacked; no pager; "Apply filters" (1419) goes. Closes F-A.7.5b (T328, 1421). |

######## BOARD R3-0-steps-2.dc.html — R3-0-steps-2 · T1–T8
### R3-0-steps-2 · T1–T8
A.1.8a rewritten, the mail, the word search, the Outcome, re-pinning, journeys, states.md rows.
#### T1 · Step A.1.8a (NEW, after A.1.8, before A.1.9)
Role: Trainee, Dr Nomsa Mahlangu; Coordinator, Mr Pieter Smit. Route: /account/profile → /account/data-rights → /admin/data-rights/{Id:guid} → /account/data-rights → /account/profile → /admin/data-rights/{Id:guid} → /account/data-rights. Before: none (no seeded name).
Do: Dr Mahlangu opens My account from the name in the top bar and saves Last name "Mahlngu" ("Name saved."); submits a Rectification with ‹reason›. Mr Smit approves it with ‹note› (R3-C01), reads the empty Correction section with its four records (R3-C02), presses Complete request with none recorded (refused, R3-C03). Dr Mahlangu reads the row: Approved, "Being corrected", no Withdraw, top bar "Nomsa Mahlngu" (R3-M08). Dr Mahlangu saves Last name "Mahlangu" on My account ("Name saved."). Mr Smit opens the request again (h1 now "Rectification request from Nomsa Mahlangu"), records Account › Last name, "Mahlngu" to "Mahlangu" (R3-C05), completes the request (R3-C06). Dr Mahlangu reads the row Completed, "Completed on ‹date›. Corrected: Account › Last name." (R3-M09).
Expect: each word from those boards; the mails of T2.
#### T2 · The mail
- A.1.3: a stub decision mail logged for the Export's approval (Actual's "No mail" goes).
- A.1.5: "An email told Anele Dlamini of the decision at A.1.3; none is sent on download".
- A.1.8: a stub mail for the rejection. A.1.8a: two (the approval, the completion). A.1.9: "an email told Nomsa Mahlangu of each decision".
- A.1.12: one stub mail, the erasure's confirmation, with no link, logged after the commit. A.1.13: "The erasure's confirmation was sent at A.1.12; nothing more is sent."
- The Outcome's "6 Stub emails" becomes 11 (10 if Complete request sends none: still open).
#### T3 · The word search (appendix-cross-cutting.md line numbers, as the message gives them)
Words that change
| Words | Hits | Gap closed |
| "Processing preferences saved." | A.1.1 (Expect, Actual), A.2.3 (379, 381), A.2.5 (403, 405) | F-A.1.1a |
| "Opt out of digest emails" | A.2.3 (378, 380), A.2.5 (402) | — |
| "optional processing" | A.1.1 (Do, Note, Gap F-A.1.1a), the Outcome (1746, 1761, 1810) | F-A.1.1a (T318) |
| "Apply filters" | A.1.3 (107), A.7.5 (1419, Gap 1421) | F-A.7.5b (T328) |
| "UnderReview" | A.1.3 (113, Gap 116) | F-A.1.3b (T324) |
| "Back to list", "Not found" | A.5.7 (854, 859) | F-A.5.7a |
| "badge-draft" | A.1.8 (195) | — |
| "by the address" | A.1.3 (99), A.1.12 (260) | F-A.1.12a (T276) |
| "No confirmation is asked" | A.1.12 (259) | F-A.1.12a (T264) |
| "No mail" / "No email" | A.1.3 (113), A.1.5 (140), A.1.8 (198), A.1.9 (206, 210), A.1.13 (280, 284); A.1.2's (86) stays | F-A.1.2a, F-A.1.9a (T319) |
| UTC times | A.1.2 (85-87), A.1.3 (109, 113), A.1.5, A.1.6, A.1.7, A.1.8 (197), A.1.11, A.1.12 (267), the Outcome | F-A.1.2c (T325) |
| (the request page's width) | A.1.3 | F-A.1.3c (T323) |
#### T4 · The Outcome state
- The Goal's "four data-rights requests" → five; the tuples (0,5,1) (1,3,1) (1,4,1) (2,4,1) (4,4,1).
- The audit: Submit 5, Withdraw 1, Approve 3, Reject 1; ApplyRectificationCommand 1 and CompleteRectificationRequestCommand 1 added to the IN (…) list.
- Shared state: "KGK has five data-rights requests" and "Dr Dlamini has opted out of optional processing" dropped (Q28, T318); the SELECT loses "OptOutOfOptionalProcessing" (a hard SQL error after T318) and reads "dlamini: not opted out".
- "Dr Mahlangu's last name was saved twice (A.1.8a)" beside Prof Mbatha's name save; the Handoff's "applying and completing a rectification, which no page offers" goes; states.md's data-rights rows and coverage.md's change with them (the build's).
#### T5 · The cast review's list
A row each on R3-0-steps (A.1.1, A.1.2, A.1.3, A.1.5, A.1.8, A.1.9, A.1.12, A.1.13, A.2.3, A.2.5, A.5.7, A.7.3, A.7.5); A.2.4 and A.2.6 unchanged (they read the log); A.1.4, A.1.10 and A.1.14 unchanged.
#### T6 · Re-pinning
Every hard value is T379's. A.1.8a adds five actions before A.1.11, so the replay that first plays it mints new ids, minutes, a new pseudonym and a new "51 s", and the build re-pins from it; if A.1.8a's request lands in the minute of the rejected one, the queue's two Review names carry the second (N4).
#### T7 · Journeys, presses + fields typed (390: Menu one more press)
Journeys
| Journey | Today | 1280 | 390 | Changed by F2 or F10? |
| An export requested and downloaded (Dr Dlamini) | 4 + 1 | 4 + 1 | 5 + 1 | No |
| A rejected answer read (Dr Mahlangu) | not possible | 1 + 0 | 2 + 0 | No |
| A request decided (Mr Smit, A.1.3) | 6 + 1 | 6 + 1 | 7 + 1 | No (the pager is not pressed) |
| A correction carried through (Mr Smit, A.1.8a; the approval apart, 1 + 1) | not possible | 5 + 2 | 6 + 2 | No: Data rights requests, Review, Account › Last name, before and after typed, Record correction, Complete request |
| A name corrected on My account (Dr Mahlangu, NEW) | 2 + 1 | 2 + 1 | 3 + 1 | New in the step by F2: the name in the top bar, Last name typed, Save (at 390 the account row is in the menu: Menu, then the name) |
| An erasure approved (devadmin) | 5 + 1 | 6 + 1 | 7 + 1 | No |
| The reminders stopped (Mr Smit) | 3 + 0 | 3 + 0 | 4 + 0 | No |
#### T8 · Build states that need a states.md row
M13, M14, M15, M16, M17, M18; Q02, Q09, Q10, Q11, Q12; D10, D11, D12, D13, D14, D15, D16, D17, D18, D19, D20, D21, D22; C04, C07, C08, C09, C10, C11; K02; U02.

######## BOARD R3-M01-1280.dc.html — R3-M01 · A.1.1 · Anele Dlamini reads the page, nothing requested, nothing saved · 1280
> R3-M01 · 1280 · focus: none (the page as read; Q28)
Wombat
Acting as Trainee
MenuClose
Acting as Trainee
- Home
- Log an activity
- My activities
- MSF reports
- My committee reviews
- Export portfolio
- My progress
- My data rights
Anele Dlamini
Sign out
### My data rights
Ask for a copy of your data, a correction or erasure, and read the answers. Stop the regular reminder emails.
#### Your requests
No requests yet.
You have not asked for your data, a correction or erasure. Make a request below.
#### Make a request
Under data protection law you may ask for a copy of your personal data, its correction, or its erasure. Staff at Kgosi Kgari Teaching Hospital decide it; the answer shows under Your requests.
Request type * required
[value: Access]
Access
Everything Wombat holds about you, as a download, once the request is approved.
[value: Export]
Export
The same download, to take your data elsewhere: machine-readable data with a PDF summary.
[value: Rectification]
Rectification
Something about you is recorded wrongly. Say what it is and what it should be.
[value: Erasure]
Erasure
Your account and personal data are removed; your training records are kept under a name that identifies nobody. Once approved it cannot be undone, and you can no longer sign in.
Reason *required
0 of 4,000 characters
What happens next: staff at Kgosi Kgari Teaching Hospital read it and approve or reject it with a note, and you are emailed when it is decided. While it waits you can withdraw it under Your requests.
Submit request
#### Reminder emails
Wombat processes what your training programme needs (your record, assessments and what regulation requires), and that cannot be objected to. You can stop its regular reminders.
Stop the regular reminder emails
Stops three regular reminders: the weekly coordinator digest, the reminder of drafts left untouched for 14 days, and the reminder of activities that have waited more than 5 days for you to assess. Email about one particular thing is still sent, such as an invitation to give feedback or a coordinator's reminder about one activity.
Save

######## BOARD R3-M02-1280.dc.html — R3-M02 · A.1.2 · Export chosen, Submit with no reason: refused · 1280
> R3-M02 · 1280 · focus: Reason (refused, Export kept)
Wombat
Acting as Trainee
MenuClose
Acting as Trainee
- Home
- Log an activity
- My activities
- MSF reports
- My committee reviews
- Export portfolio
- My progress
- My data rights
Anele Dlamini
Sign out
### My data rights
Ask for a copy of your data, a correction or erasure, and read the answers. Stop the regular reminder emails.
#### Your requests
No requests yet.
You have not asked for your data, a correction or erasure. Make a request below.
#### Make a request
Under data protection law you may ask for a copy of your personal data, its correction, or its erasure. Staff at Kgosi Kgari Teaching Hospital decide it; the answer shows under Your requests.
Request type * required
[value: Access]
Access
Everything Wombat holds about you, as a download, once the request is approved.
[value: Export] [checked]
Export
The same download, to take your data elsewhere: machine-readable data with a PDF summary.
[value: Rectification]
Rectification
Something about you is recorded wrongly. Say what it is and what it should be.
[value: Erasure]
Erasure
Your account and personal data are removed; your training records are kept under a name that identifies nobody. Once approved it cannot be undone, and you can no longer sign in.
Reason *required
Please provide a reason for your request.
0 of 4,000 characters
What happens next: staff at Kgosi Kgari Teaching Hospital read it and approve or reject it with a note, and you are emailed when it is decided. While it waits you can withdraw it under Your requests.
Submit request
#### Reminder emails
Wombat processes what your training programme needs (your record, assessments and what regulation requires), and that cannot be objected to. You can stop its regular reminders.
Stop the regular reminder emails
Stops three regular reminders: the weekly coordinator digest, the reminder of drafts left untouched for 14 days, and the reminder of activities that have waited more than 5 days for you to assess. Email about one particular thing is still sent, such as an invitation to give feedback or a coordinator's reminder about one activity.
Save

######## BOARD R3-M03-1280.dc.html — R3-M03 · A.1.2 · submitted: the result, the row Submitted with Withdraw · 1280
> R3-M03 · 1280 · focus: the result in Your requests
Wombat
Acting as Trainee
MenuClose
Acting as Trainee
- Home
- Log an activity
- My activities
- MSF reports
- My committee reviews
- Export portfolio
- My progress
- My data rights
Anele Dlamini
Sign out
### My data rights
Ask for a copy of your data, a correction or erasure, and read the answers. Stop the regular reminder emails.
#### Your requests
Request submitted. You will be notified when it is reviewed.
Your data rights requests, newest first
| Submitted | Type | Status | Answer | Actions |
| 2026-10-28 00:01 SAST | Export | Submitted | Waiting for a decision. | [aria-label: Withdraw the Export request made 2026-10-28 00:01 SAST] Withdraw |
#### Make a request
Under data protection law you may ask for a copy of your personal data, its correction, or its erasure. Staff at Kgosi Kgari Teaching Hospital decide it; the answer shows under Your requests.
Request type * required
[value: Access]
Access
Everything Wombat holds about you, as a download, once the request is approved.
[value: Export]
Export
The same download, to take your data elsewhere: machine-readable data with a PDF summary.
[value: Rectification]
Rectification
Something about you is recorded wrongly. Say what it is and what it should be.
[value: Erasure]
Erasure
Your account and personal data are removed; your training records are kept under a name that identifies nobody. Once approved it cannot be undone, and you can no longer sign in.
Reason *required
0 of 4,000 characters
What happens next: staff at Kgosi Kgari Teaching Hospital read it and approve or reject it with a note, and you are emailed when it is decided. While it waits you can withdraw it under Your requests.
Submit request
#### Reminder emails
Wombat processes what your training programme needs (your record, assessments and what regulation requires), and that cannot be objected to. You can stop its regular reminders.
Stop the regular reminder emails
Stops three regular reminders: the weekly coordinator digest, the reminder of drafts left untouched for 14 days, and the reminder of activities that have waited more than 5 days for you to assess. Email about one particular thing is still sent, such as an invitation to give feedback or a coordinator's reminder about one activity.
Save

######## BOARD R3-M04-1280.dc.html — R3-M04 · A.1.5 · Completed with its answer and Download (390 twin: A.7.3) · 1280
> R3-M04 · 1280 · focus: none (page load)
Wombat
Acting as Trainee
MenuClose
Acting as Trainee
- Home
- Log an activity
- My activities
- MSF reports
- My committee reviews
- Export portfolio
- My progress
- My data rights
Anele Dlamini
Sign out
### My data rights
Ask for a copy of your data, a correction or erasure, and read the answers. Stop the regular reminder emails.
#### Your requests
Your data rights requests, newest first
| Submitted | Type | Status | Answer | Actions |
| 2026-10-28 00:01 SAST | Export | Completed | Approved on 2026-10-28: “Identity confirmed; export released to the data subject.” | [aria-label: Download the export of the Export request made 2026-10-28 00:01 SAST] Download |
#### Make a request
Under data protection law you may ask for a copy of your personal data, its correction, or its erasure. Staff at Kgosi Kgari Teaching Hospital decide it; the answer shows under Your requests.
Request type * required
[value: Access]
Access
Everything Wombat holds about you, as a download, once the request is approved.
[value: Export]
Export
The same download, to take your data elsewhere: machine-readable data with a PDF summary.
[value: Rectification]
Rectification
Something about you is recorded wrongly. Say what it is and what it should be.
[value: Erasure]
Erasure
Your account and personal data are removed; your training records are kept under a name that identifies nobody. Once approved it cannot be undone, and you can no longer sign in.
Reason *required
0 of 4,000 characters
What happens next: staff at Kgosi Kgari Teaching Hospital read it and approve or reject it with a note, and you are emailed when it is decided. While it waits you can withdraw it under Your requests.
Submit request
#### Reminder emails
Wombat processes what your training programme needs (your record, assessments and what regulation requires), and that cannot be objected to. You can stop its regular reminders.
Stop the regular reminder emails
Stops three regular reminders: the weekly coordinator digest, the reminder of drafts left untouched for 14 days, and the reminder of activities that have waited more than 5 days for you to assess. Email about one particular thing is still sent, such as an invitation to give feedback or a coordinator's reminder about one activity.
Save

######## BOARD R3-M05-1280.dc.html — R3-M05 · A.1.6 · Pieter du Plessis (former trainee), Access submitted · 1280
> R3-M05 · 1280 · focus: the result in Your requests
Wombat
MenuClose
- Home
- My progress
- Export portfolio
- My data rights
Pieter du Plessis
Sign out
### My data rights
Ask for a copy of your data, a correction or erasure, and read the answers. Stop the regular reminder emails.
#### Your requests
Request submitted. You will be notified when it is reviewed.
Your data rights requests, newest first
| Submitted | Type | Status | Answer | Actions |
| 2026-10-28 00:03 SAST | Access | Submitted | Waiting for a decision. | [aria-label: Withdraw the Access request made 2026-10-28 00:03 SAST] Withdraw |
#### Make a request
Under data protection law you may ask for a copy of your personal data, its correction, or its erasure. Staff at Kgosi Kgari Teaching Hospital decide it; the answer shows under Your requests.
Request type * required
[value: Access]
Access
Everything Wombat holds about you, as a download, once the request is approved.
[value: Export]
Export
The same download, to take your data elsewhere: machine-readable data with a PDF summary.
[value: Rectification]
Rectification
Something about you is recorded wrongly. Say what it is and what it should be.
[value: Erasure]
Erasure
Your account and personal data are removed; your training records are kept under a name that identifies nobody. Once approved it cannot be undone, and you can no longer sign in.
Reason *required
0 of 4,000 characters
What happens next: staff at Kgosi Kgari Teaching Hospital read it and approve or reject it with a note, and you are emailed when it is decided. While it waits you can withdraw it under Your requests.
Submit request
#### Reminder emails
Wombat processes what your training programme needs (your record, assessments and what regulation requires), and that cannot be objected to. You can stop its regular reminders.
Stop the regular reminder emails
Stops three regular reminders: the weekly coordinator digest, the reminder of drafts left untouched for 14 days, and the reminder of activities that have waited more than 5 days for you to assess. Email about one particular thing is still sent, such as an invitation to give feedback or a coordinator's reminder about one activity.
Save

######## BOARD R3-M06-1280.dc.html — R3-M06 · A.1.6 · withdrawn: nothing offered · 1280
> R3-M06 · 1280 · focus: “Request withdrawn.”
Wombat
MenuClose
- Home
- My progress
- Export portfolio
- My data rights
Pieter du Plessis
Sign out
### My data rights
Ask for a copy of your data, a correction or erasure, and read the answers. Stop the regular reminder emails.
#### Your requests
Request withdrawn.
Your data rights requests, newest first
| Submitted | Type | Status | Answer | Actions |
| 2026-10-28 00:03 SAST | Access | Withdrawn | You withdrew it before a decision. |  |
#### Make a request
Under data protection law you may ask for a copy of your personal data, its correction, or its erasure. Staff at Kgosi Kgari Teaching Hospital decide it; the answer shows under Your requests.
Request type * required
[value: Access]
Access
Everything Wombat holds about you, as a download, once the request is approved.
[value: Export]
Export
The same download, to take your data elsewhere: machine-readable data with a PDF summary.
[value: Rectification]
Rectification
Something about you is recorded wrongly. Say what it is and what it should be.
[value: Erasure]
Erasure
Your account and personal data are removed; your training records are kept under a name that identifies nobody. Once approved it cannot be undone, and you can no longer sign in.
Reason *required
0 of 4,000 characters
What happens next: staff at Kgosi Kgari Teaching Hospital read it and approve or reject it with a note, and you are emailed when it is decided. While it waits you can withdraw it under Your requests.
Submit request
#### Reminder emails
Wombat processes what your training programme needs (your record, assessments and what regulation requires), and that cannot be objected to. You can stop its regular reminders.
Stop the regular reminder emails
Stops three regular reminders: the weekly coordinator digest, the reminder of drafts left untouched for 14 days, and the reminder of activities that have waited more than 5 days for you to assess. Email about one particular thing is still sent, such as an invitation to give feedback or a coordinator's reminder about one activity.
Save

######## BOARD R3-M07-1280.dc.html — R3-M07 · A.1.7 · Nomsa Mahlangu, Rectification submitted · 1280
> R3-M07 · 1280 · focus: the result in Your requests
Wombat
Acting as Trainee
MenuClose
Acting as Trainee
- Home
- Log an activity
- My activities
- MSF reports
- My committee reviews
- Export portfolio
- My progress
- My data rights
Nomsa Mahlangu
Sign out
### My data rights
Ask for a copy of your data, a correction or erasure, and read the answers. Stop the regular reminder emails.
#### Your requests
Request submitted. You will be notified when it is reviewed.
Your data rights requests, newest first
| Submitted | Type | Status | Answer | Actions |
| 2026-10-28 00:03 SAST | Rectification | Submitted | Waiting for a decision. | [aria-label: Withdraw the Rectification request made 2026-10-28 00:03 SAST] Withdraw |
#### Make a request
Under data protection law you may ask for a copy of your personal data, its correction, or its erasure. Staff at Kgosi Kgari Teaching Hospital decide it; the answer shows under Your requests.
Request type * required
[value: Access]
Access
Everything Wombat holds about you, as a download, once the request is approved.
[value: Export]
Export
The same download, to take your data elsewhere: machine-readable data with a PDF summary.
[value: Rectification]
Rectification
Something about you is recorded wrongly. Say what it is and what it should be.
[value: Erasure]
Erasure
Your account and personal data are removed; your training records are kept under a name that identifies nobody. Once approved it cannot be undone, and you can no longer sign in.
Reason *required
0 of 4,000 characters
What happens next: staff at Kgosi Kgari Teaching Hospital read it and approve or reject it with a note, and you are emailed when it is decided. While it waits you can withdraw it under Your requests.
Submit request
#### Reminder emails
Wombat processes what your training programme needs (your record, assessments and what regulation requires), and that cannot be objected to. You can stop its regular reminders.
Stop the regular reminder emails
Stops three regular reminders: the weekly coordinator digest, the reminder of drafts left untouched for 14 days, and the reminder of activities that have waited more than 5 days for you to assess. Email about one particular thing is still sent, such as an invitation to give feedback or a coordinator's reminder about one activity.
Save

######## BOARD R3-M08-1280.dc.html — R3-M08 · A.1.8a · the name Rectification approved, before the corrective save · 1280
> R3-M08 · 1280 · focus: none (page load) · read between the approval and the corrective save: top bar Nomsa Mahlngu
Wombat
Acting as Trainee
MenuClose
Acting as Trainee
- Home
- Log an activity
- My activities
- MSF reports
- My committee reviews
- Export portfolio
- My progress
- My data rights
Nomsa Mahlngu
Sign out
### My data rights
Ask for a copy of your data, a correction or erasure, and read the answers. Stop the regular reminder emails.
#### Your requests
Your data rights requests, newest first
| Submitted | Type | Status | Answer | Actions |
| ‹time› SAST | Rectification | Approved | Approved on ‹date›: “‹note›”. Being corrected; it reads Completed once done. |  |
| 2026-10-28 00:03 SAST | Rectification | Rejected | Rejected on 2026-10-28: “The whole January 2026 intake is admitted on 15 January, the semester boundary (D42). Nothing to correct.” |  |
#### Make a request
Under data protection law you may ask for a copy of your personal data, its correction, or its erasure. Staff at Kgosi Kgari Teaching Hospital decide it; the answer shows under Your requests.
Request type * required
[value: Access]
Access
Everything Wombat holds about you, as a download, once the request is approved.
[value: Export]
Export
The same download, to take your data elsewhere: machine-readable data with a PDF summary.
[value: Rectification]
Rectification
Something about you is recorded wrongly. Say what it is and what it should be.
[value: Erasure]
Erasure
Your account and personal data are removed; your training records are kept under a name that identifies nobody. Once approved it cannot be undone, and you can no longer sign in.
Reason *required
0 of 4,000 characters
What happens next: staff at Kgosi Kgari Teaching Hospital read it and approve or reject it with a note, and you are emailed when it is decided. While it waits you can withdraw it under Your requests.
Submit request
#### Reminder emails
Wombat processes what your training programme needs (your record, assessments and what regulation requires), and that cannot be objected to. You can stop its regular reminders.
Stop the regular reminder emails
Stops three regular reminders: the weekly coordinator digest, the reminder of drafts left untouched for 14 days, and the reminder of activities that have waited more than 5 days for you to assess. Email about one particular thing is still sent, such as an invitation to give feedback or a coordinator's reminder about one activity.
Save

######## BOARD R3-M09-1280.dc.html — R3-M09 · A.1.9 · Nomsa Mahlangu reads both answers · 1280
> R3-M09 · 1280 · focus: none (page load) · after the corrective save
Wombat
Acting as Trainee
MenuClose
Acting as Trainee
- Home
- Log an activity
- My activities
- MSF reports
- My committee reviews
- Export portfolio
- My progress
- My data rights
Nomsa Mahlangu
Sign out
### My data rights
Ask for a copy of your data, a correction or erasure, and read the answers. Stop the regular reminder emails.
#### Your requests
Your data rights requests, newest first
| Submitted | Type | Status | Answer | Actions |
| ‹time› SAST | Rectification | Completed | Completed on ‹date›. Corrected: Account › Last name. |  |
| 2026-10-28 00:03 SAST | Rectification | Rejected | Rejected on 2026-10-28: “The whole January 2026 intake is admitted on 15 January, the semester boundary (D42). Nothing to correct.” |  |
#### Make a request
Under data protection law you may ask for a copy of your personal data, its correction, or its erasure. Staff at Kgosi Kgari Teaching Hospital decide it; the answer shows under Your requests.
Request type * required
[value: Access]
Access
Everything Wombat holds about you, as a download, once the request is approved.
[value: Export]
Export
The same download, to take your data elsewhere: machine-readable data with a PDF summary.
[value: Rectification]
Rectification
Something about you is recorded wrongly. Say what it is and what it should be.
[value: Erasure]
Erasure
Your account and personal data are removed; your training records are kept under a name that identifies nobody. Once approved it cannot be undone, and you can no longer sign in.
Reason *required
0 of 4,000 characters
What happens next: staff at Kgosi Kgari Teaching Hospital read it and approve or reject it with a note, and you are emailed when it is decided. While it waits you can withdraw it under Your requests.
Submit request
#### Reminder emails
Wombat processes what your training programme needs (your record, assessments and what regulation requires), and that cannot be objected to. You can stop its regular reminders.
Stop the regular reminder emails
Stops three regular reminders: the weekly coordinator digest, the reminder of drafts left untouched for 14 days, and the reminder of activities that have waited more than 5 days for you to assess. Email about one particular thing is still sent, such as an invitation to give feedback or a coordinator's reminder about one activity.
Save

######## BOARD R3-M10-1280.dc.html — R3-M10 · A.1.11 · Sipho Ndlovu, Erasure submitted with Withdraw · 1280
> R3-M10 · 1280 · focus: the result in Your requests
Wombat
Acting as Trainee
MenuClose
Acting as Trainee
- Home
- Log an activity
- My activities
- MSF reports
- My committee reviews
- Export portfolio
- My progress
- My data rights
Sipho Ndlovu
Sign out
### My data rights
Ask for a copy of your data, a correction or erasure, and read the answers. Stop the regular reminder emails.
#### Your requests
Request submitted. You will be notified when it is reviewed.
Your data rights requests, newest first
| Submitted | Type | Status | Answer | Actions |
| 2026-10-28 00:04 SAST | Erasure | Submitted | Waiting for a decision. | [aria-label: Withdraw the Erasure request made 2026-10-28 00:04 SAST] Withdraw |
#### Make a request
Under data protection law you may ask for a copy of your personal data, its correction, or its erasure. Staff at Kgosi Kgari Teaching Hospital decide it; the answer shows under Your requests.
Request type * required
[value: Access]
Access
Everything Wombat holds about you, as a download, once the request is approved.
[value: Export]
Export
The same download, to take your data elsewhere: machine-readable data with a PDF summary.
[value: Rectification]
Rectification
Something about you is recorded wrongly. Say what it is and what it should be.
[value: Erasure]
Erasure
Your account and personal data are removed; your training records are kept under a name that identifies nobody. Once approved it cannot be undone, and you can no longer sign in.
Reason *required
0 of 4,000 characters
What happens next: staff at Kgosi Kgari Teaching Hospital read it and approve or reject it with a note, and you are emailed when it is decided. While it waits you can withdraw it under Your requests.
Submit request
#### Reminder emails
Wombat processes what your training programme needs (your record, assessments and what regulation requires), and that cannot be objected to. You can stop its regular reminders.
Stop the regular reminder emails
Stops three regular reminders: the weekly coordinator digest, the reminder of drafts left untouched for 14 days, and the reminder of activities that have waited more than 5 days for you to assess. Email about one particular thing is still sent, such as an invitation to give feedback or a coordinator's reminder about one activity.
Save

######## BOARD R3-M11-1280.dc.html — R3-M11 · A.2.3 · Pieter Smit stops the reminder emails · 1280
> R3-M11 · 1280 · focus: “Reminder emails stopped.”
Wombat
Acting as Coordinator
MenuClose
Acting as Coordinator
- Home
- Programme trainees
- Waiting for assessors
- Decisions due
- MSF campaigns
- Committee reviews
- Decision panels
- Data rights requests
- My data rights
Pieter Smit
Sign out
### My data rights
Ask for a copy of your data, a correction or erasure, and read the answers. Stop the regular reminder emails.
#### Your requests
No requests yet.
You have not asked for your data, a correction or erasure. Make a request below.
#### Make a request
Under data protection law you may ask for a copy of your personal data, its correction, or its erasure. Staff at Kgosi Kgari Teaching Hospital decide it; the answer shows under Your requests.
Request type * required
[value: Access]
Access
Everything Wombat holds about you, as a download, once the request is approved.
[value: Export]
Export
The same download, to take your data elsewhere: machine-readable data with a PDF summary.
[value: Rectification]
Rectification
Something about you is recorded wrongly. Say what it is and what it should be.
[value: Erasure]
Erasure
Your account and personal data are removed; your training records are kept under a name that identifies nobody. Once approved it cannot be undone, and you can no longer sign in.
Reason *required
0 of 4,000 characters
What happens next: staff at Kgosi Kgari Teaching Hospital read it and approve or reject it with a note, and you are emailed when it is decided. While it waits you can withdraw it under Your requests.
Submit request
#### Reminder emails
Reminder emails stopped.
Wombat processes what your training programme needs (your record, assessments and what regulation requires), and that cannot be objected to. You can stop its regular reminders.
[checked]
Stop the regular reminder emails
Stops three regular reminders: the weekly coordinator digest, the reminder of drafts left untouched for 14 days, and the reminder of activities that have waited more than 5 days for you to assess. Email about one particular thing is still sent, such as an invitation to give feedback or a coordinator's reminder about one activity.
Save

######## BOARD R3-M12-1280.dc.html — R3-M12 · A.2.5 · Pieter Smit starts them again · 1280
> R3-M12 · 1280 · focus: “Reminder emails will be sent again.”
Wombat
Acting as Coordinator
MenuClose
Acting as Coordinator
- Home
- Programme trainees
- Waiting for assessors
- Decisions due
- MSF campaigns
- Committee reviews
- Decision panels
- Data rights requests
- My data rights
Pieter Smit
Sign out
### My data rights
Ask for a copy of your data, a correction or erasure, and read the answers. Stop the regular reminder emails.
#### Your requests
No requests yet.
You have not asked for your data, a correction or erasure. Make a request below.
#### Make a request
Under data protection law you may ask for a copy of your personal data, its correction, or its erasure. Staff at Kgosi Kgari Teaching Hospital decide it; the answer shows under Your requests.
Request type * required
[value: Access]
Access
Everything Wombat holds about you, as a download, once the request is approved.
[value: Export]
Export
The same download, to take your data elsewhere: machine-readable data with a PDF summary.
[value: Rectification]
Rectification
Something about you is recorded wrongly. Say what it is and what it should be.
[value: Erasure]
Erasure
Your account and personal data are removed; your training records are kept under a name that identifies nobody. Once approved it cannot be undone, and you can no longer sign in.
Reason *required
0 of 4,000 characters
What happens next: staff at Kgosi Kgari Teaching Hospital read it and approve or reject it with a note, and you are emailed when it is decided. While it waits you can withdraw it under Your requests.
Submit request
#### Reminder emails
Reminder emails will be sent again.
Wombat processes what your training programme needs (your record, assessments and what regulation requires), and that cannot be objected to. You can stop its regular reminders.
Stop the regular reminder emails
Stops three regular reminders: the weekly coordinator digest, the reminder of drafts left untouched for 14 days, and the reminder of activities that have waited more than 5 days for you to assess. Email about one particular thing is still sent, such as an invitation to give feedback or a coordinator's reminder about one activity.
Save

######## BOARD R3-M13-1280.dc.html — R3-M13 · A refusal after an earlier result: one result only (T318) · 1280 · Illustration
> IllustrationR3-M13 · 1280 · focus: Reason; “Reminder emails stopped.” is gone
Wombat
Acting as Coordinator
MenuClose
Acting as Coordinator
- Home
- Programme trainees
- Waiting for assessors
- Decisions due
- MSF campaigns
- Committee reviews
- Decision panels
- Data rights requests
- My data rights
Pieter Smit
Sign out
### My data rights
Ask for a copy of your data, a correction or erasure, and read the answers. Stop the regular reminder emails.
#### Your requests
No requests yet.
You have not asked for your data, a correction or erasure. Make a request below.
#### Make a request
Under data protection law you may ask for a copy of your personal data, its correction, or its erasure. Staff at Kgosi Kgari Teaching Hospital decide it; the answer shows under Your requests.
Request type * required
[value: Access]
Access
Everything Wombat holds about you, as a download, once the request is approved.
[value: Export] [checked]
Export
The same download, to take your data elsewhere: machine-readable data with a PDF summary.
[value: Rectification]
Rectification
Something about you is recorded wrongly. Say what it is and what it should be.
[value: Erasure]
Erasure
Your account and personal data are removed; your training records are kept under a name that identifies nobody. Once approved it cannot be undone, and you can no longer sign in.
Reason *required
Please provide a reason for your request.
0 of 4,000 characters
What happens next: staff at Kgosi Kgari Teaching Hospital read it and approve or reject it with a note, and you are emailed when it is decided. While it waits you can withdraw it under Your requests.
Submit request
#### Reminder emails
Wombat processes what your training programme needs (your record, assessments and what regulation requires), and that cannot be objected to. You can stop its regular reminders.
[checked]
Stop the regular reminder emails
Stops three regular reminders: the weekly coordinator digest, the reminder of drafts left untouched for 14 days, and the reminder of activities that have waited more than 5 days for you to assess. Email about one particular thing is still sent, such as an invitation to give feedback or a coordinator's reminder about one activity.
Save

######## BOARD R3-M14-1280.dc.html — R3-M14 · Loading · 1280
> R3-M14 · 1280 · focus: none
Wombat
Acting as Trainee
MenuClose
Acting as Trainee
- Home
- Log an activity
- My activities
- MSF reports
- My committee reviews
- Export portfolio
- My progress
- My data rights
Anele Dlamini
Sign out
### My data rights
Ask for a copy of your data, a correction or erasure, and read the answers. Stop the regular reminder emails.
Loading your data rights.

######## BOARD R3-M15-1280.dc.html — R3-M15 · The load error · 1280
> R3-M15 · 1280 · focus: Try again → the h1 (tabindex -1)
Wombat
Acting as Trainee
MenuClose
Acting as Trainee
- Home
- Log an activity
- My activities
- MSF reports
- My committee reviews
- Export portfolio
- My progress
- My data rights
Anele Dlamini
Sign out
### My data rights
Ask for a copy of your data, a correction or erasure, and read the answers. Stop the regular reminder emails.
Could not load your data rights. Nothing has changed. Try again, or come back in a few minutes.Try again

######## BOARD R3-M16-1280.dc.html — R3-M16 · A failed submit: the type and reason kept · 1280 · Illustration
> IllustrationR3-M16 · 1280 · focus: the failure alert · fails A.1.2's submit
Wombat
Acting as Trainee
MenuClose
Acting as Trainee
- Home
- Log an activity
- My activities
- MSF reports
- My committee reviews
- Export portfolio
- My progress
- My data rights
Anele Dlamini
Sign out
### My data rights
Ask for a copy of your data, a correction or erasure, and read the answers. Stop the regular reminder emails.
#### Your requests
No requests yet.
You have not asked for your data, a correction or erasure. Make a request below.
#### Make a request
The request could not be sent just now. Nothing was sent. Try again, or come back in a few minutes.
Under data protection law you may ask for a copy of your personal data, its correction, or its erasure. Staff at Kgosi Kgari Teaching Hospital decide it; the answer shows under Your requests.
Request type * required
[value: Access]
Access
Everything Wombat holds about you, as a download, once the request is approved.
[value: Export] [checked]
Export
The same download, to take your data elsewhere: machine-readable data with a PDF summary.
[value: Rectification]
Rectification
Something about you is recorded wrongly. Say what it is and what it should be.
[value: Erasure]
Erasure
Your account and personal data are removed; your training records are kept under a name that identifies nobody. Once approved it cannot be undone, and you can no longer sign in.
Reason *required
A copy of my training record for my own files.
46 of 4,000 characters
What happens next: staff at Kgosi Kgari Teaching Hospital read it and approve or reject it with a note, and you are emailed when it is decided. While it waits you can withdraw it under Your requests.
Submit request
#### Reminder emails
Wombat processes what your training programme needs (your record, assessments and what regulation requires), and that cannot be objected to. You can stop its regular reminders.
Stop the regular reminder emails
Stops three regular reminders: the weekly coordinator digest, the reminder of drafts left untouched for 14 days, and the reminder of activities that have waited more than 5 days for you to assess. Email about one particular thing is still sent, such as an invitation to give feedback or a coordinator's reminder about one activity.
Save

######## BOARD R3-M17-1280.dc.html — R3-M17 · Two Export requests in one minute; a long note; a 4,000-character reason · 1280 · Illustration
> IllustrationR3-M17 · 1280 · focus: none · the two Withdraws named to the second (N4)
Wombat
Acting as Trainee
MenuClose
Acting as Trainee
- Home
- Log an activity
- My activities
- MSF reports
- My committee reviews
- Export portfolio
- My progress
- My data rights
‹name›
Sign out
### My data rights
Ask for a copy of your data, a correction or erasure, and read the answers. Stop the regular reminder emails.
#### Your requests
Your data rights requests, newest first
| Submitted | Type | Status | Answer | Actions |
| 2026-10-28 ‹hh:mm› SAST | Export | Submitted | Waiting for a decision. | [aria-label: Withdraw the Export request made 2026-10-28 ‹hh:mm:ss› SAST] Withdraw |
| 2026-10-28 ‹hh:mm› SAST | Export | Submitted | Waiting for a decision. | [aria-label: Withdraw the Export request made 2026-10-28 ‹hh:mm:ss› SAST] Withdraw |
| ‹time› SAST | Rectification | Rejected | Rejected on ‹date›: “‹a long decision note, wrapping in its cell› ‹a long decision note, wrapping in its cell› ‹a long decision note, wrapping in its cell› ‹a long decision note, wrapping in its cell› ‹a long decision note, wrapping in its cell› ‹a long decision note, wrapping in its cell› ‹a long decision note, wrapping in its cell› ‹a long decision note, wrapping in its cell› ‹a long decision note, wrapping in its cell› ‹a long decision note, wrapping in its cell› ‹a long decision note, wrapping in its cell› ‹a long decision note, wrapping in its cell› ‹a long decision note, wrapping in its cell› ‹a long decision note, wrapping in its cell›” |  |
#### Make a request
Under data protection law you may ask for a copy of your personal data, its correction, or its erasure. Staff at Kgosi Kgari Teaching Hospital decide it; the answer shows under Your requests.
Request type * required
[value: Access]
Access
Everything Wombat holds about you, as a download, once the request is approved.
[value: Export] [checked]
Export
The same download, to take your data elsewhere: machine-readable data with a PDF summary.
[value: Rectification]
Rectification
Something about you is recorded wrongly. Say what it is and what it should be.
[value: Erasure]
Erasure
Your account and personal data are removed; your training records are kept under a name that identifies nobody. Once approved it cannot be undone, and you can no longer sign in.
Reason *required
‹a reason of 4,000 characters, wrapping in the box›
4,000 of 4,000 characters
What happens next: staff at Kgosi Kgari Teaching Hospital read it and approve or reject it with a note, and you are emailed when it is decided. While it waits you can withdraw it under Your requests.
Submit request
#### Reminder emails
Wombat processes what your training programme needs (your record, assessments and what regulation requires), and that cannot be objected to. You can stop its regular reminders.
Stop the regular reminder emails
Stops three regular reminders: the weekly coordinator digest, the reminder of drafts left untouched for 14 days, and the reminder of activities that have waited more than 5 days for you to assess. Email about one particular thing is still sent, such as an invitation to give feedback or a coordinator's reminder about one activity.
Save

######## BOARD R3-M18-1280.dc.html — R3-M18 · A requester with no institution: the intro's other sentence · 1280 · Illustration
> IllustrationR3-M18 · 1280 · focus: none
Wombat
MenuClose
- Home
- My data rights
‹name›
Sign out
### My data rights
Ask for a copy of your data, a correction or erasure, and read the answers. Stop the regular reminder emails.
#### Your requests
No requests yet.
You have not asked for your data, a correction or erasure. Make a request below.
#### Make a request
Under data protection law you may ask for a copy of your personal data, its correction, or its erasure. The Wombat administrator decides it; the answer shows under Your requests.
Request type * required
[value: Access]
Access
Everything Wombat holds about you, as a download, once the request is approved.
[value: Export]
Export
The same download, to take your data elsewhere: machine-readable data with a PDF summary.
[value: Rectification]
Rectification
Something about you is recorded wrongly. Say what it is and what it should be.
[value: Erasure]
Erasure
Your account and personal data are removed; your training records are kept under a name that identifies nobody. Once approved it cannot be undone, and you can no longer sign in.
Reason *required
0 of 4,000 characters
What happens next: the Wombat administrator reads it and approves or rejects it with a note, and you are emailed when it is decided. While it waits you can withdraw it under Your requests.
Submit request
#### Reminder emails
Wombat processes what your training programme needs (your record, assessments and what regulation requires), and that cannot be objected to. You can stop its regular reminders.
Stop the regular reminder emails
Stops three regular reminders: the weekly coordinator digest, the reminder of drafts left untouched for 14 days, and the reminder of activities that have waited more than 5 days for you to assess. Email about one particular thing is still sent, such as an invitation to give feedback or a coordinator's reminder about one activity.
Save

######## BOARD R3-M01-390.dc.html — R3-M01 · A.1.1 · Anele Dlamini reads the page, nothing requested, nothing saved · 390
> R3-M01 · 390 · focus: none (the page as read; Q28)
Wombat
Acting as Trainee
MenuClose
Acting as Trainee
- Home
- Log an activity
- My activities
- MSF reports
- My committee reviews
- Export portfolio
- My progress
- My data rights
Anele Dlamini
Sign out
### My data rights
Ask for a copy of your data, a correction or erasure, and read the answers. Stop the regular reminder emails.
#### Your requests
No requests yet.
You have not asked for your data, a correction or erasure. Make a request below.
#### Make a request
Under data protection law you may ask for a copy of your personal data, its correction, or its erasure. Staff at Kgosi Kgari Teaching Hospital decide it; the answer shows under Your requests.
Request type * required
[value: Access]
Access
Everything Wombat holds about you, as a download, once the request is approved.
[value: Export]
Export
The same download, to take your data elsewhere: machine-readable data with a PDF summary.
[value: Rectification]
Rectification
Something about you is recorded wrongly. Say what it is and what it should be.
[value: Erasure]
Erasure
Your account and personal data are removed; your training records are kept under a name that identifies nobody. Once approved it cannot be undone, and you can no longer sign in.
Reason *required
0 of 4,000 characters
What happens next: staff at Kgosi Kgari Teaching Hospital read it and approve or reject it with a note, and you are emailed when it is decided. While it waits you can withdraw it under Your requests.
Submit request
#### Reminder emails
Wombat processes what your training programme needs (your record, assessments and what regulation requires), and that cannot be objected to. You can stop its regular reminders.
Stop the regular reminder emails
Stops three regular reminders: the weekly coordinator digest, the reminder of drafts left untouched for 14 days, and the reminder of activities that have waited more than 5 days for you to assess. Email about one particular thing is still sent, such as an invitation to give feedback or a coordinator's reminder about one activity.
Save

######## BOARD R3-M02-390.dc.html — R3-M02 · A.1.2 · Export chosen, Submit with no reason: refused · 390
> R3-M02 · 390 · focus: Reason (refused, Export kept)
Wombat
Acting as Trainee
MenuClose
Acting as Trainee
- Home
- Log an activity
- My activities
- MSF reports
- My committee reviews
- Export portfolio
- My progress
- My data rights
Anele Dlamini
Sign out
### My data rights
Ask for a copy of your data, a correction or erasure, and read the answers. Stop the regular reminder emails.
#### Your requests
No requests yet.
You have not asked for your data, a correction or erasure. Make a request below.
#### Make a request
Under data protection law you may ask for a copy of your personal data, its correction, or its erasure. Staff at Kgosi Kgari Teaching Hospital decide it; the answer shows under Your requests.
Request type * required
[value: Access]
Access
Everything Wombat holds about you, as a download, once the request is approved.
[value: Export] [checked]
Export
The same download, to take your data elsewhere: machine-readable data with a PDF summary.
[value: Rectification]
Rectification
Something about you is recorded wrongly. Say what it is and what it should be.
[value: Erasure]
Erasure
Your account and personal data are removed; your training records are kept under a name that identifies nobody. Once approved it cannot be undone, and you can no longer sign in.
Reason *required
Please provide a reason for your request.
0 of 4,000 characters
What happens next: staff at Kgosi Kgari Teaching Hospital read it and approve or reject it with a note, and you are emailed when it is decided. While it waits you can withdraw it under Your requests.
Submit request
#### Reminder emails
Wombat processes what your training programme needs (your record, assessments and what regulation requires), and that cannot be objected to. You can stop its regular reminders.
Stop the regular reminder emails
Stops three regular reminders: the weekly coordinator digest, the reminder of drafts left untouched for 14 days, and the reminder of activities that have waited more than 5 days for you to assess. Email about one particular thing is still sent, such as an invitation to give feedback or a coordinator's reminder about one activity.
Save

######## BOARD R3-M03-390.dc.html — R3-M03 · A.1.2 · submitted: the result, the row Submitted with Withdraw · 390
> R3-M03 · 390 · focus: the result in Your requests
Wombat
Acting as Trainee
MenuClose
Acting as Trainee
- Home
- Log an activity
- My activities
- MSF reports
- My committee reviews
- Export portfolio
- My progress
- My data rights
Anele Dlamini
Sign out
### My data rights
Ask for a copy of your data, a correction or erasure, and read the answers. Stop the regular reminder emails.
#### Your requests
Request submitted. You will be notified when it is reviewed.
Your data rights requests, newest first
| Submitted | Type | Status | Answer | Actions |
| 2026-10-28 00:01 SAST | Export | Submitted | Waiting for a decision. | [aria-label: Withdraw the Export request made 2026-10-28 00:01 SAST] Withdraw |
#### Make a request
Under data protection law you may ask for a copy of your personal data, its correction, or its erasure. Staff at Kgosi Kgari Teaching Hospital decide it; the answer shows under Your requests.
Request type * required
[value: Access]
Access
Everything Wombat holds about you, as a download, once the request is approved.
[value: Export]
Export
The same download, to take your data elsewhere: machine-readable data with a PDF summary.
[value: Rectification]
Rectification
Something about you is recorded wrongly. Say what it is and what it should be.
[value: Erasure]
Erasure
Your account and personal data are removed; your training records are kept under a name that identifies nobody. Once approved it cannot be undone, and you can no longer sign in.
Reason *required
0 of 4,000 characters
What happens next: staff at Kgosi Kgari Teaching Hospital read it and approve or reject it with a note, and you are emailed when it is decided. While it waits you can withdraw it under Your requests.
Submit request
#### Reminder emails
Wombat processes what your training programme needs (your record, assessments and what regulation requires), and that cannot be objected to. You can stop its regular reminders.
Stop the regular reminder emails
Stops three regular reminders: the weekly coordinator digest, the reminder of drafts left untouched for 14 days, and the reminder of activities that have waited more than 5 days for you to assess. Email about one particular thing is still sent, such as an invitation to give feedback or a coordinator's reminder about one activity.
Save

######## BOARD R3-M04-390.dc.html — R3-M04 · A.1.5 · Completed with its answer and Download (390 twin: A.7.3) · 390
> R3-M04 · 390 · focus: none (page load)
Wombat
Acting as Trainee
MenuClose
Acting as Trainee
- Home
- Log an activity
- My activities
- MSF reports
- My committee reviews
- Export portfolio
- My progress
- My data rights
Anele Dlamini
Sign out
### My data rights
Ask for a copy of your data, a correction or erasure, and read the answers. Stop the regular reminder emails.
#### Your requests
Your data rights requests, newest first
| Submitted | Type | Status | Answer | Actions |
| 2026-10-28 00:01 SAST | Export | Completed | Approved on 2026-10-28: “Identity confirmed; export released to the data subject.” | [aria-label: Download the export of the Export request made 2026-10-28 00:01 SAST] Download |
#### Make a request
Under data protection law you may ask for a copy of your personal data, its correction, or its erasure. Staff at Kgosi Kgari Teaching Hospital decide it; the answer shows under Your requests.
Request type * required
[value: Access]
Access
Everything Wombat holds about you, as a download, once the request is approved.
[value: Export]
Export
The same download, to take your data elsewhere: machine-readable data with a PDF summary.
[value: Rectification]
Rectification
Something about you is recorded wrongly. Say what it is and what it should be.
[value: Erasure]
Erasure
Your account and personal data are removed; your training records are kept under a name that identifies nobody. Once approved it cannot be undone, and you can no longer sign in.
Reason *required
0 of 4,000 characters
What happens next: staff at Kgosi Kgari Teaching Hospital read it and approve or reject it with a note, and you are emailed when it is decided. While it waits you can withdraw it under Your requests.
Submit request
#### Reminder emails
Wombat processes what your training programme needs (your record, assessments and what regulation requires), and that cannot be objected to. You can stop its regular reminders.
Stop the regular reminder emails
Stops three regular reminders: the weekly coordinator digest, the reminder of drafts left untouched for 14 days, and the reminder of activities that have waited more than 5 days for you to assess. Email about one particular thing is still sent, such as an invitation to give feedback or a coordinator's reminder about one activity.
Save

######## BOARD R3-M05-390.dc.html — R3-M05 · A.1.6 · Pieter du Plessis (former trainee), Access submitted · 390
> R3-M05 · 390 · focus: the result in Your requests
Wombat
MenuClose
- Home
- My progress
- Export portfolio
- My data rights
Pieter du Plessis
Sign out
### My data rights
Ask for a copy of your data, a correction or erasure, and read the answers. Stop the regular reminder emails.
#### Your requests
Request submitted. You will be notified when it is reviewed.
Your data rights requests, newest first
| Submitted | Type | Status | Answer | Actions |
| 2026-10-28 00:03 SAST | Access | Submitted | Waiting for a decision. | [aria-label: Withdraw the Access request made 2026-10-28 00:03 SAST] Withdraw |
#### Make a request
Under data protection law you may ask for a copy of your personal data, its correction, or its erasure. Staff at Kgosi Kgari Teaching Hospital decide it; the answer shows under Your requests.
Request type * required
[value: Access]
Access
Everything Wombat holds about you, as a download, once the request is approved.
[value: Export]
Export
The same download, to take your data elsewhere: machine-readable data with a PDF summary.
[value: Rectification]
Rectification
Something about you is recorded wrongly. Say what it is and what it should be.
[value: Erasure]
Erasure
Your account and personal data are removed; your training records are kept under a name that identifies nobody. Once approved it cannot be undone, and you can no longer sign in.
Reason *required
0 of 4,000 characters
What happens next: staff at Kgosi Kgari Teaching Hospital read it and approve or reject it with a note, and you are emailed when it is decided. While it waits you can withdraw it under Your requests.
Submit request
#### Reminder emails
Wombat processes what your training programme needs (your record, assessments and what regulation requires), and that cannot be objected to. You can stop its regular reminders.
Stop the regular reminder emails
Stops three regular reminders: the weekly coordinator digest, the reminder of drafts left untouched for 14 days, and the reminder of activities that have waited more than 5 days for you to assess. Email about one particular thing is still sent, such as an invitation to give feedback or a coordinator's reminder about one activity.
Save

######## BOARD R3-M06-390.dc.html — R3-M06 · A.1.6 · withdrawn: nothing offered · 390
> R3-M06 · 390 · focus: “Request withdrawn.”
Wombat
MenuClose
- Home
- My progress
- Export portfolio
- My data rights
Pieter du Plessis
Sign out
### My data rights
Ask for a copy of your data, a correction or erasure, and read the answers. Stop the regular reminder emails.
#### Your requests
Request withdrawn.
Your data rights requests, newest first
| Submitted | Type | Status | Answer | Actions |
| 2026-10-28 00:03 SAST | Access | Withdrawn | You withdrew it before a decision. |  |
#### Make a request
Under data protection law you may ask for a copy of your personal data, its correction, or its erasure. Staff at Kgosi Kgari Teaching Hospital decide it; the answer shows under Your requests.
Request type * required
[value: Access]
Access
Everything Wombat holds about you, as a download, once the request is approved.
[value: Export]
Export
The same download, to take your data elsewhere: machine-readable data with a PDF summary.
[value: Rectification]
Rectification
Something about you is recorded wrongly. Say what it is and what it should be.
[value: Erasure]
Erasure
Your account and personal data are removed; your training records are kept under a name that identifies nobody. Once approved it cannot be undone, and you can no longer sign in.
Reason *required
0 of 4,000 characters
What happens next: staff at Kgosi Kgari Teaching Hospital read it and approve or reject it with a note, and you are emailed when it is decided. While it waits you can withdraw it under Your requests.
Submit request
#### Reminder emails
Wombat processes what your training programme needs (your record, assessments and what regulation requires), and that cannot be objected to. You can stop its regular reminders.
Stop the regular reminder emails
Stops three regular reminders: the weekly coordinator digest, the reminder of drafts left untouched for 14 days, and the reminder of activities that have waited more than 5 days for you to assess. Email about one particular thing is still sent, such as an invitation to give feedback or a coordinator's reminder about one activity.
Save

######## BOARD R3-M07-390.dc.html — R3-M07 · A.1.7 · Nomsa Mahlangu, Rectification submitted · 390
> R3-M07 · 390 · focus: the result in Your requests
Wombat
Acting as Trainee
MenuClose
Acting as Trainee
- Home
- Log an activity
- My activities
- MSF reports
- My committee reviews
- Export portfolio
- My progress
- My data rights
Nomsa Mahlangu
Sign out
### My data rights
Ask for a copy of your data, a correction or erasure, and read the answers. Stop the regular reminder emails.
#### Your requests
Request submitted. You will be notified when it is reviewed.
Your data rights requests, newest first
| Submitted | Type | Status | Answer | Actions |
| 2026-10-28 00:03 SAST | Rectification | Submitted | Waiting for a decision. | [aria-label: Withdraw the Rectification request made 2026-10-28 00:03 SAST] Withdraw |
#### Make a request
Under data protection law you may ask for a copy of your personal data, its correction, or its erasure. Staff at Kgosi Kgari Teaching Hospital decide it; the answer shows under Your requests.
Request type * required
[value: Access]
Access
Everything Wombat holds about you, as a download, once the request is approved.
[value: Export]
Export
The same download, to take your data elsewhere: machine-readable data with a PDF summary.
[value: Rectification]
Rectification
Something about you is recorded wrongly. Say what it is and what it should be.
[value: Erasure]
Erasure
Your account and personal data are removed; your training records are kept under a name that identifies nobody. Once approved it cannot be undone, and you can no longer sign in.
Reason *required
0 of 4,000 characters
What happens next: staff at Kgosi Kgari Teaching Hospital read it and approve or reject it with a note, and you are emailed when it is decided. While it waits you can withdraw it under Your requests.
Submit request
#### Reminder emails
Wombat processes what your training programme needs (your record, assessments and what regulation requires), and that cannot be objected to. You can stop its regular reminders.
Stop the regular reminder emails
Stops three regular reminders: the weekly coordinator digest, the reminder of drafts left untouched for 14 days, and the reminder of activities that have waited more than 5 days for you to assess. Email about one particular thing is still sent, such as an invitation to give feedback or a coordinator's reminder about one activity.
Save

######## BOARD R3-M08-390.dc.html — R3-M08 · A.1.8a · the name Rectification approved, before the corrective save · 390
> R3-M08 · 390 · focus: none (page load) · read between the approval and the corrective save: top bar Nomsa Mahlngu
Wombat
Acting as Trainee
MenuClose
Acting as Trainee
- Home
- Log an activity
- My activities
- MSF reports
- My committee reviews
- Export portfolio
- My progress
- My data rights
Nomsa Mahlngu
Sign out
### My data rights
Ask for a copy of your data, a correction or erasure, and read the answers. Stop the regular reminder emails.
#### Your requests
Your data rights requests, newest first
| Submitted | Type | Status | Answer | Actions |
| ‹time› SAST | Rectification | Approved | Approved on ‹date›: “‹note›”. Being corrected; it reads Completed once done. |  |
| 2026-10-28 00:03 SAST | Rectification | Rejected | Rejected on 2026-10-28: “The whole January 2026 intake is admitted on 15 January, the semester boundary (D42). Nothing to correct.” |  |
#### Make a request
Under data protection law you may ask for a copy of your personal data, its correction, or its erasure. Staff at Kgosi Kgari Teaching Hospital decide it; the answer shows under Your requests.
Request type * required
[value: Access]
Access
Everything Wombat holds about you, as a download, once the request is approved.
[value: Export]
Export
The same download, to take your data elsewhere: machine-readable data with a PDF summary.
[value: Rectification]
Rectification
Something about you is recorded wrongly. Say what it is and what it should be.
[value: Erasure]
Erasure
Your account and personal data are removed; your training records are kept under a name that identifies nobody. Once approved it cannot be undone, and you can no longer sign in.
Reason *required
0 of 4,000 characters
What happens next: staff at Kgosi Kgari Teaching Hospital read it and approve or reject it with a note, and you are emailed when it is decided. While it waits you can withdraw it under Your requests.
Submit request
#### Reminder emails
Wombat processes what your training programme needs (your record, assessments and what regulation requires), and that cannot be objected to. You can stop its regular reminders.
Stop the regular reminder emails
Stops three regular reminders: the weekly coordinator digest, the reminder of drafts left untouched for 14 days, and the reminder of activities that have waited more than 5 days for you to assess. Email about one particular thing is still sent, such as an invitation to give feedback or a coordinator's reminder about one activity.
Save

######## BOARD R3-M09-390.dc.html — R3-M09 · A.1.9 · Nomsa Mahlangu reads both answers · 390
> R3-M09 · 390 · focus: none (page load) · after the corrective save
Wombat
Acting as Trainee
MenuClose
Acting as Trainee
- Home
- Log an activity
- My activities
- MSF reports
- My committee reviews
- Export portfolio
- My progress
- My data rights
Nomsa Mahlangu
Sign out
### My data rights
Ask for a copy of your data, a correction or erasure, and read the answers. Stop the regular reminder emails.
#### Your requests
Your data rights requests, newest first
| Submitted | Type | Status | Answer | Actions |
| ‹time› SAST | Rectification | Completed | Completed on ‹date›. Corrected: Account › Last name. |  |
| 2026-10-28 00:03 SAST | Rectification | Rejected | Rejected on 2026-10-28: “The whole January 2026 intake is admitted on 15 January, the semester boundary (D42). Nothing to correct.” |  |
#### Make a request
Under data protection law you may ask for a copy of your personal data, its correction, or its erasure. Staff at Kgosi Kgari Teaching Hospital decide it; the answer shows under Your requests.
Request type * required
[value: Access]
Access
Everything Wombat holds about you, as a download, once the request is approved.
[value: Export]
Export
The same download, to take your data elsewhere: machine-readable data with a PDF summary.
[value: Rectification]
Rectification
Something about you is recorded wrongly. Say what it is and what it should be.
[value: Erasure]
Erasure
Your account and personal data are removed; your training records are kept under a name that identifies nobody. Once approved it cannot be undone, and you can no longer sign in.
Reason *required
0 of 4,000 characters
What happens next: staff at Kgosi Kgari Teaching Hospital read it and approve or reject it with a note, and you are emailed when it is decided. While it waits you can withdraw it under Your requests.
Submit request
#### Reminder emails
Wombat processes what your training programme needs (your record, assessments and what regulation requires), and that cannot be objected to. You can stop its regular reminders.
Stop the regular reminder emails
Stops three regular reminders: the weekly coordinator digest, the reminder of drafts left untouched for 14 days, and the reminder of activities that have waited more than 5 days for you to assess. Email about one particular thing is still sent, such as an invitation to give feedback or a coordinator's reminder about one activity.
Save

######## BOARD R3-M10-390.dc.html — R3-M10 · A.1.11 · Sipho Ndlovu, Erasure submitted with Withdraw · 390
> R3-M10 · 390 · focus: the result in Your requests
Wombat
Acting as Trainee
MenuClose
Acting as Trainee
- Home
- Log an activity
- My activities
- MSF reports
- My committee reviews
- Export portfolio
- My progress
- My data rights
Sipho Ndlovu
Sign out
### My data rights
Ask for a copy of your data, a correction or erasure, and read the answers. Stop the regular reminder emails.
#### Your requests
Request submitted. You will be notified when it is reviewed.
Your data rights requests, newest first
| Submitted | Type | Status | Answer | Actions |
| 2026-10-28 00:04 SAST | Erasure | Submitted | Waiting for a decision. | [aria-label: Withdraw the Erasure request made 2026-10-28 00:04 SAST] Withdraw |
#### Make a request
Under data protection law you may ask for a copy of your personal data, its correction, or its erasure. Staff at Kgosi Kgari Teaching Hospital decide it; the answer shows under Your requests.
Request type * required
[value: Access]
Access
Everything Wombat holds about you, as a download, once the request is approved.
[value: Export]
Export
The same download, to take your data elsewhere: machine-readable data with a PDF summary.
[value: Rectification]
Rectification
Something about you is recorded wrongly. Say what it is and what it should be.
[value: Erasure]
Erasure
Your account and personal data are removed; your training records are kept under a name that identifies nobody. Once approved it cannot be undone, and you can no longer sign in.
Reason *required
0 of 4,000 characters
What happens next: staff at Kgosi Kgari Teaching Hospital read it and approve or reject it with a note, and you are emailed when it is decided. While it waits you can withdraw it under Your requests.
Submit request
#### Reminder emails
Wombat processes what your training programme needs (your record, assessments and what regulation requires), and that cannot be objected to. You can stop its regular reminders.
Stop the regular reminder emails
Stops three regular reminders: the weekly coordinator digest, the reminder of drafts left untouched for 14 days, and the reminder of activities that have waited more than 5 days for you to assess. Email about one particular thing is still sent, such as an invitation to give feedback or a coordinator's reminder about one activity.
Save

######## BOARD R3-M11-390.dc.html — R3-M11 · A.2.3 · Pieter Smit stops the reminder emails · 390
> R3-M11 · 390 · focus: “Reminder emails stopped.”
Wombat
Acting as Coordinator
MenuClose
Acting as Coordinator
- Home
- Programme trainees
- Waiting for assessors
- Decisions due
- MSF campaigns
- Committee reviews
- Decision panels
- Data rights requests
- My data rights
Pieter Smit
Sign out
### My data rights
Ask for a copy of your data, a correction or erasure, and read the answers. Stop the regular reminder emails.
#### Your requests
No requests yet.
You have not asked for your data, a correction or erasure. Make a request below.
#### Make a request
Under data protection law you may ask for a copy of your personal data, its correction, or its erasure. Staff at Kgosi Kgari Teaching Hospital decide it; the answer shows under Your requests.
Request type * required
[value: Access]
Access
Everything Wombat holds about you, as a download, once the request is approved.
[value: Export]
Export
The same download, to take your data elsewhere: machine-readable data with a PDF summary.
[value: Rectification]
Rectification
Something about you is recorded wrongly. Say what it is and what it should be.
[value: Erasure]
Erasure
Your account and personal data are removed; your training records are kept under a name that identifies nobody. Once approved it cannot be undone, and you can no longer sign in.
Reason *required
0 of 4,000 characters
What happens next: staff at Kgosi Kgari Teaching Hospital read it and approve or reject it with a note, and you are emailed when it is decided. While it waits you can withdraw it under Your requests.
Submit request
#### Reminder emails
Reminder emails stopped.
Wombat processes what your training programme needs (your record, assessments and what regulation requires), and that cannot be objected to. You can stop its regular reminders.
[checked]
Stop the regular reminder emails
Stops three regular reminders: the weekly coordinator digest, the reminder of drafts left untouched for 14 days, and the reminder of activities that have waited more than 5 days for you to assess. Email about one particular thing is still sent, such as an invitation to give feedback or a coordinator's reminder about one activity.
Save

######## BOARD R3-M12-390.dc.html — R3-M12 · A.2.5 · Pieter Smit starts them again · 390
> R3-M12 · 390 · focus: “Reminder emails will be sent again.”
Wombat
Acting as Coordinator
MenuClose
Acting as Coordinator
- Home
- Programme trainees
- Waiting for assessors
- Decisions due
- MSF campaigns
- Committee reviews
- Decision panels
- Data rights requests
- My data rights
Pieter Smit
Sign out
### My data rights
Ask for a copy of your data, a correction or erasure, and read the answers. Stop the regular reminder emails.
#### Your requests
No requests yet.
You have not asked for your data, a correction or erasure. Make a request below.
#### Make a request
Under data protection law you may ask for a copy of your personal data, its correction, or its erasure. Staff at Kgosi Kgari Teaching Hospital decide it; the answer shows under Your requests.
Request type * required
[value: Access]
Access
Everything Wombat holds about you, as a download, once the request is approved.
[value: Export]
Export
The same download, to take your data elsewhere: machine-readable data with a PDF summary.
[value: Rectification]
Rectification
Something about you is recorded wrongly. Say what it is and what it should be.
[value: Erasure]
Erasure
Your account and personal data are removed; your training records are kept under a name that identifies nobody. Once approved it cannot be undone, and you can no longer sign in.
Reason *required
0 of 4,000 characters
What happens next: staff at Kgosi Kgari Teaching Hospital read it and approve or reject it with a note, and you are emailed when it is decided. While it waits you can withdraw it under Your requests.
Submit request
#### Reminder emails
Reminder emails will be sent again.
Wombat processes what your training programme needs (your record, assessments and what regulation requires), and that cannot be objected to. You can stop its regular reminders.
Stop the regular reminder emails
Stops three regular reminders: the weekly coordinator digest, the reminder of drafts left untouched for 14 days, and the reminder of activities that have waited more than 5 days for you to assess. Email about one particular thing is still sent, such as an invitation to give feedback or a coordinator's reminder about one activity.
Save

######## BOARD R3-M13-390.dc.html — R3-M13 · A refusal after an earlier result: one result only (T318) · 390 · Illustration
> IllustrationR3-M13 · 390 · focus: Reason; “Reminder emails stopped.” is gone
Wombat
Acting as Coordinator
MenuClose
Acting as Coordinator
- Home
- Programme trainees
- Waiting for assessors
- Decisions due
- MSF campaigns
- Committee reviews
- Decision panels
- Data rights requests
- My data rights
Pieter Smit
Sign out
### My data rights
Ask for a copy of your data, a correction or erasure, and read the answers. Stop the regular reminder emails.
#### Your requests
No requests yet.
You have not asked for your data, a correction or erasure. Make a request below.
#### Make a request
Under data protection law you may ask for a copy of your personal data, its correction, or its erasure. Staff at Kgosi Kgari Teaching Hospital decide it; the answer shows under Your requests.
Request type * required
[value: Access]
Access
Everything Wombat holds about you, as a download, once the request is approved.
[value: Export] [checked]
Export
The same download, to take your data elsewhere: machine-readable data with a PDF summary.
[value: Rectification]
Rectification
Something about you is recorded wrongly. Say what it is and what it should be.
[value: Erasure]
Erasure
Your account and personal data are removed; your training records are kept under a name that identifies nobody. Once approved it cannot be undone, and you can no longer sign in.
Reason *required
Please provide a reason for your request.
0 of 4,000 characters
What happens next: staff at Kgosi Kgari Teaching Hospital read it and approve or reject it with a note, and you are emailed when it is decided. While it waits you can withdraw it under Your requests.
Submit request
#### Reminder emails
Wombat processes what your training programme needs (your record, assessments and what regulation requires), and that cannot be objected to. You can stop its regular reminders.
[checked]
Stop the regular reminder emails
Stops three regular reminders: the weekly coordinator digest, the reminder of drafts left untouched for 14 days, and the reminder of activities that have waited more than 5 days for you to assess. Email about one particular thing is still sent, such as an invitation to give feedback or a coordinator's reminder about one activity.
Save

######## BOARD R3-M14-390.dc.html — R3-M14 · Loading · 390
> R3-M14 · 390 · focus: none
Wombat
Acting as Trainee
MenuClose
Acting as Trainee
- Home
- Log an activity
- My activities
- MSF reports
- My committee reviews
- Export portfolio
- My progress
- My data rights
Anele Dlamini
Sign out
### My data rights
Ask for a copy of your data, a correction or erasure, and read the answers. Stop the regular reminder emails.
Loading your data rights.

######## BOARD R3-M15-390.dc.html — R3-M15 · The load error · 390
> R3-M15 · 390 · focus: Try again → the h1 (tabindex -1)
Wombat
Acting as Trainee
MenuClose
Acting as Trainee
- Home
- Log an activity
- My activities
- MSF reports
- My committee reviews
- Export portfolio
- My progress
- My data rights
Anele Dlamini
Sign out
### My data rights
Ask for a copy of your data, a correction or erasure, and read the answers. Stop the regular reminder emails.
Could not load your data rights. Nothing has changed. Try again, or come back in a few minutes.Try again

######## BOARD R3-M16-390.dc.html — R3-M16 · A failed submit: the type and reason kept · 390 · Illustration
> IllustrationR3-M16 · 390 · focus: the failure alert · fails A.1.2's submit
Wombat
Acting as Trainee
MenuClose
Acting as Trainee
- Home
- Log an activity
- My activities
- MSF reports
- My committee reviews
- Export portfolio
- My progress
- My data rights
Anele Dlamini
Sign out
### My data rights
Ask for a copy of your data, a correction or erasure, and read the answers. Stop the regular reminder emails.
#### Your requests
No requests yet.
You have not asked for your data, a correction or erasure. Make a request below.
#### Make a request
The request could not be sent just now. Nothing was sent. Try again, or come back in a few minutes.
Under data protection law you may ask for a copy of your personal data, its correction, or its erasure. Staff at Kgosi Kgari Teaching Hospital decide it; the answer shows under Your requests.
Request type * required
[value: Access]
Access
Everything Wombat holds about you, as a download, once the request is approved.
[value: Export] [checked]
Export
The same download, to take your data elsewhere: machine-readable data with a PDF summary.
[value: Rectification]
Rectification
Something about you is recorded wrongly. Say what it is and what it should be.
[value: Erasure]
Erasure
Your account and personal data are removed; your training records are kept under a name that identifies nobody. Once approved it cannot be undone, and you can no longer sign in.
Reason *required
A copy of my training record for my own files.
46 of 4,000 characters
What happens next: staff at Kgosi Kgari Teaching Hospital read it and approve or reject it with a note, and you are emailed when it is decided. While it waits you can withdraw it under Your requests.
Submit request
#### Reminder emails
Wombat processes what your training programme needs (your record, assessments and what regulation requires), and that cannot be objected to. You can stop its regular reminders.
Stop the regular reminder emails
Stops three regular reminders: the weekly coordinator digest, the reminder of drafts left untouched for 14 days, and the reminder of activities that have waited more than 5 days for you to assess. Email about one particular thing is still sent, such as an invitation to give feedback or a coordinator's reminder about one activity.
Save

######## BOARD R3-M17-390.dc.html — R3-M17 · Two Export requests in one minute; a long note; a 4,000-character reason · 390 · Illustration
> IllustrationR3-M17 · 390 · focus: none · the two Withdraws named to the second (N4)
Wombat
Acting as Trainee
MenuClose
Acting as Trainee
- Home
- Log an activity
- My activities
- MSF reports
- My committee reviews
- Export portfolio
- My progress
- My data rights
‹name›
Sign out
### My data rights
Ask for a copy of your data, a correction or erasure, and read the answers. Stop the regular reminder emails.
#### Your requests
Your data rights requests, newest first
| Submitted | Type | Status | Answer | Actions |
| 2026-10-28 ‹hh:mm› SAST | Export | Submitted | Waiting for a decision. | [aria-label: Withdraw the Export request made 2026-10-28 ‹hh:mm:ss› SAST] Withdraw |
| 2026-10-28 ‹hh:mm› SAST | Export | Submitted | Waiting for a decision. | [aria-label: Withdraw the Export request made 2026-10-28 ‹hh:mm:ss› SAST] Withdraw |
| ‹time› SAST | Rectification | Rejected | Rejected on ‹date›: “‹a long decision note, wrapping in its cell› ‹a long decision note, wrapping in its cell› ‹a long decision note, wrapping in its cell› ‹a long decision note, wrapping in its cell› ‹a long decision note, wrapping in its cell› ‹a long decision note, wrapping in its cell› ‹a long decision note, wrapping in its cell› ‹a long decision note, wrapping in its cell› ‹a long decision note, wrapping in its cell› ‹a long decision note, wrapping in its cell› ‹a long decision note, wrapping in its cell› ‹a long decision note, wrapping in its cell› ‹a long decision note, wrapping in its cell› ‹a long decision note, wrapping in its cell›” |  |
#### Make a request
Under data protection law you may ask for a copy of your personal data, its correction, or its erasure. Staff at Kgosi Kgari Teaching Hospital decide it; the answer shows under Your requests.
Request type * required
[value: Access]
Access
Everything Wombat holds about you, as a download, once the request is approved.
[value: Export] [checked]
Export
The same download, to take your data elsewhere: machine-readable data with a PDF summary.
[value: Rectification]
Rectification
Something about you is recorded wrongly. Say what it is and what it should be.
[value: Erasure]
Erasure
Your account and personal data are removed; your training records are kept under a name that identifies nobody. Once approved it cannot be undone, and you can no longer sign in.
Reason *required
‹a reason of 4,000 characters, wrapping in the box›
4,000 of 4,000 characters
What happens next: staff at Kgosi Kgari Teaching Hospital read it and approve or reject it with a note, and you are emailed when it is decided. While it waits you can withdraw it under Your requests.
Submit request
#### Reminder emails
Wombat processes what your training programme needs (your record, assessments and what regulation requires), and that cannot be objected to. You can stop its regular reminders.
Stop the regular reminder emails
Stops three regular reminders: the weekly coordinator digest, the reminder of drafts left untouched for 14 days, and the reminder of activities that have waited more than 5 days for you to assess. Email about one particular thing is still sent, such as an invitation to give feedback or a coordinator's reminder about one activity.
Save

######## BOARD R3-M18-390.dc.html — R3-M18 · A requester with no institution: the intro's other sentence · 390 · Illustration
> IllustrationR3-M18 · 390 · focus: none
Wombat
MenuClose
- Home
- My data rights
‹name›
Sign out
### My data rights
Ask for a copy of your data, a correction or erasure, and read the answers. Stop the regular reminder emails.
#### Your requests
No requests yet.
You have not asked for your data, a correction or erasure. Make a request below.
#### Make a request
Under data protection law you may ask for a copy of your personal data, its correction, or its erasure. The Wombat administrator decides it; the answer shows under Your requests.
Request type * required
[value: Access]
Access
Everything Wombat holds about you, as a download, once the request is approved.
[value: Export]
Export
The same download, to take your data elsewhere: machine-readable data with a PDF summary.
[value: Rectification]
Rectification
Something about you is recorded wrongly. Say what it is and what it should be.
[value: Erasure]
Erasure
Your account and personal data are removed; your training records are kept under a name that identifies nobody. Once approved it cannot be undone, and you can no longer sign in.
Reason *required
0 of 4,000 characters
What happens next: the Wombat administrator reads it and approves or rejects it with a note, and you are emailed when it is decided. While it waits you can withdraw it under Your requests.
Submit request
#### Reminder emails
Wombat processes what your training programme needs (your record, assessments and what regulation requires), and that cannot be objected to. You can stop its regular reminders.
Stop the regular reminder emails
Stops three regular reminders: the weekly coordinator digest, the reminder of drafts left untouched for 14 days, and the reminder of activities that have waited more than 5 days for you to assess. Email about one particular thing is still sent, such as an invitation to give feedback or a coordinator's reminder about one activity.
Save

######## BOARD R3-M01-768.dc.html — R3-M01 · A.1.1 · Anele Dlamini reads the page, nothing requested, nothing saved · 768
> R3-M01 · 768 · focus: none (the page as read; Q28)
Wombat
Acting as Trainee
MenuClose
Acting as Trainee
- Home
- Log an activity
- My activities
- MSF reports
- My committee reviews
- Export portfolio
- My progress
- My data rights
Anele Dlamini
Sign out
### My data rights
Ask for a copy of your data, a correction or erasure, and read the answers. Stop the regular reminder emails.
#### Your requests
No requests yet.
You have not asked for your data, a correction or erasure. Make a request below.
#### Make a request
Under data protection law you may ask for a copy of your personal data, its correction, or its erasure. Staff at Kgosi Kgari Teaching Hospital decide it; the answer shows under Your requests.
Request type * required
[value: Access]
Access
Everything Wombat holds about you, as a download, once the request is approved.
[value: Export]
Export
The same download, to take your data elsewhere: machine-readable data with a PDF summary.
[value: Rectification]
Rectification
Something about you is recorded wrongly. Say what it is and what it should be.
[value: Erasure]
Erasure
Your account and personal data are removed; your training records are kept under a name that identifies nobody. Once approved it cannot be undone, and you can no longer sign in.
Reason *required
0 of 4,000 characters
What happens next: staff at Kgosi Kgari Teaching Hospital read it and approve or reject it with a note, and you are emailed when it is decided. While it waits you can withdraw it under Your requests.
Submit request
#### Reminder emails
Wombat processes what your training programme needs (your record, assessments and what regulation requires), and that cannot be objected to. You can stop its regular reminders.
Stop the regular reminder emails
Stops three regular reminders: the weekly coordinator digest, the reminder of drafts left untouched for 14 days, and the reminder of activities that have waited more than 5 days for you to assess. Email about one particular thing is still sent, such as an invitation to give feedback or a coordinator's reminder about one activity.
Save

######## BOARD R3-M04-768.dc.html — R3-M04 · A.1.5 · Completed with its answer and Download (390 twin: A.7.3) · 768
> R3-M04 · 768 · focus: none (page load)
Wombat
Acting as Trainee
MenuClose
Acting as Trainee
- Home
- Log an activity
- My activities
- MSF reports
- My committee reviews
- Export portfolio
- My progress
- My data rights
Anele Dlamini
Sign out
### My data rights
Ask for a copy of your data, a correction or erasure, and read the answers. Stop the regular reminder emails.
#### Your requests
Your data rights requests, newest first
| Submitted | Type | Status | Answer | Actions |
| 2026-10-28 00:01 SAST | Export | Completed | Approved on 2026-10-28: “Identity confirmed; export released to the data subject.” | [aria-label: Download the export of the Export request made 2026-10-28 00:01 SAST] Download |
#### Make a request
Under data protection law you may ask for a copy of your personal data, its correction, or its erasure. Staff at Kgosi Kgari Teaching Hospital decide it; the answer shows under Your requests.
Request type * required
[value: Access]
Access
Everything Wombat holds about you, as a download, once the request is approved.
[value: Export]
Export
The same download, to take your data elsewhere: machine-readable data with a PDF summary.
[value: Rectification]
Rectification
Something about you is recorded wrongly. Say what it is and what it should be.
[value: Erasure]
Erasure
Your account and personal data are removed; your training records are kept under a name that identifies nobody. Once approved it cannot be undone, and you can no longer sign in.
Reason *required
0 of 4,000 characters
What happens next: staff at Kgosi Kgari Teaching Hospital read it and approve or reject it with a note, and you are emailed when it is decided. While it waits you can withdraw it under Your requests.
Submit request
#### Reminder emails
Wombat processes what your training programme needs (your record, assessments and what regulation requires), and that cannot be objected to. You can stop its regular reminders.
Stop the regular reminder emails
Stops three regular reminders: the weekly coordinator digest, the reminder of drafts left untouched for 14 days, and the reminder of activities that have waited more than 5 days for you to assess. Email about one particular thing is still sent, such as an invitation to give feedback or a coordinator's reminder about one activity.
Save

######## BOARD R3-Q01-1280.dc.html — R3-Q01 · A.1.3 · Pieter Smit arrives: Anele Dlamini's row · 1280
> R3-Q01 · 1280 · focus: none (page load)
Wombat
Acting as Coordinator
MenuClose
Acting as Coordinator
- Home
- Programme trainees
- Waiting for assessors
- Decisions due
- MSF campaigns
- Committee reviews
- Decision panels
- Data rights requests
- My data rights
Pieter Smit
Sign out
### Data rights requests
Requests from people at Kgosi Kgari Teaching Hospital about their personal data: decide each, and carry an approved correction through.
[aria-label: Filter Data rights requests]
Type
All types
Access
Export
Rectification
Erasure
Status
All statuses
Submitted
Approved
Completed
Rejected
Withdrawn
Show
#### Requests (1)
Kgosi Kgari Teaching Hospital's data rights requests, newest first
| Submitted | Requester | Type | Status | Request id | Actions |
| 2026-10-28 00:01 SAST | Anele Dlaminidlamini@kgk.wombat.local | Export | Submitted | 01a17f61-f16f-77c1-860c-661586d5b733 | [aria-label: Review the Export request from Anele Dlamini, 2026-10-28 00:01 SAST] Review |

######## BOARD R3-Q02-1280.dc.html — R3-Q02 · A.1.3 · Type Erasure, Show: nothing matches · 1280
> R3-Q02 · 1280 · focus: “Requests (0)”
Wombat
Acting as Coordinator
MenuClose
Acting as Coordinator
- Home
- Programme trainees
- Waiting for assessors
- Decisions due
- MSF campaigns
- Committee reviews
- Decision panels
- Data rights requests
- My data rights
Pieter Smit
Sign out
### Data rights requests
Requests from people at Kgosi Kgari Teaching Hospital about their personal data: decide each, and carry an approved correction through.
[aria-label: Filter Data rights requests]
Type
All types
Access
Export
Rectification
Erasure
Status
All statuses
Submitted
Approved
Completed
Rejected
Withdrawn
ShowClear filters
#### Requests (0)
No requests match
No Erasure request at Kgosi Kgari Teaching Hospital. Clear filters to see every request.

######## BOARD R3-Q03-1280.dc.html — R3-Q03 · A.1.3 · Type Export and Status Submitted, Show: one row · 1280
> R3-Q03 · 1280 · focus: “Requests (1)”
Wombat
Acting as Coordinator
MenuClose
Acting as Coordinator
- Home
- Programme trainees
- Waiting for assessors
- Decisions due
- MSF campaigns
- Committee reviews
- Decision panels
- Data rights requests
- My data rights
Pieter Smit
Sign out
### Data rights requests
Requests from people at Kgosi Kgari Teaching Hospital about their personal data: decide each, and carry an approved correction through.
[aria-label: Filter Data rights requests]
Type
All types
Access
Export
Rectification
Erasure
Status
All statuses
Submitted
Approved
Completed
Rejected
Withdrawn
ShowClear filters
#### Requests (1)
Kgosi Kgari Teaching Hospital's data rights requests, newest first: Export, Submitted
| Submitted | Requester | Type | Status | Request id | Actions |
| 2026-10-28 00:01 SAST | Anele Dlaminidlamini@kgk.wombat.local | Export | Submitted | 01a17f61-f16f-77c1-860c-661586d5b733 | [aria-label: Review the Export request from Anele Dlamini, 2026-10-28 00:01 SAST] Review |

######## BOARD R3-Q04-1280.dc.html — R3-Q04 · A.1.8 · both filters cleared, Show: three, newest first · 1280
> R3-Q04 · 1280 · focus: “Requests (3)”
Wombat
Acting as Coordinator
MenuClose
Acting as Coordinator
- Home
- Programme trainees
- Waiting for assessors
- Decisions due
- MSF campaigns
- Committee reviews
- Decision panels
- Data rights requests
- My data rights
Pieter Smit
Sign out
### Data rights requests
Requests from people at Kgosi Kgari Teaching Hospital about their personal data: decide each, and carry an approved correction through.
[aria-label: Filter Data rights requests]
Type
All types
Access
Export
Rectification
Erasure
Status
All statuses
Submitted
Approved
Completed
Rejected
Withdrawn
Show
#### Requests (3)
Kgosi Kgari Teaching Hospital's data rights requests, newest first
| Submitted | Requester | Type | Status | Request id | Actions |
| 2026-10-28 00:03 SAST | Nomsa Mahlangumahlangu@kgk.wombat.local | Rectification | Submitted | 01a17f63-8c25-7c6e-8724-8593f3d7a145 | [aria-label: Review the Rectification request from Nomsa Mahlangu, 2026-10-28 00:03 SAST] Review |
| 2026-10-28 00:03 SAST | Pieter du Plessisduplessis@kgk.wombat.local | Access | Withdrawn | 01a17f63-7af4-7ac5-a6d1-d01726b57bc0 | [aria-label: Review the Access request from Pieter du Plessis, 2026-10-28 00:03 SAST] Review |
| 2026-10-28 00:01 SAST | Anele Dlaminidlamini@kgk.wombat.local | Export | Completed | 01a17f61-f16f-77c1-860c-661586d5b733 | [aria-label: Review the Export request from Anele Dlamini, 2026-10-28 00:01 SAST] Review |

######## BOARD R3-Q05-1280.dc.html — R3-Q05 · A.1.12 · devadmin's queue, every institution's: five · 1280
> R3-Q05 · 1280 · focus: none (page load)
Wombat
Acting as Administrator
MenuClose
Acting as Administrator
- Home
Platform
- Scheduled jobs
- Audit log
- SSO mappings
- Data rights requests
Organisations
- Institutions
- Colleges
People
- Users
- Invitations
Catalogue
- EPAs
- Curricula
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- My data rights
Demo Administrator
Sign out
### Data rights requests
Requests from people at every institution about their personal data: decide each, and carry an approved correction through.
[aria-label: Filter Data rights requests]
Type
All types
Access
Export
Rectification
Erasure
Status
All statuses
Submitted
Approved
Completed
Rejected
Withdrawn
Show
#### Requests (5)
Every institution's data rights requests, newest first
| Submitted | Requester | Institution | Type | Status | Request id | Actions |
| 2026-10-28 00:04 SAST | Sipho Ndlovundlovu@kgk.wombat.local | Kgosi Kgari Teaching Hospital | Erasure | Submitted | 01a17f64-f566-75f0-91ba-5d71863b77d0 | [aria-label: Review the Erasure request from Sipho Ndlovu, 2026-10-28 00:04 SAST] Review |
| ‹time› SAST | Nomsa Mahlangumahlangu@kgk.wombat.local | Kgosi Kgari Teaching Hospital | Rectification | Completed | ‹request id› | [aria-label: Review the Rectification request from Nomsa Mahlangu, ‹time› SAST] Review |
| 2026-10-28 00:03 SAST | Nomsa Mahlangumahlangu@kgk.wombat.local | Kgosi Kgari Teaching Hospital | Rectification | Rejected | 01a17f63-8c25-7c6e-8724-8593f3d7a145 | [aria-label: Review the Rectification request from Nomsa Mahlangu, 2026-10-28 00:03 SAST] Review |
| 2026-10-28 00:03 SAST | Pieter du Plessisduplessis@kgk.wombat.local | Kgosi Kgari Teaching Hospital | Access | Withdrawn | 01a17f63-7af4-7ac5-a6d1-d01726b57bc0 | [aria-label: Review the Access request from Pieter du Plessis, 2026-10-28 00:03 SAST] Review |
| 2026-10-28 00:01 SAST | Anele Dlaminidlamini@kgk.wombat.local | Kgosi Kgari Teaching Hospital | Export | Completed | 01a17f61-f16f-77c1-860c-661586d5b733 | [aria-label: Review the Export request from Anele Dlamini, 2026-10-28 00:01 SAST] Review |

######## BOARD R3-Q06-1280.dc.html — R3-Q06 · A.1.12 · devadmin, Type Erasure, Show: one row · 1280
> R3-Q06 · 1280 · focus: “Requests (1)”
Wombat
Acting as Administrator
MenuClose
Acting as Administrator
- Home
Platform
- Scheduled jobs
- Audit log
- SSO mappings
- Data rights requests
Organisations
- Institutions
- Colleges
People
- Users
- Invitations
Catalogue
- EPAs
- Curricula
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- My data rights
Demo Administrator
Sign out
### Data rights requests
Requests from people at every institution about their personal data: decide each, and carry an approved correction through.
[aria-label: Filter Data rights requests]
Type
All types
Access
Export
Rectification
Erasure
Status
All statuses
Submitted
Approved
Completed
Rejected
Withdrawn
ShowClear filters
#### Requests (1)
Every institution's data rights requests, newest first: Erasure
| Submitted | Requester | Institution | Type | Status | Request id | Actions |
| 2026-10-28 00:04 SAST | Sipho Ndlovundlovu@kgk.wombat.local | Kgosi Kgari Teaching Hospital | Erasure | Submitted | 01a17f64-f566-75f0-91ba-5d71863b77d0 | [aria-label: Review the Erasure request from Sipho Ndlovu, 2026-10-28 00:04 SAST] Review |

######## BOARD R3-Q07-1280.dc.html — R3-Q07 · A.1.12 · the same after the erasure: the pseudonym · 1280
> R3-Q07 · 1280 · focus: none (back from the request)
Wombat
Acting as Administrator
MenuClose
Acting as Administrator
- Home
Platform
- Scheduled jobs
- Audit log
- SSO mappings
- Data rights requests
Organisations
- Institutions
- Colleges
People
- Users
- Invitations
Catalogue
- EPAs
- Curricula
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- My data rights
Demo Administrator
Sign out
### Data rights requests
Requests from people at every institution about their personal data: decide each, and carry an approved correction through.
[aria-label: Filter Data rights requests]
Type
All types
Access
Export
Rectification
Erasure
Status
All statuses
Submitted
Approved
Completed
Rejected
Withdrawn
ShowClear filters
#### Requests (1)
Every institution's data rights requests, newest first: Erasure
| Submitted | Requester | Institution | Type | Status | Request id | Actions |
| 2026-10-28 00:04 SAST | deleted_user_0ae1d64bErased 2026-10-28 | Kgosi Kgari Teaching Hospital | Erasure | Completed | 01a17f64-f566-75f0-91ba-5d71863b77d0 | [aria-label: Review the Erasure request from deleted_user_0ae1d64b, 2026-10-28 00:04 SAST] Review |

######## BOARD R3-Q08-1280.dc.html — R3-Q08 · Pieter Smit's queue after the erasure: the kept row among KGK's · 1280
> R3-Q08 · 1280 · focus: none (page load)
Wombat
Acting as Coordinator
MenuClose
Acting as Coordinator
- Home
- Programme trainees
- Waiting for assessors
- Decisions due
- MSF campaigns
- Committee reviews
- Decision panels
- Data rights requests
- My data rights
Pieter Smit
Sign out
### Data rights requests
Requests from people at Kgosi Kgari Teaching Hospital about their personal data: decide each, and carry an approved correction through.
[aria-label: Filter Data rights requests]
Type
All types
Access
Export
Rectification
Erasure
Status
All statuses
Submitted
Approved
Completed
Rejected
Withdrawn
Show
#### Requests (5)
Kgosi Kgari Teaching Hospital's data rights requests, newest first
| Submitted | Requester | Type | Status | Request id | Actions |
| 2026-10-28 00:04 SAST | deleted_user_0ae1d64bErased 2026-10-28 | Erasure | Completed | 01a17f64-f566-75f0-91ba-5d71863b77d0 | [aria-label: Review the Erasure request from deleted_user_0ae1d64b, 2026-10-28 00:04 SAST] Review |
| ‹time› SAST | Nomsa Mahlangumahlangu@kgk.wombat.local | Rectification | Completed | ‹request id› | [aria-label: Review the Rectification request from Nomsa Mahlangu, ‹time› SAST] Review |
| 2026-10-28 00:03 SAST | Nomsa Mahlangumahlangu@kgk.wombat.local | Rectification | Rejected | 01a17f63-8c25-7c6e-8724-8593f3d7a145 | [aria-label: Review the Rectification request from Nomsa Mahlangu, 2026-10-28 00:03 SAST] Review |
| 2026-10-28 00:03 SAST | Pieter du Plessisduplessis@kgk.wombat.local | Access | Withdrawn | 01a17f63-7af4-7ac5-a6d1-d01726b57bc0 | [aria-label: Review the Access request from Pieter du Plessis, 2026-10-28 00:03 SAST] Review |
| 2026-10-28 00:01 SAST | Anele Dlaminidlamini@kgk.wombat.local | Export | Completed | 01a17f61-f16f-77c1-860c-661586d5b733 | [aria-label: Review the Export request from Anele Dlamini, 2026-10-28 00:01 SAST] Review |

######## BOARD R3-Q09-1280.dc.html — R3-Q09 · Fifty across four institutions, page 1 of 3 · 1280 · Illustration
> IllustrationR3-Q09 · 1280 · focus: none · a 60-character address, “None”, an Approved row
Wombat
Acting as Administrator
MenuClose
Acting as Administrator
- Home
Platform
- Scheduled jobs
- Audit log
- SSO mappings
- Data rights requests
Organisations
- Institutions
- Colleges
People
- Users
- Invitations
Catalogue
- EPAs
- Curricula
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- My data rights
Demo Administrator
Sign out
### Data rights requests
Requests from people at every institution about their personal data: decide each, and carry an approved correction through.
[aria-label: Filter Data rights requests]
Type
All types
Access
Export
Rectification
Erasure
Status
All statuses
Submitted
Approved
Completed
Rejected
Withdrawn
Show
#### Requests (50)
Every institution's data rights requests, newest first
| Submitted | Requester | Institution | Type | Status | Request id | Actions |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Access | Submitted | ‹request id› | [aria-label: Review the Access request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx@‹domain› | ‹institution› | Export | Completed | ‹request id› | [aria-label: Review the Export request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | None | Rectification | Rejected | ‹request id› | [aria-label: Review the Rectification request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Rectification | Approved | ‹request id› | [aria-label: Review the Rectification request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Access | Completed | ‹request id› | [aria-label: Review the Access request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Export | Submitted | ‹request id› | [aria-label: Review the Export request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Rectification | Completed | ‹request id› | [aria-label: Review the Rectification request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Erasure | Rejected | ‹request id› | [aria-label: Review the Erasure request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Access | Withdrawn | ‹request id› | [aria-label: Review the Access request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Export | Completed | ‹request id› | [aria-label: Review the Export request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Rectification | Submitted | ‹request id› | [aria-label: Review the Rectification request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Erasure | Completed | ‹request id› | [aria-label: Review the Erasure request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Access | Rejected | ‹request id› | [aria-label: Review the Access request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Export | Withdrawn | ‹request id› | [aria-label: Review the Export request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Rectification | Completed | ‹request id› | [aria-label: Review the Rectification request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Erasure | Submitted | ‹request id› | [aria-label: Review the Erasure request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Access | Completed | ‹request id› | [aria-label: Review the Access request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Export | Rejected | ‹request id› | [aria-label: Review the Export request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Rectification | Withdrawn | ‹request id› | [aria-label: Review the Rectification request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Erasure | Completed | ‹request id› | [aria-label: Review the Erasure request from ‹name›, ‹date› ‹time› SAST] Review |
Showing 1–20 of 50
[aria-disabled] PreviousNext
Requests per page
10
20
50
100

######## BOARD R3-Q10-1280.dc.html — R3-Q10 · Page 2 reached by Next: the focus on “Requests (50)” · 1280 · Illustration
> IllustrationR3-Q10 · 1280 · focus: “Requests (50)”
Wombat
Acting as Administrator
MenuClose
Acting as Administrator
- Home
Platform
- Scheduled jobs
- Audit log
- SSO mappings
- Data rights requests
Organisations
- Institutions
- Colleges
People
- Users
- Invitations
Catalogue
- EPAs
- Curricula
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- My data rights
Demo Administrator
Sign out
### Data rights requests
Requests from people at every institution about their personal data: decide each, and carry an approved correction through.
[aria-label: Filter Data rights requests]
Type
All types
Access
Export
Rectification
Erasure
Status
All statuses
Submitted
Approved
Completed
Rejected
Withdrawn
Show
#### Requests (50)
Every institution's data rights requests, newest first
| Submitted | Requester | Institution | Type | Status | Request id | Actions |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Access | Submitted | ‹request id› | [aria-label: Review the Access request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Export | Completed | ‹request id› | [aria-label: Review the Export request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Rectification | Rejected | ‹request id› | [aria-label: Review the Rectification request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Erasure | Withdrawn | ‹request id› | [aria-label: Review the Erasure request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Access | Completed | ‹request id› | [aria-label: Review the Access request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Export | Submitted | ‹request id› | [aria-label: Review the Export request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Rectification | Completed | ‹request id› | [aria-label: Review the Rectification request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Erasure | Rejected | ‹request id› | [aria-label: Review the Erasure request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Access | Withdrawn | ‹request id› | [aria-label: Review the Access request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Export | Completed | ‹request id› | [aria-label: Review the Export request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Rectification | Submitted | ‹request id› | [aria-label: Review the Rectification request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Erasure | Completed | ‹request id› | [aria-label: Review the Erasure request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Access | Rejected | ‹request id› | [aria-label: Review the Access request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Export | Withdrawn | ‹request id› | [aria-label: Review the Export request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Rectification | Completed | ‹request id› | [aria-label: Review the Rectification request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Erasure | Submitted | ‹request id› | [aria-label: Review the Erasure request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Access | Completed | ‹request id› | [aria-label: Review the Access request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Export | Rejected | ‹request id› | [aria-label: Review the Export request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Rectification | Withdrawn | ‹request id› | [aria-label: Review the Rectification request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Erasure | Completed | ‹request id› | [aria-label: Review the Erasure request from ‹name›, ‹date› ‹time› SAST] Review |
Showing 21–40 of 50
PreviousNext
Requests per page
10
20
50
100

######## BOARD R3-Q11-1280.dc.html — R3-Q11 · Loading · 1280
> R3-Q11 · 1280 · focus: none
Wombat
Acting as Coordinator
MenuClose
Acting as Coordinator
- Home
- Programme trainees
- Waiting for assessors
- Decisions due
- MSF campaigns
- Committee reviews
- Decision panels
- Data rights requests
- My data rights
Pieter Smit
Sign out
### Data rights requests
Requests from people at Kgosi Kgari Teaching Hospital about their personal data: decide each, and carry an approved correction through.
Loading the data rights requests.

######## BOARD R3-Q12-1280.dc.html — R3-Q12 · The load error · 1280
> R3-Q12 · 1280 · focus: Try again → the h1 (tabindex -1)
Wombat
Acting as Coordinator
MenuClose
Acting as Coordinator
- Home
- Programme trainees
- Waiting for assessors
- Decisions due
- MSF campaigns
- Committee reviews
- Decision panels
- Data rights requests
- My data rights
Pieter Smit
Sign out
### Data rights requests
Requests from people at Kgosi Kgari Teaching Hospital about their personal data: decide each, and carry an approved correction through.
Could not load the data rights requests. Nothing has changed. Try again, or come back in a few minutes.Try again

######## BOARD R3-Q01-390.dc.html — R3-Q01 · A.1.3 · Pieter Smit arrives: Anele Dlamini's row · 390
> R3-Q01 · 390 · focus: none (page load)
Wombat
Acting as Coordinator
MenuClose
Acting as Coordinator
- Home
- Programme trainees
- Waiting for assessors
- Decisions due
- MSF campaigns
- Committee reviews
- Decision panels
- Data rights requests
- My data rights
Pieter Smit
Sign out
### Data rights requests
Requests from people at Kgosi Kgari Teaching Hospital about their personal data: decide each, and carry an approved correction through.
[aria-label: Filter Data rights requests]
Type
All types
Access
Export
Rectification
Erasure
Status
All statuses
Submitted
Approved
Completed
Rejected
Withdrawn
Show
#### Requests (1)
Kgosi Kgari Teaching Hospital's data rights requests, newest first
| Submitted | Requester | Type | Status | Request id | Actions |
| 2026-10-28 00:01 SAST | Anele Dlaminidlamini@kgk.wombat.local | Export | Submitted | 01a17f61-f16f-77c1-860c-661586d5b733 | [aria-label: Review the Export request from Anele Dlamini, 2026-10-28 00:01 SAST] Review |

######## BOARD R3-Q02-390.dc.html — R3-Q02 · A.1.3 · Type Erasure, Show: nothing matches · 390
> R3-Q02 · 390 · focus: “Requests (0)”
Wombat
Acting as Coordinator
MenuClose
Acting as Coordinator
- Home
- Programme trainees
- Waiting for assessors
- Decisions due
- MSF campaigns
- Committee reviews
- Decision panels
- Data rights requests
- My data rights
Pieter Smit
Sign out
### Data rights requests
Requests from people at Kgosi Kgari Teaching Hospital about their personal data: decide each, and carry an approved correction through.
[aria-label: Filter Data rights requests]
Type
All types
Access
Export
Rectification
Erasure
Status
All statuses
Submitted
Approved
Completed
Rejected
Withdrawn
ShowClear filters
#### Requests (0)
No requests match
No Erasure request at Kgosi Kgari Teaching Hospital. Clear filters to see every request.

######## BOARD R3-Q03-390.dc.html — R3-Q03 · A.1.3 · Type Export and Status Submitted, Show: one row · 390
> R3-Q03 · 390 · focus: “Requests (1)”
Wombat
Acting as Coordinator
MenuClose
Acting as Coordinator
- Home
- Programme trainees
- Waiting for assessors
- Decisions due
- MSF campaigns
- Committee reviews
- Decision panels
- Data rights requests
- My data rights
Pieter Smit
Sign out
### Data rights requests
Requests from people at Kgosi Kgari Teaching Hospital about their personal data: decide each, and carry an approved correction through.
[aria-label: Filter Data rights requests]
Type
All types
Access
Export
Rectification
Erasure
Status
All statuses
Submitted
Approved
Completed
Rejected
Withdrawn
ShowClear filters
#### Requests (1)
Kgosi Kgari Teaching Hospital's data rights requests, newest first: Export, Submitted
| Submitted | Requester | Type | Status | Request id | Actions |
| 2026-10-28 00:01 SAST | Anele Dlaminidlamini@kgk.wombat.local | Export | Submitted | 01a17f61-f16f-77c1-860c-661586d5b733 | [aria-label: Review the Export request from Anele Dlamini, 2026-10-28 00:01 SAST] Review |

######## BOARD R3-Q04-390.dc.html — R3-Q04 · A.1.8 · both filters cleared, Show: three, newest first · 390
> R3-Q04 · 390 · focus: “Requests (3)”
Wombat
Acting as Coordinator
MenuClose
Acting as Coordinator
- Home
- Programme trainees
- Waiting for assessors
- Decisions due
- MSF campaigns
- Committee reviews
- Decision panels
- Data rights requests
- My data rights
Pieter Smit
Sign out
### Data rights requests
Requests from people at Kgosi Kgari Teaching Hospital about their personal data: decide each, and carry an approved correction through.
[aria-label: Filter Data rights requests]
Type
All types
Access
Export
Rectification
Erasure
Status
All statuses
Submitted
Approved
Completed
Rejected
Withdrawn
Show
#### Requests (3)
Kgosi Kgari Teaching Hospital's data rights requests, newest first
| Submitted | Requester | Type | Status | Request id | Actions |
| 2026-10-28 00:03 SAST | Nomsa Mahlangumahlangu@kgk.wombat.local | Rectification | Submitted | 01a17f63-8c25-7c6e-8724-8593f3d7a145 | [aria-label: Review the Rectification request from Nomsa Mahlangu, 2026-10-28 00:03 SAST] Review |
| 2026-10-28 00:03 SAST | Pieter du Plessisduplessis@kgk.wombat.local | Access | Withdrawn | 01a17f63-7af4-7ac5-a6d1-d01726b57bc0 | [aria-label: Review the Access request from Pieter du Plessis, 2026-10-28 00:03 SAST] Review |
| 2026-10-28 00:01 SAST | Anele Dlaminidlamini@kgk.wombat.local | Export | Completed | 01a17f61-f16f-77c1-860c-661586d5b733 | [aria-label: Review the Export request from Anele Dlamini, 2026-10-28 00:01 SAST] Review |

######## BOARD R3-Q05-390.dc.html — R3-Q05 · A.1.12 · devadmin's queue, every institution's: five · 390
> R3-Q05 · 390 · focus: none (page load)
Wombat
Acting as Administrator
MenuClose
Acting as Administrator
- Home
Platform
- Scheduled jobs
- Audit log
- SSO mappings
- Data rights requests
Organisations
- Institutions
- Colleges
People
- Users
- Invitations
Catalogue
- EPAs
- Curricula
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- My data rights
Demo Administrator
Sign out
### Data rights requests
Requests from people at every institution about their personal data: decide each, and carry an approved correction through.
[aria-label: Filter Data rights requests]
Type
All types
Access
Export
Rectification
Erasure
Status
All statuses
Submitted
Approved
Completed
Rejected
Withdrawn
Show
#### Requests (5)
Every institution's data rights requests, newest first
| Submitted | Requester | Institution | Type | Status | Request id | Actions |
| 2026-10-28 00:04 SAST | Sipho Ndlovundlovu@kgk.wombat.local | Kgosi Kgari Teaching Hospital | Erasure | Submitted | 01a17f64-f566-75f0-91ba-5d71863b77d0 | [aria-label: Review the Erasure request from Sipho Ndlovu, 2026-10-28 00:04 SAST] Review |
| ‹time› SAST | Nomsa Mahlangumahlangu@kgk.wombat.local | Kgosi Kgari Teaching Hospital | Rectification | Completed | ‹request id› | [aria-label: Review the Rectification request from Nomsa Mahlangu, ‹time› SAST] Review |
| 2026-10-28 00:03 SAST | Nomsa Mahlangumahlangu@kgk.wombat.local | Kgosi Kgari Teaching Hospital | Rectification | Rejected | 01a17f63-8c25-7c6e-8724-8593f3d7a145 | [aria-label: Review the Rectification request from Nomsa Mahlangu, 2026-10-28 00:03 SAST] Review |
| 2026-10-28 00:03 SAST | Pieter du Plessisduplessis@kgk.wombat.local | Kgosi Kgari Teaching Hospital | Access | Withdrawn | 01a17f63-7af4-7ac5-a6d1-d01726b57bc0 | [aria-label: Review the Access request from Pieter du Plessis, 2026-10-28 00:03 SAST] Review |
| 2026-10-28 00:01 SAST | Anele Dlaminidlamini@kgk.wombat.local | Kgosi Kgari Teaching Hospital | Export | Completed | 01a17f61-f16f-77c1-860c-661586d5b733 | [aria-label: Review the Export request from Anele Dlamini, 2026-10-28 00:01 SAST] Review |

######## BOARD R3-Q06-390.dc.html — R3-Q06 · A.1.12 · devadmin, Type Erasure, Show: one row · 390
> R3-Q06 · 390 · focus: “Requests (1)”
Wombat
Acting as Administrator
MenuClose
Acting as Administrator
- Home
Platform
- Scheduled jobs
- Audit log
- SSO mappings
- Data rights requests
Organisations
- Institutions
- Colleges
People
- Users
- Invitations
Catalogue
- EPAs
- Curricula
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- My data rights
Demo Administrator
Sign out
### Data rights requests
Requests from people at every institution about their personal data: decide each, and carry an approved correction through.
[aria-label: Filter Data rights requests]
Type
All types
Access
Export
Rectification
Erasure
Status
All statuses
Submitted
Approved
Completed
Rejected
Withdrawn
ShowClear filters
#### Requests (1)
Every institution's data rights requests, newest first: Erasure
| Submitted | Requester | Institution | Type | Status | Request id | Actions |
| 2026-10-28 00:04 SAST | Sipho Ndlovundlovu@kgk.wombat.local | Kgosi Kgari Teaching Hospital | Erasure | Submitted | 01a17f64-f566-75f0-91ba-5d71863b77d0 | [aria-label: Review the Erasure request from Sipho Ndlovu, 2026-10-28 00:04 SAST] Review |

######## BOARD R3-Q07-390.dc.html — R3-Q07 · A.1.12 · the same after the erasure: the pseudonym · 390
> R3-Q07 · 390 · focus: none (back from the request)
Wombat
Acting as Administrator
MenuClose
Acting as Administrator
- Home
Platform
- Scheduled jobs
- Audit log
- SSO mappings
- Data rights requests
Organisations
- Institutions
- Colleges
People
- Users
- Invitations
Catalogue
- EPAs
- Curricula
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- My data rights
Demo Administrator
Sign out
### Data rights requests
Requests from people at every institution about their personal data: decide each, and carry an approved correction through.
[aria-label: Filter Data rights requests]
Type
All types
Access
Export
Rectification
Erasure
Status
All statuses
Submitted
Approved
Completed
Rejected
Withdrawn
ShowClear filters
#### Requests (1)
Every institution's data rights requests, newest first: Erasure
| Submitted | Requester | Institution | Type | Status | Request id | Actions |
| 2026-10-28 00:04 SAST | deleted_user_0ae1d64bErased 2026-10-28 | Kgosi Kgari Teaching Hospital | Erasure | Completed | 01a17f64-f566-75f0-91ba-5d71863b77d0 | [aria-label: Review the Erasure request from deleted_user_0ae1d64b, 2026-10-28 00:04 SAST] Review |

######## BOARD R3-Q08-390.dc.html — R3-Q08 · Pieter Smit's queue after the erasure: the kept row among KGK's · 390
> R3-Q08 · 390 · focus: none (page load)
Wombat
Acting as Coordinator
MenuClose
Acting as Coordinator
- Home
- Programme trainees
- Waiting for assessors
- Decisions due
- MSF campaigns
- Committee reviews
- Decision panels
- Data rights requests
- My data rights
Pieter Smit
Sign out
### Data rights requests
Requests from people at Kgosi Kgari Teaching Hospital about their personal data: decide each, and carry an approved correction through.
[aria-label: Filter Data rights requests]
Type
All types
Access
Export
Rectification
Erasure
Status
All statuses
Submitted
Approved
Completed
Rejected
Withdrawn
Show
#### Requests (5)
Kgosi Kgari Teaching Hospital's data rights requests, newest first
| Submitted | Requester | Type | Status | Request id | Actions |
| 2026-10-28 00:04 SAST | deleted_user_0ae1d64bErased 2026-10-28 | Erasure | Completed | 01a17f64-f566-75f0-91ba-5d71863b77d0 | [aria-label: Review the Erasure request from deleted_user_0ae1d64b, 2026-10-28 00:04 SAST] Review |
| ‹time› SAST | Nomsa Mahlangumahlangu@kgk.wombat.local | Rectification | Completed | ‹request id› | [aria-label: Review the Rectification request from Nomsa Mahlangu, ‹time› SAST] Review |
| 2026-10-28 00:03 SAST | Nomsa Mahlangumahlangu@kgk.wombat.local | Rectification | Rejected | 01a17f63-8c25-7c6e-8724-8593f3d7a145 | [aria-label: Review the Rectification request from Nomsa Mahlangu, 2026-10-28 00:03 SAST] Review |
| 2026-10-28 00:03 SAST | Pieter du Plessisduplessis@kgk.wombat.local | Access | Withdrawn | 01a17f63-7af4-7ac5-a6d1-d01726b57bc0 | [aria-label: Review the Access request from Pieter du Plessis, 2026-10-28 00:03 SAST] Review |
| 2026-10-28 00:01 SAST | Anele Dlaminidlamini@kgk.wombat.local | Export | Completed | 01a17f61-f16f-77c1-860c-661586d5b733 | [aria-label: Review the Export request from Anele Dlamini, 2026-10-28 00:01 SAST] Review |

######## BOARD R3-Q09-390.dc.html — R3-Q09 · Fifty across four institutions, page 1 of 3 · 390 · Illustration
> IllustrationR3-Q09 · 390 · focus: none · a 60-character address, “None”, an Approved row
Wombat
Acting as Administrator
MenuClose
Acting as Administrator
- Home
Platform
- Scheduled jobs
- Audit log
- SSO mappings
- Data rights requests
Organisations
- Institutions
- Colleges
People
- Users
- Invitations
Catalogue
- EPAs
- Curricula
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- My data rights
Demo Administrator
Sign out
### Data rights requests
Requests from people at every institution about their personal data: decide each, and carry an approved correction through.
[aria-label: Filter Data rights requests]
Type
All types
Access
Export
Rectification
Erasure
Status
All statuses
Submitted
Approved
Completed
Rejected
Withdrawn
Show
#### Requests (50)
Every institution's data rights requests, newest first
| Submitted | Requester | Institution | Type | Status | Request id | Actions |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Access | Submitted | ‹request id› | [aria-label: Review the Access request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx@‹domain› | ‹institution› | Export | Completed | ‹request id› | [aria-label: Review the Export request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | None | Rectification | Rejected | ‹request id› | [aria-label: Review the Rectification request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Rectification | Approved | ‹request id› | [aria-label: Review the Rectification request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Access | Completed | ‹request id› | [aria-label: Review the Access request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Export | Submitted | ‹request id› | [aria-label: Review the Export request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Rectification | Completed | ‹request id› | [aria-label: Review the Rectification request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Erasure | Rejected | ‹request id› | [aria-label: Review the Erasure request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Access | Withdrawn | ‹request id› | [aria-label: Review the Access request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Export | Completed | ‹request id› | [aria-label: Review the Export request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Rectification | Submitted | ‹request id› | [aria-label: Review the Rectification request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Erasure | Completed | ‹request id› | [aria-label: Review the Erasure request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Access | Rejected | ‹request id› | [aria-label: Review the Access request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Export | Withdrawn | ‹request id› | [aria-label: Review the Export request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Rectification | Completed | ‹request id› | [aria-label: Review the Rectification request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Erasure | Submitted | ‹request id› | [aria-label: Review the Erasure request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Access | Completed | ‹request id› | [aria-label: Review the Access request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Export | Rejected | ‹request id› | [aria-label: Review the Export request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Rectification | Withdrawn | ‹request id› | [aria-label: Review the Rectification request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Erasure | Completed | ‹request id› | [aria-label: Review the Erasure request from ‹name›, ‹date› ‹time› SAST] Review |
Showing 1–20 of 50
[aria-disabled] PreviousNext
Requests per page
10
20
50
100

######## BOARD R3-Q10-390.dc.html — R3-Q10 · Page 2 reached by Next: the focus on “Requests (50)” · 390 · Illustration
> IllustrationR3-Q10 · 390 · focus: “Requests (50)”
Wombat
Acting as Administrator
MenuClose
Acting as Administrator
- Home
Platform
- Scheduled jobs
- Audit log
- SSO mappings
- Data rights requests
Organisations
- Institutions
- Colleges
People
- Users
- Invitations
Catalogue
- EPAs
- Curricula
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- My data rights
Demo Administrator
Sign out
### Data rights requests
Requests from people at every institution about their personal data: decide each, and carry an approved correction through.
[aria-label: Filter Data rights requests]
Type
All types
Access
Export
Rectification
Erasure
Status
All statuses
Submitted
Approved
Completed
Rejected
Withdrawn
Show
#### Requests (50)
Every institution's data rights requests, newest first
| Submitted | Requester | Institution | Type | Status | Request id | Actions |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Access | Submitted | ‹request id› | [aria-label: Review the Access request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Export | Completed | ‹request id› | [aria-label: Review the Export request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Rectification | Rejected | ‹request id› | [aria-label: Review the Rectification request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Erasure | Withdrawn | ‹request id› | [aria-label: Review the Erasure request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Access | Completed | ‹request id› | [aria-label: Review the Access request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Export | Submitted | ‹request id› | [aria-label: Review the Export request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Rectification | Completed | ‹request id› | [aria-label: Review the Rectification request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Erasure | Rejected | ‹request id› | [aria-label: Review the Erasure request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Access | Withdrawn | ‹request id› | [aria-label: Review the Access request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Export | Completed | ‹request id› | [aria-label: Review the Export request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Rectification | Submitted | ‹request id› | [aria-label: Review the Rectification request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Erasure | Completed | ‹request id› | [aria-label: Review the Erasure request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Access | Rejected | ‹request id› | [aria-label: Review the Access request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Export | Withdrawn | ‹request id› | [aria-label: Review the Export request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Rectification | Completed | ‹request id› | [aria-label: Review the Rectification request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Erasure | Submitted | ‹request id› | [aria-label: Review the Erasure request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Access | Completed | ‹request id› | [aria-label: Review the Access request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Export | Rejected | ‹request id› | [aria-label: Review the Export request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Rectification | Withdrawn | ‹request id› | [aria-label: Review the Rectification request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Erasure | Completed | ‹request id› | [aria-label: Review the Erasure request from ‹name›, ‹date› ‹time› SAST] Review |
Showing 21–40 of 50
PreviousNext
Requests per page
10
20
50
100

######## BOARD R3-Q11-390.dc.html — R3-Q11 · Loading · 390
> R3-Q11 · 390 · focus: none
Wombat
Acting as Coordinator
MenuClose
Acting as Coordinator
- Home
- Programme trainees
- Waiting for assessors
- Decisions due
- MSF campaigns
- Committee reviews
- Decision panels
- Data rights requests
- My data rights
Pieter Smit
Sign out
### Data rights requests
Requests from people at Kgosi Kgari Teaching Hospital about their personal data: decide each, and carry an approved correction through.
Loading the data rights requests.

######## BOARD R3-Q12-390.dc.html — R3-Q12 · The load error · 390
> R3-Q12 · 390 · focus: Try again → the h1 (tabindex -1)
Wombat
Acting as Coordinator
MenuClose
Acting as Coordinator
- Home
- Programme trainees
- Waiting for assessors
- Decisions due
- MSF campaigns
- Committee reviews
- Decision panels
- Data rights requests
- My data rights
Pieter Smit
Sign out
### Data rights requests
Requests from people at Kgosi Kgari Teaching Hospital about their personal data: decide each, and carry an approved correction through.
Could not load the data rights requests. Nothing has changed. Try again, or come back in a few minutes.Try again

######## BOARD R3-Q13-390.dc.html — R3-Q13 · A.7.5 · Pieter Smit's queue at 390 after the erasure · 390
> R3-Q13 · 390 · focus: none · filters at their defaults: no Clear filters, no pager
Wombat
Acting as Coordinator
MenuClose
Acting as Coordinator
- Home
- Programme trainees
- Waiting for assessors
- Decisions due
- MSF campaigns
- Committee reviews
- Decision panels
- Data rights requests
- My data rights
Pieter Smit
Sign out
### Data rights requests
Requests from people at Kgosi Kgari Teaching Hospital about their personal data: decide each, and carry an approved correction through.
[aria-label: Filter Data rights requests]
Type
All types
Access
Export
Rectification
Erasure
Status
All statuses
Submitted
Approved
Completed
Rejected
Withdrawn
Show
#### Requests (5)
Kgosi Kgari Teaching Hospital's data rights requests, newest first
| Submitted | Requester | Type | Status | Request id | Actions |
| 2026-10-28 00:04 SAST | deleted_user_0ae1d64bErased 2026-10-28 | Erasure | Completed | 01a17f64-f566-75f0-91ba-5d71863b77d0 | [aria-label: Review the Erasure request from deleted_user_0ae1d64b, 2026-10-28 00:04 SAST] Review |
| ‹time› SAST | Nomsa Mahlangumahlangu@kgk.wombat.local | Rectification | Completed | ‹request id› | [aria-label: Review the Rectification request from Nomsa Mahlangu, ‹time› SAST] Review |
| 2026-10-28 00:03 SAST | Nomsa Mahlangumahlangu@kgk.wombat.local | Rectification | Rejected | 01a17f63-8c25-7c6e-8724-8593f3d7a145 | [aria-label: Review the Rectification request from Nomsa Mahlangu, 2026-10-28 00:03 SAST] Review |
| 2026-10-28 00:03 SAST | Pieter du Plessisduplessis@kgk.wombat.local | Access | Withdrawn | 01a17f63-7af4-7ac5-a6d1-d01726b57bc0 | [aria-label: Review the Access request from Pieter du Plessis, 2026-10-28 00:03 SAST] Review |
| 2026-10-28 00:01 SAST | Anele Dlaminidlamini@kgk.wombat.local | Export | Completed | 01a17f61-f16f-77c1-860c-661586d5b733 | [aria-label: Review the Export request from Anele Dlamini, 2026-10-28 00:01 SAST] Review |

######## BOARD R3-Q04-768.dc.html — R3-Q04 · A.1.8 · both filters cleared, Show: three, newest first · 768
> R3-Q04 · 768 · focus: “Requests (3)”
Wombat
Acting as Coordinator
MenuClose
Acting as Coordinator
- Home
- Programme trainees
- Waiting for assessors
- Decisions due
- MSF campaigns
- Committee reviews
- Decision panels
- Data rights requests
- My data rights
Pieter Smit
Sign out
### Data rights requests
Requests from people at Kgosi Kgari Teaching Hospital about their personal data: decide each, and carry an approved correction through.
[aria-label: Filter Data rights requests]
Type
All types
Access
Export
Rectification
Erasure
Status
All statuses
Submitted
Approved
Completed
Rejected
Withdrawn
Show
#### Requests (3)
Kgosi Kgari Teaching Hospital's data rights requests, newest first
| Submitted | Requester | Type | Status | Request id | Actions |
| 2026-10-28 00:03 SAST | Nomsa Mahlangumahlangu@kgk.wombat.local | Rectification | Submitted | 01a17f63-8c25-7c6e-8724-8593f3d7a145 | [aria-label: Review the Rectification request from Nomsa Mahlangu, 2026-10-28 00:03 SAST] Review |
| 2026-10-28 00:03 SAST | Pieter du Plessisduplessis@kgk.wombat.local | Access | Withdrawn | 01a17f63-7af4-7ac5-a6d1-d01726b57bc0 | [aria-label: Review the Access request from Pieter du Plessis, 2026-10-28 00:03 SAST] Review |
| 2026-10-28 00:01 SAST | Anele Dlaminidlamini@kgk.wombat.local | Export | Completed | 01a17f61-f16f-77c1-860c-661586d5b733 | [aria-label: Review the Export request from Anele Dlamini, 2026-10-28 00:01 SAST] Review |

######## BOARD R3-Q09-768.dc.html — R3-Q09 · Fifty across four institutions, page 1 of 3 · 768 · Illustration
> IllustrationR3-Q09 · 768 · focus: none · a 60-character address, “None”, an Approved row
Wombat
Acting as Administrator
MenuClose
Acting as Administrator
- Home
Platform
- Scheduled jobs
- Audit log
- SSO mappings
- Data rights requests
Organisations
- Institutions
- Colleges
People
- Users
- Invitations
Catalogue
- EPAs
- Curricula
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- My data rights
Demo Administrator
Sign out
### Data rights requests
Requests from people at every institution about their personal data: decide each, and carry an approved correction through.
[aria-label: Filter Data rights requests]
Type
All types
Access
Export
Rectification
Erasure
Status
All statuses
Submitted
Approved
Completed
Rejected
Withdrawn
Show
#### Requests (50)
Every institution's data rights requests, newest first
| Submitted | Requester | Institution | Type | Status | Request id | Actions |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Access | Submitted | ‹request id› | [aria-label: Review the Access request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx@‹domain› | ‹institution› | Export | Completed | ‹request id› | [aria-label: Review the Export request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | None | Rectification | Rejected | ‹request id› | [aria-label: Review the Rectification request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Rectification | Approved | ‹request id› | [aria-label: Review the Rectification request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Access | Completed | ‹request id› | [aria-label: Review the Access request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Export | Submitted | ‹request id› | [aria-label: Review the Export request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Rectification | Completed | ‹request id› | [aria-label: Review the Rectification request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Erasure | Rejected | ‹request id› | [aria-label: Review the Erasure request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Access | Withdrawn | ‹request id› | [aria-label: Review the Access request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Export | Completed | ‹request id› | [aria-label: Review the Export request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Rectification | Submitted | ‹request id› | [aria-label: Review the Rectification request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Erasure | Completed | ‹request id› | [aria-label: Review the Erasure request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Access | Rejected | ‹request id› | [aria-label: Review the Access request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Export | Withdrawn | ‹request id› | [aria-label: Review the Export request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Rectification | Completed | ‹request id› | [aria-label: Review the Rectification request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Erasure | Submitted | ‹request id› | [aria-label: Review the Erasure request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Access | Completed | ‹request id› | [aria-label: Review the Access request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Export | Rejected | ‹request id› | [aria-label: Review the Export request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Rectification | Withdrawn | ‹request id› | [aria-label: Review the Rectification request from ‹name›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | ‹name›‹name›@‹domain› | ‹institution› | Erasure | Completed | ‹request id› | [aria-label: Review the Erasure request from ‹name›, ‹date› ‹time› SAST] Review |
Showing 1–20 of 50
[aria-disabled] PreviousNext
Requests per page
10
20
50
100

######## BOARD R3-D01-1280.dc.html — R3-D01 · A.1.3 · Anele Dlamini's Export, Submitted, with Decide · 1280
> R3-D01 · 1280 · focus: none (page load)
Wombat
Acting as Coordinator
MenuClose
Acting as Coordinator
- Home
- Programme trainees
- Waiting for assessors
- Decisions due
- MSF campaigns
- Committee reviews
- Decision panels
- Data rights requests
- My data rights
Pieter Smit
Sign out
- Home
- Data rights requests
- 01a17f61-f16f-77c1-860c-661586d5b733
### Export request from Anele Dlamini
Submitted 2026-10-28 00:01 SAST · Kgosi Kgari Teaching Hospital
#### Request
**Requester
Anele Dlamini
**Email
dlamini@kgk.wombat.local
**User id
67083efa-bbaa-48d5-a753-b01fdf38530d
**Request id
01a17f61-f16f-77c1-860c-661586d5b733
**Type
Export
**Status
Submitted
**Reason
A copy of my training record for my own files.
#### Decide
Approving makes the export ready at once: Anele Dlamini downloads it from My data rights, and it is built at that moment. You cannot download it yourself.
Decision note *required
Anele Dlamini reads this note with the decision on My data rights, and is emailed that it was decided. Who decided is not shown.
RejectApprove

######## BOARD R3-D02-1280.dc.html — R3-D02 · A.1.3 · Approve with no note: refused under the note · 1280
> R3-D02 · 1280 · focus: the note (refused)
Wombat
Acting as Coordinator
MenuClose
Acting as Coordinator
- Home
- Programme trainees
- Waiting for assessors
- Decisions due
- MSF campaigns
- Committee reviews
- Decision panels
- Data rights requests
- My data rights
Pieter Smit
Sign out
- Home
- Data rights requests
- 01a17f61-f16f-77c1-860c-661586d5b733
### Export request from Anele Dlamini
Submitted 2026-10-28 00:01 SAST · Kgosi Kgari Teaching Hospital
#### Request
**Requester
Anele Dlamini
**Email
dlamini@kgk.wombat.local
**User id
67083efa-bbaa-48d5-a753-b01fdf38530d
**Request id
01a17f61-f16f-77c1-860c-661586d5b733
**Type
Export
**Status
Submitted
**Reason
A copy of my training record for my own files.
#### Decide
Approving makes the export ready at once: Anele Dlamini downloads it from My data rights, and it is built at that moment. You cannot download it yourself.
Decision note *required
A decision note is required.
Anele Dlamini reads this note with the decision on My data rights, and is emailed that it was decided. Who decided is not shown.
RejectApprove

######## BOARD R3-D03-1280.dc.html — R3-D03 · A.1.3 · approved: Completed at once · 1280
> R3-D03 · 1280 · focus: the result, top of Decision
Wombat
Acting as Coordinator
MenuClose
Acting as Coordinator
- Home
- Programme trainees
- Waiting for assessors
- Decisions due
- MSF campaigns
- Committee reviews
- Decision panels
- Data rights requests
- My data rights
Pieter Smit
Sign out
- Home
- Data rights requests
- 01a17f61-f16f-77c1-860c-661586d5b733
### Export request from Anele Dlamini
Submitted 2026-10-28 00:01 SAST · Kgosi Kgari Teaching Hospital
#### Request
**Requester
Anele Dlamini
**Email
dlamini@kgk.wombat.local
**User id
67083efa-bbaa-48d5-a753-b01fdf38530d
**Request id
01a17f61-f16f-77c1-860c-661586d5b733
**Type
Export
**Status
Completed
**Reason
A copy of my training record for my own files.
#### Decision
Request approved. Anele Dlamini is emailed, and can download the export from My data rights.
**Decision
Approved
**Decided by
Pieter Smit
43ad0cb7-5ce6-46d7-8595-7117d130c7dc
**Decided on
2026-10-28 00:01 SAST
**Note
Identity confirmed; export released to the data subject.
**Completed on
2026-10-28 00:01 SAST

######## BOARD R3-D04-1280.dc.html — R3-D04 · A.1.8 · Pieter du Plessis's Access, withdrawn: nothing to decide · 1280
> R3-D04 · 1280 · focus: none (page load)
Wombat
Acting as Coordinator
MenuClose
Acting as Coordinator
- Home
- Programme trainees
- Waiting for assessors
- Decisions due
- MSF campaigns
- Committee reviews
- Decision panels
- Data rights requests
- My data rights
Pieter Smit
Sign out
- Home
- Data rights requests
- 01a17f63-7af4-7ac5-a6d1-d01726b57bc0
### Access request from Pieter du Plessis
Submitted 2026-10-28 00:03 SAST · Kgosi Kgari Teaching Hospital
#### Request
**Requester
Pieter du Plessis
**Email
duplessis@kgk.wombat.local
**User id
6b844c8a-2a79-4f85-a118-28a321709844
**Request id
01a17f63-7af4-7ac5-a6d1-d01726b57bc0
**Type
Access
**Status
Withdrawn
**Reason
Everything held about me.
Pieter du Plessis withdrew this request before it was decided. There is nothing to decide.

######## BOARD R3-D05-1280.dc.html — R3-D05 · A.1.8 · Nomsa Mahlangu's programme-start Rectification rejected · 1280
> R3-D05 · 1280 · focus: the result, top of Decision
Wombat
Acting as Coordinator
MenuClose
Acting as Coordinator
- Home
- Programme trainees
- Waiting for assessors
- Decisions due
- MSF campaigns
- Committee reviews
- Decision panels
- Data rights requests
- My data rights
Pieter Smit
Sign out
- Home
- Data rights requests
- 01a17f63-8c25-7c6e-8724-8593f3d7a145
### Rectification request from Nomsa Mahlangu
Submitted 2026-10-28 00:03 SAST · Kgosi Kgari Teaching Hospital
#### Request
**Requester
Nomsa Mahlangu
**Email
mahlangu@kgk.wombat.local
**User id
d04f4dfb-f855-4345-9efa-edd168d5a2b6
**Request id
01a17f63-8c25-7c6e-8724-8593f3d7a145
**Type
Rectification
**Status
Rejected
**Reason
My programme start is recorded as 15 January 2026; I started work on 19 January.
#### Decision
Request rejected. Nomsa Mahlangu is emailed, and reads your note on My data rights.
**Decision
Rejected
**Decided by
Pieter Smit
43ad0cb7-5ce6-46d7-8595-7117d130c7dc
**Decided on
2026-10-28 00:03 SAST
**Note
The whole January 2026 intake is admitted on 15 January, the semester boundary (D42). Nothing to correct.

######## BOARD R3-D06-1280.dc.html — R3-D06 · A.1.12 · devadmin on Sipho Ndlovu's Erasure, before approval · 1280
> R3-D06 · 1280 · focus: none (the note typed)
Wombat
Acting as Administrator
MenuClose
Acting as Administrator
- Home
Platform
- Scheduled jobs
- Audit log
- SSO mappings
- Data rights requests
Organisations
- Institutions
- Colleges
People
- Users
- Invitations
Catalogue
- EPAs
- Curricula
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- My data rights
Demo Administrator
Sign out
- Home
- Data rights requests
- 01a17f64-f566-75f0-91ba-5d71863b77d0
### Erasure request from Sipho Ndlovu
Submitted 2026-10-28 00:04 SAST · Kgosi Kgari Teaching Hospital
#### Request
**Requester
Sipho Ndlovu
**Email
ndlovu@kgk.wombat.local
**User id
4e5ce9ca-438b-4d98-8635-a7259638d7c9
**Request id
01a17f64-f566-75f0-91ba-5d71863b77d0
**Type
Erasure
**Status
Submitted
**Reason
I am leaving the programme and want my personal data removed.
#### Decide
Approving erases Sipho Ndlovu's personal data at once and cannot be undone. You are asked to confirm, with what it ends and keeps.
Decision note *required
Identity confirmed. Programme records are kept under a pseudonym.
Kept with the request. Sipho Ndlovu will not read it: approving ends the account.
RejectApprove and erase…

######## BOARD R3-D07-1280.dc.html — R3-D07 · A.1.12 · the dialog, opening on Cancel · 1280
> R3-D07 · 1280 · focus: Cancel (in the dialog)
Wombat
Acting as Administrator
MenuClose
Acting as Administrator
- Home
Platform
- Scheduled jobs
- Audit log
- SSO mappings
- Data rights requests
Organisations
- Institutions
- Colleges
People
- Users
- Invitations
Catalogue
- EPAs
- Curricula
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- My data rights
Demo Administrator
Sign out
- Home
- Data rights requests
- 01a17f64-f566-75f0-91ba-5d71863b77d0
### Erasure request from Sipho Ndlovu
Submitted 2026-10-28 00:04 SAST · Kgosi Kgari Teaching Hospital
#### Request
**Requester
Sipho Ndlovu
**Email
ndlovu@kgk.wombat.local
**User id
4e5ce9ca-438b-4d98-8635-a7259638d7c9
**Request id
01a17f64-f566-75f0-91ba-5d71863b77d0
**Type
Erasure
**Status
Submitted
**Reason
I am leaving the programme and want my personal data removed.
#### Decide
Approving erases Sipho Ndlovu's personal data at once and cannot be undone. You are asked to confirm, with what it ends and keeps.
Decision note *required
Identity confirmed. Programme records are kept under a pseudonym.
Kept with the request. Sipho Ndlovu will not read it: approving ends the account.
RejectApprove and erase…
#### Erase Sipho Ndlovu's personal data?
This cannot be undone.
Approving erases it now, in one step, and completes the request.
It ends:
- The account and its sign-in. Sipho Ndlovu can no longer sign in, and an open session ends within a minute.
- 1 open committee review: Formative check-in #8, Paed Annual Review Panel, Scheduled. It is withdrawn, and nothing more is decided at it.
- No MSF campaign: none about Sipho Ndlovu is open.
- The programme, Paediatric EPA Curriculum (11.2), which ends today as Withdrawn.
It keeps, under a pseudonym that names nobody: the ratified review #5, closed check-in #6, the activities, progress and audit entries, and this request.
CancelErase and approve

######## BOARD R3-D08-1280.dc.html — R3-D08 · A.1.12 · Cancel: nothing changed · 1280
> R3-D08 · 1280 · focus: “Approve and erase…” (Cancel and Escape alike)
Wombat
Acting as Administrator
MenuClose
Acting as Administrator
- Home
Platform
- Scheduled jobs
- Audit log
- SSO mappings
- Data rights requests
Organisations
- Institutions
- Colleges
People
- Users
- Invitations
Catalogue
- EPAs
- Curricula
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- My data rights
Demo Administrator
Sign out
- Home
- Data rights requests
- 01a17f64-f566-75f0-91ba-5d71863b77d0
### Erasure request from Sipho Ndlovu
Submitted 2026-10-28 00:04 SAST · Kgosi Kgari Teaching Hospital
#### Request
**Requester
Sipho Ndlovu
**Email
ndlovu@kgk.wombat.local
**User id
4e5ce9ca-438b-4d98-8635-a7259638d7c9
**Request id
01a17f64-f566-75f0-91ba-5d71863b77d0
**Type
Erasure
**Status
Submitted
**Reason
I am leaving the programme and want my personal data removed.
#### Decide
Approving erases Sipho Ndlovu's personal data at once and cannot be undone. You are asked to confirm, with what it ends and keeps.
Decision note *required
Identity confirmed. Programme records are kept under a pseudonym.
Kept with the request. Sipho Ndlovu will not read it: approving ends the account.
RejectApprove and erase…

######## BOARD R3-D09-1280.dc.html — R3-D09 · A.1.12 · Erase and approve: erased, approved, completed · 1280
> R3-D09 · 1280 · focus: the result, top of Decision
Wombat
Acting as Administrator
MenuClose
Acting as Administrator
- Home
Platform
- Scheduled jobs
- Audit log
- SSO mappings
- Data rights requests
Organisations
- Institutions
- Colleges
People
- Users
- Invitations
Catalogue
- EPAs
- Curricula
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- My data rights
Demo Administrator
Sign out
- Home
- Data rights requests
- 01a17f64-f566-75f0-91ba-5d71863b77d0
### Erasure request from deleted_user_0ae1d64b
Submitted 2026-10-28 00:04 SAST · Kgosi Kgari Teaching Hospital
This person's personal data was erased on 2026-10-28, at this request. The request is kept as the record of it, under the pseudonym.
#### Request
**Requester
deleted_user_0ae1d64bErased 2026-10-28
**User id
4e5ce9ca-438b-4d98-8635-a7259638d7c9
**Request id
01a17f64-f566-75f0-91ba-5d71863b77d0
**Type
Erasure
**Status
Completed
**Reason
I am leaving the programme and want my personal data removed.
#### Decision
The personal data is erased. The request is approved and completed. 1 committee review was withdrawn and the programme ended today. The account is now deleted_user_0ae1d64b; its open session ends within a minute.
**Decision
Approved
**Decided by
Demo Administrator
abd4ed66-e9ad-4187-b87f-c3c481d05c30
**Decided on
2026-10-28 00:04 SAST
**Note
Identity confirmed. Programme records are kept under a pseudonym.
**Completed on
2026-10-28 00:04 SAST

######## BOARD R3-D10-1280.dc.html — R3-D10 · The PseudonymSalt refusal after the dialog closes · 1280 · Illustration
> IllustrationR3-D10 · 1280 · focus: the refusal alert
Wombat
Acting as Administrator
MenuClose
Acting as Administrator
- Home
Platform
- Scheduled jobs
- Audit log
- SSO mappings
- Data rights requests
Organisations
- Institutions
- Colleges
People
- Users
- Invitations
Catalogue
- EPAs
- Curricula
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- My data rights
Demo Administrator
Sign out
- Home
- Data rights requests
- 01a17f64-f566-75f0-91ba-5d71863b77d0
### Erasure request from Sipho Ndlovu
Submitted 2026-10-28 00:04 SAST · Kgosi Kgari Teaching Hospital
#### Request
**Requester
Sipho Ndlovu
**Email
ndlovu@kgk.wombat.local
**User id
4e5ce9ca-438b-4d98-8635-a7259638d7c9
**Request id
01a17f64-f566-75f0-91ba-5d71863b77d0
**Type
Erasure
**Status
Submitted
**Reason
I am leaving the programme and want my personal data removed.
#### Decide
Nothing has been erased. Erasure is not set up on this server. The request is still Submitted. Ask whoever runs the server to set it up, then approve again.
Approving erases Sipho Ndlovu's personal data at once and cannot be undone. You are asked to confirm, with what it ends and keeps.
Decision note *required
Identity confirmed. Programme records are kept under a pseudonym.
Kept with the request. Sipho Ndlovu will not read it: approving ends the account.
RejectApprove and erase…

######## BOARD R3-D11-1280.dc.html — R3-D11 · The concurrent-change refusal · 1280 · Illustration
> IllustrationR3-D11 · 1280 · focus: the refusal alert
Wombat
Acting as Administrator
MenuClose
Acting as Administrator
- Home
Platform
- Scheduled jobs
- Audit log
- SSO mappings
- Data rights requests
Organisations
- Institutions
- Colleges
People
- Users
- Invitations
Catalogue
- EPAs
- Curricula
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- My data rights
Demo Administrator
Sign out
- Home
- Data rights requests
- 01a17f64-f566-75f0-91ba-5d71863b77d0
### Erasure request from Sipho Ndlovu
Submitted 2026-10-28 00:04 SAST · Kgosi Kgari Teaching Hospital
#### Request
**Requester
Sipho Ndlovu
**Email
ndlovu@kgk.wombat.local
**User id
4e5ce9ca-438b-4d98-8635-a7259638d7c9
**Request id
01a17f64-f566-75f0-91ba-5d71863b77d0
**Type
Erasure
**Status
Submitted
**Reason
I am leaving the programme and want my personal data removed.
#### Decide
Something about this person changed while they were being erased: a review, a feedback campaign or their account was changed elsewhere at the same moment. Nothing has been erased, and the request has not been approved. Approve it again.
Approving erases Sipho Ndlovu's personal data at once and cannot be undone. You are asked to confirm, with what it ends and keeps.
Decision note *required
Identity confirmed. Programme records are kept under a pseudonym.
Kept with the request. Sipho Ndlovu will not read it: approving ends the account.
RejectApprove and erase…

######## BOARD R3-D12-1280.dc.html — R3-D12 · Withdrawn meanwhile: the press refused, the page as Withdrawn · 1280 · Illustration
> IllustrationR3-D12 · 1280 · focus: the refusal alert · Approve and Reject check Withdrawn first (never the domain's “Only submitted or under-review requests can be approved.”); the page reads the row again and draws it Withdrawn; the name read live (F1)
Wombat
Acting as Administrator
MenuClose
Acting as Administrator
- Home
Platform
- Scheduled jobs
- Audit log
- SSO mappings
- Data rights requests
Organisations
- Institutions
- Colleges
People
- Users
- Invitations
Catalogue
- EPAs
- Curricula
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- My data rights
Demo Administrator
Sign out
- Home
- Data rights requests
- 01a17f64-f566-75f0-91ba-5d71863b77d0
### Erasure request from Sipho Ndlovu
Submitted 2026-10-28 00:04 SAST · Kgosi Kgari Teaching Hospital
#### Request
**Requester
Sipho Ndlovu
**Email
ndlovu@kgk.wombat.local
**User id
4e5ce9ca-438b-4d98-8635-a7259638d7c9
**Request id
01a17f64-f566-75f0-91ba-5d71863b77d0
**Type
Erasure
**Status
Withdrawn
**Reason
I am leaving the programme and want my personal data removed.
Sipho Ndlovu withdrew this request. Nothing was decided or erased.

######## BOARD R3-D13-1280.dc.html — R3-D13 · Pieter Smit: the dialog for someone holding a staff role, a review elsewhere counted · 1280 · Illustration
> IllustrationR3-D13 · 1280 · focus: Cancel (in the dialog)
Wombat
Acting as Coordinator
MenuClose
Acting as Coordinator
- Home
- Programme trainees
- Waiting for assessors
- Decisions due
- MSF campaigns
- Committee reviews
- Decision panels
- Data rights requests
- My data rights
Pieter Smit
Sign out
- Home
- Data rights requests
- ‹request id›
### Erasure request from ‹name›
Submitted ‹date› ‹time› SAST · Kgosi Kgari Teaching Hospital
#### Request
**Requester
‹name›
**Email
‹address›
**User id
‹user id›
**Request id
‹request id›
**Type
Erasure
**Status
Submitted
**Reason
‹reason›
#### Decide
Approving erases ‹name›'s personal data at once and cannot be undone. You are asked to confirm, with what it ends and keeps.
Decision note *required
‹note›
Kept with the request. ‹name› will not read it: approving ends the account.
RejectApprove and erase…
#### Erase ‹name›'s personal data?
This cannot be undone.
Approving erases it now, in one step, and completes the request.
It ends:
- The account and its sign-in. ‹name› can no longer sign in, and an open session ends within a minute.
- The roles ‹roles› and their scopes.
- ‹n› open committee reviews: ‹reviews at Kgosi Kgari Teaching Hospital›, and 1 open review at another institution. Each is withdrawn, and nothing more is decided at it.
- ‹n› MSF campaigns not yet released: ‹campaigns›.
- ‹n› other open data rights requests: ‹requests›. Each is withdrawn.
- The programme, ‹curriculum›, which ends today as Withdrawn.
It keeps, under a pseudonym that names nobody: ‹what is settled›, and this request.
CancelErase and approve

######## BOARD R3-D14-1280.dc.html — R3-D14 · The dialog for a person with a review under appeal · 1280 · Illustration
> IllustrationR3-D14 · 1280 · focus: Cancel (in the dialog)
Wombat
Acting as Administrator
MenuClose
Acting as Administrator
- Home
Platform
- Scheduled jobs
- Audit log
- SSO mappings
- Data rights requests
Organisations
- Institutions
- Colleges
People
- Users
- Invitations
Catalogue
- EPAs
- Curricula
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- My data rights
Demo Administrator
Sign out
- Home
- Data rights requests
- ‹request id›
### Erasure request from ‹name›
Submitted ‹date› ‹time› SAST · ‹institution›
#### Request
**Requester
‹name›
**Email
‹address›
**User id
‹user id›
**Request id
‹request id›
**Type
Erasure
**Status
Submitted
**Reason
‹reason›
#### Decide
Approving erases ‹name›'s personal data at once and cannot be undone. You are asked to confirm, with what it ends and keeps.
Decision note *required
‹note›
Kept with the request. ‹name› will not read it: approving ends the account.
RejectApprove and erase…
#### Erase ‹name›'s personal data?
This cannot be undone.
Approving erases it now, in one step, and completes the request.
It ends:
- The account and its sign-in. ‹name› can no longer sign in, and an open session ends within a minute.
- ‹n› open committee reviews: ‹reviews›. Each is withdrawn, and nothing more is decided at it.
- ‹n› MSF campaigns not yet released: ‹campaigns›.
- The programme, ‹curriculum›, which ends today as Withdrawn.
It keeps, under a pseudonym that names nobody: ‹what is settled›, this request, and the review under appeal, which can still be answered (‹review›).
CancelErase and approve

######## BOARD R3-D15-1280.dc.html — R3-D15 · A reviewer's own request: no Decide · 1280 · Illustration
> IllustrationR3-D15 · 1280 · focus: none
Wombat
Acting as Coordinator
MenuClose
Acting as Coordinator
- Home
- Programme trainees
- Waiting for assessors
- Decisions due
- MSF campaigns
- Committee reviews
- Decision panels
- Data rights requests
- My data rights
‹name›
Sign out
- Home
- Data rights requests
- ‹request id›
### Export request from ‹name›
Submitted ‹date› ‹time› SAST · Kgosi Kgari Teaching Hospital
#### Request
**Requester
‹name›
**Email
‹address›
**User id
‹user id›
**Request id
‹request id›
**Type
Export
**Status
Submitted
**Reason
‹reason›
You cannot decide your own request: another Coordinator at Kgosi Kgari Teaching Hospital, or the Wombat administrator, decides it.

######## BOARD R3-D16-1280.dc.html — R3-D16 · A failed decision: the note kept · 1280 · Illustration
> IllustrationR3-D16 · 1280 · focus: the failure alert · fails A.1.3's Approve
Wombat
Acting as Coordinator
MenuClose
Acting as Coordinator
- Home
- Programme trainees
- Waiting for assessors
- Decisions due
- MSF campaigns
- Committee reviews
- Decision panels
- Data rights requests
- My data rights
Pieter Smit
Sign out
- Home
- Data rights requests
- 01a17f61-f16f-77c1-860c-661586d5b733
### Export request from Anele Dlamini
Submitted 2026-10-28 00:01 SAST · Kgosi Kgari Teaching Hospital
#### Request
**Requester
Anele Dlamini
**Email
dlamini@kgk.wombat.local
**User id
67083efa-bbaa-48d5-a753-b01fdf38530d
**Request id
01a17f61-f16f-77c1-860c-661586d5b733
**Type
Export
**Status
Submitted
**Reason
A copy of my training record for my own files.
#### Decide
The decision could not be saved just now. Nothing has changed. Try again, or come back in a few minutes.
Approving makes the export ready at once: Anele Dlamini downloads it from My data rights, and it is built at that moment. You cannot download it yourself.
Decision note *required
Identity confirmed; export released to the data subject.
Anele Dlamini reads this note with the decision on My data rights, and is emailed that it was decided. Who decided is not shown.
RejectApprove

######## BOARD R3-D17-1280.dc.html — R3-D17 · Loading: the trail with the id, a skeleton, no Decide · 1280
> R3-D17 · 1280 · focus: none · the read not yet back (not null, no exception)
Wombat
Acting as Coordinator
MenuClose
Acting as Coordinator
- Home
- Programme trainees
- Waiting for assessors
- Decisions due
- MSF campaigns
- Committee reviews
- Decision panels
- Data rights requests
- My data rights
Pieter Smit
Sign out
- Home
- Data rights requests
- 01a17f61-f16f-77c1-860c-661586d5b733
### Data rights request
Loading the request.

######## BOARD R3-D18-1280.dc.html — R3-D18 · The load error · 1280
> R3-D18 · 1280 · focus: Try again → the h1 (tabindex -1) · drawn when the read throws; never ex.Message
Wombat
Acting as Coordinator
MenuClose
Acting as Coordinator
- Home
- Programme trainees
- Waiting for assessors
- Decisions due
- MSF campaigns
- Committee reviews
- Decision panels
- Data rights requests
- My data rights
Pieter Smit
Sign out
- Home
- Data rights requests
- 01a17f61-f16f-77c1-860c-661586d5b733
### Data rights request
Could not load this request. Nothing has changed. Try again, or come back in a few minutes.Try again

######## BOARD R3-D19-1280.dc.html — R3-D19 · A 60-character address, a 4,000-character reason, a long note · 1280 · Illustration
> IllustrationR3-D19 · 1280 · focus: none · Pieter Smit as the decider of ‹name›'s request
Wombat
Acting as Coordinator
MenuClose
Acting as Coordinator
- Home
- Programme trainees
- Waiting for assessors
- Decisions due
- MSF campaigns
- Committee reviews
- Decision panels
- Data rights requests
- My data rights
Pieter Smit
Sign out
- Home
- Data rights requests
- ‹request id›
### Rectification request from ‹name›
Submitted ‹date› ‹time› SAST · Kgosi Kgari Teaching Hospital
#### Request
**Requester
‹name›
**Email
xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx@kgk.wombat.local
**User id
‹user id›
**Request id
‹request id›
**Type
Rectification
**Status
Rejected
**Reason
‹a reason of 4,000 characters, wrapping in its dd› ‹a reason of 4,000 characters, wrapping in its dd› ‹a reason of 4,000 characters, wrapping in its dd› ‹a reason of 4,000 characters, wrapping in its dd› ‹a reason of 4,000 characters, wrapping in its dd› ‹a reason of 4,000 characters, wrapping in its dd› ‹a reason of 4,000 characters, wrapping in its dd› ‹a reason of 4,000 characters, wrapping in its dd› ‹a reason of 4,000 characters, wrapping in its dd› ‹a reason of 4,000 characters, wrapping in its dd› ‹a reason of 4,000 characters, wrapping in its dd› ‹a reason of 4,000 characters, wrapping in its dd› ‹a reason of 4,000 characters, wrapping in its dd› ‹a reason of 4,000 characters, wrapping in its dd› ‹a reason of 4,000 characters, wrapping in its dd› ‹a reason of 4,000 characters, wrapping in its dd› ‹a reason of 4,000 characters, wrapping in its dd› ‹a reason of 4,000 characters, wrapping in its dd› ‹a reason of 4,000 characters, wrapping in its dd› ‹a reason of 4,000 characters, wrapping in its dd› ‹a reason of 4,000 characters, wrapping in its dd› ‹a reason of 4,000 characters, wrapping in its dd› ‹a reason of 4,000 characters, wrapping in its dd› ‹a reason of 4,000 characters, wrapping in its dd› ‹a reason of 4,000 characters, wrapping in its dd› ‹a reason of 4,000 characters, wrapping in its dd› ‹a reason of 4,000 characters, wrapping in its dd› ‹a reason of 4,000 characters, wrapping in its dd› ‹a reason of 4,000 characters, wrapping in its dd› ‹a reason of 4,000 characters, wrapping in its dd›
#### Decision
**Decision
Rejected
**Decided by
Pieter Smit
43ad0cb7-5ce6-46d7-8595-7117d130c7dc
**Decided on
‹date› ‹time› SAST
**Note
‹a long decision note› ‹a long decision note› ‹a long decision note› ‹a long decision note› ‹a long decision note› ‹a long decision note› ‹a long decision note› ‹a long decision note› ‹a long decision note› ‹a long decision note› ‹a long decision note› ‹a long decision note› ‹a long decision note› ‹a long decision note› ‹a long decision note› ‹a long decision note› ‹a long decision note› ‹a long decision note› ‹a long decision note› ‹a long decision note›

######## BOARD R3-D20-1280.dc.html — R3-D20 · devadmin's own Export request: no Decide · 1280 · Illustration
> IllustrationR3-D20 · 1280 · focus: none
Wombat
Acting as Administrator
MenuClose
Acting as Administrator
- Home
Platform
- Scheduled jobs
- Audit log
- SSO mappings
- Data rights requests
Organisations
- Institutions
- Colleges
People
- Users
- Invitations
Catalogue
- EPAs
- Curricula
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- My data rights
Demo Administrator
Sign out
- Home
- Data rights requests
- ‹request id›
### Export request from Demo Administrator
Submitted ‹date› ‹time› SAST
#### Request
**Requester
Demo Administrator
**Email
‹address›
**User id
abd4ed66-e9ad-4187-b87f-c3c481d05c30
**Request id
‹request id›
**Type
Export
**Status
Submitted
**Reason
‹reason›
You cannot decide your own request: another Administrator decides it.

######## BOARD R3-D21-1280.dc.html — R3-D21 · Pieter Smit: the erasure of the only Administrator, refused · 1280 · Illustration
> IllustrationR3-D21 · 1280 · focus: the refusal alert, after Erase and approve; still Submitted, the note kept
Wombat
Acting as Coordinator
MenuClose
Acting as Coordinator
- Home
- Programme trainees
- Waiting for assessors
- Decisions due
- MSF campaigns
- Committee reviews
- Decision panels
- Data rights requests
- My data rights
Pieter Smit
Sign out
- Home
- Data rights requests
- ‹request id›
### Erasure request from ‹name›
Submitted ‹date› ‹time› SAST · Kgosi Kgari Teaching Hospital
#### Request
**Requester
‹name›
**Email
‹address›
**User id
‹user id›
**Request id
‹request id›
**Type
Erasure
**Status
Submitted
**Reason
‹reason›
#### Decide
Nothing has been erased. ‹name› is Wombat's only Administrator, and erasing the account would leave nobody to run it. Add another Administrator first, then approve again.
Approving erases ‹name›'s personal data at once and cannot be undone. You are asked to confirm, with what it ends and keeps.
Decision note *required
‹note›
Kept with the request. ‹name› will not read it: approving ends the account.
RejectApprove and erase…

######## BOARD R3-D22-1280.dc.html — R3-D22 · An erasure with nothing else open: the result · 1280 · Illustration
> IllustrationR3-D22 · 1280 · focus: the result, top of Decision
Wombat
Acting as Administrator
MenuClose
Acting as Administrator
- Home
Platform
- Scheduled jobs
- Audit log
- SSO mappings
- Data rights requests
Organisations
- Institutions
- Colleges
People
- Users
- Invitations
Catalogue
- EPAs
- Curricula
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- My data rights
Demo Administrator
Sign out
- Home
- Data rights requests
- ‹request id›
### Erasure request from ‹pseudonym›
Submitted ‹date› ‹time› SAST · ‹institution›
This person's personal data was erased on ‹date›, at this request. The request is kept as the record of it, under the pseudonym.
#### Request
**Requester
‹pseudonym›Erased ‹date›
**User id
‹user id›
**Request id
‹request id›
**Type
Erasure
**Status
Completed
**Reason
‹reason›
#### Decision
The personal data is erased. The request is approved and completed. Nothing else was open to end. The account is now ‹pseudonym›; its open session ends within a minute.
**Decision
Approved
**Decided by
Demo Administrator
abd4ed66-e9ad-4187-b87f-c3c481d05c30
**Decided on
‹date› ‹time› SAST
**Note
‹note›
**Completed on
‹date› ‹time› SAST

######## BOARD R3-D01-390.dc.html — R3-D01 · A.1.3 · Anele Dlamini's Export, Submitted, with Decide · 390
> R3-D01 · 390 · focus: none (page load)
Wombat
Acting as Coordinator
MenuClose
Acting as Coordinator
- Home
- Programme trainees
- Waiting for assessors
- Decisions due
- MSF campaigns
- Committee reviews
- Decision panels
- Data rights requests
- My data rights
Pieter Smit
Sign out
- Home
- Data rights requests
- 01a17f61-f16f-77c1-860c-661586d5b733
### Export request from Anele Dlamini
Submitted 2026-10-28 00:01 SAST · Kgosi Kgari Teaching Hospital
#### Request
**Requester
Anele Dlamini
**Email
dlamini@kgk.wombat.local
**User id
67083efa-bbaa-48d5-a753-b01fdf38530d
**Request id
01a17f61-f16f-77c1-860c-661586d5b733
**Type
Export
**Status
Submitted
**Reason
A copy of my training record for my own files.
#### Decide
Approving makes the export ready at once: Anele Dlamini downloads it from My data rights, and it is built at that moment. You cannot download it yourself.
Decision note *required
Anele Dlamini reads this note with the decision on My data rights, and is emailed that it was decided. Who decided is not shown.
RejectApprove

######## BOARD R3-D02-390.dc.html — R3-D02 · A.1.3 · Approve with no note: refused under the note · 390
> R3-D02 · 390 · focus: the note (refused)
Wombat
Acting as Coordinator
MenuClose
Acting as Coordinator
- Home
- Programme trainees
- Waiting for assessors
- Decisions due
- MSF campaigns
- Committee reviews
- Decision panels
- Data rights requests
- My data rights
Pieter Smit
Sign out
- Home
- Data rights requests
- 01a17f61-f16f-77c1-860c-661586d5b733
### Export request from Anele Dlamini
Submitted 2026-10-28 00:01 SAST · Kgosi Kgari Teaching Hospital
#### Request
**Requester
Anele Dlamini
**Email
dlamini@kgk.wombat.local
**User id
67083efa-bbaa-48d5-a753-b01fdf38530d
**Request id
01a17f61-f16f-77c1-860c-661586d5b733
**Type
Export
**Status
Submitted
**Reason
A copy of my training record for my own files.
#### Decide
Approving makes the export ready at once: Anele Dlamini downloads it from My data rights, and it is built at that moment. You cannot download it yourself.
Decision note *required
A decision note is required.
Anele Dlamini reads this note with the decision on My data rights, and is emailed that it was decided. Who decided is not shown.
RejectApprove

######## BOARD R3-D03-390.dc.html — R3-D03 · A.1.3 · approved: Completed at once · 390
> R3-D03 · 390 · focus: the result, top of Decision
Wombat
Acting as Coordinator
MenuClose
Acting as Coordinator
- Home
- Programme trainees
- Waiting for assessors
- Decisions due
- MSF campaigns
- Committee reviews
- Decision panels
- Data rights requests
- My data rights
Pieter Smit
Sign out
- Home
- Data rights requests
- 01a17f61-f16f-77c1-860c-661586d5b733
### Export request from Anele Dlamini
Submitted 2026-10-28 00:01 SAST · Kgosi Kgari Teaching Hospital
#### Request
**Requester
Anele Dlamini
**Email
dlamini@kgk.wombat.local
**User id
67083efa-bbaa-48d5-a753-b01fdf38530d
**Request id
01a17f61-f16f-77c1-860c-661586d5b733
**Type
Export
**Status
Completed
**Reason
A copy of my training record for my own files.
#### Decision
Request approved. Anele Dlamini is emailed, and can download the export from My data rights.
**Decision
Approved
**Decided by
Pieter Smit
43ad0cb7-5ce6-46d7-8595-7117d130c7dc
**Decided on
2026-10-28 00:01 SAST
**Note
Identity confirmed; export released to the data subject.
**Completed on
2026-10-28 00:01 SAST

######## BOARD R3-D04-390.dc.html — R3-D04 · A.1.8 · Pieter du Plessis's Access, withdrawn: nothing to decide · 390
> R3-D04 · 390 · focus: none (page load)
Wombat
Acting as Coordinator
MenuClose
Acting as Coordinator
- Home
- Programme trainees
- Waiting for assessors
- Decisions due
- MSF campaigns
- Committee reviews
- Decision panels
- Data rights requests
- My data rights
Pieter Smit
Sign out
- Home
- Data rights requests
- 01a17f63-7af4-7ac5-a6d1-d01726b57bc0
### Access request from Pieter du Plessis
Submitted 2026-10-28 00:03 SAST · Kgosi Kgari Teaching Hospital
#### Request
**Requester
Pieter du Plessis
**Email
duplessis@kgk.wombat.local
**User id
6b844c8a-2a79-4f85-a118-28a321709844
**Request id
01a17f63-7af4-7ac5-a6d1-d01726b57bc0
**Type
Access
**Status
Withdrawn
**Reason
Everything held about me.
Pieter du Plessis withdrew this request before it was decided. There is nothing to decide.

######## BOARD R3-D05-390.dc.html — R3-D05 · A.1.8 · Nomsa Mahlangu's programme-start Rectification rejected · 390
> R3-D05 · 390 · focus: the result, top of Decision
Wombat
Acting as Coordinator
MenuClose
Acting as Coordinator
- Home
- Programme trainees
- Waiting for assessors
- Decisions due
- MSF campaigns
- Committee reviews
- Decision panels
- Data rights requests
- My data rights
Pieter Smit
Sign out
- Home
- Data rights requests
- 01a17f63-8c25-7c6e-8724-8593f3d7a145
### Rectification request from Nomsa Mahlangu
Submitted 2026-10-28 00:03 SAST · Kgosi Kgari Teaching Hospital
#### Request
**Requester
Nomsa Mahlangu
**Email
mahlangu@kgk.wombat.local
**User id
d04f4dfb-f855-4345-9efa-edd168d5a2b6
**Request id
01a17f63-8c25-7c6e-8724-8593f3d7a145
**Type
Rectification
**Status
Rejected
**Reason
My programme start is recorded as 15 January 2026; I started work on 19 January.
#### Decision
Request rejected. Nomsa Mahlangu is emailed, and reads your note on My data rights.
**Decision
Rejected
**Decided by
Pieter Smit
43ad0cb7-5ce6-46d7-8595-7117d130c7dc
**Decided on
2026-10-28 00:03 SAST
**Note
The whole January 2026 intake is admitted on 15 January, the semester boundary (D42). Nothing to correct.

######## BOARD R3-D06-390.dc.html — R3-D06 · A.1.12 · devadmin on Sipho Ndlovu's Erasure, before approval · 390
> R3-D06 · 390 · focus: none (the note typed)
Wombat
Acting as Administrator
MenuClose
Acting as Administrator
- Home
Platform
- Scheduled jobs
- Audit log
- SSO mappings
- Data rights requests
Organisations
- Institutions
- Colleges
People
- Users
- Invitations
Catalogue
- EPAs
- Curricula
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- My data rights
Demo Administrator
Sign out
- Home
- Data rights requests
- 01a17f64-f566-75f0-91ba-5d71863b77d0
### Erasure request from Sipho Ndlovu
Submitted 2026-10-28 00:04 SAST · Kgosi Kgari Teaching Hospital
#### Request
**Requester
Sipho Ndlovu
**Email
ndlovu@kgk.wombat.local
**User id
4e5ce9ca-438b-4d98-8635-a7259638d7c9
**Request id
01a17f64-f566-75f0-91ba-5d71863b77d0
**Type
Erasure
**Status
Submitted
**Reason
I am leaving the programme and want my personal data removed.
#### Decide
Approving erases Sipho Ndlovu's personal data at once and cannot be undone. You are asked to confirm, with what it ends and keeps.
Decision note *required
Identity confirmed. Programme records are kept under a pseudonym.
Kept with the request. Sipho Ndlovu will not read it: approving ends the account.
RejectApprove and erase…

######## BOARD R3-D07-390.dc.html — R3-D07 · A.1.12 · the dialog, opening on Cancel · 390
> R3-D07 · 390 · focus: Cancel (in the dialog)
Wombat
Acting as Administrator
MenuClose
Acting as Administrator
- Home
Platform
- Scheduled jobs
- Audit log
- SSO mappings
- Data rights requests
Organisations
- Institutions
- Colleges
People
- Users
- Invitations
Catalogue
- EPAs
- Curricula
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- My data rights
Demo Administrator
Sign out
- Home
- Data rights requests
- 01a17f64-f566-75f0-91ba-5d71863b77d0
### Erasure request from Sipho Ndlovu
Submitted 2026-10-28 00:04 SAST · Kgosi Kgari Teaching Hospital
#### Request
**Requester
Sipho Ndlovu
**Email
ndlovu@kgk.wombat.local
**User id
4e5ce9ca-438b-4d98-8635-a7259638d7c9
**Request id
01a17f64-f566-75f0-91ba-5d71863b77d0
**Type
Erasure
**Status
Submitted
**Reason
I am leaving the programme and want my personal data removed.
#### Decide
Approving erases Sipho Ndlovu's personal data at once and cannot be undone. You are asked to confirm, with what it ends and keeps.
Decision note *required
Identity confirmed. Programme records are kept under a pseudonym.
Kept with the request. Sipho Ndlovu will not read it: approving ends the account.
RejectApprove and erase…
#### Erase Sipho Ndlovu's personal data?
This cannot be undone.
Approving erases it now, in one step, and completes the request.
It ends:
- The account and its sign-in. Sipho Ndlovu can no longer sign in, and an open session ends within a minute.
- 1 open committee review: Formative check-in #8, Paed Annual Review Panel, Scheduled. It is withdrawn, and nothing more is decided at it.
- No MSF campaign: none about Sipho Ndlovu is open.
- The programme, Paediatric EPA Curriculum (11.2), which ends today as Withdrawn.
It keeps, under a pseudonym that names nobody: the ratified review #5, closed check-in #6, the activities, progress and audit entries, and this request.
CancelErase and approve

######## BOARD R3-D08-390.dc.html — R3-D08 · A.1.12 · Cancel: nothing changed · 390
> R3-D08 · 390 · focus: “Approve and erase…” (Cancel and Escape alike)
Wombat
Acting as Administrator
MenuClose
Acting as Administrator
- Home
Platform
- Scheduled jobs
- Audit log
- SSO mappings
- Data rights requests
Organisations
- Institutions
- Colleges
People
- Users
- Invitations
Catalogue
- EPAs
- Curricula
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- My data rights
Demo Administrator
Sign out
- Home
- Data rights requests
- 01a17f64-f566-75f0-91ba-5d71863b77d0
### Erasure request from Sipho Ndlovu
Submitted 2026-10-28 00:04 SAST · Kgosi Kgari Teaching Hospital
#### Request
**Requester
Sipho Ndlovu
**Email
ndlovu@kgk.wombat.local
**User id
4e5ce9ca-438b-4d98-8635-a7259638d7c9
**Request id
01a17f64-f566-75f0-91ba-5d71863b77d0
**Type
Erasure
**Status
Submitted
**Reason
I am leaving the programme and want my personal data removed.
#### Decide
Approving erases Sipho Ndlovu's personal data at once and cannot be undone. You are asked to confirm, with what it ends and keeps.
Decision note *required
Identity confirmed. Programme records are kept under a pseudonym.
Kept with the request. Sipho Ndlovu will not read it: approving ends the account.
RejectApprove and erase…

######## BOARD R3-D09-390.dc.html — R3-D09 · A.1.12 · Erase and approve: erased, approved, completed · 390
> R3-D09 · 390 · focus: the result, top of Decision
Wombat
Acting as Administrator
MenuClose
Acting as Administrator
- Home
Platform
- Scheduled jobs
- Audit log
- SSO mappings
- Data rights requests
Organisations
- Institutions
- Colleges
People
- Users
- Invitations
Catalogue
- EPAs
- Curricula
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- My data rights
Demo Administrator
Sign out
- Home
- Data rights requests
- 01a17f64-f566-75f0-91ba-5d71863b77d0
### Erasure request from deleted_user_0ae1d64b
Submitted 2026-10-28 00:04 SAST · Kgosi Kgari Teaching Hospital
This person's personal data was erased on 2026-10-28, at this request. The request is kept as the record of it, under the pseudonym.
#### Request
**Requester
deleted_user_0ae1d64bErased 2026-10-28
**User id
4e5ce9ca-438b-4d98-8635-a7259638d7c9
**Request id
01a17f64-f566-75f0-91ba-5d71863b77d0
**Type
Erasure
**Status
Completed
**Reason
I am leaving the programme and want my personal data removed.
#### Decision
The personal data is erased. The request is approved and completed. 1 committee review was withdrawn and the programme ended today. The account is now deleted_user_0ae1d64b; its open session ends within a minute.
**Decision
Approved
**Decided by
Demo Administrator
abd4ed66-e9ad-4187-b87f-c3c481d05c30
**Decided on
2026-10-28 00:04 SAST
**Note
Identity confirmed. Programme records are kept under a pseudonym.
**Completed on
2026-10-28 00:04 SAST

######## BOARD R3-D10-390.dc.html — R3-D10 · The PseudonymSalt refusal after the dialog closes · 390 · Illustration
> IllustrationR3-D10 · 390 · focus: the refusal alert
Wombat
Acting as Administrator
MenuClose
Acting as Administrator
- Home
Platform
- Scheduled jobs
- Audit log
- SSO mappings
- Data rights requests
Organisations
- Institutions
- Colleges
People
- Users
- Invitations
Catalogue
- EPAs
- Curricula
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- My data rights
Demo Administrator
Sign out
- Home
- Data rights requests
- 01a17f64-f566-75f0-91ba-5d71863b77d0
### Erasure request from Sipho Ndlovu
Submitted 2026-10-28 00:04 SAST · Kgosi Kgari Teaching Hospital
#### Request
**Requester
Sipho Ndlovu
**Email
ndlovu@kgk.wombat.local
**User id
4e5ce9ca-438b-4d98-8635-a7259638d7c9
**Request id
01a17f64-f566-75f0-91ba-5d71863b77d0
**Type
Erasure
**Status
Submitted
**Reason
I am leaving the programme and want my personal data removed.
#### Decide
Nothing has been erased. Erasure is not set up on this server. The request is still Submitted. Ask whoever runs the server to set it up, then approve again.
Approving erases Sipho Ndlovu's personal data at once and cannot be undone. You are asked to confirm, with what it ends and keeps.
Decision note *required
Identity confirmed. Programme records are kept under a pseudonym.
Kept with the request. Sipho Ndlovu will not read it: approving ends the account.
RejectApprove and erase…

######## BOARD R3-D11-390.dc.html — R3-D11 · The concurrent-change refusal · 390 · Illustration
> IllustrationR3-D11 · 390 · focus: the refusal alert
Wombat
Acting as Administrator
MenuClose
Acting as Administrator
- Home
Platform
- Scheduled jobs
- Audit log
- SSO mappings
- Data rights requests
Organisations
- Institutions
- Colleges
People
- Users
- Invitations
Catalogue
- EPAs
- Curricula
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- My data rights
Demo Administrator
Sign out
- Home
- Data rights requests
- 01a17f64-f566-75f0-91ba-5d71863b77d0
### Erasure request from Sipho Ndlovu
Submitted 2026-10-28 00:04 SAST · Kgosi Kgari Teaching Hospital
#### Request
**Requester
Sipho Ndlovu
**Email
ndlovu@kgk.wombat.local
**User id
4e5ce9ca-438b-4d98-8635-a7259638d7c9
**Request id
01a17f64-f566-75f0-91ba-5d71863b77d0
**Type
Erasure
**Status
Submitted
**Reason
I am leaving the programme and want my personal data removed.
#### Decide
Something about this person changed while they were being erased: a review, a feedback campaign or their account was changed elsewhere at the same moment. Nothing has been erased, and the request has not been approved. Approve it again.
Approving erases Sipho Ndlovu's personal data at once and cannot be undone. You are asked to confirm, with what it ends and keeps.
Decision note *required
Identity confirmed. Programme records are kept under a pseudonym.
Kept with the request. Sipho Ndlovu will not read it: approving ends the account.
RejectApprove and erase…

######## BOARD R3-D12-390.dc.html — R3-D12 · Withdrawn meanwhile: the press refused, the page as Withdrawn · 390 · Illustration
> IllustrationR3-D12 · 390 · focus: the refusal alert · Approve and Reject check Withdrawn first (never the domain's “Only submitted or under-review requests can be approved.”); the page reads the row again and draws it Withdrawn; the name read live (F1)
Wombat
Acting as Administrator
MenuClose
Acting as Administrator
- Home
Platform
- Scheduled jobs
- Audit log
- SSO mappings
- Data rights requests
Organisations
- Institutions
- Colleges
People
- Users
- Invitations
Catalogue
- EPAs
- Curricula
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- My data rights
Demo Administrator
Sign out
- Home
- Data rights requests
- 01a17f64-f566-75f0-91ba-5d71863b77d0
### Erasure request from Sipho Ndlovu
Submitted 2026-10-28 00:04 SAST · Kgosi Kgari Teaching Hospital
#### Request
**Requester
Sipho Ndlovu
**Email
ndlovu@kgk.wombat.local
**User id
4e5ce9ca-438b-4d98-8635-a7259638d7c9
**Request id
01a17f64-f566-75f0-91ba-5d71863b77d0
**Type
Erasure
**Status
Withdrawn
**Reason
I am leaving the programme and want my personal data removed.
Sipho Ndlovu withdrew this request. Nothing was decided or erased.

######## BOARD R3-D13-390.dc.html — R3-D13 · Pieter Smit: the dialog for someone holding a staff role, a review elsewhere counted · 390 · Illustration
> IllustrationR3-D13 · 390 · focus: Cancel (in the dialog)
Wombat
Acting as Coordinator
MenuClose
Acting as Coordinator
- Home
- Programme trainees
- Waiting for assessors
- Decisions due
- MSF campaigns
- Committee reviews
- Decision panels
- Data rights requests
- My data rights
Pieter Smit
Sign out
- Home
- Data rights requests
- ‹request id›
### Erasure request from ‹name›
Submitted ‹date› ‹time› SAST · Kgosi Kgari Teaching Hospital
#### Request
**Requester
‹name›
**Email
‹address›
**User id
‹user id›
**Request id
‹request id›
**Type
Erasure
**Status
Submitted
**Reason
‹reason›
#### Decide
Approving erases ‹name›'s personal data at once and cannot be undone. You are asked to confirm, with what it ends and keeps.
Decision note *required
‹note›
Kept with the request. ‹name› will not read it: approving ends the account.
RejectApprove and erase…
#### Erase ‹name›'s personal data?
This cannot be undone.
Approving erases it now, in one step, and completes the request.
It ends:
- The account and its sign-in. ‹name› can no longer sign in, and an open session ends within a minute.
- The roles ‹roles› and their scopes.
- ‹n› open committee reviews: ‹reviews at Kgosi Kgari Teaching Hospital›, and 1 open review at another institution. Each is withdrawn, and nothing more is decided at it.
- ‹n› MSF campaigns not yet released: ‹campaigns›.
- ‹n› other open data rights requests: ‹requests›. Each is withdrawn.
- The programme, ‹curriculum›, which ends today as Withdrawn.
It keeps, under a pseudonym that names nobody: ‹what is settled›, and this request.
CancelErase and approve

######## BOARD R3-D14-390.dc.html — R3-D14 · The dialog for a person with a review under appeal · 390 · Illustration
> IllustrationR3-D14 · 390 · focus: Cancel (in the dialog)
Wombat
Acting as Administrator
MenuClose
Acting as Administrator
- Home
Platform
- Scheduled jobs
- Audit log
- SSO mappings
- Data rights requests
Organisations
- Institutions
- Colleges
People
- Users
- Invitations
Catalogue
- EPAs
- Curricula
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- My data rights
Demo Administrator
Sign out
- Home
- Data rights requests
- ‹request id›
### Erasure request from ‹name›
Submitted ‹date› ‹time› SAST · ‹institution›
#### Request
**Requester
‹name›
**Email
‹address›
**User id
‹user id›
**Request id
‹request id›
**Type
Erasure
**Status
Submitted
**Reason
‹reason›
#### Decide
Approving erases ‹name›'s personal data at once and cannot be undone. You are asked to confirm, with what it ends and keeps.
Decision note *required
‹note›
Kept with the request. ‹name› will not read it: approving ends the account.
RejectApprove and erase…
#### Erase ‹name›'s personal data?
This cannot be undone.
Approving erases it now, in one step, and completes the request.
It ends:
- The account and its sign-in. ‹name› can no longer sign in, and an open session ends within a minute.
- ‹n› open committee reviews: ‹reviews›. Each is withdrawn, and nothing more is decided at it.
- ‹n› MSF campaigns not yet released: ‹campaigns›.
- The programme, ‹curriculum›, which ends today as Withdrawn.
It keeps, under a pseudonym that names nobody: ‹what is settled›, this request, and the review under appeal, which can still be answered (‹review›).
CancelErase and approve

######## BOARD R3-D15-390.dc.html — R3-D15 · A reviewer's own request: no Decide · 390 · Illustration
> IllustrationR3-D15 · 390 · focus: none
Wombat
Acting as Coordinator
MenuClose
Acting as Coordinator
- Home
- Programme trainees
- Waiting for assessors
- Decisions due
- MSF campaigns
- Committee reviews
- Decision panels
- Data rights requests
- My data rights
‹name›
Sign out
- Home
- Data rights requests
- ‹request id›
### Export request from ‹name›
Submitted ‹date› ‹time› SAST · Kgosi Kgari Teaching Hospital
#### Request
**Requester
‹name›
**Email
‹address›
**User id
‹user id›
**Request id
‹request id›
**Type
Export
**Status
Submitted
**Reason
‹reason›
You cannot decide your own request: another Coordinator at Kgosi Kgari Teaching Hospital, or the Wombat administrator, decides it.

######## BOARD R3-D16-390.dc.html — R3-D16 · A failed decision: the note kept · 390 · Illustration
> IllustrationR3-D16 · 390 · focus: the failure alert · fails A.1.3's Approve
Wombat
Acting as Coordinator
MenuClose
Acting as Coordinator
- Home
- Programme trainees
- Waiting for assessors
- Decisions due
- MSF campaigns
- Committee reviews
- Decision panels
- Data rights requests
- My data rights
Pieter Smit
Sign out
- Home
- Data rights requests
- 01a17f61-f16f-77c1-860c-661586d5b733
### Export request from Anele Dlamini
Submitted 2026-10-28 00:01 SAST · Kgosi Kgari Teaching Hospital
#### Request
**Requester
Anele Dlamini
**Email
dlamini@kgk.wombat.local
**User id
67083efa-bbaa-48d5-a753-b01fdf38530d
**Request id
01a17f61-f16f-77c1-860c-661586d5b733
**Type
Export
**Status
Submitted
**Reason
A copy of my training record for my own files.
#### Decide
The decision could not be saved just now. Nothing has changed. Try again, or come back in a few minutes.
Approving makes the export ready at once: Anele Dlamini downloads it from My data rights, and it is built at that moment. You cannot download it yourself.
Decision note *required
Identity confirmed; export released to the data subject.
Anele Dlamini reads this note with the decision on My data rights, and is emailed that it was decided. Who decided is not shown.
RejectApprove

######## BOARD R3-D17-390.dc.html — R3-D17 · Loading: the trail with the id, a skeleton, no Decide · 390
> R3-D17 · 390 · focus: none · the read not yet back (not null, no exception)
Wombat
Acting as Coordinator
MenuClose
Acting as Coordinator
- Home
- Programme trainees
- Waiting for assessors
- Decisions due
- MSF campaigns
- Committee reviews
- Decision panels
- Data rights requests
- My data rights
Pieter Smit
Sign out
- Home
- Data rights requests
- 01a17f61-f16f-77c1-860c-661586d5b733
### Data rights request
Loading the request.

######## BOARD R3-D18-390.dc.html — R3-D18 · The load error · 390
> R3-D18 · 390 · focus: Try again → the h1 (tabindex -1) · drawn when the read throws; never ex.Message
Wombat
Acting as Coordinator
MenuClose
Acting as Coordinator
- Home
- Programme trainees
- Waiting for assessors
- Decisions due
- MSF campaigns
- Committee reviews
- Decision panels
- Data rights requests
- My data rights
Pieter Smit
Sign out
- Home
- Data rights requests
- 01a17f61-f16f-77c1-860c-661586d5b733
### Data rights request
Could not load this request. Nothing has changed. Try again, or come back in a few minutes.Try again

######## BOARD R3-D19-390.dc.html — R3-D19 · A 60-character address, a 4,000-character reason, a long note · 390 · Illustration
> IllustrationR3-D19 · 390 · focus: none · Pieter Smit as the decider of ‹name›'s request
Wombat
Acting as Coordinator
MenuClose
Acting as Coordinator
- Home
- Programme trainees
- Waiting for assessors
- Decisions due
- MSF campaigns
- Committee reviews
- Decision panels
- Data rights requests
- My data rights
Pieter Smit
Sign out
- Home
- Data rights requests
- ‹request id›
### Rectification request from ‹name›
Submitted ‹date› ‹time› SAST · Kgosi Kgari Teaching Hospital
#### Request
**Requester
‹name›
**Email
xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx@kgk.wombat.local
**User id
‹user id›
**Request id
‹request id›
**Type
Rectification
**Status
Rejected
**Reason
‹a reason of 4,000 characters, wrapping in its dd› ‹a reason of 4,000 characters, wrapping in its dd› ‹a reason of 4,000 characters, wrapping in its dd› ‹a reason of 4,000 characters, wrapping in its dd› ‹a reason of 4,000 characters, wrapping in its dd› ‹a reason of 4,000 characters, wrapping in its dd› ‹a reason of 4,000 characters, wrapping in its dd› ‹a reason of 4,000 characters, wrapping in its dd› ‹a reason of 4,000 characters, wrapping in its dd› ‹a reason of 4,000 characters, wrapping in its dd› ‹a reason of 4,000 characters, wrapping in its dd› ‹a reason of 4,000 characters, wrapping in its dd› ‹a reason of 4,000 characters, wrapping in its dd› ‹a reason of 4,000 characters, wrapping in its dd› ‹a reason of 4,000 characters, wrapping in its dd› ‹a reason of 4,000 characters, wrapping in its dd› ‹a reason of 4,000 characters, wrapping in its dd› ‹a reason of 4,000 characters, wrapping in its dd› ‹a reason of 4,000 characters, wrapping in its dd› ‹a reason of 4,000 characters, wrapping in its dd› ‹a reason of 4,000 characters, wrapping in its dd› ‹a reason of 4,000 characters, wrapping in its dd› ‹a reason of 4,000 characters, wrapping in its dd› ‹a reason of 4,000 characters, wrapping in its dd› ‹a reason of 4,000 characters, wrapping in its dd› ‹a reason of 4,000 characters, wrapping in its dd› ‹a reason of 4,000 characters, wrapping in its dd› ‹a reason of 4,000 characters, wrapping in its dd› ‹a reason of 4,000 characters, wrapping in its dd› ‹a reason of 4,000 characters, wrapping in its dd›
#### Decision
**Decision
Rejected
**Decided by
Pieter Smit
43ad0cb7-5ce6-46d7-8595-7117d130c7dc
**Decided on
‹date› ‹time› SAST
**Note
‹a long decision note› ‹a long decision note› ‹a long decision note› ‹a long decision note› ‹a long decision note› ‹a long decision note› ‹a long decision note› ‹a long decision note› ‹a long decision note› ‹a long decision note› ‹a long decision note› ‹a long decision note› ‹a long decision note› ‹a long decision note› ‹a long decision note› ‹a long decision note› ‹a long decision note› ‹a long decision note› ‹a long decision note› ‹a long decision note›

######## BOARD R3-D20-390.dc.html — R3-D20 · devadmin's own Export request: no Decide · 390 · Illustration
> IllustrationR3-D20 · 390 · focus: none
Wombat
Acting as Administrator
MenuClose
Acting as Administrator
- Home
Platform
- Scheduled jobs
- Audit log
- SSO mappings
- Data rights requests
Organisations
- Institutions
- Colleges
People
- Users
- Invitations
Catalogue
- EPAs
- Curricula
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- My data rights
Demo Administrator
Sign out
- Home
- Data rights requests
- ‹request id›
### Export request from Demo Administrator
Submitted ‹date› ‹time› SAST
#### Request
**Requester
Demo Administrator
**Email
‹address›
**User id
abd4ed66-e9ad-4187-b87f-c3c481d05c30
**Request id
‹request id›
**Type
Export
**Status
Submitted
**Reason
‹reason›
You cannot decide your own request: another Administrator decides it.

######## BOARD R3-D21-390.dc.html — R3-D21 · Pieter Smit: the erasure of the only Administrator, refused · 390 · Illustration
> IllustrationR3-D21 · 390 · focus: the refusal alert, after Erase and approve; still Submitted, the note kept
Wombat
Acting as Coordinator
MenuClose
Acting as Coordinator
- Home
- Programme trainees
- Waiting for assessors
- Decisions due
- MSF campaigns
- Committee reviews
- Decision panels
- Data rights requests
- My data rights
Pieter Smit
Sign out
- Home
- Data rights requests
- ‹request id›
### Erasure request from ‹name›
Submitted ‹date› ‹time› SAST · Kgosi Kgari Teaching Hospital
#### Request
**Requester
‹name›
**Email
‹address›
**User id
‹user id›
**Request id
‹request id›
**Type
Erasure
**Status
Submitted
**Reason
‹reason›
#### Decide
Nothing has been erased. ‹name› is Wombat's only Administrator, and erasing the account would leave nobody to run it. Add another Administrator first, then approve again.
Approving erases ‹name›'s personal data at once and cannot be undone. You are asked to confirm, with what it ends and keeps.
Decision note *required
‹note›
Kept with the request. ‹name› will not read it: approving ends the account.
RejectApprove and erase…

######## BOARD R3-D22-390.dc.html — R3-D22 · An erasure with nothing else open: the result · 390 · Illustration
> IllustrationR3-D22 · 390 · focus: the result, top of Decision
Wombat
Acting as Administrator
MenuClose
Acting as Administrator
- Home
Platform
- Scheduled jobs
- Audit log
- SSO mappings
- Data rights requests
Organisations
- Institutions
- Colleges
People
- Users
- Invitations
Catalogue
- EPAs
- Curricula
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- My data rights
Demo Administrator
Sign out
- Home
- Data rights requests
- ‹request id›
### Erasure request from ‹pseudonym›
Submitted ‹date› ‹time› SAST · ‹institution›
This person's personal data was erased on ‹date›, at this request. The request is kept as the record of it, under the pseudonym.
#### Request
**Requester
‹pseudonym›Erased ‹date›
**User id
‹user id›
**Request id
‹request id›
**Type
Erasure
**Status
Completed
**Reason
‹reason›
#### Decision
The personal data is erased. The request is approved and completed. Nothing else was open to end. The account is now ‹pseudonym›; its open session ends within a minute.
**Decision
Approved
**Decided by
Demo Administrator
abd4ed66-e9ad-4187-b87f-c3c481d05c30
**Decided on
‹date› ‹time› SAST
**Note
‹note›
**Completed on
‹date› ‹time› SAST

######## BOARD R3-D01-768.dc.html — R3-D01 · A.1.3 · Anele Dlamini's Export, Submitted, with Decide · 768
> R3-D01 · 768 · focus: none (page load)
Wombat
Acting as Coordinator
MenuClose
Acting as Coordinator
- Home
- Programme trainees
- Waiting for assessors
- Decisions due
- MSF campaigns
- Committee reviews
- Decision panels
- Data rights requests
- My data rights
Pieter Smit
Sign out
- Home
- Data rights requests
- 01a17f61-f16f-77c1-860c-661586d5b733
### Export request from Anele Dlamini
Submitted 2026-10-28 00:01 SAST · Kgosi Kgari Teaching Hospital
#### Request
**Requester
Anele Dlamini
**Email
dlamini@kgk.wombat.local
**User id
67083efa-bbaa-48d5-a753-b01fdf38530d
**Request id
01a17f61-f16f-77c1-860c-661586d5b733
**Type
Export
**Status
Submitted
**Reason
A copy of my training record for my own files.
#### Decide
Approving makes the export ready at once: Anele Dlamini downloads it from My data rights, and it is built at that moment. You cannot download it yourself.
Decision note *required
Anele Dlamini reads this note with the decision on My data rights, and is emailed that it was decided. Who decided is not shown.
RejectApprove

######## BOARD R3-C01-1280.dc.html — R3-C01 · A.1.8a · approved by Pieter Smit: the Correction section opens · 1280
> R3-C01 · 1280 · focus: the result, top of Decision · before the corrective save
Wombat
Acting as Coordinator
MenuClose
Acting as Coordinator
- Home
- Programme trainees
- Waiting for assessors
- Decisions due
- MSF campaigns
- Committee reviews
- Decision panels
- Data rights requests
- My data rights
Pieter Smit
Sign out
- Home
- Data rights requests
- ‹request id›
### Rectification request from Nomsa Mahlngu
Submitted ‹time› SAST · Kgosi Kgari Teaching Hospital
#### Request
**Requester
Nomsa Mahlngu
**Email
mahlangu@kgk.wombat.local
**User id
d04f4dfb-f855-4345-9efa-edd168d5a2b6
**Request id
‹request id›
**Type
Rectification
**Status
Approved
**Reason
‹reason›
#### Decision
Request approved. Nomsa Mahlngu is emailed. Record each correction below, then complete the request.
**Decision
Approved
**Decided by
Pieter Smit
43ad0cb7-5ce6-46d7-8595-7117d130c7dc
**Decided on
‹time› SAST
**Note
‹note›
#### Correction
Record each change as it is made on the page that holds the data.
No correction is recorded yet.
Record * required
Account › First name
Changed by Nomsa Mahlngu on My account. No staff page changes a name.
Account › Last name
Changed by Nomsa Mahlngu on My account. No staff page changes a name.
Trainee profile › Programme start
Changed on Nomsa Mahlngu's programme page, which an Institutional admin at Kgosi Kgari Teaching Hospital opens; you cannot.
Other
Name the record in the field under it.
Value before *required
Value after *required
Record correction
Once every correction is made and recorded, complete the request. It then reads Completed here and on Nomsa Mahlngu's My data rights.
Complete request

######## BOARD R3-C02-1280.dc.html — R3-C02 · A.1.8a · approved, nothing recorded: the four records · 1280
> R3-C02 · 1280 · focus: none (page load) · before the corrective save
Wombat
Acting as Coordinator
MenuClose
Acting as Coordinator
- Home
- Programme trainees
- Waiting for assessors
- Decisions due
- MSF campaigns
- Committee reviews
- Decision panels
- Data rights requests
- My data rights
Pieter Smit
Sign out
- Home
- Data rights requests
- ‹request id›
### Rectification request from Nomsa Mahlngu
Submitted ‹time› SAST · Kgosi Kgari Teaching Hospital
#### Request
**Requester
Nomsa Mahlngu
**Email
mahlangu@kgk.wombat.local
**User id
d04f4dfb-f855-4345-9efa-edd168d5a2b6
**Request id
‹request id›
**Type
Rectification
**Status
Approved
**Reason
‹reason›
#### Decision
**Decision
Approved
**Decided by
Pieter Smit
43ad0cb7-5ce6-46d7-8595-7117d130c7dc
**Decided on
‹time› SAST
**Note
‹note›
#### Correction
Record each change as it is made on the page that holds the data.
No correction is recorded yet.
Record * required
Account › First name
Changed by Nomsa Mahlngu on My account. No staff page changes a name.
Account › Last name
Changed by Nomsa Mahlngu on My account. No staff page changes a name.
Trainee profile › Programme start
Changed on Nomsa Mahlngu's programme page, which an Institutional admin at Kgosi Kgari Teaching Hospital opens; you cannot.
Other
Name the record in the field under it.
Value before *required
Value after *required
Record correction
Once every correction is made and recorded, complete the request. It then reads Completed here and on Nomsa Mahlngu's My data rights.
Complete request

######## BOARD R3-C03-1280.dc.html — R3-C03 · A.1.8a · Complete request with none recorded: refused · 1280
> R3-C03 · 1280 · focus: Complete request (the refusal in the section's ActionResult) · before the corrective save
Wombat
Acting as Coordinator
MenuClose
Acting as Coordinator
- Home
- Programme trainees
- Waiting for assessors
- Decisions due
- MSF campaigns
- Committee reviews
- Decision panels
- Data rights requests
- My data rights
Pieter Smit
Sign out
- Home
- Data rights requests
- ‹request id›
### Rectification request from Nomsa Mahlngu
Submitted ‹time› SAST · Kgosi Kgari Teaching Hospital
#### Request
**Requester
Nomsa Mahlngu
**Email
mahlangu@kgk.wombat.local
**User id
d04f4dfb-f855-4345-9efa-edd168d5a2b6
**Request id
‹request id›
**Type
Rectification
**Status
Approved
**Reason
‹reason›
#### Decision
**Decision
Approved
**Decided by
Pieter Smit
43ad0cb7-5ce6-46d7-8595-7117d130c7dc
**Decided on
‹time› SAST
**Note
‹note›
#### Correction
Record each change as it is made on the page that holds the data.
No correction is recorded yet.
Record * required
Account › First name
Changed by Nomsa Mahlngu on My account. No staff page changes a name.
Account › Last name
Changed by Nomsa Mahlngu on My account. No staff page changes a name.
Trainee profile › Programme start
Changed on Nomsa Mahlngu's programme page, which an Institutional admin at Kgosi Kgari Teaching Hospital opens; you cannot.
Other
Name the record in the field under it.
Value before *required
Value after *required
Record correction
Record at least one correction before completing the request.
Once every correction is made and recorded, complete the request. It then reads Completed here and on Nomsa Mahlngu's My data rights.
Complete request

######## BOARD R3-C04-1280.dc.html — R3-C04 · Record correction with nothing chosen or typed · 1280 · Illustration
> IllustrationR3-C04 · 1280 · focus: the first record's radio · refuses A.1.8a's record, before the corrective save
Wombat
Acting as Coordinator
MenuClose
Acting as Coordinator
- Home
- Programme trainees
- Waiting for assessors
- Decisions due
- MSF campaigns
- Committee reviews
- Decision panels
- Data rights requests
- My data rights
Pieter Smit
Sign out
- Home
- Data rights requests
- ‹request id›
### Rectification request from Nomsa Mahlngu
Submitted ‹time› SAST · Kgosi Kgari Teaching Hospital
#### Request
**Requester
Nomsa Mahlngu
**Email
mahlangu@kgk.wombat.local
**User id
d04f4dfb-f855-4345-9efa-edd168d5a2b6
**Request id
‹request id›
**Type
Rectification
**Status
Approved
**Reason
‹reason›
#### Decision
**Decision
Approved
**Decided by
Pieter Smit
43ad0cb7-5ce6-46d7-8595-7117d130c7dc
**Decided on
‹time› SAST
**Note
‹note›
#### Correction
Record each change as it is made on the page that holds the data.
No correction is recorded yet.
Record * required
Account › First name
Changed by Nomsa Mahlngu on My account. No staff page changes a name.
Account › Last name
Changed by Nomsa Mahlngu on My account. No staff page changes a name.
Trainee profile › Programme start
Changed on Nomsa Mahlngu's programme page, which an Institutional admin at Kgosi Kgari Teaching Hospital opens; you cannot.
Other
Name the record in the field under it.
Choose the record.
Value before *required
Enter the value before.
Value after *required
Enter the value after.
Record correction
Once every correction is made and recorded, complete the request. It then reads Completed here and on Nomsa Mahlngu's My data rights.
Complete request

######## BOARD R3-C05-1280.dc.html — R3-C05 · A.1.8a · one recorded: Account › Last name · 1280
> R3-C05 · 1280 · focus: “Correction recorded: …” · after the corrective save
Wombat
Acting as Coordinator
MenuClose
Acting as Coordinator
- Home
- Programme trainees
- Waiting for assessors
- Decisions due
- MSF campaigns
- Committee reviews
- Decision panels
- Data rights requests
- My data rights
Pieter Smit
Sign out
- Home
- Data rights requests
- ‹request id›
### Rectification request from Nomsa Mahlangu
Submitted ‹time› SAST · Kgosi Kgari Teaching Hospital
#### Request
**Requester
Nomsa Mahlangu
**Email
mahlangu@kgk.wombat.local
**User id
d04f4dfb-f855-4345-9efa-edd168d5a2b6
**Request id
‹request id›
**Type
Rectification
**Status
Approved
**Reason
‹reason›
#### Decision
**Decision
Approved
**Decided by
Pieter Smit
43ad0cb7-5ce6-46d7-8595-7117d130c7dc
**Decided on
‹time› SAST
**Note
‹note›
#### Correction
Correction recorded: Account › Last name.
Record each change as it is made on the page that holds the data.
Corrections recorded on this request
| Record | Value before | Value after | Recorded |
| Account › Last name | Mahlngu | Mahlangu | Pieter Smit · ‹time› SAST |
Record * required
Account › First name
Changed by Nomsa Mahlangu on My account. No staff page changes a name.
Account › Last name
Changed by Nomsa Mahlangu on My account. No staff page changes a name.
Trainee profile › Programme start
Changed on Nomsa Mahlangu's programme page, which an Institutional admin at Kgosi Kgari Teaching Hospital opens; you cannot.
Other
Name the record in the field under it.
Value before *required
Value after *required
Record correction
Once every correction is made and recorded, complete the request. It then reads Completed here and on Nomsa Mahlangu's My data rights.
Complete request

######## BOARD R3-C06-1280.dc.html — R3-C06 · A.1.8a · completed · 1280
> R3-C06 · 1280 · focus: the result, top of Correction · after the corrective save
Wombat
Acting as Coordinator
MenuClose
Acting as Coordinator
- Home
- Programme trainees
- Waiting for assessors
- Decisions due
- MSF campaigns
- Committee reviews
- Decision panels
- Data rights requests
- My data rights
Pieter Smit
Sign out
- Home
- Data rights requests
- ‹request id›
### Rectification request from Nomsa Mahlangu
Submitted ‹time› SAST · Kgosi Kgari Teaching Hospital
#### Request
**Requester
Nomsa Mahlangu
**Email
mahlangu@kgk.wombat.local
**User id
d04f4dfb-f855-4345-9efa-edd168d5a2b6
**Request id
‹request id›
**Type
Rectification
**Status
Completed
**Reason
‹reason›
#### Decision
**Decision
Approved
**Decided by
Pieter Smit
43ad0cb7-5ce6-46d7-8595-7117d130c7dc
**Decided on
‹time› SAST
**Note
‹note›
**Completed on
‹time› SAST
#### Correction
Request completed. Nomsa Mahlangu is emailed, and reads it as Completed on My data rights.
Corrections recorded on this request
| Record | Value before | Value after | Recorded |
| Account › Last name | Mahlngu | Mahlangu | Pieter Smit · ‹time› SAST |

######## BOARD R3-C07-1280.dc.html — R3-C07 · devadmin's view: the programme-start line links · 1280 · Illustration
> IllustrationR3-C07 · 1280 · focus: none · before the corrective save
Wombat
Acting as Administrator
MenuClose
Acting as Administrator
- Home
Platform
- Scheduled jobs
- Audit log
- SSO mappings
- Data rights requests
Organisations
- Institutions
- Colleges
People
- Users
- Invitations
Catalogue
- EPAs
- Curricula
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- My data rights
Demo Administrator
Sign out
- Home
- Data rights requests
- ‹request id›
### Rectification request from Nomsa Mahlngu
Submitted ‹time› SAST · Kgosi Kgari Teaching Hospital
#### Request
**Requester
Nomsa Mahlngu
**Email
mahlangu@kgk.wombat.local
**User id
d04f4dfb-f855-4345-9efa-edd168d5a2b6
**Request id
‹request id›
**Type
Rectification
**Status
Approved
**Reason
‹reason›
#### Decision
**Decision
Approved
**Decided by
Pieter Smit
43ad0cb7-5ce6-46d7-8595-7117d130c7dc
**Decided on
‹time› SAST
**Note
‹note›
#### Correction
Record each change as it is made on the page that holds the data.
No correction is recorded yet.
Record * required
Account › First name
Changed by Nomsa Mahlngu on My account. No staff page changes a name.
Account › Last name
Changed by Nomsa Mahlngu on My account. No staff page changes a name.
Trainee profile › Programme start
Changed on Nomsa Mahlngu's programme page. Open Nomsa Mahlngu's programme
Other
Name the record in the field under it.
Value before *required
Value after *required
Record correction
Once every correction is made and recorded, complete the request. It then reads Completed here and on Nomsa Mahlngu's My data rights.
Complete request

######## BOARD R3-C08-1280.dc.html — R3-C08 · An ended programme's requester: the archived line · 1280 · Illustration
> IllustrationR3-C08 · 1280 · focus: none
Wombat
Acting as Coordinator
MenuClose
Acting as Coordinator
- Home
- Programme trainees
- Waiting for assessors
- Decisions due
- MSF campaigns
- Committee reviews
- Decision panels
- Data rights requests
- My data rights
Pieter Smit
Sign out
- Home
- Data rights requests
- ‹request id›
### Rectification request from ‹name›
Submitted ‹time› SAST · Kgosi Kgari Teaching Hospital
#### Request
**Requester
‹name›
**Email
‹address›
**User id
‹user id›
**Request id
‹request id›
**Type
Rectification
**Status
Approved
**Reason
‹reason›
#### Decision
**Decision
Approved
**Decided by
Pieter Smit
43ad0cb7-5ce6-46d7-8595-7117d130c7dc
**Decided on
‹time› SAST
**Note
‹note›
#### Correction
Record each change as it is made on the page that holds the data.
No correction is recorded yet.
Record * required
Account › First name
Changed by ‹name› on My account. No staff page changes a name.
Account › Last name
Changed by ‹name› on My account. No staff page changes a name.
Trainee profile › Programme start
This programme has ended, so its record is archived and cannot be changed.
Other
Name the record in the field under it.
Value before *required
Value after *required
Record correction
Once every correction is made and recorded, complete the request. It then reads Completed here and on ‹name›'s My data rights.
Complete request

######## BOARD R3-C09-1280.dc.html — R3-C09 · Five corrections recorded; Other chosen with its name · 1280 · Illustration
> IllustrationR3-C09 · 1280 · focus: none
Wombat
Acting as Coordinator
MenuClose
Acting as Coordinator
- Home
- Programme trainees
- Waiting for assessors
- Decisions due
- MSF campaigns
- Committee reviews
- Decision panels
- Data rights requests
- My data rights
Pieter Smit
Sign out
- Home
- Data rights requests
- ‹request id›
### Rectification request from ‹name›
Submitted ‹time› SAST · Kgosi Kgari Teaching Hospital
#### Request
**Requester
‹name›
**Email
‹address›
**User id
‹user id›
**Request id
‹request id›
**Type
Rectification
**Status
Approved
**Reason
‹reason›
#### Decision
**Decision
Approved
**Decided by
Pieter Smit
43ad0cb7-5ce6-46d7-8595-7117d130c7dc
**Decided on
‹time› SAST
**Note
‹note›
#### Correction
Record each change as it is made on the page that holds the data.
Corrections recorded on this request
| Record | Value before | Value after | Recorded |
| Account › First name | ‹before› | ‹after› | Pieter Smit · ‹time› SAST |
| Account › Last name | ‹before› | ‹after› | Pieter Smit · ‹time› SAST |
| Trainee profile › Programme start | ‹before› | ‹after› | ‹recorder› · ‹time› SAST |
| Other › ‹record› | ‹before› | ‹after› | Pieter Smit · ‹time› SAST |
| Other › ‹record› | ‹before› | ‹after› | Pieter Smit · ‹time› SAST |
Record * required
Account › First name
Changed by ‹name› on My account. No staff page changes a name.
Account › Last name
Changed by ‹name› on My account. No staff page changes a name.
Trainee profile › Programme start
Changed on ‹name›'s programme page, which an Institutional admin at Kgosi Kgari Teaching Hospital opens; you cannot.
[checked]
Other
Name the record in the field under it.
Record's name *required
[value: ‹the record's name›]
Value before *required
Value after *required
Record correction
Once every correction is made and recorded, complete the request. It then reads Completed here and on ‹name›'s My data rights.
Complete request

######## BOARD R3-C10-1280.dc.html — R3-C10 · A failed record: the values kept · 1280 · Illustration
> IllustrationR3-C10 · 1280 · focus: the failure alert · fails A.1.8a's record, after the corrective save
Wombat
Acting as Coordinator
MenuClose
Acting as Coordinator
- Home
- Programme trainees
- Waiting for assessors
- Decisions due
- MSF campaigns
- Committee reviews
- Decision panels
- Data rights requests
- My data rights
Pieter Smit
Sign out
- Home
- Data rights requests
- ‹request id›
### Rectification request from Nomsa Mahlangu
Submitted ‹time› SAST · Kgosi Kgari Teaching Hospital
#### Request
**Requester
Nomsa Mahlangu
**Email
mahlangu@kgk.wombat.local
**User id
d04f4dfb-f855-4345-9efa-edd168d5a2b6
**Request id
‹request id›
**Type
Rectification
**Status
Approved
**Reason
‹reason›
#### Decision
**Decision
Approved
**Decided by
Pieter Smit
43ad0cb7-5ce6-46d7-8595-7117d130c7dc
**Decided on
‹time› SAST
**Note
‹note›
#### Correction
The correction could not be recorded just now. Nothing has changed. Try again, or come back in a few minutes.
Record each change as it is made on the page that holds the data.
No correction is recorded yet.
Record * required
Account › First name
Changed by Nomsa Mahlangu on My account. No staff page changes a name.
[checked]
Account › Last name
Changed by Nomsa Mahlangu on My account. No staff page changes a name.
Trainee profile › Programme start
Changed on Nomsa Mahlangu's programme page, which an Institutional admin at Kgosi Kgari Teaching Hospital opens; you cannot.
Other
Name the record in the field under it.
Value before *required
[value: Mahlngu]
Value after *required
[value: Mahlangu]
Record correction
Once every correction is made and recorded, complete the request. It then reads Completed here and on Nomsa Mahlangu's My data rights.
Complete request

######## BOARD R3-C11-1280.dc.html — R3-C11 · A requester who signs in through an institution: the name radios disabled · 1280 · Illustration
> IllustrationR3-C11 · 1280 · focus: none · the correction records as Other (F6)
Wombat
Acting as Coordinator
MenuClose
Acting as Coordinator
- Home
- Programme trainees
- Waiting for assessors
- Decisions due
- MSF campaigns
- Committee reviews
- Decision panels
- Data rights requests
- My data rights
Pieter Smit
Sign out
- Home
- Data rights requests
- ‹request id›
### Rectification request from ‹name›
Submitted ‹time› SAST · Kgosi Kgari Teaching Hospital
#### Request
**Requester
‹name›
**Email
‹address›
**User id
‹user id›
**Request id
‹request id›
**Type
Rectification
**Status
Approved
**Reason
‹reason›
#### Decision
**Decision
Approved
**Decided by
Pieter Smit
43ad0cb7-5ce6-46d7-8595-7117d130c7dc
**Decided on
‹time› SAST
**Note
‹note›
#### Correction
Record each change as it is made on the page that holds the data.
No correction is recorded yet.
Record * required
[disabled]
Account › First name
This name comes from ‹institution›'s sign-in; the institution corrects it.
[disabled]
Account › Last name
This name comes from ‹institution›'s sign-in; the institution corrects it.
Trainee profile › Programme start
Changed on ‹name›'s programme page, which an Institutional admin at Kgosi Kgari Teaching Hospital opens; you cannot.
Other
Name the record in the field under it.
Value before *required
Value after *required
Record correction
Once every correction is made and recorded, complete the request. It then reads Completed here and on ‹name›'s My data rights.
Complete request

######## BOARD R3-C01-390.dc.html — R3-C01 · A.1.8a · approved by Pieter Smit: the Correction section opens · 390
> R3-C01 · 390 · focus: the result, top of Decision · before the corrective save
Wombat
Acting as Coordinator
MenuClose
Acting as Coordinator
- Home
- Programme trainees
- Waiting for assessors
- Decisions due
- MSF campaigns
- Committee reviews
- Decision panels
- Data rights requests
- My data rights
Pieter Smit
Sign out
- Home
- Data rights requests
- ‹request id›
### Rectification request from Nomsa Mahlngu
Submitted ‹time› SAST · Kgosi Kgari Teaching Hospital
#### Request
**Requester
Nomsa Mahlngu
**Email
mahlangu@kgk.wombat.local
**User id
d04f4dfb-f855-4345-9efa-edd168d5a2b6
**Request id
‹request id›
**Type
Rectification
**Status
Approved
**Reason
‹reason›
#### Decision
Request approved. Nomsa Mahlngu is emailed. Record each correction below, then complete the request.
**Decision
Approved
**Decided by
Pieter Smit
43ad0cb7-5ce6-46d7-8595-7117d130c7dc
**Decided on
‹time› SAST
**Note
‹note›
#### Correction
Record each change as it is made on the page that holds the data.
No correction is recorded yet.
Record * required
Account › First name
Changed by Nomsa Mahlngu on My account. No staff page changes a name.
Account › Last name
Changed by Nomsa Mahlngu on My account. No staff page changes a name.
Trainee profile › Programme start
Changed on Nomsa Mahlngu's programme page, which an Institutional admin at Kgosi Kgari Teaching Hospital opens; you cannot.
Other
Name the record in the field under it.
Value before *required
Value after *required
Record correction
Once every correction is made and recorded, complete the request. It then reads Completed here and on Nomsa Mahlngu's My data rights.
Complete request

######## BOARD R3-C02-390.dc.html — R3-C02 · A.1.8a · approved, nothing recorded: the four records · 390
> R3-C02 · 390 · focus: none (page load) · before the corrective save
Wombat
Acting as Coordinator
MenuClose
Acting as Coordinator
- Home
- Programme trainees
- Waiting for assessors
- Decisions due
- MSF campaigns
- Committee reviews
- Decision panels
- Data rights requests
- My data rights
Pieter Smit
Sign out
- Home
- Data rights requests
- ‹request id›
### Rectification request from Nomsa Mahlngu
Submitted ‹time› SAST · Kgosi Kgari Teaching Hospital
#### Request
**Requester
Nomsa Mahlngu
**Email
mahlangu@kgk.wombat.local
**User id
d04f4dfb-f855-4345-9efa-edd168d5a2b6
**Request id
‹request id›
**Type
Rectification
**Status
Approved
**Reason
‹reason›
#### Decision
**Decision
Approved
**Decided by
Pieter Smit
43ad0cb7-5ce6-46d7-8595-7117d130c7dc
**Decided on
‹time› SAST
**Note
‹note›
#### Correction
Record each change as it is made on the page that holds the data.
No correction is recorded yet.
Record * required
Account › First name
Changed by Nomsa Mahlngu on My account. No staff page changes a name.
Account › Last name
Changed by Nomsa Mahlngu on My account. No staff page changes a name.
Trainee profile › Programme start
Changed on Nomsa Mahlngu's programme page, which an Institutional admin at Kgosi Kgari Teaching Hospital opens; you cannot.
Other
Name the record in the field under it.
Value before *required
Value after *required
Record correction
Once every correction is made and recorded, complete the request. It then reads Completed here and on Nomsa Mahlngu's My data rights.
Complete request

######## BOARD R3-C03-390.dc.html — R3-C03 · A.1.8a · Complete request with none recorded: refused · 390
> R3-C03 · 390 · focus: Complete request (the refusal in the section's ActionResult) · before the corrective save
Wombat
Acting as Coordinator
MenuClose
Acting as Coordinator
- Home
- Programme trainees
- Waiting for assessors
- Decisions due
- MSF campaigns
- Committee reviews
- Decision panels
- Data rights requests
- My data rights
Pieter Smit
Sign out
- Home
- Data rights requests
- ‹request id›
### Rectification request from Nomsa Mahlngu
Submitted ‹time› SAST · Kgosi Kgari Teaching Hospital
#### Request
**Requester
Nomsa Mahlngu
**Email
mahlangu@kgk.wombat.local
**User id
d04f4dfb-f855-4345-9efa-edd168d5a2b6
**Request id
‹request id›
**Type
Rectification
**Status
Approved
**Reason
‹reason›
#### Decision
**Decision
Approved
**Decided by
Pieter Smit
43ad0cb7-5ce6-46d7-8595-7117d130c7dc
**Decided on
‹time› SAST
**Note
‹note›
#### Correction
Record each change as it is made on the page that holds the data.
No correction is recorded yet.
Record * required
Account › First name
Changed by Nomsa Mahlngu on My account. No staff page changes a name.
Account › Last name
Changed by Nomsa Mahlngu on My account. No staff page changes a name.
Trainee profile › Programme start
Changed on Nomsa Mahlngu's programme page, which an Institutional admin at Kgosi Kgari Teaching Hospital opens; you cannot.
Other
Name the record in the field under it.
Value before *required
Value after *required
Record correction
Record at least one correction before completing the request.
Once every correction is made and recorded, complete the request. It then reads Completed here and on Nomsa Mahlngu's My data rights.
Complete request

######## BOARD R3-C04-390.dc.html — R3-C04 · Record correction with nothing chosen or typed · 390 · Illustration
> IllustrationR3-C04 · 390 · focus: the first record's radio · refuses A.1.8a's record, before the corrective save
Wombat
Acting as Coordinator
MenuClose
Acting as Coordinator
- Home
- Programme trainees
- Waiting for assessors
- Decisions due
- MSF campaigns
- Committee reviews
- Decision panels
- Data rights requests
- My data rights
Pieter Smit
Sign out
- Home
- Data rights requests
- ‹request id›
### Rectification request from Nomsa Mahlngu
Submitted ‹time› SAST · Kgosi Kgari Teaching Hospital
#### Request
**Requester
Nomsa Mahlngu
**Email
mahlangu@kgk.wombat.local
**User id
d04f4dfb-f855-4345-9efa-edd168d5a2b6
**Request id
‹request id›
**Type
Rectification
**Status
Approved
**Reason
‹reason›
#### Decision
**Decision
Approved
**Decided by
Pieter Smit
43ad0cb7-5ce6-46d7-8595-7117d130c7dc
**Decided on
‹time› SAST
**Note
‹note›
#### Correction
Record each change as it is made on the page that holds the data.
No correction is recorded yet.
Record * required
Account › First name
Changed by Nomsa Mahlngu on My account. No staff page changes a name.
Account › Last name
Changed by Nomsa Mahlngu on My account. No staff page changes a name.
Trainee profile › Programme start
Changed on Nomsa Mahlngu's programme page, which an Institutional admin at Kgosi Kgari Teaching Hospital opens; you cannot.
Other
Name the record in the field under it.
Choose the record.
Value before *required
Enter the value before.
Value after *required
Enter the value after.
Record correction
Once every correction is made and recorded, complete the request. It then reads Completed here and on Nomsa Mahlngu's My data rights.
Complete request

######## BOARD R3-C05-390.dc.html — R3-C05 · A.1.8a · one recorded: Account › Last name · 390
> R3-C05 · 390 · focus: “Correction recorded: …” · after the corrective save
Wombat
Acting as Coordinator
MenuClose
Acting as Coordinator
- Home
- Programme trainees
- Waiting for assessors
- Decisions due
- MSF campaigns
- Committee reviews
- Decision panels
- Data rights requests
- My data rights
Pieter Smit
Sign out
- Home
- Data rights requests
- ‹request id›
### Rectification request from Nomsa Mahlangu
Submitted ‹time› SAST · Kgosi Kgari Teaching Hospital
#### Request
**Requester
Nomsa Mahlangu
**Email
mahlangu@kgk.wombat.local
**User id
d04f4dfb-f855-4345-9efa-edd168d5a2b6
**Request id
‹request id›
**Type
Rectification
**Status
Approved
**Reason
‹reason›
#### Decision
**Decision
Approved
**Decided by
Pieter Smit
43ad0cb7-5ce6-46d7-8595-7117d130c7dc
**Decided on
‹time› SAST
**Note
‹note›
#### Correction
Correction recorded: Account › Last name.
Record each change as it is made on the page that holds the data.
Corrections recorded on this request
| Record | Value before | Value after | Recorded |
| Account › Last name | Mahlngu | Mahlangu | Pieter Smit · ‹time› SAST |
Record * required
Account › First name
Changed by Nomsa Mahlangu on My account. No staff page changes a name.
Account › Last name
Changed by Nomsa Mahlangu on My account. No staff page changes a name.
Trainee profile › Programme start
Changed on Nomsa Mahlangu's programme page, which an Institutional admin at Kgosi Kgari Teaching Hospital opens; you cannot.
Other
Name the record in the field under it.
Value before *required
Value after *required
Record correction
Once every correction is made and recorded, complete the request. It then reads Completed here and on Nomsa Mahlangu's My data rights.
Complete request

######## BOARD R3-C06-390.dc.html — R3-C06 · A.1.8a · completed · 390
> R3-C06 · 390 · focus: the result, top of Correction · after the corrective save
Wombat
Acting as Coordinator
MenuClose
Acting as Coordinator
- Home
- Programme trainees
- Waiting for assessors
- Decisions due
- MSF campaigns
- Committee reviews
- Decision panels
- Data rights requests
- My data rights
Pieter Smit
Sign out
- Home
- Data rights requests
- ‹request id›
### Rectification request from Nomsa Mahlangu
Submitted ‹time› SAST · Kgosi Kgari Teaching Hospital
#### Request
**Requester
Nomsa Mahlangu
**Email
mahlangu@kgk.wombat.local
**User id
d04f4dfb-f855-4345-9efa-edd168d5a2b6
**Request id
‹request id›
**Type
Rectification
**Status
Completed
**Reason
‹reason›
#### Decision
**Decision
Approved
**Decided by
Pieter Smit
43ad0cb7-5ce6-46d7-8595-7117d130c7dc
**Decided on
‹time› SAST
**Note
‹note›
**Completed on
‹time› SAST
#### Correction
Request completed. Nomsa Mahlangu is emailed, and reads it as Completed on My data rights.
Corrections recorded on this request
| Record | Value before | Value after | Recorded |
| Account › Last name | Mahlngu | Mahlangu | Pieter Smit · ‹time› SAST |

######## BOARD R3-C07-390.dc.html — R3-C07 · devadmin's view: the programme-start line links · 390 · Illustration
> IllustrationR3-C07 · 390 · focus: none · before the corrective save
Wombat
Acting as Administrator
MenuClose
Acting as Administrator
- Home
Platform
- Scheduled jobs
- Audit log
- SSO mappings
- Data rights requests
Organisations
- Institutions
- Colleges
People
- Users
- Invitations
Catalogue
- EPAs
- Curricula
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- My data rights
Demo Administrator
Sign out
- Home
- Data rights requests
- ‹request id›
### Rectification request from Nomsa Mahlngu
Submitted ‹time› SAST · Kgosi Kgari Teaching Hospital
#### Request
**Requester
Nomsa Mahlngu
**Email
mahlangu@kgk.wombat.local
**User id
d04f4dfb-f855-4345-9efa-edd168d5a2b6
**Request id
‹request id›
**Type
Rectification
**Status
Approved
**Reason
‹reason›
#### Decision
**Decision
Approved
**Decided by
Pieter Smit
43ad0cb7-5ce6-46d7-8595-7117d130c7dc
**Decided on
‹time› SAST
**Note
‹note›
#### Correction
Record each change as it is made on the page that holds the data.
No correction is recorded yet.
Record * required
Account › First name
Changed by Nomsa Mahlngu on My account. No staff page changes a name.
Account › Last name
Changed by Nomsa Mahlngu on My account. No staff page changes a name.
Trainee profile › Programme start
Changed on Nomsa Mahlngu's programme page. Open Nomsa Mahlngu's programme
Other
Name the record in the field under it.
Value before *required
Value after *required
Record correction
Once every correction is made and recorded, complete the request. It then reads Completed here and on Nomsa Mahlngu's My data rights.
Complete request

######## BOARD R3-C08-390.dc.html — R3-C08 · An ended programme's requester: the archived line · 390 · Illustration
> IllustrationR3-C08 · 390 · focus: none
Wombat
Acting as Coordinator
MenuClose
Acting as Coordinator
- Home
- Programme trainees
- Waiting for assessors
- Decisions due
- MSF campaigns
- Committee reviews
- Decision panels
- Data rights requests
- My data rights
Pieter Smit
Sign out
- Home
- Data rights requests
- ‹request id›
### Rectification request from ‹name›
Submitted ‹time› SAST · Kgosi Kgari Teaching Hospital
#### Request
**Requester
‹name›
**Email
‹address›
**User id
‹user id›
**Request id
‹request id›
**Type
Rectification
**Status
Approved
**Reason
‹reason›
#### Decision
**Decision
Approved
**Decided by
Pieter Smit
43ad0cb7-5ce6-46d7-8595-7117d130c7dc
**Decided on
‹time› SAST
**Note
‹note›
#### Correction
Record each change as it is made on the page that holds the data.
No correction is recorded yet.
Record * required
Account › First name
Changed by ‹name› on My account. No staff page changes a name.
Account › Last name
Changed by ‹name› on My account. No staff page changes a name.
Trainee profile › Programme start
This programme has ended, so its record is archived and cannot be changed.
Other
Name the record in the field under it.
Value before *required
Value after *required
Record correction
Once every correction is made and recorded, complete the request. It then reads Completed here and on ‹name›'s My data rights.
Complete request

######## BOARD R3-C09-390.dc.html — R3-C09 · Five corrections recorded; Other chosen with its name · 390 · Illustration
> IllustrationR3-C09 · 390 · focus: none
Wombat
Acting as Coordinator
MenuClose
Acting as Coordinator
- Home
- Programme trainees
- Waiting for assessors
- Decisions due
- MSF campaigns
- Committee reviews
- Decision panels
- Data rights requests
- My data rights
Pieter Smit
Sign out
- Home
- Data rights requests
- ‹request id›
### Rectification request from ‹name›
Submitted ‹time› SAST · Kgosi Kgari Teaching Hospital
#### Request
**Requester
‹name›
**Email
‹address›
**User id
‹user id›
**Request id
‹request id›
**Type
Rectification
**Status
Approved
**Reason
‹reason›
#### Decision
**Decision
Approved
**Decided by
Pieter Smit
43ad0cb7-5ce6-46d7-8595-7117d130c7dc
**Decided on
‹time› SAST
**Note
‹note›
#### Correction
Record each change as it is made on the page that holds the data.
Corrections recorded on this request
| Record | Value before | Value after | Recorded |
| Account › First name | ‹before› | ‹after› | Pieter Smit · ‹time› SAST |
| Account › Last name | ‹before› | ‹after› | Pieter Smit · ‹time› SAST |
| Trainee profile › Programme start | ‹before› | ‹after› | ‹recorder› · ‹time› SAST |
| Other › ‹record› | ‹before› | ‹after› | Pieter Smit · ‹time› SAST |
| Other › ‹record› | ‹before› | ‹after› | Pieter Smit · ‹time› SAST |
Record * required
Account › First name
Changed by ‹name› on My account. No staff page changes a name.
Account › Last name
Changed by ‹name› on My account. No staff page changes a name.
Trainee profile › Programme start
Changed on ‹name›'s programme page, which an Institutional admin at Kgosi Kgari Teaching Hospital opens; you cannot.
[checked]
Other
Name the record in the field under it.
Record's name *required
[value: ‹the record's name›]
Value before *required
Value after *required
Record correction
Once every correction is made and recorded, complete the request. It then reads Completed here and on ‹name›'s My data rights.
Complete request

######## BOARD R3-C10-390.dc.html — R3-C10 · A failed record: the values kept · 390 · Illustration
> IllustrationR3-C10 · 390 · focus: the failure alert · fails A.1.8a's record, after the corrective save
Wombat
Acting as Coordinator
MenuClose
Acting as Coordinator
- Home
- Programme trainees
- Waiting for assessors
- Decisions due
- MSF campaigns
- Committee reviews
- Decision panels
- Data rights requests
- My data rights
Pieter Smit
Sign out
- Home
- Data rights requests
- ‹request id›
### Rectification request from Nomsa Mahlangu
Submitted ‹time› SAST · Kgosi Kgari Teaching Hospital
#### Request
**Requester
Nomsa Mahlangu
**Email
mahlangu@kgk.wombat.local
**User id
d04f4dfb-f855-4345-9efa-edd168d5a2b6
**Request id
‹request id›
**Type
Rectification
**Status
Approved
**Reason
‹reason›
#### Decision
**Decision
Approved
**Decided by
Pieter Smit
43ad0cb7-5ce6-46d7-8595-7117d130c7dc
**Decided on
‹time› SAST
**Note
‹note›
#### Correction
The correction could not be recorded just now. Nothing has changed. Try again, or come back in a few minutes.
Record each change as it is made on the page that holds the data.
No correction is recorded yet.
Record * required
Account › First name
Changed by Nomsa Mahlangu on My account. No staff page changes a name.
[checked]
Account › Last name
Changed by Nomsa Mahlangu on My account. No staff page changes a name.
Trainee profile › Programme start
Changed on Nomsa Mahlangu's programme page, which an Institutional admin at Kgosi Kgari Teaching Hospital opens; you cannot.
Other
Name the record in the field under it.
Value before *required
[value: Mahlngu]
Value after *required
[value: Mahlangu]
Record correction
Once every correction is made and recorded, complete the request. It then reads Completed here and on Nomsa Mahlangu's My data rights.
Complete request

######## BOARD R3-C11-390.dc.html — R3-C11 · A requester who signs in through an institution: the name radios disabled · 390 · Illustration
> IllustrationR3-C11 · 390 · focus: none · the correction records as Other (F6)
Wombat
Acting as Coordinator
MenuClose
Acting as Coordinator
- Home
- Programme trainees
- Waiting for assessors
- Decisions due
- MSF campaigns
- Committee reviews
- Decision panels
- Data rights requests
- My data rights
Pieter Smit
Sign out
- Home
- Data rights requests
- ‹request id›
### Rectification request from ‹name›
Submitted ‹time› SAST · Kgosi Kgari Teaching Hospital
#### Request
**Requester
‹name›
**Email
‹address›
**User id
‹user id›
**Request id
‹request id›
**Type
Rectification
**Status
Approved
**Reason
‹reason›
#### Decision
**Decision
Approved
**Decided by
Pieter Smit
43ad0cb7-5ce6-46d7-8595-7117d130c7dc
**Decided on
‹time› SAST
**Note
‹note›
#### Correction
Record each change as it is made on the page that holds the data.
No correction is recorded yet.
Record * required
[disabled]
Account › First name
This name comes from ‹institution›'s sign-in; the institution corrects it.
[disabled]
Account › Last name
This name comes from ‹institution›'s sign-in; the institution corrects it.
Trainee profile › Programme start
Changed on ‹name›'s programme page, which an Institutional admin at Kgosi Kgari Teaching Hospital opens; you cannot.
Other
Name the record in the field under it.
Value before *required
Value after *required
Record correction
Once every correction is made and recorded, complete the request. It then reads Completed here and on ‹name›'s My data rights.
Complete request

######## BOARD R3-C02-768.dc.html — R3-C02 · A.1.8a · approved, nothing recorded: the four records · 768
> R3-C02 · 768 · focus: none (page load) · before the corrective save
Wombat
Acting as Coordinator
MenuClose
Acting as Coordinator
- Home
- Programme trainees
- Waiting for assessors
- Decisions due
- MSF campaigns
- Committee reviews
- Decision panels
- Data rights requests
- My data rights
Pieter Smit
Sign out
- Home
- Data rights requests
- ‹request id›
### Rectification request from Nomsa Mahlngu
Submitted ‹time› SAST · Kgosi Kgari Teaching Hospital
#### Request
**Requester
Nomsa Mahlngu
**Email
mahlangu@kgk.wombat.local
**User id
d04f4dfb-f855-4345-9efa-edd168d5a2b6
**Request id
‹request id›
**Type
Rectification
**Status
Approved
**Reason
‹reason›
#### Decision
**Decision
Approved
**Decided by
Pieter Smit
43ad0cb7-5ce6-46d7-8595-7117d130c7dc
**Decided on
‹time› SAST
**Note
‹note›
#### Correction
Record each change as it is made on the page that holds the data.
No correction is recorded yet.
Record * required
Account › First name
Changed by Nomsa Mahlngu on My account. No staff page changes a name.
Account › Last name
Changed by Nomsa Mahlngu on My account. No staff page changes a name.
Trainee profile › Programme start
Changed on Nomsa Mahlngu's programme page, which an Institutional admin at Kgosi Kgari Teaching Hospital opens; you cannot.
Other
Name the record in the field under it.
Value before *required
Value after *required
Record correction
Once every correction is made and recorded, complete the request. It then reads Completed here and on Nomsa Mahlngu's My data rights.
Complete request

######## BOARD R3-C05-768.dc.html — R3-C05 · A.1.8a · one recorded: Account › Last name · 768
> R3-C05 · 768 · focus: “Correction recorded: …” · after the corrective save
Wombat
Acting as Coordinator
MenuClose
Acting as Coordinator
- Home
- Programme trainees
- Waiting for assessors
- Decisions due
- MSF campaigns
- Committee reviews
- Decision panels
- Data rights requests
- My data rights
Pieter Smit
Sign out
- Home
- Data rights requests
- ‹request id›
### Rectification request from Nomsa Mahlangu
Submitted ‹time› SAST · Kgosi Kgari Teaching Hospital
#### Request
**Requester
Nomsa Mahlangu
**Email
mahlangu@kgk.wombat.local
**User id
d04f4dfb-f855-4345-9efa-edd168d5a2b6
**Request id
‹request id›
**Type
Rectification
**Status
Approved
**Reason
‹reason›
#### Decision
**Decision
Approved
**Decided by
Pieter Smit
43ad0cb7-5ce6-46d7-8595-7117d130c7dc
**Decided on
‹time› SAST
**Note
‹note›
#### Correction
Correction recorded: Account › Last name.
Record each change as it is made on the page that holds the data.
Corrections recorded on this request
| Record | Value before | Value after | Recorded |
| Account › Last name | Mahlngu | Mahlangu | Pieter Smit · ‹time› SAST |
Record * required
Account › First name
Changed by Nomsa Mahlangu on My account. No staff page changes a name.
Account › Last name
Changed by Nomsa Mahlangu on My account. No staff page changes a name.
Trainee profile › Programme start
Changed on Nomsa Mahlangu's programme page, which an Institutional admin at Kgosi Kgari Teaching Hospital opens; you cannot.
Other
Name the record in the field under it.
Value before *required
Value after *required
Record correction
Once every correction is made and recorded, complete the request. It then reads Completed here and on Nomsa Mahlangu's My data rights.
Complete request

######## BOARD R3-C09-768.dc.html — R3-C09 · Five corrections recorded; Other chosen with its name · 768 · Illustration
> IllustrationR3-C09 · 768 · focus: none
Wombat
Acting as Coordinator
MenuClose
Acting as Coordinator
- Home
- Programme trainees
- Waiting for assessors
- Decisions due
- MSF campaigns
- Committee reviews
- Decision panels
- Data rights requests
- My data rights
Pieter Smit
Sign out
- Home
- Data rights requests
- ‹request id›
### Rectification request from ‹name›
Submitted ‹time› SAST · Kgosi Kgari Teaching Hospital
#### Request
**Requester
‹name›
**Email
‹address›
**User id
‹user id›
**Request id
‹request id›
**Type
Rectification
**Status
Approved
**Reason
‹reason›
#### Decision
**Decision
Approved
**Decided by
Pieter Smit
43ad0cb7-5ce6-46d7-8595-7117d130c7dc
**Decided on
‹time› SAST
**Note
‹note›
#### Correction
Record each change as it is made on the page that holds the data.
Corrections recorded on this request
| Record | Value before | Value after | Recorded |
| Account › First name | ‹before› | ‹after› | Pieter Smit · ‹time› SAST |
| Account › Last name | ‹before› | ‹after› | Pieter Smit · ‹time› SAST |
| Trainee profile › Programme start | ‹before› | ‹after› | ‹recorder› · ‹time› SAST |
| Other › ‹record› | ‹before› | ‹after› | Pieter Smit · ‹time› SAST |
| Other › ‹record› | ‹before› | ‹after› | Pieter Smit · ‹time› SAST |
Record * required
Account › First name
Changed by ‹name› on My account. No staff page changes a name.
Account › Last name
Changed by ‹name› on My account. No staff page changes a name.
Trainee profile › Programme start
Changed on ‹name›'s programme page, which an Institutional admin at Kgosi Kgari Teaching Hospital opens; you cannot.
[checked]
Other
Name the record in the field under it.
Record's name *required
[value: ‹the record's name›]
Value before *required
Value after *required
Record correction
Once every correction is made and recorded, complete the request. It then reads Completed here and on ‹name›'s My data rights.
Complete request

######## BOARD R3-K01-1280.dc.html — R3-K01 · Pieter Smit opens the kept request after the erasure · 1280
> R3-K01 · 1280 · focus: none (page load)
Wombat
Acting as Coordinator
MenuClose
Acting as Coordinator
- Home
- Programme trainees
- Waiting for assessors
- Decisions due
- MSF campaigns
- Committee reviews
- Decision panels
- Data rights requests
- My data rights
Pieter Smit
Sign out
- Home
- Data rights requests
- 01a17f64-f566-75f0-91ba-5d71863b77d0
### Erasure request from deleted_user_0ae1d64b
Submitted 2026-10-28 00:04 SAST · Kgosi Kgari Teaching Hospital
This person's personal data was erased on 2026-10-28, at this request. The request is kept as the record of it, under the pseudonym.
#### Request
**Requester
deleted_user_0ae1d64bErased 2026-10-28
**User id
4e5ce9ca-438b-4d98-8635-a7259638d7c9
**Request id
01a17f64-f566-75f0-91ba-5d71863b77d0
**Type
Erasure
**Status
Completed
**Reason
I am leaving the programme and want my personal data removed.
#### Decision
**Decision
Approved
**Decided by
Demo Administrator
abd4ed66-e9ad-4187-b87f-c3c481d05c30
**Decided on
2026-10-28 00:04 SAST
**Note
Identity confirmed. Programme records are kept under a pseudonym.
**Completed on
2026-10-28 00:04 SAST

######## BOARD R3-K02-1280.dc.html — R3-K02 · Pieter Smit's queue after the erasure of a person with two earlier requests · 1280 · Illustration
> IllustrationR3-K02 · 1280 · focus: none (page load) · every row of the person rewritten (F3)
Wombat
Acting as Coordinator
MenuClose
Acting as Coordinator
- Home
- Programme trainees
- Waiting for assessors
- Decisions due
- MSF campaigns
- Committee reviews
- Decision panels
- Data rights requests
- My data rights
Pieter Smit
Sign out
### Data rights requests
Requests from people at Kgosi Kgari Teaching Hospital about their personal data: decide each, and carry an approved correction through.
[aria-label: Filter Data rights requests]
Type
All types
Access
Export
Rectification
Erasure
Status
All statuses
Submitted
Approved
Completed
Rejected
Withdrawn
Show
#### Requests (3)
Kgosi Kgari Teaching Hospital's data rights requests, newest first
| Submitted | Requester | Type | Status | Request id | Actions |
| ‹date› ‹time› SAST | deleted_user_‹…›Erased ‹date› | Erasure | Completed | ‹request id› | [aria-label: Review the Erasure request from deleted_user_‹…›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | deleted_user_‹…›Erased ‹date› | Access | Withdrawn | ‹request id› | [aria-label: Review the Access request from deleted_user_‹…›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | deleted_user_‹…›Erased ‹date› | ‹type› | Completed | ‹request id› | [aria-label: Review the ‹type› request from deleted_user_‹…›, ‹date› ‹time› SAST] Review |

######## BOARD R3-U01-1280.dc.html — R3-U01 · A.5.7 · the changed last digit: Request unavailable · 1280
> R3-U01 · 1280 · focus: none (page load: the h1) · the read returned null (unknown id)
Wombat
Acting as Coordinator
MenuClose
Acting as Coordinator
- Home
- Programme trainees
- Waiting for assessors
- Decisions due
- MSF campaigns
- Committee reviews
- Decision panels
- Data rights requests
- My data rights
Pieter Smit
Sign out
- Home
- Data rights requests
- Request unavailable
### Request unavailable
This data rights request does not exist, or you cannot open it.
Go to Data rights requests

######## BOARD R3-U02-1280.dc.html — R3-U02 · Another institution's id: the same page, word for word · 1280 · Illustration
> IllustrationR3-U02 · 1280 · focus: none (page load: the h1) · the read returned null (foreign id)
Wombat
Acting as Coordinator
MenuClose
Acting as Coordinator
- Home
- Programme trainees
- Waiting for assessors
- Decisions due
- MSF campaigns
- Committee reviews
- Decision panels
- Data rights requests
- My data rights
Pieter Smit
Sign out
- Home
- Data rights requests
- Request unavailable
### Request unavailable
This data rights request does not exist, or you cannot open it.
Go to Data rights requests

######## BOARD R3-K01-390.dc.html — R3-K01 · Pieter Smit opens the kept request after the erasure · 390
> R3-K01 · 390 · focus: none (page load)
Wombat
Acting as Coordinator
MenuClose
Acting as Coordinator
- Home
- Programme trainees
- Waiting for assessors
- Decisions due
- MSF campaigns
- Committee reviews
- Decision panels
- Data rights requests
- My data rights
Pieter Smit
Sign out
- Home
- Data rights requests
- 01a17f64-f566-75f0-91ba-5d71863b77d0
### Erasure request from deleted_user_0ae1d64b
Submitted 2026-10-28 00:04 SAST · Kgosi Kgari Teaching Hospital
This person's personal data was erased on 2026-10-28, at this request. The request is kept as the record of it, under the pseudonym.
#### Request
**Requester
deleted_user_0ae1d64bErased 2026-10-28
**User id
4e5ce9ca-438b-4d98-8635-a7259638d7c9
**Request id
01a17f64-f566-75f0-91ba-5d71863b77d0
**Type
Erasure
**Status
Completed
**Reason
I am leaving the programme and want my personal data removed.
#### Decision
**Decision
Approved
**Decided by
Demo Administrator
abd4ed66-e9ad-4187-b87f-c3c481d05c30
**Decided on
2026-10-28 00:04 SAST
**Note
Identity confirmed. Programme records are kept under a pseudonym.
**Completed on
2026-10-28 00:04 SAST

######## BOARD R3-K02-390.dc.html — R3-K02 · Pieter Smit's queue after the erasure of a person with two earlier requests · 390 · Illustration
> IllustrationR3-K02 · 390 · focus: none (page load) · every row of the person rewritten (F3)
Wombat
Acting as Coordinator
MenuClose
Acting as Coordinator
- Home
- Programme trainees
- Waiting for assessors
- Decisions due
- MSF campaigns
- Committee reviews
- Decision panels
- Data rights requests
- My data rights
Pieter Smit
Sign out
### Data rights requests
Requests from people at Kgosi Kgari Teaching Hospital about their personal data: decide each, and carry an approved correction through.
[aria-label: Filter Data rights requests]
Type
All types
Access
Export
Rectification
Erasure
Status
All statuses
Submitted
Approved
Completed
Rejected
Withdrawn
Show
#### Requests (3)
Kgosi Kgari Teaching Hospital's data rights requests, newest first
| Submitted | Requester | Type | Status | Request id | Actions |
| ‹date› ‹time› SAST | deleted_user_‹…›Erased ‹date› | Erasure | Completed | ‹request id› | [aria-label: Review the Erasure request from deleted_user_‹…›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | deleted_user_‹…›Erased ‹date› | Access | Withdrawn | ‹request id› | [aria-label: Review the Access request from deleted_user_‹…›, ‹date› ‹time› SAST] Review |
| ‹date› ‹time› SAST | deleted_user_‹…›Erased ‹date› | ‹type› | Completed | ‹request id› | [aria-label: Review the ‹type› request from deleted_user_‹…›, ‹date› ‹time› SAST] Review |

######## BOARD R3-U01-390.dc.html — R3-U01 · A.5.7 · the changed last digit: Request unavailable · 390
> R3-U01 · 390 · focus: none (page load: the h1) · the read returned null (unknown id)
Wombat
Acting as Coordinator
MenuClose
Acting as Coordinator
- Home
- Programme trainees
- Waiting for assessors
- Decisions due
- MSF campaigns
- Committee reviews
- Decision panels
- Data rights requests
- My data rights
Pieter Smit
Sign out
- Home
- Data rights requests
- Request unavailable
### Request unavailable
This data rights request does not exist, or you cannot open it.
Go to Data rights requests

######## BOARD R3-U02-390.dc.html — R3-U02 · Another institution's id: the same page, word for word · 390 · Illustration
> IllustrationR3-U02 · 390 · focus: none (page load: the h1) · the read returned null (foreign id)
Wombat
Acting as Coordinator
MenuClose
Acting as Coordinator
- Home
- Programme trainees
- Waiting for assessors
- Decisions due
- MSF campaigns
- Committee reviews
- Decision panels
- Data rights requests
- My data rights
Pieter Smit
Sign out
- Home
- Data rights requests
- Request unavailable
### Request unavailable
This data rights request does not exist, or you cannot open it.
Go to Data rights requests

