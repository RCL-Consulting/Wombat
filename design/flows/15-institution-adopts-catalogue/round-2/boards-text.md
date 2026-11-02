# Flow 15 round 2: the text of every board

Generated from `round-2/R2-*.dc.html` (T392, build-contracts § 0) by a script, as flow 14's was: style, script, svg, head and helmet blocks stripped; headings, labels, aria-labels, placeholders, input values, alt text, `[checked]`, `[disabled]`, `[aria-disabled]`, `[aria-haspopup]` and annotations kept; a link is marked `[link]`, a button `[button]`. `round-2/` carries no `canvas.json`, so the order is by id: the Spec and Steps boards, then A, B, C, D, E, F, each at 1280 then its other widths. An `anno` chip is quoted after `>`. Design data: a caption, a chip, a label strip or a `DATA:` note is never a shipped word. A table row is one `| a | b |` line; a `dt` opens with `**`.

######## BOARD R2-0-spec.dc.html
### R2-0-spec · flow 15, round 2
The picks: Curricula A1 (one page), the items B2 (one table, form pages of their own), the EPA list C2's without its Owner column or badge, with C1's page for KGK's own EPA and the national record, the move D1, and E, F, G as corrected. Full fidelity on Wombat v41 (ds/wombat), page "Round 2"; round 1's 17 boards untouched on "Round 1".
#### 1 · The captures: they arrived, read from the folder
The captures arrived, read from the folder, not as attachments. No image came with the message and the session's uploads were empty; the folder C:\dev\Wombat\design\upload was connected for this session (approved on the computer) and its files staged.
Opened and read (12 key, all present): act-1/1.19-2-epas-empty, 1.19-3-epa-create-no-subspeciality, 1.20-1-adopted, 1.21-1-institution-epas, 1.21-3-institution-items, 1.22-1-scales-read-only; act-6/6.25-1-kgk001-saved, 6.26-3-own-item-added, 6.32-1-adopted-11.2, 6.33-1-mbatha-curricula-list, 6.36-1-ndlovu-moved-to-11.2, 6.38-4-rebuilt. Of these, viewed as images: 6.25-1, 6.32-1, 6.33-1, 6.38-4; the other eight staged and checked present, not viewed.
Also opened (of the other 34): 6.26-2-add-form-before-rung (viewed: the item form's labels, options and helps), 6.34-3-profile-saved (viewed: the profile's Status and Email read-out, its subtitle, End this programme), 6.37-1-ndlovu-progress (viewed: My progress's subtitle, This period's lines and every count, so G carries no bracket). Staged, not viewed: 1.19-1, 1.20-0, 1.20-2, 6.22-2, 6.38-2, 6.38-3, states/epa-edit--not-found. Not opened: the remaining 24 (among them 6.22-1, 6.25-2, 6.33-2, 6.35-1, 6.38-1, 6.9-1, 6.9-2 and the states/ captures but one).
The shell's menus are the FIXTURES' (checked against 6.33-1 and 6.38-4: same groups, icons as built). Nothing is drawn from memory; what neither the message nor a capture gives stays a bracket ‹…›.
#### 2 · The changes the boards need, as code changes
Each row is the change a board assumes; decided ones name their Q.
| Where | Change | Status | Boards |
| NavItems | CurriculumAdoptions leaves the Institutional admin's Curriculum group; /admin/adoptions redirects to /admin/curricula; OnboardingLinks.CurriculumAdoptions re-pointed to /admin/curricula and renamed for Curricula; the four sentences re-pointed (§ 9). DESIGN.md's NavMenu table loses the row (NavMenuAuthorizationTests parses it). | PROPOSED, decided Q1 | every IA board (the menu) |
| GetCurriculaListQuery (an institution) | Per version AdoptedOn; SupersededOn, derived from the next adoption of the same institution and discipline (nothing stores it); the count of ACTIVE profiles pinned through its adoption; the own-item count (CurriculumDto.Items' IsLocal). CurriculumAdminScope's second arm widened: "an active or ended profile pinned to it, or an own item" (Q9). The Administrator gets the same per institution behind Show (Q18). | PROPOSED, decided Q9, Q10, Q18 · DATA | R2-A02 to A13 |
| GetAdoptableCurriculaQuery | Open versions the institution has not ACTIVELY adopted (today every active version, the adopted one included: GetAdoptableCurricula.cs:45), each with EffectiveFrom, its item count and its relation to the active adoption of its discipline (none, newer, older; by EffectiveFrom). For the dialog: the version it would supersede, that adoption's active-profile count, and the institution's own items on it. The page's adoption parts by AdministratorOrInstitutionalAdmin, not NationalCatalogueAccess (S5). | PROPOSED, decided Q7 · DATA | R2-A01, A03, A05, A08, A10, A11, A12 |
| AdoptCurriculumCommand | Refuses a held-back version before writing: "This curriculum version is not open for adoption." (today AdoptCurriculum.cs:47–52 loads it with no check). AdoptedOn by the South African day (today DateOnly.FromDateTime(DateTime.UtcNow), :77). Going back stays accepted (:54–66 refuse only the active one). The stale guards stay: "This institution has already adopted this curriculum version.", "The selected curriculum could not be found." | PROPOSED, decided Q5, Q7, T325 | R2-A07, A08; A16 (words only) |
| EpaDto / ListEpasForSubSpecialityQuery | The owning institution's name; a "may change" flag from UpdateEpa's own rule (UpdateEpa.cs:98–104), never from roles (T291 item 1, C17); each row's discipline read from the DTO, not by Single over the caller's adopted disciplines (EpasList.razor:39–40); a flag "the caller has an adopted discipline or a College" (Q12). | PROPOSED, decided Q3, Q12 · DATA | R2-C01 to C03, C09, C15 |
| GetEpaByIdQuery | Null for an EPA outside the list's rule (today GetEpas.cs:178–186 admits any Institutional admin to every national EPA): the page draws Unavailable. The "may change" flag chooses the form or the record. | PROPOSED, decided Q2, Q19 | R2-C08, C10, C11 |
| CreateEpaCommand, UpdateEpaCommand | Refuse an institution's own EPA in a discipline it has not actively adopted (today CreateEpa.cs:68–80 and UpdateEpa.cs:116–121 check the institution only). A duplicate only on the unique index's violation; any other DbUpdateException is T272's failure (CreateEpa.cs:100–103). | PROPOSED, decided Q12, T272 | R2-C05, C06, C07 |
| EpaEdit | Titled by the EPA (EpaOptionLabel), crumb its code, no Back to EPAs; Q11's help and dialog by the EPA's owner; a LoadError of its own (today an alert above an empty body, EpaEdit.razor:33, :204–207); titled "EPA" while loading or failed (T7). | PROPOSED, decided Q11 · PROPOSED (THIS ASK) T7 | R2-C08, C12 to C14, C18a, C18b, C19 |
| GetCurriculumByIdQuery; the item routes | AdoptedOn for the caller's institution (the subtitle); null for an id out of scope (Unavailable). Routes /admin/curricula/{{Id:int}}/items/new and …/items/{{ItemId:int}}, NavOwners rows lighting Curricula. The edit row and its colspan form go (T323). The EPA picker on edit as built (M6). Served to every caller who may add or edit an item (T4). | PROPOSED, decided Q14 · PROPOSED (THIS ASK) T4, T5 | R2-B01 to B20 |
| GetTraineeCurriculumChoicesQuery; TraineeProfileEdit | Each option's state for the line under the picker (the active adoption, or superseded on ‹date›). The line (.submit-check), the submit's label "Save profile and move…" when the picker moved, the ConfirmDialog. No new read for a count (Q20). | PROPOSED, decided Q8 · PROPOSED (THIS ASK) T2 | R2-D01 to D06 |
| CurriculumProgressRebuild | The failure's fixed words (today :13, :116 print the exception). The trigger stays primary. | PROPOSED, decided Q16 · PROPOSED (THIS ASK) T8 | R2-F05 |
| EntrustmentScalesList | The subtitle by role (Q13 to a reader; the Administrator keeps today's); the reader's empty state. | PROPOSED, decided Q13 | R2-E01 to E05 |
| The headings | Every <h3> section becomes an <h2> (AdoptionsList.razor:44, :63 and CurriculumItemsEdit.razor:76, :329 go with their pages). The Administrator's KGK sections are h3 under the h2 "An institution's curricula". | PROPOSED, decided | R2-A*, B* |
#### 3 · The class table
Every class the boards use. "Reuses" = app.css as shipped (bundle.css); nothing redeclared.
| Class | Kind | Where, and why |
| .header-container, .page-subtitle, .breadcrumbs (breadcrumb-ancestor, -parent, -current, -separator, -back) | reuses | PageHeader with its trail and actions (.actions-cell takes the row at 390) |
| .table-container, .clinic-table, .clinic-table--stack, .clinic-table--stack-wide, .col-wrap, .col-wrap--wide, .col-fit, .actions-cell, .visually-hidden | reuses | DataTable with StackWide: every list (Curricula's tables, the items, the EPAs, the scales). .col-wrap wraps long values in their own cell |
| .muted .text-sm (on a div under the title) | reuses | N1's replacement: "Your institution's own EPA / item" and "(inactive: not in force)" in words (M4) |
| .muted | reuses | N2's replacement: a one-line note; "Set by the College." in a national row's actions cell |
| .form-section-head | reuses | N3's replacement: "Items (15)" with "Add your institution's own item" on its baseline |
| .user-page, .user-section, dl.detail-list | reuses | N4's replacement: the national EPA's record (flow 12's column); also the EPA form pages, the item form pages, the Unavailable pages, the profile and the rebuild page, for their 44 px controls and stacked actions at 390 |
| .submit-check | reuses | N5's replacement: the move's words under the picker (info tint, the action-blue stripe) |
| dl.details-list | reuses | N6's replacement: the rebuild's five figures, as built |
| .form-container (paragraphs, .muted) | reuses | N7's replacement: the rebuild page as built |
| .form-actions | reuses | N8's replacement: stacks under .user-page and .dialog-card at 390, primary (or confirm) on top, the safe button first in the DOM |
| .alert (-success, -danger, -warning, -info), .alert-row, .alert-row-text, .action-result | reuses | Alert and ActionResult; a standing notice is role="" |
| .detail-card--empty, .state-panel-title, .state-panel-copy, .skeleton, .skel-title, .skel-line, .skel-short, .loading-line | reuses | StatePanel and Skeleton (N5: .state-panel-title, never .panel-title) |
| .form-group, .form-grid, .form-control, .form-select, .form-check, .form-check-input, .check-grid, .field-help, .validation-message, .invalid, .required-mark | reuses | FormField, FieldHelp, refusals under their field |
| .dialog-card, .dialog-scrim, .dialog-form, .dialog-body, .dialog-body--scroll | reuses | ConfirmDialog in action mode; the adoption's body scrolls inside the dialog |
| .search-container, .search-grid, .search-field, .filter-actions | reuses | The Administrator's Institution + Show (flow 06's filter) |
| .pager | reuses | PagerControls on the Administrator's 40 EPAs |
| .badge, .badge-draft, .badge-standing-none, .dashboard-metric-row, .dashboard-metric, .dashboard-metric-label | reuses | My progress only, as flow 05 built it (G). No badge for an owner or an EPA's status anywhere else |
| .sidebar and the shell's classes (.brand, .nav-*, .role-*, .account-*, .phone-role, .nav-toggle-label) | reuses | The flow 01 shell, as built |
| .curriculum-item-page | NEW shipped (S1) | The item form pages below 641 px: Tools' and Opportunistic's rows 44 px, each label filling its row, as .activity-form and .review-page do. The only shipped rule in f15r2.css |
| .cv-board, .cv-w1280/768/390, .cv-label*, .cv-chip, .cv-stage, .cv-shell, .cv-main, .cv-topbar, .cv-article, .cv-overlay, .cv-note, .cv-elide, .cv-doc, .cv-tag, .cv-legend | canvas only (K1–K9) | Lay the canvas out: the label strip and its "Illustration" chip, the shell made static (no fixed bar, no sticky sidebar), a dialog drawn open over its page, annotations. Kept off the components; never shipped |
No new token, no new icon, no new badge. Icons named by Lucide name as built: download (Export), refresh-cw (Try again), chevron-left / chevron-right (the trail), menu, user, log-out, and the menus' own.
#### 4 · 390, 768 and 1100
- 44 px at 390 comes from the system: StackWide's .clinic-table--stack-wide .btn (900 px down) on every list; .user-page on the EPA pages, the item form pages, the Unavailable pages, the profile and the rebuild page (controls, buttons, stacked .form-actions full width); .curriculum-item-page for the item form's checkboxes; .dialog-card .form-actions for every dialog; .header-container .actions-cell for header actions; .breadcrumb-parent for the trail's parent link. No inline style on any board.
- 768: StackWide stacks from 900 px down, so every table at 768 (and at 1100 the items table is a table) is stacked, a block per row, each cell labelled.
- Nothing scrolls sideways at 1280, 1100, 768 or 390: the items table is seven columns (Scale folded into Minimum level; window and weight one cell) with .col-wrap on EPA, decision, minimum and tools, so "Neonatal team Clinical Competency Committee" and a 150-character title wrap in their cells (R2-C19); a curriculum's name with its version wraps in .col-wrap; the dialogs are 28rem, their bodies scrolling inside at 390.
#### 5 · The DESIGN.md text each changes
| Section | Change |
| § Table system, T211 paragraph | The own EPA is marked as the own item is, on the EPA list too: "Your institution's own EPA / item" in .muted .text-sm under its title (to the Administrator "Kgosi Kgari Teaching Hospital's own …"); a national row's actions cell "Set by the College."; words, never a badge. |
| The picker rule | Adoption is by rows: an adopted version is a row with its state, never an option; an older version is offered under "Open for adoption". |
| § Button system | The adoption of a superseding version and the move are confirmed (ConfirmDialog, opening on Cancel); the rebuild stays primary with no "…". |
| § Form system | Refusals under their field on the EPA and item pages (Code, Sub-speciality, EPA) and on the profile (Curriculum), first in aria-describedby; no ValidationSummary at the top; a failure with no field is an alert above the form. |
| § Page-level patterns | An EPA's page is titled by the EPA; a national EPA is a read-only record (.user-page); Unavailable gains "EPA unavailable", "Curriculum unavailable", "Item unavailable"; the items list is the page and the item's form a page of its own; a page that cannot load its record titles itself by what it knows (T7). |
| § The NavMenu's table | Curriculum adoptions gone. |
#### 6 · Tokens and contrast
Every colour is an existing token; f15r2.css's canvas rules use only tokens. Pairs drawn, from DESIGN.md § Contrast: text-color on surface, header-bg and every tint (≥ 11.23:1); muted-text (the .muted mark) on surface 5.09, on hover-bg (a hovered row) 4.83, on header-bg 4.57, on info-bg (under .submit-check) 4.77, on warning-bg 4.79; link-color on surface 6.70; danger-color (a refusal) on surface 5.95; input-border 3.80; focus-ring 4.86 on surface, 4.55 on info-bg. All pass.

######## BOARD R2-0-spec-2.dc.html
### R2-0-spec-2 · messages, M/S/N/T, edge cases
#### 7 · DATA, by query
- DATA GetCurriculaListQuery: AdoptedOn; SupersededOn (derived); active-profile count; own-item count; for the items subtitle, AdoptedOn for the caller's institution.
- DATA GetAdoptableCurriculaQuery: EffectiveFrom; item count; newer / older / none against the active adoption (by EffectiveFrom; the College never back-dates); the superseded version's active-profile count and the institution's own items on it (the dialog).
- DATA EpaDto: the owner's name; "may change"; the discipline; "has an adopted discipline or a College".
- DATA The items page: KGK's own items on another version (11.2's line "Your item on 11.1 stays there.").
- DATA GetTraineeCurriculumChoicesQuery: each option's state (active adoption, or superseded on ‹date›).
#### 8 · Every message, by kind
Built = in the code today; round 1 = drawn in round 1 and kept; NEW = this message's; PROPOSED = this message's choice.
| Kind | Words | Source | Board |
| result | Paediatric EPA Curriculum (11.1) adopted. New admissions to Paediatrics / Paediatrics join it. | round 1 (replaces "Curriculum adopted.") | R2-A02 |
| result | Paediatric EPA Curriculum (11.2) adopted. New admissions to Paediatrics / Paediatrics join it. Paediatric EPA Curriculum (11.1) is superseded; its 3 registrars stay on it until each is moved. | round 1 | R2-A05 |
| refusal | This curriculum version is not open for adoption. Its College has held it back, so nothing was adopted. | round 1 (Q5) | R2-A07 |
| refusal (stale guard) | This institution has already adopted this curriculum version. / The selected curriculum could not be found. | built | none (unreachable from the page) |
| hint | Your institution has adopted no Paediatrics / Paediatrics version yet. / Newer than the version your institution has adopted (11.1). / Older than the version your institution has adopted (11.2). | NEW (replaces nothing; A2's line) | R2-A01, A03, A05 |
| notice | Adopting a version makes it the one that new admissions to its discipline join. … | NEW (replaces "Re-adopting a newer version of a discipline you already follow supersedes …") | R2-A* |
| confirmation | Adopt Paediatric EPA Curriculum (11.2) for Kgosi Kgari Teaching Hospital? It supersedes … / The 3 registrars on 11.1 stay on it … / Your institution's own item on 11.1, KGK-001, is not carried over … | NEW | R2-A03, A08, A11 |
| empty | No curricula yet / Your institution has not adopted a curriculum yet. Adopt one below before admitting trainees. | round 1 (replaces "… on the Curriculum adoptions page …") | R2-A01 |
| failure | Could not load the curricula. Nothing has changed. Try again, or come back in a few minutes. | round 1 | R2-A15 |
| subtitle | Every College's curricula; and, for one institution, what it trains against and may adopt. | NEW (N7) | R2-A09 to A11 |
| notice | The items page's standing notice; the paused EPA's notice; "(inactive: not in force)" | built | R2-B01, B02 |
| hint | A new version carries no institution's own items. Your item on 11.1 stays there. | round 1 (B1) | R2-B07, B14 |
| result | Curriculum item added. / Curriculum item saved. / KGK-001 removed from this curriculum. | built | R2-B06, B08, B10, B14 |
| confirmation | Remove KGK-001 from Paediatric EPA Curriculum (11.2)? (title) with the built body, em dash | PROPOSED (THIS ASK) T3 (replaces "Remove this item?") | R2-B13 |
| refusal | ‹the command's refusal› under EPA | built (words not given) | R2-B05, B12 |
| unavailable | Item unavailable / This curriculum item does not exist, or you cannot change it. · Curriculum unavailable / This curriculum does not exist, or you cannot open it. · EPA unavailable / This EPA does not exist, or you cannot open it. | decided (Q19) | R2-B15, B16, C11 |
| failure | Could not load this curriculum's items. / Could not load the EPAs. / Could not load this EPA. / Could not load the entrustment scales. | round 1 | R2-B19, C17, C18b, E04 |
| subtitle | The EPAs of the curricula Kgosi Kgari Teaching Hospital has adopted. The College's are read only; your institution's own are yours to change. / Every College's EPAs, and every institution's own. | NEW | R2-C* |
| empty | No EPAs yet / Your institution has not adopted a curriculum yet, so there is no discipline to add an EPA to. Adopt one under Curricula. | decided (Q12; replaces "Create the first EPA …") | R2-C01 |
| help | The disciplines your institution has adopted. (Sub-speciality) | NEW | R2-C04 |
| refusal | Your institution has not adopted Paediatrics / Neonatology, so an EPA cannot be added to it. | NEW | R2-C07 |
| refusal | An EPA with the same code already exists for this sub-speciality. (a real duplicate only) | built | R2-C05 |
| failure | The EPA could not be saved. Nothing has changed. Try again, or come back in a few minutes. | round 1 (T272) | R2-C06 |
| help | An inactive EPA cannot be chosen by Kgosi Kgari Teaching Hospital's trainees for a new activity … | NEW (Q11; replaces "An inactive EPA cannot be chosen for a new activity …") | R2-C08 |
| confirmation | Deactivate KGK-001? It can no longer be chosen … for Kgosi Kgari Teaching Hospital's trainees. … | NEW (Q11, S8; replaces "Deactivate this EPA?") | R2-C12 |
| result | EPA saved. / EPA deactivated. / EPA reactivated. No activity was completed … (or 1 activity … now counts …) | built (N4) | R2-C08, C13, C14 |
| hint | Anele Dlamini is on Paediatric EPA Curriculum (11.1), which your institution superseded on 2026-10-30. Keeping it keeps Anele Dlamini on 11.1. | NEW | R2-D01 |
| hint | When you save, Sipho Ndlovu moves to Paediatric EPA Curriculum (11.2). Sipho Ndlovu's completed activities are counted again … cannot be moved back to 11.1 while 11.2 is the version your institution has adopted. | NEW (M1, M2) | R2-D02 |
| confirmation | Move Sipho Ndlovu to Paediatric EPA Curriculum (11.2)? (round 1's title) · Move and save · body NEW | round 1 + NEW | R2-D03 |
| hint | The version your institution has adopted for Paediatrics / Paediatrics. | round 1 (N2's word) | R2-D05 |
| result | Trainee profile saved. 1 completion was checked against 11.2, and 1 counts towards it. (and the other built results) | built | R2-D01, D05, D07a, D07b |
| refusal | A trainee can be moved only into the curriculum version this institution has adopted for their discipline, so nothing was saved. | built | R2-D06 |
| subtitle | The rating ladders that assessment forms and committee reviews use. A Wombat administrator sets them; they are shown here for reference. | decided (Q13; replaces "Define and maintain …") | R2-E* |
| empty | No scales yet / A Wombat administrator has not set up an entrustment scale yet. | NEW | R2-E05 |
| result | Curriculum progress was rebuilt. (five figures) | built | R2-F04 |
| failure | The rebuild failed, and nothing was changed. Try again, or come back in a few minutes. | NEW (Q16; replaces "The rebuild failed and nothing was changed: <exception>") | R2-F05 |
#### 9 · Re-pointed (S4; no board)
- OnboardingLinks.CurriculumAdoptions → /admin/curricula, renamed for Curricula.
- Invite a person: "Nothing is adopted at Kgosi Kgari Teaching Hospital yet, so roles that need a speciality cannot be invited. Adopt a curriculum under Curricula." (link "Curricula") NEW
- Admit and the profile: "This institution has adopted no curriculum yet, so no trainee can be admitted. Adopt one under Curricula, then admit the trainee." (link "Curricula") NEW
- Home's Curriculum card footer: "Open Curricula" (flow 12's card, its link only).
- Curricula's own empty body (R2-A01).
#### 10 · PROPOSED (THIS ASK)
|  | Choice | Board |
| T1 | Going back: a superseded, still-open version listed under "Open for adoption" with "Adopt…" and the "Older than…" line; its row above keeps Items alone. | R2-A05, A06a, A06b, A08 |
| T2 | "Save profile and move…" when the picker moved. | R2-D02 to D04 |
| T3 | The Remove dialog's title names its item. | R2-B13 |
| T4 | The items page and its form pages serve every caller who may add or edit an item (College admin and Administrator too). | R2-B17 |
| T5 | No Back to curricula / Back to curriculum on the items page for anyone. | R2-B* |
| T6 | Create EPA's submit stays "Save". | R2-C04 |
| T7 | A page that cannot load its record titles itself "EPA" / "Curriculum items". | R2-B18, B19, C18a, C18b |
| T8 | The rebuild's failure offers no Try again button; Rebuild progress stays. | R2-F05 |
| T9 | The College's and the Administrator's Curricula Status reads Open / Held back. | R2-A09, A12 |
| T10 | The items table in seven columns, its caption "… 's items, in EPA order", its h2 "Items (15)". | R2-B* |
| T11 | The Remove result "KGK-001 removed from this curriculum." focused at the list's top. | R2-B14 |
| T12 | On KGK-001's page the Active box and Deactivate… stay as built; the results are the built ones. | R2-C08, C12 to C14 |
#### 11 · The corrections, each with its board
|  | Where drawn |
| M1 no gendered pronoun | R2-D01 to D07b (every sentence written with the name); every annotation checked |
| M2 going back; conditional move words | R2-A05, A08 (T1); R2-D02, D03 ("while 11.2 is the version your institution has adopted"); P5 dropped |
| M3 rebuild primary, no "…" | R2-F01 to F05 |
| M4 owner mark in words, .muted .text-sm | R2-B06, B08, B17, B20, C09, C15 |
| M5 every changed Expect a row | R2-0-steps |
| M6 EPA a required select on edit | R2-B09, B12 |
| S1 no Adoption history | R2-A* |
| S2 3 then 2 active registrars | R2-A05, A06a, A06b, A10 |
| S3 DATA | § 7 |
| S4 re-pointed | § 9 |
| S5 adoption parts by AdministratorOrInstitutionalAdmin | R2-A12 |
| S6 no Owner column; College column kept | R2-C* |
| S7 Status in plain words | R2-C02, C03 |
| S8 deactivate keeps built sentences | R2-C12 |
| S9 no "On your institution's curricula" | R2-C10 |
| S10 scales: subtitle, no column, count | R2-E* |
| S11 the profile's built labels | R2-D* |
| S12 no bracket where a value is given | every board; G from capture 6.37-1 |
| S13 no inline style | every board |
| S14 no class for N1–N8 | § 3 |
| S15 768, the Administrator on items, EPA load error, refused add/save, College admin, Administrator past Show | R2-*-768, B17, C18b, B05, B12, A12, A10, A11 |
| S16 items table fits 1280 | R2-B01, B08 |
| N1 one counting rule | R2-0-steps; journey counts in the boards' labels |
| N2 "adopted", never "follows" | every board |
| N3 1.19's EPA empty state | R2-C01 |
| N4 built EPA results | R2-C08, C13, C14 |
| N5 .state-panel-title | R2-A01, E05, C01 |
| N6 no id on any page | R2-B15, B16, C11 |
| N7 the Administrator's subtitle | R2-A09 |
Illustrations (a state the replay never reaches; chip on the board, word in its title): R2-A07, A08, A11, A13, B05, B12, B20, C06, C07, C15, C19, D06, D07a, D07b, E05, F05.
#### 12 · Edge cases
- A second discipline adopted beside Paediatrics: R2-A13.
- A College version held back after KGK adopted it: KGK keeps it; its row unchanged (words only).
- A superseded version with no registrar (active or ended) and no own item: it leaves the list (words only).
- A move while the registrar's completion is credited elsewhere: the save waits (T304); no board.
- KGK's own EPA in a discipline KGK no longer adopts: listed, Edit offered, Create refused for that discipline; the list reads the DTO's discipline (words only).
- Two institutions' own EPAs with one code on the Administrator's list: R2-C15.
- A 150-character title: R2-C19.
- A registrar on 11.1 when KGK supersedes 11.2 with 11.3: the picker offers 11.1 and 11.3, never 11.2 (words only).
- R2-A16: an adoption between 00:00 and 02:00 SAST is dated the South African day (T325; words only).
- Going back: R2-A08.
#### 13 · Accessibility
One h1 a screen; h2 per section in order (h3 only under "An institution's curricula"); every table captioned with scope on its headers and the row's th scope="row"; every row action named for its row, no two alike ("Adopt Paediatric EPA Curriculum (11.2)…", "Items of …", "Edit KGK-001", "Remove KGK-001", "Edit KGK-001 — Running …"); owner and pause told in words; focus after every action on the board's label; refusals first in aria-describedby; help outside the label, named by aria-describedby; standing notices role=""; results role="status"; dialogs open on Cancel; targets ≥ 24 px at 1280 and 44 px at 390.

######## BOARD R2-0-steps.dc.html
### R2-0-steps · flow 15, round 2
The counting rule (one, used on every board). A journey counts from Home: presses + fields typed or chosen. A menu link is a press; at 390, opening the menu is one more. A dialog's confirm is a press. A field that arrives chosen (KGK-001 as the only EPA, the suggested scale, the preset sub-speciality) counts as none. A page the app returns to by itself (back on the list after Add item) is no press.
#### 1 · The journeys
Each press named; today's beside it.
| Journey | 1280 | 390 | Today | Presses, fields |
| First adoption (1.20, Nolwazi Mbatha) | 2 + 0 | 3 + 0 | 2 + 1 | Curricula; Adopt on 11.1 |
| A newer adoption (6.32, Nolwazi Mbatha) | 3 + 0 | 4 + 0 | 2 + 1, no question | Curricula; Adopt… on 11.2; Adopt 11.2 |
| KGK's own EPA and item (6.25 + 6.26) | 7 + 9 | 9 + 9 | 6 + 9 by the rule (EPAs, Create EPA, 4 fields, Save; Curricula, Items, 5 fields, Add item: the add form sits under the table today, so no press opens it); 6 + 10 if today's Sub-speciality arrives unchosen (not seen in a capture before Save). Round 1's 6 + 13 counted fields that arrive chosen. | EPAs; Create EPA; Code, Title, Description, Category; Save · Curricula; Items of 11.1; Add your institution's own item; Target, Per, Decision cadence, Minimum level, Tools; Add item (EPA, scale, Decided by and window arrive chosen) |
| A move (6.36) | 4 + 1 | 5 + 1 | 3 + 1 | Trainees; Edit; choose 11.2; Save profile and move…; Move and save |
| The rebuild (6.38, Demo Administrator) | 4 + 0 | 5 + 0 | 4 + 0 | Curricula; Curriculum progress; Rebuild progress; Rebuild progress (confirm) |
| The Administrator adopting for KGK | 4 + 1 | 5 + 1 | ‹unplayed› | Curricula; the institution (field); Show; Adopt…; Adopt 11.2 |
#### 2 · Each step whose Expect changes, and its new Expect
Values person by person from FIXTURES. "unchanged" said where so.
| Step | Board | Today's words | Once built |
| 1.10 Mbatha registers | — | "Open Curriculum adoptions"; the Curriculum group led by Curriculum adoptions | The Curriculum card's footer reads "Open Curricula" (to /admin/curricula); the Curriculum group reads Curricula, EPAs, Activity types, Entrustment scales. The rest unchanged. |
| 1.18 Kruger, 11.1's items | R2-B17 (shape) | "Curriculum items", Back to curricula, Back to curriculum, the edit row | The page is "Items of Paediatric EPA Curriculum (11.1)" (tab with " · Wombat"), trail Home › Curricula › Items of Paediatric EPA Curriculum (11.1), neither Back link (T5); the College's warning unchanged; 15 rows each with Edit and Remove…; no Add, the built "Every national EPA of Paediatrics is already on this curriculum, …" in its place (T4). |
| 1.19 | R2-A01, R2-C01 | "… on the Curriculum adoptions page …"; "Create the first EPA …" | Curricula: the empty first section, "Open for adoption" with 11.1 and IM Core Curriculum (2026.1), no Create curriculum. EPAs: "No EPAs yet" / "Your institution has not adopted a curriculum yet, so there is no discipline to add an EPA to. Adopt one under Curricula." with Go to Curricula, no Create EPA. |
| 1.20 | R2-A01, R2-A02 | Route /admin/adoptions; "Curriculum adopted."; the second Adopt refused | Route /admin/curricula. Do: "Open Curricula. Under Open for adoption, press Adopt on Paediatric EPA Curriculum (11.1)." Expect: no institution picker; Adopt asks nothing; "Paediatric EPA Curriculum (11.1) adopted. New admissions to Paediatrics / Paediatrics join it." takes the focus; 11.1 in Curricula you train against (College of Paediatricians of South Africa, Paediatrics / Paediatrics, effective 2026-01-01, 15 national · 0 your own, Adopted on 2026-10-30, Items); Open for adoption lists IM Core Curriculum (2026.1) alone; /admin/adoptions lands on Curricula; no Curriculum adoptions in the menu. The second press and "This institution has already adopted this curriculum version." leave the Expect. |
| 1.21 | R2-C02, R2-A02, R2-B01 | Edit on every EPA; Curricula's Status; "Set by the College" per item | EPAs: the 15 PAED rows, each title a link, each actions cell "Set by the College.", no Edit, Create EPA in the header. Curricula: the one row, Adopted on 2026-10-30, Items only. Items: its title and subtitle, the standing notice, 15 rows matching 1.18 each "Set by the College.", no Add and the built empty line. Home's card unchanged but "Open Curricula". |
| 1.22, 6.9 | R2-E01, R2-E02 | "Define and maintain …" | Q13's subtitle; two scales (CPSA Paediatric Entrustment Scale v11.1 6, O-R Scale 5), then three (CNSA Neurology Entrustment Scale 6 added); no Create scale, no actions column; stacked at 390, nothing sideways. |
| 2.44 | — | "Open Curriculum adoptions" | The Curriculum card's footer "Open Curricula"; the rest unchanged. |
| 6.13, 6.29, 6.31 (Kruger) | R2-A12 | Status "Inactive" / "Active" | Curricula's Status reads "Held back" / "Open" (T9); the College's box is flow 16's; the rest unchanged. |
| 6.14, 6.23, 6.28 | R2-C08 (shape) | — | Unchanged (the page is titled by the EPA; no Expect quotes "Edit EPA"). |
| 6.17 | — | "Deactivate this EPA?" … "for every institution that uses it" | "Deactivate PAED-012?", its body as built with "for every institution that has adopted it" (Q11). |
| 6.22 | R2-C03, R2-B02 | Edit on national rows | PAED-012 "Inactive" in plain words, "Set by the College." on every row, no Edit, Create EPA offered; its item "(inactive: not in force)" under the notice; the STARs unchanged (flow 09's). |
| 6.25 | R2-C04, C08, C09 | "Edit EPA", Back to EPAs | Create EPA from the header; "Create EPA" with the preset Paediatrics / Paediatrics and its help; Save opens /admin/epas/17, "KGK-001 — Running a paediatric outreach clinic at a district hospital", crumb KGK-001, no Back to EPAs, "EPA saved." focused, Active; the list puts KGK-001 first with "Your institution's own EPA" under its title and "Edit KGK-001 — Running a paediatric outreach clinic at a district hospital" its only Edit. |
| 6.26 | R2-B03, B04, B06 | The add form below the table | Curricula → Items (11.1) → "Add your institution's own item" → the add page (EPA KGK-001 alone, the suggested scale, Add item off until a rung) → Add item → back on the items page, "Curriculum item added." focused, KGK-001's row last, "Your institution's own item" under its EPA, "1 per academic year" over "No decision cadence", 3a, Direct observation, Edit and Remove…; the built empty line again. Curricula's 11.1 row then reads "15 national · 1 your own". |
| 6.30 (Kruger) | — | "PAED-011's edit row opens beneath its row" | "Edit PAED-011 opens its page"; Save item returns to the list with "Curriculum item saved." (T4); the rest unchanged. |
| 6.32 | R2-A03, R2-A05 | Route /admin/adoptions; "Curriculum adopted." at once | Route /admin/curricula. Do: "Under Open for adoption, press Adopt… on Paediatric EPA Curriculum (11.2), read the question and confirm Adopt 11.2." Expect: Open for adoption lists 11.2 ("Newer than the version your institution has adopted (11.1).") and IM Core, not Neonatology (held back); the dialog's words, its first focus on Cancel, "The 3 registrars on 11.1 stay on it…"; the superseding result focused; 11.2 Adopted on 2026-10-30, 11.1 Superseded on 2026-10-30 over "3 registrars still on it"; 11.1 offered again under Open for adoption with "Older than…". |
| 6.33 | R2-A06a, R2-B07, B08 | Curricula's Status "Active" for 11.1 | Curricula's two rows with their states (11.2 "16 national · 1 your own", 11.1 "3 registrars still on it"); 11.2's items: 16 rows "Set by the College.", PAED-016 and PAED-011's "2 per academic year" among them, the new-version line, the action; the add as 6.26 on 11.2; KGK-001's row marked. |
| 6.34 | R2-D01 | — | Anele Dlamini: the line under the picker ("… Keeping it keeps Anele Dlamini on 11.1."); Save profile asks nothing; "Trainee profile saved."; the rest unchanged. |
| 6.35, 6.37, 6.39 | R2-G01 | — | Unchanged (flow 05's; KGK-001's badge is T357's). |
| 6.36 | R2-D02 to D05 | No question; "Save profile" | Sipho Ndlovu. Do: "Choose Paediatric EPA Curriculum (11.2), keep the dates, press Save profile and move…, read the question and confirm Move and save." Expect: on choosing 11.2 the words under the picker change and the submit reads "Save profile and move…"; the dialog "Move Sipho Ndlovu to Paediatric EPA Curriculum (11.2)?", first focus Cancel; the result as built, focused; reopened, 11.2 alone with "The version your institution has adopted for Paediatrics / Paediatrics."; SQL unchanged. |
| 6.38 | R2-F01 to F04 | — | Unchanged but Curricula's Status words (T9) and its two h2 sections; the trigger stays primary. |
| A.5.2, A.7.9 | — | — | Expects unchanged; their Actuals' menu lists lose Curriculum adoptions. |
| A.7.10 (Kruger at 390) | R2-B*-390 | "The EPA list and the items table scroll inside their containers" | "The EPA list, Curricula and the items table stack, a block per row, each action 44 px; Edit PAED-001 opens the item's own page, its fields one a row; nothing scrolls sideways (T323)". |
#### 3 · The runbook's words that change (BRIEF.md § 9 item 7), by this message's search
- "Curriculum adoptions": 1.10 (act-1-setup.md:286, :291), 1.19 (:500), 1.20 (:513–514), 2.44 (act-2-onboarding.md:1182, :1195), 6.32 (act-6-catalogue.md:778), A.5.2 and A.7.9 (appendix-cross-cutting.md:997, :1798).
- "Back to curricula", "Back to curriculum": 1.18 (act-1-setup.md:441, :452). "Curriculum items": 1.18's Actual (:451).
- "Define and maintain": 1.22's Gap (:566–567). "Curriculum adopted." and "already adopted": 1.20, 6.32 (act-1-setup.md:521–528, act-6-catalogue.md:788).
- "Deactivate this EPA", "every institution that uses it": 6.17 (act-6-catalogue.md:407–421). "Create the first EPA": 1.19's Gap (act-1-setup.md:504–508). "Adopt a curriculum", "Current adoptions": 1.20, 6.32.
- No hit: "Edit EPA", "Back to EPAs", "Manage curriculum versions", "Maintain the entrustable", "Remove this item", "The rebuild failed".
This board did not search the runbook itself (it is not in the connected folder); the hits are the message's.

######## BOARD R2-A01.dc.html
R2-A01 · 1280
1.19 · Nolwazi Mbatha · Curricula, nothing adopted: no Create curriculum; Adopt asks nothing (a first adoption supersedes nothing)
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] [aria-current="page"] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
### Curricula
The College curricula Kgosi Kgari Teaching Hospital trains against, and the versions open to it.
#### Curricula you train against
No curricula yet
Your institution has not adopted a curriculum yet. Adopt one below before admitting trainees.
#### Open for adoption
Adopting a version makes it the one that new admissions to its discipline join. Adopting another version of a discipline your institution has adopted supersedes the current one: registrars already admitted stay on theirs until each is moved, and your institution's own items are not carried over.
College curriculum versions open for adoption
| Curriculum | College | Discipline | Effective | Items | Actions |
| Paediatric EPA Curriculum (11.1)Your institution has adopted no Paediatrics / Paediatrics version yet. | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-01-01 | 15 | [button] [aria-label="Adopt Paediatric EPA Curriculum (11.1)"] Adopt |
| IM Core Curriculum (2026.1)Your institution has adopted no General Medicine / General Internal Medicine version yet. | Demo College | General Medicine / General Internal Medicine | 2026-01-01 | 1 | [button] [aria-label="Adopt IM Core Curriculum (2026.1)"] Adopt |

######## BOARD R2-A01-390.dc.html
R2-A01-390 · 390
1.19 · Nolwazi Mbatha · Curricula, nothing adopted: no Create curriculum; Adopt asks nothing (a first adoption supersedes nothing)
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] [aria-current="page"] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
### Curricula
The College curricula Kgosi Kgari Teaching Hospital trains against, and the versions open to it.
#### Curricula you train against
No curricula yet
Your institution has not adopted a curriculum yet. Adopt one below before admitting trainees.
#### Open for adoption
Adopting a version makes it the one that new admissions to its discipline join. Adopting another version of a discipline your institution has adopted supersedes the current one: registrars already admitted stay on theirs until each is moved, and your institution's own items are not carried over.
College curriculum versions open for adoption
| Curriculum | College | Discipline | Effective | Items | Actions |
| Paediatric EPA Curriculum (11.1)Your institution has adopted no Paediatrics / Paediatrics version yet. | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-01-01 | 15 | [button] [aria-label="Adopt Paediatric EPA Curriculum (11.1)"] Adopt |
| IM Core Curriculum (2026.1)Your institution has adopted no General Medicine / General Internal Medicine version yet. | Demo College | General Medicine / General Internal Medicine | 2026-01-01 | 1 | [button] [aria-label="Adopt IM Core Curriculum (2026.1)"] Adopt |

######## BOARD R2-A01-768.dc.html
R2-A01-768 · 768
1.19 · Nolwazi Mbatha · Curricula, nothing adopted: no Create curriculum; Adopt asks nothing (a first adoption supersedes nothing)
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] [aria-current="page"] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
### Curricula
The College curricula Kgosi Kgari Teaching Hospital trains against, and the versions open to it.
#### Curricula you train against
No curricula yet
Your institution has not adopted a curriculum yet. Adopt one below before admitting trainees.
#### Open for adoption
Adopting a version makes it the one that new admissions to its discipline join. Adopting another version of a discipline your institution has adopted supersedes the current one: registrars already admitted stay on theirs until each is moved, and your institution's own items are not carried over.
College curriculum versions open for adoption
| Curriculum | College | Discipline | Effective | Items | Actions |
| Paediatric EPA Curriculum (11.1)Your institution has adopted no Paediatrics / Paediatrics version yet. | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-01-01 | 15 | [button] [aria-label="Adopt Paediatric EPA Curriculum (11.1)"] Adopt |
| IM Core Curriculum (2026.1)Your institution has adopted no General Medicine / General Internal Medicine version yet. | Demo College | General Medicine / General Internal Medicine | 2026-01-01 | 1 | [button] [aria-label="Adopt IM Core Curriculum (2026.1)"] Adopt |

######## BOARD R2-A02.dc.html
R2-A02 · 1280
1.20 · Nolwazi Mbatha · after Adopt on 11.1: 11.1 is a row with its state, not offered again (Q7); journey 2 + 0 (390: 3 + 0)
Focus: the result, at the page's top
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] [aria-current="page"] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
### Curricula
The College curricula Kgosi Kgari Teaching Hospital trains against, and the versions open to it.
Paediatric EPA Curriculum (11.1) adopted. New admissions to Paediatrics / Paediatrics join it.
#### Curricula you train against
The curricula your institution trains against, with the state of each
| Curriculum | College | Discipline | Effective | Items | For your institution | Actions |
| Paediatric EPA Curriculum (11.1) | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-01-01 | 15 national · 0 your own | Adopted on 2026-10-30 | [link] [aria-label="Items of Paediatric EPA Curriculum (11.1)"] Items |
#### Open for adoption
Adopting a version makes it the one that new admissions to its discipline join. Adopting another version of a discipline your institution has adopted supersedes the current one: registrars already admitted stay on theirs until each is moved, and your institution's own items are not carried over.
College curriculum versions open for adoption
| Curriculum | College | Discipline | Effective | Items | Actions |
| IM Core Curriculum (2026.1)Your institution has adopted no General Medicine / General Internal Medicine version yet. | Demo College | General Medicine / General Internal Medicine | 2026-01-01 | 1 | [button] [aria-label="Adopt IM Core Curriculum (2026.1)"] Adopt |

######## BOARD R2-A02-390.dc.html
R2-A02-390 · 390
1.20 · Nolwazi Mbatha · after Adopt on 11.1: 11.1 is a row with its state, not offered again (Q7); journey 2 + 0 (390: 3 + 0)
Focus: the result, at the page's top
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] [aria-current="page"] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
### Curricula
The College curricula Kgosi Kgari Teaching Hospital trains against, and the versions open to it.
Paediatric EPA Curriculum (11.1) adopted. New admissions to Paediatrics / Paediatrics join it.
#### Curricula you train against
The curricula your institution trains against, with the state of each
| Curriculum | College | Discipline | Effective | Items | For your institution | Actions |
| Paediatric EPA Curriculum (11.1) | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-01-01 | 15 national · 0 your own | Adopted on 2026-10-30 | [link] [aria-label="Items of Paediatric EPA Curriculum (11.1)"] Items |
#### Open for adoption
Adopting a version makes it the one that new admissions to its discipline join. Adopting another version of a discipline your institution has adopted supersedes the current one: registrars already admitted stay on theirs until each is moved, and your institution's own items are not carried over.
College curriculum versions open for adoption
| Curriculum | College | Discipline | Effective | Items | Actions |
| IM Core Curriculum (2026.1)Your institution has adopted no General Medicine / General Internal Medicine version yet. | Demo College | General Medicine / General Internal Medicine | 2026-01-01 | 1 | [button] [aria-label="Adopt IM Core Curriculum (2026.1)"] Adopt |

######## BOARD R2-A02-768.dc.html
R2-A02-768 · 768
1.20 · Nolwazi Mbatha · after Adopt on 11.1: 11.1 is a row with its state, not offered again (Q7); journey 2 + 0 (390: 3 + 0)
Focus: the result, at the page's top
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] [aria-current="page"] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
### Curricula
The College curricula Kgosi Kgari Teaching Hospital trains against, and the versions open to it.
Paediatric EPA Curriculum (11.1) adopted. New admissions to Paediatrics / Paediatrics join it.
#### Curricula you train against
The curricula your institution trains against, with the state of each
| Curriculum | College | Discipline | Effective | Items | For your institution | Actions |
| Paediatric EPA Curriculum (11.1) | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-01-01 | 15 national · 0 your own | Adopted on 2026-10-30 | [link] [aria-label="Items of Paediatric EPA Curriculum (11.1)"] Items |
#### Open for adoption
Adopting a version makes it the one that new admissions to its discipline join. Adopting another version of a discipline your institution has adopted supersedes the current one: registrars already admitted stay on theirs until each is moved, and your institution's own items are not carried over.
College curriculum versions open for adoption
| Curriculum | College | Discipline | Effective | Items | Actions |
| IM Core Curriculum (2026.1)Your institution has adopted no General Medicine / General Internal Medicine version yet. | Demo College | General Medicine / General Internal Medicine | 2026-01-01 | 1 | [button] [aria-label="Adopt IM Core Curriculum (2026.1)"] Adopt |

######## BOARD R2-A03.dc.html
R2-A03 · 1280
6.32 · Nolwazi Mbatha · Adopt… on 11.2 asks first (Q6); its body scrolls inside the dialog at 390
Focus: Cancel (the dialog opens on it); Escape or Cancel returns to "Adopt Paediatric EPA Curriculum (11.2)…"
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] [aria-current="page"] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
### Curricula
The College curricula Kgosi Kgari Teaching Hospital trains against, and the versions open to it.
#### Curricula you train against
The curricula your institution trains against, with the state of each
| Curriculum | College | Discipline | Effective | Items | For your institution | Actions |
| Paediatric EPA Curriculum (11.1) | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-01-01 | 15 national · 1 your own | Adopted on 2026-10-30 | [link] [aria-label="Items of Paediatric EPA Curriculum (11.1)"] Items |
#### Open for adoption
Adopting a version makes it the one that new admissions to its discipline join. Adopting another version of a discipline your institution has adopted supersedes the current one: registrars already admitted stay on theirs until each is moved, and your institution's own items are not carried over.
College curriculum versions open for adoption
| Curriculum | College | Discipline | Effective | Items | Actions |
| Paediatric EPA Curriculum (11.2)Newer than the version your institution has adopted (11.1). | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-10-30 | 16 | [button] [aria-label="Adopt Paediatric EPA Curriculum (11.2)…"] Adopt… |
| IM Core Curriculum (2026.1)Your institution has adopted no General Medicine / General Internal Medicine version yet. | Demo College | General Medicine / General Internal Medicine | 2026-01-01 | 1 | [button] [aria-label="Adopt IM Core Curriculum (2026.1)"] Adopt |
#### Adopt Paediatric EPA Curriculum (11.2) for Kgosi Kgari Teaching Hospital?
It supersedes Paediatric EPA Curriculum (11.1) for every new admission to Paediatrics / Paediatrics at Kgosi Kgari Teaching Hospital.
The 3 registrars on 11.1 stay on it until each is moved on their profile.
Your institution's own item on 11.1, KGK-001, is not carried over: add it again on 11.2.
[button] Cancel
[button] Adopt 11.2

######## BOARD R2-A03-390.dc.html
R2-A03-390 · 390
6.32 · Nolwazi Mbatha · Adopt… on 11.2 asks first (Q6); its body scrolls inside the dialog at 390
Focus: Cancel (the dialog opens on it); Escape or Cancel returns to "Adopt Paediatric EPA Curriculum (11.2)…"
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] [aria-current="page"] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
### Curricula
The College curricula Kgosi Kgari Teaching Hospital trains against, and the versions open to it.
#### Curricula you train against
The curricula your institution trains against, with the state of each
| Curriculum | College | Discipline | Effective | Items | For your institution | Actions |
| Paediatric EPA Curriculum (11.1) | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-01-01 | 15 national · 1 your own | Adopted on 2026-10-30 | [link] [aria-label="Items of Paediatric EPA Curriculum (11.1)"] Items |
#### Open for adoption
Adopting a version makes it the one that new admissions to its discipline join. Adopting another version of a discipline your institution has adopted supersedes the current one: registrars already admitted stay on theirs until each is moved, and your institution's own items are not carried over.
College curriculum versions open for adoption
| Curriculum | College | Discipline | Effective | Items | Actions |
| Paediatric EPA Curriculum (11.2)Newer than the version your institution has adopted (11.1). | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-10-30 | 16 | [button] [aria-label="Adopt Paediatric EPA Curriculum (11.2)…"] Adopt… |
| IM Core Curriculum (2026.1)Your institution has adopted no General Medicine / General Internal Medicine version yet. | Demo College | General Medicine / General Internal Medicine | 2026-01-01 | 1 | [button] [aria-label="Adopt IM Core Curriculum (2026.1)"] Adopt |
#### Adopt Paediatric EPA Curriculum (11.2) for Kgosi Kgari Teaching Hospital?
It supersedes Paediatric EPA Curriculum (11.1) for every new admission to Paediatrics / Paediatrics at Kgosi Kgari Teaching Hospital.
The 3 registrars on 11.1 stay on it until each is moved on their profile.
Your institution's own item on 11.1, KGK-001, is not carried over: add it again on 11.2.
[button] Cancel
[button] Adopt 11.2

######## BOARD R2-A04.dc.html
R2-A04 · 1280
6.32 · Nolwazi Mbatha · Cancel pressed: the dialog closed, nothing adopted
Focus: back on 11.2's Adopt… (the trigger)
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] [aria-current="page"] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
### Curricula
The College curricula Kgosi Kgari Teaching Hospital trains against, and the versions open to it.
#### Curricula you train against
The curricula your institution trains against, with the state of each
| Curriculum | College | Discipline | Effective | Items | For your institution | Actions |
| Paediatric EPA Curriculum (11.1) | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-01-01 | 15 national · 1 your own | Adopted on 2026-10-30 | [link] [aria-label="Items of Paediatric EPA Curriculum (11.1)"] Items |
#### Open for adoption
Adopting a version makes it the one that new admissions to its discipline join. Adopting another version of a discipline your institution has adopted supersedes the current one: registrars already admitted stay on theirs until each is moved, and your institution's own items are not carried over.
College curriculum versions open for adoption
| Curriculum | College | Discipline | Effective | Items | Actions |
| Paediatric EPA Curriculum (11.2)Newer than the version your institution has adopted (11.1). | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-10-30 | 16 | [button] [aria-label="Adopt Paediatric EPA Curriculum (11.2)…"] Adopt… |
| IM Core Curriculum (2026.1)Your institution has adopted no General Medicine / General Internal Medicine version yet. | Demo College | General Medicine / General Internal Medicine | 2026-01-01 | 1 | [button] [aria-label="Adopt IM Core Curriculum (2026.1)"] Adopt |

######## BOARD R2-A04-390.dc.html
R2-A04-390 · 390
6.32 · Nolwazi Mbatha · Cancel pressed: the dialog closed, nothing adopted
Focus: back on 11.2's Adopt… (the trigger)
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] [aria-current="page"] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
### Curricula
The College curricula Kgosi Kgari Teaching Hospital trains against, and the versions open to it.
#### Curricula you train against
The curricula your institution trains against, with the state of each
| Curriculum | College | Discipline | Effective | Items | For your institution | Actions |
| Paediatric EPA Curriculum (11.1) | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-01-01 | 15 national · 1 your own | Adopted on 2026-10-30 | [link] [aria-label="Items of Paediatric EPA Curriculum (11.1)"] Items |
#### Open for adoption
Adopting a version makes it the one that new admissions to its discipline join. Adopting another version of a discipline your institution has adopted supersedes the current one: registrars already admitted stay on theirs until each is moved, and your institution's own items are not carried over.
College curriculum versions open for adoption
| Curriculum | College | Discipline | Effective | Items | Actions |
| Paediatric EPA Curriculum (11.2)Newer than the version your institution has adopted (11.1). | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-10-30 | 16 | [button] [aria-label="Adopt Paediatric EPA Curriculum (11.2)…"] Adopt… |
| IM Core Curriculum (2026.1)Your institution has adopted no General Medicine / General Internal Medicine version yet. | Demo College | General Medicine / General Internal Medicine | 2026-01-01 | 1 | [button] [aria-label="Adopt IM Core Curriculum (2026.1)"] Adopt |

######## BOARD R2-A05.dc.html
R2-A05 · 1280
6.32 · Nolwazi Mbatha · Adopt 11.2 confirmed: 11.2 adopted, 11.1 superseded with 3 registrars (S2), 11.1 open again with "Older than…" (T1); journey 3 + 0 (390: 4 + 0)
Focus: the superseding result, at the page's top
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] [aria-current="page"] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
### Curricula
The College curricula Kgosi Kgari Teaching Hospital trains against, and the versions open to it.
Paediatric EPA Curriculum (11.2) adopted. New admissions to Paediatrics / Paediatrics join it. Paediatric EPA Curriculum (11.1) is superseded; its 3 registrars stay on it until each is moved.
#### Curricula you train against
The curricula your institution trains against, with the state of each
| Curriculum | College | Discipline | Effective | Items | For your institution | Actions |
| Paediatric EPA Curriculum (11.2) | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-10-30 | 16 national · 0 your own | Adopted on 2026-10-30 | [link] [aria-label="Items of Paediatric EPA Curriculum (11.2)"] Items |
| Paediatric EPA Curriculum (11.1) | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-01-01 | 15 national · 1 your own | Superseded on 2026-10-303 registrars still on it | [link] [aria-label="Items of Paediatric EPA Curriculum (11.1)"] Items |
#### Open for adoption
Adopting a version makes it the one that new admissions to its discipline join. Adopting another version of a discipline your institution has adopted supersedes the current one: registrars already admitted stay on theirs until each is moved, and your institution's own items are not carried over.
College curriculum versions open for adoption
| Curriculum | College | Discipline | Effective | Items | Actions |
| Paediatric EPA Curriculum (11.1)Older than the version your institution has adopted (11.2). | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-01-01 | 15 | [button] [aria-label="Adopt Paediatric EPA Curriculum (11.1)…"] Adopt… |
| IM Core Curriculum (2026.1)Your institution has adopted no General Medicine / General Internal Medicine version yet. | Demo College | General Medicine / General Internal Medicine | 2026-01-01 | 1 | [button] [aria-label="Adopt IM Core Curriculum (2026.1)"] Adopt |

######## BOARD R2-A05-390.dc.html
R2-A05-390 · 390
6.32 · Nolwazi Mbatha · Adopt 11.2 confirmed: 11.2 adopted, 11.1 superseded with 3 registrars (S2), 11.1 open again with "Older than…" (T1); journey 3 + 0 (390: 4 + 0)
Focus: the superseding result, at the page's top
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] [aria-current="page"] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
### Curricula
The College curricula Kgosi Kgari Teaching Hospital trains against, and the versions open to it.
Paediatric EPA Curriculum (11.2) adopted. New admissions to Paediatrics / Paediatrics join it. Paediatric EPA Curriculum (11.1) is superseded; its 3 registrars stay on it until each is moved.
#### Curricula you train against
The curricula your institution trains against, with the state of each
| Curriculum | College | Discipline | Effective | Items | For your institution | Actions |
| Paediatric EPA Curriculum (11.2) | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-10-30 | 16 national · 0 your own | Adopted on 2026-10-30 | [link] [aria-label="Items of Paediatric EPA Curriculum (11.2)"] Items |
| Paediatric EPA Curriculum (11.1) | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-01-01 | 15 national · 1 your own | Superseded on 2026-10-303 registrars still on it | [link] [aria-label="Items of Paediatric EPA Curriculum (11.1)"] Items |
#### Open for adoption
Adopting a version makes it the one that new admissions to its discipline join. Adopting another version of a discipline your institution has adopted supersedes the current one: registrars already admitted stay on theirs until each is moved, and your institution's own items are not carried over.
College curriculum versions open for adoption
| Curriculum | College | Discipline | Effective | Items | Actions |
| Paediatric EPA Curriculum (11.1)Older than the version your institution has adopted (11.2). | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-01-01 | 15 | [button] [aria-label="Adopt Paediatric EPA Curriculum (11.1)…"] Adopt… |
| IM Core Curriculum (2026.1)Your institution has adopted no General Medicine / General Internal Medicine version yet. | Demo College | General Medicine / General Internal Medicine | 2026-01-01 | 1 | [button] [aria-label="Adopt IM Core Curriculum (2026.1)"] Adopt |

######## BOARD R2-A05-768.dc.html
R2-A05-768 · 768
6.32 · Nolwazi Mbatha · Adopt 11.2 confirmed: 11.2 adopted, 11.1 superseded with 3 registrars (S2), 11.1 open again with "Older than…" (T1); journey 3 + 0 (390: 4 + 0)
Focus: the superseding result, at the page's top
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] [aria-current="page"] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
### Curricula
The College curricula Kgosi Kgari Teaching Hospital trains against, and the versions open to it.
Paediatric EPA Curriculum (11.2) adopted. New admissions to Paediatrics / Paediatrics join it. Paediatric EPA Curriculum (11.1) is superseded; its 3 registrars stay on it until each is moved.
#### Curricula you train against
The curricula your institution trains against, with the state of each
| Curriculum | College | Discipline | Effective | Items | For your institution | Actions |
| Paediatric EPA Curriculum (11.2) | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-10-30 | 16 national · 0 your own | Adopted on 2026-10-30 | [link] [aria-label="Items of Paediatric EPA Curriculum (11.2)"] Items |
| Paediatric EPA Curriculum (11.1) | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-01-01 | 15 national · 1 your own | Superseded on 2026-10-303 registrars still on it | [link] [aria-label="Items of Paediatric EPA Curriculum (11.1)"] Items |
#### Open for adoption
Adopting a version makes it the one that new admissions to its discipline join. Adopting another version of a discipline your institution has adopted supersedes the current one: registrars already admitted stay on theirs until each is moved, and your institution's own items are not carried over.
College curriculum versions open for adoption
| Curriculum | College | Discipline | Effective | Items | Actions |
| Paediatric EPA Curriculum (11.1)Older than the version your institution has adopted (11.2). | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-01-01 | 15 | [button] [aria-label="Adopt Paediatric EPA Curriculum (11.1)…"] Adopt… |
| IM Core Curriculum (2026.1)Your institution has adopted no General Medicine / General Internal Medicine version yet. | Demo College | General Medicine / General Internal Medicine | 2026-01-01 | 1 | [button] [aria-label="Adopt IM Core Curriculum (2026.1)"] Adopt |

######## BOARD R2-A06a.dc.html
R2-A06a · 1280
6.33 · Nolwazi Mbatha · after KGK-001 was added on 11.2: "16 national · 1 your own"; 11.1 still 3 registrars
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] [aria-current="page"] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
### Curricula
The College curricula Kgosi Kgari Teaching Hospital trains against, and the versions open to it.
#### Curricula you train against
The curricula your institution trains against, with the state of each
| Curriculum | College | Discipline | Effective | Items | For your institution | Actions |
| Paediatric EPA Curriculum (11.2) | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-10-30 | 16 national · 1 your own | Adopted on 2026-10-30 | [link] [aria-label="Items of Paediatric EPA Curriculum (11.2)"] Items |
| Paediatric EPA Curriculum (11.1) | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-01-01 | 15 national · 1 your own | Superseded on 2026-10-303 registrars still on it | [link] [aria-label="Items of Paediatric EPA Curriculum (11.1)"] Items |
#### Open for adoption
Adopting a version makes it the one that new admissions to its discipline join. Adopting another version of a discipline your institution has adopted supersedes the current one: registrars already admitted stay on theirs until each is moved, and your institution's own items are not carried over.
College curriculum versions open for adoption
| Curriculum | College | Discipline | Effective | Items | Actions |
| Paediatric EPA Curriculum (11.1)Older than the version your institution has adopted (11.2). | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-01-01 | 15 | [button] [aria-label="Adopt Paediatric EPA Curriculum (11.1)…"] Adopt… |
| IM Core Curriculum (2026.1)Your institution has adopted no General Medicine / General Internal Medicine version yet. | Demo College | General Medicine / General Internal Medicine | 2026-01-01 | 1 | [button] [aria-label="Adopt IM Core Curriculum (2026.1)"] Adopt |

######## BOARD R2-A06a-390.dc.html
R2-A06a-390 · 390
6.33 · Nolwazi Mbatha · after KGK-001 was added on 11.2: "16 national · 1 your own"; 11.1 still 3 registrars
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] [aria-current="page"] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
### Curricula
The College curricula Kgosi Kgari Teaching Hospital trains against, and the versions open to it.
#### Curricula you train against
The curricula your institution trains against, with the state of each
| Curriculum | College | Discipline | Effective | Items | For your institution | Actions |
| Paediatric EPA Curriculum (11.2) | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-10-30 | 16 national · 1 your own | Adopted on 2026-10-30 | [link] [aria-label="Items of Paediatric EPA Curriculum (11.2)"] Items |
| Paediatric EPA Curriculum (11.1) | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-01-01 | 15 national · 1 your own | Superseded on 2026-10-303 registrars still on it | [link] [aria-label="Items of Paediatric EPA Curriculum (11.1)"] Items |
#### Open for adoption
Adopting a version makes it the one that new admissions to its discipline join. Adopting another version of a discipline your institution has adopted supersedes the current one: registrars already admitted stay on theirs until each is moved, and your institution's own items are not carried over.
College curriculum versions open for adoption
| Curriculum | College | Discipline | Effective | Items | Actions |
| Paediatric EPA Curriculum (11.1)Older than the version your institution has adopted (11.2). | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-01-01 | 15 | [button] [aria-label="Adopt Paediatric EPA Curriculum (11.1)…"] Adopt… |
| IM Core Curriculum (2026.1)Your institution has adopted no General Medicine / General Internal Medicine version yet. | Demo College | General Medicine / General Internal Medicine | 2026-01-01 | 1 | [button] [aria-label="Adopt IM Core Curriculum (2026.1)"] Adopt |

######## BOARD R2-A06b.dc.html
R2-A06b · 1280
after 6.36 · Nolwazi Mbatha · Sipho Ndlovu moved: 11.1 "2 registrars still on it" (active profiles only; Lerato Molefe's ended profile keeps 11.1 listed, not counted)
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] [aria-current="page"] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
### Curricula
The College curricula Kgosi Kgari Teaching Hospital trains against, and the versions open to it.
#### Curricula you train against
The curricula your institution trains against, with the state of each
| Curriculum | College | Discipline | Effective | Items | For your institution | Actions |
| Paediatric EPA Curriculum (11.2) | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-10-30 | 16 national · 1 your own | Adopted on 2026-10-30 | [link] [aria-label="Items of Paediatric EPA Curriculum (11.2)"] Items |
| Paediatric EPA Curriculum (11.1) | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-01-01 | 15 national · 1 your own | Superseded on 2026-10-302 registrars still on it | [link] [aria-label="Items of Paediatric EPA Curriculum (11.1)"] Items |
#### Open for adoption
Adopting a version makes it the one that new admissions to its discipline join. Adopting another version of a discipline your institution has adopted supersedes the current one: registrars already admitted stay on theirs until each is moved, and your institution's own items are not carried over.
College curriculum versions open for adoption
| Curriculum | College | Discipline | Effective | Items | Actions |
| Paediatric EPA Curriculum (11.1)Older than the version your institution has adopted (11.2). | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-01-01 | 15 | [button] [aria-label="Adopt Paediatric EPA Curriculum (11.1)…"] Adopt… |
| IM Core Curriculum (2026.1)Your institution has adopted no General Medicine / General Internal Medicine version yet. | Demo College | General Medicine / General Internal Medicine | 2026-01-01 | 1 | [button] [aria-label="Adopt IM Core Curriculum (2026.1)"] Adopt |

######## BOARD R2-A06b-390.dc.html
R2-A06b-390 · 390
after 6.36 · Nolwazi Mbatha · Sipho Ndlovu moved: 11.1 "2 registrars still on it" (active profiles only; Lerato Molefe's ended profile keeps 11.1 listed, not counted)
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] [aria-current="page"] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
### Curricula
The College curricula Kgosi Kgari Teaching Hospital trains against, and the versions open to it.
#### Curricula you train against
The curricula your institution trains against, with the state of each
| Curriculum | College | Discipline | Effective | Items | For your institution | Actions |
| Paediatric EPA Curriculum (11.2) | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-10-30 | 16 national · 1 your own | Adopted on 2026-10-30 | [link] [aria-label="Items of Paediatric EPA Curriculum (11.2)"] Items |
| Paediatric EPA Curriculum (11.1) | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-01-01 | 15 national · 1 your own | Superseded on 2026-10-302 registrars still on it | [link] [aria-label="Items of Paediatric EPA Curriculum (11.1)"] Items |
#### Open for adoption
Adopting a version makes it the one that new admissions to its discipline join. Adopting another version of a discipline your institution has adopted supersedes the current one: registrars already admitted stay on theirs until each is moved, and your institution's own items are not carried over.
College curriculum versions open for adoption
| Curriculum | College | Discipline | Effective | Items | Actions |
| Paediatric EPA Curriculum (11.1)Older than the version your institution has adopted (11.2). | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-01-01 | 15 | [button] [aria-label="Adopt Paediatric EPA Curriculum (11.1)…"] Adopt… |
| IM Core Curriculum (2026.1)Your institution has adopted no General Medicine / General Internal Medicine version yet. | Demo College | General Medicine / General Internal Medicine | 2026-01-01 | 1 | [button] [aria-label="Adopt IM Core Curriculum (2026.1)"] Adopt |

######## BOARD R2-A07.dc.html
IllustrationR2-A07 · 1280
Illustration · between 6.13 and 6.25 · a stale page offered Neonatology EPA Curriculum (1.0), held back at 6.13; Adopt pressed; the list read again (T301, Q5)
Focus: the refusal, above "Open for adoption"
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] [aria-current="page"] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
### Curricula
The College curricula Kgosi Kgari Teaching Hospital trains against, and the versions open to it.
#### Curricula you train against
The curricula your institution trains against, with the state of each
| Curriculum | College | Discipline | Effective | Items | For your institution | Actions |
| Paediatric EPA Curriculum (11.1) | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-01-01 | 15 national · 0 your own | Adopted on 2026-10-30 | [link] [aria-label="Items of Paediatric EPA Curriculum (11.1)"] Items |
#### Open for adoption
This curriculum version is not open for adoption. Its College has held it back, so nothing was adopted.
Adopting a version makes it the one that new admissions to its discipline join. Adopting another version of a discipline your institution has adopted supersedes the current one: registrars already admitted stay on theirs until each is moved, and your institution's own items are not carried over.
College curriculum versions open for adoption
| Curriculum | College | Discipline | Effective | Items | Actions |
| IM Core Curriculum (2026.1)Your institution has adopted no General Medicine / General Internal Medicine version yet. | Demo College | General Medicine / General Internal Medicine | 2026-01-01 | 1 | [button] [aria-label="Adopt IM Core Curriculum (2026.1)"] Adopt |

######## BOARD R2-A07-390.dc.html
IllustrationR2-A07-390 · 390
Illustration · between 6.13 and 6.25 · a stale page offered Neonatology EPA Curriculum (1.0), held back at 6.13; Adopt pressed; the list read again (T301, Q5)
Focus: the refusal, above "Open for adoption"
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] [aria-current="page"] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
### Curricula
The College curricula Kgosi Kgari Teaching Hospital trains against, and the versions open to it.
#### Curricula you train against
The curricula your institution trains against, with the state of each
| Curriculum | College | Discipline | Effective | Items | For your institution | Actions |
| Paediatric EPA Curriculum (11.1) | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-01-01 | 15 national · 0 your own | Adopted on 2026-10-30 | [link] [aria-label="Items of Paediatric EPA Curriculum (11.1)"] Items |
#### Open for adoption
This curriculum version is not open for adoption. Its College has held it back, so nothing was adopted.
Adopting a version makes it the one that new admissions to its discipline join. Adopting another version of a discipline your institution has adopted supersedes the current one: registrars already admitted stay on theirs until each is moved, and your institution's own items are not carried over.
College curriculum versions open for adoption
| Curriculum | College | Discipline | Effective | Items | Actions |
| IM Core Curriculum (2026.1)Your institution has adopted no General Medicine / General Internal Medicine version yet. | Demo College | General Medicine / General Internal Medicine | 2026-01-01 | 1 | [button] [aria-label="Adopt IM Core Curriculum (2026.1)"] Adopt |

######## BOARD R2-A08.dc.html
IllustrationR2-A08 · 1280
Illustration · after 6.36 · going back (Q7, M2, T1): Adopt… on 11.1 under "Open for adoption"; its dialog says it supersedes 11.2
Focus: Cancel
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] [aria-current="page"] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
### Curricula
The College curricula Kgosi Kgari Teaching Hospital trains against, and the versions open to it.
#### Curricula you train against
The curricula your institution trains against, with the state of each
| Curriculum | College | Discipline | Effective | Items | For your institution | Actions |
| Paediatric EPA Curriculum (11.2) | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-10-30 | 16 national · 1 your own | Adopted on 2026-10-30 | [link] [aria-label="Items of Paediatric EPA Curriculum (11.2)"] Items |
| Paediatric EPA Curriculum (11.1) | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-01-01 | 15 national · 1 your own | Superseded on 2026-10-302 registrars still on it | [link] [aria-label="Items of Paediatric EPA Curriculum (11.1)"] Items |
#### Open for adoption
Adopting a version makes it the one that new admissions to its discipline join. Adopting another version of a discipline your institution has adopted supersedes the current one: registrars already admitted stay on theirs until each is moved, and your institution's own items are not carried over.
College curriculum versions open for adoption
| Curriculum | College | Discipline | Effective | Items | Actions |
| Paediatric EPA Curriculum (11.1)Older than the version your institution has adopted (11.2). | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-01-01 | 15 | [button] [aria-label="Adopt Paediatric EPA Curriculum (11.1)…"] Adopt… |
| IM Core Curriculum (2026.1)Your institution has adopted no General Medicine / General Internal Medicine version yet. | Demo College | General Medicine / General Internal Medicine | 2026-01-01 | 1 | [button] [aria-label="Adopt IM Core Curriculum (2026.1)"] Adopt |
#### Adopt Paediatric EPA Curriculum (11.1) for Kgosi Kgari Teaching Hospital?
It supersedes Paediatric EPA Curriculum (11.2) for every new admission to Paediatrics / Paediatrics at Kgosi Kgari Teaching Hospital.
The ‹N› registrars on 11.2 stay on it until each is moved on their profile.
‹the own-items line, DATA›
[button] Cancel
[button] Adopt 11.1

######## BOARD R2-A08-390.dc.html
IllustrationR2-A08-390 · 390
Illustration · after 6.36 · going back (Q7, M2, T1): Adopt… on 11.1 under "Open for adoption"; its dialog says it supersedes 11.2
Focus: Cancel
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] [aria-current="page"] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
### Curricula
The College curricula Kgosi Kgari Teaching Hospital trains against, and the versions open to it.
#### Curricula you train against
The curricula your institution trains against, with the state of each
| Curriculum | College | Discipline | Effective | Items | For your institution | Actions |
| Paediatric EPA Curriculum (11.2) | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-10-30 | 16 national · 1 your own | Adopted on 2026-10-30 | [link] [aria-label="Items of Paediatric EPA Curriculum (11.2)"] Items |
| Paediatric EPA Curriculum (11.1) | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-01-01 | 15 national · 1 your own | Superseded on 2026-10-302 registrars still on it | [link] [aria-label="Items of Paediatric EPA Curriculum (11.1)"] Items |
#### Open for adoption
Adopting a version makes it the one that new admissions to its discipline join. Adopting another version of a discipline your institution has adopted supersedes the current one: registrars already admitted stay on theirs until each is moved, and your institution's own items are not carried over.
College curriculum versions open for adoption
| Curriculum | College | Discipline | Effective | Items | Actions |
| Paediatric EPA Curriculum (11.1)Older than the version your institution has adopted (11.2). | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-01-01 | 15 | [button] [aria-label="Adopt Paediatric EPA Curriculum (11.1)…"] Adopt… |
| IM Core Curriculum (2026.1)Your institution has adopted no General Medicine / General Internal Medicine version yet. | Demo College | General Medicine / General Internal Medicine | 2026-01-01 | 1 | [button] [aria-label="Adopt IM Core Curriculum (2026.1)"] Adopt |
#### Adopt Paediatric EPA Curriculum (11.1) for Kgosi Kgari Teaching Hospital?
It supersedes Paediatric EPA Curriculum (11.2) for every new admission to Paediatrics / Paediatrics at Kgosi Kgari Teaching Hospital.
The ‹N› registrars on 11.2 stay on it until each is moved on their profile.
‹the own-items line, DATA›
[button] Cancel
[button] Adopt 11.1

######## BOARD R2-A09.dc.html
R2-A09 · 1280
Demo Administrator · Curricula: All curricula (Status Open / Held back, T9) and "An institution's curricula" before Show (Q18)
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
### Curricula
Every College's curricula; and, for one institution, what it trains against and may adopt.
[link] Curriculum progress
[link] Create curriculum
#### All curricula
Every College's curriculum versions, with the College's status
| Curriculum | College | Discipline | Effective | Items | Status | Actions |
| Paediatric EPA Curriculum (11.2) | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-10-30 | 17 | Open | [link] [aria-label="Edit Paediatric EPA Curriculum (11.2)"] Edit [link] [aria-label="Items of Paediatric EPA Curriculum (11.2)"] Items |
| Paediatric EPA Curriculum (11.1) | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-01-01 | 16 | Open | [link] [aria-label="Edit Paediatric EPA Curriculum (11.1)"] Edit [link] [aria-label="Items of Paediatric EPA Curriculum (11.1)"] Items |
| Neonatology EPA Curriculum (1.0) | College of Paediatricians of South Africa | Paediatrics / Neonatology | 2026-10-30 | 0 | Held back | [link] [aria-label="Edit Neonatology EPA Curriculum (1.0)"] Edit [link] [aria-label="Items of Neonatology EPA Curriculum (1.0)"] Items |
| IM Core Curriculum (2026.1) | Demo College | General Medicine / General Internal Medicine | 2026-01-01 | 1 | Open | [link] [aria-label="Edit IM Core Curriculum (2026.1)"] Edit [link] [aria-label="Items of IM Core Curriculum (2026.1)"] Items |
#### An institution's curricula
Institution
— Select an institution —
[button] Show
Choose an institution, then Show, to see what it trains against and adopt for it.

######## BOARD R2-A09-390.dc.html
R2-A09-390 · 390
Demo Administrator · Curricula: All curricula (Status Open / Held back, T9) and "An institution's curricula" before Show (Q18)
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
### Curricula
Every College's curricula; and, for one institution, what it trains against and may adopt.
[link] Curriculum progress
[link] Create curriculum
#### All curricula
Every College's curriculum versions, with the College's status
| Curriculum | College | Discipline | Effective | Items | Status | Actions |
| Paediatric EPA Curriculum (11.2) | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-10-30 | 17 | Open | [link] [aria-label="Edit Paediatric EPA Curriculum (11.2)"] Edit [link] [aria-label="Items of Paediatric EPA Curriculum (11.2)"] Items |
| Paediatric EPA Curriculum (11.1) | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-01-01 | 16 | Open | [link] [aria-label="Edit Paediatric EPA Curriculum (11.1)"] Edit [link] [aria-label="Items of Paediatric EPA Curriculum (11.1)"] Items |
| Neonatology EPA Curriculum (1.0) | College of Paediatricians of South Africa | Paediatrics / Neonatology | 2026-10-30 | 0 | Held back | [link] [aria-label="Edit Neonatology EPA Curriculum (1.0)"] Edit [link] [aria-label="Items of Neonatology EPA Curriculum (1.0)"] Items |
| IM Core Curriculum (2026.1) | Demo College | General Medicine / General Internal Medicine | 2026-01-01 | 1 | Open | [link] [aria-label="Edit IM Core Curriculum (2026.1)"] Edit [link] [aria-label="Items of IM Core Curriculum (2026.1)"] Items |
#### An institution's curricula
Institution
— Select an institution —
[button] Show
Choose an institution, then Show, to see what it trains against and adopt for it.

######## BOARD R2-A09-768.dc.html
R2-A09-768 · 768
Demo Administrator · Curricula: All curricula (Status Open / Held back, T9) and "An institution's curricula" before Show (Q18)
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
### Curricula
Every College's curricula; and, for one institution, what it trains against and may adopt.
[link] Curriculum progress
[link] Create curriculum
#### All curricula
Every College's curriculum versions, with the College's status
| Curriculum | College | Discipline | Effective | Items | Status | Actions |
| Paediatric EPA Curriculum (11.2) | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-10-30 | 17 | Open | [link] [aria-label="Edit Paediatric EPA Curriculum (11.2)"] Edit [link] [aria-label="Items of Paediatric EPA Curriculum (11.2)"] Items |
| Paediatric EPA Curriculum (11.1) | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-01-01 | 16 | Open | [link] [aria-label="Edit Paediatric EPA Curriculum (11.1)"] Edit [link] [aria-label="Items of Paediatric EPA Curriculum (11.1)"] Items |
| Neonatology EPA Curriculum (1.0) | College of Paediatricians of South Africa | Paediatrics / Neonatology | 2026-10-30 | 0 | Held back | [link] [aria-label="Edit Neonatology EPA Curriculum (1.0)"] Edit [link] [aria-label="Items of Neonatology EPA Curriculum (1.0)"] Items |
| IM Core Curriculum (2026.1) | Demo College | General Medicine / General Internal Medicine | 2026-01-01 | 1 | Open | [link] [aria-label="Edit IM Core Curriculum (2026.1)"] Edit [link] [aria-label="Items of IM Core Curriculum (2026.1)"] Items |
#### An institution's curricula
Institution
— Select an institution —
[button] Show
Choose an institution, then Show, to see what it trains against and adopt for it.

######## BOARD R2-A10.dc.html
R2-A10 · 1280
Demo Administrator · Kgosi Kgari Teaching Hospital chosen, Show pressed (as at 6.38); journey to adopt for KGK: Curricula, the institution, Show, Adopt…, Adopt 11.2 = 4 + 1
Focus: the first h3, "Curricula Kgosi Kgari Teaching Hospital trains against"
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
### Curricula
Every College's curricula; and, for one institution, what it trains against and may adopt.
[link] Curriculum progress
[link] Create curriculum
#### All curricula
Every College's curriculum versions, with the College's status
| Curriculum | College | Discipline | Effective | Items | Status | Actions |
| Paediatric EPA Curriculum (11.2) | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-10-30 | 17 | Open | [link] [aria-label="Edit Paediatric EPA Curriculum (11.2)"] Edit [link] [aria-label="Items of Paediatric EPA Curriculum (11.2)"] Items |
| Paediatric EPA Curriculum (11.1) | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-01-01 | 16 | Open | [link] [aria-label="Edit Paediatric EPA Curriculum (11.1)"] Edit [link] [aria-label="Items of Paediatric EPA Curriculum (11.1)"] Items |
| Neonatology EPA Curriculum (1.0) | College of Paediatricians of South Africa | Paediatrics / Neonatology | 2026-10-30 | 0 | Held back | [link] [aria-label="Edit Neonatology EPA Curriculum (1.0)"] Edit [link] [aria-label="Items of Neonatology EPA Curriculum (1.0)"] Items |
| IM Core Curriculum (2026.1) | Demo College | General Medicine / General Internal Medicine | 2026-01-01 | 1 | Open | [link] [aria-label="Edit IM Core Curriculum (2026.1)"] Edit [link] [aria-label="Items of IM Core Curriculum (2026.1)"] Items |
#### An institution's curricula
Institution
Kgosi Kgari Teaching Hospital
[button] Show
##### Curricula Kgosi Kgari Teaching Hospital trains against
The curricula Kgosi Kgari Teaching Hospital trains against, with the state of each
| Curriculum | College | Discipline | Effective | Items | For your institution | Actions |
| Paediatric EPA Curriculum (11.2) | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-10-30 | 16 national · 1 Kgosi Kgari Teaching Hospital's own | Adopted on 2026-10-30 | [link] [aria-label="Items of Paediatric EPA Curriculum (11.2)"] Items |
| Paediatric EPA Curriculum (11.1) | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-01-01 | 15 national · 1 Kgosi Kgari Teaching Hospital's own | Superseded on 2026-10-302 registrars still on it | [link] [aria-label="Items of Paediatric EPA Curriculum (11.1)"] Items |
##### Open to Kgosi Kgari Teaching Hospital
Adopting a version makes it the one that new admissions to its discipline join. Adopting another version of a discipline your institution has adopted supersedes the current one: registrars already admitted stay on theirs until each is moved, and your institution's own items are not carried over.
College curriculum versions open for adoption
| Curriculum | College | Discipline | Effective | Items | Actions |
| Paediatric EPA Curriculum (11.1)Older than the version Kgosi Kgari Teaching Hospital has adopted (11.2). | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-01-01 | 15 | [button] [aria-label="Adopt Paediatric EPA Curriculum (11.1) for Kgosi Kgari Teaching Hospital…"] Adopt… |
| IM Core Curriculum (2026.1)Kgosi Kgari Teaching Hospital has adopted no General Medicine / General Internal Medicine version yet. | Demo College | General Medicine / General Internal Medicine | 2026-01-01 | 1 | [button] [aria-label="Adopt IM Core Curriculum (2026.1) for Kgosi Kgari Teaching Hospital"] Adopt |

######## BOARD R2-A10-390.dc.html
R2-A10-390 · 390
Demo Administrator · Kgosi Kgari Teaching Hospital chosen, Show pressed (as at 6.38); journey to adopt for KGK: Curricula, the institution, Show, Adopt…, Adopt 11.2 = 4 + 1
Focus: the first h3, "Curricula Kgosi Kgari Teaching Hospital trains against"
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
### Curricula
Every College's curricula; and, for one institution, what it trains against and may adopt.
[link] Curriculum progress
[link] Create curriculum
#### All curricula
Every College's curriculum versions, with the College's status
| Curriculum | College | Discipline | Effective | Items | Status | Actions |
| Paediatric EPA Curriculum (11.2) | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-10-30 | 17 | Open | [link] [aria-label="Edit Paediatric EPA Curriculum (11.2)"] Edit [link] [aria-label="Items of Paediatric EPA Curriculum (11.2)"] Items |
| Paediatric EPA Curriculum (11.1) | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-01-01 | 16 | Open | [link] [aria-label="Edit Paediatric EPA Curriculum (11.1)"] Edit [link] [aria-label="Items of Paediatric EPA Curriculum (11.1)"] Items |
| Neonatology EPA Curriculum (1.0) | College of Paediatricians of South Africa | Paediatrics / Neonatology | 2026-10-30 | 0 | Held back | [link] [aria-label="Edit Neonatology EPA Curriculum (1.0)"] Edit [link] [aria-label="Items of Neonatology EPA Curriculum (1.0)"] Items |
| IM Core Curriculum (2026.1) | Demo College | General Medicine / General Internal Medicine | 2026-01-01 | 1 | Open | [link] [aria-label="Edit IM Core Curriculum (2026.1)"] Edit [link] [aria-label="Items of IM Core Curriculum (2026.1)"] Items |
#### An institution's curricula
Institution
Kgosi Kgari Teaching Hospital
[button] Show
##### Curricula Kgosi Kgari Teaching Hospital trains against
The curricula Kgosi Kgari Teaching Hospital trains against, with the state of each
| Curriculum | College | Discipline | Effective | Items | For your institution | Actions |
| Paediatric EPA Curriculum (11.2) | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-10-30 | 16 national · 1 Kgosi Kgari Teaching Hospital's own | Adopted on 2026-10-30 | [link] [aria-label="Items of Paediatric EPA Curriculum (11.2)"] Items |
| Paediatric EPA Curriculum (11.1) | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-01-01 | 15 national · 1 Kgosi Kgari Teaching Hospital's own | Superseded on 2026-10-302 registrars still on it | [link] [aria-label="Items of Paediatric EPA Curriculum (11.1)"] Items |
##### Open to Kgosi Kgari Teaching Hospital
Adopting a version makes it the one that new admissions to its discipline join. Adopting another version of a discipline your institution has adopted supersedes the current one: registrars already admitted stay on theirs until each is moved, and your institution's own items are not carried over.
College curriculum versions open for adoption
| Curriculum | College | Discipline | Effective | Items | Actions |
| Paediatric EPA Curriculum (11.1)Older than the version Kgosi Kgari Teaching Hospital has adopted (11.2). | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-01-01 | 15 | [button] [aria-label="Adopt Paediatric EPA Curriculum (11.1) for Kgosi Kgari Teaching Hospital…"] Adopt… |
| IM Core Curriculum (2026.1)Kgosi Kgari Teaching Hospital has adopted no General Medicine / General Internal Medicine version yet. | Demo College | General Medicine / General Internal Medicine | 2026-01-01 | 1 | [button] [aria-label="Adopt IM Core Curriculum (2026.1) for Kgosi Kgari Teaching Hospital"] Adopt |

######## BOARD R2-A11.dc.html
IllustrationR2-A11 · 1280
Illustration · unplayed (coverage.md:198) · Demo Administrator adopting 11.2 for Kgosi Kgari Teaching Hospital at 6.31's state; the dialog names the institution
Focus: Cancel
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
### Curricula
Every College's curricula; and, for one institution, what it trains against and may adopt.
[link] Curriculum progress
[link] Create curriculum
#### All curricula
Every College's curriculum versions, with the College's status
| Curriculum | College | Discipline | Effective | Items | Status | Actions |
| Paediatric EPA Curriculum (11.2) | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-10-30 | 17 | Open | [link] [aria-label="Edit Paediatric EPA Curriculum (11.2)"] Edit [link] [aria-label="Items of Paediatric EPA Curriculum (11.2)"] Items |
| Paediatric EPA Curriculum (11.1) | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-01-01 | 16 | Open | [link] [aria-label="Edit Paediatric EPA Curriculum (11.1)"] Edit [link] [aria-label="Items of Paediatric EPA Curriculum (11.1)"] Items |
| Neonatology EPA Curriculum (1.0) | College of Paediatricians of South Africa | Paediatrics / Neonatology | 2026-10-30 | 0 | Held back | [link] [aria-label="Edit Neonatology EPA Curriculum (1.0)"] Edit [link] [aria-label="Items of Neonatology EPA Curriculum (1.0)"] Items |
| IM Core Curriculum (2026.1) | Demo College | General Medicine / General Internal Medicine | 2026-01-01 | 1 | Open | [link] [aria-label="Edit IM Core Curriculum (2026.1)"] Edit [link] [aria-label="Items of IM Core Curriculum (2026.1)"] Items |
#### An institution's curricula
Institution
Kgosi Kgari Teaching Hospital
[button] Show
##### Curricula Kgosi Kgari Teaching Hospital trains against
The curricula Kgosi Kgari Teaching Hospital trains against, with the state of each
| Curriculum | College | Discipline | Effective | Items | For your institution | Actions |
| Paediatric EPA Curriculum (11.1) | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-01-01 | 15 national · 1 Kgosi Kgari Teaching Hospital's own | Adopted on 2026-10-30 | [link] [aria-label="Items of Paediatric EPA Curriculum (11.1)"] Items |
##### Open to Kgosi Kgari Teaching Hospital
Adopting a version makes it the one that new admissions to its discipline join. Adopting another version of a discipline your institution has adopted supersedes the current one: registrars already admitted stay on theirs until each is moved, and your institution's own items are not carried over.
College curriculum versions open for adoption
| Curriculum | College | Discipline | Effective | Items | Actions |
| Paediatric EPA Curriculum (11.2)Newer than the version Kgosi Kgari Teaching Hospital has adopted (11.1). | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-10-30 | 16 | [button] [aria-label="Adopt Paediatric EPA Curriculum (11.2) for Kgosi Kgari Teaching Hospital…"] Adopt… |
| IM Core Curriculum (2026.1)Kgosi Kgari Teaching Hospital has adopted no General Medicine / General Internal Medicine version yet. | Demo College | General Medicine / General Internal Medicine | 2026-01-01 | 1 | [button] [aria-label="Adopt IM Core Curriculum (2026.1) for Kgosi Kgari Teaching Hospital"] Adopt |
#### Adopt Paediatric EPA Curriculum (11.2) for Kgosi Kgari Teaching Hospital?
It supersedes Paediatric EPA Curriculum (11.1) for every new admission to Paediatrics / Paediatrics at Kgosi Kgari Teaching Hospital.
The 3 registrars on 11.1 stay on it until each is moved on their profile.
Kgosi Kgari Teaching Hospital's own item on 11.1, KGK-001, is not carried over: add it again on 11.2.
[button] Cancel
[button] Adopt 11.2

######## BOARD R2-A11-390.dc.html
IllustrationR2-A11-390 · 390
Illustration · unplayed (coverage.md:198) · Demo Administrator adopting 11.2 for Kgosi Kgari Teaching Hospital at 6.31's state; the dialog names the institution
Focus: Cancel
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
### Curricula
Every College's curricula; and, for one institution, what it trains against and may adopt.
[link] Curriculum progress
[link] Create curriculum
#### All curricula
Every College's curriculum versions, with the College's status
| Curriculum | College | Discipline | Effective | Items | Status | Actions |
| Paediatric EPA Curriculum (11.2) | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-10-30 | 17 | Open | [link] [aria-label="Edit Paediatric EPA Curriculum (11.2)"] Edit [link] [aria-label="Items of Paediatric EPA Curriculum (11.2)"] Items |
| Paediatric EPA Curriculum (11.1) | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-01-01 | 16 | Open | [link] [aria-label="Edit Paediatric EPA Curriculum (11.1)"] Edit [link] [aria-label="Items of Paediatric EPA Curriculum (11.1)"] Items |
| Neonatology EPA Curriculum (1.0) | College of Paediatricians of South Africa | Paediatrics / Neonatology | 2026-10-30 | 0 | Held back | [link] [aria-label="Edit Neonatology EPA Curriculum (1.0)"] Edit [link] [aria-label="Items of Neonatology EPA Curriculum (1.0)"] Items |
| IM Core Curriculum (2026.1) | Demo College | General Medicine / General Internal Medicine | 2026-01-01 | 1 | Open | [link] [aria-label="Edit IM Core Curriculum (2026.1)"] Edit [link] [aria-label="Items of IM Core Curriculum (2026.1)"] Items |
#### An institution's curricula
Institution
Kgosi Kgari Teaching Hospital
[button] Show
##### Curricula Kgosi Kgari Teaching Hospital trains against
The curricula Kgosi Kgari Teaching Hospital trains against, with the state of each
| Curriculum | College | Discipline | Effective | Items | For your institution | Actions |
| Paediatric EPA Curriculum (11.1) | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-01-01 | 15 national · 1 Kgosi Kgari Teaching Hospital's own | Adopted on 2026-10-30 | [link] [aria-label="Items of Paediatric EPA Curriculum (11.1)"] Items |
##### Open to Kgosi Kgari Teaching Hospital
Adopting a version makes it the one that new admissions to its discipline join. Adopting another version of a discipline your institution has adopted supersedes the current one: registrars already admitted stay on theirs until each is moved, and your institution's own items are not carried over.
College curriculum versions open for adoption
| Curriculum | College | Discipline | Effective | Items | Actions |
| Paediatric EPA Curriculum (11.2)Newer than the version Kgosi Kgari Teaching Hospital has adopted (11.1). | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-10-30 | 16 | [button] [aria-label="Adopt Paediatric EPA Curriculum (11.2) for Kgosi Kgari Teaching Hospital…"] Adopt… |
| IM Core Curriculum (2026.1)Kgosi Kgari Teaching Hospital has adopted no General Medicine / General Internal Medicine version yet. | Demo College | General Medicine / General Internal Medicine | 2026-01-01 | 1 | [button] [aria-label="Adopt IM Core Curriculum (2026.1) for Kgosi Kgari Teaching Hospital"] Adopt |
#### Adopt Paediatric EPA Curriculum (11.2) for Kgosi Kgari Teaching Hospital?
It supersedes Paediatric EPA Curriculum (11.1) for every new admission to Paediatrics / Paediatrics at Kgosi Kgari Teaching Hospital.
The 3 registrars on 11.1 stay on it until each is moved on their profile.
Kgosi Kgari Teaching Hospital's own item on 11.1, KGK-001, is not carried over: add it again on 11.2.
[button] Cancel
[button] Adopt 11.2

######## BOARD R2-A12.dc.html
R2-A12 · 1280
after 6.31 · Anton Kruger, College admin · Anton Kruger's Curricula: Status Open / Held back (T9), Edit and Items; no "Open for adoption", no institution section (S5)
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
[link] My data rights
[link] Anton Kruger
[button] Sign out
### Curricula
Manage curriculum versions and the sub-speciality they apply to.
[link] Create curriculum
College of Paediatricians of South Africa's curriculum versions
| Curriculum | Discipline | Effective | Items | Status | Actions |
| Neonatology EPA Curriculum (1.0) | Paediatrics / Neonatology | 2026-10-30 | 0 | Held back | [link] [aria-label="Edit Neonatology EPA Curriculum (1.0)"] Edit [link] [aria-label="Items of Neonatology EPA Curriculum (1.0)"] Items |
| Paediatric EPA Curriculum (11.2) | Paediatrics / Paediatrics | 2026-10-30 | 16 | Open | [link] [aria-label="Edit Paediatric EPA Curriculum (11.2)"] Edit [link] [aria-label="Items of Paediatric EPA Curriculum (11.2)"] Items |
| Paediatric EPA Curriculum (11.1) | Paediatrics / Paediatrics | 2026-01-01 | 15 | Open | [link] [aria-label="Edit Paediatric EPA Curriculum (11.1)"] Edit [link] [aria-label="Items of Paediatric EPA Curriculum (11.1)"] Items |

######## BOARD R2-A12-390.dc.html
R2-A12-390 · 390
after 6.31 · Anton Kruger, College admin · Anton Kruger's Curricula: Status Open / Held back (T9), Edit and Items; no "Open for adoption", no institution section (S5)
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
[link] My data rights
[link] Anton Kruger
[button] Sign out
### Curricula
Manage curriculum versions and the sub-speciality they apply to.
[link] Create curriculum
College of Paediatricians of South Africa's curriculum versions
| Curriculum | Discipline | Effective | Items | Status | Actions |
| Neonatology EPA Curriculum (1.0) | Paediatrics / Neonatology | 2026-10-30 | 0 | Held back | [link] [aria-label="Edit Neonatology EPA Curriculum (1.0)"] Edit [link] [aria-label="Items of Neonatology EPA Curriculum (1.0)"] Items |
| Paediatric EPA Curriculum (11.2) | Paediatrics / Paediatrics | 2026-10-30 | 16 | Open | [link] [aria-label="Edit Paediatric EPA Curriculum (11.2)"] Edit [link] [aria-label="Items of Paediatric EPA Curriculum (11.2)"] Items |
| Paediatric EPA Curriculum (11.1) | Paediatrics / Paediatrics | 2026-01-01 | 15 | Open | [link] [aria-label="Edit Paediatric EPA Curriculum (11.1)"] Edit [link] [aria-label="Items of Paediatric EPA Curriculum (11.1)"] Items |

######## BOARD R2-A13.dc.html
IllustrationR2-A13 · 1280
Illustration · a second discipline adopted beside Paediatrics: Adopt on IM Core asked nothing (it supersedes nothing)
Focus: the result
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] [aria-current="page"] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
### Curricula
The College curricula Kgosi Kgari Teaching Hospital trains against, and the versions open to it.
IM Core Curriculum (2026.1) adopted. New admissions to General Medicine / General Internal Medicine join it.
#### Curricula you train against
The curricula your institution trains against, with the state of each
| Curriculum | College | Discipline | Effective | Items | For your institution | Actions |
| Paediatric EPA Curriculum (11.2) | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-10-30 | 16 national · 1 your own | Adopted on 2026-10-30 | [link] [aria-label="Items of Paediatric EPA Curriculum (11.2)"] Items |
| Paediatric EPA Curriculum (11.1) | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-01-01 | 15 national · 1 your own | Superseded on 2026-10-302 registrars still on it | [link] [aria-label="Items of Paediatric EPA Curriculum (11.1)"] Items |
| IM Core Curriculum (2026.1) | Demo College | General Medicine / General Internal Medicine | 2026-01-01 | 1 national · 0 your own | Adopted on ‹date› | [link] [aria-label="Items of IM Core Curriculum (2026.1)"] Items |
#### Open for adoption
Adopting a version makes it the one that new admissions to its discipline join. Adopting another version of a discipline your institution has adopted supersedes the current one: registrars already admitted stay on theirs until each is moved, and your institution's own items are not carried over.
College curriculum versions open for adoption
| Curriculum | College | Discipline | Effective | Items | Actions |
| Paediatric EPA Curriculum (11.1)Older than the version your institution has adopted (11.2). | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-01-01 | 15 | [button] [aria-label="Adopt Paediatric EPA Curriculum (11.1)…"] Adopt… |

######## BOARD R2-A13-390.dc.html
IllustrationR2-A13-390 · 390
Illustration · a second discipline adopted beside Paediatrics: Adopt on IM Core asked nothing (it supersedes nothing)
Focus: the result
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] [aria-current="page"] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
### Curricula
The College curricula Kgosi Kgari Teaching Hospital trains against, and the versions open to it.
IM Core Curriculum (2026.1) adopted. New admissions to General Medicine / General Internal Medicine join it.
#### Curricula you train against
The curricula your institution trains against, with the state of each
| Curriculum | College | Discipline | Effective | Items | For your institution | Actions |
| Paediatric EPA Curriculum (11.2) | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-10-30 | 16 national · 1 your own | Adopted on 2026-10-30 | [link] [aria-label="Items of Paediatric EPA Curriculum (11.2)"] Items |
| Paediatric EPA Curriculum (11.1) | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-01-01 | 15 national · 1 your own | Superseded on 2026-10-302 registrars still on it | [link] [aria-label="Items of Paediatric EPA Curriculum (11.1)"] Items |
| IM Core Curriculum (2026.1) | Demo College | General Medicine / General Internal Medicine | 2026-01-01 | 1 national · 0 your own | Adopted on ‹date› | [link] [aria-label="Items of IM Core Curriculum (2026.1)"] Items |
#### Open for adoption
Adopting a version makes it the one that new admissions to its discipline join. Adopting another version of a discipline your institution has adopted supersedes the current one: registrars already admitted stay on theirs until each is moved, and your institution's own items are not carried over.
College curriculum versions open for adoption
| Curriculum | College | Discipline | Effective | Items | Actions |
| Paediatric EPA Curriculum (11.1)Older than the version your institution has adopted (11.2). | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-01-01 | 15 | [button] [aria-label="Adopt Paediatric EPA Curriculum (11.1)…"] Adopt… |

######## BOARD R2-A13-768.dc.html
IllustrationR2-A13-768 · 768
Illustration · a second discipline adopted beside Paediatrics: Adopt on IM Core asked nothing (it supersedes nothing)
Focus: the result
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] [aria-current="page"] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
### Curricula
The College curricula Kgosi Kgari Teaching Hospital trains against, and the versions open to it.
IM Core Curriculum (2026.1) adopted. New admissions to General Medicine / General Internal Medicine join it.
#### Curricula you train against
The curricula your institution trains against, with the state of each
| Curriculum | College | Discipline | Effective | Items | For your institution | Actions |
| Paediatric EPA Curriculum (11.2) | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-10-30 | 16 national · 1 your own | Adopted on 2026-10-30 | [link] [aria-label="Items of Paediatric EPA Curriculum (11.2)"] Items |
| Paediatric EPA Curriculum (11.1) | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-01-01 | 15 national · 1 your own | Superseded on 2026-10-302 registrars still on it | [link] [aria-label="Items of Paediatric EPA Curriculum (11.1)"] Items |
| IM Core Curriculum (2026.1) | Demo College | General Medicine / General Internal Medicine | 2026-01-01 | 1 national · 0 your own | Adopted on ‹date› | [link] [aria-label="Items of IM Core Curriculum (2026.1)"] Items |
#### Open for adoption
Adopting a version makes it the one that new admissions to its discipline join. Adopting another version of a discipline your institution has adopted supersedes the current one: registrars already admitted stay on theirs until each is moved, and your institution's own items are not carried over.
College curriculum versions open for adoption
| Curriculum | College | Discipline | Effective | Items | Actions |
| Paediatric EPA Curriculum (11.1)Older than the version your institution has adopted (11.2). | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-01-01 | 15 | [button] [aria-label="Adopt Paediatric EPA Curriculum (11.1)…"] Adopt… |

######## BOARD R2-A14.dc.html
R2-A14 · 1280
Nolwazi Mbatha · loading: the header at once, a skeleton per section; no Adopt until the read is in
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] [aria-current="page"] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
### Curricula
The College curricula Kgosi Kgari Teaching Hospital trains against, and the versions open to it.
#### Curricula you train against
Loading.
#### Open for adoption
Loading.

######## BOARD R2-A14-390.dc.html
R2-A14-390 · 390
Nolwazi Mbatha · loading: the header at once, a skeleton per section; no Adopt until the read is in
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] [aria-current="page"] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
### Curricula
The College curricula Kgosi Kgari Teaching Hospital trains against, and the versions open to it.
#### Curricula you train against
Loading.
#### Open for adoption
Loading.

######## BOARD R2-A15.dc.html
R2-A15 · 1280
Nolwazi Mbatha · the load error: no empty state under it, no "Open for adoption"
Focus: Try again moves the focus to the reloaded page's h1
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] [aria-current="page"] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
### Curricula
The College curricula Kgosi Kgari Teaching Hospital trains against, and the versions open to it.
Could not load the curricula. Nothing has changed. Try again, or come back in a few minutes.
[button] Try again

######## BOARD R2-A15-390.dc.html
R2-A15-390 · 390
Nolwazi Mbatha · the load error: no empty state under it, no "Open for adoption"
Focus: Try again moves the focus to the reloaded page's h1
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] [aria-current="page"] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
### Curricula
The College curricula Kgosi Kgari Teaching Hospital trains against, and the versions open to it.
Could not load the curricula. Nothing has changed. Try again, or come back in a few minutes.
[button] Try again

######## BOARD R2-B01.dc.html
R2-B01 · 1280
1.21 · Nolwazi Mbatha · 11.1 read only: the standing notice, 15 rows "Set by the College.", no Add (no EPA it could take), the built empty line
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] [aria-current="true"] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
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
College of Paediatricians of South Africa · Paediatrics / Paediatrics · effective 2026-01-01 · adopted by your institution on 2026-10-30
The College sets this curriculum's national items, so they are read only here. Your institution's own items apply only to its trainees, and you can add, edit and remove them. A target and its period are read live: changing one changes what every trainee of your institution on this curriculum is measured against, including periods that have already closed. A tool list is checked when an activity is filed and submitted, and never takes back credit already earned.
#### Items (15)
Every EPA your institution could add is already on this curriculum: each national EPA of Paediatrics, and each of your institution's own EPAs of Paediatrics. To add an item of your own, first create a local EPA of Paediatrics.
Paediatric EPA Curriculum (11.1)'s items, in EPA order
| EPA | Target and decision | Minimum level | Minimum by training year | Window and weight | Tools | Actions |
| PAED-001 — Providing paediatric emergency care to children | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | Set by the College. |
| PAED-002 — Managing common paediatric presentations | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Chart-stimulated recall, Clinical audit, Direct observation, DOPS, Mini-CEX, MSF, RCA | Set by the College. |
| PAED-003 — Providing intensive care to children | 3 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | Set by the College. |
| PAED-004 — Managing common neonatal conditions | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Direct observation, DOPS, Mini-CEX, MSF | Set by the College. |
| PAED-005 — Providing neonatal care in intensive and high-care settings | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, DOPS, MSF | Set by the College. |
| PAED-006 — Managing long-term health conditions (LTHCs) | 2 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CCA, Direct observation, DOPS, Mini-CEX, MSF | Set by the College. |
| PAED-007 — Maintaining and promoting the health and well-being of children | 1 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | Direct observation, DOPS, Mini-CEX, MSF | Set by the College. |
| PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, DOPS, Mini-CEX, MSF, Reflective exercise | Set by the College. |
| PAED-009 — Providing care for an adolescent patient | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, MSF | Set by the College. |
| PAED-010 — Leading and operating within a clinical team | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | Direct observation, MSF | Set by the College. |
| PAED-011 — Managing population health challenges | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, MSF | Set by the College. |
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, Chart-stimulated recall, Mini-CEX, MSF | Set by the College. |
| PAED-013 — Managing child protection, legal and ethical aspects of children at risk | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, Mini-CEX, MSF | Set by the College. |
| PAED-014 — Providing palliative and end-of-life care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, MSF, Reflective exercise | Set by the College. |
| PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 1 per semesterDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, Learner feedback, MSF, Portfolio and logbook review | Set by the College. |

######## BOARD R2-B01-390.dc.html
R2-B01-390 · 390
1.21 · Nolwazi Mbatha · 11.1 read only: the standing notice, 15 rows "Set by the College.", no Add (no EPA it could take), the built empty line
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] [aria-current="true"] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
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
College of Paediatricians of South Africa · Paediatrics / Paediatrics · effective 2026-01-01 · adopted by your institution on 2026-10-30
The College sets this curriculum's national items, so they are read only here. Your institution's own items apply only to its trainees, and you can add, edit and remove them. A target and its period are read live: changing one changes what every trainee of your institution on this curriculum is measured against, including periods that have already closed. A tool list is checked when an activity is filed and submitted, and never takes back credit already earned.
#### Items (15)
Every EPA your institution could add is already on this curriculum: each national EPA of Paediatrics, and each of your institution's own EPAs of Paediatrics. To add an item of your own, first create a local EPA of Paediatrics.
Paediatric EPA Curriculum (11.1)'s items, in EPA order
| EPA | Target and decision | Minimum level | Minimum by training year | Window and weight | Tools | Actions |
| PAED-001 — Providing paediatric emergency care to children | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | Set by the College. |
| PAED-002 — Managing common paediatric presentations | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Chart-stimulated recall, Clinical audit, Direct observation, DOPS, Mini-CEX, MSF, RCA | Set by the College. |
| PAED-003 — Providing intensive care to children | 3 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | Set by the College. |
| PAED-004 — Managing common neonatal conditions | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Direct observation, DOPS, Mini-CEX, MSF | Set by the College. |
| PAED-005 — Providing neonatal care in intensive and high-care settings | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, DOPS, MSF | Set by the College. |
| PAED-006 — Managing long-term health conditions (LTHCs) | 2 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CCA, Direct observation, DOPS, Mini-CEX, MSF | Set by the College. |
| PAED-007 — Maintaining and promoting the health and well-being of children | 1 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | Direct observation, DOPS, Mini-CEX, MSF | Set by the College. |
| PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, DOPS, Mini-CEX, MSF, Reflective exercise | Set by the College. |
| PAED-009 — Providing care for an adolescent patient | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, MSF | Set by the College. |
| PAED-010 — Leading and operating within a clinical team | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | Direct observation, MSF | Set by the College. |
| PAED-011 — Managing population health challenges | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, MSF | Set by the College. |
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, Chart-stimulated recall, Mini-CEX, MSF | Set by the College. |
| PAED-013 — Managing child protection, legal and ethical aspects of children at risk | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, Mini-CEX, MSF | Set by the College. |
| PAED-014 — Providing palliative and end-of-life care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, MSF, Reflective exercise | Set by the College. |
| PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 1 per semesterDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, Learner feedback, MSF, Portfolio and logbook review | Set by the College. |

######## BOARD R2-B01-768.dc.html
R2-B01-768 · 768
1.21 · Nolwazi Mbatha · 11.1 read only: the standing notice, 15 rows "Set by the College.", no Add (no EPA it could take), the built empty line
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] [aria-current="true"] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
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
College of Paediatricians of South Africa · Paediatrics / Paediatrics · effective 2026-01-01 · adopted by your institution on 2026-10-30
The College sets this curriculum's national items, so they are read only here. Your institution's own items apply only to its trainees, and you can add, edit and remove them. A target and its period are read live: changing one changes what every trainee of your institution on this curriculum is measured against, including periods that have already closed. A tool list is checked when an activity is filed and submitted, and never takes back credit already earned.
#### Items (15)
Every EPA your institution could add is already on this curriculum: each national EPA of Paediatrics, and each of your institution's own EPAs of Paediatrics. To add an item of your own, first create a local EPA of Paediatrics.
Paediatric EPA Curriculum (11.1)'s items, in EPA order
| EPA | Target and decision | Minimum level | Minimum by training year | Window and weight | Tools | Actions |
| PAED-001 — Providing paediatric emergency care to children | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | Set by the College. |
| PAED-002 — Managing common paediatric presentations | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Chart-stimulated recall, Clinical audit, Direct observation, DOPS, Mini-CEX, MSF, RCA | Set by the College. |
| PAED-003 — Providing intensive care to children | 3 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | Set by the College. |
| PAED-004 — Managing common neonatal conditions | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Direct observation, DOPS, Mini-CEX, MSF | Set by the College. |
| PAED-005 — Providing neonatal care in intensive and high-care settings | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, DOPS, MSF | Set by the College. |
| PAED-006 — Managing long-term health conditions (LTHCs) | 2 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CCA, Direct observation, DOPS, Mini-CEX, MSF | Set by the College. |
| PAED-007 — Maintaining and promoting the health and well-being of children | 1 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | Direct observation, DOPS, Mini-CEX, MSF | Set by the College. |
| PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, DOPS, Mini-CEX, MSF, Reflective exercise | Set by the College. |
| PAED-009 — Providing care for an adolescent patient | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, MSF | Set by the College. |
| PAED-010 — Leading and operating within a clinical team | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | Direct observation, MSF | Set by the College. |
| PAED-011 — Managing population health challenges | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, MSF | Set by the College. |
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, Chart-stimulated recall, Mini-CEX, MSF | Set by the College. |
| PAED-013 — Managing child protection, legal and ethical aspects of children at risk | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, Mini-CEX, MSF | Set by the College. |
| PAED-014 — Providing palliative and end-of-life care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, MSF, Reflective exercise | Set by the College. |
| PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 1 per semesterDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, Learner feedback, MSF, Portfolio and logbook review | Set by the College. |

######## BOARD R2-B02.dc.html
R2-B02 · 1280
6.22 · Nolwazi Mbatha · PAED-012 paused by the College: the info notice under the standing one, its row "(inactive: not in force)"
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] [aria-current="true"] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
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
College of Paediatricians of South Africa · Paediatrics / Paediatrics · effective 2026-01-01 · adopted by your institution on 2026-10-30
The College sets this curriculum's national items, so they are read only here. Your institution's own items apply only to its trainees, and you can add, edit and remove them. A target and its period are read live: changing one changes what every trainee of your institution on this curriculum is measured against, including periods that have already closed. A tool list is checked when an activity is filed and submitted, and never takes back credit already earned.
PAED-012 is inactive, so its item is not in force: no trainee can choose it for a new activity, its credit is paused, and no running programme's progress page or dashboard shows it (a programme that has ended keeps it on its record). Reactivating an EPA on its own page brings its item back, and credits what was completed against it meanwhile.
#### Items (15)
Every EPA your institution could add is already on this curriculum: each national EPA of Paediatrics, and each of your institution's own EPAs of Paediatrics. To add an item of your own, first create a local EPA of Paediatrics.
Paediatric EPA Curriculum (11.1)'s items, in EPA order
| EPA | Target and decision | Minimum level | Minimum by training year | Window and weight | Tools | Actions |
| PAED-001 — Providing paediatric emergency care to children | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | Set by the College. |
| PAED-002 — Managing common paediatric presentations | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Chart-stimulated recall, Clinical audit, Direct observation, DOPS, Mini-CEX, MSF, RCA | Set by the College. |
| PAED-003 — Providing intensive care to children | 3 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | Set by the College. |
| PAED-004 — Managing common neonatal conditions | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Direct observation, DOPS, Mini-CEX, MSF | Set by the College. |
| PAED-005 — Providing neonatal care in intensive and high-care settings | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, DOPS, MSF | Set by the College. |
| PAED-006 — Managing long-term health conditions (LTHCs) in children | 2 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CCA, Direct observation, DOPS, Mini-CEX, MSF | Set by the College. |
| PAED-007 — Maintaining and promoting the health and well-being of children | 1 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | Direct observation, DOPS, Mini-CEX, MSF | Set by the College. |
| PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, DOPS, Mini-CEX, MSF, Reflective exercise | Set by the College. |
| PAED-009 — Providing care for an adolescent patient | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, MSF | Set by the College. |
| PAED-010 — Leading and operating within a clinical team | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | Direct observation, MSF | Set by the College. |
| PAED-011 — Managing population health challenges | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, MSF | Set by the College. |
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams(inactive: not in force) | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, Chart-stimulated recall, Mini-CEX, MSF | Set by the College. |
| PAED-013 — Managing child protection, legal and ethical aspects of children at risk | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, Mini-CEX, MSF | Set by the College. |
| PAED-014 — Providing palliative and end-of-life care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, MSF, Reflective exercise | Set by the College. |
| PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 1 per semesterDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, Learner feedback, MSF, Portfolio and logbook review | Set by the College. |

######## BOARD R2-B02-390.dc.html
R2-B02-390 · 390
6.22 · Nolwazi Mbatha · PAED-012 paused by the College: the info notice under the standing one, its row "(inactive: not in force)"
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] [aria-current="true"] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
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
College of Paediatricians of South Africa · Paediatrics / Paediatrics · effective 2026-01-01 · adopted by your institution on 2026-10-30
The College sets this curriculum's national items, so they are read only here. Your institution's own items apply only to its trainees, and you can add, edit and remove them. A target and its period are read live: changing one changes what every trainee of your institution on this curriculum is measured against, including periods that have already closed. A tool list is checked when an activity is filed and submitted, and never takes back credit already earned.
PAED-012 is inactive, so its item is not in force: no trainee can choose it for a new activity, its credit is paused, and no running programme's progress page or dashboard shows it (a programme that has ended keeps it on its record). Reactivating an EPA on its own page brings its item back, and credits what was completed against it meanwhile.
#### Items (15)
Every EPA your institution could add is already on this curriculum: each national EPA of Paediatrics, and each of your institution's own EPAs of Paediatrics. To add an item of your own, first create a local EPA of Paediatrics.
Paediatric EPA Curriculum (11.1)'s items, in EPA order
| EPA | Target and decision | Minimum level | Minimum by training year | Window and weight | Tools | Actions |
| PAED-001 — Providing paediatric emergency care to children | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | Set by the College. |
| PAED-002 — Managing common paediatric presentations | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Chart-stimulated recall, Clinical audit, Direct observation, DOPS, Mini-CEX, MSF, RCA | Set by the College. |
| PAED-003 — Providing intensive care to children | 3 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | Set by the College. |
| PAED-004 — Managing common neonatal conditions | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Direct observation, DOPS, Mini-CEX, MSF | Set by the College. |
| PAED-005 — Providing neonatal care in intensive and high-care settings | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, DOPS, MSF | Set by the College. |
| PAED-006 — Managing long-term health conditions (LTHCs) in children | 2 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CCA, Direct observation, DOPS, Mini-CEX, MSF | Set by the College. |
| PAED-007 — Maintaining and promoting the health and well-being of children | 1 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | Direct observation, DOPS, Mini-CEX, MSF | Set by the College. |
| PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, DOPS, Mini-CEX, MSF, Reflective exercise | Set by the College. |
| PAED-009 — Providing care for an adolescent patient | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, MSF | Set by the College. |
| PAED-010 — Leading and operating within a clinical team | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | Direct observation, MSF | Set by the College. |
| PAED-011 — Managing population health challenges | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, MSF | Set by the College. |
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams(inactive: not in force) | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, Chart-stimulated recall, Mini-CEX, MSF | Set by the College. |
| PAED-013 — Managing child protection, legal and ethical aspects of children at risk | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, Mini-CEX, MSF | Set by the College. |
| PAED-014 — Providing palliative and end-of-life care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, MSF, Reflective exercise | Set by the College. |
| PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 1 per semesterDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, Learner feedback, MSF, Portfolio and logbook review | Set by the College. |

######## BOARD R2-B03.dc.html
R2-B03 · 1280
after 6.25 · Nolwazi Mbatha · KGK-001 exists, so "Add your institution's own item" is offered by "Items (15)"
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] [aria-current="true"] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
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
College of Paediatricians of South Africa · Paediatrics / Paediatrics · effective 2026-01-01 · adopted by your institution on 2026-10-30
The College sets this curriculum's national items, so they are read only here. Your institution's own items apply only to its trainees, and you can add, edit and remove them. A target and its period are read live: changing one changes what every trainee of your institution on this curriculum is measured against, including periods that have already closed. A tool list is checked when an activity is filed and submitted, and never takes back credit already earned.
#### Items (15)
[link] Add your institution's own item
Paediatric EPA Curriculum (11.1)'s items, in EPA order
| EPA | Target and decision | Minimum level | Minimum by training year | Window and weight | Tools | Actions |
| PAED-001 — Providing paediatric emergency care to children | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | Set by the College. |
| PAED-002 — Managing common paediatric presentations | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Chart-stimulated recall, Clinical audit, Direct observation, DOPS, Mini-CEX, MSF, RCA | Set by the College. |
| PAED-003 — Providing intensive care to children | 3 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | Set by the College. |
| PAED-004 — Managing common neonatal conditions | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Direct observation, DOPS, Mini-CEX, MSF | Set by the College. |
| PAED-005 — Providing neonatal care in intensive and high-care settings | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, DOPS, MSF | Set by the College. |
| PAED-006 — Managing long-term health conditions (LTHCs) in children | 2 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CCA, Direct observation, DOPS, Mini-CEX, MSF | Set by the College. |
| PAED-007 — Maintaining and promoting the health and well-being of children | 1 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | Direct observation, DOPS, Mini-CEX, MSF | Set by the College. |
| PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, DOPS, Mini-CEX, MSF, Reflective exercise | Set by the College. |
| PAED-009 — Providing care for an adolescent patient | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, MSF | Set by the College. |
| PAED-010 — Leading and operating within a clinical team | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | Direct observation, MSF | Set by the College. |
| PAED-011 — Managing population health challenges | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, MSF | Set by the College. |
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, Chart-stimulated recall, Mini-CEX, MSF | Set by the College. |
| PAED-013 — Managing child protection, legal and ethical aspects of children at risk | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, Mini-CEX, MSF | Set by the College. |
| PAED-014 — Providing palliative and end-of-life care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, MSF, Reflective exercise | Set by the College. |
| PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 1 per semesterDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, Learner feedback, MSF, Portfolio and logbook review | Set by the College. |

######## BOARD R2-B03-390.dc.html
R2-B03-390 · 390
after 6.25 · Nolwazi Mbatha · KGK-001 exists, so "Add your institution's own item" is offered by "Items (15)"
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] [aria-current="true"] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
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
College of Paediatricians of South Africa · Paediatrics / Paediatrics · effective 2026-01-01 · adopted by your institution on 2026-10-30
The College sets this curriculum's national items, so they are read only here. Your institution's own items apply only to its trainees, and you can add, edit and remove them. A target and its period are read live: changing one changes what every trainee of your institution on this curriculum is measured against, including periods that have already closed. A tool list is checked when an activity is filed and submitted, and never takes back credit already earned.
#### Items (15)
[link] Add your institution's own item
Paediatric EPA Curriculum (11.1)'s items, in EPA order
| EPA | Target and decision | Minimum level | Minimum by training year | Window and weight | Tools | Actions |
| PAED-001 — Providing paediatric emergency care to children | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | Set by the College. |
| PAED-002 — Managing common paediatric presentations | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Chart-stimulated recall, Clinical audit, Direct observation, DOPS, Mini-CEX, MSF, RCA | Set by the College. |
| PAED-003 — Providing intensive care to children | 3 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | Set by the College. |
| PAED-004 — Managing common neonatal conditions | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Direct observation, DOPS, Mini-CEX, MSF | Set by the College. |
| PAED-005 — Providing neonatal care in intensive and high-care settings | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, DOPS, MSF | Set by the College. |
| PAED-006 — Managing long-term health conditions (LTHCs) in children | 2 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CCA, Direct observation, DOPS, Mini-CEX, MSF | Set by the College. |
| PAED-007 — Maintaining and promoting the health and well-being of children | 1 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | Direct observation, DOPS, Mini-CEX, MSF | Set by the College. |
| PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, DOPS, Mini-CEX, MSF, Reflective exercise | Set by the College. |
| PAED-009 — Providing care for an adolescent patient | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, MSF | Set by the College. |
| PAED-010 — Leading and operating within a clinical team | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | Direct observation, MSF | Set by the College. |
| PAED-011 — Managing population health challenges | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, MSF | Set by the College. |
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, Chart-stimulated recall, Mini-CEX, MSF | Set by the College. |
| PAED-013 — Managing child protection, legal and ethical aspects of children at risk | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, Mini-CEX, MSF | Set by the College. |
| PAED-014 — Providing palliative and end-of-life care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, MSF, Reflective exercise | Set by the College. |
| PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 1 per semesterDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, Learner feedback, MSF, Portfolio and logbook review | Set by the College. |

######## BOARD R2-B04.dc.html
R2-B04 · 1280
6.26 · Nolwazi Mbatha · the add page before a rung: KGK-001 the one option, chosen; the suggested scale; Add item off with its built line; at 390 the parent link and 44px rows
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] [aria-current="true"] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
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
- [aria-current="page"] Add your institution's own item
### Add your institution's own item
On Paediatric EPA Curriculum (11.1). It applies only to Kgosi Kgari Teaching Hospital's trainees.
EPA*
[selected] KGK-001 — Running a paediatric outreach clinic at a district hospital
Lists only the EPAs this item can name that no item on this curriculum already holds for the same trainees: the national EPAs of its sub-speciality and, for an item an institution added, that institution's own local EPAs. A national item holds its EPA for every institution; an institution's own item holds it for that institution alone.
Target
[value="1"]
Credited encounters required in each period below. This is the published frequency, not a total for the whole programme.
Per
Semester
[selected] Academic year (January to November)
A semester target applies to each semester on its own. A surplus in one semester does not carry into the next.
Decision cadence
[selected] No published cadence
Each semester
Each academic year
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
[selected] Choose a rung
1
2
3a
3b
4
5
Minimum by training year
A trainee in a year listed here is measured against that year's rung instead of the minimum level. A year not listed uses the minimum level.
No year has its own minimum.
[button] Add year 1
Completion window (months)
[value="12"]
Only used to suggest a trainee's expected completion date. It is not the target period, and credit does not check it.
Weight
Tools
The instruments that may be used as evidence for this EPA. None ticked means any instrument. Checked when an activity is filed and submitted; it never takes back credit already earned.
(checkbox)
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
(checkbox)
MSF
(checkbox)
Portfolio and logbook review
(checkbox)
RCA
(checkbox)
Reflective exercise
[link] Cancel
Adding is off until every minimum is a rung on CPSA Paediatric Entrustment Scale v11.1.
[button] [aria-disabled="true"] Add item

######## BOARD R2-B04-390.dc.html
R2-B04-390 · 390
6.26 · Nolwazi Mbatha · the add page before a rung: KGK-001 the one option, chosen; the suggested scale; Add item off with its built line; at 390 the parent link and 44px rows
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] [aria-current="true"] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
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
- [aria-current="page"] Add your institution's own item
### Add your institution's own item
On Paediatric EPA Curriculum (11.1). It applies only to Kgosi Kgari Teaching Hospital's trainees.
EPA*
[selected] KGK-001 — Running a paediatric outreach clinic at a district hospital
Lists only the EPAs this item can name that no item on this curriculum already holds for the same trainees: the national EPAs of its sub-speciality and, for an item an institution added, that institution's own local EPAs. A national item holds its EPA for every institution; an institution's own item holds it for that institution alone.
Target
[value="1"]
Credited encounters required in each period below. This is the published frequency, not a total for the whole programme.
Per
Semester
[selected] Academic year (January to November)
A semester target applies to each semester on its own. A surplus in one semester does not carry into the next.
Decision cadence
[selected] No published cadence
Each semester
Each academic year
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
[selected] Choose a rung
1
2
3a
3b
4
5
Minimum by training year
A trainee in a year listed here is measured against that year's rung instead of the minimum level. A year not listed uses the minimum level.
No year has its own minimum.
[button] Add year 1
Completion window (months)
[value="12"]
Only used to suggest a trainee's expected completion date. It is not the target period, and credit does not check it.
Weight
Tools
The instruments that may be used as evidence for this EPA. None ticked means any instrument. Checked when an activity is filed and submitted; it never takes back credit already earned.
(checkbox)
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
(checkbox)
MSF
(checkbox)
Portfolio and logbook review
(checkbox)
RCA
(checkbox)
Reflective exercise
[link] Cancel
Adding is off until every minimum is a rung on CPSA Paediatric Entrustment Scale v11.1.
[button] [aria-disabled="true"] Add item

######## BOARD R2-B05.dc.html
IllustrationR2-B05 · 1280
Illustration · a stale page · Add item refused: the refusal under EPA, every value kept
Focus: EPA (the refusal first in its aria-describedby)
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] [aria-current="true"] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
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
- [aria-current="page"] Add your institution's own item
### Add your institution's own item
On Paediatric EPA Curriculum (11.1). It applies only to Kgosi Kgari Teaching Hospital's trainees.
EPA*
[selected] KGK-001 — Running a paediatric outreach clinic at a district hospital
‹the command's refusal›
Lists only the EPAs this item can name that no item on this curriculum already holds for the same trainees: the national EPAs of its sub-speciality and, for an item an institution added, that institution's own local EPAs. A national item holds its EPA for every institution; an institution's own item holds it for that institution alone.
Target
[value="1"]
Credited encounters required in each period below. This is the published frequency, not a total for the whole programme.
Per
Semester
[selected] Academic year (January to November)
A semester target applies to each semester on its own. A surplus in one semester does not carry into the next.
Decision cadence
[selected] No published cadence
Each semester
Each academic year
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
[selected] 3a
3b
4
5
Minimum by training year
A trainee in a year listed here is measured against that year's rung instead of the minimum level. A year not listed uses the minimum level.
No year has its own minimum.
[button] Add year 1
Completion window (months)
[value="12"]
Only used to suggest a trainee's expected completion date. It is not the target period, and credit does not check it.
Weight
Tools
The instruments that may be used as evidence for this EPA. None ticked means any instrument. Checked when an activity is filed and submitted; it never takes back credit already earned.
(checkbox)
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
(checkbox)
MSF
(checkbox)
Portfolio and logbook review
(checkbox)
RCA
(checkbox)
Reflective exercise
[link] Cancel
[button] Add item

######## BOARD R2-B05-390.dc.html
IllustrationR2-B05-390 · 390
Illustration · a stale page · Add item refused: the refusal under EPA, every value kept
Focus: EPA (the refusal first in its aria-describedby)
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] [aria-current="true"] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
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
- [aria-current="page"] Add your institution's own item
### Add your institution's own item
On Paediatric EPA Curriculum (11.1). It applies only to Kgosi Kgari Teaching Hospital's trainees.
EPA*
[selected] KGK-001 — Running a paediatric outreach clinic at a district hospital
‹the command's refusal›
Lists only the EPAs this item can name that no item on this curriculum already holds for the same trainees: the national EPAs of its sub-speciality and, for an item an institution added, that institution's own local EPAs. A national item holds its EPA for every institution; an institution's own item holds it for that institution alone.
Target
[value="1"]
Credited encounters required in each period below. This is the published frequency, not a total for the whole programme.
Per
Semester
[selected] Academic year (January to November)
A semester target applies to each semester on its own. A surplus in one semester does not carry into the next.
Decision cadence
[selected] No published cadence
Each semester
Each academic year
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
[selected] 3a
3b
4
5
Minimum by training year
A trainee in a year listed here is measured against that year's rung instead of the minimum level. A year not listed uses the minimum level.
No year has its own minimum.
[button] Add year 1
Completion window (months)
[value="12"]
Only used to suggest a trainee's expected completion date. It is not the target period, and credit does not check it.
Weight
Tools
The instruments that may be used as evidence for this EPA. None ticked means any instrument. Checked when an activity is filed and submitted; it never takes back credit already earned.
(checkbox)
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
(checkbox)
MSF
(checkbox)
Portfolio and logbook review
(checkbox)
RCA
(checkbox)
Reflective exercise
[link] Cancel
[button] Add item

######## BOARD R2-B06.dc.html
R2-B06 · 1280
6.26 · Nolwazi Mbatha · back on the list after Add item: KGK-001 last with its mark, Edit and Remove…; the built empty line again; journey (6.25 + 6.26) 7 + 9 (390: 9 + 9)
Focus: the result, at the list's top
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] [aria-current="true"] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
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
College of Paediatricians of South Africa · Paediatrics / Paediatrics · effective 2026-01-01 · adopted by your institution on 2026-10-30
Curriculum item added.
The College sets this curriculum's national items, so they are read only here. Your institution's own items apply only to its trainees, and you can add, edit and remove them. A target and its period are read live: changing one changes what every trainee of your institution on this curriculum is measured against, including periods that have already closed. A tool list is checked when an activity is filed and submitted, and never takes back credit already earned.
#### Items (16)
Every EPA your institution could add is already on this curriculum: each national EPA of Paediatrics, and each of your institution's own EPAs of Paediatrics. To add an item of your own, first create a local EPA of Paediatrics.
Paediatric EPA Curriculum (11.1)'s items, in EPA order
| EPA | Target and decision | Minimum level | Minimum by training year | Window and weight | Tools | Actions |
| PAED-001 — Providing paediatric emergency care to children | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | Set by the College. |
| PAED-002 — Managing common paediatric presentations | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Chart-stimulated recall, Clinical audit, Direct observation, DOPS, Mini-CEX, MSF, RCA | Set by the College. |
| PAED-003 — Providing intensive care to children | 3 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | Set by the College. |
| PAED-004 — Managing common neonatal conditions | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Direct observation, DOPS, Mini-CEX, MSF | Set by the College. |
| PAED-005 — Providing neonatal care in intensive and high-care settings | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, DOPS, MSF | Set by the College. |
| PAED-006 — Managing long-term health conditions (LTHCs) in children | 2 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CCA, Direct observation, DOPS, Mini-CEX, MSF | Set by the College. |
| PAED-007 — Maintaining and promoting the health and well-being of children | 1 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | Direct observation, DOPS, Mini-CEX, MSF | Set by the College. |
| PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, DOPS, Mini-CEX, MSF, Reflective exercise | Set by the College. |
| PAED-009 — Providing care for an adolescent patient | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, MSF | Set by the College. |
| PAED-010 — Leading and operating within a clinical team | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | Direct observation, MSF | Set by the College. |
| PAED-011 — Managing population health challenges | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, MSF | Set by the College. |
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, Chart-stimulated recall, Mini-CEX, MSF | Set by the College. |
| PAED-013 — Managing child protection, legal and ethical aspects of children at risk | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, Mini-CEX, MSF | Set by the College. |
| PAED-014 — Providing palliative and end-of-life care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, MSF, Reflective exercise | Set by the College. |
| PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 1 per semesterDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, Learner feedback, MSF, Portfolio and logbook review | Set by the College. |
| KGK-001 — Running a paediatric outreach clinic at a district hospitalYour institution's own item | 1 per academic yearNo decision cadence | 3aCPSA Paediatric Entrustment Scale v11.1 | — | 12 monthsWeight — | Direct observation | [link] [aria-label="Edit KGK-001"] Edit [button] [aria-label="Remove KGK-001"] Remove… |

######## BOARD R2-B06-390.dc.html
R2-B06-390 · 390
6.26 · Nolwazi Mbatha · back on the list after Add item: KGK-001 last with its mark, Edit and Remove…; the built empty line again; journey (6.25 + 6.26) 7 + 9 (390: 9 + 9)
Focus: the result, at the list's top
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] [aria-current="true"] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
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
College of Paediatricians of South Africa · Paediatrics / Paediatrics · effective 2026-01-01 · adopted by your institution on 2026-10-30
Curriculum item added.
The College sets this curriculum's national items, so they are read only here. Your institution's own items apply only to its trainees, and you can add, edit and remove them. A target and its period are read live: changing one changes what every trainee of your institution on this curriculum is measured against, including periods that have already closed. A tool list is checked when an activity is filed and submitted, and never takes back credit already earned.
#### Items (16)
Every EPA your institution could add is already on this curriculum: each national EPA of Paediatrics, and each of your institution's own EPAs of Paediatrics. To add an item of your own, first create a local EPA of Paediatrics.
Paediatric EPA Curriculum (11.1)'s items, in EPA order
| EPA | Target and decision | Minimum level | Minimum by training year | Window and weight | Tools | Actions |
| PAED-001 — Providing paediatric emergency care to children | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | Set by the College. |
| PAED-002 — Managing common paediatric presentations | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Chart-stimulated recall, Clinical audit, Direct observation, DOPS, Mini-CEX, MSF, RCA | Set by the College. |
| PAED-003 — Providing intensive care to children | 3 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | Set by the College. |
| PAED-004 — Managing common neonatal conditions | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Direct observation, DOPS, Mini-CEX, MSF | Set by the College. |
| PAED-005 — Providing neonatal care in intensive and high-care settings | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, DOPS, MSF | Set by the College. |
| PAED-006 — Managing long-term health conditions (LTHCs) in children | 2 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CCA, Direct observation, DOPS, Mini-CEX, MSF | Set by the College. |
| PAED-007 — Maintaining and promoting the health and well-being of children | 1 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | Direct observation, DOPS, Mini-CEX, MSF | Set by the College. |
| PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, DOPS, Mini-CEX, MSF, Reflective exercise | Set by the College. |
| PAED-009 — Providing care for an adolescent patient | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, MSF | Set by the College. |
| PAED-010 — Leading and operating within a clinical team | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | Direct observation, MSF | Set by the College. |
| PAED-011 — Managing population health challenges | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, MSF | Set by the College. |
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, Chart-stimulated recall, Mini-CEX, MSF | Set by the College. |
| PAED-013 — Managing child protection, legal and ethical aspects of children at risk | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, Mini-CEX, MSF | Set by the College. |
| PAED-014 — Providing palliative and end-of-life care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, MSF, Reflective exercise | Set by the College. |
| PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 1 per semesterDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, Learner feedback, MSF, Portfolio and logbook review | Set by the College. |
| KGK-001 — Running a paediatric outreach clinic at a district hospitalYour institution's own item | 1 per academic yearNo decision cadence | 3aCPSA Paediatric Entrustment Scale v11.1 | — | 12 monthsWeight — | Direct observation | [link] [aria-label="Edit KGK-001"] Edit [button] [aria-label="Remove KGK-001"] Remove… |

######## BOARD R2-B06-768.dc.html
R2-B06-768 · 768
6.26 · Nolwazi Mbatha · back on the list after Add item: KGK-001 last with its mark, Edit and Remove…; the built empty line again; journey (6.25 + 6.26) 7 + 9 (390: 9 + 9)
Focus: the result, at the list's top
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] [aria-current="true"] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
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
College of Paediatricians of South Africa · Paediatrics / Paediatrics · effective 2026-01-01 · adopted by your institution on 2026-10-30
Curriculum item added.
The College sets this curriculum's national items, so they are read only here. Your institution's own items apply only to its trainees, and you can add, edit and remove them. A target and its period are read live: changing one changes what every trainee of your institution on this curriculum is measured against, including periods that have already closed. A tool list is checked when an activity is filed and submitted, and never takes back credit already earned.
#### Items (16)
Every EPA your institution could add is already on this curriculum: each national EPA of Paediatrics, and each of your institution's own EPAs of Paediatrics. To add an item of your own, first create a local EPA of Paediatrics.
Paediatric EPA Curriculum (11.1)'s items, in EPA order
| EPA | Target and decision | Minimum level | Minimum by training year | Window and weight | Tools | Actions |
| PAED-001 — Providing paediatric emergency care to children | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | Set by the College. |
| PAED-002 — Managing common paediatric presentations | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Chart-stimulated recall, Clinical audit, Direct observation, DOPS, Mini-CEX, MSF, RCA | Set by the College. |
| PAED-003 — Providing intensive care to children | 3 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | Set by the College. |
| PAED-004 — Managing common neonatal conditions | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Direct observation, DOPS, Mini-CEX, MSF | Set by the College. |
| PAED-005 — Providing neonatal care in intensive and high-care settings | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, DOPS, MSF | Set by the College. |
| PAED-006 — Managing long-term health conditions (LTHCs) in children | 2 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CCA, Direct observation, DOPS, Mini-CEX, MSF | Set by the College. |
| PAED-007 — Maintaining and promoting the health and well-being of children | 1 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | Direct observation, DOPS, Mini-CEX, MSF | Set by the College. |
| PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, DOPS, Mini-CEX, MSF, Reflective exercise | Set by the College. |
| PAED-009 — Providing care for an adolescent patient | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, MSF | Set by the College. |
| PAED-010 — Leading and operating within a clinical team | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | Direct observation, MSF | Set by the College. |
| PAED-011 — Managing population health challenges | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, MSF | Set by the College. |
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, Chart-stimulated recall, Mini-CEX, MSF | Set by the College. |
| PAED-013 — Managing child protection, legal and ethical aspects of children at risk | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, Mini-CEX, MSF | Set by the College. |
| PAED-014 — Providing palliative and end-of-life care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, MSF, Reflective exercise | Set by the College. |
| PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 1 per semesterDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, Learner feedback, MSF, Portfolio and logbook review | Set by the College. |
| KGK-001 — Running a paediatric outreach clinic at a district hospitalYour institution's own item | 1 per academic yearNo decision cadence | 3aCPSA Paediatric Entrustment Scale v11.1 | — | 12 monthsWeight — | Direct observation | [link] [aria-label="Edit KGK-001"] Edit [button] [aria-label="Remove KGK-001"] Remove… |

######## BOARD R2-B07.dc.html
R2-B07 · 1280
6.33 · Nolwazi Mbatha · 11.2 before the add: 16 rows, PAED-016 and PAED-011's "2 per academic year" among them; the new-version line; the action offered
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] [aria-current="true"] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
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
College of Paediatricians of South Africa · Paediatrics / Paediatrics · effective 2026-10-30 · adopted by your institution on 2026-10-30
The College sets this curriculum's national items, so they are read only here. Your institution's own items apply only to its trainees, and you can add, edit and remove them. A target and its period are read live: changing one changes what every trainee of your institution on this curriculum is measured against, including periods that have already closed. A tool list is checked when an activity is filed and submitted, and never takes back credit already earned.
#### Items (16)
[link] Add your institution's own item
A new version carries no institution's own items. Your item on 11.1 stays there.
Paediatric EPA Curriculum (11.2)'s items, in EPA order
| EPA | Target and decision | Minimum level | Minimum by training year | Window and weight | Tools | Actions |
| PAED-001 — Providing paediatric emergency care to children | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | Set by the College. |
| PAED-002 — Managing common paediatric presentations | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Chart-stimulated recall, Clinical audit, Direct observation, DOPS, Mini-CEX, MSF, RCA | Set by the College. |
| PAED-003 — Providing intensive care to children | 3 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | Set by the College. |
| PAED-004 — Managing common neonatal conditions | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Direct observation, DOPS, Mini-CEX, MSF | Set by the College. |
| PAED-005 — Providing neonatal care in intensive and high-care settings | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, DOPS, MSF | Set by the College. |
| PAED-006 — Managing long-term health conditions (LTHCs) in children | 2 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CCA, Direct observation, DOPS, Mini-CEX, MSF | Set by the College. |
| PAED-007 — Maintaining and promoting the health and well-being of children | 1 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | Direct observation, DOPS, Mini-CEX, MSF | Set by the College. |
| PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, DOPS, Mini-CEX, MSF, Reflective exercise | Set by the College. |
| PAED-009 — Providing care for an adolescent patient | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, MSF | Set by the College. |
| PAED-010 — Leading and operating within a clinical team | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | Direct observation, MSF | Set by the College. |
| PAED-011 — Managing population health challenges | 2 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, MSF | Set by the College. |
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, Chart-stimulated recall, Mini-CEX, MSF | Set by the College. |
| PAED-013 — Managing child protection, legal and ethical aspects of children at risk | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, Mini-CEX, MSF | Set by the College. |
| PAED-014 — Providing palliative and end-of-life care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, MSF, Reflective exercise | Set by the College. |
| PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 1 per semesterDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, Learner feedback, MSF, Portfolio and logbook review | Set by the College. |
| PAED-016 — Transitioning adolescents with long-term conditions to adult care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Mini-CEX, MSF | Set by the College. |

######## BOARD R2-B07-390.dc.html
R2-B07-390 · 390
6.33 · Nolwazi Mbatha · 11.2 before the add: 16 rows, PAED-016 and PAED-011's "2 per academic year" among them; the new-version line; the action offered
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] [aria-current="true"] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
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
College of Paediatricians of South Africa · Paediatrics / Paediatrics · effective 2026-10-30 · adopted by your institution on 2026-10-30
The College sets this curriculum's national items, so they are read only here. Your institution's own items apply only to its trainees, and you can add, edit and remove them. A target and its period are read live: changing one changes what every trainee of your institution on this curriculum is measured against, including periods that have already closed. A tool list is checked when an activity is filed and submitted, and never takes back credit already earned.
#### Items (16)
[link] Add your institution's own item
A new version carries no institution's own items. Your item on 11.1 stays there.
Paediatric EPA Curriculum (11.2)'s items, in EPA order
| EPA | Target and decision | Minimum level | Minimum by training year | Window and weight | Tools | Actions |
| PAED-001 — Providing paediatric emergency care to children | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | Set by the College. |
| PAED-002 — Managing common paediatric presentations | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Chart-stimulated recall, Clinical audit, Direct observation, DOPS, Mini-CEX, MSF, RCA | Set by the College. |
| PAED-003 — Providing intensive care to children | 3 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | Set by the College. |
| PAED-004 — Managing common neonatal conditions | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Direct observation, DOPS, Mini-CEX, MSF | Set by the College. |
| PAED-005 — Providing neonatal care in intensive and high-care settings | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, DOPS, MSF | Set by the College. |
| PAED-006 — Managing long-term health conditions (LTHCs) in children | 2 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CCA, Direct observation, DOPS, Mini-CEX, MSF | Set by the College. |
| PAED-007 — Maintaining and promoting the health and well-being of children | 1 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | Direct observation, DOPS, Mini-CEX, MSF | Set by the College. |
| PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, DOPS, Mini-CEX, MSF, Reflective exercise | Set by the College. |
| PAED-009 — Providing care for an adolescent patient | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, MSF | Set by the College. |
| PAED-010 — Leading and operating within a clinical team | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | Direct observation, MSF | Set by the College. |
| PAED-011 — Managing population health challenges | 2 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, MSF | Set by the College. |
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, Chart-stimulated recall, Mini-CEX, MSF | Set by the College. |
| PAED-013 — Managing child protection, legal and ethical aspects of children at risk | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, Mini-CEX, MSF | Set by the College. |
| PAED-014 — Providing palliative and end-of-life care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, MSF, Reflective exercise | Set by the College. |
| PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 1 per semesterDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, Learner feedback, MSF, Portfolio and logbook review | Set by the College. |
| PAED-016 — Transitioning adolescents with long-term conditions to adult care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Mini-CEX, MSF | Set by the College. |

######## BOARD R2-B08.dc.html
R2-B08 · 1280
6.33 · Nolwazi Mbatha · 11.2 with KGK's item: 17 rows
Focus: the result
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] [aria-current="true"] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
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
College of Paediatricians of South Africa · Paediatrics / Paediatrics · effective 2026-10-30 · adopted by your institution on 2026-10-30
Curriculum item added.
The College sets this curriculum's national items, so they are read only here. Your institution's own items apply only to its trainees, and you can add, edit and remove them. A target and its period are read live: changing one changes what every trainee of your institution on this curriculum is measured against, including periods that have already closed. A tool list is checked when an activity is filed and submitted, and never takes back credit already earned.
#### Items (17)
Every EPA your institution could add is already on this curriculum: each national EPA of Paediatrics, and each of your institution's own EPAs of Paediatrics. To add an item of your own, first create a local EPA of Paediatrics.
Paediatric EPA Curriculum (11.2)'s items, in EPA order
| EPA | Target and decision | Minimum level | Minimum by training year | Window and weight | Tools | Actions |
| PAED-001 — Providing paediatric emergency care to children | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | Set by the College. |
| PAED-002 — Managing common paediatric presentations | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Chart-stimulated recall, Clinical audit, Direct observation, DOPS, Mini-CEX, MSF, RCA | Set by the College. |
| PAED-003 — Providing intensive care to children | 3 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | Set by the College. |
| PAED-004 — Managing common neonatal conditions | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Direct observation, DOPS, Mini-CEX, MSF | Set by the College. |
| PAED-005 — Providing neonatal care in intensive and high-care settings | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, DOPS, MSF | Set by the College. |
| PAED-006 — Managing long-term health conditions (LTHCs) in children | 2 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CCA, Direct observation, DOPS, Mini-CEX, MSF | Set by the College. |
| PAED-007 — Maintaining and promoting the health and well-being of children | 1 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | Direct observation, DOPS, Mini-CEX, MSF | Set by the College. |
| PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, DOPS, Mini-CEX, MSF, Reflective exercise | Set by the College. |
| PAED-009 — Providing care for an adolescent patient | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, MSF | Set by the College. |
| PAED-010 — Leading and operating within a clinical team | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | Direct observation, MSF | Set by the College. |
| PAED-011 — Managing population health challenges | 2 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, MSF | Set by the College. |
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, Chart-stimulated recall, Mini-CEX, MSF | Set by the College. |
| PAED-013 — Managing child protection, legal and ethical aspects of children at risk | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, Mini-CEX, MSF | Set by the College. |
| PAED-014 — Providing palliative and end-of-life care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, MSF, Reflective exercise | Set by the College. |
| PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 1 per semesterDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, Learner feedback, MSF, Portfolio and logbook review | Set by the College. |
| PAED-016 — Transitioning adolescents with long-term conditions to adult care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Mini-CEX, MSF | Set by the College. |
| KGK-001 — Running a paediatric outreach clinic at a district hospitalYour institution's own item | 1 per academic yearNo decision cadence | 3aCPSA Paediatric Entrustment Scale v11.1 | — | 12 monthsWeight — | Direct observation | [link] [aria-label="Edit KGK-001"] Edit [button] [aria-label="Remove KGK-001"] Remove… |

######## BOARD R2-B08-390.dc.html
R2-B08-390 · 390
6.33 · Nolwazi Mbatha · 11.2 with KGK's item: 17 rows
Focus: the result
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] [aria-current="true"] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
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
College of Paediatricians of South Africa · Paediatrics / Paediatrics · effective 2026-10-30 · adopted by your institution on 2026-10-30
Curriculum item added.
The College sets this curriculum's national items, so they are read only here. Your institution's own items apply only to its trainees, and you can add, edit and remove them. A target and its period are read live: changing one changes what every trainee of your institution on this curriculum is measured against, including periods that have already closed. A tool list is checked when an activity is filed and submitted, and never takes back credit already earned.
#### Items (17)
Every EPA your institution could add is already on this curriculum: each national EPA of Paediatrics, and each of your institution's own EPAs of Paediatrics. To add an item of your own, first create a local EPA of Paediatrics.
Paediatric EPA Curriculum (11.2)'s items, in EPA order
| EPA | Target and decision | Minimum level | Minimum by training year | Window and weight | Tools | Actions |
| PAED-001 — Providing paediatric emergency care to children | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | Set by the College. |
| PAED-002 — Managing common paediatric presentations | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Chart-stimulated recall, Clinical audit, Direct observation, DOPS, Mini-CEX, MSF, RCA | Set by the College. |
| PAED-003 — Providing intensive care to children | 3 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | Set by the College. |
| PAED-004 — Managing common neonatal conditions | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Direct observation, DOPS, Mini-CEX, MSF | Set by the College. |
| PAED-005 — Providing neonatal care in intensive and high-care settings | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, DOPS, MSF | Set by the College. |
| PAED-006 — Managing long-term health conditions (LTHCs) in children | 2 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CCA, Direct observation, DOPS, Mini-CEX, MSF | Set by the College. |
| PAED-007 — Maintaining and promoting the health and well-being of children | 1 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | Direct observation, DOPS, Mini-CEX, MSF | Set by the College. |
| PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, DOPS, Mini-CEX, MSF, Reflective exercise | Set by the College. |
| PAED-009 — Providing care for an adolescent patient | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, MSF | Set by the College. |
| PAED-010 — Leading and operating within a clinical team | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | Direct observation, MSF | Set by the College. |
| PAED-011 — Managing population health challenges | 2 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, MSF | Set by the College. |
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, Chart-stimulated recall, Mini-CEX, MSF | Set by the College. |
| PAED-013 — Managing child protection, legal and ethical aspects of children at risk | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, Mini-CEX, MSF | Set by the College. |
| PAED-014 — Providing palliative and end-of-life care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, MSF, Reflective exercise | Set by the College. |
| PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 1 per semesterDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, Learner feedback, MSF, Portfolio and logbook review | Set by the College. |
| PAED-016 — Transitioning adolescents with long-term conditions to adult care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Mini-CEX, MSF | Set by the College. |
| KGK-001 — Running a paediatric outreach clinic at a district hospitalYour institution's own item | 1 per academic yearNo decision cadence | 3aCPSA Paediatric Entrustment Scale v11.1 | — | 12 monthsWeight — | Direct observation | [link] [aria-label="Edit KGK-001"] Edit [button] [aria-label="Remove KGK-001"] Remove… |

######## BOARD R2-B08-768.dc.html
R2-B08-768 · 768
6.33 · Nolwazi Mbatha · 11.2 with KGK's item: 17 rows
Focus: the result
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] [aria-current="true"] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
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
College of Paediatricians of South Africa · Paediatrics / Paediatrics · effective 2026-10-30 · adopted by your institution on 2026-10-30
Curriculum item added.
The College sets this curriculum's national items, so they are read only here. Your institution's own items apply only to its trainees, and you can add, edit and remove them. A target and its period are read live: changing one changes what every trainee of your institution on this curriculum is measured against, including periods that have already closed. A tool list is checked when an activity is filed and submitted, and never takes back credit already earned.
#### Items (17)
Every EPA your institution could add is already on this curriculum: each national EPA of Paediatrics, and each of your institution's own EPAs of Paediatrics. To add an item of your own, first create a local EPA of Paediatrics.
Paediatric EPA Curriculum (11.2)'s items, in EPA order
| EPA | Target and decision | Minimum level | Minimum by training year | Window and weight | Tools | Actions |
| PAED-001 — Providing paediatric emergency care to children | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | Set by the College. |
| PAED-002 — Managing common paediatric presentations | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Chart-stimulated recall, Clinical audit, Direct observation, DOPS, Mini-CEX, MSF, RCA | Set by the College. |
| PAED-003 — Providing intensive care to children | 3 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | Set by the College. |
| PAED-004 — Managing common neonatal conditions | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Direct observation, DOPS, Mini-CEX, MSF | Set by the College. |
| PAED-005 — Providing neonatal care in intensive and high-care settings | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, DOPS, MSF | Set by the College. |
| PAED-006 — Managing long-term health conditions (LTHCs) in children | 2 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CCA, Direct observation, DOPS, Mini-CEX, MSF | Set by the College. |
| PAED-007 — Maintaining and promoting the health and well-being of children | 1 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | Direct observation, DOPS, Mini-CEX, MSF | Set by the College. |
| PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, DOPS, Mini-CEX, MSF, Reflective exercise | Set by the College. |
| PAED-009 — Providing care for an adolescent patient | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, MSF | Set by the College. |
| PAED-010 — Leading and operating within a clinical team | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | Direct observation, MSF | Set by the College. |
| PAED-011 — Managing population health challenges | 2 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, MSF | Set by the College. |
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, Chart-stimulated recall, Mini-CEX, MSF | Set by the College. |
| PAED-013 — Managing child protection, legal and ethical aspects of children at risk | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, Mini-CEX, MSF | Set by the College. |
| PAED-014 — Providing palliative and end-of-life care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, MSF, Reflective exercise | Set by the College. |
| PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 1 per semesterDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, Learner feedback, MSF, Portfolio and logbook review | Set by the College. |
| PAED-016 — Transitioning adolescents with long-term conditions to adult care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Mini-CEX, MSF | Set by the College. |
| KGK-001 — Running a paediatric outreach clinic at a district hospitalYour institution's own item | 1 per academic yearNo decision cadence | 3aCPSA Paediatric Entrustment Scale v11.1 | — | 12 monthsWeight — | Direct observation | [link] [aria-label="Edit KGK-001"] Edit [button] [aria-label="Remove KGK-001"] Remove… |

######## BOARD R2-B09.dc.html
R2-B09 · 1280
after 6.33 · Nolwazi Mbatha · KGK's item's edit page: EPA a required select offering the item's EPA and the caller's own EPAs not on the curriculum (M6: on 11.2, KGK-001 alone)
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] [aria-current="true"] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
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
- [aria-current="page"] KGK-001
### Edit KGK-001 — Running a paediatric outreach clinic at a district hospital
Your institution's own item on Paediatric EPA Curriculum (11.2)
EPA*
[selected] KGK-001 — Running a paediatric outreach clinic at a district hospital
Lists only the EPAs this item can name that no item on this curriculum already holds for the same trainees: the national EPAs of its sub-speciality and, for an item an institution added, that institution's own local EPAs. A national item holds its EPA for every institution; an institution's own item holds it for that institution alone.
Target
[value="1"]
Credited encounters required in each period below. This is the published frequency, not a total for the whole programme.
Per
Semester
[selected] Academic year (January to November)
A semester target applies to each semester on its own. A surplus in one semester does not carry into the next.
Decision cadence
[selected] No published cadence
Each semester
Each academic year
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
[selected] 3a
3b
4
5
Minimum by training year
A trainee in a year listed here is measured against that year's rung instead of the minimum level. A year not listed uses the minimum level.
No year has its own minimum.
[button] Add year 1
Completion window (months)
[value="12"]
Only used to suggest a trainee's expected completion date. It is not the target period, and credit does not check it.
Weight
Tools
The instruments that may be used as evidence for this EPA. None ticked means any instrument. Checked when an activity is filed and submitted; it never takes back credit already earned.
(checkbox)
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
(checkbox)
MSF
(checkbox)
Portfolio and logbook review
(checkbox)
RCA
(checkbox)
Reflective exercise
[link] Cancel
[button] Save item

######## BOARD R2-B09-390.dc.html
R2-B09-390 · 390
after 6.33 · Nolwazi Mbatha · KGK's item's edit page: EPA a required select offering the item's EPA and the caller's own EPAs not on the curriculum (M6: on 11.2, KGK-001 alone)
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] [aria-current="true"] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
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
- [aria-current="page"] KGK-001
### Edit KGK-001 — Running a paediatric outreach clinic at a district hospital
Your institution's own item on Paediatric EPA Curriculum (11.2)
EPA*
[selected] KGK-001 — Running a paediatric outreach clinic at a district hospital
Lists only the EPAs this item can name that no item on this curriculum already holds for the same trainees: the national EPAs of its sub-speciality and, for an item an institution added, that institution's own local EPAs. A national item holds its EPA for every institution; an institution's own item holds it for that institution alone.
Target
[value="1"]
Credited encounters required in each period below. This is the published frequency, not a total for the whole programme.
Per
Semester
[selected] Academic year (January to November)
A semester target applies to each semester on its own. A surplus in one semester does not carry into the next.
Decision cadence
[selected] No published cadence
Each semester
Each academic year
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
[selected] 3a
3b
4
5
Minimum by training year
A trainee in a year listed here is measured against that year's rung instead of the minimum level. A year not listed uses the minimum level.
No year has its own minimum.
[button] Add year 1
Completion window (months)
[value="12"]
Only used to suggest a trainee's expected completion date. It is not the target period, and credit does not check it.
Weight
Tools
The instruments that may be used as evidence for this EPA. None ticked means any instrument. Checked when an activity is filed and submitted; it never takes back credit already earned.
(checkbox)
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
(checkbox)
MSF
(checkbox)
Portfolio and logbook review
(checkbox)
RCA
(checkbox)
Reflective exercise
[link] Cancel
[button] Save item

######## BOARD R2-B10.dc.html
R2-B10 · 1280
Nolwazi Mbatha · Save item pressed: back on the list
Focus: the result
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] [aria-current="true"] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
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
College of Paediatricians of South Africa · Paediatrics / Paediatrics · effective 2026-10-30 · adopted by your institution on 2026-10-30
Curriculum item saved.
The College sets this curriculum's national items, so they are read only here. Your institution's own items apply only to its trainees, and you can add, edit and remove them. A target and its period are read live: changing one changes what every trainee of your institution on this curriculum is measured against, including periods that have already closed. A tool list is checked when an activity is filed and submitted, and never takes back credit already earned.
#### Items (17)
Every EPA your institution could add is already on this curriculum: each national EPA of Paediatrics, and each of your institution's own EPAs of Paediatrics. To add an item of your own, first create a local EPA of Paediatrics.
Paediatric EPA Curriculum (11.2)'s items, in EPA order
| EPA | Target and decision | Minimum level | Minimum by training year | Window and weight | Tools | Actions |
| PAED-001 — Providing paediatric emergency care to children | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | Set by the College. |
| PAED-002 — Managing common paediatric presentations | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Chart-stimulated recall, Clinical audit, Direct observation, DOPS, Mini-CEX, MSF, RCA | Set by the College. |
| PAED-003 — Providing intensive care to children | 3 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | Set by the College. |
| PAED-004 — Managing common neonatal conditions | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Direct observation, DOPS, Mini-CEX, MSF | Set by the College. |
| PAED-005 — Providing neonatal care in intensive and high-care settings | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, DOPS, MSF | Set by the College. |
| PAED-006 — Managing long-term health conditions (LTHCs) in children | 2 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CCA, Direct observation, DOPS, Mini-CEX, MSF | Set by the College. |
| PAED-007 — Maintaining and promoting the health and well-being of children | 1 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | Direct observation, DOPS, Mini-CEX, MSF | Set by the College. |
| PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, DOPS, Mini-CEX, MSF, Reflective exercise | Set by the College. |
| PAED-009 — Providing care for an adolescent patient | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, MSF | Set by the College. |
| PAED-010 — Leading and operating within a clinical team | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | Direct observation, MSF | Set by the College. |
| PAED-011 — Managing population health challenges | 2 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, MSF | Set by the College. |
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, Chart-stimulated recall, Mini-CEX, MSF | Set by the College. |
| PAED-013 — Managing child protection, legal and ethical aspects of children at risk | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, Mini-CEX, MSF | Set by the College. |
| PAED-014 — Providing palliative and end-of-life care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, MSF, Reflective exercise | Set by the College. |
| PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 1 per semesterDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, Learner feedback, MSF, Portfolio and logbook review | Set by the College. |
| PAED-016 — Transitioning adolescents with long-term conditions to adult care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Mini-CEX, MSF | Set by the College. |
| KGK-001 — Running a paediatric outreach clinic at a district hospitalYour institution's own item | 1 per academic yearNo decision cadence | 3aCPSA Paediatric Entrustment Scale v11.1 | — | 12 monthsWeight — | Direct observation | [link] [aria-label="Edit KGK-001"] Edit [button] [aria-label="Remove KGK-001"] Remove… |

######## BOARD R2-B10-390.dc.html
R2-B10-390 · 390
Nolwazi Mbatha · Save item pressed: back on the list
Focus: the result
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] [aria-current="true"] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
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
College of Paediatricians of South Africa · Paediatrics / Paediatrics · effective 2026-10-30 · adopted by your institution on 2026-10-30
Curriculum item saved.
The College sets this curriculum's national items, so they are read only here. Your institution's own items apply only to its trainees, and you can add, edit and remove them. A target and its period are read live: changing one changes what every trainee of your institution on this curriculum is measured against, including periods that have already closed. A tool list is checked when an activity is filed and submitted, and never takes back credit already earned.
#### Items (17)
Every EPA your institution could add is already on this curriculum: each national EPA of Paediatrics, and each of your institution's own EPAs of Paediatrics. To add an item of your own, first create a local EPA of Paediatrics.
Paediatric EPA Curriculum (11.2)'s items, in EPA order
| EPA | Target and decision | Minimum level | Minimum by training year | Window and weight | Tools | Actions |
| PAED-001 — Providing paediatric emergency care to children | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | Set by the College. |
| PAED-002 — Managing common paediatric presentations | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Chart-stimulated recall, Clinical audit, Direct observation, DOPS, Mini-CEX, MSF, RCA | Set by the College. |
| PAED-003 — Providing intensive care to children | 3 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | Set by the College. |
| PAED-004 — Managing common neonatal conditions | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Direct observation, DOPS, Mini-CEX, MSF | Set by the College. |
| PAED-005 — Providing neonatal care in intensive and high-care settings | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, DOPS, MSF | Set by the College. |
| PAED-006 — Managing long-term health conditions (LTHCs) in children | 2 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CCA, Direct observation, DOPS, Mini-CEX, MSF | Set by the College. |
| PAED-007 — Maintaining and promoting the health and well-being of children | 1 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | Direct observation, DOPS, Mini-CEX, MSF | Set by the College. |
| PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, DOPS, Mini-CEX, MSF, Reflective exercise | Set by the College. |
| PAED-009 — Providing care for an adolescent patient | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, MSF | Set by the College. |
| PAED-010 — Leading and operating within a clinical team | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | Direct observation, MSF | Set by the College. |
| PAED-011 — Managing population health challenges | 2 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, MSF | Set by the College. |
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, Chart-stimulated recall, Mini-CEX, MSF | Set by the College. |
| PAED-013 — Managing child protection, legal and ethical aspects of children at risk | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, Mini-CEX, MSF | Set by the College. |
| PAED-014 — Providing palliative and end-of-life care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, MSF, Reflective exercise | Set by the College. |
| PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 1 per semesterDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, Learner feedback, MSF, Portfolio and logbook review | Set by the College. |
| PAED-016 — Transitioning adolescents with long-term conditions to adult care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Mini-CEX, MSF | Set by the College. |
| KGK-001 — Running a paediatric outreach clinic at a district hospitalYour institution's own item | 1 per academic yearNo decision cadence | 3aCPSA Paediatric Entrustment Scale v11.1 | — | 12 monthsWeight — | Direct observation | [link] [aria-label="Edit KGK-001"] Edit [button] [aria-label="Remove KGK-001"] Remove… |

######## BOARD R2-B11.dc.html
R2-B11 · 1280
Nolwazi Mbatha · Cancel pressed on the edit page: back on the list, nothing saved
Focus: KGK-001's Edit
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] [aria-current="true"] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
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
College of Paediatricians of South Africa · Paediatrics / Paediatrics · effective 2026-10-30 · adopted by your institution on 2026-10-30
The College sets this curriculum's national items, so they are read only here. Your institution's own items apply only to its trainees, and you can add, edit and remove them. A target and its period are read live: changing one changes what every trainee of your institution on this curriculum is measured against, including periods that have already closed. A tool list is checked when an activity is filed and submitted, and never takes back credit already earned.
#### Items (17)
Every EPA your institution could add is already on this curriculum: each national EPA of Paediatrics, and each of your institution's own EPAs of Paediatrics. To add an item of your own, first create a local EPA of Paediatrics.
Paediatric EPA Curriculum (11.2)'s items, in EPA order
| EPA | Target and decision | Minimum level | Minimum by training year | Window and weight | Tools | Actions |
| PAED-001 — Providing paediatric emergency care to children | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | Set by the College. |
| PAED-002 — Managing common paediatric presentations | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Chart-stimulated recall, Clinical audit, Direct observation, DOPS, Mini-CEX, MSF, RCA | Set by the College. |
| PAED-003 — Providing intensive care to children | 3 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | Set by the College. |
| PAED-004 — Managing common neonatal conditions | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Direct observation, DOPS, Mini-CEX, MSF | Set by the College. |
| PAED-005 — Providing neonatal care in intensive and high-care settings | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, DOPS, MSF | Set by the College. |
| PAED-006 — Managing long-term health conditions (LTHCs) in children | 2 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CCA, Direct observation, DOPS, Mini-CEX, MSF | Set by the College. |
| PAED-007 — Maintaining and promoting the health and well-being of children | 1 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | Direct observation, DOPS, Mini-CEX, MSF | Set by the College. |
| PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, DOPS, Mini-CEX, MSF, Reflective exercise | Set by the College. |
| PAED-009 — Providing care for an adolescent patient | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, MSF | Set by the College. |
| PAED-010 — Leading and operating within a clinical team | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | Direct observation, MSF | Set by the College. |
| PAED-011 — Managing population health challenges | 2 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, MSF | Set by the College. |
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, Chart-stimulated recall, Mini-CEX, MSF | Set by the College. |
| PAED-013 — Managing child protection, legal and ethical aspects of children at risk | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, Mini-CEX, MSF | Set by the College. |
| PAED-014 — Providing palliative and end-of-life care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, MSF, Reflective exercise | Set by the College. |
| PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 1 per semesterDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, Learner feedback, MSF, Portfolio and logbook review | Set by the College. |
| PAED-016 — Transitioning adolescents with long-term conditions to adult care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Mini-CEX, MSF | Set by the College. |
| KGK-001 — Running a paediatric outreach clinic at a district hospitalYour institution's own item | 1 per academic yearNo decision cadence | 3aCPSA Paediatric Entrustment Scale v11.1 | — | 12 monthsWeight — | Direct observation | [link] [aria-label="Edit KGK-001"] Edit [button] [aria-label="Remove KGK-001"] Remove… |

######## BOARD R2-B11-390.dc.html
R2-B11-390 · 390
Nolwazi Mbatha · Cancel pressed on the edit page: back on the list, nothing saved
Focus: KGK-001's Edit
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] [aria-current="true"] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
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
College of Paediatricians of South Africa · Paediatrics / Paediatrics · effective 2026-10-30 · adopted by your institution on 2026-10-30
The College sets this curriculum's national items, so they are read only here. Your institution's own items apply only to its trainees, and you can add, edit and remove them. A target and its period are read live: changing one changes what every trainee of your institution on this curriculum is measured against, including periods that have already closed. A tool list is checked when an activity is filed and submitted, and never takes back credit already earned.
#### Items (17)
Every EPA your institution could add is already on this curriculum: each national EPA of Paediatrics, and each of your institution's own EPAs of Paediatrics. To add an item of your own, first create a local EPA of Paediatrics.
Paediatric EPA Curriculum (11.2)'s items, in EPA order
| EPA | Target and decision | Minimum level | Minimum by training year | Window and weight | Tools | Actions |
| PAED-001 — Providing paediatric emergency care to children | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | Set by the College. |
| PAED-002 — Managing common paediatric presentations | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Chart-stimulated recall, Clinical audit, Direct observation, DOPS, Mini-CEX, MSF, RCA | Set by the College. |
| PAED-003 — Providing intensive care to children | 3 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | Set by the College. |
| PAED-004 — Managing common neonatal conditions | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Direct observation, DOPS, Mini-CEX, MSF | Set by the College. |
| PAED-005 — Providing neonatal care in intensive and high-care settings | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, DOPS, MSF | Set by the College. |
| PAED-006 — Managing long-term health conditions (LTHCs) in children | 2 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CCA, Direct observation, DOPS, Mini-CEX, MSF | Set by the College. |
| PAED-007 — Maintaining and promoting the health and well-being of children | 1 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | Direct observation, DOPS, Mini-CEX, MSF | Set by the College. |
| PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, DOPS, Mini-CEX, MSF, Reflective exercise | Set by the College. |
| PAED-009 — Providing care for an adolescent patient | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, MSF | Set by the College. |
| PAED-010 — Leading and operating within a clinical team | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | Direct observation, MSF | Set by the College. |
| PAED-011 — Managing population health challenges | 2 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, MSF | Set by the College. |
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, Chart-stimulated recall, Mini-CEX, MSF | Set by the College. |
| PAED-013 — Managing child protection, legal and ethical aspects of children at risk | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, Mini-CEX, MSF | Set by the College. |
| PAED-014 — Providing palliative and end-of-life care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, MSF, Reflective exercise | Set by the College. |
| PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 1 per semesterDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, Learner feedback, MSF, Portfolio and logbook review | Set by the College. |
| PAED-016 — Transitioning adolescents with long-term conditions to adult care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Mini-CEX, MSF | Set by the College. |
| KGK-001 — Running a paediatric outreach clinic at a district hospitalYour institution's own item | 1 per academic yearNo decision cadence | 3aCPSA Paediatric Entrustment Scale v11.1 | — | 12 monthsWeight — | Direct observation | [link] [aria-label="Edit KGK-001"] Edit [button] [aria-label="Remove KGK-001"] Remove… |

######## BOARD R2-B12.dc.html
IllustrationR2-B12 · 1280
Illustration · a save refused: the refusal under its field (here EPA), every value kept
Focus: the refused field
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] [aria-current="true"] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
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
- [aria-current="page"] KGK-001
### Edit KGK-001 — Running a paediatric outreach clinic at a district hospital
Your institution's own item on Paediatric EPA Curriculum (11.2)
EPA*
[selected] KGK-001 — Running a paediatric outreach clinic at a district hospital
‹the command's refusal›
Lists only the EPAs this item can name that no item on this curriculum already holds for the same trainees: the national EPAs of its sub-speciality and, for an item an institution added, that institution's own local EPAs. A national item holds its EPA for every institution; an institution's own item holds it for that institution alone.
Target
[value="1"]
Credited encounters required in each period below. This is the published frequency, not a total for the whole programme.
Per
Semester
[selected] Academic year (January to November)
A semester target applies to each semester on its own. A surplus in one semester does not carry into the next.
Decision cadence
[selected] No published cadence
Each semester
Each academic year
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
[selected] 3a
3b
4
5
Minimum by training year
A trainee in a year listed here is measured against that year's rung instead of the minimum level. A year not listed uses the minimum level.
No year has its own minimum.
[button] Add year 1
Completion window (months)
[value="12"]
Only used to suggest a trainee's expected completion date. It is not the target period, and credit does not check it.
Weight
Tools
The instruments that may be used as evidence for this EPA. None ticked means any instrument. Checked when an activity is filed and submitted; it never takes back credit already earned.
(checkbox)
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
(checkbox)
MSF
(checkbox)
Portfolio and logbook review
(checkbox)
RCA
(checkbox)
Reflective exercise
[link] Cancel
[button] Save item

######## BOARD R2-B12-390.dc.html
IllustrationR2-B12-390 · 390
Illustration · a save refused: the refusal under its field (here EPA), every value kept
Focus: the refused field
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] [aria-current="true"] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
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
- [aria-current="page"] KGK-001
### Edit KGK-001 — Running a paediatric outreach clinic at a district hospital
Your institution's own item on Paediatric EPA Curriculum (11.2)
EPA*
[selected] KGK-001 — Running a paediatric outreach clinic at a district hospital
‹the command's refusal›
Lists only the EPAs this item can name that no item on this curriculum already holds for the same trainees: the national EPAs of its sub-speciality and, for an item an institution added, that institution's own local EPAs. A national item holds its EPA for every institution; an institution's own item holds it for that institution alone.
Target
[value="1"]
Credited encounters required in each period below. This is the published frequency, not a total for the whole programme.
Per
Semester
[selected] Academic year (January to November)
A semester target applies to each semester on its own. A surplus in one semester does not carry into the next.
Decision cadence
[selected] No published cadence
Each semester
Each academic year
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
[selected] 3a
3b
4
5
Minimum by training year
A trainee in a year listed here is measured against that year's rung instead of the minimum level. A year not listed uses the minimum level.
No year has its own minimum.
[button] Add year 1
Completion window (months)
[value="12"]
Only used to suggest a trainee's expected completion date. It is not the target period, and credit does not check it.
Weight
Tools
The instruments that may be used as evidence for this EPA. None ticked means any instrument. Checked when an activity is filed and submitted; it never takes back credit already earned.
(checkbox)
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
(checkbox)
MSF
(checkbox)
Portfolio and logbook review
(checkbox)
RCA
(checkbox)
Reflective exercise
[link] Cancel
[button] Save item

######## BOARD R2-B13.dc.html
R2-B13 · 1280
Nolwazi Mbatha · Remove… on KGK-001: the dialog names its item (T3)
Focus: Cancel; Cancel or Escape returns to "Remove KGK-001"
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] [aria-current="true"] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
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
College of Paediatricians of South Africa · Paediatrics / Paediatrics · effective 2026-10-30 · adopted by your institution on 2026-10-30
The College sets this curriculum's national items, so they are read only here. Your institution's own items apply only to its trainees, and you can add, edit and remove them. A target and its period are read live: changing one changes what every trainee of your institution on this curriculum is measured against, including periods that have already closed. A tool list is checked when an activity is filed and submitted, and never takes back credit already earned.
#### Items (17)
Every EPA your institution could add is already on this curriculum: each national EPA of Paediatrics, and each of your institution's own EPAs of Paediatrics. To add an item of your own, first create a local EPA of Paediatrics.
Paediatric EPA Curriculum (11.2)'s items, in EPA order
| EPA | Target and decision | Minimum level | Minimum by training year | Window and weight | Tools | Actions |
| PAED-001 — Providing paediatric emergency care to children | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | Set by the College. |
| PAED-002 — Managing common paediatric presentations | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Chart-stimulated recall, Clinical audit, Direct observation, DOPS, Mini-CEX, MSF, RCA | Set by the College. |
| PAED-003 — Providing intensive care to children | 3 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | Set by the College. |
| PAED-004 — Managing common neonatal conditions | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Direct observation, DOPS, Mini-CEX, MSF | Set by the College. |
| PAED-005 — Providing neonatal care in intensive and high-care settings | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, DOPS, MSF | Set by the College. |
| PAED-006 — Managing long-term health conditions (LTHCs) in children | 2 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CCA, Direct observation, DOPS, Mini-CEX, MSF | Set by the College. |
| PAED-007 — Maintaining and promoting the health and well-being of children | 1 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | Direct observation, DOPS, Mini-CEX, MSF | Set by the College. |
| PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, DOPS, Mini-CEX, MSF, Reflective exercise | Set by the College. |
| PAED-009 — Providing care for an adolescent patient | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, MSF | Set by the College. |
| PAED-010 — Leading and operating within a clinical team | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | Direct observation, MSF | Set by the College. |
| PAED-011 — Managing population health challenges | 2 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, MSF | Set by the College. |
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, Chart-stimulated recall, Mini-CEX, MSF | Set by the College. |
| PAED-013 — Managing child protection, legal and ethical aspects of children at risk | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, Mini-CEX, MSF | Set by the College. |
| PAED-014 — Providing palliative and end-of-life care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, MSF, Reflective exercise | Set by the College. |
| PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 1 per semesterDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, Learner feedback, MSF, Portfolio and logbook review | Set by the College. |
| PAED-016 — Transitioning adolescents with long-term conditions to adult care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Mini-CEX, MSF | Set by the College. |
| KGK-001 — Running a paediatric outreach clinic at a district hospitalYour institution's own item | 1 per academic yearNo decision cadence | 3aCPSA Paediatric Entrustment Scale v11.1 | — | 12 monthsWeight — | Direct observation | [link] [aria-label="Edit KGK-001"] Edit [button] [aria-label="Remove KGK-001"] Remove… |
#### Remove KGK-001 from Paediatric EPA Curriculum (11.2)?
Remove KGK-001 — Running a paediatric outreach clinic at a district hospital from Paediatric EPA Curriculum (11.2)? Its target, minima, decision and tool list are deleted, and so is each trainee's tally toward it. Your institution's trainees on this curriculum are no longer measured against this EPA. This cannot be undone.
[button] Cancel
[button] Remove item

######## BOARD R2-B13-390.dc.html
R2-B13-390 · 390
Nolwazi Mbatha · Remove… on KGK-001: the dialog names its item (T3)
Focus: Cancel; Cancel or Escape returns to "Remove KGK-001"
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] [aria-current="true"] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
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
College of Paediatricians of South Africa · Paediatrics / Paediatrics · effective 2026-10-30 · adopted by your institution on 2026-10-30
The College sets this curriculum's national items, so they are read only here. Your institution's own items apply only to its trainees, and you can add, edit and remove them. A target and its period are read live: changing one changes what every trainee of your institution on this curriculum is measured against, including periods that have already closed. A tool list is checked when an activity is filed and submitted, and never takes back credit already earned.
#### Items (17)
Every EPA your institution could add is already on this curriculum: each national EPA of Paediatrics, and each of your institution's own EPAs of Paediatrics. To add an item of your own, first create a local EPA of Paediatrics.
Paediatric EPA Curriculum (11.2)'s items, in EPA order
| EPA | Target and decision | Minimum level | Minimum by training year | Window and weight | Tools | Actions |
| PAED-001 — Providing paediatric emergency care to children | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | Set by the College. |
| PAED-002 — Managing common paediatric presentations | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Chart-stimulated recall, Clinical audit, Direct observation, DOPS, Mini-CEX, MSF, RCA | Set by the College. |
| PAED-003 — Providing intensive care to children | 3 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | Set by the College. |
| PAED-004 — Managing common neonatal conditions | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Direct observation, DOPS, Mini-CEX, MSF | Set by the College. |
| PAED-005 — Providing neonatal care in intensive and high-care settings | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, DOPS, MSF | Set by the College. |
| PAED-006 — Managing long-term health conditions (LTHCs) in children | 2 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CCA, Direct observation, DOPS, Mini-CEX, MSF | Set by the College. |
| PAED-007 — Maintaining and promoting the health and well-being of children | 1 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | Direct observation, DOPS, Mini-CEX, MSF | Set by the College. |
| PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, DOPS, Mini-CEX, MSF, Reflective exercise | Set by the College. |
| PAED-009 — Providing care for an adolescent patient | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, MSF | Set by the College. |
| PAED-010 — Leading and operating within a clinical team | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | Direct observation, MSF | Set by the College. |
| PAED-011 — Managing population health challenges | 2 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, MSF | Set by the College. |
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, Chart-stimulated recall, Mini-CEX, MSF | Set by the College. |
| PAED-013 — Managing child protection, legal and ethical aspects of children at risk | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, Mini-CEX, MSF | Set by the College. |
| PAED-014 — Providing palliative and end-of-life care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, MSF, Reflective exercise | Set by the College. |
| PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 1 per semesterDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, Learner feedback, MSF, Portfolio and logbook review | Set by the College. |
| PAED-016 — Transitioning adolescents with long-term conditions to adult care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Mini-CEX, MSF | Set by the College. |
| KGK-001 — Running a paediatric outreach clinic at a district hospitalYour institution's own item | 1 per academic yearNo decision cadence | 3aCPSA Paediatric Entrustment Scale v11.1 | — | 12 monthsWeight — | Direct observation | [link] [aria-label="Edit KGK-001"] Edit [button] [aria-label="Remove KGK-001"] Remove… |
#### Remove KGK-001 from Paediatric EPA Curriculum (11.2)?
Remove KGK-001 — Running a paediatric outreach clinic at a district hospital from Paediatric EPA Curriculum (11.2)? Its target, minima, decision and tool list are deleted, and so is each trainee's tally toward it. Your institution's trainees on this curriculum are no longer measured against this EPA. This cannot be undone.
[button] Cancel
[button] Remove item

######## BOARD R2-B14.dc.html
R2-B14 · 1280
Nolwazi Mbatha · removed (T11): the row gone, the action offered again
Focus: the result, at the list's top
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] [aria-current="true"] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
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
College of Paediatricians of South Africa · Paediatrics / Paediatrics · effective 2026-10-30 · adopted by your institution on 2026-10-30
KGK-001 removed from this curriculum.
The College sets this curriculum's national items, so they are read only here. Your institution's own items apply only to its trainees, and you can add, edit and remove them. A target and its period are read live: changing one changes what every trainee of your institution on this curriculum is measured against, including periods that have already closed. A tool list is checked when an activity is filed and submitted, and never takes back credit already earned.
#### Items (16)
[link] Add your institution's own item
A new version carries no institution's own items. Your item on 11.1 stays there.
Paediatric EPA Curriculum (11.2)'s items, in EPA order
| EPA | Target and decision | Minimum level | Minimum by training year | Window and weight | Tools | Actions |
| PAED-001 — Providing paediatric emergency care to children | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | Set by the College. |
| PAED-002 — Managing common paediatric presentations | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Chart-stimulated recall, Clinical audit, Direct observation, DOPS, Mini-CEX, MSF, RCA | Set by the College. |
| PAED-003 — Providing intensive care to children | 3 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | Set by the College. |
| PAED-004 — Managing common neonatal conditions | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Direct observation, DOPS, Mini-CEX, MSF | Set by the College. |
| PAED-005 — Providing neonatal care in intensive and high-care settings | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, DOPS, MSF | Set by the College. |
| PAED-006 — Managing long-term health conditions (LTHCs) in children | 2 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CCA, Direct observation, DOPS, Mini-CEX, MSF | Set by the College. |
| PAED-007 — Maintaining and promoting the health and well-being of children | 1 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | Direct observation, DOPS, Mini-CEX, MSF | Set by the College. |
| PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, DOPS, Mini-CEX, MSF, Reflective exercise | Set by the College. |
| PAED-009 — Providing care for an adolescent patient | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, MSF | Set by the College. |
| PAED-010 — Leading and operating within a clinical team | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | Direct observation, MSF | Set by the College. |
| PAED-011 — Managing population health challenges | 2 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, MSF | Set by the College. |
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, Chart-stimulated recall, Mini-CEX, MSF | Set by the College. |
| PAED-013 — Managing child protection, legal and ethical aspects of children at risk | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, Mini-CEX, MSF | Set by the College. |
| PAED-014 — Providing palliative and end-of-life care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, MSF, Reflective exercise | Set by the College. |
| PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 1 per semesterDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, Learner feedback, MSF, Portfolio and logbook review | Set by the College. |
| PAED-016 — Transitioning adolescents with long-term conditions to adult care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Mini-CEX, MSF | Set by the College. |

######## BOARD R2-B14-390.dc.html
R2-B14-390 · 390
Nolwazi Mbatha · removed (T11): the row gone, the action offered again
Focus: the result, at the list's top
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] [aria-current="true"] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
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
College of Paediatricians of South Africa · Paediatrics / Paediatrics · effective 2026-10-30 · adopted by your institution on 2026-10-30
KGK-001 removed from this curriculum.
The College sets this curriculum's national items, so they are read only here. Your institution's own items apply only to its trainees, and you can add, edit and remove them. A target and its period are read live: changing one changes what every trainee of your institution on this curriculum is measured against, including periods that have already closed. A tool list is checked when an activity is filed and submitted, and never takes back credit already earned.
#### Items (16)
[link] Add your institution's own item
A new version carries no institution's own items. Your item on 11.1 stays there.
Paediatric EPA Curriculum (11.2)'s items, in EPA order
| EPA | Target and decision | Minimum level | Minimum by training year | Window and weight | Tools | Actions |
| PAED-001 — Providing paediatric emergency care to children | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | Set by the College. |
| PAED-002 — Managing common paediatric presentations | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Chart-stimulated recall, Clinical audit, Direct observation, DOPS, Mini-CEX, MSF, RCA | Set by the College. |
| PAED-003 — Providing intensive care to children | 3 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | Set by the College. |
| PAED-004 — Managing common neonatal conditions | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Direct observation, DOPS, Mini-CEX, MSF | Set by the College. |
| PAED-005 — Providing neonatal care in intensive and high-care settings | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, DOPS, MSF | Set by the College. |
| PAED-006 — Managing long-term health conditions (LTHCs) in children | 2 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CCA, Direct observation, DOPS, Mini-CEX, MSF | Set by the College. |
| PAED-007 — Maintaining and promoting the health and well-being of children | 1 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | Direct observation, DOPS, Mini-CEX, MSF | Set by the College. |
| PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, DOPS, Mini-CEX, MSF, Reflective exercise | Set by the College. |
| PAED-009 — Providing care for an adolescent patient | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, MSF | Set by the College. |
| PAED-010 — Leading and operating within a clinical team | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | Direct observation, MSF | Set by the College. |
| PAED-011 — Managing population health challenges | 2 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, MSF | Set by the College. |
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, Chart-stimulated recall, Mini-CEX, MSF | Set by the College. |
| PAED-013 — Managing child protection, legal and ethical aspects of children at risk | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, Mini-CEX, MSF | Set by the College. |
| PAED-014 — Providing palliative and end-of-life care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, MSF, Reflective exercise | Set by the College. |
| PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 1 per semesterDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, Learner feedback, MSF, Portfolio and logbook review | Set by the College. |
| PAED-016 — Transitioning adolescents with long-term conditions to adult care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Mini-CEX, MSF | Set by the College. |

######## BOARD R2-B15.dc.html
R2-B15 · 1280
Nolwazi Mbatha · a College item's edit page typed by id (Q19): never an id, nothing said of which
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] [aria-current="true"] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Curricula
-
- [aria-current="page"] Item unavailable
### Item unavailable
This curriculum item does not exist, or you cannot change it.
[link] Go to Items of Paediatric EPA Curriculum (11.2)

######## BOARD R2-B15-390.dc.html
R2-B15-390 · 390
Nolwazi Mbatha · a College item's edit page typed by id (Q19): never an id, nothing said of which
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] [aria-current="true"] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Curricula
-
- [aria-current="page"] Item unavailable
### Item unavailable
This curriculum item does not exist, or you cannot change it.
[link] Go to Items of Paediatric EPA Curriculum (11.2)

######## BOARD R2-B16.dc.html
R2-B16 · 1280
before IM Core is adopted · Nolwazi Mbatha · /admin/curricula/1/items (the Demo IM Core curriculum)
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] [aria-current="true"] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Curricula
-
- [aria-current="page"] Curriculum unavailable
### Curriculum unavailable
This curriculum does not exist, or you cannot open it.
[link] Go to Curricula

######## BOARD R2-B16-390.dc.html
R2-B16-390 · 390
before IM Core is adopted · Nolwazi Mbatha · /admin/curricula/1/items (the Demo IM Core curriculum)
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] [aria-current="true"] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Curricula
-
- [aria-current="page"] Curriculum unavailable
### Curriculum unavailable
This curriculum does not exist, or you cannot open it.
[link] Go to Curricula

######## BOARD R2-B17.dc.html
R2-B17 · 1280
after 6.33 · Demo Administrator on 11.2's items (T4): KGK's row "Kgosi Kgari Teaching Hospital's own item", every row Edit and Remove…, named with the owner on KGK's
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
[link] [aria-current="true"] Curricula
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
- [aria-current="page"] Items of Paediatric EPA Curriculum (11.2)
### Items of Paediatric EPA Curriculum (11.2)
College of Paediatricians of South Africa · Paediatrics / Paediatrics · effective 2026-10-30
‹the College's warning, as built›
#### Items (17)
Every national EPA of Paediatrics is already on this curriculum, ‹… the built line for the caller›
Paediatric EPA Curriculum (11.2)'s items, in EPA order
| EPA | Target and decision | Minimum level | Minimum by training year | Window and weight | Tools | Actions |
| PAED-001 — Providing paediatric emergency care to children | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | [link] [aria-label="Edit PAED-001"] Edit [button] [aria-label="Remove PAED-001"] Remove… |
| PAED-002 — Managing common paediatric presentations | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Chart-stimulated recall, Clinical audit, Direct observation, DOPS, Mini-CEX, MSF, RCA | [link] [aria-label="Edit PAED-002"] Edit [button] [aria-label="Remove PAED-002"] Remove… |
| PAED-003 — Providing intensive care to children | 3 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | [link] [aria-label="Edit PAED-003"] Edit [button] [aria-label="Remove PAED-003"] Remove… |
| PAED-004 — Managing common neonatal conditions | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-004"] Edit [button] [aria-label="Remove PAED-004"] Remove… |
| PAED-005 — Providing neonatal care in intensive and high-care settings | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, DOPS, MSF | [link] [aria-label="Edit PAED-005"] Edit [button] [aria-label="Remove PAED-005"] Remove… |
| PAED-006 — Managing long-term health conditions (LTHCs) in children | 2 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CCA, Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-006"] Edit [button] [aria-label="Remove PAED-006"] Remove… |
| PAED-007 — Maintaining and promoting the health and well-being of children | 1 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-007"] Edit [button] [aria-label="Remove PAED-007"] Remove… |
| PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, DOPS, Mini-CEX, MSF, Reflective exercise | [link] [aria-label="Edit PAED-008"] Edit [button] [aria-label="Remove PAED-008"] Remove… |
| PAED-009 — Providing care for an adolescent patient | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, MSF | [link] [aria-label="Edit PAED-009"] Edit [button] [aria-label="Remove PAED-009"] Remove… |
| PAED-010 — Leading and operating within a clinical team | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | Direct observation, MSF | [link] [aria-label="Edit PAED-010"] Edit [button] [aria-label="Remove PAED-010"] Remove… |
| PAED-011 — Managing population health challenges | 2 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, MSF | [link] [aria-label="Edit PAED-011"] Edit [button] [aria-label="Remove PAED-011"] Remove… |
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, Chart-stimulated recall, Mini-CEX, MSF | [link] [aria-label="Edit PAED-012"] Edit [button] [aria-label="Remove PAED-012"] Remove… |
| PAED-013 — Managing child protection, legal and ethical aspects of children at risk | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, Mini-CEX, MSF | [link] [aria-label="Edit PAED-013"] Edit [button] [aria-label="Remove PAED-013"] Remove… |
| PAED-014 — Providing palliative and end-of-life care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, MSF, Reflective exercise | [link] [aria-label="Edit PAED-014"] Edit [button] [aria-label="Remove PAED-014"] Remove… |
| PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 1 per semesterDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, Learner feedback, MSF, Portfolio and logbook review | [link] [aria-label="Edit PAED-015"] Edit [button] [aria-label="Remove PAED-015"] Remove… |
| PAED-016 — Transitioning adolescents with long-term conditions to adult care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Mini-CEX, MSF | [link] [aria-label="Edit PAED-016"] Edit [button] [aria-label="Remove PAED-016"] Remove… |
| KGK-001 — Running a paediatric outreach clinic at a district hospitalKgosi Kgari Teaching Hospital's own item | 1 per academic yearNo decision cadence | 3aCPSA Paediatric Entrustment Scale v11.1 | — | 12 monthsWeight — | Direct observation | [link] [aria-label="Edit KGK-001 (Kgosi Kgari Teaching Hospital's own item)"] Edit [button] [aria-label="Remove KGK-001 (Kgosi Kgari Teaching Hospital's own item)"] Remove… |

######## BOARD R2-B17-390.dc.html
R2-B17-390 · 390
after 6.33 · Demo Administrator on 11.2's items (T4): KGK's row "Kgosi Kgari Teaching Hospital's own item", every row Edit and Remove…, named with the owner on KGK's
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
[link] [aria-current="true"] Curricula
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
- [aria-current="page"] Items of Paediatric EPA Curriculum (11.2)
### Items of Paediatric EPA Curriculum (11.2)
College of Paediatricians of South Africa · Paediatrics / Paediatrics · effective 2026-10-30
‹the College's warning, as built›
#### Items (17)
Every national EPA of Paediatrics is already on this curriculum, ‹… the built line for the caller›
Paediatric EPA Curriculum (11.2)'s items, in EPA order
| EPA | Target and decision | Minimum level | Minimum by training year | Window and weight | Tools | Actions |
| PAED-001 — Providing paediatric emergency care to children | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | [link] [aria-label="Edit PAED-001"] Edit [button] [aria-label="Remove PAED-001"] Remove… |
| PAED-002 — Managing common paediatric presentations | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Chart-stimulated recall, Clinical audit, Direct observation, DOPS, Mini-CEX, MSF, RCA | [link] [aria-label="Edit PAED-002"] Edit [button] [aria-label="Remove PAED-002"] Remove… |
| PAED-003 — Providing intensive care to children | 3 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | [link] [aria-label="Edit PAED-003"] Edit [button] [aria-label="Remove PAED-003"] Remove… |
| PAED-004 — Managing common neonatal conditions | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-004"] Edit [button] [aria-label="Remove PAED-004"] Remove… |
| PAED-005 — Providing neonatal care in intensive and high-care settings | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, DOPS, MSF | [link] [aria-label="Edit PAED-005"] Edit [button] [aria-label="Remove PAED-005"] Remove… |
| PAED-006 — Managing long-term health conditions (LTHCs) in children | 2 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CCA, Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-006"] Edit [button] [aria-label="Remove PAED-006"] Remove… |
| PAED-007 — Maintaining and promoting the health and well-being of children | 1 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-007"] Edit [button] [aria-label="Remove PAED-007"] Remove… |
| PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, DOPS, Mini-CEX, MSF, Reflective exercise | [link] [aria-label="Edit PAED-008"] Edit [button] [aria-label="Remove PAED-008"] Remove… |
| PAED-009 — Providing care for an adolescent patient | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, MSF | [link] [aria-label="Edit PAED-009"] Edit [button] [aria-label="Remove PAED-009"] Remove… |
| PAED-010 — Leading and operating within a clinical team | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | Direct observation, MSF | [link] [aria-label="Edit PAED-010"] Edit [button] [aria-label="Remove PAED-010"] Remove… |
| PAED-011 — Managing population health challenges | 2 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, MSF | [link] [aria-label="Edit PAED-011"] Edit [button] [aria-label="Remove PAED-011"] Remove… |
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, Chart-stimulated recall, Mini-CEX, MSF | [link] [aria-label="Edit PAED-012"] Edit [button] [aria-label="Remove PAED-012"] Remove… |
| PAED-013 — Managing child protection, legal and ethical aspects of children at risk | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, Mini-CEX, MSF | [link] [aria-label="Edit PAED-013"] Edit [button] [aria-label="Remove PAED-013"] Remove… |
| PAED-014 — Providing palliative and end-of-life care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, MSF, Reflective exercise | [link] [aria-label="Edit PAED-014"] Edit [button] [aria-label="Remove PAED-014"] Remove… |
| PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 1 per semesterDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, Learner feedback, MSF, Portfolio and logbook review | [link] [aria-label="Edit PAED-015"] Edit [button] [aria-label="Remove PAED-015"] Remove… |
| PAED-016 — Transitioning adolescents with long-term conditions to adult care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Mini-CEX, MSF | [link] [aria-label="Edit PAED-016"] Edit [button] [aria-label="Remove PAED-016"] Remove… |
| KGK-001 — Running a paediatric outreach clinic at a district hospitalKgosi Kgari Teaching Hospital's own item | 1 per academic yearNo decision cadence | 3aCPSA Paediatric Entrustment Scale v11.1 | — | 12 monthsWeight — | Direct observation | [link] [aria-label="Edit KGK-001 (Kgosi Kgari Teaching Hospital's own item)"] Edit [button] [aria-label="Remove KGK-001 (Kgosi Kgari Teaching Hospital's own item)"] Remove… |

######## BOARD R2-B18.dc.html
R2-B18 · 1280
Nolwazi Mbatha · loading (T7: titled by what it knows): the trail and a skeleton, no action
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] [aria-current="true"] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Curricula
-
- [aria-current="page"] Curriculum items
### Curriculum items
Loading.

######## BOARD R2-B18-390.dc.html
R2-B18-390 · 390
Nolwazi Mbatha · loading (T7: titled by what it knows): the trail and a skeleton, no action
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] [aria-current="true"] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Curricula
-
- [aria-current="page"] Curriculum items
### Curriculum items
Loading.

######## BOARD R2-B19.dc.html
R2-B19 · 1280
Nolwazi Mbatha · the load error: no notice, no table under it
Focus: Try again moves the focus to the reloaded page's h1
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] [aria-current="true"] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Curricula
-
- [aria-current="page"] Curriculum items
### Curriculum items
Could not load this curriculum's items. Nothing has changed. Try again, or come back in a few minutes.
[button] Try again

######## BOARD R2-B19-390.dc.html
R2-B19-390 · 390
Nolwazi Mbatha · the load error: no notice, no table under it
Focus: Try again moves the focus to the reloaded page's h1
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] [aria-current="true"] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Curricula
-
- [aria-current="page"] Curriculum items
### Curriculum items
Could not load this curriculum's items. Nothing has changed. Try again, or come back in a few minutes.
[button] Try again

######## BOARD R2-B20.dc.html
IllustrationR2-B20 · 1280
Illustration · the heavy volume: seventeen rows, three of them the institution's own (‹codes›, ‹titles›)
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] [aria-current="true"] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
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
College of Paediatricians of South Africa · Paediatrics / Paediatrics · effective 2026-01-01 · adopted by your institution on 2026-10-30
The College sets this curriculum's national items, so they are read only here. Your institution's own items apply only to its trainees, and you can add, edit and remove them. A target and its period are read live: changing one changes what every trainee of your institution on this curriculum is measured against, including periods that have already closed. A tool list is checked when an activity is filed and submitted, and never takes back credit already earned.
#### Items (17)
Every EPA your institution could add is already on this curriculum: each national EPA of Paediatrics, and each of your institution's own EPAs of Paediatrics. To add an item of your own, first create a local EPA of Paediatrics.
Paediatric EPA Curriculum (11.1)'s items, in EPA order
| EPA | Target and decision | Minimum level | Minimum by training year | Window and weight | Tools | Actions |
| PAED-001 — Providing paediatric emergency care to children | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | Set by the College. |
| PAED-002 — Managing common paediatric presentations | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Chart-stimulated recall, Clinical audit, Direct observation, DOPS, Mini-CEX, MSF, RCA | Set by the College. |
| PAED-003 — Providing intensive care to children | 3 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | Set by the College. |
| PAED-004 — Managing common neonatal conditions | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Direct observation, DOPS, Mini-CEX, MSF | Set by the College. |
| PAED-005 — Providing neonatal care in intensive and high-care settings | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, DOPS, MSF | Set by the College. |
| PAED-006 — Managing long-term health conditions (LTHCs) in children | 2 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CCA, Direct observation, DOPS, Mini-CEX, MSF | Set by the College. |
| PAED-007 — Maintaining and promoting the health and well-being of children | 1 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | Direct observation, DOPS, Mini-CEX, MSF | Set by the College. |
| PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, DOPS, Mini-CEX, MSF, Reflective exercise | Set by the College. |
| PAED-009 — Providing care for an adolescent patient | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, MSF | Set by the College. |
| PAED-010 — Leading and operating within a clinical team | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | Direct observation, MSF | Set by the College. |
| PAED-011 — Managing population health challenges | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, MSF | Set by the College. |
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, Chart-stimulated recall, Mini-CEX, MSF | Set by the College. |
| PAED-013 — Managing child protection, legal and ethical aspects of children at risk | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, Mini-CEX, MSF | Set by the College. |
| PAED-014 — Providing palliative and end-of-life care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, MSF, Reflective exercise | Set by the College. |
| ‹code 1› — ‹title›Your institution's own item | ‹target›‹decision› | ‹rung› | — | ‹…› | ‹tools› | [link] [aria-label="Edit ‹code 1›"] Edit [button] [aria-label="Remove ‹code 1›"] Remove… |
| ‹code 2› — ‹title›Your institution's own item | ‹target›‹decision› | ‹rung› | — | ‹…› | ‹tools› | [link] [aria-label="Edit ‹code 2›"] Edit [button] [aria-label="Remove ‹code 2›"] Remove… |
| ‹code 3› — ‹title›Your institution's own item | ‹target›‹decision› | ‹rung› | — | ‹…› | ‹tools› | [link] [aria-label="Edit ‹code 3›"] Edit [button] [aria-label="Remove ‹code 3›"] Remove… |

######## BOARD R2-B20-390.dc.html
IllustrationR2-B20-390 · 390
Illustration · the heavy volume: seventeen rows, three of them the institution's own (‹codes›, ‹titles›)
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] [aria-current="true"] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
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
College of Paediatricians of South Africa · Paediatrics / Paediatrics · effective 2026-01-01 · adopted by your institution on 2026-10-30
The College sets this curriculum's national items, so they are read only here. Your institution's own items apply only to its trainees, and you can add, edit and remove them. A target and its period are read live: changing one changes what every trainee of your institution on this curriculum is measured against, including periods that have already closed. A tool list is checked when an activity is filed and submitted, and never takes back credit already earned.
#### Items (17)
Every EPA your institution could add is already on this curriculum: each national EPA of Paediatrics, and each of your institution's own EPAs of Paediatrics. To add an item of your own, first create a local EPA of Paediatrics.
Paediatric EPA Curriculum (11.1)'s items, in EPA order
| EPA | Target and decision | Minimum level | Minimum by training year | Window and weight | Tools | Actions |
| PAED-001 — Providing paediatric emergency care to children | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | Set by the College. |
| PAED-002 — Managing common paediatric presentations | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Chart-stimulated recall, Clinical audit, Direct observation, DOPS, Mini-CEX, MSF, RCA | Set by the College. |
| PAED-003 — Providing intensive care to children | 3 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | Set by the College. |
| PAED-004 — Managing common neonatal conditions | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Direct observation, DOPS, Mini-CEX, MSF | Set by the College. |
| PAED-005 — Providing neonatal care in intensive and high-care settings | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, DOPS, MSF | Set by the College. |
| PAED-006 — Managing long-term health conditions (LTHCs) in children | 2 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CCA, Direct observation, DOPS, Mini-CEX, MSF | Set by the College. |
| PAED-007 — Maintaining and promoting the health and well-being of children | 1 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | Direct observation, DOPS, Mini-CEX, MSF | Set by the College. |
| PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, DOPS, Mini-CEX, MSF, Reflective exercise | Set by the College. |
| PAED-009 — Providing care for an adolescent patient | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, MSF | Set by the College. |
| PAED-010 — Leading and operating within a clinical team | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | Direct observation, MSF | Set by the College. |
| PAED-011 — Managing population health challenges | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, MSF | Set by the College. |
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, Chart-stimulated recall, Mini-CEX, MSF | Set by the College. |
| PAED-013 — Managing child protection, legal and ethical aspects of children at risk | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, Mini-CEX, MSF | Set by the College. |
| PAED-014 — Providing palliative and end-of-life care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, MSF, Reflective exercise | Set by the College. |
| ‹code 1› — ‹title›Your institution's own item | ‹target›‹decision› | ‹rung› | — | ‹…› | ‹tools› | [link] [aria-label="Edit ‹code 1›"] Edit [button] [aria-label="Remove ‹code 1›"] Remove… |
| ‹code 2› — ‹title›Your institution's own item | ‹target›‹decision› | ‹rung› | — | ‹…› | ‹tools› | [link] [aria-label="Edit ‹code 2›"] Edit [button] [aria-label="Remove ‹code 2›"] Remove… |
| ‹code 3› — ‹title›Your institution's own item | ‹target›‹decision› | ‹rung› | — | ‹…› | ‹tools› | [link] [aria-label="Edit ‹code 3›"] Edit [button] [aria-label="Remove ‹code 3›"] Remove… |

######## BOARD R2-B20-768.dc.html
IllustrationR2-B20-768 · 768
Illustration · the heavy volume: seventeen rows, three of them the institution's own (‹codes›, ‹titles›)
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] [aria-current="true"] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
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
College of Paediatricians of South Africa · Paediatrics / Paediatrics · effective 2026-01-01 · adopted by your institution on 2026-10-30
The College sets this curriculum's national items, so they are read only here. Your institution's own items apply only to its trainees, and you can add, edit and remove them. A target and its period are read live: changing one changes what every trainee of your institution on this curriculum is measured against, including periods that have already closed. A tool list is checked when an activity is filed and submitted, and never takes back credit already earned.
#### Items (17)
Every EPA your institution could add is already on this curriculum: each national EPA of Paediatrics, and each of your institution's own EPAs of Paediatrics. To add an item of your own, first create a local EPA of Paediatrics.
Paediatric EPA Curriculum (11.1)'s items, in EPA order
| EPA | Target and decision | Minimum level | Minimum by training year | Window and weight | Tools | Actions |
| PAED-001 — Providing paediatric emergency care to children | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | Set by the College. |
| PAED-002 — Managing common paediatric presentations | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Chart-stimulated recall, Clinical audit, Direct observation, DOPS, Mini-CEX, MSF, RCA | Set by the College. |
| PAED-003 — Providing intensive care to children | 3 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | Set by the College. |
| PAED-004 — Managing common neonatal conditions | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Direct observation, DOPS, Mini-CEX, MSF | Set by the College. |
| PAED-005 — Providing neonatal care in intensive and high-care settings | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, DOPS, MSF | Set by the College. |
| PAED-006 — Managing long-term health conditions (LTHCs) in children | 2 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CCA, Direct observation, DOPS, Mini-CEX, MSF | Set by the College. |
| PAED-007 — Maintaining and promoting the health and well-being of children | 1 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | Direct observation, DOPS, Mini-CEX, MSF | Set by the College. |
| PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, DOPS, Mini-CEX, MSF, Reflective exercise | Set by the College. |
| PAED-009 — Providing care for an adolescent patient | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, MSF | Set by the College. |
| PAED-010 — Leading and operating within a clinical team | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | Direct observation, MSF | Set by the College. |
| PAED-011 — Managing population health challenges | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, MSF | Set by the College. |
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, Chart-stimulated recall, Mini-CEX, MSF | Set by the College. |
| PAED-013 — Managing child protection, legal and ethical aspects of children at risk | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, Mini-CEX, MSF | Set by the College. |
| PAED-014 — Providing palliative and end-of-life care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, MSF, Reflective exercise | Set by the College. |
| ‹code 1› — ‹title›Your institution's own item | ‹target›‹decision› | ‹rung› | — | ‹…› | ‹tools› | [link] [aria-label="Edit ‹code 1›"] Edit [button] [aria-label="Remove ‹code 1›"] Remove… |
| ‹code 2› — ‹title›Your institution's own item | ‹target›‹decision› | ‹rung› | — | ‹…› | ‹tools› | [link] [aria-label="Edit ‹code 2›"] Edit [button] [aria-label="Remove ‹code 2›"] Remove… |
| ‹code 3› — ‹title›Your institution's own item | ‹target›‹decision› | ‹rung› | — | ‹…› | ‹tools› | [link] [aria-label="Edit ‹code 3›"] Edit [button] [aria-label="Remove ‹code 3›"] Remove… |

######## BOARD R2-C01.dc.html
R2-C01 · 1280
1.19 · Nolwazi Mbatha · nothing adopted: Q12's words, Go to Curricula, no Create EPA
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] [aria-current="page"] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
### EPAs
The EPAs of the curricula Kgosi Kgari Teaching Hospital has adopted. The College's are read only; your institution's own are yours to change.
No EPAs yet
Your institution has not adopted a curriculum yet, so there is no discipline to add an EPA to. Adopt one under Curricula.
[link] Go to Curricula

######## BOARD R2-C01-390.dc.html
R2-C01-390 · 390
1.19 · Nolwazi Mbatha · nothing adopted: Q12's words, Go to Curricula, no Create EPA
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] [aria-current="page"] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
### EPAs
The EPAs of the curricula Kgosi Kgari Teaching Hospital has adopted. The College's are read only; your institution's own are yours to change.
No EPAs yet
Your institution has not adopted a curriculum yet, so there is no discipline to add an EPA to. Adopt one under Curricula.
[link] Go to Curricula

######## BOARD R2-C02.dc.html
R2-C02 · 1280
1.21 · Nolwazi Mbatha · the adopted fifteen: each title a link to its record, "Set by the College." in each actions cell, Create EPA in the header
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] [aria-current="page"] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
### EPAs
The EPAs of the curricula Kgosi Kgari Teaching Hospital has adopted. The College's are read only; your institution's own are yours to change.
[link] Create EPA
EPAs your institution reads
| EPA | College | Sub-speciality | Status | Actions |
| [link] PAED-001 — Providing paediatric emergency care to children | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-002 — Managing common paediatric presentations | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-003 — Providing intensive care to children | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-004 — Managing common neonatal conditions | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-005 — Providing neonatal care in intensive and high-care settings | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-006 — Managing long-term health conditions (LTHCs) | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-007 — Maintaining and promoting the health and well-being of children | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-009 — Providing care for an adolescent patient | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-010 — Leading and operating within a clinical team | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-011 — Managing population health challenges | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-013 — Managing child protection, legal and ethical aspects of children at risk | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-014 — Providing palliative and end-of-life care | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |

######## BOARD R2-C02-390.dc.html
R2-C02-390 · 390
1.21 · Nolwazi Mbatha · the adopted fifteen: each title a link to its record, "Set by the College." in each actions cell, Create EPA in the header
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] [aria-current="page"] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
### EPAs
The EPAs of the curricula Kgosi Kgari Teaching Hospital has adopted. The College's are read only; your institution's own are yours to change.
[link] Create EPA
EPAs your institution reads
| EPA | College | Sub-speciality | Status | Actions |
| [link] PAED-001 — Providing paediatric emergency care to children | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-002 — Managing common paediatric presentations | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-003 — Providing intensive care to children | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-004 — Managing common neonatal conditions | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-005 — Providing neonatal care in intensive and high-care settings | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-006 — Managing long-term health conditions (LTHCs) | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-007 — Maintaining and promoting the health and well-being of children | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-009 — Providing care for an adolescent patient | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-010 — Leading and operating within a clinical team | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-011 — Managing population health challenges | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-013 — Managing child protection, legal and ethical aspects of children at risk | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-014 — Providing palliative and end-of-life care | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |

######## BOARD R2-C02-768.dc.html
R2-C02-768 · 768
1.21 · Nolwazi Mbatha · the adopted fifteen: each title a link to its record, "Set by the College." in each actions cell, Create EPA in the header
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] [aria-current="page"] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
### EPAs
The EPAs of the curricula Kgosi Kgari Teaching Hospital has adopted. The College's are read only; your institution's own are yours to change.
[link] Create EPA
EPAs your institution reads
| EPA | College | Sub-speciality | Status | Actions |
| [link] PAED-001 — Providing paediatric emergency care to children | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-002 — Managing common paediatric presentations | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-003 — Providing intensive care to children | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-004 — Managing common neonatal conditions | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-005 — Providing neonatal care in intensive and high-care settings | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-006 — Managing long-term health conditions (LTHCs) | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-007 — Maintaining and promoting the health and well-being of children | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-009 — Providing care for an adolescent patient | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-010 — Leading and operating within a clinical team | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-011 — Managing population health challenges | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-013 — Managing child protection, legal and ethical aspects of children at risk | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-014 — Providing palliative and end-of-life care | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |

######## BOARD R2-C03.dc.html
R2-C03 · 1280
6.22 · Nolwazi Mbatha · PAED-012 Inactive, in plain words (S7)
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] [aria-current="page"] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
### EPAs
The EPAs of the curricula Kgosi Kgari Teaching Hospital has adopted. The College's are read only; your institution's own are yours to change.
[link] Create EPA
EPAs your institution reads
| EPA | College | Sub-speciality | Status | Actions |
| [link] PAED-001 — Providing paediatric emergency care to children | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-002 — Managing common paediatric presentations | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-003 — Providing intensive care to children | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-004 — Managing common neonatal conditions | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-005 — Providing neonatal care in intensive and high-care settings | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-006 — Managing long-term health conditions (LTHCs) in children | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-007 — Maintaining and promoting the health and well-being of children | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-009 — Providing care for an adolescent patient | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-010 — Leading and operating within a clinical team | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-011 — Managing population health challenges | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Inactive | Set by the College. |
| [link] PAED-013 — Managing child protection, legal and ethical aspects of children at risk | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-014 — Providing palliative and end-of-life care | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |

######## BOARD R2-C03-390.dc.html
R2-C03-390 · 390
6.22 · Nolwazi Mbatha · PAED-012 Inactive, in plain words (S7)
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] [aria-current="page"] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
### EPAs
The EPAs of the curricula Kgosi Kgari Teaching Hospital has adopted. The College's are read only; your institution's own are yours to change.
[link] Create EPA
EPAs your institution reads
| EPA | College | Sub-speciality | Status | Actions |
| [link] PAED-001 — Providing paediatric emergency care to children | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-002 — Managing common paediatric presentations | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-003 — Providing intensive care to children | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-004 — Managing common neonatal conditions | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-005 — Providing neonatal care in intensive and high-care settings | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-006 — Managing long-term health conditions (LTHCs) in children | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-007 — Maintaining and promoting the health and well-being of children | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-009 — Providing care for an adolescent patient | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-010 — Leading and operating within a clinical team | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-011 — Managing population health challenges | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Inactive | Set by the College. |
| [link] PAED-013 — Managing child protection, legal and ethical aspects of children at risk | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-014 — Providing palliative and end-of-life care | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |

######## BOARD R2-C04.dc.html
R2-C04 · 1280
6.25 · Nolwazi Mbatha · Create EPA: Sub-speciality preset to the one adopted discipline, with its help; the fields typed; submit "Save" (T6)
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] [aria-current="true"] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
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
An EPA of your institution's own. It applies only to Kgosi Kgari Teaching Hospital's trainees.
Sub-speciality*
[selected] Paediatrics / Paediatrics
The disciplines your institution has adopted.
Code*
[value="KGK-001"]
Title*
[value="Running a paediatric outreach clinic at a district hospital"]
Description
Plans and runs a monthly outreach clinic at a North West district hospital, with its referral follow-up.
Required knowledge and skills
Category*
Core
[selected] Elective
[link] Cancel
[button] Save

######## BOARD R2-C04-390.dc.html
R2-C04-390 · 390
6.25 · Nolwazi Mbatha · Create EPA: Sub-speciality preset to the one adopted discipline, with its help; the fields typed; submit "Save" (T6)
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] [aria-current="true"] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
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
An EPA of your institution's own. It applies only to Kgosi Kgari Teaching Hospital's trainees.
Sub-speciality*
[selected] Paediatrics / Paediatrics
The disciplines your institution has adopted.
Code*
[value="KGK-001"]
Title*
[value="Running a paediatric outreach clinic at a district hospital"]
Description
Plans and runs a monthly outreach clinic at a North West district hospital, with its referral follow-up.
Required knowledge and skills
Category*
Core
[selected] Elective
[link] Cancel
[button] Save

######## BOARD R2-C05.dc.html
R2-C05 · 1280
Nolwazi Mbatha · a real duplicate (T272): the refusal under Code, everything kept
Focus: Code
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] [aria-current="true"] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
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
An EPA of your institution's own. It applies only to Kgosi Kgari Teaching Hospital's trainees.
Sub-speciality*
[selected] Paediatrics / Paediatrics
The disciplines your institution has adopted.
Code*
[value="KGK-001"]
An EPA with the same code already exists for this sub-speciality.
Title*
[value="Running a paediatric outreach clinic at a district hospital"]
Description
Plans and runs a monthly outreach clinic at a North West district hospital, with its referral follow-up.
Required knowledge and skills
Category*
Core
[selected] Elective
[link] Cancel
[button] Save

######## BOARD R2-C05-390.dc.html
R2-C05-390 · 390
Nolwazi Mbatha · a real duplicate (T272): the refusal under Code, everything kept
Focus: Code
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] [aria-current="true"] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
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
An EPA of your institution's own. It applies only to Kgosi Kgari Teaching Hospital's trainees.
Sub-speciality*
[selected] Paediatrics / Paediatrics
The disciplines your institution has adopted.
Code*
[value="KGK-001"]
An EPA with the same code already exists for this sub-speciality.
Title*
[value="Running a paediatric outreach clinic at a district hospital"]
Description
Plans and runs a monthly outreach clinic at a North West district hospital, with its referral follow-up.
Required knowledge and skills
Category*
Core
[selected] Elective
[link] Cancel
[button] Save

######## BOARD R2-C06.dc.html
IllustrationR2-C06 · 1280
Illustration · a failure that is not a duplicate (T272): the alert above the form
Focus: the alert
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] [aria-current="true"] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
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
An EPA of your institution's own. It applies only to Kgosi Kgari Teaching Hospital's trainees.
The EPA could not be saved. Nothing has changed. Try again, or come back in a few minutes.
Sub-speciality*
[selected] Paediatrics / Paediatrics
The disciplines your institution has adopted.
Code*
[value="KGK-001"]
Title*
[value="Running a paediatric outreach clinic at a district hospital"]
Description
Plans and runs a monthly outreach clinic at a North West district hospital, with its referral follow-up.
Required knowledge and skills
Category*
Core
[selected] Elective
[link] Cancel
[button] Save

######## BOARD R2-C06-390.dc.html
IllustrationR2-C06-390 · 390
Illustration · a failure that is not a duplicate (T272): the alert above the form
Focus: the alert
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] [aria-current="true"] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
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
An EPA of your institution's own. It applies only to Kgosi Kgari Teaching Hospital's trainees.
The EPA could not be saved. Nothing has changed. Try again, or come back in a few minutes.
Sub-speciality*
[selected] Paediatrics / Paediatrics
The disciplines your institution has adopted.
Code*
[value="KGK-001"]
Title*
[value="Running a paediatric outreach clinic at a district hospital"]
Description
Plans and runs a monthly outreach clinic at a North West district hospital, with its referral follow-up.
Required knowledge and skills
Category*
Core
[selected] Elective
[link] Cancel
[button] Save

######## BOARD R2-C07.dc.html
IllustrationR2-C07 · 1280
Illustration · a stale page (P7, Q12): the refusal under Sub-speciality
Focus: Sub-speciality
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] [aria-current="true"] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
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
An EPA of your institution's own. It applies only to Kgosi Kgari Teaching Hospital's trainees.
Sub-speciality*
[selected] Paediatrics / Neonatology
Your institution has not adopted Paediatrics / Neonatology, so an EPA cannot be added to it.
The disciplines your institution has adopted.
Code*
[value="KGK-001"]
Title*
[value="Running a paediatric outreach clinic at a district hospital"]
Description
Plans and runs a monthly outreach clinic at a North West district hospital, with its referral follow-up.
Required knowledge and skills
Category*
Core
[selected] Elective
[link] Cancel
[button] Save

######## BOARD R2-C07-390.dc.html
IllustrationR2-C07-390 · 390
Illustration · a stale page (P7, Q12): the refusal under Sub-speciality
Focus: Sub-speciality
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] [aria-current="true"] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
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
An EPA of your institution's own. It applies only to Kgosi Kgari Teaching Hospital's trainees.
Sub-speciality*
[selected] Paediatrics / Neonatology
Your institution has not adopted Paediatrics / Neonatology, so an EPA cannot be added to it.
The disciplines your institution has adopted.
Code*
[value="KGK-001"]
Title*
[value="Running a paediatric outreach clinic at a district hospital"]
Description
Plans and runs a monthly outreach clinic at a North West district hospital, with its referral follow-up.
Required knowledge and skills
Category*
Core
[selected] Elective
[link] Cancel
[button] Save

######## BOARD R2-C08.dc.html
R2-C08 · 1280
6.25 · Nolwazi Mbatha · saved: /admin/epas/17 titled by the EPA, crumb KGK-001, no Back to EPAs; Status Active with Q11's help; Cancel, Deactivate…, Save
Focus: "EPA saved."
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] [aria-current="true"] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] EPAs
-
- [aria-current="page"] KGK-001
### KGK-001 — Running a paediatric outreach clinic at a district hospital
Define the EPA code, scope, and descriptive guidance.
EPA saved.
Sub-speciality*
[selected] Paediatrics / Paediatrics
The disciplines your institution has adopted.
Code*
[value="KGK-001"]
Title*
[value="Running a paediatric outreach clinic at a district hospital"]
Description
Plans and runs a monthly outreach clinic at a North West district hospital, with its referral follow-up.
Required knowledge and skills
Category*
Core
[selected] Elective
Status
(checkbox) [checked]
Active
An inactive EPA cannot be chosen by Kgosi Kgari Teaching Hospital's trainees for a new activity and is not a target on their running programmes' progress pages or dashboards; a programme that has ended keeps it on its record. Evidence already recorded against it is kept, and so is the progress it earned while it was active. An activity completed against it while it is inactive counts towards nothing until the EPA is reactivated, and reactivating it credits those activities.
[link] Cancel
[button] Deactivate…
[button] Save

######## BOARD R2-C08-390.dc.html
R2-C08-390 · 390
6.25 · Nolwazi Mbatha · saved: /admin/epas/17 titled by the EPA, crumb KGK-001, no Back to EPAs; Status Active with Q11's help; Cancel, Deactivate…, Save
Focus: "EPA saved."
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] [aria-current="true"] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] EPAs
-
- [aria-current="page"] KGK-001
### KGK-001 — Running a paediatric outreach clinic at a district hospital
Define the EPA code, scope, and descriptive guidance.
EPA saved.
Sub-speciality*
[selected] Paediatrics / Paediatrics
The disciplines your institution has adopted.
Code*
[value="KGK-001"]
Title*
[value="Running a paediatric outreach clinic at a district hospital"]
Description
Plans and runs a monthly outreach clinic at a North West district hospital, with its referral follow-up.
Required knowledge and skills
Category*
Core
[selected] Elective
Status
(checkbox) [checked]
Active
An inactive EPA cannot be chosen by Kgosi Kgari Teaching Hospital's trainees for a new activity and is not a target on their running programmes' progress pages or dashboards; a programme that has ended keeps it on its record. Evidence already recorded against it is kept, and so is the progress it earned while it was active. An activity completed against it while it is inactive counts towards nothing until the EPA is reactivated, and reactivating it credits those activities.
[link] Cancel
[button] Deactivate…
[button] Save

######## BOARD R2-C09.dc.html
R2-C09 · 1280
6.25 · Nolwazi Mbatha · the list after: KGK-001 first, "Your institution's own EPA" under its title in words (M4), its Edit the only one
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] [aria-current="page"] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
### EPAs
The EPAs of the curricula Kgosi Kgari Teaching Hospital has adopted. The College's are read only; your institution's own are yours to change.
[link] Create EPA
EPAs your institution reads
| EPA | College | Sub-speciality | Status | Actions |
| [link] KGK-001 — Running a paediatric outreach clinic at a district hospitalYour institution's own EPA | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit KGK-001 — Running a paediatric outreach clinic at a district hospital"] Edit |
| [link] PAED-001 — Providing paediatric emergency care to children | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-002 — Managing common paediatric presentations | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-003 — Providing intensive care to children | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-004 — Managing common neonatal conditions | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-005 — Providing neonatal care in intensive and high-care settings | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-006 — Managing long-term health conditions (LTHCs) in children | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-007 — Maintaining and promoting the health and well-being of children | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-009 — Providing care for an adolescent patient | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-010 — Leading and operating within a clinical team | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-011 — Managing population health challenges | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-013 — Managing child protection, legal and ethical aspects of children at risk | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-014 — Providing palliative and end-of-life care | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |

######## BOARD R2-C09-390.dc.html
R2-C09-390 · 390
6.25 · Nolwazi Mbatha · the list after: KGK-001 first, "Your institution's own EPA" under its title in words (M4), its Edit the only one
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] [aria-current="page"] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
### EPAs
The EPAs of the curricula Kgosi Kgari Teaching Hospital has adopted. The College's are read only; your institution's own are yours to change.
[link] Create EPA
EPAs your institution reads
| EPA | College | Sub-speciality | Status | Actions |
| [link] KGK-001 — Running a paediatric outreach clinic at a district hospitalYour institution's own EPA | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit KGK-001 — Running a paediatric outreach clinic at a district hospital"] Edit |
| [link] PAED-001 — Providing paediatric emergency care to children | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-002 — Managing common paediatric presentations | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-003 — Providing intensive care to children | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-004 — Managing common neonatal conditions | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-005 — Providing neonatal care in intensive and high-care settings | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-006 — Managing long-term health conditions (LTHCs) in children | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-007 — Maintaining and promoting the health and well-being of children | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-009 — Providing care for an adolescent patient | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-010 — Leading and operating within a clinical team | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-011 — Managing population health challenges | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-013 — Managing child protection, legal and ethical aspects of children at risk | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-014 — Providing palliative and end-of-life care | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |

######## BOARD R2-C09-768.dc.html
R2-C09-768 · 768
6.25 · Nolwazi Mbatha · the list after: KGK-001 first, "Your institution's own EPA" under its title in words (M4), its Edit the only one
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] [aria-current="page"] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
### EPAs
The EPAs of the curricula Kgosi Kgari Teaching Hospital has adopted. The College's are read only; your institution's own are yours to change.
[link] Create EPA
EPAs your institution reads
| EPA | College | Sub-speciality | Status | Actions |
| [link] KGK-001 — Running a paediatric outreach clinic at a district hospitalYour institution's own EPA | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit KGK-001 — Running a paediatric outreach clinic at a district hospital"] Edit |
| [link] PAED-001 — Providing paediatric emergency care to children | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-002 — Managing common paediatric presentations | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-003 — Providing intensive care to children | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-004 — Managing common neonatal conditions | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-005 — Providing neonatal care in intensive and high-care settings | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-006 — Managing long-term health conditions (LTHCs) in children | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-007 — Maintaining and promoting the health and well-being of children | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-009 — Providing care for an adolescent patient | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-010 — Leading and operating within a clinical team | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-011 — Managing population health challenges | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-013 — Managing child protection, legal and ethical aspects of children at risk | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-014 — Providing palliative and end-of-life care | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |
| [link] PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | Set by the College. |

######## BOARD R2-C10.dc.html
R2-C10 · 1280
after 6.23 · Nolwazi Mbatha · PAED-012's record (Q2): no form, no Save, no Deactivate; no "On your institution's curricula" (S9)
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] [aria-current="true"] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
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
College of Paediatricians of South Africa · Paediatrics / Paediatrics · Set by the College, so read only here
#### About this EPA
**College
College of Paediatricians of South Africa
**Sub-speciality
Paediatrics / Paediatrics
**Category
Core
**Status
Active
#### Description
This EPA focuses on sharing clear health information with patients and their families, and with colleagues across the healthcare team. The registrar must convey medical information in an easy-to-understand way, listen well, and work effectively as a team member.
#### Required knowledge and skills
12.1 Communicate effectively with patients and families in emergency settings12.2 Communicate effectively with patients and families in the neonatal ICU, neonatal high care and PICU12.3 Counsel caregivers about immunisation, growth charts and dysmorphism12.4 Demonstrate counselling for common presentations12.5 Lead a counselling session with the family of a child with an LTHC12.6 Demonstrate counselling in palliative care scenarios12.7 Communicate effectively with healthcare colleagues

######## BOARD R2-C10-390.dc.html
R2-C10-390 · 390
after 6.23 · Nolwazi Mbatha · PAED-012's record (Q2): no form, no Save, no Deactivate; no "On your institution's curricula" (S9)
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] [aria-current="true"] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
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
College of Paediatricians of South Africa · Paediatrics / Paediatrics · Set by the College, so read only here
#### About this EPA
**College
College of Paediatricians of South Africa
**Sub-speciality
Paediatrics / Paediatrics
**Category
Core
**Status
Active
#### Description
This EPA focuses on sharing clear health information with patients and their families, and with colleagues across the healthcare team. The registrar must convey medical information in an easy-to-understand way, listen well, and work effectively as a team member.
#### Required knowledge and skills
12.1 Communicate effectively with patients and families in emergency settings12.2 Communicate effectively with patients and families in the neonatal ICU, neonatal high care and PICU12.3 Counsel caregivers about immunisation, growth charts and dysmorphism12.4 Demonstrate counselling for common presentations12.5 Lead a counselling session with the family of a child with an LTHC12.6 Demonstrate counselling in palliative care scenarios12.7 Communicate effectively with healthcare colleagues

######## BOARD R2-C11.dc.html
R2-C11 · 1280
Nolwazi Mbatha · /admin/epas/1 (the Demo College's EPA-001) typed by id (Q2, Q19): no id shown, nothing said of which
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] [aria-current="true"] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] EPAs
-
- [aria-current="page"] EPA unavailable
### EPA unavailable
This EPA does not exist, or you cannot open it.
[link] Go to EPAs

######## BOARD R2-C11-390.dc.html
R2-C11-390 · 390
Nolwazi Mbatha · /admin/epas/1 (the Demo College's EPA-001) typed by id (Q2, Q19): no id shown, nothing said of which
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] [aria-current="true"] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] EPAs
-
- [aria-current="page"] EPA unavailable
### EPA unavailable
This EPA does not exist, or you cannot open it.
[link] Go to EPAs

######## BOARD R2-C12.dc.html
R2-C12 · 1280
Nolwazi Mbatha · Deactivate… on KGK-001 (Q11, S8)
Focus: Cancel; Cancel or Escape returns to Deactivate…
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] [aria-current="true"] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] EPAs
-
- [aria-current="page"] KGK-001
### KGK-001 — Running a paediatric outreach clinic at a district hospital
Define the EPA code, scope, and descriptive guidance.
Sub-speciality*
[selected] Paediatrics / Paediatrics
The disciplines your institution has adopted.
Code*
[value="KGK-001"]
Title*
[value="Running a paediatric outreach clinic at a district hospital"]
Description
Plans and runs a monthly outreach clinic at a North West district hospital, with its referral follow-up.
Required knowledge and skills
Category*
Core
[selected] Elective
Status
(checkbox) [checked]
Active
An inactive EPA cannot be chosen by Kgosi Kgari Teaching Hospital's trainees for a new activity and is not a target on their running programmes' progress pages or dashboards; a programme that has ended keeps it on its record. Evidence already recorded against it is kept, and so is the progress it earned while it was active. An activity completed against it while it is inactive counts towards nothing until the EPA is reactivated, and reactivating it credits those activities.
[link] Cancel
[button] Deactivate…
[button] Save
#### Deactivate KGK-001?
It can no longer be chosen for a new activity, and it stops being a target on running programmes' progress pages and dashboards, for Kgosi Kgari Teaching Hospital's trainees. A programme that has ended keeps it on its record, marked no longer in use. The progress it has earned is kept. An activity completed against it while it is inactive counts towards nothing until it is reactivated. You can reactivate it on this page.
[button] Cancel
[button] Deactivate

######## BOARD R2-C12-390.dc.html
R2-C12-390 · 390
Nolwazi Mbatha · Deactivate… on KGK-001 (Q11, S8)
Focus: Cancel; Cancel or Escape returns to Deactivate…
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] [aria-current="true"] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] EPAs
-
- [aria-current="page"] KGK-001
### KGK-001 — Running a paediatric outreach clinic at a district hospital
Define the EPA code, scope, and descriptive guidance.
Sub-speciality*
[selected] Paediatrics / Paediatrics
The disciplines your institution has adopted.
Code*
[value="KGK-001"]
Title*
[value="Running a paediatric outreach clinic at a district hospital"]
Description
Plans and runs a monthly outreach clinic at a North West district hospital, with its referral follow-up.
Required knowledge and skills
Category*
Core
[selected] Elective
Status
(checkbox) [checked]
Active
An inactive EPA cannot be chosen by Kgosi Kgari Teaching Hospital's trainees for a new activity and is not a target on their running programmes' progress pages or dashboards; a programme that has ended keeps it on its record. Evidence already recorded against it is kept, and so is the progress it earned while it was active. An activity completed against it while it is inactive counts towards nothing until the EPA is reactivated, and reactivating it credits those activities.
[link] Cancel
[button] Deactivate…
[button] Save
#### Deactivate KGK-001?
It can no longer be chosen for a new activity, and it stops being a target on running programmes' progress pages and dashboards, for Kgosi Kgari Teaching Hospital's trainees. A programme that has ended keeps it on its record, marked no longer in use. The progress it has earned is kept. An activity completed against it while it is inactive counts towards nothing until it is reactivated. You can reactivate it on this page.
[button] Cancel
[button] Deactivate

######## BOARD R2-C13.dc.html
R2-C13 · 1280
Nolwazi Mbatha · deactivated: Active unticked, Deactivate… gone, the h1 unchanged
Focus: "EPA deactivated."
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] [aria-current="true"] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] EPAs
-
- [aria-current="page"] KGK-001
### KGK-001 — Running a paediatric outreach clinic at a district hospital
Define the EPA code, scope, and descriptive guidance.
EPA deactivated.
Sub-speciality*
[selected] Paediatrics / Paediatrics
The disciplines your institution has adopted.
Code*
[value="KGK-001"]
Title*
[value="Running a paediatric outreach clinic at a district hospital"]
Description
Plans and runs a monthly outreach clinic at a North West district hospital, with its referral follow-up.
Required knowledge and skills
Category*
Core
[selected] Elective
Status
(checkbox)
Active
An inactive EPA cannot be chosen by Kgosi Kgari Teaching Hospital's trainees for a new activity and is not a target on their running programmes' progress pages or dashboards; a programme that has ended keeps it on its record. Evidence already recorded against it is kept, and so is the progress it earned while it was active. An activity completed against it while it is inactive counts towards nothing until the EPA is reactivated, and reactivating it credits those activities.
[link] Cancel
[button] Save

######## BOARD R2-C13-390.dc.html
R2-C13-390 · 390
Nolwazi Mbatha · deactivated: Active unticked, Deactivate… gone, the h1 unchanged
Focus: "EPA deactivated."
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] [aria-current="true"] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] EPAs
-
- [aria-current="page"] KGK-001
### KGK-001 — Running a paediatric outreach clinic at a district hospital
Define the EPA code, scope, and descriptive guidance.
EPA deactivated.
Sub-speciality*
[selected] Paediatrics / Paediatrics
The disciplines your institution has adopted.
Code*
[value="KGK-001"]
Title*
[value="Running a paediatric outreach clinic at a district hospital"]
Description
Plans and runs a monthly outreach clinic at a North West district hospital, with its referral follow-up.
Required knowledge and skills
Category*
Core
[selected] Elective
Status
(checkbox)
Active
An inactive EPA cannot be chosen by Kgosi Kgari Teaching Hospital's trainees for a new activity and is not a target on their running programmes' progress pages or dashboards; a programme that has ended keeps it on its record. Evidence already recorded against it is kept, and so is the progress it earned while it was active. An activity completed against it while it is inactive counts towards nothing until the EPA is reactivated, and reactivating it credits those activities.
[link] Cancel
[button] Save

######## BOARD R2-C14.dc.html
R2-C14 · 1280
Nolwazi Mbatha · Active ticked and Save: nothing asked. The other built count: "EPA reactivated. 1 activity completed against it while it was inactive now counts towards progress."
Focus: the result
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] [aria-current="true"] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] EPAs
-
- [aria-current="page"] KGK-001
### KGK-001 — Running a paediatric outreach clinic at a district hospital
Define the EPA code, scope, and descriptive guidance.
EPA reactivated. No activity was completed against it while it was inactive, so no progress changed.
Sub-speciality*
[selected] Paediatrics / Paediatrics
The disciplines your institution has adopted.
Code*
[value="KGK-001"]
Title*
[value="Running a paediatric outreach clinic at a district hospital"]
Description
Plans and runs a monthly outreach clinic at a North West district hospital, with its referral follow-up.
Required knowledge and skills
Category*
Core
[selected] Elective
Status
(checkbox) [checked]
Active
An inactive EPA cannot be chosen by Kgosi Kgari Teaching Hospital's trainees for a new activity and is not a target on their running programmes' progress pages or dashboards; a programme that has ended keeps it on its record. Evidence already recorded against it is kept, and so is the progress it earned while it was active. An activity completed against it while it is inactive counts towards nothing until the EPA is reactivated, and reactivating it credits those activities.
[link] Cancel
[button] Deactivate…
[button] Save

######## BOARD R2-C14-390.dc.html
R2-C14-390 · 390
Nolwazi Mbatha · Active ticked and Save: nothing asked. The other built count: "EPA reactivated. 1 activity completed against it while it was inactive now counts towards progress."
Focus: the result
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] [aria-current="true"] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] EPAs
-
- [aria-current="page"] KGK-001
### KGK-001 — Running a paediatric outreach clinic at a district hospital
Define the EPA code, scope, and descriptive guidance.
EPA reactivated. No activity was completed against it while it was inactive, so no progress changed.
Sub-speciality*
[selected] Paediatrics / Paediatrics
The disciplines your institution has adopted.
Code*
[value="KGK-001"]
Title*
[value="Running a paediatric outreach clinic at a district hospital"]
Description
Plans and runs a monthly outreach clinic at a North West district hospital, with its referral follow-up.
Required knowledge and skills
Category*
Core
[selected] Elective
Status
(checkbox) [checked]
Active
An inactive EPA cannot be chosen by Kgosi Kgari Teaching Hospital's trainees for a new activity and is not a target on their running programmes' progress pages or dashboards; a programme that has ended keeps it on its record. Evidence already recorded against it is kept, and so is the progress it earned while it was active. An activity completed against it while it is inactive counts towards nothing until the EPA is reactivated, and reactivating it credits those activities.
[link] Cancel
[button] Deactivate…
[button] Save

######## BOARD R2-C15.dc.html
IllustrationR2-C15 · 1280
Illustration (the volume) · Demo Administrator · 40 EPAs across CPSA, Demo College, KGK's own and ‹institution›'s own; the College column kept (S6); two own EPAs sharing "KGK-001" read apart by their marks; Edit on every row
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
Every College's EPAs, and every institution's own.
[link] Create EPA
Every EPA
| EPA | College | Sub-speciality | Status | Actions |
| [link] KGK-001 — Running a paediatric outreach clinic at a district hospitalKgosi Kgari Teaching Hospital's own EPA | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit KGK-001 — Running a paediatric outreach clinic at a district hospital"] Edit |
| [link] ‹code› — ‹title›‹institution›'s own EPA | ‹College› | ‹discipline› | Active | [link] [aria-label="Edit ‹code› — ‹title›"] Edit |
| [link] KGK-001 — ‹title›‹institution›'s own EPA | ‹College› | ‹discipline› | Active | [link] [aria-label="Edit KGK-001 — ‹title›"] Edit |
| [link] PAED-001 — Providing paediatric emergency care to children | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-001 — Providing paediatric emergency care to children"] Edit |
| [link] PAED-002 — Managing common paediatric presentations | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-002 — Managing common paediatric presentations"] Edit |
| [link] PAED-003 — Providing intensive care to children | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-003 — Providing intensive care to children"] Edit |
| [link] PAED-004 — Managing common neonatal conditions | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-004 — Managing common neonatal conditions"] Edit |
| [link] PAED-005 — Providing neonatal care in intensive and high-care settings | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-005 — Providing neonatal care in intensive and high-care settings"] Edit |
| [link] PAED-006 — Managing long-term health conditions (LTHCs) in children | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-006 — Managing long-term health conditions (LTHCs) in children"] Edit |
| [link] PAED-007 — Maintaining and promoting the health and well-being of children | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-007 — Maintaining and promoting the health and well-being of children"] Edit |
| [link] PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children"] Edit |
| [link] PAED-009 — Providing care for an adolescent patient | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-009 — Providing care for an adolescent patient"] Edit |
| [link] PAED-010 — Leading and operating within a clinical team | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-010 — Leading and operating within a clinical team"] Edit |
| [link] PAED-011 — Managing population health challenges | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-011 — Managing population health challenges"] Edit |
| [link] PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams"] Edit |
| [link] PAED-013 — Managing child protection, legal and ethical aspects of children at risk | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-013 — Managing child protection, legal and ethical aspects of children at risk"] Edit |
| [link] PAED-014 — Providing palliative and end-of-life care | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-014 — Providing palliative and end-of-life care"] Edit |
| [link] PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research"] Edit |
| [link] PAED-016 — Transitioning adolescents with long-term conditions to adult care | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-016 — Transitioning adolescents with long-term conditions to adult care"] Edit |
| [link] EPA-001 — Clerk, assess, and present a general medical admission | Demo College | General Medicine / General Internal Medicine | Active | [link] [aria-label="Edit EPA-001 — Clerk, assess, and present a general medical admission"] Edit |
| ‹the other rows of the 40, past PagerControls› |
[aria-label="Pages of EPAs"]
[button] [aria-disabled="true"] Previous
Page 1 of ‹n›
[button] Next

######## BOARD R2-C15-390.dc.html
IllustrationR2-C15-390 · 390
Illustration (the volume) · Demo Administrator · 40 EPAs across CPSA, Demo College, KGK's own and ‹institution›'s own; the College column kept (S6); two own EPAs sharing "KGK-001" read apart by their marks; Edit on every row
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
Every College's EPAs, and every institution's own.
[link] Create EPA
Every EPA
| EPA | College | Sub-speciality | Status | Actions |
| [link] KGK-001 — Running a paediatric outreach clinic at a district hospitalKgosi Kgari Teaching Hospital's own EPA | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit KGK-001 — Running a paediatric outreach clinic at a district hospital"] Edit |
| [link] ‹code› — ‹title›‹institution›'s own EPA | ‹College› | ‹discipline› | Active | [link] [aria-label="Edit ‹code› — ‹title›"] Edit |
| [link] KGK-001 — ‹title›‹institution›'s own EPA | ‹College› | ‹discipline› | Active | [link] [aria-label="Edit KGK-001 — ‹title›"] Edit |
| [link] PAED-001 — Providing paediatric emergency care to children | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-001 — Providing paediatric emergency care to children"] Edit |
| [link] PAED-002 — Managing common paediatric presentations | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-002 — Managing common paediatric presentations"] Edit |
| [link] PAED-003 — Providing intensive care to children | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-003 — Providing intensive care to children"] Edit |
| [link] PAED-004 — Managing common neonatal conditions | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-004 — Managing common neonatal conditions"] Edit |
| [link] PAED-005 — Providing neonatal care in intensive and high-care settings | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-005 — Providing neonatal care in intensive and high-care settings"] Edit |
| [link] PAED-006 — Managing long-term health conditions (LTHCs) in children | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-006 — Managing long-term health conditions (LTHCs) in children"] Edit |
| [link] PAED-007 — Maintaining and promoting the health and well-being of children | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-007 — Maintaining and promoting the health and well-being of children"] Edit |
| [link] PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children"] Edit |
| [link] PAED-009 — Providing care for an adolescent patient | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-009 — Providing care for an adolescent patient"] Edit |
| [link] PAED-010 — Leading and operating within a clinical team | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-010 — Leading and operating within a clinical team"] Edit |
| [link] PAED-011 — Managing population health challenges | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-011 — Managing population health challenges"] Edit |
| [link] PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams"] Edit |
| [link] PAED-013 — Managing child protection, legal and ethical aspects of children at risk | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-013 — Managing child protection, legal and ethical aspects of children at risk"] Edit |
| [link] PAED-014 — Providing palliative and end-of-life care | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-014 — Providing palliative and end-of-life care"] Edit |
| [link] PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research"] Edit |
| [link] PAED-016 — Transitioning adolescents with long-term conditions to adult care | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-016 — Transitioning adolescents with long-term conditions to adult care"] Edit |
| [link] EPA-001 — Clerk, assess, and present a general medical admission | Demo College | General Medicine / General Internal Medicine | Active | [link] [aria-label="Edit EPA-001 — Clerk, assess, and present a general medical admission"] Edit |
| ‹the other rows of the 40, past PagerControls› |
[aria-label="Pages of EPAs"]
[button] [aria-disabled="true"] Previous
Page 1 of ‹n›
[button] Next

######## BOARD R2-C15-768.dc.html
IllustrationR2-C15-768 · 768
Illustration (the volume) · Demo Administrator · 40 EPAs across CPSA, Demo College, KGK's own and ‹institution›'s own; the College column kept (S6); two own EPAs sharing "KGK-001" read apart by their marks; Edit on every row
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
Every College's EPAs, and every institution's own.
[link] Create EPA
Every EPA
| EPA | College | Sub-speciality | Status | Actions |
| [link] KGK-001 — Running a paediatric outreach clinic at a district hospitalKgosi Kgari Teaching Hospital's own EPA | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit KGK-001 — Running a paediatric outreach clinic at a district hospital"] Edit |
| [link] ‹code› — ‹title›‹institution›'s own EPA | ‹College› | ‹discipline› | Active | [link] [aria-label="Edit ‹code› — ‹title›"] Edit |
| [link] KGK-001 — ‹title›‹institution›'s own EPA | ‹College› | ‹discipline› | Active | [link] [aria-label="Edit KGK-001 — ‹title›"] Edit |
| [link] PAED-001 — Providing paediatric emergency care to children | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-001 — Providing paediatric emergency care to children"] Edit |
| [link] PAED-002 — Managing common paediatric presentations | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-002 — Managing common paediatric presentations"] Edit |
| [link] PAED-003 — Providing intensive care to children | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-003 — Providing intensive care to children"] Edit |
| [link] PAED-004 — Managing common neonatal conditions | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-004 — Managing common neonatal conditions"] Edit |
| [link] PAED-005 — Providing neonatal care in intensive and high-care settings | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-005 — Providing neonatal care in intensive and high-care settings"] Edit |
| [link] PAED-006 — Managing long-term health conditions (LTHCs) in children | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-006 — Managing long-term health conditions (LTHCs) in children"] Edit |
| [link] PAED-007 — Maintaining and promoting the health and well-being of children | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-007 — Maintaining and promoting the health and well-being of children"] Edit |
| [link] PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children"] Edit |
| [link] PAED-009 — Providing care for an adolescent patient | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-009 — Providing care for an adolescent patient"] Edit |
| [link] PAED-010 — Leading and operating within a clinical team | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-010 — Leading and operating within a clinical team"] Edit |
| [link] PAED-011 — Managing population health challenges | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-011 — Managing population health challenges"] Edit |
| [link] PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams"] Edit |
| [link] PAED-013 — Managing child protection, legal and ethical aspects of children at risk | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-013 — Managing child protection, legal and ethical aspects of children at risk"] Edit |
| [link] PAED-014 — Providing palliative and end-of-life care | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-014 — Providing palliative and end-of-life care"] Edit |
| [link] PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research"] Edit |
| [link] PAED-016 — Transitioning adolescents with long-term conditions to adult care | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active | [link] [aria-label="Edit PAED-016 — Transitioning adolescents with long-term conditions to adult care"] Edit |
| [link] EPA-001 — Clerk, assess, and present a general medical admission | Demo College | General Medicine / General Internal Medicine | Active | [link] [aria-label="Edit EPA-001 — Clerk, assess, and present a general medical admission"] Edit |
| ‹the other rows of the 40, past PagerControls› |
[aria-label="Pages of EPAs"]
[button] [aria-disabled="true"] Previous
Page 1 of ‹n›
[button] Next

######## BOARD R2-C16.dc.html
R2-C16 · 1280
Nolwazi Mbatha · the list loading: no Create EPA until the read is in
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] [aria-current="page"] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
### EPAs
The EPAs of the curricula Kgosi Kgari Teaching Hospital has adopted. The College's are read only; your institution's own are yours to change.
Loading.

######## BOARD R2-C16-390.dc.html
R2-C16-390 · 390
Nolwazi Mbatha · the list loading: no Create EPA until the read is in
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] [aria-current="page"] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
### EPAs
The EPAs of the curricula Kgosi Kgari Teaching Hospital has adopted. The College's are read only; your institution's own are yours to change.
Loading.

######## BOARD R2-C17.dc.html
R2-C17 · 1280
Nolwazi Mbatha · the list's load error
Focus: Try again moves the focus to the reloaded page's h1
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] [aria-current="page"] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
### EPAs
The EPAs of the curricula Kgosi Kgari Teaching Hospital has adopted. The College's are read only; your institution's own are yours to change.
Could not load the EPAs. Nothing has changed. Try again, or come back in a few minutes.
[button] Try again

######## BOARD R2-C17-390.dc.html
R2-C17-390 · 390
Nolwazi Mbatha · the list's load error
Focus: Try again moves the focus to the reloaded page's h1
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] [aria-current="page"] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
### EPAs
The EPAs of the curricula Kgosi Kgari Teaching Hospital has adopted. The College's are read only; your institution's own are yours to change.
Could not load the EPAs. Nothing has changed. Try again, or come back in a few minutes.
[button] Try again

######## BOARD R2-C18a.dc.html
R2-C18a · 1280
Nolwazi Mbatha · an EPA page loading (T7: titled "EPA" until read)
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] [aria-current="true"] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] EPAs
-
- [aria-current="page"] EPA
### EPA
Loading.

######## BOARD R2-C18a-390.dc.html
R2-C18a-390 · 390
Nolwazi Mbatha · an EPA page loading (T7: titled "EPA" until read)
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] [aria-current="true"] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] EPAs
-
- [aria-current="page"] EPA
### EPA
Loading.

######## BOARD R2-C18b.dc.html
R2-C18b · 1280
Nolwazi Mbatha · an EPA page's own load error (T329; not "unavailable")
Focus: Try again
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] [aria-current="true"] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] EPAs
-
- [aria-current="page"] EPA
### EPA
Could not load this EPA. Nothing has changed. Try again, or come back in a few minutes.
[button] Try again

######## BOARD R2-C18b-390.dc.html
R2-C18b-390 · 390
Nolwazi Mbatha · an EPA page's own load error (T329; not "unavailable")
Focus: Try again
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] [aria-current="true"] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] EPAs
-
- [aria-current="page"] EPA
### EPA
Could not load this EPA. Nothing has changed. Try again, or come back in a few minutes.
[button] Try again

######## BOARD R2-C19.dc.html
IllustrationR2-C19 · 1280
Illustration · a 150-character EPA title in the h1, the trail and the Title field
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] [aria-current="true"] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] EPAs
-
- [aria-current="page"] KGK-001
### KGK-001 — ‹a title of 150 characters, which wraps inside its cell and inside the h1 and is never cut, here written out to its full length so the wrap can be measured at every width›
Define the EPA code, scope, and descriptive guidance.
Sub-speciality*
[selected] Paediatrics / Paediatrics
The disciplines your institution has adopted.
Code*
[value="KGK-001"]
Title*
[value="‹a title of 150 characters, which wraps inside its cell and inside the h1 and is never cut, here written out to its full length so the wrap can be measured at every width›"]
Description
Plans and runs a monthly outreach clinic at a North West district hospital, with its referral follow-up.
Required knowledge and skills
Category*
Core
[selected] Elective
Status
(checkbox) [checked]
Active
An inactive EPA cannot be chosen by Kgosi Kgari Teaching Hospital's trainees for a new activity and is not a target on their running programmes' progress pages or dashboards; a programme that has ended keeps it on its record. Evidence already recorded against it is kept, and so is the progress it earned while it was active. An activity completed against it while it is inactive counts towards nothing until the EPA is reactivated, and reactivating it credits those activities.
[link] Cancel
[button] Deactivate…
[button] Save

######## BOARD R2-C19-390.dc.html
IllustrationR2-C19-390 · 390
Illustration · a 150-character EPA title in the h1, the trail and the Title field
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] [aria-current="true"] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] EPAs
-
- [aria-current="page"] KGK-001
### KGK-001 — ‹a title of 150 characters, which wraps inside its cell and inside the h1 and is never cut, here written out to its full length so the wrap can be measured at every width›
Define the EPA code, scope, and descriptive guidance.
Sub-speciality*
[selected] Paediatrics / Paediatrics
The disciplines your institution has adopted.
Code*
[value="KGK-001"]
Title*
[value="‹a title of 150 characters, which wraps inside its cell and inside the h1 and is never cut, here written out to its full length so the wrap can be measured at every width›"]
Description
Plans and runs a monthly outreach clinic at a North West district hospital, with its referral follow-up.
Required knowledge and skills
Category*
Core
[selected] Elective
Status
(checkbox) [checked]
Active
An inactive EPA cannot be chosen by Kgosi Kgari Teaching Hospital's trainees for a new activity and is not a target on their running programmes' progress pages or dashboards; a programme that has ended keeps it on its record. Evidence already recorded against it is kept, and so is the progress it earned while it was active. An activity completed against it while it is inactive counts towards nothing until the EPA is reactivated, and reactivating it credits those activities.
[link] Cancel
[button] Deactivate…
[button] Save

######## BOARD R2-D01.dc.html
R2-D01 · 1280
6.34 · Nolwazi Mbatha · Anele Dlamini saved on 11.1 with the curriculum untouched: Save profile asked nothing
Focus: "Trainee profile saved."
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] [aria-current="true"] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Trainees
-
- [aria-current="page"] Anele Dlamini's programme
### Anele Dlamini's programme
Update curriculum and completion details for this trainee.
[link] Export Anele Dlamini's portfolio
**Status
Active
**Email
dlamini@kgk.wombat.local
#### Profile details
Trainee profile saved.
Curriculum*
[selected] Paediatric EPA Curriculum (11.1)
Paediatric EPA Curriculum (11.2)
Anele Dlamini is on Paediatric EPA Curriculum (11.1), which your institution superseded on 2026-10-30. Keeping it keeps Anele Dlamini on 11.1.
Programme start date*
[value="2024-01-15"]
Expected completion date
[value="2028-07-14"]
Leave this empty to derive it from the curriculum window.
[link] Cancel
[button] Save profile
#### End this programme
As flow 13 built it: not redrawn.

######## BOARD R2-D01-390.dc.html
R2-D01-390 · 390
6.34 · Nolwazi Mbatha · Anele Dlamini saved on 11.1 with the curriculum untouched: Save profile asked nothing
Focus: "Trainee profile saved."
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] [aria-current="true"] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Trainees
-
- [aria-current="page"] Anele Dlamini's programme
### Anele Dlamini's programme
Update curriculum and completion details for this trainee.
[link] Export Anele Dlamini's portfolio
**Status
Active
**Email
dlamini@kgk.wombat.local
#### Profile details
Trainee profile saved.
Curriculum*
[selected] Paediatric EPA Curriculum (11.1)
Paediatric EPA Curriculum (11.2)
Anele Dlamini is on Paediatric EPA Curriculum (11.1), which your institution superseded on 2026-10-30. Keeping it keeps Anele Dlamini on 11.1.
Programme start date*
[value="2024-01-15"]
Expected completion date
[value="2028-07-14"]
Leave this empty to derive it from the curriculum window.
[link] Cancel
[button] Save profile
#### End this programme
As flow 13 built it: not redrawn.

######## BOARD R2-D01-768.dc.html
R2-D01-768 · 768
6.34 · Nolwazi Mbatha · Anele Dlamini saved on 11.1 with the curriculum untouched: Save profile asked nothing
Focus: "Trainee profile saved."
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] [aria-current="true"] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Trainees
-
- [aria-current="page"] Anele Dlamini's programme
### Anele Dlamini's programme
Update curriculum and completion details for this trainee.
[link] Export Anele Dlamini's portfolio
**Status
Active
**Email
dlamini@kgk.wombat.local
#### Profile details
Trainee profile saved.
Curriculum*
[selected] Paediatric EPA Curriculum (11.1)
Paediatric EPA Curriculum (11.2)
Anele Dlamini is on Paediatric EPA Curriculum (11.1), which your institution superseded on 2026-10-30. Keeping it keeps Anele Dlamini on 11.1.
Programme start date*
[value="2024-01-15"]
Expected completion date
[value="2028-07-14"]
Leave this empty to derive it from the curriculum window.
[link] Cancel
[button] Save profile
#### End this programme
As flow 13 built it: not redrawn.

######## BOARD R2-D02.dc.html
R2-D02 · 1280
6.36 · Nolwazi Mbatha · 11.2 chosen, before Save: the words under the picker change; the submit reads "Save profile and move…" (T2)
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] [aria-current="true"] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Trainees
-
- [aria-current="page"] Sipho Ndlovu's programme
### Sipho Ndlovu's programme
Update curriculum and completion details for this trainee.
[link] Export Sipho Ndlovu's portfolio
**Status
Active
**Email
ndlovu@kgk.wombat.local
#### Profile details
Curriculum*
Paediatric EPA Curriculum (11.1)
[selected] Paediatric EPA Curriculum (11.2)
When you save, Sipho Ndlovu moves to Paediatric EPA Curriculum (11.2). Sipho Ndlovu's completed activities are counted again against 11.2 in the same save, so Sipho Ndlovu's progress page reads 11.2's targets at once. Sipho Ndlovu cannot be moved back to 11.1 while 11.2 is the version your institution has adopted.
Programme start date*
[value="2026-01-15"]
Expected completion date
[value="2030-01-14"]
Leave this empty to derive it from the curriculum window.
[link] Cancel
[button] Save profile and move…
#### End this programme
As flow 13 built it: not redrawn.

######## BOARD R2-D02-390.dc.html
R2-D02-390 · 390
6.36 · Nolwazi Mbatha · 11.2 chosen, before Save: the words under the picker change; the submit reads "Save profile and move…" (T2)
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] [aria-current="true"] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Trainees
-
- [aria-current="page"] Sipho Ndlovu's programme
### Sipho Ndlovu's programme
Update curriculum and completion details for this trainee.
[link] Export Sipho Ndlovu's portfolio
**Status
Active
**Email
ndlovu@kgk.wombat.local
#### Profile details
Curriculum*
Paediatric EPA Curriculum (11.1)
[selected] Paediatric EPA Curriculum (11.2)
When you save, Sipho Ndlovu moves to Paediatric EPA Curriculum (11.2). Sipho Ndlovu's completed activities are counted again against 11.2 in the same save, so Sipho Ndlovu's progress page reads 11.2's targets at once. Sipho Ndlovu cannot be moved back to 11.1 while 11.2 is the version your institution has adopted.
Programme start date*
[value="2026-01-15"]
Expected completion date
[value="2030-01-14"]
Leave this empty to derive it from the curriculum window.
[link] Cancel
[button] Save profile and move…
#### End this programme
As flow 13 built it: not redrawn.

######## BOARD R2-D03.dc.html
R2-D03 · 1280
6.36 · Nolwazi Mbatha · Save profile and move… asks (Q8, Q20, M2)
Focus: Cancel; Cancel or Escape returns to "Save profile and move…"
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] [aria-current="true"] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Trainees
-
- [aria-current="page"] Sipho Ndlovu's programme
### Sipho Ndlovu's programme
Update curriculum and completion details for this trainee.
[link] Export Sipho Ndlovu's portfolio
**Status
Active
**Email
ndlovu@kgk.wombat.local
#### Profile details
Curriculum*
Paediatric EPA Curriculum (11.1)
[selected] Paediatric EPA Curriculum (11.2)
When you save, Sipho Ndlovu moves to Paediatric EPA Curriculum (11.2). Sipho Ndlovu's completed activities are counted again against 11.2 in the same save, so Sipho Ndlovu's progress page reads 11.2's targets at once. Sipho Ndlovu cannot be moved back to 11.1 while 11.2 is the version your institution has adopted.
Programme start date*
[value="2026-01-15"]
Expected completion date
[value="2030-01-14"]
Leave this empty to derive it from the curriculum window.
[link] Cancel
[button] Save profile and move…
#### End this programme
As flow 13 built it: not redrawn.
#### Move Sipho Ndlovu to Paediatric EPA Curriculum (11.2)?
Sipho Ndlovu's completed activities are counted again against 11.2 as the profile saves, and from then on they credit 11.2's items.
Sipho Ndlovu cannot be moved back to 11.1 while 11.2 is the version your institution has adopted.
[button] Cancel
[button] Move and save

######## BOARD R2-D03-390.dc.html
R2-D03-390 · 390
6.36 · Nolwazi Mbatha · Save profile and move… asks (Q8, Q20, M2)
Focus: Cancel; Cancel or Escape returns to "Save profile and move…"
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] [aria-current="true"] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Trainees
-
- [aria-current="page"] Sipho Ndlovu's programme
### Sipho Ndlovu's programme
Update curriculum and completion details for this trainee.
[link] Export Sipho Ndlovu's portfolio
**Status
Active
**Email
ndlovu@kgk.wombat.local
#### Profile details
Curriculum*
Paediatric EPA Curriculum (11.1)
[selected] Paediatric EPA Curriculum (11.2)
When you save, Sipho Ndlovu moves to Paediatric EPA Curriculum (11.2). Sipho Ndlovu's completed activities are counted again against 11.2 in the same save, so Sipho Ndlovu's progress page reads 11.2's targets at once. Sipho Ndlovu cannot be moved back to 11.1 while 11.2 is the version your institution has adopted.
Programme start date*
[value="2026-01-15"]
Expected completion date
[value="2030-01-14"]
Leave this empty to derive it from the curriculum window.
[link] Cancel
[button] Save profile and move…
#### End this programme
As flow 13 built it: not redrawn.
#### Move Sipho Ndlovu to Paediatric EPA Curriculum (11.2)?
Sipho Ndlovu's completed activities are counted again against 11.2 as the profile saves, and from then on they credit 11.2's items.
Sipho Ndlovu cannot be moved back to 11.1 while 11.2 is the version your institution has adopted.
[button] Cancel
[button] Move and save

######## BOARD R2-D04.dc.html
R2-D04 · 1280
6.36 · Nolwazi Mbatha · Cancel: closed, nothing saved, every field as typed
Focus: "Save profile and move…"
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] [aria-current="true"] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Trainees
-
- [aria-current="page"] Sipho Ndlovu's programme
### Sipho Ndlovu's programme
Update curriculum and completion details for this trainee.
[link] Export Sipho Ndlovu's portfolio
**Status
Active
**Email
ndlovu@kgk.wombat.local
#### Profile details
Curriculum*
Paediatric EPA Curriculum (11.1)
[selected] Paediatric EPA Curriculum (11.2)
When you save, Sipho Ndlovu moves to Paediatric EPA Curriculum (11.2). Sipho Ndlovu's completed activities are counted again against 11.2 in the same save, so Sipho Ndlovu's progress page reads 11.2's targets at once. Sipho Ndlovu cannot be moved back to 11.1 while 11.2 is the version your institution has adopted.
Programme start date*
[value="2026-01-15"]
Expected completion date
[value="2030-01-14"]
Leave this empty to derive it from the curriculum window.
[link] Cancel
[button] Save profile and move…
#### End this programme
As flow 13 built it: not redrawn.

######## BOARD R2-D04-390.dc.html
R2-D04-390 · 390
6.36 · Nolwazi Mbatha · Cancel: closed, nothing saved, every field as typed
Focus: "Save profile and move…"
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] [aria-current="true"] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Trainees
-
- [aria-current="page"] Sipho Ndlovu's programme
### Sipho Ndlovu's programme
Update curriculum and completion details for this trainee.
[link] Export Sipho Ndlovu's portfolio
**Status
Active
**Email
ndlovu@kgk.wombat.local
#### Profile details
Curriculum*
Paediatric EPA Curriculum (11.1)
[selected] Paediatric EPA Curriculum (11.2)
When you save, Sipho Ndlovu moves to Paediatric EPA Curriculum (11.2). Sipho Ndlovu's completed activities are counted again against 11.2 in the same save, so Sipho Ndlovu's progress page reads 11.2's targets at once. Sipho Ndlovu cannot be moved back to 11.1 while 11.2 is the version your institution has adopted.
Programme start date*
[value="2026-01-15"]
Expected completion date
[value="2030-01-14"]
Leave this empty to derive it from the curriculum window.
[link] Cancel
[button] Save profile and move…
#### End this programme
As flow 13 built it: not redrawn.

######## BOARD R2-D05.dc.html
R2-D05 · 1280
6.36 · Nolwazi Mbatha · Move and save: the built result; the picker read again offering 11.2 alone; journey 4 + 1 (390: 5 + 1)
Focus: the result
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] [aria-current="true"] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Trainees
-
- [aria-current="page"] Sipho Ndlovu's programme
### Sipho Ndlovu's programme
Update curriculum and completion details for this trainee.
[link] Export Sipho Ndlovu's portfolio
**Status
Active
**Email
ndlovu@kgk.wombat.local
#### Profile details
Trainee profile saved. 1 completion was checked against 11.2, and 1 counts towards it.
Curriculum*
[selected] Paediatric EPA Curriculum (11.2)
The version your institution has adopted for Paediatrics / Paediatrics.
Programme start date*
[value="2026-01-15"]
Expected completion date
[value="2030-01-14"]
Leave this empty to derive it from the curriculum window.
[link] Cancel
[button] Save profile
#### End this programme
As flow 13 built it: not redrawn.

######## BOARD R2-D05-390.dc.html
R2-D05-390 · 390
6.36 · Nolwazi Mbatha · Move and save: the built result; the picker read again offering 11.2 alone; journey 4 + 1 (390: 5 + 1)
Focus: the result
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] [aria-current="true"] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Trainees
-
- [aria-current="page"] Sipho Ndlovu's programme
### Sipho Ndlovu's programme
Update curriculum and completion details for this trainee.
[link] Export Sipho Ndlovu's portfolio
**Status
Active
**Email
ndlovu@kgk.wombat.local
#### Profile details
Trainee profile saved. 1 completion was checked against 11.2, and 1 counts towards it.
Curriculum*
[selected] Paediatric EPA Curriculum (11.2)
The version your institution has adopted for Paediatrics / Paediatrics.
Programme start date*
[value="2026-01-15"]
Expected completion date
[value="2030-01-14"]
Leave this empty to derive it from the curriculum window.
[link] Cancel
[button] Save profile
#### End this programme
As flow 13 built it: not redrawn.

######## BOARD R2-D05-768.dc.html
R2-D05-768 · 768
6.36 · Nolwazi Mbatha · Move and save: the built result; the picker read again offering 11.2 alone; journey 4 + 1 (390: 5 + 1)
Focus: the result
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] [aria-current="true"] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Trainees
-
- [aria-current="page"] Sipho Ndlovu's programme
### Sipho Ndlovu's programme
Update curriculum and completion details for this trainee.
[link] Export Sipho Ndlovu's portfolio
**Status
Active
**Email
ndlovu@kgk.wombat.local
#### Profile details
Trainee profile saved. 1 completion was checked against 11.2, and 1 counts towards it.
Curriculum*
[selected] Paediatric EPA Curriculum (11.2)
The version your institution has adopted for Paediatrics / Paediatrics.
Programme start date*
[value="2026-01-15"]
Expected completion date
[value="2030-01-14"]
Leave this empty to derive it from the curriculum window.
[link] Cancel
[button] Save profile
#### End this programme
As flow 13 built it: not redrawn.

######## BOARD R2-D06.dc.html
IllustrationR2-D06 · 1280
Illustration · a stale page offered a version no longer adopted: the refusal under Curriculum, the picker read again, every field kept
Focus: Curriculum (the refusal first in its aria-describedby)
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] [aria-current="true"] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Trainees
-
- [aria-current="page"] Sipho Ndlovu's programme
### Sipho Ndlovu's programme
Update curriculum and completion details for this trainee.
[link] Export Sipho Ndlovu's portfolio
**Status
Active
**Email
ndlovu@kgk.wombat.local
#### Profile details
Curriculum*
[selected] ‹the pin›
‹the active adoption›
A trainee can be moved only into the curriculum version this institution has adopted for their discipline, so nothing was saved.
The version your institution has adopted for Paediatrics / Paediatrics.
Programme start date*
[value="2026-01-15"]
Expected completion date
[value="2030-01-14"]
Leave this empty to derive it from the curriculum window.
[link] Cancel
[button] Save profile
#### End this programme
As flow 13 built it: not redrawn.

######## BOARD R2-D06-390.dc.html
IllustrationR2-D06-390 · 390
Illustration · a stale page offered a version no longer adopted: the refusal under Curriculum, the picker read again, every field kept
Focus: Curriculum (the refusal first in its aria-describedby)
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] [aria-current="true"] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Trainees
-
- [aria-current="page"] Sipho Ndlovu's programme
### Sipho Ndlovu's programme
Update curriculum and completion details for this trainee.
[link] Export Sipho Ndlovu's portfolio
**Status
Active
**Email
ndlovu@kgk.wombat.local
#### Profile details
Curriculum*
[selected] ‹the pin›
‹the active adoption›
A trainee can be moved only into the curriculum version this institution has adopted for their discipline, so nothing was saved.
The version your institution has adopted for Paediatrics / Paediatrics.
Programme start date*
[value="2026-01-15"]
Expected completion date
[value="2030-01-14"]
Leave this empty to derive it from the curriculum window.
[link] Cancel
[button] Save profile
#### End this programme
As flow 13 built it: not redrawn.

######## BOARD R2-D07a.dc.html
IllustrationR2-D07a · 1280
Illustration · the built result of a move with nothing to count
Focus: the result
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] [aria-current="true"] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Trainees
-
- [aria-current="page"] Sipho Ndlovu's programme
### Sipho Ndlovu's programme
Update curriculum and completion details for this trainee.
[link] Export Sipho Ndlovu's portfolio
**Status
Active
**Email
ndlovu@kgk.wombat.local
#### Profile details
Trainee profile saved. Sipho Ndlovu has no completions to count against 11.2.
Curriculum*
[selected] Paediatric EPA Curriculum (11.2)
The version your institution has adopted for Paediatrics / Paediatrics.
Programme start date*
[value="2026-01-15"]
Expected completion date
[value="2030-01-14"]
Leave this empty to derive it from the curriculum window.
[link] Cancel
[button] Save profile
#### End this programme
As flow 13 built it: not redrawn.

######## BOARD R2-D07a-390.dc.html
IllustrationR2-D07a-390 · 390
Illustration · the built result of a move with nothing to count
Focus: the result
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] [aria-current="true"] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Trainees
-
- [aria-current="page"] Sipho Ndlovu's programme
### Sipho Ndlovu's programme
Update curriculum and completion details for this trainee.
[link] Export Sipho Ndlovu's portfolio
**Status
Active
**Email
ndlovu@kgk.wombat.local
#### Profile details
Trainee profile saved. Sipho Ndlovu has no completions to count against 11.2.
Curriculum*
[selected] Paediatric EPA Curriculum (11.2)
The version your institution has adopted for Paediatrics / Paediatrics.
Programme start date*
[value="2026-01-15"]
Expected completion date
[value="2030-01-14"]
Leave this empty to derive it from the curriculum window.
[link] Cancel
[button] Save profile
#### End this programme
As flow 13 built it: not redrawn.

######## BOARD R2-D07b.dc.html
IllustrationR2-D07b · 1280
Illustration · the built result of a changed start date (T304's replay; no new rebuild on the profile, Q4)
Focus: the result
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] [aria-current="true"] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Trainees
-
- [aria-current="page"] Sipho Ndlovu's programme
### Sipho Ndlovu's programme
Update curriculum and completion details for this trainee.
[link] Export Sipho Ndlovu's portfolio
**Status
Active
**Email
ndlovu@kgk.wombat.local
#### Profile details
Trainee profile saved. ‹N› completions checked again from the new programme start date.
Curriculum*
[selected] Paediatric EPA Curriculum (11.2)
The version your institution has adopted for Paediatrics / Paediatrics.
Programme start date*
[value="‹a new start date›"]
Expected completion date
[value="2030-01-14"]
Leave this empty to derive it from the curriculum window.
[link] Cancel
[button] Save profile
#### End this programme
As flow 13 built it: not redrawn.

######## BOARD R2-D07b-390.dc.html
IllustrationR2-D07b-390 · 390
Illustration · the built result of a changed start date (T304's replay; no new rebuild on the profile, Q4)
Focus: the result
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] [aria-current="true"] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Trainees
-
- [aria-current="page"] Sipho Ndlovu's programme
### Sipho Ndlovu's programme
Update curriculum and completion details for this trainee.
[link] Export Sipho Ndlovu's portfolio
**Status
Active
**Email
ndlovu@kgk.wombat.local
#### Profile details
Trainee profile saved. ‹N› completions checked again from the new programme start date.
Curriculum*
[selected] Paediatric EPA Curriculum (11.2)
The version your institution has adopted for Paediatrics / Paediatrics.
Programme start date*
[value="‹a new start date›"]
Expected completion date
[value="2030-01-14"]
Leave this empty to derive it from the curriculum window.
[link] Cancel
[button] Save profile
#### End this programme
As flow 13 built it: not redrawn.

######## BOARD R2-E01.dc.html
R2-E01 · 1280
1.22 · Nolwazi Mbatha · two scales (CNSA not yet added); no Create scale, no actions column
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
### Entrustment scales
The rating ladders that assessment forms and committee reviews use. A Wombat administrator sets them; they are shown here for reference.
Entrustment scales, by name
| Name | Description | Levels |
| CPSA Paediatric Entrustment Scale v11.1 | Six rungs in four bands. Only level 3 is split: the gap between "check everything with me" and "call me if you are worried" is the whole of a registrar's third year. | 6 |
| O-R Scale | Standard 1-5 observation-to-entrustment scale. | 5 |

######## BOARD R2-E01-390.dc.html
R2-E01-390 · 390
1.22 · Nolwazi Mbatha · two scales (CNSA not yet added); no Create scale, no actions column
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
### Entrustment scales
The rating ladders that assessment forms and committee reviews use. A Wombat administrator sets them; they are shown here for reference.
Entrustment scales, by name
| Name | Description | Levels |
| CPSA Paediatric Entrustment Scale v11.1 | Six rungs in four bands. Only level 3 is split: the gap between "check everything with me" and "call me if you are worried" is the whole of a registrar's third year. | 6 |
| O-R Scale | Standard 1-5 observation-to-entrustment scale. | 5 |

######## BOARD R2-E02.dc.html
R2-E02 · 1280
6.9 · Nolwazi Mbatha · three scales; Q13's subtitle; Levels the count alone (S10); stacked at 768 and 390
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
### Entrustment scales
The rating ladders that assessment forms and committee reviews use. A Wombat administrator sets them; they are shown here for reference.
Entrustment scales, by name
| Name | Description | Levels |
| CNSA Neurology Entrustment Scale | Supervision ladder for CNSA EPAs. | 6 |
| CPSA Paediatric Entrustment Scale v11.1 | Six rungs in four bands. Only level 3 is split: the gap between "check everything with me" and "call me if you are worried" is the whole of a registrar's third year. | 6 |
| O-R Scale | Standard 1-5 observation-to-entrustment scale. | 5 |

######## BOARD R2-E02-390.dc.html
R2-E02-390 · 390
6.9 · Nolwazi Mbatha · three scales; Q13's subtitle; Levels the count alone (S10); stacked at 768 and 390
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
### Entrustment scales
The rating ladders that assessment forms and committee reviews use. A Wombat administrator sets them; they are shown here for reference.
Entrustment scales, by name
| Name | Description | Levels |
| CNSA Neurology Entrustment Scale | Supervision ladder for CNSA EPAs. | 6 |
| CPSA Paediatric Entrustment Scale v11.1 | Six rungs in four bands. Only level 3 is split: the gap between "check everything with me" and "call me if you are worried" is the whole of a registrar's third year. | 6 |
| O-R Scale | Standard 1-5 observation-to-entrustment scale. | 5 |

######## BOARD R2-E02-768.dc.html
R2-E02-768 · 768
6.9 · Nolwazi Mbatha · three scales; Q13's subtitle; Levels the count alone (S10); stacked at 768 and 390
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
### Entrustment scales
The rating ladders that assessment forms and committee reviews use. A Wombat administrator sets them; they are shown here for reference.
Entrustment scales, by name
| Name | Description | Levels |
| CNSA Neurology Entrustment Scale | Supervision ladder for CNSA EPAs. | 6 |
| CPSA Paediatric Entrustment Scale v11.1 | Six rungs in four bands. Only level 3 is split: the gap between "check everything with me" and "call me if you are worried" is the whole of a registrar's third year. | 6 |
| O-R Scale | Standard 1-5 observation-to-entrustment scale. | 5 |

######## BOARD R2-E03.dc.html
R2-E03 · 1280
Nolwazi Mbatha · loading
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
### Entrustment scales
The rating ladders that assessment forms and committee reviews use. A Wombat administrator sets them; they are shown here for reference.
Loading.

######## BOARD R2-E03-390.dc.html
R2-E03-390 · 390
Nolwazi Mbatha · loading
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
### Entrustment scales
The rating ladders that assessment forms and committee reviews use. A Wombat administrator sets them; they are shown here for reference.
Loading.

######## BOARD R2-E04.dc.html
R2-E04 · 1280
Nolwazi Mbatha · the load error
Focus: Try again moves the focus to the reloaded page's h1
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
### Entrustment scales
The rating ladders that assessment forms and committee reviews use. A Wombat administrator sets them; they are shown here for reference.
Could not load the entrustment scales. Nothing has changed. Try again, or come back in a few minutes.
[button] Try again

######## BOARD R2-E04-390.dc.html
R2-E04-390 · 390
Nolwazi Mbatha · the load error
Focus: Try again moves the focus to the reloaded page's h1
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
### Entrustment scales
The rating ladders that assessment forms and committee reviews use. A Wombat administrator sets them; they are shown here for reference.
Could not load the entrustment scales. Nothing has changed. Try again, or come back in a few minutes.
[button] Try again

######## BOARD R2-E05.dc.html
IllustrationR2-E05 · 1280
Illustration · the reader's empty state (the seed makes it unreachable)
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
### Entrustment scales
The rating ladders that assessment forms and committee reviews use. A Wombat administrator sets them; they are shown here for reference.
No scales yet
A Wombat administrator has not set up an entrustment scale yet.

######## BOARD R2-E05-390.dc.html
IllustrationR2-E05-390 · 390
Illustration · the reader's empty state (the seed makes it unreachable)
[link] Wombat
Acting asInstitutional admin
Menu
Acting asInstitutional admin
[aria-label="Main"]
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] EPAs
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
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
### Entrustment scales
The rating ladders that assessment forms and committee reviews use. A Wombat administrator sets them; they are shown here for reference.
No scales yet
A Wombat administrator has not set up an entrustment scale yet.

######## BOARD R2-F01.dc.html
R2-F01 · 1280
6.38 · Demo Administrator · the page: built subtitle and paragraphs; Rebuild progress primary, no "…" (M3, Q16)
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
[link] [aria-current="true"] Curricula
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
- [aria-current="page"] Curriculum progress
### Curriculum progress
Recalculate every trainee's curriculum targets from their completed activities.
Each completed activity is credited to the curriculum items it matches, in the semester that contains its encounter date. A rebuild throws the stored tallies away and credits every completed activity again, in the order the activities were completed.
Run it after changing where the semester boundary falls, after correcting a trainee's encounter dates, or when a trainee's figures look wrong. It is safe to run more than once. It runs as one transaction, so if it fails, nothing changes. It also runs by itself at startup whenever the stored tallies are empty but completions have credited.
A rebuild credits each activity against the curriculum as it is today: its items, targets and minimum levels. Only whether an EPA was active is judged as of when the activity was completed. An inactive EPA keeps the progress it earned while it was active. An activity completed against it since it was deactivated counts once the EPA is reactivated, and reactivating it credits those activities without a rebuild.
[button] Rebuild progress

######## BOARD R2-F01-390.dc.html
R2-F01-390 · 390
6.38 · Demo Administrator · the page: built subtitle and paragraphs; Rebuild progress primary, no "…" (M3, Q16)
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
[link] [aria-current="true"] Curricula
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
- [aria-current="page"] Curriculum progress
### Curriculum progress
Recalculate every trainee's curriculum targets from their completed activities.
Each completed activity is credited to the curriculum items it matches, in the semester that contains its encounter date. A rebuild throws the stored tallies away and credits every completed activity again, in the order the activities were completed.
Run it after changing where the semester boundary falls, after correcting a trainee's encounter dates, or when a trainee's figures look wrong. It is safe to run more than once. It runs as one transaction, so if it fails, nothing changes. It also runs by itself at startup whenever the stored tallies are empty but completions have credited.
A rebuild credits each activity against the curriculum as it is today: its items, targets and minimum levels. Only whether an EPA was active is judged as of when the activity was completed. An inactive EPA keeps the progress it earned while it was active. An activity completed against it since it was deactivated counts once the EPA is reactivated, and reactivating it credits those activities without a rebuild.
[button] Rebuild progress

######## BOARD R2-F02.dc.html
R2-F02 · 1280
6.38 · Demo Administrator · the dialog, as built
Focus: Cancel; Cancel or Escape returns to Rebuild progress
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
[link] [aria-current="true"] Curricula
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
- [aria-current="page"] Curriculum progress
### Curriculum progress
Recalculate every trainee's curriculum targets from their completed activities.
Each completed activity is credited to the curriculum items it matches, in the semester that contains its encounter date. A rebuild throws the stored tallies away and credits every completed activity again, in the order the activities were completed.
Run it after changing where the semester boundary falls, after correcting a trainee's encounter dates, or when a trainee's figures look wrong. It is safe to run more than once. It runs as one transaction, so if it fails, nothing changes. It also runs by itself at startup whenever the stored tallies are empty but completions have credited.
A rebuild credits each activity against the curriculum as it is today: its items, targets and minimum levels. Only whether an EPA was active is judged as of when the activity was completed. An inactive EPA keeps the progress it earned while it was active. An activity completed against it since it was deactivated counts once the EPA is reactivated, and reactivating it credits those activities without a rebuild.
[button] Rebuild progress
#### Rebuild curriculum progress?
Every trainee's curriculum progress is recalculated from their completed activities, each filed into the semester of its encounter date. An inactive EPA keeps the progress it earned while it was active. It is safe to run more than once, and if it fails nothing changes.
[button] Cancel
[button] Rebuild progress

######## BOARD R2-F02-390.dc.html
R2-F02-390 · 390
6.38 · Demo Administrator · the dialog, as built
Focus: Cancel; Cancel or Escape returns to Rebuild progress
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
[link] [aria-current="true"] Curricula
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
- [aria-current="page"] Curriculum progress
### Curriculum progress
Recalculate every trainee's curriculum targets from their completed activities.
Each completed activity is credited to the curriculum items it matches, in the semester that contains its encounter date. A rebuild throws the stored tallies away and credits every completed activity again, in the order the activities were completed.
Run it after changing where the semester boundary falls, after correcting a trainee's encounter dates, or when a trainee's figures look wrong. It is safe to run more than once. It runs as one transaction, so if it fails, nothing changes. It also runs by itself at startup whenever the stored tallies are empty but completions have credited.
A rebuild credits each activity against the curriculum as it is today: its items, targets and minimum levels. Only whether an EPA was active is judged as of when the activity was completed. An inactive EPA keeps the progress it earned while it was active. An activity completed against it since it was deactivated counts once the EPA is reactivated, and reactivating it credits those activities without a rebuild.
[button] Rebuild progress
#### Rebuild curriculum progress?
Every trainee's curriculum progress is recalculated from their completed activities, each filed into the semester of its encounter date. An inactive EPA keeps the progress it earned while it was active. It is safe to run more than once, and if it fails nothing changes.
[button] Cancel
[button] Rebuild progress

######## BOARD R2-F03.dc.html
R2-F03 · 1280
6.38 · Demo Administrator · running
Focus: kept on the button (aria-disabled, not disabled)
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
[link] [aria-current="true"] Curricula
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
- [aria-current="page"] Curriculum progress
### Curriculum progress
Recalculate every trainee's curriculum targets from their completed activities.
Each completed activity is credited to the curriculum items it matches, in the semester that contains its encounter date. A rebuild throws the stored tallies away and credits every completed activity again, in the order the activities were completed.
Run it after changing where the semester boundary falls, after correcting a trainee's encounter dates, or when a trainee's figures look wrong. It is safe to run more than once. It runs as one transaction, so if it fails, nothing changes. It also runs by itself at startup whenever the stored tallies are empty but completions have credited.
A rebuild credits each activity against the curriculum as it is today: its items, targets and minimum levels. Only whether an EPA was active is judged as of when the activity was completed. An inactive EPA keeps the progress it earned while it was active. An activity completed against it since it was deactivated counts once the EPA is reactivated, and reactivating it credits those activities without a rebuild.
[button] [aria-disabled="true"] Rebuilding…

######## BOARD R2-F03-390.dc.html
R2-F03-390 · 390
6.38 · Demo Administrator · running
Focus: kept on the button (aria-disabled, not disabled)
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
[link] [aria-current="true"] Curricula
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
- [aria-current="page"] Curriculum progress
### Curriculum progress
Recalculate every trainee's curriculum targets from their completed activities.
Each completed activity is credited to the curriculum items it matches, in the semester that contains its encounter date. A rebuild throws the stored tallies away and credits every completed activity again, in the order the activities were completed.
Run it after changing where the semester boundary falls, after correcting a trainee's encounter dates, or when a trainee's figures look wrong. It is safe to run more than once. It runs as one transaction, so if it fails, nothing changes. It also runs by itself at startup whenever the stored tallies are empty but completions have credited.
A rebuild credits each activity against the curriculum as it is today: its items, targets and minimum levels. Only whether an EPA was active is judged as of when the activity was completed. An inactive EPA keeps the progress it earned while it was active. An activity completed against it since it was deactivated counts once the EPA is reactivated, and reactivating it credits those activities without a rebuild.
[button] [aria-disabled="true"] Rebuilding…

######## BOARD R2-F04.dc.html
R2-F04 · 1280
6.38 · Demo Administrator · the result over the five figures; Stale tallies removed 0 (T304); journey 4 + 0 (390: 5 + 0), unchanged
Focus: the result
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
[link] [aria-current="true"] Curricula
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
- [aria-current="page"] Curriculum progress
### Curriculum progress
Recalculate every trainee's curriculum targets from their completed activities.
Each completed activity is credited to the curriculum items it matches, in the semester that contains its encounter date. A rebuild throws the stored tallies away and credits every completed activity again, in the order the activities were completed.
Run it after changing where the semester boundary falls, after correcting a trainee's encounter dates, or when a trainee's figures look wrong. It is safe to run more than once. It runs as one transaction, so if it fails, nothing changes. It also runs by itself at startup whenever the stored tallies are empty but completions have credited.
A rebuild credits each activity against the curriculum as it is today: its items, targets and minimum levels. Only whether an EPA was active is judged as of when the activity was completed. An inactive EPA keeps the progress it earned while it was active. An activity completed against it since it was deactivated counts once the EPA is reactivated, and reactivating it credits those activities without a rebuild.
[button] Rebuild progress
Curriculum progress was rebuilt.
**Activities re-read
15
**Curriculum items credited
14
**Semester tallies written
9
**Stale tallies removed
0
**Completions re-stamped
15

######## BOARD R2-F04-390.dc.html
R2-F04-390 · 390
6.38 · Demo Administrator · the result over the five figures; Stale tallies removed 0 (T304); journey 4 + 0 (390: 5 + 0), unchanged
Focus: the result
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
[link] [aria-current="true"] Curricula
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
- [aria-current="page"] Curriculum progress
### Curriculum progress
Recalculate every trainee's curriculum targets from their completed activities.
Each completed activity is credited to the curriculum items it matches, in the semester that contains its encounter date. A rebuild throws the stored tallies away and credits every completed activity again, in the order the activities were completed.
Run it after changing where the semester boundary falls, after correcting a trainee's encounter dates, or when a trainee's figures look wrong. It is safe to run more than once. It runs as one transaction, so if it fails, nothing changes. It also runs by itself at startup whenever the stored tallies are empty but completions have credited.
A rebuild credits each activity against the curriculum as it is today: its items, targets and minimum levels. Only whether an EPA was active is judged as of when the activity was completed. An inactive EPA keeps the progress it earned while it was active. An activity completed against it since it was deactivated counts once the EPA is reactivated, and reactivating it credits those activities without a rebuild.
[button] Rebuild progress
Curriculum progress was rebuilt.
**Activities re-read
15
**Curriculum items credited
14
**Semester tallies written
9
**Stale tallies removed
0
**Completions re-stamped
15

######## BOARD R2-F05.dc.html
IllustrationR2-F05 · 1280
Illustration · the failure (Q16): no exception's text; no Try again button, Rebuild progress stays (T8)
Focus: the alert
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
[link] [aria-current="true"] Curricula
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
- [aria-current="page"] Curriculum progress
### Curriculum progress
Recalculate every trainee's curriculum targets from their completed activities.
Each completed activity is credited to the curriculum items it matches, in the semester that contains its encounter date. A rebuild throws the stored tallies away and credits every completed activity again, in the order the activities were completed.
Run it after changing where the semester boundary falls, after correcting a trainee's encounter dates, or when a trainee's figures look wrong. It is safe to run more than once. It runs as one transaction, so if it fails, nothing changes. It also runs by itself at startup whenever the stored tallies are empty but completions have credited.
A rebuild credits each activity against the curriculum as it is today: its items, targets and minimum levels. Only whether an EPA was active is judged as of when the activity was completed. An inactive EPA keeps the progress it earned while it was active. An activity completed against it since it was deactivated counts once the EPA is reactivated, and reactivating it credits those activities without a rebuild.
[button] Rebuild progress
The rebuild failed, and nothing was changed. Try again, or come back in a few minutes.

######## BOARD R2-F05-390.dc.html
IllustrationR2-F05-390 · 390
Illustration · the failure (Q16): no exception's text; no Try again button, Rebuild progress stays (T8)
Focus: the alert
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
[link] [aria-current="true"] Curricula
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
- [aria-current="page"] Curriculum progress
### Curriculum progress
Recalculate every trainee's curriculum targets from their completed activities.
Each completed activity is credited to the curriculum items it matches, in the semester that contains its encounter date. A rebuild throws the stored tallies away and credits every completed activity again, in the order the activities were completed.
Run it after changing where the semester boundary falls, after correcting a trainee's encounter dates, or when a trainee's figures look wrong. It is safe to run more than once. It runs as one transaction, so if it fails, nothing changes. It also runs by itself at startup whenever the stored tallies are empty but completions have credited.
A rebuild credits each activity against the curriculum as it is today: its items, targets and minimum levels. Only whether an EPA was active is judged as of when the activity was completed. An inactive EPA keeps the progress it earned while it was active. An activity completed against it since it was deactivated counts once the EPA is reactivated, and reactivating it credits those activities without a rebuild.
[button] Rebuild progress
The rebuild failed, and nothing was changed. Try again, or come back in a few minutes.

######## BOARD R2-G01.dc.html
R2-G01 · 1280
6.37 and 6.39 · Sipho Ndlovu · My progress on 11.2: seventeen rows; PAED-002 "1 of 3 this semester"; PAED-016 and KGK-001 "0 of 1 in 2026"; PAED-011 "0 of 2 in 2026"; KGK-001's badge is T357's, as built
[link] Wombat
Acting asTrainee
Menu
Acting asTrainee
[aria-label="Main"]
-
[link] Home
-
[link] Log an activity
-
[link] My activities
-
[link] MSF reports
-
[link] My committee reviews
-
[link] Export portfolio
-
[link] [aria-current="page"] My progress
-
[link] My data rights
[link] Sipho Ndlovu
[button] Sign out
### My progress
Training year 1 · Semester 2, 2026
#### This period
0 of 10
EPAs met this semester
0 of 7
EPAs met in 2026
Semester 2, 2026 ends on 2026-11-30.
Training year 1 — it sets the minimum level each encounter is judged against.
Multi-source feedback: 0 of 17 EPAs covered by a released campaign that closed this semester. MSF is tracked on its own and counts towards no target.
#### Your EPAs
Each semester · 10 EPAs
| EPA | Semester 2, 2026 | Committee decides | STAR against training year 1 |
| [link] PAED-001 — Providing paediatric emergency care to children | 0 of 3 this semester3 more by 2026-11-30 | Decided each semester | No decision |
| [link] PAED-002 — Managing common paediatric presentations | 1 of 3 this semester2 more by 2026-11-30 | Decided each semester | No decision |
| [link] PAED-003 — Providing intensive care to children | 0 of 3 this semester3 more by 2026-11-30 | Decided once a year | No decision |
| [link] PAED-004 — Managing common neonatal conditions | 0 of 3 this semester3 more by 2026-11-30 | Decided each semester | No decision |
| [link] PAED-005 — Providing neonatal care in intensive and high-care settings | 0 of 3 this semester3 more by 2026-11-30 | Decided each semester | No decision |
| [link] PAED-006 — Managing long-term health conditions (LTHCs) in children | 0 of 2 this semester2 more by 2026-11-30 | Decided once a year | No decision |
| [link] PAED-007 — Maintaining and promoting the health and well-being of children | 0 of 1 this semester1 more by 2026-11-30 | Decided once a year | No decision |
| [link] PAED-010 — Leading and operating within a clinical team | 0 of 3 this semester3 more by 2026-11-30 | Decided each semester | No decision |
| [link] PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 0 of 3 this semester3 more by 2026-11-30 | Decided each semester | No decision |
| [link] PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 0 of 1 this semester1 more by 2026-11-30 | Decided once a year | No decision |
Once a year · 7 EPAs
| EPA | 2026 academic year | Committee decides | STAR against training year 1 |
| [link] KGK-001 — Running a paediatric outreach clinic at a district hospitalKgosi Kgari Teaching Hospital's own | 0 of 1 in 20261 more by 2026-11-30 | — | No decision |
| [link] PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 0 of 1 in 20261 more by 2026-11-30 | Decided as opportunity allows | No decision |
| [link] PAED-009 — Providing care for an adolescent patient | 0 of 1 in 20261 more by 2026-11-30 | Decided as opportunity allows | No decision |
| [link] PAED-011 — Managing population health challenges | 0 of 2 in 20262 more by 2026-11-30 | Decided once a year | No decision |
| [link] PAED-013 — Managing child protection, legal and ethical aspects of children at risk | 0 of 1 in 20261 more by 2026-11-30 | Decided as opportunity allows | No decision |
| [link] PAED-014 — Providing palliative and end-of-life care | 0 of 1 in 20261 more by 2026-11-30 | Decided once a year | No decision |
| [link] PAED-016 — Transitioning adolescents with long-term conditions to adult care | 0 of 1 in 20261 more by 2026-11-30 | Decided once a year | No decision |
The bars under each count and "Entrustment against Annexure A" follow as flow 05 built them: not redrawn (from capture 6.37-1).

######## BOARD R2-G01-390.dc.html
R2-G01-390 · 390
6.37 and 6.39 · Sipho Ndlovu · My progress on 11.2: seventeen rows; PAED-002 "1 of 3 this semester"; PAED-016 and KGK-001 "0 of 1 in 2026"; PAED-011 "0 of 2 in 2026"; KGK-001's badge is T357's, as built
[link] Wombat
Acting asTrainee
Menu
Acting asTrainee
[aria-label="Main"]
-
[link] Home
-
[link] Log an activity
-
[link] My activities
-
[link] MSF reports
-
[link] My committee reviews
-
[link] Export portfolio
-
[link] [aria-current="page"] My progress
-
[link] My data rights
[link] Sipho Ndlovu
[button] Sign out
### My progress
Training year 1 · Semester 2, 2026
#### This period
0 of 10
EPAs met this semester
0 of 7
EPAs met in 2026
Semester 2, 2026 ends on 2026-11-30.
Training year 1 — it sets the minimum level each encounter is judged against.
Multi-source feedback: 0 of 17 EPAs covered by a released campaign that closed this semester. MSF is tracked on its own and counts towards no target.
#### Your EPAs
Each semester · 10 EPAs
| EPA | Semester 2, 2026 | Committee decides | STAR against training year 1 |
| [link] PAED-001 — Providing paediatric emergency care to children | 0 of 3 this semester3 more by 2026-11-30 | Decided each semester | No decision |
| [link] PAED-002 — Managing common paediatric presentations | 1 of 3 this semester2 more by 2026-11-30 | Decided each semester | No decision |
| [link] PAED-003 — Providing intensive care to children | 0 of 3 this semester3 more by 2026-11-30 | Decided once a year | No decision |
| [link] PAED-004 — Managing common neonatal conditions | 0 of 3 this semester3 more by 2026-11-30 | Decided each semester | No decision |
| [link] PAED-005 — Providing neonatal care in intensive and high-care settings | 0 of 3 this semester3 more by 2026-11-30 | Decided each semester | No decision |
| [link] PAED-006 — Managing long-term health conditions (LTHCs) in children | 0 of 2 this semester2 more by 2026-11-30 | Decided once a year | No decision |
| [link] PAED-007 — Maintaining and promoting the health and well-being of children | 0 of 1 this semester1 more by 2026-11-30 | Decided once a year | No decision |
| [link] PAED-010 — Leading and operating within a clinical team | 0 of 3 this semester3 more by 2026-11-30 | Decided each semester | No decision |
| [link] PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 0 of 3 this semester3 more by 2026-11-30 | Decided each semester | No decision |
| [link] PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 0 of 1 this semester1 more by 2026-11-30 | Decided once a year | No decision |
Once a year · 7 EPAs
| EPA | 2026 academic year | Committee decides | STAR against training year 1 |
| [link] KGK-001 — Running a paediatric outreach clinic at a district hospitalKgosi Kgari Teaching Hospital's own | 0 of 1 in 20261 more by 2026-11-30 | — | No decision |
| [link] PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 0 of 1 in 20261 more by 2026-11-30 | Decided as opportunity allows | No decision |
| [link] PAED-009 — Providing care for an adolescent patient | 0 of 1 in 20261 more by 2026-11-30 | Decided as opportunity allows | No decision |
| [link] PAED-011 — Managing population health challenges | 0 of 2 in 20262 more by 2026-11-30 | Decided once a year | No decision |
| [link] PAED-013 — Managing child protection, legal and ethical aspects of children at risk | 0 of 1 in 20261 more by 2026-11-30 | Decided as opportunity allows | No decision |
| [link] PAED-014 — Providing palliative and end-of-life care | 0 of 1 in 20261 more by 2026-11-30 | Decided once a year | No decision |
| [link] PAED-016 — Transitioning adolescents with long-term conditions to adult care | 0 of 1 in 20261 more by 2026-11-30 | Decided once a year | No decision |
The bars under each count and "Entrustment against Annexure A" follow as flow 05 built them: not redrawn (from capture 6.37-1).
