# Flow 16 round 1: assessment (T394, part 1)

Read-only, at HEAD `af8a5c45` (the boards are `b486bb38`). Canvas "Wombat 16"
(https://claude.ai/artifact/AqaH447KL91WCTbjQFJnWf), design system copied at version 1793670418-6412 (`canvas.json`,
`designSystems`). Boards: `design/flows/16-college-catalogue/round-1/*.dc.html` (50, every board's text and markup
read), `canvas.json` and the round's stylesheet `f16r1.css`. `W/` = `src/Wombat.Web/Components/Pages/`, `A/` =
`src/Wombat.Application/Features/`, `Nav/` = `src/Wombat.Web/Navigation/`, `app.css` = `src/Wombat.Web/wwwroot/app.css`,
`seed` = `src/Wombat.Infrastructure/Persistence/Seeds/paediatric-epa-v11.1.json`. **Obs** = observed in the code, the
runbook or the boards. **Inf** = inferred. Brief sections are `design/flows/16-college-catalogue.md`'s. The
`design/baseline/` captures were not opened: today's pages were read in the code, at the lines cited.

**Q1–Q14 are decided** (the operator took every recommendation as written on 2026-11-04; Q3 is D56, Q8 is D57,
`execution/knowledge/EPA-PROGRAMME.md:611–612`). The ask the canvas got (`round-1-ask.txt`) still lists them as open
questions, so the canvas answered some of them itself (Spec § 9). A board that contradicts a decision is a must-fix
below, unless it argues to reopen it; where it argues, the point is a question in § 8.

**Decided 2026-11-04:** the operator took the picks (A2, B1, C1 with C2's textarea) and Q15–Q17 as recommended ("yes").

## Summary

- **The College's tree: pick A2** (one Specialities page per College: each speciality an `<h2>` section, its
  sub-specialities and their default ladders in a table under it; the records stay form pages of their own).
  - **Why A2:** it is Q1 as decided, and it meets the goal's "read the College's tree at a glance": 1.12–1.14 take one
    press (today three). Choosing the default at create cuts 6.12's separate save (Steps, recomputed: 3 + 3, today
    5 + 3).
  - **Reject A1.** It keeps today's separate Sub-specialities page, which is what Q1 decided against. Its one gain, the
    default ladder as a column, is A2's too.
  - **Reject A3.** It needs a page shape of its own for a speciality (a record and a table), with Status as an end
    section, which D56 does not have (reactivating is the Active box). Keep it as the fallback if a College ever holds
    many specialities (Spec § 9).
  - **Take from A1:** its stacked 390 rows ("Default scale: CPSA Paediatric Entrustment Scale v11.1 · Active"), which
    A2's 390 board already copies. Nothing structural.
  - **Correct in A2:** the scales and the Home card (M1, M2); the Unavailable words (M3); the two "Edit Paediatrics"
    and other same-name collisions (M8); 6.7 does not fold into 6.4 (M7); the tree's subtitle (S9).
- **Colleges and a College: pick B1** (the record is its form, flow 15's EPA page shape).
  - **Why B1:** it is D56 as decided: an outline "Deactivate…" offered only on a College stored as active, and the Active
    box to reactivate. Saving with Active unticked asks the same question, as the EPA page does
    (`W/Admin/Epas/EpaEdit.razor:395–430`). It reads nothing new and keeps 6.3's two presses.
  - **Reject B2.** Its "Reactivate" action contradicts D56 (M6). Its Specialities count and College admins section are
    DATA for a page opened a few times a year. Its rows-as-links layout costs 6.3 a press (3 against 2).
  - **Take from B2:** only the inactive status line, "Inactive. A new invitation cannot name this College.", as the
    short form of B1's help under an unticked Active box (B1-4's help runs to three sentences).
  - **Correct in B1:** the h1 (S11); the duplicate refusal is DATA (S10); the Unavailable words (M3); Created as the
    South African day (N2).
- **Scales and a scale: pick C1** (the rung table, corrected).
  - **Why C1:** it is Q11 as decided. Delete… asks first, then the refusal is the focused result at the list's top,
    which is also 6.8's Expect as written ("Delete asks first … Once confirmed, it is refused",
    `act-6-catalogue.md:178–190`). It adds no shipped class.
  - **Reject C2.** It hides Delete on a scale in use, so 6.8 never asks and never refuses, against Q11 (M4). It needs
    three NEW classes (`.rung-edit`, `.rung-edit--removed`, `.rung-edit-moves`, f16r1.css S1–S3) and a "Used by" read
    (DATA) on the list and the page.
  - **Take from C2:** each rung's description as a textarea, since the CPSA descriptors run to 200–300 characters
    (`seed`, `scale.levels`) and C1's single-line input hides them (S4). Also "Create scale" as the create page's submit,
    as B0 has "Create college" (N9).
  - **Correct in C1:** the College admin's read-only list (M1); Q11's dialog words (M4); Q6's refusal (M5); the built
    caption, subtitle and order (S2); 6.8's sentence as built (S1); the scale page's other refusals (S3); moving a rung
    on a scale in use (Q16).
- **The must-fixes** (§ 7):
  - **M1:** D57 contradicted. The Spec answers Q8 "Access denied as today". Dr Kruger's menu has no Entrustment scales,
    and his pages say "the scales are kept by the platform administrator; this list names every one", which is D57's
    rejected alternative.
  - **M2:** Q12 contradicted. Home's card has three links and no Activity types, in new words.
  - **M3:** Q7's Unavailable words. The boards say which cause applies ("or it belongs to another College"; "or it was
    deleted"), against DESIGN.md's rule and Q7's words.
  - **M4:** Q11 contradicted. C2 offers no Delete on a scale in use, and C1's dialog has neither Q11's words nor its
    "Delete scale".
  - **M5:** Q6 contradicted. The rung refusal names one item and a count, where Q6 names up to three items and then
    "and ‹N› more".
  - **M6:** B2's Reactivate action, against D56 (moot once B1 is taken).
  - **M7:** the Steps board. A2 and A3 fold 6.7 into 6.4, which the story's order makes impossible. It also misses
    Expects whose words change (1.8, 1.4's Note, Phase 6.B, 6.2's and 6.3's subtitle, "Manage sub-specialities", 6.8's
    sentence) and marks 1.15 unchanged.
  - **M8:** the speciality and the sub-speciality are both named "Paediatrics". A2's tree offers two "Edit Paediatrics"
    controls with different targets, and the records share an h1, a tab and a dialog title.
- **The attachments did not arrive** (Spec § 1, Obs: "Opened: none. Not opened: all 14"). The boards bracket what only a
  capture held. That is right, but some brackets the code fills (S7). Round 2's ask must re-attach the screenshots and
  paste those values (§ 9).
- **The step count: 18 is right.** 1.2–1.4 is 3 steps, 1.12–1.15 is 4, 6.1–6.8 is 8 and 6.10–6.12 is 3, so 18 in all.
  The runbook has no lettered sub-step in those ranges (`act-1-setup.md:76–390`, `act-6-catalogue.md:60–268`; 6.9 is
  Prof Mbatha's and outside the flow). The "19" in the brief's ask (§ 1, "ASK") and in `round-1-ask.txt:322` is a
  miscount. Part 2 has the other 15 steps (1.16–1.18, 6.13, 6.14, 6.17–6.19, 6.23, 6.24, 6.28–6.31, A.7.10), so 33
  in all, as § 1 says.

## 1. What was drawn

50 boards in five rows (`canvas.json` notes r1row0–r1row4).

| Row | Boards | State drawn |
|---|---|---|
| 0 · Spec, Steps | R1-0-spec, R1-0-steps | The screenshots (none arrived); each variation in a line; components; DESIGN.md rules changed; the class table (K1–K10 canvas, S1–S3 shipped NEW, C2 only); PROPOSED; DATA; messages by kind; Q1–Q14 as round 1 answers them; edge cases; accessibility. Journeys counted for A, B, C at 1280/390; 18 Expects |
| 1 · The tree (wireframes) | A1, A2, A3, each with a 390 board | Frame 1 Home's card (6.10); 2 Specialities (1.12, 6.11); 3 sub-specialities (1.13, 6.12); 4 a record (1.14, 6.11 or 6.12); 5 devadmin's CNSA from empty (6.3, 6.4, 6.7); 6 the deactivation dialog and Unavailable |
| 2 · Colleges (fidelity) | B0 (shared), B1-1…4, B2-1…4, each with a 390 board | 6.1's refusal; the list of three (1.2, 6.1); CPSA's record saved (1.3, 6.2); Deactivate… asked; deactivated |
| 3 · Scales C1 (fidelity) | C1-1…7, each with a 390 board | The list (1.4, 6.6); Delete… asked; refused (6.8); the CPSA ladder (1.4); create refused (6.5); sixth rung saved (6.6); a rung refused (Q6) |
| 4 · Scales C2 (fidelity) | C2-1…5, each with a 390 board | The list with Used by; the CPSA ladder; create refused; sixth rung saved (Delete offered while unused); a rung refused in its block |

- **The wireframes** are grey `.wf-*` boxes with real words, right for a wireframe. The only inline styles are the
  wireframes' 390 padding (canvas only).
- **The fidelity boards** link `ds/wombat/tokens.css`, `components/bundle.css` and `f16r1.css` in that order, with
  `body{margin:0}` alone in `<style>` (Obs, as the ask required). Every class they use is in `app.css`, in the shell's
  scoped CSS (the `nav-*`, `brand*`, `role-*` classes), or in f16r1.css's S rows. The one exception is `.record-page`
  on B2-2 (N1).
- **No gendered word on any page** (Obs). "He", "his" and "him" appear only in the Spec's own notes (§ 9, § 10) and in
  A1's frame label (N8).

**Not drawn in round 1** (round 2's, S8):
- **For every page:** required fields empty, a failed save, loading, a load error and an unknown id. The Spec defers
  required fields and Unavailable to round 2, and draws the words only.
- **The College admin's read-only scales list** (D57; M1).
- **On the tree:** an inactive sub-speciality; the heavy cases (a speciality with six sub-specialities, five Colleges, a
  ladder of 8 rungs, a scale used by forty items); a College admin whose account names no College (Spec § 10 has the
  words only).
- **The scales:** "Entrustment scale deleted."; a rename that unbinds a form (the Spec defers it, with DATA).
- **768 px** anywhere, and the 900–1100 px sweep of the new tables against their container (§ 11, flow 15).

## 2. The College's tree: A1, A2, A3

All three draw, rightly (Obs):
- no Back link on any page, the trail alone (Q9). Today: Back to colleges for every caller
  (`W/Admin/Institutions/SpecialitiesList.razor:11`), Back to specialities (`SubSpecialitiesList.razor:12`,
  `SpecialityEdit.razor:12`), Back to sub-specialities (`SubSpecialityEdit.razor:13`);
- a record titled by its name (h1 "Neonatology", subtitle "Sub-speciality of Paediatrics"; today "Edit
  sub-speciality", `SubSpecialityEdit.razor:11`);
- the outline "Deactivate…" with the Active box beside it (D56), and its dialog opening on Cancel. The dialog says what
  deactivation does in D56's terms: no new invitation can name it, it leaves an institution's adopted disciplines, and
  the EPAs, the curricula and their trainees carry on;
- devadmin's way in through Colleges › CNSA › Specialities, with Colleges lit (`Nav/NavOwners.cs:142–145`), and the
  empty state naming the College;
- the sub-speciality help as built, "Committee STARs for this programme's trainees are limited to this scale's levels."
  (`SubSpecialityEdit.razor:50–59`), though with a sentence after it that M1 removes.

- **A1, separate lists, tidied.** Read the tree (1.12–1.14): 2 / 3. 6.12: 5 + 3 (the board says 6 + 3; Steps 5 + 3,
  which is right).
- **A2, one page per College.** Read the tree: 1 / 2. 6.12: 3 + 3 (the board's figure; the Steps board's 4 + 3
  contradicts its own press list "Specialities; Add a sub-speciality to Paediatrics; …; Add sub-speciality", N10).
- **A3, a speciality's page holds its sub-specialities.** Read the tree: 2 / 3; 6.12: 4 + 3.

**Recommend A2** (§ Summary). Correct in it:
- **M1, M2:** the Home card's words and links, the sidebar, and the sub-speciality help (frames 1–4).
- **M3:** the Unavailable words (frame 6).
- **M8:** name the two "Paediatrics" apart.
- **S9:** the tree's subtitle, "College of Paediatricians of South Africa (CPSA): its specialities, their
  sub-specialities and the ladder each uses." Today's "College: College of Paediatricians of South Africa (CPSA)"
  (`SpecialitiesList.razor:8`) is quoted by 1.12, 6.2 and 6.3. Keep it, or list each Expect it changes.
- **M7:** 6.4 cannot choose the CNSA scale. It does not exist until 6.5, so 6.7 stays a step of its own: Specialities,
  Edit Adult Neurology, Default, Save.
- **N4, N6:** "Add a sub-speciality to Paediatrics" against Q1's "Create sub-speciality"; the caption "Paediatrics's
  sub-specialities".
- **Q15:** the "Reach" line in the deactivation dialog.

**Against the code** (Obs):
- `CreateSubSpecialityCommand` takes no default scale (`A/Institutions/Commands/CreateSubSpeciality/CreateSubSpecialityCommand.cs:6–10`).
  Only `UpdateSubSpecialityCommand` does (`…/UpdateSubSpeciality/UpdateSubSpecialityCommand.cs:12`), and today the
  page offers the select only on an existing sub-speciality. A2's default at create is a rightly marked PROPOSED
  command change.
- `SubSpecialityDto` carries `DefaultEntrustmentScaleId`, not the scale's name (`A/Institutions/SubSpecialityDto.cs:17`).
  "Default scale" by name is DATA, and cheap: the scales list is one unscoped read.
- The tree needs every speciality's sub-specialities in one read. Today that is one query per speciality
  (`GetSubSpecialitiesForSpecialityQuery`) (DATA, cheap).
- Today a save stays on the record; a create goes to the new record with no result (`SpecialityEdit.razor:164`,
  `SubSpecialityEdit.razor:178`). A2's return to the tree with a one-shot token (`CatalogueLinks`, flow 15) is
  PROPOSED, rightly marked.
- **What deactivation reaches** (D56, checked): the three deactivate handlers set one flag
  (`…/DeactivateSubSpeciality/DeactivateSubSpecialityCommandHandler.cs:30`). `InvitationRules` refuses an inactive
  College, speciality or sub-speciality at issue (`A/Invitations/InvitationRules.cs:116`, `:126`, `:137`), and
  `AcceptInvitation` does not re-check, so "everyone already invited keeps their invitation" (B1-3) holds (Inf, from
  `AcceptInvitation.cs:87`). `AdoptedDisciplines.LoadAsync` leaves inactive ones out
  (`…/GetAdoptedDisciplinesForInstitutionQuery.cs:75`, `:86`). It feeds a person's role scopes (`AddRoleToUserCommand.cs:102`,
  `W/Admin/Users/UserDetail.razor:711`) and SSO group mappings (`A/Sso/CreateSsoGroupMapping.cs:214`). It does not
  feed the EPA picker, as the brief inferred (§ 5, "What a deactivation does"). So the dialog's "it leaves the
  disciplines an institution has adopted" means: no new speciality or sub-speciality admin role, and no SSO mapping,
  for it. Round 2 may say that plainly (PROPOSED words, within D56).
- `MySpecialitiesRedirect` sends a College admin with no College to `/admin/colleges`, which refuses him
  (`W/Admin/Institutions/MySpecialitiesRedirect.razor:26–32`). The Spec's § 10 words for it are right (Q13 folds it).

## 3. Colleges and a College: B1, B2

Both draw, rightly (Obs):
- the list of three in 6.1's order (CNSA, CPSA, Demo College), StackWide (today none, `W/Admin/Colleges/CollegesList.razor:20`),
  row actions named for the row ("Edit College of Neurologists of South Africa");
- B0's create refused under Short code, `aria-invalid` and `aria-describedby="code-err"`, what was typed kept, and the
  trail Home › Colleges › Create college;
- the dialog "Deactivate College of Neurologists of South Africa?": Cancel autofocused, `aria-labelledby` and
  `aria-describedby`, its body D56's ("A new invitation will no longer be able to name it. That is all it changes: …"),
  confirm "Deactivate" as `btn-danger` (as `EpaWords.DeactivateConfirm`, `W/Admin/Epas/EpaWords.cs:135`, with
  `DangerAction`);
- the result naming the College, "College of Neurologists of South Africa deactivated. A new invitation can no longer
  name it.", focused; Deactivate… gone once inactive (today offered whatever the state, `CollegeEdit.razor:69`).

- **B1** keeps "Edit college" as the h1 (1.3, T190), the Active box with help, and Save that stays on the record.
- **B2** gives each College a page of `<h2>` sections (Details, Specialities, College admins, Status), with Save
  details and a Status end section.

**Recommend B1** (§ Summary). Correct in it:
- **S11:** the h1. A2 titles a speciality's and a sub-speciality's record by its name, and flow 15 titled an EPA's page
  by the EPA. One rule for the flow: "College of Paediatricians of South Africa", subtitle "CPSA" (and the College's
  state). 1.3's Expect changes with it (M7).
- **S10:** the duplicate refusal names the College that holds the code. That is DATA and a handler change.
- **M3:** "College unavailable" / "This College does not exist, or you cannot open it." / Go to Colleges.
- **N2:** Created. Today it is "yyyy-MM-dd HH:mm" in the server's local time with no zone (`CollegesList.razor:37`).
  B1's date alone is fine, but it must be the South African day and marked as a change.

**Against the code** (Obs):
- `CreateCollege` and `UpdateCollege` turn every `DbUpdateException` into "A college with the same name or short code
  already exists." (`A/Colleges/CreateCollege.cs:57–60`, `UpdateCollege.cs:60–63`). B0's words need a check by name
  and by code that reads the holder, with the database path left for a race (T272).
- Today a College's save already stays on the record ("College saved.", `CollegeEdit.razor:160`), and a create opens
  the new record with no result (`:154`). B0's "College created." is a new result (PROPOSED, unmarked).
- `CollegeDto` has no speciality count and no admins (`A/Colleges/CollegeDto.cs:7–13`). B2's DATA is right, and moot.
- The College's record and every College command are the Administrator's (`CollegeEdit.razor:3`,
  `CreateCollege.cs:38–40`, `UpdateCollege.cs:43–45`, `DeactivateCollege.cs:26–28`). The boards offer nothing else.

## 4. Scales and a scale: C1, C2

Both draw, rightly (Obs):
- `<h2>` Details and Levels in place of today's `<h3>`s (`W/Admin/EntrustmentScales/EntrustmentScaleEdit.razor:43`,
  `:57`); one h1;
- Order shown on this page only (D32), and the Levels help on label against rank;
- 6.5's refusal under the fourth rung's Label, by its label, `aria-describedby="l4-err"`, every rung kept as typed;
- Remove as an outline button that asks nothing (Q11; today `btn-danger`, `:97`); the moves named by label ("Move 3a
  up"; today "Move level 3 up", `:95`);
- a rung refusal that names the rung by its label and the item by "PAED-001 on Paediatric EPA Curriculum (11.1)". That
  is right against the seed: PAED-001's minimum is rank 6, which is rung "5" (`seed`, `minimumLevelOrder: 6`). The
  refusal is never "level 6".

- **C1:** the rung table with inputs (`.clinic-table--inputs`, `app.css:395–426`), Delete… on each row, and the
  refusal at the list's top with "Open Adult Neurology" (PROPOSED).
- **C2:** each rung a fieldset (NEW); "Used by" on the list and the page (DATA); Delete offered only while unused; a
  removed rung kept on the page until Save.

**Recommend C1** (§ Summary). Correct in it:
- **M1:** the College admin's list. It needs the reader's subtitle, no Create scale and no actions column, as Prof
  Mbatha's list has (`EntrustmentScalesWords.ReaderSubtitle`, `W/Admin/EntrustmentScales/EntrustmentScalesWords.cs:10–12`).
- **M4:** Q11's dialog. Title "Delete CNSA Neurology Entrustment Scale?" (as drawn), body "It is removed from every
  list. This cannot be undone.", confirm "Delete scale". C1's second sentence, "A scale that anything uses is not
  deleted: you will be told what uses it, and nothing changes.", adds a true fact and may stay, marked PROPOSED.
- **M5:** Q6's refusal.
- **S1:** 6.8's sentence as built. C1-3 calls its sentence "today's sentence kept", but it is rewritten ("CNSA Neurology
  Entrustment Scale was not deleted. It is the default scale of the sub-speciality Adult Neurology (Neurology). …").
  Today's reads "This entrustment scale is the default scale of the sub-speciality "Adult Neurology" (Neurology), so
  it cannot be deleted. Change that sub-speciality's default entrustment scale to another scale, or to no default,
  first." (`A/Epas/EntrustmentScaleReferences.cs:290–292`), which 6.8's Expect quotes. Keep it word for word, or mark
  the new words PROPOSED and change 6.8.
- **S2:** the list's built words and order (§ 7).
- **S3:** the other refusals.
- **S4:** descriptions as textareas.
- **N9:** "Create scale" on the create page.
- **Q16:** Up and Down on a scale in use.

**Against the code** (Obs):
- Today the create form starts with two empty rungs (`EntrustmentScaleEdit.razor:148–149`), so C1-5's "(a scale needs
  two; PROPOSED)" is built, not proposed. Today a save of an existing scale stays on the page ("Entrustment scale
  saved.", `:256`), so C1-6's "today returns to the list" is false. A create goes to the new scale's page (`:234`).
- The rung guard throws at the first item it finds (`EntrustmentScaleReferences.cs:213–222`). Q6's up-to-three is
  DATA: collect every item, then name three. On 11.1, nine items need rank 6 (PAED-001–007, PAED-010, PAED-012;
  `seed`), so at 1.4 the refusal reads three of them "and 6 more" (Inf: the guard reads `MinimumLevelOrder` and
  `MinimumLevelByStageJson`, and the stage-4 minimum is rank 6 for the same nine). After 6.29, 11.2's copies add nine
  more.
- **The guard runs only when a rung is removed, and checks only that each pinned minimum's rank still exists**
  (`A/Epas/Commands/UpdateEntrustmentScale/UpdateEntrustmentScaleCommandHandler.cs:62–84`; the remarks at
  `EntrustmentScaleReferences.cs:191–200`). Up and Down renumber the ranks with no check, and every minimum, STAR and
  comparison reads the rank. So Move 3a up on the CPSA ladder silently changes what every pinned item's minimum means
  (Q16).
- `GetEntrustmentScalesListQuery` is unscoped. The list's policy is `AdministratorOrInstitutionalAdmin`
  (`W/Admin/EntrustmentScales/EntrustmentScalesList.razor:2`), so D57 needs that policy widened, or a new one. The
  scale's page stays `Administrator` (`EntrustmentScaleEdit.razor:3`).

## 5. The Home card (screen 1)

Every structure draws one card, "National catalogue". It holds "College of Paediatricians of South Africa's catalogue:
its specialities and sub-specialities, its EPAs and its curriculum versions. Institutions adopt a version once you open
it for adoption." and a second paragraph on who keeps the Colleges and the scales, with three links (Specialities,
EPAs, Curricula).
- **Q12 (decided):** "Your College's specialities, EPAs, curricula and activity types. An institution can adopt a
  curriculum version once you open it." with the four menu links. The board draws three, and the Spec argues "the card
  is the national catalogue's three parts" (§ 9). That repeats what Q12 weighed. The College authors its activity types
  (D52; `Nav/NavItems.cs:160`, T300), and the card's own comment says "Its links are the CollegeAdmin's nav row"
  (`W/Dashboards/CollegeAdminDashboard.razor:3–6`). So it is a must-fix (M2), not a question. With D57 the nav row
  gains Entrustment scales, which Q17 settles.
- **The second paragraph** ("The Colleges' own records and the entrustment scales are kept by the platform
  administrator. You read a ladder by name where your College uses it …") is D57's rejected alternative. Under D57 the
  scales are his to read on Entrustment scales; the College's record stays the Administrator's. Say only that (M1).

## 6. The Spec and Steps boards

- **Spec § 1:** it says first that no capture arrived, and brackets in their place. Right, as the ask asked.
- **Spec § 4:** the NavMenu row must also carry D57: the College admin's row gains Entrustment scales
  (`execution/architecture/DESIGN.md:594`), with the list lighting itself for him. `NavMenuAuthorizationTests` parses
  that table. StackWide "on Entrustment scales" is built already (`EntrustmentScalesList.razor:61`) (N5).
- **Spec § 9:** Q8 and Q12 contradict decisions (M1, M2). Q2's part-2 sketch, "Opening 11.2 lets ‹n› institutions
  adopt it", counts who could adopt. Q2 counts who has adopted (active adoptions, and registrars on them), so part 2
  must not carry the sketch (§ 9).
- **Steps § 1 counts (recomputed):** the A and C rows hold except as noted in § 2 and N10. In the B and C rows' "Today"
  column: 6.2 is 4 + 1, not 3 + 1, since today's save stays on the record and 6.2 reads the list; 6.5 is 7 + 13, not
  "‹n›", since the form starts with two rungs; and 6.6's "+1 to read the list" holds today too (N10).
- **Steps § 2** (M7).

## 7. Corrections

Severity: **M** must (round 2 cannot go out without it), **S** should, **N** nit.

### Wrong against the code, the runbook or a decision

| # | Board(s) | Correction | Sev |
|---|---|---|---|
| M1 | Spec § 9 (Q8), § 4; A1–A3 frames 1–4 and their 390s; C1, C2 (no reader board); Steps 1.15 | **D57 is contradicted.** The Spec answers Q8 "Access denied as today". Kruger's sidebar has no Entrustment scales on any A board. Home's card and the sub-speciality help say "The scales are kept by the platform administrator; this list names every one. A ladder's rungs are read where it is used…", the line D57 rejected (`EPA-PROGRAMME.md:612`). Draw: Entrustment scales in his menu after Curricula and Activity types (or as DESIGN.md's table orders it; six links with Home, under `FlatLimit` 8, `Nav/NavItems.cs:85`); his read-only list with the reader's subtitle, no Create scale and no actions column (`EntrustmentScalesWords.cs:10–12`, T239); the help as built alone, or "Read each ladder's rungs under Entrustment scales." (PROPOSED); a scale's page still Access denied (6.10's second address). The C1/C2 Administrator subtitle "A College reads a ladder by name where it uses it." goes too (S2). | M |
| M2 | A1–A3 frame 1; Spec § 9 (Q12) | **Q12 is contradicted.** Draw the card in Q12's words, "Your College's specialities, EPAs, curricula and activity types. An institution can adopt a curriculum version once you open it.", with four links: Specialities, EPAs, Curricula, Activity types (§ 5). Today: "Your college's … Institutions adopt an active curriculum to train against it." with three (`CollegeAdminDashboard.razor:9`, `:11–13`). | M |
| M3 | A1–A3 frame 6; Spec § 8 (Unavailable) | **Q7's words, and DESIGN.md's rule that Unavailable never says which cause** (`DESIGN.md:1910–1912`): "This speciality is not available. It does not exist, or it belongs to another College.", "This College is not available. It does not exist.", "… or it was deleted." Use Q7's: "College unavailable" / "This College does not exist, or you cannot open it." / Go to Colleges (for the College admin, Go to Specialities); "Speciality unavailable", "Sub-speciality unavailable", "Entrustment scale unavailable" the same way, with the h1 as the title, drawn in the page's own place, lighting its owner. A list typed with another College's or speciality's id is the same state, never an empty list with Create (today `SpecialitiesList`, brief § 5). An unknown scale id stops returning silently to the list (`EntrustmentScaleEdit.razor:154–157`). | M |
| M4 | C2-1, C2-2, C2-5 (Delete section); C1-2 | **Q11 is contradicted.** C2 offers no Delete on a scale in use, so 6.8 neither asks nor refuses (`act-6-catalogue.md:180–184` asks for both). C1's dialog body "It is deleted for every College and institution, with its rungs, and cannot be undone." and confirm "Delete" are not Q11's "It is removed from every list. This cannot be undone." and "Delete scale". Take C1's flow with Q11's words (§ 4). | M |
| M5 | C1-7, C2-5; Spec § 8 (Rung needed) | **Q6 is contradicted.** "Not saved. Rung 5 cannot be removed: PAED-001 on Paediatric EPA Curriculum (11.1) needs rung 5 as its minimum level, and so do ‹n› other items. Change their minimum level first, or keep rung 5." names one item. Q6: one sentence per item, up to three, then "and ‹N› more": "PAED-001 on Paediatric EPA Curriculum (11.1) needs rung 5 of this scale, which the new levels leave out. Change that item's minimum first." On the seed that is PAED-001, PAED-002, PAED-003, "and 6 more" at 1.4 (§ 4). Keep C1's "Put rung 5 back" (PROPOSED). | M |
| M6 | B2-4; Spec § 6 (B2) | **"Reactivate" as an action of its own, asking nothing, against D56** ("reactivating is the Active box, as on an EPA"). Moot once B1 is taken. If any part of B2 survives, its Status section offers the Active box, not a button. | M |
| M7 | R1-0-steps § 1, § 2 | **The Steps board.** (a) "6.7: A2, A3: folded into 6.4 when the default is chosen at create": the CNSA scale is created at 6.5, after 6.4 (`act-6-catalogue.md:114–176`). So 6.4 has two fields and "No default", and 6.7 stays its own (+3 presses, +1 field). devadmin's A2 journeys are 6 + 4 (6.3–6.4) and then 4 + 1 (6.7: Colleges, Specialities on CNSA, Edit Adult Neurology, Save), 10 + 5 in all at 1280, not 6 + 5. (b) "1.15 Unchanged" is false under D57: he reads the list (M1). (c) Expects outside the 18 whose words change, unlisted: **1.8** (the sidebar gains Entrustment scales; the card gains Activity types, `act-1-setup.md:243–245`); **1.4's Note** ("a CollegeAdmin is admitted to neither page", `:115–116`); **Phase 6.B's intro** ("A CollegeAdmin chooses a sub-speciality's default scale from them", `act-6-catalogue.md:131`); and the Starting and end states that list menus, if any (search, BRIEF.md § 9 item 7). (d) Expects inside the 18 that the picks change, unlisted: **1.12, 6.2 and 6.3's** "College: College of … (CPSA/CNSA)" (A2's tree subtitle, S9; B1's record subtitle without "College:"); **6.3's** "offers 'Manage sub-specialities'" (gone under A2: the sub-specialities are on the tree); **6.8's** sentence (C1 rewrites it, S1); **1.3's** "Edit college" (if S11 is taken). (e) The count: 18 is right (§ Summary). | M |
| M8 | A2 frame 2 and 390; A1–A3 frames 4, 6 | **The speciality and its sub-speciality are both "Paediatrics"** (1.13: "The sub-speciality shares its speciality's name"). A2's tree draws "Edit Paediatrics" in the section header (the speciality) and "Edit Paediatrics" on the row (the sub-speciality): two controls with one name and two targets (WCAG 2.4.6; `Accessibility/RowNamesTests`). The records share h1 and tab "Paediatrics · Wombat", and "Deactivate Paediatrics?" names neither kind. Name the kind wherever the name alone is ambiguous: "Edit the speciality Paediatrics" / "Edit the sub-speciality Paediatrics" (accessible names; the visible label may stay "Edit"), the dialog "Deactivate the sub-speciality Paediatrics?", and the subtitle "Sub-speciality of Paediatrics" as drawn. PROPOSED words. | M |
| S1 | C1-3; Spec § 8 (Scale in use) | **6.8's sentence as built** (§ 4). | S |
| S2 | C1-1…3, C2-1 | **The scales list's built words and order.** Caption "Every entrustment scale" in place of "Entrustment scales, by name"; the Administrator's subtitle rewritten in place of "Define and maintain the rating ladders used by assessment forms and committee reviews." (`EntrustmentScalesWords.cs:15–19`; the brief: "subtitle as built for him; caption as built"); rows CPSA, O-R, CNSA where the list is by name: CNSA, CPSA, O-R Scale (6.9's Actual, `act-6-catalogue.md:204–207`). | S |
| S3 | C1-6, C1-7; Spec § 8 | **The scale's other refusals, by kind and in words**, none drawn: a level an entrustment decision uses ("One or more levels are referenced by entrustment decisions and cannot be removed.", `UpdateEntrustmentScaleCommandHandler.cs:72–73`); a rung an ended programme's record needs (T312, `EntrustmentScaleReferences.cs:229–233`, naming "Curriculum item {id}"); a taken name (`:46`, "An entrustment scale named '…' already exists."). Delete's other five uses also go unshown: an MSF question, a pinned item, an ended record, scored progress, an entrustment decision (`DeleteEntrustmentScaleCommandHandler.cs:141–190`). Each gets label words: no id, and the rung by its label. On the sub-speciality: the save's refusals "Entrustment scale {id} was not found." and "Speciality {id} was not found." (`UpdateSubSpecialityCommandHandler.cs:35`, `:45`) and its read-back (`:64–72`). | S |
| S4 | C1-4…7 | **Descriptions as textareas** (C2's). The CPSA descriptors are 200–300 characters (`seed`, `scale.levels`); a single-line input shows a fraction. The heavy case ("a ladder of 8 rungs with long descriptors") needs them whole at 1280 and 390. | S |
| S5 | A1–A3 frame 6 | **The deactivation dialog's "Reach (DATA, Q2)" line**: Q15. | S |
| S6 | Spec § 3 (DataTable with inputs) | **C1's levels table at 390** is `.clinic-table--stack` with inputs. Round 2 measures that each input and move is 44 px and nothing scrolls sideways (the Spec's own "Not yet measured"). | S |
| S7 | B1, C1, C2, A boards | **Brackets the code fills.** Paste them: the Demo College's description "Seeded demo college (national catalogue owner)." (`src/Wombat.Infrastructure/Persistence/DataSeeder.cs:280`); its one speciality (General Medicine, `:297`); the CPSA ladder's description ("Six rungs in four bands. Only level 3 is split: …") and six descriptors (`seed`, `scale`); the O-R Scale's description "Standard 1-5 observation-to-entrustment scale." and rungs (`DataSeeder.cs:313–320`); ‹n› items = 9 on 11.1 at 1.4 (§ 4); Created for CPSA and the Demo College is the replay day the database was seeded (Inf: 2026-11-02 in T392). | S |
| S8 | (not drawn) | **The states § 1 lists** under "Not drawn": required empty, failed save, loading, load error, unknown id, for every page; the College admin's scales list; an inactive sub-speciality on the tree; the heavy volumes; a College admin with no College; "Entrustment scale deleted."; the rename warning; 768; the 900–1100 sweep of the Colleges list and the tree's tables. | S |
| S9 | A2 frame 2 | **The tree's subtitle** (§ 2). | S |
| S10 | B0; Spec § 7 | **DATA missing from the table:** the College that holds a taken name or code (B0; `CreateCollege.cs:57–60`); the default scale at create (a command field, § 2); the tree's one read of every speciality's sub-specialities. | S |
| S11 | B1-2…4 | **One rule for a record's h1.** A2 titles records by name; B1 keeps "Edit college". Title the College's record by its name too (§ 3); the create pages keep "Create college", "Create speciality", "Add a sub-speciality". | S |
| N1 | B2-2; Spec § 3 | `.record-page` is not a class in `app.css`. Flow 12's page is `.user-page` and `.user-section` (`app.css:6515–6550`). | N |
| N2 | B1-1, B2-2 | Created as the South African day, marked as a change from "yyyy-MM-dd HH:mm" (`CollegesList.razor:37`). | N |
| N3 | B0, B1, A boards | "college" and "College" mixed: "Create college", "College saved.", "a College's name", "Your account names no College". Keep the built action labels; in running text the College is a proper noun (the ask's own usage). One rule, in the Spec. | N |
| N4 | A2 frames 2, 3 | "Add a sub-speciality to Paediatrics" / "Add a sub-speciality" against Q1's "Create sub-speciality" and the tree's own "Create speciality": one verb. | N |
| N5 | Spec § 4 | StackWide on Entrustment scales is built (`EntrustmentScalesList.razor:61`); only the three part-1 lists gain it. | N |
| N6 | A2 frames 2, 5 | Caption "Paediatrics's sub-specialities": "Sub-specialities of Paediatrics". | N |
| N7 | B2-3 | "Until one is invited, you keep its specialities, EPAs and curricula": the Administrator keeps them whether or not the College has an admin (`AdministratorOrCollegeAdmin`, `SubSpecialityEdit.razor:3`). Moot with B1. | N |
| N8 | Spec § 9, § 10; A1 frame 2's label | "his pages", "where he reads", "refuses him", "his College's list": notes only, but keep them neutral (BRIEF § 11, flows 13 and 14). | N |
| N9 | C1-5 | The create page's submit "Save": "Create scale", as C2-3 and B0 ("Create college"). | N |
| N10 | Steps § 1; A1, A2 boards | Counts that disagree: A1 6.12 is 6 + 3 on its board and 5 + 3 on Steps (5 + 3 is right); A2 6.12 is 3 + 3 on its board and 4 + 3 on Steps (3 + 3 is right, by Steps' own press list); "Today" for 6.2, 6.5 and 6.6 (§ 6). | N |

### Invented people or values

- **No invented person** (Obs). Anton Kruger, Demo Administrator, Kgosi Kgari Teaching Hospital, the three Colleges,
  the specialities, the sub-specialities, the three scales, the rung labels (1, 2, 3a, 3b, 4, 5; the CNSA five and
  "Supervises others") and their descriptions as given, CNSA's Created 2026-11-02 (D) are the ask's.
- **Right against the seed:** PAED-001 needs rung "5" on the CPSA ladder (`seed`); "Not entrusted to act…" and
  "Supervises others…" as the first and last descriptors.
- **Bracketed where the ask gave nothing,** rightly: descriptors, created dates, counts (S7 fills those the code holds).

### PROPOSED items, with a recommendation each

| Item | Recommend |
|---|---|
| Every subtitle, help line, refusal, result and dialog (Spec § 8) | Accept, with M3–M5, S1, S2 and Q6, Q7, Q11, Q12's words |
| A record titled by its name (A1–A3) | Accept, for the College too (S11), the kind named where names collide (M8) |
| A2: Specialities becomes the College's tree; the Sub-specialities route redirects to its section | Accept (Q1) |
| A2: the default chosen at create | Accept (cuts 6.12's second save; a field on `CreateSubSpecialityCommand`) |
| A2: records return to the tree with a one-shot token | Accept (flow 15's `CatalogueLinks`) |
| A3's speciality page; the default at create in A3 | Drop (A2 picked) |
| D56's question on a College, speciality, sub-speciality; Save with Active unticked asks the same | Accept (D56; EpaEdit's precedent, `EpaEdit.razor:395–430`) |
| B0's "College created." result; the refusal naming the College that holds the code | Accept (S10) |
| B2's page of sections, Reactivate, Save details, College admins | Drop (B1; M6) |
| C1: "Put rung 5 back"; "Open Adult Neurology" in the delete refusal | Accept |
| C1: "the create form starts with two empty rungs" | Not PROPOSED: built (`EntrustmentScaleEdit.razor:148–149`) |
| C2: Delete only when unused; a removed rung kept until Save, "Keep rung 5" | Drop (Q11; M4) |
| Q7's Unavailable on every part-1 record and list page | Accept, in Q7's words (M3) |
| Specialities for a College admin whose account names no College: "Your account names no College. Ask the platform administrator to add it." and no list (Spec § 10) | Accept (Q13 folds T291's redirect) |
| D57: Entrustment scales in the College admin's menu, its list admitting him read-only | Accept (decided; not drawn, M1) |

### DATA items, with a recommendation each

| Page | Item | Recommend |
|---|---|---|
| The tree | Each sub-speciality's default scale by name | Accept (cheap: id on `SubSpecialityDto`) |
| The tree | Every speciality's sub-specialities in one read | Accept (S10) |
| Specialities (A1, A3) | Sub-speciality count or names per speciality | Drop (A2 reads the sub-specialities themselves) |
| Deactivate a speciality or sub-speciality | Institutions that have adopted a curriculum in the discipline, and their names | Q15: drop |
| Colleges (B2) | Each College's speciality count | Drop (B1) |
| A College's page (B2) | Its specialities, its College admins | Drop (B1) |
| Create college (B0) | The College holding a taken name or code | Accept |
| Scales list, a scale's page (C2) | Used by | Drop (C1; the refusal after Delete… tells it) |
| The rung refusal | Every item that needs the rung, by EPA and curriculum, and how many more | Accept (Q6; the guard stops at the first today) |
| A scale's rename | The forms a rename unbinds | Built already (`EntrustmentScaleReferences.DescribeWhatARenameUnbindsAsync`); draw its words in round 2 |

### Access control

| # | Board(s) | Finding | Sev |
|---|---|---|---|
| A1 | (C, not drawn) | **D57 widens one policy.** `/admin/entrustment-scales` is `AdministratorOrInstitutionalAdmin` (`EntrustmentScalesList.razor:2`); it must admit the CollegeAdmin read-only (the `_isAdministrator` branch already chooses the subtitle and actions, `:19`). The scale's page and every scale command stay the Administrator's (`EntrustmentScaleEdit.razor:3`; `UpdateEntrustmentScaleCommandHandler.cs:29`). | M1 |
| A2 | A boards | **The tree's reads are scoped by College** (`CanAccessCollege`, `UpdateSubSpecialityCommandHandler.cs:26`, `:37`). A typed foreign id must answer null and draw Unavailable (M3), as the EPA's page does (`DESIGN.md:1921–1925`). | — |
| A3 | B boards | **The College's record stays the Administrator's** (`CollegeEdit.razor:3`); the College admin's 6.10 Access denied on `/admin/colleges/{Id}` holds. | — |
| A4 | A boards | **No page offers him what refuses him** (T291 item 2): no Back to colleges; the no-College redirect becomes words (Spec § 10). Holds on every A board (Obs). | — |

### Accessibility and the 390 boards

- **Good (Obs):** one h1 per board; `<h2>` sections; every table captioned ("Every College", "Levels of CPSA Paediatric
  Entrustment Scale v11.1, lowest first"); row actions named for the row; dialogs with `aria-labelledby`,
  `aria-describedby` and Cancel autofocused; refusals with `aria-invalid` and the message's id in `aria-describedby`;
  results focused; `.user-page` on the forms, so the system's 44 px phone rule reaches them (`app.css:6597`); lists at
  390 stacked (`clinic-table--stack-wide`).
- **M8:** two controls named "Edit Paediatrics".
- **S6:** C1's input table at 390, unmeasured.
- **The scale's Label inputs** are named by rank ("Label of the rung in order 3") while the moves are named by label
  ("Move 3a up"). That is right: the rank is this page's own column (D32), and today's are "Label, level 3" (`:91`).

### DESIGN.md and flows 01–15's patterns

- **The rules the Spec names** (§ 4) are the right ones: § Button system's destructive action, § Form system's
  refusals, § Table system's stacking, § Page-level patterns' Unavailable and Breadcrumbs, the NavMenu's owner table.
  Round 2 must also name:
  - § The NavMenu's role table, the College admin's row gaining Entrustment scales (`DESIGN.md:594`; M1);
  - its owner table, the sub-specialities list redirecting to the tree, still under Specialities / Colleges
    (`DESIGN.md:670`), and the scales list lighting itself for him;
  - § Page-level patterns' Unavailable list gaining College, speciality, sub-speciality and scale (`DESIGN.md:1910`).
- **The classes (Obs):** A, B1, C1 add no shipped class, as the Spec says. C2's S1–S3 go with C2. f16r1.css holds no
  hex. Its K-rows are canvas only, with literal 2/6/10/12 px paddings there alone (eight, canvas rules). Round 2 needs
  no NEW class, provided C1's description textarea sits in `.clinic-table--inputs` (`app.css:395–426`; check its 390
  rule keeps a textarea's width).
- **BRIEF § 11's lessons, checked:** no gendered word on a page (N8 for notes); no existing class duplicated as NEW (the
  one NEW set, C2's, is not taken); step coverage (M7); one word for one thing (N3, N4).

## 8. New questions for the operator

Q1–Q14 are decided. These three are new; each has a recommendation.

15. **Q15. The reach in a deactivation dialog.** A1–A3 frame 6 add "‹n› institutions have adopted a curriculum in this
    discipline: ‹Kgosi Kgari Teaching Hospital›" to "Deactivate Neonatology?" (DATA). D56 says the dialog "says only what
    it does", and Q2's counts are for an EPA's pause, an item and opening a version (part 2). What deactivation changes
    for an institution is narrow. It cannot give a new speciality or sub-speciality admin role, or map an SSO group, in
    that discipline (§ 2), and its trainees and curricula carry on. *Recommend:* no count. Keep D56's sentences, with
    "it leaves the disciplines an institution has adopted" said plainly ("an institution can no longer give anyone a
    role for it"; PROPOSED). The count needs a query for a rare act whose effect on those institutions is that small.
16. **Q16. Moving a rung on a scale in use.** C1 offers Up and Down on every rung of the CPSA ladder. The save checks
    nothing when ranks move (§ 4), and every pinned minimum, STAR and comparison reads the rank. So "Move 3a up" would
    silently change what PAED-001's minimum and every 3a rating mean. Removing a middle rung of a scale whose items stop
    short of its top does the same (the O-R Scale: the demo item's minimum is rank 4, `DataSeeder.cs:363`).
    *Recommend:* offer Up, Down and Remove only on a scale nothing uses (the delete refusal's uses, in its order); on one
    in use, show the rungs with Add level alone and a line, "Rungs can be reordered or removed only while nothing uses
    this scale." (PROPOSED), with the same refusal in the handler. File it as a task beside T324's scale half; the build
    folds it if the operator agrees.
17. **Q17 (refines Q12 with D57). Does Home's card link Entrustment scales?** Q12 gave four links "the four menu links".
    D57 makes it five in the menu, and the card's comment says its links are the nav row (`CollegeAdminDashboard.razor:3–6`).
    *Recommend:* Q12's four. The card names what the College keeps; the scales are read-only to him and one menu item
    away. Change the comment to "the College's own pages in the nav row". Its sentence stays Q12's.

## 9. What part 2's ask must carry forward from part 1

- **The picks:** A2, B1, C1, with every correction above (M1–M8, S1–S11, the nits in one pass), drawn at fidelity with
  every state § 1 lists under "Not drawn", at 1280, 768 and 390, before or with part 2's structures (the brief's Mode:
  "the picks with every state at fidelity").
- **The decisions, stated as decided, not as questions:** Q1–Q14 with D56 and D57, and the operator's answers to
  Q15–Q17. The ask must say so in its first lines. Round 1's ask listed them as open, and the canvas answered Q8 and
  Q12 against the decisions.
- **D57 in every part-2 page:** Kruger's menu holds Entrustment scales on every board; an item page's Minimum level and
  the scale column may link the ladder for him (PROPOSED); nothing says the scales are kept out of his sight.
- **Words part 1 settles, for part 2 to reuse:** "Enter a name." for required fields (once, under the field); the load
  error and failed-save sentences (Spec § 8); "‹Record› unavailable" / "This ‹record› does not exist, or you cannot
  open it."; a rung by its label, an item by "PAED-001 on Paediatric EPA Curriculum (11.1)"; "‹Record› created."
  after a create, as the model for Q5's curriculum results; Deactivate… plus the Active box (D56) as the model for the
  curriculum's open/close box (Q4); the kind named where names collide (M8: Paediatrics / Paediatrics).
- **Q2 counts who has adopted, never who could.** Drop the Spec's "Opening 11.2 lets ‹n› institutions adopt it". Draw
  "Adopted by 1 institution, Kgosi Kgari Teaching Hospital; 3 registrars in training on it" (Q2) where the change is
  made.
- **The steps:** part 2's 15 (1.16–1.18, 6.13, 6.14, 6.17–6.19, 6.23, 6.24, 6.28–6.31, A.7.10), its Steps board naming
  every Expect whose words change, inside the 15 and outside (M7's lesson: the menu and the card recur in 1.8 and
  6.10; T301's box word in 6.31's Do; T326's empty line in 6.13; states.md's rows). Fix the brief's ask line "part 1's
  19 Expects" to 18.
- **Re-attach the screenshots,** part 1's 45 and part 2's 25 (brief § 4), and have the Spec say first which arrived.
  Round 1 got none, and the folder request timed out. Check the canvas's version before and after the send.
- **Paste the values only the seed or a capture holds** (S7), and for part 2: the PAED-001–PAED-016 titles; each item's
  target, decision cadence, body, minimum rung, window and tools on 11.1 and 11.2 (`seed`; 6.30's Expect); the
  minimums by training year (`seed`, `stageLevels`); KGK's five profiles (three in training) at 6.17.
- **The stylesheet:** `f16r2.css` as a project file, with no shipped NEW class unless a board argues one (C2's S1–S3
  are dropped with C2).
- **Checked here, so the ask need not re-derive it:** `CreateSubSpecialityCommand` takes no default; the College
  handlers turn every `DbUpdateException` into "already exists"; the rung guard names one item, by id, and ignores
  reordering; the scales list is `AdministratorOrInstitutionalAdmin`; deactivation reaches invitations, role scopes and
  SSO mappings only; today's scale create starts with two rungs and its save stays on the page.
