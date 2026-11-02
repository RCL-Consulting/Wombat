# Flow 15 round 3: the check

Round 3 (canvas page "Round 3", saved flat in `round-3/`: 113 files = 107 screens + the five Spec/Steps boards + `f15r3.css`) was checked
item by item against `round-3-ask.txt` (M1-M6, S1-S25, N1-N9, T1-T8 = 48 items; F1-F9 are the decisions behind them and are read with the
items), 2026-11-02. The canvas's own checklist (R3-0-spec § 4) was not trusted: each claim was re-derived from the board HTML.

Method. Scripted over all 107 screens: `td.actions-cell` (0; 819 `<td>` hold exactly one `div.actions-cell`, none is itself the cluster); inline
`style=` (0); raw hex (0); every class against `design/system/components/bundle.css` plus `f15r3.css` (0 unknown); one `/_blob/` (the given mark,
107 times); gendered pronouns in the whole text, annotations included (0); one h1 a board; stylesheet order in `<head>` (tokens, bundle,
`f15r3.css`); badges (0); each 1280 board against its 390/768/1100/901 twins with label strip and ids removed (words identical on every pair);
every dialog's markup (S13); every `…` trigger for `aria-haspopup`. Then the boards were **rendered** (Chromium, staged with the shipped
tokens, `bundle.css`, `f15r3.css`, Source Sans 3): the 14 items boards at 1280, 1100 and 901 with the board width freed, all 46 boards at 390
(every `.btn`, `.form-control`, `.form-select`, `.form-check`, breadcrumb parent link, pager control, dialog button), and every board at its own
width for sideways scroll. Code facts were spot-checked in `src/` and `DESIGN.md`. Not verifiable from the files: the canvas page and title
(`round-3/` carries no `canvas.json`, and each board's `<title>` is only its id; the Illustration word is checked in the label strip and chip).

## Verdict

**48 items: done 45, partly 2, not done 1.** The Spec's checklist agrees on 45 and is wrong on N6 and S17 (and on one sentence in § 5, below).
The render-check numbers are right: 321 controls at 390, the same five under 44 px, the same nine boards scrolling 10 px at 901. Round 3 is
fit to build from once the operator decisions below are taken.

## M items

| Item | Verdict | Evidence | What remains |
|---|---|---|---|
| M1 actions cell | Done | 819 cells in 77 board files, every one `<td><div class="actions-cell">`; none `td.actions-cell`. "Set by the College." spans sit inside the same div; no cell holds two divs. | Nothing. (The standing R2 boards C02-C07 carry no `td.actions-cell` either, so the Spec § 5 sentence about them is moot.) |
| M2 A11 count | Done | A11 and A11-390: All curricula 11.2 = 16, 11.1 = 16; 6.31's state (Neonatology Held back, 0). A09 and A10 read 17 (6.33 and after), as FIXTURES. | None. |
| M3 College item pages | Done | B22 (add): trail Home › Curricula › Items of ... (11.2) › Add item; h1 "Add item"; NEW subtitle exact; EPA select offers PAED-016 only, chosen; target 1, per Academic year, cadence Each academic year, scale v11.1, min 4, years 2/3a/3b/4, window 12, weight empty, CBD/Mini-CEX/MSF ticked; labels and helps identical to R2-B04 apart from the filled-state differences; Cancel, Add item. B23: `?added` result focusable, notice, h2 "Items (16)", built national empty line, 16 rows PAED-016 last, Edit/Remove… named per EPA, no owner mark, subtitle as asked. B24: h1 "Edit PAED-011 — Managing population health challenges", same subtitle, EPA PAED-011 alone, Target 2, tools CBD/Direct observation/MSF. 390 twins carry the same words; the 390 parent link and checkbox rows measure 44 px. | One open question for the build: operator item 7 (the filled Minimum-by-year rows). |
| M4 Steps 1.17, Act 6 Outcome | Done | R3-0-steps: 1.17 "15 items, Open"; Outcome Open/Open/Held back; KGK "Adopted on 2026-10-30" / "Superseded on 2026-10-30" over "2 registrars still on it". | None. |
| M5 "EPA saved." | Done | C08: `action-result tabindex="-1"` holding "EPA saved."; label: reached by `/admin/epas/17?created`, token gone, focus on the result; Spec § 6 EpaEdit and CatalogueLinks rows list it. | None. |
| M6 item-page return | Done | B06 `?added`, B10 `?saved`, B11 `?back` (no result, focus on "Edit KGK-001"), B23 `?added`: each label names the token and that it was stripped; B06/B10/B23 carry the result with the right sentence, B11 has none. Spec § 6 lists the four tokens, CatalogueLinks and the stale-id rule. | None. |

## S items

| Item | Verdict | Evidence | What remains |
|---|---|---|---|
| S1 supersession | Done | A08 (Illustration): dialog "Adopt ... (11.1) for Kgosi Kgari Teaching Hospital?", "supersedes (11.2)", singular "The registrar on 11.2 stays on it until moved on their profile.", no own-items line, Cancel then "Adopt 11.1"; no bracket. Spec § 6 (GetCurriculaList/GetAdoptable rows) writes the (AdoptedOn, Id) and active-profile rules; § 8 lists the singular as NEW. | None. |
| S2 tie rule | Done (Spec) | § 6 GetAdoptableCurriculaQuery row: "(EffectiveFrom, Id) (F2)". | None. |
| S3 InstitutionId | Done (Spec) | § 6 rows for both queries. GetCurricula.cs:12, :28-41 and GetAdoptableCurricula.cs:26, :39 hold in `src/`. | None. |
| S4 Create EPA flag | Done (Spec) | § 6 EpaDto/EpasList row; CreateEpa.cs:66-80 holds. | None. |
| S5 F9 | Done (Spec) | § 6 CreateEpa/UpdateEpa row; UpdateEpa.cs:98-104 holds. | None. |
| S6 FieldRefusalException | Done (Spec) | § 6 row names the five throwers and EpaEdit.razor:275, CurriculumItemsEdit.razor:625 and :734, TraineeProfileEdit.razor:496-506 (all hold). | None. |
| S7 load errors | Done | A15, B19, C17, C18b, E04 (+390): `div.action-result tabindex="-1"` > `alert alert-danger` > `div.alert-row` > `span.alert-row-text` (words as R2) + `btn btn-outline btn-sm` "Try again" with an icon; no empty state; h1 `tabindex="-1"`. Spec § 6 lists the four pages' `exception.Message` lines (104, 77, 94, 590 hold). | None. |
| S8 F5 steps | Done | 6.13, 6.29, 6.31 rows match the ask; Spec § 6 CreateCurriculumCommand row (Curriculum.cs:19, :35 hold). | None. |
| S9 rebuild focus | Done (Spec) | § 6 CurriculumProgressRebuild row (razor :11-14 and :46-48 hold). | None. |
| S10 em dash | Done (Spec) | § 6 item-routes row names :111, :769, the legend, EpaOptionLabel.cs:20; built :111 and :769 are hyphens, :20 is an em dash. | None. |
| S11 Deactivate… | Done | Spec § 6 EpaEdit row (EpaEdit.razor:96 holds); Steps 6.17 Do/Expect, 6.23 Expect; 6.14 stated unchanged and the review's claim called wrong. | None. |
| S12 superseded items (B21) | Done | B21 (+390): subtitle exact (NEW); standing notice as built, no role; "Items (16)"; no Add, the `.muted` NEW line in its place; 15 "Set by the College." rows, PAED-006 "(LTHCs) in children", PAED-012 without an inactive mark, KGK-001 last "Your institution's own item" with Edit and Remove… (haspopup). Spec § 6 GetCurriculumByIdQuery row has the state and may-add flag. | None. |
| S13 dialogs | Done | All seven dialogs (A03, A08, A11, B13, C12, D03, F02, both widths): `dialog.dialog-scrim.dialog-card` with `aria-labelledby`/`aria-describedby`, `.form-container`, `h2#dlg-t`, body `p#dlg-b` (B13, C12) or `div#dlg-b.dialog-body` (A03, A08, A11, D03, F02), `.form-actions` with Cancel `autofocus` first; no `.dialog-form`. | Nit: A03/A08/A11 add `dialog-body--scroll`, which ConfirmDialog's action mode (`ConfirmDialog.razor:58`) does not render. |
| S14 pager | Done | C15 (+390/768): `nav.pager aria-label="Pages of EPAs"`, `.pager-info` "1–‹n› of 40", `.pager-actions` (Previous `aria-disabled`, Next), `.pager-page-size` with label "EPAs per page" (NEW, label and select paired); no hand-drawn "Page 1 of". | Nit: `.pager-info` is a `span` (built: `div`). |
| S15 link names | Done | A10/A11: institution-section links "Items of Paediatric EPA Curriculum (11.2) for Kgosi Kgari Teaching Hospital" (and 11.1); All curricula keeps the short name. | None. |
| S16 render-check | Done | Re-rendered. 321 controls at 390 on 46 boards, 5 under 44 px (as the Spec); no page scrolls sideways at any width; the items table container fits at 1280 and 1100; at 901 the nine boards with Edit/Remove… (B06, B08, B10, B11, B13, B17, B20, B21, B23) have a container 593 wide holding a 603-wide table (scrolls 10 px); B01, B02, B03, B07, B14 fit. The Spec reported this as asked and drew nothing to hide it. | Operator items 1 and 2. |
| S17 DESIGN.md lines | Partly | Spec § 7 lists "Editing a row in place" (:896 ff), the row-actions/picker edit-row lines, :587, :3985, :4441, the :3959 ValidationSummary (all lines hold). But it names the picker rule as "§ Form system's picker paragraph"; the paragraph is DESIGN.md:966, under `## Table system` (:872). | Correct the section name to § Table system in the Spec (the ask's "name the section" item). |
| S18 F8 scope | Done (Spec) | § 7 T211 paragraph carries "words, never a badge, on the admin lists and pages"; trainee h1 and My progress keep it until T357. No badge on any R3 board. | None. |
| S19 loading | Done | A14, B18, C16, C18a, E03 (+390): exactly one `p.loading-line role="status"` each with the NEW words; A14 says it once; skeletons kept. | None. |
| S20-S24 Steps | Done | T5-T7 below. | None. |
| S25 captures | Done | Spec § 1: arrived from the folder; present 12 + 3; viewed in round 2 (7: the 4 key plus 3 others, as R2's own Spec says), in round 3 (11, the ask's eleven); staged, not viewed; FIXTURES-only boards named (A01, A02, B01, B06, C01, C02, E01, D05). I viewed `6.26-3`: it confirms the Spec's row (KGK-001 first today). | None. |

## N items

| Item | Verdict | Evidence | What remains |
|---|---|---|---|
| N1 B17 brackets | Done | Notice and national empty line are written in full and equal `CurriculumItemsEdit.razor:44-49` and `:1140` word for word. | None. |
| N2 caption | Done | A09, A10, A11: "Every College's curriculum versions, with the College's status. Items counts each institution's own items too." | None. |
| N3 column and intro | Done | A10, A11: "For this institution"; the Administrator's intro as asked. | None. |
| N4 no role on notices | Done | All 36 `alert alert-warning` and 2 `alert-info` carry no role; success carries `status`, danger `alert`. | None. |
| N5 StatePanel empty | Done | A01 (+390/768): `div.detail-card.detail-card--empty tabindex="-1"`, `div.state-panel-title`, `div.state-panel-copy`; matches `StatePanel.razor`. | None. |
| N6 aria-haspopup | Partly | Every Adopt…, Remove…, Deactivate… trigger carries `aria-haspopup="dialog"`. D03's "Save profile and move…" (the move's gate) has none; the Spec says "every redrawn trigger". | Operator item 8. |
| N7 trail = Breadcrumbs.razor | Not done | The Spec says so. Checked against `src/Wombat.Web/Components/Shared/Breadcrumbs.razor`: the drawn trail differs in markup (operator item 3). Rendering is equivalent. | Operator item 3. |
| N8 "scratch" | Done | B10, B11, B14 labels say "scratch"; Steps § 4 has B09 and the three. | None. |
| N9 one `.user-page` rule | Done | `f15r3.css` PART 2 is one `.user-page.curriculum-item-page .form-check` rule (plus input and label); quoted on the Spec § 9; B22/B24 checkbox rows measure 44 px at 390. | None. |

## T items (Steps)

| Item | Verdict | Evidence |
|---|---|---|
| T1 | Done | 1.17 and Act 6 Outcome as M4. |
| T2 | Done | 6.30 (add page, `?added` result, 16 rows, empty line; "Edit PAED-011 opens its page", `?saved`, row "2 per academic year", 11.1's PAED-011 still 1); A.7.10 adds "press Edit PAED-001 and read its page". |
| T3 | Done | 6.13, 6.29, 6.31 as the ask. |
| T4 | Done | 6.17, 6.23; 6.14 unchanged. |
| T5 | Done | 6.33 order, 1.20 "Type /admin/adoptions", 6.25 quotes today's Actual beside the new. |
| T6 | Done | Every cited line and all seven Gaps are in `R3-0-steps-2`. |
| T7 | Done | Table lists B09-B16, B21-B24, C05-C07, C10-C14, C16-C18b, D06, F05, A10-A11 with step or new row and "replay/scratch". |
| T8 | Done | § 1 table: all journeys at 1280 and 390 with today's; the College's add (4 + 6 / 5 + 6) and edit (4 + 1 / 5 + 1); F1 changes none, F5 removes a tick and a Save from 6.13 and 6.29. Recounted from the Do lines: correct. |

## Where the canvas's self-report was wrong

1. **N6** reported Done for "every redrawn trigger"; D03's trigger has no `aria-haspopup`.
2. **S17** reported Spec-done; the picker rule's section is misnamed (§ Form system; it is § Table system, `DESIGN.md:966`).
3. **Spec § 5** says the standing boards C02-C07 carry round 2's `td.actions-cell` "where they have one": none has one.
4. Smaller: Spec § 6 cites `AdoptCurriculum.cs:47-52` for the held-back refusal; those lines load the curriculum (no such refusal exists today; it is new code after line 52). The ask's "h1 Add item (built, :329)" is an `h3` today (the ask's wording). The Spec's capture-difference table does not list that today's items table has a separate Scale column and an "Existing items" h2 (the boards fold the scale into Minimum level and use "Items (n)"; decided in earlier rounds, so only a missing line).
5. Checked and right: the render numbers (5 short controls; -10 px at 901 on exactly the nine boards), and every Done row for M1-M6, S7, S12, S13, S19.

## For the operator (decisions, not redraws)

1. **Items table at 901 px scrolls 10 px sideways** on B06, B08, B10, B11, B13, B17, B20, B21, B23 (verified by render: container 593, table 603; the five national-only boards fit). The canvas drew nothing to hide it, as asked. Recommendation: **move Window and weight to the item's page** (frees about 72 px; the item page already holds both; the year labels moved the same way). A named focusable scroll region is the fallback.
2. **Five 390 controls under 44 px**: "Add your institution's own item" (36 px, B03, B07, B14) and EPA Status's "Active" checkbox row (24 px, C08, C12). Verified. The College's "Add item" link on the items list is drawn on no board (the list boards show the empty line where it would be) and is the same `.form-section-head` action, so it will be 36 px too. Recommendation: **take one page-scoped rule each** (`.form-section-head .btn` and EpaEdit's `.form-check` row to 2.75rem below 641 px), as flow 12 did; the other 316 controls already keep the 44 px rule.
3. **N7 (Breadcrumbs.razor)**: the drawn trail does not match the built markup. Built: the separator is an icon inside the preceding `li` (drawn: its own `li.breadcrumb-separator`); `aria-current="page"` is on a `span` inside `li.breadcrumb-current` (drawn: on the `li`); every link carries the `.breadcrumb-back` chevron (drawn: only the parent, in a `span`). Flow 14's trail does match the component. Recommendation: **no redraw; build from the component** (rendering is the same: app.css hides the chevron above 640 px and every crumb but the parent below). Tell the build lane not to copy the boards' trail markup.
4. **KGK-001 moved from first to last** in the items table (6.26-3 shows it first today; round 2's rule "EPA order, own after the PAED codes" drew it last; verified on B06, B08, B10, B13, B17, B21). Recommendation: **accept last**: the table is in EPA order and the own row is the one that carries Edit and Remove…. Alternative: own first, which keeps today's habit and breaks the "rows in EPA order" caption.
5. **K3 grid fix** (`f15r3.css`: `grid-template-areas` and `.cv-main { grid-area: main }`): canvas-only (`.cv-*`), never shipped, so **no build impact**. It does mean every R2 board still on `f15r2.css` renders its page squeezed into 250 px at 641 px and up. Recommendation: **do not judge the standing R2 boards' layout at 1280/768 on the canvas**; re-link them to `f15r3.css` if they are used again. The 390 findings (phone bar, no grid) and all words and markup of round 2 stand.
6. **Plus icon dropped**: built `EpasList.razor:10-12` and `CurriculaList.razor:20-22` put `<Icon Name="plus" />` in Create EPA and Create curriculum; the boards draw neither (C09, C15 and A09-A11, with twins; the Spec names only the EPA one). Recommendation: **build as built (keep the plus)**; add both to the Spec's capture table. A canvas omission, not a design decision.
7. **Filled "Minimum by training year" rows** on B22/B24 are drawn as four labelled selects ("Year 1" to "Year 4"); no capture shows a filled row (the Spec says so). Recommendation: **check `StageMinimaEditor.razor`'s rows in the build lane and keep whatever it renders**.
8. **D03's trigger** ("Save profile and move…"): Recommendation: **give it `aria-haspopup="dialog"`** (it opens ConfirmDialog, DESIGN.md:757's pattern) and say so on the Spec. One attribute.
9. **Illustration title and `canvas.json`**: `round-3/` has no `canvas.json` and each `<title>` is the id, so the "Illustration in its title" half of the labelling rule cannot be checked from the repo (chips and label strips are right on A07, A08, A11, A13, B20, C15 and twins, and on no other board). Recommendation: **accept**; ask the canvas to save `canvas.json` next time, as flow 14 had.
10. **S17's section name**: fix in the Spec before the DESIGN.md edit is written (picker rule = § Table system, `DESIGN.md:966`).
