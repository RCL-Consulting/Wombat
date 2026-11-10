# Flow 17 round 3: the check

Round 3 (canvas "Wombat 17 · Round 3", saved flat in `round-3/`: 87 boards + `f17r3.css` + `canvas.json`) was checked item by item against
`round-3-ask.txt`, 2026-11-10. The ask numbers **70 lettered ids**: M1-M5, S1-S29, N1-N24, Q17-Q20 and T1-T8. Its stylesheet block (C1, R1, C2, S2, C3),
its sweep (tabs, outline, list, header, 44 px, sideways) and its boards-whole duties (reference files first, the CHECK BEFORE YOU STOP list, counts, Spec
and Steps whole) add **16 more rows**, so this check has **86 rows**. (The brief for this check says 89 items; the ask holds no list that reaches 89, so
the figure was not reproduced.) The canvas's own check boards (R3-0-check, check-2) were not trusted: each claim was re-derived from the boards' markup
and text.

Method. Scripts (Python stdlib, `python -I`) in `C:\Users\Renier\AppData\Local\Temp\claude\67560d182d14635f\5ae5486c-8726-4e89-ae35-c60429e28267\scratchpad\chk17`
(`lib.py` the parser, `chk.py` the per-board summary, `diff.py`/`wdiff.py` the R2 diffs, `css.py` the stylesheet, `view.py` a board's outline). Over all 87 files:
the inventory against the ask's lists; every `required-mark` against its control's `aria-required="true"`, the hidden "required", and the "* marks a
field you must fill in." line, with where the line sits; every `button`/`a` `aria-label` against its visible words; `aria-label`, `aria-describedby` and
the badge markup on every tab; inline `style=`, `<style>` bodies, `.tab-problem`, `.field-row-actions`, `.wf-*`, `.catalogue-record`; brackets
`‹…›` as an input, textarea, select or option value; ids with spaces or brackets, duplicate ids, and every `for`, `aria-labelledby`, `aria-describedby`
and `aria-controls` resolving on its board (and any `aria-describedby=""`); one `h1`; gendered words in the whole text of every file, annotations
included (he, she, his, her, hers, him, himself, herself); each 390 and 768 twin's article against its 1280 board, markup and words; each redrawn board
against its R2 file twice (a markup diff, then a words-and-attributes diff, so a change is told from a regression); each chip and "Illustration" title
against S15's rule; the outline's list markup; every `Done` button's `aria-controls` against the editor that follows; tabpanel `tabindex` against whether
the panel holds a focusable control; the four `dialog` bodies (E05, E07, E11, E16); the Steps arithmetic; the five `states.md` rows; every PART 2 rule of `f17r3.css` against the
Spec's quote; every contrast figure recomputed from `app.css`'s tokens; 24 Spec citations of today's code opened in `src/`.

**Not rendered, not measured.** No board was rendered by this check (the boards link `ds/wombat/…`, which `round-3/` does not hold). Every pixel figure
(tab tracks, rail widths, button runs, header positions, 44 px, sideways) is the canvas's own and is marked "reported, not measured" below. Where an
independent route existed (the list's arithmetic, the 952 count, the contrast ratios) it was taken and is said.

## Verdict

**86 rows: done 82, partly 4, not done 0.** The four partly rows are S2 (the stylesheet block: two new margin rules the Spec quotes but does not flag),
C3 (the Spec and the stand-in say "six selector lists" and "nine rules"; `app.css` holds seven lists and the stand-in is seven rules), Spec whole (round 2's
Accessibility rows, the "100 rows" row and the 768 rows are not restated; the supersession list is short) and Steps whole (seven rows read "As round 2's
row" on a canvas that does not hold round 2). One **new defect** against round 2: `li role="group"` for the editor in place, inside the new `ul` (17 files).
No board in the ask's lists is missing and no other regression of what held in round 2. Round 3 is fit to build from once "For the operator" is read; the
new defect and the C3 miscount want fixing first.

## Inventory

| What | Ask | Found |
|---|---|---|
| Redrawn at 1280 | 32 | 32 (A09-nocreate, A12, B07, C01-C07, C08-list, C10-C15-before/after, C17-C20, D01, D04, D05, E01, E02, E04, E05, E07, E11, E12) |
| Their 390 twins | 32 | 32 |
| Redrawn at 768 | 2 | 2 (C19-768, E04-768) |
| New at 1280 / 390 | 7 / 7 | 7 / 7 (A13, C18b, C22, C23, E02b, E16, E17) |
| Check, Spec, Steps | about 7 | 7 (check, check-2, spec, spec-2, spec-3, spec-4, steps) |
| Total boards | 87 | 87 (no extra, no missing, by name) |
| Page boards / twin pairs | 80 | 80; 41 pairs, each pair identical (below) |
| Every R2 source for a redrawn board | present | present for all 66 redrawn; the 14 new ones start from the R2 file the ask names |
| `f17r3.css`, `canvas.json` | project file | both present |

Every board's `<head>` links `ds/wombat/tokens.css`, `ds/wombat/components/bundle.css`, `f17r3.css`, in that order, with no `/_blob/` stylesheet (87 of 87);
every `<style>` holds `body{margin:0}` and nothing else; no inline `style=` (0); no raw hex in `f17r3.css` (0); the brand mark is
`/_blob/1464be2bd2c027ed86bec42ee064aa32` on all 80 page boards and `a89fec…` (round 2's) on none; one `h1` a board (0 exceptions); no duplicate id; no id
holding a bracket or a space; **0 unresolved** `for`/`aria-*` references and **0** `aria-describedby=""`; no bracket as a control's value (0); no gendered word in
any of the 87 files, annotations included (0); no `.tab-problem`, `.field-row-actions`, `.wf-*` or `.catalogue-record` on any board or in `f17r3.css` (0).
Every 390 and 768 twin's article is identical to its 1280 board in markup and words (41 of 41 pairs; only the title's width suffix differs), and its chip and
title agree.

## M items

| Item | Verdict | Evidence | What remains |
|---|---|---|---|
| M1 required fields | Done | The line "* marks a field you must fill in." as `p.muted`: once on Details under Q14's line (C01-C07, C15-before, C22, E02b, E11, E12, E16, E17; checked that its previous sibling is "Saved with Save draft…" and the next is the first field) and once in each editor in place directly under its `h4` (C10-C14, C15-after, C17, C19, C23). Every starred label's control has `aria-required="true"` (0 unmatched): Key, Name, Sub-speciality (C05), Section key, Title, Field key, Label, Type, Options (C12, C14, C15-after, C19). Scope (C04-C07): `fieldset.choice-group role="radiogroup" aria-required="true"`, legend "Scope *" with the hidden "required". Every `.required-mark` is `aria-hidden` except the preview's own line (`p.field-help`, as placed: the ask keeps it); every starred label carries the hidden "required". A read-out takes none (E02b, E11, E12, E16, E17: Key read-out, one mark, Name only). | None. |
| M2 names | Done | (a) Seven boards (C14, C15-before, C15-after, C20, D01, D04, C22, each with its twin): the tab is `Form <span class="badge badge-declined">1<span> problem</span></span>`; **0 tabs carry `aria-label` or `aria-describedby` on any of the 80 page boards**. D01 reads "Workflow 1 problem", D04 "Credit 1 problem", C22 "Details 1 problem". (b) C19: six folds "Hide fields of section ‹section 1›" / "Show fields of section ‹section 2›…"; no `aria-expanded`; `aria-controls` `fields-section-n`, each resolving to the section's `ul`. Every name that has an `aria-label` contains its visible words (0 mismatches over all boards). | 350 row buttons "Move … up/down" contain "Up"/"Down" at the end, not the start (passes WCAG 2.5.3, which asks for containment; as round 2 had them). |
| M3 | Done | Steps § Counts: 1.27 = 15 + 23 / 15 + 25; A.7.9 = 3 + 1; 1.26-1.30 = 24 + 28 / 24 + 30 (arithmetic below). | None. |
| M4 | Done | R3-E02: Credit tab selected, "Published version 1." in the region, Credit rules JSON laid out (`{\n  "counts_for": []\n}`), no Discard draft…, Publish `aria-disabled` with `aria-describedby="publish-reason"` and "Save a draft to publish.", strip "Version 1, 2026-11-05, Nolwazi Mbatha" / "None". Steps T2. | None. |
| M5 | Done | Steps § states.md: rows 902, 904, 906, 911, 912 with the new words and boards; each row's subject re-read against `execution/knowledge/scenario-paediatrics/states.md` lines 902 (Searched), 904 (All), 906 (Loading), 911 (User field editor), 912 (Duplicate key): all five match. | None. |

## S items

| Item | Verdict | Evidence | What remains |
|---|---|---|---|
| S1 RefusalSummary | Done | Spec-2 § 3 row: callback in place of the fixed `FocusFieldAsync`, the id to focus, the prefix made by the page. `RefusalSummary.razor:25` and `:70` opened: `FocusFieldAsync` and the `@onclick` it is wired to are on those lines. | None. |
| S2 RefusalPlace | Done | Spec-2 § 3 "The refusal (S2)". `ActorFieldRules.cs:155-217` and `EvidenceEpa.cs:60-143` are method-sized spans as cited. | The type argument in "IReadOnlyList<RefusalPlace>" is unescaped HTML on R3-0-spec-2, so a browser drops "<RefusalPlace>" and shows "IReadOnlyList (RefusalPlace: …". |
| S3 refusal rule | Done | Spec-2 § 3 "The refusal rule (S3)"; the two guards cited exist (`SaveActivityTypeDraftCommand.cs:87`, `ActivityType.cs:134`). | None. |
| S4 | Done | R3-D04: summary "Credit: Credit rule 1 reads its EPA from field 'epa', but the form's EPA field is 'epa_id'. The EPA an activity is filed against and the EPA it credits must be the same field." (`EvidenceEpa.cs:106-108`, word for word); under the JSON "Rule 1 reads its EPA from 'epa', but this form's EPA field is 'epa_id'. Use 'epa_id'." Spec § 9 lists both beside what each replaces. | None. |
| S5 PublishAsync | Done | Spec-2 § 3 ActivityTypeEdit row; `ActivityTypeEdit.razor:915` tests only `_inFlight` and `CanWrite`, as cited. Q16's "(as it already does for a new type)" appears nowhere in round 3. | None. |
| S6 | Done | Spec § 3 and § 9: "Save draft to see what publishing changes." (NEW); the live alert (`:50-61`, `RefreshWarnings :1107-1119`, both opened) goes. No board draws an alert above a tab bar. | None. |
| S7 | Done | R3-E05 (+390): `dialog` confirm `btn btn-primary` "Publish version 2", Cancel `btn-outline` with `autofocus`; the header Publish stays `btn-success`. `ConfirmDialog.razor:46` and `:78` render `btn-primary` for a non-danger action. | None. |
| S8 | Done | Spec-2 § 3 ActivityForm row; `ActivityForm.razor:1211-1214` loads its own options, `ActivityTypeEdit.razor:508` passes none and `SectionHeadingLevel="4"`, as cited. | None. |
| S9 ActorRuleWords | Done | Spec-2 § 3 row gives the whole grammar: subject, creator, `a|b` with "or", `role:X` with its article, `field:k`, `a+b` "who is also", both scopes (the sub-speciality one NEW), the three validation words, the fallback in `<code>`. | None. |
| S10 | Done | R3-C22 (+390): region holds the summary ("Nothing was saved." / "Everything you typed is kept." / "Details: An activity type with key 'kgk_teaching_log' already exists."); Key `aria-invalid="true"`, `aria-describedby="f-key-err f-key-help"` (refusal first), the same sentence under Key; tab "Details 1 problem". Spec § 3, § 9, § 10 carry the four words and the validator row; `SaveActivityTypeDraftCommand.cs:24` is `[NoValidator]`, `:92` throws the key-taken sentence, `CollegeHolders.cs:31` and `:33` hold "Enter a name." and "A name is at most 200 characters.". | None. |
| S11 | Done | Spec-2 § 3 parser row; `ActivityTypeSeedCatalogue.cs:272-274` and `ActivityTypePublishCheckTests.cs:117-118` are the callers cited; `ActivityTypeEdit.razor:836-837` reloads both JSON texts. | None. |
| S12 | Done | Spec-2 § 3 "Entrustment scales (S12)"; `ActivityReferenceDataService.cs:439-447` returns `scale.Id` and name only, as cited. | None. |
| S13 | Done | R3-C23 (+390): Topic's row with Edit; the new row "New field" (Text · `field_1`), Done `aria-expanded="true"` `aria-controls="fe-field-1"`, the editor an `li` next, Field key `field_1`, Label "New field", Type Text selected, Required unticked. No chip. Spec § 6 and Steps T5. | Focus on Field key is not drawn (a static board). |
| S14 | Done | R3-A12: Draft cell "Saved ‹date›" (the only change from R2). | None. |
| S15 | Done | The chip and "Illustration" in the title agree on all 80 page boards. Redrawn boards with both: A09-nocreate, A12, B07, C07, C08-list, C15-before, C15-after, C17, C18, C18b, C19, C20, C22, D04, D05, E01, E05, E11, E12, E16, E17 (21); without either: A13, C23, E02b, E04, E07 and the rest. 21 + the 8 standing R2 boards that carry a chip (A09-create, B08, C08-new, E06, E09, E10, E13, E15) = 29, as Spec § 2 lists. No chip holds a decision code or a file path. | R3-E04's and R3-E07's *titles* (the canvas label, not the chip) still say "states.md:916" / "states.md:917". |
| S16 | Done | Spec-4 § 9 "hint (round 2's, never listed: S16)" quotes "None: a Global type belongs to no institution or speciality." and "No fields in this section yet.", each "NEW in round 2, replaced nothing". | None. |
| S17 | Done | R3-C19 title and chip: "after Hide fields on sections 2 to 6"; the board has section 1 open with 7 rows and sections 2-6 folded (0 rows each), `Hide fields` on one, `Show fields` on five. | None. |
| S18 | Done | R3-A13 (+390): 22 data rows (23 `tr`), no KGK row, every action View, "22 activity types." `role="status"`, empty search input, only "Show" (no Clear), New activity type offered, Q12's institution subtitle. Steps 1.24 cites it. | Focus on the search input is not drawn. |
| S19 | Done | R3-B07: the five seed help texts under EPA, Learner feedback campaign, Feedback window closed, Teaching contexts that responded, and Reviewer's summary, word for word from the ask; Questionnaires returned has none. Spec § 2 FIXTURES names both seeds. | None. |
| S20 | Done | Steps § Runbook hits T4 lines. | See T4. |
| S21 | Done | The count is `badge badge-declined` on all seven boards; no `.tab-problem` anywhere (0 in boards, 0 in css). | None. |
| S22 | Done | `f17r3.css` R1 (`.tab-bar-tab .badge { padding-inline: 6px }`; below 641px `.tab-bar-tab .badge > span` visually hidden, the `.visually-hidden` declarations copied) is in the file and quoted in Spec-2 § 4. The 390 boards carry the same markup as the 1280 ones, so the number-alone is the CSS rule's, not a drawn difference. | The widths (72/72 tracks etc.) are reported, not measured here. |
| S23 | Done | Spec-3 § 6 and § 7. Every figure recomputed from `app.css`'s tokens: selected underline on the page ground 4.61:1, label 10.42:1, unselected 11.99:1 (all exact). `DESIGN.md:237` ("badges, tabs") and `:1811` ("inside the surrounding .form-container") are the lines it rewrites. | None. |
| S24 | Done | `f17r3.css` C1: `.tab-bar-tab:hover { background: var(--header-bg); }`; Spec-3 § 7: text 11.36:1 (exact), muted 4.57:1 (exact). | None. |
| S25 | Done | Spec-2 § 3 app.css row and § 4 C3 row; the list sweep (below). | The selector count: see C3. |
| S26 | Done | Spec-2 § 4: four `btn-sm` about 220px (223-230px measured), `.actions-cell` wraps (`app.css:461`, opened), the 363px rail, DataTable's viewport query (`app.css:4144`, `:6278`), the Field-and-Edit-only alternative recorded; "Round 2's figure was derived, not rendered." | None. |
| S27 | Done | B07, C10-C14, C15-after, C17, C18, C19, C20, C23, E07: every row is `li.field-row` in `ul.list-unstyled` (0 `div` rows); the buttons a `div.actions-cell`; the section `h3` is the title alone with the key in `p.field-row-meta > code`; Add section and Add field stand directly in the page and the section (no wrapper). Spec-2 § 4 names the standing boards' div rows as superseded. | **New defect:** the editor in place is `<li role="group">` inside the `ul` (17 files). See Regressions. The Spec's list of superseded boards is short (For the operator 6). |
| S28 | Done | `f17r3.css` forced-colors block, quoted in Spec-2 § 4: `border-left: 4px solid Highlight; box-shadow: none; padding-left: calc(… - 4px)` for open row, section head and editor; `CanvasText` for refused. `app.css:4734-4742` is the pattern. | None. |
| S29 | Done | C19's ids: `sec-section-1`, `fe-section-1-field-3` (+ `-h`, `-key`, `-label`, `-type`, `-options`…), `fields-section-1…6`, `pv-section-n-field-m`; 0 bracket or space ids; 0 unresolved references. The brackets stay in the visible words and in names such as "Done with field ‹a 12-option choice›". | None. |

## N items

| Item | Verdict | Evidence | What remains |
|---|---|---|---|
| N1 | Done | Spec-2 § 3 GetPublishWarnings row: quotes round a type go, pointer warnings looked up in the draft's schema (the published one's for a dropped field), the draft's label wins. `BuilderModels.cs:350` is `AddPointerWarning`, `:237-239` its callers. | None. |
| N2 | Done | The shorter Active help on C01-C07, C15-before, C22, E02b, E11, E12, E16, E17 (each + twin; C08-list and A-boards have none); the dialog body "Registrars are no longer offered it when filing. Activities already filed are not changed." / "You can reactivate it on this page." on E11 and E16. The old words occur only in Spec "replaces" cells. | None. |
| N3 | Done | Spec-2 § 3 GetActivityTypeEditorQuery row; `:83`, `:87` throw, `NewActivity.razor:308` passes `ForBuilder: false`, `ActivityTypeScopeGuardTests.cs:433` and `ActivityTypeBuilderAccessTests.cs:259` carry the exception. | None. |
| N4 | Done | Spec-2 § 3 CatalogueLinks row. | `CatalogueLinks.cs:48` is blank text; the `Tokens` array is at `:55`. A stale line, carried from the ask. |
| N5 | Done | Spec-4 § 9: the three words-only results and failures, "words only, no board". | None. |
| N6 | Done | 0 `aria-describedby=""` on any R3 board (the R2 `aria-describedby=""` on Name is gone from C01-C07, E11, E12). | None. |
| N7 | Done | Spec-2 § 3 "Dates on Illustrations (N7)": the code fills the SA day of that save; boards keep ‹date› (A12, C18, C18b, E04, E05, E12, E16, E17). | None. |
| N8 | Done | Spec-2 § 3: "@onkeydown:preventDefault on the tablist, the key read in C# (chosen; no script)". | None. |
| N9 | Done | Type select, built order, chosen option selected in place, on C11 (Text), C12 (Choice), C13 (User), C17 (Scale), C23 (Text); Names a (C13): "Assessor (default)", Institutional admin, Coordinator, Committee member, Trainee; scale (C17): CNSA, CPSA v11.1 (selected), O-R. | None. |
| N10 | Done | Spec-4 § 9: "Nothing was saved. Multi-Source Feedback (Paediatrics) is written by the MSF release, so it cannot be changed here." listed with the guards, NEW, replacing "Not saved. …". | None. |
| N11 | Done | Spec-2 § 3 "Seed ownership (N11)" and § 8; `ActivityTypeSeedCatalogue.cs:92` is `SeedActorUserId = "seed-system"`. | None. |
| N12 | Done | A09-nocreate and C08-list (+ twins): "The forms registrars file and assessors complete. ‹College name› has no speciality yet, so there are no types for you to change." and nothing else changed. Spec-2 § 3 and Spec-4 § 8 carry the flag. | None. |
| N13 | Done | Spec-2 § 3 ActivityTypesList row; `ListActivityTypesAdminQuery.cs:49` orders by `Name`; `ProgrammeCalendar.cs:23` is `DateOf`. | None. |
| N14 | Done | = S5. | |
| N15 | Done | R3-D01: the textarea holds the runbook's block as typed, one state and one move a line, `cancel` actor `subject|field:topic`; it is not re-laid-out. | None. |
| N16 | Done | Spec-1 FIXTURES: "Inferred, not observed (N16): the Demo types' 'Version 1', and Mini-CEX's (and the two releases') '2026-11-05, Wombat's catalogue'." | None. |
| N17 | Done | Spec § 2 Q11 row states the rule for a College admin with no scope (`ListActivityTypesAdminQuery.cs:41-45` opened: Global, in-scope Institution, Speciality and SubSpeciality types all kept) and "a subtitle by role, never by count"; 0 brackets as control values (C19's editor key, label and options are empty). | See the C19 note under Regressions. |
| N18 | Done | Spec § 2: "key under Name in monospace". | None. |
| N19 | Done | E12's chip "Illustration: KGK Teaching Session Log deactivated"; E04 and E07 have none; Spec § 2 N19 row: R2-B08's "Q9" stands. | The two titles in S15. |
| N20 | Done | 0 tabs with `aria-label` on the 80 page boards. | None. |
| N21 | Done | Every R3 tabpanel holds a focusable control and none carries `tabindex`; a reader's Form panel (B07, View buttons) the same. No panel in the round lacks a control, so no `tabindex="0"` case arises. | The check counts a `button[aria-disabled]` as focusable; E02's Credit panel holds a textarea. |
| N22 | Done | Every `Done` has `aria-expanded="true"` and an `aria-controls` equal to the next sibling's id: C10 (`se-session`), C11, C12, C13, C14, C15-after, C17, C19, C23. | None. |
| N23 | Done | Spec-2 § 4 C2 row: "N23: it is CSS state, so switching tab and back sets it to 'Edit the form' again; intended." | None. |
| N24 | Done | Spec-3 § 7: muted on header-bg 4.57:1 (exact), beside 5.09:1 (exact) and 4.83:1 (exact). | None. |

## Q items

| Item | Verdict | Evidence | What remains |
|---|---|---|---|
| Q17 | Done | R3-E16 (+390): `dialog` "Deactivate KGK Teaching Session Log?", N2's two lines, Cancel `autofocus`, "Deactivate" `btn-danger`; Details behind it with M1, Active unticked, strip "Version 1, 2026-11-05, Nolwazi Mbatha" / "Saved ‹date› by Nolwazi Mbatha". R3-E17: no dialog, empty region, Active unticked, same strip, title "the focus back on Publish". Spec-2 § 3 ActivityTypeEdit row gives the order; Spec-4 § 9 the two results, NEW and words only. | E16 does not say on the board that the trigger and the return target are the header's Publish; Spec-4 § 10 says it for Cancel. |
| Q18 | Done | R3-C18 (+390): "Draft saved." in the region; Encounter date select "None — this form has no single encounter date", valid, no message under it; no Date delivered row or preview field; strip "Draft" / "Saved ‹date› by Nolwazi Mbatha"; Discard draft… and Publish on. R2's "delivered_on (not in the form)" and its refusal are gone from every R3 board. R3-C18b: the two warnings in the ask's order, the pinned line with Date delivered, Versions one row, "Publish version 2". Spec-4 § 10 carries the edge row; `BuilderModels.cs:86-124` is `ToJson`. | The Spec cites `ActivityTypeEdit.razor:43-66` for `EligibleFieldKeys`; that method is at `:742` (used `:209`, `:220`, `:234`). A stale range, carried from the ask. |
| Q19 | Done | Spec-2 § 3 "ActivityTypeAdminScope (Q9, Q19)" row (MayWrite refuses to everyone, a separate may-read rule, the callers, the release map, the test `SystemManagedActivityTypeTests…KeepsItSystemManaged`, which exists at `:79`) and Spec-4 § 8. `GetActivityTypeEditorQuery.cs:174-176` and `ListActivityTypesAdminQuery.cs:84` read as the Spec says. | None. |
| Q20 | Done | See M4; R3-E02b: Details selected, Key `<code>kgk_teaching_log</code> / Fixed: version 1 is published.`, Name starred with `aria-required`, "Published version 1." still in the region, Active ticked with the shorter help. Spec § 2 lists the region rule as PROPOSED. | Focus on the Details tab is not drawn. |

## T items (Steps)

Arithmetic redone from the rule. **1.27**: presses = Form 1 + Edit (section) 1 + Edit (Title) 1 + Add field 5 + Required 4 + Save draft 1 + the summary's line 1 +
Save draft 1 = **15**; fields = section key and title 2 + Title's key and label 2 + five new fields' keys and labels 10 + five type picks 5 + Audience's options 1 +
the key retyped 1 + two Form-settings picks 2 = **23**; at 390 Preview and back are two more radio picks: **15 + 25**. **Today's 1.27** from the runbook's Do
(`act-1-setup.md:858-880`): Form, Edit section (it opened Title too), Add field ×5, Required ×4, Save draft, Edit Audience, Save draft = **14**, the same 23 fields:
**14 + 23**. **1.26-1.30**: presses 2 + 15 + 3 + 1 + 3 = 24; fields 3 + 23 + 2 + 0 + 0 = 28; at 390 3 + 25 + 2 = 30; today 3 + 14 + 3 + 1 + 1 = 22 presses and
3 + 23 + 2 = 28 fields. The board's **24 + 28 / 24 + 30 / 22 + 28** are right. **1.30** = Publish, Details, the Publish tab = 3 + 0 (today 1 + 0). **A.7.9** = Menu,
Activity types, Edit = 3 + Preview 1. **6.14a** 6 + 5 at 1280 and 7 + 5 at 390 (the menu). **1.24** 3 + 1 and 4 + 1.

| Item | Verdict | Evidence | What remains |
|---|---|---|---|
| T1 (M3) | Done | Steps § Counts as above; the rule says "(the Preview choice included: it is a radio)"; 6.14a "holds (the Scope radios were fields)". | None. |
| T2 (Q20) | Done | Steps 1.30 row: Do from Credit; Expect "Published version 1." focused above the tabs, Credit selected (R3-E02), Details with the Key read-out (R3-E02b), the Publish tab (R2-E03); 3 + 0, today 1 + 0; A.7.14 cites R3-E02. | None. |
| T3 | Done | Rows 902, 904, 906, 911, 912 present (see M5). | None. |
| T4 | Done | Steps § Runbook hits: 6.14a's "Draft saved" and "v0" on `act-6-catalogue.md:483` (`:482` is the "no 'Draft saved.'" clause: both verified); 1.26's Scope `act-1-setup.md:839-841` (the Scope sentence is `:841`, so the range holds). | 6.14a's Scope is cited as `act-6-catalogue.md:459-461`; those lines are the system-managed bullet. The Scope text is `:444-447` and `:462-465`. The line range comes from the ask. |
| T5 | Done | 1.27 row: "Add field ×5 (each opens its editor, R3-C23)"; boards R2-C09, R3-C10-C15, R2-C16, R3-C23. | None. |
| T6 | Done | 1.24 row: "Clear brings back the 22 rows and '22 activity types.', the focus on the search input (R3-A13)". | None. |
| T7 | Done | States rows: 909 R3-C01; 910 R3-C03; 913 R3-D01; 915 R3-E02; 916 R3-E04; 917 R3-E07 and R2-E08. | None. |
| T8 | Done | 1.26 R3-C01-C03; 1.27 R2-C09, R3-C10-C15, R2-C16, R3-C23; 1.28 R3-D01, R2-D02; 1.30 R3-E02, E02b, R2-E03; 6.14a R3-C04-C06 (standing R2 ids kept for the rest). | None. |

## The stylesheet block

`f17r3.css` was diffed against `f17r2.css`: PART 1 (K1-K3) is untouched; PART 2 has **34 top-level rules and every one is quoted verbatim in Spec-2 § 4** (checked
by normalised text; flow 16's round 3 was marked partly for pointing to its file instead).

| Item | Verdict | Evidence | What remains |
|---|---|---|---|
| C1 | Done | `.tab-bar-tab:hover` on `--header-bg`; the rest as round 2. | None. |
| R1 | Done | As S22. Replaces S1 / `.tab-problem` (gone). | None. |
| C2 | Done | Unchanged from round 2 (diff shows no line). | None. |
| S2 | **Partly** | Done as asked: `.field-row-actions` and its `min-height` line gone; the phone rule only `.field-row .actions-cell .btn { flex: 1 1 auto; }`; the `code` selector `.outline-section-head .field-row-meta code`; the forced-colors block with the 4px edges and `padding-left` less 4px. | Two rules the ask did not name: `.outline-section-head > * { margin: 0; }` and `.field-editor-inplace > * { margin: 0; }`. The ask allows "no other NEW rule" and, for a change made to render, "say so on the Spec". The Spec says so for `.outline-section > .btn` only. |
| C3 | **Partly** | The stand-in is in the file and quoted. | The comment, Spec-2 § 3 and § 4 say the build adds `.activity-types-page` to "each of the **six** selector lists" and that the stand-in is "**nine** rules". `app.css:7547-7604` holds **seven** lists (thead, tr, tr:last-child, td/tbody th, th[scope=row], td::before, `.btn`) and the stand-in has seven rules; the Spec's own "e.g. … and likewise for tbody tr, … .btn" names seven. A build that follows "six" misses one. |

## The sweep

| Block | Verdict | Evidence | What remains |
|---|---|---|---|
| Tabs at 390 | Done (reported) | Spec-3 § 5 and check-2: C14-390, C20-390, D01-390, D04-390, C22-390 at 390, 640, 641 (track / scrollWidth, all equal, 44px) and a scratch "12" at all three widths. Plausible by arithmetic: (390 - 32) / 5 = 72, (640 - 32) / 5 = 122; at 641 the five content-width tabs total 468px plus gaps in a 609px bar. | Not measured here. |
| Outline | Done (reported) | C11 and C19 at 1280, 1116, 1115, 900, 768, 390: rail 363 / 320, columns 743 / 528 / 396 / 292, four buttons on one line (230px; 258px at 390), none clipped. Row padding leaves 288px in the 320px rail, 331px in the 363px one. | Not measured here. C19's run is 223px against C11's 230px; the board attributes it to "Done in place of Edit", which is not obviously narrower. No consequence (100px of headroom). |
| List | Done (arithmetic checked) | Min-content of the table (a clone at `width: min-content`) against the container, not the box: 275 stacked at 1000 and 1024, **682** at 1025, 1050, 1100 against 717 / 742 / 792 (container = viewport - 308 re-derived: 717, 742, 792, 692, 716 all right); spare 35 / 60 / 110. Round 2's own figure for the same table was 682 in 642 at 950px, so the 682 agrees. | Spare is **35px at 1025**. Round 2 recorded the Administrator's list at 687, i.e. 30px; the sweep draws Nolwazi's and Anton Kruger's lists only (as asked). |
| Header | Done (reported) | C03 and E16 at 1280, 1100, 1024, 1023, 390: Save draft, Publish and Discard draft… beside the h1 from 1024 (x 925/1033/1124 at 1280), under the subtitle at 1023 and 390; the h1 one line. The 1280 buttons are 36px, 44px at 390. | Not measured here. |
| 44 px at 390 | Done (count reconciled) | "952 controls, none under 44px" on 39 boards. A markup count of every `.btn`, `.form-control`, `.form-select`, `.form-check` row, tab, breadcrumb parent link and dialog button on the 39 `-390` boards gives **1052**; the preview column (`.activity-form`, 100 controls on 13 boards) is exactly the 100 difference. So 952 = the controls outside the preview. | The 44px heights are the canvas's. The preview's 100 shipped controls are not in the count (hidden behind "Edit the form" at 390 until Preview is chosen). |
| Sideways | Done (reported) | "Each of the 80 renders' scrollWidth equals its width." 80 = the page boards. | Not measured here. |

## Boards-whole duties

| Item | Verdict | Evidence | What remains |
|---|---|---|---|
| Reference files first | Done | R3-0-check § 1 and Spec § 1 say first which reference files arrived (145 R2 files and `f17r2.css` read whole, the 30 captures, the 9 seeds, which captures were viewed and for which boards, nothing drawn from memory). | The claim that each capture opened is the canvas's; the images were not reopened here. |
| CHECK BEFORE YOU STOP | Done | Every clause re-derived above: heads, h1, inline style, undefined `.wf-*`/`.tab-problem`/`.field-row-actions` (none), M1, aria-labels, one `action-result` per board (36 writer boards each exactly one, `tabindex="-1"`, Alert inside; none outside), no empty describedby, no bracket value, ids, outline list, btn-primary confirm, no refusal under a deleted setting's field, E02 on Credit, Illustration rule, twins identical, `f17r3.css` a project file, the mark. | Undefined-class check not run (`bundle.css` is not in `round-3/`). |
| Counts | Done | Check-2: 66 redrawn screens, 14 new, 7 doc boards, 35 standing boards / 75 screens. Re-counted: 66 = 32 + 32 + 2; 14; 7; the standing list names 35 R2 boards, which are 75 files with their twins. | None. |
| Spec whole | **Partly** | Spec 1-4 hold § 1-§ 11 as the ask lists, every PART 2 rule is quoted, and every NEW sentence is listed beside what it replaces. | Compared with round 2's three Spec boards, round 3 **drops**: the whole Accessibility section (headings, captioned tables, row actions named for their row, focus rules, help named by `aria-describedby`); "At 100 rows (no pager, as built)"; "Every column of the list at 1280"; "At 768: the list stacks…, nothing scrolls sideways" and the 900-1100 sweep rows. "ActivityTypeEdit: Round 2's row (…)" is a summary, not the row. The supersession note covers six standing families with div rows (C09, C16, C21, E08, E13, E14); the measured count is twelve (B01, B02, B06, B08, E09, E10 as well, 42 standing screens in all carry `aria-label` on tabs, `div` rows with `.field-row-actions`, and in 8 families `aria-describedby=""`). |
| Steps whole | **Partly** | The Steps board carries the counting rule, 11 steps, the five new states rows, the runbook hits and the recount table. | Rows 1.25, 1.29, 1.31, 6.14a, A.7.9, A.7.14 and 1.28 read "As round 2's row" (round 2's Expect text is not restated), on a canvas that does not hold round 2. |

## Regressions and new defects

| Grep or diff | Result |
|---|---|
| R2 → R3 words diff of the 66 redrawn boards | Only the ask's changes: tab labels and counts, the required line, the new Active help, A09/C08 subtitle, A12 Draft cell, B07 help texts, D04's two sentences, C18 (Q18), E02 (Credit panel), the folds' names, C19's empty bracket values, the dropped `aria-describedby=""`. No word lost or changed outside an item. |
| Select markup | A non-standard `value="…"` attribute and `selected` were added to many `<select>` elements (e.g. Details' "This tool is" on C01-C07, E11, E12, E16, E17). Harmless; unrequested (N9 asked for it on the editors' selects only). |
| **`li role="group"`** | **New.** The editor in place is `<li class="field-editor-inplace" role="group" aria-labelledby=…>` inside `ul.list-unstyled` on 17 files (C10-C14, C15-after, C17, C19, C23, with twins and C19-768). Round 2 had `div role="group"`. `group` is not an allowed role for `li` (ARIA in HTML), and the `ul` then holds a child that is not a list item, so a screen reader announces one item fewer than the rows plus editors. Fix: drop `role` and `aria-labelledby` from the `li` (the editor keeps its `h4`), or keep a `div` editor outside the `ul` after its row. |
| C19's open editor | Field key, Label and Options are empty required controls with no refusal. An empty required field is a transient typing state, so the board is not impossible, but it reads as an unfilled form; the title says nothing of it. N17 asked for empty controls here. |
| Accessible names | 0 names that omit their visible words; the 350 "Move … up/down" names contain but do not start with them (as round 2). |
| Standing R2 boards | 42 standing screens still carry round 2 patterns the Spec does not list as superseded (tab `aria-label`, panel `tabindex`, `div` rows, `aria-describedby=""`). Not redrawn by the ask. |

## Where the canvas's check board claims more than the markup shows

1. "Done" for S2 and C3 (they are partly; above) and for every Spec/Steps duty.
2. "Six selector lists" and "nine rules" (C3).
3. "952 controls measured" is right only if the preview's 100 controls are out; the board does not say so.
4. "No `.tab-problem`, `.field-row-actions`" is true of the 80 R3 boards, not of the 42 standing screens they sit beside.
5. The Spec's supersession note names six families of the twelve it should.

## The PROPOSED (THIS ASK) settlements

Each is drawn on its boards and listed on the Spec under PROPOSED (Spec § 2) or in its NEW table (§ 2 and § 9). What each would cost to change:

| Settlement | Drawn | On the Spec | Cost to change |
|---|---|---|---|
| M2(b) C19's folds: a fixed-name-free pair ("Hide/Show fields of section ‹title›"), no `aria-expanded` | C19, C19-390, C19-768, six buttons each | § 2 PROPOSED; § 9 names | To the alternative (a fixed name such as "Fields of section ‹title›" with `aria-expanded`) the visible words would have to change with it, or the name stops containing them (WCAG 2.5.3). 3 boards, 18 buttons, 2 Spec lines; the build is one button either way. |
| S22 the tab's problem count as a number alone at 390 | R1 in `f17r3.css`, the 7 tab boards' 390 twins (CSS, not markup) | § 2 PROPOSED; § 4; § 9 | To "1 problem" at 390 the five 72px tracks cannot hold "Form 1 problem"; it needs a two-line tab, a scrolling bar or a shorter word. Drop R1's second rule, re-sweep 5 boards at 3 widths, re-draw 7 twins. Keeping it costs nothing. |
| S7 Publish's dialog confirm on `btn-primary` | E05 (+390) | § 2 PROPOSED; § 6; § 7 (4.86:1) | A success confirm needs a new `ConfirmDialog` kind (component, a test, the DESIGN.md button text) and 2 boards; today's `ConfirmDialog.razor:46`/`:78` renders `btn-primary` with no change. |
| N2 Active's help shortened | 14 Details boards + twins, E11 and E16 dialogs | § 2 PROPOSED; § 9 | Text only on 28 files and 2 Spec cells. The longer words say something false for a Global type (found by anyone), so reverting needs the Global case reworded. |
| N12 a College with no speciality reads its own subtitle | A09-nocreate, C08-list (+ twins) | § 2 PROPOSED; § 3 and § 8 (the discipline flag, DATA) | Reverting is 4 files and one Spec row, and drops the new `HasDiscipline` read from `ListActivityTypesAdminQuery`; the replaced sentence is wrong for a College with no speciality. |
| E02b the result kept in its region when a tab is pressed | E02b (+390) | § 2 PROPOSED | The default behaviour (a region keeps what was last set); clearing on a tab press is extra code in `ActivityTypeEdit` and an empty region on E02b. 2 boards. |
| Words "Enter a key." and "A key is at most 100 characters." | Words only; Spec § 3, § 9, § 10 | NEW table | One validator message each in `SaveActivityTypeDraftCommand`'s new validator; no board. |

Also PROPOSED and unchanged by this check: S17 (C19 after Hide fields on sections 2 to 6, no cost), N7 (‹date› stays), Q17's two results (words only), S9's `ActorRuleWords`/`RefusalPlace` names.

## For the operator

1. **Fix the editor `li` before the build reads it.** Drop `role="group"` and `aria-labelledby` from `li.field-editor-inplace` on the 17 files (or move the editor out of the `ul`); round 2's `div` was fine, the list change made it wrong.
2. **Correct C3:** seven selector lists in `app.css` (not six) and a seven-rule stand-in (not nine), in the css comment, Spec-2 § 3 and § 4. A build told "six" will miss one.
3. **Say or drop two margin rules** in the S2 block (`.outline-section-head > *`, `.field-editor-inplace > *`); the ask allowed no other NEW rule.
4. **Restate what the Spec and Steps boards only point at:** round 2's Accessibility rows, the 100-rows / 768 / column-width rows, and the seven Steps rows that say "As round 2's row"; or accept that round 2's canvas is the reference. Text only.
5. **The supersession list is short:** the Spec names six standing families with div rows; twelve carry them (add B01, B02, B06, B08, E09, E10), and 42 standing screens keep tab `aria-label`s (harmless names), panel `tabindex` and, on eight families, `aria-describedby=""`. Say these are superseded too, or redraw.
6. **Stale citations carried from the ask:** `CatalogueLinks.cs:48` (the `Tokens` array is `:55`); `ActivityTypeEdit.razor:43-66` for `EligibleFieldKeys` (it is `:742`); `act-6-catalogue.md:459-461` for 6.14a's Scope (`:444-447`, `:462-465`). Of 24 line citations opened, 21 hold as cited and these three do not.
7. **Nothing pixel-level was measured here.** The tabs, the outline, the header, 44px and sideways rest on the canvas's render. The build's states pass should measure them, and the list at 1025px in particular: 35px spare for Nolwazi's list, about 30px for the Administrator's.
8. **Seven PROPOSED settlements** (table above): all are cheap to keep and cheap to change except S7 (a component change) and M2(b) (changes the visible words). They need a yes or a change from you before the build.
9. **Small:** R3-0-spec-2 loses "<RefusalPlace>" to unescaped HTML; E04's and E07's titles still carry `states.md` lines (their chips are gone); C19's open editor is empty with required controls; unrequested `value`/`selected` attributes sit on many selects; the preview's 100 controls are outside the 952; the 350 "Move … up/down" names contain but do not start with "Up"/"Down".
