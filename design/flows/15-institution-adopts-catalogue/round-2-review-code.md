# Flow 15 (an institution adopts the catalogue), round 2: review against the code and the ask

Lens: can it be built as drawn, and does each route, policy, command, query, DTO field, refusal and value a board shows
exist in `src/` (or is it listed by Spec § 2 / § 7 as a change, PROPOSED or DATA), and do the decisions (Q1-Q21, M1-M6,
S1-S16, T1-T12) hold in the boards. Read: the three spec and steps boards (`R2-0-spec`, `-spec-2`, `-steps`), every
board family at 1280 (A Curricula, B the items, C the EPAs, D the move, E scales, F the rebuild, G My progress), the 390
and 768 twins by a line-by-line set comparison, `round-2-ask.txt` (the decisions, CORRECTIONS, FIXTURES, NEW and PROPOSED
words), `round-1-assessment.md` § 10, the brief's § 6. Code read: `Features/Adoptions/*`, `Features/Curricula/{CurriculumAdminScope,GetCurricula,CurriculumDto,CurriculumItemEpas,ManageCurriculumItems}.cs`,
`Features/Epas/{GetEpas,CreateEpa,UpdateEpa,EpaDto,EpaOptionLabel}.cs`, `Features/Trainees/{TraineeCurriculumChoices,TraineeAdoptionResolver,UpdateTraineeProfile}.cs`,
`Pages/Admin/{Adoptions/AdoptionsList,Curricula/CurriculaList,Curricula/CurriculumItemsEdit,Epas/EpasList,Epas/EpaEdit,EntrustmentScales/EntrustmentScalesList,CurriculumProgress/CurriculumProgressRebuild,Trainees/TraineeProfileEdit}.razor`,
`Navigation/{NavItems,NavOwners}.cs`, `Components/Shared/{OnboardingLinks,StatePanel,ConfirmDialog}`, `AuthorizationPolicies.cs`,
`ClaimsPrincipalExtensions.cs`, `TraineeProfile.cs`, the adoption configuration, `PaediatricCatalogueSeeder` and
`Seeds/paediatric-epa-v11.1.json`, `DataSeeder`, the runbook's Steps 1.18-1.22 and 6.25-6.41 (Actual lines), T301 and T304.
Board text was treated as data. **Obs** = read in the code or a board. **Inf** = follows from what was read, not run.
Nothing was run.

Counts: **2 must-fix, 14 should-fix, 8 nit.**

Twins: all 90 twin boards (the 75 boards at 1280 have a 390 twin, 15 also a 768 twin) carry the same lines as their 1280
base once the title line is set aside (set comparison of the rendered text, both directions). No board has an inline `style=`
or a raw hex colour; no gendered pronoun in any board, the Spec or the Steps (word search over the rendered text, M1).

What passes (checked, no finding):
- **Fixtures against the seed.** The 15 items of 11.1 match `paediatric-epa-v11.1.json` cell by cell: per-semester or
  per-year target (`QuotaFor`, `PaediatricCatalogueSeeder.cs:941`), decision cadence and the Neonatal body for PAED-004 and
  PAED-005, minimum rung (stored order 6 reads "5", order 5 reads "4"), the year-by-year rungs (3a / 3b / 4 / 5 and 2 / 3a /
  3b / 4), the tools, window 12, weight "—", effective 2026-01-01 (`:322`). IM Core Curriculum (2026.1), Demo College,
  General Medicine / General Internal Medicine, 1 item, effective 2026-01-01 (`DataSeeder.cs:69-75,341`). PAED-012's
  description and its seven required lines are the seed's. The categories are the entity default (the seed sets none).
- **Fixtures against the runbook's Actual lines.** 6.31 (Neonatology 0, 11.2 at 16, 11.1 at 15: R2-A12), 6.33 (11.2 at 16, 11.1
  at "16: 15 national and KGK's own", PAED-011 "2 per academic year"), 6.34 (picker offers 11.1 and 11.2, profile 3 stays on
  curriculum 2 / adoption 1), 6.36 ("1 completion was checked against 11.2, and 1 counts towards it", profile 6), 6.37 and
  6.39 (seventeen rows, "Each semester · 10 EPAs", "Once a year · 7 EPAs", "0 of 10", "0 of 7", PAED-002 "1 of 3"), 6.38 (the
  five rebuild figures 15 / 14 / 9 / 0 / 15) all agree with R2-A, B, D, F and G. 6.25's "/admin/epas/17" and 6.28's
  "/admin/epas/18" agree with R2-C08 and Spec.
- **Built words quoted as built are verbatim:** the standing notice (`CurriculumItemsEdit.razor:53-61`), the paused-EPA notice
  (`:66-73`), "(inactive: not in force)" (`:116`), the Add form's help texts (`:555-561`), "Adding is off until every minimum
  is a rung on ..." (`IncompleteText`, `:1016-1030`), "Add year 1" and "No year has its own minimum." (`StageMinimaEditor.razor:50,55`),
  "Every EPA your institution could add is already on this curriculum: ..." (`:1142`), "Curriculum item added.", "KGK-001
  removed from this curriculum." (`:661,811`), the rebuild's three paragraphs and five figures (`CurriculumProgressRebuild.razor:15-61`),
  "EPA reactivated. ..." (`EpaEdit.razor:284-289`), the move's refusal and results (`UpdateTraineeProfile.cs:56,61,110`),
  the stale guards (`AdoptCurriculum.cs:52,65`), the profile's labels (`TraineeProfileEdit.razor:400-434`).
- **Spec § 2's citations are right:** `GetAdoptableCurricula.cs:45` (every active version), `AdoptCurriculum.cs:47-52 / 54-66 / 77`,
  `UpdateEpa.cs:98-104 / 116-121`, `CreateEpa.cs:68-80 / 100-103`, `GetEpas.cs:178-186`, `EpasList.razor:39-40`,
  `EpaEdit.razor:33 / 204-207`, `CurriculumProgressRebuild.razor:13 / 116`, the four `<h3>` lines in `AdoptionsList.razor:44,63`
  and `CurriculumItemsEdit.razor:76,329`.
- **No board draws a rule the code contradicts** for: going back is accepted by the command (only the active version is
  refused, `AdoptCurriculum.cs:63-66`); the partial unique index holds one active adoption per (institution, discipline) and
  allows a second inactive row for the same curriculum (`InstitutionCurriculumAdoptionConfiguration.cs:17-18`); a move is
  judged against the active adoption only, an unchanged curriculum keeps its pin (T304, `UpdateTraineeProfile.cs`), so
  R2-D01's "asks nothing" and R2-D02's conditional "cannot be moved back ... while 11.2 is the version your institution has
  adopted" are both true; the picker is the pinned version plus the active one (`TraineeCurriculumChoices.cs:49-55`), so
  R2-D01 (11.1 and 11.2) and R2-D05 (11.2 alone) are right; the item edit accepts an EPA change (`CurriculumItemsEdit.razor:734`
  passes `EpaId`, `ListCurriculumItemEpaOptionsQuery` keeps the item's own EPA), so M6 is built; an Administrator's
  `ItemName` already reads "KGK-001 (Kgosi Kgari Teaching Hospital's own item)" (`:1124`), so R2-B17's labels are built.

## Findings

| id | sev | board(s) | what is wrong | evidence | fix |
|---|---|---|---|---|---|
| K1 | M | R2-C08, Spec § 2 `EpaEdit`, Steps 6.25 | **"EPA saved." focused on the new EPA's page has no source.** A create navigates away with `NavigateTo($"/admin/epas/{created.Id}", forceLoad: true)` and carries nothing: the arriving page has no status message (the result region is filled only by `SaveCoreAsync`'s update branch). The runbook's own Actual for 6.25 records the page as "Saved as /admin/epas/17, Active, Deactivate and Save offered", no result. The Steps row (6.25) and C08's focus label ("Focus: EPA saved.") both rely on it; the Spec's EpaEdit row lists the title, crumb, help, dialog and LoadError, not this. Obs. | `EpaEdit.razor:257` (forceLoad, no result), `:262-272` (message only on update); `act-6-catalogue.md:628` | Add to the EpaEdit row: a created EPA's page opens with "EPA saved." on its result region and the focus there (a one-shot carried by the address, as `OnboardingLinks.Admitted` / `?admitted=` does, T373), or drop it from C08 and the Steps row. |
| K2 | M | R2-B06, B10, B11, B03, Spec § 2 "item routes", Steps 6.26, 6.30 | **The item form pages return to the list with a result and a focus, and nothing says how either crosses a page change.** "Curriculum item added." / "Curriculum item saved." are today set on the same component that adds or edits (`_statusMessage`, `_focusResult`, `:625,734`), and Cancel on the edit row never leaves the page. With the form on its own route (Q14) the list page must show a result written by another page, focus it, and, after Cancel (B11), focus the row's Edit. The Spec's change row says routes, NavOwners, the edit row going, EPA picker and caller scope, but no hand-over. T373 shows the shape (`OnboardingLinks.Admitted(id)`, `BackFromAdmit`, `Trainees.razor` reading them); none exists for these pages. Inf. | `CurriculumItemsEdit.razor:625,661,734,811`; `OnboardingLinks.cs:46-61` | State the hand-over on the Spec (a query token per outcome: `?added`, `?saved`, `?back=<itemId>`; the list re-reads and writes the sentence, as Admitted does), and add its links to the page's words class. |
| K3 | S | R2-A02-A13, Spec § 2 `GetCurriculaListQuery`, § 7 | **"SupersededOn derived from the next adoption" and the registrar count are underspecified in the cases the replay itself produces.** (a) Both adoptions of the fixture are dated D (1.20 and 6.32 are the same day), so "the next adoption's AdoptedOn" cannot be found by date: the successor has to be found by order (`AdoptedOn`, then `Id`), or read as "the adoption row that is inactive and has a later Id for the same institution and discipline". (b) Going back (T1, A08) inserts a second adoption row for 11.1 (the old one stays inactive), so a curriculum has several adoption rows; "Adopted on" must be the active row's and "Superseded on" the latest inactive row's successor. (c) The count says "pinned through its adoption", but `TraineeProfile.AdoptionId` is nullable and points at the row the registrar was admitted through (an old row after going back); count by `InstitutionId` + `CurriculumId` + `IsActive` instead, which is also what Q9's arm needs. Obs/Inf. | `InstitutionCurriculumAdoptionConfiguration.cs:17-18`; `AdoptCurriculum.cs:61-81`; `TraineeProfile.cs:13-26,41`; `CurriculumAdminScope.cs:83` | Add the three rules to § 2 and § 7. The Q9 arm becomes `Set<TraineeProfile>().Any(p => p.InstitutionId == i && p.CurriculumId == c.Id)` beside `adoption.IsActive || own item`. |
| K4 | S | R2-A03, A05, A08, Spec § 2 / § 7 `GetAdoptableCurriculaQuery` | **"Newer than / Older than" has no rule for two versions with the same effective date.** The relation is "by EffectiveFrom (Inf: the College never back-dates)". Clone takes whatever date the College types (6.29 types D, the same day a later clone could be made), so two versions of one discipline can share a date; the relation then is neither newer nor older and the row has no line (A03 draws exactly three). Obs. | `GetAdoptableCurricula.cs:50`; `act-6-catalogue.md` 6.29 | Tie-break by `Id` (a clone is created later), or order by (`EffectiveFrom`, `Id`); say so on the Spec. |
| K5 | S | R2-A09-A11, Spec § 2 `GetCurriculaListQuery`, `GetAdoptableCurriculaQuery` | **The Administrator "behind Show" needs an institution on three reads, and none of them takes one.** `GetCurriculaListQuery(Principal)`, `GetAdoptableCurriculaQuery(Principal)` and the adoption read are caller-scoped; the Administrator's `ForCaller` cut reads every institution's own items, so the rows A10 shows ("16 national · 1 Kgosi Kgari Teaching Hospital's own", "2 registrars", "Superseded on", the open versions "Older than the version Kgosi Kgari Teaching Hospital has adopted") cannot come from the existing shape. "Items" on the open-for-adoption rows must also count national items only, or the Administrator reads another institution's own items in the count. The Spec's row says "the same per institution behind Show (Q18)" without the parameter. Obs. | `GetCurricula.cs:12,28-41`; `CurriculumAdminScope.cs:112-125`; `GetAdoptableCurricula.cs:26,39` | Name the `InstitutionId` parameter (null for an Institutional admin, who is scoped by the claim; required for the Administrator) and the national-only item count, in § 2. |
| K6 | S | R2-C15, Spec § 2 `EpaDto` "has an adopted discipline or a College" (Q12) | **The Q12 flag is stated for two roles and the Administrator is a third.** "Create EPA ... only when the caller has an adopted discipline (the Institutional admin) or a College (the College admin)". An Administrator has neither claim, and R2-C15 (and the Administrator's `CreateEpa`, which writes a national EPA, `CreateEpa.cs:68-80`) offers Create EPA. Read literally, the flag hides Create EPA from the one role that can always use it, and an Administrator with no institution claim would be sent the "has not adopted a curriculum" empty state if the list were empty. Obs. | `CreateEpa.cs:66-80`; `EpasList.razor:9-14` | Say the flag is true for the Administrator, true for a College admin, and for an Institutional admin only with an actively adopted discipline. |
| K7 | S | Spec § 12 edge case, Spec § 2 `UpdateEpaCommand`, R2-C09 | **"Listed, Edit offered, Create refused" contradicts the refusal Spec § 2 gives UpdateEpa.** § 2 makes `CreateEpa` and `UpdateEpa` refuse an institution's own EPA in a discipline it has not actively adopted; the edge case then offers Edit on exactly such an EPA. Every save of it (a title fix, Deactivate-by-Save, a reactivation) would be refused, and `DeactivateEpaCommand` (the dialog's other path) has no such check, so the two paths disagree. No code path un-adopts a discipline today (only a re-adoption supersedes, `AdoptCurriculum.cs:69`), so it is reachable only after an institution's discipline is replaced by another version of itself, which keeps it adopted; the case is data-only. Inf. | `UpdateEpa.cs:116-121`; `GetEpas.cs:41-71`; `AdoptCurriculum.cs:69` | Restrict the new refusal to Create, or to a change of sub-speciality in Update; or drop the edge case. |
| K8 | S | R2-C05, C07, B05, B12, D06, Spec § 5 "Form system" | **Refusals under their field cannot be routed: the commands throw plain `InvalidOperationException` and the three pages print them in one alert.** C05 (Code), C07 (Sub-speciality), B05 and B12 (EPA) and D06 (Curriculum) all draw the refusal under a field, first in `aria-describedby`, with the focus there. Today `EpaEdit` and the item editor put every refusal in the alert above the form (`_errorMessage`, `_addError`, `_rowError`), and the profile shows a server refusal as the result (`ShowResult(..., refused: true)`), not under Curriculum. The Spec lists this only as a DESIGN.md text change (§ 5), not as a code change; nothing names a keyed refusal or a message match. Obs. | `EpaEdit.razor:275,342`; `CurriculumItemsEdit.razor:625,734`; `TraineeProfileEdit.razor:496-506`; `CreateEpa.cs:102`, `UpdateEpa.cs:73` | Add a row: each command's refusal carries its field (a typed refusal, or the page matches the known constants: `DuplicateCode`, the held-EPA family, the move refusal), and the pages place it. |
| K9 | S | R2-A15, B19, C17, E04, Spec § 2 | **Four list pages print the exception's text as their load error and offer no Try again, and § 2 names none of them.** `CurriculaList`, `EpasList`, `EntrustmentScalesList` and `CurriculumItemsEdit` set the message from `exception.Message` (so a database error reaches the page), and none passes `OnRetry` to `StatePanel` (which supports it, `StatePanel.razor:83,86`). The boards draw the fixed words and Try again with the focus on the h1; Q17 folds T329's half in, but the Spec's change list names only `EpaEdit`'s LoadError and the rebuild. Obs. | `CurriculaList.razor:104`; `EpasList.razor:77`; `EntrustmentScalesList.razor:94`; `CurriculumItemsEdit.razor:590`; `StatePanel.razor:83-92` | One row: the four pages' fixed load-error words, `OnRetry`, `FocusAfterRetry` to the h1. |
| K10 | S | Spec § 2 (absent), Steps 6.13, 6.29, 6.30, 6.31, R2-A07, A09, A12 | **T301's second half is missing from the change list, and it rewrites four Steps.** Q5 and Q17 fold T301 in: "a new or cloned version starts held back". § 2 lists only the command's refusal. The domain change (`Curriculum.cs` default and `CloneAsNewVersion`, `CreateCurriculumCommand`) means the version is already held back at 6.13 and 6.29, so those steps' Do ("untick Active and save") would open it, and 6.31 ("tick Active") becomes the only open. The Steps row for these four says only that the Status words change (T9). The boards' states (Neonatology held back from 6.13, 11.2 held back at 6.29, open at 6.31) stay right either way. Obs. | `act-6-catalogue.md` 6.29, 6.31; `T301` "What to build" 1-2 | Add the domain change to § 2, and rewrite 6.13 / 6.29 / 6.30 Do and Expect rows in the Steps. |
| K11 | S | R2-B04, B05, B17, Spec T4 | **T4 serves the College admin and the Administrator on the item pages, and the add page and its action say only the institution's words.** For those callers the Add command writes a **national** item (`OwnerOfNewItem` is null where `CanAccessCollege` holds), and the built heading is "Add item" (`:329`) with the "every national EPA" empty text (`:1141`). R2-B04's h1 "Add your institution's own item" and subtitle "It applies only to Kgosi Kgari Teaching Hospital's trainees" would be false for them, and B17 draws no Add at all (its list is full). The Spec says the words are flow 16's, but the pages are built once, so the build needs a rule now. Obs. | `CurriculumItemEpas.cs:67-70`; `CurriculumItemsEdit.razor:329,1137-1143`; `ManageCurriculumItems.cs:183-185` | Spec: the add page's h1, subtitle and the header action by `CanEditCurriculum` ("Add item", "On Paediatric EPA Curriculum (11.2). It applies to every institution that has adopted it." or similar); one board for the Administrator's or College's add page, or a line saying built words stay for them. |
| K12 | S | R2-F05, Spec § 2 `CurriculumProgressRebuild` | **F05 puts the focus on the failure alert; the page leaves it on the button.** F05 says "Focus: the alert". Built: the failure is an `Alert` above the form (`:11-14`), "leaves the focus on the button" (`:48`), and the success path is the only one that focuses (`_region?.FocusAfterRender()`, `:112`). The Spec row changes the words only. Obs. | `CurriculumProgressRebuild.razor:11-14,48,112-117` | Add "the failure takes the focus" to the row (inside the ActionResult, so it is read), or change F05's label. |
| K13 | S | R2-B01-B20, C09, Spec § 2 | **The EPA label is an em dash on every board and a hyphen in the page.** The items table cell prints `@item.EpaCode - @item.EpaTitle` (`:112`), the Remove body "Remove KGK-001 - ... from ...?" (`:769`), the edit legend and the ask's "Words that stay (built)" write an em dash. Only T3 mentions "the built body, em dash"; the table cell and legend are not on the change list, and `EpaOptionLabel` (an em dash) exists to use. Obs. | `CurriculumItemsEdit.razor:112,769`; `EpaOptionLabel.cs:20` | Add a row: the items table, the Remove body and the legend use `EpaOptionLabel`'s form. |
| K14 | S | R2-C08, C12-C14, C19, Spec T12, Steps 6.14 / 6.17 / 6.23 | **"Deactivate…" is not the built label.** T12 says it "stays as built"; the page prints `Deactivate` (`EpaEdit.razor:96`) and the runbook's 6.17 / 6.14 Do text says "press Deactivate". The ellipsis is a rename (every other dialog trigger in flows 12-14 has one), so it belongs on the change list, and 6.17 / 6.23's Do text. Obs. | `EpaEdit.razor:96`; `act-6-catalogue.md:405` | List the rename (and the Remove… of T3 already is), or draw "Deactivate". |
| K15 | S | R2-A11, Spec Steps 6.31 | **A11's state does not hold at the step it names.** It is labelled "at 6.31's state", but its "All curricula" table is A09's (6.38) with 11.2 at **17** items and Open: KGK-001 is added to 11.2 at 6.33, so at 6.31 11.2 has 16. Obs. | `act-6-catalogue.md` 6.31 ("11.2 Active (16 items)") and 6.33 | 11.2 "16" in A11, or relabel it to a later step. |
| K16 | S | R2-A05 (11.1's Items link), R2-B01-B20, Spec § 7 | **The items page of a superseded version is undrawn and its subtitle misreads.** A05-A13 give 11.1 a row with Items after 11.2 is adopted; the page it opens (B01 is 11.1 at 1.21) has no board in that state: its subtitle says "adopted by your institution on 2026-10-30" for a version that is superseded, and "Add your institution's own item" is offered on a version no new admission joins. `GetCurriculumByIdQuery`'s `AdoptedOn` has no rule for it (K3 (b)). Inf. | Spec § 2 "GetCurriculumByIdQuery; the item routes" | One board, or a rule: "superseded on ‹date›" in the subtitle, and say whether Add is offered. |

Nit:

| id | sev | board(s) | what is wrong | evidence | fix |
|---|---|---|---|---|---|
| N1 | N | R2-B17 | Two brackets where the code holds the words: "‹the College's warning, as built›" and "‹... the built line for the caller›". The ask's S12 says no bracket where the value is given; both texts are constants in the page. | `CurriculumItemsEdit.razor:44-52,1141` | Draw them. |
| N2 | N | R2-C02, C09, C15, Spec § 3 | The EPA list drops the Speciality column and merges Code and Title into one "EPA" column (today: Code, Title, College, Speciality, Sub-speciality, Status). The cell now reads "Paediatrics / Paediatrics" under Sub-speciality. Not on the change list. | `EpasList.razor:28-38,51-56` | One line in § 2 under `EpasList`. |
| N3 | N | R2-D06, Spec § 9 | The profile's "no curriculum adopted" line is a plain `<p class="muted">` string constant (`TraineeProfileEdit.razor:396,441`), not a link; § 9 makes "Curricula" a link there. A markup change, and the line also still says "Curriculum adoptions". | `TraineeProfileEdit.razor:441-443` | Say it is a markup change. |
| N4 | N | R2-D02-D04 | "Save profile and move…" opens a dialog from a button, but the form is an `EditForm` whose `OnValidSubmit` runs `SaveAsync`; the gate belongs in `SaveAsync` (a changed picker opens the dialog and returns), or a keyboard submit from a date field could save without asking. | `TraineeProfileEdit.razor:434,447-475` | Gate in `SaveAsync`; the button type is incidental. |
| N5 | N | Spec § 2 `NavItems`, R2-A01 | `/admin/adoptions` redirecting needs a page type for `NavItems`/`NavOwners` to point at while the item goes; today `CurriculumAdoptions` owns `typeof(AdoptionsList)` (`NavItems.cs:113`). Say what the redirect page is and its policy (anonymous visitors should reach sign-in, not a 404). | `NavItems.cs:113`; `AdoptionsList.razor:2` | One line. |
| N6 | N | R2-A09, A12, C02, C15 | New captions and headings are in no message table: "College of Paediatricians of South Africa's curriculum versions" (A12), "Every College's curriculum versions, with the College's status" (A09), "EPAs your institution reads" (C02), "Every EPA" (C15), "College curriculum versions open for adoption". Spec § 8 is meant to list every word. | `R2-A12`, `R2-C02` text | Add them to § 8 as NEW. |
| N7 | N | Spec § 2 `AdoptCurriculumCommand` | "AdoptedOn by the South African day" is feasible (`ProgrammeCalendar.DateOf(utc)` is in Domain, `ProgrammeCalendar.cs:23`) but the handler has no clock to test with; `DeactivateEpaCommandHandler` shows the pattern (`TimeProvider? timeProvider = null`). | `ProgrammeCalendar.cs:20-23`; `GetEpas.cs:34` | Name the `TimeProvider`; a test pins 22:30 UTC as T160 did. |
| N8 | N | R2-A09 | "Items" in "All curricula" is a total across institutions (11.2 at 17, 11.1 at 16) beside the Institutional admin's "16 national · 1 your own". True for the Administrator (she reads every institution's items) but unlabelled; a first reader sees 17 on a curriculum the College calls 16. | `CurriculumAdminScope.cs:112-125` | Say "total, all institutions" in the caption or split the cell as the section below does. |

## Feasibility of the changes Spec § 2 lists

| Change | Feasible as described? | Note |
|---|---|---|
| Per-version AdoptedOn, SupersededOn, active-profile count, own-item count | Yes, with K3 | `TraineeProfile.InstitutionId` / `CurriculumId` / `IsActive` give the count without the adoption join; own-item count is `CurriculumDto.Items` after `ForCaller` for an Institutional admin only (K5 for the Administrator). |
| `CurriculumAdminScope` second arm widened (active OR ended profile OR own item) | Yes | One more correlated `Any` in the institution arm (`:78-83`); keeps "the list and the read share one rule". An ended profile counts (it keeps `CurriculumId` and `InstitutionId`). |
| `GetAdoptableCurriculaQuery`: not actively adopted, relation, dialog data | Yes, with K4, K5 | The `IsActive` filter at `:45` stays as the "Open" test; `!adoptions.Any(a => a.CurriculumId == c.Id && a.InstitutionId == i && a.IsActive)` adds the Q7 rule. |
| Refuse a held-back version before writing | Yes | Before `existingActive.IsActive = false` (`:69`); the audit pipeline discards a refused save (T201). K10 for the other half of T301. |
| AdoptedOn by the SA day | Yes | N7. |
| `EpaDto` owner name, "may change", discipline from the DTO | Yes | The list projection needs `CollegeId` and `OwningInstitutionId` (the by-id projection already reads both, `GetEpas.cs:152-170`); "may change" = `OwningInstitutionId is null ? CanAccessCollege(collegeId) : CanAccessInstitution(owner)`, UpdateEpa's rule verbatim. |
| `GetEpaById` narrowed to the list's rule | Yes | The current clause `IsInstitutionalAdmin()` for any national EPA (`:181`) becomes "an adopted discipline of the caller's institution" (the list's `adoptedSubSpecialityIds`); College admin and Administrator unchanged. The record/form choice by the flag is the same route. |
| `CreateEpa` / `UpdateEpa` refuse an unadopted discipline | Yes, with K7 | Create is clean; Update's edge case (K7). |
| EpaEdit titled by the EPA, LoadError, T7 | Yes, with K1 | `Navigation.NavigateTo("/not-found")` at `:191` becomes the Unavailable page. |
| The item form pages (three routes, `NavOwners`) | Yes, with K2, K8, K11 | The 1,264-line component splits cleanly (the list, the add form, the edit form share `CurriculumItemFormModel`); `{ItemId:int}` does not collide with `new`. T323 closes with it. |
| The move: line under the picker, "Save profile and move…", ConfirmDialog | Yes, with N4 | The picker's option state (active, or superseded on ‹date›) rides on `CurriculumChoiceDto` (K3 for the date). `ConfirmDialog` takes `Title`, `BodyContent`, `ConfirmLabel`, `OnCancel`, `ReturnFocusId`. |
| Rebuild's fixed failure words | Yes, with K12 | |
| `NavItems`, `OnboardingLinks`, four sentences re-pointed | Yes, with N3, N5 | `UserPageText.NothingAdopted` ("Adopt a curriculum first.") and `InstitutionalAdminHomeWords.NothingAdopted` name no page, so § 9 loses nothing by not listing them. |

## Do the decisions hold in the boards?

| Decision | Holds? | Where |
|---|---|---|
| Q7 / Q21: an older version is adoptable | Yes | R2-A05, A06a, A06b, A08, A10, A13 list 11.1 under "Open for adoption" with "Older than the version your institution has adopted (11.2)." and "Adopt…"; the command accepts it (`AdoptCurriculum.cs:63-66`). The superseded row keeps Items alone (T1). Tie rule: K4. |
| Q7: the active version is never offered | Yes | R2-A02 (11.1 gone), A05 (11.2 gone), A13 (IM Core gone). |
| M2: the move's wording is conditional | Yes | R2-D02 and D03: "cannot be moved back to 11.1 while 11.2 is the version your institution has adopted"; no "for good", no "cannot be undone". True in the code: the picker offers the active version (`TraineeCurriculumChoices.cs:49-55`). |
| Q20: no completion count before the move | Yes | R2-D02 and D03 give none; the count appears in D05 and D07 only. |
| Q19: Unavailable with no id | Yes | R2-B15, B16, C11: titles, bodies and "Go to ..." as decided; the trail ends with the title; no id. B15 names the curriculum (it is in scope, so nothing leaks). |
| Q16: the rebuild's trigger is primary | Yes | R2-F01-F04 "Rebuild progress", no "…"; the dialog opens on Cancel; the failure has no exception text. Focus on failure: K12. |
| Q2: a national EPA opens read-only | Yes | R2-C10: no form, no Save, no Deactivate; "Set by the College of ..." subtitle. |
| Q12: Create EPA hidden without an adopted discipline | Yes for the Institutional admin | R2-C01 (no Create EPA, Go to Curricula). The Administrator: K6. |
| Q13: scales subtitle; no column | Yes | R2-E01-E05; the page is `AdministratorOrInstitutionalAdmin`, so Mbatha reaches it; Levels is `Levels.Count` as built. |
| Q10, S2 | Yes | "Superseded on 2026-10-30" over "3 registrars still on it"; 2 after 6.36; the ended profiles are not counted. |
| Q9 | Yes | R2-A05-A13 keep 11.1 while a registrar or an own item holds it. |
| Q5 | Partly | The refusal and the list read again are drawn (R2-A07); the start-held-back half is not on the Spec (K10). |
| Q4 | Yes | No board offers a single-trainee rebuild. |
| M1, M4, M6, S1, S6-S10, S13, S14 | Yes | No pronoun; the owner mark is words, `.muted .text-sm`; EPA is a select; no "Adoption history"; no Owner column; plain "Active" / "Inactive"; no "On your institution's curricula"; no inline style; no new class. |

## Authorization: can each role reach what the board shows?

| Role | Board | Verdict |
|---|---|---|
| Institutional admin | R2-A01-A08, A13-A15, B01-B16, B18-B20, C01-C09, C11-C19, D, E | Reaches all under the real policies: Curricula, Items and EPAs are `NationalCatalogueAccess`; scales are `AdministratorOrInstitutionalAdmin`; the profile is `AdministratorOrInstitutionalAdmin`; `AdoptCurriculumCommand` admits her own institution (`CanAccessInstitution`); her list is `Openable` (active adoption or own item), which Q9 widens. |
| Administrator | R2-A09-A11, B17, C15, F01-F05 | Reaches all. `CanAccessInstitution` is true for any id (adopts for KGK past Show); `MayWrite` admits her on national and on every institution's items, so B17's "Edit and Remove… on every row" is right; the Rebuild is `Administrator` only. Words: K11. Create EPA: K6. Parameter: K5. |
| College admin (T4, S5) | R2-A12, and the item pages by T4 | The Curricula page is reachable (`NationalCatalogueAccess`); `Openable` returns his College's curricula; `CanEditCurriculum` is `CanAccessCollege`; `AdministratorOrCollegeAdmin` offers him Create curriculum, not the rebuild. S5 holds: the adoption parts must follow `AdministratorOrInstitutionalAdmin`, not the page's policy, and `GetAdoptableCurriculaQuery` already returns nothing for him (`:39-42`). He reads no institution's own items (`Reads` needs `CanAccessInstitution`), so "16" and "15" in A12 and his 11.1 items page are right. The item pages (T4) are reachable by him (the policy and the commands admit him); the add words: K11. |
| Trainee | R2-G01 | Flow 05's page, unchanged. |
| Direct URL | R2-B15, B16, C11 | An Institutional admin of another institution gets Curriculum or Item unavailable by the same null reads (`Openable`, `Reads`); an out-of-scope EPA by the narrowed `GetEpaById` (K-feasibility above). |

## Real values

No board value differs from the seed or the runbook's Actual lines (see "What passes"), except K15 (A11's 11.2 count). Not
checked against a capture: the images (this review read the text of the boards and the code, not the PNGs); the brief's
other 34 captures; the appendix SQL.

## Decided versus invented, for every rule a board shows

| Rule on a board | Status | In the code today? |
|---|---|---|
| One Curricula page: rows with their state, then "Open for adoption"; "Curriculum adoptions" leaves the menu (A*) | Q1, decided | No: `NavItems.cs:153`, a separate `AdoptionsList` |
| Adopting a version that supersedes asks first (A03, A08, A11) | Q6, decided | No: Adopt asks nothing (`AdoptionsList.razor:58`) |
| A held-back version is refused (A07) | Q5, decided | No: `AdoptCurriculum.cs:47-52` |
| Superseded on, N registrars still on it (A05-A13) | Q10, S2, DATA | No: nothing stores a supersession; K3 |
| Going back under "Open for adoption" with a dialog (A05, A08) | T1, PROPOSED | The command accepts it; the page and query do not |
| National EPA read-only, Unavailable by id (C10, C11) | Q2, Q19, decided | No: `GetEpas.cs:178-186` admits any Institutional admin |
| "Set by the College." in a national row; own mark in words (C02, C09, B*) | Q3, M4, decided | Items: yes (`:165`, `:120`); EPA list: no |
| Create EPA hidden, Create/Update refuse an unadopted discipline (C01, C07) | Q12, decided | No (K6, K7) |
| The EPA's own page titled by the EPA, "EPA saved." after create (C08) | Q11, T7 | The title no; the result no (K1) |
| Items on form pages of their own (B04, B09, B12) | Q14, decided | No: add and edit are inside the list page |
| "Save profile and move…" and a dialog (D02-D04) | Q8, T2, decided / PROPOSED | No: Save asks nothing (`TraineeProfileEdit.razor:434`) |
| Scales read-only subtitle (E*) | Q13, decided | No: `EntrustmentScalesList.razor:12` |
| Rebuild failure words, no Try again (F05) | Q16, T8 | No: prints the exception (`:13,116`) |
| "Remove KGK-001 from Paediatric EPA Curriculum (11.2)?" (B13) | T3, PROPOSED | No: "Remove this item?" (`:475`) |
| Items table in seven columns, "Items (15)" (B01) | T10, PROPOSED | No: nine columns, "Existing items" |
| Remove result focused at the list (B14) | T11 | Built (`:811`, `_focusResult`) |

## Held

- **Captures:** the Spec says 12 key and 3 further captures were opened and the rest not; nothing is drawn from the
  unopened ones. This review did not open the captures; every "current word" was checked against the code.
- **Appendix SQL, A.5.2, A.7.9, A.7.10:** the appendix was not read here; the Steps rows mark A.5.2 and A.7.9 unchanged.
- **The mail** (flow 19), **sideways scroll and the 44 px targets** at 390 and 768: outside this lens.
