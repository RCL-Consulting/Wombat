# F16 The College keeps the national catalogue

**Who and why:** the College's administrator keeps the College's specialities and sub-specialities (each with a default
entrustment ladder), its EPAs and its curriculum versions: he corrects an EPA's wording, pauses an EPA while the College
revises it and restores it, starts a curriculum and holds it back from adoption, clones a version, changes its items and
opens it for adoption. The Administrator keeps the Colleges and the entrustment scales. A target, its period and an
EPA's pause are read live in every institution that has adopted the curriculum, closed periods included, so every page
must say whose page it is and how far a save reaches before it is made. CollegeAdmin Dr Anton Kruger (CPSA) reads the
CPSA's catalogue (Act 1), corrects Paediatrics and PAED-006, adds Neonatology and its curriculum 1.0 (held back), pauses
and restores PAED-012, adds PAED-016, clones 11.1 as 11.2, edits its items and opens it (Act 6); `devadmin` reads the
CPSA's record and ladder (Act 1), adds the CNSA with Neurology, Adult Neurology and its ladder, and is refused the
delete of a ladder in use (Act 6). Pages: Colleges, a College's record, its specialities and their records,
sub-specialities and their records, Entrustment scales and a scale, EPAs and an EPA's page, Curricula, a curriculum's
page (create, edit, clone, open for adoption), a curriculum's items and an item's page, Access denied, and the College
admin's Home. Of these, flow 15 (T392, `c931d56f`, with `e10a5f22` and `50ca6470`) rebuilt the EPA list, an EPA's page,
Curricula, a curriculum's items, the new item page and the scales list, for every caller but in the institution's words;
flow 01 gave every page the shell and its " · Wombat" title; nothing else has touched the College's and the
Administrator's own pages since 2026-09-25. The step captures are from flow 15's replay (T392, 2026-11-02), on the
shell as built; the `states/` captures of Colleges, specialities, sub-specialities and a scale's page are from
2026-09-26, on the old frame.

Restated 2026-11-04 for T394 (the brief of 2026-09-26 predates flows 01–15: its LAYOUT line described the pre-shell
frame and a nine-column items table with an edit row, which flow 15 replaced by a six-column table and an item page of
its own (T323's item-editor half is gone); it asked T301 for held-back starts and "Open" / "Held back", which flow 15
built (F5, T9), leaving only the curriculum page's box; it asked T331, done in flow 01 (`b347e11c`); it said nothing
reads a College's, speciality's or sub-speciality's Active flag, which is not so (invitations and an institution's
adopted disciplines read them, below); its CONTENT named KGK's "five registrars" with no source, and its STEPS quoted
Expects flow 15 has since rewritten). Paths to screenshots are under `design/baseline/` (gitignored). Code is cited at
`fdbf2ff0`; `W/` is `src/Wombat.Web/Components/Pages/`, `A/` is `src/Wombat.Application/Features/`, `Nav/` is
`src/Wombat.Web/Navigation/`.

| | |
|---|---|
| Decision | **Restructure, with UX in scope** (W-008; BRIEF.md § 4), inside flow 01's shell. Marked **F** in BRIEF.md § 8; restated here as **F/W**: in part 2 the EPA list, an EPA's page, Curricula and the items keep flow 15's shapes and change in the College's words, but in part 1 how the College's tree (speciality › sub-speciality, with its default ladder) is kept (Q1) and where a change's reach is told (Q2) are structural, and so is the curriculum's own page, which no flow has redesigned (Q4, Q5). |
| Frequency and stakes | Rare: a few times a year; about one curriculum version a year. High: "a target and its period are read live … in every institution that has adopted it, including periods that have already closed" (`W/Admin/Curricula/CurriculumItemsWords.cs:70–71`); a pause stops an EPA being a target on every running programme at once (6.17); opening a version lets every institution adopt it (6.31). |
| Mode | **Restated by BRIEF.md § 2.3 step 4 and § 2.4** (the old row's "straight to fidelity … the List and Form page shapes hold (BRIEF § 4)" cites the pre-W-008 recommendation). **In two parts, as the old brief and T394 say.** Part 1: **wireframe first** (2–3 STRUCTURES) for (A) the College's tree: Specialities, a speciality, its sub-specialities and their records (Q1: separate lists as today, or one College page with each speciality's sub-specialities and default ladders), with the Administrator's way in from Colleges; **straight to fidelity** (2–3 variations) for (B) Colleges and a College's record and (C) the scales list and a scale's page (Q6, Q7, Q8, Q11). Part 2: **wireframe first** (2–3 STRUCTURES) for (D) a curriculum's own page: create, edit, open for adoption, clone (Q4, Q5), and where the reach is told before a save (Q2); **one proposal at fidelity** each, to confirm or correct, for (E) the EPA list and an EPA's page in the College's words and (F) a curriculum's items and an item's page as the College keeps them (flow 15's shapes; Q2's reach, Q14's empty state, Q10's refusal). Each part: the picks with every state at fidelity; then one review round from the boards' text, the operator's decisions, and one correction round. |
| Viewports | Desktop 1280×800 and phone 390×844; the build is also checked at 768, and a new table is swept from 900 to 1100 px against its container (§ 11, flows 06 and 15) |
| Variations | 2–3 structures (wireframes) for (A) and (D); 2–3 variations (fidelity) for (B) and (C); one proposal each for (E) and (F); the chosen ones at fidelity after each pick |
| People | CollegeAdmin Dr Anton Kruger (College of Paediatricians of South Africa, CPSA; College 2; `kruger@cmsa.wombat.local`). Administrator `devadmin` ("Demo Administrator"). Seen through the pause: Assessor Dr Mohammed Patel (6.18), Trainee Dr Anele Dlamini (6.19, 6.24), flows 04 and 05's pages. The reach of a CPSA change: one institution, Kgosi Kgari Teaching Hospital (KGK), whose adoption of 11.1 holds five profiles at 6.17 (Dr Dlamini, Dr Mahlangu and Dr Ndlovu in training; Dr Molefe's programme complete; Dr du Plessis withdrawn: `act-6-catalogue.md`'s Starting state). `D` is the replay day: 2026-11-02 in the T392 replay, which played Acts 1–6 that day |
| Runbook steps | 33, pasted verbatim in § 8: 1.2–1.4, 1.12–1.18, 6.1–6.8, 6.10–6.14, 6.17–6.19, 6.23, 6.24, 6.28–6.31 and A.7.10. 6.18 is flow 04's activity page, 6.19 and 6.24 flows 05 and 03's My progress, an EPA's page, My activities and the picker (keep them; draw only what this flow changes). 6.14a (the College's activity type) sits between 6.14 and 6.17 and is flow 17's. 1.15 and 6.10 end on flow 01's Access denied |
| Pages (`coverage.md` templates) | The 21 the old brief named, plus flow 15's two item-page templates and Home. Part 1 (none redesigned; only `b347e11c`, the shell, and `0097df6e`/`2b1b0357`/`6dc4d541`, the 2026-09-25 row names, result focus and scale rename warning, have touched them): `/admin/colleges` (`W/Admin/Colleges/CollegesList.razor`, 67 lines, `Administrator` (`:2`)); `/admin/colleges/new` and `/admin/colleges/{Id:int}` (`W/Admin/Colleges/CollegeEdit.razor`, 219, `Administrator` (`:3`)); `/admin/specialities` (`W/Admin/Institutions/MySpecialitiesRedirect.razor`, 34, `AdministratorOrCollegeAdmin` (`:2`): a College admin to his College's list, anyone else to `/admin/colleges` (`:26–32`)); `/admin/colleges/{CollegeId:int}/specialities` (`W/Admin/Institutions/SpecialitiesList.razor`, 87, `AdministratorOrCollegeAdmin`); `/admin/colleges/{CollegeId:int}/specialities/new` and `/{Id:int}` (`W/Admin/Institutions/SpecialityEdit.razor`, 225); `/admin/specialities/{SpecialityId:int}/sub-specialities` (`W/Admin/Institutions/SubSpecialitiesList.razor`, 71); `/admin/specialities/{SpecialityId:int}/sub-specialities/new` and `/{Id:int}` (`W/Admin/Institutions/SubSpecialityEdit.razor`, 242), all four `AdministratorOrCollegeAdmin`; `/admin/entrustment-scales/new` and `/{Id:int}` (`W/Admin/EntrustmentScales/EntrustmentScaleEdit.razor`, 277, `Administrator` (`:3`)); `/access-denied` (`W/AccessDenied.razor`, 71, anonymous allowed; flow 01's); `/` (`W/Dashboards/CollegeAdminDashboard.razor`, 16, `Roles = "CollegeAdmin"`, rendered by `W/Home.razor:32–33`). Part 2 (rebuilt by flow 15 except the curriculum's page): `/admin/epas` (`W/Admin/Epas/EpasList.razor`, 184, `NationalCatalogueAccess`: Administrator, CollegeAdmin, InstitutionalAdmin, `src/Wombat.Infrastructure/Identity/AuthorizationPolicies.cs:81–82`); `/admin/epas/new` and `/{Id:int}` (`W/Admin/Epas/EpaEdit.razor`, 628, `NationalCatalogueAccess`); `/admin/curricula` (`W/Admin/Curricula/CurriculaList.razor`, 715, `NationalCatalogueAccess`); `/admin/curricula/new` and `/{Id:int}` (`W/Admin/Curricula/CurriculumEdit.razor`, 296, `AdministratorOrCollegeAdmin` (`:5`); **untouched by flow 15**, last `b347e11c`); `/admin/curricula/{Id:int}/items` (`W/Admin/Curricula/CurriculumItemsEdit.razor`, 422, `NationalCatalogueAccess`); `/admin/curricula/{Id:int}/items/new` and `/{ItemId:int}` (`W/Admin/Curricula/CurriculumItemPage.razor`, 870, `NationalCatalogueAccess`; new in flow 15); `/admin/entrustment-scales` (`W/Admin/EntrustmentScales/EntrustmentScalesList.razor`, 156, `AdministratorOrInstitutionalAdmin` (`:2`): no College admin; rebuilt by flow 15). No page was renamed or moved since 2026-09-26. The templates in full: `/admin/colleges`, `/admin/colleges/new`, `/admin/colleges/{Id:int}`, `/admin/colleges/{CollegeId:int}/specialities`, `/admin/colleges/{CollegeId:int}/specialities/new`, `/admin/colleges/{CollegeId:int}/specialities/{Id:int}`, `/admin/specialities`, `/admin/specialities/{SpecialityId:int}/sub-specialities`, `/admin/specialities/{SpecialityId:int}/sub-specialities/new`, `/admin/specialities/{SpecialityId:int}/sub-specialities/{Id:int}`, `/admin/entrustment-scales`, `/admin/entrustment-scales/new`, `/admin/entrustment-scales/{Id:int}`, `/admin/epas`, `/admin/epas/new`, `/admin/epas/{Id:int}`, `/admin/curricula`, `/admin/curricula/new`, `/admin/curricula/{Id:int}`, `/admin/curricula/{Id:int}/items`, `/admin/curricula/{Id:int}/items/new`, `/admin/curricula/{Id:int}/items/{ItemId:int}`, `/access-denied`, `/`. |
| Menus (`Nav/NavItems.cs`) | The College admin's one ungrouped group: Specialities (`/admin/specialities`), EPAs, Curricula, Activity types (`:160`), then My data rights; no Colleges and no Entrustment scales. The Administrator's "Organisations" (Institutions, Colleges) and "Catalogue" (EPAs, Curricula, Activity types, Entrustment scales) (`:140–147`). Owners (`Nav/NavOwners.cs`): the specialities and sub-specialities pages light Specialities for the College admin and Colleges for the Administrator (`:84–88`, `:142–145`; T331, done in `b347e11c`); a College's record Colleges (`:146`); an EPA's page EPAs, the items and an item's page Curricula (`:151–155`); a curriculum's page Curricula (`:158`); a scale's page Entrustment scales (`:162`). |
| Components drawn at runtime | **Part 1, all on the old shapes.** `PageHeader`: Colleges "Colleges" / "National governing bodies (CMSA constituent colleges) that own the EPAs and curricula institutions adopt." with Create college (`CollegesList.razor:8–13`); a College "Create college" / "Edit college" / "Maintain the national college record that owns specialities, EPAs and curricula." with **Back to colleges** (`CollegeEdit.razor:8–12`); Specialities "Specialities" / "College: College of Paediatricians of South Africa (CPSA)" with **Back to colleges for every caller** and Create speciality (`SpecialitiesList.razor:8–15`); a speciality "Edit speciality" with Back to specialities and Manage sub-specialities (`SpecialityEdit.razor:10–16`); Sub-specialities "Speciality: Paediatrics" with Back to specialities and Create sub-speciality, **no trail of its own** (`SubSpecialitiesList.razor:8–17`); a sub-speciality "Edit sub-speciality" with Back to sub-specialities (`SubSpecialityEdit.razor:11–13`); a scale "Edit entrustment scale" / "Define the ordered ladder of entrustment levels." with Back to scales (`EntrustmentScaleEdit.razor:11–15`). `DataTable` with a caption ("Colleges", "Specialities", "Sub-specialities") and **no `Stack` or `StackWide`** on the three lists. `StatePanel` with `LoadError` printing `exception.Message` (`CollegesList.razor:64`, `SpecialitiesList.razor:84`, `SubSpecialitiesList.razor:68`). The four record forms: `EditForm` with `DataAnnotationsValidator`, `ValidationSummary` **and** `ValidationMessage` (each message twice), default DataAnnotations sentences ("The ShortCode field is required."), `FormField Required`, an `ActionResult` whose success takes the focus ("College saved.", "Speciality saved.", "Sub-speciality saved."), and a **`btn-danger` Deactivate with no dialog, offered whenever the record exists** (`CollegeEdit.razor:63–72`, `SpecialityEdit.razor:63–70`, `SubSpecialityEdit.razor:71–78`). The sub-speciality's Default entrustment scale select, "No default — offer every scale", help "Committee STARs for this programme's trainees are limited to this scale's levels." (`SubSpecialityEdit.razor:50–59`). The scale's page: no `EditForm`; `<h3>` "Scale details" and "Levels" with no `<h2>` (`:43`, `:57`); per level Up, Down and a `btn-danger` Remove (`:95–97`); a rename warning alert (`:31`). **Part 2, flow 15's shapes** (`DataTable` `StackWide`, stacked to 1024 px on the EPA list and the items; `StatePanel` with fixed load-failure words and Try again; `FieldRefusal` under the field with `aria-describedby` and focus; `CatalogueLinks`' one-shot `?created`/`?added`/`?saved`/`?back` tokens; `ConfirmDialog` opening on Cancel; owner marks in `.muted .text-sm`; "Set by the College."; `CurriculaWords.Status` "Open" / "Held back"), in words written for the institution: the College admin keeps the old subtitles "Maintain the entrustable professional activities available to curricula and forms." (`EpaWords.cs:26`) and "Manage curriculum versions and the sub-speciality they apply to." (`CurriculaWords.cs:15`). **The curriculum's page is neither:** `details-grid` of `detail-card`s, `EditForm` with `ValidationSummary` twice (`CurriculumEdit.razor:42`, `:98`) over `ValidationMessage`s, a Status checkbox labelled **"Active"** on an existing curriculum (`:72–78`), a "Clone as new version" card ("‹version›-copy", from the effective date + 1, `:187–188`), Back to curricula and Manage items in the header (`:12–20`), every failure `exception.Message` (`:193`, `:231`, `:259`). Requests: `GetCollegesListQuery`, `GetCollegeByIdQuery`, `Create`/`Update`/`DeactivateCollegeCommand`, `GetSpecialitiesForInstitutionQuery`, `Create`/`Update`/`DeactivateSpecialityCommand`, `GetSubSpecialitiesForSpecialityQuery`, `Create`/`Update`/`DeactivateSubSpecialityCommand`, `GetEntrustmentScalesListQuery`, `Create`/`Update`/`DeleteEntrustmentScaleCommand`, the EPA and curriculum requests flow 15 lists, `CreateCurriculumCommand`, `UpdateCurriculumCommand`, `CloneCurriculumCommand`. |
| Held | None. Every step capture is from the T392 replay (2026-11-02) on the shell as built, except `act-6/6.13-2-saved-inactive.png` (2026-10-30: nothing is unticked since F5; do not attach). Five EPA-list step captures (`1.16-1`, `6.14-2`, `6.17-3`, `6.23-2`, `6.28-2`) and the states `epas-list--college`, `--institution`, `--inactive` were taken before `50ca6470` widened the EPA column (22:23 that day), so their titles wrap over three to six lines: read their words, not their widths. Every `states/` capture of Colleges, specialities, sub-specialities and a scale's page is from 2026-09-26, on the old frame (a Title Case sidebar with Logout, an email in the top bar): read their bodies only. `states/home--college-admin.png` and `access-denied--signed-in` are from 2026-09-27, on the shell |
| Depends on | Flow 01, built (`b347e11c`): the shell, the menus, the trail, not found, Access denied, `ActionResult`, `ConfirmDialog`, `StatePanel` with Try again. Flow 11 (`69a17c70`): refusals under their field, focused; the required mark; `StackWide`; the list is the page and its create form a page of its own. Flow 12 (`e53d4ae7`): a destructive action confirmed, its trigger an outline button; a detail page in one column of `<h2>` sections. Flow 13 (`5cc10c66`): an end section of its own apart from Save. Flow 14 (`e8eea1d2`): `ConfirmDialog` in action mode opens on Cancel; a page's own Unavailable state. Flow 15 (`c931d56f`, `e10a5f22`, `50ca6470`): everything named under Part 2 above, F3 (one item editor for every caller), F5 (a new or cloned version starts held back), T9 (the lists' "Open" / "Held back"), the held-back refusal at adoption (`A/Adoptions/AdoptCurriculum.cs:71`, `:108–111`), and its round-2 ask's deferrals to this flow ("the College's box is flow 16's"; "flow 16 restyles the College's words"; "flow 16 may add a header link"; T242's refusal "flow 16 draws it"). The design system is **version 45** |

**How to run this thread** (BRIEF.md § 2.3 step 4; § 11):
1. **Stage the upload set:** `powershell -File design/tools/stage_upload.ps1 -Flow 16`. Open each screenshot. The
   script puts any capture whose name contains "invit", "issued", "resent", "resend" or "reset" in
   `design/upload/crop-first/`; § 4 names none. No capture here shows a password or a one-time link; the captures show
   scenario names, College short codes and EPA codes, which are the pages' own record.
2. **The canvas** is "Wombat 16" (https://claude.ai/artifact/AqaH447KL91WCTbjQFJnWf), on the Wombat design system
   v45, with `tokens.css` written by hand (§ 11, flow 09) and the mark copied in (T394's note, 2026-11-04). § 1 and
   `round-1-ask.txt` carry a `MARK:` line for the mark's `/_blob/` URL, filled in before the send.
3. **Check on the canvas that the attachments arrive** before round 1 (§ 11, flow 13): the ask tells it to say so first
   on its Spec board. Then **send § 1 and § 8 as ONE message** (`design/flows/16-college-catalogue/round-1-ask.txt`),
   with the key screenshots in § 4. Read `project/canvas.json`'s version before and after: a send can draw nothing.
   The ask tells the canvas to save its stylesheet as a project file (§ 11, flow 14).
4. **Answer § 6's operator questions first** (Q1, Q3–Q5, Q7–Q10, Q12, Q13 are the operator's): Q1 decides structure (A),
   Q4 and Q5 structure (D). A question whose answer rests on an open task names it, and the task lands before the build
   when it changes data or rules (§ 11, flow 05). Pick a structure for (A), a variation for (B) and (C), and stop: part
   2 is asked after part 1's pick, in the same thread. Say why in a sentence each time.
5. **Review each part's fidelity round from the boards' text** (`Artifact read`), as
   `design/flows/15-institution-adopts-catalogue/round-2-review.md` did: one reviewer on the code (and the ask), one on
   the cast and the runbook, one on the design system. Put the decisions to the operator, then ask for one correction
   round.
6. **Record every round** in `design/flows/16-college-catalogue/` before moving on.

---

## 1. The ask (paste this first, with § 8 in the same message)

```text
FLOW 16 — The College keeps the national catalogue: its specialities, ladders, EPAs and curriculum versions

This is flow 16 of a RESTRUCTURE with UX in scope (design/BRIEF.md § 4). Flows 01 (the shell), 02 (sign-in and
  account), 03 (a registrar files an activity), 04 (an assessor's inbox), 05 (a registrar reads where she stands),
  06 (watching the programme), 07 (the review sitting), 08 (panels, what is due, the schedule), 09 (STARs, the
  register, appeals), 10 (MSF campaigns), 11 (invitation to admission), 12 (the institution's people), 13 (graduation
  and exit), 14 (data rights) and 15 (an institution adopts the catalogue) are designed and built; every page here sits
  inside the shell. Flow 15 rebuilt the EPA list, an EPA's page, Curricula, a curriculum's items, an item's own page and
  the scales list, for every caller, in an institution's words; this flow gives them the College's and the
  Administrator's words and redesigns the pages no flow has reached.
  This round is PART 1 of two, and it draws only part 1's screens (1–4 below): 2–3 STRUCTURES as wireframes for (A) the
  College's tree, Specialities > a speciality > its sub-specialities and their default ladders; 2–3 variations at FULL
  FIDELITY each for (B) Colleges and a College's record and (C) the entrustment scales list and a scale's page. I pick
  before you draw part 2 (screens 5–7), which I will ask for in this thread.
  The runbook steps follow below in this same message. The brand mark is at the URL on the next line: use it, never a
  drawn disc.
  MARK: <blob url to be filled>
  Every person, College, code, title, version, date and count comes from this message and the steps below; invent
  none. Where a value is not given, write a bracket ‹…›.

THE SCREENSHOTS AND THE STYLESHEET:
  - The key screenshots listed under ATTACHED are attached to THIS message (copies of design/baseline/, staged in
    design/upload/). SAY ON YOUR SPEC BOARD, FIRST, whether they arrived (as attachments, or read from a folder), which
    you opened and which you did not. If any did not arrive, draw nothing from memory in its place: write a bracket
    (flows 06, 12 and 13's lesson). The words in them are today's pages, defects included; they are not the target.
  - Write the round's shared stylesheet as a PROJECT FILE, project/f16r1.css (upload or write it as a file of the
    project, NOT as an asset, so the saved round holds it). Every board's head links ds/wombat/tokens.css,
    ds/wombat/components/bundle.css, then f16r1.css, in that order, and no /_blob/ stylesheet; a board's <style>
    holds body{margin:0} at most. Each NEW rule in f16r1.css names its row on the Spec's class table.

GOAL:
  The College's administrator (Dr Kruger, CPSA) must be able to:
    - keep the College's specialities and sub-specialities, and each sub-speciality's default ladder, and read the
      College's tree at a glance (which sub-specialities each speciality has, and the ladder each uses);
    - correct an EPA's wording, pause an EPA and restore it, and add one, told before he saves how far it reaches:
      which institutions have adopted a curriculum that holds it, and how many registrars it measures;
    - start a curriculum version held back, clone one, change its items, and open it for adoption, told in words what
      "open" and "held back" mean, what a clone copies (the College's items, never an institution's own), and that a
      target or period he changes is read live, closed periods included, in every institution that adopted it;
    - be offered nothing he cannot open: no Back to colleges, no link to a page the Administrator alone keeps; and know
      beforehand that the scales and the College's own record are the Administrator's, and where he reads a ladder by
      name.
  The Administrator (devadmin) must be able to keep the Colleges and the entrustment scales: create a College, refused
  plainly when its name or short code is taken; deactivate one only after a question that names it and says what it
  does; build a ladder rung by rung, told a rung by its own label ("5", never "level 6") and a refusal by the item's EPA
  and curriculum, never an id; delete a ladder only after a question, told what still uses it.
  What is wrong today (the attached screenshots show each; on the shell as built, 2026-11-02, unless dated otherwise):
  - The College's specialities list offers him "Back to colleges", a page that refuses him (1.12, 6.11). Every part-1
    page has Back links beside the trail, which flows 12, 13 and 15 dropped.
  - Deactivate on a College, a speciality and a sub-speciality is a red button that acts at once, with no question, and
    stays offered on the record it has just deactivated; Delete on a scale acts at once (6.8). What a deactivation does
    is not said: it stops new invitations naming the record and takes it out of an institution's adopted disciplines,
    nothing else.
  - The record forms print every required message twice (a summary and under the field), naming the model property
    ("The ShortCode field is required.", 6.1); every failed save of a College reads "A college with the same name or
    short code already exists."; a failed read or save prints the exception's text.
  - The scale's page refuses a rung a curriculum item needs with "Curriculum item 2 … requires level 6" (2026-09-26):
    the item by its id and the rung by its rank, where the College prints "5". An unknown scale id silently returns to
    the list; a specialities or sub-specialities list typed with another College's id reads as an empty list offering
    Create.
  - None of Colleges, Specialities or Sub-specialities stacks at 390 px. The scale's page has <h3> sections and no <h2>.
  - Part 2 (asked later): the curriculum's page is untouched by any flow: its box reads "Active" where every list reads
    "Open" / "Held back"; it shows no result after a create or a clone; its messages show twice and are not tied to
    their fields; its controls are 36–38 px at 390. A new Neonatology curriculum's items page says "Every national EPA
    of Neonatology is already on this curriculum", though Neonatology has none (6.13). Nothing anywhere says how many
    institutions or registrars a change reaches.

AUDIENCE:
  - CollegeAdmin: Dr Anton Kruger, College of Paediatricians of South Africa (CPSA). A few times a year; about one
    curriculum version a year; high stakes.
  - Administrator: devadmin ("Demo Administrator"). Keeps the Colleges and the scales.
  - Seen through the pause, on pages other flows built: Dr Mohammed Patel (Assessor), Dr Anele Dlamini (Trainee).
  - Desktop 1280×800 and phone 390×844.

THE RULES THE PAGES CARRY TODAY (real words; they are the data to draw with, and the rules hold whatever the shape):
  - Whose is what. Only the Administrator creates a College or changes a College's own record, and only he creates,
    edits or deletes a scale (scales are platform-wide). A College's CollegeAdmin keeps what the College owns: its
    specialities, sub-specialities (and each one's default ladder), EPAs and curricula. The College admin opens neither
    the Colleges pages nor the scales pages: "You cannot open this page", "Your role (College admin) does not open this
    page.", "If you need it for your work, ask the platform administrator.", Go to Home. He reads a ladder by name
    where the College uses it: a sub-speciality's default and each item's minimum level. An institution's own EPAs and
    items are not shown to the College.
  - Colleges (/admin/colleges): Name, Short code, Description, Status (Active, Inactive), Created; Edit and Specialities
    on each row ("Edit College of Paediatricians of South Africa"); Create college; empty "No colleges yet" / "Create the
    first college to start defining national specialities, EPAs and curricula.". A College: Name*, Short code*,
    Description, Active; Cancel, Deactivate, Save; "College saved.", "College deactivated."; a taken name or short code
    "A college with the same name or short code already exists.". An unknown id is not found.
  - Specialities (the College admin's menu item sends him to his College's list): subtitle "College: College of
    Paediatricians of South Africa (CPSA)"; trail Home › Specialities › College of Paediatricians of South Africa (to the
    Administrator: Home › Colleges › the College › Specialities); Name, Description, Status; Edit and Sub-specialities;
    Create speciality; empty "No specialities yet" / "Create the first speciality for this college.". A speciality:
    Name*, Description, Active; "Manage sub-specialities"; "Speciality saved.", "Speciality deactivated.".
  - Sub-specialities: subtitle "Speciality: Paediatrics"; Name, Description, Status; Edit; Create sub-speciality; empty
    "No sub-specialities yet" / "Create the first sub-speciality under this speciality.". A sub-speciality: Name*,
    Description, Active, and on an existing one Default entrustment scale ("No default — offer every scale", then every
    scale by name) with "Committee STARs for this programme's trainees are limited to this scale's levels.";
    "Sub-speciality saved.", "Sub-speciality deactivated.".
  - What Active does on a College, speciality or sub-speciality: an inactive one cannot be named by a new invitation,
    and an inactive speciality or sub-speciality is not among an institution's adopted disciplines. Nothing else reads
    it; deactivating cascades nothing.
  - Entrustment scales (/admin/entrustment-scales), to the Administrator: subtitle as built for him; caption as built;
    Name, Description, Levels, and Edit and Delete on each row; Create scale. Delete: refused while anything uses the
    scale, naming the first use found in this order: an MSF question, a pinned curriculum item, scored progress, a
    sub-speciality's default, a published form's binding, an entrustment decision ("This entrustment scale is the
    default scale of the sub-speciality "Adult Neurology" (Neurology), so it cannot be deleted. Change that
    sub-speciality's default entrustment scale to another scale, or to no default, first."); else "Entrustment scale
    deleted.". A scale needs at least two levels.
  - A scale (/admin/entrustment-scales/new, /{Id}): Name*, Description; Levels, each with Order (its rank, shown only
    here), Label and Description, with Up, Down and Remove, and Add level; Cancel, Save; "Entrustment scale saved.";
    refusals "Level labels must be unique within a scale." and the rung one above; a rename warns that a form bound by
    name loses it. A rung's label is not its rank: on the CPSA ladder 3 is split, so "4" is the fifth rung and "5" the
    sixth; everything that compares levels uses the rank.
  - Part 2's rules, for reading only now: an EPA is the College's to change; a pause ("Deactivate PAED-012?") stops it
    being chosen for a new activity and being a target on running programmes, keeps what it earned, and credits nothing
    completed meanwhile until it is restored, when it credits it ("EPA reactivated. 1 activity completed against it
    while it was inactive now counts towards progress."). A new or cloned curriculum version starts held back; only
    an open version can be adopted ("This curriculum version is not open for adoption. Its College has held it back,
    so nothing was adopted."); closing a version does not affect an institution that has adopted it.
  - Data volumes: none (CNSA before 6.3: no specialities; Neurology before 6.4: no sub-specialities); typical (CPSA:
    one speciality, two sub-specialities; three scales of 5–6 rungs); heavy (five Colleges; a speciality with six
    sub-specialities; a ladder of 8 rungs with long descriptors; a scale used by forty items).

CONTENT (real scenario values from the T392 replay, D = 2026-11-02):
  - Colleges: College of Paediatricians of South Africa (CPSA), "Constituent College of the Colleges of Medicine of
    South Africa; owns the national Paediatric EPA catalogue." (from 6.2: "… catalogue and its versions."); College of
    Neurologists of South Africa (CNSA), "Constituent College of the CMSA; owns the national Neurology catalogue.",
    created at 6.1; Demo College (DEMO-C). All Active.
  - CPSA: speciality Paediatrics ("Specialist training in Paediatrics."; from 6.11 "Specialist training in Paediatrics,
    and certificate training in its sub-specialities."); sub-specialities Paediatrics ("General paediatric specialist
    training programme.", default CPSA Paediatric Entrustment Scale v11.1) and Neonatology ("Certificate training in
    Neonatology.", created at 6.12, default the CPSA ladder). CNSA: Neurology ("Specialist training in Neurology.") and
    Adult Neurology ("Adult neurology specialist training programme."; "No default — offer every scale" until 6.7 sets
    the CNSA scale).
  - Scales: CPSA Paediatric Entrustment Scale v11.1 (six rungs labelled 1, 2, 3a, 3b, 4, 5, from "Not entrusted to
    act…" to "Supervises others…"); O-R Scale (5, the Demo College's); CNSA Neurology Entrustment Scale ("Supervision
    ladder for CNSA EPAs.": Observe only, Direct supervision, Indirect supervision, Distant supervision, Unsupervised
    practice, and from 6.6 Supervises others ("Supervises juniors performing the activity.")).
  - EPAs: PAED-001 to PAED-015, then PAED-016 "Transitioning adolescents with long-term conditions to adult care"
    (6.28); PAED-006 "Managing long-term health conditions (LTHCs) in children" (corrected at 6.14); PAED-012
    "Communicating with and counselling patients, caregivers and healthcare teams" (paused at 6.17, restored at 6.23).
  - Curricula: Paediatric EPA Curriculum 11.1 (effective 2026-01-01, 15 items, Open); 11.2 (cloned at 6.29, effective
    D, held back, 16 items after 6.30 with PAED-016 and PAED-011 at "2 per academic year", opened at 6.31); Neonatology
    EPA Curriculum 1.0 (effective D, 0 items, Held back). The clone form starts at "11.1-copy", from 2026-01-02.
  - The reach of a CPSA change: one institution, Kgosi Kgari Teaching Hospital (KGK), which adopted 11.1 (and 11.2 at
    6.32); on 11.1 at 6.17, three registrars in training (Dr Dlamini, Dr Mahlangu, Dr Ndlovu) and two whose programmes
    ended (Dr Molefe completed, Dr du Plessis withdrawn). KGK also keeps KGK-001 as its own item, which the College does
    not see.
  - Ids as replayed (they change on every replay: draw them as given): CPSA 2, CNSA 4; Paediatrics speciality 2,
    Neurology 3; sub-specialities Paediatrics 2, Adult Neurology 3, Neonatology 4; scales CPSA ladder 2, CNSA 3;
    curricula 11.1 2, Neonatology 1.0 3, 11.2 4; EPAs PAED-006 7, PAED-012 13, PAED-016 18.

WHAT FLOW 16 MAY CHANGE OF WHAT EARLIER FLOWS BUILT, AND WHAT IT MAY NOT:
  May not: the shell, the menus and Access denied (flow 01); the institution's view of the catalogue pages, their owner
    marks, "Set by the College.", the adoption on Curricula and its dialog, the item page's form and its record view,
    the one-shot result tokens (flow 15); My progress, the activity page and the picker (flows 03–05); the rules above
    (the College keeps its catalogue, the Administrator the Colleges and the scales; a pause keeps earned credit; a new
    version starts held back; an institution's own items are the institution's; an out-of-scope record is not found).
  May, for these pages: the College's tree and how its lists and records are reached; every part-1 page's header, Back
    links, table stacking, forms, refusals and results; Deactivate and Delete and their questions; the scale's page and
    its rung words; not-found and Unavailable states; the College admin's Home card; and (part 2) the College's and
    the Administrator's words on the EPA list, an EPA's page, Curricula, the items and an item's page; the curriculum's
    own page; where the reach of a change is told. Mark PROPOSED any rule change (a role newly allowed, a new route, a
    new action, a new refusal, a new confirmation, data a page does not read today): those are mine. The runbook's
    Expects change with the words.
  Say which of these each variation changes, and why.

SCREENS, in order. Part 1 (this round):
  1. The College admin's way in: Home's National catalogue card (6.10; it links Specialities, EPAs, Curricula and not
     Activity types, and says "Institutions adopt an active curriculum"); how his pages tell him beforehand that the
     scales and the College's own record are the Administrator's (1.15, 6.10 end on Access denied, flow 01's).
  2. The College's tree (structure A): his Specialities, CPSA's one speciality (1.12); Paediatrics' sub-specialities
     (1.13); a sub-speciality with its default ladder (1.14); a speciality edited and saved (6.11); Neonatology created
     with its default (6.12: before, created, the default saved and reopened); devadmin giving the CNSA its first
     speciality and sub-speciality from an empty list (6.3, 6.4) and Adult Neurology's default (6.7); a deactivation
     asked and done (Q3), and a deactivated record; another College's id typed (Q7); an unknown id; required fields
     empty; a failed save; loading; a load error; 390 px.
  3. Colleges and a College (variations B): the list of two, then three (1.2, 6.1); the CPSA's record (1.3); a create
     refused for a taken short code, then created (6.1); the description saved (6.2); required fields empty; deactivate
     asked and done (Q3); not found; loading; a load error; 390 px; five Colleges.
  4. Scales and a scale (variations C): the Administrator's list (1.4); the CPSA ladder's six rungs with Order beside
     each label (1.4); a ladder created, first refused for a duplicate label (6.5); a sixth rung added and saved (6.6);
     Delete asked, then refused naming what uses it (6.8); deleted; a rung a curriculum item needs, refused in the
     College's label and the item's EPA (Q6); a rename that unbinds a form; unknown id (Q7); loading; a load error; 390
     px; a ladder of 8.
  Part 2 (the next round, after my pick; listed so part 1 leaves room for it):
  5. A curriculum's own page (structures D): create Neonatology 1.0, held back (6.13); 11.1's page and its clone to 11.2
     (6.29); 11.2 opened for adoption (6.31); the reach told before a save (Q2); required fields empty; not found; 390.
  6. The EPA list and an EPA's page in the College's words (proposal E): the College's fifteen and sixteen (1.16, 6.28);
     PAED-006 corrected (6.14); PAED-012 paused with its reach told (6.17), restored (6.23); PAED-016 created (6.28);
     the Administrator's list.
  7. Curricula, the items and an item's page as the College keeps them (proposal F): 1.17, 1.18, 6.13's empty
     Neonatology items (Q14), 6.17's paused item, 6.29–6.31, A.7.10 at 390; T242's refusal naming the institution (Q10).

STEPS: 1.2–1.4, 1.12–1.18, 6.1–6.8, 6.10–6.14, 6.17–6.19, 6.23, 6.24, 6.28–6.31 and A.7.10 (33). Pasted verbatim below
  (Role / Route / Do / Expect). In them, D is the day the story is replayed. Where an Expect quotes today's wording or
  layout, that is today's page, not a requirement on the new one. 6.18 is flow 04's activity page and 6.19 and 6.24
  flows 03 and 05's pages (keep them; draw only what this flow changes); 1.15 and 6.10 end on flow 01's Access denied.

STATES TO SHOW:
  Every state named in SCREENS; loading for every page (the header and a skeleton, no action offered until the read is
  in); a load error (a sentence and Try again, never an exception's text, and no empty state under it); each
  confirmation, its cancel, and the result after it with the focus on it; a refusal beside the field or row that caused
  it, keeping what was typed; not found or Unavailable for every record page; 390 px for every page, long names
  wrapping, nothing sideways; the heavy volumes.

REQUIREMENTS FROM KNOWN DEFECTS:
  - T291 item 2: no page offers the College admin a link to a page that refuses him (Back to colleges; the
    redirect that sends a College admin with no College to Colleges). A page offers only what its target admits.
  - T264: Deactivate on a College, speciality and sub-speciality, and Delete on a scale, ask first in a ConfirmDialog
    naming the target ("Deactivate College of Neurologists of South Africa?"), say only what the deactivation does
    (above), open on Cancel; the trigger an outline button, offered only on a record stored as active; the result takes
    the focus.
  - T324: every required message names the field's label ("Enter a short code."), once, under its field, tied to it and
    focused; the scale's refusal names a rung by its label and an item by its EPA and curriculum ("PAED-001 on
    Paediatric EPA Curriculum (11.1) needs rung 5 …", PROPOSED words), never "Curriculum item 2" or "level 6".
  - T272 and T393: a failed read or save says what failed and offers Try again; "already exists" only for a real
    duplicate; no exception's text anywhere (the College pages, the specialities pages, the scale's page, the
    curriculum's page).
  - T326: (part 2) a curriculum whose discipline has no national EPA says so ("Neonatology has no national EPA yet …"),
    not that every EPA is already on it; part 1's and part 2's subtitles speak to the reader they have (the College
    admin's EPA and curriculum subtitles still speak as flow 15 left them).
  - T301 (its last half): (part 2) the curriculum page's box is "Open for adoption", with "An institution can adopt this
    version only while it is open. Closing it does not affect an institution that has already adopted it.", the same
    words as the lists' "Open" / "Held back".
  - T323 (gone; keep it): at 390 px nothing scrolls sideways and every control is 44 px, the curriculum's page included.
  - T329 / A6: a skeleton while loading, the alert on a load error with no empty state under it.
  - T242 (decided (c), flow 15's Q15): (part 2) the College's add of an EPA an institution holds as its own item stays
    refused, and the refusal names the institution (DATA: today it names none).
  - Every dialog names its target, its trigger is an outline button, it opens on Cancel, and the result takes the focus;
    a refusal keeps what was typed. One <h1>, then <h2> in order. Every list stacks at 390 (StackWide). People and
    Colleges by name; states by label.
  - Done since the old brief, keep them: T331 (Specialities stays lit on his College's list, flow 01); T323's item
    editor (its own page, flow 15); T301's held-back start and adoption refusal (flow 15, F5); "Open" / "Held back" on
    the lists (T9); the EPA's pause dialog and its result (flow 15, Q11); "Deactivate PAED-012?" opening on Cancel.

QUESTIONS THE DESIGN MUST ANSWER:
  1. (Mine) The College's tree: separate lists as today (Specialities, then a speciality's Sub-specialities), or one
     page for the College listing each speciality with its sub-specialities and their default ladders?
  2. (Mine) The reach: how many institutions have adopted a curriculum that holds this EPA or item, and how many
     registrars in training it measures, told before a save, a pause and an opening? Where (the dialog, the notice,
     the box), and in what words?
  3. (Mine) Deactivating a College, speciality or sub-speciality: what the question says it does, given that only
     invitations and an institution's adopted disciplines read it; or no Deactivate until a decision?
  4. (Mine; part 2) The curriculum's own page: one form with its release box and its clone, or the release and the
     clone as sections or pages of their own?
  5. (Mine; part 2) A result after a create and a clone, as an EPA's page does it (a one-shot token)?
  6. A rung and an item in a scale's refusal: the words.
  7. (Mine) An unknown or another College's id on every record and list page: the page's own Unavailable state?
  8. (Mine) The College admin and the scales: read-only access to the list (as an institution's administrator has), or
     Access denied as today, with his pages saying beforehand where he reads a ladder?
  9. (Mine) Back links on the part-1 pages: the trail alone, as flows 12, 13 and 15?
  10. (Mine; part 2) T242's refusal naming the institution: its words.
  11. Delete on a scale: the question; and Remove on a rung (unsaved until Save): no question?
  12. (Mine) The College admin's Home card: its words, and whether it links Activity types too.
  13. (Mine) What the build folds in.
  14. (Part 2) A curriculum whose discipline has no national EPA: its words and its link to EPAs.

CONSTRAINTS:
WOMBAT CONSTRAINTS (from design/BRIEF.md § 5; restated after the flow 01 pilot, 2026-09-27)
Stack: Blazor Server (.NET 10), Razor components. Signed-in pages are interactive: every click is a server round trip
  over SignalR, so prefer explicit actions and flag any per-keystroke behaviour (typeahead, drag, live filtering).
Static pages: sign-in, register, forgot-password, link account, access denied and not found for a signed-out visitor,
  the error page, /msf/respond and /portfolio/verify are plain server-rendered HTML with form posts. No client-side
  behaviour beyond a small script module; no live validation; the phone menu is CSS-only.
Security policy (CSP): fonts, scripts, styles and images from this site only (images may be data: URIs). No Google
  Fonts, no CDN, no Tailwind CDN, no jQuery, no inline scripts or onclick attributes, no third-party calls or avatars.
  A new typeface must be a self-hosted woff2 with a GPLv3-compatible licence (Source Sans 3 and Fraunces, OFL, ship).
No CSS framework: no Bootstrap, Tailwind, MudBlazor or Radzen classes. Every class is defined in app.css. Name every
  colour as an existing token (the design system's tokens.json) or a NEW token with its value; prefer the spacing scale
  xs 4, sm 8, md 16, lg 24, xl 32, 2xl 48 px.
Icons: Lucide line icons only, named by their Lucide name.
The shell is designed and built (flow 01): design the page body inside it, not a new frame. The sidebar shows the
  acting role's navigation (one role at a time; access is the union of the roles held), grouped above eight links, with
  My progress and My data rights under a rule; the top bar holds the person's name and Sign out; below 641 px a phone
  bar with a CSS-only menu. A new page names its owning list (its nav item lights, its breadcrumb follows it) and its
  title "<Page> · Wombat", the same words as its h1 and its nav label, in sentence case.
Components (compose from these; mark anything else NEW): PageHeader (the page's one h1, subtitle, the trail from the
  owning list, an optional icon, header actions), DataTable (.clinic-table in .table-container, PagerControls),
  FormField / FormActions (.form-container, .form-grid, .form-actions), DashboardCard (.detail-card in .dashboard-grid,
  loading and load error), StatePanel (loading / empty / error), Skeleton, Alert (success / info / warning / danger,
  text on the tint), ActionResult, ConfirmDialog (native <dialog>), badges (five tints), Icon, TrajectoryChart
  (hand-drawn SVG, no chart library), ReferenceBlock.
  Since flow 02 also: the auth card, PasswordField with its Show toggle and PasswordRules. Since flow 03:
  RefusalSummary, DataTable's stacked rows (Stack). Since flow 05: ISO dates, the training year's wording, the ended
  record (R5). Since flow 06: the role Homes; filters applied with Show. Since flow 07: the container-query stack.
  Since flow 08: one action a row, a choice of a few kinds as radios (choice-group), a form page of its own, the
  pager's focus on the list's heading. Since flow 09: a page of its own per list row; a dialog with a field. Since flow
  10: a create page of its own reached from Home's header action. Since flow 11: the list is the page and its create
  form a page of its own; DataTable's StackWide (stacked from 900 px down); a role chosen from described radios; the
  required mark; only the chosen role's fields drawn; refusals under their field, focused. Since flow 12: a detail
  page in one column of <h2> sections with no details-grid (the user page); a form in a note panel opened in place
  (Add a role…); a destructive action confirmed, its trigger an outline button. Since flow 13: a record card naming
  an end and linking the record (FormerTraineeRecordCard); a page's header action named for the person; an Alert with
  its first sentence bold over a details list (Verify's answer); an end section of its own apart from Save. Since
  flow 14: a ConfirmDialog in action mode opens with the focus on Cancel; a page's own Unavailable state for a record
  that does not exist or is not the caller's; a textarea's character counter; the refusal first in aria-describedby.
  Since flow 15: the owner mark in words under a title (.muted .text-sm); "Set by the College." where a row offers no
  change; a one-shot result token that survives a page change and leaves the address; a list stacked to 1024 px when
  its columns need it; an item's page of its own with its record view; "Open" / "Held back".
Already designed by flow 01, reuse them: the active nav item, the acting-role switch and its result alert, access
  denied, not found, the error page, the reconnect dialog and the in-app error bar. Designed by flow 02: session ended.
  Still to design where a flow meets them: field validation (invalid border plus a stripe, not colour alone; message
  under the field; the summary).
Content: sentence case everywhere; people by name as stored (no titles); states and types by label; times in South
  African time with the zone shown; an out-of-scope record is "not found", never "forbidden".
Accessibility: WCAG 2.1 AA; text 4.5:1, control borders and focus ring 3:1 (the sidebar's white ring included); targets
  ≥ 24 px, 44 px on a phone; focus moves to an action's result; every page works at 390 px with no sideways scroll;
  reduced motion honoured.
Viewports: 1280×800 and 390×844.
Also: each page has exactly one h1, and its headings are in order. A confirmation is flow 01's native <dialog>
  ConfirmDialog. A date is ISO (YYYY-MM-DD) on every screen. An EPA is written "PAED-001 — Title" everywhere
  (EpaOptionLabel). A rung is written by its label as the College prints it ("3a", "5"); its rank (Order) appears only
  on a scale's own page.

ASK:
  - Round 1 (part 1): 2–3 structures as wireframes for screen 2 (Dr Kruger at 1.12–1.14, 6.11 and 6.12; devadmin at
    6.3 and 6.4), each with its reasoning and its step count; 2–3 variations at full fidelity each for screen 3 (1.2,
    1.3, 6.1, 6.2, the deactivation) and for screen 4 (1.4, 6.5, 6.6, 6.8, the rung refusal); screen 1 drawn in the
    chosen frame of each structure. At 1280 and 390 px. With them, two boards of their own: a Spec board (FIRST, whether
    the screenshots arrived; then every component each variation uses, NEW marked; the DESIGN.md rules each changes;
    everything PROPOSED; the data a page would need that it does not read today, marked DATA) and a Steps board (each
    variation's journeys counted, presses + fields typed, at 1280 and 390, and which of part 1's 19 Expects (1.2–1.4,
    1.12–1.15, 6.1–6.8, 6.10–6.12) it would change). Stop and wait for my pick.
  - After the pick: part 1's chosen designs with every state above, at 1280 and 390 px, every confirmation, and where
    the focus lands after each action, with a Spec board (every NEW and changed component, its classes, the DESIGN.md
    text each changes, the tokens, the DATA, and every message by kind with its words) and a Steps board. Then part 2,
    as I ask.
  - Name every design-system component you use, and mark anything else NEW. Say which DESIGN.md rule a variation
    changes (§ Button system's destructive action; § Form system's refusals; § Table system's stacking; § Page-level
    patterns' Unavailable and Breadcrumbs; the NavMenu's owner table).
  - Mark PROPOSED anything not built: a route, an action, a confirmation, a refusal, a rule change, a role newly
    allowed.
  - Flag edge cases: a College with no CollegeAdmin (CNSA); a College admin whose account names no College; a speciality
    with no sub-speciality; a sub-speciality whose default ladder is deleted elsewhere (refused while it is the
    default); a ladder whose rung a pinned item needs; a College, speciality or sub-speciality deactivated while an
    institution has adopted its curriculum; two Colleges with similar names; a description of 2,000 characters.
  - Run an accessibility review against WCAG 2.1 AA: one h1, headings in order, every table captioned, every row
    action named for its row, focus after every action and confirmation, refusals tied to their fields
    (aria-describedby), targets ≥ 24 px (44 at 390).

ATTACHED: act-1/1.12-1-kruger-specialities.png, act-1/1.13-1-sub-specialities.png,
  act-1/1.14-1-sub-speciality-ladder.png, act-1/1.2-1-colleges-list.png, act-1/1.3-1-cpsa-record.png,
  act-1/1.4-2-cpsa-ladder.png, act-6/6.1-1-duplicate-refused.png, act-6/6.3-1-no-specialities.png,
  act-6/6.5-1-duplicate-labels-refused.png, act-6/6.8-1-delete-refused.png, act-6/6.10-1-kruger-home.png and
  act-6/6.10-2-college-record-refused.png, all from 2026-11-02 on the shell as built, with
  states/entrustment-scale-edit--rung-refused.png and states/college-edit--invalid.png from 2026-09-26, an older frame
  (a Title Case sidebar with Logout, an email in the top bar): read their bodies only. No attached capture shows a
  password.
```

---

## 2. The journey

| Step | Page | Who does | They must be able to see |
|---|---|---|---|
| 1.2 | `/admin/colleges` | devadmin (Administrator) | two rows, CPSA and the Demo College, Active, Edit and Specialities; Create college. **No stacking at 390** |
| 1.3 | `/admin/colleges` → `/admin/colleges/{Id:int}` | devadmin | "Edit college"; Name, Short code, Description, Active; Cancel, Deactivate, Save; leaving saves nothing. **Back to colleges beside the trail; a red Deactivate that asks nothing (T264)** |
| 1.4 | `/admin/entrustment-scales` → `/admin/entrustment-scales/{Id:int}` | devadmin | two scales with their level counts; the CPSA ladder's six rungs, Order beside each label. **`<h3>` sections; Remove a red button** |
| 1.12 | `/admin/specialities` → `/admin/colleges/{CollegeId:int}/specialities` | Dr Kruger (CollegeAdmin) | his College's list, one row; Specialities lit, the trail Home › Specialities › the College (T331, done). **"Back to colleges", a page that refuses him (T291 item 2, observed)** |
| 1.13 | `/admin/specialities/{SpecialityId:int}/sub-specialities` | Dr Kruger | "Speciality: Paediatrics"; one row; Create sub-speciality. **Back to specialities; no crumb of its own** |
| 1.14 | `/admin/specialities/{SpecialityId:int}/sub-specialities/{Id:int}` | Dr Kruger | the default ladder, the other choices, the help |
| 1.15 | `/admin/entrustment-scales` → `/access-denied` | Dr Kruger | Access denied (flow 01's). **Nothing beforehand says the scales are the Administrator's (Q8)** |
| 1.16 | `/admin/epas` | Dr Kruger | 15 rows, the College's alone, Active, Edit; Create EPA. **The editor's subtitle (T326's kind); the capture predates `50ca6470`** |
| 1.17 | `/admin/curricula` | Dr Kruger | 11.1, Open, Edit and Items; Create curriculum (flow 15's T9). **"Manage curriculum versions…"** |
| 1.18 | `/admin/curricula/{Id:int}/items` | Dr Kruger | the live-target warning; 15 rows; Edit and Remove… named for their EPA; the nothing-to-add note. **No reach in numbers (Q2)** |
| 6.1 | `/admin/colleges` → `…/new` → `…/{Id:int}` → list | devadmin | the duplicate refused, then the CNSA and three rows. **Required messages name the property, twice (T324, observed in the Actual); every failed save reads "already exists" (T272, code)** |
| 6.2 | `/admin/colleges` → `…/{Id:int}` → list | devadmin | "College saved."; the new description |
| 6.3 | `/admin/colleges/{CollegeId:int}/specialities` → `…/new` → `…/{Id:int}` → list | devadmin | an empty list inviting the first; Neurology, "Manage sub-specialities" |
| 6.4 | `/admin/specialities/{SpecialityId:int}/sub-specialities` → `…/new` → `…/{Id:int}` → list | devadmin | an empty list, then Adult Neurology, "No default — offer every scale" |
| 6.5 | `/admin/entrustment-scales` → `…/new` → `…/{Id:int}` → list | devadmin | "Level labels must be unique within a scale."; five rungs; three scales |
| 6.6 | `/admin/entrustment-scales` → `…/{Id:int}` → list | devadmin | six rungs; the list reads 6 |
| 6.7 | `/admin/specialities/{SpecialityId:int}/sub-specialities/{Id:int}` | devadmin | saved and still selected on reopening |
| 6.8 | `/admin/entrustment-scales` | devadmin | the refusal naming Adult Neurology. **Delete acts at once (T264, observed)** |
| 6.10 | `/account/login` → `/` → `/admin/colleges/{Id:int}` → `/access-denied` → `/admin/entrustment-scales/new` → `/access-denied` | Dr Kruger | his one-card Home; Access denied twice. **The card says "an active curriculum" and leaves out Activity types (code)** |
| 6.11 | `/admin/specialities` → `/admin/colleges/{CollegeId:int}/specialities` → `…/{Id:int}` → list | Dr Kruger | "Speciality saved."; the new description. **Back to colleges again (T291 item 2, observed)** |
| 6.12 | `/admin/colleges/{CollegeId:int}/specialities/{Id:int}` → sub-specialities → new → `{Id}` → list | Dr Kruger | Neonatology with the CPSA ladder as default; two rows |
| 6.13 | `/admin/curricula` → `/admin/curricula/new` → `/admin/curricula/{Id:int}` → list → items | Dr Kruger | held back from creation (flow 15's F5); "Held back", 0 items. **The box reads "Active"; no result after the create; "Every national EPA of Neonatology is already on this curriculum" (T326, observed)** |
| 6.14 | `/admin/epas` → `/admin/epas/{Id:int}` → `/admin/epas` | Dr Kruger | "EPA saved."; the new title read live |
| 6.17 | `/admin/epas` → `/admin/epas/{Id:int}` → `/admin/epas` → `/admin/curricula` → items | Dr Kruger | "Deactivate PAED-012?" opening on Cancel (flow 15's Q11); Inactive; the item "(inactive: not in force)". **"every institution that has adopted it", not how many (Q2)** |
| 6.18 | `/activities/inbox` → `/activities/{ActivityId:int}` | Dr Patel (Assessor) | "(no longer in use)"; completion allowed; its credit waits (flow 04's) |
| 6.19 | `/portfolio/progress` → `/portfolio/progress/{EpaId:int}` → `/activities/mine` → `/activities/{ActivityId:int}` → `/activities/new` | Dr Dlamini (Trainee) | PAED-012 under "No longer in use"; the picker without it (flows 05 and 03's) |
| 6.23 | `/admin/epas` → `/admin/epas/{Id:int}` → `/admin/epas` | Dr Kruger | "EPA reactivated. 1 activity … now counts towards progress."; Active |
| 6.24 | `/portfolio/progress` → `/portfolio/progress/{EpaId:int}` → `/activities/mine` → `/activities/{ActivityId:int}` | Dr Dlamini | PAED-012 back, the paused completion counted (flow 05's) |
| 6.28 | `/admin/epas` → `/admin/epas/new` → `/admin/epas/{Id:int}` → `/admin/epas` | Dr Kruger | CPSA's two sub-specialities offered; PAED-016 Active; no KGK-001 |
| 6.29 | `/admin/curricula` → items → `/admin/curricula/{Id:int}` → `/admin/curricula` | Dr Kruger | 15 items, no KGK item; "11.1-copy"; the clone held back. **No result after the clone (code)** |
| 6.30 | `/admin/curricula` → `/admin/curricula/{Id:int}/items` → `…/items/new` → items → `…/items/{ItemId:int}` → items | Dr Kruger | Add item's page offering PAED-016 alone; "Curriculum item added."; Edit PAED-011's page; "2 per academic year" (flow 15's F3) |
| 6.31 | `/admin/curricula` → `/admin/curricula/{Id:int}` → `/admin/curricula` | Dr Kruger | "Curriculum saved."; 11.1 and 11.2 Open, Neonatology Held back. **"tick Active" opens it: the box's word is not the list's (T301)** |
| A.7.10 | `/` → `/admin/epas` → `/admin/curricula` → `/admin/curricula/{Id:int}/items` → `…/items/{ItemId:int}` → items | Dr Kruger, 390 px | everything stacked, 44 px controls, nothing sideways (T323, gone). **Not on his path: the curriculum's page at 36–38 px (code)** |

## 3. States to design

Each screenshot is under `design/baseline/`. How to reach each is in
`execution/knowledge/scenario-paediatrics/states.md` (the line numbers below are its rows on 2026-11-04). The step
captures are from the T392 replay (2026-11-02), on the shell as built; the `states/` captures of part 1's pages are from
2026-09-26 (old frame: read their bodies only), and part 2's from 2026-11-02.

| Page | State | Screenshot | Who | Reached at (`states.md` line) |
|---|---|---|---|---|
| `/` | The College admin's Home; narrow | `act-6/6.10-1-kruger-home.png` (`states/home--college-admin.png`), `act-A/A.7.10-1-home-390.png` (`states/home--narrow-college-admin.png`) | Dr Kruger | Steps 6.10, A.7.10 (:142, :195) |
| `/access-denied` | Signed in | `act-1/1.15-1-kruger-access-denied.png`, `act-6/6.10-2-college-record-refused.png`, `act-6/6.10-3-scale-create-refused.png` (`states/access-denied--signed-in.png`) | Dr Kruger | Steps 1.15, 6.10 (:202) |
| `/admin/colleges` | Two; three | `act-1/1.2-1-colleges-list.png` (`states/colleges-list--two.png`), `act-6/6.1-3-three-colleges.png` (`states/colleges-list--three.png`) | devadmin | Steps 1.2, 6.1 (:719, :720) |
| `/admin/colleges` | Loading; narrow | `states/colleges-list--loading.png`, `states/colleges-list--narrow.png` | devadmin | held read; Step 6.2 at 390 (:721, :722) |
| `/admin/colleges/{Id:int}` | Edit; saved | `act-1/1.3-1-cpsa-record.png` (`states/college-edit--edit.png`), `act-6/6.2-1-cpsa-saved.png` (`states/college-edit--saved.png`) | devadmin | Steps 1.3, 6.2 (:723, :727) |
| `/admin/colleges/new` | Create; required empty; duplicate refused; created | `states/college-edit--create.png`, `states/college-edit--invalid.png`, `act-6/6.1-1-duplicate-refused.png` (`states/college-edit--duplicate.png`), `act-6/6.1-2-cnsa-created.png` | devadmin | Step 6.1 (:724–726) |
| `/admin/colleges/{Id:int}` | Deactivated (**the defect: no question, Deactivate still offered**); unknown id; narrow | `states/college-edit--deactivated.png`, `states/college-edit--not-found.png`, `states/college-edit--narrow.png` | devadmin | scratch; typed; Step 6.2 at 390 (:728–730) |
| `/admin/colleges/{CollegeId:int}/specialities` | One; none yet | `act-1/1.12-1-kruger-specialities.png` (`states/specialities-list--one.png`), `act-6/6.11-1-cpsa-specialities.png`, `act-6/6.3-1-no-specialities.png` (`states/specialities-list--empty.png`), `act-6/6.3-3-one-speciality.png` | Dr Kruger, devadmin | Steps 1.12, 6.11, 6.3 (:732, :733) |
| `/admin/colleges/{CollegeId:int}/specialities` | Another College's id (**the defect: an empty list with Create**); loading; narrow | `states/specialities-list--other-college.png`, `states/specialities-list--loading.png`, `states/specialities-list--narrow.png` | Dr Kruger | typed; held read; Step 6.11 at 390 (:734–736) |
| `/admin/colleges/{CollegeId:int}/specialities/{Id:int}` | Edit; saved; created | `states/speciality-edit--edit.png`, `act-6/6.11-2-paediatrics-saved.png` (`states/speciality-edit--saved.png`), `act-6/6.3-2-neurology-created.png` | Dr Kruger, devadmin | Steps 6.11, 6.3 (:737, :738) |
| `/admin/colleges/{CollegeId:int}/specialities/new` | Create; required empty | `states/speciality-edit--create.png`, `states/speciality-edit--invalid.png` | devadmin | Step 6.3 (:739, :740) |
| `/admin/colleges/{CollegeId:int}/specialities/{Id:int}` | Deactivated (the defect); unknown id; narrow | `states/speciality-edit--deactivated.png`, `states/speciality-edit--not-found.png`, `states/speciality-edit--narrow.png` | devadmin, Dr Kruger | scratch; typed; Step 6.11 at 390 (:741–743) |
| `/admin/specialities/{SpecialityId:int}/sub-specialities` | One; none yet; two | `act-1/1.13-1-sub-specialities.png` (`states/sub-specialities-list--one.png`), `act-6/6.4-1-no-sub-specialities.png` (`states/sub-specialities-list--empty.png`), `act-6/6.12-2-two-sub-specialities.png` (`states/sub-specialities-list--two.png`) | Dr Kruger, devadmin | Steps 1.13, 6.4, 6.12 (:744–746) |
| `/admin/specialities/{SpecialityId:int}/sub-specialities` | Loading; narrow | `states/sub-specialities-list--loading.png`, `states/sub-specialities-list--narrow.png` | Dr Kruger | held read; Step 6.12 at 390 (:747, :748) |
| `/admin/specialities/{SpecialityId:int}/sub-specialities/{Id:int}` | With a default; no default; default saved | `act-1/1.14-1-sub-speciality-ladder.png` (`states/sub-speciality-edit--edit.png`), `act-6/6.4-2-adult-neurology-created.png` (`states/sub-speciality-edit--no-default.png`), `act-6/6.7-2-default-reopened.png` (`states/sub-speciality-edit--saved.png`), `act-6/6.12-1-neonatology-default-saved.png` | Dr Kruger, devadmin | Steps 1.14, 6.4, 6.7, 6.12 (:749, :752, :753) |
| `/admin/specialities/{SpecialityId:int}/sub-specialities/new` | Create; required empty | `states/sub-speciality-edit--create.png`, `states/sub-speciality-edit--invalid.png` | devadmin | Step 6.4 (:750, :751) |
| `/admin/specialities/{SpecialityId:int}/sub-specialities/{Id:int}` | Deactivated (the defect); unknown id; narrow | `states/sub-speciality-edit--deactivated.png`, `states/sub-speciality-edit--not-found.png`, `states/sub-speciality-edit--narrow.png` | devadmin, Dr Kruger | scratch; typed; Step 6.12 at 390 (:754–756) |
| `/admin/entrustment-scales` | The Administrator's; three; six levels | `act-1/1.4-1-scales-list.png` (`states/entrustment-scales-list--administrator.png`), `act-6/6.5-3-three-scales.png`, `act-6/6.6-2-list-six-levels.png` | devadmin | Steps 1.4, 6.5, 6.6 (:834) |
| `/admin/entrustment-scales` | Delete refused (**no question first**); deleted; loading; load error; narrow | `act-6/6.8-1-delete-refused.png` (`states/entrustment-scales-list--delete-refused.png`), `states/entrustment-scales-list--deleted.png`, `states/entrustment-scales-list--loading.png`, `states/entrustment-scales-list--load-error.png`, `states/entrustment-scales-list--narrow.png` | devadmin, Prof Mbatha | Step 6.8; scratch; held read (:836–840) |
| `/admin/entrustment-scales/{Id:int}` | Six rungs; saved | `act-1/1.4-2-cpsa-ladder.png` (`states/entrustment-scale-edit--edit.png`), `act-6/6.6-1-sixth-level-saved.png` (`states/entrustment-scale-edit--saved.png`) | devadmin | Steps 1.4, 6.6 (:841, :844) |
| `/admin/entrustment-scales/new` | Create; duplicate labels refused; created | `states/entrustment-scale-edit--create.png`, `act-6/6.5-1-duplicate-labels-refused.png` (`states/entrustment-scale-edit--duplicate.png`), `act-6/6.5-2-cnsa-scale-created.png` | devadmin | Step 6.5 (:842, :843) |
| `/admin/entrustment-scales/{Id:int}` | A rung still needed (**"Curriculum item N … level 6"**); unknown id (**silently back to the list**); narrow | `states/entrustment-scale-edit--rung-refused.png`, `states/entrustment-scale-edit--unknown.png`, `states/entrustment-scale-edit--narrow.png` | devadmin | scratch; typed; Step 6.6 at 390 (:845–847) |
| `/admin/epas` | The College's fifteen; one inactive; sixteen | `act-1/1.16-1-college-epas.png` (`states/epas-list--college.png`), `act-6/6.17-3-epa-list-inactive.png` (`states/epas-list--inactive.png`), `act-6/6.28-2-kruger-epa-list.png` (all before `50ca6470`'s widths) | Dr Kruger | Steps 1.16, 6.17, 6.28 (:758, :760) |
| `/admin/epas` | The Administrator's; loading; load error; narrow | `states/epas-list--administrator.png`, `states/epas-list--loading.png`, `states/epas-list--load-error.png`, `act-A/A.7.10-2-epas-390.png` (`states/epas-list--narrow.png`) | devadmin, Dr Kruger | after 6.28; held read; Step A.7.10 (:762–765) |
| `/admin/epas/{Id:int}` | Edit; saved; deactivate dialog; deactivated; reactivated | `states/epa-edit--edit.png`, `act-6/6.14-1-paed-006-saved.png` (`states/epa-edit--saved.png`), `act-6/6.17-1-deactivate-dialog.png` (`states/epa-edit--deactivate-dialog.png`), `act-6/6.17-2-deactivated.png` (`states/epa-edit--deactivated.png`), `act-6/6.23-1-reactivated.png` (`states/epa-edit--reactivated.png`) | Dr Kruger | Steps 6.14, 6.17, 6.23 (:766–770) |
| `/admin/epas/new` | Create, the College; created; required empty; duplicate; save failed | `states/epa-edit--create-national.png`, `act-6/6.28-1-paed016-saved.png`, `states/epa-edit--invalid.png`, `states/epa-edit--duplicate.png`, `states/epa-edit--save-failed.png` | Dr Kruger | Step 6.28; scratch (:773–775, :777) |
| `/admin/epas/{Id:int}` | Loading; load error; narrow | `states/epa-edit--loading.png`, `states/epa-edit--load-error.png`, `states/epa-edit--narrow.png` | Dr Kruger | held read; Step 6.14 at 390 (:781–783) |
| `/admin/curricula` | The College's; open and held back; held back after a create and a clone | `act-1/1.17-1-college-curricula.png` (`states/curricula-list--college.png`), `act-6/6.31-2-curricula-list.png` (`states/curricula-list--versions.png`), `act-6/6.13-3-curricula-list.png`, `act-6/6.29-3-list-11.2-inactive.png` | Dr Kruger | Steps 1.17, 6.31, 6.13, 6.29 (:785, :787) |
| `/admin/curricula` | The Administrator's; narrow | `states/curricula-list--administrator.png`, `act-A/A.7.10-3-curricula-390.png` (`states/curricula-list--narrow.png`) | devadmin, Dr Kruger | after 6.38; Step A.7.10 (:795, :800) |
| `/admin/curricula/new` | Create; required fields empty (**twice, the property's name**) | `states/curriculum-edit--create.png`, `states/curriculum-edit--invalid.png` | Dr Kruger | Step 6.13 (:801, :802) |
| `/admin/curricula/{Id:int}` | Created, held back (**no result**); with the clone card; cloned (**no result**); opened | `act-6/6.13-1-neonatology-curriculum-created.png` (`states/curriculum-edit--saved.png`), `states/curriculum-edit--clone.png`, `act-6/6.29-2-cloned-11.2-active.png` (`states/curriculum-edit--cloned.png`), `act-6/6.31-1-11.2-saved-active.png` | Dr Kruger | Steps 6.13, 6.29, 6.31 (:803–805) |
| `/admin/curricula/{Id:int}` | Unknown id; narrow (**36–38 px controls**) | `states/curriculum-edit--not-found.png`, `states/curriculum-edit--narrow.png` | Dr Kruger | typed; Step 6.31 at 390 (:806, :807) |
| `/admin/curricula/{Id:int}/items` | The College's items; no items (**T326's false line**); an inactive EPA | `act-1/1.18-1-college-items.png` (`states/curriculum-items-edit--college.png`), `act-6/6.13-4-items-nothing-to-add.png` (`states/curriculum-items-edit--no-items.png`), `act-6/6.17-4-items-paused.png` (`states/curriculum-items-edit--inactive.png`) | Dr Kruger | Steps 1.18, 6.13, 6.17 (:808, :810, :811) |
| `/admin/curricula/{Id:int}/items` | 11.1 with the add offered; added; saved | `act-6/6.29-1-kruger-11.1-items.png`, `act-6/6.30-2-paed016-added.png`, `act-6/6.30-4-paed011-saved.png` (`states/curriculum-items-edit--saved.png`) | Dr Kruger | Steps 6.29, 6.30 (:815) |
| `/admin/curricula/{Id:int}/items` | Loading; load error; narrow; 768; 901 | `states/curriculum-items-edit--loading.png`, `states/curriculum-items-edit--load-error.png`, `act-A/A.7.10-4-items-390.png` (`states/curriculum-items-edit--narrow.png`), `act-A/A.7.10-7-items-768.png`, `act-A/A.7.10-7-items-901.png` | Dr Kruger | held read; Step A.7.10 (:821–823) |
| `/admin/curricula/{Id:int}/items/new`, `/{ItemId:int}` | The College's add; the College's edit; save refused; narrow; back focused | `act-6/6.30-1-add-paed016-form.png` (`states/curriculum-item-page--add-national.png`), `act-6/6.30-3-paed011-edit-row.png` (`states/curriculum-item-page--edit-national.png`), `states/curriculum-item-page--save-refused.png`, `act-A/A.7.10-5-item-editor-390.png` (`states/curriculum-item-page--narrow.png`), `act-A/A.7.10-6-items-back-focus-390.png` | Dr Kruger | Steps 6.30, A.7.10; scratch (:824, :828, :831, :833) |

Not captured, so describe them in words (`states.md`; `coverage.md` § Flows and states not played):
- Never built: a question before Deactivate on a College, speciality or sub-speciality, and before Delete on a scale
  (T264); the curriculum page's "Open for adoption" box (T301); a result after a curriculum's create or clone; the
  reach of a change in numbers (Q2); an Unavailable state for a College, speciality, sub-speciality or scale (Q7); T242's
  refusal naming the institution (Q10).
- Unplayed by the story: deactivating a College, speciality or sub-speciality (`coverage.md:209`, whose "nothing reads
  their `IsActive`" this brief corrects: invitations and an institution's adopted disciplines do,
  `A/Invitations/InvitationRules.cs:116`, `:126`, `:137`;
  `A/Institutions/Queries/GetAdoptedDisciplinesForInstitution/GetAdoptedDisciplinesForInstitutionQuery.cs:75`, `:86`);
  a scale edit refused for a rung an item needs, and a rename that unbinds a form (`coverage.md:213`); the College's
  national add of an EPA an institution holds as its own item (`coverage.md:214`).
- Load errors: Colleges, Specialities and Sub-specialities print the exception's text (code); the record forms and the
  curriculum's page put it in a danger alert (code).

**Belongs to other flows:** flow 01: Access denied (1.15, 6.10). Flow 04: Dr Patel's inbox and activity page
(`act-6/6.18-…`). Flows 03 and 05: Dr Dlamini's progress, PAED-012's page, My activities and the picker (`act-6/6.19-…`,
`act-6/6.24-…`). Flow 15: the institution's view of every part-2 page (`states/epas-list--institution`, `--local`,
`--empty`; `epa-edit--create-local`, `--created`, `--record`, `--not-found`, `--local-deactivated`,
`--local-reactivated`, `--discipline-refused`; every `curricula-list--` state of an institution or the adoption;
`curriculum-items-edit--institution`, `--own-form`, `--own-added`, `--new-version`, `--back`, `--superseded`,
`--remove-dialog`, `--removed`, `--not-found`; `curriculum-item-page--add-own`, `--no-rung`, `--epa-refused`,
`--edit-own`, `--record`, `--unavailable`; `entrustment-scales-list--read-only`). Flow 17: the College's activity type
(6.14a).

## 4. Attach

Paths are relative to `design/baseline/`. The key screenshots were read from their step's Actual; `1.16-1` and
`states/epas-list--local.png` were opened on 2026-11-04 (the EPA column's old widths; Create EPA under the
institution's long subtitle). None shows a password or a one-time link.

**Key screenshots (attach these first, with § 1; part 1):**
1. `act-1/1.12-1-kruger-specialities.png`: "Back to colleges" offered to the College admin
2. `act-1/1.13-1-sub-specialities.png`: a speciality's sub-specialities, Back to specialities, no crumb of its own
3. `act-1/1.14-1-sub-speciality-ladder.png`: the default ladder and its help
4. `act-1/1.2-1-colleges-list.png`: Colleges
5. `act-1/1.3-1-cpsa-record.png`: a College's record, a red Deactivate beside Save
6. `act-1/1.4-2-cpsa-ladder.png`: the CPSA ladder, Order beside each label, Remove in red
7. `act-6/6.1-1-duplicate-refused.png`: the College refused for a taken short code
8. `act-6/6.3-1-no-specialities.png`: an empty specialities list
9. `act-6/6.5-1-duplicate-labels-refused.png`: a ladder's refusal
10. `act-6/6.8-1-delete-refused.png`: Delete acted at once, then refused
11. `act-6/6.10-1-kruger-home.png`: the College admin's one card
12. `act-6/6.10-2-college-record-refused.png`: Access denied on the College's own record
13. `states/entrustment-scale-edit--rung-refused.png` (2026-09-26): "Curriculum item N … requires level 6"
14. `states/college-edit--invalid.png` (2026-09-26): "The ShortCode field is required.", twice

**Part 1, as the chat asks:** `act-1/1.15-1-kruger-access-denied.png`, `act-1/1.4-1-scales-list.png`,
`act-6/6.1-2-cnsa-created.png`, `act-6/6.1-3-three-colleges.png`, `act-6/6.2-1-cpsa-saved.png`,
`act-6/6.3-2-neurology-created.png`, `act-6/6.4-1-no-sub-specialities.png`, `act-6/6.4-2-adult-neurology-created.png`,
`act-6/6.5-2-cnsa-scale-created.png`, `act-6/6.6-1-sixth-level-saved.png`, `act-6/6.7-2-default-reopened.png`,
`act-6/6.10-3-scale-create-refused.png`, `act-6/6.11-1-cpsa-specialities.png`, `act-6/6.11-2-paediatrics-saved.png`,
`act-6/6.12-1-neonatology-default-saved.png`, `act-6/6.12-2-two-sub-specialities.png`, `act-A/A.7.10-1-home-390.png`, and
(old frame) `states/colleges-list--loading.png`, `states/colleges-list--narrow.png`, `states/college-edit--deactivated.png`,
`states/college-edit--not-found.png`, `states/college-edit--narrow.png`, `states/specialities-list--other-college.png`,
`states/specialities-list--narrow.png`, `states/speciality-edit--deactivated.png`, `states/sub-specialities-list--narrow.png`,
`states/sub-speciality-edit--narrow.png`, `states/entrustment-scale-edit--unknown.png`,
`states/entrustment-scale-edit--narrow.png`, and (2026-11-02) `states/entrustment-scales-list--deleted.png`,
`states/entrustment-scales-list--narrow.png`.

**Part 2, with its ask:** `act-1/1.16-1-college-epas.png` (before `50ca6470`), `act-1/1.17-1-college-curricula.png`,
`act-1/1.18-1-college-items.png`, `act-6/6.13-1-neonatology-curriculum-created.png`,
`act-6/6.13-4-items-nothing-to-add.png`, `act-6/6.14-1-paed-006-saved.png`, `act-6/6.17-1-deactivate-dialog.png`,
`act-6/6.17-4-items-paused.png`, `act-6/6.23-1-reactivated.png`, `act-6/6.28-1-paed016-saved.png`,
`act-6/6.29-1-kruger-11.1-items.png`, `act-6/6.29-2-cloned-11.2-active.png`, `act-6/6.30-1-add-paed016-form.png`,
`act-6/6.30-3-paed011-edit-row.png`, `act-6/6.31-1-11.2-saved-active.png`, `act-6/6.31-2-curricula-list.png`,
`act-A/A.7.10-2-epas-390.png`, `act-A/A.7.10-4-items-390.png`, `act-A/A.7.10-5-item-editor-390.png`, and
`states/curriculum-edit--invalid.png`, `states/curriculum-edit--clone.png`, `states/curriculum-edit--narrow.png`,
`states/epas-list--administrator.png`, `states/curricula-list--administrator.png`,
`states/epas-list--local.png` (Create EPA under a long subtitle).

That is 14 key files, 31 more for part 1 and 25 for part 2: 70.

**Do not attach:** `act-6/6.13-2-saved-inactive.png` (2026-10-30, a step that no longer unticks anything); the duplicates of a step capture (`states/colleges-list--two`, `--three`; `college-edit--edit`, `--duplicate`, `--saved`; `specialities-list--one`, `--empty`; `speciality-edit--saved`; `sub-specialities-list--one`, `--empty`, `--two`; `sub-speciality-edit--edit`, `--no-default`, `--saved`; `entrustment-scales-list--administrator`, `--delete-refused`; `entrustment-scale-edit--edit`, `--duplicate`, `--saved`; `epas-list--college`, `--inactive`, `--narrow`; `epa-edit--saved`, `--deactivate-dialog`, `--deactivated`, `--reactivated`; `curricula-list--college`, `--versions`, `--narrow`; `curriculum-edit--saved`, `--cloned`; `curriculum-items-edit--college`, `--no-items`, `--inactive`, `--narrow`, `--saved`; `curriculum-item-page--add-national`, `--edit-national`, `--narrow`; `home--college-admin`, `home--narrow-college-admin`; `access-denied--signed-in`); other flows' pages (`act-6/6.18-1-patel-inbox-paused.png`, `act-6/6.18-2-completed-credited-nothing.png`, flow 04's; `act-6/6.19-1-dlamini-progress.png` to `act-6/6.19-5-dlamini-mini-cex-paused.png` and `act-6/6.24-1-dlamini-progress.png` to `act-6/6.24-4-dlamini-mini-cex-credited.png`, flows 03 and 05's; flow 15's institution states named in § 3).

## 5. Known problems this design must solve

The evidence column is for the operator and for Claude Code. Attach only what § 4 lists. Code is cited at `fdbf2ff0`.
"Observed" is seen in a capture or a replay's Actual; "code" is read in the source and not replayed; "inferred" is
neither.

| Task | What it means for the design | Evidence |
|---|---|---|
| **T291** (P3, queued; item 2 here; items 3 and 6 are flow 17's, item 8 flow 18's) | Q9. **Item 2, still true:** `SpecialitiesList.razor:11` renders Back to colleges for every caller its `AdministratorOrCollegeAdmin` policy admits, while `CollegesList.razor:2` admits only an Administrator (observed 1.12, 6.11: "known-task" gaps). **New beside it (code):** `MySpecialitiesRedirect.razor:26–32` sends a caller with no College claim to `/admin/colleges`, so a College admin whose account names no College lands on Access denied from his own menu. | `act-1/1.12-1…`, `act-6/6.11-1…` |
| **T264** (P3, queued; the College, speciality, sub-speciality and scale halves) | Q3, Q11. Still true: Deactivate is a `btn btn-danger` with no ConfirmDialog, rendered whenever `!IsNew`, whatever the stored state (`CollegeEdit.razor:67–70`, `SpecialityEdit.razor:66–69`, `SubSpecialityEdit.razor:74–77`); Delete on the scales list is a `btn-danger btn-sm` that sends at once (`EntrustmentScalesList.razor:86`, `:130–138`; observed 6.8). The scale's Remove level is a `btn-danger` too (`EntrustmentScaleEdit.razor:97`; it changes nothing until Save). The model is EpaEdit's (an outline trigger offered on a stored-active record, a dialog naming it, gone once done), and T302's on the institution page. | `act-6/6.8-1…`, `act-1/1.3-1…`, `states/college-edit--deactivated.png` |
| **T324** (P3, queued; the College forms and the scale) | Q6. Still true: `[Required]` with no `ErrorMessage` or `[Display]` on the College, speciality and sub-speciality models (`CollegeEdit.razor:206–212`, `SpecialityEdit.razor:216–218`, `SubSpecialityEdit.razor:231–233`) and on the curriculum's (`CurriculumEdit.razor:269–278`, `:287–289`; a `[Range]` on `SubSpecialityId` would read "The field SubSpecialityId must be between 1 and 2147483647.", inferred), each shown twice (summary and field). The rung refusal: `A/Epas/EntrustmentScaleReferences.cs:219–221`, "Curriculum item {item.Id} is pinned to this entrustment scale and requires level {Order}…" (observed in `states/…--rung-refused`, 2026-09-26). The EPA's page already uses label words ("Enter a code.", `A/Epas/CreateEpa.cs:40–45`). | `states/college-edit--invalid.png`, `states/entrustment-scale-edit--rung-refused.png`, `act-6/6.1-…` (Actual) |
| **T393** (proposed; an empty template, its title only) | Still true: `CurriculumEdit.razor:193` (load), `:231` (save), `:259` (clone) assign `exception.Message` to a danger alert; `EntrustmentScaleEdit.razor:176` (load) and `:262` (save, through `RefusalText.Of`, which prints any exception's message but a validator's). Flow 15's build review named the same lines (`15-institution-adopts-catalogue/build-review-pages.md:73`). | — (code) |
| **T272** (P3, queued; the College handlers and the part-1 pages) | Still true: `A/…/CreateCollege.cs:57–60` and `UpdateCollege.cs:60–63` turn every `DbUpdateException` into "A college with the same name or short code already exists."; the part-1 pages print `exception.Message` on every load and save failure (`CollegesList.razor:64`, `CollegeEdit.razor:128`, `:165`, `:194`, `SpecialitiesList.razor:84`, `SpecialityEdit.razor:137`, `:175`, `:204`, `SubSpecialitiesList.razor:68`, `SubSpecialityEdit.razor:151`, `:190`, `:219`). **Gone:** the scales list's raw text (fixed words since flow 15, `EntrustmentScalesList.razor:123–124`, `:142–151`); CreateEpa's duplicate is now only a real unique violation (`CreateEpa.cs:152–156`). | `act-6/6.1-1…` |
| **T326** (P3, queued; the no-items half and the College's subtitles) | Q14. Still true: `CurriculumItemsWords.NothingToAdd` (`W/Admin/Curricula/CurriculumItemsWords.cs:93–97`) says "Every national EPA of {sub-speciality} is already on this curriculum…" whenever the add picker is empty, with no branch for a discipline that has no national EPA (observed 6.13: a "known-task" gap); the item page uses it too (`CurriculumItemPage.razor:77`). **Gone:** the empty table under it (flow 15 draws no table at "Items (0)"). The same kind: the College admin's subtitles still speak as before (`EpaWords.cs:26`, `CurriculaWords.cs:15`), and the curriculum's page's "Set the scope, version window, and lifecycle of the curriculum." | `act-6/6.13-4…` |
| **T301** (P3, queued; its first item remains) | Q4. **Done by flow 15:** a new and a cloned version start held back (`A/…/CreateCurriculum.cs:73–75`, `src/Wombat.Domain/…/Curriculum.cs:35–37`; observed 6.13, 6.29: "T301 no longer occurs"); Adopt refuses a held-back version (`AdoptCurriculum.cs:71`, `:108–111`); the lists read "Open" / "Held back" (`CurriculaWords.cs:62`). **Still open:** the curriculum's box is labelled "Active" with no help (`CurriculumEdit.razor:72–78`), so 6.31's Do says "tick Active" and the page and the list disagree. | `act-6/6.31-1…`, `act-6/6.13-1…` |
| **T323** (P3, queued; the item editor's half) | **Gone** (observed A.7.10: "T323's item-editor half no longer occurs"; the item has its own page, one field a row, 44 px). Flow 15 also found and fixed the items table overflowing from 901 to about 990 px (`e10a5f22`). **The same kind, still true:** the curriculum's own page has no page class the 44 px phone rule reaches (it applies under `.people-page`/`.user-page`, app.css:6595–6603), so its buttons are 36 px (`.btn`, app.css:466) and its inputs 38 px (`.form-control`, `.form-select`, app.css:612, :629) at 390 (code; inferred from CSS, not measured). None of Colleges, Specialities or Sub-specialities stacks (no `Stack`/`StackWide`; code). | `states/curriculum-edit--narrow.png`, `states/colleges-list--narrow.png` |
| **T331** (P3, **done** in `b347e11c`) | Keep: Specialities stays lit on his College's list and its pages (`NavOwners.cs:84–88`, `:142–145`; observed 1.12). | `act-1/1.12-1…` |
| **T329** (A6; part 1's lists the same kind) | The EPA list's skeleton beside the error is gone (flow 15, `EpasList.razor:142–148`). Still the old kind: Colleges, Specialities and Sub-specialities have `LoadError` but print the exception (above); the curriculum's page has a bare `StatePanel` with no loading words (`CurriculumEdit.razor:36`). | `states/colleges-list--loading.png` |
| **T242** (P3, queued; decided (c) in flow 15's Q15, drawn here) | Q10. The refusal today names no institution: "This curriculum already contains the selected EPA, as an institution's own item. …" (`A/Curricula/CurriculumAdminScope.cs:286–287`), as a field refusal on EPA (`:279–282`); the picker leaves such an EPA out, so only a stale page or a race reaches it (code). Naming the institution is DATA. | — (code) |
| **New: no result after a create or a clone** (code; T392's replay left it unfiled) | Q5. `CurriculumEdit.razor:219–221` (create) and `:254–255` (clone) navigate to the new curriculum with `forceLoad` and no token, and `OnParametersSetAsync` clears the status (`:154`), so the arriving page says nothing; only a save of an existing curriculum reads "Curriculum saved." (`:226–227`). EpaEdit's `?created` (`CatalogueLinks`) is the model. | `act-6/6.13-1…`, `act-6/6.29-2…` |
| **New: the curriculum form's messages twice and unlinked** (code; T392 left it unfiled) | `ValidationSummary` at `CurriculumEdit.razor:42` and `:98` over `ValidationMessage`s under Name, Version and the clone's Version (`:56`, `:61`, `:103`); no `aria-describedby`, `aria-invalid` or focus (code; the states row :802 quotes "The Name field is required." and "The Version field is required." in a summary). Flow 11's rule: a refusal under its field, focused. | `states/curriculum-edit--invalid.png` |
| **New: Create EPA under the long subtitle** (observed; T392 left it unfiled) | `PageHeader` wraps its actions under the title block when the row is too narrow (`header-container`, app.css:223–232). Observed on the institution's list at 1280 (`states/epas-list--local.png`: Create EPA below "The EPAs of the curricula Kgosi Kgari Teaching Hospital has adopted. …"); the College admin's shorter subtitle keeps it beside at 1280 (observed `1.16-1`); the Administrator's at 1280 and anyone's at 768 not checked (inferred to wrap). A header rule, not this list's alone. | `states/epas-list--local.png`, `act-1/1.16-1…` |
| **New: three EPA-list states on the old column widths** (observed; T392 left it unfiled) | `states/epas-list--college`, `--institution` and `--inactive` were taken at 21:51 on 2026-11-02, before `50ca6470` (22:23) gave the EPA column 45% from 1025 px (app.css:7495–7497) and stacked the list to 1024 px; so were the step captures `1.16-1`, `6.14-2`, `6.17-3`, `6.23-2`, `6.28-2` (titles over three to six lines, observed in `1.16-1`). A re-take, not a design fault; this flow's replay re-takes them. | `act-1/1.16-1…` |
| **New: a list typed with another College's id reads as empty** (code) | Q7. `SpecialitiesList` with the Demo College's id, as Dr Kruger: `GetCollegeById` returns null (`A/…/GetColleges.cs:57–60`) and the list query an empty array, so the page draws "No specialities yet" with Create speciality and Back to colleges (observed `states/specialities-list--other-college.png`, old frame); `SubSpecialitiesList` the same for a foreign or unknown speciality (code). An unknown scale id returns silently to the list (`EntrustmentScaleEdit.razor:154–157`); the record pages go to `/not-found` (`CollegeEdit.razor:112–115`, `SpecialityEdit.razor:108–125`, `SubSpecialityEdit.razor:117–138`, `CurriculumEdit.razor:176`), not flow 14's Unavailable. | `states/specialities-list--other-college.png`, `states/entrustment-scale-edit--unknown.png` |
| **New: what a deactivation does** (code; corrects the old brief and `coverage.md:209`) | Q3. An inactive College, speciality or sub-speciality cannot be named by a new invitation (`A/Invitations/InvitationRules.cs:116`, `:126`, `:137`), and an inactive speciality or sub-speciality leaves an institution's adopted disciplines (`GetAdoptedDisciplinesForInstitutionQuery.cs:75`, `:86`), which feed Create EPA's picker and Home's card (inferred from flow 15's use). Nothing cascades (the three deactivate handlers set one flag). A dialog may say this and nothing more. | — (code) |
| **New: the reach is told in words only** (code) | Q2. No page shows how many institutions or registrars a change reaches: the items' notice and the pause dialog say "every institution that has adopted it" (`CurriculumItemsWords.cs:70–71`, `EpaWords.cs:171–180`); the only registrar count is an institution's superseded subtitle (`CurriculumItemsWords.cs:62–66`). The old brief's question 2. | `act-6/6.17-1…`, `act-1/1.18-1…` |
| **New: the scale's page on the old shapes** (code) | No `EditForm`; `<h3>` "Scale details" and "Levels" under the `<h1>` with no `<h2>` (`EntrustmentScaleEdit.razor:43`, `:57`); Back to scales beside the trail (`:15`). | `act-1/1.4-2…` |
| **New: Back links on every part-1 page and the curriculum's** (code) | Back to colleges (`CollegeEdit.razor:12`), Back to specialities (`SpecialityEdit.razor:12`; `SubSpecialitiesList.razor:10–13`), Back to sub-specialities (`SubSpecialityEdit.razor:13`), Back to scales (`EntrustmentScaleEdit.razor:15`), Back to curricula and Manage items (`CurriculumEdit.razor:12–20`), beside the trail, which flows 12, 13 and 15 dropped. Sub-specialities draws no crumb of its own (`SubSpecialitiesList.razor:8`). | `act-1/1.13-1…` |
| **New: the College admin's Home card** (code) | "Your college's specialities, its EPAs and its curricula. Institutions adopt an active curriculum to train against it." (`CollegeAdminDashboard.razor:9`): "active" where the lists say "Open"; it links Specialities, EPAs and Curricula, not Activity types, which his menu holds (`:11–13`; `NavItems.cs:160`). | `act-6/6.10-1…` |
| **New: Access denied names neither the page nor who opens it** (code; flow 01's page) | Q8. `W/AccessDenied.razor:17–28` with `SystemPageText.Refusal`: "Your role (College admin) does not open this page." and "ask the platform administrator"; the old brief's "tell him beforehand" is still unmet: nothing on his pages says the scales and the College record are the Administrator's. | `act-6/6.10-2…` |
| **Inferred, from the design system's known gaps** (`design/system/README.md:192`) | The EPA list's empty-state body is chosen by `MayCreate` alone, so a College admin with no College would read the institution's sentence (inferred; no step reaches it); a national EPA's record draws its sections with no card (the institution's view, flow 15's). | — |

**Dropped from the old brief (done since, or decided):**
- T323's item editor at 390 (flow 15's item page, `c931d56f`; observed A.7.10), and its LAYOUT line's nine-column table
  with an edit row.
- T331 (`b347e11c`; observed 1.12).
- T301's held-back start, its adoption refusal and the lists' "Open" / "Held back" (flow 15, F5, T9; observed 6.13,
  6.29, 6.31); only the box remains.
- T329's EPA-list skeleton (flow 15); T272's CreateEpa half (flow 15).
- T300's note on Dr Kruger's sidebar (every step capture since T335 shows Activity types).
- The old CONTENT's "five registrars" at KGK (replaced by the Starting state's five profiles, three in training) and
  "minimum 5 … completion window 12 months, weight —" for PAED-002 (the window and weight are on the item's page, not in
  the table, and were not re-read for this brief).
- T271 (a race and the builder's binding: flow 17's and a lock, no screen here).

## 6. Questions for the operator

**Answered 2026-11-04: the operator took every recommendation as written ("yes"); Q3 is D56, Q8 is D57 (EPA-PROGRAMME § 3).** Each with a recommendation. The canvas answers 2, 6, 11 and 14 in its variations; 1, 3–5, 7–10, 12 and 13 are the
operator's, and 1 decides part 1's structure and 4 and 5 part 2's, so 1 is answered before round 1.

1. **Q1. (The operator's; the old brief's question 1) The College's tree.** Today: Specialities (reached through a
   redirect), a speciality's record, its Sub-specialities (a page of its own, with no crumb), each sub-speciality's
   record; 1.12–1.14 take three pages to read one sub-speciality's default ladder. *Recommendation:* one page per College
   for its tree: "Specialities" lists each speciality as an `<h2>` section with its sub-specialities as rows (Name,
   Default ladder, Status, Edit), and Create sub-speciality in each section; a speciality's and a sub-speciality's
   records stay form pages of their own (flow 11's rule); the Sub-specialities list route sends to the College's page
   at that speciality (PROPOSED). Round 1 draws it beside today's separate lists and a two-pane variant, with each
   structure's step count.
2. **Q2. (The old brief's question 2) The reach.** *Recommendation:* in numbers, where the change is made: the items'
   notice and the item page's subtitle ("Adopted by 1 institution, Kgosi Kgari Teaching Hospital; 3 registrars in
   training on it"), the pause dialog ("PAED-012 is on 2 curricula adopted by 1 institution; 3 registrars in training
   are measured against it") and the open/close box (DATA: a count of active adoptions and of active profiles pinned
   to them, by curriculum; PROPOSED words). The College sees counts and institution names, never a registrar's name.
3. **Q3. (The operator's; T264) Deactivating a College, speciality or sub-speciality.** *Recommendation:* keep it, behind
   a ConfirmDialog naming the record ("Deactivate College of Neurologists of South Africa?") that says only what it
   does: no new invitation can name it, and (a speciality or sub-speciality) it leaves every institution's adopted
   disciplines; registrars already admitted and curricula already adopted are not changed. The trigger an outline
   "Deactivate…", offered only on a record stored as active; "Activate" is the Active box, as on an EPA. A decision
   recorded in EPA-PROGRAMME § 3 (PROPOSED).
4. **Q4. (The operator's; part 2) The curriculum's own page.** *Recommendation:* one form page, `<h2>` sections in one
   column (flow 12): Details (sub-speciality, name, version, effective from and to), "Open for adoption" with T301's
   help and Q2's reach, then "A new version" (the clone, `<h2>`, with its own Clone button and what it copies: "the
   College's items, never an institution's own"). Opening asks nothing (6.31's "the only opening"); closing an open
   version asks first, naming who has adopted it (PROPOSED).
5. **Q5. (The operator's; part 2) A result after a create and a clone.** *Recommendation:* as an EPA's page: a one-shot
   token in `CatalogueLinks` (`?created`, `?cloned=‹fromId›`), "Neonatology EPA Curriculum (1.0) created. It is held
   back: no institution can adopt it until you open it." and "Paediatric EPA Curriculum (11.2) cloned from 11.1, with
   its 15 national items. It is held back." (PROPOSED words), focused.
6. **Q6. (T324) The scale's refusal.** *Recommendation:* "PAED-001 on Paediatric EPA Curriculum (11.1) needs rung 5 of
   this scale, which the new levels leave out. Change that item's minimum first." (PROPOSED), one sentence per item,
   up to three, then "and ‹N› more"; the rung by its label (D32).
7. **Q7. (The operator's) Unknown and out-of-scope ids.** *Recommendation:* flow 14's Unavailable state on every record
   and list page: "College unavailable" / "This College does not exist, or you cannot open it." with Go to Specialities
   (or Colleges); the same for a speciality, sub-speciality, scale and curriculum; never an empty list offering Create.
8. **Q8. (The operator's; 1.15, 6.10) The College admin and the scales.** *Recommendation:* admit the College admin to
   the scales list read-only, as an institution's administrator is (the reader's subtitle, no actions), and add
   Entrustment scales to his menu (PROPOSED: a role newly allowed; 1.15's Expect changes); a scale's page and the
   College's own record stay the Administrator's. The alternative is today's Access denied with a line on the
   sub-speciality page: "Scales are kept by the platform administrator."
9. **Q9. (The operator's) Back links.** *Recommendation:* drop every Back link on part 1's pages and the curriculum's;
   the trail is the way back (flows 12, 13 and 15); the curriculum's page links its items under its own heading, and
   the items page links the curriculum's page for the College (flow 15's T5: "flow 16 may add a header link").
10. **Q10. (The operator's; T242) Its refusal.** *Recommendation:* "Kgosi Kgari Teaching Hospital has PAED-016 as its
    own item on this curriculum, so it cannot be added nationally until that institution removes it." (PROPOSED words;
    DATA: the holding institution's name).
11. **Q11. (T264) Delete on a scale, and Remove on a rung.** *Recommendation:* Delete… asks first ("Delete CNSA Neurology
    Entrustment Scale?"; "It is removed from every list. This cannot be undone."), confirm "Delete scale", and a refusal
    after it stays at the list's top, naming the use; Remove on a rung asks nothing (nothing is saved until Save) and
    becomes an outline button.
12. **Q12. (The operator's) The College admin's Home card.** *Recommendation:* "Your College's specialities, EPAs,
    curricula and activity types. An institution can adopt a curriculum version once you open it." with the four menu
    links.
13. **Q13. (The operator's) What the build folds in.** *Recommendation:* T291 item 2 and the redirect; T264's College,
    speciality, sub-speciality and scale halves; T324's College, curriculum and scale halves; T393; T272's College
    handlers and part-1 pages; T326's no-items branch and the College's subtitles; T301's box; T242's refusal with the
    institution named; the new faults above. Leave T264's other pages, T291 items 3, 6 and 8, T324's other pages, T271
    and T390 (the pager) as their own.
14. **Q14. (T326) A discipline with no national EPA.** *Recommendation:* "Neonatology has no national EPA yet. Add one
    under EPAs, then come back to add it here." with Go to EPAs, in place of the add link; the item page's "Cannot add
    an item" the same.

## 7. Acceptance

A flow is done when BRIEF.md § 9's four checks hold. For this flow:

- **Replay the whole runbook** on a fresh database (the flow's steps are 1.2–1.4, 1.12–1.18, 6.1–6.8, 6.10–6.14,
  6.17–6.19, 6.23, 6.24, 6.28–6.31 and A.7.10; § 9): every Expect must hold, the refusals' exact words included.
  **Rewrite the steps that change:** 1.3 (no Back to colleges; the deactivate trigger), 1.4 (the scale's page), 1.12,
  1.13 and 6.11 (Q1's page; no Back to colleges), 1.14 and 6.12 (Q1), 1.15 and 6.10 (Q8; Home's card, Q12), 6.1 (the
  label's message, once), 6.3, 6.4 and 6.7 (Q1), 6.8 (Delete asks first), 6.13 (the box; Q5's result; Q14's words),
  6.17 (Q2's reach), 6.29 (Q5's result), 6.31 (tick "Open for adoption"), 1.18 and 6.30 (Q2's reach), A.7.10 (the
  curriculum's page at 390, if played); search the whole runbook (BRIEF.md § 9 item 7) for the words that change
  ("Back to colleges", "Active" on a curriculum, "Manage sub-specialities"). Record passwords only in
  `pwd_DO_NOT_COMMIT.txt`.
- **Checks the design adds:** no page offers the College admin a page that refuses him; each Deactivate and Delete asks
  first and names its target; the rung refusal reads a label and an EPA; another College's id reads Unavailable; a
  create and a clone say what they made; 390 and 768 with nothing scrolling sideways and every control 44 px at 390, the
  curriculum's page included; new tables swept from 900 to 1100 px against their container; no exception's text on any
  failed read or save.
- **The tests pass** (`dotnet test tests/Wombat.Web.Tests/Wombat.Web.Tests.csproj`, never with `--no-build`), with all
  six suites green. They must include `Admin/EpaEditDeactivationTests`, `Admin/EntrustmentScalesListActionsTests`,
  `Admin/CurriculumItemsEditLayoutTests`, `Admin/CurriculaListScopeTests`, with new ones for the deactivate and delete
  dialogs, the field messages, the rung refusal, Unavailable, the curriculum's results and box; `Navigation/
  ActiveNavItemTests` and `NavMenuAuthorizationTests` (if Q1 or Q8 changes a menu or an owner); `Hosting/PageTitleTests`;
  `Accessibility/RowNamesTests`, `ActionFocusTests`, `AccessibleNamesTests`; `Design/*`, including `NarrowLayoutTests`,
  `DefinedClassTests`, `TableColumnClassTests`, `ContrastTests`; the scenario guard in `Scenario/`; the handler tests
  T264, T272, T324 and T242 bring.
- **Re-capture** the states in § 3 (the old-frame ones, the three EPA-list states and five step captures before
  `50ca6470`, and the new ones on scratch copies, as `states.md` says) and the steps' own captures, keeping their names;
  `check_baseline_paths.py` at missing 0. Compare them with the chosen artboards.
- **Browser check** at 1280, 768 and 390 px as Dr Kruger and `devadmin`, and as Dr Patel and Dr Dlamini on the pause
  steps.

## 8. The runbook steps, verbatim (sent with § 1, in the same message)

These are pasted from `execution/knowledge/scenario-paediatrics/` (`act-1-setup.md`, `act-6-catalogue.md`,
`appendix-cross-cutting.md`; Role, Route, Do and Expect only), as flow 15's replay left them (T392, 2026-11-02; the Gaps
it recorded on these steps are 1.12's and 6.11's T291, 6.1's T324, 6.8's T264 and 6.13's T326). `D` is the replay day
(README § How to play). Steps 1.16 and 1.18 point to tables of the 15 EPAs and their items; those tables are in
`act-1-setup.md` under each step and are not pasted here.

```text
Step 1.2 — The CPSA is among the Colleges (act-1-setup.md)
Role: Administrator — the platform operator
Route: /admin/colleges
Do: Open Colleges from the nav and read the list. Create nothing.
Expect: The list has two rows, each Active and each offering Edit and Specialities:
  - `Demo College` (`DEMO-C`);
  - `College of Paediatricians of South Africa` (`CPSA`), described as "Constituent College of the Colleges of Medicine
    of South Africa; owns the national Paediatric EPA catalogue."
  The header offers Create college. No College is named "College of Paediatricians" alone.

Step 1.3 — The CPSA's own record (act-1-setup.md)
Role: Administrator — the platform operator
Route: /admin/colleges → /admin/colleges/{Id:int}
Do: Open the CPSA's Edit and read the record. Leave by Back to colleges without saving. Record the CPSA's id as
  `{CollegeId}`.
Expect: The page is headed "Edit college", and the browser tab reads "Edit college · Wombat" (T190). Name, Short
  code and Description hold Step 1.2's values, and Active is ticked. Deactivate and Save are offered. Leaving saves
  nothing, and the list is unchanged.

Step 1.4 — The CPSA ladder has six rungs (act-1-setup.md)
Role: Administrator — the platform operator
Route: /admin/entrustment-scales → /admin/entrustment-scales/{Id:int}
Do: Open Entrustment scales and read the list. Then open Edit on `CPSA Paediatric Entrustment Scale v11.1` and read its
  levels. Leave by Cancel without saving. Record the scale's id as `{ScaleId}`.
Expect: The list has two scales: `CPSA Paediatric Entrustment Scale v11.1` at 6 levels and the Demo `O-R Scale` at 5.
  The Administrator is offered Create scale, and Edit and Delete on each row.
  The CPSA ladder's levels, in order 1 to 6, are labelled `1`, `2`, `3a`, `3b`, `4` and `5`, each with its descriptor
  (from "Not entrusted to act…" to "Supervises others…"). Level 3 is split, so a rung's label is not its rank: `4` is
  the fifth rung and `5` is the sixth. Everything that compares levels uses the rank. This editor is the only place
  that shows the rank, in its own Order column (D32).

Step 1.12 — The Paediatrics speciality (act-1-setup.md)
Role: CollegeAdmin — Dr Anton Kruger
Route: /admin/specialities → /admin/colleges/{CollegeId:int}/specialities
Do: Open Specialities from the nav and read the page. Record the Paediatrics speciality's id as `{SpecialityId}`.
Expect: He is redirected to his own College's specialities, subtitled "College: College of Paediatricians of South
  Africa (CPSA)". The page has one row: `Paediatrics`, "Specialist training in Paediatrics.", Active, offering Edit and
  Sub-specialities. The Demo College's speciality is not listed. The header offers Create speciality. It offers no link
  to the Colleges list, which does not admit him (T291 item 2). His menu's Specialities stays lit on it (T331), and the
  trail above the header reads Home › Specialities › College of Paediatricians of South Africa.

Step 1.13 — The Paediatrics sub-speciality (act-1-setup.md)
Role: CollegeAdmin — Dr Anton Kruger
Route: /admin/specialities/{SpecialityId:int}/sub-specialities
Do: Open Sub-specialities on the Paediatrics row. Record the sub-speciality's id as `{SubSpecialityId}`.
Expect: The page is subtitled "Speciality: Paediatrics". It has one row: `Paediatrics`, "General paediatric specialist
  training programme.", Active, offering Edit. The header offers Back to specialities and Create sub-speciality. The
  sub-speciality shares its speciality's name, and there is no "General Paediatrics".

Step 1.14 — The sub-speciality defaults to the CPSA ladder (act-1-setup.md)
Role: CollegeAdmin — Dr Anton Kruger
Route: /admin/specialities/{SpecialityId:int}/sub-specialities/{Id:int}
Do: Open the sub-speciality's Edit and read it. Leave by Cancel without saving.
Expect: The page is headed "Edit sub-speciality" and subtitled "Speciality: Paediatrics".
  - Default entrustment scale is set to `CPSA Paediatric Entrustment Scale v11.1`. The other choices are "No default —
    offer every scale" and `O-R Scale`.
  - The field's help reads "Committee STARs for this programme's trainees are limited to this scale's levels."
  - Active is ticked, and Deactivate and Save are offered.
  The seeder set this default when it created the sub-speciality, and never resets it (T187). So Act 4's STAR picker
  offers the six rungs only (T076).

Step 1.15 — The College does not open the scale pages (act-1-setup.md)
Role: CollegeAdmin — Dr Anton Kruger
Route: /admin/entrustment-scales → /access-denied
Do: Type the address of the entrustment scales list.
Expect: The page reads "You cannot open this page" and "Your role (College admin) does not open this page.", then "If
  you need it for your work, ask the platform administrator.", with Go to Home (T335).
  The scales list admits only an Administrator or an InstitutionalAdmin, and Kruger's menu offers no Entrustment scales.
  He reads the ladder by name where the College uses it: the sub-speciality's default (Step 1.14) and each item's Scale
  column (Step 1.18).

Step 1.16 — The 15 v11.1 EPAs (act-1-setup.md)
Role: CollegeAdmin — Dr Anton Kruger
Route: /admin/epas
Do: Open EPAs and read the list. Create nothing.
Expect: The list has exactly 15 rows, PAED-001 to PAED-015, with the titles in the table below. Every row reads:
  - College `College of Paediatricians of South Africa`;
  - Speciality `Paediatrics` and Sub-speciality `Paediatrics`;
  - Active, with Edit.
  The Demo `EPA-001` is not listed, because a CollegeAdmin reads only his own College's EPAs. The header offers Create
  EPA.

Step 1.17 — Curriculum 11.1 (act-1-setup.md)
Role: CollegeAdmin — Dr Anton Kruger
Route: /admin/curricula
Do: Open Curricula and read the list. Record the curriculum's id as `{CurriculumId}`.
Expect: The list is captioned "College of Paediatricians of South Africa's curriculum versions", with no section
  heading and no College column (T392, R3-A12). It has one row: `Paediatric EPA Curriculum (11.1)`, Discipline
  `Paediatrics / Paediatrics`, effective `2026-01-01`, Items 15, Status Open (the College has released it for
  adoption, T9), offering Edit and Items. The header offers Create curriculum. The Demo `IM Core Curriculum` is not
  listed.

Step 1.18 — The 15 items and their targets (act-1-setup.md)
Role: CollegeAdmin — Dr Anton Kruger
Route: /admin/curricula/{Id:int}/items
Do: Open Items on the curriculum's row. Read the items table against the table below. Add, edit and remove nothing.
Expect: The page is "Items of Paediatric EPA Curriculum (11.1)" (its h1 and tab), under the trail Home › Curricula ›
  Items of Paediatric EPA Curriculum (11.1), with no Back links (T392, T5). Its subtitle reads "College of
  Paediatricians of South Africa · Paediatrics / Paediatrics · effective 2026-01-01".
  - A warning says that a target and its period are read live, in every institution that has adopted the curriculum,
    including periods that have already closed. It also says that a tool list is checked when an activity is filed and
    submitted, and never takes back credit already earned.
  - Under "Items (15)", the table, captioned "Paediatric EPA Curriculum (11.1)'s items, in EPA order", has 15 rows,
    which match the table below. Its columns are EPA, Target and decision, Minimum level, Minimum by training year and
    Tools: an item's completion window and weight are on its own page (T392, decision 1). Every row's Minimum level
    reads its rung over `CPSA Paediatric Entrustment Scale v11.1`, and every row offers Edit and Remove…, each named
    for its EPA ("Edit PAED-001").
  - Target and decision holds the target above the decision, for example "3 per semester" above "Decided each
    semester".
  - There is no Add item: in its place, "Every national EPA of Paediatrics is already on this curriculum, …", which
    explains how an EPA would be freed (T392, F3).

Step 6.1 — devadmin creates a second national College (act-6-catalogue.md)
Role: Administrator — devadmin
Route: /admin/colleges → /admin/colleges/new → /admin/colleges/{Id:int} → /admin/colleges
Do: Create the College of Neurologists of South Africa, with the description "Constituent College of the CMSA; owns
  the national Neurology catalogue." Save it first with the short code `CPSA` by mistake, then correct the code to
  `CNSA` and save again.
Expect: The first save is refused: "A college with the same name or short code already exists." Nothing is created.
  The second save opens the new College's edit page, Active. The colleges list reads three rows, each Active and each
  offering Edit and Specialities: the new College, the College of Paediatricians of South Africa and the Demo College.

Step 6.2 — devadmin corrects CPSA's description, and nothing else (act-6-catalogue.md)
Role: Administrator — devadmin
Route: /admin/colleges → /admin/colleges/{Id:int} → /admin/colleges
Do: Open CPSA's record and note its id from the address, for Step 6.10. Change the description to "Constituent
  College of the Colleges of Medicine of South Africa; owns the national Paediatric EPA catalogue and its versions."
  Leave the name, the short code and Active as they are, and save.
Expect: The save is confirmed, and the list shows the new description, with CPSA still Active. The name and short code
  are unchanged, so every CPSA page still reads "College: College of Paediatricians of South Africa (CPSA)". A restart
  neither re-seeds nor restores the record: the seeder finds its College by seed key (T221).

Step 6.3 — devadmin adds the Neurology speciality (act-6-catalogue.md)
Role: Administrator — devadmin
Route: /admin/colleges → /admin/colleges/{CollegeId:int}/specialities → /admin/colleges/{CollegeId:int}/specialities/new → /admin/colleges/{CollegeId:int}/specialities/{Id:int} → /admin/colleges/{CollegeId:int}/specialities
Do: From the new College's row, open its specialities, and create `Neurology` with the description "Specialist training
  in Neurology."
Expect: Before the create, the list says the College has no specialities yet and invites the first. After it:
  - the edit page reads "College: College of Neurologists of South Africa (CNSA)", Active, and offers "Manage
    sub-specialities";
  - the list has one row, Neurology, Active.

Step 6.4 — devadmin adds the Adult Neurology sub-speciality (act-6-catalogue.md)
Role: Administrator — devadmin
Route: /admin/colleges/{CollegeId:int}/specialities/{Id:int} → /admin/specialities/{SpecialityId:int}/sub-specialities → /admin/specialities/{SpecialityId:int}/sub-specialities/new → /admin/specialities/{SpecialityId:int}/sub-specialities/{Id:int} → /admin/specialities/{SpecialityId:int}/sub-specialities
Do: From Neurology's page, manage its sub-specialities and create `Adult Neurology`, with the description "Adult
  neurology specialist training programme."
Expect: The list is empty before the create and has one row after it. The new sub-speciality's edit page offers a
  default entrustment scale, which reads "No default — offer every scale". Its choices are the two existing scales.

Step 6.5 — devadmin creates the Neurology ladder (act-6-catalogue.md)
Role: Administrator — devadmin
Route: /admin/entrustment-scales → /admin/entrustment-scales/new → /admin/entrustment-scales/{Id:int} → /admin/entrustment-scales
Do: Create `CNSA Neurology Entrustment Scale`, with the description "Supervision ladder for CNSA EPAs." Give it five
  levels, in this order, each with a one-line description:
  1. `Observe only`
  2. `Direct supervision`
  3. `Indirect supervision`
  4. `Distant supervision`
  5. `Unsupervised practice`
  Save it first with the fourth level also labelled `Indirect supervision`, then correct that label and save again.
Expect: The first save is refused: "Level labels must be unique within a scale." Nothing is created. The second save
  opens the new scale's edit page, with the five levels numbered 1 to 5 in the order given. The scales list shows
  three scales, the new one with 5 levels.

Step 6.6 — devadmin adds a sixth level (act-6-catalogue.md)
Role: Administrator — devadmin
Route: /admin/entrustment-scales → /admin/entrustment-scales/{Id:int} → /admin/entrustment-scales
Do: Open the CNSA scale, add a sixth level, `Supervises others` ("Supervises juniors performing the activity."), and
  save.
Expect: The save is confirmed, and the page lists six levels, 1 to 6, with `Supervises others` at 6. The list shows 6
  levels.

Step 6.7 — devadmin makes it Adult Neurology's default (act-6-catalogue.md)
Role: Administrator — devadmin
Route: /admin/specialities/{SpecialityId:int}/sub-specialities/{Id:int}
Do: Open Adult Neurology, set its default entrustment scale to the CNSA Neurology Entrustment Scale, and save.
Expect: The save is confirmed, and reopening the page shows the scale selected. Its help says that committee STARs for
  this programme's trainees are limited to that scale's levels.

Step 6.8 — devadmin cannot delete a scale in use (act-6-catalogue.md)
Role: Administrator — devadmin
Route: /admin/entrustment-scales
Do: Delete the CNSA Neurology Entrustment Scale from the list.
Expect: Delete asks first, naming the scale (DESIGN.md § Button system: a destructive action opens a ConfirmDialog).
  Once confirmed, it is refused: "This entrustment scale is the default scale of the sub-speciality "Adult Neurology"
  (Neurology), so it cannot be deleted. Change that sub-speciality's default entrustment scale to another scale, or to
  no default, first." The scale stays, with 6 levels.

Step 6.10 — Dr Kruger is refused what only an Administrator may do (act-6-catalogue.md)
Role: CollegeAdmin — Dr Anton Kruger
Route: /account/login → /account/login/submit → / → /admin/colleges/{Id:int} → /access-denied → /admin/entrustment-scales/new → /access-denied
Do: Sign in. From the dashboard, type the address of CPSA's own College record (the id from Step 6.2), then the
  address for creating an entrustment scale.
Expect: The dashboard has one card, National catalogue, with Specialities, EPAs and Curricula. His menu offers neither
  of the pages he types. Each address lands on "You cannot open this page" ("Your role (College admin) does not open
  this page."), with Go to Home, and nothing changes.

Step 6.11 — Dr Kruger edits the Paediatrics speciality (act-6-catalogue.md)
Role: CollegeAdmin — Dr Anton Kruger
Route: /admin/specialities → /admin/colleges/{CollegeId:int}/specialities → /admin/colleges/{CollegeId:int}/specialities/{Id:int} → /admin/colleges/{CollegeId:int}/specialities
Do: From Specialities in his menu, open Paediatrics. Change its description from "Specialist training in Paediatrics."
  to "Specialist training in Paediatrics, and certificate training in its sub-specialities." Save.
Expect:
  - Specialities takes him to CPSA's list, which reads "College: College of Paediatricians of South Africa (CPSA)"
    and has one row.
  - The save is confirmed, and the list shows the new description.
  - Neither page offers him a way to the colleges list, which he may not open (T211's rule: a page offers only what
    its target admits).

Step 6.12 — Dr Kruger creates the Neonatology sub-speciality (act-6-catalogue.md)
Role: CollegeAdmin — Dr Anton Kruger
Route: /admin/colleges/{CollegeId:int}/specialities/{Id:int} → /admin/specialities/{SpecialityId:int}/sub-specialities → /admin/specialities/{SpecialityId:int}/sub-specialities/new → /admin/specialities/{SpecialityId:int}/sub-specialities/{Id:int} → /admin/specialities/{SpecialityId:int}/sub-specialities
Do: From Paediatrics, manage its sub-specialities and create `Neonatology`, with the description "Certificate training
  in Neonatology." On its edit page, set the default entrustment scale to the CPSA Paediatric Entrustment Scale v11.1,
  and save.
Expect: The list holds Neonatology and Paediatrics, both Active. The default is saved and stays selected when the page
  is reopened. The Paediatrics sub-speciality keeps its own default, the CPSA ladder. Nothing about KGK's programme
  changes.

Step 6.13 — Dr Kruger starts a Neonatology curriculum, and holds it back (act-6-catalogue.md)
Role: CollegeAdmin — Dr Anton Kruger
Route: /admin/curricula → /admin/curricula/new → /admin/curricula/{Id:int} → /admin/curricula → /admin/curricula/{Id:int}/items
Do: Create `Neonatology EPA Curriculum`, version `1.0`, in Paediatrics / Neonatology, effective from `D`, with no end.
  The form selects Neonatology first. Then open its items.
Expect:
  - The create opens the curriculum's edit page, with Active unticked and a card for cloning it: a new version starts
    held back, so no institution can adopt it yet (T392, F5, T301).
  - The curricula list shows the curriculum Held back, with 0 items.
  - Its items page, "Items of Neonatology EPA Curriculum (1.0)", reads "Items (0)" and has no rows. In place of the add
    link, it says there is nothing to add, and that a national EPA of Neonatology must be added first.

Step 6.14 — Dr Kruger corrects an EPA's wording (act-6-catalogue.md)
Role: CollegeAdmin — Dr Anton Kruger
Route: /admin/epas → /admin/epas/{Id:int} → /admin/epas
Do: Open PAED-006. Correct its title from "Managing long-term health conditions (LTHCs)" to "Managing long-term health
  conditions (LTHCs) in children". Change nothing else, and save.
Expect: The save is confirmed, and the EPA stays Active. The EPA list shows the new title. The title is read live
  wherever the EPA is named: 11.1's items, the activity pickers and every registrar's progress card (Step 6.15). An
  EPA's wording has no versions.

Step 6.17 — Dr Kruger pauses PAED-012 (act-6-catalogue.md)
Role: CollegeAdmin — Dr Anton Kruger
Route: /admin/epas → /admin/epas/{Id:int} → /admin/epas → /admin/curricula → /admin/curricula/{Id:int}/items
Do: Open PAED-012 and press Deactivate…, read the question and confirm. Then open 11.1's items.
Expect:
  - **The question.** Deactivate… asks first, "Deactivate PAED-012?" (T392, S11, Q11). The dialog says that the EPA:
    - can no longer be chosen for a new activity;
    - stops being a target on running programmes' progress pages and dashboards, for every institution that has
      adopted it, while a programme that has ended keeps it on its record, marked no longer in use (T312);
    - keeps the progress it has earned;
    - credits nothing for an activity completed against it while it is inactive, until it is reactivated.
  - **After confirming.** The page says the EPA is deactivated. Active is unticked, and Deactivate… is gone. The EPA
    list reads PAED-012 Inactive.
  - **11.1's items.** PAED-012's row is marked "(inactive: not in force)". A notice says its credit is paused and that
    reactivating it credits what was completed meanwhile.

Step 6.18 — Dr Patel completes the Mini-CEX during the pause (act-6-catalogue.md)
Role: Assessor — Dr Mohammed Patel
Route: /activities/inbox → /activities/{ActivityId:int}
Do: Open Dr Dlamini's Mini-CEX from the inbox. Rate it at the rung that Dr Dlamini's PAED-012 card named as the minimum
  now (Step 6.15). Write the three feedback fields, and complete it.
Expect:
  - The inbox's Waiting for you lists Dr du Plessis's portfolio review first, Overdue since Step 3.30's ageing, then
    "Mini-CEX (Paediatrics) · PAED-012 · `D`", from Anele Dlamini (T350). The row's EPA cell and the request's EPA both
    read "PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams (no longer in use)".
  - He rates it on the rung picker. The completion is not refused (D48): "Completed. 1 more waits for you.", with the
    review's row, Open the next and Back to Activity inbox under it (T350). The activity is read-only.
  - The status card reads "Done. You completed it on …" and "Rated <the rung>. Its credit to PAED-012 waits while the
    EPA is paused." About's Credit reads None, with "This activity's EPA is paused: its credit waits." under it. In
    its history, the completion's Credit reads None.

Step 6.19 — Dr Dlamini during the pause (act-6-catalogue.md)
Role: Trainee — Dr Anele Dlamini
Route: /portfolio/progress → /portfolio/progress/{EpaId:int} → /activities/mine → /activities/{ActivityId:int} →
  /activities/new → /activities/new?type=mini_cex_cpsa
Do: Read her progress and PAED-012's page, then her activities and the Mini-CEX. Then start a Mini-CEX (Paediatrics) and
  open its EPA picker. Leave without saving.
Expect:
  - **Progress.** PAED-012 is no row of "Each semester · 9 EPAs", and This period reads "1 of 9" over "EPAs met this
    semester" (`m − 1`). Under "No longer in use · 1 EPA", PAED-012's row reads "PAED-012 — Communicating with and
    counselling patients, caregivers and healthcare teams (no longer in use)", its mark muted (`.paused-mark`, never
    bold), a link to its page, beside "Paused by the College. It is not a target while it is paused. Its ratings and the
    credit it had earned are kept, and what is completed on it meanwhile is credited if it is restored." (T355).
  - **Its page.** The h1 and the tab read "PAED-012 — … (no longer in use)", the mark muted, and the same sentence
    stands under it as an information alert, with no count. Entrustment reads "While PAED-012 is paused it is not in
    your standing against Annexure A." The trajectory, headed "Rating trajectory (no longer in use)", still charts every
    rating on it, the one from Step 6.18 included (T255). Activities on this EPA lists the Mini-CEX, "PAED-012 (no
    longer in use)" under its name, Credit None.
  - **My activities.** The Mini-CEX carries "PAED-012 (no longer in use)" under its name, and its Credit reads None, as
    text. Its status card reads "Rated 4. Its credit to PAED-012 waits while the EPA is paused.", with no count, and its
    Open My progress lands on PAED-012's page, named "Open My progress at PAED-012" (T355, build review D1).
  - **The picker.** It offers eight EPAs, without PAED-012.

Step 6.23 — Dr Kruger restores PAED-012 (act-6-catalogue.md)
Role: CollegeAdmin — Dr Anton Kruger
Route: /admin/epas → /admin/epas/{Id:int} → /admin/epas
Do: Open PAED-012, tick Active and save.
Expect: Nothing asks first. The page reads "EPA reactivated. 1 activity completed against it while it was inactive now
  counts towards progress." Deactivate… is offered again (T392, S11). The EPA list reads PAED-012 Active.

Step 6.24 — Dr Dlamini's credit is counted (act-6-catalogue.md)
Role: Trainee — Dr Anele Dlamini
Route: /portfolio/progress → /portfolio/progress/{EpaId:int} → /activities/mine → /activities/{ActivityId:int}
Do: Read her progress and PAED-012's page, then her activities and the Mini-CEX.
Expect:
  - PAED-012's row is back in "Each semester · 10 EPAs", with `n + 1` this semester, and "No longer in use" is gone.
    This period reads "… of `m`" over "EPAs met this semester" again. Its page reads one more at the minimum level when
    observed than in Step 6.15, and "Last encounter `D`".
  - Its h1, its trajectory's heading and My activities no longer carry "(no longer in use)", and the Mini-CEX's Credit
    reads "1 item", a link to PAED-012's page. Its status card reads "Rated 4. Credited 1 item to PAED-012. PAED-012:
    `n + 1` of 3 this semester."
  - The credit PAED-012 had earned before the pause was kept, not cancelled. Dr Dlamini holds none from Acts 3-4 (`n` is
    0 in Step 6.15), so this is checked on Dr Molefe's PAED-012 progress from Act 3: its row is unchanged through the
    pause (by SQL, or on her record in Step 6.40).

Step 6.28 — Dr Kruger adds a national EPA (act-6-catalogue.md)
Role: CollegeAdmin — Dr Anton Kruger
Route: /admin/epas → /admin/epas/new → /admin/epas/{Id:int} → /admin/epas
Do: Create an EPA:
  - sub-speciality: Paediatrics / Paediatrics (the form selects Paediatrics / Neonatology first, so change it);
  - code: `PAED-016`;
  - title: "Transitioning adolescents with long-term conditions to adult care";
  - description: "Plans and leads the handover of an adolescent with a long-term condition to adult services, with the
    young person and their caregivers.";
  - category: Core.
  Save.
Expect:
  - The sub-speciality list offers CPSA's two, Paediatrics / Neonatology and Paediatrics / Paediatrics.
  - Once saved, PAED-016 is Active.
  - His EPA list holds PAED-001 to PAED-016, and not KGK-001: a College does not see an institution's own EPAs.
  - PAED-016 is on no curriculum yet, so no registrar is offered it or measured against it.

Step 6.29 — Dr Kruger clones 11.1 as 11.2, and holds it back (act-6-catalogue.md)
Role: CollegeAdmin — Dr Anton Kruger
Route: /admin/curricula → /admin/curricula/{Id:int}/items → /admin/curricula/{Id:int} → /admin/curricula
Do: Read 11.1's items. Then, on 11.1's page, clone it: new version `11.2`, effective from `D`, with no end. Then open
  Curricula.
Expect:
  - 11.1's items still show him 15 rows: KGK's own item is not shown to the College (T211). "Add item" stands beside
    "Items (15)", because PAED-016 is on no curriculum.
  - The clone form starts with the version "11.1-copy".
  - Once cloned, the page is 11.2's, with Active unticked: a clone starts held back (T392, F5).
  - The list shows 11.2 Held back, with 15 items.

Step 6.30 — Dr Kruger edits 11.2's items (act-6-catalogue.md)
Role: CollegeAdmin — Dr Anton Kruger
Route: /admin/curricula → /admin/curricula/{Id:int}/items → /admin/curricula/{Id:int}/items/new → /admin/curricula/{Id:int}/items → /admin/curricula/{Id:int}/items/{ItemId:int} → /admin/curricula/{Id:int}/items
Do: On 11.2's items:
  1. Add PAED-016:
     - target: 1 per academic year;
     - decided each academic year, by the trainee's general panel;
     - the suggested scale, and minimum level 4;
     - year-by-year minima: 2, 3a, 3b and 4 for years 1 to 4;
     - tools: Mini-CEX, CBD and MSF.
  2. Edit PAED-011: set its target to 2 per academic year, and change nothing else.
Expect:
  - **Before the edits.** The 15 rows match 11.1's, cell for cell, and "Add item" stands beside "Items (15)".
  - **The add.** "Add item" opens a page of its own (T392, Q14, R3-B22), headed "Add item" and subtitled "On Paediatric
    EPA Curriculum (11.2). It applies to every institution that has adopted it." Its EPA picker offers PAED-016 alone,
    chosen. Add item returns to the list with "Curriculum item added." at its top, focused (F1, R3-B23): 16 rows,
    PAED-016 last, and in place of the add link the note that every national EPA of Paediatrics is already on the
    curriculum.
  - **The edit.** Edit PAED-011 opens its own page, "Edit PAED-011 — Managing population health challenges", with the
    same subtitle, its EPA a select offering PAED-011 alone (R3-B24). Save item returns to the list with "Curriculum
    item saved." at its top, focused, and the row reads "2 per academic year". 11.1's PAED-011 still reads 1 (outcome
    SQL).

Step 6.31 — Dr Kruger publishes 11.2 (act-6-catalogue.md)
Role: CollegeAdmin — Dr Anton Kruger
Route: /admin/curricula → /admin/curricula/{Id:int} → /admin/curricula
Do: Open 11.2, tick Active and save.
Expect: The save is confirmed, and this is the only opening: a version starts held back (T392, F5). The curricula
  list shows 11.1 and 11.2 Open, with 11.2 at 16 items, and the Neonatology curriculum still Held back (T9).

Step A.7.10 — Dr Kruger on his phone (appendix-cross-cutting.md)
Role: CollegeAdmin — Dr Anton Kruger
Route: / → /admin/epas → /admin/curricula → /admin/curricula/{Id:int}/items → /admin/curricula/{Id:int}/items/{ItemId:int} → /admin/curricula/{Id:int}/items
Do: At 390 px, open EPAs, then Curricula and the items of Paediatric EPA Curriculum 11.2. Press Edit PAED-001 and read
  its page, then leave it by Cancel.
Expect: Nothing scrolls sideways on any of them (T323; T392, S16).
  - **EPAs, Curricula and the items** stack, a block per row, each text cell labelled, and each row's actions 44 px
    tall and named for the row ("Edit PAED-001", "Remove PAED-001"). The items carry no window or weight: those are on
    an item's own page (T392, decision 1).
  - **PAED-001's page**, "Edit PAED-001 — Providing paediatric emergency care to children", subtitled "On Paediatric
    EPA Curriculum (11.2). It applies to every institution that has adopted it.", sets one field a row, each control
    44 px, with Save item on top of Cancel (T392, Q14, R3-B24 at 390). Cancel returns to the list with PAED-001's Edit
    focused (R3-B11).
```
