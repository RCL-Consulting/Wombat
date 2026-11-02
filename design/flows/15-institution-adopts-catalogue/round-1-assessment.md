# Flow 15 round 1: assessment (T392 step 5)

Read-only, at HEAD `ff17241b`. Canvas "Wombat 15" (https://claude.ai/artifact/QvQL26tCCJh6S9zoLBywdZ), design system
copied at version 1793385917-2f3c (`canvas.json`, `designSystems`). Boards: `design/flows/15-institution-adopts-catalogue/round-1/*.dc.html`
(17, every board's text and markup read), `canvas.json` and the round's stylesheet `f15r1.css`. `W/` =
`src/Wombat.Web/Components/Pages/`, `A/` = `src/Wombat.Application/Features/`, `Nav/` = `src/Wombat.Web/Navigation/`,
`app.css` = `src/Wombat.Web/wwwroot/app.css`. **Obs** = observed in the code, the runbook or the boards. **Inf** =
inferred. Brief sections are `design/flows/15-institution-adopts-catalogue.md`'s. The `design/baseline/` captures were
not opened for this assessment: today's pages were read in the code, at the lines cited.

**Q1–Q17 are treated as taken as recommended, to confirm** (the operator has not answered them). As in flows 13 and 14,
a board that contradicts one is a must-fix unless it argues to reopen it; where it argues, the point is a question in
§ 8.

## Summary

- **Where KGK adopts: pick A1** (one Curricula page: what KGK trains against, then the versions open to it, each with
  its own Adopt; Curriculum adoptions leaves the menu).
  - **Why A1:** it is Q1 as recommended, the cheapest (first adoption 2 + 0, a newer one 3 + 0; Steps, recomputed), and
    it meets DESIGN.md's picker rule by its shape: an adopted version is a row with its state, never an option, so
    1.20's second press cannot happen.
  - **Reject A2.** Its adopt page is a PROPOSED route (`/admin/curricula/adopt`) and costs a press and a choice more for
    a rare act whose explanation A1 already carries in its section intro and its dialog.
  - **Take from A2:** the per-version line "Your institution follows no Paediatrics version yet." under a first-adoption
    row (A1 has only the "Newer than…" line). The Administrator's way in is Q18.
  - **Correct in A1:** drop the separate Adoption history (S1); Q9's rule and the count (S2); the re-pointed links
    (S4); the CollegeAdmin on a merged page (S5).
- **A curriculum's items: pick B2** (one table, KGK's row marked under its EPA, "Set by the College." on the rest; add
  and edit on a form page of their own).
  - **Why B2:** it is Q14 as recommended and the only structure that keeps DESIGN.md § Table system's T211 rule (one
    list, the own item marked under its title, a standing notice above). It ends T323's form-in-a-cell outright.
  - **Reject B1.** Two tables split by owner replace T211's rule with a NEW section pattern (N2), and its edit panel
    "under the row, sharing a 4px stripe" is today's edit row again: a form inside a table.
  - **Take from B1:** the 11.2 empty-state words ("A new version carries no institution's own items. Your item on 11.1
    stays there."); the folded cells of KGK's table ("3a (CPSA Paediatric Entrustment Scale v11.1) · window 12 months")
    as the way B2 fits 1280 (S16); the subtitle line "adopted by your institution on ‹date›" (DATA).
  - **Correct in B2:** the edit page's EPA (M6); the mark as words (M4).
- **The EPA list and an EPA's page: pick C2's list, with C1's page for KGK's own EPA.**
  - **Why C2's list:** one table matching B2 and T211; "Set by the College." in a national row's actions cell, Edit on
    KGK-001 alone (T291 item 1, C17). Q3 as recommended.
  - **Why C1's own-EPA page:** today's form, titled by the EPA, with Deactivate… and the Active box as built (T196). C2's
    record page plus a PROPOSED `/admin/epas/{Id}/edit` route and a NEW Reactivate action cost a route, a press and an
    action for a page edited a few times a year.
  - **The national EPA's read-only page** is the same in both (C1 frame 7, C2 frame 6): take it (Q2), without C1's
    "On your institution's curricula" section (DATA 7; S9).
  - **Correct in C2:** the mark as words, not a `badge-draft` badge (M4); drop the Owner column (S6); Status as plain
    words (S7).
- **The move: pick D1** (words under the picker; Save asks first only when the curriculum changed).
  - **Why D1:** it is Q8 as recommended, keeps flow 13's profile layout, and asks only when the picker moved.
  - **Reject D2.** It removes the picker, adds a NEW move action and a dirty-form rule ("save or cancel your other
    changes first"), and rewrites 6.34's and 6.36's Expects for no fewer steps that matter (4 + 0 against 4 + 1).
  - **Take from D2:** nothing structural. Its "Superseded by your institution on ‹date›" help is already D1's line.
  - **Correct in D1:** pronouns (M1); "cannot be undone" (M2); the built labels (S11); the count before the save (Q20).
- **E, the scales: confirm, corrected** to Q13's words, without the "On your curricula" column (S10).
- **F, the rebuild: confirm, corrected.** The trigger stays primary (M3). Its subtitle exists today (S12).
- **G, My progress: confirm.** Nothing changes; KGK-001 keeps T357's badge until T357 takes Q3's words.
- **The must-fixes** (§ 7):
  - **M1:** gendered pronouns in the move's words and dialog.
  - **M2:** the boards refuse going back to an older version, against Q7, so the move's "cannot be undone" is drawn as
    absolute when it is not.
  - **M3:** F's outline trigger, against Q16.
  - **M4:** the owner mark as a `badge-draft` badge, against Q3 and DESIGN.md's T211 rule.
  - **M5:** the Steps board misses the steps outside the 16 whose Expects change.
  - **M6:** B's edit page shows the item's EPA as fixed text; the command and the editor change it today.
- **The attachments did not arrive** (Spec § 1, Obs). Nothing on the boards came from a capture; values only a capture
  or the seed holds are brackets. Round 2's ask must paste them (§ 9).

## 1. What was drawn

17 boards in six rows (`canvas.json` notes t0–t5).

| Row | Boards | State drawn |
|---|---|---|
| 0 · Spec, Steps | Main, Steps, Sidebar | The variations, components (NEW marked), DESIGN.md rules changed, P1–P13, DATA 1–7, the class table N1–N8, Q1–Q17 answered, edge cases, an accessibility note; journeys counted per variation and the 16 steps' Expects |
| 1 · Curricula (wireframes) | A1, A2 | 1.19 nothing adopted; 1.20 adopted; 6.32 the dialog; 6.32/6.33 superseded; T301's refusal; the Administrator with no institution, then KGK; loading and load error; each at 390 |
| 2 · Items (wireframes) | B1, B2 | 1.21 read only; 6.22 PAED-012 paused; 6.26 the add before a rung, added; 6.33 on 11.2; editing; Remove…; not found; loading and load error; 390 |
| 3 · EPAs (fidelity) | C1, C2, C390 | 1.19 empty; 1.21 fifteen; 6.22 Inactive; 6.25 create, saved, listed; a national EPA read only; out of scope; Deactivate… on KGK-001; deactivated and reactivated; a duplicate code; loading and load error; the Administrator's 40 |
| 4 · The move (fidelity) | D1, D2, D390 | 6.34 kept on 11.1; 6.36 11.2 chosen, the dialog, the result, reopened; a stale move refused |
| 5 · One proposal each | E, F, G, EFG390 | 6.9 three scales; loading and load error; 6.38 page, dialog, running, result, failure; 6.37/6.39 My progress on 11.2 |

- **The wireframes** are grey `.wf-*` boxes, right for a wireframe.
- **The fidelity boards** link the design system and `f15r1.css`; every class they use is in the system or in the N/B/W
  rows, except `.panel-title` (StatePanel's is `.state-panel-title`,
  `src/Wombat.Web/Components/Shared/StatePanel.razor:39`) (N5).
- **Two variations or structures each,** as the ask allowed; one proposal each for E, F, G.

**Not drawn in round 1** (round 2's):
- **768 px** anywhere (§ 11, flow 06).
- **The Administrator on the items page,** KGK's row named "Kgosi Kgari Teaching Hospital's own item" (DESIGN.md T211;
  `W/Admin/Curricula/CurriculumItemsEdit.razor:1115`).
- **The CollegeAdmin on the merged Curricula** (A1): her page must show no adoption sections (S5).
- **An EPA page's own load error.** Today it is an alert above an empty body (`W/Admin/Epas/EpaEdit.razor:33`,
  `:204–207`; T329). C1 frame 12 draws the list's only.
- **An own-item add refused** from a stale picker, and an own-item save refused (B1 frame 6 says it in words only).
- **The Administrator adopting for KGK** past "Show": A1 frame 6 says it in words.
- **An adoption between 00:00 and 02:00 SAST** (T325, P13): words only; enough, since the fix is the date.

## 2. Where KGK adopts: A1, A2

Both draw, rightly (Obs):
- the empty state with no Create curriculum (T211) and Adopt as the way on;
- the result "Paediatric EPA Curriculum (11.1) adopted. New admissions to Paediatrics / Paediatrics join it.",
  focused;
- the 6.32 dialog on Q6's words, opening on Cancel, its confirm "Adopt 11.2", and naming KGK-001's item as not carried
  over;
- T301's refusal before anything is written, "This curriculum version is not open for adoption." (P4, Q5), and the
  re-read list;
- a row's state for KGK in words, "Adopted on 2026-10-30" and "Superseded on 2026-10-30 · ‹N› registrars still on it"
  (Q10), the College's Open/Held back only on the Administrator's list;
- the load error with Try again and no empty state under it (A6).

- **A1, one page.** "Curricula you train against", "Open for adoption" (a table with an Adopt per row) and "Adoption
  history". Counts (Steps, recomputed): first adoption 2 + 0, a newer one 3 + 0; at 390 one more each.
- **A2, an adopt page.** Curricula reads; "Adopt a curriculum version" opens `/admin/curricula/adopt` (PROPOSED) with
  described radios. Counts 3 + 1 and 4 + 1.

**Recommend A1.** Correct in it:
- **Drop "Adoption history" (S1).** Every row it holds repeats a row above (Adopted on, Superseded on). Q1 folds the
  history into the rows.
- **Older versions (M2).** A1 frame 3 leaves no older Paediatric version to adopt, by P5.
- **The Administrator (Q18).** Frame 6 adds "An institution's curricula" (Institution + Show) under her College-wide
  list. That refines Q1's "from Curricula's header".
- **"Adopt" and "Adopt…".** Right: "…" only where it asks (a newer version), as the board notes.

**Against the code** (Obs):
- `AdoptCurriculumCommand` accepts any version of any discipline, older ones included, and refuses only the one KGK has
  actively adopted (`A/Adoptions/AdoptCurriculum.cs:54–66`). P5's refusal is NEW.
- `GetAdoptableCurriculaQuery` lists every active curriculum, adopted or not (`A/Adoptions/GetAdoptableCurricula.cs:45`).
  A1's "Open for adoption" needs it narrowed (Q7) and widened: its rows show Effective and an item count, which
  `AdoptableCurriculumDto` does not carry (`:11–18`) (DATA, S3).
- `InstitutionCurriculumAdoption` stores `AdoptedOn` and `IsActive` only
  (`src/Wombat.Domain/Institutions/InstitutionCurriculumAdoption.cs:28–29`). "Superseded on" is DATA: derive it from
  the next adoption's `AdoptedOn` for the same institution and discipline (cheap), or add a column.
- "‹N› registrars still on it" counts `TraineeProfile.AdoptionId` (cheap). The brief's acceptance gives 3 at 6.32
  (Dlamini, Mahlangu, Ndlovu; brief § 7). Ended profiles are not "still on it" (S2).
- "15 national · 1 your own" is derivable from `CurriculumDto.Items` and each item's `IsLocal` (cheap).
- "Newer than the version you follow (11.1)" needs an order. `Curriculum.Version` is free text, so order by
  `EffectiveFrom` (`src/Wombat.Domain/Curricula/Curriculum.cs:17`) (DATA; Inf that the College never back-dates).
- Curricula's policy is `NationalCatalogueAccess`, which admits the CollegeAdmin (`W/Admin/Curricula/CurriculaList.razor:2`);
  `/admin/adoptions` is `AdministratorOrInstitutionalAdmin` (`W/Admin/Adoptions/AdoptionsList.razor:2`). The merged
  page must offer the adoption parts by that policy (S5).
- The Administrator's menu never held Curriculum adoptions (`Nav/NavItems.cs:147`), so P1's "leaves both menus" is one
  menu (`:153`).

## 3. A curriculum's items: B1, B2

Both draw, rightly (Obs):
- the h1 "Items of Paediatric EPA Curriculum (11.1)" with the trail as the way back, "Back to curricula" gone
  (`W/Admin/Curricula/CurriculumItemsEdit.razor:12–20`);
- the standing notice, verbatim as built (`:55–60`);
- the empty add state, verbatim (`:1142`), with no Add offered (the picker rule);
- PAED-012 "(inactive: not in force)" as muted words (T158, T196);
- the own-item form with KGK-001 alone, the suggested scale, Add off until a rung, "Adding is off until every minimum is
  a rung on CPSA Paediatric Entrustment Scale v11.1.";
- the Remove dialog's built words, its hyphen made an em dash (`:771`; `A/Epas/EpaOptionLabel.cs:20`), opening on Cancel;
- the result naming the item, focused.

- **B1, two tables by owner, a note panel in place.** Add 2 + 8 from the page.
- **B2, one table, a form page.** Add 2 + 8 from the page (the Steps board's 7 + 13 from Home holds for both).

**Recommend B2** (§ Summary). Correct in it:
- **The EPA on the edit page (M6).**
- **The mark as words (M4).** "Your institution's own item" under the EPA in `.muted .text-sm`, as the page writes it
  today (`:1115`) and DESIGN.md's T211 paragraph says.
- **1280 (S16).** Nine columns beside the sidebar must fit their container without scrolling (DESIGN.md § Table system,
  "The read-only table must fit its container"). Fold Scale into Minimum level and window and weight into one cell, as
  B1's KGK table does.
- **"Item not found"** for a College item's edit page typed by id: right as a page of its own; its words are Q19's.

**Against the code** (Obs):
- `CurriculumItemDto` carries `Weight`, the decision fields, `IsLocal` and `OwningInstitutionName`
  (`A/Curricula/CurriculumDto.cs:33–57`); every column B2 draws is there.
- The subtitle's "adopted by your institution on 2026-10-30" is not on `CurriculumDto` (DATA).
- P9's routes are NEW; they need `NavOwners` rows lighting Curricula and a trail Home › Curricula › Items of ‹curriculum›
  › Add your institution's own item, as B2 frame 2 draws.

## 4. The EPA list and an EPA's page: C1, C2

Both draw, rightly (Obs):
- no Create EPA and no "Create the first EPA…" before an adoption, with a link to Curricula (Q12, T291's 1.19 widening;
  today unconditional, `W/Admin/Epas/EpasList.razor:10–13`, `:17`);
- no Edit on a national EPA (T291 item 1; today on every row, `:50`, though `UpdateEpa` admits only the College for one,
  `A/Epas/UpdateEpa.cs:98–104`);
- the page titled by the EPA, "KGK-001 — Running a paediatric outreach clinic at a district hospital", crumb its code,
  "Back to EPAs" gone (today "Edit EPA", `W/Admin/Epas/EpaEdit.razor:8–14`);
- a national EPA as a record, with no form (Q2, P8), and an out-of-scope id as a page of its own (Q2; words Q19);
- "Deactivate KGK-001?" naming KGK's trainees (Q11);
- the reactivation results as built (`EpaEdit.razor:284–288`);
- a duplicate code refused under Code, and any other failure as "The EPA could not be saved. Nothing has changed…"
  with no exception's text (T272; today every `DbUpdateException` reads as a duplicate, `A/Epas/CreateEpa.cs:100–103`);
- the Administrator's Edit on every row: right, `CanAccessCollege` and `CanAccessInstitution` admit her
  (`A/Common/Extensions/ClaimsPrincipalExtensions.cs:67–72`, `:88–93`).

- **C1, two sections by owner.** KGK's own (Create EPA beside the heading), then the College's with no actions column.
- **C2, one list.** An Owner column, the mark under KGK-001's title as a `badge-draft`, "Set by the College." per row;
  every EPA a record page, KGK's with "Edit KGK-001" to a PROPOSED edit route and a NEW Reactivate action.

**Recommend C2's list with C1's own-EPA page** (§ Summary). Correct:
- **M4:** the mark in words, `.muted .text-sm` under the title, "Your institution's own EPA" (to the Administrator
  "Kgosi Kgari Teaching Hospital's own EPA").
- **S6:** drop the Owner column. To Prof Mbatha it reads "College of Paediatricians of South Africa" fifteen times
  beside a mark that already says whose the sixteenth is. The Administrator's list keeps today's College column, and the
  mark names the institution.
- **S7:** Status as the plain words it is today (`EpasList.razor:52`), not `badge-completed` / `badge-withdrawn`.
- **S8:** the Deactivate dialog keeps its built sentences.
- **S9:** drop "On your institution's curricula" (DATA 7) from the national record.

**Against the code** (Obs):
- `EpaDto` has `CollegeName` but no owner and no "may change" flag (`A/Epas/EpaDto.cs:5–16`): DATA 1 is right.
- `GetEpaById` admits any InstitutionalAdmin to every national EPA (`A/Epas/GetEpas.cs:178–186`), wider than the list
  (`:91–117`). P8 narrows it to the list's rule, as Q2 asks.
- The list looks each row's sub-speciality up with `Single` in the caller's adopted disciplines (`EpasList.razor:39–40`).
  An own EPA left in a discipline KGK no longer adopts would throw (brief § 5, Inf). Spec § 9's edge "it stays listed,
  Edit offered" is right only once the list reads the discipline from `EpaDto.SubSpecialityName`. Build note, not a
  board fix.
- **Category** is a column on C1 and C2, and today's list has none. It is on `EpaDto` (Core/Elective,
  `src/Wombat.Domain/Epas/EpaCategory.cs`), so it is cheap.

## 5. The move: D1, D2

Both draw, rightly (Obs):
- "Anele Dlamini's programme" with Export in the header and End this programme below Profile details (flow 13);
- a save that keeps the curriculum asks nothing and keeps the pin (6.34; T304);
- the move's result as built, "Trainee profile saved. 1 completion was checked against 11.2, and 1 counts towards it."
  (`A/Trainees/UpdateTraineeProfile.cs:52–62`; 6.36's Actual);
- the stale-move refusal verbatim, `MoveNotAdopted` (`:109–111`), under the picker, focused, the picker read again;
- reopened, the picker offers 11.2 alone (the pin and the active adoption).

- **D1.** Words under the picker change with the choice (one round trip on the select's change, which the board flags);
  Save becomes "Save and move…" and opens "Move Sipho Ndlovu to Paediatric EPA Curriculum (11.2)?", confirm "Move and
  save" (Q8's words). Move 4 + 1.
- **D2.** The curriculum as text; a card "Move to 11.2…" saving only the curriculum. Move 4 + 0.

**Recommend D1.** Correct:
- **M1:** pronouns.
- **M2:** "cannot be undone", "he cannot be moved back to 11.1", "leaves 11.1 for good".
- **S11:** the labels as built: "Save profile" (`W/Admin/Trainees/TraineeProfileEdit.razor:434`), "Programme start
  date", "Expected completion date" (`:415`, `:424`; not required).
- **Q20:** the completions count before the save.

## 6. One proposal each: E, F, G

- **E, scales: confirm, corrected (S10).**
  - Right: every scale (unscoped, `A/Epas/GetEpas.cs:205–212`); no Create scale, no actions column (T057, T239;
    `W/Admin/EntrustmentScales/EntrustmentScalesList.razor:13–19`, `:41–44`); 1.22's two rows, 6.9's three.
  - Wrong: the subtitle "The ladders an assessor rates against on your curricula's forms" is false of the CNSA and O-R
    scales that no KGK curriculum uses. Take Q13's words. Drop the "On your curricula" column (DATA 6), which Q13
    declined.
  - "6 (1, 2, 3a, 3b, 4, 5)": the labels are the seed's (`src/Wombat.Infrastructure/Persistence/Seeds/paediatric-epa-v11.1.json:20`).
    It is new, and harmless. Keep the count alone unless every scale's labels are short.
- **F, the rebuild: confirm, corrected.**
  - Right: the trail Home › Curricula › Curriculum progress; the dialog's built words (`W/Admin/CurriculumProgress/CurriculumProgressRebuild.razor:64–68`);
    the five figures and their labels (`:54–58`); 6.38's values (15, 14, 9, 0, 15; the T387 Actual); "Rebuilding…"
    keeping the focus; a failure with no exception's text (T272; today `:13`, `:116`).
  - **M3:** the trigger stays primary.
  - **S12:** the board says "the brief gives none" for a subtitle; today's is "Recalculate every trainee's curriculum
    targets from their completed activities." (`:9`; brief § 1's components row). Keep it, or reword it: do not add a
    second.
  - The figures are a `dl.details-list` today (`:53`); N6 is not needed (S14).
- **G, My progress: confirm.** Seventeen rows, "Once a year · 7 EPAs", PAED-016 and KGK-001 "0 of 1 in 2026", PAED-011
  "0 of 2 in 2026" (6.37, 6.39). The badge stays T357's; when T357 takes Q3's words, it changes there, not here.

## 7. Corrections

Severity: **M** must (round 2 cannot go out without it), **S** should, **N** nit.

### Wrong against the code, the runbook or a recommended question

| # | Board(s) | Correction | Sev |
|---|---|---|---|
| M1 | D1, D2, D390 | **Pronouns Wombat cannot know.** "keeps her on 11.1", "moves her", "her completions", "he cannot be moved back", "His ‹N› completions", "his progress page", "He leaves…", "until you move her". No gender is stored. Use the name, or neutral words: "Sipho Ndlovu's completions are counted again…", "Keeping it keeps Anele Dlamini on 11.1." (flow 13's S6, flow 14's M4). | M |
| M2 | A1 frame 3, A2 frame 2, Spec P5, D1/D2 dialogs and words | **Going back is refused against Q7, and the move's words depend on it.** Q7 keeps an older version adoptable under Q6's dialog, which says it supersedes the newer one; the command accepts it today (`AdoptCurriculum.cs:54–66`). P5 adds a NEW refusal unargued. Draw Q7: an older version is a row with "Adopt…", its dialog saying it supersedes 11.2. Then a move back is possible once KGK re-adopts 11.1 (`TraineeAdoptionResolver` takes the active adoption, `UpdateTraineeProfile.cs:179–181`), so "This cannot be undone" and "leaves 11.1 for good" are false. Say "Sipho Ndlovu cannot be moved back to 11.1 while your institution follows 11.2." If the operator chooses P5 when confirming Q7, the absolute words stand. | M |
| M3 | F, EFG390, Spec § 4 (Button system row) | **The rebuild's trigger made outline, against Q16.** Q16 keeps it primary: a rebuild loses nothing and is safe to repeat. DESIGN.md's outline trigger is for a destructive action. The Spec's reason ("its dialog is the decision") would make every confirmed action outline, yet D1's "Save and move…" stays primary. Keep "Rebuild progress" primary, and drop "a rebuild" from the Spec's Button system row. | M |
| M4 | C2, C390, B2, G's note, Spec § 3, § 4, § 7 (N1) | **The owner mark as a badge.** Q3 and DESIGN.md's T211 paragraph mark an own EPA or item under its title in `.muted .text-sm` words, "never colour or a badge alone". `badge-draft` means a draft (`src/Wombat.Web/Components/Shared/BadgeFor.cs:178–182`). Draw the words. My progress's badge is T357's to change to the same words. N1 goes. | M |
| M5 | Steps § 2 | **Missing steps whose Expects change.** Under A1: 1.10 and A.5.2 (the menu's "Curriculum adoptions", `act-1-setup.md:286–291`, `appendix-cross-cutting.md:997`); 1.10 and 2.44 (Home's "Open Curriculum adoptions", `act-1-setup.md:303`, `act-2-onboarding.md:1182`, `:1195`); A.7.9 (the 44 px menu rows naming it, `appendix-cross-cutting.md:1798`). Under B2: A.7.10 ("the items table scroll[s] inside [its] container… the item editor's inputs keep usable widths", `appendix-cross-cutting.md:1834`). Under Q5: T301's 6.13, 6.29 and 6.31 (flow 16's, brief § 7). List each with its new Expect, or "unchanged". | M |
| M6 | B1 frame 6, B2 frame 4 | **"EPA as text (an item's EPA does not change)" is false.** The edit row has an EPA picker (`CurriculumItemsEdit.razor:194`), and `UpdateCurriculumItemCommand` takes `EpaId` (`:734`). Draw the EPA select on the edit page, offering the item's EPA and the caller's own EPAs not on the curriculum (the picker rule), or mark a fixed EPA PROPOSED. T381 (a changed item EPA after an end) stays its own. | M |
| S1 | A1 frames 1–4, 6 | **Adoption history repeats the rows.** Q1 folds the history into the rows (Adopted on, Superseded on). Drop the section; a version that leaves Curricula under Q9 has no registrar and no item of KGK's. | S |
| S2 | A1 frame 4, Spec P6, § 9 | **Q9's rule and the count.** P6 says "while any registrar of KGK is on it". Q9 says an active **or ended** profile pinned to it, or an item of KGK's. Dr Molefe's ended profile keeps 11.1 readable. The row's "‹N› registrars still on it" counts active profiles only: 3 at 6.32 and 6.33, 2 after 6.36. | S |
| S3 | Spec § 6 (DATA) | **DATA missing from the table:** an open version's Effective and item count (`AdoptableCurriculumDto`, `GetAdoptableCurricula.cs:11–18`); a curriculum's Adopted on, on Curricula and the items subtitle; "Superseded on", which nothing stores (derive it from the next adoption, § 2); the order behind "Newer than…" (`EffectiveFrom`); the owning institution's name on `EpaDto`; KGK's own items on the superseded version, for the dialog's "KGK-001 is not carried over". | S |
| S4 | A1 (not drawn) | **The links a merge re-points.** `OnboardingLinks.CurriculumAdoptions` (Invite a person's nothing-adopted line, `W/Admin/Invitations/InviteAPerson.razor:187`; Admit, `W/Admin/Trainees/AdmissionText.cs:186`); the profile's nothing-adopted words (`TraineeProfileEdit.razor:442`); Curricula's own empty body (`CurriculaList.razor:91`); Home's "Open Curriculum adoptions" (`W/Dashboards/InstitutionalAdminHomeWords.cs:17`). Name each one's new words and link ("Adopt one on Curricula"). | S |
| S5 | A1 frame 6, Spec P1 | **Who sees the adoption parts.** Curricula admits the CollegeAdmin; adopting does not. Offer "Open for adoption", Adopt and the institution section by `AdministratorOrInstitutionalAdmin`, and draw the CollegeAdmin's Curricula unchanged. P1: only the InstitutionalAdmin's menu holds Curriculum adoptions (`Nav/NavItems.cs:147`, `:153`). | S |
| S6 | C2 frames 2–4, 8; C390 | **The Owner column repeats the mark.** Drop it (§ 4). | S |
| S7 | C1, C2, C390 | **EPA Status as badges.** `badge-completed` "Active" and `badge-withdrawn` "Inactive". A running state's "Active" is a plain word (`BadgeFor.cs:178–182`), and `badge-withdrawn` means a programme or request withdrawn. Keep the plain words as built. | S |
| S8 | C1 frame 9, C2 frame 7, C390 | **Q11's dialog drops built facts.** Today's body also says "The progress it has earned is kept. An activity completed against it while it is inactive counts towards nothing until it is reactivated." (`EpaEdit.razor:112`; D48). Keep both sentences; change only the title and whose trainees, as Q11 says. | S |
| S9 | C1 frame 7 | **"On your institution's curricula"** (DATA 7): drop it. Q2 lists the record's fields and this is not one; the items page is one press from Curricula. | S |
| S10 | E, EFG390 | **Q13's words and no curricula column** (§ 6). | S |
| S11 | D1, D390 | **The profile's built labels** (§ 5). "Save and move…" is "Save profile and move…", or keep "Save profile" with "…" added (Q8 does not name it; either is PROPOSED). | S |
| S12 | F; all boards | **Values the brief, the runbook or the code give, left as brackets:** the rebuild's subtitle (`CurriculumProgressRebuild.razor:9`); the menu groups ("Curriculum" for the InstitutionalAdmin, "Catalogue" for the Administrator, `Nav/NavItems.cs:147`, `:153`); ‹N› registrars = 3 (brief § 7); ‹N› completions = 1 (6.36's Actual), if Q20 keeps it; the PAED titles and categories (§ 9). | S |
| S13 | C390, D390, EFG390 | **Inline 390 styles that fight the system.** `min-height: 2.75rem; width: 100%` on buttons repeats the system's 44 px rules (`.clinic-table--stack-wide .btn`, `.user-page .btn`, `app.css:6594–6602`); `margin-top` inline. Drop them; the build must not copy them. | S |
| S14 | Spec § 7, `f15r1.css` | **N1–N8 duplicate what exists** (§ 7, Design system). Round 2 needs no NEW class. | S |
| S15 | (not drawn) | **768 px; the Administrator on the items page; an EPA page's own load error; a refused own-item add or save; the CollegeAdmin's Curricula** (§ 1). | S |
| S16 | B2 frames 1, 3 | **Nine columns at 1280** must fit beside the sidebar; fold cells as B1's KGK table does (§ 3). | S |
| N1 | Steps, B2 intro | B2's intro counts "back on the list" as a press (3 + 8); the Steps board does not. Use one rule. | N |
| N2 | A1, A2, C1, C2, Spec P7 | "follows" and "has adopted" for one thing. The pages say "adopted"; Q12's empty state and the refusal should too ("Your institution has not adopted Paediatrics / Paediatrics, so…"). | N |
| N3 | C1, C2, C390 (1.19) | The empty state's words differ from Q12's (both PROPOSED). Pick one; Q12's says why there is nothing to create. | N |
| N4 | C1 frames 5, 10 | "EPA saved. KGK-001 is Active. Add it to a curriculum on that curriculum's items page." and "EPA deactivated. Your institution's trainees can no longer choose it…" are new words, unmarked. Today: "EPA saved." and "EPA deactivated." (`EpaEdit.razor:269`, `:337`). Mark them PROPOSED. | N |
| N5 | C1, C2, C390 | `.panel-title` is not the system's: StatePanel's empty title is `.state-panel-title`. (`.text-muted` is defined, an alias of `.muted`, `app.css:3248–3251`.) | N |
| N6 | C1 frame 8, B1 frame 8 | "There is no EPA 1…", "no curriculum 1…" show an id (BRIEF § 6 A8). Q19's words carry none. | N |
| N7 | A1 frame 6 | The Administrator's Curricula subtitle changes to "Every College's curricula; and, for one institution, what it trains against and may adopt." Unmarked as new words (T326's kind). | N |

### Invented people or values

- **No invented person** (Obs). Every name, institution, curriculum, EPA code and date is the cast's or the brief's.
- **Right against the runbook:** D = 2026-10-30, both adoptions dated it; 11.1 effective 2026-01-01 and 11.2 2026-10-30
  (brief § 1); 15 then 16 national items; KGK-001's title, description and Elective; 6.26's item; 6.36's result
  sentence; 6.38's five figures; 6.37/6.39's rows and counts.
- **Bracketed, the brief gives the volume only:** the Administrator's 40 EPAs, the second institution and the
  150-character title (brief § 1:211, :274). Right to bracket.

### PROPOSED items, with a recommendation each

| Item | Recommend |
|---|---|
| P1 `/admin/adoptions` redirects to Curricula; the menu item goes | Accept (Q1), one menu (S5) |
| P2 `/admin/curricula/adopt` | Drop (A2 not picked) |
| P3 the adoption's dialog when it supersedes | Accept (Q6) |
| P4 T301's refusal, "Open" / "Held back", a new version held back | Accept (Q5) |
| P5 no going back, and its refusal | Drop (Q7; M2) |
| P6 a superseded version readable while registrars are on it | Accept, with Q9's "active or ended" (S2) |
| P7 Create EPA only with an adopted discipline; the commands' refusal | Accept (Q12), "adopted" words (N2) |
| P8 a national EPA read-only; out of scope as a page of its own | Accept (Q2); words Q19 |
| P9 `…/items/new`, `…/items/{ItemId}` | Accept (Q14, B2) |
| P10 the move's dialog | Accept for D1 (Q8); D2's separate action drop |
| P11 T242's refusal naming KGK | Accept (Q15; flow 16 draws it) |
| P12 no single-trainee rebuild | Accept (Q4) |
| P13 an adoption dated by the South African day | Accept (T325) |
| C2's `/admin/epas/{Id}/edit` and its Reactivate action (unmarked) | Drop (C1's own-EPA page) |
| D1's words changing on the select's change (one round trip) | Accept |

### DATA items, with a recommendation each

| Page | Item | Recommend |
|---|---|---|
| EPAs | DATA 1: the owner and the caller's "may change" flag | Accept, from `UpdateEpa`'s rule (`UpdateEpa.cs:98–104`), never roles |
| Curricula | DATA 2: Adopted on, Superseded on, registrars pinned, own item held | Accept; Superseded on derived (S3) |
| Curricula | An open version's Effective and item count | Accept (S3) |
| Curricula, EPAs | DATA 3: "does this institution actively adopt any discipline" | Accept |
| Adopt dialog | DATA 4: the version superseded, its registrar count, KGK's own items on it | Accept |
| Profile | DATA 5: the trainee's completion count before the save | Drop (Q20) |
| Scales | DATA 6: which of KGK's curricula pin each scale | Drop (Q13; S10) |
| EPA record | DATA 7: KGK's curricula holding the EPA | Drop (S9) |
| Items | "adopted by your institution on ‹date›" | Accept, with DATA 2 |

### Access control

| # | Board(s) | Finding | Sev |
|---|---|---|---|
| A1 | C1/C2 frame 8 | **The sweep holds once P8 lands.** Today the Demo EPA-001 opens as a full form with Save and Deactivate (`GetEpas.cs:178–186`; `EpaEdit.razor:94–100`; `states.md:775`). P8 makes the read the list's rule; a page of its own answers unknown and foreign ids alike (§ 4.4's invariant). | — |
| A2 | A1 frame 6 | **The Administrator adopts for any institution** (`AdoptCurriculum.cs:42–45`: `CanAccessInstitution`). The dialog must name the institution, as the board says. | — |
| A3 | A1 | **The merged page's adoption parts** go by `AdministratorOrInstitutionalAdmin`, not Curricula's policy (S5). | S5 |
| A4 | Spec P7 | **The commands' rule** (`CreateEpa.cs:68–80`, `UpdateEpa.cs:116–121` accept an own EPA in any discipline) is the T291 half Q12 folds in; the picker already offers only adopted disciplines. | — |

### Accessibility and the 390 boards

- **Good (Obs):**
  - one h1 per page from PageHeader, `<h2>` sections (today Adoptions' and the items page's `<h3>` with no `<h2>`,
    `AdoptionsList.razor:44`, `:63`; `CurriculumItemsEdit.razor:76`, `:329`);
  - every table captioned; row actions named for the row ("Adopt Paediatric EPA Curriculum (11.2)…", "Edit KGK-001 —
    Running…", T239);
  - dialogs open on Cancel and return the focus to their trigger; results focused; a refusal under its field;
  - nothing offered before a read is in (A6);
  - status and owner in words (once M4 lands);
  - row actions 28 px at 1280 (≥ 24 px, A4), 44 px at 390.
- **S13:** the inline 390 styles.
- **Round 2 must also say:**
  - the dialog's scroll at 390 with A1's three-paragraph body;
  - the radio rows' 44 px on the item form's "Minimum level" at 390 (the people pages' `.choice-group` rule does not
    reach the items page: name the scope);
  - 768 for Curricula, the items page and the profile.

### DESIGN.md and flows 01–14's patterns

- **The Spec names the rules each variation changes:** § Table system's T211 paragraph, the picker rule, § Button
  system, § Form system's refusals, § Page-level patterns, the NavMenu owner table. Round 2 must also name:
  - § Page-level patterns' Unavailable list, gaining the EPA's and the curriculum's pages (Q19);
  - § The NavMenu's table, losing Curriculum adoptions (`NavMenuAuthorizationTests` parses it);
  - the reuse of flow 12's `.user-page` and `.user-section` for the EPA record (N4), as flow 13's S8 and flow 14 did.
- **The class table (S14), Obs:**
  - **N1** `.owner-line`: drop (M4; the mark is `.muted .text-sm`, `app.css:3248`, `:3253`).
  - **N2** `.owner-section-note`: drop (B1 and C1 not picked; it is `.muted` text).
  - **N3** `.section-head`: is `.form-section-head` (`app.css:1481–1492`: baseline flex, wrap, space-between, h2
    margin 0).
  - **N4** `.epa-record`: is `.user-page` and `.user-section` (`app.css:6515–6550`: a column, h2 at 1.25rem, margin 0).
  - **N5** `.move-check`: is `.submit-check` (`app.css:1629–1635`), rule for rule (info tint, the action-blue 4px
    stripe, `--radius-md`). Reuse it under the picker.
  - **N6** `.result-figures`: is the built `dl.details-list` (`CurriculumProgressRebuild.razor:53`; `app.css:1956`).
  - **N7** `.prose-block`: the built `.form-container` paragraphs (`CurriculumProgressRebuild.razor:15–35`).
  - **N8** `.page-actions-stack`: is `.form-actions` stacking `column-reverse` at 390 under `.user-page` and
    `.dialog-card` (`app.css:6272–6274`; flow 12).
- **Tokens:** `f15r1.css` uses tokens throughout (Obs: no hex in N1–N8); `12px` paddings and gaps are literal where
  `--space-sm`/`--space-md` exist (N, folded into S14).
- **No Bootstrap classes** (Obs); `.text-muted` is the system's own alias of `.muted`.
- **Wording (BRIEF § 6 A8, A10; § 11):** no ids on pages (N6); one word for one thing (N2); copy for the viewer (E's
  subtitle, S10); no pronouns (M1).

## 8. The operator's decisions still needed

Q1–Q17 first (brief § 6), each taken here as recommended. Q7 decides M2; Q16 decides M3. The four below are new; each
has a recommendation.

18. **Q18 (refines Q1). Where the Administrator adopts for an institution.** Q1 said "from Curricula's header, as
    Curriculum progress is reached", which implies a page of its own. A1 draws a section on her Curricula, "An
    institution's curricula" (Institution + Show), opening the same rows and dialog as Prof Mbatha's, named for the
    institution; A2 draws a header link to the adopt page. *Recommend:* A1's section. No new route, no menu item, the
    same rows and dialog as the institution's own, reached from her Catalogue › Curricula. Flow 18 may move it to the
    institution's page.
19. **Q19 (reopens Q2's words). The pages of their own for an id she cannot open.** Q2 said "EPA not found" / "There is
    no EPA with this id that you can open."; the boards draw "EPA not found", "Curriculum not found" and "Item not
    found", with the id. DESIGN.md's Unavailable pattern, which flows 12–14 took (flow 14's Q17), is one page for both
    causes, never saying which and never showing the id. *Recommend:* "EPA unavailable" / "This EPA does not exist, or
    you cannot open it." / Go to EPAs; "Curriculum unavailable" / "This curriculum does not exist, or you cannot open
    it." / Go to Curricula; "Item unavailable" / "This curriculum item does not exist, or you cannot change it." / Go to
    ‹the curriculum's items›. Each drawn in its page's place, lighting its menu item. `states.md:775` and `:800` change.
20. **Q20 (refines Q8). A count before the move.** Q8's words say "his ‹N› completions are counted again" before Save
    (DATA 5). The result already gives the replay's count ("1 completion was checked against 11.2, and 1 counts towards
    it."). A count read before the save must apply the replay's own rule (which completions it re-judges), or the
    dialog and the result disagree (Inf). *Recommend:* no number before the save: "Sipho Ndlovu's completed activities
    are counted again against 11.2 as the profile saves." The result keeps its count. Q6's registrar count stays: it is
    one exact query, and it is what the adoption decides.
21. **Q21 (new; the boards' P5 against Q7). Going back to an older version.** Asked only if the operator wants P5 when
    confirming Q7. P5 refuses adopting a version older than the one KGK follows. Then a move is final, and the profile's
    words can say "cannot be undone". Q7 keeps it adoptable, so a mistaken adoption or move can be reversed, at the
    price of conditional words (M2). *Recommend:* Q7 as recommended (no P5). Adoption is rare; a College may withdraw a
    version; the reverse path costs nothing to build since the command allows it today.

## 9. What round 2's ask must require

- **The picks:** A1, B2, C2's list with C1's own-EPA page and the shared national record, D1; E, F, G confirmed; every
  state of each, at 1280, 768 and 390, with the Spec and Steps boards.
- **Every must-fix** M1–M6, and the should-fixes S1–S16, by number.
- **The operator's answers** to Q1–Q21, drawn as decided.
- **Paste the values only a capture or the seed holds**, since round 1 received no capture (Spec § 1):
  - the PAED-001–PAED-016 titles and categories (`src/Wombat.Infrastructure/Persistence/Seeds/paediatric-epa-v11.1.json`;
    PAED-016 from 6.28);
  - each item's target, decision cadence, decision body, minimum rung, window and tools on 11.1 and 11.2 (Step 1.18's
    and 6.30's Expects);
  - the scales' descriptions and the CNSA and O-R rung labels;
  - ‹N› = 3 registrars on 11.1 at 6.32 (Dlamini, Mahlangu, Ndlovu), 2 after 6.36;
  - the Demo IM Core curriculum's effective date and item count;
  - the menu groups ("Curriculum", "Catalogue") and their items in order (`Nav/NavItems.cs:147`, `:153`);
  - Ndlovu's profile fields as flow 13 built them (`TraineeProfileEdit.razor:380–436`).
- **Re-attach § 4's twelve key screenshots,** and ask the Spec to say first whether they arrived (§ 11, flows 13 and
  14). Check the canvas's version before and after the send.
- **Save the stylesheet as a project file** (§ 11, flow 14); round 1 did (`f15r1.css`). Ask it to drop N1–N8 for the
  existing classes (S14).
- **Checked here,** so the ask need not re-derive it: `AdoptCurriculum` accepts older versions; `UpdateCurriculumItem`
  changes an item's EPA; nothing stores Superseded on; `EpaDto` has no owner; the Administrator may edit every EPA.

## 10. The operator's decisions (2026-11-02)

The operator took Q1–Q17 (the brief's § 6) and Q18–Q21 (§ 8) as recommended, with the picks of § Summary:

- **A1**, one Curricula page, with A2's per-version line for a first adoption ("Your institution has adopted no
  Paediatrics / Paediatrics version yet.", in N2's word); Adoption history dropped (S1).
- **B2**, one items table and a form page of its own for KGK's item, with B1's borrowings: the 11.2 empty line, the
  folded cells (S16), the subtitle's "adopted by your institution on ‹date›" (DATA).
- **C2's list** (without its Owner column and badge, S6, M4) **with C1's page for KGK's own EPA** (today's form, titled
  by the EPA, Deactivate… and the Active box as built) and the shared national record (without "On your institution's
  curricula", S9).
- **D1**, the move asked on Save, with M1, M2, S11 and Q20 applied.
- **E, F and G confirmed** with their corrections (S10, Q13's words; M3 and S12; G unchanged).
- **Q18:** the Administrator adopts from an "An institution's curricula" section on her Curricula (Institution + Show,
  then the same rows and dialog as Prof Mbatha's, named for the institution).
- **Q19:** each page of its own for an id the caller cannot open is DESIGN.md's Unavailable pattern and shows no id:
  "EPA unavailable", "Curriculum unavailable", "Item unavailable", with § 8's sentences and ways on.
- **Q20:** no completions count before the move; the result gives it.
- **Q21:** an older version stays adoptable (Q7 kept, P5 dropped), so the move's words are M2's conditional ones.

Every M and S item in § 7 applies, and the nits in one pass. One correction to § 7's accessibility list, found while
writing the ask (Obs): the item form's Minimum level is `RungPicker`, a select on a pinned scale (a number input when
unpinned), not radios (`W/Admin/Curricula/RungPicker.razor:9`, `:26`); the 390 px rows that need 44 px
there are the Tools checkboxes (`.check-grid`) and Opportunistic. Round 2's ask is `round-2-ask.txt`; § 4's twelve key
screenshots are re-attached with it, from `design/upload/baseline/`.
