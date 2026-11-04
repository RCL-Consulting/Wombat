# Flow 16 round 3: the check

Round 3 (canvas "Wombat 16 · Round 3", saved flat in `round-3/`: 230 boards + `f16r3.css` + `canvas.json`) was checked item by item against
`round-3-ask.txt`, 2026-11-04: M1-M5, S1-S21, N1-N21, Q18-Q21 (51 corrections) and T1-T8 (8 Steps lines) = **59 rows**. The canvas's own
check board (R3-0-check, check-2, check-3) was not trusted: each claim was re-derived from the boards' markup and text.

Method. Scripted over all 230 files (stdlib HTML parse of each board's `<article>`, shell and label strip left out, label strip read where an
item is about the label): file inventory against the ask's lists; every `required-mark` against `aria-required` and the hidden "required";
every `a`/`button` `aria-label` against its visible words (icons, `visually-hidden` and `aria-hidden` left out); `.action-result` count and
what it holds; `th[scope=row]` in the levels tables; the M2 select options; every `<legend>`; inline `style=`; `.catalogue-record`,
`.wf-*`, `.rung-edit*`; brackets `‹…›` as a control's value; gendered words in the whole text of every board, annotations included;
identifier-like words (camelCase, snake_case, `.razor`, role enums) on the screens; breadcrumb markup on all 181 trails; one `h1` a board;
the header block's children against `f16r3.css` R3's selectors; each 390 and 768 board's words against its 1280 board (identical on every
pair); every board's words and attributes against its round 2 board (a diff, to tell what changed). **Not rendered**: nothing in this check was
measured in a browser. Widths below are layout arithmetic from `f16r3.css`, `src/Wombat.Web/wwwroot/app.css` and the markup, and are marked
"not measured". The canvas's 1528-control 44 px count and its header-check lines are therefore neither confirmed nor refuted here.

## Verdict

**59 rows: done 55, partly 4, not done 0.** The four partly rows are S1 (the sweep table says nothing about headroom, and the three shipped
rules are not quoted on the Spec), S15 (several NEW subtitles and helps are named, not worded), N1 (two field refusals are named, not worded)
and T8 (two Steps rows cite R2 boards that round 3 redrew). No regression of what held in round 2. No board in the ask's lists is missing.
Round 3 is fit to build from once the operator reads "For the operator" at the end.

## Inventory

| What | Ask | Found |
|---|---|---|
| Redrawn at 1280 | 99 (A 19, B 16, C 15, D 17, E 15, F 17) | 99 |
| Their 390 twins | 99 | 99 |
| Redrawn at 768 | 11 (A02 A07 A22 B01 B02 B16 E01 E04 F01 F04 F05) | 11 |
| 390 alone | 5 (F20a, c, d, e, f) | 5 |
| New screens | 10 (C11, C13, D20 at 1280 and 390; B02-1024, B02-1025, F11-1025, E01-1023) | 10 |
| Check, Spec, Steps boards | check, check-2, check-3, spec, spec-2, steps | 6 |
| Total boards | 230 | 230 (no extra, no missing) |
| EV1 boards | none | none |
| `f16r3.css`, `canvas.json` | project file | both present (flow 15 had no `canvas.json`) |

Every R3 board's `<head>` links `ds/wombat/tokens.css`, `ds/wombat/components/bundle.css`, `f16r3.css`, in that order; no inline `style=`; no raw hex
in `f16r3.css`; no `.catalogue-record`, `.wf-*` or `.rung-edit*` on any screen (the Spec names `.catalogue-record` only to say it is gone).
The 99 + 5 redrawn and 10 new counts on the canvas's check board are right. The standing R2 boards the ask lists all exist in `round-2/`.

## M items

| Item | Verdict | Evidence | What remains |
|---|---|---|---|
| M1 rung refusals | Done | R3-C11 (+390): "Not saved." danger result with "Put rung “Supervises others” back"; the same sentence three times, the third ending "and ‹N› more." (N5); five rungs left. R3-C13 (+390): the ended-record sentence word for word. R3-C12 (+390): Q16's line, six rungs, no Up/Down/Remove, no "Put … back". Spec-2 § 6 states Q18's order. All three carry the Illustration chip and title. | None. |
| M2 CNSA in the select | Done | Options by name: A06, A08, A15, A16a (+390) = No default, CNSA, CPSA v11.1, O-R (3 scales; the chosen one as round 2). A14a: same, CNSA chosen. A05 and A13a: two scales (No default, CPSA, O-R). Steps 6.12 carries T1's sentence. | None. |
| M3 F18 | Done | R3-F18 (+390): h2 "Items (16)" over 16 rows, KGK-001 last with "1 per academic year", "No decision cadence", minimum 3a, "—" by year, Direct observation, and the owner mark; "Add item" present; "Edit this version of Paediatric EPA Curriculum (11.1)"; alert "Adopted by 1 institution, Kgosi Kgari Teaching Hospital; 3 registrars". Label names the moment (after 6.28) and which count is which. D06-D14 read 15 and the clone intro 15; D10, D11, D18, F20f-390 read 16 and 16: consistent with S9. | None. |
| M4 required fields | Done | All 138 boards that carry a starred field (the ask's M4 list at 1280 and 390, plus F20d-390 and F20f-390) hold exactly one "* marks a field you must fill in."; none on the list lacks it and none outside the list has it. Every starred label's control has `aria-required="true"` (0 unmatched across all boards); every `.required-mark` is `aria-hidden`; every starred `<label>` holds a visually hidden "required". Q19 read-outs carry no star (D06-D14, D20, F20f). | The canvas says the hidden "required" is "on each rung's Label input". It is not: the Label column header (`th`, "Label*") has the aria-hidden star and no hidden word, and each rung's hidden label is "Label of the rung in order n". The inputs do carry `aria-required`, so the name is read as required; the claim is overstated, the markup acceptable. |
| M5 names | Done | Long names on every listed board: "Items (n) of …" on D05-D14, D09a/b, D18, D19, D20, F20f; "Create sub-speciality of X" on A02, A04, A07, A12, A13b, A14b, A16b, A22; "Edit this version of …" on F05-F09, F11, F11-1025, F13, F15, F18, F20c, F20e; "Open Adult Neurology, the sub-speciality" on C08. No short form left on any of them. A06, A08 and A13a keep a plain "Create sub-speciality" button: those are form submits, not in the list. | None. |

All `aria-label` vs visible-word mismatches across the 230 boards (20 distinct) are one case: the visible "Remove…" and "Delete…" against names "Remove PAED-001" / "Delete O-R Scale"
(the ellipsis character only). It is as built and as flow 15 shipped; not a regression.

## S items

| Item | Verdict | Evidence | What remains |
|---|---|---|---|
| S1 tables 900-1100 | **Partly** | CSS: `f16r3.css` R2 stacks `.colleges-page`, `.college-tree-page`, `.curricula-page` at ≤1024 px and `.curriculum-items-page` at ≤1100 px; every table's page class matches (A02/A04/A07/A13b/A14b/A16b/A22 = `college-tree-page`; B01/B02/B11b/B16/B17a = `colleges-page`; F01-F04 = `curricula-page`; F05-F18 = `curriculum-items-page`; E01-E04 = `epas-page`, stacked ≤1024 as built). Boards: B02-1024 stacked width, B02-1025 and F11-1025 and E01-1023 present, with matching words. Arithmetic below says the fix fits. | (a) The sweep table on check-2 and check-3 is uninformative: every one of the 5 table shapes reports the same table px (e.g. 972 at 1280, 792 at 1100), "spare" is 0 on every row, and "Page overflow" 0. A `width:100%` table reads its container whatever its content needs, so these numbers cannot show headroom, and different tables cannot all measure the same. (b) The ask requires the three shipped rules to be quoted on the Spec; spec § 5 says only that they are "quoted there in full" in `f16r3.css`. |
| S2 header (Q20) | Done | See Q20. | |
| S3 one result region | Done | All 33 listed boards (+ twins) hold exactly one `div.action-result[tabindex=-1]` with the Alert inside; none holds two; no Alert sits outside a region. Round 2's second focusable div is gone (A16a, B06, C08, C09, F11 diffs). The only other `tabindex=-1` div is C15c's field `validation-message`. | None. |
| S4 one checkbox rule | Done | `f16r3.css` R1 `.user-page .form-check` below 641 px; no `.catalogue-record` on any board; every `form-check`, `select`, `textarea` and text input on every 390 board sits inside `.user-page`, so R1 reaches them. Spec § 4 gives the row and § 6 the retired copies (`:7462-7477`, `:7515-7529`, `:7242`). | The Spec does not quote the rule (see S1b). The 44 px result is not measured here. |
| S5 Order row header | Done | All 14 levels tables (C02, C03a/b, C04, C05, C11-C16, C20): every body row has `th scope="row" data-label="Order"`, no `td` Order; inputs keep "Label of the rung in order n" / "Description of <label>". | None. |
| S6 C08 link | Done | `href="/admin/specialities/3/sub-specialities/3"`; link named "Open Adult Neurology, the sub-speciality"; sentence as asked. Spec § 6 carries (SpecialityId, SubSpecialityId), "and ‹N› more.". | None. |
| S7 cap | Done | R3-B17a: the Description cell is exactly 1,000 characters in `td.col-wrap`; R3-B17b: the textarea holds exactly 1,000 (rows=3). Chip "A 1,000-character description". Spec § 11 cites A22 for 2,000 and B17 for 1,000. | A22's cell (unchanged from round 2) is 2,051 characters of repeated "‹a description of 2,000 characters›", past the 2,000 cap it illustrates. Cosmetic; say so on the Spec or shorten. |
| S8 HasNationalEpas | Done | Spec-2 § 6. | None. |
| S9 which count | Done | Spec-2 § 6 and the F18 and D18 labels; consistent counts (see M3). | None. |
| S10 E04 order | Done | R3-E04 (+390, +768): 18 rows; KGK-001 first with College of Paediatricians of South Africa and the owner mark; PAED-001..016; EPA-001 (Demo College) last; subtitle "Every College's EPAs, and every institution's own."; Edit named by full label. | None. |
| S11 Item unavailable | Done | R3-F16: h1 "Item unavailable"; "This curriculum item does not exist, or you cannot change it."; `div.state-panel-copy`, `div.form-actions` around one primary "Go to Items of Paediatric EPA Curriculum (11.1)". | None. |
| S12 no-College words | Done | Spec-2 § 6. | None. |
| S13 one-shot tokens | Done | Spec-2 § 6 (CatalogueLinks.Tokens, strip on arrival, stale id writes nothing). Labels of A04, A07, A12, A13b, A14b, B06, C04, D05, D08 name the token. | The sentences ("College created." etc.) are not quoted in the Spec; they are on the boards. |
| S14 Put … back | Done | R3-C11 draws it; Spec-2 § 6 (kept rung, re-insert, renumber, focus Label; offered after Q6, never after Q16). | None. |
| S15 NEW words | **Partly** | Spec-2 § 9 has rows for M5's names, Q18's three sentences and "Put … back", Q19's help and refusal, N10, N12, S6, S16, the tree's subtitle, the record subtitles, "CPSA · Active", the new-version subtitle, the scale subtitle, and the "Release" legend. | Named but not worded: "the College admin's EPA and Curricula subtitles and captions", "the clone intro and its none variant", "the Status help of a speciality and a sub-speciality". The ask: "a row for each … by their words". |
| S16 reach rule | Done | Spec-2 § 6 (ACTIVE TraineeProfiles, InstitutionId + CurriculumId, never AdoptionId; sums; superseded count; own EPA no reach line) and § 9 (the KGK-001 sentence, no board). | None. |
| S17 Steps counts | Done | Steps § 3: presses, fields, 1280, 390, today for each journey; 6.6 is "+2 at 390" with the list (7+2); 6.30's add lists its eleven values and today's add is rebuilt (Curricula; Items; Add item; eleven; Add item = 4+11). | 6.2: the board counts 6+1 at 390 (menu twice); the ask writes "6+2, not 5+1", which does not reduce to the same convention. Counting rule is stated once at the top; leave. |
| S18 Steps gaps | Done | T3 a-d and T5 present (see T rows). | None. |
| S19 one statement | Done | R3-B01, E01, F01 labels say "beside the h1 from 1024 px, under the subtitle below"; Steps 1.16; Spec § 5. | None. |
| S20 other delete refusals | Done | Spec-2 § 9: MSF questions, pinned (R3-C10), ended record, scored progress, published form (with its brackets), entrustment decisions, and the default-of-a-sub-speciality (R3-C08). | None. |
| S21 Illustration rule | Done | 38 boards carry both the `cv-chip` and "(Illustration)" in the title; no board has one without the other. C15a-c carry none; D19, C11, C13, D20 carry it; the list on the check board and Spec § 2 equals the files' 38. | The rule's boundary (a load error carries none) is read against A15 ("Q3 (D56)", a dialog, not a load error): consistent. |

## N items

| Item | Verdict | Evidence | What remains |
|---|---|---|---|
| N1 refusals no board draws | **Partly** | Spec-2 § 9 "Field refusals no board draws": the speciality duplicate quoted (built); NEW words for label > 200, description > 2,000, College name > 200, College description > 1,000. | The sub-speciality's `DuplicateName` and the scale's `ReservedNameRefusal` are named, with no words (the ask: the built words where they exist). |
| N2 | Done | Spec-2 § 11. | None. |
| N3 | Done | Edit named "Edit PAED-001 — Providing paediatric emergency care to children" on E01, E02, E03, E04, E01-1023, F20a-390. The items table keeps "Edit PAED-001". | None. |
| N4 | Done | Spec-2 § 6. | None. |
| N5 | Done | R3-C11: the tail joined, lower case, to the third sentence. | None. |
| N6, N7, N9, N11 | Done | Spec-2 § 6. | None. |
| N8 | Done | Spec § 3 (`.dashboard-card-footer` markup; links right). | None. |
| N10 | Done | R3-A17: "… can no longer give anyone a role for it or any of its sub-specialities. That is all it changes: …" exact. | None. |
| N12 | Done | "Levels of the new scale, lowest first" on C03a, C03b, C14, C15a-c; a stored name on C02, C04, C05, C11-C13; C16 and C20 read "Levels of ‹…›, lowest first" with the name a bracket (not given). | None. |
| N13 | Done | = S10. | |
| N14 | Done | No `<legend>` on D05-D14, D09a/b, D18, D19, D20, F20f-390 and no "Release" text; no `<fieldset>`. | None. |
| N15 | Done | = M3. | |
| N16 | Done | R3-E20 Sub-speciality select: Neurology / Adult Neurology, Paediatrics / Neonatology, Paediatrics / Paediatrics, General Medicine / General Internal Medicine; help "Every College's sub-specialities." | None. |
| N17 | Done | 181 trails across 230 boards: separator inside each crumb's `li`, `aria-current="page"` on the current crumb's span, none on `li` or `a`, no `li.breadcrumb-separator`, back arrow in the parent link. | None. |
| N18 | Done | No `‹…›` in any control's value, placeholder, textarea text or selected option, on any board (round 2's EPA description and knowledge brackets are gone; E05-E11 diff confirms). Brackets stay in cells, captions, h1 of C16 and C20, and the label strip. | None. |
| N19 | Done | R3-F16 (S11); Spec § 3 and § 6. | None. |
| N20 | Done | `aria-haspopup="dialog"` on Save in R3-B10 and R3-D14 only; B11a's Save (box unticked after the fact) has none. Spec-2 § 6. | None. |
| N21 | Done | = T5. | |

## Q items

| Item | Verdict | Evidence | What remains |
|---|---|---|---|
| Q18 | Done | See M1. | |
| Q19 | Done | D06-D14, D09a/b, D18, F20f-390: `dl.detail-list` "Sub-speciality / Paediatrics / Paediatrics" with the exact help; no select. D01-D05 and D19 (Neonatology 1.0): select live. D20: danger Alert in the region with the exact sentence; page read again: read-out "Paediatrics / Neonatology", help, "Items (‹n›)". Spec-2 § 6: `SubSpecialityFixed` PROPOSED, the refusal. | D20 also prints "No institution has adopted this version yet.", which would contradict the cause if it were an adoption; the label leaves the cause as "‹an item or an adoption›". Not a failure; the board draws the item cause. |
| Q20 | Done | `f16r3.css` R3: from 1024 px the title block `flex: 1 1 20rem; min-width: 0`, actions `flex: none`, `flex-wrap: nowrap`; below 1024 the actions `width:100%`. All 48 listed boards have `.header-container` with exactly two children, `div` then `div.actions-cell` (the selectors match); C08-C10 also have actions and follow the same rule. Every header holds one button or link (Create X, Items (n), Edit this version), so the title block keeps well over the 20rem basis at 1024. R3-E01-1023 and B02-1024/1025 show either side. | Spec § 8 and § 6 state the rule; not quoted (S1b). The canvas's header table (check-3) is not re-measured. |
| Q21 | Done | R3-E05, E06, E09, E11 are the EV2 boards without a suffix (diff against `R2-E05-EV2` etc. shows only M4 and N18 changes); no EV1 board; Spec § 2 and Steps T6. | None. |

## T items (Steps)

| Item | Verdict | Evidence | What remains |
|---|---|---|---|
| T1 | Done | 6.12 carries "the Default entrustment scale select offers three scales by name after No default (the CNSA scale, the CPSA ladder, the O-R Scale)". | None. |
| T2 | Done | Steps § 3 (see S17). | |
| T3 | Done | (a) § 2 rows for states.md :719-756, :801-807, :834-847 with new words; (b) 6.5 quotes the sentence; (c) act-6-catalogue.md:144 and :149; (d) R2-C21a on 6.10, R3-B03 on 6.1. | None. |
| T4 | Done | 1.16 says Create EPA beside the h1 at 1280 and under the subtitle below 1024. | None. |
| T5 | Done | A.7.10 row: Edit this version on 11.2's items, read the curriculum's page, leave by Cancel; Expect: one field a row, read-out, 44 px row, Save on top of Cancel, nothing sideways. | None. |
| T6 | Done | 6.14 R3-E05, E06; 6.23 R3-E09; 6.28 R3-E10, E11. | None. |
| T7 | Done | 6.29 and 6.31 add the read-out; 6.13 keeps the select. | None. |
| T8 | **Partly** | Most changed boards take their R3 id. | Row 1.14 cites "R2-A05 (stands)" and row 6.4 cites "R2-A13a"; both were redrawn as R3-A05 and R3-A13a (M4, S4). The ask's own M2 line says "R2-A05 (1.14) and R2-A13a (6.4) keep two", which may have misled; the ask's redraw list still names A05 and A13a, so the Boards column should read R3-A05 and R3-A13a. |

## Where the canvas's check board claims more than the markup shows

1. **Sweep numbers (S1).** Uniform across five different tables; "spare 0" on every row. See S1(a).
2. **Hidden "required" on each rung's Label input (M4).** Not in the markup; `aria-required` is. See M4.
3. **Captures (§ 1).** The board says "40 in all". The ids it lists count to 39 (act-1 8, act-6 19, act-A 3, states 8, plus 6.30-3). The words a capture shows
   against a board are plausible and could not be re-checked here (images not reopened).
4. **Spec § 5 "quoted there in full".** The rules are in `f16r3.css`; the Spec holds a pointer, not the quote (S1b).
5. **S15 and N1 "Done"** on the check board; both are partly (above).
6. **T8 "Done"**; two rows cite R2 boards that were redrawn.

## The table widths (arithmetic, not measured)

Container = viewport − 308 px (250 sidebar + gutters), as the ask and the canvas both use: 717 px at 1025, 793 px at 1101. App.css padding: default cell 2 × 1rem, `--compact` 2 × 0.5rem;
`.people-page .clinic-table th.col-wrap { min-width: 7rem }` (112 px) is the floor for every `col-wrap` header on these pages; `td.col-wrap` breaks anywhere. Minimum
table widths, summed from those floors and the longest unbreakable header or button label in each column:

| Table | Stacks at | First width as a table | Container there | Minimum table (arithmetic) | Headroom |
|---|---|---|---|---|---|
| Colleges (6 columns) | ≤ 1024 | 1025 | 717 | about 595 to 667 (round 2: 667) | about 50 to 120 px |
| The tree's sub-speciality tables (5 columns) | ≤ 1024 | 1025 | 717 | about 570 to 596 (round 2: 596) | about 120 px |
| Curricula (6 columns) | ≤ 1024 | 1025 | 717 | about 620 to 630 (round 2: 630) | about 90 px |
| Items (6 compact columns) | ≤ 1100 | 1101 | 793 | 691 (T392 replay, measured on the built table) to 744 (round 2's arithmetic) | about 50 to 100 px |
| EPA list (4 and 5 columns, as built) | ≤ 1024 | 1025 | 717 | about 565 to 600 | about 120 px |

**Verdict: stacks at the right edge for all four; none is expected to overflow between 900 and 1100 px, and a stacked row never scrolls.** Each stack threshold sits at or above the
widest overflow round 2 found (Colleges about 975, Curricula about 940, the tree 901, the items 1051 on round 2's number). The tightest case is the items table just above 1100 px
on the higher estimate (49 px); if the build finds it tight, stacking to 1100 and a table above it is the answer, as the data-rights queue does. **Not measured.** The sweep belongs in
the build's states pass in a browser, in 50 px steps, comparing the table's `scrollWidth` with its container's `clientWidth` (not the page's): the check this flow's own BRIEF § 11 "Flow 15" teaches.

Also not verifiable from the files: the boards are drawn at fixed widths inside one canvas, so the `@media` stacks show on the 768, 1023, 1024 and 1025 boards only if the canvas renders each board in a viewport
of its own width (as flow 15's did).

## Regressions grep (what held in round 2)

| Grep | Result |
|---|---|
| Gendered words (he, she, him, her, his, hers, man, woman, male, female, Mr, Mrs, Ms, sir) in the whole text of all 230 boards, annotations included | 0 |
| Code names where labels belong (camelCase, snake_case, `.razor`/`.cs`, role enums such as CollegeAdmin, PendingTrainee) on the screens; the label strip, Spec and Steps are annotation and left out | 0 |
| `required-mark` without `aria-required` (starred labels without a matching `aria-required` control) | 0 |
| `required-mark` not `aria-hidden` | 0 |
| Accessible names that do not contain their visible words | 20 distinct names, all "Remove…" or "Delete…" against "Remove PAED-001" and the like (the ellipsis only); same as flow 15 shipped |
| More than one `h1` on a board | 0 |
| Inline `style=`, raw hex in `f16r3.css`, `.catalogue-record`, `.wf-*`, `.rung-edit*` | 0 |
| Brackets as a control's value | 0 |
| Navigation lists | 132 boards (Kruger): Home, Specialities, EPAs, Curricula, Activity types, Entrustment scales, My data rights; 92 (Administrator): the full menu; none other |

## For the operator

1. **Sweep before build.** The canvas's sweep is not evidence of headroom. Take the arithmetic above as the case for the stack edges, and require the build's states pass to
   measure the four tables 900 to 1100 px against their containers. Decide whether the items' 1100 px edge is accepted on a 49 px margin.
2. **Four small fixes to the canvas or the Spec before the build reads them:** quote R1-R3 on the Spec (S1b); give the words for the three S15 rows and the two N1 refusals;
   change Steps 1.14 and 6.4 to R3-A05 and R3-A13a (T8); correct the check board's M4 claim about the rung Label inputs and its "40" captures. None changes a board.
3. **A22's 2,051-character cell** exceeds the 2,000 cap it illustrates; shorten it or leave it as a bracketed filler.
4. **D20** leaves the cause of the refusal open (an item or an adoption) while the page prints "No institution has adopted this version yet." If an adoption is the cause the board
   is inconsistent; the label should say item.
5. **Levels table Label header:** accept `aria-required` on the inputs without the hidden word on the header (the inputs' own hidden labels do not say "required"), or ask for it.
