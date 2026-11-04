# Flow 16 round 2: the runbook, cast and words review

Reviewed 2026-11-04 from the boards' text and markup in `round-2/` (306 files: R2-0-spec, R2-0-spec-2, R2-0-steps, 141 page
boards at 1280, 147 twins at 390 and 15 at 768; the 147 include the six 390-only boards R2-F20a to f), against `round-2-ask.txt`
(DECISIONS, CORRECTIONS, FIXTURES, STATES TO DRAW, STEPS BOARD, CHECK BEFORE YOU STOP), the brief
`design/flows/16-college-catalogue.md`, `round-1-assessment.md`, `design/BRIEF.md` § 6-7 and § 11, and the runbook
(`execution/knowledge/scenario-paediatrics/`: act-1, act-6, the appendix's A.7.10, `coverage.md`, `states.md`). Severity: must,
should, nit. D = 2026-11-02. Every board's text, `aria-label`, `title`, `alt`, `value`, `placeholder` and `href` was extracted and the
twins diffed against their 1280 board; the menus, the lit item, the checked and selected states, the chips and the label notes were
parsed from the markup. Boards are written R2-xnn; findings are written C1 to C16 ("finding Cn" where the two could be confused).

## What holds

- **Twins.** All 162 twins (147 at 390, 15 at 768) carry the same text and attribute values as their 1280 board, the same
  checked, selected, `aria-invalid`, `aria-describedby`, `role` and `id` set, the same label notes and the same Illustration chip.
  The 15 at 768 are the ones the ask marks (A02, A07, A22, B01, B02, B16, C01, C06, C21a, C21b, E01, E04, F01, F04, F05).
- **Menus and the lit item.** Anton Kruger's menu is Home, Specialities, EPAs, Curricula, Activity types, Entrustment scales, My data
  rights, with "Acting as College admin", on all 183 of his boards and twins (M1, D57); Demo Administrator's is the 120-board
  Platform / Organisations / People / Catalogue / Reviews menu with Entrustment scales under Catalogue. The lit item is right on every
  1280 board: Home on A01, Specialities on Kruger's A boards, Colleges on the Administrator's tree (A10-A14b) and on B, Entrustment
  scales on C, Curricula on D, F and D18, EPAs on E.
- **Cast and values.** Anton Kruger (College admin, CPSA), Demo Administrator (devadmin never appears), the three Colleges with their
  short codes (CNSA, CPSA, DEMO-C), their descriptions (CPSA's without and with "and its versions" at 6.2) and Created 2026-11-02;
  the tree (Paediatrics with Neonatology and Paediatrics, Neurology with Adult Neurology) with its descriptions before and after 6.11;
  the scales (CPSA 6, O-R 5, CNSA 5 then 6, in name order) and the CPSA ladder's six labels and descriptors, word for word; the
  fifteen EPA titles, PAED-006 "(LTHCs)" until 6.14 and "in children" after (E01, F05, E05 before; E02, F07 on), PAED-012 Inactive on
  E02 and F07 only, PAED-016 from E03, KGK-001 on E04 and F18 only; the three curricula in the ask's order (F03, F04: Neonatology
  1.0, 11.2, 11.1), their dates (11.1 2026-01-01, 11.2 and 1.0 2026-11-02, clone defaults 2026-01-02 and 2026-11-03), item counts
  (0, 15, 15, 16) and Held back / Open at each step; F05's fifteen item rows equal the ask's per-row targets, decisions, minima,
  years and tool lists, cell by cell.
- **The reach.** "Adopted by 1 institution, Kgosi Kgari Teaching Hospital; 3 registrars in training on it." on 11.1 from 6.17 to
  6.29 (F07, F08, D06, D07, D09, D12-D14), "No institution has adopted this version yet." on 11.1 at 1.18 (F05), on 1.0, and on 11.2
  from 6.29 to 6.31 (D08, D10, D11, D18, F09, F11, F13), "1 registrar" on 11.2 at A.7.10 (F20c-f); PAED-006 and PAED-012 "on 1
  curriculum adopted by 1 institution ... 3 registrars" (E05-EV2, E06-EV2, E07, E09-EV2); PAED-016 "on no curriculum" (E11-EV2).
  No board names a registrar or draws an ended profile.
- **Words (N8, M3, M5, A8).** A search of every board's text, labels, `aria-label`, `title` and `alt`, and of the three Spec boards,
  finds no he, she, his, her, him, Dr, Mr, Mrs, Ms or Prof; no "Curriculum item N", no rank, no "level 6" (the one "level 3" is the
  CPSA ladder's own descriptor); no id on any Unavailable page (A18a-c, B13, C17, D15, E15, F16, F17: "‹Record› unavailable", the
  sentence, one primary Go to); no task, question or correction code, class or component name in any product string (the codes live
  in the label notes and the Spec only). Refusals name things by label: R2-C11 and R2-C13 by EPA code, curriculum name and version
  and the rung "5"; the tree's actions by kind and name (M8).
- **Built and decided words kept verbatim:** the items' standing notice, the paused notice, the nothing-to-add line, the Remove
  dialog (F15, "Remove item"), the pause dialog's built body under Q2's first paragraph (E07), 6.8's sentence (C08), the CPSA Delete
  refusal (C10), the rename warning with its brackets (C16), the reader's scales subtitle (C21), the scales list's built subtitle
  and caption (C01, C06), Q11's dialog with its marked second sentence (C07), Q15's dialogs (A15, A17), D56's College dialog (B09,
  B10), T301's help (D05 on), T7's two results (D11, D13), Q5's two results (D05, D08), Q14's words (F06, F19), Q10's refusal (F14).
- **Brackets.** Every ‹…› is where the ask gives no value: B05's code, B16's near-twin Colleges, A22's names and 2,000-character
  descriptions, the CNSA rungs' one-line descriptions (C03b-C05, C12), C16's rename, C20's eight rungs, EPA descriptions and required
  knowledge (E boards), F18's KGK-001 cells (finding C14). Where the ask gives a value it is drawn: the Demo College, the CPSA ladder
  and O-R Scale descriptions and descriptors, the nine items that need rung 5, the Created dates.
- **Illustrations.** Forty-one boards carry the chip and "(Illustration)" in their title, and the Spec's list names the same
  forty-one (A09, A15, A16a/b, A17, A21, A22, B05, B09, B10, B11a/b, B12, B16, B17a/b, C09-C16, C20, D03, D04, D09a/b, D12-D14, D18,
  E14, E19, E20, F14, F15, F18). Four of them the ask does not mark (finding C8). Loading, load-error, required and Unavailable
  states carry none, as the Spec says.
- **Steps board.** All 33 steps are rows (1.2-1.4, 1.12-1.18, 6.1-6.8, 6.10-6.14, 6.17-6.19, 6.23, 6.24, 6.28-6.31, A.7.10), with
  1.8, Phase 6.B's intro, 6.9, 6.32-6.39, A.5.2, A.7.9 and states.md besides; the counting rule is said at the top; 18 + 15 = 33
  recounts; § 2's runbook search repeats the ask's list and every file:line pair it gives exists in the runbook as quoted; 1.2's
  two rows, 1.3's Do (Cancel for Back to colleges), 1.13's redirect, 1.15's new Do, 6.4's Do, 6.12's Do, 6.29's route and 6.31's
  "Open for adoption" are the ask's. Journey counts recount by the rule for 1.12-1.14 (1+0 / 2+0), 6.12 (3+3 / 4+3), 6.3-6.4
  (6+4 / 7+4), 6.7 (4+1 / 5+1; 10+5 in all), 6.1 (4+4 / 5+4), 6.5 (7+13 / 8+13), 6.8 (3+0 / 4+0), 6.13, 6.14, 6.17, 6.28, 6.29, 6.31
  (exceptions: finding C4).
- **Spec.** Says first that the captures arrived from the folder, which nine it viewed, which fourteen it staged and which 47 it did
  not (9 + 14 + 47 = 70); carries the decision table, M/S/N/T, the Illustration list, EV2's recommendation, the code changes, the
  class table (S1 NEW, S2 measured and not written), 44 px and 768 behaviour, § 8's DESIGN.md text, § 9's messages by kind with their
  source, the tokens and the edge cases, each with its board. It reports two faults it could not fix inside the ask's two-rule
  limit (Colleges overflows by 25 px at 950; header actions wrap at 1280), which finding C6 weighs.
- **Classes and CSS.** No `.record-page`, no `.wf-*`, no `.rung-edit*`, no inline `style`, no raw hex; every board links tokens.css,
  bundle.css, f16r2.css in that order; StackWide is on B01, A02, C01, C02, E01, F01 and F05.
- **Dialogs.** Every dialog names its target and carries Cancel then the confirm (A15, A17, B09, B10, C07, D12, D14, E07, F15); the
  only count in a deactivation question is Q2's, in the EPA's (E07), none in the College, speciality or sub-speciality ones (Q15).

## Checklist (119 numbered states, R2-A01 to R2-F20)

Done 114, partly 5, missing 0. Every numbered state is a board: A22, B17, C21, D19, E20 and F20 make 22 + 17 + 21 + 19 + 20 + 20 =
119 states, drawn as 141 boards at 1280 with their variants (A13, A14, A16, A18, A19, A20, B11, B14, B15, B17, C03, C15, C18, C19,
C21, D09, E05, E06, E09, E11, E18) plus the six 390-only F20 boards, every one with its 390 twin. Partly: **A06, A08, A15 and A16**
(the Default entrustment scale select leaves out the CNSA scale that exists from 6.5, finding C1) and **F18** (a count and a state at
odds, finding C2).

| State | Board | | State | Board | | State | Board |
|---|---|---|---|---|---|---|---|
| A01 | R2-A01 done | A02 | R2-A02 done | A03 | R2-A03 done |
| A04 | R2-A04 done | A05 | R2-A05 done | A06 | R2-A06 partly |
| A07 | R2-A07 done | A08 | R2-A08 partly | A09 | R2-A09 done |
| A10 | R2-A10 done | A11 | R2-A11 done | A12 | R2-A12 done |
| A13 | R2-A13a, R2-A13b done | A14 | R2-A14a, R2-A14b done | A15 | R2-A15 partly |
| A16 | R2-A16a, R2-A16b partly | A17 | R2-A17 done | A18 | R2-A18a, R2-A18b, R2-A18c done |
| A19 | R2-A19a, R2-A19b done | A20 | R2-A20a, R2-A20b done | A21 | R2-A21 done |
| A22 | R2-A22 done | B01 | R2-B01 done | B02 | R2-B02 done |
| B03 | R2-B03 done | B04 | R2-B04 done | B05 | R2-B05 done |
| B06 | R2-B06 done | B07 | R2-B07 done | B08 | R2-B08 done |
| B09 | R2-B09 done | B10 | R2-B10 done | B11 | R2-B11a, R2-B11b done |
| B12 | R2-B12 done | B13 | R2-B13 done | B14 | R2-B14a, R2-B14b done |
| B15 | R2-B15a, R2-B15b done | B16 | R2-B16 done | B17 | R2-B17a, R2-B17b done |
| C01 | R2-C01 done | C02 | R2-C02 done | C03 | R2-C03a, R2-C03b done |
| C04 | R2-C04 done | C05 | R2-C05 done | C06 | R2-C06 done |
| C07 | R2-C07 done | C08 | R2-C08 done | C09 | R2-C09 done |
| C10 | R2-C10 done | C11 | R2-C11 done | C12 | R2-C12 done |
| C13 | R2-C13 done | C14 | R2-C14 done | C15 | R2-C15a, R2-C15b, R2-C15c done |
| C16 | R2-C16 done | C17 | R2-C17 done | C18 | R2-C18a, R2-C18b done |
| C19 | R2-C19a, R2-C19b done | C20 | R2-C20 done | C21 | R2-C21a, R2-C21b done |
| D01 | R2-D01 done | D02 | R2-D02 done | D03 | R2-D03 done |
| D04 | R2-D04 done | D05 | R2-D05 done | D06 | R2-D06 done |
| D07 | R2-D07 done | D08 | R2-D08 done | D09 | R2-D09a, R2-D09b done |
| D10 | R2-D10 done | D11 | R2-D11 done | D12 | R2-D12 done |
| D13 | R2-D13 done | D14 | R2-D14 done | D15 | R2-D15 done |
| D16 | R2-D16 done | D17 | R2-D17 done | D18 | R2-D18 done |
| D19 | R2-D19 done | E01 | R2-E01 done | E02 | R2-E02 done |
| E03 | R2-E03 done | E04 | R2-E04 done | E05 | R2-E05-EV1, -EV2 done |
| E06 | R2-E06-EV1, -EV2 done | E07 | R2-E07 done | E08 | R2-E08 done |
| E09 | R2-E09-EV1, -EV2 done | E10 | R2-E10 done | E11 | R2-E11-EV1, -EV2 done |
| E12 | R2-E12 done | E13 | R2-E13 done | E14 | R2-E14 done |
| E15 | R2-E15 done | E16 | R2-E16 done | E17 | R2-E17 done |
| E18 | R2-E18a, R2-E18b done | E19 | R2-E19 done | E20 | R2-E20 done |
| F01 | R2-F01 done | F02 | R2-F02 done | F03 | R2-F03 done |
| F04 | R2-F04 done | F05 | R2-F05 done | F06 | R2-F06 done |
| F07 | R2-F07 done | F08 | R2-F08 done | F09 | R2-F09 done |
| F10 | R2-F10 done | F11 | R2-F11 done | F12 | R2-F12 done |
| F13 | R2-F13 done | F14 | R2-F14 done | F15 | R2-F15 done |
| F16 | R2-F16 done | F17 | R2-F17 done | F18 | R2-F18 partly |
| F19 | R2-F19 done | F20 | R2-F20a to f (390) done | | |

## Corrections

M1 to N10 (29 items): **done 27, partly 2 (S3, N10), not 0.** N7 is moot with B2.

| # | Verdict | Board(s) and note |
|---|---|---|
| M1 | done | Entrustment scales in Kruger's menu on all 183 boards; R2-C21a/b read-only; A05, A06 carry the built help alone; A01 has one paragraph; C01's subtitle loses the S2 sentence; no board for a scale's page |
| M2 | done | A01: Q12's sentence, four links (Specialities, EPAs, Curricula, Activity types) |
| M3 | done | A18a-c, B13, C17, D15, E15, F16, F17: "‹Record› unavailable", the one sentence, one primary Go to, owner lit, no id |
| M4 | done | C07: title naming the scale, "It is removed from every list. This cannot be undone.", the marked second sentence, Cancel, "Delete scale" |
| M5 | done | C11: three sentences and "And 6 more." (capital: finding C9) |
| M6 | done | no Reactivate button on any board (searched) |
| M7 | done | every changed Expect is a row; 6.7 its own; 1.15 changed (gaps in the rows: finding C5) |
| M8 | done | row and section actions named by kind in `aria-label`; dialogs "Deactivate the speciality Paediatrics?" / "the sub-speciality Neonatology?"; subtitles say the kind |
| S1 | done | C08: 6.8's sentence word for word, "Open Adult Neurology" named for the sub-speciality |
| S2 | done | C01, C06: the built subtitle, "Entrustment scales, by name", rows by name |
| S3 | partly | C11-C13 drawn and worded; the Delete refusals beyond 6.8's and the pinned one are listed by kind on Spec § 9 with no words (finding C7) |
| S4 | done | C02-C05, C20: each rung's Description a textarea (seven in C02) |
| S5 | done | no count in A15, A17, B09, B10 |
| S6 | done | Spec § 5: StackWide alone, the textarea 258 px of a 290 px block; S2 not written (measurement not re-run here) |
| S7 | done | no bracket where the ask gives the value (finding C14 is the one near miss) |
| S8 | done | required, failed save, loading, load error, Unavailable on every page; C21; A16b; A22, B16, B17, C20; A21, E19; C09; C16; 768 on 15; the sweep on Spec § 5 |
| S9 | done | the tree's subtitle on A02, A04, A07, A10, A12, A13b, A14b, A16b, A20-A22 |
| S10 | done | B04, B05 name the holder |
| S11 | done | h1 and crumb are the record's name on B06-B12, C02-C05, D05-D14, E05-E11; Create pages keep their verbs |
| N1 | done | no `.record-page` on any board |
| N2 | done | B01, B02: Created 2026-11-02 |
| N3 | done | "Create college" in actions, "College" as a proper noun in running text; said on the Spec |
| N4 | done | "Create sub-speciality" and "Create speciality" everywhere; no "Add a sub-speciality" |
| N5 | done | StackWide on Colleges and the tree |
| N6 | done | "Sub-specialities of Paediatrics" |
| N7 | done | moot |
| N8 | done | no gendered word on any board, note or Spec board |
| N9 | done | "Create scale" on C03a, C03b, C14, C15a-c |
| N10 | partly | the rule is said once and most rows recount; 6.2 at 390, 6.6's note and 6.30 do not (finding C4) |

## Findings

| # | Sev | Board | Finding |
|---|---|---|---|
| C1 | must | R2-A06, A08, A15, A16a (and their 390 twins); R2-0-steps 6.12 | **The Default entrustment scale select leaves out the CNSA scale.** All four boards are Kruger's pages at or after 6.12 and list "No default — offer every scale", "CPSA Paediatric Entrustment Scale v11.1", "O-R Scale". The CNSA Neurology Entrustment Scale has existed since 6.5, and the runbook's 6.7 Actual lists "No default, the CNSA scale, the CPSA ladder and the O-R Scale"; FIXTURES say "from 6.5 CNSA Neurology Entrustment Scale too". R2-A14a draws three, rightly; R2-A05 (1.14) and R2-A13a (6.4) are right with two. Add the CNSA scale after "No default" (name order) on the four boards, and say on Steps 6.12 that the select now offers three (its Expect says "reopened, selected" only). |
| C2 | must | R2-F18 (and -390) | **A count and a state at odds with the board's label.** (a) The heading reads "Items (15)" over sixteen rows (fifteen national and KGK-001); the runbook reads "Items (16)" for the same rows after 6.26 (act-6 6.26 Actual), and the Administrator sees every row. (b) The label says "after 6.26", yet "Add item" stands beside the heading: PAED-016 is created at 6.28, so from 6.26 to 6.27 every national EPA is on 11.1 and the board should carry the nothing-to-add line (as R2-F07 does at 6.17); "Add item" is right only from 6.28 (R2-F08 at 6.29). The ask's F18 asks for neither. The chip does not excuse it: the state is reachable. Draw "Items (16)" and either relabel "after 6.28" (PAED-016 on no curriculum) or replace "Add item" with the line. |
| C3 | should | R2-C11, C13 (and the Spec's label of C11) | **A refusal drawn on a page that the same board set says cannot offer it.** C11 and C13 draw Up, Down and Remove on the CPSA ladder, C11 explaining it as "a page read before the scale came into use". The ladder has been pinned by every 11.1 item since the seeder, so there was no such moment, and R2-C02 (and Step 1.4) draw the same ladder with no Remove. Under Q16 the handler refuses any removal on a scale in use, and a scale an item or an ended record is pinned to is in use by the delete refusal's own order, so Q6's refusal and S3's ended-record refusal can never reach the page; `states.md:845` reached it only while removal was still allowed. The words are the ask's and the boards followed it, so this is the operator's: keep Q6 and S3 as the second-line guard and say so on the boards (a stale page, as R2-C12 does), or narrow Q16. |
| C4 | should | R2-0-steps § 3 (6.2, 6.6, 6.30) | **Three counts that the stated rule does not give.** 6.2 "Colleges; Edit; Description; Save; Colleges" presses the menu link twice, so by the rule (+1 at 390 for each opening, as flow 15 counted EPAs and Curricula) 390 is 6+1, not 5+1; 6.6's "(+1 to read the list)" would be +2 at 390. 6.30 says "4+6" for the add: the Do's values are the target, the period, the cadence, the minimum, four year minima and three tools (eleven; the panel and the suggested scale arrive chosen), not six, and "today 3+6 and 4+1" cannot be rebuilt (today's add also needs Curricula, Items, Add item, Add item). List the fields of each journey. |
| C5 | should | R2-0-steps § 1, § 2 | **Gaps in the rows.** (a) The ask says to list states.md's changed rows ("list them"); the board gives one sentence. They are `states.md:719-756` (Colleges, tree, sub-specialities: names, Unavailable for the not-found and other-College rows, dialogs for the deactivated rows), `:801-807` (curriculum) and `:834-847` (scales): about fifty rows. (b) 6.5's row says "(today ...)" but never quotes the new sentence the ask gives. (c) The "Level labels must be unique" hit names 6.5 with no lines (`act-6-catalogue.md:144, :149`). (d) The Boards column omits R2-C21 on 6.10 (the menu now offers the list) and R2-B03 on 6.1 (the required message). |
| C6 | should | R2-E01, R2-B01, R2-F01 labels; R2-0-steps 1.16; R2-0-spec § 5 | **Three statements about one header.** E01's label says "Create EPA beside the h1 at 1280 (checked against the long subtitle)"; Spec § 5 measures that Create EPA wraps under the subtitle at 1280 on Colleges, the College's EPA list and Curricula (and at 1100 on the tree); Steps 1.16 says both ("beside the h1 (it wraps under the subtitle at 1280 as measured)"). The ask requires the actions beside the title at 1280 and says to report what each does; the Spec reports, but the labels still assert the opposite. Make the labels say what the Spec measured, and put the fix (the title block `flex: 1 1 20rem; min-width: 0`) and the Colleges overflow at 901 to about 975 px to the operator as the third and fourth shipped rule beyond the ask's two. |
| C7 | should | R2-0-spec-2 § 9 | **S3's other Delete refusals are not worded.** The ask says "each drawn or listed on the Spec"; § 9 lists them by kind with "(FIXTURES)": MSF questions, an ended record's item, scored progress, a published form's binding, entrustment decisions. Only the pinned scale (R2-C10) and 6.8's (R2-C08) have words anywhere. They are in FIXTURES; copy them into § 9. |
| C8 | should | R2-C14, R2-C15a-c; R2-D19 | **The chip is not applied by one rule.** R2-C14 (a taken scale name) and R2-C15a-c (required) carry the chip, which the ask does not mark and the Spec's own rule denies to required states; R2-D19 (Effective to before Effective from, a NEW refusal no replay step or capture reaches) carries none. Either mark by "the replay never reaches it" (C14, C15, D19 get it, `states.md` has no scale-required row) or follow the ask (none of the four has it, D19 as well). |
| C9 | nit | R2-C11 | The ask's words are "and 6 more." inside the list; the board starts a sentence, "And 6 more.". |
| C10 | nit | R2-C03a, C03b, C14, C15a-c, C16 | The Levels caption on a Create page reads "Levels of the new scale" (C03a, C15a), "Levels of CNSA Neurology Entrustment Scale" (C03b, C15b, C15c, the typed name) and "Levels of CPSA Paediatric Entrustment Scale v11.1" (C14, the taken name). One rule. |
| C11 | nit | R2-E04 | The Administrator's eighteen rows run EPA-001, PAED-001 to PAED-016, KGK-001. That is neither code order (KGK-001 before PAED-001) nor id order (KGK-001, EPA 17, before PAED-016, EPA 18); the runbook puts an institution's own EPA first on KGK's list (act-6 6.25). The built order is not in the runbook for the Administrator (inferred), so say which the board follows. |
| C12 | nit | R2-E01 to E04, R2-F20a | The EPA list's row action is named "Edit PAED-001"; the runbook's Actuals name it "Edit PAED-006 — …" and "Edit PAED-001 — Providing paediatric emergency care to children" (6.14, A.7.10). The items table's "Edit PAED-001" is as built. State which is the target. |
| C13 | nit | R2-D05 to D19, R2-F20f | The "Open for adoption" box sits in a fieldset whose visually hidden legend reads "Release", a word in neither the ask nor the lists (Open, Held back). A screen reader reads "Release" before the label. Drop the fieldset or name the legend "Open for adoption". |
| C14 | nit | R2-F18 | KGK-001's row is ‹target›, ‹decision›, ‹minimum›, ‹by year›, ‹tools›; the runbook (6.26) gives the values: 1 per academic year over "No decision cadence", 3a, "—" by year, Direct observation. The ask does not give them, so the bracket is allowed; S7's spirit says draw them. |
| C15 | nit | R2-E20 | Demo Administrator's Sub-speciality help reads "Every College's sub-specialities." and the select offers Paediatrics / Neonatology and Paediatrics / Paediatrics only. A select of every College's would carry Adult Neurology and General Internal Medicine too. |
| C16 | nit | R2-0-steps A.7.10 | T12's Do (open Edit on 11.2, read it, leave by Cancel) is not written; the row says only "Do adds 11.2's own page (T12)". |
