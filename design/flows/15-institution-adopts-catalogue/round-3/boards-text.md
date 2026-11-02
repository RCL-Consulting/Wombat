# Flow 15 round 3: the text of every board

Generated from `round-3/R3-*.dc.html` (T392, build-contracts § 0) by a script, as flow 14's was: style, script, svg, head and helmet blocks stripped; headings, labels, aria-labels, placeholders, input values, alt text, `[checked]`, `[disabled]`, `[aria-disabled]`, `[aria-haspopup]` and annotations kept; a link is marked `[link]`, a button `[button]`. `round-3/` carries no `canvas.json`, so the order is by id: the Spec and Steps boards, then A, B, C, D, E, F, each at 1280 then its other widths. An `anno` chip is quoted after `>`. Design data: a caption, a chip, a label strip or a `DATA:` note is never a shipped word. A table row is one `| a | b |` line; a `dt` opens with `**`.

######## BOARD R3-0-spec.dc.html
### R3-0-spec · flow 15, round 3 (the correction round)
Round 3 redraws 42 of round 2's boards at every width round 2 drew them (42 at 1280, 42 at 390, 11 at 768), adds R3-B21 to B24 (1280 and 390) and R3-B08 and B17 at 1100 and 901: 107 screens, on the page "Round 3". Rounds 1 and 2 stay on their pages, untouched. One stylesheet, project/f15r3.css.
#### 1 · The screenshots
They arrived from the folder, not as attachments. No image reached this session with the message. The folder C:\dev\Wombat\design\upload stayed connected from round 2, so each capture was staged from baseline\ and viewed as an image.
Present: all 12 key captures, and the 3 for redrawn boards. Viewed in round 2 (7): 6.25-1, 6.32-1, 6.33-1, 6.38-4, 6.26-2, 6.34-3, 6.37-1. Viewed in round 3 (11): 1.19-2, 1.19-3, 1.20-1, 1.21-1, 1.21-3, 1.22-1, 6.26-3, 6.36-1, 6.22-2 (B02), 6.33-2 (B08), 6.38-3 (F02). Staged, not viewed: 1.19-1, 1.20-0, 1.20-2, 6.38-2, states/epa-edit--not-found. Not opened: the other 24 of § 4.
Boards that rest on FIXTURES alone (no capture of their state was viewed): A01, A02, B01, B06, C01, C02, E01, D05. B01, B06 and E01 can now be read against 1.21-3, 6.26-3 and 1.22-1, and A02 against 1.20-1. Their built words match.
##### What a capture shows that a board draws differently
Every difference found. None is drawn from a capture; each is today's page against the decided design.
| Capture | Today's page | The boards | Why |
| 6.26-3, 6.33-2 | KGK-001's own item is the FIRST row of the items table. | Last, after the PAED codes (R3-B06, B08, B10, B11, B13, B17, B21). | Round 2's ITEMS TABLE ("rows in EPA order, KGK-001 after the PAED codes"). A change from today: listed here so it is weighed. |
| 1.21-3, 6.22-2, 6.26-3, 6.33-2 | Minimum by training year one per line: "Year 1: 3a" … "Year 4: 5". | "3a / 3b / 4 / 5" (the folded cell, S16). | Decided (T10). The year labels go to the item's page. |
| 6.26-3, 6.33-2 | Row actions "Edit" and "Remove", with no "…". | "Edit" and "Remove…". | T3 and N6: Remove opens a dialog. |
| 6.26-3, 6.33-2 | After an add, the empty panel repeats the result: "Curriculum item added. Every EPA your institution could add …". | The result once, in the ActionResult; the empty line alone. | A defect today. Not repeated. |
| 1.19-2, 1.21-1 | Create EPA carries the plus icon; the table's caption is "Entrustable professional activities". | R3-C09 and C15 carry round 2's header action with no icon, and its caption "EPAs your institution reads" / "Every EPA". | No item changes these, so round 2's drawing is carried over. The icon is flagged for round 4 or the build: put the plus back as built. |
| 6.38-3 | The rebuild dialog: title, body, Cancel and Rebuild progress, word for word. | R3-F02 is the same. | No difference. |
| 6.22-2 | The paused notice and "(inactive: not in force)". | R2-B02's words are the same. | No difference. |
| 6.36-1 | The move's result: "Trainee profile saved. 1 completion was checked against 11.2, and 1 counts towards it." | R2-D05 is the same. | No difference. |
#### 2 · The render-check (S16, F7)
Method. Chromium (Playwright) loaded the R3 files with f15r3.css, the ds/wombat files and the Source Sans 3 faces. The items boards were rendered with the board's width freed (.cv-board width auto) at 1280, 1100 and 901, the sidebar rendering as it does at each width; every 390 board at 390 and every 768 board at 768. For each render it read the page's scrollWidth against its clientWidth and every .table-container's, and measured the box of every .btn, .form-control, .form-select, .form-check row, breadcrumb parent link and pager control in the page and its dialog.
The canvas needed one rule changed to render, and round 2's boards are affected. The built .sidebar carries grid-area: sidebar (MainLayout's), so round 2's .cv-shell grid, which named no areas, put the sidebar in an implicit column and the page in a 250 px track. Every round 2 board linking f15r2.css renders its page squeezed into 250 px beside the sidebar. f15r3.css K3 now reads: .cv-shell { display: grid; grid-template-areas: "sidebar main"; grid-template-columns: var(--sidebar-width) minmax(0, 1fr); } and .cv-shell .cv-main { grid-area: main; } (from 641 px). Round 2's boards are not touched, as asked: they render squeezed until they are moved onto f15r3.css.
Numbers. Spare = the table container's width minus the table's narrowest width (its columns at their floors).
| What | Result |
| Boards measured | 14 items boards × 3 widths (42 renders); 46 boards at 390; 11 at 768; the 4 boards at 1100 and 901 are the same pages as R3-B08 and B17. |
| Page sideways scroll | None at 1280, 1100, 901, 768 or 390: the page's scrollWidth equals its clientWidth on every render. |
| Items table at 1280 | Spare 398 px (15 national rows) / 369 px (with an own row or Edit/Remove on every row). |
| Items table at 1100 | Spare 218 px / 189 px. |
| Items table at 901 | Spare 19 px with national rows only (B01, B02, B03, B07, B14). −10 px on the nine boards with Edit and Remove… in a row (B06, B08, B10, B11, B13, B17, B20, B21, B23): their table container scrolls 10 px sideways. Its columns sit at their floors: EPA 96, Target and decision 80, Minimum level 80, Minimum by training year 81, Window and weight 72, Tools 96, the actions cell 98 (Remove… at its own width). |
| Fix for 901 (not drawn) | F7's first alternative: Window and weight move to the item's page. That frees the 72 px column, leaving about 62 px spare at 901. The second alternative is a named, focusable scroll container (.table-container tabindex="0" with a role="region" name, as MSF coverage does). Not hidden on any board. |
| Long values | "Decided each semester, by the Neonatal team Clinical Competency Committee" and the nine-tool lists wrap in their .col-wrap cells at every width; nothing breaks out. |
| 390 controls | 321 measured; 5 fall short of 44 px: "Add your institution's own item" is 36 px on R3-B03-390, B07-390 and B14-390 (.form-section-head's action has no 390 rule); the "Active" checkbox row on EPA's Status is 24 px on R3-C08-390 and C12-390 (.user-page makes .btn, .form-control, .form-select and a choice-group's radios 44 px, not a lone .form-check). Fix, not drawn: the items page's .form-section-head .btn and EpaEdit's .form-check row take 2.75rem below 641 px, in a page scope as flow 12 did. That is a new shipped rule each, so it is for the operator to weigh. The item form pages' checkbox rows (S1) measure 44 px. |
#### 3 · F1–F9, the operator's decisions
|  | Decision | Where |
| F1 | One-shot query tokens, built by CatalogueLinks (NEW, PROPOSED name): /admin/epas/{{id}}?created, /admin/curricula/{{Id}}/items?added={{itemId}}, ?saved={{itemId}}, ?back={{itemId}}. The arriving page re-reads, writes its own result ("EPA saved.", "Curriculum item added.", "Curriculum item saved."), focuses it (back: the row's Edit), then replaces the address without the token. A stale or foreign item id in a token writes nothing and focuses nothing. | R3-C08, B06, B10, B11, B23 (their labels name the token and that it was stripped); § 6 |
| F2 | Newer and Older are ordered by (EffectiveFrom, Id): a later clone is newer. The query and the dialog share the rule. | Spec § 6 (S2) |
| F3 | One item editor for every caller. CanEditCurriculum picks the words: the College and the Administrator get h1 "Add item", subtitle "On Paediatric EPA Curriculum (11.2). It applies to every institution that has adopted it.", and the national empty line. A national item's edit page is titled by its EPA. | R3-B22, B23, B24; R3-B17 |
| F4 | FieldRefusalException(field, message) (NEW, PROPOSED name), thrown where the commands throw today; the pages map its key to the field and aria-describedby. | Spec § 6 (S6); the refusal boards stand |
| F5 | CreateCurriculumCommand and Curriculum.CloneAsNewVersion create a version held back; the seeders open theirs explicitly. | Spec § 6 (S8); Steps 6.13, 6.29, 6.31 |
| F6 | A superseded version's items page: the superseded subtitle; KGK's own items keep Edit and Remove…; no Add. | R3-B21 |
| F7 | clinic-table--compact on every items table; measured at 1280, 1100 and 901. | Every R3 items board; § 2 |
| F8 | Words, never a badge, on the admin lists and pages. The trainee's EPA page h1 and My progress keep the badge until T357. | Spec § 7 (S18); no board |
| F9 | CreateEpa refuses an own EPA in an unadopted discipline; UpdateEpa only when the save changes the sub-speciality; an own EPA left in a discipline no longer adopted keeps Edit. | Spec § 6 (S5) |
#### 4 · The checklist, item by item
Done = drawn as the item says. Spec = written here, no board.
| Item | Done | Boards or where |
| M1 actions cell | Done: every row-action cell is <td><div class="actions-cell">…</div></td>, and "Set by the College." sits in the same div. No td.actions-cell on any R3 board (checked by search). | R3-A01–A13, B06, B08, B10, B11, B13, B17, B20, B21, B23, C09, C15 and their twins |
| M2 A11's count | Done: All curricula reads 11.2 at 16, 11.1 at 16. | R3-A11, A11-390 |
| M3 the College's item pages | Done | R3-B22, B23, B24 (1280 and 390) |
| M4 Steps 1.17 and the Act 6 Outcome | Done | R3-0-steps |
| M5 "EPA saved." by ?created | Done: the label says so; the EpaEdit row in § 6 lists it | R3-C08 |
| M6 the item pages' return | Done | R3-B06 (?added), B10 (?saved), B11 (?back), B23 (?added); § 6 |
| S1 supersession rules | Done: A08 counts one registrar, has no own-items line and keeps no bracket | R3-A08; § 6 |
| S2 tie rule | Spec | § 6 |
| S3 InstitutionId | Spec | § 6 |
| S4 Create EPA flag | Spec | § 6 |
| S5 F9 | Spec | § 6 |
| S6 F4 | Spec | § 6 |
| S7 load errors | Done: an ActionResult holding the Alert danger, its alert-row with Try again; the h1 tabindex="-1" | R3-A15, B19, C17, C18b, E04; § 6 |
| S8 F5 | Spec and Steps | § 6; Steps T3 |
| S9 the rebuild's failure focus | Spec (R2-F05 stands) | § 6 |
| S10 the em dash | Spec | § 6 |
| S11 Deactivate… | Spec and Steps | § 6; Steps T4 |
| S12 the superseded version's items | Done | R3-B21; § 6 |
| S13 ConfirmDialog action mode | Done: dialog.dialog-scrim.dialog-card with aria-labelledby and aria-describedby; .form-container; h2; one paragraph p#dlg-b (B13, C12) or div#dlg-b.dialog-body (A03, A08, A11, D03, F02); Cancel autofocus first; no .dialog-form | R3-A03, A08, A11, B13, C12, D03, F02 |
| S14 PagerControls | Done: .pager-info "1–‹n› of 40", .pager-actions, .pager-page-size with "EPAs per page" | R3-C15 |
| S15 one name a link | Done | R3-A10, A11 |
| S16 the items table measured | Done, with the 901 finding | § 2 |
| S17 DESIGN.md lines | Spec | § 7 |
| S18 F8's scope | Spec | § 7 |
| S19 loading lines | Done: one p.loading-line role="status" a page | R3-A14, B18, C16, C18a, E03 |
| S20–S24 | Steps | R3-0-steps T5–T7 |
| S25 the captures paragraph | Done | § 1 |
| N1 B17's brackets | Done: the College's notice and the national empty line | R3-B17 |
| N2 All curricula's caption | Done | R3-A09, A10, A11 |
| N3 "For this institution" and the intro | Done | R3-A10, A11 |
| N4 no role on standing alerts | Done | every redrawn items notice |
| N5 StatePanel's empty | Done: div.detail-card--empty tabindex="-1" with div.state-panel-title and div.state-panel-copy | R3-A01 |
| N6 aria-haspopup="dialog" | Done: Adopt…, Remove…, Deactivate… | every redrawn trigger |
| N7 Breadcrumbs.razor's markup | Not done as asked. Breadcrumbs.razor is not in the connected folder (only wwwroot is), so the trail is round 2's markup from bundle.css's classes: li.breadcrumb-ancestor / -parent / -current, li.breadcrumb-separator aria-hidden, aria-current="page" on the current li. Check it against the component. | every redrawn trail |
| N8 "scratch" in the label | Done | R3-B10, B11, B14; Steps T7 (B09) |
| N9 one .user-page-scoped rule | Done: .user-page.curriculum-item-page .form-check (and its input and label). It renders the same: the checkbox rows measure 44 px at 390 on R3-B22 and B24. | f15r3.css PART 2 |

######## BOARD R3-0-spec-2.dc.html
### R3-0-spec-2 · the changes as code, the DESIGN.md text, what stands
#### 5 · R2 boards that stand (not redrawn)
R2-B04, B05, B09, B12, B15, B16, C01–C07, C10, C11, C13, C14, C19, D01, D02, D04, D05, D06, D07a, D07b, E01, E02, E05, F01, F03, F04, F05, G01, with their 390 and 768 twins. They stand as drawn, but § 2's grid finding applies to them: they render the page squeezed until moved to f15r3.css. Their row-action cells carry round 2's td.actions-cell where they have one (C02 to C07: C02 only lists "Set by the College." spans).
Illustrations among the R3 boards: R3-A07, A08, A11, A13, B20, C15. None new: B21–B24 are played states (6.38's read; 6.30).
Brackets kept: ‹item id›, which the replay prints nowhere; C15's ‹institution›, ‹code›, ‹title› and ‹n› (page size), since FIXTURES give no second institution; B20's ‹code n›, ‹title›, ‹target›, ‹decision›, ‹rung›, ‹tools›; A13's ‹date›.
#### 6 · The changes the boards need, as code changes (round 2's list restated whole, with round 3's)
| Where | Change | Status |
| NavItems, NavOwners | CurriculumAdoptions leaves the Institutional admin's group; /admin/adoptions redirects to /admin/curricula, which needs a page type for NavItems/NavOwners and a policy that sends a visitor to sign-in (NavItems.cs:113); OnboardingLinks.CurriculumAdoptions re-pointed and renamed; the four sentences re-pointed; DESIGN.md's NavMenu table loses the row. | PROPOSED, decided Q1 |
| CatalogueLinks (NEW, PROPOSED name) | Builds /admin/epas/{{Id:int}}?created and /admin/curricula/{{Id:int}}/items?added={{ItemId}}, ?saved={{ItemId}}, ?back={{ItemId}}, as OnboardingLinks.Admitted and BackFromAdmit do (OnboardingLinks.cs:43–61). One-shot: the page reads it, writes the result, focuses it, then replaces the address without the token, so a reload says nothing; it survives EpaEdit's forceLoad (EpaEdit.razor:257); a stale or foreign item id writes and focuses nothing; a bUnit test reads it. | PROPOSED, decided F1 |
| GetCurriculaListQuery | Takes an InstitutionId (null: the Institutional admin's own; required for the Administrator, from Show). Per version: AdoptedOn of the active adoption; SupersededOn = the AdoptedOn of the adoption that followed the version's latest inactive adoption, ordered by (AdoptedOn, Id), since both are dated D and going back adds a second row; the registrar count = ACTIVE TraineeProfiles with its InstitutionId and CurriculumId (TraineeProfile.cs:13–26, :41), never the nullable AdoptionId; the own-item count. CurriculumAdminScope's second arm: a profile (active or ended) with that InstitutionId and CurriculumId, or an own item (Q9). National items counted only on the open rows (GetCurricula.cs:12, :28–41). | PROPOSED, decided Q9, Q10, Q18, S1, S3 · DATA |
| GetAdoptableCurriculaQuery | Takes the InstitutionId; open versions not ACTIVELY adopted (today :45 lists the adopted one); EffectiveFrom, item count, relation newer / older / none by (EffectiveFrom, Id) (F2); for the dialog, the superseded version, its active-registrar count (one: "The registrar on 11.2 stays on it until moved on their profile."; two or more: "The ‹N› registrars on … stay on it until each is moved on their profile."; none: left out) and the own items it holds that the other version does not (none: the line left out). Adoption parts by AdministratorOrInstitutionalAdmin (S5 of round 2). | PROPOSED, decided Q6, Q7, F2, S1, S3 · DATA |
| AdoptCurriculumCommand | Refuses a held-back version before writing (AdoptCurriculum.cs:47–52); AdoptedOn by the handler's TimeProvider on the South African day, a test pinning 22:30 UTC (:77); going back accepted (:54–66). | PROPOSED, decided Q5, T325 |
| CreateCurriculumCommand, Curriculum.CloneAsNewVersion | Create a version held back (today Curriculum.IsActive defaults to true and the clone sets it, Curriculum.cs:19, :35); the seeders set their curricula open explicitly, so no seeded value changes. | PROPOSED, decided F5 |
| EpaDto / ListEpasForSubSpecialityQuery; EpasList | The owner's name; "may change" from UpdateEpa's rule (UpdateEpa.cs:98–104), never roles; each row's discipline from the DTO (EpasList.razor:39–40); Create EPA flag: true for the Administrator and a College admin, for an Institutional admin only with an actively adopted discipline (CreateEpa.cs:66–80). EpasList drops Speciality and merges Code and Title (EpasList.razor:28–38). Stops printing exception.Message and passes OnRetry (EpasList.razor:77). | PROPOSED, decided Q3, Q12, S4, S7 |
| GetEpaByIdQuery | Null outside the list's rule (GetEpas.cs:178–186): Unavailable. "May change" chooses form or record. | PROPOSED, decided Q2, Q19 |
| CreateEpaCommand, UpdateEpaCommand | CreateEpa refuses an own EPA in a discipline not actively adopted; UpdateEpa refuses only when the save changes the sub-speciality (F9); a duplicate only on the unique index (T272); refusals thrown as FieldRefusalException("SubSpecialityId" / "Code", …). DeactivateEpaCommand unchanged. | PROPOSED, decided Q12, F9, F4 |
| FieldRefusalException (NEW, PROPOSED name) | An Application exception carrying the field's key and the sentence, thrown where CreateEpa, UpdateEpa, AddCurriculumItem, UpdateCurriculumItem and the profile's move throw today, so the audit pipeline still discards the refused save (T201). The pages map the key to the field and its aria-describedby: EpaEdit.razor:275, CurriculumItemsEdit.razor:625 and :734, TraineeProfileEdit.razor:496–506. Anything without a key stays in the alert. | PROPOSED, decided F4, S6 |
| EpaEdit | Titled by the EPA (EpaOptionLabel), crumb its code, no Back to EPAs; the create branch navigates to CatalogueLinks' ?created and the page writes "EPA saved." from it; Q11's help and dialog by owner; "Deactivate…" renames the built "Deactivate" (EpaEdit.razor:96), opening its dialog with aria-haspopup; a LoadError with OnRetry (EpaEdit.razor:33, :204–207); titled "EPA" while loading or failed (T7). | PROPOSED, decided Q11, F1, S11 · PROPOSED (THIS ASK) T7 |
| GetCurriculumByIdQuery; CurriculumItemsEdit; the item routes | The version's state for the caller's institution (adopted on; or superseded on, by, and the count) and a may-add flag (S12, F6); null out of scope (Unavailable). Routes /admin/curricula/{{Id:int}}/items/new and …/items/{{ItemId:int}}, NavOwners lighting Curricula; the in-place edit row and its colspan form go for every caller, the College's and the Administrator's too (F3, T323); the page reads ?added, ?saved and ?back (F1). Words by CanEditCurriculum (below). clinic-table--compact on the items table (F7). The EPA label's em dash at CurriculumItemsEdit.razor:111, the Remove body at :769 and the legend (EpaOptionLabel.cs:20 has it, S10). Stops printing exception.Message, passes OnRetry (:590). | PROPOSED, decided Q14, F1, F3, F6, F7, S10, S12 · PROPOSED (THIS ASK) T4, T5 |
| GetTraineeCurriculumChoicesQuery; TraineeProfileEdit | Each option's state for the line under the picker; the line (.submit-check; role="status" if the build finds it silent); "Save profile and move…" when the picker moved; the ConfirmDialog gates in SaveAsync (OnSubmit="SaveAsync", TraineeProfileEdit.razor:390), so Enter in a date field asks too; D06's "Curricula" link is a markup change at :441–443; no new count read (Q20). | PROPOSED, decided Q8 · PROPOSED (THIS ASK) T2 |
| CurriculumProgressRebuild | The failure's fixed words (today :13, :116 print the exception); the failure Alert inside an ActionResult that takes the focus (today :11–14, :46–48 leave it on the button). The trigger stays primary. | PROPOSED, decided Q16, S9 · PROPOSED (THIS ASK) T8 |
| EntrustmentScalesList, CurriculaList | Q13's subtitle to a reader; the reader's empty state; LoadError with OnRetry instead of exception.Message (EntrustmentScalesList.razor:94, CurriculaList.razor:104). | PROPOSED, decided Q13, S7 |
| The headings | Every <h3> section an <h2>; the Administrator's KGK sections h3 under "An institution's curricula". | PROPOSED, decided |
##### The item pages' words, by CanEditCurriculum (F3, M3)
| Part | The Institutional admin (own items) | The College admin and the Administrator |
| Add page h1 | Add your institution's own item | Add item (built, CurriculumItemsEdit.razor:329) |
| Add page subtitle | On Paediatric EPA Curriculum (11.1). It applies only to Kgosi Kgari Teaching Hospital's trainees. | On Paediatric EPA Curriculum (11.2). It applies to every institution that has adopted it. NEW |
| Edit page h1 | Edit KGK-001 — Running a paediatric outreach clinic at a district hospital | Edit PAED-011 — Managing population health challenges |
| Edit page subtitle | Your institution's own item on Paediatric EPA Curriculum (11.2) | On Paediatric EPA Curriculum (11.2). It applies to every institution that has adopted it. NEW |
| List action | Add your institution's own item | Add item |
| List empty line | Every EPA your institution could add is already on this curriculum: … | Every national EPA of Paediatrics is already on this curriculum, as a national item or as an institution's own item, … (built, :1139–1140) |
| List notice | The College sets this curriculum's national items, so they are read only here. … | A target and its period are read live. Changing one changes what every trainee on this curriculum is measured against, in every institution that has adopted it, … (built, :44–49) |
R3-B22 draws Minimum by training year's four filled rows as labelled selects ("Year 1" … "Year 4"). No capture shows a filled row, since 6.26-2 shows only the empty state, so the built row's markup is to be checked.
#### 7 · The DESIGN.md text each changes
| Section | Change |
| § Table system, T211 paragraph | The own EPA is marked as the own item is, on the EPA list too: words in .muted .text-sm under the title; a national row's "Set by the College."; F8: "words, never a badge, on the admin lists and pages". The trainee's EPA page h1 (DESIGN.md:3776–3777) and My progress keep the badge until T357. |
| § Table system, "Editing a row in place" (DESIGN.md:896 ff) and the edit row in the row-actions and picker paragraphs | False after T323: no list edits a row in place; an item is added and edited on a page of its own, for every caller. |
| The picker rule (§ Form system's picker paragraph) | Adoption is by rows: an adopted version is a row with its state, never an option. |
| § Button system | Superseding adoption and the move are confirmed; Deactivate… and Remove… open their dialogs (aria-haspopup); the rebuild stays primary. |
| § Form system and the Form page template (:3959) | Refusals under their field on the EPA, item and profile pages (FieldRefusalException), first in aria-describedby; the template's ValidationSummary amended for those three pages only. |
| § Page-level patterns | An EPA titled by the EPA; a national EPA a record; Unavailable gains EPA, Curriculum and Item; the items list is the page and the item's form a page of its own; T7's titles. |
| § The NavMenu's table (:587), and :3985, :4441 | "Curriculum adoptions" gone from all three. |

######## BOARD R3-0-spec-3.dc.html
### R3-0-spec-3 · messages, classes, edge cases
#### 8 · Every message by kind (round 2's table restated, with round 3's)
Built = in the code today; R1 / R2 = drawn in that round and kept; NEW = this round's, beside what it replaces.
| Kind | Words | Source | Board |
| result | Paediatric EPA Curriculum (11.1) adopted. New admissions to Paediatrics / Paediatrics join it. | R1 (replaces "Curriculum adopted.") | R3-A02 |
| result | Paediatric EPA Curriculum (11.2) adopted. … its 3 registrars stay on it until each is moved. | R1 | R3-A05 |
| refusal | This curriculum version is not open for adoption. Its College has held it back, so nothing was adopted. | R1 (Q5) | R3-A07 |
| confirmation | Adopt Paediatric EPA Curriculum (11.2) for Kgosi Kgari Teaching Hospital? … The 3 registrars on 11.1 stay on it … KGK-001, is not carried over … | R2 | R3-A03, A11 |
| confirmation | "The registrar on 11.2 stays on it until moved on their profile." (one registrar) | NEW (S1; replaces "The ‹N› registrars on 11.2 stay on it …" for a count of one; the plural stays for two or more) | R3-A08 |
| notice | Adopting a version makes it the one that new admissions to its discipline join. Adopting another version of a discipline the institution has adopted supersedes the current one: … the institution's own items are not carried over. | NEW (N3; the Administrator's form of "… your institution has adopted … your institution's own items …") | R3-A10, A11 |
| column | For this institution | NEW (N3; replaces "For your institution" in the Administrator's section) | R3-A10, A11 |
| caption | Every College's curriculum versions, with the College's status. Items counts each institution's own items too. | NEW (N2; replaces "Every College's curriculum versions, with the College's status") | R3-A09, A10, A11 |
| link name | Items of Paediatric EPA Curriculum (11.2) for Kgosi Kgari Teaching Hospital (and for 11.1) | NEW (S15; replaces "Items of Paediatric EPA Curriculum (11.2)" in the institution section) | R3-A10, A11 |
| loading | Loading the curricula. / Loading this curriculum's items. / Loading the EPAs. / Loading this EPA. / Loading the entrustment scales. | NEW (S19; replaces round 2's "Loading.") | R3-A14, B18, C16, C18a, E03 |
| subtitle | On Paediatric EPA Curriculum (11.2). It applies to every institution that has adopted it. | NEW (F3) | R3-B22, B24 |
| subtitle | College of Paediatricians of South Africa · Paediatrics / Paediatrics · effective 2026-01-01 · superseded on 2026-10-30 by Paediatric EPA Curriculum (11.2); 2 registrars still on it | NEW (F6; replaces "… · adopted by your institution on 2026-10-30" for a superseded version) | R3-B21 |
| hint | Your institution has superseded this version, so no item is added to it. Its own items stay for the registrars still on it. | NEW PROPOSED (S12; in place of the add action) | R3-B21 |
| label | EPAs per page | NEW PROPOSED (S14) | R3-C15 |
| notice | A target and its period are read live. Changing one changes … in every institution that has adopted it, … | built (:44–49) | R3-B17, B23 |
| empty | Every national EPA of Paediatrics is already on this curriculum, as a national item or as an institution's own item, … | built (:1139–1140) | R3-B17, B23 |
| notice | The items page's standing notice; the paused notice; "(inactive: not in force)" | built | R3-B01, B02 … |
| hint | A new version carries no institution's own items. Your item on 11.1 stays there. | R1 (B1) | R3-B07, B14 |
| result | Curriculum item added. / Curriculum item saved. / KGK-001 removed from this curriculum. | built | R3-B06, B08, B10, B14, B23 |
| confirmation | Remove KGK-001 from Paediatric EPA Curriculum (11.2)? with the built body, em dash | R2 PROPOSED (THIS ASK) T3 (replaces "Remove this item?") | R3-B13 |
| failure | Could not load the curricula. / this curriculum's items. / the EPAs. / this EPA. / the entrustment scales. — "Nothing has changed. Try again, or come back in a few minutes." | R1 | R3-A15, B19, C17, C18b, E04 |
| result | EPA saved. (by ?created) / EPA deactivated. / EPA reactivated. … | built | R3-C08 |
| confirmation | Deactivate KGK-001? It can no longer be chosen … for Kgosi Kgari Teaching Hospital's trainees. … | R2 (Q11; replaces "Deactivate this EPA?") | R3-C12 |
| confirmation | Move Sipho Ndlovu to Paediatric EPA Curriculum (11.2)? Sipho Ndlovu's completed activities are counted again … cannot be moved back to 11.1 while 11.2 is the version your institution has adopted. | R2 | R3-D03 |
| confirmation | Rebuild curriculum progress? (built body) | built | R3-F02 |
| subtitle, empty, etc. | Round 2's other words (Curricula's subtitles, the "Open for adoption" lines, Q12's empty state, the Sub-speciality help and refusal, the move's line under the picker, Q13's subtitle, the scales' empty state, the rebuild's failure) | R2 as listed on R2-0-spec-2 § 8 | the boards that stand and R3 |
#### 9 · The class table
Every class an R3 board uses. Reuses = bundle.css as shipped; nothing redeclared.
| Class | Kind | Where |
| PageHeader, Breadcrumbs (see N7), DataTable with StackWide and clinic-table--compact (F7), .col-wrap, .col-wrap--wide, .col-fit, .visually-hidden | reuses | every list |
| div.actions-cell inside a plain td (M1) | reuses | every row-action cell |
| .muted .text-sm | reuses | the owner and pause marks, a row's second line |
| .form-section-head | reuses | "Items (n)" with its action |
| .user-page, .user-section, dl.detail-list | reuses | the record, the form pages, Unavailable, the profile, the rebuild page |
| .submit-check | reuses | the move's line |
| dl.details-list | reuses | the rebuild's figures |
| .form-container (inside a dialog in action mode, S13), .form-actions | reuses | every dialog and form |
| .dialog-scrim, .dialog-card, .dialog-body, .dialog-body--scroll | reuses | ConfirmDialog in action mode |
| .action-result, .alert(-*), .alert-row, .alert-row-text | reuses | results and load errors (S7) |
| .loading-line (role="status"), .skeleton, .skel-title, .skel-line, .skel-short | reuses | loading (S19) |
| .detail-card--empty (tabindex -1), .state-panel-title, .state-panel-copy (divs) | reuses | StatePanel empty (N5) |
| .pager, .pager-info, .pager-actions, .pager-page-size, .pager-page-size-label, .pager-page-size-select, .form-select-sm | reuses | PagerControls (S14) |
| .search-container, .search-grid, .search-field, .filter-actions | reuses | the Administrator's Show |
| the form classes (.form-group, .form-grid, .form-control, .form-select, .form-check, .check-grid, .field-help, .validation-message, .required-mark) | reuses | the form pages |
| the shell's classes | reuses | flow 01's shell |
| .curriculum-item-page | NEW shipped, f15r3.css PART 2 | the item form pages at 390 (N9's one rule, below) |
| .cv-board, .cv-w1280 / 1100 / 901 / 768 / 390, .cv-label*, .cv-chip, .cv-stage, .cv-shell, .cv-main, .cv-topbar, .cv-article, .cv-overlay, .cv-note, .cv-elide, .cv-doc, .cv-tag, .cv-legend | canvas only, f15r3.css PART 1 (K1–K9) | never shipped; K1 gains 1100 and 901; K3 names its grid areas (§ 2) |
f15r3.css PART 2, as saved (N9): @media (max-width: 640.98px) { .user-page.curriculum-item-page .form-check { min-height: 2.75rem; } .user-page.curriculum-item-page .form-check-input { height: 1.25rem; width: 1.25rem; } .user-page.curriculum-item-page .form-check label { align-items: center; display: inline-flex; flex: 1; min-height: 2.75rem; } }
#### 10 · Edge cases (round 2's, standing)
- A second discipline: R3-A13. A version held back after adoption: kept, row unchanged. A superseded version with no registrar (active or ended) and no own item: leaves the list.
- Going back: R3-A08 (one registrar; no own-items line, since 11.1 already holds KGK-001). Two adoptions of 11.1 on one day: ordered by (AdoptedOn, Id) (S1).
- An own EPA in a discipline no longer adopted: keeps Edit; a save that changes its sub-speciality to an unadopted one is refused (F9).
- Two institutions' own EPAs with one code: R3-C15. A 150-character title: R2-C19 (stands; written out at length when the build tests it).
- 11.3 superseding 11.2 while a registrar is on 11.1: the picker offers 11.1 and 11.3. An adoption between 00:00 and 02:00 SAST: dated the South African day. A move while a completion credits elsewhere: the save waits (T304).
- D07a's "no completions to count" brackets the person in the build test (‹name›): Sipho Ndlovu's own 6.36 move counts 1.

######## BOARD R3-0-steps.dc.html
### R3-0-steps · flow 15, round 3
The counting rule (round 2's, unchanged). A journey counts from Home: presses + fields typed or chosen. A menu link is a press; at 390, opening the menu is one more. A dialog's confirm is a press. A field that arrives chosen counts as none. A page the app returns to by itself is no press.
#### 1 · The journeys
| Journey | 1280 | 390 | Today | Presses, fields |
| First adoption (1.20, Nolwazi Mbatha) | 2 + 0 | 3 + 0 | 2 + 1 | Curricula; Adopt on 11.1 |
| A newer adoption (6.32) | 3 + 0 | 4 + 0 | 2 + 1, no question | Curricula; Adopt… on 11.2; Adopt 11.2 |
| KGK's own EPA and item (6.25 + 6.26) | 7 + 9 | 9 + 9 | 6 + 9 (6 + 10 if today's Sub-speciality arrives unchosen; 1.19-3 shows it blank before an adoption) | EPAs; Create EPA; Code, Title, Description, Category; Save · Curricula; Items; Add your institution's own item; Target, Per, Decision cadence, Minimum level, Tools; Add item |
| A move (6.36) | 4 + 1 | 5 + 1 | 3 + 1 | Trainees; Edit; choose 11.2; Save profile and move…; Move and save |
| The rebuild (6.38) | 4 + 0 | 5 + 0 | 4 + 0 | Curricula; Curriculum progress; Rebuild progress; Rebuild progress |
| The Administrator adopting for KGK | 4 + 1 | 5 + 1 | unplayed | Curricula; the institution; Show; Adopt…; Adopt 11.2 |
| The College's add (6.30, Anton Kruger) | 4 + 6 | 5 + 6 | 3 + 6 (the form under the table) | Curricula; Items of 11.2; Add item; Target, Per, Decision cadence, Minimum level, Minimum by training year (4 rows counted as one field), Tools; Add item (EPA PAED-016 and the scale arrive chosen) |
| The College's edit (6.30) | 4 + 1 | 5 + 1 | 4 + 1 (the edit row opens in the table) | Curricula; Items of 11.2; Edit PAED-011; Target; Save item |
| Opening a held-back version (6.13 / 6.29, Kruger) | one tick and one Save fewer | the same | untick Active, Save | F5: a created or cloned version starts held back |
F1 changes no count (the return is the app's). F5 removes a tick and a Save from 6.13 and from 6.29.
#### 2 · Each step whose Expect changes (round 2's rows, with round 3's)
Values person by person from FIXTURES. "unchanged" said where so.
| Step | Board | Once built |
| 1.10 | — | The Curriculum card's footer "Open Curricula"; the Curriculum group Curricula, EPAs, Activity types, Entrustment scales. The rest unchanged. |
| 1.17 (Kruger, Curricula; act-1-setup.md:424–433) | R2-A12 (shape) | "15 items, Active" becomes "15 items, Open" (T9); the rest unchanged. |
| 1.18 (Kruger, 11.1's items) | R3-B23 (shape) | The page is "Items of Paediatric EPA Curriculum (11.1)", trail Home › Curricula › Items of Paediatric EPA Curriculum (11.1), no Back links (T5); the College's notice; 15 rows with Edit and Remove…; no Add item, the built national empty line (T4, F3). |
| 1.19 | R3-A01, R2-C01 | Curricula: the empty first section, "Open for adoption" with 11.1 and IM Core Curriculum (2026.1). EPAs: Q12's empty state, Go to Curricula, no Create EPA. |
| 1.20 | R3-A01, A02 | Route /admin/curricula. Do: "Open Curricula. Under Open for adoption, press Adopt on Paediatric EPA Curriculum (11.1). Type /admin/adoptions and read where it lands." (S21) Expect: Adopt asks nothing; "Paediatric EPA Curriculum (11.1) adopted. New admissions to Paediatrics / Paediatrics join it." focused; 11.1 in Curricula you train against (15 national · 0 your own, Adopted on 2026-10-30, Items); Open for adoption lists IM Core alone; /admin/adoptions lands on Curricula; no Curriculum adoptions in the menu. |
| 1.21 | R2-C02, R3-A02, B01 | EPAs: 15 rows, each "Set by the College.", Create EPA. Curricula: one row, Adopted on 2026-10-30, Items. Items: notice, 15 rows "Set by the College.", no Add, the built empty line. Home's card "Open Curricula". |
| 1.22, 6.9 | R2-E01, E02 | Q13's subtitle; two scales, then three; no Create scale, no actions column. |
| 2.44 | — | The card's footer "Open Curricula". |
| 6.13 (Kruger) | — | Do: "Create Neonatology EPA Curriculum, version 1.0, in Paediatrics / Neonatology, effective from D, with no end. Then open its items." ("Untick Active and save" goes, F5.) Expect: the create opens its edit page with Active unticked; the list shows it Held back, 0 items; its items page as before. |
| 6.14, 6.28 | — | Unchanged. 6.14's Do edits a title and names neither "Deactivate" nor "Deactivate…": the review's "6.14's Do" was wrong. |
| 6.17 | — | Do: "press Deactivate…" (S11). Expect: "Deactivate PAED-012?" with "for every institution that has adopted it"; after it, "Deactivate… is gone". |
| 6.22 | R2-C03, R3-B02 | PAED-012 "Inactive"; "Set by the College." on every row; its item "(inactive: not in force)". |
| 6.23 | — | Expect: "Deactivate… is offered again" (S11). |
| 6.25 | R2-C04, R3-C08, C09 | Today's Actual (act-6-catalogue.md:627–630): "/admin/epas/17, Active, Deactivate and Save offered", "Edit on all 16". Once built: Save opens /admin/epas/17 by ?created, "EPA saved." focused, the token gone; titled "KGK-001 — Running a paediatric outreach clinic at a district hospital"; the list puts KGK-001 first with "Your institution's own EPA" and its Edit the only one (S23). |
| 6.26 | R2-B04, R3-B06 | The add page; Add item returns by ?added, "Curriculum item added." focused; KGK-001's row last, marked; the empty line. Curricula's 11.1 then "15 national · 1 your own". |
| 6.29 (Kruger) | — | The Do's last sentence ("On the new version's page, untick Active and save.") goes. Expect: "Once cloned, the page is 11.2's, with Active unticked: a clone starts held back"; the list shows 11.2 Held back, 15 items. |
| 6.30 (Kruger) | R3-B22, B23, B24 | Do unchanged. Expect: "Add item" beside "Items (15)" opens /admin/curricula/4/items/new: h1 "Add item", the NEW subtitle, the picker PAED-016 alone; Add item returns with "Curriculum item added." focused, 16 rows, the national empty line. "Edit PAED-011 opens its page"; Save item returns with "Curriculum item saved." focused, the row "2 per academic year"; 11.1's PAED-011 still 1 (outcome SQL unchanged). |
| 6.31 (Kruger) | — | Do unchanged ("tick Active and save", the only opening). Expect: 11.1 and 11.2 Open, 11.2 at 16 items, Neonatology still Held back. |
| 6.32 | R3-A03, A05 | Route /admin/curricula. Adopt… on 11.2; the dialog, first focus Cancel, "The 3 registrars on 11.1 stay on it…"; the result focused; 11.1 superseded, 3 registrars; 11.1 offered again, "Older than…". |
| 6.33 | R3-A05, A06a, B07, B08 | In the Do's order (S20): the list read first is R3-A05's (11.2 "16 national · 0 your own"; 11.1 "3 registrars still on it"); 11.2's items before the add (16 rows, the new-version line, the action); the add; after it R3-A06a's (11.2 "16 national · 1 your own"). |
| 6.34 | R2-D01 | The line under the picker; Save profile asks nothing; "Trainee profile saved." |
| 6.35, 6.37, 6.39 | R2-G01 | Unchanged. |
| 6.36 | R2-D02, R3-D03, R2-D05 | Save profile and move…; the dialog; the built result; reopened, 11.2 alone. |
| 6.38 | R2-F01–F04, R3-F02 | Unchanged but Curricula's Status words and two h2 sections; the trigger primary. |
| Act 6 Outcome (act-6-catalogue.md:1019–1023, :1032) | R3-A06b, A09 | The curricula table's Status "Open" / "Open" / "Held back" for 11.1, 11.2 and Neonatology 1.0; KGK's "Adoptions: 11.2 Active and 11.1 Superseded" reads as Curricula says it: 11.2 "Adopted on 2026-10-30"; 11.1 "Superseded on 2026-10-30" over "2 registrars still on it". |
| A.5.2, A.7.9 | — | Expects unchanged; their Actuals' menu lists lose Curriculum adoptions. |
| A.7.10 (Kruger at 390) | R3-B23-390, B24-390 | Do adds "press Edit PAED-001 and read its page". Expect: the lists stack, each action 44 px; the item's page one field a row; nothing sideways. Note § 2's 390 finding for the Institutional admin's "Add your institution's own item" (36 px). |

######## BOARD R3-0-steps-2.dc.html
### R3-0-steps-2 · the missed hits, and the boards no step plays
#### 3 · The missed hits (S22), each with its new words
Line numbers as the review gives them; the words are this round's.
| Where | New words |
| states.md:143 | Curricula (Institutional admin) replaces Curriculum adoptions as the adoption page. |
| states.md:765–776 | Curricula's states: empty first section; adopted; the adoption dialog and its Cancel; superseded with registrars; T301's refusal; going back; loading ("Loading the curricula."); load error with Try again. |
| states.md:797–798, :800 | The items page: Edit and Remove… open the item's own page and the Remove dialog; no row edits in place. |
| states.md:840–847 | The item add and edit pages' states: before a rung, refused under its field, added (?added), saved (?saved), cancelled (?back). |
| states.md:901 | EPA: titled by the EPA; "EPA saved." by ?created; Unavailable for an EPA out of scope. |
| states.md:981 | Curriculum progress: the failure as an Alert in an ActionResult, focused. |
| coverage.md:104 | Curriculum adoptions → Curricula's "Open for adoption". |
| coverage.md:198 | The Administrator adopting for an institution: Curricula's "An institution's curricula" (R3-A10, A11). |
| coverage.md:323–324 | The items page's edit row → the item's own page, for every caller. |
| act-6-catalogue.md:1019–1023, :1032 | As Steps T1 (§ 2, Act 6 Outcome). |
| act-1-setup.md:285, :302–303 | Curriculum adoptions → Curricula in 1.10's menu and card. |
| Gaps that close | F-1.19a (Create EPA with no discipline), F-1.21a (Edit on national EPAs), F-1.22a (the scales' subtitle), F-6.22a (Edit on a paused national EPA), F-6.25a and b (the page titled "Edit EPA"; KGK-001 unmarked), F-A.7.10a (the items table sideways at 390). |
#### 4 · Boards no step plays as drawn (S24)
| Board | Its step, or the states.md row it needs | Reached by |
| R2-B09 (the edit page) | 6.33 continued: "press Edit KGK-001" | scratch |
| R3-B10 (saved) | a new states.md row: Save item from KGK-001's page | scratch |
| R3-B11 (cancelled) | a new row: Cancel from KGK-001's page | scratch |
| R2-B12 (save refused) | a new row (Illustration) | scratch, a stale page |
| R3-B13, B14 (Remove) | a new row: Remove KGK-001 from 11.2 (it undoes 6.33) | scratch |
| R2-B15, B16 (Unavailable) | new rows: an id typed | the replay (typing a URL) |
| R3-B21 (superseded items) | 6.38's read of 11.1's items by Nolwazi Mbatha: a new row | the replay |
| R3-B22–B24 | 6.30 | the replay |
| R2-C05–C07 (refusals) | new rows (C06, C07 Illustration) | scratch |
| R2-C10, C11 (the record; Unavailable) | new rows: open PAED-012; type /admin/epas/1 | the replay |
| R3-C12, R2-C13, C14 (deactivate, reactivate KGK-001) | new rows | scratch |
| R3-C16, C17, C18a, C18b (loading, load errors) | build states: states.md rows with a fault injected | scratch, a fault |
| R2-D06 (move refused) | a new row (Illustration) | scratch, a stale page |
| R2-F05 (rebuild failed) | a new row (Illustration) | scratch, a fault |
| R3-A10, A11 (the Administrator's section) | A10: a new row after 6.38; A11: unplayed (Illustration) | the replay; scratch |

######## BOARD R3-A01.dc.html
R3-A01 · 1280
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

######## BOARD R3-A01-390.dc.html
R3-A01-390 · 390
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

######## BOARD R3-A01-768.dc.html
R3-A01-768 · 768
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

######## BOARD R3-A02.dc.html
R3-A02 · 1280
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

######## BOARD R3-A02-390.dc.html
R3-A02-390 · 390
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

######## BOARD R3-A02-768.dc.html
R3-A02-768 · 768
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

######## BOARD R3-A03.dc.html
R3-A03 · 1280
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
| Paediatric EPA Curriculum (11.2)Newer than the version your institution has adopted (11.1). | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-10-30 | 16 | [button] [aria-label="Adopt Paediatric EPA Curriculum (11.2)…"] [aria-haspopup="dialog"] Adopt… |
| IM Core Curriculum (2026.1)Your institution has adopted no General Medicine / General Internal Medicine version yet. | Demo College | General Medicine / General Internal Medicine | 2026-01-01 | 1 | [button] [aria-label="Adopt IM Core Curriculum (2026.1)"] Adopt |
#### Adopt Paediatric EPA Curriculum (11.2) for Kgosi Kgari Teaching Hospital?
It supersedes Paediatric EPA Curriculum (11.1) for every new admission to Paediatrics / Paediatrics at Kgosi Kgari Teaching Hospital.
The 3 registrars on 11.1 stay on it until each is moved on their profile.
Your institution's own item on 11.1, KGK-001, is not carried over: add it again on 11.2.
[button] Cancel
[button] Adopt 11.2

######## BOARD R3-A03-390.dc.html
R3-A03-390 · 390
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
| Paediatric EPA Curriculum (11.2)Newer than the version your institution has adopted (11.1). | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-10-30 | 16 | [button] [aria-label="Adopt Paediatric EPA Curriculum (11.2)…"] [aria-haspopup="dialog"] Adopt… |
| IM Core Curriculum (2026.1)Your institution has adopted no General Medicine / General Internal Medicine version yet. | Demo College | General Medicine / General Internal Medicine | 2026-01-01 | 1 | [button] [aria-label="Adopt IM Core Curriculum (2026.1)"] Adopt |
#### Adopt Paediatric EPA Curriculum (11.2) for Kgosi Kgari Teaching Hospital?
It supersedes Paediatric EPA Curriculum (11.1) for every new admission to Paediatrics / Paediatrics at Kgosi Kgari Teaching Hospital.
The 3 registrars on 11.1 stay on it until each is moved on their profile.
Your institution's own item on 11.1, KGK-001, is not carried over: add it again on 11.2.
[button] Cancel
[button] Adopt 11.2

######## BOARD R3-A04.dc.html
R3-A04 · 1280
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
| Paediatric EPA Curriculum (11.2)Newer than the version your institution has adopted (11.1). | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-10-30 | 16 | [button] [aria-label="Adopt Paediatric EPA Curriculum (11.2)…"] [aria-haspopup="dialog"] Adopt… |
| IM Core Curriculum (2026.1)Your institution has adopted no General Medicine / General Internal Medicine version yet. | Demo College | General Medicine / General Internal Medicine | 2026-01-01 | 1 | [button] [aria-label="Adopt IM Core Curriculum (2026.1)"] Adopt |

######## BOARD R3-A04-390.dc.html
R3-A04-390 · 390
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
| Paediatric EPA Curriculum (11.2)Newer than the version your institution has adopted (11.1). | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-10-30 | 16 | [button] [aria-label="Adopt Paediatric EPA Curriculum (11.2)…"] [aria-haspopup="dialog"] Adopt… |
| IM Core Curriculum (2026.1)Your institution has adopted no General Medicine / General Internal Medicine version yet. | Demo College | General Medicine / General Internal Medicine | 2026-01-01 | 1 | [button] [aria-label="Adopt IM Core Curriculum (2026.1)"] Adopt |

######## BOARD R3-A05.dc.html
R3-A05 · 1280
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
| Paediatric EPA Curriculum (11.1)Older than the version your institution has adopted (11.2). | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-01-01 | 15 | [button] [aria-label="Adopt Paediatric EPA Curriculum (11.1)…"] [aria-haspopup="dialog"] Adopt… |
| IM Core Curriculum (2026.1)Your institution has adopted no General Medicine / General Internal Medicine version yet. | Demo College | General Medicine / General Internal Medicine | 2026-01-01 | 1 | [button] [aria-label="Adopt IM Core Curriculum (2026.1)"] Adopt |

######## BOARD R3-A05-390.dc.html
R3-A05-390 · 390
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
| Paediatric EPA Curriculum (11.1)Older than the version your institution has adopted (11.2). | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-01-01 | 15 | [button] [aria-label="Adopt Paediatric EPA Curriculum (11.1)…"] [aria-haspopup="dialog"] Adopt… |
| IM Core Curriculum (2026.1)Your institution has adopted no General Medicine / General Internal Medicine version yet. | Demo College | General Medicine / General Internal Medicine | 2026-01-01 | 1 | [button] [aria-label="Adopt IM Core Curriculum (2026.1)"] Adopt |

######## BOARD R3-A05-768.dc.html
R3-A05-768 · 768
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
| Paediatric EPA Curriculum (11.1)Older than the version your institution has adopted (11.2). | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-01-01 | 15 | [button] [aria-label="Adopt Paediatric EPA Curriculum (11.1)…"] [aria-haspopup="dialog"] Adopt… |
| IM Core Curriculum (2026.1)Your institution has adopted no General Medicine / General Internal Medicine version yet. | Demo College | General Medicine / General Internal Medicine | 2026-01-01 | 1 | [button] [aria-label="Adopt IM Core Curriculum (2026.1)"] Adopt |

######## BOARD R3-A06a.dc.html
R3-A06a · 1280
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
| Paediatric EPA Curriculum (11.1)Older than the version your institution has adopted (11.2). | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-01-01 | 15 | [button] [aria-label="Adopt Paediatric EPA Curriculum (11.1)…"] [aria-haspopup="dialog"] Adopt… |
| IM Core Curriculum (2026.1)Your institution has adopted no General Medicine / General Internal Medicine version yet. | Demo College | General Medicine / General Internal Medicine | 2026-01-01 | 1 | [button] [aria-label="Adopt IM Core Curriculum (2026.1)"] Adopt |

######## BOARD R3-A06a-390.dc.html
R3-A06a-390 · 390
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
| Paediatric EPA Curriculum (11.1)Older than the version your institution has adopted (11.2). | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-01-01 | 15 | [button] [aria-label="Adopt Paediatric EPA Curriculum (11.1)…"] [aria-haspopup="dialog"] Adopt… |
| IM Core Curriculum (2026.1)Your institution has adopted no General Medicine / General Internal Medicine version yet. | Demo College | General Medicine / General Internal Medicine | 2026-01-01 | 1 | [button] [aria-label="Adopt IM Core Curriculum (2026.1)"] Adopt |

######## BOARD R3-A06b.dc.html
R3-A06b · 1280
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
| Paediatric EPA Curriculum (11.1)Older than the version your institution has adopted (11.2). | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-01-01 | 15 | [button] [aria-label="Adopt Paediatric EPA Curriculum (11.1)…"] [aria-haspopup="dialog"] Adopt… |
| IM Core Curriculum (2026.1)Your institution has adopted no General Medicine / General Internal Medicine version yet. | Demo College | General Medicine / General Internal Medicine | 2026-01-01 | 1 | [button] [aria-label="Adopt IM Core Curriculum (2026.1)"] Adopt |

######## BOARD R3-A06b-390.dc.html
R3-A06b-390 · 390
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
| Paediatric EPA Curriculum (11.1)Older than the version your institution has adopted (11.2). | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-01-01 | 15 | [button] [aria-label="Adopt Paediatric EPA Curriculum (11.1)…"] [aria-haspopup="dialog"] Adopt… |
| IM Core Curriculum (2026.1)Your institution has adopted no General Medicine / General Internal Medicine version yet. | Demo College | General Medicine / General Internal Medicine | 2026-01-01 | 1 | [button] [aria-label="Adopt IM Core Curriculum (2026.1)"] Adopt |

######## BOARD R3-A07.dc.html
IllustrationR3-A07 · 1280
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

######## BOARD R3-A07-390.dc.html
IllustrationR3-A07-390 · 390
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

######## BOARD R3-A08.dc.html
IllustrationR3-A08 · 1280
Illustration · 6.38's state · going back (Q7, M2, T1, S1): Adopt… on 11.1; the dialog counts the one registrar on 11.2 (Sipho Ndlovu) and has no own-items line, since 11.1 already holds KGK-001
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
| Paediatric EPA Curriculum (11.1)Older than the version your institution has adopted (11.2). | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-01-01 | 15 | [button] [aria-label="Adopt Paediatric EPA Curriculum (11.1)…"] [aria-haspopup="dialog"] Adopt… |
| IM Core Curriculum (2026.1)Your institution has adopted no General Medicine / General Internal Medicine version yet. | Demo College | General Medicine / General Internal Medicine | 2026-01-01 | 1 | [button] [aria-label="Adopt IM Core Curriculum (2026.1)"] Adopt |
#### Adopt Paediatric EPA Curriculum (11.1) for Kgosi Kgari Teaching Hospital?
It supersedes Paediatric EPA Curriculum (11.2) for every new admission to Paediatrics / Paediatrics at Kgosi Kgari Teaching Hospital.
The registrar on 11.2 stays on it until moved on their profile.
[button] Cancel
[button] Adopt 11.1

######## BOARD R3-A08-390.dc.html
IllustrationR3-A08-390 · 390
Illustration · 6.38's state · going back (Q7, M2, T1, S1): Adopt… on 11.1; the dialog counts the one registrar on 11.2 (Sipho Ndlovu) and has no own-items line, since 11.1 already holds KGK-001
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
| Paediatric EPA Curriculum (11.1)Older than the version your institution has adopted (11.2). | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-01-01 | 15 | [button] [aria-label="Adopt Paediatric EPA Curriculum (11.1)…"] [aria-haspopup="dialog"] Adopt… |
| IM Core Curriculum (2026.1)Your institution has adopted no General Medicine / General Internal Medicine version yet. | Demo College | General Medicine / General Internal Medicine | 2026-01-01 | 1 | [button] [aria-label="Adopt IM Core Curriculum (2026.1)"] Adopt |
#### Adopt Paediatric EPA Curriculum (11.1) for Kgosi Kgari Teaching Hospital?
It supersedes Paediatric EPA Curriculum (11.2) for every new admission to Paediatrics / Paediatrics at Kgosi Kgari Teaching Hospital.
The registrar on 11.2 stays on it until moved on their profile.
[button] Cancel
[button] Adopt 11.1

######## BOARD R3-A09.dc.html
R3-A09 · 1280
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
Every College's curriculum versions, with the College's status. Items counts each institution's own items too.
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

######## BOARD R3-A09-390.dc.html
R3-A09-390 · 390
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
Every College's curriculum versions, with the College's status. Items counts each institution's own items too.
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

######## BOARD R3-A09-768.dc.html
R3-A09-768 · 768
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
Every College's curriculum versions, with the College's status. Items counts each institution's own items too.
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

######## BOARD R3-A10.dc.html
R3-A10 · 1280
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
Every College's curriculum versions, with the College's status. Items counts each institution's own items too.
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
| Curriculum | College | Discipline | Effective | Items | For this institution | Actions |
| Paediatric EPA Curriculum (11.2) | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-10-30 | 16 national · 1 Kgosi Kgari Teaching Hospital's own | Adopted on 2026-10-30 | [link] [aria-label="Items of Paediatric EPA Curriculum (11.2) for Kgosi Kgari Teaching Hospital"] Items |
| Paediatric EPA Curriculum (11.1) | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-01-01 | 15 national · 1 Kgosi Kgari Teaching Hospital's own | Superseded on 2026-10-302 registrars still on it | [link] [aria-label="Items of Paediatric EPA Curriculum (11.1) for Kgosi Kgari Teaching Hospital"] Items |
##### Open to Kgosi Kgari Teaching Hospital
Adopting a version makes it the one that new admissions to its discipline join. Adopting another version of a discipline the institution has adopted supersedes the current one: registrars already admitted stay on theirs until each is moved, and the institution's own items are not carried over.
College curriculum versions open for adoption
| Curriculum | College | Discipline | Effective | Items | Actions |
| Paediatric EPA Curriculum (11.1)Older than the version Kgosi Kgari Teaching Hospital has adopted (11.2). | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-01-01 | 15 | [button] [aria-label="Adopt Paediatric EPA Curriculum (11.1) for Kgosi Kgari Teaching Hospital…"] [aria-haspopup="dialog"] Adopt… |
| IM Core Curriculum (2026.1)Kgosi Kgari Teaching Hospital has adopted no General Medicine / General Internal Medicine version yet. | Demo College | General Medicine / General Internal Medicine | 2026-01-01 | 1 | [button] [aria-label="Adopt IM Core Curriculum (2026.1) for Kgosi Kgari Teaching Hospital"] Adopt |

######## BOARD R3-A10-390.dc.html
R3-A10-390 · 390
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
Every College's curriculum versions, with the College's status. Items counts each institution's own items too.
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
| Curriculum | College | Discipline | Effective | Items | For this institution | Actions |
| Paediatric EPA Curriculum (11.2) | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-10-30 | 16 national · 1 Kgosi Kgari Teaching Hospital's own | Adopted on 2026-10-30 | [link] [aria-label="Items of Paediatric EPA Curriculum (11.2) for Kgosi Kgari Teaching Hospital"] Items |
| Paediatric EPA Curriculum (11.1) | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-01-01 | 15 national · 1 Kgosi Kgari Teaching Hospital's own | Superseded on 2026-10-302 registrars still on it | [link] [aria-label="Items of Paediatric EPA Curriculum (11.1) for Kgosi Kgari Teaching Hospital"] Items |
##### Open to Kgosi Kgari Teaching Hospital
Adopting a version makes it the one that new admissions to its discipline join. Adopting another version of a discipline the institution has adopted supersedes the current one: registrars already admitted stay on theirs until each is moved, and the institution's own items are not carried over.
College curriculum versions open for adoption
| Curriculum | College | Discipline | Effective | Items | Actions |
| Paediatric EPA Curriculum (11.1)Older than the version Kgosi Kgari Teaching Hospital has adopted (11.2). | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-01-01 | 15 | [button] [aria-label="Adopt Paediatric EPA Curriculum (11.1) for Kgosi Kgari Teaching Hospital…"] [aria-haspopup="dialog"] Adopt… |
| IM Core Curriculum (2026.1)Kgosi Kgari Teaching Hospital has adopted no General Medicine / General Internal Medicine version yet. | Demo College | General Medicine / General Internal Medicine | 2026-01-01 | 1 | [button] [aria-label="Adopt IM Core Curriculum (2026.1) for Kgosi Kgari Teaching Hospital"] Adopt |

######## BOARD R3-A11.dc.html
IllustrationR3-A11 · 1280
Illustration · unplayed (coverage.md:198) · Demo Administrator adopting 11.2 for Kgosi Kgari Teaching Hospital at 6.31's state; the dialog names the institution; All curricula reads 16 for 11.2 (M2); the institution's Items links named for it (S15)
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
Every College's curriculum versions, with the College's status. Items counts each institution's own items too.
| Curriculum | College | Discipline | Effective | Items | Status | Actions |
| Paediatric EPA Curriculum (11.2) | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-10-30 | 16 | Open | [link] [aria-label="Edit Paediatric EPA Curriculum (11.2)"] Edit [link] [aria-label="Items of Paediatric EPA Curriculum (11.2)"] Items |
| Paediatric EPA Curriculum (11.1) | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-01-01 | 16 | Open | [link] [aria-label="Edit Paediatric EPA Curriculum (11.1)"] Edit [link] [aria-label="Items of Paediatric EPA Curriculum (11.1)"] Items |
| Neonatology EPA Curriculum (1.0) | College of Paediatricians of South Africa | Paediatrics / Neonatology | 2026-10-30 | 0 | Held back | [link] [aria-label="Edit Neonatology EPA Curriculum (1.0)"] Edit [link] [aria-label="Items of Neonatology EPA Curriculum (1.0)"] Items |
| IM Core Curriculum (2026.1) | Demo College | General Medicine / General Internal Medicine | 2026-01-01 | 1 | Open | [link] [aria-label="Edit IM Core Curriculum (2026.1)"] Edit [link] [aria-label="Items of IM Core Curriculum (2026.1)"] Items |
#### An institution's curricula
Institution
Kgosi Kgari Teaching Hospital
[button] Show
##### Curricula Kgosi Kgari Teaching Hospital trains against
The curricula Kgosi Kgari Teaching Hospital trains against, with the state of each
| Curriculum | College | Discipline | Effective | Items | For this institution | Actions |
| Paediatric EPA Curriculum (11.1) | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-01-01 | 15 national · 1 Kgosi Kgari Teaching Hospital's own | Adopted on 2026-10-30 | [link] [aria-label="Items of Paediatric EPA Curriculum (11.1) for Kgosi Kgari Teaching Hospital"] Items |
##### Open to Kgosi Kgari Teaching Hospital
Adopting a version makes it the one that new admissions to its discipline join. Adopting another version of a discipline the institution has adopted supersedes the current one: registrars already admitted stay on theirs until each is moved, and the institution's own items are not carried over.
College curriculum versions open for adoption
| Curriculum | College | Discipline | Effective | Items | Actions |
| Paediatric EPA Curriculum (11.2)Newer than the version Kgosi Kgari Teaching Hospital has adopted (11.1). | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-10-30 | 16 | [button] [aria-label="Adopt Paediatric EPA Curriculum (11.2) for Kgosi Kgari Teaching Hospital…"] [aria-haspopup="dialog"] Adopt… |
| IM Core Curriculum (2026.1)Kgosi Kgari Teaching Hospital has adopted no General Medicine / General Internal Medicine version yet. | Demo College | General Medicine / General Internal Medicine | 2026-01-01 | 1 | [button] [aria-label="Adopt IM Core Curriculum (2026.1) for Kgosi Kgari Teaching Hospital"] Adopt |
#### Adopt Paediatric EPA Curriculum (11.2) for Kgosi Kgari Teaching Hospital?
It supersedes Paediatric EPA Curriculum (11.1) for every new admission to Paediatrics / Paediatrics at Kgosi Kgari Teaching Hospital.
The 3 registrars on 11.1 stay on it until each is moved on their profile.
Kgosi Kgari Teaching Hospital's own item on 11.1, KGK-001, is not carried over: add it again on 11.2.
[button] Cancel
[button] Adopt 11.2

######## BOARD R3-A11-390.dc.html
IllustrationR3-A11-390 · 390
Illustration · unplayed (coverage.md:198) · Demo Administrator adopting 11.2 for Kgosi Kgari Teaching Hospital at 6.31's state; the dialog names the institution; All curricula reads 16 for 11.2 (M2); the institution's Items links named for it (S15)
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
Every College's curriculum versions, with the College's status. Items counts each institution's own items too.
| Curriculum | College | Discipline | Effective | Items | Status | Actions |
| Paediatric EPA Curriculum (11.2) | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-10-30 | 16 | Open | [link] [aria-label="Edit Paediatric EPA Curriculum (11.2)"] Edit [link] [aria-label="Items of Paediatric EPA Curriculum (11.2)"] Items |
| Paediatric EPA Curriculum (11.1) | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-01-01 | 16 | Open | [link] [aria-label="Edit Paediatric EPA Curriculum (11.1)"] Edit [link] [aria-label="Items of Paediatric EPA Curriculum (11.1)"] Items |
| Neonatology EPA Curriculum (1.0) | College of Paediatricians of South Africa | Paediatrics / Neonatology | 2026-10-30 | 0 | Held back | [link] [aria-label="Edit Neonatology EPA Curriculum (1.0)"] Edit [link] [aria-label="Items of Neonatology EPA Curriculum (1.0)"] Items |
| IM Core Curriculum (2026.1) | Demo College | General Medicine / General Internal Medicine | 2026-01-01 | 1 | Open | [link] [aria-label="Edit IM Core Curriculum (2026.1)"] Edit [link] [aria-label="Items of IM Core Curriculum (2026.1)"] Items |
#### An institution's curricula
Institution
Kgosi Kgari Teaching Hospital
[button] Show
##### Curricula Kgosi Kgari Teaching Hospital trains against
The curricula Kgosi Kgari Teaching Hospital trains against, with the state of each
| Curriculum | College | Discipline | Effective | Items | For this institution | Actions |
| Paediatric EPA Curriculum (11.1) | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-01-01 | 15 national · 1 Kgosi Kgari Teaching Hospital's own | Adopted on 2026-10-30 | [link] [aria-label="Items of Paediatric EPA Curriculum (11.1) for Kgosi Kgari Teaching Hospital"] Items |
##### Open to Kgosi Kgari Teaching Hospital
Adopting a version makes it the one that new admissions to its discipline join. Adopting another version of a discipline the institution has adopted supersedes the current one: registrars already admitted stay on theirs until each is moved, and the institution's own items are not carried over.
College curriculum versions open for adoption
| Curriculum | College | Discipline | Effective | Items | Actions |
| Paediatric EPA Curriculum (11.2)Newer than the version Kgosi Kgari Teaching Hospital has adopted (11.1). | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-10-30 | 16 | [button] [aria-label="Adopt Paediatric EPA Curriculum (11.2) for Kgosi Kgari Teaching Hospital…"] [aria-haspopup="dialog"] Adopt… |
| IM Core Curriculum (2026.1)Kgosi Kgari Teaching Hospital has adopted no General Medicine / General Internal Medicine version yet. | Demo College | General Medicine / General Internal Medicine | 2026-01-01 | 1 | [button] [aria-label="Adopt IM Core Curriculum (2026.1) for Kgosi Kgari Teaching Hospital"] Adopt |
#### Adopt Paediatric EPA Curriculum (11.2) for Kgosi Kgari Teaching Hospital?
It supersedes Paediatric EPA Curriculum (11.1) for every new admission to Paediatrics / Paediatrics at Kgosi Kgari Teaching Hospital.
The 3 registrars on 11.1 stay on it until each is moved on their profile.
Kgosi Kgari Teaching Hospital's own item on 11.1, KGK-001, is not carried over: add it again on 11.2.
[button] Cancel
[button] Adopt 11.2

######## BOARD R3-A12.dc.html
R3-A12 · 1280
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

######## BOARD R3-A12-390.dc.html
R3-A12-390 · 390
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

######## BOARD R3-A13.dc.html
IllustrationR3-A13 · 1280
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
| Paediatric EPA Curriculum (11.1)Older than the version your institution has adopted (11.2). | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-01-01 | 15 | [button] [aria-label="Adopt Paediatric EPA Curriculum (11.1)…"] [aria-haspopup="dialog"] Adopt… |

######## BOARD R3-A13-390.dc.html
IllustrationR3-A13-390 · 390
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
| Paediatric EPA Curriculum (11.1)Older than the version your institution has adopted (11.2). | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-01-01 | 15 | [button] [aria-label="Adopt Paediatric EPA Curriculum (11.1)…"] [aria-haspopup="dialog"] Adopt… |

######## BOARD R3-A13-768.dc.html
IllustrationR3-A13-768 · 768
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
| Paediatric EPA Curriculum (11.1)Older than the version your institution has adopted (11.2). | College of Paediatricians of South Africa | Paediatrics / Paediatrics | 2026-01-01 | 15 | [button] [aria-label="Adopt Paediatric EPA Curriculum (11.1)…"] [aria-haspopup="dialog"] Adopt… |

######## BOARD R3-A14.dc.html
R3-A14 · 1280
Nolwazi Mbatha · loading (S19): the header at once, one loading line, a skeleton per section; no Adopt until the read is in
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
Loading the curricula.
#### Curricula you train against
#### Open for adoption

######## BOARD R3-A14-390.dc.html
R3-A14-390 · 390
Nolwazi Mbatha · loading (S19): the header at once, one loading line, a skeleton per section; no Adopt until the read is in
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
Loading the curricula.
#### Curricula you train against
#### Open for adoption

######## BOARD R3-A15.dc.html
R3-A15 · 1280
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

######## BOARD R3-A15-390.dc.html
R3-A15-390 · 390
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

######## BOARD R3-B01.dc.html
R3-B01 · 1280
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

######## BOARD R3-B01-390.dc.html
R3-B01-390 · 390
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

######## BOARD R3-B01-768.dc.html
R3-B01-768 · 768
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

######## BOARD R3-B02.dc.html
R3-B02 · 1280
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

######## BOARD R3-B02-390.dc.html
R3-B02-390 · 390
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

######## BOARD R3-B03.dc.html
R3-B03 · 1280
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

######## BOARD R3-B03-390.dc.html
R3-B03-390 · 390
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

######## BOARD R3-B06.dc.html
R3-B06 · 1280
6.26 · Nolwazi Mbatha · back on the list by /admin/curricula/2/items?added=‹item id› (F1), the token stripped from the address once read; the page wrote "Curriculum item added." itself; KGK-001 last with its mark, Edit and Remove…; the built empty line again; journey (6.25 + 6.26) 7 + 9 (390: 9 + 9)
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
| KGK-001 — Running a paediatric outreach clinic at a district hospitalYour institution's own item | 1 per academic yearNo decision cadence | 3aCPSA Paediatric Entrustment Scale v11.1 | — | 12 monthsWeight — | Direct observation | [link] [aria-label="Edit KGK-001"] Edit [button] [aria-label="Remove KGK-001"] [aria-haspopup="dialog"] Remove… |

######## BOARD R3-B06-390.dc.html
R3-B06-390 · 390
6.26 · Nolwazi Mbatha · back on the list by /admin/curricula/2/items?added=‹item id› (F1), the token stripped from the address once read; the page wrote "Curriculum item added." itself; KGK-001 last with its mark, Edit and Remove…; the built empty line again; journey (6.25 + 6.26) 7 + 9 (390: 9 + 9)
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
| KGK-001 — Running a paediatric outreach clinic at a district hospitalYour institution's own item | 1 per academic yearNo decision cadence | 3aCPSA Paediatric Entrustment Scale v11.1 | — | 12 monthsWeight — | Direct observation | [link] [aria-label="Edit KGK-001"] Edit [button] [aria-label="Remove KGK-001"] [aria-haspopup="dialog"] Remove… |

######## BOARD R3-B06-768.dc.html
R3-B06-768 · 768
6.26 · Nolwazi Mbatha · back on the list by /admin/curricula/2/items?added=‹item id› (F1), the token stripped from the address once read; the page wrote "Curriculum item added." itself; KGK-001 last with its mark, Edit and Remove…; the built empty line again; journey (6.25 + 6.26) 7 + 9 (390: 9 + 9)
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
| KGK-001 — Running a paediatric outreach clinic at a district hospitalYour institution's own item | 1 per academic yearNo decision cadence | 3aCPSA Paediatric Entrustment Scale v11.1 | — | 12 monthsWeight — | Direct observation | [link] [aria-label="Edit KGK-001"] Edit [button] [aria-label="Remove KGK-001"] [aria-haspopup="dialog"] Remove… |

######## BOARD R3-B07.dc.html
R3-B07 · 1280
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

######## BOARD R3-B07-390.dc.html
R3-B07-390 · 390
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

######## BOARD R3-B08.dc.html
R3-B08 · 1280
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
| KGK-001 — Running a paediatric outreach clinic at a district hospitalYour institution's own item | 1 per academic yearNo decision cadence | 3aCPSA Paediatric Entrustment Scale v11.1 | — | 12 monthsWeight — | Direct observation | [link] [aria-label="Edit KGK-001"] Edit [button] [aria-label="Remove KGK-001"] [aria-haspopup="dialog"] Remove… |

######## BOARD R3-B08-390.dc.html
R3-B08-390 · 390
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
| KGK-001 — Running a paediatric outreach clinic at a district hospitalYour institution's own item | 1 per academic yearNo decision cadence | 3aCPSA Paediatric Entrustment Scale v11.1 | — | 12 monthsWeight — | Direct observation | [link] [aria-label="Edit KGK-001"] Edit [button] [aria-label="Remove KGK-001"] [aria-haspopup="dialog"] Remove… |

######## BOARD R3-B08-768.dc.html
R3-B08-768 · 768
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
| KGK-001 — Running a paediatric outreach clinic at a district hospitalYour institution's own item | 1 per academic yearNo decision cadence | 3aCPSA Paediatric Entrustment Scale v11.1 | — | 12 monthsWeight — | Direct observation | [link] [aria-label="Edit KGK-001"] Edit [button] [aria-label="Remove KGK-001"] [aria-haspopup="dialog"] Remove… |

######## BOARD R3-B08-901.dc.html
R3-B08-901 · 901
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
| KGK-001 — Running a paediatric outreach clinic at a district hospitalYour institution's own item | 1 per academic yearNo decision cadence | 3aCPSA Paediatric Entrustment Scale v11.1 | — | 12 monthsWeight — | Direct observation | [link] [aria-label="Edit KGK-001"] Edit [button] [aria-label="Remove KGK-001"] [aria-haspopup="dialog"] Remove… |

######## BOARD R3-B08-1100.dc.html
R3-B08-1100 · 1100
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
| KGK-001 — Running a paediatric outreach clinic at a district hospitalYour institution's own item | 1 per academic yearNo decision cadence | 3aCPSA Paediatric Entrustment Scale v11.1 | — | 12 monthsWeight — | Direct observation | [link] [aria-label="Edit KGK-001"] Edit [button] [aria-label="Remove KGK-001"] [aria-haspopup="dialog"] Remove… |

######## BOARD R3-B10.dc.html
R3-B10 · 1280
scratch · Nolwazi Mbatha · Save item pressed: back on the list by ?saved=‹item id› (F1), the token stripped once read; the page wrote "Curriculum item saved."
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
| KGK-001 — Running a paediatric outreach clinic at a district hospitalYour institution's own item | 1 per academic yearNo decision cadence | 3aCPSA Paediatric Entrustment Scale v11.1 | — | 12 monthsWeight — | Direct observation | [link] [aria-label="Edit KGK-001"] Edit [button] [aria-label="Remove KGK-001"] [aria-haspopup="dialog"] Remove… |

######## BOARD R3-B10-390.dc.html
R3-B10-390 · 390
scratch · Nolwazi Mbatha · Save item pressed: back on the list by ?saved=‹item id› (F1), the token stripped once read; the page wrote "Curriculum item saved."
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
| KGK-001 — Running a paediatric outreach clinic at a district hospitalYour institution's own item | 1 per academic yearNo decision cadence | 3aCPSA Paediatric Entrustment Scale v11.1 | — | 12 monthsWeight — | Direct observation | [link] [aria-label="Edit KGK-001"] Edit [button] [aria-label="Remove KGK-001"] [aria-haspopup="dialog"] Remove… |

######## BOARD R3-B11.dc.html
R3-B11 · 1280
scratch · Nolwazi Mbatha · Cancel pressed on the edit page: back on the list by ?back=‹item id› (F1), the token stripped once read; nothing saved, no result
Focus: the row's "Edit KGK-001"
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
| KGK-001 — Running a paediatric outreach clinic at a district hospitalYour institution's own item | 1 per academic yearNo decision cadence | 3aCPSA Paediatric Entrustment Scale v11.1 | — | 12 monthsWeight — | Direct observation | [link] [aria-label="Edit KGK-001"] Edit [button] [aria-label="Remove KGK-001"] [aria-haspopup="dialog"] Remove… |

######## BOARD R3-B11-390.dc.html
R3-B11-390 · 390
scratch · Nolwazi Mbatha · Cancel pressed on the edit page: back on the list by ?back=‹item id› (F1), the token stripped once read; nothing saved, no result
Focus: the row's "Edit KGK-001"
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
| KGK-001 — Running a paediatric outreach clinic at a district hospitalYour institution's own item | 1 per academic yearNo decision cadence | 3aCPSA Paediatric Entrustment Scale v11.1 | — | 12 monthsWeight — | Direct observation | [link] [aria-label="Edit KGK-001"] Edit [button] [aria-label="Remove KGK-001"] [aria-haspopup="dialog"] Remove… |

######## BOARD R3-B13.dc.html
R3-B13 · 1280
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
| KGK-001 — Running a paediatric outreach clinic at a district hospitalYour institution's own item | 1 per academic yearNo decision cadence | 3aCPSA Paediatric Entrustment Scale v11.1 | — | 12 monthsWeight — | Direct observation | [link] [aria-label="Edit KGK-001"] Edit [button] [aria-label="Remove KGK-001"] [aria-haspopup="dialog"] Remove… |
#### Remove KGK-001 from Paediatric EPA Curriculum (11.2)?
Remove KGK-001 — Running a paediatric outreach clinic at a district hospital from Paediatric EPA Curriculum (11.2)? Its target, minima, decision and tool list are deleted, and so is each trainee's tally toward it. Your institution's trainees on this curriculum are no longer measured against this EPA. This cannot be undone.
[button] Cancel
[button] Remove item

######## BOARD R3-B13-390.dc.html
R3-B13-390 · 390
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
| KGK-001 — Running a paediatric outreach clinic at a district hospitalYour institution's own item | 1 per academic yearNo decision cadence | 3aCPSA Paediatric Entrustment Scale v11.1 | — | 12 monthsWeight — | Direct observation | [link] [aria-label="Edit KGK-001"] Edit [button] [aria-label="Remove KGK-001"] [aria-haspopup="dialog"] Remove… |
#### Remove KGK-001 from Paediatric EPA Curriculum (11.2)?
Remove KGK-001 — Running a paediatric outreach clinic at a district hospital from Paediatric EPA Curriculum (11.2)? Its target, minima, decision and tool list are deleted, and so is each trainee's tally toward it. Your institution's trainees on this curriculum are no longer measured against this EPA. This cannot be undone.
[button] Cancel
[button] Remove item

######## BOARD R3-B14.dc.html
R3-B14 · 1280
scratch · Nolwazi Mbatha · removed (T11): the row gone, the action offered again
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

######## BOARD R3-B14-390.dc.html
R3-B14-390 · 390
scratch · Nolwazi Mbatha · removed (T11): the row gone, the action offered again
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

######## BOARD R3-B17.dc.html
R3-B17 · 1280
after 6.33 · Demo Administrator on 11.2's items (T4, F3): the College's notice and the national empty line as CanEditCurriculum gives them (N1); KGK's row "Kgosi Kgari Teaching Hospital's own item", every row Edit and Remove…, named with the owner on KGK's
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
A target and its period are read live. Changing one changes what every trainee on this curriculum is measured against, in every institution that has adopted it, including periods that have already closed. A tool list is checked when an activity is filed and submitted, and never takes back credit already earned.
#### Items (17)
Every national EPA of Paediatrics is already on this curriculum, as a national item or as an institution's own item, so there is none left to add. To add an item, first add a national EPA of Paediatrics, or remove a national item to free its EPA. An EPA an institution has as its own item is freed only when that institution removes it.
Paediatric EPA Curriculum (11.2)'s items, in EPA order
| EPA | Target and decision | Minimum level | Minimum by training year | Window and weight | Tools | Actions |
| PAED-001 — Providing paediatric emergency care to children | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | [link] [aria-label="Edit PAED-001"] Edit [button] [aria-label="Remove PAED-001"] [aria-haspopup="dialog"] Remove… |
| PAED-002 — Managing common paediatric presentations | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Chart-stimulated recall, Clinical audit, Direct observation, DOPS, Mini-CEX, MSF, RCA | [link] [aria-label="Edit PAED-002"] Edit [button] [aria-label="Remove PAED-002"] [aria-haspopup="dialog"] Remove… |
| PAED-003 — Providing intensive care to children | 3 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | [link] [aria-label="Edit PAED-003"] Edit [button] [aria-label="Remove PAED-003"] [aria-haspopup="dialog"] Remove… |
| PAED-004 — Managing common neonatal conditions | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-004"] Edit [button] [aria-label="Remove PAED-004"] [aria-haspopup="dialog"] Remove… |
| PAED-005 — Providing neonatal care in intensive and high-care settings | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, DOPS, MSF | [link] [aria-label="Edit PAED-005"] Edit [button] [aria-label="Remove PAED-005"] [aria-haspopup="dialog"] Remove… |
| PAED-006 — Managing long-term health conditions (LTHCs) in children | 2 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CCA, Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-006"] Edit [button] [aria-label="Remove PAED-006"] [aria-haspopup="dialog"] Remove… |
| PAED-007 — Maintaining and promoting the health and well-being of children | 1 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-007"] Edit [button] [aria-label="Remove PAED-007"] [aria-haspopup="dialog"] Remove… |
| PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, DOPS, Mini-CEX, MSF, Reflective exercise | [link] [aria-label="Edit PAED-008"] Edit [button] [aria-label="Remove PAED-008"] [aria-haspopup="dialog"] Remove… |
| PAED-009 — Providing care for an adolescent patient | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, MSF | [link] [aria-label="Edit PAED-009"] Edit [button] [aria-label="Remove PAED-009"] [aria-haspopup="dialog"] Remove… |
| PAED-010 — Leading and operating within a clinical team | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | Direct observation, MSF | [link] [aria-label="Edit PAED-010"] Edit [button] [aria-label="Remove PAED-010"] [aria-haspopup="dialog"] Remove… |
| PAED-011 — Managing population health challenges | 2 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, MSF | [link] [aria-label="Edit PAED-011"] Edit [button] [aria-label="Remove PAED-011"] [aria-haspopup="dialog"] Remove… |
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, Chart-stimulated recall, Mini-CEX, MSF | [link] [aria-label="Edit PAED-012"] Edit [button] [aria-label="Remove PAED-012"] [aria-haspopup="dialog"] Remove… |
| PAED-013 — Managing child protection, legal and ethical aspects of children at risk | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, Mini-CEX, MSF | [link] [aria-label="Edit PAED-013"] Edit [button] [aria-label="Remove PAED-013"] [aria-haspopup="dialog"] Remove… |
| PAED-014 — Providing palliative and end-of-life care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, MSF, Reflective exercise | [link] [aria-label="Edit PAED-014"] Edit [button] [aria-label="Remove PAED-014"] [aria-haspopup="dialog"] Remove… |
| PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 1 per semesterDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, Learner feedback, MSF, Portfolio and logbook review | [link] [aria-label="Edit PAED-015"] Edit [button] [aria-label="Remove PAED-015"] [aria-haspopup="dialog"] Remove… |
| PAED-016 — Transitioning adolescents with long-term conditions to adult care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Mini-CEX, MSF | [link] [aria-label="Edit PAED-016"] Edit [button] [aria-label="Remove PAED-016"] [aria-haspopup="dialog"] Remove… |
| KGK-001 — Running a paediatric outreach clinic at a district hospitalKgosi Kgari Teaching Hospital's own item | 1 per academic yearNo decision cadence | 3aCPSA Paediatric Entrustment Scale v11.1 | — | 12 monthsWeight — | Direct observation | [link] [aria-label="Edit KGK-001 (Kgosi Kgari Teaching Hospital's own item)"] Edit [button] [aria-label="Remove KGK-001 (Kgosi Kgari Teaching Hospital's own item)"] [aria-haspopup="dialog"] Remove… |

######## BOARD R3-B17-390.dc.html
R3-B17-390 · 390
after 6.33 · Demo Administrator on 11.2's items (T4, F3): the College's notice and the national empty line as CanEditCurriculum gives them (N1); KGK's row "Kgosi Kgari Teaching Hospital's own item", every row Edit and Remove…, named with the owner on KGK's
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
A target and its period are read live. Changing one changes what every trainee on this curriculum is measured against, in every institution that has adopted it, including periods that have already closed. A tool list is checked when an activity is filed and submitted, and never takes back credit already earned.
#### Items (17)
Every national EPA of Paediatrics is already on this curriculum, as a national item or as an institution's own item, so there is none left to add. To add an item, first add a national EPA of Paediatrics, or remove a national item to free its EPA. An EPA an institution has as its own item is freed only when that institution removes it.
Paediatric EPA Curriculum (11.2)'s items, in EPA order
| EPA | Target and decision | Minimum level | Minimum by training year | Window and weight | Tools | Actions |
| PAED-001 — Providing paediatric emergency care to children | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | [link] [aria-label="Edit PAED-001"] Edit [button] [aria-label="Remove PAED-001"] [aria-haspopup="dialog"] Remove… |
| PAED-002 — Managing common paediatric presentations | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Chart-stimulated recall, Clinical audit, Direct observation, DOPS, Mini-CEX, MSF, RCA | [link] [aria-label="Edit PAED-002"] Edit [button] [aria-label="Remove PAED-002"] [aria-haspopup="dialog"] Remove… |
| PAED-003 — Providing intensive care to children | 3 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | [link] [aria-label="Edit PAED-003"] Edit [button] [aria-label="Remove PAED-003"] [aria-haspopup="dialog"] Remove… |
| PAED-004 — Managing common neonatal conditions | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-004"] Edit [button] [aria-label="Remove PAED-004"] [aria-haspopup="dialog"] Remove… |
| PAED-005 — Providing neonatal care in intensive and high-care settings | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, DOPS, MSF | [link] [aria-label="Edit PAED-005"] Edit [button] [aria-label="Remove PAED-005"] [aria-haspopup="dialog"] Remove… |
| PAED-006 — Managing long-term health conditions (LTHCs) in children | 2 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CCA, Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-006"] Edit [button] [aria-label="Remove PAED-006"] [aria-haspopup="dialog"] Remove… |
| PAED-007 — Maintaining and promoting the health and well-being of children | 1 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-007"] Edit [button] [aria-label="Remove PAED-007"] [aria-haspopup="dialog"] Remove… |
| PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, DOPS, Mini-CEX, MSF, Reflective exercise | [link] [aria-label="Edit PAED-008"] Edit [button] [aria-label="Remove PAED-008"] [aria-haspopup="dialog"] Remove… |
| PAED-009 — Providing care for an adolescent patient | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, MSF | [link] [aria-label="Edit PAED-009"] Edit [button] [aria-label="Remove PAED-009"] [aria-haspopup="dialog"] Remove… |
| PAED-010 — Leading and operating within a clinical team | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | Direct observation, MSF | [link] [aria-label="Edit PAED-010"] Edit [button] [aria-label="Remove PAED-010"] [aria-haspopup="dialog"] Remove… |
| PAED-011 — Managing population health challenges | 2 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, MSF | [link] [aria-label="Edit PAED-011"] Edit [button] [aria-label="Remove PAED-011"] [aria-haspopup="dialog"] Remove… |
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, Chart-stimulated recall, Mini-CEX, MSF | [link] [aria-label="Edit PAED-012"] Edit [button] [aria-label="Remove PAED-012"] [aria-haspopup="dialog"] Remove… |
| PAED-013 — Managing child protection, legal and ethical aspects of children at risk | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, Mini-CEX, MSF | [link] [aria-label="Edit PAED-013"] Edit [button] [aria-label="Remove PAED-013"] [aria-haspopup="dialog"] Remove… |
| PAED-014 — Providing palliative and end-of-life care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, MSF, Reflective exercise | [link] [aria-label="Edit PAED-014"] Edit [button] [aria-label="Remove PAED-014"] [aria-haspopup="dialog"] Remove… |
| PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 1 per semesterDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, Learner feedback, MSF, Portfolio and logbook review | [link] [aria-label="Edit PAED-015"] Edit [button] [aria-label="Remove PAED-015"] [aria-haspopup="dialog"] Remove… |
| PAED-016 — Transitioning adolescents with long-term conditions to adult care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Mini-CEX, MSF | [link] [aria-label="Edit PAED-016"] Edit [button] [aria-label="Remove PAED-016"] [aria-haspopup="dialog"] Remove… |
| KGK-001 — Running a paediatric outreach clinic at a district hospitalKgosi Kgari Teaching Hospital's own item | 1 per academic yearNo decision cadence | 3aCPSA Paediatric Entrustment Scale v11.1 | — | 12 monthsWeight — | Direct observation | [link] [aria-label="Edit KGK-001 (Kgosi Kgari Teaching Hospital's own item)"] Edit [button] [aria-label="Remove KGK-001 (Kgosi Kgari Teaching Hospital's own item)"] [aria-haspopup="dialog"] Remove… |

######## BOARD R3-B17-901.dc.html
R3-B17-901 · 901
after 6.33 · Demo Administrator on 11.2's items (T4, F3): the College's notice and the national empty line as CanEditCurriculum gives them (N1); KGK's row "Kgosi Kgari Teaching Hospital's own item", every row Edit and Remove…, named with the owner on KGK's
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
A target and its period are read live. Changing one changes what every trainee on this curriculum is measured against, in every institution that has adopted it, including periods that have already closed. A tool list is checked when an activity is filed and submitted, and never takes back credit already earned.
#### Items (17)
Every national EPA of Paediatrics is already on this curriculum, as a national item or as an institution's own item, so there is none left to add. To add an item, first add a national EPA of Paediatrics, or remove a national item to free its EPA. An EPA an institution has as its own item is freed only when that institution removes it.
Paediatric EPA Curriculum (11.2)'s items, in EPA order
| EPA | Target and decision | Minimum level | Minimum by training year | Window and weight | Tools | Actions |
| PAED-001 — Providing paediatric emergency care to children | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | [link] [aria-label="Edit PAED-001"] Edit [button] [aria-label="Remove PAED-001"] [aria-haspopup="dialog"] Remove… |
| PAED-002 — Managing common paediatric presentations | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Chart-stimulated recall, Clinical audit, Direct observation, DOPS, Mini-CEX, MSF, RCA | [link] [aria-label="Edit PAED-002"] Edit [button] [aria-label="Remove PAED-002"] [aria-haspopup="dialog"] Remove… |
| PAED-003 — Providing intensive care to children | 3 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | [link] [aria-label="Edit PAED-003"] Edit [button] [aria-label="Remove PAED-003"] [aria-haspopup="dialog"] Remove… |
| PAED-004 — Managing common neonatal conditions | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-004"] Edit [button] [aria-label="Remove PAED-004"] [aria-haspopup="dialog"] Remove… |
| PAED-005 — Providing neonatal care in intensive and high-care settings | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, DOPS, MSF | [link] [aria-label="Edit PAED-005"] Edit [button] [aria-label="Remove PAED-005"] [aria-haspopup="dialog"] Remove… |
| PAED-006 — Managing long-term health conditions (LTHCs) in children | 2 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CCA, Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-006"] Edit [button] [aria-label="Remove PAED-006"] [aria-haspopup="dialog"] Remove… |
| PAED-007 — Maintaining and promoting the health and well-being of children | 1 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-007"] Edit [button] [aria-label="Remove PAED-007"] [aria-haspopup="dialog"] Remove… |
| PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, DOPS, Mini-CEX, MSF, Reflective exercise | [link] [aria-label="Edit PAED-008"] Edit [button] [aria-label="Remove PAED-008"] [aria-haspopup="dialog"] Remove… |
| PAED-009 — Providing care for an adolescent patient | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, MSF | [link] [aria-label="Edit PAED-009"] Edit [button] [aria-label="Remove PAED-009"] [aria-haspopup="dialog"] Remove… |
| PAED-010 — Leading and operating within a clinical team | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | Direct observation, MSF | [link] [aria-label="Edit PAED-010"] Edit [button] [aria-label="Remove PAED-010"] [aria-haspopup="dialog"] Remove… |
| PAED-011 — Managing population health challenges | 2 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, MSF | [link] [aria-label="Edit PAED-011"] Edit [button] [aria-label="Remove PAED-011"] [aria-haspopup="dialog"] Remove… |
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, Chart-stimulated recall, Mini-CEX, MSF | [link] [aria-label="Edit PAED-012"] Edit [button] [aria-label="Remove PAED-012"] [aria-haspopup="dialog"] Remove… |
| PAED-013 — Managing child protection, legal and ethical aspects of children at risk | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, Mini-CEX, MSF | [link] [aria-label="Edit PAED-013"] Edit [button] [aria-label="Remove PAED-013"] [aria-haspopup="dialog"] Remove… |
| PAED-014 — Providing palliative and end-of-life care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, MSF, Reflective exercise | [link] [aria-label="Edit PAED-014"] Edit [button] [aria-label="Remove PAED-014"] [aria-haspopup="dialog"] Remove… |
| PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 1 per semesterDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, Learner feedback, MSF, Portfolio and logbook review | [link] [aria-label="Edit PAED-015"] Edit [button] [aria-label="Remove PAED-015"] [aria-haspopup="dialog"] Remove… |
| PAED-016 — Transitioning adolescents with long-term conditions to adult care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Mini-CEX, MSF | [link] [aria-label="Edit PAED-016"] Edit [button] [aria-label="Remove PAED-016"] [aria-haspopup="dialog"] Remove… |
| KGK-001 — Running a paediatric outreach clinic at a district hospitalKgosi Kgari Teaching Hospital's own item | 1 per academic yearNo decision cadence | 3aCPSA Paediatric Entrustment Scale v11.1 | — | 12 monthsWeight — | Direct observation | [link] [aria-label="Edit KGK-001 (Kgosi Kgari Teaching Hospital's own item)"] Edit [button] [aria-label="Remove KGK-001 (Kgosi Kgari Teaching Hospital's own item)"] [aria-haspopup="dialog"] Remove… |

######## BOARD R3-B17-1100.dc.html
R3-B17-1100 · 1100
after 6.33 · Demo Administrator on 11.2's items (T4, F3): the College's notice and the national empty line as CanEditCurriculum gives them (N1); KGK's row "Kgosi Kgari Teaching Hospital's own item", every row Edit and Remove…, named with the owner on KGK's
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
A target and its period are read live. Changing one changes what every trainee on this curriculum is measured against, in every institution that has adopted it, including periods that have already closed. A tool list is checked when an activity is filed and submitted, and never takes back credit already earned.
#### Items (17)
Every national EPA of Paediatrics is already on this curriculum, as a national item or as an institution's own item, so there is none left to add. To add an item, first add a national EPA of Paediatrics, or remove a national item to free its EPA. An EPA an institution has as its own item is freed only when that institution removes it.
Paediatric EPA Curriculum (11.2)'s items, in EPA order
| EPA | Target and decision | Minimum level | Minimum by training year | Window and weight | Tools | Actions |
| PAED-001 — Providing paediatric emergency care to children | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | [link] [aria-label="Edit PAED-001"] Edit [button] [aria-label="Remove PAED-001"] [aria-haspopup="dialog"] Remove… |
| PAED-002 — Managing common paediatric presentations | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Chart-stimulated recall, Clinical audit, Direct observation, DOPS, Mini-CEX, MSF, RCA | [link] [aria-label="Edit PAED-002"] Edit [button] [aria-label="Remove PAED-002"] [aria-haspopup="dialog"] Remove… |
| PAED-003 — Providing intensive care to children | 3 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | [link] [aria-label="Edit PAED-003"] Edit [button] [aria-label="Remove PAED-003"] [aria-haspopup="dialog"] Remove… |
| PAED-004 — Managing common neonatal conditions | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-004"] Edit [button] [aria-label="Remove PAED-004"] [aria-haspopup="dialog"] Remove… |
| PAED-005 — Providing neonatal care in intensive and high-care settings | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, DOPS, MSF | [link] [aria-label="Edit PAED-005"] Edit [button] [aria-label="Remove PAED-005"] [aria-haspopup="dialog"] Remove… |
| PAED-006 — Managing long-term health conditions (LTHCs) in children | 2 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CCA, Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-006"] Edit [button] [aria-label="Remove PAED-006"] [aria-haspopup="dialog"] Remove… |
| PAED-007 — Maintaining and promoting the health and well-being of children | 1 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-007"] Edit [button] [aria-label="Remove PAED-007"] [aria-haspopup="dialog"] Remove… |
| PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, DOPS, Mini-CEX, MSF, Reflective exercise | [link] [aria-label="Edit PAED-008"] Edit [button] [aria-label="Remove PAED-008"] [aria-haspopup="dialog"] Remove… |
| PAED-009 — Providing care for an adolescent patient | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, MSF | [link] [aria-label="Edit PAED-009"] Edit [button] [aria-label="Remove PAED-009"] [aria-haspopup="dialog"] Remove… |
| PAED-010 — Leading and operating within a clinical team | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | Direct observation, MSF | [link] [aria-label="Edit PAED-010"] Edit [button] [aria-label="Remove PAED-010"] [aria-haspopup="dialog"] Remove… |
| PAED-011 — Managing population health challenges | 2 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, MSF | [link] [aria-label="Edit PAED-011"] Edit [button] [aria-label="Remove PAED-011"] [aria-haspopup="dialog"] Remove… |
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, Chart-stimulated recall, Mini-CEX, MSF | [link] [aria-label="Edit PAED-012"] Edit [button] [aria-label="Remove PAED-012"] [aria-haspopup="dialog"] Remove… |
| PAED-013 — Managing child protection, legal and ethical aspects of children at risk | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, Mini-CEX, MSF | [link] [aria-label="Edit PAED-013"] Edit [button] [aria-label="Remove PAED-013"] [aria-haspopup="dialog"] Remove… |
| PAED-014 — Providing palliative and end-of-life care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, MSF, Reflective exercise | [link] [aria-label="Edit PAED-014"] Edit [button] [aria-label="Remove PAED-014"] [aria-haspopup="dialog"] Remove… |
| PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 1 per semesterDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, Learner feedback, MSF, Portfolio and logbook review | [link] [aria-label="Edit PAED-015"] Edit [button] [aria-label="Remove PAED-015"] [aria-haspopup="dialog"] Remove… |
| PAED-016 — Transitioning adolescents with long-term conditions to adult care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Mini-CEX, MSF | [link] [aria-label="Edit PAED-016"] Edit [button] [aria-label="Remove PAED-016"] [aria-haspopup="dialog"] Remove… |
| KGK-001 — Running a paediatric outreach clinic at a district hospitalKgosi Kgari Teaching Hospital's own item | 1 per academic yearNo decision cadence | 3aCPSA Paediatric Entrustment Scale v11.1 | — | 12 monthsWeight — | Direct observation | [link] [aria-label="Edit KGK-001 (Kgosi Kgari Teaching Hospital's own item)"] Edit [button] [aria-label="Remove KGK-001 (Kgosi Kgari Teaching Hospital's own item)"] [aria-haspopup="dialog"] Remove… |

######## BOARD R3-B18.dc.html
R3-B18 · 1280
Nolwazi Mbatha · loading (T7: titled by what it knows): the trail, one loading line (S19) and a skeleton, no action
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
Loading this curriculum's items.

######## BOARD R3-B18-390.dc.html
R3-B18-390 · 390
Nolwazi Mbatha · loading (T7: titled by what it knows): the trail, one loading line (S19) and a skeleton, no action
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
Loading this curriculum's items.

######## BOARD R3-B19.dc.html
R3-B19 · 1280
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

######## BOARD R3-B19-390.dc.html
R3-B19-390 · 390
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

######## BOARD R3-B20.dc.html
IllustrationR3-B20 · 1280
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
| ‹code 1› — ‹title›Your institution's own item | ‹target›‹decision› | ‹rung› | — | ‹…› | ‹tools› | [link] [aria-label="Edit ‹code 1›"] Edit [button] [aria-label="Remove ‹code 1›"] [aria-haspopup="dialog"] Remove… |
| ‹code 2› — ‹title›Your institution's own item | ‹target›‹decision› | ‹rung› | — | ‹…› | ‹tools› | [link] [aria-label="Edit ‹code 2›"] Edit [button] [aria-label="Remove ‹code 2›"] [aria-haspopup="dialog"] Remove… |
| ‹code 3› — ‹title›Your institution's own item | ‹target›‹decision› | ‹rung› | — | ‹…› | ‹tools› | [link] [aria-label="Edit ‹code 3›"] Edit [button] [aria-label="Remove ‹code 3›"] [aria-haspopup="dialog"] Remove… |

######## BOARD R3-B20-390.dc.html
IllustrationR3-B20-390 · 390
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
| ‹code 1› — ‹title›Your institution's own item | ‹target›‹decision› | ‹rung› | — | ‹…› | ‹tools› | [link] [aria-label="Edit ‹code 1›"] Edit [button] [aria-label="Remove ‹code 1›"] [aria-haspopup="dialog"] Remove… |
| ‹code 2› — ‹title›Your institution's own item | ‹target›‹decision› | ‹rung› | — | ‹…› | ‹tools› | [link] [aria-label="Edit ‹code 2›"] Edit [button] [aria-label="Remove ‹code 2›"] [aria-haspopup="dialog"] Remove… |
| ‹code 3› — ‹title›Your institution's own item | ‹target›‹decision› | ‹rung› | — | ‹…› | ‹tools› | [link] [aria-label="Edit ‹code 3›"] Edit [button] [aria-label="Remove ‹code 3›"] [aria-haspopup="dialog"] Remove… |

######## BOARD R3-B20-768.dc.html
IllustrationR3-B20-768 · 768
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
| ‹code 1› — ‹title›Your institution's own item | ‹target›‹decision› | ‹rung› | — | ‹…› | ‹tools› | [link] [aria-label="Edit ‹code 1›"] Edit [button] [aria-label="Remove ‹code 1›"] [aria-haspopup="dialog"] Remove… |
| ‹code 2› — ‹title›Your institution's own item | ‹target›‹decision› | ‹rung› | — | ‹…› | ‹tools› | [link] [aria-label="Edit ‹code 2›"] Edit [button] [aria-label="Remove ‹code 2›"] [aria-haspopup="dialog"] Remove… |
| ‹code 3› — ‹title›Your institution's own item | ‹target›‹decision› | ‹rung› | — | ‹…› | ‹tools› | [link] [aria-label="Edit ‹code 3›"] Edit [button] [aria-label="Remove ‹code 3›"] [aria-haspopup="dialog"] Remove… |

######## BOARD R3-B21.dc.html
R3-B21 · 1280
after 6.36 (as read at 6.38) · Nolwazi Mbatha · the superseded version's items (F6, S12): no Add; KGK's own item keeps Edit and Remove… for the 2 registrars still on 11.1
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
College of Paediatricians of South Africa · Paediatrics / Paediatrics · effective 2026-01-01 · superseded on 2026-10-30 by Paediatric EPA Curriculum (11.2); 2 registrars still on it
The College sets this curriculum's national items, so they are read only here. Your institution's own items apply only to its trainees, and you can add, edit and remove them. A target and its period are read live: changing one changes what every trainee of your institution on this curriculum is measured against, including periods that have already closed. A tool list is checked when an activity is filed and submitted, and never takes back credit already earned.
#### Items (16)
Your institution has superseded this version, so no item is added to it. Its own items stay for the registrars still on it.
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
| KGK-001 — Running a paediatric outreach clinic at a district hospitalYour institution's own item | 1 per academic yearNo decision cadence | 3aCPSA Paediatric Entrustment Scale v11.1 | — | 12 monthsWeight — | Direct observation | [link] [aria-label="Edit KGK-001"] Edit [button] [aria-label="Remove KGK-001"] [aria-haspopup="dialog"] Remove… |

######## BOARD R3-B21-390.dc.html
R3-B21-390 · 390
after 6.36 (as read at 6.38) · Nolwazi Mbatha · the superseded version's items (F6, S12): no Add; KGK's own item keeps Edit and Remove… for the 2 registrars still on 11.1
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
College of Paediatricians of South Africa · Paediatrics / Paediatrics · effective 2026-01-01 · superseded on 2026-10-30 by Paediatric EPA Curriculum (11.2); 2 registrars still on it
The College sets this curriculum's national items, so they are read only here. Your institution's own items apply only to its trainees, and you can add, edit and remove them. A target and its period are read live: changing one changes what every trainee of your institution on this curriculum is measured against, including periods that have already closed. A tool list is checked when an activity is filed and submitted, and never takes back credit already earned.
#### Items (16)
Your institution has superseded this version, so no item is added to it. Its own items stay for the registrars still on it.
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
| KGK-001 — Running a paediatric outreach clinic at a district hospitalYour institution's own item | 1 per academic yearNo decision cadence | 3aCPSA Paediatric Entrustment Scale v11.1 | — | 12 monthsWeight — | Direct observation | [link] [aria-label="Edit KGK-001"] Edit [button] [aria-label="Remove KGK-001"] [aria-haspopup="dialog"] Remove… |

######## BOARD R3-B22.dc.html
R3-B22 · 1280
6.30 · Anton Kruger, College admin · the add page (F3, M3): PAED-016 the one option, chosen; the 6.30 values typed, minimum 4 chosen, so Add item is on
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
[link] [aria-current="true"] Curricula
-
[link] Activity types
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
On Paediatric EPA Curriculum (11.2). It applies to every institution that has adopted it.
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

######## BOARD R3-B22-390.dc.html
R3-B22-390 · 390
6.30 · Anton Kruger, College admin · the add page (F3, M3): PAED-016 the one option, chosen; the 6.30 values typed, minimum 4 chosen, so Add item is on
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
[link] [aria-current="true"] Curricula
-
[link] Activity types
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
On Paediatric EPA Curriculum (11.2). It applies to every institution that has adopted it.
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

######## BOARD R3-B23.dc.html
R3-B23 · 1280
6.30 · Anton Kruger · back on the list by /admin/curricula/4/items?added=‹item id› (F1), the token stripped once read; 16 rows, PAED-016 last, PAED-011 still "1 per academic year"; the national empty line where the action was
Focus: "Curriculum item added."
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
[link] [aria-current="true"] Curricula
-
[link] Activity types
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
College of Paediatricians of South Africa · Paediatrics / Paediatrics · effective 2026-10-30
Curriculum item added.
A target and its period are read live. Changing one changes what every trainee on this curriculum is measured against, in every institution that has adopted it, including periods that have already closed. A tool list is checked when an activity is filed and submitted, and never takes back credit already earned.
#### Items (16)
Every national EPA of Paediatrics is already on this curriculum, as a national item or as an institution's own item, so there is none left to add. To add an item, first add a national EPA of Paediatrics, or remove a national item to free its EPA. An EPA an institution has as its own item is freed only when that institution removes it.
Paediatric EPA Curriculum (11.2)'s items, in EPA order
| EPA | Target and decision | Minimum level | Minimum by training year | Window and weight | Tools | Actions |
| PAED-001 — Providing paediatric emergency care to children | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | [link] [aria-label="Edit PAED-001"] Edit [button] [aria-label="Remove PAED-001"] [aria-haspopup="dialog"] Remove… |
| PAED-002 — Managing common paediatric presentations | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Chart-stimulated recall, Clinical audit, Direct observation, DOPS, Mini-CEX, MSF, RCA | [link] [aria-label="Edit PAED-002"] Edit [button] [aria-label="Remove PAED-002"] [aria-haspopup="dialog"] Remove… |
| PAED-003 — Providing intensive care to children | 3 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | [link] [aria-label="Edit PAED-003"] Edit [button] [aria-label="Remove PAED-003"] [aria-haspopup="dialog"] Remove… |
| PAED-004 — Managing common neonatal conditions | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-004"] Edit [button] [aria-label="Remove PAED-004"] [aria-haspopup="dialog"] Remove… |
| PAED-005 — Providing neonatal care in intensive and high-care settings | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, DOPS, MSF | [link] [aria-label="Edit PAED-005"] Edit [button] [aria-label="Remove PAED-005"] [aria-haspopup="dialog"] Remove… |
| PAED-006 — Managing long-term health conditions (LTHCs) in children | 2 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CCA, Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-006"] Edit [button] [aria-label="Remove PAED-006"] [aria-haspopup="dialog"] Remove… |
| PAED-007 — Maintaining and promoting the health and well-being of children | 1 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-007"] Edit [button] [aria-label="Remove PAED-007"] [aria-haspopup="dialog"] Remove… |
| PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, DOPS, Mini-CEX, MSF, Reflective exercise | [link] [aria-label="Edit PAED-008"] Edit [button] [aria-label="Remove PAED-008"] [aria-haspopup="dialog"] Remove… |
| PAED-009 — Providing care for an adolescent patient | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, MSF | [link] [aria-label="Edit PAED-009"] Edit [button] [aria-label="Remove PAED-009"] [aria-haspopup="dialog"] Remove… |
| PAED-010 — Leading and operating within a clinical team | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | Direct observation, MSF | [link] [aria-label="Edit PAED-010"] Edit [button] [aria-label="Remove PAED-010"] [aria-haspopup="dialog"] Remove… |
| PAED-011 — Managing population health challenges | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, MSF | [link] [aria-label="Edit PAED-011"] Edit [button] [aria-label="Remove PAED-011"] [aria-haspopup="dialog"] Remove… |
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, Chart-stimulated recall, Mini-CEX, MSF | [link] [aria-label="Edit PAED-012"] Edit [button] [aria-label="Remove PAED-012"] [aria-haspopup="dialog"] Remove… |
| PAED-013 — Managing child protection, legal and ethical aspects of children at risk | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, Mini-CEX, MSF | [link] [aria-label="Edit PAED-013"] Edit [button] [aria-label="Remove PAED-013"] [aria-haspopup="dialog"] Remove… |
| PAED-014 — Providing palliative and end-of-life care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, MSF, Reflective exercise | [link] [aria-label="Edit PAED-014"] Edit [button] [aria-label="Remove PAED-014"] [aria-haspopup="dialog"] Remove… |
| PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 1 per semesterDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, Learner feedback, MSF, Portfolio and logbook review | [link] [aria-label="Edit PAED-015"] Edit [button] [aria-label="Remove PAED-015"] [aria-haspopup="dialog"] Remove… |
| PAED-016 — Transitioning adolescents with long-term conditions to adult care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Mini-CEX, MSF | [link] [aria-label="Edit PAED-016"] Edit [button] [aria-label="Remove PAED-016"] [aria-haspopup="dialog"] Remove… |

######## BOARD R3-B23-390.dc.html
R3-B23-390 · 390
6.30 · Anton Kruger · back on the list by /admin/curricula/4/items?added=‹item id› (F1), the token stripped once read; 16 rows, PAED-016 last, PAED-011 still "1 per academic year"; the national empty line where the action was
Focus: "Curriculum item added."
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
[link] [aria-current="true"] Curricula
-
[link] Activity types
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
College of Paediatricians of South Africa · Paediatrics / Paediatrics · effective 2026-10-30
Curriculum item added.
A target and its period are read live. Changing one changes what every trainee on this curriculum is measured against, in every institution that has adopted it, including periods that have already closed. A tool list is checked when an activity is filed and submitted, and never takes back credit already earned.
#### Items (16)
Every national EPA of Paediatrics is already on this curriculum, as a national item or as an institution's own item, so there is none left to add. To add an item, first add a national EPA of Paediatrics, or remove a national item to free its EPA. An EPA an institution has as its own item is freed only when that institution removes it.
Paediatric EPA Curriculum (11.2)'s items, in EPA order
| EPA | Target and decision | Minimum level | Minimum by training year | Window and weight | Tools | Actions |
| PAED-001 — Providing paediatric emergency care to children | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | [link] [aria-label="Edit PAED-001"] Edit [button] [aria-label="Remove PAED-001"] [aria-haspopup="dialog"] Remove… |
| PAED-002 — Managing common paediatric presentations | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Chart-stimulated recall, Clinical audit, Direct observation, DOPS, Mini-CEX, MSF, RCA | [link] [aria-label="Edit PAED-002"] Edit [button] [aria-label="Remove PAED-002"] [aria-haspopup="dialog"] Remove… |
| PAED-003 — Providing intensive care to children | 3 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise | [link] [aria-label="Edit PAED-003"] Edit [button] [aria-label="Remove PAED-003"] [aria-haspopup="dialog"] Remove… |
| PAED-004 — Managing common neonatal conditions | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, CCA, Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-004"] Edit [button] [aria-label="Remove PAED-004"] [aria-haspopup="dialog"] Remove… |
| PAED-005 — Providing neonatal care in intensive and high-care settings | 3 per semesterDecided each semester, by the Neonatal team Clinical Competency Committee | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, DOPS, MSF | [link] [aria-label="Edit PAED-005"] Edit [button] [aria-label="Remove PAED-005"] [aria-haspopup="dialog"] Remove… |
| PAED-006 — Managing long-term health conditions (LTHCs) in children | 2 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CCA, Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-006"] Edit [button] [aria-label="Remove PAED-006"] [aria-haspopup="dialog"] Remove… |
| PAED-007 — Maintaining and promoting the health and well-being of children | 1 per semesterDecided each academic year | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | Direct observation, DOPS, Mini-CEX, MSF | [link] [aria-label="Edit PAED-007"] Edit [button] [aria-label="Remove PAED-007"] [aria-haspopup="dialog"] Remove… |
| PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, DOPS, Mini-CEX, MSF, Reflective exercise | [link] [aria-label="Edit PAED-008"] Edit [button] [aria-label="Remove PAED-008"] [aria-haspopup="dialog"] Remove… |
| PAED-009 — Providing care for an adolescent patient | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, MSF | [link] [aria-label="Edit PAED-009"] Edit [button] [aria-label="Remove PAED-009"] [aria-haspopup="dialog"] Remove… |
| PAED-010 — Leading and operating within a clinical team | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | Direct observation, MSF | [link] [aria-label="Edit PAED-010"] Edit [button] [aria-label="Remove PAED-010"] [aria-haspopup="dialog"] Remove… |
| PAED-011 — Managing population health challenges | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, MSF | [link] [aria-label="Edit PAED-011"] Edit [button] [aria-label="Remove PAED-011"] [aria-haspopup="dialog"] Remove… |
| PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams | 3 per semesterDecided each semester | 5CPSA Paediatric Entrustment Scale v11.1 | 3a / 3b / 4 / 5 | 12 monthsWeight — | CBD, Chart-stimulated recall, Mini-CEX, MSF | [link] [aria-label="Edit PAED-012"] Edit [button] [aria-label="Remove PAED-012"] [aria-haspopup="dialog"] Remove… |
| PAED-013 — Managing child protection, legal and ethical aspects of children at risk | 1 per academic yearDecided each academic year, as opportunity allows | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, Mini-CEX, MSF | [link] [aria-label="Edit PAED-013"] Edit [button] [aria-label="Remove PAED-013"] [aria-haspopup="dialog"] Remove… |
| PAED-014 — Providing palliative and end-of-life care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, MSF, Reflective exercise | [link] [aria-label="Edit PAED-014"] Edit [button] [aria-label="Remove PAED-014"] [aria-haspopup="dialog"] Remove… |
| PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | 1 per semesterDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Direct observation, Learner feedback, MSF, Portfolio and logbook review | [link] [aria-label="Edit PAED-015"] Edit [button] [aria-label="Remove PAED-015"] [aria-haspopup="dialog"] Remove… |
| PAED-016 — Transitioning adolescents with long-term conditions to adult care | 1 per academic yearDecided each academic year | 4CPSA Paediatric Entrustment Scale v11.1 | 2 / 3a / 3b / 4 | 12 monthsWeight — | CBD, Mini-CEX, MSF | [link] [aria-label="Edit PAED-016"] Edit [button] [aria-label="Remove PAED-016"] [aria-haspopup="dialog"] Remove… |

######## BOARD R3-B24.dc.html
R3-B24 · 1280
6.30 · Anton Kruger · Edit PAED-011 opens its page (F3, M3): EPA a select offering PAED-011 alone, chosen; Target typed 2, nothing else changed; Cancel, Save item
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
[link] [aria-current="true"] Curricula
-
[link] Activity types
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
On Paediatric EPA Curriculum (11.2). It applies to every institution that has adopted it.
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

######## BOARD R3-B24-390.dc.html
R3-B24-390 · 390
6.30 · Anton Kruger · Edit PAED-011 opens its page (F3, M3): EPA a select offering PAED-011 alone, chosen; Target typed 2, nothing else changed; Cancel, Save item
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
[link] [aria-current="true"] Curricula
-
[link] Activity types
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
On Paediatric EPA Curriculum (11.2). It applies to every institution that has adopted it.
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

######## BOARD R3-C08.dc.html
R3-C08 · 1280
6.25 · Nolwazi Mbatha · saved: /admin/epas/17 reached by /admin/epas/17?created (F1, M5), the token gone from the address once read; the page wrote "EPA saved." itself; titled by the EPA, crumb KGK-001, no Back to EPAs; Status Active with Q11's help; Cancel, Deactivate…, Save
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
[button] [aria-haspopup="dialog"] Deactivate…
[button] Save

######## BOARD R3-C08-390.dc.html
R3-C08-390 · 390
6.25 · Nolwazi Mbatha · saved: /admin/epas/17 reached by /admin/epas/17?created (F1, M5), the token gone from the address once read; the page wrote "EPA saved." itself; titled by the EPA, crumb KGK-001, no Back to EPAs; Status Active with Q11's help; Cancel, Deactivate…, Save
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
[button] [aria-haspopup="dialog"] Deactivate…
[button] Save

######## BOARD R3-C09.dc.html
R3-C09 · 1280
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

######## BOARD R3-C09-390.dc.html
R3-C09-390 · 390
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

######## BOARD R3-C09-768.dc.html
R3-C09-768 · 768
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

######## BOARD R3-C12.dc.html
R3-C12 · 1280
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
[button] [aria-haspopup="dialog"] Deactivate…
[button] Save
#### Deactivate KGK-001?
It can no longer be chosen for a new activity, and it stops being a target on running programmes' progress pages and dashboards, for Kgosi Kgari Teaching Hospital's trainees. A programme that has ended keeps it on its record, marked no longer in use. The progress it has earned is kept. An activity completed against it while it is inactive counts towards nothing until it is reactivated. You can reactivate it on this page.
[button] Cancel
[button] Deactivate

######## BOARD R3-C12-390.dc.html
R3-C12-390 · 390
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
[button] [aria-haspopup="dialog"] Deactivate…
[button] Save
#### Deactivate KGK-001?
It can no longer be chosen for a new activity, and it stops being a target on running programmes' progress pages and dashboards, for Kgosi Kgari Teaching Hospital's trainees. A programme that has ended keeps it on its record, marked no longer in use. The progress it has earned is kept. An activity completed against it while it is inactive counts towards nothing until it is reactivated. You can reactivate it on this page.
[button] Cancel
[button] Deactivate

######## BOARD R3-C15.dc.html
IllustrationR3-C15 · 1280
Illustration (the volume) · Demo Administrator · PagerControls (S14) · 40 EPAs across CPSA, Demo College, KGK's own and ‹institution›'s own; the College column kept (S6); two own EPAs sharing "KGK-001" read apart by their marks; Edit on every row
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
[aria-label="Pages of EPAs"] 1–‹n› of 40
[button] [aria-disabled="true"] Previous
[button] Next
EPAs per page
‹n›

######## BOARD R3-C15-390.dc.html
IllustrationR3-C15-390 · 390
Illustration (the volume) · Demo Administrator · PagerControls (S14) · 40 EPAs across CPSA, Demo College, KGK's own and ‹institution›'s own; the College column kept (S6); two own EPAs sharing "KGK-001" read apart by their marks; Edit on every row
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
[aria-label="Pages of EPAs"] 1–‹n› of 40
[button] [aria-disabled="true"] Previous
[button] Next
EPAs per page
‹n›

######## BOARD R3-C15-768.dc.html
IllustrationR3-C15-768 · 768
Illustration (the volume) · Demo Administrator · PagerControls (S14) · 40 EPAs across CPSA, Demo College, KGK's own and ‹institution›'s own; the College column kept (S6); two own EPAs sharing "KGK-001" read apart by their marks; Edit on every row
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
[aria-label="Pages of EPAs"] 1–‹n› of 40
[button] [aria-disabled="true"] Previous
[button] Next
EPAs per page
‹n›

######## BOARD R3-C16.dc.html
R3-C16 · 1280
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
Loading the EPAs.

######## BOARD R3-C16-390.dc.html
R3-C16-390 · 390
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
Loading the EPAs.

######## BOARD R3-C17.dc.html
R3-C17 · 1280
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

######## BOARD R3-C17-390.dc.html
R3-C17-390 · 390
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

######## BOARD R3-C18a.dc.html
R3-C18a · 1280
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
Loading this EPA.

######## BOARD R3-C18a-390.dc.html
R3-C18a-390 · 390
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
Loading this EPA.

######## BOARD R3-C18b.dc.html
R3-C18b · 1280
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

######## BOARD R3-C18b-390.dc.html
R3-C18b-390 · 390
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

######## BOARD R3-D03.dc.html
R3-D03 · 1280
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

######## BOARD R3-D03-390.dc.html
R3-D03-390 · 390
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

######## BOARD R3-E03.dc.html
R3-E03 · 1280
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
Loading the entrustment scales.

######## BOARD R3-E03-390.dc.html
R3-E03-390 · 390
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
Loading the entrustment scales.

######## BOARD R3-E04.dc.html
R3-E04 · 1280
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

######## BOARD R3-E04-390.dc.html
R3-E04-390 · 390
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

######## BOARD R3-F02.dc.html
R3-F02 · 1280
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

######## BOARD R3-F02-390.dc.html
R3-F02-390 · 390
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
