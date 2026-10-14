######## BOARD R3-0-spec.dc.html — Spec · flow 09 round 3
### Flow 09, round 3: B and P corrected
Round 2's boards, redrawn on this page as R3- with every state, and changed only where an item below asks; rounds 1 and 2 stay on their pages. Captions carry PROPOSED, NEW, decided, T367 and R3 where round 3 changed the frame. Data from the captures in C:\dev\Wombat\design\upload; no NEW token.
Boards. 1 My committee reviews (1280, 768, 390) · 2 Her review page (1280 a–c, 768, 390 a–c) · 3 The committee's page: order and jump (1280, 390), Appeals section (1280 a–c, 390 a–b) · 5 My authorisations (1280 a–b, 768, 390) · 6 The register (1280 a–c, 768, 390 a–b).
Her review page's order, at every width: header (its facts), Decisions, Appeal, Agenda. Her page heads the history “Decisions”; flow 07's keeps “Decision”.
### Round 3, item by item
| What changed
| Where
| F1
| My committee reviews newest first: scheduled date, then review number, both descending (a build change); #7 above #1, #6 above #5
| 1 (all three widths), Steps 4.51, 5.8
| F2
| The expired STAR is a states.md row only (the clock past 2026-11-02 on a scratch database); its frames stay “Illustration”
| 5 a, 390
| F3
| Revoke the STAR always enabled; blank → “Give a reason for the revocation.” under the box, the focus to it, the dialog open
| 6 b, 390 b
| F4
| Download names keep the visible words: cards “Download certificate” named “Download certificate for PAED-001”; the table's “Download” named “Download certificate for PAED-002 (revoked)”; the register keeps its row names
| 5 (all)
| F5
| “Go to the appeal”, outline small, in a reader's bar while Under appeal; PageFocus to the Appeals h2. At 1280 at the bar's right end, saving about 7,800 px; at 390 under the state line, full width, 44px, saving about 18,000 px (estimated; the A.7 replay measures it)
| 3 order 1280 and 390; 3 a, b, c
| F6
| Keep editing and Go back return the focus to the button that opened the dialog; the text is kept
| 2 b “Keep editing”; 3 b, c captions
| F7
| C13 redrawn: a Coordinator holding the External seat who cannot act; a lapsed member with no other rung is refused as “could not be found” (T279), not drawn
| 3 a
| F8
| /committee/my-reviews/{id} only for the review's own trainee, ratified or later, every role; otherwise not found. An Administrator reads /committee/reviews/{id}
| 2 c Not found; Spec
| F9
| The register's button reads Show; Clear filters stays
| 6 (all)
| M1
| Row order and names: “…, Annual progression” / “…, Pre-graduation” on Dr Molefe's pair; the formative row “View the 2026 S2 formative review before Paed Annual Review Panel”
| 1 (all)
| M2
| C13's reader is ‹name›, the Coordinator's menu
| 3 a, 390 a
| M3
| “A lodge refused” reads “Only ratified reviews can be appealed.”; the other sentence is drawn nowhere
| 2 b, 390 b
| M4
| “A revoke refused” reads “Only active entrustment decisions can be revoked.”, the reason kept, the dialog open
| 6 b, 390 b
| M5
| ConfirmDialog CHANGED (below)
| Spec; 6 b
| M6
| The revoke dialog's open and blank-refused states; the per-keystroke flag dropped
| 6 b, 390 b
| M7
| F4's names on every My authorisations board
| 5 (all)
| M8
| The appeal form's quorum line and its refusal after the actions are CHANGED (below)
| Spec; 3 b
| M9
| Issued's link an inline-flex target, 24px at 1280 and 768, 44px at 390
| 6 (all)
| M10
| A.7.7: “18 shown”, 18 stacked rows (16 and 2)
| 6 390 a
| M11
| The certificate checks restored word for word
| Steps 4.35, 4.37, 4.39
| S1
| At 390: her pre-graduation, formative and Dr Ndlovu's annual pages; the register's none, five, Molefe, Revoked, fifteen Active, All, paused, refused, loading. At 1280: fifteen Active. At 768: My authorisations
| 2 390 a; 6 390 a, b; 6 c; 5 768
| S2
| None in force: “In force” with its empty sentence, then No longer in force
| 5 a, 390
| S3
| Expires in N days / 1 day / today on the South African calendar; the expired Since 2026-11-03, Why “Its expiry date, 2026-11-02, passed.”
| 5 a; Spec
| S4
| “Superseded by STAR #18, issued 2026-10-13.”
| 6 c, 768, 390
| S5
| After a lodge refused the page reads the review again: form gone, Under appeal, the open entry, the refusal with the focus
| 2 b, 390 b
| S6
| The mail sentence: “… with a link to My authorisations, unless her account is deactivated.”; nothing claims delivery
| 6 b
| S7
| Issued a link only where IssuingReviewReadable; one plain row drawn
| 6 a
| S8
| Tables hand-written with both stack classes (flows 06 and 08's); DataTable unchanged; FilterBar a pattern
| Spec
| S9
| CSS reuse: .actions-cell, .detail-list, flow 03's note row generalised, no textarea rule, the counter a .field-help, the phone rule scoped, .star-grid equal columns
| every board's helmet; Spec
| S10
| “Show the whole reason” only when the clamped reason overflows, measured after render; line-clamp with its prefixed twin
| Spec; CSS
| S11
| A visually hidden role=status “3 shown” after Show
| 6 a, b, c
| S12
| The DESIGN.md list, below
| Spec
| S13
| Review #2's rationale; Act 5's rationales; “A formative check-in records no decision and issues no STAR.” marked NEW
| 2 a, c; 5 b; 3 c
Nits. The order line and “Decisions” (above); board 3 at 390 drops the caption about 1280; the quorum refusal leads “Resolve appeal was refused:”; the replaced entrustment-only decision's h3 reads “Entrustment decisions only, replaced on appeal”; Issued's link is named “Open review #2”, its visible words kept.
### Components
| Component
| Status
| Classes
| DESIGN.md
| PageHeader, Breadcrumbs, NavMenu, DetailCard, Badge, Alert, ActionResult, FormField, StatePanel, Skeleton, EpaLabel, AccessDenied, NotFound
| used as built
| header-container, breadcrumbs, detail-card, badge-*, alert-*, action-result, form-group, field-help, validation-message, skeleton, paused-mark, system-panel, actions-cell, detail-list
| unchanged
| ConfirmDialog
| CHANGED (M5): in action mode the first focus can go to a field in its body (the revoke reason), and a refusal the caller reports keeps the dialog open (the reason kept). Lodge, Remit and Dismiss use it as built
| dialog-scrim dialog-card, form-container, dialog-body
| § Button system's dialog paragraph adds: “In the action mode a dialog may carry a field: the field, not the safe button, takes the first focus, and a refusal of the action shows inside the dialog, under the field, with what was typed kept; the dialog closes only on Cancel or success (the register's Revoke).”
| The FilterBar pattern (not a component)
| used
| search-container, search-grid, search-field, filter-actions
| § Page-level patterns, List page: unchanged; the register's button reads Show (F9)
| Tables (S8)
| hand-written .clinic-table.clinic-table--stack.clinic-table--stack-wide, as flows 06 and 08 write theirs; DataTable gains no StackWide
| the three: My committee reviews, No longer in force, the register
| § Table system: “A list that must stack at 900px writes its table with both stack classes; DataTable's Stack is the 641px one.”
| ReviewSitting, SittingBar, AgendaTable, .decision-actions
| flow 07's
| review-page, review-section-title, sitting-bar--reader, sitting-bar-actions, agenda-table
| § Detail page: the reader's bar carries “Go to the appeal” while Under appeal (F5)
| p.quorum-line and the refusal on the appeal form
| CHANGED (M8): today the line is the decision form's only, and a resolve refusal shows at the section's head; here the line sits beside Resolve appeal and the refusal after the form's actions
| quorum-line (aria-live polite), action-result
| § The chair's controls: “The appeal's resolve form carries the quorum line beside Resolve appeal, and its refusal after its actions.”
| The trainee's review page NEW
| new page
| my-review-page on review-page; detail-list for its facts (only the badge's alignment is new)
| § Detail page; the title rule; owner table “A trainee's review | Trainee: My committee reviews”
| DecisionHistory NEW
| lifted from DecisionForm's read branch
| decision-history, decision-record, decision-replaced, decision-note
| § Entrustment-only reviews, “The trainee's reviews”
| AppealEntry NEW
| new
| appeal-list, appeal-entry(-head, -meta, -outcome, -label, -reason, -reason--clamped, -more)
| § Badges (below)
| The lodge block NEW
| new
| lodge-block, lodge-next; the counter a second .field-help
| § Form system: a counter is a .field-help named by aria-describedby
| .resolve-form
| changed
| one column, 48rem
| § Form system (T323)
| My authorisations NEW
| changed
| star-grid (equal columns: .details-grid is 1fr 2fr), star-card, star-rationale, stars-section, download-result
| owner table unchanged
| Flow 03's note row
| generalised
| .has-note / .history-note from .history-table to any .clinic-table; stacked, the note joins its row
| § Table system
| .issued-review
| new
| inline-flex, 24px; 44px below 641px
| § Accessibility, target size
| Phone rule
| scoped
| .my-review-page, .appeal-entry-more, .my-authorisations, .entrustment-list; nothing flow 07 already gives
| § app.css section order
### DESIGN.md, the sections this flow changes (S12)
§ Badges: an appeal's state, BadgeFor.Appeal: Open amber (badge-accepted), Dismissed and Remitted green (badge-completed).
§ Button system: M5's dialog sentence (above).
§ Table system: S8's sentence; the note row generalised.
§ Page-level patterns: My committee reviews' State a badge (ReviewSitting.StateBadge), View named by the review (ReviewRowNames, the type added where two rows share a name); a loading or failed review page heads “Committee review”, on the committee's page and on the trainee's.
§ Accessibility: the label in the name (F4: “Download certificate” in “Download certificate for PAED-001”); a filter's count announced by a visually hidden role=status (S11).
§ app.css section order: “Outcomes, STARs and appeals” after “The committee calendar”, holding .decision-history, .decision-record, .decision-replaced, .decision-note, .appeal-*, .appeals-who, .lodge-*, .resolve-form, .stars-section, .star-grid, .star-card, .star-rationale, .download-result, .issued-review, the note row's generalised selectors and the scoped phone rule.
§ Detail page and the title rule; the owner table's new row; § Entrustment-only reviews, “The trainee's reviews”: as round 2.
### Data the build must add
The appeal body read for her Ratified review (AppealBodyOf, today Under appeal only) for “What happens next”; and the review read again after Lodge (S5).
The panel's institution name, for the readers sentence.
LodgedByName and ResolvedByName on CommitteeAppealDto (T308).
An appeal BadgeFor mapping (Open, Dismissed, Remitted).
RevokedByName on both DTOs (the register's and hers), and the certificate's reviser by name (T324).
The South African calendar for Expires in N days / 1 day / today (S3).
IssuingReviewReadable on the register's row (S7).
The trainee query widened to revoked and expired decided, T367.
My committee reviews' order: scheduled date, then review number, descending (F1).
### Flags
The Outcome select at 390 can cut the chosen option's end (“Remitted: the appeal body replaces the decision”) in a closed native select. Radios (flow 08's choice-group) would show both whole; not changed here.
“Show the whole reason” (S10): drawn only when the clamped paragraph overflows (scrollHeight > clientHeight), measured after render in the page's script module; CSS carries both -webkit-line-clamp: 6 and line-clamp: 6.
The 390 scroll distance F5 saves is an estimate from the sections' stacked heights until the replay measures it.
The sitting bar's words on review #2 and #1 (“Satisfactory Progress · 2 STARs issued”, “… 3 STARs issued”) follow the captured “no STAR issued” pattern; confirm.
### Accessibility (WCAG 2.1 AA)
One h1 a page; sections h2; a decision and an appeal entry h3; dialogs their own h2.
Every table captioned; scope on headers; stacked cells keep their labels.
Every row action named for its row, the visible words inside the name (F4, the nit on “Open review #2”).
Focus: the h1 on arrival; the field on a blank reason; the safe button in Lodge, Remit, Dismiss; the reason box in Revoke; results and refusals in an ActionResult; Keep editing and Go back to their opener (F6); Go to the appeal to the Appeals h2 (F5).
Targets: 24px at 1280 and 768 (Issued's link included, M9); 44px at 390.
Contrast: as round 2, every pair from tokens.json (text 12.63:1, muted 5.09:1, link 6.70:1, action blue 4.86:1, input edge 3.80:1, badge words ≥ 11.23:1, badge edges ≥ 3.42:1).

######## BOARD R3-0-steps.dc.html — Steps · the 17 runbook steps (round 3)
### The 17 steps once B is built (round 3)
Every Expect clause today's runbook carries stays; the new ones stand beside it.
| Step
| Role
| Route
| Expect, once built
| 4.35
| Institutional admin, Nolwazi Mbatha
| / → /admin/entrustment-decisions
| From her Home's quick links; the trail Home › Entrustment decisions (no menu item for her). Five rows, all Active, issued 2026-10-13, newest issued first, then STAR number descending (one day's ties): Dlamini PAED-002 3b, PAED-001 4; Molefe PAED-012 5, PAED-010 4 expiring 2026-11-02, PAED-001 5. Expires reads “—” where none was set. Each Issued carries “review #n”. The certificate (Dr Molefe's PAED-010) names Kgosi Kgari Teaching Hospital, “Statement of Awarded Responsibility”, Lerato Molefe, PAED-010 and level 4; Issued on 2026-10-13 and Expires on 2026-11-02, Status Active; the rationale and the evidence summary of its two snapshot lines; Panel Paed Annual Review Panel, the review number, Chair Thandi Zulu. The filter: “Molefe” and Show leave her three rows, “· 3 shown”, the status line “3 shown”, and Clear filters.
| 4.36
| Speciality admin, Refilwe Mokoena
| /admin/entrustment-decisions
| Do: Entrustment decisions from the menu; Revoke on Dlamini's PAED-002 row; press Revoke the STAR with the reason blank once; type the reason; Revoke the STAR. Expect: the menu marks the page, no trail; the same five rows. The dialog “Revoke Anele Dlamini's PAED-002 STAR?”, the focus in Revocation reason, Revoke the STAR enabled; blank, “Give a reason for the revocation.” under the box, the dialog open, the focus in the box; the dialog's mail sentence (“… unless her account is deactivated”). After: “Anele Dlamini's PAED-002 STAR is revoked.” with the focus; the row Revoked, Download only, its line “Revoked 2026-10-13 at HH:mm SAST by Refilwe Mokoena: Staged on evidence about PAED-001, not PAED-002; to be decided again.”, HH:mm equal to RevokedOn + 2 h (as 4.50 asserts). The application log holds the mail to Dr Dlamini (T367).
| 4.37
| Sub-speciality admin, Kabelo Sithole
| /admin/entrustment-decisions → /committee/decisions-due
| The menu marks the page, no trail. Revoked leaves Dlamini's PAED-002 with its line; Active leaves four. The revoked certificate reads Status Revoked, “REVOKED”, the date, who revoked it (Refilwe Mokoena, by name, T142, T324) and the reason. Decisions due: Dlamini's PAED-002 reads “Revoked: re-decide”, its detail “STAR #5, issued for 2026 S2, was revoked. Schedule a review to decide it again.”, its one action Schedule; By EPA PAED-002 reads Decided 0, Not decided 4, To schedule 1; the count “71 of 75 decisions due in 2026 S2 shown.”
| 4.39
| Trainee, Lerato Molefe
| / → /portfolio/authorisations
| Home's My authorisations card reads “2 at or above · 1 below · 12 with no decision, of 15 EPAs” and lists “PAED-010 — Leading and operating within a clinical team”, a link to its EPA's page, with “4, below training year 4's level of 5 · expires 2026-11-02” under it; its footer Open My authorisations (T355). The page: trail Home › My progress › My authorisations; In force: three cards issued 2026-10-13 (ISO) with the committee's rationale; PAED-001 and 012 at 5, “No expiry”; PAED-010 at 4, “2026-11-02”, “Expires in 20 days”. Each Download certificate named “Download certificate for PAED-001”, “… PAED-010”, “… PAED-012” (F4). No “No longer in force”. The PAED-001 certificate: Status Active, evidence Mini-CEX #12 and CBD #13.
| 4.41
| Trainee, Lerato Molefe
| /committee/my-reviews → /committee/my-reviews/1 → /committee/reviews/1
| The list: one row, Period “2026 S2 · 2026-01-01 to 2026-12-31”, Type Annual progression, State Ratified (a badge), Decision Satisfactory Progress, View named by the review. Her page: h1 and tab “2026 S2 annual progression review”, trail Home › My committee reviews › …; Sits for 2026 S2, the window, Type, State. Decisions: Satisfactory Progress, its rationale, “About the EPAs not decided: To be decided at her final review.”, “Present: Thandi Zulu (chair), David Naidoo, Sarah Botha, John van Rensburg (external)”. The agenda: Decided “STAR #1.”, “#2.”, “#3.” on PAED-001, 010, 012; the other twelve “Not decided at this sitting” with the committee's sentence “To be decided at her final review.”: no reason label, no Deferred (T361, D53, E1). /committee/reviews/1: access denied, unchanged.
| 4.42
| Trainee, Anele Dlamini
| / → /portfolio/authorisations → /committee/my-reviews/2
| In force: PAED-001 at 4. No longer in force: PAED-002, 3b, Revoked, since 2026-10-13, “Revoked by Refilwe Mokoena: Staged on evidence about PAED-001, not PAED-002; to be decided again.”, Download named “Download certificate for PAED-002 (revoked)”. Her review: Ratified, Satisfactory Progress, “Steady progress in year 3; two EPAs entrusted at this sitting.”; the agenda still Decided “STAR #4.” and “STAR #5.”.
| 4.43
| Trainee, Nomsa Mahlangu
| /committee/my-reviews → /committee/my-reviews/4 → /committee/my-reviews
| Do: View; press Lodge appeal with the reason blank; type the reason; Lodge appeal; the dialog's Lodge appeal; back to the list. Expect: before, the decision “Inadequate Progress — Additional Training”, its rationale, “Present: Thandi Zulu (chair), Sarah Botha, John van Rensburg (external)”; fifteen lines “Not decided at this sitting” with “Not enough observed evidence yet this year.”; “What happens next” names the appeal body and every reader. Blank: “Give your reason for appealing.” under the field. The dialog “Lodge your appeal?”, focus Keep editing; Keep editing returns the focus to Lodge appeal with the reason kept (F6). After: “Appeal lodged.” with the focus; State Under appeal; her entry Open, “Lodged 2026-10-13”, “Heard by Thandi Zulu (chair) and John van Rensburg (external).”; no form; the agenda in view; the list's row Under appeal.
| 4.44
| Committee member, David Naidoo
| /committee/reviews → /committee/reviews/4
| Do: open her review; press Go to the appeal. Expect: the list shows her review Under appeal. The bar carries Go to the appeal (F5); pressed, the focus moves to the Appeals h2. The entry: “Appeal by Nomsa Mahlangu”, Open, Lodged 2026-10-13, “Heard by Thandi Zulu (chair) and John van Rensburg (external).”, the reason; no form; “Only the appeal body can resolve the appeal: the panel's chair, Thandi Zulu, and its external member, John van Rensburg.” (T213, T237)
| 4.45
| Committee member (external), John van Rensburg
| /committee/reviews → /committee/reviews/4
| The list: the panel's five reviews. The form inline; Outcome opens on “Select an outcome…”, then “Dismissed: the decision stands” and “Remitted: the appeal body replaces the decision” (T307, D51: no Upheld); its help text says Dismissed leaves the decision in force, Remitted replaces it, and either closes the review. Resolve appeal with none: “Choose an outcome.” under the select, nothing resolved. Remitted: Replacement category (“Select a category…”), Replacement rationale, Replacement conditions (optional), Present with Thandi Zulu (chair) and himself ticked and locked, the quorum line “2 present: the chair and 1 other. The quorum is met.”; the Outcome readable in full at 1280 (T323). Nothing changes until he submits. (At 4.46 the chair ticks only herself.)
| 4.46
| Committee member (chair), Thandi Zulu
| /committee/reviews/4
| Do: Remitted, Satisfactory with Observations, the rationale; read the quorum line; Resolve appeal. Expect: before pressing, the quorum line reads “Only the chair is ticked. A committee decision needs at least two panel members present: the chair and at least one other.”; Resolve appeal is refused with “Resolve appeal was refused: A committee decision needs at least two panel members present: the chair and at least one other.” after the form's actions, in the Appeals section, with the focus (T361 round 3 item 15); what she typed is kept; the review stays Under appeal.
| 4.47
| Committee member (chair), Thandi Zulu
| /committee/reviews/4
| Sarah Botha ticked, the conditions typed, Resolve appeal → “Remit Nomsa Mahlangu's appeal?” (focus Go back) → Remit the appeal. “Appeal resolved.” heads the Appeals section with the focus; the state Closed (T250). The replacement heads the decision history: “Satisfactory with Observations”, “About the EPAs not decided: Not enough observed evidence yet this year.” (carried, T361, E5), “Conditions: Two observed Mini-CEX and one DOPS before the next review.” (T307), “Present: Thandi Zulu (chair), Sarah Botha”; the first below it, “Replaced on appeal by the decision above.”. The entry Remitted, “Resolved 2026-10-13 by Thandi Zulu”. No STAR changed; the Outcome-state SQL check kept.
| 4.48
| Trainee, Nomsa Mahlangu
| /committee/my-reviews → /committee/my-reviews/4
| The row: Closed, Satisfactory with Observations. Her page: both decisions, newest first, the replacement with “Present: Thandi Zulu (chair), Sarah Botha”, the first marked replaced with its rationale and who sat; her entry Remitted, Resolved 2026-10-13 by Thandi Zulu; “This review is closed and cannot be appealed again.”; the fifteen lines “Not decided at this sitting”, “Not enough observed evidence yet this year.” (T361, E5); no form.
| 4.51
| Trainee, Sipho Ndlovu
| /committee/my-reviews → /committee/my-reviews/6 → /committee/my-reviews/5
| Two rows for 2026 S2, #6 first (F1): Formative check-in, Closed, No binding decision (T309 item 2; T364, C8); Annual progression, Ratified, Outcome Deferred. View each: “2026 S2 formative check-in” (Type “Formative check-in”, no decision, no lodge) and “2026 S2 annual progression review”.
| 5.7
| Institutional admin, Nolwazi Mbatha
| / → /admin/entrustment-decisions
| Molefe, Active: fifteen rows, “· 15 shown”, one per EPA PAED-001 to PAED-015, each at its exit level, issued 2026-10-13, no expiry, Download and Revoke; newest issued first, review #7's thirteen first. With All: sixteen; Act 4's PAED-010 reads Superseded with Download only and the line “Superseded by STAR #18, issued 2026-10-13.”. She revokes nothing.
| 5.8
| Trainee, Lerato Molefe
| /committee/my-reviews → /committee/my-reviews/7 → / → /portfolio/authorisations
| The list: #7 above #1 (F1): Pre-graduation, Ratified, Graduate (programme complete). Her page “2026 S2 pre-graduation review”: the type, the decision and its rationale, “Present: Thandi Zulu (chair), David Naidoo, Sarah Botha, John van Rensburg (external)”; thirteen lines Decided with STAR #6 to #18; the lodge block offered and unused. My authorisations: fifteen cards, each its level, issued 2026-10-13, “No expiry”; the PAED-001 certificate downloads as star-certificate-PAED-001-<id>-<8 hex digits>.pdf.
| 6.22
| Institutional admin, Nolwazi Mbatha
| /admin/epas → /admin/curricula → /admin/curricula/{Id}/items → /admin/entrustment-decisions
| The EPA list: PAED-012 reads Inactive; no national EPA offers her Edit (only its College changes one; T211, UpdateEpa). 11.1's items: PAED-012 marked “(inactive: not in force)”, every row “Set by the College”. Her fifteen STARs Active beside Act 4's PAED-010 Superseded (the filter starts on All); PAED-012's EPA “… healthcare teams (no longer in use)” (T255).
| A.7.7
| Speciality admin, Refilwe Mokoena, 390px
| / → /committee/panels → /committee/decisions-due → /admin/entrustment-decisions
| Home stacks Waiting for assessors (“3 waiting, 1 overdue”), Registrars (“2 registrars”) and Targets by EPA, KGK-001 first (T358). Decision panels' list and Who decides each EPA stack (T364); Decisions due as at A.7.5. Entrustment decisions in the menu, lit, no trail; “· 18 shown” (M10): eighteen stacked rows, each with Trainee, EPA, Level, Issued and its review (44px), Expires, Status and its actions; the revoked line wraps; nothing scrolls sideways.
### Gaps these steps close
For the runbook to clean: F-4.36a, F-4.37a, F-4.43a, F-4.45c, F-4.48a, and T260's entrustment half.
### C26: steps, presses and fields typed
| Journey
| Presses and fields
| 1280
| 390
| Change
| Dr Molefe reads her review (4.41)
| My committee reviews, View
| 2
| 3
| unchanged
| Dr Mahlangu lodges (4.43)
| menu, View, Lodge appeal blank, reason typed, Lodge appeal, the dialog's Lodge appeal
| 6
| 7
| +1: the blank press
| Dr Mahlangu reads the outcome (4.48)
| menu, View
| 2
| 3
| unchanged
| Dr Naidoo reads the appeal (4.44)
| Committee reviews, the review, Go to the appeal (no scroll)
| 3
| 4
| +1: the jump, saving about 7,800 / 18,000 px
| Dr van Rensburg: no outcome, then Remitted (4.45)
| Committee reviews, the review, Resolve appeal, Outcome Remitted
| 4
| 5
| unchanged
| Dr Zulu: refused, then remits (4.46–4.47)
| Committee reviews, review, Remitted, category, rationale, Resolve appeal (refused), tick Sarah Botha, conditions, Resolve appeal, Remit the appeal
| 10
| 11
| unchanged (she may also jump: +1)
| Dr Mokoena revokes (4.36)
| Entrustment decisions, Revoke, Revoke the STAR blank, reason typed, Revoke the STAR
| 5
| 6
| +1: the blank press
| Dr Dlamini's My authorisations (4.42)
| Home's card, Open My authorisations
| 1
| 1
| unchanged

######## BOARD R3-1-my-reviews-1280.dc.html — My committee reviews · 1280
## My committee reviews · 1280px
Route /committee/my-reviews · Trainee (and Administrator, as today) · the menu's My committee reviews, lit, no trail.
Order Newest first: scheduled date, then review number, both descending (F1, a build change); the caption's words stay.
Components PageHeader, a hand-written .clinic-table with both stack classes (flows 06 and 08's; DataTable unchanged, S8), Badge (ReviewSitting.StateBadge: Ratified and Closed green, Under appeal amber), StatePanel, Skeleton, Alert with Try again. NEW the State badge; View's target.
DESIGN.md § Page-level patterns, List page: State a badge, View named by the review (S12); § Table system (S8). Round 3 F1, M1, S8.
None yetSipho Ndlovu before his first review is ratified · StatePanel's empty card, today's wordsFocus: the h1, as the page loads
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
[title: Sipho Ndlovu] Sipho Ndlovu
Sign out
## My committee reviews
Ratified decisions, appeal status, and final outcomes.
No decisions yet
Ratified committee outcomes will appear here.
One review (4.41)Lerato Molefe before Act 5 · View links to her review's page, named “View the 2026 S2 review before Paed Annual Review Panel”; State a badge in words (NEW)Focus: the h1NEW
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
## My committee reviews
Ratified decisions, appeal status, and final outcomes.
Your committee reviews, newest first
| Panel
| Period
| Type
| State
| Decision
| Actions
| Paed Annual Review Panel
| 2026 S22026-01-01 to 2026-12-31
| Annual progression
| Ratified
| Satisfactory Progress
| [aria-label: View the 2026 S2 review before Paed Annual Review Panel] View
Two reviews in one period, #6 first (4.51, F1, M1)Sipho Ndlovu · newest first by scheduled date, then review number, both descending: the formative check-in (#6) above the annual review (#5); the names differ already, so ReviewRowNames adds no typeR3
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
[title: Sipho Ndlovu] Sipho Ndlovu
Sign out
## My committee reviews
Ratified decisions, appeal status, and final outcomes.
Your committee reviews, newest first
| Panel
| Period
| Type
| State
| Decision
| Actions
| Paed Annual Review Panel
| 2026 S22026-01-01 to 2026-12-31
| Formative check-in
| Closed
| No binding decision
| [aria-label: View the 2026 S2 formative review before Paed Annual Review Panel] View
| Paed Annual Review Panel
| 2026 S22026-01-01 to 2026-12-31
| Annual progression
| Ratified
| Outcome Deferred
| [aria-label: View the 2026 S2 review before Paed Annual Review Panel] View
Her two reviews, #7 first (5.8, F1, M1)Lerato Molefe · #7 pre-graduation (window 2023-01-15 to 2026-12-31) above #1; both rows share a name, so each adds its type: “…, Pre-graduation” and “…, Annual progression”R3
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
## My committee reviews
Ratified decisions, appeal status, and final outcomes.
Your committee reviews, newest first
| Panel
| Period
| Type
| State
| Decision
| Actions
| Paed Annual Review Panel
| 2026 S22023-01-15 to 2026-12-31
| Pre-graduation
| Ratified
| Graduate (programme complete)
| [aria-label: View the 2026 S2 review before Paed Annual Review Panel, Pre-graduation] View
| Paed Annual Review Panel
| 2026 S22026-01-01 to 2026-12-31
| Annual progression
| Ratified
| Satisfactory Progress
| [aria-label: View the 2026 S2 review before Paed Annual Review Panel, Annual progression] View
Under appeal, right after lodging (4.43, T308)Nomsa Mahlangu, back from her review's trail · the row reads Under appeal at once: the list reads the reviews again on every visit
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
[title: Nomsa Mahlangu] Nomsa Mahlangu
Sign out
## My committee reviews
Ratified decisions, appeal status, and final outcomes.
Your committee reviews, newest first
| Panel
| Period
| Type
| State
| Decision
| Actions
| Paed Annual Review Panel
| 2026 S22026-01-01 to 2026-12-31
| Annual progression
| Under appeal
| Inadequate Progress — Additional Training
| [aria-label: View the 2026 S2 review before Paed Annual Review Panel] View
Closed after a remit (4.48)Nomsa Mahlangu · State Closed, Decision the replacement's category
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
[title: Nomsa Mahlangu] Nomsa Mahlangu
Sign out
## My committee reviews
Ratified decisions, appeal status, and final outcomes.
Your committee reviews, newest first
| Panel
| Period
| Type
| State
| Decision
| Actions
| Paed Annual Review Panel
| 2026 S22026-01-01 to 2026-12-31
| Annual progression
| Closed
| Satisfactory with Observations
| [aria-label: View the 2026 S2 review before Paed Annual Review Panel] View
LoadingThe header at once, a status line read to a screen reader, three row skeletons; no View until the read is in
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
## My committee reviews
Ratified decisions, appeal status, and final outcomes.
Loading your committee reviews.
Load error (T329)Fixed words with Try again, never the exception's text (today RefusalText.Of(exception))Focus: the alert
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
## My committee reviews
Ratified decisions, appeal status, and final outcomes.
Could not load your committee reviews. Nothing has changed. Try again, or come back in a few minutes.
Try again

######## BOARD R3-1-my-reviews-768.dc.html — My committee reviews · 768
## My committee reviews · 768px
Route /committee/my-reviews · Trainee (and Administrator, as today) · the menu's My committee reviews, lit, no trail.
Order Newest first: scheduled date, then review number, both descending (F1, a build change); the caption's words stay.
Components PageHeader, a hand-written .clinic-table with both stack classes (flows 06 and 08's; DataTable unchanged, S8), Badge (ReviewSitting.StateBadge: Ratified and Closed green, Under appeal amber), StatePanel, Skeleton, Alert with Try again. NEW the State badge; View's target.
DESIGN.md § Page-level patterns, List page: State a badge, View named by the review (S12); § Table system (S8). Round 3 F1, M1, S8.
None yetSipho Ndlovu before his first review is ratified · StatePanel's empty card, today's wordsFocus: the h1, as the page loads
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
[title: Sipho Ndlovu] Sipho Ndlovu
Sign out
## My committee reviews
Ratified decisions, appeal status, and final outcomes.
No decisions yet
Ratified committee outcomes will appear here.
One review (4.41)Lerato Molefe before Act 5 · View links to her review's page, named “View the 2026 S2 review before Paed Annual Review Panel”; State a badge in words (NEW)Focus: the h1NEW
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
## My committee reviews
Ratified decisions, appeal status, and final outcomes.
Your committee reviews, newest first
| Panel
| Period
| Type
| State
| Decision
| Actions
| Paed Annual Review Panel
| 2026 S22026-01-01 to 2026-12-31
| Annual progression
| Ratified
| Satisfactory Progress
| [aria-label: View the 2026 S2 review before Paed Annual Review Panel] View
Two reviews in one period, #6 first (4.51, F1, M1)Sipho Ndlovu · newest first by scheduled date, then review number, both descending: the formative check-in (#6) above the annual review (#5); the names differ already, so ReviewRowNames adds no typeR3
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
[title: Sipho Ndlovu] Sipho Ndlovu
Sign out
## My committee reviews
Ratified decisions, appeal status, and final outcomes.
Your committee reviews, newest first
| Panel
| Period
| Type
| State
| Decision
| Actions
| Paed Annual Review Panel
| 2026 S22026-01-01 to 2026-12-31
| Formative check-in
| Closed
| No binding decision
| [aria-label: View the 2026 S2 formative review before Paed Annual Review Panel] View
| Paed Annual Review Panel
| 2026 S22026-01-01 to 2026-12-31
| Annual progression
| Ratified
| Outcome Deferred
| [aria-label: View the 2026 S2 review before Paed Annual Review Panel] View
Her two reviews, #7 first (5.8, F1, M1)Lerato Molefe · #7 pre-graduation (window 2023-01-15 to 2026-12-31) above #1; both rows share a name, so each adds its type: “…, Pre-graduation” and “…, Annual progression”R3
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
## My committee reviews
Ratified decisions, appeal status, and final outcomes.
Your committee reviews, newest first
| Panel
| Period
| Type
| State
| Decision
| Actions
| Paed Annual Review Panel
| 2026 S22023-01-15 to 2026-12-31
| Pre-graduation
| Ratified
| Graduate (programme complete)
| [aria-label: View the 2026 S2 review before Paed Annual Review Panel, Pre-graduation] View
| Paed Annual Review Panel
| 2026 S22026-01-01 to 2026-12-31
| Annual progression
| Ratified
| Satisfactory Progress
| [aria-label: View the 2026 S2 review before Paed Annual Review Panel, Annual progression] View
Under appeal, right after lodging (4.43, T308)Nomsa Mahlangu, back from her review's trail · the row reads Under appeal at once: the list reads the reviews again on every visit
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
[title: Nomsa Mahlangu] Nomsa Mahlangu
Sign out
## My committee reviews
Ratified decisions, appeal status, and final outcomes.
Your committee reviews, newest first
| Panel
| Period
| Type
| State
| Decision
| Actions
| Paed Annual Review Panel
| 2026 S22026-01-01 to 2026-12-31
| Annual progression
| Under appeal
| Inadequate Progress — Additional Training
| [aria-label: View the 2026 S2 review before Paed Annual Review Panel] View
Closed after a remit (4.48)Nomsa Mahlangu · State Closed, Decision the replacement's category
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
[title: Nomsa Mahlangu] Nomsa Mahlangu
Sign out
## My committee reviews
Ratified decisions, appeal status, and final outcomes.
Your committee reviews, newest first
| Panel
| Period
| Type
| State
| Decision
| Actions
| Paed Annual Review Panel
| 2026 S22026-01-01 to 2026-12-31
| Annual progression
| Closed
| Satisfactory with Observations
| [aria-label: View the 2026 S2 review before Paed Annual Review Panel] View
LoadingThe header at once, a status line read to a screen reader, three row skeletons; no View until the read is in
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
## My committee reviews
Ratified decisions, appeal status, and final outcomes.
Loading your committee reviews.
Load error (T329)Fixed words with Try again, never the exception's text (today RefusalText.Of(exception))Focus: the alert
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
## My committee reviews
Ratified decisions, appeal status, and final outcomes.
Could not load your committee reviews. Nothing has changed. Try again, or come back in a few minutes.
Try again

######## BOARD R3-1-my-reviews-390.dc.html — My committee reviews · 390
## My committee reviews · 390px
Route /committee/my-reviews · Trainee (and Administrator, as today) · the menu's My committee reviews, lit, no trail.
Order Newest first: scheduled date, then review number, both descending (F1, a build change); the caption's words stay.
Components PageHeader, a hand-written .clinic-table with both stack classes (flows 06 and 08's; DataTable unchanged, S8), Badge (ReviewSitting.StateBadge: Ratified and Closed green, Under appeal amber), StatePanel, Skeleton, Alert with Try again. NEW the State badge; View's target.
DESIGN.md § Page-level patterns, List page: State a badge, View named by the review (S12); § Table system (S8). Round 3 F1, M1, S8.
None yetSipho Ndlovu before his first review is ratified · StatePanel's empty card, today's wordsFocus: the h1, as the page loads
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
[title: Sipho Ndlovu] Sipho Ndlovu
Sign out
## My committee reviews
Ratified decisions, appeal status, and final outcomes.
No decisions yet
Ratified committee outcomes will appear here.
One review (4.41)Lerato Molefe before Act 5 · View links to her review's page, named “View the 2026 S2 review before Paed Annual Review Panel”; State a badge in words (NEW)Focus: the h1NEW
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
## My committee reviews
Ratified decisions, appeal status, and final outcomes.
Your committee reviews, newest first
| Panel
| Period
| Type
| State
| Decision
| Actions
| Paed Annual Review Panel
| 2026 S22026-01-01 to 2026-12-31
| Annual progression
| Ratified
| Satisfactory Progress
| [aria-label: View the 2026 S2 review before Paed Annual Review Panel] View
Two reviews in one period, #6 first (4.51, F1, M1)Sipho Ndlovu · newest first by scheduled date, then review number, both descending: the formative check-in (#6) above the annual review (#5); the names differ already, so ReviewRowNames adds no typeR3
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
[title: Sipho Ndlovu] Sipho Ndlovu
Sign out
## My committee reviews
Ratified decisions, appeal status, and final outcomes.
Your committee reviews, newest first
| Panel
| Period
| Type
| State
| Decision
| Actions
| Paed Annual Review Panel
| 2026 S22026-01-01 to 2026-12-31
| Formative check-in
| Closed
| No binding decision
| [aria-label: View the 2026 S2 formative review before Paed Annual Review Panel] View
| Paed Annual Review Panel
| 2026 S22026-01-01 to 2026-12-31
| Annual progression
| Ratified
| Outcome Deferred
| [aria-label: View the 2026 S2 review before Paed Annual Review Panel] View
Her two reviews, #7 first (5.8, F1, M1)Lerato Molefe · #7 pre-graduation (window 2023-01-15 to 2026-12-31) above #1; both rows share a name, so each adds its type: “…, Pre-graduation” and “…, Annual progression”R3
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
## My committee reviews
Ratified decisions, appeal status, and final outcomes.
Your committee reviews, newest first
| Panel
| Period
| Type
| State
| Decision
| Actions
| Paed Annual Review Panel
| 2026 S22023-01-15 to 2026-12-31
| Pre-graduation
| Ratified
| Graduate (programme complete)
| [aria-label: View the 2026 S2 review before Paed Annual Review Panel, Pre-graduation] View
| Paed Annual Review Panel
| 2026 S22026-01-01 to 2026-12-31
| Annual progression
| Ratified
| Satisfactory Progress
| [aria-label: View the 2026 S2 review before Paed Annual Review Panel, Annual progression] View
Under appeal, right after lodging (4.43, T308)Nomsa Mahlangu, back from her review's trail · the row reads Under appeal at once: the list reads the reviews again on every visit
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
[title: Nomsa Mahlangu] Nomsa Mahlangu
Sign out
## My committee reviews
Ratified decisions, appeal status, and final outcomes.
Your committee reviews, newest first
| Panel
| Period
| Type
| State
| Decision
| Actions
| Paed Annual Review Panel
| 2026 S22026-01-01 to 2026-12-31
| Annual progression
| Under appeal
| Inadequate Progress — Additional Training
| [aria-label: View the 2026 S2 review before Paed Annual Review Panel] View
Closed after a remit (4.48)Nomsa Mahlangu · State Closed, Decision the replacement's category
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
[title: Nomsa Mahlangu] Nomsa Mahlangu
Sign out
## My committee reviews
Ratified decisions, appeal status, and final outcomes.
Your committee reviews, newest first
| Panel
| Period
| Type
| State
| Decision
| Actions
| Paed Annual Review Panel
| 2026 S22026-01-01 to 2026-12-31
| Annual progression
| Closed
| Satisfactory with Observations
| [aria-label: View the 2026 S2 review before Paed Annual Review Panel] View
LoadingThe header at once, a status line read to a screen reader, three row skeletons; no View until the read is in
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
## My committee reviews
Ratified decisions, appeal status, and final outcomes.
Loading your committee reviews.
Load error (T329)Fixed words with Try again, never the exception's text (today RefusalText.Of(exception))Focus: the alert
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
## My committee reviews
Ratified decisions, appeal status, and final outcomes.
Could not load your committee reviews. Nothing has changed. Try again, or come back in a few minutes.
Try again

######## BOARD R3-2-review-1280-a.dc.html — Her review · 1280 · a
## Her review page · 1280px · a: reading
Route /committee/my-reviews/{id} (decided 2026-10-14), owned by My committee reviews: its nav item lit, its trail Home › My committee reviews › the h1; tab “<h1> · Wombat”. Only the review's own trainee, once it is ratified or later; anything else, every role, is flow 01's not found (F8).
Order One column at every width: header (its facts), Decisions, Appeal, Agenda.
Components PageHeader, Breadcrumbs, DetailCard, .detail-list (the facts), AgendaTable (read-only), Badge, Alert, ActionResult, ConfirmDialog (action mode, as built), StatePanel, Skeleton, FormField. NEW the page (.my-review-page on flow 07's .review-page); DecisionHistory, lifted; AppealEntry; the lodge block.
DESIGN.md § Detail page and the title rule; the owner table's row “A trainee's review | Trainee: My committee reviews”; § Entrustment-only reviews, “The trainee's reviews”. Round 3 F6, F8, M3, S1, S5, S9, S13, nits.
Ratified, with the lodge block (4.41)Lerato Molefe · /committee/my-reviews/1 · h1 and tab “2026 S2 annual progression review · Wombat”; trail Home › My committee reviews › 2026 S2 annual progression review; Decided lines read “STAR #n.” with no download (Q6)Focus: the h1 (FocusOnNavigate)PROPOSEDNEW
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
My committee reviews
2026 S2 annual progression review
## 2026 S2 annual progression review
Paed Annual Review Panel
[aria-label: About this review]
Panel
Paed Annual Review Panel
Sits for
2026 S2
Evidence window
2026-01-01 to 2026-12-31
Type
Annual progression review
State
Ratified
### Decisions
#### Satisfactory Progress
On track for completion; three EPAs entrusted, the rest to be decided at her final review.
About the EPAs not decided: To be decided at her final review.
Present: Thandi Zulu (chair), David Naidoo, Sarah Botha, John van Rensburg (external)
### Appeal
#### What happens next
Your appeal is heard by the panel's appeal body: today, Thandi Zulu (chair) and John van Rensburg (external).
They dismiss it, and the decision stands, or remit it, and take a new decision in its place. Either outcome is final: nothing reopens the review, and it cannot be appealed again.
Who reads your reason: everyone who can open this review on the committee's side: the panel's members, the coordinators and institutional administrators of Kgosi Kgari Teaching Hospital, your programme's speciality and sub-speciality administrators, and Wombat's system administrators.
Why you are appealing *required
Up to 4,000 characters.
0 of 4,000 characters
Lodge appeal
### Agenda
The EPAs this review was there to decide for 2026 S2, and what the committee did with each.
| EPA
| Window
| Outcome
| PAED-001Providing paediatric emergency care to children
| 2026 S2
| DecidedSTAR #1.
| PAED-002Managing common paediatric presentations
| 2026 S2
| Not decided at this sittingTo be decided at her final review.
| PAED-003Providing intensive care to children
| 2026
| Not decided at this sittingTo be decided at her final review.
| PAED-004Managing common neonatal conditions
| 2026 S2
| Not decided at this sittingTo be decided at her final review.
| PAED-005Providing neonatal care in intensive and high-care settings
| 2026 S2
| Not decided at this sittingTo be decided at her final review.
| PAED-006Managing long-term health conditions (LTHCs)
| 2026
| Not decided at this sittingTo be decided at her final review.
| PAED-007Maintaining and promoting the health and well-being of children
| 2026
| Not decided at this sittingTo be decided at her final review.
| PAED-008Evaluating and managing neurodevelopmental and behavioural presentations in children
| 2026
| Not decided at this sittingTo be decided at her final review.
| PAED-009Providing care for an adolescent patient
| 2026
| Not decided at this sittingTo be decided at her final review.
| PAED-010Leading and operating within a clinical team
| 2026 S2
| DecidedSTAR #2.
| PAED-011Managing population health challenges
| 2026
| Not decided at this sittingTo be decided at her final review.
| PAED-012Communicating with and counselling patients, caregivers and healthcare teams
| 2026 S2
| DecidedSTAR #3.
| PAED-013Managing child protection, legal and ethical aspects of children at risk
| 2026
| Not decided at this sittingTo be decided at her final review.
| PAED-014Providing palliative and end-of-life care
| 2026
| Not decided at this sittingTo be decided at her final review.
| PAED-015Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
| 2026
| Not decided at this sittingTo be decided at her final review.
After a revocation, the agenda unchanged (4.42)Anele Dlamini · PAED-002 still reads Decided, STAR #5.: the agenda records what the sitting did; the revocation is on My authorisations · the rationale from the captures (S13)
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
[aria-label: Breadcrumb]
Home
My committee reviews
2026 S2 annual progression review
## 2026 S2 annual progression review
Paed Annual Review Panel
[aria-label: About this review]
Panel
Paed Annual Review Panel
Sits for
2026 S2
Evidence window
2026-01-01 to 2026-12-31
Type
Annual progression review
State
Ratified
### Decisions
#### Satisfactory Progress
Steady progress in year 3; two EPAs entrusted at this sitting.
About the EPAs not decided: Not at a decision point this year.
Present: Thandi Zulu (chair), David Naidoo, Sarah Botha
### Appeal
#### What happens next
Your appeal is heard by the panel's appeal body: today, Thandi Zulu (chair) and John van Rensburg (external).
They dismiss it, and the decision stands, or remit it, and take a new decision in its place. Either outcome is final: nothing reopens the review, and it cannot be appealed again.
Who reads your reason: everyone who can open this review on the committee's side: the panel's members, the coordinators and institutional administrators of Kgosi Kgari Teaching Hospital, your programme's speciality and sub-speciality administrators, and Wombat's system administrators.
Why you are appealing *required
Up to 4,000 characters.
0 of 4,000 characters
Lodge appeal
### Agenda
The EPAs this review was there to decide for 2026 S2, and what the committee did with each.
| EPA
| Window
| Outcome
| PAED-001Providing paediatric emergency care to children
| 2026 S2
| DecidedSTAR #4.
| PAED-002Managing common paediatric presentations
| 2026 S2
| DecidedSTAR #5.
… 13 more lines “Not decided at this sitting”, “Not at a decision point this year.”
Pre-graduation, every line Decided (5.8)Lerato Molefe · /committee/my-reviews/7 · h1 “2026 S2 pre-graduation review”; thirteen lines, STAR #6 to #18 (PAED-001 and 012 were at exit level before it sat); the lodge block offered, and she lodges nothing
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
My committee reviews
2026 S2 pre-graduation review
## 2026 S2 pre-graduation review
Paed Annual Review Panel
[aria-label: About this review]
Panel
Paed Annual Review Panel
Sits for
2026 S2
Evidence window
2023-01-15 to 2026-12-31
Type
Pre-graduation review
State
Ratified
### Decisions
#### Graduate (programme complete)
Every EPA is entrusted at its exit level, and the panel recommends completion.
Present: Thandi Zulu (chair), David Naidoo, Sarah Botha, John van Rensburg (external)
### Appeal
#### What happens next
Your appeal is heard by the panel's appeal body: today, Thandi Zulu (chair) and John van Rensburg (external).
They dismiss it, and the decision stands, or remit it, and take a new decision in its place. Either outcome is final: nothing reopens the review, and it cannot be appealed again.
Who reads your reason: everyone who can open this review on the committee's side: the panel's members, the coordinators and institutional administrators of Kgosi Kgari Teaching Hospital, your programme's speciality and sub-speciality administrators, and Wombat's system administrators.
Why you are appealing *required
Up to 4,000 characters.
0 of 4,000 characters
Lodge appeal
### Agenda
The EPAs this review was there to decide for 2026 S2, and what the committee did with each.
| EPA
| Window
| Outcome
| PAED-002Managing common paediatric presentations
| 2026 S2
| DecidedSTAR #6.
| PAED-003Providing intensive care to children
| 2026
| DecidedSTAR #7.
| PAED-004Managing common neonatal conditions
| 2026 S2
| DecidedSTAR #8.
| PAED-005Providing neonatal care in intensive and high-care settings
| 2026 S2
| DecidedSTAR #9.
| PAED-006Managing long-term health conditions (LTHCs)
| 2026
| DecidedSTAR #10.
| PAED-007Maintaining and promoting the health and well-being of children
| 2026
| DecidedSTAR #11.
| PAED-008Evaluating and managing neurodevelopmental and behavioural presentations in children
| 2026
| DecidedSTAR #12.
| PAED-009Providing care for an adolescent patient
| 2026
| DecidedSTAR #13.
| PAED-010Leading and operating within a clinical team
| 2026 S2
| DecidedSTAR #18.
| PAED-011Managing population health challenges
| 2026
| DecidedSTAR #14.
| PAED-013Managing child protection, legal and ethical aspects of children at risk
| 2026
| DecidedSTAR #15.
| PAED-014Providing palliative and end-of-life care
| 2026
| DecidedSTAR #16.
| PAED-015Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
| 2026
| DecidedSTAR #17.
A formative check-in: no decision, no lodgeSipho Ndlovu · /committee/my-reviews/6 · h1 “2026 S2 formative check-in”, distinct from his annual review's (C6); Type “Formative check-in” (T309); nothing to appeal, so no Appeal section; a check-in has no agendaNEW
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
[title: Sipho Ndlovu] Sipho Ndlovu
Sign out
[aria-label: Breadcrumb]
Home
My committee reviews
2026 S2 formative check-in
## 2026 S2 formative check-in
Paed Annual Review Panel
[aria-label: About this review]
Panel
Paed Annual Review Panel
Sits for
2026 S2
Evidence window
2026-01-01 to 2026-12-31
Type
Formative check-in
State
Closed
### Decisions
No binding decision. A formative check-in records no decision and issues no STAR.
His annual review, Outcome Deferred (4.51)Sipho Ndlovu · /committee/my-reviews/5 · Ratified, so the lodge block is offered
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
[title: Sipho Ndlovu] Sipho Ndlovu
Sign out
[aria-label: Breadcrumb]
Home
My committee reviews
2026 S2 annual progression review
## 2026 S2 annual progression review
Paed Annual Review Panel
[aria-label: About this review]
Panel
Paed Annual Review Panel
Sits for
2026 S2
Evidence window
2026-01-01 to 2026-12-31
Type
Annual progression review
State
Ratified
### Decisions
#### Outcome Deferred
One completed Mini-CEX and one reflection in his first semester; to be judged at the next sitting.
About the EPAs not decided: Too early in the programme to decide.
Present: Thandi Zulu (chair), Sarah Botha
### Appeal
#### What happens next
Your appeal is heard by the panel's appeal body: today, Thandi Zulu (chair) and John van Rensburg (external).
They dismiss it, and the decision stands, or remit it, and take a new decision in its place. Either outcome is final: nothing reopens the review, and it cannot be appealed again.
Who reads your reason: everyone who can open this review on the committee's side: the panel's members, the coordinators and institutional administrators of Kgosi Kgari Teaching Hospital, your programme's speciality and sub-speciality administrators, and Wombat's system administrators.
Why you are appealing *required
Up to 4,000 characters.
0 of 4,000 characters
Lodge appeal
### Agenda
The EPAs this review was there to decide for 2026 S2, and what the committee did with each.
| EPA
| Window
| Outcome
| PAED-001Providing paediatric emergency care to children
| 2026 S2
| Not decided at this sittingToo early in the programme to decide.
| PAED-002Managing common paediatric presentations
| 2026 S2
| Not decided at this sittingToo early in the programme to decide.
| PAED-003Providing intensive care to children
| 2026
| Not decided at this sittingToo early in the programme to decide.
… 12 more lines, as drawn in full on the 4.41 frame

######## BOARD R3-2-review-1280-b.dc.html — Her review · 1280 · b
## Her review page · 1280px · b: lodging
Route /committee/my-reviews/{id} (decided 2026-10-14), owned by My committee reviews: its nav item lit, its trail Home › My committee reviews › the h1; tab “<h1> · Wombat”. Only the review's own trainee, once it is ratified or later; anything else, every role, is flow 01's not found (F8).
Order One column at every width: header (its facts), Decisions, Appeal, Agenda.
Components PageHeader, Breadcrumbs, DetailCard, .detail-list (the facts), AgendaTable (read-only), Badge, Alert, ActionResult, ConfirmDialog (action mode, as built), StatePanel, Skeleton, FormField. NEW the page (.my-review-page on flow 07's .review-page); DecisionHistory, lifted; AppealEntry; the lodge block.
DESIGN.md § Detail page and the title rule; the owner table's row “A trainee's review | Trainee: My committee reviews”; § Entrustment-only reviews, “The trainee's reviews”. Round 3 F6, F8, M3, S1, S5, S9, S13, nits.
Before lodging (4.43)Nomsa Mahlangu · /committee/my-reviews/4 · the decision with its rationale and who was present (C2); “What happens next” (C8): the appeal body's names are PROPOSED data (the appeal body read for a Ratified review, a build change); the readers named trulyFocus: the h1PROPOSEDNEW
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
[title: Nomsa Mahlangu] Nomsa Mahlangu
Sign out
[aria-label: Breadcrumb]
Home
My committee reviews
2026 S2 annual progression review
## 2026 S2 annual progression review
Paed Annual Review Panel
[aria-label: About this review]
Panel
Paed Annual Review Panel
Sits for
2026 S2
Evidence window
2026-01-01 to 2026-12-31
Type
Annual progression review
State
Ratified
### Decisions
#### Inadequate Progress — Additional Training
One DOPS in the year and a Mini-CEX that stalled; a supervised support plan and re-review in six months.
About the EPAs not decided: Not enough observed evidence yet this year.
Present: Thandi Zulu (chair), Sarah Botha, John van Rensburg (external)
### Appeal
#### What happens next
Your appeal is heard by the panel's appeal body: today, Thandi Zulu (chair) and John van Rensburg (external).
They dismiss it, and the decision stands, or remit it, and take a new decision in its place. Either outcome is final: nothing reopens the review, and it cannot be appealed again.
Who reads your reason: everyone who can open this review on the committee's side: the panel's members, the coordinators and institutional administrators of Kgosi Kgari Teaching Hospital, your programme's speciality and sub-speciality administrators, and Wombat's system administrators.
Why you are appealing *required
Up to 4,000 characters.
0 of 4,000 characters
Lodge appeal
### Agenda
The EPAs this review was there to decide for 2026 S2, and what the committee did with each.
| EPA
| Window
| Outcome
| PAED-001Providing paediatric emergency care to children
| 2026 S2
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-002Managing common paediatric presentations
| 2026 S2
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-003Providing intensive care to children
| 2026
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-004Managing common neonatal conditions
| 2026 S2
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-005Providing neonatal care in intensive and high-care settings
| 2026 S2
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-006Managing long-term health conditions (LTHCs)
| 2026
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-007Maintaining and promoting the health and well-being of children
| 2026
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-008Evaluating and managing neurodevelopmental and behavioural presentations in children
| 2026
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-009Providing care for an adolescent patient
| 2026
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-010Leading and operating within a clinical team
| 2026 S2
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-011Managing population health challenges
| 2026
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-012Communicating with and counselling patients, caregivers and healthcare teams
| 2026 S2
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-013Managing child protection, legal and ethical aspects of children at risk
| 2026
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-014Providing palliative and end-of-life care
| 2026
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-015Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
| 2026
| Not decided at this sittingNot enough observed evidence yet this year.
Lodge appeal with the reason blankNothing is sent; “Give your reason for appealing.” under the field (T324); the field marked with its danger edge and stripe; the counter a second .field-help (S9)Focus: the fieldNEW
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
[title: Nomsa Mahlangu] Nomsa Mahlangu
Sign out
[aria-label: Breadcrumb]
Home
My committee reviews
2026 S2 annual progression review
## 2026 S2 annual progression review
Paed Annual Review Panel
[aria-label: About this review]
Panel
Paed Annual Review Panel
Sits for
2026 S2
Evidence window
2026-01-01 to 2026-12-31
Type
Annual progression review
State
Ratified
### Decisions
#### Inadequate Progress — Additional Training
One DOPS in the year and a Mini-CEX that stalled; a supervised support plan and re-review in six months.
About the EPAs not decided: Not enough observed evidence yet this year.
Present: Thandi Zulu (chair), Sarah Botha, John van Rensburg (external)
### Appeal
#### What happens next
Your appeal is heard by the panel's appeal body: today, Thandi Zulu (chair) and John van Rensburg (external).
They dismiss it, and the decision stands, or remit it, and take a new decision in its place. Either outcome is final: nothing reopens the review, and it cannot be appealed again.
Who reads your reason: everyone who can open this review on the committee's side: the panel's members, the coordinators and institutional administrators of Kgosi Kgari Teaching Hospital, your programme's speciality and sub-speciality administrators, and Wombat's system administrators.
Why you are appealing *required
Up to 4,000 characters.
Give your reason for appealing.
0 of 4,000 characters
Lodge appeal
### Agenda
The EPAs this review was there to decide for 2026 S2, and what the committee did with each.
| EPA
| Window
| Outcome
| PAED-001Providing paediatric emergency care to children
| 2026 S2
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-002Managing common paediatric presentations
| 2026 S2
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-003Providing intensive care to children
| 2026
| Not decided at this sittingNot enough observed evidence yet this year.
… 12 more lines, as drawn in full on the 4.41 frame
Lodge appeal pressed: the dialog (C14a)ConfirmDialog, action mode, as built · the safe button “Keep editing” has the focusFocus: Keep editingPROPOSED
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
[title: Nomsa Mahlangu] Nomsa Mahlangu
Sign out
[aria-label: Breadcrumb]
Home
My committee reviews
2026 S2 annual progression review
## 2026 S2 annual progression review
Paed Annual Review Panel
[aria-label: About this review]
Panel
Paed Annual Review Panel
Sits for
2026 S2
Evidence window
2026-01-01 to 2026-12-31
Type
Annual progression review
State
Ratified
### Decisions
#### Inadequate Progress — Additional Training
One DOPS in the year and a Mini-CEX that stalled; a supervised support plan and re-review in six months.
About the EPAs not decided: Not enough observed evidence yet this year.
Present: Thandi Zulu (chair), Sarah Botha, John van Rensburg (external)
### Appeal
#### What happens next
Your appeal is heard by the panel's appeal body: today, Thandi Zulu (chair) and John van Rensburg (external).
They dismiss it, and the decision stands, or remit it, and take a new decision in its place. Either outcome is final: nothing reopens the review, and it cannot be appealed again.
Who reads your reason: everyone who can open this review on the committee's side: the panel's members, the coordinators and institutional administrators of Kgosi Kgari Teaching Hospital, your programme's speciality and sub-speciality administrators, and Wombat's system administrators.
Why you are appealing *required
The single DOPS reflects the start of the year, and the Mini-CEX waited eight days on the assessor's side before it was rated. I ask for reconsideration.
Up to 4,000 characters.
153 of 4,000 characters
Lodge appeal
### Agenda
The EPAs this review was there to decide for 2026 S2, and what the committee did with each.
| EPA
| Window
| Outcome
| PAED-001Providing paediatric emergency care to children
| 2026 S2
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-002Managing common paediatric presentations
| 2026 S2
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-003Providing intensive care to children
| 2026
| Not decided at this sittingNot enough observed evidence yet this year.
… 12 more lines, as drawn in full on the 4.41 frame
### Lodge your appeal?
Against the 2026 S2 annual progression review's decision, Inadequate Progress — Additional Training.
Your appeal is heard by the panel's appeal body: today, Thandi Zulu (chair) and John van Rensburg (external). Either outcome is final.
Who reads your reason: everyone who can open this review on the committee's side: the panel's members, the coordinators and institutional administrators of Kgosi Kgari Teaching Hospital, your programme's speciality and sub-speciality administrators, and Wombat's system administrators.
Keep editing
Lodge appeal
Keep editing (F6)The dialog closes as the native dialog does; the reason is kept (153 of 4,000)Focus: back on Lodge appeal, the button that opened itR3
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
[title: Nomsa Mahlangu] Nomsa Mahlangu
Sign out
[aria-label: Breadcrumb]
Home
My committee reviews
2026 S2 annual progression review
## 2026 S2 annual progression review
Paed Annual Review Panel
[aria-label: About this review]
Panel
Paed Annual Review Panel
Sits for
2026 S2
Evidence window
2026-01-01 to 2026-12-31
Type
Annual progression review
State
Ratified
### Decisions
#### Inadequate Progress — Additional Training
One DOPS in the year and a Mini-CEX that stalled; a supervised support plan and re-review in six months.
About the EPAs not decided: Not enough observed evidence yet this year.
Present: Thandi Zulu (chair), Sarah Botha, John van Rensburg (external)
### Appeal
#### What happens next
Your appeal is heard by the panel's appeal body: today, Thandi Zulu (chair) and John van Rensburg (external).
They dismiss it, and the decision stands, or remit it, and take a new decision in its place. Either outcome is final: nothing reopens the review, and it cannot be appealed again.
Who reads your reason: everyone who can open this review on the committee's side: the panel's members, the coordinators and institutional administrators of Kgosi Kgari Teaching Hospital, your programme's speciality and sub-speciality administrators, and Wombat's system administrators.
Why you are appealing *required
The single DOPS reflects the start of the year, and the Mini-CEX waited eight days on the assessor's side before it was rated. I ask for reconsideration.
Up to 4,000 characters.
153 of 4,000 characters
Lodge appeal
### Agenda
The EPAs this review was there to decide for 2026 S2, and what the committee did with each.
| EPA
| Window
| Outcome
| PAED-001Providing paediatric emergency care to children
| 2026 S2
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-002Managing common paediatric presentations
| 2026 S2
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-003Providing intensive care to children
| 2026
| Not decided at this sittingNot enough observed evidence yet this year.
… 12 more lines, as drawn in full on the 4.41 frame
Lodged (4.43)“Appeal lodged.” heads her Appeal section; State reads Under appeal at once; her entry Open, Heard by the appeal body (T308); the form is gone (one open appeal at a time)Focus: the resultNEW
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
[title: Nomsa Mahlangu] Nomsa Mahlangu
Sign out
[aria-label: Breadcrumb]
Home
My committee reviews
2026 S2 annual progression review
## 2026 S2 annual progression review
Paed Annual Review Panel
[aria-label: About this review]
Panel
Paed Annual Review Panel
Sits for
2026 S2
Evidence window
2026-01-01 to 2026-12-31
Type
Annual progression review
State
Under appeal
### Decisions
#### Inadequate Progress — Additional Training
One DOPS in the year and a Mini-CEX that stalled; a supervised support plan and re-review in six months.
About the EPAs not decided: Not enough observed evidence yet this year.
Present: Thandi Zulu (chair), Sarah Botha, John van Rensburg (external)
### Appeal
Appeal lodged.
#### Your appeal Open
Lodged 2026-10-13
Heard by Thandi Zulu (chair) and John van Rensburg (external).
Reason
The single DOPS reflects the start of the year, and the Mini-CEX waited eight days on the assessor's side before it was rated. I ask for reconsideration.
### Agenda
The EPAs this review was there to decide for 2026 S2, and what the committee did with each.
| EPA
| Window
| Outcome
| PAED-001Providing paediatric emergency care to children
| 2026 S2
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-002Managing common paediatric presentations
| 2026 S2
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-003Providing intensive care to children
| 2026
| Not decided at this sittingNot enough observed evidence yet this year.
… 12 more lines, as drawn in full on the 4.41 frame
A lodge refused (M3)Scratch state: a second tab lodged first, so this review is Under appeal · Lodge appeal is refused with the domain's words, under the button; the reason keptFocus: the refusalR3
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
[title: Nomsa Mahlangu] Nomsa Mahlangu
Sign out
[aria-label: Breadcrumb]
Home
My committee reviews
2026 S2 annual progression review
## 2026 S2 annual progression review
Paed Annual Review Panel
[aria-label: About this review]
Panel
Paed Annual Review Panel
Sits for
2026 S2
Evidence window
2026-01-01 to 2026-12-31
Type
Annual progression review
State
Ratified
### Decisions
#### Inadequate Progress — Additional Training
One DOPS in the year and a Mini-CEX that stalled; a supervised support plan and re-review in six months.
About the EPAs not decided: Not enough observed evidence yet this year.
Present: Thandi Zulu (chair), Sarah Botha, John van Rensburg (external)
### Appeal
#### What happens next
Your appeal is heard by the panel's appeal body: today, Thandi Zulu (chair) and John van Rensburg (external).
They dismiss it, and the decision stands, or remit it, and take a new decision in its place. Either outcome is final: nothing reopens the review, and it cannot be appealed again.
Who reads your reason: everyone who can open this review on the committee's side: the panel's members, the coordinators and institutional administrators of Kgosi Kgari Teaching Hospital, your programme's speciality and sub-speciality administrators, and Wombat's system administrators.
Why you are appealing *required
The single DOPS reflects the start of the year, and the Mini-CEX waited eight days on the assessor's side before it was rated. I ask for reconsideration.
Up to 4,000 characters.
153 of 4,000 characters
Lodge appeal
Only ratified reviews can be appealed.
### Agenda
The EPAs this review was there to decide for 2026 S2, and what the committee did with each.
| EPA
| Window
| Outcome
| PAED-001Providing paediatric emergency care to children
| 2026 S2
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-002Managing common paediatric presentations
| 2026 S2
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-003Providing intensive care to children
| 2026
| Not decided at this sittingNot enough observed evidence yet this year.
… 12 more lines, as drawn in full on the 4.41 frame
Then the page reads the review again (S5)The form is gone; State Under appeal; the open entry from the other tab; the refusal still shown at the head of her Appeal section, with the focusR3
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
[title: Nomsa Mahlangu] Nomsa Mahlangu
Sign out
[aria-label: Breadcrumb]
Home
My committee reviews
2026 S2 annual progression review
## 2026 S2 annual progression review
Paed Annual Review Panel
[aria-label: About this review]
Panel
Paed Annual Review Panel
Sits for
2026 S2
Evidence window
2026-01-01 to 2026-12-31
Type
Annual progression review
State
Under appeal
### Decisions
#### Inadequate Progress — Additional Training
One DOPS in the year and a Mini-CEX that stalled; a supervised support plan and re-review in six months.
About the EPAs not decided: Not enough observed evidence yet this year.
Present: Thandi Zulu (chair), Sarah Botha, John van Rensburg (external)
### Appeal
Only ratified reviews can be appealed.
#### Your appeal Open
Lodged 2026-10-13
Heard by Thandi Zulu (chair) and John van Rensburg (external).
Reason
The single DOPS reflects the start of the year, and the Mini-CEX waited eight days on the assessor's side before it was rated. I ask for reconsideration.
### Agenda
The EPAs this review was there to decide for 2026 S2, and what the committee did with each.
| EPA
| Window
| Outcome
| PAED-001Providing paediatric emergency care to children
| 2026 S2
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-002Managing common paediatric presentations
| 2026 S2
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-003Providing intensive care to children
| 2026
| Not decided at this sittingNot enough observed evidence yet this year.
… 12 more lines, as drawn in full on the 4.41 frame

######## BOARD R3-2-review-1280-c.dc.html — Her review · 1280 · c
## Her review page · 1280px · c: outcomes and states
Route /committee/my-reviews/{id} (decided 2026-10-14), owned by My committee reviews: its nav item lit, its trail Home › My committee reviews › the h1; tab “<h1> · Wombat”. Only the review's own trainee, once it is ratified or later; anything else, every role, is flow 01's not found (F8).
Order One column at every width: header (its facts), Decisions, Appeal, Agenda.
Components PageHeader, Breadcrumbs, DetailCard, .detail-list (the facts), AgendaTable (read-only), Badge, Alert, ActionResult, ConfirmDialog (action mode, as built), StatePanel, Skeleton, FormField. NEW the page (.my-review-page on flow 07's .review-page); DecisionHistory, lifted; AppealEntry; the lodge block.
DESIGN.md § Detail page and the title rule; the owner table's row “A trainee's review | Trainee: My committee reviews”; § Entrustment-only reviews, “The trainee's reviews”. Round 3 F6, F8, M3, S1, S5, S9, S13, nits.
Closed after a remit (4.48)Nomsa Mahlangu · two decisions, newest first, the first marked “Replaced on appeal by the decision above.” with its rationale and who sat (T308); her entry Remitted, “Resolved 2026-10-13 by Thandi Zulu” (C15); no lodge formFocus: the h1NEW
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
[title: Nomsa Mahlangu] Nomsa Mahlangu
Sign out
[aria-label: Breadcrumb]
Home
My committee reviews
2026 S2 annual progression review
## 2026 S2 annual progression review
Paed Annual Review Panel
[aria-label: About this review]
Panel
Paed Annual Review Panel
Sits for
2026 S2
Evidence window
2026-01-01 to 2026-12-31
Type
Annual progression review
State
Closed
### Decisions
#### Satisfactory with Observations
The stalled Mini-CEX was the assessor's delay; progress is adequate, with observed assessments to follow.
About the EPAs not decided: Not enough observed evidence yet this year.
Conditions: Two observed Mini-CEX and one DOPS before the next review.
Present: Thandi Zulu (chair), Sarah Botha
#### Inadequate Progress — Additional Training
Replaced on appeal by the decision above.
One DOPS in the year and a Mini-CEX that stalled; a supervised support plan and re-review in six months.
About the EPAs not decided: Not enough observed evidence yet this year.
Present: Thandi Zulu (chair), Sarah Botha, John van Rensburg (external)
### Appeal
#### Your appeal Remitted
Lodged 2026-10-13 · Resolved 2026-10-13 by Thandi Zulu
Remitted: the appeal body replaced the decision. The decision above is theirs, and it is final.
Reason
The single DOPS reflects the start of the year, and the Mini-CEX waited eight days on the assessor's side before it was rated. I ask for reconsideration.
This review is closed and cannot be appealed again.
### Agenda
The EPAs this review was there to decide for 2026 S2, and what the committee did with each.
| EPA
| Window
| Outcome
| PAED-001Providing paediatric emergency care to children
| 2026 S2
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-002Managing common paediatric presentations
| 2026 S2
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-003Providing intensive care to children
| 2026
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-004Managing common neonatal conditions
| 2026 S2
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-005Providing neonatal care in intensive and high-care settings
| 2026 S2
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-006Managing long-term health conditions (LTHCs)
| 2026
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-007Maintaining and promoting the health and well-being of children
| 2026
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-008Evaluating and managing neurodevelopmental and behavioural presentations in children
| 2026
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-009Providing care for an adolescent patient
| 2026
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-010Leading and operating within a clinical team
| 2026 S2
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-011Managing population health challenges
| 2026
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-012Communicating with and counselling patients, caregivers and healthcare teams
| 2026 S2
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-013Managing child protection, legal and ethical aspects of children at risk
| 2026
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-014Providing palliative and end-of-life care
| 2026
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-015Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
| 2026
| Not decided at this sittingNot enough observed evidence yet this year.
Closed after a dismissalScratch database (states/review-detail--appeal-dismissed): Anele Dlamini appealed review #2 on 2026-10-12; Thandi Zulu dismissed it · the decision stands, alone in the history
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
[aria-label: Breadcrumb]
Home
My committee reviews
2026 S2 annual progression review
## 2026 S2 annual progression review
Paed Annual Review Panel
[aria-label: About this review]
Panel
Paed Annual Review Panel
Sits for
2026 S2
Evidence window
2026-01-01 to 2026-12-31
Type
Annual progression review
State
Closed
### Decisions
#### Satisfactory Progress
Steady progress in year 3; two EPAs entrusted at this sitting.
About the EPAs not decided: Not at a decision point this year.
Present: Thandi Zulu (chair), David Naidoo, Sarah Botha
### Appeal
#### Your appeal Dismissed
Lodged 2026-10-12 · Resolved 2026-10-13 by Thandi Zulu
Dismissed: the decision stands, and it is final.
Reason
The PAED-002 decision rested on PAED-001 evidence only. I ask for reconsideration.
This review is closed and cannot be appealed again.
### Agenda
The EPAs this review was there to decide for 2026 S2, and what the committee did with each.
| EPA
| Window
| Outcome
| PAED-001Providing paediatric emergency care to children
| 2026 S2
| DecidedSTAR #4.
| PAED-002Managing common paediatric presentations
| 2026 S2
| DecidedSTAR #5.
… 13 more lines, as on the 4.42 frame
Illustration: an entrustment-only review, remitted (C17)No cast name: no step appeals one · h1 “2026 S1 entrustment-only review”; the two sentences under the current decision's heading; the replaced decision's h3 says so; no category; the STARs it issued stand
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
[title: ‹trainee›] ‹trainee›
Sign out
[aria-label: Breadcrumb]
Home
My committee reviews
2026 S1 entrustment-only review
## 2026 S1 entrustment-only review
Paed Annual Review Panel
[aria-label: About this review]
Panel
Paed Annual Review Panel
Sits for
2026 S2
Evidence window
2026-01-01 to 2026-06-30
Type
Entrustment-only review
State
Closed
### Decisions
#### Entrustment decisions only
This review decided entrustment only, so it records no progression outcome.
The appeal was remitted: this decision is the appeal body's, and the entrustment decisions this review issued stand unless one is revoked.
What it decided on each EPA is on its agenda below.
‹the appeal body's rationale›
Present: ‹chair› (chair), ‹member›
#### Entrustment decisions only, replaced on appeal
This review decided entrustment only, so it records no progression outcome.
Replaced on appeal by the decision above.
‹the committee's rationale›
Present: ‹chair› (chair), ‹members›
### Appeal
#### Your appeal Remitted
Lodged ‹date› · Resolved ‹date› by ‹resolver›
Remitted: the appeal body replaced the decision. The decision above is theirs, and it is final.
Reason
‹her reason›
### Agenda
The EPAs this review was there to decide for 2026 S1, and what the committee did with each.
| EPA
| Window
| Outcome
| PAED-004Managing common neonatal conditions
| 2026 S1
| DecidedSTAR #‹n›.
| PAED-005Providing neonatal care in intensive and high-care settings
| 2026 S1
| DecidedSTAR #‹n›.
Not found (Q1, F8)Lerato Molefe on /committee/my-reviews/4 (Nomsa Mahlangu's), or on one of hers not yet ratified: flow 01's Page not found, word for word; nothing lit, no trail. The same for every role, the Administrator included: she reads a review at /committee/reviews/{id}R3
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
## Page not found
There is no page at this address.
Check the address, or start again from Home.
Go to Home
LoadingThe header says “Committee review” until the read is in (as flow 07's page does, S12); a status line; skeletons in the page's shape; no lodge form
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
My committee reviews
Committee review
## Committee review
Loading your review.
Load error (T329)Flow 07's fixed sentence and Try again; the header stays “Committee review”Focus: the alert
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
My committee reviews
Committee review
## Committee review
The review could not be loaded. Nothing on it has changed.
Try again
Access denied: the committee's address (4.41, C7)Lerato Molefe types /committee/reviews/1 · flow 01's page, unchanged: nothing lit, no trail, Go to Home
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
## You cannot open this page
Your role (Trainee) does not open this page.
If you need it for your work, ask your institution's Wombat administrator.
Go to Home

######## BOARD R3-2-review-768.dc.html — Her review · 768
## Her review page · 768px
Route /committee/my-reviews/{id} (decided 2026-10-14), owned by My committee reviews: its nav item lit, its trail Home › My committee reviews › the h1; tab “<h1> · Wombat”. Only the review's own trainee, once it is ratified or later; anything else, every role, is flow 01's not found (F8).
Order One column at every width: header (its facts), Decisions, Appeal, Agenda.
Components PageHeader, Breadcrumbs, DetailCard, .detail-list (the facts), AgendaTable (read-only), Badge, Alert, ActionResult, ConfirmDialog (action mode, as built), StatePanel, Skeleton, FormField. NEW the page (.my-review-page on flow 07's .review-page); DecisionHistory, lifted; AppealEntry; the lodge block.
DESIGN.md § Detail page and the title rule; the owner table's row “A trainee's review | Trainee: My committee reviews”; § Entrustment-only reviews, “The trainee's reviews”. Round 3 F6, F8, M3, S1, S5, S9, S13, nits.
Before lodging (4.43)Nomsa Mahlangu · /committee/my-reviews/4 · the decision with its rationale and who was present (C2); “What happens next” (C8): the appeal body's names are PROPOSED data (the appeal body read for a Ratified review, a build change); the readers named trulyFocus: the h1PROPOSEDNEW
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
[title: Nomsa Mahlangu] Nomsa Mahlangu
Sign out
[aria-label: Breadcrumb]
Home
My committee reviews
2026 S2 annual progression review
## 2026 S2 annual progression review
Paed Annual Review Panel
[aria-label: About this review]
Panel
Paed Annual Review Panel
Sits for
2026 S2
Evidence window
2026-01-01 to 2026-12-31
Type
Annual progression review
State
Ratified
### Decisions
#### Inadequate Progress — Additional Training
One DOPS in the year and a Mini-CEX that stalled; a supervised support plan and re-review in six months.
About the EPAs not decided: Not enough observed evidence yet this year.
Present: Thandi Zulu (chair), Sarah Botha, John van Rensburg (external)
### Appeal
#### What happens next
Your appeal is heard by the panel's appeal body: today, Thandi Zulu (chair) and John van Rensburg (external).
They dismiss it, and the decision stands, or remit it, and take a new decision in its place. Either outcome is final: nothing reopens the review, and it cannot be appealed again.
Who reads your reason: everyone who can open this review on the committee's side: the panel's members, the coordinators and institutional administrators of Kgosi Kgari Teaching Hospital, your programme's speciality and sub-speciality administrators, and Wombat's system administrators.
Why you are appealing *required
Up to 4,000 characters.
0 of 4,000 characters
Lodge appeal
### Agenda
The EPAs this review was there to decide for 2026 S2, and what the committee did with each.
| EPA
| Window
| Outcome
| PAED-001Providing paediatric emergency care to children
| 2026 S2
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-002Managing common paediatric presentations
| 2026 S2
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-003Providing intensive care to children
| 2026
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-004Managing common neonatal conditions
| 2026 S2
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-005Providing neonatal care in intensive and high-care settings
| 2026 S2
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-006Managing long-term health conditions (LTHCs)
| 2026
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-007Maintaining and promoting the health and well-being of children
| 2026
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-008Evaluating and managing neurodevelopmental and behavioural presentations in children
| 2026
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-009Providing care for an adolescent patient
| 2026
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-010Leading and operating within a clinical team
| 2026 S2
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-011Managing population health challenges
| 2026
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-012Communicating with and counselling patients, caregivers and healthcare teams
| 2026 S2
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-013Managing child protection, legal and ethical aspects of children at risk
| 2026
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-014Providing palliative and end-of-life care
| 2026
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-015Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
| 2026
| Not decided at this sittingNot enough observed evidence yet this year.
Closed after a remit (4.48)Nomsa Mahlangu · two decisions, newest first, the first marked “Replaced on appeal by the decision above.” with its rationale and who sat (T308); her entry Remitted, “Resolved 2026-10-13 by Thandi Zulu” (C15); no lodge formFocus: the h1NEW
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
[title: Nomsa Mahlangu] Nomsa Mahlangu
Sign out
[aria-label: Breadcrumb]
Home
My committee reviews
2026 S2 annual progression review
## 2026 S2 annual progression review
Paed Annual Review Panel
[aria-label: About this review]
Panel
Paed Annual Review Panel
Sits for
2026 S2
Evidence window
2026-01-01 to 2026-12-31
Type
Annual progression review
State
Closed
### Decisions
#### Satisfactory with Observations
The stalled Mini-CEX was the assessor's delay; progress is adequate, with observed assessments to follow.
About the EPAs not decided: Not enough observed evidence yet this year.
Conditions: Two observed Mini-CEX and one DOPS before the next review.
Present: Thandi Zulu (chair), Sarah Botha
#### Inadequate Progress — Additional Training
Replaced on appeal by the decision above.
One DOPS in the year and a Mini-CEX that stalled; a supervised support plan and re-review in six months.
About the EPAs not decided: Not enough observed evidence yet this year.
Present: Thandi Zulu (chair), Sarah Botha, John van Rensburg (external)
### Appeal
#### Your appeal Remitted
Lodged 2026-10-13 · Resolved 2026-10-13 by Thandi Zulu
Remitted: the appeal body replaced the decision. The decision above is theirs, and it is final.
Reason
The single DOPS reflects the start of the year, and the Mini-CEX waited eight days on the assessor's side before it was rated. I ask for reconsideration.
This review is closed and cannot be appealed again.
### Agenda
The EPAs this review was there to decide for 2026 S2, and what the committee did with each.
| EPA
| Window
| Outcome
| PAED-001Providing paediatric emergency care to children
| 2026 S2
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-002Managing common paediatric presentations
| 2026 S2
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-003Providing intensive care to children
| 2026
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-004Managing common neonatal conditions
| 2026 S2
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-005Providing neonatal care in intensive and high-care settings
| 2026 S2
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-006Managing long-term health conditions (LTHCs)
| 2026
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-007Maintaining and promoting the health and well-being of children
| 2026
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-008Evaluating and managing neurodevelopmental and behavioural presentations in children
| 2026
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-009Providing care for an adolescent patient
| 2026
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-010Leading and operating within a clinical team
| 2026 S2
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-011Managing population health challenges
| 2026
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-012Communicating with and counselling patients, caregivers and healthcare teams
| 2026 S2
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-013Managing child protection, legal and ethical aspects of children at risk
| 2026
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-014Providing palliative and end-of-life care
| 2026
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-015Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
| 2026
| Not decided at this sittingNot enough observed evidence yet this year.

######## BOARD R3-2-review-390-a.dc.html — Her review · 390 · a
## Her review page · 390px · a: reading
Route /committee/my-reviews/{id} (decided 2026-10-14), owned by My committee reviews: its nav item lit, its trail Home › My committee reviews › the h1; tab “<h1> · Wombat”. Only the review's own trainee, once it is ratified or later; anything else, every role, is flow 01's not found (F8).
Order One column at every width: header (its facts), Decisions, Appeal, Agenda.
Components PageHeader, Breadcrumbs, DetailCard, .detail-list (the facts), AgendaTable (read-only), Badge, Alert, ActionResult, ConfirmDialog (action mode, as built), StatePanel, Skeleton, FormField. NEW the page (.my-review-page on flow 07's .review-page); DecisionHistory, lifted; AppealEntry; the lodge block.
DESIGN.md § Detail page and the title rule; the owner table's row “A trainee's review | Trainee: My committee reviews”; § Entrustment-only reviews, “The trainee's reviews”. Round 3 F6, F8, M3, S1, S5, S9, S13, nits.
Ratified, with the lodge block (4.41)Lerato Molefe · /committee/my-reviews/1 · h1 and tab “2026 S2 annual progression review · Wombat”; trail Home › My committee reviews › 2026 S2 annual progression review; Decided lines read “STAR #n.” with no download (Q6)Focus: the h1 (FocusOnNavigate)PROPOSEDNEW
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
My committee reviews
2026 S2 annual progression review
## 2026 S2 annual progression review
Paed Annual Review Panel
[aria-label: About this review]
Panel
Paed Annual Review Panel
Sits for
2026 S2
Evidence window
2026-01-01 to 2026-12-31
Type
Annual progression review
State
Ratified
### Decisions
#### Satisfactory Progress
On track for completion; three EPAs entrusted, the rest to be decided at her final review.
About the EPAs not decided: To be decided at her final review.
Present: Thandi Zulu (chair), David Naidoo, Sarah Botha, John van Rensburg (external)
### Appeal
#### What happens next
Your appeal is heard by the panel's appeal body: today, Thandi Zulu (chair) and John van Rensburg (external).
They dismiss it, and the decision stands, or remit it, and take a new decision in its place. Either outcome is final: nothing reopens the review, and it cannot be appealed again.
Who reads your reason: everyone who can open this review on the committee's side: the panel's members, the coordinators and institutional administrators of Kgosi Kgari Teaching Hospital, your programme's speciality and sub-speciality administrators, and Wombat's system administrators.
Why you are appealing *required
Up to 4,000 characters.
0 of 4,000 characters
Lodge appeal
### Agenda
The EPAs this review was there to decide for 2026 S2, and what the committee did with each.
| EPA
| Window
| Outcome
| PAED-001Providing paediatric emergency care to children
| 2026 S2
| DecidedSTAR #1.
| PAED-002Managing common paediatric presentations
| 2026 S2
| Not decided at this sittingTo be decided at her final review.
| PAED-003Providing intensive care to children
| 2026
| Not decided at this sittingTo be decided at her final review.
| PAED-004Managing common neonatal conditions
| 2026 S2
| Not decided at this sittingTo be decided at her final review.
| PAED-005Providing neonatal care in intensive and high-care settings
| 2026 S2
| Not decided at this sittingTo be decided at her final review.
| PAED-006Managing long-term health conditions (LTHCs)
| 2026
| Not decided at this sittingTo be decided at her final review.
| PAED-007Maintaining and promoting the health and well-being of children
| 2026
| Not decided at this sittingTo be decided at her final review.
| PAED-008Evaluating and managing neurodevelopmental and behavioural presentations in children
| 2026
| Not decided at this sittingTo be decided at her final review.
| PAED-009Providing care for an adolescent patient
| 2026
| Not decided at this sittingTo be decided at her final review.
| PAED-010Leading and operating within a clinical team
| 2026 S2
| DecidedSTAR #2.
| PAED-011Managing population health challenges
| 2026
| Not decided at this sittingTo be decided at her final review.
| PAED-012Communicating with and counselling patients, caregivers and healthcare teams
| 2026 S2
| DecidedSTAR #3.
| PAED-013Managing child protection, legal and ethical aspects of children at risk
| 2026
| Not decided at this sittingTo be decided at her final review.
| PAED-014Providing palliative and end-of-life care
| 2026
| Not decided at this sittingTo be decided at her final review.
| PAED-015Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
| 2026
| Not decided at this sittingTo be decided at her final review.
After a revocation, the agenda unchanged (4.42)Anele Dlamini · PAED-002 still reads Decided, STAR #5.: the agenda records what the sitting did; the revocation is on My authorisations · the rationale from the captures (S13)
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
[aria-label: Breadcrumb]
Home
My committee reviews
2026 S2 annual progression review
## 2026 S2 annual progression review
Paed Annual Review Panel
[aria-label: About this review]
Panel
Paed Annual Review Panel
Sits for
2026 S2
Evidence window
2026-01-01 to 2026-12-31
Type
Annual progression review
State
Ratified
### Decisions
#### Satisfactory Progress
Steady progress in year 3; two EPAs entrusted at this sitting.
About the EPAs not decided: Not at a decision point this year.
Present: Thandi Zulu (chair), David Naidoo, Sarah Botha
### Appeal
#### What happens next
Your appeal is heard by the panel's appeal body: today, Thandi Zulu (chair) and John van Rensburg (external).
They dismiss it, and the decision stands, or remit it, and take a new decision in its place. Either outcome is final: nothing reopens the review, and it cannot be appealed again.
Who reads your reason: everyone who can open this review on the committee's side: the panel's members, the coordinators and institutional administrators of Kgosi Kgari Teaching Hospital, your programme's speciality and sub-speciality administrators, and Wombat's system administrators.
Why you are appealing *required
Up to 4,000 characters.
0 of 4,000 characters
Lodge appeal
### Agenda
The EPAs this review was there to decide for 2026 S2, and what the committee did with each.
| EPA
| Window
| Outcome
| PAED-001Providing paediatric emergency care to children
| 2026 S2
| DecidedSTAR #4.
| PAED-002Managing common paediatric presentations
| 2026 S2
| DecidedSTAR #5.
… 13 more lines “Not decided at this sitting”, “Not at a decision point this year.”
Pre-graduation, every line Decided (5.8)Lerato Molefe · /committee/my-reviews/7 · h1 “2026 S2 pre-graduation review”; thirteen lines, STAR #6 to #18 (PAED-001 and 012 were at exit level before it sat); the lodge block offered, and she lodges nothingR3
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
My committee reviews
2026 S2 pre-graduation review
## 2026 S2 pre-graduation review
Paed Annual Review Panel
[aria-label: About this review]
Panel
Paed Annual Review Panel
Sits for
2026 S2
Evidence window
2023-01-15 to 2026-12-31
Type
Pre-graduation review
State
Ratified
### Decisions
#### Graduate (programme complete)
Every EPA is entrusted at its exit level, and the panel recommends completion.
Present: Thandi Zulu (chair), David Naidoo, Sarah Botha, John van Rensburg (external)
### Appeal
#### What happens next
Your appeal is heard by the panel's appeal body: today, Thandi Zulu (chair) and John van Rensburg (external).
They dismiss it, and the decision stands, or remit it, and take a new decision in its place. Either outcome is final: nothing reopens the review, and it cannot be appealed again.
Who reads your reason: everyone who can open this review on the committee's side: the panel's members, the coordinators and institutional administrators of Kgosi Kgari Teaching Hospital, your programme's speciality and sub-speciality administrators, and Wombat's system administrators.
Why you are appealing *required
Up to 4,000 characters.
0 of 4,000 characters
Lodge appeal
### Agenda
The EPAs this review was there to decide for 2026 S2, and what the committee did with each.
| EPA
| Window
| Outcome
| PAED-002Managing common paediatric presentations
| 2026 S2
| DecidedSTAR #6.
| PAED-003Providing intensive care to children
| 2026
| DecidedSTAR #7.
| PAED-004Managing common neonatal conditions
| 2026 S2
| DecidedSTAR #8.
| PAED-005Providing neonatal care in intensive and high-care settings
| 2026 S2
| DecidedSTAR #9.
| PAED-006Managing long-term health conditions (LTHCs)
| 2026
| DecidedSTAR #10.
| PAED-007Maintaining and promoting the health and well-being of children
| 2026
| DecidedSTAR #11.
| PAED-008Evaluating and managing neurodevelopmental and behavioural presentations in children
| 2026
| DecidedSTAR #12.
| PAED-009Providing care for an adolescent patient
| 2026
| DecidedSTAR #13.
| PAED-010Leading and operating within a clinical team
| 2026 S2
| DecidedSTAR #18.
| PAED-011Managing population health challenges
| 2026
| DecidedSTAR #14.
| PAED-013Managing child protection, legal and ethical aspects of children at risk
| 2026
| DecidedSTAR #15.
| PAED-014Providing palliative and end-of-life care
| 2026
| DecidedSTAR #16.
| PAED-015Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
| 2026
| DecidedSTAR #17.
A formative check-in: no decision, no lodgeSipho Ndlovu · /committee/my-reviews/6 · h1 “2026 S2 formative check-in”, distinct from his annual review's (C6); Type “Formative check-in” (T309); nothing to appeal, so no Appeal section; a check-in has no agendaNEWR3
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
[title: Sipho Ndlovu] Sipho Ndlovu
Sign out
[aria-label: Breadcrumb]
Home
My committee reviews
2026 S2 formative check-in
## 2026 S2 formative check-in
Paed Annual Review Panel
[aria-label: About this review]
Panel
Paed Annual Review Panel
Sits for
2026 S2
Evidence window
2026-01-01 to 2026-12-31
Type
Formative check-in
State
Closed
### Decisions
No binding decision. A formative check-in records no decision and issues no STAR.
His annual review, Outcome Deferred (4.51)Sipho Ndlovu · /committee/my-reviews/5 · Ratified, so the lodge block is offeredR3
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
[title: Sipho Ndlovu] Sipho Ndlovu
Sign out
[aria-label: Breadcrumb]
Home
My committee reviews
2026 S2 annual progression review
## 2026 S2 annual progression review
Paed Annual Review Panel
[aria-label: About this review]
Panel
Paed Annual Review Panel
Sits for
2026 S2
Evidence window
2026-01-01 to 2026-12-31
Type
Annual progression review
State
Ratified
### Decisions
#### Outcome Deferred
One completed Mini-CEX and one reflection in his first semester; to be judged at the next sitting.
About the EPAs not decided: Too early in the programme to decide.
Present: Thandi Zulu (chair), Sarah Botha
### Appeal
#### What happens next
Your appeal is heard by the panel's appeal body: today, Thandi Zulu (chair) and John van Rensburg (external).
They dismiss it, and the decision stands, or remit it, and take a new decision in its place. Either outcome is final: nothing reopens the review, and it cannot be appealed again.
Who reads your reason: everyone who can open this review on the committee's side: the panel's members, the coordinators and institutional administrators of Kgosi Kgari Teaching Hospital, your programme's speciality and sub-speciality administrators, and Wombat's system administrators.
Why you are appealing *required
Up to 4,000 characters.
0 of 4,000 characters
Lodge appeal
### Agenda
The EPAs this review was there to decide for 2026 S2, and what the committee did with each.
| EPA
| Window
| Outcome
| PAED-001Providing paediatric emergency care to children
| 2026 S2
| Not decided at this sittingToo early in the programme to decide.
| PAED-002Managing common paediatric presentations
| 2026 S2
| Not decided at this sittingToo early in the programme to decide.
| PAED-003Providing intensive care to children
| 2026
| Not decided at this sittingToo early in the programme to decide.
… 12 more lines, as drawn in full on the 4.41 frame

######## BOARD R3-2-review-390-b.dc.html — Her review · 390 · b
## Her review page · 390px · b: lodging
Route /committee/my-reviews/{id} (decided 2026-10-14), owned by My committee reviews: its nav item lit, its trail Home › My committee reviews › the h1; tab “<h1> · Wombat”. Only the review's own trainee, once it is ratified or later; anything else, every role, is flow 01's not found (F8).
Order One column at every width: header (its facts), Decisions, Appeal, Agenda.
Components PageHeader, Breadcrumbs, DetailCard, .detail-list (the facts), AgendaTable (read-only), Badge, Alert, ActionResult, ConfirmDialog (action mode, as built), StatePanel, Skeleton, FormField. NEW the page (.my-review-page on flow 07's .review-page); DecisionHistory, lifted; AppealEntry; the lodge block.
DESIGN.md § Detail page and the title rule; the owner table's row “A trainee's review | Trainee: My committee reviews”; § Entrustment-only reviews, “The trainee's reviews”. Round 3 F6, F8, M3, S1, S5, S9, S13, nits.
Before lodging (4.43)Nomsa Mahlangu · /committee/my-reviews/4 · the decision with its rationale and who was present (C2); “What happens next” (C8): the appeal body's names are PROPOSED data (the appeal body read for a Ratified review, a build change); the readers named trulyFocus: the h1PROPOSEDNEW
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
[title: Nomsa Mahlangu] Nomsa Mahlangu
Sign out
[aria-label: Breadcrumb]
Home
My committee reviews
2026 S2 annual progression review
## 2026 S2 annual progression review
Paed Annual Review Panel
[aria-label: About this review]
Panel
Paed Annual Review Panel
Sits for
2026 S2
Evidence window
2026-01-01 to 2026-12-31
Type
Annual progression review
State
Ratified
### Decisions
#### Inadequate Progress — Additional Training
One DOPS in the year and a Mini-CEX that stalled; a supervised support plan and re-review in six months.
About the EPAs not decided: Not enough observed evidence yet this year.
Present: Thandi Zulu (chair), Sarah Botha, John van Rensburg (external)
### Appeal
#### What happens next
Your appeal is heard by the panel's appeal body: today, Thandi Zulu (chair) and John van Rensburg (external).
They dismiss it, and the decision stands, or remit it, and take a new decision in its place. Either outcome is final: nothing reopens the review, and it cannot be appealed again.
Who reads your reason: everyone who can open this review on the committee's side: the panel's members, the coordinators and institutional administrators of Kgosi Kgari Teaching Hospital, your programme's speciality and sub-speciality administrators, and Wombat's system administrators.
Why you are appealing *required
Up to 4,000 characters.
0 of 4,000 characters
Lodge appeal
### Agenda
The EPAs this review was there to decide for 2026 S2, and what the committee did with each.
| EPA
| Window
| Outcome
| PAED-001Providing paediatric emergency care to children
| 2026 S2
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-002Managing common paediatric presentations
| 2026 S2
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-003Providing intensive care to children
| 2026
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-004Managing common neonatal conditions
| 2026 S2
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-005Providing neonatal care in intensive and high-care settings
| 2026 S2
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-006Managing long-term health conditions (LTHCs)
| 2026
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-007Maintaining and promoting the health and well-being of children
| 2026
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-008Evaluating and managing neurodevelopmental and behavioural presentations in children
| 2026
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-009Providing care for an adolescent patient
| 2026
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-010Leading and operating within a clinical team
| 2026 S2
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-011Managing population health challenges
| 2026
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-012Communicating with and counselling patients, caregivers and healthcare teams
| 2026 S2
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-013Managing child protection, legal and ethical aspects of children at risk
| 2026
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-014Providing palliative and end-of-life care
| 2026
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-015Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
| 2026
| Not decided at this sittingNot enough observed evidence yet this year.
Lodge appeal with the reason blankNothing is sent; “Give your reason for appealing.” under the field (T324); the field marked with its danger edge and stripe; the counter a second .field-help (S9)Focus: the fieldNEW
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
[title: Nomsa Mahlangu] Nomsa Mahlangu
Sign out
[aria-label: Breadcrumb]
Home
My committee reviews
2026 S2 annual progression review
## 2026 S2 annual progression review
Paed Annual Review Panel
[aria-label: About this review]
Panel
Paed Annual Review Panel
Sits for
2026 S2
Evidence window
2026-01-01 to 2026-12-31
Type
Annual progression review
State
Ratified
### Decisions
#### Inadequate Progress — Additional Training
One DOPS in the year and a Mini-CEX that stalled; a supervised support plan and re-review in six months.
About the EPAs not decided: Not enough observed evidence yet this year.
Present: Thandi Zulu (chair), Sarah Botha, John van Rensburg (external)
### Appeal
#### What happens next
Your appeal is heard by the panel's appeal body: today, Thandi Zulu (chair) and John van Rensburg (external).
They dismiss it, and the decision stands, or remit it, and take a new decision in its place. Either outcome is final: nothing reopens the review, and it cannot be appealed again.
Who reads your reason: everyone who can open this review on the committee's side: the panel's members, the coordinators and institutional administrators of Kgosi Kgari Teaching Hospital, your programme's speciality and sub-speciality administrators, and Wombat's system administrators.
Why you are appealing *required
Up to 4,000 characters.
Give your reason for appealing.
0 of 4,000 characters
Lodge appeal
### Agenda
The EPAs this review was there to decide for 2026 S2, and what the committee did with each.
| EPA
| Window
| Outcome
| PAED-001Providing paediatric emergency care to children
| 2026 S2
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-002Managing common paediatric presentations
| 2026 S2
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-003Providing intensive care to children
| 2026
| Not decided at this sittingNot enough observed evidence yet this year.
… 12 more lines, as drawn in full on the 4.41 frame
Lodge appeal pressed: the dialog (C14a)ConfirmDialog, action mode, as built · the safe button “Keep editing” has the focusFocus: Keep editingPROPOSED
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
[title: Nomsa Mahlangu] Nomsa Mahlangu
Sign out
[aria-label: Breadcrumb]
Home
My committee reviews
2026 S2 annual progression review
## 2026 S2 annual progression review
Paed Annual Review Panel
[aria-label: About this review]
Panel
Paed Annual Review Panel
Sits for
2026 S2
Evidence window
2026-01-01 to 2026-12-31
Type
Annual progression review
State
Ratified
### Decisions
#### Inadequate Progress — Additional Training
One DOPS in the year and a Mini-CEX that stalled; a supervised support plan and re-review in six months.
About the EPAs not decided: Not enough observed evidence yet this year.
Present: Thandi Zulu (chair), Sarah Botha, John van Rensburg (external)
### Appeal
#### What happens next
Your appeal is heard by the panel's appeal body: today, Thandi Zulu (chair) and John van Rensburg (external).
They dismiss it, and the decision stands, or remit it, and take a new decision in its place. Either outcome is final: nothing reopens the review, and it cannot be appealed again.
Who reads your reason: everyone who can open this review on the committee's side: the panel's members, the coordinators and institutional administrators of Kgosi Kgari Teaching Hospital, your programme's speciality and sub-speciality administrators, and Wombat's system administrators.
Why you are appealing *required
The single DOPS reflects the start of the year, and the Mini-CEX waited eight days on the assessor's side before it was rated. I ask for reconsideration.
Up to 4,000 characters.
153 of 4,000 characters
Lodge appeal
### Agenda
The EPAs this review was there to decide for 2026 S2, and what the committee did with each.
| EPA
| Window
| Outcome
| PAED-001Providing paediatric emergency care to children
| 2026 S2
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-002Managing common paediatric presentations
| 2026 S2
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-003Providing intensive care to children
| 2026
| Not decided at this sittingNot enough observed evidence yet this year.
… 12 more lines, as drawn in full on the 4.41 frame
### Lodge your appeal?
Against the 2026 S2 annual progression review's decision, Inadequate Progress — Additional Training.
Your appeal is heard by the panel's appeal body: today, Thandi Zulu (chair) and John van Rensburg (external). Either outcome is final.
Who reads your reason: everyone who can open this review on the committee's side: the panel's members, the coordinators and institutional administrators of Kgosi Kgari Teaching Hospital, your programme's speciality and sub-speciality administrators, and Wombat's system administrators.
Keep editing
Lodge appeal
Keep editing (F6)The dialog closes as the native dialog does; the reason is kept (153 of 4,000)Focus: back on Lodge appeal, the button that opened itR3
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
[title: Nomsa Mahlangu] Nomsa Mahlangu
Sign out
[aria-label: Breadcrumb]
Home
My committee reviews
2026 S2 annual progression review
## 2026 S2 annual progression review
Paed Annual Review Panel
[aria-label: About this review]
Panel
Paed Annual Review Panel
Sits for
2026 S2
Evidence window
2026-01-01 to 2026-12-31
Type
Annual progression review
State
Ratified
### Decisions
#### Inadequate Progress — Additional Training
One DOPS in the year and a Mini-CEX that stalled; a supervised support plan and re-review in six months.
About the EPAs not decided: Not enough observed evidence yet this year.
Present: Thandi Zulu (chair), Sarah Botha, John van Rensburg (external)
### Appeal
#### What happens next
Your appeal is heard by the panel's appeal body: today, Thandi Zulu (chair) and John van Rensburg (external).
They dismiss it, and the decision stands, or remit it, and take a new decision in its place. Either outcome is final: nothing reopens the review, and it cannot be appealed again.
Who reads your reason: everyone who can open this review on the committee's side: the panel's members, the coordinators and institutional administrators of Kgosi Kgari Teaching Hospital, your programme's speciality and sub-speciality administrators, and Wombat's system administrators.
Why you are appealing *required
The single DOPS reflects the start of the year, and the Mini-CEX waited eight days on the assessor's side before it was rated. I ask for reconsideration.
Up to 4,000 characters.
153 of 4,000 characters
Lodge appeal
### Agenda
The EPAs this review was there to decide for 2026 S2, and what the committee did with each.
| EPA
| Window
| Outcome
| PAED-001Providing paediatric emergency care to children
| 2026 S2
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-002Managing common paediatric presentations
| 2026 S2
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-003Providing intensive care to children
| 2026
| Not decided at this sittingNot enough observed evidence yet this year.
… 12 more lines, as drawn in full on the 4.41 frame
Lodged (4.43)“Appeal lodged.” heads her Appeal section; State reads Under appeal at once; her entry Open, Heard by the appeal body (T308); the form is gone (one open appeal at a time)Focus: the resultNEW
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
[title: Nomsa Mahlangu] Nomsa Mahlangu
Sign out
[aria-label: Breadcrumb]
Home
My committee reviews
2026 S2 annual progression review
## 2026 S2 annual progression review
Paed Annual Review Panel
[aria-label: About this review]
Panel
Paed Annual Review Panel
Sits for
2026 S2
Evidence window
2026-01-01 to 2026-12-31
Type
Annual progression review
State
Under appeal
### Decisions
#### Inadequate Progress — Additional Training
One DOPS in the year and a Mini-CEX that stalled; a supervised support plan and re-review in six months.
About the EPAs not decided: Not enough observed evidence yet this year.
Present: Thandi Zulu (chair), Sarah Botha, John van Rensburg (external)
### Appeal
Appeal lodged.
#### Your appeal Open
Lodged 2026-10-13
Heard by Thandi Zulu (chair) and John van Rensburg (external).
Reason
The single DOPS reflects the start of the year, and the Mini-CEX waited eight days on the assessor's side before it was rated. I ask for reconsideration.
### Agenda
The EPAs this review was there to decide for 2026 S2, and what the committee did with each.
| EPA
| Window
| Outcome
| PAED-001Providing paediatric emergency care to children
| 2026 S2
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-002Managing common paediatric presentations
| 2026 S2
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-003Providing intensive care to children
| 2026
| Not decided at this sittingNot enough observed evidence yet this year.
… 12 more lines, as drawn in full on the 4.41 frame
A lodge refused (M3)Scratch state: a second tab lodged first, so this review is Under appeal · Lodge appeal is refused with the domain's words, under the button; the reason keptFocus: the refusalR3
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
[title: Nomsa Mahlangu] Nomsa Mahlangu
Sign out
[aria-label: Breadcrumb]
Home
My committee reviews
2026 S2 annual progression review
## 2026 S2 annual progression review
Paed Annual Review Panel
[aria-label: About this review]
Panel
Paed Annual Review Panel
Sits for
2026 S2
Evidence window
2026-01-01 to 2026-12-31
Type
Annual progression review
State
Ratified
### Decisions
#### Inadequate Progress — Additional Training
One DOPS in the year and a Mini-CEX that stalled; a supervised support plan and re-review in six months.
About the EPAs not decided: Not enough observed evidence yet this year.
Present: Thandi Zulu (chair), Sarah Botha, John van Rensburg (external)
### Appeal
#### What happens next
Your appeal is heard by the panel's appeal body: today, Thandi Zulu (chair) and John van Rensburg (external).
They dismiss it, and the decision stands, or remit it, and take a new decision in its place. Either outcome is final: nothing reopens the review, and it cannot be appealed again.
Who reads your reason: everyone who can open this review on the committee's side: the panel's members, the coordinators and institutional administrators of Kgosi Kgari Teaching Hospital, your programme's speciality and sub-speciality administrators, and Wombat's system administrators.
Why you are appealing *required
The single DOPS reflects the start of the year, and the Mini-CEX waited eight days on the assessor's side before it was rated. I ask for reconsideration.
Up to 4,000 characters.
153 of 4,000 characters
Lodge appeal
Only ratified reviews can be appealed.
### Agenda
The EPAs this review was there to decide for 2026 S2, and what the committee did with each.
| EPA
| Window
| Outcome
| PAED-001Providing paediatric emergency care to children
| 2026 S2
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-002Managing common paediatric presentations
| 2026 S2
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-003Providing intensive care to children
| 2026
| Not decided at this sittingNot enough observed evidence yet this year.
… 12 more lines, as drawn in full on the 4.41 frame
Then the page reads the review again (S5)The form is gone; State Under appeal; the open entry from the other tab; the refusal still shown at the head of her Appeal section, with the focusR3
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
[title: Nomsa Mahlangu] Nomsa Mahlangu
Sign out
[aria-label: Breadcrumb]
Home
My committee reviews
2026 S2 annual progression review
## 2026 S2 annual progression review
Paed Annual Review Panel
[aria-label: About this review]
Panel
Paed Annual Review Panel
Sits for
2026 S2
Evidence window
2026-01-01 to 2026-12-31
Type
Annual progression review
State
Under appeal
### Decisions
#### Inadequate Progress — Additional Training
One DOPS in the year and a Mini-CEX that stalled; a supervised support plan and re-review in six months.
About the EPAs not decided: Not enough observed evidence yet this year.
Present: Thandi Zulu (chair), Sarah Botha, John van Rensburg (external)
### Appeal
Only ratified reviews can be appealed.
#### Your appeal Open
Lodged 2026-10-13
Heard by Thandi Zulu (chair) and John van Rensburg (external).
Reason
The single DOPS reflects the start of the year, and the Mini-CEX waited eight days on the assessor's side before it was rated. I ask for reconsideration.
### Agenda
The EPAs this review was there to decide for 2026 S2, and what the committee did with each.
| EPA
| Window
| Outcome
| PAED-001Providing paediatric emergency care to children
| 2026 S2
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-002Managing common paediatric presentations
| 2026 S2
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-003Providing intensive care to children
| 2026
| Not decided at this sittingNot enough observed evidence yet this year.
… 12 more lines, as drawn in full on the 4.41 frame

######## BOARD R3-2-review-390-c.dc.html — Her review · 390 · c
## Her review page · 390px · c: outcomes and states
Route /committee/my-reviews/{id} (decided 2026-10-14), owned by My committee reviews: its nav item lit, its trail Home › My committee reviews › the h1; tab “<h1> · Wombat”. Only the review's own trainee, once it is ratified or later; anything else, every role, is flow 01's not found (F8).
Order One column at every width: header (its facts), Decisions, Appeal, Agenda.
Components PageHeader, Breadcrumbs, DetailCard, .detail-list (the facts), AgendaTable (read-only), Badge, Alert, ActionResult, ConfirmDialog (action mode, as built), StatePanel, Skeleton, FormField. NEW the page (.my-review-page on flow 07's .review-page); DecisionHistory, lifted; AppealEntry; the lodge block.
DESIGN.md § Detail page and the title rule; the owner table's row “A trainee's review | Trainee: My committee reviews”; § Entrustment-only reviews, “The trainee's reviews”. Round 3 F6, F8, M3, S1, S5, S9, S13, nits.
Closed after a remit (4.48)Nomsa Mahlangu · two decisions, newest first, the first marked “Replaced on appeal by the decision above.” with its rationale and who sat (T308); her entry Remitted, “Resolved 2026-10-13 by Thandi Zulu” (C15); no lodge formFocus: the h1NEW
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
[title: Nomsa Mahlangu] Nomsa Mahlangu
Sign out
[aria-label: Breadcrumb]
Home
My committee reviews
2026 S2 annual progression review
## 2026 S2 annual progression review
Paed Annual Review Panel
[aria-label: About this review]
Panel
Paed Annual Review Panel
Sits for
2026 S2
Evidence window
2026-01-01 to 2026-12-31
Type
Annual progression review
State
Closed
### Decisions
#### Satisfactory with Observations
The stalled Mini-CEX was the assessor's delay; progress is adequate, with observed assessments to follow.
About the EPAs not decided: Not enough observed evidence yet this year.
Conditions: Two observed Mini-CEX and one DOPS before the next review.
Present: Thandi Zulu (chair), Sarah Botha
#### Inadequate Progress — Additional Training
Replaced on appeal by the decision above.
One DOPS in the year and a Mini-CEX that stalled; a supervised support plan and re-review in six months.
About the EPAs not decided: Not enough observed evidence yet this year.
Present: Thandi Zulu (chair), Sarah Botha, John van Rensburg (external)
### Appeal
#### Your appeal Remitted
Lodged 2026-10-13 · Resolved 2026-10-13 by Thandi Zulu
Remitted: the appeal body replaced the decision. The decision above is theirs, and it is final.
Reason
The single DOPS reflects the start of the year, and the Mini-CEX waited eight days on the assessor's side before it was rated. I ask for reconsideration.
This review is closed and cannot be appealed again.
### Agenda
The EPAs this review was there to decide for 2026 S2, and what the committee did with each.
| EPA
| Window
| Outcome
| PAED-001Providing paediatric emergency care to children
| 2026 S2
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-002Managing common paediatric presentations
| 2026 S2
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-003Providing intensive care to children
| 2026
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-004Managing common neonatal conditions
| 2026 S2
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-005Providing neonatal care in intensive and high-care settings
| 2026 S2
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-006Managing long-term health conditions (LTHCs)
| 2026
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-007Maintaining and promoting the health and well-being of children
| 2026
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-008Evaluating and managing neurodevelopmental and behavioural presentations in children
| 2026
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-009Providing care for an adolescent patient
| 2026
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-010Leading and operating within a clinical team
| 2026 S2
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-011Managing population health challenges
| 2026
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-012Communicating with and counselling patients, caregivers and healthcare teams
| 2026 S2
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-013Managing child protection, legal and ethical aspects of children at risk
| 2026
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-014Providing palliative and end-of-life care
| 2026
| Not decided at this sittingNot enough observed evidence yet this year.
| PAED-015Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
| 2026
| Not decided at this sittingNot enough observed evidence yet this year.
Closed after a dismissalScratch database (states/review-detail--appeal-dismissed): Anele Dlamini appealed review #2 on 2026-10-12; Thandi Zulu dismissed it · the decision stands, alone in the history
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
[aria-label: Breadcrumb]
Home
My committee reviews
2026 S2 annual progression review
## 2026 S2 annual progression review
Paed Annual Review Panel
[aria-label: About this review]
Panel
Paed Annual Review Panel
Sits for
2026 S2
Evidence window
2026-01-01 to 2026-12-31
Type
Annual progression review
State
Closed
### Decisions
#### Satisfactory Progress
Steady progress in year 3; two EPAs entrusted at this sitting.
About the EPAs not decided: Not at a decision point this year.
Present: Thandi Zulu (chair), David Naidoo, Sarah Botha
### Appeal
#### Your appeal Dismissed
Lodged 2026-10-12 · Resolved 2026-10-13 by Thandi Zulu
Dismissed: the decision stands, and it is final.
Reason
The PAED-002 decision rested on PAED-001 evidence only. I ask for reconsideration.
This review is closed and cannot be appealed again.
### Agenda
The EPAs this review was there to decide for 2026 S2, and what the committee did with each.
| EPA
| Window
| Outcome
| PAED-001Providing paediatric emergency care to children
| 2026 S2
| DecidedSTAR #4.
| PAED-002Managing common paediatric presentations
| 2026 S2
| DecidedSTAR #5.
… 13 more lines, as on the 4.42 frame
Illustration: an entrustment-only review, remitted (C17)No cast name: no step appeals one · h1 “2026 S1 entrustment-only review”; the two sentences under the current decision's heading; the replaced decision's h3 says so; no category; the STARs it issued stand
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
[title: ‹trainee›] ‹trainee›
Sign out
[aria-label: Breadcrumb]
Home
My committee reviews
2026 S1 entrustment-only review
## 2026 S1 entrustment-only review
Paed Annual Review Panel
[aria-label: About this review]
Panel
Paed Annual Review Panel
Sits for
2026 S2
Evidence window
2026-01-01 to 2026-06-30
Type
Entrustment-only review
State
Closed
### Decisions
#### Entrustment decisions only
This review decided entrustment only, so it records no progression outcome.
The appeal was remitted: this decision is the appeal body's, and the entrustment decisions this review issued stand unless one is revoked.
What it decided on each EPA is on its agenda below.
‹the appeal body's rationale›
Present: ‹chair› (chair), ‹member›
#### Entrustment decisions only, replaced on appeal
This review decided entrustment only, so it records no progression outcome.
Replaced on appeal by the decision above.
‹the committee's rationale›
Present: ‹chair› (chair), ‹members›
### Appeal
#### Your appeal Remitted
Lodged ‹date› · Resolved ‹date› by ‹resolver›
Remitted: the appeal body replaced the decision. The decision above is theirs, and it is final.
Reason
‹her reason›
### Agenda
The EPAs this review was there to decide for 2026 S1, and what the committee did with each.
| EPA
| Window
| Outcome
| PAED-004Managing common neonatal conditions
| 2026 S1
| DecidedSTAR #‹n›.
| PAED-005Providing neonatal care in intensive and high-care settings
| 2026 S1
| DecidedSTAR #‹n›.
Not found (Q1, F8)Lerato Molefe on /committee/my-reviews/4 (Nomsa Mahlangu's), or on one of hers not yet ratified: flow 01's Page not found, word for word; nothing lit, no trail. The same for every role, the Administrator included: she reads a review at /committee/reviews/{id}R3
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
## Page not found
There is no page at this address.
Check the address, or start again from Home.
Go to Home
LoadingThe header says “Committee review” until the read is in (as flow 07's page does, S12); a status line; skeletons in the page's shape; no lodge form
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
My committee reviews
Committee review
## Committee review
Loading your review.
Load error (T329)Flow 07's fixed sentence and Try again; the header stays “Committee review”Focus: the alert
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
My committee reviews
Committee review
## Committee review
The review could not be loaded. Nothing on it has changed.
Try again
Access denied: the committee's address (4.41, C7)Lerato Molefe types /committee/reviews/1 · flow 01's page, unchanged: nothing lit, no trail, Go to Home
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
## You cannot open this page
Your role (Trainee) does not open this page.
If you need it for your work, ask your institution's Wombat administrator.
Go to Home

######## BOARD R3-3-order-1280.dc.html — Appeals · the page's order · 1280
## The committee's review page: order and the jump (C9, F5) · 1280px
Route /committee/reviews/{id} · flow 07's page, unchanged above its Appeals section and its decision history, but for F5's button in a reader's bar while Under appeal.
Components ReviewSitting, SittingBar (reader; .sitting-bar-actions), DecisionHistory (lifted, shared), AgendaTable, EntrustmentStandingPanel, EvidenceReader, TrajectoryChart: flow 07's.
DESIGN.md § Detail page, “The committee review has no details-grid”: adds the jump while Under appeal. Round 3 F5.
Flow 07's page, top to bottom, while Under appeal (C9, F5) · 1280David Naidoo · the Appeals section is last, so “The replacement decision is above” stays true · “Go to the appeal”, an outline small button in the bar's actions, at its right end on the bar's one line; it saves about 7,800 px of scrollR3
Wombat
Acting as Committee member
MenuClose
[aria-label: Main]
Acting as Committee member
Switch to Assessor
Home
Programme trainees
Committee reviews
Decision panels
My data rights
[title: David Naidoo] David Naidoo
Sign out
[aria-label: Breadcrumb]
Home
Committee reviews
Nomsa Mahlangu · 2026 S2
## Nomsa Mahlangu · 2026 S2
Annual progression review · Paed Annual Review Panel · Training year 1
Nomsa Mahlangu's progress
Under appealInadequate Progress — Additional Training · no STAR issued
Go to the appeal
### Decision
#### Inadequate Progress — Additional Training
One DOPS in the year and a Mini-CEX that stalled; a supervised support plan and re-review in six months.
About the EPAs not decided: Not enough observed evidence yet this year.
Present: Thandi Zulu (chair), Sarah Botha, John van Rensburg (external)
1. Header · Breadcrumbs Home › Committee reviews › Nomsa Mahlangu · 2026 S2; h1; “Annual progression review · Paed Annual Review Panel · Training year 1”; Nomsa Mahlangu's progress
2. Sitting bar · Under appeal · Inadequate Progress — Additional Training · no STAR issued · Go to the appeal (F5)
3. Before the committee decides · Sampling concentration warnings (PAED-002, PAED-004)
4. Decision · DecisionHistory, newest first: the replacement heads it once remitted
5. Agenda · 15 lines, each “Not decided at this sitting”
6. Entrustment against Annexure A · Standing, 15 EPAs
7. Multi-source feedback by EPA · MSF coverage
8. Evidence snapshot · The frozen items, each with Read
9. Rating trajectories · Two charts an EPA, with their tables
10. Appeals · The last section, about 7,800 px below the top at 1280: the entry, then the form or the who-may line
### Appeals
#### Appeal by Nomsa Mahlangu Open
Lodged 2026-10-13
Heard by Thandi Zulu (chair) and John van Rensburg (external).
Reason
The single DOPS reflects the start of the year, and the Mini-CEX waited eight days on the assessor's side before it was rated. I ask for reconsideration.
Only the appeal body can resolve the appeal: the panel's chair, Thandi Zulu, and its external member, John van Rensburg.
Go to the appeal pressed (F5) · 1280PageFocus moves the focus to the Appeals h2 (tabindex −1), not a bare #appeals link; the browser brings it into view under the bars (flow 07's scroll-margin)Focus: the Appeals h2R3
Wombat
Acting as Committee member
MenuClose
[aria-label: Main]
Acting as Committee member
Switch to Assessor
Home
Programme trainees
Committee reviews
Decision panels
My data rights
[title: David Naidoo] David Naidoo
Sign out
[aria-label: Breadcrumb]
Home
Committee reviews
Nomsa Mahlangu · 2026 S2
## Nomsa Mahlangu · 2026 S2
Annual progression review · Paed Annual Review Panel · Training year 1
Nomsa Mahlangu's progress
Under appealInadequate Progress — Additional Training · no STAR issued
Go to the appeal
… the page above, scrolled past …
### Appeals
#### Appeal by Nomsa Mahlangu Open
Lodged 2026-10-13
Heard by Thandi Zulu (chair) and John van Rensburg (external).
Reason
The single DOPS reflects the start of the year, and the Mini-CEX waited eight days on the assessor's side before it was rated. I ask for reconsideration.
Only the appeal body can resolve the appeal: the panel's chair, Thandi Zulu, and its external member, John van Rensburg.

######## BOARD R3-3-order-390.dc.html — Appeals · the page's order · 390
## The committee's review page: order and the jump (C9, F5) · 390px
Route /committee/reviews/{id} · flow 07's page, unchanged above its Appeals section and its decision history, but for F5's button in a reader's bar while Under appeal.
Components ReviewSitting, SittingBar (reader; .sitting-bar-actions), DecisionHistory (lifted, shared), AgendaTable, EntrustmentStandingPanel, EvidenceReader, TrajectoryChart: flow 07's.
DESIGN.md § Detail page, “The committee review has no details-grid”: adds the jump while Under appeal. Round 3 F5.
Flow 07's page, top to bottom, while Under appeal (C9, F5) · 390David Naidoo · the Appeals section is last, so “The replacement decision is above” stays true · “Go to the appeal”, an outline small button under the state line, the bar stacked: full width, 44px (flow 07's .sitting-bar-actions below 641px); it saves about 18,000 px (estimated from the sections' stacked heights; the A.7 replay measures it) of scrollR3
Wombat
Acting as Committee member
MenuClose
[aria-label: Main]
Acting as Committee member
Switch to Assessor
Home
Programme trainees
Committee reviews
Decision panels
My data rights
[title: David Naidoo] David Naidoo
Sign out
[aria-label: Breadcrumb]
Home
Committee reviews
Nomsa Mahlangu · 2026 S2
## Nomsa Mahlangu · 2026 S2
Annual progression review · Paed Annual Review Panel · Training year 1
Nomsa Mahlangu's progress
Under appealInadequate Progress — Additional Training · no STAR issued
Go to the appeal
### Decision
#### Inadequate Progress — Additional Training
One DOPS in the year and a Mini-CEX that stalled; a supervised support plan and re-review in six months.
About the EPAs not decided: Not enough observed evidence yet this year.
Present: Thandi Zulu (chair), Sarah Botha, John van Rensburg (external)
1. Header · Breadcrumbs Home › Committee reviews › Nomsa Mahlangu · 2026 S2; h1; “Annual progression review · Paed Annual Review Panel · Training year 1”; Nomsa Mahlangu's progress
2. Sitting bar · Under appeal · Inadequate Progress — Additional Training · no STAR issued · Go to the appeal (F5)
3. Before the committee decides · Sampling concentration warnings (PAED-002, PAED-004)
4. Decision · DecisionHistory, newest first: the replacement heads it once remitted
5. Agenda · 15 lines, each “Not decided at this sitting”
6. Entrustment against Annexure A · Standing, 15 EPAs
7. Multi-source feedback by EPA · MSF coverage
8. Evidence snapshot · The frozen items, each with Read
9. Rating trajectories · Two charts an EPA, with their tables
10. Appeals · The last section, about 18,000 px (estimated from the sections' stacked heights; the A.7 replay measures it) below the top at 390: the entry, then the form or the who-may line
### Appeals
#### Appeal by Nomsa Mahlangu Open
Lodged 2026-10-13
Heard by Thandi Zulu (chair) and John van Rensburg (external).
Reason
The single DOPS reflects the start of the year, and the Mini-CEX waited eight days on the assessor's side before it was rated. I ask for reconsideration.
Only the appeal body can resolve the appeal: the panel's chair, Thandi Zulu, and its external member, John van Rensburg.
Go to the appeal pressed (F5) · 390PageFocus moves the focus to the Appeals h2 (tabindex −1), not a bare #appeals link; the browser brings it into view under the bars (flow 07's scroll-margin)Focus: the Appeals h2R3
Wombat
Acting as Committee member
MenuClose
[aria-label: Main]
Acting as Committee member
Switch to Assessor
Home
Programme trainees
Committee reviews
Decision panels
My data rights
[title: David Naidoo] David Naidoo
Sign out
[aria-label: Breadcrumb]
Home
Committee reviews
Nomsa Mahlangu · 2026 S2
## Nomsa Mahlangu · 2026 S2
Annual progression review · Paed Annual Review Panel · Training year 1
Nomsa Mahlangu's progress
Under appealInadequate Progress — Additional Training · no STAR issued
Go to the appeal
… the page above, scrolled past …
### Appeals
#### Appeal by Nomsa Mahlangu Open
Lodged 2026-10-13
Heard by Thandi Zulu (chair) and John van Rensburg (external).
Reason
The single DOPS reflects the start of the year, and the Mini-CEX waited eight days on the assessor's side before it was rated. I ask for reconsideration.
Only the appeal body can resolve the appeal: the panel's chair, Thandi Zulu, and its external member, John van Rensburg.

######## BOARD R3-3-appeals-1280-a.dc.html — Appeals · 1280 · a
## The Appeals section · 1280px · a: reading and the form
Route /committee/reviews/{id}#appeals · the Appeals section; above it flow 07's page, cut (the order boards draw it whole).
Components ReviewSitting's Appeals section, FormField, Badge, Alert, ActionResult, ConfirmDialog (action mode, as built), .decision-actions and p.quorum-line. CHANGED (M8): the quorum line and a refusal after the actions on the appeal form. NEW AppealEntry; .resolve-form; DecisionHistory lifted.
DESIGN.md § Entrustment-only reviews (the appeal's Outcome; the remit's note): unchanged words; § Form system: the resolve form one column. Round 3 F5, F6, F7, M2, M8, nits.
A member's view (4.44)David Naidoo · the entry, Open, “Heard by Thandi Zulu (chair) and John van Rensburg (external).”; the who-may line; no form (C10); Go to the appeal in the barNEW
Wombat
Acting as Committee member
MenuClose
[aria-label: Main]
Acting as Committee member
Switch to Assessor
Home
Programme trainees
Committee reviews
Decision panels
My data rights
[title: David Naidoo] David Naidoo
Sign out
[aria-label: Breadcrumb]
Home
Committee reviews
Nomsa Mahlangu · 2026 S2
## Nomsa Mahlangu · 2026 S2
Annual progression review · Paed Annual Review Panel · Training year 1
Nomsa Mahlangu's progress
Under appealInadequate Progress — Additional Training · no STAR issued
Go to the appeal
### Decision
#### Inadequate Progress — Additional Training
One DOPS in the year and a Mini-CEX that stalled; a supervised support plan and re-review in six months.
About the EPAs not decided: Not enough observed evidence yet this year.
Present: Thandi Zulu (chair), Sarah Botha, John van Rensburg (external)
Before the committee decides, Decision, Agenda (15 lines), Entrustment against Annexure A, Multi-source feedback by EPA, Evidence snapshot and Rating trajectories: flow 07's, unchanged; the order board draws them. About 7,000 px.
### Appeals
#### Appeal by Nomsa Mahlangu Open
Lodged 2026-10-13
Heard by Thandi Zulu (chair) and John van Rensburg (external).
Reason
The single DOPS reflects the start of the year, and the Mini-CEX waited eight days on the assessor's side before it was rated. I ask for reconsideration.
Only the appeal body can resolve the appeal: the panel's chair, Thandi Zulu, and its external member, John van Rensburg.
An appeal body member who cannot act now (C13, F7, M2)Scratch state: ‹name›, a Coordinator at Kgosi Kgari Teaching Hospital who also holds the panel's External seat and cannot act now · reads the page as a Coordinator (the Coordinator's menu); “Heard by” and the who-may line leave them out. A lapsed member with no other rung cannot open the review (T279)R3
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Switch to Committee member
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: ‹name›] ‹name›
Sign out
[aria-label: Breadcrumb]
Home
Committee reviews
Nomsa Mahlangu · 2026 S2
## Nomsa Mahlangu · 2026 S2
Annual progression review · Paed Annual Review Panel · Training year 1
Nomsa Mahlangu's progress
Under appealInadequate Progress — Additional Training · no STAR issued
Go to the appeal
Before the committee decides, Decision, Agenda (15 lines), Entrustment against Annexure A, Multi-source feedback by EPA, Evidence snapshot and Rating trajectories: flow 07's, unchanged; the order board draws them. About 7,000 px.
### Appeals
#### Appeal by Nomsa Mahlangu Open
Lodged 2026-10-13
Heard by Thandi Zulu (chair).
Reason
The single DOPS reflects the start of the year, and the Mini-CEX waited eight days on the assessor's side before it was rated. I ask for reconsideration.
Only the appeal body can resolve the appeal: the panel's chair, Thandi Zulu.
The appeal body's form (4.45)John van Rensburg · inline (C10): only to a member of the appeal body who can act now, only while Under appeal · Outcome opens on “Select an outcome…”
Wombat
Acting as Committee member
MenuClose
[aria-label: Main]
Acting as Committee member
Home
Programme trainees
Committee reviews
Decision panels
My data rights
[title: John van Rensburg] John van Rensburg
Sign out
[aria-label: Breadcrumb]
Home
Committee reviews
Nomsa Mahlangu · 2026 S2
## Nomsa Mahlangu · 2026 S2
Annual progression review · Paed Annual Review Panel · Training year 1
Nomsa Mahlangu's progress
Under appealInadequate Progress — Additional Training · no STAR issued
Go to the appeal
Before the committee decides, Decision, Agenda (15 lines), Entrustment against Annexure A, Multi-source feedback by EPA, Evidence snapshot and Rating trajectories: flow 07's, unchanged; the order board draws them. About 7,000 px.
### Appeals
#### Appeal by Nomsa Mahlangu Open
Lodged 2026-10-13
Heard by Thandi Zulu (chair) and John van Rensburg (external).
Reason
The single DOPS reflects the start of the year, and the Mini-CEX waited eight days on the assessor's side before it was rated. I ask for reconsideration.
[aria-label: Resolve the appeal]
Outcome *required
Select an outcome…
Dismissed: the decision stands
Remitted: the appeal body replaces the decision
Dismissed leaves the committee's decision in force. Remitted replaces it with a decision the appeal body takes now, recorded below with who sat for it. Either closes the review, and nothing reopens it.
Resolve appeal
Resolve appeal with no outcome (4.45)Nothing is sent; “Choose an outcome.” under the select (#appeal-outcome-message)Focus: the select
Wombat
Acting as Committee member
MenuClose
[aria-label: Main]
Acting as Committee member
Home
Programme trainees
Committee reviews
Decision panels
My data rights
[title: John van Rensburg] John van Rensburg
Sign out
[aria-label: Breadcrumb]
Home
Committee reviews
Nomsa Mahlangu · 2026 S2
## Nomsa Mahlangu · 2026 S2
Annual progression review · Paed Annual Review Panel · Training year 1
Nomsa Mahlangu's progress
Under appealInadequate Progress — Additional Training · no STAR issued
Go to the appeal
Before the committee decides, Decision, Agenda (15 lines), Entrustment against Annexure A, Multi-source feedback by EPA, Evidence snapshot and Rating trajectories: flow 07's, unchanged; the order board draws them. About 7,000 px.
### Appeals
#### Appeal by Nomsa Mahlangu Open
Lodged 2026-10-13
Heard by Thandi Zulu (chair) and John van Rensburg (external).
Reason
The single DOPS reflects the start of the year, and the Mini-CEX waited eight days on the assessor's side before it was rated. I ask for reconsideration.
[aria-label: Resolve the appeal]
Outcome *required
Select an outcome…
Dismissed: the decision stands
Remitted: the appeal body replaces the decision
Dismissed leaves the committee's decision in force. Remitted replaces it with a decision the appeal body takes now, recorded below with who sat for it. Either closes the review, and nothing reopens it.
Choose an outcome.
Resolve appeal
Remitted on a progression review (4.45)John van Rensburg · the Outcome reads whole (one column, T323); Replacement category opens on “Select a category…”; the chair and he ticked and locked, so the quorum line already reads met: only a chair resolving alone meets the refusal (C11)
Wombat
Acting as Committee member
MenuClose
[aria-label: Main]
Acting as Committee member
Home
Programme trainees
Committee reviews
Decision panels
My data rights
[title: John van Rensburg] John van Rensburg
Sign out
[aria-label: Breadcrumb]
Home
Committee reviews
Nomsa Mahlangu · 2026 S2
## Nomsa Mahlangu · 2026 S2
Annual progression review · Paed Annual Review Panel · Training year 1
Nomsa Mahlangu's progress
Under appealInadequate Progress — Additional Training · no STAR issued
Go to the appeal
Before the committee decides, Decision, Agenda (15 lines), Entrustment against Annexure A, Multi-source feedback by EPA, Evidence snapshot and Rating trajectories: flow 07's, unchanged; the order board draws them. About 7,000 px.
### Appeals
#### Appeal by Nomsa Mahlangu Open
Lodged 2026-10-13
Heard by Thandi Zulu (chair) and John van Rensburg (external).
Reason
The single DOPS reflects the start of the year, and the Mini-CEX waited eight days on the assessor's side before it was rated. I ask for reconsideration.
[aria-label: Resolve the appeal]
Outcome *required
Select an outcome…
Dismissed: the decision stands
Remitted: the appeal body replaces the decision
Dismissed leaves the committee's decision in force. Remitted replaces it with a decision the appeal body takes now, recorded below with who sat for it. Either closes the review, and nothing reopens it.
Replacement category *required
Select a category…
Satisfactory Progress
Satisfactory with Observations
Inadequate Progress — Additional Training
Inadequate Progress — Repeat
Release from Training
Outcome Deferred
Replacement rationale *required
Replacement conditions
Present *
Tick every panel member at this sitting. A committee decision needs at least two panel members present: the chair and at least one other.
Thandi Zulu (chair)
David Naidoo
Sarah Botha
John van Rensburg (external)
Resolve appeal
2 present: the chair and 1 other. The quorum is met.

######## BOARD R3-3-appeals-1280-b.dc.html — Appeals · 1280 · b
## The Appeals section · 1280px · b: remitting
Route /committee/reviews/{id}#appeals · the Appeals section; above it flow 07's page, cut (the order boards draw it whole).
Components ReviewSitting's Appeals section, FormField, Badge, Alert, ActionResult, ConfirmDialog (action mode, as built), .decision-actions and p.quorum-line. CHANGED (M8): the quorum line and a refusal after the actions on the appeal form. NEW AppealEntry; .resolve-form; DecisionHistory lifted.
DESIGN.md § Entrustment-only reviews (the appeal's Outcome; the remit's note): unchanged words; § Form system: the resolve form one column. Round 3 F5, F6, F7, M2, M8, nits.
Illustration: Remitted on an entrustment-only review (C17)No cast name · the note in place of Replacement category; rationale, conditions and Present as on any review
Wombat
Acting as Committee member
MenuClose
[aria-label: Main]
Acting as Committee member
Switch to Assessor
Home
Programme trainees
Committee reviews
Decision panels
My data rights
[title: Thandi Zulu] Thandi Zulu
Sign out
[aria-label: Breadcrumb]
Home
Committee reviews
‹trainee› · 2026 S1
## ‹trainee› · 2026 S1
Entrustment-only review · Paed Annual Review Panel · Training year ‹n›
Decision (Entrustment decisions only), Agenda, Standing, MSF coverage, Evidence snapshot, Rating trajectories
### Appeals
#### Appeal by ‹trainee› Open
Lodged ‹date›
Heard by Thandi Zulu (chair) and John van Rensburg (external).
Reason
‹her reason›
[aria-label: Resolve the appeal]
Outcome *required
Select an outcome…
Dismissed: the decision stands
Remitted: the appeal body replaces the decision
Dismissed leaves the committee's decision in force. Remitted replaces it with a decision the appeal body takes now, recorded below with who sat for it. Either closes the review, and nothing reopens it.
This review decides entrustment only, so the replacement decision records no progression category, and it changes no entrustment decision: the STARs this review issued stand. To change one, revoke it and re-decide the EPA at a new review.
Replacement rationale *required
Replacement conditions
Present *
Tick every panel member at this sitting. A committee decision needs at least two panel members present: the chair and at least one other.
Thandi Zulu (chair)
David Naidoo
Sarah Botha
John van Rensburg (external)
Resolve appeal
Only the chair is ticked. A committee decision needs at least two panel members present: the chair and at least one other.
Only the chair ticked, refused (4.46)Thandi Zulu · the quorum line (flow 07's p.quorum-line, aria-live polite, now on the appeal form: CHANGED, M8) says why before she presses; the refusal after the form's actions leads “Resolve appeal was refused:” so it does not repeat the line; what she typed keptFocus: the refusalR3
Wombat
Acting as Committee member
MenuClose
[aria-label: Main]
Acting as Committee member
Switch to Assessor
Home
Programme trainees
Committee reviews
Decision panels
My data rights
[title: Thandi Zulu] Thandi Zulu
Sign out
[aria-label: Breadcrumb]
Home
Committee reviews
Nomsa Mahlangu · 2026 S2
## Nomsa Mahlangu · 2026 S2
Annual progression review · Paed Annual Review Panel · Training year 1
Nomsa Mahlangu's progress
Under appealInadequate Progress — Additional Training · no STAR issued
Go to the appeal
Before the committee decides, Decision, Agenda (15 lines), Entrustment against Annexure A, Multi-source feedback by EPA, Evidence snapshot and Rating trajectories: flow 07's, unchanged; the order board draws them. About 7,000 px.
### Appeals
#### Appeal by Nomsa Mahlangu Open
Lodged 2026-10-13
Heard by Thandi Zulu (chair) and John van Rensburg (external).
Reason
The single DOPS reflects the start of the year, and the Mini-CEX waited eight days on the assessor's side before it was rated. I ask for reconsideration.
[aria-label: Resolve the appeal]
Outcome *required
Select an outcome…
Dismissed: the decision stands
Remitted: the appeal body replaces the decision
Dismissed leaves the committee's decision in force. Remitted replaces it with a decision the appeal body takes now, recorded below with who sat for it. Either closes the review, and nothing reopens it.
Replacement category *required
Select a category…
Satisfactory Progress
Satisfactory with Observations
Inadequate Progress — Additional Training
Inadequate Progress — Repeat
Release from Training
Outcome Deferred
Replacement rationale *required
The stalled Mini-CEX was the assessor's delay; progress is adequate, with observed assessments to follow.
Replacement conditions
Present *
Tick every panel member at this sitting. A committee decision needs at least two panel members present: the chair and at least one other.
Thandi Zulu (chair)
David Naidoo
Sarah Botha
John van Rensburg (external)
Resolve appeal
Only the chair is ticked. A committee decision needs at least two panel members present: the chair and at least one other.
Resolve appeal was refused: A committee decision needs at least two panel members present: the chair and at least one other.
Sarah Botha ticked, Resolve appeal: the remit dialog (4.47, C14b)ConfirmDialog, action mode, as built; Go back returns the focus to Resolve appeal, the button that opened it (F6), what was typed keptFocus: Go backPROPOSED
Wombat
Acting as Committee member
MenuClose
[aria-label: Main]
Acting as Committee member
Switch to Assessor
Home
Programme trainees
Committee reviews
Decision panels
My data rights
[title: Thandi Zulu] Thandi Zulu
Sign out
[aria-label: Breadcrumb]
Home
Committee reviews
Nomsa Mahlangu · 2026 S2
## Nomsa Mahlangu · 2026 S2
Annual progression review · Paed Annual Review Panel · Training year 1
Nomsa Mahlangu's progress
Under appealInadequate Progress — Additional Training · no STAR issued
Go to the appeal
Before the committee decides, Decision, Agenda (15 lines), Entrustment against Annexure A, Multi-source feedback by EPA, Evidence snapshot and Rating trajectories: flow 07's, unchanged; the order board draws them. About 7,000 px.
### Appeals
#### Appeal by Nomsa Mahlangu Open
Lodged 2026-10-13
Heard by Thandi Zulu (chair) and John van Rensburg (external).
Reason
The single DOPS reflects the start of the year, and the Mini-CEX waited eight days on the assessor's side before it was rated. I ask for reconsideration.
[aria-label: Resolve the appeal]
Outcome *required
Select an outcome…
Dismissed: the decision stands
Remitted: the appeal body replaces the decision
Dismissed leaves the committee's decision in force. Remitted replaces it with a decision the appeal body takes now, recorded below with who sat for it. Either closes the review, and nothing reopens it.
Replacement category *required
Select a category…
Satisfactory Progress
Satisfactory with Observations
Inadequate Progress — Additional Training
Inadequate Progress — Repeat
Release from Training
Outcome Deferred
Replacement rationale *required
The stalled Mini-CEX was the assessor's delay; progress is adequate, with observed assessments to follow.
Replacement conditions
Two observed Mini-CEX and one DOPS before the next review.
Present *
Tick every panel member at this sitting. A committee decision needs at least two panel members present: the chair and at least one other.
Thandi Zulu (chair)
David Naidoo
Sarah Botha
John van Rensburg (external)
Resolve appeal
2 present: the chair and 1 other. The quorum is met.
### Remit Nomsa Mahlangu's appeal?
Satisfactory with Observations replaces Inadequate Progress — Additional Training.
Present: Thandi Zulu (chair), Sarah Botha.
This closes the 2026 S2 review. Nothing reopens it, and no STAR changes.
Go back
Remit the appeal
Resolved, Remitted (4.47)The top: Closed, no jump (no longer Under appeal); the replacement heads the history with its Conditions and who sat · the foot: “Appeal resolved.” heads the Appeals section; the entry Remitted, “Resolved 2026-10-13 by Thandi Zulu”Focus: the resultNEW
Wombat
Acting as Committee member
MenuClose
[aria-label: Main]
Acting as Committee member
Switch to Assessor
Home
Programme trainees
Committee reviews
Decision panels
My data rights
[title: Thandi Zulu] Thandi Zulu
Sign out
[aria-label: Breadcrumb]
Home
Committee reviews
Nomsa Mahlangu · 2026 S2
## Nomsa Mahlangu · 2026 S2
Annual progression review · Paed Annual Review Panel · Training year 1
Nomsa Mahlangu's progress
ClosedSatisfactory with Observations · no STAR issued
### Decision
#### Satisfactory with Observations
The stalled Mini-CEX was the assessor's delay; progress is adequate, with observed assessments to follow.
About the EPAs not decided: Not enough observed evidence yet this year.
Conditions: Two observed Mini-CEX and one DOPS before the next review.
Present: Thandi Zulu (chair), Sarah Botha
#### Inadequate Progress — Additional Training
Replaced on appeal by the decision above.
One DOPS in the year and a Mini-CEX that stalled; a supervised support plan and re-review in six months.
About the EPAs not decided: Not enough observed evidence yet this year.
Present: Thandi Zulu (chair), Sarah Botha, John van Rensburg (external)
Before the committee decides, Decision, Agenda (15 lines), Entrustment against Annexure A, Multi-source feedback by EPA, Evidence snapshot and Rating trajectories: flow 07's, unchanged; the order board draws them. About 7,000 px.
### Appeals
Appeal resolved.
#### Appeal by Nomsa Mahlangu Remitted
Lodged 2026-10-13 · Resolved 2026-10-13 by Thandi Zulu
Remitted: the appeal body replaced the decision. The replacement decision is above.
Reason
The single DOPS reflects the start of the year, and the Mini-CEX waited eight days on the assessor's side before it was rated. I ask for reconsideration.

######## BOARD R3-3-appeals-1280-c.dc.html — Appeals · 1280 · c
## The Appeals section · 1280px · c: dismissing and the edge states
Route /committee/reviews/{id}#appeals · the Appeals section; above it flow 07's page, cut (the order boards draw it whole).
Components ReviewSitting's Appeals section, FormField, Badge, Alert, ActionResult, ConfirmDialog (action mode, as built), .decision-actions and p.quorum-line. CHANGED (M8): the quorum line and a refusal after the actions on the appeal form. NEW AppealEntry; .resolve-form; DecisionHistory lifted.
DESIGN.md § Entrustment-only reviews (the appeal's Outcome; the remit's note): unchanged words; § Form system: the resolve form one column. Round 3 F5, F6, F7, M2, M8, nits.
Dismissed chosen, Resolve appeal: the dismiss dialog (C14c)No Present asked: a dismissal takes no new decision · Go back returns the focus to Resolve appeal (F6)Focus: Go backPROPOSED
Wombat
Acting as Committee member
MenuClose
[aria-label: Main]
Acting as Committee member
Switch to Assessor
Home
Programme trainees
Committee reviews
Decision panels
My data rights
[title: Thandi Zulu] Thandi Zulu
Sign out
[aria-label: Breadcrumb]
Home
Committee reviews
Nomsa Mahlangu · 2026 S2
## Nomsa Mahlangu · 2026 S2
Annual progression review · Paed Annual Review Panel · Training year 1
Nomsa Mahlangu's progress
Under appealInadequate Progress — Additional Training · no STAR issued
Go to the appeal
Before the committee decides, Decision, Agenda (15 lines), Entrustment against Annexure A, Multi-source feedback by EPA, Evidence snapshot and Rating trajectories: flow 07's, unchanged; the order board draws them. About 7,000 px.
### Appeals
#### Appeal by Nomsa Mahlangu Open
Lodged 2026-10-13
Heard by Thandi Zulu (chair) and John van Rensburg (external).
Reason
The single DOPS reflects the start of the year, and the Mini-CEX waited eight days on the assessor's side before it was rated. I ask for reconsideration.
[aria-label: Resolve the appeal]
Outcome *required
Select an outcome…
Dismissed: the decision stands
Remitted: the appeal body replaces the decision
Dismissed leaves the committee's decision in force. Remitted replaces it with a decision the appeal body takes now, recorded below with who sat for it. Either closes the review, and nothing reopens it.
Resolve appeal
### Dismiss Nomsa Mahlangu's appeal?
Inadequate Progress — Additional Training stays in force.
This closes the 2026 S2 review. Nothing reopens it.
Go back
Dismiss the appeal
Resolved, DismissedScratch database: Anele Dlamini's appeal on review #2, dismissed by Thandi Zulu · the decision stands, alone in the historyFocus: the result
Wombat
Acting as Committee member
MenuClose
[aria-label: Main]
Acting as Committee member
Switch to Assessor
Home
Programme trainees
Committee reviews
Decision panels
My data rights
[title: Thandi Zulu] Thandi Zulu
Sign out
[aria-label: Breadcrumb]
Home
Committee reviews
Anele Dlamini · 2026 S2
## Anele Dlamini · 2026 S2
Annual progression review · Paed Annual Review Panel · Training year 3
Anele Dlamini's progress
ClosedSatisfactory Progress · 2 STARs issued
### Decision
#### Satisfactory Progress
Steady progress in year 3; two EPAs entrusted at this sitting.
About the EPAs not decided: Not at a decision point this year.
Present: Thandi Zulu (chair), David Naidoo, Sarah Botha
Before the committee decides, Decision, Agenda (15 lines), Entrustment against Annexure A, Multi-source feedback by EPA, Evidence snapshot and Rating trajectories: flow 07's, unchanged; the order board draws them. About 7,000 px.
### Appeals
Appeal resolved.
#### Appeal by Anele Dlamini Dismissed
Lodged 2026-10-12 · Resolved 2026-10-13 by Thandi Zulu
Dismissed: the decision stands.
Reason
The PAED-002 decision rested on PAED-001 evidence only. I ask for reconsideration.
Nobody on the appeal body can act now (C12a)Scratch state: the chair's seat lapsed and no external member may sit · no form; the line names nobody; the entry has no “Heard by”
Wombat
Acting as Committee member
MenuClose
[aria-label: Main]
Acting as Committee member
Switch to Assessor
Home
Programme trainees
Committee reviews
Decision panels
My data rights
[title: Sarah Botha] Sarah Botha
Sign out
[aria-label: Breadcrumb]
Home
Committee reviews
Nomsa Mahlangu · 2026 S2
## Nomsa Mahlangu · 2026 S2
Annual progression review · Paed Annual Review Panel · Training year 1
Nomsa Mahlangu's progress
Under appealInadequate Progress — Additional Training · no STAR issued
Go to the appeal
Before the committee decides, Decision, Agenda (15 lines), Entrustment against Annexure A, Multi-source feedback by EPA, Evidence snapshot and Rating trajectories: flow 07's, unchanged; the order board draws them. About 7,000 px.
### Appeals
#### Appeal by Nomsa Mahlangu Open
Lodged 2026-10-13
Reason
The single DOPS reflects the start of the year, and the Mini-CEX waited eight days on the assessor's side before it was rated. I ask for reconsideration.
Only the appeal body, the panel's chair or an external member, can resolve the appeal, and none of them can act now. A panel administrator must seat a chair who can.
The panel cannot seat a quorum (C12b)Scratch state: no second member who may sit · the form is offered and Dismissed works; with Remitted, Resolve appeal is aria-disabled with PanelShortfall's reason beside it, in place of the quorum line
Wombat
Acting as Committee member
MenuClose
[aria-label: Main]
Acting as Committee member
Switch to Assessor
Home
Programme trainees
Committee reviews
Decision panels
My data rights
[title: Thandi Zulu] Thandi Zulu
Sign out
[aria-label: Breadcrumb]
Home
Committee reviews
Nomsa Mahlangu · 2026 S2
## Nomsa Mahlangu · 2026 S2
Annual progression review · Paed Annual Review Panel · Training year 1
Nomsa Mahlangu's progress
Under appealInadequate Progress — Additional Training · no STAR issued
Go to the appeal
Before the committee decides, Decision, Agenda (15 lines), Entrustment against Annexure A, Multi-source feedback by EPA, Evidence snapshot and Rating trajectories: flow 07's, unchanged; the order board draws them. About 7,000 px.
### Appeals
#### Appeal by Nomsa Mahlangu Open
Lodged 2026-10-13
Heard by Thandi Zulu (chair) and John van Rensburg (external).
Reason
The single DOPS reflects the start of the year, and the Mini-CEX waited eight days on the assessor's side before it was rated. I ask for reconsideration.
[aria-label: Resolve the appeal]
Outcome *required
Select an outcome…
Dismissed: the decision stands
Remitted: the appeal body replaces the decision
Dismissed leaves the committee's decision in force. Remitted replaces it with a decision the appeal body takes now, recorded below with who sat for it. Either closes the review, and nothing reopens it.
Replacement category *required
Select a category…
Satisfactory Progress
Satisfactory with Observations
Inadequate Progress — Additional Training
Inadequate Progress — Repeat
Release from Training
Outcome Deferred
Replacement rationale *required
The stalled Mini-CEX was the assessor's delay; progress is adequate, with observed assessments to follow.
Replacement conditions
Present *
Tick every panel member at this sitting. A committee decision needs at least two panel members present: the chair and at least one other.
Thandi Zulu (chair)
Resolve appeal
Resolve appeal: Only the chair can be recorded as present: a decision needs the chair and at least one other, each an active committee member at the panel's institution who is not a trainee, and none the trainee under review. A panel administrator must add a member.
No appeal lodgedSarah Botha on Lerato Molefe's ratified review #1 · today's words; no jump (nothing Under appeal)
Wombat
Acting as Committee member
MenuClose
[aria-label: Main]
Acting as Committee member
Switch to Assessor
Home
Programme trainees
Committee reviews
Decision panels
My data rights
[title: Sarah Botha] Sarah Botha
Sign out
[aria-label: Breadcrumb]
Home
Committee reviews
Lerato Molefe · 2026 S2
## Lerato Molefe · 2026 S2
Annual progression review · Paed Annual Review Panel · Training year 4
Lerato Molefe's progress
RatifiedSatisfactory Progress · 3 STARs issued
Before the committee decides, Decision, Agenda (15 lines), Entrustment against Annexure A, Multi-source feedback by EPA, Evidence snapshot and Rating trajectories: flow 07's, unchanged; the order board draws them. About 7,000 px.
### Appeals
No appeal has been lodged against this review.

######## BOARD R3-3-appeals-390-a.dc.html — Appeals · 390 · a
## The Appeals section · 390px · a
Route /committee/reviews/{id}#appeals · the Appeals section; above it flow 07's page, cut (the order boards draw it whole).
Components ReviewSitting's Appeals section, FormField, Badge, Alert, ActionResult, ConfirmDialog (action mode, as built), .decision-actions and p.quorum-line. CHANGED (M8): the quorum line and a refusal after the actions on the appeal form. NEW AppealEntry; .resolve-form; DecisionHistory lifted.
DESIGN.md § Entrustment-only reviews (the appeal's Outcome; the remit's note): unchanged words; § Form system: the resolve form one column. Round 3 F5, F6, F7, M2, M8, nits.
A member's view (4.44)David Naidoo · the entry, Open, “Heard by Thandi Zulu (chair) and John van Rensburg (external).”; the who-may line; no form (C10); Go to the appeal in the barNEW
Wombat
Acting as Committee member
MenuClose
[aria-label: Main]
Acting as Committee member
Switch to Assessor
Home
Programme trainees
Committee reviews
Decision panels
My data rights
[title: David Naidoo] David Naidoo
Sign out
[aria-label: Breadcrumb]
Home
Committee reviews
Nomsa Mahlangu · 2026 S2
## Nomsa Mahlangu · 2026 S2
Annual progression review · Paed Annual Review Panel · Training year 1
Nomsa Mahlangu's progress
Under appealInadequate Progress — Additional Training · no STAR issued
Go to the appeal
### Decision
#### Inadequate Progress — Additional Training
One DOPS in the year and a Mini-CEX that stalled; a supervised support plan and re-review in six months.
About the EPAs not decided: Not enough observed evidence yet this year.
Present: Thandi Zulu (chair), Sarah Botha, John van Rensburg (external)
Before the committee decides, Decision, Agenda (15 lines), Entrustment against Annexure A, Multi-source feedback by EPA, Evidence snapshot and Rating trajectories: flow 07's, unchanged; the order board draws them. About 7,000 px.
### Appeals
#### Appeal by Nomsa Mahlangu Open
Lodged 2026-10-13
Heard by Thandi Zulu (chair) and John van Rensburg (external).
Reason
The single DOPS reflects the start of the year, and the Mini-CEX waited eight days on the assessor's side before it was rated. I ask for reconsideration.
Only the appeal body can resolve the appeal: the panel's chair, Thandi Zulu, and its external member, John van Rensburg.
An appeal body member who cannot act now (C13, F7, M2)Scratch state: ‹name›, a Coordinator at Kgosi Kgari Teaching Hospital who also holds the panel's External seat and cannot act now · reads the page as a Coordinator (the Coordinator's menu); “Heard by” and the who-may line leave them out. A lapsed member with no other rung cannot open the review (T279)R3
Wombat
Acting as Coordinator
MenuClose
[aria-label: Main]
Acting as Coordinator
Switch to Committee member
Home
Programme trainees
Waiting for assessors
Decisions due
MSF campaigns
Committee reviews
Decision panels
Data rights requests
My data rights
[title: ‹name›] ‹name›
Sign out
[aria-label: Breadcrumb]
Home
Committee reviews
Nomsa Mahlangu · 2026 S2
## Nomsa Mahlangu · 2026 S2
Annual progression review · Paed Annual Review Panel · Training year 1
Nomsa Mahlangu's progress
Under appealInadequate Progress — Additional Training · no STAR issued
Go to the appeal
Before the committee decides, Decision, Agenda (15 lines), Entrustment against Annexure A, Multi-source feedback by EPA, Evidence snapshot and Rating trajectories: flow 07's, unchanged; the order board draws them. About 7,000 px.
### Appeals
#### Appeal by Nomsa Mahlangu Open
Lodged 2026-10-13
Heard by Thandi Zulu (chair).
Reason
The single DOPS reflects the start of the year, and the Mini-CEX waited eight days on the assessor's side before it was rated. I ask for reconsideration.
Only the appeal body can resolve the appeal: the panel's chair, Thandi Zulu.
The appeal body's form (4.45)John van Rensburg · inline (C10): only to a member of the appeal body who can act now, only while Under appeal · Outcome opens on “Select an outcome…”
Wombat
Acting as Committee member
MenuClose
[aria-label: Main]
Acting as Committee member
Home
Programme trainees
Committee reviews
Decision panels
My data rights
[title: John van Rensburg] John van Rensburg
Sign out
[aria-label: Breadcrumb]
Home
Committee reviews
Nomsa Mahlangu · 2026 S2
## Nomsa Mahlangu · 2026 S2
Annual progression review · Paed Annual Review Panel · Training year 1
Nomsa Mahlangu's progress
Under appealInadequate Progress — Additional Training · no STAR issued
Go to the appeal
Before the committee decides, Decision, Agenda (15 lines), Entrustment against Annexure A, Multi-source feedback by EPA, Evidence snapshot and Rating trajectories: flow 07's, unchanged; the order board draws them. About 7,000 px.
### Appeals
#### Appeal by Nomsa Mahlangu Open
Lodged 2026-10-13
Heard by Thandi Zulu (chair) and John van Rensburg (external).
Reason
The single DOPS reflects the start of the year, and the Mini-CEX waited eight days on the assessor's side before it was rated. I ask for reconsideration.
[aria-label: Resolve the appeal]
Outcome *required
Select an outcome…
Dismissed: the decision stands
Remitted: the appeal body replaces the decision
Dismissed leaves the committee's decision in force. Remitted replaces it with a decision the appeal body takes now, recorded below with who sat for it. Either closes the review, and nothing reopens it.
Resolve appeal
Resolve appeal with no outcome (4.45)Nothing is sent; “Choose an outcome.” under the select (#appeal-outcome-message)Focus: the select
Wombat
Acting as Committee member
MenuClose
[aria-label: Main]
Acting as Committee member
Home
Programme trainees
Committee reviews
Decision panels
My data rights
[title: John van Rensburg] John van Rensburg
Sign out
[aria-label: Breadcrumb]
Home
Committee reviews
Nomsa Mahlangu · 2026 S2
## Nomsa Mahlangu · 2026 S2
Annual progression review · Paed Annual Review Panel · Training year 1
Nomsa Mahlangu's progress
Under appealInadequate Progress — Additional Training · no STAR issued
Go to the appeal
Before the committee decides, Decision, Agenda (15 lines), Entrustment against Annexure A, Multi-source feedback by EPA, Evidence snapshot and Rating trajectories: flow 07's, unchanged; the order board draws them. About 7,000 px.
### Appeals
#### Appeal by Nomsa Mahlangu Open
Lodged 2026-10-13
Heard by Thandi Zulu (chair) and John van Rensburg (external).
Reason
The single DOPS reflects the start of the year, and the Mini-CEX waited eight days on the assessor's side before it was rated. I ask for reconsideration.
[aria-label: Resolve the appeal]
Outcome *required
Select an outcome…
Dismissed: the decision stands
Remitted: the appeal body replaces the decision
Dismissed leaves the committee's decision in force. Remitted replaces it with a decision the appeal body takes now, recorded below with who sat for it. Either closes the review, and nothing reopens it.
Choose an outcome.
Resolve appeal
Remitted on a progression review (4.45)John van Rensburg · at 390 the closed select can still cut the chosen option's end (“…replaces the decision”): flagged on the Spec; Replacement category opens on “Select a category…”; the chair and he ticked and locked, so the quorum line already reads met: only a chair resolving alone meets the refusal (C11)
Wombat
Acting as Committee member
MenuClose
[aria-label: Main]
Acting as Committee member
Home
Programme trainees
Committee reviews
Decision panels
My data rights
[title: John van Rensburg] John van Rensburg
Sign out
[aria-label: Breadcrumb]
Home
Committee reviews
Nomsa Mahlangu · 2026 S2
## Nomsa Mahlangu · 2026 S2
Annual progression review · Paed Annual Review Panel · Training year 1
Nomsa Mahlangu's progress
Under appealInadequate Progress — Additional Training · no STAR issued
Go to the appeal
Before the committee decides, Decision, Agenda (15 lines), Entrustment against Annexure A, Multi-source feedback by EPA, Evidence snapshot and Rating trajectories: flow 07's, unchanged; the order board draws them. About 7,000 px.
### Appeals
#### Appeal by Nomsa Mahlangu Open
Lodged 2026-10-13
Heard by Thandi Zulu (chair) and John van Rensburg (external).
Reason
The single DOPS reflects the start of the year, and the Mini-CEX waited eight days on the assessor's side before it was rated. I ask for reconsideration.
[aria-label: Resolve the appeal]
Outcome *required
Select an outcome…
Dismissed: the decision stands
Remitted: the appeal body replaces the decision
Dismissed leaves the committee's decision in force. Remitted replaces it with a decision the appeal body takes now, recorded below with who sat for it. Either closes the review, and nothing reopens it.
Replacement category *required
Select a category…
Satisfactory Progress
Satisfactory with Observations
Inadequate Progress — Additional Training
Inadequate Progress — Repeat
Release from Training
Outcome Deferred
Replacement rationale *required
Replacement conditions
Present *
Tick every panel member at this sitting. A committee decision needs at least two panel members present: the chair and at least one other.
Thandi Zulu (chair)
David Naidoo
Sarah Botha
John van Rensburg (external)
Resolve appeal
2 present: the chair and 1 other. The quorum is met.
Only the chair ticked, refused (4.46)Thandi Zulu · the quorum line (flow 07's p.quorum-line, aria-live polite, now on the appeal form: CHANGED, M8) says why before she presses; the refusal after the form's actions leads “Resolve appeal was refused:” so it does not repeat the line; what she typed keptFocus: the refusalR3
Wombat
Acting as Committee member
MenuClose
[aria-label: Main]
Acting as Committee member
Switch to Assessor
Home
Programme trainees
Committee reviews
Decision panels
My data rights
[title: Thandi Zulu] Thandi Zulu
Sign out
[aria-label: Breadcrumb]
Home
Committee reviews
Nomsa Mahlangu · 2026 S2
## Nomsa Mahlangu · 2026 S2
Annual progression review · Paed Annual Review Panel · Training year 1
Nomsa Mahlangu's progress
Under appealInadequate Progress — Additional Training · no STAR issued
Go to the appeal
Before the committee decides, Decision, Agenda (15 lines), Entrustment against Annexure A, Multi-source feedback by EPA, Evidence snapshot and Rating trajectories: flow 07's, unchanged; the order board draws them. About 7,000 px.
### Appeals
#### Appeal by Nomsa Mahlangu Open
Lodged 2026-10-13
Heard by Thandi Zulu (chair) and John van Rensburg (external).
Reason
The single DOPS reflects the start of the year, and the Mini-CEX waited eight days on the assessor's side before it was rated. I ask for reconsideration.
[aria-label: Resolve the appeal]
Outcome *required
Select an outcome…
Dismissed: the decision stands
Remitted: the appeal body replaces the decision
Dismissed leaves the committee's decision in force. Remitted replaces it with a decision the appeal body takes now, recorded below with who sat for it. Either closes the review, and nothing reopens it.
Replacement category *required
Select a category…
Satisfactory Progress
Satisfactory with Observations
Inadequate Progress — Additional Training
Inadequate Progress — Repeat
Release from Training
Outcome Deferred
Replacement rationale *required
The stalled Mini-CEX was the assessor's delay; progress is adequate, with observed assessments to follow.
Replacement conditions
Present *
Tick every panel member at this sitting. A committee decision needs at least two panel members present: the chair and at least one other.
Thandi Zulu (chair)
David Naidoo
Sarah Botha
John van Rensburg (external)
Resolve appeal
Only the chair is ticked. A committee decision needs at least two panel members present: the chair and at least one other.
Resolve appeal was refused: A committee decision needs at least two panel members present: the chair and at least one other.
Sarah Botha ticked, Resolve appeal: the remit dialog (4.47, C14b)ConfirmDialog, action mode, as built; Go back returns the focus to Resolve appeal, the button that opened it (F6), what was typed keptFocus: Go backPROPOSED
Wombat
Acting as Committee member
MenuClose
[aria-label: Main]
Acting as Committee member
Switch to Assessor
Home
Programme trainees
Committee reviews
Decision panels
My data rights
[title: Thandi Zulu] Thandi Zulu
Sign out
[aria-label: Breadcrumb]
Home
Committee reviews
Nomsa Mahlangu · 2026 S2
## Nomsa Mahlangu · 2026 S2
Annual progression review · Paed Annual Review Panel · Training year 1
Nomsa Mahlangu's progress
Under appealInadequate Progress — Additional Training · no STAR issued
Go to the appeal
Before the committee decides, Decision, Agenda (15 lines), Entrustment against Annexure A, Multi-source feedback by EPA, Evidence snapshot and Rating trajectories: flow 07's, unchanged; the order board draws them. About 7,000 px.
### Appeals
#### Appeal by Nomsa Mahlangu Open
Lodged 2026-10-13
Heard by Thandi Zulu (chair) and John van Rensburg (external).
Reason
The single DOPS reflects the start of the year, and the Mini-CEX waited eight days on the assessor's side before it was rated. I ask for reconsideration.
[aria-label: Resolve the appeal]
Outcome *required
Select an outcome…
Dismissed: the decision stands
Remitted: the appeal body replaces the decision
Dismissed leaves the committee's decision in force. Remitted replaces it with a decision the appeal body takes now, recorded below with who sat for it. Either closes the review, and nothing reopens it.
Replacement category *required
Select a category…
Satisfactory Progress
Satisfactory with Observations
Inadequate Progress — Additional Training
Inadequate Progress — Repeat
Release from Training
Outcome Deferred
Replacement rationale *required
The stalled Mini-CEX was the assessor's delay; progress is adequate, with observed assessments to follow.
Replacement conditions
Two observed Mini-CEX and one DOPS before the next review.
Present *
Tick every panel member at this sitting. A committee decision needs at least two panel members present: the chair and at least one other.
Thandi Zulu (chair)
David Naidoo
Sarah Botha
John van Rensburg (external)
Resolve appeal
2 present: the chair and 1 other. The quorum is met.
### Remit Nomsa Mahlangu's appeal?
Satisfactory with Observations replaces Inadequate Progress — Additional Training.
Present: Thandi Zulu (chair), Sarah Botha.
This closes the 2026 S2 review. Nothing reopens it, and no STAR changes.
Go back
Remit the appeal

######## BOARD R3-3-appeals-390-b.dc.html — Appeals · 390 · b
## The Appeals section · 390px · b
Route /committee/reviews/{id}#appeals · the Appeals section; above it flow 07's page, cut (the order boards draw it whole).
Components ReviewSitting's Appeals section, FormField, Badge, Alert, ActionResult, ConfirmDialog (action mode, as built), .decision-actions and p.quorum-line. CHANGED (M8): the quorum line and a refusal after the actions on the appeal form. NEW AppealEntry; .resolve-form; DecisionHistory lifted.
DESIGN.md § Entrustment-only reviews (the appeal's Outcome; the remit's note): unchanged words; § Form system: the resolve form one column. Round 3 F5, F6, F7, M2, M8, nits.
Resolved, Remitted (4.47)The top: Closed, no jump (no longer Under appeal); the replacement heads the history with its Conditions and who sat · the foot: “Appeal resolved.” heads the Appeals section; the entry Remitted, “Resolved 2026-10-13 by Thandi Zulu”Focus: the resultNEW
Wombat
Acting as Committee member
MenuClose
[aria-label: Main]
Acting as Committee member
Switch to Assessor
Home
Programme trainees
Committee reviews
Decision panels
My data rights
[title: Thandi Zulu] Thandi Zulu
Sign out
[aria-label: Breadcrumb]
Home
Committee reviews
Nomsa Mahlangu · 2026 S2
## Nomsa Mahlangu · 2026 S2
Annual progression review · Paed Annual Review Panel · Training year 1
Nomsa Mahlangu's progress
ClosedSatisfactory with Observations · no STAR issued
### Decision
#### Satisfactory with Observations
The stalled Mini-CEX was the assessor's delay; progress is adequate, with observed assessments to follow.
About the EPAs not decided: Not enough observed evidence yet this year.
Conditions: Two observed Mini-CEX and one DOPS before the next review.
Present: Thandi Zulu (chair), Sarah Botha
#### Inadequate Progress — Additional Training
Replaced on appeal by the decision above.
One DOPS in the year and a Mini-CEX that stalled; a supervised support plan and re-review in six months.
About the EPAs not decided: Not enough observed evidence yet this year.
Present: Thandi Zulu (chair), Sarah Botha, John van Rensburg (external)
Before the committee decides, Decision, Agenda (15 lines), Entrustment against Annexure A, Multi-source feedback by EPA, Evidence snapshot and Rating trajectories: flow 07's, unchanged; the order board draws them. About 7,000 px.
### Appeals
Appeal resolved.
#### Appeal by Nomsa Mahlangu Remitted
Lodged 2026-10-13 · Resolved 2026-10-13 by Thandi Zulu
Remitted: the appeal body replaced the decision. The replacement decision is above.
Reason
The single DOPS reflects the start of the year, and the Mini-CEX waited eight days on the assessor's side before it was rated. I ask for reconsideration.
Dismissed chosen, Resolve appeal: the dismiss dialog (C14c)No Present asked: a dismissal takes no new decision · Go back returns the focus to Resolve appeal (F6)Focus: Go backPROPOSED
Wombat
Acting as Committee member
MenuClose
[aria-label: Main]
Acting as Committee member
Switch to Assessor
Home
Programme trainees
Committee reviews
Decision panels
My data rights
[title: Thandi Zulu] Thandi Zulu
Sign out
[aria-label: Breadcrumb]
Home
Committee reviews
Nomsa Mahlangu · 2026 S2
## Nomsa Mahlangu · 2026 S2
Annual progression review · Paed Annual Review Panel · Training year 1
Nomsa Mahlangu's progress
Under appealInadequate Progress — Additional Training · no STAR issued
Go to the appeal
Before the committee decides, Decision, Agenda (15 lines), Entrustment against Annexure A, Multi-source feedback by EPA, Evidence snapshot and Rating trajectories: flow 07's, unchanged; the order board draws them. About 7,000 px.
### Appeals
#### Appeal by Nomsa Mahlangu Open
Lodged 2026-10-13
Heard by Thandi Zulu (chair) and John van Rensburg (external).
Reason
The single DOPS reflects the start of the year, and the Mini-CEX waited eight days on the assessor's side before it was rated. I ask for reconsideration.
[aria-label: Resolve the appeal]
Outcome *required
Select an outcome…
Dismissed: the decision stands
Remitted: the appeal body replaces the decision
Dismissed leaves the committee's decision in force. Remitted replaces it with a decision the appeal body takes now, recorded below with who sat for it. Either closes the review, and nothing reopens it.
Resolve appeal
### Dismiss Nomsa Mahlangu's appeal?
Inadequate Progress — Additional Training stays in force.
This closes the 2026 S2 review. Nothing reopens it.
Go back
Dismiss the appeal
Resolved, DismissedScratch database: Anele Dlamini's appeal on review #2, dismissed by Thandi Zulu · the decision stands, alone in the historyFocus: the result
Wombat
Acting as Committee member
MenuClose
[aria-label: Main]
Acting as Committee member
Switch to Assessor
Home
Programme trainees
Committee reviews
Decision panels
My data rights
[title: Thandi Zulu] Thandi Zulu
Sign out
[aria-label: Breadcrumb]
Home
Committee reviews
Anele Dlamini · 2026 S2
## Anele Dlamini · 2026 S2
Annual progression review · Paed Annual Review Panel · Training year 3
Anele Dlamini's progress
ClosedSatisfactory Progress · 2 STARs issued
### Decision
#### Satisfactory Progress
Steady progress in year 3; two EPAs entrusted at this sitting.
About the EPAs not decided: Not at a decision point this year.
Present: Thandi Zulu (chair), David Naidoo, Sarah Botha
Before the committee decides, Decision, Agenda (15 lines), Entrustment against Annexure A, Multi-source feedback by EPA, Evidence snapshot and Rating trajectories: flow 07's, unchanged; the order board draws them. About 7,000 px.
### Appeals
Appeal resolved.
#### Appeal by Anele Dlamini Dismissed
Lodged 2026-10-12 · Resolved 2026-10-13 by Thandi Zulu
Dismissed: the decision stands.
Reason
The PAED-002 decision rested on PAED-001 evidence only. I ask for reconsideration.
Nobody on the appeal body can act now (C12a)Scratch state: the chair's seat lapsed and no external member may sit · no form; the line names nobody; the entry has no “Heard by”
Wombat
Acting as Committee member
MenuClose
[aria-label: Main]
Acting as Committee member
Switch to Assessor
Home
Programme trainees
Committee reviews
Decision panels
My data rights
[title: Sarah Botha] Sarah Botha
Sign out
[aria-label: Breadcrumb]
Home
Committee reviews
Nomsa Mahlangu · 2026 S2
## Nomsa Mahlangu · 2026 S2
Annual progression review · Paed Annual Review Panel · Training year 1
Nomsa Mahlangu's progress
Under appealInadequate Progress — Additional Training · no STAR issued
Go to the appeal
Before the committee decides, Decision, Agenda (15 lines), Entrustment against Annexure A, Multi-source feedback by EPA, Evidence snapshot and Rating trajectories: flow 07's, unchanged; the order board draws them. About 7,000 px.
### Appeals
#### Appeal by Nomsa Mahlangu Open
Lodged 2026-10-13
Reason
The single DOPS reflects the start of the year, and the Mini-CEX waited eight days on the assessor's side before it was rated. I ask for reconsideration.
Only the appeal body, the panel's chair or an external member, can resolve the appeal, and none of them can act now. A panel administrator must seat a chair who can.
The panel cannot seat a quorum (C12b)Scratch state: no second member who may sit · the form is offered and Dismissed works; with Remitted, Resolve appeal is aria-disabled with PanelShortfall's reason beside it, in place of the quorum line
Wombat
Acting as Committee member
MenuClose
[aria-label: Main]
Acting as Committee member
Switch to Assessor
Home
Programme trainees
Committee reviews
Decision panels
My data rights
[title: Thandi Zulu] Thandi Zulu
Sign out
[aria-label: Breadcrumb]
Home
Committee reviews
Nomsa Mahlangu · 2026 S2
## Nomsa Mahlangu · 2026 S2
Annual progression review · Paed Annual Review Panel · Training year 1
Nomsa Mahlangu's progress
Under appealInadequate Progress — Additional Training · no STAR issued
Go to the appeal
Before the committee decides, Decision, Agenda (15 lines), Entrustment against Annexure A, Multi-source feedback by EPA, Evidence snapshot and Rating trajectories: flow 07's, unchanged; the order board draws them. About 7,000 px.
### Appeals
#### Appeal by Nomsa Mahlangu Open
Lodged 2026-10-13
Heard by Thandi Zulu (chair) and John van Rensburg (external).
Reason
The single DOPS reflects the start of the year, and the Mini-CEX waited eight days on the assessor's side before it was rated. I ask for reconsideration.
[aria-label: Resolve the appeal]
Outcome *required
Select an outcome…
Dismissed: the decision stands
Remitted: the appeal body replaces the decision
Dismissed leaves the committee's decision in force. Remitted replaces it with a decision the appeal body takes now, recorded below with who sat for it. Either closes the review, and nothing reopens it.
Replacement category *required
Select a category…
Satisfactory Progress
Satisfactory with Observations
Inadequate Progress — Additional Training
Inadequate Progress — Repeat
Release from Training
Outcome Deferred
Replacement rationale *required
The stalled Mini-CEX was the assessor's delay; progress is adequate, with observed assessments to follow.
Replacement conditions
Present *
Tick every panel member at this sitting. A committee decision needs at least two panel members present: the chair and at least one other.
Thandi Zulu (chair)
Resolve appeal
Resolve appeal: Only the chair can be recorded as present: a decision needs the chair and at least one other, each an active committee member at the panel's institution who is not a trainee, and none the trainee under review. A panel administrator must add a member.
No appeal lodgedSarah Botha on Lerato Molefe's ratified review #1 · today's words; no jump (nothing Under appeal)
Wombat
Acting as Committee member
MenuClose
[aria-label: Main]
Acting as Committee member
Switch to Assessor
Home
Programme trainees
Committee reviews
Decision panels
My data rights
[title: Sarah Botha] Sarah Botha
Sign out
[aria-label: Breadcrumb]
Home
Committee reviews
Lerato Molefe · 2026 S2
## Lerato Molefe · 2026 S2
Annual progression review · Paed Annual Review Panel · Training year 4
Lerato Molefe's progress
RatifiedSatisfactory Progress · 3 STARs issued
Before the committee decides, Decision, Agenda (15 lines), Entrustment against Annexure A, Multi-source feedback by EPA, Evidence snapshot and Rating trajectories: flow 07's, unchanged; the order board draws them. About 7,000 px.
### Appeals
No appeal has been lodged against this review.
Illustration: Remitted on an entrustment-only review (C17)No cast name · the note in place of Replacement category; rationale, conditions and Present as on any review
Wombat
Acting as Committee member
MenuClose
[aria-label: Main]
Acting as Committee member
Switch to Assessor
Home
Programme trainees
Committee reviews
Decision panels
My data rights
[title: Thandi Zulu] Thandi Zulu
Sign out
[aria-label: Breadcrumb]
Home
Committee reviews
‹trainee› · 2026 S1
## ‹trainee› · 2026 S1
Entrustment-only review · Paed Annual Review Panel · Training year ‹n›
Decision (Entrustment decisions only), Agenda, Standing, MSF coverage, Evidence snapshot, Rating trajectories
### Appeals
#### Appeal by ‹trainee› Open
Lodged ‹date›
Heard by Thandi Zulu (chair) and John van Rensburg (external).
Reason
‹her reason›
[aria-label: Resolve the appeal]
Outcome *required
Select an outcome…
Dismissed: the decision stands
Remitted: the appeal body replaces the decision
Dismissed leaves the committee's decision in force. Remitted replaces it with a decision the appeal body takes now, recorded below with who sat for it. Either closes the review, and nothing reopens it.
This review decides entrustment only, so the replacement decision records no progression category, and it changes no entrustment decision: the STARs this review issued stand. To change one, revoke it and re-decide the EPA at a new review.
Replacement rationale *required
Replacement conditions
Present *
Tick every panel member at this sitting. A committee decision needs at least two panel members present: the chair and at least one other.
Thandi Zulu (chair)
David Naidoo
Sarah Botha
John van Rensburg (external)
Resolve appeal
Only the chair is ticked. A committee decision needs at least two panel members present: the chair and at least one other.

######## BOARD R3-5-auth-1280-a.dc.html — My authorisations · 1280 · a
## My authorisations · 1280px · a
Route /portfolio/authorisations · Trainee · under My progress, its trail Home › My progress › My authorisations (Q10).
Components PageHeader, Breadcrumbs, DetailCard, .detail-list, Badge (BadgeFor.EntrustmentDecision), a hand-written stacked .clinic-table (S8), EpaLabel, StatePanel, Skeleton, Alert with Try again. NEW .star-grid (equal columns), .star-card, No longer in force, .download-result; the subtitle's words.
Rests on T367 decided, T367: her query widened to revoked and expired, with the reviser's name.
DESIGN.md the owner table's row for My authorisations (unchanged); § Badges, EntrustmentDecision (unchanged); § Accessibility, the label in the name (F4). Round 3 F2, F4, M7, S1, S2, S3, S9, S13.
None yetSipho Ndlovu · StatePanel's empty card, today's words; no “No longer in force” (hidden when empty, C18)Focus: the h1
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
[title: Sipho Ndlovu] Sipho Ndlovu
Sign out
[aria-label: Breadcrumb]
Home
My progress
My authorisations
## My authorisations
The Statements of Awarded Responsibility (STAR) the committee has issued you, and their certificates.
No active authorisations yet
When a committee review ratifies a decision authorising you to perform an EPA, it will appear here. You can then download a STAR certificate for your records.
Three, one expiring (4.39)Lerato Molefe · one card form (C18); ISO dates; “Expires in 20 days” on the South African calendar (“Expires in 1 day” the day before, “Expires today” on the last day, S3); Download certificate named “Download certificate for PAED-010” (F4, M7); the cards an equal-column grid (S9)NEWR3
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
My progress
My authorisations
## My authorisations
The Statements of Awarded Responsibility (STAR) the committee has issued you, and their certificates.
### In force
#### PAED-001 — Providing paediatric emergency care to children
Authorised level
5
Issued
2026-10-13
Expires
No expiry
Leads resuscitation and emergency care without prompting; ready for unsupervised practice.
[aria-label: Download certificate for PAED-001] Download certificate
#### PAED-010 — Leading and operating within a clinical team
Authorised level
4
Issued
2026-10-13
Expires
2026-11-02 Expires in 20 days
Leads the ward round safely; one observation so far, to be confirmed at her final review.
[aria-label: Download certificate for PAED-010] Download certificate
#### PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
Authorised level
5
Issued
2026-10-13
Expires
No expiry
Breaks difficult news clearly and kindly; consistent across settings.
[aria-label: Download certificate for PAED-012] Download certificate
After a revocation (4.42)Anele Dlamini · PAED-002 moves to No longer in force: Revoked, since 2026-10-13, why in the reviser's name and words (Q8); “Download” named “Download certificate for PAED-002 (revoked)” (F4, M7)NEWdecided, T367R3
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
[aria-label: Breadcrumb]
Home
My progress
My authorisations
## My authorisations
The Statements of Awarded Responsibility (STAR) the committee has issued you, and their certificates.
### In force
#### PAED-001 — Providing paediatric emergency care to children
Authorised level
4
Issued
2026-10-13
Expires
No expiry
Manages the acutely ill child with the senior available on request.
[aria-label: Download certificate for PAED-001] Download certificate
### No longer in force
STARs that are no longer in force: revoked, or past their expiry
| EPA
| Level
| Status
| Since
| Why
| Actions
| PAED-002 — Managing common paediatric presentations
| 3b
| Revoked
| 2026-10-13
| Revoked by Refilwe Mokoena: Staged on evidence about PAED-001, not PAED-002; to be decided again.
[aria-label: Download certificate for PAED-002 (revoked)] Download
Illustration: an expired STAR (F2, S3)A states.md row only: the clock moved past 2026-11-02 on a scratch database; no journey step reads it · valid through its expiry date, so Since reads 2026-11-03NEWdecided, T367R3
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
My progress
My authorisations
## My authorisations
The Statements of Awarded Responsibility (STAR) the committee has issued you, and their certificates.
### In force
#### PAED-001 — Providing paediatric emergency care to children
Authorised level
5
Issued
2026-10-13
Expires
No expiry
Leads resuscitation and emergency care without prompting; ready for unsupervised practice.
[aria-label: Download certificate for PAED-001] Download certificate
#### PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
Authorised level
5
Issued
2026-10-13
Expires
No expiry
Breaks difficult news clearly and kindly; consistent across settings.
[aria-label: Download certificate for PAED-012] Download certificate
### No longer in force
STARs that are no longer in force: revoked, or past their expiry
| EPA
| Level
| Status
| Since
| Why
| Actions
| PAED-010 — Leading and operating within a clinical team
| 4
| Expired
| 2026-11-03
| Its expiry date, 2026-11-02, passed.
[aria-label: Download certificate for PAED-010 (expired)] Download
Illustration: every STAR revoked or expired (S2)A trainee with none in force · “In force” keeps its heading with its empty sentence, then No longer in forceNEWR3
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
[title: ‹trainee›] ‹trainee›
Sign out
[aria-label: Breadcrumb]
Home
My progress
My authorisations
## My authorisations
The Statements of Awarded Responsibility (STAR) the committee has issued you, and their certificates.
### In force
No STAR is in force now. When a committee review ratifies a decision authorising you to perform an EPA, it will appear here.
### No longer in force
STARs that are no longer in force: revoked, or past their expiry
| EPA
| Level
| Status
| Since
| Why
| Actions
| PAED-002 — Managing common paediatric presentations
| 3b
| Revoked
| 2026-10-13
| Revoked by Refilwe Mokoena: Staged on evidence about PAED-001, not PAED-002; to be decided again.
[aria-label: Download certificate for PAED-002 (revoked)] Download
| PAED-001 — Providing paediatric emergency care to children
| 4
| Expired
| ‹date›
| Its expiry date, ‹date›, passed.
[aria-label: Download certificate for PAED-001 (expired)] Download
A download running, then refused, then failed (C19)Lerato Molefe · “Generating…”, aria-disabled; a null result “The certificate could not be found.” beside its button; any other failure its fixed words with Try again (T272)Focus: the result, under its buttonNEW
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
My progress
My authorisations
## My authorisations
The Statements of Awarded Responsibility (STAR) the committee has issued you, and their certificates.
### In force
#### PAED-001 — Providing paediatric emergency care to children
Authorised level
5
Issued
2026-10-13
Expires
No expiry
Leads resuscitation and emergency care without prompting; ready for unsupervised practice.
[aria-label: Download certificate for PAED-001, generating] Generating…
#### PAED-010 — Leading and operating within a clinical team
Authorised level
4
Issued
2026-10-13
Expires
2026-11-02 Expires in 20 days
Leads the ward round safely; one observation so far, to be confirmed at her final review.
[aria-label: Download certificate for PAED-010] Download certificate
The certificate could not be found.
#### PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
Authorised level
5
Issued
2026-10-13
Expires
No expiry
Breaks difficult news clearly and kindly; consistent across settings.
[aria-label: Download certificate for PAED-012] Download certificate
The certificate could not be made just now.
Try again
LoadingThe header at once, a status line, card skeletons; no Download until the read is in
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
My progress
My authorisations
## My authorisations
The Statements of Awarded Responsibility (STAR) the committee has issued you, and their certificates.
Loading your authorisations.
Load error (T329)Fixed words with Try againFocus: the alert
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
My progress
My authorisations
## My authorisations
The Statements of Awarded Responsibility (STAR) the committee has issued you, and their certificates.
Could not load your authorisations. Nothing has changed. Try again, or come back in a few minutes.
Try again

######## BOARD R3-5-auth-1280-b.dc.html — My authorisations · 1280 · b
## My authorisations · 1280px · b: fifteen, and a pause
Route /portfolio/authorisations · Trainee · under My progress, its trail Home › My progress › My authorisations (Q10).
Components PageHeader, Breadcrumbs, DetailCard, .detail-list, Badge (BadgeFor.EntrustmentDecision), a hand-written stacked .clinic-table (S8), EpaLabel, StatePanel, Skeleton, Alert with Try again. NEW .star-grid (equal columns), .star-card, No longer in force, .download-result; the subtitle's words.
Rests on T367 decided, T367: her query widened to revoked and expired, with the reviser's name.
DESIGN.md the owner table's row for My authorisations (unchanged); § Badges, EntrustmentDecision (unchanged); § Accessibility, the label in the name (F4). Round 3 F2, F4, M7, S1, S2, S3, S9, S13.
Fifteen at exit level (5.8)Lerato Molefe · the same card for fifteen as for three, by EPA code; Act 5's rationales from the captures (S13); PAED-015's 120-character title wraps in its cardR3
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
My progress
My authorisations
## My authorisations
The Statements of Awarded Responsibility (STAR) the committee has issued you, and their certificates.
### In force
#### PAED-001 — Providing paediatric emergency care to children
Authorised level
5
Issued
2026-10-13
Expires
No expiry
Leads resuscitation and emergency care without prompting; ready for unsupervised practice.
[aria-label: Download certificate for PAED-001] Download certificate
#### PAED-002 — Managing common paediatric presentations
Authorised level
5
Issued
2026-10-13
Expires
No expiry
PAED-002 entrusted at its exit level; the panel recommends completion.
[aria-label: Download certificate for PAED-002] Download certificate
#### PAED-003 — Providing intensive care to children
Authorised level
5
Issued
2026-10-13
Expires
No expiry
PAED-003 entrusted at its exit level; the panel recommends completion.
[aria-label: Download certificate for PAED-003] Download certificate
#### PAED-004 — Managing common neonatal conditions
Authorised level
5
Issued
2026-10-13
Expires
No expiry
PAED-004 entrusted at its exit level; the panel recommends completion.
[aria-label: Download certificate for PAED-004] Download certificate
#### PAED-005 — Providing neonatal care in intensive and high-care settings
Authorised level
5
Issued
2026-10-13
Expires
No expiry
PAED-005 entrusted at its exit level; the panel recommends completion.
[aria-label: Download certificate for PAED-005] Download certificate
#### PAED-006 — Managing long-term health conditions (LTHCs)
Authorised level
5
Issued
2026-10-13
Expires
No expiry
PAED-006 entrusted at its exit level; the panel recommends completion.
[aria-label: Download certificate for PAED-006] Download certificate
#### PAED-007 — Maintaining and promoting the health and well-being of children
Authorised level
5
Issued
2026-10-13
Expires
No expiry
PAED-007 entrusted at its exit level; the panel recommends completion.
[aria-label: Download certificate for PAED-007] Download certificate
#### PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
Authorised level
4
Issued
2026-10-13
Expires
No expiry
PAED-008 entrusted at its exit level; the panel recommends completion.
[aria-label: Download certificate for PAED-008] Download certificate
#### PAED-009 — Providing care for an adolescent patient
Authorised level
4
Issued
2026-10-13
Expires
No expiry
PAED-009 entrusted at its exit level; the panel recommends completion.
[aria-label: Download certificate for PAED-009] Download certificate
#### PAED-010 — Leading and operating within a clinical team
Authorised level
5
Issued
2026-10-13
Expires
No expiry
PAED-010 entrusted at its exit level; the panel recommends completion.
[aria-label: Download certificate for PAED-010] Download certificate
#### PAED-011 — Managing population health challenges
Authorised level
4
Issued
2026-10-13
Expires
No expiry
PAED-011 entrusted at its exit level; the panel recommends completion.
[aria-label: Download certificate for PAED-011] Download certificate
#### PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
Authorised level
5
Issued
2026-10-13
Expires
No expiry
Breaks difficult news clearly and kindly; consistent across settings.
[aria-label: Download certificate for PAED-012] Download certificate
#### PAED-013 — Managing child protection, legal and ethical aspects of children at risk
Authorised level
4
Issued
2026-10-13
Expires
No expiry
PAED-013 entrusted at its exit level; the panel recommends completion.
[aria-label: Download certificate for PAED-013] Download certificate
#### PAED-014 — Providing palliative and end-of-life care
Authorised level
4
Issued
2026-10-13
Expires
No expiry
PAED-014 entrusted at its exit level; the panel recommends completion.
[aria-label: Download certificate for PAED-014] Download certificate
#### PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
Authorised level
4
Issued
2026-10-13
Expires
No expiry
PAED-015 entrusted at its exit level; the panel recommends completion.
[aria-label: Download certificate for PAED-015] Download certificate
A paused EPA's STAR (6.22)Lerato Molefe after Act 6 · PAED-012's card keeps its place: a pause leaves a STAR standing; “(no longer in use)” muted after its title (EpaLabel, T255)
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
My progress
My authorisations
## My authorisations
The Statements of Awarded Responsibility (STAR) the committee has issued you, and their certificates.
### In force
#### PAED-001 — Providing paediatric emergency care to children
Authorised level
5
Issued
2026-10-13
Expires
No expiry
Leads resuscitation and emergency care without prompting; ready for unsupervised practice.
[aria-label: Download certificate for PAED-001] Download certificate
#### PAED-002 — Managing common paediatric presentations
Authorised level
5
Issued
2026-10-13
Expires
No expiry
PAED-002 entrusted at its exit level; the panel recommends completion.
[aria-label: Download certificate for PAED-002] Download certificate
#### PAED-003 — Providing intensive care to children
Authorised level
5
Issued
2026-10-13
Expires
No expiry
PAED-003 entrusted at its exit level; the panel recommends completion.
[aria-label: Download certificate for PAED-003] Download certificate
#### PAED-004 — Managing common neonatal conditions
Authorised level
5
Issued
2026-10-13
Expires
No expiry
PAED-004 entrusted at its exit level; the panel recommends completion.
[aria-label: Download certificate for PAED-004] Download certificate
#### PAED-005 — Providing neonatal care in intensive and high-care settings
Authorised level
5
Issued
2026-10-13
Expires
No expiry
PAED-005 entrusted at its exit level; the panel recommends completion.
[aria-label: Download certificate for PAED-005] Download certificate
#### PAED-006 — Managing long-term health conditions (LTHCs)
Authorised level
5
Issued
2026-10-13
Expires
No expiry
PAED-006 entrusted at its exit level; the panel recommends completion.
[aria-label: Download certificate for PAED-006] Download certificate
#### PAED-007 — Maintaining and promoting the health and well-being of children
Authorised level
5
Issued
2026-10-13
Expires
No expiry
PAED-007 entrusted at its exit level; the panel recommends completion.
[aria-label: Download certificate for PAED-007] Download certificate
#### PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
Authorised level
4
Issued
2026-10-13
Expires
No expiry
PAED-008 entrusted at its exit level; the panel recommends completion.
[aria-label: Download certificate for PAED-008] Download certificate
#### PAED-009 — Providing care for an adolescent patient
Authorised level
4
Issued
2026-10-13
Expires
No expiry
PAED-009 entrusted at its exit level; the panel recommends completion.
[aria-label: Download certificate for PAED-009] Download certificate
#### PAED-010 — Leading and operating within a clinical team
Authorised level
5
Issued
2026-10-13
Expires
No expiry
PAED-010 entrusted at its exit level; the panel recommends completion.
[aria-label: Download certificate for PAED-010] Download certificate
#### PAED-011 — Managing population health challenges
Authorised level
4
Issued
2026-10-13
Expires
No expiry
PAED-011 entrusted at its exit level; the panel recommends completion.
[aria-label: Download certificate for PAED-011] Download certificate
#### PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams (no longer in use)
Authorised level
5
Issued
2026-10-13
Expires
No expiry
Breaks difficult news clearly and kindly; consistent across settings.
[aria-label: Download certificate for PAED-012] Download certificate
#### PAED-013 — Managing child protection, legal and ethical aspects of children at risk
Authorised level
4
Issued
2026-10-13
Expires
No expiry
PAED-013 entrusted at its exit level; the panel recommends completion.
[aria-label: Download certificate for PAED-013] Download certificate
#### PAED-014 — Providing palliative and end-of-life care
Authorised level
4
Issued
2026-10-13
Expires
No expiry
PAED-014 entrusted at its exit level; the panel recommends completion.
[aria-label: Download certificate for PAED-014] Download certificate
#### PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
Authorised level
4
Issued
2026-10-13
Expires
No expiry
PAED-015 entrusted at its exit level; the panel recommends completion.
[aria-label: Download certificate for PAED-015] Download certificate

######## BOARD R3-5-auth-768.dc.html — My authorisations · 768
## My authorisations · 768px (S1, S9)
Route /portfolio/authorisations · Trainee · under My progress, its trail Home › My progress › My authorisations (Q10).
Components PageHeader, Breadcrumbs, DetailCard, .detail-list, Badge (BadgeFor.EntrustmentDecision), a hand-written stacked .clinic-table (S8), EpaLabel, StatePanel, Skeleton, Alert with Try again. NEW .star-grid (equal columns), .star-card, No longer in force, .download-result; the subtitle's words.
Rests on T367 decided, T367: her query widened to revoked and expired, with the reviser's name.
DESIGN.md the owner table's row for My authorisations (unchanged); § Badges, EntrustmentDecision (unchanged); § Accessibility, the label in the name (F4). Round 3 F2, F4, M7, S1, S2, S3, S9, S13.
Three, one expiring (4.39)Lerato Molefe · one card form (C18); ISO dates; “Expires in 20 days” on the South African calendar (“Expires in 1 day” the day before, “Expires today” on the last day, S3); Download certificate named “Download certificate for PAED-010” (F4, M7); the cards an equal-column grid (S9)NEWR3
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
My progress
My authorisations
## My authorisations
The Statements of Awarded Responsibility (STAR) the committee has issued you, and their certificates.
### In force
#### PAED-001 — Providing paediatric emergency care to children
Authorised level
5
Issued
2026-10-13
Expires
No expiry
Leads resuscitation and emergency care without prompting; ready for unsupervised practice.
[aria-label: Download certificate for PAED-001] Download certificate
#### PAED-010 — Leading and operating within a clinical team
Authorised level
4
Issued
2026-10-13
Expires
2026-11-02 Expires in 20 days
Leads the ward round safely; one observation so far, to be confirmed at her final review.
[aria-label: Download certificate for PAED-010] Download certificate
#### PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
Authorised level
5
Issued
2026-10-13
Expires
No expiry
Breaks difficult news clearly and kindly; consistent across settings.
[aria-label: Download certificate for PAED-012] Download certificate
After a revocation (4.42)Anele Dlamini · PAED-002 moves to No longer in force: Revoked, since 2026-10-13, why in the reviser's name and words (Q8); “Download” named “Download certificate for PAED-002 (revoked)” (F4, M7)NEWdecided, T367R3
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
[aria-label: Breadcrumb]
Home
My progress
My authorisations
## My authorisations
The Statements of Awarded Responsibility (STAR) the committee has issued you, and their certificates.
### In force
#### PAED-001 — Providing paediatric emergency care to children
Authorised level
4
Issued
2026-10-13
Expires
No expiry
Manages the acutely ill child with the senior available on request.
[aria-label: Download certificate for PAED-001] Download certificate
### No longer in force
STARs that are no longer in force: revoked, or past their expiry
| EPA
| Level
| Status
| Since
| Why
| Actions
| PAED-002 — Managing common paediatric presentations
| 3b
| Revoked
| 2026-10-13
| Revoked by Refilwe Mokoena: Staged on evidence about PAED-001, not PAED-002; to be decided again.
[aria-label: Download certificate for PAED-002 (revoked)] Download
Fifteen at exit level (5.8)Lerato Molefe · the same card for fifteen as for three, by EPA code; Act 5's rationales from the captures (S13); PAED-015's 120-character title wraps in its cardR3
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
My progress
My authorisations
## My authorisations
The Statements of Awarded Responsibility (STAR) the committee has issued you, and their certificates.
### In force
#### PAED-001 — Providing paediatric emergency care to children
Authorised level
5
Issued
2026-10-13
Expires
No expiry
Leads resuscitation and emergency care without prompting; ready for unsupervised practice.
[aria-label: Download certificate for PAED-001] Download certificate
#### PAED-002 — Managing common paediatric presentations
Authorised level
5
Issued
2026-10-13
Expires
No expiry
PAED-002 entrusted at its exit level; the panel recommends completion.
[aria-label: Download certificate for PAED-002] Download certificate
#### PAED-003 — Providing intensive care to children
Authorised level
5
Issued
2026-10-13
Expires
No expiry
PAED-003 entrusted at its exit level; the panel recommends completion.
[aria-label: Download certificate for PAED-003] Download certificate
#### PAED-004 — Managing common neonatal conditions
Authorised level
5
Issued
2026-10-13
Expires
No expiry
PAED-004 entrusted at its exit level; the panel recommends completion.
[aria-label: Download certificate for PAED-004] Download certificate
#### PAED-005 — Providing neonatal care in intensive and high-care settings
Authorised level
5
Issued
2026-10-13
Expires
No expiry
PAED-005 entrusted at its exit level; the panel recommends completion.
[aria-label: Download certificate for PAED-005] Download certificate
#### PAED-006 — Managing long-term health conditions (LTHCs)
Authorised level
5
Issued
2026-10-13
Expires
No expiry
PAED-006 entrusted at its exit level; the panel recommends completion.
[aria-label: Download certificate for PAED-006] Download certificate
#### PAED-007 — Maintaining and promoting the health and well-being of children
Authorised level
5
Issued
2026-10-13
Expires
No expiry
PAED-007 entrusted at its exit level; the panel recommends completion.
[aria-label: Download certificate for PAED-007] Download certificate
#### PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
Authorised level
4
Issued
2026-10-13
Expires
No expiry
PAED-008 entrusted at its exit level; the panel recommends completion.
[aria-label: Download certificate for PAED-008] Download certificate
#### PAED-009 — Providing care for an adolescent patient
Authorised level
4
Issued
2026-10-13
Expires
No expiry
PAED-009 entrusted at its exit level; the panel recommends completion.
[aria-label: Download certificate for PAED-009] Download certificate
#### PAED-010 — Leading and operating within a clinical team
Authorised level
5
Issued
2026-10-13
Expires
No expiry
PAED-010 entrusted at its exit level; the panel recommends completion.
[aria-label: Download certificate for PAED-010] Download certificate
#### PAED-011 — Managing population health challenges
Authorised level
4
Issued
2026-10-13
Expires
No expiry
PAED-011 entrusted at its exit level; the panel recommends completion.
[aria-label: Download certificate for PAED-011] Download certificate
#### PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
Authorised level
5
Issued
2026-10-13
Expires
No expiry
Breaks difficult news clearly and kindly; consistent across settings.
[aria-label: Download certificate for PAED-012] Download certificate
#### PAED-013 — Managing child protection, legal and ethical aspects of children at risk
Authorised level
4
Issued
2026-10-13
Expires
No expiry
PAED-013 entrusted at its exit level; the panel recommends completion.
[aria-label: Download certificate for PAED-013] Download certificate
#### PAED-014 — Providing palliative and end-of-life care
Authorised level
4
Issued
2026-10-13
Expires
No expiry
PAED-014 entrusted at its exit level; the panel recommends completion.
[aria-label: Download certificate for PAED-014] Download certificate
#### PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
Authorised level
4
Issued
2026-10-13
Expires
No expiry
PAED-015 entrusted at its exit level; the panel recommends completion.
[aria-label: Download certificate for PAED-015] Download certificate

######## BOARD R3-5-auth-390.dc.html — My authorisations · 390
## My authorisations · 390px
Route /portfolio/authorisations · Trainee · under My progress, its trail Home › My progress › My authorisations (Q10).
Components PageHeader, Breadcrumbs, DetailCard, .detail-list, Badge (BadgeFor.EntrustmentDecision), a hand-written stacked .clinic-table (S8), EpaLabel, StatePanel, Skeleton, Alert with Try again. NEW .star-grid (equal columns), .star-card, No longer in force, .download-result; the subtitle's words.
Rests on T367 decided, T367: her query widened to revoked and expired, with the reviser's name.
DESIGN.md the owner table's row for My authorisations (unchanged); § Badges, EntrustmentDecision (unchanged); § Accessibility, the label in the name (F4). Round 3 F2, F4, M7, S1, S2, S3, S9, S13.
None yetSipho Ndlovu · StatePanel's empty card, today's words; no “No longer in force” (hidden when empty, C18)Focus: the h1
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
[title: Sipho Ndlovu] Sipho Ndlovu
Sign out
[aria-label: Breadcrumb]
Home
My progress
My authorisations
## My authorisations
The Statements of Awarded Responsibility (STAR) the committee has issued you, and their certificates.
No active authorisations yet
When a committee review ratifies a decision authorising you to perform an EPA, it will appear here. You can then download a STAR certificate for your records.
Three, one expiring (4.39)Lerato Molefe · one card form (C18); ISO dates; “Expires in 20 days” on the South African calendar (“Expires in 1 day” the day before, “Expires today” on the last day, S3); Download certificate named “Download certificate for PAED-010” (F4, M7); the cards an equal-column grid (S9)NEWR3
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
My progress
My authorisations
## My authorisations
The Statements of Awarded Responsibility (STAR) the committee has issued you, and their certificates.
### In force
#### PAED-001 — Providing paediatric emergency care to children
Authorised level
5
Issued
2026-10-13
Expires
No expiry
Leads resuscitation and emergency care without prompting; ready for unsupervised practice.
[aria-label: Download certificate for PAED-001] Download certificate
#### PAED-010 — Leading and operating within a clinical team
Authorised level
4
Issued
2026-10-13
Expires
2026-11-02 Expires in 20 days
Leads the ward round safely; one observation so far, to be confirmed at her final review.
[aria-label: Download certificate for PAED-010] Download certificate
#### PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
Authorised level
5
Issued
2026-10-13
Expires
No expiry
Breaks difficult news clearly and kindly; consistent across settings.
[aria-label: Download certificate for PAED-012] Download certificate
After a revocation (4.42)Anele Dlamini · PAED-002 moves to No longer in force: Revoked, since 2026-10-13, why in the reviser's name and words (Q8); “Download” named “Download certificate for PAED-002 (revoked)” (F4, M7)NEWdecided, T367R3
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
[aria-label: Breadcrumb]
Home
My progress
My authorisations
## My authorisations
The Statements of Awarded Responsibility (STAR) the committee has issued you, and their certificates.
### In force
#### PAED-001 — Providing paediatric emergency care to children
Authorised level
4
Issued
2026-10-13
Expires
No expiry
Manages the acutely ill child with the senior available on request.
[aria-label: Download certificate for PAED-001] Download certificate
### No longer in force
STARs that are no longer in force: revoked, or past their expiry
| EPA
| Level
| Status
| Since
| Why
| Actions
| PAED-002 — Managing common paediatric presentations
| 3b
| Revoked
| 2026-10-13
| Revoked by Refilwe Mokoena: Staged on evidence about PAED-001, not PAED-002; to be decided again.
[aria-label: Download certificate for PAED-002 (revoked)] Download
Illustration: an expired STAR (F2, S3)A states.md row only: the clock moved past 2026-11-02 on a scratch database; no journey step reads it · valid through its expiry date, so Since reads 2026-11-03NEWdecided, T367R3
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
My progress
My authorisations
## My authorisations
The Statements of Awarded Responsibility (STAR) the committee has issued you, and their certificates.
### In force
#### PAED-001 — Providing paediatric emergency care to children
Authorised level
5
Issued
2026-10-13
Expires
No expiry
Leads resuscitation and emergency care without prompting; ready for unsupervised practice.
[aria-label: Download certificate for PAED-001] Download certificate
#### PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
Authorised level
5
Issued
2026-10-13
Expires
No expiry
Breaks difficult news clearly and kindly; consistent across settings.
[aria-label: Download certificate for PAED-012] Download certificate
### No longer in force
STARs that are no longer in force: revoked, or past their expiry
| EPA
| Level
| Status
| Since
| Why
| Actions
| PAED-010 — Leading and operating within a clinical team
| 4
| Expired
| 2026-11-03
| Its expiry date, 2026-11-02, passed.
[aria-label: Download certificate for PAED-010 (expired)] Download
Illustration: every STAR revoked or expired (S2)A trainee with none in force · “In force” keeps its heading with its empty sentence, then No longer in forceNEWR3
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
[title: ‹trainee›] ‹trainee›
Sign out
[aria-label: Breadcrumb]
Home
My progress
My authorisations
## My authorisations
The Statements of Awarded Responsibility (STAR) the committee has issued you, and their certificates.
### In force
No STAR is in force now. When a committee review ratifies a decision authorising you to perform an EPA, it will appear here.
### No longer in force
STARs that are no longer in force: revoked, or past their expiry
| EPA
| Level
| Status
| Since
| Why
| Actions
| PAED-002 — Managing common paediatric presentations
| 3b
| Revoked
| 2026-10-13
| Revoked by Refilwe Mokoena: Staged on evidence about PAED-001, not PAED-002; to be decided again.
[aria-label: Download certificate for PAED-002 (revoked)] Download
| PAED-001 — Providing paediatric emergency care to children
| 4
| Expired
| ‹date›
| Its expiry date, ‹date›, passed.
[aria-label: Download certificate for PAED-001 (expired)] Download
A download running, then refused, then failed (C19)Lerato Molefe · “Generating…”, aria-disabled; a null result “The certificate could not be found.” beside its button; any other failure its fixed words with Try again (T272)Focus: the result, under its buttonNEW
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
My progress
My authorisations
## My authorisations
The Statements of Awarded Responsibility (STAR) the committee has issued you, and their certificates.
### In force
#### PAED-001 — Providing paediatric emergency care to children
Authorised level
5
Issued
2026-10-13
Expires
No expiry
Leads resuscitation and emergency care without prompting; ready for unsupervised practice.
[aria-label: Download certificate for PAED-001, generating] Generating…
#### PAED-010 — Leading and operating within a clinical team
Authorised level
4
Issued
2026-10-13
Expires
2026-11-02 Expires in 20 days
Leads the ward round safely; one observation so far, to be confirmed at her final review.
[aria-label: Download certificate for PAED-010] Download certificate
The certificate could not be found.
#### PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
Authorised level
5
Issued
2026-10-13
Expires
No expiry
Breaks difficult news clearly and kindly; consistent across settings.
[aria-label: Download certificate for PAED-012] Download certificate
The certificate could not be made just now.
Try again
LoadingThe header at once, a status line, card skeletons; no Download until the read is in
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
My progress
My authorisations
## My authorisations
The Statements of Awarded Responsibility (STAR) the committee has issued you, and their certificates.
Loading your authorisations.
Load error (T329)Fixed words with Try againFocus: the alert
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
My progress
My authorisations
## My authorisations
The Statements of Awarded Responsibility (STAR) the committee has issued you, and their certificates.
Could not load your authorisations. Nothing has changed. Try again, or come back in a few minutes.
Try again
Fifteen at exit level (5.8)Lerato Molefe · the same card for fifteen as for three, by EPA code; Act 5's rationales from the captures (S13); PAED-015's 120-character title wraps in its cardR3
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
My progress
My authorisations
## My authorisations
The Statements of Awarded Responsibility (STAR) the committee has issued you, and their certificates.
### In force
#### PAED-001 — Providing paediatric emergency care to children
Authorised level
5
Issued
2026-10-13
Expires
No expiry
Leads resuscitation and emergency care without prompting; ready for unsupervised practice.
[aria-label: Download certificate for PAED-001] Download certificate
#### PAED-002 — Managing common paediatric presentations
Authorised level
5
Issued
2026-10-13
Expires
No expiry
PAED-002 entrusted at its exit level; the panel recommends completion.
[aria-label: Download certificate for PAED-002] Download certificate
#### PAED-003 — Providing intensive care to children
Authorised level
5
Issued
2026-10-13
Expires
No expiry
PAED-003 entrusted at its exit level; the panel recommends completion.
[aria-label: Download certificate for PAED-003] Download certificate
#### PAED-004 — Managing common neonatal conditions
Authorised level
5
Issued
2026-10-13
Expires
No expiry
PAED-004 entrusted at its exit level; the panel recommends completion.
[aria-label: Download certificate for PAED-004] Download certificate
#### PAED-005 — Providing neonatal care in intensive and high-care settings
Authorised level
5
Issued
2026-10-13
Expires
No expiry
PAED-005 entrusted at its exit level; the panel recommends completion.
[aria-label: Download certificate for PAED-005] Download certificate
#### PAED-006 — Managing long-term health conditions (LTHCs)
Authorised level
5
Issued
2026-10-13
Expires
No expiry
PAED-006 entrusted at its exit level; the panel recommends completion.
[aria-label: Download certificate for PAED-006] Download certificate
#### PAED-007 — Maintaining and promoting the health and well-being of children
Authorised level
5
Issued
2026-10-13
Expires
No expiry
PAED-007 entrusted at its exit level; the panel recommends completion.
[aria-label: Download certificate for PAED-007] Download certificate
#### PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
Authorised level
4
Issued
2026-10-13
Expires
No expiry
PAED-008 entrusted at its exit level; the panel recommends completion.
[aria-label: Download certificate for PAED-008] Download certificate
#### PAED-009 — Providing care for an adolescent patient
Authorised level
4
Issued
2026-10-13
Expires
No expiry
PAED-009 entrusted at its exit level; the panel recommends completion.
[aria-label: Download certificate for PAED-009] Download certificate
#### PAED-010 — Leading and operating within a clinical team
Authorised level
5
Issued
2026-10-13
Expires
No expiry
PAED-010 entrusted at its exit level; the panel recommends completion.
[aria-label: Download certificate for PAED-010] Download certificate
#### PAED-011 — Managing population health challenges
Authorised level
4
Issued
2026-10-13
Expires
No expiry
PAED-011 entrusted at its exit level; the panel recommends completion.
[aria-label: Download certificate for PAED-011] Download certificate
#### PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
Authorised level
5
Issued
2026-10-13
Expires
No expiry
Breaks difficult news clearly and kindly; consistent across settings.
[aria-label: Download certificate for PAED-012] Download certificate
#### PAED-013 — Managing child protection, legal and ethical aspects of children at risk
Authorised level
4
Issued
2026-10-13
Expires
No expiry
PAED-013 entrusted at its exit level; the panel recommends completion.
[aria-label: Download certificate for PAED-013] Download certificate
#### PAED-014 — Providing palliative and end-of-life care
Authorised level
4
Issued
2026-10-13
Expires
No expiry
PAED-014 entrusted at its exit level; the panel recommends completion.
[aria-label: Download certificate for PAED-014] Download certificate
#### PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
Authorised level
4
Issued
2026-10-13
Expires
No expiry
PAED-015 entrusted at its exit level; the panel recommends completion.
[aria-label: Download certificate for PAED-015] Download certificate

######## BOARD R3-6-register-1280-a.dc.html — Register · 1280 · a
## The STAR register · 1280px · a
Route /admin/entrustment-decisions · Administrator, Institutional admin, Speciality and Sub-speciality admin · lit for the two speciality admins (no trail); Home › Entrustment decisions for the others.
Components PageHeader, the FilterBar pattern (search-container; Show, Clear filters, F9), a hand-written .clinic-table with both stack classes (S8), Badge (Active green, Expired amber, Revoked red, Superseded grey; labels kept), ConfirmDialog CHANGED (a field takes the first focus; a refusal keeps it open, M5), Alert, ActionResult, StatePanel, EpaLabel, .actions-cell. NEW flow 03's note row generalised (.has-note / .history-note), .issued-review, the status count.
Rests on T367 decided, T367: the mail the dialog announces; the reviser's name in the row and on the certificate (T324).
DESIGN.md § Table system (S8, the note row); § Button system (M5); § Accessibility (S11). Round 3 F3, F9, M4, M6, M9, M10, S1, S4, S6, S7, S11, nits.
None issuedRefilwe Mokoena before any review is ratified · no filter form when there is nothing to filter (FilterBar's rule)Focus: the h1
Wombat
Acting as Speciality admin
MenuClose
[aria-label: Main]
Acting as Speciality admin
Home
Programme trainees
Waiting for assessors
Decisions due
Committee reviews
Decision panels
Entrustment decisions
My data rights
[title: Refilwe Mokoena] Refilwe Mokoena
Sign out
## Entrustment decisions
Issued Statements of Awarded Responsibility across the programme.
No STARs issued yet
When a committee ratifies an entrustment decision, its STAR is listed here.
Five issued (4.35)Nolwazi Mbatha from her Home's quick links: no menu item for her, so the trail Home › Entrustment decisions · newest issued first, then STAR number descending; Issued's link “review #2” named “Open review #2”, 24px tall (M9); Revoke an outline button named for its rowNEWR3
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Entrustment decisions
## Entrustment decisions
Issued Statements of Awarded Responsibility across the programme.
[aria-label: Filter Entrustment decisions]
Trainee
[placeholder: Filter by name]
Status
All
Active
Expired
Revoked
Superseded
Show
Issued STARs (entrustment decisions) · 5 shown
| Trainee
| EPA
| Level
| Issued
| Expires
| Status
| Actions
| Anele Dlamini
| PAED-002 — Managing common paediatric presentations
| 3b
| 2026-10-13
[aria-label: Open review #2] review #2
| —
| Active
[aria-label: Download the certificate of Anele Dlamini's PAED-002 decision, issued 2026-10-13] Download
[aria-label: Revoke Anele Dlamini's PAED-002 decision, issued 2026-10-13] Revoke
| Anele Dlamini
| PAED-001 — Providing paediatric emergency care to children
| 4
| 2026-10-13
[aria-label: Open review #2] review #2
| —
| Active
[aria-label: Download the certificate of Anele Dlamini's PAED-001 decision, issued 2026-10-13] Download
[aria-label: Revoke Anele Dlamini's PAED-001 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
| 5
| 2026-10-13
[aria-label: Open review #1] review #1
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-012 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-012 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-010 — Leading and operating within a clinical team
| 4
| 2026-10-13
[aria-label: Open review #1] review #1
| 2026-11-02
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-010 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-010 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-001 — Providing paediatric emergency care to children
| 5
| 2026-10-13
[aria-label: Open review #1] review #1
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-001 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-001 decision, issued 2026-10-13] Revoke
Filtered to Molefe, after Show (4.35, F9, S11)The Trainee field binds on change; Show applies it; Clear filters once a filter is set; a visually hidden status reads “3 shown”Focus: stays on ShowR3
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Entrustment decisions
## Entrustment decisions
Issued Statements of Awarded Responsibility across the programme.
[aria-label: Filter Entrustment decisions]
Trainee
[placeholder: Filter by name] [value: Molefe]
Status
All
Active
Expired
Revoked
Superseded
Show
Clear filters
3 shown
Issued STARs (entrustment decisions) · 3 shown
| Trainee
| EPA
| Level
| Issued
| Expires
| Status
| Actions
| Lerato Molefe
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
| 5
| 2026-10-13
[aria-label: Open review #1] review #1
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-012 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-012 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-010 — Leading and operating within a clinical team
| 4
| 2026-10-13
[aria-label: Open review #1] review #1
| 2026-11-02
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-010 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-010 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-001 — Providing paediatric emergency care to children
| 5
| 2026-10-13
[aria-label: Open review #1] review #1
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-001 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-001 decision, issued 2026-10-13] Revoke
Filtered to nothingNolwazi Mbatha · “Mahlangu”, Revoked: the count stays, then the card with Clear filters
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Entrustment decisions
## Entrustment decisions
Issued Statements of Awarded Responsibility across the programme.
[aria-label: Filter Entrustment decisions]
Trainee
[placeholder: Filter by name] [value: Mahlangu]
Status
All
Active
Expired
Revoked
Superseded
Show
Clear filters
Issued STARs (entrustment decisions) · 0 shown
No STAR matches these filters.
Trainee “Mahlangu”, status Revoked.
Clear filters
Issued as plain text (S7)Illustration: a row whose issuing review the viewer may not open (IssuingReviewReadable false, e.g. another panel's) · “review #2” is text, not a linkR3
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Entrustment decisions
## Entrustment decisions
Issued Statements of Awarded Responsibility across the programme.
[aria-label: Filter Entrustment decisions]
Trainee
[placeholder: Filter by name]
Status
All
Active
Expired
Revoked
Superseded
Show
Issued STARs (entrustment decisions) · 5 shown
| Trainee
| EPA
| Level
| Issued
| Expires
| Status
| Actions
| Anele Dlamini
| PAED-002 — Managing common paediatric presentations
| 3b
| 2026-10-13
review #2
| —
| Active
[aria-label: Download the certificate of Anele Dlamini's PAED-002 decision, issued 2026-10-13] Download
[aria-label: Revoke Anele Dlamini's PAED-002 decision, issued 2026-10-13] Revoke
| Anele Dlamini
| PAED-001 — Providing paediatric emergency care to children
| 4
| 2026-10-13
[aria-label: Open review #2] review #2
| —
| Active
[aria-label: Download the certificate of Anele Dlamini's PAED-001 decision, issued 2026-10-13] Download
[aria-label: Revoke Anele Dlamini's PAED-001 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
| 5
| 2026-10-13
[aria-label: Open review #1] review #1
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-012 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-012 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-010 — Leading and operating within a clinical team
| 4
| 2026-10-13
[aria-label: Open review #1] review #1
| 2026-11-02
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-010 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-010 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-001 — Providing paediatric emergency care to children
| 5
| 2026-10-13
[aria-label: Open review #1] review #1
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-001 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-001 decision, issued 2026-10-13] Revoke

######## BOARD R3-6-register-1280-b.dc.html — Register · 1280 · b
## The STAR register · 1280px · b: revoking
Route /admin/entrustment-decisions · Administrator, Institutional admin, Speciality and Sub-speciality admin · lit for the two speciality admins (no trail); Home › Entrustment decisions for the others.
Components PageHeader, the FilterBar pattern (search-container; Show, Clear filters, F9), a hand-written .clinic-table with both stack classes (S8), Badge (Active green, Expired amber, Revoked red, Superseded grey; labels kept), ConfirmDialog CHANGED (a field takes the first focus; a refusal keeps it open, M5), Alert, ActionResult, StatePanel, EpaLabel, .actions-cell. NEW flow 03's note row generalised (.has-note / .history-note), .issued-review, the status count.
Rests on T367 decided, T367: the mail the dialog announces; the reviser's name in the row and on the certificate (T324).
DESIGN.md § Table system (S8, the note row); § Button system (M5); § Accessibility (S11). Round 3 F3, F9, M4, M6, M9, M10, S1, S4, S6, S7, S11, nits.
Revoke pressed (4.36, F3, M6)Refilwe Mokoena · ConfirmDialog with a field: the focus in the reason box; Revoke the STAR enabled with the box empty; the mail sentence true when T367 skips a deactivated account (S6)Focus: the reason boxPROPOSEDdecided, T367R3
Wombat
Acting as Speciality admin
MenuClose
[aria-label: Main]
Acting as Speciality admin
Home
Programme trainees
Waiting for assessors
Decisions due
Committee reviews
Decision panels
Entrustment decisions
My data rights
[title: Refilwe Mokoena] Refilwe Mokoena
Sign out
## Entrustment decisions
Issued Statements of Awarded Responsibility across the programme.
[aria-label: Filter Entrustment decisions]
Trainee
[placeholder: Filter by name]
Status
All
Active
Expired
Revoked
Superseded
Show
Issued STARs (entrustment decisions) · 5 shown
| Trainee
| EPA
| Level
| Issued
| Expires
| Status
| Actions
| Anele Dlamini
| PAED-002 — Managing common paediatric presentations
| 3b
| 2026-10-13
[aria-label: Open review #2] review #2
| —
| Active
[aria-label: Download the certificate of Anele Dlamini's PAED-002 decision, issued 2026-10-13] Download
[aria-label: Revoke Anele Dlamini's PAED-002 decision, issued 2026-10-13] Revoke
| Anele Dlamini
| PAED-001 — Providing paediatric emergency care to children
| 4
| 2026-10-13
[aria-label: Open review #2] review #2
| —
| Active
[aria-label: Download the certificate of Anele Dlamini's PAED-001 decision, issued 2026-10-13] Download
[aria-label: Revoke Anele Dlamini's PAED-001 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
| 5
| 2026-10-13
[aria-label: Open review #1] review #1
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-012 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-012 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-010 — Leading and operating within a clinical team
| 4
| 2026-10-13
[aria-label: Open review #1] review #1
| 2026-11-02
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-010 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-010 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-001 — Providing paediatric emergency care to children
| 5
| 2026-10-13
[aria-label: Open review #1] review #1
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-001 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-001 decision, issued 2026-10-13] Revoke
### Revoke Anele Dlamini's PAED-002 STAR?
PAED-002 — Managing common paediatric presentations, level 3b, issued 2026-10-13. Revocation is immediate and cannot be undone.
Anele Dlamini is sent an email saying the STAR was revoked and by whom, with a link to My authorisations, unless her account is deactivated. The reason is not in the email: she reads it on My authorisations and on the certificate.
The EPA reads 'Revoked: re-decide' on Decisions due, and her review's agenda keeps its line.
Revocation reason *required
0 of 1,000 characters
Cancel
Revoke the STAR
Revoke the STAR pressed blank (F3, M6)Nothing is sent; “Give a reason for the revocation.” under the box, tied to it by aria-describedby; the dialog stays openFocus: the reason boxR3
Wombat
Acting as Speciality admin
MenuClose
[aria-label: Main]
Acting as Speciality admin
Home
Programme trainees
Waiting for assessors
Decisions due
Committee reviews
Decision panels
Entrustment decisions
My data rights
[title: Refilwe Mokoena] Refilwe Mokoena
Sign out
## Entrustment decisions
Issued Statements of Awarded Responsibility across the programme.
[aria-label: Filter Entrustment decisions]
Trainee
[placeholder: Filter by name]
Status
All
Active
Expired
Revoked
Superseded
Show
Issued STARs (entrustment decisions) · 5 shown
| Trainee
| EPA
| Level
| Issued
| Expires
| Status
| Actions
| Anele Dlamini
| PAED-002 — Managing common paediatric presentations
| 3b
| 2026-10-13
[aria-label: Open review #2] review #2
| —
| Active
[aria-label: Download the certificate of Anele Dlamini's PAED-002 decision, issued 2026-10-13] Download
[aria-label: Revoke Anele Dlamini's PAED-002 decision, issued 2026-10-13] Revoke
| Anele Dlamini
| PAED-001 — Providing paediatric emergency care to children
| 4
| 2026-10-13
[aria-label: Open review #2] review #2
| —
| Active
[aria-label: Download the certificate of Anele Dlamini's PAED-001 decision, issued 2026-10-13] Download
[aria-label: Revoke Anele Dlamini's PAED-001 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
| 5
| 2026-10-13
[aria-label: Open review #1] review #1
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-012 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-012 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-010 — Leading and operating within a clinical team
| 4
| 2026-10-13
[aria-label: Open review #1] review #1
| 2026-11-02
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-010 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-010 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-001 — Providing paediatric emergency care to children
| 5
| 2026-10-13
[aria-label: Open review #1] review #1
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-001 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-001 decision, issued 2026-10-13] Revoke
### Revoke Anele Dlamini's PAED-002 STAR?
PAED-002 — Managing common paediatric presentations, level 3b, issued 2026-10-13. Revocation is immediate and cannot be undone.
Anele Dlamini is sent an email saying the STAR was revoked and by whom, with a link to My authorisations, unless her account is deactivated. The reason is not in the email: she reads it on My authorisations and on the certificate.
The EPA reads 'Revoked: re-decide' on Decisions due, and her review's agenda keeps its line.
Revocation reason *required
Give a reason for the revocation.
0 of 1,000 characters
Cancel
Revoke the STAR
The reason typed (4.36)69 of 1,000; Revoke the STARFocus: the reason box
Wombat
Acting as Speciality admin
MenuClose
[aria-label: Main]
Acting as Speciality admin
Home
Programme trainees
Waiting for assessors
Decisions due
Committee reviews
Decision panels
Entrustment decisions
My data rights
[title: Refilwe Mokoena] Refilwe Mokoena
Sign out
## Entrustment decisions
Issued Statements of Awarded Responsibility across the programme.
[aria-label: Filter Entrustment decisions]
Trainee
[placeholder: Filter by name]
Status
All
Active
Expired
Revoked
Superseded
Show
Issued STARs (entrustment decisions) · 5 shown
| Trainee
| EPA
| Level
| Issued
| Expires
| Status
| Actions
| Anele Dlamini
| PAED-002 — Managing common paediatric presentations
| 3b
| 2026-10-13
[aria-label: Open review #2] review #2
| —
| Active
[aria-label: Download the certificate of Anele Dlamini's PAED-002 decision, issued 2026-10-13] Download
[aria-label: Revoke Anele Dlamini's PAED-002 decision, issued 2026-10-13] Revoke
| Anele Dlamini
| PAED-001 — Providing paediatric emergency care to children
| 4
| 2026-10-13
[aria-label: Open review #2] review #2
| —
| Active
[aria-label: Download the certificate of Anele Dlamini's PAED-001 decision, issued 2026-10-13] Download
[aria-label: Revoke Anele Dlamini's PAED-001 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
| 5
| 2026-10-13
[aria-label: Open review #1] review #1
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-012 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-012 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-010 — Leading and operating within a clinical team
| 4
| 2026-10-13
[aria-label: Open review #1] review #1
| 2026-11-02
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-010 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-010 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-001 — Providing paediatric emergency care to children
| 5
| 2026-10-13
[aria-label: Open review #1] review #1
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-001 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-001 decision, issued 2026-10-13] Revoke
### Revoke Anele Dlamini's PAED-002 STAR?
PAED-002 — Managing common paediatric presentations, level 3b, issued 2026-10-13. Revocation is immediate and cannot be undone.
Anele Dlamini is sent an email saying the STAR was revoked and by whom, with a link to My authorisations, unless her account is deactivated. The reason is not in the email: she reads it on My authorisations and on the certificate.
The EPA reads 'Revoked: re-decide' on Decisions due, and her review's agenda keeps its line.
Revocation reason *required
Staged on evidence about PAED-001, not PAED-002; to be decided again.
69 of 1,000 characters
Cancel
Revoke the STAR
A revoke refused, in the dialog (M4)Scratch state: revoked from a second tab first · “Only active entrustment decisions can be revoked.” under the reason, the reason kept, the dialog openFocus: the refusalR3
Wombat
Acting as Speciality admin
MenuClose
[aria-label: Main]
Acting as Speciality admin
Home
Programme trainees
Waiting for assessors
Decisions due
Committee reviews
Decision panels
Entrustment decisions
My data rights
[title: Refilwe Mokoena] Refilwe Mokoena
Sign out
## Entrustment decisions
Issued Statements of Awarded Responsibility across the programme.
[aria-label: Filter Entrustment decisions]
Trainee
[placeholder: Filter by name]
Status
All
Active
Expired
Revoked
Superseded
Show
Issued STARs (entrustment decisions) · 5 shown
| Trainee
| EPA
| Level
| Issued
| Expires
| Status
| Actions
| Anele Dlamini
| PAED-002 — Managing common paediatric presentations
| 3b
| 2026-10-13
[aria-label: Open review #2] review #2
| —
| Active
[aria-label: Download the certificate of Anele Dlamini's PAED-002 decision, issued 2026-10-13] Download
[aria-label: Revoke Anele Dlamini's PAED-002 decision, issued 2026-10-13] Revoke
| Anele Dlamini
| PAED-001 — Providing paediatric emergency care to children
| 4
| 2026-10-13
[aria-label: Open review #2] review #2
| —
| Active
[aria-label: Download the certificate of Anele Dlamini's PAED-001 decision, issued 2026-10-13] Download
[aria-label: Revoke Anele Dlamini's PAED-001 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
| 5
| 2026-10-13
[aria-label: Open review #1] review #1
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-012 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-012 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-010 — Leading and operating within a clinical team
| 4
| 2026-10-13
[aria-label: Open review #1] review #1
| 2026-11-02
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-010 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-010 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-001 — Providing paediatric emergency care to children
| 5
| 2026-10-13
[aria-label: Open review #1] review #1
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-001 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-001 decision, issued 2026-10-13] Revoke
### Revoke Anele Dlamini's PAED-002 STAR?
PAED-002 — Managing common paediatric presentations, level 3b, issued 2026-10-13. Revocation is immediate and cannot be undone.
Anele Dlamini is sent an email saying the STAR was revoked and by whom, with a link to My authorisations, unless her account is deactivated. The reason is not in the email: she reads it on My authorisations and on the certificate.
The EPA reads 'Revoked: re-decide' on Decisions due, and her review's agenda keeps its line.
Revocation reason *required
Staged on evidence about PAED-001, not PAED-002; to be decided again.
69 of 1,000 characters
Only active entrustment decisions can be revoked.
Cancel
Revoke the STAR
Revoked (4.36, C21, C22)“Anele Dlamini's PAED-002 STAR is revoked.” above the table: nothing claims the mail's delivery · the row Revoked with Download only, and its line (flow 03's note row, generalised) names when, who and whyFocus: the resultNEW
Wombat
Acting as Speciality admin
MenuClose
[aria-label: Main]
Acting as Speciality admin
Home
Programme trainees
Waiting for assessors
Decisions due
Committee reviews
Decision panels
Entrustment decisions
My data rights
[title: Refilwe Mokoena] Refilwe Mokoena
Sign out
## Entrustment decisions
Issued Statements of Awarded Responsibility across the programme.
[aria-label: Filter Entrustment decisions]
Trainee
[placeholder: Filter by name]
Status
All
Active
Expired
Revoked
Superseded
Show
Anele Dlamini's PAED-002 STAR is revoked.
Issued STARs (entrustment decisions) · 5 shown
| Trainee
| EPA
| Level
| Issued
| Expires
| Status
| Actions
| Anele Dlamini
| PAED-002 — Managing common paediatric presentations
| 3b
| 2026-10-13
[aria-label: Open review #2] review #2
| —
| Revoked
[aria-label: Download the certificate of Anele Dlamini's PAED-002 decision, issued 2026-10-13] Download
| Revoked 2026-10-13 at ‹hh:mm› SAST by Refilwe Mokoena: Staged on evidence about PAED-001, not PAED-002; to be decided again.
| Anele Dlamini
| PAED-001 — Providing paediatric emergency care to children
| 4
| 2026-10-13
[aria-label: Open review #2] review #2
| —
| Active
[aria-label: Download the certificate of Anele Dlamini's PAED-001 decision, issued 2026-10-13] Download
[aria-label: Revoke Anele Dlamini's PAED-001 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
| 5
| 2026-10-13
[aria-label: Open review #1] review #1
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-012 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-012 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-010 — Leading and operating within a clinical team
| 4
| 2026-10-13
[aria-label: Open review #1] review #1
| 2026-11-02
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-010 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-010 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-001 — Providing paediatric emergency care to children
| 5
| 2026-10-13
[aria-label: Open review #1] review #1
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-001 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-001 decision, issued 2026-10-13] Revoke
Filtered to Revoked (4.37)Kabelo Sithole · one row, its line; its certificate names Refilwe Mokoena, not a user id (T324)
Wombat
Acting as Sub-speciality admin
MenuClose
[aria-label: Main]
Acting as Sub-speciality admin
Home
Programme trainees
Waiting for assessors
Decisions due
Committee reviews
Decision panels
Entrustment decisions
My data rights
[title: Kabelo Sithole] Kabelo Sithole
Sign out
## Entrustment decisions
Issued Statements of Awarded Responsibility across the programme.
[aria-label: Filter Entrustment decisions]
Trainee
[placeholder: Filter by name]
Status
All
Active
Expired
Revoked
Superseded
Show
Clear filters
1 shown
Issued STARs (entrustment decisions) · 1 shown
| Trainee
| EPA
| Level
| Issued
| Expires
| Status
| Actions
| Anele Dlamini
| PAED-002 — Managing common paediatric presentations
| 3b
| 2026-10-13
[aria-label: Open review #2] review #2
| —
| Revoked
[aria-label: Download the certificate of Anele Dlamini's PAED-002 decision, issued 2026-10-13] Download
| Revoked 2026-10-13 at ‹hh:mm› SAST by Refilwe Mokoena: Staged on evidence about PAED-001, not PAED-002; to be decided again.

######## BOARD R3-6-register-1280-c.dc.html — Register · 1280 · c
## The STAR register · 1280px · c: heavy and states
Route /admin/entrustment-decisions · Administrator, Institutional admin, Speciality and Sub-speciality admin · lit for the two speciality admins (no trail); Home › Entrustment decisions for the others.
Components PageHeader, the FilterBar pattern (search-container; Show, Clear filters, F9), a hand-written .clinic-table with both stack classes (S8), Badge (Active green, Expired amber, Revoked red, Superseded grey; labels kept), ConfirmDialog CHANGED (a field takes the first focus; a refusal keeps it open, M5), Alert, ActionResult, StatePanel, EpaLabel, .actions-cell. NEW flow 03's note row generalised (.has-note / .history-note), .issued-review, the status count.
Rests on T367 decided, T367: the mail the dialog announces; the reviser's name in the row and on the certificate (T324).
DESIGN.md § Table system (S8, the note row); § Button system (M5); § Accessibility (S11). Round 3 F3, F9, M4, M6, M9, M10, S1, S4, S6, S7, S11, nits.
Fifteen Active (5.7, S1)Nolwazi Mbatha, Molefe, Active · “· 15 shown”: review #7's thirteen, then #1's PAED-012 and PAED-001R3
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Entrustment decisions
## Entrustment decisions
Issued Statements of Awarded Responsibility across the programme.
[aria-label: Filter Entrustment decisions]
Trainee
[placeholder: Filter by name] [value: Molefe]
Status
All
Active
Expired
Revoked
Superseded
Show
Clear filters
15 shown
Issued STARs (entrustment decisions) · 15 shown
| Trainee
| EPA
| Level
| Issued
| Expires
| Status
| Actions
| Lerato Molefe
| PAED-010 — Leading and operating within a clinical team
| 5
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-010 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-010 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
| 4
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-015 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-015 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-014 — Providing palliative and end-of-life care
| 4
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-014 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-014 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-013 — Managing child protection, legal and ethical aspects of children at risk
| 4
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-013 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-013 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-011 — Managing population health challenges
| 4
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-011 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-011 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-009 — Providing care for an adolescent patient
| 4
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-009 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-009 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
| 4
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-008 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-008 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-007 — Maintaining and promoting the health and well-being of children
| 5
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-007 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-007 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-006 — Managing long-term health conditions (LTHCs)
| 5
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-006 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-006 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-005 — Providing neonatal care in intensive and high-care settings
| 5
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-005 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-005 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-004 — Managing common neonatal conditions
| 5
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-004 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-004 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-003 — Providing intensive care to children
| 5
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-003 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-003 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-002 — Managing common paediatric presentations
| 5
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-002 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-002 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
| 5
| 2026-10-13
[aria-label: Open review #1] review #1
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-012 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-012 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-001 — Providing paediatric emergency care to children
| 5
| 2026-10-13
[aria-label: Open review #1] review #1
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-001 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-001 decision, issued 2026-10-13] Revoke
All, with the Superseded (5.7, S4)Nolwazi Mbatha, Molefe, All · newest issued first; the superseded PAED-010 reads “Superseded by STAR #18, issued 2026-10-13.” (no supersession date is stored)R3
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Entrustment decisions
## Entrustment decisions
Issued Statements of Awarded Responsibility across the programme.
[aria-label: Filter Entrustment decisions]
Trainee
[placeholder: Filter by name] [value: Molefe]
Status
All
Active
Expired
Revoked
Superseded
Show
Clear filters
16 shown
Issued STARs (entrustment decisions) · 16 shown
| Trainee
| EPA
| Level
| Issued
| Expires
| Status
| Actions
| Lerato Molefe
| PAED-010 — Leading and operating within a clinical team
| 5
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-010 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-010 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
| 4
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-015 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-015 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-014 — Providing palliative and end-of-life care
| 4
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-014 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-014 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-013 — Managing child protection, legal and ethical aspects of children at risk
| 4
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-013 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-013 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-011 — Managing population health challenges
| 4
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-011 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-011 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-009 — Providing care for an adolescent patient
| 4
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-009 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-009 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
| 4
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-008 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-008 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-007 — Maintaining and promoting the health and well-being of children
| 5
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-007 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-007 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-006 — Managing long-term health conditions (LTHCs)
| 5
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-006 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-006 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-005 — Providing neonatal care in intensive and high-care settings
| 5
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-005 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-005 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-004 — Managing common neonatal conditions
| 5
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-004 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-004 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-003 — Providing intensive care to children
| 5
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-003 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-003 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-002 — Managing common paediatric presentations
| 5
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-002 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-002 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
| 5
| 2026-10-13
[aria-label: Open review #1] review #1
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-012 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-012 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-010 — Leading and operating within a clinical team
| 4
| 2026-10-13
[aria-label: Open review #1] review #1
| 2026-11-02
| Superseded
[aria-label: Download the certificate of Lerato Molefe's PAED-010 decision, issued 2026-10-13] Download
| Superseded by STAR #18, issued 2026-10-13.
| Lerato Molefe
| PAED-001 — Providing paediatric emergency care to children
| 5
| 2026-10-13
[aria-label: Open review #1] review #1
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-001 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-001 decision, issued 2026-10-13] Revoke
A paused EPA (6.22)Nolwazi Mbatha after Act 6 · PAED-012's row keeps Active and Revoke; “(no longer in use)” muted after its title (T255)
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Entrustment decisions
## Entrustment decisions
Issued Statements of Awarded Responsibility across the programme.
[aria-label: Filter Entrustment decisions]
Trainee
[placeholder: Filter by name] [value: Molefe]
Status
All
Active
Expired
Revoked
Superseded
Show
Clear filters
Issued STARs (entrustment decisions) · 16 shown
| Trainee
| EPA
| Level
| Issued
| Expires
| Status
| Actions
| Lerato Molefe
| PAED-010 — Leading and operating within a clinical team
| 5
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-010 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-010 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
| 4
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-015 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-015 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-014 — Providing palliative and end-of-life care
| 4
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-014 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-014 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-013 — Managing child protection, legal and ethical aspects of children at risk
| 4
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-013 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-013 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-011 — Managing population health challenges
| 4
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-011 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-011 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-009 — Providing care for an adolescent patient
| 4
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-009 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-009 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
| 4
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-008 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-008 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-007 — Maintaining and promoting the health and well-being of children
| 5
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-007 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-007 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-006 — Managing long-term health conditions (LTHCs)
| 5
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-006 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-006 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-005 — Providing neonatal care in intensive and high-care settings
| 5
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-005 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-005 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-004 — Managing common neonatal conditions
| 5
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-004 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-004 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-003 — Providing intensive care to children
| 5
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-003 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-003 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-002 — Managing common paediatric presentations
| 5
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-002 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-002 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams (no longer in use)
| 5
| 2026-10-13
[aria-label: Open review #1] review #1
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-012 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-012 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-010 — Leading and operating within a clinical team
| 4
| 2026-10-13
[aria-label: Open review #1] review #1
| 2026-11-02
| Superseded
[aria-label: Download the certificate of Lerato Molefe's PAED-010 decision, issued 2026-10-13] Download
| Superseded by STAR #18, issued 2026-10-13.
| Lerato Molefe
| PAED-001 — Providing paediatric emergency care to children
| 5
| 2026-10-13
[aria-label: Open review #1] review #1
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-001 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-001 decision, issued 2026-10-13] Revoke
LoadingThe header and the filter form stand; skeletons; no Download or Revoke until the read is in
Wombat
Acting as Speciality admin
MenuClose
[aria-label: Main]
Acting as Speciality admin
Home
Programme trainees
Waiting for assessors
Decisions due
Committee reviews
Decision panels
Entrustment decisions
My data rights
[title: Refilwe Mokoena] Refilwe Mokoena
Sign out
## Entrustment decisions
Issued Statements of Awarded Responsibility across the programme.
[aria-label: Filter Entrustment decisions]
Trainee
[placeholder: Filter by name]
Status
All
Active
Expired
Revoked
Superseded
Show
Loading the STARs.
Load error (T329)Fixed words with Try againFocus: the alert
Wombat
Acting as Speciality admin
MenuClose
[aria-label: Main]
Acting as Speciality admin
Home
Programme trainees
Waiting for assessors
Decisions due
Committee reviews
Decision panels
Entrustment decisions
My data rights
[title: Refilwe Mokoena] Refilwe Mokoena
Sign out
## Entrustment decisions
Issued Statements of Awarded Responsibility across the programme.
[aria-label: Filter Entrustment decisions]
Trainee
[placeholder: Filter by name]
Status
All
Active
Expired
Revoked
Superseded
Show
Could not load the STARs. Nothing has changed. Try again, or come back in a few minutes.
Try again

######## BOARD R3-6-register-768.dc.html — Register · 768
## The STAR register · 768px
Route /admin/entrustment-decisions · Administrator, Institutional admin, Speciality and Sub-speciality admin · lit for the two speciality admins (no trail); Home › Entrustment decisions for the others.
Components PageHeader, the FilterBar pattern (search-container; Show, Clear filters, F9), a hand-written .clinic-table with both stack classes (S8), Badge (Active green, Expired amber, Revoked red, Superseded grey; labels kept), ConfirmDialog CHANGED (a field takes the first focus; a refusal keeps it open, M5), Alert, ActionResult, StatePanel, EpaLabel, .actions-cell. NEW flow 03's note row generalised (.has-note / .history-note), .issued-review, the status count.
Rests on T367 decided, T367: the mail the dialog announces; the reviser's name in the row and on the certificate (T324).
DESIGN.md § Table system (S8, the note row); § Button system (M5); § Accessibility (S11). Round 3 F3, F9, M4, M6, M9, M10, S1, S4, S6, S7, S11, nits.
Five issued (4.35)Nolwazi Mbatha from her Home's quick links: no menu item for her, so the trail Home › Entrustment decisions · newest issued first, then STAR number descending; Issued's link “review #2” named “Open review #2”, 24px tall (M9); Revoke an outline button named for its rowNEWR3
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Entrustment decisions
## Entrustment decisions
Issued Statements of Awarded Responsibility across the programme.
[aria-label: Filter Entrustment decisions]
Trainee
[placeholder: Filter by name]
Status
All
Active
Expired
Revoked
Superseded
Show
Issued STARs (entrustment decisions) · 5 shown
| Trainee
| EPA
| Level
| Issued
| Expires
| Status
| Actions
| Anele Dlamini
| PAED-002 — Managing common paediatric presentations
| 3b
| 2026-10-13
[aria-label: Open review #2] review #2
| —
| Active
[aria-label: Download the certificate of Anele Dlamini's PAED-002 decision, issued 2026-10-13] Download
[aria-label: Revoke Anele Dlamini's PAED-002 decision, issued 2026-10-13] Revoke
| Anele Dlamini
| PAED-001 — Providing paediatric emergency care to children
| 4
| 2026-10-13
[aria-label: Open review #2] review #2
| —
| Active
[aria-label: Download the certificate of Anele Dlamini's PAED-001 decision, issued 2026-10-13] Download
[aria-label: Revoke Anele Dlamini's PAED-001 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
| 5
| 2026-10-13
[aria-label: Open review #1] review #1
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-012 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-012 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-010 — Leading and operating within a clinical team
| 4
| 2026-10-13
[aria-label: Open review #1] review #1
| 2026-11-02
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-010 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-010 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-001 — Providing paediatric emergency care to children
| 5
| 2026-10-13
[aria-label: Open review #1] review #1
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-001 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-001 decision, issued 2026-10-13] Revoke
Revoked (4.36, C21, C22)“Anele Dlamini's PAED-002 STAR is revoked.” above the table: nothing claims the mail's delivery · the row Revoked with Download only, and its line (flow 03's note row, generalised) names when, who and whyFocus: the resultNEW
Wombat
Acting as Speciality admin
MenuClose
[aria-label: Main]
Acting as Speciality admin
Home
Programme trainees
Waiting for assessors
Decisions due
Committee reviews
Decision panels
Entrustment decisions
My data rights
[title: Refilwe Mokoena] Refilwe Mokoena
Sign out
## Entrustment decisions
Issued Statements of Awarded Responsibility across the programme.
[aria-label: Filter Entrustment decisions]
Trainee
[placeholder: Filter by name]
Status
All
Active
Expired
Revoked
Superseded
Show
Anele Dlamini's PAED-002 STAR is revoked.
Issued STARs (entrustment decisions) · 5 shown
| Trainee
| EPA
| Level
| Issued
| Expires
| Status
| Actions
| Anele Dlamini
| PAED-002 — Managing common paediatric presentations
| 3b
| 2026-10-13
[aria-label: Open review #2] review #2
| —
| Revoked
[aria-label: Download the certificate of Anele Dlamini's PAED-002 decision, issued 2026-10-13] Download
| Revoked 2026-10-13 at ‹hh:mm› SAST by Refilwe Mokoena: Staged on evidence about PAED-001, not PAED-002; to be decided again.
| Anele Dlamini
| PAED-001 — Providing paediatric emergency care to children
| 4
| 2026-10-13
[aria-label: Open review #2] review #2
| —
| Active
[aria-label: Download the certificate of Anele Dlamini's PAED-001 decision, issued 2026-10-13] Download
[aria-label: Revoke Anele Dlamini's PAED-001 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
| 5
| 2026-10-13
[aria-label: Open review #1] review #1
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-012 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-012 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-010 — Leading and operating within a clinical team
| 4
| 2026-10-13
[aria-label: Open review #1] review #1
| 2026-11-02
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-010 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-010 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-001 — Providing paediatric emergency care to children
| 5
| 2026-10-13
[aria-label: Open review #1] review #1
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-001 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-001 decision, issued 2026-10-13] Revoke
All, with the Superseded (5.7, S4)Nolwazi Mbatha, Molefe, All · newest issued first; the superseded PAED-010 reads “Superseded by STAR #18, issued 2026-10-13.” (no supersession date is stored)R3
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Entrustment decisions
## Entrustment decisions
Issued Statements of Awarded Responsibility across the programme.
[aria-label: Filter Entrustment decisions]
Trainee
[placeholder: Filter by name] [value: Molefe]
Status
All
Active
Expired
Revoked
Superseded
Show
Clear filters
16 shown
Issued STARs (entrustment decisions) · 16 shown
| Trainee
| EPA
| Level
| Issued
| Expires
| Status
| Actions
| Lerato Molefe
| PAED-010 — Leading and operating within a clinical team
| 5
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-010 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-010 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
| 4
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-015 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-015 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-014 — Providing palliative and end-of-life care
| 4
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-014 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-014 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-013 — Managing child protection, legal and ethical aspects of children at risk
| 4
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-013 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-013 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-011 — Managing population health challenges
| 4
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-011 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-011 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-009 — Providing care for an adolescent patient
| 4
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-009 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-009 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
| 4
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-008 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-008 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-007 — Maintaining and promoting the health and well-being of children
| 5
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-007 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-007 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-006 — Managing long-term health conditions (LTHCs)
| 5
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-006 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-006 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-005 — Providing neonatal care in intensive and high-care settings
| 5
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-005 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-005 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-004 — Managing common neonatal conditions
| 5
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-004 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-004 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-003 — Providing intensive care to children
| 5
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-003 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-003 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-002 — Managing common paediatric presentations
| 5
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-002 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-002 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
| 5
| 2026-10-13
[aria-label: Open review #1] review #1
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-012 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-012 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-010 — Leading and operating within a clinical team
| 4
| 2026-10-13
[aria-label: Open review #1] review #1
| 2026-11-02
| Superseded
[aria-label: Download the certificate of Lerato Molefe's PAED-010 decision, issued 2026-10-13] Download
| Superseded by STAR #18, issued 2026-10-13.
| Lerato Molefe
| PAED-001 — Providing paediatric emergency care to children
| 5
| 2026-10-13
[aria-label: Open review #1] review #1
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-001 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-001 decision, issued 2026-10-13] Revoke

######## BOARD R3-6-register-390-a.dc.html — Register · 390 · a
## The STAR register · 390px · a
Route /admin/entrustment-decisions · Administrator, Institutional admin, Speciality and Sub-speciality admin · lit for the two speciality admins (no trail); Home › Entrustment decisions for the others.
Components PageHeader, the FilterBar pattern (search-container; Show, Clear filters, F9), a hand-written .clinic-table with both stack classes (S8), Badge (Active green, Expired amber, Revoked red, Superseded grey; labels kept), ConfirmDialog CHANGED (a field takes the first focus; a refusal keeps it open, M5), Alert, ActionResult, StatePanel, EpaLabel, .actions-cell. NEW flow 03's note row generalised (.has-note / .history-note), .issued-review, the status count.
Rests on T367 decided, T367: the mail the dialog announces; the reviser's name in the row and on the certificate (T324).
DESIGN.md § Table system (S8, the note row); § Button system (M5); § Accessibility (S11). Round 3 F3, F9, M4, M6, M9, M10, S1, S4, S6, S7, S11, nits.
On her phone (A.7.7, M10)Refilwe Mokoena after Acts 5 and 6 · “· 18 shown”: Lerato Molefe's 16 and Anele Dlamini's 2; every stacked row shows all its fields, Issued and Expires included; Issued's link 44px; the revoked line wraps, never cut (C22)R3
Wombat
Acting as Speciality admin
MenuClose
[aria-label: Main]
Acting as Speciality admin
Home
Programme trainees
Waiting for assessors
Decisions due
Committee reviews
Decision panels
Entrustment decisions
My data rights
[title: Refilwe Mokoena] Refilwe Mokoena
Sign out
## Entrustment decisions
Issued Statements of Awarded Responsibility across the programme.
[aria-label: Filter Entrustment decisions]
Trainee
[placeholder: Filter by name]
Status
All
Active
Expired
Revoked
Superseded
Show
Issued STARs (entrustment decisions) · 18 shown
| Trainee
| EPA
| Level
| Issued
| Expires
| Status
| Actions
| Lerato Molefe
| PAED-010 — Leading and operating within a clinical team
| 5
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-010 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-010 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
| 4
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-015 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-015 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-014 — Providing palliative and end-of-life care
| 4
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-014 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-014 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-013 — Managing child protection, legal and ethical aspects of children at risk
| 4
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-013 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-013 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-011 — Managing population health challenges
| 4
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-011 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-011 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-009 — Providing care for an adolescent patient
| 4
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-009 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-009 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
| 4
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-008 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-008 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-007 — Maintaining and promoting the health and well-being of children
| 5
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-007 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-007 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-006 — Managing long-term health conditions (LTHCs)
| 5
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-006 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-006 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-005 — Providing neonatal care in intensive and high-care settings
| 5
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-005 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-005 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-004 — Managing common neonatal conditions
| 5
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-004 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-004 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-003 — Providing intensive care to children
| 5
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-003 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-003 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-002 — Managing common paediatric presentations
| 5
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-002 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-002 decision, issued 2026-10-13] Revoke
| Anele Dlamini
| PAED-002 — Managing common paediatric presentations
| 3b
| 2026-10-13
[aria-label: Open review #2] review #2
| —
| Revoked
[aria-label: Download the certificate of Anele Dlamini's PAED-002 decision, issued 2026-10-13] Download
| Revoked 2026-10-13 at ‹hh:mm› SAST by Refilwe Mokoena: Staged on evidence about PAED-001, not PAED-002; to be decided again.
| Anele Dlamini
| PAED-001 — Providing paediatric emergency care to children
| 4
| 2026-10-13
[aria-label: Open review #2] review #2
| —
| Active
[aria-label: Download the certificate of Anele Dlamini's PAED-001 decision, issued 2026-10-13] Download
[aria-label: Revoke Anele Dlamini's PAED-001 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams (no longer in use)
| 5
| 2026-10-13
[aria-label: Open review #1] review #1
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-012 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-012 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-010 — Leading and operating within a clinical team
| 4
| 2026-10-13
[aria-label: Open review #1] review #1
| 2026-11-02
| Superseded
[aria-label: Download the certificate of Lerato Molefe's PAED-010 decision, issued 2026-10-13] Download
| Superseded by STAR #18, issued 2026-10-13.
| Lerato Molefe
| PAED-001 — Providing paediatric emergency care to children
| 5
| 2026-10-13
[aria-label: Open review #1] review #1
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-001 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-001 decision, issued 2026-10-13] Revoke
None issuedRefilwe Mokoena before any review is ratified · no filter form when there is nothing to filter (FilterBar's rule)Focus: the h1
Wombat
Acting as Speciality admin
MenuClose
[aria-label: Main]
Acting as Speciality admin
Home
Programme trainees
Waiting for assessors
Decisions due
Committee reviews
Decision panels
Entrustment decisions
My data rights
[title: Refilwe Mokoena] Refilwe Mokoena
Sign out
## Entrustment decisions
Issued Statements of Awarded Responsibility across the programme.
No STARs issued yet
When a committee ratifies an entrustment decision, its STAR is listed here.
Five issued (4.35)Nolwazi Mbatha from her Home's quick links: no menu item for her, so the trail Home › Entrustment decisions · newest issued first, then STAR number descending; Issued's link “review #2” named “Open review #2”, 24px tall (M9); Revoke an outline button named for its rowNEWR3
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Entrustment decisions
## Entrustment decisions
Issued Statements of Awarded Responsibility across the programme.
[aria-label: Filter Entrustment decisions]
Trainee
[placeholder: Filter by name]
Status
All
Active
Expired
Revoked
Superseded
Show
Issued STARs (entrustment decisions) · 5 shown
| Trainee
| EPA
| Level
| Issued
| Expires
| Status
| Actions
| Anele Dlamini
| PAED-002 — Managing common paediatric presentations
| 3b
| 2026-10-13
[aria-label: Open review #2] review #2
| —
| Active
[aria-label: Download the certificate of Anele Dlamini's PAED-002 decision, issued 2026-10-13] Download
[aria-label: Revoke Anele Dlamini's PAED-002 decision, issued 2026-10-13] Revoke
| Anele Dlamini
| PAED-001 — Providing paediatric emergency care to children
| 4
| 2026-10-13
[aria-label: Open review #2] review #2
| —
| Active
[aria-label: Download the certificate of Anele Dlamini's PAED-001 decision, issued 2026-10-13] Download
[aria-label: Revoke Anele Dlamini's PAED-001 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
| 5
| 2026-10-13
[aria-label: Open review #1] review #1
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-012 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-012 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-010 — Leading and operating within a clinical team
| 4
| 2026-10-13
[aria-label: Open review #1] review #1
| 2026-11-02
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-010 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-010 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-001 — Providing paediatric emergency care to children
| 5
| 2026-10-13
[aria-label: Open review #1] review #1
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-001 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-001 decision, issued 2026-10-13] Revoke
Filtered to Molefe, after Show (4.35, F9, S11)The Trainee field binds on change; Show applies it; Clear filters once a filter is set; a visually hidden status reads “3 shown”Focus: stays on ShowR3
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Entrustment decisions
## Entrustment decisions
Issued Statements of Awarded Responsibility across the programme.
[aria-label: Filter Entrustment decisions]
Trainee
[placeholder: Filter by name] [value: Molefe]
Status
All
Active
Expired
Revoked
Superseded
Show
Clear filters
3 shown
Issued STARs (entrustment decisions) · 3 shown
| Trainee
| EPA
| Level
| Issued
| Expires
| Status
| Actions
| Lerato Molefe
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
| 5
| 2026-10-13
[aria-label: Open review #1] review #1
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-012 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-012 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-010 — Leading and operating within a clinical team
| 4
| 2026-10-13
[aria-label: Open review #1] review #1
| 2026-11-02
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-010 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-010 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-001 — Providing paediatric emergency care to children
| 5
| 2026-10-13
[aria-label: Open review #1] review #1
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-001 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-001 decision, issued 2026-10-13] Revoke
Filtered to Revoked (4.37)Kabelo Sithole · one row, its line; its certificate names Refilwe Mokoena, not a user id (T324)
Wombat
Acting as Sub-speciality admin
MenuClose
[aria-label: Main]
Acting as Sub-speciality admin
Home
Programme trainees
Waiting for assessors
Decisions due
Committee reviews
Decision panels
Entrustment decisions
My data rights
[title: Kabelo Sithole] Kabelo Sithole
Sign out
## Entrustment decisions
Issued Statements of Awarded Responsibility across the programme.
[aria-label: Filter Entrustment decisions]
Trainee
[placeholder: Filter by name]
Status
All
Active
Expired
Revoked
Superseded
Show
Clear filters
1 shown
Issued STARs (entrustment decisions) · 1 shown
| Trainee
| EPA
| Level
| Issued
| Expires
| Status
| Actions
| Anele Dlamini
| PAED-002 — Managing common paediatric presentations
| 3b
| 2026-10-13
[aria-label: Open review #2] review #2
| —
| Revoked
[aria-label: Download the certificate of Anele Dlamini's PAED-002 decision, issued 2026-10-13] Download
| Revoked 2026-10-13 at ‹hh:mm› SAST by Refilwe Mokoena: Staged on evidence about PAED-001, not PAED-002; to be decided again.
Filtered to nothingNolwazi Mbatha · “Mahlangu”, Revoked: the count stays, then the card with Clear filters
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Entrustment decisions
## Entrustment decisions
Issued Statements of Awarded Responsibility across the programme.
[aria-label: Filter Entrustment decisions]
Trainee
[placeholder: Filter by name] [value: Mahlangu]
Status
All
Active
Expired
Revoked
Superseded
Show
Clear filters
Issued STARs (entrustment decisions) · 0 shown
No STAR matches these filters.
Trainee “Mahlangu”, status Revoked.
Clear filters

######## BOARD R3-6-register-390-b.dc.html — Register · 390 · b
## The STAR register · 390px · b
Route /admin/entrustment-decisions · Administrator, Institutional admin, Speciality and Sub-speciality admin · lit for the two speciality admins (no trail); Home › Entrustment decisions for the others.
Components PageHeader, the FilterBar pattern (search-container; Show, Clear filters, F9), a hand-written .clinic-table with both stack classes (S8), Badge (Active green, Expired amber, Revoked red, Superseded grey; labels kept), ConfirmDialog CHANGED (a field takes the first focus; a refusal keeps it open, M5), Alert, ActionResult, StatePanel, EpaLabel, .actions-cell. NEW flow 03's note row generalised (.has-note / .history-note), .issued-review, the status count.
Rests on T367 decided, T367: the mail the dialog announces; the reviser's name in the row and on the certificate (T324).
DESIGN.md § Table system (S8, the note row); § Button system (M5); § Accessibility (S11). Round 3 F3, F9, M4, M6, M9, M10, S1, S4, S6, S7, S11, nits.
Revoke pressed (4.36, F3, M6)Refilwe Mokoena · ConfirmDialog with a field: the focus in the reason box; Revoke the STAR enabled with the box empty; the mail sentence true when T367 skips a deactivated account (S6)Focus: the reason boxPROPOSEDdecided, T367R3
Wombat
Acting as Speciality admin
MenuClose
[aria-label: Main]
Acting as Speciality admin
Home
Programme trainees
Waiting for assessors
Decisions due
Committee reviews
Decision panels
Entrustment decisions
My data rights
[title: Refilwe Mokoena] Refilwe Mokoena
Sign out
## Entrustment decisions
Issued Statements of Awarded Responsibility across the programme.
[aria-label: Filter Entrustment decisions]
Trainee
[placeholder: Filter by name]
Status
All
Active
Expired
Revoked
Superseded
Show
Issued STARs (entrustment decisions) · 5 shown
| Trainee
| EPA
| Level
| Issued
| Expires
| Status
| Actions
| Anele Dlamini
| PAED-002 — Managing common paediatric presentations
| 3b
| 2026-10-13
[aria-label: Open review #2] review #2
| —
| Active
[aria-label: Download the certificate of Anele Dlamini's PAED-002 decision, issued 2026-10-13] Download
[aria-label: Revoke Anele Dlamini's PAED-002 decision, issued 2026-10-13] Revoke
| Anele Dlamini
| PAED-001 — Providing paediatric emergency care to children
| 4
| 2026-10-13
[aria-label: Open review #2] review #2
| —
| Active
[aria-label: Download the certificate of Anele Dlamini's PAED-001 decision, issued 2026-10-13] Download
[aria-label: Revoke Anele Dlamini's PAED-001 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
| 5
| 2026-10-13
[aria-label: Open review #1] review #1
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-012 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-012 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-010 — Leading and operating within a clinical team
| 4
| 2026-10-13
[aria-label: Open review #1] review #1
| 2026-11-02
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-010 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-010 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-001 — Providing paediatric emergency care to children
| 5
| 2026-10-13
[aria-label: Open review #1] review #1
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-001 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-001 decision, issued 2026-10-13] Revoke
### Revoke Anele Dlamini's PAED-002 STAR?
PAED-002 — Managing common paediatric presentations, level 3b, issued 2026-10-13. Revocation is immediate and cannot be undone.
Anele Dlamini is sent an email saying the STAR was revoked and by whom, with a link to My authorisations, unless her account is deactivated. The reason is not in the email: she reads it on My authorisations and on the certificate.
The EPA reads 'Revoked: re-decide' on Decisions due, and her review's agenda keeps its line.
Revocation reason *required
0 of 1,000 characters
Cancel
Revoke the STAR
Revoke the STAR pressed blank (F3, M6)Nothing is sent; “Give a reason for the revocation.” under the box, tied to it by aria-describedby; the dialog stays openFocus: the reason boxR3
Wombat
Acting as Speciality admin
MenuClose
[aria-label: Main]
Acting as Speciality admin
Home
Programme trainees
Waiting for assessors
Decisions due
Committee reviews
Decision panels
Entrustment decisions
My data rights
[title: Refilwe Mokoena] Refilwe Mokoena
Sign out
## Entrustment decisions
Issued Statements of Awarded Responsibility across the programme.
[aria-label: Filter Entrustment decisions]
Trainee
[placeholder: Filter by name]
Status
All
Active
Expired
Revoked
Superseded
Show
Issued STARs (entrustment decisions) · 5 shown
| Trainee
| EPA
| Level
| Issued
| Expires
| Status
| Actions
| Anele Dlamini
| PAED-002 — Managing common paediatric presentations
| 3b
| 2026-10-13
[aria-label: Open review #2] review #2
| —
| Active
[aria-label: Download the certificate of Anele Dlamini's PAED-002 decision, issued 2026-10-13] Download
[aria-label: Revoke Anele Dlamini's PAED-002 decision, issued 2026-10-13] Revoke
| Anele Dlamini
| PAED-001 — Providing paediatric emergency care to children
| 4
| 2026-10-13
[aria-label: Open review #2] review #2
| —
| Active
[aria-label: Download the certificate of Anele Dlamini's PAED-001 decision, issued 2026-10-13] Download
[aria-label: Revoke Anele Dlamini's PAED-001 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
| 5
| 2026-10-13
[aria-label: Open review #1] review #1
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-012 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-012 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-010 — Leading and operating within a clinical team
| 4
| 2026-10-13
[aria-label: Open review #1] review #1
| 2026-11-02
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-010 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-010 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-001 — Providing paediatric emergency care to children
| 5
| 2026-10-13
[aria-label: Open review #1] review #1
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-001 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-001 decision, issued 2026-10-13] Revoke
### Revoke Anele Dlamini's PAED-002 STAR?
PAED-002 — Managing common paediatric presentations, level 3b, issued 2026-10-13. Revocation is immediate and cannot be undone.
Anele Dlamini is sent an email saying the STAR was revoked and by whom, with a link to My authorisations, unless her account is deactivated. The reason is not in the email: she reads it on My authorisations and on the certificate.
The EPA reads 'Revoked: re-decide' on Decisions due, and her review's agenda keeps its line.
Revocation reason *required
Give a reason for the revocation.
0 of 1,000 characters
Cancel
Revoke the STAR
A revoke refused, in the dialog (M4)Scratch state: revoked from a second tab first · “Only active entrustment decisions can be revoked.” under the reason, the reason kept, the dialog openFocus: the refusalR3
Wombat
Acting as Speciality admin
MenuClose
[aria-label: Main]
Acting as Speciality admin
Home
Programme trainees
Waiting for assessors
Decisions due
Committee reviews
Decision panels
Entrustment decisions
My data rights
[title: Refilwe Mokoena] Refilwe Mokoena
Sign out
## Entrustment decisions
Issued Statements of Awarded Responsibility across the programme.
[aria-label: Filter Entrustment decisions]
Trainee
[placeholder: Filter by name]
Status
All
Active
Expired
Revoked
Superseded
Show
Issued STARs (entrustment decisions) · 5 shown
| Trainee
| EPA
| Level
| Issued
| Expires
| Status
| Actions
| Anele Dlamini
| PAED-002 — Managing common paediatric presentations
| 3b
| 2026-10-13
[aria-label: Open review #2] review #2
| —
| Active
[aria-label: Download the certificate of Anele Dlamini's PAED-002 decision, issued 2026-10-13] Download
[aria-label: Revoke Anele Dlamini's PAED-002 decision, issued 2026-10-13] Revoke
| Anele Dlamini
| PAED-001 — Providing paediatric emergency care to children
| 4
| 2026-10-13
[aria-label: Open review #2] review #2
| —
| Active
[aria-label: Download the certificate of Anele Dlamini's PAED-001 decision, issued 2026-10-13] Download
[aria-label: Revoke Anele Dlamini's PAED-001 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
| 5
| 2026-10-13
[aria-label: Open review #1] review #1
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-012 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-012 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-010 — Leading and operating within a clinical team
| 4
| 2026-10-13
[aria-label: Open review #1] review #1
| 2026-11-02
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-010 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-010 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-001 — Providing paediatric emergency care to children
| 5
| 2026-10-13
[aria-label: Open review #1] review #1
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-001 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-001 decision, issued 2026-10-13] Revoke
### Revoke Anele Dlamini's PAED-002 STAR?
PAED-002 — Managing common paediatric presentations, level 3b, issued 2026-10-13. Revocation is immediate and cannot be undone.
Anele Dlamini is sent an email saying the STAR was revoked and by whom, with a link to My authorisations, unless her account is deactivated. The reason is not in the email: she reads it on My authorisations and on the certificate.
The EPA reads 'Revoked: re-decide' on Decisions due, and her review's agenda keeps its line.
Revocation reason *required
Staged on evidence about PAED-001, not PAED-002; to be decided again.
69 of 1,000 characters
Only active entrustment decisions can be revoked.
Cancel
Revoke the STAR
Revoked (4.36, C21, C22)“Anele Dlamini's PAED-002 STAR is revoked.” above the table: nothing claims the mail's delivery · the row Revoked with Download only, and its line (flow 03's note row, generalised) names when, who and whyFocus: the resultNEW
Wombat
Acting as Speciality admin
MenuClose
[aria-label: Main]
Acting as Speciality admin
Home
Programme trainees
Waiting for assessors
Decisions due
Committee reviews
Decision panels
Entrustment decisions
My data rights
[title: Refilwe Mokoena] Refilwe Mokoena
Sign out
## Entrustment decisions
Issued Statements of Awarded Responsibility across the programme.
[aria-label: Filter Entrustment decisions]
Trainee
[placeholder: Filter by name]
Status
All
Active
Expired
Revoked
Superseded
Show
Anele Dlamini's PAED-002 STAR is revoked.
Issued STARs (entrustment decisions) · 5 shown
| Trainee
| EPA
| Level
| Issued
| Expires
| Status
| Actions
| Anele Dlamini
| PAED-002 — Managing common paediatric presentations
| 3b
| 2026-10-13
[aria-label: Open review #2] review #2
| —
| Revoked
[aria-label: Download the certificate of Anele Dlamini's PAED-002 decision, issued 2026-10-13] Download
| Revoked 2026-10-13 at ‹hh:mm› SAST by Refilwe Mokoena: Staged on evidence about PAED-001, not PAED-002; to be decided again.
| Anele Dlamini
| PAED-001 — Providing paediatric emergency care to children
| 4
| 2026-10-13
[aria-label: Open review #2] review #2
| —
| Active
[aria-label: Download the certificate of Anele Dlamini's PAED-001 decision, issued 2026-10-13] Download
[aria-label: Revoke Anele Dlamini's PAED-001 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
| 5
| 2026-10-13
[aria-label: Open review #1] review #1
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-012 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-012 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-010 — Leading and operating within a clinical team
| 4
| 2026-10-13
[aria-label: Open review #1] review #1
| 2026-11-02
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-010 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-010 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-001 — Providing paediatric emergency care to children
| 5
| 2026-10-13
[aria-label: Open review #1] review #1
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-001 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-001 decision, issued 2026-10-13] Revoke
Fifteen Active (5.7, S1)Nolwazi Mbatha, Molefe, Active · “· 15 shown”: review #7's thirteen, then #1's PAED-012 and PAED-001R3
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Entrustment decisions
## Entrustment decisions
Issued Statements of Awarded Responsibility across the programme.
[aria-label: Filter Entrustment decisions]
Trainee
[placeholder: Filter by name] [value: Molefe]
Status
All
Active
Expired
Revoked
Superseded
Show
Clear filters
15 shown
Issued STARs (entrustment decisions) · 15 shown
| Trainee
| EPA
| Level
| Issued
| Expires
| Status
| Actions
| Lerato Molefe
| PAED-010 — Leading and operating within a clinical team
| 5
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-010 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-010 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
| 4
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-015 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-015 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-014 — Providing palliative and end-of-life care
| 4
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-014 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-014 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-013 — Managing child protection, legal and ethical aspects of children at risk
| 4
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-013 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-013 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-011 — Managing population health challenges
| 4
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-011 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-011 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-009 — Providing care for an adolescent patient
| 4
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-009 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-009 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
| 4
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-008 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-008 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-007 — Maintaining and promoting the health and well-being of children
| 5
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-007 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-007 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-006 — Managing long-term health conditions (LTHCs)
| 5
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-006 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-006 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-005 — Providing neonatal care in intensive and high-care settings
| 5
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-005 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-005 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-004 — Managing common neonatal conditions
| 5
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-004 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-004 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-003 — Providing intensive care to children
| 5
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-003 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-003 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-002 — Managing common paediatric presentations
| 5
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-002 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-002 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
| 5
| 2026-10-13
[aria-label: Open review #1] review #1
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-012 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-012 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-001 — Providing paediatric emergency care to children
| 5
| 2026-10-13
[aria-label: Open review #1] review #1
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-001 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-001 decision, issued 2026-10-13] Revoke
All, with the Superseded (5.7, S4)Nolwazi Mbatha, Molefe, All · newest issued first; the superseded PAED-010 reads “Superseded by STAR #18, issued 2026-10-13.” (no supersession date is stored)R3
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Entrustment decisions
## Entrustment decisions
Issued Statements of Awarded Responsibility across the programme.
[aria-label: Filter Entrustment decisions]
Trainee
[placeholder: Filter by name] [value: Molefe]
Status
All
Active
Expired
Revoked
Superseded
Show
Clear filters
16 shown
Issued STARs (entrustment decisions) · 16 shown
| Trainee
| EPA
| Level
| Issued
| Expires
| Status
| Actions
| Lerato Molefe
| PAED-010 — Leading and operating within a clinical team
| 5
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-010 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-010 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
| 4
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-015 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-015 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-014 — Providing palliative and end-of-life care
| 4
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-014 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-014 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-013 — Managing child protection, legal and ethical aspects of children at risk
| 4
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-013 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-013 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-011 — Managing population health challenges
| 4
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-011 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-011 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-009 — Providing care for an adolescent patient
| 4
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-009 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-009 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
| 4
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-008 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-008 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-007 — Maintaining and promoting the health and well-being of children
| 5
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-007 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-007 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-006 — Managing long-term health conditions (LTHCs)
| 5
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-006 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-006 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-005 — Providing neonatal care in intensive and high-care settings
| 5
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-005 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-005 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-004 — Managing common neonatal conditions
| 5
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-004 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-004 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-003 — Providing intensive care to children
| 5
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-003 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-003 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-002 — Managing common paediatric presentations
| 5
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-002 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-002 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
| 5
| 2026-10-13
[aria-label: Open review #1] review #1
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-012 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-012 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-010 — Leading and operating within a clinical team
| 4
| 2026-10-13
[aria-label: Open review #1] review #1
| 2026-11-02
| Superseded
[aria-label: Download the certificate of Lerato Molefe's PAED-010 decision, issued 2026-10-13] Download
| Superseded by STAR #18, issued 2026-10-13.
| Lerato Molefe
| PAED-001 — Providing paediatric emergency care to children
| 5
| 2026-10-13
[aria-label: Open review #1] review #1
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-001 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-001 decision, issued 2026-10-13] Revoke
A paused EPA (6.22)Nolwazi Mbatha after Act 6 · PAED-012's row keeps Active and Revoke; “(no longer in use)” muted after its title (T255)
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Entrustment decisions
## Entrustment decisions
Issued Statements of Awarded Responsibility across the programme.
[aria-label: Filter Entrustment decisions]
Trainee
[placeholder: Filter by name] [value: Molefe]
Status
All
Active
Expired
Revoked
Superseded
Show
Clear filters
Issued STARs (entrustment decisions) · 16 shown
| Trainee
| EPA
| Level
| Issued
| Expires
| Status
| Actions
| Lerato Molefe
| PAED-010 — Leading and operating within a clinical team
| 5
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-010 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-010 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
| 4
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-015 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-015 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-014 — Providing palliative and end-of-life care
| 4
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-014 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-014 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-013 — Managing child protection, legal and ethical aspects of children at risk
| 4
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-013 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-013 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-011 — Managing population health challenges
| 4
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-011 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-011 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-009 — Providing care for an adolescent patient
| 4
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-009 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-009 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
| 4
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-008 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-008 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-007 — Maintaining and promoting the health and well-being of children
| 5
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-007 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-007 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-006 — Managing long-term health conditions (LTHCs)
| 5
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-006 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-006 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-005 — Providing neonatal care in intensive and high-care settings
| 5
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-005 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-005 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-004 — Managing common neonatal conditions
| 5
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-004 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-004 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-003 — Providing intensive care to children
| 5
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-003 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-003 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-002 — Managing common paediatric presentations
| 5
| 2026-10-13
[aria-label: Open review #7] review #7
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-002 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-002 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams (no longer in use)
| 5
| 2026-10-13
[aria-label: Open review #1] review #1
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-012 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-012 decision, issued 2026-10-13] Revoke
| Lerato Molefe
| PAED-010 — Leading and operating within a clinical team
| 4
| 2026-10-13
[aria-label: Open review #1] review #1
| 2026-11-02
| Superseded
[aria-label: Download the certificate of Lerato Molefe's PAED-010 decision, issued 2026-10-13] Download
| Superseded by STAR #18, issued 2026-10-13.
| Lerato Molefe
| PAED-001 — Providing paediatric emergency care to children
| 5
| 2026-10-13
[aria-label: Open review #1] review #1
| —
| Active
[aria-label: Download the certificate of Lerato Molefe's PAED-001 decision, issued 2026-10-13] Download
[aria-label: Revoke Lerato Molefe's PAED-001 decision, issued 2026-10-13] Revoke
LoadingThe header and the filter form stand; skeletons; no Download or Revoke until the read is in
Wombat
Acting as Speciality admin
MenuClose
[aria-label: Main]
Acting as Speciality admin
Home
Programme trainees
Waiting for assessors
Decisions due
Committee reviews
Decision panels
Entrustment decisions
My data rights
[title: Refilwe Mokoena] Refilwe Mokoena
Sign out
## Entrustment decisions
Issued Statements of Awarded Responsibility across the programme.
[aria-label: Filter Entrustment decisions]
Trainee
[placeholder: Filter by name]
Status
All
Active
Expired
Revoked
Superseded
Show
Loading the STARs.
Load error (T329)Fixed words with Try againFocus: the alert
Wombat
Acting as Speciality admin
MenuClose
[aria-label: Main]
Acting as Speciality admin
Home
Programme trainees
Waiting for assessors
Decisions due
Committee reviews
Decision panels
Entrustment decisions
My data rights
[title: Refilwe Mokoena] Refilwe Mokoena
Sign out
## Entrustment decisions
Issued Statements of Awarded Responsibility across the programme.
[aria-label: Filter Entrustment decisions]
Trainee
[placeholder: Filter by name]
Status
All
Active
Expired
Revoked
Superseded
Show
Could not load the STARs. Nothing has changed. Try again, or come back in a few minutes.
Try again
