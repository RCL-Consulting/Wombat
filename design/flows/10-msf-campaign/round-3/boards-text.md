# Flow 10 round 3: the text of every board

Generated from `round-3/project/R3-*.dc.html` (T369, build-contracts § 0): style, script and helmet blocks stripped; headings, labels, aria-labels, aria-describedby reasons, captions and annotations kept. Board order is the canvas's. Design data: a caption or chip is never a shipped word.

######## BOARD R3-0-spec.dc.html — R3-0-spec · F1–F11, M1–M10, S1–S17, classes, DESIGN.md, DTOs
## R3-0-spec · Flow 10, the MSF campaign, round 3
The screenshots arrived. Not as attachments (none reached this session again), but read from C:\dev\Wombat\design\upload once the folder was granted: 3.36-2 (crop-first), 3.38-1, 3.44-1, 3.46-1, 3.46-2, 3.47-1 and A.7.5-2, and the old-frame states campaign-report--blocked, --no-epa and campaign-edit--withdrawn. Their orders confirm M1 (Pieter du Plessis above Lerato Molefe) and M2 (Peer doctor, Consultant, Nurse, Allied health professional). The built add form reads "Category" with Nurse chosen after an add; round 3 draws M2's Peer doctor.
Chips: DATA a value no DTO carries today; NEW a component or wording this flow adds; PROPOSED not yet decided; decided, T367 rests on T367's notice path; Illustration a state the replay does not reach. A dashed strip is the board's note; the scrim behind an open dialog is the canvas's. A drawn ring marks where the focus lands. Boards are R3-<key>-<width>, each state's 1280, 768 and 390 side by side.
### F1–F11
The operator's decisions, F1–F11, and where each is drawn
| What changed
| Where
| F1
| While Open the report shows the status card and the checklist only, with a NEW line saying the responses are read once it has closed; group cards and the suppressed line appear once closed.
| R1 (1280, 768, 390), R2, R16
| F2
| Floor of 3 for a respondent group's minimum, refused below 3 ("Enter a number from 3 to 100."). The cast gains a third Consultant and a third Nurse (‹address›, ‹comment›): invited 2 / 3 / 3 / 1 = 9; responded 1 / 3 / 3 / 1 = 8 of 9; Lerato Molefe's minimums 5 / 3 / 2; both shown groups "from 3 responses", their averages ‹n.nn› (the third ratings are not given); suppressed "Fewer than 3". The per-point counts are dropped from the data.
| every L, C, K, R, M board; C8 (the refusal)
| F3
| Her list: Report, Released, View; no Responses column.
| M2, M11
| F4
| No release mail when her programme has ended; R6d's line kept, its PROPOSED chip removed.
| R6d, R10
| F5
| ClosedByUserId stored (null when the hourly job closed it); "Closed by itself on ‹date›, once 2026-10-28 had passed."; "hourly".
| R4
| F6
| The required mark on every required control at every width (Template, Trainee, Opens on, Closes on, the three minimums, the EPA legend, the fold's three fields, the invitee's email, K15's Teaching context); none on the respondent group select; "* marks a field you must fill in." above the create form's first card and in the add form.
| C1–C8, K1–K5c, K13, K15, K20
| F7
| The status stripe as the activity page's: --emphasis on Draft and Under review, activity-status--others while Open, --done Released, --declined Withdrawn.
| every K and R status card
| F8
| Close campaign stays danger: it destroys (the unused links stop working and every address is removed). Release stays primary: it destroys nothing, though it cannot be undone.
| R2, R6a–R6d (Spec only)
| F9
| "Open its campaign page" alone on its line under the subtitle, .review-progress-link (24px, 44px at 390).
| every R board
| F10
| R8a and R9 are Anele Dlamini's: "MSF report for Anele Dlamini", one invitee (a Consultant), 1 response, PAED-012 (R8a) or no EPA (R9), no Illustration. Thresholds are drawn 5 / 3 / 2 as the old-frame captures read them, not ‹n›: say if ‹n› was meant.
| R8a, R9
| F11
| "Invitee added." alone; the address left the result.
| K2
### M1–M10
Must fix, M1–M10
| What changed
| Where
| M1
| Newest first by creation: Pieter du Plessis's row above Lerato Molefe's; the ten keep the order within each semester.
| L2–L7 at every width
| M2
| Peer doctor, Consultant, Nurse, Allied health professional, Other everywhere; the Consultant card above the Nurse card; the select in that order, Peer doctor chosen.
| K1–K5c, K13, K20; R3–R11, R17; M3, M4, M8
| M3
| K4 reads "Opening emails each of the 8 invitees …": F2's nine less Palesa Tau's first address (round 2's 7 less 1 would be 6). K2 and K3 as drawn, recounted for F2.
| K4
| M4
| K15: thresholds "‹n› responses; ‹n› in a respondent group; 1 respondent group reporting" with the note that the groups-reporting minimum is fixed at 1; evidence PAED-015 only; ‹n› invitees; ‹closes›. On create, Learner feedback shows Minimum respondent groups reporting disabled at 1 with that help (Spec only: no learner create board is asked for).
| K15
| M5
| An ended programme: "As Lerato Molefe would have read it."; the release section's first line and R6d's first item "She can no longer open MSF reports; the report is kept with the evidence on her record."; the mail line as F4.
| R6d, R10
| M6
| The DTO table below restated from the DTOs as they stand.
| Spec
| M7
| Labels end in <span class="required-mark" aria-hidden="true"> *</span>; controls carry aria-required="true"; the visually hidden "required" stays on the EPA legend only. R12 changes the markup, not only the look.
| C1–C8, K boards' add form, K15
| M8
| The stripe by class only; no inline colour; K11 and R7 agree ("The report has been released to the trainee.").
| K, R
| M9
| C4's refusal is the system's summary: div.validation-summary-errors.error-summary, role=alert, tabindex=-1, "Not created." bold then what to do; its line a link that moves the focus to the first EPA checkbox (#e1) by PageFocus, never the legend. The summary takes the focus; the fieldset's message stays under its legend.
| C4, C8
| M10
| Dialog rules written against what ships: each ConfirmDialog is rendered inside the page's .msf-page column (the page component's root), so .msf-page dialog .form-actions{flex-direction:column-reverse} and .msf-page dialog .btn{min-height:44px} reach it. Stacked order: the action on top, the safe button last on screen and first in tab order (a rule this flow changes: flow 01's ConfirmDialog stacked nothing).
| L3, K3, K10, R2, R6a–R6d at 390
### S1–S17
Should fix, S1–S17
| What changed
| Where
| S1
| R2 over 3.44's page before the close: Open, so (F1) the met checklist and no group cards, not S1's cards. R6c's backdrop is Lerato Molefe's page with no EPA named: R9 is now Anele Dlamini's and blocked, so no dialog opens over it. R6d over R10's page. R6b, R6c, R6d chipped Illustration; R6d no longer PROPOSED (F4).
| R2, R6b–R6d
| S2
| As F10.
| R8a, R9
| S3
| NEW chips on the hour line, Go to MSF campaigns, "Today, on the South African calendar." (also DATA); trainee help "Current trainees you can run a campaign for." (NEW); K2's result as F11.
| K7, K8, K22, K16, R12, C1–C8
| S4
| K5c: "Enter a valid email address, for example name@example.org." (NEW). C8: "Enter a number from 3 to 100." (F2's floor). The email input is type="email" in a form with novalidate, so the browser stays quiet and the server's words show.
| K5c, C8
| S5
| The template fold is outside the campaign's EditForm: its fields post nothing with Create; Add template is type="button" with its own handler; the Template select joins the campaign form through form="campaign-form". Drawn the same.
| Spec only
| S6
| Her report is its own page component, MyMsfReport (/msf/my-reports/{id}): h1 and tab "Default MSF, released 2026-10-14" (· Wombat), the trail Home › MSF reports › {h1}, MSF reports lit (aria-current="true"). An id not hers or not released goes to her list with one info alert (M5, M11); a failed read is the load error with Try again (M10), never that alert.
| M3, M5, M9–M11
| S7
| A group's h3 row with no scale question reads the group's name alone; with several, the name alone, then each question's average under it ("‹n.nn› average rating, from 3 responses").
| Spec only
| S8
| Added: K19 and R15 (fifteen EPAs, 390), M8 (PAED-015 on her portfolio, 390); M9, M10, M11; K20 refused Open, K21 refused Withdraw, K22 refused Resend, R16 refused Close, R17 refused Release (danger Alert, the focus left on the button; refusal words ‹as built›); Close's result is R3's; Q9 the refused post; 768 for C3, K7, V1 and R6a.
| as listed
| S9
| Each state keeps its Alert under its h1: success thank you, info used, warning closed, expired and not recognised, danger the fault.
| Q3–Q8
| S10
| Row actions in div.actions-cell; the check line is .submit-check; every class in the table below.
| every board
| S11
| The DESIGN.md list below.
| Spec
| S12
| The EPA card opens with an h2 ("Evidence"), the legend inside it; its invalid state .choice-group.is-invalid with its forced-colors border.
| C1–C8
| S13
| V1 at 768; coverage's named, focusable scroll regions are R9's one deliberate exception to "nothing scrolls sideways"; the count tables on K and R stay unstacked (two or three narrow columns fit 358px).
| V1, Spec
| S14
| The add form stacks at 900px and below; Add invitee by align-self:end, no margin-top.
| K2 at 768
| S15
| "withdraw it on its campaign page" an inline-block 24px link (.msf-inline-link, flow 08's rule, 44px at 390), in the help and in the disabled Release's reason; F9's link.
| R3–R11, R8a, R8b, R9
| S16
| No inline style but Skeleton's Width and Height (the component's own); .msf-page--measure, .section-title--h3, .msf-stack-card and the small layout classes in place of style.
| every board
| S17
| K10 from WithdrawBody's under-review form: its known opening, then the draft's tail ("A withdrawn campaign is never released to the trainee, and withdrawing cannot be undone."): check against MsfCampaignText. K12 from StateNote (the old-frame capture's words). R12 "The selected report could not be loaded." follows the campaign's (the source names only the title). R11's 4,000 characters drawn at length; ‹date› where the replay has none.
| K10, K12, R12, R11
### Classes
Every class a board uses beyond the design system's own (S10)
| Class
| NEW, or reuses
| Why
| msf-header
| NEW
| PageHeader's container, so its actions stack at 390 (New campaign, then MSF coverage)
| msf-page, msf-page--measure, msf-measure, msf-stack-card
| NEW
| the page column (24px between sections; it holds the open dialog, M10); the report's 60rem measure; a section's stack
| section-title, section-title--h3
| NEW (reuses list-section-title's scale)
| a card's h2 at 1.1rem and its h3 at 1rem
| status-card, status-head, status-line, status-actions, status-split
| NEW, on .detail-card with --emphasis or activity-status--others / --done / --declined
| the campaign's and the report's status card (F7)
| msf-facts, msf-facts-wide, epa-lines
| NEW (a details list in columns; details-list stacks one per row)
| About this campaign
| count-table, count-box, num, anon-caption
| NEW on .clinic-table
| unstacked count tables (S13); no .shadow inside a card, whose own frame is the lift
| links-part
| NEW
| the open campaign's Links
| invitee-rows, invitee-row, invitee-address, invitee-group
| NEW (replaces the address table)
| Remove in each list item
| add-form, add-invitee
| NEW
| the add form; stacks at 900px (S14)
| threshold-list, report-line, group-card, group-avg, group-question, group-comments, groups-intro, suppressed-line, release-section, release-actions, stored
| NEW
| the report
| submit-check (and its ul)
| reuses SubmitCheck
| the release section's check line
| move-reasons
| reuses ActivityWorkflowActions
| every disabled action's reason
| review-progress-link
| reuses flow 07's
| "Open its campaign page" (F9)
| msf-inline-link
| NEW, flow 08's rule
| a 24px link in running text (S15)
| required-mark
| reuses FormField's
| M7
| choice-group, choice-row
| reuses flow 08's
| the EPA fieldset and its invalid stripe (S12)
| error-summary, validation-summary-errors
| reuses RefusalSummary's and the respondent page's
| M9, Q9
| msf-form-column, msf-card-title, template-fold, template-form, epa-checks, choice-stack, msf-gap-top, msf-narrow-field, msf-centre, msf-block
| NEW
| the create page and small layout
| trainee-cell, kind-line, nowrap
| NEW (person-cell is a name over an address; here a name alone, and the template over its kind)
| list rows
| skeleton-stack
| NEW wrapper around Skeleton (Count could replace it: Skeleton Count="4")
| loading
| respond-card, respond-meta, respond-privacy, respond-privacy-title, respond-privacy-list, respond-form, respond-label, respond-optional, respond-required
| NEW on the wide auth card
| /msf/respond
| my-report, portfolio-links
| NEW
| her report
| coverage-windows, coverage-programme, coverage-table, is-covered
| NEW
| coverage
### DESIGN.md
DESIGN.md text this flow changes (S11 and the rest)
| Section
| The change
| § The MSF campaign page
| Its h4s become h2 and h3. The campaign card gives way to the status card (the state to its badge, the window to the subtitle) and About this campaign. The status stripe as F7. The draft's addresses are list items, Remove in each. The add form's "Category" becomes "Respondent group".
| § The MSF campaign page (report)
| Status first, release last; while Open the checklist only (F1); a blocked release says why in the checklist and beside the disabled Release; "Open its campaign page" on its own line (F9).
| § Form system
| The required mark (M7) on the fields F6 names; the Quick template card becomes the Questionnaire card with its fold (S5); the EPA group's h2, legend and .choice-group.is-invalid (S12); the counter.
| § Table system
| Which tables stack: the campaign list and her list (Stack and stack-wide); the count tables never (S13); coverage's grids scroll in their named regions.
| § Page-level patterns: Detail page and the title rule; the owner table
| Her report page's h1 and tab; /msf/my-reports/{id} → Trainee: MSF reports (S6).
| § ConfirmDialog
| At 390 the actions stack, the action on top and the safe button last on screen (M10).
| § Anonymous static page
| Each state's h2 becomes the card's h1, no tabindex on it; the state's Alert stays under it (S9); the mark 44px.
| § The programme's MSF coverage
| No "Back to campaigns"; "Coverage is not a target." in the subtitle; the scroll regions (S13).
### DTO additions
DTO additions, by query (M6: what already exists is not an addition)
| Query
| Exists today, not an addition
| Added (DATA)
| ListMsfCampaignsForCoordinator
| the rows, ClosesOn, invitations and responses
| HasOpened (Responses "—" when false)
| GetMsfCampaignSetup
| the campaign, its invitees by group
| MinimumResponses, MinimumCategoryResponses, MinimumRespondentCategories, CoveredEpas (code and title), TrainingYear, ClosedOn, ClosedByUserId (F5), SubjectIsCurrent, SubjectEndedOn; the South African date for Opens on's default (S3)
| GetCampaignAggregateReport
| EvidenceRecordedOn, each EPA's Recorded (serving only after release), TeachingContextCount
| SubjectName, OpensOn, ClosesOn, InvitedTotal (the Close dialog's unused links: InvitedTotal less the responses), ClosedOn, ClosedByUserId (F5), ReleasedOn, SubjectIsCurrent, SubjectEndedOn, and each EPA's WillBeRecorded, read before release by the release section's records line and by R6a–R6b. One read, never GetMsfCampaignSetup beside it.
| ListMsfCampaignsForTrainee
| ReleasedOn
| none (F3 drops Responses)
#### Notes and nits
Opens on and Closes on carry the mark at every width (F6); the respondent group select carries none.
C3–C5: Mr Smit typed 5; 3 and 2 are the defaults (8 / 3 / 2).
K13: the address and the group are natively disabled (nothing can be sent); Add invitee and Open campaign are aria-disabled, each with its reason, so they stay in the tab order.
The list's count is a plain count, "Campaigns (2)", as My activities' "All activities (2)".
The dropped EPA is PAED-012 throughout (R6b, R7b, M4).
The counter counts in the browser (flow 09's js); /msf/respond says "Up to 4,000 characters", as the report.
390 boards carry the closed menu's list and the account row (hidden until Menu opens), as flow 09's do.
The canvas's ds/ (tokens.css, bundle.css) is saved with the round.
Learner feedback in words: its questionnaire reads "feedback on the teaching of Lerato Molefe, a trainee who has taught you"; her report reads "Teaching contexts that responded: ‹n›" (TeachingContextCount); its respondent-group minimum is fixed at 1 (M4).
An ended programme's registrar has lost the Trainee role (D54): F4 decides no mail.

######## BOARD R3-0-steps.dc.html — R3-0-steps · the 18 steps' Expects, and each journey's count
## R3-0-steps · the runbook once built
The 18 runbook steps: today's Expect clauses stay, with the new one beside them
| Step
| Expect, once built
| 3.34
| As today, and: New campaign then MSF coverage in the list's header; the Questionnaire card's fold open and Create campaign aria-disabled ("Create campaign: write a questionnaire first."); "Template created. Default MSF is chosen as this campaign's template.", focused.
| 3.35
| As today, and: the page is "New MSF campaign"; "Minimum responses in a respondent group" (at least 3, F2) and "Minimum respondent groups reporting"; minimums 5 / 3 / 2; "2 of 15 ticked"; created, "MSF campaign for Lerato Molefe", Draft, Open aria-disabled with its reason.
| 3.36
| As today (nine colleagues with F2's two), and: the select offers Peer doctor, Consultant, Nurse, Allied health professional, Other, Peer doctor chosen; each add reads "Invitee added."; the rows each with Remove named for its address; counts 2, 3, 3, 1, All groups 9. A select left at its default counts no field.
| 3.37
| As today, and: "0 of 9 invitees have responded."; "9 links are still being sent. …" in the Links part.
| 3.38
| As today, and: Pieter du Plessis's row first (Draft, Invitations 0, Responses "—"), then Lerato Molefe's (Open, 9, 0); Withdraw's dialog Keep the campaign / Withdraw campaign.
| 3.39
| As today, and: one h1, "Feedback on Lerato Molefe"; "Who sees your feedback" before the rating.
| 3.40
| Unchanged, "Feedback link already used" now the card's h1 with its info Alert.
| 3.41
| As F1: the status card (Open, "3 responses, of the 5 needed.", "0 respondent groups reporting, of the 2 needed.") and Close campaign…; no group card, no suppressed line; Release not offered.
| 3.42
| Unchanged (with F2's two more respondents answering).
| 3.43
| As today, with "8 of 9 invitees have responded." and Responded 1, 3, 3, 1.
| 3.44
| As today, and: Close's dialog (1 unused link, 8 responses kept); the result focused; Under review; the narrative and "Supervision level this feedback supports" (Not stated, 1, 2, 3a, 3b, 4, 5) stand above Release; the Consultant card above the Nurse card, each "from 3 responses"; the suppressed line "Peer doctor, Allied health professional".
| 3.45
| Unchanged, "Feedback request closed" with its warning Alert.
| 3.46
| As today, and: the counter 106 of 4,000 characters; Release's dialog with the mail line; "Report released to Lerato Molefe."; the status line "2026-10-14 16:34 SAST · 8 responses, with 5 needed · 2 respondent groups reporting, with 2 needed"; the Consultant card first; the list row Released, 9, 8.
| 3.47
| As today, and: her list Report, Released, View (F3); her own page, h1 "Default MSF, released 2026-10-14", trail Home › MSF reports › it; the Consultant card first, "Consultant · ‹n.nn› average rating", the question once, no ".:"; On your portfolio.
| 3.48
| Unchanged.
| 3.49
| As today, and: no Back to campaigns.
| A.4.7
| Unchanged; L5 stands for the list as loaded.
| A.7.5
| As today, and: Pieter du Plessis's row first; the rows stacked, every field shown, each action a 44px block.
### Journeys
Each journey: presses + fields typed (a select left at its default is no field)
| Journey
| 1280
| 390
| Create the questionnaire and the campaign (3.34–3.35), from Home's Start an MSF campaign: Add template; Trainee and minimum responses (the two fields); tick 2 EPAs; Create campaign
| 5 + 2
| 5 + 2
| Invite nine and remove one (3.36): 9 adds (address, and the group where Peer doctor is not it: 7 of 9), Remove…, Remove invitee, 1 add (Palesa Tau as Nurse)
| 12 + 18
| 12 + 18
| Open (3.37)
| 1 + 0
| 1 + 0
| Withdraw from the list (3.38): Withdraw…, Withdraw campaign
| 2 + 0
| 2 + 0
| Resend (3.43), from the list: Manage, Resend 1 link
| 2 + 0
| 2 + 0
| Close (3.44), from the list: View report, Close campaign…, Close campaign
| 3 + 0
| 3 + 0
| Release (3.46), from the list: Review and release, the narrative, Release to trainee…, Release to Lerato Molefe
| 3 + 1
| 3 + 1
| Sister Mokwena answers (3.39): a point, the comment, Submit feedback
| 2 + 1
| 2 + 1
| Dr Molefe reads her report (3.47), from Home: MSF reports, View
| 2 + 0
| 3 + 0 (Menu first)
| Mr Smit reads coverage (3.49), from the list: MSF coverage
| 1 + 0
| 1 + 0

######## BOARD R3-L1-none-1280.dc.html — MSF campaigns · none (3.34) · 1280
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
## MSF campaigns
Create and review multi-source feedback campaigns.
New campaign MSF coverage
No MSF campaigns
Create the first MSF campaign to start respondent outreach.

######## BOARD R3-L1-none-390.dc.html — MSF campaigns · none (3.34) · 390
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
## MSF campaigns
Create and review multi-source feedback campaigns.
New campaign MSF coverage
No MSF campaigns
Create the first MSF campaign to start respondent outreach.

######## BOARD R3-L2-draft-open-1280.dc.html — MSF campaigns · a draft and an open campaign, newest first (3.38) · 1280
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
## MSF campaigns
Create and review multi-source feedback campaigns.
New campaign MSF coverage
### Campaigns (2)
MSF campaigns, newest first. Counted by respondent group: this list never shows who was invited or who responded.
| Trainee
| Template
| State
| Invitations
| Responses
| Closes
| Actions
| Pieter du Plessis
| Default MSFMulti-source feedback
| Draft
| 0
| —
| 2026-10-28
[aria-label: Manage: the campaign for Pieter du Plessis (Default MSF, closing 2026-10-28)] Manage [aria-label: Withdraw: the campaign for Pieter du Plessis (Default MSF, closing 2026-10-28)] Withdraw…
| Lerato Molefe
| Default MSFMulti-source feedback
| Open
| 9
| 0
| 2026-10-28
[aria-label: Manage: the campaign for Lerato Molefe (Default MSF, closing 2026-10-28)] Manage [aria-label: View report: the campaign for Lerato Molefe (Default MSF, closing 2026-10-28)] View report [aria-label: Withdraw: the campaign for Lerato Molefe (Default MSF, closing 2026-10-28)] Withdraw…

######## BOARD R3-L2-draft-open-768.dc.html — MSF campaigns · a draft and an open campaign, newest first (3.38) · 768
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
## MSF campaigns
Create and review multi-source feedback campaigns.
New campaign MSF coverage
### Campaigns (2)
MSF campaigns, newest first. Counted by respondent group: this list never shows who was invited or who responded.
| Trainee
| Template
| State
| Invitations
| Responses
| Closes
| Actions
| Pieter du Plessis
| Default MSFMulti-source feedback
| Draft
| 0
| —
| 2026-10-28
[aria-label: Manage: the campaign for Pieter du Plessis (Default MSF, closing 2026-10-28)] Manage [aria-label: Withdraw: the campaign for Pieter du Plessis (Default MSF, closing 2026-10-28)] Withdraw…
| Lerato Molefe
| Default MSFMulti-source feedback
| Open
| 9
| 0
| 2026-10-28
[aria-label: Manage: the campaign for Lerato Molefe (Default MSF, closing 2026-10-28)] Manage [aria-label: View report: the campaign for Lerato Molefe (Default MSF, closing 2026-10-28)] View report [aria-label: Withdraw: the campaign for Lerato Molefe (Default MSF, closing 2026-10-28)] Withdraw…

######## BOARD R3-L2-draft-open-390.dc.html — MSF campaigns · a draft and an open campaign, newest first (3.38) · 390
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
## MSF campaigns
Create and review multi-source feedback campaigns.
New campaign MSF coverage
### Campaigns (2)
MSF campaigns, newest first. Counted by respondent group: this list never shows who was invited or who responded.
| Trainee
| Template
| State
| Invitations
| Responses
| Closes
| Actions
| Pieter du Plessis
| Default MSFMulti-source feedback
| Draft
| 0
| —
| 2026-10-28
[aria-label: Manage: the campaign for Pieter du Plessis (Default MSF, closing 2026-10-28)] Manage [aria-label: Withdraw: the campaign for Pieter du Plessis (Default MSF, closing 2026-10-28)] Withdraw…
| Lerato Molefe
| Default MSFMulti-source feedback
| Open
| 9
| 0
| 2026-10-28
[aria-label: Manage: the campaign for Lerato Molefe (Default MSF, closing 2026-10-28)] Manage [aria-label: View report: the campaign for Lerato Molefe (Default MSF, closing 2026-10-28)] View report [aria-label: Withdraw: the campaign for Lerato Molefe (Default MSF, closing 2026-10-28)] Withdraw…

######## BOARD R3-L3-withdraw-dialog-1280.dc.html — MSF campaigns · Withdraw's dialog, focus on Keep the campaign (3.38) · 1280
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
## MSF campaigns
Create and review multi-source feedback campaigns.
New campaign MSF coverage
### Campaigns (2)
MSF campaigns, newest first. Counted by respondent group: this list never shows who was invited or who responded.
| Trainee
| Template
| State
| Invitations
| Responses
| Closes
| Actions
| Pieter du Plessis
| Default MSFMulti-source feedback
| Draft
| 0
| —
| 2026-10-28
[aria-label: Manage: the campaign for Pieter du Plessis (Default MSF, closing 2026-10-28)] Manage [aria-label: Withdraw: the campaign for Pieter du Plessis (Default MSF, closing 2026-10-28)] Withdraw…
| Lerato Molefe
| Default MSFMulti-source feedback
| Open
| 9
| 0
| 2026-10-28
[aria-label: Manage: the campaign for Lerato Molefe (Default MSF, closing 2026-10-28)] Manage [aria-label: View report: the campaign for Lerato Molefe (Default MSF, closing 2026-10-28)] View report [aria-label: Withdraw: the campaign for Lerato Molefe (Default MSF, closing 2026-10-28)] Withdraw…
[aria-describedby: Withdraw the campaign for Pieter du Plessis (Default MSF, closing 2026-10-28)? It will take no more responses: any link already sent stops working, and every respondent's email address is removed. A withdrawn campaign is never released to the trainee, and withdrawing cannot be undone.]
### Withdraw this campaign?
Withdraw the campaign for Pieter du Plessis (Default MSF, closing 2026-10-28)? It will take no more responses: any link already sent stops working, and every respondent's email address is removed. A withdrawn campaign is never released to the trainee, and withdrawing cannot be undone.
Keep the campaign Withdraw campaign

######## BOARD R3-L3-withdraw-dialog-390.dc.html — MSF campaigns · Withdraw's dialog, focus on Keep the campaign (3.38) · 390
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
## MSF campaigns
Create and review multi-source feedback campaigns.
New campaign MSF coverage
### Campaigns (2)
MSF campaigns, newest first. Counted by respondent group: this list never shows who was invited or who responded.
| Trainee
| Template
| State
| Invitations
| Responses
| Closes
| Actions
| Pieter du Plessis
| Default MSFMulti-source feedback
| Draft
| 0
| —
| 2026-10-28
[aria-label: Manage: the campaign for Pieter du Plessis (Default MSF, closing 2026-10-28)] Manage [aria-label: Withdraw: the campaign for Pieter du Plessis (Default MSF, closing 2026-10-28)] Withdraw…
| Lerato Molefe
| Default MSFMulti-source feedback
| Open
| 9
| 0
| 2026-10-28
[aria-label: Manage: the campaign for Lerato Molefe (Default MSF, closing 2026-10-28)] Manage [aria-label: View report: the campaign for Lerato Molefe (Default MSF, closing 2026-10-28)] View report [aria-label: Withdraw: the campaign for Lerato Molefe (Default MSF, closing 2026-10-28)] Withdraw…
[aria-describedby: Withdraw the campaign for Pieter du Plessis (Default MSF, closing 2026-10-28)? It will take no more responses: any link already sent stops working, and every respondent's email address is removed. A withdrawn campaign is never released to the trainee, and withdrawing cannot be undone.]
### Withdraw this campaign?
Withdraw the campaign for Pieter du Plessis (Default MSF, closing 2026-10-28)? It will take no more responses: any link already sent stops working, and every respondent's email address is removed. A withdrawn campaign is never released to the trainee, and withdrawing cannot be undone.
Keep the campaign Withdraw campaign

######## BOARD R3-L4-withdrawn-1280.dc.html — MSF campaigns · withdrawn, the result focused (3.38) · 1280
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
## MSF campaigns
Create and review multi-source feedback campaigns.
New campaign MSF coverage
The campaign for Pieter du Plessis (Default MSF, closing 2026-10-28) has been withdrawn. Its respondents' links no longer work, and their email addresses have been removed.
### Campaigns (2)
MSF campaigns, newest first. Counted by respondent group: this list never shows who was invited or who responded.
| Trainee
| Template
| State
| Invitations
| Responses
| Closes
| Actions
| Pieter du Plessis
| Default MSFMulti-source feedback
| Withdrawn
| 0
| —
| 2026-10-28
[aria-label: View campaign: the campaign for Pieter du Plessis (Default MSF, closing 2026-10-28)] View campaign
| Lerato Molefe
| Default MSFMulti-source feedback
| Open
| 9
| 0
| 2026-10-28
[aria-label: Manage: the campaign for Lerato Molefe (Default MSF, closing 2026-10-28)] Manage [aria-label: View report: the campaign for Lerato Molefe (Default MSF, closing 2026-10-28)] View report [aria-label: Withdraw: the campaign for Lerato Molefe (Default MSF, closing 2026-10-28)] Withdraw…

######## BOARD R3-L4-withdrawn-390.dc.html — MSF campaigns · withdrawn, the result focused (3.38) · 390
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
## MSF campaigns
Create and review multi-source feedback campaigns.
New campaign MSF coverage
The campaign for Pieter du Plessis (Default MSF, closing 2026-10-28) has been withdrawn. Its respondents' links no longer work, and their email addresses have been removed.
### Campaigns (2)
MSF campaigns, newest first. Counted by respondent group: this list never shows who was invited or who responded.
| Trainee
| Template
| State
| Invitations
| Responses
| Closes
| Actions
| Pieter du Plessis
| Default MSFMulti-source feedback
| Withdrawn
| 0
| —
| 2026-10-28
[aria-label: View campaign: the campaign for Pieter du Plessis (Default MSF, closing 2026-10-28)] View campaign
| Lerato Molefe
| Default MSFMulti-source feedback
| Open
| 9
| 0
| 2026-10-28
[aria-label: Manage: the campaign for Lerato Molefe (Default MSF, closing 2026-10-28)] Manage [aria-label: View report: the campaign for Lerato Molefe (Default MSF, closing 2026-10-28)] View report [aria-label: Withdraw: the campaign for Lerato Molefe (Default MSF, closing 2026-10-28)] Withdraw…

######## BOARD R3-L5-released-1280.dc.html — MSF campaigns · released and withdrawn (3.46; stands for A.4.7) · 1280
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
## MSF campaigns
Create and review multi-source feedback campaigns.
New campaign MSF coverage
### Campaigns (2)
MSF campaigns, newest first. Counted by respondent group: this list never shows who was invited or who responded.
| Trainee
| Template
| State
| Invitations
| Responses
| Closes
| Actions
| Pieter du Plessis
| Default MSFMulti-source feedback
| Withdrawn
| 0
| —
| 2026-10-28
[aria-label: View campaign: the campaign for Pieter du Plessis (Default MSF, closing 2026-10-28)] View campaign
| Lerato Molefe
| Default MSFMulti-source feedback
| Released
| 9
| 8
| 2026-10-28
[aria-label: View campaign: the campaign for Lerato Molefe (Default MSF, closing 2026-10-28)] View campaign [aria-label: View report: the campaign for Lerato Molefe (Default MSF, closing 2026-10-28)] View report

######## BOARD R3-L5-released-768.dc.html — MSF campaigns · released and withdrawn (3.46; stands for A.4.7) · 768
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
## MSF campaigns
Create and review multi-source feedback campaigns.
New campaign MSF coverage
### Campaigns (2)
MSF campaigns, newest first. Counted by respondent group: this list never shows who was invited or who responded.
| Trainee
| Template
| State
| Invitations
| Responses
| Closes
| Actions
| Pieter du Plessis
| Default MSFMulti-source feedback
| Withdrawn
| 0
| —
| 2026-10-28
[aria-label: View campaign: the campaign for Pieter du Plessis (Default MSF, closing 2026-10-28)] View campaign
| Lerato Molefe
| Default MSFMulti-source feedback
| Released
| 9
| 8
| 2026-10-28
[aria-label: View campaign: the campaign for Lerato Molefe (Default MSF, closing 2026-10-28)] View campaign [aria-label: View report: the campaign for Lerato Molefe (Default MSF, closing 2026-10-28)] View report

######## BOARD R3-L5-released-390.dc.html — MSF campaigns · released and withdrawn (3.46; stands for A.4.7) · 390
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
## MSF campaigns
Create and review multi-source feedback campaigns.
New campaign MSF coverage
### Campaigns (2)
MSF campaigns, newest first. Counted by respondent group: this list never shows who was invited or who responded.
| Trainee
| Template
| State
| Invitations
| Responses
| Closes
| Actions
| Pieter du Plessis
| Default MSFMulti-source feedback
| Withdrawn
| 0
| —
| 2026-10-28
[aria-label: View campaign: the campaign for Pieter du Plessis (Default MSF, closing 2026-10-28)] View campaign
| Lerato Molefe
| Default MSFMulti-source feedback
| Released
| 9
| 8
| 2026-10-28
[aria-label: View campaign: the campaign for Lerato Molefe (Default MSF, closing 2026-10-28)] View campaign [aria-label: View report: the campaign for Lerato Molefe (Default MSF, closing 2026-10-28)] View report

######## BOARD R3-L6-under-review-1280.dc.html — MSF campaigns · under review (3.44) · 1280
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
## MSF campaigns
Create and review multi-source feedback campaigns.
New campaign MSF coverage
### Campaigns (2)
MSF campaigns, newest first. Counted by respondent group: this list never shows who was invited or who responded.
| Trainee
| Template
| State
| Invitations
| Responses
| Closes
| Actions
| Pieter du Plessis
| Default MSFMulti-source feedback
| Withdrawn
| 0
| —
| 2026-10-28
[aria-label: View campaign: the campaign for Pieter du Plessis (Default MSF, closing 2026-10-28)] View campaign
| Lerato Molefe
| Default MSFMulti-source feedback
| Under review
| 9
| 8
| 2026-10-28
[aria-label: View campaign: the campaign for Lerato Molefe (Default MSF, closing 2026-10-28)] View campaign [aria-label: Review and release: the campaign for Lerato Molefe (Default MSF, closing 2026-10-28)] Review and release [aria-label: Withdraw: the campaign for Lerato Molefe (Default MSF, closing 2026-10-28)] Withdraw…

######## BOARD R3-L6-under-review-390.dc.html — MSF campaigns · under review (3.44) · 390
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
## MSF campaigns
Create and review multi-source feedback campaigns.
New campaign MSF coverage
### Campaigns (2)
MSF campaigns, newest first. Counted by respondent group: this list never shows who was invited or who responded.
| Trainee
| Template
| State
| Invitations
| Responses
| Closes
| Actions
| Pieter du Plessis
| Default MSFMulti-source feedback
| Withdrawn
| 0
| —
| 2026-10-28
[aria-label: View campaign: the campaign for Pieter du Plessis (Default MSF, closing 2026-10-28)] View campaign
| Lerato Molefe
| Default MSFMulti-source feedback
| Under review
| 9
| 8
| 2026-10-28
[aria-label: View campaign: the campaign for Lerato Molefe (Default MSF, closing 2026-10-28)] View campaign [aria-label: Review and release: the campaign for Lerato Molefe (Default MSF, closing 2026-10-28)] Review and release [aria-label: Withdraw: the campaign for Lerato Molefe (Default MSF, closing 2026-10-28)] Withdraw…

######## BOARD R3-L7-ten-1280.dc.html — MSF campaigns · ten campaigns, two semesters (R10, Illustration) · 1280
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
Illustration KGK's five trainees in two semesters, newest first by creation. Counts and dates the replay does not reach read ‹n› and ‹date›.
## MSF campaigns
Create and review multi-source feedback campaigns.
New campaign MSF coverage
### Campaigns (10)
MSF campaigns, newest first. Counted by respondent group: this list never shows who was invited or who responded.
| Trainee
| Template
| State
| Invitations
| Responses
| Closes
| Actions
| Sipho Ndlovu
| Default MSFMulti-source feedback
| Under review
| ‹n›
| ‹n›
| ‹date›
[aria-label: View campaign: the campaign for Sipho Ndlovu (Default MSF, closing ‹date›)] View campaign [aria-label: Review and release: the campaign for Sipho Ndlovu (Default MSF, closing ‹date›)] Review and release [aria-label: Withdraw: the campaign for Sipho Ndlovu (Default MSF, closing ‹date›)] Withdraw…
| Nomsa Mahlangu
| Default MSFMulti-source feedback
| Draft
| ‹n›
| —
| ‹date›
[aria-label: Manage: the campaign for Nomsa Mahlangu (Default MSF, closing ‹date›)] Manage [aria-label: Withdraw: the campaign for Nomsa Mahlangu (Default MSF, closing ‹date›)] Withdraw…
| Anele Dlamini
| Default MSFMulti-source feedback
| Open
| ‹n›
| ‹n›
| ‹date›
[aria-label: Manage: the campaign for Anele Dlamini (Default MSF, closing ‹date›)] Manage [aria-label: View report: the campaign for Anele Dlamini (Default MSF, closing ‹date›)] View report [aria-label: Withdraw: the campaign for Anele Dlamini (Default MSF, closing ‹date›)] Withdraw…
| Pieter du Plessis
| Default MSFMulti-source feedback
| Withdrawn
| 0
| —
| 2026-10-28
[aria-label: View campaign: the campaign for Pieter du Plessis (Default MSF, closing 2026-10-28)] View campaign
| Lerato Molefe
| Default MSFMulti-source feedback
| Released
| 9
| 8
| 2026-10-28
[aria-label: View campaign: the campaign for Lerato Molefe (Default MSF, closing 2026-10-28)] View campaign [aria-label: View report: the campaign for Lerato Molefe (Default MSF, closing 2026-10-28)] View report
| Sipho Ndlovu
| Default MSFMulti-source feedback
| Released
| ‹n›
| ‹n›
| ‹S1 date›
[aria-label: View campaign: the campaign for Sipho Ndlovu (Default MSF, closing ‹S1 date›)] View campaign [aria-label: View report: the campaign for Sipho Ndlovu (Default MSF, closing ‹S1 date›)] View report
| Nomsa Mahlangu
| Default MSFMulti-source feedback
| Released
| ‹n›
| ‹n›
| ‹S1 date›
[aria-label: View campaign: the campaign for Nomsa Mahlangu (Default MSF, closing ‹S1 date›)] View campaign [aria-label: View report: the campaign for Nomsa Mahlangu (Default MSF, closing ‹S1 date›)] View report
| Anele Dlamini
| Default MSFMulti-source feedback
| Released
| ‹n›
| ‹n›
| ‹S1 date›
[aria-label: View campaign: the campaign for Anele Dlamini (Default MSF, closing ‹S1 date›)] View campaign [aria-label: View report: the campaign for Anele Dlamini (Default MSF, closing ‹S1 date›)] View report
| Pieter du Plessis
| Default MSFMulti-source feedback
| Released
| ‹n›
| ‹n›
| ‹S1 date›
[aria-label: View campaign: the campaign for Pieter du Plessis (Default MSF, closing ‹S1 date›)] View campaign [aria-label: View report: the campaign for Pieter du Plessis (Default MSF, closing ‹S1 date›)] View report
| Lerato Molefe
| Default MSFMulti-source feedback
| Released
| ‹n›
| ‹n›
| ‹S1 date›
[aria-label: View campaign: the campaign for Lerato Molefe (Default MSF, closing ‹S1 date›)] View campaign [aria-label: View report: the campaign for Lerato Molefe (Default MSF, closing ‹S1 date›)] View report

######## BOARD R3-L7-ten-390.dc.html — MSF campaigns · ten campaigns, two semesters (R10, Illustration) · 390
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
Illustration KGK's five trainees in two semesters, newest first by creation. Counts and dates the replay does not reach read ‹n› and ‹date›.
## MSF campaigns
Create and review multi-source feedback campaigns.
New campaign MSF coverage
### Campaigns (10)
MSF campaigns, newest first. Counted by respondent group: this list never shows who was invited or who responded.
| Trainee
| Template
| State
| Invitations
| Responses
| Closes
| Actions
| Sipho Ndlovu
| Default MSFMulti-source feedback
| Under review
| ‹n›
| ‹n›
| ‹date›
[aria-label: View campaign: the campaign for Sipho Ndlovu (Default MSF, closing ‹date›)] View campaign [aria-label: Review and release: the campaign for Sipho Ndlovu (Default MSF, closing ‹date›)] Review and release [aria-label: Withdraw: the campaign for Sipho Ndlovu (Default MSF, closing ‹date›)] Withdraw…
| Nomsa Mahlangu
| Default MSFMulti-source feedback
| Draft
| ‹n›
| —
| ‹date›
[aria-label: Manage: the campaign for Nomsa Mahlangu (Default MSF, closing ‹date›)] Manage [aria-label: Withdraw: the campaign for Nomsa Mahlangu (Default MSF, closing ‹date›)] Withdraw…
| Anele Dlamini
| Default MSFMulti-source feedback
| Open
| ‹n›
| ‹n›
| ‹date›
[aria-label: Manage: the campaign for Anele Dlamini (Default MSF, closing ‹date›)] Manage [aria-label: View report: the campaign for Anele Dlamini (Default MSF, closing ‹date›)] View report [aria-label: Withdraw: the campaign for Anele Dlamini (Default MSF, closing ‹date›)] Withdraw…
| Pieter du Plessis
| Default MSFMulti-source feedback
| Withdrawn
| 0
| —
| 2026-10-28
[aria-label: View campaign: the campaign for Pieter du Plessis (Default MSF, closing 2026-10-28)] View campaign
| Lerato Molefe
| Default MSFMulti-source feedback
| Released
| 9
| 8
| 2026-10-28
[aria-label: View campaign: the campaign for Lerato Molefe (Default MSF, closing 2026-10-28)] View campaign [aria-label: View report: the campaign for Lerato Molefe (Default MSF, closing 2026-10-28)] View report
| Sipho Ndlovu
| Default MSFMulti-source feedback
| Released
| ‹n›
| ‹n›
| ‹S1 date›
[aria-label: View campaign: the campaign for Sipho Ndlovu (Default MSF, closing ‹S1 date›)] View campaign [aria-label: View report: the campaign for Sipho Ndlovu (Default MSF, closing ‹S1 date›)] View report
| Nomsa Mahlangu
| Default MSFMulti-source feedback
| Released
| ‹n›
| ‹n›
| ‹S1 date›
[aria-label: View campaign: the campaign for Nomsa Mahlangu (Default MSF, closing ‹S1 date›)] View campaign [aria-label: View report: the campaign for Nomsa Mahlangu (Default MSF, closing ‹S1 date›)] View report
| Anele Dlamini
| Default MSFMulti-source feedback
| Released
| ‹n›
| ‹n›
| ‹S1 date›
[aria-label: View campaign: the campaign for Anele Dlamini (Default MSF, closing ‹S1 date›)] View campaign [aria-label: View report: the campaign for Anele Dlamini (Default MSF, closing ‹S1 date›)] View report
| Pieter du Plessis
| Default MSFMulti-source feedback
| Released
| ‹n›
| ‹n›
| ‹S1 date›
[aria-label: View campaign: the campaign for Pieter du Plessis (Default MSF, closing ‹S1 date›)] View campaign [aria-label: View report: the campaign for Pieter du Plessis (Default MSF, closing ‹S1 date›)] View report
| Lerato Molefe
| Default MSFMulti-source feedback
| Released
| ‹n›
| ‹n›
| ‹S1 date›
[aria-label: View campaign: the campaign for Lerato Molefe (Default MSF, closing ‹S1 date›)] View campaign [aria-label: View report: the campaign for Lerato Molefe (Default MSF, closing ‹S1 date›)] View report

######## BOARD R3-L8-loading-1280.dc.html — MSF campaigns · loading · 1280
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
## MSF campaigns
Create and review multi-source feedback campaigns.
New campaign MSF coverage
### Campaigns
Loading MSF campaigns.

######## BOARD R3-L8-loading-390.dc.html — MSF campaigns · loading · 390
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
## MSF campaigns
Create and review multi-source feedback campaigns.
New campaign MSF coverage
### Campaigns
Loading MSF campaigns.

######## BOARD R3-L9-error-1280.dc.html — MSF campaigns · load error · 1280
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
## MSF campaigns
Create and review multi-source feedback campaigns.
New campaign MSF coverage
Could not load MSF campaigns. Nothing has changed. Try again, or come back in a few minutes.Try again

######## BOARD R3-L9-error-390.dc.html — MSF campaigns · load error · 390
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
## MSF campaigns
Create and review multi-source feedback campaigns.
New campaign MSF coverage
Could not load MSF campaigns. Nothing has changed. Try again, or come back in a few minutes.Try again

######## BOARD R3-C1-no-template-1280.dc.html — New MSF campaign · no template yet: the fold open, Create disabled (3.34) · 1280
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
New MSF campaign
## New MSF campaign
One campaign for one current trainee: its questionnaire, its window, its thresholds and the EPAs it is evidence for. Creating it makes a draft; nobody is emailed until you open it.
* marks a field you must fill in.
### QuestionnaireWritten once, used by every campaign that names it.
Template *
[aria-describedby: Write the first one below; it is chosen here once it is added.] [disabled]
No template yet
Write the first one below; it is chosen here once it is added.
Write a new questionnaire
Kind
[checked] Multi-source feedback
Learner feedback
Template name * [input: Default MSF]
Scale question * [aria-describedby: Answered on five points, from Well below expectations to Well above expectations.] [input: Rates the trainee's overall professional performance.]
Answered on five points, from Well below expectations to Well above expectations.
Comment question * [input: What should the trainee keep doing or improve?]
Add template
### Trainee and window
Trainee *
[aria-describedby: Current trainees you can run a campaign for. NEW]
Choose a trainee
Current trainees you can run a campaign for. NEW
Opens on * [aria-describedby: Today, on the South African calendar. NEW DATA: the South African date] [input: 2026-10-14]
Today, on the South African calendar. NEW DATA: the South African date
Closes on * [aria-describedby: The respondents' last day to answer, on the South African calendar.] [input: 2026-10-28]
The respondents' last day to answer, on the South African calendar.
### ThresholdsNothing is shown of a respondent group that falls short, so no answer can be traced to its author.
Minimum responses * [aria-describedby: Responses needed in all before the report can be released.] [input: 8]
Responses needed in all before the report can be released.
Minimum responses in a respondent group * [aria-describedby: A respondent group with fewer is suppressed: its rating and comments are never shown. At least 3 for multi-source feedback.] [input: 3]
A respondent group with fewer is suppressed: its rating and comments are never shown. At least 3 for multi-source feedback.
Minimum respondent groups reporting * [aria-describedby: How many respondent groups must clear the threshold beside it before the report can be released. Two is the College's answer: eight peer doctors alone are not multi-source feedback.] [input: 2]
How many respondent groups must clear the threshold beside it before the report can be released. Two is the College's answer: eight peer doctors alone are not multi-source feedback.
### Evidence
[aria-describedby: Select a trainee first.]
Evidence for these EPAs * required
Select a trainee first.
Cancel [aria-describedby: Create campaign: write a questionnaire first. NEW] [aria-disabled] Create campaign
Create campaign: write a questionnaire first. NEW

######## BOARD R3-C1-no-template-390.dc.html — New MSF campaign · no template yet: the fold open, Create disabled (3.34) · 390
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
New MSF campaign
## New MSF campaign
One campaign for one current trainee: its questionnaire, its window, its thresholds and the EPAs it is evidence for. Creating it makes a draft; nobody is emailed until you open it.
* marks a field you must fill in.
### QuestionnaireWritten once, used by every campaign that names it.
Template *
[aria-describedby: Write the first one below; it is chosen here once it is added.] [disabled]
No template yet
Write the first one below; it is chosen here once it is added.
Write a new questionnaire
Kind
[checked] Multi-source feedback
Learner feedback
Template name * [input: Default MSF]
Scale question * [aria-describedby: Answered on five points, from Well below expectations to Well above expectations.] [input: Rates the trainee's overall professional performance.]
Answered on five points, from Well below expectations to Well above expectations.
Comment question * [input: What should the trainee keep doing or improve?]
Add template
### Trainee and window
Trainee *
[aria-describedby: Current trainees you can run a campaign for. NEW]
Choose a trainee
Current trainees you can run a campaign for. NEW
Opens on * [aria-describedby: Today, on the South African calendar. NEW DATA: the South African date] [input: 2026-10-14]
Today, on the South African calendar. NEW DATA: the South African date
Closes on * [aria-describedby: The respondents' last day to answer, on the South African calendar.] [input: 2026-10-28]
The respondents' last day to answer, on the South African calendar.
### ThresholdsNothing is shown of a respondent group that falls short, so no answer can be traced to its author.
Minimum responses * [aria-describedby: Responses needed in all before the report can be released.] [input: 8]
Responses needed in all before the report can be released.
Minimum responses in a respondent group * [aria-describedby: A respondent group with fewer is suppressed: its rating and comments are never shown. At least 3 for multi-source feedback.] [input: 3]
A respondent group with fewer is suppressed: its rating and comments are never shown. At least 3 for multi-source feedback.
Minimum respondent groups reporting * [aria-describedby: How many respondent groups must clear the threshold beside it before the report can be released. Two is the College's answer: eight peer doctors alone are not multi-source feedback.] [input: 2]
How many respondent groups must clear the threshold beside it before the report can be released. Two is the College's answer: eight peer doctors alone are not multi-source feedback.
### Evidence
[aria-describedby: Select a trainee first.]
Evidence for these EPAs * required
Select a trainee first.
Cancel [aria-describedby: Create campaign: write a questionnaire first. NEW] [aria-disabled] Create campaign
Create campaign: write a questionnaire first. NEW

######## BOARD R3-C2-template-created-1280.dc.html — New MSF campaign · Template created, Default MSF chosen (3.34) · 1280
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
New MSF campaign
## New MSF campaign
One campaign for one current trainee: its questionnaire, its window, its thresholds and the EPAs it is evidence for. Creating it makes a draft; nobody is emailed until you open it.
* marks a field you must fill in.
### QuestionnaireWritten once, used by every campaign that names it.
Template created. Default MSF is chosen as this campaign's template. NEW
Template *
[aria-describedby: Rates the trainee's overall professional performance. · What should the trainee keep doing or improve?]
Default MSF (Multi-source feedback)
Rates the trainee's overall professional performance. · What should the trainee keep doing or improve?
Write a new questionnaire
Kind
[checked] Multi-source feedback
Learner feedback
Template name * [input: Default MSF]
Scale question * [aria-describedby: Answered on five points, from Well below expectations to Well above expectations.] [input: Rates the trainee's overall professional performance.]
Answered on five points, from Well below expectations to Well above expectations.
Comment question * [input: What should the trainee keep doing or improve?]
Add template
### Trainee and window
Trainee *
[aria-describedby: Current trainees you can run a campaign for. NEW]
Choose a trainee
Current trainees you can run a campaign for. NEW
Opens on * [aria-describedby: Today, on the South African calendar. NEW DATA: the South African date] [input: 2026-10-14]
Today, on the South African calendar. NEW DATA: the South African date
Closes on * [aria-describedby: The respondents' last day to answer, on the South African calendar.] [input: 2026-10-28]
The respondents' last day to answer, on the South African calendar.
### ThresholdsNothing is shown of a respondent group that falls short, so no answer can be traced to its author.
Minimum responses * [aria-describedby: Responses needed in all before the report can be released.] [input: 8]
Responses needed in all before the report can be released.
Minimum responses in a respondent group * [aria-describedby: A respondent group with fewer is suppressed: its rating and comments are never shown. At least 3 for multi-source feedback.] [input: 3]
A respondent group with fewer is suppressed: its rating and comments are never shown. At least 3 for multi-source feedback.
Minimum respondent groups reporting * [aria-describedby: How many respondent groups must clear the threshold beside it before the report can be released. Two is the College's answer: eight peer doctors alone are not multi-source feedback.] [input: 2]
How many respondent groups must clear the threshold beside it before the report can be released. Two is the College's answer: eight peer doctors alone are not multi-source feedback.
### Evidence
[aria-describedby: Select a trainee first.]
Evidence for these EPAs * required
Select a trainee first.
Cancel Create campaign

######## BOARD R3-C2-template-created-390.dc.html — New MSF campaign · Template created, Default MSF chosen (3.34) · 390
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
New MSF campaign
## New MSF campaign
One campaign for one current trainee: its questionnaire, its window, its thresholds and the EPAs it is evidence for. Creating it makes a draft; nobody is emailed until you open it.
* marks a field you must fill in.
### QuestionnaireWritten once, used by every campaign that names it.
Template created. Default MSF is chosen as this campaign's template. NEW
Template *
[aria-describedby: Rates the trainee's overall professional performance. · What should the trainee keep doing or improve?]
Default MSF (Multi-source feedback)
Rates the trainee's overall professional performance. · What should the trainee keep doing or improve?
Write a new questionnaire
Kind
[checked] Multi-source feedback
Learner feedback
Template name * [input: Default MSF]
Scale question * [aria-describedby: Answered on five points, from Well below expectations to Well above expectations.] [input: Rates the trainee's overall professional performance.]
Answered on five points, from Well below expectations to Well above expectations.
Comment question * [input: What should the trainee keep doing or improve?]
Add template
### Trainee and window
Trainee *
[aria-describedby: Current trainees you can run a campaign for. NEW]
Choose a trainee
Current trainees you can run a campaign for. NEW
Opens on * [aria-describedby: Today, on the South African calendar. NEW DATA: the South African date] [input: 2026-10-14]
Today, on the South African calendar. NEW DATA: the South African date
Closes on * [aria-describedby: The respondents' last day to answer, on the South African calendar.] [input: 2026-10-28]
The respondents' last day to answer, on the South African calendar.
### ThresholdsNothing is shown of a respondent group that falls short, so no answer can be traced to its author.
Minimum responses * [aria-describedby: Responses needed in all before the report can be released.] [input: 8]
Responses needed in all before the report can be released.
Minimum responses in a respondent group * [aria-describedby: A respondent group with fewer is suppressed: its rating and comments are never shown. At least 3 for multi-source feedback.] [input: 3]
A respondent group with fewer is suppressed: its rating and comments are never shown. At least 3 for multi-source feedback.
Minimum respondent groups reporting * [aria-describedby: How many respondent groups must clear the threshold beside it before the report can be released. Two is the College's answer: eight peer doctors alone are not multi-source feedback.] [input: 2]
How many respondent groups must clear the threshold beside it before the report can be released. Two is the College's answer: eight peer doctors alone are not multi-source feedback.
### Evidence
[aria-describedby: Select a trainee first.]
Evidence for these EPAs * required
Select a trainee first.
Cancel Create campaign

######## BOARD R3-C3-molefe-1280.dc.html — New MSF campaign · Lerato Molefe, 5 / 3 / 2, PAED-010 and PAED-012 ticked (3.35) · 1280
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
Mr Smit typed 5; 3 and 2 are the form's defaults (8 / 3 / 2). F2: floor 3
[aria-label: Breadcrumb]
Home
MSF campaigns
New MSF campaign
## New MSF campaign
One campaign for one current trainee: its questionnaire, its window, its thresholds and the EPAs it is evidence for. Creating it makes a draft; nobody is emailed until you open it.
* marks a field you must fill in.
### QuestionnaireWritten once, used by every campaign that names it.
Template *
[aria-describedby: Rates the trainee's overall professional performance. · What should the trainee keep doing or improve?]
Default MSF (Multi-source feedback)
Rates the trainee's overall professional performance. · What should the trainee keep doing or improve?
Write a new questionnaire
Kind
[checked] Multi-source feedback
Learner feedback
Template name * [input: Default MSF]
Scale question * [aria-describedby: Answered on five points, from Well below expectations to Well above expectations.] [input: Rates the trainee's overall professional performance.]
Answered on five points, from Well below expectations to Well above expectations.
Comment question * [input: What should the trainee keep doing or improve?]
Add template
### Trainee and window
Trainee *
[aria-describedby: Current trainees you can run a campaign for. NEW]
Lerato Molefe (molefe@kgk.wombat.local)
Current trainees you can run a campaign for. NEW
Opens on * [aria-describedby: Today, on the South African calendar. NEW DATA: the South African date] [input: 2026-10-14]
Today, on the South African calendar. NEW DATA: the South African date
Closes on * [aria-describedby: The respondents' last day to answer, on the South African calendar.] [input: 2026-10-28]
The respondents' last day to answer, on the South African calendar.
### ThresholdsNothing is shown of a respondent group that falls short, so no answer can be traced to its author.
Minimum responses * [aria-describedby: Responses needed in all before the report can be released.] [input: 5]
Responses needed in all before the report can be released.
Minimum responses in a respondent group * [aria-describedby: A respondent group with fewer is suppressed: its rating and comments are never shown. At least 3 for multi-source feedback.] [input: 3]
A respondent group with fewer is suppressed: its rating and comments are never shown. At least 3 for multi-source feedback.
Minimum respondent groups reporting * [aria-describedby: How many respondent groups must clear the threshold beside it before the report can be released. Two is the College's answer: eight peer doctors alone are not multi-source feedback.] [input: 2]
How many respondent groups must clear the threshold beside it before the report can be released. Two is the College's answer: eight peer doctors alone are not multi-source feedback.
### Evidence
[aria-describedby: One campaign covers many EPAs and runs once a semester. Each one selected here becomes a separate evidence record on the trainee's portfolio when the report is released.]
Evidence for these EPAs * required
One campaign covers many EPAs and runs once a semester. Each one selected here becomes a separate evidence record on the trainee's portfolio when the report is released.
2 of 15 ticked
PAED-001 — Providing paediatric emergency care to children
PAED-002 — Managing common paediatric presentations
PAED-003 — Providing intensive care to children
PAED-004 — Managing common neonatal conditions
PAED-005 — Providing neonatal care in intensive and high-care settings
PAED-006 — Managing long-term health conditions (LTHCs)
PAED-007 — Maintaining and promoting the health and well-being of children
PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
PAED-009 — Providing care for an adolescent patient
[checked] PAED-010 — Leading and operating within a clinical team
PAED-011 — Managing population health challenges
[checked] PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
PAED-013 — Managing child protection, legal and ethical aspects of children at risk
PAED-014 — Providing palliative and end-of-life care
PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
Cancel Create campaign

######## BOARD R3-C3-molefe-768.dc.html — New MSF campaign · Lerato Molefe, 5 / 3 / 2, PAED-010 and PAED-012 ticked (3.35) · 768
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
Mr Smit typed 5; 3 and 2 are the form's defaults (8 / 3 / 2). F2: floor 3
[aria-label: Breadcrumb]
Home
MSF campaigns
New MSF campaign
## New MSF campaign
One campaign for one current trainee: its questionnaire, its window, its thresholds and the EPAs it is evidence for. Creating it makes a draft; nobody is emailed until you open it.
* marks a field you must fill in.
### QuestionnaireWritten once, used by every campaign that names it.
Template *
[aria-describedby: Rates the trainee's overall professional performance. · What should the trainee keep doing or improve?]
Default MSF (Multi-source feedback)
Rates the trainee's overall professional performance. · What should the trainee keep doing or improve?
Write a new questionnaire
Kind
[checked] Multi-source feedback
Learner feedback
Template name * [input: Default MSF]
Scale question * [aria-describedby: Answered on five points, from Well below expectations to Well above expectations.] [input: Rates the trainee's overall professional performance.]
Answered on five points, from Well below expectations to Well above expectations.
Comment question * [input: What should the trainee keep doing or improve?]
Add template
### Trainee and window
Trainee *
[aria-describedby: Current trainees you can run a campaign for. NEW]
Lerato Molefe (molefe@kgk.wombat.local)
Current trainees you can run a campaign for. NEW
Opens on * [aria-describedby: Today, on the South African calendar. NEW DATA: the South African date] [input: 2026-10-14]
Today, on the South African calendar. NEW DATA: the South African date
Closes on * [aria-describedby: The respondents' last day to answer, on the South African calendar.] [input: 2026-10-28]
The respondents' last day to answer, on the South African calendar.
### ThresholdsNothing is shown of a respondent group that falls short, so no answer can be traced to its author.
Minimum responses * [aria-describedby: Responses needed in all before the report can be released.] [input: 5]
Responses needed in all before the report can be released.
Minimum responses in a respondent group * [aria-describedby: A respondent group with fewer is suppressed: its rating and comments are never shown. At least 3 for multi-source feedback.] [input: 3]
A respondent group with fewer is suppressed: its rating and comments are never shown. At least 3 for multi-source feedback.
Minimum respondent groups reporting * [aria-describedby: How many respondent groups must clear the threshold beside it before the report can be released. Two is the College's answer: eight peer doctors alone are not multi-source feedback.] [input: 2]
How many respondent groups must clear the threshold beside it before the report can be released. Two is the College's answer: eight peer doctors alone are not multi-source feedback.
### Evidence
[aria-describedby: One campaign covers many EPAs and runs once a semester. Each one selected here becomes a separate evidence record on the trainee's portfolio when the report is released.]
Evidence for these EPAs * required
One campaign covers many EPAs and runs once a semester. Each one selected here becomes a separate evidence record on the trainee's portfolio when the report is released.
2 of 15 ticked
PAED-001 — Providing paediatric emergency care to children
PAED-002 — Managing common paediatric presentations
PAED-003 — Providing intensive care to children
PAED-004 — Managing common neonatal conditions
PAED-005 — Providing neonatal care in intensive and high-care settings
PAED-006 — Managing long-term health conditions (LTHCs)
PAED-007 — Maintaining and promoting the health and well-being of children
PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
PAED-009 — Providing care for an adolescent patient
[checked] PAED-010 — Leading and operating within a clinical team
PAED-011 — Managing population health challenges
[checked] PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
PAED-013 — Managing child protection, legal and ethical aspects of children at risk
PAED-014 — Providing palliative and end-of-life care
PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
Cancel Create campaign

######## BOARD R3-C3-molefe-390.dc.html — New MSF campaign · Lerato Molefe, 5 / 3 / 2, PAED-010 and PAED-012 ticked (3.35) · 390
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
Mr Smit typed 5; 3 and 2 are the form's defaults (8 / 3 / 2). F2: floor 3
[aria-label: Breadcrumb]
Home
MSF campaigns
New MSF campaign
## New MSF campaign
One campaign for one current trainee: its questionnaire, its window, its thresholds and the EPAs it is evidence for. Creating it makes a draft; nobody is emailed until you open it.
* marks a field you must fill in.
### QuestionnaireWritten once, used by every campaign that names it.
Template *
[aria-describedby: Rates the trainee's overall professional performance. · What should the trainee keep doing or improve?]
Default MSF (Multi-source feedback)
Rates the trainee's overall professional performance. · What should the trainee keep doing or improve?
Write a new questionnaire
Kind
[checked] Multi-source feedback
Learner feedback
Template name * [input: Default MSF]
Scale question * [aria-describedby: Answered on five points, from Well below expectations to Well above expectations.] [input: Rates the trainee's overall professional performance.]
Answered on five points, from Well below expectations to Well above expectations.
Comment question * [input: What should the trainee keep doing or improve?]
Add template
### Trainee and window
Trainee *
[aria-describedby: Current trainees you can run a campaign for. NEW]
Lerato Molefe (molefe@kgk.wombat.local)
Current trainees you can run a campaign for. NEW
Opens on * [aria-describedby: Today, on the South African calendar. NEW DATA: the South African date] [input: 2026-10-14]
Today, on the South African calendar. NEW DATA: the South African date
Closes on * [aria-describedby: The respondents' last day to answer, on the South African calendar.] [input: 2026-10-28]
The respondents' last day to answer, on the South African calendar.
### ThresholdsNothing is shown of a respondent group that falls short, so no answer can be traced to its author.
Minimum responses * [aria-describedby: Responses needed in all before the report can be released.] [input: 5]
Responses needed in all before the report can be released.
Minimum responses in a respondent group * [aria-describedby: A respondent group with fewer is suppressed: its rating and comments are never shown. At least 3 for multi-source feedback.] [input: 3]
A respondent group with fewer is suppressed: its rating and comments are never shown. At least 3 for multi-source feedback.
Minimum respondent groups reporting * [aria-describedby: How many respondent groups must clear the threshold beside it before the report can be released. Two is the College's answer: eight peer doctors alone are not multi-source feedback.] [input: 2]
How many respondent groups must clear the threshold beside it before the report can be released. Two is the College's answer: eight peer doctors alone are not multi-source feedback.
### Evidence
[aria-describedby: One campaign covers many EPAs and runs once a semester. Each one selected here becomes a separate evidence record on the trainee's portfolio when the report is released.]
Evidence for these EPAs * required
One campaign covers many EPAs and runs once a semester. Each one selected here becomes a separate evidence record on the trainee's portfolio when the report is released.
2 of 15 ticked
PAED-001 — Providing paediatric emergency care to children
PAED-002 — Managing common paediatric presentations
PAED-003 — Providing intensive care to children
PAED-004 — Managing common neonatal conditions
PAED-005 — Providing neonatal care in intensive and high-care settings
PAED-006 — Managing long-term health conditions (LTHCs)
PAED-007 — Maintaining and promoting the health and well-being of children
PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
PAED-009 — Providing care for an adolescent patient
[checked] PAED-010 — Leading and operating within a clinical team
PAED-011 — Managing population health challenges
[checked] PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
PAED-013 — Managing child protection, legal and ethical aspects of children at risk
PAED-014 — Providing palliative and end-of-life care
PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
Cancel Create campaign

######## BOARD R3-C4-refused-1280.dc.html — New MSF campaign · no EPA ticked: the refusal summary takes the focus (M9) · 1280
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
The summary's link moves the focus to the first EPA checkbox by script (PageFocus), never to the legend; the fieldset's message stays under its legend.
[aria-label: Breadcrumb]
Home
MSF campaigns
New MSF campaign
## New MSF campaign
One campaign for one current trainee: its questionnaire, its window, its thresholds and the EPAs it is evidence for. Creating it makes a draft; nobody is emailed until you open it.
Not created. Fix the field below and create the campaign again.
Select at least one EPA this campaign is evidence for.
* marks a field you must fill in.
### QuestionnaireWritten once, used by every campaign that names it.
Template *
[aria-describedby: Rates the trainee's overall professional performance. · What should the trainee keep doing or improve?]
Default MSF (Multi-source feedback)
Rates the trainee's overall professional performance. · What should the trainee keep doing or improve?
Write a new questionnaire
Kind
[checked] Multi-source feedback
Learner feedback
Template name * [input: Default MSF]
Scale question * [aria-describedby: Answered on five points, from Well below expectations to Well above expectations.] [input: Rates the trainee's overall professional performance.]
Answered on five points, from Well below expectations to Well above expectations.
Comment question * [input: What should the trainee keep doing or improve?]
Add template
### Trainee and window
Trainee *
[aria-describedby: Current trainees you can run a campaign for. NEW]
Lerato Molefe (molefe@kgk.wombat.local)
Current trainees you can run a campaign for. NEW
Opens on * [aria-describedby: Today, on the South African calendar. NEW DATA: the South African date] [input: 2026-10-14]
Today, on the South African calendar. NEW DATA: the South African date
Closes on * [aria-describedby: The respondents' last day to answer, on the South African calendar.] [input: 2026-10-28]
The respondents' last day to answer, on the South African calendar.
### ThresholdsNothing is shown of a respondent group that falls short, so no answer can be traced to its author.
Minimum responses * [aria-describedby: Responses needed in all before the report can be released.] [input: 5]
Responses needed in all before the report can be released.
Minimum responses in a respondent group * [aria-describedby: A respondent group with fewer is suppressed: its rating and comments are never shown. At least 3 for multi-source feedback.] [input: 3]
A respondent group with fewer is suppressed: its rating and comments are never shown. At least 3 for multi-source feedback.
Minimum respondent groups reporting * [aria-describedby: How many respondent groups must clear the threshold beside it before the report can be released. Two is the College's answer: eight peer doctors alone are not multi-source feedback.] [input: 2]
How many respondent groups must clear the threshold beside it before the report can be released. Two is the College's answer: eight peer doctors alone are not multi-source feedback.
### Evidence
[aria-describedby: One campaign covers many EPAs and runs once a semester. Each one selected here becomes a separate evidence record on the trainee's portfolio when the report is released. / Select at least one EPA this campaign is evidence for.]
Evidence for these EPAs * required
One campaign covers many EPAs and runs once a semester. Each one selected here becomes a separate evidence record on the trainee's portfolio when the report is released.
Select at least one EPA this campaign is evidence for.
0 of 15 ticked
PAED-001 — Providing paediatric emergency care to children
PAED-002 — Managing common paediatric presentations
PAED-003 — Providing intensive care to children
PAED-004 — Managing common neonatal conditions
PAED-005 — Providing neonatal care in intensive and high-care settings
PAED-006 — Managing long-term health conditions (LTHCs)
PAED-007 — Maintaining and promoting the health and well-being of children
PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
PAED-009 — Providing care for an adolescent patient
PAED-010 — Leading and operating within a clinical team
PAED-011 — Managing population health challenges
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
PAED-013 — Managing child protection, legal and ethical aspects of children at risk
PAED-014 — Providing palliative and end-of-life care
PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
Cancel Create campaign

######## BOARD R3-C4-refused-390.dc.html — New MSF campaign · no EPA ticked: the refusal summary takes the focus (M9) · 390
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
The summary's link moves the focus to the first EPA checkbox by script (PageFocus), never to the legend; the fieldset's message stays under its legend.
[aria-label: Breadcrumb]
Home
MSF campaigns
New MSF campaign
## New MSF campaign
One campaign for one current trainee: its questionnaire, its window, its thresholds and the EPAs it is evidence for. Creating it makes a draft; nobody is emailed until you open it.
Not created. Fix the field below and create the campaign again.
Select at least one EPA this campaign is evidence for.
* marks a field you must fill in.
### QuestionnaireWritten once, used by every campaign that names it.
Template *
[aria-describedby: Rates the trainee's overall professional performance. · What should the trainee keep doing or improve?]
Default MSF (Multi-source feedback)
Rates the trainee's overall professional performance. · What should the trainee keep doing or improve?
Write a new questionnaire
Kind
[checked] Multi-source feedback
Learner feedback
Template name * [input: Default MSF]
Scale question * [aria-describedby: Answered on five points, from Well below expectations to Well above expectations.] [input: Rates the trainee's overall professional performance.]
Answered on five points, from Well below expectations to Well above expectations.
Comment question * [input: What should the trainee keep doing or improve?]
Add template
### Trainee and window
Trainee *
[aria-describedby: Current trainees you can run a campaign for. NEW]
Lerato Molefe (molefe@kgk.wombat.local)
Current trainees you can run a campaign for. NEW
Opens on * [aria-describedby: Today, on the South African calendar. NEW DATA: the South African date] [input: 2026-10-14]
Today, on the South African calendar. NEW DATA: the South African date
Closes on * [aria-describedby: The respondents' last day to answer, on the South African calendar.] [input: 2026-10-28]
The respondents' last day to answer, on the South African calendar.
### ThresholdsNothing is shown of a respondent group that falls short, so no answer can be traced to its author.
Minimum responses * [aria-describedby: Responses needed in all before the report can be released.] [input: 5]
Responses needed in all before the report can be released.
Minimum responses in a respondent group * [aria-describedby: A respondent group with fewer is suppressed: its rating and comments are never shown. At least 3 for multi-source feedback.] [input: 3]
A respondent group with fewer is suppressed: its rating and comments are never shown. At least 3 for multi-source feedback.
Minimum respondent groups reporting * [aria-describedby: How many respondent groups must clear the threshold beside it before the report can be released. Two is the College's answer: eight peer doctors alone are not multi-source feedback.] [input: 2]
How many respondent groups must clear the threshold beside it before the report can be released. Two is the College's answer: eight peer doctors alone are not multi-source feedback.
### Evidence
[aria-describedby: One campaign covers many EPAs and runs once a semester. Each one selected here becomes a separate evidence record on the trainee's portfolio when the report is released. / Select at least one EPA this campaign is evidence for.]
Evidence for these EPAs * required
One campaign covers many EPAs and runs once a semester. Each one selected here becomes a separate evidence record on the trainee's portfolio when the report is released.
Select at least one EPA this campaign is evidence for.
0 of 15 ticked
PAED-001 — Providing paediatric emergency care to children
PAED-002 — Managing common paediatric presentations
PAED-003 — Providing intensive care to children
PAED-004 — Managing common neonatal conditions
PAED-005 — Providing neonatal care in intensive and high-care settings
PAED-006 — Managing long-term health conditions (LTHCs)
PAED-007 — Maintaining and promoting the health and well-being of children
PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
PAED-009 — Providing care for an adolescent patient
PAED-010 — Leading and operating within a clinical team
PAED-011 — Managing population health challenges
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
PAED-013 — Managing child protection, legal and ethical aspects of children at risk
PAED-014 — Providing palliative and end-of-life care
PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
Cancel Create campaign

######## BOARD R3-C5-fifteen-1280.dc.html — New MSF campaign · all fifteen ticked (R10) · 1280
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
Mr Smit typed 5; 3 and 2 are the form's defaults (8 / 3 / 2). F2: floor 3
[aria-label: Breadcrumb]
Home
MSF campaigns
New MSF campaign
## New MSF campaign
One campaign for one current trainee: its questionnaire, its window, its thresholds and the EPAs it is evidence for. Creating it makes a draft; nobody is emailed until you open it.
* marks a field you must fill in.
### QuestionnaireWritten once, used by every campaign that names it.
Template *
[aria-describedby: Rates the trainee's overall professional performance. · What should the trainee keep doing or improve?]
Default MSF (Multi-source feedback)
Rates the trainee's overall professional performance. · What should the trainee keep doing or improve?
Write a new questionnaire
Kind
[checked] Multi-source feedback
Learner feedback
Template name * [input: Default MSF]
Scale question * [aria-describedby: Answered on five points, from Well below expectations to Well above expectations.] [input: Rates the trainee's overall professional performance.]
Answered on five points, from Well below expectations to Well above expectations.
Comment question * [input: What should the trainee keep doing or improve?]
Add template
### Trainee and window
Trainee *
[aria-describedby: Current trainees you can run a campaign for. NEW]
Lerato Molefe (molefe@kgk.wombat.local)
Current trainees you can run a campaign for. NEW
Opens on * [aria-describedby: Today, on the South African calendar. NEW DATA: the South African date] [input: 2026-10-14]
Today, on the South African calendar. NEW DATA: the South African date
Closes on * [aria-describedby: The respondents' last day to answer, on the South African calendar.] [input: 2026-10-28]
The respondents' last day to answer, on the South African calendar.
### ThresholdsNothing is shown of a respondent group that falls short, so no answer can be traced to its author.
Minimum responses * [aria-describedby: Responses needed in all before the report can be released.] [input: 5]
Responses needed in all before the report can be released.
Minimum responses in a respondent group * [aria-describedby: A respondent group with fewer is suppressed: its rating and comments are never shown. At least 3 for multi-source feedback.] [input: 3]
A respondent group with fewer is suppressed: its rating and comments are never shown. At least 3 for multi-source feedback.
Minimum respondent groups reporting * [aria-describedby: How many respondent groups must clear the threshold beside it before the report can be released. Two is the College's answer: eight peer doctors alone are not multi-source feedback.] [input: 2]
How many respondent groups must clear the threshold beside it before the report can be released. Two is the College's answer: eight peer doctors alone are not multi-source feedback.
### Evidence
[aria-describedby: One campaign covers many EPAs and runs once a semester. Each one selected here becomes a separate evidence record on the trainee's portfolio when the report is released.]
Evidence for these EPAs * required
One campaign covers many EPAs and runs once a semester. Each one selected here becomes a separate evidence record on the trainee's portfolio when the report is released.
15 of 15 ticked
[checked] PAED-001 — Providing paediatric emergency care to children
[checked] PAED-002 — Managing common paediatric presentations
[checked] PAED-003 — Providing intensive care to children
[checked] PAED-004 — Managing common neonatal conditions
[checked] PAED-005 — Providing neonatal care in intensive and high-care settings
[checked] PAED-006 — Managing long-term health conditions (LTHCs)
[checked] PAED-007 — Maintaining and promoting the health and well-being of children
[checked] PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
[checked] PAED-009 — Providing care for an adolescent patient
[checked] PAED-010 — Leading and operating within a clinical team
[checked] PAED-011 — Managing population health challenges
[checked] PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
[checked] PAED-013 — Managing child protection, legal and ethical aspects of children at risk
[checked] PAED-014 — Providing palliative and end-of-life care
[checked] PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
Cancel Create campaign

######## BOARD R3-C5-fifteen-390.dc.html — New MSF campaign · all fifteen ticked (R10) · 390
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
Mr Smit typed 5; 3 and 2 are the form's defaults (8 / 3 / 2). F2: floor 3
[aria-label: Breadcrumb]
Home
MSF campaigns
New MSF campaign
## New MSF campaign
One campaign for one current trainee: its questionnaire, its window, its thresholds and the EPAs it is evidence for. Creating it makes a draft; nobody is emailed until you open it.
* marks a field you must fill in.
### QuestionnaireWritten once, used by every campaign that names it.
Template *
[aria-describedby: Rates the trainee's overall professional performance. · What should the trainee keep doing or improve?]
Default MSF (Multi-source feedback)
Rates the trainee's overall professional performance. · What should the trainee keep doing or improve?
Write a new questionnaire
Kind
[checked] Multi-source feedback
Learner feedback
Template name * [input: Default MSF]
Scale question * [aria-describedby: Answered on five points, from Well below expectations to Well above expectations.] [input: Rates the trainee's overall professional performance.]
Answered on five points, from Well below expectations to Well above expectations.
Comment question * [input: What should the trainee keep doing or improve?]
Add template
### Trainee and window
Trainee *
[aria-describedby: Current trainees you can run a campaign for. NEW]
Lerato Molefe (molefe@kgk.wombat.local)
Current trainees you can run a campaign for. NEW
Opens on * [aria-describedby: Today, on the South African calendar. NEW DATA: the South African date] [input: 2026-10-14]
Today, on the South African calendar. NEW DATA: the South African date
Closes on * [aria-describedby: The respondents' last day to answer, on the South African calendar.] [input: 2026-10-28]
The respondents' last day to answer, on the South African calendar.
### ThresholdsNothing is shown of a respondent group that falls short, so no answer can be traced to its author.
Minimum responses * [aria-describedby: Responses needed in all before the report can be released.] [input: 5]
Responses needed in all before the report can be released.
Minimum responses in a respondent group * [aria-describedby: A respondent group with fewer is suppressed: its rating and comments are never shown. At least 3 for multi-source feedback.] [input: 3]
A respondent group with fewer is suppressed: its rating and comments are never shown. At least 3 for multi-source feedback.
Minimum respondent groups reporting * [aria-describedby: How many respondent groups must clear the threshold beside it before the report can be released. Two is the College's answer: eight peer doctors alone are not multi-source feedback.] [input: 2]
How many respondent groups must clear the threshold beside it before the report can be released. Two is the College's answer: eight peer doctors alone are not multi-source feedback.
### Evidence
[aria-describedby: One campaign covers many EPAs and runs once a semester. Each one selected here becomes a separate evidence record on the trainee's portfolio when the report is released.]
Evidence for these EPAs * required
One campaign covers many EPAs and runs once a semester. Each one selected here becomes a separate evidence record on the trainee's portfolio when the report is released.
15 of 15 ticked
[checked] PAED-001 — Providing paediatric emergency care to children
[checked] PAED-002 — Managing common paediatric presentations
[checked] PAED-003 — Providing intensive care to children
[checked] PAED-004 — Managing common neonatal conditions
[checked] PAED-005 — Providing neonatal care in intensive and high-care settings
[checked] PAED-006 — Managing long-term health conditions (LTHCs)
[checked] PAED-007 — Maintaining and promoting the health and well-being of children
[checked] PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
[checked] PAED-009 — Providing care for an adolescent patient
[checked] PAED-010 — Leading and operating within a clinical team
[checked] PAED-011 — Managing population health challenges
[checked] PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
[checked] PAED-013 — Managing child protection, legal and ethical aspects of children at risk
[checked] PAED-014 — Providing palliative and end-of-life care
[checked] PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
Cancel Create campaign

######## BOARD R3-C8-threshold-refused-1280.dc.html — New MSF campaign · a threshold out of range: 2 in a respondent group, under F2's floor (S4) · 1280
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
New MSF campaign
## New MSF campaign
One campaign for one current trainee: its questionnaire, its window, its thresholds and the EPAs it is evidence for. Creating it makes a draft; nobody is emailed until you open it.
Not created. Fix the field below and create the campaign again.
Minimum responses in a respondent group: enter a number from 3 to 100.
* marks a field you must fill in.
### QuestionnaireWritten once, used by every campaign that names it.
Template *
[aria-describedby: Rates the trainee's overall professional performance. · What should the trainee keep doing or improve?]
Default MSF (Multi-source feedback)
Rates the trainee's overall professional performance. · What should the trainee keep doing or improve?
Write a new questionnaire
Kind
[checked] Multi-source feedback
Learner feedback
Template name * [input: Default MSF]
Scale question * [aria-describedby: Answered on five points, from Well below expectations to Well above expectations.] [input: Rates the trainee's overall professional performance.]
Answered on five points, from Well below expectations to Well above expectations.
Comment question * [input: What should the trainee keep doing or improve?]
Add template
### Trainee and window
Trainee *
[aria-describedby: Current trainees you can run a campaign for. NEW]
Lerato Molefe (molefe@kgk.wombat.local)
Current trainees you can run a campaign for. NEW
Opens on * [aria-describedby: Today, on the South African calendar. NEW DATA: the South African date] [input: 2026-10-14]
Today, on the South African calendar. NEW DATA: the South African date
Closes on * [aria-describedby: The respondents' last day to answer, on the South African calendar.] [input: 2026-10-28]
The respondents' last day to answer, on the South African calendar.
### ThresholdsNothing is shown of a respondent group that falls short, so no answer can be traced to its author.
Minimum responses * [aria-describedby: Responses needed in all before the report can be released.] [input: 5]
Responses needed in all before the report can be released.
Minimum responses in a respondent group * [aria-describedby: A respondent group with fewer is suppressed: its rating and comments are never shown. At least 3 for multi-source feedback. / Enter a number from 3 to 100. NEW] [input: 2]
A respondent group with fewer is suppressed: its rating and comments are never shown. At least 3 for multi-source feedback.
Enter a number from 3 to 100. NEW
Minimum respondent groups reporting * [aria-describedby: How many respondent groups must clear the threshold beside it before the report can be released. Two is the College's answer: eight peer doctors alone are not multi-source feedback.] [input: 2]
How many respondent groups must clear the threshold beside it before the report can be released. Two is the College's answer: eight peer doctors alone are not multi-source feedback.
### Evidence
[aria-describedby: One campaign covers many EPAs and runs once a semester. Each one selected here becomes a separate evidence record on the trainee's portfolio when the report is released.]
Evidence for these EPAs * required
One campaign covers many EPAs and runs once a semester. Each one selected here becomes a separate evidence record on the trainee's portfolio when the report is released.
2 of 15 ticked
PAED-001 — Providing paediatric emergency care to children
PAED-002 — Managing common paediatric presentations
PAED-003 — Providing intensive care to children
PAED-004 — Managing common neonatal conditions
PAED-005 — Providing neonatal care in intensive and high-care settings
PAED-006 — Managing long-term health conditions (LTHCs)
PAED-007 — Maintaining and promoting the health and well-being of children
PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
PAED-009 — Providing care for an adolescent patient
[checked] PAED-010 — Leading and operating within a clinical team
PAED-011 — Managing population health challenges
[checked] PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
PAED-013 — Managing child protection, legal and ethical aspects of children at risk
PAED-014 — Providing palliative and end-of-life care
PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
Cancel Create campaign

######## BOARD R3-C8-threshold-refused-390.dc.html — New MSF campaign · a threshold out of range: 2 in a respondent group, under F2's floor (S4) · 390
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
New MSF campaign
## New MSF campaign
One campaign for one current trainee: its questionnaire, its window, its thresholds and the EPAs it is evidence for. Creating it makes a draft; nobody is emailed until you open it.
Not created. Fix the field below and create the campaign again.
Minimum responses in a respondent group: enter a number from 3 to 100.
* marks a field you must fill in.
### QuestionnaireWritten once, used by every campaign that names it.
Template *
[aria-describedby: Rates the trainee's overall professional performance. · What should the trainee keep doing or improve?]
Default MSF (Multi-source feedback)
Rates the trainee's overall professional performance. · What should the trainee keep doing or improve?
Write a new questionnaire
Kind
[checked] Multi-source feedback
Learner feedback
Template name * [input: Default MSF]
Scale question * [aria-describedby: Answered on five points, from Well below expectations to Well above expectations.] [input: Rates the trainee's overall professional performance.]
Answered on five points, from Well below expectations to Well above expectations.
Comment question * [input: What should the trainee keep doing or improve?]
Add template
### Trainee and window
Trainee *
[aria-describedby: Current trainees you can run a campaign for. NEW]
Lerato Molefe (molefe@kgk.wombat.local)
Current trainees you can run a campaign for. NEW
Opens on * [aria-describedby: Today, on the South African calendar. NEW DATA: the South African date] [input: 2026-10-14]
Today, on the South African calendar. NEW DATA: the South African date
Closes on * [aria-describedby: The respondents' last day to answer, on the South African calendar.] [input: 2026-10-28]
The respondents' last day to answer, on the South African calendar.
### ThresholdsNothing is shown of a respondent group that falls short, so no answer can be traced to its author.
Minimum responses * [aria-describedby: Responses needed in all before the report can be released.] [input: 5]
Responses needed in all before the report can be released.
Minimum responses in a respondent group * [aria-describedby: A respondent group with fewer is suppressed: its rating and comments are never shown. At least 3 for multi-source feedback. / Enter a number from 3 to 100. NEW] [input: 2]
A respondent group with fewer is suppressed: its rating and comments are never shown. At least 3 for multi-source feedback.
Enter a number from 3 to 100. NEW
Minimum respondent groups reporting * [aria-describedby: How many respondent groups must clear the threshold beside it before the report can be released. Two is the College's answer: eight peer doctors alone are not multi-source feedback.] [input: 2]
How many respondent groups must clear the threshold beside it before the report can be released. Two is the College's answer: eight peer doctors alone are not multi-source feedback.
### Evidence
[aria-describedby: One campaign covers many EPAs and runs once a semester. Each one selected here becomes a separate evidence record on the trainee's portfolio when the report is released.]
Evidence for these EPAs * required
One campaign covers many EPAs and runs once a semester. Each one selected here becomes a separate evidence record on the trainee's portfolio when the report is released.
2 of 15 ticked
PAED-001 — Providing paediatric emergency care to children
PAED-002 — Managing common paediatric presentations
PAED-003 — Providing intensive care to children
PAED-004 — Managing common neonatal conditions
PAED-005 — Providing neonatal care in intensive and high-care settings
PAED-006 — Managing long-term health conditions (LTHCs)
PAED-007 — Maintaining and promoting the health and well-being of children
PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
PAED-009 — Providing care for an adolescent patient
[checked] PAED-010 — Leading and operating within a clinical team
PAED-011 — Managing population health challenges
[checked] PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
PAED-013 — Managing child protection, legal and ethical aspects of children at risk
PAED-014 — Providing palliative and end-of-life care
PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
Cancel Create campaign

######## BOARD R3-C6-read-failed-1280.dc.html — New MSF campaign · the templates or trainees could not be read (R11) · 1280
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
New MSF campaign
## New MSF campaign
One campaign for one current trainee: its questionnaire, its window, its thresholds and the EPAs it is evidence for. Creating it makes a draft; nobody is emailed until you open it.
* marks a field you must fill in.
Could not load what this campaign needs. Nothing has changed. Try again, or come back in a few minutes.Try again
The live connection stays up; Try again reads the templates and the trainees again. NEW

######## BOARD R3-C6-read-failed-390.dc.html — New MSF campaign · the templates or trainees could not be read (R11) · 390
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
New MSF campaign
## New MSF campaign
One campaign for one current trainee: its questionnaire, its window, its thresholds and the EPAs it is evidence for. Creating it makes a draft; nobody is emailed until you open it.
* marks a field you must fill in.
Could not load what this campaign needs. Nothing has changed. Try again, or come back in a few minutes.Try again
The live connection stays up; Try again reads the templates and the trainees again. NEW

######## BOARD R3-C7-loading-1280.dc.html — New MSF campaign · loading · 1280
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
New MSF campaign
## New MSF campaign
One campaign for one current trainee: its questionnaire, its window, its thresholds and the EPAs it is evidence for. Creating it makes a draft; nobody is emailed until you open it.
* marks a field you must fill in.
Loading the templates and trainees.

######## BOARD R3-C7-loading-390.dc.html — New MSF campaign · loading · 390
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
New MSF campaign
## New MSF campaign
One campaign for one current trainee: its questionnaire, its window, its thresholds and the EPAs it is evidence for. Creating it makes a draft; nobody is emailed until you open it.
* marks a field you must fill in.
Loading the templates and trainees.

######## BOARD R3-K1-draft-empty-1280.dc.html — Campaign · draft, nobody invited, Open disabled with its reason (3.35) · 1280
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF campaign for Lerato Molefe
## MSF campaign for Lerato Molefe
Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28
Draft
### Add invitees, then open the campaign to send anonymous response links.
Nobody has been invited yet.
Once the campaign opens, this page never lists who was invited or which of them responded.
[aria-describedby: Open campaign: add at least one invitee first. Opening the campaign emails each invitee a link to respond.] [aria-disabled] Open campaign Withdraw campaign…
Open campaign: add at least one invitee first. Opening the campaign emails each invitee a link to respond.
### Addresses invited
Listed only while the campaign is a draft, so that an address added by mistake can be removed. No invitee holds a working link until the campaign opens.
#### Add an invitee
* marks a field you must fill in.
Respondent email *
Respondent group
[selected] Peer doctor
Consultant
Nurse
Allied health professional
Other
Add invitee
#### 0 addresses
No address yet.
### About this campaign
Trainee
Lerato Molefe, training year 4 DATA: TrainingYear
Template
Default MSF (Multi-source feedback)
Thresholds
5 responses; 3 in a respondent group; 2 respondent groups reporting DATA: Minimum…
Evidence
Evidence for PAED-010 and PAED-012 DATA: CoveredEpas
PAED-010 — Leading and operating within a clinical team
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams

######## BOARD R3-K1-draft-empty-390.dc.html — Campaign · draft, nobody invited, Open disabled with its reason (3.35) · 390
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF campaign for Lerato Molefe
## MSF campaign for Lerato Molefe
Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28
Draft
### Add invitees, then open the campaign to send anonymous response links.
Nobody has been invited yet.
Once the campaign opens, this page never lists who was invited or which of them responded.
[aria-describedby: Open campaign: add at least one invitee first. Opening the campaign emails each invitee a link to respond.] [aria-disabled] Open campaign Withdraw campaign…
Open campaign: add at least one invitee first. Opening the campaign emails each invitee a link to respond.
### Addresses invited
Listed only while the campaign is a draft, so that an address added by mistake can be removed. No invitee holds a working link until the campaign opens.
#### Add an invitee
* marks a field you must fill in.
Respondent email *
Respondent group
[selected] Peer doctor
Consultant
Nurse
Allied health professional
Other
Add invitee
#### 0 addresses
No address yet.
### About this campaign
Trainee
Lerato Molefe, training year 4 DATA: TrainingYear
Template
Default MSF (Multi-source feedback)
Thresholds
5 responses; 3 in a respondent group; 2 respondent groups reporting DATA: Minimum…
Evidence
Evidence for PAED-010 and PAED-012 DATA: CoveredEpas
PAED-010 — Leading and operating within a clinical team
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams

######## BOARD R3-K2-draft-nine-1280.dc.html — Campaign · draft, nine invitees, Invitee added, focus back in the cleared address (3.36) · 1280
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF campaign for Lerato Molefe
## MSF campaign for Lerato Molefe
Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28
Draft
### Add invitees, then open the campaign to send anonymous response links.
Counted by respondent group. Once the campaign opens, this page never lists who was invited or which of them responded.
| Respondent group
| Invited
| Peer doctor
| 2
| Consultant
| 3
| Nurse
| 3
| Allied health professional
| 1
| All groups
| 9
Open campaign Withdraw campaign…
Opening emails each of the 9 invitees a link to respond, with 2026-10-28 as their last day.
### Addresses invited
Listed only while the campaign is a draft, so that an address added by mistake can be removed. No invitee holds a working link until the campaign opens.
#### Add an invitee
* marks a field you must fill in.
Invitee added.
Respondent email * [aria-describedby: Invitee added.]
Respondent group
[selected] Peer doctor
Consultant
Nurse
Allied health professional
Other
Add invitee
#### 9 addresses
grace.mokwena@kgk.wombat.localNurse[aria-label: Remove grace.mokwena@kgk.wombat.local (Nurse)] Remove…
kagiso.motsepe@kgk.wombat.localPeer doctor[aria-label: Remove kagiso.motsepe@kgk.wombat.local (Peer doctor)] Remove…
lindiwe.khoza@kgk.wombat.localPeer doctor[aria-label: Remove lindiwe.khoza@kgk.wombat.local (Peer doctor)] Remove…
khumalo@kgk.wombat.localConsultant[aria-label: Remove khumalo@kgk.wombat.local (Consultant)] Remove…
botha@kgk.wombat.localConsultant[aria-label: Remove botha@kgk.wombat.local (Consultant)] Remove…
naledi.sebego@kgk.wombat.localAllied health professional[aria-label: Remove naledi.sebego@kgk.wombat.local (Allied health professional)] Remove…
‹the third consultant's address›Consultant[aria-label: Remove ‹the third consultant's address› (Consultant)] Remove…
‹the third nurse's address›Nurse[aria-label: Remove ‹the third nurse's address› (Nurse)] Remove…
palesa.tau@kgk.wombat.localNurse[aria-label: Remove palesa.tau@kgk.wombat.local (Nurse)] Remove…
### About this campaign
Trainee
Lerato Molefe, training year 4 DATA: TrainingYear
Template
Default MSF (Multi-source feedback)
Thresholds
5 responses; 3 in a respondent group; 2 respondent groups reporting DATA: Minimum…
Evidence
Evidence for PAED-010 and PAED-012 DATA: CoveredEpas
PAED-010 — Leading and operating within a clinical team
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams

######## BOARD R3-K2-draft-nine-768.dc.html — Campaign · draft, nine invitees, Invitee added, focus back in the cleared address (3.36) · 768
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF campaign for Lerato Molefe
## MSF campaign for Lerato Molefe
Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28
Draft
### Add invitees, then open the campaign to send anonymous response links.
Counted by respondent group. Once the campaign opens, this page never lists who was invited or which of them responded.
| Respondent group
| Invited
| Peer doctor
| 2
| Consultant
| 3
| Nurse
| 3
| Allied health professional
| 1
| All groups
| 9
Open campaign Withdraw campaign…
Opening emails each of the 9 invitees a link to respond, with 2026-10-28 as their last day.
### Addresses invited
Listed only while the campaign is a draft, so that an address added by mistake can be removed. No invitee holds a working link until the campaign opens.
#### Add an invitee
* marks a field you must fill in.
Invitee added.
Respondent email * [aria-describedby: Invitee added.]
Respondent group
[selected] Peer doctor
Consultant
Nurse
Allied health professional
Other
Add invitee
#### 9 addresses
grace.mokwena@kgk.wombat.localNurse[aria-label: Remove grace.mokwena@kgk.wombat.local (Nurse)] Remove…
kagiso.motsepe@kgk.wombat.localPeer doctor[aria-label: Remove kagiso.motsepe@kgk.wombat.local (Peer doctor)] Remove…
lindiwe.khoza@kgk.wombat.localPeer doctor[aria-label: Remove lindiwe.khoza@kgk.wombat.local (Peer doctor)] Remove…
khumalo@kgk.wombat.localConsultant[aria-label: Remove khumalo@kgk.wombat.local (Consultant)] Remove…
botha@kgk.wombat.localConsultant[aria-label: Remove botha@kgk.wombat.local (Consultant)] Remove…
naledi.sebego@kgk.wombat.localAllied health professional[aria-label: Remove naledi.sebego@kgk.wombat.local (Allied health professional)] Remove…
‹the third consultant's address›Consultant[aria-label: Remove ‹the third consultant's address› (Consultant)] Remove…
‹the third nurse's address›Nurse[aria-label: Remove ‹the third nurse's address› (Nurse)] Remove…
palesa.tau@kgk.wombat.localNurse[aria-label: Remove palesa.tau@kgk.wombat.local (Nurse)] Remove…
### About this campaign
Trainee
Lerato Molefe, training year 4 DATA: TrainingYear
Template
Default MSF (Multi-source feedback)
Thresholds
5 responses; 3 in a respondent group; 2 respondent groups reporting DATA: Minimum…
Evidence
Evidence for PAED-010 and PAED-012 DATA: CoveredEpas
PAED-010 — Leading and operating within a clinical team
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams

######## BOARD R3-K2-draft-nine-390.dc.html — Campaign · draft, nine invitees, Invitee added, focus back in the cleared address (3.36) · 390
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF campaign for Lerato Molefe
## MSF campaign for Lerato Molefe
Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28
Draft
### Add invitees, then open the campaign to send anonymous response links.
Counted by respondent group. Once the campaign opens, this page never lists who was invited or which of them responded.
| Respondent group
| Invited
| Peer doctor
| 2
| Consultant
| 3
| Nurse
| 3
| Allied health professional
| 1
| All groups
| 9
Open campaign Withdraw campaign…
Opening emails each of the 9 invitees a link to respond, with 2026-10-28 as their last day.
### Addresses invited
Listed only while the campaign is a draft, so that an address added by mistake can be removed. No invitee holds a working link until the campaign opens.
#### Add an invitee
* marks a field you must fill in.
Invitee added.
Respondent email * [aria-describedby: Invitee added.]
Respondent group
[selected] Peer doctor
Consultant
Nurse
Allied health professional
Other
Add invitee
#### 9 addresses
grace.mokwena@kgk.wombat.localNurse[aria-label: Remove grace.mokwena@kgk.wombat.local (Nurse)] Remove…
kagiso.motsepe@kgk.wombat.localPeer doctor[aria-label: Remove kagiso.motsepe@kgk.wombat.local (Peer doctor)] Remove…
lindiwe.khoza@kgk.wombat.localPeer doctor[aria-label: Remove lindiwe.khoza@kgk.wombat.local (Peer doctor)] Remove…
khumalo@kgk.wombat.localConsultant[aria-label: Remove khumalo@kgk.wombat.local (Consultant)] Remove…
botha@kgk.wombat.localConsultant[aria-label: Remove botha@kgk.wombat.local (Consultant)] Remove…
naledi.sebego@kgk.wombat.localAllied health professional[aria-label: Remove naledi.sebego@kgk.wombat.local (Allied health professional)] Remove…
‹the third consultant's address›Consultant[aria-label: Remove ‹the third consultant's address› (Consultant)] Remove…
‹the third nurse's address›Nurse[aria-label: Remove ‹the third nurse's address› (Nurse)] Remove…
palesa.tau@kgk.wombat.localNurse[aria-label: Remove palesa.tau@kgk.wombat.local (Nurse)] Remove…
### About this campaign
Trainee
Lerato Molefe, training year 4 DATA: TrainingYear
Template
Default MSF (Multi-source feedback)
Thresholds
5 responses; 3 in a respondent group; 2 respondent groups reporting DATA: Minimum…
Evidence
Evidence for PAED-010 and PAED-012 DATA: CoveredEpas
PAED-010 — Leading and operating within a clinical team
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams

######## BOARD R3-K3-remove-dialog-1280.dc.html — Campaign · Remove's dialog on Palesa Tau's address, focus on Keep the invitee (3.36) · 1280
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF campaign for Lerato Molefe
## MSF campaign for Lerato Molefe
Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28
Draft
### Add invitees, then open the campaign to send anonymous response links.
Counted by respondent group. Once the campaign opens, this page never lists who was invited or which of them responded.
| Respondent group
| Invited
| Peer doctor
| 2
| Consultant
| 3
| Nurse
| 2
| Allied health professional
| 2
| All groups
| 9
Open campaign Withdraw campaign…
Opening emails each of the 9 invitees a link to respond, with 2026-10-28 as their last day.
### Addresses invited
Listed only while the campaign is a draft, so that an address added by mistake can be removed. No invitee holds a working link until the campaign opens.
#### Add an invitee
* marks a field you must fill in.
Respondent email *
Respondent group
[selected] Peer doctor
Consultant
Nurse
Allied health professional
Other
Add invitee
#### 9 addresses
grace.mokwena@kgk.wombat.localNurse[aria-label: Remove grace.mokwena@kgk.wombat.local (Nurse)] Remove…
palesa.tau@kgk.wombat.localAllied health professional[aria-label: Remove palesa.tau@kgk.wombat.local (Allied health professional)] Remove…
kagiso.motsepe@kgk.wombat.localPeer doctor[aria-label: Remove kagiso.motsepe@kgk.wombat.local (Peer doctor)] Remove…
lindiwe.khoza@kgk.wombat.localPeer doctor[aria-label: Remove lindiwe.khoza@kgk.wombat.local (Peer doctor)] Remove…
khumalo@kgk.wombat.localConsultant[aria-label: Remove khumalo@kgk.wombat.local (Consultant)] Remove…
botha@kgk.wombat.localConsultant[aria-label: Remove botha@kgk.wombat.local (Consultant)] Remove…
naledi.sebego@kgk.wombat.localAllied health professional[aria-label: Remove naledi.sebego@kgk.wombat.local (Allied health professional)] Remove…
‹the third consultant's address›Consultant[aria-label: Remove ‹the third consultant's address› (Consultant)] Remove…
‹the third nurse's address›Nurse[aria-label: Remove ‹the third nurse's address› (Nurse)] Remove…
### About this campaign
Trainee
Lerato Molefe, training year 4 DATA: TrainingYear
Template
Default MSF (Multi-source feedback)
Thresholds
5 responses; 3 in a respondent group; 2 respondent groups reporting DATA: Minimum…
Evidence
Evidence for PAED-010 and PAED-012 DATA: CoveredEpas
PAED-010 — Leading and operating within a clinical team
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
[aria-describedby: Remove palesa.tau@kgk.wombat.local (Allied health professional) from this campaign? They hold no working link, and will not be sent one when the campaign opens. While the campaign is a draft, they can be added again.]
### Remove this invitee?
Remove palesa.tau@kgk.wombat.local (Allied health professional) from this campaign? They hold no working link, and will not be sent one when the campaign opens. While the campaign is a draft, they can be added again.
Keep the invitee Remove invitee

######## BOARD R3-K3-remove-dialog-390.dc.html — Campaign · Remove's dialog on Palesa Tau's address, focus on Keep the invitee (3.36) · 390
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF campaign for Lerato Molefe
## MSF campaign for Lerato Molefe
Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28
Draft
### Add invitees, then open the campaign to send anonymous response links.
Counted by respondent group. Once the campaign opens, this page never lists who was invited or which of them responded.
| Respondent group
| Invited
| Peer doctor
| 2
| Consultant
| 3
| Nurse
| 2
| Allied health professional
| 2
| All groups
| 9
Open campaign Withdraw campaign…
Opening emails each of the 9 invitees a link to respond, with 2026-10-28 as their last day.
### Addresses invited
Listed only while the campaign is a draft, so that an address added by mistake can be removed. No invitee holds a working link until the campaign opens.
#### Add an invitee
* marks a field you must fill in.
Respondent email *
Respondent group
[selected] Peer doctor
Consultant
Nurse
Allied health professional
Other
Add invitee
#### 9 addresses
grace.mokwena@kgk.wombat.localNurse[aria-label: Remove grace.mokwena@kgk.wombat.local (Nurse)] Remove…
palesa.tau@kgk.wombat.localAllied health professional[aria-label: Remove palesa.tau@kgk.wombat.local (Allied health professional)] Remove…
kagiso.motsepe@kgk.wombat.localPeer doctor[aria-label: Remove kagiso.motsepe@kgk.wombat.local (Peer doctor)] Remove…
lindiwe.khoza@kgk.wombat.localPeer doctor[aria-label: Remove lindiwe.khoza@kgk.wombat.local (Peer doctor)] Remove…
khumalo@kgk.wombat.localConsultant[aria-label: Remove khumalo@kgk.wombat.local (Consultant)] Remove…
botha@kgk.wombat.localConsultant[aria-label: Remove botha@kgk.wombat.local (Consultant)] Remove…
naledi.sebego@kgk.wombat.localAllied health professional[aria-label: Remove naledi.sebego@kgk.wombat.local (Allied health professional)] Remove…
‹the third consultant's address›Consultant[aria-label: Remove ‹the third consultant's address› (Consultant)] Remove…
‹the third nurse's address›Nurse[aria-label: Remove ‹the third nurse's address› (Nurse)] Remove…
### About this campaign
Trainee
Lerato Molefe, training year 4 DATA: TrainingYear
Template
Default MSF (Multi-source feedback)
Thresholds
5 responses; 3 in a respondent group; 2 respondent groups reporting DATA: Minimum…
Evidence
Evidence for PAED-010 and PAED-012 DATA: CoveredEpas
PAED-010 — Leading and operating within a clinical team
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
[aria-describedby: Remove palesa.tau@kgk.wombat.local (Allied health professional) from this campaign? They hold no working link, and will not be sent one when the campaign opens. While the campaign is a draft, they can be added again.]
### Remove this invitee?
Remove palesa.tau@kgk.wombat.local (Allied health professional) from this campaign? They hold no working link, and will not be sent one when the campaign opens. While the campaign is a draft, they can be added again.
Keep the invitee Remove invitee

######## BOARD R3-K4-removed-1280.dc.html — Campaign · removed, the result above the address rows, focused (3.36) · 1280
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
Eight invitees once Palesa Tau's first address is removed (F2's cast: nine, less one).
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF campaign for Lerato Molefe
## MSF campaign for Lerato Molefe
Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28
Draft
### Add invitees, then open the campaign to send anonymous response links.
Counted by respondent group. Once the campaign opens, this page never lists who was invited or which of them responded.
| Respondent group
| Invited
| Peer doctor
| 2
| Consultant
| 3
| Nurse
| 2
| Allied health professional
| 1
| All groups
| 8
Open campaign Withdraw campaign…
Opening emails each of the 8 invitees a link to respond, with 2026-10-28 as their last day.
### Addresses invited
Listed only while the campaign is a draft, so that an address added by mistake can be removed. No invitee holds a working link until the campaign opens.
#### Add an invitee
* marks a field you must fill in.
Respondent email *
Respondent group
[selected] Peer doctor
Consultant
Nurse
Allied health professional
Other
Add invitee
palesa.tau@kgk.wombat.local (Allied health professional) has been removed from this campaign, and will not be emailed a link when it opens.
#### 8 addresses
grace.mokwena@kgk.wombat.localNurse[aria-label: Remove grace.mokwena@kgk.wombat.local (Nurse)] Remove…
kagiso.motsepe@kgk.wombat.localPeer doctor[aria-label: Remove kagiso.motsepe@kgk.wombat.local (Peer doctor)] Remove…
lindiwe.khoza@kgk.wombat.localPeer doctor[aria-label: Remove lindiwe.khoza@kgk.wombat.local (Peer doctor)] Remove…
khumalo@kgk.wombat.localConsultant[aria-label: Remove khumalo@kgk.wombat.local (Consultant)] Remove…
botha@kgk.wombat.localConsultant[aria-label: Remove botha@kgk.wombat.local (Consultant)] Remove…
naledi.sebego@kgk.wombat.localAllied health professional[aria-label: Remove naledi.sebego@kgk.wombat.local (Allied health professional)] Remove…
‹the third consultant's address›Consultant[aria-label: Remove ‹the third consultant's address› (Consultant)] Remove…
‹the third nurse's address›Nurse[aria-label: Remove ‹the third nurse's address› (Nurse)] Remove…
### About this campaign
Trainee
Lerato Molefe, training year 4 DATA: TrainingYear
Template
Default MSF (Multi-source feedback)
Thresholds
5 responses; 3 in a respondent group; 2 respondent groups reporting DATA: Minimum…
Evidence
Evidence for PAED-010 and PAED-012 DATA: CoveredEpas
PAED-010 — Leading and operating within a clinical team
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams

######## BOARD R3-K4-removed-390.dc.html — Campaign · removed, the result above the address rows, focused (3.36) · 390
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
Eight invitees once Palesa Tau's first address is removed (F2's cast: nine, less one).
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF campaign for Lerato Molefe
## MSF campaign for Lerato Molefe
Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28
Draft
### Add invitees, then open the campaign to send anonymous response links.
Counted by respondent group. Once the campaign opens, this page never lists who was invited or which of them responded.
| Respondent group
| Invited
| Peer doctor
| 2
| Consultant
| 3
| Nurse
| 2
| Allied health professional
| 1
| All groups
| 8
Open campaign Withdraw campaign…
Opening emails each of the 8 invitees a link to respond, with 2026-10-28 as their last day.
### Addresses invited
Listed only while the campaign is a draft, so that an address added by mistake can be removed. No invitee holds a working link until the campaign opens.
#### Add an invitee
* marks a field you must fill in.
Respondent email *
Respondent group
[selected] Peer doctor
Consultant
Nurse
Allied health professional
Other
Add invitee
palesa.tau@kgk.wombat.local (Allied health professional) has been removed from this campaign, and will not be emailed a link when it opens.
#### 8 addresses
grace.mokwena@kgk.wombat.localNurse[aria-label: Remove grace.mokwena@kgk.wombat.local (Nurse)] Remove…
kagiso.motsepe@kgk.wombat.localPeer doctor[aria-label: Remove kagiso.motsepe@kgk.wombat.local (Peer doctor)] Remove…
lindiwe.khoza@kgk.wombat.localPeer doctor[aria-label: Remove lindiwe.khoza@kgk.wombat.local (Peer doctor)] Remove…
khumalo@kgk.wombat.localConsultant[aria-label: Remove khumalo@kgk.wombat.local (Consultant)] Remove…
botha@kgk.wombat.localConsultant[aria-label: Remove botha@kgk.wombat.local (Consultant)] Remove…
naledi.sebego@kgk.wombat.localAllied health professional[aria-label: Remove naledi.sebego@kgk.wombat.local (Allied health professional)] Remove…
‹the third consultant's address›Consultant[aria-label: Remove ‹the third consultant's address› (Consultant)] Remove…
‹the third nurse's address›Nurse[aria-label: Remove ‹the third nurse's address› (Nurse)] Remove…
### About this campaign
Trainee
Lerato Molefe, training year 4 DATA: TrainingYear
Template
Default MSF (Multi-source feedback)
Thresholds
5 responses; 3 in a respondent group; 2 respondent groups reporting DATA: Minimum…
Evidence
Evidence for PAED-010 and PAED-012 DATA: CoveredEpas
PAED-010 — Leading and operating within a clinical team
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams

######## BOARD R3-K5-refused-duplicate-1280.dc.html — Campaign · a refused add: the duplicate, under the field, typed address kept, focused (R4d, T228) · 1280
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF campaign for Lerato Molefe
## MSF campaign for Lerato Molefe
Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28
Draft
### Add invitees, then open the campaign to send anonymous response links.
Counted by respondent group. Once the campaign opens, this page never lists who was invited or which of them responded.
| Respondent group
| Invited
| Peer doctor
| 2
| Consultant
| 3
| Nurse
| 3
| Allied health professional
| 1
| All groups
| 9
Open campaign Withdraw campaign…
Opening emails each of the 9 invitees a link to respond, with 2026-10-28 as their last day.
### Addresses invited
Listed only while the campaign is a draft, so that an address added by mistake can be removed. No invitee holds a working link until the campaign opens.
#### Add an invitee
* marks a field you must fill in.
Respondent email * [aria-describedby: This campaign already invites that address. Capitals are ignored, so the same address typed differently is the same respondent. Each respondent is invited once, so that nobody can respond twice. The invitee has not been added again.] [input: Grace.Mokwena@kgk.wombat.local]
This campaign already invites that address. Capitals are ignored, so the same address typed differently is the same respondent. Each respondent is invited once, so that nobody can respond twice. The invitee has not been added again.
Respondent group
Peer doctor
Consultant
[selected] Nurse
Allied health professional
Other
Add invitee
#### 9 addresses
grace.mokwena@kgk.wombat.localNurse[aria-label: Remove grace.mokwena@kgk.wombat.local (Nurse)] Remove…
kagiso.motsepe@kgk.wombat.localPeer doctor[aria-label: Remove kagiso.motsepe@kgk.wombat.local (Peer doctor)] Remove…
lindiwe.khoza@kgk.wombat.localPeer doctor[aria-label: Remove lindiwe.khoza@kgk.wombat.local (Peer doctor)] Remove…
khumalo@kgk.wombat.localConsultant[aria-label: Remove khumalo@kgk.wombat.local (Consultant)] Remove…
botha@kgk.wombat.localConsultant[aria-label: Remove botha@kgk.wombat.local (Consultant)] Remove…
naledi.sebego@kgk.wombat.localAllied health professional[aria-label: Remove naledi.sebego@kgk.wombat.local (Allied health professional)] Remove…
‹the third consultant's address›Consultant[aria-label: Remove ‹the third consultant's address› (Consultant)] Remove…
‹the third nurse's address›Nurse[aria-label: Remove ‹the third nurse's address› (Nurse)] Remove…
palesa.tau@kgk.wombat.localNurse[aria-label: Remove palesa.tau@kgk.wombat.local (Nurse)] Remove…
### About this campaign
Trainee
Lerato Molefe, training year 4 DATA: TrainingYear
Template
Default MSF (Multi-source feedback)
Thresholds
5 responses; 3 in a respondent group; 2 respondent groups reporting DATA: Minimum…
Evidence
Evidence for PAED-010 and PAED-012 DATA: CoveredEpas
PAED-010 — Leading and operating within a clinical team
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams

######## BOARD R3-K5-refused-duplicate-390.dc.html — Campaign · a refused add: the duplicate, under the field, typed address kept, focused (R4d, T228) · 390
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF campaign for Lerato Molefe
## MSF campaign for Lerato Molefe
Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28
Draft
### Add invitees, then open the campaign to send anonymous response links.
Counted by respondent group. Once the campaign opens, this page never lists who was invited or which of them responded.
| Respondent group
| Invited
| Peer doctor
| 2
| Consultant
| 3
| Nurse
| 3
| Allied health professional
| 1
| All groups
| 9
Open campaign Withdraw campaign…
Opening emails each of the 9 invitees a link to respond, with 2026-10-28 as their last day.
### Addresses invited
Listed only while the campaign is a draft, so that an address added by mistake can be removed. No invitee holds a working link until the campaign opens.
#### Add an invitee
* marks a field you must fill in.
Respondent email * [aria-describedby: This campaign already invites that address. Capitals are ignored, so the same address typed differently is the same respondent. Each respondent is invited once, so that nobody can respond twice. The invitee has not been added again.] [input: Grace.Mokwena@kgk.wombat.local]
This campaign already invites that address. Capitals are ignored, so the same address typed differently is the same respondent. Each respondent is invited once, so that nobody can respond twice. The invitee has not been added again.
Respondent group
Peer doctor
Consultant
[selected] Nurse
Allied health professional
Other
Add invitee
#### 9 addresses
grace.mokwena@kgk.wombat.localNurse[aria-label: Remove grace.mokwena@kgk.wombat.local (Nurse)] Remove…
kagiso.motsepe@kgk.wombat.localPeer doctor[aria-label: Remove kagiso.motsepe@kgk.wombat.local (Peer doctor)] Remove…
lindiwe.khoza@kgk.wombat.localPeer doctor[aria-label: Remove lindiwe.khoza@kgk.wombat.local (Peer doctor)] Remove…
khumalo@kgk.wombat.localConsultant[aria-label: Remove khumalo@kgk.wombat.local (Consultant)] Remove…
botha@kgk.wombat.localConsultant[aria-label: Remove botha@kgk.wombat.local (Consultant)] Remove…
naledi.sebego@kgk.wombat.localAllied health professional[aria-label: Remove naledi.sebego@kgk.wombat.local (Allied health professional)] Remove…
‹the third consultant's address›Consultant[aria-label: Remove ‹the third consultant's address› (Consultant)] Remove…
‹the third nurse's address›Nurse[aria-label: Remove ‹the third nurse's address› (Nurse)] Remove…
palesa.tau@kgk.wombat.localNurse[aria-label: Remove palesa.tau@kgk.wombat.local (Nurse)] Remove…
### About this campaign
Trainee
Lerato Molefe, training year 4 DATA: TrainingYear
Template
Default MSF (Multi-source feedback)
Thresholds
5 responses; 3 in a respondent group; 2 respondent groups reporting DATA: Minimum…
Evidence
Evidence for PAED-010 and PAED-012 DATA: CoveredEpas
PAED-010 — Leading and operating within a clinical team
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams

######## BOARD R3-K5b-refused-blank-1280.dc.html — Campaign · a refused add: left blank (R4d, T324) · 1280
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF campaign for Lerato Molefe
## MSF campaign for Lerato Molefe
Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28
Draft
### Add invitees, then open the campaign to send anonymous response links.
Counted by respondent group. Once the campaign opens, this page never lists who was invited or which of them responded.
| Respondent group
| Invited
| Peer doctor
| 2
| Consultant
| 3
| Nurse
| 3
| Allied health professional
| 1
| All groups
| 9
Open campaign Withdraw campaign…
Opening emails each of the 9 invitees a link to respond, with 2026-10-28 as their last day.
### Addresses invited
Listed only while the campaign is a draft, so that an address added by mistake can be removed. No invitee holds a working link until the campaign opens.
#### Add an invitee
* marks a field you must fill in.
Respondent email * [aria-describedby: Enter the respondent's email address. NEW]
Enter the respondent's email address. NEW
Respondent group
[selected] Peer doctor
Consultant
Nurse
Allied health professional
Other
Add invitee
#### 9 addresses
grace.mokwena@kgk.wombat.localNurse[aria-label: Remove grace.mokwena@kgk.wombat.local (Nurse)] Remove…
kagiso.motsepe@kgk.wombat.localPeer doctor[aria-label: Remove kagiso.motsepe@kgk.wombat.local (Peer doctor)] Remove…
lindiwe.khoza@kgk.wombat.localPeer doctor[aria-label: Remove lindiwe.khoza@kgk.wombat.local (Peer doctor)] Remove…
khumalo@kgk.wombat.localConsultant[aria-label: Remove khumalo@kgk.wombat.local (Consultant)] Remove…
botha@kgk.wombat.localConsultant[aria-label: Remove botha@kgk.wombat.local (Consultant)] Remove…
naledi.sebego@kgk.wombat.localAllied health professional[aria-label: Remove naledi.sebego@kgk.wombat.local (Allied health professional)] Remove…
‹the third consultant's address›Consultant[aria-label: Remove ‹the third consultant's address› (Consultant)] Remove…
‹the third nurse's address›Nurse[aria-label: Remove ‹the third nurse's address› (Nurse)] Remove…
palesa.tau@kgk.wombat.localNurse[aria-label: Remove palesa.tau@kgk.wombat.local (Nurse)] Remove…
### About this campaign
Trainee
Lerato Molefe, training year 4 DATA: TrainingYear
Template
Default MSF (Multi-source feedback)
Thresholds
5 responses; 3 in a respondent group; 2 respondent groups reporting DATA: Minimum…
Evidence
Evidence for PAED-010 and PAED-012 DATA: CoveredEpas
PAED-010 — Leading and operating within a clinical team
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams

######## BOARD R3-K5b-refused-blank-390.dc.html — Campaign · a refused add: left blank (R4d, T324) · 390
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF campaign for Lerato Molefe
## MSF campaign for Lerato Molefe
Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28
Draft
### Add invitees, then open the campaign to send anonymous response links.
Counted by respondent group. Once the campaign opens, this page never lists who was invited or which of them responded.
| Respondent group
| Invited
| Peer doctor
| 2
| Consultant
| 3
| Nurse
| 3
| Allied health professional
| 1
| All groups
| 9
Open campaign Withdraw campaign…
Opening emails each of the 9 invitees a link to respond, with 2026-10-28 as their last day.
### Addresses invited
Listed only while the campaign is a draft, so that an address added by mistake can be removed. No invitee holds a working link until the campaign opens.
#### Add an invitee
* marks a field you must fill in.
Respondent email * [aria-describedby: Enter the respondent's email address. NEW]
Enter the respondent's email address. NEW
Respondent group
[selected] Peer doctor
Consultant
Nurse
Allied health professional
Other
Add invitee
#### 9 addresses
grace.mokwena@kgk.wombat.localNurse[aria-label: Remove grace.mokwena@kgk.wombat.local (Nurse)] Remove…
kagiso.motsepe@kgk.wombat.localPeer doctor[aria-label: Remove kagiso.motsepe@kgk.wombat.local (Peer doctor)] Remove…
lindiwe.khoza@kgk.wombat.localPeer doctor[aria-label: Remove lindiwe.khoza@kgk.wombat.local (Peer doctor)] Remove…
khumalo@kgk.wombat.localConsultant[aria-label: Remove khumalo@kgk.wombat.local (Consultant)] Remove…
botha@kgk.wombat.localConsultant[aria-label: Remove botha@kgk.wombat.local (Consultant)] Remove…
naledi.sebego@kgk.wombat.localAllied health professional[aria-label: Remove naledi.sebego@kgk.wombat.local (Allied health professional)] Remove…
‹the third consultant's address›Consultant[aria-label: Remove ‹the third consultant's address› (Consultant)] Remove…
‹the third nurse's address›Nurse[aria-label: Remove ‹the third nurse's address› (Nurse)] Remove…
palesa.tau@kgk.wombat.localNurse[aria-label: Remove palesa.tau@kgk.wombat.local (Nurse)] Remove…
### About this campaign
Trainee
Lerato Molefe, training year 4 DATA: TrainingYear
Template
Default MSF (Multi-source feedback)
Thresholds
5 responses; 3 in a respondent group; 2 respondent groups reporting DATA: Minimum…
Evidence
Evidence for PAED-010 and PAED-012 DATA: CoveredEpas
PAED-010 — Leading and operating within a clinical team
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams

######## BOARD R3-K5c-refused-malformed-1280.dc.html — Campaign · a refused add: a malformed address (S4) · 1280
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF campaign for Lerato Molefe
## MSF campaign for Lerato Molefe
Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28
Draft
### Add invitees, then open the campaign to send anonymous response links.
Counted by respondent group. Once the campaign opens, this page never lists who was invited or which of them responded.
| Respondent group
| Invited
| Peer doctor
| 2
| Consultant
| 3
| Nurse
| 3
| Allied health professional
| 1
| All groups
| 9
Open campaign Withdraw campaign…
Opening emails each of the 9 invitees a link to respond, with 2026-10-28 as their last day.
### Addresses invited
Listed only while the campaign is a draft, so that an address added by mistake can be removed. No invitee holds a working link until the campaign opens.
#### Add an invitee
* marks a field you must fill in.
Respondent email * [aria-describedby: Enter a valid email address, for example name@example.org. NEW] [input: naledi.sebego@kgk]
Enter a valid email address, for example name@example.org. NEW
Respondent group
[selected] Peer doctor
Consultant
Nurse
Allied health professional
Other
Add invitee
#### 9 addresses
grace.mokwena@kgk.wombat.localNurse[aria-label: Remove grace.mokwena@kgk.wombat.local (Nurse)] Remove…
kagiso.motsepe@kgk.wombat.localPeer doctor[aria-label: Remove kagiso.motsepe@kgk.wombat.local (Peer doctor)] Remove…
lindiwe.khoza@kgk.wombat.localPeer doctor[aria-label: Remove lindiwe.khoza@kgk.wombat.local (Peer doctor)] Remove…
khumalo@kgk.wombat.localConsultant[aria-label: Remove khumalo@kgk.wombat.local (Consultant)] Remove…
botha@kgk.wombat.localConsultant[aria-label: Remove botha@kgk.wombat.local (Consultant)] Remove…
naledi.sebego@kgk.wombat.localAllied health professional[aria-label: Remove naledi.sebego@kgk.wombat.local (Allied health professional)] Remove…
‹the third consultant's address›Consultant[aria-label: Remove ‹the third consultant's address› (Consultant)] Remove…
‹the third nurse's address›Nurse[aria-label: Remove ‹the third nurse's address› (Nurse)] Remove…
palesa.tau@kgk.wombat.localNurse[aria-label: Remove palesa.tau@kgk.wombat.local (Nurse)] Remove…
### About this campaign
Trainee
Lerato Molefe, training year 4 DATA: TrainingYear
Template
Default MSF (Multi-source feedback)
Thresholds
5 responses; 3 in a respondent group; 2 respondent groups reporting DATA: Minimum…
Evidence
Evidence for PAED-010 and PAED-012 DATA: CoveredEpas
PAED-010 — Leading and operating within a clinical team
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams

######## BOARD R3-K5c-refused-malformed-390.dc.html — Campaign · a refused add: a malformed address (S4) · 390
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF campaign for Lerato Molefe
## MSF campaign for Lerato Molefe
Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28
Draft
### Add invitees, then open the campaign to send anonymous response links.
Counted by respondent group. Once the campaign opens, this page never lists who was invited or which of them responded.
| Respondent group
| Invited
| Peer doctor
| 2
| Consultant
| 3
| Nurse
| 3
| Allied health professional
| 1
| All groups
| 9
Open campaign Withdraw campaign…
Opening emails each of the 9 invitees a link to respond, with 2026-10-28 as their last day.
### Addresses invited
Listed only while the campaign is a draft, so that an address added by mistake can be removed. No invitee holds a working link until the campaign opens.
#### Add an invitee
* marks a field you must fill in.
Respondent email * [aria-describedby: Enter a valid email address, for example name@example.org. NEW] [input: naledi.sebego@kgk]
Enter a valid email address, for example name@example.org. NEW
Respondent group
[selected] Peer doctor
Consultant
Nurse
Allied health professional
Other
Add invitee
#### 9 addresses
grace.mokwena@kgk.wombat.localNurse[aria-label: Remove grace.mokwena@kgk.wombat.local (Nurse)] Remove…
kagiso.motsepe@kgk.wombat.localPeer doctor[aria-label: Remove kagiso.motsepe@kgk.wombat.local (Peer doctor)] Remove…
lindiwe.khoza@kgk.wombat.localPeer doctor[aria-label: Remove lindiwe.khoza@kgk.wombat.local (Peer doctor)] Remove…
khumalo@kgk.wombat.localConsultant[aria-label: Remove khumalo@kgk.wombat.local (Consultant)] Remove…
botha@kgk.wombat.localConsultant[aria-label: Remove botha@kgk.wombat.local (Consultant)] Remove…
naledi.sebego@kgk.wombat.localAllied health professional[aria-label: Remove naledi.sebego@kgk.wombat.local (Allied health professional)] Remove…
‹the third consultant's address›Consultant[aria-label: Remove ‹the third consultant's address› (Consultant)] Remove…
‹the third nurse's address›Nurse[aria-label: Remove ‹the third nurse's address› (Nurse)] Remove…
palesa.tau@kgk.wombat.localNurse[aria-label: Remove palesa.tau@kgk.wombat.local (Nurse)] Remove…
### About this campaign
Trainee
Lerato Molefe, training year 4 DATA: TrainingYear
Template
Default MSF (Multi-source feedback)
Thresholds
5 responses; 3 in a respondent group; 2 respondent groups reporting DATA: Minimum…
Evidence
Evidence for PAED-010 and PAED-012 DATA: CoveredEpas
PAED-010 — Leading and operating within a clinical team
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams

######## BOARD R3-K6-opened-1280.dc.html — Campaign · opened, links being sent, the result focused (3.37) · 1280
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF campaign for Lerato Molefe
## MSF campaign for Lerato Molefe
Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28
Campaign opened; links are being sent.
Open
### 0 of 9 invitees have responded.
Each respondent is emailed a link, which gives 2026-10-28 as their last day to respond. The campaign closes by itself once that day has passed; to close it sooner, use Close campaign on its report.
#### Invitees by respondent group
Counted by respondent group. This page never lists who was invited or which of them responded.
| Respondent group
| Invited
| Responded
| Peer doctor
| 2
| 0
| Consultant
| 3
| 0
| Nurse
| 3
| 0
| Allied health professional
| 1
| 0
| All groups
| 9
| 0
#### Links
9 links are still being sent. Reload this page to see whether they were delivered.
View report Withdraw campaign…
### About this campaign
Trainee
Lerato Molefe, training year 4 DATA: TrainingYear
Template
Default MSF (Multi-source feedback)
Thresholds
5 responses; 3 in a respondent group; 2 respondent groups reporting DATA: Minimum…
Evidence
Evidence for PAED-010 and PAED-012 DATA: CoveredEpas
PAED-010 — Leading and operating within a clinical team
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams

######## BOARD R3-K6-opened-390.dc.html — Campaign · opened, links being sent, the result focused (3.37) · 390
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF campaign for Lerato Molefe
## MSF campaign for Lerato Molefe
Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28
Campaign opened; links are being sent.
Open
### 0 of 9 invitees have responded.
Each respondent is emailed a link, which gives 2026-10-28 as their last day to respond. The campaign closes by itself once that day has passed; to close it sooner, use Close campaign on its report.
#### Invitees by respondent group
Counted by respondent group. This page never lists who was invited or which of them responded.
| Respondent group
| Invited
| Responded
| Peer doctor
| 2
| 0
| Consultant
| 3
| 0
| Nurse
| 3
| 0
| Allied health professional
| 1
| 0
| All groups
| 9
| 0
#### Links
9 links are still being sent. Reload this page to see whether they were delivered.
View report Withdraw campaign…
### About this campaign
Trainee
Lerato Molefe, training year 4 DATA: TrainingYear
Template
Default MSF (Multi-source feedback)
Thresholds
5 responses; 3 in a respondent group; 2 respondent groups reporting DATA: Minimum…
Evidence
Evidence for PAED-010 and PAED-012 DATA: CoveredEpas
PAED-010 — Leading and operating within a clinical team
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams

######## BOARD R3-K7-not-delivered-1280.dc.html — Campaign · a link not delivered, Resend in its row (3.43) · 1280
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF campaign for Lerato Molefe
## MSF campaign for Lerato Molefe
Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28
Open
### 8 of 9 invitees have responded.
Each respondent is emailed a link, which gives 2026-10-28 as their last day to respond. The campaign closes by itself once that day has passed; to close it sooner, use Close campaign on its report.
#### Invitees by respondent group
Counted by respondent group. This page never lists who was invited or which of them responded.
| Respondent group
| Invited
| Responded
| Peer doctor
| 2
| 1
| Consultant
| 3
| 3
| Nurse
| 3
| 3
| Allied health professional
| 1
| 1
| All groups
| 9
| 8
#### Links
1 link was not delivered. Resend sends each of these respondents a new link; this page never says who they are.Resend 1 link
A link nothing was heard of for an hour counts as not delivered. NEW
View report Withdraw campaign…
### About this campaign
Trainee
Lerato Molefe, training year 4 DATA: TrainingYear
Template
Default MSF (Multi-source feedback)
Thresholds
5 responses; 3 in a respondent group; 2 respondent groups reporting DATA: Minimum…
Evidence
Evidence for PAED-010 and PAED-012 DATA: CoveredEpas
PAED-010 — Leading and operating within a clinical team
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams

######## BOARD R3-K7-not-delivered-768.dc.html — Campaign · a link not delivered, Resend in its row (3.43) · 768
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF campaign for Lerato Molefe
## MSF campaign for Lerato Molefe
Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28
Open
### 8 of 9 invitees have responded.
Each respondent is emailed a link, which gives 2026-10-28 as their last day to respond. The campaign closes by itself once that day has passed; to close it sooner, use Close campaign on its report.
#### Invitees by respondent group
Counted by respondent group. This page never lists who was invited or which of them responded.
| Respondent group
| Invited
| Responded
| Peer doctor
| 2
| 1
| Consultant
| 3
| 3
| Nurse
| 3
| 3
| Allied health professional
| 1
| 1
| All groups
| 9
| 8
#### Links
1 link was not delivered. Resend sends each of these respondents a new link; this page never says who they are.Resend 1 link
A link nothing was heard of for an hour counts as not delivered. NEW
View report Withdraw campaign…
### About this campaign
Trainee
Lerato Molefe, training year 4 DATA: TrainingYear
Template
Default MSF (Multi-source feedback)
Thresholds
5 responses; 3 in a respondent group; 2 respondent groups reporting DATA: Minimum…
Evidence
Evidence for PAED-010 and PAED-012 DATA: CoveredEpas
PAED-010 — Leading and operating within a clinical team
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams

######## BOARD R3-K7-not-delivered-390.dc.html — Campaign · a link not delivered, Resend in its row (3.43) · 390
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF campaign for Lerato Molefe
## MSF campaign for Lerato Molefe
Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28
Open
### 8 of 9 invitees have responded.
Each respondent is emailed a link, which gives 2026-10-28 as their last day to respond. The campaign closes by itself once that day has passed; to close it sooner, use Close campaign on its report.
#### Invitees by respondent group
Counted by respondent group. This page never lists who was invited or which of them responded.
| Respondent group
| Invited
| Responded
| Peer doctor
| 2
| 1
| Consultant
| 3
| 3
| Nurse
| 3
| 3
| Allied health professional
| 1
| 1
| All groups
| 9
| 8
#### Links
1 link was not delivered. Resend sends each of these respondents a new link; this page never says who they are.Resend 1 link
A link nothing was heard of for an hour counts as not delivered. NEW
View report Withdraw campaign…
### About this campaign
Trainee
Lerato Molefe, training year 4 DATA: TrainingYear
Template
Default MSF (Multi-source feedback)
Thresholds
5 responses; 3 in a respondent group; 2 respondent groups reporting DATA: Minimum…
Evidence
Evidence for PAED-010 and PAED-012 DATA: CoveredEpas
PAED-010 — Leading and operating within a clinical team
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams

######## BOARD R3-K8-resent-1280.dc.html — Campaign · resent, the result in the Links part, focused (3.43) · 1280
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF campaign for Lerato Molefe
## MSF campaign for Lerato Molefe
Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28
Open
### 8 of 9 invitees have responded.
Each respondent is emailed a link, which gives 2026-10-28 as their last day to respond. The campaign closes by itself once that day has passed; to close it sooner, use Close campaign on its report.
#### Invitees by respondent group
Counted by respondent group. This page never lists who was invited or which of them responded.
| Respondent group
| Invited
| Responded
| Peer doctor
| 2
| 1
| Consultant
| 3
| 3
| Nurse
| 3
| 3
| Allied health professional
| 1
| 1
| All groups
| 9
| 8
#### Links
1 new link is being sent.
1 link is still being sent. Reload this page to see whether it was delivered.
A link nothing was heard of for an hour counts as not delivered. NEW
View report Withdraw campaign…
### About this campaign
Trainee
Lerato Molefe, training year 4 DATA: TrainingYear
Template
Default MSF (Multi-source feedback)
Thresholds
5 responses; 3 in a respondent group; 2 respondent groups reporting DATA: Minimum…
Evidence
Evidence for PAED-010 and PAED-012 DATA: CoveredEpas
PAED-010 — Leading and operating within a clinical team
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams

######## BOARD R3-K8-resent-390.dc.html — Campaign · resent, the result in the Links part, focused (3.43) · 390
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF campaign for Lerato Molefe
## MSF campaign for Lerato Molefe
Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28
Open
### 8 of 9 invitees have responded.
Each respondent is emailed a link, which gives 2026-10-28 as their last day to respond. The campaign closes by itself once that day has passed; to close it sooner, use Close campaign on its report.
#### Invitees by respondent group
Counted by respondent group. This page never lists who was invited or which of them responded.
| Respondent group
| Invited
| Responded
| Peer doctor
| 2
| 1
| Consultant
| 3
| 3
| Nurse
| 3
| 3
| Allied health professional
| 1
| 1
| All groups
| 9
| 8
#### Links
1 new link is being sent.
1 link is still being sent. Reload this page to see whether it was delivered.
A link nothing was heard of for an hour counts as not delivered. NEW
View report Withdraw campaign…
### About this campaign
Trainee
Lerato Molefe, training year 4 DATA: TrainingYear
Template
Default MSF (Multi-source feedback)
Thresholds
5 responses; 3 in a respondent group; 2 respondent groups reporting DATA: Minimum…
Evidence
Evidence for PAED-010 and PAED-012 DATA: CoveredEpas
PAED-010 — Leading and operating within a clinical team
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams

######## BOARD R3-K9-under-review-1280.dc.html — Campaign · under review, the addresses gone (3.44) · 1280
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF campaign for Lerato Molefe
## MSF campaign for Lerato Molefe
Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28
Under review
### Closed to responses, and every respondent's email address has been removed.
Review the responses on its report, and release it to the trainee there. If it should never be released, withdraw it here.
#### Invitees by respondent group
Counted by respondent group. This page never lists who was invited or which of them responded.
| Respondent group
| Invited
| Responded
| Peer doctor
| 2
| 1
| Consultant
| 3
| 3
| Nurse
| 3
| 3
| Allied health professional
| 1
| 1
| All groups
| 9
| 8
Review and release Withdraw campaign…
### About this campaign
Trainee
Lerato Molefe, training year 4 DATA: TrainingYear
Template
Default MSF (Multi-source feedback)
Thresholds
5 responses; 3 in a respondent group; 2 respondent groups reporting DATA: Minimum…
Closed to responses
on 2026-10-14 DATA: ClosedOn
Evidence
Evidence for PAED-010 and PAED-012 DATA: CoveredEpas
PAED-010 — Leading and operating within a clinical team
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams

######## BOARD R3-K9-under-review-390.dc.html — Campaign · under review, the addresses gone (3.44) · 390
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF campaign for Lerato Molefe
## MSF campaign for Lerato Molefe
Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28
Under review
### Closed to responses, and every respondent's email address has been removed.
Review the responses on its report, and release it to the trainee there. If it should never be released, withdraw it here.
#### Invitees by respondent group
Counted by respondent group. This page never lists who was invited or which of them responded.
| Respondent group
| Invited
| Responded
| Peer doctor
| 2
| 1
| Consultant
| 3
| 3
| Nurse
| 3
| 3
| Allied health professional
| 1
| 1
| All groups
| 9
| 8
Review and release Withdraw campaign…
### About this campaign
Trainee
Lerato Molefe, training year 4 DATA: TrainingYear
Template
Default MSF (Multi-source feedback)
Thresholds
5 responses; 3 in a respondent group; 2 respondent groups reporting DATA: Minimum…
Closed to responses
on 2026-10-14 DATA: ClosedOn
Evidence
Evidence for PAED-010 and PAED-012 DATA: CoveredEpas
PAED-010 — Leading and operating within a clinical team
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams

######## BOARD R3-K10-withdraw-under-review-1280.dc.html — Campaign · Withdraw's dialog under review (R6a; WithdrawBody's under-review form) · 1280
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF campaign for Lerato Molefe
## MSF campaign for Lerato Molefe
Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28
Under review
### Closed to responses, and every respondent's email address has been removed.
Review the responses on its report, and release it to the trainee there. If it should never be released, withdraw it here.
#### Invitees by respondent group
Counted by respondent group. This page never lists who was invited or which of them responded.
| Respondent group
| Invited
| Responded
| Peer doctor
| 2
| 1
| Consultant
| 3
| 3
| Nurse
| 3
| 3
| Allied health professional
| 1
| 1
| All groups
| 9
| 8
Review and release Withdraw campaign…
### About this campaign
Trainee
Lerato Molefe, training year 4 DATA: TrainingYear
Template
Default MSF (Multi-source feedback)
Thresholds
5 responses; 3 in a respondent group; 2 respondent groups reporting DATA: Minimum…
Closed to responses
on 2026-10-14 DATA: ClosedOn
Evidence
Evidence for PAED-010 and PAED-012 DATA: CoveredEpas
PAED-010 — Leading and operating within a clinical team
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
[aria-describedby: Withdraw the campaign for Lerato Molefe (Default MSF, closing 2026-10-28)? It has closed, and its report has not been released. A withdrawn campaign is never released to the trainee, and withdrawing cannot be undone.]
### Withdraw this campaign?
Withdraw the campaign for Lerato Molefe (Default MSF, closing 2026-10-28)? It has closed, and its report has not been released. A withdrawn campaign is never released to the trainee, and withdrawing cannot be undone.
Keep the campaign Withdraw campaign

######## BOARD R3-K10-withdraw-under-review-390.dc.html — Campaign · Withdraw's dialog under review (R6a; WithdrawBody's under-review form) · 390
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF campaign for Lerato Molefe
## MSF campaign for Lerato Molefe
Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28
Under review
### Closed to responses, and every respondent's email address has been removed.
Review the responses on its report, and release it to the trainee there. If it should never be released, withdraw it here.
#### Invitees by respondent group
Counted by respondent group. This page never lists who was invited or which of them responded.
| Respondent group
| Invited
| Responded
| Peer doctor
| 2
| 1
| Consultant
| 3
| 3
| Nurse
| 3
| 3
| Allied health professional
| 1
| 1
| All groups
| 9
| 8
Review and release Withdraw campaign…
### About this campaign
Trainee
Lerato Molefe, training year 4 DATA: TrainingYear
Template
Default MSF (Multi-source feedback)
Thresholds
5 responses; 3 in a respondent group; 2 respondent groups reporting DATA: Minimum…
Closed to responses
on 2026-10-14 DATA: ClosedOn
Evidence
Evidence for PAED-010 and PAED-012 DATA: CoveredEpas
PAED-010 — Leading and operating within a clinical team
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
[aria-describedby: Withdraw the campaign for Lerato Molefe (Default MSF, closing 2026-10-28)? It has closed, and its report has not been released. A withdrawn campaign is never released to the trainee, and withdrawing cannot be undone.]
### Withdraw this campaign?
Withdraw the campaign for Lerato Molefe (Default MSF, closing 2026-10-28)? It has closed, and its report has not been released. A withdrawn campaign is never released to the trainee, and withdrawing cannot be undone.
Keep the campaign Withdraw campaign

######## BOARD R3-K11-released-1280.dc.html — Campaign · released (A.7.5 at 390) · 1280
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF campaign for Lerato Molefe
## MSF campaign for Lerato Molefe
Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28
Released
### The report has been released to the trainee.
#### Invitees by respondent group
Counted by respondent group. This page never lists who was invited or which of them responded.
| Respondent group
| Invited
| Responded
| Peer doctor
| 2
| 1
| Consultant
| 3
| 3
| Nurse
| 3
| 3
| Allied health professional
| 1
| 1
| All groups
| 9
| 8
View report
### About this campaign
Trainee
Lerato Molefe, training year 4 DATA: TrainingYear
Template
Default MSF (Multi-source feedback)
Thresholds
5 responses; 3 in a respondent group; 2 respondent groups reporting DATA: Minimum…
Closed to responses
on 2026-10-14 DATA: ClosedOn
Evidence
Evidence for PAED-010 and PAED-012 DATA: CoveredEpas
PAED-010 — Leading and operating within a clinical team
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams

######## BOARD R3-K11-released-390.dc.html — Campaign · released (A.7.5 at 390) · 390
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF campaign for Lerato Molefe
## MSF campaign for Lerato Molefe
Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28
Released
### The report has been released to the trainee.
#### Invitees by respondent group
Counted by respondent group. This page never lists who was invited or which of them responded.
| Respondent group
| Invited
| Responded
| Peer doctor
| 2
| 1
| Consultant
| 3
| 3
| Nurse
| 3
| 3
| Allied health professional
| 1
| 1
| All groups
| 9
| 8
View report
### About this campaign
Trainee
Lerato Molefe, training year 4 DATA: TrainingYear
Template
Default MSF (Multi-source feedback)
Thresholds
5 responses; 3 in a respondent group; 2 respondent groups reporting DATA: Minimum…
Closed to responses
on 2026-10-14 DATA: ClosedOn
Evidence
Evidence for PAED-010 and PAED-012 DATA: CoveredEpas
PAED-010 — Leading and operating within a clinical team
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams

######## BOARD R3-K12-withdrawn-draft-1280.dc.html — Campaign · Pieter du Plessis's, withdrawn as a draft (3.38) · 1280
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF campaign for Pieter du Plessis
## MSF campaign for Pieter du Plessis
Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28
Withdrawn
### Withdrawn. It takes no responses, any link sent no longer works, and every respondent's email address has been removed. It will never be released to the trainee.
Nobody was invited.
### About this campaign
Trainee
Pieter du Plessis, training year 2 DATA: TrainingYear
Template
Default MSF (Multi-source feedback)
Thresholds
8 responses; 3 in a respondent group; 2 respondent groups reporting DATA: Minimum…
Evidence
Evidence for PAED-015 DATA: CoveredEpas
PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research

######## BOARD R3-K12-withdrawn-draft-390.dc.html — Campaign · Pieter du Plessis's, withdrawn as a draft (3.38) · 390
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF campaign for Pieter du Plessis
## MSF campaign for Pieter du Plessis
Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28
Withdrawn
### Withdrawn. It takes no responses, any link sent no longer works, and every respondent's email address has been removed. It will never be released to the trainee.
Nobody was invited.
### About this campaign
Trainee
Pieter du Plessis, training year 2 DATA: TrainingYear
Template
Default MSF (Multi-source feedback)
Thresholds
8 responses; 3 in a respondent group; 2 respondent groups reporting DATA: Minimum…
Evidence
Evidence for PAED-015 DATA: CoveredEpas
PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research

######## BOARD R3-K13-not-current-1280.dc.html — Campaign · a draft whose trainee is no longer current (R4f, T270, Illustration) · 1280
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
Illustration A draft campaign for a registrar whose programme has since ended (‹date›). The address and the group are natively disabled; Add invitee and Open campaign are aria-disabled, each with its reason.
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF campaign for Pieter du Plessis
## MSF campaign for Pieter du Plessis
Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28
Draft
### Add invitees, then open the campaign to send anonymous response links.
Nobody has been invited yet.
Once the campaign opens, this page never lists who was invited or which of them responded.
[aria-describedby: Open campaign: Pieter du Plessis is no longer in a programme. Withdraw this campaign. NEW] [aria-disabled] Open campaign Withdraw campaign…
Open campaign: Pieter du Plessis is no longer in a programme. Withdraw this campaign. NEW
### Addresses invited
Listed only while the campaign is a draft, so that an address added by mistake can be removed. No invitee holds a working link until the campaign opens.
#### Add an invitee
* marks a field you must fill in.
Respondent email * [disabled]
Respondent group
[disabled]
[selected] Peer doctor
Consultant
Nurse
Allied health professional
Other
[aria-describedby: Add invitee: Pieter du Plessis is no longer in a programme, so nobody can be invited. NEW] [aria-disabled] Add invitee
Add invitee: Pieter du Plessis is no longer in a programme, so nobody can be invited. NEW
#### 0 addresses
No address yet.
### About this campaign
Trainee
Pieter du Plessis DATA: SubjectIsCurrent, SubjectEndedOn DATA: TrainingYear
Template
Default MSF (Multi-source feedback)
Thresholds
8 responses; 3 in a respondent group; 2 respondent groups reporting DATA: Minimum…
Evidence
Evidence for PAED-015 DATA: CoveredEpas
PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research

######## BOARD R3-K13-not-current-390.dc.html — Campaign · a draft whose trainee is no longer current (R4f, T270, Illustration) · 390
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
Illustration A draft campaign for a registrar whose programme has since ended (‹date›). The address and the group are natively disabled; Add invitee and Open campaign are aria-disabled, each with its reason.
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF campaign for Pieter du Plessis
## MSF campaign for Pieter du Plessis
Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28
Draft
### Add invitees, then open the campaign to send anonymous response links.
Nobody has been invited yet.
Once the campaign opens, this page never lists who was invited or which of them responded.
[aria-describedby: Open campaign: Pieter du Plessis is no longer in a programme. Withdraw this campaign. NEW] [aria-disabled] Open campaign Withdraw campaign…
Open campaign: Pieter du Plessis is no longer in a programme. Withdraw this campaign. NEW
### Addresses invited
Listed only while the campaign is a draft, so that an address added by mistake can be removed. No invitee holds a working link until the campaign opens.
#### Add an invitee
* marks a field you must fill in.
Respondent email * [disabled]
Respondent group
[disabled]
[selected] Peer doctor
Consultant
Nurse
Allied health professional
Other
[aria-describedby: Add invitee: Pieter du Plessis is no longer in a programme, so nobody can be invited. NEW] [aria-disabled] Add invitee
Add invitee: Pieter du Plessis is no longer in a programme, so nobody can be invited. NEW
#### 0 addresses
No address yet.
### About this campaign
Trainee
Pieter du Plessis DATA: SubjectIsCurrent, SubjectEndedOn DATA: TrainingYear
Template
Default MSF (Multi-source feedback)
Thresholds
8 responses; 3 in a respondent group; 2 respondent groups reporting DATA: Minimum…
Evidence
Evidence for PAED-015 DATA: CoveredEpas
PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research

######## BOARD R3-K14-programme-ended-1280.dc.html — Campaign · under review, the registrar's programme has ended (R4g, Q10, Illustration) · 1280
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
Illustration Lerato Molefe's programme ends while the campaign is under review.
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF campaign for Lerato Molefe
## MSF campaign for Lerato Molefe
Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28
Under review
### Closed to responses, and every respondent's email address has been removed.
Lerato Molefe's programme ended on ‹date›. A release still records the feedback as evidence on her record, but she can no longer open MSF reports to read it. NEW DATA: SubjectIsCurrent, SubjectEndedOn
Review the responses on its report, and release it to the trainee there. If it should never be released, withdraw it here.
#### Invitees by respondent group
Counted by respondent group. This page never lists who was invited or which of them responded.
| Respondent group
| Invited
| Responded
| Peer doctor
| 2
| 1
| Consultant
| 3
| 3
| Nurse
| 3
| 3
| Allied health professional
| 1
| 1
| All groups
| 9
| 8
Review and release Withdraw campaign…
### About this campaign
Trainee
Lerato Molefe, training year 4 DATA: TrainingYear
Template
Default MSF (Multi-source feedback)
Thresholds
5 responses; 3 in a respondent group; 2 respondent groups reporting DATA: Minimum…
Closed to responses
on 2026-10-14 DATA: ClosedOn
Evidence
Evidence for PAED-010 and PAED-012 DATA: CoveredEpas
PAED-010 — Leading and operating within a clinical team
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams

######## BOARD R3-K14-programme-ended-390.dc.html — Campaign · under review, the registrar's programme has ended (R4g, Q10, Illustration) · 390
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
Illustration Lerato Molefe's programme ends while the campaign is under review.
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF campaign for Lerato Molefe
## MSF campaign for Lerato Molefe
Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28
Under review
### Closed to responses, and every respondent's email address has been removed.
Lerato Molefe's programme ended on ‹date›. A release still records the feedback as evidence on her record, but she can no longer open MSF reports to read it. NEW DATA: SubjectIsCurrent, SubjectEndedOn
Review the responses on its report, and release it to the trainee there. If it should never be released, withdraw it here.
#### Invitees by respondent group
Counted by respondent group. This page never lists who was invited or which of them responded.
| Respondent group
| Invited
| Responded
| Peer doctor
| 2
| 1
| Consultant
| 3
| 3
| Nurse
| 3
| 3
| Allied health professional
| 1
| 1
| All groups
| 9
| 8
Review and release Withdraw campaign…
### About this campaign
Trainee
Lerato Molefe, training year 4 DATA: TrainingYear
Template
Default MSF (Multi-source feedback)
Thresholds
5 responses; 3 in a respondent group; 2 respondent groups reporting DATA: Minimum…
Closed to responses
on 2026-10-14 DATA: ClosedOn
Evidence
Evidence for PAED-010 and PAED-012 DATA: CoveredEpas
PAED-010 — Leading and operating within a clinical team
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams

######## BOARD R3-K15-learner-draft-1280.dc.html — Campaign · learner feedback's draft: one group, a teaching context per invitee (R4h, M4, Illustration) · 1280
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
Illustration Learner feedback (only PAED-015's tools name it, D35). The counts table shows the one group, never the contexts.
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF campaign for Lerato Molefe
## MSF campaign for Lerato Molefe
‹template› · Learner feedback · ‹opens› to ‹closes›
Draft
### Add invitees, then open the campaign to send anonymous response links.
Counted by respondent group. Once the campaign opens, this page never lists who was invited or which of them responded.
| Respondent group
| Invited
| Learner
| ‹n›
| All groups
| ‹n›
Open campaign Withdraw campaign…
Opening emails each of the ‹n› invitees a link to respond, with ‹closes› as their last day.
### Addresses invited
Listed only while the campaign is a draft, so that an address added by mistake can be removed. No invitee holds a working link until the campaign opens.
#### Add an invitee
* marks a field you must fill in.
Respondent email *
Teaching context *
Add invitee
Every invitee is in one respondent group, Learner. The context is named only on the coordinator's report.
#### ‹n› addresses
‹learner 1's address›Learner · Teaching context: ‹context›[aria-label: Remove ‹learner 1's address› (Learner)] Remove…
‹learner 2's address›Learner · Teaching context: ‹context›[aria-label: Remove ‹learner 2's address› (Learner)] Remove…
‹learner 3's address›Learner · Teaching context: ‹context›[aria-label: Remove ‹learner 3's address› (Learner)] Remove…
### About this campaign
Trainee
Lerato Molefe, training year 4 DATA: TrainingYear
Template
‹template› (Learner feedback)
Thresholds
‹n› responses; ‹n› in a respondent group; 1 respondent group reporting DATA: Minimum…Learner feedback has one respondent group, so its groups-reporting minimum is fixed at 1.
Evidence
Evidence for PAED-015 DATA: CoveredEpas
PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research

######## BOARD R3-K15-learner-draft-390.dc.html — Campaign · learner feedback's draft: one group, a teaching context per invitee (R4h, M4, Illustration) · 390
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
Illustration Learner feedback (only PAED-015's tools name it, D35). The counts table shows the one group, never the contexts.
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF campaign for Lerato Molefe
## MSF campaign for Lerato Molefe
‹template› · Learner feedback · ‹opens› to ‹closes›
Draft
### Add invitees, then open the campaign to send anonymous response links.
Counted by respondent group. Once the campaign opens, this page never lists who was invited or which of them responded.
| Respondent group
| Invited
| Learner
| ‹n›
| All groups
| ‹n›
Open campaign Withdraw campaign…
Opening emails each of the ‹n› invitees a link to respond, with ‹closes› as their last day.
### Addresses invited
Listed only while the campaign is a draft, so that an address added by mistake can be removed. No invitee holds a working link until the campaign opens.
#### Add an invitee
* marks a field you must fill in.
Respondent email *
Teaching context *
Add invitee
Every invitee is in one respondent group, Learner. The context is named only on the coordinator's report.
#### ‹n› addresses
‹learner 1's address›Learner · Teaching context: ‹context›[aria-label: Remove ‹learner 1's address› (Learner)] Remove…
‹learner 2's address›Learner · Teaching context: ‹context›[aria-label: Remove ‹learner 2's address› (Learner)] Remove…
‹learner 3's address›Learner · Teaching context: ‹context›[aria-label: Remove ‹learner 3's address› (Learner)] Remove…
### About this campaign
Trainee
Lerato Molefe, training year 4 DATA: TrainingYear
Template
‹template› (Learner feedback)
Thresholds
‹n› responses; ‹n› in a respondent group; 1 respondent group reporting DATA: Minimum…Learner feedback has one respondent group, so its groups-reporting minimum is fixed at 1.
Evidence
Evidence for PAED-015 DATA: CoveredEpas
PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research

######## BOARD R3-K16-unavailable-1280.dc.html — Campaign · unavailable · 1280
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
Campaign unavailable
## Campaign unavailable
The selected campaign could not be loaded.
Go to MSF campaigns NEW

######## BOARD R3-K16-unavailable-390.dc.html — Campaign · unavailable · 390
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
Campaign unavailable
## Campaign unavailable
The selected campaign could not be loaded.
Go to MSF campaigns NEW

######## BOARD R3-K17-loading-1280.dc.html — Campaign · loading: the header at once, no action until the read is in · 1280
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF campaign
## MSF campaign
Loading the campaign.

######## BOARD R3-K17-loading-390.dc.html — Campaign · loading: the header at once, no action until the read is in · 390
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF campaign
## MSF campaign
Loading the campaign.

######## BOARD R3-K18-error-1280.dc.html — Campaign · load error · 1280
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF campaign
## MSF campaign
Could not load this campaign. Nothing has changed. Try again, or come back in a few minutes.Try again

######## BOARD R3-K18-error-390.dc.html — Campaign · load error · 390
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF campaign
## MSF campaign
Could not load this campaign. Nothing has changed. Try again, or come back in a few minutes.Try again

######## BOARD R3-K20-refused-open-1280.dc.html — Campaign · a refused Open: the danger Alert beside the button, the focus left on it (S8) · 1280
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF campaign for Lerato Molefe
## MSF campaign for Lerato Molefe
Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28
Draft
### Add invitees, then open the campaign to send anonymous response links.
Counted by respondent group. Once the campaign opens, this page never lists who was invited or which of them responded.
| Respondent group
| Invited
| Peer doctor
| 2
| Consultant
| 3
| Nurse
| 3
| Allied health professional
| 1
| All groups
| 9
Not opened. ‹The refusal in its own words, as built.›
Open campaign Withdraw campaign…
### Addresses invited
Listed only while the campaign is a draft, so that an address added by mistake can be removed. No invitee holds a working link until the campaign opens.
#### Add an invitee
* marks a field you must fill in.
Respondent email *
Respondent group
[selected] Peer doctor
Consultant
Nurse
Allied health professional
Other
Add invitee
#### 9 addresses
grace.mokwena@kgk.wombat.localNurse[aria-label: Remove grace.mokwena@kgk.wombat.local (Nurse)] Remove…
kagiso.motsepe@kgk.wombat.localPeer doctor[aria-label: Remove kagiso.motsepe@kgk.wombat.local (Peer doctor)] Remove…
lindiwe.khoza@kgk.wombat.localPeer doctor[aria-label: Remove lindiwe.khoza@kgk.wombat.local (Peer doctor)] Remove…
khumalo@kgk.wombat.localConsultant[aria-label: Remove khumalo@kgk.wombat.local (Consultant)] Remove…
botha@kgk.wombat.localConsultant[aria-label: Remove botha@kgk.wombat.local (Consultant)] Remove…
naledi.sebego@kgk.wombat.localAllied health professional[aria-label: Remove naledi.sebego@kgk.wombat.local (Allied health professional)] Remove…
‹the third consultant's address›Consultant[aria-label: Remove ‹the third consultant's address› (Consultant)] Remove…
‹the third nurse's address›Nurse[aria-label: Remove ‹the third nurse's address› (Nurse)] Remove…
palesa.tau@kgk.wombat.localNurse[aria-label: Remove palesa.tau@kgk.wombat.local (Nurse)] Remove…
### About this campaign
Trainee
Lerato Molefe, training year 4 DATA: TrainingYear
Template
Default MSF (Multi-source feedback)
Thresholds
5 responses; 3 in a respondent group; 2 respondent groups reporting DATA: Minimum…
Evidence
Evidence for PAED-010 and PAED-012 DATA: CoveredEpas
PAED-010 — Leading and operating within a clinical team
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams

######## BOARD R3-K20-refused-open-390.dc.html — Campaign · a refused Open: the danger Alert beside the button, the focus left on it (S8) · 390
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF campaign for Lerato Molefe
## MSF campaign for Lerato Molefe
Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28
Draft
### Add invitees, then open the campaign to send anonymous response links.
Counted by respondent group. Once the campaign opens, this page never lists who was invited or which of them responded.
| Respondent group
| Invited
| Peer doctor
| 2
| Consultant
| 3
| Nurse
| 3
| Allied health professional
| 1
| All groups
| 9
Not opened. ‹The refusal in its own words, as built.›
Open campaign Withdraw campaign…
### Addresses invited
Listed only while the campaign is a draft, so that an address added by mistake can be removed. No invitee holds a working link until the campaign opens.
#### Add an invitee
* marks a field you must fill in.
Respondent email *
Respondent group
[selected] Peer doctor
Consultant
Nurse
Allied health professional
Other
Add invitee
#### 9 addresses
grace.mokwena@kgk.wombat.localNurse[aria-label: Remove grace.mokwena@kgk.wombat.local (Nurse)] Remove…
kagiso.motsepe@kgk.wombat.localPeer doctor[aria-label: Remove kagiso.motsepe@kgk.wombat.local (Peer doctor)] Remove…
lindiwe.khoza@kgk.wombat.localPeer doctor[aria-label: Remove lindiwe.khoza@kgk.wombat.local (Peer doctor)] Remove…
khumalo@kgk.wombat.localConsultant[aria-label: Remove khumalo@kgk.wombat.local (Consultant)] Remove…
botha@kgk.wombat.localConsultant[aria-label: Remove botha@kgk.wombat.local (Consultant)] Remove…
naledi.sebego@kgk.wombat.localAllied health professional[aria-label: Remove naledi.sebego@kgk.wombat.local (Allied health professional)] Remove…
‹the third consultant's address›Consultant[aria-label: Remove ‹the third consultant's address› (Consultant)] Remove…
‹the third nurse's address›Nurse[aria-label: Remove ‹the third nurse's address› (Nurse)] Remove…
palesa.tau@kgk.wombat.localNurse[aria-label: Remove palesa.tau@kgk.wombat.local (Nurse)] Remove…
### About this campaign
Trainee
Lerato Molefe, training year 4 DATA: TrainingYear
Template
Default MSF (Multi-source feedback)
Thresholds
5 responses; 3 in a respondent group; 2 respondent groups reporting DATA: Minimum…
Evidence
Evidence for PAED-010 and PAED-012 DATA: CoveredEpas
PAED-010 — Leading and operating within a clinical team
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams

######## BOARD R3-K21-refused-withdraw-1280.dc.html — Campaign · a refused Withdraw (after its dialog): the danger Alert, the focus back on Withdraw campaign… (S8) · 1280
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF campaign for Lerato Molefe
## MSF campaign for Lerato Molefe
Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28
Under review
### Closed to responses, and every respondent's email address has been removed.
Review the responses on its report, and release it to the trainee there. If it should never be released, withdraw it here.
#### Invitees by respondent group
Counted by respondent group. This page never lists who was invited or which of them responded.
| Respondent group
| Invited
| Responded
| Peer doctor
| 2
| 1
| Consultant
| 3
| 3
| Nurse
| 3
| 3
| Allied health professional
| 1
| 1
| All groups
| 9
| 8
Review and release Withdraw campaign…
Not withdrawn. ‹The refusal in its own words, as built.›
### About this campaign
Trainee
Lerato Molefe, training year 4 DATA: TrainingYear
Template
Default MSF (Multi-source feedback)
Thresholds
5 responses; 3 in a respondent group; 2 respondent groups reporting DATA: Minimum…
Closed to responses
on 2026-10-14 DATA: ClosedOn
Evidence
Evidence for PAED-010 and PAED-012 DATA: CoveredEpas
PAED-010 — Leading and operating within a clinical team
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams

######## BOARD R3-K21-refused-withdraw-390.dc.html — Campaign · a refused Withdraw (after its dialog): the danger Alert, the focus back on Withdraw campaign… (S8) · 390
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF campaign for Lerato Molefe
## MSF campaign for Lerato Molefe
Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28
Under review
### Closed to responses, and every respondent's email address has been removed.
Review the responses on its report, and release it to the trainee there. If it should never be released, withdraw it here.
#### Invitees by respondent group
Counted by respondent group. This page never lists who was invited or which of them responded.
| Respondent group
| Invited
| Responded
| Peer doctor
| 2
| 1
| Consultant
| 3
| 3
| Nurse
| 3
| 3
| Allied health professional
| 1
| 1
| All groups
| 9
| 8
Review and release Withdraw campaign…
Not withdrawn. ‹The refusal in its own words, as built.›
### About this campaign
Trainee
Lerato Molefe, training year 4 DATA: TrainingYear
Template
Default MSF (Multi-source feedback)
Thresholds
5 responses; 3 in a respondent group; 2 respondent groups reporting DATA: Minimum…
Closed to responses
on 2026-10-14 DATA: ClosedOn
Evidence
Evidence for PAED-010 and PAED-012 DATA: CoveredEpas
PAED-010 — Leading and operating within a clinical team
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams

######## BOARD R3-K22-refused-resend-1280.dc.html — Campaign · a refused Resend: the danger Alert in the Links part, the focus left on Resend (S8) · 1280
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF campaign for Lerato Molefe
## MSF campaign for Lerato Molefe
Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28
Open
### 8 of 9 invitees have responded.
Each respondent is emailed a link, which gives 2026-10-28 as their last day to respond. The campaign closes by itself once that day has passed; to close it sooner, use Close campaign on its report.
#### Invitees by respondent group
Counted by respondent group. This page never lists who was invited or which of them responded.
| Respondent group
| Invited
| Responded
| Peer doctor
| 2
| 1
| Consultant
| 3
| 3
| Nurse
| 3
| 3
| Allied health professional
| 1
| 1
| All groups
| 9
| 8
#### Links
1 link was not delivered. Resend sends each of these respondents a new link; this page never says who they are.Resend 1 link
Not resent. ‹The refusal in its own words, as built.›
A link nothing was heard of for an hour counts as not delivered. NEW
View report Withdraw campaign…
### About this campaign
Trainee
Lerato Molefe, training year 4 DATA: TrainingYear
Template
Default MSF (Multi-source feedback)
Thresholds
5 responses; 3 in a respondent group; 2 respondent groups reporting DATA: Minimum…
Evidence
Evidence for PAED-010 and PAED-012 DATA: CoveredEpas
PAED-010 — Leading and operating within a clinical team
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams

######## BOARD R3-K22-refused-resend-390.dc.html — Campaign · a refused Resend: the danger Alert in the Links part, the focus left on Resend (S8) · 390
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF campaign for Lerato Molefe
## MSF campaign for Lerato Molefe
Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28
Open
### 8 of 9 invitees have responded.
Each respondent is emailed a link, which gives 2026-10-28 as their last day to respond. The campaign closes by itself once that day has passed; to close it sooner, use Close campaign on its report.
#### Invitees by respondent group
Counted by respondent group. This page never lists who was invited or which of them responded.
| Respondent group
| Invited
| Responded
| Peer doctor
| 2
| 1
| Consultant
| 3
| 3
| Nurse
| 3
| 3
| Allied health professional
| 1
| 1
| All groups
| 9
| 8
#### Links
1 link was not delivered. Resend sends each of these respondents a new link; this page never says who they are.Resend 1 link
Not resent. ‹The refusal in its own words, as built.›
A link nothing was heard of for an hour counts as not delivered. NEW
View report Withdraw campaign…
### About this campaign
Trainee
Lerato Molefe, training year 4 DATA: TrainingYear
Template
Default MSF (Multi-source feedback)
Thresholds
5 responses; 3 in a respondent group; 2 respondent groups reporting DATA: Minimum…
Evidence
Evidence for PAED-010 and PAED-012 DATA: CoveredEpas
PAED-010 — Leading and operating within a clinical team
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams

######## BOARD R3-K19-fifteen-390.dc.html — Campaign · fifteen EPAs in About (S8, R10, Illustration) · 390
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
Illustration A campaign that is evidence for all fifteen EPAs.
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF campaign for Lerato Molefe
## MSF campaign for Lerato Molefe
Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28
Draft
### Add invitees, then open the campaign to send anonymous response links.
Counted by respondent group. Once the campaign opens, this page never lists who was invited or which of them responded.
| Respondent group
| Invited
| Peer doctor
| 2
| Consultant
| 3
| Nurse
| 3
| Allied health professional
| 1
| All groups
| 9
Open campaign Withdraw campaign…
Opening emails each of the 9 invitees a link to respond, with 2026-10-28 as their last day.
### About this campaign
Trainee
Lerato Molefe, training year 4 DATA: TrainingYear
Template
Default MSF (Multi-source feedback)
Thresholds
5 responses; 3 in a respondent group; 2 respondent groups reporting DATA: Minimum…
Evidence
Evidence for 15 EPAs DATA: CoveredEpas
PAED-001 — Providing paediatric emergency care to children
PAED-002 — Managing common paediatric presentations
PAED-003 — Providing intensive care to children
PAED-004 — Managing common neonatal conditions
PAED-005 — Providing neonatal care in intensive and high-care settings
PAED-006 — Managing long-term health conditions (LTHCs)
PAED-007 — Maintaining and promoting the health and well-being of children
PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
PAED-009 — Providing care for an adolescent patient
PAED-010 — Leading and operating within a clinical team
PAED-011 — Managing population health challenges
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
PAED-013 — Managing child protection, legal and ethical aspects of children at risk
PAED-014 — Providing palliative and end-of-life care
PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research

######## BOARD R3-R1-open-1280.dc.html — Report · open: the status card and the checklist only (3.41, F1) · 1280
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF report for Lerato Molefe
## MSF report for Lerato Molefe
Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28
Open its campaign page
Open
### Still taking responses, until 2026-10-28.
The responses are read once the campaign has closed and every address has been removed. F1
3 responses, of the 5 needed.
0 respondent groups reporting, of the 2 needed.
A respondent group reports once 3 of it have responded; a group with fewer is suppressed.
#### Evidence for
PAED-010 — Leading and operating within a clinical team
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
One evidence record per EPA is written to the trainee's portfolio when the report is released. DATA: will be recorded
Close campaign…
Release is offered once the campaign has closed. It closes by itself once 2026-10-28 has passed; close it sooner here.

######## BOARD R3-R1-open-768.dc.html — Report · open: the status card and the checklist only (3.41, F1) · 768
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF report for Lerato Molefe
## MSF report for Lerato Molefe
Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28
Open its campaign page
Open
### Still taking responses, until 2026-10-28.
The responses are read once the campaign has closed and every address has been removed. F1
3 responses, of the 5 needed.
0 respondent groups reporting, of the 2 needed.
A respondent group reports once 3 of it have responded; a group with fewer is suppressed.
#### Evidence for
PAED-010 — Leading and operating within a clinical team
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
One evidence record per EPA is written to the trainee's portfolio when the report is released. DATA: will be recorded
Close campaign…
Release is offered once the campaign has closed. It closes by itself once 2026-10-28 has passed; close it sooner here.

######## BOARD R3-R1-open-390.dc.html — Report · open: the status card and the checklist only (3.41, F1) · 390
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF report for Lerato Molefe
## MSF report for Lerato Molefe
Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28
Open its campaign page
Open
### Still taking responses, until 2026-10-28.
The responses are read once the campaign has closed and every address has been removed. F1
3 responses, of the 5 needed.
0 respondent groups reporting, of the 2 needed.
A respondent group reports once 3 of it have responded; a group with fewer is suppressed.
#### Evidence for
PAED-010 — Leading and operating within a clinical team
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
One evidence record per EPA is written to the trainee's portfolio when the report is released. DATA: will be recorded
Close campaign…
Release is offered once the campaign has closed. It closes by itself once 2026-10-28 has passed; close it sooner here.

######## BOARD R3-R2-close-dialog-1280.dc.html — Report · Close's dialog over 3.44's open page, focus on Keep it open (3.44) · 1280
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
The backdrop is 3.44's page before the close: Open, so (F1) the checklist and no group cards.
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF report for Lerato Molefe
## MSF report for Lerato Molefe
Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28
Open its campaign page
Open
### Still taking responses, until 2026-10-28.
The responses are read once the campaign has closed and every address has been removed. F1
8 responses, of the 5 needed.
2 respondent groups reporting, of the 2 needed.
A respondent group reports once 3 of it have responded; a group with fewer is suppressed.
#### Evidence for
PAED-010 — Leading and operating within a clinical team
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
One evidence record per EPA is written to the trainee's portfolio when the report is released. DATA: will be recorded
Close campaign…
Release is offered once the campaign has closed. It closes by itself once 2026-10-28 has passed; close it sooner here.
[aria-describedby: Close the campaign for Lerato Molefe (Default MSF, closing 2026-10-28) before its last day? It takes no more responses: the 1 link not yet used stops working. DATA: InvitedTotal less the responses Every respondent's email address is removed, so no link can be resent. The report keeps the 8 responses it has, for you to review and release. Closing cannot be undone.]
### Close this campaign now?
Close the campaign for Lerato Molefe (Default MSF, closing 2026-10-28) before its last day?
It takes no more responses: the 1 link not yet used stops working. DATA: InvitedTotal less the responses
Every respondent's email address is removed, so no link can be resent.
The report keeps the 8 responses it has, for you to review and release.
Closing cannot be undone.
Keep it open Close campaign

######## BOARD R3-R2-close-dialog-390.dc.html — Report · Close's dialog over 3.44's open page, focus on Keep it open (3.44) · 390
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
The backdrop is 3.44's page before the close: Open, so (F1) the checklist and no group cards.
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF report for Lerato Molefe
## MSF report for Lerato Molefe
Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28
Open its campaign page
Open
### Still taking responses, until 2026-10-28.
The responses are read once the campaign has closed and every address has been removed. F1
8 responses, of the 5 needed.
2 respondent groups reporting, of the 2 needed.
A respondent group reports once 3 of it have responded; a group with fewer is suppressed.
#### Evidence for
PAED-010 — Leading and operating within a clinical team
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
One evidence record per EPA is written to the trainee's portfolio when the report is released. DATA: will be recorded
Close campaign…
Release is offered once the campaign has closed. It closes by itself once 2026-10-28 has passed; close it sooner here.
[aria-describedby: Close the campaign for Lerato Molefe (Default MSF, closing 2026-10-28) before its last day? It takes no more responses: the 1 link not yet used stops working. DATA: InvitedTotal less the responses Every respondent's email address is removed, so no link can be resent. The report keeps the 8 responses it has, for you to review and release. Closing cannot be undone.]
### Close this campaign now?
Close the campaign for Lerato Molefe (Default MSF, closing 2026-10-28) before its last day?
It takes no more responses: the 1 link not yet used stops working. DATA: InvitedTotal less the responses
Every respondent's email address is removed, so no link can be resent.
The report keeps the 8 responses it has, for you to review and release.
Closing cannot be undone.
Keep it open Close campaign

######## BOARD R3-R3-under-review-1280.dc.html — Report · just closed, Close's result focused; under review, ready (3.44) · 1280
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF report for Lerato Molefe
## MSF report for Lerato Molefe
Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28
Open its campaign page
Campaign closed and anonymised for review. Every respondent's email address has been removed.
Under review
### Ready to release: every threshold is met.
8 responses, of the 5 needed.
2 respondent groups reporting, of the 2 needed.
A respondent group reports once 3 of it have responded; a group with fewer is suppressed.
#### Evidence for
PAED-010 — Leading and operating within a clinical team
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
One evidence record per EPA is written to the trainee's portfolio when the report is released. DATA: will be recorded
### By respondent group
As Lerato Molefe will read it once released. Ratings are averaged from 1, Well below expectations, to 5, Well above expectations.
#### Consultant · ‹n.nn› average rating, from 3 responses
What should the trainee keep doing or improve?
Sound judgement in clinic; teaches the interns well
Ready to run a ward; should document escalation decisions more fully
‹the third consultant's comment›
#### Nurse · ‹n.nn› average rating, from 3 responses
What should the trainee keep doing or improve?
Calm and kind with parents at night; hands over clearly
Leads neonatal resuscitations and explains her decisions to the nurses
‹the third nurse's comment›
Suppressed: Peer doctor, Allied health professional. Fewer than 3 responses each, so their ratings and comments are not shown here. Lerato Molefe sees nothing of them, not even that they answered.
### Release to Lerato Molefe
When you release:
Lerato Molefe can read your narrative and the groups shown above on MSF reports.
2 evidence records are written to Lerato Molefe's portfolio, for PAED-010 and PAED-012.
Lerato Molefe is sent an email saying a report has been released, with a link to MSF reports. decided, T367
It cannot be undone.
Narrative
[aria-describedby: 0 of 4,000 characters NEW: counter]
0 of 4,000 characters NEW: counter
Supervision level this feedback supports
[aria-describedby: Optional, and blank is a perfectly good answer. Your own judgement as the releasing clinician, informed by the aggregate above — never an average of respondent scores.]
Not stated
1
2
3a
3b
4
5
Optional, and blank is a perfectly good answer. Your own judgement as the releasing clinician, informed by the aggregate above — never an average of respondent scores.
Release to trainee…
If it should never be released, withdraw it on its campaign page.

######## BOARD R3-R3-under-review-768.dc.html — Report · just closed, Close's result focused; under review, ready (3.44) · 768
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF report for Lerato Molefe
## MSF report for Lerato Molefe
Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28
Open its campaign page
Campaign closed and anonymised for review. Every respondent's email address has been removed.
Under review
### Ready to release: every threshold is met.
8 responses, of the 5 needed.
2 respondent groups reporting, of the 2 needed.
A respondent group reports once 3 of it have responded; a group with fewer is suppressed.
#### Evidence for
PAED-010 — Leading and operating within a clinical team
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
One evidence record per EPA is written to the trainee's portfolio when the report is released. DATA: will be recorded
### By respondent group
As Lerato Molefe will read it once released. Ratings are averaged from 1, Well below expectations, to 5, Well above expectations.
#### Consultant · ‹n.nn› average rating, from 3 responses
What should the trainee keep doing or improve?
Sound judgement in clinic; teaches the interns well
Ready to run a ward; should document escalation decisions more fully
‹the third consultant's comment›
#### Nurse · ‹n.nn› average rating, from 3 responses
What should the trainee keep doing or improve?
Calm and kind with parents at night; hands over clearly
Leads neonatal resuscitations and explains her decisions to the nurses
‹the third nurse's comment›
Suppressed: Peer doctor, Allied health professional. Fewer than 3 responses each, so their ratings and comments are not shown here. Lerato Molefe sees nothing of them, not even that they answered.
### Release to Lerato Molefe
When you release:
Lerato Molefe can read your narrative and the groups shown above on MSF reports.
2 evidence records are written to Lerato Molefe's portfolio, for PAED-010 and PAED-012.
Lerato Molefe is sent an email saying a report has been released, with a link to MSF reports. decided, T367
It cannot be undone.
Narrative
[aria-describedby: 0 of 4,000 characters NEW: counter]
0 of 4,000 characters NEW: counter
Supervision level this feedback supports
[aria-describedby: Optional, and blank is a perfectly good answer. Your own judgement as the releasing clinician, informed by the aggregate above — never an average of respondent scores.]
Not stated
1
2
3a
3b
4
5
Optional, and blank is a perfectly good answer. Your own judgement as the releasing clinician, informed by the aggregate above — never an average of respondent scores.
Release to trainee…
If it should never be released, withdraw it on its campaign page.

######## BOARD R3-R3-under-review-390.dc.html — Report · just closed, Close's result focused; under review, ready (3.44) · 390
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF report for Lerato Molefe
## MSF report for Lerato Molefe
Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28
Open its campaign page
Campaign closed and anonymised for review. Every respondent's email address has been removed.
Under review
### Ready to release: every threshold is met.
8 responses, of the 5 needed.
2 respondent groups reporting, of the 2 needed.
A respondent group reports once 3 of it have responded; a group with fewer is suppressed.
#### Evidence for
PAED-010 — Leading and operating within a clinical team
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
One evidence record per EPA is written to the trainee's portfolio when the report is released. DATA: will be recorded
### By respondent group
As Lerato Molefe will read it once released. Ratings are averaged from 1, Well below expectations, to 5, Well above expectations.
#### Consultant · ‹n.nn› average rating, from 3 responses
What should the trainee keep doing or improve?
Sound judgement in clinic; teaches the interns well
Ready to run a ward; should document escalation decisions more fully
‹the third consultant's comment›
#### Nurse · ‹n.nn› average rating, from 3 responses
What should the trainee keep doing or improve?
Calm and kind with parents at night; hands over clearly
Leads neonatal resuscitations and explains her decisions to the nurses
‹the third nurse's comment›
Suppressed: Peer doctor, Allied health professional. Fewer than 3 responses each, so their ratings and comments are not shown here. Lerato Molefe sees nothing of them, not even that they answered.
### Release to Lerato Molefe
When you release:
Lerato Molefe can read your narrative and the groups shown above on MSF reports.
2 evidence records are written to Lerato Molefe's portfolio, for PAED-010 and PAED-012.
Lerato Molefe is sent an email saying a report has been released, with a link to MSF reports. decided, T367
It cannot be undone.
Narrative
[aria-describedby: 0 of 4,000 characters NEW: counter]
0 of 4,000 characters NEW: counter
Supervision level this feedback supports
[aria-describedby: Optional, and blank is a perfectly good answer. Your own judgement as the releasing clinician, informed by the aggregate above — never an average of respondent scores.]
Not stated
1
2
3a
3b
4
5
Optional, and blank is a perfectly good answer. Your own judgement as the releasing clinician, informed by the aggregate above — never an average of respondent scores.
Release to trainee…
If it should never be released, withdraw it on its campaign page.

######## BOARD R3-R4-closed-by-job-1280.dc.html — Report · closed by the hourly job, not by hand (R5f, F5, Illustration) · 1280
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
Illustration ClosedByUserId is null: the hourly job closed it. No result: nothing was pressed.
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF report for Lerato Molefe
## MSF report for Lerato Molefe
Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28
Open its campaign page
Under review
### Ready to release: every threshold is met.
Closed by itself on ‹date›, once 2026-10-28 had passed. DATA: ClosedByUserId null
8 responses, of the 5 needed.
2 respondent groups reporting, of the 2 needed.
A respondent group reports once 3 of it have responded; a group with fewer is suppressed.
#### Evidence for
PAED-010 — Leading and operating within a clinical team
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
One evidence record per EPA is written to the trainee's portfolio when the report is released. DATA: will be recorded
### By respondent group
As Lerato Molefe will read it once released. Ratings are averaged from 1, Well below expectations, to 5, Well above expectations.
#### Consultant · ‹n.nn› average rating, from 3 responses
What should the trainee keep doing or improve?
Sound judgement in clinic; teaches the interns well
Ready to run a ward; should document escalation decisions more fully
‹the third consultant's comment›
#### Nurse · ‹n.nn› average rating, from 3 responses
What should the trainee keep doing or improve?
Calm and kind with parents at night; hands over clearly
Leads neonatal resuscitations and explains her decisions to the nurses
‹the third nurse's comment›
Suppressed: Peer doctor, Allied health professional. Fewer than 3 responses each, so their ratings and comments are not shown here. Lerato Molefe sees nothing of them, not even that they answered.
### Release to Lerato Molefe
When you release:
Lerato Molefe can read your narrative and the groups shown above on MSF reports.
2 evidence records are written to Lerato Molefe's portfolio, for PAED-010 and PAED-012.
Lerato Molefe is sent an email saying a report has been released, with a link to MSF reports. decided, T367
It cannot be undone.
Narrative
[aria-describedby: 0 of 4,000 characters NEW: counter]
0 of 4,000 characters NEW: counter
Supervision level this feedback supports
[aria-describedby: Optional, and blank is a perfectly good answer. Your own judgement as the releasing clinician, informed by the aggregate above — never an average of respondent scores.]
Not stated
1
2
3a
3b
4
5
Optional, and blank is a perfectly good answer. Your own judgement as the releasing clinician, informed by the aggregate above — never an average of respondent scores.
Release to trainee…
If it should never be released, withdraw it on its campaign page.

######## BOARD R3-R4-closed-by-job-390.dc.html — Report · closed by the hourly job, not by hand (R5f, F5, Illustration) · 390
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
Illustration ClosedByUserId is null: the hourly job closed it. No result: nothing was pressed.
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF report for Lerato Molefe
## MSF report for Lerato Molefe
Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28
Open its campaign page
Under review
### Ready to release: every threshold is met.
Closed by itself on ‹date›, once 2026-10-28 had passed. DATA: ClosedByUserId null
8 responses, of the 5 needed.
2 respondent groups reporting, of the 2 needed.
A respondent group reports once 3 of it have responded; a group with fewer is suppressed.
#### Evidence for
PAED-010 — Leading and operating within a clinical team
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
One evidence record per EPA is written to the trainee's portfolio when the report is released. DATA: will be recorded
### By respondent group
As Lerato Molefe will read it once released. Ratings are averaged from 1, Well below expectations, to 5, Well above expectations.
#### Consultant · ‹n.nn› average rating, from 3 responses
What should the trainee keep doing or improve?
Sound judgement in clinic; teaches the interns well
Ready to run a ward; should document escalation decisions more fully
‹the third consultant's comment›
#### Nurse · ‹n.nn› average rating, from 3 responses
What should the trainee keep doing or improve?
Calm and kind with parents at night; hands over clearly
Leads neonatal resuscitations and explains her decisions to the nurses
‹the third nurse's comment›
Suppressed: Peer doctor, Allied health professional. Fewer than 3 responses each, so their ratings and comments are not shown here. Lerato Molefe sees nothing of them, not even that they answered.
### Release to Lerato Molefe
When you release:
Lerato Molefe can read your narrative and the groups shown above on MSF reports.
2 evidence records are written to Lerato Molefe's portfolio, for PAED-010 and PAED-012.
Lerato Molefe is sent an email saying a report has been released, with a link to MSF reports. decided, T367
It cannot be undone.
Narrative
[aria-describedby: 0 of 4,000 characters NEW: counter]
0 of 4,000 characters NEW: counter
Supervision level this feedback supports
[aria-describedby: Optional, and blank is a perfectly good answer. Your own judgement as the releasing clinician, informed by the aggregate above — never an average of respondent scores.]
Not stated
1
2
3a
3b
4
5
Optional, and blank is a perfectly good answer. Your own judgement as the releasing clinician, informed by the aggregate above — never an average of respondent scores.
Release to trainee…
If it should never be released, withdraw it on its campaign page.

######## BOARD R3-R5-typed-1280.dc.html — Report · the narrative typed, 106 of 4,000 characters (3.46) · 1280
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF report for Lerato Molefe
## MSF report for Lerato Molefe
Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28
Open its campaign page
Under review
### Ready to release: every threshold is met.
8 responses, of the 5 needed.
2 respondent groups reporting, of the 2 needed.
A respondent group reports once 3 of it have responded; a group with fewer is suppressed.
#### Evidence for
PAED-010 — Leading and operating within a clinical team
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
One evidence record per EPA is written to the trainee's portfolio when the report is released. DATA: will be recorded
### By respondent group
As Lerato Molefe will read it once released. Ratings are averaged from 1, Well below expectations, to 5, Well above expectations.
#### Consultant · ‹n.nn› average rating, from 3 responses
What should the trainee keep doing or improve?
Sound judgement in clinic; teaches the interns well
Ready to run a ward; should document escalation decisions more fully
‹the third consultant's comment›
#### Nurse · ‹n.nn› average rating, from 3 responses
What should the trainee keep doing or improve?
Calm and kind with parents at night; hands over clearly
Leads neonatal resuscitations and explains her decisions to the nurses
‹the third nurse's comment›
Suppressed: Peer doctor, Allied health professional. Fewer than 3 responses each, so their ratings and comments are not shown here. Lerato Molefe sees nothing of them, not even that they answered.
### Release to Lerato Molefe
When you release:
Lerato Molefe can read your narrative and the groups shown above on MSF reports.
2 evidence records are written to Lerato Molefe's portfolio, for PAED-010 and PAED-012.
Lerato Molefe is sent an email saying a report has been released, with a link to MSF reports. decided, T367
It cannot be undone.
Narrative
[aria-describedby: 106 of 4,000 characters NEW: counter] Consistently strong feedback from nursing and consultant colleagues; keep documenting escalation decisions
106 of 4,000 characters NEW: counter
Supervision level this feedback supports
[aria-describedby: Optional, and blank is a perfectly good answer. Your own judgement as the releasing clinician, informed by the aggregate above — never an average of respondent scores.]
Not stated
1
2
3a
3b
4
5
Optional, and blank is a perfectly good answer. Your own judgement as the releasing clinician, informed by the aggregate above — never an average of respondent scores.
Release to trainee…
If it should never be released, withdraw it on its campaign page.

######## BOARD R3-R5-typed-390.dc.html — Report · the narrative typed, 106 of 4,000 characters (3.46) · 390
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF report for Lerato Molefe
## MSF report for Lerato Molefe
Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28
Open its campaign page
Under review
### Ready to release: every threshold is met.
8 responses, of the 5 needed.
2 respondent groups reporting, of the 2 needed.
A respondent group reports once 3 of it have responded; a group with fewer is suppressed.
#### Evidence for
PAED-010 — Leading and operating within a clinical team
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
One evidence record per EPA is written to the trainee's portfolio when the report is released. DATA: will be recorded
### By respondent group
As Lerato Molefe will read it once released. Ratings are averaged from 1, Well below expectations, to 5, Well above expectations.
#### Consultant · ‹n.nn› average rating, from 3 responses
What should the trainee keep doing or improve?
Sound judgement in clinic; teaches the interns well
Ready to run a ward; should document escalation decisions more fully
‹the third consultant's comment›
#### Nurse · ‹n.nn› average rating, from 3 responses
What should the trainee keep doing or improve?
Calm and kind with parents at night; hands over clearly
Leads neonatal resuscitations and explains her decisions to the nurses
‹the third nurse's comment›
Suppressed: Peer doctor, Allied health professional. Fewer than 3 responses each, so their ratings and comments are not shown here. Lerato Molefe sees nothing of them, not even that they answered.
### Release to Lerato Molefe
When you release:
Lerato Molefe can read your narrative and the groups shown above on MSF reports.
2 evidence records are written to Lerato Molefe's portfolio, for PAED-010 and PAED-012.
Lerato Molefe is sent an email saying a report has been released, with a link to MSF reports. decided, T367
It cannot be undone.
Narrative
[aria-describedby: 106 of 4,000 characters NEW: counter] Consistently strong feedback from nursing and consultant colleagues; keep documenting escalation decisions
106 of 4,000 characters NEW: counter
Supervision level this feedback supports
[aria-describedby: Optional, and blank is a perfectly good answer. Your own judgement as the releasing clinician, informed by the aggregate above — never an average of respondent scores.]
Not stated
1
2
3a
3b
4
5
Optional, and blank is a perfectly good answer. Your own judgement as the releasing clinician, informed by the aggregate above — never an average of respondent scores.
Release to trainee…
If it should never be released, withdraw it on its campaign page.

######## BOARD R3-R6a-release-dialog-1280.dc.html — Report · Release's dialog, focus on Not yet (3.46) · 1280
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF report for Lerato Molefe
## MSF report for Lerato Molefe
Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28
Open its campaign page
Under review
### Ready to release: every threshold is met.
8 responses, of the 5 needed.
2 respondent groups reporting, of the 2 needed.
A respondent group reports once 3 of it have responded; a group with fewer is suppressed.
#### Evidence for
PAED-010 — Leading and operating within a clinical team
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
One evidence record per EPA is written to the trainee's portfolio when the report is released. DATA: will be recorded
### By respondent group
As Lerato Molefe will read it once released. Ratings are averaged from 1, Well below expectations, to 5, Well above expectations.
#### Consultant · ‹n.nn› average rating, from 3 responses
What should the trainee keep doing or improve?
Sound judgement in clinic; teaches the interns well
Ready to run a ward; should document escalation decisions more fully
‹the third consultant's comment›
#### Nurse · ‹n.nn› average rating, from 3 responses
What should the trainee keep doing or improve?
Calm and kind with parents at night; hands over clearly
Leads neonatal resuscitations and explains her decisions to the nurses
‹the third nurse's comment›
Suppressed: Peer doctor, Allied health professional. Fewer than 3 responses each, so their ratings and comments are not shown here. Lerato Molefe sees nothing of them, not even that they answered.
### Release to Lerato Molefe
When you release:
Lerato Molefe can read your narrative and the groups shown above on MSF reports.
2 evidence records are written to Lerato Molefe's portfolio, for PAED-010 and PAED-012.
Lerato Molefe is sent an email saying a report has been released, with a link to MSF reports. decided, T367
It cannot be undone.
Narrative
[aria-describedby: 106 of 4,000 characters NEW: counter] Consistently strong feedback from nursing and consultant colleagues; keep documenting escalation decisions
106 of 4,000 characters NEW: counter
Supervision level this feedback supports
[aria-describedby: Optional, and blank is a perfectly good answer. Your own judgement as the releasing clinician, informed by the aggregate above — never an average of respondent scores.]
Not stated
1
2
3a
3b
4
5
Optional, and blank is a perfectly good answer. Your own judgement as the releasing clinician, informed by the aggregate above — never an average of respondent scores.
Release to trainee…
If it should never be released, withdraw it on its campaign page.
[aria-describedby: Lerato Molefe can read it on MSF reports: your narrative, and the ratings and comments of the Consultant and Nurse groups. Nothing of the other groups is shown, not even that they answered. 2 evidence records are written to Lerato Molefe's portfolio: PAED-010 and PAED-012. DATA: will be recorded Supervision level: Not stated. Lerato Molefe is sent an email saying a report has been released, with a link to MSF reports. The email holds none of the feedback. decided, T367 Releasing cannot be undone: the narrative and the level cannot be changed afterwards.]
### Release this report to Lerato Molefe?
Lerato Molefe can read it on MSF reports: your narrative, and the ratings and comments of the Consultant and Nurse groups. Nothing of the other groups is shown, not even that they answered.
2 evidence records are written to Lerato Molefe's portfolio: PAED-010 and PAED-012. DATA: will be recorded
Supervision level: Not stated.
Lerato Molefe is sent an email saying a report has been released, with a link to MSF reports. The email holds none of the feedback. decided, T367
Releasing cannot be undone: the narrative and the level cannot be changed afterwards.
Not yet Release to Lerato Molefe

######## BOARD R3-R6a-release-dialog-768.dc.html — Report · Release's dialog, focus on Not yet (3.46) · 768
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF report for Lerato Molefe
## MSF report for Lerato Molefe
Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28
Open its campaign page
Under review
### Ready to release: every threshold is met.
8 responses, of the 5 needed.
2 respondent groups reporting, of the 2 needed.
A respondent group reports once 3 of it have responded; a group with fewer is suppressed.
#### Evidence for
PAED-010 — Leading and operating within a clinical team
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
One evidence record per EPA is written to the trainee's portfolio when the report is released. DATA: will be recorded
### By respondent group
As Lerato Molefe will read it once released. Ratings are averaged from 1, Well below expectations, to 5, Well above expectations.
#### Consultant · ‹n.nn› average rating, from 3 responses
What should the trainee keep doing or improve?
Sound judgement in clinic; teaches the interns well
Ready to run a ward; should document escalation decisions more fully
‹the third consultant's comment›
#### Nurse · ‹n.nn› average rating, from 3 responses
What should the trainee keep doing or improve?
Calm and kind with parents at night; hands over clearly
Leads neonatal resuscitations and explains her decisions to the nurses
‹the third nurse's comment›
Suppressed: Peer doctor, Allied health professional. Fewer than 3 responses each, so their ratings and comments are not shown here. Lerato Molefe sees nothing of them, not even that they answered.
### Release to Lerato Molefe
When you release:
Lerato Molefe can read your narrative and the groups shown above on MSF reports.
2 evidence records are written to Lerato Molefe's portfolio, for PAED-010 and PAED-012.
Lerato Molefe is sent an email saying a report has been released, with a link to MSF reports. decided, T367
It cannot be undone.
Narrative
[aria-describedby: 106 of 4,000 characters NEW: counter] Consistently strong feedback from nursing and consultant colleagues; keep documenting escalation decisions
106 of 4,000 characters NEW: counter
Supervision level this feedback supports
[aria-describedby: Optional, and blank is a perfectly good answer. Your own judgement as the releasing clinician, informed by the aggregate above — never an average of respondent scores.]
Not stated
1
2
3a
3b
4
5
Optional, and blank is a perfectly good answer. Your own judgement as the releasing clinician, informed by the aggregate above — never an average of respondent scores.
Release to trainee…
If it should never be released, withdraw it on its campaign page.
[aria-describedby: Lerato Molefe can read it on MSF reports: your narrative, and the ratings and comments of the Consultant and Nurse groups. Nothing of the other groups is shown, not even that they answered. 2 evidence records are written to Lerato Molefe's portfolio: PAED-010 and PAED-012. DATA: will be recorded Supervision level: Not stated. Lerato Molefe is sent an email saying a report has been released, with a link to MSF reports. The email holds none of the feedback. decided, T367 Releasing cannot be undone: the narrative and the level cannot be changed afterwards.]
### Release this report to Lerato Molefe?
Lerato Molefe can read it on MSF reports: your narrative, and the ratings and comments of the Consultant and Nurse groups. Nothing of the other groups is shown, not even that they answered.
2 evidence records are written to Lerato Molefe's portfolio: PAED-010 and PAED-012. DATA: will be recorded
Supervision level: Not stated.
Lerato Molefe is sent an email saying a report has been released, with a link to MSF reports. The email holds none of the feedback. decided, T367
Releasing cannot be undone: the narrative and the level cannot be changed afterwards.
Not yet Release to Lerato Molefe

######## BOARD R3-R6a-release-dialog-390.dc.html — Report · Release's dialog, focus on Not yet (3.46) · 390
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF report for Lerato Molefe
## MSF report for Lerato Molefe
Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28
Open its campaign page
Under review
### Ready to release: every threshold is met.
8 responses, of the 5 needed.
2 respondent groups reporting, of the 2 needed.
A respondent group reports once 3 of it have responded; a group with fewer is suppressed.
#### Evidence for
PAED-010 — Leading and operating within a clinical team
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
One evidence record per EPA is written to the trainee's portfolio when the report is released. DATA: will be recorded
### By respondent group
As Lerato Molefe will read it once released. Ratings are averaged from 1, Well below expectations, to 5, Well above expectations.
#### Consultant · ‹n.nn› average rating, from 3 responses
What should the trainee keep doing or improve?
Sound judgement in clinic; teaches the interns well
Ready to run a ward; should document escalation decisions more fully
‹the third consultant's comment›
#### Nurse · ‹n.nn› average rating, from 3 responses
What should the trainee keep doing or improve?
Calm and kind with parents at night; hands over clearly
Leads neonatal resuscitations and explains her decisions to the nurses
‹the third nurse's comment›
Suppressed: Peer doctor, Allied health professional. Fewer than 3 responses each, so their ratings and comments are not shown here. Lerato Molefe sees nothing of them, not even that they answered.
### Release to Lerato Molefe
When you release:
Lerato Molefe can read your narrative and the groups shown above on MSF reports.
2 evidence records are written to Lerato Molefe's portfolio, for PAED-010 and PAED-012.
Lerato Molefe is sent an email saying a report has been released, with a link to MSF reports. decided, T367
It cannot be undone.
Narrative
[aria-describedby: 106 of 4,000 characters NEW: counter] Consistently strong feedback from nursing and consultant colleagues; keep documenting escalation decisions
106 of 4,000 characters NEW: counter
Supervision level this feedback supports
[aria-describedby: Optional, and blank is a perfectly good answer. Your own judgement as the releasing clinician, informed by the aggregate above — never an average of respondent scores.]
Not stated
1
2
3a
3b
4
5
Optional, and blank is a perfectly good answer. Your own judgement as the releasing clinician, informed by the aggregate above — never an average of respondent scores.
Release to trainee…
If it should never be released, withdraw it on its campaign page.
[aria-describedby: Lerato Molefe can read it on MSF reports: your narrative, and the ratings and comments of the Consultant and Nurse groups. Nothing of the other groups is shown, not even that they answered. 2 evidence records are written to Lerato Molefe's portfolio: PAED-010 and PAED-012. DATA: will be recorded Supervision level: Not stated. Lerato Molefe is sent an email saying a report has been released, with a link to MSF reports. The email holds none of the feedback. decided, T367 Releasing cannot be undone: the narrative and the level cannot be changed afterwards.]
### Release this report to Lerato Molefe?
Lerato Molefe can read it on MSF reports: your narrative, and the ratings and comments of the Consultant and Nurse groups. Nothing of the other groups is shown, not even that they answered.
2 evidence records are written to Lerato Molefe's portfolio: PAED-010 and PAED-012. DATA: will be recorded
Supervision level: Not stated.
Lerato Molefe is sent an email saying a report has been released, with a link to MSF reports. The email holds none of the feedback. decided, T367
Releasing cannot be undone: the narrative and the level cannot be changed afterwards.
Not yet Release to Lerato Molefe

######## BOARD R3-R6b-release-dialog-dropped-1280.dc.html — Report · Release's dialog: PAED-012 no longer on her curriculum (R6d, Illustration) · 1280
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
Illustration PAED-012 left her curriculum after the campaign was created.
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF report for Lerato Molefe
## MSF report for Lerato Molefe
Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28
Open its campaign page
Under review
### Ready to release: every threshold is met.
8 responses, of the 5 needed.
2 respondent groups reporting, of the 2 needed.
A respondent group reports once 3 of it have responded; a group with fewer is suppressed.
#### Evidence for
PAED-010 — Leading and operating within a clinical team
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
PAED-012 is no longer on her curriculum and will not be recorded. One evidence record per EPA is written to the trainee's portfolio when the report is released. DATA: will be recorded
### By respondent group
As Lerato Molefe will read it once released. Ratings are averaged from 1, Well below expectations, to 5, Well above expectations.
#### Consultant · ‹n.nn› average rating, from 3 responses
What should the trainee keep doing or improve?
Sound judgement in clinic; teaches the interns well
Ready to run a ward; should document escalation decisions more fully
‹the third consultant's comment›
#### Nurse · ‹n.nn› average rating, from 3 responses
What should the trainee keep doing or improve?
Calm and kind with parents at night; hands over clearly
Leads neonatal resuscitations and explains her decisions to the nurses
‹the third nurse's comment›
Suppressed: Peer doctor, Allied health professional. Fewer than 3 responses each, so their ratings and comments are not shown here. Lerato Molefe sees nothing of them, not even that they answered.
### Release to Lerato Molefe
When you release:
Lerato Molefe can read your narrative and the groups shown above on MSF reports.
1 evidence record is written to Lerato Molefe's portfolio, for PAED-010; PAED-012 is no longer on her curriculum and is not recorded.
Lerato Molefe is sent an email saying a report has been released, with a link to MSF reports. decided, T367
It cannot be undone.
Narrative
[aria-describedby: 106 of 4,000 characters NEW: counter] Consistently strong feedback from nursing and consultant colleagues; keep documenting escalation decisions
106 of 4,000 characters NEW: counter
Supervision level this feedback supports
[aria-describedby: Optional, and blank is a perfectly good answer. Your own judgement as the releasing clinician, informed by the aggregate above — never an average of respondent scores.]
Not stated
1
2
3a
3b
4
5
Optional, and blank is a perfectly good answer. Your own judgement as the releasing clinician, informed by the aggregate above — never an average of respondent scores.
Release to trainee…
If it should never be released, withdraw it on its campaign page.
[aria-describedby: Lerato Molefe can read it on MSF reports: your narrative, and the ratings and comments of the Consultant and Nurse groups. Nothing of the other groups is shown, not even that they answered. 1 evidence record is written to Lerato Molefe's portfolio: PAED-010. PAED-012: not recorded, no longer on her curriculum. DATA: will be recorded Supervision level: Not stated. Lerato Molefe is sent an email saying a report has been released, with a link to MSF reports. The email holds none of the feedback. decided, T367 Releasing cannot be undone: the narrative and the level cannot be changed afterwards.]
### Release this report to Lerato Molefe?
Lerato Molefe can read it on MSF reports: your narrative, and the ratings and comments of the Consultant and Nurse groups. Nothing of the other groups is shown, not even that they answered.
1 evidence record is written to Lerato Molefe's portfolio: PAED-010. PAED-012: not recorded, no longer on her curriculum. DATA: will be recorded
Supervision level: Not stated.
Lerato Molefe is sent an email saying a report has been released, with a link to MSF reports. The email holds none of the feedback. decided, T367
Releasing cannot be undone: the narrative and the level cannot be changed afterwards.
Not yet Release to Lerato Molefe

######## BOARD R3-R6b-release-dialog-dropped-390.dc.html — Report · Release's dialog: PAED-012 no longer on her curriculum (R6d, Illustration) · 390
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
Illustration PAED-012 left her curriculum after the campaign was created.
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF report for Lerato Molefe
## MSF report for Lerato Molefe
Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28
Open its campaign page
Under review
### Ready to release: every threshold is met.
8 responses, of the 5 needed.
2 respondent groups reporting, of the 2 needed.
A respondent group reports once 3 of it have responded; a group with fewer is suppressed.
#### Evidence for
PAED-010 — Leading and operating within a clinical team
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
PAED-012 is no longer on her curriculum and will not be recorded. One evidence record per EPA is written to the trainee's portfolio when the report is released. DATA: will be recorded
### By respondent group
As Lerato Molefe will read it once released. Ratings are averaged from 1, Well below expectations, to 5, Well above expectations.
#### Consultant · ‹n.nn› average rating, from 3 responses
What should the trainee keep doing or improve?
Sound judgement in clinic; teaches the interns well
Ready to run a ward; should document escalation decisions more fully
‹the third consultant's comment›
#### Nurse · ‹n.nn› average rating, from 3 responses
What should the trainee keep doing or improve?
Calm and kind with parents at night; hands over clearly
Leads neonatal resuscitations and explains her decisions to the nurses
‹the third nurse's comment›
Suppressed: Peer doctor, Allied health professional. Fewer than 3 responses each, so their ratings and comments are not shown here. Lerato Molefe sees nothing of them, not even that they answered.
### Release to Lerato Molefe
When you release:
Lerato Molefe can read your narrative and the groups shown above on MSF reports.
1 evidence record is written to Lerato Molefe's portfolio, for PAED-010; PAED-012 is no longer on her curriculum and is not recorded.
Lerato Molefe is sent an email saying a report has been released, with a link to MSF reports. decided, T367
It cannot be undone.
Narrative
[aria-describedby: 106 of 4,000 characters NEW: counter] Consistently strong feedback from nursing and consultant colleagues; keep documenting escalation decisions
106 of 4,000 characters NEW: counter
Supervision level this feedback supports
[aria-describedby: Optional, and blank is a perfectly good answer. Your own judgement as the releasing clinician, informed by the aggregate above — never an average of respondent scores.]
Not stated
1
2
3a
3b
4
5
Optional, and blank is a perfectly good answer. Your own judgement as the releasing clinician, informed by the aggregate above — never an average of respondent scores.
Release to trainee…
If it should never be released, withdraw it on its campaign page.
[aria-describedby: Lerato Molefe can read it on MSF reports: your narrative, and the ratings and comments of the Consultant and Nurse groups. Nothing of the other groups is shown, not even that they answered. 1 evidence record is written to Lerato Molefe's portfolio: PAED-010. PAED-012: not recorded, no longer on her curriculum. DATA: will be recorded Supervision level: Not stated. Lerato Molefe is sent an email saying a report has been released, with a link to MSF reports. The email holds none of the feedback. decided, T367 Releasing cannot be undone: the narrative and the level cannot be changed afterwards.]
### Release this report to Lerato Molefe?
Lerato Molefe can read it on MSF reports: your narrative, and the ratings and comments of the Consultant and Nurse groups. Nothing of the other groups is shown, not even that they answered.
1 evidence record is written to Lerato Molefe's portfolio: PAED-010. PAED-012: not recorded, no longer on her curriculum. DATA: will be recorded
Supervision level: Not stated.
Lerato Molefe is sent an email saying a report has been released, with a link to MSF reports. The email holds none of the feedback. decided, T367
Releasing cannot be undone: the narrative and the level cannot be changed afterwards.
Not yet Release to Lerato Molefe

######## BOARD R3-R6c-release-dialog-no-epa-1280.dc.html — Report · Release's dialog: a campaign naming no EPA (R6d, Illustration) · 1280
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
Illustration R9 (Anele Dlamini's) is blocked, so no dialog opens there: this backdrop is Lerato Molefe's page with no EPA named.
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF report for Lerato Molefe
## MSF report for Lerato Molefe
Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28
Open its campaign page
Under review
### Ready to release: every threshold is met.
8 responses, of the 5 needed.
2 respondent groups reporting, of the 2 needed.
A respondent group reports once 3 of it have responded; a group with fewer is suppressed.
#### Evidence for
This campaign names no EPA, so releasing it will record no evidence on the trainee's portfolio.
### By respondent group
As Lerato Molefe will read it once released. Ratings are averaged from 1, Well below expectations, to 5, Well above expectations.
#### Consultant · ‹n.nn› average rating, from 3 responses
What should the trainee keep doing or improve?
Sound judgement in clinic; teaches the interns well
Ready to run a ward; should document escalation decisions more fully
‹the third consultant's comment›
#### Nurse · ‹n.nn› average rating, from 3 responses
What should the trainee keep doing or improve?
Calm and kind with parents at night; hands over clearly
Leads neonatal resuscitations and explains her decisions to the nurses
‹the third nurse's comment›
Suppressed: Peer doctor, Allied health professional. Fewer than 3 responses each, so their ratings and comments are not shown here. Lerato Molefe sees nothing of them, not even that they answered.
### Release to Lerato Molefe
When you release:
Lerato Molefe can read your narrative and the groups shown above on MSF reports.
Lerato Molefe is sent an email saying a report has been released, with a link to MSF reports. decided, T367
It cannot be undone.
Narrative
[aria-describedby: 106 of 4,000 characters NEW: counter] Consistently strong feedback from nursing and consultant colleagues; keep documenting escalation decisions
106 of 4,000 characters NEW: counter
Supervision level this feedback supports
[aria-describedby: Optional, and blank is a perfectly good answer. Your own judgement as the releasing clinician, informed by the aggregate above — never an average of respondent scores.]
Not stated
1
2
3a
3b
4
5
Optional, and blank is a perfectly good answer. Your own judgement as the releasing clinician, informed by the aggregate above — never an average of respondent scores.
Release to trainee…
If it should never be released, withdraw it on its campaign page.
[aria-describedby: Lerato Molefe can read it on MSF reports: your narrative, and the ratings and comments of the Consultant and Nurse groups. Nothing of the other groups is shown, not even that they answered. No evidence is recorded. Supervision level: Not stated. Lerato Molefe is sent an email saying a report has been released, with a link to MSF reports. The email holds none of the feedback. decided, T367 Releasing cannot be undone: the narrative and the level cannot be changed afterwards.]
### Release this report to Lerato Molefe?
Lerato Molefe can read it on MSF reports: your narrative, and the ratings and comments of the Consultant and Nurse groups. Nothing of the other groups is shown, not even that they answered.
No evidence is recorded.
Supervision level: Not stated.
Lerato Molefe is sent an email saying a report has been released, with a link to MSF reports. The email holds none of the feedback. decided, T367
Releasing cannot be undone: the narrative and the level cannot be changed afterwards.
Not yet Release to Lerato Molefe

######## BOARD R3-R6c-release-dialog-no-epa-390.dc.html — Report · Release's dialog: a campaign naming no EPA (R6d, Illustration) · 390
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
Illustration R9 (Anele Dlamini's) is blocked, so no dialog opens there: this backdrop is Lerato Molefe's page with no EPA named.
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF report for Lerato Molefe
## MSF report for Lerato Molefe
Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28
Open its campaign page
Under review
### Ready to release: every threshold is met.
8 responses, of the 5 needed.
2 respondent groups reporting, of the 2 needed.
A respondent group reports once 3 of it have responded; a group with fewer is suppressed.
#### Evidence for
This campaign names no EPA, so releasing it will record no evidence on the trainee's portfolio.
### By respondent group
As Lerato Molefe will read it once released. Ratings are averaged from 1, Well below expectations, to 5, Well above expectations.
#### Consultant · ‹n.nn› average rating, from 3 responses
What should the trainee keep doing or improve?
Sound judgement in clinic; teaches the interns well
Ready to run a ward; should document escalation decisions more fully
‹the third consultant's comment›
#### Nurse · ‹n.nn› average rating, from 3 responses
What should the trainee keep doing or improve?
Calm and kind with parents at night; hands over clearly
Leads neonatal resuscitations and explains her decisions to the nurses
‹the third nurse's comment›
Suppressed: Peer doctor, Allied health professional. Fewer than 3 responses each, so their ratings and comments are not shown here. Lerato Molefe sees nothing of them, not even that they answered.
### Release to Lerato Molefe
When you release:
Lerato Molefe can read your narrative and the groups shown above on MSF reports.
Lerato Molefe is sent an email saying a report has been released, with a link to MSF reports. decided, T367
It cannot be undone.
Narrative
[aria-describedby: 106 of 4,000 characters NEW: counter] Consistently strong feedback from nursing and consultant colleagues; keep documenting escalation decisions
106 of 4,000 characters NEW: counter
Supervision level this feedback supports
[aria-describedby: Optional, and blank is a perfectly good answer. Your own judgement as the releasing clinician, informed by the aggregate above — never an average of respondent scores.]
Not stated
1
2
3a
3b
4
5
Optional, and blank is a perfectly good answer. Your own judgement as the releasing clinician, informed by the aggregate above — never an average of respondent scores.
Release to trainee…
If it should never be released, withdraw it on its campaign page.
[aria-describedby: Lerato Molefe can read it on MSF reports: your narrative, and the ratings and comments of the Consultant and Nurse groups. Nothing of the other groups is shown, not even that they answered. No evidence is recorded. Supervision level: Not stated. Lerato Molefe is sent an email saying a report has been released, with a link to MSF reports. The email holds none of the feedback. decided, T367 Releasing cannot be undone: the narrative and the level cannot be changed afterwards.]
### Release this report to Lerato Molefe?
Lerato Molefe can read it on MSF reports: your narrative, and the ratings and comments of the Consultant and Nurse groups. Nothing of the other groups is shown, not even that they answered.
No evidence is recorded.
Supervision level: Not stated.
Lerato Molefe is sent an email saying a report has been released, with a link to MSF reports. The email holds none of the feedback. decided, T367
Releasing cannot be undone: the narrative and the level cannot be changed afterwards.
Not yet Release to Lerato Molefe

######## BOARD R3-R6d-release-dialog-ended-1280.dc.html — Report · Release's dialog: the programme ended (R6d, Q10, F4, Illustration) · 1280
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
Illustration The backdrop is R10.
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF report for Lerato Molefe
## MSF report for Lerato Molefe
Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28
Open its campaign page
Under review
### Ready to release: every threshold is met.
Lerato Molefe's programme ended on ‹date›. A release still records the feedback as evidence on her record, but she can no longer open MSF reports to read it. DATA: SubjectIsCurrent, SubjectEndedOn
8 responses, of the 5 needed.
2 respondent groups reporting, of the 2 needed.
A respondent group reports once 3 of it have responded; a group with fewer is suppressed.
#### Evidence for
PAED-010 — Leading and operating within a clinical team
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
One evidence record per EPA is written to the trainee's portfolio when the report is released. DATA: will be recorded
### By respondent group
As Lerato Molefe would have read it. Ratings are averaged from 1, Well below expectations, to 5, Well above expectations.
#### Consultant · ‹n.nn› average rating, from 3 responses
What should the trainee keep doing or improve?
Sound judgement in clinic; teaches the interns well
Ready to run a ward; should document escalation decisions more fully
‹the third consultant's comment›
#### Nurse · ‹n.nn› average rating, from 3 responses
What should the trainee keep doing or improve?
Calm and kind with parents at night; hands over clearly
Leads neonatal resuscitations and explains her decisions to the nurses
‹the third nurse's comment›
Suppressed: Peer doctor, Allied health professional. Fewer than 3 responses each, so their ratings and comments are not shown here. Lerato Molefe sees nothing of them, not even that they answered.
### Release to Lerato Molefe
When you release:
She can no longer open MSF reports; the report is kept with the evidence on her record.
2 evidence records are written to Lerato Molefe's portfolio, for PAED-010 and PAED-012.
No email is sent: she can no longer open MSF reports.
It cannot be undone.
Narrative
[aria-describedby: 106 of 4,000 characters NEW: counter] Consistently strong feedback from nursing and consultant colleagues; keep documenting escalation decisions
106 of 4,000 characters NEW: counter
Supervision level this feedback supports
[aria-describedby: Optional, and blank is a perfectly good answer. Your own judgement as the releasing clinician, informed by the aggregate above — never an average of respondent scores.]
Not stated
1
2
3a
3b
4
5
Optional, and blank is a perfectly good answer. Your own judgement as the releasing clinician, informed by the aggregate above — never an average of respondent scores.
Release to trainee…
If it should never be released, withdraw it on its campaign page.
[aria-describedby: Lerato Molefe's programme ended on ‹date›. A release still records the feedback as evidence on her record, but she can no longer open MSF reports to read it. She can no longer open MSF reports; the report is kept with the evidence on her record. 2 evidence records are written to Lerato Molefe's portfolio: PAED-010 and PAED-012. DATA: will be recorded Supervision level: Not stated. No email is sent: she can no longer open MSF reports. Releasing cannot be undone: the narrative and the level cannot be changed afterwards.]
### Release this report to Lerato Molefe?
Lerato Molefe's programme ended on ‹date›. A release still records the feedback as evidence on her record, but she can no longer open MSF reports to read it.
She can no longer open MSF reports; the report is kept with the evidence on her record.
2 evidence records are written to Lerato Molefe's portfolio: PAED-010 and PAED-012. DATA: will be recorded
Supervision level: Not stated.
No email is sent: she can no longer open MSF reports.
Releasing cannot be undone: the narrative and the level cannot be changed afterwards.
Not yet Release to Lerato Molefe

######## BOARD R3-R6d-release-dialog-ended-390.dc.html — Report · Release's dialog: the programme ended (R6d, Q10, F4, Illustration) · 390
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
Illustration The backdrop is R10.
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF report for Lerato Molefe
## MSF report for Lerato Molefe
Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28
Open its campaign page
Under review
### Ready to release: every threshold is met.
Lerato Molefe's programme ended on ‹date›. A release still records the feedback as evidence on her record, but she can no longer open MSF reports to read it. DATA: SubjectIsCurrent, SubjectEndedOn
8 responses, of the 5 needed.
2 respondent groups reporting, of the 2 needed.
A respondent group reports once 3 of it have responded; a group with fewer is suppressed.
#### Evidence for
PAED-010 — Leading and operating within a clinical team
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
One evidence record per EPA is written to the trainee's portfolio when the report is released. DATA: will be recorded
### By respondent group
As Lerato Molefe would have read it. Ratings are averaged from 1, Well below expectations, to 5, Well above expectations.
#### Consultant · ‹n.nn› average rating, from 3 responses
What should the trainee keep doing or improve?
Sound judgement in clinic; teaches the interns well
Ready to run a ward; should document escalation decisions more fully
‹the third consultant's comment›
#### Nurse · ‹n.nn› average rating, from 3 responses
What should the trainee keep doing or improve?
Calm and kind with parents at night; hands over clearly
Leads neonatal resuscitations and explains her decisions to the nurses
‹the third nurse's comment›
Suppressed: Peer doctor, Allied health professional. Fewer than 3 responses each, so their ratings and comments are not shown here. Lerato Molefe sees nothing of them, not even that they answered.
### Release to Lerato Molefe
When you release:
She can no longer open MSF reports; the report is kept with the evidence on her record.
2 evidence records are written to Lerato Molefe's portfolio, for PAED-010 and PAED-012.
No email is sent: she can no longer open MSF reports.
It cannot be undone.
Narrative
[aria-describedby: 106 of 4,000 characters NEW: counter] Consistently strong feedback from nursing and consultant colleagues; keep documenting escalation decisions
106 of 4,000 characters NEW: counter
Supervision level this feedback supports
[aria-describedby: Optional, and blank is a perfectly good answer. Your own judgement as the releasing clinician, informed by the aggregate above — never an average of respondent scores.]
Not stated
1
2
3a
3b
4
5
Optional, and blank is a perfectly good answer. Your own judgement as the releasing clinician, informed by the aggregate above — never an average of respondent scores.
Release to trainee…
If it should never be released, withdraw it on its campaign page.
[aria-describedby: Lerato Molefe's programme ended on ‹date›. A release still records the feedback as evidence on her record, but she can no longer open MSF reports to read it. She can no longer open MSF reports; the report is kept with the evidence on her record. 2 evidence records are written to Lerato Molefe's portfolio: PAED-010 and PAED-012. DATA: will be recorded Supervision level: Not stated. No email is sent: she can no longer open MSF reports. Releasing cannot be undone: the narrative and the level cannot be changed afterwards.]
### Release this report to Lerato Molefe?
Lerato Molefe's programme ended on ‹date›. A release still records the feedback as evidence on her record, but she can no longer open MSF reports to read it.
She can no longer open MSF reports; the report is kept with the evidence on her record.
2 evidence records are written to Lerato Molefe's portfolio: PAED-010 and PAED-012. DATA: will be recorded
Supervision level: Not stated.
No email is sent: she can no longer open MSF reports.
Releasing cannot be undone: the narrative and the level cannot be changed afterwards.
Not yet Release to Lerato Molefe

######## BOARD R3-R7-released-1280.dc.html — Report · released, as stored, the result focused (3.46) · 1280
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF report for Lerato Molefe
## MSF report for Lerato Molefe
Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28
Open its campaign page
Report released to Lerato Molefe.
Released
### The report has been released to the trainee.
2026-10-14 16:34 SAST·8 responses, with 5 needed·2 respondent groups reporting, with 2 needed
#### Evidence for
PAED-010 — Leading and operating within a clinical team
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
Recorded on the portfolio: 2026-10-14 16:34 SAST
### By respondent group
As Lerato Molefe reads it. Ratings are averaged from 1, Well below expectations, to 5, Well above expectations.
#### Consultant · ‹n.nn› average rating, from 3 responses
What should the trainee keep doing or improve?
Sound judgement in clinic; teaches the interns well
Ready to run a ward; should document escalation decisions more fully
‹the third consultant's comment›
#### Nurse · ‹n.nn› average rating, from 3 responses
What should the trainee keep doing or improve?
Calm and kind with parents at night; hands over clearly
Leads neonatal resuscitations and explains her decisions to the nurses
‹the third nurse's comment›
Suppressed: Peer doctor, Allied health professional. Fewer than 3 responses each, so their ratings and comments are not shown here.
### As released
Narrative
Consistently strong feedback from nursing and consultant colleagues; keep documenting escalation decisions
Supervision level
Not stated

######## BOARD R3-R7-released-768.dc.html — Report · released, as stored, the result focused (3.46) · 768
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF report for Lerato Molefe
## MSF report for Lerato Molefe
Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28
Open its campaign page
Report released to Lerato Molefe.
Released
### The report has been released to the trainee.
2026-10-14 16:34 SAST·8 responses, with 5 needed·2 respondent groups reporting, with 2 needed
#### Evidence for
PAED-010 — Leading and operating within a clinical team
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
Recorded on the portfolio: 2026-10-14 16:34 SAST
### By respondent group
As Lerato Molefe reads it. Ratings are averaged from 1, Well below expectations, to 5, Well above expectations.
#### Consultant · ‹n.nn› average rating, from 3 responses
What should the trainee keep doing or improve?
Sound judgement in clinic; teaches the interns well
Ready to run a ward; should document escalation decisions more fully
‹the third consultant's comment›
#### Nurse · ‹n.nn› average rating, from 3 responses
What should the trainee keep doing or improve?
Calm and kind with parents at night; hands over clearly
Leads neonatal resuscitations and explains her decisions to the nurses
‹the third nurse's comment›
Suppressed: Peer doctor, Allied health professional. Fewer than 3 responses each, so their ratings and comments are not shown here.
### As released
Narrative
Consistently strong feedback from nursing and consultant colleagues; keep documenting escalation decisions
Supervision level
Not stated

######## BOARD R3-R7-released-390.dc.html — Report · released, as stored, the result focused (3.46) · 390
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF report for Lerato Molefe
## MSF report for Lerato Molefe
Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28
Open its campaign page
Report released to Lerato Molefe.
Released
### The report has been released to the trainee.
2026-10-14 16:34 SAST·8 responses, with 5 needed·2 respondent groups reporting, with 2 needed
#### Evidence for
PAED-010 — Leading and operating within a clinical team
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
Recorded on the portfolio: 2026-10-14 16:34 SAST
### By respondent group
As Lerato Molefe reads it. Ratings are averaged from 1, Well below expectations, to 5, Well above expectations.
#### Consultant · ‹n.nn› average rating, from 3 responses
What should the trainee keep doing or improve?
Sound judgement in clinic; teaches the interns well
Ready to run a ward; should document escalation decisions more fully
‹the third consultant's comment›
#### Nurse · ‹n.nn› average rating, from 3 responses
What should the trainee keep doing or improve?
Calm and kind with parents at night; hands over clearly
Leads neonatal resuscitations and explains her decisions to the nurses
‹the third nurse's comment›
Suppressed: Peer doctor, Allied health professional. Fewer than 3 responses each, so their ratings and comments are not shown here.
### As released
Narrative
Consistently strong feedback from nursing and consultant colleagues; keep documenting escalation decisions
Supervision level
Not stated

######## BOARD R3-R7b-released-not-recorded-1280.dc.html — Report · released, PAED-012's record not written (R5g, Illustration) · 1280
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
Illustration PAED-012 left her curriculum before the release.
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF report for Lerato Molefe
## MSF report for Lerato Molefe
Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28
Open its campaign page
Released
### The report has been released to the trainee.
2026-10-14 16:34 SAST·8 responses, with 5 needed·2 respondent groups reporting, with 2 needed
#### Evidence for
PAED-010 — Leading and operating within a clinical team
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams: not recorded
Recorded on the portfolio: 2026-10-14 16:34 SAST
### By respondent group
As Lerato Molefe reads it. Ratings are averaged from 1, Well below expectations, to 5, Well above expectations.
#### Consultant · ‹n.nn› average rating, from 3 responses
What should the trainee keep doing or improve?
Sound judgement in clinic; teaches the interns well
Ready to run a ward; should document escalation decisions more fully
‹the third consultant's comment›
#### Nurse · ‹n.nn› average rating, from 3 responses
What should the trainee keep doing or improve?
Calm and kind with parents at night; hands over clearly
Leads neonatal resuscitations and explains her decisions to the nurses
‹the third nurse's comment›
Suppressed: Peer doctor, Allied health professional. Fewer than 3 responses each, so their ratings and comments are not shown here.
### As released
Narrative
Consistently strong feedback from nursing and consultant colleagues; keep documenting escalation decisions
Supervision level
Not stated

######## BOARD R3-R7b-released-not-recorded-390.dc.html — Report · released, PAED-012's record not written (R5g, Illustration) · 390
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
Illustration PAED-012 left her curriculum before the release.
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF report for Lerato Molefe
## MSF report for Lerato Molefe
Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28
Open its campaign page
Released
### The report has been released to the trainee.
2026-10-14 16:34 SAST·8 responses, with 5 needed·2 respondent groups reporting, with 2 needed
#### Evidence for
PAED-010 — Leading and operating within a clinical team
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams: not recorded
Recorded on the portfolio: 2026-10-14 16:34 SAST
### By respondent group
As Lerato Molefe reads it. Ratings are averaged from 1, Well below expectations, to 5, Well above expectations.
#### Consultant · ‹n.nn› average rating, from 3 responses
What should the trainee keep doing or improve?
Sound judgement in clinic; teaches the interns well
Ready to run a ward; should document escalation decisions more fully
‹the third consultant's comment›
#### Nurse · ‹n.nn› average rating, from 3 responses
What should the trainee keep doing or improve?
Calm and kind with parents at night; hands over clearly
Leads neonatal resuscitations and explains her decisions to the nurses
‹the third nurse's comment›
Suppressed: Peer doctor, Allied health professional. Fewer than 3 responses each, so their ratings and comments are not shown here.
### As released
Narrative
Consistently strong feedback from nursing and consultant colleagues; keep documenting escalation decisions
Supervision level
Not stated

######## BOARD R3-R8a-blocked-responses-1280.dc.html — Report · blocked: too few responses, Anele Dlamini's campaign (R5b, F10) · 1280
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
Anele Dlamini's campaign: one invitee, a Consultant, 1 response; thresholds 5 / 3 / 2 as the old-frame captures read them (F10 gave ‹n›).
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF report for Anele Dlamini
## MSF report for Anele Dlamini
Default MSF · Multi-source feedback · ‹opens› to ‹closes›
Open its campaign page
Under review
### Not ready to release.
1 response, of the 5 needed.
0 respondent groups reporting, of the 2 needed.
A respondent group reports once 3 of it have responded; a group with fewer is suppressed.
#### Evidence for
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
One evidence record per EPA is written to the trainee's portfolio when the report is released. DATA: will be recorded
### By respondent group
As Anele Dlamini will read it once released. Ratings are averaged from 1, Well below expectations, to 5, Well above expectations.
Suppressed: Consultant. Fewer than 3 responses, so its rating and comment are not shown here. Anele Dlamini sees nothing of it, not even that it answered.
### Release to Anele Dlamini
When you release:
Anele Dlamini can read your narrative and the groups shown above on MSF reports.
1 evidence record is written to Anele Dlamini's portfolio, for PAED-012.
Anele Dlamini is sent an email saying a report has been released, with a link to MSF reports. decided, T367
It cannot be undone.
Narrative
[aria-describedby: 0 of 4,000 characters NEW: counter]
0 of 4,000 characters NEW: counter
Supervision level this feedback supports
[aria-describedby: Optional, and blank is a perfectly good answer. Your own judgement as the releasing clinician, informed by the aggregate above — never an average of respondent scores.]
Not stated
1
2
3a
3b
4
5
Optional, and blank is a perfectly good answer. Your own judgement as the releasing clinician, informed by the aggregate above — never an average of respondent scores.
[aria-describedby: Release to trainee: Release needs 5 responses; 1 has come back. If it should never be released, withdraw it on its campaign page .] [aria-disabled] Release to trainee…
Release to trainee: Release needs 5 responses; 1 has come back. If it should never be released, withdraw it on its campaign page.

######## BOARD R3-R8a-blocked-responses-768.dc.html — Report · blocked: too few responses, Anele Dlamini's campaign (R5b, F10) · 768
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
Anele Dlamini's campaign: one invitee, a Consultant, 1 response; thresholds 5 / 3 / 2 as the old-frame captures read them (F10 gave ‹n›).
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF report for Anele Dlamini
## MSF report for Anele Dlamini
Default MSF · Multi-source feedback · ‹opens› to ‹closes›
Open its campaign page
Under review
### Not ready to release.
1 response, of the 5 needed.
0 respondent groups reporting, of the 2 needed.
A respondent group reports once 3 of it have responded; a group with fewer is suppressed.
#### Evidence for
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
One evidence record per EPA is written to the trainee's portfolio when the report is released. DATA: will be recorded
### By respondent group
As Anele Dlamini will read it once released. Ratings are averaged from 1, Well below expectations, to 5, Well above expectations.
Suppressed: Consultant. Fewer than 3 responses, so its rating and comment are not shown here. Anele Dlamini sees nothing of it, not even that it answered.
### Release to Anele Dlamini
When you release:
Anele Dlamini can read your narrative and the groups shown above on MSF reports.
1 evidence record is written to Anele Dlamini's portfolio, for PAED-012.
Anele Dlamini is sent an email saying a report has been released, with a link to MSF reports. decided, T367
It cannot be undone.
Narrative
[aria-describedby: 0 of 4,000 characters NEW: counter]
0 of 4,000 characters NEW: counter
Supervision level this feedback supports
[aria-describedby: Optional, and blank is a perfectly good answer. Your own judgement as the releasing clinician, informed by the aggregate above — never an average of respondent scores.]
Not stated
1
2
3a
3b
4
5
Optional, and blank is a perfectly good answer. Your own judgement as the releasing clinician, informed by the aggregate above — never an average of respondent scores.
[aria-describedby: Release to trainee: Release needs 5 responses; 1 has come back. If it should never be released, withdraw it on its campaign page .] [aria-disabled] Release to trainee…
Release to trainee: Release needs 5 responses; 1 has come back. If it should never be released, withdraw it on its campaign page.

######## BOARD R3-R8a-blocked-responses-390.dc.html — Report · blocked: too few responses, Anele Dlamini's campaign (R5b, F10) · 390
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
Anele Dlamini's campaign: one invitee, a Consultant, 1 response; thresholds 5 / 3 / 2 as the old-frame captures read them (F10 gave ‹n›).
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF report for Anele Dlamini
## MSF report for Anele Dlamini
Default MSF · Multi-source feedback · ‹opens› to ‹closes›
Open its campaign page
Under review
### Not ready to release.
1 response, of the 5 needed.
0 respondent groups reporting, of the 2 needed.
A respondent group reports once 3 of it have responded; a group with fewer is suppressed.
#### Evidence for
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
One evidence record per EPA is written to the trainee's portfolio when the report is released. DATA: will be recorded
### By respondent group
As Anele Dlamini will read it once released. Ratings are averaged from 1, Well below expectations, to 5, Well above expectations.
Suppressed: Consultant. Fewer than 3 responses, so its rating and comment are not shown here. Anele Dlamini sees nothing of it, not even that it answered.
### Release to Anele Dlamini
When you release:
Anele Dlamini can read your narrative and the groups shown above on MSF reports.
1 evidence record is written to Anele Dlamini's portfolio, for PAED-012.
Anele Dlamini is sent an email saying a report has been released, with a link to MSF reports. decided, T367
It cannot be undone.
Narrative
[aria-describedby: 0 of 4,000 characters NEW: counter]
0 of 4,000 characters NEW: counter
Supervision level this feedback supports
[aria-describedby: Optional, and blank is a perfectly good answer. Your own judgement as the releasing clinician, informed by the aggregate above — never an average of respondent scores.]
Not stated
1
2
3a
3b
4
5
Optional, and blank is a perfectly good answer. Your own judgement as the releasing clinician, informed by the aggregate above — never an average of respondent scores.
[aria-describedby: Release to trainee: Release needs 5 responses; 1 has come back. If it should never be released, withdraw it on its campaign page .] [aria-disabled] Release to trainee…
Release to trainee: Release needs 5 responses; 1 has come back. If it should never be released, withdraw it on its campaign page.

######## BOARD R3-R8b-blocked-groups-1280.dc.html — Report · blocked: too few respondent groups (R5b, Illustration) · 1280
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
Illustration Enough responses, but only one group cleared the threshold.
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF report for Lerato Molefe
## MSF report for Lerato Molefe
Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28
Open its campaign page
Under review
### Not ready to release.
‹n› responses, of the 5 needed.
1 respondent group reporting, of the 2 needed.
A respondent group reports once 3 of it have responded; a group with fewer is suppressed.
#### Evidence for
PAED-010 — Leading and operating within a clinical team
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
One evidence record per EPA is written to the trainee's portfolio when the report is released. DATA: will be recorded
### By respondent group
As Lerato Molefe will read it once released. Ratings are averaged from 1, Well below expectations, to 5, Well above expectations.
#### ‹group› · ‹n.nn› average rating, from ‹n› responses
What should the trainee keep doing or improve?
‹comment›
‹comment›
‹comment›
Suppressed: ‹groups›. Fewer than 3 responses each, so their ratings and comments are not shown here. Lerato Molefe sees nothing of them, not even that they answered.
### Release to Lerato Molefe
When you release:
Lerato Molefe can read your narrative and the groups shown above on MSF reports.
2 evidence records are written to Lerato Molefe's portfolio, for PAED-010 and PAED-012.
Lerato Molefe is sent an email saying a report has been released, with a link to MSF reports. decided, T367
It cannot be undone.
Narrative
[aria-describedby: 0 of 4,000 characters NEW: counter]
0 of 4,000 characters NEW: counter
Supervision level this feedback supports
[aria-describedby: Optional, and blank is a perfectly good answer. Your own judgement as the releasing clinician, informed by the aggregate above — never an average of respondent scores.]
Not stated
1
2
3a
3b
4
5
Optional, and blank is a perfectly good answer. Your own judgement as the releasing clinician, informed by the aggregate above — never an average of respondent scores.
[aria-describedby: Release to trainee: Release needs 2 respondent groups to report; 1 cleared the 3-response threshold. Feedback that survives from too few groups is not multi-source feedback.] [aria-disabled] Release to trainee…
Release to trainee: Release needs 2 respondent groups to report; 1 cleared the 3-response threshold. Feedback that survives from too few groups is not multi-source feedback.

######## BOARD R3-R8b-blocked-groups-390.dc.html — Report · blocked: too few respondent groups (R5b, Illustration) · 390
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
Illustration Enough responses, but only one group cleared the threshold.
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF report for Lerato Molefe
## MSF report for Lerato Molefe
Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28
Open its campaign page
Under review
### Not ready to release.
‹n› responses, of the 5 needed.
1 respondent group reporting, of the 2 needed.
A respondent group reports once 3 of it have responded; a group with fewer is suppressed.
#### Evidence for
PAED-010 — Leading and operating within a clinical team
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
One evidence record per EPA is written to the trainee's portfolio when the report is released. DATA: will be recorded
### By respondent group
As Lerato Molefe will read it once released. Ratings are averaged from 1, Well below expectations, to 5, Well above expectations.
#### ‹group› · ‹n.nn› average rating, from ‹n› responses
What should the trainee keep doing or improve?
‹comment›
‹comment›
‹comment›
Suppressed: ‹groups›. Fewer than 3 responses each, so their ratings and comments are not shown here. Lerato Molefe sees nothing of them, not even that they answered.
### Release to Lerato Molefe
When you release:
Lerato Molefe can read your narrative and the groups shown above on MSF reports.
2 evidence records are written to Lerato Molefe's portfolio, for PAED-010 and PAED-012.
Lerato Molefe is sent an email saying a report has been released, with a link to MSF reports. decided, T367
It cannot be undone.
Narrative
[aria-describedby: 0 of 4,000 characters NEW: counter]
0 of 4,000 characters NEW: counter
Supervision level this feedback supports
[aria-describedby: Optional, and blank is a perfectly good answer. Your own judgement as the releasing clinician, informed by the aggregate above — never an average of respondent scores.]
Not stated
1
2
3a
3b
4
5
Optional, and blank is a perfectly good answer. Your own judgement as the releasing clinician, informed by the aggregate above — never an average of respondent scores.
[aria-describedby: Release to trainee: Release needs 2 respondent groups to report; 1 cleared the 3-response threshold. Feedback that survives from too few groups is not multi-source feedback.] [aria-disabled] Release to trainee…
Release to trainee: Release needs 2 respondent groups to report; 1 cleared the 3-response threshold. Feedback that survives from too few groups is not multi-source feedback.

######## BOARD R3-R9-no-epa-1280.dc.html — Report · a campaign that names no EPA, Anele Dlamini's (R5e, F10) · 1280
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
Anele Dlamini's campaign: one invitee, a Consultant, 1 response; thresholds 5 / 3 / 2 as the old-frame captures read them (F10 gave ‹n›). No EPA named: the records line drops.
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF report for Anele Dlamini
## MSF report for Anele Dlamini
Default MSF · Multi-source feedback · ‹opens› to ‹closes›
Open its campaign page
Under review
### Not ready to release.
1 response, of the 5 needed.
0 respondent groups reporting, of the 2 needed.
A respondent group reports once 3 of it have responded; a group with fewer is suppressed.
#### Evidence for
This campaign names no EPA, so releasing it will record no evidence on the trainee's portfolio.
### By respondent group
As Anele Dlamini will read it once released. Ratings are averaged from 1, Well below expectations, to 5, Well above expectations.
Suppressed: Consultant. Fewer than 3 responses, so its rating and comment are not shown here. Anele Dlamini sees nothing of it, not even that it answered.
### Release to Anele Dlamini
When you release:
Anele Dlamini can read your narrative and the groups shown above on MSF reports.
Anele Dlamini is sent an email saying a report has been released, with a link to MSF reports. decided, T367
It cannot be undone.
Narrative
[aria-describedby: 0 of 4,000 characters NEW: counter]
0 of 4,000 characters NEW: counter
Supervision level this feedback supports
[aria-describedby: Optional, and blank is a perfectly good answer. Your own judgement as the releasing clinician, informed by the aggregate above — never an average of respondent scores.]
Not stated
1
2
3a
3b
4
5
Optional, and blank is a perfectly good answer. Your own judgement as the releasing clinician, informed by the aggregate above — never an average of respondent scores.
[aria-describedby: Release to trainee: Release needs 5 responses; 1 has come back. If it should never be released, withdraw it on its campaign page .] [aria-disabled] Release to trainee…
Release to trainee: Release needs 5 responses; 1 has come back. If it should never be released, withdraw it on its campaign page.

######## BOARD R3-R9-no-epa-390.dc.html — Report · a campaign that names no EPA, Anele Dlamini's (R5e, F10) · 390
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
Anele Dlamini's campaign: one invitee, a Consultant, 1 response; thresholds 5 / 3 / 2 as the old-frame captures read them (F10 gave ‹n›). No EPA named: the records line drops.
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF report for Anele Dlamini
## MSF report for Anele Dlamini
Default MSF · Multi-source feedback · ‹opens› to ‹closes›
Open its campaign page
Under review
### Not ready to release.
1 response, of the 5 needed.
0 respondent groups reporting, of the 2 needed.
A respondent group reports once 3 of it have responded; a group with fewer is suppressed.
#### Evidence for
This campaign names no EPA, so releasing it will record no evidence on the trainee's portfolio.
### By respondent group
As Anele Dlamini will read it once released. Ratings are averaged from 1, Well below expectations, to 5, Well above expectations.
Suppressed: Consultant. Fewer than 3 responses, so its rating and comment are not shown here. Anele Dlamini sees nothing of it, not even that it answered.
### Release to Anele Dlamini
When you release:
Anele Dlamini can read your narrative and the groups shown above on MSF reports.
Anele Dlamini is sent an email saying a report has been released, with a link to MSF reports. decided, T367
It cannot be undone.
Narrative
[aria-describedby: 0 of 4,000 characters NEW: counter]
0 of 4,000 characters NEW: counter
Supervision level this feedback supports
[aria-describedby: Optional, and blank is a perfectly good answer. Your own judgement as the releasing clinician, informed by the aggregate above — never an average of respondent scores.]
Not stated
1
2
3a
3b
4
5
Optional, and blank is a perfectly good answer. Your own judgement as the releasing clinician, informed by the aggregate above — never an average of respondent scores.
[aria-describedby: Release to trainee: Release needs 5 responses; 1 has come back. If it should never be released, withdraw it on its campaign page .] [aria-disabled] Release to trainee…
Release to trainee: Release needs 5 responses; 1 has come back. If it should never be released, withdraw it on its campaign page.

######## BOARD R3-R10-programme-ended-1280.dc.html — Report · the registrar's programme has ended (R4g, Q10, M5, Illustration) · 1280
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
Illustration The release is kept; no email is sent (F4).
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF report for Lerato Molefe
## MSF report for Lerato Molefe
Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28
Open its campaign page
Under review
### Ready to release: every threshold is met.
Lerato Molefe's programme ended on ‹date›. A release still records the feedback as evidence on her record, but she can no longer open MSF reports to read it. DATA: SubjectIsCurrent, SubjectEndedOn
8 responses, of the 5 needed.
2 respondent groups reporting, of the 2 needed.
A respondent group reports once 3 of it have responded; a group with fewer is suppressed.
#### Evidence for
PAED-010 — Leading and operating within a clinical team
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
One evidence record per EPA is written to the trainee's portfolio when the report is released. DATA: will be recorded
### By respondent group
As Lerato Molefe would have read it. Ratings are averaged from 1, Well below expectations, to 5, Well above expectations.
#### Consultant · ‹n.nn› average rating, from 3 responses
What should the trainee keep doing or improve?
Sound judgement in clinic; teaches the interns well
Ready to run a ward; should document escalation decisions more fully
‹the third consultant's comment›
#### Nurse · ‹n.nn› average rating, from 3 responses
What should the trainee keep doing or improve?
Calm and kind with parents at night; hands over clearly
Leads neonatal resuscitations and explains her decisions to the nurses
‹the third nurse's comment›
Suppressed: Peer doctor, Allied health professional. Fewer than 3 responses each, so their ratings and comments are not shown here. Lerato Molefe sees nothing of them, not even that they answered.
### Release to Lerato Molefe
When you release:
She can no longer open MSF reports; the report is kept with the evidence on her record.
2 evidence records are written to Lerato Molefe's portfolio, for PAED-010 and PAED-012.
No email is sent: she can no longer open MSF reports.
It cannot be undone.
Narrative
[aria-describedby: 106 of 4,000 characters NEW: counter] Consistently strong feedback from nursing and consultant colleagues; keep documenting escalation decisions
106 of 4,000 characters NEW: counter
Supervision level this feedback supports
[aria-describedby: Optional, and blank is a perfectly good answer. Your own judgement as the releasing clinician, informed by the aggregate above — never an average of respondent scores.]
Not stated
1
2
3a
3b
4
5
Optional, and blank is a perfectly good answer. Your own judgement as the releasing clinician, informed by the aggregate above — never an average of respondent scores.
Release to trainee…
If it should never be released, withdraw it on its campaign page.

######## BOARD R3-R10-programme-ended-390.dc.html — Report · the registrar's programme has ended (R4g, Q10, M5, Illustration) · 390
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
Illustration The release is kept; no email is sent (F4).
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF report for Lerato Molefe
## MSF report for Lerato Molefe
Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28
Open its campaign page
Under review
### Ready to release: every threshold is met.
Lerato Molefe's programme ended on ‹date›. A release still records the feedback as evidence on her record, but she can no longer open MSF reports to read it. DATA: SubjectIsCurrent, SubjectEndedOn
8 responses, of the 5 needed.
2 respondent groups reporting, of the 2 needed.
A respondent group reports once 3 of it have responded; a group with fewer is suppressed.
#### Evidence for
PAED-010 — Leading and operating within a clinical team
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
One evidence record per EPA is written to the trainee's portfolio when the report is released. DATA: will be recorded
### By respondent group
As Lerato Molefe would have read it. Ratings are averaged from 1, Well below expectations, to 5, Well above expectations.
#### Consultant · ‹n.nn› average rating, from 3 responses
What should the trainee keep doing or improve?
Sound judgement in clinic; teaches the interns well
Ready to run a ward; should document escalation decisions more fully
‹the third consultant's comment›
#### Nurse · ‹n.nn› average rating, from 3 responses
What should the trainee keep doing or improve?
Calm and kind with parents at night; hands over clearly
Leads neonatal resuscitations and explains her decisions to the nurses
‹the third nurse's comment›
Suppressed: Peer doctor, Allied health professional. Fewer than 3 responses each, so their ratings and comments are not shown here. Lerato Molefe sees nothing of them, not even that they answered.
### Release to Lerato Molefe
When you release:
She can no longer open MSF reports; the report is kept with the evidence on her record.
2 evidence records are written to Lerato Molefe's portfolio, for PAED-010 and PAED-012.
No email is sent: she can no longer open MSF reports.
It cannot be undone.
Narrative
[aria-describedby: 106 of 4,000 characters NEW: counter] Consistently strong feedback from nursing and consultant colleagues; keep documenting escalation decisions
106 of 4,000 characters NEW: counter
Supervision level this feedback supports
[aria-describedby: Optional, and blank is a perfectly good answer. Your own judgement as the releasing clinician, informed by the aggregate above — never an average of respondent scores.]
Not stated
1
2
3a
3b
4
5
Optional, and blank is a perfectly good answer. Your own judgement as the releasing clinician, informed by the aggregate above — never an average of respondent scores.
Release to trainee…
If it should never be released, withdraw it on its campaign page.

######## BOARD R3-R11-dozen-1280.dc.html — Report · a respondent group with a dozen comments, one of 4,000 characters (R10, Illustration) · 1280
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
Illustration Twelve nurses, three consultants, one peer doctor and one allied health professional answered: 17. The twelfth comment is 4,000 characters.
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF report for Lerato Molefe
## MSF report for Lerato Molefe
Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28
Open its campaign page
Under review
### Ready to release: every threshold is met.
17 responses, of the 5 needed.
2 respondent groups reporting, of the 2 needed.
A respondent group reports once 3 of it have responded; a group with fewer is suppressed.
#### Evidence for
PAED-010 — Leading and operating within a clinical team
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
One evidence record per EPA is written to the trainee's portfolio when the report is released. DATA: will be recorded
### By respondent group
As Lerato Molefe will read it once released. Ratings are averaged from 1, Well below expectations, to 5, Well above expectations.
#### Consultant · ‹n.nn› average rating, from 3 responses
What should the trainee keep doing or improve?
Sound judgement in clinic; teaches the interns well
Ready to run a ward; should document escalation decisions more fully
‹the third consultant's comment›
#### Nurse · ‹n.nn› average rating, from 12 responses
What should the trainee keep doing or improve?
‹comment 1›
‹comment 2›
‹comment 3›
‹comment 4›
‹comment 5›
‹comment 6›
‹comment 7›
‹comment 8›
‹comment 9›
‹comment 10›
‹comment 11›
‹One respondent's comment of up to 4,000 characters, shown whole and wrapping inside its line, never cut and never ended with an ellipsis.› ‹One respondent's comment of up to 4,000 characters, shown whole and wrapping inside its line, never cut and never ended with an ellipsis.› ‹One respondent's comment of up to 4,000 characters, shown whole and wrapping inside its line, never cut and never ended with an ellipsis.› ‹One respondent's comment of up to 4,000 characters, shown whole and wrapping inside its line, never cut and never ended with an ellipsis.› ‹One respondent's comment of up to 4,000 characters, shown whole and wrapping inside its line, never cut and never ended with an ellipsis.› ‹One respondent's comment of up to 4,000 characters, shown whole and wrapping inside its line, never cut and never ended with an ellipsis.› ‹One respondent's comment of up to 4,000 characters, shown whole and wrapping inside its line, never cut and never ended with an ellipsis.› ‹One respondent's comment of up to 4,000 characters, shown whole and wrapping inside its line, never cut and never ended with an ellipsis.› ‹One respondent's comment of up to 4,000 characters, shown whole and wrapping inside its line, never cut and never ended with an ellipsis.› ‹One respondent's comment of up to 4,000 characters, shown whole and wrapping inside its line, never cut and never ended with an ellipsis.› ‹One respondent's comment of up to 4,000 characters, shown whole and wrapping inside its line, never cut and never ended with an ellipsis.› ‹One respondent's comment of up to 4,000 characters, shown whole and wrapping inside its line, never cut and never ended with an ellipsis.› ‹One respondent's comment of up to 4,000 characters, shown whole and wrapping inside its line, never cut and never ended with an ellipsis.› ‹One respondent's comment of up to 4,000 characters, shown whole and wrapping inside its line, never cut and never ended with an ellipsis.› ‹One respondent's comment of up to 4,000 characters, shown whole and wrapping inside its line, never cut and never ended with an ellipsis.› ‹One respondent's comment of up to 4,000 characters, shown whole and wrapping inside its line, never cut and never ended with an ellipsis.› ‹One respondent's comment of up to 4,000 characters, shown whole and wrapping inside its line, never cut and never ended with an ellipsis.› ‹One respondent's comment of up to 4,000 characters, shown whole and wrapping inside its line, never cut and never ended with an ellipsis.› ‹One respondent's comment of up to 4,000 characters, shown whole and wrapping inside its line, never cut and never ended with an ellipsis.› ‹One respondent's comment of up to 4,000 characters, shown whole and wrapping inside its line, never cut and never ended with an ellipsis.› ‹One respondent's comment of up to 4,000 characters, shown whole and wrapping inside its line, never cut and never ended with an ellipsis.› ‹One respondent's comment of up to 4,000 characters, shown whole and wrapping inside its line, never cut and never ended with an ellipsis.› ‹One respondent's comment of up to 4,000 characters, shown whole and wrapping inside its line, never cut and never ended with an ellipsis.› ‹One respondent's comment of up to 4,000 characters, shown whole and wrapping inside its line, never cut and never ended with an ellipsis.› ‹One respondent's comment of up to 4,000 characters, shown whole and wrapping inside its line, never cut and never ended with an ellipsis.› ‹One respondent's comment of up to 4,000 characters, shown whole and wrapping inside its line, never cut and never ended with an ellipsis.› ‹One respondent's comment of up to 4,000 characters, shown whole and wrapping inside its line, never cut and never ended with an ellipsis.›
Suppressed: Peer doctor, Allied health professional. Fewer than 3 responses each, so their ratings and comments are not shown here. Lerato Molefe sees nothing of them, not even that they answered.
### Release to Lerato Molefe
When you release:
Lerato Molefe can read your narrative and the groups shown above on MSF reports.
2 evidence records are written to Lerato Molefe's portfolio, for PAED-010 and PAED-012.
Lerato Molefe is sent an email saying a report has been released, with a link to MSF reports. decided, T367
It cannot be undone.
Narrative
[aria-describedby: 0 of 4,000 characters NEW: counter]
0 of 4,000 characters NEW: counter
Supervision level this feedback supports
[aria-describedby: Optional, and blank is a perfectly good answer. Your own judgement as the releasing clinician, informed by the aggregate above — never an average of respondent scores.]
Not stated
1
2
3a
3b
4
5
Optional, and blank is a perfectly good answer. Your own judgement as the releasing clinician, informed by the aggregate above — never an average of respondent scores.
Release to trainee…
If it should never be released, withdraw it on its campaign page.

######## BOARD R3-R11-dozen-390.dc.html — Report · a respondent group with a dozen comments, one of 4,000 characters (R10, Illustration) · 390
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
Illustration Twelve nurses, three consultants, one peer doctor and one allied health professional answered: 17. The twelfth comment is 4,000 characters.
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF report for Lerato Molefe
## MSF report for Lerato Molefe
Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28
Open its campaign page
Under review
### Ready to release: every threshold is met.
17 responses, of the 5 needed.
2 respondent groups reporting, of the 2 needed.
A respondent group reports once 3 of it have responded; a group with fewer is suppressed.
#### Evidence for
PAED-010 — Leading and operating within a clinical team
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
One evidence record per EPA is written to the trainee's portfolio when the report is released. DATA: will be recorded
### By respondent group
As Lerato Molefe will read it once released. Ratings are averaged from 1, Well below expectations, to 5, Well above expectations.
#### Consultant · ‹n.nn› average rating, from 3 responses
What should the trainee keep doing or improve?
Sound judgement in clinic; teaches the interns well
Ready to run a ward; should document escalation decisions more fully
‹the third consultant's comment›
#### Nurse · ‹n.nn› average rating, from 12 responses
What should the trainee keep doing or improve?
‹comment 1›
‹comment 2›
‹comment 3›
‹comment 4›
‹comment 5›
‹comment 6›
‹comment 7›
‹comment 8›
‹comment 9›
‹comment 10›
‹comment 11›
‹One respondent's comment of up to 4,000 characters, shown whole and wrapping inside its line, never cut and never ended with an ellipsis.› ‹One respondent's comment of up to 4,000 characters, shown whole and wrapping inside its line, never cut and never ended with an ellipsis.› ‹One respondent's comment of up to 4,000 characters, shown whole and wrapping inside its line, never cut and never ended with an ellipsis.› ‹One respondent's comment of up to 4,000 characters, shown whole and wrapping inside its line, never cut and never ended with an ellipsis.› ‹One respondent's comment of up to 4,000 characters, shown whole and wrapping inside its line, never cut and never ended with an ellipsis.› ‹One respondent's comment of up to 4,000 characters, shown whole and wrapping inside its line, never cut and never ended with an ellipsis.› ‹One respondent's comment of up to 4,000 characters, shown whole and wrapping inside its line, never cut and never ended with an ellipsis.› ‹One respondent's comment of up to 4,000 characters, shown whole and wrapping inside its line, never cut and never ended with an ellipsis.› ‹One respondent's comment of up to 4,000 characters, shown whole and wrapping inside its line, never cut and never ended with an ellipsis.› ‹One respondent's comment of up to 4,000 characters, shown whole and wrapping inside its line, never cut and never ended with an ellipsis.› ‹One respondent's comment of up to 4,000 characters, shown whole and wrapping inside its line, never cut and never ended with an ellipsis.› ‹One respondent's comment of up to 4,000 characters, shown whole and wrapping inside its line, never cut and never ended with an ellipsis.› ‹One respondent's comment of up to 4,000 characters, shown whole and wrapping inside its line, never cut and never ended with an ellipsis.› ‹One respondent's comment of up to 4,000 characters, shown whole and wrapping inside its line, never cut and never ended with an ellipsis.› ‹One respondent's comment of up to 4,000 characters, shown whole and wrapping inside its line, never cut and never ended with an ellipsis.› ‹One respondent's comment of up to 4,000 characters, shown whole and wrapping inside its line, never cut and never ended with an ellipsis.› ‹One respondent's comment of up to 4,000 characters, shown whole and wrapping inside its line, never cut and never ended with an ellipsis.› ‹One respondent's comment of up to 4,000 characters, shown whole and wrapping inside its line, never cut and never ended with an ellipsis.› ‹One respondent's comment of up to 4,000 characters, shown whole and wrapping inside its line, never cut and never ended with an ellipsis.› ‹One respondent's comment of up to 4,000 characters, shown whole and wrapping inside its line, never cut and never ended with an ellipsis.› ‹One respondent's comment of up to 4,000 characters, shown whole and wrapping inside its line, never cut and never ended with an ellipsis.› ‹One respondent's comment of up to 4,000 characters, shown whole and wrapping inside its line, never cut and never ended with an ellipsis.› ‹One respondent's comment of up to 4,000 characters, shown whole and wrapping inside its line, never cut and never ended with an ellipsis.› ‹One respondent's comment of up to 4,000 characters, shown whole and wrapping inside its line, never cut and never ended with an ellipsis.› ‹One respondent's comment of up to 4,000 characters, shown whole and wrapping inside its line, never cut and never ended with an ellipsis.› ‹One respondent's comment of up to 4,000 characters, shown whole and wrapping inside its line, never cut and never ended with an ellipsis.› ‹One respondent's comment of up to 4,000 characters, shown whole and wrapping inside its line, never cut and never ended with an ellipsis.›
Suppressed: Peer doctor, Allied health professional. Fewer than 3 responses each, so their ratings and comments are not shown here. Lerato Molefe sees nothing of them, not even that they answered.
### Release to Lerato Molefe
When you release:
Lerato Molefe can read your narrative and the groups shown above on MSF reports.
2 evidence records are written to Lerato Molefe's portfolio, for PAED-010 and PAED-012.
Lerato Molefe is sent an email saying a report has been released, with a link to MSF reports. decided, T367
It cannot be undone.
Narrative
[aria-describedby: 0 of 4,000 characters NEW: counter]
0 of 4,000 characters NEW: counter
Supervision level this feedback supports
[aria-describedby: Optional, and blank is a perfectly good answer. Your own judgement as the releasing clinician, informed by the aggregate above — never an average of respondent scores.]
Not stated
1
2
3a
3b
4
5
Optional, and blank is a perfectly good answer. Your own judgement as the releasing clinician, informed by the aggregate above — never an average of respondent scores.
Release to trainee…
If it should never be released, withdraw it on its campaign page.

######## BOARD R3-R12-unavailable-1280.dc.html — Report · unavailable · 1280
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
The sentence follows the campaign page's (the source names only the title, "Report unavailable").
[aria-label: Breadcrumb]
Home
MSF campaigns
Report unavailable
## Report unavailable
The selected report could not be loaded.
Go to MSF campaigns NEW

######## BOARD R3-R12-unavailable-390.dc.html — Report · unavailable · 390
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
The sentence follows the campaign page's (the source names only the title, "Report unavailable").
[aria-label: Breadcrumb]
Home
MSF campaigns
Report unavailable
## Report unavailable
The selected report could not be loaded.
Go to MSF campaigns NEW

######## BOARD R3-R13-loading-1280.dc.html — Report · loading · 1280
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF report
## MSF report
Loading the report.

######## BOARD R3-R13-loading-390.dc.html — Report · loading · 390
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF report
## MSF report
Loading the report.

######## BOARD R3-R14-error-1280.dc.html — Report · load error · 1280
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF report
## MSF report
Could not load this report. Nothing has changed. Try again, or come back in a few minutes.Try again

######## BOARD R3-R14-error-390.dc.html — Report · load error · 390
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF report
## MSF report
Could not load this report. Nothing has changed. Try again, or come back in a few minutes.Try again

######## BOARD R3-R15-fifteen-390.dc.html — Report · fifteen EPAs in Evidence for, PAED-015's title whole (S8, Illustration) · 390
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
Illustration A campaign that is evidence for all fifteen EPAs.
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF report for Lerato Molefe
## MSF report for Lerato Molefe
Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28
Open its campaign page
Under review
### Ready to release: every threshold is met.
8 responses, of the 5 needed.
2 respondent groups reporting, of the 2 needed.
A respondent group reports once 3 of it have responded; a group with fewer is suppressed.
#### Evidence for
PAED-001 — Providing paediatric emergency care to children
PAED-002 — Managing common paediatric presentations
PAED-003 — Providing intensive care to children
PAED-004 — Managing common neonatal conditions
PAED-005 — Providing neonatal care in intensive and high-care settings
PAED-006 — Managing long-term health conditions (LTHCs)
PAED-007 — Maintaining and promoting the health and well-being of children
PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
PAED-009 — Providing care for an adolescent patient
PAED-010 — Leading and operating within a clinical team
PAED-011 — Managing population health challenges
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
PAED-013 — Managing child protection, legal and ethical aspects of children at risk
PAED-014 — Providing palliative and end-of-life care
PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
One evidence record per EPA is written to the trainee's portfolio when the report is released. DATA: will be recorded

######## BOARD R3-R16-refused-close-1280.dc.html — Report · a refused Close: the danger Alert, the focus back on Close campaign… (S8) · 1280
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF report for Lerato Molefe
## MSF report for Lerato Molefe
Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28
Open its campaign page
Open
### Still taking responses, until 2026-10-28.
The responses are read once the campaign has closed and every address has been removed. F1
8 responses, of the 5 needed.
2 respondent groups reporting, of the 2 needed.
A respondent group reports once 3 of it have responded; a group with fewer is suppressed.
#### Evidence for
PAED-010 — Leading and operating within a clinical team
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
One evidence record per EPA is written to the trainee's portfolio when the report is released. DATA: will be recorded
Not closed. ‹The refusal in its own words, as built.›
Close campaign…
Release is offered once the campaign has closed. It closes by itself once 2026-10-28 has passed; close it sooner here.

######## BOARD R3-R16-refused-close-390.dc.html — Report · a refused Close: the danger Alert, the focus back on Close campaign… (S8) · 390
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF report for Lerato Molefe
## MSF report for Lerato Molefe
Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28
Open its campaign page
Open
### Still taking responses, until 2026-10-28.
The responses are read once the campaign has closed and every address has been removed. F1
8 responses, of the 5 needed.
2 respondent groups reporting, of the 2 needed.
A respondent group reports once 3 of it have responded; a group with fewer is suppressed.
#### Evidence for
PAED-010 — Leading and operating within a clinical team
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
One evidence record per EPA is written to the trainee's portfolio when the report is released. DATA: will be recorded
Not closed. ‹The refusal in its own words, as built.›
Close campaign…
Release is offered once the campaign has closed. It closes by itself once 2026-10-28 has passed; close it sooner here.

######## BOARD R3-R17-refused-release-1280.dc.html — Report · a refused Release: the danger Alert beside the button, the focus back on it (S8) · 1280
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF report for Lerato Molefe
## MSF report for Lerato Molefe
Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28
Open its campaign page
Under review
### Ready to release: every threshold is met.
8 responses, of the 5 needed.
2 respondent groups reporting, of the 2 needed.
A respondent group reports once 3 of it have responded; a group with fewer is suppressed.
#### Evidence for
PAED-010 — Leading and operating within a clinical team
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
One evidence record per EPA is written to the trainee's portfolio when the report is released. DATA: will be recorded
### By respondent group
As Lerato Molefe will read it once released. Ratings are averaged from 1, Well below expectations, to 5, Well above expectations.
#### Consultant · ‹n.nn› average rating, from 3 responses
What should the trainee keep doing or improve?
Sound judgement in clinic; teaches the interns well
Ready to run a ward; should document escalation decisions more fully
‹the third consultant's comment›
#### Nurse · ‹n.nn› average rating, from 3 responses
What should the trainee keep doing or improve?
Calm and kind with parents at night; hands over clearly
Leads neonatal resuscitations and explains her decisions to the nurses
‹the third nurse's comment›
Suppressed: Peer doctor, Allied health professional. Fewer than 3 responses each, so their ratings and comments are not shown here. Lerato Molefe sees nothing of them, not even that they answered.
### Release to Lerato Molefe
When you release:
Lerato Molefe can read your narrative and the groups shown above on MSF reports.
2 evidence records are written to Lerato Molefe's portfolio, for PAED-010 and PAED-012.
Lerato Molefe is sent an email saying a report has been released, with a link to MSF reports. decided, T367
It cannot be undone.
Narrative
[aria-describedby: 106 of 4,000 characters NEW: counter] Consistently strong feedback from nursing and consultant colleagues; keep documenting escalation decisions
106 of 4,000 characters NEW: counter
Supervision level this feedback supports
[aria-describedby: Optional, and blank is a perfectly good answer. Your own judgement as the releasing clinician, informed by the aggregate above — never an average of respondent scores.]
Not stated
1
2
3a
3b
4
5
Optional, and blank is a perfectly good answer. Your own judgement as the releasing clinician, informed by the aggregate above — never an average of respondent scores.
Not released. ‹The refusal in its own words, as built.› It is still Under review.
Release to trainee…
If it should never be released, withdraw it on its campaign page.

######## BOARD R3-R17-refused-release-390.dc.html — Report · a refused Release: the danger Alert beside the button, the focus back on it (S8) · 390
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF report for Lerato Molefe
## MSF report for Lerato Molefe
Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28
Open its campaign page
Under review
### Ready to release: every threshold is met.
8 responses, of the 5 needed.
2 respondent groups reporting, of the 2 needed.
A respondent group reports once 3 of it have responded; a group with fewer is suppressed.
#### Evidence for
PAED-010 — Leading and operating within a clinical team
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
One evidence record per EPA is written to the trainee's portfolio when the report is released. DATA: will be recorded
### By respondent group
As Lerato Molefe will read it once released. Ratings are averaged from 1, Well below expectations, to 5, Well above expectations.
#### Consultant · ‹n.nn› average rating, from 3 responses
What should the trainee keep doing or improve?
Sound judgement in clinic; teaches the interns well
Ready to run a ward; should document escalation decisions more fully
‹the third consultant's comment›
#### Nurse · ‹n.nn› average rating, from 3 responses
What should the trainee keep doing or improve?
Calm and kind with parents at night; hands over clearly
Leads neonatal resuscitations and explains her decisions to the nurses
‹the third nurse's comment›
Suppressed: Peer doctor, Allied health professional. Fewer than 3 responses each, so their ratings and comments are not shown here. Lerato Molefe sees nothing of them, not even that they answered.
### Release to Lerato Molefe
When you release:
Lerato Molefe can read your narrative and the groups shown above on MSF reports.
2 evidence records are written to Lerato Molefe's portfolio, for PAED-010 and PAED-012.
Lerato Molefe is sent an email saying a report has been released, with a link to MSF reports. decided, T367
It cannot be undone.
Narrative
[aria-describedby: 106 of 4,000 characters NEW: counter] Consistently strong feedback from nursing and consultant colleagues; keep documenting escalation decisions
106 of 4,000 characters NEW: counter
Supervision level this feedback supports
[aria-describedby: Optional, and blank is a perfectly good answer. Your own judgement as the releasing clinician, informed by the aggregate above — never an average of respondent scores.]
Not stated
1
2
3a
3b
4
5
Optional, and blank is a perfectly good answer. Your own judgement as the releasing clinician, informed by the aggregate above — never an average of respondent scores.
Not released. ‹The refusal in its own words, as built.› It is still Under review.
Release to trainee…
If it should never be released, withdraw it on its campaign page.

######## BOARD R3-Q1-questionnaire-1280.dc.html — /msf/respond · the questionnaire, a point chosen, Sister Mokwena (3.39) · 1280
Wombat
## Feedback on Lerato Molefe
Default MSF · Last day to respond: 2026-10-28
You have been asked to give multi-source feedback on Lerato Molefe, a trainee you have worked with.
Who sees your feedback
Your name and email address are never shown to Lerato Molefe.
Lerato Molefe sees the feedback only after the request has closed and a coordinator has reviewed and released it, grouped by respondent role.
Questions marked * must be answered.
[aria-describedby: For each rating, compare Lerato Molefe with what you would expect of a trainee at their stage of training.]
Rates the trainee's overall professional performance. * required
For each rating, compare Lerato Molefe with what you would expect of a trainee at their stage of training.
Well below expectations
Below expectations
Meets expectations
[checked] Above expectations
Well above expectations
What should the trainee keep doing or improve? (optional)
Up to 4,000 characters. Your comments may be shown to Lerato Molefe word for word, so leave out anything that would identify you.
[aria-describedby: Up to 4,000 characters. Your comments may be shown to Lerato Molefe word for word, so leave out anything that would identify you.] Calm and kind with parents at night; hands over clearly
Submit feedback

######## BOARD R3-Q1-questionnaire-390.dc.html — /msf/respond · the questionnaire, a point chosen, Sister Mokwena (3.39) · 390
Wombat
## Feedback on Lerato Molefe
Default MSF · Last day to respond: 2026-10-28
You have been asked to give multi-source feedback on Lerato Molefe, a trainee you have worked with.
Who sees your feedback
Your name and email address are never shown to Lerato Molefe.
Lerato Molefe sees the feedback only after the request has closed and a coordinator has reviewed and released it, grouped by respondent role.
Questions marked * must be answered.
[aria-describedby: For each rating, compare Lerato Molefe with what you would expect of a trainee at their stage of training.]
Rates the trainee's overall professional performance. * required
For each rating, compare Lerato Molefe with what you would expect of a trainee at their stage of training.
Well below expectations
Below expectations
Meets expectations
[checked] Above expectations
Well above expectations
What should the trainee keep doing or improve? (optional)
Up to 4,000 characters. Your comments may be shown to Lerato Molefe word for word, so leave out anything that would identify you.
[aria-describedby: Up to 4,000 characters. Your comments may be shown to Lerato Molefe word for word, so leave out anything that would identify you.] Calm and kind with parents at night; hands over clearly
Submit feedback

######## BOARD R3-Q2-required-1280.dc.html — /msf/respond · Submit with no rating: the browser's own required prompt · 1280
The browser draws this prompt (Chrome's words shown); the page adds no live validation and no script.
Wombat
## Feedback on Lerato Molefe
Default MSF · Last day to respond: 2026-10-28
You have been asked to give multi-source feedback on Lerato Molefe, a trainee you have worked with.
Who sees your feedback
Your name and email address are never shown to Lerato Molefe.
Lerato Molefe sees the feedback only after the request has closed and a coordinator has reviewed and released it, grouped by respondent role.
Questions marked * must be answered.
[aria-describedby: For each rating, compare Lerato Molefe with what you would expect of a trainee at their stage of training.]
Rates the trainee's overall professional performance. * required
For each rating, compare Lerato Molefe with what you would expect of a trainee at their stage of training.
Well below expectations
Below expectations
Meets expectations
Above expectations
Well above expectations
Please select one of these options.
What should the trainee keep doing or improve? (optional)
Up to 4,000 characters. Your comments may be shown to Lerato Molefe word for word, so leave out anything that would identify you.
[aria-describedby: Up to 4,000 characters. Your comments may be shown to Lerato Molefe word for word, so leave out anything that would identify you.]
Submit feedback

######## BOARD R3-Q2-required-390.dc.html — /msf/respond · Submit with no rating: the browser's own required prompt · 390
The browser draws this prompt (Chrome's words shown); the page adds no live validation and no script.
Wombat
## Feedback on Lerato Molefe
Default MSF · Last day to respond: 2026-10-28
You have been asked to give multi-source feedback on Lerato Molefe, a trainee you have worked with.
Who sees your feedback
Your name and email address are never shown to Lerato Molefe.
Lerato Molefe sees the feedback only after the request has closed and a coordinator has reviewed and released it, grouped by respondent role.
Questions marked * must be answered.
[aria-describedby: For each rating, compare Lerato Molefe with what you would expect of a trainee at their stage of training.]
Rates the trainee's overall professional performance. * required
For each rating, compare Lerato Molefe with what you would expect of a trainee at their stage of training.
Well below expectations
Below expectations
Meets expectations
Above expectations
Well above expectations
Please select one of these options.
What should the trainee keep doing or improve? (optional)
Up to 4,000 characters. Your comments may be shown to Lerato Molefe word for word, so leave out anything that would identify you.
[aria-describedby: Up to 4,000 characters. Your comments may be shown to Lerato Molefe word for word, so leave out anything that would identify you.]
Submit feedback

######## BOARD R3-Q9-refused-post-1280.dc.html — /msf/respond · a refused post: the error summary takes the focus (S8) · 1280
The tab reads "Error: Feedback on Lerato Molefe · Wombat"; #msf-error (tabindex -1, autofocus) takes the focus; the comment carries aria-invalid.
Wombat
## Feedback on Lerato Molefe
Default MSF · Last day to respond: 2026-10-28
You have been asked to give multi-source feedback on Lerato Molefe, a trainee you have worked with.
Who sees your feedback
Your name and email address are never shown to Lerato Molefe.
Lerato Molefe sees the feedback only after the request has closed and a coordinator has reviewed and released it, grouped by respondent role.
Your feedback was not recorded. ‹The refusal in its own words, as built: the comment is over 4,000 characters.›
Questions marked * must be answered.
[aria-describedby: For each rating, compare Lerato Molefe with what you would expect of a trainee at their stage of training.]
Rates the trainee's overall professional performance. * required
For each rating, compare Lerato Molefe with what you would expect of a trainee at their stage of training.
Well below expectations
Below expectations
Meets expectations
[checked] Above expectations
Well above expectations
What should the trainee keep doing or improve? (optional)
Up to 4,000 characters. Your comments may be shown to Lerato Molefe word for word, so leave out anything that would identify you.
[aria-describedby: Up to 4,000 characters. Your comments may be shown to Lerato Molefe word for word, so leave out anything that would identify you. / Your feedback was not recorded. ‹The refusal in its own words, as built: the comment is over 4,000 characters.›] ‹a comment over 4,000 characters›
Submit feedback

######## BOARD R3-Q9-refused-post-390.dc.html — /msf/respond · a refused post: the error summary takes the focus (S8) · 390
The tab reads "Error: Feedback on Lerato Molefe · Wombat"; #msf-error (tabindex -1, autofocus) takes the focus; the comment carries aria-invalid.
Wombat
## Feedback on Lerato Molefe
Default MSF · Last day to respond: 2026-10-28
You have been asked to give multi-source feedback on Lerato Molefe, a trainee you have worked with.
Who sees your feedback
Your name and email address are never shown to Lerato Molefe.
Lerato Molefe sees the feedback only after the request has closed and a coordinator has reviewed and released it, grouped by respondent role.
Your feedback was not recorded. ‹The refusal in its own words, as built: the comment is over 4,000 characters.›
Questions marked * must be answered.
[aria-describedby: For each rating, compare Lerato Molefe with what you would expect of a trainee at their stage of training.]
Rates the trainee's overall professional performance. * required
For each rating, compare Lerato Molefe with what you would expect of a trainee at their stage of training.
Well below expectations
Below expectations
Meets expectations
[checked] Above expectations
Well above expectations
What should the trainee keep doing or improve? (optional)
Up to 4,000 characters. Your comments may be shown to Lerato Molefe word for word, so leave out anything that would identify you.
[aria-describedby: Up to 4,000 characters. Your comments may be shown to Lerato Molefe word for word, so leave out anything that would identify you. / Your feedback was not recorded. ‹The refusal in its own words, as built: the comment is over 4,000 characters.›] ‹a comment over 4,000 characters›
Submit feedback

######## BOARD R3-Q3-thanks-1280.dc.html — /msf/respond · thank you (3.39, 3.42) · 1280
Wombat
## Thank you
Your feedback on Lerato Molefe has been recorded.
The link in your invitation has now been used, so it will not open the questionnaire again. You can close this page.

######## BOARD R3-Q3-thanks-390.dc.html — /msf/respond · thank you (3.39, 3.42) · 390
Wombat
## Thank you
Your feedback on Lerato Molefe has been recorded.
The link in your invitation has now been used, so it will not open the questionnaire again. You can close this page.

######## BOARD R3-Q4-used-1280.dc.html — /msf/respond · link already used (3.40) · 1280
Wombat
## Feedback link already used
This feedback link has already been used: a response was submitted through it, and each link takes one response.
You can close this page.

######## BOARD R3-Q4-used-390.dc.html — /msf/respond · link already used (3.40) · 390
Wombat
## Feedback link already used
This feedback link has already been used: a response was submitted through it, and each link takes one response.
You can close this page.

######## BOARD R3-Q5-closed-1280.dc.html — /msf/respond · request closed (3.45) · 1280
Wombat
## Feedback request closed
This feedback request has closed and is no longer accepting responses.
You can close this page.

######## BOARD R3-Q5-closed-390.dc.html — /msf/respond · request closed (3.45) · 390
Wombat
## Feedback request closed
This feedback request has closed and is no longer accepting responses.
You can close this page.

######## BOARD R3-Q6-expired-1280.dc.html — /msf/respond · link expired · 1280
Wombat
## Feedback link expired
This feedback link has expired, so it can no longer be used.
You can close this page.

######## BOARD R3-Q6-expired-390.dc.html — /msf/respond · link expired · 390
Wombat
## Feedback link expired
This feedback link has expired, so it can no longer be used.
You can close this page.

######## BOARD R3-Q7-not-recognised-1280.dc.html — /msf/respond · link not recognised · 1280
Wombat
## Feedback link not recognised
This feedback link is not recognised. If you were sent a reminder about this request, use the link in the most recent email: it opens the questionnaire, or says whether your feedback has already been recorded.
You can close this page.

######## BOARD R3-Q7-not-recognised-390.dc.html — /msf/respond · link not recognised · 390
Wombat
## Feedback link not recognised
This feedback link is not recognised. If you were sent a reminder about this request, use the link in the most recent email: it opens the questionnaire, or says whether your feedback has already been recorded.
You can close this page.

######## BOARD R3-Q8-fault-1280.dc.html — /msf/respond · something went wrong · 1280
Wombat
## Something went wrong
Something went wrong on our side, and this page could not be shown. Please try again in a few minutes. If you had submitted the form, opening your link again will say whether your feedback was recorded.
You can close this page.

######## BOARD R3-Q8-fault-390.dc.html — /msf/respond · something went wrong · 390
Wombat
## Something went wrong
Something went wrong on our side, and this page could not be shown. Please try again in a few minutes. If you had submitted the form, opening your link again will say whether your feedback was recorded.
You can close this page.

######## BOARD R3-M1-none-1280.dc.html — MSF reports · none released · 1280
Wombat
Acting as Trainee
MenuClose
[aria-label: Main]
Acting as Trainee
Home
Log an activity
My activities
MSF reports
My committee reviews
Export portfolio
My progress
My data rights
[title: Lerato Molefe] Lerato Molefe
Sign out
## MSF reports
Released aggregate feedback only. Individual responses are never shown.
No released reports
Released multi-source feedback will appear here.

######## BOARD R3-M1-none-390.dc.html — MSF reports · none released · 390
Wombat
Acting as Trainee
MenuClose
[aria-label: Main]
Acting as Trainee
Home
Log an activity
My activities
MSF reports
My committee reviews
Export portfolio
My progress
My data rights
[title: Lerato Molefe] Lerato Molefe
Sign out
## MSF reports
Released aggregate feedback only. Individual responses are never shown.
No released reports
Released multi-source feedback will appear here.

######## BOARD R3-M2-list-1280.dc.html — MSF reports · the list: Report, Released, View (3.47, F3) · 1280
Wombat
Acting as Trainee
MenuClose
[aria-label: Main]
Acting as Trainee
Home
Log an activity
My activities
MSF reports
My committee reviews
Export portfolio
My progress
My data rights
[title: Lerato Molefe] Lerato Molefe
Sign out
## MSF reports
Released aggregate feedback only. Individual responses are never shown.
### Your released reports
Your released MSF reports, newest first
| Report
| Released
| Actions
| Default MSFMulti-source feedback
| 2026-10-14 16:34 SAST
[aria-label: View the Default MSF report, released 2026-10-14] View

######## BOARD R3-M2-list-768.dc.html — MSF reports · the list: Report, Released, View (3.47, F3) · 768
Wombat
Acting as Trainee
MenuClose
[aria-label: Main]
Acting as Trainee
Home
Log an activity
My activities
MSF reports
My committee reviews
Export portfolio
My progress
My data rights
[title: Lerato Molefe] Lerato Molefe
Sign out
## MSF reports
Released aggregate feedback only. Individual responses are never shown.
### Your released reports
Your released MSF reports, newest first
| Report
| Released
| Actions
| Default MSFMulti-source feedback
| 2026-10-14 16:34 SAST
[aria-label: View the Default MSF report, released 2026-10-14] View

######## BOARD R3-M2-list-390.dc.html — MSF reports · the list: Report, Released, View (3.47, F3) · 390
Wombat
Acting as Trainee
MenuClose
[aria-label: Main]
Acting as Trainee
Home
Log an activity
My activities
MSF reports
My committee reviews
Export portfolio
My progress
My data rights
[title: Lerato Molefe] Lerato Molefe
Sign out
## MSF reports
Released aggregate feedback only. Individual responses are never shown.
### Your released reports
Your released MSF reports, newest first
| Report
| Released
| Actions
| Default MSFMulti-source feedback
| 2026-10-14 16:34 SAST
[aria-label: View the Default MSF report, released 2026-10-14] View

######## BOARD R3-M3-report-1280.dc.html — Her report, a page of its own at /msf/my-reports/{id} (3.47) · 1280
Wombat
Acting as Trainee
MenuClose
[aria-label: Main]
Acting as Trainee
Home
Log an activity
My activities
MSF reports
My committee reviews
Export portfolio
My progress
My data rights
[title: Lerato Molefe] Lerato Molefe
Sign out
[aria-label: Breadcrumb]
Home
MSF reports
Default MSF, released 2026-10-14
## Default MSF, released 2026-10-14
Multi-source feedback · Released 2026-10-14 16:34 SAST
### Narrative
Consistently strong feedback from nursing and consultant colleagues; keep documenting escalation decisions
### What your colleagues said
Grouped by respondent role; no one is named. Ratings are averaged from 1, Well below expectations, to 5, Well above expectations. Comments are word for word.
#### Consultant · ‹n.nn› average rating
What should the trainee keep doing or improve?
Sound judgement in clinic; teaches the interns well
Ready to run a ward; should document escalation decisions more fully
‹the third consultant's comment›
#### Nurse · ‹n.nn› average rating
What should the trainee keep doing or improve?
Calm and kind with parents at night; hands over clearly
Leads neonatal resuscitations and explains her decisions to the nurses
‹the third nurse's comment›
### On your portfolio
Recorded as evidence on 2026-10-14 16:34 SAST:
PAED-010 — Leading and operating within a clinical team
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams

######## BOARD R3-M3-report-768.dc.html — Her report, a page of its own at /msf/my-reports/{id} (3.47) · 768
Wombat
Acting as Trainee
MenuClose
[aria-label: Main]
Acting as Trainee
Home
Log an activity
My activities
MSF reports
My committee reviews
Export portfolio
My progress
My data rights
[title: Lerato Molefe] Lerato Molefe
Sign out
[aria-label: Breadcrumb]
Home
MSF reports
Default MSF, released 2026-10-14
## Default MSF, released 2026-10-14
Multi-source feedback · Released 2026-10-14 16:34 SAST
### Narrative
Consistently strong feedback from nursing and consultant colleagues; keep documenting escalation decisions
### What your colleagues said
Grouped by respondent role; no one is named. Ratings are averaged from 1, Well below expectations, to 5, Well above expectations. Comments are word for word.
#### Consultant · ‹n.nn› average rating
What should the trainee keep doing or improve?
Sound judgement in clinic; teaches the interns well
Ready to run a ward; should document escalation decisions more fully
‹the third consultant's comment›
#### Nurse · ‹n.nn› average rating
What should the trainee keep doing or improve?
Calm and kind with parents at night; hands over clearly
Leads neonatal resuscitations and explains her decisions to the nurses
‹the third nurse's comment›
### On your portfolio
Recorded as evidence on 2026-10-14 16:34 SAST:
PAED-010 — Leading and operating within a clinical team
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams

######## BOARD R3-M3-report-390.dc.html — Her report, a page of its own at /msf/my-reports/{id} (3.47) · 390
Wombat
Acting as Trainee
MenuClose
[aria-label: Main]
Acting as Trainee
Home
Log an activity
My activities
MSF reports
My committee reviews
Export portfolio
My progress
My data rights
[title: Lerato Molefe] Lerato Molefe
Sign out
[aria-label: Breadcrumb]
Home
MSF reports
Default MSF, released 2026-10-14
## Default MSF, released 2026-10-14
Multi-source feedback · Released 2026-10-14 16:34 SAST
### Narrative
Consistently strong feedback from nursing and consultant colleagues; keep documenting escalation decisions
### What your colleagues said
Grouped by respondent role; no one is named. Ratings are averaged from 1, Well below expectations, to 5, Well above expectations. Comments are word for word.
#### Consultant · ‹n.nn› average rating
What should the trainee keep doing or improve?
Sound judgement in clinic; teaches the interns well
Ready to run a ward; should document escalation decisions more fully
‹the third consultant's comment›
#### Nurse · ‹n.nn› average rating
What should the trainee keep doing or improve?
Calm and kind with parents at night; hands over clearly
Leads neonatal resuscitations and explains her decisions to the nurses
‹the third nurse's comment›
### On your portfolio
Recorded as evidence on 2026-10-14 16:34 SAST:
PAED-010 — Leading and operating within a clinical team
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams

######## BOARD R3-M4-not-recorded-1280.dc.html — Her report · PAED-012 not recorded, left out (R8c, Illustration) · 1280
Wombat
Acting as Trainee
MenuClose
[aria-label: Main]
Acting as Trainee
Home
Log an activity
My activities
MSF reports
My committee reviews
Export portfolio
My progress
My data rights
[title: Lerato Molefe] Lerato Molefe
Sign out
Illustration PAED-012 left her curriculum before the release: it is left out. With none recorded or none named the section reads the NEW sentence.
[aria-label: Breadcrumb]
Home
MSF reports
Default MSF, released 2026-10-14
## Default MSF, released 2026-10-14
Multi-source feedback · Released 2026-10-14 16:34 SAST
### Narrative
Consistently strong feedback from nursing and consultant colleagues; keep documenting escalation decisions
### What your colleagues said
Grouped by respondent role; no one is named. Ratings are averaged from 1, Well below expectations, to 5, Well above expectations. Comments are word for word.
#### Consultant · ‹n.nn› average rating
What should the trainee keep doing or improve?
Sound judgement in clinic; teaches the interns well
Ready to run a ward; should document escalation decisions more fully
‹the third consultant's comment›
#### Nurse · ‹n.nn› average rating
What should the trainee keep doing or improve?
Calm and kind with parents at night; hands over clearly
Leads neonatal resuscitations and explains her decisions to the nurses
‹the third nurse's comment›
### On your portfolio
Recorded as evidence on 2026-10-14 16:34 SAST:
PAED-010 — Leading and operating within a clinical team

######## BOARD R3-M4-not-recorded-390.dc.html — Her report · PAED-012 not recorded, left out (R8c, Illustration) · 390
Wombat
Acting as Trainee
MenuClose
[aria-label: Main]
Acting as Trainee
Home
Log an activity
My activities
MSF reports
My committee reviews
Export portfolio
My progress
My data rights
[title: Lerato Molefe] Lerato Molefe
Sign out
Illustration PAED-012 left her curriculum before the release: it is left out. With none recorded or none named the section reads the NEW sentence.
[aria-label: Breadcrumb]
Home
MSF reports
Default MSF, released 2026-10-14
## Default MSF, released 2026-10-14
Multi-source feedback · Released 2026-10-14 16:34 SAST
### Narrative
Consistently strong feedback from nursing and consultant colleagues; keep documenting escalation decisions
### What your colleagues said
Grouped by respondent role; no one is named. Ratings are averaged from 1, Well below expectations, to 5, Well above expectations. Comments are word for word.
#### Consultant · ‹n.nn› average rating
What should the trainee keep doing or improve?
Sound judgement in clinic; teaches the interns well
Ready to run a ward; should document escalation decisions more fully
‹the third consultant's comment›
#### Nurse · ‹n.nn› average rating
What should the trainee keep doing or improve?
Calm and kind with parents at night; hands over clearly
Leads neonatal resuscitations and explains her decisions to the nurses
‹the third nurse's comment›
### On your portfolio
Recorded as evidence on 2026-10-14 16:34 SAST:
PAED-010 — Leading and operating within a clinical team

######## BOARD R3-M8-fifteen-390.dc.html — Her report · fifteen EPAs on her portfolio, PAED-015's title whole (S8, Illustration) · 390
Wombat
Acting as Trainee
MenuClose
[aria-label: Main]
Acting as Trainee
Home
Log an activity
My activities
MSF reports
My committee reviews
Export portfolio
My progress
My data rights
[title: Lerato Molefe] Lerato Molefe
Sign out
Illustration A report recorded as evidence for all fifteen EPAs.
[aria-label: Breadcrumb]
Home
MSF reports
Default MSF, released 2026-10-14
## Default MSF, released 2026-10-14
Multi-source feedback · Released 2026-10-14 16:34 SAST
### Narrative
Consistently strong feedback from nursing and consultant colleagues; keep documenting escalation decisions
### What your colleagues said
Grouped by respondent role; no one is named. Ratings are averaged from 1, Well below expectations, to 5, Well above expectations. Comments are word for word.
#### Consultant · ‹n.nn› average rating
What should the trainee keep doing or improve?
Sound judgement in clinic; teaches the interns well
Ready to run a ward; should document escalation decisions more fully
‹the third consultant's comment›
#### Nurse · ‹n.nn› average rating
What should the trainee keep doing or improve?
Calm and kind with parents at night; hands over clearly
Leads neonatal resuscitations and explains her decisions to the nurses
‹the third nurse's comment›
### On your portfolio
Recorded as evidence on 2026-10-14 16:34 SAST:
PAED-001 — Providing paediatric emergency care to children
PAED-002 — Managing common paediatric presentations
PAED-003 — Providing intensive care to children
PAED-004 — Managing common neonatal conditions
PAED-005 — Providing neonatal care in intensive and high-care settings
PAED-006 — Managing long-term health conditions (LTHCs)
PAED-007 — Maintaining and promoting the health and well-being of children
PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
PAED-009 — Providing care for an adolescent patient
PAED-010 — Leading and operating within a clinical team
PAED-011 — Managing population health challenges
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
PAED-013 — Managing child protection, legal and ethical aspects of children at risk
PAED-014 — Providing palliative and end-of-life care
PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research

######## BOARD R3-M5-not-hers-1280.dc.html — MSF reports · another trainee's id: Anele Dlamini on Lerato Molefe's (R8d) · 1280
Wombat
Acting as Trainee
MenuClose
[aria-label: Main]
Acting as Trainee
Home
Log an activity
My activities
MSF reports
My committee reviews
Export portfolio
My progress
My data rights
[title: Anele Dlamini] Anele Dlamini
Sign out
## MSF reports
Released aggregate feedback only. Individual responses are never shown.
That report could not be found. Your released reports are listed below.
No released reports
Released multi-source feedback will appear here.

######## BOARD R3-M5-not-hers-390.dc.html — MSF reports · another trainee's id: Anele Dlamini on Lerato Molefe's (R8d) · 390
Wombat
Acting as Trainee
MenuClose
[aria-label: Main]
Acting as Trainee
Home
Log an activity
My activities
MSF reports
My committee reviews
Export portfolio
My progress
My data rights
[title: Anele Dlamini] Anele Dlamini
Sign out
## MSF reports
Released aggregate feedback only. Individual responses are never shown.
That report could not be found. Your released reports are listed below.
No released reports
Released multi-source feedback will appear here.

######## BOARD R3-M6-loading-1280.dc.html — MSF reports · loading · 1280
Wombat
Acting as Trainee
MenuClose
[aria-label: Main]
Acting as Trainee
Home
Log an activity
My activities
MSF reports
My committee reviews
Export portfolio
My progress
My data rights
[title: Lerato Molefe] Lerato Molefe
Sign out
## MSF reports
Released aggregate feedback only. Individual responses are never shown.
### Your released reports
Loading your MSF reports.

######## BOARD R3-M6-loading-390.dc.html — MSF reports · loading · 390
Wombat
Acting as Trainee
MenuClose
[aria-label: Main]
Acting as Trainee
Home
Log an activity
My activities
MSF reports
My committee reviews
Export portfolio
My progress
My data rights
[title: Lerato Molefe] Lerato Molefe
Sign out
## MSF reports
Released aggregate feedback only. Individual responses are never shown.
### Your released reports
Loading your MSF reports.

######## BOARD R3-M7-error-1280.dc.html — MSF reports · load error · 1280
Wombat
Acting as Trainee
MenuClose
[aria-label: Main]
Acting as Trainee
Home
Log an activity
My activities
MSF reports
My committee reviews
Export portfolio
My progress
My data rights
[title: Lerato Molefe] Lerato Molefe
Sign out
## MSF reports
Released aggregate feedback only. Individual responses are never shown.
Could not load your MSF reports. Nothing has changed. Try again, or come back in a few minutes.Try again

######## BOARD R3-M7-error-390.dc.html — MSF reports · load error · 390
Wombat
Acting as Trainee
MenuClose
[aria-label: Main]
Acting as Trainee
Home
Log an activity
My activities
MSF reports
My committee reviews
Export portfolio
My progress
My data rights
[title: Lerato Molefe] Lerato Molefe
Sign out
## MSF reports
Released aggregate feedback only. Individual responses are never shown.
Could not load your MSF reports. Nothing has changed. Try again, or come back in a few minutes.Try again

######## BOARD R3-M9-report-loading-1280.dc.html — Her report · loading (S8) · 1280
Wombat
Acting as Trainee
MenuClose
[aria-label: Main]
Acting as Trainee
Home
Log an activity
My activities
MSF reports
My committee reviews
Export portfolio
My progress
My data rights
[title: Lerato Molefe] Lerato Molefe
Sign out
[aria-label: Breadcrumb]
Home
MSF reports
MSF report
## MSF report
Loading the report.

######## BOARD R3-M9-report-loading-390.dc.html — Her report · loading (S8) · 390
Wombat
Acting as Trainee
MenuClose
[aria-label: Main]
Acting as Trainee
Home
Log an activity
My activities
MSF reports
My committee reviews
Export portfolio
My progress
My data rights
[title: Lerato Molefe] Lerato Molefe
Sign out
[aria-label: Breadcrumb]
Home
MSF reports
MSF report
## MSF report
Loading the report.

######## BOARD R3-M10-report-error-1280.dc.html — Her report · load error, kept apart from the not-found alert (S8, S6) · 1280
Wombat
Acting as Trainee
MenuClose
[aria-label: Main]
Acting as Trainee
Home
Log an activity
My activities
MSF reports
My committee reviews
Export portfolio
My progress
My data rights
[title: Lerato Molefe] Lerato Molefe
Sign out
[aria-label: Breadcrumb]
Home
MSF reports
MSF report
## MSF report
Could not load this report. Nothing has changed. Try again, or come back in a few minutes.Try again

######## BOARD R3-M10-report-error-390.dc.html — Her report · load error, kept apart from the not-found alert (S8, S6) · 390
Wombat
Acting as Trainee
MenuClose
[aria-label: Main]
Acting as Trainee
Home
Log an activity
My activities
MSF reports
My committee reviews
Export portfolio
My progress
My data rights
[title: Lerato Molefe] Lerato Molefe
Sign out
[aria-label: Breadcrumb]
Home
MSF reports
MSF report
## MSF report
Could not load this report. Nothing has changed. Try again, or come back in a few minutes.Try again

######## BOARD R3-M11-report-unavailable-1280.dc.html — Her report · an id that is not one of her released reports: her list, one alert, focused (S8, S6) · 1280
Wombat
Acting as Trainee
MenuClose
[aria-label: Main]
Acting as Trainee
Home
Log an activity
My activities
MSF reports
My committee reviews
Export portfolio
My progress
My data rights
[title: Lerato Molefe] Lerato Molefe
Sign out
An id that is not hers, or not released, lands on her own list with one alert; a failed read is the load error (M10), never this alert.
## MSF reports
Released aggregate feedback only. Individual responses are never shown.
That report could not be found. Your released reports are listed below.
### Your released reports
Your released MSF reports, newest first
| Report
| Released
| Actions
| Default MSFMulti-source feedback
| 2026-10-14 16:34 SAST
[aria-label: View the Default MSF report, released 2026-10-14] View

######## BOARD R3-M11-report-unavailable-390.dc.html — Her report · an id that is not one of her released reports: her list, one alert, focused (S8, S6) · 390
Wombat
Acting as Trainee
MenuClose
[aria-label: Main]
Acting as Trainee
Home
Log an activity
My activities
MSF reports
My committee reviews
Export portfolio
My progress
My data rights
[title: Lerato Molefe] Lerato Molefe
Sign out
An id that is not hers, or not released, lands on her own list with one alert; a failed read is the load error (M10), never this alert.
## MSF reports
Released aggregate feedback only. Individual responses are never shown.
That report could not be found. Your released reports are listed below.
### Your released reports
Your released MSF reports, newest first
| Report
| Released
| Actions
| Default MSFMulti-source feedback
| 2026-10-14 16:34 SAST
[aria-label: View the Default MSF report, released 2026-10-14] View

######## BOARD R3-V1-loaded-1280.dc.html — MSF coverage · loaded (3.49) · 1280
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF coverage
## MSF coverage
How many of each programme's trainees a released multi-source feedback campaign covered, per EPA and semester. Coverage is not a target.
### Paediatric EPA Curriculum 11.1 at Kgosi Kgari Teaching Hospital
Semester 1, 2026: 5 trainees, whose programme had started by 30 June 2026
Semester 2, 2026: 5 trainees, whose programme had started by 31 December 2026
#### By EPA
Trainees covered by a released campaign that named the EPA, each semester. A withdrawn or unreleased campaign counts for nothing.
| EPA
| Semester 1, 2026
| Semester 2, 2026
| PAED-001 — Providing paediatric emergency care to children
| 0 of 5 trainees covered
| 0 of 5 trainees covered
| PAED-002 — Managing common paediatric presentations
| 0 of 5 trainees covered
| 0 of 5 trainees covered
| PAED-003 — Providing intensive care to children
| 0 of 5 trainees covered
| 0 of 5 trainees covered
| PAED-004 — Managing common neonatal conditions
| 0 of 5 trainees covered
| 0 of 5 trainees covered
| PAED-005 — Providing neonatal care in intensive and high-care settings
| 0 of 5 trainees covered
| 0 of 5 trainees covered
| PAED-006 — Managing long-term health conditions (LTHCs)
| 0 of 5 trainees covered
| 0 of 5 trainees covered
| PAED-007 — Maintaining and promoting the health and well-being of children
| 0 of 5 trainees covered
| 0 of 5 trainees covered
| PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
| 0 of 5 trainees covered
| 0 of 5 trainees covered
| PAED-009 — Providing care for an adolescent patient
| 0 of 5 trainees covered
| 0 of 5 trainees covered
| PAED-010 — Leading and operating within a clinical team
| 0 of 5 trainees covered
| 1 of 5 trainees covered
| PAED-011 — Managing population health challenges
| 0 of 5 trainees covered
| 0 of 5 trainees covered
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
| 0 of 5 trainees covered
| 1 of 5 trainees covered
| PAED-013 — Managing child protection, legal and ethical aspects of children at risk
| 0 of 5 trainees covered
| 0 of 5 trainees covered
| PAED-014 — Providing palliative and end-of-life care
| 0 of 5 trainees covered
| 0 of 5 trainees covered
| PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
| 0 of 5 trainees covered
| 0 of 5 trainees covered
#### By trainee
EPAs of the curriculum covered by a released campaign, each semester.
| Trainee
| Semester 1, 2026
| Semester 2, 2026
| Anele Dlamini
| 0 of 15 EPAs covered
| 0 of 15 EPAs covered
| Lerato Molefe
| 0 of 15 EPAs covered
| 2 of 15 EPAs covered
| Nomsa Mahlangu
| 0 of 15 EPAs covered
| 0 of 15 EPAs covered
| Pieter du Plessis
| 0 of 15 EPAs covered
| 0 of 15 EPAs covered
| Sipho Ndlovu
| 0 of 15 EPAs covered
| 0 of 15 EPAs covered

######## BOARD R3-V1-loaded-768.dc.html — MSF coverage · loaded (3.49) · 768
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF coverage
## MSF coverage
How many of each programme's trainees a released multi-source feedback campaign covered, per EPA and semester. Coverage is not a target.
### Paediatric EPA Curriculum 11.1 at Kgosi Kgari Teaching Hospital
Semester 1, 2026: 5 trainees, whose programme had started by 30 June 2026
Semester 2, 2026: 5 trainees, whose programme had started by 31 December 2026
#### By EPA
Trainees covered by a released campaign that named the EPA, each semester. A withdrawn or unreleased campaign counts for nothing.
| EPA
| Semester 1, 2026
| Semester 2, 2026
| PAED-001 — Providing paediatric emergency care to children
| 0 of 5 trainees covered
| 0 of 5 trainees covered
| PAED-002 — Managing common paediatric presentations
| 0 of 5 trainees covered
| 0 of 5 trainees covered
| PAED-003 — Providing intensive care to children
| 0 of 5 trainees covered
| 0 of 5 trainees covered
| PAED-004 — Managing common neonatal conditions
| 0 of 5 trainees covered
| 0 of 5 trainees covered
| PAED-005 — Providing neonatal care in intensive and high-care settings
| 0 of 5 trainees covered
| 0 of 5 trainees covered
| PAED-006 — Managing long-term health conditions (LTHCs)
| 0 of 5 trainees covered
| 0 of 5 trainees covered
| PAED-007 — Maintaining and promoting the health and well-being of children
| 0 of 5 trainees covered
| 0 of 5 trainees covered
| PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
| 0 of 5 trainees covered
| 0 of 5 trainees covered
| PAED-009 — Providing care for an adolescent patient
| 0 of 5 trainees covered
| 0 of 5 trainees covered
| PAED-010 — Leading and operating within a clinical team
| 0 of 5 trainees covered
| 1 of 5 trainees covered
| PAED-011 — Managing population health challenges
| 0 of 5 trainees covered
| 0 of 5 trainees covered
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
| 0 of 5 trainees covered
| 1 of 5 trainees covered
| PAED-013 — Managing child protection, legal and ethical aspects of children at risk
| 0 of 5 trainees covered
| 0 of 5 trainees covered
| PAED-014 — Providing palliative and end-of-life care
| 0 of 5 trainees covered
| 0 of 5 trainees covered
| PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
| 0 of 5 trainees covered
| 0 of 5 trainees covered
#### By trainee
EPAs of the curriculum covered by a released campaign, each semester.
| Trainee
| Semester 1, 2026
| Semester 2, 2026
| Anele Dlamini
| 0 of 15 EPAs covered
| 0 of 15 EPAs covered
| Lerato Molefe
| 0 of 15 EPAs covered
| 2 of 15 EPAs covered
| Nomsa Mahlangu
| 0 of 15 EPAs covered
| 0 of 15 EPAs covered
| Pieter du Plessis
| 0 of 15 EPAs covered
| 0 of 15 EPAs covered
| Sipho Ndlovu
| 0 of 15 EPAs covered
| 0 of 15 EPAs covered

######## BOARD R3-V1-loaded-390.dc.html — MSF coverage · loaded (3.49) · 390
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF coverage
## MSF coverage
How many of each programme's trainees a released multi-source feedback campaign covered, per EPA and semester. Coverage is not a target.
### Paediatric EPA Curriculum 11.1 at Kgosi Kgari Teaching Hospital
Semester 1, 2026: 5 trainees, whose programme had started by 30 June 2026
Semester 2, 2026: 5 trainees, whose programme had started by 31 December 2026
#### By EPA
Trainees covered by a released campaign that named the EPA, each semester. A withdrawn or unreleased campaign counts for nothing.
| EPA
| Semester 1, 2026
| Semester 2, 2026
| PAED-001 — Providing paediatric emergency care to children
| 0 of 5 trainees covered
| 0 of 5 trainees covered
| PAED-002 — Managing common paediatric presentations
| 0 of 5 trainees covered
| 0 of 5 trainees covered
| PAED-003 — Providing intensive care to children
| 0 of 5 trainees covered
| 0 of 5 trainees covered
| PAED-004 — Managing common neonatal conditions
| 0 of 5 trainees covered
| 0 of 5 trainees covered
| PAED-005 — Providing neonatal care in intensive and high-care settings
| 0 of 5 trainees covered
| 0 of 5 trainees covered
| PAED-006 — Managing long-term health conditions (LTHCs)
| 0 of 5 trainees covered
| 0 of 5 trainees covered
| PAED-007 — Maintaining and promoting the health and well-being of children
| 0 of 5 trainees covered
| 0 of 5 trainees covered
| PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
| 0 of 5 trainees covered
| 0 of 5 trainees covered
| PAED-009 — Providing care for an adolescent patient
| 0 of 5 trainees covered
| 0 of 5 trainees covered
| PAED-010 — Leading and operating within a clinical team
| 0 of 5 trainees covered
| 1 of 5 trainees covered
| PAED-011 — Managing population health challenges
| 0 of 5 trainees covered
| 0 of 5 trainees covered
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
| 0 of 5 trainees covered
| 1 of 5 trainees covered
| PAED-013 — Managing child protection, legal and ethical aspects of children at risk
| 0 of 5 trainees covered
| 0 of 5 trainees covered
| PAED-014 — Providing palliative and end-of-life care
| 0 of 5 trainees covered
| 0 of 5 trainees covered
| PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
| 0 of 5 trainees covered
| 0 of 5 trainees covered
#### By trainee
EPAs of the curriculum covered by a released campaign, each semester.
| Trainee
| Semester 1, 2026
| Semester 2, 2026
| Anele Dlamini
| 0 of 15 EPAs covered
| 0 of 15 EPAs covered
| Lerato Molefe
| 0 of 15 EPAs covered
| 2 of 15 EPAs covered
| Nomsa Mahlangu
| 0 of 15 EPAs covered
| 0 of 15 EPAs covered
| Pieter du Plessis
| 0 of 15 EPAs covered
| 0 of 15 EPAs covered
| Sipho Ndlovu
| 0 of 15 EPAs covered
| 0 of 15 EPAs covered

######## BOARD R3-V2-nobody-1280.dc.html — MSF coverage · nobody to show · 1280
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF coverage
## MSF coverage
How many of each programme's trainees a released multi-source feedback campaign covered, per EPA and semester. Coverage is not a target.
No trainees to show

######## BOARD R3-V2-nobody-390.dc.html — MSF coverage · nobody to show · 390
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF coverage
## MSF coverage
How many of each programme's trainees a released multi-source feedback campaign covered, per EPA and semester. Coverage is not a target.
No trainees to show

######## BOARD R3-V3-two-programmes-1280.dc.html — MSF coverage · an Administrator's view, two programmes (R9, Illustration) · 1280
Wombat
Acting as Administrator
MenuClose
[aria-label: Main]
Acting as Administrator
Home
Platform
Scheduled jobs
Audit log
SSO mappings
Data rights requests
Organisations
Institutions
Colleges
People
Users
Invitations
Catalogue
EPAs
Curricula
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
My data rights
[title: ‹Administrator›] ‹Administrator›
Sign out
Illustration The Administrator has no MSF item (Q7): nothing is lit, and the trail is Home › MSF coverage.
[aria-label: Breadcrumb]
Home
MSF coverage
## MSF coverage
How many of each programme's trainees a released multi-source feedback campaign covered, per EPA and semester. Coverage is not a target.
### Paediatric EPA Curriculum 11.1 at Kgosi Kgari Teaching Hospital
Semester 1, 2026: 5 trainees, whose programme had started by 30 June 2026
Semester 2, 2026: 5 trainees, whose programme had started by 31 December 2026
#### By EPA
Trainees covered by a released campaign that named the EPA, each semester. A withdrawn or unreleased campaign counts for nothing.
| EPA
| Semester 1, 2026
| Semester 2, 2026
| PAED-001 — Providing paediatric emergency care to children
| 0 of 5 trainees covered
| 0 of 5 trainees covered
| PAED-002 — Managing common paediatric presentations
| 0 of 5 trainees covered
| 0 of 5 trainees covered
| PAED-003 — Providing intensive care to children
| 0 of 5 trainees covered
| 0 of 5 trainees covered
| PAED-004 — Managing common neonatal conditions
| 0 of 5 trainees covered
| 0 of 5 trainees covered
| PAED-005 — Providing neonatal care in intensive and high-care settings
| 0 of 5 trainees covered
| 0 of 5 trainees covered
| PAED-006 — Managing long-term health conditions (LTHCs)
| 0 of 5 trainees covered
| 0 of 5 trainees covered
| PAED-007 — Maintaining and promoting the health and well-being of children
| 0 of 5 trainees covered
| 0 of 5 trainees covered
| PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
| 0 of 5 trainees covered
| 0 of 5 trainees covered
| PAED-009 — Providing care for an adolescent patient
| 0 of 5 trainees covered
| 0 of 5 trainees covered
| PAED-010 — Leading and operating within a clinical team
| 0 of 5 trainees covered
| 1 of 5 trainees covered
| PAED-011 — Managing population health challenges
| 0 of 5 trainees covered
| 0 of 5 trainees covered
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
| 0 of 5 trainees covered
| 1 of 5 trainees covered
| PAED-013 — Managing child protection, legal and ethical aspects of children at risk
| 0 of 5 trainees covered
| 0 of 5 trainees covered
| PAED-014 — Providing palliative and end-of-life care
| 0 of 5 trainees covered
| 0 of 5 trainees covered
| PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
| 0 of 5 trainees covered
| 0 of 5 trainees covered
#### By trainee
EPAs of the curriculum covered by a released campaign, each semester.
| Trainee
| Semester 1, 2026
| Semester 2, 2026
| Anele Dlamini
| 0 of 15 EPAs covered
| 0 of 15 EPAs covered
| Lerato Molefe
| 0 of 15 EPAs covered
| 2 of 15 EPAs covered
| Nomsa Mahlangu
| 0 of 15 EPAs covered
| 0 of 15 EPAs covered
| Pieter du Plessis
| 0 of 15 EPAs covered
| 0 of 15 EPAs covered
| Sipho Ndlovu
| 0 of 15 EPAs covered
| 0 of 15 EPAs covered
### ‹programme› at ‹institution›
Semester 1, 2026: ‹n› trainees, whose programme had started by 30 June 2026
Semester 2, 2026: ‹n› trainees, whose programme had started by 31 December 2026
#### By EPA
Trainees covered by a released campaign that named the EPA, each semester. A withdrawn or unreleased campaign counts for nothing.
| EPA
| Semester 1, 2026
| Semester 2, 2026
| PAED-001 — Providing paediatric emergency care to children
| ‹n› of ‹n› trainees covered
| ‹n› of ‹n› trainees covered
| PAED-002 — Managing common paediatric presentations
| ‹n› of ‹n› trainees covered
| ‹n› of ‹n› trainees covered
| PAED-003 — Providing intensive care to children
| ‹n› of ‹n› trainees covered
| ‹n› of ‹n› trainees covered
| PAED-004 — Managing common neonatal conditions
| ‹n› of ‹n› trainees covered
| ‹n› of ‹n› trainees covered
| PAED-005 — Providing neonatal care in intensive and high-care settings
| ‹n› of ‹n› trainees covered
| ‹n› of ‹n› trainees covered
| PAED-006 — Managing long-term health conditions (LTHCs)
| ‹n› of ‹n› trainees covered
| ‹n› of ‹n› trainees covered
| PAED-007 — Maintaining and promoting the health and well-being of children
| ‹n› of ‹n› trainees covered
| ‹n› of ‹n› trainees covered
| PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
| ‹n› of ‹n› trainees covered
| ‹n› of ‹n› trainees covered
| PAED-009 — Providing care for an adolescent patient
| ‹n› of ‹n› trainees covered
| ‹n› of ‹n› trainees covered
| PAED-010 — Leading and operating within a clinical team
| ‹n› of ‹n› trainees covered
| ‹n› of ‹n› trainees covered
| PAED-011 — Managing population health challenges
| ‹n› of ‹n› trainees covered
| ‹n› of ‹n› trainees covered
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
| ‹n› of ‹n› trainees covered
| ‹n› of ‹n› trainees covered
| PAED-013 — Managing child protection, legal and ethical aspects of children at risk
| ‹n› of ‹n› trainees covered
| ‹n› of ‹n› trainees covered
| PAED-014 — Providing palliative and end-of-life care
| ‹n› of ‹n› trainees covered
| ‹n› of ‹n› trainees covered
| PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
| ‹n› of ‹n› trainees covered
| ‹n› of ‹n› trainees covered
#### By trainee
EPAs of the curriculum covered by a released campaign, each semester.
| Trainee
| Semester 1, 2026
| Semester 2, 2026
| ‹trainee›
| ‹n› of 15 EPAs covered
| ‹n› of 15 EPAs covered
| ‹another trainee›
| ‹n› of 15 EPAs covered
| ‹n› of 15 EPAs covered

######## BOARD R3-V3-two-programmes-390.dc.html — MSF coverage · an Administrator's view, two programmes (R9, Illustration) · 390
Wombat
Acting as Administrator
MenuClose
[aria-label: Main]
Acting as Administrator
Home
Platform
Scheduled jobs
Audit log
SSO mappings
Data rights requests
Organisations
Institutions
Colleges
People
Users
Invitations
Catalogue
EPAs
Curricula
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
My data rights
[title: ‹Administrator›] ‹Administrator›
Sign out
Illustration The Administrator has no MSF item (Q7): nothing is lit, and the trail is Home › MSF coverage.
[aria-label: Breadcrumb]
Home
MSF coverage
## MSF coverage
How many of each programme's trainees a released multi-source feedback campaign covered, per EPA and semester. Coverage is not a target.
### Paediatric EPA Curriculum 11.1 at Kgosi Kgari Teaching Hospital
Semester 1, 2026: 5 trainees, whose programme had started by 30 June 2026
Semester 2, 2026: 5 trainees, whose programme had started by 31 December 2026
#### By EPA
Trainees covered by a released campaign that named the EPA, each semester. A withdrawn or unreleased campaign counts for nothing.
| EPA
| Semester 1, 2026
| Semester 2, 2026
| PAED-001 — Providing paediatric emergency care to children
| 0 of 5 trainees covered
| 0 of 5 trainees covered
| PAED-002 — Managing common paediatric presentations
| 0 of 5 trainees covered
| 0 of 5 trainees covered
| PAED-003 — Providing intensive care to children
| 0 of 5 trainees covered
| 0 of 5 trainees covered
| PAED-004 — Managing common neonatal conditions
| 0 of 5 trainees covered
| 0 of 5 trainees covered
| PAED-005 — Providing neonatal care in intensive and high-care settings
| 0 of 5 trainees covered
| 0 of 5 trainees covered
| PAED-006 — Managing long-term health conditions (LTHCs)
| 0 of 5 trainees covered
| 0 of 5 trainees covered
| PAED-007 — Maintaining and promoting the health and well-being of children
| 0 of 5 trainees covered
| 0 of 5 trainees covered
| PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
| 0 of 5 trainees covered
| 0 of 5 trainees covered
| PAED-009 — Providing care for an adolescent patient
| 0 of 5 trainees covered
| 0 of 5 trainees covered
| PAED-010 — Leading and operating within a clinical team
| 0 of 5 trainees covered
| 1 of 5 trainees covered
| PAED-011 — Managing population health challenges
| 0 of 5 trainees covered
| 0 of 5 trainees covered
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
| 0 of 5 trainees covered
| 1 of 5 trainees covered
| PAED-013 — Managing child protection, legal and ethical aspects of children at risk
| 0 of 5 trainees covered
| 0 of 5 trainees covered
| PAED-014 — Providing palliative and end-of-life care
| 0 of 5 trainees covered
| 0 of 5 trainees covered
| PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
| 0 of 5 trainees covered
| 0 of 5 trainees covered
#### By trainee
EPAs of the curriculum covered by a released campaign, each semester.
| Trainee
| Semester 1, 2026
| Semester 2, 2026
| Anele Dlamini
| 0 of 15 EPAs covered
| 0 of 15 EPAs covered
| Lerato Molefe
| 0 of 15 EPAs covered
| 2 of 15 EPAs covered
| Nomsa Mahlangu
| 0 of 15 EPAs covered
| 0 of 15 EPAs covered
| Pieter du Plessis
| 0 of 15 EPAs covered
| 0 of 15 EPAs covered
| Sipho Ndlovu
| 0 of 15 EPAs covered
| 0 of 15 EPAs covered
### ‹programme› at ‹institution›
Semester 1, 2026: ‹n› trainees, whose programme had started by 30 June 2026
Semester 2, 2026: ‹n› trainees, whose programme had started by 31 December 2026
#### By EPA
Trainees covered by a released campaign that named the EPA, each semester. A withdrawn or unreleased campaign counts for nothing.
| EPA
| Semester 1, 2026
| Semester 2, 2026
| PAED-001 — Providing paediatric emergency care to children
| ‹n› of ‹n› trainees covered
| ‹n› of ‹n› trainees covered
| PAED-002 — Managing common paediatric presentations
| ‹n› of ‹n› trainees covered
| ‹n› of ‹n› trainees covered
| PAED-003 — Providing intensive care to children
| ‹n› of ‹n› trainees covered
| ‹n› of ‹n› trainees covered
| PAED-004 — Managing common neonatal conditions
| ‹n› of ‹n› trainees covered
| ‹n› of ‹n› trainees covered
| PAED-005 — Providing neonatal care in intensive and high-care settings
| ‹n› of ‹n› trainees covered
| ‹n› of ‹n› trainees covered
| PAED-006 — Managing long-term health conditions (LTHCs)
| ‹n› of ‹n› trainees covered
| ‹n› of ‹n› trainees covered
| PAED-007 — Maintaining and promoting the health and well-being of children
| ‹n› of ‹n› trainees covered
| ‹n› of ‹n› trainees covered
| PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
| ‹n› of ‹n› trainees covered
| ‹n› of ‹n› trainees covered
| PAED-009 — Providing care for an adolescent patient
| ‹n› of ‹n› trainees covered
| ‹n› of ‹n› trainees covered
| PAED-010 — Leading and operating within a clinical team
| ‹n› of ‹n› trainees covered
| ‹n› of ‹n› trainees covered
| PAED-011 — Managing population health challenges
| ‹n› of ‹n› trainees covered
| ‹n› of ‹n› trainees covered
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
| ‹n› of ‹n› trainees covered
| ‹n› of ‹n› trainees covered
| PAED-013 — Managing child protection, legal and ethical aspects of children at risk
| ‹n› of ‹n› trainees covered
| ‹n› of ‹n› trainees covered
| PAED-014 — Providing palliative and end-of-life care
| ‹n› of ‹n› trainees covered
| ‹n› of ‹n› trainees covered
| PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
| ‹n› of ‹n› trainees covered
| ‹n› of ‹n› trainees covered
#### By trainee
EPAs of the curriculum covered by a released campaign, each semester.
| Trainee
| Semester 1, 2026
| Semester 2, 2026
| ‹trainee›
| ‹n› of 15 EPAs covered
| ‹n› of 15 EPAs covered
| ‹another trainee›
| ‹n› of 15 EPAs covered
| ‹n› of 15 EPAs covered

######## BOARD R3-V4-loading-1280.dc.html — MSF coverage · loading · 1280
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF coverage
## MSF coverage
How many of each programme's trainees a released multi-source feedback campaign covered, per EPA and semester. Coverage is not a target.
Loading MSF coverage.

######## BOARD R3-V4-loading-390.dc.html — MSF coverage · loading · 390
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF coverage
## MSF coverage
How many of each programme's trainees a released multi-source feedback campaign covered, per EPA and semester. Coverage is not a target.
Loading MSF coverage.

######## BOARD R3-V5-error-1280.dc.html — MSF coverage · load error · 1280
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF coverage
## MSF coverage
How many of each programme's trainees a released multi-source feedback campaign covered, per EPA and semester. Coverage is not a target.
Could not load MSF coverage. Nothing has changed. Try again, or come back in a few minutes.Try again

######## BOARD R3-V5-error-390.dc.html — MSF coverage · load error · 390
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: Pieter Smit] Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
MSF campaigns
MSF coverage
## MSF coverage
How many of each programme's trainees a released multi-source feedback campaign covered, per EPA and semester. Coverage is not a target.
Could not load MSF coverage. Nothing has changed. Try again, or come back in a few minutes.Try again
