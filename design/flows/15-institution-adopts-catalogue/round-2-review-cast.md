# Flow 15 round 2: the runbook, cast and words review

Reviewed 2026-11-02 from the boards' text and markup in `round-2/` (168 boards: R2-0-spec, R2-0-spec-2, R2-0-steps, 75 page
boards at 1280, 75 twins at 390, 15 twins at 768), against `round-2-ask.txt` (DECISIONS, CORRECTIONS, FIXTURES, STATES TO DRAW,
STEPS BOARD), the brief `design/flows/15-institution-adopts-catalogue.md`, `design/BRIEF.md` § 6-7, and the runbook
(`execution/knowledge/scenario-paediatrics/`: act-1, act-2, act-6, the appendix, `coverage.md`, `states.md`). Severity: must,
should, nit. D = 2026-10-30. The twins were diffed on text, `aria-label`, `title`, `alt`, `value` and `href` against their 1280
board; the menus, buttons, ids and classes were parsed from the markup.

## What holds

- **Cast and values.** Nolwazi Mbatha (Institutional admin, KGK), Demo Administrator (devadmin), Anton Kruger, Sipho Ndlovu and
  Anele Dlamini are spelt as FIXTURES. Dates: every adoption and "Superseded on" is 2026-10-30; 11.1 effective 2026-01-01, 11.2 and
  Neonatology 1.0 effective 2026-10-30; Ndlovu 2026-01-15 / 2030-01-14; Dlamini 2024-01-15 / 2028-07-14 (saved at 6.34).
  "15 national · 0 your own" at 1.20, "15 national · 1 your own" from 6.26, 11.2 "16 national · 0" then "· 1 your own" after 6.33
  (A05, A06a), "3 registrars" at 6.32 and 6.33, "2" after 6.36 (A06b, A10, A13). The Administrator's counts (11.2 = 17, 11.1 = 16,
  Neonatology 0, IM Core 1) are the replay's at 6.38; Kruger's (16, 15, 0) are the College's, without KGK's own item (A12).
  KGK-001 is EPA 17 (C08), PAED-016 EPA 18; the rebuild's five figures are 6.38's (15, 14, 9, 0, 15).
- **Items tables.** B01's fifteen rows equal the ask's per-row targets, decisions, minima, years and tool lists, cell by cell.
  PAED-006 reads "(LTHCs)" until 6.14 and "(LTHCs) in children" after (B02, B03, B06, C03, C09, C15, G01). PAED-011 reads "2 per
  academic year" on 11.2 (B07 and on) and PAED-016 is the sixteenth row.
- **Menus** (parsed from the markup) equal FIXTURES on every board: Mbatha's grouped menu with "Curricula" leading Curriculum and no
  "Curriculum adoptions"; devadmin's Platform / Organisations / People / Catalogue / Reviews; Kruger's flat six; Ndlovu's with My
  progress. The lit item is right on every board (Curricula on A, B, F; EPAs on C; Trainees on D; Entrustment scales on E; Curricula
  on the Administrator's rebuild page).
- **Words (M1, N2, N6).** A search of every board's text, labels, `aria-label`, `title` and `alt` finds no he, she, his, her, him,
  Dr, Mr, Prof; no "follows" (the only hits are the Spec naming the rule and a description's "follow-up"). No id on any Unavailable
  page (B15, B16, C11; the ids that appear are in canvas labels). No "cannot be undone" on the move (D02, D03 say "cannot be moved
  back to 11.1 while 11.2 is the version your institution has adopted"); the one "cannot be undone" is the built Remove body (B13).
- **Built words kept verbatim:** the standing notice (B01, B02, B03, B06), the paused notice (B02), the empty line, "Adding is off
  until...", the Remove body with its em dash, the Deactivate and Active help (C08, C12), the move's results, the rebuild's subtitle,
  three paragraphs, dialog and five figures (F01 to F04), the refusal "A trainee can be moved only into...", the stale guards.
- **Buttons and classes.** "Rebuild progress" is `btn-primary` with no "..." on F01 to F05 (M3). Triggers are outline, confirms primary,
  Remove item and Deactivate danger. No inline `style`, no raw hex, no badge or `.panel-title` outside My progress (G01, as built);
  every class used is defined in `design/system/components/bundle.css` or `f15r2.css`; the one shipped rule is `.curriculum-item-page`
  (S1) and is on B04, B05, B09, B12. One h1 a board, every `aria-describedby`/`for` target exists, no duplicate ids.
- **Illustrations.** Sixteen boards carry the chip and the word in the label: A07, A08, A11, A13, B05, B12, B20, C06, C07, C15, C19,
  D06, D07a, D07b, E05, F05. The Spec's list and the ask's agree; no twin differs on the chip.
- **Journey counts** on the Steps board and the labels (2+0 / 3+0, 3+0 / 4+0, 7+9 / 9+9, 4+1 / 5+1, 4+0 / 5+0, 4+1 / 5+1) recount
  correctly by the stated rule (a menu link is a press, +1 at 390 for each menu opening; EPAs and Curricula are two openings).
- **Twins.** All 90 twins carry the same text and attribute values as their 1280 board; the 15 at 768 are the ones the ask marks
  (A01, A02, A05, A09, A13, B01, B06, B08, B20, C02, C09, C15, D01, D05, E02).

## Checklist (74 states, R2-A01 to R2-G01)

Done 72, partly 2, missing 0. A16 is words only and is on the Spec (§ 12). C18 (loading, own load error) is two boards and D07 two
boards; every other state is one. Partly: **A08** (brackets where FIXTURES give the value, C11 below) and **A11** (a count at odds
with the state its label gives, C1 below). Every state has its 390 twin; the marked ones have their 768 twin.

| State | Board | | State | Board | | State | Board |
|---|---|---|---|---|---|---|---|
| A01 | R2-A01 done | | B01 | R2-B01 done | | C01 | R2-C01 done |
| A02 | R2-A02 done | | B02 | R2-B02 done | | C02 | R2-C02 done |
| A03 | R2-A03 done | | B03 | R2-B03 done | | C03 | R2-C03 done |
| A04 | R2-A04 done | | B04 | R2-B04 done | | C04 | R2-C04 done |
| A05 | R2-A05 done | | B05 | R2-B05 done | | C05 | R2-C05 done |
| A06a | R2-A06a done | | B06 | R2-B06 done | | C06 | R2-C06 done |
| A06b | R2-A06b done | | B07 | R2-B07 done | | C07 | R2-C07 done |
| A07 | R2-A07 done | | B08 | R2-B08 done | | C08 | R2-C08 done |
| A08 | R2-A08 partly | | B09 | R2-B09 done | | C09 | R2-C09 done |
| A09 | R2-A09 done | | B10 | R2-B10 done | | C10 | R2-C10 done |
| A10 | R2-A10 done | | B11 | R2-B11 done | | C11 | R2-C11 done |
| A11 | R2-A11 partly | | B12 | R2-B12 done | | C12 | R2-C12 done |
| A12 | R2-A12 done | | B13 | R2-B13 done | | C13 | R2-C13 done |
| A13 | R2-A13 done | | B14 | R2-B14 done | | C14 | R2-C14 done |
| A14 | R2-A14 done | | B15 | R2-B15 done | | C15 | R2-C15 done |
| A15 | R2-A15 done | | B16 | R2-B16 done | | C16 | R2-C16 done |
| A16 | Spec § 12 done (words only) | | B17 | R2-B17 done | | C17 | R2-C17 done |
| D01 | R2-D01 done | | B18 | R2-B18 done | | C18 | R2-C18a, C18b done |
| D02 | R2-D02 done | | B19 | R2-B19 done | | C19 | R2-C19 done |
| D03 | R2-D03 done | | B20 | R2-B20 done | | E01 | R2-E01 done |
| D04 | R2-D04 done | | F01 | R2-F01 done | | E02 | R2-E02 done |
| D05 | R2-D05 done | | F02 | R2-F02 done | | E03 | R2-E03 done |
| D06 | R2-D06 done | | F03 | R2-F03 done | | E04 | R2-E04 done |
| D07 | R2-D07a, D07b done | | F04 | R2-F04 done | | E05 | R2-E05 done |
| G01 | R2-G01 done | | F05 | R2-F05 done | | | |

## Corrections

M1 to S16: **done 20, partly 2, not 0.** N1 to N7: done 7.

| # | Verdict | Board(s) and note |
|---|---|---|
| M1 | done | whole text search clean (above); D01 to D07b sentences carry the names |
| M2 | done | A05, A08 (T1: Adopt... on 11.1 as a row, its dialog supersedes 11.2); D02, D03 conditional |
| M3 | done | F01 to F05 primary, no "..."; Spec § 5 Button system |
| M4 | done | B06, B08, B17, B20, C09, C15 `.muted .text-sm` words under the title, no badge |
| M5 | partly | the eleven named steps are rows; 1.17 and the add half of 6.30 are not (C2, C3) |
| M6 | done | B09, B12 EPA a select offering KGK-001 alone, selected |
| S1 | done | no "Adoption history" on any A board |
| S2 | done | 3 on A05, A06a; 2 on A06b, A08, A10, A13; Molefe's ended profile in A06b's label |
| S3 | done | Spec § 7 |
| S4 | done | Spec § 9 (no board) |
| S5 | done | A12 has no adoption parts, no institution section; Spec row |
| S6 | done | no Owner column; College column kept (C02, C15) |
| S7 | done | "Active" / "Inactive" in words, no badge (C02, C03, C15) |
| S8 | done | C12's body keeps every built sentence; only the title and whose trainees change |
| S9 | done | C10 has no curricula section |
| S10 | done | E01, E02 Q13's subtitle, no actions column, Levels "6" |
| S11 | done | D01 to D07b: "Programme start date", "Expected completion date" with its help, "Save profile", "Save profile and move..." |
| S12 | partly | A08 brackets ‹N› and the own-items line (C11); G01 carries no bracket; D06, A13 brackets are values the ask does not give |
| S13 | done | zero `style=` on any board |
| S14 | done | no N1-N8 classes; the class table (Spec § 3) maps each to a shipped class |
| S15 | done | 768 on the marked boards; B17, C18b, B05, B12, A12, A10, A11 drawn |
| S16 | done | seven columns, Scale folded into Minimum level, window and weight one cell (B01, B08) |
| N1 | done | counting rule at the top of R2-0-steps; counts in A02, A05, A10, B06, D05, F04 |
| N2 | done | "adopted" throughout |
| N3 | done | C01 |
| N4 | done | C08, C13, C14 built results |
| N5 | done | `.state-panel-title` on A01, C01, E05 |
| N6 | done | no id on B15, B16, C11 |
| N7 | done | A09 to A11 subtitle |

## Findings

| # | Sev | Board | Finding |
|---|---|---|---|
| C1 | must | R2-A11 (and -390) | **Two states that cannot hold at once.** The label says "Demo Administrator adopting 11.2 for KGK at 6.31's state". The "All curricula" table shows 11.2 with **17** items; at 6.31 it holds 16, KGK-001 being added to 11.2 only at 6.33 (A12, "after 6.31", correctly says 16; B07 says 16 before the add). The same board's KGK section has 11.1 only, "15 national · 1 Kgosi Kgari Teaching Hospital's own", so no own item sits on 11.2. Draw 16. The ask's rule: never two states on one board. |
| C2 | must | R2-0-steps 6.30 | **The add half of 6.30 is not rewritten, and its page is not drawn.** The runbook Expect and Actual (act-6-catalogue.md:748, :755) say "The form is headed 'Add item', its EPA picker offers PAED-016 alone... After the add, the confirmation heads a note", and the Do adds PAED-016 as Dr Kruger. After the build (Q14, T4) the form is a page of its own; the board's 6.30 row changes only the edit ("Edit PAED-011 opens its page"). No board draws the College's add page (its h1, subtitle, picker with PAED-016 alone, the result), and B04's "Add your institution's own item ... Kgosi Kgari Teaching Hospital's trainees" is KGK's wording, wrong for the College. Add the add half to the row and name the page (a bracketed board, or a stated "same page, College's words are flow 16's"). |
| C3 | must | R2-0-steps (missing row) | **1.17 is not listed.** act-1-setup.md:428-430 (Expect) and :432-433 (Actual) read Dr Kruger's Curricula row "Active". T9 makes the College's Status "Open" / "Held back" (A12), so the Expect changes; M5 asks for every such step. The board lists 6.13, 6.29 and 6.31 only. Add 1.17 (and the Act 6 Outcome table below, C7). |
| C4 | should | R2-0-steps A.7.10, 6.30; R2-B09 | **A national item's edit page is drawn nowhere.** A.7.10's new Expect ("Edit PAED-001 opens the item's own page, its fields one a row") and 6.30's ("Edit PAED-011 opens its page") need a page for a College item; B09 and B12 are KGK's item only (subtitle "Your institution's own item on ..."). B17 gives the Administrator's list but not its page. State it, or draw B09 once for a national row. A.7.10's Do also never presses Edit PAED-001 (it only opens the lists), so the new Expect is not playable as written. |
| C5 | should | R2-0-steps 6.33 | **Order.** The row says the list reads "11.2 '16 national · 1 your own', 11.1 '3 registrars still on it'". 6.33's Do reads the list first and adds KGK-001 after (act-6-catalogue.md:793-801), so the first read is A05's "16 national · 0 your own"; "1 your own" is A06a, after the add. Say both, or put a re-read in the Do. |
| C6 | should | R2-0-steps 1.20 | **The Expect checks what the Do does not do.** "Typing /admin/adoptions lands on Curricula" is in the Expect, and the new Do ("Open Curricula... press Adopt...") never types it; "the menu has no Curriculum adoptions" is the only part a reader sees. Add the typed address to the Do or move the check to A.5.x. The same for "the second press ... leave the Expect": fine, said. |
| C7 | should | R2-0-steps § 3, "words that change" | **The search the board admits it did not run finds more.** Hits missed (file:line): **states.md:143** (Home, "Open ... adoptions" state), **:841** ("Curriculum adopted."), **:840-847** (the whole /admin/adoptions block: route, "Adopted twice, refused" can no longer occur, "Curriculum adoptions in the menu" at :846), **:901** and **:981** (the S4 sentences, "Adopt a curriculum under Curriculum adoptions" and "Adopt one under Curriculum adoptions", Spec § 9 re-points them), **:775 and :800** ("Page not found" becomes "EPA unavailable" / "Curriculum unavailable"), **:797** (the row-edit state goes), **:798** (the Remove dialog's title), **:765-776** (Edit page names); **coverage.md:104, :198, :323, :324** (`/admin/adoptions`); **act-6-catalogue.md:1019-1023** (the Outcome table's Status Active / Inactive, T9) and **:1032** ("11.2 Active and 11.1 Superseded"); **act-1-setup.md:285 and :302-303** (1.10's Home line and Actual, "Open Curriculum adoptions"; the board cites :286 and :291 only). The Gaps that close are not named either: F-1.19a (act-1:507), F-1.21a (:553), F-1.22a (:567), F-6.22a (act-6:563), F-6.25a and b (:631-632), F-A.7.10a (appendix:1844). |
| C8 | should | R2-0-steps 6.25 | **Quotes words the runbook does not hold.** The row's "today's words" are "Edit EPA", "Back to EPAs"; the board's own § 3 says neither has a runbook hit, and act-6-catalogue.md:627-630 (6.25 Actual) quotes neither. Quote the Actual (the page opened as /admin/epas/17 with Deactivate and Save; "Edit on all 16"). |
| C9 | should | R2-0-steps | **New states have no step.** B09 to B16, C05 to C07, C10 to C14, C16 to C18b, D06, F05 and the Administrator's A10 to A11 are not reached by any Do. The board lists each step's changed Expect but not which boards need a new step or a `states.md` row (B15 and B16 are the A.5.5 kind; C10 and C11 are typed or followed from the list; B09 to B11 need an edit of KGK-001 the replay never presses; B13 is the existing states.md:798 row). Add a short table: board, step or "new row", scratch or replay. |
| C10 | should | R2-0-spec § 1 | **The captures paragraph contradicts itself.** The heading reads "Opened and read (12 key, all present)" and the next sentences say eight were "staged and checked present, not viewed" and four viewed. The ask's rule is to say first which were opened and which not. Retitle: "Present: 12. Viewed: 4 (6.25-1, 6.32-1, 6.33-1, 6.38-4). Staged, not viewed: 8", and note that A01, A02, B01, B06, C01, C02, E01 and D05 stand on the ask's FIXTURES alone. |
| C11 | should | R2-A08 | **Brackets where the cast gives the value (S12).** The dialog reads "The ‹N› registrars on 11.2" and "‹the own-items line, DATA›". FIXTURES give 1 active profile on 11.2 after 6.36 and KGK-001 on 11.2 since 6.33. Draw "The registrar on 11.2 stays on it...". The own-items line also needs deciding, not bracketing: KGK-001 already sits on 11.1, so "add it again on 11.1" would be false; say what the dialog reads (the sentence is left out when the other version already holds the item, or the line names it). |
| C12 | nit | R2-D07a, D07b | The ask said "one board"; two are drawn (the Spec lists both as Illustrations). D07a names Sipho Ndlovu with "no completions to count against 11.2", which the cast contradicts (6.36: 1 counts); bracket the person or say so in the chip. |
| C13 | nit | R2-C19 | The 150-character title is a bracketed description (about 170 characters of prose), not a title of that length, beside KGK-001's real description and code. Bracket the code and description too, or write a 150-character title; the wrap is then measurable. |
| C14 | nit | R2-B14, B09 to B11 | Unplayed states (states.md:797-799 are scratch or Dr Kruger's 6.30 edit; Mbatha never edits or removes KGK-001 in the replay) without the Illustration chip. The ask did not list them; the Spec could say "scratch" in the label. |
| C15 | nit | R2-0-spec § 2 | The route reads `/admin/curricula/{{Id:int}}/items/new` (doubled braces) in the GetCurriculumByIdQuery row. |
| C16 | nit | R2-B17 | Brackets ‹the College's warning, as built› and ‹... the built line for the caller›; the runbook (act-1-setup.md:442-445, 1.18's Expect) holds the College's warning in words, so the line can be written. |

## Steps whose Expect changes: the Steps board against the ask

The eleven the ask names are all rows: 1.10, 1.18, 2.44, 6.13, 6.17, 6.29, 6.30, 6.31, A.5.2 and A.7.9 (one row), A.7.10. Also
listed: 1.19, 1.20, 1.21, 1.22 and 6.9, 6.14/6.23/6.28 (unchanged), 6.22, 6.25, 6.26, 6.32, 6.33, 6.34, 6.35/6.37/6.39
(unchanged), 6.36, 6.38. Playable in order: 1.19 to 1.21, 6.22, 6.25, 6.26, 6.32, 6.34, 6.36, 6.38. Not playable as written: 1.20 (C6),
6.33 (C5), A.7.10 (C4). Missing: 1.17 (C3), the add half of 6.30 (C2).

Grep of the runbook for the words the build changes, hits the board lists: Curriculum adoptions (act-1:286, :291, :500, :513-514;
act-2:1182, :1195; act-6:778; appendix:997, :1798), Back to curricula / Back to curriculum (act-1:441, :452), Curriculum items
(act-1:451), Define and maintain (act-1:566-567), Curriculum adopted. / already adopted (act-1:521-528; act-6:788), Deactivate
this EPA / uses it (act-6:407-421; the "uses it" at act-4:59 is another sentence), Create the first EPA (act-1:504-508), Adopt a
curriculum / Current adoptions (act-1:514; act-6:783, :788). "Edit EPA", "Back to EPAs", "Manage curriculum versions", "Maintain the
entrustable", "Remove this item", "The rebuild failed": no hit, confirmed. Hits the board missed: C7.

## Counts

Boards 168 (3 R2-0, 75 at 1280, 75 at 390, 15 at 768); states 74 (72 done, 2 partly, 0 missing); corrections M1 to S16 22
(20 done, 2 partly, 0 not), N1 to N7 7 (7 done); Illustrations 16. Findings 16: **3 must, 8 should, 5 nit.**
