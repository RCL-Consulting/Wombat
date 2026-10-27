# Flow 13 round 3: the text of every board

Generated from `round-3/project/R3-*.dc.html` (T379, build-contracts § 0) by a script: style, script, svg and helmet blocks stripped; headings, labels, aria-labels, placeholders, input values, captions and annotations kept. Board order is the canvas's (top to bottom, then left to right). Each board's head (its title, the caption under it, its chips) is the designer's annotation, quoted after `>`. Design data: a caption, a chip or a `DATA:` note is never a shipped word.

######## BOARD R3-0-spec.dc.html — R3-0-spec · Spec (1 of 3)
> First the screenshots; then the prerequisites, and F1–F11 and M1–M5, each with what changed and where.
### First: the screenshots
The captures did not arrive this round either. None was opened.
Nothing was attached in the session (its uploads folder is empty). I asked again to connect C:\dev\Wombat\design\upload on the linked computer, and the request could not be shown: the Claude desktop window was not available there. So neither the twelve attachments nor the seven further captures this round names were read:
- act-5 › 5.21-1-molefe-home-graduate.png
- act-5 › 5.22-2-molefe-authorisations-denied.png
- act-5 › 5.10-1-mbatha-staff-export.png
- act-5 › 5.13-1-verifier-genuine-verified.png
- act-5 › 5.13-2-verifier-uppercase-hash.png
- act-5 › 5.16-1-mbatha-complete-dialog-future-day.png
- act-5 › 5.16-2-mbatha-future-day-refused.png
- act-5 › 5.17-1-mbatha-marked-complete.png
- act-5 › 5.27-1-mbatha-deactivate-dialog.png
- act-5 › 5.27-3-mbatha-trainees-closed.png
- act-5 › 5.20-1-molefe-progress-completed.png
- act-A › A.7.4-1-home-390.png
- 5.20-2-molefe-epa-page-completed
- 5.23-2-graduate-file-verified
- 5.28-2-duplessis-progress-withdrawn
- 5.28-3-duplessis-my-activities-none
- 6.21-1-molefe-progress
- 6.21-2-molefe-paed012-page-paused
- A.7.12-3-verify-390
Brackets filled this round: none from a capture. Every FIXTURES value under “The record” is drawn (R01–R08). The brackets that only a capture holds stay, and nothing stands in their place from memory: R02's and R04's 2025 semesters (5.28-2), R07's six other rows' EPA, date, holder and credit (5.28-3), R03's and R06's trajectory charts and activity tables (5.20-2, 6.21-2), R03's two MSF lines (5.20-2), R06's trajectory line, R02's standing, PAED-006's periods on R05 (6.21-1), and R08's titles other than PAED-001, 002, 010 and 012 (the seed is not in hand).
### Prerequisites
- T312 lands first, as its own task, grown by F3 (recorded in T312 on 2026-10-27): an ended record keeps the EPA list and targets it ended with; a paused EPA stays, headed “(no longer in use)”, on the card (EpaLabel), on its page and in the standing's count; an item added later does not appear. Boards: R3-R05, R3-R06, and R3-P18's annotation. Steps 6.21, 6.40, 6.41.
- Everything else is this build's (Q16): T311, T314, T313 with T287's verify item (PortfolioExportCleanupJob, its registration in DependencyInjection.cs and its ScheduledJobDefinition row removed), the T325, T329, T280 and T323 halves of these pages, the closed list's View and the end section. T310, T287's sign-in items, T166 and T374 stay their own.
### The operator's decisions, F1–F11
| Item | What changed | Where |
| F1 the export's gate | MayExport (MayReadAsync on the trainee's user id) on CommitteeReviewDetailDto, ProgrammeTraineeDto and TraineeProfileDto; the action in the page header's actions only where it is true. The Administrator is not refused; that case is gone. | R3-X01–X10, R3-P01, P05, P11; Spec 2 |
| F2 the take-back | Both end commands return the count ProgrammeEndCredit.SaveAsync took back; the result is built from it; the dialog's count is a forecast read; Mark complete carries it as Deactivate does. | R3-P03, P05, P09, P11, P20, P21 |
| F3 T312 grown | The ended EPA page of a paused EPA, the ended card's EpaLabel, the standing's count. | R3-R05, R3-R06, R3-P18; Prerequisites |
| F4 no trail on the personal export | /portfolio/export draws no breadcrumb for a Trainee or a record holder; Export portfolio lights itself. | R3-E01–E03, E07, E08, E11, E12, E16 |
| F5 the account sentence | Shown only when the account holds no role and is neither locked nor erased; its STAR clause only when a STAR exists; a variant for a person who holds Assessor now. | R3-P05, P11, P24, P25; Spec 2 (erased) |
| F6 holds a STAR | Any ratified STAR decision, in force or not; Home's count says “in force”. No profile at /portfolio/export: the Unavailable page. | R3-H01, H02, R01, R02, E19 |
| F7 the closed list | The Administrator's list puts the institution under each name; the count word in AdmissionText; an erased person listed as Withdrawn under the pseudonym. | R3-C01–C07 |
| F8 Mark complete's address | “sent to molefe@kgk.wombat.local” kept, NEW, from TraineeProfileDto.Email. | R3-P03, P17, P19, P20 |
| F9 Verify's answer | ActionResult (FocusOnLoad) holding one success Alert, “Export verified.” bold, then the details list; replaces round 2's h2 section. | R3-V02, V03, V07, V09, V10 |
| F10 § Badges | Completed .badge-completed (shipped); Withdrawn and Inactive .badge-withdrawn (NEW, grey); Active a plain word; badge-draft on no R3 board. | R3-C*, R3-P05, P11, P21; Spec 3 |
| F11 the Domain nouns | EnsureCanEnd and its tests say “last day”; the before-start refusal a NEW row with its own board. | R3-P02, P07, P22 |
### Must fix, M1–M5
| Item | What changed | Where |
| M1 the gate drawn | X08 redrawn: Pieter Smit on ‹registrar›'s review with TraineeElsewhere set, no progress link, no action, “MayExport is false”. X09 (Demo Administrator) and X10 (Nolwazi Mbatha) new: no progress link (RosterRoles), the action in the header. X01, X03, X05: the action moved to the header's actions. No board says an Administrator is refused. | R3-X01, X03, X05, X08, X09, X10 |
| M2 the take-back | P09 “…: 1 activity, observed 2026-10-25, will stop counting.” (NEW, a forecast); P03 and P05 count 0, so no clause and no sentence (their captions say so); P20 and P21 new. | R3-P03, P05, P09, P11, P20, P21 |
| M3 T312's ended EPA page | R06 with no PausedRow alert, flow 05's ended lines, Entrustment 5 · At or above · Issued 2026-10-26 · Exit level 5 · reached, Open My authorisations, headed “(no longer in use)”; R05's PAED-012 card by EpaLabel and the standing 15 of 15; both pilled “after T312”. | R3-R05, R3-R06; Steps 6.21 |
| M4 the staff trail | GetExportSubjectQuery carries ProfileId (from PreferredProfiles, null when none); the middle crumb through PageHeader.Trail: “Home › Trainees › Lerato Molefe's programme › Export portfolio”, href /admin/trainees/edit?id=2; “Home › Programme trainees › Lerato Molefe › Export portfolio”, href /programme/trainees/2. | R3-E04, E05, E13, E17, E18 (Trainees); R3-E06, E20 (Programme trainees) |
| M5 A.7.12's board | V02 titled “5.13 verified, lower case, three exports” at every width, no twin claim; V07 at 390 titled “5.23 the second file (at 390, A.7.12)”; Steps A.7.12 names R3-V07 at 390. | R3-V02, R3-V07-390; R3-0-steps |

######## BOARD R3-0-spec-2.dc.html — R3-0-spec-2 · Spec (2 of 3)
> S1–S24, N1–N16, T1–T12 and A1–A5, each with what changed and where; the commands and queries as written; the build rules; the Where column.
### Should fix, S1–S24
| Item | What changed | Where |
| S1 one trail rule | No breadcrumb nav on the personal export; one aria-current="page", the menu's. | R3-E01, E07, E08 |
| S2 the lit item by route | /portfolio/export/{id} lights the staff owner (Trainees, or Programme trainees) even for a staff member who is also a former trainee; the Unavailable page passes Page=, so nothing is lit. | R3-E09, E10, E19; edge list |
| S3 the refusals | “The last day cannot be after today (2026-10-26).” for both ends; P22 the before-start refusal; P23 “Choose the last day.” (NEW, PROPOSED). EndDate becomes nullable on the page, so an empty field reaches the press check. | R3-P02, P07, P22, P23 |
| S4 the profile's read | Spec only: GetTraineeProfileByIdQuery returns null for unknown and out of scope (one body); the catch shows fixed words; the DTO carries the end's kind and day, the training year at the end, the last ratified decision and MayExport; StatePanel gets LoadError and OnRetry, Try again focusing the h1. File: TraineeProfileEdit.razor. | R3-P12, P13, P15; below |
| S5 the account sentence | The DATA row (roles now, locked, erased, STAR count); P05 keeps the STAR clause, P11 none; P24 the Assessor variant; P25 locked, the link alone. Erased: no sentence, and the name is the pseudonym. | R3-P05, P11, P24, P25 |
| S6 Export's changes | ValidationException mapped to its field (E11, E12), every other exception to E13's words; “returns the file name” struck (PortfolioExportResult.FileName exists). E19 and E20 new. | R3-E11, E12, E13, E19, E20 |
| S7 built subtitles | My progress ended: “Your record of the programme you ended: what your curriculum held you to in each period.”; the active profile: “Update curriculum and completion details for this trainee.”; My activities: the built sentence, then the NEW one. | R3-R01, R02, R05, R10; R3-P01–P04, P06–P10, P16, P17, P19, P20, P22, P23; R3-R07 |
| S8 the record line and Home's rule | The “Your record:” line is MyProgress.razor's, around EndedRecord (its golden-markup test unchanged), never on the staff page; if the standing read fails it shows My activities only. H01 “Your 15 STARs in force, each with its certificate to download.” (NEW). GetFormerTraineeHomeQuery's rule below; “None of your STARs is in force now; My authorisations lists them.” (NEW, PROPOSED, no board). | R3-H01, R3-R01; below |
| S9 the Where column | Corrected below. The certificate download handler needs no change. Home's former-trainee branch becomes FormerTraineeRecordCard under DashboardFrame (DashboardLinkAuthorizationTests renders it). | below; R3-H01–H07 |
| S10 the not-a-hash family | Three refusals (below); V06 draws the third. | R3-V06; Messages |
| S11 the closed list | ListEndedProfilesQuery paged in SQL, 20 a page, the latest end first; PagerControls new on this page; “‹n› ended” AdmissionText's (NEW). C06, C07 new. | R3-C01–C07 |
| S12 Mark complete's address | “sent to molefe@kgk.wombat.local” (P03); “sent to ‹address›” (P19, P20); NEW, from TraineeProfileDto.Email. | R3-P03, P19, P20 |
| S13 the result's Verify link | A plain <a href="/portfolio/verify">, a full navigation (Verify is static). | R3-E03, E05, E06, E08 (E20 is before the export) |
| S14 P18 | The Curriculum read-out is built today (an ended profile's CurriculumId is final); the T312 DATA row is gone; the “after T312” pill only on the annotation of what T312 changes there. | R3-P18 |
| S15 R10 | The standing's counts bracketed: “‹m› of 17 EPAs …”, “level 5: ‹…› · level 4: ‹…›”. | R3-R10 |
| S16 Steps | T1–T12. | R3-0-steps, R3-0-steps-2 |
| S17 the brackets | Every FIXTURES value under “The record” drawn on R01–R08; the “did not arrive” notes on R03, R04, R06 gone; X08's ‹name as stored› gone (X09 reads Demo Administrator). | R3-R01–R08, R3-X08, X09 |
| S18 R07 | Trail Home › My progress › My activities at 1280, 768 and 390; caption “Every activity filed by you or about you, with who has it now and its credit.” (the shipped table has none; the build adds it). | R3-R07 |
| S19 the refusal before a dialog | div.validation-message role="alert", the focus in the field; DESIGN.md text below. | R3-P02, P22, P23 (and P07) |
| S20 Verify's answer markup | div.action-result tabindex="-1" holding the success Alert (role="status"), “Export verified.” bold, the sentences, then the details list; no section, no .verify-answer, no hand-built stripe. V10 not focused. | R3-V02, V03, V07, V09, V10 |
| S21 the details list | dl.detail-list, with “.verify-page .detail-list dd{min-width:0}” and minmax(0, 1fr) in f13r3.css; .verify-details gone. | R3-V02–V10 |
| S22 the Programme cell | .person-cell; .closed-when gone. | R3-C01–C07 |
| S23 the badges | Withdrawn and Inactive .badge-withdrawn; Completed .badge-completed; Active a plain word. badge-draft on 0 R3 boards. | R3-C02, C04–C07, P05, P11, P21; P01–P04, P08–P10, P22, P23 |
| S24 C04 | Every Outcome a real word, Withdrawn, with ‹day›; the Programme th and td .col-wrap, its cells overflow-wrap: anywhere; re-measured at 901. | R3-C04; A4 below |
### Nits, N1–N16
| Item | What changed | Where |
| N1 | E17's caption: the header reads from the subject query. | R3-E17 |
| N2 | An activity opened from My activities by a former trainee has the trail “Home › the activity”. | R3-0-steps-2 |
| N3 | A holder of Trainee acting as another role is offered My progress and My data rights only. | edge list |
| N4 | V05 and V06: the input carries autofocus (static SSR). | R3-V05, V06; build rules |
| N5 | The header slot: PageHeader's Actions on the review page (moved out of SubtitleContent), a new Actions slot on the programme page. | R3-X01–X10; build rules |
| N6 | P05, P11: one Status row. Completed reads the badge “Completed” and the day, “2026-10-26” (the built “Completed: ” moves into the badge); Inactive reads the badge and the built “Left the programme: 2026-10-24”. | R3-P05, P11, P21 |
| N7 | R07's Mini-CEX row carries “to David Naidoo” under its title (.activity-link-to). | R3-R07 |
| N8 | The build states that need a states.md row. | R3-0-steps-2 |
| N9 | Illustrations bracket what is the bracketed person's: E17's start date, E18's and P19's address and training year, E16's ‹about 200›, C04's curricula. | R3-E16, E17, E18, P19, C04 |
| N10 | No pronoun in any canvas note: “Lerato Molefe's”, or “their”. | every R3 board's caption |
| N11 | One stylesheet: every board links ds/wombat/tokens.css, ds/wombat/components/bundle.css, then f13r3.css; f13r3.css's section 1 the canvas, section 2 each NEW rule under a comment naming its class-table row; a board's <style> is body{margin:0} alone. | every R3 board; project/f13r3.css |
| N12 | E14, E15: the trail Home › Trainees › Export portfolio. | R3-E14, E15 |
| N13 | aria-describedby from Mark complete… and Deactivate… to their notes (#end-mc-note, #end-dc-note). | R3-P01–P04, P06–P10, P16, P17, P19, P20, P22, P23 |
| N14 | h2#end-h has no tabindex; .end-help and .end-decision gone (the decision line a plain p, the help .field-help). | R3-P* |
| N15 | V05, V06: aria-describedby="hash-e hash-h hash-cmds", the refusal first. | R3-V05, V06; DESIGN.md text |
| N16 | The pager's size select is 28 px at 390 (shared PagerControls, every flow's pager): the one exception on these boards; proposed as a system task, “.pager .form-select{min-height:2.75rem}” below 641px, not in f13r3.css. | R3-C04-390; A3 |
### Steps, T1–T12
| Item | Where |
| T1–T12 | R3-0-steps and R3-0-steps-2, each by its id |
### Also, A1–A5
| Item | What changed | Where |
| A1 the primary on top | Stacked form actions and stacked row actions keep the primary on top (.user-page .form-actions column-reverse; Export and Verify have one button); dialogs at 390 stack the action on top (.dialog-card .form-actions). | every 390 board |
| A2 a form's parts | Export: form.form-container holds .form-grid, the note and .form-actions, a child of .export-page (which spaces the result and the form, not their parts). Verify: form.form-container holds .form-group and .form-actions, a child of .verify-page. Profile details: the form inside section.user-section. End this programme: the section itself (no form element; the triggers are buttons). | R3-E*, V*, P* |
| A3, A4 | Measured: below. | R3-0-spec-3 |
| A5 | No password and no secret drawn; no board holds two states that cannot hold at once (a dialog over the page that opened it holds at once). | every R3 board |
### The commands and queries, as written
| Command or query | As written (each PROPOSED) |
| MayExport (F1) | bool on CommitteeReviewDetailDto, ProgrammeTraineeDto and TraineeProfileDto: MayReadAsync(caller, trainee user id), the export command's own check. Shown on the review page to the Coordinator, the Committee member, both speciality admins, the Institutional admin and the Administrator where it is true (X01, X03, X05, X09, X10); the Institutional admin and the Administrator see no progress link there (RosterRoles). |
| MarkTraineeCompleteCommand, DeactivateTraineeProfileCommand (M2) | IRequest<ProgrammeEndResult> with TakenBack (int), the count ProgrammeEndCredit.SaveAsync took back; the page builds “‹n› activities observed after that day no longer count.” from it, nothing when 0. The dialog's count is GetTakeBackForecastQuery, read when it opens for the typed day. |
| GetExportSubjectQuery (M4) | Name, institution, curriculum, training year, programme start, last day, ProfileId (the preferred profile from PreferredProfiles; null when none); null for every refused cause. |
| GetTraineeProfileByIdQuery (S4) | Null for unknown and out of scope; the DTO adds EndKind, EndDay, TrainingYearAtEnd, LastRatifiedDecision, MayExport, Email. |
| GetFormerTraineeHomeQuery (S8) | The preferred profile; the end's kind (CompletedOn or DeactivatedOn) and day; the training year at the end; the STAR count under F6 (any ratified decision, “in force” counted); null for a current Trainee (the trainee_record claim is issued for any profile). |
| ListEndedProfilesQuery (S11) | Paged in SQL, 20 a page, the latest end first; start, end day, kind, training year at the end; the institution for the Administrator; an erased person under the pseudonym. |
| VerifyExportQuery (S10) | Strip all whitespace, lower-case; three refusals: a wrong length, a non-hex character, the 12-digit prefix; then the first generation, the count and the last export (ORDER BY ExportedOn, Id) with the first exporter. |
### The build rules
- S2: /portfolio/export/{id} lights its staff owner by route, whatever personal links the caller has; the Unavailable page passes Page= to PageHeader, so nothing is lit.
- S6: Export maps a ValidationException to its field; every other exception shows E13's fixed words.
- S13: the result's Verify link is a plain href, a full navigation.
- S19: a refusal found before any dialog stays beside its field, role="alert", the focus in the field.
- N4: Verify's input carries autofocus when the page renders a refusal (static SSR, no script).
- N5: PageHeader's Actions on the review page; a new Actions slot on the programme page.
### Where (S9, corrected)
| Change | Where |
| GetExportSubjectQuery, VerifyExportQuery, ExportPortfolioCommand | Features/Reporting |
| GetLastRatifiedDecisionQuery | Features/CommitteeDecisions |
| the take-back count; GetTakeBackForecastQuery | Features/Trainees/ProgrammeEndCredit (internal) |
| GetFormerTraineeHomeQuery; FormerTraineeRecordCard under DashboardFrame | Features/Dashboards; Components/Dashboard |
| PortfolioExportCleanupJob removed | Infrastructure/Scheduling/Jobs, DependencyInjection.cs and its ScheduledJobDefinition row |
| the profile page | TraineeProfileEdit.razor |
| the EPA page's link condition | EpaProgress.razor |
| My authorisations' policy | Pages/Portfolio/MyAuthorisations.razor (the certificate download handler needs no change) |
| the record line | MyProgress.razor, around EndedRecord |

######## BOARD R3-0-spec-3.dc.html — R3-0-spec-3 · Spec (3 of 3)
> DESIGN.md text; the class table; messages by kind; DATA; Illustrations; brackets; edge cases; A3's and A4's measurements.
### DESIGN.md text
| DESIGN.md section | The text it changes |
| § Dashboard page (no role) | A record holder's Home is one emphasised card, “Your training record”, led by the end and its day, the record kept read-only, then the record's pages, each a link with a line; My authorisations only where a ratified STAR decision exists (F6), its count “in force”; no subtitle, no header action. A former trainee with a staff role keeps that Home (Q25). |
| § The NavMenu's personal links | A trainee record without the Trainee role adds Export portfolio between My progress and My data rights (Q2); it lights itself and draws no trail (F4). My authorisations and My activities are pages under My progress. |
| § Detail page and the title rule | A trainee's profile is “‹name›'s programme” (h1, tab, crumb); the staff export “Export ‹name›'s portfolio”; an ended record shows the outcome first, one Status row. |
| § Form page | Export's dates open on the programme start and today, or the last day; both required; the result above the form, focused, naming the file; From after To refused under From. |
| § Button system (amended, S19) | The end's two triggers are outline buttons in their own section, apart from Save profile, each described by its note; Mark complete's confirm primary, Deactivate's filled danger. A refusal found before any dialog stays beside its field with the focus in it, role="alert" so it is read at once. |
| § Badges (F10) | Completed green (.badge-completed); Withdrawn and Inactive grey (.badge-withdrawn, the neutral tint under its own name), never .badge-draft; Active a plain word everywhere in the summary. |
| § Table system | Completed and closed: Name as the row's header (“Name and institution” for the Administrator), View not Edit, the latest end first, a caption that does not repeat the heading, StackWide, PagerControls past 20. |
| § Page-level patterns | The Unavailable page for the export and the profile: one body for every cause, no form, one way on, nothing lit. |
| § Account / auth page's static pages | Verify's answer is an ActionResult holding one success Alert, its first sentence bold, the details list beneath (F9; replaces round 2's h2); focused after a press, left alone from a link. A refusal is named first by the field (aria-describedby, N15), and the field carries autofocus. |
### Classes not in bundle.css v33
| Class | NEW, or reuses | Why |
| .record-links, .record-link-why, .record-link-none, .record-link-plain | NEW | Home's list of the record's pages; the no-STAR row as text. |
| .ended-line, .record-card-body | NEW | The card's lead sentence; the card body's column. |
| .record-line, .record-line-label | NEW | My progress's “Your record:” line. |
| .export-page, .export-note | NEW | Export's column and 390 scope; the same-file note. |
| .verify-page, .verify-ids-note | NEW | Verify's column and 390 scope; the ids sentence. Its dl is .detail-list, with “.verify-page .detail-list dd{min-width:0}” and minmax(0, 1fr). |
| .hash-value | NEW | A hash or a user id, wrapping in its cell. |
| .cmd-list | NEW | The field's command list. |
| .outcome-cell | NEW | The closed list's Outcome cell. |
| .closed-list | NEW | The closed table's scope for its wrapping cells (S24). |
| .badge-withdrawn | NEW | F10: Withdrawn and Inactive, grey. |
| .account-open | NEW | The account's link to the user page, a 44 px line at 390. |
| .verify-answer, .verify-answer-title, .verify-details, .closed-when, .end-help, .end-decision | out | Gone this round (S20, S21, S22, N14). |
| .user-page, .user-section, .role-rows, .role-row, .role-row-note | reuses flow 12's | The profile's one column, the stacked Save row, the 44 px rows. |
| .person-cell | reuses flow 11's | The closed list's Name and Programme cells. |
| .detail-card--empty, .state-panel-copy, .form-actions | reuses | The Unavailable page. |
| .review-progress-link, .history-note, .has-note, .activity-link-to, .epa-auth-link, .star-grid, .star-card, .my-authorisations | reuses | As flows 03, 05, 07 and 09 built them. |
### Canvas-only classes
- Canvas only, in f13r3.css's section 1: .sheet, .w1280/.w768/.w390/.w1440, .board-head, .board-title, .board-sub, .chips, .chip and its kinds, .frame, .fh-800/844/1024, .anno, .anno-focus, .anno-scrim, .doc.
### Messages by kind
| Kind | Words (NEW beside the words they replace) |
| Result | NEW “Your portfolio exported: portfolio-aba6ce7943c2.pdf. The download should start automatically. Anyone holding the file can check it on Verify portfolio export.” for “Portfolio exported successfully. The download should start automatically.” · built “Trainee marked complete. The Trainee role has been removed and a graduation email sent.”, NEW sentence “‹n› activities observed after that day no longer count.” when the count is not 0 (M2) · “Trainee profile deactivated. The last day in the programme is recorded as 2026-10-24, and the Trainee role has been removed. 1 activity observed after that day no longer counts.” for “… Their last day …” · “Export verified. Wombat generated this PDF: its content is byte for byte an export Wombat recorded.” NEW for “This PDF was generated by Wombat on 2026-10-26 20:51 UTC.” |
| Refusal | built “The start date must be before or equal to the end date.” · NEW PROPOSED “Choose the from date.” / “Choose the to date.” · NEW “The last day cannot be after today (2026-10-26).” for “The completion date cannot be after today (2026-10-26).” · NEW (F11) “The last day cannot be before the programme start date (2023-01-15).” · NEW PROPOSED “Choose the last day.” · built “Enter the content hash to check.” · NEW PROPOSED (S10) “This is not a SHA-256 hash: it has ‹n› characters, and a SHA-256 hash has 64 hexadecimal digits (0 to 9, a to f).”; “This is not a SHA-256 hash: “‹c›” is not a hexadecimal digit (0 to 9, a to f).”; “This is not a SHA-256 hash: it has 12 characters, and a SHA-256 hash has 64 hexadecimal digits (0 to 9, a to f). The 12 in a file's name are only its start.” · built “No matching export found. This hash does not correspond to any PDF generated by this system.” + NEW “One changed byte changes the whole hash: check that it was computed from the file you were sent.” · NEW “Portfolio unavailable” / “This portfolio does not exist, or you cannot export it.” · NEW “Profile unavailable” / “This trainee profile does not exist, or you cannot open it.” for “… The trainee profile could not be loaded.” |
| Failure | NEW “The portfolio could not be exported just now. Nothing was downloaded. Try again, or come back in a few minutes.” · NEW “Could not load this export. …” · NEW “The export could not be checked just now. Try again in a few minutes.” for “… Please try again in a few minutes.” · NEW “The programme could not be ended just now. Nothing has changed. …” · NEW “Could not load this trainee profile. …” · built “Could not load your Home. …” |
| Hint | NEW From “Your programme started on 2023-01-15.” / “Lerato Molefe's programme started on 2023-01-15.”; To “Today, on the South African calendar.” / “Your last day in the programme.” / NEW (S6) “Lerato Molefe's last day in the programme.” · NEW “Exporting the same dates again gives the same file, until the record changes.” · NEW the hash field's help and command list, for “Enter the hash from the PDF footer” · NEW the last day's help, for “Mark complete records it as the graduation day, … (D54). …” · NEW the two ends' notes · built “Leave this empty to derive it from the curriculum window.” |
| Notice | NEW Home's card (“You completed your programme on 2026-10-26.” / “Your programme ended on 2026-10-24.”, “Your record is kept, read-only: …”, “Your 15 STARs in force, each with its certificate to download.” (S8), the other lines) for “You hold no role at the moment. …” · NEW PROPOSED “None of your STARs is in force now; My authorisations lists them.” · NEW Export's subtitle, for “Generate a PDF export of the trainee's portfolio.” · NEW Verify's subtitle, for “Check whether a PDF was produced by this system.” · NEW “People are shown by their Wombat user ids, never by name. …” · NEW dialog titles “Mark Lerato Molefe's programme complete?” (for “Mark this programme complete?”) and “Deactivate Pieter du Plessis's profile?” (for “Deactivate this trainee profile?”); their added sentences “… sent to molefe@kgk.wombat.local.” (S12), “Lerato Molefe keeps the record, read-only, and can still export the portfolio.”, “: 1 activity, observed 2026-10-25, will stop counting.” (M2), “This cannot be undone.” · NEW the profile's h1 “Lerato Molefe's programme” for “Trainee profile” · built subtitles (S7): “Update curriculum and completion details for this trainee.”, “Your record of the programme you ended: what your curriculum held you to in each period.”, “Everything you have filed or logged, newest encounter first.” + NEW “Read-only: your programme ended on 2026-10-24, so nothing new can be filed.” · NEW My activities' note row · NEW the account's sentences (S5): “… can still sign in to read the record, export the portfolio and download the STAR certificates.”, “… can still sign in to read the record and export the portfolio.”, “‹registrar› now holds Assessor. They can still read the record and export the portfolio.” (PROPOSED) |
| Empty | NEW “No STAR was issued to you, so there is no certificate to download.” · NEW PROPOSED “No programme has ended yet.” / “A trainee is listed here once their programme is marked complete or a withdrawal is recorded, on their programme's page.” · built “No active authorisations yet” |
| Row and table | NEW caption “Programmes that have ended, the latest end first. An ended record is read-only.” for “Completed and closed trainee profiles” · NEW View, named “View Lerato Molefe's record”, for Edit, named “Edit Lerato Molefe” · NEW “‹n› ended” (AdmissionText) · NEW My activities' caption “Every activity filed by you or about you, with who has it now and its credit.” |
### DATA the build must add
- MayExport on the three DTOs (F1)
- the take-back count returned by both end commands, and its forecast (M2)
- ProfileId on the export subject (M4)
- the profile's end kind and day, training year at the end, last ratified decision (S4)
- roles now, locked, erased and the STAR count for the account sentence (S5)
- Home's preferred profile, end kind, training year at the end, STAR count (S8)
- the export subject's name, institution, curriculum, year, start and last day
- Verify's first generation, count, last export and first exporter
- the closed list paged, with the institution for the Administrator (S11)
- My activities' note row's reason
- nothing for P18's label (S14)
### Illustrations
- Illustrations (chip and title): R3-H03, H04, H07, X08, X09, X10, E16, E17, E18, E19, E20, P07, P16, P17, P18, P19, P20, P21, P24, P25, C04, C05, C06, R10. Build states with no chip: P22, P23 (and the loading, failure and refusal boards). C07 is A.1.14's.
### Brackets kept
- Kept, as FIXTURES lists them: H03's and H04's ‹name as stored› and H03's recent decisions; H07's, E18's and P19's 60-character name; E17's ‹institution› and ‹registrar›; P16–P18's and P20–P25's ‹registrar›, ‹curriculum›, ‹day›, ‹address›, ‹n›; C04's ‹further people› and ‹curriculum›; C05's second person; C07's ‹deleted_user_…› and its day; R10's ‹PAED-016›, ‹PAED-017› and the standing's counts; V04's 5c171ec5‹…›; X08's ‹registrar›, ‹period›, ‹review type›, ‹panel›.
- Kept because only a capture holds them (none arrived): listed on R3-0-spec, first section.
### Edge cases
- No STAR decision at all (du Plessis): Home's row plain text; no My authorisations on the record line (R3-H02, R3-R02, R3-R09).
- A staff role later: the staff Home, no record line; personal links My progress, Export portfolio, My data rights (R3-H03).
- Neither role nor record: “No role assigned” (R3-H04). No profile at /portfolio/export: Portfolio unavailable (R3-E19).
- S2: a staff member who is also a former trainee, on /portfolio/export/{id}: the staff owner lit, by route.
- N3: a holder of Trainee acting as another role is offered My progress and My data rights only (Export portfolio is in the Trainee menu).
- The pasted hash (R3-V09); the 12-digit prefix (R3-V06); a trainee elsewhere (R3-X08); another institution (R3-E17, R3-P13); a moved profile (R3-P18); a period's last month (R3-P17); the first day (R3-P16); From after To (R3-E11); two people with one name (R3-C05); a 60-character name (R3-H07, E18, P19); an erased person (R3-C07, and the account sentence's absence).
### A3 and A4, measured
Rendered with bundle.css v33 and f13r3.css in Chromium (the canvas's fonts fall back, so heights are near, not exact), 632 renders: every 1280 board at 1280, 1100, 1000, 901, 900 and 768 with its width freed, every 768 board at 768, every 390 board at 390. A box past the right edge counts unless it is a visually hidden stacked header.
| Width | Boards rendered | Sideways |
| 1280 | 89 | 0 px |
| 1100 | 89 | 0 px |
| 1000 | 89 | 0 px |
| 901 | 89 | R3-R03-1280: 17 px, R3-R06-1280: 17 px |
| 900 | 89 | 0 px |
| 768 | 97 | 0 px |
| 390 | 90 | 0 px |
Not 0 at 901: R3-R03 and R3-R06 overflow by 17 px there: the Entrustment card's details list (flow 05's .details-list, a 128 px term column) in the narrow column of .details-grid--reverse, between 901 and about 940 px. It is flow 05's shipped EPA page, not changed here; proposed as a system task (minmax(0, 1fr) on .details-list's value column).
A3, 44 px at 390: 90 boards measured, 583 controls. Short of 44 px, as exceptions: R3-C04-390: select: 102050100 (28) (N16: the shared pager's size select). Inline or shipped-table links under 44 px: R3-E05-390: a: Verify portfolio export (20); R3-E06-390: a: Verify portfolio export (20); R3-R07-390: a: DOPS (Paediatrics) · ‹EPA› · ‹date› (20), a: Teaching Session Log · ‹date› (20), a: Teaching Session Log · ‹date› (20), a: Teaching Session Log · ‹date› (20). The Verify link inside the export's result sentence is inline text (WCAG 2.5.8's inline exception). R3-R07's one-line activity links are flow 03's shipped My activities table (.activity-link, 20 px on one line; a two-line link reaches 44 px), not changed here: proposed with N16's task.

######## BOARD R3-0-steps.dc.html — R3-0-steps · Steps (1 of 2)
> The 26 steps: each board, today's Expect, and what it says once built (round 2's clauses stand unless an item changes them).
### The 26 steps
| Step | Board | Today's Expect | Once built |
| 5.9 | R3-E01–E03 | “The page opens with the last twelve months filled in. … ‘Portfolio exported successfully. …’” | Opens with From 2023-01-15 and To 2026-10-26, no trail; the result “Your portfolio exported: portfolio-aba6ce7943c2.pdf. …”, focused, its Verify link a plain href. |
| 5.10 | R3-P01, R3-E04, E05 | Users › Lerato Molefe, the id typed into the export address; Note “No page links to the staff export” | Trainees › Edit Lerato Molefe's profile › “Export Lerato Molefe's portfolio” in the page header (F1, T7); the trail Home › Trainees › Lerato Molefe's programme › Export portfolio (M4); dates already set; the same file. The Note goes; F-5.10a closes. |
| 5.11 | — | the PDF | unchanged |
| 5.12 | R3-X01, R3-E06 | the export address typed with the id sent to him | Committee reviews › Lerato Molefe · 2026 S2 › “Export Lerato Molefe's portfolio” in the header (F1, T7); the trail Home › Programme trainees › Lerato Molefe › Export portfolio; the same file. |
| 5.13 | R3-V02, V03 | “… when Wombat generated the file, in UTC” | “Export verified. …” in ActionResult; First generated 2026-10-26 22:51 SAST; Exported 3 times, the last on 2026-10-26 22:52 SAST; Trainee and First exported by 2b5abc2f-9e37-4714-bb7a-8e48fa11f699; Filter; File portfolio-aba6ce7943c2.pdf. Upper case the same. F-5.13a and F-5.13b close. |
| 5.14 | R3-V04, V05 | “No matching export found. …” · “Enter the content hash to check.” | The first with its third sentence (NEW); spaces only under the field, autofocus. |
| 5.15 | — |  | unchanged |
| 5.16 | R3-P01, P02 | “… Once confirmed, the request is refused with ‘The completion date cannot be after today (2026-10-26).’” | No dialog: refused on the press, under the field, role="alert", “The last day cannot be after today (2026-10-26).”, 2027-01-14 kept and focused. |
| 5.17 | R3-P03, P05, R3-C01 | “Trainee marked complete. …”; “Name:” in the summary; “Trainee summary”; Back to trainees | The dialog sent to molefe@kgk.wombat.local, no count clause (0); the result unchanged (count 0, no sentence; T9); the page “Lerato Molefe's programme”, no “Name:” row and no “Trainee summary” heading (T6), one Status row; Completed and closed: View, “View Lerato Molefe's record”. |
| 5.18 | — (Q12) | “… her committee has ratified her final entrustment decisions …” | That clause drops; the mail links My progress. |
| 5.19 | — |  | unchanged |
| 5.20 | R3-R01, R03 | the notice, Your programme, the cards, the standing | The built subtitle; “Your record:” My authorisations, My activities; PAED-001's page shows Open My authorisations. |
| 5.21 | R3-H01 | “Home says ‘You completed your programme on 2026-10-26, so no target applies …’” | The card: “You completed your programme on 2026-10-26.”, “Your 15 STARs in force, …” among the four lines; the menu with Export portfolio. F-5.21a closes. |
| 5.22 | R3-R08 | the first three read “You cannot open this page” | My authorisations opens; My committee reviews, MSF reports and New activity still refuse. F-5.22a closes. |
| 5.23 | R3-E07, E08, R3-V07 | “The export page is not in her menu …” | Export portfolio in the menu, lit, no trail (F4, T7); To 2026-10-26; portfolio-344dd52bdded.pdf, once, 22:56 SAST. F-5.23a closes. |
| 5.24–5.26 | — |  | unchanged |
| 5.27 | R3-P08–P11, R3-C02 | “Trainee profile deactivated. Their last day … ‘Withdrawn 2026-10-24’” | The dialog “…: 1 activity, observed 2026-10-25, will stop counting.” (T9); the result built from the count, 1; Inactive (grey) first; Withdrawn 2026-10-24, the latest end first (T6). |
| 5.28 | R3-H02, R3-R02, R3-R07 | “Home shows the former trainee's card …” | Home: “Your programme ended on 2026-10-24.”, STARs plain text; My progress's built subtitle and line; My activities: Home › My progress › My activities, All activities (7), the Mini-CEX with its note row. |
| 5.29 | — |  | unchanged |
| 6.21 | R3-R05, R3-R06 | “It lists no PAED-012 card while PAED-012 is paused …” | After T312 (T5): the Do and bullets invert: a PAED-012 card headed “(no longer in use)” and its standing row; “15 of 15 · level 5: 9 of 9”; PAED-012's page reads as R3-R06; PAED-006's card “PAED-006 — Managing long-term health conditions (LTHCs) in children”. F-6.21a closes. |
| 6.40 | as R3-R01 | “A KGK-001 card has appeared …” | After T312: 15 EPAs, no KGK-001. F-6.40a closes. |
| 6.41 | as R3-R02 | “A KGK-001 card has appeared on his record too” | After T312: no KGK-001; “15 with no decision, of 15”. |
| A.7.4 | R3-H01 at 390, R3-H08 | “the menu holds Home, My progress and My data rights” | Home, My progress, Export portfolio, My data rights. F-A.7.4a closes. |
| A.7.12 | R3-V07 at 390 (M5, T8) | “The verify page's result fits the width.” | The second file's answer fits 390, the details one column, the ids and the hash wrapping. |

######## BOARD R3-0-steps-2.dc.html — R3-0-steps-2 · Steps (2 of 2)
> T1–T12 by id, and each journey recounted after F1, F4 and M4.
### T1–T11
| Item | What changes |
| T1 5.8 | Expect unchanged; Note rewritten: the “last point … to download” Note is false after Q3. F-5.22a closes. |
| T2 the profile-opening steps | 2.30 (Sipho Ndlovu, edit?id=6), 6.34 (Anele Dlamini, id 3), 6.36 (Sipho Ndlovu) and A.7.9 each gain the h1 “‹name›'s programme”, no Back to trainees, End this programme and the header action; their quoted words stay. 2.39 and 2.40: unchanged. |
| T3 A.7.9 | “the closed list stacked, View 44 px” (not “scrolls in its own container, each Edit”); “three fields plus the last day and two triggers” (not “stacks its four fields”). |
| T4 A.2.1, A.2.9 | The nine-job list becomes eight (portfolio-export-cleanup gone); “All nine had run” becomes “All eight”; A.2.9's title “the other six jobs” becomes “the other five”, its cleanup bullet goes, the 90-day Note goes; Gap F-A.2.9a closes with T313. |
| T5 6.21, 6.40, 6.41 | As R3-0-steps (after T312); F-6.21a and F-6.40a close. |
| T6 the summary and the order | 5.17, 5.27, A.1.14: no “Name:” and no “Trainee summary” heading; the closed list the latest end first at A.1.14, 5.27 and 6.34; A.1.14 gains the closed list's line with the erased row (R3-C07). |
| T7 the header and the trail | 5.10 and 5.12: the action in the page header (F1); 5.23: no trail (F4), Export portfolio lit. |
| T8 A.7.12 | R3-V07 at 390. |
| T9 the counts | 5.17's result built from 0 (nothing added); 5.27's from 1; 5.27's dialog “will stop counting”. |
| T10 the rest | 5.9 (dates, result), 5.10 (Note goes, F-5.10a), 5.12, 5.13 (F-5.13a, F-5.13b), 5.14, 5.16, 5.18, 5.20, 5.21 (F-5.21a), 5.22 (F-5.22a), 5.23 (F-5.23a), 5.28, A.1.6 and A.4.6 (Pieter du Plessis's Home: R3-H02's card, his menu with Export portfolio), A.7.4 (F-A.7.4a): as R3-0-steps. Unchanged: 5.1–5.7, 5.11, 5.15, 5.19, 5.24–5.26, 5.29. coverage.md's rows at lines 143, 223, 323, 385 and 494: each re-pointed to these R3 boards (their text not in hand here: check). |
| T11 N2, N8, A4 | N2: an activity opened from My activities by a former trainee has the trail “Home › the activity”. N8: states.md rows for E09–E15, E19, H05, H06, P06, P12–P15, P22, P23, V08–V10, X03–X07. A4: after the end, Pieter Smit reaches the export from the review page (R3-X03), or the programme page by its address, /programme/trainees/2 (R3-X04). |
### T12 the journeys
| Journey (presses + fields) | Today | 1280 | 390 | Changed by F1, F4, M4? |
| A STAR certificate from Home | refused | 2 + 0: My authorisations · Download certificate for PAED-001 | 2 + 0 | no |
| Her export (5.23) | 1 + 2: (the address) · (From) · Export PDF | 2 + 0: Export portfolio (menu or Home's card) · Export PDF | 2 + 0 from Home's card; 3 + 0 by Menu | no (F4 removes the trail only) |
| Prof Mbatha's (5.10) | 4 + 3: Users · (Molefe) · Show · Lerato Molefe · (the address) · (From) · Export PDF | 4 + 0: Trainees · Edit Lerato Molefe's profile · Export Lerato Molefe's portfolio · Export PDF | 5 + 0: Menu first | no (F1 moves no action on the profile; M4 adds a link back) |
| Mr Smit's (5.12) | 1 + 2: (the address sent to him) · (From) · Export PDF | 4 + 0: Committee reviews · Lerato Molefe · 2026 S2 · Export Lerato Molefe's portfolio (header) · Export PDF | 5 + 0 | no (F1 moves the action from the subtitle to the header; the count stands) |
| Verifying (5.13) | 1 + 2: (the address) · (the hash) · Verify; upper case refused | 1 + 2 | 1 + 2 | no |
| Marking complete (5.17) | 4 + 0: Trainees · Edit · Mark complete · Mark complete | 4 + 0: Trainees · Edit Lerato Molefe's profile · Mark complete… · Mark complete | 5 + 0 | no |
| A withdrawal (5.27) | 4 + 1: Trainees · Edit · (2026-10-24) · Deactivate · Deactivate | 4 + 1: Trainees · Edit Pieter du Plessis's profile · (2026-10-24) · Deactivate… · Deactivate | 5 + 1 | no |

######## BOARD R3-H01-1280.dc.html — R3-H01 · Home · Lerato Molefe after graduating (5.21; at 390, A.7.4) · 1280
> One emphasised card: the end and its day, the record kept read-only, then the record's four pages, each with what it holds; the STAR count says “in force” (S8). Menu: Home; My progress, Export portfolio, My data rights (Q2). FormerTraineeRecordCard under DashboardFrame (S9).
> chips: PROPOSED · Focus: the h1, on arrival
Wombat
Menu
Close
[aria-label: Main]
Home
My progress
Export portfolio
My data rights
Lerato Molefe
Sign out
## Home
Your training record
You completed your programme on 2026-10-26.
Your record is kept, read-only: no target applies to you any more.
My progress
Every period of your programme, to training year 4, and your standing against Annexure A.
My authorisations
Your 15 STARs in force, each with its certificate to download.
My activities
Every activity you filed or that was filed about you, read-only.
Export portfolio
A PDF of your record. Anyone holding it can check it on Verify portfolio export.

######## BOARD R3-H01-390.dc.html — R3-H01 · Home · Lerato Molefe after graduating (5.21; at 390, A.7.4) · 390
> One emphasised card: the end and its day, the record kept read-only, then the record's four pages, each with what it holds; the STAR count says “in force” (S8). Menu: Home; My progress, Export portfolio, My data rights (Q2). FormerTraineeRecordCard under DashboardFrame (S9).
> chips: PROPOSED · Focus: the h1, on arrival
Wombat
Menu
Close
[aria-label: Main]
Home
My progress
Export portfolio
My data rights
Lerato Molefe
Sign out
## Home
Your training record
You completed your programme on 2026-10-26.
Your record is kept, read-only: no target applies to you any more.
My progress
Every period of your programme, to training year 4, and your standing against Annexure A.
My authorisations
Your 15 STARs in force, each with its certificate to download.
My activities
Every activity you filed or that was filed about you, read-only.
Export portfolio
A PDF of your record. Anyone holding it can check it on Verify portfolio export.

######## BOARD R3-H02-1280.dc.html — R3-H02 · Home · Pieter du Plessis after withdrawing (5.28) · 1280
> No STAR decision of any kind (F6): the STARs row is plain text, with no link (N6).
> chips: PROPOSED
Wombat
Menu
Close
[aria-label: Main]
Home
My progress
Export portfolio
My data rights
Pieter du Plessis
Sign out
## Home
Your training record
Your programme ended on 2026-10-24.
Your record is kept, read-only: no target applies to you any more.
My progress
Every period of your programme, to training year 2, and your standing against Annexure A.
STARs
No STAR was issued to you, so there is no certificate to download.
My activities
Every activity you filed or that was filed about you, read-only.
Export portfolio
A PDF of your record. Anyone holding it can check it on Verify portfolio export.

######## BOARD R3-H02-390.dc.html — R3-H02 · Home · Pieter du Plessis after withdrawing (5.28) · 390
> No STAR decision of any kind (F6): the STARs row is plain text, with no link (N6).
> chips: PROPOSED
Wombat
Menu
Close
[aria-label: Main]
Home
My progress
Export portfolio
My data rights
Pieter du Plessis
Sign out
## Home
Your training record
Your programme ended on 2026-10-24.
Your record is kept, read-only: no target applies to you any more.
My progress
Every period of your programme, to training year 2, and your standing against Annexure A.
STARs
No STAR was issued to you, so there is no certificate to download.
My activities
Every activity you filed or that was filed about you, read-only.
Export portfolio
A PDF of your record. Anyone holding it can check it on Verify portfolio export.

######## BOARD R3-H03-1280.dc.html — R3-H03 · Home · a former trainee holding Assessor · Illustration · 1280
> The Assessor Home as flow 04 built it, no record line (Q25); the personal links My progress, Export portfolio, My data rights.
> chips: Illustration · PROPOSED
Wombat
Acting as Assessor
Menu
Close
[aria-label: Main]
Acting as Assessor
Home
Activity inbox
My progress
Export portfolio
My data rights
‹name as stored›
Sign out
## Home
Assessor · Semester 2, 2026
Waiting for you
Nothing is waiting for you.
Open Activity inbox
Recent decisions
‹recent decisions, as flow 04 lists them›

######## BOARD R3-H03-390.dc.html — R3-H03 · Home · a former trainee holding Assessor · Illustration · 390
> The Assessor Home as flow 04 built it, no record line (Q25); the personal links My progress, Export portfolio, My data rights.
> chips: Illustration · PROPOSED
Wombat
Acting as Assessor
Menu
Close
[aria-label: Main]
Acting as Assessor
Home
Activity inbox
My progress
Export portfolio
My data rights
‹name as stored›
Sign out
## Home
Assessor · Semester 2, 2026
Waiting for you
Nothing is waiting for you.
Open Activity inbox
Recent decisions
‹recent decisions, as flow 04 lists them›

######## BOARD R3-H04-1280.dc.html — R3-H04 · Home · neither a role nor a record · Illustration · 1280
> Flow 01's “No role assigned”, unchanged; menu Home and My data rights.
> chips: Illustration
Wombat
Menu
Close
[aria-label: Main]
Home
My data rights
‹name as stored›
Sign out
## Home
No role assigned
Your account holds no role at the moment, so there is nothing to show you here. An administrator can give you one.

######## BOARD R3-H04-390.dc.html — R3-H04 · Home · neither a role nor a record · Illustration · 390
> Flow 01's “No role assigned”, unchanged; menu Home and My data rights.
> chips: Illustration
Wombat
Menu
Close
[aria-label: Main]
Home
My data rights
‹name as stored›
Sign out
## Home
No role assigned
Your account holds no role at the moment, so there is nothing to show you here. An administrator can give you one.

######## BOARD R3-H05-1280.dc.html — R3-H05 · Home · loading · 1280
> The card's skeleton; no link until the read is in.
Wombat
Menu
Close
[aria-label: Main]
Home
My progress
Export portfolio
My data rights
Lerato Molefe
Sign out
## Home
Loading your Home.
Your training record

######## BOARD R3-H05-390.dc.html — R3-H05 · Home · loading · 390
> The card's skeleton; no link until the read is in.
Wombat
Menu
Close
[aria-label: Main]
Home
My progress
Export portfolio
My data rights
Lerato Molefe
Sign out
## Home
Loading your Home.
Your training record

######## BOARD R3-H06-1280.dc.html — R3-H06 · Home · the load error · 1280
> Flow 01's words and Try again; Try again focuses the h1 once the read is in.
Wombat
Menu
Close
[aria-label: Main]
Home
My progress
Export portfolio
My data rights
Lerato Molefe
Sign out
## Home
Could not load your Home. Nothing has changed. Try again, or come back in a few minutes.
Try again

######## BOARD R3-H06-390.dc.html — R3-H06 · Home · the load error · 390
> Flow 01's words and Try again; Try again focuses the h1 once the read is in.
Wombat
Menu
Close
[aria-label: Main]
Home
My progress
Export portfolio
My data rights
Lerato Molefe
Sign out
## Home
Could not load your Home. Nothing has changed. Try again, or come back in a few minutes.
Try again

######## BOARD R3-H07-1280.dc.html — R3-H07 · Home · a 60-character name · Illustration · 1280
> The top bar cuts the name at 28 characters (its link names it whole); the open menu's foot wraps it. The card has no name in it.
> chips: Illustration
Wombat
Menu
Close
[aria-label: Main]
Home
My progress
Export portfolio
My data rights
‹a 60-character name as stored, given names and surname, here›
Sign out
## Home
Your training record
You completed your programme on 2026-10-26.
Your record is kept, read-only: no target applies to you any more.
My progress
Every period of your programme, to training year 4, and your standing against Annexure A.
My authorisations
Your 15 STARs in force, each with its certificate to download.
My activities
Every activity you filed or that was filed about you, read-only.
Export portfolio
A PDF of your record. Anyone holding it can check it on Verify portfolio export.

######## BOARD R3-H07-390.dc.html — R3-H07 · Home · a 60-character name · Illustration · 390
> The top bar cuts the name at 28 characters (its link names it whole); the open menu's foot wraps it. The card has no name in it.
> chips: Illustration
Wombat
Menu
Close
[aria-label: Main]
Home
My progress
Export portfolio
My data rights
‹a 60-character name as stored, given names and surname, here›
Sign out
## Home
Your training record
You completed your programme on 2026-10-26.
Your record is kept, read-only: no target applies to you any more.
My progress
Every period of your programme, to training year 4, and your standing against Annexure A.
My authorisations
Your 15 STARs in force, each with its certificate to download.
My activities
Every activity you filed or that was filed about you, read-only.
Export portfolio
A PDF of your record. Anyone holding it can check it on Verify portfolio export.

######## BOARD R3-H08-390.dc.html — R3-H08 · Home · A.7.4's menu open · 390
> Home; My progress, Export portfolio, My data rights; Lerato Molefe's name and Sign out at its foot.
> chips: PROPOSED
Wombat
Menu
Close
[aria-label: Main]
Home
My progress
Export portfolio
My data rights
Lerato Molefe
Sign out

######## BOARD R3-X01-1280.dc.html — R3-X01 · Committee review · Pieter Smit, current (5.12) · 1280
> “Export Lerato Molefe's portfolio” in the page header's actions (PageHeader's Actions, moved out of SubtitleContent; F1, N5); shown because MayExport is true. “Lerato Molefe's progress” stays in the subtitle.
> chips: PROPOSED
Wombat
Acting as Coordinator
Menu
Close
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
Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
Committee reviews
- Lerato Molefe · 2026 S2
## Lerato Molefe · 2026 S2
Pre-graduation review · Paed Annual Review Panel · Training year 4Lerato Molefe's progress
Export Lerato Molefe's portfolio
The rest of the committee review as flow 07 built it: decision Graduate (programme complete), Ratified 2026-10-26.

######## BOARD R3-X01-390.dc.html — R3-X01 · Committee review · Pieter Smit, current (5.12) · 390
> “Export Lerato Molefe's portfolio” in the page header's actions (PageHeader's Actions, moved out of SubtitleContent; F1, N5); shown because MayExport is true. “Lerato Molefe's progress” stays in the subtitle.
> chips: PROPOSED
Wombat
Acting as Coordinator
Menu
Close
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
Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
Committee reviews
- Lerato Molefe · 2026 S2
## Lerato Molefe · 2026 S2
Pre-graduation review · Paed Annual Review Panel · Training year 4Lerato Molefe's progress
Export Lerato Molefe's portfolio
The rest of the committee review as flow 07 built it: decision Graduate (programme complete), Ratified 2026-10-26.

######## BOARD R3-X02-1280.dc.html — R3-X02 · Programme page · Pieter Smit, current · 1280
> The outline header action, in the programme page's new Actions slot (N5); the subtitle as RegistrarWords.Subtitle builds it.
> chips: PROPOSED
Wombat
Acting as Coordinator
Menu
Close
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
Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
Programme trainees
- Lerato Molefe
## Lerato Molefe
Training year 4 · Semester 2, 2026 · Kgosi Kgari Teaching Hospital, Paediatrics
Export Lerato Molefe's portfolio
The rest of the registrar page as flow 06 built it.

######## BOARD R3-X02-390.dc.html — R3-X02 · Programme page · Pieter Smit, current · 390
> The outline header action, in the programme page's new Actions slot (N5); the subtitle as RegistrarWords.Subtitle builds it.
> chips: PROPOSED
Wombat
Acting as Coordinator
Menu
Close
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
Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
Programme trainees
- Lerato Molefe
## Lerato Molefe
Training year 4 · Semester 2, 2026 · Kgosi Kgari Teaching Hospital, Paediatrics
Export Lerato Molefe's portfolio
The rest of the registrar page as flow 06 built it.

######## BOARD R3-X03-1280.dc.html — R3-X03 · Committee review · Pieter Smit, after the end · 1280
> Still shown: MayExport is true for an ended record (MayReadAsync on the trainee's user id).
> chips: PROPOSED
Wombat
Acting as Coordinator
Menu
Close
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
Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
Committee reviews
- Lerato Molefe · 2026 S2
## Lerato Molefe · 2026 S2
Pre-graduation review · Paed Annual Review Panel · Training year 4Lerato Molefe's progress
Export Lerato Molefe's portfolio
The rest of the committee review as flow 07 built it: decision Graduate (programme complete), Ratified 2026-10-26.

######## BOARD R3-X03-390.dc.html — R3-X03 · Committee review · Pieter Smit, after the end · 390
> Still shown: MayExport is true for an ended record (MayReadAsync on the trainee's user id).
> chips: PROPOSED
Wombat
Acting as Coordinator
Menu
Close
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
Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
Committee reviews
- Lerato Molefe · 2026 S2
## Lerato Molefe · 2026 S2
Pre-graduation review · Paed Annual Review Panel · Training year 4Lerato Molefe's progress
Export Lerato Molefe's portfolio
The rest of the committee review as flow 07 built it: decision Graduate (programme complete), Ratified 2026-10-26.

######## BOARD R3-X04-1280.dc.html — R3-X04 · Programme page · Pieter Smit, after the end, by its address · 1280
> Programme trainees lists current trainees only, so Pieter Smit reaches this by /programme/trainees/2 (A4); the owner is still lit.
> chips: PROPOSED
Wombat
Acting as Coordinator
Menu
Close
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
Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
Programme trainees
- Lerato Molefe
## Lerato Molefe
Training year 4 · Programme completed 2026-10-26 · Kgosi Kgari Teaching Hospital, Paediatrics
Export Lerato Molefe's portfolio
The rest of the registrar page as flow 06 built it.

######## BOARD R3-X04-390.dc.html — R3-X04 · Programme page · Pieter Smit, after the end, by its address · 390
> Programme trainees lists current trainees only, so Pieter Smit reaches this by /programme/trainees/2 (A4); the owner is still lit.
> chips: PROPOSED
Wombat
Acting as Coordinator
Menu
Close
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
Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
Programme trainees
- Lerato Molefe
## Lerato Molefe
Training year 4 · Programme completed 2026-10-26 · Kgosi Kgari Teaching Hospital, Paediatrics
Export Lerato Molefe's portfolio
The rest of the registrar page as flow 06 built it.

######## BOARD R3-X05-1280.dc.html — R3-X05 · Committee review · David Naidoo, Committee member · 1280
> A Committee member oversees Lerato Molefe, so MayExport is true and the action is in the header.
> chips: PROPOSED
Wombat
Acting as Committee member
Menu
Close
[aria-label: Main]
Acting as Committee member
Home
Programme trainees
Committee reviews
Decision panels
My data rights
David Naidoo
Sign out
[aria-label: Breadcrumb]
Home
Committee reviews
- Lerato Molefe · 2026 S2
## Lerato Molefe · 2026 S2
Pre-graduation review · Paed Annual Review Panel · Training year 4Lerato Molefe's progress
Export Lerato Molefe's portfolio
The rest of the committee review as flow 07 built it: decision Graduate (programme complete), Ratified 2026-10-26.

######## BOARD R3-X05-390.dc.html — R3-X05 · Committee review · David Naidoo, Committee member · 390
> A Committee member oversees Lerato Molefe, so MayExport is true and the action is in the header.
> chips: PROPOSED
Wombat
Acting as Committee member
Menu
Close
[aria-label: Main]
Acting as Committee member
Home
Programme trainees
Committee reviews
Decision panels
My data rights
David Naidoo
Sign out
[aria-label: Breadcrumb]
Home
Committee reviews
- Lerato Molefe · 2026 S2
## Lerato Molefe · 2026 S2
Pre-graduation review · Paed Annual Review Panel · Training year 4Lerato Molefe's progress
Export Lerato Molefe's portfolio
The rest of the committee review as flow 07 built it: decision Graduate (programme complete), Ratified 2026-10-26.

######## BOARD R3-X06-1280.dc.html — R3-X06 · Programme page · Refilwe Mokoena, Speciality admin · 1280
> A Speciality admin of Paediatrics oversees Lerato Molefe.
> chips: PROPOSED
Wombat
Acting as Speciality admin
Menu
Close
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
Refilwe Mokoena
Sign out
[aria-label: Breadcrumb]
Home
Programme trainees
- Lerato Molefe
## Lerato Molefe
Training year 4 · Semester 2, 2026 · Kgosi Kgari Teaching Hospital, Paediatrics
Export Lerato Molefe's portfolio
The rest of the registrar page as flow 06 built it.

######## BOARD R3-X06-390.dc.html — R3-X06 · Programme page · Refilwe Mokoena, Speciality admin · 390
> A Speciality admin of Paediatrics oversees Lerato Molefe.
> chips: PROPOSED
Wombat
Acting as Speciality admin
Menu
Close
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
Refilwe Mokoena
Sign out
[aria-label: Breadcrumb]
Home
Programme trainees
- Lerato Molefe
## Lerato Molefe
Training year 4 · Semester 2, 2026 · Kgosi Kgari Teaching Hospital, Paediatrics
Export Lerato Molefe's portfolio
The rest of the registrar page as flow 06 built it.

######## BOARD R3-X07-1280.dc.html — R3-X07 · Programme page · Kabelo Sithole, Sub-speciality admin · 1280
> A Sub-speciality admin of Paediatrics / Paediatrics oversees Lerato Molefe.
> chips: PROPOSED
Wombat
Acting as Sub-speciality admin
Menu
Close
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
Kabelo Sithole
Sign out
[aria-label: Breadcrumb]
Home
Programme trainees
- Lerato Molefe
## Lerato Molefe
Training year 4 · Semester 2, 2026 · Kgosi Kgari Teaching Hospital, Paediatrics
Export Lerato Molefe's portfolio
The rest of the registrar page as flow 06 built it.

######## BOARD R3-X07-390.dc.html — R3-X07 · Programme page · Kabelo Sithole, Sub-speciality admin · 390
> A Sub-speciality admin of Paediatrics / Paediatrics oversees Lerato Molefe.
> chips: PROPOSED
Wombat
Acting as Sub-speciality admin
Menu
Close
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
Kabelo Sithole
Sign out
[aria-label: Breadcrumb]
Home
Programme trainees
- Lerato Molefe
## Lerato Molefe
Training year 4 · Semester 2, 2026 · Kgosi Kgari Teaching Hospital, Paediatrics
Export Lerato Molefe's portfolio
The rest of the registrar page as flow 06 built it.

######## BOARD R3-X08-1280.dc.html — R3-X08 · Committee review · Pieter Smit, a trainee elsewhere · Illustration · 1280
> The review of ‹registrar›, whose review has TraineeElsewhere set: no “‹registrar›'s progress” link and no export action. MayExport is false.
> chips: PROPOSED · Illustration
Wombat
Acting as Coordinator
Menu
Close
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
Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
Committee reviews
- ‹registrar› · ‹period›
## ‹registrar› · ‹period›
‹review type› · ‹panel› · Training year ‹n›
MayExport is false: the export would be refused, so the header offers nothing. The rest as flow 07 built it.

######## BOARD R3-X08-390.dc.html — R3-X08 · Committee review · Pieter Smit, a trainee elsewhere · Illustration · 390
> The review of ‹registrar›, whose review has TraineeElsewhere set: no “‹registrar›'s progress” link and no export action. MayExport is false.
> chips: PROPOSED · Illustration
Wombat
Acting as Coordinator
Menu
Close
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
Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
Committee reviews
- ‹registrar› · ‹period›
## ‹registrar› · ‹period›
‹review type› · ‹panel› · Training year ‹n›
MayExport is false: the export would be refused, so the header offers nothing. The rest as flow 07 built it.

######## BOARD R3-X09-1280.dc.html — R3-X09 · Committee review · devadmin, the Administrator · Illustration · 1280
> Lerato Molefe · 2026 S2 read by the Administrator: no progress link (RosterRoles), the export action in the header (MayExport is true: the Administrator is not refused).
> chips: PROPOSED · Illustration
Wombat
Acting as Administrator
Menu
Close
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
Demo Administrator
Sign out
[aria-label: Breadcrumb]
Home
Committee reviews
- Lerato Molefe · 2026 S2
## Lerato Molefe · 2026 S2
Pre-graduation review · Paed Annual Review Panel · Training year 4
Export Lerato Molefe's portfolio
The rest of the committee review as flow 07 built it: decision Graduate (programme complete), Ratified 2026-10-26.

######## BOARD R3-X09-390.dc.html — R3-X09 · Committee review · devadmin, the Administrator · Illustration · 390
> Lerato Molefe · 2026 S2 read by the Administrator: no progress link (RosterRoles), the export action in the header (MayExport is true: the Administrator is not refused).
> chips: PROPOSED · Illustration
Wombat
Acting as Administrator
Menu
Close
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
Demo Administrator
Sign out
[aria-label: Breadcrumb]
Home
Committee reviews
- Lerato Molefe · 2026 S2
## Lerato Molefe · 2026 S2
Pre-graduation review · Paed Annual Review Panel · Training year 4
Export Lerato Molefe's portfolio
The rest of the committee review as flow 07 built it: decision Graduate (programme complete), Ratified 2026-10-26.

######## BOARD R3-X10-1280.dc.html — R3-X10 · Committee review · Nolwazi Mbatha, Institutional admin · Illustration · 1280
> The same: no progress link (RosterRoles), the export action in the header.
> chips: PROPOSED · Illustration
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Committee reviews
- Lerato Molefe · 2026 S2
## Lerato Molefe · 2026 S2
Pre-graduation review · Paed Annual Review Panel · Training year 4
Export Lerato Molefe's portfolio
The rest of the committee review as flow 07 built it: decision Graduate (programme complete), Ratified 2026-10-26.

######## BOARD R3-X10-390.dc.html — R3-X10 · Committee review · Nolwazi Mbatha, Institutional admin · Illustration · 390
> The same: no progress link (RosterRoles), the export action in the header.
> chips: PROPOSED · Illustration
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Committee reviews
- Lerato Molefe · 2026 S2
## Lerato Molefe · 2026 S2
Pre-graduation review · Paed Annual Review Panel · Training year 4
Export Lerato Molefe's portfolio
The rest of the committee review as flow 07 built it: decision Graduate (programme complete), Ratified 2026-10-26.

######## BOARD R3-E01-1280.dc.html — R3-E01 · Export portfolio · Lerato Molefe's own, before (5.9) · 1280
> Trainee; Export portfolio lit, no trail (one rule, F4: a page the menu offers draws none). From on the programme start, To on today, South African calendar.
> chips: PROPOSED
Wombat
Acting as Trainee
Menu
Close
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
Lerato Molefe
Sign out
## Export portfolio
A PDF of your portfolio for the dates you choose: your progress, your STARs, your committee's decisions and your activities.
From date *required
[value: 2023-01-15]
Your programme started on 2023-01-15.
To date *required
[value: 2026-10-26]
Today, on the South African calendar.
Exporting the same dates again gives the same file, until the record changes.
Export PDF

######## BOARD R3-E01-390.dc.html — R3-E01 · Export portfolio · Lerato Molefe's own, before (5.9) · 390
> Trainee; Export portfolio lit, no trail (one rule, F4: a page the menu offers draws none). From on the programme start, To on today, South African calendar.
> chips: PROPOSED
Wombat
Acting as Trainee
Menu
Close
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
Lerato Molefe
Sign out
## Export portfolio
A PDF of your portfolio for the dates you choose: your progress, your STARs, your committee's decisions and your activities.
From date *required
[value: 2023-01-15]
Your programme started on 2023-01-15.
To date *required
[value: 2026-10-26]
Today, on the South African calendar.
Exporting the same dates again gives the same file, until the record changes.
Export PDF

######## BOARD R3-E02-1280.dc.html — R3-E02 · Export portfolio · generating · 1280
> “Generating...” keeps the focus, aria-disabled; a second press sends nothing.
> chips: Focus: Export PDF, generating
Wombat
Acting as Trainee
Menu
Close
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
Lerato Molefe
Sign out
## Export portfolio
A PDF of your portfolio for the dates you choose: your progress, your STARs, your committee's decisions and your activities.
Generating the PDF.
From date *required
[value: 2023-01-15]
Your programme started on 2023-01-15.
To date *required
[value: 2026-10-26]
Today, on the South African calendar.
Exporting the same dates again gives the same file, until the record changes.
Generating...

######## BOARD R3-E02-390.dc.html — R3-E02 · Export portfolio · generating · 390
> “Generating...” keeps the focus, aria-disabled; a second press sends nothing.
> chips: Focus: Export PDF, generating
Wombat
Acting as Trainee
Menu
Close
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
Lerato Molefe
Sign out
## Export portfolio
A PDF of your portfolio for the dates you choose: your progress, your STARs, your committee's decisions and your activities.
Generating the PDF.
From date *required
[value: 2023-01-15]
Your programme started on 2023-01-15.
To date *required
[value: 2026-10-26]
Today, on the South African calendar.
Exporting the same dates again gives the same file, until the record changes.
Generating...

######## BOARD R3-E03-1280.dc.html — R3-E03 · Export portfolio · Lerato Molefe's own, after (5.9) · 1280
> The result above the form, naming the file (PortfolioExportResult.FileName), focused; its Verify link a plain href, a full navigation (S13).
> chips: Focus: the result
Wombat
Acting as Trainee
Menu
Close
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
Lerato Molefe
Sign out
## Export portfolio
A PDF of your portfolio for the dates you choose: your progress, your STARs, your committee's decisions and your activities.
Your portfolio exported: portfolio-aba6ce7943c2.pdf. The download should start automatically. Anyone holding the file can check it on Verify portfolio export.
From date *required
[value: 2023-01-15]
Your programme started on 2023-01-15.
To date *required
[value: 2026-10-26]
Today, on the South African calendar.
Exporting the same dates again gives the same file, until the record changes.
Export PDF

######## BOARD R3-E03-390.dc.html — R3-E03 · Export portfolio · Lerato Molefe's own, after (5.9) · 390
> The result above the form, naming the file (PortfolioExportResult.FileName), focused; its Verify link a plain href, a full navigation (S13).
> chips: Focus: the result
Wombat
Acting as Trainee
Menu
Close
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
Lerato Molefe
Sign out
## Export portfolio
A PDF of your portfolio for the dates you choose: your progress, your STARs, your committee's decisions and your activities.
Your portfolio exported: portfolio-aba6ce7943c2.pdf. The download should start automatically. Anyone holding the file can check it on Verify portfolio export.
From date *required
[value: 2023-01-15]
Your programme started on 2023-01-15.
To date *required
[value: 2026-10-26]
Today, on the South African calendar.
Exporting the same dates again gives the same file, until the record changes.
Export PDF

######## BOARD R3-E04-1280.dc.html — R3-E04 · Export portfolio · Nolwazi Mbatha, before (5.10) · 1280
> Reached from the programme's header action; Trainees lit; the name and ProfileId read before the form by GetExportSubjectQuery; the middle crumb through PageHeader.Trail, to /admin/trainees/edit?id=2 (M4).
> chips: PROPOSED
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
Lerato Molefe's programme
- Export portfolio
## Export Lerato Molefe's portfolio
Kgosi Kgari Teaching Hospital · Paediatric EPA Curriculum (11.1) · Training year 4
From date *required
[value: 2023-01-15]
Lerato Molefe's programme started on 2023-01-15.
To date *required
[value: 2026-10-26]
Today, on the South African calendar.
Exporting the same dates again gives the same file, until the record changes.
Export PDF

######## BOARD R3-E04-390.dc.html — R3-E04 · Export portfolio · Nolwazi Mbatha, before (5.10) · 390
> Reached from the programme's header action; Trainees lit; the name and ProfileId read before the form by GetExportSubjectQuery; the middle crumb through PageHeader.Trail, to /admin/trainees/edit?id=2 (M4).
> chips: PROPOSED
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
Lerato Molefe's programme
- Export portfolio
## Export Lerato Molefe's portfolio
Kgosi Kgari Teaching Hospital · Paediatric EPA Curriculum (11.1) · Training year 4
From date *required
[value: 2023-01-15]
Lerato Molefe's programme started on 2023-01-15.
To date *required
[value: 2026-10-26]
Today, on the South African calendar.
Exporting the same dates again gives the same file, until the record changes.
Export PDF

######## BOARD R3-E05-1280.dc.html — R3-E05 · Export portfolio · Nolwazi Mbatha, after (5.10) · 1280
> The same file name as 5.9's.
> chips: Focus: the result
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
Lerato Molefe's programme
- Export portfolio
## Export Lerato Molefe's portfolio
Kgosi Kgari Teaching Hospital · Paediatric EPA Curriculum (11.1) · Training year 4
Lerato Molefe's portfolio exported: portfolio-aba6ce7943c2.pdf. The download should start automatically. Anyone holding the file can check it on Verify portfolio export.
From date *required
[value: 2023-01-15]
Lerato Molefe's programme started on 2023-01-15.
To date *required
[value: 2026-10-26]
Today, on the South African calendar.
Exporting the same dates again gives the same file, until the record changes.
Export PDF

######## BOARD R3-E05-768.dc.html — R3-E05 · Export portfolio · Nolwazi Mbatha, after (5.10) · 768
> The same file name as 5.9's.
> chips: Focus: the result
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
Lerato Molefe's programme
- Export portfolio
## Export Lerato Molefe's portfolio
Kgosi Kgari Teaching Hospital · Paediatric EPA Curriculum (11.1) · Training year 4
Lerato Molefe's portfolio exported: portfolio-aba6ce7943c2.pdf. The download should start automatically. Anyone holding the file can check it on Verify portfolio export.
From date *required
[value: 2023-01-15]
Lerato Molefe's programme started on 2023-01-15.
To date *required
[value: 2026-10-26]
Today, on the South African calendar.
Exporting the same dates again gives the same file, until the record changes.
Export PDF

######## BOARD R3-E05-390.dc.html — R3-E05 · Export portfolio · Nolwazi Mbatha, after (5.10) · 390
> The same file name as 5.9's.
> chips: Focus: the result
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
Lerato Molefe's programme
- Export portfolio
## Export Lerato Molefe's portfolio
Kgosi Kgari Teaching Hospital · Paediatric EPA Curriculum (11.1) · Training year 4
Lerato Molefe's portfolio exported: portfolio-aba6ce7943c2.pdf. The download should start automatically. Anyone holding the file can check it on Verify portfolio export.
From date *required
[value: 2023-01-15]
Lerato Molefe's programme started on 2023-01-15.
To date *required
[value: 2026-10-26]
Today, on the South African calendar.
Exporting the same dates again gives the same file, until the record changes.
Export PDF

######## BOARD R3-E06-1280.dc.html — R3-E06 · Export portfolio · Pieter Smit, after (5.12) · 1280
> Programme trainees lit; the trail by the fixed owner (Q27), its middle crumb to /programme/trainees/2 (M4).
> chips: Focus: the result
Wombat
Acting as Coordinator
Menu
Close
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
Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
Programme trainees
Lerato Molefe
- Export portfolio
## Export Lerato Molefe's portfolio
Kgosi Kgari Teaching Hospital · Paediatric EPA Curriculum (11.1) · Training year 4
Lerato Molefe's portfolio exported: portfolio-aba6ce7943c2.pdf. The download should start automatically. Anyone holding the file can check it on Verify portfolio export.
From date *required
[value: 2023-01-15]
Lerato Molefe's programme started on 2023-01-15.
To date *required
[value: 2026-10-26]
Today, on the South African calendar.
Exporting the same dates again gives the same file, until the record changes.
Export PDF

######## BOARD R3-E06-390.dc.html — R3-E06 · Export portfolio · Pieter Smit, after (5.12) · 390
> Programme trainees lit; the trail by the fixed owner (Q27), its middle crumb to /programme/trainees/2 (M4).
> chips: Focus: the result
Wombat
Acting as Coordinator
Menu
Close
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
Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
Programme trainees
Lerato Molefe
- Export portfolio
## Export Lerato Molefe's portfolio
Kgosi Kgari Teaching Hospital · Paediatric EPA Curriculum (11.1) · Training year 4
Lerato Molefe's portfolio exported: portfolio-aba6ce7943c2.pdf. The download should start automatically. Anyone holding the file can check it on Verify portfolio export.
From date *required
[value: 2023-01-15]
Lerato Molefe's programme started on 2023-01-15.
To date *required
[value: 2026-10-26]
Today, on the South African calendar.
Exporting the same dates again gives the same file, until the record changes.
Export PDF

######## BOARD R3-E07-1280.dc.html — R3-E07 · Export portfolio · Lerato Molefe's own after graduating, before (5.23) · 1280
> Export portfolio lit as a personal link, no trail (F4); To on the last day.
> chips: PROPOSED
Wombat
Menu
Close
[aria-label: Main]
Home
My progress
Export portfolio
My data rights
Lerato Molefe
Sign out
## Export portfolio
A PDF of your portfolio for the dates you choose: your progress, your STARs, your committee's decisions and your activities.
From date *required
[value: 2023-01-15]
Your programme started on 2023-01-15.
To date *required
[value: 2026-10-26]
Your last day in the programme.
Exporting the same dates again gives the same file, until the record changes.
Export PDF

######## BOARD R3-E07-390.dc.html — R3-E07 · Export portfolio · Lerato Molefe's own after graduating, before (5.23) · 390
> Export portfolio lit as a personal link, no trail (F4); To on the last day.
> chips: PROPOSED
Wombat
Menu
Close
[aria-label: Main]
Home
My progress
Export portfolio
My data rights
Lerato Molefe
Sign out
## Export portfolio
A PDF of your portfolio for the dates you choose: your progress, your STARs, your committee's decisions and your activities.
From date *required
[value: 2023-01-15]
Your programme started on 2023-01-15.
To date *required
[value: 2026-10-26]
Your last day in the programme.
Exporting the same dates again gives the same file, until the record changes.
Export PDF

######## BOARD R3-E08-1280.dc.html — R3-E08 · Export portfolio · Lerato Molefe's own after graduating, after (5.23) · 1280
> The new file: its Progress per EPA says Lerato Molefe completed the programme.
> chips: Focus: the result
Wombat
Menu
Close
[aria-label: Main]
Home
My progress
Export portfolio
My data rights
Lerato Molefe
Sign out
## Export portfolio
A PDF of your portfolio for the dates you choose: your progress, your STARs, your committee's decisions and your activities.
Your portfolio exported: portfolio-344dd52bdded.pdf. The download should start automatically. Anyone holding the file can check it on Verify portfolio export.
From date *required
[value: 2023-01-15]
Your programme started on 2023-01-15.
To date *required
[value: 2026-10-26]
Your last day in the programme.
Exporting the same dates again gives the same file, until the record changes.
Export PDF

######## BOARD R3-E08-390.dc.html — R3-E08 · Export portfolio · Lerato Molefe's own after graduating, after (5.23) · 390
> The new file: its Progress per EPA says Lerato Molefe completed the programme.
> chips: Focus: the result
Wombat
Menu
Close
[aria-label: Main]
Home
My progress
Export portfolio
My data rights
Lerato Molefe
Sign out
## Export portfolio
A PDF of your portfolio for the dates you choose: your progress, your STARs, your committee's decisions and your activities.
Your portfolio exported: portfolio-344dd52bdded.pdf. The download should start automatically. Anyone holding the file can check it on Verify portfolio export.
From date *required
[value: 2023-01-15]
Your programme started on 2023-01-15.
To date *required
[value: 2026-10-26]
Your last day in the programme.
Exporting the same dates again gives the same file, until the record changes.
Export PDF

######## BOARD R3-E09-1280.dc.html — R3-E09 · Portfolio unavailable · Anele Dlamini types Lerato Molefe's address · 1280
> One page for every cause, no form, naming nobody (Q19); the command's “You are not authorized to export this portfolio.” stays behind it. Passes Page=, so nothing is lit (S2).
> chips: PROPOSED
Wombat
Acting as Trainee
Menu
Close
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
Anele Dlamini
Sign out
## Portfolio unavailable
This portfolio does not exist, or you cannot export it.
Go to Home

######## BOARD R3-E09-390.dc.html — R3-E09 · Portfolio unavailable · Anele Dlamini types Lerato Molefe's address · 390
> One page for every cause, no form, naming nobody (Q19); the command's “You are not authorized to export this portfolio.” stays behind it. Passes Page=, so nothing is lit (S2).
> chips: PROPOSED
Wombat
Acting as Trainee
Menu
Close
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
Anele Dlamini
Sign out
## Portfolio unavailable
This portfolio does not exist, or you cannot export it.
Go to Home

######## BOARD R3-E10-1280.dc.html — R3-E10 · Portfolio unavailable · an unknown id, Nolwazi Mbatha · 1280
> The same page and words; nothing lit (S2).
> chips: PROPOSED
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
## Portfolio unavailable
This portfolio does not exist, or you cannot export it.
Go to Home

######## BOARD R3-E10-390.dc.html — R3-E10 · Portfolio unavailable · an unknown id, Nolwazi Mbatha · 390
> The same page and words; nothing lit (S2).
> chips: PROPOSED
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
## Portfolio unavailable
This portfolio does not exist, or you cannot export it.
Go to Home

######## BOARD R3-E11-1280.dc.html — R3-E11 · Export portfolio · From after To · 1280
> The command's ValidationException mapped to From (S6): refused under From, both dates kept, From focused (today's words).
> chips: Focus: From date
Wombat
Acting as Trainee
Menu
Close
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
Lerato Molefe
Sign out
## Export portfolio
A PDF of your portfolio for the dates you choose: your progress, your STARs, your committee's decisions and your activities.
From date *required
[value: 2026-11-02]
The start date must be before or equal to the end date.
Your programme started on 2023-01-15.
To date *required
[value: 2026-10-26]
Today, on the South African calendar.
Exporting the same dates again gives the same file, until the record changes.
Export PDF

######## BOARD R3-E11-390.dc.html — R3-E11 · Export portfolio · From after To · 390
> The command's ValidationException mapped to From (S6): refused under From, both dates kept, From focused (today's words).
> chips: Focus: From date
Wombat
Acting as Trainee
Menu
Close
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
Lerato Molefe
Sign out
## Export portfolio
A PDF of your portfolio for the dates you choose: your progress, your STARs, your committee's decisions and your activities.
From date *required
[value: 2026-11-02]
The start date must be before or equal to the end date.
Your programme started on 2023-01-15.
To date *required
[value: 2026-10-26]
Today, on the South African calendar.
Exporting the same dates again gives the same file, until the record changes.
Export PDF

######## BOARD R3-E12-1280.dc.html — R3-E12 · Export portfolio · an empty date · 1280
> From and To required (S11 of round 2); the ValidationException mapped to its field.
> chips: PROPOSED · Focus: From date
Wombat
Acting as Trainee
Menu
Close
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
Lerato Molefe
Sign out
## Export portfolio
A PDF of your portfolio for the dates you choose: your progress, your STARs, your committee's decisions and your activities.
From date *required
Choose the from date.
Your programme started on 2023-01-15.
To date *required
[value: 2026-10-26]
Today, on the South African calendar.
Exporting the same dates again gives the same file, until the record changes.
Export PDF

######## BOARD R3-E12-390.dc.html — R3-E12 · Export portfolio · an empty date · 390
> From and To required (S11 of round 2); the ValidationException mapped to its field.
> chips: PROPOSED · Focus: From date
Wombat
Acting as Trainee
Menu
Close
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
Lerato Molefe
Sign out
## Export portfolio
A PDF of your portfolio for the dates you choose: your progress, your STARs, your committee's decisions and your activities.
From date *required
Choose the from date.
Your programme started on 2023-01-15.
To date *required
[value: 2026-10-26]
Today, on the South African calendar.
Exporting the same dates again gives the same file, until the record changes.
Export PDF

######## BOARD R3-E13-1280.dc.html — R3-E13 · Export portfolio · a failed export · 1280
> Every exception but a ValidationException gets these fixed words (S6); the dates kept; Export PDF offered again.
> chips: Focus: the result
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
Lerato Molefe's programme
- Export portfolio
## Export Lerato Molefe's portfolio
Kgosi Kgari Teaching Hospital · Paediatric EPA Curriculum (11.1) · Training year 4
The portfolio could not be exported just now. Nothing was downloaded. Try again, or come back in a few minutes.
From date *required
[value: 2023-01-15]
Lerato Molefe's programme started on 2023-01-15.
To date *required
[value: 2026-10-26]
Today, on the South African calendar.
Exporting the same dates again gives the same file, until the record changes.
Export PDF

######## BOARD R3-E13-390.dc.html — R3-E13 · Export portfolio · a failed export · 390
> Every exception but a ValidationException gets these fixed words (S6); the dates kept; Export PDF offered again.
> chips: Focus: the result
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
Lerato Molefe's programme
- Export portfolio
## Export Lerato Molefe's portfolio
Kgosi Kgari Teaching Hospital · Paediatric EPA Curriculum (11.1) · Training year 4
The portfolio could not be exported just now. Nothing was downloaded. Try again, or come back in a few minutes.
From date *required
[value: 2023-01-15]
Lerato Molefe's programme started on 2023-01-15.
To date *required
[value: 2026-10-26]
Today, on the South African calendar.
Exporting the same dates again gives the same file, until the record changes.
Export PDF

######## BOARD R3-E14-1280.dc.html — R3-E14 · Export portfolio · loading · 1280
> The header with the trail Home › Trainees › Export portfolio, no name known yet (N12); a skeleton; no Export PDF.
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
- Export portfolio
## Export portfolio
Loading this export.

######## BOARD R3-E14-390.dc.html — R3-E14 · Export portfolio · loading · 390
> The header with the trail Home › Trainees › Export portfolio, no name known yet (N12); a skeleton; no Export PDF.
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
- Export portfolio
## Export portfolio
Loading this export.

######## BOARD R3-E15-1280.dc.html — R3-E15 · Export portfolio · the load error · 1280
> The subject read failed: h1 “Export portfolio” and the trail without a name (N12); Try again, no form.
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
- Export portfolio
## Export portfolio
Could not load this export. Nothing has changed. Try again, or come back in a few minutes.
Try again

######## BOARD R3-E15-390.dc.html — R3-E15 · Export portfolio · the load error · 390
> The subject read failed: h1 “Export portfolio” and the trail without a name (N12); Try again, no form.
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
- Export portfolio
## Export portfolio
Could not load this export. Nothing has changed. Try again, or come back in a few minutes.
Try again

######## BOARD R3-E16-1280.dc.html — R3-E16 · Export portfolio · ‹about 200› activities, generating · Illustration · 1280
> Nothing on the page differs from R3-E02: the wait is longer, and Generating... holds until the file is made.
> chips: Illustration
Wombat
Acting as Trainee
Menu
Close
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
Lerato Molefe
Sign out
## Export portfolio
A PDF of your portfolio for the dates you choose: your progress, your STARs, your committee's decisions and your activities.
Generating the PDF.
From date *required
[value: 2023-01-15]
Your programme started on 2023-01-15.
To date *required
[value: 2026-10-26]
Today, on the South African calendar.
Exporting the same dates again gives the same file, until the record changes.
Generating...

######## BOARD R3-E16-390.dc.html — R3-E16 · Export portfolio · ‹about 200› activities, generating · Illustration · 390
> Nothing on the page differs from R3-E02: the wait is longer, and Generating... holds until the file is made.
> chips: Illustration
Wombat
Acting as Trainee
Menu
Close
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
Lerato Molefe
Sign out
## Export portfolio
A PDF of your portfolio for the dates you choose: your progress, your STARs, your committee's decisions and your activities.
Generating the PDF.
From date *required
[value: 2023-01-15]
Your programme started on 2023-01-15.
To date *required
[value: 2026-10-26]
Today, on the South African calendar.
Exporting the same dates again gives the same file, until the record changes.
Generating...

######## BOARD R3-E17-1280.dc.html — R3-E17 · Export portfolio · a caller who also oversees elsewhere · Illustration · 1280
> The header reads from GetExportSubjectQuery (M4, N1): the registrar's name, institution, year and start, whichever institution the caller also serves.
> chips: Illustration
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
‹name as stored›
Sign out
[aria-label: Breadcrumb]
Home
Trainees
‹registrar›'s programme
- Export portfolio
## Export ‹registrar›'s portfolio
‹institution› · Paediatric EPA Curriculum (11.1) · Training year ‹n›
From date *required
‹registrar›'s programme started on ‹start date›.
To date *required
[value: 2026-10-26]
Today, on the South African calendar.
Exporting the same dates again gives the same file, until the record changes.
Export PDF

######## BOARD R3-E17-390.dc.html — R3-E17 · Export portfolio · a caller who also oversees elsewhere · Illustration · 390
> The header reads from GetExportSubjectQuery (M4, N1): the registrar's name, institution, year and start, whichever institution the caller also serves.
> chips: Illustration
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
‹name as stored›
Sign out
[aria-label: Breadcrumb]
Home
Trainees
‹registrar›'s programme
- Export portfolio
## Export ‹registrar›'s portfolio
‹institution› · Paediatric EPA Curriculum (11.1) · Training year ‹n›
From date *required
‹registrar›'s programme started on ‹start date›.
To date *required
[value: 2026-10-26]
Today, on the South African calendar.
Exporting the same dates again gives the same file, until the record changes.
Export PDF

######## BOARD R3-E18-1280.dc.html — R3-E18 · Export portfolio · a 60-character name in the staff header · Illustration · 1280
> The h1 and the trail wrap; at 390 the crumb folds to its parent.
> chips: Illustration
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
‹a 60-character name as stored, given names and surname, here›'s programme
- Export portfolio
## Export ‹a 60-character name as stored, given names and surname, here›'s portfolio
Kgosi Kgari Teaching Hospital · Paediatric EPA Curriculum (11.1) · Training year ‹n›
From date *required
‹a 60-character name as stored, given names and surname, here›'s programme started on ‹start date›.
To date *required
[value: 2026-10-26]
Today, on the South African calendar.
Exporting the same dates again gives the same file, until the record changes.
Export PDF

######## BOARD R3-E18-390.dc.html — R3-E18 · Export portfolio · a 60-character name in the staff header · Illustration · 390
> The h1 and the trail wrap; at 390 the crumb folds to its parent.
> chips: Illustration
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
‹a 60-character name as stored, given names and surname, here›'s programme
- Export portfolio
## Export ‹a 60-character name as stored, given names and surname, here›'s portfolio
Kgosi Kgari Teaching Hospital · Paediatric EPA Curriculum (11.1) · Training year ‹n›
From date *required
‹a 60-character name as stored, given names and surname, here›'s programme started on ‹start date›.
To date *required
[value: 2026-10-26]
Today, on the South African calendar.
Exporting the same dates again gives the same file, until the record changes.
Export PDF

######## BOARD R3-E19-1280.dc.html — R3-E19 · Portfolio unavailable · David Naidoo, acting as Assessor, types /portfolio/export · Illustration · 1280
> No trainee profile, so the personal export has no subject (F6): the Unavailable page; nothing lit.
> chips: Illustration · PROPOSED
Wombat
Acting as Assessor
Menu
Close
[aria-label: Main]
Acting as Assessor
Home
Activity inbox
My data rights
David Naidoo
Sign out
## Portfolio unavailable
This portfolio does not exist, or you cannot export it.
Go to Home

######## BOARD R3-E19-390.dc.html — R3-E19 · Portfolio unavailable · David Naidoo, acting as Assessor, types /portfolio/export · Illustration · 390
> No trainee profile, so the personal export has no subject (F6): the Unavailable page; nothing lit.
> chips: Illustration · PROPOSED
Wombat
Acting as Assessor
Menu
Close
[aria-label: Main]
Acting as Assessor
Home
Activity inbox
My data rights
David Naidoo
Sign out
## Portfolio unavailable
This portfolio does not exist, or you cannot export it.
Go to Home

######## BOARD R3-E20-1280.dc.html — R3-E20 · Export portfolio · Pieter Smit after the end (from R3-X03) · Illustration · 1280
> To on 2026-10-26 with the hint for the last day (NEW); From on the start.
> chips: Illustration · PROPOSED
Wombat
Acting as Coordinator
Menu
Close
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
Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
Programme trainees
Lerato Molefe
- Export portfolio
## Export Lerato Molefe's portfolio
Kgosi Kgari Teaching Hospital · Paediatric EPA Curriculum (11.1) · Training year 4
From date *required
[value: 2023-01-15]
Lerato Molefe's programme started on 2023-01-15.
To date *required
[value: 2026-10-26]
Lerato Molefe's last day in the programme.
Exporting the same dates again gives the same file, until the record changes.
Export PDF

######## BOARD R3-E20-390.dc.html — R3-E20 · Export portfolio · Pieter Smit after the end (from R3-X03) · Illustration · 390
> To on 2026-10-26 with the hint for the last day (NEW); From on the start.
> chips: Illustration · PROPOSED
Wombat
Acting as Coordinator
Menu
Close
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
Pieter Smit
Sign out
[aria-label: Breadcrumb]
Home
Programme trainees
Lerato Molefe
- Export portfolio
## Export Lerato Molefe's portfolio
Kgosi Kgari Teaching Hospital · Paediatric EPA Curriculum (11.1) · Training year 4
From date *required
[value: 2023-01-15]
Lerato Molefe's programme started on 2023-01-15.
To date *required
[value: 2026-10-26]
Lerato Molefe's last day in the programme.
Exporting the same dates again gives the same file, until the record changes.
Export PDF

######## BOARD R3-V01-1280.dc.html — R3-V01 · Verify portfolio export · blank · 1280
> Static, under the signed-out bar; a GET form, no script.
> chips: PROPOSED · Focus: the h1, on arrival
Wombat
Sign in
## Verify portfolio export
Check whether a PDF was produced by Wombat, without signing in.
Content hash (SHA-256) *required
The file's SHA-256: 64 hexadecimal digits, computed on your computer from the PDF you were sent. Upper or lower case, and spaces or line breaks in it, are accepted.
- Windows: certutil -hashfile <file> SHA256, or in PowerShell Get-FileHash <file>
- macOS: shasum -a 256 <file>
- Linux: sha256sum <file>
Verify

######## BOARD R3-V01-390.dc.html — R3-V01 · Verify portfolio export · blank · 390
> Static, under the signed-out bar; a GET form, no script.
> chips: PROPOSED · Focus: the h1, on arrival
Wombat
Sign in
## Verify portfolio export
Check whether a PDF was produced by Wombat, without signing in.
Content hash (SHA-256) *required
The file's SHA-256: 64 hexadecimal digits, computed on your computer from the PDF you were sent. Upper or lower case, and spaces or line breaks in it, are accepted.
- Windows: certutil -hashfile <file> SHA256, or in PowerShell Get-FileHash <file>
- macOS: shasum -a 256 <file>
- Linux: sha256sum <file>
Verify

######## BOARD R3-V02-1280.dc.html — R3-V02 · Verify · 5.13 verified, lower case, three exports · 1280
> certutil's lower case, the value kept. The answer in ActionResult (FocusOnLoad): one success Alert, “Export verified.” bold, then the details list (dl.detail-list) beneath (F9, S20, S21). The ids and the hash wrap in their cells.
> chips: PROPOSED · Focus: the answer, after Verify
Wombat
Sign in
## Verify portfolio export
Check whether a PDF was produced by Wombat, without signing in.
Export verified. Wombat generated this PDF: its content is byte for byte an export Wombat recorded.
First generated
2026-10-26 22:51 SAST
Exported
3 times, the last on 2026-10-26 22:52 SAST
Trainee
2b5abc2f-9e37-4714-bb7a-8e48fa11f699
First exported by
2b5abc2f-9e37-4714-bb7a-8e48fa11f699
Filter
2023-01-15 to 2026-10-26
File
portfolio-aba6ce7943c2.pdf
People are shown by their Wombat user ids, never by name. The institution that issued the portfolio can say whose they are.
Content hash (SHA-256) *required
[value: aba6ce7943c20681a22f7bbfd89a51b9e19ff005892e318806c28dd215a6c995]
The file's SHA-256: 64 hexadecimal digits, computed on your computer from the PDF you were sent. Upper or lower case, and spaces or line breaks in it, are accepted.
- Windows: certutil -hashfile <file> SHA256, or in PowerShell Get-FileHash <file>
- macOS: shasum -a 256 <file>
- Linux: sha256sum <file>
Verify

######## BOARD R3-V02-768.dc.html — R3-V02 · Verify · 5.13 verified, lower case, three exports · 768
> certutil's lower case, the value kept. The answer in ActionResult (FocusOnLoad): one success Alert, “Export verified.” bold, then the details list (dl.detail-list) beneath (F9, S20, S21). The ids and the hash wrap in their cells.
> chips: PROPOSED · Focus: the answer, after Verify
Wombat
Sign in
## Verify portfolio export
Check whether a PDF was produced by Wombat, without signing in.
Export verified. Wombat generated this PDF: its content is byte for byte an export Wombat recorded.
First generated
2026-10-26 22:51 SAST
Exported
3 times, the last on 2026-10-26 22:52 SAST
Trainee
2b5abc2f-9e37-4714-bb7a-8e48fa11f699
First exported by
2b5abc2f-9e37-4714-bb7a-8e48fa11f699
Filter
2023-01-15 to 2026-10-26
File
portfolio-aba6ce7943c2.pdf
People are shown by their Wombat user ids, never by name. The institution that issued the portfolio can say whose they are.
Content hash (SHA-256) *required
[value: aba6ce7943c20681a22f7bbfd89a51b9e19ff005892e318806c28dd215a6c995]
The file's SHA-256: 64 hexadecimal digits, computed on your computer from the PDF you were sent. Upper or lower case, and spaces or line breaks in it, are accepted.
- Windows: certutil -hashfile <file> SHA256, or in PowerShell Get-FileHash <file>
- macOS: shasum -a 256 <file>
- Linux: sha256sum <file>
Verify

######## BOARD R3-V02-390.dc.html — R3-V02 · Verify · 5.13 verified, lower case, three exports · 390
> certutil's lower case, the value kept. The answer in ActionResult (FocusOnLoad): one success Alert, “Export verified.” bold, then the details list (dl.detail-list) beneath (F9, S20, S21). The ids and the hash wrap in their cells.
> chips: PROPOSED · Focus: the answer, after Verify
Wombat
Sign in
## Verify portfolio export
Check whether a PDF was produced by Wombat, without signing in.
Export verified. Wombat generated this PDF: its content is byte for byte an export Wombat recorded.
First generated
2026-10-26 22:51 SAST
Exported
3 times, the last on 2026-10-26 22:52 SAST
Trainee
2b5abc2f-9e37-4714-bb7a-8e48fa11f699
First exported by
2b5abc2f-9e37-4714-bb7a-8e48fa11f699
Filter
2023-01-15 to 2026-10-26
File
portfolio-aba6ce7943c2.pdf
People are shown by their Wombat user ids, never by name. The institution that issued the portfolio can say whose they are.
Content hash (SHA-256) *required
[value: aba6ce7943c20681a22f7bbfd89a51b9e19ff005892e318806c28dd215a6c995]
The file's SHA-256: 64 hexadecimal digits, computed on your computer from the PDF you were sent. Upper or lower case, and spaces or line breaks in it, are accepted.
- Windows: certutil -hashfile <file> SHA256, or in PowerShell Get-FileHash <file>
- macOS: shasum -a 256 <file>
- Linux: sha256sum <file>
Verify

######## BOARD R3-V03-1280.dc.html — R3-V03 · Verify · 5.13 the same hash in upper case · 1280
> Get-FileHash's upper case, kept in the field as typed; the same answer (VerifyExportQuery lower-cases it).
> chips: PROPOSED · Focus: the answer
Wombat
Sign in
## Verify portfolio export
Check whether a PDF was produced by Wombat, without signing in.
Export verified. Wombat generated this PDF: its content is byte for byte an export Wombat recorded.
First generated
2026-10-26 22:51 SAST
Exported
3 times, the last on 2026-10-26 22:52 SAST
Trainee
2b5abc2f-9e37-4714-bb7a-8e48fa11f699
First exported by
2b5abc2f-9e37-4714-bb7a-8e48fa11f699
Filter
2023-01-15 to 2026-10-26
File
portfolio-aba6ce7943c2.pdf
People are shown by their Wombat user ids, never by name. The institution that issued the portfolio can say whose they are.
Content hash (SHA-256) *required
[value: ABA6CE7943C20681A22F7BBFD89A51B9E19FF005892E318806C28DD215A6C995]
The file's SHA-256: 64 hexadecimal digits, computed on your computer from the PDF you were sent. Upper or lower case, and spaces or line breaks in it, are accepted.
- Windows: certutil -hashfile <file> SHA256, or in PowerShell Get-FileHash <file>
- macOS: shasum -a 256 <file>
- Linux: sha256sum <file>
Verify

######## BOARD R3-V03-390.dc.html — R3-V03 · Verify · 5.13 the same hash in upper case · 390
> Get-FileHash's upper case, kept in the field as typed; the same answer (VerifyExportQuery lower-cases it).
> chips: PROPOSED · Focus: the answer
Wombat
Sign in
## Verify portfolio export
Check whether a PDF was produced by Wombat, without signing in.
Export verified. Wombat generated this PDF: its content is byte for byte an export Wombat recorded.
First generated
2026-10-26 22:51 SAST
Exported
3 times, the last on 2026-10-26 22:52 SAST
Trainee
2b5abc2f-9e37-4714-bb7a-8e48fa11f699
First exported by
2b5abc2f-9e37-4714-bb7a-8e48fa11f699
Filter
2023-01-15 to 2026-10-26
File
portfolio-aba6ce7943c2.pdf
People are shown by their Wombat user ids, never by name. The institution that issued the portfolio can say whose they are.
Content hash (SHA-256) *required
[value: ABA6CE7943C20681A22F7BBFD89A51B9E19FF005892E318806C28DD215A6C995]
The file's SHA-256: 64 hexadecimal digits, computed on your computer from the PDF you were sent. Upper or lower case, and spaces or line breaks in it, are accepted.
- Windows: certutil -hashfile <file> SHA256, or in PowerShell Get-FileHash <file>
- macOS: shasum -a 256 <file>
- Linux: sha256sum <file>
Verify

######## BOARD R3-V04-1280.dc.html — R3-V04 · Verify · 5.14 the tampered copy · 1280
> Today's two sentences, a third added (NEW).
> chips: PROPOSED · Focus: the answer
Wombat
Sign in
## Verify portfolio export
Check whether a PDF was produced by Wombat, without signing in.
No matching export found. This hash does not correspond to any PDF generated by this system. One changed byte changes the whole hash: check that it was computed from the file you were sent.
Content hash (SHA-256) *required
[value: 5c171ec5‹…›]
The file's SHA-256: 64 hexadecimal digits, computed on your computer from the PDF you were sent. Upper or lower case, and spaces or line breaks in it, are accepted.
- Windows: certutil -hashfile <file> SHA256, or in PowerShell Get-FileHash <file>
- macOS: shasum -a 256 <file>
- Linux: sha256sum <file>
Verify

######## BOARD R3-V04-390.dc.html — R3-V04 · Verify · 5.14 the tampered copy · 390
> Today's two sentences, a third added (NEW).
> chips: PROPOSED · Focus: the answer
Wombat
Sign in
## Verify portfolio export
Check whether a PDF was produced by Wombat, without signing in.
No matching export found. This hash does not correspond to any PDF generated by this system. One changed byte changes the whole hash: check that it was computed from the file you were sent.
Content hash (SHA-256) *required
[value: 5c171ec5‹…›]
The file's SHA-256: 64 hexadecimal digits, computed on your computer from the PDF you were sent. Upper or lower case, and spaces or line breaks in it, are accepted.
- Windows: certutil -hashfile <file> SHA256, or in PowerShell Get-FileHash <file>
- macOS: shasum -a 256 <file>
- Linux: sha256sum <file>
Verify

######## BOARD R3-V05-1280.dc.html — R3-V05 · Verify · 5.14 only spaces · 1280
> Refused under the field (today's words); the input carries autofocus (static SSR, N4) and names the refusal first (N15); no answer.
> chips: PROPOSED · Focus: the field
Wombat
Sign in
## Verify portfolio export
Check whether a PDF was produced by Wombat, without signing in.
Content hash (SHA-256) *required
[value:    ]
Enter the content hash to check.
The file's SHA-256: 64 hexadecimal digits, computed on your computer from the PDF you were sent. Upper or lower case, and spaces or line breaks in it, are accepted.
- Windows: certutil -hashfile <file> SHA256, or in PowerShell Get-FileHash <file>
- macOS: shasum -a 256 <file>
- Linux: sha256sum <file>
Verify

######## BOARD R3-V05-390.dc.html — R3-V05 · Verify · 5.14 only spaces · 390
> Refused under the field (today's words); the input carries autofocus (static SSR, N4) and names the refusal first (N15); no answer.
> chips: PROPOSED · Focus: the field
Wombat
Sign in
## Verify portfolio export
Check whether a PDF was produced by Wombat, without signing in.
Content hash (SHA-256) *required
[value:    ]
Enter the content hash to check.
The file's SHA-256: 64 hexadecimal digits, computed on your computer from the PDF you were sent. Upper or lower case, and spaces or line breaks in it, are accepted.
- Windows: certutil -hashfile <file> SHA256, or in PowerShell Get-FileHash <file>
- macOS: shasum -a 256 <file>
- Linux: sha256sum <file>
Verify

######## BOARD R3-V06-1280.dc.html — R3-V06 · Verify · not a hash · 1280
> A file name's 12-digit prefix: the third of the not-a-hash family (S10, NEW, PROPOSED), not “No matching export found”; autofocus, the refusal first.
> chips: PROPOSED · Focus: the field
Wombat
Sign in
## Verify portfolio export
Check whether a PDF was produced by Wombat, without signing in.
Content hash (SHA-256) *required
[value: aba6ce7943c2]
This is not a SHA-256 hash: it has 12 characters, and a SHA-256 hash has 64 hexadecimal digits (0 to 9, a to f). The 12 in a file's name are only its start.
The file's SHA-256: 64 hexadecimal digits, computed on your computer from the PDF you were sent. Upper or lower case, and spaces or line breaks in it, are accepted.
- Windows: certutil -hashfile <file> SHA256, or in PowerShell Get-FileHash <file>
- macOS: shasum -a 256 <file>
- Linux: sha256sum <file>
Verify

######## BOARD R3-V06-390.dc.html — R3-V06 · Verify · not a hash · 390
> A file name's 12-digit prefix: the third of the not-a-hash family (S10, NEW, PROPOSED), not “No matching export found”; autofocus, the refusal first.
> chips: PROPOSED · Focus: the field
Wombat
Sign in
## Verify portfolio export
Check whether a PDF was produced by Wombat, without signing in.
Content hash (SHA-256) *required
[value: aba6ce7943c2]
This is not a SHA-256 hash: it has 12 characters, and a SHA-256 hash has 64 hexadecimal digits (0 to 9, a to f). The 12 in a file's name are only its start.
The file's SHA-256: 64 hexadecimal digits, computed on your computer from the PDF you were sent. Upper or lower case, and spaces or line breaks in it, are accepted.
- Windows: certutil -hashfile <file> SHA256, or in PowerShell Get-FileHash <file>
- macOS: shasum -a 256 <file>
- Linux: sha256sum <file>
Verify

######## BOARD R3-V07-1280.dc.html — R3-V07 · Verify · 5.23 the second file · 1280
> Generated once, at 22:56 SAST; the earlier file still verifies (R3-V02).
> chips: PROPOSED · Focus: the answer
Wombat
Sign in
## Verify portfolio export
Check whether a PDF was produced by Wombat, without signing in.
Export verified. Wombat generated this PDF: its content is byte for byte an export Wombat recorded.
First generated
2026-10-26 22:56 SAST
Exported
Once
Trainee
2b5abc2f-9e37-4714-bb7a-8e48fa11f699
First exported by
2b5abc2f-9e37-4714-bb7a-8e48fa11f699
Filter
2023-01-15 to 2026-10-26
File
portfolio-344dd52bdded.pdf
People are shown by their Wombat user ids, never by name. The institution that issued the portfolio can say whose they are.
Content hash (SHA-256) *required
[value: 344dd52bddedd1f9e93cd1ef9217ca19542952ce6e8bac8739b84987ec5911fd]
The file's SHA-256: 64 hexadecimal digits, computed on your computer from the PDF you were sent. Upper or lower case, and spaces or line breaks in it, are accepted.
- Windows: certutil -hashfile <file> SHA256, or in PowerShell Get-FileHash <file>
- macOS: shasum -a 256 <file>
- Linux: sha256sum <file>
Verify

######## BOARD R3-V07-390.dc.html — R3-V07 · Verify · 5.23 the second file (at 390, A.7.12) · 390
> Generated once, at 22:56 SAST; the earlier file still verifies (R3-V02).
> chips: PROPOSED · Focus: the answer
Wombat
Sign in
## Verify portfolio export
Check whether a PDF was produced by Wombat, without signing in.
Export verified. Wombat generated this PDF: its content is byte for byte an export Wombat recorded.
First generated
2026-10-26 22:56 SAST
Exported
Once
Trainee
2b5abc2f-9e37-4714-bb7a-8e48fa11f699
First exported by
2b5abc2f-9e37-4714-bb7a-8e48fa11f699
Filter
2023-01-15 to 2026-10-26
File
portfolio-344dd52bdded.pdf
People are shown by their Wombat user ids, never by name. The institution that issued the portfolio can say whose they are.
Content hash (SHA-256) *required
[value: 344dd52bddedd1f9e93cd1ef9217ca19542952ce6e8bac8739b84987ec5911fd]
The file's SHA-256: 64 hexadecimal digits, computed on your computer from the PDF you were sent. Upper or lower case, and spaces or line breaks in it, are accepted.
- Windows: certutil -hashfile <file> SHA256, or in PowerShell Get-FileHash <file>
- macOS: shasum -a 256 <file>
- Linux: sha256sum <file>
Verify

######## BOARD R3-V08-1280.dc.html — R3-V08 · Verify · the check failed · 1280
> Today's words, “Please” dropped; the value kept.
> chips: PROPOSED · Focus: the answer
Wombat
Sign in
## Verify portfolio export
Check whether a PDF was produced by Wombat, without signing in.
The export could not be checked just now. Try again in a few minutes.
Content hash (SHA-256) *required
[value: aba6ce7943c20681a22f7bbfd89a51b9e19ff005892e318806c28dd215a6c995]
The file's SHA-256: 64 hexadecimal digits, computed on your computer from the PDF you were sent. Upper or lower case, and spaces or line breaks in it, are accepted.
- Windows: certutil -hashfile <file> SHA256, or in PowerShell Get-FileHash <file>
- macOS: shasum -a 256 <file>
- Linux: sha256sum <file>
Verify

######## BOARD R3-V08-390.dc.html — R3-V08 · Verify · the check failed · 390
> Today's words, “Please” dropped; the value kept.
> chips: PROPOSED · Focus: the answer
Wombat
Sign in
## Verify portfolio export
Check whether a PDF was produced by Wombat, without signing in.
The export could not be checked just now. Try again in a few minutes.
Content hash (SHA-256) *required
[value: aba6ce7943c20681a22f7bbfd89a51b9e19ff005892e318806c28dd215a6c995]
The file's SHA-256: 64 hexadecimal digits, computed on your computer from the PDF you were sent. Upper or lower case, and spaces or line breaks in it, are accepted.
- Windows: certutil -hashfile <file> SHA256, or in PowerShell Get-FileHash <file>
- macOS: shasum -a 256 <file>
- Linux: sha256sum <file>
Verify

######## BOARD R3-V09-1280.dc.html — R3-V09 · Verify · the hash pasted with spaces and a line break · 1280
> As certutil's older form prints it, byte by byte. The browser drops the line break, or makes it a space, as it is pasted; the round trip shows the rest as typed; the query strips all whitespace and verifies.
> chips: PROPOSED · Focus: the answer
Wombat
Sign in
## Verify portfolio export
Check whether a PDF was produced by Wombat, without signing in.
Export verified. Wombat generated this PDF: its content is byte for byte an export Wombat recorded.
First generated
2026-10-26 22:51 SAST
Exported
3 times, the last on 2026-10-26 22:52 SAST
Trainee
2b5abc2f-9e37-4714-bb7a-8e48fa11f699
First exported by
2b5abc2f-9e37-4714-bb7a-8e48fa11f699
Filter
2023-01-15 to 2026-10-26
File
portfolio-aba6ce7943c2.pdf
People are shown by their Wombat user ids, never by name. The institution that issued the portfolio can say whose they are.
Content hash (SHA-256) *required
[value: ab a6 ce 79 43 c2 06 81 a2 2f 7b bf d8 9a 51 b9 e1 9f f0 05 89 2e 31 88 06 c2 8d d2 15 a6 c9 95]
The file's SHA-256: 64 hexadecimal digits, computed on your computer from the PDF you were sent. Upper or lower case, and spaces or line breaks in it, are accepted.
- Windows: certutil -hashfile <file> SHA256, or in PowerShell Get-FileHash <file>
- macOS: shasum -a 256 <file>
- Linux: sha256sum <file>
Verify

######## BOARD R3-V09-390.dc.html — R3-V09 · Verify · the hash pasted with spaces and a line break · 390
> As certutil's older form prints it, byte by byte. The browser drops the line break, or makes it a space, as it is pasted; the round trip shows the rest as typed; the query strips all whitespace and verifies.
> chips: PROPOSED · Focus: the answer
Wombat
Sign in
## Verify portfolio export
Check whether a PDF was produced by Wombat, without signing in.
Export verified. Wombat generated this PDF: its content is byte for byte an export Wombat recorded.
First generated
2026-10-26 22:51 SAST
Exported
3 times, the last on 2026-10-26 22:52 SAST
Trainee
2b5abc2f-9e37-4714-bb7a-8e48fa11f699
First exported by
2b5abc2f-9e37-4714-bb7a-8e48fa11f699
Filter
2023-01-15 to 2026-10-26
File
portfolio-aba6ce7943c2.pdf
People are shown by their Wombat user ids, never by name. The institution that issued the portfolio can say whose they are.
Content hash (SHA-256) *required
[value: ab a6 ce 79 43 c2 06 81 a2 2f 7b bf d8 9a 51 b9 e1 9f f0 05 89 2e 31 88 06 c2 8d d2 15 a6 c9 95]
The file's SHA-256: 64 hexadecimal digits, computed on your computer from the PDF you were sent. Upper or lower case, and spaces or line breaks in it, are accepted.
- Windows: certutil -hashfile <file> SHA256, or in PowerShell Get-FileHash <file>
- macOS: shasum -a 256 <file>
- Linux: sha256sum <file>
Verify

######## BOARD R3-V10-1280.dc.html — R3-V10 · Verify · arrived from a link (?hash=… with no check=1) · 1280
> The same answer; FocusOnLoad only when check=1, so the focus is left on the page.
> chips: PROPOSED · Focus: left alone, from a link
Wombat
Sign in
## Verify portfolio export
Check whether a PDF was produced by Wombat, without signing in.
Export verified. Wombat generated this PDF: its content is byte for byte an export Wombat recorded.
First generated
2026-10-26 22:51 SAST
Exported
3 times, the last on 2026-10-26 22:52 SAST
Trainee
2b5abc2f-9e37-4714-bb7a-8e48fa11f699
First exported by
2b5abc2f-9e37-4714-bb7a-8e48fa11f699
Filter
2023-01-15 to 2026-10-26
File
portfolio-aba6ce7943c2.pdf
People are shown by their Wombat user ids, never by name. The institution that issued the portfolio can say whose they are.
Content hash (SHA-256) *required
[value: aba6ce7943c20681a22f7bbfd89a51b9e19ff005892e318806c28dd215a6c995]
The file's SHA-256: 64 hexadecimal digits, computed on your computer from the PDF you were sent. Upper or lower case, and spaces or line breaks in it, are accepted.
- Windows: certutil -hashfile <file> SHA256, or in PowerShell Get-FileHash <file>
- macOS: shasum -a 256 <file>
- Linux: sha256sum <file>
Verify

######## BOARD R3-V10-390.dc.html — R3-V10 · Verify · arrived from a link (?hash=… with no check=1) · 390
> The same answer; FocusOnLoad only when check=1, so the focus is left on the page.
> chips: PROPOSED · Focus: left alone, from a link
Wombat
Sign in
## Verify portfolio export
Check whether a PDF was produced by Wombat, without signing in.
Export verified. Wombat generated this PDF: its content is byte for byte an export Wombat recorded.
First generated
2026-10-26 22:51 SAST
Exported
3 times, the last on 2026-10-26 22:52 SAST
Trainee
2b5abc2f-9e37-4714-bb7a-8e48fa11f699
First exported by
2b5abc2f-9e37-4714-bb7a-8e48fa11f699
Filter
2023-01-15 to 2026-10-26
File
portfolio-aba6ce7943c2.pdf
People are shown by their Wombat user ids, never by name. The institution that issued the portfolio can say whose they are.
Content hash (SHA-256) *required
[value: aba6ce7943c20681a22f7bbfd89a51b9e19ff005892e318806c28dd215a6c995]
The file's SHA-256: 64 hexadecimal digits, computed on your computer from the PDF you were sent. Upper or lower case, and spaces or line breaks in it, are accepted.
- Windows: certutil -hashfile <file> SHA256, or in PowerShell Get-FileHash <file>
- macOS: shasum -a 256 <file>
- Linux: sha256sum <file>
Verify

######## BOARD R3-P01-1280.dc.html — R3-P01 · Lerato Molefe's programme · active (5.16, before) · 1280
> Titled by the programme (M3); the built subtitle (S7); Status first, Active a plain word; Profile details with Save profile; then End this programme: the last committee decision (DATA), the day on today, and the two named ends, each trigger described by its note (N13). Back to trainees gone.
> chips: PROPOSED
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
- Lerato Molefe's programme
## Lerato Molefe's programme
Update curriculum and completion details for this trainee.
Export Lerato Molefe's portfolio
[aria-label: Trainee summary]
Status
Active
Email
molefe@kgk.wombat.local
### Profile details
Curriculum *required
Paediatric EPA Curriculum (11.1)
Programme start date *required
[value: 2023-01-15]
Expected completion date
[value: 2027-01-14]
Leave this empty to derive it from the curriculum window.
Cancel
Save profile
### End this programme
The last committee decision: Graduate (programme complete), Pre-graduation review, ratified 2026-10-26.
Last day in the programme *required
[value: 2026-10-26]
The graduation day, or the last day of a registrar who leaves without completing. It cannot be after today, and it cannot be changed afterwards. The period it falls in holds no target unless the day is in that period's last month, and encounters observed after it count towards nothing in this programme, even those already counted.
- A graduation: archives the profile, removes the Trainee role and sends Lerato Molefe a graduation email.
Mark complete…
- A withdrawal: archives the profile and removes the Trainee role. No email is sent.
Deactivate…

######## BOARD R3-P01-768.dc.html — R3-P01 · Lerato Molefe's programme · active (5.16, before) · 768
> Titled by the programme (M3); the built subtitle (S7); Status first, Active a plain word; Profile details with Save profile; then End this programme: the last committee decision (DATA), the day on today, and the two named ends, each trigger described by its note (N13). Back to trainees gone.
> chips: PROPOSED
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
- Lerato Molefe's programme
## Lerato Molefe's programme
Update curriculum and completion details for this trainee.
Export Lerato Molefe's portfolio
[aria-label: Trainee summary]
Status
Active
Email
molefe@kgk.wombat.local
### Profile details
Curriculum *required
Paediatric EPA Curriculum (11.1)
Programme start date *required
[value: 2023-01-15]
Expected completion date
[value: 2027-01-14]
Leave this empty to derive it from the curriculum window.
Cancel
Save profile
### End this programme
The last committee decision: Graduate (programme complete), Pre-graduation review, ratified 2026-10-26.
Last day in the programme *required
[value: 2026-10-26]
The graduation day, or the last day of a registrar who leaves without completing. It cannot be after today, and it cannot be changed afterwards. The period it falls in holds no target unless the day is in that period's last month, and encounters observed after it count towards nothing in this programme, even those already counted.
- A graduation: archives the profile, removes the Trainee role and sends Lerato Molefe a graduation email.
Mark complete…
- A withdrawal: archives the profile and removes the Trainee role. No email is sent.
Deactivate…

######## BOARD R3-P01-390.dc.html — R3-P01 · Lerato Molefe's programme · active (5.16, before) · 390
> Titled by the programme (M3); the built subtitle (S7); Status first, Active a plain word; Profile details with Save profile; then End this programme: the last committee decision (DATA), the day on today, and the two named ends, each trigger described by its note (N13). Back to trainees gone.
> chips: PROPOSED
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
- Lerato Molefe's programme
## Lerato Molefe's programme
Update curriculum and completion details for this trainee.
Export Lerato Molefe's portfolio
[aria-label: Trainee summary]
Status
Active
Email
molefe@kgk.wombat.local
### Profile details
Curriculum *required
Paediatric EPA Curriculum (11.1)
Programme start date *required
[value: 2023-01-15]
Expected completion date
[value: 2027-01-14]
Leave this empty to derive it from the curriculum window.
Cancel
Save profile
### End this programme
The last committee decision: Graduate (programme complete), Pre-graduation review, ratified 2026-10-26.
Last day in the programme *required
[value: 2026-10-26]
The graduation day, or the last day of a registrar who leaves without completing. It cannot be after today, and it cannot be changed afterwards. The period it falls in holds no target unless the day is in that period's last month, and encounters observed after it count towards nothing in this programme, even those already counted.
- A graduation: archives the profile, removes the Trainee role and sends Lerato Molefe a graduation email.
Mark complete…
- A withdrawal: archives the profile and removes the Trainee role. No email is sent.
Deactivate…

######## BOARD R3-P02-1280.dc.html — R3-P02 · The story's day, Mark complete… pressed (5.16) · 1280
> A refusal found before any dialog stays beside its field with the focus in it, role="alert" so it is read at once (S19): no dialog, the typed day kept.
> chips: PROPOSED · Focus: the last day
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
- Lerato Molefe's programme
## Lerato Molefe's programme
Update curriculum and completion details for this trainee.
Export Lerato Molefe's portfolio
[aria-label: Trainee summary]
Status
Active
Email
molefe@kgk.wombat.local
### Profile details
Curriculum *required
Paediatric EPA Curriculum (11.1)
Programme start date *required
[value: 2023-01-15]
Expected completion date
[value: 2027-01-14]
Leave this empty to derive it from the curriculum window.
Cancel
Save profile
### End this programme
The last committee decision: Graduate (programme complete), Pre-graduation review, ratified 2026-10-26.
Last day in the programme *required
[value: 2027-01-14]
The last day cannot be after today (2026-10-26).
The graduation day, or the last day of a registrar who leaves without completing. It cannot be after today, and it cannot be changed afterwards. The period it falls in holds no target unless the day is in that period's last month, and encounters observed after it count towards nothing in this programme, even those already counted.
- A graduation: archives the profile, removes the Trainee role and sends Lerato Molefe a graduation email.
Mark complete…
- A withdrawal: archives the profile and removes the Trainee role. No email is sent.
Deactivate…

######## BOARD R3-P02-390.dc.html — R3-P02 · The story's day, Mark complete… pressed (5.16) · 390
> A refusal found before any dialog stays beside its field with the focus in it, role="alert" so it is read at once (S19): no dialog, the typed day kept.
> chips: PROPOSED · Focus: the last day
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
- Lerato Molefe's programme
## Lerato Molefe's programme
Update curriculum and completion details for this trainee.
Export Lerato Molefe's portfolio
[aria-label: Trainee summary]
Status
Active
Email
molefe@kgk.wombat.local
### Profile details
Curriculum *required
Paediatric EPA Curriculum (11.1)
Programme start date *required
[value: 2023-01-15]
Expected completion date
[value: 2027-01-14]
Leave this empty to derive it from the curriculum window.
Cancel
Save profile
### End this programme
The last committee decision: Graduate (programme complete), Pre-graduation review, ratified 2026-10-26.
Last day in the programme *required
[value: 2027-01-14]
The last day cannot be after today (2026-10-26).
The graduation day, or the last day of a registrar who leaves without completing. It cannot be after today, and it cannot be changed afterwards. The period it falls in holds no target unless the day is in that period's last month, and encounters observed after it count towards nothing in this programme, even those already counted.
- A graduation: archives the profile, removes the Trainee role and sends Lerato Molefe a graduation email.
Mark complete…
- A withdrawal: archives the profile and removes the Trainee role. No email is sent.
Deactivate…

######## BOARD R3-P03-1280.dc.html — R3-P03 · Mark complete's dialog (5.17) · 1280
> Named for Lerato Molefe and the day; sent to molefe@kgk.wombat.local (NEW, read from TraineeProfileDto.Email, S12); the forecast count is 0, so no count clause; opens on Cancel; Mark complete a primary (Q24).
> chips: Focus: Cancel, on opening
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
- Lerato Molefe's programme
## Lerato Molefe's programme
Update curriculum and completion details for this trainee.
Export Lerato Molefe's portfolio
[aria-label: Trainee summary]
Status
Active
Email
molefe@kgk.wombat.local
### Profile details
Curriculum *required
Paediatric EPA Curriculum (11.1)
Programme start date *required
[value: 2023-01-15]
Expected completion date
[value: 2027-01-14]
Leave this empty to derive it from the curriculum window.
Cancel
Save profile
### End this programme
The last committee decision: Graduate (programme complete), Pre-graduation review, ratified 2026-10-26.
Last day in the programme *required
[value: 2026-10-26]
The graduation day, or the last day of a registrar who leaves without completing. It cannot be after today, and it cannot be changed afterwards. The period it falls in holds no target unless the day is in that period's last month, and encounters observed after it count towards nothing in this programme, even those already counted.
- A graduation: archives the profile, removes the Trainee role and sends Lerato Molefe a graduation email.
Mark complete…
- A withdrawal: archives the profile and removes the Trainee role. No email is sent.
Deactivate…
### Mark Lerato Molefe's programme complete?
Record 2026-10-26 as Lerato Molefe's graduation day? The profile is archived, the Trainee role is removed and a graduation email is sent to molefe@kgk.wombat.local.
Encounters observed after that day will count towards nothing in this programme, including any already counted. Lerato Molefe keeps the record, read-only, and can still export the portfolio.
The day cannot be changed afterwards, and this cannot be undone.
Cancel
Mark complete

######## BOARD R3-P03-390.dc.html — R3-P03 · Mark complete's dialog (5.17) · 390
> Named for Lerato Molefe and the day; sent to molefe@kgk.wombat.local (NEW, read from TraineeProfileDto.Email, S12); the forecast count is 0, so no count clause; opens on Cancel; Mark complete a primary (Q24).
> chips: Focus: Cancel, on opening
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
- Lerato Molefe's programme
## Lerato Molefe's programme
Update curriculum and completion details for this trainee.
Export Lerato Molefe's portfolio
[aria-label: Trainee summary]
Status
Active
Email
molefe@kgk.wombat.local
### Profile details
Curriculum *required
Paediatric EPA Curriculum (11.1)
Programme start date *required
[value: 2023-01-15]
Expected completion date
[value: 2027-01-14]
Leave this empty to derive it from the curriculum window.
Cancel
Save profile
### End this programme
The last committee decision: Graduate (programme complete), Pre-graduation review, ratified 2026-10-26.
Last day in the programme *required
[value: 2026-10-26]
The graduation day, or the last day of a registrar who leaves without completing. It cannot be after today, and it cannot be changed afterwards. The period it falls in holds no target unless the day is in that period's last month, and encounters observed after it count towards nothing in this programme, even those already counted.
- A graduation: archives the profile, removes the Trainee role and sends Lerato Molefe a graduation email.
Mark complete…
- A withdrawal: archives the profile and removes the Trainee role. No email is sent.
Deactivate…
### Mark Lerato Molefe's programme complete?
Record 2026-10-26 as Lerato Molefe's graduation day? The profile is archived, the Trainee role is removed and a graduation email is sent to molefe@kgk.wombat.local.
Encounters observed after that day will count towards nothing in this programme, including any already counted. Lerato Molefe keeps the record, read-only, and can still export the portfolio.
The day cannot be changed afterwards, and this cannot be undone.
Cancel
Mark complete

######## BOARD R3-P04-1280.dc.html — R3-P04 · Mark complete's dialog cancelled · 1280
> Closed; nothing changed; the focus back on Mark complete….
> chips: Focus: Mark complete…
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
- Lerato Molefe's programme
## Lerato Molefe's programme
Update curriculum and completion details for this trainee.
Export Lerato Molefe's portfolio
[aria-label: Trainee summary]
Status
Active
Email
molefe@kgk.wombat.local
### Profile details
Curriculum *required
Paediatric EPA Curriculum (11.1)
Programme start date *required
[value: 2023-01-15]
Expected completion date
[value: 2027-01-14]
Leave this empty to derive it from the curriculum window.
Cancel
Save profile
### End this programme
The last committee decision: Graduate (programme complete), Pre-graduation review, ratified 2026-10-26.
Last day in the programme *required
[value: 2026-10-26]
The graduation day, or the last day of a registrar who leaves without completing. It cannot be after today, and it cannot be changed afterwards. The period it falls in holds no target unless the day is in that period's last month, and encounters observed after it count towards nothing in this programme, even those already counted.
- A graduation: archives the profile, removes the Trainee role and sends Lerato Molefe a graduation email.
Mark complete…
- A withdrawal: archives the profile and removes the Trainee role. No email is sent.
Deactivate…

######## BOARD R3-P04-390.dc.html — R3-P04 · Mark complete's dialog cancelled · 390
> Closed; nothing changed; the focus back on Mark complete….
> chips: Focus: Mark complete…
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
- Lerato Molefe's programme
## Lerato Molefe's programme
Update curriculum and completion details for this trainee.
Export Lerato Molefe's portfolio
[aria-label: Trainee summary]
Status
Active
Email
molefe@kgk.wombat.local
### Profile details
Curriculum *required
Paediatric EPA Curriculum (11.1)
Programme start date *required
[value: 2023-01-15]
Expected completion date
[value: 2027-01-14]
Leave this empty to derive it from the curriculum window.
Cancel
Save profile
### End this programme
The last committee decision: Graduate (programme complete), Pre-graduation review, ratified 2026-10-26.
Last day in the programme *required
[value: 2026-10-26]
The graduation day, or the last day of a registrar who leaves without completing. It cannot be after today, and it cannot be changed afterwards. The period it falls in holds no target unless the day is in that period's last month, and encounters observed after it count towards nothing in this programme, even those already counted.
- A graduation: archives the profile, removes the Trainee role and sends Lerato Molefe a graduation email.
Mark complete…
- A withdrawal: archives the profile and removes the Trainee role. No email is sent.
Deactivate…

######## BOARD R3-P05-1280.dc.html — R3-P05 · Completed: the ended record (5.17) · 1280
> The result built from the command's count, 0, so no count sentence (M2); focused. One Status row: the badge “Completed” and the day (N6). The account keeps its STAR clause (S5).
> chips: PROPOSED · Focus: the result
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
- Lerato Molefe's programme
## Lerato Molefe's programme
The record of a programme that has ended.
Export Lerato Molefe's portfolio
Trainee marked complete. The Trainee role has been removed and a graduation email sent.
[aria-label: Trainee summary]
Status
Completed 2026-10-26
Email
molefe@kgk.wombat.local
### Profile details
This programme has ended, so its record is archived and cannot be changed.
Curriculum
Paediatric EPA Curriculum (11.1)
Programme start date
2023-01-15
Expected completion date
2027-01-14
### The account
Lerato Molefe holds no role now, and can still sign in to read the record, export the portfolio and download the STAR certificates.
Open Lerato Molefe's user page

######## BOARD R3-P05-768.dc.html — R3-P05 · Completed: the ended record (5.17) · 768
> The result built from the command's count, 0, so no count sentence (M2); focused. One Status row: the badge “Completed” and the day (N6). The account keeps its STAR clause (S5).
> chips: PROPOSED · Focus: the result
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
- Lerato Molefe's programme
## Lerato Molefe's programme
The record of a programme that has ended.
Export Lerato Molefe's portfolio
Trainee marked complete. The Trainee role has been removed and a graduation email sent.
[aria-label: Trainee summary]
Status
Completed 2026-10-26
Email
molefe@kgk.wombat.local
### Profile details
This programme has ended, so its record is archived and cannot be changed.
Curriculum
Paediatric EPA Curriculum (11.1)
Programme start date
2023-01-15
Expected completion date
2027-01-14
### The account
Lerato Molefe holds no role now, and can still sign in to read the record, export the portfolio and download the STAR certificates.
Open Lerato Molefe's user page

######## BOARD R3-P05-390.dc.html — R3-P05 · Completed: the ended record (5.17) · 390
> The result built from the command's count, 0, so no count sentence (M2); focused. One Status row: the badge “Completed” and the day (N6). The account keeps its STAR clause (S5).
> chips: PROPOSED · Focus: the result
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
- Lerato Molefe's programme
## Lerato Molefe's programme
The record of a programme that has ended.
Export Lerato Molefe's portfolio
Trainee marked complete. The Trainee role has been removed and a graduation email sent.
[aria-label: Trainee summary]
Status
Completed 2026-10-26
Email
molefe@kgk.wombat.local
### Profile details
This programme has ended, so its record is archived and cannot be changed.
Curriculum
Paediatric EPA Curriculum (11.1)
Programme start date
2023-01-15
Expected completion date
2027-01-14
### The account
Lerato Molefe holds no role now, and can still sign in to read the record, export the portfolio and download the STAR certificates.
Open Lerato Molefe's user page

######## BOARD R3-P06-1280.dc.html — R3-P06 · A failed end · 1280
> Nothing changed; the day kept; the result in the section, focused.
> chips: Focus: the result
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
- Lerato Molefe's programme
## Lerato Molefe's programme
Update curriculum and completion details for this trainee.
Export Lerato Molefe's portfolio
[aria-label: Trainee summary]
Status
Active
Email
molefe@kgk.wombat.local
### Profile details
Curriculum *required
Paediatric EPA Curriculum (11.1)
Programme start date *required
[value: 2023-01-15]
Expected completion date
[value: 2027-01-14]
Leave this empty to derive it from the curriculum window.
Cancel
Save profile
### End this programme
The programme could not be ended just now. Nothing has changed. Try again, or come back in a few minutes.
The last committee decision: Graduate (programme complete), Pre-graduation review, ratified 2026-10-26.
Last day in the programme *required
[value: 2026-10-26]
The graduation day, or the last day of a registrar who leaves without completing. It cannot be after today, and it cannot be changed afterwards. The period it falls in holds no target unless the day is in that period's last month, and encounters observed after it count towards nothing in this programme, even those already counted.
- A graduation: archives the profile, removes the Trainee role and sends Lerato Molefe a graduation email.
Mark complete…
- A withdrawal: archives the profile and removes the Trainee role. No email is sent.
Deactivate…

######## BOARD R3-P06-390.dc.html — R3-P06 · A failed end · 390
> Nothing changed; the day kept; the result in the section, focused.
> chips: Focus: the result
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
- Lerato Molefe's programme
## Lerato Molefe's programme
Update curriculum and completion details for this trainee.
Export Lerato Molefe's portfolio
[aria-label: Trainee summary]
Status
Active
Email
molefe@kgk.wombat.local
### Profile details
Curriculum *required
Paediatric EPA Curriculum (11.1)
Programme start date *required
[value: 2023-01-15]
Expected completion date
[value: 2027-01-14]
Leave this empty to derive it from the curriculum window.
Cancel
Save profile
### End this programme
The programme could not be ended just now. Nothing has changed. Try again, or come back in a few minutes.
The last committee decision: Graduate (programme complete), Pre-graduation review, ratified 2026-10-26.
Last day in the programme *required
[value: 2026-10-26]
The graduation day, or the last day of a registrar who leaves without completing. It cannot be after today, and it cannot be changed afterwards. The period it falls in holds no target unless the day is in that period's last month, and encounters observed after it count towards nothing in this programme, even those already counted.
- A graduation: archives the profile, removes the Trainee role and sends Lerato Molefe a graduation email.
Mark complete…
- A withdrawal: archives the profile and removes the Trainee role. No email is sent.
Deactivate…

######## BOARD R3-P07-1280.dc.html — R3-P07 · The command's own refusal at confirm · Illustration · 1280
> The command keeps its own check behind the press-time one; when it refuses at confirm, the dialog closes and the same words show in the same place, under the field, role="alert", focused.
> chips: Illustration · Focus: the last day
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
- Lerato Molefe's programme
## Lerato Molefe's programme
Update curriculum and completion details for this trainee.
Export Lerato Molefe's portfolio
[aria-label: Trainee summary]
Status
Active
Email
molefe@kgk.wombat.local
### Profile details
Curriculum *required
Paediatric EPA Curriculum (11.1)
Programme start date *required
[value: 2023-01-15]
Expected completion date
[value: 2027-01-14]
Leave this empty to derive it from the curriculum window.
Cancel
Save profile
### End this programme
The last committee decision: Graduate (programme complete), Pre-graduation review, ratified 2026-10-26.
Last day in the programme *required
[value: 2027-01-14]
The last day cannot be after today (2026-10-26).
The graduation day, or the last day of a registrar who leaves without completing. It cannot be after today, and it cannot be changed afterwards. The period it falls in holds no target unless the day is in that period's last month, and encounters observed after it count towards nothing in this programme, even those already counted.
- A graduation: archives the profile, removes the Trainee role and sends Lerato Molefe a graduation email.
Mark complete…
- A withdrawal: archives the profile and removes the Trainee role. No email is sent.
Deactivate…

######## BOARD R3-P07-390.dc.html — R3-P07 · The command's own refusal at confirm · Illustration · 390
> The command keeps its own check behind the press-time one; when it refuses at confirm, the dialog closes and the same words show in the same place, under the field, role="alert", focused.
> chips: Illustration · Focus: the last day
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
- Lerato Molefe's programme
## Lerato Molefe's programme
Update curriculum and completion details for this trainee.
Export Lerato Molefe's portfolio
[aria-label: Trainee summary]
Status
Active
Email
molefe@kgk.wombat.local
### Profile details
Curriculum *required
Paediatric EPA Curriculum (11.1)
Programme start date *required
[value: 2023-01-15]
Expected completion date
[value: 2027-01-14]
Leave this empty to derive it from the curriculum window.
Cancel
Save profile
### End this programme
The last committee decision: Graduate (programme complete), Pre-graduation review, ratified 2026-10-26.
Last day in the programme *required
[value: 2027-01-14]
The last day cannot be after today (2026-10-26).
The graduation day, or the last day of a registrar who leaves without completing. It cannot be after today, and it cannot be changed afterwards. The period it falls in holds no target unless the day is in that period's last month, and encounters observed after it count towards nothing in this programme, even those already counted.
- A graduation: archives the profile, removes the Trainee role and sends Lerato Molefe a graduation email.
Mark complete…
- A withdrawal: archives the profile and removes the Trainee role. No email is sent.
Deactivate…

######## BOARD R3-P08-1280.dc.html — R3-P08 · Pieter du Plessis's programme · active, 2026-10-24 typed (5.27) · 1280
> M7's decision line.
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
- Pieter du Plessis's programme
## Pieter du Plessis's programme
Update curriculum and completion details for this trainee.
Export Pieter du Plessis's portfolio
[aria-label: Trainee summary]
Status
Active
Email
duplessis@kgk.wombat.local
### Profile details
Curriculum *required
Paediatric EPA Curriculum (11.1)
Programme start date *required
[value: 2025-01-15]
Expected completion date
[value: 2029-01-14]
Leave this empty to derive it from the curriculum window.
Cancel
Save profile
### End this programme
The last committee decision: Satisfactory with Observations, Annual progression review, ratified 2026-10-26.
Last day in the programme *required
[value: 2026-10-24]
The graduation day, or the last day of a registrar who leaves without completing. It cannot be after today, and it cannot be changed afterwards. The period it falls in holds no target unless the day is in that period's last month, and encounters observed after it count towards nothing in this programme, even those already counted.
- A graduation: archives the profile, removes the Trainee role and sends Pieter du Plessis a graduation email.
Mark complete…
- A withdrawal: archives the profile and removes the Trainee role. No email is sent.
Deactivate…

######## BOARD R3-P08-390.dc.html — R3-P08 · Pieter du Plessis's programme · active, 2026-10-24 typed (5.27) · 390
> M7's decision line.
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
- Pieter du Plessis's programme
## Pieter du Plessis's programme
Update curriculum and completion details for this trainee.
Export Pieter du Plessis's portfolio
[aria-label: Trainee summary]
Status
Active
Email
duplessis@kgk.wombat.local
### Profile details
Curriculum *required
Paediatric EPA Curriculum (11.1)
Programme start date *required
[value: 2025-01-15]
Expected completion date
[value: 2029-01-14]
Leave this empty to derive it from the curriculum window.
Cancel
Save profile
### End this programme
The last committee decision: Satisfactory with Observations, Annual progression review, ratified 2026-10-26.
Last day in the programme *required
[value: 2026-10-24]
The graduation day, or the last day of a registrar who leaves without completing. It cannot be after today, and it cannot be changed afterwards. The period it falls in holds no target unless the day is in that period's last month, and encounters observed after it count towards nothing in this programme, even those already counted.
- A graduation: archives the profile, removes the Trainee role and sends Pieter du Plessis a graduation email.
Mark complete…
- A withdrawal: archives the profile and removes the Trainee role. No email is sent.
Deactivate…

######## BOARD R3-P09-1280.dc.html — R3-P09 · Deactivate's dialog (5.27) · 1280
> The day and the forecast it reads (DATA): “will stop counting” (NEW, M2); Deactivate a filled danger (Q24); opens on Cancel.
> chips: PROPOSED · Focus: Cancel, on opening
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
- Pieter du Plessis's programme
## Pieter du Plessis's programme
Update curriculum and completion details for this trainee.
Export Pieter du Plessis's portfolio
[aria-label: Trainee summary]
Status
Active
Email
duplessis@kgk.wombat.local
### Profile details
Curriculum *required
Paediatric EPA Curriculum (11.1)
Programme start date *required
[value: 2025-01-15]
Expected completion date
[value: 2029-01-14]
Leave this empty to derive it from the curriculum window.
Cancel
Save profile
### End this programme
The last committee decision: Satisfactory with Observations, Annual progression review, ratified 2026-10-26.
Last day in the programme *required
[value: 2026-10-24]
The graduation day, or the last day of a registrar who leaves without completing. It cannot be after today, and it cannot be changed afterwards. The period it falls in holds no target unless the day is in that period's last month, and encounters observed after it count towards nothing in this programme, even those already counted.
- A graduation: archives the profile, removes the Trainee role and sends Pieter du Plessis a graduation email.
Mark complete…
- A withdrawal: archives the profile and removes the Trainee role. No email is sent.
Deactivate…
### Deactivate Pieter du Plessis's profile?
Deactivate Pieter du Plessis's profile, with 2026-10-24 as their last day in the programme? The Trainee role is removed; they keep their own record, read-only.
Encounters observed after that day will count towards nothing in this programme, including any already counted: 1 activity, observed 2026-10-25, will stop counting.
The day cannot be changed afterwards, and a deactivated profile cannot be made active again. This cannot be undone.
Cancel
Deactivate

######## BOARD R3-P09-390.dc.html — R3-P09 · Deactivate's dialog (5.27) · 390
> The day and the forecast it reads (DATA): “will stop counting” (NEW, M2); Deactivate a filled danger (Q24); opens on Cancel.
> chips: PROPOSED · Focus: Cancel, on opening
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
- Pieter du Plessis's programme
## Pieter du Plessis's programme
Update curriculum and completion details for this trainee.
Export Pieter du Plessis's portfolio
[aria-label: Trainee summary]
Status
Active
Email
duplessis@kgk.wombat.local
### Profile details
Curriculum *required
Paediatric EPA Curriculum (11.1)
Programme start date *required
[value: 2025-01-15]
Expected completion date
[value: 2029-01-14]
Leave this empty to derive it from the curriculum window.
Cancel
Save profile
### End this programme
The last committee decision: Satisfactory with Observations, Annual progression review, ratified 2026-10-26.
Last day in the programme *required
[value: 2026-10-24]
The graduation day, or the last day of a registrar who leaves without completing. It cannot be after today, and it cannot be changed afterwards. The period it falls in holds no target unless the day is in that period's last month, and encounters observed after it count towards nothing in this programme, even those already counted.
- A graduation: archives the profile, removes the Trainee role and sends Pieter du Plessis a graduation email.
Mark complete…
- A withdrawal: archives the profile and removes the Trainee role. No email is sent.
Deactivate…
### Deactivate Pieter du Plessis's profile?
Deactivate Pieter du Plessis's profile, with 2026-10-24 as their last day in the programme? The Trainee role is removed; they keep their own record, read-only.
Encounters observed after that day will count towards nothing in this programme, including any already counted: 1 activity, observed 2026-10-25, will stop counting.
The day cannot be changed afterwards, and a deactivated profile cannot be made active again. This cannot be undone.
Cancel
Deactivate

######## BOARD R3-P10-1280.dc.html — R3-P10 · Deactivate's dialog cancelled · 1280
> Closed; the focus back on Deactivate….
> chips: Focus: Deactivate…
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
- Pieter du Plessis's programme
## Pieter du Plessis's programme
Update curriculum and completion details for this trainee.
Export Pieter du Plessis's portfolio
[aria-label: Trainee summary]
Status
Active
Email
duplessis@kgk.wombat.local
### Profile details
Curriculum *required
Paediatric EPA Curriculum (11.1)
Programme start date *required
[value: 2025-01-15]
Expected completion date
[value: 2029-01-14]
Leave this empty to derive it from the curriculum window.
Cancel
Save profile
### End this programme
The last committee decision: Satisfactory with Observations, Annual progression review, ratified 2026-10-26.
Last day in the programme *required
[value: 2026-10-24]
The graduation day, or the last day of a registrar who leaves without completing. It cannot be after today, and it cannot be changed afterwards. The period it falls in holds no target unless the day is in that period's last month, and encounters observed after it count towards nothing in this programme, even those already counted.
- A graduation: archives the profile, removes the Trainee role and sends Pieter du Plessis a graduation email.
Mark complete…
- A withdrawal: archives the profile and removes the Trainee role. No email is sent.
Deactivate…

######## BOARD R3-P10-390.dc.html — R3-P10 · Deactivate's dialog cancelled · 390
> Closed; the focus back on Deactivate….
> chips: Focus: Deactivate…
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
- Pieter du Plessis's programme
## Pieter du Plessis's programme
Update curriculum and completion details for this trainee.
Export Pieter du Plessis's portfolio
[aria-label: Trainee summary]
Status
Active
Email
duplessis@kgk.wombat.local
### Profile details
Curriculum *required
Paediatric EPA Curriculum (11.1)
Programme start date *required
[value: 2025-01-15]
Expected completion date
[value: 2029-01-14]
Leave this empty to derive it from the curriculum window.
Cancel
Save profile
### End this programme
The last committee decision: Satisfactory with Observations, Annual progression review, ratified 2026-10-26.
Last day in the programme *required
[value: 2026-10-24]
The graduation day, or the last day of a registrar who leaves without completing. It cannot be after today, and it cannot be changed afterwards. The period it falls in holds no target unless the day is in that period's last month, and encounters observed after it count towards nothing in this programme, even those already counted.
- A graduation: archives the profile, removes the Trainee role and sends Pieter du Plessis a graduation email.
Mark complete…
- A withdrawal: archives the profile and removes the Trainee role. No email is sent.
Deactivate…

######## BOARD R3-P11-1280.dc.html — R3-P11 · Withdrawn: the ended record (5.27) · 1280
> The result built from the count the command returned (1), focused; Inactive (.badge-withdrawn) and Left the programme first; The account without a STAR clause.
> chips: PROPOSED · Focus: the result
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
- Pieter du Plessis's programme
## Pieter du Plessis's programme
The record of a programme that has ended.
Export Pieter du Plessis's portfolio
Trainee profile deactivated. The last day in the programme is recorded as 2026-10-24, and the Trainee role has been removed. 1 activity observed after that day no longer counts.
[aria-label: Trainee summary]
Status
Inactive Left the programme: 2026-10-24
Email
duplessis@kgk.wombat.local
### Profile details
This programme has ended, so its record is archived and cannot be changed.
Curriculum
Paediatric EPA Curriculum (11.1)
Programme start date
2025-01-15
Expected completion date
2029-01-14
### The account
Pieter du Plessis holds no role now, and can still sign in to read the record and export the portfolio.
Open Pieter du Plessis's user page

######## BOARD R3-P11-768.dc.html — R3-P11 · Withdrawn: the ended record (5.27) · 768
> The result built from the count the command returned (1), focused; Inactive (.badge-withdrawn) and Left the programme first; The account without a STAR clause.
> chips: PROPOSED · Focus: the result
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
- Pieter du Plessis's programme
## Pieter du Plessis's programme
The record of a programme that has ended.
Export Pieter du Plessis's portfolio
Trainee profile deactivated. The last day in the programme is recorded as 2026-10-24, and the Trainee role has been removed. 1 activity observed after that day no longer counts.
[aria-label: Trainee summary]
Status
Inactive Left the programme: 2026-10-24
Email
duplessis@kgk.wombat.local
### Profile details
This programme has ended, so its record is archived and cannot be changed.
Curriculum
Paediatric EPA Curriculum (11.1)
Programme start date
2025-01-15
Expected completion date
2029-01-14
### The account
Pieter du Plessis holds no role now, and can still sign in to read the record and export the portfolio.
Open Pieter du Plessis's user page

######## BOARD R3-P11-390.dc.html — R3-P11 · Withdrawn: the ended record (5.27) · 390
> The result built from the count the command returned (1), focused; Inactive (.badge-withdrawn) and Left the programme first; The account without a STAR clause.
> chips: PROPOSED · Focus: the result
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
- Pieter du Plessis's programme
## Pieter du Plessis's programme
The record of a programme that has ended.
Export Pieter du Plessis's portfolio
Trainee profile deactivated. The last day in the programme is recorded as 2026-10-24, and the Trainee role has been removed. 1 activity observed after that day no longer counts.
[aria-label: Trainee summary]
Status
Inactive Left the programme: 2026-10-24
Email
duplessis@kgk.wombat.local
### Profile details
This programme has ended, so its record is archived and cannot be changed.
Curriculum
Paediatric EPA Curriculum (11.1)
Programme start date
2025-01-15
Expected completion date
2029-01-14
### The account
Pieter du Plessis holds no role now, and can still sign in to read the record and export the portfolio.
Open Pieter du Plessis's user page

######## BOARD R3-P12-1280.dc.html — R3-P12 · Profile unavailable · an unknown id · 1280
> GetTraineeProfileByIdQuery returns null (S4): one body for both causes; Go to Trainees.
> chips: PROPOSED
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
- Profile unavailable
## Profile unavailable
This trainee profile does not exist, or you cannot open it.
Go to Trainees

######## BOARD R3-P12-390.dc.html — R3-P12 · Profile unavailable · an unknown id · 390
> GetTraineeProfileByIdQuery returns null (S4): one body for both causes; Go to Trainees.
> chips: PROPOSED
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
- Profile unavailable
## Profile unavailable
This trainee profile does not exist, or you cannot open it.
Go to Trainees

######## BOARD R3-P13-1280.dc.html — R3-P13 · Profile unavailable · another institution's profile id · 1280
> The same null, the same page and words: an out-of-scope record is never confirmed.
> chips: PROPOSED
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
- Profile unavailable
## Profile unavailable
This trainee profile does not exist, or you cannot open it.
Go to Trainees

######## BOARD R3-P13-390.dc.html — R3-P13 · Profile unavailable · another institution's profile id · 390
> The same null, the same page and words: an out-of-scope record is never confirmed.
> chips: PROPOSED
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
- Profile unavailable
## Profile unavailable
This trainee profile does not exist, or you cannot open it.
Go to Trainees

######## BOARD R3-P14-1280.dc.html — R3-P14 · Trainee profile · loading · 1280
> h1 “Trainee profile” while no name is known; skeletons; no action.
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
- Trainee profile
## Trainee profile
Loading this trainee profile.

######## BOARD R3-P14-390.dc.html — R3-P14 · Trainee profile · loading · 390
> h1 “Trainee profile” while no name is known; skeletons; no action.
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
- Trainee profile
## Trainee profile
Loading this trainee profile.

######## BOARD R3-P15-1280.dc.html — R3-P15 · Trainee profile · the load error · 1280
> The catch shows fixed words (S4); StatePanel's LoadError and OnRetry; Try again focuses the h1 once loaded.
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
- Trainee profile
## Trainee profile
Could not load this trainee profile. Nothing has changed. Try again, or come back in a few minutes.
Try again

######## BOARD R3-P15-390.dc.html — R3-P15 · Trainee profile · the load error · 390
> The catch shows fixed words (S4); StatePanel's LoadError and OnRetry; Try again focuses the h1 once loaded.
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
- Trainee profile
## Trainee profile
Could not load this trainee profile. Nothing has changed. Try again, or come back in a few minutes.
Try again

######## BOARD R3-P16-1280.dc.html — R3-P16 · The end on the programme's first day · Illustration · 1280
> Allowed: the dialog names that day and its forecast.
> chips: Illustration · Focus: Cancel, on opening
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
- ‹registrar›'s programme
## ‹registrar›'s programme
Update curriculum and completion details for this trainee.
Export ‹registrar›'s portfolio
[aria-label: Trainee summary]
Status
Active
Email
‹address›
### Profile details
Curriculum *required
Paediatric EPA Curriculum (11.1)
Programme start date *required
[value: ‹start date›]
Expected completion date
[value: ‹expected completion›]
Leave this empty to derive it from the curriculum window.
Cancel
Save profile
### End this programme
The last committee decision: ‹the last ratified decision›
Last day in the programme *required
[value: ‹day›]
The graduation day, or the last day of a registrar who leaves without completing. It cannot be after today, and it cannot be changed afterwards. The period it falls in holds no target unless the day is in that period's last month, and encounters observed after it count towards nothing in this programme, even those already counted.
- A graduation: archives the profile, removes the Trainee role and sends ‹registrar› a graduation email.
Mark complete…
- A withdrawal: archives the profile and removes the Trainee role. No email is sent.
Deactivate…
### Deactivate ‹registrar›'s profile?
Deactivate ‹registrar›'s profile, with ‹day› as their last day in the programme? The Trainee role is removed; they keep their own record, read-only.
Encounters observed after that day will count towards nothing in this programme, including any already counted: ‹n› activities, observed after ‹day›, will stop counting.
The day cannot be changed afterwards, and a deactivated profile cannot be made active again. This cannot be undone.
Cancel
Deactivate

######## BOARD R3-P16-390.dc.html — R3-P16 · The end on the programme's first day · Illustration · 390
> Allowed: the dialog names that day and its forecast.
> chips: Illustration · Focus: Cancel, on opening
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
- ‹registrar›'s programme
## ‹registrar›'s programme
Update curriculum and completion details for this trainee.
Export ‹registrar›'s portfolio
[aria-label: Trainee summary]
Status
Active
Email
‹address›
### Profile details
Curriculum *required
Paediatric EPA Curriculum (11.1)
Programme start date *required
[value: ‹start date›]
Expected completion date
[value: ‹expected completion›]
Leave this empty to derive it from the curriculum window.
Cancel
Save profile
### End this programme
The last committee decision: ‹the last ratified decision›
Last day in the programme *required
[value: ‹day›]
The graduation day, or the last day of a registrar who leaves without completing. It cannot be after today, and it cannot be changed afterwards. The period it falls in holds no target unless the day is in that period's last month, and encounters observed after it count towards nothing in this programme, even those already counted.
- A graduation: archives the profile, removes the Trainee role and sends ‹registrar› a graduation email.
Mark complete…
- A withdrawal: archives the profile and removes the Trainee role. No email is sent.
Deactivate…
### Deactivate ‹registrar›'s profile?
Deactivate ‹registrar›'s profile, with ‹day› as their last day in the programme? The Trainee role is removed; they keep their own record, read-only.
Encounters observed after that day will count towards nothing in this programme, including any already counted: ‹n› activities, observed after ‹day›, will stop counting.
The day cannot be changed afterwards, and a deactivated profile cannot be made active again. This cannot be undone.
Cancel
Deactivate

######## BOARD R3-P17-1280.dc.html — R3-P17 · A last day in a period's last month · Illustration · 1280
> 2026-06-30: the dialog says the semester keeps its target (NEW, PROPOSED).
> chips: Illustration · PROPOSED · Focus: Cancel, on opening
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
- ‹registrar›'s programme
## ‹registrar›'s programme
Update curriculum and completion details for this trainee.
Export ‹registrar›'s portfolio
[aria-label: Trainee summary]
Status
Active
Email
‹address›
### Profile details
Curriculum *required
Paediatric EPA Curriculum (11.1)
Programme start date *required
[value: ‹start date›]
Expected completion date
[value: ‹expected completion›]
Leave this empty to derive it from the curriculum window.
Cancel
Save profile
### End this programme
The last committee decision: ‹the last ratified decision›
Last day in the programme *required
[value: 2026-06-30]
The graduation day, or the last day of a registrar who leaves without completing. It cannot be after today, and it cannot be changed afterwards. The period it falls in holds no target unless the day is in that period's last month, and encounters observed after it count towards nothing in this programme, even those already counted.
- A graduation: archives the profile, removes the Trainee role and sends ‹registrar› a graduation email.
Mark complete…
- A withdrawal: archives the profile and removes the Trainee role. No email is sent.
Deactivate…
### Mark ‹registrar›'s programme complete?
Record 2026-06-30 as ‹registrar›'s graduation day? The profile is archived, the Trainee role is removed and a graduation email is sent to ‹address›.
Encounters observed after that day will count towards nothing in this programme, including any already counted: ‹n› activities, observed after 2026-06-30, will stop counting. ‹registrar› keeps the record, read-only, and can still export the portfolio.
2026-06-30 is in the last month of Semester 1, 2026, so that semester keeps its target.
The day cannot be changed afterwards, and this cannot be undone.
Cancel
Mark complete

######## BOARD R3-P17-390.dc.html — R3-P17 · A last day in a period's last month · Illustration · 390
> 2026-06-30: the dialog says the semester keeps its target (NEW, PROPOSED).
> chips: Illustration · PROPOSED · Focus: Cancel, on opening
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
- ‹registrar›'s programme
## ‹registrar›'s programme
Update curriculum and completion details for this trainee.
Export ‹registrar›'s portfolio
[aria-label: Trainee summary]
Status
Active
Email
‹address›
### Profile details
Curriculum *required
Paediatric EPA Curriculum (11.1)
Programme start date *required
[value: ‹start date›]
Expected completion date
[value: ‹expected completion›]
Leave this empty to derive it from the curriculum window.
Cancel
Save profile
### End this programme
The last committee decision: ‹the last ratified decision›
Last day in the programme *required
[value: 2026-06-30]
The graduation day, or the last day of a registrar who leaves without completing. It cannot be after today, and it cannot be changed afterwards. The period it falls in holds no target unless the day is in that period's last month, and encounters observed after it count towards nothing in this programme, even those already counted.
- A graduation: archives the profile, removes the Trainee role and sends ‹registrar› a graduation email.
Mark complete…
- A withdrawal: archives the profile and removes the Trainee role. No email is sent.
Deactivate…
### Mark ‹registrar›'s programme complete?
Record 2026-06-30 as ‹registrar›'s graduation day? The profile is archived, the Trainee role is removed and a graduation email is sent to ‹address›.
Encounters observed after that day will count towards nothing in this programme, including any already counted: ‹n› activities, observed after 2026-06-30, will stop counting. ‹registrar› keeps the record, read-only, and can still export the portfolio.
2026-06-30 is in the last month of Semester 1, 2026, so that semester keeps its target.
The day cannot be changed afterwards, and this cannot be undone.
Cancel
Mark complete

######## BOARD R3-P18-1280.dc.html — R3-P18 · A profile moved before the end · Illustration · 1280
> The Curriculum read-out is built today: an ended profile's CurriculumId is final (S14).
> chips: Illustration · after T312
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
- ‹registrar›'s programme
## ‹registrar›'s programme
The record of a programme that has ended.
Export ‹registrar›'s portfolio
[aria-label: Trainee summary]
Status
Completed ‹day›
Email
‹address›
### Profile details
This programme has ended, so its record is archived and cannot be changed.
Curriculum
‹curriculum›
Programme start date
‹start date›
Expected completion date
‹expected completion›
What T312 changes here: the record's EPA list and targets come from the curriculum the programme ended on.
### The account
‹registrar› holds no role now, and can still sign in to read the record, export the portfolio and download the STAR certificates.
Open ‹registrar›'s user page

######## BOARD R3-P18-390.dc.html — R3-P18 · A profile moved before the end · Illustration · 390
> The Curriculum read-out is built today: an ended profile's CurriculumId is final (S14).
> chips: Illustration · after T312
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
- ‹registrar›'s programme
## ‹registrar›'s programme
The record of a programme that has ended.
Export ‹registrar›'s portfolio
[aria-label: Trainee summary]
Status
Completed ‹day›
Email
‹address›
### Profile details
This programme has ended, so its record is archived and cannot be changed.
Curriculum
‹curriculum›
Programme start date
‹start date›
Expected completion date
‹expected completion›
What T312 changes here: the record's EPA list and targets come from the curriculum the programme ended on.
### The account
‹registrar› holds no role now, and can still sign in to read the record, export the portfolio and download the STAR certificates.
Open ‹registrar›'s user page

######## BOARD R3-P19-1280.dc.html — R3-P19 · A 60-character name · Illustration · 1280
> The h1, the header action and the dialog's title wrap; the address bracketed (N9).
> chips: Illustration
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
- ‹a 60-character name as stored, given names and surname, here›'s programme
## ‹a 60-character name as stored, given names and surname, here›'s programme
Update curriculum and completion details for this trainee.
Export ‹a 60-character name as stored, given names and surname, here›'s portfolio
[aria-label: Trainee summary]
Status
Active
Email
‹address›
### Profile details
Curriculum *required
Paediatric EPA Curriculum (11.1)
Programme start date *required
[value: 2023-01-15]
Expected completion date
[value: 2027-01-14]
Leave this empty to derive it from the curriculum window.
Cancel
Save profile
### End this programme
The last committee decision: Graduate (programme complete), Pre-graduation review, ratified 2026-10-26.
Last day in the programme *required
[value: 2026-10-26]
The graduation day, or the last day of a registrar who leaves without completing. It cannot be after today, and it cannot be changed afterwards. The period it falls in holds no target unless the day is in that period's last month, and encounters observed after it count towards nothing in this programme, even those already counted.
- A graduation: archives the profile, removes the Trainee role and sends ‹a 60-character name as stored, given names and surname, here› a graduation email.
Mark complete…
- A withdrawal: archives the profile and removes the Trainee role. No email is sent.
Deactivate…
### Mark ‹a 60-character name as stored, given names and surname, here›'s programme complete?
Record 2026-10-26 as ‹a 60-character name as stored, given names and surname, here›'s graduation day? The profile is archived, the Trainee role is removed and a graduation email is sent to ‹address›.
Encounters observed after that day will count towards nothing in this programme, including any already counted. ‹a 60-character name as stored, given names and surname, here› keeps the record, read-only, and can still export the portfolio.
The day cannot be changed afterwards, and this cannot be undone.
Cancel
Mark complete

######## BOARD R3-P19-390.dc.html — R3-P19 · A 60-character name · Illustration · 390
> The h1, the header action and the dialog's title wrap; the address bracketed (N9).
> chips: Illustration
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
- ‹a 60-character name as stored, given names and surname, here›'s programme
## ‹a 60-character name as stored, given names and surname, here›'s programme
Update curriculum and completion details for this trainee.
Export ‹a 60-character name as stored, given names and surname, here›'s portfolio
[aria-label: Trainee summary]
Status
Active
Email
‹address›
### Profile details
Curriculum *required
Paediatric EPA Curriculum (11.1)
Programme start date *required
[value: 2023-01-15]
Expected completion date
[value: 2027-01-14]
Leave this empty to derive it from the curriculum window.
Cancel
Save profile
### End this programme
The last committee decision: Graduate (programme complete), Pre-graduation review, ratified 2026-10-26.
Last day in the programme *required
[value: 2026-10-26]
The graduation day, or the last day of a registrar who leaves without completing. It cannot be after today, and it cannot be changed afterwards. The period it falls in holds no target unless the day is in that period's last month, and encounters observed after it count towards nothing in this programme, even those already counted.
- A graduation: archives the profile, removes the Trainee role and sends ‹a 60-character name as stored, given names and surname, here› a graduation email.
Mark complete…
- A withdrawal: archives the profile and removes the Trainee role. No email is sent.
Deactivate…
### Mark ‹a 60-character name as stored, given names and surname, here›'s programme complete?
Record 2026-10-26 as ‹a 60-character name as stored, given names and surname, here›'s graduation day? The profile is archived, the Trainee role is removed and a graduation email is sent to ‹address›.
Encounters observed after that day will count towards nothing in this programme, including any already counted. ‹a 60-character name as stored, given names and surname, here› keeps the record, read-only, and can still export the portfolio.
The day cannot be changed afterwards, and this cannot be undone.
Cancel
Mark complete

######## BOARD R3-P20-1280.dc.html — R3-P20 · Mark complete recorded after the fact: the dialog · Illustration · 1280
> ‹registrar›, last day ‹day›: the forecast clause, as P09 words it (M2).
> chips: Illustration · PROPOSED · Focus: Cancel, on opening
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
- ‹registrar›'s programme
## ‹registrar›'s programme
Update curriculum and completion details for this trainee.
Export ‹registrar›'s portfolio
[aria-label: Trainee summary]
Status
Active
Email
‹address›
### Profile details
Curriculum *required
Paediatric EPA Curriculum (11.1)
Programme start date *required
[value: ‹start date›]
Expected completion date
[value: ‹expected completion›]
Leave this empty to derive it from the curriculum window.
Cancel
Save profile
### End this programme
The last committee decision: ‹the last ratified decision›
Last day in the programme *required
[value: ‹day›]
The graduation day, or the last day of a registrar who leaves without completing. It cannot be after today, and it cannot be changed afterwards. The period it falls in holds no target unless the day is in that period's last month, and encounters observed after it count towards nothing in this programme, even those already counted.
- A graduation: archives the profile, removes the Trainee role and sends ‹registrar› a graduation email.
Mark complete…
- A withdrawal: archives the profile and removes the Trainee role. No email is sent.
Deactivate…
### Mark ‹registrar›'s programme complete?
Record ‹day› as ‹registrar›'s graduation day? The profile is archived, the Trainee role is removed and a graduation email is sent to ‹address›.
Encounters observed after that day will count towards nothing in this programme, including any already counted: ‹n› activities, observed after ‹day›, will stop counting. ‹registrar› keeps the record, read-only, and can still export the portfolio.
The day cannot be changed afterwards, and this cannot be undone.
Cancel
Mark complete

######## BOARD R3-P20-390.dc.html — R3-P20 · Mark complete recorded after the fact: the dialog · Illustration · 390
> ‹registrar›, last day ‹day›: the forecast clause, as P09 words it (M2).
> chips: Illustration · PROPOSED · Focus: Cancel, on opening
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
- ‹registrar›'s programme
## ‹registrar›'s programme
Update curriculum and completion details for this trainee.
Export ‹registrar›'s portfolio
[aria-label: Trainee summary]
Status
Active
Email
‹address›
### Profile details
Curriculum *required
Paediatric EPA Curriculum (11.1)
Programme start date *required
[value: ‹start date›]
Expected completion date
[value: ‹expected completion›]
Leave this empty to derive it from the curriculum window.
Cancel
Save profile
### End this programme
The last committee decision: ‹the last ratified decision›
Last day in the programme *required
[value: ‹day›]
The graduation day, or the last day of a registrar who leaves without completing. It cannot be after today, and it cannot be changed afterwards. The period it falls in holds no target unless the day is in that period's last month, and encounters observed after it count towards nothing in this programme, even those already counted.
- A graduation: archives the profile, removes the Trainee role and sends ‹registrar› a graduation email.
Mark complete…
- A withdrawal: archives the profile and removes the Trainee role. No email is sent.
Deactivate…
### Mark ‹registrar›'s programme complete?
Record ‹day› as ‹registrar›'s graduation day? The profile is archived, the Trainee role is removed and a graduation email is sent to ‹address›.
Encounters observed after that day will count towards nothing in this programme, including any already counted: ‹n› activities, observed after ‹day›, will stop counting. ‹registrar› keeps the record, read-only, and can still export the portfolio.
The day cannot be changed afterwards, and this cannot be undone.
Cancel
Mark complete

######## BOARD R3-P21-1280.dc.html — R3-P21 · Mark complete recorded after the fact: the result · Illustration · 1280
> The result built from the count the command returned (NEW, M2).
> chips: Illustration · PROPOSED · Focus: the result
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
- ‹registrar›'s programme
## ‹registrar›'s programme
The record of a programme that has ended.
Export ‹registrar›'s portfolio
Trainee marked complete. The Trainee role has been removed and a graduation email sent. ‹n› activities observed after that day no longer count.
[aria-label: Trainee summary]
Status
Completed ‹day›
Email
‹address›
### Profile details
This programme has ended, so its record is archived and cannot be changed.
Curriculum
Paediatric EPA Curriculum (11.1)
Programme start date
‹start date›
Expected completion date
‹expected completion›
### The account
‹registrar› holds no role now, and can still sign in to read the record, export the portfolio and download the STAR certificates.
Open ‹registrar›'s user page

######## BOARD R3-P21-390.dc.html — R3-P21 · Mark complete recorded after the fact: the result · Illustration · 390
> The result built from the count the command returned (NEW, M2).
> chips: Illustration · PROPOSED · Focus: the result
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
- ‹registrar›'s programme
## ‹registrar›'s programme
The record of a programme that has ended.
Export ‹registrar›'s portfolio
Trainee marked complete. The Trainee role has been removed and a graduation email sent. ‹n› activities observed after that day no longer count.
[aria-label: Trainee summary]
Status
Completed ‹day›
Email
‹address›
### Profile details
This programme has ended, so its record is archived and cannot be changed.
Curriculum
Paediatric EPA Curriculum (11.1)
Programme start date
‹start date›
Expected completion date
‹expected completion›
### The account
‹registrar› holds no role now, and can still sign in to read the record, export the portfolio and download the STAR certificates.
Open ‹registrar›'s user page

######## BOARD R3-P22-1280.dc.html — R3-P22 · A day before the start, Mark complete… pressed · 1280
> Refused on the press (F11, NEW row), under the field, role="alert", the focus in it; no dialog.
> chips: PROPOSED · Focus: the last day
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
- Lerato Molefe's programme
## Lerato Molefe's programme
Update curriculum and completion details for this trainee.
Export Lerato Molefe's portfolio
[aria-label: Trainee summary]
Status
Active
Email
molefe@kgk.wombat.local
### Profile details
Curriculum *required
Paediatric EPA Curriculum (11.1)
Programme start date *required
[value: 2023-01-15]
Expected completion date
[value: 2027-01-14]
Leave this empty to derive it from the curriculum window.
Cancel
Save profile
### End this programme
The last committee decision: Graduate (programme complete), Pre-graduation review, ratified 2026-10-26.
Last day in the programme *required
[value: ‹a day before 2023-01-15›]
The last day cannot be before the programme start date (2023-01-15).
The graduation day, or the last day of a registrar who leaves without completing. It cannot be after today, and it cannot be changed afterwards. The period it falls in holds no target unless the day is in that period's last month, and encounters observed after it count towards nothing in this programme, even those already counted.
- A graduation: archives the profile, removes the Trainee role and sends Lerato Molefe a graduation email.
Mark complete…
- A withdrawal: archives the profile and removes the Trainee role. No email is sent.
Deactivate…

######## BOARD R3-P22-390.dc.html — R3-P22 · A day before the start, Mark complete… pressed · 390
> Refused on the press (F11, NEW row), under the field, role="alert", the focus in it; no dialog.
> chips: PROPOSED · Focus: the last day
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
- Lerato Molefe's programme
## Lerato Molefe's programme
Update curriculum and completion details for this trainee.
Export Lerato Molefe's portfolio
[aria-label: Trainee summary]
Status
Active
Email
molefe@kgk.wombat.local
### Profile details
Curriculum *required
Paediatric EPA Curriculum (11.1)
Programme start date *required
[value: 2023-01-15]
Expected completion date
[value: 2027-01-14]
Leave this empty to derive it from the curriculum window.
Cancel
Save profile
### End this programme
The last committee decision: Graduate (programme complete), Pre-graduation review, ratified 2026-10-26.
Last day in the programme *required
[value: ‹a day before 2023-01-15›]
The last day cannot be before the programme start date (2023-01-15).
The graduation day, or the last day of a registrar who leaves without completing. It cannot be after today, and it cannot be changed afterwards. The period it falls in holds no target unless the day is in that period's last month, and encounters observed after it count towards nothing in this programme, even those already counted.
- A graduation: archives the profile, removes the Trainee role and sends Lerato Molefe a graduation email.
Mark complete…
- A withdrawal: archives the profile and removes the Trainee role. No email is sent.
Deactivate…

######## BOARD R3-P23-1280.dc.html — R3-P23 · The day emptied, Mark complete… pressed · 1280
> EndDate nullable on the page, so an empty field reaches the press check (S3): “Choose the last day.” (NEW, PROPOSED) under it; no dialog.
> chips: PROPOSED · Focus: the last day
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
- Lerato Molefe's programme
## Lerato Molefe's programme
Update curriculum and completion details for this trainee.
Export Lerato Molefe's portfolio
[aria-label: Trainee summary]
Status
Active
Email
molefe@kgk.wombat.local
### Profile details
Curriculum *required
Paediatric EPA Curriculum (11.1)
Programme start date *required
[value: 2023-01-15]
Expected completion date
[value: 2027-01-14]
Leave this empty to derive it from the curriculum window.
Cancel
Save profile
### End this programme
The last committee decision: Graduate (programme complete), Pre-graduation review, ratified 2026-10-26.
Last day in the programme *required
Choose the last day.
The graduation day, or the last day of a registrar who leaves without completing. It cannot be after today, and it cannot be changed afterwards. The period it falls in holds no target unless the day is in that period's last month, and encounters observed after it count towards nothing in this programme, even those already counted.
- A graduation: archives the profile, removes the Trainee role and sends Lerato Molefe a graduation email.
Mark complete…
- A withdrawal: archives the profile and removes the Trainee role. No email is sent.
Deactivate…

######## BOARD R3-P23-390.dc.html — R3-P23 · The day emptied, Mark complete… pressed · 390
> EndDate nullable on the page, so an empty field reaches the press check (S3): “Choose the last day.” (NEW, PROPOSED) under it; no dialog.
> chips: PROPOSED · Focus: the last day
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
- Lerato Molefe's programme
## Lerato Molefe's programme
Update curriculum and completion details for this trainee.
Export Lerato Molefe's portfolio
[aria-label: Trainee summary]
Status
Active
Email
molefe@kgk.wombat.local
### Profile details
Curriculum *required
Paediatric EPA Curriculum (11.1)
Programme start date *required
[value: 2023-01-15]
Expected completion date
[value: 2027-01-14]
Leave this empty to derive it from the curriculum window.
Cancel
Save profile
### End this programme
The last committee decision: Graduate (programme complete), Pre-graduation review, ratified 2026-10-26.
Last day in the programme *required
Choose the last day.
The graduation day, or the last day of a registrar who leaves without completing. It cannot be after today, and it cannot be changed afterwards. The period it falls in holds no target unless the day is in that period's last month, and encounters observed after it count towards nothing in this programme, even those already counted.
- A graduation: archives the profile, removes the Trainee role and sends Lerato Molefe a graduation email.
Mark complete…
- A withdrawal: archives the profile and removes the Trainee role. No email is sent.
Deactivate…

######## BOARD R3-P24-1280.dc.html — R3-P24 · An ended record whose account holds Assessor now · Illustration · 1280
> The account's variant sentence (F5; NEW, PROPOSED).
> chips: Illustration · PROPOSED
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
- ‹registrar›'s programme
## ‹registrar›'s programme
The record of a programme that has ended.
Export ‹registrar›'s portfolio
[aria-label: Trainee summary]
Status
Completed ‹day›
Email
‹address›
### Profile details
This programme has ended, so its record is archived and cannot be changed.
Curriculum
Paediatric EPA Curriculum (11.1)
Programme start date
‹start date›
Expected completion date
‹expected completion›
### The account
‹registrar› now holds Assessor. They can still read the record and export the portfolio.
Open ‹registrar›'s user page

######## BOARD R3-P24-390.dc.html — R3-P24 · An ended record whose account holds Assessor now · Illustration · 390
> The account's variant sentence (F5; NEW, PROPOSED).
> chips: Illustration · PROPOSED
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
- ‹registrar›'s programme
## ‹registrar›'s programme
The record of a programme that has ended.
Export ‹registrar›'s portfolio
[aria-label: Trainee summary]
Status
Completed ‹day›
Email
‹address›
### Profile details
This programme has ended, so its record is archived and cannot be changed.
Curriculum
Paediatric EPA Curriculum (11.1)
Programme start date
‹start date›
Expected completion date
‹expected completion›
### The account
‹registrar› now holds Assessor. They can still read the record and export the portfolio.
Open ‹registrar›'s user page

######## BOARD R3-P25-1280.dc.html — R3-P25 · An ended record whose account is locked · Illustration · 1280
> The account shows no sentence, only the user page link (F5).
> chips: Illustration · PROPOSED
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
- ‹registrar›'s programme
## ‹registrar›'s programme
The record of a programme that has ended.
Export ‹registrar›'s portfolio
[aria-label: Trainee summary]
Status
Completed ‹day›
Email
‹address›
### Profile details
This programme has ended, so its record is archived and cannot be changed.
Curriculum
Paediatric EPA Curriculum (11.1)
Programme start date
‹start date›
Expected completion date
‹expected completion›
### The account
Open ‹registrar›'s user page

######## BOARD R3-P25-390.dc.html — R3-P25 · An ended record whose account is locked · Illustration · 390
> The account shows no sentence, only the user page link (F5).
> chips: Illustration · PROPOSED
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
- ‹registrar›'s programme
## ‹registrar›'s programme
The record of a programme that has ended.
Export ‹registrar›'s portfolio
[aria-label: Trainee summary]
Status
Completed ‹day›
Email
‹address›
### Profile details
This programme has ended, so its record is archived and cannot be changed.
Curriculum
Paediatric EPA Curriculum (11.1)
Programme start date
‹start date›
Expected completion date
‹expected completion›
### The account
Open ‹registrar›'s user page

######## BOARD R3-C01-1280.dc.html — R3-C01 · Trainees · one ended (5.17) · 1280
> Completed and closed under Current trainees: Name as the row's header, the programme's dates over the training year it ended in (.person-cell, DATA), the outcome, View named for its row. The count word “1 ended” is AdmissionText's (NEW).
> chips: PROPOSED
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
## Trainees
Admit registrars who are waiting, and keep current trainees' programmes.
### Waiting to be admitted
Nobody is waiting to be admitted.
A registrar appears here after registering from a Trainee invitation.
### Current trainees 4 current
Flow 11's table, unchanged: Anele Dlamini, Pieter du Plessis, Nomsa Mahlangu, Sipho Ndlovu, each with Edit.
### Completed and closed 1 ended
Programmes that have ended, the latest end first. An ended record is read-only.
| Name | Curriculum and sub-speciality | Programme | Outcome | Actions |
[aria-label: View Lerato Molefe's record]
| Lerato Molefe | Paediatric EPA Curriculum (11.1) Paediatrics | 2023-01-15 to 2026-10-26 Training year 4 when it ended | Completed 2026-10-26 | View |

######## BOARD R3-C01-390.dc.html — R3-C01 · Trainees · one ended (5.17) · 390
> Completed and closed under Current trainees: Name as the row's header, the programme's dates over the training year it ended in (.person-cell, DATA), the outcome, View named for its row. The count word “1 ended” is AdmissionText's (NEW).
> chips: PROPOSED
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
## Trainees
Admit registrars who are waiting, and keep current trainees' programmes.
### Waiting to be admitted
Nobody is waiting to be admitted.
A registrar appears here after registering from a Trainee invitation.
### Current trainees 4 current
Flow 11's table, unchanged: Anele Dlamini, Pieter du Plessis, Nomsa Mahlangu, Sipho Ndlovu, each with Edit.
### Completed and closed 1 ended
Programmes that have ended, the latest end first. An ended record is read-only.
| Name | Curriculum and sub-speciality | Programme | Outcome | Actions |
[aria-label: View Lerato Molefe's record]
| Lerato Molefe | Paediatric EPA Curriculum (11.1) Paediatrics | 2023-01-15 to 2026-10-26 Training year 4 when it ended | Completed 2026-10-26 | View |

######## BOARD R3-C02-1280.dc.html — R3-C02 · Trainees · two ended (5.27) · 1280
> The latest end first; Withdrawn grey (.badge-withdrawn, F10). Stacked from 900 px (StackWide); View a 44 px block below it.
> chips: PROPOSED
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
## Trainees
Admit registrars who are waiting, and keep current trainees' programmes.
### Waiting to be admitted
Nobody is waiting to be admitted.
A registrar appears here after registering from a Trainee invitation.
### Current trainees 3 current
Flow 11's table, unchanged: Anele Dlamini, Nomsa Mahlangu, Sipho Ndlovu, each with Edit.
### Completed and closed 2 ended
Programmes that have ended, the latest end first. An ended record is read-only.
| Name | Curriculum and sub-speciality | Programme | Outcome | Actions |
[aria-label: View Lerato Molefe's record]
| Lerato Molefe | Paediatric EPA Curriculum (11.1) Paediatrics | 2023-01-15 to 2026-10-26 Training year 4 when it ended | Completed 2026-10-26 | View |
[aria-label: View Pieter du Plessis's record]
| Pieter du Plessis | Paediatric EPA Curriculum (11.1) Paediatrics | 2025-01-15 to 2026-10-24 Training year 2 when it ended | Withdrawn 2026-10-24 | View |

######## BOARD R3-C02-768.dc.html — R3-C02 · Trainees · two ended (5.27) · 768
> The latest end first; Withdrawn grey (.badge-withdrawn, F10). Stacked from 900 px (StackWide); View a 44 px block below it.
> chips: PROPOSED
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
## Trainees
Admit registrars who are waiting, and keep current trainees' programmes.
### Waiting to be admitted
Nobody is waiting to be admitted.
A registrar appears here after registering from a Trainee invitation.
### Current trainees 3 current
Flow 11's table, unchanged: Anele Dlamini, Nomsa Mahlangu, Sipho Ndlovu, each with Edit.
### Completed and closed 2 ended
Programmes that have ended, the latest end first. An ended record is read-only.
| Name | Curriculum and sub-speciality | Programme | Outcome | Actions |
[aria-label: View Lerato Molefe's record]
| Lerato Molefe | Paediatric EPA Curriculum (11.1) Paediatrics | 2023-01-15 to 2026-10-26 Training year 4 when it ended | Completed 2026-10-26 | View |
[aria-label: View Pieter du Plessis's record]
| Pieter du Plessis | Paediatric EPA Curriculum (11.1) Paediatrics | 2025-01-15 to 2026-10-24 Training year 2 when it ended | Withdrawn 2026-10-24 | View |

######## BOARD R3-C02-390.dc.html — R3-C02 · Trainees · two ended (5.27) · 390
> The latest end first; Withdrawn grey (.badge-withdrawn, F10). Stacked from 900 px (StackWide); View a 44 px block below it.
> chips: PROPOSED
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
## Trainees
Admit registrars who are waiting, and keep current trainees' programmes.
### Waiting to be admitted
Nobody is waiting to be admitted.
A registrar appears here after registering from a Trainee invitation.
### Current trainees 3 current
Flow 11's table, unchanged: Anele Dlamini, Nomsa Mahlangu, Sipho Ndlovu, each with Edit.
### Completed and closed 2 ended
Programmes that have ended, the latest end first. An ended record is read-only.
| Name | Curriculum and sub-speciality | Programme | Outcome | Actions |
[aria-label: View Lerato Molefe's record]
| Lerato Molefe | Paediatric EPA Curriculum (11.1) Paediatrics | 2023-01-15 to 2026-10-26 Training year 4 when it ended | Completed 2026-10-26 | View |
[aria-label: View Pieter du Plessis's record]
| Pieter du Plessis | Paediatric EPA Curriculum (11.1) Paediatrics | 2025-01-15 to 2026-10-24 Training year 2 when it ended | Withdrawn 2026-10-24 | View |

######## BOARD R3-C03-1280.dc.html — R3-C03 · Trainees · none ended · 1280
> The section with its empty state (NEW; today the section is not drawn).
> chips: PROPOSED
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
## Trainees
Admit registrars who are waiting, and keep current trainees' programmes.
### Waiting to be admitted
Nobody is waiting to be admitted.
A registrar appears here after registering from a Trainee invitation.
### Current trainees 5 current
Flow 11's table, unchanged: Anele Dlamini, Pieter du Plessis, Nomsa Mahlangu, Lerato Molefe, Sipho Ndlovu, each with Edit.
### Completed and closed
No programme has ended yet.
A trainee is listed here once their programme is marked complete or a withdrawal is recorded, on their programme's page.

######## BOARD R3-C03-390.dc.html — R3-C03 · Trainees · none ended · 390
> The section with its empty state (NEW; today the section is not drawn).
> chips: PROPOSED
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
## Trainees
Admit registrars who are waiting, and keep current trainees' programmes.
### Waiting to be admitted
Nobody is waiting to be admitted.
A registrar appears here after registering from a Trainee invitation.
### Current trainees 5 current
Flow 11's table, unchanged: Anele Dlamini, Pieter du Plessis, Nomsa Mahlangu, Lerato Molefe, Sipho Ndlovu, each with Edit.
### Completed and closed
No programme has ended yet.
A trainee is listed here once their programme is marked complete or a withdrawal is recorded, on their programme's page.

######## BOARD R3-C04-1280.dc.html — R3-C04 · Trainees · forty ended over several intakes · Illustration · 1280
> Page 1: 20 rows, paged in SQL by ListEndedProfilesQuery, and PagerControls, new on this page (its select named “Ended programmes per page”). Each bracketed row's Outcome the longer real word, Withdrawn (S24).
> chips: PROPOSED · Illustration
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
## Trainees
Admit registrars who are waiting, and keep current trainees' programmes.
### Waiting to be admitted
Nobody is waiting to be admitted.
A registrar appears here after registering from a Trainee invitation.
### Current trainees 3 current
Flow 11's table, unchanged: Anele Dlamini, Nomsa Mahlangu, Sipho Ndlovu, each with Edit.
### Completed and closed 40 ended
Programmes that have ended, the latest end first. An ended record is read-only.
| Name | Curriculum and sub-speciality | Programme | Outcome | Actions |
[aria-label: View Lerato Molefe's record]
| Lerato Molefe | Paediatric EPA Curriculum (11.1) Paediatrics | 2023-01-15 to 2026-10-26 Training year 4 when it ended | Completed 2026-10-26 | View |
[aria-label: View Pieter du Plessis's record]
| Pieter du Plessis | Paediatric EPA Curriculum (11.1) Paediatrics | 2025-01-15 to 2026-10-24 Training year 2 when it ended | Withdrawn 2026-10-24 | View |
[aria-label: View ‹further person 3›'s record]
| ‹further person› | ‹curriculum› Paediatrics | ‹start› to ‹day› Training year ‹n› when it ended | Withdrawn ‹day› | View |
[aria-label: View ‹further person 4›'s record]
| ‹further person› | ‹curriculum› Paediatrics | ‹start› to ‹day› Training year ‹n› when it ended | Withdrawn ‹day› | View |
[aria-label: View ‹further person 5›'s record]
| ‹further person› | ‹curriculum› Paediatrics | ‹start› to ‹day› Training year ‹n› when it ended | Withdrawn ‹day› | View |
[aria-label: View ‹further person 6›'s record]
| ‹further person› | ‹curriculum› Paediatrics | ‹start› to ‹day› Training year ‹n› when it ended | Withdrawn ‹day› | View |
[aria-label: View ‹further person 7›'s record]
| ‹further person› | ‹curriculum› Paediatrics | ‹start› to ‹day› Training year ‹n› when it ended | Withdrawn ‹day› | View |
[aria-label: View ‹further person 8›'s record]
| ‹further person› | ‹curriculum› Paediatrics | ‹start› to ‹day› Training year ‹n› when it ended | Withdrawn ‹day› | View |
[aria-label: View ‹further person 9›'s record]
| ‹further person› | ‹curriculum› Paediatrics | ‹start› to ‹day› Training year ‹n› when it ended | Withdrawn ‹day› | View |
[aria-label: View ‹further person 10›'s record]
| ‹further person› | ‹curriculum› Paediatrics | ‹start› to ‹day› Training year ‹n› when it ended | Withdrawn ‹day› | View |
[aria-label: View ‹further person 11›'s record]
| ‹further person› | ‹curriculum› Paediatrics | ‹start› to ‹day› Training year ‹n› when it ended | Withdrawn ‹day› | View |
[aria-label: View ‹further person 12›'s record]
| ‹further person› | ‹curriculum› Paediatrics | ‹start› to ‹day› Training year ‹n› when it ended | Withdrawn ‹day› | View |
[aria-label: View ‹further person 13›'s record]
| ‹further person› | ‹curriculum› Paediatrics | ‹start› to ‹day› Training year ‹n› when it ended | Withdrawn ‹day› | View |
[aria-label: View ‹further person 14›'s record]
| ‹further person› | ‹curriculum› Paediatrics | ‹start› to ‹day› Training year ‹n› when it ended | Withdrawn ‹day› | View |
[aria-label: View ‹further person 15›'s record]
| ‹further person› | ‹curriculum› Paediatrics | ‹start› to ‹day› Training year ‹n› when it ended | Withdrawn ‹day› | View |
[aria-label: View ‹further person 16›'s record]
| ‹further person› | ‹curriculum› Paediatrics | ‹start› to ‹day› Training year ‹n› when it ended | Withdrawn ‹day› | View |
[aria-label: View ‹further person 17›'s record]
| ‹further person› | ‹curriculum› Paediatrics | ‹start› to ‹day› Training year ‹n› when it ended | Withdrawn ‹day› | View |
[aria-label: View ‹further person 18›'s record]
| ‹further person› | ‹curriculum› Paediatrics | ‹start› to ‹day› Training year ‹n› when it ended | Withdrawn ‹day› | View |
[aria-label: View ‹further person 19›'s record]
| ‹further person› | ‹curriculum› Paediatrics | ‹start› to ‹day› Training year ‹n› when it ended | Withdrawn ‹day› | View |
[aria-label: View ‹further person 20›'s record]
| ‹further person› | ‹curriculum› Paediatrics | ‹start› to ‹day› Training year ‹n› when it ended | Withdrawn ‹day› | View |
[aria-label: Completed and closed, pages]
Showing 1–20 of 40
Previous
Next
Ended programmes per page
10
20
50
100

######## BOARD R3-C04-768.dc.html — R3-C04 · Trainees · forty ended over several intakes · Illustration · 768
> Page 1: 20 rows, paged in SQL by ListEndedProfilesQuery, and PagerControls, new on this page (its select named “Ended programmes per page”). Each bracketed row's Outcome the longer real word, Withdrawn (S24).
> chips: PROPOSED · Illustration
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
## Trainees
Admit registrars who are waiting, and keep current trainees' programmes.
### Waiting to be admitted
Nobody is waiting to be admitted.
A registrar appears here after registering from a Trainee invitation.
### Current trainees 3 current
Flow 11's table, unchanged: Anele Dlamini, Nomsa Mahlangu, Sipho Ndlovu, each with Edit.
### Completed and closed 40 ended
Programmes that have ended, the latest end first. An ended record is read-only.
| Name | Curriculum and sub-speciality | Programme | Outcome | Actions |
[aria-label: View Lerato Molefe's record]
| Lerato Molefe | Paediatric EPA Curriculum (11.1) Paediatrics | 2023-01-15 to 2026-10-26 Training year 4 when it ended | Completed 2026-10-26 | View |
[aria-label: View Pieter du Plessis's record]
| Pieter du Plessis | Paediatric EPA Curriculum (11.1) Paediatrics | 2025-01-15 to 2026-10-24 Training year 2 when it ended | Withdrawn 2026-10-24 | View |
[aria-label: View ‹further person 3›'s record]
| ‹further person› | ‹curriculum› Paediatrics | ‹start› to ‹day› Training year ‹n› when it ended | Withdrawn ‹day› | View |
[aria-label: View ‹further person 4›'s record]
| ‹further person› | ‹curriculum› Paediatrics | ‹start› to ‹day› Training year ‹n› when it ended | Withdrawn ‹day› | View |
[aria-label: View ‹further person 5›'s record]
| ‹further person› | ‹curriculum› Paediatrics | ‹start› to ‹day› Training year ‹n› when it ended | Withdrawn ‹day› | View |
[aria-label: View ‹further person 6›'s record]
| ‹further person› | ‹curriculum› Paediatrics | ‹start› to ‹day› Training year ‹n› when it ended | Withdrawn ‹day› | View |
[aria-label: View ‹further person 7›'s record]
| ‹further person› | ‹curriculum› Paediatrics | ‹start› to ‹day› Training year ‹n› when it ended | Withdrawn ‹day› | View |
[aria-label: View ‹further person 8›'s record]
| ‹further person› | ‹curriculum› Paediatrics | ‹start› to ‹day› Training year ‹n› when it ended | Withdrawn ‹day› | View |
[aria-label: View ‹further person 9›'s record]
| ‹further person› | ‹curriculum› Paediatrics | ‹start› to ‹day› Training year ‹n› when it ended | Withdrawn ‹day› | View |
[aria-label: View ‹further person 10›'s record]
| ‹further person› | ‹curriculum› Paediatrics | ‹start› to ‹day› Training year ‹n› when it ended | Withdrawn ‹day› | View |
[aria-label: View ‹further person 11›'s record]
| ‹further person› | ‹curriculum› Paediatrics | ‹start› to ‹day› Training year ‹n› when it ended | Withdrawn ‹day› | View |
[aria-label: View ‹further person 12›'s record]
| ‹further person› | ‹curriculum› Paediatrics | ‹start› to ‹day› Training year ‹n› when it ended | Withdrawn ‹day› | View |
[aria-label: View ‹further person 13›'s record]
| ‹further person› | ‹curriculum› Paediatrics | ‹start› to ‹day› Training year ‹n› when it ended | Withdrawn ‹day› | View |
[aria-label: View ‹further person 14›'s record]
| ‹further person› | ‹curriculum› Paediatrics | ‹start› to ‹day› Training year ‹n› when it ended | Withdrawn ‹day› | View |
[aria-label: View ‹further person 15›'s record]
| ‹further person› | ‹curriculum› Paediatrics | ‹start› to ‹day› Training year ‹n› when it ended | Withdrawn ‹day› | View |
[aria-label: View ‹further person 16›'s record]
| ‹further person› | ‹curriculum› Paediatrics | ‹start› to ‹day› Training year ‹n› when it ended | Withdrawn ‹day› | View |
[aria-label: View ‹further person 17›'s record]
| ‹further person› | ‹curriculum› Paediatrics | ‹start› to ‹day› Training year ‹n› when it ended | Withdrawn ‹day› | View |
[aria-label: View ‹further person 18›'s record]
| ‹further person› | ‹curriculum› Paediatrics | ‹start› to ‹day› Training year ‹n› when it ended | Withdrawn ‹day› | View |
[aria-label: View ‹further person 19›'s record]
| ‹further person› | ‹curriculum› Paediatrics | ‹start› to ‹day› Training year ‹n› when it ended | Withdrawn ‹day› | View |
[aria-label: View ‹further person 20›'s record]
| ‹further person› | ‹curriculum› Paediatrics | ‹start› to ‹day› Training year ‹n› when it ended | Withdrawn ‹day› | View |
[aria-label: Completed and closed, pages]
Showing 1–20 of 40
Previous
Next
Ended programmes per page
10
20
50
100

######## BOARD R3-C04-390.dc.html — R3-C04 · Trainees · forty ended over several intakes · Illustration · 390
> Page 1: 20 rows, paged in SQL by ListEndedProfilesQuery, and PagerControls, new on this page (its select named “Ended programmes per page”). Each bracketed row's Outcome the longer real word, Withdrawn (S24).
> chips: PROPOSED · Illustration
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
## Trainees
Admit registrars who are waiting, and keep current trainees' programmes.
### Waiting to be admitted
Nobody is waiting to be admitted.
A registrar appears here after registering from a Trainee invitation.
### Current trainees 3 current
Flow 11's table, unchanged: Anele Dlamini, Nomsa Mahlangu, Sipho Ndlovu, each with Edit.
### Completed and closed 40 ended
Programmes that have ended, the latest end first. An ended record is read-only.
| Name | Curriculum and sub-speciality | Programme | Outcome | Actions |
[aria-label: View Lerato Molefe's record]
| Lerato Molefe | Paediatric EPA Curriculum (11.1) Paediatrics | 2023-01-15 to 2026-10-26 Training year 4 when it ended | Completed 2026-10-26 | View |
[aria-label: View Pieter du Plessis's record]
| Pieter du Plessis | Paediatric EPA Curriculum (11.1) Paediatrics | 2025-01-15 to 2026-10-24 Training year 2 when it ended | Withdrawn 2026-10-24 | View |
[aria-label: View ‹further person 3›'s record]
| ‹further person› | ‹curriculum› Paediatrics | ‹start› to ‹day› Training year ‹n› when it ended | Withdrawn ‹day› | View |
[aria-label: View ‹further person 4›'s record]
| ‹further person› | ‹curriculum› Paediatrics | ‹start› to ‹day› Training year ‹n› when it ended | Withdrawn ‹day› | View |
[aria-label: View ‹further person 5›'s record]
| ‹further person› | ‹curriculum› Paediatrics | ‹start› to ‹day› Training year ‹n› when it ended | Withdrawn ‹day› | View |
[aria-label: View ‹further person 6›'s record]
| ‹further person› | ‹curriculum› Paediatrics | ‹start› to ‹day› Training year ‹n› when it ended | Withdrawn ‹day› | View |
[aria-label: View ‹further person 7›'s record]
| ‹further person› | ‹curriculum› Paediatrics | ‹start› to ‹day› Training year ‹n› when it ended | Withdrawn ‹day› | View |
[aria-label: View ‹further person 8›'s record]
| ‹further person› | ‹curriculum› Paediatrics | ‹start› to ‹day› Training year ‹n› when it ended | Withdrawn ‹day› | View |
[aria-label: View ‹further person 9›'s record]
| ‹further person› | ‹curriculum› Paediatrics | ‹start› to ‹day› Training year ‹n› when it ended | Withdrawn ‹day› | View |
[aria-label: View ‹further person 10›'s record]
| ‹further person› | ‹curriculum› Paediatrics | ‹start› to ‹day› Training year ‹n› when it ended | Withdrawn ‹day› | View |
[aria-label: View ‹further person 11›'s record]
| ‹further person› | ‹curriculum› Paediatrics | ‹start› to ‹day› Training year ‹n› when it ended | Withdrawn ‹day› | View |
[aria-label: View ‹further person 12›'s record]
| ‹further person› | ‹curriculum› Paediatrics | ‹start› to ‹day› Training year ‹n› when it ended | Withdrawn ‹day› | View |
[aria-label: View ‹further person 13›'s record]
| ‹further person› | ‹curriculum› Paediatrics | ‹start› to ‹day› Training year ‹n› when it ended | Withdrawn ‹day› | View |
[aria-label: View ‹further person 14›'s record]
| ‹further person› | ‹curriculum› Paediatrics | ‹start› to ‹day› Training year ‹n› when it ended | Withdrawn ‹day› | View |
[aria-label: View ‹further person 15›'s record]
| ‹further person› | ‹curriculum› Paediatrics | ‹start› to ‹day› Training year ‹n› when it ended | Withdrawn ‹day› | View |
[aria-label: View ‹further person 16›'s record]
| ‹further person› | ‹curriculum› Paediatrics | ‹start› to ‹day› Training year ‹n› when it ended | Withdrawn ‹day› | View |
[aria-label: View ‹further person 17›'s record]
| ‹further person› | ‹curriculum› Paediatrics | ‹start› to ‹day› Training year ‹n› when it ended | Withdrawn ‹day› | View |
[aria-label: View ‹further person 18›'s record]
| ‹further person› | ‹curriculum› Paediatrics | ‹start› to ‹day› Training year ‹n› when it ended | Withdrawn ‹day› | View |
[aria-label: View ‹further person 19›'s record]
| ‹further person› | ‹curriculum› Paediatrics | ‹start› to ‹day› Training year ‹n› when it ended | Withdrawn ‹day› | View |
[aria-label: View ‹further person 20›'s record]
| ‹further person› | ‹curriculum› Paediatrics | ‹start› to ‹day› Training year ‹n› when it ended | Withdrawn ‹day› | View |
[aria-label: Completed and closed, pages]
Showing 1–20 of 40
Previous
Next
Ended programmes per page
10
20
50
100

######## BOARD R3-C05-1280.dc.html — R3-C05 · Trainees · two people with one name · Illustration · 1280
> The address under each name and in View's name (flow 11's rule).
> chips: PROPOSED · Illustration
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
## Trainees
Admit registrars who are waiting, and keep current trainees' programmes.
### Waiting to be admitted
Nobody is waiting to be admitted.
A registrar appears here after registering from a Trainee invitation.
### Current trainees 3 current
Flow 11's table, unchanged: Anele Dlamini, Nomsa Mahlangu, Sipho Ndlovu, each with Edit.
### Completed and closed 2 ended
Programmes that have ended, the latest end first. An ended record is read-only.
| Name | Curriculum and sub-speciality | Programme | Outcome | Actions |
[aria-label: View ‹name›'s record, ‹address 1›]
| ‹name› ‹address 1› | ‹curriculum› Paediatrics | ‹start› to ‹day› Training year ‹n› when it ended | Completed ‹day› | View |
[aria-label: View ‹name›'s record, ‹address 2›]
| ‹name› ‹address 2› | ‹curriculum› Paediatrics | ‹start› to ‹day› Training year ‹n› when it ended | Withdrawn ‹day› | View |

######## BOARD R3-C05-390.dc.html — R3-C05 · Trainees · two people with one name · Illustration · 390
> The address under each name and in View's name (flow 11's rule).
> chips: PROPOSED · Illustration
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
## Trainees
Admit registrars who are waiting, and keep current trainees' programmes.
### Waiting to be admitted
Nobody is waiting to be admitted.
A registrar appears here after registering from a Trainee invitation.
### Current trainees 3 current
Flow 11's table, unchanged: Anele Dlamini, Nomsa Mahlangu, Sipho Ndlovu, each with Edit.
### Completed and closed 2 ended
Programmes that have ended, the latest end first. An ended record is read-only.
| Name | Curriculum and sub-speciality | Programme | Outcome | Actions |
[aria-label: View ‹name›'s record, ‹address 1›]
| ‹name› ‹address 1› | ‹curriculum› Paediatrics | ‹start› to ‹day› Training year ‹n› when it ended | Completed ‹day› | View |
[aria-label: View ‹name›'s record, ‹address 2›]
| ‹name› ‹address 2› | ‹curriculum› Paediatrics | ‹start› to ‹day› Training year ‹n› when it ended | Withdrawn ‹day› | View |

######## BOARD R3-C06-1280.dc.html — R3-C06 · Trainees · the Administrator after 5.27 · Illustration · 1280
> devadmin at /admin/trainees by its address (no Trainees item, nothing lit): the header “Name and institution”, the institution under each name, as the Current list (F7).
> chips: PROPOSED · Illustration
Wombat
Acting as Administrator
Menu
Close
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
Demo Administrator
Sign out
## Trainees
Admit registrars who are waiting, and keep current trainees' programmes.
### Waiting to be admitted
Nobody is waiting to be admitted.
A registrar appears here after registering from a Trainee invitation.
### Current trainees 3 current
Flow 11's table, unchanged: Anele Dlamini, Nomsa Mahlangu, Sipho Ndlovu, each with Edit.
### Completed and closed 2 ended
Programmes that have ended, the latest end first. An ended record is read-only.
| Name and institution | Curriculum and sub-speciality | Programme | Outcome | Actions |
[aria-label: View Lerato Molefe's record]
| Lerato Molefe Kgosi Kgari Teaching Hospital | Paediatric EPA Curriculum (11.1) Paediatrics | 2023-01-15 to 2026-10-26 Training year 4 when it ended | Completed 2026-10-26 | View |
[aria-label: View Pieter du Plessis's record]
| Pieter du Plessis Kgosi Kgari Teaching Hospital | Paediatric EPA Curriculum (11.1) Paediatrics | 2025-01-15 to 2026-10-24 Training year 2 when it ended | Withdrawn 2026-10-24 | View |

######## BOARD R3-C06-390.dc.html — R3-C06 · Trainees · the Administrator after 5.27 · Illustration · 390
> devadmin at /admin/trainees by its address (no Trainees item, nothing lit): the header “Name and institution”, the institution under each name, as the Current list (F7).
> chips: PROPOSED · Illustration
Wombat
Acting as Administrator
Menu
Close
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
Demo Administrator
Sign out
## Trainees
Admit registrars who are waiting, and keep current trainees' programmes.
### Waiting to be admitted
Nobody is waiting to be admitted.
A registrar appears here after registering from a Trainee invitation.
### Current trainees 3 current
Flow 11's table, unchanged: Anele Dlamini, Nomsa Mahlangu, Sipho Ndlovu, each with Edit.
### Completed and closed 2 ended
Programmes that have ended, the latest end first. An ended record is read-only.
| Name and institution | Curriculum and sub-speciality | Programme | Outcome | Actions |
[aria-label: View Lerato Molefe's record]
| Lerato Molefe Kgosi Kgari Teaching Hospital | Paediatric EPA Curriculum (11.1) Paediatrics | 2023-01-15 to 2026-10-26 Training year 4 when it ended | Completed 2026-10-26 | View |
[aria-label: View Pieter du Plessis's record]
| Pieter du Plessis Kgosi Kgari Teaching Hospital | Paediatric EPA Curriculum (11.1) Paediatrics | 2025-01-15 to 2026-10-24 Training year 2 when it ended | Withdrawn 2026-10-24 | View |

######## BOARD R3-C07-1280.dc.html — R3-C07 · Trainees · A.1.14, after the erasure · 1280
> An erased person listed as Withdrawn under the pseudonym (F7), placed by its day, latest end first; here drawn first, ‹its place if its day is earlier›.
> chips: PROPOSED
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
## Trainees
Admit registrars who are waiting, and keep current trainees' programmes.
### Waiting to be admitted
Nobody is waiting to be admitted.
A registrar appears here after registering from a Trainee invitation.
### Current trainees ‹n› current
Flow 11's table, unchanged: ‹the current trainees at A.1.14›, each with Edit.
### Completed and closed 3 ended
Programmes that have ended, the latest end first. An ended record is read-only.
| Name | Curriculum and sub-speciality | Programme | Outcome | Actions |
[aria-label: View ‹deleted_user_…›'s record]
| ‹deleted_user_…› | ‹curriculum› Paediatrics | ‹start› to ‹its day› Training year ‹n› when it ended | Withdrawn ‹its day› | View |
[aria-label: View Lerato Molefe's record]
| Lerato Molefe | Paediatric EPA Curriculum (11.1) Paediatrics | 2023-01-15 to 2026-10-26 Training year 4 when it ended | Completed 2026-10-26 | View |
[aria-label: View Pieter du Plessis's record]
| Pieter du Plessis | Paediatric EPA Curriculum (11.1) Paediatrics | 2025-01-15 to 2026-10-24 Training year 2 when it ended | Withdrawn 2026-10-24 | View |

######## BOARD R3-C07-390.dc.html — R3-C07 · Trainees · A.1.14, after the erasure · 390
> An erased person listed as Withdrawn under the pseudonym (F7), placed by its day, latest end first; here drawn first, ‹its place if its day is earlier›.
> chips: PROPOSED
Wombat
Acting as Institutional admin
Menu
Close
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
Entrustment decisions
Access and audit
SSO mappings
Audit log
My data rights
Nolwazi Mbatha
Sign out
## Trainees
Admit registrars who are waiting, and keep current trainees' programmes.
### Waiting to be admitted
Nobody is waiting to be admitted.
A registrar appears here after registering from a Trainee invitation.
### Current trainees ‹n› current
Flow 11's table, unchanged: ‹the current trainees at A.1.14›, each with Edit.
### Completed and closed 3 ended
Programmes that have ended, the latest end first. An ended record is read-only.
| Name | Curriculum and sub-speciality | Programme | Outcome | Actions |
[aria-label: View ‹deleted_user_…›'s record]
| ‹deleted_user_…› | ‹curriculum› Paediatrics | ‹start› to ‹its day› Training year ‹n› when it ended | Withdrawn ‹its day› | View |
[aria-label: View Lerato Molefe's record]
| Lerato Molefe | Paediatric EPA Curriculum (11.1) Paediatrics | 2023-01-15 to 2026-10-26 Training year 4 when it ended | Completed 2026-10-26 | View |
[aria-label: View Pieter du Plessis's record]
| Pieter du Plessis | Paediatric EPA Curriculum (11.1) Paediatrics | 2025-01-15 to 2026-10-24 Training year 2 when it ended | Withdrawn 2026-10-24 | View |

######## BOARD R3-R01-1280.dc.html — R3-R01 · My progress · completed (5.20) · 1280
> The built subtitle (S7); the notice as flow 05 built it, then the record line (MyProgress.razor's, around EndedRecord, S8): My authorisations and My activities. PAED-010 and PAED-012 under Each semester with their periods; the standing as 5.20 records it (FIXTURES).
> chips: PROPOSED
Wombat
Menu
Close
[aria-label: Main]
Home
My progress
Export portfolio
My data rights
Lerato Molefe
Sign out
## My progress
Your record of the programme you ended: what your curriculum held you to in each period.
You completed your programme on 2026-10-26. This page is your record of it and is read-only: no target applies to you any more. A period your programme ended in before that period's last month has no target, and no period after it is listed.
Your record:
My authorisations
My activities
### Curriculum targets
#### Your programme
Started
2023-01-15
Completed
2026-10-26
Training year
4 when your programme ended
#### Each semester
[aria-label: PAED-010 — Leading and operating within a clinical team]
PAED-010
Leading and operating within a clinical team
Target: 3 per semester (6 a year). Minimum when your programme ended: 5.
Semester 2, 2026
no target (your programme ended part-way through) · 1 recorded
Semester 1, 2026
0 of 3, 3 short
Semester 2, 2025
0 of 3, 3 short
Semester 1, 2025
0 of 3, 3 short
Semester 2, 2024
0 of 3, 3 short
Semester 1, 2024
0 of 3, 3 short
Semester 2, 2023
0 of 3, 3 short
Semester 1, 2023
0 of 3, 3 short
[aria-label: PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams]
PAED-012
Communicating with and counselling patients, caregivers and healthcare teams
Target: 3 per semester (6 a year). Minimum when your programme ended: 5.
Semester 2, 2026
no target (your programme ended part-way through) · 2 recorded
Semester 1, 2026
0 of 3, 3 short
Semester 2, 2025
0 of 3, 3 short
Semester 1, 2025
0 of 3, 3 short
Semester 2, 2024
0 of 3, 3 short
Semester 1, 2024
0 of 3, 3 short
Semester 2, 2023
0 of 3, 3 short
Semester 1, 2023
0 of 3, 3 short
PAED-001 to PAED-015's other cards, as flow 05 draws them.
### Entrustment against Annexure A
You completed your programme on 2026-10-26, in training year 4, so the targets shown are that year's. STAR decisions and ratings are shown as they stand today. Each target is the level the curriculum sets for training year 4: Annexure A's, for the College's EPAs. The exit level is the level each EPA must reach to finish the programme. A level is compared only on the ladder the curriculum sets for that EPA.
15 at or above · 0 below · 0 with no decision, of 15 EPAs
15 of 15 EPAs at their exit level by STAR decision (level 5: 9 of 9 · level 4: 6 of 6). Every EPA is at its exit level.
The standing table, as flow 05 built it, its rows read on the last day.

######## BOARD R3-R01-390.dc.html — R3-R01 · My progress · completed (5.20) · 390
> The built subtitle (S7); the notice as flow 05 built it, then the record line (MyProgress.razor's, around EndedRecord, S8): My authorisations and My activities. PAED-010 and PAED-012 under Each semester with their periods; the standing as 5.20 records it (FIXTURES).
> chips: PROPOSED
Wombat
Menu
Close
[aria-label: Main]
Home
My progress
Export portfolio
My data rights
Lerato Molefe
Sign out
## My progress
Your record of the programme you ended: what your curriculum held you to in each period.
You completed your programme on 2026-10-26. This page is your record of it and is read-only: no target applies to you any more. A period your programme ended in before that period's last month has no target, and no period after it is listed.
Your record:
My authorisations
My activities
### Curriculum targets
#### Your programme
Started
2023-01-15
Completed
2026-10-26
Training year
4 when your programme ended
#### Each semester
[aria-label: PAED-010 — Leading and operating within a clinical team]
PAED-010
Leading and operating within a clinical team
Target: 3 per semester (6 a year). Minimum when your programme ended: 5.
Semester 2, 2026
no target (your programme ended part-way through) · 1 recorded
Semester 1, 2026
0 of 3, 3 short
Semester 2, 2025
0 of 3, 3 short
Semester 1, 2025
0 of 3, 3 short
Semester 2, 2024
0 of 3, 3 short
Semester 1, 2024
0 of 3, 3 short
Semester 2, 2023
0 of 3, 3 short
Semester 1, 2023
0 of 3, 3 short
[aria-label: PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams]
PAED-012
Communicating with and counselling patients, caregivers and healthcare teams
Target: 3 per semester (6 a year). Minimum when your programme ended: 5.
Semester 2, 2026
no target (your programme ended part-way through) · 2 recorded
Semester 1, 2026
0 of 3, 3 short
Semester 2, 2025
0 of 3, 3 short
Semester 1, 2025
0 of 3, 3 short
Semester 2, 2024
0 of 3, 3 short
Semester 1, 2024
0 of 3, 3 short
Semester 2, 2023
0 of 3, 3 short
Semester 1, 2023
0 of 3, 3 short
PAED-001 to PAED-015's other cards, as flow 05 draws them.
### Entrustment against Annexure A
You completed your programme on 2026-10-26, in training year 4, so the targets shown are that year's. STAR decisions and ratings are shown as they stand today. Each target is the level the curriculum sets for training year 4: Annexure A's, for the College's EPAs. The exit level is the level each EPA must reach to finish the programme. A level is compared only on the ladder the curriculum sets for that EPA.
15 at or above · 0 below · 0 with no decision, of 15 EPAs
15 of 15 EPAs at their exit level by STAR decision (level 5: 9 of 9 · level 4: 6 of 6). Every EPA is at its exit level.
The standing table, as flow 05 built it, its rows read on the last day.

######## BOARD R3-R02-1280.dc.html — R3-R02 · My progress · withdrawn (5.28) · 1280
> No STAR decision: the line links My activities only. PAED-002's Semester 2, 2026 reads 0 recorded after the take-back.
> chips: PROPOSED
Wombat
Menu
Close
[aria-label: Main]
Home
My progress
Export portfolio
My data rights
Pieter du Plessis
Sign out
## My progress
Your record of the programme you ended: what your curriculum held you to in each period.
Your programme ended on 2026-10-24. This page is your record of it and is read-only: no target applies to you any more. A period your programme ended in before that period's last month has no target, and no period after it is listed.
Your record:
My activities
### Curriculum targets
#### Your programme
Started
2025-01-15
Ended
2026-10-24
Training year
2 when your programme ended
#### Each semester
[aria-label: PAED-002 — Managing common paediatric presentations]
PAED-002
Managing common paediatric presentations
Target: 3 per semester (6 a year). Minimum when your programme ended: 3b.
Semester 2, 2026
no target (your programme ended part-way through) · 0 recorded
Semester 1, 2026
0 of 3, 3 short
Semester 2, 2025
‹as 5.28-2 records it›
Semester 1, 2025
‹as 5.28-2 records it›
Pieter du Plessis's other cards, as flow 05 draws them.
### Entrustment against Annexure A
‹the standing as 5.28-2 records it›

######## BOARD R3-R02-390.dc.html — R3-R02 · My progress · withdrawn (5.28) · 390
> No STAR decision: the line links My activities only. PAED-002's Semester 2, 2026 reads 0 recorded after the take-back.
> chips: PROPOSED
Wombat
Menu
Close
[aria-label: Main]
Home
My progress
Export portfolio
My data rights
Pieter du Plessis
Sign out
## My progress
Your record of the programme you ended: what your curriculum held you to in each period.
Your programme ended on 2026-10-24. This page is your record of it and is read-only: no target applies to you any more. A period your programme ended in before that period's last month has no target, and no period after it is listed.
Your record:
My activities
### Curriculum targets
#### Your programme
Started
2025-01-15
Ended
2026-10-24
Training year
2 when your programme ended
#### Each semester
[aria-label: PAED-002 — Managing common paediatric presentations]
PAED-002
Managing common paediatric presentations
Target: 3 per semester (6 a year). Minimum when your programme ended: 3b.
Semester 2, 2026
no target (your programme ended part-way through) · 0 recorded
Semester 1, 2026
0 of 3, 3 short
Semester 2, 2025
‹as 5.28-2 records it›
Semester 1, 2025
‹as 5.28-2 records it›
Pieter du Plessis's other cards, as flow 05 draws them.
### Entrustment against Annexure A
‹the standing as 5.28-2 records it›

######## BOARD R3-R03-1280.dc.html — R3-R03 · PAED-001's page, ended (5.20) · 1280
> Flow 05's headings and ended lines (FIXTURES); Open My authorisations (.epa-auth-link) now shown to Lerato Molefe (Q2, PROPOSED).
> chips: PROPOSED
Wombat
Menu
Close
[aria-label: Main]
Home
My progress
Export portfolio
My data rights
Lerato Molefe
Sign out
[aria-label: Breadcrumb]
Home
My progress
- PAED-001
## PAED-001 — Providing paediatric emergency care to children
3 a semester · Decided each semester · Exit level 5
### Observations
Semester 2, 2026
no target (your programme ended part-way through) · 3 recorded
Semester 1, 2026
0 of 3, 3 short
Semester 2, 2025
0 of 3, 3 short
Semester 1, 2025
0 of 3, 3 short
Semester 2, 2024
0 of 3, 3 short
Semester 1, 2024
0 of 3, 3 short
Semester 2, 2023
0 of 3, 3 short
Semester 1, 2023
0 of 3, 3 short
Training year 4: level 5, the minimum each encounter is judged against and your STAR's target.
‹the two MSF lines, as 5.20-2 shows them›
### Entrustment
5 At or above
Issued
2026-10-26
Exit level
5 · reached
Open My authorisations
### Rating trajectory
3 ratings in the 2026 academic year, from Thandi Zulu, David Naidoo and Mohammed Patel. 2 at the minimum, 1 below.
‹the chart and its table, as the capture shows them›
### Activities on this EPA
‹the activities table, as the capture shows it›

######## BOARD R3-R03-390.dc.html — R3-R03 · PAED-001's page, ended (5.20) · 390
> Flow 05's headings and ended lines (FIXTURES); Open My authorisations (.epa-auth-link) now shown to Lerato Molefe (Q2, PROPOSED).
> chips: PROPOSED
Wombat
Menu
Close
[aria-label: Main]
Home
My progress
Export portfolio
My data rights
Lerato Molefe
Sign out
[aria-label: Breadcrumb]
Home
My progress
- PAED-001
## PAED-001 — Providing paediatric emergency care to children
3 a semester · Decided each semester · Exit level 5
### Observations
Semester 2, 2026
no target (your programme ended part-way through) · 3 recorded
Semester 1, 2026
0 of 3, 3 short
Semester 2, 2025
0 of 3, 3 short
Semester 1, 2025
0 of 3, 3 short
Semester 2, 2024
0 of 3, 3 short
Semester 1, 2024
0 of 3, 3 short
Semester 2, 2023
0 of 3, 3 short
Semester 1, 2023
0 of 3, 3 short
Training year 4: level 5, the minimum each encounter is judged against and your STAR's target.
‹the two MSF lines, as 5.20-2 shows them›
### Entrustment
5 At or above
Issued
2026-10-26
Exit level
5 · reached
Open My authorisations
### Rating trajectory
3 ratings in the 2026 academic year, from Thandi Zulu, David Naidoo and Mohammed Patel. 2 at the minimum, 1 below.
‹the chart and its table, as the capture shows them›
### Activities on this EPA
‹the activities table, as the capture shows it›

######## BOARD R3-R04-1280.dc.html — R3-R04 · Pieter du Plessis's PAED-002 page, ended · 1280
> No STAR: “No STAR yet.”, no link.
Wombat
Menu
Close
[aria-label: Main]
Home
My progress
Export portfolio
My data rights
Pieter du Plessis
Sign out
[aria-label: Breadcrumb]
Home
My progress
- PAED-002
## PAED-002 — Managing common paediatric presentations
3 a semester · Decided each semester · Exit level 5
### Observations
Semester 2, 2026
no target (your programme ended part-way through) · 0 recorded
Semester 1, 2026
0 of 3, 3 short
Semester 2, 2025
‹as 5.28-2 records it›
Semester 1, 2025
‹as 5.28-2 records it›
Training year 2: level 3b, the minimum each encounter is judged against and your STAR's target.
### Entrustment
No STAR yet.
### Rating trajectory
‹the trajectory's line, as the capture shows it›
‹the chart and its table, as the capture shows them›
### Activities on this EPA
‹the activities table, as the capture shows it›

######## BOARD R3-R04-390.dc.html — R3-R04 · Pieter du Plessis's PAED-002 page, ended · 390
> No STAR: “No STAR yet.”, no link.
Wombat
Menu
Close
[aria-label: Main]
Home
My progress
Export portfolio
My data rights
Pieter du Plessis
Sign out
[aria-label: Breadcrumb]
Home
My progress
- PAED-002
## PAED-002 — Managing common paediatric presentations
3 a semester · Decided each semester · Exit level 5
### Observations
Semester 2, 2026
no target (your programme ended part-way through) · 0 recorded
Semester 1, 2026
0 of 3, 3 short
Semester 2, 2025
‹as 5.28-2 records it›
Semester 1, 2025
‹as 5.28-2 records it›
Training year 2: level 3b, the minimum each encounter is judged against and your STAR's target.
### Entrustment
No STAR yet.
### Rating trajectory
‹the trajectory's line, as the capture shows it›
‹the chart and its table, as the capture shows them›
### Activities on this EPA
‹the activities table, as the capture shows it›

######## BOARD R3-R05-1280.dc.html — R3-R05 · My progress · 6.21, PAED-012 paused after Lerato Molefe graduated · 1280
> The ended card for PAED-012 kept, headed by EpaLabel “(no longer in use)”, its code a link (N4); the standing 15 of 15; PAED-006 with its corrected title (6.14).
> chips: after T312 · PROPOSED
Wombat
Menu
Close
[aria-label: Main]
Home
My progress
Export portfolio
My data rights
Lerato Molefe
Sign out
## My progress
Your record of the programme you ended: what your curriculum held you to in each period.
You completed your programme on 2026-10-26. This page is your record of it and is read-only: no target applies to you any more. A period your programme ended in before that period's last month has no target, and no period after it is listed.
Your record:
My authorisations
My activities
### Curriculum targets
#### Your programme
Started
2023-01-15
Completed
2026-10-26
Training year
4 when your programme ended
#### Each semester
[aria-label: PAED-006 — Managing long-term health conditions (LTHCs) in children]
PAED-006
Managing long-term health conditions (LTHCs) in children
‹target and minimum, as 6.21 records them›
Semester 2, 2026
‹as 6.21 records it›
[aria-label: PAED-010 — Leading and operating within a clinical team]
PAED-010
Leading and operating within a clinical team
Target: 3 per semester (6 a year). Minimum when your programme ended: 5.
Semester 2, 2026
no target (your programme ended part-way through) · 1 recorded
Semester 1, 2026
0 of 3, 3 short
Semester 2, 2025
0 of 3, 3 short
Semester 1, 2025
0 of 3, 3 short
Semester 2, 2024
0 of 3, 3 short
Semester 1, 2024
0 of 3, 3 short
Semester 2, 2023
0 of 3, 3 short
Semester 1, 2023
0 of 3, 3 short
[aria-label: PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams]
PAED-012 (no longer in use)
Communicating with and counselling patients, caregivers and healthcare teams
Target: 3 per semester (6 a year). Minimum when your programme ended: 5.
Semester 2, 2026
no target (your programme ended part-way through) · 2 recorded
Semester 1, 2026
0 of 3, 3 short
Semester 2, 2025
0 of 3, 3 short
Semester 1, 2025
0 of 3, 3 short
Semester 2, 2024
0 of 3, 3 short
Semester 1, 2024
0 of 3, 3 short
Semester 2, 2023
0 of 3, 3 short
Semester 1, 2023
0 of 3, 3 short
PAED-001 to PAED-015's other cards, as flow 05 draws them.
### Entrustment against Annexure A
You completed your programme on 2026-10-26, in training year 4, so the targets shown are that year's. STAR decisions and ratings are shown as they stand today. Each target is the level the curriculum sets for training year 4: Annexure A's, for the College's EPAs. The exit level is the level each EPA must reach to finish the programme. A level is compared only on the ladder the curriculum sets for that EPA.
15 at or above · 0 below · 0 with no decision, of 15 EPAs
15 of 15 EPAs at their exit level by STAR decision (level 5: 9 of 9 · level 4: 6 of 6). Every EPA is at its exit level.
The standing table, as flow 05 built it, its rows read on the last day.

######## BOARD R3-R05-390.dc.html — R3-R05 · My progress · 6.21, PAED-012 paused after Lerato Molefe graduated · 390
> The ended card for PAED-012 kept, headed by EpaLabel “(no longer in use)”, its code a link (N4); the standing 15 of 15; PAED-006 with its corrected title (6.14).
> chips: after T312 · PROPOSED
Wombat
Menu
Close
[aria-label: Main]
Home
My progress
Export portfolio
My data rights
Lerato Molefe
Sign out
## My progress
Your record of the programme you ended: what your curriculum held you to in each period.
You completed your programme on 2026-10-26. This page is your record of it and is read-only: no target applies to you any more. A period your programme ended in before that period's last month has no target, and no period after it is listed.
Your record:
My authorisations
My activities
### Curriculum targets
#### Your programme
Started
2023-01-15
Completed
2026-10-26
Training year
4 when your programme ended
#### Each semester
[aria-label: PAED-006 — Managing long-term health conditions (LTHCs) in children]
PAED-006
Managing long-term health conditions (LTHCs) in children
‹target and minimum, as 6.21 records them›
Semester 2, 2026
‹as 6.21 records it›
[aria-label: PAED-010 — Leading and operating within a clinical team]
PAED-010
Leading and operating within a clinical team
Target: 3 per semester (6 a year). Minimum when your programme ended: 5.
Semester 2, 2026
no target (your programme ended part-way through) · 1 recorded
Semester 1, 2026
0 of 3, 3 short
Semester 2, 2025
0 of 3, 3 short
Semester 1, 2025
0 of 3, 3 short
Semester 2, 2024
0 of 3, 3 short
Semester 1, 2024
0 of 3, 3 short
Semester 2, 2023
0 of 3, 3 short
Semester 1, 2023
0 of 3, 3 short
[aria-label: PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams]
PAED-012 (no longer in use)
Communicating with and counselling patients, caregivers and healthcare teams
Target: 3 per semester (6 a year). Minimum when your programme ended: 5.
Semester 2, 2026
no target (your programme ended part-way through) · 2 recorded
Semester 1, 2026
0 of 3, 3 short
Semester 2, 2025
0 of 3, 3 short
Semester 1, 2025
0 of 3, 3 short
Semester 2, 2024
0 of 3, 3 short
Semester 1, 2024
0 of 3, 3 short
Semester 2, 2023
0 of 3, 3 short
Semester 1, 2023
0 of 3, 3 short
PAED-001 to PAED-015's other cards, as flow 05 draws them.
### Entrustment against Annexure A
You completed your programme on 2026-10-26, in training year 4, so the targets shown are that year's. STAR decisions and ratings are shown as they stand today. Each target is the level the curriculum sets for training year 4: Annexure A's, for the College's EPAs. The exit level is the level each EPA must reach to finish the programme. A level is compared only on the ladder the curriculum sets for that EPA.
15 at or above · 0 below · 0 with no decision, of 15 EPAs
15 of 15 EPAs at their exit level by STAR decision (level 5: 9 of 9 · level 4: 6 of 6). Every EPA is at its exit level.
The standing table, as flow 05 built it, its rows read on the last day.

######## BOARD R3-R06-1280.dc.html — R3-R06 · PAED-012's page, paused (6.21) · 1280
> As T312 delivers it: no “credited if it is restored” alert on an ended record; flow 05's ended lines; Entrustment from the standing, with Open My authorisations; headed “(no longer in use)”.
> chips: after T312 · PROPOSED
Wombat
Menu
Close
[aria-label: Main]
Home
My progress
Export portfolio
My data rights
Lerato Molefe
Sign out
[aria-label: Breadcrumb]
Home
My progress
- PAED-012
## PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams (no longer in use)
3 a semester · Decided each semester · Exit level 5
### Observations
Semester 2, 2026
no target (your programme ended part-way through) · 2 recorded
Semester 1, 2026
0 of 3, 3 short
Semester 2, 2025
0 of 3, 3 short
Semester 1, 2025
0 of 3, 3 short
Semester 2, 2024
0 of 3, 3 short
Semester 1, 2024
0 of 3, 3 short
Semester 2, 2023
0 of 3, 3 short
Semester 1, 2023
0 of 3, 3 short
Training year 4: level 5, the minimum each encounter is judged against and your STAR's target.
### Entrustment
5 At or above
Issued
2026-10-26
Exit level
5 · reached
Open My authorisations
### Rating trajectory (no longer in use)
‹the trajectory's line, as 6.21-2 shows it; ratings 2026-10-19 and 2026-10-21›
‹the chart and its table, as the capture shows them›
### Activities on this EPA
‹the activities table, as the capture shows it›

######## BOARD R3-R06-390.dc.html — R3-R06 · PAED-012's page, paused (6.21) · 390
> As T312 delivers it: no “credited if it is restored” alert on an ended record; flow 05's ended lines; Entrustment from the standing, with Open My authorisations; headed “(no longer in use)”.
> chips: after T312 · PROPOSED
Wombat
Menu
Close
[aria-label: Main]
Home
My progress
Export portfolio
My data rights
Lerato Molefe
Sign out
[aria-label: Breadcrumb]
Home
My progress
- PAED-012
## PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams (no longer in use)
3 a semester · Decided each semester · Exit level 5
### Observations
Semester 2, 2026
no target (your programme ended part-way through) · 2 recorded
Semester 1, 2026
0 of 3, 3 short
Semester 2, 2025
0 of 3, 3 short
Semester 1, 2025
0 of 3, 3 short
Semester 2, 2024
0 of 3, 3 short
Semester 1, 2024
0 of 3, 3 short
Semester 2, 2023
0 of 3, 3 short
Semester 1, 2023
0 of 3, 3 short
Training year 4: level 5, the minimum each encounter is judged against and your STAR's target.
### Entrustment
5 At or above
Issued
2026-10-26
Exit level
5 · reached
Open My authorisations
### Rating trajectory (no longer in use)
‹the trajectory's line, as 6.21-2 shows it; ratings 2026-10-19 and 2026-10-21›
‹the chart and its table, as the capture shows them›
### Activities on this EPA
‹the activities table, as the capture shows it›

######## BOARD R3-R07-1280.dc.html — R3-R07 · My activities · Pieter du Plessis (5.28) · 1280
> Flow 03's table as built, under Home › My progress › My activities; a caption the build adds (S18); no Log an activity; the built subtitle with a NEW second sentence; the Mini-CEX “to David Naidoo” (N7) and its note row (NEW, the reason DATA).
> chips: PROPOSED
Wombat
Menu
Close
[aria-label: Main]
Home
My progress
Export portfolio
My data rights
Pieter du Plessis
Sign out
[aria-label: Breadcrumb]
Home
My progress
- My activities
## My activities
Everything you have filed or logged, newest encounter first. Read-only: your programme ended on 2026-10-24, so nothing new can be filed.
### All activities (7)
Every activity filed by you or about you, with who has it now and its credit.
| Activity | Who has it now | State | Credit |
| DOPS (Paediatrics) · ‹EPA› · ‹date› | ‹who› | Cancelled | ‹credit› |
| Mini-CEX (Paediatrics) · PAED-002 · 2026-10-25, to David Naidoo | Done | Completed | None |
| Observed after your last day in the programme, 2026-10-24, so it counts towards nothing. |
| Portfolio and Logbook Review · ‹EPA› · ‹date› | ‹who› | Awaiting review | ‹credit› |
| Case-Based Discussion (Paediatrics) · ‹EPA› · ‹date› | ‹who› | Requested | ‹credit› |
| Teaching Session Log · ‹date› | ‹who› | ‹state› | ‹credit› |
| Teaching Session Log · ‹date› | ‹who› | ‹state› | ‹credit› |
| Teaching Session Log · ‹date› | ‹who› | ‹state› | ‹credit› |

######## BOARD R3-R07-768.dc.html — R3-R07 · My activities · Pieter du Plessis (5.28) · 768
> Flow 03's table as built, under Home › My progress › My activities; a caption the build adds (S18); no Log an activity; the built subtitle with a NEW second sentence; the Mini-CEX “to David Naidoo” (N7) and its note row (NEW, the reason DATA).
> chips: PROPOSED
Wombat
Menu
Close
[aria-label: Main]
Home
My progress
Export portfolio
My data rights
Pieter du Plessis
Sign out
[aria-label: Breadcrumb]
Home
My progress
- My activities
## My activities
Everything you have filed or logged, newest encounter first. Read-only: your programme ended on 2026-10-24, so nothing new can be filed.
### All activities (7)
Every activity filed by you or about you, with who has it now and its credit.
| Activity | Who has it now | State | Credit |
| DOPS (Paediatrics) · ‹EPA› · ‹date› | ‹who› | Cancelled | ‹credit› |
| Mini-CEX (Paediatrics) · PAED-002 · 2026-10-25, to David Naidoo | Done | Completed | None |
| Observed after your last day in the programme, 2026-10-24, so it counts towards nothing. |
| Portfolio and Logbook Review · ‹EPA› · ‹date› | ‹who› | Awaiting review | ‹credit› |
| Case-Based Discussion (Paediatrics) · ‹EPA› · ‹date› | ‹who› | Requested | ‹credit› |
| Teaching Session Log · ‹date› | ‹who› | ‹state› | ‹credit› |
| Teaching Session Log · ‹date› | ‹who› | ‹state› | ‹credit› |
| Teaching Session Log · ‹date› | ‹who› | ‹state› | ‹credit› |

######## BOARD R3-R07-390.dc.html — R3-R07 · My activities · Pieter du Plessis (5.28) · 390
> Flow 03's table as built, under Home › My progress › My activities; a caption the build adds (S18); no Log an activity; the built subtitle with a NEW second sentence; the Mini-CEX “to David Naidoo” (N7) and its note row (NEW, the reason DATA).
> chips: PROPOSED
Wombat
Menu
Close
[aria-label: Main]
Home
My progress
Export portfolio
My data rights
Pieter du Plessis
Sign out
[aria-label: Breadcrumb]
Home
My progress
- My activities
## My activities
Everything you have filed or logged, newest encounter first. Read-only: your programme ended on 2026-10-24, so nothing new can be filed.
### All activities (7)
Every activity filed by you or about you, with who has it now and its credit.
| Activity | Who has it now | State | Credit |
| DOPS (Paediatrics) · ‹EPA› · ‹date› | ‹who› | Cancelled | ‹credit› |
| Mini-CEX (Paediatrics) · PAED-002 · 2026-10-25, to David Naidoo | Done | Completed | None |
| Observed after your last day in the programme, 2026-10-24, so it counts towards nothing. |
| Portfolio and Logbook Review · ‹EPA› · ‹date› | ‹who› | Awaiting review | ‹credit› |
| Case-Based Discussion (Paediatrics) · ‹EPA› · ‹date› | ‹who› | Requested | ‹credit› |
| Teaching Session Log · ‹date› | ‹who› | ‹state› | ‹credit› |
| Teaching Session Log · ‹date› | ‹who› | ‹state› | ‹credit› |
| Teaching Session Log · ‹date› | ‹who› | ‹state› | ‹credit› |

######## BOARD R3-R08-1280.dc.html — R3-R08 · My authorisations · as a former trainee · 1280
> Admitted to the record (Q3); 15 cards, levels and rationales as seeded, each Issued 2026-10-26, No expiry, Download named for its EPA; My progress lit.
> chips: PROPOSED
Wombat
Menu
Close
[aria-label: Main]
Home
My progress
Export portfolio
My data rights
Lerato Molefe
Sign out
[aria-label: Breadcrumb]
Home
My progress
- My authorisations
## My authorisations
The Statements of Awarded Responsibility (STAR) the committee has issued you, and their certificates.
### In force
#### PAED-001 — Providing paediatric emergency care to children
Authorised level
5
Issued
2026-10-26
Expires
No expiry
Leads resuscitation and emergency care without prompting; ready for unsupervised practice.
[aria-label: Download certificate for PAED-001]
Download certificate
#### PAED-002 — Managing common paediatric presentations
Authorised level
5
Issued
2026-10-26
Expires
No expiry
PAED-002 entrusted at its exit level; the panel recommends completion.
[aria-label: Download certificate for PAED-002]
Download certificate
#### PAED-003 — ‹title, as seeded›
Authorised level
5
Issued
2026-10-26
Expires
No expiry
PAED-003 entrusted at its exit level; the panel recommends completion.
[aria-label: Download certificate for PAED-003]
Download certificate
#### PAED-004 — ‹title, as seeded›
Authorised level
5
Issued
2026-10-26
Expires
No expiry
PAED-004 entrusted at its exit level; the panel recommends completion.
[aria-label: Download certificate for PAED-004]
Download certificate
#### PAED-005 — ‹title, as seeded›
Authorised level
5
Issued
2026-10-26
Expires
No expiry
PAED-005 entrusted at its exit level; the panel recommends completion.
[aria-label: Download certificate for PAED-005]
Download certificate
#### PAED-006 — ‹title, as seeded›
Authorised level
5
Issued
2026-10-26
Expires
No expiry
PAED-006 entrusted at its exit level; the panel recommends completion.
[aria-label: Download certificate for PAED-006]
Download certificate
#### PAED-007 — ‹title, as seeded›
Authorised level
5
Issued
2026-10-26
Expires
No expiry
PAED-007 entrusted at its exit level; the panel recommends completion.
[aria-label: Download certificate for PAED-007]
Download certificate
#### PAED-008 — ‹title, as seeded›
Authorised level
4
Issued
2026-10-26
Expires
No expiry
PAED-008 entrusted at its exit level; the panel recommends completion.
[aria-label: Download certificate for PAED-008]
Download certificate
#### PAED-009 — ‹title, as seeded›
Authorised level
4
Issued
2026-10-26
Expires
No expiry
PAED-009 entrusted at its exit level; the panel recommends completion.
[aria-label: Download certificate for PAED-009]
Download certificate
#### PAED-010 — Leading and operating within a clinical team
Authorised level
5
Issued
2026-10-26
Expires
No expiry
PAED-010 entrusted at its exit level; the panel recommends completion.
[aria-label: Download certificate for PAED-010]
Download certificate
#### PAED-011 — ‹title, as seeded›
Authorised level
4
Issued
2026-10-26
Expires
No expiry
PAED-011 entrusted at its exit level; the panel recommends completion.
[aria-label: Download certificate for PAED-011]
Download certificate
#### PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
Authorised level
5
Issued
2026-10-26
Expires
No expiry
Breaks difficult news clearly and kindly; consistent across settings.
[aria-label: Download certificate for PAED-012]
Download certificate
#### PAED-013 — ‹title, as seeded›
Authorised level
4
Issued
2026-10-26
Expires
No expiry
PAED-013 entrusted at its exit level; the panel recommends completion.
[aria-label: Download certificate for PAED-013]
Download certificate
#### PAED-014 — ‹title, as seeded›
Authorised level
4
Issued
2026-10-26
Expires
No expiry
PAED-014 entrusted at its exit level; the panel recommends completion.
[aria-label: Download certificate for PAED-014]
Download certificate
#### PAED-015 — ‹title, as seeded›
Authorised level
4
Issued
2026-10-26
Expires
No expiry
PAED-015 entrusted at its exit level; the panel recommends completion.
[aria-label: Download certificate for PAED-015]
Download certificate

######## BOARD R3-R08-390.dc.html — R3-R08 · My authorisations · as a former trainee · 390
> Admitted to the record (Q3); 15 cards, levels and rationales as seeded, each Issued 2026-10-26, No expiry, Download named for its EPA; My progress lit.
> chips: PROPOSED
Wombat
Menu
Close
[aria-label: Main]
Home
My progress
Export portfolio
My data rights
Lerato Molefe
Sign out
[aria-label: Breadcrumb]
Home
My progress
- My authorisations
## My authorisations
The Statements of Awarded Responsibility (STAR) the committee has issued you, and their certificates.
### In force
#### PAED-001 — Providing paediatric emergency care to children
Authorised level
5
Issued
2026-10-26
Expires
No expiry
Leads resuscitation and emergency care without prompting; ready for unsupervised practice.
[aria-label: Download certificate for PAED-001]
Download certificate
#### PAED-002 — Managing common paediatric presentations
Authorised level
5
Issued
2026-10-26
Expires
No expiry
PAED-002 entrusted at its exit level; the panel recommends completion.
[aria-label: Download certificate for PAED-002]
Download certificate
#### PAED-003 — ‹title, as seeded›
Authorised level
5
Issued
2026-10-26
Expires
No expiry
PAED-003 entrusted at its exit level; the panel recommends completion.
[aria-label: Download certificate for PAED-003]
Download certificate
#### PAED-004 — ‹title, as seeded›
Authorised level
5
Issued
2026-10-26
Expires
No expiry
PAED-004 entrusted at its exit level; the panel recommends completion.
[aria-label: Download certificate for PAED-004]
Download certificate
#### PAED-005 — ‹title, as seeded›
Authorised level
5
Issued
2026-10-26
Expires
No expiry
PAED-005 entrusted at its exit level; the panel recommends completion.
[aria-label: Download certificate for PAED-005]
Download certificate
#### PAED-006 — ‹title, as seeded›
Authorised level
5
Issued
2026-10-26
Expires
No expiry
PAED-006 entrusted at its exit level; the panel recommends completion.
[aria-label: Download certificate for PAED-006]
Download certificate
#### PAED-007 — ‹title, as seeded›
Authorised level
5
Issued
2026-10-26
Expires
No expiry
PAED-007 entrusted at its exit level; the panel recommends completion.
[aria-label: Download certificate for PAED-007]
Download certificate
#### PAED-008 — ‹title, as seeded›
Authorised level
4
Issued
2026-10-26
Expires
No expiry
PAED-008 entrusted at its exit level; the panel recommends completion.
[aria-label: Download certificate for PAED-008]
Download certificate
#### PAED-009 — ‹title, as seeded›
Authorised level
4
Issued
2026-10-26
Expires
No expiry
PAED-009 entrusted at its exit level; the panel recommends completion.
[aria-label: Download certificate for PAED-009]
Download certificate
#### PAED-010 — Leading and operating within a clinical team
Authorised level
5
Issued
2026-10-26
Expires
No expiry
PAED-010 entrusted at its exit level; the panel recommends completion.
[aria-label: Download certificate for PAED-010]
Download certificate
#### PAED-011 — ‹title, as seeded›
Authorised level
4
Issued
2026-10-26
Expires
No expiry
PAED-011 entrusted at its exit level; the panel recommends completion.
[aria-label: Download certificate for PAED-011]
Download certificate
#### PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
Authorised level
5
Issued
2026-10-26
Expires
No expiry
Breaks difficult news clearly and kindly; consistent across settings.
[aria-label: Download certificate for PAED-012]
Download certificate
#### PAED-013 — ‹title, as seeded›
Authorised level
4
Issued
2026-10-26
Expires
No expiry
PAED-013 entrusted at its exit level; the panel recommends completion.
[aria-label: Download certificate for PAED-013]
Download certificate
#### PAED-014 — ‹title, as seeded›
Authorised level
4
Issued
2026-10-26
Expires
No expiry
PAED-014 entrusted at its exit level; the panel recommends completion.
[aria-label: Download certificate for PAED-014]
Download certificate
#### PAED-015 — ‹title, as seeded›
Authorised level
4
Issued
2026-10-26
Expires
No expiry
PAED-015 entrusted at its exit level; the panel recommends completion.
[aria-label: Download certificate for PAED-015]
Download certificate

######## BOARD R3-R09-1280.dc.html — R3-R09 · My authorisations · Pieter du Plessis · 1280
> Flow 09's empty card, as worded; reached by its address only, since nothing links it for Pieter du Plessis.
> chips: PROPOSED
Wombat
Menu
Close
[aria-label: Main]
Home
My progress
Export portfolio
My data rights
Pieter du Plessis
Sign out
[aria-label: Breadcrumb]
Home
My progress
- My authorisations
## My authorisations
The Statements of Awarded Responsibility (STAR) the committee has issued you, and their certificates.
No active authorisations yet
When a committee review ratifies a decision authorising you to perform an EPA, it will appear here. You can then download a STAR certificate for your records.

######## BOARD R3-R09-390.dc.html — R3-R09 · My authorisations · Pieter du Plessis · 390
> Flow 09's empty card, as worded; reached by its address only, since nothing links it for Pieter du Plessis.
> chips: PROPOSED
Wombat
Menu
Close
[aria-label: Main]
Home
My progress
Export portfolio
My data rights
Pieter du Plessis
Sign out
[aria-label: Breadcrumb]
Home
My progress
- My authorisations
## My authorisations
The Statements of Awarded Responsibility (STAR) the committee has issued you, and their certificates.
No active authorisations yet
When a committee review ratifies a decision authorising you to perform an EPA, it will appear here. You can then download a STAR certificate for your records.

######## BOARD R3-R10-1280.dc.html — R3-R10 · My progress · a record with 17 EPA cards · Illustration · 1280
> The grid takes them as flow 05 lays them out; the standing's counts bracketed so 17 cards and the standing hold at once (S15).
> chips: Illustration
Wombat
Menu
Close
[aria-label: Main]
Home
My progress
Export portfolio
My data rights
Lerato Molefe
Sign out
## My progress
Your record of the programme you ended: what your curriculum held you to in each period.
You completed your programme on 2026-10-26. This page is your record of it and is read-only: no target applies to you any more. A period your programme ended in before that period's last month has no target, and no period after it is listed.
Your record:
My authorisations
My activities
### Curriculum targets
#### Your programme
Started
2023-01-15
Completed
2026-10-26
Training year
4 when your programme ended
#### Each semester
[aria-label: PAED-001 — Providing paediatric emergency care to children]
PAED-001
Providing paediatric emergency care to children
Target: ‹as it was on the last day›.
Semester 2, 2026
no target (your programme ended part-way through) · ‹n› recorded
[aria-label: PAED-002 — Managing common paediatric presentations]
PAED-002
Managing common paediatric presentations
Target: ‹as it was on the last day›.
Semester 2, 2026
no target (your programme ended part-way through) · ‹n› recorded
[aria-label: PAED-003 — ‹title›]
PAED-003
‹title›
Target: ‹as it was on the last day›.
Semester 2, 2026
no target (your programme ended part-way through) · ‹n› recorded
[aria-label: PAED-004 — ‹title›]
PAED-004
‹title›
Target: ‹as it was on the last day›.
Semester 2, 2026
no target (your programme ended part-way through) · ‹n› recorded
[aria-label: PAED-005 — ‹title›]
PAED-005
‹title›
Target: ‹as it was on the last day›.
Semester 2, 2026
no target (your programme ended part-way through) · ‹n› recorded
[aria-label: PAED-006 — ‹title›]
PAED-006
‹title›
Target: ‹as it was on the last day›.
Semester 2, 2026
no target (your programme ended part-way through) · ‹n› recorded
[aria-label: PAED-007 — ‹title›]
PAED-007
‹title›
Target: ‹as it was on the last day›.
Semester 2, 2026
no target (your programme ended part-way through) · ‹n› recorded
[aria-label: PAED-008 — ‹title›]
PAED-008
‹title›
Target: ‹as it was on the last day›.
Semester 2, 2026
no target (your programme ended part-way through) · ‹n› recorded
[aria-label: PAED-009 — ‹title›]
PAED-009
‹title›
Target: ‹as it was on the last day›.
Semester 2, 2026
no target (your programme ended part-way through) · ‹n› recorded
[aria-label: PAED-010 — Leading and operating within a clinical team]
PAED-010
Leading and operating within a clinical team
Target: ‹as it was on the last day›.
Semester 2, 2026
no target (your programme ended part-way through) · ‹n› recorded
[aria-label: PAED-011 — ‹title›]
PAED-011
‹title›
Target: ‹as it was on the last day›.
Semester 2, 2026
no target (your programme ended part-way through) · ‹n› recorded
[aria-label: PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams]
PAED-012
Communicating with and counselling patients, caregivers and healthcare teams
Target: ‹as it was on the last day›.
Semester 2, 2026
no target (your programme ended part-way through) · ‹n› recorded
[aria-label: PAED-013 — ‹title›]
PAED-013
‹title›
Target: ‹as it was on the last day›.
Semester 2, 2026
no target (your programme ended part-way through) · ‹n› recorded
[aria-label: PAED-014 — ‹title›]
PAED-014
‹title›
Target: ‹as it was on the last day›.
Semester 2, 2026
no target (your programme ended part-way through) · ‹n› recorded
[aria-label: PAED-015 — ‹title›]
PAED-015
‹title›
Target: ‹as it was on the last day›.
Semester 2, 2026
no target (your programme ended part-way through) · ‹n› recorded
[aria-label: ‹PAED-016› — ‹title›]
‹PAED-016›
‹title›
Target: ‹as it was on the last day›.
Semester 2, 2026
no target (your programme ended part-way through) · ‹n› recorded
[aria-label: ‹PAED-017› — ‹title›]
‹PAED-017›
‹title›
Target: ‹as it was on the last day›.
Semester 2, 2026
no target (your programme ended part-way through) · ‹n› recorded
### Entrustment against Annexure A
You completed your programme on 2026-10-26, in training year 4, so the targets shown are that year's. STAR decisions and ratings are shown as they stand today. Each target is the level the curriculum sets for training year 4: Annexure A's, for the College's EPAs. The exit level is the level each EPA must reach to finish the programme. A level is compared only on the ladder the curriculum sets for that EPA.
‹m› at or above · ‹…› below · ‹…› with no decision, of 17 EPAs
‹m› of 17 EPAs at their exit level by STAR decision (level 5: ‹…› · level 4: ‹…›).
The standing table, as flow 05 built it, its rows read on the last day.

######## BOARD R3-R10-390.dc.html — R3-R10 · My progress · a record with 17 EPA cards · Illustration · 390
> The grid takes them as flow 05 lays them out; the standing's counts bracketed so 17 cards and the standing hold at once (S15).
> chips: Illustration
Wombat
Menu
Close
[aria-label: Main]
Home
My progress
Export portfolio
My data rights
Lerato Molefe
Sign out
## My progress
Your record of the programme you ended: what your curriculum held you to in each period.
You completed your programme on 2026-10-26. This page is your record of it and is read-only: no target applies to you any more. A period your programme ended in before that period's last month has no target, and no period after it is listed.
Your record:
My authorisations
My activities
### Curriculum targets
#### Your programme
Started
2023-01-15
Completed
2026-10-26
Training year
4 when your programme ended
#### Each semester
[aria-label: PAED-001 — Providing paediatric emergency care to children]
PAED-001
Providing paediatric emergency care to children
Target: ‹as it was on the last day›.
Semester 2, 2026
no target (your programme ended part-way through) · ‹n› recorded
[aria-label: PAED-002 — Managing common paediatric presentations]
PAED-002
Managing common paediatric presentations
Target: ‹as it was on the last day›.
Semester 2, 2026
no target (your programme ended part-way through) · ‹n› recorded
[aria-label: PAED-003 — ‹title›]
PAED-003
‹title›
Target: ‹as it was on the last day›.
Semester 2, 2026
no target (your programme ended part-way through) · ‹n› recorded
[aria-label: PAED-004 — ‹title›]
PAED-004
‹title›
Target: ‹as it was on the last day›.
Semester 2, 2026
no target (your programme ended part-way through) · ‹n› recorded
[aria-label: PAED-005 — ‹title›]
PAED-005
‹title›
Target: ‹as it was on the last day›.
Semester 2, 2026
no target (your programme ended part-way through) · ‹n› recorded
[aria-label: PAED-006 — ‹title›]
PAED-006
‹title›
Target: ‹as it was on the last day›.
Semester 2, 2026
no target (your programme ended part-way through) · ‹n› recorded
[aria-label: PAED-007 — ‹title›]
PAED-007
‹title›
Target: ‹as it was on the last day›.
Semester 2, 2026
no target (your programme ended part-way through) · ‹n› recorded
[aria-label: PAED-008 — ‹title›]
PAED-008
‹title›
Target: ‹as it was on the last day›.
Semester 2, 2026
no target (your programme ended part-way through) · ‹n› recorded
[aria-label: PAED-009 — ‹title›]
PAED-009
‹title›
Target: ‹as it was on the last day›.
Semester 2, 2026
no target (your programme ended part-way through) · ‹n› recorded
[aria-label: PAED-010 — Leading and operating within a clinical team]
PAED-010
Leading and operating within a clinical team
Target: ‹as it was on the last day›.
Semester 2, 2026
no target (your programme ended part-way through) · ‹n› recorded
[aria-label: PAED-011 — ‹title›]
PAED-011
‹title›
Target: ‹as it was on the last day›.
Semester 2, 2026
no target (your programme ended part-way through) · ‹n› recorded
[aria-label: PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams]
PAED-012
Communicating with and counselling patients, caregivers and healthcare teams
Target: ‹as it was on the last day›.
Semester 2, 2026
no target (your programme ended part-way through) · ‹n› recorded
[aria-label: PAED-013 — ‹title›]
PAED-013
‹title›
Target: ‹as it was on the last day›.
Semester 2, 2026
no target (your programme ended part-way through) · ‹n› recorded
[aria-label: PAED-014 — ‹title›]
PAED-014
‹title›
Target: ‹as it was on the last day›.
Semester 2, 2026
no target (your programme ended part-way through) · ‹n› recorded
[aria-label: PAED-015 — ‹title›]
PAED-015
‹title›
Target: ‹as it was on the last day›.
Semester 2, 2026
no target (your programme ended part-way through) · ‹n› recorded
[aria-label: ‹PAED-016› — ‹title›]
‹PAED-016›
‹title›
Target: ‹as it was on the last day›.
Semester 2, 2026
no target (your programme ended part-way through) · ‹n› recorded
[aria-label: ‹PAED-017› — ‹title›]
‹PAED-017›
‹title›
Target: ‹as it was on the last day›.
Semester 2, 2026
no target (your programme ended part-way through) · ‹n› recorded
### Entrustment against Annexure A
You completed your programme on 2026-10-26, in training year 4, so the targets shown are that year's. STAR decisions and ratings are shown as they stand today. Each target is the level the curriculum sets for training year 4: Annexure A's, for the College's EPAs. The exit level is the level each EPA must reach to finish the programme. A level is compared only on the ladder the curriculum sets for that EPA.
‹m› at or above · ‹…› below · ‹…› with no decision, of 17 EPAs
‹m› of 17 EPAs at their exit level by STAR decision (level 5: ‹…› · level 4: ‹…›).
The standing table, as flow 05 built it, its rows read on the last day.
