# Flow 15 round 2: the review

Round 2 (the boards saved in `round-2/`: 168 boards, the 75 page boards at 1280 with their 75 twins at 390 and 15 at 768,
plus `R2-0-spec`, `-spec-2` and `R2-0-steps`; the NEW rule in `round-2/f15r2.css`) drew Curricula and its adoption (A),
a curriculum's items and their form pages (B), the EPA list and an EPA's page (C), the move (D), the scales (E), the
rebuild (F) and My progress (G), with 16 Illustrations marked. Reviewed 2026-11-02 from three sides, each a Sonnet agent:
**the code** (`round-2-review-code.md`, K1-K16 and N1-N8: `src/`, the ask, Q1-Q21, M1-M6, S1-S16, T1-T12), **the cast**
(`round-2-review-cast.md`, C1-C16: the runbook, FIXTURES, `states.md`, `coverage.md`; checklist 72 of 74 states done,
corrections 20 of 22 done) and **the design system** (`round-2-review-ds.md`, M1, S1-S8, N1-N7: `app.css`, DESIGN.md,
the built components, `f15r2.css`, a static pass). The three source files are kept as they are; this file merges them.
The integrator checked every must-fix and every finding promoted here against the code, the runbook or the boards
(`file:line` below) before carrying it; what was corrected is said where it was. Severity: M must-fix, S should-fix, N
nit. Each item says what round 3 does with it: **redraw** (a board changes), **Spec** (the Spec board says it),
**Steps** (the Steps board changes) or **build** (a code change the Spec already implies or must list; no board changes).
Round 1's M1-M6 and S1-S16 are fixed on the boards but for M5 and S12, done in part (M4 and S1 below); N1-N7 are done.

**What holds, by all three:** every person, date, count and value against FIXTURES, the seed
(`paediatric-epa-v11.1.json`, `PaediatricCatalogueSeeder.cs:941`, `DataSeeder.cs:69-75`) and the runbook's T387 Actual
lines (6.31, 6.33, 6.34, 6.36-6.39, the rebuild's 15 / 14 / 9 / 0 / 15), but for A11 (M2); B01's fifteen rows cell by
cell; the menus and the lit item on every board; no gendered pronoun or title in any board, the Spec or the Steps; no id
on an Unavailable page (Q19); the move's words conditional (M2, Q21) with no count before it (Q20); the rebuild's
trigger primary with no "…" (Q16); the owner mark in `.muted .text-sm` words on every admin board (M4); the built words
quoted as built, verbatim, but for the em dash (S10); Spec § 2's citations; the 90 twins carry their base board's text
and attributes; one h1 a board, no inline `style=`, no raw colour, every class in `app.css` or `f15r2.css`'s one NEW
rule (`.curriculum-item-page`, needed: ds N1), every table captioned, scoped and labelled, every `aria-describedby`
target present, refusals first; the contrast figures recomputed and equal to the Spec's. No board draws a rule the code
contradicts on going back, the one-active-adoption index, the move's pin or the picker (`AdoptCurriculum.cs:63-66`,
`InstitutionCurriculumAdoptionConfiguration.cs:17-18`, `TraineeCurriculumChoices.cs:49-55`).

## Must fix

| # | Side | Finding | Fix |
|---|---|---|---|
| M1 | ds (M1) | **Every row-action cell is a `td.actions-cell`** (242 cells across A01-A13, B06, B08, B10, B11, B13, B17, B20, C09, C15 and twins; only 38 cells, on A09-A12 and C02, draw the `div`). Checked: DESIGN.md:927-928 (T266, the ds review cites :919-922) puts a cell's buttons in a `div.actions-cell` inside the `<td>`, never `td.actions-cell`, because `app.css:442-446` makes it a flex box and a flex `td` stops being a table cell; `RowActionMarkupTests.NoTableCell_IsItselfTheActionsCluster` (`:35`, regex `:185`) fails any page built from these boards. Flow 14 drew the `div`. **Redraw** | `<td><div class="actions-cell">…</div></td>` on every row-action cell, 1280 and twins |
| M2 | code (K15), cast (C1) | **A11 draws two states at once.** Checked: its label is "Demo Administrator adopting 11.2 for Kgosi Kgari Teaching Hospital at 6.31's state", but its "All curricula" table reads 11.2 at **17** items, the count after KGK-001 is added to 11.2 at 6.33 (act-6-catalogue.md 6.31 "11.2 Active (16 items)", 6.33's Do); the same board's KGK section has no own item on 11.2. The ask: never two states on one board. **Redraw** | 11.2 "16" on A11 and its twin (11.1 stays 16: 15 national and KGK-001) |
| M3 | cast (C2, C4), code (K11) | **T4 sends the College admin and the Administrator to the item form pages, and nothing draws or words them.** Checked: the Steps' 6.30 row rewrites only the edit ("Edit PAED-011 opens its page"); the add half, Dr Kruger adding PAED-016 (act-6-catalogue.md:748, :755: "The form is headed 'Add item', its EPA picker offers PAED-016 alone … the confirmation heads a note that every national EPA … is already on the curriculum"), has no row and no board. For those callers the command writes a **national** item (`CurriculumItemEpas.cs:67-70`), the built heading is "Add item" and the empty line the national one (`CurriculumItemsEdit.razor:329`, `:1137-1142`), so B04's h1 "Add your institution's own item" and "It applies only to Kgosi Kgari Teaching Hospital's trainees" are false for them. A national item's edit page is drawn nowhere either (B09 and B12 are KGK-001's, "Your institution's own item on …"), yet A.7.10's and 6.30's new Expects need it, and A.7.10's Do never presses Edit PAED-001 | F3. **Spec**: the add and edit pages' h1, subtitle and empty line by `CanEditCurriculum`. **Redraw**: one add page for Dr Kruger at 6.30 (picker PAED-016 alone, the result and the built note) and one national item's edit page (PAED-011). **Steps**: 6.30's add half; A.7.10's Do presses Edit PAED-001 |
| M4 | cast (C3) | **Step 1.17 is not on the Steps board.** Checked: act-1-setup.md:428-433 has Dr Kruger read his Curricula row "Active" (Expect and Actual); T9 makes the College's Status "Open" / "Held back" (A12), so the replay would fail 1.17 as written. M5 of round 1 asked for every such step. **Steps** | Add 1.17 ("Open" for "Active"); with it the Act 6 Outcome table's Status column (act-6-catalogue.md:1019-1023, "Active" / "Inactive") and "11.2 Active and 11.1 Superseded" (:1032), S22 |
| M5 | code (K1) | **"EPA saved." on a created EPA's page has no source.** Checked: a create runs `Navigation.NavigateTo($"/admin/epas/{created.Id}", forceLoad: true)` and carries nothing (`EpaEdit.razor:257`); the result is set only on the update branch (`:261-272`); the runbook's 6.25 Actual records the page with no result (act-6-catalogue.md:627-628). C08's "Focus: EPA saved." and the Steps' 6.25 row rely on it; the Spec's EpaEdit row does not list it. **Spec + build** | F1: the hand-over on the Spec's EpaEdit row, or C08 and 6.25 lose the result |
| M6 | code (K2) | **The item form pages return to the list with a result and a focus, and nothing says how either crosses the page change.** Checked: "Curriculum item added." / "saved." and the result's focus are fields of the one component that adds and edits (`CurriculumItemsEdit.razor:625`, `:661`, `:734`); Cancel never leaves it. With Q14's routes, B06, B10 and B03 need the list to show a sentence another page wrote and focus it, and B11 (Cancel) the row's Edit. T373 built the shape (`OnboardingLinks.Admitted`, `BackFromAdmit`, `OnboardingLinks.cs:46-61`); nothing like it exists here. **Spec + build** | F1: the tokens and their links class on the Spec's item-routes row |

## Should fix

- **S1 The supersession rules and A08's dialog** (K3, C11; A02-A13, A08). **Spec + redraw.** Both adoptions are dated D
  (1.20 and 6.32), so "the next adoption's AdoptedOn" is found by order (`AdoptedOn`, then `Id`); going back (T1) adds a
  second row for 11.1, so "Adopted on" is the active row's and "Superseded on" the latest inactive row's successor's;
  count registrars by `InstitutionId` + `CurriculumId` + `IsActive` on `TraineeProfile`, not by the nullable `AdoptionId`
  (`TraineeProfile.cs:13-26,41`), which is also Q9's arm. A08 then draws the count it gives, "The registrar on 11.2 stays
  on it …" (1 after 6.36), not "‹N›"; and the own-items line is decided, not bracketed: KGK-001 already sits on 11.1, so
  the sentence is left out when the other version already holds every own item (checked: A08 prints both brackets).
- **S2 Newer and Older have no tie rule** (K4; A03, A05, A08). **Spec.** `GetAdoptableCurricula.cs:45-50` orders by
  `EffectiveFrom` and a clone takes any date (6.29 types D). F2.
- **S3 The Administrator's "An institution's curricula" needs an institution on three reads** (K5; A09-A11). **Spec +
  build.** `GetCurriculaListQuery`, `GetAdoptableCurriculaQuery` and the adoption read are caller-scoped
  (`GetCurricula.cs:12,28-41`, `GetAdoptableCurricula.cs:26,39`); name the `InstitutionId` parameter (null for an
  Institutional admin, required for the Administrator) and count national items only on the open rows.
- **S4 The Create EPA flag for the Administrator** (K6; C15). **Spec.** The Spec's Q12 flag names the Institutional admin
  and the College admin; the Administrator has neither claim and creates national EPAs (`CreateEpa.cs:66-80`). Say: true
  for the Administrator and a College admin, for an Institutional admin only with an actively adopted discipline.
- **S5 UpdateEpa's new refusal contradicts the Spec's edge case** (K7; Spec § 12, C09). **Spec.** The edge case offers
  Edit on an own EPA in a discipline no longer adopted, which every save would refuse, while `DeactivateEpaCommand` has
  no such check. Data-only today (no path un-adopts a discipline, `AdoptCurriculum.cs:69`). F9.
- **S6 Refusals under their field cannot be routed** (K8; C05, C07, B05, B12, D06). **Spec + build.** The commands throw
  `InvalidOperationException` and the pages print one alert (`EpaEdit.razor:275`, `CurriculumItemsEdit.razor:625,734`,
  `TraineeProfileEdit.razor:496-506`); the Spec lists only a DESIGN.md change. F4.
- **S7 The load errors: four pages print the exception and offer no Try again, and five boards draw a bare alert** (K9,
  ds S3; A15, B19, C17, C18b, E04). **Redraw + build.** `CurriculaList.razor:104`, `EpasList.razor:77`,
  `EntrustmentScalesList.razor:94`, `CurriculumItemsEdit.razor:590` set `exception.Message` and pass no `OnRetry`
  (`StatePanel.razor:83-92` supports it); the boards have no `div.action-result` for the answer's focus. One Spec row
  (fixed words, `OnRetry`, `FocusAfterRetry` to the h1) and the five boards drawn as `StatePanel` renders them.
- **S8 T301's second half: a new or cloned version starts held back** (K10; Steps 6.13, 6.29-6.31; A07, A09, A12).
  **Spec + Steps + build.** Checked: `Curriculum.IsActive` defaults to `true` and the clone sets it (`Curriculum.cs:19,35`);
  T301 item 2 changes both. Q5 folded it in but § 2 lists only the refusal. F5.
- **S9 The rebuild's failure focus** (K12; F05). **Build.** T8 decided the focus goes to the alert; built, a failure is
  an `Alert` that "leaves the focus on the button" (`CurriculumProgressRebuild.razor:11-14,46-48`), only success focuses
  (`:112`). The Spec row says so (the alert inside the `ActionResult`).
- **S10 The EPA label is a hyphen in the page, an em dash on the boards** (K13; B01-B20, B13, C09). **Build.** Checked:
  `CurriculumItemsEdit.razor:111` (`@item.EpaCode - @item.EpaTitle`) and the Remove body `:769`; `EpaOptionLabel.cs:20`
  has the em dash. The cast review's "the Remove body with its em dash" kept verbatim, and T3's "body as built with the
  em dash", are wrong about the built text: list the change (table cell, Remove body, legend) on the Spec.
- **S11 "Deactivate…" is a rename** (K14; C08, C12-C14, C19, T12). **Spec + Steps.** Built `Deactivate` (`EpaEdit.razor:96`);
  6.14, 6.17 and 6.23's Do say "press Deactivate". List it with T3's Remove…, and change the Do lines.
- **S12 The items page of a superseded version** (K16; A05-A13's 11.1 Items link). **Redraw.** No B board shows 11.1
  after 6.32 (B01-B06 are before it, B07-B09 are 11.2); its subtitle would say "adopted by your institution on
  2026-10-30" and offer Add. F6; one board.
- **S13 The dialogs are not ConfirmDialog's action mode** (ds S1; A03, A08, A11, B13, C12, D03, F02 and twins). **Redraw.**
  Checked on A03: `<dialog aria-labelledby="dlg-t">` with `.dialog-form` and no `aria-describedby`. Built: `.form-container`,
  the body `p#BodyId` or `div#BodyId.dialog-body`, `aria-describedby=BodyId`, so a screen reader reads the body on open.
- **S14 C15's pager is hand-drawn** (ds S2). **Redraw.** "Previous · Page 1 of ‹n› · Next" with no page-size select;
  draw `PagerControls` (`.pager-info`, `.pager-actions`, the labelled select) with the Spec's 40 EPAs.
- **S15 Two links with one name** (ds S4; A10, A11 and twins). **Redraw.** Checked: "Items of Paediatric EPA Curriculum
  (11.1)" and "(11.2)" each twice on A10, once per table (T239). Name the second table's links for the institution.
- **S16 The items table's width at 1280 and 901-1100 is asserted, not shown** (ds S5; B01-B20). **Redraw (measure).**
  Seven columns on the default padding; DESIGN.md:1038-1062 measured the old table with 5 px to spare at 1280 compact.
  F7.
- **S17 DESIGN.md § 5 misses the text T323 makes false** (ds S6). **Spec.** "Editing a row in place" (DESIGN.md:896 ff),
  the edit row in the row-actions and picker paragraphs, and "Curriculum adoptions" at :3985 and :4441 (and the NavMenu
  table, :587); "the picker rule" names no section. The Form page template's `ValidationSummary` (:3959): scope the
  amendment to the EPA, item and profile pages.
- **S18 "Words, never a badge" against DESIGN.md:3776-3777 and G01** (ds S7). **Spec.** The EPA page's h1 carries an own
  EPA's badge (`PageHeader.TitleContent`), and G01 draws "Kgosi Kgari Teaching Hospital's own" as `.badge-draft`. F8.
- **S19 The loading boards have no status role** (ds S8; A14, B18, C16, C18a, E03). **Redraw.** `<p class="loading-line">`
  without `role="status"` (built: `AssessorsList.razor:34`); A14 repeats "Loading." under two h2s.
- **S20 6.33's row reads the list in the wrong order** (C5). **Steps.** The Do reads the list before adding KGK-001
  (act-6-catalogue.md:793-801), so the first read is A05's "16 national · 0 your own", then A06a's "1 your own".
- **S21 1.20's Expect checks a typed address the Do never types** (C6). **Steps.** Put `/admin/adoptions` in the Do, or
  move the check to A.5.x.
- **S22 The Steps' word search missed hits** (C7). **Steps.** states.md:143, :765-776, :797-798, :800, :840-847, :901, :981;
  coverage.md:104, :198, :323-324; act-6-catalogue.md:1019-1023, :1032 (M4); act-1-setup.md:285, :302-303; and the Gaps
  that close (F-1.19a, F-1.21a, F-1.22a, F-6.22a, F-6.25a and b, F-A.7.10a).
- **S23 6.25's row quotes words the runbook does not hold** (C8). **Steps.** "Edit EPA", "Back to EPAs": quote the Actual
  (act-6-catalogue.md:627-630: "/admin/epas/17, Active, Deactivate and Save offered", "Edit on all 16").
- **S24 New and unplayed states have no step** (C9, C14; B09-B16, C05-C07, C10-C14, C16-C18b, D06, F05, A10-A11). **Steps.**
  A short table: board, step or new `states.md` row, scratch or replay; the scratch ones (B09-B11, B14) say "scratch" in
  their label.
- **S25 The Spec's captures paragraph contradicts itself** (C10). **Spec.** "Opened and read (12 key, all present)" then
  eight "staged … not viewed": retitle as present 12, viewed 4, staged 8, and name the boards that stand on FIXTURES
  alone (A01, A02, B01, B06, C01, C02, E01, D05).

## Nits

- B17's two brackets are constants in the page and in act-1-setup.md:442-445: write them (K N1, C16).
- The EPA list drops Speciality and merges Code and Title (`EpasList.razor:28-38`): one line under `EpasList` in § 2 (K N2).
- D06's "Curricula" link is a markup change: the profile's line is a `<p class="muted">` constant still saying
  "Curriculum adoptions" (`TraineeProfileEdit.razor:441-443`) (K N3).
- The move's dialog gates in `SaveAsync`: the form submits through `OnSubmit="SaveAsync"` (`TraineeProfileEdit.razor:390`;
  the code review says `OnValidSubmit`), so a button-only gate lets Enter in a date field save unasked (K N4).
- `/admin/adoptions`'s redirect needs a page type for `NavItems`/`NavOwners` and a policy that sends a visitor to sign-in
  (`NavItems.cs:113`) (K N5).
- A09's and A12's captions, C02's and C15's headings are in no § 8 table: add them as NEW (K N6).
- AdoptedOn by the South African day: name the handler's `TimeProvider`; a test pins 22:30 UTC (K N7).
- A09's Items is a total across institutions (17 beside "16 national · 1 your own"): say so in the caption or split the
  cell (K N8).
- D07 was asked as one board and drawn as two; D07a's "no completions to count" is false of Sipho Ndlovu (6.36: 1 counts):
  bracket the person or say so in the chip (C12).
- C19's "150-character title" is a bracketed description: write a title of that length, so the wrap is measurable (C13).
- The Spec's route reads `{{Id:int}}` and `{{ItemId:…}}` (C15).
- `.curriculum-item-page` is needed; its `.check-grid .form-check` selector repeats `.form-check`; consider one
  `.user-page .form-check` rule (ds N1).
- Breadcrumb markup is not `Breadcrumbs.razor`'s (separator inside the crumb's `li`, `aria-current` on the `span`); it
  renders the same (ds N2).
- `role=""` printed on standing alerts; `Alert` with `Role=""` renders none (ds N3).
- Empty states use `<p class="state-panel-title">`; `StatePanel` renders a `div` (ds N4).
- No `aria-haspopup="dialog"` on Remove…, Deactivate…, Adopt…, Save profile and move… (DESIGN.md:757) (ds N5).
- A10's "For your institution" is read by the Administrator: "For this institution" (ds N6).
- D02's `.submit-check` changes with the picker and is not a live region; `role="status"` if the build finds it silent
  (ds N7).

## Decisions for the operator

The operator's Q1-Q21 (`15-institution-adopts-catalogue.md` § 6, `round-1-assessment.md` § 10) stand as given, with
the ask's T1-T12 except where F3 and F5 below settle them; nothing below reopens a Q.

| | Question | Recommendation | Alternatives |
|---|---|---|---|
| F1 | **How a result and its focus cross a page change** (M5, M6): "EPA saved." after a create; "Curriculum item added." / "saved." after the item pages; Cancel's return to the row's Edit | **A one-shot query token per outcome, T373's shape**: a links class (as `OnboardingLinks.Admitted` / `BackFromAdmit`) builds `/admin/epas/{id}?created`, `…/items?added={itemId}`, `?saved={itemId}`, `?back={itemId}`; the arriving page re-reads its record, writes the sentence itself from the token, focuses the result (or the row's Edit for `back`), and replaces the address without the token so a reload says nothing. It survives `forceLoad` (`EpaEdit.razor:257`), needs no state, and a bUnit test reads it | A scoped flash-message service (lost on the `forceLoad` reload and across circuits, so the create path would have to drop `forceLoad`; state to test); or no result after a create (C08 drops "EPA saved.", the h1 takes the focus, 6.25 unchanged), keeping only the item pages' tokens |
| F2 | **The tie rule for Newer / Older** (S2) | **Order by (`EffectiveFrom`, `Id`)**: a later clone is newer; the Spec says so and the query and the dialog share it | No line when the dates are equal (A03's row would then carry none); or refuse a second version of a discipline with the same effective date (flow 16's College form) |
| F3 | **The College admin and the Administrator on the item form pages** (M3, T4) | **T4 as proposed: one editor for every caller**, the edit row gone for all; the add and edit pages take their words from `CanEditCurriculum`: h1 "Add item" (built, `:329`), subtitle "On Paediatric EPA Curriculum (11.2). It applies to every institution that has adopted it.", the built national empty line (`:1139-1140`); a national item's edit page "Edit PAED-011" with the same subtitle. Draw both for Dr Kruger (6.30); flow 16 restyles the College's words | Keep the built in-place editor for the College and the Administrator until flow 16 (two editors, T323's edit row stays for them, 6.30 and A.7.10 unchanged); not recommended: T323's fault stays on the College's page and two editors drift |
| F4 | **How a refusal reaches its field** (S6) | **A typed refusal**: an Application exception carrying the field's key and the sentence (e.g. `FieldRefusalException(field, message)`), thrown where the commands throw today, so the audit pipeline still discards the refused save (T201); the pages map the key to the field and `aria-describedby`; anything else stays in the alert | A result type (`Result<T>` with a keyed refusal) on the three commands: cleaner, but every caller and the rollback path change; or the pages match known message constants (`DuplicateCode`, the held-EPA family, the move refusal): no command change, brittle to a reworded sentence |
| F5 | **T301's "a new version starts held back"** (S8) | **Fold it in, as Q5 decided**: `CreateCurriculumCommand` and `CloneAsNewVersion` create the version held back (the seeders set their curricula open explicitly, so no seeded value changes); Steps: 6.13's and 6.29's Do drop "untick Active", their Expect reads "Held back" after the save; 6.31's "tick Active" becomes the only opening; the boards' states stand | Only the refusal here, the default left to flow 16 with the College's box; 6.13 and 6.29 then keep their untick, and a fresh version is open for adoption until the College unticks it |
| F6 | **The items page of a superseded version** (S12) | **Subtitle "superseded on ‹date› by Paediatric EPA Curriculum (11.2); ‹N› registrars still on it"** (S1's reads); KGK's own items keep Edit and Remove… (the registrars still on 11.1 are measured by them); **no Add** (no new admission joins it; a new requirement on the remaining registrars mid-programme is a College-level question); one board, 11.1 after 6.36 | Add offered as on any version (the command allows it); or the page read-only for a superseded version |
| F7 | **The items table at 1280 and 901-1100** (S16) | **`clinic-table--compact`**, as DESIGN.md:1040-1042 measured for this table, and the round-3 render at 1280, 1100 and 901 (the first measurement any review has owed since round 1) | Fewer columns: window and weight to the item's page; or accept a sideways scroll inside `.table-container` with `tabindex="0"` and a name |
| F8 | **Badge or words for an institution's own EPA** (S18) | **Scope § 5's amendment to the admin lists and pages** ("words, never a badge, on the admin lists and pages"); the trainee's EPA page h1 (DESIGN.md:3776-3777) and My progress (G01) keep the badge until T357 (flow 05's) adopts Q3's words, as Q3 and Q17 leave it | Flow 15 changes G01 and the EPA page too (out of Q17's scope); or DESIGN.md says words everywhere now and lists the two as known debt under T357 |
| F9 | **Where UpdateEpa's "unadopted discipline" refusal applies** (S5) | **Create, and an Update that changes the sub-speciality**; an own EPA left in a discipline no longer adopted keeps Edit (a title fix, a deactivation), and the edge case stands as drawn | Refuse every Update of such an EPA and drop "Edit offered" from the edge case (Deactivate then still works through `DeactivateEpaCommand`, the two paths disagreeing); or Create only |

## What round 3's ask must do

- **Restate F1-F9 as decided**, beside Q1-Q21 and T1-T12, and draw what each decides (F1: C08 and the item list's
  results; F3: the College's add page and a national item's edit page; F6: 11.1's items page after 6.36; F7: the items
  table's class; F8: the § 5 wording).
- **Fix M1-M6 and S1-S25 board by board**, citing the id; the nits as one list, no agent each. Redraw every row-action
  cell (M1), A11 (M2), A08 (S1), the five load errors (S7), the seven dialogs (S13), C15's pager (S14), A10 and A11's
  link names (S15), the five loading boards (S19).
- **The Spec**: the hand-over (M5, M6); the item pages' words by caller (M3); the supersession and count rules (S1); the
  tie rule (S2); the Administrator's institution parameter (S3); the Create EPA flag (S4); UpdateEpa's refusal (S5); the
  keyed refusal (S6); the four pages' load errors (S7); T301's default (S8); the rebuild's focus (S9); the em dash (S10);
  the Deactivate… rename (S11); DESIGN.md § 5's further lines (S17, S18); the captures paragraph (S25).
- **The Steps**: 1.17 and the Act 6 Outcome (M4); 6.30's add half and A.7.10's Edit press (M3); 6.13, 6.29, 6.31 (S8);
  6.14, 6.17, 6.23 (S11); 6.33, 1.20, 6.25 (S20, S21, S23); the missed hits and Gaps (S22); the board-to-step table (S24).
- **Measure** the items table and the A boards at 1280, 1100, 901, 768 and 390 with `f15r2.css` linked (S16); open the
  eight captures staged and not viewed (S25); keep every fixture, word and order the reviews found true, invent nothing.

## Who raised what

| Here | Code review | Cast review | Design review |
|---|---|---|---|
| M1 | | | M1 |
| M2 | K15 | C1 | |
| M3 | K11 | C2, C4 | |
| M4 | | C3 | |
| M5 | K1 | | |
| M6 | K2 | | |
| S1-S12 | K3-K10, K12-K14, K16 (S1 = K3 and C11, S2 = K4, S3 = K5, S4 = K6, S5 = K7, S6 = K8, S7 = K9 and ds S3, S8 = K10, S9 = K12, S10 = K13, S11 = K14, S12 = K16) | S1 also C11 | S7 also S3 |
| S13-S19 | | | S1, S2, S4, S5, S6, S7, S8 |
| S20-S25 | | C5, C6, C7, C8, C9 and C14, C10 | |
| Nits | N1-N8 | C12, C13, C15, C16 (with K N1) | N1-N7 |

Merged from 6 must, 30 should and 20 nits across the three reviews (56) into **6 must, 25 should and 18 nits** (C1 into
M2; K11 and C4 into M3; C11 into S1; ds S3 into S7; C14 into S24; C16 into the first nit). C4, a should, is carried as
part of M3, since the same missing pages make 6.30 unplayable. Nothing was dropped. Corrected on the way: ds M1's
DESIGN.md lines (927-928, not 919-922); the cast review's and T3's "em dash" as the built Remove body (built is a hyphen,
`CurriculumItemsEdit.razor:769`); K N4's `OnValidSubmit` (the form uses `OnSubmit`, `TraineeProfileEdit.razor:390`); K13's
table-cell line (`:111`). Where the reviews meet, none contradicts another: K15 and C1 are one count, K11 gives C2's
mechanism, K9 and ds S3 are one load error seen from the page and from the board.
