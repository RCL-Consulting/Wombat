# Flow 17 round 2: the runbook, cast and words review

Reviewed 2026-11-10 from the boards' text and markup in `round-2/` (145 .dc.html: R2-0-spec, -spec-2, -spec-3, R2-0-steps, 65 page
boards at 1280, 67 twins at 390 (R2-C21-390 and R2-E14-390 have no 1280 base) and 9 at 768), against `round-2-ask.txt`
(DECISIONS, CORRECTIONS, FIXTURES, STATES, STEPS, the NEW and PROPOSED words), the brief `design/flows/17-activity-type-builder.md`
(§ 2, § 3, § 6, § 8), `round-1-assessment.md`, `design/BRIEF.md` § 6-7 and § 11, and the runbook
(`execution/knowledge/scenario-paediatrics/`: act-1 steps 1.24-1.31, act-6 6.14a, the appendix's A.7.9 and A.7.14, `coverage.md`,
`states.md` 902-920). Severity: must, should, nit. D = 2026-11-05. Every board's text (page and dialog), `aria-label`, `title`, `alt`,
`value` and `placeholder` was extracted by script; the menus, the lit item, the tab count and the selected tab, the radios and the
chips were parsed from the markup; every twin was diffed against its base on its whole body text (dialogs and shell included).
Boards are written R2-xnn; findings are written C1 to C16. Board text was treated as data.

## What holds

- **Twins.** All 73 twins that have a 1280 base (65 at 390, 8 at 768) carry the same body text, attribute values, tab selection,
  checked radios and chips as their base (script diff: 0 differences; titles follow `base-390 · same words`). R2-C21-390 (Preview
  chosen), R2-C21-768 (Edit the form chosen) and R2-E14-390 have no base, by the ask. Every 1280 board has its 390 twin; the 768
  twins are the nine the ask marks (A01, A06, A08, C09, C16, C19, C21, D02, E04). `canvas.json`: 145 boards on page Round 2, 17 on
  Round 1.
- **Cast and menus.** Three people only: Nolwazi Mbatha (Institutional admin, 103 boards), Anton Kruger (College admin, 23) and
  Demo Administrator (9; devadmin never appears), plus the bracketed "‹name›" College admin on the six no-scope boards. No Anele
  Dlamini, no second institution's admin, no title (Dr, Prof) and no he, she, his, her or him in any board, attribute, canvas
  title, annotation or Spec board. Nolwazi Mbatha's menu is Home / People (Invitations, Trainees, Assessors, Users) / Curriculum
  (Curricula, EPAs, Activity types, Entrustment scales) / Reviews (Decisions due, Committee reviews, Decision panels, Entrustment
  decisions) / Access and audit (SSO mappings, Audit log) / My data rights, "Acting as Institutional admin". Anton Kruger's is flat:
  Home, Specialities, EPAs, Curricula, Activity types, Entrustment scales, My data rights, "Acting as College admin". Demo
  Administrator's is Platform / Organisations / People / Catalogue / Reviews exactly as FIXTURES, Entrustment scales under
  Catalogue. Activity types is lit on all 141 page boards.
- **Types, keys, ids, dates, counts.** The 24 types, keys, "Set by" words (CPSA, Demo College, Kgosi Kgari Teaching Hospital, the
  platform) and scopes are as FIXTURES; the lists run in the ask's order (A01 23 rows, KGK first with the only Edit; A02 the twelve,
  "12 of 22 ... match “cpsa”."; A03 "1 of 23 ... “KGK”."; A05 KGK Not published, Saved 2026-11-05; A06 22 rows, ten Edit then twelve
  View; A07 23 with CPSA Case Presentation Log among the Edit rows; A08 24, Edit on all but the two system-managed). Descriptions are
  94 and 101 characters (counted by script) and the counters say so. Every date is 2026-11-05 or a ‹date› bracket (E04-E07, E12,
  E15, where the ask gives none); every version is 1, or the Illustration's 2 and 3. Ids 23 and 24 are the addresses.
- **Mini-CEX, MSF, Learner Feedback, KGK.** Mini-CEX's form settings, ten fields all required, choices and values, workflow
  (Draft, Requested, Completed final, Declined, Cancelled; Submit, Complete, Decline, Cancel with the ask's actors, checks and notes)
  and credit sentence are word for word. MSF's and Learner Feedback's forms, settings and unrequired fields are as pasted. KGK's
  six fields, its four options, the workflow Log and Cancel and the laid-out JSON (checked against the ask's block, two-space
  indent) and `{"counts_for": []}` on three lines are right. The 15 PAED EPAs, no EPA-001, "(LTHCs)" before 6.14, in every preview.
- **Refusals, results, dialogs.** The summary, "Everything you typed is kept.", the four summary lines with their prefixes, the
  at-the-place sentences, Q14's two Discard bodies, Q16's dialog, T17's dialog and two results and T18's two notes are the ask's
  words. The Unavailable board carries no id and no cause. The dialogs open on Cancel (`autofocus`) and name their target.
- **Tabs.** Writers' boards carry five tabs, readers' four; /new and a first save sit on Details, an existing type on Form;
  "1 problem" is on the Form tab (R2-C14, R2-C15, R2-C20), Workflow (D01) and Credit (D04).
- **Illustration chips.** 25 boards carry one (exactly the Spec's list of 25); none sits on a state a step plays. A `‹…›` bracket
  stands wherever FIXTURES gives no value (R2-C19, E04-E07, E12, E15, the no-scope College).
- **Runbook hits.** The Steps board's word hits (Back to list, Metadata, code block, on the Form tab, v1, v0, Draft saved, could
  not be found, Save a draft to publish, A.7.9, A.7.14) agree with a fresh grep of the runbook (two gaps: finding C4). The states.md
  rows it lists are right and each is drawn.

## Checklist (STATES TO DRAW)

All 64 numbered states are boards: A01-A12 (A09 two boards), B01-B11, C01-C21 (C08 two, C15 two), D01-D05, E01-E15 (E14 390
only), each with its twin. Done 61, partly 3 (A12, C19, E02: findings C6, C9, C2). Missing 0.

## Steps board: counts recomputed under the ask's rule

Rule: presses + fields; a radio choice is a field, not a press; a field that arrives chosen is none; the menu is one more at 390.

| Journey | Board says | Recount | Verdict |
|---|---|---|---|
| 1.24 Activity types, Show, Clear; `cpsa` | 3+1, 390 4+1 | 3+1, 4+1 | holds |
| 1.25 View, View overall_level, Details, Workflow, Credit, trail | 6+0 | 6+0 | holds |
| 1.26 New activity type, Save draft; Key, Name, Description | 2+3 | 2+3 | holds |
| 1.27 | 15+24; 390 17+24 | 1280: 15 presses, fields 23 (C5); 390: 15+26 | wrong at 390 (C1); 24 doubtful (C5) |
| 1.28 Workflow, Save, Save; JSON, actor | 3+2 | 3+2 | holds |
| 1.29, 1.30 | 1+0 each | 1.29 holds; 1.30 reads Details and the Publish tab in its Expect | C2 |
| 1.26-1.30 | 22+29; 390 24+29 | 1280 holds; 390 22+31 | C1 |
| 1.31 trail, Show; `KGK` | 2+1 | 2+1 | holds |
| 6.14a Activity types, View, trail, New, Save, trail; Key, Name, Description, Sub-speciality, Speciality | 6+5; 390 7+5 | 6+5; 7+5 | holds (the two radios are fields, as drawn) |
| A.7.9 Menu, Activity types, Edit, Preview | 4+0 | 3+1 (Preview is a radio) | C1 |

## Findings

| # | Sev | Board | Finding |
|---|---|---|---|
| C1 | must | R2-0-steps (1.27 at 390, 1.26-1.30 at 390, A.7.9) | **The Preview control is counted as a press, against the board's own rule.** Q5's "Edit the form" / "Preview" are two radios (R2-B01, R2-C21 `input[type=radio]`), so by the rule at the top of the board each is a field. 1.27 at 390 is 15+26, not 17+24; 1.26-1.30 at 390 is 22+31, not 24+29; A.7.9 is 3+1, not 4+0. 6.14a's two Scope radios are counted as fields, so the board is inconsistent with itself. Spec M7 repeats "22 + 29" (1280 holds). |
| C2 | must | R2-0-steps 1.30; R2-E02, R2-E01, R2-E03 | **1.30's count and E02's tab do not match its Expect.** The Do and count are "Publish: 1+0", pressed from the Credit tab (1.29). T2 says a save keeps the selected tab, so after the publish Credit is selected; E02 draws Details. The Expects "the Key a read-out, 'Fixed: version 1 is published.'" (Details tab) and "the Publish tab's versions one row" (Publish tab) can be read only after two more presses, which 1.25's row does count (Details, Workflow, Credit). Either 1.30 is 3+0 and E02 is Details reached by press, or the Expect drops those two lines. E01 and E03 show the Publish tab, which no step opens (C7). |
| C3 | must | R2-0-steps, states.md rows | **Five states.md rows that change are not listed.** M7 asks for every Expect and every states.md row that changes. The board lists 903, 905, 907-910 and 913-920. Missing: **902** (Searched, 1.24: applied with Show, with the count line), **904** (All, 1.31: Set by, Version 1, the count line), **906** (the list's Loading: "Loading the activity types." over a skeleton, R2-A10), **911** (the User field editor: now in place under its row, Names a by label, R2-C13) and **912** (Duplicate key refused: now the summary plus both fields, R2-C14). The ask omitted them too; each is drawn, so only the rows are missing. |
| C4 | should | R2-0-steps, runbook hits | **Two runbook hits are missing or off.** (a) The 6.14a Actual's "Draft saved" and "v0" are both on act-6-catalogue.md:483; :482 is the "no 'Draft saved.'" clause. (b) The 1.26 Expect's "Scope offers her only what she may save: Institution, with KGK the only institution" (act-1-setup.md:839-841), which becomes a read-out, and 6.14a's "Scope offers Speciality and Sub-speciality only" (act-6-catalogue.md:459-461), which becomes radios plus a read-out, have no hit; the 1.26 row covers the first in one clause, the second not at all. |
| C5 | should | R2-0-steps 1.27 | **The 1.27 count rests on two unstated assumptions.** (a) Fields: section key and title 2, Title key and label 2, five new fields' key and label 10, five type picks, Audience's options, the key retyped, and two Form-settings picks (Encounter date, EPA field; the rating's "None" arrives chosen, R2-C09) make 23, not 24; 24 equals today's figure and looks carried over. (b) Presses: 15 assumes "Add field" opens the new row's editor. The ask and the Spec say only that it "adds 'New field'"; if it does not, five Edit presses are added (20). Say which on the Spec. |
| C6 | should | R2-A12, R2-E12 | **The inactive list row contradicts the board it follows.** A12 ("after R2-E12") reads KGK Version 1, Draft None, Inactive; E12 shows the same type with Draft "Saved ‹date› by Nolwazi Mbatha" and Discard draft offered, because Save draft saved a draft. A12's Draft cell should be "Saved ‹date›" (or E12 shows no draft). |
| C7 | should | R2-A11, R2-B11, R2-D05, R2-E01, R2-E03; R2-E04, R2-E08 | **Chips and the replay.** The ask says to label a state the replay never reaches "Illustration". The two load errors (A11, B11) have no capture and no step; D05 (/new, Workflow tab), E01 and E03 (the Publish tab) are reached by no step as the Steps board counts them (C2). None carries a chip. Either chip them or add the presses. Conversely E04 (states.md:916) is chipped and E08 (states.md:917), the same kind of scratch row from the same pass, is not; pick one rule. |
| C8 | should | R2-C07, R2-C20 | **Two NEW sentences are on boards but not on the Spec.** R2-C07 draws Target "None: a Global type belongs to no institution or speciality." (the ask says only "no target for Global"); R2-C20 draws "No fields in this section yet." under the head's "This section has no fields. Add a field, or delete the section.". Spec-3's message table lists neither, and its NEW rows do not carry the words they replace. |
| C9 | should | R2-C19 | **The folds contradict T23.** T23: "all start open". C19 draws the first section open (Hide fields) and sections 2 to 6 folded (Show fields, no rows). Say it is the case after folding, or draw all six open. |
| C10 | should | brief § 3; R2-A01, R2-A02 | **No board draws 1.24's cleared list of 22.** The brief's first state is "Searched `cpsa`; all, 22"; Steps 1.24 says Clear "brings back 22" and its count line would read "22 activity types.". The only 22-row boards are Anton Kruger's (A06) and the no-scope Illustration (C08-list). A01's 23 includes KGK. |
| C11 | should | R2-B06, R2-B07, R2-B08 | **MSF's help texts are drawn, Learner Feedback's are not.** The five MSF help sentences are not in FIXTURES; they are true (msf_cpsa/schema.json; the Spec says "from capture 6.14a-2"). Learner Feedback's seed has helps (e.g. "How many distinct teaching contexts...") that B07 leaves out, so the two read-only previews differ in kind. State the source on the Spec's FIXTURES line and draw or bracket the other. |
| C12 | nit | R2-D01 | The JSON "as typed" is drawn laid out; the runbook pastes the compact form, and Q13 lays it out only after a save. |
| C13 | nit | R2-A01, R2-B01 to B05 | The Demo types' "Version 1" and Mini-CEX's "2026-11-05, Wombat's catalogue" are marked "inferred" in FIXTURES; the Spec does not say so. |
| C14 | nit | R2-C08-list, R2-A09-create | A College admin with no scope sees 22 rows including all twelve CPSA types and "Set by CPSA" (Q11's rule for another College's admin is unstated); the Administrator's empty catalogue carries "You can change every type but the two that Wombat's releases write." with no types. Both are Illustrations; say or bracket. |
| C15 | nit | R2-0-spec (M2 row) | "key under Name in <code>; Draft ..." the `<code>` is never closed or filled (the word "monospace" was lost), so the cell reads "in ;" and may style the rest of the cell as code. |
| C16 | nit | R2-B08, R2-E12 | The chips name "Q9" and "Q15's result" (decision codes on a face) and "states.md:916" (a file path). Canvas-only, so allowed, but they are the only codes visible. |

## Words that are NEW or PROPOSED and not on the Spec

R2-C07 "None: a Global type belongs to no institution or speciality." and R2-C20 "No fields in this section yet." (finding C8). Every
other NEW or PROPOSED sentence on a board is on the Spec's message table or T1-T24: Nothing to compare, Not published yet, Nothing on
the form changes, the T15 sentences, T17's five, T18's two, T22, T10's help, the Credit line, "Date delivered is no longer in the
form...", the two at-the-place NEW refusals and the three failures.

## Illustration chips

25 boards: A09-create, A09-nocreate, A12, B07, B08, C07, C08-new, C08-list, C15-before, C15-after, C17, C18, C19, C20, D04, E04,
E05, E06, E07, E09, E10, E11, E12, E13, E15 (and their twins). None sits on a state a runbook step plays; the gaps are in C7.

## Decisions this review leaves to the operator

- C1: recount with radios as fields (the rule's own text), or restate the rule so "Preview" is a press.
- C2: 1.30 as 3+0 with the two tab presses (and E02 on Details reached by press), or a shorter Expect for 1.30.
