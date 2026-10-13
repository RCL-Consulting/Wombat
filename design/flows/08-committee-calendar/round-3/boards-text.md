

######## BOARD R2-0-spec.dc.html — Spec · flow 08 round 3
Spec · flow 08 round 3 
## Flow 08, round 3: A and the panels, corrected
 Round 2's boards, kept with every state and corrected in place on this page (named “Round 3”); round 1's stay on their page. Boards 1 to 5 hold the pages at 1280 and 390, with 768 for the three stack-wide lists and the schedule page, and 1024 for the schedule page's stacked grid. Each frame's dark caption names the state, the step, the role, the round-3 item it answers and where the focus lands. Every colour is a token; no NEW token.
 The cadence drawn (the College's v11.1 seed): semester EPAs PAED-001, 002, 004, 005, 010 and 012 (004 and 005 the neonatal committee's); annual PAED-003, 006, 007, 011, 014 and 015; optional, “As opportunity allows”, PAED-008, 009 and 013. Titles: PAED-001 the seed's; 003, 008 and 012 as the design system's previews (taken from the seed); 015 the brief's; the others are marked “[PAED-0nn title, as seeded]”, never invented.
## Round 3, item by item
 Item | What changed | Where | 
 F1 | A Missed row's Schedule opens the form on the row's own ended semester; under Period, “2026 S1 has ended. A review can still sit for it: sittings are often held after the period ends.”; the preview for 2026 S1: the six semester EPAs due, the annual six “Due by year end”, the optional three. Q7's sentence and its “current semester” name are gone. | 4 Schedule a review: From a Missed row of 2026 S1 (also at 1024); 3 Decisions due: A past semester (each row's Schedule named “… for 2026 S1”) | 
 F2 | Show on every filter bar; “Apply” is gone from the Spec. | 3 Decisions due, 1 Decision panels (the Administrator's) | 
 F3 | Dates typed in the browser's date input; every date shown reads ISO. The ISO text field, its YYYY-MM-DD hint and its parse refusal are gone. | 4 Schedule a review: every form frame | 
 F4 | Review type preselected: Annual progression in S2, Entrustment-only in S1 (my reading of “the default”: confirm), Entrustment-only as the one binding type before a panel sitting as the neonatal committee. No “Choose a review type.” refusal. | 4 Schedule a review: every form frame; Heavy: the neonatal panel's one type; A refusal on Create: missing fields | 
 F5 | 4.8 and 4.13 check the warnings shown first. The race refusals stay as their own states, captioned “Scratch state: a second tab”, covered by handler and bUnit tests; no step plays a race. | 4 Schedule a review: The seat already held; Scratch state (C7). 2 A panel: Decides for, locked; Scratch state (C13). Steps board 4.8, 4.13 | 
 F6 | “The College's semester 2 ends in November; its window runs to 31 December, so December's work counts.” replaces the old line everywhere. | 3 Decisions due: Period's help; 4 Schedule a review: Period's help in S2; 5 Committee reviews: the rule line | 
 M1 | Every row from the cadence: windows, statuses and sentences. In 2026 S1 the six semester EPAs read Missed for every registrar, the annual six “Due by year end” (PAED-003 and 006 never Missed), the optional three “As opportunity allows”. | 3 Decisions due: every list frame | 
 M2 | An annual EPA's Not scheduled row reads “No open review holds it, and 2026 has not ended.” | 3 Decisions due: Outstanding; Filtered to one EPA; the heavy one-trainee frame | 
 M3 | By EPA's counts computed from each board's own rows. The fold drawn open at 4.10 (Scheduled 5, To schedule 0), 4.34 (PAED-001 Due 5, Decided 2, Not decided 3), 4.37 (PAED-002 Decided 0, Not decided 4, To schedule 1) and 4.2 under an S1 caption (Missed 5 on the six semester EPAs, To schedule 5 on the annual six, Optional 5 on the three). | 3 Decisions due: All scheduled; After the sitting; Revoked; A past semester; By EPA shown | 
 M4 | Q7's sentence is gone everywhere (with F1). | 4 Schedule a review, 3 Decisions due, Spec, Steps | 
 M5 | The schedule page's Period offers the six Decisions due offers: 2025 S1 to 2027 S2. | 4 Schedule a review: every form frame | 
 M6 | Sarah Botha is in no control on the can-no-longer-sit board: not in Chair's options, not a seat; the warning names her. | 2 A panel: A member who can no longer sit; After Save panel | 
 M7 | A panel not found is flow 01's Not found page, word for word (“There is no page at this address.” / “Check the address, or start again from Home.”). | 2 A panel: Not found | 
 M8 | Committee reviews, Decision panels (the list and Who decides each EPA) and By EPA are .clinic-table--stack-wide, stacked at 900px and below; each drawn at 768. | 5 Committee reviews 768; 1 Decision panels 768; 3 Decisions due 768 (By EPA open in “After the sitting”) | 
 M9 | The schedule grid stacks below 1100px: the Agenda under the fields, before the summary and the actions. | 4 Schedule a review 1024 and 768 | 
 M10 | Steps 4.1 and 4.2 restored with their checks; 4.2's Do presses one Missed row's Schedule and reads F1's form without creating. | Steps board | 
 S1 | The heavy case: one trainee line a panel; the two-option Panel select; the neonatal rows named for their panel; S1's four types with their help, and the neonatal panel's one binding type; the trainee picker (current registrars, training year, 5.29); the new-panel note for someone who also holds Trainee; NothingToDecide (the preview says so first; Create refused in the handler's own sentence, left as a marked placeholder). | 3 Decisions due: Heavy: two panels; 4 Schedule a review: Heavy ×2, Current registrars only, From a Missed row (S1 types), NothingToDecide; 2 A panel: Someone who also holds Trainee | 
 S2 | Committee reviews' heavy rows after A.1.14: Sipho Ndlovu's three reviews show the pseudonym as stored; no Dlamini check-in; Entrustment only only in 2026 S1. | 5 Committee reviews: Heavy | 
 S3 | Addresses whole, on .local domains; John van Rensburg on sun.wombat.local. | 2 A panel: every seat picker and Chair | 
 S4 | Refusals in Wombat's words: the evidence window (“The window ends before it starts. Choose a day on or after 2026-01-01.”; a missing From “Choose the day the window starts.”), a missing name (“Give the panel a name.”), speciality (“Choose a speciality.”) and institution (“Choose an institution.”). “Never the exception's text” is for unexpected failures only; a handler's own sentence still shows. The Decisions due subtitle drops “each semester”. “Read again when it opens” is gone: the agenda is planned at Create. The race summaries link a short clause. The Status filter keeps today's order (Outstanding, Every status, then the statuses as the brief lists them). Rows sort by surname. | 4 Schedule a review: the evidence window refusal, the preview failed, Scratch state; 2 A panel: An Administrator, the institution missing; 3 Decisions due: every frame | 
 S5 | Steps 5.1, 5.2, 4.6 and 4.9 keep their quoted checks; the trainee line's three states are played in 4.9 and 4.12 (held, none) and in 4.49 and A.1.10 (the check-in). | Steps board | 
 S6 | Reuse: By EPA's fold is .rung-legend's, with the global .fold-show / .fold-hide, its summary 44px at every width; the types are .scale-choices .form-check; Scope and the seats are .check-grid .form-check (no class says “seat”); busy is .is-running; the opening line .muted. Every new phone rule is scoped to .committee-calendar. One narrow live region in the Agenda (.agenda-live), with no role="alert" inside it. A refused group is named by its message (aria-describedby), its summary link focuses the first radio, it has a forced-colours stripe and no padding shift. No summary line points at a disabled control. No static inline style in the product markup (a skeleton's size stays StatePanel's parameter). Cancel while busy is a disabled button, not a link. | every board; 2 A panel: A panel of one; 4 Schedule a review: Busy, A refusal on Create | 
 S7 | “Open check-in #6” keeps its visible words in its name (“Open check-in #6 of Sipho Ndlovu”). Scheduled is grey on both lists (StateBadge). “1 seat”. The header button reads “Schedule a review”. “—” reads “None” to a screen reader. | 3 Decisions due: One trainee: a check-in; 5 Committee reviews: every list; 2 A panel: the seat count | 
 S8 | The Today column recounted below. | Spec: Step counts | 
## Components from the design system
 Component | Used on | How | 
 MainLayout, NavMenu | every page | each role's own menu; nothing lit on Access denied and Not found | 
 PageHeader, Breadcrumbs | every page | one h1; a panel's page and a review titled by their record | 
 FilterBar | Decisions due; the Administrator's Decision panels | GET form, Show, Clear filters once a filter is set | 
 DataTable (Stack, stack-wide) | every list, By EPA, Who decides each EPA, the seats | a caption on every table; row actions named for their row | 
 PagerControls | Decisions due | “Decisions, pages”; “Decisions per page” labelled (T280) | 
 The .rung-legend fold | By EPA | flow 04's fold, its summary 44px, Show / Hide | 
 StatePanel, Skeleton, Alert, ActionResult | every page | header first, then skeletons; fixed words with Try again for an unexpected failure | 
 RefusalSummary, FormField, FormActions | New panel, Decides for, Schedule a review | the summary takes the focus; each line moves it to its field | 
 .scale-choices / .check-grid with .form-check | Review type; Scope and the seats | native radios in a legend-named fieldset | 
 DetailCard, details-list | About this panel | | 
 Badge (BadgeFor, StateBadge) | Decisions due, Committee reviews | Scheduled grey on both | 
 SittingBar | the review page on arrival | flow 07's | 
 AccessDenied, NotFound | system pages | flow 01's words | 
## NEW
 NEW | Class(es) | What | Tokens | 
 Page scope | .committee-calendar | wraps these five pages; every new phone rule is scoped to it | — | 
 Trainee filter | (FilterBar field) | “Every trainee”, then the current registrars by surname | — | 
 TraineeLine (C2) | .trainee-line | what holds the period, one line a panel that can hold it | surface-color, border-color, radius-lg, shadow-raised | 
 Status cell | .status-cell | the badge over its sentence | space-xs | 
 List heading's page and count | .list-section-page, .list-section-count | “Decisions, page 2 of 4”; “75 of 75 …” | muted-text | 
 By EPA in the fold | .rung-legend > .table-container | the table's margin inside the fold | — | 
 Schedule grid | .schedule-grid, .schedule-fields, .field-pair | the Agenda beside from 1100px, under the fields below | space-lg, space-xl | 
 AgendaPreview | .agenda-preview, .agenda-live, .agenda-count | advice, never a gate; one live region | info-bg, border-color, radius-lg | 
 Choice group | .choice-group, .choice-group.is-invalid, .choice-row | a bare fieldset; refused, the 4px stripe (a border in forced colours), no shift | danger-color | 
 Person cell | .person-cell | the name over the whole address | muted-text | 
 Read-only value; sections | .readonly-value, .panel-section, .form-section-title | | space-lg, space-xl | 
 Period cell | .period-window | the period over its window, each date unbroken | muted-text | 
## DESIGN.md changes
 Section | Change | 
 § Page-level patterns: Form page | Schedule a review at /committee/reviews/new, with an advisory aside that stacks below 1100px; Cancel returns where it came from | 
 § Page-level patterns: Detail page, the title rule | a panel's page titled by the panel's name; no “Back to panels” | 
 Owner table | NEW row: Schedule a review → Committee reviews (the five schedulers) | 
 § Table system | one action per row; Committee reviews, Decision panels and By EPA stack-wide (900px); no Mode column | 
 § Form system | the seat picker; Formative only folds into Review type; the type preselected; dates in the browser's date input, shown ISO | 
 § Pager | the page-size select labelled; the focus on the list's heading, which says the page | 
 Decisions due's section | the Trainee filter and line; By EPA in the fold; a row's Schedule opens the form on the row's own period (F1) | 
## Contrast (WCAG 2.1, computed from the tokens)
 Foreground | Ground | Ratio | Needs | | Where | 
 text-color | surface-color | 12.63:1 | 4.5:1 | passes | body text | 
 text-color | info-bg | 11.84:1 | 4.5:1 | passes | the Agenda | 
 text-color | warning-bg | 11.89:1 | 4.5:1 | passes | Already held, Not decided | 
 text-color | danger-bg | 11.81:1 | 4.5:1 | passes | refusals, Missed and Revoked | 
 text-color | success-bg | 11.23:1 | 4.5:1 | passes | results, Decided | 
 text-color | header-bg | 11.36:1 | 4.5:1 | passes | Scheduled and other grey badges, table heads | 
 muted-text | surface-color | 5.09:1 | 4.5:1 | passes | help, counts | 
 muted-text | info-bg | 4.77:1 | 4.5:1 | passes | the Agenda's note | 
 link-color | surface-color | 6.70:1 | 4.5:1 | passes | links | 
 link-color | info-bg | 6.28:1 | 4.5:1 | passes | links in the Agenda | 
 link-color | danger-bg | 6.27:1 | 4.5:1 | passes | a summary's line | 
 on-fill | secondary-color | 4.86:1 | 4.5:1 | passes | Show, Create review | 
 secondary-color | surface-color | 4.86:1 | 3.0:1 | passes | outline buttons; the focus ring | 
 secondary-color | info-bg | 4.55:1 | 3.0:1 | passes | the ring in the Agenda | 
 input-border | surface-color | 3.80:1 | 3.0:1 | passes | a control's edge | 
 danger-color | surface-color | 5.95:1 | 4.5:1 | passes | a field's message; the stripe | 
## Step counts (C14), recounted (S8)
 A press is a click, a tap, or a choice in a select or radio group; a typed field is one field. At 390 add one press (Menu) for each move through the menu. Scheduled on starts on today; today's Review type select starts on its first option, and Formative only is a tick.
 Journey | A, 1280 | A, 390 | Today, 1280 | 
 4.1 read, page through, filter to PAED-004 | 7 + 0 (Decisions due, Next ×3, Status, EPA, Show) | 8 + 0 | 6 + 0 (no Show: each filter is a round trip) | 
 4.6 schedule Dr Molefe from an empty schedule | 6 + 0 (Committee reviews, Schedule a review, Panel, Trainee, Period, Create review) | 7 + 0 | 6 + 0 | 
 4.7 two more | 12 + 0 | 14 + 0 | 12 + 0 | 
 4.8 stopped from a second review | 5 + 0, before Create | 6 + 0 | 6 + 0, refused after Create | 
 4.9 Dr Mahlangu from Decisions due | 4 + 0 by Next (Decisions due, Next, Schedule, Create review); 5 + 0 with the Trainee filter | 5 + 0 / 6 + 0 | 4 + 0 (Decisions due, Next, Schedule, Create) | 
 5.1, 5.2 Dr Molefe's rows, then her pre-graduation review | 7 + 1 (Decisions due, Trainee, Status, Show, Schedule a review, Pre-graduation, From typed, Create review) | 8 + 1 | 6 + 1 (Decisions due, Status, page 3, Schedule, type, From typed, Create), after reading past 45 rows | 
 4.49 a formative check-in | 7 + 0 | 8 + 0 | 7 + 0 | 
 2.21–2.22 the panel, the refusal included | 10 + 1 | 11 + 1 | 10 + 1 (Ctrl-clicks in the multi-selects) | 
 … without the refusal | 6 + 1 | 7 + 1 | 6 + 1 | 
## Accessibility review, WCAG 2.1 AA
 One h1 a page; h2 for each section (By EPA, Decisions, Agenda, Who decides each EPA, About this panel, Who sits on it, Decides for); h3 below. Every table captioned.
 Every row action named for its row, the visible words inside the name (“Open check-in #6 of Sipho Ndlovu”); a row with no action says why.
 Badges read as words; “—” is read as “None”.
 The pager's select labelled; the focus on the list's heading after Show and a page turn; on the result after a save; on the summary after a refusal; the Already held warning announced in the Agenda's one live region without moving the focus.
 A refused group: named by its message, its stripe drawn as a border in forced colours, its summary link focusing the first radio; no summary line points at a disabled control.
 Targets: 36px buttons, 28px small buttons, the fold's summary 44px everywhere; below 641px every control and row action on these pages 44px.
 No page scrolls sideways at 390, 768 or 1024 (each board rendered at its width); the 120-character title wraps.
## Data the build must add
 New to the Spec in round 3: round 2 did not carry this list.
 A decision row's holding review: its id and its panel's name (T259); the trainee line's read: per trainee, period and panel, the open binding review (id, type, scheduled on) and any open formative check-ins.
 The Trainee filter's options: the current registrars in the reader's scope, by surname.
 A row's Schedule link carries the row's own period (F1); the form accepts an ended period and says so under Period.
 The default review type per panel and period (F4), and each type's help.
 The preview's Already held: the same check the create handler runs, returning the holding review's id and scheduled-on date.
 Panel routing per EPA for the two-panel case; the preview's “decided by {panel}” line.
 NothingToDecide: the preview's sentence for a panel that decides no EPA of the trainee's curriculum, and the handler's own refusal (placeholder on the board).
 The trainee picker's training year.
 A panel's open reviews (id, period, state) for About and the Decides for lock; the members who can no longer sit, by name as stored.
 The seat count's words; the result sentences (“… was created. Its reviews can now be scheduled on Committee reviews.”, “Panel members updated. …”).
 Committee reviews: the Type label “Formative check-in”; pseudonyms as stored (flow 14).
## Runbook steps outside this flow that change
 The build edits them; the boards need not: 2.35, 2.37, 4.14, A.1.14, A.7.5 to A.7.8, A.7.14, and every step that says “Formative only” (now a Review type) or reads a Mode column (gone).
## Flags
 F4's default in S1: drawn as Entrustment-only review. If the default is the first option today offers (Annual progression), it is one radio to move.
 S1's annual and optional agenda lines (“Due by year end”, “As opportunity allows”) are drawn as the planner is expected to give them; the build takes the planner's.
 Titles not in the brief are marked; the neonatal panel's name, the Administrator's name and two people are placeholders.
 NothingToDecide's refusal is a placeholder for the handler's own sentence.


######## BOARD R2-0-steps.dc.html — Steps · the 30 runbook steps once A is built
Steps · the 30 runbook steps once A is built 
## The runbook steps once A is built
 Route, Do and Expect as they become once A is built (round 3: 4.1 and 4.2 restored, M10; 4.8 and 4.13 check the warnings, F5; 5.1, 5.2, 4.6 and 4.9 keep their quoted checks, and the trainee line is played in 4.9, 4.12, 4.49 and A.1.10, S5).
 Step | Role | Route | Do | Expect | 
 2.20 | Institutional admin, Nolwazi Mbatha | /committee/panels | Open Decision panels. | While it loads, the header and skeletons, no New panel and no routing sentence. Then “No decision panels” / “Create a panel before scheduling reviews.”, New panel in the header; Who decides each EPA names Paediatric EPA Curriculum 11.1 with “No panel at this institution covers this programme.” against PAED-001 to PAED-015. | 
 2.21 | Institutional admin, Nolwazi Mbatha | /committee/panels/new | Name it “Paed Annual Review Panel”, Scope Speciality, Speciality Paediatrics, Decides for General panel. Seat Zulu and Naidoo as Member, then choose Zulu as Chair. Set Naidoo to Not seated and press Create panel. | Scope offers Institution and Speciality, on Speciality; Speciality offers Paediatrics alone; Decides for offers General panel and the Neonatal team Clinical Competency Committee. Chair and the seats list exactly Botha, Naidoo, van Rensburg and Zulu, by surname, with their whole addresses (van Rensburg's on sun.wombat.local). Zulu as chair: her row reads “Chair · chosen above” and “Thandi Zulu is the chair now, so is no longer seated as a member.” Refused: “The panel was not created. …” with “Seats: A panel needs at least two members: the chair and at least one other, so that no decision is one person's.”, the focus on it. | 
 2.22 | Institutional admin, Nolwazi Mbatha | /committee/panels/new → /committee/panels/{PanelId:int} | Seat Naidoo as Member and van Rensburg as External member; Create panel. | The panel's page, tab and h1 “Paed Annual Review Panel”, its subtitle, “Paed Annual Review Panel was created. …” with the focus; About this panel; Who sits on it (Zulu chair, Naidoo Member, van Rensburg External member, Botha Not seated); Decides for, General panel, Save committee and the rule. No “Back to panels”. | 
 2.23 | Speciality admin, Refilwe Mokoena | /account/login → / → /committee/panels → /committee/panels/{PanelId:int} → /committee/panels | Sign in, Decision panels, Edit. Seat Botha as Member, Save panel, back to Decision panels. | New panel and Edit offered (T194). Decides for reads “General panel” and “Only an institutional administrator can change which College committee a panel sits as.” “Panel members updated. Thandi Zulu chairs, with 2 members and 1 external member.” with the focus. The list's Members reads the four by name. | 
 2.24 | Institutional admin, Nolwazi Mbatha | /committee/panels | Read the list and Who decides each EPA. | One row: Paed Annual Review Panel, “Speciality · Paediatrics”, General panel, the four, Edit. PAED-004 and PAED-005 go to the panel with “No panel covering this programme sits as the Neonatal team Clinical Competency Committee, so the general panel decides.”; the other thirteen to the panel. | 
 2.25 | Sub-speciality admin, Kabelo Sithole | /account/login → / → /committee/panels → /committee/panels/new | Sign in, Decision panels, New panel; read; Cancel. | New panel and Edit. Scope reads “Speciality” as text; Speciality Paediatrics; no Decides for. Cancel returns; nothing created. | 
 2.32 | Coordinator, Pieter Smit | … → /committee/panels → /committee/panels/new → /access-denied | As today. | Decision panels lists the panel, no New panel, no Edit column, Decision panels lit. The form reads “You cannot open this page” / “Your role (Coordinator) does not open this page.” | 
 4.1 | Coordinator, Pieter Smit | / → /committee/decisions-due | Open Decisions due; leave Period on and Status on “Outstanding: not yet decided”; page through with Next; then Status Not scheduled, EPA PAED-004, Show. | The opening names , “5 trainees you oversee at Kgosi Kgari Teaching Hospital” and what Missed means; Period's help: “The College's semester 2 ends in November; its window runs to 31 December, so December's work counts.” By EPA (its fold opened): all 15 EPAs, each Due 5; twelve To schedule 5; PAED-008, 009 and 013 Optional 5. “75 of 75 decisions due in shown.”, 20 rows a page. Each semester EPA's row (PAED-001, 002, 004, 005, 010, 012): Not scheduled, window , “No open review holds it, and has not ended.” Each annual EPA's row (PAED-003, 006, 007, 011, 014, 015): Not scheduled, window , “No open review holds it, and has not ended.” The three optional: “As opportunity allows”. Every Not scheduled row offers Schedule, named by trainee, EPA and panel. After Next the focus is on “Decisions, page 2 of 4”. The filters leave five PAED-004 rows, one a registrar; By EPA does not change. No Missed row. | 
## The runbook steps once A is built (continued)
 Step | Role | Route | Do | Expect | 
 4.2 | Institutional admin, Nolwazi Mbatha | / → /committee/decisions-due → /committee/reviews/new | Open Decisions due; look for an Institution filter; switch Period to S1 and Show. Press Schedule on one Missed row and read the form; Cancel without creating. Back to . | No Institution filter. The same five registrars and 75 rows. For S1: every registrar's six semester EPAs read Missed (“ S1 has ended with nothing decided or on an open review's agenda.”, T361); Mahlangu's and Ndlovu's 15 January start falls inside the semester's first month, so their windows count (D42). The annual six read “Due by year end” (PAED-003 and 006 never Missed); PAED-008, 009 and 013 “As opportunity allows”. By EPA: Missed 5 on the six. The form opens with the panel, the registrar and Period S1, and under it “ S1 has ended. A review can still sit for it: sittings are often held after the period ends.”; the Agenda reads S1's plan: the six semester EPAs due, the annual six and the optional three as the planner gives them. Nothing is created. | 
 4.3 | Speciality admin, Refilwe Mokoena | /account/login → / → /committee/decisions-due | As today. | As today; 75 rows, each with one Schedule. | 
 4.4 | Sub-speciality admin, Kabelo Sithole | /account/login → / → /committee/decisions-due | As today. | As Step 4.3. | 
 4.5 | Committee member, David Naidoo | … → /committee/decisions-due → /access-denied | As today. | No Decisions due in his menu; the address reads “You cannot open this page” / “None of your roles (Committee member, Assessor) opens this page.” | 
 4.6 | Coordinator, Pieter Smit | /committee/reviews → /committee/reviews/new → /committee/reviews/{ReviewId:int} | Committee reviews, Schedule a review. Panel `Paed Annual Review Panel`; Trainee Dr Molefe; Period , the window as it fills; Scheduled on D; Review type Annual progression review (preselected); read the Agenda; Create review. | Panel: the one panel. Trainee empty until a panel is chosen, then the five, with their training years. Period labelled “ · 1 Jul to 31 Dec ”; choosing it fills the window as -01-01 to -12-31. Review type: “Annual progression review” and “Pre-graduation review”, with “The semester-2 sitting decides the trainee's progression for the year.”, and Formative check-in beside them (T309). The preview reads “15 EPAs will be on the agenda for .”, then “PAED-001, PAED-002, PAED-003, PAED-004, PAED-005, PAED-006, PAED-007, PAED-010, PAED-011, PAED-012, PAED-014 and PAED-015 are due at this sitting.”, and PAED-008, 009 and 013 as “As opportunity allows”. After Create review the review opens: h1 and tab “Lerato Molefe · ”; subtitle “Annual progression review · Paed Annual Review Panel · Training year 4”; the sitting bar Scheduled, “15 EPAs on the agenda”, “Summative · evidence -01-01 to -12-31”; Start review… offered; “No evidence has been frozen yet. Starting the review captures the current bundle.” | 
 4.7 | Coordinator, Pieter Smit | as 4.6 | As today, from Schedule a review. | Each opens Scheduled with a 15-EPA preview. Three rows for : Period “ ” over “ -01-01 to -12-31”, Type Annual progression, State Scheduled, Decision Pending; no Mode column. Each Open named by its review. | 
 4.8 | Coordinator, Pieter Smit | /committee/reviews → /committee/reviews/new | Schedule a review: the panel, Dr Molefe, . | Before Create, the Agenda reads “Already held. Review #N already puts Lerato Molefe before Paed Annual Review Panel for (scheduled on D). A trainee has one binding review for each period …”; Create review is not offered (“Create review is not offered: review #N already holds for Lerato Molefe.”); Open review #N is. Nothing is created; the list holds three rows. (The race refusal is a scratch state, not played here: F5.) | 
 4.9 | Speciality admin, Refilwe Mokoena | /committee/decisions-due → /committee/reviews/new → /committee/reviews/{ReviewId:int} | On Decisions due, set Trainee to Dr Molefe and Show; read the line; then Trainee Dr Mahlangu and Show; press Schedule a review on the line (or Schedule on her PAED-001 row); set Scheduled on D; create. | Molefe's, Dlamini's and du Plessis's rows read Scheduled, “On the agenda of review #N before Paed Annual Review Panel, which is still open.” (T259), each with Open review #N. With Dr Molefe filtered, the line: “Review #N (annual progression) holds before Paed Annual Review Panel, scheduled on D.” with Open review #N; with Dr Mahlangu, “No open review holds .” with Schedule a review. The form opens filled: Panel `Paed Annual Review Panel`, Trainee Dr Mahlangu, Period ; the preview shows 15 EPAs; the query fills only what the form offers, so the link authorises nothing. The review opens Scheduled; Start review… is not offered: “Only the coordinators of the panel's institution, and those of its members who are active committee members there, can start this review.” (T194, E2), with “As of hh:mm SAST” and Refresh. | 
## The runbook steps once A is built (continued)
 Step | Role | Route | Do | Expect | 
 4.10 | Sub-speciality admin, Kabelo Sithole | as 4.9 | On Decisions due, Schedule on one of Dr Ndlovu's rows; Scheduled on D; create; back to Decisions due. | As Step 4.9. Decisions due reads Scheduled on all 75 rows; By EPA (opened) reads Scheduled 5 and To schedule 0 for every EPA. | 
 4.11 | Committee member, David Naidoo | /committee/reviews → /committee/reviews/{ReviewId:int} | As today. | “Open existing committee reviews.”, no Schedule a review; five reviews, Scheduled, Pending; Start review… offered to him on Dlamini's. | 
 4.12 | Institutional admin, Nolwazi Mbatha | /committee/reviews → /committee/decisions-due | Read Committee reviews, then Decisions due; set Trainee to Dr Ndlovu and Show. | Schedule a review offered; five reviews. Decisions due reads Scheduled on all 75 rows, each with Open review #N; no Schedule is drawn. With Dr Ndlovu filtered, the line “Review #N (annual progression) holds before Paed Annual Review Panel, scheduled on D.” with Open review #N. | 
 4.13 | Institutional admin, Nolwazi Mbatha | /committee/panels → /committee/panels/{PanelId:int} | Open the panel; read Decides for. | Decides for reads “General panel” and “It cannot change now. Review #N ( , scheduled) still sits before this panel. What a review decides, and its agenda, follow …”, with Open review #N; no select, no Save committee; About lists the open reviews. (The race refusal is a scratch state: F5.) | 
 4.33 | Coordinator, Pieter Smit | /committee/reviews → /committee/reviews/{ReviewId:int} | As today. | All five Ratified; Satisfactory Progress for Molefe and Dlamini; Satisfactory with Observations for du Plessis; Inadequate Progress — Additional Training for Mahlangu; Outcome Deferred for Ndlovu. No enum name (T250); no Mode column. | 
 4.34 | Institutional admin, Nolwazi Mbatha | /committee/decisions-due | Read with the default filter, then Every status and Show; open By EPA. | “70 of 75 decisions due in shown.” Every status: Decided on five rows, “STAR #n, issued at review #N.” (Molefe's PAED-001, 010 and 012; Dlamini's PAED-001 and 002); Not decided on 55 (amber), “Not decided at review #N. A later sitting for plans it again.”, or for an annual EPA; As opportunity allows on 15. By EPA: PAED-001 Due 5, Decided 2, Not decided 3. A Not decided row offers only Schedule; its review is a link in the sentence (C3). | 
 4.37 | Sub-speciality admin, Kabelo Sithole | /admin/entrustment-decisions → /committee/decisions-due | As today; Decisions due with Every status; open By EPA. | Dlamini's PAED-002 reads “Revoked: re-decide”, “STAR #n, issued for , was revoked. Schedule a review to decide it again.”, and offers Schedule. By EPA: PAED-002 Decided 0, Not decided 4, To schedule 1. Default filter: “71 of 75 decisions due in shown.” | 
 4.49 | Coordinator, Pieter Smit | /committee/reviews → /committee/reviews/new → /committee/reviews/{ReviewId:int} → /committee/decisions-due | Schedule a review: the panel, Dr Ndlovu, , Scheduled on D, Review type Formative check-in; create. Then Decisions due, Trainee Dr Ndlovu, Show. | Accepted: a formative review takes no binding seat. The preview: “A formative review carries no agenda: it decides no EPA.” The review: h1 “Sipho Ndlovu · ”, subtitle “Formative check-in · Paed Annual Review Panel · Training year 1”, the sitting bar Scheduled and “Formative: an interim check-in, no binding decision · evidence -01-01 to -12-31”, Start review…; no Decision, Agenda or Appeals section. The list: Type “Formative check-in”, Decision “—”. Decisions due's line: “Formative check-in #N is open: it takes no seat.” with Open check-in #N. | 
## The runbook steps once A is built (continued)
 Step | Role | Route | Do | Expect | 
 5.1 | Coordinator, Pieter Smit | /committee/decisions-due | Decisions due for the period holding D; Trainee Dr Molefe, Status Every status, Show. | The line: “No open review holds .” with Schedule a review. One row for each EPA due for her, each with its status in words: an EPA an Act 4 STAR decided reads Decided and names the STAR and the review; an EPA due at Act 4's sitting and left unstaged reads Not decided (amber), “Not decided at review #N. A later sitting for … plans it again.”; the others Not scheduled, Due by year end or As opportunity allows: PAED-008, 009 and 013 read As opportunity allows while is open. Every row not yet decided offers Schedule. While the filter is set only her rows are listed; with Every trainee, KGK's other registrars too, and nobody outside KGK. | 
 5.2 | Coordinator, Pieter Smit | /committee/decisions-due → /committee/reviews/new → /committee/reviews/{ReviewId:int} | Schedule a review on the line (or Schedule on an outstanding row). Pre-graduation review; widen From to J−3y; Scheduled on D; create. | The form opens with the Paed Annual Review Panel, Dr Molefe and the period. The type offers Annual progression review and Pre-graduation review (Entrustment-only only in a semester-1 period), Pre-graduation's help “The final review before the programme ends: it decides the trainee's progression, and whether they may graduate.” The preview: “12 EPAs will be on the agenda for …”; “PAED-002, PAED-003, PAED-004, PAED-005, PAED-006, PAED-007, PAED-011, PAED-014 and PAED-015 are due at this sitting.”; PAED-008, 009 and 013 As opportunity allows; “Already decided in this window, so not on the agenda: PAED-001, PAED-010 and PAED-012.” The review: h1 “Lerato Molefe · ”, subtitle “Pre-graduation review · Paed Annual Review Panel · Training year 4”, the sitting bar Scheduled, “12 EPAs on the agenda”, “Summative · evidence J−3y to -12-31” (ISO); Start review… offered. | 
 5.29 | Coordinator, Pieter Smit | /committee/reviews → /committee/reviews/new → /committee/reviews | Schedule a review; choose the panel; read Trainee; Cancel. | Only Dr Dlamini, Dr Mahlangu and Dr Ndlovu, each with the training year (T238). Cancel returns to Committee reviews. | 
 A.1.10 | Coordinator, Pieter Smit | as 4.49 | As Step 4.49; note the review's id. | As Step 4.49, the line included. | 
 A.7.5 | Coordinator, Pieter Smit | / → … → /committee/decisions-due → /admin/data-rights | At 390px, as today. | Decisions due: the filters stacked, Show and Clear filters 44px; By EPA's fold summary 44px, opened one block per EPA; each decision a block with its action a 44px full-width button; the pager's buttons 44px. | 
 A.7.8 | Sub-speciality admin, Kabelo Sithole | / → /committee/reviews → /committee/decisions-due | At 390px, as today. | Committee reviews: Schedule a review a 44px row under the header; each review a block (trainee, Panel, Period over its window, Type, State, Decision, Open 44px). Decisions due as A.7.5. |


######## BOARD R2-1-panels-1280-a.dc.html — Decision panels · 1280 · a
Decision panels · 1280 · a 
# Decision panels · 1280px
 Route /committee/panels · a list the menu offers, so it lights itself and draws no trail.
 Components MainLayout, NavMenu, PageHeader, DataTable (stack-wide), FilterBar (the Administrator's, Show), StatePanel, Alert. NEW the page scope .committee-calendar.
 DESIGN.md § Table system: Scope names the speciality or institution, Members names who sits; the list and Who decides each EPA stack at 900px and below (M8). Round 3 F2, M8, S6.
=== FRAME: | Empty | | Step 2.20 · Nolwazi Mbatha | 
 [shell] 
# Decision panels
 The panels that take entrustment and progression decisions at Kgosi Kgari Teaching Hospital.
 New panel 
 No decision panels
 Create a panel before scheduling reviews.
## Who decides each EPA
 Which panel takes each EPA's entrustment decision for a trainee of each programme at Kgosi Kgari Teaching Hospital. An EPA the College gives to a named committee (EPAs 4 and 5 to the neonatal CCC) goes to a panel sitting as it, one covering the trainee's speciality before an institution-wide one, and to the general panels while none does.
### Paediatric EPA Curriculum 11.1
 Who decides each EPA of Paediatric EPA Curriculum 11.1
 EPAs | Decided by | 
 PAED-001 to PAED-015 | No panel at this institution covers this programme. | 
=== FRAME: | The manager's list, with Who decides each EPA | | Step 2.24 · Nolwazi Mbatha · stacked at 900px and below (M8) | 
 [shell] 
# Decision panels
 The panels that take entrustment and progression decisions at Kgosi Kgari Teaching Hospital.
 New panel 
 Decision panels at Kgosi Kgari Teaching Hospital
 Name | Scope | Decides for | Members | Actions | 
 Paed Annual Review Panel | Speciality · Paediatrics | General panel | 4: Thandi Zulu (chair), David Naidoo, Sarah Botha, John van Rensburg (external) | Edit 
 | 
## Who decides each EPA
 Which panel takes each EPA's entrustment decision for a trainee of each programme at Kgosi Kgari Teaching Hospital. An EPA the College gives to a named committee (EPAs 4 and 5 to the neonatal CCC) goes to a panel sitting as it, one covering the trainee's speciality before an institution-wide one, and to the general panels while none does.
### Paediatric EPA Curriculum 11.1
 Who decides each EPA of Paediatric EPA Curriculum 11.1
 EPAs | Decided by | 
 PAED-001 to PAED-003, PAED-006 to PAED-015 | Paed Annual Review Panel | 
 PAED-004, PAED-005 | Paed Annual Review Panel. No panel covering this programme sits as the Neonatal team Clinical Competency Committee, so the general panel decides. | 
=== FRAME: | Heavy: a panel sitting as the neonatal committee | | Nolwazi Mbatha | 
 [shell] 
# Decision panels
 The panels that take entrustment and progression decisions at Kgosi Kgari Teaching Hospital.
 New panel 
 Decision panels at Kgosi Kgari Teaching Hospital
 Name | Scope | Decides for | Members | Actions | 
 Paed Annual Review Panel | Speciality · Paediatrics | General panel | 4: Thandi Zulu (chair), David Naidoo, Sarah Botha, John van Rensburg (external) | Edit 
 | 
 [Neonatal panel] | Speciality · Paediatrics | Neonatal team Clinical Competency Committee | 3: Sarah Botha (chair), David Naidoo, John van Rensburg (external) | Edit 
 | 
## Who decides each EPA
 Which panel takes each EPA's entrustment decision for a trainee of each programme at Kgosi Kgari Teaching Hospital. An EPA the College gives to a named committee (EPAs 4 and 5 to the neonatal CCC) goes to a panel sitting as it, one covering the trainee's speciality before an institution-wide one, and to the general panels while none does.
### Paediatric EPA Curriculum 11.1
 Who decides each EPA of Paediatric EPA Curriculum 11.1
 EPAs | Decided by | 
 PAED-001 to PAED-003, PAED-006 to PAED-015 | Paed Annual Review Panel | 
 PAED-004, PAED-005 | [Neonatal panel], sitting as the Neonatal team Clinical Competency Committee. | 
=== FRAME: | A speciality admin's | | Step 2.23 · Refilwe Mokoena | 
 [shell] 
# Decision panels
 The panels that take entrustment and progression decisions at Kgosi Kgari Teaching Hospital.
 New panel 
 Decision panels at Kgosi Kgari Teaching Hospital
 Name | Scope | Decides for | Members | Actions | 
 Paed Annual Review Panel | Speciality · Paediatrics | General panel | 4: Thandi Zulu (chair), David Naidoo, Sarah Botha, John van Rensburg (external) | Edit 
 | 
## Who decides each EPA
 Which panel takes each EPA's entrustment decision for a trainee of each programme at Kgosi Kgari Teaching Hospital. An EPA the College gives to a named committee (EPAs 4 and 5 to the neonatal CCC) goes to a panel sitting as it, one covering the trainee's speciality before an institution-wide one, and to the general panels while none does.
### Paediatric EPA Curriculum 11.1
 Who decides each EPA of Paediatric EPA Curriculum 11.1
 EPAs | Decided by | 
 PAED-001 to PAED-003, PAED-006 to PAED-015 | Paed Annual Review Panel | 
 PAED-004, PAED-005 | Paed Annual Review Panel. No panel covering this programme sits as the Neonatal team Clinical Competency Committee, so the general panel decides. | 
=== FRAME: | The Coordinator's: no New panel, no Edit | | Step 2.32 · Pieter Smit | 
 [shell] 
# Decision panels
 The panels that take entrustment and progression decisions at Kgosi Kgari Teaching Hospital.
 Decision panels at Kgosi Kgari Teaching Hospital
 Name | Scope | Decides for | Members | 
 Paed Annual Review Panel | Speciality · Paediatrics | General panel | 4: Thandi Zulu (chair), David Naidoo, Sarah Botha, John van Rensburg (external) | 
## Who decides each EPA
 Which panel takes each EPA's entrustment decision for a trainee of each programme at Kgosi Kgari Teaching Hospital. An EPA the College gives to a named committee (EPAs 4 and 5 to the neonatal CCC) goes to a panel sitting as it, one covering the trainee's speciality before an institution-wide one, and to the general panels while none does.
### Paediatric EPA Curriculum 11.1
 Who decides each EPA of Paediatric EPA Curriculum 11.1
 EPAs | Decided by | 
 PAED-001 to PAED-003, PAED-006 to PAED-015 | Paed Annual Review Panel | 
 PAED-004, PAED-005 | Paed Annual Review Panel. No panel covering this programme sits as the Neonatal team Clinical Competency Committee, so the general panel decides. | 
=== FRAME: | A member's: no New panel, no Edit | | David Naidoo | 
 [shell] 
# Decision panels
 The panels that take entrustment and progression decisions at Kgosi Kgari Teaching Hospital.
 Decision panels at Kgosi Kgari Teaching Hospital
 Name | Scope | Decides for | Members | 
 Paed Annual Review Panel | Speciality · Paediatrics | General panel | 4: Thandi Zulu (chair), David Naidoo, Sarah Botha, John van Rensburg (external) | 
## Who decides each EPA
 Which panel takes each EPA's entrustment decision for a trainee of each programme at Kgosi Kgari Teaching Hospital. An EPA the College gives to a named committee (EPAs 4 and 5 to the neonatal CCC) goes to a panel sitting as it, one covering the trainee's speciality before an institution-wide one, and to the general panels while none does.
### Paediatric EPA Curriculum 11.1
 Who decides each EPA of Paediatric EPA Curriculum 11.1
 EPAs | Decided by | 
 PAED-001 to PAED-003, PAED-006 to PAED-015 | Paed Annual Review Panel | 
 PAED-004, PAED-005 | Paed Annual Review Panel. No panel covering this programme sits as the Neonatal team Clinical Competency Committee, so the general panel decides. | 
=== FRAME: | An Administrator choosing an institution | | Administrator · Show (F2) | 
 [shell] 
# Decision panels
 The panels that take entrustment and progression decisions, by institution.
 New panel 
 Institution
 Select institution
 Kgosi Kgari Teaching Hospital
 Show 
 Choose an institution
 An administrator belongs to no institution, so the panels are shown for the one chosen above.
## Who decides each EPA
 Choose an institution to see which panel decides each EPA there.


######## BOARD R2-1-panels-1280-b.dc.html — Decision panels · 1280 · b
Decision panels · 1280 · b 
# Decision panels · 1280px (continued, part b) 
 Route /committee/panels · a list the menu offers, so it lights itself and draws no trail.
 Components MainLayout, NavMenu, PageHeader, DataTable (stack-wide), FilterBar (the Administrator's, Show), StatePanel, Alert. NEW the page scope .committee-calendar.
 DESIGN.md § Table system: Scope names the speciality or institution, Members names who sits; the list and Who decides each EPA stack at 900px and below (M8). Round 3 F2, M8, S6.
=== FRAME: | A panel manager with nowhere to create (T260) | | A speciality admin whose institution has adopted no curriculum in the speciality | 
 [shell] 
# Decision panels
 The panels that take entrustment and progression decisions at Kgosi Kgari Teaching Hospital.
 You cannot create a decision panel. A speciality or sub-speciality administrator creates panels for their own speciality at their own institution, once the institution has adopted a curriculum in it. An institutional administrator creates the institution's panels.
 Decision panels at Kgosi Kgari Teaching Hospital
 Name | Scope | Decides for | Members | 
 Paed Annual Review Panel | Speciality · Paediatrics | General panel | 4: Thandi Zulu (chair), David Naidoo, Sarah Botha, John van Rensburg (external) | 
## Who decides each EPA
 Which panel takes each EPA's entrustment decision for a trainee of each programme at Kgosi Kgari Teaching Hospital. An EPA the College gives to a named committee (EPAs 4 and 5 to the neonatal CCC) goes to a panel sitting as it, one covering the trainee's speciality before an institution-wide one, and to the general panels while none does.
### Paediatric EPA Curriculum 11.1
 Who decides each EPA of Paediatric EPA Curriculum 11.1
 EPAs | Decided by | 
 PAED-001 to PAED-003, PAED-006 to PAED-015 | Paed Annual Review Panel | 
 PAED-004, PAED-005 | Paed Annual Review Panel. No panel covering this programme sits as the Neonatal team Clinical Competency Committee, so the general panel decides. | 
=== FRAME: | Loading | | C11, T329 · no New panel until the read is in; no routing sentence while it loads | 
 [shell] 
# Decision panels
 The panels that take entrustment and progression decisions at Kgosi Kgari Teaching Hospital.
 Loading Decision panels.
## Who decides each EPA
=== FRAME: | Load error | | C11, T338 | | Focus: the alert | 
 [shell] 
# Decision panels
 The panels that take entrustment and progression decisions at Kgosi Kgari Teaching Hospital.
 Could not load Decision panels. Nothing has changed. Try again, or come back in a few minutes. Try again


######## BOARD R2-1-panels-768.dc.html — Decision panels · 768
Decision panels · 768 
# Decision panels · 768px
 Route /committee/panels · a list the menu offers, so it lights itself and draws no trail.
 Components MainLayout, NavMenu, PageHeader, DataTable (stack-wide), FilterBar (the Administrator's, Show), StatePanel, Alert. NEW the page scope .committee-calendar.
 DESIGN.md § Table system: Scope names the speciality or institution, Members names who sits; the list and Who decides each EPA stack at 900px and below (M8). Round 3 F2, M8, S6.
=== FRAME: | The manager's list, with Who decides each EPA | | Step 2.24 · Nolwazi Mbatha · stacked at 900px and below (M8) | 
 [shell] 
# Decision panels
 The panels that take entrustment and progression decisions at Kgosi Kgari Teaching Hospital.
 New panel 
 Decision panels at Kgosi Kgari Teaching Hospital
 Name | Scope | Decides for | Members | Actions | 
 Paed Annual Review Panel | Speciality · Paediatrics | General panel | 4: Thandi Zulu (chair), David Naidoo, Sarah Botha, John van Rensburg (external) | Edit 
 | 
## Who decides each EPA
 Which panel takes each EPA's entrustment decision for a trainee of each programme at Kgosi Kgari Teaching Hospital. An EPA the College gives to a named committee (EPAs 4 and 5 to the neonatal CCC) goes to a panel sitting as it, one covering the trainee's speciality before an institution-wide one, and to the general panels while none does.
### Paediatric EPA Curriculum 11.1
 Who decides each EPA of Paediatric EPA Curriculum 11.1
 EPAs | Decided by | 
 PAED-001 to PAED-003, PAED-006 to PAED-015 | Paed Annual Review Panel | 
 PAED-004, PAED-005 | Paed Annual Review Panel. No panel covering this programme sits as the Neonatal team Clinical Competency Committee, so the general panel decides. | 
=== FRAME: | Heavy: a panel sitting as the neonatal committee | | Nolwazi Mbatha | 
 [shell] 
# Decision panels
 The panels that take entrustment and progression decisions at Kgosi Kgari Teaching Hospital.
 New panel 
 Decision panels at Kgosi Kgari Teaching Hospital
 Name | Scope | Decides for | Members | Actions | 
 Paed Annual Review Panel | Speciality · Paediatrics | General panel | 4: Thandi Zulu (chair), David Naidoo, Sarah Botha, John van Rensburg (external) | Edit 
 | 
 [Neonatal panel] | Speciality · Paediatrics | Neonatal team Clinical Competency Committee | 3: Sarah Botha (chair), David Naidoo, John van Rensburg (external) | Edit 
 | 
## Who decides each EPA
 Which panel takes each EPA's entrustment decision for a trainee of each programme at Kgosi Kgari Teaching Hospital. An EPA the College gives to a named committee (EPAs 4 and 5 to the neonatal CCC) goes to a panel sitting as it, one covering the trainee's speciality before an institution-wide one, and to the general panels while none does.
### Paediatric EPA Curriculum 11.1
 Who decides each EPA of Paediatric EPA Curriculum 11.1
 EPAs | Decided by | 
 PAED-001 to PAED-003, PAED-006 to PAED-015 | Paed Annual Review Panel | 
 PAED-004, PAED-005 | [Neonatal panel], sitting as the Neonatal team Clinical Competency Committee. |


######## BOARD R2-1-panels-390-a.dc.html — Decision panels · 390 · a
Decision panels · 390 · a 
# Decision panels · 390px
 Route /committee/panels · a list the menu offers, so it lights itself and draws no trail.
 Components MainLayout, NavMenu, PageHeader, DataTable (stack-wide), FilterBar (the Administrator's, Show), StatePanel, Alert. NEW the page scope .committee-calendar.
 DESIGN.md § Table system: Scope names the speciality or institution, Members names who sits; the list and Who decides each EPA stack at 900px and below (M8). Round 3 F2, M8, S6.
=== FRAME: | Empty | | Step 2.20 · Nolwazi Mbatha | 
 [shell] 
# Decision panels
 The panels that take entrustment and progression decisions at Kgosi Kgari Teaching Hospital.
 New panel 
 No decision panels
 Create a panel before scheduling reviews.
## Who decides each EPA
 Which panel takes each EPA's entrustment decision for a trainee of each programme at Kgosi Kgari Teaching Hospital. An EPA the College gives to a named committee (EPAs 4 and 5 to the neonatal CCC) goes to a panel sitting as it, one covering the trainee's speciality before an institution-wide one, and to the general panels while none does.
### Paediatric EPA Curriculum 11.1
 Who decides each EPA of Paediatric EPA Curriculum 11.1
 EPAs | Decided by | 
 PAED-001 to PAED-015 | No panel at this institution covers this programme. | 
=== FRAME: | The manager's list, with Who decides each EPA | | Step 2.24 · Nolwazi Mbatha · stacked at 900px and below (M8) | 
 [shell] 
# Decision panels
 The panels that take entrustment and progression decisions at Kgosi Kgari Teaching Hospital.
 New panel 
 Decision panels at Kgosi Kgari Teaching Hospital
 Name | Scope | Decides for | Members | Actions | 
 Paed Annual Review Panel | Speciality · Paediatrics | General panel | 4: Thandi Zulu (chair), David Naidoo, Sarah Botha, John van Rensburg (external) | Edit 
 | 
## Who decides each EPA
 Which panel takes each EPA's entrustment decision for a trainee of each programme at Kgosi Kgari Teaching Hospital. An EPA the College gives to a named committee (EPAs 4 and 5 to the neonatal CCC) goes to a panel sitting as it, one covering the trainee's speciality before an institution-wide one, and to the general panels while none does.
### Paediatric EPA Curriculum 11.1
 Who decides each EPA of Paediatric EPA Curriculum 11.1
 EPAs | Decided by | 
 PAED-001 to PAED-003, PAED-006 to PAED-015 | Paed Annual Review Panel | 
 PAED-004, PAED-005 | Paed Annual Review Panel. No panel covering this programme sits as the Neonatal team Clinical Competency Committee, so the general panel decides. | 
=== FRAME: | Heavy: a panel sitting as the neonatal committee | | Nolwazi Mbatha | 
 [shell] 
# Decision panels
 The panels that take entrustment and progression decisions at Kgosi Kgari Teaching Hospital.
 New panel 
 Decision panels at Kgosi Kgari Teaching Hospital
 Name | Scope | Decides for | Members | Actions | 
 Paed Annual Review Panel | Speciality · Paediatrics | General panel | 4: Thandi Zulu (chair), David Naidoo, Sarah Botha, John van Rensburg (external) | Edit 
 | 
 [Neonatal panel] | Speciality · Paediatrics | Neonatal team Clinical Competency Committee | 3: Sarah Botha (chair), David Naidoo, John van Rensburg (external) | Edit 
 | 
## Who decides each EPA
 Which panel takes each EPA's entrustment decision for a trainee of each programme at Kgosi Kgari Teaching Hospital. An EPA the College gives to a named committee (EPAs 4 and 5 to the neonatal CCC) goes to a panel sitting as it, one covering the trainee's speciality before an institution-wide one, and to the general panels while none does.
### Paediatric EPA Curriculum 11.1
 Who decides each EPA of Paediatric EPA Curriculum 11.1
 EPAs | Decided by | 
 PAED-001 to PAED-003, PAED-006 to PAED-015 | Paed Annual Review Panel | 
 PAED-004, PAED-005 | [Neonatal panel], sitting as the Neonatal team Clinical Competency Committee. | 
=== FRAME: | A speciality admin's | | Step 2.23 · Refilwe Mokoena | 
 [shell] 
# Decision panels
 The panels that take entrustment and progression decisions at Kgosi Kgari Teaching Hospital.
 New panel 
 Decision panels at Kgosi Kgari Teaching Hospital
 Name | Scope | Decides for | Members | Actions | 
 Paed Annual Review Panel | Speciality · Paediatrics | General panel | 4: Thandi Zulu (chair), David Naidoo, Sarah Botha, John van Rensburg (external) | Edit 
 | 
## Who decides each EPA
 Which panel takes each EPA's entrustment decision for a trainee of each programme at Kgosi Kgari Teaching Hospital. An EPA the College gives to a named committee (EPAs 4 and 5 to the neonatal CCC) goes to a panel sitting as it, one covering the trainee's speciality before an institution-wide one, and to the general panels while none does.
### Paediatric EPA Curriculum 11.1
 Who decides each EPA of Paediatric EPA Curriculum 11.1
 EPAs | Decided by | 
 PAED-001 to PAED-003, PAED-006 to PAED-015 | Paed Annual Review Panel | 
 PAED-004, PAED-005 | Paed Annual Review Panel. No panel covering this programme sits as the Neonatal team Clinical Competency Committee, so the general panel decides. | 
=== FRAME: | The Coordinator's: no New panel, no Edit | | Step 2.32 · Pieter Smit | 
 [shell] 
# Decision panels
 The panels that take entrustment and progression decisions at Kgosi Kgari Teaching Hospital.
 Decision panels at Kgosi Kgari Teaching Hospital
 Name | Scope | Decides for | Members | 
 Paed Annual Review Panel | Speciality · Paediatrics | General panel | 4: Thandi Zulu (chair), David Naidoo, Sarah Botha, John van Rensburg (external) | 
## Who decides each EPA
 Which panel takes each EPA's entrustment decision for a trainee of each programme at Kgosi Kgari Teaching Hospital. An EPA the College gives to a named committee (EPAs 4 and 5 to the neonatal CCC) goes to a panel sitting as it, one covering the trainee's speciality before an institution-wide one, and to the general panels while none does.
### Paediatric EPA Curriculum 11.1
 Who decides each EPA of Paediatric EPA Curriculum 11.1
 EPAs | Decided by | 
 PAED-001 to PAED-003, PAED-006 to PAED-015 | Paed Annual Review Panel | 
 PAED-004, PAED-005 | Paed Annual Review Panel. No panel covering this programme sits as the Neonatal team Clinical Competency Committee, so the general panel decides. | 
=== FRAME: | A member's: no New panel, no Edit | | David Naidoo | 
 [shell] 
# Decision panels
 The panels that take entrustment and progression decisions at Kgosi Kgari Teaching Hospital.
 Decision panels at Kgosi Kgari Teaching Hospital
 Name | Scope | Decides for | Members | 
 Paed Annual Review Panel | Speciality · Paediatrics | General panel | 4: Thandi Zulu (chair), David Naidoo, Sarah Botha, John van Rensburg (external) | 
## Who decides each EPA
 Which panel takes each EPA's entrustment decision for a trainee of each programme at Kgosi Kgari Teaching Hospital. An EPA the College gives to a named committee (EPAs 4 and 5 to the neonatal CCC) goes to a panel sitting as it, one covering the trainee's speciality before an institution-wide one, and to the general panels while none does.
### Paediatric EPA Curriculum 11.1
 Who decides each EPA of Paediatric EPA Curriculum 11.1
 EPAs | Decided by | 
 PAED-001 to PAED-003, PAED-006 to PAED-015 | Paed Annual Review Panel | 
 PAED-004, PAED-005 | Paed Annual Review Panel. No panel covering this programme sits as the Neonatal team Clinical Competency Committee, so the general panel decides. |


######## BOARD R2-1-panels-390-b.dc.html — Decision panels · 390 · b
Decision panels · 390 · b 
# Decision panels · 390px (continued, part b) 
 Route /committee/panels · a list the menu offers, so it lights itself and draws no trail.
 Components MainLayout, NavMenu, PageHeader, DataTable (stack-wide), FilterBar (the Administrator's, Show), StatePanel, Alert. NEW the page scope .committee-calendar.
 DESIGN.md § Table system: Scope names the speciality or institution, Members names who sits; the list and Who decides each EPA stack at 900px and below (M8). Round 3 F2, M8, S6.
=== FRAME: | An Administrator choosing an institution | | Administrator · Show (F2) | 
 [shell] 
# Decision panels
 The panels that take entrustment and progression decisions, by institution.
 New panel 
 Institution
 Select institution
 Kgosi Kgari Teaching Hospital
 Show 
 Choose an institution
 An administrator belongs to no institution, so the panels are shown for the one chosen above.
## Who decides each EPA
 Choose an institution to see which panel decides each EPA there.
=== FRAME: | A panel manager with nowhere to create (T260) | | A speciality admin whose institution has adopted no curriculum in the speciality | 
 [shell] 
# Decision panels
 The panels that take entrustment and progression decisions at Kgosi Kgari Teaching Hospital.
 You cannot create a decision panel. A speciality or sub-speciality administrator creates panels for their own speciality at their own institution, once the institution has adopted a curriculum in it. An institutional administrator creates the institution's panels.
 Decision panels at Kgosi Kgari Teaching Hospital
 Name | Scope | Decides for | Members | 
 Paed Annual Review Panel | Speciality · Paediatrics | General panel | 4: Thandi Zulu (chair), David Naidoo, Sarah Botha, John van Rensburg (external) | 
## Who decides each EPA
 Which panel takes each EPA's entrustment decision for a trainee of each programme at Kgosi Kgari Teaching Hospital. An EPA the College gives to a named committee (EPAs 4 and 5 to the neonatal CCC) goes to a panel sitting as it, one covering the trainee's speciality before an institution-wide one, and to the general panels while none does.
### Paediatric EPA Curriculum 11.1
 Who decides each EPA of Paediatric EPA Curriculum 11.1
 EPAs | Decided by | 
 PAED-001 to PAED-003, PAED-006 to PAED-015 | Paed Annual Review Panel | 
 PAED-004, PAED-005 | Paed Annual Review Panel. No panel covering this programme sits as the Neonatal team Clinical Competency Committee, so the general panel decides. | 
=== FRAME: | Loading | | C11, T329 · no New panel until the read is in; no routing sentence while it loads | 
 [shell] 
# Decision panels
 The panels that take entrustment and progression decisions at Kgosi Kgari Teaching Hospital.
 Loading Decision panels.
## Who decides each EPA
=== FRAME: | Load error | | C11, T338 | | Focus: the alert | 
 [shell] 
# Decision panels
 The panels that take entrustment and progression decisions at Kgosi Kgari Teaching Hospital.
 Could not load Decision panels. Nothing has changed. Try again, or come back in a few minutes. Try again


######## BOARD R2-2-panel-1280-a.dc.html — A panel · 1280 · a
A panel · 1280 · a 
# A panel · 1280px
 Routes /committee/panels/new and /committee/panels/{PanelId} · the owner table's row for a panel unchanged; the Coordinator is refused.
 Components PageHeader, Breadcrumbs, FormField, FormActions, RefusalSummary, Alert, DetailCard and details-list, DataTable (Stack), .check-grid and .form-check radios, StatePanel, NotFound, AccessDenied. NEW .choice-group, .choice-row, .person-cell, .readonly-value, .form-section-title, .panel-section.
 DESIGN.md § Form system: the seat picker (one radio group a committee member) replaces the native multi-selects; § Page-level patterns: a panel's page titled by its name. Round 3 M6, M7, F5, S1, S3, S4, S6, S7.
=== FRAME: | New panel: the Institutional admin's | | Step 2.21 · Nolwazi Mbatha · addresses whole, on .local domains (S3) | | Focus: the h1, on arrival | 
 [shell] 
 Home 
 Decision panels 
 New panel 
# New panel
 A panel takes entrustment and progression decisions for the trainees it covers. Its name, scope, institution and speciality cannot change once it is created.
## About this panel
 Panel name * required 
 Scope
 Institution
 Speciality
 Speciality
 Paediatrics
 The panel runs at your institution. Only the specialities it has adopted a curriculum in are listed. 
 Decides for
 General panel
 Neonatal team Clinical Competency Committee
 A panel sitting as a College committee decides the EPAs the College gives that committee (the neonatal CCC decides EPAs 4 and 5), and nothing else. Where no panel sits as it, the general panels decide them. 
## Who sits on it
 Chair
 Choose a chair
 Sarah Botha (botha@kgk.wombat.local)
 David Naidoo (naidoo@kgk.wombat.local)
 John van Rensburg (vanrensburg@sun.wombat.local)
 Thandi Zulu (zulu@kgk.wombat.local)
 Only active committee members at the panel's institution who are not trainees are listed. 
 Seats on the panel: the active committee members at Kgosi Kgari Teaching Hospital who are not trainees, by surname
 Committee member | Seat | 
 Sarah Botha botha@kgk.wombat.local 
 | Seat for Sarah Botha
 Not seated
 Member
 External member
 | 
 David Naidoo naidoo@kgk.wombat.local 
 | Seat for David Naidoo
 Not seated
 Member
 External member
 | 
 John van Rensburg vanrensburg@sun.wombat.local 
 | Seat for John van Rensburg
 Not seated
 Member
 External member
 | 
 Thandi Zulu zulu@kgk.wombat.local 
 | Seat for Thandi Zulu
 Not seated
 Member
 External member
 | 
 0 seats: no chair, 0 members and 0 external members.
 External members are external examiners, who sit with the chair on the panel's appeal body. Only active committee members at the panel's institution who are not trainees are listed: cross-institution externals are not yet supported. 
 Cancel Create panel 
=== FRAME: | New panel: a sub-speciality admin's | | Step 2.25 · Kabelo Sithole · Scope as text; no Decides for | 
 [shell] 
 Home 
 Decision panels 
 New panel 
# New panel
 A panel takes entrustment and progression decisions for the trainees it covers. Its name, scope, institution and speciality cannot change once it is created.
## About this panel
 Panel name * required 
 Scope | Speciality | A sub-speciality administrator creates Speciality-scoped panels for the speciality of the sub-speciality. | Speciality
 Paediatrics
 The panel runs at your institution. Only the specialities it has adopted a curriculum in are listed. 
## Who sits on it
 Chair
 Choose a chair
 Sarah Botha (botha@kgk.wombat.local)
 David Naidoo (naidoo@kgk.wombat.local)
 John van Rensburg (vanrensburg@sun.wombat.local)
 Thandi Zulu (zulu@kgk.wombat.local)
 Only active committee members at the panel's institution who are not trainees are listed. 
 Seats on the panel: the active committee members at Kgosi Kgari Teaching Hospital who are not trainees, by surname
 Committee member | Seat | 
 Sarah Botha botha@kgk.wombat.local 
 | Seat for Sarah Botha
 Not seated
 Member
 External member
 | 
 David Naidoo naidoo@kgk.wombat.local 
 | Seat for David Naidoo
 Not seated
 Member
 External member
 | 
 John van Rensburg vanrensburg@sun.wombat.local 
 | Seat for John van Rensburg
 Not seated
 Member
 External member
 | 
 Thandi Zulu zulu@kgk.wombat.local 
 | Seat for Thandi Zulu
 Not seated
 Member
 External member
 | 
 0 seats: no chair, 0 members and 0 external members.
 External members are external examiners, who sit with the chair on the panel's appeal body. Only active committee members at the panel's institution who are not trainees are listed: cross-institution externals are not yet supported. 
 Cancel Create panel 
=== FRAME: | The chair chosen | | Step 2.21 · Nolwazi Mbatha | | Focus: stays on Chair; the note is announced | 
 [shell] 
 Home 
 Decision panels 
 New panel 
# New panel
 A panel takes entrustment and progression decisions for the trainees it covers. Its name, scope, institution and speciality cannot change once it is created.
## About this panel
 Panel name * required 
 Scope
 Institution
 Speciality
 Speciality
 Paediatrics
 The panel runs at your institution. Only the specialities it has adopted a curriculum in are listed. 
 Decides for
 General panel
 Neonatal team Clinical Competency Committee
 A panel sitting as a College committee decides the EPAs the College gives that committee (the neonatal CCC decides EPAs 4 and 5), and nothing else. Where no panel sits as it, the general panels decide them. 
## Who sits on it
 Chair
 Choose a chair
 Sarah Botha (botha@kgk.wombat.local)
 David Naidoo (naidoo@kgk.wombat.local)
 John van Rensburg (vanrensburg@sun.wombat.local)
 Thandi Zulu (zulu@kgk.wombat.local)
 Thandi Zulu is the chair now, so is no longer seated as a member.
 Only active committee members at the panel's institution who are not trainees are listed. 
 Seats on the panel: the active committee members at Kgosi Kgari Teaching Hospital who are not trainees, by surname
 Committee member | Seat | 
 Sarah Botha botha@kgk.wombat.local 
 | Seat for Sarah Botha
 Not seated
 Member
 External member
 | 
 David Naidoo naidoo@kgk.wombat.local 
 | Seat for David Naidoo
 Not seated
 Member
 External member
 | 
 John van Rensburg vanrensburg@sun.wombat.local 
 | Seat for John van Rensburg
 Not seated
 Member
 External member
 | 
 Thandi Zulu zulu@kgk.wombat.local 
 | Chair · chosen above 
 | 
 2 seats: the chair, 1 member and 0 external members.
 External members are external examiners, who sit with the chair on the panel's appeal body. Only active committee members at the panel's institution who are not trainees are listed: cross-institution externals are not yet supported. 
 Cancel Create panel 
=== FRAME: | A panel of one, refused | | Step 2.21 · Nolwazi Mbatha · the group named by its message (aria-describedby); the summary's link focuses the first radio; a stripe in forced colours too; no padding shift (S6) | | Focus: the summary | 
 [shell] 
 Home 
 Decision panels 
 New panel 
# New panel
 A panel takes entrustment and progression decisions for the trainees it covers. Its name, scope, institution and speciality cannot change once it is created.
 The panel was not created. Everything you chose is kept below. Seats: A panel needs at least two members: the chair and at least one other, so that no decision is one person's. 
## About this panel
 Panel name * required 
 Scope
 Institution
 Speciality
 Speciality
 Paediatrics
 The panel runs at your institution. Only the specialities it has adopted a curriculum in are listed. 
 Decides for
 General panel
 Neonatal team Clinical Competency Committee
 A panel sitting as a College committee decides the EPAs the College gives that committee (the neonatal CCC decides EPAs 4 and 5), and nothing else. Where no panel sits as it, the general panels decide them. 
## Who sits on it
 Chair
 Choose a chair
 Sarah Botha (botha@kgk.wombat.local)
 David Naidoo (naidoo@kgk.wombat.local)
 John van Rensburg (vanrensburg@sun.wombat.local)
 Thandi Zulu (zulu@kgk.wombat.local)
 Only active committee members at the panel's institution who are not trainees are listed. 
 A panel needs at least two members: the chair and at least one other, so that no decision is one person's.
 Seats on the panel: the active committee members at Kgosi Kgari Teaching Hospital who are not trainees, by surname
 Committee member | Seat | 
 Sarah Botha botha@kgk.wombat.local 
 | Seat for Sarah Botha
 Not seated
 Member
 External member
 | 
 David Naidoo naidoo@kgk.wombat.local 
 | Seat for David Naidoo
 Not seated
 Member
 External member
 | 
 John van Rensburg vanrensburg@sun.wombat.local 
 | Seat for John van Rensburg
 Not seated
 Member
 External member
 | 
 Thandi Zulu zulu@kgk.wombat.local 
 | Chair · chosen above 
 | 
 1 seat: the chair, 0 members and 0 external members.
 External members are external examiners, who sit with the chair on the panel's appeal body. Only active committee members at the panel's institution who are not trainees are listed: cross-institution externals are not yet supported. 
 Cancel Create panel


######## BOARD R2-2-panel-1280-b.dc.html — A panel · 1280 · b
A panel · 1280 · b 
# A panel · 1280px (continued, part b) 
 Routes /committee/panels/new and /committee/panels/{PanelId} · the owner table's row for a panel unchanged; the Coordinator is refused.
 Components PageHeader, Breadcrumbs, FormField, FormActions, RefusalSummary, Alert, DetailCard and details-list, DataTable (Stack), .check-grid and .form-check radios, StatePanel, NotFound, AccessDenied. NEW .choice-group, .choice-row, .person-cell, .readonly-value, .form-section-title, .panel-section.
 DESIGN.md § Form system: the seat picker (one radio group a committee member) replaces the native multi-selects; § Page-level patterns: a panel's page titled by its name. Round 3 M6, M7, F5, S1, S3, S4, S6, S7.
=== FRAME: | Name and chair missing (T324) | | Nolwazi Mbatha | | Focus: the summary | 
 [shell] 
 Home 
 Decision panels 
 New panel 
# New panel
 A panel takes entrustment and progression decisions for the trainees it covers. Its name, scope, institution and speciality cannot change once it is created.
 The panel was not created. Everything you chose is kept below. Panel name: Give the panel a name. 
 Chair: Choose a chair. 
## About this panel
 Panel name * required 
 Give the panel a name.
 Scope
 Institution
 Speciality
 Speciality
 Paediatrics
 The panel runs at your institution. Only the specialities it has adopted a curriculum in are listed. 
 Decides for
 General panel
 Neonatal team Clinical Competency Committee
 A panel sitting as a College committee decides the EPAs the College gives that committee (the neonatal CCC decides EPAs 4 and 5), and nothing else. Where no panel sits as it, the general panels decide them. 
## Who sits on it
 Chair
 Choose a chair
 Sarah Botha (botha@kgk.wombat.local)
 David Naidoo (naidoo@kgk.wombat.local)
 John van Rensburg (vanrensburg@sun.wombat.local)
 Thandi Zulu (zulu@kgk.wombat.local)
 Choose a chair.
 Only active committee members at the panel's institution who are not trainees are listed. 
 Seats on the panel: the active committee members at Kgosi Kgari Teaching Hospital who are not trainees, by surname
 Committee member | Seat | 
 Sarah Botha botha@kgk.wombat.local 
 | Seat for Sarah Botha
 Not seated
 Member
 External member
 | 
 David Naidoo naidoo@kgk.wombat.local 
 | Seat for David Naidoo
 Not seated
 Member
 External member
 | 
 John van Rensburg vanrensburg@sun.wombat.local 
 | Seat for John van Rensburg
 Not seated
 Member
 External member
 | 
 Thandi Zulu zulu@kgk.wombat.local 
 | Seat for Thandi Zulu
 Not seated
 Member
 External member
 | 
 1 seat: no chair, 1 member and 0 external members.
 External members are external examiners, who sit with the chair on the panel's appeal body. Only active committee members at the panel's institution who are not trainees are listed: cross-institution externals are not yet supported. 
 Cancel Create panel 
=== FRAME: | An Administrator: the institution missing (S4) | | Administrator · Scope Institution · a missing speciality reads “Choose a speciality.” | | Focus: the summary | 
 [shell] 
 Home 
 Decision panels 
 New panel 
# New panel
 A panel takes entrustment and progression decisions for the trainees it covers. Its name, scope, institution and speciality cannot change once it is created.
 The panel was not created. Everything you chose is kept below. Panel name: Give the panel a name. 
 Institution: Choose an institution. 
 Chair: Choose a chair. 
## About this panel
 Panel name * required 
 Give the panel a name.
 Scope
 Institution
 Speciality
 Institution
 Choose an institution
 Kgosi Kgari Teaching Hospital
 Choose an institution.
 Decides for
 General panel
 Neonatal team Clinical Competency Committee
 A panel sitting as a College committee decides the EPAs the College gives that committee (the neonatal CCC decides EPAs 4 and 5), and nothing else. Where no panel sits as it, the general panels decide them. 
## Who sits on it
 Chair
 Choose a chair
 Sarah Botha (botha@kgk.wombat.local)
 David Naidoo (naidoo@kgk.wombat.local)
 John van Rensburg (vanrensburg@sun.wombat.local)
 Thandi Zulu (zulu@kgk.wombat.local)
 Choose a chair.
 Only active committee members at the panel's institution who are not trainees are listed. 
 Seats on the panel: the active committee members at Kgosi Kgari Teaching Hospital who are not trainees, by surname
 Committee member | Seat | 
 Sarah Botha botha@kgk.wombat.local 
 | Seat for Sarah Botha
 Not seated
 Member
 External member
 | 
 David Naidoo naidoo@kgk.wombat.local 
 | Seat for David Naidoo
 Not seated
 Member
 External member
 | 
 John van Rensburg vanrensburg@sun.wombat.local 
 | Seat for John van Rensburg
 Not seated
 Member
 External member
 | 
 Thandi Zulu zulu@kgk.wombat.local 
 | Seat for Thandi Zulu
 Not seated
 Member
 External member
 | 
 0 seats: no chair, 0 members and 0 external members.
 External members are external examiners, who sit with the chair on the panel's appeal body. Only active committee members at the panel's institution who are not trainees are listed: cross-institution externals are not yet supported. 
 Cancel Create panel 
=== FRAME: | Someone who also holds Trainee (S1) | | A speciality admin who is also a registrar · the rule said first; the form is not drawn | 
 [shell] 
 Home 
 Decision panels 
 New panel 
# New panel
 A panel takes entrustment and progression decisions for the trainees it covers. Its name, scope, institution and speciality cannot change once it is created.
 You hold the Trainee role, so you cannot create or change a decision panel, including one that reviews you.
 Go to Decision panels 
=== FRAME: | Created: the page names the panel | | Step 2.22 · Nolwazi Mbatha | | Focus: the result | 
 [shell] 
 Home 
 Decision panels 
 Paed Annual Review Panel 
# Paed Annual Review Panel
 Speciality panel · Paediatrics · Kgosi Kgari Teaching Hospital
 Paed Annual Review Panel was created. Its reviews can now be scheduled on Committee reviews.
## About this panel
 Scope | Speciality: Paediatrics | 
 Institution | Kgosi Kgari Teaching Hospital | 
 Open reviews | None | 
 Its name, scope, institution and speciality cannot change once it is created. Its members can, even while its reviews are open.
## Who sits on it
 Chair
 Choose a chair
 Sarah Botha (botha@kgk.wombat.local)
 David Naidoo (naidoo@kgk.wombat.local)
 John van Rensburg (vanrensburg@sun.wombat.local)
 Thandi Zulu (zulu@kgk.wombat.local)
 Only active committee members at the panel's institution who are not trainees are listed. 
 Seats on the panel: the active committee members at Kgosi Kgari Teaching Hospital who are not trainees, by surname
 Committee member | Seat | 
 Sarah Botha botha@kgk.wombat.local 
 | Seat for Sarah Botha
 Not seated
 Member
 External member
 | 
 David Naidoo naidoo@kgk.wombat.local 
 | Seat for David Naidoo
 Not seated
 Member
 External member
 | 
 John van Rensburg vanrensburg@sun.wombat.local 
 | Seat for John van Rensburg
 Not seated
 Member
 External member
 | 
 Thandi Zulu zulu@kgk.wombat.local 
 | Chair · chosen above 
 | 
 3 seats: the chair, 1 member and 1 external member.
 External members are external examiners, who sit with the chair on the panel's appeal body. Only active committee members at the panel's institution who are not trainees are listed: cross-institution externals are not yet supported. 
 Save panel 
## Decides for
 Decides for
 General panel
 Neonatal team Clinical Competency Committee
 A panel sitting as a College committee decides the EPAs the College gives that committee (the neonatal CCC decides EPAs 4 and 5), and nothing else. Where no panel sits as it, the general panels decide them. It cannot change while a review before the panel is scheduled, in progress or awaiting ratification. 
 Save committee


######## BOARD R2-2-panel-1280-c.dc.html — A panel · 1280 · c
A panel · 1280 · c 
# A panel · 1280px (continued, part c) 
 Routes /committee/panels/new and /committee/panels/{PanelId} · the owner table's row for a panel unchanged; the Coordinator is refused.
 Components PageHeader, Breadcrumbs, FormField, FormActions, RefusalSummary, Alert, DetailCard and details-list, DataTable (Stack), .check-grid and .form-check radios, StatePanel, NotFound, AccessDenied. NEW .choice-group, .choice-row, .person-cell, .readonly-value, .form-section-title, .panel-section.
 DESIGN.md § Form system: the seat picker (one radio group a committee member) replaces the native multi-selects; § Page-level patterns: a panel's page titled by its name. Round 3 M6, M7, F5, S1, S3, S4, S6, S7.
=== FRAME: | Members updated; Decides for read-only | | Step 2.23 · Refilwe Mokoena | | Focus: the result | 
 [shell] 
 Home 
 Decision panels 
 Paed Annual Review Panel 
# Paed Annual Review Panel
 Speciality panel · Paediatrics · Kgosi Kgari Teaching Hospital
 Panel members updated. Thandi Zulu chairs, with 2 members and 1 external member.
## About this panel
 Scope | Speciality: Paediatrics | 
 Institution | Kgosi Kgari Teaching Hospital | 
 Open reviews | None | 
 Its name, scope, institution and speciality cannot change once it is created. Its members can, even while its reviews are open.
## Who sits on it
 Chair
 Choose a chair
 Sarah Botha (botha@kgk.wombat.local)
 David Naidoo (naidoo@kgk.wombat.local)
 John van Rensburg (vanrensburg@sun.wombat.local)
 Thandi Zulu (zulu@kgk.wombat.local)
 Only active committee members at the panel's institution who are not trainees are listed. 
 Seats on the panel: the active committee members at Kgosi Kgari Teaching Hospital who are not trainees, by surname
 Committee member | Seat | 
 Sarah Botha botha@kgk.wombat.local 
 | Seat for Sarah Botha
 Not seated
 Member
 External member
 | 
 David Naidoo naidoo@kgk.wombat.local 
 | Seat for David Naidoo
 Not seated
 Member
 External member
 | 
 John van Rensburg vanrensburg@sun.wombat.local 
 | Seat for John van Rensburg
 Not seated
 Member
 External member
 | 
 Thandi Zulu zulu@kgk.wombat.local 
 | Chair · chosen above 
 | 
 4 seats: the chair, 2 members and 1 external member.
 External members are external examiners, who sit with the chair on the panel's appeal body. Only active committee members at the panel's institution who are not trainees are listed: cross-institution externals are not yet supported. 
 Save panel 
## Decides for
 General panel
 Only an institutional administrator can change which College committee a panel sits as.
=== FRAME: | Decides for, locked by an open review | | Step 4.13 checks this (F5) · Nolwazi Mbatha · no select is offered while a review is open | 
 [shell] 
 Home 
 Decision panels 
 Paed Annual Review Panel 
# Paed Annual Review Panel
 Speciality panel · Paediatrics · Kgosi Kgari Teaching Hospital
## About this panel
 Scope | Speciality: Paediatrics | 
 Institution | Kgosi Kgari Teaching Hospital | 
 Open reviews | Review #1 (2026 S2, scheduled) and 4 more | 
 Its name, scope, institution and speciality cannot change once it is created. Its members can, even while its reviews are open.
## Who sits on it
 Chair
 Choose a chair
 Sarah Botha (botha@kgk.wombat.local)
 David Naidoo (naidoo@kgk.wombat.local)
 John van Rensburg (vanrensburg@sun.wombat.local)
 Thandi Zulu (zulu@kgk.wombat.local)
 Only active committee members at the panel's institution who are not trainees are listed. 
 Seats on the panel: the active committee members at Kgosi Kgari Teaching Hospital who are not trainees, by surname
 Committee member | Seat | 
 Sarah Botha botha@kgk.wombat.local 
 | Seat for Sarah Botha
 Not seated
 Member
 External member
 | 
 David Naidoo naidoo@kgk.wombat.local 
 | Seat for David Naidoo
 Not seated
 Member
 External member
 | 
 John van Rensburg vanrensburg@sun.wombat.local 
 | Seat for John van Rensburg
 Not seated
 Member
 External member
 | 
 Thandi Zulu zulu@kgk.wombat.local 
 | Chair · chosen above 
 | 
 4 seats: the chair, 2 members and 1 external member.
 External members are external examiners, who sit with the chair on the panel's appeal body. Only active committee members at the panel's institution who are not trainees are listed: cross-institution externals are not yet supported. 
 Save panel 
## Decides for
 General panel
 It cannot change now. Review #1 (2026 S2, scheduled) still sits before this panel. What a review decides, and its agenda, follow the College committee the panel sat as when it was scheduled, so the panel keeps what it sits as until its open reviews are ratified, or closed if formative.
 Open review #1 
=== FRAME: | Scratch state: a second tab. Decides for, refused by a race (C13, F5) | | No runbook step plays it: handler and bUnit tests cover it · the refusal beside the select, the select back on General panel; the summary links a short clause | | Focus: the section's summary | 
 [shell] 
 Home 
 Decision panels 
 Paed Annual Review Panel 
# Paed Annual Review Panel
 Speciality panel · Paediatrics · Kgosi Kgari Teaching Hospital
## About this panel
 Scope | Speciality: Paediatrics | 
 Institution | Kgosi Kgari Teaching Hospital | 
 Open reviews | None | 
 Its name, scope, institution and speciality cannot change once it is created. Its members can, even while its reviews are open.
## Who sits on it
 Chair
 Choose a chair
 Sarah Botha (botha@kgk.wombat.local)
 David Naidoo (naidoo@kgk.wombat.local)
 John van Rensburg (vanrensburg@sun.wombat.local)
 Thandi Zulu (zulu@kgk.wombat.local)
 Only active committee members at the panel's institution who are not trainees are listed. 
 Seats on the panel: the active committee members at Kgosi Kgari Teaching Hospital who are not trainees, by surname
 Committee member | Seat | 
 Sarah Botha botha@kgk.wombat.local 
 | Seat for Sarah Botha
 Not seated
 Member
 External member
 | 
 David Naidoo naidoo@kgk.wombat.local 
 | Seat for David Naidoo
 Not seated
 Member
 External member
 | 
 John van Rensburg vanrensburg@sun.wombat.local 
 | Seat for John van Rensburg
 Not seated
 Member
 External member
 | 
 Thandi Zulu zulu@kgk.wombat.local 
 | Chair · chosen above 
 | 
 4 seats: the chair, 2 members and 1 external member.
 External members are external examiners, who sit with the chair on the panel's appeal body. Only active committee members at the panel's institution who are not trainees are listed: cross-institution externals are not yet supported. 
 Save panel 
## Decides for
 Not saved. The panel is still a general panel. Decides for: review #1 now sits before this panel . What a review decides, and its agenda, follow the College committee the panel sat as when it was scheduled, so the panel keeps what it sits as until its open reviews are ratified, or closed if formative.
 Decides for
 General panel
 Neonatal team Clinical Competency Committee
 Not saved: review #1 (2026 S2, scheduled) now sits before this panel. The select shows what is saved.
 A panel sitting as a College committee decides the EPAs the College gives that committee (the neonatal CCC decides EPAs 4 and 5), and nothing else. Where no panel sits as it, the general panels decide them. It cannot change while a review before the panel is scheduled, in progress or awaiting ratification. 
 Save committee


######## BOARD R2-2-panel-1280-d.dc.html — A panel · 1280 · d
A panel · 1280 · d 
# A panel · 1280px (continued, part d) 
 Routes /committee/panels/new and /committee/panels/{PanelId} · the owner table's row for a panel unchanged; the Coordinator is refused.
 Components PageHeader, Breadcrumbs, FormField, FormActions, RefusalSummary, Alert, DetailCard and details-list, DataTable (Stack), .check-grid and .form-check radios, StatePanel, NotFound, AccessDenied. NEW .choice-group, .choice-row, .person-cell, .readonly-value, .form-section-title, .panel-section.
 DESIGN.md § Form system: the seat picker (one radio group a committee member) replaces the native multi-selects; § Page-level patterns: a panel's page titled by its name. Round 3 M6, M7, F5, S1, S3, S4, S6, S7.
=== FRAME: | A member who can no longer sit (M6, C12) | | Nolwazi Mbatha · Sarah Botha is in no control, neither Chair nor a seat; the warning names her; a live alert the first time it shows | 
 [shell] 
 Home 
 Decision panels 
 Paed Annual Review Panel 
# Paed Annual Review Panel
 Speciality panel · Paediatrics · Kgosi Kgari Teaching Hospital
## About this panel
 Scope | Speciality: Paediatrics | 
 Institution | Kgosi Kgari Teaching Hospital | 
 Open reviews | None | 
 Its name, scope, institution and speciality cannot change once it is created. Its members can, even while its reviews are open.
## Who sits on it
 Sarah Botha can no longer sit on this panel, so is not listed below: only an active committee member at its institution who is not a trainee can. Saving takes Sarah Botha off the panel.
 Chair
 Choose a chair
 David Naidoo (naidoo@kgk.wombat.local)
 John van Rensburg (vanrensburg@sun.wombat.local)
 Thandi Zulu (zulu@kgk.wombat.local)
 Only active committee members at the panel's institution who are not trainees are listed. 
 Seats on the panel: the active committee members at Kgosi Kgari Teaching Hospital who are not trainees, by surname
 Committee member | Seat | 
 David Naidoo naidoo@kgk.wombat.local 
 | Seat for David Naidoo
 Not seated
 Member
 External member
 | 
 John van Rensburg vanrensburg@sun.wombat.local 
 | Seat for John van Rensburg
 Not seated
 Member
 External member
 | 
 Thandi Zulu zulu@kgk.wombat.local 
 | Chair · chosen above 
 | 
 3 seats: the chair, 1 member and 1 external member.
 External members are external examiners, who sit with the chair on the panel's appeal body. Only active committee members at the panel's institution who are not trainees are listed: cross-institution externals are not yet supported. 
 Save panel 
## Decides for
 Decides for
 General panel
 Neonatal team Clinical Competency Committee
 A panel sitting as a College committee decides the EPAs the College gives that committee (the neonatal CCC decides EPAs 4 and 5), and nothing else. Where no panel sits as it, the general panels decide them. It cannot change while a review before the panel is scheduled, in progress or awaiting ratification. 
 Save committee 
=== FRAME: | After Save panel: the warning is gone (C12, T260) | | Nolwazi Mbatha · re-read from the saved panel | | Focus: the result | 
 [shell] 
 Home 
 Decision panels 
 Paed Annual Review Panel 
# Paed Annual Review Panel
 Speciality panel · Paediatrics · Kgosi Kgari Teaching Hospital
 Panel members updated. Sarah Botha is off the panel. Thandi Zulu chairs, with 1 member and 1 external member.
## About this panel
 Scope | Speciality: Paediatrics | 
 Institution | Kgosi Kgari Teaching Hospital | 
 Open reviews | None | 
 Its name, scope, institution and speciality cannot change once it is created. Its members can, even while its reviews are open.
## Who sits on it
 Chair
 Choose a chair
 David Naidoo (naidoo@kgk.wombat.local)
 John van Rensburg (vanrensburg@sun.wombat.local)
 Thandi Zulu (zulu@kgk.wombat.local)
 Only active committee members at the panel's institution who are not trainees are listed. 
 Seats on the panel: the active committee members at Kgosi Kgari Teaching Hospital who are not trainees, by surname
 Committee member | Seat | 
 David Naidoo naidoo@kgk.wombat.local 
 | Seat for David Naidoo
 Not seated
 Member
 External member
 | 
 John van Rensburg vanrensburg@sun.wombat.local 
 | Seat for John van Rensburg
 Not seated
 Member
 External member
 | 
 Thandi Zulu zulu@kgk.wombat.local 
 | Chair · chosen above 
 | 
 3 seats: the chair, 1 member and 1 external member.
 External members are external examiners, who sit with the chair on the panel's appeal body. Only active committee members at the panel's institution who are not trainees are listed: cross-institution externals are not yet supported. 
 Save panel 
## Decides for
 Decides for
 General panel
 Neonatal team Clinical Competency Committee
 A panel sitting as a College committee decides the EPAs the College gives that committee (the neonatal CCC decides EPAs 4 and 5), and nothing else. Where no panel sits as it, the general panels decide them. It cannot change while a review before the panel is scheduled, in progress or awaiting ratification. 
 Save committee 
=== FRAME: | Not found (M7) | | Nolwazi Mbatha · flow 01's Not found page, word for word; nothing lit | 
 [shell] 
# Page not found
 There is no page at this address.
 Check the address, or start again from Home.
 Go to Home 
=== FRAME: | Access denied: the Coordinator | | Step 2.32 · Pieter Smit | 
 [shell] 
# You cannot open this page
 Your role (Coordinator) does not open this page.
 If you need it for your work, ask your institution's Wombat administrator.
 Go to Home 
=== FRAME: | Loading | | C11 · the header reads “Decision panel” until the name is in | 
 [shell] 
 Home 
 Decision panels 
 Decision panel 
# Decision panel
 Loading the panel.
=== FRAME: | Load error | | C11, T338 | | Focus: the alert | 
 [shell] 
 Home 
 Decision panels 
 Decision panel 
# Decision panel
 Could not load this panel. Nothing has changed. Try again, or come back in a few minutes. Try again


######## BOARD R2-2-panel-390-a.dc.html — A panel · 390 · a
A panel · 390 · a 
# A panel · 390px
 Routes /committee/panels/new and /committee/panels/{PanelId} · the owner table's row for a panel unchanged; the Coordinator is refused.
 Components PageHeader, Breadcrumbs, FormField, FormActions, RefusalSummary, Alert, DetailCard and details-list, DataTable (Stack), .check-grid and .form-check radios, StatePanel, NotFound, AccessDenied. NEW .choice-group, .choice-row, .person-cell, .readonly-value, .form-section-title, .panel-section.
 DESIGN.md § Form system: the seat picker (one radio group a committee member) replaces the native multi-selects; § Page-level patterns: a panel's page titled by its name. Round 3 M6, M7, F5, S1, S3, S4, S6, S7.
=== FRAME: | New panel: the Institutional admin's | | Step 2.21 · Nolwazi Mbatha · addresses whole, on .local domains (S3) | | Focus: the h1, on arrival | 
 [shell] 
 Home 
 Decision panels 
 New panel 
# New panel
 A panel takes entrustment and progression decisions for the trainees it covers. Its name, scope, institution and speciality cannot change once it is created.
## About this panel
 Panel name * required 
 Scope
 Institution
 Speciality
 Speciality
 Paediatrics
 The panel runs at your institution. Only the specialities it has adopted a curriculum in are listed. 
 Decides for
 General panel
 Neonatal team Clinical Competency Committee
 A panel sitting as a College committee decides the EPAs the College gives that committee (the neonatal CCC decides EPAs 4 and 5), and nothing else. Where no panel sits as it, the general panels decide them. 
## Who sits on it
 Chair
 Choose a chair
 Sarah Botha (botha@kgk.wombat.local)
 David Naidoo (naidoo@kgk.wombat.local)
 John van Rensburg (vanrensburg@sun.wombat.local)
 Thandi Zulu (zulu@kgk.wombat.local)
 Only active committee members at the panel's institution who are not trainees are listed. 
 Seats on the panel: the active committee members at Kgosi Kgari Teaching Hospital who are not trainees, by surname
 Committee member | Seat | 
 Sarah Botha botha@kgk.wombat.local 
 | Seat for Sarah Botha
 Not seated
 Member
 External member
 | 
 David Naidoo naidoo@kgk.wombat.local 
 | Seat for David Naidoo
 Not seated
 Member
 External member
 | 
 John van Rensburg vanrensburg@sun.wombat.local 
 | Seat for John van Rensburg
 Not seated
 Member
 External member
 | 
 Thandi Zulu zulu@kgk.wombat.local 
 | Seat for Thandi Zulu
 Not seated
 Member
 External member
 | 
 0 seats: no chair, 0 members and 0 external members.
 External members are external examiners, who sit with the chair on the panel's appeal body. Only active committee members at the panel's institution who are not trainees are listed: cross-institution externals are not yet supported. 
 Cancel Create panel 
=== FRAME: | New panel: a sub-speciality admin's | | Step 2.25 · Kabelo Sithole · Scope as text; no Decides for | 
 [shell] 
 Home 
 Decision panels 
 New panel 
# New panel
 A panel takes entrustment and progression decisions for the trainees it covers. Its name, scope, institution and speciality cannot change once it is created.
## About this panel
 Panel name * required 
 Scope | Speciality | A sub-speciality administrator creates Speciality-scoped panels for the speciality of the sub-speciality. | Speciality
 Paediatrics
 The panel runs at your institution. Only the specialities it has adopted a curriculum in are listed. 
## Who sits on it
 Chair
 Choose a chair
 Sarah Botha (botha@kgk.wombat.local)
 David Naidoo (naidoo@kgk.wombat.local)
 John van Rensburg (vanrensburg@sun.wombat.local)
 Thandi Zulu (zulu@kgk.wombat.local)
 Only active committee members at the panel's institution who are not trainees are listed. 
 Seats on the panel: the active committee members at Kgosi Kgari Teaching Hospital who are not trainees, by surname
 Committee member | Seat | 
 Sarah Botha botha@kgk.wombat.local 
 | Seat for Sarah Botha
 Not seated
 Member
 External member
 | 
 David Naidoo naidoo@kgk.wombat.local 
 | Seat for David Naidoo
 Not seated
 Member
 External member
 | 
 John van Rensburg vanrensburg@sun.wombat.local 
 | Seat for John van Rensburg
 Not seated
 Member
 External member
 | 
 Thandi Zulu zulu@kgk.wombat.local 
 | Seat for Thandi Zulu
 Not seated
 Member
 External member
 | 
 0 seats: no chair, 0 members and 0 external members.
 External members are external examiners, who sit with the chair on the panel's appeal body. Only active committee members at the panel's institution who are not trainees are listed: cross-institution externals are not yet supported. 
 Cancel Create panel


######## BOARD R2-2-panel-390-b.dc.html — A panel · 390 · b
A panel · 390 · b 
# A panel · 390px (continued, part b) 
 Routes /committee/panels/new and /committee/panels/{PanelId} · the owner table's row for a panel unchanged; the Coordinator is refused.
 Components PageHeader, Breadcrumbs, FormField, FormActions, RefusalSummary, Alert, DetailCard and details-list, DataTable (Stack), .check-grid and .form-check radios, StatePanel, NotFound, AccessDenied. NEW .choice-group, .choice-row, .person-cell, .readonly-value, .form-section-title, .panel-section.
 DESIGN.md § Form system: the seat picker (one radio group a committee member) replaces the native multi-selects; § Page-level patterns: a panel's page titled by its name. Round 3 M6, M7, F5, S1, S3, S4, S6, S7.
=== FRAME: | The chair chosen | | Step 2.21 · Nolwazi Mbatha | | Focus: stays on Chair; the note is announced | 
 [shell] 
 Home 
 Decision panels 
 New panel 
# New panel
 A panel takes entrustment and progression decisions for the trainees it covers. Its name, scope, institution and speciality cannot change once it is created.
## About this panel
 Panel name * required 
 Scope
 Institution
 Speciality
 Speciality
 Paediatrics
 The panel runs at your institution. Only the specialities it has adopted a curriculum in are listed. 
 Decides for
 General panel
 Neonatal team Clinical Competency Committee
 A panel sitting as a College committee decides the EPAs the College gives that committee (the neonatal CCC decides EPAs 4 and 5), and nothing else. Where no panel sits as it, the general panels decide them. 
## Who sits on it
 Chair
 Choose a chair
 Sarah Botha (botha@kgk.wombat.local)
 David Naidoo (naidoo@kgk.wombat.local)
 John van Rensburg (vanrensburg@sun.wombat.local)
 Thandi Zulu (zulu@kgk.wombat.local)
 Thandi Zulu is the chair now, so is no longer seated as a member.
 Only active committee members at the panel's institution who are not trainees are listed. 
 Seats on the panel: the active committee members at Kgosi Kgari Teaching Hospital who are not trainees, by surname
 Committee member | Seat | 
 Sarah Botha botha@kgk.wombat.local 
 | Seat for Sarah Botha
 Not seated
 Member
 External member
 | 
 David Naidoo naidoo@kgk.wombat.local 
 | Seat for David Naidoo
 Not seated
 Member
 External member
 | 
 John van Rensburg vanrensburg@sun.wombat.local 
 | Seat for John van Rensburg
 Not seated
 Member
 External member
 | 
 Thandi Zulu zulu@kgk.wombat.local 
 | Chair · chosen above 
 | 
 2 seats: the chair, 1 member and 0 external members.
 External members are external examiners, who sit with the chair on the panel's appeal body. Only active committee members at the panel's institution who are not trainees are listed: cross-institution externals are not yet supported. 
 Cancel Create panel 
=== FRAME: | A panel of one, refused | | Step 2.21 · Nolwazi Mbatha · the group named by its message (aria-describedby); the summary's link focuses the first radio; a stripe in forced colours too; no padding shift (S6) | | Focus: the summary | 
 [shell] 
 Home 
 Decision panels 
 New panel 
# New panel
 A panel takes entrustment and progression decisions for the trainees it covers. Its name, scope, institution and speciality cannot change once it is created.
 The panel was not created. Everything you chose is kept below. Seats: A panel needs at least two members: the chair and at least one other, so that no decision is one person's. 
## About this panel
 Panel name * required 
 Scope
 Institution
 Speciality
 Speciality
 Paediatrics
 The panel runs at your institution. Only the specialities it has adopted a curriculum in are listed. 
 Decides for
 General panel
 Neonatal team Clinical Competency Committee
 A panel sitting as a College committee decides the EPAs the College gives that committee (the neonatal CCC decides EPAs 4 and 5), and nothing else. Where no panel sits as it, the general panels decide them. 
## Who sits on it
 Chair
 Choose a chair
 Sarah Botha (botha@kgk.wombat.local)
 David Naidoo (naidoo@kgk.wombat.local)
 John van Rensburg (vanrensburg@sun.wombat.local)
 Thandi Zulu (zulu@kgk.wombat.local)
 Only active committee members at the panel's institution who are not trainees are listed. 
 A panel needs at least two members: the chair and at least one other, so that no decision is one person's.
 Seats on the panel: the active committee members at Kgosi Kgari Teaching Hospital who are not trainees, by surname
 Committee member | Seat | 
 Sarah Botha botha@kgk.wombat.local 
 | Seat for Sarah Botha
 Not seated
 Member
 External member
 | 
 David Naidoo naidoo@kgk.wombat.local 
 | Seat for David Naidoo
 Not seated
 Member
 External member
 | 
 John van Rensburg vanrensburg@sun.wombat.local 
 | Seat for John van Rensburg
 Not seated
 Member
 External member
 | 
 Thandi Zulu zulu@kgk.wombat.local 
 | Chair · chosen above 
 | 
 1 seat: the chair, 0 members and 0 external members.
 External members are external examiners, who sit with the chair on the panel's appeal body. Only active committee members at the panel's institution who are not trainees are listed: cross-institution externals are not yet supported. 
 Cancel Create panel


######## BOARD R2-2-panel-390-c.dc.html — A panel · 390 · c
A panel · 390 · c 
# A panel · 390px (continued, part c) 
 Routes /committee/panels/new and /committee/panels/{PanelId} · the owner table's row for a panel unchanged; the Coordinator is refused.
 Components PageHeader, Breadcrumbs, FormField, FormActions, RefusalSummary, Alert, DetailCard and details-list, DataTable (Stack), .check-grid and .form-check radios, StatePanel, NotFound, AccessDenied. NEW .choice-group, .choice-row, .person-cell, .readonly-value, .form-section-title, .panel-section.
 DESIGN.md § Form system: the seat picker (one radio group a committee member) replaces the native multi-selects; § Page-level patterns: a panel's page titled by its name. Round 3 M6, M7, F5, S1, S3, S4, S6, S7.
=== FRAME: | Name and chair missing (T324) | | Nolwazi Mbatha | | Focus: the summary | 
 [shell] 
 Home 
 Decision panels 
 New panel 
# New panel
 A panel takes entrustment and progression decisions for the trainees it covers. Its name, scope, institution and speciality cannot change once it is created.
 The panel was not created. Everything you chose is kept below. Panel name: Give the panel a name. 
 Chair: Choose a chair. 
## About this panel
 Panel name * required 
 Give the panel a name.
 Scope
 Institution
 Speciality
 Speciality
 Paediatrics
 The panel runs at your institution. Only the specialities it has adopted a curriculum in are listed. 
 Decides for
 General panel
 Neonatal team Clinical Competency Committee
 A panel sitting as a College committee decides the EPAs the College gives that committee (the neonatal CCC decides EPAs 4 and 5), and nothing else. Where no panel sits as it, the general panels decide them. 
## Who sits on it
 Chair
 Choose a chair
 Sarah Botha (botha@kgk.wombat.local)
 David Naidoo (naidoo@kgk.wombat.local)
 John van Rensburg (vanrensburg@sun.wombat.local)
 Thandi Zulu (zulu@kgk.wombat.local)
 Choose a chair.
 Only active committee members at the panel's institution who are not trainees are listed. 
 Seats on the panel: the active committee members at Kgosi Kgari Teaching Hospital who are not trainees, by surname
 Committee member | Seat | 
 Sarah Botha botha@kgk.wombat.local 
 | Seat for Sarah Botha
 Not seated
 Member
 External member
 | 
 David Naidoo naidoo@kgk.wombat.local 
 | Seat for David Naidoo
 Not seated
 Member
 External member
 | 
 John van Rensburg vanrensburg@sun.wombat.local 
 | Seat for John van Rensburg
 Not seated
 Member
 External member
 | 
 Thandi Zulu zulu@kgk.wombat.local 
 | Seat for Thandi Zulu
 Not seated
 Member
 External member
 | 
 1 seat: no chair, 1 member and 0 external members.
 External members are external examiners, who sit with the chair on the panel's appeal body. Only active committee members at the panel's institution who are not trainees are listed: cross-institution externals are not yet supported. 
 Cancel Create panel 
=== FRAME: | An Administrator: the institution missing (S4) | | Administrator · Scope Institution · a missing speciality reads “Choose a speciality.” | | Focus: the summary | 
 [shell] 
 Home 
 Decision panels 
 New panel 
# New panel
 A panel takes entrustment and progression decisions for the trainees it covers. Its name, scope, institution and speciality cannot change once it is created.
 The panel was not created. Everything you chose is kept below. Panel name: Give the panel a name. 
 Institution: Choose an institution. 
 Chair: Choose a chair. 
## About this panel
 Panel name * required 
 Give the panel a name.
 Scope
 Institution
 Speciality
 Institution
 Choose an institution
 Kgosi Kgari Teaching Hospital
 Choose an institution.
 Decides for
 General panel
 Neonatal team Clinical Competency Committee
 A panel sitting as a College committee decides the EPAs the College gives that committee (the neonatal CCC decides EPAs 4 and 5), and nothing else. Where no panel sits as it, the general panels decide them. 
## Who sits on it
 Chair
 Choose a chair
 Sarah Botha (botha@kgk.wombat.local)
 David Naidoo (naidoo@kgk.wombat.local)
 John van Rensburg (vanrensburg@sun.wombat.local)
 Thandi Zulu (zulu@kgk.wombat.local)
 Choose a chair.
 Only active committee members at the panel's institution who are not trainees are listed. 
 Seats on the panel: the active committee members at Kgosi Kgari Teaching Hospital who are not trainees, by surname
 Committee member | Seat | 
 Sarah Botha botha@kgk.wombat.local 
 | Seat for Sarah Botha
 Not seated
 Member
 External member
 | 
 David Naidoo naidoo@kgk.wombat.local 
 | Seat for David Naidoo
 Not seated
 Member
 External member
 | 
 John van Rensburg vanrensburg@sun.wombat.local 
 | Seat for John van Rensburg
 Not seated
 Member
 External member
 | 
 Thandi Zulu zulu@kgk.wombat.local 
 | Seat for Thandi Zulu
 Not seated
 Member
 External member
 | 
 0 seats: no chair, 0 members and 0 external members.
 External members are external examiners, who sit with the chair on the panel's appeal body. Only active committee members at the panel's institution who are not trainees are listed: cross-institution externals are not yet supported. 
 Cancel Create panel 
=== FRAME: | Someone who also holds Trainee (S1) | | A speciality admin who is also a registrar · the rule said first; the form is not drawn | 
 [shell] 
 Home 
 Decision panels 
 New panel 
# New panel
 A panel takes entrustment and progression decisions for the trainees it covers. Its name, scope, institution and speciality cannot change once it is created.
 You hold the Trainee role, so you cannot create or change a decision panel, including one that reviews you.
 Go to Decision panels


######## BOARD R2-2-panel-390-d.dc.html — A panel · 390 · d
A panel · 390 · d 
# A panel · 390px (continued, part d) 
 Routes /committee/panels/new and /committee/panels/{PanelId} · the owner table's row for a panel unchanged; the Coordinator is refused.
 Components PageHeader, Breadcrumbs, FormField, FormActions, RefusalSummary, Alert, DetailCard and details-list, DataTable (Stack), .check-grid and .form-check radios, StatePanel, NotFound, AccessDenied. NEW .choice-group, .choice-row, .person-cell, .readonly-value, .form-section-title, .panel-section.
 DESIGN.md § Form system: the seat picker (one radio group a committee member) replaces the native multi-selects; § Page-level patterns: a panel's page titled by its name. Round 3 M6, M7, F5, S1, S3, S4, S6, S7.
=== FRAME: | Created: the page names the panel | | Step 2.22 · Nolwazi Mbatha | | Focus: the result | 
 [shell] 
 Home 
 Decision panels 
 Paed Annual Review Panel 
# Paed Annual Review Panel
 Speciality panel · Paediatrics · Kgosi Kgari Teaching Hospital
 Paed Annual Review Panel was created. Its reviews can now be scheduled on Committee reviews.
## About this panel
 Scope | Speciality: Paediatrics | 
 Institution | Kgosi Kgari Teaching Hospital | 
 Open reviews | None | 
 Its name, scope, institution and speciality cannot change once it is created. Its members can, even while its reviews are open.
## Who sits on it
 Chair
 Choose a chair
 Sarah Botha (botha@kgk.wombat.local)
 David Naidoo (naidoo@kgk.wombat.local)
 John van Rensburg (vanrensburg@sun.wombat.local)
 Thandi Zulu (zulu@kgk.wombat.local)
 Only active committee members at the panel's institution who are not trainees are listed. 
 Seats on the panel: the active committee members at Kgosi Kgari Teaching Hospital who are not trainees, by surname
 Committee member | Seat | 
 Sarah Botha botha@kgk.wombat.local 
 | Seat for Sarah Botha
 Not seated
 Member
 External member
 | 
 David Naidoo naidoo@kgk.wombat.local 
 | Seat for David Naidoo
 Not seated
 Member
 External member
 | 
 John van Rensburg vanrensburg@sun.wombat.local 
 | Seat for John van Rensburg
 Not seated
 Member
 External member
 | 
 Thandi Zulu zulu@kgk.wombat.local 
 | Chair · chosen above 
 | 
 3 seats: the chair, 1 member and 1 external member.
 External members are external examiners, who sit with the chair on the panel's appeal body. Only active committee members at the panel's institution who are not trainees are listed: cross-institution externals are not yet supported. 
 Save panel 
## Decides for
 Decides for
 General panel
 Neonatal team Clinical Competency Committee
 A panel sitting as a College committee decides the EPAs the College gives that committee (the neonatal CCC decides EPAs 4 and 5), and nothing else. Where no panel sits as it, the general panels decide them. It cannot change while a review before the panel is scheduled, in progress or awaiting ratification. 
 Save committee 
=== FRAME: | Members updated; Decides for read-only | | Step 2.23 · Refilwe Mokoena | | Focus: the result | 
 [shell] 
 Home 
 Decision panels 
 Paed Annual Review Panel 
# Paed Annual Review Panel
 Speciality panel · Paediatrics · Kgosi Kgari Teaching Hospital
 Panel members updated. Thandi Zulu chairs, with 2 members and 1 external member.
## About this panel
 Scope | Speciality: Paediatrics | 
 Institution | Kgosi Kgari Teaching Hospital | 
 Open reviews | None | 
 Its name, scope, institution and speciality cannot change once it is created. Its members can, even while its reviews are open.
## Who sits on it
 Chair
 Choose a chair
 Sarah Botha (botha@kgk.wombat.local)
 David Naidoo (naidoo@kgk.wombat.local)
 John van Rensburg (vanrensburg@sun.wombat.local)
 Thandi Zulu (zulu@kgk.wombat.local)
 Only active committee members at the panel's institution who are not trainees are listed. 
 Seats on the panel: the active committee members at Kgosi Kgari Teaching Hospital who are not trainees, by surname
 Committee member | Seat | 
 Sarah Botha botha@kgk.wombat.local 
 | Seat for Sarah Botha
 Not seated
 Member
 External member
 | 
 David Naidoo naidoo@kgk.wombat.local 
 | Seat for David Naidoo
 Not seated
 Member
 External member
 | 
 John van Rensburg vanrensburg@sun.wombat.local 
 | Seat for John van Rensburg
 Not seated
 Member
 External member
 | 
 Thandi Zulu zulu@kgk.wombat.local 
 | Chair · chosen above 
 | 
 4 seats: the chair, 2 members and 1 external member.
 External members are external examiners, who sit with the chair on the panel's appeal body. Only active committee members at the panel's institution who are not trainees are listed: cross-institution externals are not yet supported. 
 Save panel 
## Decides for
 General panel
 Only an institutional administrator can change which College committee a panel sits as.


######## BOARD R2-2-panel-390-e.dc.html — A panel · 390 · e
A panel · 390 · e 
# A panel · 390px (continued, part e) 
 Routes /committee/panels/new and /committee/panels/{PanelId} · the owner table's row for a panel unchanged; the Coordinator is refused.
 Components PageHeader, Breadcrumbs, FormField, FormActions, RefusalSummary, Alert, DetailCard and details-list, DataTable (Stack), .check-grid and .form-check radios, StatePanel, NotFound, AccessDenied. NEW .choice-group, .choice-row, .person-cell, .readonly-value, .form-section-title, .panel-section.
 DESIGN.md § Form system: the seat picker (one radio group a committee member) replaces the native multi-selects; § Page-level patterns: a panel's page titled by its name. Round 3 M6, M7, F5, S1, S3, S4, S6, S7.
=== FRAME: | Decides for, locked by an open review | | Step 4.13 checks this (F5) · Nolwazi Mbatha · no select is offered while a review is open | 
 [shell] 
 Home 
 Decision panels 
 Paed Annual Review Panel 
# Paed Annual Review Panel
 Speciality panel · Paediatrics · Kgosi Kgari Teaching Hospital
## About this panel
 Scope | Speciality: Paediatrics | 
 Institution | Kgosi Kgari Teaching Hospital | 
 Open reviews | Review #1 (2026 S2, scheduled) and 4 more | 
 Its name, scope, institution and speciality cannot change once it is created. Its members can, even while its reviews are open.
## Who sits on it
 Chair
 Choose a chair
 Sarah Botha (botha@kgk.wombat.local)
 David Naidoo (naidoo@kgk.wombat.local)
 John van Rensburg (vanrensburg@sun.wombat.local)
 Thandi Zulu (zulu@kgk.wombat.local)
 Only active committee members at the panel's institution who are not trainees are listed. 
 Seats on the panel: the active committee members at Kgosi Kgari Teaching Hospital who are not trainees, by surname
 Committee member | Seat | 
 Sarah Botha botha@kgk.wombat.local 
 | Seat for Sarah Botha
 Not seated
 Member
 External member
 | 
 David Naidoo naidoo@kgk.wombat.local 
 | Seat for David Naidoo
 Not seated
 Member
 External member
 | 
 John van Rensburg vanrensburg@sun.wombat.local 
 | Seat for John van Rensburg
 Not seated
 Member
 External member
 | 
 Thandi Zulu zulu@kgk.wombat.local 
 | Chair · chosen above 
 | 
 4 seats: the chair, 2 members and 1 external member.
 External members are external examiners, who sit with the chair on the panel's appeal body. Only active committee members at the panel's institution who are not trainees are listed: cross-institution externals are not yet supported. 
 Save panel 
## Decides for
 General panel
 It cannot change now. Review #1 (2026 S2, scheduled) still sits before this panel. What a review decides, and its agenda, follow the College committee the panel sat as when it was scheduled, so the panel keeps what it sits as until its open reviews are ratified, or closed if formative.
 Open review #1 
=== FRAME: | Scratch state: a second tab. Decides for, refused by a race (C13, F5) | | No runbook step plays it: handler and bUnit tests cover it · the refusal beside the select, the select back on General panel; the summary links a short clause | | Focus: the section's summary | 
 [shell] 
 Home 
 Decision panels 
 Paed Annual Review Panel 
# Paed Annual Review Panel
 Speciality panel · Paediatrics · Kgosi Kgari Teaching Hospital
## About this panel
 Scope | Speciality: Paediatrics | 
 Institution | Kgosi Kgari Teaching Hospital | 
 Open reviews | None | 
 Its name, scope, institution and speciality cannot change once it is created. Its members can, even while its reviews are open.
## Who sits on it
 Chair
 Choose a chair
 Sarah Botha (botha@kgk.wombat.local)
 David Naidoo (naidoo@kgk.wombat.local)
 John van Rensburg (vanrensburg@sun.wombat.local)
 Thandi Zulu (zulu@kgk.wombat.local)
 Only active committee members at the panel's institution who are not trainees are listed. 
 Seats on the panel: the active committee members at Kgosi Kgari Teaching Hospital who are not trainees, by surname
 Committee member | Seat | 
 Sarah Botha botha@kgk.wombat.local 
 | Seat for Sarah Botha
 Not seated
 Member
 External member
 | 
 David Naidoo naidoo@kgk.wombat.local 
 | Seat for David Naidoo
 Not seated
 Member
 External member
 | 
 John van Rensburg vanrensburg@sun.wombat.local 
 | Seat for John van Rensburg
 Not seated
 Member
 External member
 | 
 Thandi Zulu zulu@kgk.wombat.local 
 | Chair · chosen above 
 | 
 4 seats: the chair, 2 members and 1 external member.
 External members are external examiners, who sit with the chair on the panel's appeal body. Only active committee members at the panel's institution who are not trainees are listed: cross-institution externals are not yet supported. 
 Save panel 
## Decides for
 Not saved. The panel is still a general panel. Decides for: review #1 now sits before this panel . What a review decides, and its agenda, follow the College committee the panel sat as when it was scheduled, so the panel keeps what it sits as until its open reviews are ratified, or closed if formative.
 Decides for
 General panel
 Neonatal team Clinical Competency Committee
 Not saved: review #1 (2026 S2, scheduled) now sits before this panel. The select shows what is saved.
 A panel sitting as a College committee decides the EPAs the College gives that committee (the neonatal CCC decides EPAs 4 and 5), and nothing else. Where no panel sits as it, the general panels decide them. It cannot change while a review before the panel is scheduled, in progress or awaiting ratification. 
 Save committee


######## BOARD R2-2-panel-390-f.dc.html — A panel · 390 · f
A panel · 390 · f 
# A panel · 390px (continued, part f) 
 Routes /committee/panels/new and /committee/panels/{PanelId} · the owner table's row for a panel unchanged; the Coordinator is refused.
 Components PageHeader, Breadcrumbs, FormField, FormActions, RefusalSummary, Alert, DetailCard and details-list, DataTable (Stack), .check-grid and .form-check radios, StatePanel, NotFound, AccessDenied. NEW .choice-group, .choice-row, .person-cell, .readonly-value, .form-section-title, .panel-section.
 DESIGN.md § Form system: the seat picker (one radio group a committee member) replaces the native multi-selects; § Page-level patterns: a panel's page titled by its name. Round 3 M6, M7, F5, S1, S3, S4, S6, S7.
=== FRAME: | A member who can no longer sit (M6, C12) | | Nolwazi Mbatha · Sarah Botha is in no control, neither Chair nor a seat; the warning names her; a live alert the first time it shows | 
 [shell] 
 Home 
 Decision panels 
 Paed Annual Review Panel 
# Paed Annual Review Panel
 Speciality panel · Paediatrics · Kgosi Kgari Teaching Hospital
## About this panel
 Scope | Speciality: Paediatrics | 
 Institution | Kgosi Kgari Teaching Hospital | 
 Open reviews | None | 
 Its name, scope, institution and speciality cannot change once it is created. Its members can, even while its reviews are open.
## Who sits on it
 Sarah Botha can no longer sit on this panel, so is not listed below: only an active committee member at its institution who is not a trainee can. Saving takes Sarah Botha off the panel.
 Chair
 Choose a chair
 David Naidoo (naidoo@kgk.wombat.local)
 John van Rensburg (vanrensburg@sun.wombat.local)
 Thandi Zulu (zulu@kgk.wombat.local)
 Only active committee members at the panel's institution who are not trainees are listed. 
 Seats on the panel: the active committee members at Kgosi Kgari Teaching Hospital who are not trainees, by surname
 Committee member | Seat | 
 David Naidoo naidoo@kgk.wombat.local 
 | Seat for David Naidoo
 Not seated
 Member
 External member
 | 
 John van Rensburg vanrensburg@sun.wombat.local 
 | Seat for John van Rensburg
 Not seated
 Member
 External member
 | 
 Thandi Zulu zulu@kgk.wombat.local 
 | Chair · chosen above 
 | 
 3 seats: the chair, 1 member and 1 external member.
 External members are external examiners, who sit with the chair on the panel's appeal body. Only active committee members at the panel's institution who are not trainees are listed: cross-institution externals are not yet supported. 
 Save panel 
## Decides for
 Decides for
 General panel
 Neonatal team Clinical Competency Committee
 A panel sitting as a College committee decides the EPAs the College gives that committee (the neonatal CCC decides EPAs 4 and 5), and nothing else. Where no panel sits as it, the general panels decide them. It cannot change while a review before the panel is scheduled, in progress or awaiting ratification. 
 Save committee 
=== FRAME: | After Save panel: the warning is gone (C12, T260) | | Nolwazi Mbatha · re-read from the saved panel | | Focus: the result | 
 [shell] 
 Home 
 Decision panels 
 Paed Annual Review Panel 
# Paed Annual Review Panel
 Speciality panel · Paediatrics · Kgosi Kgari Teaching Hospital
 Panel members updated. Sarah Botha is off the panel. Thandi Zulu chairs, with 1 member and 1 external member.
## About this panel
 Scope | Speciality: Paediatrics | 
 Institution | Kgosi Kgari Teaching Hospital | 
 Open reviews | None | 
 Its name, scope, institution and speciality cannot change once it is created. Its members can, even while its reviews are open.
## Who sits on it
 Chair
 Choose a chair
 David Naidoo (naidoo@kgk.wombat.local)
 John van Rensburg (vanrensburg@sun.wombat.local)
 Thandi Zulu (zulu@kgk.wombat.local)
 Only active committee members at the panel's institution who are not trainees are listed. 
 Seats on the panel: the active committee members at Kgosi Kgari Teaching Hospital who are not trainees, by surname
 Committee member | Seat | 
 David Naidoo naidoo@kgk.wombat.local 
 | Seat for David Naidoo
 Not seated
 Member
 External member
 | 
 John van Rensburg vanrensburg@sun.wombat.local 
 | Seat for John van Rensburg
 Not seated
 Member
 External member
 | 
 Thandi Zulu zulu@kgk.wombat.local 
 | Chair · chosen above 
 | 
 3 seats: the chair, 1 member and 1 external member.
 External members are external examiners, who sit with the chair on the panel's appeal body. Only active committee members at the panel's institution who are not trainees are listed: cross-institution externals are not yet supported. 
 Save panel 
## Decides for
 Decides for
 General panel
 Neonatal team Clinical Competency Committee
 A panel sitting as a College committee decides the EPAs the College gives that committee (the neonatal CCC decides EPAs 4 and 5), and nothing else. Where no panel sits as it, the general panels decide them. It cannot change while a review before the panel is scheduled, in progress or awaiting ratification. 
 Save committee 
=== FRAME: | Not found (M7) | | Nolwazi Mbatha · flow 01's Not found page, word for word; nothing lit | 
 [shell] 
# Page not found
 There is no page at this address.
 Check the address, or start again from Home.
 Go to Home 
=== FRAME: | Access denied: the Coordinator | | Step 2.32 · Pieter Smit | 
 [shell] 
# You cannot open this page
 Your role (Coordinator) does not open this page.
 If you need it for your work, ask your institution's Wombat administrator.
 Go to Home 
=== FRAME: | Loading | | C11 · the header reads “Decision panel” until the name is in | 
 [shell] 
 Home 
 Decision panels 
 Decision panel 
# Decision panel
 Loading the panel.


######## BOARD R2-2-panel-390-g.dc.html — A panel · 390 · g
A panel · 390 · g 
# A panel · 390px (continued, part g) 
 Routes /committee/panels/new and /committee/panels/{PanelId} · the owner table's row for a panel unchanged; the Coordinator is refused.
 Components PageHeader, Breadcrumbs, FormField, FormActions, RefusalSummary, Alert, DetailCard and details-list, DataTable (Stack), .check-grid and .form-check radios, StatePanel, NotFound, AccessDenied. NEW .choice-group, .choice-row, .person-cell, .readonly-value, .form-section-title, .panel-section.
 DESIGN.md § Form system: the seat picker (one radio group a committee member) replaces the native multi-selects; § Page-level patterns: a panel's page titled by its name. Round 3 M6, M7, F5, S1, S3, S4, S6, S7.
=== FRAME: | Load error | | C11, T338 | | Focus: the alert | 
 [shell] 
 Home 
 Decision panels 
 Decision panel 
# Decision panel
 Could not load this panel. Nothing has changed. Try again, or come back in a few minutes. Try again


######## BOARD R2-3-decisions-due-1280-a.dc.html — Decisions due · 1280 · a
Decisions due · 1280 · a 
# Decisions due · 1280px
 Route /committee/decisions-due · the schedulers' list; a Committee member is refused.
 Components PageHeader, FilterBar (Show, Clear filters), DataTable (stack-wide), PagerControls (labelled), the .rung-legend fold with .fold-show / .fold-hide, StatePanel, Badge, AccessDenied. NEW the Trainee filter, .trainee-line (C2), .status-cell, .list-section-page, .list-section-count.
 DESIGN.md Decisions due's section; § Pager; § Table system. Round 3 the cadence (M1, M2), By EPA from the board's rows (M3), F1, F2, F6, M8, S1, S4, S5, S7.
=== FRAME: | An Administrator choosing an institution | | Administrator · no institution yet: no By EPA, no list | 
 [shell] 
# Decisions due
 What the committee must decide, per registrar and per EPA, and where each decision stands.
 An administrator belongs to no institution, so the decisions due are shown for the one chosen above.
 Institution
 Select institution
 Kgosi Kgari Teaching Hospital
 Period
 2025 S1 · 1 Jan to 30 Jun 2025
 2025 S2 · 1 Jul to 31 Dec 2025
 2026 S1 · 1 Jan to 30 Jun 2026
 2026 S2 · 1 Jul to 31 Dec 2026
 2027 S1 · 1 Jan to 30 Jun 2027
 2027 S2 · 1 Jul to 31 Dec 2027
 The College's semester 2 ends in November; its window runs to 31 December, so December's work counts. 
 Trainee
 Every trainee
 Anele Dlamini
 Pieter du Plessis
 Nomsa Mahlangu
 Lerato Molefe
 Sipho Ndlovu
 Status
 Outstanding: not yet decided
 Every status
 Not scheduled
 Scheduled
 Decided
 Not decided
 Revoked: re-decide
 Due by year end
 Partial period
 As opportunity allows
 Missed
 EPA
 Every EPA
 PAED-001 — Providing paediatric emergency care to children
 PAED-002 — [PAED-002 title, as seeded]
 PAED-003 — Providing intensive care to children
 PAED-004 — [PAED-004 title, as seeded]
 PAED-005 — [PAED-005 title, as seeded]
 PAED-006 — [PAED-006 title, as seeded]
 PAED-007 — [PAED-007 title, as seeded]
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
 PAED-009 — [PAED-009 title, as seeded]
 PAED-010 — [PAED-010 title, as seeded]
 PAED-011 — [PAED-011 title, as seeded]
 PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
 PAED-013 — [PAED-013 title, as seeded]
 PAED-014 — [PAED-014 title, as seeded]
 PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
 Show 
 Choose an institution
 Choose an institution above, then Show, to read what its committees must decide.
=== FRAME: | Outstanding, every trainee (the default) | | Step 4.1 · Pieter Smit, Coordinator · page 1 of 4, 20 rows: semester rows read “… 2026 S2 has not ended.”, annual rows “… 2026 has not ended.” (M2), optional rows As opportunity allows; no Missed row | 
 [shell] 
# Decisions due
 What the committee must decide, per registrar and per EPA, and where each decision stands.
 The EPAs due for an entrustment decision in 2026 S2 for the 5 trainees you oversee at Kgosi Kgari Teaching Hospital, by each EPA's decision cadence: every semester, or once a year. Missed is read today: the EPA's window has ended with nothing decided or on an open review's agenda.
 Period
 2025 S1 · 1 Jan to 30 Jun 2025
 2025 S2 · 1 Jul to 31 Dec 2025
 2026 S1 · 1 Jan to 30 Jun 2026
 2026 S2 · 1 Jul to 31 Dec 2026
 2027 S1 · 1 Jan to 30 Jun 2027
 2027 S2 · 1 Jul to 31 Dec 2027
 The College's semester 2 ends in November; its window runs to 31 December, so December's work counts. 
 Trainee
 Every trainee
 Anele Dlamini
 Pieter du Plessis
 Nomsa Mahlangu
 Lerato Molefe
 Sipho Ndlovu
 Status
 Outstanding: not yet decided
 Every status
 Not scheduled
 Scheduled
 Decided
 Not decided
 Revoked: re-decide
 Due by year end
 Partial period
 As opportunity allows
 Missed
 EPA
 Every EPA
 PAED-001 — Providing paediatric emergency care to children
 PAED-002 — [PAED-002 title, as seeded]
 PAED-003 — Providing intensive care to children
 PAED-004 — [PAED-004 title, as seeded]
 PAED-005 — [PAED-005 title, as seeded]
 PAED-006 — [PAED-006 title, as seeded]
 PAED-007 — [PAED-007 title, as seeded]
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
 PAED-009 — [PAED-009 title, as seeded]
 PAED-010 — [PAED-010 title, as seeded]
 PAED-011 — [PAED-011 title, as seeded]
 PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
 PAED-013 — [PAED-013 title, as seeded]
 PAED-014 — [PAED-014 title, as seeded]
 PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
 Show 
## By EPA
 The filters above do not narrow these counts.
 Counts for the 15 EPAs due in 2026 S2 Show Hide 
 How many trainees stand where on each EPA due in 2026 S2. To schedule: not scheduled, due by year end, or revoked. Optional: a partial period, or decided as opportunity allows.
 EPA | Due | Decided | Scheduled | Not decided | To schedule | Missed | Optional | 
 PAED-001 — Providing paediatric emergency care to children | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-002 — [PAED-002 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-003 — Providing intensive care to children | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-004 — [PAED-004 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-005 — [PAED-005 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-006 — [PAED-006 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-007 — [PAED-007 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-009 — [PAED-009 title, as seeded] | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-010 — [PAED-010 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-011 — [PAED-011 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-013 — [PAED-013 title, as seeded] | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-014 — [PAED-014 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
## Decisions , page 1 of 4 
 75 of 75 decisions due in 2026 S2 shown.
 Decisions due in 2026 S2, by trainee then EPA
 EPA | Trainee | Window | Status | Actions | 
 PAED-001 — Providing paediatric emergency care to children | Anele Dlamini | 2026 S2 | Not scheduled No open review holds it, and 2026 S2 has not ended.
 | Schedule 
 | 
 PAED-002 — [PAED-002 title, as seeded] | Anele Dlamini | 2026 S2 | Not scheduled No open review holds it, and 2026 S2 has not ended.
 | Schedule 
 | 
 PAED-003 — Providing intensive care to children | Anele Dlamini | 2026 | Not scheduled No open review holds it, and 2026 has not ended.
 | Schedule 
 | 
 PAED-004 — [PAED-004 title, as seeded] | Anele Dlamini | 2026 S2 | Not scheduled No open review holds it, and 2026 S2 has not ended.
 | Schedule 
 | 
 PAED-005 — [PAED-005 title, as seeded] | Anele Dlamini | 2026 S2 | Not scheduled No open review holds it, and 2026 S2 has not ended.
 | Schedule 
 | 
 PAED-006 — [PAED-006 title, as seeded] | Anele Dlamini | 2026 | Not scheduled No open review holds it, and 2026 has not ended.
 | Schedule 
 | 
 PAED-007 — [PAED-007 title, as seeded] | Anele Dlamini | 2026 | Not scheduled No open review holds it, and 2026 has not ended.
 | Schedule 
 | 
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | Anele Dlamini | 2026 | As opportunity allows Decided as opportunity allows: optional, never missed.
 | Schedule 
 | 
 PAED-009 — [PAED-009 title, as seeded] | Anele Dlamini | 2026 | As opportunity allows Decided as opportunity allows: optional, never missed.
 | Schedule 
 | 
 PAED-010 — [PAED-010 title, as seeded] | Anele Dlamini | 2026 S2 | Not scheduled No open review holds it, and 2026 S2 has not ended.
 | Schedule 
 | 
 PAED-011 — [PAED-011 title, as seeded] | Anele Dlamini | 2026 | Not scheduled No open review holds it, and 2026 has not ended.
 | Schedule 
 | 
 PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | Anele Dlamini | 2026 S2 | Not scheduled No open review holds it, and 2026 S2 has not ended.
 | Schedule 
 | 
 PAED-013 — [PAED-013 title, as seeded] | Anele Dlamini | 2026 | As opportunity allows Decided as opportunity allows: optional, never missed.
 | Schedule 
 | 
 PAED-014 — [PAED-014 title, as seeded] | Anele Dlamini | 2026 | Not scheduled No open review holds it, and 2026 has not ended.
 | Schedule 
 | 
 PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | Anele Dlamini | 2026 | Not scheduled No open review holds it, and 2026 has not ended.
 | Schedule 
 | 
 PAED-001 — Providing paediatric emergency care to children | Pieter du Plessis | 2026 S2 | Not scheduled No open review holds it, and 2026 S2 has not ended.
 | Schedule 
 | 
 PAED-002 — [PAED-002 title, as seeded] | Pieter du Plessis | 2026 S2 | Not scheduled No open review holds it, and 2026 S2 has not ended.
 | Schedule 
 | 
 PAED-003 — Providing intensive care to children | Pieter du Plessis | 2026 | Not scheduled No open review holds it, and 2026 has not ended.
 | Schedule 
 | 
 PAED-004 — [PAED-004 title, as seeded] | Pieter du Plessis | 2026 S2 | Not scheduled No open review holds it, and 2026 S2 has not ended.
 | Schedule 
 | 
 PAED-005 — [PAED-005 title, as seeded] | Pieter du Plessis | 2026 S2 | Not scheduled No open review holds it, and 2026 S2 has not ended.
 | Schedule 
 | 
 Showing 1–20 of 75
 Previous Next 
 Decisions per page
 10
 20
 50
 100
=== FRAME: | By EPA shown | | Step 4.1 · Pieter Smit · twelve EPAs To schedule 5, three Optional 5; the fold's summary 44px at every width (S6); stacked at 900px and below (M8) | | Focus: stays on the fold's summary | 
 [shell] 
# Decisions due
 What the committee must decide, per registrar and per EPA, and where each decision stands.
 The EPAs due for an entrustment decision in 2026 S2 for the 5 trainees you oversee at Kgosi Kgari Teaching Hospital, by each EPA's decision cadence: every semester, or once a year. Missed is read today: the EPA's window has ended with nothing decided or on an open review's agenda.
 Period
 2025 S1 · 1 Jan to 30 Jun 2025
 2025 S2 · 1 Jul to 31 Dec 2025
 2026 S1 · 1 Jan to 30 Jun 2026
 2026 S2 · 1 Jul to 31 Dec 2026
 2027 S1 · 1 Jan to 30 Jun 2027
 2027 S2 · 1 Jul to 31 Dec 2027
 The College's semester 2 ends in November; its window runs to 31 December, so December's work counts. 
 Trainee
 Every trainee
 Anele Dlamini
 Pieter du Plessis
 Nomsa Mahlangu
 Lerato Molefe
 Sipho Ndlovu
 Status
 Outstanding: not yet decided
 Every status
 Not scheduled
 Scheduled
 Decided
 Not decided
 Revoked: re-decide
 Due by year end
 Partial period
 As opportunity allows
 Missed
 EPA
 Every EPA
 PAED-001 — Providing paediatric emergency care to children
 PAED-002 — [PAED-002 title, as seeded]
 PAED-003 — Providing intensive care to children
 PAED-004 — [PAED-004 title, as seeded]
 PAED-005 — [PAED-005 title, as seeded]
 PAED-006 — [PAED-006 title, as seeded]
 PAED-007 — [PAED-007 title, as seeded]
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
 PAED-009 — [PAED-009 title, as seeded]
 PAED-010 — [PAED-010 title, as seeded]
 PAED-011 — [PAED-011 title, as seeded]
 PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
 PAED-013 — [PAED-013 title, as seeded]
 PAED-014 — [PAED-014 title, as seeded]
 PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
 Show 
## By EPA
 The filters above do not narrow these counts.
 Counts for the 15 EPAs due in 2026 S2 Show Hide 
 How many trainees stand where on each EPA due in 2026 S2. To schedule: not scheduled, due by year end, or revoked. Optional: a partial period, or decided as opportunity allows.
 EPA | Due | Decided | Scheduled | Not decided | To schedule | Missed | Optional | 
 PAED-001 — Providing paediatric emergency care to children | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-002 — [PAED-002 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-003 — Providing intensive care to children | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-004 — [PAED-004 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-005 — [PAED-005 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-006 — [PAED-006 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-007 — [PAED-007 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-009 — [PAED-009 title, as seeded] | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-010 — [PAED-010 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-011 — [PAED-011 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-013 — [PAED-013 title, as seeded] | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-014 — [PAED-014 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 5 | 0 | 0 | 0 | 5 | 0 | 0 |


######## BOARD R2-3-decisions-due-1280-b.dc.html — Decisions due · 1280 · b
Decisions due · 1280 · b 
# Decisions due · 1280px (continued, part b) 
 Route /committee/decisions-due · the schedulers' list; a Committee member is refused.
 Components PageHeader, FilterBar (Show, Clear filters), DataTable (stack-wide), PagerControls (labelled), the .rung-legend fold with .fold-show / .fold-hide, StatePanel, Badge, AccessDenied. NEW the Trainee filter, .trainee-line (C2), .status-cell, .list-section-page, .list-section-count.
 DESIGN.md Decisions due's section; § Pager; § Table system. Round 3 the cadence (M1, M2), By EPA from the board's rows (M3), F1, F2, F6, M8, S1, S4, S5, S7.
=== FRAME: | After Next: page 2 | | Step 4.1 · Pieter Smit · C9, T264 | | Focus: the Decisions heading, which says the page | 
 [shell] 
# Decisions due
 What the committee must decide, per registrar and per EPA, and where each decision stands.
 The EPAs due for an entrustment decision in 2026 S2 for the 5 trainees you oversee at Kgosi Kgari Teaching Hospital, by each EPA's decision cadence: every semester, or once a year. Missed is read today: the EPA's window has ended with nothing decided or on an open review's agenda.
 Period
 2025 S1 · 1 Jan to 30 Jun 2025
 2025 S2 · 1 Jul to 31 Dec 2025
 2026 S1 · 1 Jan to 30 Jun 2026
 2026 S2 · 1 Jul to 31 Dec 2026
 2027 S1 · 1 Jan to 30 Jun 2027
 2027 S2 · 1 Jul to 31 Dec 2027
 The College's semester 2 ends in November; its window runs to 31 December, so December's work counts. 
 Trainee
 Every trainee
 Anele Dlamini
 Pieter du Plessis
 Nomsa Mahlangu
 Lerato Molefe
 Sipho Ndlovu
 Status
 Outstanding: not yet decided
 Every status
 Not scheduled
 Scheduled
 Decided
 Not decided
 Revoked: re-decide
 Due by year end
 Partial period
 As opportunity allows
 Missed
 EPA
 Every EPA
 PAED-001 — Providing paediatric emergency care to children
 PAED-002 — [PAED-002 title, as seeded]
 PAED-003 — Providing intensive care to children
 PAED-004 — [PAED-004 title, as seeded]
 PAED-005 — [PAED-005 title, as seeded]
 PAED-006 — [PAED-006 title, as seeded]
 PAED-007 — [PAED-007 title, as seeded]
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
 PAED-009 — [PAED-009 title, as seeded]
 PAED-010 — [PAED-010 title, as seeded]
 PAED-011 — [PAED-011 title, as seeded]
 PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
 PAED-013 — [PAED-013 title, as seeded]
 PAED-014 — [PAED-014 title, as seeded]
 PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
 Show 
## By EPA
 The filters above do not narrow these counts.
 Counts for the 15 EPAs due in 2026 S2 Show Hide 
 How many trainees stand where on each EPA due in 2026 S2. To schedule: not scheduled, due by year end, or revoked. Optional: a partial period, or decided as opportunity allows.
 EPA | Due | Decided | Scheduled | Not decided | To schedule | Missed | Optional | 
 PAED-001 — Providing paediatric emergency care to children | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-002 — [PAED-002 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-003 — Providing intensive care to children | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-004 — [PAED-004 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-005 — [PAED-005 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-006 — [PAED-006 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-007 — [PAED-007 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-009 — [PAED-009 title, as seeded] | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-010 — [PAED-010 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-011 — [PAED-011 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-013 — [PAED-013 title, as seeded] | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-014 — [PAED-014 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
## Decisions , page 2 of 4 
 75 of 75 decisions due in 2026 S2 shown.
 Decisions due in 2026 S2, by trainee then EPA
 EPA | Trainee | Window | Status | Actions | 
 PAED-006 — [PAED-006 title, as seeded] | Pieter du Plessis | 2026 | Not scheduled No open review holds it, and 2026 has not ended.
 | Schedule 
 | 
 PAED-007 — [PAED-007 title, as seeded] | Pieter du Plessis | 2026 | Not scheduled No open review holds it, and 2026 has not ended.
 | Schedule 
 | 
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | Pieter du Plessis | 2026 | As opportunity allows Decided as opportunity allows: optional, never missed.
 | Schedule 
 | 
 PAED-009 — [PAED-009 title, as seeded] | Pieter du Plessis | 2026 | As opportunity allows Decided as opportunity allows: optional, never missed.
 | Schedule 
 | 
 PAED-010 — [PAED-010 title, as seeded] | Pieter du Plessis | 2026 S2 | Not scheduled No open review holds it, and 2026 S2 has not ended.
 | Schedule 
 | 
 PAED-011 — [PAED-011 title, as seeded] | Pieter du Plessis | 2026 | Not scheduled No open review holds it, and 2026 has not ended.
 | Schedule 
 | 
 The other 14 rows of this page continue in the same form
 Showing 21–40 of 75
 Previous Next 
 Decisions per page
 10
 20
 50
 100
=== FRAME: | One trainee: no open review holds the period (C2) | | Step 5.1 · Pieter Smit · Trainee Lerato Molefe, every status | | Focus: the Decisions heading, after Show | 
 [shell] 
# Decisions due
 What the committee must decide, per registrar and per EPA, and where each decision stands.
 The EPAs due for an entrustment decision in 2026 S2 for the 5 trainees you oversee at Kgosi Kgari Teaching Hospital, by each EPA's decision cadence: every semester, or once a year. Missed is read today: the EPA's window has ended with nothing decided or on an open review's agenda.
 Period
 2025 S1 · 1 Jan to 30 Jun 2025
 2025 S2 · 1 Jul to 31 Dec 2025
 2026 S1 · 1 Jan to 30 Jun 2026
 2026 S2 · 1 Jul to 31 Dec 2026
 2027 S1 · 1 Jan to 30 Jun 2027
 2027 S2 · 1 Jul to 31 Dec 2027
 The College's semester 2 ends in November; its window runs to 31 December, so December's work counts. 
 Trainee
 Every trainee
 Anele Dlamini
 Pieter du Plessis
 Nomsa Mahlangu
 Lerato Molefe
 Sipho Ndlovu
 Status
 Outstanding: not yet decided
 Every status
 Not scheduled
 Scheduled
 Decided
 Not decided
 Revoked: re-decide
 Due by year end
 Partial period
 As opportunity allows
 Missed
 EPA
 Every EPA
 PAED-001 — Providing paediatric emergency care to children
 PAED-002 — [PAED-002 title, as seeded]
 PAED-003 — Providing intensive care to children
 PAED-004 — [PAED-004 title, as seeded]
 PAED-005 — [PAED-005 title, as seeded]
 PAED-006 — [PAED-006 title, as seeded]
 PAED-007 — [PAED-007 title, as seeded]
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
 PAED-009 — [PAED-009 title, as seeded]
 PAED-010 — [PAED-010 title, as seeded]
 PAED-011 — [PAED-011 title, as seeded]
 PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
 PAED-013 — [PAED-013 title, as seeded]
 PAED-014 — [PAED-014 title, as seeded]
 PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
 Show Clear filters 
## By EPA
 The filters above do not narrow these counts.
 Counts for the 15 EPAs due in 2026 S2 Show Hide 
 How many trainees stand where on each EPA due in 2026 S2. To schedule: not scheduled, due by year end, or revoked. Optional: a partial period, or decided as opportunity allows.
 EPA | Due | Decided | Scheduled | Not decided | To schedule | Missed | Optional | 
 PAED-001 — Providing paediatric emergency care to children | 5 | 2 | 0 | 3 | 0 | 0 | 0 | 
 PAED-002 — [PAED-002 title, as seeded] | 5 | 1 | 0 | 4 | 0 | 0 | 0 | 
 PAED-003 — Providing intensive care to children | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
 PAED-004 — [PAED-004 title, as seeded] | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
 PAED-005 — [PAED-005 title, as seeded] | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
 PAED-006 — [PAED-006 title, as seeded] | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
 PAED-007 — [PAED-007 title, as seeded] | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-009 — [PAED-009 title, as seeded] | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-010 — [PAED-010 title, as seeded] | 5 | 1 | 0 | 4 | 0 | 0 | 0 | 
 PAED-011 — [PAED-011 title, as seeded] | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
 PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 5 | 1 | 0 | 4 | 0 | 0 | 0 | 
 PAED-013 — [PAED-013 title, as seeded] | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-014 — [PAED-014 title, as seeded] | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
 PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
## Decisions 
 15 of 75 decisions due in 2026 S2 shown: Lerato Molefe, every status.
 No open review holds 2026 S2.
 Schedule a review 
 Lerato Molefe's decisions due in 2026 S2, every status, by EPA
 EPA | Window | Status | Actions | 
 PAED-001 — Providing paediatric emergency care to children | 2026 S2 | Decided STAR #1, issued at review #1 .
 | Decided: nothing to schedule 
 | 
 PAED-002 — [PAED-002 title, as seeded] | 2026 S2 | Not decided Not decided at review #1 . A later sitting for 2026 S2 plans it again.
 | Schedule 
 | 
 PAED-003 — Providing intensive care to children | 2026 | Not decided Not decided at review #1 . A later sitting for 2026 plans it again.
 | Schedule 
 | 
 PAED-004 — [PAED-004 title, as seeded] | 2026 S2 | Not decided Not decided at review #1 . A later sitting for 2026 S2 plans it again.
 | Schedule 
 | 
 PAED-005 — [PAED-005 title, as seeded] | 2026 S2 | Not decided Not decided at review #1 . A later sitting for 2026 S2 plans it again.
 | Schedule 
 | 
 PAED-006 — [PAED-006 title, as seeded] | 2026 | Not decided Not decided at review #1 . A later sitting for 2026 plans it again.
 | Schedule 
 | 
 PAED-007 — [PAED-007 title, as seeded] | 2026 | Not decided Not decided at review #1 . A later sitting for 2026 plans it again.
 | Schedule 
 | 
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 2026 | As opportunity allows Decided as opportunity allows: optional, never missed.
 | Schedule 
 | 
 PAED-009 — [PAED-009 title, as seeded] | 2026 | As opportunity allows Decided as opportunity allows: optional, never missed.
 | Schedule 
 | 
 PAED-010 — [PAED-010 title, as seeded] | 2026 S2 | Decided STAR #2, issued at review #1 .
 | Decided: nothing to schedule 
 | 
 PAED-011 — [PAED-011 title, as seeded] | 2026 | Not decided Not decided at review #1 . A later sitting for 2026 plans it again.
 | Schedule 
 | 
 PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 2026 S2 | Decided STAR #3, issued at review #1 .
 | Decided: nothing to schedule 
 | 
 PAED-013 — [PAED-013 title, as seeded] | 2026 | As opportunity allows Decided as opportunity allows: optional, never missed.
 | Schedule 
 | 
 PAED-014 — [PAED-014 title, as seeded] | 2026 | Not decided Not decided at review #1 . A later sitting for 2026 plans it again.
 | Schedule 
 | 
 PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 2026 | Not decided Not decided at review #1 . A later sitting for 2026 plans it again.
 | Schedule 
 | 
 Showing 1–15 of 15
 Previous Next 
 Decisions per page
 10
 20
 50
 100
=== FRAME: | One trainee: an open review holds the period (C2, C3) | | Steps 4.9 and 4.12 · each row's one action is Open review #1; no Schedule is drawn | 
 [shell] 
# Decisions due
 What the committee must decide, per registrar and per EPA, and where each decision stands.
 The EPAs due for an entrustment decision in 2026 S2 for the 5 trainees you oversee at Kgosi Kgari Teaching Hospital, by each EPA's decision cadence: every semester, or once a year. Missed is read today: the EPA's window has ended with nothing decided or on an open review's agenda.
 Period
 2025 S1 · 1 Jan to 30 Jun 2025
 2025 S2 · 1 Jul to 31 Dec 2025
 2026 S1 · 1 Jan to 30 Jun 2026
 2026 S2 · 1 Jul to 31 Dec 2026
 2027 S1 · 1 Jan to 30 Jun 2027
 2027 S2 · 1 Jul to 31 Dec 2027
 The College's semester 2 ends in November; its window runs to 31 December, so December's work counts. 
 Trainee
 Every trainee
 Anele Dlamini
 Pieter du Plessis
 Nomsa Mahlangu
 Lerato Molefe
 Sipho Ndlovu
 Status
 Outstanding: not yet decided
 Every status
 Not scheduled
 Scheduled
 Decided
 Not decided
 Revoked: re-decide
 Due by year end
 Partial period
 As opportunity allows
 Missed
 EPA
 Every EPA
 PAED-001 — Providing paediatric emergency care to children
 PAED-002 — [PAED-002 title, as seeded]
 PAED-003 — Providing intensive care to children
 PAED-004 — [PAED-004 title, as seeded]
 PAED-005 — [PAED-005 title, as seeded]
 PAED-006 — [PAED-006 title, as seeded]
 PAED-007 — [PAED-007 title, as seeded]
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
 PAED-009 — [PAED-009 title, as seeded]
 PAED-010 — [PAED-010 title, as seeded]
 PAED-011 — [PAED-011 title, as seeded]
 PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
 PAED-013 — [PAED-013 title, as seeded]
 PAED-014 — [PAED-014 title, as seeded]
 PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
 Show Clear filters 
## By EPA
 The filters above do not narrow these counts.
 Counts for the 15 EPAs due in 2026 S2 Show Hide 
 How many trainees stand where on each EPA due in 2026 S2. To schedule: not scheduled, due by year end, or revoked. Optional: a partial period, or decided as opportunity allows.
 EPA | Due | Decided | Scheduled | Not decided | To schedule | Missed | Optional | 
 PAED-001 — Providing paediatric emergency care to children | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-002 — [PAED-002 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-003 — Providing intensive care to children | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-004 — [PAED-004 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-005 — [PAED-005 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-006 — [PAED-006 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-007 — [PAED-007 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-009 — [PAED-009 title, as seeded] | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-010 — [PAED-010 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-011 — [PAED-011 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-013 — [PAED-013 title, as seeded] | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-014 — [PAED-014 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
## Decisions 
 15 of 75 decisions due in 2026 S2 shown: Lerato Molefe, outstanding.
 Review #1 (annual progression) holds 2026 S2 before Paed Annual Review Panel, scheduled on 2026-10-12.
 Open review #1 
 Lerato Molefe's decisions due in 2026 S2, outstanding, by EPA
 EPA | Window | Status | Actions | 
 PAED-001 — Providing paediatric emergency care to children | 2026 S2 | Scheduled On the agenda of review #1 before Paed Annual Review Panel, which is still open.
 | Open review #1 
 | 
 PAED-002 — [PAED-002 title, as seeded] | 2026 S2 | Scheduled On the agenda of review #1 before Paed Annual Review Panel, which is still open.
 | Open review #1 
 | 
 PAED-003 — Providing intensive care to children | 2026 | Scheduled On the agenda of review #1 before Paed Annual Review Panel, which is still open.
 | Open review #1 
 | 
 PAED-004 — [PAED-004 title, as seeded] | 2026 S2 | Scheduled On the agenda of review #1 before Paed Annual Review Panel, which is still open.
 | Open review #1 
 | 
 The other 11 rows of this page continue in the same form
 Showing 1–15 of 15
 Previous Next 
 Decisions per page
 10
 20
 50
 100


######## BOARD R2-3-decisions-due-1280-c.dc.html — Decisions due · 1280 · c
Decisions due · 1280 · c 
# Decisions due · 1280px (continued, part c) 
 Route /committee/decisions-due · the schedulers' list; a Committee member is refused.
 Components PageHeader, FilterBar (Show, Clear filters), DataTable (stack-wide), PagerControls (labelled), the .rung-legend fold with .fold-show / .fold-hide, StatePanel, Badge, AccessDenied. NEW the Trainee filter, .trainee-line (C2), .status-cell, .list-section-page, .list-section-count.
 DESIGN.md Decisions due's section; § Pager; § Table system. Round 3 the cadence (M1, M2), By EPA from the board's rows (M3), F1, F2, F6, M8, S1, S4, S5, S7.
=== FRAME: | Heavy: two panels can hold the trainee (S1) | | Pieter Smit · one trainee line per panel; PAED-004 and PAED-005 are the neonatal panel's: Not scheduled, their Schedule named for it | 
 [shell] 
# Decisions due
 What the committee must decide, per registrar and per EPA, and where each decision stands.
 The EPAs due for an entrustment decision in 2026 S2 for the 5 trainees you oversee at Kgosi Kgari Teaching Hospital, by each EPA's decision cadence: every semester, or once a year. Missed is read today: the EPA's window has ended with nothing decided or on an open review's agenda.
 Period
 2025 S1 · 1 Jan to 30 Jun 2025
 2025 S2 · 1 Jul to 31 Dec 2025
 2026 S1 · 1 Jan to 30 Jun 2026
 2026 S2 · 1 Jul to 31 Dec 2026
 2027 S1 · 1 Jan to 30 Jun 2027
 2027 S2 · 1 Jul to 31 Dec 2027
 The College's semester 2 ends in November; its window runs to 31 December, so December's work counts. 
 Trainee
 Every trainee
 Anele Dlamini
 Pieter du Plessis
 Nomsa Mahlangu
 Lerato Molefe
 Sipho Ndlovu
 Status
 Outstanding: not yet decided
 Every status
 Not scheduled
 Scheduled
 Decided
 Not decided
 Revoked: re-decide
 Due by year end
 Partial period
 As opportunity allows
 Missed
 EPA
 Every EPA
 PAED-001 — Providing paediatric emergency care to children
 PAED-002 — [PAED-002 title, as seeded]
 PAED-003 — Providing intensive care to children
 PAED-004 — [PAED-004 title, as seeded]
 PAED-005 — [PAED-005 title, as seeded]
 PAED-006 — [PAED-006 title, as seeded]
 PAED-007 — [PAED-007 title, as seeded]
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
 PAED-009 — [PAED-009 title, as seeded]
 PAED-010 — [PAED-010 title, as seeded]
 PAED-011 — [PAED-011 title, as seeded]
 PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
 PAED-013 — [PAED-013 title, as seeded]
 PAED-014 — [PAED-014 title, as seeded]
 PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
 Show Clear filters 
## By EPA
 The filters above do not narrow these counts.
 Counts for the 15 EPAs due in 2026 S2 Show Hide 
 How many trainees stand where on each EPA due in 2026 S2. To schedule: not scheduled, due by year end, or revoked. Optional: a partial period, or decided as opportunity allows.
 EPA | Due | Decided | Scheduled | Not decided | To schedule | Missed | Optional | 
 PAED-001 — Providing paediatric emergency care to children | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-002 — [PAED-002 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-003 — Providing intensive care to children | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-004 — [PAED-004 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-005 — [PAED-005 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-006 — [PAED-006 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-007 — [PAED-007 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-009 — [PAED-009 title, as seeded] | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-010 — [PAED-010 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-011 — [PAED-011 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-013 — [PAED-013 title, as seeded] | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-014 — [PAED-014 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
## Decisions 
 15 of 75 decisions due in 2026 S2 shown: Lerato Molefe, outstanding.
 Review #1 (annual progression) holds 2026 S2 before Paed Annual Review Panel, scheduled on 2026-10-12.
 Open review #1 
 No open review before [Neonatal panel] holds 2026 S2.
 Schedule a review before [Neonatal panel] 
 Lerato Molefe's decisions due in 2026 S2, outstanding, by EPA
 EPA | Window | Status | Actions | 
 PAED-001 — Providing paediatric emergency care to children | 2026 S2 | Scheduled On the agenda of review #1 before Paed Annual Review Panel, which is still open.
 | Open review #1 
 | 
 PAED-002 — [PAED-002 title, as seeded] | 2026 S2 | Scheduled On the agenda of review #1 before Paed Annual Review Panel, which is still open.
 | Open review #1 
 | 
 PAED-003 — Providing intensive care to children | 2026 | Scheduled On the agenda of review #1 before Paed Annual Review Panel, which is still open.
 | Open review #1 
 | 
 PAED-004 — [PAED-004 title, as seeded] | 2026 S2 | Not scheduled No open review holds it, and 2026 S2 has not ended.
 | Schedule 
 | 
 PAED-005 — [PAED-005 title, as seeded] | 2026 S2 | Not scheduled No open review holds it, and 2026 S2 has not ended.
 | Schedule 
 | 
 PAED-006 — [PAED-006 title, as seeded] | 2026 | Scheduled On the agenda of review #1 before Paed Annual Review Panel, which is still open.
 | Open review #1 
 | 
 The other 9 rows of this page continue in the same form
 Showing 1–15 of 15
 Previous Next 
 Decisions per page
 10
 20
 50
 100
=== FRAME: | One trainee: a formative check-in is open (C2) | | Steps 4.49 and A.1.10 · the check-in takes no seat, so the period can still be scheduled | 
 [shell] 
# Decisions due
 What the committee must decide, per registrar and per EPA, and where each decision stands.
 The EPAs due for an entrustment decision in 2026 S2 for the 5 trainees you oversee at Kgosi Kgari Teaching Hospital, by each EPA's decision cadence: every semester, or once a year. Missed is read today: the EPA's window has ended with nothing decided or on an open review's agenda.
 Period
 2025 S1 · 1 Jan to 30 Jun 2025
 2025 S2 · 1 Jul to 31 Dec 2025
 2026 S1 · 1 Jan to 30 Jun 2026
 2026 S2 · 1 Jul to 31 Dec 2026
 2027 S1 · 1 Jan to 30 Jun 2027
 2027 S2 · 1 Jul to 31 Dec 2027
 The College's semester 2 ends in November; its window runs to 31 December, so December's work counts. 
 Trainee
 Every trainee
 Anele Dlamini
 Pieter du Plessis
 Nomsa Mahlangu
 Lerato Molefe
 Sipho Ndlovu
 Status
 Outstanding: not yet decided
 Every status
 Not scheduled
 Scheduled
 Decided
 Not decided
 Revoked: re-decide
 Due by year end
 Partial period
 As opportunity allows
 Missed
 EPA
 Every EPA
 PAED-001 — Providing paediatric emergency care to children
 PAED-002 — [PAED-002 title, as seeded]
 PAED-003 — Providing intensive care to children
 PAED-004 — [PAED-004 title, as seeded]
 PAED-005 — [PAED-005 title, as seeded]
 PAED-006 — [PAED-006 title, as seeded]
 PAED-007 — [PAED-007 title, as seeded]
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
 PAED-009 — [PAED-009 title, as seeded]
 PAED-010 — [PAED-010 title, as seeded]
 PAED-011 — [PAED-011 title, as seeded]
 PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
 PAED-013 — [PAED-013 title, as seeded]
 PAED-014 — [PAED-014 title, as seeded]
 PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
 Show Clear filters 
## By EPA
 The filters above do not narrow these counts.
 Counts for the 15 EPAs due in 2026 S2 Show Hide 
 How many trainees stand where on each EPA due in 2026 S2. To schedule: not scheduled, due by year end, or revoked. Optional: a partial period, or decided as opportunity allows.
 EPA | Due | Decided | Scheduled | Not decided | To schedule | Missed | Optional | 
 PAED-001 — Providing paediatric emergency care to children | 5 | 2 | 0 | 3 | 0 | 0 | 0 | 
 PAED-002 — [PAED-002 title, as seeded] | 5 | 1 | 0 | 4 | 0 | 0 | 0 | 
 PAED-003 — Providing intensive care to children | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
 PAED-004 — [PAED-004 title, as seeded] | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
 PAED-005 — [PAED-005 title, as seeded] | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
 PAED-006 — [PAED-006 title, as seeded] | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
 PAED-007 — [PAED-007 title, as seeded] | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-009 — [PAED-009 title, as seeded] | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-010 — [PAED-010 title, as seeded] | 5 | 1 | 0 | 4 | 0 | 0 | 0 | 
 PAED-011 — [PAED-011 title, as seeded] | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
 PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 5 | 1 | 0 | 4 | 0 | 0 | 0 | 
 PAED-013 — [PAED-013 title, as seeded] | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-014 — [PAED-014 title, as seeded] | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
 PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
## Decisions 
 15 of 75 decisions due in 2026 S2 shown: Sipho Ndlovu, every status.
 No open review holds 2026 S2.
 Schedule a review 
 Formative check-in #6 is open: it takes no seat.
 Open check-in #6 
 Sipho Ndlovu's decisions due in 2026 S2, every status, by EPA
 EPA | Window | Status | Actions | 
 PAED-001 — Providing paediatric emergency care to children | 2026 S2 | Not decided Not decided at review #5 . A later sitting for 2026 S2 plans it again.
 | Schedule 
 | 
 PAED-002 — [PAED-002 title, as seeded] | 2026 S2 | Not decided Not decided at review #5 . A later sitting for 2026 S2 plans it again.
 | Schedule 
 | 
 PAED-003 — Providing intensive care to children | 2026 | Not decided Not decided at review #5 . A later sitting for 2026 plans it again.
 | Schedule 
 | 
 PAED-004 — [PAED-004 title, as seeded] | 2026 S2 | Not decided Not decided at review #5 . A later sitting for 2026 S2 plans it again.
 | Schedule 
 | 
 The other 11 rows of this page continue in the same form
 Showing 1–15 of 15
 Previous Next 
 Decisions per page
 10
 20
 50
 100
=== FRAME: | Filtered to one EPA | | Step 4.1 · Pieter Smit · one row a registrar, by surname; By EPA does not change | | Focus: the Decisions heading, after Show | 
 [shell] 
# Decisions due
 What the committee must decide, per registrar and per EPA, and where each decision stands.
 The EPAs due for an entrustment decision in 2026 S2 for the 5 trainees you oversee at Kgosi Kgari Teaching Hospital, by each EPA's decision cadence: every semester, or once a year. Missed is read today: the EPA's window has ended with nothing decided or on an open review's agenda.
 Period
 2025 S1 · 1 Jan to 30 Jun 2025
 2025 S2 · 1 Jul to 31 Dec 2025
 2026 S1 · 1 Jan to 30 Jun 2026
 2026 S2 · 1 Jul to 31 Dec 2026
 2027 S1 · 1 Jan to 30 Jun 2027
 2027 S2 · 1 Jul to 31 Dec 2027
 The College's semester 2 ends in November; its window runs to 31 December, so December's work counts. 
 Trainee
 Every trainee
 Anele Dlamini
 Pieter du Plessis
 Nomsa Mahlangu
 Lerato Molefe
 Sipho Ndlovu
 Status
 Outstanding: not yet decided
 Every status
 Not scheduled
 Scheduled
 Decided
 Not decided
 Revoked: re-decide
 Due by year end
 Partial period
 As opportunity allows
 Missed
 EPA
 Every EPA
 PAED-001 — Providing paediatric emergency care to children
 PAED-002 — [PAED-002 title, as seeded]
 PAED-003 — Providing intensive care to children
 PAED-004 — [PAED-004 title, as seeded]
 PAED-005 — [PAED-005 title, as seeded]
 PAED-006 — [PAED-006 title, as seeded]
 PAED-007 — [PAED-007 title, as seeded]
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
 PAED-009 — [PAED-009 title, as seeded]
 PAED-010 — [PAED-010 title, as seeded]
 PAED-011 — [PAED-011 title, as seeded]
 PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
 PAED-013 — [PAED-013 title, as seeded]
 PAED-014 — [PAED-014 title, as seeded]
 PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
 Show Clear filters 
## By EPA
 The filters above do not narrow these counts.
 Counts for the 15 EPAs due in 2026 S2 Show Hide 
 How many trainees stand where on each EPA due in 2026 S2. To schedule: not scheduled, due by year end, or revoked. Optional: a partial period, or decided as opportunity allows.
 EPA | Due | Decided | Scheduled | Not decided | To schedule | Missed | Optional | 
 PAED-001 — Providing paediatric emergency care to children | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-002 — [PAED-002 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-003 — Providing intensive care to children | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-004 — [PAED-004 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-005 — [PAED-005 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-006 — [PAED-006 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-007 — [PAED-007 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-009 — [PAED-009 title, as seeded] | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-010 — [PAED-010 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-011 — [PAED-011 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-013 — [PAED-013 title, as seeded] | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-014 — [PAED-014 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
## Decisions 
 5 of 75 decisions due in 2026 S2 shown: Not scheduled, PAED-004.
 Decisions due in 2026 S2: Not scheduled, PAED-004
 EPA | Trainee | Window | Status | Actions | 
 PAED-004 — [PAED-004 title, as seeded] | Anele Dlamini | 2026 S2 | Not scheduled No open review holds it, and 2026 S2 has not ended.
 | Schedule 
 | 
 PAED-004 — [PAED-004 title, as seeded] | Pieter du Plessis | 2026 S2 | Not scheduled No open review holds it, and 2026 S2 has not ended.
 | Schedule 
 | 
 PAED-004 — [PAED-004 title, as seeded] | Nomsa Mahlangu | 2026 S2 | Not scheduled No open review holds it, and 2026 S2 has not ended.
 | Schedule 
 | 
 PAED-004 — [PAED-004 title, as seeded] | Lerato Molefe | 2026 S2 | Not scheduled No open review holds it, and 2026 S2 has not ended.
 | Schedule 
 | 
 PAED-004 — [PAED-004 title, as seeded] | Sipho Ndlovu | 2026 S2 | Not scheduled No open review holds it, and 2026 S2 has not ended.
 | Schedule 
 | 
 Showing 1–5 of 5
 Previous Next 
 Decisions per page
 10
 20
 50
 100
=== FRAME: | No row matches | | Pieter Smit · FilterBar's pattern | | Focus: the Decisions heading, after Show | 
 [shell] 
# Decisions due
 What the committee must decide, per registrar and per EPA, and where each decision stands.
 The EPAs due for an entrustment decision in 2026 S2 for the 5 trainees you oversee at Kgosi Kgari Teaching Hospital, by each EPA's decision cadence: every semester, or once a year. Missed is read today: the EPA's window has ended with nothing decided or on an open review's agenda.
 Period
 2025 S1 · 1 Jan to 30 Jun 2025
 2025 S2 · 1 Jul to 31 Dec 2025
 2026 S1 · 1 Jan to 30 Jun 2026
 2026 S2 · 1 Jul to 31 Dec 2026
 2027 S1 · 1 Jan to 30 Jun 2027
 2027 S2 · 1 Jul to 31 Dec 2027
 The College's semester 2 ends in November; its window runs to 31 December, so December's work counts. 
 Trainee
 Every trainee
 Anele Dlamini
 Pieter du Plessis
 Nomsa Mahlangu
 Lerato Molefe
 Sipho Ndlovu
 Status
 Outstanding: not yet decided
 Every status
 Not scheduled
 Scheduled
 Decided
 Not decided
 Revoked: re-decide
 Due by year end
 Partial period
 As opportunity allows
 Missed
 EPA
 Every EPA
 PAED-001 — Providing paediatric emergency care to children
 PAED-002 — [PAED-002 title, as seeded]
 PAED-003 — Providing intensive care to children
 PAED-004 — [PAED-004 title, as seeded]
 PAED-005 — [PAED-005 title, as seeded]
 PAED-006 — [PAED-006 title, as seeded]
 PAED-007 — [PAED-007 title, as seeded]
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
 PAED-009 — [PAED-009 title, as seeded]
 PAED-010 — [PAED-010 title, as seeded]
 PAED-011 — [PAED-011 title, as seeded]
 PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
 PAED-013 — [PAED-013 title, as seeded]
 PAED-014 — [PAED-014 title, as seeded]
 PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
 Show Clear filters 
## By EPA
 The filters above do not narrow these counts.
 Counts for the 15 EPAs due in 2026 S2 Show Hide 
 How many trainees stand where on each EPA due in 2026 S2. To schedule: not scheduled, due by year end, or revoked. Optional: a partial period, or decided as opportunity allows.
 EPA | Due | Decided | Scheduled | Not decided | To schedule | Missed | Optional | 
 PAED-001 — Providing paediatric emergency care to children | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-002 — [PAED-002 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-003 — Providing intensive care to children | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-004 — [PAED-004 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-005 — [PAED-005 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-006 — [PAED-006 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-007 — [PAED-007 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-009 — [PAED-009 title, as seeded] | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-010 — [PAED-010 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-011 — [PAED-011 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-013 — [PAED-013 title, as seeded] | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-014 — [PAED-014 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
## Decisions
 0 of 75 decisions due in 2026 S2 shown.
 No decision matches these filters.
 Missed only, every trainee, every EPA.
 Clear filters


######## BOARD R2-3-decisions-due-1280-d.dc.html — Decisions due · 1280 · d
Decisions due · 1280 · d 
# Decisions due · 1280px (continued, part d) 
 Route /committee/decisions-due · the schedulers' list; a Committee member is refused.
 Components PageHeader, FilterBar (Show, Clear filters), DataTable (stack-wide), PagerControls (labelled), the .rung-legend fold with .fold-show / .fold-hide, StatePanel, Badge, AccessDenied. NEW the Trainee filter, .trainee-line (C2), .status-cell, .list-section-page, .list-section-count.
 DESIGN.md Decisions due's section; § Pager; § Table system. Round 3 the cadence (M1, M2), By EPA from the board's rows (M3), F1, F2, F6, M8, S1, S4, S5, S7.
=== FRAME: | A past semester, 2026 S1: Missed rows (F1, M1, M3) | | Step 4.2 · Nolwazi Mbatha · the six semester EPAs Missed for every registrar (Mahlangu's and Ndlovu's 15 January start counts, D42); the annual six Due by year end, PAED-003 and 006 among them; the optional three As opportunity allows. By EPA open: Missed 5 on 001, 002, 004, 005, 010 and 012. Each row's Schedule opens the form on 2026 S1 | 
 [shell] 
# Decisions due
 What the committee must decide, per registrar and per EPA, and where each decision stands.
 The EPAs due for an entrustment decision in 2026 S1 for the 5 trainees you oversee at Kgosi Kgari Teaching Hospital, by each EPA's decision cadence: every semester, or once a year. Missed is read today: the EPA's window has ended with nothing decided or on an open review's agenda.
 Period
 2025 S1 · 1 Jan to 30 Jun 2025
 2025 S2 · 1 Jul to 31 Dec 2025
 2026 S1 · 1 Jan to 30 Jun 2026
 2026 S2 · 1 Jul to 31 Dec 2026
 2027 S1 · 1 Jan to 30 Jun 2027
 2027 S2 · 1 Jul to 31 Dec 2027
 Trainee
 Every trainee
 Anele Dlamini
 Pieter du Plessis
 Nomsa Mahlangu
 Lerato Molefe
 Sipho Ndlovu
 Status
 Outstanding: not yet decided
 Every status
 Not scheduled
 Scheduled
 Decided
 Not decided
 Revoked: re-decide
 Due by year end
 Partial period
 As opportunity allows
 Missed
 EPA
 Every EPA
 PAED-001 — Providing paediatric emergency care to children
 PAED-002 — [PAED-002 title, as seeded]
 PAED-003 — Providing intensive care to children
 PAED-004 — [PAED-004 title, as seeded]
 PAED-005 — [PAED-005 title, as seeded]
 PAED-006 — [PAED-006 title, as seeded]
 PAED-007 — [PAED-007 title, as seeded]
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
 PAED-009 — [PAED-009 title, as seeded]
 PAED-010 — [PAED-010 title, as seeded]
 PAED-011 — [PAED-011 title, as seeded]
 PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
 PAED-013 — [PAED-013 title, as seeded]
 PAED-014 — [PAED-014 title, as seeded]
 PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
 Show Clear filters 
## By EPA
 The filters above do not narrow these counts.
 Counts for the 15 EPAs due in 2026 S1 Show Hide 
 How many trainees stand where on each EPA due in 2026 S1. To schedule: not scheduled, due by year end, or revoked. Optional: a partial period, or decided as opportunity allows.
 EPA | Due | Decided | Scheduled | Not decided | To schedule | Missed | Optional | 
 PAED-001 — Providing paediatric emergency care to children | 5 | 0 | 0 | 0 | 0 | 5 | 0 | 
 PAED-002 — [PAED-002 title, as seeded] | 5 | 0 | 0 | 0 | 0 | 5 | 0 | 
 PAED-003 — Providing intensive care to children | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-004 — [PAED-004 title, as seeded] | 5 | 0 | 0 | 0 | 0 | 5 | 0 | 
 PAED-005 — [PAED-005 title, as seeded] | 5 | 0 | 0 | 0 | 0 | 5 | 0 | 
 PAED-006 — [PAED-006 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-007 — [PAED-007 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-009 — [PAED-009 title, as seeded] | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-010 — [PAED-010 title, as seeded] | 5 | 0 | 0 | 0 | 0 | 5 | 0 | 
 PAED-011 — [PAED-011 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 5 | 0 | 0 | 0 | 0 | 5 | 0 | 
 PAED-013 — [PAED-013 title, as seeded] | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-014 — [PAED-014 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
## Decisions , page 1 of 4 
 75 of 75 decisions due in 2026 S1 shown.
 Decisions due in 2026 S1, by trainee then EPA
 EPA | Trainee | Window | Status | Actions | 
 PAED-001 — Providing paediatric emergency care to children | Anele Dlamini | 2026 S1 | Missed 2026 S1 has ended with nothing decided or on an open review's agenda.
 | Schedule 
 | 
 PAED-002 — [PAED-002 title, as seeded] | Anele Dlamini | 2026 S1 | Missed 2026 S1 has ended with nothing decided or on an open review's agenda.
 | Schedule 
 | 
 PAED-003 — Providing intensive care to children | Anele Dlamini | 2026 | Due by year end Decided once a year: due by the last sitting of 2026, and not missed before the year ends.
 | Schedule 
 | 
 PAED-004 — [PAED-004 title, as seeded] | Anele Dlamini | 2026 S1 | Missed 2026 S1 has ended with nothing decided or on an open review's agenda.
 | Schedule 
 | 
 PAED-005 — [PAED-005 title, as seeded] | Anele Dlamini | 2026 S1 | Missed 2026 S1 has ended with nothing decided or on an open review's agenda.
 | Schedule 
 | 
 PAED-006 — [PAED-006 title, as seeded] | Anele Dlamini | 2026 | Due by year end Decided once a year: due by the last sitting of 2026, and not missed before the year ends.
 | Schedule 
 | 
 PAED-007 — [PAED-007 title, as seeded] | Anele Dlamini | 2026 | Due by year end Decided once a year: due by the last sitting of 2026, and not missed before the year ends.
 | Schedule 
 | 
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | Anele Dlamini | 2026 | As opportunity allows Decided as opportunity allows: optional, never missed.
 | Schedule 
 | 
 The other 12 rows of this page continue in the same form
 Showing 1–20 of 75
 Previous Next 
 Decisions per page
 10
 20
 50
 100
=== FRAME: | All scheduled | | Step 4.10 (4.12 reads the same) · Kabelo Sithole · By EPA open: Scheduled 5 and To schedule 0 on every EPA; each row's one action is Open review #n; the sentence names the panel (T259) | 
 [shell] 
# Decisions due
 What the committee must decide, per registrar and per EPA, and where each decision stands.
 The EPAs due for an entrustment decision in 2026 S2 for the 5 trainees you oversee at Kgosi Kgari Teaching Hospital, by each EPA's decision cadence: every semester, or once a year. Missed is read today: the EPA's window has ended with nothing decided or on an open review's agenda.
 Period
 2025 S1 · 1 Jan to 30 Jun 2025
 2025 S2 · 1 Jul to 31 Dec 2025
 2026 S1 · 1 Jan to 30 Jun 2026
 2026 S2 · 1 Jul to 31 Dec 2026
 2027 S1 · 1 Jan to 30 Jun 2027
 2027 S2 · 1 Jul to 31 Dec 2027
 The College's semester 2 ends in November; its window runs to 31 December, so December's work counts. 
 Trainee
 Every trainee
 Anele Dlamini
 Pieter du Plessis
 Nomsa Mahlangu
 Lerato Molefe
 Sipho Ndlovu
 Status
 Outstanding: not yet decided
 Every status
 Not scheduled
 Scheduled
 Decided
 Not decided
 Revoked: re-decide
 Due by year end
 Partial period
 As opportunity allows
 Missed
 EPA
 Every EPA
 PAED-001 — Providing paediatric emergency care to children
 PAED-002 — [PAED-002 title, as seeded]
 PAED-003 — Providing intensive care to children
 PAED-004 — [PAED-004 title, as seeded]
 PAED-005 — [PAED-005 title, as seeded]
 PAED-006 — [PAED-006 title, as seeded]
 PAED-007 — [PAED-007 title, as seeded]
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
 PAED-009 — [PAED-009 title, as seeded]
 PAED-010 — [PAED-010 title, as seeded]
 PAED-011 — [PAED-011 title, as seeded]
 PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
 PAED-013 — [PAED-013 title, as seeded]
 PAED-014 — [PAED-014 title, as seeded]
 PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
 Show 
## By EPA
 The filters above do not narrow these counts.
 Counts for the 15 EPAs due in 2026 S2 Show Hide 
 How many trainees stand where on each EPA due in 2026 S2. To schedule: not scheduled, due by year end, or revoked. Optional: a partial period, or decided as opportunity allows.
 EPA | Due | Decided | Scheduled | Not decided | To schedule | Missed | Optional | 
 PAED-001 — Providing paediatric emergency care to children | 5 | 0 | 5 | 0 | 0 | 0 | 0 | 
 PAED-002 — [PAED-002 title, as seeded] | 5 | 0 | 5 | 0 | 0 | 0 | 0 | 
 PAED-003 — Providing intensive care to children | 5 | 0 | 5 | 0 | 0 | 0 | 0 | 
 PAED-004 — [PAED-004 title, as seeded] | 5 | 0 | 5 | 0 | 0 | 0 | 0 | 
 PAED-005 — [PAED-005 title, as seeded] | 5 | 0 | 5 | 0 | 0 | 0 | 0 | 
 PAED-006 — [PAED-006 title, as seeded] | 5 | 0 | 5 | 0 | 0 | 0 | 0 | 
 PAED-007 — [PAED-007 title, as seeded] | 5 | 0 | 5 | 0 | 0 | 0 | 0 | 
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 5 | 0 | 5 | 0 | 0 | 0 | 0 | 
 PAED-009 — [PAED-009 title, as seeded] | 5 | 0 | 5 | 0 | 0 | 0 | 0 | 
 PAED-010 — [PAED-010 title, as seeded] | 5 | 0 | 5 | 0 | 0 | 0 | 0 | 
 PAED-011 — [PAED-011 title, as seeded] | 5 | 0 | 5 | 0 | 0 | 0 | 0 | 
 PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 5 | 0 | 5 | 0 | 0 | 0 | 0 | 
 PAED-013 — [PAED-013 title, as seeded] | 5 | 0 | 5 | 0 | 0 | 0 | 0 | 
 PAED-014 — [PAED-014 title, as seeded] | 5 | 0 | 5 | 0 | 0 | 0 | 0 | 
 PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 5 | 0 | 5 | 0 | 0 | 0 | 0 | 
## Decisions , page 1 of 4 
 75 of 75 decisions due in 2026 S2 shown.
 Decisions due in 2026 S2, by trainee then EPA
 EPA | Trainee | Window | Status | Actions | 
 PAED-001 — Providing paediatric emergency care to children | Anele Dlamini | 2026 S2 | Scheduled On the agenda of review #2 before Paed Annual Review Panel, which is still open.
 | Open review #2 
 | 
 PAED-002 — [PAED-002 title, as seeded] | Anele Dlamini | 2026 S2 | Scheduled On the agenda of review #2 before Paed Annual Review Panel, which is still open.
 | Open review #2 
 | 
 PAED-003 — Providing intensive care to children | Anele Dlamini | 2026 | Scheduled On the agenda of review #2 before Paed Annual Review Panel, which is still open.
 | Open review #2 
 | 
 PAED-004 — [PAED-004 title, as seeded] | Anele Dlamini | 2026 S2 | Scheduled On the agenda of review #2 before Paed Annual Review Panel, which is still open.
 | Open review #2 
 | 
 PAED-005 — [PAED-005 title, as seeded] | Anele Dlamini | 2026 S2 | Scheduled On the agenda of review #2 before Paed Annual Review Panel, which is still open.
 | Open review #2 
 | 
 PAED-006 — [PAED-006 title, as seeded] | Anele Dlamini | 2026 | Scheduled On the agenda of review #2 before Paed Annual Review Panel, which is still open.
 | Open review #2 
 | 
 PAED-007 — [PAED-007 title, as seeded] | Anele Dlamini | 2026 | Scheduled On the agenda of review #2 before Paed Annual Review Panel, which is still open.
 | Open review #2 
 | 
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | Anele Dlamini | 2026 | Scheduled On the agenda of review #2 before Paed Annual Review Panel, which is still open.
 | Open review #2 
 | 
 The other 12 rows of this page continue in the same form
 Showing 1–20 of 75
 Previous Next 
 Decisions per page
 10
 20
 50
 100


######## BOARD R2-3-decisions-due-1280-e.dc.html — Decisions due · 1280 · e
Decisions due · 1280 · e 
# Decisions due · 1280px (continued, part e) 
 Route /committee/decisions-due · the schedulers' list; a Committee member is refused.
 Components PageHeader, FilterBar (Show, Clear filters), DataTable (stack-wide), PagerControls (labelled), the .rung-legend fold with .fold-show / .fold-hide, StatePanel, Badge, AccessDenied. NEW the Trainee filter, .trainee-line (C2), .status-cell, .list-section-page, .list-section-count.
 DESIGN.md Decisions due's section; § Pager; § Table system. Round 3 the cadence (M1, M2), By EPA from the board's rows (M3), F1, F2, F6, M8, S1, S4, S5, S7.
=== FRAME: | After the sitting: Decided, Not decided, optional | | Step 4.34 · Nolwazi Mbatha · Every status (the default filter shows 70 of 75); By EPA open: PAED-001 Due 5, Decided 2, Not decided 3 | 
 [shell] 
# Decisions due
 What the committee must decide, per registrar and per EPA, and where each decision stands.
 The EPAs due for an entrustment decision in 2026 S2 for the 5 trainees you oversee at Kgosi Kgari Teaching Hospital, by each EPA's decision cadence: every semester, or once a year. Missed is read today: the EPA's window has ended with nothing decided or on an open review's agenda.
 Period
 2025 S1 · 1 Jan to 30 Jun 2025
 2025 S2 · 1 Jul to 31 Dec 2025
 2026 S1 · 1 Jan to 30 Jun 2026
 2026 S2 · 1 Jul to 31 Dec 2026
 2027 S1 · 1 Jan to 30 Jun 2027
 2027 S2 · 1 Jul to 31 Dec 2027
 The College's semester 2 ends in November; its window runs to 31 December, so December's work counts. 
 Trainee
 Every trainee
 Anele Dlamini
 Pieter du Plessis
 Nomsa Mahlangu
 Lerato Molefe
 Sipho Ndlovu
 Status
 Outstanding: not yet decided
 Every status
 Not scheduled
 Scheduled
 Decided
 Not decided
 Revoked: re-decide
 Due by year end
 Partial period
 As opportunity allows
 Missed
 EPA
 Every EPA
 PAED-001 — Providing paediatric emergency care to children
 PAED-002 — [PAED-002 title, as seeded]
 PAED-003 — Providing intensive care to children
 PAED-004 — [PAED-004 title, as seeded]
 PAED-005 — [PAED-005 title, as seeded]
 PAED-006 — [PAED-006 title, as seeded]
 PAED-007 — [PAED-007 title, as seeded]
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
 PAED-009 — [PAED-009 title, as seeded]
 PAED-010 — [PAED-010 title, as seeded]
 PAED-011 — [PAED-011 title, as seeded]
 PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
 PAED-013 — [PAED-013 title, as seeded]
 PAED-014 — [PAED-014 title, as seeded]
 PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
 Show Clear filters 
## By EPA
 The filters above do not narrow these counts.
 Counts for the 15 EPAs due in 2026 S2 Show Hide 
 How many trainees stand where on each EPA due in 2026 S2. To schedule: not scheduled, due by year end, or revoked. Optional: a partial period, or decided as opportunity allows.
 EPA | Due | Decided | Scheduled | Not decided | To schedule | Missed | Optional | 
 PAED-001 — Providing paediatric emergency care to children | 5 | 2 | 0 | 3 | 0 | 0 | 0 | 
 PAED-002 — [PAED-002 title, as seeded] | 5 | 1 | 0 | 4 | 0 | 0 | 0 | 
 PAED-003 — Providing intensive care to children | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
 PAED-004 — [PAED-004 title, as seeded] | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
 PAED-005 — [PAED-005 title, as seeded] | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
 PAED-006 — [PAED-006 title, as seeded] | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
 PAED-007 — [PAED-007 title, as seeded] | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-009 — [PAED-009 title, as seeded] | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-010 — [PAED-010 title, as seeded] | 5 | 1 | 0 | 4 | 0 | 0 | 0 | 
 PAED-011 — [PAED-011 title, as seeded] | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
 PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 5 | 1 | 0 | 4 | 0 | 0 | 0 | 
 PAED-013 — [PAED-013 title, as seeded] | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-014 — [PAED-014 title, as seeded] | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
 PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
## Decisions , page 1 of 4 
 75 of 75 decisions due in 2026 S2 shown.
 Decisions due in 2026 S2, by trainee then EPA
 EPA | Trainee | Window | Status | Actions | 
 PAED-001 — Providing paediatric emergency care to children | Anele Dlamini | 2026 S2 | Decided STAR #4, issued at review #2 .
 | Decided: nothing to schedule 
 | 
 PAED-002 — [PAED-002 title, as seeded] | Anele Dlamini | 2026 S2 | Decided STAR #5, issued at review #2 .
 | Decided: nothing to schedule 
 | 
 PAED-003 — Providing intensive care to children | Anele Dlamini | 2026 | Not decided Not decided at review #2 . A later sitting for 2026 plans it again.
 | Schedule 
 | 
 PAED-004 — [PAED-004 title, as seeded] | Anele Dlamini | 2026 S2 | Not decided Not decided at review #2 . A later sitting for 2026 S2 plans it again.
 | Schedule 
 | 
 PAED-005 — [PAED-005 title, as seeded] | Anele Dlamini | 2026 S2 | Not decided Not decided at review #2 . A later sitting for 2026 S2 plans it again.
 | Schedule 
 | 
 PAED-006 — [PAED-006 title, as seeded] | Anele Dlamini | 2026 | Not decided Not decided at review #2 . A later sitting for 2026 plans it again.
 | Schedule 
 | 
 PAED-007 — [PAED-007 title, as seeded] | Anele Dlamini | 2026 | Not decided Not decided at review #2 . A later sitting for 2026 plans it again.
 | Schedule 
 | 
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | Anele Dlamini | 2026 | As opportunity allows Decided as opportunity allows: optional, never missed.
 | Schedule 
 | 
 PAED-009 — [PAED-009 title, as seeded] | Anele Dlamini | 2026 | As opportunity allows Decided as opportunity allows: optional, never missed.
 | Schedule 
 | 
 PAED-010 — [PAED-010 title, as seeded] | Anele Dlamini | 2026 S2 | Not decided Not decided at review #2 . A later sitting for 2026 S2 plans it again.
 | Schedule 
 | 
 PAED-011 — [PAED-011 title, as seeded] | Anele Dlamini | 2026 | Not decided Not decided at review #2 . A later sitting for 2026 plans it again.
 | Schedule 
 | 
 PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | Anele Dlamini | 2026 S2 | Not decided Not decided at review #2 . A later sitting for 2026 S2 plans it again.
 | Schedule 
 | 
 PAED-013 — [PAED-013 title, as seeded] | Anele Dlamini | 2026 | As opportunity allows Decided as opportunity allows: optional, never missed.
 | Schedule 
 | 
 PAED-014 — [PAED-014 title, as seeded] | Anele Dlamini | 2026 | Not decided Not decided at review #2 . A later sitting for 2026 plans it again.
 | Schedule 
 | 
 PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | Anele Dlamini | 2026 | Not decided Not decided at review #2 . A later sitting for 2026 plans it again.
 | Schedule 
 | 
 PAED-001 — Providing paediatric emergency care to children | Pieter du Plessis | 2026 S2 | Not decided Not decided at review #3 . A later sitting for 2026 S2 plans it again.
 | Schedule 
 | 
 PAED-002 — [PAED-002 title, as seeded] | Pieter du Plessis | 2026 S2 | Not decided Not decided at review #3 . A later sitting for 2026 S2 plans it again.
 | Schedule 
 | 
 PAED-003 — Providing intensive care to children | Pieter du Plessis | 2026 | Not decided Not decided at review #3 . A later sitting for 2026 plans it again.
 | Schedule 
 | 
 PAED-004 — [PAED-004 title, as seeded] | Pieter du Plessis | 2026 S2 | Not decided Not decided at review #3 . A later sitting for 2026 S2 plans it again.
 | Schedule 
 | 
 PAED-005 — [PAED-005 title, as seeded] | Pieter du Plessis | 2026 S2 | Not decided Not decided at review #3 . A later sitting for 2026 S2 plans it again.
 | Schedule 
 | 
 Showing 1–20 of 75
 Previous Next 
 Decisions per page
 10
 20
 50
 100
=== FRAME: | Revoked: decide it again | | Step 4.37 · Kabelo Sithole · Anele Dlamini's PAED-002; By EPA open: PAED-002 Decided 0, Not decided 4, To schedule 1 (the default filter shows 71 of 75) | 
 [shell] 
# Decisions due
 What the committee must decide, per registrar and per EPA, and where each decision stands.
 The EPAs due for an entrustment decision in 2026 S2 for the 5 trainees you oversee at Kgosi Kgari Teaching Hospital, by each EPA's decision cadence: every semester, or once a year. Missed is read today: the EPA's window has ended with nothing decided or on an open review's agenda.
 Period
 2025 S1 · 1 Jan to 30 Jun 2025
 2025 S2 · 1 Jul to 31 Dec 2025
 2026 S1 · 1 Jan to 30 Jun 2026
 2026 S2 · 1 Jul to 31 Dec 2026
 2027 S1 · 1 Jan to 30 Jun 2027
 2027 S2 · 1 Jul to 31 Dec 2027
 The College's semester 2 ends in November; its window runs to 31 December, so December's work counts. 
 Trainee
 Every trainee
 Anele Dlamini
 Pieter du Plessis
 Nomsa Mahlangu
 Lerato Molefe
 Sipho Ndlovu
 Status
 Outstanding: not yet decided
 Every status
 Not scheduled
 Scheduled
 Decided
 Not decided
 Revoked: re-decide
 Due by year end
 Partial period
 As opportunity allows
 Missed
 EPA
 Every EPA
 PAED-001 — Providing paediatric emergency care to children
 PAED-002 — [PAED-002 title, as seeded]
 PAED-003 — Providing intensive care to children
 PAED-004 — [PAED-004 title, as seeded]
 PAED-005 — [PAED-005 title, as seeded]
 PAED-006 — [PAED-006 title, as seeded]
 PAED-007 — [PAED-007 title, as seeded]
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
 PAED-009 — [PAED-009 title, as seeded]
 PAED-010 — [PAED-010 title, as seeded]
 PAED-011 — [PAED-011 title, as seeded]
 PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
 PAED-013 — [PAED-013 title, as seeded]
 PAED-014 — [PAED-014 title, as seeded]
 PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
 Show Clear filters 
## By EPA
 The filters above do not narrow these counts.
 Counts for the 15 EPAs due in 2026 S2 Show Hide 
 How many trainees stand where on each EPA due in 2026 S2. To schedule: not scheduled, due by year end, or revoked. Optional: a partial period, or decided as opportunity allows.
 EPA | Due | Decided | Scheduled | Not decided | To schedule | Missed | Optional | 
 PAED-001 — Providing paediatric emergency care to children | 5 | 2 | 0 | 3 | 0 | 0 | 0 | 
 PAED-002 — [PAED-002 title, as seeded] | 5 | 0 | 0 | 4 | 1 | 0 | 0 | 
 PAED-003 — Providing intensive care to children | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
 PAED-004 — [PAED-004 title, as seeded] | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
 PAED-005 — [PAED-005 title, as seeded] | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
 PAED-006 — [PAED-006 title, as seeded] | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
 PAED-007 — [PAED-007 title, as seeded] | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-009 — [PAED-009 title, as seeded] | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-010 — [PAED-010 title, as seeded] | 5 | 1 | 0 | 4 | 0 | 0 | 0 | 
 PAED-011 — [PAED-011 title, as seeded] | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
 PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 5 | 1 | 0 | 4 | 0 | 0 | 0 | 
 PAED-013 — [PAED-013 title, as seeded] | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-014 — [PAED-014 title, as seeded] | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
 PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
## Decisions , page 1 of 4 
 75 of 75 decisions due in 2026 S2 shown.
 Decisions due in 2026 S2, by trainee then EPA
 EPA | Trainee | Window | Status | Actions | 
 PAED-001 — Providing paediatric emergency care to children | Anele Dlamini | 2026 S2 | Decided STAR #4, issued at review #2 .
 | Decided: nothing to schedule 
 | 
 PAED-002 — [PAED-002 title, as seeded] | Anele Dlamini | 2026 S2 | Revoked: re-decide STAR #5, issued for 2026 S2, was revoked. Schedule a review to decide it again.
 | Schedule 
 | 
 PAED-003 — Providing intensive care to children | Anele Dlamini | 2026 | Not decided Not decided at review #2 . A later sitting for 2026 plans it again.
 | Schedule 
 | 
 PAED-004 — [PAED-004 title, as seeded] | Anele Dlamini | 2026 S2 | Not decided Not decided at review #2 . A later sitting for 2026 S2 plans it again.
 | Schedule 
 | 
 PAED-005 — [PAED-005 title, as seeded] | Anele Dlamini | 2026 S2 | Not decided Not decided at review #2 . A later sitting for 2026 S2 plans it again.
 | Schedule 
 | 
 PAED-006 — [PAED-006 title, as seeded] | Anele Dlamini | 2026 | Not decided Not decided at review #2 . A later sitting for 2026 plans it again.
 | Schedule 
 | 
 The other 9 rows of this page continue in the same form
 Showing 1–20 of 75
 Previous Next 
 Decisions per page
 10
 20
 50
 100


######## BOARD R2-3-decisions-due-1280-f.dc.html — Decisions due · 1280 · f
Decisions due · 1280 · f 
# Decisions due · 1280px (continued, part f) 
 Route /committee/decisions-due · the schedulers' list; a Committee member is refused.
 Components PageHeader, FilterBar (Show, Clear filters), DataTable (stack-wide), PagerControls (labelled), the .rung-legend fold with .fold-show / .fold-hide, StatePanel, Badge, AccessDenied. NEW the Trainee filter, .trainee-line (C2), .status-cell, .list-section-page, .list-section-count.
 DESIGN.md Decisions due's section; § Pager; § Table system. Round 3 the cadence (M1, M2), By EPA from the board's rows (M3), F1, F2, F6, M8, S1, S4, S5, S7.
=== FRAME: | Nothing due | | Pieter Smit · the form stands, so another period can be read | 
 [shell] 
# Decisions due
 What the committee must decide, per registrar and per EPA, and where each decision stands.
 Period
 2025 S1 · 1 Jan to 30 Jun 2025
 2025 S2 · 1 Jul to 31 Dec 2025
 2026 S1 · 1 Jan to 30 Jun 2026
 2026 S2 · 1 Jul to 31 Dec 2026
 2027 S1 · 1 Jan to 30 Jun 2027
 2027 S2 · 1 Jul to 31 Dec 2027
 The College's semester 2 ends in November; its window runs to 31 December, so December's work counts. 
 Trainee
 Every trainee
 Anele Dlamini
 Pieter du Plessis
 Nomsa Mahlangu
 Lerato Molefe
 Sipho Ndlovu
 Status
 Outstanding: not yet decided
 Every status
 Not scheduled
 Scheduled
 Decided
 Not decided
 Revoked: re-decide
 Due by year end
 Partial period
 As opportunity allows
 Missed
 EPA
 Every EPA
 PAED-001 — Providing paediatric emergency care to children
 PAED-002 — [PAED-002 title, as seeded]
 PAED-003 — Providing intensive care to children
 PAED-004 — [PAED-004 title, as seeded]
 PAED-005 — [PAED-005 title, as seeded]
 PAED-006 — [PAED-006 title, as seeded]
 PAED-007 — [PAED-007 title, as seeded]
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
 PAED-009 — [PAED-009 title, as seeded]
 PAED-010 — [PAED-010 title, as seeded]
 PAED-011 — [PAED-011 title, as seeded]
 PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
 PAED-013 — [PAED-013 title, as seeded]
 PAED-014 — [PAED-014 title, as seeded]
 PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
 Show 
 Nothing due for 2026 S2
 No trainee you oversee at Kgosi Kgari Teaching Hospital owes an entrustment decision in 2026 S2.
=== FRAME: | Loading | | C11, T329 · no Schedule or Open review until the read is in | 
 [shell] 
# Decisions due
 What the committee must decide, per registrar and per EPA, and where each decision stands.
 Loading Decisions due.
 Period
 2025 S1 · 1 Jan to 30 Jun 2025
 2025 S2 · 1 Jul to 31 Dec 2025
 2026 S1 · 1 Jan to 30 Jun 2026
 2026 S2 · 1 Jul to 31 Dec 2026
 2027 S1 · 1 Jan to 30 Jun 2027
 2027 S2 · 1 Jul to 31 Dec 2027
 The College's semester 2 ends in November; its window runs to 31 December, so December's work counts. 
 Trainee
 Every trainee
 Anele Dlamini
 Pieter du Plessis
 Nomsa Mahlangu
 Lerato Molefe
 Sipho Ndlovu
 Status
 Outstanding: not yet decided
 Every status
 Not scheduled
 Scheduled
 Decided
 Not decided
 Revoked: re-decide
 Due by year end
 Partial period
 As opportunity allows
 Missed
 EPA
 Every EPA
 PAED-001 — Providing paediatric emergency care to children
 PAED-002 — [PAED-002 title, as seeded]
 PAED-003 — Providing intensive care to children
 PAED-004 — [PAED-004 title, as seeded]
 PAED-005 — [PAED-005 title, as seeded]
 PAED-006 — [PAED-006 title, as seeded]
 PAED-007 — [PAED-007 title, as seeded]
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
 PAED-009 — [PAED-009 title, as seeded]
 PAED-010 — [PAED-010 title, as seeded]
 PAED-011 — [PAED-011 title, as seeded]
 PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
 PAED-013 — [PAED-013 title, as seeded]
 PAED-014 — [PAED-014 title, as seeded]
 PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
 Show 
## By EPA
## Decisions
=== FRAME: | Load error | | C11, T338 · fixed words for an unexpected failure; a handler's own sentence still shows (S4) | | Focus: the alert, once drawn | 
 [shell] 
# Decisions due
 What the committee must decide, per registrar and per EPA, and where each decision stands.
 Period
 2025 S1 · 1 Jan to 30 Jun 2025
 2025 S2 · 1 Jul to 31 Dec 2025
 2026 S1 · 1 Jan to 30 Jun 2026
 2026 S2 · 1 Jul to 31 Dec 2026
 2027 S1 · 1 Jan to 30 Jun 2027
 2027 S2 · 1 Jul to 31 Dec 2027
 The College's semester 2 ends in November; its window runs to 31 December, so December's work counts. 
 Trainee
 Every trainee
 Anele Dlamini
 Pieter du Plessis
 Nomsa Mahlangu
 Lerato Molefe
 Sipho Ndlovu
 Status
 Outstanding: not yet decided
 Every status
 Not scheduled
 Scheduled
 Decided
 Not decided
 Revoked: re-decide
 Due by year end
 Partial period
 As opportunity allows
 Missed
 EPA
 Every EPA
 PAED-001 — Providing paediatric emergency care to children
 PAED-002 — [PAED-002 title, as seeded]
 PAED-003 — Providing intensive care to children
 PAED-004 — [PAED-004 title, as seeded]
 PAED-005 — [PAED-005 title, as seeded]
 PAED-006 — [PAED-006 title, as seeded]
 PAED-007 — [PAED-007 title, as seeded]
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
 PAED-009 — [PAED-009 title, as seeded]
 PAED-010 — [PAED-010 title, as seeded]
 PAED-011 — [PAED-011 title, as seeded]
 PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
 PAED-013 — [PAED-013 title, as seeded]
 PAED-014 — [PAED-014 title, as seeded]
 PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
 Show 
 Could not load Decisions due. Nothing has changed. Try again, or come back in a few minutes. Try again 
=== FRAME: | Access denied: a committee member | | Step 4.5 · David Naidoo · flow 01's page; nothing lit | 
 [shell] 
# You cannot open this page
 None of your roles (Committee member, Assessor) opens this page.
 If you need it for your work, ask your institution's Wombat administrator.
 Go to Home


######## BOARD R2-3-decisions-due-768-a.dc.html — Decisions due · 768 · a
Decisions due · 768 · a 
# Decisions due · 768px
 Route /committee/decisions-due · the schedulers' list; a Committee member is refused.
 Components PageHeader, FilterBar (Show, Clear filters), DataTable (stack-wide), PagerControls (labelled), the .rung-legend fold with .fold-show / .fold-hide, StatePanel, Badge, AccessDenied. NEW the Trainee filter, .trainee-line (C2), .status-cell, .list-section-page, .list-section-count.
 DESIGN.md Decisions due's section; § Pager; § Table system. Round 3 the cadence (M1, M2), By EPA from the board's rows (M3), F1, F2, F6, M8, S1, S4, S5, S7.
=== FRAME: | Outstanding, every trainee (the default) | | Step 4.1 · Pieter Smit, Coordinator · page 1 of 4, 20 rows: semester rows read “… 2026 S2 has not ended.”, annual rows “… 2026 has not ended.” (M2), optional rows As opportunity allows; no Missed row | 
 [shell] 
# Decisions due
 What the committee must decide, per registrar and per EPA, and where each decision stands.
 The EPAs due for an entrustment decision in 2026 S2 for the 5 trainees you oversee at Kgosi Kgari Teaching Hospital, by each EPA's decision cadence: every semester, or once a year. Missed is read today: the EPA's window has ended with nothing decided or on an open review's agenda.
 Period
 2025 S1 · 1 Jan to 30 Jun 2025
 2025 S2 · 1 Jul to 31 Dec 2025
 2026 S1 · 1 Jan to 30 Jun 2026
 2026 S2 · 1 Jul to 31 Dec 2026
 2027 S1 · 1 Jan to 30 Jun 2027
 2027 S2 · 1 Jul to 31 Dec 2027
 The College's semester 2 ends in November; its window runs to 31 December, so December's work counts. 
 Trainee
 Every trainee
 Anele Dlamini
 Pieter du Plessis
 Nomsa Mahlangu
 Lerato Molefe
 Sipho Ndlovu
 Status
 Outstanding: not yet decided
 Every status
 Not scheduled
 Scheduled
 Decided
 Not decided
 Revoked: re-decide
 Due by year end
 Partial period
 As opportunity allows
 Missed
 EPA
 Every EPA
 PAED-001 — Providing paediatric emergency care to children
 PAED-002 — [PAED-002 title, as seeded]
 PAED-003 — Providing intensive care to children
 PAED-004 — [PAED-004 title, as seeded]
 PAED-005 — [PAED-005 title, as seeded]
 PAED-006 — [PAED-006 title, as seeded]
 PAED-007 — [PAED-007 title, as seeded]
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
 PAED-009 — [PAED-009 title, as seeded]
 PAED-010 — [PAED-010 title, as seeded]
 PAED-011 — [PAED-011 title, as seeded]
 PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
 PAED-013 — [PAED-013 title, as seeded]
 PAED-014 — [PAED-014 title, as seeded]
 PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
 Show 
## By EPA
 The filters above do not narrow these counts.
 Counts for the 15 EPAs due in 2026 S2 Show Hide 
 How many trainees stand where on each EPA due in 2026 S2. To schedule: not scheduled, due by year end, or revoked. Optional: a partial period, or decided as opportunity allows.
 EPA | Due | Decided | Scheduled | Not decided | To schedule | Missed | Optional | 
 PAED-001 — Providing paediatric emergency care to children | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-002 — [PAED-002 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-003 — Providing intensive care to children | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-004 — [PAED-004 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-005 — [PAED-005 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-006 — [PAED-006 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-007 — [PAED-007 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-009 — [PAED-009 title, as seeded] | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-010 — [PAED-010 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-011 — [PAED-011 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-013 — [PAED-013 title, as seeded] | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-014 — [PAED-014 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
## Decisions , page 1 of 4 
 75 of 75 decisions due in 2026 S2 shown.
 Decisions due in 2026 S2, by trainee then EPA
 EPA | Trainee | Window | Status | Actions | 
 PAED-001 — Providing paediatric emergency care to children | Anele Dlamini | 2026 S2 | Not scheduled No open review holds it, and 2026 S2 has not ended.
 | Schedule 
 | 
 PAED-002 — [PAED-002 title, as seeded] | Anele Dlamini | 2026 S2 | Not scheduled No open review holds it, and 2026 S2 has not ended.
 | Schedule 
 | 
 PAED-003 — Providing intensive care to children | Anele Dlamini | 2026 | Not scheduled No open review holds it, and 2026 has not ended.
 | Schedule 
 | 
 PAED-004 — [PAED-004 title, as seeded] | Anele Dlamini | 2026 S2 | Not scheduled No open review holds it, and 2026 S2 has not ended.
 | Schedule 
 | 
 PAED-005 — [PAED-005 title, as seeded] | Anele Dlamini | 2026 S2 | Not scheduled No open review holds it, and 2026 S2 has not ended.
 | Schedule 
 | 
 PAED-006 — [PAED-006 title, as seeded] | Anele Dlamini | 2026 | Not scheduled No open review holds it, and 2026 has not ended.
 | Schedule 
 | 
 PAED-007 — [PAED-007 title, as seeded] | Anele Dlamini | 2026 | Not scheduled No open review holds it, and 2026 has not ended.
 | Schedule 
 | 
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | Anele Dlamini | 2026 | As opportunity allows Decided as opportunity allows: optional, never missed.
 | Schedule 
 | 
 PAED-009 — [PAED-009 title, as seeded] | Anele Dlamini | 2026 | As opportunity allows Decided as opportunity allows: optional, never missed.
 | Schedule 
 | 
 PAED-010 — [PAED-010 title, as seeded] | Anele Dlamini | 2026 S2 | Not scheduled No open review holds it, and 2026 S2 has not ended.
 | Schedule 
 | 
 PAED-011 — [PAED-011 title, as seeded] | Anele Dlamini | 2026 | Not scheduled No open review holds it, and 2026 has not ended.
 | Schedule 
 | 
 PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | Anele Dlamini | 2026 S2 | Not scheduled No open review holds it, and 2026 S2 has not ended.
 | Schedule 
 | 
 PAED-013 — [PAED-013 title, as seeded] | Anele Dlamini | 2026 | As opportunity allows Decided as opportunity allows: optional, never missed.
 | Schedule 
 | 
 PAED-014 — [PAED-014 title, as seeded] | Anele Dlamini | 2026 | Not scheduled No open review holds it, and 2026 has not ended.
 | Schedule 
 | 
 PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | Anele Dlamini | 2026 | Not scheduled No open review holds it, and 2026 has not ended.
 | Schedule 
 | 
 PAED-001 — Providing paediatric emergency care to children | Pieter du Plessis | 2026 S2 | Not scheduled No open review holds it, and 2026 S2 has not ended.
 | Schedule 
 | 
 PAED-002 — [PAED-002 title, as seeded] | Pieter du Plessis | 2026 S2 | Not scheduled No open review holds it, and 2026 S2 has not ended.
 | Schedule 
 | 
 PAED-003 — Providing intensive care to children | Pieter du Plessis | 2026 | Not scheduled No open review holds it, and 2026 has not ended.
 | Schedule 
 | 
 PAED-004 — [PAED-004 title, as seeded] | Pieter du Plessis | 2026 S2 | Not scheduled No open review holds it, and 2026 S2 has not ended.
 | Schedule 
 | 
 PAED-005 — [PAED-005 title, as seeded] | Pieter du Plessis | 2026 S2 | Not scheduled No open review holds it, and 2026 S2 has not ended.
 | Schedule 
 | 
 Showing 1–20 of 75
 Previous Next 
 Decisions per page
 10
 20
 50
 100


######## BOARD R2-3-decisions-due-768-b.dc.html — Decisions due · 768 · b
Decisions due · 768 · b 
# Decisions due · 768px (continued, part b) 
 Route /committee/decisions-due · the schedulers' list; a Committee member is refused.
 Components PageHeader, FilterBar (Show, Clear filters), DataTable (stack-wide), PagerControls (labelled), the .rung-legend fold with .fold-show / .fold-hide, StatePanel, Badge, AccessDenied. NEW the Trainee filter, .trainee-line (C2), .status-cell, .list-section-page, .list-section-count.
 DESIGN.md Decisions due's section; § Pager; § Table system. Round 3 the cadence (M1, M2), By EPA from the board's rows (M3), F1, F2, F6, M8, S1, S4, S5, S7.
=== FRAME: | After the sitting: Decided, Not decided, optional | | Step 4.34 · Nolwazi Mbatha · Every status (the default filter shows 70 of 75); By EPA open: PAED-001 Due 5, Decided 2, Not decided 3 | 
 [shell] 
# Decisions due
 What the committee must decide, per registrar and per EPA, and where each decision stands.
 The EPAs due for an entrustment decision in 2026 S2 for the 5 trainees you oversee at Kgosi Kgari Teaching Hospital, by each EPA's decision cadence: every semester, or once a year. Missed is read today: the EPA's window has ended with nothing decided or on an open review's agenda.
 Period
 2025 S1 · 1 Jan to 30 Jun 2025
 2025 S2 · 1 Jul to 31 Dec 2025
 2026 S1 · 1 Jan to 30 Jun 2026
 2026 S2 · 1 Jul to 31 Dec 2026
 2027 S1 · 1 Jan to 30 Jun 2027
 2027 S2 · 1 Jul to 31 Dec 2027
 The College's semester 2 ends in November; its window runs to 31 December, so December's work counts. 
 Trainee
 Every trainee
 Anele Dlamini
 Pieter du Plessis
 Nomsa Mahlangu
 Lerato Molefe
 Sipho Ndlovu
 Status
 Outstanding: not yet decided
 Every status
 Not scheduled
 Scheduled
 Decided
 Not decided
 Revoked: re-decide
 Due by year end
 Partial period
 As opportunity allows
 Missed
 EPA
 Every EPA
 PAED-001 — Providing paediatric emergency care to children
 PAED-002 — [PAED-002 title, as seeded]
 PAED-003 — Providing intensive care to children
 PAED-004 — [PAED-004 title, as seeded]
 PAED-005 — [PAED-005 title, as seeded]
 PAED-006 — [PAED-006 title, as seeded]
 PAED-007 — [PAED-007 title, as seeded]
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
 PAED-009 — [PAED-009 title, as seeded]
 PAED-010 — [PAED-010 title, as seeded]
 PAED-011 — [PAED-011 title, as seeded]
 PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
 PAED-013 — [PAED-013 title, as seeded]
 PAED-014 — [PAED-014 title, as seeded]
 PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
 Show Clear filters 
## By EPA
 The filters above do not narrow these counts.
 Counts for the 15 EPAs due in 2026 S2 Show Hide 
 How many trainees stand where on each EPA due in 2026 S2. To schedule: not scheduled, due by year end, or revoked. Optional: a partial period, or decided as opportunity allows.
 EPA | Due | Decided | Scheduled | Not decided | To schedule | Missed | Optional | 
 PAED-001 — Providing paediatric emergency care to children | 5 | 2 | 0 | 3 | 0 | 0 | 0 | 
 PAED-002 — [PAED-002 title, as seeded] | 5 | 1 | 0 | 4 | 0 | 0 | 0 | 
 PAED-003 — Providing intensive care to children | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
 PAED-004 — [PAED-004 title, as seeded] | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
 PAED-005 — [PAED-005 title, as seeded] | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
 PAED-006 — [PAED-006 title, as seeded] | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
 PAED-007 — [PAED-007 title, as seeded] | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-009 — [PAED-009 title, as seeded] | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-010 — [PAED-010 title, as seeded] | 5 | 1 | 0 | 4 | 0 | 0 | 0 | 
 PAED-011 — [PAED-011 title, as seeded] | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
 PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 5 | 1 | 0 | 4 | 0 | 0 | 0 | 
 PAED-013 — [PAED-013 title, as seeded] | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-014 — [PAED-014 title, as seeded] | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
 PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
## Decisions , page 1 of 4 
 75 of 75 decisions due in 2026 S2 shown.
 Decisions due in 2026 S2, by trainee then EPA
 EPA | Trainee | Window | Status | Actions | 
 PAED-001 — Providing paediatric emergency care to children | Anele Dlamini | 2026 S2 | Decided STAR #4, issued at review #2 .
 | Decided: nothing to schedule 
 | 
 PAED-002 — [PAED-002 title, as seeded] | Anele Dlamini | 2026 S2 | Decided STAR #5, issued at review #2 .
 | Decided: nothing to schedule 
 | 
 PAED-003 — Providing intensive care to children | Anele Dlamini | 2026 | Not decided Not decided at review #2 . A later sitting for 2026 plans it again.
 | Schedule 
 | 
 PAED-004 — [PAED-004 title, as seeded] | Anele Dlamini | 2026 S2 | Not decided Not decided at review #2 . A later sitting for 2026 S2 plans it again.
 | Schedule 
 | 
 PAED-005 — [PAED-005 title, as seeded] | Anele Dlamini | 2026 S2 | Not decided Not decided at review #2 . A later sitting for 2026 S2 plans it again.
 | Schedule 
 | 
 PAED-006 — [PAED-006 title, as seeded] | Anele Dlamini | 2026 | Not decided Not decided at review #2 . A later sitting for 2026 plans it again.
 | Schedule 
 | 
 PAED-007 — [PAED-007 title, as seeded] | Anele Dlamini | 2026 | Not decided Not decided at review #2 . A later sitting for 2026 plans it again.
 | Schedule 
 | 
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | Anele Dlamini | 2026 | As opportunity allows Decided as opportunity allows: optional, never missed.
 | Schedule 
 | 
 PAED-009 — [PAED-009 title, as seeded] | Anele Dlamini | 2026 | As opportunity allows Decided as opportunity allows: optional, never missed.
 | Schedule 
 | 
 PAED-010 — [PAED-010 title, as seeded] | Anele Dlamini | 2026 S2 | Not decided Not decided at review #2 . A later sitting for 2026 S2 plans it again.
 | Schedule 
 | 
 PAED-011 — [PAED-011 title, as seeded] | Anele Dlamini | 2026 | Not decided Not decided at review #2 . A later sitting for 2026 plans it again.
 | Schedule 
 | 
 PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | Anele Dlamini | 2026 S2 | Not decided Not decided at review #2 . A later sitting for 2026 S2 plans it again.
 | Schedule 
 | 
 PAED-013 — [PAED-013 title, as seeded] | Anele Dlamini | 2026 | As opportunity allows Decided as opportunity allows: optional, never missed.
 | Schedule 
 | 
 PAED-014 — [PAED-014 title, as seeded] | Anele Dlamini | 2026 | Not decided Not decided at review #2 . A later sitting for 2026 plans it again.
 | Schedule 
 | 
 PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | Anele Dlamini | 2026 | Not decided Not decided at review #2 . A later sitting for 2026 plans it again.
 | Schedule 
 | 
 PAED-001 — Providing paediatric emergency care to children | Pieter du Plessis | 2026 S2 | Not decided Not decided at review #3 . A later sitting for 2026 S2 plans it again.
 | Schedule 
 | 
 PAED-002 — [PAED-002 title, as seeded] | Pieter du Plessis | 2026 S2 | Not decided Not decided at review #3 . A later sitting for 2026 S2 plans it again.
 | Schedule 
 | 
 PAED-003 — Providing intensive care to children | Pieter du Plessis | 2026 | Not decided Not decided at review #3 . A later sitting for 2026 plans it again.
 | Schedule 
 | 
 PAED-004 — [PAED-004 title, as seeded] | Pieter du Plessis | 2026 S2 | Not decided Not decided at review #3 . A later sitting for 2026 S2 plans it again.
 | Schedule 
 | 
 PAED-005 — [PAED-005 title, as seeded] | Pieter du Plessis | 2026 S2 | Not decided Not decided at review #3 . A later sitting for 2026 S2 plans it again.
 | Schedule 
 | 
 Showing 1–20 of 75
 Previous Next 
 Decisions per page
 10
 20
 50
 100


######## BOARD R2-3-decisions-due-768-c.dc.html — Decisions due · 768 · c
Decisions due · 768 · c 
# Decisions due · 768px (continued, part c) 
 Route /committee/decisions-due · the schedulers' list; a Committee member is refused.
 Components PageHeader, FilterBar (Show, Clear filters), DataTable (stack-wide), PagerControls (labelled), the .rung-legend fold with .fold-show / .fold-hide, StatePanel, Badge, AccessDenied. NEW the Trainee filter, .trainee-line (C2), .status-cell, .list-section-page, .list-section-count.
 DESIGN.md Decisions due's section; § Pager; § Table system. Round 3 the cadence (M1, M2), By EPA from the board's rows (M3), F1, F2, F6, M8, S1, S4, S5, S7.
=== FRAME: | One trainee: no open review holds the period (C2) | | Step 5.1 · Pieter Smit · Trainee Lerato Molefe, every status | | Focus: the Decisions heading, after Show | 
 [shell] 
# Decisions due
 What the committee must decide, per registrar and per EPA, and where each decision stands.
 The EPAs due for an entrustment decision in 2026 S2 for the 5 trainees you oversee at Kgosi Kgari Teaching Hospital, by each EPA's decision cadence: every semester, or once a year. Missed is read today: the EPA's window has ended with nothing decided or on an open review's agenda.
 Period
 2025 S1 · 1 Jan to 30 Jun 2025
 2025 S2 · 1 Jul to 31 Dec 2025
 2026 S1 · 1 Jan to 30 Jun 2026
 2026 S2 · 1 Jul to 31 Dec 2026
 2027 S1 · 1 Jan to 30 Jun 2027
 2027 S2 · 1 Jul to 31 Dec 2027
 The College's semester 2 ends in November; its window runs to 31 December, so December's work counts. 
 Trainee
 Every trainee
 Anele Dlamini
 Pieter du Plessis
 Nomsa Mahlangu
 Lerato Molefe
 Sipho Ndlovu
 Status
 Outstanding: not yet decided
 Every status
 Not scheduled
 Scheduled
 Decided
 Not decided
 Revoked: re-decide
 Due by year end
 Partial period
 As opportunity allows
 Missed
 EPA
 Every EPA
 PAED-001 — Providing paediatric emergency care to children
 PAED-002 — [PAED-002 title, as seeded]
 PAED-003 — Providing intensive care to children
 PAED-004 — [PAED-004 title, as seeded]
 PAED-005 — [PAED-005 title, as seeded]
 PAED-006 — [PAED-006 title, as seeded]
 PAED-007 — [PAED-007 title, as seeded]
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
 PAED-009 — [PAED-009 title, as seeded]
 PAED-010 — [PAED-010 title, as seeded]
 PAED-011 — [PAED-011 title, as seeded]
 PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
 PAED-013 — [PAED-013 title, as seeded]
 PAED-014 — [PAED-014 title, as seeded]
 PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
 Show Clear filters 
## By EPA
 The filters above do not narrow these counts.
 Counts for the 15 EPAs due in 2026 S2 Show Hide 
 How many trainees stand where on each EPA due in 2026 S2. To schedule: not scheduled, due by year end, or revoked. Optional: a partial period, or decided as opportunity allows.
 EPA | Due | Decided | Scheduled | Not decided | To schedule | Missed | Optional | 
 PAED-001 — Providing paediatric emergency care to children | 5 | 2 | 0 | 3 | 0 | 0 | 0 | 
 PAED-002 — [PAED-002 title, as seeded] | 5 | 1 | 0 | 4 | 0 | 0 | 0 | 
 PAED-003 — Providing intensive care to children | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
 PAED-004 — [PAED-004 title, as seeded] | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
 PAED-005 — [PAED-005 title, as seeded] | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
 PAED-006 — [PAED-006 title, as seeded] | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
 PAED-007 — [PAED-007 title, as seeded] | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-009 — [PAED-009 title, as seeded] | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-010 — [PAED-010 title, as seeded] | 5 | 1 | 0 | 4 | 0 | 0 | 0 | 
 PAED-011 — [PAED-011 title, as seeded] | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
 PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 5 | 1 | 0 | 4 | 0 | 0 | 0 | 
 PAED-013 — [PAED-013 title, as seeded] | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-014 — [PAED-014 title, as seeded] | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
 PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
## Decisions 
 15 of 75 decisions due in 2026 S2 shown: Lerato Molefe, every status.
 No open review holds 2026 S2.
 Schedule a review 
 Lerato Molefe's decisions due in 2026 S2, every status, by EPA
 EPA | Window | Status | Actions | 
 PAED-001 — Providing paediatric emergency care to children | 2026 S2 | Decided STAR #1, issued at review #1 .
 | Decided: nothing to schedule 
 | 
 PAED-002 — [PAED-002 title, as seeded] | 2026 S2 | Not decided Not decided at review #1 . A later sitting for 2026 S2 plans it again.
 | Schedule 
 | 
 PAED-003 — Providing intensive care to children | 2026 | Not decided Not decided at review #1 . A later sitting for 2026 plans it again.
 | Schedule 
 | 
 PAED-004 — [PAED-004 title, as seeded] | 2026 S2 | Not decided Not decided at review #1 . A later sitting for 2026 S2 plans it again.
 | Schedule 
 | 
 PAED-005 — [PAED-005 title, as seeded] | 2026 S2 | Not decided Not decided at review #1 . A later sitting for 2026 S2 plans it again.
 | Schedule 
 | 
 PAED-006 — [PAED-006 title, as seeded] | 2026 | Not decided Not decided at review #1 . A later sitting for 2026 plans it again.
 | Schedule 
 | 
 PAED-007 — [PAED-007 title, as seeded] | 2026 | Not decided Not decided at review #1 . A later sitting for 2026 plans it again.
 | Schedule 
 | 
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 2026 | As opportunity allows Decided as opportunity allows: optional, never missed.
 | Schedule 
 | 
 PAED-009 — [PAED-009 title, as seeded] | 2026 | As opportunity allows Decided as opportunity allows: optional, never missed.
 | Schedule 
 | 
 PAED-010 — [PAED-010 title, as seeded] | 2026 S2 | Decided STAR #2, issued at review #1 .
 | Decided: nothing to schedule 
 | 
 PAED-011 — [PAED-011 title, as seeded] | 2026 | Not decided Not decided at review #1 . A later sitting for 2026 plans it again.
 | Schedule 
 | 
 PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 2026 S2 | Decided STAR #3, issued at review #1 .
 | Decided: nothing to schedule 
 | 
 PAED-013 — [PAED-013 title, as seeded] | 2026 | As opportunity allows Decided as opportunity allows: optional, never missed.
 | Schedule 
 | 
 PAED-014 — [PAED-014 title, as seeded] | 2026 | Not decided Not decided at review #1 . A later sitting for 2026 plans it again.
 | Schedule 
 | 
 PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 2026 | Not decided Not decided at review #1 . A later sitting for 2026 plans it again.
 | Schedule 
 | 
 Showing 1–15 of 15
 Previous Next 
 Decisions per page
 10
 20
 50
 100


######## BOARD R2-3-decisions-due-390-a.dc.html — Decisions due · 390 · a
Decisions due · 390 · a 
# Decisions due · 390px
 Route /committee/decisions-due · the schedulers' list; a Committee member is refused.
 Components PageHeader, FilterBar (Show, Clear filters), DataTable (stack-wide), PagerControls (labelled), the .rung-legend fold with .fold-show / .fold-hide, StatePanel, Badge, AccessDenied. NEW the Trainee filter, .trainee-line (C2), .status-cell, .list-section-page, .list-section-count.
 DESIGN.md Decisions due's section; § Pager; § Table system. Round 3 the cadence (M1, M2), By EPA from the board's rows (M3), F1, F2, F6, M8, S1, S4, S5, S7.
=== FRAME: | An Administrator choosing an institution | | Administrator · no institution yet: no By EPA, no list | 
 [shell] 
# Decisions due
 What the committee must decide, per registrar and per EPA, and where each decision stands.
 An administrator belongs to no institution, so the decisions due are shown for the one chosen above.
 Institution
 Select institution
 Kgosi Kgari Teaching Hospital
 Period
 2025 S1 · 1 Jan to 30 Jun 2025
 2025 S2 · 1 Jul to 31 Dec 2025
 2026 S1 · 1 Jan to 30 Jun 2026
 2026 S2 · 1 Jul to 31 Dec 2026
 2027 S1 · 1 Jan to 30 Jun 2027
 2027 S2 · 1 Jul to 31 Dec 2027
 The College's semester 2 ends in November; its window runs to 31 December, so December's work counts. 
 Trainee
 Every trainee
 Anele Dlamini
 Pieter du Plessis
 Nomsa Mahlangu
 Lerato Molefe
 Sipho Ndlovu
 Status
 Outstanding: not yet decided
 Every status
 Not scheduled
 Scheduled
 Decided
 Not decided
 Revoked: re-decide
 Due by year end
 Partial period
 As opportunity allows
 Missed
 EPA
 Every EPA
 PAED-001 — Providing paediatric emergency care to children
 PAED-002 — [PAED-002 title, as seeded]
 PAED-003 — Providing intensive care to children
 PAED-004 — [PAED-004 title, as seeded]
 PAED-005 — [PAED-005 title, as seeded]
 PAED-006 — [PAED-006 title, as seeded]
 PAED-007 — [PAED-007 title, as seeded]
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
 PAED-009 — [PAED-009 title, as seeded]
 PAED-010 — [PAED-010 title, as seeded]
 PAED-011 — [PAED-011 title, as seeded]
 PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
 PAED-013 — [PAED-013 title, as seeded]
 PAED-014 — [PAED-014 title, as seeded]
 PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
 Show 
 Choose an institution
 Choose an institution above, then Show, to read what its committees must decide.
=== FRAME: | Outstanding, every trainee (the default) | | Step 4.1 · Pieter Smit, Coordinator · page 1 of 4, 20 rows: semester rows read “… 2026 S2 has not ended.”, annual rows “… 2026 has not ended.” (M2), optional rows As opportunity allows; no Missed row | 
 [shell] 
# Decisions due
 What the committee must decide, per registrar and per EPA, and where each decision stands.
 The EPAs due for an entrustment decision in 2026 S2 for the 5 trainees you oversee at Kgosi Kgari Teaching Hospital, by each EPA's decision cadence: every semester, or once a year. Missed is read today: the EPA's window has ended with nothing decided or on an open review's agenda.
 Period
 2025 S1 · 1 Jan to 30 Jun 2025
 2025 S2 · 1 Jul to 31 Dec 2025
 2026 S1 · 1 Jan to 30 Jun 2026
 2026 S2 · 1 Jul to 31 Dec 2026
 2027 S1 · 1 Jan to 30 Jun 2027
 2027 S2 · 1 Jul to 31 Dec 2027
 The College's semester 2 ends in November; its window runs to 31 December, so December's work counts. 
 Trainee
 Every trainee
 Anele Dlamini
 Pieter du Plessis
 Nomsa Mahlangu
 Lerato Molefe
 Sipho Ndlovu
 Status
 Outstanding: not yet decided
 Every status
 Not scheduled
 Scheduled
 Decided
 Not decided
 Revoked: re-decide
 Due by year end
 Partial period
 As opportunity allows
 Missed
 EPA
 Every EPA
 PAED-001 — Providing paediatric emergency care to children
 PAED-002 — [PAED-002 title, as seeded]
 PAED-003 — Providing intensive care to children
 PAED-004 — [PAED-004 title, as seeded]
 PAED-005 — [PAED-005 title, as seeded]
 PAED-006 — [PAED-006 title, as seeded]
 PAED-007 — [PAED-007 title, as seeded]
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
 PAED-009 — [PAED-009 title, as seeded]
 PAED-010 — [PAED-010 title, as seeded]
 PAED-011 — [PAED-011 title, as seeded]
 PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
 PAED-013 — [PAED-013 title, as seeded]
 PAED-014 — [PAED-014 title, as seeded]
 PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
 Show 
## By EPA
 The filters above do not narrow these counts.
 Counts for the 15 EPAs due in 2026 S2 Show Hide 
 How many trainees stand where on each EPA due in 2026 S2. To schedule: not scheduled, due by year end, or revoked. Optional: a partial period, or decided as opportunity allows.
 EPA | Due | Decided | Scheduled | Not decided | To schedule | Missed | Optional | 
 PAED-001 — Providing paediatric emergency care to children | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-002 — [PAED-002 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-003 — Providing intensive care to children | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-004 — [PAED-004 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-005 — [PAED-005 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-006 — [PAED-006 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-007 — [PAED-007 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-009 — [PAED-009 title, as seeded] | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-010 — [PAED-010 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-011 — [PAED-011 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-013 — [PAED-013 title, as seeded] | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-014 — [PAED-014 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
## Decisions , page 1 of 4 
 75 of 75 decisions due in 2026 S2 shown.
 Decisions due in 2026 S2, by trainee then EPA
 EPA | Trainee | Window | Status | Actions | 
 PAED-001 — Providing paediatric emergency care to children | Anele Dlamini | 2026 S2 | Not scheduled No open review holds it, and 2026 S2 has not ended.
 | Schedule 
 | 
 PAED-002 — [PAED-002 title, as seeded] | Anele Dlamini | 2026 S2 | Not scheduled No open review holds it, and 2026 S2 has not ended.
 | Schedule 
 | 
 PAED-003 — Providing intensive care to children | Anele Dlamini | 2026 | Not scheduled No open review holds it, and 2026 has not ended.
 | Schedule 
 | 
 PAED-004 — [PAED-004 title, as seeded] | Anele Dlamini | 2026 S2 | Not scheduled No open review holds it, and 2026 S2 has not ended.
 | Schedule 
 | 
 PAED-005 — [PAED-005 title, as seeded] | Anele Dlamini | 2026 S2 | Not scheduled No open review holds it, and 2026 S2 has not ended.
 | Schedule 
 | 
 PAED-006 — [PAED-006 title, as seeded] | Anele Dlamini | 2026 | Not scheduled No open review holds it, and 2026 has not ended.
 | Schedule 
 | 
 The other 14 rows of this page continue in the same form
 Showing 1–20 of 75
 Previous Next 
 Decisions per page
 10
 20
 50
 100


######## BOARD R2-3-decisions-due-390-b.dc.html — Decisions due · 390 · b
Decisions due · 390 · b 
# Decisions due · 390px (continued, part b) 
 Route /committee/decisions-due · the schedulers' list; a Committee member is refused.
 Components PageHeader, FilterBar (Show, Clear filters), DataTable (stack-wide), PagerControls (labelled), the .rung-legend fold with .fold-show / .fold-hide, StatePanel, Badge, AccessDenied. NEW the Trainee filter, .trainee-line (C2), .status-cell, .list-section-page, .list-section-count.
 DESIGN.md Decisions due's section; § Pager; § Table system. Round 3 the cadence (M1, M2), By EPA from the board's rows (M3), F1, F2, F6, M8, S1, S4, S5, S7.
=== FRAME: | By EPA shown | | Step 4.1 · Pieter Smit · twelve EPAs To schedule 5, three Optional 5; the fold's summary 44px at every width (S6); stacked at 900px and below (M8) | | Focus: stays on the fold's summary | 
 [shell] 
# Decisions due
 What the committee must decide, per registrar and per EPA, and where each decision stands.
 The EPAs due for an entrustment decision in 2026 S2 for the 5 trainees you oversee at Kgosi Kgari Teaching Hospital, by each EPA's decision cadence: every semester, or once a year. Missed is read today: the EPA's window has ended with nothing decided or on an open review's agenda.
 Period
 2025 S1 · 1 Jan to 30 Jun 2025
 2025 S2 · 1 Jul to 31 Dec 2025
 2026 S1 · 1 Jan to 30 Jun 2026
 2026 S2 · 1 Jul to 31 Dec 2026
 2027 S1 · 1 Jan to 30 Jun 2027
 2027 S2 · 1 Jul to 31 Dec 2027
 The College's semester 2 ends in November; its window runs to 31 December, so December's work counts. 
 Trainee
 Every trainee
 Anele Dlamini
 Pieter du Plessis
 Nomsa Mahlangu
 Lerato Molefe
 Sipho Ndlovu
 Status
 Outstanding: not yet decided
 Every status
 Not scheduled
 Scheduled
 Decided
 Not decided
 Revoked: re-decide
 Due by year end
 Partial period
 As opportunity allows
 Missed
 EPA
 Every EPA
 PAED-001 — Providing paediatric emergency care to children
 PAED-002 — [PAED-002 title, as seeded]
 PAED-003 — Providing intensive care to children
 PAED-004 — [PAED-004 title, as seeded]
 PAED-005 — [PAED-005 title, as seeded]
 PAED-006 — [PAED-006 title, as seeded]
 PAED-007 — [PAED-007 title, as seeded]
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
 PAED-009 — [PAED-009 title, as seeded]
 PAED-010 — [PAED-010 title, as seeded]
 PAED-011 — [PAED-011 title, as seeded]
 PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
 PAED-013 — [PAED-013 title, as seeded]
 PAED-014 — [PAED-014 title, as seeded]
 PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
 Show 
## By EPA
 The filters above do not narrow these counts.
 Counts for the 15 EPAs due in 2026 S2 Show Hide 
 How many trainees stand where on each EPA due in 2026 S2. To schedule: not scheduled, due by year end, or revoked. Optional: a partial period, or decided as opportunity allows.
 EPA | Due | Decided | Scheduled | Not decided | To schedule | Missed | Optional | 
 PAED-001 — Providing paediatric emergency care to children | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-002 — [PAED-002 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-003 — Providing intensive care to children | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-004 — [PAED-004 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-005 — [PAED-005 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-006 — [PAED-006 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-007 — [PAED-007 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-009 — [PAED-009 title, as seeded] | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-010 — [PAED-010 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-011 — [PAED-011 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-013 — [PAED-013 title, as seeded] | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-014 — [PAED-014 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 5 | 0 | 0 | 0 | 5 | 0 | 0 |


######## BOARD R2-3-decisions-due-390-c.dc.html — Decisions due · 390 · c
Decisions due · 390 · c 
# Decisions due · 390px (continued, part c) 
 Route /committee/decisions-due · the schedulers' list; a Committee member is refused.
 Components PageHeader, FilterBar (Show, Clear filters), DataTable (stack-wide), PagerControls (labelled), the .rung-legend fold with .fold-show / .fold-hide, StatePanel, Badge, AccessDenied. NEW the Trainee filter, .trainee-line (C2), .status-cell, .list-section-page, .list-section-count.
 DESIGN.md Decisions due's section; § Pager; § Table system. Round 3 the cadence (M1, M2), By EPA from the board's rows (M3), F1, F2, F6, M8, S1, S4, S5, S7.
=== FRAME: | After Next: page 2 | | Step 4.1 · Pieter Smit · C9, T264 | | Focus: the Decisions heading, which says the page | 
 [shell] 
# Decisions due
 What the committee must decide, per registrar and per EPA, and where each decision stands.
 The EPAs due for an entrustment decision in 2026 S2 for the 5 trainees you oversee at Kgosi Kgari Teaching Hospital, by each EPA's decision cadence: every semester, or once a year. Missed is read today: the EPA's window has ended with nothing decided or on an open review's agenda.
 Period
 2025 S1 · 1 Jan to 30 Jun 2025
 2025 S2 · 1 Jul to 31 Dec 2025
 2026 S1 · 1 Jan to 30 Jun 2026
 2026 S2 · 1 Jul to 31 Dec 2026
 2027 S1 · 1 Jan to 30 Jun 2027
 2027 S2 · 1 Jul to 31 Dec 2027
 The College's semester 2 ends in November; its window runs to 31 December, so December's work counts. 
 Trainee
 Every trainee
 Anele Dlamini
 Pieter du Plessis
 Nomsa Mahlangu
 Lerato Molefe
 Sipho Ndlovu
 Status
 Outstanding: not yet decided
 Every status
 Not scheduled
 Scheduled
 Decided
 Not decided
 Revoked: re-decide
 Due by year end
 Partial period
 As opportunity allows
 Missed
 EPA
 Every EPA
 PAED-001 — Providing paediatric emergency care to children
 PAED-002 — [PAED-002 title, as seeded]
 PAED-003 — Providing intensive care to children
 PAED-004 — [PAED-004 title, as seeded]
 PAED-005 — [PAED-005 title, as seeded]
 PAED-006 — [PAED-006 title, as seeded]
 PAED-007 — [PAED-007 title, as seeded]
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
 PAED-009 — [PAED-009 title, as seeded]
 PAED-010 — [PAED-010 title, as seeded]
 PAED-011 — [PAED-011 title, as seeded]
 PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
 PAED-013 — [PAED-013 title, as seeded]
 PAED-014 — [PAED-014 title, as seeded]
 PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
 Show 
## By EPA
 The filters above do not narrow these counts.
 Counts for the 15 EPAs due in 2026 S2 Show Hide 
 How many trainees stand where on each EPA due in 2026 S2. To schedule: not scheduled, due by year end, or revoked. Optional: a partial period, or decided as opportunity allows.
 EPA | Due | Decided | Scheduled | Not decided | To schedule | Missed | Optional | 
 PAED-001 — Providing paediatric emergency care to children | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-002 — [PAED-002 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-003 — Providing intensive care to children | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-004 — [PAED-004 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-005 — [PAED-005 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-006 — [PAED-006 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-007 — [PAED-007 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-009 — [PAED-009 title, as seeded] | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-010 — [PAED-010 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-011 — [PAED-011 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-013 — [PAED-013 title, as seeded] | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-014 — [PAED-014 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
## Decisions , page 2 of 4 
 75 of 75 decisions due in 2026 S2 shown.
 Decisions due in 2026 S2, by trainee then EPA
 EPA | Trainee | Window | Status | Actions | 
 PAED-006 — [PAED-006 title, as seeded] | Pieter du Plessis | 2026 | Not scheduled No open review holds it, and 2026 has not ended.
 | Schedule 
 | 
 PAED-007 — [PAED-007 title, as seeded] | Pieter du Plessis | 2026 | Not scheduled No open review holds it, and 2026 has not ended.
 | Schedule 
 | 
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | Pieter du Plessis | 2026 | As opportunity allows Decided as opportunity allows: optional, never missed.
 | Schedule 
 | 
 PAED-009 — [PAED-009 title, as seeded] | Pieter du Plessis | 2026 | As opportunity allows Decided as opportunity allows: optional, never missed.
 | Schedule 
 | 
 The other 16 rows of this page continue in the same form
 Showing 21–40 of 75
 Previous Next 
 Decisions per page
 10
 20
 50
 100
=== FRAME: | One trainee: no open review holds the period (C2) | | Step 5.1 · Pieter Smit · Trainee Lerato Molefe, every status | | Focus: the Decisions heading, after Show | 
 [shell] 
# Decisions due
 What the committee must decide, per registrar and per EPA, and where each decision stands.
 The EPAs due for an entrustment decision in 2026 S2 for the 5 trainees you oversee at Kgosi Kgari Teaching Hospital, by each EPA's decision cadence: every semester, or once a year. Missed is read today: the EPA's window has ended with nothing decided or on an open review's agenda.
 Period
 2025 S1 · 1 Jan to 30 Jun 2025
 2025 S2 · 1 Jul to 31 Dec 2025
 2026 S1 · 1 Jan to 30 Jun 2026
 2026 S2 · 1 Jul to 31 Dec 2026
 2027 S1 · 1 Jan to 30 Jun 2027
 2027 S2 · 1 Jul to 31 Dec 2027
 The College's semester 2 ends in November; its window runs to 31 December, so December's work counts. 
 Trainee
 Every trainee
 Anele Dlamini
 Pieter du Plessis
 Nomsa Mahlangu
 Lerato Molefe
 Sipho Ndlovu
 Status
 Outstanding: not yet decided
 Every status
 Not scheduled
 Scheduled
 Decided
 Not decided
 Revoked: re-decide
 Due by year end
 Partial period
 As opportunity allows
 Missed
 EPA
 Every EPA
 PAED-001 — Providing paediatric emergency care to children
 PAED-002 — [PAED-002 title, as seeded]
 PAED-003 — Providing intensive care to children
 PAED-004 — [PAED-004 title, as seeded]
 PAED-005 — [PAED-005 title, as seeded]
 PAED-006 — [PAED-006 title, as seeded]
 PAED-007 — [PAED-007 title, as seeded]
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
 PAED-009 — [PAED-009 title, as seeded]
 PAED-010 — [PAED-010 title, as seeded]
 PAED-011 — [PAED-011 title, as seeded]
 PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
 PAED-013 — [PAED-013 title, as seeded]
 PAED-014 — [PAED-014 title, as seeded]
 PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
 Show Clear filters 
## By EPA
 The filters above do not narrow these counts.
 Counts for the 15 EPAs due in 2026 S2 Show Hide 
 How many trainees stand where on each EPA due in 2026 S2. To schedule: not scheduled, due by year end, or revoked. Optional: a partial period, or decided as opportunity allows.
 EPA | Due | Decided | Scheduled | Not decided | To schedule | Missed | Optional | 
 PAED-001 — Providing paediatric emergency care to children | 5 | 2 | 0 | 3 | 0 | 0 | 0 | 
 PAED-002 — [PAED-002 title, as seeded] | 5 | 1 | 0 | 4 | 0 | 0 | 0 | 
 PAED-003 — Providing intensive care to children | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
 PAED-004 — [PAED-004 title, as seeded] | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
 PAED-005 — [PAED-005 title, as seeded] | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
 PAED-006 — [PAED-006 title, as seeded] | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
 PAED-007 — [PAED-007 title, as seeded] | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-009 — [PAED-009 title, as seeded] | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-010 — [PAED-010 title, as seeded] | 5 | 1 | 0 | 4 | 0 | 0 | 0 | 
 PAED-011 — [PAED-011 title, as seeded] | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
 PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 5 | 1 | 0 | 4 | 0 | 0 | 0 | 
 PAED-013 — [PAED-013 title, as seeded] | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-014 — [PAED-014 title, as seeded] | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
 PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
## Decisions 
 15 of 75 decisions due in 2026 S2 shown: Lerato Molefe, every status.
 No open review holds 2026 S2.
 Schedule a review 
 Lerato Molefe's decisions due in 2026 S2, every status, by EPA
 EPA | Window | Status | Actions | 
 PAED-001 — Providing paediatric emergency care to children | 2026 S2 | Decided STAR #1, issued at review #1 .
 | Decided: nothing to schedule 
 | 
 PAED-002 — [PAED-002 title, as seeded] | 2026 S2 | Not decided Not decided at review #1 . A later sitting for 2026 S2 plans it again.
 | Schedule 
 | 
 PAED-003 — Providing intensive care to children | 2026 | Not decided Not decided at review #1 . A later sitting for 2026 plans it again.
 | Schedule 
 | 
 PAED-004 — [PAED-004 title, as seeded] | 2026 S2 | Not decided Not decided at review #1 . A later sitting for 2026 S2 plans it again.
 | Schedule 
 | 
 PAED-005 — [PAED-005 title, as seeded] | 2026 S2 | Not decided Not decided at review #1 . A later sitting for 2026 S2 plans it again.
 | Schedule 
 | 
 PAED-006 — [PAED-006 title, as seeded] | 2026 | Not decided Not decided at review #1 . A later sitting for 2026 plans it again.
 | Schedule 
 | 
 The other 9 rows of this page continue in the same form
 Showing 1–15 of 15
 Previous Next 
 Decisions per page
 10
 20
 50
 100


######## BOARD R2-3-decisions-due-390-d.dc.html — Decisions due · 390 · d
Decisions due · 390 · d 
# Decisions due · 390px (continued, part d) 
 Route /committee/decisions-due · the schedulers' list; a Committee member is refused.
 Components PageHeader, FilterBar (Show, Clear filters), DataTable (stack-wide), PagerControls (labelled), the .rung-legend fold with .fold-show / .fold-hide, StatePanel, Badge, AccessDenied. NEW the Trainee filter, .trainee-line (C2), .status-cell, .list-section-page, .list-section-count.
 DESIGN.md Decisions due's section; § Pager; § Table system. Round 3 the cadence (M1, M2), By EPA from the board's rows (M3), F1, F2, F6, M8, S1, S4, S5, S7.
=== FRAME: | One trainee: an open review holds the period (C2, C3) | | Steps 4.9 and 4.12 · each row's one action is Open review #1; no Schedule is drawn | 
 [shell] 
# Decisions due
 What the committee must decide, per registrar and per EPA, and where each decision stands.
 The EPAs due for an entrustment decision in 2026 S2 for the 5 trainees you oversee at Kgosi Kgari Teaching Hospital, by each EPA's decision cadence: every semester, or once a year. Missed is read today: the EPA's window has ended with nothing decided or on an open review's agenda.
 Period
 2025 S1 · 1 Jan to 30 Jun 2025
 2025 S2 · 1 Jul to 31 Dec 2025
 2026 S1 · 1 Jan to 30 Jun 2026
 2026 S2 · 1 Jul to 31 Dec 2026
 2027 S1 · 1 Jan to 30 Jun 2027
 2027 S2 · 1 Jul to 31 Dec 2027
 The College's semester 2 ends in November; its window runs to 31 December, so December's work counts. 
 Trainee
 Every trainee
 Anele Dlamini
 Pieter du Plessis
 Nomsa Mahlangu
 Lerato Molefe
 Sipho Ndlovu
 Status
 Outstanding: not yet decided
 Every status
 Not scheduled
 Scheduled
 Decided
 Not decided
 Revoked: re-decide
 Due by year end
 Partial period
 As opportunity allows
 Missed
 EPA
 Every EPA
 PAED-001 — Providing paediatric emergency care to children
 PAED-002 — [PAED-002 title, as seeded]
 PAED-003 — Providing intensive care to children
 PAED-004 — [PAED-004 title, as seeded]
 PAED-005 — [PAED-005 title, as seeded]
 PAED-006 — [PAED-006 title, as seeded]
 PAED-007 — [PAED-007 title, as seeded]
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
 PAED-009 — [PAED-009 title, as seeded]
 PAED-010 — [PAED-010 title, as seeded]
 PAED-011 — [PAED-011 title, as seeded]
 PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
 PAED-013 — [PAED-013 title, as seeded]
 PAED-014 — [PAED-014 title, as seeded]
 PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
 Show Clear filters 
## By EPA
 The filters above do not narrow these counts.
 Counts for the 15 EPAs due in 2026 S2 Show Hide 
 How many trainees stand where on each EPA due in 2026 S2. To schedule: not scheduled, due by year end, or revoked. Optional: a partial period, or decided as opportunity allows.
 EPA | Due | Decided | Scheduled | Not decided | To schedule | Missed | Optional | 
 PAED-001 — Providing paediatric emergency care to children | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-002 — [PAED-002 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-003 — Providing intensive care to children | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-004 — [PAED-004 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-005 — [PAED-005 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-006 — [PAED-006 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-007 — [PAED-007 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-009 — [PAED-009 title, as seeded] | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-010 — [PAED-010 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-011 — [PAED-011 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-013 — [PAED-013 title, as seeded] | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-014 — [PAED-014 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
## Decisions 
 15 of 75 decisions due in 2026 S2 shown: Lerato Molefe, outstanding.
 Review #1 (annual progression) holds 2026 S2 before Paed Annual Review Panel, scheduled on 2026-10-12.
 Open review #1 
 Lerato Molefe's decisions due in 2026 S2, outstanding, by EPA
 EPA | Window | Status | Actions | 
 PAED-001 — Providing paediatric emergency care to children | 2026 S2 | Scheduled On the agenda of review #1 before Paed Annual Review Panel, which is still open.
 | Open review #1 
 | 
 PAED-002 — [PAED-002 title, as seeded] | 2026 S2 | Scheduled On the agenda of review #1 before Paed Annual Review Panel, which is still open.
 | Open review #1 
 | 
 PAED-003 — Providing intensive care to children | 2026 | Scheduled On the agenda of review #1 before Paed Annual Review Panel, which is still open.
 | Open review #1 
 | 
 PAED-004 — [PAED-004 title, as seeded] | 2026 S2 | Scheduled On the agenda of review #1 before Paed Annual Review Panel, which is still open.
 | Open review #1 
 | 
 The other 11 rows of this page continue in the same form
 Showing 1–15 of 15
 Previous Next 
 Decisions per page
 10
 20
 50
 100
=== FRAME: | Heavy: two panels can hold the trainee (S1) | | Pieter Smit · one trainee line per panel; PAED-004 and PAED-005 are the neonatal panel's: Not scheduled, their Schedule named for it | 
 [shell] 
# Decisions due
 What the committee must decide, per registrar and per EPA, and where each decision stands.
 The EPAs due for an entrustment decision in 2026 S2 for the 5 trainees you oversee at Kgosi Kgari Teaching Hospital, by each EPA's decision cadence: every semester, or once a year. Missed is read today: the EPA's window has ended with nothing decided or on an open review's agenda.
 Period
 2025 S1 · 1 Jan to 30 Jun 2025
 2025 S2 · 1 Jul to 31 Dec 2025
 2026 S1 · 1 Jan to 30 Jun 2026
 2026 S2 · 1 Jul to 31 Dec 2026
 2027 S1 · 1 Jan to 30 Jun 2027
 2027 S2 · 1 Jul to 31 Dec 2027
 The College's semester 2 ends in November; its window runs to 31 December, so December's work counts. 
 Trainee
 Every trainee
 Anele Dlamini
 Pieter du Plessis
 Nomsa Mahlangu
 Lerato Molefe
 Sipho Ndlovu
 Status
 Outstanding: not yet decided
 Every status
 Not scheduled
 Scheduled
 Decided
 Not decided
 Revoked: re-decide
 Due by year end
 Partial period
 As opportunity allows
 Missed
 EPA
 Every EPA
 PAED-001 — Providing paediatric emergency care to children
 PAED-002 — [PAED-002 title, as seeded]
 PAED-003 — Providing intensive care to children
 PAED-004 — [PAED-004 title, as seeded]
 PAED-005 — [PAED-005 title, as seeded]
 PAED-006 — [PAED-006 title, as seeded]
 PAED-007 — [PAED-007 title, as seeded]
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
 PAED-009 — [PAED-009 title, as seeded]
 PAED-010 — [PAED-010 title, as seeded]
 PAED-011 — [PAED-011 title, as seeded]
 PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
 PAED-013 — [PAED-013 title, as seeded]
 PAED-014 — [PAED-014 title, as seeded]
 PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
 Show Clear filters 
## By EPA
 The filters above do not narrow these counts.
 Counts for the 15 EPAs due in 2026 S2 Show Hide 
 How many trainees stand where on each EPA due in 2026 S2. To schedule: not scheduled, due by year end, or revoked. Optional: a partial period, or decided as opportunity allows.
 EPA | Due | Decided | Scheduled | Not decided | To schedule | Missed | Optional | 
 PAED-001 — Providing paediatric emergency care to children | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-002 — [PAED-002 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-003 — Providing intensive care to children | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-004 — [PAED-004 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-005 — [PAED-005 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-006 — [PAED-006 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-007 — [PAED-007 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-009 — [PAED-009 title, as seeded] | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-010 — [PAED-010 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-011 — [PAED-011 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-013 — [PAED-013 title, as seeded] | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-014 — [PAED-014 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
## Decisions 
 15 of 75 decisions due in 2026 S2 shown: Lerato Molefe, outstanding.
 Review #1 (annual progression) holds 2026 S2 before Paed Annual Review Panel, scheduled on 2026-10-12.
 Open review #1 
 No open review before [Neonatal panel] holds 2026 S2.
 Schedule a review before [Neonatal panel] 
 Lerato Molefe's decisions due in 2026 S2, outstanding, by EPA
 EPA | Window | Status | Actions | 
 PAED-001 — Providing paediatric emergency care to children | 2026 S2 | Scheduled On the agenda of review #1 before Paed Annual Review Panel, which is still open.
 | Open review #1 
 | 
 PAED-002 — [PAED-002 title, as seeded] | 2026 S2 | Scheduled On the agenda of review #1 before Paed Annual Review Panel, which is still open.
 | Open review #1 
 | 
 PAED-003 — Providing intensive care to children | 2026 | Scheduled On the agenda of review #1 before Paed Annual Review Panel, which is still open.
 | Open review #1 
 | 
 PAED-004 — [PAED-004 title, as seeded] | 2026 S2 | Not scheduled No open review holds it, and 2026 S2 has not ended.
 | Schedule 
 | 
 PAED-005 — [PAED-005 title, as seeded] | 2026 S2 | Not scheduled No open review holds it, and 2026 S2 has not ended.
 | Schedule 
 | 
 PAED-006 — [PAED-006 title, as seeded] | 2026 | Scheduled On the agenda of review #1 before Paed Annual Review Panel, which is still open.
 | Open review #1 
 | 
 The other 9 rows of this page continue in the same form
 Showing 1–15 of 15
 Previous Next 
 Decisions per page
 10
 20
 50
 100


######## BOARD R2-3-decisions-due-390-e.dc.html — Decisions due · 390 · e
Decisions due · 390 · e 
# Decisions due · 390px (continued, part e) 
 Route /committee/decisions-due · the schedulers' list; a Committee member is refused.
 Components PageHeader, FilterBar (Show, Clear filters), DataTable (stack-wide), PagerControls (labelled), the .rung-legend fold with .fold-show / .fold-hide, StatePanel, Badge, AccessDenied. NEW the Trainee filter, .trainee-line (C2), .status-cell, .list-section-page, .list-section-count.
 DESIGN.md Decisions due's section; § Pager; § Table system. Round 3 the cadence (M1, M2), By EPA from the board's rows (M3), F1, F2, F6, M8, S1, S4, S5, S7.
=== FRAME: | One trainee: a formative check-in is open (C2) | | Steps 4.49 and A.1.10 · the check-in takes no seat, so the period can still be scheduled | 
 [shell] 
# Decisions due
 What the committee must decide, per registrar and per EPA, and where each decision stands.
 The EPAs due for an entrustment decision in 2026 S2 for the 5 trainees you oversee at Kgosi Kgari Teaching Hospital, by each EPA's decision cadence: every semester, or once a year. Missed is read today: the EPA's window has ended with nothing decided or on an open review's agenda.
 Period
 2025 S1 · 1 Jan to 30 Jun 2025
 2025 S2 · 1 Jul to 31 Dec 2025
 2026 S1 · 1 Jan to 30 Jun 2026
 2026 S2 · 1 Jul to 31 Dec 2026
 2027 S1 · 1 Jan to 30 Jun 2027
 2027 S2 · 1 Jul to 31 Dec 2027
 The College's semester 2 ends in November; its window runs to 31 December, so December's work counts. 
 Trainee
 Every trainee
 Anele Dlamini
 Pieter du Plessis
 Nomsa Mahlangu
 Lerato Molefe
 Sipho Ndlovu
 Status
 Outstanding: not yet decided
 Every status
 Not scheduled
 Scheduled
 Decided
 Not decided
 Revoked: re-decide
 Due by year end
 Partial period
 As opportunity allows
 Missed
 EPA
 Every EPA
 PAED-001 — Providing paediatric emergency care to children
 PAED-002 — [PAED-002 title, as seeded]
 PAED-003 — Providing intensive care to children
 PAED-004 — [PAED-004 title, as seeded]
 PAED-005 — [PAED-005 title, as seeded]
 PAED-006 — [PAED-006 title, as seeded]
 PAED-007 — [PAED-007 title, as seeded]
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
 PAED-009 — [PAED-009 title, as seeded]
 PAED-010 — [PAED-010 title, as seeded]
 PAED-011 — [PAED-011 title, as seeded]
 PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
 PAED-013 — [PAED-013 title, as seeded]
 PAED-014 — [PAED-014 title, as seeded]
 PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
 Show Clear filters 
## By EPA
 The filters above do not narrow these counts.
 Counts for the 15 EPAs due in 2026 S2 Show Hide 
 How many trainees stand where on each EPA due in 2026 S2. To schedule: not scheduled, due by year end, or revoked. Optional: a partial period, or decided as opportunity allows.
 EPA | Due | Decided | Scheduled | Not decided | To schedule | Missed | Optional | 
 PAED-001 — Providing paediatric emergency care to children | 5 | 2 | 0 | 3 | 0 | 0 | 0 | 
 PAED-002 — [PAED-002 title, as seeded] | 5 | 1 | 0 | 4 | 0 | 0 | 0 | 
 PAED-003 — Providing intensive care to children | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
 PAED-004 — [PAED-004 title, as seeded] | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
 PAED-005 — [PAED-005 title, as seeded] | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
 PAED-006 — [PAED-006 title, as seeded] | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
 PAED-007 — [PAED-007 title, as seeded] | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-009 — [PAED-009 title, as seeded] | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-010 — [PAED-010 title, as seeded] | 5 | 1 | 0 | 4 | 0 | 0 | 0 | 
 PAED-011 — [PAED-011 title, as seeded] | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
 PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 5 | 1 | 0 | 4 | 0 | 0 | 0 | 
 PAED-013 — [PAED-013 title, as seeded] | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-014 — [PAED-014 title, as seeded] | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
 PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
## Decisions 
 15 of 75 decisions due in 2026 S2 shown: Sipho Ndlovu, every status.
 No open review holds 2026 S2.
 Schedule a review 
 Formative check-in #6 is open: it takes no seat.
 Open check-in #6 
 Sipho Ndlovu's decisions due in 2026 S2, every status, by EPA
 EPA | Window | Status | Actions | 
 PAED-001 — Providing paediatric emergency care to children | 2026 S2 | Not decided Not decided at review #5 . A later sitting for 2026 S2 plans it again.
 | Schedule 
 | 
 PAED-002 — [PAED-002 title, as seeded] | 2026 S2 | Not decided Not decided at review #5 . A later sitting for 2026 S2 plans it again.
 | Schedule 
 | 
 PAED-003 — Providing intensive care to children | 2026 | Not decided Not decided at review #5 . A later sitting for 2026 plans it again.
 | Schedule 
 | 
 PAED-004 — [PAED-004 title, as seeded] | 2026 S2 | Not decided Not decided at review #5 . A later sitting for 2026 S2 plans it again.
 | Schedule 
 | 
 The other 11 rows of this page continue in the same form
 Showing 1–15 of 15
 Previous Next 
 Decisions per page
 10
 20
 50
 100
=== FRAME: | Filtered to one EPA | | Step 4.1 · Pieter Smit · one row a registrar, by surname; By EPA does not change | | Focus: the Decisions heading, after Show | 
 [shell] 
# Decisions due
 What the committee must decide, per registrar and per EPA, and where each decision stands.
 The EPAs due for an entrustment decision in 2026 S2 for the 5 trainees you oversee at Kgosi Kgari Teaching Hospital, by each EPA's decision cadence: every semester, or once a year. Missed is read today: the EPA's window has ended with nothing decided or on an open review's agenda.
 Period
 2025 S1 · 1 Jan to 30 Jun 2025
 2025 S2 · 1 Jul to 31 Dec 2025
 2026 S1 · 1 Jan to 30 Jun 2026
 2026 S2 · 1 Jul to 31 Dec 2026
 2027 S1 · 1 Jan to 30 Jun 2027
 2027 S2 · 1 Jul to 31 Dec 2027
 The College's semester 2 ends in November; its window runs to 31 December, so December's work counts. 
 Trainee
 Every trainee
 Anele Dlamini
 Pieter du Plessis
 Nomsa Mahlangu
 Lerato Molefe
 Sipho Ndlovu
 Status
 Outstanding: not yet decided
 Every status
 Not scheduled
 Scheduled
 Decided
 Not decided
 Revoked: re-decide
 Due by year end
 Partial period
 As opportunity allows
 Missed
 EPA
 Every EPA
 PAED-001 — Providing paediatric emergency care to children
 PAED-002 — [PAED-002 title, as seeded]
 PAED-003 — Providing intensive care to children
 PAED-004 — [PAED-004 title, as seeded]
 PAED-005 — [PAED-005 title, as seeded]
 PAED-006 — [PAED-006 title, as seeded]
 PAED-007 — [PAED-007 title, as seeded]
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
 PAED-009 — [PAED-009 title, as seeded]
 PAED-010 — [PAED-010 title, as seeded]
 PAED-011 — [PAED-011 title, as seeded]
 PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
 PAED-013 — [PAED-013 title, as seeded]
 PAED-014 — [PAED-014 title, as seeded]
 PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
 Show Clear filters 
## By EPA
 The filters above do not narrow these counts.
 Counts for the 15 EPAs due in 2026 S2 Show Hide 
 How many trainees stand where on each EPA due in 2026 S2. To schedule: not scheduled, due by year end, or revoked. Optional: a partial period, or decided as opportunity allows.
 EPA | Due | Decided | Scheduled | Not decided | To schedule | Missed | Optional | 
 PAED-001 — Providing paediatric emergency care to children | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-002 — [PAED-002 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-003 — Providing intensive care to children | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-004 — [PAED-004 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-005 — [PAED-005 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-006 — [PAED-006 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-007 — [PAED-007 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-009 — [PAED-009 title, as seeded] | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-010 — [PAED-010 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-011 — [PAED-011 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-013 — [PAED-013 title, as seeded] | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-014 — [PAED-014 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
## Decisions 
 5 of 75 decisions due in 2026 S2 shown: Not scheduled, PAED-004.
 Decisions due in 2026 S2: Not scheduled, PAED-004
 EPA | Trainee | Window | Status | Actions | 
 PAED-004 — [PAED-004 title, as seeded] | Anele Dlamini | 2026 S2 | Not scheduled No open review holds it, and 2026 S2 has not ended.
 | Schedule 
 | 
 PAED-004 — [PAED-004 title, as seeded] | Pieter du Plessis | 2026 S2 | Not scheduled No open review holds it, and 2026 S2 has not ended.
 | Schedule 
 | 
 PAED-004 — [PAED-004 title, as seeded] | Nomsa Mahlangu | 2026 S2 | Not scheduled No open review holds it, and 2026 S2 has not ended.
 | Schedule 
 | 
 PAED-004 — [PAED-004 title, as seeded] | Lerato Molefe | 2026 S2 | Not scheduled No open review holds it, and 2026 S2 has not ended.
 | Schedule 
 | 
 PAED-004 — [PAED-004 title, as seeded] | Sipho Ndlovu | 2026 S2 | Not scheduled No open review holds it, and 2026 S2 has not ended.
 | Schedule 
 | 
 Showing 1–5 of 5
 Previous Next 
 Decisions per page
 10
 20
 50
 100
=== FRAME: | No row matches | | Pieter Smit · FilterBar's pattern | | Focus: the Decisions heading, after Show | 
 [shell] 
# Decisions due
 What the committee must decide, per registrar and per EPA, and where each decision stands.
 The EPAs due for an entrustment decision in 2026 S2 for the 5 trainees you oversee at Kgosi Kgari Teaching Hospital, by each EPA's decision cadence: every semester, or once a year. Missed is read today: the EPA's window has ended with nothing decided or on an open review's agenda.
 Period
 2025 S1 · 1 Jan to 30 Jun 2025
 2025 S2 · 1 Jul to 31 Dec 2025
 2026 S1 · 1 Jan to 30 Jun 2026
 2026 S2 · 1 Jul to 31 Dec 2026
 2027 S1 · 1 Jan to 30 Jun 2027
 2027 S2 · 1 Jul to 31 Dec 2027
 The College's semester 2 ends in November; its window runs to 31 December, so December's work counts. 
 Trainee
 Every trainee
 Anele Dlamini
 Pieter du Plessis
 Nomsa Mahlangu
 Lerato Molefe
 Sipho Ndlovu
 Status
 Outstanding: not yet decided
 Every status
 Not scheduled
 Scheduled
 Decided
 Not decided
 Revoked: re-decide
 Due by year end
 Partial period
 As opportunity allows
 Missed
 EPA
 Every EPA
 PAED-001 — Providing paediatric emergency care to children
 PAED-002 — [PAED-002 title, as seeded]
 PAED-003 — Providing intensive care to children
 PAED-004 — [PAED-004 title, as seeded]
 PAED-005 — [PAED-005 title, as seeded]
 PAED-006 — [PAED-006 title, as seeded]
 PAED-007 — [PAED-007 title, as seeded]
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
 PAED-009 — [PAED-009 title, as seeded]
 PAED-010 — [PAED-010 title, as seeded]
 PAED-011 — [PAED-011 title, as seeded]
 PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
 PAED-013 — [PAED-013 title, as seeded]
 PAED-014 — [PAED-014 title, as seeded]
 PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
 Show Clear filters 
## By EPA
 The filters above do not narrow these counts.
 Counts for the 15 EPAs due in 2026 S2 Show Hide 
 How many trainees stand where on each EPA due in 2026 S2. To schedule: not scheduled, due by year end, or revoked. Optional: a partial period, or decided as opportunity allows.
 EPA | Due | Decided | Scheduled | Not decided | To schedule | Missed | Optional | 
 PAED-001 — Providing paediatric emergency care to children | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-002 — [PAED-002 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-003 — Providing intensive care to children | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-004 — [PAED-004 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-005 — [PAED-005 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-006 — [PAED-006 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-007 — [PAED-007 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-009 — [PAED-009 title, as seeded] | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-010 — [PAED-010 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-011 — [PAED-011 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-013 — [PAED-013 title, as seeded] | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-014 — [PAED-014 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
## Decisions
 0 of 75 decisions due in 2026 S2 shown.
 No decision matches these filters.
 Missed only, every trainee, every EPA.
 Clear filters


######## BOARD R2-3-decisions-due-390-f.dc.html — Decisions due · 390 · f
Decisions due · 390 · f 
# Decisions due · 390px (continued, part f) 
 Route /committee/decisions-due · the schedulers' list; a Committee member is refused.
 Components PageHeader, FilterBar (Show, Clear filters), DataTable (stack-wide), PagerControls (labelled), the .rung-legend fold with .fold-show / .fold-hide, StatePanel, Badge, AccessDenied. NEW the Trainee filter, .trainee-line (C2), .status-cell, .list-section-page, .list-section-count.
 DESIGN.md Decisions due's section; § Pager; § Table system. Round 3 the cadence (M1, M2), By EPA from the board's rows (M3), F1, F2, F6, M8, S1, S4, S5, S7.
=== FRAME: | A past semester, 2026 S1: Missed rows (F1, M1, M3) | | Step 4.2 · Nolwazi Mbatha · the six semester EPAs Missed for every registrar (Mahlangu's and Ndlovu's 15 January start counts, D42); the annual six Due by year end, PAED-003 and 006 among them; the optional three As opportunity allows. By EPA open: Missed 5 on 001, 002, 004, 005, 010 and 012. Each row's Schedule opens the form on 2026 S1 | 
 [shell] 
# Decisions due
 What the committee must decide, per registrar and per EPA, and where each decision stands.
 The EPAs due for an entrustment decision in 2026 S1 for the 5 trainees you oversee at Kgosi Kgari Teaching Hospital, by each EPA's decision cadence: every semester, or once a year. Missed is read today: the EPA's window has ended with nothing decided or on an open review's agenda.
 Period
 2025 S1 · 1 Jan to 30 Jun 2025
 2025 S2 · 1 Jul to 31 Dec 2025
 2026 S1 · 1 Jan to 30 Jun 2026
 2026 S2 · 1 Jul to 31 Dec 2026
 2027 S1 · 1 Jan to 30 Jun 2027
 2027 S2 · 1 Jul to 31 Dec 2027
 Trainee
 Every trainee
 Anele Dlamini
 Pieter du Plessis
 Nomsa Mahlangu
 Lerato Molefe
 Sipho Ndlovu
 Status
 Outstanding: not yet decided
 Every status
 Not scheduled
 Scheduled
 Decided
 Not decided
 Revoked: re-decide
 Due by year end
 Partial period
 As opportunity allows
 Missed
 EPA
 Every EPA
 PAED-001 — Providing paediatric emergency care to children
 PAED-002 — [PAED-002 title, as seeded]
 PAED-003 — Providing intensive care to children
 PAED-004 — [PAED-004 title, as seeded]
 PAED-005 — [PAED-005 title, as seeded]
 PAED-006 — [PAED-006 title, as seeded]
 PAED-007 — [PAED-007 title, as seeded]
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
 PAED-009 — [PAED-009 title, as seeded]
 PAED-010 — [PAED-010 title, as seeded]
 PAED-011 — [PAED-011 title, as seeded]
 PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
 PAED-013 — [PAED-013 title, as seeded]
 PAED-014 — [PAED-014 title, as seeded]
 PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
 Show Clear filters 
## By EPA
 The filters above do not narrow these counts.
 Counts for the 15 EPAs due in 2026 S1 Show Hide 
 How many trainees stand where on each EPA due in 2026 S1. To schedule: not scheduled, due by year end, or revoked. Optional: a partial period, or decided as opportunity allows.
 EPA | Due | Decided | Scheduled | Not decided | To schedule | Missed | Optional | 
 PAED-001 — Providing paediatric emergency care to children | 5 | 0 | 0 | 0 | 0 | 5 | 0 | 
 PAED-002 — [PAED-002 title, as seeded] | 5 | 0 | 0 | 0 | 0 | 5 | 0 | 
 PAED-003 — Providing intensive care to children | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-004 — [PAED-004 title, as seeded] | 5 | 0 | 0 | 0 | 0 | 5 | 0 | 
 PAED-005 — [PAED-005 title, as seeded] | 5 | 0 | 0 | 0 | 0 | 5 | 0 | 
 PAED-006 — [PAED-006 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-007 — [PAED-007 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-009 — [PAED-009 title, as seeded] | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-010 — [PAED-010 title, as seeded] | 5 | 0 | 0 | 0 | 0 | 5 | 0 | 
 PAED-011 — [PAED-011 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 5 | 0 | 0 | 0 | 0 | 5 | 0 | 
 PAED-013 — [PAED-013 title, as seeded] | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-014 — [PAED-014 title, as seeded] | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
 PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 5 | 0 | 0 | 0 | 5 | 0 | 0 | 
## Decisions , page 1 of 4 
 75 of 75 decisions due in 2026 S1 shown.
 Decisions due in 2026 S1, by trainee then EPA
 EPA | Trainee | Window | Status | Actions | 
 PAED-001 — Providing paediatric emergency care to children | Anele Dlamini | 2026 S1 | Missed 2026 S1 has ended with nothing decided or on an open review's agenda.
 | Schedule 
 | 
 PAED-002 — [PAED-002 title, as seeded] | Anele Dlamini | 2026 S1 | Missed 2026 S1 has ended with nothing decided or on an open review's agenda.
 | Schedule 
 | 
 PAED-003 — Providing intensive care to children | Anele Dlamini | 2026 | Due by year end Decided once a year: due by the last sitting of 2026, and not missed before the year ends.
 | Schedule 
 | 
 PAED-004 — [PAED-004 title, as seeded] | Anele Dlamini | 2026 S1 | Missed 2026 S1 has ended with nothing decided or on an open review's agenda.
 | Schedule 
 | 
 PAED-005 — [PAED-005 title, as seeded] | Anele Dlamini | 2026 S1 | Missed 2026 S1 has ended with nothing decided or on an open review's agenda.
 | Schedule 
 | 
 PAED-006 — [PAED-006 title, as seeded] | Anele Dlamini | 2026 | Due by year end Decided once a year: due by the last sitting of 2026, and not missed before the year ends.
 | Schedule 
 | 
 The other 14 rows of this page continue in the same form
 Showing 1–20 of 75
 Previous Next 
 Decisions per page
 10
 20
 50
 100


######## BOARD R2-3-decisions-due-390-g.dc.html — Decisions due · 390 · g
Decisions due · 390 · g 
# Decisions due · 390px (continued, part g) 
 Route /committee/decisions-due · the schedulers' list; a Committee member is refused.
 Components PageHeader, FilterBar (Show, Clear filters), DataTable (stack-wide), PagerControls (labelled), the .rung-legend fold with .fold-show / .fold-hide, StatePanel, Badge, AccessDenied. NEW the Trainee filter, .trainee-line (C2), .status-cell, .list-section-page, .list-section-count.
 DESIGN.md Decisions due's section; § Pager; § Table system. Round 3 the cadence (M1, M2), By EPA from the board's rows (M3), F1, F2, F6, M8, S1, S4, S5, S7.
=== FRAME: | All scheduled | | Step 4.10 (4.12 reads the same) · Kabelo Sithole · By EPA open: Scheduled 5 and To schedule 0 on every EPA; each row's one action is Open review #n; the sentence names the panel (T259) | 
 [shell] 
# Decisions due
 What the committee must decide, per registrar and per EPA, and where each decision stands.
 The EPAs due for an entrustment decision in 2026 S2 for the 5 trainees you oversee at Kgosi Kgari Teaching Hospital, by each EPA's decision cadence: every semester, or once a year. Missed is read today: the EPA's window has ended with nothing decided or on an open review's agenda.
 Period
 2025 S1 · 1 Jan to 30 Jun 2025
 2025 S2 · 1 Jul to 31 Dec 2025
 2026 S1 · 1 Jan to 30 Jun 2026
 2026 S2 · 1 Jul to 31 Dec 2026
 2027 S1 · 1 Jan to 30 Jun 2027
 2027 S2 · 1 Jul to 31 Dec 2027
 The College's semester 2 ends in November; its window runs to 31 December, so December's work counts. 
 Trainee
 Every trainee
 Anele Dlamini
 Pieter du Plessis
 Nomsa Mahlangu
 Lerato Molefe
 Sipho Ndlovu
 Status
 Outstanding: not yet decided
 Every status
 Not scheduled
 Scheduled
 Decided
 Not decided
 Revoked: re-decide
 Due by year end
 Partial period
 As opportunity allows
 Missed
 EPA
 Every EPA
 PAED-001 — Providing paediatric emergency care to children
 PAED-002 — [PAED-002 title, as seeded]
 PAED-003 — Providing intensive care to children
 PAED-004 — [PAED-004 title, as seeded]
 PAED-005 — [PAED-005 title, as seeded]
 PAED-006 — [PAED-006 title, as seeded]
 PAED-007 — [PAED-007 title, as seeded]
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
 PAED-009 — [PAED-009 title, as seeded]
 PAED-010 — [PAED-010 title, as seeded]
 PAED-011 — [PAED-011 title, as seeded]
 PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
 PAED-013 — [PAED-013 title, as seeded]
 PAED-014 — [PAED-014 title, as seeded]
 PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
 Show 
## By EPA
 The filters above do not narrow these counts.
 Counts for the 15 EPAs due in 2026 S2 Show Hide 
 How many trainees stand where on each EPA due in 2026 S2. To schedule: not scheduled, due by year end, or revoked. Optional: a partial period, or decided as opportunity allows.
 EPA | Due | Decided | Scheduled | Not decided | To schedule | Missed | Optional | 
 PAED-001 — Providing paediatric emergency care to children | 5 | 0 | 5 | 0 | 0 | 0 | 0 | 
 PAED-002 — [PAED-002 title, as seeded] | 5 | 0 | 5 | 0 | 0 | 0 | 0 | 
 PAED-003 — Providing intensive care to children | 5 | 0 | 5 | 0 | 0 | 0 | 0 | 
 PAED-004 — [PAED-004 title, as seeded] | 5 | 0 | 5 | 0 | 0 | 0 | 0 | 
 PAED-005 — [PAED-005 title, as seeded] | 5 | 0 | 5 | 0 | 0 | 0 | 0 | 
 PAED-006 — [PAED-006 title, as seeded] | 5 | 0 | 5 | 0 | 0 | 0 | 0 | 
 PAED-007 — [PAED-007 title, as seeded] | 5 | 0 | 5 | 0 | 0 | 0 | 0 | 
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 5 | 0 | 5 | 0 | 0 | 0 | 0 | 
 PAED-009 — [PAED-009 title, as seeded] | 5 | 0 | 5 | 0 | 0 | 0 | 0 | 
 PAED-010 — [PAED-010 title, as seeded] | 5 | 0 | 5 | 0 | 0 | 0 | 0 | 
 PAED-011 — [PAED-011 title, as seeded] | 5 | 0 | 5 | 0 | 0 | 0 | 0 | 
 PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 5 | 0 | 5 | 0 | 0 | 0 | 0 | 
 PAED-013 — [PAED-013 title, as seeded] | 5 | 0 | 5 | 0 | 0 | 0 | 0 | 
 PAED-014 — [PAED-014 title, as seeded] | 5 | 0 | 5 | 0 | 0 | 0 | 0 | 
 PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 5 | 0 | 5 | 0 | 0 | 0 | 0 | 
## Decisions , page 1 of 4 
 75 of 75 decisions due in 2026 S2 shown.
 Decisions due in 2026 S2, by trainee then EPA
 EPA | Trainee | Window | Status | Actions | 
 PAED-001 — Providing paediatric emergency care to children | Anele Dlamini | 2026 S2 | Scheduled On the agenda of review #2 before Paed Annual Review Panel, which is still open.
 | Open review #2 
 | 
 PAED-002 — [PAED-002 title, as seeded] | Anele Dlamini | 2026 S2 | Scheduled On the agenda of review #2 before Paed Annual Review Panel, which is still open.
 | Open review #2 
 | 
 PAED-003 — Providing intensive care to children | Anele Dlamini | 2026 | Scheduled On the agenda of review #2 before Paed Annual Review Panel, which is still open.
 | Open review #2 
 | 
 PAED-004 — [PAED-004 title, as seeded] | Anele Dlamini | 2026 S2 | Scheduled On the agenda of review #2 before Paed Annual Review Panel, which is still open.
 | Open review #2 
 | 
 The other 16 rows of this page continue in the same form
 Showing 1–20 of 75
 Previous Next 
 Decisions per page
 10
 20
 50
 100


######## BOARD R2-3-decisions-due-390-h.dc.html — Decisions due · 390 · h
Decisions due · 390 · h 
# Decisions due · 390px (continued, part h) 
 Route /committee/decisions-due · the schedulers' list; a Committee member is refused.
 Components PageHeader, FilterBar (Show, Clear filters), DataTable (stack-wide), PagerControls (labelled), the .rung-legend fold with .fold-show / .fold-hide, StatePanel, Badge, AccessDenied. NEW the Trainee filter, .trainee-line (C2), .status-cell, .list-section-page, .list-section-count.
 DESIGN.md Decisions due's section; § Pager; § Table system. Round 3 the cadence (M1, M2), By EPA from the board's rows (M3), F1, F2, F6, M8, S1, S4, S5, S7.
=== FRAME: | After the sitting: Decided, Not decided, optional | | Step 4.34 · Nolwazi Mbatha · Every status (the default filter shows 70 of 75); By EPA open: PAED-001 Due 5, Decided 2, Not decided 3 | 
 [shell] 
# Decisions due
 What the committee must decide, per registrar and per EPA, and where each decision stands.
 The EPAs due for an entrustment decision in 2026 S2 for the 5 trainees you oversee at Kgosi Kgari Teaching Hospital, by each EPA's decision cadence: every semester, or once a year. Missed is read today: the EPA's window has ended with nothing decided or on an open review's agenda.
 Period
 2025 S1 · 1 Jan to 30 Jun 2025
 2025 S2 · 1 Jul to 31 Dec 2025
 2026 S1 · 1 Jan to 30 Jun 2026
 2026 S2 · 1 Jul to 31 Dec 2026
 2027 S1 · 1 Jan to 30 Jun 2027
 2027 S2 · 1 Jul to 31 Dec 2027
 The College's semester 2 ends in November; its window runs to 31 December, so December's work counts. 
 Trainee
 Every trainee
 Anele Dlamini
 Pieter du Plessis
 Nomsa Mahlangu
 Lerato Molefe
 Sipho Ndlovu
 Status
 Outstanding: not yet decided
 Every status
 Not scheduled
 Scheduled
 Decided
 Not decided
 Revoked: re-decide
 Due by year end
 Partial period
 As opportunity allows
 Missed
 EPA
 Every EPA
 PAED-001 — Providing paediatric emergency care to children
 PAED-002 — [PAED-002 title, as seeded]
 PAED-003 — Providing intensive care to children
 PAED-004 — [PAED-004 title, as seeded]
 PAED-005 — [PAED-005 title, as seeded]
 PAED-006 — [PAED-006 title, as seeded]
 PAED-007 — [PAED-007 title, as seeded]
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
 PAED-009 — [PAED-009 title, as seeded]
 PAED-010 — [PAED-010 title, as seeded]
 PAED-011 — [PAED-011 title, as seeded]
 PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
 PAED-013 — [PAED-013 title, as seeded]
 PAED-014 — [PAED-014 title, as seeded]
 PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
 Show Clear filters 
## By EPA
 The filters above do not narrow these counts.
 Counts for the 15 EPAs due in 2026 S2 Show Hide 
 How many trainees stand where on each EPA due in 2026 S2. To schedule: not scheduled, due by year end, or revoked. Optional: a partial period, or decided as opportunity allows.
 EPA | Due | Decided | Scheduled | Not decided | To schedule | Missed | Optional | 
 PAED-001 — Providing paediatric emergency care to children | 5 | 2 | 0 | 3 | 0 | 0 | 0 | 
 PAED-002 — [PAED-002 title, as seeded] | 5 | 1 | 0 | 4 | 0 | 0 | 0 | 
 PAED-003 — Providing intensive care to children | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
 PAED-004 — [PAED-004 title, as seeded] | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
 PAED-005 — [PAED-005 title, as seeded] | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
 PAED-006 — [PAED-006 title, as seeded] | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
 PAED-007 — [PAED-007 title, as seeded] | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-009 — [PAED-009 title, as seeded] | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-010 — [PAED-010 title, as seeded] | 5 | 1 | 0 | 4 | 0 | 0 | 0 | 
 PAED-011 — [PAED-011 title, as seeded] | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
 PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 5 | 1 | 0 | 4 | 0 | 0 | 0 | 
 PAED-013 — [PAED-013 title, as seeded] | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-014 — [PAED-014 title, as seeded] | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
 PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
## Decisions , page 1 of 4 
 75 of 75 decisions due in 2026 S2 shown.
 Decisions due in 2026 S2, by trainee then EPA
 EPA | Trainee | Window | Status | Actions | 
 PAED-001 — Providing paediatric emergency care to children | Anele Dlamini | 2026 S2 | Decided STAR #4, issued at review #2 .
 | Decided: nothing to schedule 
 | 
 PAED-002 — [PAED-002 title, as seeded] | Anele Dlamini | 2026 S2 | Decided STAR #5, issued at review #2 .
 | Decided: nothing to schedule 
 | 
 PAED-003 — Providing intensive care to children | Anele Dlamini | 2026 | Not decided Not decided at review #2 . A later sitting for 2026 plans it again.
 | Schedule 
 | 
 PAED-004 — [PAED-004 title, as seeded] | Anele Dlamini | 2026 S2 | Not decided Not decided at review #2 . A later sitting for 2026 S2 plans it again.
 | Schedule 
 | 
 PAED-005 — [PAED-005 title, as seeded] | Anele Dlamini | 2026 S2 | Not decided Not decided at review #2 . A later sitting for 2026 S2 plans it again.
 | Schedule 
 | 
 PAED-006 — [PAED-006 title, as seeded] | Anele Dlamini | 2026 | Not decided Not decided at review #2 . A later sitting for 2026 plans it again.
 | Schedule 
 | 
 The other 14 rows of this page continue in the same form
 Showing 1–20 of 75
 Previous Next 
 Decisions per page
 10
 20
 50
 100


######## BOARD R2-3-decisions-due-390-i.dc.html — Decisions due · 390 · i
Decisions due · 390 · i 
# Decisions due · 390px (continued, part i) 
 Route /committee/decisions-due · the schedulers' list; a Committee member is refused.
 Components PageHeader, FilterBar (Show, Clear filters), DataTable (stack-wide), PagerControls (labelled), the .rung-legend fold with .fold-show / .fold-hide, StatePanel, Badge, AccessDenied. NEW the Trainee filter, .trainee-line (C2), .status-cell, .list-section-page, .list-section-count.
 DESIGN.md Decisions due's section; § Pager; § Table system. Round 3 the cadence (M1, M2), By EPA from the board's rows (M3), F1, F2, F6, M8, S1, S4, S5, S7.
=== FRAME: | Revoked: decide it again | | Step 4.37 · Kabelo Sithole · Anele Dlamini's PAED-002; By EPA open: PAED-002 Decided 0, Not decided 4, To schedule 1 (the default filter shows 71 of 75) | 
 [shell] 
# Decisions due
 What the committee must decide, per registrar and per EPA, and where each decision stands.
 The EPAs due for an entrustment decision in 2026 S2 for the 5 trainees you oversee at Kgosi Kgari Teaching Hospital, by each EPA's decision cadence: every semester, or once a year. Missed is read today: the EPA's window has ended with nothing decided or on an open review's agenda.
 Period
 2025 S1 · 1 Jan to 30 Jun 2025
 2025 S2 · 1 Jul to 31 Dec 2025
 2026 S1 · 1 Jan to 30 Jun 2026
 2026 S2 · 1 Jul to 31 Dec 2026
 2027 S1 · 1 Jan to 30 Jun 2027
 2027 S2 · 1 Jul to 31 Dec 2027
 The College's semester 2 ends in November; its window runs to 31 December, so December's work counts. 
 Trainee
 Every trainee
 Anele Dlamini
 Pieter du Plessis
 Nomsa Mahlangu
 Lerato Molefe
 Sipho Ndlovu
 Status
 Outstanding: not yet decided
 Every status
 Not scheduled
 Scheduled
 Decided
 Not decided
 Revoked: re-decide
 Due by year end
 Partial period
 As opportunity allows
 Missed
 EPA
 Every EPA
 PAED-001 — Providing paediatric emergency care to children
 PAED-002 — [PAED-002 title, as seeded]
 PAED-003 — Providing intensive care to children
 PAED-004 — [PAED-004 title, as seeded]
 PAED-005 — [PAED-005 title, as seeded]
 PAED-006 — [PAED-006 title, as seeded]
 PAED-007 — [PAED-007 title, as seeded]
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
 PAED-009 — [PAED-009 title, as seeded]
 PAED-010 — [PAED-010 title, as seeded]
 PAED-011 — [PAED-011 title, as seeded]
 PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
 PAED-013 — [PAED-013 title, as seeded]
 PAED-014 — [PAED-014 title, as seeded]
 PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
 Show Clear filters 
## By EPA
 The filters above do not narrow these counts.
 Counts for the 15 EPAs due in 2026 S2 Show Hide 
 How many trainees stand where on each EPA due in 2026 S2. To schedule: not scheduled, due by year end, or revoked. Optional: a partial period, or decided as opportunity allows.
 EPA | Due | Decided | Scheduled | Not decided | To schedule | Missed | Optional | 
 PAED-001 — Providing paediatric emergency care to children | 5 | 2 | 0 | 3 | 0 | 0 | 0 | 
 PAED-002 — [PAED-002 title, as seeded] | 5 | 0 | 0 | 4 | 1 | 0 | 0 | 
 PAED-003 — Providing intensive care to children | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
 PAED-004 — [PAED-004 title, as seeded] | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
 PAED-005 — [PAED-005 title, as seeded] | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
 PAED-006 — [PAED-006 title, as seeded] | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
 PAED-007 — [PAED-007 title, as seeded] | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-009 — [PAED-009 title, as seeded] | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-010 — [PAED-010 title, as seeded] | 5 | 1 | 0 | 4 | 0 | 0 | 0 | 
 PAED-011 — [PAED-011 title, as seeded] | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
 PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 5 | 1 | 0 | 4 | 0 | 0 | 0 | 
 PAED-013 — [PAED-013 title, as seeded] | 5 | 0 | 0 | 0 | 0 | 0 | 5 | 
 PAED-014 — [PAED-014 title, as seeded] | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
 PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 5 | 0 | 0 | 5 | 0 | 0 | 0 | 
## Decisions , page 1 of 4 
 75 of 75 decisions due in 2026 S2 shown.
 Decisions due in 2026 S2, by trainee then EPA
 EPA | Trainee | Window | Status | Actions | 
 PAED-001 — Providing paediatric emergency care to children | Anele Dlamini | 2026 S2 | Decided STAR #4, issued at review #2 .
 | Decided: nothing to schedule 
 | 
 PAED-002 — [PAED-002 title, as seeded] | Anele Dlamini | 2026 S2 | Revoked: re-decide STAR #5, issued for 2026 S2, was revoked. Schedule a review to decide it again.
 | Schedule 
 | 
 PAED-003 — Providing intensive care to children | Anele Dlamini | 2026 | Not decided Not decided at review #2 . A later sitting for 2026 plans it again.
 | Schedule 
 | 
 PAED-004 — [PAED-004 title, as seeded] | Anele Dlamini | 2026 S2 | Not decided Not decided at review #2 . A later sitting for 2026 S2 plans it again.
 | Schedule 
 | 
 The other 11 rows of this page continue in the same form
 Showing 1–20 of 75
 Previous Next 
 Decisions per page
 10
 20
 50
 100


######## BOARD R2-3-decisions-due-390-j.dc.html — Decisions due · 390 · j
Decisions due · 390 · j 
# Decisions due · 390px (continued, part j) 
 Route /committee/decisions-due · the schedulers' list; a Committee member is refused.
 Components PageHeader, FilterBar (Show, Clear filters), DataTable (stack-wide), PagerControls (labelled), the .rung-legend fold with .fold-show / .fold-hide, StatePanel, Badge, AccessDenied. NEW the Trainee filter, .trainee-line (C2), .status-cell, .list-section-page, .list-section-count.
 DESIGN.md Decisions due's section; § Pager; § Table system. Round 3 the cadence (M1, M2), By EPA from the board's rows (M3), F1, F2, F6, M8, S1, S4, S5, S7.
=== FRAME: | Nothing due | | Pieter Smit · the form stands, so another period can be read | 
 [shell] 
# Decisions due
 What the committee must decide, per registrar and per EPA, and where each decision stands.
 Period
 2025 S1 · 1 Jan to 30 Jun 2025
 2025 S2 · 1 Jul to 31 Dec 2025
 2026 S1 · 1 Jan to 30 Jun 2026
 2026 S2 · 1 Jul to 31 Dec 2026
 2027 S1 · 1 Jan to 30 Jun 2027
 2027 S2 · 1 Jul to 31 Dec 2027
 The College's semester 2 ends in November; its window runs to 31 December, so December's work counts. 
 Trainee
 Every trainee
 Anele Dlamini
 Pieter du Plessis
 Nomsa Mahlangu
 Lerato Molefe
 Sipho Ndlovu
 Status
 Outstanding: not yet decided
 Every status
 Not scheduled
 Scheduled
 Decided
 Not decided
 Revoked: re-decide
 Due by year end
 Partial period
 As opportunity allows
 Missed
 EPA
 Every EPA
 PAED-001 — Providing paediatric emergency care to children
 PAED-002 — [PAED-002 title, as seeded]
 PAED-003 — Providing intensive care to children
 PAED-004 — [PAED-004 title, as seeded]
 PAED-005 — [PAED-005 title, as seeded]
 PAED-006 — [PAED-006 title, as seeded]
 PAED-007 — [PAED-007 title, as seeded]
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
 PAED-009 — [PAED-009 title, as seeded]
 PAED-010 — [PAED-010 title, as seeded]
 PAED-011 — [PAED-011 title, as seeded]
 PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
 PAED-013 — [PAED-013 title, as seeded]
 PAED-014 — [PAED-014 title, as seeded]
 PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
 Show 
 Nothing due for 2026 S2
 No trainee you oversee at Kgosi Kgari Teaching Hospital owes an entrustment decision in 2026 S2.
=== FRAME: | Loading | | C11, T329 · no Schedule or Open review until the read is in | 
 [shell] 
# Decisions due
 What the committee must decide, per registrar and per EPA, and where each decision stands.
 Loading Decisions due.
 Period
 2025 S1 · 1 Jan to 30 Jun 2025
 2025 S2 · 1 Jul to 31 Dec 2025
 2026 S1 · 1 Jan to 30 Jun 2026
 2026 S2 · 1 Jul to 31 Dec 2026
 2027 S1 · 1 Jan to 30 Jun 2027
 2027 S2 · 1 Jul to 31 Dec 2027
 The College's semester 2 ends in November; its window runs to 31 December, so December's work counts. 
 Trainee
 Every trainee
 Anele Dlamini
 Pieter du Plessis
 Nomsa Mahlangu
 Lerato Molefe
 Sipho Ndlovu
 Status
 Outstanding: not yet decided
 Every status
 Not scheduled
 Scheduled
 Decided
 Not decided
 Revoked: re-decide
 Due by year end
 Partial period
 As opportunity allows
 Missed
 EPA
 Every EPA
 PAED-001 — Providing paediatric emergency care to children
 PAED-002 — [PAED-002 title, as seeded]
 PAED-003 — Providing intensive care to children
 PAED-004 — [PAED-004 title, as seeded]
 PAED-005 — [PAED-005 title, as seeded]
 PAED-006 — [PAED-006 title, as seeded]
 PAED-007 — [PAED-007 title, as seeded]
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
 PAED-009 — [PAED-009 title, as seeded]
 PAED-010 — [PAED-010 title, as seeded]
 PAED-011 — [PAED-011 title, as seeded]
 PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
 PAED-013 — [PAED-013 title, as seeded]
 PAED-014 — [PAED-014 title, as seeded]
 PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
 Show 
## By EPA
## Decisions
=== FRAME: | Load error | | C11, T338 · fixed words for an unexpected failure; a handler's own sentence still shows (S4) | | Focus: the alert, once drawn | 
 [shell] 
# Decisions due
 What the committee must decide, per registrar and per EPA, and where each decision stands.
 Period
 2025 S1 · 1 Jan to 30 Jun 2025
 2025 S2 · 1 Jul to 31 Dec 2025
 2026 S1 · 1 Jan to 30 Jun 2026
 2026 S2 · 1 Jul to 31 Dec 2026
 2027 S1 · 1 Jan to 30 Jun 2027
 2027 S2 · 1 Jul to 31 Dec 2027
 The College's semester 2 ends in November; its window runs to 31 December, so December's work counts. 
 Trainee
 Every trainee
 Anele Dlamini
 Pieter du Plessis
 Nomsa Mahlangu
 Lerato Molefe
 Sipho Ndlovu
 Status
 Outstanding: not yet decided
 Every status
 Not scheduled
 Scheduled
 Decided
 Not decided
 Revoked: re-decide
 Due by year end
 Partial period
 As opportunity allows
 Missed
 EPA
 Every EPA
 PAED-001 — Providing paediatric emergency care to children
 PAED-002 — [PAED-002 title, as seeded]
 PAED-003 — Providing intensive care to children
 PAED-004 — [PAED-004 title, as seeded]
 PAED-005 — [PAED-005 title, as seeded]
 PAED-006 — [PAED-006 title, as seeded]
 PAED-007 — [PAED-007 title, as seeded]
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
 PAED-009 — [PAED-009 title, as seeded]
 PAED-010 — [PAED-010 title, as seeded]
 PAED-011 — [PAED-011 title, as seeded]
 PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
 PAED-013 — [PAED-013 title, as seeded]
 PAED-014 — [PAED-014 title, as seeded]
 PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
 Show 
 Could not load Decisions due. Nothing has changed. Try again, or come back in a few minutes. Try again 
=== FRAME: | Access denied: a committee member | | Step 4.5 · David Naidoo · flow 01's page; nothing lit | 
 [shell] 
# You cannot open this page
 None of your roles (Committee member, Assessor) opens this page.
 If you need it for your work, ask your institution's Wombat administrator.
 Go to Home


######## BOARD R2-4-schedule-1280-a.dc.html — Schedule a review · 1280 · a
Schedule a review · 1280 · a 
# Schedule a review · 1280px
 Route /committee/reviews/new?panel=&trainee=&period= · NEW owner table row: Committee reviews lit for the five schedulers.
 Components PageHeader, Breadcrumbs, FormField, FormActions, RefusalSummary, Alert, StatePanel, SittingBar, .scale-choices .form-check (Review type), .is-running (busy). NEW .schedule-grid (stacked below 1100px, M9), .agenda-preview with one live region .agenda-live, .field-pair.
 DESIGN.md § Page-level patterns, Form page; § Form system (Formative only folds into Review type; the type preselected, F4; the browser's date input, F3). Round 3 F1, F3, F4, F5, F6, M5, M9, S1, S4, S6.
=== FRAME: | Empty, from Committee reviews (C4) | | Step 4.6 · Pieter Smit · Trainee disabled until a panel is chosen; Review type preselected on the default (F4); Cancel back to Committee reviews | | Focus: the h1, on arrival (no ring: FocusOnNavigate) | 
 [shell] 
 Home 
 Committee reviews 
 Schedule a review 
# Schedule a review
 Put a trainee before a panel for a period. The review opens once it is created.
 Panel
 Choose a panel
 Paed Annual Review Panel
 Only the panels with a trainee you can schedule are listed. 
 Trainee
 Choose a panel first
 Choose a panel first. The list holds the trainees at its institution you can schedule. 
 Period
 Choose a period
 2025 S1 · 1 Jan to 30 Jun 2025
 2025 S2 · 1 Jul to 31 Dec 2025
 2026 S1 · 1 Jan to 30 Jun 2026
 2026 S2 · 1 Jul to 31 Dec 2026
 2027 S1 · 1 Jan to 30 Jun 2027
 2027 S2 · 1 Jul to 31 Dec 2027
 The semester the committee decides for. Choosing one fills the evidence window from the start of its academic year, so the annual EPAs a semester-2 sitting decides are judged on the whole year. 
 Evidence window
 From
 To
 Scheduled on
 The day the review is put on the schedule. 
 Review type
 Annual progression review The semester-2 sitting decides the trainee's progression for the year. 
 Pre-graduation review The final review before the programme ends: it decides the trainee's progression, and whether they may graduate. 
 Formative check-in Closes without a binding decision and cannot issue entrustment decisions. Use for interim progress check-ins. It takes no seat for the period. 
## Agenda
 Choose a panel, a trainee and a period to see which EPAs the review will be there to decide.
 Cancel Create review 
=== FRAME: | Filled from a Decisions due row | | Step 4.9 · Refilwe Mokoena · fills only what the form offers her; Cancel back to Decisions due with its filters; dates in the browser's date input, shown ISO elsewhere (F3) | | Focus: the h1, on arrival | 
 [shell] 
 Home 
 Committee reviews 
 Schedule a review 
# Schedule a review
 Put a trainee before a panel for a period. The review opens once it is created.
 Panel
 Choose a panel
 Paed Annual Review Panel
 Only the panels with a trainee you can schedule are listed. 
 Trainee
 Choose a trainee
 Anele Dlamini · Training year 3
 Pieter du Plessis · Training year 2
 Nomsa Mahlangu · Training year 1
 Lerato Molefe · Training year 4
 Sipho Ndlovu · Training year 1
 Current registrars at the panel's institution whose programme you oversee, with their training year. 
 Period
 Choose a period
 2025 S1 · 1 Jan to 30 Jun 2025
 2025 S2 · 1 Jul to 31 Dec 2025
 2026 S1 · 1 Jan to 30 Jun 2026
 2026 S2 · 1 Jul to 31 Dec 2026
 2027 S1 · 1 Jan to 30 Jun 2027
 2027 S2 · 1 Jul to 31 Dec 2027
 The semester the committee decides for. Choosing one fills the evidence window from the start of its academic year, so the annual EPAs a semester-2 sitting decides are judged on the whole year. The College's semester 2 ends in November; its window runs to 31 December, so December's work counts. 
 Evidence window
 From
 To
 Scheduled on
 The day the review is put on the schedule. 
 Review type
 Annual progression review The semester-2 sitting decides the trainee's progression for the year. 
 Pre-graduation review The final review before the programme ends: it decides the trainee's progression, and whether they may graduate. 
 Formative check-in Closes without a binding decision and cannot issue entrustment decisions. Use for interim progress check-ins. It takes no seat for the period. 
## Agenda
 15 EPAs will be on the agenda for 2026 S2.
 PAED-001, PAED-002, PAED-003, PAED-004, PAED-005, PAED-006, PAED-007, PAED-010, PAED-011, PAED-012, PAED-014 and PAED-015 are due at this sitting.
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children · As opportunity allows
 PAED-009 — [PAED-009 title, as seeded] · As opportunity allows
 PAED-013 — [PAED-013 title, as seeded] · As opportunity allows
 Cancel Create review 
=== FRAME: | The preview: 15 EPAs | | Step 4.6 · Pieter Smit · Annual progression review, preselected (F4) | 
 [shell] 
 Home 
 Committee reviews 
 Schedule a review 
# Schedule a review
 Put a trainee before a panel for a period. The review opens once it is created.
 Panel
 Choose a panel
 Paed Annual Review Panel
 Only the panels with a trainee you can schedule are listed. 
 Trainee
 Choose a trainee
 Anele Dlamini · Training year 3
 Pieter du Plessis · Training year 2
 Nomsa Mahlangu · Training year 1
 Lerato Molefe · Training year 4
 Sipho Ndlovu · Training year 1
 Current registrars at the panel's institution whose programme you oversee, with their training year. 
 Period
 Choose a period
 2025 S1 · 1 Jan to 30 Jun 2025
 2025 S2 · 1 Jul to 31 Dec 2025
 2026 S1 · 1 Jan to 30 Jun 2026
 2026 S2 · 1 Jul to 31 Dec 2026
 2027 S1 · 1 Jan to 30 Jun 2027
 2027 S2 · 1 Jul to 31 Dec 2027
 The semester the committee decides for. Choosing one fills the evidence window from the start of its academic year, so the annual EPAs a semester-2 sitting decides are judged on the whole year. The College's semester 2 ends in November; its window runs to 31 December, so December's work counts. 
 Evidence window
 From
 To
 Scheduled on
 The day the review is put on the schedule. 
 Review type
 Annual progression review The semester-2 sitting decides the trainee's progression for the year. 
 Pre-graduation review The final review before the programme ends: it decides the trainee's progression, and whether they may graduate. 
 Formative check-in Closes without a binding decision and cannot issue entrustment decisions. Use for interim progress check-ins. It takes no seat for the period. 
## Agenda
 15 EPAs will be on the agenda for 2026 S2.
 PAED-001, PAED-002, PAED-003, PAED-004, PAED-005, PAED-006, PAED-007, PAED-010, PAED-011, PAED-012, PAED-014 and PAED-015 are due at this sitting.
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children · As opportunity allows
 PAED-009 — [PAED-009 title, as seeded] · As opportunity allows
 PAED-013 — [PAED-013 title, as seeded] · As opportunity allows
 Cancel Create review 
=== FRAME: | Pre-graduation, with Already decided | | Step 5.2 · Pieter Smit · the window widened to her programme start (C1's help) | 
 [shell] 
 Home 
 Committee reviews 
 Schedule a review 
# Schedule a review
 Put a trainee before a panel for a period. The review opens once it is created.
 Panel
 Choose a panel
 Paed Annual Review Panel
 Only the panels with a trainee you can schedule are listed. 
 Trainee
 Choose a trainee
 Anele Dlamini · Training year 3
 Pieter du Plessis · Training year 2
 Nomsa Mahlangu · Training year 1
 Lerato Molefe · Training year 4
 Sipho Ndlovu · Training year 1
 Current registrars at the panel's institution whose programme you oversee, with their training year. 
 Period
 Choose a period
 2025 S1 · 1 Jan to 30 Jun 2025
 2025 S2 · 1 Jul to 31 Dec 2025
 2026 S1 · 1 Jan to 30 Jun 2026
 2026 S2 · 1 Jul to 31 Dec 2026
 2027 S1 · 1 Jan to 30 Jun 2027
 2027 S2 · 1 Jul to 31 Dec 2027
 The semester the committee decides for. Choosing one fills the evidence window from the start of its academic year, so the annual EPAs a semester-2 sitting decides are judged on the whole year. The College's semester 2 ends in November; its window runs to 31 December, so December's work counts. 
 Evidence window
 From
 To
 Scheduled on
 The day the review is put on the schedule. 
 Review type
 Annual progression review The semester-2 sitting decides the trainee's progression for the year. 
 Pre-graduation review The final review before the programme ends: it decides the trainee's progression, and whether they may graduate. 
 Formative check-in Closes without a binding decision and cannot issue entrustment decisions. Use for interim progress check-ins. It takes no seat for the period. 
## Agenda
 12 EPAs will be on the agenda for 2026 S2.
 PAED-002, PAED-003, PAED-004, PAED-005, PAED-006, PAED-007, PAED-011, PAED-014 and PAED-015 are due at this sitting.
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children · As opportunity allows
 PAED-009 — [PAED-009 title, as seeded] · As opportunity allows
 PAED-013 — [PAED-013 title, as seeded] · As opportunity allows
 Already decided in this window, so not on the agenda: PAED-001, PAED-010 and PAED-012.
 Cancel Create review


######## BOARD R2-4-schedule-1280-b.dc.html — Schedule a review · 1280 · b
Schedule a review · 1280 · b 
# Schedule a review · 1280px (continued, part b) 
 Route /committee/reviews/new?panel=&trainee=&period= · NEW owner table row: Committee reviews lit for the five schedulers.
 Components PageHeader, Breadcrumbs, FormField, FormActions, RefusalSummary, Alert, StatePanel, SittingBar, .scale-choices .form-check (Review type), .is-running (busy). NEW .schedule-grid (stacked below 1100px, M9), .agenda-preview with one live region .agenda-live, .field-pair.
 DESIGN.md § Page-level patterns, Form page; § Form system (Formative only folds into Review type; the type preselected, F4; the browser's date input, F3). Round 3 F1, F3, F4, F5, F6, M5, M9, S1, S4, S6.
=== FRAME: | The seat already held, before Create (Q3, T309) | | Step 4.8 checks this (F5) · Pieter Smit · Create review is not offered | | Focus: stays on the Period select; the warning is announced in the aside's live region | 
 [shell] 
 Home 
 Committee reviews 
 Schedule a review 
# Schedule a review
 Put a trainee before a panel for a period. The review opens once it is created.
 Panel
 Choose a panel
 Paed Annual Review Panel
 Only the panels with a trainee you can schedule are listed. 
 Trainee
 Choose a trainee
 Anele Dlamini · Training year 3
 Pieter du Plessis · Training year 2
 Nomsa Mahlangu · Training year 1
 Lerato Molefe · Training year 4
 Sipho Ndlovu · Training year 1
 Current registrars at the panel's institution whose programme you oversee, with their training year. 
 Period
 Choose a period
 2025 S1 · 1 Jan to 30 Jun 2025
 2025 S2 · 1 Jul to 31 Dec 2025
 2026 S1 · 1 Jan to 30 Jun 2026
 2026 S2 · 1 Jul to 31 Dec 2026
 2027 S1 · 1 Jan to 30 Jun 2027
 2027 S2 · 1 Jul to 31 Dec 2027
 The semester the committee decides for. Choosing one fills the evidence window from the start of its academic year, so the annual EPAs a semester-2 sitting decides are judged on the whole year. The College's semester 2 ends in November; its window runs to 31 December, so December's work counts. 
 Evidence window
 From
 To
 Scheduled on
 The day the review is put on the schedule. 
 Review type
 Annual progression review The semester-2 sitting decides the trainee's progression for the year. 
 Pre-graduation review The final review before the programme ends: it decides the trainee's progression, and whether they may graduate. 
 Formative check-in Closes without a binding decision and cannot issue entrustment decisions. Use for interim progress check-ins. It takes no seat for the period. 
## Agenda
 Already held. Review #1 already puts Lerato Molefe before Paed Annual Review Panel for 2026 S2 (scheduled on 2026-10-12). A trainee has one binding review for each period before the panels that decide the same EPAs, until it is ratified: open that review, or ratify it before scheduling another.
 A formative check-in takes no seat: choose Formative check-in under Review type to schedule one.
 Create review is not offered: review #1 already holds 2026 S2 for Lerato Molefe.
 Cancel Open review #1 
=== FRAME: | Formative check-in: no agenda, no seat | | Step 4.49 · Pieter Smit · accepted even where a binding review holds the period | 
 [shell] 
 Home 
 Committee reviews 
 Schedule a review 
# Schedule a review
 Put a trainee before a panel for a period. The review opens once it is created.
 Panel
 Choose a panel
 Paed Annual Review Panel
 Only the panels with a trainee you can schedule are listed. 
 Trainee
 Choose a trainee
 Anele Dlamini · Training year 3
 Pieter du Plessis · Training year 2
 Nomsa Mahlangu · Training year 1
 Lerato Molefe · Training year 4
 Sipho Ndlovu · Training year 1
 Current registrars at the panel's institution whose programme you oversee, with their training year. 
 Period
 Choose a period
 2025 S1 · 1 Jan to 30 Jun 2025
 2025 S2 · 1 Jul to 31 Dec 2025
 2026 S1 · 1 Jan to 30 Jun 2026
 2026 S2 · 1 Jul to 31 Dec 2026
 2027 S1 · 1 Jan to 30 Jun 2027
 2027 S2 · 1 Jul to 31 Dec 2027
 The semester the committee decides for. Choosing one fills the evidence window from the start of its academic year, so the annual EPAs a semester-2 sitting decides are judged on the whole year. The College's semester 2 ends in November; its window runs to 31 December, so December's work counts. 
 Evidence window
 From
 To
 Scheduled on
 The day the review is put on the schedule. 
 Review type
 Annual progression review The semester-2 sitting decides the trainee's progression for the year. 
 Pre-graduation review The final review before the programme ends: it decides the trainee's progression, and whether they may graduate. 
 Formative check-in Closes without a binding decision and cannot issue entrustment decisions. Use for interim progress check-ins. It takes no seat for the period. 
## Agenda
 A formative review carries no agenda: it decides no EPA.
 Cancel Create review 
=== FRAME: | From a Missed row of 2026 S1 (F1, C5) | | Step 4.2 · Nolwazi Mbatha · the form opens on the row's own ended semester, the only review that clears it; S1's types with their help (S1); the preview for 2026 S1 | 
 [shell] 
 Home 
 Committee reviews 
 Schedule a review 
# Schedule a review
 Put a trainee before a panel for a period. The review opens once it is created.
 Panel
 Choose a panel
 Paed Annual Review Panel
 Only the panels with a trainee you can schedule are listed. 
 Trainee
 Choose a trainee
 Anele Dlamini · Training year 3
 Pieter du Plessis · Training year 2
 Nomsa Mahlangu · Training year 1
 Lerato Molefe · Training year 4
 Sipho Ndlovu · Training year 1
 Current registrars at the panel's institution whose programme you oversee, with their training year. 
 Period
 Choose a period
 2025 S1 · 1 Jan to 30 Jun 2025
 2025 S2 · 1 Jul to 31 Dec 2025
 2026 S1 · 1 Jan to 30 Jun 2026
 2026 S2 · 1 Jul to 31 Dec 2026
 2027 S1 · 1 Jan to 30 Jun 2027
 2027 S2 · 1 Jul to 31 Dec 2027
 2026 S1 has ended. A review can still sit for it: sittings are often held after the period ends. The semester the committee decides for. Choosing one fills the evidence window from the start of its academic year, so the annual EPAs a semester-2 sitting decides are judged on the whole year. 
 Evidence window
 From
 To
 Scheduled on
 The day the review is put on the schedule. 
 Review type
 Entrustment-only review Decides entrustment only: a review before it records no progression category. 
 Annual progression review Decides the trainee's progression for the year. 
 Pre-graduation review The final review before the programme ends: it decides the trainee's progression, and whether they may graduate. 
 Formative check-in Closes without a binding decision and cannot issue entrustment decisions. Use for interim progress check-ins. It takes no seat for the period. 
## Agenda
 15 EPAs will be on the agenda for 2026 S1.
 PAED-001, PAED-002, PAED-004, PAED-005, PAED-010 and PAED-012 are due at this sitting.
 PAED-003 — Providing intensive care to children · Due by year end
 PAED-006 — [PAED-006 title, as seeded] · Due by year end
 PAED-007 — [PAED-007 title, as seeded] · Due by year end
 PAED-011 — [PAED-011 title, as seeded] · Due by year end
 PAED-014 — [PAED-014 title, as seeded] · Due by year end
 PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research · Due by year end
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children · As opportunity allows
 PAED-009 — [PAED-009 title, as seeded] · As opportunity allows
 PAED-013 — [PAED-013 title, as seeded] · As opportunity allows
 Cancel Create review 
=== FRAME: | Heavy: two panels; the general panel chosen (S1) | | Pieter Smit · the Panel select offers both; the preview names the panel that decides PAED-004 and 005 | 
 [shell] 
 Home 
 Committee reviews 
 Schedule a review 
# Schedule a review
 Put a trainee before a panel for a period. The review opens once it is created.
 Panel
 Choose a panel
 Paed Annual Review Panel
 [Neonatal panel]
 Only the panels with a trainee you can schedule are listed. 
 Trainee
 Choose a trainee
 Anele Dlamini · Training year 3
 Pieter du Plessis · Training year 2
 Nomsa Mahlangu · Training year 1
 Lerato Molefe · Training year 4
 Sipho Ndlovu · Training year 1
 Current registrars at the panel's institution whose programme you oversee, with their training year. 
 Period
 Choose a period
 2025 S1 · 1 Jan to 30 Jun 2025
 2025 S2 · 1 Jul to 31 Dec 2025
 2026 S1 · 1 Jan to 30 Jun 2026
 2026 S2 · 1 Jul to 31 Dec 2026
 2027 S1 · 1 Jan to 30 Jun 2027
 2027 S2 · 1 Jul to 31 Dec 2027
 The semester the committee decides for. Choosing one fills the evidence window from the start of its academic year, so the annual EPAs a semester-2 sitting decides are judged on the whole year. The College's semester 2 ends in November; its window runs to 31 December, so December's work counts. 
 Evidence window
 From
 To
 Scheduled on
 The day the review is put on the schedule. 
 Review type
 Annual progression review The semester-2 sitting decides the trainee's progression for the year. 
 Pre-graduation review The final review before the programme ends: it decides the trainee's progression, and whether they may graduate. 
 Formative check-in Closes without a binding decision and cannot issue entrustment decisions. Use for interim progress check-ins. It takes no seat for the period. 
## Agenda
 13 EPAs will be on the agenda for 2026 S2.
 PAED-001, PAED-002, PAED-003, PAED-006, PAED-007, PAED-010, PAED-011, PAED-012, PAED-014 and PAED-015 are due at this sitting.
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children · As opportunity allows
 PAED-009 — [PAED-009 title, as seeded] · As opportunity allows
 PAED-013 — [PAED-013 title, as seeded] · As opportunity allows
 PAED-004 and PAED-005 are decided by [Neonatal panel]: schedule them separately.
 Cancel Create review


######## BOARD R2-4-schedule-1280-c.dc.html — Schedule a review · 1280 · c
Schedule a review · 1280 · c 
# Schedule a review · 1280px (continued, part c) 
 Route /committee/reviews/new?panel=&trainee=&period= · NEW owner table row: Committee reviews lit for the five schedulers.
 Components PageHeader, Breadcrumbs, FormField, FormActions, RefusalSummary, Alert, StatePanel, SittingBar, .scale-choices .form-check (Review type), .is-running (busy). NEW .schedule-grid (stacked below 1100px, M9), .agenda-preview with one live region .agenda-live, .field-pair.
 DESIGN.md § Page-level patterns, Form page; § Form system (Formative only folds into Review type; the type preselected, F4; the browser's date input, F3). Round 3 F1, F3, F4, F5, F6, M5, M9, S1, S4, S6.
=== FRAME: | Heavy: the neonatal panel's one type (S1, F4) | | Pieter Smit · before the panel sitting as the neonatal committee, Entrustment-only is the one binding type, preselected | 
 [shell] 
 Home 
 Committee reviews 
 Schedule a review 
# Schedule a review
 Put a trainee before a panel for a period. The review opens once it is created.
 Panel
 Choose a panel
 Paed Annual Review Panel
 [Neonatal panel]
 Only the panels with a trainee you can schedule are listed. 
 Trainee
 Choose a trainee
 Anele Dlamini · Training year 3
 Pieter du Plessis · Training year 2
 Nomsa Mahlangu · Training year 1
 Lerato Molefe · Training year 4
 Sipho Ndlovu · Training year 1
 Current registrars at the panel's institution whose programme you oversee, with their training year. 
 Period
 Choose a period
 2025 S1 · 1 Jan to 30 Jun 2025
 2025 S2 · 1 Jul to 31 Dec 2025
 2026 S1 · 1 Jan to 30 Jun 2026
 2026 S2 · 1 Jul to 31 Dec 2026
 2027 S1 · 1 Jan to 30 Jun 2027
 2027 S2 · 1 Jul to 31 Dec 2027
 The semester the committee decides for. Choosing one fills the evidence window from the start of its academic year, so the annual EPAs a semester-2 sitting decides are judged on the whole year. The College's semester 2 ends in November; its window runs to 31 December, so December's work counts. 
 Evidence window
 From
 To
 Scheduled on
 The day the review is put on the schedule. 
 Review type
 Entrustment-only review [Neonatal panel] sits as the Neonatal team Clinical Competency Committee, which decides entrustment only: a review before it records no progression category. 
 Formative check-in Closes without a binding decision and cannot issue entrustment decisions. Use for interim progress check-ins. It takes no seat for the period. 
## Agenda
 2 EPAs will be on the agenda for 2026 S2.
 PAED-004 and PAED-005 are due at this sitting.
 Cancel Create review 
=== FRAME: | Current registrars only (5.29, S1) | | Pieter Smit · the Trainee list drawn open (a listbox here only to show its options): a graduate and a withdrawn registrar are not offered; each with the training year | 
 [shell] 
 Home 
 Committee reviews 
 Schedule a review 
# Schedule a review
 Put a trainee before a panel for a period. The review opens once it is created.
 Panel
 Choose a panel
 Paed Annual Review Panel
 Only the panels with a trainee you can schedule are listed. 
 Trainee
 Choose a trainee
 Anele Dlamini · Training year 3
 Nomsa Mahlangu · Training year 1
 Sipho Ndlovu · Training year 1
 Current registrars at the panel's institution whose programme you oversee, with their training year. 
 Period
 Choose a period
 2025 S1 · 1 Jan to 30 Jun 2025
 2025 S2 · 1 Jul to 31 Dec 2025
 2026 S1 · 1 Jan to 30 Jun 2026
 2026 S2 · 1 Jul to 31 Dec 2026
 2027 S1 · 1 Jan to 30 Jun 2027
 2027 S2 · 1 Jul to 31 Dec 2027
 The semester the committee decides for. Choosing one fills the evidence window from the start of its academic year, so the annual EPAs a semester-2 sitting decides are judged on the whole year. 
 Evidence window
 From
 To
 Scheduled on
 The day the review is put on the schedule. 
 Review type
 Annual progression review The semester-2 sitting decides the trainee's progression for the year. 
 Pre-graduation review The final review before the programme ends: it decides the trainee's progression, and whether they may graduate. 
 Formative check-in Closes without a binding decision and cannot issue entrustment decisions. Use for interim progress check-ins. It takes no seat for the period. 
## Agenda
 Choose a panel, a trainee and a period to see which EPAs the review will be there to decide.
 Cancel Create review 
=== FRAME: | NothingToDecide (S1) | | Pieter Smit · an entrustment-only review before a panel that decides no EPA on the trainee's curriculum: the preview says so first; Create is refused in the handler's own sentence | | Focus: the summary | 
 [shell] 
 Home 
 Committee reviews 
 Schedule a review 
# Schedule a review
 Put a trainee before a panel for a period. The review opens once it is created.
 Panel
 Choose a panel
 Paed Annual Review Panel
 [Neonatal panel]
 Only the panels with a trainee you can schedule are listed. 
 Trainee
 Choose a trainee
 Anele Dlamini · Training year 3
 Pieter du Plessis · Training year 2
 Nomsa Mahlangu · Training year 1
 Lerato Molefe · Training year 4
 Sipho Ndlovu · Training year 1
 Current registrars at the panel's institution whose programme you oversee, with their training year. 
 Period
 Choose a period
 2025 S1 · 1 Jan to 30 Jun 2025
 2025 S2 · 1 Jul to 31 Dec 2025
 2026 S1 · 1 Jan to 30 Jun 2026
 2026 S2 · 1 Jul to 31 Dec 2026
 2027 S1 · 1 Jan to 30 Jun 2027
 2027 S2 · 1 Jul to 31 Dec 2027
 The semester the committee decides for. Choosing one fills the evidence window from the start of its academic year, so the annual EPAs a semester-2 sitting decides are judged on the whole year. The College's semester 2 ends in November; its window runs to 31 December, so December's work counts. 
 Evidence window
 From
 To
 Scheduled on
 The day the review is put on the schedule. 
 Review type
 Entrustment-only review [Neonatal panel] sits as the Neonatal team Clinical Competency Committee, which decides entrustment only: a review before it records no progression category. 
 Formative check-in Closes without a binding decision and cannot issue entrustment decisions. Use for interim progress check-ins. It takes no seat for the period. 
## Agenda
 No EPA will be on the agenda.
 [Neonatal panel] decides no EPA of this trainee's curriculum, so the review would have nothing on its agenda.
 The review was not created. Everything you chose is kept below. Panel : [NothingToDecide: the handler's own sentence, as it ships]
 Cancel Create review 
=== FRAME: | The preview failed (C6, T338) | | Pieter Smit · Create stays offered: the preview is advice; the agenda is planned at Create (S4) | | Focus: stays where it was; the failure is announced in the aside's one live region | 
 [shell] 
 Home 
 Committee reviews 
 Schedule a review 
# Schedule a review
 Put a trainee before a panel for a period. The review opens once it is created.
 Panel
 Choose a panel
 Paed Annual Review Panel
 Only the panels with a trainee you can schedule are listed. 
 Trainee
 Choose a trainee
 Anele Dlamini · Training year 3
 Pieter du Plessis · Training year 2
 Nomsa Mahlangu · Training year 1
 Lerato Molefe · Training year 4
 Sipho Ndlovu · Training year 1
 Current registrars at the panel's institution whose programme you oversee, with their training year. 
 Period
 Choose a period
 2025 S1 · 1 Jan to 30 Jun 2025
 2025 S2 · 1 Jul to 31 Dec 2025
 2026 S1 · 1 Jan to 30 Jun 2026
 2026 S2 · 1 Jul to 31 Dec 2026
 2027 S1 · 1 Jan to 30 Jun 2027
 2027 S2 · 1 Jul to 31 Dec 2027
 The semester the committee decides for. Choosing one fills the evidence window from the start of its academic year, so the annual EPAs a semester-2 sitting decides are judged on the whole year. The College's semester 2 ends in November; its window runs to 31 December, so December's work counts. 
 Evidence window
 From
 To
 Scheduled on
 The day the review is put on the schedule. 
 Review type
 Annual progression review The semester-2 sitting decides the trainee's progression for the year. 
 Pre-graduation review The final review before the programme ends: it decides the trainee's progression, and whether they may graduate. 
 Formative check-in Closes without a binding decision and cannot issue entrustment decisions. Use for interim progress check-ins. It takes no seat for the period. 
## Agenda
 The agenda could not be read just now. Try again 
 The review can still be created: its agenda is planned when you press Create review.
 Cancel Create review


######## BOARD R2-4-schedule-1280-d.dc.html — Schedule a review · 1280 · d
Schedule a review · 1280 · d 
# Schedule a review · 1280px (continued, part d) 
 Route /committee/reviews/new?panel=&trainee=&period= · NEW owner table row: Committee reviews lit for the five schedulers.
 Components PageHeader, Breadcrumbs, FormField, FormActions, RefusalSummary, Alert, StatePanel, SittingBar, .scale-choices .form-check (Review type), .is-running (busy). NEW .schedule-grid (stacked below 1100px, M9), .agenda-preview with one live region .agenda-live, .field-pair.
 DESIGN.md § Page-level patterns, Form page; § Form system (Formative only folds into Review type; the type preselected, F4; the browser's date input, F3). Round 3 F1, F3, F4, F5, F6, M5, M9, S1, S4, S6.
=== FRAME: | A refusal on Create: missing fields (C7, T324) | | Pieter Smit · above the actions and beside each field; no line points at the disabled Trainee (S6); no type refusal (F4) | | Focus: the summary; each line moves it to its field | 
 [shell] 
 Home 
 Committee reviews 
 Schedule a review 
# Schedule a review
 Put a trainee before a panel for a period. The review opens once it is created.
 Panel
 Choose a panel
 Paed Annual Review Panel
 Choose a panel.
 Only the panels with a trainee you can schedule are listed. 
 Trainee
 Choose a panel first
 Choose a panel first. The list holds the trainees at its institution you can schedule. 
 Period
 Choose a period
 2025 S1 · 1 Jan to 30 Jun 2025
 2025 S2 · 1 Jul to 31 Dec 2025
 2026 S1 · 1 Jan to 30 Jun 2026
 2026 S2 · 1 Jul to 31 Dec 2026
 2027 S1 · 1 Jan to 30 Jun 2027
 2027 S2 · 1 Jul to 31 Dec 2027
 Choose the period the review sits for.
 The semester the committee decides for. Choosing one fills the evidence window from the start of its academic year, so the annual EPAs a semester-2 sitting decides are judged on the whole year. 
 Evidence window
 From
 To
 Scheduled on
 The day the review is put on the schedule. 
 Review type
 Annual progression review The semester-2 sitting decides the trainee's progression for the year. 
 Pre-graduation review The final review before the programme ends: it decides the trainee's progression, and whether they may graduate. 
 Formative check-in Closes without a binding decision and cannot issue entrustment decisions. Use for interim progress check-ins. It takes no seat for the period. 
## Agenda
 Choose a panel, a trainee and a period to see which EPAs the review will be there to decide.
 The review was not created. Everything you chose is kept below. Panel: Choose a panel. 
 Period: Choose the period the review sits for. 
 Cancel Create review 
=== FRAME: | A refusal on Create: the evidence window (S4) | | Pieter Smit · in Wombat's words; a missing From reads “Choose the day the window starts.” | | Focus: the summary | 
 [shell] 
 Home 
 Committee reviews 
 Schedule a review 
# Schedule a review
 Put a trainee before a panel for a period. The review opens once it is created.
 Panel
 Choose a panel
 Paed Annual Review Panel
 Only the panels with a trainee you can schedule are listed. 
 Trainee
 Choose a trainee
 Anele Dlamini · Training year 3
 Pieter du Plessis · Training year 2
 Nomsa Mahlangu · Training year 1
 Lerato Molefe · Training year 4
 Sipho Ndlovu · Training year 1
 Current registrars at the panel's institution whose programme you oversee, with their training year. 
 Period
 Choose a period
 2025 S1 · 1 Jan to 30 Jun 2025
 2025 S2 · 1 Jul to 31 Dec 2025
 2026 S1 · 1 Jan to 30 Jun 2026
 2026 S2 · 1 Jul to 31 Dec 2026
 2027 S1 · 1 Jan to 30 Jun 2027
 2027 S2 · 1 Jul to 31 Dec 2027
 The semester the committee decides for. Choosing one fills the evidence window from the start of its academic year, so the annual EPAs a semester-2 sitting decides are judged on the whole year. The College's semester 2 ends in November; its window runs to 31 December, so December's work counts. 
 Evidence window
 From
 To
 The window ends before it starts. Choose a day on or after 2026-01-01.
 Scheduled on
 The day the review is put on the schedule. 
 Review type
 Annual progression review The semester-2 sitting decides the trainee's progression for the year. 
 Pre-graduation review The final review before the programme ends: it decides the trainee's progression, and whether they may graduate. 
 Formative check-in Closes without a binding decision and cannot issue entrustment decisions. Use for interim progress check-ins. It takes no seat for the period. 
## Agenda
 15 EPAs will be on the agenda for 2026 S2.
 PAED-001, PAED-002, PAED-003, PAED-004, PAED-005, PAED-006, PAED-007, PAED-010, PAED-011, PAED-012, PAED-014 and PAED-015 are due at this sitting.
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children · As opportunity allows
 PAED-009 — [PAED-009 title, as seeded] · As opportunity allows
 PAED-013 — [PAED-013 title, as seeded] · As opportunity allows
 The review was not created. Everything you chose is kept below. Evidence window, To: The window ends before it starts. Choose a day on or after 2026-01-01. 
 Cancel Create review 
=== FRAME: | Scratch state: a second tab. The seat taken meanwhile (C7, F5) | | No runbook step plays it: handler and bUnit tests cover it · the summary links a short clause (S4); the preview turns to Already held | | Focus: the summary | 
 [shell] 
 Home 
 Committee reviews 
 Schedule a review 
# Schedule a review
 Put a trainee before a panel for a period. The review opens once it is created.
 Panel
 Choose a panel
 Paed Annual Review Panel
 Only the panels with a trainee you can schedule are listed. 
 Trainee
 Choose a trainee
 Anele Dlamini · Training year 3
 Pieter du Plessis · Training year 2
 Nomsa Mahlangu · Training year 1
 Lerato Molefe · Training year 4
 Sipho Ndlovu · Training year 1
 Current registrars at the panel's institution whose programme you oversee, with their training year. 
 Period
 Choose a period
 2025 S1 · 1 Jan to 30 Jun 2025
 2025 S2 · 1 Jul to 31 Dec 2025
 2026 S1 · 1 Jan to 30 Jun 2026
 2026 S2 · 1 Jul to 31 Dec 2026
 2027 S1 · 1 Jan to 30 Jun 2027
 2027 S2 · 1 Jul to 31 Dec 2027
 The semester the committee decides for. Choosing one fills the evidence window from the start of its academic year, so the annual EPAs a semester-2 sitting decides are judged on the whole year. The College's semester 2 ends in November; its window runs to 31 December, so December's work counts. 
 Evidence window
 From
 To
 Scheduled on
 The day the review is put on the schedule. 
 Review type
 Annual progression review The semester-2 sitting decides the trainee's progression for the year. 
 Pre-graduation review The final review before the programme ends: it decides the trainee's progression, and whether they may graduate. 
 Formative check-in Closes without a binding decision and cannot issue entrustment decisions. Use for interim progress check-ins. It takes no seat for the period. 
## Agenda
 Already held. Review #1 already puts Lerato Molefe before Paed Annual Review Panel for 2026 S2 (scheduled on 2026-10-12). A trainee has one binding review for each period before the panels that decide the same EPAs, until it is ratified: open that review, or ratify it before scheduling another.
 A formative check-in takes no seat: choose Formative check-in under Review type to schedule one.
 The review was not created. Everything you chose is kept below. Review #1 already puts Lerato Molefe before Paed Annual Review Panel for 2026 S2 (scheduled on 2026-10-12). A trainee has one binding review for each period before the panels that decide the same EPAs, until it is ratified: open that review, or ratify it before scheduling another.
 Open review #1 
 Create review is not offered: review #1 already holds 2026 S2 for Lerato Molefe.
 Cancel Open review #1 
=== FRAME: | Busy: Creating… | | Step 4.6 · .is-running with aria-disabled, the focus kept; Cancel a disabled button, not a followable link (S6) | | Focus: stays on Creating… | 
 [shell] 
 Home 
 Committee reviews 
 Schedule a review 
# Schedule a review
 Put a trainee before a panel for a period. The review opens once it is created.
 Panel
 Choose a panel
 Paed Annual Review Panel
 Only the panels with a trainee you can schedule are listed. 
 Trainee
 Choose a trainee
 Anele Dlamini · Training year 3
 Pieter du Plessis · Training year 2
 Nomsa Mahlangu · Training year 1
 Lerato Molefe · Training year 4
 Sipho Ndlovu · Training year 1
 Current registrars at the panel's institution whose programme you oversee, with their training year. 
 Period
 Choose a period
 2025 S1 · 1 Jan to 30 Jun 2025
 2025 S2 · 1 Jul to 31 Dec 2025
 2026 S1 · 1 Jan to 30 Jun 2026
 2026 S2 · 1 Jul to 31 Dec 2026
 2027 S1 · 1 Jan to 30 Jun 2027
 2027 S2 · 1 Jul to 31 Dec 2027
 The semester the committee decides for. Choosing one fills the evidence window from the start of its academic year, so the annual EPAs a semester-2 sitting decides are judged on the whole year. The College's semester 2 ends in November; its window runs to 31 December, so December's work counts. 
 Evidence window
 From
 To
 Scheduled on
 The day the review is put on the schedule. 
 Review type
 Annual progression review The semester-2 sitting decides the trainee's progression for the year. 
 Pre-graduation review The final review before the programme ends: it decides the trainee's progression, and whether they may graduate. 
 Formative check-in Closes without a binding decision and cannot issue entrustment decisions. Use for interim progress check-ins. It takes no seat for the period. 
## Agenda
 15 EPAs will be on the agenda for 2026 S2.
 PAED-001, PAED-002, PAED-003, PAED-004, PAED-005, PAED-006, PAED-007, PAED-010, PAED-011, PAED-012, PAED-014 and PAED-015 are due at this sitting.
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children · As opportunity allows
 PAED-009 — [PAED-009 title, as seeded] · As opportunity allows
 PAED-013 — [PAED-013 title, as seeded] · As opportunity allows
 Cancel Creating… 
=== FRAME: | Created: the review opens (flow 07's page, its top only) | | Step 5.2 · Pieter Smit | | Focus: its h1 | 
 [shell] 
 Home 
 Committee reviews 
 Lerato Molefe · 2026 S2 
# Lerato Molefe · 2026 S2
 Pre-graduation review · Paed Annual Review Panel · Training year 4
 Lerato Molefe's progress 
## The sitting
 Scheduled 12 EPAs on the agenda Summative · evidence 2023-01-15 to 2026-12-31 
 As of 10:42 SAST
 Refresh Start review… 
 The rest is flow 07's review page, unchanged


######## BOARD R2-4-schedule-1280-e.dc.html — Schedule a review · 1280 · e
Schedule a review · 1280 · e 
# Schedule a review · 1280px (continued, part e) 
 Route /committee/reviews/new?panel=&trainee=&period= · NEW owner table row: Committee reviews lit for the five schedulers.
 Components PageHeader, Breadcrumbs, FormField, FormActions, RefusalSummary, Alert, StatePanel, SittingBar, .scale-choices .form-check (Review type), .is-running (busy). NEW .schedule-grid (stacked below 1100px, M9), .agenda-preview with one live region .agenda-live, .field-pair.
 DESIGN.md § Page-level patterns, Form page; § Form system (Formative only folds into Review type; the type preselected, F4; the browser's date input, F3). Round 3 F1, F3, F4, F5, F6, M5, M9, S1, S4, S6.
=== FRAME: | Loading | | C11 · Create is not offered until the read is in | 
 [shell] 
 Home 
 Committee reviews 
 Schedule a review 
# Schedule a review
 Put a trainee before a panel for a period. The review opens once it is created.
 Loading what you can schedule.
## Agenda
 Cancel 
=== FRAME: | Load error | | C11, T338 | | Focus: the alert | 
 [shell] 
 Home 
 Committee reviews 
 Schedule a review 
# Schedule a review
 Put a trainee before a panel for a period. The review opens once it is created.
 Could not load what you can schedule. Nothing has changed. Try again, or come back in a few minutes. Try again


######## BOARD R2-4-schedule-768.dc.html — Schedule a review · 768
Schedule a review · 768 
# Schedule a review · 768px
 Route /committee/reviews/new?panel=&trainee=&period= · NEW owner table row: Committee reviews lit for the five schedulers.
 Components PageHeader, Breadcrumbs, FormField, FormActions, RefusalSummary, Alert, StatePanel, SittingBar, .scale-choices .form-check (Review type), .is-running (busy). NEW .schedule-grid (stacked below 1100px, M9), .agenda-preview with one live region .agenda-live, .field-pair.
 DESIGN.md § Page-level patterns, Form page; § Form system (Formative only folds into Review type; the type preselected, F4; the browser's date input, F3). Round 3 F1, F3, F4, F5, F6, M5, M9, S1, S4, S6.
=== FRAME: | Filled from a Decisions due row | | Step 4.9 · Refilwe Mokoena · fills only what the form offers her; Cancel back to Decisions due with its filters; dates in the browser's date input, shown ISO elsewhere (F3) | | Focus: the h1, on arrival | 
 [shell] 
 Home 
 Committee reviews 
 Schedule a review 
# Schedule a review
 Put a trainee before a panel for a period. The review opens once it is created.
 Panel
 Choose a panel
 Paed Annual Review Panel
 Only the panels with a trainee you can schedule are listed. 
 Trainee
 Choose a trainee
 Anele Dlamini · Training year 3
 Pieter du Plessis · Training year 2
 Nomsa Mahlangu · Training year 1
 Lerato Molefe · Training year 4
 Sipho Ndlovu · Training year 1
 Current registrars at the panel's institution whose programme you oversee, with their training year. 
 Period
 Choose a period
 2025 S1 · 1 Jan to 30 Jun 2025
 2025 S2 · 1 Jul to 31 Dec 2025
 2026 S1 · 1 Jan to 30 Jun 2026
 2026 S2 · 1 Jul to 31 Dec 2026
 2027 S1 · 1 Jan to 30 Jun 2027
 2027 S2 · 1 Jul to 31 Dec 2027
 The semester the committee decides for. Choosing one fills the evidence window from the start of its academic year, so the annual EPAs a semester-2 sitting decides are judged on the whole year. The College's semester 2 ends in November; its window runs to 31 December, so December's work counts. 
 Evidence window
 From
 To
 Scheduled on
 The day the review is put on the schedule. 
 Review type
 Annual progression review The semester-2 sitting decides the trainee's progression for the year. 
 Pre-graduation review The final review before the programme ends: it decides the trainee's progression, and whether they may graduate. 
 Formative check-in Closes without a binding decision and cannot issue entrustment decisions. Use for interim progress check-ins. It takes no seat for the period. 
## Agenda
 15 EPAs will be on the agenda for 2026 S2.
 PAED-001, PAED-002, PAED-003, PAED-004, PAED-005, PAED-006, PAED-007, PAED-010, PAED-011, PAED-012, PAED-014 and PAED-015 are due at this sitting.
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children · As opportunity allows
 PAED-009 — [PAED-009 title, as seeded] · As opportunity allows
 PAED-013 — [PAED-013 title, as seeded] · As opportunity allows
 Cancel Create review 
=== FRAME: | The seat already held, before Create (Q3, T309) | | Step 4.8 checks this (F5) · Pieter Smit · Create review is not offered | | Focus: stays on the Period select; the warning is announced in the aside's live region | 
 [shell] 
 Home 
 Committee reviews 
 Schedule a review 
# Schedule a review
 Put a trainee before a panel for a period. The review opens once it is created.
 Panel
 Choose a panel
 Paed Annual Review Panel
 Only the panels with a trainee you can schedule are listed. 
 Trainee
 Choose a trainee
 Anele Dlamini · Training year 3
 Pieter du Plessis · Training year 2
 Nomsa Mahlangu · Training year 1
 Lerato Molefe · Training year 4
 Sipho Ndlovu · Training year 1
 Current registrars at the panel's institution whose programme you oversee, with their training year. 
 Period
 Choose a period
 2025 S1 · 1 Jan to 30 Jun 2025
 2025 S2 · 1 Jul to 31 Dec 2025
 2026 S1 · 1 Jan to 30 Jun 2026
 2026 S2 · 1 Jul to 31 Dec 2026
 2027 S1 · 1 Jan to 30 Jun 2027
 2027 S2 · 1 Jul to 31 Dec 2027
 The semester the committee decides for. Choosing one fills the evidence window from the start of its academic year, so the annual EPAs a semester-2 sitting decides are judged on the whole year. The College's semester 2 ends in November; its window runs to 31 December, so December's work counts. 
 Evidence window
 From
 To
 Scheduled on
 The day the review is put on the schedule. 
 Review type
 Annual progression review The semester-2 sitting decides the trainee's progression for the year. 
 Pre-graduation review The final review before the programme ends: it decides the trainee's progression, and whether they may graduate. 
 Formative check-in Closes without a binding decision and cannot issue entrustment decisions. Use for interim progress check-ins. It takes no seat for the period. 
## Agenda
 Already held. Review #1 already puts Lerato Molefe before Paed Annual Review Panel for 2026 S2 (scheduled on 2026-10-12). A trainee has one binding review for each period before the panels that decide the same EPAs, until it is ratified: open that review, or ratify it before scheduling another.
 A formative check-in takes no seat: choose Formative check-in under Review type to schedule one.
 Create review is not offered: review #1 already holds 2026 S2 for Lerato Molefe.
 Cancel Open review #1


######## BOARD R2-4-schedule-1024.dc.html — Schedule a review · 1024
Schedule a review · 1024 
# Schedule a review · 1024px
 Route /committee/reviews/new?panel=&trainee=&period= · NEW owner table row: Committee reviews lit for the five schedulers.
 Components PageHeader, Breadcrumbs, FormField, FormActions, RefusalSummary, Alert, StatePanel, SittingBar, .scale-choices .form-check (Review type), .is-running (busy). NEW .schedule-grid (stacked below 1100px, M9), .agenda-preview with one live region .agenda-live, .field-pair.
 DESIGN.md § Page-level patterns, Form page; § Form system (Formative only folds into Review type; the type preselected, F4; the browser's date input, F3). Round 3 F1, F3, F4, F5, F6, M5, M9, S1, S4, S6.
=== FRAME: | Filled from a Decisions due row | | Step 4.9 · Refilwe Mokoena · fills only what the form offers her; Cancel back to Decisions due with its filters; dates in the browser's date input, shown ISO elsewhere (F3) | | Focus: the h1, on arrival | 
 [shell] 
 Home 
 Committee reviews 
 Schedule a review 
# Schedule a review
 Put a trainee before a panel for a period. The review opens once it is created.
 Panel
 Choose a panel
 Paed Annual Review Panel
 Only the panels with a trainee you can schedule are listed. 
 Trainee
 Choose a trainee
 Anele Dlamini · Training year 3
 Pieter du Plessis · Training year 2
 Nomsa Mahlangu · Training year 1
 Lerato Molefe · Training year 4
 Sipho Ndlovu · Training year 1
 Current registrars at the panel's institution whose programme you oversee, with their training year. 
 Period
 Choose a period
 2025 S1 · 1 Jan to 30 Jun 2025
 2025 S2 · 1 Jul to 31 Dec 2025
 2026 S1 · 1 Jan to 30 Jun 2026
 2026 S2 · 1 Jul to 31 Dec 2026
 2027 S1 · 1 Jan to 30 Jun 2027
 2027 S2 · 1 Jul to 31 Dec 2027
 The semester the committee decides for. Choosing one fills the evidence window from the start of its academic year, so the annual EPAs a semester-2 sitting decides are judged on the whole year. The College's semester 2 ends in November; its window runs to 31 December, so December's work counts. 
 Evidence window
 From
 To
 Scheduled on
 The day the review is put on the schedule. 
 Review type
 Annual progression review The semester-2 sitting decides the trainee's progression for the year. 
 Pre-graduation review The final review before the programme ends: it decides the trainee's progression, and whether they may graduate. 
 Formative check-in Closes without a binding decision and cannot issue entrustment decisions. Use for interim progress check-ins. It takes no seat for the period. 
## Agenda
 15 EPAs will be on the agenda for 2026 S2.
 PAED-001, PAED-002, PAED-003, PAED-004, PAED-005, PAED-006, PAED-007, PAED-010, PAED-011, PAED-012, PAED-014 and PAED-015 are due at this sitting.
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children · As opportunity allows
 PAED-009 — [PAED-009 title, as seeded] · As opportunity allows
 PAED-013 — [PAED-013 title, as seeded] · As opportunity allows
 Cancel Create review 
=== FRAME: | From a Missed row of 2026 S1 (F1, C5) | | Step 4.2 · Nolwazi Mbatha · the form opens on the row's own ended semester, the only review that clears it; S1's types with their help (S1); the preview for 2026 S1 | 
 [shell] 
 Home 
 Committee reviews 
 Schedule a review 
# Schedule a review
 Put a trainee before a panel for a period. The review opens once it is created.
 Panel
 Choose a panel
 Paed Annual Review Panel
 Only the panels with a trainee you can schedule are listed. 
 Trainee
 Choose a trainee
 Anele Dlamini · Training year 3
 Pieter du Plessis · Training year 2
 Nomsa Mahlangu · Training year 1
 Lerato Molefe · Training year 4
 Sipho Ndlovu · Training year 1
 Current registrars at the panel's institution whose programme you oversee, with their training year. 
 Period
 Choose a period
 2025 S1 · 1 Jan to 30 Jun 2025
 2025 S2 · 1 Jul to 31 Dec 2025
 2026 S1 · 1 Jan to 30 Jun 2026
 2026 S2 · 1 Jul to 31 Dec 2026
 2027 S1 · 1 Jan to 30 Jun 2027
 2027 S2 · 1 Jul to 31 Dec 2027
 2026 S1 has ended. A review can still sit for it: sittings are often held after the period ends. The semester the committee decides for. Choosing one fills the evidence window from the start of its academic year, so the annual EPAs a semester-2 sitting decides are judged on the whole year. 
 Evidence window
 From
 To
 Scheduled on
 The day the review is put on the schedule. 
 Review type
 Entrustment-only review Decides entrustment only: a review before it records no progression category. 
 Annual progression review Decides the trainee's progression for the year. 
 Pre-graduation review The final review before the programme ends: it decides the trainee's progression, and whether they may graduate. 
 Formative check-in Closes without a binding decision and cannot issue entrustment decisions. Use for interim progress check-ins. It takes no seat for the period. 
## Agenda
 15 EPAs will be on the agenda for 2026 S1.
 PAED-001, PAED-002, PAED-004, PAED-005, PAED-010 and PAED-012 are due at this sitting.
 PAED-003 — Providing intensive care to children · Due by year end
 PAED-006 — [PAED-006 title, as seeded] · Due by year end
 PAED-007 — [PAED-007 title, as seeded] · Due by year end
 PAED-011 — [PAED-011 title, as seeded] · Due by year end
 PAED-014 — [PAED-014 title, as seeded] · Due by year end
 PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research · Due by year end
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children · As opportunity allows
 PAED-009 — [PAED-009 title, as seeded] · As opportunity allows
 PAED-013 — [PAED-013 title, as seeded] · As opportunity allows
 Cancel Create review


######## BOARD R2-4-schedule-390-a.dc.html — Schedule a review · 390 · a
Schedule a review · 390 · a 
# Schedule a review · 390px
 Route /committee/reviews/new?panel=&trainee=&period= · NEW owner table row: Committee reviews lit for the five schedulers.
 Components PageHeader, Breadcrumbs, FormField, FormActions, RefusalSummary, Alert, StatePanel, SittingBar, .scale-choices .form-check (Review type), .is-running (busy). NEW .schedule-grid (stacked below 1100px, M9), .agenda-preview with one live region .agenda-live, .field-pair.
 DESIGN.md § Page-level patterns, Form page; § Form system (Formative only folds into Review type; the type preselected, F4; the browser's date input, F3). Round 3 F1, F3, F4, F5, F6, M5, M9, S1, S4, S6.
=== FRAME: | Empty, from Committee reviews (C4) | | Step 4.6 · Pieter Smit · Trainee disabled until a panel is chosen; Review type preselected on the default (F4); Cancel back to Committee reviews | | Focus: the h1, on arrival (no ring: FocusOnNavigate) | 
 [shell] 
 Home 
 Committee reviews 
 Schedule a review 
# Schedule a review
 Put a trainee before a panel for a period. The review opens once it is created.
 Panel
 Choose a panel
 Paed Annual Review Panel
 Only the panels with a trainee you can schedule are listed. 
 Trainee
 Choose a panel first
 Choose a panel first. The list holds the trainees at its institution you can schedule. 
 Period
 Choose a period
 2025 S1 · 1 Jan to 30 Jun 2025
 2025 S2 · 1 Jul to 31 Dec 2025
 2026 S1 · 1 Jan to 30 Jun 2026
 2026 S2 · 1 Jul to 31 Dec 2026
 2027 S1 · 1 Jan to 30 Jun 2027
 2027 S2 · 1 Jul to 31 Dec 2027
 The semester the committee decides for. Choosing one fills the evidence window from the start of its academic year, so the annual EPAs a semester-2 sitting decides are judged on the whole year. 
 Evidence window
 From
 To
 Scheduled on
 The day the review is put on the schedule. 
 Review type
 Annual progression review The semester-2 sitting decides the trainee's progression for the year. 
 Pre-graduation review The final review before the programme ends: it decides the trainee's progression, and whether they may graduate. 
 Formative check-in Closes without a binding decision and cannot issue entrustment decisions. Use for interim progress check-ins. It takes no seat for the period. 
## Agenda
 Choose a panel, a trainee and a period to see which EPAs the review will be there to decide.
 Cancel Create review 
=== FRAME: | Filled from a Decisions due row | | Step 4.9 · Refilwe Mokoena · fills only what the form offers her; Cancel back to Decisions due with its filters; dates in the browser's date input, shown ISO elsewhere (F3) | | Focus: the h1, on arrival | 
 [shell] 
 Home 
 Committee reviews 
 Schedule a review 
# Schedule a review
 Put a trainee before a panel for a period. The review opens once it is created.
 Panel
 Choose a panel
 Paed Annual Review Panel
 Only the panels with a trainee you can schedule are listed. 
 Trainee
 Choose a trainee
 Anele Dlamini · Training year 3
 Pieter du Plessis · Training year 2
 Nomsa Mahlangu · Training year 1
 Lerato Molefe · Training year 4
 Sipho Ndlovu · Training year 1
 Current registrars at the panel's institution whose programme you oversee, with their training year. 
 Period
 Choose a period
 2025 S1 · 1 Jan to 30 Jun 2025
 2025 S2 · 1 Jul to 31 Dec 2025
 2026 S1 · 1 Jan to 30 Jun 2026
 2026 S2 · 1 Jul to 31 Dec 2026
 2027 S1 · 1 Jan to 30 Jun 2027
 2027 S2 · 1 Jul to 31 Dec 2027
 The semester the committee decides for. Choosing one fills the evidence window from the start of its academic year, so the annual EPAs a semester-2 sitting decides are judged on the whole year. The College's semester 2 ends in November; its window runs to 31 December, so December's work counts. 
 Evidence window
 From
 To
 Scheduled on
 The day the review is put on the schedule. 
 Review type
 Annual progression review The semester-2 sitting decides the trainee's progression for the year. 
 Pre-graduation review The final review before the programme ends: it decides the trainee's progression, and whether they may graduate. 
 Formative check-in Closes without a binding decision and cannot issue entrustment decisions. Use for interim progress check-ins. It takes no seat for the period. 
## Agenda
 15 EPAs will be on the agenda for 2026 S2.
 PAED-001, PAED-002, PAED-003, PAED-004, PAED-005, PAED-006, PAED-007, PAED-010, PAED-011, PAED-012, PAED-014 and PAED-015 are due at this sitting.
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children · As opportunity allows
 PAED-009 — [PAED-009 title, as seeded] · As opportunity allows
 PAED-013 — [PAED-013 title, as seeded] · As opportunity allows
 Cancel Create review 
=== FRAME: | The preview: 15 EPAs | | Step 4.6 · Pieter Smit · Annual progression review, preselected (F4) | 
 [shell] 
 Home 
 Committee reviews 
 Schedule a review 
# Schedule a review
 Put a trainee before a panel for a period. The review opens once it is created.
 Panel
 Choose a panel
 Paed Annual Review Panel
 Only the panels with a trainee you can schedule are listed. 
 Trainee
 Choose a trainee
 Anele Dlamini · Training year 3
 Pieter du Plessis · Training year 2
 Nomsa Mahlangu · Training year 1
 Lerato Molefe · Training year 4
 Sipho Ndlovu · Training year 1
 Current registrars at the panel's institution whose programme you oversee, with their training year. 
 Period
 Choose a period
 2025 S1 · 1 Jan to 30 Jun 2025
 2025 S2 · 1 Jul to 31 Dec 2025
 2026 S1 · 1 Jan to 30 Jun 2026
 2026 S2 · 1 Jul to 31 Dec 2026
 2027 S1 · 1 Jan to 30 Jun 2027
 2027 S2 · 1 Jul to 31 Dec 2027
 The semester the committee decides for. Choosing one fills the evidence window from the start of its academic year, so the annual EPAs a semester-2 sitting decides are judged on the whole year. The College's semester 2 ends in November; its window runs to 31 December, so December's work counts. 
 Evidence window
 From
 To
 Scheduled on
 The day the review is put on the schedule. 
 Review type
 Annual progression review The semester-2 sitting decides the trainee's progression for the year. 
 Pre-graduation review The final review before the programme ends: it decides the trainee's progression, and whether they may graduate. 
 Formative check-in Closes without a binding decision and cannot issue entrustment decisions. Use for interim progress check-ins. It takes no seat for the period. 
## Agenda
 15 EPAs will be on the agenda for 2026 S2.
 PAED-001, PAED-002, PAED-003, PAED-004, PAED-005, PAED-006, PAED-007, PAED-010, PAED-011, PAED-012, PAED-014 and PAED-015 are due at this sitting.
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children · As opportunity allows
 PAED-009 — [PAED-009 title, as seeded] · As opportunity allows
 PAED-013 — [PAED-013 title, as seeded] · As opportunity allows
 Cancel Create review


######## BOARD R2-4-schedule-390-b.dc.html — Schedule a review · 390 · b
Schedule a review · 390 · b 
# Schedule a review · 390px (continued, part b) 
 Route /committee/reviews/new?panel=&trainee=&period= · NEW owner table row: Committee reviews lit for the five schedulers.
 Components PageHeader, Breadcrumbs, FormField, FormActions, RefusalSummary, Alert, StatePanel, SittingBar, .scale-choices .form-check (Review type), .is-running (busy). NEW .schedule-grid (stacked below 1100px, M9), .agenda-preview with one live region .agenda-live, .field-pair.
 DESIGN.md § Page-level patterns, Form page; § Form system (Formative only folds into Review type; the type preselected, F4; the browser's date input, F3). Round 3 F1, F3, F4, F5, F6, M5, M9, S1, S4, S6.
=== FRAME: | Pre-graduation, with Already decided | | Step 5.2 · Pieter Smit · the window widened to her programme start (C1's help) | 
 [shell] 
 Home 
 Committee reviews 
 Schedule a review 
# Schedule a review
 Put a trainee before a panel for a period. The review opens once it is created.
 Panel
 Choose a panel
 Paed Annual Review Panel
 Only the panels with a trainee you can schedule are listed. 
 Trainee
 Choose a trainee
 Anele Dlamini · Training year 3
 Pieter du Plessis · Training year 2
 Nomsa Mahlangu · Training year 1
 Lerato Molefe · Training year 4
 Sipho Ndlovu · Training year 1
 Current registrars at the panel's institution whose programme you oversee, with their training year. 
 Period
 Choose a period
 2025 S1 · 1 Jan to 30 Jun 2025
 2025 S2 · 1 Jul to 31 Dec 2025
 2026 S1 · 1 Jan to 30 Jun 2026
 2026 S2 · 1 Jul to 31 Dec 2026
 2027 S1 · 1 Jan to 30 Jun 2027
 2027 S2 · 1 Jul to 31 Dec 2027
 The semester the committee decides for. Choosing one fills the evidence window from the start of its academic year, so the annual EPAs a semester-2 sitting decides are judged on the whole year. The College's semester 2 ends in November; its window runs to 31 December, so December's work counts. 
 Evidence window
 From
 To
 Scheduled on
 The day the review is put on the schedule. 
 Review type
 Annual progression review The semester-2 sitting decides the trainee's progression for the year. 
 Pre-graduation review The final review before the programme ends: it decides the trainee's progression, and whether they may graduate. 
 Formative check-in Closes without a binding decision and cannot issue entrustment decisions. Use for interim progress check-ins. It takes no seat for the period. 
## Agenda
 12 EPAs will be on the agenda for 2026 S2.
 PAED-002, PAED-003, PAED-004, PAED-005, PAED-006, PAED-007, PAED-011, PAED-014 and PAED-015 are due at this sitting.
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children · As opportunity allows
 PAED-009 — [PAED-009 title, as seeded] · As opportunity allows
 PAED-013 — [PAED-013 title, as seeded] · As opportunity allows
 Already decided in this window, so not on the agenda: PAED-001, PAED-010 and PAED-012.
 Cancel Create review 
=== FRAME: | The seat already held, before Create (Q3, T309) | | Step 4.8 checks this (F5) · Pieter Smit · Create review is not offered | | Focus: stays on the Period select; the warning is announced in the aside's live region | 
 [shell] 
 Home 
 Committee reviews 
 Schedule a review 
# Schedule a review
 Put a trainee before a panel for a period. The review opens once it is created.
 Panel
 Choose a panel
 Paed Annual Review Panel
 Only the panels with a trainee you can schedule are listed. 
 Trainee
 Choose a trainee
 Anele Dlamini · Training year 3
 Pieter du Plessis · Training year 2
 Nomsa Mahlangu · Training year 1
 Lerato Molefe · Training year 4
 Sipho Ndlovu · Training year 1
 Current registrars at the panel's institution whose programme you oversee, with their training year. 
 Period
 Choose a period
 2025 S1 · 1 Jan to 30 Jun 2025
 2025 S2 · 1 Jul to 31 Dec 2025
 2026 S1 · 1 Jan to 30 Jun 2026
 2026 S2 · 1 Jul to 31 Dec 2026
 2027 S1 · 1 Jan to 30 Jun 2027
 2027 S2 · 1 Jul to 31 Dec 2027
 The semester the committee decides for. Choosing one fills the evidence window from the start of its academic year, so the annual EPAs a semester-2 sitting decides are judged on the whole year. The College's semester 2 ends in November; its window runs to 31 December, so December's work counts. 
 Evidence window
 From
 To
 Scheduled on
 The day the review is put on the schedule. 
 Review type
 Annual progression review The semester-2 sitting decides the trainee's progression for the year. 
 Pre-graduation review The final review before the programme ends: it decides the trainee's progression, and whether they may graduate. 
 Formative check-in Closes without a binding decision and cannot issue entrustment decisions. Use for interim progress check-ins. It takes no seat for the period. 
## Agenda
 Already held. Review #1 already puts Lerato Molefe before Paed Annual Review Panel for 2026 S2 (scheduled on 2026-10-12). A trainee has one binding review for each period before the panels that decide the same EPAs, until it is ratified: open that review, or ratify it before scheduling another.
 A formative check-in takes no seat: choose Formative check-in under Review type to schedule one.
 Create review is not offered: review #1 already holds 2026 S2 for Lerato Molefe.
 Cancel Open review #1 
=== FRAME: | Formative check-in: no agenda, no seat | | Step 4.49 · Pieter Smit · accepted even where a binding review holds the period | 
 [shell] 
 Home 
 Committee reviews 
 Schedule a review 
# Schedule a review
 Put a trainee before a panel for a period. The review opens once it is created.
 Panel
 Choose a panel
 Paed Annual Review Panel
 Only the panels with a trainee you can schedule are listed. 
 Trainee
 Choose a trainee
 Anele Dlamini · Training year 3
 Pieter du Plessis · Training year 2
 Nomsa Mahlangu · Training year 1
 Lerato Molefe · Training year 4
 Sipho Ndlovu · Training year 1
 Current registrars at the panel's institution whose programme you oversee, with their training year. 
 Period
 Choose a period
 2025 S1 · 1 Jan to 30 Jun 2025
 2025 S2 · 1 Jul to 31 Dec 2025
 2026 S1 · 1 Jan to 30 Jun 2026
 2026 S2 · 1 Jul to 31 Dec 2026
 2027 S1 · 1 Jan to 30 Jun 2027
 2027 S2 · 1 Jul to 31 Dec 2027
 The semester the committee decides for. Choosing one fills the evidence window from the start of its academic year, so the annual EPAs a semester-2 sitting decides are judged on the whole year. The College's semester 2 ends in November; its window runs to 31 December, so December's work counts. 
 Evidence window
 From
 To
 Scheduled on
 The day the review is put on the schedule. 
 Review type
 Annual progression review The semester-2 sitting decides the trainee's progression for the year. 
 Pre-graduation review The final review before the programme ends: it decides the trainee's progression, and whether they may graduate. 
 Formative check-in Closes without a binding decision and cannot issue entrustment decisions. Use for interim progress check-ins. It takes no seat for the period. 
## Agenda
 A formative review carries no agenda: it decides no EPA.
 Cancel Create review


######## BOARD R2-4-schedule-390-c.dc.html — Schedule a review · 390 · c
Schedule a review · 390 · c 
# Schedule a review · 390px (continued, part c) 
 Route /committee/reviews/new?panel=&trainee=&period= · NEW owner table row: Committee reviews lit for the five schedulers.
 Components PageHeader, Breadcrumbs, FormField, FormActions, RefusalSummary, Alert, StatePanel, SittingBar, .scale-choices .form-check (Review type), .is-running (busy). NEW .schedule-grid (stacked below 1100px, M9), .agenda-preview with one live region .agenda-live, .field-pair.
 DESIGN.md § Page-level patterns, Form page; § Form system (Formative only folds into Review type; the type preselected, F4; the browser's date input, F3). Round 3 F1, F3, F4, F5, F6, M5, M9, S1, S4, S6.
=== FRAME: | From a Missed row of 2026 S1 (F1, C5) | | Step 4.2 · Nolwazi Mbatha · the form opens on the row's own ended semester, the only review that clears it; S1's types with their help (S1); the preview for 2026 S1 | 
 [shell] 
 Home 
 Committee reviews 
 Schedule a review 
# Schedule a review
 Put a trainee before a panel for a period. The review opens once it is created.
 Panel
 Choose a panel
 Paed Annual Review Panel
 Only the panels with a trainee you can schedule are listed. 
 Trainee
 Choose a trainee
 Anele Dlamini · Training year 3
 Pieter du Plessis · Training year 2
 Nomsa Mahlangu · Training year 1
 Lerato Molefe · Training year 4
 Sipho Ndlovu · Training year 1
 Current registrars at the panel's institution whose programme you oversee, with their training year. 
 Period
 Choose a period
 2025 S1 · 1 Jan to 30 Jun 2025
 2025 S2 · 1 Jul to 31 Dec 2025
 2026 S1 · 1 Jan to 30 Jun 2026
 2026 S2 · 1 Jul to 31 Dec 2026
 2027 S1 · 1 Jan to 30 Jun 2027
 2027 S2 · 1 Jul to 31 Dec 2027
 2026 S1 has ended. A review can still sit for it: sittings are often held after the period ends. The semester the committee decides for. Choosing one fills the evidence window from the start of its academic year, so the annual EPAs a semester-2 sitting decides are judged on the whole year. 
 Evidence window
 From
 To
 Scheduled on
 The day the review is put on the schedule. 
 Review type
 Entrustment-only review Decides entrustment only: a review before it records no progression category. 
 Annual progression review Decides the trainee's progression for the year. 
 Pre-graduation review The final review before the programme ends: it decides the trainee's progression, and whether they may graduate. 
 Formative check-in Closes without a binding decision and cannot issue entrustment decisions. Use for interim progress check-ins. It takes no seat for the period. 
## Agenda
 15 EPAs will be on the agenda for 2026 S1.
 PAED-001, PAED-002, PAED-004, PAED-005, PAED-010 and PAED-012 are due at this sitting.
 PAED-003 — Providing intensive care to children · Due by year end
 PAED-006 — [PAED-006 title, as seeded] · Due by year end
 PAED-007 — [PAED-007 title, as seeded] · Due by year end
 PAED-011 — [PAED-011 title, as seeded] · Due by year end
 PAED-014 — [PAED-014 title, as seeded] · Due by year end
 PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research · Due by year end
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children · As opportunity allows
 PAED-009 — [PAED-009 title, as seeded] · As opportunity allows
 PAED-013 — [PAED-013 title, as seeded] · As opportunity allows
 Cancel Create review 
=== FRAME: | Heavy: two panels; the general panel chosen (S1) | | Pieter Smit · the Panel select offers both; the preview names the panel that decides PAED-004 and 005 | 
 [shell] 
 Home 
 Committee reviews 
 Schedule a review 
# Schedule a review
 Put a trainee before a panel for a period. The review opens once it is created.
 Panel
 Choose a panel
 Paed Annual Review Panel
 [Neonatal panel]
 Only the panels with a trainee you can schedule are listed. 
 Trainee
 Choose a trainee
 Anele Dlamini · Training year 3
 Pieter du Plessis · Training year 2
 Nomsa Mahlangu · Training year 1
 Lerato Molefe · Training year 4
 Sipho Ndlovu · Training year 1
 Current registrars at the panel's institution whose programme you oversee, with their training year. 
 Period
 Choose a period
 2025 S1 · 1 Jan to 30 Jun 2025
 2025 S2 · 1 Jul to 31 Dec 2025
 2026 S1 · 1 Jan to 30 Jun 2026
 2026 S2 · 1 Jul to 31 Dec 2026
 2027 S1 · 1 Jan to 30 Jun 2027
 2027 S2 · 1 Jul to 31 Dec 2027
 The semester the committee decides for. Choosing one fills the evidence window from the start of its academic year, so the annual EPAs a semester-2 sitting decides are judged on the whole year. The College's semester 2 ends in November; its window runs to 31 December, so December's work counts. 
 Evidence window
 From
 To
 Scheduled on
 The day the review is put on the schedule. 
 Review type
 Annual progression review The semester-2 sitting decides the trainee's progression for the year. 
 Pre-graduation review The final review before the programme ends: it decides the trainee's progression, and whether they may graduate. 
 Formative check-in Closes without a binding decision and cannot issue entrustment decisions. Use for interim progress check-ins. It takes no seat for the period. 
## Agenda
 13 EPAs will be on the agenda for 2026 S2.
 PAED-001, PAED-002, PAED-003, PAED-006, PAED-007, PAED-010, PAED-011, PAED-012, PAED-014 and PAED-015 are due at this sitting.
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children · As opportunity allows
 PAED-009 — [PAED-009 title, as seeded] · As opportunity allows
 PAED-013 — [PAED-013 title, as seeded] · As opportunity allows
 PAED-004 and PAED-005 are decided by [Neonatal panel]: schedule them separately.
 Cancel Create review


######## BOARD R2-4-schedule-390-d.dc.html — Schedule a review · 390 · d
Schedule a review · 390 · d 
# Schedule a review · 390px (continued, part d) 
 Route /committee/reviews/new?panel=&trainee=&period= · NEW owner table row: Committee reviews lit for the five schedulers.
 Components PageHeader, Breadcrumbs, FormField, FormActions, RefusalSummary, Alert, StatePanel, SittingBar, .scale-choices .form-check (Review type), .is-running (busy). NEW .schedule-grid (stacked below 1100px, M9), .agenda-preview with one live region .agenda-live, .field-pair.
 DESIGN.md § Page-level patterns, Form page; § Form system (Formative only folds into Review type; the type preselected, F4; the browser's date input, F3). Round 3 F1, F3, F4, F5, F6, M5, M9, S1, S4, S6.
=== FRAME: | Heavy: the neonatal panel's one type (S1, F4) | | Pieter Smit · before the panel sitting as the neonatal committee, Entrustment-only is the one binding type, preselected | 
 [shell] 
 Home 
 Committee reviews 
 Schedule a review 
# Schedule a review
 Put a trainee before a panel for a period. The review opens once it is created.
 Panel
 Choose a panel
 Paed Annual Review Panel
 [Neonatal panel]
 Only the panels with a trainee you can schedule are listed. 
 Trainee
 Choose a trainee
 Anele Dlamini · Training year 3
 Pieter du Plessis · Training year 2
 Nomsa Mahlangu · Training year 1
 Lerato Molefe · Training year 4
 Sipho Ndlovu · Training year 1
 Current registrars at the panel's institution whose programme you oversee, with their training year. 
 Period
 Choose a period
 2025 S1 · 1 Jan to 30 Jun 2025
 2025 S2 · 1 Jul to 31 Dec 2025
 2026 S1 · 1 Jan to 30 Jun 2026
 2026 S2 · 1 Jul to 31 Dec 2026
 2027 S1 · 1 Jan to 30 Jun 2027
 2027 S2 · 1 Jul to 31 Dec 2027
 The semester the committee decides for. Choosing one fills the evidence window from the start of its academic year, so the annual EPAs a semester-2 sitting decides are judged on the whole year. The College's semester 2 ends in November; its window runs to 31 December, so December's work counts. 
 Evidence window
 From
 To
 Scheduled on
 The day the review is put on the schedule. 
 Review type
 Entrustment-only review [Neonatal panel] sits as the Neonatal team Clinical Competency Committee, which decides entrustment only: a review before it records no progression category. 
 Formative check-in Closes without a binding decision and cannot issue entrustment decisions. Use for interim progress check-ins. It takes no seat for the period. 
## Agenda
 2 EPAs will be on the agenda for 2026 S2.
 PAED-004 and PAED-005 are due at this sitting.
 Cancel Create review 
=== FRAME: | Current registrars only (5.29, S1) | | Pieter Smit · the Trainee list drawn open (a listbox here only to show its options): a graduate and a withdrawn registrar are not offered; each with the training year | 
 [shell] 
 Home 
 Committee reviews 
 Schedule a review 
# Schedule a review
 Put a trainee before a panel for a period. The review opens once it is created.
 Panel
 Choose a panel
 Paed Annual Review Panel
 Only the panels with a trainee you can schedule are listed. 
 Trainee
 Choose a trainee
 Anele Dlamini · Training year 3
 Nomsa Mahlangu · Training year 1
 Sipho Ndlovu · Training year 1
 Current registrars at the panel's institution whose programme you oversee, with their training year. 
 Period
 Choose a period
 2025 S1 · 1 Jan to 30 Jun 2025
 2025 S2 · 1 Jul to 31 Dec 2025
 2026 S1 · 1 Jan to 30 Jun 2026
 2026 S2 · 1 Jul to 31 Dec 2026
 2027 S1 · 1 Jan to 30 Jun 2027
 2027 S2 · 1 Jul to 31 Dec 2027
 The semester the committee decides for. Choosing one fills the evidence window from the start of its academic year, so the annual EPAs a semester-2 sitting decides are judged on the whole year. 
 Evidence window
 From
 To
 Scheduled on
 The day the review is put on the schedule. 
 Review type
 Annual progression review The semester-2 sitting decides the trainee's progression for the year. 
 Pre-graduation review The final review before the programme ends: it decides the trainee's progression, and whether they may graduate. 
 Formative check-in Closes without a binding decision and cannot issue entrustment decisions. Use for interim progress check-ins. It takes no seat for the period. 
## Agenda
 Choose a panel, a trainee and a period to see which EPAs the review will be there to decide.
 Cancel Create review 
=== FRAME: | NothingToDecide (S1) | | Pieter Smit · an entrustment-only review before a panel that decides no EPA on the trainee's curriculum: the preview says so first; Create is refused in the handler's own sentence | | Focus: the summary | 
 [shell] 
 Home 
 Committee reviews 
 Schedule a review 
# Schedule a review
 Put a trainee before a panel for a period. The review opens once it is created.
 Panel
 Choose a panel
 Paed Annual Review Panel
 [Neonatal panel]
 Only the panels with a trainee you can schedule are listed. 
 Trainee
 Choose a trainee
 Anele Dlamini · Training year 3
 Pieter du Plessis · Training year 2
 Nomsa Mahlangu · Training year 1
 Lerato Molefe · Training year 4
 Sipho Ndlovu · Training year 1
 Current registrars at the panel's institution whose programme you oversee, with their training year. 
 Period
 Choose a period
 2025 S1 · 1 Jan to 30 Jun 2025
 2025 S2 · 1 Jul to 31 Dec 2025
 2026 S1 · 1 Jan to 30 Jun 2026
 2026 S2 · 1 Jul to 31 Dec 2026
 2027 S1 · 1 Jan to 30 Jun 2027
 2027 S2 · 1 Jul to 31 Dec 2027
 The semester the committee decides for. Choosing one fills the evidence window from the start of its academic year, so the annual EPAs a semester-2 sitting decides are judged on the whole year. The College's semester 2 ends in November; its window runs to 31 December, so December's work counts. 
 Evidence window
 From
 To
 Scheduled on
 The day the review is put on the schedule. 
 Review type
 Entrustment-only review [Neonatal panel] sits as the Neonatal team Clinical Competency Committee, which decides entrustment only: a review before it records no progression category. 
 Formative check-in Closes without a binding decision and cannot issue entrustment decisions. Use for interim progress check-ins. It takes no seat for the period. 
## Agenda
 No EPA will be on the agenda.
 [Neonatal panel] decides no EPA of this trainee's curriculum, so the review would have nothing on its agenda.
 The review was not created. Everything you chose is kept below. Panel : [NothingToDecide: the handler's own sentence, as it ships]
 Cancel Create review


######## BOARD R2-4-schedule-390-e.dc.html — Schedule a review · 390 · e
Schedule a review · 390 · e 
# Schedule a review · 390px (continued, part e) 
 Route /committee/reviews/new?panel=&trainee=&period= · NEW owner table row: Committee reviews lit for the five schedulers.
 Components PageHeader, Breadcrumbs, FormField, FormActions, RefusalSummary, Alert, StatePanel, SittingBar, .scale-choices .form-check (Review type), .is-running (busy). NEW .schedule-grid (stacked below 1100px, M9), .agenda-preview with one live region .agenda-live, .field-pair.
 DESIGN.md § Page-level patterns, Form page; § Form system (Formative only folds into Review type; the type preselected, F4; the browser's date input, F3). Round 3 F1, F3, F4, F5, F6, M5, M9, S1, S4, S6.
=== FRAME: | The preview failed (C6, T338) | | Pieter Smit · Create stays offered: the preview is advice; the agenda is planned at Create (S4) | | Focus: stays where it was; the failure is announced in the aside's one live region | 
 [shell] 
 Home 
 Committee reviews 
 Schedule a review 
# Schedule a review
 Put a trainee before a panel for a period. The review opens once it is created.
 Panel
 Choose a panel
 Paed Annual Review Panel
 Only the panels with a trainee you can schedule are listed. 
 Trainee
 Choose a trainee
 Anele Dlamini · Training year 3
 Pieter du Plessis · Training year 2
 Nomsa Mahlangu · Training year 1
 Lerato Molefe · Training year 4
 Sipho Ndlovu · Training year 1
 Current registrars at the panel's institution whose programme you oversee, with their training year. 
 Period
 Choose a period
 2025 S1 · 1 Jan to 30 Jun 2025
 2025 S2 · 1 Jul to 31 Dec 2025
 2026 S1 · 1 Jan to 30 Jun 2026
 2026 S2 · 1 Jul to 31 Dec 2026
 2027 S1 · 1 Jan to 30 Jun 2027
 2027 S2 · 1 Jul to 31 Dec 2027
 The semester the committee decides for. Choosing one fills the evidence window from the start of its academic year, so the annual EPAs a semester-2 sitting decides are judged on the whole year. The College's semester 2 ends in November; its window runs to 31 December, so December's work counts. 
 Evidence window
 From
 To
 Scheduled on
 The day the review is put on the schedule. 
 Review type
 Annual progression review The semester-2 sitting decides the trainee's progression for the year. 
 Pre-graduation review The final review before the programme ends: it decides the trainee's progression, and whether they may graduate. 
 Formative check-in Closes without a binding decision and cannot issue entrustment decisions. Use for interim progress check-ins. It takes no seat for the period. 
## Agenda
 The agenda could not be read just now. Try again 
 The review can still be created: its agenda is planned when you press Create review.
 Cancel Create review 
=== FRAME: | A refusal on Create: missing fields (C7, T324) | | Pieter Smit · above the actions and beside each field; no line points at the disabled Trainee (S6); no type refusal (F4) | | Focus: the summary; each line moves it to its field | 
 [shell] 
 Home 
 Committee reviews 
 Schedule a review 
# Schedule a review
 Put a trainee before a panel for a period. The review opens once it is created.
 Panel
 Choose a panel
 Paed Annual Review Panel
 Choose a panel.
 Only the panels with a trainee you can schedule are listed. 
 Trainee
 Choose a panel first
 Choose a panel first. The list holds the trainees at its institution you can schedule. 
 Period
 Choose a period
 2025 S1 · 1 Jan to 30 Jun 2025
 2025 S2 · 1 Jul to 31 Dec 2025
 2026 S1 · 1 Jan to 30 Jun 2026
 2026 S2 · 1 Jul to 31 Dec 2026
 2027 S1 · 1 Jan to 30 Jun 2027
 2027 S2 · 1 Jul to 31 Dec 2027
 Choose the period the review sits for.
 The semester the committee decides for. Choosing one fills the evidence window from the start of its academic year, so the annual EPAs a semester-2 sitting decides are judged on the whole year. 
 Evidence window
 From
 To
 Scheduled on
 The day the review is put on the schedule. 
 Review type
 Annual progression review The semester-2 sitting decides the trainee's progression for the year. 
 Pre-graduation review The final review before the programme ends: it decides the trainee's progression, and whether they may graduate. 
 Formative check-in Closes without a binding decision and cannot issue entrustment decisions. Use for interim progress check-ins. It takes no seat for the period. 
## Agenda
 Choose a panel, a trainee and a period to see which EPAs the review will be there to decide.
 The review was not created. Everything you chose is kept below. Panel: Choose a panel. 
 Period: Choose the period the review sits for. 
 Cancel Create review


######## BOARD R2-4-schedule-390-f.dc.html — Schedule a review · 390 · f
Schedule a review · 390 · f 
# Schedule a review · 390px (continued, part f) 
 Route /committee/reviews/new?panel=&trainee=&period= · NEW owner table row: Committee reviews lit for the five schedulers.
 Components PageHeader, Breadcrumbs, FormField, FormActions, RefusalSummary, Alert, StatePanel, SittingBar, .scale-choices .form-check (Review type), .is-running (busy). NEW .schedule-grid (stacked below 1100px, M9), .agenda-preview with one live region .agenda-live, .field-pair.
 DESIGN.md § Page-level patterns, Form page; § Form system (Formative only folds into Review type; the type preselected, F4; the browser's date input, F3). Round 3 F1, F3, F4, F5, F6, M5, M9, S1, S4, S6.
=== FRAME: | A refusal on Create: the evidence window (S4) | | Pieter Smit · in Wombat's words; a missing From reads “Choose the day the window starts.” | | Focus: the summary | 
 [shell] 
 Home 
 Committee reviews 
 Schedule a review 
# Schedule a review
 Put a trainee before a panel for a period. The review opens once it is created.
 Panel
 Choose a panel
 Paed Annual Review Panel
 Only the panels with a trainee you can schedule are listed. 
 Trainee
 Choose a trainee
 Anele Dlamini · Training year 3
 Pieter du Plessis · Training year 2
 Nomsa Mahlangu · Training year 1
 Lerato Molefe · Training year 4
 Sipho Ndlovu · Training year 1
 Current registrars at the panel's institution whose programme you oversee, with their training year. 
 Period
 Choose a period
 2025 S1 · 1 Jan to 30 Jun 2025
 2025 S2 · 1 Jul to 31 Dec 2025
 2026 S1 · 1 Jan to 30 Jun 2026
 2026 S2 · 1 Jul to 31 Dec 2026
 2027 S1 · 1 Jan to 30 Jun 2027
 2027 S2 · 1 Jul to 31 Dec 2027
 The semester the committee decides for. Choosing one fills the evidence window from the start of its academic year, so the annual EPAs a semester-2 sitting decides are judged on the whole year. The College's semester 2 ends in November; its window runs to 31 December, so December's work counts. 
 Evidence window
 From
 To
 The window ends before it starts. Choose a day on or after 2026-01-01.
 Scheduled on
 The day the review is put on the schedule. 
 Review type
 Annual progression review The semester-2 sitting decides the trainee's progression for the year. 
 Pre-graduation review The final review before the programme ends: it decides the trainee's progression, and whether they may graduate. 
 Formative check-in Closes without a binding decision and cannot issue entrustment decisions. Use for interim progress check-ins. It takes no seat for the period. 
## Agenda
 15 EPAs will be on the agenda for 2026 S2.
 PAED-001, PAED-002, PAED-003, PAED-004, PAED-005, PAED-006, PAED-007, PAED-010, PAED-011, PAED-012, PAED-014 and PAED-015 are due at this sitting.
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children · As opportunity allows
 PAED-009 — [PAED-009 title, as seeded] · As opportunity allows
 PAED-013 — [PAED-013 title, as seeded] · As opportunity allows
 The review was not created. Everything you chose is kept below. Evidence window, To: The window ends before it starts. Choose a day on or after 2026-01-01. 
 Cancel Create review 
=== FRAME: | Scratch state: a second tab. The seat taken meanwhile (C7, F5) | | No runbook step plays it: handler and bUnit tests cover it · the summary links a short clause (S4); the preview turns to Already held | | Focus: the summary | 
 [shell] 
 Home 
 Committee reviews 
 Schedule a review 
# Schedule a review
 Put a trainee before a panel for a period. The review opens once it is created.
 Panel
 Choose a panel
 Paed Annual Review Panel
 Only the panels with a trainee you can schedule are listed. 
 Trainee
 Choose a trainee
 Anele Dlamini · Training year 3
 Pieter du Plessis · Training year 2
 Nomsa Mahlangu · Training year 1
 Lerato Molefe · Training year 4
 Sipho Ndlovu · Training year 1
 Current registrars at the panel's institution whose programme you oversee, with their training year. 
 Period
 Choose a period
 2025 S1 · 1 Jan to 30 Jun 2025
 2025 S2 · 1 Jul to 31 Dec 2025
 2026 S1 · 1 Jan to 30 Jun 2026
 2026 S2 · 1 Jul to 31 Dec 2026
 2027 S1 · 1 Jan to 30 Jun 2027
 2027 S2 · 1 Jul to 31 Dec 2027
 The semester the committee decides for. Choosing one fills the evidence window from the start of its academic year, so the annual EPAs a semester-2 sitting decides are judged on the whole year. The College's semester 2 ends in November; its window runs to 31 December, so December's work counts. 
 Evidence window
 From
 To
 Scheduled on
 The day the review is put on the schedule. 
 Review type
 Annual progression review The semester-2 sitting decides the trainee's progression for the year. 
 Pre-graduation review The final review before the programme ends: it decides the trainee's progression, and whether they may graduate. 
 Formative check-in Closes without a binding decision and cannot issue entrustment decisions. Use for interim progress check-ins. It takes no seat for the period. 
## Agenda
 Already held. Review #1 already puts Lerato Molefe before Paed Annual Review Panel for 2026 S2 (scheduled on 2026-10-12). A trainee has one binding review for each period before the panels that decide the same EPAs, until it is ratified: open that review, or ratify it before scheduling another.
 A formative check-in takes no seat: choose Formative check-in under Review type to schedule one.
 The review was not created. Everything you chose is kept below. Review #1 already puts Lerato Molefe before Paed Annual Review Panel for 2026 S2 (scheduled on 2026-10-12). A trainee has one binding review for each period before the panels that decide the same EPAs, until it is ratified: open that review, or ratify it before scheduling another.
 Open review #1 
 Create review is not offered: review #1 already holds 2026 S2 for Lerato Molefe.
 Cancel Open review #1


######## BOARD R2-4-schedule-390-g.dc.html — Schedule a review · 390 · g
Schedule a review · 390 · g 
# Schedule a review · 390px (continued, part g) 
 Route /committee/reviews/new?panel=&trainee=&period= · NEW owner table row: Committee reviews lit for the five schedulers.
 Components PageHeader, Breadcrumbs, FormField, FormActions, RefusalSummary, Alert, StatePanel, SittingBar, .scale-choices .form-check (Review type), .is-running (busy). NEW .schedule-grid (stacked below 1100px, M9), .agenda-preview with one live region .agenda-live, .field-pair.
 DESIGN.md § Page-level patterns, Form page; § Form system (Formative only folds into Review type; the type preselected, F4; the browser's date input, F3). Round 3 F1, F3, F4, F5, F6, M5, M9, S1, S4, S6.
=== FRAME: | Busy: Creating… | | Step 4.6 · .is-running with aria-disabled, the focus kept; Cancel a disabled button, not a followable link (S6) | | Focus: stays on Creating… | 
 [shell] 
 Home 
 Committee reviews 
 Schedule a review 
# Schedule a review
 Put a trainee before a panel for a period. The review opens once it is created.
 Panel
 Choose a panel
 Paed Annual Review Panel
 Only the panels with a trainee you can schedule are listed. 
 Trainee
 Choose a trainee
 Anele Dlamini · Training year 3
 Pieter du Plessis · Training year 2
 Nomsa Mahlangu · Training year 1
 Lerato Molefe · Training year 4
 Sipho Ndlovu · Training year 1
 Current registrars at the panel's institution whose programme you oversee, with their training year. 
 Period
 Choose a period
 2025 S1 · 1 Jan to 30 Jun 2025
 2025 S2 · 1 Jul to 31 Dec 2025
 2026 S1 · 1 Jan to 30 Jun 2026
 2026 S2 · 1 Jul to 31 Dec 2026
 2027 S1 · 1 Jan to 30 Jun 2027
 2027 S2 · 1 Jul to 31 Dec 2027
 The semester the committee decides for. Choosing one fills the evidence window from the start of its academic year, so the annual EPAs a semester-2 sitting decides are judged on the whole year. The College's semester 2 ends in November; its window runs to 31 December, so December's work counts. 
 Evidence window
 From
 To
 Scheduled on
 The day the review is put on the schedule. 
 Review type
 Annual progression review The semester-2 sitting decides the trainee's progression for the year. 
 Pre-graduation review The final review before the programme ends: it decides the trainee's progression, and whether they may graduate. 
 Formative check-in Closes without a binding decision and cannot issue entrustment decisions. Use for interim progress check-ins. It takes no seat for the period. 
## Agenda
 15 EPAs will be on the agenda for 2026 S2.
 PAED-001, PAED-002, PAED-003, PAED-004, PAED-005, PAED-006, PAED-007, PAED-010, PAED-011, PAED-012, PAED-014 and PAED-015 are due at this sitting.
 PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children · As opportunity allows
 PAED-009 — [PAED-009 title, as seeded] · As opportunity allows
 PAED-013 — [PAED-013 title, as seeded] · As opportunity allows
 Cancel Creating… 
=== FRAME: | Created: the review opens (flow 07's page, its top only) | | Step 5.2 · Pieter Smit | | Focus: its h1 | 
 [shell] 
 Home 
 Committee reviews 
 Lerato Molefe · 2026 S2 
# Lerato Molefe · 2026 S2
 Pre-graduation review · Paed Annual Review Panel · Training year 4
 Lerato Molefe's progress 
## The sitting
 Scheduled 12 EPAs on the agenda Summative · evidence 2023-01-15 to 2026-12-31 
 As of 10:42 SAST
 Refresh Start review… 
 The rest is flow 07's review page, unchanged
=== FRAME: | Loading | | C11 · Create is not offered until the read is in | 
 [shell] 
 Home 
 Committee reviews 
 Schedule a review 
# Schedule a review
 Put a trainee before a panel for a period. The review opens once it is created.
 Loading what you can schedule.
## Agenda
 Cancel 
=== FRAME: | Load error | | C11, T338 | | Focus: the alert | 
 [shell] 
 Home 
 Committee reviews 
 Schedule a review 
# Schedule a review
 Put a trainee before a panel for a period. The review opens once it is created.
 Could not load what you can schedule. Nothing has changed. Try again, or come back in a few minutes. Try again


######## BOARD R2-5-committee-reviews-1280.dc.html — Committee reviews · 1280
Committee reviews · 1280 
# Committee reviews · 1280px
 Route /committee/reviews · header action “Schedule a review”, a link to the schedule page.
 Components PageHeader, DataTable (stack-wide), Badge (StateBadge's tints: Scheduled grey on both lists), StatePanel. NEW .period-window.
 DESIGN.md § Table system: no Mode column (C8); stacked at 900px and below (M8). Round 3 F6, M8, S2, S7.
=== FRAME: | Empty, for a scheduler | | Pieter Smit · the header action reads “Schedule a review”, the page's own words (S7) | 
 [shell] 
# Committee reviews
 Schedule progression reviews and open existing ones.
 Schedule a review 
 No reviews yet
 The reviews you can open appear here once they are scheduled. Use Schedule a review to put a trainee before a panel.
=== FRAME: | Empty, for a member | | David Naidoo · no Schedule a review | 
 [shell] 
# Committee reviews
 Open existing committee reviews.
 No reviews yet
 The reviews of the panels you sit on appear here once they are scheduled.
=== FRAME: | Scheduled | | Step 4.10, after · Pieter Smit · Scheduled grey here and on Decisions due (S7); no Mode column; stacked at 900px and below (M8) | 
 [shell] 
# Committee reviews
 Schedule progression reviews and open existing ones.
 Schedule a review 
 The College's semester 2 ends in November; its window runs to 31 December, so December's work counts.
 Committee reviews at Kgosi Kgari Teaching Hospital, newest scheduled first
 Trainee | Panel | Period | Type | State | Decision | Actions | 
 Sipho Ndlovu | Paed Annual Review Panel | 2026 S2 2026-01-01 to 2026-12-31 | Annual progression | Scheduled | Pending | Open 
 | 
 Nomsa Mahlangu | Paed Annual Review Panel | 2026 S2 2026-01-01 to 2026-12-31 | Annual progression | Scheduled | Pending | Open 
 | 
 Pieter du Plessis | Paed Annual Review Panel | 2026 S2 2026-01-01 to 2026-12-31 | Annual progression | Scheduled | Pending | Open 
 | 
 Anele Dlamini | Paed Annual Review Panel | 2026 S2 2026-01-01 to 2026-12-31 | Annual progression | Scheduled | Pending | Open 
 | 
 Lerato Molefe | Paed Annual Review Panel | 2026 S2 2026-01-01 to 2026-12-31 | Annual progression | Scheduled | Pending | Open 
 | 
=== FRAME: | Ratified: decisions by label | | Step 4.33 · Pieter Smit | 
 [shell] 
# Committee reviews
 Schedule progression reviews and open existing ones.
 Schedule a review 
 The College's semester 2 ends in November; its window runs to 31 December, so December's work counts.
 Committee reviews at Kgosi Kgari Teaching Hospital, newest scheduled first
 Trainee | Panel | Period | Type | State | Decision | Actions | 
 Sipho Ndlovu | Paed Annual Review Panel | 2026 S2 2026-01-01 to 2026-12-31 | Annual progression | Ratified | Outcome Deferred | Open 
 | 
 Nomsa Mahlangu | Paed Annual Review Panel | 2026 S2 2026-01-01 to 2026-12-31 | Annual progression | Ratified | Inadequate Progress — Additional Training | Open 
 | 
 Pieter du Plessis | Paed Annual Review Panel | 2026 S2 2026-01-01 to 2026-12-31 | Annual progression | Ratified | Satisfactory with Observations | Open 
 | 
 Anele Dlamini | Paed Annual Review Panel | 2026 S2 2026-01-01 to 2026-12-31 | Annual progression | Ratified | Satisfactory Progress | Open 
 | 
 Lerato Molefe | Paed Annual Review Panel | 2026 S2 2026-01-01 to 2026-12-31 | Annual progression | Ratified | Satisfactory Progress | Open 
 | 
=== FRAME: | Heavy, after Step A.1.14 (S2) | | Pieter Smit · Sipho Ndlovu's three reviews show his pseudonym as stored; Entrustment only appears only in 2026 S1, before the neonatal panel; a check-in's Decision “—” reads “None” (S7) | 
 [shell] 
# Committee reviews
 Schedule progression reviews and open existing ones.
 Schedule a review 
 The College's semester 2 ends in November; its window runs to 31 December, so December's work counts.
 Committee reviews at Kgosi Kgari Teaching Hospital, newest scheduled first
 Trainee | Panel | Period | Type | State | Decision | Actions | 
 Lerato Molefe | Paed Annual Review Panel | 2026 S2 2023-01-15 to 2026-12-31 | Pre-graduation | Scheduled | Pending | Open 
 | 
 [Pseudonym, as stored] | Paed Annual Review Panel | 2026 S2 2026-01-01 to 2026-12-31 | Formative check-in | Withdrawn | — None | Open 
 | 
 [Pseudonym, as stored] | Paed Annual Review Panel | 2026 S2 2026-01-01 to 2026-12-31 | Formative check-in | Closed | — None | Open 
 | 
 Nomsa Mahlangu | Paed Annual Review Panel | 2026 S2 2026-01-01 to 2026-12-31 | Annual progression | Ratified | Inadequate Progress — Additional Training | Open 
 | 
 Pieter du Plessis | Paed Annual Review Panel | 2026 S2 2026-01-01 to 2026-12-31 | Annual progression | Ratified | Satisfactory with Observations | Open 
 | 
 Anele Dlamini | Paed Annual Review Panel | 2026 S2 2026-01-01 to 2026-12-31 | Annual progression | Ratified | Satisfactory Progress | Open 
 | 
 Lerato Molefe | Paed Annual Review Panel | 2026 S2 2026-01-01 to 2026-12-31 | Annual progression | Ratified | Satisfactory Progress | Open 
 | 
 [Pseudonym, as stored] | Paed Annual Review Panel | 2026 S2 2026-01-01 to 2026-12-31 | Annual progression | Ratified | Outcome Deferred | Open 
 | 
 Nomsa Mahlangu | [Neonatal panel] | 2026 S1 2026-01-01 to 2026-06-30 | Entrustment only | Ratified | Entrustment decisions only | Open 
 | 
 Anele Dlamini | [Neonatal panel] | 2026 S1 2026-01-01 to 2026-06-30 | Entrustment only | Ratified | Entrustment decisions only | Open 
 | 
=== FRAME: | A member's list | | Step 4.11 · David Naidoo | 
 [shell] 
# Committee reviews
 Open existing committee reviews.
 The College's semester 2 ends in November; its window runs to 31 December, so December's work counts.
 Committee reviews at Kgosi Kgari Teaching Hospital, newest scheduled first
 Trainee | Panel | Period | Type | State | Decision | Actions | 
 Sipho Ndlovu | Paed Annual Review Panel | 2026 S2 2026-01-01 to 2026-12-31 | Annual progression | Scheduled | Pending | Open 
 | 
 Nomsa Mahlangu | Paed Annual Review Panel | 2026 S2 2026-01-01 to 2026-12-31 | Annual progression | Scheduled | Pending | Open 
 | 
 Pieter du Plessis | Paed Annual Review Panel | 2026 S2 2026-01-01 to 2026-12-31 | Annual progression | Scheduled | Pending | Open 
 | 
 Anele Dlamini | Paed Annual Review Panel | 2026 S2 2026-01-01 to 2026-12-31 | Annual progression | Scheduled | Pending | Open 
 | 
 Lerato Molefe | Paed Annual Review Panel | 2026 S2 2026-01-01 to 2026-12-31 | Annual progression | Scheduled | Pending | Open 
 | 
=== FRAME: | Loading | | C11 · Schedule a review is offered once the read is in | 
 [shell] 
# Committee reviews
 Schedule progression reviews and open existing ones.
 Loading Committee reviews.
=== FRAME: | Load error | | C11, T338 | | Focus: the alert | 
 [shell] 
# Committee reviews
 Schedule progression reviews and open existing ones.
 Schedule a review 
 Could not load Committee reviews. Nothing has changed. Try again, or come back in a few minutes. Try again


######## BOARD R2-5-committee-reviews-768.dc.html — Committee reviews · 768
Committee reviews · 768 
# Committee reviews · 768px
 Route /committee/reviews · header action “Schedule a review”, a link to the schedule page.
 Components PageHeader, DataTable (stack-wide), Badge (StateBadge's tints: Scheduled grey on both lists), StatePanel. NEW .period-window.
 DESIGN.md § Table system: no Mode column (C8); stacked at 900px and below (M8). Round 3 F6, M8, S2, S7.
=== FRAME: | Scheduled | | Step 4.10, after · Pieter Smit · Scheduled grey here and on Decisions due (S7); no Mode column; stacked at 900px and below (M8) | 
 [shell] 
# Committee reviews
 Schedule progression reviews and open existing ones.
 Schedule a review 
 The College's semester 2 ends in November; its window runs to 31 December, so December's work counts.
 Committee reviews at Kgosi Kgari Teaching Hospital, newest scheduled first
 Trainee | Panel | Period | Type | State | Decision | Actions | 
 Sipho Ndlovu | Paed Annual Review Panel | 2026 S2 2026-01-01 to 2026-12-31 | Annual progression | Scheduled | Pending | Open 
 | 
 Nomsa Mahlangu | Paed Annual Review Panel | 2026 S2 2026-01-01 to 2026-12-31 | Annual progression | Scheduled | Pending | Open 
 | 
 Pieter du Plessis | Paed Annual Review Panel | 2026 S2 2026-01-01 to 2026-12-31 | Annual progression | Scheduled | Pending | Open 
 | 
 Anele Dlamini | Paed Annual Review Panel | 2026 S2 2026-01-01 to 2026-12-31 | Annual progression | Scheduled | Pending | Open 
 | 
 Lerato Molefe | Paed Annual Review Panel | 2026 S2 2026-01-01 to 2026-12-31 | Annual progression | Scheduled | Pending | Open 
 | 
=== FRAME: | Heavy, after Step A.1.14 (S2) | | Pieter Smit · Sipho Ndlovu's three reviews show his pseudonym as stored; Entrustment only appears only in 2026 S1, before the neonatal panel; a check-in's Decision “—” reads “None” (S7) | 
 [shell] 
# Committee reviews
 Schedule progression reviews and open existing ones.
 Schedule a review 
 The College's semester 2 ends in November; its window runs to 31 December, so December's work counts.
 Committee reviews at Kgosi Kgari Teaching Hospital, newest scheduled first
 Trainee | Panel | Period | Type | State | Decision | Actions | 
 Lerato Molefe | Paed Annual Review Panel | 2026 S2 2023-01-15 to 2026-12-31 | Pre-graduation | Scheduled | Pending | Open 
 | 
 [Pseudonym, as stored] | Paed Annual Review Panel | 2026 S2 2026-01-01 to 2026-12-31 | Formative check-in | Withdrawn | — None | Open 
 | 
 [Pseudonym, as stored] | Paed Annual Review Panel | 2026 S2 2026-01-01 to 2026-12-31 | Formative check-in | Closed | — None | Open 
 | 
 Nomsa Mahlangu | Paed Annual Review Panel | 2026 S2 2026-01-01 to 2026-12-31 | Annual progression | Ratified | Inadequate Progress — Additional Training | Open 
 | 
 Pieter du Plessis | Paed Annual Review Panel | 2026 S2 2026-01-01 to 2026-12-31 | Annual progression | Ratified | Satisfactory with Observations | Open 
 | 
 Anele Dlamini | Paed Annual Review Panel | 2026 S2 2026-01-01 to 2026-12-31 | Annual progression | Ratified | Satisfactory Progress | Open 
 | 
 Lerato Molefe | Paed Annual Review Panel | 2026 S2 2026-01-01 to 2026-12-31 | Annual progression | Ratified | Satisfactory Progress | Open 
 | 
 [Pseudonym, as stored] | Paed Annual Review Panel | 2026 S2 2026-01-01 to 2026-12-31 | Annual progression | Ratified | Outcome Deferred | Open 
 | 
 Nomsa Mahlangu | [Neonatal panel] | 2026 S1 2026-01-01 to 2026-06-30 | Entrustment only | Ratified | Entrustment decisions only | Open 
 | 
 Anele Dlamini | [Neonatal panel] | 2026 S1 2026-01-01 to 2026-06-30 | Entrustment only | Ratified | Entrustment decisions only | Open 
 |


######## BOARD R2-5-committee-reviews-390-a.dc.html — Committee reviews · 390 · a
Committee reviews · 390 · a 
# Committee reviews · 390px
 Route /committee/reviews · header action “Schedule a review”, a link to the schedule page.
 Components PageHeader, DataTable (stack-wide), Badge (StateBadge's tints: Scheduled grey on both lists), StatePanel. NEW .period-window.
 DESIGN.md § Table system: no Mode column (C8); stacked at 900px and below (M8). Round 3 F6, M8, S2, S7.
=== FRAME: | Empty, for a scheduler | | Pieter Smit · the header action reads “Schedule a review”, the page's own words (S7) | 
 [shell] 
# Committee reviews
 Schedule progression reviews and open existing ones.
 Schedule a review 
 No reviews yet
 The reviews you can open appear here once they are scheduled. Use Schedule a review to put a trainee before a panel.
=== FRAME: | Empty, for a member | | David Naidoo · no Schedule a review | 
 [shell] 
# Committee reviews
 Open existing committee reviews.
 No reviews yet
 The reviews of the panels you sit on appear here once they are scheduled.
=== FRAME: | Scheduled | | Step 4.10, after · Pieter Smit · Scheduled grey here and on Decisions due (S7); no Mode column; stacked at 900px and below (M8) | 
 [shell] 
# Committee reviews
 Schedule progression reviews and open existing ones.
 Schedule a review 
 The College's semester 2 ends in November; its window runs to 31 December, so December's work counts.
 Committee reviews at Kgosi Kgari Teaching Hospital, newest scheduled first
 Trainee | Panel | Period | Type | State | Decision | Actions | 
 Sipho Ndlovu | Paed Annual Review Panel | 2026 S2 2026-01-01 to 2026-12-31 | Annual progression | Scheduled | Pending | Open 
 | 
 Nomsa Mahlangu | Paed Annual Review Panel | 2026 S2 2026-01-01 to 2026-12-31 | Annual progression | Scheduled | Pending | Open 
 | 
 Pieter du Plessis | Paed Annual Review Panel | 2026 S2 2026-01-01 to 2026-12-31 | Annual progression | Scheduled | Pending | Open 
 | 
 Anele Dlamini | Paed Annual Review Panel | 2026 S2 2026-01-01 to 2026-12-31 | Annual progression | Scheduled | Pending | Open 
 | 
 Lerato Molefe | Paed Annual Review Panel | 2026 S2 2026-01-01 to 2026-12-31 | Annual progression | Scheduled | Pending | Open 
 | 
=== FRAME: | Ratified: decisions by label | | Step 4.33 · Pieter Smit | 
 [shell] 
# Committee reviews
 Schedule progression reviews and open existing ones.
 Schedule a review 
 The College's semester 2 ends in November; its window runs to 31 December, so December's work counts.
 Committee reviews at Kgosi Kgari Teaching Hospital, newest scheduled first
 Trainee | Panel | Period | Type | State | Decision | Actions | 
 Sipho Ndlovu | Paed Annual Review Panel | 2026 S2 2026-01-01 to 2026-12-31 | Annual progression | Ratified | Outcome Deferred | Open 
 | 
 Nomsa Mahlangu | Paed Annual Review Panel | 2026 S2 2026-01-01 to 2026-12-31 | Annual progression | Ratified | Inadequate Progress — Additional Training | Open 
 | 
 Pieter du Plessis | Paed Annual Review Panel | 2026 S2 2026-01-01 to 2026-12-31 | Annual progression | Ratified | Satisfactory with Observations | Open 
 | 
 Anele Dlamini | Paed Annual Review Panel | 2026 S2 2026-01-01 to 2026-12-31 | Annual progression | Ratified | Satisfactory Progress | Open 
 | 
 Lerato Molefe | Paed Annual Review Panel | 2026 S2 2026-01-01 to 2026-12-31 | Annual progression | Ratified | Satisfactory Progress | Open 
 | 
=== FRAME: | Heavy, after Step A.1.14 (S2) | | Pieter Smit · Sipho Ndlovu's three reviews show his pseudonym as stored; Entrustment only appears only in 2026 S1, before the neonatal panel; a check-in's Decision “—” reads “None” (S7) | 
 [shell] 
# Committee reviews
 Schedule progression reviews and open existing ones.
 Schedule a review 
 The College's semester 2 ends in November; its window runs to 31 December, so December's work counts.
 Committee reviews at Kgosi Kgari Teaching Hospital, newest scheduled first
 Trainee | Panel | Period | Type | State | Decision | Actions | 
 Lerato Molefe | Paed Annual Review Panel | 2026 S2 2023-01-15 to 2026-12-31 | Pre-graduation | Scheduled | Pending | Open 
 | 
 [Pseudonym, as stored] | Paed Annual Review Panel | 2026 S2 2026-01-01 to 2026-12-31 | Formative check-in | Withdrawn | — None | Open 
 | 
 [Pseudonym, as stored] | Paed Annual Review Panel | 2026 S2 2026-01-01 to 2026-12-31 | Formative check-in | Closed | — None | Open 
 | 
 Nomsa Mahlangu | Paed Annual Review Panel | 2026 S2 2026-01-01 to 2026-12-31 | Annual progression | Ratified | Inadequate Progress — Additional Training | Open 
 | 
 Pieter du Plessis | Paed Annual Review Panel | 2026 S2 2026-01-01 to 2026-12-31 | Annual progression | Ratified | Satisfactory with Observations | Open 
 | 
 5 more rows


######## BOARD R2-5-committee-reviews-390-b.dc.html — Committee reviews · 390 · b
Committee reviews · 390 · b 
# Committee reviews · 390px (continued, part b) 
 Route /committee/reviews · header action “Schedule a review”, a link to the schedule page.
 Components PageHeader, DataTable (stack-wide), Badge (StateBadge's tints: Scheduled grey on both lists), StatePanel. NEW .period-window.
 DESIGN.md § Table system: no Mode column (C8); stacked at 900px and below (M8). Round 3 F6, M8, S2, S7.
=== FRAME: | A member's list | | Step 4.11 · David Naidoo | 
 [shell] 
# Committee reviews
 Open existing committee reviews.
 The College's semester 2 ends in November; its window runs to 31 December, so December's work counts.
 Committee reviews at Kgosi Kgari Teaching Hospital, newest scheduled first
 Trainee | Panel | Period | Type | State | Decision | Actions | 
 Sipho Ndlovu | Paed Annual Review Panel | 2026 S2 2026-01-01 to 2026-12-31 | Annual progression | Scheduled | Pending | Open 
 | 
 Nomsa Mahlangu | Paed Annual Review Panel | 2026 S2 2026-01-01 to 2026-12-31 | Annual progression | Scheduled | Pending | Open 
 | 
 Pieter du Plessis | Paed Annual Review Panel | 2026 S2 2026-01-01 to 2026-12-31 | Annual progression | Scheduled | Pending | Open 
 | 
 Anele Dlamini | Paed Annual Review Panel | 2026 S2 2026-01-01 to 2026-12-31 | Annual progression | Scheduled | Pending | Open 
 | 
 Lerato Molefe | Paed Annual Review Panel | 2026 S2 2026-01-01 to 2026-12-31 | Annual progression | Scheduled | Pending | Open 
 | 
=== FRAME: | Loading | | C11 · Schedule a review is offered once the read is in | 
 [shell] 
# Committee reviews
 Schedule progression reviews and open existing ones.
 Loading Committee reviews.
=== FRAME: | Load error | | C11, T338 | | Focus: the alert | 
 [shell] 
# Committee reviews
 Schedule progression reviews and open existing ones.
 Schedule a review 
 Could not load Committee reviews. Nothing has changed. Try again, or come back in a few minutes. Try again