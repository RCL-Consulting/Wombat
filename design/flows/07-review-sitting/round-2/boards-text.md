

======== R2-Spec.dc.html ========
 
 R2 spec 
 Flow 07 · round 2 · Spec
 A, "the agenda is the page", on Wombat's tokens (tokens.json as of flow 06; :root unchanged since flow 01). One page, one address: /committee/reviews/{ReviewId:int} . Words in brackets are figures § 8 does not give, or were in the screenshots that did not reach this round: thirteen EPA titles (PAED-001's and PAED-015's are real), encounter dates, the third PAED-001 instrument, STAR numbers, Act 5's period and the entrustment-only registrar. The canvas loads Source Sans 3 and Fraunces from Google Fonts for preview only; the build self-hosts them as today.
 Components: new and changed
 Component Classes What it is, and its rules 
 SittingBar NEW .sitting-bar , .sitting-bar--reader , __state (a Badge), __count , __sentence , __as-of , __actions A section named "The sitting" (visually hidden h2), first under PageHeader. Holds the review's state, what is staged ("3 staged", "13 staged"), who is present (from the form's ticks, once started), and the next act: Start review…, Go to the decision, Ratify…, Close review…. For readers: the who-may-act sentence and the Refresh line in place of the acts. 4px left stripe: secondary-color when the act is the reader's (the chair; flow 03's stripe rule), input-border for others. From 641px position: sticky; top: 56px (the top bar's height, a literal as the shell writes it), its z-index under the top bar's, so it never paints over it (T356, C8); a table's .table-container creates no stacking context above it. Below 641px static, under the title, its parts stacked and its button full width (Q9). Re-rendered after every action, so the who-may-act sentence is never stale (T260). 
 Agenda line (DataTable row) CHANGED .agenda-row--open , .agenda-form-row , .agenda-result-row , .agenda-staged ; stacked: .agenda-line , .agenda-line--open Columns EPA (code over its title, which wraps), Window, State (badge over its sentence), Evidence, Action (chair only). Stage opens the line in place: the open row and the form row under it share the 4px secondary-color stripe (the existing "row open for editing" rule). A staged decision lives in its line's State cell ("Level 5 · issued 2026-10-05 · no expiry. Rests on 2 items of the snapshot: …") with Remove… ( .btn-quiet--danger , named "Remove the staged decision on PAED-012"). Stage is named "Stage a decision on PAED-0xx". A result shows in .agenda-result-row under its line and takes the focus. Stacks into .agenda-line cards when the card is narrower than 900px (the 768 board, and 390). 
 AgendaLineForm NEW .agenda-form (grid 1fr 1fr, one column below a 760px container), .snapshot-pick (fieldset), .snapshot-pick__line , .agenda-form__standing Header "Stage a decision on PAED-001" (h3); one line of standing ("Year 4 target 5 · no STAR yet · 3 ratings in the review window · MSF: Covered") and "Rating chart for PAED-001"; "Evidence it rests on": this EPA's snapshot lines first, each with its tick and Read, then "Show the other N snapshot lines"; the hints (one item; none about the EPA; none finished at Start) beside the ticks as info alerts that refuse nothing; then Authorised level, Issued on (today on the South African calendar, T325), Expires on (optional), Rationale, Stage pending decision (disabled with "Name at least one item of the evidence snapshot." until a tick), Cancel. A refusal sits at the form's top (RefusalSummary) and takes the focus; the field is marked; all typed is kept (A5). Focus on opening: Authorised level. 
 "Stage a decision on an EPA not on this agenda" CHANGED the same .agenda-form , with an EPA select Replaces the old staging form, at the agenda's foot. EPA select: the curriculum's EPAs ("The EPAs on the trainee's curriculum."). Staged, the EPA gains a chair's line ("Added by the chair") saying what it decides again. Step 4.20's refusal shows here. 
 EvidenceReader NEW .evidence-reader Inside an open line only: Read swaps the form column for the activity, read-only: flow 03's ActivityStatus, ActivityAbout and ActivityForm (read-only), no moves. "← Back to PAED-001" (a button) restores the form exactly as typed (same component, no navigation) and puts the focus on the Read pressed. On opening, the reader's heading (tabindex −1) takes the focus. 4px input-border stripe: someone else's record. 
 Chart link pair CHANGED .chart-jump , .chart-back Flow 05's T355 link becomes a focus move to that EPA's chart card heading in Rating trajectory by EPA. While the line is open, the chart's heading offers "Back to PAED-001's line", which returns the focus to the link. Chart and table unchanged (flow 05). 
 Decision form CHANGED FormField in .form-grid ; .quorum-line ( aria-live="polite" ) After the agenda; reached by the bar's "Go to the decision" (focus on its heading). Category, Rationale, About the EPAs not decided (optional) with help "The registrar sees this once the review is ratified." (C1d), Conditions, Present (the chair ticked and locked). The quorum line reads from the ticks: "Only the chair is ticked. …" or "4 present: the chair and 3 others. The quorum is met." Record decision… is disabled only while the quorum falls short (C1c), its reason beside it ( aria-describedby ). Missing category or rationale: refused in Wombat's words (T324). Recorded, the card moves above the agenda. 
 Refresh line NEW .sitting-bar__as-of ( role="status" ), .btn-sm Refresh with refresh-cw Members' and administrators' bars only: "As of 14:02 SAST" and Refresh, which re-reads the review in place, keeps the scroll position and leaves the focus on Refresh. Not live (C4); live following is T362, not drawn. The chair's page has none. 
 Return link on flow 03's activity page CHANGED .return-link "← Back to Lerato Molefe · 2026 S2" above the trail, only when the page was opened from a review's Evidence snapshot ( ?from=review-{id}&at=snap-{itemId} ); goes to /committee/reviews/{id}#snap-{itemId} , focus on that line's link. 44px row on a phone. 
 PageHeader link CHANGED .page-header-link Under the subtitle, "Lerato Molefe's progress" to flow 06's /programme/trainees/{ProfileId:int} , drawn only for readers that page admits (the administrator here; a committee member only if flow 06 admits one: to confirm). 
 ConfirmDialog uses CHANGED flow 01's native <dialog> Start (what it freezes), Remove (the EPA, what is discarded; danger fill), Record (category, fixed decisions, the EPAs recorded as not decided, who is present), Ratify (each STAR; "PAED-002 and 8 others were not decided at this sitting."; no undo; no mail promised, T320), Close review (no decision). The safe button opens with the focus; Cancel returns it to the opener; the result takes it after. 
 Review card, Pending entrustment decisions card, deferral form, Defer, Reinstate, "Defer lines still due…", the sampling card's × GONE Their parts move to the sitting bar and the agenda lines; deferral is gone (D53); the notes card is not dismissible (T280). 
 DESIGN.md: rule text that changes
 Section New text 
 § Committee agenda The agenda is the review page's spine: one line per EPA due in the window, then the chair's lines. A line opens in place to stage a decision; a staged decision is shown in its line, not in a separate list. There is no deferral (D53): a line due at this sitting that is not staged when the decision is recorded becomes "Not decided at this sitting", with the decision's optional sentence under it. The caption counts what is staged ("15 EPAs for 2026 S2 · 3 staged"). 
 § The chair's controls The chair's acts live in the sitting bar (Start, Go to the decision, Ratify, Close review) and in the agenda's lines (Stage, Remove); everyone else reads the who-may-act sentence in the bar's place, and the controls are not drawn. Record is held back only by the quorum. Start is not offered while the chair cannot act (T260). 
 § Entrustment-only reviews The decision form has no Category; the record reads "Entrustment decisions only"; "staged below" becomes "staged above". 
 § Detail page and its details-grid The committee review is the one detail page with no details-grid: one column in a fixed order (sitting bar, notes, agenda, decision, standing, MSF, snapshot, trajectories, appeals); the decision card rises above the agenda once recorded. 
 § Page-level patterns · the title rule A record page's h1 names its subject: "Lerato Molefe · 2026 S2", and the tab "Lerato Molefe · 2026 S2 · Wombat" (T190); while loading, "Committee review". 
 The owner table, a committee review Unchanged route; Committee reviews lights for every role that reaches it. The activity page opened from a review keeps its own owner and gains the return link. 
 D53: words that change elsewhere (C1h; those pages are not redrawn here)
 Where Was Now 
 This page, a line due at this sitting Due · "Must be decided at this sitting, or deferred with a reason." Due at this sitting (no sentence) 
 This page, captions and bar "15 EPAs for 2026 S2, 12 still to stage or defer" "15 EPAs for 2026 S2 · 3 staged" · bar "3 staged" 
 This page, after Record Deferred · "Reason: …" · Reinstate Not decided at this sitting · the optional sentence 
 This page, Record's reason "Record decision: PAED-001, … must be decided at this sitting. Stage a decision on each, or defer it with a reason." Only the quorum sentence 
 This page, the who-may-act sentence "… can stage entrustment decisions, defer agenda lines and record …" "… can stage entrustment decisions and record the committee's decision." 
 Decisions-due page, a status Deferred Not decided (a sitting held it and did not decide it). Missed stays (the window ended and no sitting held it). 
 The registrar's reviews page, a line "Deferred. Reason: …" "Not decided at this sitting." and the sentence, once ratified 
 States as badges (Badge's five tints; words on the tint in text-color)
 Scheduled In progress Decided Ratified Closed Due at this sitting As opportunity allows Due by year end Partial period Staged Decided Decided elsewhere Not decided at this sitting Not decided 
 Tokens and contrast (DESIGN.md § Contrast figures; no NEW colour token)
 Pair, where Needs Ratio 
 text-color on surface-color (cards, bar, dialogs) · on background-color 4.5 12.63 · 11.99 
 muted-text on surface (the bar's present line, help, captions) · on header-bg (fold strips are canvas only) 4.5 5.09 · 4.57 
 text-color on the info · warning · success tints (badges, alerts, the open snapshot line) 4.5 11.84 · 11.89 · 11.23 
 link-color on surface (chart link, Read, quiet links) 4.5 6.70 
 on-fill on secondary-color (Start, Ratify, Record, Stage pending decision) · on danger-color (Remove the decision) 4.5 4.86 · 5.95 
 secondary-color outline buttons on surface (Stage, Go to the decision, Refresh, Not yet) 4.5 4.86 
 danger-color as words: Remove… (quiet danger), a field's refusal, on surface 4.5 5.95 
 input-border on surface (controls, ticks, the readers' bar stripe, the reader's stripe) 3 3.80 
 secondary-color stripe on surface (the chair's bar, the open line) 3 4.86 
 focus-ring on surface · background · info tint 3 4.86 · 4.61 · 4.55 
 Badge and alert edges and icons on their tints: info · warning · success · danger 3 4.55 · 5.43 · 5.23 · 5.56 
 Neutral badge text · edge on header-bg 4.5 · 3 11.36 · 3.42 
 Spacing: card padding and gaps space-lg 24px; the agenda's cells 12px (the shell's literal); the open line's grid gap 24px; the bar's 10px/16px inside a row (literals as the shell). Radii: cards radius-lg , tables and dialogs radius-xl , controls radius-md , badges radius-pill . Shadows: shadow-raised (cards, bar), shadow-dialog . Icons: refresh-cw , arrow-left and chart-line are added from Lucide if not among the 60. The "Focus" chips and the dashed fold strips are canvas notation, not product.
 Targets
 Control 1280 · 768 390 
 Buttons (Start, Ratify, Record, Stage pending decision, dialog buttons) 36px 44px, full width 
 Row actions (Stage, Remove…, Read), Refresh, Back to PAED-001 28px (btn-sm), ≥ 24 44px 
 Ticks (snapshot lines, Present): the whole labelled row is the target ≥ 36px row, 24px min 44px rows 
 Links in a line ("Rating chart for …", "Show the other N snapshot lines", the progress link) text links in a sentence (WCAG 2.5.8's inline exception) 44px rows 
 Open, for the operator
 1 C4 writes "As of 14:02 · Refresh"; BRIEF.md's content rule shows the zone. The boards draw "As of 14:02 SAST". Say which. 
 2 Does flow 06's registrar page admit a committee member? If not, the chair's and members' headers carry no "Lerato Molefe's progress" (as drawn). 
 3 T324's exact sentence for a missing rationale was not in the brief; the board brackets it. 
 4 The thirteen EPA titles, Act 5's period and the encounter dates: send the catalogue (or Step 4.15's screenshot) and round 3 fills every bracket. 


======== R2-Steps.dc.html ========
 
 R2 steps 
 Flow 07 · round 2 · Steps: what each Expect will say once A is built
 The 26 steps of round 1's § 8. Only what changes is written; anything not named stays as the runbook has it. D = the replay day (2026-10-05 in the boards). CHANGED GONE mark the step.
 Step Role Expect, once built 
 4.14 Sarah Botha Unchanged (flow 08's Decision panels). 
 4.15 Thandi Zulu (chair) CHANGED h1 and tab "Lerato Molefe · 2026 S2"; subtitle "Annual progression review · Paed Annual Review Panel · Training year 4". The sitting bar reads Scheduled, "15 EPAs on the agenda", and offers Start review… . "Before the committee decides" carries the sampling warnings (PAED-010, PAED-012; the two MSF records not counted), not dismissible. Agenda: 15 lines, twelve "Due at this sitting" (no sentence), three As opportunity allows, Evidence 0 items, no Action column. Decision card below: "No decision has been recorded yet." Standing, MSF, "No evidence has been frozen yet …", the three charts, Appeals as before. No Review card, no Pending card. 
 4.16 Thandi Zulu CHANGED Start review… opens a dialog naming the 8 items and the window; Start review answers "Review started." atop the agenda, with the focus; In progress. Caption "15 EPAs for P · none staged"; each line counts its snapshot items and offers Stage only (no Defer). The decision form (Category, Rationale, About the EPAs not decided (optional), Conditions, Present) has Record decision… disabled with "Only the chair is ticked. A committee decision needs at least two panel members present: the chair and at least one other." It names no EPA. "Stage a decision on an EPA not on this agenda" at the agenda's foot. 
 4.17 Thandi Zulu CHANGED From the Evidence snapshot card, the Mini-CEX opens on flow 03's page with "← Back to Lerato Molefe · 2026 S2"; Back lands on that snapshot line, focus on its link. (Or, inside PAED-001's open line, Read shows it in place and Back restores the form.) Stage on PAED-001's line opens the line in place, focus on Authorised level (no EPA select: the line is the EPA). The line shows "Year 4 target 5 · no STAR yet · 3 ratings in the review window · MSF: Covered" and "Rating chart for PAED-001"; PAED-001's three lines first. One tick: the hint. After staging: "Pending entrustment decision staged." under the line, focus there; the line reads Staged with "Level 5 · issued D · no expiry. Rests on 2 items of the snapshot: …"; caption and bar "1 staged". 
 4.18 Thandi Zulu CHANGED Refused at the top of PAED-010's open line, focus there; Expires on marked with the same sentence; all typed kept; PAED-010 still Due at this sitting. 
 4.19 Thandi Zulu CHANGED Both staged; PAED-010 "issued D · expires D+20" in its line; "3 staged". Remove… opens a dialog naming PAED-012; Remove the decision answers "Pending entrustment decision removed." in the line, focus there; PAED-012 Due at this sitting with Stage; "2 staged". Staged again: "3 staged". 
 4.20 Thandi Zulu CHANGED In "Stage a decision on an EPA not on this agenda", PAED-001 chosen: refused at that form's top with the same sentence, focus there; PAED-001's line still holds exactly one decision, at 5. 
 4.21 Thandi Zulu GONE Deferring is gone (D53). Nothing replaces it here: the nine lines left unstaged are recorded as not decided at 4.25's Record, with the decision's optional sentence. The empty-reason refusal (T212) and Reinstate go with it. 
 4.22 David Naidoo CHANGED Everything the chair sees, none of her controls: four columns, no Stage, no Remove, no forms, no "Go to the decision". The bar reads "Only the panel's chair, Thandi Zulu, can stage entrustment decisions and record the committee's decision." and "As of hh:mm SAST" with Refresh; Refresh re-reads in place, focus kept. The staged lines show their decisions; the rest Due at this sitting (no Deferred badge). Decision card: "No decision has been recorded yet." 
 4.23 Thandi Zulu CHANGED Go to the decision (focus on its heading); Satisfactory Progress and a rationale; only herself ticked: Record decision… is disabled and beside it "Only the chair is ticked. A committee decision needs at least two panel members present: the chair and at least one other." Nothing can be pressed, nothing recorded. (The server refusal stays as backstop.) 
 4.24 Thandi Zulu CHANGED Three ticked: "4 present: the chair and 3 others. The quorum is met." Category back to "Select a category…", Record decision…: refused at the form's top, before any dialog, "This review decides the trainee's progression, so its decision records a progression category. Choose one.", focus there; Category marked; nothing recorded. 
 4.25 Thandi Zulu CHANGED Also write "To be decided at her final review." in About the EPAs not decided. Record decision… opens the dialog: "Fixes 3 staged decisions: PAED-001 at 5, PAED-010 at 4 (expires D+20), PAED-012 at 5. Records 9 EPAs due at this sitting as not decided: PAED-002, PAED-003, PAED-004, PAED-005, PAED-006, PAED-007, PAED-011, PAED-014 and PAED-015." and the four present. Record decision: "Decision recorded." in the Decision card (now above the agenda), focus there; Decided. Card: "Satisfactory Progress", the rationale, "About the EPAs not decided: …", "Present: Thandi Zulu (chair), David Naidoo, Sarah Botha, John van Rensburg (external)". The nine lines read "Not decided at this sitting" with the sentence; the caption "Fixed when the committee's decision was recorded: ratifying issues exactly these."; no Action column; the bar offers Ratify…. No chair-rated warning. 
 4.26 Nolwazi Mbatha CHANGED The bar: Decided, "Only the panel's chair, Thandi Zulu, can ratify the committee's decision.", "As of … SAST" and Refresh. Under the h1, "Lerato Molefe's progress" (flow 06). No Ratify, no Remove, no Action column. 
 4.27 Thandi Zulu CHANGED Ratify… opens a dialog listing the three STARs and "PAED-002 and 8 others were not decided at this sitting."; nothing about mail. "Decision ratified." under the bar, focus there; Ratified. PAED-001, 010, 012 Decided, "STAR #n."; the nine "Not decided at this sitting" with "To be decided at her final review."; PAED-008, 009, 013 Not decided, "The review was ratified without deciding it."; the agenda's caption "Nothing is pending: …". Standing and exit rule as the runbook. 
 4.28 Thandi Zulu CHANGED Start (dialog), stage PAED-001 at 4 and PAED-002 at 3b (the hint "None of the named items is about PAED-002."). The ten-line deferral goes: those lines stay Due at this sitting. Record is enabled as soon as a second member is ticked. 
 4.29 Thandi Zulu CHANGED About the EPAs not decided: "Not at a decision point this year."; the Record dialog names the 10 EPAs it records as not decided; after Ratify those ten read "Not decided at this sitting" with the sentence. 
 4.30 Thandi Zulu CHANGED No deferral of the twelve: "Not at a decision point this year." goes in About the EPAs not decided. Recorded and ratified: the twelve read "Not decided at this sitting" with it; "Nothing was staged when the review was ratified, so ratifying it issued no STAR." 
 4.31 Thandi Zulu CHANGED "Not enough observed evidence yet this year." in About the EPAs not decided, in place of twelve deferrals. 
 4.32 Thandi Zulu CHANGED "Too early in the programme to decide." in About the EPAs not decided, in place of twelve deferrals. Outcome Deferred stays a progression category (unrelated to D53). 
 4.50 Thandi Zulu CHANGED Type and subtitle read "Formative check-in" (T309). Start (dialog): "Review started.". Close review… opens a dialog ("It records no decision and issues no STAR. …"); "Formative review closed." with the focus; Closed; nothing offered. Others read "Only the panel's chair, Thandi Zulu, can close this review." in the bar. 
 5.3 Thandi Zulu CHANGED Start (dialog). "Record decision is shown disabled, and names the closing agenda lines …" becomes: Record decision… is disabled only for the quorum, with its sentence; it names no EPA. 
 5.4 Thandi Zulu CHANGED Stage on each line opens it in place; PAED-010 from "Stage a decision on an EPA not on this agenda", gaining a chair's line ("Added by the chair", what it decides again). After each: the result under that line, focus there; the bar and caption count up to "13 staged". "Once every closing line is staged, Record decision is enabled" becomes: Record is enabled once a second member is ticked. 
 5.5 Thandi Zulu CHANGED The Record dialog names the 13 fixed decisions and records none as not decided. Then as the runbook, the record above the agenda. 
 5.6 Thandi Zulu CHANGED The Ratify dialog lists 13 STARs by level. Then as the runbook. 
 A.7.6 Thandi Zulu, 390 CHANGED The sitting bar under the title, not sticky; the agenda as stacked lines with 44px Stage; an open line one column; dialogs at the foot, full width; charts 322 px, never scaled, tables under them; nothing scrolls sideways. 
 A.7.13 Thandi Zulu, NVDA CHANGED Headings: h1 "Lerato Molefe · 2026 S2"; h2 "The sitting" (visually hidden), "Before the committee decides", "Agenda", "Decision", "Entrustment against Annexure A", "Multi-source feedback by EPA", "Evidence snapshot", "Rating trajectory by EPA", "Appeals"; h3 per EPA group and per chart; an open line's h3 "Stage a decision on PAED-0xx". "Review" is gone (its contents are the bar). Tables: the agenda (five columns for the chair, four for others), the snapshot's, each chart's. Every row action named for its row; badges read as words; the quorum line and "As of" announce. 
 Recount (presses + fields typed)
 Journey Round 1 A Round 2 · 1280 Round 2 · 390 What changed 
 Act 4 sitting, 4.15–4.27, happy path 38 (with the bulk deferral) 36 36 Open 2 · Start 2 · evidence and back 2 · stage three 19 (6 + 7 + 6) · Go to the decision 1, category, rationale, the optional sentence, 3 ticks, Record, confirm 8 · Ratify 2. No deferral (−3), one new field (+1). At 390 the same presses, about 2,400 px more scrolling (bar not sticky). 
 Act 4 sitting, as the runbook walks it (refusals, remove and restage) — 54 54 4.18 7 · 4.19 16 · 4.20 5 · 4.23 3 (nothing to press) · 4.24 5 · 4.21's deferral and reinstatement (about 30 under round 1's per-line form) gone. 
 Pre-graduation, thirteen staged, 5.3–5.6 93 93 93 13 stages × 6, PAED-010's EPA select +1, Record 8, Ratify 2, open and Start 4. 
 4.28–4.32, each — −29 to −35 same 10 or 12 deferrals at 3 each gone; one optional sentence at Record. 
 Member 4.22 · administrator 4.26 · formative 4.50 2 · 2 · 6 2 · 2 · 6 2 · 2 · 6 Refresh not counted (only when needed). 


======== R2-1280-start.dc.html ========
 
 R2 1280 before the sitting 
### BOARD FRAME: 1 · Scheduled · the chair. Start review… in the sitting bar. The notes card above the agenda; the agenda has no Action column until the review starts. EPA titles other than PAED-001's: bracketed , the catalogue screenshot did not reach this round. 
 Wombat
 Acting as Committee member Switch to Assessor 
 Home Committee reviews Decision panels My data rights 
 Thandi Zulu Sign out 
 Committee reviews › Lerato Molefe · 2026 S2 
 Lerato Molefe · 2026 S2
 Annual progression review · Paed Annual Review Panel · Training year 4
 The sitting
 Scheduled 15 EPAs on the agenda The evidence is frozen when the review starts. Start review… 
 Before the committee decides
 Sampling concentration warnings
 PAED-010 · 1 rating · 1 assessor · 1 source. Fewer than three distinct assessors across this EPA's evidence. 
 PAED-012 · 2 ratings · n assessors · n sources . Fewer than three distinct assessors across this EPA's evidence. 
 Not counted: 2 records in the review window with no rating by a named assessor …
 Agenda
 15 EPAs for 2026 S2 
 EPA Window State Evidence 
 PAED-001 Providing paediatric emergency care to children 2026 Due at this sitting 0 items 
 PAED-002 catalogue title 2026 S2 Due at this sitting 0 items 
 Below, in order: PAED-003 to PAED-015 (PAED-008, 009 and 013 "As opportunity allows") · Decision ("No decision has been recorded yet.") · Entrustment against Annexure A · Multi-source feedback by EPA · Evidence snapshot ("No evidence has been frozen yet …") · Rating trajectory by EPA · Appeals
### BOARD FRAME: 1 · Scheduled · the chair cannot act (C5, T260, Q11). The bar's sentence in place of Start; nothing is offered. Re-read after every action. The reason inside the sentence is the system's; bracketed . 
 Wombat
 Acting as Committee member Switch to Assessor 
 Home Committee reviews Decision panels My data rights 
 Thandi Zulu Sign out 
 Committee reviews › Lerato Molefe · 2026 S2 
 Lerato Molefe · 2026 S2
 Annual progression review · Paed Annual Review Panel · Training year 4
 The sitting
 Scheduled The panel's chair, Thandi Zulu, cannot take the chair's actions now: the reason the system gives . A panel administrator must seat a chair who can. 
 Before the committee decides
 PAED-010 · 1 rating · 1 assessor · 1 source. Fewer than three distinct assessors across this EPA's evidence. 
 Agenda
 15 EPAs for 2026 S2 
 EPA Window State Evidence 
 PAED-001 Providing paediatric emergency care to children 2026 Due at this sitting 0 items 
 The same slot, the trainee elsewhere: "This review's trainee does not train at the panel's institution, so the panel cannot act on it." — no Start, no Stage, no decision form, for every reader.
### BOARD FRAME: 1 · Start review… confirms (ConfirmDialog). Names the registrar, the period and what it freezes; the focus opens on Not yet. Item count and window from Step 4.16. 
 Wombat
 Acting as Committee member Switch to Assessor 
 Home Committee reviews Decision panels My data rights 
 Thandi Zulu Sign out 
 Committee reviews › Lerato Molefe · 2026 S2 
 Lerato Molefe · 2026 S2
 Annual progression review · Paed Annual Review Panel · Training year 4
 Scheduled 15 EPAs on the agenda The evidence is frozen when the review starts. Start review… 
 Start Lerato Molefe's review for 2026 S2?
 Starting freezes the evidence: the 8 items observed from 2026-01-01 to 2026-12-31 become this review's snapshot, each in the state it is in now. Anything finished later is not added.
 Not yet Start review 
### BOARD FRAME: 2 · Started. "Review started." atop the agenda takes the focus. Stage on each line; Record is held back only by the quorum (C1c). Caption and bar count what is staged (C1b). 
 Wombat
 Acting as Committee member Switch to Assessor 
 Home Committee reviews Decision panels My data rights 
 Thandi Zulu Sign out 
 Committee reviews › Lerato Molefe · 2026 S2 
 Lerato Molefe · 2026 S2
 Annual progression review · Paed Annual Review Panel · Training year 4
 The sitting
 In progress None staged Present: the chair · one more member needed Go to the decision 
 Agenda
 15 EPAs for 2026 S2 · none staged 
 Review started.
 EPA Window State Evidence Action 
 PAED-001 Providing paediatric emergency care to children 2026 Due at this sitting 3 items Stage 
 PAED-002 catalogue title 2026 S2 Due at this sitting 0 items Stage 
 PAED-008 catalogue title 2026 As opportunity allows 0 items Stage 
 Below: the other lines · Stage a decision on an EPA not on this agenda · Decision (the form) · Entrustment against Annexure A · MSF by EPA · Evidence snapshot (frozen, by EPA then instrument) · Rating trajectory by EPA · Appeals


======== R2-1280-stage.dc.html ========
 
 R2 1280 staging in the line 
### BOARD FRAME: 2 · Stage chosen on PAED-001 (Step 4.17). The line opens in place, sharing the open row's stripe. Focus on Authorised level. One item ticked: the hint beside the ticks. The sitting bar is stuck under the top bar (C8). 
 Wombat
 Acting as Committee member Switch to Assessor 
 Home Committee reviews Decision panels My data rights 
 Thandi Zulu Sign out 
 In progress None staged Present: the chair · one more member needed Go to the decision 
 Agenda
 15 EPAs for 2026 S2 · none staged 
 EPA Window State Evidence Action 
 PAED-001 Providing paediatric emergency care to children 2026 Due at this sitting 3 items Stage 
 Stage a decision on PAED-001
 Year 4 target 5 · no STAR yet · 3 ratings in the review window · MSF: Covered · Rating chart for PAED-001 
 Evidence it rests on 
 Mini-CEX · Thandi Zulu Rated 5 · date · Completed Read 
 CBD · David Naidoo Rated rung · date · Completed Read 
 instrument · Mohammed Patel Rated rung · date · Completed Read 
 Show the other 5 snapshot lines 
 Only one item is named. The College's rule is that an entrustment decision is never taken from a single form.
 Authorised level Choose a rung… Rungs of CPSA Paediatric Entrustment Scale v11.1, the ladder this EPA is assessed on. 
 Issued on 
 Expires on (optional) 
 Rationale 
 Stage pending decision Cancel 
 Below: PAED-002 … PAED-015, each Due at this sitting or As opportunity allows, with Stage. Before a line is ticked, Stage pending decision is disabled with "Name at least one item of the evidence snapshot." beside it.
### BOARD FRAME: 3 · Read, in place (C3). The activity takes the form's place: flow 03's ActivityStatus and ActivityAbout, read-only, no moves. The focus goes to its heading. Nothing navigates; the form waits, as typed, behind Back. 
 Wombat
 Acting as Committee member Switch to Assessor 
 Home Committee reviews Decision panels My data rights 
 Thandi Zulu Sign out 
 In progress None staged Present: the chair · one more member needed Go to the decision 
 EPA Window State Evidence Action 
 PAED-001 Providing paediatric emergency care to children 2026 Due at this sitting 3 items Stage 
 Stage a decision on PAED-001
 Year 4 target 5 · no STAR yet · 3 ratings in the review window · MSF: Covered · Rating chart for PAED-001 
 Evidence it rests on 
 Mini-CEX · Thandi Zulu Rated 5 · date · Completed · open beside Read 
 CBD · David Naidoo Rated rung · date · Completed Read 
 instrument · Mohammed Patel Rated rung · Completed Read 
 Back to PAED-001 
 Mini-CEX · PAED-001 
 Completed Done. You completed it on date . 
 Rated 5. Credited 1 item to PAED-001.
 Trainee Lerato Molefe Assessor Thandi Zulu Encounter date Instrument Mini-CEX 
 The form's sections follow, read-only (ActivityForm), as flow 03 draws them.
 EvidenceReader (NEW) · tabindex −1 heading takes the focus · no moves · "You completed it" because the reader is its assessor; another member reads "Thandi Zulu completed it on …" as flow 03 words it.
### BOARD FRAME: 3 · Back to PAED-001. The form returns exactly as typed (level 5, the rationale, two ticks); the focus is on the Read she pressed. Two items named: no hint. 
 Wombat
 Acting as Committee member Switch to Assessor 
 Home Committee reviews Decision panels My data rights 
 Thandi Zulu Sign out 
 In progress None staged Present: the chair · one more member needed Go to the decision 
 EPA Window State Evidence Action 
 PAED-001 Providing paediatric emergency care to children 2026 Due at this sitting 3 items Stage 
 Stage a decision on PAED-001
 Year 4 target 5 · no STAR yet · 3 ratings in the review window · MSF: Covered · Rating chart for PAED-001 
 Evidence it rests on 
 Mini-CEX · Thandi Zulu Rated 5 · date · Completed Read 
 CBD · David Naidoo Rated rung · date · Completed Read 
 instrument · Mohammed Patel Rated rung · Completed Read 
 Show the other 5 snapshot lines 
 Authorised level 5 Rungs of CPSA Paediatric Entrustment Scale v11.1, the ladder this EPA is assessed on. 
 Issued on 
 Expires on (optional) 
 Rationale Leads resuscitation and emergency care without prompting; ready for unsupervised practice. 
 Stage pending decision Cancel 
### BOARD FRAME: 2 · Staged. The line closes to Staged with its decision in the cell; "Pending entrustment decision staged." under the line takes the focus and stays until the next action. Bar and caption: 1 staged. 
 Wombat
 Acting as Committee member Switch to Assessor 
 Home Committee reviews Decision panels My data rights 
 Thandi Zulu Sign out 
 In progress 1 staged Present: the chair · one more member needed Go to the decision 
 Agenda
 15 EPAs for 2026 S2 · 1 staged 
 EPA Window State Evidence Action 
 PAED-001 Providing paediatric emergency care to children 2026 Staged Level 5 · issued 2026-10-05 · no expiry. Rests on 2 items of the snapshot: Mini-CEX (Thandi Zulu), CBD (David Naidoo). 
 3 items Remove… 
 Pending entrustment decision staged.
 PAED-002 catalogue title 2026 S2 Due at this sitting 0 items Stage 
 PAED-003 catalogue title 2026 S2 Due at this sitting 0 items Stage 
 PAED-004 catalogue title 2026 Due at this sitting 0 items Stage 
 PAED-005 catalogue title 2026 Due at this sitting 0 items Stage 
### BOARD FRAME: 2 · The expiry refused (Step 4.18). The refusal at the top of the line's form takes the focus; Expires on is marked (border and stripe) with its message; all typed is kept; PAED-010 stays Due. 
 Wombat
 Acting as Committee member Switch to Assessor 
 Home Committee reviews Decision panels My data rights 
 Thandi Zulu Sign out 
 In progress 1 staged Present: the chair · one more member needed Go to the decision 
 EPA Window State Evidence Action 
 PAED-010 catalogue title 2026 S2 Due at this sitting 2 items Stage 
 Stage a decision on PAED-010
 Not staged. An expiry date must be after the issue date.
 Year 4 target 5 · no STAR yet · 1 rating in the review window · MSF: Covered · Rating chart for PAED-010 
 Evidence it rests on 
 Direct Observation · assessor Rated rung · date · Completed Read 
 MSF record · campaign Released · not rated by a named assessor Read 
 Authorised level 4 
 Issued on 
 Expires on (optional) An expiry date must be after the issue date. 
 Rationale Leads the ward round safely; one observation so far, to be confirmed at her final review. 
 Stage pending decision Cancel 
### BOARD FRAME: C2 · the chart's end. "Rating chart for PAED-001" moved the focus to that EPA's chart in Rating trajectory by EPA; its heading offers "Back to PAED-001's line", which returns the focus to the link in the still-open line. 
 Wombat
 Acting as Committee member Switch to Assessor 
 Home Committee reviews Decision panels My data rights 
 Thandi Zulu Sign out 
 In progress None staged Present: the chair · one more member needed Go to the decision 
 Rating trajectory by EPA
 Rating chart for PAED-001
 Back to PAED-001's line 
 Lerato Molefe · 3 ratings in the review window, 2026-01-01 to 2026-12-31, from Thandi Zulu, David Naidoo and Mohammed Patel. 2 at the minimum, 1 below.
 Flow 05's TrajectoryChart, drawn at 900 px, placed and not redrawn: the ladder's rungs 1, 2, 3a, 3b, 4, 5 over the review's window, with no Today.
 Date Instrument Assessor Rung 
 date Mini-CEX Thandi Zulu 5 
 date CBD David Naidoo rung 
 The link is flow 05's T355 anchor, now a focus move; the chart's region keeps flow 05's name ("Rating chart for PAED-001"). The Back link shows only while that EPA's line is open.
### BOARD FRAME: 2 · Remove… confirms. Names the EPA and what is discarded; the focus opens on Keep it; Remove the decision is the danger fill. 
 Wombat
 Acting as Committee member Switch to Assessor 
 Home Committee reviews Decision panels My data rights 
 Thandi Zulu Sign out 
 In progress 3 staged Present: the chair · one more member needed Go to the decision 
 EPA Window State Evidence Action 
 PAED-012 catalogue title 2026 Staged Level 5 · issued 2026-10-05 · no expiry. Rests on 2 items of the snapshot: … 
 2 items Remove… 
 Remove the staged decision on PAED-012?
 Its level 5, its rationale and the 2 snapshot items it rests on are discarded. PAED-012 is due at this sitting again.
 Keep it Remove the decision 
### BOARD FRAME: 2 · Removed (Step 4.19). "Pending entrustment decision removed." in the line's place takes the focus; PAED-012 reads Due at this sitting with Stage. 2 staged. 
 Wombat
 Acting as Committee member Switch to Assessor 
 Home Committee reviews Decision panels My data rights 
 Thandi Zulu Sign out 
 In progress 2 staged Present: the chair · one more member needed Go to the decision 
 Agenda
 15 EPAs for 2026 S2 · 2 staged 
 EPA Window State Evidence Action 
 PAED-011 catalogue title 2026 Due at this sitting 0 items Stage 
 PAED-012 catalogue title 2026 Due at this sitting 2 items Stage 
 Pending entrustment decision removed.
 PAED-013 catalogue title 2026 As opportunity allows 0 items Stage 
### BOARD FRAME: 2 · A second decision on PAED-001 refused (Step 4.20). The form at the agenda's foot (EPA select: the curriculum's EPAs). Refusal at its top, focus there, typed kept; the staged PAED-001 at 5 is untouched. 
 Wombat
 Acting as Committee member Switch to Assessor 
 Home Committee reviews Decision panels My data rights 
 Thandi Zulu Sign out 
 In progress 3 staged Present: the chair · one more member needed Go to the decision 
 EPA Window State Evidence Action 
 PAED-015 Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research 2026 Due at this sitting 0 items Stage 
 Stage a decision on an EPA not on this agenda
 Not staged. An entrustment decision on PAED-001 is already staged at this review. Remove it to stage another.
 EPA PAED-001 — Providing paediatric emergency care to children The EPAs on the trainee's curriculum. 
 Authorised level 4 
 Rationale [as typed in the runbook's step] 
 Evidence it rests on Mini-CEX · Thandi Zulu Rated 5 · Completed 
### BOARD FRAME: 2 · The chair's line for an EPA not on the agenda (Step 5.4, PAED-010). Staged from the foot form, PAED-010 gains a line of its own, with what it decides again; the result under it takes the focus. 
 Wombat
 Acting as Committee member Switch to Assessor 
 Home Committee reviews Decision panels My data rights 
 Thandi Zulu Sign out 
 In progress 13 staged Present: the chair · one more member needed Go to the decision 
 Agenda
 15 EPAs for period · 13 staged 
 EPA Window State Evidence Action 
 PAED-009 catalogue title window Staged Level 4 · issued D · no expiry. Rests on 2 items of the snapshot: … 
 n items Remove… 
 PAED-010 catalogue title Added by the chair Staged Not on this agenda: STAR # n decided it at 4 in this window. This decision, level 5 · issued D · no expiry, decides it again. Rests on 2 items of the snapshot: … 
 n items Remove… 
 Pending entrustment decision staged.
 PAED-011 catalogue title window Staged Level 4 · … 
 n items Remove… 
 The same line serves a STAR revoked mid-review: "PAED-0xx is no longer decided in its window: the STAR that decided it has been revoked. It is not on this agenda; stage a decision on it to decide it at this sitting." (in the notes card), then the chair's line once staged.
### BOARD FRAME: 2 · Three staged (Step 4.19). The agenda at a glance: each staged decision in its line, the rest Due at this sitting or As opportunity allows. No deferral, no reason per line (D53). 
 Wombat
 Acting as Committee member Switch to Assessor 
 Home Committee reviews Decision panels My data rights 
 Thandi Zulu Sign out 
 In progress 3 staged Present: the chair · one more member needed Go to the decision 
 Agenda
 15 EPAs for 2026 S2 · 3 staged 
 EPA Window State Evidence Action 
 PAED-001 Providing paediatric emergency care to children 2026 Staged Level 5 · issued 2026-10-05 · no expiry. Rests on 2 items: Mini-CEX (Thandi Zulu), CBD (David Naidoo). 
 3 items Remove… 
 PAED-002 catalogue title 2026 S2 Due at this sitting 0 items Stage 
 PAED-008 catalogue title 2026 As opportunity allows 0 items Stage 
 PAED-010 catalogue title 2026 S2 Staged Level 4 · issued 2026-10-05 · expires 2026-10-25. Rests on 2 items: Direct Observation, MSF record. 
 2 items Remove… 
 PAED-012 catalogue title 2026 Staged Level 5 · issued 2026-10-05 · no expiry. Rests on 2 items: two WBA lines . 
 2 items Remove… 
### BOARD FRAME: 3 · The Evidence snapshot card (Q6). Here "open" goes to flow 03's page with its return link; Back lands on this line with the focus on its link. Frozen at Start; grouped by EPA, then instrument; every state listed. 
 Wombat
 Acting as Committee member Switch to Assessor 
 Home Committee reviews Decision panels My data rights 
 Thandi Zulu Sign out 
 In progress None staged Present: the chair · one more member needed Go to the decision 
 Evidence snapshot
 Frozen when the review started, 2026-10-05 · 8 items 
 PAED-001 — Providing paediatric emergency care to children
 Item Rating Encounter State 
 Mini-CEX · Thandi Zulu 5 date Completed 
 CBD · David Naidoo rung date Completed 
 instrument · Mohammed Patel rung date Completed 
 PAED-010 — catalogue title 
 Direct Observation · MSF record (campaign's per-EPA record)
 Not about a single EPA
 MSF campaign report · campaign 
 Return address: /committee/reviews/{id}#snap-{itemId}. The headings stay as frozen: an EPA no longer in force keeps its old heading here (T273, decided 2026-09-25).


======== R2-768.dc.html ========
 
 R2 768 chair working view 
### BOARD FRAME: 768 · a line opened, the bar stuck (C8). Sidebar kept (≥641). The agenda stacks (narrower than 900 beside the sidebar); the line's form goes to one column. The bar wraps under the 56 px top bar and never paints over it. 
 Wombat
 Acting as Committee member Switch to Assessor 
 Home Committee reviews Decision panels My data rights 
 Thandi Zulu Sign out 
 In progress 1 staged Present: the chair · one more member needed Go to the decision 
 Agenda
 15 EPAs for 2026 S2 · 1 staged 
 PAED-001 Providing paediatric emergency care to children 
 Staged 
 Level 5 · issued 2026-10-05 · no expiry. Rests on 2 items.
 Remove… 
 PAED-010 catalogue title 
 Due at this sitting 
 Stage a decision on PAED-010
 Year 4 target 5 · no STAR yet · 1 rating in the review window · MSF: Covered
 Rating chart for PAED-010 
 Evidence it rests on 
 Direct Observation · assessor Rated rung · Completed Read 
 MSF record · campaign Released Read 
 Only one item is named. The College's rule is that an entrustment decision is never taken from a single form.
 Authorised level Choose a rung… Rungs of CPSA Paediatric Entrustment Scale v11.1, the ladder this EPA is assessed on. 
 Issued on 
 Expires on (optional) 
 Rationale 
 Stage pending decision Cancel 
 Read opens the activity in this column in the form's place, Back restores it, as at 1280.
### BOARD FRAME: 768 · the decision, the bar still stuck. Present and the live quorum line; Record disabled with its reason until a second member is ticked. Fields one column; Present beneath. 
 Wombat
 Acting as Committee member Switch to Assessor 
 Home Committee reviews Decision panels My data rights 
 Thandi Zulu Sign out 
 In progress 3 staged Present: the chair · one more member needed 
 Decision
 Category Satisfactory Progress 
 Rationale On track for completion; three EPAs entrusted, the rest to be decided at her final review. 
 About the EPAs not decided (optional) To be decided at her final review. The registrar sees this once the review is ratified. 
 Conditions 
 Present 
 Thandi Zulu (chair) David Naidoo Sarah Botha John van Rensburg (external) 
 Tick every panel member at this sitting. A committee decision needs at least two panel members present: the chair and at least one other. 
 Record decision… 
 Only the chair is ticked. A committee decision needs at least two panel members present: the chair and at least one other.


======== R2-1280-decide.dc.html ========
 
 R2 1280 record and ratify 
### BOARD FRAME: 2 · The decision form, only the chair ticked (Step 4.23). Reached by "Go to the decision". Present reads live from the ticks; Record decision… is disabled with the quorum sentence beside it. Unstaged lines hold nothing back (C1c). New optional field after Rationale (C1d). 
 Wombat
 Acting as Committee member Switch to Assessor 
 Home Committee reviews Decision panels My data rights 
 Thandi Zulu Sign out 
 In progress 3 staged Present: the chair · one more member needed 
 Decision
 Category Satisfactory Progress 
 Rationale On track for completion; three EPAs entrusted, the rest to be decided at her final review. 
 About the EPAs not decided (optional) The registrar sees this once the review is ratified. 
 Conditions 
 Present 
 Thandi Zulu (chair) David Naidoo Sarah Botha John van Rensburg (external) 
 Tick every panel member at this sitting. A committee decision needs at least two panel members present: the chair and at least one other. 
 Record decision… Only the chair is ticked. A committee decision needs at least two panel members present: the chair and at least one other. 
 Focus lands on the Decision heading after "Go to the decision". The server keeps its refusal with the same words, should the page be stale.
### BOARD FRAME: 2 · No category, refused (Step 4.24). The refusal at the top of the form takes the focus; Category is marked; all typed and ticked is kept (A5). A missing rationale reads in Wombat's words the same way (T324). 
 Wombat
 Acting as Committee member Switch to Assessor 
 Home Committee reviews Decision panels My data rights 
 Thandi Zulu Sign out 
 In progress 3 staged 4 present · quorum met 
 Decision
 Not recorded. This review decides the trainee's progression, so its decision records a progression category. Choose one.
 Category Select a category… Choose a progression category. 
 Rationale On track for completion; three EPAs entrusted, the rest to be decided at her final review. 
 About the EPAs not decided (optional) To be decided at her final review. The registrar sees this once the review is ratified. 
 Present 
 Thandi Zulu (chair) David Naidoo Sarah Botha John van Rensburg (external) 
 4 present: the chair and 3 others. The quorum is met. 
 Below: Conditions · Record decision… (enabled). A missing rationale: "Not recorded. Say why the committee decided this." under Rationale, never "The Rationale field is required." wording to confirm against T324's list 
### BOARD FRAME: 2 · Ready (Step 4.25). 4 present, the quorum met; Record decision… enabled, and the line beside it says what recording fixes and records. 
 Wombat
 Acting as Committee member Switch to Assessor 
 Home Committee reviews Decision panels My data rights 
 Thandi Zulu Sign out 
 In progress 3 staged 4 present · quorum met 
 Decision
 Category Satisfactory Progress 
 Rationale On track for completion; three EPAs entrusted, the rest to be decided at her final review. 
 About the EPAs not decided (optional) To be decided at her final review. The registrar sees this once the review is ratified. 
 Conditions 
 Present 
 Thandi Zulu (chair) David Naidoo Sarah Botha John van Rensburg (external) 
 Tick every panel member at this sitting. A committee decision needs at least two panel members present: the chair and at least one other. 
 4 present: the chair and 3 others. The quorum is met. 
 Record decision… Recording fixes the 3 staged decisions, and records the 9 EPAs due at this sitting that are not staged as not decided. 
### BOARD FRAME: 2 · Record decision… confirms (C1e). Names the category, what it fixes, what it records as not decided, and who is present. Focus opens on Go back. 
 Wombat
 Acting as Committee member Switch to Assessor 
 Home Committee reviews Decision panels My data rights 
 Thandi Zulu Sign out 
 In progress 3 staged 4 present · quorum met 
 Decision
 Record Satisfactory Progress for Lerato Molefe?
 Fixes 3 staged decisions: PAED-001 at 5, PAED-010 at 4 (expires 2026-10-25), PAED-012 at 5.
 Records 9 EPAs due at this sitting as not decided: PAED-002, PAED-003, PAED-004, PAED-005, PAED-006, PAED-007, PAED-011, PAED-014 and PAED-015.
 Present: Thandi Zulu (chair), David Naidoo, Sarah Botha, John van Rensburg (external).
 Afterwards only a decision that no longer fits the trainee's curriculum can be removed.
 Go back Record decision 
### BOARD FRAME: 5 · Decided · the chair. "Decision recorded." in the Decision card takes the focus. The bar offers Ratify…. The record shows the not-decided sentence. The form is gone; staged lines read Fixed. 
 Wombat
 Acting as Committee member Switch to Assessor 
 Home Committee reviews Decision panels My data rights 
 Thandi Zulu Sign out 
 Decided Satisfactory Progress · 3 decisions to issue Ratify… 
 Decision
 Decision recorded.
 Satisfactory Progress
 On track for completion; three EPAs entrusted, the rest to be decided at her final review.
 About the EPAs not decided: To be decided at her final review.
 Present: Thandi Zulu (chair), David Naidoo, Sarah Botha, John van Rensburg (external)
 Agenda
 Fixed when the committee's decision was recorded: ratifying issues exactly these. 
 EPA Window State Evidence 
 PAED-001 Providing paediatric emergency care to children 2026 Staged Level 5 · issued 2026-10-05 · fixed. 
 3 items 
 PAED-002 catalogue title 2026 S2 Not decided at this sitting To be decided at her final review. 
 0 items 
 The Decision card stands above the agenda once a decision is recorded, so the result sits where the chair pressed Record. Only a decision that no longer fits the curriculum still offers Remove….
### BOARD FRAME: 5 · Ratify… confirms (C1g, T320). Names each STAR it issues and what was not decided; no undo; nothing promised to the registrar. Focus opens on Not yet. 
 Wombat
 Acting as Committee member Switch to Assessor 
 Home Committee reviews Decision panels My data rights 
 Thandi Zulu Sign out 
 Decided Satisfactory Progress · 3 decisions to issue Ratify… 
 Decision
 Ratify and issue 3 STARs to Lerato Molefe?
 PAED-001 at 5, issued 2026-10-05
 PAED-010 at 4, issued 2026-10-05, expires 2026-10-25
 PAED-012 at 5, issued 2026-10-05
 PAED-002 and 8 others were not decided at this sitting. PAED-008, PAED-009 and PAED-013 are not decided.
 Ratifying cannot be undone: nothing on this review can change afterwards.
 Not yet Ratify and issue 3 STARs 
### BOARD FRAME: 6 · Ratified (Step 4.27). "Decision ratified." under the bar takes the focus. The STARs in their lines; the not-decided lines with their sentence (C1f); the optional lines Not decided. The standing below has updated. 
 Wombat
 Acting as Committee member Switch to Assessor 
 Home Committee reviews Decision panels My data rights 
 Thandi Zulu Sign out 
 Ratified Satisfactory Progress · 3 STARs issued 
 Decision ratified.
 Agenda
 Nothing is pending: ratifying the review issued what was staged as STARs, and the agenda names each one. 
 EPA Window State Evidence 
 PAED-001 Providing paediatric emergency care to children 2026 Decided STAR # n . Level 5. 
 3 items 
 PAED-002 catalogue title 2026 S2 Not decided at this sitting To be decided at her final review. 
 0 items 
 PAED-008 catalogue title 2026 Not decided The review was ratified without deciding it. 
 0 items 
 PAED-010 catalogue title 2026 S2 Decided STAR # n . Level 4, expires 2026-10-25. 
 2 items 
 Below: Decision (as recorded) · Entrustment against Annexure A: "2 at or above · 1 below · 12 with no decision, of 15 EPAs"; "2 of 15 EPAs at their exit level by STAR decision" · … · Appeals.
### BOARD FRAME: 6 · Ratified with nothing staged (du Plessis, Step 4.30). No STAR; the twelve due lines carry the one sentence written at Record ("Not at a decision point this year."). 
 Wombat
 Acting as Committee member Switch to Assessor 
 Home Committee reviews Decision panels My data rights 
 Thandi Zulu Sign out 
 Committee reviews › Pieter du Plessis · 2026 S2 
 Pieter du Plessis · 2026 S2
 Annual progression review · Paed Annual Review Panel · Training year 2
 Ratified Satisfactory with Observations · no STAR issued 
 Decision
 Satisfactory with Observations
 Teaching logs are sound; no observed clinical assessment completed yet.
 About the EPAs not decided: Not at a decision point this year.
 Conditions: At least two Mini-CEX before the next sitting.
 Present: Thandi Zulu (chair), David Naidoo
 Agenda
 Nothing was staged when the review was ratified, so ratifying it issued no STAR. 
 Rating trajectory: "No rating by a named assessor to chart in this review's window." The snapshot lists each item in its state: the CBD Requested, the DOPS cancelled, the portfolio review Awaiting review (T138).
### BOARD FRAME: 6 · Outcome Deferred (Ndlovu, Step 4.32). A progression category the College lists, recorded and ratified like any other. It is not a deferral and is unrelated to D53. 
 Wombat
 Acting as Committee member Switch to Assessor 
 Home Committee reviews Decision panels My data rights 
 Thandi Zulu Sign out 
 Committee reviews › Sipho Ndlovu · 2026 S2 
 Sipho Ndlovu · 2026 S2
 Annual progression review · Paed Annual Review Panel · Training year 1
 Ratified Outcome Deferred · no STAR issued 
 Decision
 Outcome Deferred
 One completed Mini-CEX and one reflection in his first semester; to be judged at the next sitting.
 About the EPAs not decided: Too early in the programme to decide.
 Present: Thandi Zulu (chair), Sarah Botha
 Before the committee decides
 PAED-002 · 1 rating · 1 assessor · 1 source. Fewer than three distinct assessors across this EPA's evidence.
 Snapshot: the Mini-CEX Khumalo declined and the one Sarah Botha completed at 3a (PAED-002), the reflective exercise (PAED-001), discussed. Trajectory: PAED-002 alone.


======== R2-1280-more.dc.html ========
 
 R2 1280 readers variants loading 
### BOARD FRAME: 7 · Pre-graduation · thirteen staged (Step 5.4, C7). Bar: 13 staged. PAED-001 keeps "Decided elsewhere" and its sentence; long titles wrap in their cell. Act 5's period is not given: bracketed . 
 Wombat
 Acting as Committee member Switch to Assessor 
 Home Committee reviews Decision panels My data rights 
 Thandi Zulu Sign out 
 In progress 13 staged Present: the chair · one more member needed Go to the decision 
 Agenda
 15 EPAs for period · 13 staged 
 EPA Window State Evidence Action 
 PAED-001 Providing paediatric emergency care to children window Decided elsewhere Another sitting has decided it in this window, so it need not be decided here. A decision staged on it decides it again. 
 n items Stage 
 PAED-002 catalogue title window Staged Level 5 · issued D . Rests on 2 items of the snapshot. 
 n items Remove… 
 PAED-006 catalogue title window Staged Level 5 · issued D . Rests on 2 items of the snapshot. None of the named items is about PAED-006. 
 0 items Remove… 
 PAED-015 Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research window Staged Level 4 · issued D . Rests on 1 item of the snapshot. Only one item is named. 
 n items Remove… 
 Each staged line repeats its hint (one item; none about the EPA) so the panel sees it before Record. With 13 lines the agenda is about 1,500 px; the sticky bar's "Go to the decision" is the way down.
### BOARD FRAME: 7 · Graduate ratified, every EPA at its exit level (Step 5.6). The exit rule's line, then "For information only". PAED-010's STAR at 4 is superseded by today's at 5. 
 Wombat
 Acting as Committee member Switch to Assessor 
 Home Committee reviews Decision panels My data rights 
 Thandi Zulu Sign out 
 Committee reviews › Lerato Molefe · period 
 Lerato Molefe · period 
 Pre-graduation review · Paed Annual Review Panel · Training year 4
 Ratified Graduate (programme complete) · 13 STARs issued 
 Decision ratified.
 Decision
 Graduate (programme complete)
 Present: Thandi Zulu (chair), David Naidoo, Sarah Botha, John van Rensburg (external)
 Entrustment against Annexure A
 Training year 4 · 15 at or above · 0 below · 0 with no decision, of 15 EPAs
 15 of 15 EPAs at their exit level by STAR decision (level 5: 9 of 9 · level 4: 6 of 6). Every EPA is at its exit level.
 For information only. Recording a Graduate decision or completing the programme does not check the exit rule.
 The Ratify dialog lists 13 STARs grouped by level (5: PAED-002 to 007 and 010 · 4: PAED-008, 009, 011, 013, 014, 015).
### BOARD FRAME: 4 · A member mid-sitting (David Naidoo, Step 4.22, C4). Everything the chair sees, none of her controls. "As of 14:02 SAST · Refresh" in the bar, not live (T362 filed, not drawn). Four columns, no Action. 
 Wombat
 Acting as Committee member Switch to Assessor 
 Home Committee reviews Decision panels My data rights 
 David Naidoo Sign out 
 Committee reviews › Lerato Molefe · 2026 S2 
 Lerato Molefe · 2026 S2
 Annual progression review · Paed Annual Review Panel · Training year 4
 In progress 3 staged Only the panel's chair, Thandi Zulu, can stage entrustment decisions and record the committee's decision. As of 14:02 SAST Refresh 
 Agenda
 15 EPAs for 2026 S2 · 3 staged 
 EPA Window State Evidence 
 PAED-001 Providing paediatric emergency care to children 2026 Staged Level 5 · issued 2026-10-05 · no expiry. Rests on 2 items: Mini-CEX (Thandi Zulu), CBD (David Naidoo). 
 3 items 
 PAED-002 catalogue title 2026 S2 Due at this sitting 0 items 
 Decision card, below the agenda: "No decision has been recorded yet." No staging form, no Remove, no decision form, no "Go to the decision" (nothing to do there).
### BOARD FRAME: 4 · The member after Refresh. The review is re-read in place; the reader keeps his scroll position and the focus stays on Refresh. Since 14:02 the chair has recorded: the bar and sentence follow (T260). 
 Wombat
 Acting as Committee member Switch to Assessor 
 Home Committee reviews Decision panels My data rights 
 David Naidoo Sign out 
 Committee reviews › Lerato Molefe · 2026 S2 
 Lerato Molefe · 2026 S2
 Annual progression review · Paed Annual Review Panel · Training year 4
 Decided Satisfactory Progress · 3 decisions to issue Only the panel's chair, Thandi Zulu, can ratify the committee's decision. As of 14:31 SAST Refresh 
 Decision
 Satisfactory Progress
 On track for completion; three EPAs entrusted, the rest to be decided at her final review.
 About the EPAs not decided: To be decided at her final review.
 Present: Thandi Zulu (chair), David Naidoo, Sarah Botha, John van Rensburg (external)
 "As of" is a status region, so the new time is read out; nothing else moves. The chair's own page has no Refresh: her actions re-read it.
### BOARD FRAME: 5 · Decided · the administrator (Nolwazi Mbatha, Step 4.26). No Ratify, no Remove, no Action column; the sentence names who ratifies. Under the h1, the quiet link to flow 06's registrar page, which admits her (Q12). 
 Wombat
 Acting as Institutional admin 
 Home Programme trainees Committee reviews Decision panels … My data rights 
 Nolwazi Mbatha Sign out 
 Committee reviews › Lerato Molefe · 2026 S2 
 Lerato Molefe · 2026 S2
 Annual progression review · Paed Annual Review Panel · Training year 4
 Lerato Molefe's progress 
 Decided Satisfactory Progress Only the panel's chair, Thandi Zulu, can ratify the committee's decision. As of 14:40 SAST Refresh 
 Decision
 Satisfactory Progress
 On track for completion; three EPAs entrusted, the rest to be decided at her final review.
 Present: Thandi Zulu (chair), David Naidoo, Sarah Botha, John van Rensburg (external)
 Agenda
 Fixed when the committee's decision was recorded: ratifying issues exactly these. 
### BOARD FRAME: 7 · Formative check-in, in progress (Step 4.50, T309). Type reads "Formative check-in". No agenda, no decision: Close review… is the chair's one control. Others read "Only the panel's chair, Thandi Zulu, can close this review." 
 Wombat
 Acting as Committee member Switch to Assessor 
 Home Committee reviews Decision panels My data rights 
 Thandi Zulu Sign out 
 Committee reviews › Sipho Ndlovu · 2026 S2 
 Sipho Ndlovu · 2026 S2
 Formative check-in · Paed Annual Review Panel · Training year 1
 In progress Formative (interim check-in, no binding decision) Close review… 
 Evidence snapshot
 Frozen when the review started, 2026-10-05 · 3 items 
 PAED-001 — Providing paediatric emergency care to children
 Reflective exercise · Sipho Ndlovu Discussed 
 PAED-002 — catalogue title 
 Mini-CEX · Khumalo Declined 
 Mini-CEX · Sarah Botha · rated 3a Completed 
 Below: Entrustment against Annexure A · Multi-source feedback by EPA · Rating trajectory (PAED-002). Every formative review's Type, in every list, reads "Formative check-in".
### BOARD FRAME: 7 · Close review… confirms. Says it records no decision and cannot change afterwards. Focus opens on Not yet. 
 Wombat
 Acting as Committee member Switch to Assessor 
 Home Committee reviews Decision panels My data rights 
 Thandi Zulu Sign out 
 Committee reviews › Sipho Ndlovu · 2026 S2 
 Sipho Ndlovu · 2026 S2
 Formative check-in · Paed Annual Review Panel · Training year 1
 In progress Formative (interim check-in, no binding decision) Close review… 
 Close Sipho Ndlovu's formative check-in?
 It records no decision and issues no STAR. Nothing on it can change once it is closed.
 Not yet Close review 
### BOARD FRAME: 7 · Closed. "Formative review closed." under the bar takes the focus; state Closed; nothing is offered. 
 Wombat
 Acting as Committee member Switch to Assessor 
 Home Committee reviews Decision panels My data rights 
 Thandi Zulu Sign out 
 Committee reviews › Sipho Ndlovu · 2026 S2 
 Sipho Ndlovu · 2026 S2
 Formative check-in · Paed Annual Review Panel · Training year 1
 Closed Formative check-in · no decision recorded 
 Formative review closed.
 Evidence snapshot
 Frozen when the review started, 2026-10-05 · 3 items 
### BOARD FRAME: 7 · Entrustment-only review, recorded. The form has no Category; the record reads "Entrustment decisions only". Which registrar sits such a review is not in § 8: bracketed . 
 Wombat
 Acting as Committee member Switch to Assessor 
 Home Committee reviews Decision panels My data rights 
 Thandi Zulu Sign out 
 Committee reviews › Registrar · 2026 S2 
 Registrar · 2026 S2
 Entrustment-only review · Paed Annual Review Panel · Training year n 
 Decided Entrustment decisions only · 2 to issue Ratify… 
 Decision recorded.
 Decision
 Entrustment decisions only
 This review decides entrustment only. Its decision is the entrustment decisions staged above, and it records no progression category: rest of the sentence as stored 
 Present: Thandi Zulu (chair), members 
 Before Record: Rationale, About the EPAs not decided (optional), Conditions and Present; no Category, so no category refusal. "staged below" becomes "staged above" (the agenda precedes the decision now).
### BOARD FRAME: 8 · Loading (T329). The header renders at once with "Committee review" and a skeleton beneath; no action is drawn until the review has loaded. Reduced motion: still blocks. 
 Wombat
 Acting as Committee member Switch to Assessor 
 Home Committee reviews Decision panels My data rights 
 Thandi Zulu Sign out 
 Committee reviews › Committee review 
 Committee review
 Loading the review…
 Tab: "Committee review · Wombat" while loading, then "Lerato Molefe · 2026 S2 · Wombat". Skeleton component; aria-busy on the body.
### BOARD FRAME: 8 · Load error. One sentence and Try again (StatePanel's OnRetry), never an exception's text; the alert takes the focus. 
 Wombat
 Acting as Committee member Switch to Assessor 
 Home Committee reviews Decision panels My data rights 
 Thandi Zulu Sign out 
 Committee reviews › Committee review 
 Committee review
 The review could not be loaded. Nothing on it has changed.
 Try again 
### BOARD FRAME: 8 · Not found. Flow 01's not found in the shell, with this page's sentence; out-of-scope reviews read the same. 
 Wombat
 Acting as Committee member Switch to Assessor 
 Home Committee reviews Decision panels My data rights 
 Thandi Zulu Sign out 
 Committee reviews › Not found 
 Page not found
 The committee review could not be found among the reviews you can view.
 Back to Committee reviews 


======== R2-Activity.dc.html ========
 
 R2 activity page return link 
### BOARD FRAME: 3 · Flow 03's activity page, opened from the review's snapshot (Q6, decided). "← Back to Lerato Molefe · 2026 S2" above the trail, only when opened from a review; Back lands on the snapshot line it came from, focus on its link. 
 Wombat
 Acting as Committee member Switch to Assessor 
 Home Committee reviews Decision panels My data rights 
 Thandi Zulu Sign out 
 Back to Lerato Molefe · 2026 S2 
 Committee reviews › Lerato Molefe · 2026 S2 › Mini-CEX · PAED-001 
 Mini-CEX · PAED-001 — Providing paediatric emergency care to children
 Lerato Molefe's request to Thandi Zulu
 Completed Done. You completed it on date . 
 Rated 5. Credited 1 item to PAED-001.
 About
 Trainee Lerato Molefe Assessor Thandi Zulu Encounter date 
 The form, read-only
 Flow 03's ActivityForm sections, unchanged.
 Address: /activities/{id}?from=review-{ReviewId}&at=snap-{itemId}. Without "from" (any other route in), the link is not drawn. The trail is the owner table's, unchanged.
### BOARD FRAME: 390 · the same page. The return link is the first 44 px row, before flow 01's phone back crumb. 
 Wombat Acting as Committee member Menu 
 Back to Lerato Molefe · 2026 S2 
 Mini-CEX · PAED-001 — Providing paediatric emergency care to children
 Lerato Molefe's request to Thandi Zulu
 Completed Done. You completed it on date . 
 Rated 5. Credited 1 item to PAED-001.
 About
 Trainee Lerato Molefe Assessor Thandi Zulu 


======== R2-P-start.dc.html ========
 
 R2 390 before the sitting and staging 
### BOARD FRAME: 1 · Scheduled. The bar under the title, not sticky (Q9); Start full width, 44 px. 
 Wombat Acting as Committee member Menu 
 ‹ Committee reviews 
 Lerato Molefe · 2026 S2
 Annual progression review · Paed Annual Review Panel · Training year 4
 Scheduled 15 EPAs on the agenda 
 The evidence is frozen when the review starts. Start review… 
 Before the committee decides
 PAED-010 · 1 rating · 1 assessor · 1 source. Fewer than three distinct assessors across this EPA's evidence. 
### BOARD FRAME: 1 · The chair cannot act (C5). The sentence in Start's place; nothing offered. 
 Wombat Acting as Committee member Menu 
 ‹ Committee reviews 
 Lerato Molefe · 2026 S2
 Annual progression review · Paed Annual Review Panel · Training year 4
 Scheduled 
 The panel's chair, Thandi Zulu, cannot take the chair's actions now: reason . A panel administrator must seat a chair who can. 
 Scheduled the same slot, another review 
 This review's trainee does not train at the panel's institution, so the panel cannot act on it. 
### BOARD FRAME: 1 · Start review… confirms. ConfirmDialog at 390: full width at the foot, the safe button nearest the thumb's reach is Not yet, with the focus. 
 Wombat Acting as Committee member Menu 
 Lerato Molefe · 2026 S2
 Scheduled Start review… 
 Start Lerato Molefe's review for 2026 S2?
 Starting freezes the evidence: the 8 items observed from 2026-01-01 to 2026-12-31 become this review's snapshot, each in the state it is in now. Anything finished later is not added.
 Not yet Start review 
### BOARD FRAME: 2 · Started. "Review started." atop the agenda with the focus; stacked lines, Stage 44 px. 
 Wombat Acting as Committee member Menu 
 In progress None staged 
 Present: the chair · one more member needed Go to the decision 
 Agenda
 15 EPAs for 2026 S2 · none staged 
 Review started.
 PAED-001 Providing paediatric emergency care to children 
 Due at this sitting 2026 · 3 items 
 Stage 
 PAED-002 catalogue title 
 Due at this sitting 2026 S2 · 0 items 
 Stage 
### BOARD FRAME: 2 · Stage chosen. The line opens in place (stripe); standing, chart link, its lines first; focus on Authorised level. 
 Wombat Acting as Committee member Menu 
 PAED-001 Providing paediatric emergency care to children 
 Stage a decision on PAED-001
 Year 4 target 5 · no STAR yet · 3 ratings in the review window · MSF: Covered
 Rating chart for PAED-001 
 Evidence it rests on 
 Mini-CEX · Thandi Zulu Rated 5 · Completed Read 
 CBD · David Naidoo Rated rung · Completed Read 
 Only one item is named. The College's rule is that an entrustment decision is never taken from a single form.
 Authorised level Choose a rung… 
 Below: the third line · Show the other 5 snapshot lines · Issued on (2026-10-05) · Expires on · Rationale · Stage pending decision · Cancel
### BOARD FRAME: 3 · Read, in place. The activity takes the form's place; focus on its heading; Back to PAED-001 at the top. 
 Wombat Acting as Committee member Menu 
 PAED-001 Providing paediatric emergency care to children 
 Back to PAED-001 
 Mini-CEX · PAED-001 
 Completed Done. You completed it on date . 
 Rated 5. Credited 1 item to PAED-001.
 Trainee Lerato Molefe Assessor Thandi Zulu Encounter date 
 The form's sections, read-only.
### BOARD FRAME: 3 · Back. The form as typed; the focus on the Read she pressed. 
 Wombat Acting as Committee member Menu 
 Stage a decision on PAED-001
 Evidence it rests on 
 Mini-CEX · Thandi Zulu Rated 5 · Completed Read 
 CBD · David Naidoo Rated rung · Completed Read 
 Authorised level 5 
 Issued on 
 Rationale Leads resuscitation and emergency care without prompting; ready for unsupervised practice. 
### BOARD FRAME: 2 · Staged. The line reads Staged; the result under it takes the focus. 
 Wombat Acting as Committee member Menu 
 PAED-001 Providing paediatric emergency care to children 
 Staged 2026 · 3 items 
 Level 5 · issued 2026-10-05 · no expiry. Rests on 2 items of the snapshot: Mini-CEX (Thandi Zulu), CBD (David Naidoo).
 Remove… 
 Pending entrustment decision staged.
 PAED-002 catalogue title 
 Due at this sitting 2026 S2 · 0 items 
 Stage 
### BOARD FRAME: 2 · Expiry refused (4.18). Refusal atop the line's form, focus there; the field marked; typed kept. 
 Wombat Acting as Committee member Menu 
 PAED-010 catalogue title 
 Stage a decision on PAED-010
 Not staged. An expiry date must be after the issue date.
 Authorised level 4 
 Issued on 
 Expires on (optional) An expiry date must be after the issue date. 
 Rationale Leads the ward round safely; one observation so far, to be confirmed at her final review. 
### BOARD FRAME: 2 · A second PAED-001 refused (4.20). The form at the agenda's foot; refusal atop it, focus there. 
 Wombat Acting as Committee member Menu 
 Stage a decision on an EPA not on this agenda
 Not staged. An entrustment decision on PAED-001 is already staged at this review. Remove it to stage another.
 EPA PAED-001 — Providing paediatric emergency care to children The EPAs on the trainee's curriculum. 
 Authorised level 4 
 Rationale [as typed] 
### BOARD FRAME: 2 · Remove… confirms. Names the EPA and what is discarded; focus on Keep it. 
 Wombat Acting as Committee member Menu 
 PAED-012 Staged 
 Remove the staged decision on PAED-012?
 Its level 5, its rationale and the 2 snapshot items it rests on are discarded. PAED-012 is due at this sitting again.
 Keep it Remove the decision 
### BOARD FRAME: 2 · Removed. The result in the line's place, focus there; Stage returns. 
 Wombat Acting as Committee member Menu 
 PAED-012 catalogue title 
 Due at this sitting 2026 · 2 items 
 Stage 
 Pending entrustment decision removed.
### BOARD FRAME: 2 · The chair's line (5.4, PAED-010). An EPA not on the agenda gains a line once staged. 
 Wombat Acting as Committee member Menu 
 PAED-010 catalogue title 
 Staged Added by the chair 
 Not on this agenda: STAR # n decided it at 4 in this window. This decision, level 5 · issued D · no expiry, decides it again.
 Remove… 
 Pending entrustment decision staged.
### BOARD FRAME: C2 · the chart's end at 322 px. Focus on the chart's heading; "Back to PAED-001's line", 44 px. 
 Wombat Acting as Committee member Menu 
 Rating chart for PAED-001
 Back to PAED-001's line 
 Lerato Molefe · 3 ratings in the review window, 2026-01-01 to 2026-12-31, from Thandi Zulu, David Naidoo and Mohammed Patel. 2 at the minimum, 1 below.
 Flow 05's TrajectoryChart, its 322 px drawing, never scaled
 Its table stacked under it (flow 05).


======== R2-P-decide.dc.html ========
 
 R2 390 record and ratify 
### BOARD FRAME: 2 · Only the chair ticked. Present 44 px rows; the quorum sentence; Record disabled. 
 Wombat Acting as Committee member Menu 
 Decision
 Category Satisfactory Progress 
 Present Thandi Zulu (chair) David Naidoo Sarah Botha John van Rensburg (external) 
 Record decision… 
 Only the chair is ticked. A committee decision needs at least two panel members present: the chair and at least one other. 
 Above Present: Rationale · About the EPAs not decided (optional) · Conditions.
### BOARD FRAME: 2 · No category, refused. Refusal atop the form, focus there; Category marked. 
 Wombat Acting as Committee member Menu 
 Decision
 Not recorded. This review decides the trainee's progression, so its decision records a progression category. Choose one.
 Category Select a category… Choose a progression category. 
 Rationale On track for completion; three EPAs entrusted, the rest to be decided at her final review. 
### BOARD FRAME: 2 · Ready. 4 present, quorum met; the optional field filled. 
 Wombat Acting as Committee member Menu 
 Decision
 About the EPAs not decided (optional) To be decided at her final review. The registrar sees this once the review is ratified. 
 Present Thandi Zulu (chair) David Naidoo Sarah Botha John van Rensburg (external) 
 4 present: the chair and 3 others. The quorum is met. 
 Record decision… 
### BOARD FRAME: 2 · Record decision… confirms. The full list of what it fixes and records; scrolls inside the dialog if long. 
 Wombat Acting as Committee member Menu 
 Record Satisfactory Progress for Lerato Molefe?
 Fixes 3 staged decisions: PAED-001 at 5, PAED-010 at 4 (expires 2026-10-25), PAED-012 at 5.
 Records 9 EPAs due at this sitting as not decided: PAED-002, PAED-003, PAED-004, PAED-005, PAED-006, PAED-007, PAED-011, PAED-014 and PAED-015.
 Present: Thandi Zulu (chair), David Naidoo, Sarah Botha, John van Rensburg (external).
 Afterwards only a decision that no longer fits the trainee's curriculum can be removed.
 Go back Record decision 
### BOARD FRAME: 5 · Decided. "Decision recorded." with the focus; Ratify… in the bar. 
 Wombat Acting as Committee member Menu 
 Decided Satisfactory Progress 
 3 decisions to issue Ratify… 
 Decision
 Decision recorded.
 Satisfactory Progress
 On track for completion; three EPAs entrusted, the rest to be decided at her final review.
 About the EPAs not decided: To be decided at her final review.
 At 390 the bar is not sticky; after Record the page scrolls the Decision card into view, so Ratify… sits above, in the bar, one scroll up.
### BOARD FRAME: 5 · Ratify… confirms. STARs listed; the not-decided line; no undo; focus on Not yet. 
 Wombat Acting as Committee member Menu 
 Ratify and issue 3 STARs to Lerato Molefe?
 PAED-001 at 5, issued 2026-10-05
 PAED-010 at 4, issued 2026-10-05, expires 2026-10-25
 PAED-012 at 5, issued 2026-10-05
 PAED-002 and 8 others were not decided at this sitting.
 Ratifying cannot be undone: nothing on this review can change afterwards.
 Not yet Ratify and issue 3 STARs 
### BOARD FRAME: 6 · Ratified. The result with the focus; STARs and the not-decided lines. 
 Wombat Acting as Committee member Menu 
 Ratified 3 STARs issued 
 Decision ratified.
 PAED-001 Providing paediatric emergency care to children 
 Decided STAR # n · level 5 
 PAED-002 catalogue title 
 Not decided at this sitting To be decided at her final review.
 PAED-008 catalogue title 
 Not decided The review was ratified without deciding it.
### BOARD FRAME: 6 · Nothing staged (du Plessis). No STAR; the sentence written at Record on every due line. 
 Wombat Acting as Committee member Menu 
 Pieter du Plessis · 2026 S2
 Annual progression review · Training year 2
 Ratified no STAR issued 
 Decision
 Satisfactory with Observations
 Conditions: At least two Mini-CEX before the next sitting.
 Present: Thandi Zulu (chair), David Naidoo
 Agenda
 Nothing was staged when the review was ratified, so ratifying it issued no STAR. 
### BOARD FRAME: 6 · Outcome Deferred (Ndlovu). A progression category; not a deferral; unrelated to D53. 
 Wombat Acting as Committee member Menu 
 Sipho Ndlovu · 2026 S2
 Annual progression review · Training year 1
 Ratified Outcome Deferred 
 Decision
 Outcome Deferred
 One completed Mini-CEX and one reflection in his first semester; to be judged at the next sitting.
 About the EPAs not decided: Too early in the programme to decide.
 Present: Thandi Zulu (chair), Sarah Botha
### BOARD FRAME: 7 · Pre-graduation, thirteen staged. 13 staged; the long title wraps; hints repeated in each line. 
 Wombat Acting as Committee member Menu 
 In progress 13 staged 
 Go to the decision 
 PAED-001 Providing paediatric emergency care to children 
 Decided elsewhere Another sitting has decided it in this window, so it need not be decided here. A decision staged on it decides it again.
 Stage 
 PAED-015 Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research 
 Staged Level 4. Rests on 1 item. Only one item is named.
### BOARD FRAME: 7 · Graduate ratified. The exit rule met; for information only. 
 Wombat Acting as Committee member Menu 
 Ratified Graduate (programme complete) 
 13 STARs issued 
 Decision ratified.
 Entrustment against Annexure A
 15 at or above · 0 below · 0 with no decision, of 15 EPAs
 15 of 15 EPAs at their exit level by STAR decision (level 5: 9 of 9 · level 4: 6 of 6). Every EPA is at its exit level.
 For information only. Recording a Graduate decision or completing the programme does not check the exit rule.


======== R2-P-more.dc.html ========
 
 R2 390 readers variants loading 
### BOARD FRAME: 4 · A member mid-sitting. The sentence, "As of 14:02 SAST" and Refresh (44 px). 
 Wombat Acting as Committee member Menu 
 ‹ Committee reviews 
 Lerato Molefe · 2026 S2
 Annual progression review · Paed Annual Review Panel · Training year 4
 In progress 3 staged 
 Only the panel's chair, Thandi Zulu, can stage entrustment decisions and record the committee's decision. As of 14:02 SAST Refresh 
 PAED-001 Providing paediatric emergency care to children 
 Staged Level 5 · issued 2026-10-05. Rests on 2 items.
### BOARD FRAME: 4 · After Refresh. Re-read in place; the focus stays on Refresh; the bar follows. 
 Wombat Acting as Committee member Menu 
 ‹ Committee reviews 
 Lerato Molefe · 2026 S2
 Decided Satisfactory Progress 
 Only the panel's chair, Thandi Zulu, can ratify the committee's decision. As of 14:31 SAST Refresh 
 Decision
 Satisfactory Progress
 Present: Thandi Zulu (chair), David Naidoo, Sarah Botha, John van Rensburg (external)
### BOARD FRAME: 5 · The administrator, decided. No Ratify; the sentence; the quiet link to her progress. 
 Wombat Acting as Institutional admin Menu 
 ‹ Committee reviews 
 Lerato Molefe · 2026 S2
 Annual progression review · Paed Annual Review Panel · Training year 4
 Lerato Molefe's progress 
 Decided Satisfactory Progress 
 Only the panel's chair, Thandi Zulu, can ratify the committee's decision. As of 14:40 SAST Refresh 
### BOARD FRAME: 7 · Formative check-in, in progress. Type "Formative check-in"; Close review… alone. 
 Wombat Acting as Committee member Menu 
 ‹ Committee reviews 
 Sipho Ndlovu · 2026 S2
 Formative check-in · Paed Annual Review Panel · Training year 1
 In progress 
 Formative (interim check-in, no binding decision) Close review… 
 Evidence snapshot
 Frozen when the review started, 2026-10-05 · 3 items 
### BOARD FRAME: 7 · Close review… confirms. No decision, no STAR, no change afterwards. 
 Wombat Acting as Committee member Menu 
 Close Sipho Ndlovu's formative check-in?
 It records no decision and issues no STAR. Nothing on it can change once it is closed.
 Not yet Close review 
### BOARD FRAME: 7 · Closed. The result with the focus; nothing offered. 
 Wombat Acting as Committee member Menu 
 Sipho Ndlovu · 2026 S2
 Formative check-in · Training year 1
 Closed No decision recorded 
 Formative review closed.
### BOARD FRAME: 7 · Entrustment-only, recorded. No category; "Entrustment decisions only". 
 Wombat Acting as Committee member Menu 
 Registrar · 2026 S2
 Entrustment-only review · Paed Annual Review Panel
 Decided Entrustment decisions only 
 Ratify… 
 Decision
 Entrustment decisions only
 This review decides entrustment only. Its decision is the entrustment decisions staged above, and it records no progression category: … 
### BOARD FRAME: 8 · Loading. Header at once, skeleton, no actions. 
 Wombat Acting as Committee member Menu 
 ‹ Committee reviews 
 Committee review
 Loading the review…
### BOARD FRAME: 8 · Load error. A sentence and Try again (44 px); focus on the alert. 
 Wombat Acting as Committee member Menu 
 ‹ Committee reviews 
 Committee review
 The review could not be loaded. Nothing on it has changed.
 Try again 
### BOARD FRAME: 8 · Not found. Flow 01's not found, this page's sentence. 
 Wombat Acting as Committee member Menu 
 ‹ Committee reviews 
 Page not found
 The committee review could not be found among the reviews you can view.
 Back to Committee reviews 
