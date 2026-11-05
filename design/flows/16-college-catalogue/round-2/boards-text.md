# Flow 16 round 2: the text of every board

Generated from `round-2/R2-*.dc.html` (T394, build-contracts § 0) by a script, as flow 14's was: style, script, svg, head and helmet blocks stripped; headings, labels, aria-labels, placeholders, input values, alt text, `[checked]`, `[disabled]`, `[aria-disabled]`, `[aria-haspopup]` and annotations kept; a link is marked `[link]`, a button `[button]`. `round-2/`'s `canvas.json` is not read for order; the order is by id: the Spec, Steps and check boards, then A, B, C, D, E, F, each at 1280 then its other widths. An `anno` chip is quoted after `>`. Design data: a caption, a chip, a label strip or a `DATA:` note is never a shipped word. A table row is one `| a | b |` line; a `dt` opens with `**`.

######## BOARD R2-0-spec-2.dc.html
### R2-0-spec-2 · flow 16, round 2 (continued)
#### 6 · The changes the boards need, as code changes
- NavItems: EntrustmentScales joins the CollegeAdmin row after ActivityTypes (D57); DESIGN.md's NavMenu role table with it (NavMenuAuthorizationTests parses it); the owner table: the sub-specialities route redirects to the tree (Specialities / Colleges lit), the scales list lights itself for the College admin.
- EntrustmentScalesList: policy admits CollegeAdmin (today AdministratorOrInstitutionalAdmin, :2); the reader's branch; Delete… opens a ConfirmDialog (Q11). EntrustmentScaleEdit stays Administrator: an EditForm with h2 sections, titled by the scale (T1); Unavailable for an unknown id (today :154–157 return to the list); Remove outline; Up, Down and Remove only while nothing uses the scale (Q16), from a "scale in use" read; fixed failure words (T393; today :176, :262).
- UpdateEntrustmentScaleCommand: refuses a reorder or removal on a scale in use (Q16, T395; today the guard runs only on a removal, :62–84); the rung guard collects every item needing a removed rung by EPA code and curriculum name and version, and the count (Q6; today the first, by id and rank, EntrustmentScaleReferences.cs:213–222); the ended-record refusal in label words (S3).
- A tree read: every speciality of a College with its sub-specialities and each default scale's name in one query (today one per speciality; SubSpecialityDto has only the scale's id); null for another College or an unknown id → Unavailable (Q7).
- CreateSubSpecialityCommand gains DefaultEntrustmentScaleId. The record pages return to the tree with a one-shot token (CatalogueLinks); their Back links go (CollegeEdit:12, SpecialityEdit:12, SubSpecialityEdit:13, SubSpecialitiesList:10–13); Deactivate… a ConfirmDialog on a record stored active, Save with Active unticked asking the same (D56); refusals under their field (T324); fixed failure words (T272).
- CreateCollege, UpdateCollege: a check by name and by short code before the save, naming the holder (S10); the unique violation only for a race; any other DbUpdateException the failed-save sentence (today CreateCollege.cs:57–60, UpdateCollege.cs:60–63). "College created." by a token. Created as the South African day (N2).
- MySpecialitiesRedirect: no College → the NEW words on Specialities (today /admin/colleges). CollegeAdminDashboard: Q12's sentence, four links in the card's footer, its comment (Q17).
- The reach (Q2, T8): per curriculum and per EPA, § 9's counts (DATA), read by the items page, the item page, the EPA page (the dialog; EV2) and the curriculum's page.
- CurriculumEdit: § THE CURRICULUM'S PAGE: titled by the version (T2); no Back links; .user-page.catalogue-record; "Open for adoption" with T301's help (today "Active", :72–78); Hold back… and its dialog (Q4, T6); ?created and ?cloned tokens (Q5; today :219–221, :254–255, :154); field refusals, no ValidationSummary (today :42, :98); fixed failure words (today :193, :231, :259); Unavailable (today /not-found, :176); a loading line (today :36). UpdateCurriculumCommand, CloneCurriculumAsNewVersionCommand: a duplicate only on the unique violation.
- CurriculumItemsWords.NothingToAdd: Q14's branch (today :93–97); the item page's Cannot add an item the same. CurriculumAdminScope: T242 names the holder (Q10; today :286–287).
- The College's words (T326): EpaWords' College subtitle and caption; the EPA page's College subtitles and help; CurriculaWords.SubtitleCollege; the EPA list's College column off for the College admin (T3); the empty state for no College.
- CurriculumItemsEdit and CurriculumItemPage: the reach in the notice and the subtitle; "Edit this version" (T4).
#### 7 · 44 px at 390, and the tables at 768
- .people-page on every list and the tree; StackWide's .clinic-table--stack-wide .btn from 900 px; .user-page on every record and the curriculum's page (its buttons, inputs and selects 44 px; .form-actions stacked, primary on top, Cancel first in the DOM); .catalogue-record (S1) for their checkbox row; .curriculum-item-page and .epa-page as built; .dialog-card for every dialog; DashboardCard's footer for Home's links.
- At 768 (sidebar beside, a 486 px column): Colleges, the tree, the scales, the levels and Curricula stack (StackWide from 900 px down); the EPA list and the items stack from 1024 px down, as built.
#### 8 · DESIGN.md text each changes
- § Button system: the deactivation question on a College, speciality and sub-speciality (D56), the delete question on a scale (Q11), Hold back… on a curriculum (T6); Remove on a rung an outline button that asks nothing; no Reactivate button: the Active box reactivates.
- § Form system: refusals under their field on every part-1 form and the curriculum's page, no ValidationSummary; a rung by its label; an item by its EPA and curriculum.
- § Table system: StackWide on Colleges and the tree; the levels table at 390 by StackWide.
- § Page-level patterns: a record titled by its name; Unavailable gains College, Speciality, Sub-speciality and Entrustment scale; no Back links; the curriculum's page of h2 sections.
- § The NavMenu's tables (D57; the redirect); the College admin's Home card.
#### 9 · Messages by kind
Built = in the code; R1 = round 1's, accepted; NEW = this message's; PROPOSED (THIS ASK) = T1–T12.
| Kind | Words | Source |
| Result | "College created." · "College saved." · "College of Neurologists of South Africa deactivated. A new invitation can no longer name it." · "… reactivated. A new invitation can name it again." · "Speciality saved." · "Sub-speciality saved." · "Sub-speciality Neonatology created." · "Entrustment scale saved." · "Entrustment scale deleted." | R1 |
| Result | "Speciality Neurology created." · "Sub-speciality Neonatology deactivated. A new invitation can no longer name it." · "Entrustment scale created." | NEW |
| Result | "Neonatology EPA Curriculum (1.0) created. It is held back: no institution can adopt it until you open it." · "Paediatric EPA Curriculum (11.2) cloned from 11.1, with its 15 national items. It is held back." | Q5 |
| Result | "Curriculum saved. It is open for adoption: an institution can adopt it now." · "Curriculum saved. It is held back: no institution can adopt it until you open it again." | PROPOSED (THIS ASK) T7 |
| Refusal | "Enter a name." · "Enter a short code." · "Enter this rung's label." · "A scale needs at least two rungs. Add another." · "CPSA is already the short code of College of Paediatricians of South Africa. Choose another." · "College of Paediatricians of South Africa is already a College's name. Choose another." · "“Indirect supervision” is already the label of another rung. Give each rung its own label." (replaces "Level labels must be unique within a scale.") | R1 |
| Refusal | "Enter the new version." · "Effective to must be on or after Effective from." · the stale-page reorder and the ended-record rung (R2-C12, C13) | NEW |
| Refusal | Q6 (R2-C11); Q10 (R2-F14); 6.8 word for word; "An entrustment scale named '…' already exists."; "A curriculum with the same name and version already exists for this sub-speciality."; "Enter a code."; "Enter a title."; "An EPA with the same code already exists for this sub-speciality."; Delete's others: MSF questions, pinned items, an ended record's item, scored progress, a published form, entrustment decisions (FIXTURES) | Q6, Q10, built |
| Failure | "The College could not be saved. Nothing has changed. Try again, or come back in a few minutes." (and speciality, sub-speciality, entrustment scale, curriculum; "could not be created"; "The new version could not be made.") — replaces round 1's "Not saved. Something went wrong on our side, …" | NEW |
| Failure | "Could not load the Colleges." · "this College" · "the specialities" · "this speciality" · "this sub-speciality" · "this entrustment scale" · "this curriculum", each then "Nothing has changed. Try again, or come back in a few minutes."; loading lines "Loading …" | NEW |
| Question | "Deactivate the sub-speciality Neonatology?" / "Deactivate the speciality Paediatrics?" with D56's sentences in Q15's words · "Hold back Paediatric EPA Curriculum (11.1)?" | NEW PROPOSED (THIS ASK) (M8, T6) |
| Question | "Deactivate College of Neurologists of South Africa?" (R1) · "Delete CNSA Neurology Entrustment Scale?" / "It is removed from every list. This cannot be undone." (Q11) + "A scale that anything uses is not deleted: …" (R1, PROPOSED) / "Delete scale" | R1, Q11 |
| Notice | The reach: "Adopted by 1 institution, Kgosi Kgari Teaching Hospital; 3 registrars in training on it." (Q2) · "No institution has adopted this version yet." · EPA reach (Q2 in the pause dialog; EV2's line, NEW) · T8's "Still used by 1 institution that has superseded it, Kgosi Kgari Teaching Hospital; 2 registrars in training on it." (words only) | Q2, NEW |
| Hint | Q16's "Rungs can be reordered or removed only while nothing uses this scale." · the inactive lines · the subtitles of § NEW | NEW |
| Empty | "No College on your account" / "Your account names no College. Ask the platform administrator to add it." · Q14's "Neonatology has no national EPA yet. …" | R1, Q14 |
##### DATA reads, by query
- The tree (one query, the default scale's name).
- A College's holder of a name or code (S10).
- A scale's uses (Q16; the delete refusal's order).
- Every item needing a removed rung, with its count (Q6).
- The reach per curriculum and per EPA (Q2, T8).
- T242's holding institution (Q10).
#### 10 · Tokens and contrast
Every colour an existing token. .muted (--muted-text) on --surface-color 5.1:1 and on --background-color 4.9:1 (the reach line, the inactive line, the section lines); --danger-color on --danger-bg and --success-color on --success-bg as built; borders and the focus ring as built.
#### 11 · Edge cases
- A College with no College admin: the Administrator keeps the CNSA tree (R2-A10).
- A College admin whose account names no College (R2-A21, E19).
- A speciality with no sub-speciality (R2-A12).
- A default ladder another page would delete: refused while it is the default (R2-C08).
- A rung a pinned item needs (R2-C11).
- A record deactivated while an institution has adopted its curriculum: the adoption and its registrars carry on; the dialog says so, no count (R2-A15).
- Two Colleges with similar names (R2-B16).
- A 2,000-character description (R2-A22, B17).
- A ladder of 8 (R2-C20).
- A scale used by forty items: "and 37 more" (words only).
- Speciality and sub-speciality both "Paediatrics": both tabs "Paediatrics · Wombat"; the subtitles tell them apart (words only).
- A version superseded with registrars still on it (T8, words only).
- A version held back after an institution adopted it: it keeps it (R2-D12).
- A curriculum's sub-speciality changed after it has items or an adoption: stays a select (T11); the alternative "fixed once the version has an item or an adoption" for the review (words only).
- An adoption made between 00:00 and 02:00 SAST and the Created dates: the South African day (words only).
- T242's holder (R2-F14).

######## BOARD R2-0-spec.dc.html
### R2-0-spec · flow 16, round 2 (part 1 at full fidelity, and part 2)
Round 2 draws the picks A2, B1 and C1 at full fidelity, and part 2 (the curriculum's page; the EPA list and an EPA's page; Curricula, the items and an item's page), with EV drawn twice. 303 boards at 1280, 768 and 390 on the page "Round 2"; round 1's 50 boards stay on "Round 1", untouched. One stylesheet, project/f16r2.css.
#### 1 · The captures
They arrived from the folder, not as attachments. No image reached this session with the message (the uploads folder was empty again). Access to C:\dev\Wombat\design\upload was asked for and granted this round; all 70 captures are present under baseline\. Each was read by staging it into the session and viewing it as an image.
|  | Captures |
| Viewed (9) | act-1\1.12-1-kruger-specialities.pngact-1\1.14-1-sub-speciality-ladder.pngact-1\1.2-1-colleges-list.pngact-1\1.4-2-cpsa-ladder.pngact-6\6.10-1-kruger-home.pngact-6\6.13-1-neonatology-curriculum-created.pngact-1\1.16-1-college-epas.pngact-1\1.18-1-college-items.pngact-A\A.7.10-1-home-390.png |
| Staged, not viewed (14) | act-1\1.13-1, act-1\1.3-1, act-6\6.1-1, act-6\6.3-1, act-6\6.5-1, act-6\6.8-1, act-6\6.10-2, states\entrustment-scale-edit--rung-refused, states\college-edit--invalid, act-1\1.17-1, act-6\6.17-1, states\curriculum-edit--clone, states\epas-list--local, act-1\1.4-1. Their words are given verbatim in FIXTURES and drawn from there. |
| Not opened (47) | The other 47 of § 4. No value rests on them: § CAPTURE-ONLY is empty, and every value on the boards is FIXTURES'. |
##### What a viewed capture shows that the boards draw differently
Each is today's page against the decided design.
| Capture | Today | The boards |
| 1.2-1 | Subtitle "National governing bodies (CMSA constituent colleges) that own the EPAs and curricula institutions adopt."; Created "2026-11-02 16:26". | Round 1's accepted subtitle; Created the South African day, ISO (N2). |
| 1.12-1 | "Back to colleges" beside Create speciality; subtitle "College: College of …"; a list of specialities. | The tree (Q1); no Back link (T291); S9's subtitle. |
| 1.14-1 | "Edit sub-speciality" / "Speciality: Paediatrics"; "Back to sub-specialities"; Deactivate a filled red button. | "Paediatrics" / "Sub-speciality of Paediatrics" (S11, M8); no Back; Deactivate… outline, asking (D56). |
| 1.4-2 | Two cards side by side (Scale details, Levels) with h3-sized headings; Up, Down, filled red Remove on a scale in use; descriptors cut in one-line inputs; "Back to scales". | One column, h2 Details and Levels; Q16: no moves on a scale in use; each descriptor whole in a textarea (S4); titled by name (T1). |
| 6.10-1, A.7.10-1 | "Your college's specialities, its EPAs and its curricula. Institutions adopt an active curriculum to train against it."; three links, 36 px at 390. | Q12's sentence and four links, in DashboardCard's footer so the 44 px phone rule reaches them; Entrustment scales in the menu (D57). |
| 6.13-1 | "Edit curriculum", two cards (details | "Clone as new version"), Status "Active", Back to curricula, Manage items. | The curriculum's page of h2 sections (Q4), titled by name and version (T2), "Open for adoption", "Items (0)" (T4). |
| 1.16-1 | A College column on every row; the long titles wrap to five lines (taken before the column was widened). | No College column (T3); the EPA column 45% from 1025 px, as built. |
| 1.18-1 | As flow 15 built it. | Unchanged but the reach first in the notice (Q2) and "Edit this version" (T4). |
#### 2 · The decisions, and where each is drawn
|  | Drawn on |
| Q1 the tree | R2-A02, A04, A07, A12, A13b, A14b, A16b, A22 |
| Q2 the reach | R2-D05–D14, E07, F05–F13, F20 (and EV2 on E05, E06, E09, E11) |
| Q3 = D56 deactivation | R2-A03, A15–A17, B07–B11 |
| Q4 the curriculum's page | R2-D01–D19 |
| Q5 results after create and clone | R2-D05, D08 |
| Q6 the rung refusal | R2-C11 |
| Q7 Unavailable | R2-A18a–c, B13, C17, D15, E15, F16, F17 |
| Q8 = D57 the scales for the College admin | R2-C21a, C21b; the menu on every Anton Kruger board |
| Q9 no Back links; header links | every part-1 board; R2-D05 on ("Items (n)"); R2-F05 on ("Edit this version") |
| Q10 T242 names the holder | R2-F14 |
| Q11 Delete… asks | R2-C07, C08 |
| Q12, Q17 Home's card | R2-A01 |
| Q13 what the build folds in | § 6 |
| Q14 no national EPA | R2-F06, F19 |
| Q15 no count in a deactivation dialog | R2-A15, A17, B09, B10 |
| Q16 = T395 moves only while unused | R2-C02 (in use, no moves), C04, C05, C20 (unused), C12 (the handler) |
Corrections and the other points
|  | Where |
| M1 D57 drawn | every Anton Kruger board (menu); R2-C21; R2-A05 (the help alone); R2-A01 (no second paragraph); R2-C01 (S2 subtitle) |
| M2 Home's card | R2-A01 |
| M3 Unavailable words | R2-A18a–c, B13, C17, D15, E15, F16, F17 |
| M4 the delete dialog | R2-C07 |
| M5 "and 6 more" | R2-C11 |
| M6 no Reactivate | none drawn anywhere |
| M7 Steps | R2-0-steps |
| M8 the kind named | R2-A02 (aria-labels), A03, A05, A15, A17 |
| S1 6.8's sentence | R2-C08 |
| S2 the built list words | R2-C01, C06 |
| S3 the scale's refusals | R2-C03b, C11–C15; the Delete ones in § 9 |
| S4 textareas | R2-C02–C05, C20 |
| S5 Q15 | R2-A15 |
| S6 levels at 390 | R2-C02-390, C05-390 (measured, § 5) |
| S7 no bracket where a value is given | every board |
| S8 every state | rows A–F |
| S9 the tree's subtitle | every tree board |
| S10 the holder named | R2-B04, B05 |
| S11 a record titled by its name | R2-A03, A05, A14a, B06–B12, C02–C05, D05–D14, E05–E11 |
| N1 no .record-page | none used |
| N2 Created, SA day | R2-B01 |
| N3 "college" in action labels only | said here once: an action label keeps "Create college"; in running text College is a proper noun |
| N4 one verb, Create | every tree board |
| N5 StackWide | Colleges and the tree gain it; the scales list has it |
| N6 caption | "Sub-specialities of Paediatrics" |
| N7 | moot |
| N8 no gendered pronoun | every board and this Spec (checked by search) |
| N9 "Create scale" | R2-C03a, C03b, C14, C15 |
| N10 one counting rule | R2-0-steps |
| T1–T12 | T1 R2-C02; T2 R2-D05; T3 R2-E01; T4 R2-D05, F05; T5 R2-A16a, A16b; T6 R2-D06, D12; T7 R2-D11, D13; T8 § 9 words only; T9 R2-D01; T10 none drawn (no link); T11 § 11; T12 R2-F20f |
| The deactivation help | R2-A03, A05: D56 within Q15 |
| No College on the account | R2-A21, E19 |
| The one-shot token to the tree | R2-A04, A07, A12, A13b, A14b |
##### Illustrations
R2-A09, R2-A15, R2-A16a, R2-A16b, R2-A17, R2-A21, R2-A22, R2-B05, R2-B09, R2-B10, R2-B11a, R2-B11b, R2-B12, R2-B16, R2-B17a, R2-B17b, R2-C09, R2-C10, R2-C11, R2-C12, R2-C13, R2-C14, R2-C15a, R2-C15b, R2-C15c, R2-C16, R2-C20, R2-D03, R2-D04, R2-D09a, R2-D09b, R2-D12, R2-D13, R2-D14, R2-D18, R2-E14, R2-E19, R2-E20, R2-F14, R2-F15, R2-F18. Each carries the chip and "(Illustration)" in its title. Loading, load errors, required fields and Unavailable states are every page's states, drawn with the replay's values, so they carry no chip.
##### EV: recommend EV2
The goal asks that Anton Kruger be told before saving how far a correction reaches; EV1 tells nothing outside the pause dialog, so 6.14 and 6.28 read no count. EV2 is one standing info Alert (no role) above the form, and costs a line, not a query: it is the same per-EPA read the pause dialog needs. It also answers 6.28 truthfully ("on no curriculum an institution has adopted"). EV2's line is drawn on R2-E05, E06, E09 and E11 (-EV2) only; R2-E07, E08, E10, E12–E20 draw neither (E07's dialog carries the reach itself; E10 has no EPA yet).
#### 3 · Components
Design-system components used; NEW only .catalogue-record.
| Component | Boards |
| PageHeader (h1, subtitle, trail, header actions) | every board |
| DashboardCard (title with book icon, body, footer of links) | R2-A01 |
| DataTable with StackWide (.clinic-table--stack-wide) | Colleges, the tree's sections, the scales, the levels, EPAs, Curricula, the items |
| PagerControls | none: no list passes twenty rows |
| DataTable .clinic-table--inputs (levels; a textarea a rung) | R2-C02–C05, C11–C16, C20 |
| FormField, FieldHelp, FieldRefusal (.validation-message role="alert", first in aria-describedby) | every form |
| FormActions (.form-actions) | every form |
| ActionResult + Alert (success, danger with alert-row and Try again / Put rung 5 back) | every result and failure |
| Alert, Role="" (standing notice) | the items' notice, the paused notice, EV2, the rename warning |
| ConfirmDialog, action mode (Cancel autofocus, danger confirm) | R2-A15, A17, B09, B10, C07, D12, D14, E07, F15 |
| Unavailable (.detail-card--empty, one primary Go to …) | R2-A18a–c, B13, C17, D15, E15, F16, F17 |
| StatePanel empty | R2-A10, A21, E19 |
| Skeleton + p.loading-line[role="status"] | R2-A19, B14, C18, D16, E16, E18a |
| StatePanel load error with Try again (refresh-cw) | R2-A20, B15, C19, D17, E17, E18b |
| Icons (Lucide) | plus (Create …, Add level, Add item), refresh-cw (Try again), arrow-right (Go to …), undo-2 (Put rung 5 back), book-open (the card), gauge (menu) |
#### 4 · Classes
| Row | Class | Kind | Why |
| S1 NEW | .user-page.catalogue-record .form-check (and its input and label), below 641 px | Shipped | A lone checkbox row (Active; Open for adoption) is 44 px with a 20 px box on the College's, a speciality's, a sub-speciality's, a scale's and a curriculum's page. Flow 15's .user-page.epa-page rule has the same declarations; the build may write both selectors in one rule. Measured: every checkbox row 44 px at 390. |
| S2 | not written | — | Measured unnecessary: StackWide on the levels table makes each rung a block at 390; the Description textarea is 258 px in a 290 px block, every control 44 px. |
| reuses | .people-page, .epas-page, .curricula-page, .curriculum-items-page, .scales-page, .user-page, .epa-page, .curriculum-item-page, .user-section, .form-section-head, .form-container, .form-grid, .form-actions, .form-group, .form-check, .field-help, .validation-message, .required-mark, .detail-card--empty, .state-panel-title, .state-panel-copy, .loading-line, .skeleton, .skel-*, .action-result, .alert-*, .alert-row, .dialog-card, .dialog-scrim, .dialog-body, .table-container, .clinic-table (--stack, --stack-wide, --inputs, --compact), .col-wrap, .col-wrap--wide, .actions-cell, .btn-*, .muted, .text-sm, .paused-mark, .breadcrumbs, .header-container, .page-subtitle, .dashboard-grid, .detail-card--emphasis, .dashboard-card-title, .dashboard-card-body, .dashboard-card-footer, .visually-hidden | Shipped (app.css) | As built. |
| K1–K9 | .cv-board, .cv-w1280, .cv-w768, .cv-w390, .cv-label*, .cv-chip, .cv-stage, .cv-shell, .cv-main, .cv-topbar, .cv-article, .cv-overlay, .cv-note, .cv-doc, .cv-tag*, .cv-legend | Canvas only | Round 1's, unchanged but .cv-w768 added and the chip wrapping. Kept off the components. |
#### 5 · Measured (Chromium, Playwright, on static copies of these boards with this canvas's tokens.css, bundle.css and f16r2.css)
| Check | Result |
| Page sideways scroll | None on any board at 1280, 1100, 768 or 390 (document scrollWidth = clientWidth on every render). |
| 390 controls | Every .btn, .form-control, .form-select and .form-check in the article and every dialog ≥ 44 px on every 390 board, after the Home card's links moved into DashboardCard's footer (36 px before, as built today). |
| The levels at 390 (S6) | StackWide alone: each rung a block, the textarea 258 px of a 290 px table. S2 not needed. |
| Sweep 900–1100 (table against container) | Colleges overflows by 25 px at 950 (table 667 in a 642 container; Name and Description at their 144 px floors, Created 94, the two row actions 130): from 901 to about 975 px its container scrolls. Fixes for the operator, not drawn (each a rule this round may not add): stack Colleges to 1024 px as the EPA list does, or move Created to the record. The items table overflows by 2 px at 1050 (744 in 742), as built by flow 15 (it stacks from 1024 down). Every other table fits at 900, 950, 1000, 1050 and 1100. |
| Header actions beside the title | They wrap under the subtitle at 1280 on Colleges, the College's EPA list and Curricula, and at 1100 on those and the tree (the subtitle's block takes the row: .header-container wraps). The Administrator's EPA list, with today's short subtitle, keeps Create EPA beside the h1. Fix for the operator, not drawn: the title block flex: 1 1 20rem; min-width: 0 in .header-container, a third shipped rule. |
| Titles beside an empty band | At 1280 no EPA or item title wraps past two lines (the EPA column 45%; the items table compact). |

######## BOARD R2-0-steps.dc.html
### R2-0-steps · flow 16, round 2
The counting rule (N10), used on every row. A journey counts from Home: presses + fields typed or chosen. A menu link is a press; at 390, opening the menu is one more. A dialog's confirm is a press. A field that arrives chosen (the preset sub-speciality, the suggested scale, the only EPA) counts as none. A page the app returns to by itself (the tree after Save, the list after Add item) is no press.
33 steps: part 1's 18 (1.2–1.4, 1.12–1.15, 6.1–6.8, 6.10–6.12) and part 2's 15 (1.16–1.18, 6.13, 6.14, 6.17–6.19, 6.23, 6.24, 6.28–6.31, A.7.10). Round 1's message said "part 1's 19 Expects": it is 18.
#### 1 · Each step's Expect, once built
| Step | Boards | Once built |
| 1.2 | R2-B01 | Two rows (College of Paediatricians of South Africa, CPSA; Demo College, DEMO-C), Active, Edit and Specialities named for the row; Create college. New: StackWide at 390; Created 2026-11-02 (ISO, the SA day); the subtitle and caption. The rest unchanged. |
| 1.3 | R2-B07 | Do: "Leave by Cancel without saving." Expect: the page is "College of Paediatricians of South Africa", tab "… · Wombat" (S11; today "Edit college", T190), subtitled "CPSA · Active", under Home › Colleges › College of Paediatricians of South Africa; Active ticked; Deactivate… (outline) and Save; leaving saves nothing. |
| 1.4 | R2-C01, C02 | "Edit and Delete on each row" → "Edit and Delete… on each row"; the ladder's page "CPSA Paediatric Entrustment Scale v11.1" (T1), Details and Levels as h2, Order beside each label, each descriptor whole; in use, so no Up, Down or Remove: "Rungs can be reordered or removed only while nothing uses this scale." above Add level (Q16). Note: "a CollegeAdmin is admitted to neither page" → "a CollegeAdmin reads the list (Step 1.15), and only an Administrator opens a scale's page". |
| 1.8 (outside the 33) | R2-A01 | The card reads Q12's sentence with Specialities, EPAs, Curricula and Activity types; the sidebar Home, Specialities, EPAs, Curricula, Activity types, Entrustment scales, then My data rights. |
| 1.12 | R2-A02 | The College's tree, subtitled "College of Paediatricians of South Africa (CPSA): its specialities, their sub-specialities and the ladder each uses." (today "College: …"); one section, Paediatrics, "Specialist training in Paediatrics. · Active", Edit and Create sub-speciality; one row, the sub-speciality Paediatrics with CPSA Paediatric Entrustment Scale v11.1; no Back to colleges; Specialities lit; the trail as today; Create speciality. |
| 1.13 | R2-A02 | Route now redirects to the tree at Paediatrics' section (PROPOSED, decided). Do: read the section's table; record {SubSpecialityId} from its Edit link. Expect: one row, Paediatrics, "General paediatric specialist training programme.", CPSA Paediatric Entrustment Scale v11.1, Active, Edit named "Edit the sub-speciality Paediatrics"; no "Speciality: Paediatrics", no Back to specialities. No "General Paediatrics" (unchanged). |
| 1.14 | R2-A05 | The page is "Paediatrics", "Sub-speciality of Paediatrics" (today "Edit sub-speciality" / "Speciality: Paediatrics"); the default, its choices and help unchanged; Active ticked; Deactivate… and Save. |
| 1.15 (D57) | R2-C21a | "The College reads the scales". Route /admin/entrustment-scales. Do: open Entrustment scales from the menu. Expect: the reader's subtitle; two scales (CPSA Paediatric Entrustment Scale v11.1, 6; O-R Scale, 5); no Create scale, no actions column, no link; Entrustment scales lit. A scale's page stays the Administrator's (6.10). |
| 1.16 | R2-E01 | The NEW subtitle and caption; no College column (T3), so "Every row reads College …" goes; Sub-speciality one column, "Paediatrics / Paediatrics" (as built since flow 15: the Expect's "Speciality … and Sub-speciality …" wording is the older three-column list; check it against the build); Active, Edit; Create EPA beside the h1 (it wraps under the subtitle at 1280 as measured: Spec § 5). |
| 1.17 | R2-F01 | The NEW subtitle; the rest unchanged. |
| 1.18 | R2-F05 | "with no Back links" stays; the header offers "Edit this version" (T4); the warning begins "No institution has adopted this version yet." (Q2); the rest unchanged. |
| 6.1 | R2-B04, B06, B02 | Refused under Short code, "CPSA is already the short code of College of Paediatricians of South Africa. Choose another." (today "A college with the same name or short code already exists."); a required message names the label once; the second save opens "College of Neurologists of South Africa", "College created." focused, Active; the list three rows (CNSA, CPSA, Demo College), Active, Edit and Specialities. |
| 6.2 | R2-B08 | "College saved." focused on the record (one press more to read the list); the list shows the new description; "every CPSA page still reads 'College: College of …'" → "the tree's subtitle still reads 'College of Paediatricians of South Africa (CPSA): …' and the record's 'CPSA · Active'". |
| 6.3 | R2-A10–A12 | Before: "No specialities yet" / "Create the first speciality for College of Neurologists of South Africa."; after: the tree, "Speciality Neurology created." focused, one section Neurology, Active, "No sub-specialities yet. …" and Create sub-speciality (no "Manage sub-specialities", no "College: …" line). |
| 6.4 | R2-A13a, A13b | Do: "On the tree, press Create sub-speciality on Neurology and create Adult Neurology …". Expect: the create page offers Default entrustment scale, "No default — offer every scale" chosen, the two existing scales; back on the tree, "Sub-speciality Adult Neurology created." focused, one row, "No default". |
| 6.5 | R2-C03a, C03b, C04 | Refused under the fourth rung's Label (today "Level labels must be unique within a scale."); Add level three times (built: two rungs to start); "Create scale"; the scale's page, "Entrustment scale created." focused, five rungs; the list three, CNSA at 5. |
| 6.6 | R2-C05, C06 | "Entrustment scale saved." focused (stays on the page, built); six rungs, Supervises others at 6, Up, Down and Remove offered (unused); the list 6 (a press more). |
| 6.7 | R2-A14a, A14b | Route: Colleges › CNSA's Specialities › Edit the sub-speciality Adult Neurology. Save returns to the tree, "Sub-speciality saved." focused, the row CNSA Neurology Entrustment Scale; reopened, selected; help unchanged. Its own step (M7). |
| 6.8 | R2-C07, C08 | "Delete CNSA Neurology Entrustment Scale?", opening on Cancel, "Delete scale"; the refusal word for word, at the list's top, focused, with "Open Adult Neurology"; the scale stays at 6. |
| 6.10 | R2-A01 | The card's Q12 sentence and four links (today three, "Institutions adopt an active curriculum"); the menu offers Entrustment scales (the list), neither page typed; each address Access denied (unchanged). |
| 6.11 | R2-A03, A04 | Specialities takes Anton Kruger to CPSA's tree (S9's subtitle, one section); Save returns to it, "Speciality saved." focused, the line the new description; no way to Colleges (unchanged). |
| 6.12 | R2-A06, A07 | Do: "On the tree, press Create sub-speciality on Paediatrics and create Neonatology …, choosing CPSA Paediatric Entrustment Scale v11.1 as its default." Expect: back on the tree, "Sub-speciality Neonatology created." focused; Neonatology and Paediatrics, both CPSA, Active; reopened, selected. |
| 6.13 | R2-D01, D05, F02, F06 | Create curriculum opens Neonatology EPA Curriculum (1.0)'s page with Q5's result focused, Open for adoption unticked with T301's help and "No institution has adopted this version yet.", A new version below; the list Held back, 0 (unchanged); the items page "Items (0)", Q14's words with Go to EPAs (today "Every national EPA of Neonatology …", T326). |
| 6.14 | R2-E05, E06 | Unchanged but the subtitle (NEW), and under EV2 the reach line; "EPA saved."; the h1 the new title. |
| 6.17 | R2-E07, E08, F07 | The question gains "PAED-012 is on 1 curriculum adopted by 1 institution, Kgosi Kgari Teaching Hospital; 3 registrars in training are measured against it." first; 11.1's notice begins "Adopted by 1 institution, Kgosi Kgari Teaching Hospital; 3 registrars in training on it."; the rest unchanged. |
| 6.18, 6.19, 6.24 | — | Unchanged (flows 04, 05, 03). |
| 6.23 | R2-E09 | Unchanged (nothing asks; the result as today); under EV2 the reach line. |
| 6.28 | R2-E10, E11 | The Sub-speciality help (NEW); the subtitle (NEW); under EV2 "PAED-016 is on no curriculum an institution has adopted, so no registrar is measured against it yet."; the rest unchanged. |
| 6.29 | R2-F08, D06–D08, F03 | Route: Curricula › Items (11.1) › Edit this version (T4) › Curricula. 11.1's items with the reach (KGK, 3); its page's reach; "11.1-copy" stays; "with Active unticked" → Q5's clone result focused, Open for adoption unticked; the list 11.2 Held back, 15 (unchanged). |
| 6.30 | R2-F09–F13 | Unchanged but the notice's and the item page's reach ("No institution has adopted …"). |
| 6.31 | R2-D10, D11, F04 | Do "Open 11.2, tick Open for adoption and save." Expect: nothing asks; T7's opening result focused; Hold back… offered; 11.1 and 11.2 Open, 11.2 at 16, Neonatology Held back (unchanged). |
| A.7.10 | R2-F20a–f | Do adds 11.2's own page (T12). Expect adds: "11.2's page sets one field a row, each control and the Open for adoption row 44 px, Save on top of Cancel, nothing sideways"; the reach reads 1 registrar; the rest unchanged. |
| Phase 6.B intro | — | "A CollegeAdmin chooses a sub-speciality's default scale from them" gains "and reads their list". |
| 6.9, 6.32–6.39, A.5.2, A.7.9 | — | Unchanged (flow 15's). |
| states.md | — | The part-1 rows change with the h1s above; the not-found and other-College rows read Unavailable (Q7); the deactivated rows read the dialogs and results. |
#### 2 · The runbook's words that change
Found by the brief's search; each hit's step.
| Words | Steps (file:line) |
| "Back to colleges" | 1.3 Do and Actual (act-1-setup.md:93, :100); 1.12 Actual and Gap (:341–342); 6.11 Actual and Gap (act-6-catalogue.md:243, :249) |
| "Back to specialities" | 1.13 (act-1-setup.md:350, :354); 6.11 Actual (act-6-catalogue.md:246) |
| "Manage sub-specialities" | 6.3 Actual (:110); 6.11 (:246) |
| "Manage items", "Clone as new version" | 6.13 Actual (:283) |
| "Edit college" | 1.3 (act-1-setup.md:95, :98–99); 6.1 Actual (act-6-catalogue.md:73) |
| "Edit speciality" | 6.3 Actual (:110) |
| "Edit sub-speciality" | 1.14 (act-1-setup.md:361, :368) |
| "Edit curriculum" | 6.13, 6.29 Actuals (act-6-catalogue.md:282, :771) |
| "Speciality: Paediatrics" | 1.13, 1.14 (act-1-setup.md:349, :353, :361, :369) |
| "College: College of" | 1.12 (act-1-setup.md:332, :338); 6.2 (act-6-catalogue.md:85); 6.3 (:103, :108); 6.11 (:237) |
| "tick Active" | 6.31 (:818) |
| "Active unticked" | 6.13 (:274, :282); 6.29 (:764, :772); 6.31 Actual (:822); states.md:803, :805 |
| "Level labels must be unique" | 6.5 |
| "National catalogue, with Specialities, EPAs and Curricula" | 1.8 (act-1-setup.md:243, :253); 6.10 (act-6-catalogue.md:219, :225) |
| "a CollegeAdmin is admitted to neither page" | 1.4 Note (act-1-setup.md:115) |
| "Every national EPA of Neonatology" | 6.13 Actual (:287) |
#### 3 · Journeys, presses + fields
1280 / 390; today by the same rule.
| Journey | Built | Today | Presses and fields |
| Read the tree (1.12–1.14) | 1+0 / 2+0 | 3+0 | Specialities |
| A sub-speciality with its default (6.12) | 3+3 / 4+3 | 5+3 | Specialities; Create sub-speciality; Name, Description, Default; Create sub-speciality |
| CNSA's first speciality and sub-speciality (6.3, 6.4) | 6+4 / 7+4 | 7+4 | Colleges; Specialities on CNSA; Create speciality; Name, Description; Create speciality; Create sub-speciality on Neurology; Name, Description; Create sub-speciality |
| Its default (6.7) | 4+1 / 5+1 | 5+1 | Colleges; Specialities on CNSA; Edit the sub-speciality Adult Neurology; the scale; Save (10+5 with 6.3–6.4) |
| A College created after a refusal (6.1) | 4+4 / 5+4 | 4+4 | Colleges; Create college; Name, Short code, Description; Create college; Short code; Create college |
| A description corrected, then the list (6.2) | 4+1 / 5+1 | 4+1 | Colleges; Edit; Description; Save; Colleges |
| A ladder created after a refusal (6.5) | 7+13 / 8+13 | 7+13 | Entrustment scales; Create scale; Name, Description; Add level ×3; 5 labels, 5 descriptions; Create scale; a label; Create scale |
| Its sixth rung (6.6) | 4+2 / 5+2 (+1 to read the list) | 4+2 | Entrustment scales; Edit; Add level; Label, Description; Save |
| Delete asked and refused (6.8) | 3+0 / 4+0 | 2+0 | Entrustment scales; Delete…; Delete scale |
| A curriculum created, then its items (6.13) | 4+2 / 5+2 | 4+2 | Curricula; Create curriculum; Name, Version; Create curriculum; Items (0) |
| An EPA corrected (6.14) | 3+1 / 4+1 | 3+1 | EPAs; Edit PAED-006; Title; Save |
| An EPA paused (6.17) | 4+0 / 5+0 | 4+0 | EPAs; Edit PAED-012; Deactivate…; Deactivate |
| An EPA added (6.28) | 3+4 / 4+4 | 3+4 | EPAs; Create EPA; Sub-speciality, Code, Title, Description; Save |
| A clone (6.29) | 3+2 / 4+2 | 3+2 | Curricula; Edit; New version, Effective from; Clone |
| The items edited (6.30) | 4+6 and 2+1 / +1 | 3+6 and 4+1 | Curricula; Items of 11.2; Add item; 6 fields; Add item · Edit PAED-011; Target; Save item |
| A version opened (6.31) | 3+1 / 4+1 | 3+1 | Curricula; Edit; the box; Save |

######## BOARD R2-A01.dc.html
R2-A01 · 1280
1.8, 6.10 · Anton Kruger · Home: one DashboardCard, Q12's sentence (Q17), four links. The menu holds Entrustment scales after Activity types (D57, M1).
Round 1's second paragraph (who keeps the Colleges and the scales) is gone (M1). The links sit in DashboardCard's footer, so the system's 44 px phone rule reaches them (today 36 px at 390, A.7.10-1).
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] [aria-current="page"] Home
-
[link] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
### Home
College admin · Semester 2, 2026
#### National catalogue
Your College's specialities, EPAs, curricula and activity types. An institution can adopt a curriculum version once you open it.
[link] Specialities
[link] EPAs
[link] Curricula
[link] Activity types

######## BOARD R2-A01-390.dc.html
R2-A01-390 · 390
1.8, 6.10 · Anton Kruger · Home: one DashboardCard, Q12's sentence (Q17), four links. The menu holds Entrustment scales after Activity types (D57, M1).
Round 1's second paragraph (who keeps the Colleges and the scales) is gone (M1). The links sit in DashboardCard's footer, so the system's 44 px phone rule reaches them (today 36 px at 390, A.7.10-1).
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] [aria-current="page"] Home
-
[link] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
### Home
College admin · Semester 2, 2026
#### National catalogue
Your College's specialities, EPAs, curricula and activity types. An institution can adopt a curriculum version once you open it.
[link] Specialities
[link] EPAs
[link] Curricula
[link] Activity types

######## BOARD R2-A02.dc.html
R2-A02 · 1280
1.12, 1.13, 1.14 read at once · Anton Kruger · the College's tree (Q1): one h2 section a speciality, its sub-specialities with their default ladder. No Back to colleges (T291).
Section actions Edit and Create sub-speciality, outline, named "Edit the speciality Paediatrics" and "Create a sub-speciality of Paediatrics"; the row's Edit "Edit the sub-speciality Paediatrics" (M8). StackWide (N5).
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] [aria-current="page"] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Specialities
-
- [aria-current="page"] College of Paediatricians of South Africa
### Specialities
College of Paediatricians of South Africa (CPSA): its specialities, their sub-specialities and the ladder each uses.
[link] Create speciality
#### Paediatrics
[link] [aria-label="Edit the speciality Paediatrics"] Edit
[link] [aria-label="Create a sub-speciality of Paediatrics"] Create sub-speciality
Specialist training in Paediatrics. · Active
Sub-specialities of Paediatrics
| Sub-speciality | Description | Default scale | Status | Actions |
| Paediatrics | General paediatric specialist training programme. | CPSA Paediatric Entrustment Scale v11.1 | Active | [link] [aria-label="Edit the sub-speciality Paediatrics"] Edit |

######## BOARD R2-A02-390.dc.html
R2-A02-390 · 390
1.12, 1.13, 1.14 read at once · Anton Kruger · the College's tree (Q1): one h2 section a speciality, its sub-specialities with their default ladder. No Back to colleges (T291).
Section actions Edit and Create sub-speciality, outline, named "Edit the speciality Paediatrics" and "Create a sub-speciality of Paediatrics"; the row's Edit "Edit the sub-speciality Paediatrics" (M8). StackWide (N5).
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] [aria-current="page"] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Specialities
-
- [aria-current="page"] College of Paediatricians of South Africa
### Specialities
College of Paediatricians of South Africa (CPSA): its specialities, their sub-specialities and the ladder each uses.
[link] Create speciality
#### Paediatrics
[link] [aria-label="Edit the speciality Paediatrics"] Edit
[link] [aria-label="Create a sub-speciality of Paediatrics"] Create sub-speciality
Specialist training in Paediatrics. · Active
Sub-specialities of Paediatrics
| Sub-speciality | Description | Default scale | Status | Actions |
| Paediatrics | General paediatric specialist training programme. | CPSA Paediatric Entrustment Scale v11.1 | Active | [link] [aria-label="Edit the sub-speciality Paediatrics"] Edit |

######## BOARD R2-A02-768.dc.html
R2-A02-768 · 768
1.12, 1.13, 1.14 read at once · Anton Kruger · the College's tree (Q1): one h2 section a speciality, its sub-specialities with their default ladder. No Back to colleges (T291).
Section actions Edit and Create sub-speciality, outline, named "Edit the speciality Paediatrics" and "Create a sub-speciality of Paediatrics"; the row's Edit "Edit the sub-speciality Paediatrics" (M8). StackWide (N5).
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] [aria-current="page"] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Specialities
-
- [aria-current="page"] College of Paediatricians of South Africa
### Specialities
College of Paediatricians of South Africa (CPSA): its specialities, their sub-specialities and the ladder each uses.
[link] Create speciality
#### Paediatrics
[link] [aria-label="Edit the speciality Paediatrics"] Edit
[link] [aria-label="Create a sub-speciality of Paediatrics"] Create sub-speciality
Specialist training in Paediatrics. · Active
Sub-specialities of Paediatrics
| Sub-speciality | Description | Default scale | Status | Actions |
| Paediatrics | General paediatric specialist training programme. | CPSA Paediatric Entrustment Scale v11.1 | Active | [link] [aria-label="Edit the sub-speciality Paediatrics"] Edit |

######## BOARD R2-A03.dc.html
R2-A03 · 1280
6.11 before Save · Anton Kruger · the speciality's record: titled by its name (S11), the kind in the subtitle (M8); the new description typed. Status with D56's plain help (Q15).
Deactivate… outline, only on a record stored as active (D56). No Back link (Q9).
Focus: Description, as typed.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] [aria-current="page"] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Specialities
-
-
[link] College of Paediatricians of South Africa
-
- [aria-current="page"] Paediatrics
### Paediatrics
Speciality of College of Paediatricians of South Africa
Name*
[value="Paediatrics"]
Description
Specialist training in Paediatrics, and certificate training in its sub-specialities.
Status
(checkbox) [checked]
Active
An inactive speciality cannot be named by a new invitation, and an institution can no longer give anyone a role for it. Nothing else reads it: its EPAs and curricula, and the registrars on them, carry on.
[link] Cancel
[button] [aria-haspopup="dialog"] Deactivate…
[button] Save

######## BOARD R2-A03-390.dc.html
R2-A03-390 · 390
6.11 before Save · Anton Kruger · the speciality's record: titled by its name (S11), the kind in the subtitle (M8); the new description typed. Status with D56's plain help (Q15).
Deactivate… outline, only on a record stored as active (D56). No Back link (Q9).
Focus: Description, as typed.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] [aria-current="page"] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Specialities
-
-
[link] College of Paediatricians of South Africa
-
- [aria-current="page"] Paediatrics
### Paediatrics
Speciality of College of Paediatricians of South Africa
Name*
[value="Paediatrics"]
Description
Specialist training in Paediatrics, and certificate training in its sub-specialities.
Status
(checkbox) [checked]
Active
An inactive speciality cannot be named by a new invitation, and an institution can no longer give anyone a role for it. Nothing else reads it: its EPAs and curricula, and the registrars on them, carry on.
[link] Cancel
[button] [aria-haspopup="dialog"] Deactivate…
[button] Save

######## BOARD R2-A04.dc.html
R2-A04 · 1280
6.11 saved · Anton Kruger · Save returns to the tree by a one-shot token (CatalogueLinks): "Speciality saved." focused; the section's line reads the new description.
Focus: "Speciality saved.".
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] [aria-current="page"] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Specialities
-
- [aria-current="page"] College of Paediatricians of South Africa
### Specialities
College of Paediatricians of South Africa (CPSA): its specialities, their sub-specialities and the ladder each uses.
[link] Create speciality
Speciality saved.
#### Paediatrics
[link] [aria-label="Edit the speciality Paediatrics"] Edit
[link] [aria-label="Create a sub-speciality of Paediatrics"] Create sub-speciality
Specialist training in Paediatrics, and certificate training in its sub-specialities. · Active
Sub-specialities of Paediatrics
| Sub-speciality | Description | Default scale | Status | Actions |
| Paediatrics | General paediatric specialist training programme. | CPSA Paediatric Entrustment Scale v11.1 | Active | [link] [aria-label="Edit the sub-speciality Paediatrics"] Edit |

######## BOARD R2-A04-390.dc.html
R2-A04-390 · 390
6.11 saved · Anton Kruger · Save returns to the tree by a one-shot token (CatalogueLinks): "Speciality saved." focused; the section's line reads the new description.
Focus: "Speciality saved.".
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] [aria-current="page"] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Specialities
-
- [aria-current="page"] College of Paediatricians of South Africa
### Specialities
College of Paediatricians of South Africa (CPSA): its specialities, their sub-specialities and the ladder each uses.
[link] Create speciality
Speciality saved.
#### Paediatrics
[link] [aria-label="Edit the speciality Paediatrics"] Edit
[link] [aria-label="Create a sub-speciality of Paediatrics"] Create sub-speciality
Specialist training in Paediatrics, and certificate training in its sub-specialities. · Active
Sub-specialities of Paediatrics
| Sub-speciality | Description | Default scale | Status | Actions |
| Paediatrics | General paediatric specialist training programme. | CPSA Paediatric Entrustment Scale v11.1 | Active | [link] [aria-label="Edit the sub-speciality Paediatrics"] Edit |

######## BOARD R2-A05.dc.html
R2-A05 · 1280
1.14 · Anton Kruger · the sub-speciality's record: "Paediatrics", "Sub-speciality of Paediatrics" (M8). Default entrustment scale CPSA chosen; its help the built sentence alone (M1).
Its tab reads "Paediatrics · Wombat", as the speciality's does: the subtitle tells them apart.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] [aria-current="page"] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Specialities
-
-
[link] College of Paediatricians of South Africa
-
- [aria-current="page"] Paediatrics
### Paediatrics
Sub-speciality of Paediatrics
Name*
[value="Paediatrics"]
Description
General paediatric specialist training programme.
Default entrustment scale
No default — offer every scale
[selected] CPSA Paediatric Entrustment Scale v11.1
O-R Scale
Committee STARs for this programme's trainees are limited to this scale's levels.
Status
(checkbox) [checked]
Active
An inactive sub-speciality cannot be named by a new invitation, and an institution can no longer give anyone a role for it. Nothing else reads it: its EPAs and curricula, and the registrars on them, carry on.
[link] Cancel
[button] [aria-haspopup="dialog"] Deactivate…
[button] Save

######## BOARD R2-A05-390.dc.html
R2-A05-390 · 390
1.14 · Anton Kruger · the sub-speciality's record: "Paediatrics", "Sub-speciality of Paediatrics" (M8). Default entrustment scale CPSA chosen; its help the built sentence alone (M1).
Its tab reads "Paediatrics · Wombat", as the speciality's does: the subtitle tells them apart.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] [aria-current="page"] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Specialities
-
-
[link] College of Paediatricians of South Africa
-
- [aria-current="page"] Paediatrics
### Paediatrics
Sub-speciality of Paediatrics
Name*
[value="Paediatrics"]
Description
General paediatric specialist training programme.
Default entrustment scale
No default — offer every scale
[selected] CPSA Paediatric Entrustment Scale v11.1
O-R Scale
Committee STARs for this programme's trainees are limited to this scale's levels.
Status
(checkbox) [checked]
Active
An inactive sub-speciality cannot be named by a new invitation, and an institution can no longer give anyone a role for it. Nothing else reads it: its EPAs and curricula, and the registrars on them, carry on.
[link] Cancel
[button] [aria-haspopup="dialog"] Deactivate…
[button] Save

######## BOARD R2-A06.dc.html
R2-A06 · 1280
6.12 · Anton Kruger · Create sub-speciality on Paediatrics, filled: the default ladder chosen at create (Q1; CreateSubSpecialityCommand gains DefaultEntrustmentScaleId).
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] [aria-current="page"] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Specialities
-
-
[link] College of Paediatricians of South Africa
-
- [aria-current="page"] Create sub-speciality
### Create sub-speciality
A sub-speciality of Paediatrics, in College of Paediatricians of South Africa.
Name*
[value="Neonatology"]
Description
Certificate training in Neonatology.
Default entrustment scale
No default — offer every scale
[selected] CPSA Paediatric Entrustment Scale v11.1
O-R Scale
Committee STARs for this programme's trainees are limited to this scale's levels.
[link] Cancel
[button] Create sub-speciality

######## BOARD R2-A06-390.dc.html
R2-A06-390 · 390
6.12 · Anton Kruger · Create sub-speciality on Paediatrics, filled: the default ladder chosen at create (Q1; CreateSubSpecialityCommand gains DefaultEntrustmentScaleId).
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] [aria-current="page"] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Specialities
-
-
[link] College of Paediatricians of South Africa
-
- [aria-current="page"] Create sub-speciality
### Create sub-speciality
A sub-speciality of Paediatrics, in College of Paediatricians of South Africa.
Name*
[value="Neonatology"]
Description
Certificate training in Neonatology.
Default entrustment scale
No default — offer every scale
[selected] CPSA Paediatric Entrustment Scale v11.1
O-R Scale
Committee STARs for this programme's trainees are limited to this scale's levels.
[link] Cancel
[button] Create sub-speciality

######## BOARD R2-A07.dc.html
R2-A07 · 1280
6.12 created · Anton Kruger · back on the tree, "Sub-speciality Neonatology created." focused; two rows by name, each CPSA, Active.
Focus: the result.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] [aria-current="page"] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Specialities
-
- [aria-current="page"] College of Paediatricians of South Africa
### Specialities
College of Paediatricians of South Africa (CPSA): its specialities, their sub-specialities and the ladder each uses.
[link] Create speciality
Sub-speciality Neonatology created.
#### Paediatrics
[link] [aria-label="Edit the speciality Paediatrics"] Edit
[link] [aria-label="Create a sub-speciality of Paediatrics"] Create sub-speciality
Specialist training in Paediatrics, and certificate training in its sub-specialities. · Active
Sub-specialities of Paediatrics
| Sub-speciality | Description | Default scale | Status | Actions |
| Neonatology | Certificate training in Neonatology. | CPSA Paediatric Entrustment Scale v11.1 | Active | [link] [aria-label="Edit the sub-speciality Neonatology"] Edit |
| Paediatrics | General paediatric specialist training programme. | CPSA Paediatric Entrustment Scale v11.1 | Active | [link] [aria-label="Edit the sub-speciality Paediatrics"] Edit |

######## BOARD R2-A07-390.dc.html
R2-A07-390 · 390
6.12 created · Anton Kruger · back on the tree, "Sub-speciality Neonatology created." focused; two rows by name, each CPSA, Active.
Focus: the result.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] [aria-current="page"] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Specialities
-
- [aria-current="page"] College of Paediatricians of South Africa
### Specialities
College of Paediatricians of South Africa (CPSA): its specialities, their sub-specialities and the ladder each uses.
[link] Create speciality
Sub-speciality Neonatology created.
#### Paediatrics
[link] [aria-label="Edit the speciality Paediatrics"] Edit
[link] [aria-label="Create a sub-speciality of Paediatrics"] Create sub-speciality
Specialist training in Paediatrics, and certificate training in its sub-specialities. · Active
Sub-specialities of Paediatrics
| Sub-speciality | Description | Default scale | Status | Actions |
| Neonatology | Certificate training in Neonatology. | CPSA Paediatric Entrustment Scale v11.1 | Active | [link] [aria-label="Edit the sub-speciality Neonatology"] Edit |
| Paediatrics | General paediatric specialist training programme. | CPSA Paediatric Entrustment Scale v11.1 | Active | [link] [aria-label="Edit the sub-speciality Paediatrics"] Edit |

######## BOARD R2-A07-768.dc.html
R2-A07-768 · 768
6.12 created · Anton Kruger · back on the tree, "Sub-speciality Neonatology created." focused; two rows by name, each CPSA, Active.
Focus: the result.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] [aria-current="page"] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Specialities
-
- [aria-current="page"] College of Paediatricians of South Africa
### Specialities
College of Paediatricians of South Africa (CPSA): its specialities, their sub-specialities and the ladder each uses.
[link] Create speciality
Sub-speciality Neonatology created.
#### Paediatrics
[link] [aria-label="Edit the speciality Paediatrics"] Edit
[link] [aria-label="Create a sub-speciality of Paediatrics"] Create sub-speciality
Specialist training in Paediatrics, and certificate training in its sub-specialities. · Active
Sub-specialities of Paediatrics
| Sub-speciality | Description | Default scale | Status | Actions |
| Neonatology | Certificate training in Neonatology. | CPSA Paediatric Entrustment Scale v11.1 | Active | [link] [aria-label="Edit the sub-speciality Neonatology"] Edit |
| Paediatrics | General paediatric specialist training programme. | CPSA Paediatric Entrustment Scale v11.1 | Active | [link] [aria-label="Edit the sub-speciality Paediatrics"] Edit |

######## BOARD R2-A08.dc.html
R2-A08 · 1280
Required · Anton Kruger · Create sub-speciality with Name empty: "Enter a name." under Name, once, no summary, focused; the rest kept (T324).
Focus: Name.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] [aria-current="page"] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Specialities
-
-
[link] College of Paediatricians of South Africa
-
- [aria-current="page"] Create sub-speciality
### Create sub-speciality
A sub-speciality of Paediatrics, in College of Paediatricians of South Africa.
Name*
Enter a name.
Description
Certificate training in Neonatology.
Default entrustment scale
No default — offer every scale
[selected] CPSA Paediatric Entrustment Scale v11.1
O-R Scale
Committee STARs for this programme's trainees are limited to this scale's levels.
[link] Cancel
[button] Create sub-speciality

######## BOARD R2-A08-390.dc.html
R2-A08-390 · 390
Required · Anton Kruger · Create sub-speciality with Name empty: "Enter a name." under Name, once, no summary, focused; the rest kept (T324).
Focus: Name.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] [aria-current="page"] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Specialities
-
-
[link] College of Paediatricians of South Africa
-
- [aria-current="page"] Create sub-speciality
### Create sub-speciality
A sub-speciality of Paediatrics, in College of Paediatricians of South Africa.
Name*
Enter a name.
Description
Certificate training in Neonatology.
Default entrustment scale
No default — offer every scale
[selected] CPSA Paediatric Entrustment Scale v11.1
O-R Scale
Committee STARs for this programme's trainees are limited to this scale's levels.
[link] Cancel
[button] Create sub-speciality

######## BOARD R2-A09.dc.html
IllustrationR2-A09 · 1280
A failed save · Anton Kruger · the NEW sentence in a danger Alert, its first sentence bold, focused; the form as typed; the cause to the log (T272, T393).
Focus: the failure.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] [aria-current="page"] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Specialities
-
-
[link] College of Paediatricians of South Africa
-
- [aria-current="page"] Paediatrics
### Paediatrics
Speciality of College of Paediatricians of South Africa
The speciality could not be saved. Nothing has changed. Try again, or come back in a few minutes.
Name*
[value="Paediatrics"]
Description
Specialist training in Paediatrics, and certificate training in its sub-specialities.
Status
(checkbox) [checked]
Active
An inactive speciality cannot be named by a new invitation, and an institution can no longer give anyone a role for it. Nothing else reads it: its EPAs and curricula, and the registrars on them, carry on.
[link] Cancel
[button] [aria-haspopup="dialog"] Deactivate…
[button] Save

######## BOARD R2-A09-390.dc.html
IllustrationR2-A09-390 · 390
A failed save · Anton Kruger · the NEW sentence in a danger Alert, its first sentence bold, focused; the form as typed; the cause to the log (T272, T393).
Focus: the failure.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] [aria-current="page"] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Specialities
-
-
[link] College of Paediatricians of South Africa
-
- [aria-current="page"] Paediatrics
### Paediatrics
Speciality of College of Paediatricians of South Africa
The speciality could not be saved. Nothing has changed. Try again, or come back in a few minutes.
Name*
[value="Paediatrics"]
Description
Specialist training in Paediatrics, and certificate training in its sub-specialities.
Status
(checkbox) [checked]
Active
An inactive speciality cannot be named by a new invitation, and an institution can no longer give anyone a role for it. Nothing else reads it: its EPAs and curricula, and the registrars on them, carry on.
[link] Cancel
[button] [aria-haspopup="dialog"] Deactivate…
[button] Save

######## BOARD R2-A10.dc.html
R2-A10 · 1280
6.3 before · Demo Administrator · the CNSA's tree, empty: the trail through Colleges, Colleges lit. CNSA has no College admin, so the Administrator keeps its tree (edge case).
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] [aria-current="page"] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Colleges
-
-
[link] College of Neurologists of South Africa
-
- [aria-current="page"] Specialities
### Specialities
College of Neurologists of South Africa (CNSA): its specialities, their sub-specialities and the ladder each uses.
[link] Create speciality
No specialities yet
Create the first speciality for College of Neurologists of South Africa.

######## BOARD R2-A10-390.dc.html
R2-A10-390 · 390
6.3 before · Demo Administrator · the CNSA's tree, empty: the trail through Colleges, Colleges lit. CNSA has no College admin, so the Administrator keeps its tree (edge case).
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] [aria-current="page"] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Colleges
-
-
[link] College of Neurologists of South Africa
-
- [aria-current="page"] Specialities
### Specialities
College of Neurologists of South Africa (CNSA): its specialities, their sub-specialities and the ladder each uses.
[link] Create speciality
No specialities yet
Create the first speciality for College of Neurologists of South Africa.

######## BOARD R2-A11.dc.html
R2-A11 · 1280
6.3 · Demo Administrator · Create speciality, filled: Neurology.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] [aria-current="page"] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Colleges
-
-
[link] College of Neurologists of South Africa
-
-
[link] Specialities
-
- [aria-current="page"] Create speciality
### Create speciality
A speciality of College of Neurologists of South Africa.
Name*
[value="Neurology"]
Description
Specialist training in Neurology.
[link] Cancel
[button] Create speciality

######## BOARD R2-A11-390.dc.html
R2-A11-390 · 390
6.3 · Demo Administrator · Create speciality, filled: Neurology.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] [aria-current="page"] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Colleges
-
-
[link] College of Neurologists of South Africa
-
-
[link] Specialities
-
- [aria-current="page"] Create speciality
### Create speciality
A speciality of College of Neurologists of South Africa.
Name*
[value="Neurology"]
Description
Specialist training in Neurology.
[link] Cancel
[button] Create speciality

######## BOARD R2-A12.dc.html
R2-A12 · 1280
6.3 created · Demo Administrator · "Speciality Neurology created." focused; the Neurology section with no sub-speciality yet and its Create sub-speciality.
Focus: the result.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] [aria-current="page"] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Colleges
-
-
[link] College of Neurologists of South Africa
-
- [aria-current="page"] Specialities
### Specialities
College of Neurologists of South Africa (CNSA): its specialities, their sub-specialities and the ladder each uses.
[link] Create speciality
Speciality Neurology created.
#### Neurology
[link] [aria-label="Edit the speciality Neurology"] Edit
[link] [aria-label="Create a sub-speciality of Neurology"] Create sub-speciality
Specialist training in Neurology. · Active
No sub-specialities yet. Create the first sub-speciality of Neurology.

######## BOARD R2-A12-390.dc.html
R2-A12-390 · 390
6.3 created · Demo Administrator · "Speciality Neurology created." focused; the Neurology section with no sub-speciality yet and its Create sub-speciality.
Focus: the result.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] [aria-current="page"] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Colleges
-
-
[link] College of Neurologists of South Africa
-
- [aria-current="page"] Specialities
### Specialities
College of Neurologists of South Africa (CNSA): its specialities, their sub-specialities and the ladder each uses.
[link] Create speciality
Speciality Neurology created.
#### Neurology
[link] [aria-label="Edit the speciality Neurology"] Edit
[link] [aria-label="Create a sub-speciality of Neurology"] Create sub-speciality
Specialist training in Neurology. · Active
No sub-specialities yet. Create the first sub-speciality of Neurology.

######## BOARD R2-A13a.dc.html
R2-A13a · 1280
6.4 · Demo Administrator · Create sub-speciality on Neurology: "No default — offer every scale" kept; the choices are the two scales that exist (the CNSA scale comes at 6.5).
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] [aria-current="page"] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Colleges
-
-
[link] College of Neurologists of South Africa
-
-
[link] Specialities
-
- [aria-current="page"] Create sub-speciality
### Create sub-speciality
A sub-speciality of Neurology, in College of Neurologists of South Africa.
Name*
[value="Adult Neurology"]
Description
Adult neurology specialist training programme.
Default entrustment scale
[selected] No default — offer every scale
CPSA Paediatric Entrustment Scale v11.1
O-R Scale
Committee STARs for this programme's trainees are limited to this scale's levels.
[link] Cancel
[button] Create sub-speciality

######## BOARD R2-A13a-390.dc.html
R2-A13a-390 · 390
6.4 · Demo Administrator · Create sub-speciality on Neurology: "No default — offer every scale" kept; the choices are the two scales that exist (the CNSA scale comes at 6.5).
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] [aria-current="page"] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Colleges
-
-
[link] College of Neurologists of South Africa
-
-
[link] Specialities
-
- [aria-current="page"] Create sub-speciality
### Create sub-speciality
A sub-speciality of Neurology, in College of Neurologists of South Africa.
Name*
[value="Adult Neurology"]
Description
Adult neurology specialist training programme.
Default entrustment scale
[selected] No default — offer every scale
CPSA Paediatric Entrustment Scale v11.1
O-R Scale
Committee STARs for this programme's trainees are limited to this scale's levels.
[link] Cancel
[button] Create sub-speciality

######## BOARD R2-A13b.dc.html
R2-A13b · 1280
6.4 created · Demo Administrator · the tree, "Sub-speciality Adult Neurology created." focused; its row "No default".
Focus: the result.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] [aria-current="page"] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Colleges
-
-
[link] College of Neurologists of South Africa
-
- [aria-current="page"] Specialities
### Specialities
College of Neurologists of South Africa (CNSA): its specialities, their sub-specialities and the ladder each uses.
[link] Create speciality
Sub-speciality Adult Neurology created.
#### Neurology
[link] [aria-label="Edit the speciality Neurology"] Edit
[link] [aria-label="Create a sub-speciality of Neurology"] Create sub-speciality
Specialist training in Neurology. · Active
Sub-specialities of Neurology
| Sub-speciality | Description | Default scale | Status | Actions |
| Adult Neurology | Adult neurology specialist training programme. | No default | Active | [link] [aria-label="Edit the sub-speciality Adult Neurology"] Edit |

######## BOARD R2-A13b-390.dc.html
R2-A13b-390 · 390
6.4 created · Demo Administrator · the tree, "Sub-speciality Adult Neurology created." focused; its row "No default".
Focus: the result.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] [aria-current="page"] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Colleges
-
-
[link] College of Neurologists of South Africa
-
- [aria-current="page"] Specialities
### Specialities
College of Neurologists of South Africa (CNSA): its specialities, their sub-specialities and the ladder each uses.
[link] Create speciality
Sub-speciality Adult Neurology created.
#### Neurology
[link] [aria-label="Edit the speciality Neurology"] Edit
[link] [aria-label="Create a sub-speciality of Neurology"] Create sub-speciality
Specialist training in Neurology. · Active
Sub-specialities of Neurology
| Sub-speciality | Description | Default scale | Status | Actions |
| Adult Neurology | Adult neurology specialist training programme. | No default | Active | [link] [aria-label="Edit the sub-speciality Adult Neurology"] Edit |

######## BOARD R2-A14a.dc.html
R2-A14a · 1280
6.7 · Demo Administrator · Adult Neurology's record, CNSA Neurology Entrustment Scale chosen (the select now lists three scales by name). Still its own step (M7).
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] [aria-current="page"] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Colleges
-
-
[link] College of Neurologists of South Africa
-
-
[link] Specialities
-
- [aria-current="page"] Adult Neurology
### Adult Neurology
Sub-speciality of Neurology
Name*
[value="Adult Neurology"]
Description
Adult neurology specialist training programme.
Default entrustment scale
No default — offer every scale
[selected] CNSA Neurology Entrustment Scale
CPSA Paediatric Entrustment Scale v11.1
O-R Scale
Committee STARs for this programme's trainees are limited to this scale's levels.
Status
(checkbox) [checked]
Active
An inactive sub-speciality cannot be named by a new invitation, and an institution can no longer give anyone a role for it. Nothing else reads it: its EPAs and curricula, and the registrars on them, carry on.
[link] Cancel
[button] [aria-haspopup="dialog"] Deactivate…
[button] Save

######## BOARD R2-A14a-390.dc.html
R2-A14a-390 · 390
6.7 · Demo Administrator · Adult Neurology's record, CNSA Neurology Entrustment Scale chosen (the select now lists three scales by name). Still its own step (M7).
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] [aria-current="page"] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Colleges
-
-
[link] College of Neurologists of South Africa
-
-
[link] Specialities
-
- [aria-current="page"] Adult Neurology
### Adult Neurology
Sub-speciality of Neurology
Name*
[value="Adult Neurology"]
Description
Adult neurology specialist training programme.
Default entrustment scale
No default — offer every scale
[selected] CNSA Neurology Entrustment Scale
CPSA Paediatric Entrustment Scale v11.1
O-R Scale
Committee STARs for this programme's trainees are limited to this scale's levels.
Status
(checkbox) [checked]
Active
An inactive sub-speciality cannot be named by a new invitation, and an institution can no longer give anyone a role for it. Nothing else reads it: its EPAs and curricula, and the registrars on them, carry on.
[link] Cancel
[button] [aria-haspopup="dialog"] Deactivate…
[button] Save

######## BOARD R2-A14b.dc.html
R2-A14b · 1280
6.7 saved · Demo Administrator · the tree, "Sub-speciality saved." focused; the row reads CNSA Neurology Entrustment Scale. Reopened, the scale stays selected (R2-A14a's shape).
Focus: the result.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] [aria-current="page"] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Colleges
-
-
[link] College of Neurologists of South Africa
-
- [aria-current="page"] Specialities
### Specialities
College of Neurologists of South Africa (CNSA): its specialities, their sub-specialities and the ladder each uses.
[link] Create speciality
Sub-speciality saved.
#### Neurology
[link] [aria-label="Edit the speciality Neurology"] Edit
[link] [aria-label="Create a sub-speciality of Neurology"] Create sub-speciality
Specialist training in Neurology. · Active
Sub-specialities of Neurology
| Sub-speciality | Description | Default scale | Status | Actions |
| Adult Neurology | Adult neurology specialist training programme. | CNSA Neurology Entrustment Scale | Active | [link] [aria-label="Edit the sub-speciality Adult Neurology"] Edit |

######## BOARD R2-A14b-390.dc.html
R2-A14b-390 · 390
6.7 saved · Demo Administrator · the tree, "Sub-speciality saved." focused; the row reads CNSA Neurology Entrustment Scale. Reopened, the scale stays selected (R2-A14a's shape).
Focus: the result.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] [aria-current="page"] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Colleges
-
-
[link] College of Neurologists of South Africa
-
- [aria-current="page"] Specialities
### Specialities
College of Neurologists of South Africa (CNSA): its specialities, their sub-specialities and the ladder each uses.
[link] Create speciality
Sub-speciality saved.
#### Neurology
[link] [aria-label="Edit the speciality Neurology"] Edit
[link] [aria-label="Create a sub-speciality of Neurology"] Create sub-speciality
Specialist training in Neurology. · Active
Sub-specialities of Neurology
| Sub-speciality | Description | Default scale | Status | Actions |
| Adult Neurology | Adult neurology specialist training programme. | CNSA Neurology Entrustment Scale | Active | [link] [aria-label="Edit the sub-speciality Adult Neurology"] Edit |

######## BOARD R2-A15.dc.html
IllustrationR2-A15 · 1280
Q3 (D56) · Anton Kruger · Deactivate… on the sub-speciality Neonatology: the dialog names the kind and the record, says only what it does (Q15: no count), opens on Cancel.
Focus: Cancel (autofocus); Cancel or Escape returns to Deactivate….
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] [aria-current="page"] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Specialities
-
-
[link] College of Paediatricians of South Africa
-
- [aria-current="page"] Neonatology
### Neonatology
Sub-speciality of Paediatrics
Name*
[value="Neonatology"]
Description
Certificate training in Neonatology.
Default entrustment scale
No default — offer every scale
[selected] CPSA Paediatric Entrustment Scale v11.1
O-R Scale
Committee STARs for this programme's trainees are limited to this scale's levels.
Status
(checkbox) [checked]
Active
An inactive sub-speciality cannot be named by a new invitation, and an institution can no longer give anyone a role for it. Nothing else reads it: its EPAs and curricula, and the registrars on them, carry on.
[link] Cancel
[button] [aria-haspopup="dialog"] Deactivate…
[button] Save
#### Deactivate the sub-speciality Neonatology?
A new invitation will no longer be able to name it, and an institution can no longer give anyone a role for it. That is all it changes: its EPAs and curricula, and the registrars already on them, carry on.
You can reactivate it on this page.
[button] Cancel
[button] Deactivate

######## BOARD R2-A15-390.dc.html
IllustrationR2-A15-390 · 390
Q3 (D56) · Anton Kruger · Deactivate… on the sub-speciality Neonatology: the dialog names the kind and the record, says only what it does (Q15: no count), opens on Cancel.
Focus: Cancel (autofocus); Cancel or Escape returns to Deactivate….
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] [aria-current="page"] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Specialities
-
-
[link] College of Paediatricians of South Africa
-
- [aria-current="page"] Neonatology
### Neonatology
Sub-speciality of Paediatrics
Name*
[value="Neonatology"]
Description
Certificate training in Neonatology.
Default entrustment scale
No default — offer every scale
[selected] CPSA Paediatric Entrustment Scale v11.1
O-R Scale
Committee STARs for this programme's trainees are limited to this scale's levels.
Status
(checkbox) [checked]
Active
An inactive sub-speciality cannot be named by a new invitation, and an institution can no longer give anyone a role for it. Nothing else reads it: its EPAs and curricula, and the registrars on them, carry on.
[link] Cancel
[button] [aria-haspopup="dialog"] Deactivate…
[button] Save
#### Deactivate the sub-speciality Neonatology?
A new invitation will no longer be able to name it, and an institution can no longer give anyone a role for it. That is all it changes: its EPAs and curricula, and the registrars already on them, carry on.
You can reactivate it on this page.
[button] Cancel
[button] Deactivate

######## BOARD R2-A16a.dc.html
IllustrationR2-A16a · 1280
Deactivated (T5) · Anton Kruger · the record stays: the result focused, Active unticked with the inactive line, Deactivate… gone. Save with the box ticked reactivates, asking nothing.
Focus: the result.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] [aria-current="page"] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Specialities
-
-
[link] College of Paediatricians of South Africa
-
- [aria-current="page"] Neonatology
### Neonatology
Sub-speciality of Paediatrics
Sub-speciality Neonatology deactivated. A new invitation can no longer name it.
Name*
[value="Neonatology"]
Description
Certificate training in Neonatology.
Default entrustment scale
No default — offer every scale
[selected] CPSA Paediatric Entrustment Scale v11.1
O-R Scale
Committee STARs for this programme's trainees are limited to this scale's levels.
Status
(checkbox)
Active
Inactive. A new invitation cannot name it, and no institution can give anyone a role for it. Tick Active and save to reactivate it.
[link] Cancel
[button] Save

######## BOARD R2-A16a-390.dc.html
IllustrationR2-A16a-390 · 390
Deactivated (T5) · Anton Kruger · the record stays: the result focused, Active unticked with the inactive line, Deactivate… gone. Save with the box ticked reactivates, asking nothing.
Focus: the result.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] [aria-current="page"] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Specialities
-
-
[link] College of Paediatricians of South Africa
-
- [aria-current="page"] Neonatology
### Neonatology
Sub-speciality of Paediatrics
Sub-speciality Neonatology deactivated. A new invitation can no longer name it.
Name*
[value="Neonatology"]
Description
Certificate training in Neonatology.
Default entrustment scale
No default — offer every scale
[selected] CPSA Paediatric Entrustment Scale v11.1
O-R Scale
Committee STARs for this programme's trainees are limited to this scale's levels.
Status
(checkbox)
Active
Inactive. A new invitation cannot name it, and no institution can give anyone a role for it. Tick Active and save to reactivate it.
[link] Cancel
[button] Save

######## BOARD R2-A16b.dc.html
IllustrationR2-A16b · 1280
Deactivated · Anton Kruger · the tree: Neonatology's row reads Inactive, in words.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] [aria-current="page"] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Specialities
-
- [aria-current="page"] College of Paediatricians of South Africa
### Specialities
College of Paediatricians of South Africa (CPSA): its specialities, their sub-specialities and the ladder each uses.
[link] Create speciality
#### Paediatrics
[link] [aria-label="Edit the speciality Paediatrics"] Edit
[link] [aria-label="Create a sub-speciality of Paediatrics"] Create sub-speciality
Specialist training in Paediatrics, and certificate training in its sub-specialities. · Active
Sub-specialities of Paediatrics
| Sub-speciality | Description | Default scale | Status | Actions |
| Neonatology | Certificate training in Neonatology. | CPSA Paediatric Entrustment Scale v11.1 | Inactive | [link] [aria-label="Edit the sub-speciality Neonatology"] Edit |
| Paediatrics | General paediatric specialist training programme. | CPSA Paediatric Entrustment Scale v11.1 | Active | [link] [aria-label="Edit the sub-speciality Paediatrics"] Edit |

######## BOARD R2-A16b-390.dc.html
IllustrationR2-A16b-390 · 390
Deactivated · Anton Kruger · the tree: Neonatology's row reads Inactive, in words.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] [aria-current="page"] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Specialities
-
- [aria-current="page"] College of Paediatricians of South Africa
### Specialities
College of Paediatricians of South Africa (CPSA): its specialities, their sub-specialities and the ladder each uses.
[link] Create speciality
#### Paediatrics
[link] [aria-label="Edit the speciality Paediatrics"] Edit
[link] [aria-label="Create a sub-speciality of Paediatrics"] Create sub-speciality
Specialist training in Paediatrics, and certificate training in its sub-specialities. · Active
Sub-specialities of Paediatrics
| Sub-speciality | Description | Default scale | Status | Actions |
| Neonatology | Certificate training in Neonatology. | CPSA Paediatric Entrustment Scale v11.1 | Inactive | [link] [aria-label="Edit the sub-speciality Neonatology"] Edit |
| Paediatrics | General paediatric specialist training programme. | CPSA Paediatric Entrustment Scale v11.1 | Active | [link] [aria-label="Edit the sub-speciality Paediatrics"] Edit |

######## BOARD R2-A17.dc.html
IllustrationR2-A17 · 1280
Q3 · Anton Kruger · the speciality's dialog, "Deactivate the speciality Paediatrics?", the same sentences (one rule, M8).
Focus: Cancel.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] [aria-current="page"] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Specialities
-
-
[link] College of Paediatricians of South Africa
-
- [aria-current="page"] Paediatrics
### Paediatrics
Speciality of College of Paediatricians of South Africa
Name*
[value="Paediatrics"]
Description
Specialist training in Paediatrics, and certificate training in its sub-specialities.
Status
(checkbox) [checked]
Active
An inactive speciality cannot be named by a new invitation, and an institution can no longer give anyone a role for it. Nothing else reads it: its EPAs and curricula, and the registrars on them, carry on.
[link] Cancel
[button] [aria-haspopup="dialog"] Deactivate…
[button] Save
#### Deactivate the speciality Paediatrics?
A new invitation will no longer be able to name it, and an institution can no longer give anyone a role for it. That is all it changes: its EPAs and curricula, and the registrars already on them, carry on.
You can reactivate it on this page.
[button] Cancel
[button] Deactivate

######## BOARD R2-A17-390.dc.html
IllustrationR2-A17-390 · 390
Q3 · Anton Kruger · the speciality's dialog, "Deactivate the speciality Paediatrics?", the same sentences (one rule, M8).
Focus: Cancel.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] [aria-current="page"] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Specialities
-
-
[link] College of Paediatricians of South Africa
-
- [aria-current="page"] Paediatrics
### Paediatrics
Speciality of College of Paediatricians of South Africa
Name*
[value="Paediatrics"]
Description
Specialist training in Paediatrics, and certificate training in its sub-specialities.
Status
(checkbox) [checked]
Active
An inactive speciality cannot be named by a new invitation, and an institution can no longer give anyone a role for it. Nothing else reads it: its EPAs and curricula, and the registrars on them, carry on.
[link] Cancel
[button] [aria-haspopup="dialog"] Deactivate…
[button] Save
#### Deactivate the speciality Paediatrics?
A new invitation will no longer be able to name it, and an institution can no longer give anyone a role for it. That is all it changes: its EPAs and curricula, and the registrars already on them, carry on.
You can reactivate it on this page.
[button] Cancel
[button] Deactivate

######## BOARD R2-A18a.dc.html
R2-A18a · 1280
Q7 · Anton Kruger types the Demo College's specialities address: "College unavailable", in the page's own place, Specialities lit, no id, never which cause; never an empty list offering Create.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] [aria-current="page"] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
- [aria-current="page"] Specialities
### College unavailable
This College does not exist, or you cannot open it.
[link] Go to Specialities

######## BOARD R2-A18a-390.dc.html
R2-A18a-390 · 390
Q7 · Anton Kruger types the Demo College's specialities address: "College unavailable", in the page's own place, Specialities lit, no id, never which cause; never an empty list offering Create.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] [aria-current="page"] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
- [aria-current="page"] Specialities
### College unavailable
This College does not exist, or you cannot open it.
[link] Go to Specialities

######## BOARD R2-A18b.dc.html
R2-A18b · 1280
Q7 · Anton Kruger · a speciality record by an unknown id: "Speciality unavailable".
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] [aria-current="page"] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Specialities
-
- [aria-current="page"] College of Paediatricians of South Africa
### Speciality unavailable
This speciality does not exist, or you cannot open it.
[link] Go to Specialities

######## BOARD R2-A18b-390.dc.html
R2-A18b-390 · 390
Q7 · Anton Kruger · a speciality record by an unknown id: "Speciality unavailable".
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] [aria-current="page"] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Specialities
-
- [aria-current="page"] College of Paediatricians of South Africa
### Speciality unavailable
This speciality does not exist, or you cannot open it.
[link] Go to Specialities

######## BOARD R2-A18c.dc.html
R2-A18c · 1280
Q7 · Anton Kruger types the CNSA's Adult Neurology: "Sub-speciality unavailable" (another College's record reads as one that does not exist).
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] [aria-current="page"] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Specialities
-
- [aria-current="page"] College of Paediatricians of South Africa
### Sub-speciality unavailable
This sub-speciality does not exist, or you cannot open it.
[link] Go to Specialities

######## BOARD R2-A18c-390.dc.html
R2-A18c-390 · 390
Q7 · Anton Kruger types the CNSA's Adult Neurology: "Sub-speciality unavailable" (another College's record reads as one that does not exist).
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] [aria-current="page"] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Specialities
-
- [aria-current="page"] College of Paediatricians of South Africa
### Sub-speciality unavailable
This sub-speciality does not exist, or you cannot open it.
[link] Go to Specialities

######## BOARD R2-A19a.dc.html
R2-A19a · 1280
Loading · the tree: the header at once (Create speciality waits for the read), one loading line, a skeleton; no action.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] [aria-current="page"] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Specialities
-
- [aria-current="page"] College of Paediatricians of South Africa
### Specialities
Loading the specialities.

######## BOARD R2-A19a-390.dc.html
R2-A19a-390 · 390
Loading · the tree: the header at once (Create speciality waits for the read), one loading line, a skeleton; no action.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] [aria-current="page"] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Specialities
-
- [aria-current="page"] College of Paediatricians of South Africa
### Specialities
Loading the specialities.

######## BOARD R2-A19b.dc.html
R2-A19b · 1280
Loading · a record: titled by its kind until the read is in (flow 15's T7).
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] [aria-current="page"] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Specialities
-
- [aria-current="page"] College of Paediatricians of South Africa
### Sub-speciality
Loading this sub-speciality.

######## BOARD R2-A19b-390.dc.html
R2-A19b-390 · 390
Loading · a record: titled by its kind until the read is in (flow 15's T7).
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] [aria-current="page"] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Specialities
-
- [aria-current="page"] College of Paediatricians of South Africa
### Sub-speciality
Loading this sub-speciality.

######## BOARD R2-A20a.dc.html
R2-A20a · 1280
Load error · the tree: the fixed sentence with Try again; no empty state under it, no exception's text (T329, T393).
Focus: Try again moves the focus to the reloaded page's h1.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] [aria-current="page"] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Specialities
-
- [aria-current="page"] College of Paediatricians of South Africa
### Specialities
Could not load the specialities. Nothing has changed. Try again, or come back in a few minutes.
[button] Try again

######## BOARD R2-A20a-390.dc.html
R2-A20a-390 · 390
Load error · the tree: the fixed sentence with Try again; no empty state under it, no exception's text (T329, T393).
Focus: Try again moves the focus to the reloaded page's h1.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] [aria-current="page"] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Specialities
-
- [aria-current="page"] College of Paediatricians of South Africa
### Specialities
Could not load the specialities. Nothing has changed. Try again, or come back in a few minutes.
[button] Try again

######## BOARD R2-A20b.dc.html
R2-A20b · 1280
Load error · a record.
Focus: as R2-A20a.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] [aria-current="page"] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Specialities
-
- [aria-current="page"] College of Paediatricians of South Africa
### Speciality
Could not load this speciality. Nothing has changed. Try again, or come back in a few minutes.
[button] Try again

######## BOARD R2-A20b-390.dc.html
R2-A20b-390 · 390
Load error · a record.
Focus: as R2-A20a.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] [aria-current="page"] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Specialities
-
- [aria-current="page"] College of Paediatricians of South Africa
### Speciality
Could not load this speciality. Nothing has changed. Try again, or come back in a few minutes.
[button] Try again

######## BOARD R2-A21.dc.html
IllustrationR2-A21 · 1280
A College admin whose account names no College · words in place of today's redirect to Colleges (which refuses the College admin): no list, no Create; Specialities lit.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] [aria-current="page"] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
- [aria-current="page"] Specialities
### Specialities
No College on your account
Your account names no College. Ask the platform administrator to add it.

######## BOARD R2-A21-390.dc.html
IllustrationR2-A21-390 · 390
A College admin whose account names no College · words in place of today's redirect to Colleges (which refuses the College admin): no list, no Create; Specialities lit.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] [aria-current="page"] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
- [aria-current="page"] Specialities
### Specialities
No College on your account
Your account names no College. Ask the platform administrator to add it.

######## BOARD R2-A22.dc.html
IllustrationR2-A22 · 1280
Heavy · six sub-specialities under one speciality, a second speciality, a 2,000-character description wrapping in its line and its cell (.col-wrap): nothing sideways.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] [aria-current="page"] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Specialities
-
- [aria-current="page"] College of Paediatricians of South Africa
### Specialities
College of Paediatricians of South Africa (CPSA): its specialities, their sub-specialities and the ladder each uses.
[link] Create speciality
#### Paediatrics
[link] [aria-label="Edit the speciality Paediatrics"] Edit
[link] [aria-label="Create a sub-speciality of Paediatrics"] Create sub-speciality
Specialist training in Paediatrics, and certificate training in its sub-specialities. · Active
Sub-specialities of Paediatrics
| Sub-speciality | Description | Default scale | Status | Actions |
| Neonatology | Certificate training in Neonatology. | CPSA Paediatric Entrustment Scale v11.1 | Active | [link] [aria-label="Edit the sub-speciality Neonatology"] Edit |
| Paediatrics | General paediatric specialist training programme. | CPSA Paediatric Entrustment Scale v11.1 | Active | [link] [aria-label="Edit the sub-speciality Paediatrics"] Edit |
| ‹sub-speciality 3› | ‹description› | CPSA Paediatric Entrustment Scale v11.1 | Active | [link] [aria-label="Edit the sub-speciality ‹sub-speciality 3›"] Edit |
| ‹sub-speciality 4› | ‹description› | CPSA Paediatric Entrustment Scale v11.1 | Active | [link] [aria-label="Edit the sub-speciality ‹sub-speciality 4›"] Edit |
| ‹sub-speciality 5› | ‹description› | CPSA Paediatric Entrustment Scale v11.1 | Active | [link] [aria-label="Edit the sub-speciality ‹sub-speciality 5›"] Edit |
| ‹sub-speciality 6› | ‹description› | CPSA Paediatric Entrustment Scale v11.1 | Active | [link] [aria-label="Edit the sub-speciality ‹sub-speciality 6›"] Edit |
#### ‹second speciality›
[link] [aria-label="Edit the speciality ‹second speciality›"] Edit
[link] [aria-label="Create a sub-speciality of ‹second speciality›"] Create sub-speciality
‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› · Active
Sub-specialities of ‹second speciality›
| Sub-speciality | Description | Default scale | Status | Actions |
| ‹sub-speciality› | ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› | No default | Active | [link] [aria-label="Edit the sub-speciality ‹sub-speciality›"] Edit |

######## BOARD R2-A22-390.dc.html
IllustrationR2-A22-390 · 390
Heavy · six sub-specialities under one speciality, a second speciality, a 2,000-character description wrapping in its line and its cell (.col-wrap): nothing sideways.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] [aria-current="page"] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Specialities
-
- [aria-current="page"] College of Paediatricians of South Africa
### Specialities
College of Paediatricians of South Africa (CPSA): its specialities, their sub-specialities and the ladder each uses.
[link] Create speciality
#### Paediatrics
[link] [aria-label="Edit the speciality Paediatrics"] Edit
[link] [aria-label="Create a sub-speciality of Paediatrics"] Create sub-speciality
Specialist training in Paediatrics, and certificate training in its sub-specialities. · Active
Sub-specialities of Paediatrics
| Sub-speciality | Description | Default scale | Status | Actions |
| Neonatology | Certificate training in Neonatology. | CPSA Paediatric Entrustment Scale v11.1 | Active | [link] [aria-label="Edit the sub-speciality Neonatology"] Edit |
| Paediatrics | General paediatric specialist training programme. | CPSA Paediatric Entrustment Scale v11.1 | Active | [link] [aria-label="Edit the sub-speciality Paediatrics"] Edit |
| ‹sub-speciality 3› | ‹description› | CPSA Paediatric Entrustment Scale v11.1 | Active | [link] [aria-label="Edit the sub-speciality ‹sub-speciality 3›"] Edit |
| ‹sub-speciality 4› | ‹description› | CPSA Paediatric Entrustment Scale v11.1 | Active | [link] [aria-label="Edit the sub-speciality ‹sub-speciality 4›"] Edit |
| ‹sub-speciality 5› | ‹description› | CPSA Paediatric Entrustment Scale v11.1 | Active | [link] [aria-label="Edit the sub-speciality ‹sub-speciality 5›"] Edit |
| ‹sub-speciality 6› | ‹description› | CPSA Paediatric Entrustment Scale v11.1 | Active | [link] [aria-label="Edit the sub-speciality ‹sub-speciality 6›"] Edit |
#### ‹second speciality›
[link] [aria-label="Edit the speciality ‹second speciality›"] Edit
[link] [aria-label="Create a sub-speciality of ‹second speciality›"] Create sub-speciality
‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› · Active
Sub-specialities of ‹second speciality›
| Sub-speciality | Description | Default scale | Status | Actions |
| ‹sub-speciality› | ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› | No default | Active | [link] [aria-label="Edit the sub-speciality ‹sub-speciality›"] Edit |

######## BOARD R2-A22-768.dc.html
IllustrationR2-A22-768 · 768
Heavy · six sub-specialities under one speciality, a second speciality, a 2,000-character description wrapping in its line and its cell (.col-wrap): nothing sideways.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] [aria-current="page"] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Specialities
-
- [aria-current="page"] College of Paediatricians of South Africa
### Specialities
College of Paediatricians of South Africa (CPSA): its specialities, their sub-specialities and the ladder each uses.
[link] Create speciality
#### Paediatrics
[link] [aria-label="Edit the speciality Paediatrics"] Edit
[link] [aria-label="Create a sub-speciality of Paediatrics"] Create sub-speciality
Specialist training in Paediatrics, and certificate training in its sub-specialities. · Active
Sub-specialities of Paediatrics
| Sub-speciality | Description | Default scale | Status | Actions |
| Neonatology | Certificate training in Neonatology. | CPSA Paediatric Entrustment Scale v11.1 | Active | [link] [aria-label="Edit the sub-speciality Neonatology"] Edit |
| Paediatrics | General paediatric specialist training programme. | CPSA Paediatric Entrustment Scale v11.1 | Active | [link] [aria-label="Edit the sub-speciality Paediatrics"] Edit |
| ‹sub-speciality 3› | ‹description› | CPSA Paediatric Entrustment Scale v11.1 | Active | [link] [aria-label="Edit the sub-speciality ‹sub-speciality 3›"] Edit |
| ‹sub-speciality 4› | ‹description› | CPSA Paediatric Entrustment Scale v11.1 | Active | [link] [aria-label="Edit the sub-speciality ‹sub-speciality 4›"] Edit |
| ‹sub-speciality 5› | ‹description› | CPSA Paediatric Entrustment Scale v11.1 | Active | [link] [aria-label="Edit the sub-speciality ‹sub-speciality 5›"] Edit |
| ‹sub-speciality 6› | ‹description› | CPSA Paediatric Entrustment Scale v11.1 | Active | [link] [aria-label="Edit the sub-speciality ‹sub-speciality 6›"] Edit |
#### ‹second speciality›
[link] [aria-label="Edit the speciality ‹second speciality›"] Edit
[link] [aria-label="Create a sub-speciality of ‹second speciality›"] Create sub-speciality
‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› · Active
Sub-specialities of ‹second speciality›
| Sub-speciality | Description | Default scale | Status | Actions |
| ‹sub-speciality› | ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› | No default | Active | [link] [aria-label="Edit the sub-speciality ‹sub-speciality›"] Edit |

######## BOARD R2-B01.dc.html
R2-B01 · 1280
1.2 · Demo Administrator · two Colleges by name, StackWide (NEW on this list), Created the South African day, ISO (N2: today "2026-11-02 16:26", server time). Edit and Specialities named for the row.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] [aria-current="page"] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
### Colleges
The national Colleges. Each College's admin keeps its specialities, EPAs and curricula; you keep the Colleges themselves.
[link] Create college
Every College
| Name | Short code | Description | Status | Created | Actions |
| College of Paediatricians of South Africa | CPSA | Constituent College of the Colleges of Medicine of South Africa; owns the national Paediatric EPA catalogue. | Active | 2026-11-02 | [link] [aria-label="Edit College of Paediatricians of South Africa"] Edit [link] [aria-label="Specialities of College of Paediatricians of South Africa"] Specialities |
| Demo College | DEMO-C | Seeded demo college (national catalogue owner). | Active | 2026-11-02 | [link] [aria-label="Edit Demo College"] Edit [link] [aria-label="Specialities of Demo College"] Specialities |

######## BOARD R2-B01-390.dc.html
R2-B01-390 · 390
1.2 · Demo Administrator · two Colleges by name, StackWide (NEW on this list), Created the South African day, ISO (N2: today "2026-11-02 16:26", server time). Edit and Specialities named for the row.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] [aria-current="page"] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
### Colleges
The national Colleges. Each College's admin keeps its specialities, EPAs and curricula; you keep the Colleges themselves.
[link] Create college
Every College
| Name | Short code | Description | Status | Created | Actions |
| College of Paediatricians of South Africa | CPSA | Constituent College of the Colleges of Medicine of South Africa; owns the national Paediatric EPA catalogue. | Active | 2026-11-02 | [link] [aria-label="Edit College of Paediatricians of South Africa"] Edit [link] [aria-label="Specialities of College of Paediatricians of South Africa"] Specialities |
| Demo College | DEMO-C | Seeded demo college (national catalogue owner). | Active | 2026-11-02 | [link] [aria-label="Edit Demo College"] Edit [link] [aria-label="Specialities of Demo College"] Specialities |

######## BOARD R2-B01-768.dc.html
R2-B01-768 · 768
1.2 · Demo Administrator · two Colleges by name, StackWide (NEW on this list), Created the South African day, ISO (N2: today "2026-11-02 16:26", server time). Edit and Specialities named for the row.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] [aria-current="page"] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
### Colleges
The national Colleges. Each College's admin keeps its specialities, EPAs and curricula; you keep the Colleges themselves.
[link] Create college
Every College
| Name | Short code | Description | Status | Created | Actions |
| College of Paediatricians of South Africa | CPSA | Constituent College of the Colleges of Medicine of South Africa; owns the national Paediatric EPA catalogue. | Active | 2026-11-02 | [link] [aria-label="Edit College of Paediatricians of South Africa"] Edit [link] [aria-label="Specialities of College of Paediatricians of South Africa"] Specialities |
| Demo College | DEMO-C | Seeded demo college (national catalogue owner). | Active | 2026-11-02 | [link] [aria-label="Edit Demo College"] Edit [link] [aria-label="Specialities of Demo College"] Specialities |

######## BOARD R2-B02.dc.html
R2-B02 · 1280
6.1 done · Demo Administrator · three Colleges: CNSA, CPSA, Demo College, each Active.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] [aria-current="page"] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
### Colleges
The national Colleges. Each College's admin keeps its specialities, EPAs and curricula; you keep the Colleges themselves.
[link] Create college
Every College
| Name | Short code | Description | Status | Created | Actions |
| College of Neurologists of South Africa | CNSA | Constituent College of the CMSA; owns the national Neurology catalogue. | Active | 2026-11-02 | [link] [aria-label="Edit College of Neurologists of South Africa"] Edit [link] [aria-label="Specialities of College of Neurologists of South Africa"] Specialities |
| College of Paediatricians of South Africa | CPSA | Constituent College of the Colleges of Medicine of South Africa; owns the national Paediatric EPA catalogue. | Active | 2026-11-02 | [link] [aria-label="Edit College of Paediatricians of South Africa"] Edit [link] [aria-label="Specialities of College of Paediatricians of South Africa"] Specialities |
| Demo College | DEMO-C | Seeded demo college (national catalogue owner). | Active | 2026-11-02 | [link] [aria-label="Edit Demo College"] Edit [link] [aria-label="Specialities of Demo College"] Specialities |

######## BOARD R2-B02-390.dc.html
R2-B02-390 · 390
6.1 done · Demo Administrator · three Colleges: CNSA, CPSA, Demo College, each Active.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] [aria-current="page"] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
### Colleges
The national Colleges. Each College's admin keeps its specialities, EPAs and curricula; you keep the Colleges themselves.
[link] Create college
Every College
| Name | Short code | Description | Status | Created | Actions |
| College of Neurologists of South Africa | CNSA | Constituent College of the CMSA; owns the national Neurology catalogue. | Active | 2026-11-02 | [link] [aria-label="Edit College of Neurologists of South Africa"] Edit [link] [aria-label="Specialities of College of Neurologists of South Africa"] Specialities |
| College of Paediatricians of South Africa | CPSA | Constituent College of the Colleges of Medicine of South Africa; owns the national Paediatric EPA catalogue. | Active | 2026-11-02 | [link] [aria-label="Edit College of Paediatricians of South Africa"] Edit [link] [aria-label="Specialities of College of Paediatricians of South Africa"] Specialities |
| Demo College | DEMO-C | Seeded demo college (national catalogue owner). | Active | 2026-11-02 | [link] [aria-label="Edit Demo College"] Edit [link] [aria-label="Specialities of Demo College"] Specialities |

######## BOARD R2-B02-768.dc.html
R2-B02-768 · 768
6.1 done · Demo Administrator · three Colleges: CNSA, CPSA, Demo College, each Active.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] [aria-current="page"] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
### Colleges
The national Colleges. Each College's admin keeps its specialities, EPAs and curricula; you keep the Colleges themselves.
[link] Create college
Every College
| Name | Short code | Description | Status | Created | Actions |
| College of Neurologists of South Africa | CNSA | Constituent College of the CMSA; owns the national Neurology catalogue. | Active | 2026-11-02 | [link] [aria-label="Edit College of Neurologists of South Africa"] Edit [link] [aria-label="Specialities of College of Neurologists of South Africa"] Specialities |
| College of Paediatricians of South Africa | CPSA | Constituent College of the Colleges of Medicine of South Africa; owns the national Paediatric EPA catalogue. | Active | 2026-11-02 | [link] [aria-label="Edit College of Paediatricians of South Africa"] Edit [link] [aria-label="Specialities of College of Paediatricians of South Africa"] Specialities |
| Demo College | DEMO-C | Seeded demo college (national catalogue owner). | Active | 2026-11-02 | [link] [aria-label="Edit Demo College"] Edit [link] [aria-label="Specialities of Demo College"] Specialities |

######## BOARD R2-B03.dc.html
R2-B03 · 1280
Required · Demo Administrator · "Enter a name." and "Enter a short code.", each once under its field, the label's words, no summary (T324; today twice, naming the property, college-edit--invalid).
Focus: Name, the first refused field.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] [aria-current="page"] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Colleges
-
- [aria-current="page"] Create college
### Create college
A national College keeps its own specialities, EPAs and curricula. It starts active.
Name*
Enter a name.
Short code*
Enter a short code.
Description
[link] Cancel
[button] Create college

######## BOARD R2-B03-390.dc.html
R2-B03-390 · 390
Required · Demo Administrator · "Enter a name." and "Enter a short code.", each once under its field, the label's words, no summary (T324; today twice, naming the property, college-edit--invalid).
Focus: Name, the first refused field.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] [aria-current="page"] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Colleges
-
- [aria-current="page"] Create college
### Create college
A national College keeps its own specialities, EPAs and curricula. It starts active.
Name*
Enter a name.
Short code*
Enter a short code.
Description
[link] Cancel
[button] Create college

######## BOARD R2-B04.dc.html
R2-B04 · 1280
6.1 refused · Demo Administrator · under Short code, naming the holder (S10; DATA: read by a check before the save). Nothing created; everything typed kept.
Focus: Short code.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] [aria-current="page"] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Colleges
-
- [aria-current="page"] Create college
### Create college
A national College keeps its own specialities, EPAs and curricula. It starts active.
Name*
[value="College of Neurologists of South Africa"]
Short code*
[value="CPSA"]
CPSA is already the short code of College of Paediatricians of South Africa. Choose another.
Description
Constituent College of the CMSA; owns the national Neurology catalogue.
[link] Cancel
[button] Create college

######## BOARD R2-B04-390.dc.html
R2-B04-390 · 390
6.1 refused · Demo Administrator · under Short code, naming the holder (S10; DATA: read by a check before the save). Nothing created; everything typed kept.
Focus: Short code.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] [aria-current="page"] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Colleges
-
- [aria-current="page"] Create college
### Create college
A national College keeps its own specialities, EPAs and curricula. It starts active.
Name*
[value="College of Neurologists of South Africa"]
Short code*
[value="CPSA"]
CPSA is already the short code of College of Paediatricians of South Africa. Choose another.
Description
Constituent College of the CMSA; owns the national Neurology catalogue.
[link] Cancel
[button] Create college

######## BOARD R2-B05.dc.html
IllustrationR2-B05 · 1280
A taken name · Demo Administrator · under Name.
Focus: Name.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] [aria-current="page"] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Colleges
-
- [aria-current="page"] Create college
### Create college
A national College keeps its own specialities, EPAs and curricula. It starts active.
Name*
[value="College of Paediatricians of South Africa"]
College of Paediatricians of South Africa is already a College's name. Choose another.
Short code*
[value="‹code›"]
Description
[link] Cancel
[button] Create college

######## BOARD R2-B05-390.dc.html
IllustrationR2-B05-390 · 390
A taken name · Demo Administrator · under Name.
Focus: Name.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] [aria-current="page"] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Colleges
-
- [aria-current="page"] Create college
### Create college
A national College keeps its own specialities, EPAs and curricula. It starts active.
Name*
[value="College of Paediatricians of South Africa"]
College of Paediatricians of South Africa is already a College's name. Choose another.
Short code*
[value="‹code›"]
Description
[link] Cancel
[button] Create college

######## BOARD R2-B06.dc.html
R2-B06 · 1280
6.1 created · Demo Administrator · CNSA's record by a one-shot token: titled by its name (S11), "CNSA · Active", "College created." focused.
Focus: the result.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] [aria-current="page"] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Colleges
-
- [aria-current="page"] College of Neurologists of South Africa
### College of Neurologists of South Africa
CNSA · Active
College created.
Name*
[value="College of Neurologists of South Africa"]
Short code*
[value="CNSA"]
Description
Constituent College of the CMSA; owns the national Neurology catalogue.
Status
(checkbox) [checked]
Active
An inactive College cannot be named by a new invitation. Nothing else reads it: its specialities, EPAs and curricula, and every institution that has adopted one of its curricula, carry on as they are.
[link] Cancel
[button] [aria-haspopup="dialog"] Deactivate…
[button] Save

######## BOARD R2-B06-390.dc.html
R2-B06-390 · 390
6.1 created · Demo Administrator · CNSA's record by a one-shot token: titled by its name (S11), "CNSA · Active", "College created." focused.
Focus: the result.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] [aria-current="page"] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Colleges
-
- [aria-current="page"] College of Neurologists of South Africa
### College of Neurologists of South Africa
CNSA · Active
College created.
Name*
[value="College of Neurologists of South Africa"]
Short code*
[value="CNSA"]
Description
Constituent College of the CMSA; owns the national Neurology catalogue.
Status
(checkbox) [checked]
Active
An inactive College cannot be named by a new invitation. Nothing else reads it: its specialities, EPAs and curricula, and every institution that has adopted one of its curricula, carry on as they are.
[link] Cancel
[button] [aria-haspopup="dialog"] Deactivate…
[button] Save

######## BOARD R2-B07.dc.html
R2-B07 · 1280
1.3 · Demo Administrator · CPSA's record: no result, no Back to colleges (the trail is the way back, Q9). Tab "College of Paediatricians of South Africa · Wombat".
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] [aria-current="page"] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Colleges
-
- [aria-current="page"] College of Paediatricians of South Africa
### College of Paediatricians of South Africa
CPSA · Active
Name*
[value="College of Paediatricians of South Africa"]
Short code*
[value="CPSA"]
Description
Constituent College of the Colleges of Medicine of South Africa; owns the national Paediatric EPA catalogue.
Status
(checkbox) [checked]
Active
An inactive College cannot be named by a new invitation. Nothing else reads it: its specialities, EPAs and curricula, and every institution that has adopted one of its curricula, carry on as they are.
[link] Cancel
[button] [aria-haspopup="dialog"] Deactivate…
[button] Save

######## BOARD R2-B07-390.dc.html
R2-B07-390 · 390
1.3 · Demo Administrator · CPSA's record: no result, no Back to colleges (the trail is the way back, Q9). Tab "College of Paediatricians of South Africa · Wombat".
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] [aria-current="page"] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Colleges
-
- [aria-current="page"] College of Paediatricians of South Africa
### College of Paediatricians of South Africa
CPSA · Active
Name*
[value="College of Paediatricians of South Africa"]
Short code*
[value="CPSA"]
Description
Constituent College of the Colleges of Medicine of South Africa; owns the national Paediatric EPA catalogue.
Status
(checkbox) [checked]
Active
An inactive College cannot be named by a new invitation. Nothing else reads it: its specialities, EPAs and curricula, and every institution that has adopted one of its curricula, carry on as they are.
[link] Cancel
[button] [aria-haspopup="dialog"] Deactivate…
[button] Save

######## BOARD R2-B08.dc.html
R2-B08 · 1280
6.2 saved · Demo Administrator · "College saved." focused, the new description; the save stays on the record.
Focus: the result.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] [aria-current="page"] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Colleges
-
- [aria-current="page"] College of Paediatricians of South Africa
### College of Paediatricians of South Africa
CPSA · Active
College saved.
Name*
[value="College of Paediatricians of South Africa"]
Short code*
[value="CPSA"]
Description
Constituent College of the Colleges of Medicine of South Africa; owns the national Paediatric EPA catalogue and its versions.
Status
(checkbox) [checked]
Active
An inactive College cannot be named by a new invitation. Nothing else reads it: its specialities, EPAs and curricula, and every institution that has adopted one of its curricula, carry on as they are.
[link] Cancel
[button] [aria-haspopup="dialog"] Deactivate…
[button] Save

######## BOARD R2-B08-390.dc.html
R2-B08-390 · 390
6.2 saved · Demo Administrator · "College saved." focused, the new description; the save stays on the record.
Focus: the result.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] [aria-current="page"] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Colleges
-
- [aria-current="page"] College of Paediatricians of South Africa
### College of Paediatricians of South Africa
CPSA · Active
College saved.
Name*
[value="College of Paediatricians of South Africa"]
Short code*
[value="CPSA"]
Description
Constituent College of the Colleges of Medicine of South Africa; owns the national Paediatric EPA catalogue and its versions.
Status
(checkbox) [checked]
Active
An inactive College cannot be named by a new invitation. Nothing else reads it: its specialities, EPAs and curricula, and every institution that has adopted one of its curricula, carry on as they are.
[link] Cancel
[button] [aria-haspopup="dialog"] Deactivate…
[button] Save

######## BOARD R2-B09.dc.html
IllustrationR2-B09 · 1280
Q3 (D56) · Demo Administrator · Deactivate… on CNSA: the ConfirmDialog in action mode, opening on Cancel.
Focus: Cancel; Cancel or Escape returns to Deactivate….
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] [aria-current="page"] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Colleges
-
- [aria-current="page"] College of Neurologists of South Africa
### College of Neurologists of South Africa
CNSA · Active
Name*
[value="College of Neurologists of South Africa"]
Short code*
[value="CNSA"]
Description
Constituent College of the CMSA; owns the national Neurology catalogue.
Status
(checkbox) [checked]
Active
An inactive College cannot be named by a new invitation. Nothing else reads it: its specialities, EPAs and curricula, and every institution that has adopted one of its curricula, carry on as they are.
[link] Cancel
[button] [aria-haspopup="dialog"] Deactivate…
[button] Save
#### Deactivate College of Neurologists of South Africa?
A new invitation will no longer be able to name it. That is all it changes: its specialities, EPAs and curricula stay as they are, every institution that has adopted one of its curricula carries on, and everyone already invited keeps their invitation.
You can reactivate it on this page.
[button] Cancel
[button] Deactivate

######## BOARD R2-B09-390.dc.html
IllustrationR2-B09-390 · 390
Q3 (D56) · Demo Administrator · Deactivate… on CNSA: the ConfirmDialog in action mode, opening on Cancel.
Focus: Cancel; Cancel or Escape returns to Deactivate….
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] [aria-current="page"] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Colleges
-
- [aria-current="page"] College of Neurologists of South Africa
### College of Neurologists of South Africa
CNSA · Active
Name*
[value="College of Neurologists of South Africa"]
Short code*
[value="CNSA"]
Description
Constituent College of the CMSA; owns the national Neurology catalogue.
Status
(checkbox) [checked]
Active
An inactive College cannot be named by a new invitation. Nothing else reads it: its specialities, EPAs and curricula, and every institution that has adopted one of its curricula, carry on as they are.
[link] Cancel
[button] [aria-haspopup="dialog"] Deactivate…
[button] Save
#### Deactivate College of Neurologists of South Africa?
A new invitation will no longer be able to name it. That is all it changes: its specialities, EPAs and curricula stay as they are, every institution that has adopted one of its curricula carries on, and everyone already invited keeps their invitation.
You can reactivate it on this page.
[button] Cancel
[button] Deactivate

######## BOARD R2-B10.dc.html
IllustrationR2-B10 · 1280
D56 · Demo Administrator · Save with Active unticked asks the same question (the box unticked, unsaved, under the dialog).
Focus: Cancel; Cancel leaves the box unticked and unsaved, and Save asks again.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] [aria-current="page"] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Colleges
-
- [aria-current="page"] College of Neurologists of South Africa
### College of Neurologists of South Africa
CNSA · Active
Name*
[value="College of Neurologists of South Africa"]
Short code*
[value="CNSA"]
Description
Constituent College of the CMSA; owns the national Neurology catalogue.
Status
(checkbox)
Active
An inactive College cannot be named by a new invitation. Nothing else reads it: its specialities, EPAs and curricula, and every institution that has adopted one of its curricula, carry on as they are.
[link] Cancel
[button] [aria-haspopup="dialog"] Deactivate…
[button] Save
#### Deactivate College of Neurologists of South Africa?
A new invitation will no longer be able to name it. That is all it changes: its specialities, EPAs and curricula stay as they are, every institution that has adopted one of its curricula carries on, and everyone already invited keeps their invitation.
You can reactivate it on this page.
[button] Cancel
[button] Deactivate

######## BOARD R2-B10-390.dc.html
IllustrationR2-B10-390 · 390
D56 · Demo Administrator · Save with Active unticked asks the same question (the box unticked, unsaved, under the dialog).
Focus: Cancel; Cancel leaves the box unticked and unsaved, and Save asks again.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] [aria-current="page"] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Colleges
-
- [aria-current="page"] College of Neurologists of South Africa
### College of Neurologists of South Africa
CNSA · Active
Name*
[value="College of Neurologists of South Africa"]
Short code*
[value="CNSA"]
Description
Constituent College of the CMSA; owns the national Neurology catalogue.
Status
(checkbox)
Active
An inactive College cannot be named by a new invitation. Nothing else reads it: its specialities, EPAs and curricula, and every institution that has adopted one of its curricula, carry on as they are.
[link] Cancel
[button] [aria-haspopup="dialog"] Deactivate…
[button] Save
#### Deactivate College of Neurologists of South Africa?
A new invitation will no longer be able to name it. That is all it changes: its specialities, EPAs and curricula stay as they are, every institution that has adopted one of its curricula carries on, and everyone already invited keeps their invitation.
You can reactivate it on this page.
[button] Cancel
[button] Deactivate

######## BOARD R2-B11a.dc.html
IllustrationR2-B11a · 1280
Deactivated · Demo Administrator · the result focused; Active unticked with the inactive line; Deactivate… gone; "CNSA · Inactive". No Reactivate button anywhere (M6): the box reactivates.
Focus: the result.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] [aria-current="page"] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Colleges
-
- [aria-current="page"] College of Neurologists of South Africa
### College of Neurologists of South Africa
CNSA · Inactive
College of Neurologists of South Africa deactivated. A new invitation can no longer name it.
Name*
[value="College of Neurologists of South Africa"]
Short code*
[value="CNSA"]
Description
Constituent College of the CMSA; owns the national Neurology catalogue.
Status
(checkbox)
Active
Inactive. A new invitation cannot name this College. Tick Active and save to reactivate it.
[link] Cancel
[button] Save

######## BOARD R2-B11a-390.dc.html
IllustrationR2-B11a-390 · 390
Deactivated · Demo Administrator · the result focused; Active unticked with the inactive line; Deactivate… gone; "CNSA · Inactive". No Reactivate button anywhere (M6): the box reactivates.
Focus: the result.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] [aria-current="page"] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Colleges
-
- [aria-current="page"] College of Neurologists of South Africa
### College of Neurologists of South Africa
CNSA · Inactive
College of Neurologists of South Africa deactivated. A new invitation can no longer name it.
Name*
[value="College of Neurologists of South Africa"]
Short code*
[value="CNSA"]
Description
Constituent College of the CMSA; owns the national Neurology catalogue.
Status
(checkbox)
Active
Inactive. A new invitation cannot name this College. Tick Active and save to reactivate it.
[link] Cancel
[button] Save

######## BOARD R2-B11b.dc.html
IllustrationR2-B11b · 1280
Deactivated · Demo Administrator · the list: CNSA Inactive.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] [aria-current="page"] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
### Colleges
The national Colleges. Each College's admin keeps its specialities, EPAs and curricula; you keep the Colleges themselves.
[link] Create college
Every College
| Name | Short code | Description | Status | Created | Actions |
| College of Neurologists of South Africa | CNSA | Constituent College of the CMSA; owns the national Neurology catalogue. | Inactive | 2026-11-02 | [link] [aria-label="Edit College of Neurologists of South Africa"] Edit [link] [aria-label="Specialities of College of Neurologists of South Africa"] Specialities |
| College of Paediatricians of South Africa | CPSA | Constituent College of the Colleges of Medicine of South Africa; owns the national Paediatric EPA catalogue. | Active | 2026-11-02 | [link] [aria-label="Edit College of Paediatricians of South Africa"] Edit [link] [aria-label="Specialities of College of Paediatricians of South Africa"] Specialities |
| Demo College | DEMO-C | Seeded demo college (national catalogue owner). | Active | 2026-11-02 | [link] [aria-label="Edit Demo College"] Edit [link] [aria-label="Specialities of Demo College"] Specialities |

######## BOARD R2-B11b-390.dc.html
IllustrationR2-B11b-390 · 390
Deactivated · Demo Administrator · the list: CNSA Inactive.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] [aria-current="page"] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
### Colleges
The national Colleges. Each College's admin keeps its specialities, EPAs and curricula; you keep the Colleges themselves.
[link] Create college
Every College
| Name | Short code | Description | Status | Created | Actions |
| College of Neurologists of South Africa | CNSA | Constituent College of the CMSA; owns the national Neurology catalogue. | Inactive | 2026-11-02 | [link] [aria-label="Edit College of Neurologists of South Africa"] Edit [link] [aria-label="Specialities of College of Neurologists of South Africa"] Specialities |
| College of Paediatricians of South Africa | CPSA | Constituent College of the Colleges of Medicine of South Africa; owns the national Paediatric EPA catalogue. | Active | 2026-11-02 | [link] [aria-label="Edit College of Paediatricians of South Africa"] Edit [link] [aria-label="Specialities of College of Paediatricians of South Africa"] Specialities |
| Demo College | DEMO-C | Seeded demo college (national catalogue owner). | Active | 2026-11-02 | [link] [aria-label="Edit Demo College"] Edit [link] [aria-label="Specialities of Demo College"] Specialities |

######## BOARD R2-B12.dc.html
IllustrationR2-B12 · 1280
A failed save · Demo Administrator · the NEW sentence, focused; the form as typed. Never "already exists" for a failure that is not a duplicate (T272).
Focus: the failure.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] [aria-current="page"] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Colleges
-
- [aria-current="page"] College of Paediatricians of South Africa
### College of Paediatricians of South Africa
CPSA · Active
The College could not be saved. Nothing has changed. Try again, or come back in a few minutes.
Name*
[value="College of Paediatricians of South Africa"]
Short code*
[value="CPSA"]
Description
Constituent College of the Colleges of Medicine of South Africa; owns the national Paediatric EPA catalogue and its versions.
Status
(checkbox) [checked]
Active
An inactive College cannot be named by a new invitation. Nothing else reads it: its specialities, EPAs and curricula, and every institution that has adopted one of its curricula, carry on as they are.
[link] Cancel
[button] [aria-haspopup="dialog"] Deactivate…
[button] Save

######## BOARD R2-B12-390.dc.html
IllustrationR2-B12-390 · 390
A failed save · Demo Administrator · the NEW sentence, focused; the form as typed. Never "already exists" for a failure that is not a duplicate (T272).
Focus: the failure.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] [aria-current="page"] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Colleges
-
- [aria-current="page"] College of Paediatricians of South Africa
### College of Paediatricians of South Africa
CPSA · Active
The College could not be saved. Nothing has changed. Try again, or come back in a few minutes.
Name*
[value="College of Paediatricians of South Africa"]
Short code*
[value="CPSA"]
Description
Constituent College of the Colleges of Medicine of South Africa; owns the national Paediatric EPA catalogue and its versions.
Status
(checkbox) [checked]
Active
An inactive College cannot be named by a new invitation. Nothing else reads it: its specialities, EPAs and curricula, and every institution that has adopted one of its curricula, carry on as they are.
[link] Cancel
[button] [aria-haspopup="dialog"] Deactivate…
[button] Save

######## BOARD R2-B13.dc.html
R2-B13 · 1280
Q7 · Demo Administrator · an unknown id: "College unavailable", Colleges lit (today Page not found, college-edit--not-found).
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] [aria-current="page"] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
- [aria-current="page"] Colleges
### College unavailable
This College does not exist, or you cannot open it.
[link] Go to Colleges

######## BOARD R2-B13-390.dc.html
R2-B13-390 · 390
Q7 · Demo Administrator · an unknown id: "College unavailable", Colleges lit (today Page not found, college-edit--not-found).
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] [aria-current="page"] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
- [aria-current="page"] Colleges
### College unavailable
This College does not exist, or you cannot open it.
[link] Go to Colleges

######## BOARD R2-B14a.dc.html
R2-B14a · 1280
Loading · the list: the header, the loading line, a skeleton; Create college waits for the read.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] [aria-current="page"] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
### Colleges
The national Colleges. Each College's admin keeps its specialities, EPAs and curricula; you keep the Colleges themselves.
Loading the Colleges.

######## BOARD R2-B14a-390.dc.html
R2-B14a-390 · 390
Loading · the list: the header, the loading line, a skeleton; Create college waits for the read.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] [aria-current="page"] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
### Colleges
The national Colleges. Each College's admin keeps its specialities, EPAs and curricula; you keep the Colleges themselves.
Loading the Colleges.

######## BOARD R2-B14b.dc.html
R2-B14b · 1280
Loading · the record, titled "College".
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] [aria-current="page"] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
- [aria-current="page"] Colleges
### College
Loading this College.

######## BOARD R2-B14b-390.dc.html
R2-B14b-390 · 390
Loading · the record, titled "College".
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] [aria-current="page"] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
- [aria-current="page"] Colleges
### College
Loading this College.

######## BOARD R2-B15a.dc.html
R2-B15a · 1280
Load error · the list.
Focus: Try again → the reloaded h1.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] [aria-current="page"] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
### Colleges
The national Colleges. Each College's admin keeps its specialities, EPAs and curricula; you keep the Colleges themselves.
Could not load the Colleges. Nothing has changed. Try again, or come back in a few minutes.
[button] Try again

######## BOARD R2-B15a-390.dc.html
R2-B15a-390 · 390
Load error · the list.
Focus: Try again → the reloaded h1.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] [aria-current="page"] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
### Colleges
The national Colleges. Each College's admin keeps its specialities, EPAs and curricula; you keep the Colleges themselves.
Could not load the Colleges. Nothing has changed. Try again, or come back in a few minutes.
[button] Try again

######## BOARD R2-B15b.dc.html
R2-B15b · 1280
Load error · the record.
Focus: Try again → the reloaded h1.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] [aria-current="page"] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
- [aria-current="page"] Colleges
### College
Could not load this College. Nothing has changed. Try again, or come back in a few minutes.
[button] Try again

######## BOARD R2-B15b-390.dc.html
R2-B15b-390 · 390
Load error · the record.
Focus: Try again → the reloaded h1.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] [aria-current="page"] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
- [aria-current="page"] Colleges
### College
Could not load this College. Nothing has changed. Try again, or come back in a few minutes.
[button] Try again

######## BOARD R2-B16.dc.html
IllustrationR2-B16 · 1280
Heavy · five Colleges, two with similar names: every row action names the College in full; the short code sits beside it.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] [aria-current="page"] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
### Colleges
The national Colleges. Each College's admin keeps its specialities, EPAs and curricula; you keep the Colleges themselves.
[link] Create college
Every College
| Name | Short code | Description | Status | Created | Actions |
| College of Neurologists of South Africa | CNSA | Constituent College of the CMSA; owns the national Neurology catalogue. | Active | 2026-11-02 | [link] [aria-label="Edit College of Neurologists of South Africa"] Edit [link] [aria-label="Specialities of College of Neurologists of South Africa"] Specialities |
| College of Paediatricians of South Africa | CPSA | Constituent College of the Colleges of Medicine of South Africa; owns the national Paediatric EPA catalogue. | Active | 2026-11-02 | [link] [aria-label="Edit College of Paediatricians of South Africa"] Edit [link] [aria-label="Specialities of College of Paediatricians of South Africa"] Specialities |
| Demo College | DEMO-C | Seeded demo college (national catalogue owner). | Active | 2026-11-02 | [link] [aria-label="Edit Demo College"] Edit [link] [aria-label="Specialities of Demo College"] Specialities |
| ‹College name A› | ‹code A› | ‹description› | Active | ‹created› | [link] [aria-label="Edit ‹College name A›"] Edit [link] [aria-label="Specialities of ‹College name A›"] Specialities |
| ‹College name A, a near twin› | ‹code B› | ‹description› | Active | ‹created› | [link] [aria-label="Edit ‹College name A, a near twin›"] Edit [link] [aria-label="Specialities of ‹College name A, a near twin›"] Specialities |

######## BOARD R2-B16-390.dc.html
IllustrationR2-B16-390 · 390
Heavy · five Colleges, two with similar names: every row action names the College in full; the short code sits beside it.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] [aria-current="page"] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
### Colleges
The national Colleges. Each College's admin keeps its specialities, EPAs and curricula; you keep the Colleges themselves.
[link] Create college
Every College
| Name | Short code | Description | Status | Created | Actions |
| College of Neurologists of South Africa | CNSA | Constituent College of the CMSA; owns the national Neurology catalogue. | Active | 2026-11-02 | [link] [aria-label="Edit College of Neurologists of South Africa"] Edit [link] [aria-label="Specialities of College of Neurologists of South Africa"] Specialities |
| College of Paediatricians of South Africa | CPSA | Constituent College of the Colleges of Medicine of South Africa; owns the national Paediatric EPA catalogue. | Active | 2026-11-02 | [link] [aria-label="Edit College of Paediatricians of South Africa"] Edit [link] [aria-label="Specialities of College of Paediatricians of South Africa"] Specialities |
| Demo College | DEMO-C | Seeded demo college (national catalogue owner). | Active | 2026-11-02 | [link] [aria-label="Edit Demo College"] Edit [link] [aria-label="Specialities of Demo College"] Specialities |
| ‹College name A› | ‹code A› | ‹description› | Active | ‹created› | [link] [aria-label="Edit ‹College name A›"] Edit [link] [aria-label="Specialities of ‹College name A›"] Specialities |
| ‹College name A, a near twin› | ‹code B› | ‹description› | Active | ‹created› | [link] [aria-label="Edit ‹College name A, a near twin›"] Edit [link] [aria-label="Specialities of ‹College name A, a near twin›"] Specialities |

######## BOARD R2-B16-768.dc.html
IllustrationR2-B16-768 · 768
Heavy · five Colleges, two with similar names: every row action names the College in full; the short code sits beside it.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] [aria-current="page"] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
### Colleges
The national Colleges. Each College's admin keeps its specialities, EPAs and curricula; you keep the Colleges themselves.
[link] Create college
Every College
| Name | Short code | Description | Status | Created | Actions |
| College of Neurologists of South Africa | CNSA | Constituent College of the CMSA; owns the national Neurology catalogue. | Active | 2026-11-02 | [link] [aria-label="Edit College of Neurologists of South Africa"] Edit [link] [aria-label="Specialities of College of Neurologists of South Africa"] Specialities |
| College of Paediatricians of South Africa | CPSA | Constituent College of the Colleges of Medicine of South Africa; owns the national Paediatric EPA catalogue. | Active | 2026-11-02 | [link] [aria-label="Edit College of Paediatricians of South Africa"] Edit [link] [aria-label="Specialities of College of Paediatricians of South Africa"] Specialities |
| Demo College | DEMO-C | Seeded demo college (national catalogue owner). | Active | 2026-11-02 | [link] [aria-label="Edit Demo College"] Edit [link] [aria-label="Specialities of Demo College"] Specialities |
| ‹College name A› | ‹code A› | ‹description› | Active | ‹created› | [link] [aria-label="Edit ‹College name A›"] Edit [link] [aria-label="Specialities of ‹College name A›"] Specialities |
| ‹College name A, a near twin› | ‹code B› | ‹description› | Active | ‹created› | [link] [aria-label="Edit ‹College name A, a near twin›"] Edit [link] [aria-label="Specialities of ‹College name A, a near twin›"] Specialities |

######## BOARD R2-B17a.dc.html
IllustrationR2-B17a · 1280
A 2,000-character description · the list: it wraps in its .col-wrap cell, nothing sideways.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] [aria-current="page"] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
### Colleges
The national Colleges. Each College's admin keeps its specialities, EPAs and curricula; you keep the Colleges themselves.
[link] Create college
Every College
| Name | Short code | Description | Status | Created | Actions |
| College of Neurologists of South Africa | CNSA | ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› | Active | 2026-11-02 | [link] [aria-label="Edit College of Neurologists of South Africa"] Edit [link] [aria-label="Specialities of College of Neurologists of South Africa"] Specialities |
| College of Paediatricians of South Africa | CPSA | Constituent College of the Colleges of Medicine of South Africa; owns the national Paediatric EPA catalogue. | Active | 2026-11-02 | [link] [aria-label="Edit College of Paediatricians of South Africa"] Edit [link] [aria-label="Specialities of College of Paediatricians of South Africa"] Specialities |
| Demo College | DEMO-C | Seeded demo college (national catalogue owner). | Active | 2026-11-02 | [link] [aria-label="Edit Demo College"] Edit [link] [aria-label="Specialities of Demo College"] Specialities |

######## BOARD R2-B17a-390.dc.html
IllustrationR2-B17a-390 · 390
A 2,000-character description · the list: it wraps in its .col-wrap cell, nothing sideways.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] [aria-current="page"] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
### Colleges
The national Colleges. Each College's admin keeps its specialities, EPAs and curricula; you keep the Colleges themselves.
[link] Create college
Every College
| Name | Short code | Description | Status | Created | Actions |
| College of Neurologists of South Africa | CNSA | ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› | Active | 2026-11-02 | [link] [aria-label="Edit College of Neurologists of South Africa"] Edit [link] [aria-label="Specialities of College of Neurologists of South Africa"] Specialities |
| College of Paediatricians of South Africa | CPSA | Constituent College of the Colleges of Medicine of South Africa; owns the national Paediatric EPA catalogue. | Active | 2026-11-02 | [link] [aria-label="Edit College of Paediatricians of South Africa"] Edit [link] [aria-label="Specialities of College of Paediatricians of South Africa"] Specialities |
| Demo College | DEMO-C | Seeded demo college (national catalogue owner). | Active | 2026-11-02 | [link] [aria-label="Edit Demo College"] Edit [link] [aria-label="Specialities of Demo College"] Specialities |

######## BOARD R2-B17b.dc.html
IllustrationR2-B17b · 1280
A 2,000-character description · the record's textarea (it scrolls inside itself).
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] [aria-current="page"] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Colleges
-
- [aria-current="page"] College of Neurologists of South Africa
### College of Neurologists of South Africa
CNSA · Active
Name*
[value="College of Neurologists of South Africa"]
Short code*
[value="CNSA"]
Description
‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters›
Status
(checkbox) [checked]
Active
An inactive College cannot be named by a new invitation. Nothing else reads it: its specialities, EPAs and curricula, and every institution that has adopted one of its curricula, carry on as they are.
[link] Cancel
[button] [aria-haspopup="dialog"] Deactivate…
[button] Save

######## BOARD R2-B17b-390.dc.html
IllustrationR2-B17b-390 · 390
A 2,000-character description · the record's textarea (it scrolls inside itself).
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] [aria-current="page"] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Colleges
-
- [aria-current="page"] College of Neurologists of South Africa
### College of Neurologists of South Africa
CNSA · Active
Name*
[value="College of Neurologists of South Africa"]
Short code*
[value="CNSA"]
Description
‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters› ‹a description of 2,000 characters›
Status
(checkbox) [checked]
Active
An inactive College cannot be named by a new invitation. Nothing else reads it: its specialities, EPAs and curricula, and every institution that has adopted one of its curricula, carry on as they are.
[link] Cancel
[button] [aria-haspopup="dialog"] Deactivate…
[button] Save

######## BOARD R2-C01.dc.html
R2-C01 · 1280
1.4 · Demo Administrator · two scales by name, S2's built words; Edit and Delete… (outline, named for the row; Delete… asks, Q11).
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] [aria-current="page"] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
### Entrustment scales
Define and maintain the rating ladders used by assessment forms and committee reviews.
[link] Create scale
Entrustment scales, by name
| Name | Description | Levels | Actions |
| CPSA Paediatric Entrustment Scale v11.1 | Six rungs in four bands. Only level 3 is split: the gap between "check everything with me" and "call me if you are worried" is the whole of a registrar’s third year. | 6 | [link] [aria-label="Edit CPSA Paediatric Entrustment Scale v11.1"] Edit [button] [aria-label="Delete CPSA Paediatric Entrustment Scale v11.1"] [aria-haspopup="dialog"] Delete… |
| O-R Scale | Standard 1-5 observation-to-entrustment scale. | 5 | [link] [aria-label="Edit O-R Scale"] Edit [button] [aria-label="Delete O-R Scale"] [aria-haspopup="dialog"] Delete… |

######## BOARD R2-C01-390.dc.html
R2-C01-390 · 390
1.4 · Demo Administrator · two scales by name, S2's built words; Edit and Delete… (outline, named for the row; Delete… asks, Q11).
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] [aria-current="page"] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
### Entrustment scales
Define and maintain the rating ladders used by assessment forms and committee reviews.
[link] Create scale
Entrustment scales, by name
| Name | Description | Levels | Actions |
| CPSA Paediatric Entrustment Scale v11.1 | Six rungs in four bands. Only level 3 is split: the gap between "check everything with me" and "call me if you are worried" is the whole of a registrar’s third year. | 6 | [link] [aria-label="Edit CPSA Paediatric Entrustment Scale v11.1"] Edit [button] [aria-label="Delete CPSA Paediatric Entrustment Scale v11.1"] [aria-haspopup="dialog"] Delete… |
| O-R Scale | Standard 1-5 observation-to-entrustment scale. | 5 | [link] [aria-label="Edit O-R Scale"] Edit [button] [aria-label="Delete O-R Scale"] [aria-haspopup="dialog"] Delete… |

######## BOARD R2-C01-768.dc.html
R2-C01-768 · 768
1.4 · Demo Administrator · two scales by name, S2's built words; Edit and Delete… (outline, named for the row; Delete… asks, Q11).
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] [aria-current="page"] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
### Entrustment scales
Define and maintain the rating ladders used by assessment forms and committee reviews.
[link] Create scale
Entrustment scales, by name
| Name | Description | Levels | Actions |
| CPSA Paediatric Entrustment Scale v11.1 | Six rungs in four bands. Only level 3 is split: the gap between "check everything with me" and "call me if you are worried" is the whole of a registrar’s third year. | 6 | [link] [aria-label="Edit CPSA Paediatric Entrustment Scale v11.1"] Edit [button] [aria-label="Delete CPSA Paediatric Entrustment Scale v11.1"] [aria-haspopup="dialog"] Delete… |
| O-R Scale | Standard 1-5 observation-to-entrustment scale. | 5 | [link] [aria-label="Edit O-R Scale"] Edit [button] [aria-label="Delete O-R Scale"] [aria-haspopup="dialog"] Delete… |

######## BOARD R2-C02.dc.html
R2-C02 · 1280
1.4 · Demo Administrator · the CPSA ladder: titled by its name (T1, S11); h2 Details and Levels (today h3s); Order beside each label; each descriptor whole in its textarea (S4).
In use (every 11.1 item is pinned to it), so no Up, Down or Remove: Q16's line above Add level (T395). The 390 twin: StackWide on the inputs table, each rung a block (S6).
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] [aria-current="page"] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Entrustment scales
-
- [aria-current="page"] CPSA Paediatric Entrustment Scale v11.1
### CPSA Paediatric Entrustment Scale v11.1
A ladder shared by every College and institution.
#### Details
Name*
[value="CPSA Paediatric Entrustment Scale v11.1"]
Description
Six rungs in four bands. Only level 3 is split: the gap between "check everything with me" and "call me if you are worried" is the whole of a registrar’s third year.
#### Levels
A rung is read by its label everywhere ("3a", "5"). Its order is its rank, lowest first, and everything that compares levels uses the order: on a ladder with a split rung, the label and the order differ. The order is shown only on this page.
Levels of CPSA Paediatric Entrustment Scale v11.1, lowest first
| Order | Label* | Description |
| 1 | Label of the rung in order 1 [value="1"] | Description of 1Not entrusted to act. The supervisor performs the activity and thinks aloud; the trainee prepares, observes actively, and may not enact or document decisions. |
| 2 | Label of the rung in order 2 [value="2"] | Description of 2Direct supervision. Supervisor present throughout, sharing the task or watching silently, ready to step in. The plan may be entirely the trainee’s, but nothing is acted on without sign-off. |
| 3 | Label of the rung in order 3 [value="3a"] | Description of 3aIndirect supervision, decisions checked. Supervisor on site or immediately available and reviews findings, working diagnosis and plan before they are enacted. Trainee may act alone on routine assessment, investigation and supportive care within protocol. |
| 4 | Label of the rung in order 4 [value="3b"] | Description of 3bIndirect supervision, reviewed on the trainee’s initiative. Supervisor available on request; the trainee decides what needs discussion and shows reliable help-seeking. Full management of the activity, supervisor informed rather than consulted. |
| 5 | Label of the rung in order 5 [value="4"] | Description of 4Unsupervised practice. The trainee carries the responsibility for the activity. |
| 6 | Label of the rung in order 6 [value="5"] | Description of 5Supervises others. The trainee carries the responsibility and can supervise juniors in the activity. |
Rungs can be reordered or removed only while nothing uses this scale.
[button] Add level
[link] Cancel
[button] Save

######## BOARD R2-C02-390.dc.html
R2-C02-390 · 390
1.4 · Demo Administrator · the CPSA ladder: titled by its name (T1, S11); h2 Details and Levels (today h3s); Order beside each label; each descriptor whole in its textarea (S4).
In use (every 11.1 item is pinned to it), so no Up, Down or Remove: Q16's line above Add level (T395). The 390 twin: StackWide on the inputs table, each rung a block (S6).
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] [aria-current="page"] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Entrustment scales
-
- [aria-current="page"] CPSA Paediatric Entrustment Scale v11.1
### CPSA Paediatric Entrustment Scale v11.1
A ladder shared by every College and institution.
#### Details
Name*
[value="CPSA Paediatric Entrustment Scale v11.1"]
Description
Six rungs in four bands. Only level 3 is split: the gap between "check everything with me" and "call me if you are worried" is the whole of a registrar’s third year.
#### Levels
A rung is read by its label everywhere ("3a", "5"). Its order is its rank, lowest first, and everything that compares levels uses the order: on a ladder with a split rung, the label and the order differ. The order is shown only on this page.
Levels of CPSA Paediatric Entrustment Scale v11.1, lowest first
| Order | Label* | Description |
| 1 | Label of the rung in order 1 [value="1"] | Description of 1Not entrusted to act. The supervisor performs the activity and thinks aloud; the trainee prepares, observes actively, and may not enact or document decisions. |
| 2 | Label of the rung in order 2 [value="2"] | Description of 2Direct supervision. Supervisor present throughout, sharing the task or watching silently, ready to step in. The plan may be entirely the trainee’s, but nothing is acted on without sign-off. |
| 3 | Label of the rung in order 3 [value="3a"] | Description of 3aIndirect supervision, decisions checked. Supervisor on site or immediately available and reviews findings, working diagnosis and plan before they are enacted. Trainee may act alone on routine assessment, investigation and supportive care within protocol. |
| 4 | Label of the rung in order 4 [value="3b"] | Description of 3bIndirect supervision, reviewed on the trainee’s initiative. Supervisor available on request; the trainee decides what needs discussion and shows reliable help-seeking. Full management of the activity, supervisor informed rather than consulted. |
| 5 | Label of the rung in order 5 [value="4"] | Description of 4Unsupervised practice. The trainee carries the responsibility for the activity. |
| 6 | Label of the rung in order 6 [value="5"] | Description of 5Supervises others. The trainee carries the responsibility and can supervise juniors in the activity. |
Rungs can be reordered or removed only while nothing uses this scale.
[button] Add level
[link] Cancel
[button] Save

######## BOARD R2-C03a.dc.html
R2-C03a · 1280
6.5 · Demo Administrator · Create entrustment scale: it starts with two empty rungs (built); Add level adds a third, fourth and fifth.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] [aria-current="page"] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Entrustment scales
-
- [aria-current="page"] Create entrustment scale
### Create entrustment scale
A ladder of at least two rungs, shared by every College and institution.
#### Details
Name*
Description
#### Levels
A rung is read by its label everywhere ("3a", "5"). Its order is its rank, lowest first, and everything that compares levels uses the order: on a ladder with a split rung, the label and the order differ. The order is shown only on this page.
Levels of the new scale, lowest first
| Order | Label* | Description | Actions |
| 1 | Label of the rung in order 1 | Description of the rung in order 1 | [button] [aria-label="Move the rung in order 1 up"] [disabled] Up [button] [aria-label="Move the rung in order 1 down"] Down [button] [aria-label="Remove the rung in order 1"] Remove |
| 2 | Label of the rung in order 2 | Description of the rung in order 2 | [button] [aria-label="Move the rung in order 2 up"] Up [button] [aria-label="Move the rung in order 2 down"] [disabled] Down [button] [aria-label="Remove the rung in order 2"] Remove |
[button] Add level
[link] Cancel
[button] Create scale

######## BOARD R2-C03a-390.dc.html
R2-C03a-390 · 390
6.5 · Demo Administrator · Create entrustment scale: it starts with two empty rungs (built); Add level adds a third, fourth and fifth.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] [aria-current="page"] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Entrustment scales
-
- [aria-current="page"] Create entrustment scale
### Create entrustment scale
A ladder of at least two rungs, shared by every College and institution.
#### Details
Name*
Description
#### Levels
A rung is read by its label everywhere ("3a", "5"). Its order is its rank, lowest first, and everything that compares levels uses the order: on a ladder with a split rung, the label and the order differ. The order is shown only on this page.
Levels of the new scale, lowest first
| Order | Label* | Description | Actions |
| 1 | Label of the rung in order 1 | Description of the rung in order 1 | [button] [aria-label="Move the rung in order 1 up"] [disabled] Up [button] [aria-label="Move the rung in order 1 down"] Down [button] [aria-label="Remove the rung in order 1"] Remove |
| 2 | Label of the rung in order 2 | Description of the rung in order 2 | [button] [aria-label="Move the rung in order 2 up"] Up [button] [aria-label="Move the rung in order 2 down"] [disabled] Down [button] [aria-label="Remove the rung in order 2"] Remove |
[button] Add level
[link] Cancel
[button] Create scale

######## BOARD R2-C03b.dc.html
R2-C03b · 1280
6.5 refused · Demo Administrator · under the fourth rung's Label, the label's words (today "Level labels must be unique within a scale."); nothing created; everything kept.
Focus: the fourth rung's Label.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] [aria-current="page"] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Entrustment scales
-
- [aria-current="page"] Create entrustment scale
### Create entrustment scale
A ladder of at least two rungs, shared by every College and institution.
#### Details
Name*
[value="CNSA Neurology Entrustment Scale"]
Description
Supervision ladder for CNSA EPAs.
#### Levels
A rung is read by its label everywhere ("3a", "5"). Its order is its rank, lowest first, and everything that compares levels uses the order: on a ladder with a split rung, the label and the order differ. The order is shown only on this page.
Levels of CNSA Neurology Entrustment Scale, lowest first
| Order | Label* | Description | Actions |
| 1 | Label of the rung in order 1 [value="Observe only"] | Description of Observe only‹description› | [button] [aria-label="Move Observe only up"] [disabled] Up [button] [aria-label="Move Observe only down"] Down [button] [aria-label="Remove Observe only"] Remove |
| 2 | Label of the rung in order 2 [value="Direct supervision"] | Description of Direct supervision‹description› | [button] [aria-label="Move Direct supervision up"] Up [button] [aria-label="Move Direct supervision down"] Down [button] [aria-label="Remove Direct supervision"] Remove |
| 3 | Label of the rung in order 3 [value="Indirect supervision"] | Description of Indirect supervision‹description› | [button] [aria-label="Move Indirect supervision up"] Up [button] [aria-label="Move Indirect supervision down"] Down [button] [aria-label="Remove Indirect supervision"] Remove |
| 4 | Label of the rung in order 4 [value="Indirect supervision"] “Indirect supervision” is already the label of another rung. Give each rung its own label. | Description of Indirect supervision‹description› | [button] [aria-label="Move Indirect supervision up"] Up [button] [aria-label="Move Indirect supervision down"] Down [button] [aria-label="Remove Indirect supervision"] Remove |
| 5 | Label of the rung in order 5 [value="Unsupervised practice"] | Description of Unsupervised practice‹description› | [button] [aria-label="Move Unsupervised practice up"] Up [button] [aria-label="Move Unsupervised practice down"] [disabled] Down [button] [aria-label="Remove Unsupervised practice"] Remove |
[button] Add level
[link] Cancel
[button] Create scale

######## BOARD R2-C03b-390.dc.html
R2-C03b-390 · 390
6.5 refused · Demo Administrator · under the fourth rung's Label, the label's words (today "Level labels must be unique within a scale."); nothing created; everything kept.
Focus: the fourth rung's Label.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] [aria-current="page"] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Entrustment scales
-
- [aria-current="page"] Create entrustment scale
### Create entrustment scale
A ladder of at least two rungs, shared by every College and institution.
#### Details
Name*
[value="CNSA Neurology Entrustment Scale"]
Description
Supervision ladder for CNSA EPAs.
#### Levels
A rung is read by its label everywhere ("3a", "5"). Its order is its rank, lowest first, and everything that compares levels uses the order: on a ladder with a split rung, the label and the order differ. The order is shown only on this page.
Levels of CNSA Neurology Entrustment Scale, lowest first
| Order | Label* | Description | Actions |
| 1 | Label of the rung in order 1 [value="Observe only"] | Description of Observe only‹description› | [button] [aria-label="Move Observe only up"] [disabled] Up [button] [aria-label="Move Observe only down"] Down [button] [aria-label="Remove Observe only"] Remove |
| 2 | Label of the rung in order 2 [value="Direct supervision"] | Description of Direct supervision‹description› | [button] [aria-label="Move Direct supervision up"] Up [button] [aria-label="Move Direct supervision down"] Down [button] [aria-label="Remove Direct supervision"] Remove |
| 3 | Label of the rung in order 3 [value="Indirect supervision"] | Description of Indirect supervision‹description› | [button] [aria-label="Move Indirect supervision up"] Up [button] [aria-label="Move Indirect supervision down"] Down [button] [aria-label="Remove Indirect supervision"] Remove |
| 4 | Label of the rung in order 4 [value="Indirect supervision"] “Indirect supervision” is already the label of another rung. Give each rung its own label. | Description of Indirect supervision‹description› | [button] [aria-label="Move Indirect supervision up"] Up [button] [aria-label="Move Indirect supervision down"] Down [button] [aria-label="Remove Indirect supervision"] Remove |
| 5 | Label of the rung in order 5 [value="Unsupervised practice"] | Description of Unsupervised practice‹description› | [button] [aria-label="Move Unsupervised practice up"] Up [button] [aria-label="Move Unsupervised practice down"] [disabled] Down [button] [aria-label="Remove Unsupervised practice"] Remove |
[button] Add level
[link] Cancel
[button] Create scale

######## BOARD R2-C04.dc.html
R2-C04 · 1280
6.5 created · Demo Administrator · the CNSA scale's page by a one-shot token, "Entrustment scale created." focused; five rungs; nothing uses it, so Up, Down and Remove show (Up on the first and Down on the last disabled).
Focus: the result.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] [aria-current="page"] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Entrustment scales
-
- [aria-current="page"] CNSA Neurology Entrustment Scale
### CNSA Neurology Entrustment Scale
A ladder shared by every College and institution.
Entrustment scale created.
#### Details
Name*
[value="CNSA Neurology Entrustment Scale"]
Description
Supervision ladder for CNSA EPAs.
#### Levels
A rung is read by its label everywhere ("3a", "5"). Its order is its rank, lowest first, and everything that compares levels uses the order: on a ladder with a split rung, the label and the order differ. The order is shown only on this page.
Levels of CNSA Neurology Entrustment Scale, lowest first
| Order | Label* | Description | Actions |
| 1 | Label of the rung in order 1 [value="Observe only"] | Description of Observe only‹description› | [button] [aria-label="Move Observe only up"] [disabled] Up [button] [aria-label="Move Observe only down"] Down [button] [aria-label="Remove Observe only"] Remove |
| 2 | Label of the rung in order 2 [value="Direct supervision"] | Description of Direct supervision‹description› | [button] [aria-label="Move Direct supervision up"] Up [button] [aria-label="Move Direct supervision down"] Down [button] [aria-label="Remove Direct supervision"] Remove |
| 3 | Label of the rung in order 3 [value="Indirect supervision"] | Description of Indirect supervision‹description› | [button] [aria-label="Move Indirect supervision up"] Up [button] [aria-label="Move Indirect supervision down"] Down [button] [aria-label="Remove Indirect supervision"] Remove |
| 4 | Label of the rung in order 4 [value="Distant supervision"] | Description of Distant supervision‹description› | [button] [aria-label="Move Distant supervision up"] Up [button] [aria-label="Move Distant supervision down"] Down [button] [aria-label="Remove Distant supervision"] Remove |
| 5 | Label of the rung in order 5 [value="Unsupervised practice"] | Description of Unsupervised practice‹description› | [button] [aria-label="Move Unsupervised practice up"] Up [button] [aria-label="Move Unsupervised practice down"] [disabled] Down [button] [aria-label="Remove Unsupervised practice"] Remove |
[button] Add level
[link] Cancel
[button] Save

######## BOARD R2-C04-390.dc.html
R2-C04-390 · 390
6.5 created · Demo Administrator · the CNSA scale's page by a one-shot token, "Entrustment scale created." focused; five rungs; nothing uses it, so Up, Down and Remove show (Up on the first and Down on the last disabled).
Focus: the result.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] [aria-current="page"] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Entrustment scales
-
- [aria-current="page"] CNSA Neurology Entrustment Scale
### CNSA Neurology Entrustment Scale
A ladder shared by every College and institution.
Entrustment scale created.
#### Details
Name*
[value="CNSA Neurology Entrustment Scale"]
Description
Supervision ladder for CNSA EPAs.
#### Levels
A rung is read by its label everywhere ("3a", "5"). Its order is its rank, lowest first, and everything that compares levels uses the order: on a ladder with a split rung, the label and the order differ. The order is shown only on this page.
Levels of CNSA Neurology Entrustment Scale, lowest first
| Order | Label* | Description | Actions |
| 1 | Label of the rung in order 1 [value="Observe only"] | Description of Observe only‹description› | [button] [aria-label="Move Observe only up"] [disabled] Up [button] [aria-label="Move Observe only down"] Down [button] [aria-label="Remove Observe only"] Remove |
| 2 | Label of the rung in order 2 [value="Direct supervision"] | Description of Direct supervision‹description› | [button] [aria-label="Move Direct supervision up"] Up [button] [aria-label="Move Direct supervision down"] Down [button] [aria-label="Remove Direct supervision"] Remove |
| 3 | Label of the rung in order 3 [value="Indirect supervision"] | Description of Indirect supervision‹description› | [button] [aria-label="Move Indirect supervision up"] Up [button] [aria-label="Move Indirect supervision down"] Down [button] [aria-label="Remove Indirect supervision"] Remove |
| 4 | Label of the rung in order 4 [value="Distant supervision"] | Description of Distant supervision‹description› | [button] [aria-label="Move Distant supervision up"] Up [button] [aria-label="Move Distant supervision down"] Down [button] [aria-label="Remove Distant supervision"] Remove |
| 5 | Label of the rung in order 5 [value="Unsupervised practice"] | Description of Unsupervised practice‹description› | [button] [aria-label="Move Unsupervised practice up"] Up [button] [aria-label="Move Unsupervised practice down"] [disabled] Down [button] [aria-label="Remove Unsupervised practice"] Remove |
[button] Add level
[link] Cancel
[button] Save

######## BOARD R2-C05.dc.html
R2-C05 · 1280
6.6 saved · Demo Administrator · six rungs, Supervises others at 6, "Entrustment scale saved." focused (S6 at 390).
Focus: the result.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] [aria-current="page"] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Entrustment scales
-
- [aria-current="page"] CNSA Neurology Entrustment Scale
### CNSA Neurology Entrustment Scale
A ladder shared by every College and institution.
Entrustment scale saved.
#### Details
Name*
[value="CNSA Neurology Entrustment Scale"]
Description
Supervision ladder for CNSA EPAs.
#### Levels
A rung is read by its label everywhere ("3a", "5"). Its order is its rank, lowest first, and everything that compares levels uses the order: on a ladder with a split rung, the label and the order differ. The order is shown only on this page.
Levels of CNSA Neurology Entrustment Scale, lowest first
| Order | Label* | Description | Actions |
| 1 | Label of the rung in order 1 [value="Observe only"] | Description of Observe only‹description› | [button] [aria-label="Move Observe only up"] [disabled] Up [button] [aria-label="Move Observe only down"] Down [button] [aria-label="Remove Observe only"] Remove |
| 2 | Label of the rung in order 2 [value="Direct supervision"] | Description of Direct supervision‹description› | [button] [aria-label="Move Direct supervision up"] Up [button] [aria-label="Move Direct supervision down"] Down [button] [aria-label="Remove Direct supervision"] Remove |
| 3 | Label of the rung in order 3 [value="Indirect supervision"] | Description of Indirect supervision‹description› | [button] [aria-label="Move Indirect supervision up"] Up [button] [aria-label="Move Indirect supervision down"] Down [button] [aria-label="Remove Indirect supervision"] Remove |
| 4 | Label of the rung in order 4 [value="Distant supervision"] | Description of Distant supervision‹description› | [button] [aria-label="Move Distant supervision up"] Up [button] [aria-label="Move Distant supervision down"] Down [button] [aria-label="Remove Distant supervision"] Remove |
| 5 | Label of the rung in order 5 [value="Unsupervised practice"] | Description of Unsupervised practice‹description› | [button] [aria-label="Move Unsupervised practice up"] Up [button] [aria-label="Move Unsupervised practice down"] Down [button] [aria-label="Remove Unsupervised practice"] Remove |
| 6 | Label of the rung in order 6 [value="Supervises others"] | Description of Supervises othersSupervises juniors performing the activity. | [button] [aria-label="Move Supervises others up"] Up [button] [aria-label="Move Supervises others down"] [disabled] Down [button] [aria-label="Remove Supervises others"] Remove |
[button] Add level
[link] Cancel
[button] Save

######## BOARD R2-C05-390.dc.html
R2-C05-390 · 390
6.6 saved · Demo Administrator · six rungs, Supervises others at 6, "Entrustment scale saved." focused (S6 at 390).
Focus: the result.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] [aria-current="page"] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Entrustment scales
-
- [aria-current="page"] CNSA Neurology Entrustment Scale
### CNSA Neurology Entrustment Scale
A ladder shared by every College and institution.
Entrustment scale saved.
#### Details
Name*
[value="CNSA Neurology Entrustment Scale"]
Description
Supervision ladder for CNSA EPAs.
#### Levels
A rung is read by its label everywhere ("3a", "5"). Its order is its rank, lowest first, and everything that compares levels uses the order: on a ladder with a split rung, the label and the order differ. The order is shown only on this page.
Levels of CNSA Neurology Entrustment Scale, lowest first
| Order | Label* | Description | Actions |
| 1 | Label of the rung in order 1 [value="Observe only"] | Description of Observe only‹description› | [button] [aria-label="Move Observe only up"] [disabled] Up [button] [aria-label="Move Observe only down"] Down [button] [aria-label="Remove Observe only"] Remove |
| 2 | Label of the rung in order 2 [value="Direct supervision"] | Description of Direct supervision‹description› | [button] [aria-label="Move Direct supervision up"] Up [button] [aria-label="Move Direct supervision down"] Down [button] [aria-label="Remove Direct supervision"] Remove |
| 3 | Label of the rung in order 3 [value="Indirect supervision"] | Description of Indirect supervision‹description› | [button] [aria-label="Move Indirect supervision up"] Up [button] [aria-label="Move Indirect supervision down"] Down [button] [aria-label="Remove Indirect supervision"] Remove |
| 4 | Label of the rung in order 4 [value="Distant supervision"] | Description of Distant supervision‹description› | [button] [aria-label="Move Distant supervision up"] Up [button] [aria-label="Move Distant supervision down"] Down [button] [aria-label="Remove Distant supervision"] Remove |
| 5 | Label of the rung in order 5 [value="Unsupervised practice"] | Description of Unsupervised practice‹description› | [button] [aria-label="Move Unsupervised practice up"] Up [button] [aria-label="Move Unsupervised practice down"] Down [button] [aria-label="Remove Unsupervised practice"] Remove |
| 6 | Label of the rung in order 6 [value="Supervises others"] | Description of Supervises othersSupervises juniors performing the activity. | [button] [aria-label="Move Supervises others up"] Up [button] [aria-label="Move Supervises others down"] [disabled] Down [button] [aria-label="Remove Supervises others"] Remove |
[button] Add level
[link] Cancel
[button] Save

######## BOARD R2-C06.dc.html
R2-C06 · 1280
6.6 · Demo Administrator · the list of three by name: CNSA 6, CPSA 6, O-R 5.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] [aria-current="page"] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
### Entrustment scales
Define and maintain the rating ladders used by assessment forms and committee reviews.
[link] Create scale
Entrustment scales, by name
| Name | Description | Levels | Actions |
| CNSA Neurology Entrustment Scale | Supervision ladder for CNSA EPAs. | 6 | [link] [aria-label="Edit CNSA Neurology Entrustment Scale"] Edit [button] [aria-label="Delete CNSA Neurology Entrustment Scale"] [aria-haspopup="dialog"] Delete… |
| CPSA Paediatric Entrustment Scale v11.1 | Six rungs in four bands. Only level 3 is split: the gap between "check everything with me" and "call me if you are worried" is the whole of a registrar’s third year. | 6 | [link] [aria-label="Edit CPSA Paediatric Entrustment Scale v11.1"] Edit [button] [aria-label="Delete CPSA Paediatric Entrustment Scale v11.1"] [aria-haspopup="dialog"] Delete… |
| O-R Scale | Standard 1-5 observation-to-entrustment scale. | 5 | [link] [aria-label="Edit O-R Scale"] Edit [button] [aria-label="Delete O-R Scale"] [aria-haspopup="dialog"] Delete… |

######## BOARD R2-C06-390.dc.html
R2-C06-390 · 390
6.6 · Demo Administrator · the list of three by name: CNSA 6, CPSA 6, O-R 5.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] [aria-current="page"] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
### Entrustment scales
Define and maintain the rating ladders used by assessment forms and committee reviews.
[link] Create scale
Entrustment scales, by name
| Name | Description | Levels | Actions |
| CNSA Neurology Entrustment Scale | Supervision ladder for CNSA EPAs. | 6 | [link] [aria-label="Edit CNSA Neurology Entrustment Scale"] Edit [button] [aria-label="Delete CNSA Neurology Entrustment Scale"] [aria-haspopup="dialog"] Delete… |
| CPSA Paediatric Entrustment Scale v11.1 | Six rungs in four bands. Only level 3 is split: the gap between "check everything with me" and "call me if you are worried" is the whole of a registrar’s third year. | 6 | [link] [aria-label="Edit CPSA Paediatric Entrustment Scale v11.1"] Edit [button] [aria-label="Delete CPSA Paediatric Entrustment Scale v11.1"] [aria-haspopup="dialog"] Delete… |
| O-R Scale | Standard 1-5 observation-to-entrustment scale. | 5 | [link] [aria-label="Edit O-R Scale"] Edit [button] [aria-label="Delete O-R Scale"] [aria-haspopup="dialog"] Delete… |

######## BOARD R2-C06-768.dc.html
R2-C06-768 · 768
6.6 · Demo Administrator · the list of three by name: CNSA 6, CPSA 6, O-R 5.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] [aria-current="page"] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
### Entrustment scales
Define and maintain the rating ladders used by assessment forms and committee reviews.
[link] Create scale
Entrustment scales, by name
| Name | Description | Levels | Actions |
| CNSA Neurology Entrustment Scale | Supervision ladder for CNSA EPAs. | 6 | [link] [aria-label="Edit CNSA Neurology Entrustment Scale"] Edit [button] [aria-label="Delete CNSA Neurology Entrustment Scale"] [aria-haspopup="dialog"] Delete… |
| CPSA Paediatric Entrustment Scale v11.1 | Six rungs in four bands. Only level 3 is split: the gap between "check everything with me" and "call me if you are worried" is the whole of a registrar’s third year. | 6 | [link] [aria-label="Edit CPSA Paediatric Entrustment Scale v11.1"] Edit [button] [aria-label="Delete CPSA Paediatric Entrustment Scale v11.1"] [aria-haspopup="dialog"] Delete… |
| O-R Scale | Standard 1-5 observation-to-entrustment scale. | 5 | [link] [aria-label="Edit O-R Scale"] Edit [button] [aria-label="Delete O-R Scale"] [aria-haspopup="dialog"] Delete… |

######## BOARD R2-C07.dc.html
R2-C07 · 1280
6.8 · Demo Administrator · Delete… asks (Q11, M4): the title names the scale; the second sentence is round 1's, kept, PROPOSED; confirm "Delete scale".
Focus: Cancel; Cancel or Escape returns to the row's Delete….
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] [aria-current="page"] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
### Entrustment scales
Define and maintain the rating ladders used by assessment forms and committee reviews.
[link] Create scale
Entrustment scales, by name
| Name | Description | Levels | Actions |
| CNSA Neurology Entrustment Scale | Supervision ladder for CNSA EPAs. | 6 | [link] [aria-label="Edit CNSA Neurology Entrustment Scale"] Edit [button] [aria-label="Delete CNSA Neurology Entrustment Scale"] [aria-haspopup="dialog"] Delete… |
| CPSA Paediatric Entrustment Scale v11.1 | Six rungs in four bands. Only level 3 is split: the gap between "check everything with me" and "call me if you are worried" is the whole of a registrar’s third year. | 6 | [link] [aria-label="Edit CPSA Paediatric Entrustment Scale v11.1"] Edit [button] [aria-label="Delete CPSA Paediatric Entrustment Scale v11.1"] [aria-haspopup="dialog"] Delete… |
| O-R Scale | Standard 1-5 observation-to-entrustment scale. | 5 | [link] [aria-label="Edit O-R Scale"] Edit [button] [aria-label="Delete O-R Scale"] [aria-haspopup="dialog"] Delete… |
#### Delete CNSA Neurology Entrustment Scale?
It is removed from every list. This cannot be undone.
A scale that anything uses is not deleted: you will be told what uses it, and nothing changes.
[button] Cancel
[button] Delete scale

######## BOARD R2-C07-390.dc.html
R2-C07-390 · 390
6.8 · Demo Administrator · Delete… asks (Q11, M4): the title names the scale; the second sentence is round 1's, kept, PROPOSED; confirm "Delete scale".
Focus: Cancel; Cancel or Escape returns to the row's Delete….
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] [aria-current="page"] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
### Entrustment scales
Define and maintain the rating ladders used by assessment forms and committee reviews.
[link] Create scale
Entrustment scales, by name
| Name | Description | Levels | Actions |
| CNSA Neurology Entrustment Scale | Supervision ladder for CNSA EPAs. | 6 | [link] [aria-label="Edit CNSA Neurology Entrustment Scale"] Edit [button] [aria-label="Delete CNSA Neurology Entrustment Scale"] [aria-haspopup="dialog"] Delete… |
| CPSA Paediatric Entrustment Scale v11.1 | Six rungs in four bands. Only level 3 is split: the gap between "check everything with me" and "call me if you are worried" is the whole of a registrar’s third year. | 6 | [link] [aria-label="Edit CPSA Paediatric Entrustment Scale v11.1"] Edit [button] [aria-label="Delete CPSA Paediatric Entrustment Scale v11.1"] [aria-haspopup="dialog"] Delete… |
| O-R Scale | Standard 1-5 observation-to-entrustment scale. | 5 | [link] [aria-label="Edit O-R Scale"] Edit [button] [aria-label="Delete O-R Scale"] [aria-haspopup="dialog"] Delete… |
#### Delete CNSA Neurology Entrustment Scale?
It is removed from every list. This cannot be undone.
A scale that anything uses is not deleted: you will be told what uses it, and nothing changes.
[button] Cancel
[button] Delete scale

######## BOARD R2-C08.dc.html
R2-C08 · 1280
6.8 refused · Demo Administrator · the built sentence word for word (S1) at the list's top, a danger Alert, focused; "Open Adult Neurology" (named "Open the sub-speciality Adult Neurology", PROPOSED). The scale stays at 6.
Focus: the refusal.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] [aria-current="page"] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
### Entrustment scales
Define and maintain the rating ladders used by assessment forms and committee reviews.
[link] Create scale
This entrustment scale is the default scale of the sub-speciality "Adult Neurology" (Neurology), so it cannot be deleted. Change that sub-speciality's default entrustment scale to another scale, or to no default, first.
[link] [aria-label="Open the sub-speciality Adult Neurology"] Open Adult Neurology
Entrustment scales, by name
| Name | Description | Levels | Actions |
| CNSA Neurology Entrustment Scale | Supervision ladder for CNSA EPAs. | 6 | [link] [aria-label="Edit CNSA Neurology Entrustment Scale"] Edit [button] [aria-label="Delete CNSA Neurology Entrustment Scale"] [aria-haspopup="dialog"] Delete… |
| CPSA Paediatric Entrustment Scale v11.1 | Six rungs in four bands. Only level 3 is split: the gap between "check everything with me" and "call me if you are worried" is the whole of a registrar’s third year. | 6 | [link] [aria-label="Edit CPSA Paediatric Entrustment Scale v11.1"] Edit [button] [aria-label="Delete CPSA Paediatric Entrustment Scale v11.1"] [aria-haspopup="dialog"] Delete… |
| O-R Scale | Standard 1-5 observation-to-entrustment scale. | 5 | [link] [aria-label="Edit O-R Scale"] Edit [button] [aria-label="Delete O-R Scale"] [aria-haspopup="dialog"] Delete… |

######## BOARD R2-C08-390.dc.html
R2-C08-390 · 390
6.8 refused · Demo Administrator · the built sentence word for word (S1) at the list's top, a danger Alert, focused; "Open Adult Neurology" (named "Open the sub-speciality Adult Neurology", PROPOSED). The scale stays at 6.
Focus: the refusal.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] [aria-current="page"] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
### Entrustment scales
Define and maintain the rating ladders used by assessment forms and committee reviews.
[link] Create scale
This entrustment scale is the default scale of the sub-speciality "Adult Neurology" (Neurology), so it cannot be deleted. Change that sub-speciality's default entrustment scale to another scale, or to no default, first.
[link] [aria-label="Open the sub-speciality Adult Neurology"] Open Adult Neurology
Entrustment scales, by name
| Name | Description | Levels | Actions |
| CNSA Neurology Entrustment Scale | Supervision ladder for CNSA EPAs. | 6 | [link] [aria-label="Edit CNSA Neurology Entrustment Scale"] Edit [button] [aria-label="Delete CNSA Neurology Entrustment Scale"] [aria-haspopup="dialog"] Delete… |
| CPSA Paediatric Entrustment Scale v11.1 | Six rungs in four bands. Only level 3 is split: the gap between "check everything with me" and "call me if you are worried" is the whole of a registrar’s third year. | 6 | [link] [aria-label="Edit CPSA Paediatric Entrustment Scale v11.1"] Edit [button] [aria-label="Delete CPSA Paediatric Entrustment Scale v11.1"] [aria-haspopup="dialog"] Delete… |
| O-R Scale | Standard 1-5 observation-to-entrustment scale. | 5 | [link] [aria-label="Edit O-R Scale"] Edit [button] [aria-label="Delete O-R Scale"] [aria-haspopup="dialog"] Delete… |

######## BOARD R2-C09.dc.html
IllustrationR2-C09 · 1280
Deleted · Demo Administrator · "Entrustment scale deleted." focused, the list read again (‹a scale nothing uses› gone).
Focus: the result.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] [aria-current="page"] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
### Entrustment scales
Define and maintain the rating ladders used by assessment forms and committee reviews.
[link] Create scale
Entrustment scale deleted.
Entrustment scales, by name
| Name | Description | Levels | Actions |
| CNSA Neurology Entrustment Scale | Supervision ladder for CNSA EPAs. | 6 | [link] [aria-label="Edit CNSA Neurology Entrustment Scale"] Edit [button] [aria-label="Delete CNSA Neurology Entrustment Scale"] [aria-haspopup="dialog"] Delete… |
| CPSA Paediatric Entrustment Scale v11.1 | Six rungs in four bands. Only level 3 is split: the gap between "check everything with me" and "call me if you are worried" is the whole of a registrar’s third year. | 6 | [link] [aria-label="Edit CPSA Paediatric Entrustment Scale v11.1"] Edit [button] [aria-label="Delete CPSA Paediatric Entrustment Scale v11.1"] [aria-haspopup="dialog"] Delete… |
| O-R Scale | Standard 1-5 observation-to-entrustment scale. | 5 | [link] [aria-label="Edit O-R Scale"] Edit [button] [aria-label="Delete O-R Scale"] [aria-haspopup="dialog"] Delete… |

######## BOARD R2-C09-390.dc.html
IllustrationR2-C09-390 · 390
Deleted · Demo Administrator · "Entrustment scale deleted." focused, the list read again (‹a scale nothing uses› gone).
Focus: the result.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] [aria-current="page"] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
### Entrustment scales
Define and maintain the rating ladders used by assessment forms and committee reviews.
[link] Create scale
Entrustment scale deleted.
Entrustment scales, by name
| Name | Description | Levels | Actions |
| CNSA Neurology Entrustment Scale | Supervision ladder for CNSA EPAs. | 6 | [link] [aria-label="Edit CNSA Neurology Entrustment Scale"] Edit [button] [aria-label="Delete CNSA Neurology Entrustment Scale"] [aria-haspopup="dialog"] Delete… |
| CPSA Paediatric Entrustment Scale v11.1 | Six rungs in four bands. Only level 3 is split: the gap between "check everything with me" and "call me if you are worried" is the whole of a registrar’s third year. | 6 | [link] [aria-label="Edit CPSA Paediatric Entrustment Scale v11.1"] Edit [button] [aria-label="Delete CPSA Paediatric Entrustment Scale v11.1"] [aria-haspopup="dialog"] Delete… |
| O-R Scale | Standard 1-5 observation-to-entrustment scale. | 5 | [link] [aria-label="Edit O-R Scale"] Edit [button] [aria-label="Delete O-R Scale"] [aria-haspopup="dialog"] Delete… |

######## BOARD R2-C10.dc.html
IllustrationR2-C10 · 1280
Delete refused · Demo Administrator · the CPSA ladder: the built sentence for a pinned scale.
Focus: the refusal.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] [aria-current="page"] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
### Entrustment scales
Define and maintain the rating ladders used by assessment forms and committee reviews.
[link] Create scale
This entrustment scale is pinned to one or more curriculum items and cannot be deleted.
Entrustment scales, by name
| Name | Description | Levels | Actions |
| CNSA Neurology Entrustment Scale | Supervision ladder for CNSA EPAs. | 6 | [link] [aria-label="Edit CNSA Neurology Entrustment Scale"] Edit [button] [aria-label="Delete CNSA Neurology Entrustment Scale"] [aria-haspopup="dialog"] Delete… |
| CPSA Paediatric Entrustment Scale v11.1 | Six rungs in four bands. Only level 3 is split: the gap between "check everything with me" and "call me if you are worried" is the whole of a registrar’s third year. | 6 | [link] [aria-label="Edit CPSA Paediatric Entrustment Scale v11.1"] Edit [button] [aria-label="Delete CPSA Paediatric Entrustment Scale v11.1"] [aria-haspopup="dialog"] Delete… |
| O-R Scale | Standard 1-5 observation-to-entrustment scale. | 5 | [link] [aria-label="Edit O-R Scale"] Edit [button] [aria-label="Delete O-R Scale"] [aria-haspopup="dialog"] Delete… |

######## BOARD R2-C10-390.dc.html
IllustrationR2-C10-390 · 390
Delete refused · Demo Administrator · the CPSA ladder: the built sentence for a pinned scale.
Focus: the refusal.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] [aria-current="page"] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
### Entrustment scales
Define and maintain the rating ladders used by assessment forms and committee reviews.
[link] Create scale
This entrustment scale is pinned to one or more curriculum items and cannot be deleted.
Entrustment scales, by name
| Name | Description | Levels | Actions |
| CNSA Neurology Entrustment Scale | Supervision ladder for CNSA EPAs. | 6 | [link] [aria-label="Edit CNSA Neurology Entrustment Scale"] Edit [button] [aria-label="Delete CNSA Neurology Entrustment Scale"] [aria-haspopup="dialog"] Delete… |
| CPSA Paediatric Entrustment Scale v11.1 | Six rungs in four bands. Only level 3 is split: the gap between "check everything with me" and "call me if you are worried" is the whole of a registrar’s third year. | 6 | [link] [aria-label="Edit CPSA Paediatric Entrustment Scale v11.1"] Edit [button] [aria-label="Delete CPSA Paediatric Entrustment Scale v11.1"] [aria-haspopup="dialog"] Delete… |
| O-R Scale | Standard 1-5 observation-to-entrustment scale. | 5 | [link] [aria-label="Edit O-R Scale"] Edit [button] [aria-label="Delete O-R Scale"] [aria-haspopup="dialog"] Delete… |

######## BOARD R2-C11.dc.html
IllustrationR2-C11 · 1280
Q6 (M5) · Demo Administrator · a page read before the scale came into use, so Remove was offered: rung 5 removed and saved, refused. Three items by EPA and curriculum, then "And 6 more." (nine need rung 5). Never a rank or an item id.
"Put rung 5 back" restores the row in its place and moves the focus to its Label.
Focus: the refusal (no field holds it).
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] [aria-current="page"] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Entrustment scales
-
- [aria-current="page"] CPSA Paediatric Entrustment Scale v11.1
### CPSA Paediatric Entrustment Scale v11.1
A ladder shared by every College and institution.
Not saved.
[button] Put rung 5 back
- PAED-001 on Paediatric EPA Curriculum (11.1) needs rung 5 of this scale, which the new levels leave out. Change that item's minimum first.
- PAED-002 on Paediatric EPA Curriculum (11.1) needs rung 5 of this scale, which the new levels leave out. Change that item's minimum first.
- PAED-003 on Paediatric EPA Curriculum (11.1) needs rung 5 of this scale, which the new levels leave out. Change that item's minimum first.
- And 6 more.
#### Details
Name*
[value="CPSA Paediatric Entrustment Scale v11.1"]
Description
Six rungs in four bands. Only level 3 is split: the gap between "check everything with me" and "call me if you are worried" is the whole of a registrar’s third year.
#### Levels
A rung is read by its label everywhere ("3a", "5"). Its order is its rank, lowest first, and everything that compares levels uses the order: on a ladder with a split rung, the label and the order differ. The order is shown only on this page.
Levels of CPSA Paediatric Entrustment Scale v11.1, lowest first
| Order | Label* | Description | Actions |
| 1 | Label of the rung in order 1 [value="1"] | Description of 1Not entrusted to act. The supervisor performs the activity and thinks aloud; the trainee prepares, observes actively, and may not enact or document decisions. | [button] [aria-label="Move 1 up"] [disabled] Up [button] [aria-label="Move 1 down"] Down [button] [aria-label="Remove 1"] Remove |
| 2 | Label of the rung in order 2 [value="2"] | Description of 2Direct supervision. Supervisor present throughout, sharing the task or watching silently, ready to step in. The plan may be entirely the trainee’s, but nothing is acted on without sign-off. | [button] [aria-label="Move 2 up"] Up [button] [aria-label="Move 2 down"] Down [button] [aria-label="Remove 2"] Remove |
| 3 | Label of the rung in order 3 [value="3a"] | Description of 3aIndirect supervision, decisions checked. Supervisor on site or immediately available and reviews findings, working diagnosis and plan before they are enacted. Trainee may act alone on routine assessment, investigation and supportive care within protocol. | [button] [aria-label="Move 3a up"] Up [button] [aria-label="Move 3a down"] Down [button] [aria-label="Remove 3a"] Remove |
| 4 | Label of the rung in order 4 [value="3b"] | Description of 3bIndirect supervision, reviewed on the trainee’s initiative. Supervisor available on request; the trainee decides what needs discussion and shows reliable help-seeking. Full management of the activity, supervisor informed rather than consulted. | [button] [aria-label="Move 3b up"] Up [button] [aria-label="Move 3b down"] Down [button] [aria-label="Remove 3b"] Remove |
| 5 | Label of the rung in order 5 [value="4"] | Description of 4Unsupervised practice. The trainee carries the responsibility for the activity. | [button] [aria-label="Move 4 up"] Up [button] [aria-label="Move 4 down"] [disabled] Down [button] [aria-label="Remove 4"] Remove |
[button] Add level
[link] Cancel
[button] Save

######## BOARD R2-C11-390.dc.html
IllustrationR2-C11-390 · 390
Q6 (M5) · Demo Administrator · a page read before the scale came into use, so Remove was offered: rung 5 removed and saved, refused. Three items by EPA and curriculum, then "And 6 more." (nine need rung 5). Never a rank or an item id.
"Put rung 5 back" restores the row in its place and moves the focus to its Label.
Focus: the refusal (no field holds it).
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] [aria-current="page"] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Entrustment scales
-
- [aria-current="page"] CPSA Paediatric Entrustment Scale v11.1
### CPSA Paediatric Entrustment Scale v11.1
A ladder shared by every College and institution.
Not saved.
[button] Put rung 5 back
- PAED-001 on Paediatric EPA Curriculum (11.1) needs rung 5 of this scale, which the new levels leave out. Change that item's minimum first.
- PAED-002 on Paediatric EPA Curriculum (11.1) needs rung 5 of this scale, which the new levels leave out. Change that item's minimum first.
- PAED-003 on Paediatric EPA Curriculum (11.1) needs rung 5 of this scale, which the new levels leave out. Change that item's minimum first.
- And 6 more.
#### Details
Name*
[value="CPSA Paediatric Entrustment Scale v11.1"]
Description
Six rungs in four bands. Only level 3 is split: the gap between "check everything with me" and "call me if you are worried" is the whole of a registrar’s third year.
#### Levels
A rung is read by its label everywhere ("3a", "5"). Its order is its rank, lowest first, and everything that compares levels uses the order: on a ladder with a split rung, the label and the order differ. The order is shown only on this page.
Levels of CPSA Paediatric Entrustment Scale v11.1, lowest first
| Order | Label* | Description | Actions |
| 1 | Label of the rung in order 1 [value="1"] | Description of 1Not entrusted to act. The supervisor performs the activity and thinks aloud; the trainee prepares, observes actively, and may not enact or document decisions. | [button] [aria-label="Move 1 up"] [disabled] Up [button] [aria-label="Move 1 down"] Down [button] [aria-label="Remove 1"] Remove |
| 2 | Label of the rung in order 2 [value="2"] | Description of 2Direct supervision. Supervisor present throughout, sharing the task or watching silently, ready to step in. The plan may be entirely the trainee’s, but nothing is acted on without sign-off. | [button] [aria-label="Move 2 up"] Up [button] [aria-label="Move 2 down"] Down [button] [aria-label="Remove 2"] Remove |
| 3 | Label of the rung in order 3 [value="3a"] | Description of 3aIndirect supervision, decisions checked. Supervisor on site or immediately available and reviews findings, working diagnosis and plan before they are enacted. Trainee may act alone on routine assessment, investigation and supportive care within protocol. | [button] [aria-label="Move 3a up"] Up [button] [aria-label="Move 3a down"] Down [button] [aria-label="Remove 3a"] Remove |
| 4 | Label of the rung in order 4 [value="3b"] | Description of 3bIndirect supervision, reviewed on the trainee’s initiative. Supervisor available on request; the trainee decides what needs discussion and shows reliable help-seeking. Full management of the activity, supervisor informed rather than consulted. | [button] [aria-label="Move 3b up"] Up [button] [aria-label="Move 3b down"] Down [button] [aria-label="Remove 3b"] Remove |
| 5 | Label of the rung in order 5 [value="4"] | Description of 4Unsupervised practice. The trainee carries the responsibility for the activity. | [button] [aria-label="Move 4 up"] Up [button] [aria-label="Move 4 down"] [disabled] Down [button] [aria-label="Remove 4"] Remove |
[button] Add level
[link] Cancel
[button] Save

######## BOARD R2-C12.dc.html
IllustrationR2-C12 · 1280
Q16's handler · Demo Administrator · the CNSA scale came into use (6.7) after this page was read: the reorder is refused, the rungs read again, the moves gone (NEW words).
Focus: the refusal.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] [aria-current="page"] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Entrustment scales
-
- [aria-current="page"] CNSA Neurology Entrustment Scale
### CNSA Neurology Entrustment Scale
A ladder shared by every College and institution.
Not saved. Rungs can be reordered or removed only while nothing uses this scale, and this scale is now in use. The page has been read again.
#### Details
Name*
[value="CNSA Neurology Entrustment Scale"]
Description
Supervision ladder for CNSA EPAs.
#### Levels
A rung is read by its label everywhere ("3a", "5"). Its order is its rank, lowest first, and everything that compares levels uses the order: on a ladder with a split rung, the label and the order differ. The order is shown only on this page.
Levels of CNSA Neurology Entrustment Scale, lowest first
| Order | Label* | Description |
| 1 | Label of the rung in order 1 [value="Observe only"] | Description of Observe only‹description› |
| 2 | Label of the rung in order 2 [value="Direct supervision"] | Description of Direct supervision‹description› |
| 3 | Label of the rung in order 3 [value="Indirect supervision"] | Description of Indirect supervision‹description› |
| 4 | Label of the rung in order 4 [value="Distant supervision"] | Description of Distant supervision‹description› |
| 5 | Label of the rung in order 5 [value="Unsupervised practice"] | Description of Unsupervised practice‹description› |
| 6 | Label of the rung in order 6 [value="Supervises others"] | Description of Supervises othersSupervises juniors performing the activity. |
Rungs can be reordered or removed only while nothing uses this scale.
[button] Add level
[link] Cancel
[button] Save

######## BOARD R2-C12-390.dc.html
IllustrationR2-C12-390 · 390
Q16's handler · Demo Administrator · the CNSA scale came into use (6.7) after this page was read: the reorder is refused, the rungs read again, the moves gone (NEW words).
Focus: the refusal.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] [aria-current="page"] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Entrustment scales
-
- [aria-current="page"] CNSA Neurology Entrustment Scale
### CNSA Neurology Entrustment Scale
A ladder shared by every College and institution.
Not saved. Rungs can be reordered or removed only while nothing uses this scale, and this scale is now in use. The page has been read again.
#### Details
Name*
[value="CNSA Neurology Entrustment Scale"]
Description
Supervision ladder for CNSA EPAs.
#### Levels
A rung is read by its label everywhere ("3a", "5"). Its order is its rank, lowest first, and everything that compares levels uses the order: on a ladder with a split rung, the label and the order differ. The order is shown only on this page.
Levels of CNSA Neurology Entrustment Scale, lowest first
| Order | Label* | Description |
| 1 | Label of the rung in order 1 [value="Observe only"] | Description of Observe only‹description› |
| 2 | Label of the rung in order 2 [value="Direct supervision"] | Description of Direct supervision‹description› |
| 3 | Label of the rung in order 3 [value="Indirect supervision"] | Description of Indirect supervision‹description› |
| 4 | Label of the rung in order 4 [value="Distant supervision"] | Description of Distant supervision‹description› |
| 5 | Label of the rung in order 5 [value="Unsupervised practice"] | Description of Unsupervised practice‹description› |
| 6 | Label of the rung in order 6 [value="Supervises others"] | Description of Supervises othersSupervises juniors performing the activity. |
Rungs can be reordered or removed only while nothing uses this scale.
[button] Add level
[link] Cancel
[button] Save

######## BOARD R2-C13.dc.html
IllustrationR2-C13 · 1280
An ended record's rung (T312, S3) · Demo Administrator · label words (NEW) in place of "Curriculum item {id} … requires level {n}".
Focus: the refusal.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] [aria-current="page"] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Entrustment scales
-
- [aria-current="page"] CPSA Paediatric Entrustment Scale v11.1
### CPSA Paediatric Entrustment Scale v11.1
A ladder shared by every College and institution.
Not saved. PAED-001 on Paediatric EPA Curriculum (11.1) was on the record of a programme that has ended, which keeps rung 5 of this scale as its minimum. A closed record keeps the targets it ended with, so rung 5 cannot be removed.
#### Details
Name*
[value="CPSA Paediatric Entrustment Scale v11.1"]
Description
Six rungs in four bands. Only level 3 is split: the gap between "check everything with me" and "call me if you are worried" is the whole of a registrar’s third year.
#### Levels
A rung is read by its label everywhere ("3a", "5"). Its order is its rank, lowest first, and everything that compares levels uses the order: on a ladder with a split rung, the label and the order differ. The order is shown only on this page.
Levels of CPSA Paediatric Entrustment Scale v11.1, lowest first
| Order | Label* | Description | Actions |
| 1 | Label of the rung in order 1 [value="1"] | Description of 1Not entrusted to act. The supervisor performs the activity and thinks aloud; the trainee prepares, observes actively, and may not enact or document decisions. | [button] [aria-label="Move 1 up"] [disabled] Up [button] [aria-label="Move 1 down"] Down [button] [aria-label="Remove 1"] Remove |
| 2 | Label of the rung in order 2 [value="2"] | Description of 2Direct supervision. Supervisor present throughout, sharing the task or watching silently, ready to step in. The plan may be entirely the trainee’s, but nothing is acted on without sign-off. | [button] [aria-label="Move 2 up"] Up [button] [aria-label="Move 2 down"] Down [button] [aria-label="Remove 2"] Remove |
| 3 | Label of the rung in order 3 [value="3a"] | Description of 3aIndirect supervision, decisions checked. Supervisor on site or immediately available and reviews findings, working diagnosis and plan before they are enacted. Trainee may act alone on routine assessment, investigation and supportive care within protocol. | [button] [aria-label="Move 3a up"] Up [button] [aria-label="Move 3a down"] Down [button] [aria-label="Remove 3a"] Remove |
| 4 | Label of the rung in order 4 [value="3b"] | Description of 3bIndirect supervision, reviewed on the trainee’s initiative. Supervisor available on request; the trainee decides what needs discussion and shows reliable help-seeking. Full management of the activity, supervisor informed rather than consulted. | [button] [aria-label="Move 3b up"] Up [button] [aria-label="Move 3b down"] Down [button] [aria-label="Remove 3b"] Remove |
| 5 | Label of the rung in order 5 [value="4"] | Description of 4Unsupervised practice. The trainee carries the responsibility for the activity. | [button] [aria-label="Move 4 up"] Up [button] [aria-label="Move 4 down"] [disabled] Down [button] [aria-label="Remove 4"] Remove |
[button] Add level
[link] Cancel
[button] Save

######## BOARD R2-C13-390.dc.html
IllustrationR2-C13-390 · 390
An ended record's rung (T312, S3) · Demo Administrator · label words (NEW) in place of "Curriculum item {id} … requires level {n}".
Focus: the refusal.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] [aria-current="page"] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Entrustment scales
-
- [aria-current="page"] CPSA Paediatric Entrustment Scale v11.1
### CPSA Paediatric Entrustment Scale v11.1
A ladder shared by every College and institution.
Not saved. PAED-001 on Paediatric EPA Curriculum (11.1) was on the record of a programme that has ended, which keeps rung 5 of this scale as its minimum. A closed record keeps the targets it ended with, so rung 5 cannot be removed.
#### Details
Name*
[value="CPSA Paediatric Entrustment Scale v11.1"]
Description
Six rungs in four bands. Only level 3 is split: the gap between "check everything with me" and "call me if you are worried" is the whole of a registrar’s third year.
#### Levels
A rung is read by its label everywhere ("3a", "5"). Its order is its rank, lowest first, and everything that compares levels uses the order: on a ladder with a split rung, the label and the order differ. The order is shown only on this page.
Levels of CPSA Paediatric Entrustment Scale v11.1, lowest first
| Order | Label* | Description | Actions |
| 1 | Label of the rung in order 1 [value="1"] | Description of 1Not entrusted to act. The supervisor performs the activity and thinks aloud; the trainee prepares, observes actively, and may not enact or document decisions. | [button] [aria-label="Move 1 up"] [disabled] Up [button] [aria-label="Move 1 down"] Down [button] [aria-label="Remove 1"] Remove |
| 2 | Label of the rung in order 2 [value="2"] | Description of 2Direct supervision. Supervisor present throughout, sharing the task or watching silently, ready to step in. The plan may be entirely the trainee’s, but nothing is acted on without sign-off. | [button] [aria-label="Move 2 up"] Up [button] [aria-label="Move 2 down"] Down [button] [aria-label="Remove 2"] Remove |
| 3 | Label of the rung in order 3 [value="3a"] | Description of 3aIndirect supervision, decisions checked. Supervisor on site or immediately available and reviews findings, working diagnosis and plan before they are enacted. Trainee may act alone on routine assessment, investigation and supportive care within protocol. | [button] [aria-label="Move 3a up"] Up [button] [aria-label="Move 3a down"] Down [button] [aria-label="Remove 3a"] Remove |
| 4 | Label of the rung in order 4 [value="3b"] | Description of 3bIndirect supervision, reviewed on the trainee’s initiative. Supervisor available on request; the trainee decides what needs discussion and shows reliable help-seeking. Full management of the activity, supervisor informed rather than consulted. | [button] [aria-label="Move 3b up"] Up [button] [aria-label="Move 3b down"] Down [button] [aria-label="Remove 3b"] Remove |
| 5 | Label of the rung in order 5 [value="4"] | Description of 4Unsupervised practice. The trainee carries the responsibility for the activity. | [button] [aria-label="Move 4 up"] Up [button] [aria-label="Move 4 down"] [disabled] Down [button] [aria-label="Remove 4"] Remove |
[button] Add level
[link] Cancel
[button] Save

######## BOARD R2-C14.dc.html
IllustrationR2-C14 · 1280
A taken name · Demo Administrator · under Name, the built words.
Focus: Name.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] [aria-current="page"] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Entrustment scales
-
- [aria-current="page"] Create entrustment scale
### Create entrustment scale
A ladder of at least two rungs, shared by every College and institution.
#### Details
Name*
[value="CPSA Paediatric Entrustment Scale v11.1"]
An entrustment scale named 'CPSA Paediatric Entrustment Scale v11.1' already exists.
Description
#### Levels
A rung is read by its label everywhere ("3a", "5"). Its order is its rank, lowest first, and everything that compares levels uses the order: on a ladder with a split rung, the label and the order differ. The order is shown only on this page.
Levels of CPSA Paediatric Entrustment Scale v11.1, lowest first
| Order | Label* | Description | Actions |
| 1 | Label of the rung in order 1 | Description of the rung in order 1 | [button] [aria-label="Move the rung in order 1 up"] [disabled] Up [button] [aria-label="Move the rung in order 1 down"] Down [button] [aria-label="Remove the rung in order 1"] Remove |
| 2 | Label of the rung in order 2 | Description of the rung in order 2 | [button] [aria-label="Move the rung in order 2 up"] Up [button] [aria-label="Move the rung in order 2 down"] [disabled] Down [button] [aria-label="Remove the rung in order 2"] Remove |
[button] Add level
[link] Cancel
[button] Create scale

######## BOARD R2-C14-390.dc.html
IllustrationR2-C14-390 · 390
A taken name · Demo Administrator · under Name, the built words.
Focus: Name.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] [aria-current="page"] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Entrustment scales
-
- [aria-current="page"] Create entrustment scale
### Create entrustment scale
A ladder of at least two rungs, shared by every College and institution.
#### Details
Name*
[value="CPSA Paediatric Entrustment Scale v11.1"]
An entrustment scale named 'CPSA Paediatric Entrustment Scale v11.1' already exists.
Description
#### Levels
A rung is read by its label everywhere ("3a", "5"). Its order is its rank, lowest first, and everything that compares levels uses the order: on a ladder with a split rung, the label and the order differ. The order is shown only on this page.
Levels of CPSA Paediatric Entrustment Scale v11.1, lowest first
| Order | Label* | Description | Actions |
| 1 | Label of the rung in order 1 | Description of the rung in order 1 | [button] [aria-label="Move the rung in order 1 up"] [disabled] Up [button] [aria-label="Move the rung in order 1 down"] Down [button] [aria-label="Remove the rung in order 1"] Remove |
| 2 | Label of the rung in order 2 | Description of the rung in order 2 | [button] [aria-label="Move the rung in order 2 up"] Up [button] [aria-label="Move the rung in order 2 down"] [disabled] Down [button] [aria-label="Remove the rung in order 2"] Remove |
[button] Add level
[link] Cancel
[button] Create scale

######## BOARD R2-C15a.dc.html
IllustrationR2-C15a · 1280
Required · Name empty: "Enter a name."
Focus: Name.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] [aria-current="page"] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Entrustment scales
-
- [aria-current="page"] Create entrustment scale
### Create entrustment scale
A ladder of at least two rungs, shared by every College and institution.
#### Details
Name*
Enter a name.
Description
Supervision ladder for CNSA EPAs.
#### Levels
A rung is read by its label everywhere ("3a", "5"). Its order is its rank, lowest first, and everything that compares levels uses the order: on a ladder with a split rung, the label and the order differ. The order is shown only on this page.
Levels of the new scale, lowest first
| Order | Label* | Description | Actions |
| 1 | Label of the rung in order 1 [value="Observe only"] | Description of Observe only‹description› | [button] [aria-label="Move Observe only up"] [disabled] Up [button] [aria-label="Move Observe only down"] Down [button] [aria-label="Remove Observe only"] Remove |
| 2 | Label of the rung in order 2 [value="Direct supervision"] | Description of Direct supervision‹description› | [button] [aria-label="Move Direct supervision up"] Up [button] [aria-label="Move Direct supervision down"] [disabled] Down [button] [aria-label="Remove Direct supervision"] Remove |
[button] Add level
[link] Cancel
[button] Create scale

######## BOARD R2-C15a-390.dc.html
IllustrationR2-C15a-390 · 390
Required · Name empty: "Enter a name."
Focus: Name.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] [aria-current="page"] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Entrustment scales
-
- [aria-current="page"] Create entrustment scale
### Create entrustment scale
A ladder of at least two rungs, shared by every College and institution.
#### Details
Name*
Enter a name.
Description
Supervision ladder for CNSA EPAs.
#### Levels
A rung is read by its label everywhere ("3a", "5"). Its order is its rank, lowest first, and everything that compares levels uses the order: on a ladder with a split rung, the label and the order differ. The order is shown only on this page.
Levels of the new scale, lowest first
| Order | Label* | Description | Actions |
| 1 | Label of the rung in order 1 [value="Observe only"] | Description of Observe only‹description› | [button] [aria-label="Move Observe only up"] [disabled] Up [button] [aria-label="Move Observe only down"] Down [button] [aria-label="Remove Observe only"] Remove |
| 2 | Label of the rung in order 2 [value="Direct supervision"] | Description of Direct supervision‹description› | [button] [aria-label="Move Direct supervision up"] Up [button] [aria-label="Move Direct supervision down"] [disabled] Down [button] [aria-label="Remove Direct supervision"] Remove |
[button] Add level
[link] Cancel
[button] Create scale

######## BOARD R2-C15b.dc.html
IllustrationR2-C15b · 1280
Required · a rung's Label empty: "Enter this rung's label." under it.
Focus: the second rung's Label.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] [aria-current="page"] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Entrustment scales
-
- [aria-current="page"] Create entrustment scale
### Create entrustment scale
A ladder of at least two rungs, shared by every College and institution.
#### Details
Name*
[value="CNSA Neurology Entrustment Scale"]
Description
Supervision ladder for CNSA EPAs.
#### Levels
A rung is read by its label everywhere ("3a", "5"). Its order is its rank, lowest first, and everything that compares levels uses the order: on a ladder with a split rung, the label and the order differ. The order is shown only on this page.
Levels of CNSA Neurology Entrustment Scale, lowest first
| Order | Label* | Description | Actions |
| 1 | Label of the rung in order 1 [value="Observe only"] | Description of Observe only‹description› | [button] [aria-label="Move Observe only up"] [disabled] Up [button] [aria-label="Move Observe only down"] Down [button] [aria-label="Remove Observe only"] Remove |
| 2 | Label of the rung in order 2Enter this rung's label. | Description of the rung in order 2‹description› | [button] [aria-label="Move the rung in order 2 up"] Up [button] [aria-label="Move the rung in order 2 down"] [disabled] Down [button] [aria-label="Remove the rung in order 2"] Remove |
[button] Add level
[link] Cancel
[button] Create scale

######## BOARD R2-C15b-390.dc.html
IllustrationR2-C15b-390 · 390
Required · a rung's Label empty: "Enter this rung's label." under it.
Focus: the second rung's Label.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] [aria-current="page"] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Entrustment scales
-
- [aria-current="page"] Create entrustment scale
### Create entrustment scale
A ladder of at least two rungs, shared by every College and institution.
#### Details
Name*
[value="CNSA Neurology Entrustment Scale"]
Description
Supervision ladder for CNSA EPAs.
#### Levels
A rung is read by its label everywhere ("3a", "5"). Its order is its rank, lowest first, and everything that compares levels uses the order: on a ladder with a split rung, the label and the order differ. The order is shown only on this page.
Levels of CNSA Neurology Entrustment Scale, lowest first
| Order | Label* | Description | Actions |
| 1 | Label of the rung in order 1 [value="Observe only"] | Description of Observe only‹description› | [button] [aria-label="Move Observe only up"] [disabled] Up [button] [aria-label="Move Observe only down"] Down [button] [aria-label="Remove Observe only"] Remove |
| 2 | Label of the rung in order 2Enter this rung's label. | Description of the rung in order 2‹description› | [button] [aria-label="Move the rung in order 2 up"] Up [button] [aria-label="Move the rung in order 2 down"] [disabled] Down [button] [aria-label="Remove the rung in order 2"] Remove |
[button] Add level
[link] Cancel
[button] Create scale

######## BOARD R2-C15c.dc.html
IllustrationR2-C15c · 1280
Required · one rung left: the refusal at the Levels section, above the table (no single field holds it).
Focus: the Levels refusal; Add level follows it.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] [aria-current="page"] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Entrustment scales
-
- [aria-current="page"] Create entrustment scale
### Create entrustment scale
A ladder of at least two rungs, shared by every College and institution.
#### Details
Name*
[value="CNSA Neurology Entrustment Scale"]
Description
Supervision ladder for CNSA EPAs.
#### Levels
A rung is read by its label everywhere ("3a", "5"). Its order is its rank, lowest first, and everything that compares levels uses the order: on a ladder with a split rung, the label and the order differ. The order is shown only on this page.
A scale needs at least two rungs. Add another.
Levels of CNSA Neurology Entrustment Scale, lowest first
| Order | Label* | Description | Actions |
| 1 | Label of the rung in order 1 [value="Observe only"] | Description of Observe only‹description› | [button] [aria-label="Move Observe only up"] [disabled] Up [button] [aria-label="Move Observe only down"] [disabled] Down [button] [aria-label="Remove Observe only"] Remove |
[button] Add level
[link] Cancel
[button] Create scale

######## BOARD R2-C15c-390.dc.html
IllustrationR2-C15c-390 · 390
Required · one rung left: the refusal at the Levels section, above the table (no single field holds it).
Focus: the Levels refusal; Add level follows it.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] [aria-current="page"] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Entrustment scales
-
- [aria-current="page"] Create entrustment scale
### Create entrustment scale
A ladder of at least two rungs, shared by every College and institution.
#### Details
Name*
[value="CNSA Neurology Entrustment Scale"]
Description
Supervision ladder for CNSA EPAs.
#### Levels
A rung is read by its label everywhere ("3a", "5"). Its order is its rank, lowest first, and everything that compares levels uses the order: on a ladder with a split rung, the label and the order differ. The order is shown only on this page.
A scale needs at least two rungs. Add another.
Levels of CNSA Neurology Entrustment Scale, lowest first
| Order | Label* | Description | Actions |
| 1 | Label of the rung in order 1 [value="Observe only"] | Description of Observe only‹description› | [button] [aria-label="Move Observe only up"] [disabled] Up [button] [aria-label="Move Observe only down"] [disabled] Down [button] [aria-label="Remove Observe only"] Remove |
[button] Add level
[link] Cancel
[button] Create scale

######## BOARD R2-C16.dc.html
IllustrationR2-C16 · 1280
The rename warning · Demo Administrator · the save went through (its result), then the built warning Alert with brackets.
Focus: the result.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] [aria-current="page"] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Entrustment scales
-
- [aria-current="page"] ‹new›
### ‹new›
A ladder shared by every College and institution.
Entrustment scale saved.
The form of the activity type ‹name› (‹key›, version ‹n›) names this scale by its old name, "‹old›", so since it became "‹new›" their scale fields have no ladder: the form offers a field's own options, or asks for a bare number, instead of the ladder's rungs, and credit compares bare numbers. Renaming the scale back restores them. Picking the scale again in the activity-type builder and publishing restores it for new activities only, because activities already filed stay on their version.
#### Details
Name*
[value="‹new›"]
Description
‹description›
#### Levels
A rung is read by its label everywhere ("3a", "5"). Its order is its rank, lowest first, and everything that compares levels uses the order: on a ladder with a split rung, the label and the order differ. The order is shown only on this page.
Levels of ‹new›, lowest first
| Order | Label* | Description | Actions |
| 1 | Label of the rung in order 1 [value="‹label›"] | Description of ‹label›‹description› | [button] [aria-label="Move ‹label› up"] [disabled] Up [button] [aria-label="Move ‹label› down"] Down [button] [aria-label="Remove ‹label›"] Remove |
| 2 | Label of the rung in order 2 [value="‹label›"] | Description of ‹label›‹description› | [button] [aria-label="Move ‹label› up"] Up [button] [aria-label="Move ‹label› down"] [disabled] Down [button] [aria-label="Remove ‹label›"] Remove |
[button] Add level
[link] Cancel
[button] Save

######## BOARD R2-C16-390.dc.html
IllustrationR2-C16-390 · 390
The rename warning · Demo Administrator · the save went through (its result), then the built warning Alert with brackets.
Focus: the result.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] [aria-current="page"] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Entrustment scales
-
- [aria-current="page"] ‹new›
### ‹new›
A ladder shared by every College and institution.
Entrustment scale saved.
The form of the activity type ‹name› (‹key›, version ‹n›) names this scale by its old name, "‹old›", so since it became "‹new›" their scale fields have no ladder: the form offers a field's own options, or asks for a bare number, instead of the ladder's rungs, and credit compares bare numbers. Renaming the scale back restores them. Picking the scale again in the activity-type builder and publishing restores it for new activities only, because activities already filed stay on their version.
#### Details
Name*
[value="‹new›"]
Description
‹description›
#### Levels
A rung is read by its label everywhere ("3a", "5"). Its order is its rank, lowest first, and everything that compares levels uses the order: on a ladder with a split rung, the label and the order differ. The order is shown only on this page.
Levels of ‹new›, lowest first
| Order | Label* | Description | Actions |
| 1 | Label of the rung in order 1 [value="‹label›"] | Description of ‹label›‹description› | [button] [aria-label="Move ‹label› up"] [disabled] Up [button] [aria-label="Move ‹label› down"] Down [button] [aria-label="Remove ‹label›"] Remove |
| 2 | Label of the rung in order 2 [value="‹label›"] | Description of ‹label›‹description› | [button] [aria-label="Move ‹label› up"] Up [button] [aria-label="Move ‹label› down"] [disabled] Down [button] [aria-label="Remove ‹label›"] Remove |
[button] Add level
[link] Cancel
[button] Save

######## BOARD R2-C17.dc.html
R2-C17 · 1280
Q7 · Demo Administrator · an unknown id: "Entrustment scale unavailable" (today a silent return to the list).
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] [aria-current="page"] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
- [aria-current="page"] Entrustment scales
### Entrustment scale unavailable
This entrustment scale does not exist, or you cannot open it.
[link] Go to Entrustment scales

######## BOARD R2-C17-390.dc.html
R2-C17-390 · 390
Q7 · Demo Administrator · an unknown id: "Entrustment scale unavailable" (today a silent return to the list).
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] [aria-current="page"] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
- [aria-current="page"] Entrustment scales
### Entrustment scale unavailable
This entrustment scale does not exist, or you cannot open it.
[link] Go to Entrustment scales

######## BOARD R2-C18a.dc.html
R2-C18a · 1280
Loading · the list.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] [aria-current="page"] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
### Entrustment scales
Define and maintain the rating ladders used by assessment forms and committee reviews.
Loading the entrustment scales.

######## BOARD R2-C18a-390.dc.html
R2-C18a-390 · 390
Loading · the list.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] [aria-current="page"] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
### Entrustment scales
Define and maintain the rating ladders used by assessment forms and committee reviews.
Loading the entrustment scales.

######## BOARD R2-C18b.dc.html
R2-C18b · 1280
Loading · a scale, titled "Entrustment scale".
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] [aria-current="page"] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
- [aria-current="page"] Entrustment scales
### Entrustment scale
Loading this entrustment scale.

######## BOARD R2-C18b-390.dc.html
R2-C18b-390 · 390
Loading · a scale, titled "Entrustment scale".
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] [aria-current="page"] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
- [aria-current="page"] Entrustment scales
### Entrustment scale
Loading this entrustment scale.

######## BOARD R2-C19a.dc.html
R2-C19a · 1280
Load error · the list.
Focus: Try again → the h1.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] [aria-current="page"] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
### Entrustment scales
Define and maintain the rating ladders used by assessment forms and committee reviews.
Could not load the entrustment scales. Nothing has changed. Try again, or come back in a few minutes.
[button] Try again

######## BOARD R2-C19a-390.dc.html
R2-C19a-390 · 390
Load error · the list.
Focus: Try again → the h1.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] [aria-current="page"] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
### Entrustment scales
Define and maintain the rating ladders used by assessment forms and committee reviews.
Could not load the entrustment scales. Nothing has changed. Try again, or come back in a few minutes.
[button] Try again

######## BOARD R2-C19b.dc.html
R2-C19b · 1280
Load error · a scale.
Focus: Try again → the h1.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] [aria-current="page"] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
- [aria-current="page"] Entrustment scales
### Entrustment scale
Could not load this entrustment scale. Nothing has changed. Try again, or come back in a few minutes.
[button] Try again

######## BOARD R2-C19b-390.dc.html
R2-C19b-390 · 390
Load error · a scale.
Focus: Try again → the h1.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] [aria-current="page"] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
- [aria-current="page"] Entrustment scales
### Entrustment scale
Could not load this entrustment scale. Nothing has changed. Try again, or come back in a few minutes.
[button] Try again

######## BOARD R2-C20.dc.html
IllustrationR2-C20 · 1280
Heavy · a ladder of 8 rungs with long descriptors, nothing uses it, so the moves show; each descriptor whole in its textarea.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] [aria-current="page"] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Entrustment scales
-
- [aria-current="page"] ‹scale›
### ‹scale›
A ladder shared by every College and institution.
#### Details
Name*
[value="‹scale›"]
Description
‹description›
#### Levels
A rung is read by its label everywhere ("3a", "5"). Its order is its rank, lowest first, and everything that compares levels uses the order: on a ladder with a split rung, the label and the order differ. The order is shown only on this page.
Levels of ‹scale›, lowest first
| Order | Label* | Description | Actions |
| 1 | Label of the rung in order 1 [value="‹label 1›"] | Description of ‹label 1›‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› | [button] [aria-label="Move ‹label 1› up"] [disabled] Up [button] [aria-label="Move ‹label 1› down"] Down [button] [aria-label="Remove ‹label 1›"] Remove |
| 2 | Label of the rung in order 2 [value="‹label 2›"] | Description of ‹label 2›‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› | [button] [aria-label="Move ‹label 2› up"] Up [button] [aria-label="Move ‹label 2› down"] Down [button] [aria-label="Remove ‹label 2›"] Remove |
| 3 | Label of the rung in order 3 [value="‹label 3›"] | Description of ‹label 3›‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› | [button] [aria-label="Move ‹label 3› up"] Up [button] [aria-label="Move ‹label 3› down"] Down [button] [aria-label="Remove ‹label 3›"] Remove |
| 4 | Label of the rung in order 4 [value="‹label 4›"] | Description of ‹label 4›‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› | [button] [aria-label="Move ‹label 4› up"] Up [button] [aria-label="Move ‹label 4› down"] Down [button] [aria-label="Remove ‹label 4›"] Remove |
| 5 | Label of the rung in order 5 [value="‹label 5›"] | Description of ‹label 5›‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› | [button] [aria-label="Move ‹label 5› up"] Up [button] [aria-label="Move ‹label 5› down"] Down [button] [aria-label="Remove ‹label 5›"] Remove |
| 6 | Label of the rung in order 6 [value="‹label 6›"] | Description of ‹label 6›‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› | [button] [aria-label="Move ‹label 6› up"] Up [button] [aria-label="Move ‹label 6› down"] Down [button] [aria-label="Remove ‹label 6›"] Remove |
| 7 | Label of the rung in order 7 [value="‹label 7›"] | Description of ‹label 7›‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› | [button] [aria-label="Move ‹label 7› up"] Up [button] [aria-label="Move ‹label 7› down"] Down [button] [aria-label="Remove ‹label 7›"] Remove |
| 8 | Label of the rung in order 8 [value="‹label 8›"] | Description of ‹label 8›‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› | [button] [aria-label="Move ‹label 8› up"] Up [button] [aria-label="Move ‹label 8› down"] [disabled] Down [button] [aria-label="Remove ‹label 8›"] Remove |
[button] Add level
[link] Cancel
[button] Save

######## BOARD R2-C20-390.dc.html
IllustrationR2-C20-390 · 390
Heavy · a ladder of 8 rungs with long descriptors, nothing uses it, so the moves show; each descriptor whole in its textarea.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] [aria-current="page"] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Entrustment scales
-
- [aria-current="page"] ‹scale›
### ‹scale›
A ladder shared by every College and institution.
#### Details
Name*
[value="‹scale›"]
Description
‹description›
#### Levels
A rung is read by its label everywhere ("3a", "5"). Its order is its rank, lowest first, and everything that compares levels uses the order: on a ladder with a split rung, the label and the order differ. The order is shown only on this page.
Levels of ‹scale›, lowest first
| Order | Label* | Description | Actions |
| 1 | Label of the rung in order 1 [value="‹label 1›"] | Description of ‹label 1›‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› | [button] [aria-label="Move ‹label 1› up"] [disabled] Up [button] [aria-label="Move ‹label 1› down"] Down [button] [aria-label="Remove ‹label 1›"] Remove |
| 2 | Label of the rung in order 2 [value="‹label 2›"] | Description of ‹label 2›‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› | [button] [aria-label="Move ‹label 2› up"] Up [button] [aria-label="Move ‹label 2› down"] Down [button] [aria-label="Remove ‹label 2›"] Remove |
| 3 | Label of the rung in order 3 [value="‹label 3›"] | Description of ‹label 3›‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› | [button] [aria-label="Move ‹label 3› up"] Up [button] [aria-label="Move ‹label 3› down"] Down [button] [aria-label="Remove ‹label 3›"] Remove |
| 4 | Label of the rung in order 4 [value="‹label 4›"] | Description of ‹label 4›‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› | [button] [aria-label="Move ‹label 4› up"] Up [button] [aria-label="Move ‹label 4› down"] Down [button] [aria-label="Remove ‹label 4›"] Remove |
| 5 | Label of the rung in order 5 [value="‹label 5›"] | Description of ‹label 5›‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› | [button] [aria-label="Move ‹label 5› up"] Up [button] [aria-label="Move ‹label 5› down"] Down [button] [aria-label="Remove ‹label 5›"] Remove |
| 6 | Label of the rung in order 6 [value="‹label 6›"] | Description of ‹label 6›‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› | [button] [aria-label="Move ‹label 6› up"] Up [button] [aria-label="Move ‹label 6› down"] Down [button] [aria-label="Remove ‹label 6›"] Remove |
| 7 | Label of the rung in order 7 [value="‹label 7›"] | Description of ‹label 7›‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› | [button] [aria-label="Move ‹label 7› up"] Up [button] [aria-label="Move ‹label 7› down"] Down [button] [aria-label="Remove ‹label 7›"] Remove |
| 8 | Label of the rung in order 8 [value="‹label 8›"] | Description of ‹label 8›‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› ‹descriptor of about 300 characters› | [button] [aria-label="Move ‹label 8› up"] Up [button] [aria-label="Move ‹label 8› down"] [disabled] Down [button] [aria-label="Remove ‹label 8›"] Remove |
[button] Add level
[link] Cancel
[button] Save

######## BOARD R2-C21a.dc.html
R2-C21a · 1280
1.15 (D57) · Anton Kruger · the read-only list: the reader's subtitle, the caption, no Create scale, no actions column, no link; Entrustment scales lit. Two scales.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] [aria-current="page"] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
### Entrustment scales
The rating ladders that assessment forms and committee reviews use. A Wombat administrator sets them; they are shown here for reference.
Entrustment scales, by name
| Name | Description | Levels |
| CPSA Paediatric Entrustment Scale v11.1 | Six rungs in four bands. Only level 3 is split: the gap between "check everything with me" and "call me if you are worried" is the whole of a registrar’s third year. | 6 |
| O-R Scale | Standard 1-5 observation-to-entrustment scale. | 5 |

######## BOARD R2-C21a-390.dc.html
R2-C21a-390 · 390
1.15 (D57) · Anton Kruger · the read-only list: the reader's subtitle, the caption, no Create scale, no actions column, no link; Entrustment scales lit. Two scales.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] [aria-current="page"] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
### Entrustment scales
The rating ladders that assessment forms and committee reviews use. A Wombat administrator sets them; they are shown here for reference.
Entrustment scales, by name
| Name | Description | Levels |
| CPSA Paediatric Entrustment Scale v11.1 | Six rungs in four bands. Only level 3 is split: the gap between "check everything with me" and "call me if you are worried" is the whole of a registrar’s third year. | 6 |
| O-R Scale | Standard 1-5 observation-to-entrustment scale. | 5 |

######## BOARD R2-C21a-768.dc.html
R2-C21a-768 · 768
1.15 (D57) · Anton Kruger · the read-only list: the reader's subtitle, the caption, no Create scale, no actions column, no link; Entrustment scales lit. Two scales.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] [aria-current="page"] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
### Entrustment scales
The rating ladders that assessment forms and committee reviews use. A Wombat administrator sets them; they are shown here for reference.
Entrustment scales, by name
| Name | Description | Levels |
| CPSA Paediatric Entrustment Scale v11.1 | Six rungs in four bands. Only level 3 is split: the gap between "check everything with me" and "call me if you are worried" is the whole of a registrar’s third year. | 6 |
| O-R Scale | Standard 1-5 observation-to-entrustment scale. | 5 |

######## BOARD R2-C21b.dc.html
R2-C21b · 1280
After 6.6 · Anton Kruger · three scales.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] [aria-current="page"] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
### Entrustment scales
The rating ladders that assessment forms and committee reviews use. A Wombat administrator sets them; they are shown here for reference.
Entrustment scales, by name
| Name | Description | Levels |
| CNSA Neurology Entrustment Scale | Supervision ladder for CNSA EPAs. | 6 |
| CPSA Paediatric Entrustment Scale v11.1 | Six rungs in four bands. Only level 3 is split: the gap between "check everything with me" and "call me if you are worried" is the whole of a registrar’s third year. | 6 |
| O-R Scale | Standard 1-5 observation-to-entrustment scale. | 5 |

######## BOARD R2-C21b-390.dc.html
R2-C21b-390 · 390
After 6.6 · Anton Kruger · three scales.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] [aria-current="page"] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
### Entrustment scales
The rating ladders that assessment forms and committee reviews use. A Wombat administrator sets them; they are shown here for reference.
Entrustment scales, by name
| Name | Description | Levels |
| CNSA Neurology Entrustment Scale | Supervision ladder for CNSA EPAs. | 6 |
| CPSA Paediatric Entrustment Scale v11.1 | Six rungs in four bands. Only level 3 is split: the gap between "check everything with me" and "call me if you are worried" is the whole of a registrar’s third year. | 6 |
| O-R Scale | Standard 1-5 observation-to-entrustment scale. | 5 |

######## BOARD R2-C21b-768.dc.html
R2-C21b-768 · 768
After 6.6 · Anton Kruger · three scales.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] [aria-current="page"] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
### Entrustment scales
The rating ladders that assessment forms and committee reviews use. A Wombat administrator sets them; they are shown here for reference.
Entrustment scales, by name
| Name | Description | Levels |
| CNSA Neurology Entrustment Scale | Supervision ladder for CNSA EPAs. | 6 |
| CPSA Paediatric Entrustment Scale v11.1 | Six rungs in four bands. Only level 3 is split: the gap between "check everything with me" and "call me if you are worried" is the whole of a registrar’s third year. | 6 |
| O-R Scale | Standard 1-5 observation-to-entrustment scale. | 5 |

######## BOARD R2-D01.dc.html
R2-D01 · 1280
6.13 · Anton Kruger · Create curriculum: Paediatrics / Neonatology chosen first (built), the values typed; the NEW subtitle; submit "Create curriculum" (T9). No Back link (Q9).
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Curricula
-
- [aria-current="page"] Create curriculum
### Create curriculum
A new version starts held back: no institution can adopt it until you open it.
#### Details
Sub-speciality*
[selected] Paediatrics / Neonatology
Paediatrics / Paediatrics
Name*
[value="Neonatology EPA Curriculum"]
Version*
[value="1.0"]
Effective from
[value="2026-11-02"]
Effective to
[link] Cancel
[button] Create curriculum

######## BOARD R2-D01-390.dc.html
R2-D01-390 · 390
6.13 · Anton Kruger · Create curriculum: Paediatrics / Neonatology chosen first (built), the values typed; the NEW subtitle; submit "Create curriculum" (T9). No Back link (Q9).
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Curricula
-
- [aria-current="page"] Create curriculum
### Create curriculum
A new version starts held back: no institution can adopt it until you open it.
#### Details
Sub-speciality*
[selected] Paediatrics / Neonatology
Paediatrics / Paediatrics
Name*
[value="Neonatology EPA Curriculum"]
Version*
[value="1.0"]
Effective from
[value="2026-11-02"]
Effective to
[link] Cancel
[button] Create curriculum

######## BOARD R2-D02.dc.html
R2-D02 · 1280
Required · Name and Version empty: each under its field, the label's words; no summary (T324; today a ValidationSummary, curriculum-edit--invalid).
Focus: Name.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Curricula
-
- [aria-current="page"] Create curriculum
### Create curriculum
A new version starts held back: no institution can adopt it until you open it.
#### Details
Sub-speciality*
[selected] Paediatrics / Neonatology
Paediatrics / Paediatrics
Name*
Enter a name.
Version*
Enter a version.
Effective from
[value="2026-11-02"]
Effective to
[link] Cancel
[button] Create curriculum

######## BOARD R2-D02-390.dc.html
R2-D02-390 · 390
Required · Name and Version empty: each under its field, the label's words; no summary (T324; today a ValidationSummary, curriculum-edit--invalid).
Focus: Name.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Curricula
-
- [aria-current="page"] Create curriculum
### Create curriculum
A new version starts held back: no institution can adopt it until you open it.
#### Details
Sub-speciality*
[selected] Paediatrics / Neonatology
Paediatrics / Paediatrics
Name*
Enter a name.
Version*
Enter a version.
Effective from
[value="2026-11-02"]
Effective to
[link] Cancel
[button] Create curriculum

######## BOARD R2-D03.dc.html
IllustrationR2-D03 · 1280
A duplicate · under Version, the built words, on a real unique violation only (T272).
Focus: Version.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Curricula
-
- [aria-current="page"] Create curriculum
### Create curriculum
A new version starts held back: no institution can adopt it until you open it.
#### Details
Sub-speciality*
[selected] Paediatrics / Neonatology
Paediatrics / Paediatrics
Name*
[value="Neonatology EPA Curriculum"]
Version*
[value="1.0"]
A curriculum with the same name and version already exists for this sub-speciality.
Effective from
[value="2026-11-02"]
Effective to
[link] Cancel
[button] Create curriculum

######## BOARD R2-D03-390.dc.html
IllustrationR2-D03-390 · 390
A duplicate · under Version, the built words, on a real unique violation only (T272).
Focus: Version.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Curricula
-
- [aria-current="page"] Create curriculum
### Create curriculum
A new version starts held back: no institution can adopt it until you open it.
#### Details
Sub-speciality*
[selected] Paediatrics / Neonatology
Paediatrics / Paediatrics
Name*
[value="Neonatology EPA Curriculum"]
Version*
[value="1.0"]
A curriculum with the same name and version already exists for this sub-speciality.
Effective from
[value="2026-11-02"]
Effective to
[link] Cancel
[button] Create curriculum

######## BOARD R2-D04.dc.html
IllustrationR2-D04 · 1280
A failed create · the NEW sentence.
Focus: the failure.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Curricula
-
- [aria-current="page"] Create curriculum
### Create curriculum
A new version starts held back: no institution can adopt it until you open it.
The curriculum could not be created. Nothing has changed. Try again, or come back in a few minutes.
#### Details
Sub-speciality*
[selected] Paediatrics / Neonatology
Paediatrics / Paediatrics
Name*
[value="Neonatology EPA Curriculum"]
Version*
[value="1.0"]
Effective from
[value="2026-11-02"]
Effective to
[link] Cancel
[button] Create curriculum

######## BOARD R2-D04-390.dc.html
IllustrationR2-D04-390 · 390
A failed create · the NEW sentence.
Focus: the failure.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Curricula
-
- [aria-current="page"] Create curriculum
### Create curriculum
A new version starts held back: no institution can adopt it until you open it.
The curriculum could not be created. Nothing has changed. Try again, or come back in a few minutes.
#### Details
Sub-speciality*
[selected] Paediatrics / Neonatology
Paediatrics / Paediatrics
Name*
[value="Neonatology EPA Curriculum"]
Version*
[value="1.0"]
Effective from
[value="2026-11-02"]
Effective to
[link] Cancel
[button] Create curriculum

######## BOARD R2-D05.dc.html
R2-D05 · 1280
6.13 created · Anton Kruger · by ?created (Q5), then the address replaced: Q5's result focused. Titled by name and version (T2). Open for adoption unticked with T301's help and the reach (Q2). A new version: the none intro, "1.0-copy" from 2026-11-03. Header link "Items (0)" (T4).
Focus: the result.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Curricula
-
- [aria-current="page"] Neonatology EPA Curriculum (1.0)
### Neonatology EPA Curriculum (1.0)
College of Paediatricians of South Africa · Paediatrics / Neonatology · effective 2026-11-02
[link] [aria-label="Items of Neonatology EPA Curriculum (1.0)"] Items (0)
Neonatology EPA Curriculum (1.0) created. It is held back: no institution can adopt it until you open it.
#### Details
Sub-speciality*
[selected] Paediatrics / Neonatology
Paediatrics / Paediatrics
Name*
[value="Neonatology EPA Curriculum"]
Version*
[value="1.0"]
Effective from
[value="2026-11-02"]
Effective to
#### Open for adoption
Release
(checkbox)
Open for adoption
An institution can adopt this version only while it is open. Closing it does not affect an institution that has already adopted it.
No institution has adopted this version yet.
[link] Cancel
[button] Save
#### A new version
This version has no national items yet, so a new version starts with none. It starts held back.
New version*
[value="1.0-copy"]
Effective from
[value="2026-11-03"]
Effective to
[button] [aria-label="Clone Neonatology EPA Curriculum (1.0) as a new version"] Clone

######## BOARD R2-D05-390.dc.html
R2-D05-390 · 390
6.13 created · Anton Kruger · by ?created (Q5), then the address replaced: Q5's result focused. Titled by name and version (T2). Open for adoption unticked with T301's help and the reach (Q2). A new version: the none intro, "1.0-copy" from 2026-11-03. Header link "Items (0)" (T4).
Focus: the result.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Curricula
-
- [aria-current="page"] Neonatology EPA Curriculum (1.0)
### Neonatology EPA Curriculum (1.0)
College of Paediatricians of South Africa · Paediatrics / Neonatology · effective 2026-11-02
[link] [aria-label="Items of Neonatology EPA Curriculum (1.0)"] Items (0)
Neonatology EPA Curriculum (1.0) created. It is held back: no institution can adopt it until you open it.
#### Details
Sub-speciality*
[selected] Paediatrics / Neonatology
Paediatrics / Paediatrics
Name*
[value="Neonatology EPA Curriculum"]
Version*
[value="1.0"]
Effective from
[value="2026-11-02"]
Effective to
#### Open for adoption
Release
(checkbox)
Open for adoption
An institution can adopt this version only while it is open. Closing it does not affect an institution that has already adopted it.
No institution has adopted this version yet.
[link] Cancel
[button] Save
#### A new version
This version has no national items yet, so a new version starts with none. It starts held back.
New version*
[value="1.0-copy"]
Effective from
[value="2026-11-03"]
Effective to
[button] [aria-label="Clone Neonatology EPA Curriculum (1.0) as a new version"] Clone

######## BOARD R2-D06.dc.html
R2-D06 · 1280
6.29 before the clone · Anton Kruger · 11.1's page: the box ticked, the reach (KGK, 3 registrars in training: counts and the institution, never a registrar's name), Hold back… offered (T6), A new version "11.1-copy" from 2026-01-02.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Curricula
-
- [aria-current="page"] Paediatric EPA Curriculum (11.1)
### Paediatric EPA Curriculum (11.1)
College of Paediatricians of South Africa · Paediatrics / Paediatrics · effective 2026-01-01
[link] [aria-label="Items of Paediatric EPA Curriculum (11.1)"] Items (15)
#### Details
Sub-speciality*
Paediatrics / Neonatology
[selected] Paediatrics / Paediatrics
Name*
[value="Paediatric EPA Curriculum"]
Version*
[value="11.1"]
Effective from
[value="2026-01-01"]
Effective to
#### Open for adoption
Release
(checkbox) [checked]
Open for adoption
An institution can adopt this version only while it is open. Closing it does not affect an institution that has already adopted it.
Adopted by 1 institution, Kgosi Kgari Teaching Hospital; 3 registrars in training on it.
[link] Cancel
[button] [aria-haspopup="dialog"] Hold back…
[button] Save
#### A new version
A new version copies this version's 15 national items, with their targets, minimum levels and tools. An institution's own items are never copied. It starts held back.
New version*
[value="11.1-copy"]
Effective from
[value="2026-01-02"]
Effective to
[button] [aria-label="Clone Paediatric EPA Curriculum (11.1) as a new version"] Clone

######## BOARD R2-D06-390.dc.html
R2-D06-390 · 390
6.29 before the clone · Anton Kruger · 11.1's page: the box ticked, the reach (KGK, 3 registrars in training: counts and the institution, never a registrar's name), Hold back… offered (T6), A new version "11.1-copy" from 2026-01-02.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Curricula
-
- [aria-current="page"] Paediatric EPA Curriculum (11.1)
### Paediatric EPA Curriculum (11.1)
College of Paediatricians of South Africa · Paediatrics / Paediatrics · effective 2026-01-01
[link] [aria-label="Items of Paediatric EPA Curriculum (11.1)"] Items (15)
#### Details
Sub-speciality*
Paediatrics / Neonatology
[selected] Paediatrics / Paediatrics
Name*
[value="Paediatric EPA Curriculum"]
Version*
[value="11.1"]
Effective from
[value="2026-01-01"]
Effective to
#### Open for adoption
Release
(checkbox) [checked]
Open for adoption
An institution can adopt this version only while it is open. Closing it does not affect an institution that has already adopted it.
Adopted by 1 institution, Kgosi Kgari Teaching Hospital; 3 registrars in training on it.
[link] Cancel
[button] [aria-haspopup="dialog"] Hold back…
[button] Save
#### A new version
A new version copies this version's 15 national items, with their targets, minimum levels and tools. An institution's own items are never copied. It starts held back.
New version*
[value="11.1-copy"]
Effective from
[value="2026-01-02"]
Effective to
[button] [aria-label="Clone Paediatric EPA Curriculum (11.1) as a new version"] Clone

######## BOARD R2-D07.dc.html
R2-D07 · 1280
6.29 · the clone typed: 11.2, 2026-11-02, no end.
Focus: Clone, about to be pressed.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Curricula
-
- [aria-current="page"] Paediatric EPA Curriculum (11.1)
### Paediatric EPA Curriculum (11.1)
College of Paediatricians of South Africa · Paediatrics / Paediatrics · effective 2026-01-01
[link] [aria-label="Items of Paediatric EPA Curriculum (11.1)"] Items (15)
#### Details
Sub-speciality*
Paediatrics / Neonatology
[selected] Paediatrics / Paediatrics
Name*
[value="Paediatric EPA Curriculum"]
Version*
[value="11.1"]
Effective from
[value="2026-01-01"]
Effective to
#### Open for adoption
Release
(checkbox) [checked]
Open for adoption
An institution can adopt this version only while it is open. Closing it does not affect an institution that has already adopted it.
Adopted by 1 institution, Kgosi Kgari Teaching Hospital; 3 registrars in training on it.
[link] Cancel
[button] [aria-haspopup="dialog"] Hold back…
[button] Save
#### A new version
A new version copies this version's 15 national items, with their targets, minimum levels and tools. An institution's own items are never copied. It starts held back.
New version*
[value="11.2"]
Effective from
[value="2026-11-02"]
Effective to
[button] [aria-label="Clone Paediatric EPA Curriculum (11.1) as a new version"] Clone

######## BOARD R2-D07-390.dc.html
R2-D07-390 · 390
6.29 · the clone typed: 11.2, 2026-11-02, no end.
Focus: Clone, about to be pressed.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Curricula
-
- [aria-current="page"] Paediatric EPA Curriculum (11.1)
### Paediatric EPA Curriculum (11.1)
College of Paediatricians of South Africa · Paediatrics / Paediatrics · effective 2026-01-01
[link] [aria-label="Items of Paediatric EPA Curriculum (11.1)"] Items (15)
#### Details
Sub-speciality*
Paediatrics / Neonatology
[selected] Paediatrics / Paediatrics
Name*
[value="Paediatric EPA Curriculum"]
Version*
[value="11.1"]
Effective from
[value="2026-01-01"]
Effective to
#### Open for adoption
Release
(checkbox) [checked]
Open for adoption
An institution can adopt this version only while it is open. Closing it does not affect an institution that has already adopted it.
Adopted by 1 institution, Kgosi Kgari Teaching Hospital; 3 registrars in training on it.
[link] Cancel
[button] [aria-haspopup="dialog"] Hold back…
[button] Save
#### A new version
A new version copies this version's 15 national items, with their targets, minimum levels and tools. An institution's own items are never copied. It starts held back.
New version*
[value="11.2"]
Effective from
[value="2026-11-02"]
Effective to
[button] [aria-label="Clone Paediatric EPA Curriculum (11.1) as a new version"] Clone

######## BOARD R2-D08.dc.html
R2-D08 · 1280
6.29 cloned · Anton Kruger · 11.2's page by ?cloned=2 (Q5), the clone result focused; held back; "Items (15)"; its clone section "11.2-copy" from 2026-11-03.
Focus: the result.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Curricula
-
- [aria-current="page"] Paediatric EPA Curriculum (11.2)
### Paediatric EPA Curriculum (11.2)
College of Paediatricians of South Africa · Paediatrics / Paediatrics · effective 2026-11-02
[link] [aria-label="Items of Paediatric EPA Curriculum (11.2)"] Items (15)
Paediatric EPA Curriculum (11.2) cloned from 11.1, with its 15 national items. It is held back.
#### Details
Sub-speciality*
Paediatrics / Neonatology
[selected] Paediatrics / Paediatrics
Name*
[value="Paediatric EPA Curriculum"]
Version*
[value="11.2"]
Effective from
[value="2026-11-02"]
Effective to
#### Open for adoption
Release
(checkbox)
Open for adoption
An institution can adopt this version only while it is open. Closing it does not affect an institution that has already adopted it.
No institution has adopted this version yet.
[link] Cancel
[button] Save
#### A new version
A new version copies this version's 15 national items, with their targets, minimum levels and tools. An institution's own items are never copied. It starts held back.
New version*
[value="11.2-copy"]
Effective from
[value="2026-11-03"]
Effective to
[button] [aria-label="Clone Paediatric EPA Curriculum (11.2) as a new version"] Clone

######## BOARD R2-D08-390.dc.html
R2-D08-390 · 390
6.29 cloned · Anton Kruger · 11.2's page by ?cloned=2 (Q5), the clone result focused; held back; "Items (15)"; its clone section "11.2-copy" from 2026-11-03.
Focus: the result.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Curricula
-
- [aria-current="page"] Paediatric EPA Curriculum (11.2)
### Paediatric EPA Curriculum (11.2)
College of Paediatricians of South Africa · Paediatrics / Paediatrics · effective 2026-11-02
[link] [aria-label="Items of Paediatric EPA Curriculum (11.2)"] Items (15)
Paediatric EPA Curriculum (11.2) cloned from 11.1, with its 15 national items. It is held back.
#### Details
Sub-speciality*
Paediatrics / Neonatology
[selected] Paediatrics / Paediatrics
Name*
[value="Paediatric EPA Curriculum"]
Version*
[value="11.2"]
Effective from
[value="2026-11-02"]
Effective to
#### Open for adoption
Release
(checkbox)
Open for adoption
An institution can adopt this version only while it is open. Closing it does not affect an institution that has already adopted it.
No institution has adopted this version yet.
[link] Cancel
[button] Save
#### A new version
A new version copies this version's 15 national items, with their targets, minimum levels and tools. An institution's own items are never copied. It starts held back.
New version*
[value="11.2-copy"]
Effective from
[value="2026-11-03"]
Effective to
[button] [aria-label="Clone Paediatric EPA Curriculum (11.2) as a new version"] Clone

######## BOARD R2-D09a.dc.html
IllustrationR2-D09a · 1280
The clone refused · New version empty: "Enter the new version." (NEW) under it.
Focus: New version.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Curricula
-
- [aria-current="page"] Paediatric EPA Curriculum (11.1)
### Paediatric EPA Curriculum (11.1)
College of Paediatricians of South Africa · Paediatrics / Paediatrics · effective 2026-01-01
[link] [aria-label="Items of Paediatric EPA Curriculum (11.1)"] Items (15)
#### Details
Sub-speciality*
Paediatrics / Neonatology
[selected] Paediatrics / Paediatrics
Name*
[value="Paediatric EPA Curriculum"]
Version*
[value="11.1"]
Effective from
[value="2026-01-01"]
Effective to
#### Open for adoption
Release
(checkbox) [checked]
Open for adoption
An institution can adopt this version only while it is open. Closing it does not affect an institution that has already adopted it.
Adopted by 1 institution, Kgosi Kgari Teaching Hospital; 3 registrars in training on it.
[link] Cancel
[button] [aria-haspopup="dialog"] Hold back…
[button] Save
#### A new version
A new version copies this version's 15 national items, with their targets, minimum levels and tools. An institution's own items are never copied. It starts held back.
New version*
Enter the new version.
Effective from
[value="2026-11-02"]
Effective to
[button] [aria-label="Clone Paediatric EPA Curriculum (11.1) as a new version"] Clone

######## BOARD R2-D09a-390.dc.html
IllustrationR2-D09a-390 · 390
The clone refused · New version empty: "Enter the new version." (NEW) under it.
Focus: New version.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Curricula
-
- [aria-current="page"] Paediatric EPA Curriculum (11.1)
### Paediatric EPA Curriculum (11.1)
College of Paediatricians of South Africa · Paediatrics / Paediatrics · effective 2026-01-01
[link] [aria-label="Items of Paediatric EPA Curriculum (11.1)"] Items (15)
#### Details
Sub-speciality*
Paediatrics / Neonatology
[selected] Paediatrics / Paediatrics
Name*
[value="Paediatric EPA Curriculum"]
Version*
[value="11.1"]
Effective from
[value="2026-01-01"]
Effective to
#### Open for adoption
Release
(checkbox) [checked]
Open for adoption
An institution can adopt this version only while it is open. Closing it does not affect an institution that has already adopted it.
Adopted by 1 institution, Kgosi Kgari Teaching Hospital; 3 registrars in training on it.
[link] Cancel
[button] [aria-haspopup="dialog"] Hold back…
[button] Save
#### A new version
A new version copies this version's 15 national items, with their targets, minimum levels and tools. An institution's own items are never copied. It starts held back.
New version*
Enter the new version.
Effective from
[value="2026-11-02"]
Effective to
[button] [aria-label="Clone Paediatric EPA Curriculum (11.1) as a new version"] Clone

######## BOARD R2-D09b.dc.html
IllustrationR2-D09b · 1280
The clone refused · a taken version: the built duplicate under New version.
Focus: New version.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Curricula
-
- [aria-current="page"] Paediatric EPA Curriculum (11.1)
### Paediatric EPA Curriculum (11.1)
College of Paediatricians of South Africa · Paediatrics / Paediatrics · effective 2026-01-01
[link] [aria-label="Items of Paediatric EPA Curriculum (11.1)"] Items (15)
#### Details
Sub-speciality*
Paediatrics / Neonatology
[selected] Paediatrics / Paediatrics
Name*
[value="Paediatric EPA Curriculum"]
Version*
[value="11.1"]
Effective from
[value="2026-01-01"]
Effective to
#### Open for adoption
Release
(checkbox) [checked]
Open for adoption
An institution can adopt this version only while it is open. Closing it does not affect an institution that has already adopted it.
Adopted by 1 institution, Kgosi Kgari Teaching Hospital; 3 registrars in training on it.
[link] Cancel
[button] [aria-haspopup="dialog"] Hold back…
[button] Save
#### A new version
A new version copies this version's 15 national items, with their targets, minimum levels and tools. An institution's own items are never copied. It starts held back.
New version*
[value="11.2"]
A curriculum with the same name and version already exists for this sub-speciality.
Effective from
[value="2026-11-02"]
Effective to
[button] [aria-label="Clone Paediatric EPA Curriculum (11.1) as a new version"] Clone

######## BOARD R2-D09b-390.dc.html
IllustrationR2-D09b-390 · 390
The clone refused · a taken version: the built duplicate under New version.
Focus: New version.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Curricula
-
- [aria-current="page"] Paediatric EPA Curriculum (11.1)
### Paediatric EPA Curriculum (11.1)
College of Paediatricians of South Africa · Paediatrics / Paediatrics · effective 2026-01-01
[link] [aria-label="Items of Paediatric EPA Curriculum (11.1)"] Items (15)
#### Details
Sub-speciality*
Paediatrics / Neonatology
[selected] Paediatrics / Paediatrics
Name*
[value="Paediatric EPA Curriculum"]
Version*
[value="11.1"]
Effective from
[value="2026-01-01"]
Effective to
#### Open for adoption
Release
(checkbox) [checked]
Open for adoption
An institution can adopt this version only while it is open. Closing it does not affect an institution that has already adopted it.
Adopted by 1 institution, Kgosi Kgari Teaching Hospital; 3 registrars in training on it.
[link] Cancel
[button] [aria-haspopup="dialog"] Hold back…
[button] Save
#### A new version
A new version copies this version's 15 national items, with their targets, minimum levels and tools. An institution's own items are never copied. It starts held back.
New version*
[value="11.2"]
A curriculum with the same name and version already exists for this sub-speciality.
Effective from
[value="2026-11-02"]
Effective to
[button] [aria-label="Clone Paediatric EPA Curriculum (11.1) as a new version"] Clone

######## BOARD R2-D10.dc.html
R2-D10 · 1280
6.31 before Save · Anton Kruger · 11.2 (16 items after 6.30): Open for adoption ticked, unsaved. Opening asks nothing (Q4).
Focus: Save.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Curricula
-
- [aria-current="page"] Paediatric EPA Curriculum (11.2)
### Paediatric EPA Curriculum (11.2)
College of Paediatricians of South Africa · Paediatrics / Paediatrics · effective 2026-11-02
[link] [aria-label="Items of Paediatric EPA Curriculum (11.2)"] Items (16)
#### Details
Sub-speciality*
Paediatrics / Neonatology
[selected] Paediatrics / Paediatrics
Name*
[value="Paediatric EPA Curriculum"]
Version*
[value="11.2"]
Effective from
[value="2026-11-02"]
Effective to
#### Open for adoption
Release
(checkbox) [checked]
Open for adoption
An institution can adopt this version only while it is open. Closing it does not affect an institution that has already adopted it.
No institution has adopted this version yet.
[link] Cancel
[button] Save
#### A new version
A new version copies this version's 16 national items, with their targets, minimum levels and tools. An institution's own items are never copied. It starts held back.
New version*
[value="11.2-copy"]
Effective from
[value="2026-11-03"]
Effective to
[button] [aria-label="Clone Paediatric EPA Curriculum (11.2) as a new version"] Clone

######## BOARD R2-D10-390.dc.html
R2-D10-390 · 390
6.31 before Save · Anton Kruger · 11.2 (16 items after 6.30): Open for adoption ticked, unsaved. Opening asks nothing (Q4).
Focus: Save.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Curricula
-
- [aria-current="page"] Paediatric EPA Curriculum (11.2)
### Paediatric EPA Curriculum (11.2)
College of Paediatricians of South Africa · Paediatrics / Paediatrics · effective 2026-11-02
[link] [aria-label="Items of Paediatric EPA Curriculum (11.2)"] Items (16)
#### Details
Sub-speciality*
Paediatrics / Neonatology
[selected] Paediatrics / Paediatrics
Name*
[value="Paediatric EPA Curriculum"]
Version*
[value="11.2"]
Effective from
[value="2026-11-02"]
Effective to
#### Open for adoption
Release
(checkbox) [checked]
Open for adoption
An institution can adopt this version only while it is open. Closing it does not affect an institution that has already adopted it.
No institution has adopted this version yet.
[link] Cancel
[button] Save
#### A new version
A new version copies this version's 16 national items, with their targets, minimum levels and tools. An institution's own items are never copied. It starts held back.
New version*
[value="11.2-copy"]
Effective from
[value="2026-11-03"]
Effective to
[button] [aria-label="Clone Paediatric EPA Curriculum (11.2) as a new version"] Clone

######## BOARD R2-D11.dc.html
R2-D11 · 1280
6.31 saved · Anton Kruger · T7's opening result focused; the box ticked; Hold back… now offered; no institution has adopted it yet.
Focus: the result.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Curricula
-
- [aria-current="page"] Paediatric EPA Curriculum (11.2)
### Paediatric EPA Curriculum (11.2)
College of Paediatricians of South Africa · Paediatrics / Paediatrics · effective 2026-11-02
[link] [aria-label="Items of Paediatric EPA Curriculum (11.2)"] Items (16)
Curriculum saved. It is open for adoption: an institution can adopt it now.
#### Details
Sub-speciality*
Paediatrics / Neonatology
[selected] Paediatrics / Paediatrics
Name*
[value="Paediatric EPA Curriculum"]
Version*
[value="11.2"]
Effective from
[value="2026-11-02"]
Effective to
#### Open for adoption
Release
(checkbox) [checked]
Open for adoption
An institution can adopt this version only while it is open. Closing it does not affect an institution that has already adopted it.
No institution has adopted this version yet.
[link] Cancel
[button] [aria-haspopup="dialog"] Hold back…
[button] Save
#### A new version
A new version copies this version's 16 national items, with their targets, minimum levels and tools. An institution's own items are never copied. It starts held back.
New version*
[value="11.2-copy"]
Effective from
[value="2026-11-03"]
Effective to
[button] [aria-label="Clone Paediatric EPA Curriculum (11.2) as a new version"] Clone

######## BOARD R2-D11-390.dc.html
R2-D11-390 · 390
6.31 saved · Anton Kruger · T7's opening result focused; the box ticked; Hold back… now offered; no institution has adopted it yet.
Focus: the result.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Curricula
-
- [aria-current="page"] Paediatric EPA Curriculum (11.2)
### Paediatric EPA Curriculum (11.2)
College of Paediatricians of South Africa · Paediatrics / Paediatrics · effective 2026-11-02
[link] [aria-label="Items of Paediatric EPA Curriculum (11.2)"] Items (16)
Curriculum saved. It is open for adoption: an institution can adopt it now.
#### Details
Sub-speciality*
Paediatrics / Neonatology
[selected] Paediatrics / Paediatrics
Name*
[value="Paediatric EPA Curriculum"]
Version*
[value="11.2"]
Effective from
[value="2026-11-02"]
Effective to
#### Open for adoption
Release
(checkbox) [checked]
Open for adoption
An institution can adopt this version only while it is open. Closing it does not affect an institution that has already adopted it.
No institution has adopted this version yet.
[link] Cancel
[button] [aria-haspopup="dialog"] Hold back…
[button] Save
#### A new version
A new version copies this version's 16 national items, with their targets, minimum levels and tools. An institution's own items are never copied. It starts held back.
New version*
[value="11.2-copy"]
Effective from
[value="2026-11-03"]
Effective to
[button] [aria-label="Clone Paediatric EPA Curriculum (11.2) as a new version"] Clone

######## BOARD R2-D12.dc.html
IllustrationR2-D12 · 1280
Hold back… on 11.1 (Q4, T6) · Anton Kruger · one ConfirmDialog, opening on Cancel, naming who has adopted it.
Focus: Cancel; Cancel or Escape returns to Hold back….
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Curricula
-
- [aria-current="page"] Paediatric EPA Curriculum (11.1)
### Paediatric EPA Curriculum (11.1)
College of Paediatricians of South Africa · Paediatrics / Paediatrics · effective 2026-01-01
[link] [aria-label="Items of Paediatric EPA Curriculum (11.1)"] Items (15)
#### Details
Sub-speciality*
Paediatrics / Neonatology
[selected] Paediatrics / Paediatrics
Name*
[value="Paediatric EPA Curriculum"]
Version*
[value="11.1"]
Effective from
[value="2026-01-01"]
Effective to
#### Open for adoption
Release
(checkbox) [checked]
Open for adoption
An institution can adopt this version only while it is open. Closing it does not affect an institution that has already adopted it.
Adopted by 1 institution, Kgosi Kgari Teaching Hospital; 3 registrars in training on it.
[link] Cancel
[button] [aria-haspopup="dialog"] Hold back…
[button] Save
#### A new version
A new version copies this version's 15 national items, with their targets, minimum levels and tools. An institution's own items are never copied. It starts held back.
New version*
[value="11.1-copy"]
Effective from
[value="2026-01-02"]
Effective to
[button] [aria-label="Clone Paediatric EPA Curriculum (11.1) as a new version"] Clone
#### Hold back Paediatric EPA Curriculum (11.1)?
No institution can adopt it while it is held back.
Adopted by 1 institution, Kgosi Kgari Teaching Hospital; 3 registrars in training on it. Holding it back changes nothing for them: Kgosi Kgari Teaching Hospital keeps it, and its registrars stay on it.
You can open it again on this page.
[button] Cancel
[button] Hold back

######## BOARD R2-D12-390.dc.html
IllustrationR2-D12-390 · 390
Hold back… on 11.1 (Q4, T6) · Anton Kruger · one ConfirmDialog, opening on Cancel, naming who has adopted it.
Focus: Cancel; Cancel or Escape returns to Hold back….
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Curricula
-
- [aria-current="page"] Paediatric EPA Curriculum (11.1)
### Paediatric EPA Curriculum (11.1)
College of Paediatricians of South Africa · Paediatrics / Paediatrics · effective 2026-01-01
[link] [aria-label="Items of Paediatric EPA Curriculum (11.1)"] Items (15)
#### Details
Sub-speciality*
Paediatrics / Neonatology
[selected] Paediatrics / Paediatrics
Name*
[value="Paediatric EPA Curriculum"]
Version*
[value="11.1"]
Effective from
[value="2026-01-01"]
Effective to
#### Open for adoption
Release
(checkbox) [checked]
Open for adoption
An institution can adopt this version only while it is open. Closing it does not affect an institution that has already adopted it.
Adopted by 1 institution, Kgosi Kgari Teaching Hospital; 3 registrars in training on it.
[link] Cancel
[button] [aria-haspopup="dialog"] Hold back…
[button] Save
#### A new version
A new version copies this version's 15 national items, with their targets, minimum levels and tools. An institution's own items are never copied. It starts held back.
New version*
[value="11.1-copy"]
Effective from
[value="2026-01-02"]
Effective to
[button] [aria-label="Clone Paediatric EPA Curriculum (11.1) as a new version"] Clone
#### Hold back Paediatric EPA Curriculum (11.1)?
No institution can adopt it while it is held back.
Adopted by 1 institution, Kgosi Kgari Teaching Hospital; 3 registrars in training on it. Holding it back changes nothing for them: Kgosi Kgari Teaching Hospital keeps it, and its registrars stay on it.
You can open it again on this page.
[button] Cancel
[button] Hold back

######## BOARD R2-D13.dc.html
IllustrationR2-D13 · 1280
Held back · Anton Kruger · T7's result focused; the box unticked; Hold back… gone; KGK keeps it (the reach unchanged).
Focus: the result.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Curricula
-
- [aria-current="page"] Paediatric EPA Curriculum (11.1)
### Paediatric EPA Curriculum (11.1)
College of Paediatricians of South Africa · Paediatrics / Paediatrics · effective 2026-01-01
[link] [aria-label="Items of Paediatric EPA Curriculum (11.1)"] Items (15)
Curriculum saved. It is held back: no institution can adopt it until you open it again.
#### Details
Sub-speciality*
Paediatrics / Neonatology
[selected] Paediatrics / Paediatrics
Name*
[value="Paediatric EPA Curriculum"]
Version*
[value="11.1"]
Effective from
[value="2026-01-01"]
Effective to
#### Open for adoption
Release
(checkbox)
Open for adoption
An institution can adopt this version only while it is open. Closing it does not affect an institution that has already adopted it.
Adopted by 1 institution, Kgosi Kgari Teaching Hospital; 3 registrars in training on it.
[link] Cancel
[button] Save
#### A new version
A new version copies this version's 15 national items, with their targets, minimum levels and tools. An institution's own items are never copied. It starts held back.
New version*
[value="11.1-copy"]
Effective from
[value="2026-01-02"]
Effective to
[button] [aria-label="Clone Paediatric EPA Curriculum (11.1) as a new version"] Clone

######## BOARD R2-D13-390.dc.html
IllustrationR2-D13-390 · 390
Held back · Anton Kruger · T7's result focused; the box unticked; Hold back… gone; KGK keeps it (the reach unchanged).
Focus: the result.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Curricula
-
- [aria-current="page"] Paediatric EPA Curriculum (11.1)
### Paediatric EPA Curriculum (11.1)
College of Paediatricians of South Africa · Paediatrics / Paediatrics · effective 2026-01-01
[link] [aria-label="Items of Paediatric EPA Curriculum (11.1)"] Items (15)
Curriculum saved. It is held back: no institution can adopt it until you open it again.
#### Details
Sub-speciality*
Paediatrics / Neonatology
[selected] Paediatrics / Paediatrics
Name*
[value="Paediatric EPA Curriculum"]
Version*
[value="11.1"]
Effective from
[value="2026-01-01"]
Effective to
#### Open for adoption
Release
(checkbox)
Open for adoption
An institution can adopt this version only while it is open. Closing it does not affect an institution that has already adopted it.
Adopted by 1 institution, Kgosi Kgari Teaching Hospital; 3 registrars in training on it.
[link] Cancel
[button] Save
#### A new version
A new version copies this version's 15 national items, with their targets, minimum levels and tools. An institution's own items are never copied. It starts held back.
New version*
[value="11.1-copy"]
Effective from
[value="2026-01-02"]
Effective to
[button] [aria-label="Clone Paediatric EPA Curriculum (11.1) as a new version"] Clone

######## BOARD R2-D14.dc.html
IllustrationR2-D14 · 1280
Save with 11.1's box unticked · the same dialog (the box unticked, unsaved, under it).
Focus: Cancel; the box stays unticked and unsaved, Save asks again.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Curricula
-
- [aria-current="page"] Paediatric EPA Curriculum (11.1)
### Paediatric EPA Curriculum (11.1)
College of Paediatricians of South Africa · Paediatrics / Paediatrics · effective 2026-01-01
[link] [aria-label="Items of Paediatric EPA Curriculum (11.1)"] Items (15)
#### Details
Sub-speciality*
Paediatrics / Neonatology
[selected] Paediatrics / Paediatrics
Name*
[value="Paediatric EPA Curriculum"]
Version*
[value="11.1"]
Effective from
[value="2026-01-01"]
Effective to
#### Open for adoption
Release
(checkbox)
Open for adoption
An institution can adopt this version only while it is open. Closing it does not affect an institution that has already adopted it.
Adopted by 1 institution, Kgosi Kgari Teaching Hospital; 3 registrars in training on it.
[link] Cancel
[button] [aria-haspopup="dialog"] Hold back…
[button] Save
#### A new version
A new version copies this version's 15 national items, with their targets, minimum levels and tools. An institution's own items are never copied. It starts held back.
New version*
[value="11.1-copy"]
Effective from
[value="2026-01-02"]
Effective to
[button] [aria-label="Clone Paediatric EPA Curriculum (11.1) as a new version"] Clone
#### Hold back Paediatric EPA Curriculum (11.1)?
No institution can adopt it while it is held back.
Adopted by 1 institution, Kgosi Kgari Teaching Hospital; 3 registrars in training on it. Holding it back changes nothing for them: Kgosi Kgari Teaching Hospital keeps it, and its registrars stay on it.
You can open it again on this page.
[button] Cancel
[button] Hold back

######## BOARD R2-D14-390.dc.html
IllustrationR2-D14-390 · 390
Save with 11.1's box unticked · the same dialog (the box unticked, unsaved, under it).
Focus: Cancel; the box stays unticked and unsaved, Save asks again.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Curricula
-
- [aria-current="page"] Paediatric EPA Curriculum (11.1)
### Paediatric EPA Curriculum (11.1)
College of Paediatricians of South Africa · Paediatrics / Paediatrics · effective 2026-01-01
[link] [aria-label="Items of Paediatric EPA Curriculum (11.1)"] Items (15)
#### Details
Sub-speciality*
Paediatrics / Neonatology
[selected] Paediatrics / Paediatrics
Name*
[value="Paediatric EPA Curriculum"]
Version*
[value="11.1"]
Effective from
[value="2026-01-01"]
Effective to
#### Open for adoption
Release
(checkbox)
Open for adoption
An institution can adopt this version only while it is open. Closing it does not affect an institution that has already adopted it.
Adopted by 1 institution, Kgosi Kgari Teaching Hospital; 3 registrars in training on it.
[link] Cancel
[button] [aria-haspopup="dialog"] Hold back…
[button] Save
#### A new version
A new version copies this version's 15 national items, with their targets, minimum levels and tools. An institution's own items are never copied. It starts held back.
New version*
[value="11.1-copy"]
Effective from
[value="2026-01-02"]
Effective to
[button] [aria-label="Clone Paediatric EPA Curriculum (11.1) as a new version"] Clone
#### Hold back Paediatric EPA Curriculum (11.1)?
No institution can adopt it while it is held back.
Adopted by 1 institution, Kgosi Kgari Teaching Hospital; 3 registrars in training on it. Holding it back changes nothing for them: Kgosi Kgari Teaching Hospital keeps it, and its registrars stay on it.
You can open it again on this page.
[button] Cancel
[button] Hold back

######## BOARD R2-D15.dc.html
R2-D15 · 1280
Q7 · Anton Kruger types /admin/curricula/1 (the Demo IM Core curriculum): "Curriculum unavailable", built words (today /not-found).
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
- [aria-current="page"] Curricula
### Curriculum unavailable
This curriculum does not exist, or you cannot open it.
[link] Go to Curricula

######## BOARD R2-D15-390.dc.html
R2-D15-390 · 390
Q7 · Anton Kruger types /admin/curricula/1 (the Demo IM Core curriculum): "Curriculum unavailable", built words (today /not-found).
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
- [aria-current="page"] Curricula
### Curriculum unavailable
This curriculum does not exist, or you cannot open it.
[link] Go to Curricula

######## BOARD R2-D16.dc.html
R2-D16 · 1280
Loading · titled "Curriculum", the loading line over a skeleton; no action offered.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
- [aria-current="page"] Curricula
### Curriculum
Loading this curriculum.

######## BOARD R2-D16-390.dc.html
R2-D16-390 · 390
Loading · titled "Curriculum", the loading line over a skeleton; no action offered.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
- [aria-current="page"] Curricula
### Curriculum
Loading this curriculum.

######## BOARD R2-D17.dc.html
R2-D17 · 1280
Load error · no form under it.
Focus: Try again → the h1 once it reads.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
- [aria-current="page"] Curricula
### Curriculum
Could not load this curriculum. Nothing has changed. Try again, or come back in a few minutes.
[button] Try again

######## BOARD R2-D17-390.dc.html
R2-D17-390 · 390
Load error · no form under it.
Focus: Try again → the h1 once it reads.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
- [aria-current="page"] Curricula
### Curriculum
Could not load this curriculum. Nothing has changed. Try again, or come back in a few minutes.
[button] Try again

######## BOARD R2-D18.dc.html
IllustrationR2-D18 · 1280
Demo Administrator on 11.2's page · the same page; the College named in the subtitle; the reach after 6.31 (none yet).
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Curricula
-
- [aria-current="page"] Paediatric EPA Curriculum (11.2)
### Paediatric EPA Curriculum (11.2)
College of Paediatricians of South Africa · Paediatrics / Paediatrics · effective 2026-11-02
[link] [aria-label="Items of Paediatric EPA Curriculum (11.2)"] Items (16)
#### Details
Sub-speciality*
Paediatrics / Neonatology
[selected] Paediatrics / Paediatrics
Name*
[value="Paediatric EPA Curriculum"]
Version*
[value="11.2"]
Effective from
[value="2026-11-02"]
Effective to
#### Open for adoption
Release
(checkbox) [checked]
Open for adoption
An institution can adopt this version only while it is open. Closing it does not affect an institution that has already adopted it.
No institution has adopted this version yet.
[link] Cancel
[button] [aria-haspopup="dialog"] Hold back…
[button] Save
#### A new version
A new version copies this version's 16 national items, with their targets, minimum levels and tools. An institution's own items are never copied. It starts held back.
New version*
[value="11.2-copy"]
Effective from
[value="2026-11-03"]
Effective to
[button] [aria-label="Clone Paediatric EPA Curriculum (11.2) as a new version"] Clone

######## BOARD R2-D18-390.dc.html
IllustrationR2-D18-390 · 390
Demo Administrator on 11.2's page · the same page; the College named in the subtitle; the reach after 6.31 (none yet).
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Curricula
-
- [aria-current="page"] Paediatric EPA Curriculum (11.2)
### Paediatric EPA Curriculum (11.2)
College of Paediatricians of South Africa · Paediatrics / Paediatrics · effective 2026-11-02
[link] [aria-label="Items of Paediatric EPA Curriculum (11.2)"] Items (16)
#### Details
Sub-speciality*
Paediatrics / Neonatology
[selected] Paediatrics / Paediatrics
Name*
[value="Paediatric EPA Curriculum"]
Version*
[value="11.2"]
Effective from
[value="2026-11-02"]
Effective to
#### Open for adoption
Release
(checkbox) [checked]
Open for adoption
An institution can adopt this version only while it is open. Closing it does not affect an institution that has already adopted it.
No institution has adopted this version yet.
[link] Cancel
[button] [aria-haspopup="dialog"] Hold back…
[button] Save
#### A new version
A new version copies this version's 16 national items, with their targets, minimum levels and tools. An institution's own items are never copied. It starts held back.
New version*
[value="11.2-copy"]
Effective from
[value="2026-11-03"]
Effective to
[button] [aria-label="Clone Paediatric EPA Curriculum (11.2) as a new version"] Clone

######## BOARD R2-D19.dc.html
R2-D19 · 1280
Effective to before Effective from · refused under Effective to (NEW).
Focus: Effective to.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Curricula
-
- [aria-current="page"] Neonatology EPA Curriculum (1.0)
### Neonatology EPA Curriculum (1.0)
College of Paediatricians of South Africa · Paediatrics / Neonatology · effective 2026-11-02
[link] [aria-label="Items of Neonatology EPA Curriculum (1.0)"] Items (0)
#### Details
Sub-speciality*
[selected] Paediatrics / Neonatology
Paediatrics / Paediatrics
Name*
[value="Neonatology EPA Curriculum"]
Version*
[value="1.0"]
Effective from
[value="2026-11-02"]
Effective to
[value="2026-10-01"]
Effective to must be on or after Effective from.
#### Open for adoption
Release
(checkbox)
Open for adoption
An institution can adopt this version only while it is open. Closing it does not affect an institution that has already adopted it.
No institution has adopted this version yet.
[link] Cancel
[button] Save
#### A new version
This version has no national items yet, so a new version starts with none. It starts held back.
New version*
[value="1.0-copy"]
Effective from
[value="2026-11-03"]
Effective to
[button] [aria-label="Clone Neonatology EPA Curriculum (1.0) as a new version"] Clone

######## BOARD R2-D19-390.dc.html
R2-D19-390 · 390
Effective to before Effective from · refused under Effective to (NEW).
Focus: Effective to.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Curricula
-
- [aria-current="page"] Neonatology EPA Curriculum (1.0)
### Neonatology EPA Curriculum (1.0)
College of Paediatricians of South Africa · Paediatrics / Neonatology · effective 2026-11-02
[link] [aria-label="Items of Neonatology EPA Curriculum (1.0)"] Items (0)
#### Details
Sub-speciality*
[selected] Paediatrics / Neonatology
Paediatrics / Paediatrics
Name*
[value="Neonatology EPA Curriculum"]
Version*
[value="1.0"]
Effective from
[value="2026-11-02"]
Effective to
[value="2026-10-01"]
Effective to must be on or after Effective from.
#### Open for adoption
Release
(checkbox)
Open for adoption
An institution can adopt this version only while it is open. Closing it does not affect an institution that has already adopted it.
No institution has adopted this version yet.
[link] Cancel
[button] Save
#### A new version
This version has no national items yet, so a new version starts with none. It starts held back.
New version*
[value="1.0-copy"]
Effective from
[value="2026-11-03"]
Effective to
[button] [aria-label="Clone Neonatology EPA Curriculum (1.0) as a new version"] Clone

######## BOARD R2-E01.dc.html
R2-E01 · 1280
1.16 · Anton Kruger · the College's fifteen: the NEW subtitle and caption (T326); no College column (T3); the EPA a link, its Edit named for it; Create EPA beside the h1 at 1280 (checked against the long subtitle).
Stacks from 1024 px down (built). Pager only past twenty rows.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] [aria-current="page"] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
### EPAs
College of Paediatricians of South Africa's EPAs. A change to one is read at once wherever it is named, in every institution that has adopted a curriculum that holds it.
[link] Create EPA
College of Paediatricians of South Africa's EPAs
| EPA | Sub-speciality | Status | Actions |
| [link] PAED-001 — Providing paediatric emergency care to children | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-001"] Edit |
| [link] PAED-002 — Managing common paediatric presentations | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-002"] Edit |
| [link] PAED-003 — Providing intensive care to children | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-003"] Edit |
| [link] PAED-004 — Managing common neonatal conditions | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-004"] Edit |
| [link] PAED-005 — Providing neonatal care in intensive and high-care settings | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-005"] Edit |
| [link] PAED-006 — Managing long-term health conditions (LTHCs) | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-006"] Edit |
| [link] PAED-007 — Maintaining and promoting the health and well-being of children | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-007"] Edit |
| [link] PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-008"] Edit |
| [link] PAED-009 — Providing care for an adolescent patient | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-009"] Edit |
| [link] PAED-010 — Leading and operating within a clinical team | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-010"] Edit |
| [link] PAED-011 — Managing population health challenges | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-011"] Edit |
| [link] PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-012"] Edit |
| [link] PAED-013 — Managing child protection, legal and ethical aspects of children at risk | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-013"] Edit |
| [link] PAED-014 — Providing palliative and end-of-life care | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-014"] Edit |
| [link] PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-015"] Edit |

######## BOARD R2-E01-390.dc.html
R2-E01-390 · 390
1.16 · Anton Kruger · the College's fifteen: the NEW subtitle and caption (T326); no College column (T3); the EPA a link, its Edit named for it; Create EPA beside the h1 at 1280 (checked against the long subtitle).
Stacks from 1024 px down (built). Pager only past twenty rows.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] [aria-current="page"] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
### EPAs
College of Paediatricians of South Africa's EPAs. A change to one is read at once wherever it is named, in every institution that has adopted a curriculum that holds it.
[link] Create EPA
College of Paediatricians of South Africa's EPAs
| EPA | Sub-speciality | Status | Actions |
| [link] PAED-001 — Providing paediatric emergency care to children | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-001"] Edit |
| [link] PAED-002 — Managing common paediatric presentations | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-002"] Edit |
| [link] PAED-003 — Providing intensive care to children | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-003"] Edit |
| [link] PAED-004 — Managing common neonatal conditions | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-004"] Edit |
| [link] PAED-005 — Providing neonatal care in intensive and high-care settings | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-005"] Edit |
| [link] PAED-006 — Managing long-term health conditions (LTHCs) | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-006"] Edit |
| [link] PAED-007 — Maintaining and promoting the health and well-being of children | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-007"] Edit |
| [link] PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-008"] Edit |
| [link] PAED-009 — Providing care for an adolescent patient | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-009"] Edit |
| [link] PAED-010 — Leading and operating within a clinical team | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-010"] Edit |
| [link] PAED-011 — Managing population health challenges | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-011"] Edit |
| [link] PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-012"] Edit |
| [link] PAED-013 — Managing child protection, legal and ethical aspects of children at risk | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-013"] Edit |
| [link] PAED-014 — Providing palliative and end-of-life care | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-014"] Edit |
| [link] PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-015"] Edit |

######## BOARD R2-E01-768.dc.html
R2-E01-768 · 768
1.16 · Anton Kruger · the College's fifteen: the NEW subtitle and caption (T326); no College column (T3); the EPA a link, its Edit named for it; Create EPA beside the h1 at 1280 (checked against the long subtitle).
Stacks from 1024 px down (built). Pager only past twenty rows.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] [aria-current="page"] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
### EPAs
College of Paediatricians of South Africa's EPAs. A change to one is read at once wherever it is named, in every institution that has adopted a curriculum that holds it.
[link] Create EPA
College of Paediatricians of South Africa's EPAs
| EPA | Sub-speciality | Status | Actions |
| [link] PAED-001 — Providing paediatric emergency care to children | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-001"] Edit |
| [link] PAED-002 — Managing common paediatric presentations | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-002"] Edit |
| [link] PAED-003 — Providing intensive care to children | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-003"] Edit |
| [link] PAED-004 — Managing common neonatal conditions | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-004"] Edit |
| [link] PAED-005 — Providing neonatal care in intensive and high-care settings | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-005"] Edit |
| [link] PAED-006 — Managing long-term health conditions (LTHCs) | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-006"] Edit |
| [link] PAED-007 — Maintaining and promoting the health and well-being of children | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-007"] Edit |
| [link] PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-008"] Edit |
| [link] PAED-009 — Providing care for an adolescent patient | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-009"] Edit |
| [link] PAED-010 — Leading and operating within a clinical team | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-010"] Edit |
| [link] PAED-011 — Managing population health challenges | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-011"] Edit |
| [link] PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-012"] Edit |
| [link] PAED-013 — Managing child protection, legal and ethical aspects of children at risk | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-013"] Edit |
| [link] PAED-014 — Providing palliative and end-of-life care | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-014"] Edit |
| [link] PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-015"] Edit |

######## BOARD R2-E02.dc.html
R2-E02 · 1280
6.17 · Anton Kruger · PAED-012 Inactive (after 6.14: PAED-006 "… in children").
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] [aria-current="page"] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
### EPAs
College of Paediatricians of South Africa's EPAs. A change to one is read at once wherever it is named, in every institution that has adopted a curriculum that holds it.
[link] Create EPA
College of Paediatricians of South Africa's EPAs
| EPA | Sub-speciality | Status | Actions |
| [link] PAED-001 — Providing paediatric emergency care to children | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-001"] Edit |
| [link] PAED-002 — Managing common paediatric presentations | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-002"] Edit |
| [link] PAED-003 — Providing intensive care to children | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-003"] Edit |
| [link] PAED-004 — Managing common neonatal conditions | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-004"] Edit |
| [link] PAED-005 — Providing neonatal care in intensive and high-care settings | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-005"] Edit |
| [link] PAED-006 — Managing long-term health conditions (LTHCs) in children | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-006"] Edit |
| [link] PAED-007 — Maintaining and promoting the health and well-being of children | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-007"] Edit |
| [link] PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-008"] Edit |
| [link] PAED-009 — Providing care for an adolescent patient | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-009"] Edit |
| [link] PAED-010 — Leading and operating within a clinical team | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-010"] Edit |
| [link] PAED-011 — Managing population health challenges | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-011"] Edit |
| [link] PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | Paediatrics / Paediatrics | Inactive | [link] [aria-label="Edit PAED-012"] Edit |
| [link] PAED-013 — Managing child protection, legal and ethical aspects of children at risk | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-013"] Edit |
| [link] PAED-014 — Providing palliative and end-of-life care | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-014"] Edit |
| [link] PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-015"] Edit |

######## BOARD R2-E02-390.dc.html
R2-E02-390 · 390
6.17 · Anton Kruger · PAED-012 Inactive (after 6.14: PAED-006 "… in children").
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] [aria-current="page"] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
### EPAs
College of Paediatricians of South Africa's EPAs. A change to one is read at once wherever it is named, in every institution that has adopted a curriculum that holds it.
[link] Create EPA
College of Paediatricians of South Africa's EPAs
| EPA | Sub-speciality | Status | Actions |
| [link] PAED-001 — Providing paediatric emergency care to children | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-001"] Edit |
| [link] PAED-002 — Managing common paediatric presentations | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-002"] Edit |
| [link] PAED-003 — Providing intensive care to children | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-003"] Edit |
| [link] PAED-004 — Managing common neonatal conditions | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-004"] Edit |
| [link] PAED-005 — Providing neonatal care in intensive and high-care settings | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-005"] Edit |
| [link] PAED-006 — Managing long-term health conditions (LTHCs) in children | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-006"] Edit |
| [link] PAED-007 — Maintaining and promoting the health and well-being of children | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-007"] Edit |
| [link] PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-008"] Edit |
| [link] PAED-009 — Providing care for an adolescent patient | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-009"] Edit |
| [link] PAED-010 — Leading and operating within a clinical team | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-010"] Edit |
| [link] PAED-011 — Managing population health challenges | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-011"] Edit |
| [link] PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | Paediatrics / Paediatrics | Inactive | [link] [aria-label="Edit PAED-012"] Edit |
| [link] PAED-013 — Managing child protection, legal and ethical aspects of children at risk | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-013"] Edit |
| [link] PAED-014 — Providing palliative and end-of-life care | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-014"] Edit |
| [link] PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-015"] Edit |

######## BOARD R2-E03.dc.html
R2-E03 · 1280
6.28 · Anton Kruger · sixteen; no KGK-001 (a College does not see an institution's own EPAs).
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] [aria-current="page"] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
### EPAs
College of Paediatricians of South Africa's EPAs. A change to one is read at once wherever it is named, in every institution that has adopted a curriculum that holds it.
[link] Create EPA
College of Paediatricians of South Africa's EPAs
| EPA | Sub-speciality | Status | Actions |
| [link] PAED-001 — Providing paediatric emergency care to children | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-001"] Edit |
| [link] PAED-002 — Managing common paediatric presentations | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-002"] Edit |
| [link] PAED-003 — Providing intensive care to children | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-003"] Edit |
| [link] PAED-004 — Managing common neonatal conditions | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-004"] Edit |
| [link] PAED-005 — Providing neonatal care in intensive and high-care settings | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-005"] Edit |
| [link] PAED-006 — Managing long-term health conditions (LTHCs) in children | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-006"] Edit |
| [link] PAED-007 — Maintaining and promoting the health and well-being of children | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-007"] Edit |
| [link] PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-008"] Edit |
| [link] PAED-009 — Providing care for an adolescent patient | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-009"] Edit |
| [link] PAED-010 — Leading and operating within a clinical team | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-010"] Edit |
| [link] PAED-011 — Managing population health challenges | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-011"] Edit |
| [link] PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-012"] Edit |
| [link] PAED-013 — Managing child protection, legal and ethical aspects of children at risk | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-013"] Edit |
| [link] PAED-014 — Providing palliative and end-of-life care | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-014"] Edit |
| [link] PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-015"] Edit |
| [link] PAED-016 — Transitioning adolescents with long-term conditions to adult care | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-016"] Edit |

######## BOARD R2-E03-390.dc.html
R2-E03-390 · 390
6.28 · Anton Kruger · sixteen; no KGK-001 (a College does not see an institution's own EPAs).
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] [aria-current="page"] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
### EPAs
College of Paediatricians of South Africa's EPAs. A change to one is read at once wherever it is named, in every institution that has adopted a curriculum that holds it.
[link] Create EPA
College of Paediatricians of South Africa's EPAs
| EPA | Sub-speciality | Status | Actions |
| [link] PAED-001 — Providing paediatric emergency care to children | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-001"] Edit |
| [link] PAED-002 — Managing common paediatric presentations | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-002"] Edit |
| [link] PAED-003 — Providing intensive care to children | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-003"] Edit |
| [link] PAED-004 — Managing common neonatal conditions | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-004"] Edit |
| [link] PAED-005 — Providing neonatal care in intensive and high-care settings | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-005"] Edit |
| [link] PAED-006 — Managing long-term health conditions (LTHCs) in children | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-006"] Edit |
| [link] PAED-007 — Maintaining and promoting the health and well-being of children | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-007"] Edit |
| [link] PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-008"] Edit |
| [link] PAED-009 — Providing care for an adolescent patient | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-009"] Edit |
| [link] PAED-010 — Leading and operating within a clinical team | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-010"] Edit |
| [link] PAED-011 — Managing population health challenges | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-011"] Edit |
| [link] PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-012"] Edit |
| [link] PAED-013 — Managing child protection, legal and ethical aspects of children at risk | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-013"] Edit |
| [link] PAED-014 — Providing palliative and end-of-life care | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-014"] Edit |
| [link] PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-015"] Edit |
| [link] PAED-016 — Transitioning adolescents with long-term conditions to adult care | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-016"] Edit |

######## BOARD R2-E04.dc.html
R2-E04 · 1280
After 6.28 · Demo Administrator · eighteen rows: the College column kept; KGK-001 marked as Kgosi Kgari Teaching Hospital's own; Edit on every row.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] [aria-current="page"] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
### EPAs
Maintain the entrustable professional activities available to curricula and forms.
[link] Create EPA
Every EPA
| EPA | College | Sub-speciality | Status | Actions |
| [link] EPA-001 — Clerk, assess, and present a general medical admission | Demo College | General Medicine / General Internal Medicine | Active | [link] [aria-label="Edit EPA-001"] Edit |
| [link] PAED-001 — Providing paediatric emergency care to children | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-001"] Edit |
| [link] PAED-002 — Managing common paediatric presentations | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-002"] Edit |
| [link] PAED-003 — Providing intensive care to children | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-003"] Edit |
| [link] PAED-004 — Managing common neonatal conditions | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-004"] Edit |
| [link] PAED-005 — Providing neonatal care in intensive and high-care settings | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-005"] Edit |
| [link] PAED-006 — Managing long-term health conditions (LTHCs) in children | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-006"] Edit |
| [link] PAED-007 — Maintaining and promoting the health and well-being of children | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-007"] Edit |
| [link] PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-008"] Edit |
| [link] PAED-009 — Providing care for an adolescent patient | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-009"] Edit |
| [link] PAED-010 — Leading and operating within a clinical team | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-010"] Edit |
| [link] PAED-011 — Managing population health challenges | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-011"] Edit |
| [link] PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-012"] Edit |
| [link] PAED-013 — Managing child protection, legal and ethical aspects of children at risk | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-013"] Edit |
| [link] PAED-014 — Providing palliative and end-of-life care | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-014"] Edit |
| [link] PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-015"] Edit |
| [link] PAED-016 — Transitioning adolescents with long-term conditions to adult care | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-016"] Edit |
| [link] KGK-001 — Running a paediatric outreach clinic at a district hospitalKgosi Kgari Teaching Hospital's own EPA | Kgosi Kgari Teaching Hospital | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit KGK-001"] Edit |

######## BOARD R2-E04-390.dc.html
R2-E04-390 · 390
After 6.28 · Demo Administrator · eighteen rows: the College column kept; KGK-001 marked as Kgosi Kgari Teaching Hospital's own; Edit on every row.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] [aria-current="page"] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
### EPAs
Maintain the entrustable professional activities available to curricula and forms.
[link] Create EPA
Every EPA
| EPA | College | Sub-speciality | Status | Actions |
| [link] EPA-001 — Clerk, assess, and present a general medical admission | Demo College | General Medicine / General Internal Medicine | Active | [link] [aria-label="Edit EPA-001"] Edit |
| [link] PAED-001 — Providing paediatric emergency care to children | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-001"] Edit |
| [link] PAED-002 — Managing common paediatric presentations | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-002"] Edit |
| [link] PAED-003 — Providing intensive care to children | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-003"] Edit |
| [link] PAED-004 — Managing common neonatal conditions | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-004"] Edit |
| [link] PAED-005 — Providing neonatal care in intensive and high-care settings | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-005"] Edit |
| [link] PAED-006 — Managing long-term health conditions (LTHCs) in children | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-006"] Edit |
| [link] PAED-007 — Maintaining and promoting the health and well-being of children | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-007"] Edit |
| [link] PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-008"] Edit |
| [link] PAED-009 — Providing care for an adolescent patient | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-009"] Edit |
| [link] PAED-010 — Leading and operating within a clinical team | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-010"] Edit |
| [link] PAED-011 — Managing population health challenges | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-011"] Edit |
| [link] PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-012"] Edit |
| [link] PAED-013 — Managing child protection, legal and ethical aspects of children at risk | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-013"] Edit |
| [link] PAED-014 — Providing palliative and end-of-life care | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-014"] Edit |
| [link] PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-015"] Edit |
| [link] PAED-016 — Transitioning adolescents with long-term conditions to adult care | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-016"] Edit |
| [link] KGK-001 — Running a paediatric outreach clinic at a district hospitalKgosi Kgari Teaching Hospital's own EPA | Kgosi Kgari Teaching Hospital | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit KGK-001"] Edit |

######## BOARD R2-E04-768.dc.html
R2-E04-768 · 768
After 6.28 · Demo Administrator · eighteen rows: the College column kept; KGK-001 marked as Kgosi Kgari Teaching Hospital's own; Edit on every row.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] [aria-current="page"] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
### EPAs
Maintain the entrustable professional activities available to curricula and forms.
[link] Create EPA
Every EPA
| EPA | College | Sub-speciality | Status | Actions |
| [link] EPA-001 — Clerk, assess, and present a general medical admission | Demo College | General Medicine / General Internal Medicine | Active | [link] [aria-label="Edit EPA-001"] Edit |
| [link] PAED-001 — Providing paediatric emergency care to children | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-001"] Edit |
| [link] PAED-002 — Managing common paediatric presentations | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-002"] Edit |
| [link] PAED-003 — Providing intensive care to children | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-003"] Edit |
| [link] PAED-004 — Managing common neonatal conditions | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-004"] Edit |
| [link] PAED-005 — Providing neonatal care in intensive and high-care settings | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-005"] Edit |
| [link] PAED-006 — Managing long-term health conditions (LTHCs) in children | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-006"] Edit |
| [link] PAED-007 — Maintaining and promoting the health and well-being of children | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-007"] Edit |
| [link] PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-008"] Edit |
| [link] PAED-009 — Providing care for an adolescent patient | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-009"] Edit |
| [link] PAED-010 — Leading and operating within a clinical team | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-010"] Edit |
| [link] PAED-011 — Managing population health challenges | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-011"] Edit |
| [link] PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-012"] Edit |
| [link] PAED-013 — Managing child protection, legal and ethical aspects of children at risk | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-013"] Edit |
| [link] PAED-014 — Providing palliative and end-of-life care | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-014"] Edit |
| [link] PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-015"] Edit |
| [link] PAED-016 — Transitioning adolescents with long-term conditions to adult care | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-016"] Edit |
| [link] KGK-001 — Running a paediatric outreach clinic at a district hospitalKgosi Kgari Teaching Hospital's own EPA | Kgosi Kgari Teaching Hospital | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit KGK-001"] Edit |

######## BOARD R2-E05-EV1-390.dc.html
R2-E05-EV1-390 · 390
6.14 before Save · Anton Kruger · EV1: PAED-006's page, the new title typed; the h1 the stored title until the save. The NEW subtitle names the College and the discipline.
EV1: no reach outside the pause dialog.
Focus: Title, as typed.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] [aria-current="page"] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] EPAs
-
- [aria-current="page"] PAED-006
### PAED-006 — Managing long-term health conditions (LTHCs)
College of Paediatricians of South Africa · Paediatrics / Paediatrics
Sub-speciality*
Paediatrics / Neonatology
[selected] Paediatrics / Paediatrics
College of Paediatricians of South Africa's sub-specialities.
Code*
[value="PAED-006"]
Title*
[value="Managing long-term health conditions (LTHCs) in children"]
Description
‹description›
Required knowledge and skills
‹required knowledge and skills›
Category*
[selected] Core
Elective
Status
(checkbox) [checked]
Active
An inactive EPA cannot be chosen for a new activity and is not a target on any running programme's progress page or dashboard; a programme that has ended keeps it on its record. Evidence already recorded against it is kept, and so is the progress it earned while it was active. An activity completed against it while it is inactive counts towards nothing until the EPA is reactivated, and reactivating it credits those activities.
[link] Cancel
[button] [aria-haspopup="dialog"] Deactivate…
[button] Save

######## BOARD R2-E05-EV1.dc.html
R2-E05-EV1 · 1280
6.14 before Save · Anton Kruger · EV1: PAED-006's page, the new title typed; the h1 the stored title until the save. The NEW subtitle names the College and the discipline.
EV1: no reach outside the pause dialog.
Focus: Title, as typed.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] [aria-current="page"] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] EPAs
-
- [aria-current="page"] PAED-006
### PAED-006 — Managing long-term health conditions (LTHCs)
College of Paediatricians of South Africa · Paediatrics / Paediatrics
Sub-speciality*
Paediatrics / Neonatology
[selected] Paediatrics / Paediatrics
College of Paediatricians of South Africa's sub-specialities.
Code*
[value="PAED-006"]
Title*
[value="Managing long-term health conditions (LTHCs) in children"]
Description
‹description›
Required knowledge and skills
‹required knowledge and skills›
Category*
[selected] Core
Elective
Status
(checkbox) [checked]
Active
An inactive EPA cannot be chosen for a new activity and is not a target on any running programme's progress page or dashboard; a programme that has ended keeps it on its record. Evidence already recorded against it is kept, and so is the progress it earned while it was active. An activity completed against it while it is inactive counts towards nothing until the EPA is reactivated, and reactivating it credits those activities.
[link] Cancel
[button] [aria-haspopup="dialog"] Deactivate…
[button] Save

######## BOARD R2-E05-EV2-390.dc.html
R2-E05-EV2-390 · 390
6.14 before Save · Anton Kruger · EV2: PAED-006's page, the new title typed; the h1 the stored title until the save. The NEW subtitle names the College and the discipline.
EV2: the standing reach line, an info Alert with no role, under the result region (NEW words).
Focus: Title, as typed.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] [aria-current="page"] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] EPAs
-
- [aria-current="page"] PAED-006
### PAED-006 — Managing long-term health conditions (LTHCs)
College of Paediatricians of South Africa · Paediatrics / Paediatrics
PAED-006 is on 1 curriculum adopted by 1 institution, Kgosi Kgari Teaching Hospital; 3 registrars in training are measured against it. Its code, title and description are read at once wherever it is named.
Sub-speciality*
Paediatrics / Neonatology
[selected] Paediatrics / Paediatrics
College of Paediatricians of South Africa's sub-specialities.
Code*
[value="PAED-006"]
Title*
[value="Managing long-term health conditions (LTHCs) in children"]
Description
‹description›
Required knowledge and skills
‹required knowledge and skills›
Category*
[selected] Core
Elective
Status
(checkbox) [checked]
Active
An inactive EPA cannot be chosen for a new activity and is not a target on any running programme's progress page or dashboard; a programme that has ended keeps it on its record. Evidence already recorded against it is kept, and so is the progress it earned while it was active. An activity completed against it while it is inactive counts towards nothing until the EPA is reactivated, and reactivating it credits those activities.
[link] Cancel
[button] [aria-haspopup="dialog"] Deactivate…
[button] Save

######## BOARD R2-E05-EV2.dc.html
R2-E05-EV2 · 1280
6.14 before Save · Anton Kruger · EV2: PAED-006's page, the new title typed; the h1 the stored title until the save. The NEW subtitle names the College and the discipline.
EV2: the standing reach line, an info Alert with no role, under the result region (NEW words).
Focus: Title, as typed.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] [aria-current="page"] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] EPAs
-
- [aria-current="page"] PAED-006
### PAED-006 — Managing long-term health conditions (LTHCs)
College of Paediatricians of South Africa · Paediatrics / Paediatrics
PAED-006 is on 1 curriculum adopted by 1 institution, Kgosi Kgari Teaching Hospital; 3 registrars in training are measured against it. Its code, title and description are read at once wherever it is named.
Sub-speciality*
Paediatrics / Neonatology
[selected] Paediatrics / Paediatrics
College of Paediatricians of South Africa's sub-specialities.
Code*
[value="PAED-006"]
Title*
[value="Managing long-term health conditions (LTHCs) in children"]
Description
‹description›
Required knowledge and skills
‹required knowledge and skills›
Category*
[selected] Core
Elective
Status
(checkbox) [checked]
Active
An inactive EPA cannot be chosen for a new activity and is not a target on any running programme's progress page or dashboard; a programme that has ended keeps it on its record. Evidence already recorded against it is kept, and so is the progress it earned while it was active. An activity completed against it while it is inactive counts towards nothing until the EPA is reactivated, and reactivating it credits those activities.
[link] Cancel
[button] [aria-haspopup="dialog"] Deactivate…
[button] Save

######## BOARD R2-E06-EV1-390.dc.html
R2-E06-EV1-390 · 390
6.14 saved · Anton Kruger · EV1: "EPA saved." focused; the h1 the new title.
Focus: the result.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] [aria-current="page"] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] EPAs
-
- [aria-current="page"] PAED-006
### PAED-006 — Managing long-term health conditions (LTHCs) in children
College of Paediatricians of South Africa · Paediatrics / Paediatrics
EPA saved.
Sub-speciality*
Paediatrics / Neonatology
[selected] Paediatrics / Paediatrics
College of Paediatricians of South Africa's sub-specialities.
Code*
[value="PAED-006"]
Title*
[value="Managing long-term health conditions (LTHCs) in children"]
Description
‹description›
Required knowledge and skills
‹required knowledge and skills›
Category*
[selected] Core
Elective
Status
(checkbox) [checked]
Active
An inactive EPA cannot be chosen for a new activity and is not a target on any running programme's progress page or dashboard; a programme that has ended keeps it on its record. Evidence already recorded against it is kept, and so is the progress it earned while it was active. An activity completed against it while it is inactive counts towards nothing until the EPA is reactivated, and reactivating it credits those activities.
[link] Cancel
[button] [aria-haspopup="dialog"] Deactivate…
[button] Save

######## BOARD R2-E06-EV1.dc.html
R2-E06-EV1 · 1280
6.14 saved · Anton Kruger · EV1: "EPA saved." focused; the h1 the new title.
Focus: the result.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] [aria-current="page"] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] EPAs
-
- [aria-current="page"] PAED-006
### PAED-006 — Managing long-term health conditions (LTHCs) in children
College of Paediatricians of South Africa · Paediatrics / Paediatrics
EPA saved.
Sub-speciality*
Paediatrics / Neonatology
[selected] Paediatrics / Paediatrics
College of Paediatricians of South Africa's sub-specialities.
Code*
[value="PAED-006"]
Title*
[value="Managing long-term health conditions (LTHCs) in children"]
Description
‹description›
Required knowledge and skills
‹required knowledge and skills›
Category*
[selected] Core
Elective
Status
(checkbox) [checked]
Active
An inactive EPA cannot be chosen for a new activity and is not a target on any running programme's progress page or dashboard; a programme that has ended keeps it on its record. Evidence already recorded against it is kept, and so is the progress it earned while it was active. An activity completed against it while it is inactive counts towards nothing until the EPA is reactivated, and reactivating it credits those activities.
[link] Cancel
[button] [aria-haspopup="dialog"] Deactivate…
[button] Save

######## BOARD R2-E06-EV2-390.dc.html
R2-E06-EV2-390 · 390
6.14 saved · Anton Kruger · EV2: "EPA saved." focused; the h1 the new title.
Focus: the result.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] [aria-current="page"] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] EPAs
-
- [aria-current="page"] PAED-006
### PAED-006 — Managing long-term health conditions (LTHCs) in children
College of Paediatricians of South Africa · Paediatrics / Paediatrics
EPA saved.
PAED-006 is on 1 curriculum adopted by 1 institution, Kgosi Kgari Teaching Hospital; 3 registrars in training are measured against it. Its code, title and description are read at once wherever it is named.
Sub-speciality*
Paediatrics / Neonatology
[selected] Paediatrics / Paediatrics
College of Paediatricians of South Africa's sub-specialities.
Code*
[value="PAED-006"]
Title*
[value="Managing long-term health conditions (LTHCs) in children"]
Description
‹description›
Required knowledge and skills
‹required knowledge and skills›
Category*
[selected] Core
Elective
Status
(checkbox) [checked]
Active
An inactive EPA cannot be chosen for a new activity and is not a target on any running programme's progress page or dashboard; a programme that has ended keeps it on its record. Evidence already recorded against it is kept, and so is the progress it earned while it was active. An activity completed against it while it is inactive counts towards nothing until the EPA is reactivated, and reactivating it credits those activities.
[link] Cancel
[button] [aria-haspopup="dialog"] Deactivate…
[button] Save

######## BOARD R2-E06-EV2.dc.html
R2-E06-EV2 · 1280
6.14 saved · Anton Kruger · EV2: "EPA saved." focused; the h1 the new title.
Focus: the result.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] [aria-current="page"] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] EPAs
-
- [aria-current="page"] PAED-006
### PAED-006 — Managing long-term health conditions (LTHCs) in children
College of Paediatricians of South Africa · Paediatrics / Paediatrics
EPA saved.
PAED-006 is on 1 curriculum adopted by 1 institution, Kgosi Kgari Teaching Hospital; 3 registrars in training are measured against it. Its code, title and description are read at once wherever it is named.
Sub-speciality*
Paediatrics / Neonatology
[selected] Paediatrics / Paediatrics
College of Paediatricians of South Africa's sub-specialities.
Code*
[value="PAED-006"]
Title*
[value="Managing long-term health conditions (LTHCs) in children"]
Description
‹description›
Required knowledge and skills
‹required knowledge and skills›
Category*
[selected] Core
Elective
Status
(checkbox) [checked]
Active
An inactive EPA cannot be chosen for a new activity and is not a target on any running programme's progress page or dashboard; a programme that has ended keeps it on its record. Evidence already recorded against it is kept, and so is the progress it earned while it was active. An activity completed against it while it is inactive counts towards nothing until the EPA is reactivated, and reactivating it credits those activities.
[link] Cancel
[button] [aria-haspopup="dialog"] Deactivate…
[button] Save

######## BOARD R2-E07.dc.html
R2-E07 · 1280
6.17 · Anton Kruger · the pause dialog with Q2's reach as its first paragraph (counts and the institution, no registrar's name), then the built body; opening on Cancel.
Focus: Cancel.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] [aria-current="page"] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] EPAs
-
- [aria-current="page"] PAED-012
### PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
College of Paediatricians of South Africa · Paediatrics / Paediatrics
Sub-speciality*
Paediatrics / Neonatology
[selected] Paediatrics / Paediatrics
College of Paediatricians of South Africa's sub-specialities.
Code*
[value="PAED-012"]
Title*
[value="Communicating with and counselling patients, caregivers and healthcare teams"]
Description
‹description›
Required knowledge and skills
‹required knowledge and skills›
Category*
[selected] Core
Elective
Status
(checkbox) [checked]
Active
An inactive EPA cannot be chosen for a new activity and is not a target on any running programme's progress page or dashboard; a programme that has ended keeps it on its record. Evidence already recorded against it is kept, and so is the progress it earned while it was active. An activity completed against it while it is inactive counts towards nothing until the EPA is reactivated, and reactivating it credits those activities.
[link] Cancel
[button] [aria-haspopup="dialog"] Deactivate…
[button] Save
#### Deactivate PAED-012?
PAED-012 is on 1 curriculum adopted by 1 institution, Kgosi Kgari Teaching Hospital; 3 registrars in training are measured against it.
It can no longer be chosen for a new activity, and it stops being a target on running programmes' progress pages and dashboards, for every institution that has adopted it. A programme that has ended keeps it on its record, marked no longer in use. The progress it has earned is kept. An activity completed against it while it is inactive counts towards nothing until it is reactivated. You can reactivate it on this page.
[button] Cancel
[button] Deactivate

######## BOARD R2-E07-390.dc.html
R2-E07-390 · 390
6.17 · Anton Kruger · the pause dialog with Q2's reach as its first paragraph (counts and the institution, no registrar's name), then the built body; opening on Cancel.
Focus: Cancel.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] [aria-current="page"] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] EPAs
-
- [aria-current="page"] PAED-012
### PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
College of Paediatricians of South Africa · Paediatrics / Paediatrics
Sub-speciality*
Paediatrics / Neonatology
[selected] Paediatrics / Paediatrics
College of Paediatricians of South Africa's sub-specialities.
Code*
[value="PAED-012"]
Title*
[value="Communicating with and counselling patients, caregivers and healthcare teams"]
Description
‹description›
Required knowledge and skills
‹required knowledge and skills›
Category*
[selected] Core
Elective
Status
(checkbox) [checked]
Active
An inactive EPA cannot be chosen for a new activity and is not a target on any running programme's progress page or dashboard; a programme that has ended keeps it on its record. Evidence already recorded against it is kept, and so is the progress it earned while it was active. An activity completed against it while it is inactive counts towards nothing until the EPA is reactivated, and reactivating it credits those activities.
[link] Cancel
[button] [aria-haspopup="dialog"] Deactivate…
[button] Save
#### Deactivate PAED-012?
PAED-012 is on 1 curriculum adopted by 1 institution, Kgosi Kgari Teaching Hospital; 3 registrars in training are measured against it.
It can no longer be chosen for a new activity, and it stops being a target on running programmes' progress pages and dashboards, for every institution that has adopted it. A programme that has ended keeps it on its record, marked no longer in use. The progress it has earned is kept. An activity completed against it while it is inactive counts towards nothing until it is reactivated. You can reactivate it on this page.
[button] Cancel
[button] Deactivate

######## BOARD R2-E08.dc.html
R2-E08 · 1280
6.17 deactivated · "EPA deactivated." focused, Active unticked, Deactivate… gone.
Focus: the result.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] [aria-current="page"] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] EPAs
-
- [aria-current="page"] PAED-012
### PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
College of Paediatricians of South Africa · Paediatrics / Paediatrics
EPA deactivated.
Sub-speciality*
Paediatrics / Neonatology
[selected] Paediatrics / Paediatrics
College of Paediatricians of South Africa's sub-specialities.
Code*
[value="PAED-012"]
Title*
[value="Communicating with and counselling patients, caregivers and healthcare teams"]
Description
‹description›
Required knowledge and skills
‹required knowledge and skills›
Category*
[selected] Core
Elective
Status
(checkbox)
Active
An inactive EPA cannot be chosen for a new activity and is not a target on any running programme's progress page or dashboard; a programme that has ended keeps it on its record. Evidence already recorded against it is kept, and so is the progress it earned while it was active. An activity completed against it while it is inactive counts towards nothing until the EPA is reactivated, and reactivating it credits those activities.
[link] Cancel
[button] Save

######## BOARD R2-E08-390.dc.html
R2-E08-390 · 390
6.17 deactivated · "EPA deactivated." focused, Active unticked, Deactivate… gone.
Focus: the result.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] [aria-current="page"] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] EPAs
-
- [aria-current="page"] PAED-012
### PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
College of Paediatricians of South Africa · Paediatrics / Paediatrics
EPA deactivated.
Sub-speciality*
Paediatrics / Neonatology
[selected] Paediatrics / Paediatrics
College of Paediatricians of South Africa's sub-specialities.
Code*
[value="PAED-012"]
Title*
[value="Communicating with and counselling patients, caregivers and healthcare teams"]
Description
‹description›
Required knowledge and skills
‹required knowledge and skills›
Category*
[selected] Core
Elective
Status
(checkbox)
Active
An inactive EPA cannot be chosen for a new activity and is not a target on any running programme's progress page or dashboard; a programme that has ended keeps it on its record. Evidence already recorded against it is kept, and so is the progress it earned while it was active. An activity completed against it while it is inactive counts towards nothing until the EPA is reactivated, and reactivating it credits those activities.
[link] Cancel
[button] Save

######## BOARD R2-E09-EV1-390.dc.html
R2-E09-EV1-390 · 390
6.23 reactivated · Anton Kruger · EV1: Active ticked and Save, nothing asked; the built result focused; Deactivate… back.
Focus: the result.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] [aria-current="page"] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] EPAs
-
- [aria-current="page"] PAED-012
### PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
College of Paediatricians of South Africa · Paediatrics / Paediatrics
EPA reactivated. 1 activity completed against it while it was inactive now counts towards progress.
Sub-speciality*
Paediatrics / Neonatology
[selected] Paediatrics / Paediatrics
College of Paediatricians of South Africa's sub-specialities.
Code*
[value="PAED-012"]
Title*
[value="Communicating with and counselling patients, caregivers and healthcare teams"]
Description
‹description›
Required knowledge and skills
‹required knowledge and skills›
Category*
[selected] Core
Elective
Status
(checkbox) [checked]
Active
An inactive EPA cannot be chosen for a new activity and is not a target on any running programme's progress page or dashboard; a programme that has ended keeps it on its record. Evidence already recorded against it is kept, and so is the progress it earned while it was active. An activity completed against it while it is inactive counts towards nothing until the EPA is reactivated, and reactivating it credits those activities.
[link] Cancel
[button] [aria-haspopup="dialog"] Deactivate…
[button] Save

######## BOARD R2-E09-EV1.dc.html
R2-E09-EV1 · 1280
6.23 reactivated · Anton Kruger · EV1: Active ticked and Save, nothing asked; the built result focused; Deactivate… back.
Focus: the result.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] [aria-current="page"] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] EPAs
-
- [aria-current="page"] PAED-012
### PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
College of Paediatricians of South Africa · Paediatrics / Paediatrics
EPA reactivated. 1 activity completed against it while it was inactive now counts towards progress.
Sub-speciality*
Paediatrics / Neonatology
[selected] Paediatrics / Paediatrics
College of Paediatricians of South Africa's sub-specialities.
Code*
[value="PAED-012"]
Title*
[value="Communicating with and counselling patients, caregivers and healthcare teams"]
Description
‹description›
Required knowledge and skills
‹required knowledge and skills›
Category*
[selected] Core
Elective
Status
(checkbox) [checked]
Active
An inactive EPA cannot be chosen for a new activity and is not a target on any running programme's progress page or dashboard; a programme that has ended keeps it on its record. Evidence already recorded against it is kept, and so is the progress it earned while it was active. An activity completed against it while it is inactive counts towards nothing until the EPA is reactivated, and reactivating it credits those activities.
[link] Cancel
[button] [aria-haspopup="dialog"] Deactivate…
[button] Save

######## BOARD R2-E09-EV2-390.dc.html
R2-E09-EV2-390 · 390
6.23 reactivated · Anton Kruger · EV2: Active ticked and Save, nothing asked; the built result focused; Deactivate… back.
Focus: the result.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] [aria-current="page"] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] EPAs
-
- [aria-current="page"] PAED-012
### PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
College of Paediatricians of South Africa · Paediatrics / Paediatrics
EPA reactivated. 1 activity completed against it while it was inactive now counts towards progress.
PAED-012 is on 1 curriculum adopted by 1 institution, Kgosi Kgari Teaching Hospital; 3 registrars in training are measured against it. Its code, title and description are read at once wherever it is named.
Sub-speciality*
Paediatrics / Neonatology
[selected] Paediatrics / Paediatrics
College of Paediatricians of South Africa's sub-specialities.
Code*
[value="PAED-012"]
Title*
[value="Communicating with and counselling patients, caregivers and healthcare teams"]
Description
‹description›
Required knowledge and skills
‹required knowledge and skills›
Category*
[selected] Core
Elective
Status
(checkbox) [checked]
Active
An inactive EPA cannot be chosen for a new activity and is not a target on any running programme's progress page or dashboard; a programme that has ended keeps it on its record. Evidence already recorded against it is kept, and so is the progress it earned while it was active. An activity completed against it while it is inactive counts towards nothing until the EPA is reactivated, and reactivating it credits those activities.
[link] Cancel
[button] [aria-haspopup="dialog"] Deactivate…
[button] Save

######## BOARD R2-E09-EV2.dc.html
R2-E09-EV2 · 1280
6.23 reactivated · Anton Kruger · EV2: Active ticked and Save, nothing asked; the built result focused; Deactivate… back.
Focus: the result.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] [aria-current="page"] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] EPAs
-
- [aria-current="page"] PAED-012
### PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
College of Paediatricians of South Africa · Paediatrics / Paediatrics
EPA reactivated. 1 activity completed against it while it was inactive now counts towards progress.
PAED-012 is on 1 curriculum adopted by 1 institution, Kgosi Kgari Teaching Hospital; 3 registrars in training are measured against it. Its code, title and description are read at once wherever it is named.
Sub-speciality*
Paediatrics / Neonatology
[selected] Paediatrics / Paediatrics
College of Paediatricians of South Africa's sub-specialities.
Code*
[value="PAED-012"]
Title*
[value="Communicating with and counselling patients, caregivers and healthcare teams"]
Description
‹description›
Required knowledge and skills
‹required knowledge and skills›
Category*
[selected] Core
Elective
Status
(checkbox) [checked]
Active
An inactive EPA cannot be chosen for a new activity and is not a target on any running programme's progress page or dashboard; a programme that has ended keeps it on its record. Evidence already recorded against it is kept, and so is the progress it earned while it was active. An activity completed against it while it is inactive counts towards nothing until the EPA is reactivated, and reactivating it credits those activities.
[link] Cancel
[button] [aria-haspopup="dialog"] Deactivate…
[button] Save

######## BOARD R2-E10.dc.html
R2-E10 · 1280
6.28 · Anton Kruger · Create EPA: the NEW subtitle; Sub-speciality offers CPSA's two (Paediatrics / Neonatology first), changed to Paediatrics / Paediatrics; the NEW help; the values typed. No reach line (no EPA yet).
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] [aria-current="page"] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] EPAs
-
- [aria-current="page"] Create EPA
### Create EPA
A national EPA of College of Paediatricians of South Africa. It reaches a registrar once a curriculum an institution has adopted holds it.
Sub-speciality*
Paediatrics / Neonatology
[selected] Paediatrics / Paediatrics
College of Paediatricians of South Africa's sub-specialities.
Code*
[value="PAED-016"]
Title*
[value="Transitioning adolescents with long-term conditions to adult care"]
Description
Plans and leads the handover of an adolescent with a long-term condition to adult services, with the young person and their caregivers.
Required knowledge and skills
Category*
[selected] Core
Elective
[link] Cancel
[button] Save

######## BOARD R2-E10-390.dc.html
R2-E10-390 · 390
6.28 · Anton Kruger · Create EPA: the NEW subtitle; Sub-speciality offers CPSA's two (Paediatrics / Neonatology first), changed to Paediatrics / Paediatrics; the NEW help; the values typed. No reach line (no EPA yet).
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] [aria-current="page"] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] EPAs
-
- [aria-current="page"] Create EPA
### Create EPA
A national EPA of College of Paediatricians of South Africa. It reaches a registrar once a curriculum an institution has adopted holds it.
Sub-speciality*
Paediatrics / Neonatology
[selected] Paediatrics / Paediatrics
College of Paediatricians of South Africa's sub-specialities.
Code*
[value="PAED-016"]
Title*
[value="Transitioning adolescents with long-term conditions to adult care"]
Description
Plans and leads the handover of an adolescent with a long-term condition to adult services, with the young person and their caregivers.
Required knowledge and skills
Category*
[selected] Core
Elective
[link] Cancel
[button] Save

######## BOARD R2-E11-EV1-390.dc.html
R2-E11-EV1-390 · 390
6.28 saved · Anton Kruger · EV1: /admin/epas/18 by ?created, "EPA saved." focused, the address replaced.
Focus: the result.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] [aria-current="page"] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] EPAs
-
- [aria-current="page"] PAED-016
### PAED-016 — Transitioning adolescents with long-term conditions to adult care
College of Paediatricians of South Africa · Paediatrics / Paediatrics
EPA saved.
Sub-speciality*
Paediatrics / Neonatology
[selected] Paediatrics / Paediatrics
College of Paediatricians of South Africa's sub-specialities.
Code*
[value="PAED-016"]
Title*
[value="Transitioning adolescents with long-term conditions to adult care"]
Description
Plans and leads the handover of an adolescent with a long-term condition to adult services, with the young person and their caregivers.
Required knowledge and skills
Category*
[selected] Core
Elective
Status
(checkbox) [checked]
Active
An inactive EPA cannot be chosen for a new activity and is not a target on any running programme's progress page or dashboard; a programme that has ended keeps it on its record. Evidence already recorded against it is kept, and so is the progress it earned while it was active. An activity completed against it while it is inactive counts towards nothing until the EPA is reactivated, and reactivating it credits those activities.
[link] Cancel
[button] [aria-haspopup="dialog"] Deactivate…
[button] Save

######## BOARD R2-E11-EV1.dc.html
R2-E11-EV1 · 1280
6.28 saved · Anton Kruger · EV1: /admin/epas/18 by ?created, "EPA saved." focused, the address replaced.
Focus: the result.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] [aria-current="page"] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] EPAs
-
- [aria-current="page"] PAED-016
### PAED-016 — Transitioning adolescents with long-term conditions to adult care
College of Paediatricians of South Africa · Paediatrics / Paediatrics
EPA saved.
Sub-speciality*
Paediatrics / Neonatology
[selected] Paediatrics / Paediatrics
College of Paediatricians of South Africa's sub-specialities.
Code*
[value="PAED-016"]
Title*
[value="Transitioning adolescents with long-term conditions to adult care"]
Description
Plans and leads the handover of an adolescent with a long-term condition to adult services, with the young person and their caregivers.
Required knowledge and skills
Category*
[selected] Core
Elective
Status
(checkbox) [checked]
Active
An inactive EPA cannot be chosen for a new activity and is not a target on any running programme's progress page or dashboard; a programme that has ended keeps it on its record. Evidence already recorded against it is kept, and so is the progress it earned while it was active. An activity completed against it while it is inactive counts towards nothing until the EPA is reactivated, and reactivating it credits those activities.
[link] Cancel
[button] [aria-haspopup="dialog"] Deactivate…
[button] Save

######## BOARD R2-E11-EV2-390.dc.html
R2-E11-EV2-390 · 390
6.28 saved · Anton Kruger · EV2: /admin/epas/18 by ?created, "EPA saved." focused, the address replaced. PAED-016 reads on no adopted curriculum.
Focus: the result.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] [aria-current="page"] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] EPAs
-
- [aria-current="page"] PAED-016
### PAED-016 — Transitioning adolescents with long-term conditions to adult care
College of Paediatricians of South Africa · Paediatrics / Paediatrics
EPA saved.
PAED-016 is on no curriculum an institution has adopted, so no registrar is measured against it yet.
Sub-speciality*
Paediatrics / Neonatology
[selected] Paediatrics / Paediatrics
College of Paediatricians of South Africa's sub-specialities.
Code*
[value="PAED-016"]
Title*
[value="Transitioning adolescents with long-term conditions to adult care"]
Description
Plans and leads the handover of an adolescent with a long-term condition to adult services, with the young person and their caregivers.
Required knowledge and skills
Category*
[selected] Core
Elective
Status
(checkbox) [checked]
Active
An inactive EPA cannot be chosen for a new activity and is not a target on any running programme's progress page or dashboard; a programme that has ended keeps it on its record. Evidence already recorded against it is kept, and so is the progress it earned while it was active. An activity completed against it while it is inactive counts towards nothing until the EPA is reactivated, and reactivating it credits those activities.
[link] Cancel
[button] [aria-haspopup="dialog"] Deactivate…
[button] Save

######## BOARD R2-E11-EV2.dc.html
R2-E11-EV2 · 1280
6.28 saved · Anton Kruger · EV2: /admin/epas/18 by ?created, "EPA saved." focused, the address replaced. PAED-016 reads on no adopted curriculum.
Focus: the result.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] [aria-current="page"] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] EPAs
-
- [aria-current="page"] PAED-016
### PAED-016 — Transitioning adolescents with long-term conditions to adult care
College of Paediatricians of South Africa · Paediatrics / Paediatrics
EPA saved.
PAED-016 is on no curriculum an institution has adopted, so no registrar is measured against it yet.
Sub-speciality*
Paediatrics / Neonatology
[selected] Paediatrics / Paediatrics
College of Paediatricians of South Africa's sub-specialities.
Code*
[value="PAED-016"]
Title*
[value="Transitioning adolescents with long-term conditions to adult care"]
Description
Plans and leads the handover of an adolescent with a long-term condition to adult services, with the young person and their caregivers.
Required knowledge and skills
Category*
[selected] Core
Elective
Status
(checkbox) [checked]
Active
An inactive EPA cannot be chosen for a new activity and is not a target on any running programme's progress page or dashboard; a programme that has ended keeps it on its record. Evidence already recorded against it is kept, and so is the progress it earned while it was active. An activity completed against it while it is inactive counts towards nothing until the EPA is reactivated, and reactivating it credits those activities.
[link] Cancel
[button] [aria-haspopup="dialog"] Deactivate…
[button] Save

######## BOARD R2-E12.dc.html
R2-E12 · 1280
Required · Code and Title empty: "Enter a code." and "Enter a title." under their fields (built).
Focus: Code.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] [aria-current="page"] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] EPAs
-
- [aria-current="page"] Create EPA
### Create EPA
A national EPA of College of Paediatricians of South Africa. It reaches a registrar once a curriculum an institution has adopted holds it.
Sub-speciality*
Paediatrics / Neonatology
[selected] Paediatrics / Paediatrics
College of Paediatricians of South Africa's sub-specialities.
Code*
Enter a code.
Title*
Enter a title.
Description
Required knowledge and skills
Category*
[selected] Core
Elective
[link] Cancel
[button] Save

######## BOARD R2-E12-390.dc.html
R2-E12-390 · 390
Required · Code and Title empty: "Enter a code." and "Enter a title." under their fields (built).
Focus: Code.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] [aria-current="page"] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] EPAs
-
- [aria-current="page"] Create EPA
### Create EPA
A national EPA of College of Paediatricians of South Africa. It reaches a registrar once a curriculum an institution has adopted holds it.
Sub-speciality*
Paediatrics / Neonatology
[selected] Paediatrics / Paediatrics
College of Paediatricians of South Africa's sub-specialities.
Code*
Enter a code.
Title*
Enter a title.
Description
Required knowledge and skills
Category*
[selected] Core
Elective
[link] Cancel
[button] Save

######## BOARD R2-E13.dc.html
R2-E13 · 1280
A duplicate code · the built words under Code.
Focus: Code.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] [aria-current="page"] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] EPAs
-
- [aria-current="page"] Create EPA
### Create EPA
A national EPA of College of Paediatricians of South Africa. It reaches a registrar once a curriculum an institution has adopted holds it.
Sub-speciality*
Paediatrics / Neonatology
[selected] Paediatrics / Paediatrics
College of Paediatricians of South Africa's sub-specialities.
Code*
[value="PAED-016"]
An EPA with the same code already exists for this sub-speciality.
Title*
[value="Transitioning adolescents with long-term conditions to adult care"]
Description
‹description›
Required knowledge and skills
Category*
[selected] Core
Elective
[link] Cancel
[button] Save

######## BOARD R2-E13-390.dc.html
R2-E13-390 · 390
A duplicate code · the built words under Code.
Focus: Code.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] [aria-current="page"] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] EPAs
-
- [aria-current="page"] Create EPA
### Create EPA
A national EPA of College of Paediatricians of South Africa. It reaches a registrar once a curriculum an institution has adopted holds it.
Sub-speciality*
Paediatrics / Neonatology
[selected] Paediatrics / Paediatrics
College of Paediatricians of South Africa's sub-specialities.
Code*
[value="PAED-016"]
An EPA with the same code already exists for this sub-speciality.
Title*
[value="Transitioning adolescents with long-term conditions to adult care"]
Description
‹description›
Required knowledge and skills
Category*
[selected] Core
Elective
[link] Cancel
[button] Save

######## BOARD R2-E14.dc.html
IllustrationR2-E14 · 1280
A failed save · built words.
Focus: the failure.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] [aria-current="page"] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] EPAs
-
- [aria-current="page"] PAED-006
### PAED-006 — Managing long-term health conditions (LTHCs) in children
College of Paediatricians of South Africa · Paediatrics / Paediatrics
The EPA could not be saved. Nothing has changed. Try again, or come back in a few minutes.
Sub-speciality*
Paediatrics / Neonatology
[selected] Paediatrics / Paediatrics
College of Paediatricians of South Africa's sub-specialities.
Code*
[value="PAED-006"]
Title*
[value="Managing long-term health conditions (LTHCs) in children"]
Description
‹description›
Required knowledge and skills
‹required knowledge and skills›
Category*
[selected] Core
Elective
Status
(checkbox) [checked]
Active
An inactive EPA cannot be chosen for a new activity and is not a target on any running programme's progress page or dashboard; a programme that has ended keeps it on its record. Evidence already recorded against it is kept, and so is the progress it earned while it was active. An activity completed against it while it is inactive counts towards nothing until the EPA is reactivated, and reactivating it credits those activities.
[link] Cancel
[button] [aria-haspopup="dialog"] Deactivate…
[button] Save

######## BOARD R2-E14-390.dc.html
IllustrationR2-E14-390 · 390
A failed save · built words.
Focus: the failure.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] [aria-current="page"] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] EPAs
-
- [aria-current="page"] PAED-006
### PAED-006 — Managing long-term health conditions (LTHCs) in children
College of Paediatricians of South Africa · Paediatrics / Paediatrics
The EPA could not be saved. Nothing has changed. Try again, or come back in a few minutes.
Sub-speciality*
Paediatrics / Neonatology
[selected] Paediatrics / Paediatrics
College of Paediatricians of South Africa's sub-specialities.
Code*
[value="PAED-006"]
Title*
[value="Managing long-term health conditions (LTHCs) in children"]
Description
‹description›
Required knowledge and skills
‹required knowledge and skills›
Category*
[selected] Core
Elective
Status
(checkbox) [checked]
Active
An inactive EPA cannot be chosen for a new activity and is not a target on any running programme's progress page or dashboard; a programme that has ended keeps it on its record. Evidence already recorded against it is kept, and so is the progress it earned while it was active. An activity completed against it while it is inactive counts towards nothing until the EPA is reactivated, and reactivating it credits those activities.
[link] Cancel
[button] [aria-haspopup="dialog"] Deactivate…
[button] Save

######## BOARD R2-E15.dc.html
R2-E15 · 1280
Q7 · Anton Kruger types /admin/epas/17 (KGK-001): "EPA unavailable", as flow 15 built it.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] [aria-current="page"] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
- [aria-current="page"] EPAs
### EPA unavailable
This EPA does not exist, or you cannot open it.
[link] Go to EPAs

######## BOARD R2-E15-390.dc.html
R2-E15-390 · 390
Q7 · Anton Kruger types /admin/epas/17 (KGK-001): "EPA unavailable", as flow 15 built it.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] [aria-current="page"] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
- [aria-current="page"] EPAs
### EPA unavailable
This EPA does not exist, or you cannot open it.
[link] Go to EPAs

######## BOARD R2-E16.dc.html
R2-E16 · 1280
Loading · the list.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] [aria-current="page"] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
### EPAs
College of Paediatricians of South Africa's EPAs. A change to one is read at once wherever it is named, in every institution that has adopted a curriculum that holds it.
Loading the EPAs.

######## BOARD R2-E16-390.dc.html
R2-E16-390 · 390
Loading · the list.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] [aria-current="page"] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
### EPAs
College of Paediatricians of South Africa's EPAs. A change to one is read at once wherever it is named, in every institution that has adopted a curriculum that holds it.
Loading the EPAs.

######## BOARD R2-E17.dc.html
R2-E17 · 1280
Load error · the list (built words).
Focus: Try again → the h1.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] [aria-current="page"] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
### EPAs
College of Paediatricians of South Africa's EPAs. A change to one is read at once wherever it is named, in every institution that has adopted a curriculum that holds it.
Could not load the EPAs. Nothing has changed. Try again, or come back in a few minutes.
[button] Try again

######## BOARD R2-E17-390.dc.html
R2-E17-390 · 390
Load error · the list (built words).
Focus: Try again → the h1.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] [aria-current="page"] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
### EPAs
College of Paediatricians of South Africa's EPAs. A change to one is read at once wherever it is named, in every institution that has adopted a curriculum that holds it.
Could not load the EPAs. Nothing has changed. Try again, or come back in a few minutes.
[button] Try again

######## BOARD R2-E18a.dc.html
R2-E18a · 1280
Loading · an EPA's page.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] [aria-current="page"] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
- [aria-current="page"] EPAs
### EPA
Loading this EPA.

######## BOARD R2-E18a-390.dc.html
R2-E18a-390 · 390
Loading · an EPA's page.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] [aria-current="page"] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
- [aria-current="page"] EPAs
### EPA
Loading this EPA.

######## BOARD R2-E18b.dc.html
R2-E18b · 1280
Load error · an EPA's page.
Focus: Try again → the h1.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] [aria-current="page"] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
- [aria-current="page"] EPAs
### EPA
Could not load this EPA. Nothing has changed. Try again, or come back in a few minutes.
[button] Try again

######## BOARD R2-E18b-390.dc.html
R2-E18b-390 · 390
Load error · an EPA's page.
Focus: Try again → the h1.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] [aria-current="page"] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
- [aria-current="page"] EPAs
### EPA
Could not load this EPA. Nothing has changed. Try again, or come back in a few minutes.
[button] Try again

######## BOARD R2-E19.dc.html
IllustrationR2-E19 · 1280
A College admin with no College on EPAs · the NEW empty state; no Create EPA (today chosen by MayCreate alone).
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] [aria-current="page"] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
### EPAs
No College on your account
Your account names no College. Ask the platform administrator to add it.

######## BOARD R2-E19-390.dc.html
IllustrationR2-E19-390 · 390
A College admin with no College on EPAs · the NEW empty state; no Create EPA (today chosen by MayCreate alone).
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] [aria-current="page"] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
### EPAs
No College on your account
Your account names no College. Ask the platform administrator to add it.

######## BOARD R2-E20.dc.html
IllustrationR2-E20 · 1280
Demo Administrator on PAED-012's page · the form, the College named in the subtitle; the Administrator's Sub-speciality help.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] [aria-current="page"] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] EPAs
-
- [aria-current="page"] PAED-012
### PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
College of Paediatricians of South Africa · Paediatrics / Paediatrics
Sub-speciality*
Paediatrics / Neonatology
[selected] Paediatrics / Paediatrics
Every College's sub-specialities.
Code*
[value="PAED-012"]
Title*
[value="Communicating with and counselling patients, caregivers and healthcare teams"]
Description
‹description›
Required knowledge and skills
‹required knowledge and skills›
Category*
[selected] Core
Elective
Status
(checkbox) [checked]
Active
An inactive EPA cannot be chosen for a new activity and is not a target on any running programme's progress page or dashboard; a programme that has ended keeps it on its record. Evidence already recorded against it is kept, and so is the progress it earned while it was active. An activity completed against it while it is inactive counts towards nothing until the EPA is reactivated, and reactivating it credits those activities.
[link] Cancel
[button] [aria-haspopup="dialog"] Deactivate…
[button] Save

######## BOARD R2-E20-390.dc.html
IllustrationR2-E20-390 · 390
Demo Administrator on PAED-012's page · the form, the College named in the subtitle; the Administrator's Sub-speciality help.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] [aria-current="page"] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] EPAs
-
- [aria-current="page"] PAED-012
### PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
College of Paediatricians of South Africa · Paediatrics / Paediatrics
Sub-speciality*
Paediatrics / Neonatology
[selected] Paediatrics / Paediatrics
Every College's sub-specialities.
Code*
[value="PAED-012"]
Title*
[value="Communicating with and counselling patients, caregivers and healthcare teams"]
Description
‹description›
Required knowledge and skills
‹required knowledge and skills›
Category*
[selected] Core
Elective
Status
(checkbox) [checked]
Active
An inactive EPA cannot be chosen for a new activity and is not a target on any running programme's progress page or dashboard; a programme that has ended keeps it on its record. Evidence already recorded against it is kept, and so is the progress it earned while it was active. An activity completed against it while it is inactive counts towards nothing until the EPA is reactivated, and reactivating it credits those activities.
[link] Cancel
[button] [aria-haspopup="dialog"] Deactivate…
[button] Save

######## BOARD R2-F01.dc.html
R2-F01 · 1280
1.17 · Anton Kruger · one row, 11.1 Open, 15; the NEW subtitle (T326); the built caption, no section heading, no College column.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
### Curricula
College of Paediatricians of South Africa's curriculum versions. A new version starts held back; an institution can adopt one once you open it.
[link] Create curriculum
College of Paediatricians of South Africa's curriculum versions
| Curriculum | Discipline | Effective | Items | Status | Actions |
| Paediatric EPA Curriculum (11.1) | Paediatrics / Paediatrics | 2026-01-01 | 15 | Open | [link] [aria-label="Edit Paediatric EPA Curriculum (11.1)"] Edit [link] [aria-label="Items of Paediatric EPA Curriculum (11.1)"] Items |

######## BOARD R2-F01-390.dc.html
R2-F01-390 · 390
1.17 · Anton Kruger · one row, 11.1 Open, 15; the NEW subtitle (T326); the built caption, no section heading, no College column.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
### Curricula
College of Paediatricians of South Africa's curriculum versions. A new version starts held back; an institution can adopt one once you open it.
[link] Create curriculum
College of Paediatricians of South Africa's curriculum versions
| Curriculum | Discipline | Effective | Items | Status | Actions |
| Paediatric EPA Curriculum (11.1) | Paediatrics / Paediatrics | 2026-01-01 | 15 | Open | [link] [aria-label="Edit Paediatric EPA Curriculum (11.1)"] Edit [link] [aria-label="Items of Paediatric EPA Curriculum (11.1)"] Items |

######## BOARD R2-F01-768.dc.html
R2-F01-768 · 768
1.17 · Anton Kruger · one row, 11.1 Open, 15; the NEW subtitle (T326); the built caption, no section heading, no College column.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
### Curricula
College of Paediatricians of South Africa's curriculum versions. A new version starts held back; an institution can adopt one once you open it.
[link] Create curriculum
College of Paediatricians of South Africa's curriculum versions
| Curriculum | Discipline | Effective | Items | Status | Actions |
| Paediatric EPA Curriculum (11.1) | Paediatrics / Paediatrics | 2026-01-01 | 15 | Open | [link] [aria-label="Edit Paediatric EPA Curriculum (11.1)"] Edit [link] [aria-label="Items of Paediatric EPA Curriculum (11.1)"] Items |

######## BOARD R2-F02.dc.html
R2-F02 · 1280
After 6.13 · Neonatology 1.0 Held back, 0 items; 11.1 Open.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
### Curricula
College of Paediatricians of South Africa's curriculum versions. A new version starts held back; an institution can adopt one once you open it.
[link] Create curriculum
College of Paediatricians of South Africa's curriculum versions
| Curriculum | Discipline | Effective | Items | Status | Actions |
| Neonatology EPA Curriculum (1.0) | Paediatrics / Neonatology | 2026-11-02 | 0 | Held back | [link] [aria-label="Edit Neonatology EPA Curriculum (1.0)"] Edit [link] [aria-label="Items of Neonatology EPA Curriculum (1.0)"] Items |
| Paediatric EPA Curriculum (11.1) | Paediatrics / Paediatrics | 2026-01-01 | 15 | Open | [link] [aria-label="Edit Paediatric EPA Curriculum (11.1)"] Edit [link] [aria-label="Items of Paediatric EPA Curriculum (11.1)"] Items |

######## BOARD R2-F02-390.dc.html
R2-F02-390 · 390
After 6.13 · Neonatology 1.0 Held back, 0 items; 11.1 Open.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
### Curricula
College of Paediatricians of South Africa's curriculum versions. A new version starts held back; an institution can adopt one once you open it.
[link] Create curriculum
College of Paediatricians of South Africa's curriculum versions
| Curriculum | Discipline | Effective | Items | Status | Actions |
| Neonatology EPA Curriculum (1.0) | Paediatrics / Neonatology | 2026-11-02 | 0 | Held back | [link] [aria-label="Edit Neonatology EPA Curriculum (1.0)"] Edit [link] [aria-label="Items of Neonatology EPA Curriculum (1.0)"] Items |
| Paediatric EPA Curriculum (11.1) | Paediatrics / Paediatrics | 2026-01-01 | 15 | Open | [link] [aria-label="Edit Paediatric EPA Curriculum (11.1)"] Edit [link] [aria-label="Items of Paediatric EPA Curriculum (11.1)"] Items |

######## BOARD R2-F03.dc.html
R2-F03 · 1280
After 6.29 · 11.2 Held back, 15.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
### Curricula
College of Paediatricians of South Africa's curriculum versions. A new version starts held back; an institution can adopt one once you open it.
[link] Create curriculum
College of Paediatricians of South Africa's curriculum versions
| Curriculum | Discipline | Effective | Items | Status | Actions |
| Neonatology EPA Curriculum (1.0) | Paediatrics / Neonatology | 2026-11-02 | 0 | Held back | [link] [aria-label="Edit Neonatology EPA Curriculum (1.0)"] Edit [link] [aria-label="Items of Neonatology EPA Curriculum (1.0)"] Items |
| Paediatric EPA Curriculum (11.2) | Paediatrics / Paediatrics | 2026-11-02 | 15 | Held back | [link] [aria-label="Edit Paediatric EPA Curriculum (11.2)"] Edit [link] [aria-label="Items of Paediatric EPA Curriculum (11.2)"] Items |
| Paediatric EPA Curriculum (11.1) | Paediatrics / Paediatrics | 2026-01-01 | 15 | Open | [link] [aria-label="Edit Paediatric EPA Curriculum (11.1)"] Edit [link] [aria-label="Items of Paediatric EPA Curriculum (11.1)"] Items |

######## BOARD R2-F03-390.dc.html
R2-F03-390 · 390
After 6.29 · 11.2 Held back, 15.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
### Curricula
College of Paediatricians of South Africa's curriculum versions. A new version starts held back; an institution can adopt one once you open it.
[link] Create curriculum
College of Paediatricians of South Africa's curriculum versions
| Curriculum | Discipline | Effective | Items | Status | Actions |
| Neonatology EPA Curriculum (1.0) | Paediatrics / Neonatology | 2026-11-02 | 0 | Held back | [link] [aria-label="Edit Neonatology EPA Curriculum (1.0)"] Edit [link] [aria-label="Items of Neonatology EPA Curriculum (1.0)"] Items |
| Paediatric EPA Curriculum (11.2) | Paediatrics / Paediatrics | 2026-11-02 | 15 | Held back | [link] [aria-label="Edit Paediatric EPA Curriculum (11.2)"] Edit [link] [aria-label="Items of Paediatric EPA Curriculum (11.2)"] Items |
| Paediatric EPA Curriculum (11.1) | Paediatrics / Paediatrics | 2026-01-01 | 15 | Open | [link] [aria-label="Edit Paediatric EPA Curriculum (11.1)"] Edit [link] [aria-label="Items of Paediatric EPA Curriculum (11.1)"] Items |

######## BOARD R2-F04.dc.html
R2-F04 · 1280
After 6.31 · Neonatology Held back; 11.2 Open, 16; 11.1 Open, 15.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
### Curricula
College of Paediatricians of South Africa's curriculum versions. A new version starts held back; an institution can adopt one once you open it.
[link] Create curriculum
College of Paediatricians of South Africa's curriculum versions
| Curriculum | Discipline | Effective | Items | Status | Actions |
| Neonatology EPA Curriculum (1.0) | Paediatrics / Neonatology | 2026-11-02 | 0 | Held back | [link] [aria-label="Edit Neonatology EPA Curriculum (1.0)"] Edit [link] [aria-label="Items of Neonatology EPA Curriculum (1.0)"] Items |
| Paediatric EPA Curriculum (11.2) | Paediatrics / Paediatrics | 2026-11-02 | 16 | Open | [link] [aria-label="Edit Paediatric EPA Curriculum (11.2)"] Edit [link] [aria-label="Items of Paediatric EPA Curriculum (11.2)"] Items |
| Paediatric EPA Curriculum (11.1) | Paediatrics / Paediatrics | 2026-01-01 | 15 | Open | [link] [aria-label="Edit Paediatric EPA Curriculum (11.1)"] Edit [link] [aria-label="Items of Paediatric EPA Curriculum (11.1)"] Items |

######## BOARD R2-F04-390.dc.html
R2-F04-390 · 390
After 6.31 · Neonatology Held back; 11.2 Open, 16; 11.1 Open, 15.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
### Curricula
College of Paediatricians of South Africa's curriculum versions. A new version starts held back; an institution can adopt one once you open it.
[link] Create curriculum
College of Paediatricians of South Africa's curriculum versions
| Curriculum | Discipline | Effective | Items | Status | Actions |
| Neonatology EPA Curriculum (1.0) | Paediatrics / Neonatology | 2026-11-02 | 0 | Held back | [link] [aria-label="Edit Neonatology EPA Curriculum (1.0)"] Edit [link] [aria-label="Items of Neonatology EPA Curriculum (1.0)"] Items |
| Paediatric EPA Curriculum (11.2) | Paediatrics / Paediatrics | 2026-11-02 | 16 | Open | [link] [aria-label="Edit Paediatric EPA Curriculum (11.2)"] Edit [link] [aria-label="Items of Paediatric EPA Curriculum (11.2)"] Items |
| Paediatric EPA Curriculum (11.1) | Paediatrics / Paediatrics | 2026-01-01 | 15 | Open | [link] [aria-label="Edit Paediatric EPA Curriculum (11.1)"] Edit [link] [aria-label="Items of Paediatric EPA Curriculum (11.1)"] Items |

######## BOARD R2-F04-768.dc.html
R2-F04-768 · 768
After 6.31 · Neonatology Held back; 11.2 Open, 16; 11.1 Open, 15.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
### Curricula
College of Paediatricians of South Africa's curriculum versions. A new version starts held back; an institution can adopt one once you open it.
[link] Create curriculum
College of Paediatricians of South Africa's curriculum versions
| Curriculum | Discipline | Effective | Items | Status | Actions |
| Neonatology EPA Curriculum (1.0) | Paediatrics / Neonatology | 2026-11-02 | 0 | Held back | [link] [aria-label="Edit Neonatology EPA Curriculum (1.0)"] Edit [link] [aria-label="Items of Neonatology EPA Curriculum (1.0)"] Items |
| Paediatric EPA Curriculum (11.2) | Paediatrics / Paediatrics | 2026-11-02 | 16 | Open | [link] [aria-label="Edit Paediatric EPA Curriculum (11.2)"] Edit [link] [aria-label="Items of Paediatric EPA Curriculum (11.2)"] Items |
| Paediatric EPA Curriculum (11.1) | Paediatrics / Paediatrics | 2026-01-01 | 15 | Open | [link] [aria-label="Edit Paediatric EPA Curriculum (11.1)"] Edit [link] [aria-label="Items of Paediatric EPA Curriculum (11.1)"] Items |

######## BOARD R2-F05.dc.html
R2-F05 · 1280
1.18 · Anton Kruger · 11.1's items: h1 and trail as built, no Back links; "Edit this version" in the header (T4); the notice with the reach first, bold (Q2); "Items (15)"; 15 rows, Edit and Remove… named for the EPA; the nothing-to-add line.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Curricula
-
- [aria-current="page"] Items of Paediatric EPA Curriculum (11.1)
### Items of Paediatric EPA Curriculum (11.1)
College of Paediatricians of South Africa · Paediatrics / Paediatrics · effective 2026-01-01
[link] [aria-label="Edit Paediatric EPA Curriculum (11.1)"] Edit this version
No institution has adopted this version yet. A target and its period are read live. Changing one changes what every trainee on this curriculum is measured against, in every institution that has adopted it, including periods that have already closed. A tool list is checked when an activity is filed and submitted, and never takes back credit already earned.
#### Items (15)
Every national EPA of Paediatrics is already on this curriculum, as a national item or as an institution's own item, so there is none left to add. To add an item, first add a national EPA of Paediatrics, or remove a national item to free its EPA. An EPA an institution has as its own item is freed only when that institution removes it.
Paediatric EPA Curriculum (11.1)'s items, in EPA order
| EPA | Target and decision | Minimum level | Minimum by training year | Tools | Actions |
| PAED-001 — Providing paediatric emergency care to children | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | [link] [aria-label="Edit PAED-001"] Edit [button] [aria-label="Remove PAED-001"] [aria-haspopup="dialog"] Remove… |
| PAED-002 — Managing common paediatric presentations | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, CCA, Chart-stimulated recall, Clinical audit, Direct observation, DOPS, Mini-CEX, MSF, RCA | [link] [aria-label="Edit PAED-002"] Edit [button] [aria-label="Remove PAED-002"] [aria-haspopup="dialog"] Remove… |
| PAED-003 — Providing intensive care to children | 3 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | [link] [aria-label="Edit PAED-003"] Edit [button] [aria-label="Remove PAED-003"] [aria-haspopup="dialog"] Remove… |
| PAED-004 — Managing common neonatal conditions | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, CCA, Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-004"] Edit [button] [aria-label="Remove PAED-004"] [aria-haspopup="dialog"] Remove… |
| PAED-005 — Providing neonatal care in intensive and high-care settings | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, DOPS, MSF | [link] [aria-label="Edit PAED-005"] Edit [button] [aria-label="Remove PAED-005"] [aria-haspopup="dialog"] Remove… |
| PAED-006 — Managing long-term health conditions (LTHCs) | 2 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CCA, Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-006"] Edit [button] [aria-label="Remove PAED-006"] [aria-haspopup="dialog"] Remove… |
| PAED-007 — Maintaining and promoting the health and well-being of children | 1 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-007"] Edit [button] [aria-label="Remove PAED-007"] [aria-haspopup="dialog"] Remove… |
| PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, DOPS, Mini-CEX, MSF, Reflective exercise | [link] [aria-label="Edit PAED-008"] Edit [button] [aria-label="Remove PAED-008"] [aria-haspopup="dialog"] Remove… |
| PAED-009 — Providing care for an adolescent patient | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Direct observation, MSF | [link] [aria-label="Edit PAED-009"] Edit [button] [aria-label="Remove PAED-009"] [aria-haspopup="dialog"] Remove… |
| PAED-010 — Leading and operating within a clinical team | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | Direct observation, MSF | [link] [aria-label="Edit PAED-010"] Edit [button] [aria-label="Remove PAED-010"] [aria-haspopup="dialog"] Remove… |
| PAED-011 — Managing population health challenges | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Direct observation, MSF | [link] [aria-label="Edit PAED-011"] Edit [button] [aria-label="Remove PAED-011"] [aria-haspopup="dialog"] Remove… |
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, Chart-stimulated recall, Mini-CEX, MSF | [link] [aria-label="Edit PAED-012"] Edit [button] [aria-label="Remove PAED-012"] [aria-haspopup="dialog"] Remove… |
| PAED-013 — Managing child protection, legal and ethical aspects of children at risk | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Direct observation, Mini-CEX, MSF | [link] [aria-label="Edit PAED-013"] Edit [button] [aria-label="Remove PAED-013"] [aria-haspopup="dialog"] Remove… |
| PAED-014 — Providing palliative and end-of-life care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, MSF, Reflective exercise | [link] [aria-label="Edit PAED-014"] Edit [button] [aria-label="Remove PAED-014"] [aria-haspopup="dialog"] Remove… |
| PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 1 per semesterDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Direct observation, Learner feedback, MSF, Portfolio and logbook review | [link] [aria-label="Edit PAED-015"] Edit [button] [aria-label="Remove PAED-015"] [aria-haspopup="dialog"] Remove… |

######## BOARD R2-F05-390.dc.html
R2-F05-390 · 390
1.18 · Anton Kruger · 11.1's items: h1 and trail as built, no Back links; "Edit this version" in the header (T4); the notice with the reach first, bold (Q2); "Items (15)"; 15 rows, Edit and Remove… named for the EPA; the nothing-to-add line.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Curricula
-
- [aria-current="page"] Items of Paediatric EPA Curriculum (11.1)
### Items of Paediatric EPA Curriculum (11.1)
College of Paediatricians of South Africa · Paediatrics / Paediatrics · effective 2026-01-01
[link] [aria-label="Edit Paediatric EPA Curriculum (11.1)"] Edit this version
No institution has adopted this version yet. A target and its period are read live. Changing one changes what every trainee on this curriculum is measured against, in every institution that has adopted it, including periods that have already closed. A tool list is checked when an activity is filed and submitted, and never takes back credit already earned.
#### Items (15)
Every national EPA of Paediatrics is already on this curriculum, as a national item or as an institution's own item, so there is none left to add. To add an item, first add a national EPA of Paediatrics, or remove a national item to free its EPA. An EPA an institution has as its own item is freed only when that institution removes it.
Paediatric EPA Curriculum (11.1)'s items, in EPA order
| EPA | Target and decision | Minimum level | Minimum by training year | Tools | Actions |
| PAED-001 — Providing paediatric emergency care to children | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | [link] [aria-label="Edit PAED-001"] Edit [button] [aria-label="Remove PAED-001"] [aria-haspopup="dialog"] Remove… |
| PAED-002 — Managing common paediatric presentations | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, CCA, Chart-stimulated recall, Clinical audit, Direct observation, DOPS, Mini-CEX, MSF, RCA | [link] [aria-label="Edit PAED-002"] Edit [button] [aria-label="Remove PAED-002"] [aria-haspopup="dialog"] Remove… |
| PAED-003 — Providing intensive care to children | 3 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | [link] [aria-label="Edit PAED-003"] Edit [button] [aria-label="Remove PAED-003"] [aria-haspopup="dialog"] Remove… |
| PAED-004 — Managing common neonatal conditions | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, CCA, Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-004"] Edit [button] [aria-label="Remove PAED-004"] [aria-haspopup="dialog"] Remove… |
| PAED-005 — Providing neonatal care in intensive and high-care settings | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, DOPS, MSF | [link] [aria-label="Edit PAED-005"] Edit [button] [aria-label="Remove PAED-005"] [aria-haspopup="dialog"] Remove… |
| PAED-006 — Managing long-term health conditions (LTHCs) | 2 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CCA, Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-006"] Edit [button] [aria-label="Remove PAED-006"] [aria-haspopup="dialog"] Remove… |
| PAED-007 — Maintaining and promoting the health and well-being of children | 1 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-007"] Edit [button] [aria-label="Remove PAED-007"] [aria-haspopup="dialog"] Remove… |
| PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, DOPS, Mini-CEX, MSF, Reflective exercise | [link] [aria-label="Edit PAED-008"] Edit [button] [aria-label="Remove PAED-008"] [aria-haspopup="dialog"] Remove… |
| PAED-009 — Providing care for an adolescent patient | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Direct observation, MSF | [link] [aria-label="Edit PAED-009"] Edit [button] [aria-label="Remove PAED-009"] [aria-haspopup="dialog"] Remove… |
| PAED-010 — Leading and operating within a clinical team | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | Direct observation, MSF | [link] [aria-label="Edit PAED-010"] Edit [button] [aria-label="Remove PAED-010"] [aria-haspopup="dialog"] Remove… |
| PAED-011 — Managing population health challenges | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Direct observation, MSF | [link] [aria-label="Edit PAED-011"] Edit [button] [aria-label="Remove PAED-011"] [aria-haspopup="dialog"] Remove… |
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, Chart-stimulated recall, Mini-CEX, MSF | [link] [aria-label="Edit PAED-012"] Edit [button] [aria-label="Remove PAED-012"] [aria-haspopup="dialog"] Remove… |
| PAED-013 — Managing child protection, legal and ethical aspects of children at risk | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Direct observation, Mini-CEX, MSF | [link] [aria-label="Edit PAED-013"] Edit [button] [aria-label="Remove PAED-013"] [aria-haspopup="dialog"] Remove… |
| PAED-014 — Providing palliative and end-of-life care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, MSF, Reflective exercise | [link] [aria-label="Edit PAED-014"] Edit [button] [aria-label="Remove PAED-014"] [aria-haspopup="dialog"] Remove… |
| PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 1 per semesterDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Direct observation, Learner feedback, MSF, Portfolio and logbook review | [link] [aria-label="Edit PAED-015"] Edit [button] [aria-label="Remove PAED-015"] [aria-haspopup="dialog"] Remove… |

######## BOARD R2-F05-768.dc.html
R2-F05-768 · 768
1.18 · Anton Kruger · 11.1's items: h1 and trail as built, no Back links; "Edit this version" in the header (T4); the notice with the reach first, bold (Q2); "Items (15)"; 15 rows, Edit and Remove… named for the EPA; the nothing-to-add line.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Curricula
-
- [aria-current="page"] Items of Paediatric EPA Curriculum (11.1)
### Items of Paediatric EPA Curriculum (11.1)
College of Paediatricians of South Africa · Paediatrics / Paediatrics · effective 2026-01-01
[link] [aria-label="Edit Paediatric EPA Curriculum (11.1)"] Edit this version
No institution has adopted this version yet. A target and its period are read live. Changing one changes what every trainee on this curriculum is measured against, in every institution that has adopted it, including periods that have already closed. A tool list is checked when an activity is filed and submitted, and never takes back credit already earned.
#### Items (15)
Every national EPA of Paediatrics is already on this curriculum, as a national item or as an institution's own item, so there is none left to add. To add an item, first add a national EPA of Paediatrics, or remove a national item to free its EPA. An EPA an institution has as its own item is freed only when that institution removes it.
Paediatric EPA Curriculum (11.1)'s items, in EPA order
| EPA | Target and decision | Minimum level | Minimum by training year | Tools | Actions |
| PAED-001 — Providing paediatric emergency care to children | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | [link] [aria-label="Edit PAED-001"] Edit [button] [aria-label="Remove PAED-001"] [aria-haspopup="dialog"] Remove… |
| PAED-002 — Managing common paediatric presentations | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, CCA, Chart-stimulated recall, Clinical audit, Direct observation, DOPS, Mini-CEX, MSF, RCA | [link] [aria-label="Edit PAED-002"] Edit [button] [aria-label="Remove PAED-002"] [aria-haspopup="dialog"] Remove… |
| PAED-003 — Providing intensive care to children | 3 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | [link] [aria-label="Edit PAED-003"] Edit [button] [aria-label="Remove PAED-003"] [aria-haspopup="dialog"] Remove… |
| PAED-004 — Managing common neonatal conditions | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, CCA, Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-004"] Edit [button] [aria-label="Remove PAED-004"] [aria-haspopup="dialog"] Remove… |
| PAED-005 — Providing neonatal care in intensive and high-care settings | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, DOPS, MSF | [link] [aria-label="Edit PAED-005"] Edit [button] [aria-label="Remove PAED-005"] [aria-haspopup="dialog"] Remove… |
| PAED-006 — Managing long-term health conditions (LTHCs) | 2 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CCA, Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-006"] Edit [button] [aria-label="Remove PAED-006"] [aria-haspopup="dialog"] Remove… |
| PAED-007 — Maintaining and promoting the health and well-being of children | 1 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-007"] Edit [button] [aria-label="Remove PAED-007"] [aria-haspopup="dialog"] Remove… |
| PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, DOPS, Mini-CEX, MSF, Reflective exercise | [link] [aria-label="Edit PAED-008"] Edit [button] [aria-label="Remove PAED-008"] [aria-haspopup="dialog"] Remove… |
| PAED-009 — Providing care for an adolescent patient | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Direct observation, MSF | [link] [aria-label="Edit PAED-009"] Edit [button] [aria-label="Remove PAED-009"] [aria-haspopup="dialog"] Remove… |
| PAED-010 — Leading and operating within a clinical team | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | Direct observation, MSF | [link] [aria-label="Edit PAED-010"] Edit [button] [aria-label="Remove PAED-010"] [aria-haspopup="dialog"] Remove… |
| PAED-011 — Managing population health challenges | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Direct observation, MSF | [link] [aria-label="Edit PAED-011"] Edit [button] [aria-label="Remove PAED-011"] [aria-haspopup="dialog"] Remove… |
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, Chart-stimulated recall, Mini-CEX, MSF | [link] [aria-label="Edit PAED-012"] Edit [button] [aria-label="Remove PAED-012"] [aria-haspopup="dialog"] Remove… |
| PAED-013 — Managing child protection, legal and ethical aspects of children at risk | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Direct observation, Mini-CEX, MSF | [link] [aria-label="Edit PAED-013"] Edit [button] [aria-label="Remove PAED-013"] [aria-haspopup="dialog"] Remove… |
| PAED-014 — Providing palliative and end-of-life care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, MSF, Reflective exercise | [link] [aria-label="Edit PAED-014"] Edit [button] [aria-label="Remove PAED-014"] [aria-haspopup="dialog"] Remove… |
| PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 1 per semesterDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Direct observation, Learner feedback, MSF, Portfolio and logbook review | [link] [aria-label="Edit PAED-015"] Edit [button] [aria-label="Remove PAED-015"] [aria-haspopup="dialog"] Remove… |

######## BOARD R2-F06.dc.html
R2-F06 · 1280
6.13 · Anton Kruger · Neonatology's items: "Items (0)", Q14's words with Go to EPAs (T326; today "Every national EPA of Neonatology is already on this curriculum"), no table.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Curricula
-
- [aria-current="page"] Items of Neonatology EPA Curriculum (1.0)
### Items of Neonatology EPA Curriculum (1.0)
College of Paediatricians of South Africa · Paediatrics / Neonatology · effective 2026-11-02
[link] [aria-label="Edit Neonatology EPA Curriculum (1.0)"] Edit this version
No institution has adopted this version yet. A target and its period are read live. Changing one changes what every trainee on this curriculum is measured against, in every institution that has adopted it, including periods that have already closed. A tool list is checked when an activity is filed and submitted, and never takes back credit already earned.
#### Items (0)
Neonatology has no national EPA yet. Add one under EPAs, then come back to add it here.
[link] Go to EPAs

######## BOARD R2-F06-390.dc.html
R2-F06-390 · 390
6.13 · Anton Kruger · Neonatology's items: "Items (0)", Q14's words with Go to EPAs (T326; today "Every national EPA of Neonatology is already on this curriculum"), no table.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Curricula
-
- [aria-current="page"] Items of Neonatology EPA Curriculum (1.0)
### Items of Neonatology EPA Curriculum (1.0)
College of Paediatricians of South Africa · Paediatrics / Neonatology · effective 2026-11-02
[link] [aria-label="Edit Neonatology EPA Curriculum (1.0)"] Edit this version
No institution has adopted this version yet. A target and its period are read live. Changing one changes what every trainee on this curriculum is measured against, in every institution that has adopted it, including periods that have already closed. A tool list is checked when an activity is filed and submitted, and never takes back credit already earned.
#### Items (0)
Neonatology has no national EPA yet. Add one under EPAs, then come back to add it here.
[link] Go to EPAs

######## BOARD R2-F07.dc.html
R2-F07 · 1280
6.17 · Anton Kruger · 11.1's items with PAED-012 paused: the info notice under the standing one; the row's "(inactive: not in force)"; the reach (KGK, 3).
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Curricula
-
- [aria-current="page"] Items of Paediatric EPA Curriculum (11.1)
### Items of Paediatric EPA Curriculum (11.1)
College of Paediatricians of South Africa · Paediatrics / Paediatrics · effective 2026-01-01
[link] [aria-label="Edit Paediatric EPA Curriculum (11.1)"] Edit this version
Adopted by 1 institution, Kgosi Kgari Teaching Hospital; 3 registrars in training on it. A target and its period are read live. Changing one changes what every trainee on this curriculum is measured against, in every institution that has adopted it, including periods that have already closed. A tool list is checked when an activity is filed and submitted, and never takes back credit already earned.
PAED-012 is inactive, so its item is not in force: no trainee can choose it for a new activity, its credit is paused, and no running programme's progress page or dashboard shows it (a programme that has ended keeps it on its record). Reactivating an EPA on its own page brings its item back, and credits what was completed against it meanwhile.
#### Items (15)
Every national EPA of Paediatrics is already on this curriculum, as a national item or as an institution's own item, so there is none left to add. To add an item, first add a national EPA of Paediatrics, or remove a national item to free its EPA. An EPA an institution has as its own item is freed only when that institution removes it.
Paediatric EPA Curriculum (11.1)'s items, in EPA order
| EPA | Target and decision | Minimum level | Minimum by training year | Tools | Actions |
| PAED-001 — Providing paediatric emergency care to children | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | [link] [aria-label="Edit PAED-001"] Edit [button] [aria-label="Remove PAED-001"] [aria-haspopup="dialog"] Remove… |
| PAED-002 — Managing common paediatric presentations | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, CCA, Chart-stimulated recall, Clinical audit, Direct observation, DOPS, Mini-CEX, MSF, RCA | [link] [aria-label="Edit PAED-002"] Edit [button] [aria-label="Remove PAED-002"] [aria-haspopup="dialog"] Remove… |
| PAED-003 — Providing intensive care to children | 3 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | [link] [aria-label="Edit PAED-003"] Edit [button] [aria-label="Remove PAED-003"] [aria-haspopup="dialog"] Remove… |
| PAED-004 — Managing common neonatal conditions | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, CCA, Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-004"] Edit [button] [aria-label="Remove PAED-004"] [aria-haspopup="dialog"] Remove… |
| PAED-005 — Providing neonatal care in intensive and high-care settings | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, DOPS, MSF | [link] [aria-label="Edit PAED-005"] Edit [button] [aria-label="Remove PAED-005"] [aria-haspopup="dialog"] Remove… |
| PAED-006 — Managing long-term health conditions (LTHCs) in children | 2 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CCA, Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-006"] Edit [button] [aria-label="Remove PAED-006"] [aria-haspopup="dialog"] Remove… |
| PAED-007 — Maintaining and promoting the health and well-being of children | 1 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-007"] Edit [button] [aria-label="Remove PAED-007"] [aria-haspopup="dialog"] Remove… |
| PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, DOPS, Mini-CEX, MSF, Reflective exercise | [link] [aria-label="Edit PAED-008"] Edit [button] [aria-label="Remove PAED-008"] [aria-haspopup="dialog"] Remove… |
| PAED-009 — Providing care for an adolescent patient | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Direct observation, MSF | [link] [aria-label="Edit PAED-009"] Edit [button] [aria-label="Remove PAED-009"] [aria-haspopup="dialog"] Remove… |
| PAED-010 — Leading and operating within a clinical team | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | Direct observation, MSF | [link] [aria-label="Edit PAED-010"] Edit [button] [aria-label="Remove PAED-010"] [aria-haspopup="dialog"] Remove… |
| PAED-011 — Managing population health challenges | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Direct observation, MSF | [link] [aria-label="Edit PAED-011"] Edit [button] [aria-label="Remove PAED-011"] [aria-haspopup="dialog"] Remove… |
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams (inactive: not in force) | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, Chart-stimulated recall, Mini-CEX, MSF | [link] [aria-label="Edit PAED-012"] Edit [button] [aria-label="Remove PAED-012"] [aria-haspopup="dialog"] Remove… |
| PAED-013 — Managing child protection, legal and ethical aspects of children at risk | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Direct observation, Mini-CEX, MSF | [link] [aria-label="Edit PAED-013"] Edit [button] [aria-label="Remove PAED-013"] [aria-haspopup="dialog"] Remove… |
| PAED-014 — Providing palliative and end-of-life care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, MSF, Reflective exercise | [link] [aria-label="Edit PAED-014"] Edit [button] [aria-label="Remove PAED-014"] [aria-haspopup="dialog"] Remove… |
| PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 1 per semesterDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Direct observation, Learner feedback, MSF, Portfolio and logbook review | [link] [aria-label="Edit PAED-015"] Edit [button] [aria-label="Remove PAED-015"] [aria-haspopup="dialog"] Remove… |

######## BOARD R2-F07-390.dc.html
R2-F07-390 · 390
6.17 · Anton Kruger · 11.1's items with PAED-012 paused: the info notice under the standing one; the row's "(inactive: not in force)"; the reach (KGK, 3).
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Curricula
-
- [aria-current="page"] Items of Paediatric EPA Curriculum (11.1)
### Items of Paediatric EPA Curriculum (11.1)
College of Paediatricians of South Africa · Paediatrics / Paediatrics · effective 2026-01-01
[link] [aria-label="Edit Paediatric EPA Curriculum (11.1)"] Edit this version
Adopted by 1 institution, Kgosi Kgari Teaching Hospital; 3 registrars in training on it. A target and its period are read live. Changing one changes what every trainee on this curriculum is measured against, in every institution that has adopted it, including periods that have already closed. A tool list is checked when an activity is filed and submitted, and never takes back credit already earned.
PAED-012 is inactive, so its item is not in force: no trainee can choose it for a new activity, its credit is paused, and no running programme's progress page or dashboard shows it (a programme that has ended keeps it on its record). Reactivating an EPA on its own page brings its item back, and credits what was completed against it meanwhile.
#### Items (15)
Every national EPA of Paediatrics is already on this curriculum, as a national item or as an institution's own item, so there is none left to add. To add an item, first add a national EPA of Paediatrics, or remove a national item to free its EPA. An EPA an institution has as its own item is freed only when that institution removes it.
Paediatric EPA Curriculum (11.1)'s items, in EPA order
| EPA | Target and decision | Minimum level | Minimum by training year | Tools | Actions |
| PAED-001 — Providing paediatric emergency care to children | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | [link] [aria-label="Edit PAED-001"] Edit [button] [aria-label="Remove PAED-001"] [aria-haspopup="dialog"] Remove… |
| PAED-002 — Managing common paediatric presentations | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, CCA, Chart-stimulated recall, Clinical audit, Direct observation, DOPS, Mini-CEX, MSF, RCA | [link] [aria-label="Edit PAED-002"] Edit [button] [aria-label="Remove PAED-002"] [aria-haspopup="dialog"] Remove… |
| PAED-003 — Providing intensive care to children | 3 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | [link] [aria-label="Edit PAED-003"] Edit [button] [aria-label="Remove PAED-003"] [aria-haspopup="dialog"] Remove… |
| PAED-004 — Managing common neonatal conditions | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, CCA, Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-004"] Edit [button] [aria-label="Remove PAED-004"] [aria-haspopup="dialog"] Remove… |
| PAED-005 — Providing neonatal care in intensive and high-care settings | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, DOPS, MSF | [link] [aria-label="Edit PAED-005"] Edit [button] [aria-label="Remove PAED-005"] [aria-haspopup="dialog"] Remove… |
| PAED-006 — Managing long-term health conditions (LTHCs) in children | 2 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CCA, Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-006"] Edit [button] [aria-label="Remove PAED-006"] [aria-haspopup="dialog"] Remove… |
| PAED-007 — Maintaining and promoting the health and well-being of children | 1 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-007"] Edit [button] [aria-label="Remove PAED-007"] [aria-haspopup="dialog"] Remove… |
| PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, DOPS, Mini-CEX, MSF, Reflective exercise | [link] [aria-label="Edit PAED-008"] Edit [button] [aria-label="Remove PAED-008"] [aria-haspopup="dialog"] Remove… |
| PAED-009 — Providing care for an adolescent patient | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Direct observation, MSF | [link] [aria-label="Edit PAED-009"] Edit [button] [aria-label="Remove PAED-009"] [aria-haspopup="dialog"] Remove… |
| PAED-010 — Leading and operating within a clinical team | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | Direct observation, MSF | [link] [aria-label="Edit PAED-010"] Edit [button] [aria-label="Remove PAED-010"] [aria-haspopup="dialog"] Remove… |
| PAED-011 — Managing population health challenges | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Direct observation, MSF | [link] [aria-label="Edit PAED-011"] Edit [button] [aria-label="Remove PAED-011"] [aria-haspopup="dialog"] Remove… |
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams (inactive: not in force) | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, Chart-stimulated recall, Mini-CEX, MSF | [link] [aria-label="Edit PAED-012"] Edit [button] [aria-label="Remove PAED-012"] [aria-haspopup="dialog"] Remove… |
| PAED-013 — Managing child protection, legal and ethical aspects of children at risk | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Direct observation, Mini-CEX, MSF | [link] [aria-label="Edit PAED-013"] Edit [button] [aria-label="Remove PAED-013"] [aria-haspopup="dialog"] Remove… |
| PAED-014 — Providing palliative and end-of-life care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, MSF, Reflective exercise | [link] [aria-label="Edit PAED-014"] Edit [button] [aria-label="Remove PAED-014"] [aria-haspopup="dialog"] Remove… |
| PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 1 per semesterDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Direct observation, Learner feedback, MSF, Portfolio and logbook review | [link] [aria-label="Edit PAED-015"] Edit [button] [aria-label="Remove PAED-015"] [aria-haspopup="dialog"] Remove… |

######## BOARD R2-F08.dc.html
R2-F08 · 1280
6.29 · Anton Kruger · 11.1's items: "Add item" beside "Items (15)" (PAED-016 is on no curriculum); the reach (KGK, 3); no KGK-001 (T211).
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Curricula
-
- [aria-current="page"] Items of Paediatric EPA Curriculum (11.1)
### Items of Paediatric EPA Curriculum (11.1)
College of Paediatricians of South Africa · Paediatrics / Paediatrics · effective 2026-01-01
[link] [aria-label="Edit Paediatric EPA Curriculum (11.1)"] Edit this version
Adopted by 1 institution, Kgosi Kgari Teaching Hospital; 3 registrars in training on it. A target and its period are read live. Changing one changes what every trainee on this curriculum is measured against, in every institution that has adopted it, including periods that have already closed. A tool list is checked when an activity is filed and submitted, and never takes back credit already earned.
#### Items (15)
[link] Add item
Paediatric EPA Curriculum (11.1)'s items, in EPA order
| EPA | Target and decision | Minimum level | Minimum by training year | Tools | Actions |
| PAED-001 — Providing paediatric emergency care to children | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | [link] [aria-label="Edit PAED-001"] Edit [button] [aria-label="Remove PAED-001"] [aria-haspopup="dialog"] Remove… |
| PAED-002 — Managing common paediatric presentations | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, CCA, Chart-stimulated recall, Clinical audit, Direct observation, DOPS, Mini-CEX, MSF, RCA | [link] [aria-label="Edit PAED-002"] Edit [button] [aria-label="Remove PAED-002"] [aria-haspopup="dialog"] Remove… |
| PAED-003 — Providing intensive care to children | 3 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | [link] [aria-label="Edit PAED-003"] Edit [button] [aria-label="Remove PAED-003"] [aria-haspopup="dialog"] Remove… |
| PAED-004 — Managing common neonatal conditions | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, CCA, Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-004"] Edit [button] [aria-label="Remove PAED-004"] [aria-haspopup="dialog"] Remove… |
| PAED-005 — Providing neonatal care in intensive and high-care settings | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, DOPS, MSF | [link] [aria-label="Edit PAED-005"] Edit [button] [aria-label="Remove PAED-005"] [aria-haspopup="dialog"] Remove… |
| PAED-006 — Managing long-term health conditions (LTHCs) in children | 2 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CCA, Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-006"] Edit [button] [aria-label="Remove PAED-006"] [aria-haspopup="dialog"] Remove… |
| PAED-007 — Maintaining and promoting the health and well-being of children | 1 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-007"] Edit [button] [aria-label="Remove PAED-007"] [aria-haspopup="dialog"] Remove… |
| PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, DOPS, Mini-CEX, MSF, Reflective exercise | [link] [aria-label="Edit PAED-008"] Edit [button] [aria-label="Remove PAED-008"] [aria-haspopup="dialog"] Remove… |
| PAED-009 — Providing care for an adolescent patient | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Direct observation, MSF | [link] [aria-label="Edit PAED-009"] Edit [button] [aria-label="Remove PAED-009"] [aria-haspopup="dialog"] Remove… |
| PAED-010 — Leading and operating within a clinical team | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | Direct observation, MSF | [link] [aria-label="Edit PAED-010"] Edit [button] [aria-label="Remove PAED-010"] [aria-haspopup="dialog"] Remove… |
| PAED-011 — Managing population health challenges | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Direct observation, MSF | [link] [aria-label="Edit PAED-011"] Edit [button] [aria-label="Remove PAED-011"] [aria-haspopup="dialog"] Remove… |
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, Chart-stimulated recall, Mini-CEX, MSF | [link] [aria-label="Edit PAED-012"] Edit [button] [aria-label="Remove PAED-012"] [aria-haspopup="dialog"] Remove… |
| PAED-013 — Managing child protection, legal and ethical aspects of children at risk | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Direct observation, Mini-CEX, MSF | [link] [aria-label="Edit PAED-013"] Edit [button] [aria-label="Remove PAED-013"] [aria-haspopup="dialog"] Remove… |
| PAED-014 — Providing palliative and end-of-life care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, MSF, Reflective exercise | [link] [aria-label="Edit PAED-014"] Edit [button] [aria-label="Remove PAED-014"] [aria-haspopup="dialog"] Remove… |
| PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 1 per semesterDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Direct observation, Learner feedback, MSF, Portfolio and logbook review | [link] [aria-label="Edit PAED-015"] Edit [button] [aria-label="Remove PAED-015"] [aria-haspopup="dialog"] Remove… |

######## BOARD R2-F08-390.dc.html
R2-F08-390 · 390
6.29 · Anton Kruger · 11.1's items: "Add item" beside "Items (15)" (PAED-016 is on no curriculum); the reach (KGK, 3); no KGK-001 (T211).
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Curricula
-
- [aria-current="page"] Items of Paediatric EPA Curriculum (11.1)
### Items of Paediatric EPA Curriculum (11.1)
College of Paediatricians of South Africa · Paediatrics / Paediatrics · effective 2026-01-01
[link] [aria-label="Edit Paediatric EPA Curriculum (11.1)"] Edit this version
Adopted by 1 institution, Kgosi Kgari Teaching Hospital; 3 registrars in training on it. A target and its period are read live. Changing one changes what every trainee on this curriculum is measured against, in every institution that has adopted it, including periods that have already closed. A tool list is checked when an activity is filed and submitted, and never takes back credit already earned.
#### Items (15)
[link] Add item
Paediatric EPA Curriculum (11.1)'s items, in EPA order
| EPA | Target and decision | Minimum level | Minimum by training year | Tools | Actions |
| PAED-001 — Providing paediatric emergency care to children | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | [link] [aria-label="Edit PAED-001"] Edit [button] [aria-label="Remove PAED-001"] [aria-haspopup="dialog"] Remove… |
| PAED-002 — Managing common paediatric presentations | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, CCA, Chart-stimulated recall, Clinical audit, Direct observation, DOPS, Mini-CEX, MSF, RCA | [link] [aria-label="Edit PAED-002"] Edit [button] [aria-label="Remove PAED-002"] [aria-haspopup="dialog"] Remove… |
| PAED-003 — Providing intensive care to children | 3 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | [link] [aria-label="Edit PAED-003"] Edit [button] [aria-label="Remove PAED-003"] [aria-haspopup="dialog"] Remove… |
| PAED-004 — Managing common neonatal conditions | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, CCA, Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-004"] Edit [button] [aria-label="Remove PAED-004"] [aria-haspopup="dialog"] Remove… |
| PAED-005 — Providing neonatal care in intensive and high-care settings | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, DOPS, MSF | [link] [aria-label="Edit PAED-005"] Edit [button] [aria-label="Remove PAED-005"] [aria-haspopup="dialog"] Remove… |
| PAED-006 — Managing long-term health conditions (LTHCs) in children | 2 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CCA, Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-006"] Edit [button] [aria-label="Remove PAED-006"] [aria-haspopup="dialog"] Remove… |
| PAED-007 — Maintaining and promoting the health and well-being of children | 1 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-007"] Edit [button] [aria-label="Remove PAED-007"] [aria-haspopup="dialog"] Remove… |
| PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, DOPS, Mini-CEX, MSF, Reflective exercise | [link] [aria-label="Edit PAED-008"] Edit [button] [aria-label="Remove PAED-008"] [aria-haspopup="dialog"] Remove… |
| PAED-009 — Providing care for an adolescent patient | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Direct observation, MSF | [link] [aria-label="Edit PAED-009"] Edit [button] [aria-label="Remove PAED-009"] [aria-haspopup="dialog"] Remove… |
| PAED-010 — Leading and operating within a clinical team | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | Direct observation, MSF | [link] [aria-label="Edit PAED-010"] Edit [button] [aria-label="Remove PAED-010"] [aria-haspopup="dialog"] Remove… |
| PAED-011 — Managing population health challenges | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Direct observation, MSF | [link] [aria-label="Edit PAED-011"] Edit [button] [aria-label="Remove PAED-011"] [aria-haspopup="dialog"] Remove… |
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, Chart-stimulated recall, Mini-CEX, MSF | [link] [aria-label="Edit PAED-012"] Edit [button] [aria-label="Remove PAED-012"] [aria-haspopup="dialog"] Remove… |
| PAED-013 — Managing child protection, legal and ethical aspects of children at risk | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Direct observation, Mini-CEX, MSF | [link] [aria-label="Edit PAED-013"] Edit [button] [aria-label="Remove PAED-013"] [aria-haspopup="dialog"] Remove… |
| PAED-014 — Providing palliative and end-of-life care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, MSF, Reflective exercise | [link] [aria-label="Edit PAED-014"] Edit [button] [aria-label="Remove PAED-014"] [aria-haspopup="dialog"] Remove… |
| PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 1 per semesterDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Direct observation, Learner feedback, MSF, Portfolio and logbook review | [link] [aria-label="Edit PAED-015"] Edit [button] [aria-label="Remove PAED-015"] [aria-haspopup="dialog"] Remove… |

######## BOARD R2-F09.dc.html
R2-F09 · 1280
6.30 before · 11.2's items: 15 rows, Add item, "No institution has adopted this version yet."
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Curricula
-
- [aria-current="page"] Items of Paediatric EPA Curriculum (11.2)
### Items of Paediatric EPA Curriculum (11.2)
College of Paediatricians of South Africa · Paediatrics / Paediatrics · effective 2026-11-02
[link] [aria-label="Edit Paediatric EPA Curriculum (11.2)"] Edit this version
No institution has adopted this version yet. A target and its period are read live. Changing one changes what every trainee on this curriculum is measured against, in every institution that has adopted it, including periods that have already closed. A tool list is checked when an activity is filed and submitted, and never takes back credit already earned.
#### Items (15)
[link] Add item
Paediatric EPA Curriculum (11.2)'s items, in EPA order
| EPA | Target and decision | Minimum level | Minimum by training year | Tools | Actions |
| PAED-001 — Providing paediatric emergency care to children | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | [link] [aria-label="Edit PAED-001"] Edit [button] [aria-label="Remove PAED-001"] [aria-haspopup="dialog"] Remove… |
| PAED-002 — Managing common paediatric presentations | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, CCA, Chart-stimulated recall, Clinical audit, Direct observation, DOPS, Mini-CEX, MSF, RCA | [link] [aria-label="Edit PAED-002"] Edit [button] [aria-label="Remove PAED-002"] [aria-haspopup="dialog"] Remove… |
| PAED-003 — Providing intensive care to children | 3 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | [link] [aria-label="Edit PAED-003"] Edit [button] [aria-label="Remove PAED-003"] [aria-haspopup="dialog"] Remove… |
| PAED-004 — Managing common neonatal conditions | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, CCA, Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-004"] Edit [button] [aria-label="Remove PAED-004"] [aria-haspopup="dialog"] Remove… |
| PAED-005 — Providing neonatal care in intensive and high-care settings | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, DOPS, MSF | [link] [aria-label="Edit PAED-005"] Edit [button] [aria-label="Remove PAED-005"] [aria-haspopup="dialog"] Remove… |
| PAED-006 — Managing long-term health conditions (LTHCs) in children | 2 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CCA, Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-006"] Edit [button] [aria-label="Remove PAED-006"] [aria-haspopup="dialog"] Remove… |
| PAED-007 — Maintaining and promoting the health and well-being of children | 1 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-007"] Edit [button] [aria-label="Remove PAED-007"] [aria-haspopup="dialog"] Remove… |
| PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, DOPS, Mini-CEX, MSF, Reflective exercise | [link] [aria-label="Edit PAED-008"] Edit [button] [aria-label="Remove PAED-008"] [aria-haspopup="dialog"] Remove… |
| PAED-009 — Providing care for an adolescent patient | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Direct observation, MSF | [link] [aria-label="Edit PAED-009"] Edit [button] [aria-label="Remove PAED-009"] [aria-haspopup="dialog"] Remove… |
| PAED-010 — Leading and operating within a clinical team | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | Direct observation, MSF | [link] [aria-label="Edit PAED-010"] Edit [button] [aria-label="Remove PAED-010"] [aria-haspopup="dialog"] Remove… |
| PAED-011 — Managing population health challenges | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Direct observation, MSF | [link] [aria-label="Edit PAED-011"] Edit [button] [aria-label="Remove PAED-011"] [aria-haspopup="dialog"] Remove… |
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, Chart-stimulated recall, Mini-CEX, MSF | [link] [aria-label="Edit PAED-012"] Edit [button] [aria-label="Remove PAED-012"] [aria-haspopup="dialog"] Remove… |
| PAED-013 — Managing child protection, legal and ethical aspects of children at risk | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Direct observation, Mini-CEX, MSF | [link] [aria-label="Edit PAED-013"] Edit [button] [aria-label="Remove PAED-013"] [aria-haspopup="dialog"] Remove… |
| PAED-014 — Providing palliative and end-of-life care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, MSF, Reflective exercise | [link] [aria-label="Edit PAED-014"] Edit [button] [aria-label="Remove PAED-014"] [aria-haspopup="dialog"] Remove… |
| PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 1 per semesterDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Direct observation, Learner feedback, MSF, Portfolio and logbook review | [link] [aria-label="Edit PAED-015"] Edit [button] [aria-label="Remove PAED-015"] [aria-haspopup="dialog"] Remove… |

######## BOARD R2-F09-390.dc.html
R2-F09-390 · 390
6.30 before · 11.2's items: 15 rows, Add item, "No institution has adopted this version yet."
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Curricula
-
- [aria-current="page"] Items of Paediatric EPA Curriculum (11.2)
### Items of Paediatric EPA Curriculum (11.2)
College of Paediatricians of South Africa · Paediatrics / Paediatrics · effective 2026-11-02
[link] [aria-label="Edit Paediatric EPA Curriculum (11.2)"] Edit this version
No institution has adopted this version yet. A target and its period are read live. Changing one changes what every trainee on this curriculum is measured against, in every institution that has adopted it, including periods that have already closed. A tool list is checked when an activity is filed and submitted, and never takes back credit already earned.
#### Items (15)
[link] Add item
Paediatric EPA Curriculum (11.2)'s items, in EPA order
| EPA | Target and decision | Minimum level | Minimum by training year | Tools | Actions |
| PAED-001 — Providing paediatric emergency care to children | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | [link] [aria-label="Edit PAED-001"] Edit [button] [aria-label="Remove PAED-001"] [aria-haspopup="dialog"] Remove… |
| PAED-002 — Managing common paediatric presentations | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, CCA, Chart-stimulated recall, Clinical audit, Direct observation, DOPS, Mini-CEX, MSF, RCA | [link] [aria-label="Edit PAED-002"] Edit [button] [aria-label="Remove PAED-002"] [aria-haspopup="dialog"] Remove… |
| PAED-003 — Providing intensive care to children | 3 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | [link] [aria-label="Edit PAED-003"] Edit [button] [aria-label="Remove PAED-003"] [aria-haspopup="dialog"] Remove… |
| PAED-004 — Managing common neonatal conditions | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, CCA, Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-004"] Edit [button] [aria-label="Remove PAED-004"] [aria-haspopup="dialog"] Remove… |
| PAED-005 — Providing neonatal care in intensive and high-care settings | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, DOPS, MSF | [link] [aria-label="Edit PAED-005"] Edit [button] [aria-label="Remove PAED-005"] [aria-haspopup="dialog"] Remove… |
| PAED-006 — Managing long-term health conditions (LTHCs) in children | 2 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CCA, Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-006"] Edit [button] [aria-label="Remove PAED-006"] [aria-haspopup="dialog"] Remove… |
| PAED-007 — Maintaining and promoting the health and well-being of children | 1 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-007"] Edit [button] [aria-label="Remove PAED-007"] [aria-haspopup="dialog"] Remove… |
| PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, DOPS, Mini-CEX, MSF, Reflective exercise | [link] [aria-label="Edit PAED-008"] Edit [button] [aria-label="Remove PAED-008"] [aria-haspopup="dialog"] Remove… |
| PAED-009 — Providing care for an adolescent patient | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Direct observation, MSF | [link] [aria-label="Edit PAED-009"] Edit [button] [aria-label="Remove PAED-009"] [aria-haspopup="dialog"] Remove… |
| PAED-010 — Leading and operating within a clinical team | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | Direct observation, MSF | [link] [aria-label="Edit PAED-010"] Edit [button] [aria-label="Remove PAED-010"] [aria-haspopup="dialog"] Remove… |
| PAED-011 — Managing population health challenges | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Direct observation, MSF | [link] [aria-label="Edit PAED-011"] Edit [button] [aria-label="Remove PAED-011"] [aria-haspopup="dialog"] Remove… |
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, Chart-stimulated recall, Mini-CEX, MSF | [link] [aria-label="Edit PAED-012"] Edit [button] [aria-label="Remove PAED-012"] [aria-haspopup="dialog"] Remove… |
| PAED-013 — Managing child protection, legal and ethical aspects of children at risk | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Direct observation, Mini-CEX, MSF | [link] [aria-label="Edit PAED-013"] Edit [button] [aria-label="Remove PAED-013"] [aria-haspopup="dialog"] Remove… |
| PAED-014 — Providing palliative and end-of-life care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, MSF, Reflective exercise | [link] [aria-label="Edit PAED-014"] Edit [button] [aria-label="Remove PAED-014"] [aria-haspopup="dialog"] Remove… |
| PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 1 per semesterDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Direct observation, Learner feedback, MSF, Portfolio and logbook review | [link] [aria-label="Edit PAED-015"] Edit [button] [aria-label="Remove PAED-015"] [aria-haspopup="dialog"] Remove… |

######## BOARD R2-F10.dc.html
R2-F10 · 1280
6.30 · Anton Kruger · the add page (flow 15's form, unchanged): the subtitle gains the reach as its second sentence (Q2); PAED-016 alone and chosen; the values typed.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Curricula
-
-
[link] Items of Paediatric EPA Curriculum (11.2)
-
- [aria-current="page"] Add item
### Add item
On Paediatric EPA Curriculum (11.2). It applies to every institution that has adopted it. No institution has adopted it yet.
EPA*
[selected] PAED-016 — Transitioning adolescents with long-term conditions to adult care
Lists only the EPAs this item can name that no item on this curriculum already holds for the same trainees: the national EPAs of its sub-speciality and, for an item an institution added, that institution's own local EPAs. A national item holds its EPA for every institution; an institution's own item holds it for that institution alone.
Target
[value="1"]
Credited encounters required in each period below. This is the published frequency, not a total for the whole programme.
Per
Semester
[selected] Academic year (January to November)
A semester target applies to each semester on its own. A surplus in one semester does not carry into the next.
Decision cadence
No published cadence
Each semester
[selected] Each academic year
How often a committee takes the entrustment decision on this EPA (Annexure B). Separate from the target: an EPA observed each semester may be decided once a year. With no cadence the EPA is never due, but a committee can still decide it.
Decided by
[selected] The trainee's general panel
The committee the College names to decide this EPA instead of the trainee's general panel, such as the neonatal CCC for paediatric EPAs 4 and 5.
Opportunistic
(checkbox)
Decided as rotation or opportunity allows
For an EPA the College has decided as rotation or opportunity allows, such as paediatric EPAs 8, 9 and 13: its decision is never overdue. Needs a decision cadence.
Entrustment scale
[selected] CPSA Paediatric Entrustment Scale v11.1
Suggested because every item on this curriculum is pinned to it.
The ladder the minima are picked on. Changing it clears them, because the same number is a different rung on another ladder; changing back brings back what was picked on this one. Leave unpinned if you are not certain: a wrong scale makes assessments on another ladder stop counting toward the minimum.
Minimum level*
Choose a rung
1
2
3a
3b
[selected] 4
5
Minimum by training year
A trainee in a year listed here is measured against that year's rung instead of the minimum level. A year not listed uses the minimum level.
Year 1
1
[selected] 2
3a
3b
4
5
Year 2
1
2
[selected] 3a
3b
4
5
Year 3
1
2
3a
[selected] 3b
4
5
Year 4
1
2
3a
3b
[selected] 4
5
Completion window (months)
[value="12"]
Only used to suggest a trainee's expected completion date. It is not the target period, and credit does not check it.
Weight
Tools
The instruments that may be used as evidence for this EPA. None ticked means any instrument. Checked when an activity is filed and submitted; it never takes back credit already earned.
(checkbox) [checked]
CBD
(checkbox)
CCA
(checkbox)
Chart-stimulated recall
(checkbox)
Clinical audit
(checkbox)
Direct observation
(checkbox)
DOPS
(checkbox)
Learner feedback
(checkbox) [checked]
Mini-CEX
(checkbox) [checked]
MSF
(checkbox)
Portfolio and logbook review
(checkbox)
RCA
(checkbox)
Reflective exercise
[link] Cancel
[button] Add item

######## BOARD R2-F10-390.dc.html
R2-F10-390 · 390
6.30 · Anton Kruger · the add page (flow 15's form, unchanged): the subtitle gains the reach as its second sentence (Q2); PAED-016 alone and chosen; the values typed.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Curricula
-
-
[link] Items of Paediatric EPA Curriculum (11.2)
-
- [aria-current="page"] Add item
### Add item
On Paediatric EPA Curriculum (11.2). It applies to every institution that has adopted it. No institution has adopted it yet.
EPA*
[selected] PAED-016 — Transitioning adolescents with long-term conditions to adult care
Lists only the EPAs this item can name that no item on this curriculum already holds for the same trainees: the national EPAs of its sub-speciality and, for an item an institution added, that institution's own local EPAs. A national item holds its EPA for every institution; an institution's own item holds it for that institution alone.
Target
[value="1"]
Credited encounters required in each period below. This is the published frequency, not a total for the whole programme.
Per
Semester
[selected] Academic year (January to November)
A semester target applies to each semester on its own. A surplus in one semester does not carry into the next.
Decision cadence
No published cadence
Each semester
[selected] Each academic year
How often a committee takes the entrustment decision on this EPA (Annexure B). Separate from the target: an EPA observed each semester may be decided once a year. With no cadence the EPA is never due, but a committee can still decide it.
Decided by
[selected] The trainee's general panel
The committee the College names to decide this EPA instead of the trainee's general panel, such as the neonatal CCC for paediatric EPAs 4 and 5.
Opportunistic
(checkbox)
Decided as rotation or opportunity allows
For an EPA the College has decided as rotation or opportunity allows, such as paediatric EPAs 8, 9 and 13: its decision is never overdue. Needs a decision cadence.
Entrustment scale
[selected] CPSA Paediatric Entrustment Scale v11.1
Suggested because every item on this curriculum is pinned to it.
The ladder the minima are picked on. Changing it clears them, because the same number is a different rung on another ladder; changing back brings back what was picked on this one. Leave unpinned if you are not certain: a wrong scale makes assessments on another ladder stop counting toward the minimum.
Minimum level*
Choose a rung
1
2
3a
3b
[selected] 4
5
Minimum by training year
A trainee in a year listed here is measured against that year's rung instead of the minimum level. A year not listed uses the minimum level.
Year 1
1
[selected] 2
3a
3b
4
5
Year 2
1
2
[selected] 3a
3b
4
5
Year 3
1
2
3a
[selected] 3b
4
5
Year 4
1
2
3a
3b
[selected] 4
5
Completion window (months)
[value="12"]
Only used to suggest a trainee's expected completion date. It is not the target period, and credit does not check it.
Weight
Tools
The instruments that may be used as evidence for this EPA. None ticked means any instrument. Checked when an activity is filed and submitted; it never takes back credit already earned.
(checkbox) [checked]
CBD
(checkbox)
CCA
(checkbox)
Chart-stimulated recall
(checkbox)
Clinical audit
(checkbox)
Direct observation
(checkbox)
DOPS
(checkbox)
Learner feedback
(checkbox) [checked]
Mini-CEX
(checkbox) [checked]
MSF
(checkbox)
Portfolio and logbook review
(checkbox)
RCA
(checkbox)
Reflective exercise
[link] Cancel
[button] Add item

######## BOARD R2-F11.dc.html
R2-F11 · 1280
6.30 added · back by ?added: "Curriculum item added." focused; 16 rows, PAED-016 last; the nothing-to-add line.
Focus: the result.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Curricula
-
- [aria-current="page"] Items of Paediatric EPA Curriculum (11.2)
### Items of Paediatric EPA Curriculum (11.2)
College of Paediatricians of South Africa · Paediatrics / Paediatrics · effective 2026-11-02
[link] [aria-label="Edit Paediatric EPA Curriculum (11.2)"] Edit this version
Curriculum item added.
No institution has adopted this version yet. A target and its period are read live. Changing one changes what every trainee on this curriculum is measured against, in every institution that has adopted it, including periods that have already closed. A tool list is checked when an activity is filed and submitted, and never takes back credit already earned.
#### Items (16)
Every national EPA of Paediatrics is already on this curriculum, as a national item or as an institution's own item, so there is none left to add. To add an item, first add a national EPA of Paediatrics, or remove a national item to free its EPA. An EPA an institution has as its own item is freed only when that institution removes it.
Paediatric EPA Curriculum (11.2)'s items, in EPA order
| EPA | Target and decision | Minimum level | Minimum by training year | Tools | Actions |
| PAED-001 — Providing paediatric emergency care to children | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | [link] [aria-label="Edit PAED-001"] Edit [button] [aria-label="Remove PAED-001"] [aria-haspopup="dialog"] Remove… |
| PAED-002 — Managing common paediatric presentations | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, CCA, Chart-stimulated recall, Clinical audit, Direct observation, DOPS, Mini-CEX, MSF, RCA | [link] [aria-label="Edit PAED-002"] Edit [button] [aria-label="Remove PAED-002"] [aria-haspopup="dialog"] Remove… |
| PAED-003 — Providing intensive care to children | 3 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | [link] [aria-label="Edit PAED-003"] Edit [button] [aria-label="Remove PAED-003"] [aria-haspopup="dialog"] Remove… |
| PAED-004 — Managing common neonatal conditions | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, CCA, Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-004"] Edit [button] [aria-label="Remove PAED-004"] [aria-haspopup="dialog"] Remove… |
| PAED-005 — Providing neonatal care in intensive and high-care settings | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, DOPS, MSF | [link] [aria-label="Edit PAED-005"] Edit [button] [aria-label="Remove PAED-005"] [aria-haspopup="dialog"] Remove… |
| PAED-006 — Managing long-term health conditions (LTHCs) in children | 2 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CCA, Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-006"] Edit [button] [aria-label="Remove PAED-006"] [aria-haspopup="dialog"] Remove… |
| PAED-007 — Maintaining and promoting the health and well-being of children | 1 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-007"] Edit [button] [aria-label="Remove PAED-007"] [aria-haspopup="dialog"] Remove… |
| PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, DOPS, Mini-CEX, MSF, Reflective exercise | [link] [aria-label="Edit PAED-008"] Edit [button] [aria-label="Remove PAED-008"] [aria-haspopup="dialog"] Remove… |
| PAED-009 — Providing care for an adolescent patient | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Direct observation, MSF | [link] [aria-label="Edit PAED-009"] Edit [button] [aria-label="Remove PAED-009"] [aria-haspopup="dialog"] Remove… |
| PAED-010 — Leading and operating within a clinical team | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | Direct observation, MSF | [link] [aria-label="Edit PAED-010"] Edit [button] [aria-label="Remove PAED-010"] [aria-haspopup="dialog"] Remove… |
| PAED-011 — Managing population health challenges | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Direct observation, MSF | [link] [aria-label="Edit PAED-011"] Edit [button] [aria-label="Remove PAED-011"] [aria-haspopup="dialog"] Remove… |
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, Chart-stimulated recall, Mini-CEX, MSF | [link] [aria-label="Edit PAED-012"] Edit [button] [aria-label="Remove PAED-012"] [aria-haspopup="dialog"] Remove… |
| PAED-013 — Managing child protection, legal and ethical aspects of children at risk | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Direct observation, Mini-CEX, MSF | [link] [aria-label="Edit PAED-013"] Edit [button] [aria-label="Remove PAED-013"] [aria-haspopup="dialog"] Remove… |
| PAED-014 — Providing palliative and end-of-life care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, MSF, Reflective exercise | [link] [aria-label="Edit PAED-014"] Edit [button] [aria-label="Remove PAED-014"] [aria-haspopup="dialog"] Remove… |
| PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 1 per semesterDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Direct observation, Learner feedback, MSF, Portfolio and logbook review | [link] [aria-label="Edit PAED-015"] Edit [button] [aria-label="Remove PAED-015"] [aria-haspopup="dialog"] Remove… |
| PAED-016 — Transitioning adolescents with long-term conditions to adult care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Mini-CEX, MSF | [link] [aria-label="Edit PAED-016"] Edit [button] [aria-label="Remove PAED-016"] [aria-haspopup="dialog"] Remove… |

######## BOARD R2-F11-390.dc.html
R2-F11-390 · 390
6.30 added · back by ?added: "Curriculum item added." focused; 16 rows, PAED-016 last; the nothing-to-add line.
Focus: the result.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Curricula
-
- [aria-current="page"] Items of Paediatric EPA Curriculum (11.2)
### Items of Paediatric EPA Curriculum (11.2)
College of Paediatricians of South Africa · Paediatrics / Paediatrics · effective 2026-11-02
[link] [aria-label="Edit Paediatric EPA Curriculum (11.2)"] Edit this version
Curriculum item added.
No institution has adopted this version yet. A target and its period are read live. Changing one changes what every trainee on this curriculum is measured against, in every institution that has adopted it, including periods that have already closed. A tool list is checked when an activity is filed and submitted, and never takes back credit already earned.
#### Items (16)
Every national EPA of Paediatrics is already on this curriculum, as a national item or as an institution's own item, so there is none left to add. To add an item, first add a national EPA of Paediatrics, or remove a national item to free its EPA. An EPA an institution has as its own item is freed only when that institution removes it.
Paediatric EPA Curriculum (11.2)'s items, in EPA order
| EPA | Target and decision | Minimum level | Minimum by training year | Tools | Actions |
| PAED-001 — Providing paediatric emergency care to children | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | [link] [aria-label="Edit PAED-001"] Edit [button] [aria-label="Remove PAED-001"] [aria-haspopup="dialog"] Remove… |
| PAED-002 — Managing common paediatric presentations | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, CCA, Chart-stimulated recall, Clinical audit, Direct observation, DOPS, Mini-CEX, MSF, RCA | [link] [aria-label="Edit PAED-002"] Edit [button] [aria-label="Remove PAED-002"] [aria-haspopup="dialog"] Remove… |
| PAED-003 — Providing intensive care to children | 3 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | [link] [aria-label="Edit PAED-003"] Edit [button] [aria-label="Remove PAED-003"] [aria-haspopup="dialog"] Remove… |
| PAED-004 — Managing common neonatal conditions | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, CCA, Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-004"] Edit [button] [aria-label="Remove PAED-004"] [aria-haspopup="dialog"] Remove… |
| PAED-005 — Providing neonatal care in intensive and high-care settings | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, DOPS, MSF | [link] [aria-label="Edit PAED-005"] Edit [button] [aria-label="Remove PAED-005"] [aria-haspopup="dialog"] Remove… |
| PAED-006 — Managing long-term health conditions (LTHCs) in children | 2 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CCA, Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-006"] Edit [button] [aria-label="Remove PAED-006"] [aria-haspopup="dialog"] Remove… |
| PAED-007 — Maintaining and promoting the health and well-being of children | 1 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-007"] Edit [button] [aria-label="Remove PAED-007"] [aria-haspopup="dialog"] Remove… |
| PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, DOPS, Mini-CEX, MSF, Reflective exercise | [link] [aria-label="Edit PAED-008"] Edit [button] [aria-label="Remove PAED-008"] [aria-haspopup="dialog"] Remove… |
| PAED-009 — Providing care for an adolescent patient | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Direct observation, MSF | [link] [aria-label="Edit PAED-009"] Edit [button] [aria-label="Remove PAED-009"] [aria-haspopup="dialog"] Remove… |
| PAED-010 — Leading and operating within a clinical team | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | Direct observation, MSF | [link] [aria-label="Edit PAED-010"] Edit [button] [aria-label="Remove PAED-010"] [aria-haspopup="dialog"] Remove… |
| PAED-011 — Managing population health challenges | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Direct observation, MSF | [link] [aria-label="Edit PAED-011"] Edit [button] [aria-label="Remove PAED-011"] [aria-haspopup="dialog"] Remove… |
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, Chart-stimulated recall, Mini-CEX, MSF | [link] [aria-label="Edit PAED-012"] Edit [button] [aria-label="Remove PAED-012"] [aria-haspopup="dialog"] Remove… |
| PAED-013 — Managing child protection, legal and ethical aspects of children at risk | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Direct observation, Mini-CEX, MSF | [link] [aria-label="Edit PAED-013"] Edit [button] [aria-label="Remove PAED-013"] [aria-haspopup="dialog"] Remove… |
| PAED-014 — Providing palliative and end-of-life care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, MSF, Reflective exercise | [link] [aria-label="Edit PAED-014"] Edit [button] [aria-label="Remove PAED-014"] [aria-haspopup="dialog"] Remove… |
| PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 1 per semesterDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Direct observation, Learner feedback, MSF, Portfolio and logbook review | [link] [aria-label="Edit PAED-015"] Edit [button] [aria-label="Remove PAED-015"] [aria-haspopup="dialog"] Remove… |
| PAED-016 — Transitioning adolescents with long-term conditions to adult care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Mini-CEX, MSF | [link] [aria-label="Edit PAED-016"] Edit [button] [aria-label="Remove PAED-016"] [aria-haspopup="dialog"] Remove… |

######## BOARD R2-F12.dc.html
R2-F12 · 1280
6.30 · Edit PAED-011's page: its title, the subtitle with the reach; the EPA select offers PAED-011 alone; Target 2.
Focus: Target, as typed.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Curricula
-
-
[link] Items of Paediatric EPA Curriculum (11.2)
-
- [aria-current="page"] PAED-011
### Edit PAED-011 — Managing population health challenges
On Paediatric EPA Curriculum (11.2). It applies to every institution that has adopted it. No institution has adopted it yet.
EPA*
[selected] PAED-011 — Managing population health challenges
Lists only the EPAs this item can name that no item on this curriculum already holds for the same trainees: the national EPAs of its sub-speciality and, for an item an institution added, that institution's own local EPAs. A national item holds its EPA for every institution; an institution's own item holds it for that institution alone.
Target
[value="2"]
Credited encounters required in each period below. This is the published frequency, not a total for the whole programme.
Per
Semester
[selected] Academic year (January to November)
A semester target applies to each semester on its own. A surplus in one semester does not carry into the next.
Decision cadence
No published cadence
Each semester
[selected] Each academic year
How often a committee takes the entrustment decision on this EPA (Annexure B). Separate from the target: an EPA observed each semester may be decided once a year. With no cadence the EPA is never due, but a committee can still decide it.
Decided by
[selected] The trainee's general panel
The committee the College names to decide this EPA instead of the trainee's general panel, such as the neonatal CCC for paediatric EPAs 4 and 5.
Opportunistic
(checkbox)
Decided as rotation or opportunity allows
For an EPA the College has decided as rotation or opportunity allows, such as paediatric EPAs 8, 9 and 13: its decision is never overdue. Needs a decision cadence.
Entrustment scale
[selected] CPSA Paediatric Entrustment Scale v11.1
Suggested because every item on this curriculum is pinned to it.
The ladder the minima are picked on. Changing it clears them, because the same number is a different rung on another ladder; changing back brings back what was picked on this one. Leave unpinned if you are not certain: a wrong scale makes assessments on another ladder stop counting toward the minimum.
Minimum level*
Choose a rung
1
2
3a
3b
[selected] 4
5
Minimum by training year
A trainee in a year listed here is measured against that year's rung instead of the minimum level. A year not listed uses the minimum level.
Year 1
1
[selected] 2
3a
3b
4
5
Year 2
1
2
[selected] 3a
3b
4
5
Year 3
1
2
3a
[selected] 3b
4
5
Year 4
1
2
3a
3b
[selected] 4
5
Completion window (months)
[value="12"]
Only used to suggest a trainee's expected completion date. It is not the target period, and credit does not check it.
Weight
Tools
The instruments that may be used as evidence for this EPA. None ticked means any instrument. Checked when an activity is filed and submitted; it never takes back credit already earned.
(checkbox) [checked]
CBD
(checkbox)
CCA
(checkbox)
Chart-stimulated recall
(checkbox)
Clinical audit
(checkbox) [checked]
Direct observation
(checkbox)
DOPS
(checkbox)
Learner feedback
(checkbox)
Mini-CEX
(checkbox) [checked]
MSF
(checkbox)
Portfolio and logbook review
(checkbox)
RCA
(checkbox)
Reflective exercise
[link] Cancel
[button] Save item

######## BOARD R2-F12-390.dc.html
R2-F12-390 · 390
6.30 · Edit PAED-011's page: its title, the subtitle with the reach; the EPA select offers PAED-011 alone; Target 2.
Focus: Target, as typed.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Curricula
-
-
[link] Items of Paediatric EPA Curriculum (11.2)
-
- [aria-current="page"] PAED-011
### Edit PAED-011 — Managing population health challenges
On Paediatric EPA Curriculum (11.2). It applies to every institution that has adopted it. No institution has adopted it yet.
EPA*
[selected] PAED-011 — Managing population health challenges
Lists only the EPAs this item can name that no item on this curriculum already holds for the same trainees: the national EPAs of its sub-speciality and, for an item an institution added, that institution's own local EPAs. A national item holds its EPA for every institution; an institution's own item holds it for that institution alone.
Target
[value="2"]
Credited encounters required in each period below. This is the published frequency, not a total for the whole programme.
Per
Semester
[selected] Academic year (January to November)
A semester target applies to each semester on its own. A surplus in one semester does not carry into the next.
Decision cadence
No published cadence
Each semester
[selected] Each academic year
How often a committee takes the entrustment decision on this EPA (Annexure B). Separate from the target: an EPA observed each semester may be decided once a year. With no cadence the EPA is never due, but a committee can still decide it.
Decided by
[selected] The trainee's general panel
The committee the College names to decide this EPA instead of the trainee's general panel, such as the neonatal CCC for paediatric EPAs 4 and 5.
Opportunistic
(checkbox)
Decided as rotation or opportunity allows
For an EPA the College has decided as rotation or opportunity allows, such as paediatric EPAs 8, 9 and 13: its decision is never overdue. Needs a decision cadence.
Entrustment scale
[selected] CPSA Paediatric Entrustment Scale v11.1
Suggested because every item on this curriculum is pinned to it.
The ladder the minima are picked on. Changing it clears them, because the same number is a different rung on another ladder; changing back brings back what was picked on this one. Leave unpinned if you are not certain: a wrong scale makes assessments on another ladder stop counting toward the minimum.
Minimum level*
Choose a rung
1
2
3a
3b
[selected] 4
5
Minimum by training year
A trainee in a year listed here is measured against that year's rung instead of the minimum level. A year not listed uses the minimum level.
Year 1
1
[selected] 2
3a
3b
4
5
Year 2
1
2
[selected] 3a
3b
4
5
Year 3
1
2
3a
[selected] 3b
4
5
Year 4
1
2
3a
3b
[selected] 4
5
Completion window (months)
[value="12"]
Only used to suggest a trainee's expected completion date. It is not the target period, and credit does not check it.
Weight
Tools
The instruments that may be used as evidence for this EPA. None ticked means any instrument. Checked when an activity is filed and submitted; it never takes back credit already earned.
(checkbox) [checked]
CBD
(checkbox)
CCA
(checkbox)
Chart-stimulated recall
(checkbox)
Clinical audit
(checkbox) [checked]
Direct observation
(checkbox)
DOPS
(checkbox)
Learner feedback
(checkbox)
Mini-CEX
(checkbox) [checked]
MSF
(checkbox)
Portfolio and logbook review
(checkbox)
RCA
(checkbox)
Reflective exercise
[link] Cancel
[button] Save item

######## BOARD R2-F13.dc.html
R2-F13 · 1280
6.30 saved · "Curriculum item saved." focused; PAED-011 reads "2 per academic year".
Focus: the result.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Curricula
-
- [aria-current="page"] Items of Paediatric EPA Curriculum (11.2)
### Items of Paediatric EPA Curriculum (11.2)
College of Paediatricians of South Africa · Paediatrics / Paediatrics · effective 2026-11-02
[link] [aria-label="Edit Paediatric EPA Curriculum (11.2)"] Edit this version
Curriculum item saved.
No institution has adopted this version yet. A target and its period are read live. Changing one changes what every trainee on this curriculum is measured against, in every institution that has adopted it, including periods that have already closed. A tool list is checked when an activity is filed and submitted, and never takes back credit already earned.
#### Items (16)
Every national EPA of Paediatrics is already on this curriculum, as a national item or as an institution's own item, so there is none left to add. To add an item, first add a national EPA of Paediatrics, or remove a national item to free its EPA. An EPA an institution has as its own item is freed only when that institution removes it.
Paediatric EPA Curriculum (11.2)'s items, in EPA order
| EPA | Target and decision | Minimum level | Minimum by training year | Tools | Actions |
| PAED-001 — Providing paediatric emergency care to children | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | [link] [aria-label="Edit PAED-001"] Edit [button] [aria-label="Remove PAED-001"] [aria-haspopup="dialog"] Remove… |
| PAED-002 — Managing common paediatric presentations | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, CCA, Chart-stimulated recall, Clinical audit, Direct observation, DOPS, Mini-CEX, MSF, RCA | [link] [aria-label="Edit PAED-002"] Edit [button] [aria-label="Remove PAED-002"] [aria-haspopup="dialog"] Remove… |
| PAED-003 — Providing intensive care to children | 3 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | [link] [aria-label="Edit PAED-003"] Edit [button] [aria-label="Remove PAED-003"] [aria-haspopup="dialog"] Remove… |
| PAED-004 — Managing common neonatal conditions | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, CCA, Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-004"] Edit [button] [aria-label="Remove PAED-004"] [aria-haspopup="dialog"] Remove… |
| PAED-005 — Providing neonatal care in intensive and high-care settings | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, DOPS, MSF | [link] [aria-label="Edit PAED-005"] Edit [button] [aria-label="Remove PAED-005"] [aria-haspopup="dialog"] Remove… |
| PAED-006 — Managing long-term health conditions (LTHCs) in children | 2 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CCA, Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-006"] Edit [button] [aria-label="Remove PAED-006"] [aria-haspopup="dialog"] Remove… |
| PAED-007 — Maintaining and promoting the health and well-being of children | 1 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-007"] Edit [button] [aria-label="Remove PAED-007"] [aria-haspopup="dialog"] Remove… |
| PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, DOPS, Mini-CEX, MSF, Reflective exercise | [link] [aria-label="Edit PAED-008"] Edit [button] [aria-label="Remove PAED-008"] [aria-haspopup="dialog"] Remove… |
| PAED-009 — Providing care for an adolescent patient | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Direct observation, MSF | [link] [aria-label="Edit PAED-009"] Edit [button] [aria-label="Remove PAED-009"] [aria-haspopup="dialog"] Remove… |
| PAED-010 — Leading and operating within a clinical team | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | Direct observation, MSF | [link] [aria-label="Edit PAED-010"] Edit [button] [aria-label="Remove PAED-010"] [aria-haspopup="dialog"] Remove… |
| PAED-011 — Managing population health challenges | 2 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Direct observation, MSF | [link] [aria-label="Edit PAED-011"] Edit [button] [aria-label="Remove PAED-011"] [aria-haspopup="dialog"] Remove… |
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, Chart-stimulated recall, Mini-CEX, MSF | [link] [aria-label="Edit PAED-012"] Edit [button] [aria-label="Remove PAED-012"] [aria-haspopup="dialog"] Remove… |
| PAED-013 — Managing child protection, legal and ethical aspects of children at risk | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Direct observation, Mini-CEX, MSF | [link] [aria-label="Edit PAED-013"] Edit [button] [aria-label="Remove PAED-013"] [aria-haspopup="dialog"] Remove… |
| PAED-014 — Providing palliative and end-of-life care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, MSF, Reflective exercise | [link] [aria-label="Edit PAED-014"] Edit [button] [aria-label="Remove PAED-014"] [aria-haspopup="dialog"] Remove… |
| PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 1 per semesterDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Direct observation, Learner feedback, MSF, Portfolio and logbook review | [link] [aria-label="Edit PAED-015"] Edit [button] [aria-label="Remove PAED-015"] [aria-haspopup="dialog"] Remove… |
| PAED-016 — Transitioning adolescents with long-term conditions to adult care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Mini-CEX, MSF | [link] [aria-label="Edit PAED-016"] Edit [button] [aria-label="Remove PAED-016"] [aria-haspopup="dialog"] Remove… |

######## BOARD R2-F13-390.dc.html
R2-F13-390 · 390
6.30 saved · "Curriculum item saved." focused; PAED-011 reads "2 per academic year".
Focus: the result.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Curricula
-
- [aria-current="page"] Items of Paediatric EPA Curriculum (11.2)
### Items of Paediatric EPA Curriculum (11.2)
College of Paediatricians of South Africa · Paediatrics / Paediatrics · effective 2026-11-02
[link] [aria-label="Edit Paediatric EPA Curriculum (11.2)"] Edit this version
Curriculum item saved.
No institution has adopted this version yet. A target and its period are read live. Changing one changes what every trainee on this curriculum is measured against, in every institution that has adopted it, including periods that have already closed. A tool list is checked when an activity is filed and submitted, and never takes back credit already earned.
#### Items (16)
Every national EPA of Paediatrics is already on this curriculum, as a national item or as an institution's own item, so there is none left to add. To add an item, first add a national EPA of Paediatrics, or remove a national item to free its EPA. An EPA an institution has as its own item is freed only when that institution removes it.
Paediatric EPA Curriculum (11.2)'s items, in EPA order
| EPA | Target and decision | Minimum level | Minimum by training year | Tools | Actions |
| PAED-001 — Providing paediatric emergency care to children | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | [link] [aria-label="Edit PAED-001"] Edit [button] [aria-label="Remove PAED-001"] [aria-haspopup="dialog"] Remove… |
| PAED-002 — Managing common paediatric presentations | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, CCA, Chart-stimulated recall, Clinical audit, Direct observation, DOPS, Mini-CEX, MSF, RCA | [link] [aria-label="Edit PAED-002"] Edit [button] [aria-label="Remove PAED-002"] [aria-haspopup="dialog"] Remove… |
| PAED-003 — Providing intensive care to children | 3 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | [link] [aria-label="Edit PAED-003"] Edit [button] [aria-label="Remove PAED-003"] [aria-haspopup="dialog"] Remove… |
| PAED-004 — Managing common neonatal conditions | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, CCA, Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-004"] Edit [button] [aria-label="Remove PAED-004"] [aria-haspopup="dialog"] Remove… |
| PAED-005 — Providing neonatal care in intensive and high-care settings | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, DOPS, MSF | [link] [aria-label="Edit PAED-005"] Edit [button] [aria-label="Remove PAED-005"] [aria-haspopup="dialog"] Remove… |
| PAED-006 — Managing long-term health conditions (LTHCs) in children | 2 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CCA, Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-006"] Edit [button] [aria-label="Remove PAED-006"] [aria-haspopup="dialog"] Remove… |
| PAED-007 — Maintaining and promoting the health and well-being of children | 1 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-007"] Edit [button] [aria-label="Remove PAED-007"] [aria-haspopup="dialog"] Remove… |
| PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, DOPS, Mini-CEX, MSF, Reflective exercise | [link] [aria-label="Edit PAED-008"] Edit [button] [aria-label="Remove PAED-008"] [aria-haspopup="dialog"] Remove… |
| PAED-009 — Providing care for an adolescent patient | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Direct observation, MSF | [link] [aria-label="Edit PAED-009"] Edit [button] [aria-label="Remove PAED-009"] [aria-haspopup="dialog"] Remove… |
| PAED-010 — Leading and operating within a clinical team | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | Direct observation, MSF | [link] [aria-label="Edit PAED-010"] Edit [button] [aria-label="Remove PAED-010"] [aria-haspopup="dialog"] Remove… |
| PAED-011 — Managing population health challenges | 2 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Direct observation, MSF | [link] [aria-label="Edit PAED-011"] Edit [button] [aria-label="Remove PAED-011"] [aria-haspopup="dialog"] Remove… |
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, Chart-stimulated recall, Mini-CEX, MSF | [link] [aria-label="Edit PAED-012"] Edit [button] [aria-label="Remove PAED-012"] [aria-haspopup="dialog"] Remove… |
| PAED-013 — Managing child protection, legal and ethical aspects of children at risk | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Direct observation, Mini-CEX, MSF | [link] [aria-label="Edit PAED-013"] Edit [button] [aria-label="Remove PAED-013"] [aria-haspopup="dialog"] Remove… |
| PAED-014 — Providing palliative and end-of-life care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, MSF, Reflective exercise | [link] [aria-label="Edit PAED-014"] Edit [button] [aria-label="Remove PAED-014"] [aria-haspopup="dialog"] Remove… |
| PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 1 per semesterDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Direct observation, Learner feedback, MSF, Portfolio and logbook review | [link] [aria-label="Edit PAED-015"] Edit [button] [aria-label="Remove PAED-015"] [aria-haspopup="dialog"] Remove… |
| PAED-016 — Transitioning adolescents with long-term conditions to adult care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Mini-CEX, MSF | [link] [aria-label="Edit PAED-016"] Edit [button] [aria-label="Remove PAED-016"] [aria-haspopup="dialog"] Remove… |

######## BOARD R2-F14.dc.html
IllustrationR2-F14 · 1280
Q10 (T242) · a page read before Kgosi Kgari Teaching Hospital added PAED-016 as its own item on 11.1: the refusal under EPA names the holder (DATA), focused; the picker read again.
Focus: EPA.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Curricula
-
-
[link] Items of Paediatric EPA Curriculum (11.1)
-
- [aria-current="page"] Add item
### Add item
On Paediatric EPA Curriculum (11.1). It applies to every institution that has adopted it. Adopted by 1 institution, Kgosi Kgari Teaching Hospital; 3 registrars in training on it.
EPA*
[selected] PAED-016 — Transitioning adolescents with long-term conditions to adult care
Kgosi Kgari Teaching Hospital has PAED-016 as its own item on this curriculum, so it cannot be added nationally until that institution removes it.
Lists only the EPAs this item can name that no item on this curriculum already holds for the same trainees: the national EPAs of its sub-speciality and, for an item an institution added, that institution's own local EPAs. A national item holds its EPA for every institution; an institution's own item holds it for that institution alone.
Target
[value="1"]
Credited encounters required in each period below. This is the published frequency, not a total for the whole programme.
Per
Semester
[selected] Academic year (January to November)
A semester target applies to each semester on its own. A surplus in one semester does not carry into the next.
Decision cadence
No published cadence
Each semester
[selected] Each academic year
How often a committee takes the entrustment decision on this EPA (Annexure B). Separate from the target: an EPA observed each semester may be decided once a year. With no cadence the EPA is never due, but a committee can still decide it.
Decided by
[selected] The trainee's general panel
The committee the College names to decide this EPA instead of the trainee's general panel, such as the neonatal CCC for paediatric EPAs 4 and 5.
Opportunistic
(checkbox)
Decided as rotation or opportunity allows
For an EPA the College has decided as rotation or opportunity allows, such as paediatric EPAs 8, 9 and 13: its decision is never overdue. Needs a decision cadence.
Entrustment scale
[selected] CPSA Paediatric Entrustment Scale v11.1
Suggested because every item on this curriculum is pinned to it.
The ladder the minima are picked on. Changing it clears them, because the same number is a different rung on another ladder; changing back brings back what was picked on this one. Leave unpinned if you are not certain: a wrong scale makes assessments on another ladder stop counting toward the minimum.
Minimum level*
Choose a rung
1
2
3a
3b
[selected] 4
5
Minimum by training year
A trainee in a year listed here is measured against that year's rung instead of the minimum level. A year not listed uses the minimum level.
Year 1
1
[selected] 2
3a
3b
4
5
Year 2
1
2
[selected] 3a
3b
4
5
Year 3
1
2
3a
[selected] 3b
4
5
Year 4
1
2
3a
3b
[selected] 4
5
Completion window (months)
[value="12"]
Only used to suggest a trainee's expected completion date. It is not the target period, and credit does not check it.
Weight
Tools
The instruments that may be used as evidence for this EPA. None ticked means any instrument. Checked when an activity is filed and submitted; it never takes back credit already earned.
(checkbox) [checked]
CBD
(checkbox)
CCA
(checkbox)
Chart-stimulated recall
(checkbox)
Clinical audit
(checkbox)
Direct observation
(checkbox)
DOPS
(checkbox)
Learner feedback
(checkbox) [checked]
Mini-CEX
(checkbox) [checked]
MSF
(checkbox)
Portfolio and logbook review
(checkbox)
RCA
(checkbox)
Reflective exercise
[link] Cancel
[button] Add item

######## BOARD R2-F14-390.dc.html
IllustrationR2-F14-390 · 390
Q10 (T242) · a page read before Kgosi Kgari Teaching Hospital added PAED-016 as its own item on 11.1: the refusal under EPA names the holder (DATA), focused; the picker read again.
Focus: EPA.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Curricula
-
-
[link] Items of Paediatric EPA Curriculum (11.1)
-
- [aria-current="page"] Add item
### Add item
On Paediatric EPA Curriculum (11.1). It applies to every institution that has adopted it. Adopted by 1 institution, Kgosi Kgari Teaching Hospital; 3 registrars in training on it.
EPA*
[selected] PAED-016 — Transitioning adolescents with long-term conditions to adult care
Kgosi Kgari Teaching Hospital has PAED-016 as its own item on this curriculum, so it cannot be added nationally until that institution removes it.
Lists only the EPAs this item can name that no item on this curriculum already holds for the same trainees: the national EPAs of its sub-speciality and, for an item an institution added, that institution's own local EPAs. A national item holds its EPA for every institution; an institution's own item holds it for that institution alone.
Target
[value="1"]
Credited encounters required in each period below. This is the published frequency, not a total for the whole programme.
Per
Semester
[selected] Academic year (January to November)
A semester target applies to each semester on its own. A surplus in one semester does not carry into the next.
Decision cadence
No published cadence
Each semester
[selected] Each academic year
How often a committee takes the entrustment decision on this EPA (Annexure B). Separate from the target: an EPA observed each semester may be decided once a year. With no cadence the EPA is never due, but a committee can still decide it.
Decided by
[selected] The trainee's general panel
The committee the College names to decide this EPA instead of the trainee's general panel, such as the neonatal CCC for paediatric EPAs 4 and 5.
Opportunistic
(checkbox)
Decided as rotation or opportunity allows
For an EPA the College has decided as rotation or opportunity allows, such as paediatric EPAs 8, 9 and 13: its decision is never overdue. Needs a decision cadence.
Entrustment scale
[selected] CPSA Paediatric Entrustment Scale v11.1
Suggested because every item on this curriculum is pinned to it.
The ladder the minima are picked on. Changing it clears them, because the same number is a different rung on another ladder; changing back brings back what was picked on this one. Leave unpinned if you are not certain: a wrong scale makes assessments on another ladder stop counting toward the minimum.
Minimum level*
Choose a rung
1
2
3a
3b
[selected] 4
5
Minimum by training year
A trainee in a year listed here is measured against that year's rung instead of the minimum level. A year not listed uses the minimum level.
Year 1
1
[selected] 2
3a
3b
4
5
Year 2
1
2
[selected] 3a
3b
4
5
Year 3
1
2
3a
[selected] 3b
4
5
Year 4
1
2
3a
3b
[selected] 4
5
Completion window (months)
[value="12"]
Only used to suggest a trainee's expected completion date. It is not the target period, and credit does not check it.
Weight
Tools
The instruments that may be used as evidence for this EPA. None ticked means any instrument. Checked when an activity is filed and submitted; it never takes back credit already earned.
(checkbox) [checked]
CBD
(checkbox)
CCA
(checkbox)
Chart-stimulated recall
(checkbox)
Clinical audit
(checkbox)
Direct observation
(checkbox)
DOPS
(checkbox)
Learner feedback
(checkbox) [checked]
Mini-CEX
(checkbox) [checked]
MSF
(checkbox)
Portfolio and logbook review
(checkbox)
RCA
(checkbox)
Reflective exercise
[link] Cancel
[button] Add item

######## BOARD R2-F15.dc.html
IllustrationR2-F15 · 1280
Remove… on 11.2's PAED-016 · the built dialog, opening on Cancel.
Focus: Cancel.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Curricula
-
- [aria-current="page"] Items of Paediatric EPA Curriculum (11.2)
### Items of Paediatric EPA Curriculum (11.2)
College of Paediatricians of South Africa · Paediatrics / Paediatrics · effective 2026-11-02
[link] [aria-label="Edit Paediatric EPA Curriculum (11.2)"] Edit this version
No institution has adopted this version yet. A target and its period are read live. Changing one changes what every trainee on this curriculum is measured against, in every institution that has adopted it, including periods that have already closed. A tool list is checked when an activity is filed and submitted, and never takes back credit already earned.
#### Items (16)
Every national EPA of Paediatrics is already on this curriculum, as a national item or as an institution's own item, so there is none left to add. To add an item, first add a national EPA of Paediatrics, or remove a national item to free its EPA. An EPA an institution has as its own item is freed only when that institution removes it.
Paediatric EPA Curriculum (11.2)'s items, in EPA order
| EPA | Target and decision | Minimum level | Minimum by training year | Tools | Actions |
| PAED-001 — Providing paediatric emergency care to children | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | [link] [aria-label="Edit PAED-001"] Edit [button] [aria-label="Remove PAED-001"] [aria-haspopup="dialog"] Remove… |
| PAED-002 — Managing common paediatric presentations | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, CCA, Chart-stimulated recall, Clinical audit, Direct observation, DOPS, Mini-CEX, MSF, RCA | [link] [aria-label="Edit PAED-002"] Edit [button] [aria-label="Remove PAED-002"] [aria-haspopup="dialog"] Remove… |
| PAED-003 — Providing intensive care to children | 3 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | [link] [aria-label="Edit PAED-003"] Edit [button] [aria-label="Remove PAED-003"] [aria-haspopup="dialog"] Remove… |
| PAED-004 — Managing common neonatal conditions | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, CCA, Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-004"] Edit [button] [aria-label="Remove PAED-004"] [aria-haspopup="dialog"] Remove… |
| PAED-005 — Providing neonatal care in intensive and high-care settings | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, DOPS, MSF | [link] [aria-label="Edit PAED-005"] Edit [button] [aria-label="Remove PAED-005"] [aria-haspopup="dialog"] Remove… |
| PAED-006 — Managing long-term health conditions (LTHCs) in children | 2 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CCA, Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-006"] Edit [button] [aria-label="Remove PAED-006"] [aria-haspopup="dialog"] Remove… |
| PAED-007 — Maintaining and promoting the health and well-being of children | 1 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-007"] Edit [button] [aria-label="Remove PAED-007"] [aria-haspopup="dialog"] Remove… |
| PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, DOPS, Mini-CEX, MSF, Reflective exercise | [link] [aria-label="Edit PAED-008"] Edit [button] [aria-label="Remove PAED-008"] [aria-haspopup="dialog"] Remove… |
| PAED-009 — Providing care for an adolescent patient | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Direct observation, MSF | [link] [aria-label="Edit PAED-009"] Edit [button] [aria-label="Remove PAED-009"] [aria-haspopup="dialog"] Remove… |
| PAED-010 — Leading and operating within a clinical team | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | Direct observation, MSF | [link] [aria-label="Edit PAED-010"] Edit [button] [aria-label="Remove PAED-010"] [aria-haspopup="dialog"] Remove… |
| PAED-011 — Managing population health challenges | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Direct observation, MSF | [link] [aria-label="Edit PAED-011"] Edit [button] [aria-label="Remove PAED-011"] [aria-haspopup="dialog"] Remove… |
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, Chart-stimulated recall, Mini-CEX, MSF | [link] [aria-label="Edit PAED-012"] Edit [button] [aria-label="Remove PAED-012"] [aria-haspopup="dialog"] Remove… |
| PAED-013 — Managing child protection, legal and ethical aspects of children at risk | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Direct observation, Mini-CEX, MSF | [link] [aria-label="Edit PAED-013"] Edit [button] [aria-label="Remove PAED-013"] [aria-haspopup="dialog"] Remove… |
| PAED-014 — Providing palliative and end-of-life care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, MSF, Reflective exercise | [link] [aria-label="Edit PAED-014"] Edit [button] [aria-label="Remove PAED-014"] [aria-haspopup="dialog"] Remove… |
| PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 1 per semesterDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Direct observation, Learner feedback, MSF, Portfolio and logbook review | [link] [aria-label="Edit PAED-015"] Edit [button] [aria-label="Remove PAED-015"] [aria-haspopup="dialog"] Remove… |
| PAED-016 — Transitioning adolescents with long-term conditions to adult care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Mini-CEX, MSF | [link] [aria-label="Edit PAED-016"] Edit [button] [aria-label="Remove PAED-016"] [aria-haspopup="dialog"] Remove… |
#### Remove PAED-016 from Paediatric EPA Curriculum (11.2)?
Remove PAED-016 — Transitioning adolescents with long-term conditions to adult care from Paediatric EPA Curriculum (11.2)? Its target, minima, decision and tool list are deleted, and so is each trainee's tally toward it. Trainees on this curriculum, in every institution that has adopted it, are no longer measured against this EPA. This cannot be undone.
[button] Cancel
[button] Remove item

######## BOARD R2-F15-390.dc.html
IllustrationR2-F15-390 · 390
Remove… on 11.2's PAED-016 · the built dialog, opening on Cancel.
Focus: Cancel.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Curricula
-
- [aria-current="page"] Items of Paediatric EPA Curriculum (11.2)
### Items of Paediatric EPA Curriculum (11.2)
College of Paediatricians of South Africa · Paediatrics / Paediatrics · effective 2026-11-02
[link] [aria-label="Edit Paediatric EPA Curriculum (11.2)"] Edit this version
No institution has adopted this version yet. A target and its period are read live. Changing one changes what every trainee on this curriculum is measured against, in every institution that has adopted it, including periods that have already closed. A tool list is checked when an activity is filed and submitted, and never takes back credit already earned.
#### Items (16)
Every national EPA of Paediatrics is already on this curriculum, as a national item or as an institution's own item, so there is none left to add. To add an item, first add a national EPA of Paediatrics, or remove a national item to free its EPA. An EPA an institution has as its own item is freed only when that institution removes it.
Paediatric EPA Curriculum (11.2)'s items, in EPA order
| EPA | Target and decision | Minimum level | Minimum by training year | Tools | Actions |
| PAED-001 — Providing paediatric emergency care to children | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | [link] [aria-label="Edit PAED-001"] Edit [button] [aria-label="Remove PAED-001"] [aria-haspopup="dialog"] Remove… |
| PAED-002 — Managing common paediatric presentations | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, CCA, Chart-stimulated recall, Clinical audit, Direct observation, DOPS, Mini-CEX, MSF, RCA | [link] [aria-label="Edit PAED-002"] Edit [button] [aria-label="Remove PAED-002"] [aria-haspopup="dialog"] Remove… |
| PAED-003 — Providing intensive care to children | 3 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | [link] [aria-label="Edit PAED-003"] Edit [button] [aria-label="Remove PAED-003"] [aria-haspopup="dialog"] Remove… |
| PAED-004 — Managing common neonatal conditions | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, CCA, Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-004"] Edit [button] [aria-label="Remove PAED-004"] [aria-haspopup="dialog"] Remove… |
| PAED-005 — Providing neonatal care in intensive and high-care settings | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, DOPS, MSF | [link] [aria-label="Edit PAED-005"] Edit [button] [aria-label="Remove PAED-005"] [aria-haspopup="dialog"] Remove… |
| PAED-006 — Managing long-term health conditions (LTHCs) in children | 2 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CCA, Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-006"] Edit [button] [aria-label="Remove PAED-006"] [aria-haspopup="dialog"] Remove… |
| PAED-007 — Maintaining and promoting the health and well-being of children | 1 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-007"] Edit [button] [aria-label="Remove PAED-007"] [aria-haspopup="dialog"] Remove… |
| PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, DOPS, Mini-CEX, MSF, Reflective exercise | [link] [aria-label="Edit PAED-008"] Edit [button] [aria-label="Remove PAED-008"] [aria-haspopup="dialog"] Remove… |
| PAED-009 — Providing care for an adolescent patient | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Direct observation, MSF | [link] [aria-label="Edit PAED-009"] Edit [button] [aria-label="Remove PAED-009"] [aria-haspopup="dialog"] Remove… |
| PAED-010 — Leading and operating within a clinical team | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | Direct observation, MSF | [link] [aria-label="Edit PAED-010"] Edit [button] [aria-label="Remove PAED-010"] [aria-haspopup="dialog"] Remove… |
| PAED-011 — Managing population health challenges | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Direct observation, MSF | [link] [aria-label="Edit PAED-011"] Edit [button] [aria-label="Remove PAED-011"] [aria-haspopup="dialog"] Remove… |
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, Chart-stimulated recall, Mini-CEX, MSF | [link] [aria-label="Edit PAED-012"] Edit [button] [aria-label="Remove PAED-012"] [aria-haspopup="dialog"] Remove… |
| PAED-013 — Managing child protection, legal and ethical aspects of children at risk | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Direct observation, Mini-CEX, MSF | [link] [aria-label="Edit PAED-013"] Edit [button] [aria-label="Remove PAED-013"] [aria-haspopup="dialog"] Remove… |
| PAED-014 — Providing palliative and end-of-life care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, MSF, Reflective exercise | [link] [aria-label="Edit PAED-014"] Edit [button] [aria-label="Remove PAED-014"] [aria-haspopup="dialog"] Remove… |
| PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 1 per semesterDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Direct observation, Learner feedback, MSF, Portfolio and logbook review | [link] [aria-label="Edit PAED-015"] Edit [button] [aria-label="Remove PAED-015"] [aria-haspopup="dialog"] Remove… |
| PAED-016 — Transitioning adolescents with long-term conditions to adult care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Mini-CEX, MSF | [link] [aria-label="Edit PAED-016"] Edit [button] [aria-label="Remove PAED-016"] [aria-haspopup="dialog"] Remove… |
#### Remove PAED-016 from Paediatric EPA Curriculum (11.2)?
Remove PAED-016 — Transitioning adolescents with long-term conditions to adult care from Paediatric EPA Curriculum (11.2)? Its target, minima, decision and tool list are deleted, and so is each trainee's tally toward it. Trainees on this curriculum, in every institution that has adopted it, are no longer measured against this EPA. This cannot be undone.
[button] Cancel
[button] Remove item

######## BOARD R2-F16.dc.html
R2-F16 · 1280
Q7 · Anton Kruger types KGK-001's item on 11.1: "Item unavailable", as built.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Curricula
-
- [aria-current="page"] Items of Paediatric EPA Curriculum (11.1)
### Item unavailable
This item does not exist, or you cannot open it.
[link] Go to the items

######## BOARD R2-F16-390.dc.html
R2-F16-390 · 390
Q7 · Anton Kruger types KGK-001's item on 11.1: "Item unavailable", as built.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Curricula
-
- [aria-current="page"] Items of Paediatric EPA Curriculum (11.1)
### Item unavailable
This item does not exist, or you cannot open it.
[link] Go to the items

######## BOARD R2-F17.dc.html
R2-F17 · 1280
Q7 · /admin/curricula/1/items: "Curriculum unavailable".
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
- [aria-current="page"] Curricula
### Curriculum unavailable
This curriculum does not exist, or you cannot open it.
[link] Go to Curricula

######## BOARD R2-F17-390.dc.html
R2-F17-390 · 390
Q7 · /admin/curricula/1/items: "Curriculum unavailable".
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
- [aria-current="page"] Curricula
### Curriculum unavailable
This curriculum does not exist, or you cannot open it.
[link] Go to Curricula

######## BOARD R2-F18.dc.html
IllustrationR2-F18 · 1280
Demo Administrator on 11.1's items after 6.26 · KGK-001's row last, "Kgosi Kgari Teaching Hospital's own item"; Edit and Remove… on every row; the reach.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Curricula
-
- [aria-current="page"] Items of Paediatric EPA Curriculum (11.1)
### Items of Paediatric EPA Curriculum (11.1)
College of Paediatricians of South Africa · Paediatrics / Paediatrics · effective 2026-01-01
[link] [aria-label="Edit Paediatric EPA Curriculum (11.1)"] Edit this version
Adopted by 1 institution, Kgosi Kgari Teaching Hospital; 3 registrars in training on it. A target and its period are read live. Changing one changes what every trainee on this curriculum is measured against, in every institution that has adopted it, including periods that have already closed. A tool list is checked when an activity is filed and submitted, and never takes back credit already earned.
#### Items (15)
[link] Add item
Paediatric EPA Curriculum (11.1)'s items, in EPA order
| EPA | Target and decision | Minimum level | Minimum by training year | Tools | Actions |
| PAED-001 — Providing paediatric emergency care to children | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | [link] [aria-label="Edit PAED-001"] Edit [button] [aria-label="Remove PAED-001"] [aria-haspopup="dialog"] Remove… |
| PAED-002 — Managing common paediatric presentations | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, CCA, Chart-stimulated recall, Clinical audit, Direct observation, DOPS, Mini-CEX, MSF, RCA | [link] [aria-label="Edit PAED-002"] Edit [button] [aria-label="Remove PAED-002"] [aria-haspopup="dialog"] Remove… |
| PAED-003 — Providing intensive care to children | 3 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | [link] [aria-label="Edit PAED-003"] Edit [button] [aria-label="Remove PAED-003"] [aria-haspopup="dialog"] Remove… |
| PAED-004 — Managing common neonatal conditions | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, CCA, Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-004"] Edit [button] [aria-label="Remove PAED-004"] [aria-haspopup="dialog"] Remove… |
| PAED-005 — Providing neonatal care in intensive and high-care settings | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, DOPS, MSF | [link] [aria-label="Edit PAED-005"] Edit [button] [aria-label="Remove PAED-005"] [aria-haspopup="dialog"] Remove… |
| PAED-006 — Managing long-term health conditions (LTHCs) in children | 2 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CCA, Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-006"] Edit [button] [aria-label="Remove PAED-006"] [aria-haspopup="dialog"] Remove… |
| PAED-007 — Maintaining and promoting the health and well-being of children | 1 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-007"] Edit [button] [aria-label="Remove PAED-007"] [aria-haspopup="dialog"] Remove… |
| PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, DOPS, Mini-CEX, MSF, Reflective exercise | [link] [aria-label="Edit PAED-008"] Edit [button] [aria-label="Remove PAED-008"] [aria-haspopup="dialog"] Remove… |
| PAED-009 — Providing care for an adolescent patient | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Direct observation, MSF | [link] [aria-label="Edit PAED-009"] Edit [button] [aria-label="Remove PAED-009"] [aria-haspopup="dialog"] Remove… |
| PAED-010 — Leading and operating within a clinical team | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | Direct observation, MSF | [link] [aria-label="Edit PAED-010"] Edit [button] [aria-label="Remove PAED-010"] [aria-haspopup="dialog"] Remove… |
| PAED-011 — Managing population health challenges | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Direct observation, MSF | [link] [aria-label="Edit PAED-011"] Edit [button] [aria-label="Remove PAED-011"] [aria-haspopup="dialog"] Remove… |
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, Chart-stimulated recall, Mini-CEX, MSF | [link] [aria-label="Edit PAED-012"] Edit [button] [aria-label="Remove PAED-012"] [aria-haspopup="dialog"] Remove… |
| PAED-013 — Managing child protection, legal and ethical aspects of children at risk | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Direct observation, Mini-CEX, MSF | [link] [aria-label="Edit PAED-013"] Edit [button] [aria-label="Remove PAED-013"] [aria-haspopup="dialog"] Remove… |
| PAED-014 — Providing palliative and end-of-life care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, MSF, Reflective exercise | [link] [aria-label="Edit PAED-014"] Edit [button] [aria-label="Remove PAED-014"] [aria-haspopup="dialog"] Remove… |
| PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 1 per semesterDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Direct observation, Learner feedback, MSF, Portfolio and logbook review | [link] [aria-label="Edit PAED-015"] Edit [button] [aria-label="Remove PAED-015"] [aria-haspopup="dialog"] Remove… |
| KGK-001 — Running a paediatric outreach clinic at a district hospitalKgosi Kgari Teaching Hospital's own item | ‹target›‹decision› | ‹minimum›CPSA Paediatric Entrustment Scale v11.1 | ‹by year› | ‹tools› | [link] [aria-label="Edit KGK-001"] Edit [button] [aria-label="Remove KGK-001"] [aria-haspopup="dialog"] Remove… |

######## BOARD R2-F18-390.dc.html
IllustrationR2-F18-390 · 390
Demo Administrator on 11.1's items after 6.26 · KGK-001's row last, "Kgosi Kgari Teaching Hospital's own item"; Edit and Remove… on every row; the reach.
[link] Wombat
Acting asAdministrator
Menu
Acting asAdministrator
[aria-label="Main"]
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Curricula
-
- [aria-current="page"] Items of Paediatric EPA Curriculum (11.1)
### Items of Paediatric EPA Curriculum (11.1)
College of Paediatricians of South Africa · Paediatrics / Paediatrics · effective 2026-01-01
[link] [aria-label="Edit Paediatric EPA Curriculum (11.1)"] Edit this version
Adopted by 1 institution, Kgosi Kgari Teaching Hospital; 3 registrars in training on it. A target and its period are read live. Changing one changes what every trainee on this curriculum is measured against, in every institution that has adopted it, including periods that have already closed. A tool list is checked when an activity is filed and submitted, and never takes back credit already earned.
#### Items (15)
[link] Add item
Paediatric EPA Curriculum (11.1)'s items, in EPA order
| EPA | Target and decision | Minimum level | Minimum by training year | Tools | Actions |
| PAED-001 — Providing paediatric emergency care to children | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | [link] [aria-label="Edit PAED-001"] Edit [button] [aria-label="Remove PAED-001"] [aria-haspopup="dialog"] Remove… |
| PAED-002 — Managing common paediatric presentations | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, CCA, Chart-stimulated recall, Clinical audit, Direct observation, DOPS, Mini-CEX, MSF, RCA | [link] [aria-label="Edit PAED-002"] Edit [button] [aria-label="Remove PAED-002"] [aria-haspopup="dialog"] Remove… |
| PAED-003 — Providing intensive care to children | 3 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | [link] [aria-label="Edit PAED-003"] Edit [button] [aria-label="Remove PAED-003"] [aria-haspopup="dialog"] Remove… |
| PAED-004 — Managing common neonatal conditions | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, CCA, Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-004"] Edit [button] [aria-label="Remove PAED-004"] [aria-haspopup="dialog"] Remove… |
| PAED-005 — Providing neonatal care in intensive and high-care settings | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, DOPS, MSF | [link] [aria-label="Edit PAED-005"] Edit [button] [aria-label="Remove PAED-005"] [aria-haspopup="dialog"] Remove… |
| PAED-006 — Managing long-term health conditions (LTHCs) in children | 2 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CCA, Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-006"] Edit [button] [aria-label="Remove PAED-006"] [aria-haspopup="dialog"] Remove… |
| PAED-007 — Maintaining and promoting the health and well-being of children | 1 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-007"] Edit [button] [aria-label="Remove PAED-007"] [aria-haspopup="dialog"] Remove… |
| PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, DOPS, Mini-CEX, MSF, Reflective exercise | [link] [aria-label="Edit PAED-008"] Edit [button] [aria-label="Remove PAED-008"] [aria-haspopup="dialog"] Remove… |
| PAED-009 — Providing care for an adolescent patient | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Direct observation, MSF | [link] [aria-label="Edit PAED-009"] Edit [button] [aria-label="Remove PAED-009"] [aria-haspopup="dialog"] Remove… |
| PAED-010 — Leading and operating within a clinical team | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | Direct observation, MSF | [link] [aria-label="Edit PAED-010"] Edit [button] [aria-label="Remove PAED-010"] [aria-haspopup="dialog"] Remove… |
| PAED-011 — Managing population health challenges | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Direct observation, MSF | [link] [aria-label="Edit PAED-011"] Edit [button] [aria-label="Remove PAED-011"] [aria-haspopup="dialog"] Remove… |
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, Chart-stimulated recall, Mini-CEX, MSF | [link] [aria-label="Edit PAED-012"] Edit [button] [aria-label="Remove PAED-012"] [aria-haspopup="dialog"] Remove… |
| PAED-013 — Managing child protection, legal and ethical aspects of children at risk | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Direct observation, Mini-CEX, MSF | [link] [aria-label="Edit PAED-013"] Edit [button] [aria-label="Remove PAED-013"] [aria-haspopup="dialog"] Remove… |
| PAED-014 — Providing palliative and end-of-life care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, MSF, Reflective exercise | [link] [aria-label="Edit PAED-014"] Edit [button] [aria-label="Remove PAED-014"] [aria-haspopup="dialog"] Remove… |
| PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 1 per semesterDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Direct observation, Learner feedback, MSF, Portfolio and logbook review | [link] [aria-label="Edit PAED-015"] Edit [button] [aria-label="Remove PAED-015"] [aria-haspopup="dialog"] Remove… |
| KGK-001 — Running a paediatric outreach clinic at a district hospitalKgosi Kgari Teaching Hospital's own item | ‹target›‹decision› | ‹minimum›CPSA Paediatric Entrustment Scale v11.1 | ‹by year› | ‹tools› | [link] [aria-label="Edit KGK-001"] Edit [button] [aria-label="Remove KGK-001"] [aria-haspopup="dialog"] Remove… |

######## BOARD R2-F19.dc.html
R2-F19 · 1280
/admin/curricula/3/items/new · "Cannot add an item": Q14's words with Go to EPAs.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Curricula
-
-
[link] Items of Neonatology EPA Curriculum (1.0)
-
- [aria-current="page"] Add item
### Cannot add an item
On Neonatology EPA Curriculum (1.0).
Neonatology has no national EPA yet. Add one under EPAs, then come back to add it here.
[link] Go to EPAs

######## BOARD R2-F19-390.dc.html
R2-F19-390 · 390
/admin/curricula/3/items/new · "Cannot add an item": Q14's words with Go to EPAs.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Curricula
-
-
[link] Items of Neonatology EPA Curriculum (1.0)
-
- [aria-current="page"] Add item
### Cannot add an item
On Neonatology EPA Curriculum (1.0).
Neonatology has no national EPA yet. Add one under EPAs, then come back to add it here.
[link] Go to EPAs

######## BOARD R2-F20a-390.dc.html
R2-F20a-390 · 390
A.7.10 at 390 · EPAs: sixteen, stacked, each Edit 44 px.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] [aria-current="page"] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
### EPAs
College of Paediatricians of South Africa's EPAs. A change to one is read at once wherever it is named, in every institution that has adopted a curriculum that holds it.
[link] Create EPA
College of Paediatricians of South Africa's EPAs
| EPA | Sub-speciality | Status | Actions |
| [link] PAED-001 — Providing paediatric emergency care to children | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-001"] Edit |
| [link] PAED-002 — Managing common paediatric presentations | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-002"] Edit |
| [link] PAED-003 — Providing intensive care to children | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-003"] Edit |
| [link] PAED-004 — Managing common neonatal conditions | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-004"] Edit |
| [link] PAED-005 — Providing neonatal care in intensive and high-care settings | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-005"] Edit |
| [link] PAED-006 — Managing long-term health conditions (LTHCs) in children | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-006"] Edit |
| [link] PAED-007 — Maintaining and promoting the health and well-being of children | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-007"] Edit |
| [link] PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-008"] Edit |
| [link] PAED-009 — Providing care for an adolescent patient | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-009"] Edit |
| [link] PAED-010 — Leading and operating within a clinical team | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-010"] Edit |
| [link] PAED-011 — Managing population health challenges | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-011"] Edit |
| [link] PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-012"] Edit |
| [link] PAED-013 — Managing child protection, legal and ethical aspects of children at risk | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-013"] Edit |
| [link] PAED-014 — Providing palliative and end-of-life care | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-014"] Edit |
| [link] PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-015"] Edit |
| [link] PAED-016 — Transitioning adolescents with long-term conditions to adult care | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-016"] Edit |

######## BOARD R2-F20b-390.dc.html
R2-F20b-390 · 390
A.7.10 at 390 · Curricula.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
### Curricula
College of Paediatricians of South Africa's curriculum versions. A new version starts held back; an institution can adopt one once you open it.
[link] Create curriculum
College of Paediatricians of South Africa's curriculum versions
| Curriculum | Discipline | Effective | Items | Status | Actions |
| Neonatology EPA Curriculum (1.0) | Paediatrics / Neonatology | 2026-11-02 | 0 | Held back | [link] [aria-label="Edit Neonatology EPA Curriculum (1.0)"] Edit [link] [aria-label="Items of Neonatology EPA Curriculum (1.0)"] Items |
| Paediatric EPA Curriculum (11.2) | Paediatrics / Paediatrics | 2026-11-02 | 16 | Open | [link] [aria-label="Edit Paediatric EPA Curriculum (11.2)"] Edit [link] [aria-label="Items of Paediatric EPA Curriculum (11.2)"] Items |
| Paediatric EPA Curriculum (11.1) | Paediatrics / Paediatrics | 2026-01-01 | 15 | Open | [link] [aria-label="Edit Paediatric EPA Curriculum (11.1)"] Edit [link] [aria-label="Items of Paediatric EPA Curriculum (11.1)"] Items |

######## BOARD R2-F20c-390.dc.html
R2-F20c-390 · 390
A.7.10 at 390 · 11.2's items: the reach reads 1 registrar (after 6.36).
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Curricula
-
- [aria-current="page"] Items of Paediatric EPA Curriculum (11.2)
### Items of Paediatric EPA Curriculum (11.2)
College of Paediatricians of South Africa · Paediatrics / Paediatrics · effective 2026-11-02
[link] [aria-label="Edit Paediatric EPA Curriculum (11.2)"] Edit this version
Adopted by 1 institution, Kgosi Kgari Teaching Hospital; 1 registrar in training on it. A target and its period are read live. Changing one changes what every trainee on this curriculum is measured against, in every institution that has adopted it, including periods that have already closed. A tool list is checked when an activity is filed and submitted, and never takes back credit already earned.
#### Items (16)
Every national EPA of Paediatrics is already on this curriculum, as a national item or as an institution's own item, so there is none left to add. To add an item, first add a national EPA of Paediatrics, or remove a national item to free its EPA. An EPA an institution has as its own item is freed only when that institution removes it.
Paediatric EPA Curriculum (11.2)'s items, in EPA order
| EPA | Target and decision | Minimum level | Minimum by training year | Tools | Actions |
| PAED-001 — Providing paediatric emergency care to children | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | [link] [aria-label="Edit PAED-001"] Edit [button] [aria-label="Remove PAED-001"] [aria-haspopup="dialog"] Remove… |
| PAED-002 — Managing common paediatric presentations | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, CCA, Chart-stimulated recall, Clinical audit, Direct observation, DOPS, Mini-CEX, MSF, RCA | [link] [aria-label="Edit PAED-002"] Edit [button] [aria-label="Remove PAED-002"] [aria-haspopup="dialog"] Remove… |
| PAED-003 — Providing intensive care to children | 3 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | [link] [aria-label="Edit PAED-003"] Edit [button] [aria-label="Remove PAED-003"] [aria-haspopup="dialog"] Remove… |
| PAED-004 — Managing common neonatal conditions | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, CCA, Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-004"] Edit [button] [aria-label="Remove PAED-004"] [aria-haspopup="dialog"] Remove… |
| PAED-005 — Providing neonatal care in intensive and high-care settings | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, DOPS, MSF | [link] [aria-label="Edit PAED-005"] Edit [button] [aria-label="Remove PAED-005"] [aria-haspopup="dialog"] Remove… |
| PAED-006 — Managing long-term health conditions (LTHCs) in children | 2 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CCA, Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-006"] Edit [button] [aria-label="Remove PAED-006"] [aria-haspopup="dialog"] Remove… |
| PAED-007 — Maintaining and promoting the health and well-being of children | 1 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-007"] Edit [button] [aria-label="Remove PAED-007"] [aria-haspopup="dialog"] Remove… |
| PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, DOPS, Mini-CEX, MSF, Reflective exercise | [link] [aria-label="Edit PAED-008"] Edit [button] [aria-label="Remove PAED-008"] [aria-haspopup="dialog"] Remove… |
| PAED-009 — Providing care for an adolescent patient | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Direct observation, MSF | [link] [aria-label="Edit PAED-009"] Edit [button] [aria-label="Remove PAED-009"] [aria-haspopup="dialog"] Remove… |
| PAED-010 — Leading and operating within a clinical team | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | Direct observation, MSF | [link] [aria-label="Edit PAED-010"] Edit [button] [aria-label="Remove PAED-010"] [aria-haspopup="dialog"] Remove… |
| PAED-011 — Managing population health challenges | 2 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Direct observation, MSF | [link] [aria-label="Edit PAED-011"] Edit [button] [aria-label="Remove PAED-011"] [aria-haspopup="dialog"] Remove… |
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, Chart-stimulated recall, Mini-CEX, MSF | [link] [aria-label="Edit PAED-012"] Edit [button] [aria-label="Remove PAED-012"] [aria-haspopup="dialog"] Remove… |
| PAED-013 — Managing child protection, legal and ethical aspects of children at risk | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Direct observation, Mini-CEX, MSF | [link] [aria-label="Edit PAED-013"] Edit [button] [aria-label="Remove PAED-013"] [aria-haspopup="dialog"] Remove… |
| PAED-014 — Providing palliative and end-of-life care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, MSF, Reflective exercise | [link] [aria-label="Edit PAED-014"] Edit [button] [aria-label="Remove PAED-014"] [aria-haspopup="dialog"] Remove… |
| PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 1 per semesterDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Direct observation, Learner feedback, MSF, Portfolio and logbook review | [link] [aria-label="Edit PAED-015"] Edit [button] [aria-label="Remove PAED-015"] [aria-haspopup="dialog"] Remove… |
| PAED-016 — Transitioning adolescents with long-term conditions to adult care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Mini-CEX, MSF | [link] [aria-label="Edit PAED-016"] Edit [button] [aria-label="Remove PAED-016"] [aria-haspopup="dialog"] Remove… |

######## BOARD R2-F20d-390.dc.html
R2-F20d-390 · 390
A.7.10 at 390 · Edit PAED-001's page: one field a row, each 44 px, Save item on top of Cancel; its subtitle with 1 registrar.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Curricula
-
-
[link] Items of Paediatric EPA Curriculum (11.2)
-
- [aria-current="page"] PAED-001
### Edit PAED-001 — Providing paediatric emergency care to children
On Paediatric EPA Curriculum (11.2). It applies to every institution that has adopted it. Adopted by 1 institution, Kgosi Kgari Teaching Hospital; 1 registrar in training on it.
EPA*
[selected] PAED-001 — Providing paediatric emergency care to children
Lists only the EPAs this item can name that no item on this curriculum already holds for the same trainees: the national EPAs of its sub-speciality and, for an item an institution added, that institution's own local EPAs. A national item holds its EPA for every institution; an institution's own item holds it for that institution alone.
Target
[value="1"]
Credited encounters required in each period below. This is the published frequency, not a total for the whole programme.
Per
Semester
[selected] Academic year (January to November)
A semester target applies to each semester on its own. A surplus in one semester does not carry into the next.
Decision cadence
No published cadence
Each semester
[selected] Each academic year
How often a committee takes the entrustment decision on this EPA (Annexure B). Separate from the target: an EPA observed each semester may be decided once a year. With no cadence the EPA is never due, but a committee can still decide it.
Decided by
[selected] The trainee's general panel
The committee the College names to decide this EPA instead of the trainee's general panel, such as the neonatal CCC for paediatric EPAs 4 and 5.
Opportunistic
(checkbox)
Decided as rotation or opportunity allows
For an EPA the College has decided as rotation or opportunity allows, such as paediatric EPAs 8, 9 and 13: its decision is never overdue. Needs a decision cadence.
Entrustment scale
[selected] CPSA Paediatric Entrustment Scale v11.1
Suggested because every item on this curriculum is pinned to it.
The ladder the minima are picked on. Changing it clears them, because the same number is a different rung on another ladder; changing back brings back what was picked on this one. Leave unpinned if you are not certain: a wrong scale makes assessments on another ladder stop counting toward the minimum.
Minimum level*
Choose a rung
1
2
3a
3b
[selected] 4
5
Minimum by training year
A trainee in a year listed here is measured against that year's rung instead of the minimum level. A year not listed uses the minimum level.
Year 1
1
[selected] 2
3a
3b
4
5
Year 2
1
2
[selected] 3a
3b
4
5
Year 3
1
2
3a
[selected] 3b
4
5
Year 4
1
2
3a
3b
[selected] 4
5
Completion window (months)
[value="12"]
Only used to suggest a trainee's expected completion date. It is not the target period, and credit does not check it.
Weight
Tools
The instruments that may be used as evidence for this EPA. None ticked means any instrument. Checked when an activity is filed and submitted; it never takes back credit already earned.
(checkbox) [checked]
CBD
(checkbox)
CCA
(checkbox)
Chart-stimulated recall
(checkbox)
Clinical audit
(checkbox)
Direct observation
(checkbox)
DOPS
(checkbox)
Learner feedback
(checkbox) [checked]
Mini-CEX
(checkbox) [checked]
MSF
(checkbox)
Portfolio and logbook review
(checkbox)
RCA
(checkbox)
Reflective exercise
[link] Cancel
[button] Save item

######## BOARD R2-F20e-390.dc.html
R2-F20e-390 · 390
A.7.10 at 390 · Cancel back: the items with Edit PAED-001 focused (?back).
Focus: Edit PAED-001.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Curricula
-
- [aria-current="page"] Items of Paediatric EPA Curriculum (11.2)
### Items of Paediatric EPA Curriculum (11.2)
College of Paediatricians of South Africa · Paediatrics / Paediatrics · effective 2026-11-02
[link] [aria-label="Edit Paediatric EPA Curriculum (11.2)"] Edit this version
Adopted by 1 institution, Kgosi Kgari Teaching Hospital; 1 registrar in training on it. A target and its period are read live. Changing one changes what every trainee on this curriculum is measured against, in every institution that has adopted it, including periods that have already closed. A tool list is checked when an activity is filed and submitted, and never takes back credit already earned.
#### Items (16)
Every national EPA of Paediatrics is already on this curriculum, as a national item or as an institution's own item, so there is none left to add. To add an item, first add a national EPA of Paediatrics, or remove a national item to free its EPA. An EPA an institution has as its own item is freed only when that institution removes it.
Paediatric EPA Curriculum (11.2)'s items, in EPA order
| EPA | Target and decision | Minimum level | Minimum by training year | Tools | Actions |
| PAED-001 — Providing paediatric emergency care to children | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | [link] [aria-label="Edit PAED-001"] Edit [button] [aria-label="Remove PAED-001"] [aria-haspopup="dialog"] Remove… |
| PAED-002 — Managing common paediatric presentations | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, CCA, Chart-stimulated recall, Clinical audit, Direct observation, DOPS, Mini-CEX, MSF, RCA | [link] [aria-label="Edit PAED-002"] Edit [button] [aria-label="Remove PAED-002"] [aria-haspopup="dialog"] Remove… |
| PAED-003 — Providing intensive care to children | 3 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | [link] [aria-label="Edit PAED-003"] Edit [button] [aria-label="Remove PAED-003"] [aria-haspopup="dialog"] Remove… |
| PAED-004 — Managing common neonatal conditions | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, CCA, Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-004"] Edit [button] [aria-label="Remove PAED-004"] [aria-haspopup="dialog"] Remove… |
| PAED-005 — Providing neonatal care in intensive and high-care settings | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, DOPS, MSF | [link] [aria-label="Edit PAED-005"] Edit [button] [aria-label="Remove PAED-005"] [aria-haspopup="dialog"] Remove… |
| PAED-006 — Managing long-term health conditions (LTHCs) in children | 2 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CCA, Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-006"] Edit [button] [aria-label="Remove PAED-006"] [aria-haspopup="dialog"] Remove… |
| PAED-007 — Maintaining and promoting the health and well-being of children | 1 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-007"] Edit [button] [aria-label="Remove PAED-007"] [aria-haspopup="dialog"] Remove… |
| PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, DOPS, Mini-CEX, MSF, Reflective exercise | [link] [aria-label="Edit PAED-008"] Edit [button] [aria-label="Remove PAED-008"] [aria-haspopup="dialog"] Remove… |
| PAED-009 — Providing care for an adolescent patient | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Direct observation, MSF | [link] [aria-label="Edit PAED-009"] Edit [button] [aria-label="Remove PAED-009"] [aria-haspopup="dialog"] Remove… |
| PAED-010 — Leading and operating within a clinical team | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | Direct observation, MSF | [link] [aria-label="Edit PAED-010"] Edit [button] [aria-label="Remove PAED-010"] [aria-haspopup="dialog"] Remove… |
| PAED-011 — Managing population health challenges | 2 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Direct observation, MSF | [link] [aria-label="Edit PAED-011"] Edit [button] [aria-label="Remove PAED-011"] [aria-haspopup="dialog"] Remove… |
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | CBD, Chart-stimulated recall, Mini-CEX, MSF | [link] [aria-label="Edit PAED-012"] Edit [button] [aria-label="Remove PAED-012"] [aria-haspopup="dialog"] Remove… |
| PAED-013 — Managing child protection, legal and ethical aspects of children at risk | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Direct observation, Mini-CEX, MSF | [link] [aria-label="Edit PAED-013"] Edit [button] [aria-label="Remove PAED-013"] [aria-haspopup="dialog"] Remove… |
| PAED-014 — Providing palliative and end-of-life care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, MSF, Reflective exercise | [link] [aria-label="Edit PAED-014"] Edit [button] [aria-label="Remove PAED-014"] [aria-haspopup="dialog"] Remove… |
| PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 1 per semesterDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Direct observation, Learner feedback, MSF, Portfolio and logbook review | [link] [aria-label="Edit PAED-015"] Edit [button] [aria-label="Remove PAED-015"] [aria-haspopup="dialog"] Remove… |
| PAED-016 — Transitioning adolescents with long-term conditions to adult care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | CBD, Mini-CEX, MSF | [link] [aria-label="Edit PAED-016"] Edit [button] [aria-label="Remove PAED-016"] [aria-haspopup="dialog"] Remove… |

######## BOARD R2-F20f-390.dc.html
R2-F20f-390 · 390
A.7.10 at 390 (T12) · 11.2's own page: one field a row, the Open for adoption row 44 px (S1), Save on top of Cancel, nothing sideways.
[link] Wombat
Acting asCollege admin
Menu
Acting asCollege admin
[aria-label="Main"]
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] [aria-current="page"] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Curricula
-
- [aria-current="page"] Paediatric EPA Curriculum (11.2)
### Paediatric EPA Curriculum (11.2)
College of Paediatricians of South Africa · Paediatrics / Paediatrics · effective 2026-11-02
[link] [aria-label="Items of Paediatric EPA Curriculum (11.2)"] Items (16)
#### Details
Sub-speciality*
Paediatrics / Neonatology
[selected] Paediatrics / Paediatrics
Name*
[value="Paediatric EPA Curriculum"]
Version*
[value="11.2"]
Effective from
[value="2026-11-02"]
Effective to
#### Open for adoption
Release
(checkbox) [checked]
Open for adoption
An institution can adopt this version only while it is open. Closing it does not affect an institution that has already adopted it.
Adopted by 1 institution, Kgosi Kgari Teaching Hospital; 1 registrar in training on it.
[link] Cancel
[button] [aria-haspopup="dialog"] Hold back…
[button] Save
#### A new version
A new version copies this version's 16 national items, with their targets, minimum levels and tools. An institution's own items are never copied. It starts held back.
New version*
[value="11.2-copy"]
Effective from
[value="2026-11-03"]
Effective to
[button] [aria-label="Clone Paediatric EPA Curriculum (11.2) as a new version"] Clone
