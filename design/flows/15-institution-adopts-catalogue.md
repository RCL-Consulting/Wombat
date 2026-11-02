# F15 An institution adopts the College's curriculum, adds its own, and moves to a new version

**Who and why:** an institution's administrator adopts a College curriculum version so that her institution can admit
registrars against it, reads the College's EPAs and items (the College's to change, read-only to her), adds an EPA and
a curriculum item of the institution's own, adopts a newer version, and moves a registrar onto it; the Administrator
rebuilds everyone's progress. Adoption gates admission and a move rewrites a registrar's progress, so every page must
say whose rules these are and what a change will do. Prof Nolwazi Mbatha (KGK) adopts Paediatric EPA Curriculum 11.1,
creates KGK-001 and its item, adopts 11.2, keeps Dr Anele Dlamini on 11.1 and moves Dr Sipho Ndlovu to 11.2; `devadmin`
rebuilds progress; Dr Ndlovu reads his page. Pages: Curricula, Curriculum adoptions, EPAs, an EPA's page (create and
edit), a curriculum's items, Entrustment scales, the progress rebuild, and, as the move's ends, the trainee profile
(flows 11 and 13's) and My progress (flow 05's). None of the six catalogue pages has been redesigned: flow 01 gave them
the shell, the " · Wombat" titles and Curricula's "Curriculum progress" link; T312 (`c9d6fe13`) the ended record's words
in the EPA and item dialogs; flow 14 (P5) every ConfirmDialog's first focus on Cancel; nothing else since 2026-09-26.
The step captures are from flow 14's replay (T387, 2026-10-30), on the shell as built; every `states/` capture of the
six catalogue pages is from 2026-09-26, on the old frame.

Restated 2026-11-02 for T392 (the brief of 2026-09-26 predates flows 01–14 and T304: its CONTENT quoted the T295
replay's dates and a rebuild that removed a stale tally, its first and fifth screens drew the refusal and the zeros
T304 removed (`95d4e950`, `a27983fd`), it called the scales "the College's", which they are not (they are the
Administrator's, platform-wide), and its LAYOUT line described the pre-shell frame). Paths to screenshots are under
`design/baseline/` (gitignored). Code is cited at `a87041ac`; `W/` is `src/Wombat.Web/Components/Pages/`, `A/` is
`src/Wombat.Application/Features/`, `Nav/` is `src/Wombat.Web/Navigation/`.

| | |
|---|---|
| Decision | **Restructure, with UX in scope** (W-008; BRIEF.md § 4), inside flow 01's shell. Marked **F** in BRIEF.md § 8; restated here as **F/W**: the EPA list, an EPA's page, the scales and the rebuild keep their shapes and change in their parts, but where an institution adopts and reads its curricula (the brief's own question, Q1) and how its own item is added and edited on a shared curriculum (Q14; T323's 390 px edit row) are structural. |
| Frequency and stakes | Rare: at set-up, then about once per curriculum version per discipline; a local EPA or item a few times a year. Medium-high: adoption gates admission; a newer adoption supersedes the old for every new admission; a move rewrites a registrar's progress and cannot be undone through the page (a move back to a superseded version is refused, `A/Trainees/UpdateTraineeProfile.cs:109–111`). |
| Mode | **Restated by BRIEF.md § 2.3 step 4 and § 2.4** (the old row's "straight to fidelity: the List and Form page shapes hold (BRIEF § 4)" cites the pre-W-008 recommendation). **Wireframe first** (2–3 STRUCTURES each) for (A) where the institution adopts and keeps its curricula, Curricula with Curriculum adoptions (Q1, Q6–Q10: one page or two, what a row says about the institution's adoption, where Adopt and its confirmation sit, and how the Administrator adopts for an institution), and for (B) a curriculum's items as the institution reads them, with its own item's add, edit and remove (Q3, Q14: the edit row in the table, a note panel, or a form page of its own). **Straight to fidelity** (2–3 variations) for (C) the EPA list and an EPA's page (Q2, Q3, Q11, Q12: the owner mark, what a national row offers, create only when it can be completed, out of scope) and for (D) the move on the trainee profile (Q8: its confirmation and its result). **One proposal at fidelity** each, to confirm or correct, for (E) the scales list (Q13), (F) the progress rebuild (Q4; its failure and the trigger) and (G) My progress after a move (flow 05's page; nothing changes but what the move's Expect quotes). Round 2: the picks with every state. Then a review round from the boards' text, the operator's decisions, and one correction round. |
| Viewports | Desktop 1280×800 and phone 390×844; the build is also checked at 768 (§ 11, flow 06) |
| Variations | 2–3 structures (wireframes) for (A) and (B); 2–3 variations (fidelity) for (C) and (D); one proposal each for (E), (F) and (G); the chosen ones in round 2 |
| People | InstitutionalAdmin Prof Nolwazi Mbatha (Kgosi Kgari Teaching Hospital, KGK, institution 2). Administrator `devadmin` ("Demo Administrator"; runs the rebuild; may adopt for any institution, unplayed: `coverage.md:198`). Trainees: Dr Sipho Ndlovu (year 1, programme start 2026-01-15 = `J`, expected completion 2030-01-14, profile 6; moved to 11.2), Dr Anele Dlamini (year 3, start 2024-01-15, expected completion 2028-01-14, moved to 2028-07-14 at 6.34, profile 3; stays on 11.1), Dr Lerato Molefe (graduated in Act 5; her STARs at 6.22, flow 09's register). Off-stage, flow 16's: CollegeAdmin Dr Kruger (CPSA) pauses PAED-012 (6.17) and restores it (6.23), adds PAED-016 (6.28), clones 11.1 as 11.2 and holds it back (6.29), publishes it (6.31). `D` is the replay day: 2026-10-30 in the T387 replay, which played Acts 1–6 that day, so both adoptions are dated `D` |
| Runbook steps | 16, pasted verbatim in § 8: 1.19–1.22, 6.9, 6.22, 6.25, 6.26 and 6.32–6.39. 6.22 crosses flow 09's Entrustment decisions (the STAR register); 6.34 and 6.36 cross flows 11 and 13's Trainees and trainee profile; 6.35, 6.37 and 6.39 are flow 05's My progress, and 6.37 flow 03's EPA picker; 6.38 starts on the Administrator's Curricula. 1.21 ends on flow 12's Home (the Curriculum card, T291 item 4, done) |
| Pages (`coverage.md` templates) | `/admin/curricula` (`W/Admin/Curricula/CurriculaList.razor`, 108 lines, `[Authorize(Policy = "NationalCatalogueAccess")]` (`:2`): Administrator, CollegeAdmin, InstitutionalAdmin, `src/Wombat.Infrastructure/Identity/AuthorizationPolicies.cs:81–82`); `/admin/adoptions` (`W/Admin/Adoptions/AdoptionsList.razor`, 192, `AdministratorOrInstitutionalAdmin` (`:2`)); `/admin/epas` (`W/Admin/Epas/EpasList.razor`, 80, `NationalCatalogueAccess` (`:2`)); `/admin/epas/new` and `/admin/epas/{Id:int}` (one file, `W/Admin/Epas/EpaEdit.razor`, 392, `NationalCatalogueAccess` (`:3`)); `/admin/curricula/{Id:int}/items` (`W/Admin/Curricula/CurriculumItemsEdit.razor`, 1264, `NationalCatalogueAccess` (`:2`)); `/admin/entrustment-scales` (`W/Admin/EntrustmentScales/EntrustmentScalesList.razor`, 115, `AdministratorOrInstitutionalAdmin` (`:2`)); `/admin/curriculum-progress` (`W/Admin/CurriculumProgress/CurriculumProgressRebuild.razor`, 123, `Administrator` (`:2`)). Crossed, other flows' and built: `/admin/trainees/edit` (`W/Admin/Trainees/TraineeProfileEdit.razor`, 788, `AdministratorOrInstitutionalAdmin` (`:2`); flows 11 and 13, T304), `/portfolio/progress` (`W/Portfolio/MyProgress.razor`, 368, `TraineeOrFormerTrainee` (`:4`); flows 05, 06, 13), `/admin/entrustment-decisions` (`W/Admin/EntrustmentDecisions/Index.razor`, 573, `Roles = "Administrator,InstitutionalAdmin,SpecialityAdmin,SubSpecialityAdmin"` (`:2`); flow 09), `/admin/trainees` (`W/Admin/Trainees/PendingTraineesList.razor`; flows 11 and 13), `/activities/new` (flow 03), `/` (flow 12's InstitutionalAdmin Home). Not in this flow though they share files: `/admin/curricula/new` and `/admin/curricula/{Id:int}` (`W/Admin/Curricula/CurriculumEdit.razor`, `AdministratorOrCollegeAdmin` (`:5`): the College's, flow 16). Since 2026-09-26 the six catalogue pages changed only by `b347e11c` (flow 01: titles; Curricula's outline "Curriculum progress" in the header for those the rebuild's policy admits, `CurriculaList.razor:11–16`) and `c9d6fe13` (T312's ended-record words in `EpaEdit.razor:106–112` and the items page); the rebuild page by `b347e11c` (its way in). No page was renamed or moved. |
| Menus (`Nav/NavItems.cs`) | The InstitutionalAdmin's "Curriculum" group: Curriculum adoptions (`/admin/adoptions`, icon `book-check`, `:113`), Curricula (`book-open`, `:112`), EPAs (`list-checks`, `:111`), Activity types, Entrustment scales (`gauge`, `:115`) (`:153`). The Administrator's "Catalogue" group: EPAs, Curricula, Activity types, Entrustment scales (`:147`): **no Curriculum adoptions**, so she reaches `/admin/adoptions` only by its address or by the nothing-adopted links on Invite and Admit (`W/Admin/Invitations/InviteAPerson.razor:187`, `W/Admin/Trainees/AdmitRegistrar.razor:260`); DESIGN.md:586 records the menu as built. Owners (`Nav/NavOwners.cs:150–156`): an EPA's page lights EPAs, the items page Curricula, the rebuild Curricula (the Administrator's; observed 6.38: Home › Curricula › Curriculum progress). |
| Components drawn at runtime | `PageHeader` (Curricula: "Curricula" / "Manage curriculum versions and the sub-speciality they apply to.", `CurriculaList.razor:9`; Adoptions: "Curriculum adoptions" / "Adopt the national (college-owned) curriculum versions your institution trains against. Trainees can only be admitted into an adopted version.", `AdoptionsList.razor:8`; EPAs: "EPAs" / "Maintain the entrustable professional activities available to curricula and forms." with a primary **Create EPA for every caller** (`EpasList.razor:8–14`); an EPA: "Create EPA" or "Edit EPA" / "Define the EPA code, scope, and descriptive guidance." with **Back to EPAs** (`EpaEdit.razor:8–14`); items: "Curriculum items" / "Paediatric EPA Curriculum (11.1)" with **Back to curricula** (`CurriculumItemsEdit.razor:10–21`); scales: "Entrustment scales" / "Define and maintain the rating ladders used by assessment forms and committee reviews." (`EntrustmentScalesList.razor:11`); rebuild: "Curriculum progress" / "Recalculate every trainee's curriculum targets from their completed activities." (`CurriculumProgressRebuild.razor:9`)). `DataTable` with a caption, **no `Stack` or `StackWide` on any of these lists** (Curricula 9 columns, EPAs 7, Adoptions 7, scales 3–4); the items page a hand-built `table.clinic-table--compact` of 9 columns in a `.table-container` with the edit row a `<td colspan="9">` (`CurriculumItemsEdit.razor:84`, `:189–190`); `StatePanel` (Curricula and EPAs with `LoadError`; Adoptions and an EPA's page with `IsEmpty="false"` and no `LoadError`, so a failed read is an alert above an empty body: `AdoptionsList.razor:25`, `:135–138`; `EpaEdit.razor:33`, `:204–207`); success and danger `Alert`s at each page's top (an `ActionResult` on Adoptions, a `div.action-result` on an EPA and the items page); `<h3>` sections with no `<h2>` (Adoptions' "Adopt a curriculum" and "Current adoptions", `AdoptionsList.razor:44`, `:63`; items' "Existing items" and the add form's heading, `CurriculumItemsEdit.razor:76`, `:329`); `FormField` and `InputSelect` (the EPA's sub-speciality and category; the item's EPA, Per, decision cadence, decided by, scale, `RungPicker`, `StageMinimaEditor`, tool checkboxes); `EditForm` with `ValidationSummary` and `ValidationMessage` on an EPA's page (`EpaEdit.razor:35–37`); `ConfirmDialog` three times (Deactivate this EPA?, `EpaEdit.razor:110–116`; Remove this item?, `CurriculumItemsEdit.razor:474–480`; Rebuild curriculum progress?, `CurriculumProgressRebuild.razor:64–68`), each opening on Cancel since flow 14 (observed 6.38). No dialog on Adopt or on a move. Requests: `GetCurriculaListQuery`, `GetAdoptableCurriculaQuery`, `ListAdoptionsQuery`, `AdoptCurriculumCommand`, `ListEpasForSubSpecialityQuery`, `GetEpaByIdQuery`, `CreateEpaCommand`, `UpdateEpaCommand`, `DeactivateEpaCommand`, `GetCurriculumByIdQuery`, `ListCurriculumItemEpaOptionsQuery`, `Add`/`Update`/`RemoveCurriculumItemCommand`, `GetEntrustmentScalesListQuery` (unscoped: every scale, `A/Epas/GetEpas.cs:192–212`), `RebuildCurriculumProgressCommand` (takes an optional trainee; no page passes one), `GetTraineeCurriculumChoicesQuery` and `UpdateTraineeProfileCommand` (T304). |
| Held | None. Every step capture is from the T387 replay (2026-10-30) on the shell as built, except `act-6/6.35-1-ndlovu-progress.png` (2026-10-27, the T379 replay: the T387 replay played 6.36 before 6.35, so its 11.1 reading was not taken) and `act-6/6.34-1-profile-save-refused.png` (2026-10-03: the refusal T304 removed; do not attach). Every `states/` capture of the six catalogue pages, and `my-progress--after-rebuild`, is from 2026-09-26, on the old frame (a Title Case sidebar with Logout, an email in the top bar): read their bodies only. `states/trainee-profile-edit--moved.png` and `states/my-progress--after-move.png` are from 2026-10-04, on the shell, before flows 11 and 13 rebuilt the profile page |
| Depends on | Flow 01, built (`b347e11c`): the shell, the menus above, not found, `ActionResult`, `ConfirmDialog`, `StatePanel` with Try again. Flow 05 (`b020c942`): My progress, KGK-001's "Kgosi Kgari Teaching Hospital's own" mark (T357 notes that it is a badge). Flow 09 (`49e7cf5e`): the STAR register (6.22). Flow 11 (`69a17c70`): refusals under their field, focused; the required mark; `StackWide`; the list is the page and its create form a page of its own. Flow 12 (`e53d4ae7`): Home's Curriculum card counting what KGK adopted (T291 item 4, observed 1.21); a destructive action confirmed, its trigger an outline button; a form in a note panel opened in place. Flow 13 (`5cc10c66`, `73baa556`): the profile page ("Sipho Ndlovu's programme", End this programme apart from Save, the result's first sentence bold). Flow 14 (`e8eea1d2`): ConfirmDialog in action mode opens on Cancel; Unavailable for a null read (a page's own not-found state). T304 (`95d4e950`, `a27983fd`): the profile's picker, the kept pin, the move's replay and its result. The design system is **version 41** |

**How to run this thread** (BRIEF.md § 2.3 step 4; § 11):
1. **Stage the upload set:** `powershell -File design/tools/stage_upload.ps1 -Flow 15`. Open each screenshot. The
   script puts any capture whose name contains "invit", "issued", "resent", "resend" or "reset" in
   `design/upload/crop-first/`; § 4 names none. No capture here shows a password or a one-time link; the captures show
   scenario addresses (the profile's Email) and EPA codes, which are the pages' own record.
2. **The canvas** is "Wombat 15" (https://claude.ai/artifact/QvQL26tCCJh6S9zoLBywdZ), on the Wombat design system
   v41, with `tokens.css` written by hand (§ 11, flow 09) and the mark copied in as
   `/_blob/7d85109844af8d55420d07753d246843` (T392's note, 2026-11-02). § 1 and `round-1-ask.txt` carry that URL.
3. **Check on the canvas that the attachments arrive** before round 1 (§ 11, flow 13): the ask tells it to say so first
   on its Spec board. Then **send § 1 and § 8 as ONE message**
   (`design/flows/15-institution-adopts-catalogue/round-1-ask.txt`), with the key screenshots in § 4. Read
   `project/canvas.json`'s version before and after: a send can draw nothing. The ask tells the canvas to save its
   stylesheet as a project file (§ 11, flow 14).
4. **Answer § 6's operator questions first** (Q1, Q2, Q5–Q10, Q12, Q15–Q17 are the operator's): Q1 decides structure
   (A), Q14 structure (B). A question whose answer rests on an open task names it, and the task lands before the build
   when it changes data or rules (§ 11, flow 05). Then pick a structure for (A) and (B) and a variation for (C) and (D),
   confirm or correct (E)–(G), and say why in a sentence. Ask for round 2 at fidelity, with its Spec and Steps boards.
5. **Review round 2 from the boards' text** (`Artifact read`), as `design/flows/14-data-rights/round-2-review.md` did:
   one reviewer on the code (and the ask), one on the cast and the runbook, one on the design system. Put the decisions
   to the operator, then ask for one correction round.
6. **Record every round** in `design/flows/15-institution-adopts-catalogue/` before moving on.

---

## 1. The ask (paste this first, with § 8 in the same message)

```text
FLOW 15 — My institution adopts the College's curriculum, keeps its own EPA and item, and moves to a newer version

This is flow 15 of a RESTRUCTURE with UX in scope (design/BRIEF.md § 4). Flows 01 (the shell), 02 (sign-in and
  account), 03 (a registrar files an activity), 04 (an assessor's inbox), 05 (a registrar reads where she stands),
  06 (watching the programme), 07 (the review sitting), 08 (panels, what is due, the schedule), 09 (STARs, the
  register, appeals), 10 (MSF campaigns), 11 (invitation to admission), 12 (the institution's people), 13 (graduation
  and exit) and 14 (data rights) are designed and built; every page here sits inside the shell. Round 1: 2–3
  STRUCTURES as wireframes for (A) where an institution adopts and keeps its curricula (today two pages, Curricula and
  Curriculum adoptions) and for (B) a curriculum's items as the institution reads them, with its own item's add, edit
  and remove; 2–3 variations at FULL FIDELITY each for (C) the EPA list and an EPA's page and for (D) moving a
  registrar to a newer version on the trainee profile; ONE proposal at fidelity each for (E) the entrustment scales
  list, (F) the progress rebuild and (G) My progress after a move, to confirm or correct. I pick before round 2.
  The runbook steps follow below in this same message. The brand mark is at /_blob/7d85109844af8d55420d07753d246843:
  use it, never a drawn disc. Every person, institution, code, title, version, date and count comes from this message
  and the steps below; invent none. Where a value is not given, write a bracket ‹…›.

THE SCREENSHOTS AND THE STYLESHEET:
  - The key screenshots listed under ATTACHED are attached to THIS message (copies of design/baseline/, staged in
    design/upload/). SAY ON YOUR SPEC BOARD, FIRST, whether they arrived (as attachments, or read from a folder), which
    you opened and which you did not. If any did not arrive, draw nothing from memory in its place: write a bracket
    (flows 06, 12 and 13's lesson). The words in them are today's pages, defects included; they are not the target.
  - Write the round's shared stylesheet as a PROJECT FILE, project/f15r1.css (upload or write it as a file of the
    project, NOT as an asset, so the saved round holds it). Every board's head links ds/wombat/tokens.css,
    ds/wombat/components/bundle.css, then f15r1.css, in that order, and no /_blob/ stylesheet; a board's <style>
    holds body{margin:0} at most. Each NEW rule in f15r1.css names its row on the Spec's class table.

GOAL:
  An institution's administrator (Prof Mbatha, KGK) must be able to:
    - see which College curriculum versions are open to her institution, adopt one, and know before she presses what
      an adoption of a newer version does: it supersedes the current one for every new admission, the registrars
      already admitted stay on their version until each is moved, and KGK's own items are not carried over;
    - read the curricula her institution trains against (the adopted version, and a superseded one while registrars
      are on it), each with its College, its items and its state for KGK, in words;
    - read the College's EPAs and a curriculum's national items as the College's, read-only, and tell them at a
      glance from KGK's own EPA and KGK's own item, which she creates, edits, deactivates and removes;
    - be offered nothing she cannot complete: no Edit on a national EPA, no Create EPA before there is an adopted
      discipline to put one in, no curriculum version that adoption would refuse;
    - move a registrar to the adopted newer version on his profile, told before she saves that a move cannot be undone
      and that his evidence is counted again, and told after it what was counted;
    - read the entrustment ladders the forms use, read-only, with copy that fits a reader.
  The Administrator (devadmin) must be able to rebuild everyone's progress, told what a rebuild does and what it did,
  and, when it fails, that nothing changed, without an exception's text. A registrar (Dr Ndlovu) moved to a new version
  reads his evidence counted on it at once.
  What is wrong today (the attached screenshots show each; on the shell as built, 2026-10-30):
  - The EPA list offers Edit on all 15 national EPAs, and the save is refused ("You do not have permission to update
    this EPA."); KGK-001 is listed first under the College's name with nothing saying it is KGK's (1.21, 6.22, 6.25).
    Before KGK adopts anything it offers Create EPA and "Create the first EPA to start building curricula and assessment
    forms.", but the form's Sub-speciality list is empty, so it can never be saved (1.19).
  - An EPA's page is titled "Edit EPA" for every EPA, with Back to EPAs beside the trail. Typed with the id of an EPA of
    a College KGK never adopted (the Demo College's EPA-001), it opens as a full form with Save and Deactivate, a blank
    sub-speciality, instead of not found. Deactivating KGK's own EPA warns of "every institution that uses it".
  - Curriculum adoptions and Curricula are two pages for one job. Adopt asks nothing first, though a newer version
    supersedes the current adoption (6.32: "Curriculum adopted." at once); the picker offers the version KGK has already
    adopted, and Adopt then refuses it ("This institution has already adopted this curriculum version.", 1.20).
    Curricula's Status column prints the College's flag, so 11.1 reads "Active" after KGK superseded it (6.33).
  - A curriculum's items page: a 9-column table whose edit row is a full form inside a table cell; at 390 px it scrolls
    sideways (T323); "PAED-001 - Title" with a hyphen where every other page writes "PAED-001 — Title".
  - The scales page tells her to "Define and maintain the rating ladders…" that only an Administrator changes (1.22).
  - Every failed read or save on these pages prints the exception's text; a rebuild that fails reads "The rebuild
    failed and nothing was changed: An exception has been raised that is likely due to a transient failure…".
  - The sections are <h3> with no <h2>; no list on these pages stacks at 390 px.

AUDIENCE:
  - InstitutionalAdmin: Prof Nolwazi Mbatha, Kgosi Kgari Teaching Hospital (KGK). At set-up, then about once per
    curriculum version; medium-high stakes.
  - Administrator: devadmin ("Demo Administrator"). Runs the rebuild; may adopt for any institution.
  - Trainee: Dr Sipho Ndlovu (moved to 11.2); Dr Anele Dlamini (stays on 11.1). They read My progress.
  - Desktop 1280×800 and phone 390×844.

THE RULES THE PAGES CARRY TODAY (real words; they are the data to draw with, and the rules hold whatever the shape):
  - Whose is what. A College (here the College of Paediatricians of South Africa, CPSA) owns its national EPAs, its
    curricula and their national items; only it (or the Administrator) changes them. An institution adopts a version;
    it may add EPAs of its own in a discipline it has adopted, and items of its own on a version it has adopted, which
    apply only to its trainees and which no other institution reads. Entrustment scales are platform-wide, changed
    only by the Administrator; every admin reads every scale.
  - Adoption (/admin/adoptions; "Curriculum adoptions"): the picker offers every open College version ("College of
    Paediatricians of South Africa — Paediatrics / Paediatrics: Paediatric EPA Curriculum (11.1)"); Adopt is off until
    one is chosen; "Curriculum adopted." (focused). One active adoption per discipline: adopting another version of the
    same discipline supersedes the active one ("Re-adopting a newer version of a discipline you already follow
    supersedes the current adoption for that discipline."). The table: College, Speciality, Discipline, Curriculum,
    Version, Adopted on, Status (Active, Superseded); empty: "No curriculum adoptions yet for this institution.".
    Refusals: "This institution has already adopted this curriculum version."; "The selected curriculum could not be
    found.". The Administrator chooses an institution first ("— Select an institution —"); the page acts on the
    change. A held-back version is left out of the picker but the command does not refuse it (T301).
  - Admission and moves (the profile, flows 11 and 13's): a registrar is admitted only into the active adoption's
    version. A profile's picker offers its pinned version and the active adoption's; saving with the curriculum
    unchanged keeps the pin, superseded or not ("Trainee profile saved."). A move counts the registrar's completions
    again against the new version in the same save: "Trainee profile saved. 1 completion was checked against 11.2,
    and 1 counts towards it." (its first sentence bold); other results: "… Sipho Ndlovu has no completions to count
    against 11.2.", "… N completions checked again from the new programme start date.". A move into anything but the
    active adoption is refused: "A trainee can be moved only into the curriculum version this institution has adopted
    for their discipline, so nothing was saved." So a moved registrar cannot be moved back. No dialog asks first.
  - Curricula (/admin/curricula): to her, the versions KGK actively adopted, and a superseded one only while it holds
    an item of KGK's own; Name, Version, College, Speciality, Sub-speciality, Effective, Items (national and KGK's own),
    Status (the College's Active/Inactive), and Items ("Items of Paediatric EPA Curriculum (11.1)"); no Edit, no Create
    curriculum. Empty: "No curricula yet" / "Your institution has not adopted a curriculum yet. Adopt one on the
    Curriculum adoptions page before admitting trainees.". To the Administrator: every curriculum, Edit and Items, and
    an outline "Curriculum progress" and a primary "Create curriculum" in the header.
  - EPAs (/admin/epas): to her, the national EPAs of the disciplines KGK has actively adopted, plus KGK's own; Code,
    Title, College, Speciality, Sub-speciality, Status (Active, Inactive); Edit on every row; Create EPA. An
    InstitutionalAdmin creates only her institution's own EPA; she may change and deactivate only her own.
  - An EPA (/admin/epas/new, /admin/epas/{Id}): Sub-speciality* (to her, only the adopted ones: "Paediatrics /
    Paediatrics"), Code*, Title*, Description, Required knowledge and skills, Category* (Core, Elective); on an existing
    EPA, Status: Active, with help ("An inactive EPA cannot be chosen for a new activity and is not a target on any
    running programme's progress page or dashboard; a programme that has ended keeps it on its record. …"). Cancel,
    Deactivate (an outline button, on an active EPA), Save. "Deactivate this EPA?" (its confirm the danger button
    "Deactivate"); unticking Active and saving asks the same. Results: "EPA saved.", "EPA deactivated.", "EPA saved and
    deactivated.", "EPA reactivated. No activity was completed against it while it was inactive, so no progress
    changed." / "… 1 activity completed against it while it was inactive now counts towards progress.". A new EPA opens
    its own page. Refusals: "You do not have permission to update this EPA."; "An EPA with the same code already exists
    for this sub-speciality." (said today of every failed create, T272).
  - A curriculum's items (/admin/curricula/{Id}/items): to her, the national items read-only under a standing notice
    ("The College sets this curriculum's national items, so they are read only here. Your institution's own items
    apply only to its trainees, and you can add, edit and remove them. A target and its period are read live: changing
    one changes what every trainee of your institution on this curriculum is measured against, including periods that
    have already closed. A tool list is checked when an activity is filed and submitted, and never takes back credit
    already earned."); each national row's actions cell "Set by the College"; KGK's row "Your institution's own item"
    under its EPA, with Edit and Remove; a paused EPA's row "(inactive: not in force)" and an info notice. Columns:
    EPA, Target and decision, Scale, Minimum level, Minimum by training year, Completion window (months), Weight,
    Tools. The add form "Add your institution's own item": EPA* (only what KGK may add: KGK-001), Target, Per
    (Semester, Academic year), Decision cadence (No published cadence, Each semester, Each academic year), Decided by
    ("The trainee's general panel"), Opportunistic, Entrustment scale ("Suggested because every item on this
    curriculum is pinned to it."), Minimum level* (a rung picker), minimum by training year, Completion window
    (months), Weight, Tools; "Adding is off until every minimum is a rung on CPSA Paediatric Entrustment Scale
    v11.1."; Add item → "Curriculum item added."; then "Every EPA your institution could add is already on this
    curriculum: each national EPA of Paediatrics, and each of your institution's own EPAs of Paediatrics. To add an
    item of your own, first create a local EPA of Paediatrics.". Remove asks "Remove this item?" ("Remove KGK-001 -
    Running a paediatric outreach clinic at a district hospital from Paediatric EPA Curriculum (11.2)? Its target,
    minima, decision and tool list are deleted, and so is each trainee's tally toward it. Your institution's trainees
    on this curriculum are no longer measured against this EPA. This cannot be undone."), confirm "Remove item". A
    curriculum she cannot open is not found. A new version carries no institution's own items: KGK adds its item
    again on 11.2.
  - Entrustment scales (/admin/entrustment-scales): Name, Description, Levels; to her no Create scale and no actions
    column.
  - The rebuild (/admin/curriculum-progress; the Administrator; from Curricula's header): three paragraphs ("Each
    completed activity is credited to the curriculum items it matches, in the semester that contains its encounter
    date. …"; "… It is safe to run more than once. It runs as one transaction, so if it fails, nothing changes. …"; "A
    rebuild credits each activity against the curriculum as it is today: its items, targets and minimum levels. Only
    whether an EPA was active is judged as of when the activity was completed. …"); Rebuild progress (a primary
    button) → "Rebuild curriculum progress?" ("Every trainee's curriculum progress is recalculated from their completed
    activities, each filed into the semester of its encounter date. An inactive EPA keeps the progress it earned while
    it was active. It is safe to run more than once, and if it fails nothing changes."), confirm "Rebuild progress" →
    "Curriculum progress was rebuilt." with five figures (focused). The command can rebuild one trainee; no page
    offers it.
  - Data volumes: none (nothing adopted: 1.19); typical (KGK: two versions, 16 and 17 items, 16 EPAs); heavy (the
    Administrator's lists: four curricula of three disciplines; an EPA list of 40 across two Colleges and two
    institutions' own EPAs; an adoption history of six rows over three versions of two disciplines; an EPA title of 150
    characters; an items table of 17 rows with three of KGK's own).

CONTENT (real scenario values from the T387 replay, D = 2026-10-30):
  - The College: College of Paediatricians of South Africa (CPSA), speciality Paediatrics, sub-speciality (discipline)
    Paediatrics.
  - Paediatric EPA Curriculum 11.1 (effective 2026-01-01): 15 national items, PAED-001 to PAED-015, every one "Set by
    the College", all pinned to CPSA Paediatric Entrustment Scale v11.1 (six rungs: 1, 2, 3a, 3b, 4, 5); PAED-002
    "3 per semester", decided each semester. With KGK's own item, 16.
  - Paediatric EPA Curriculum 11.2 (effective 2026-10-30, the College's clone of 11.1, opened at 6.31): 16 national
    items, adding PAED-016 (1 per academic year) and PAED-011 at "2 per academic year". With KGK's own item, 17.
  - Also offered for adoption: Demo College's General Medicine / General Internal Medicine: IM Core Curriculum
    (2026.1). Not offered: Neonatology EPA Curriculum 1.0 (held back by the College, no items).
  - PAED-012 "Communicating with and counselling patients, caregivers and healthcare teams": paused by the College at
    6.17, Inactive at 6.22, restored at 6.23.
  - KGK's own EPA: KGK-001 "Running a paediatric outreach clinic at a district hospital", "Plans and runs a monthly
    outreach clinic at a North West district hospital, with its referral follow-up.", Elective, Paediatrics /
    Paediatrics, Active. Its item on 11.1 and on 11.2: 1 per academic year, No decision cadence, the trainee's general
    panel, CPSA ladder, minimum 3a, no year-by-year minima, completion window 12, tools: Direct observation.
  - Adoptions: 11.1 adopted D (Act 1), Superseded at 6.32; 11.2 adopted D, Active.
  - Scales, read-only to her: CNSA Neurology Entrustment Scale (6 levels, added at 6.5), CPSA Paediatric Entrustment
    Scale v11.1 (6), O-R Scale (5).
  - Dr Anele Dlamini: on 11.1; start 2024-01-15, expected completion 2028-01-14, moved to 2028-07-14 at 6.34, still on
    11.1. Trainees lists three current trainees (Dlamini, Mahlangu, Ndlovu).
  - Dr Sipho Ndlovu: start 2026-01-15, expected completion 2030-01-14. On 11.1, sixteen rows ("Each semester · 10
    EPAs", "Once a year · 6 EPAs"), PAED-002 "1 of 3 this semester". Moved: "Trainee profile saved. 1 completion was
    checked against 11.2, and 1 counts towards it."; then seventeen rows ("Each semester · 10 EPAs", "Once a year · 7
    EPAs"), PAED-002 "1 of 3 this semester" at once, PAED-016 and KGK-001 "0 of 1 in 2026", PAED-011 "0 of 2 in 2026";
    KGK-001 marked "Kgosi Kgari Teaching Hospital's own". His Mini-CEX picker offers ten EPAs, PAED-016 among them.
  - The rebuild's result: Activities re-read 15, Curriculum items credited 14, Semester tallies written 9, Stale tallies
    removed 0, Completions re-stamped 15.
  - Out of scope for her: the Demo College's EPA-001 (id 1) and IM Core Curriculum (id 1). KGK-001 is EPA 17; 11.1 is
    curriculum 2, 11.2 curriculum 4 (ids change on every replay: draw them as given).

WHAT FLOW 15 MAY CHANGE OF WHAT EARLIER FLOWS BUILT, AND WHAT IT MAY NOT:
  May not: the shell (flow 01); Home's Curriculum card (flow 12); the trainee profile's layout, its end section and its
    title (flows 11 and 13); My progress and its marks (flow 05; T357 is its own task); the STAR register (flow 09);
    the College's own pages, a curriculum's details, its clone and its "open for adoption" box (flow 16); the rules
    above (one active adoption per discipline; a kept pin; a move only into the active adoption, replayed in the same
    save; national is the College's, local the institution's; an out-of-scope record is not found).
  May, for these pages: where adoption happens and how a curriculum row reads for an institution; the adoption's
    confirmation; the EPA list's marks, row actions and Create; an EPA's page title, its read-only and not-found
    states, its deactivate words; the items page's structure, its own-item add and edit, its marks and its 390 px
    layout; a move's confirmation on the profile and the words under the picker; the scales page's words; the rebuild
    page's failure and its button; stacking at 390. Mark PROPOSED any rule change (a role newly allowed, a new route,
    a new action, a new refusal, a new confirmation, data a page does not read today): those are mine. The runbook's
    Expects change with the words.
  Say which of these each variation changes, and why.

SCREENS, in order:
  1. Where KGK adopts and keeps its curricula (structure A): nothing adopted (1.19, 1.20 before Adopt); adopted 11.1
     (1.20); the same version again (1.20's second press: draw what the page offers instead); 11.2 adopted, 11.1
     superseded with Dr Dlamini still on it (6.32, 6.33); the adoption's confirmation and its cancel; a held-back
     version refused (T301, no capture); the Administrator, no institution chosen, then KGK chosen (unplayed); loading;
     a load error; 390 px.
  2. A curriculum's items as KGK reads them (structure B): 11.1 read-only with the standing notice and nothing of KGK's
     (1.21); PAED-012 paused (6.22); the own-item form before a rung is chosen (6.26); KGK's item added and the form
     gone (6.26); 11.2 with no KGK item, then added (6.33); editing KGK's item; the Remove dialog and removed; not found
     (the Demo IM Core curriculum's items); loading; a load error; 390 px with the edit open.
  3. The EPA list and an EPA's page: nothing adopted (1.19: no Create she cannot complete); the adopted fifteen (1.21);
     PAED-012 Inactive (6.22); KGK-001 created, its page and the list marking it (6.25); a national EPA opened from the
     list (Q2); an out-of-scope EPA typed by id (Q2); KGK-001 deactivated (the dialog, then Inactive) and reactivated;
     a duplicate code refused; loading; a load error; 390 px; the Administrator's list of 40 with owners named.
  4. The move (on flow 13's profile page): Dr Dlamini saved on 11.1 with the curriculum untouched (6.34); Dr Ndlovu
     moved to 11.2 (6.36): before save, the confirmation, the result, the picker reopened offering 11.2 alone; a move
     refused (a stale page offering a superseded version).
  5. One proposal each: Entrustment scales read-only (1.22, 6.9; 390); the rebuild (6.38: the page, the dialog, the
     result, a failure with nothing changed, 390); My progress on 11.2 after the move (6.37, 6.39).

STEPS: 1.19–1.22, 6.9, 6.22, 6.25, 6.26 and 6.32–6.39 (16). Pasted verbatim below (Role / Route / Do / Expect). In them,
  D is the day the story is replayed and J the latest 15 January on or before it. Where an Expect quotes today's wording
  or layout, that is today's page, not a requirement on the new one. 6.22's Entrustment decisions is flow 09's; 6.34's
  and 6.36's Trainees and profile layout are flows 11 and 13's; 6.35, 6.37 and 6.39 are flow 05's (keep them; draw only
  what this flow changes).

STATES TO SHOW:
  Every state named in SCREENS; loading for every page (the header and a skeleton, no action offered until the read is
  in); a load error (a sentence and Try again, never an exception's text, and no empty state under it); each
  confirmation, its cancel, and the result after it with the focus on it; a refusal beside the field or row that caused
  it, keeping what was typed; 390 px for every page, long titles wrapping, nothing sideways; the heavy volumes.

REQUIREMENTS FROM KNOWN DEFECTS:
  - T291 (items 1 and 4's page half, its 1.19 widening, C17 and the sweep): a list offers Edit only where the command
    admits the caller (DATA: the EPA's own "may change" flag and its owner, read from the query, never from roles); a
    national row says why it has no Edit, or offers a read-only view (Q2); KGK's own EPA is marked under its title
    ("Your institution's own EPA"; to the Administrator, "Kgosi Kgari Teaching Hospital's own EPA"); Create EPA and the
    empty state's invitation only when an adopted discipline exists, else the empty state says to adopt first, linked;
    an EPA outside her scope reads as not found (Q2). The commands keep the same rule (PROPOSED: CreateEpa and
    UpdateEpa refuse a discipline KGK has not adopted).
  - T301: a version is "Open" or "Held back" for adoption; the picker offers only open versions, and adopting a
    held-back one is refused before anything is written: "This curriculum version is not open for adoption." (a new or
    cloned version starts held back; the College's box is flow 16's).
  - T326 (the scales half): the scales page's subtitle fits a reader.
  - T323 (the item editor's half): at 390 px nothing on the items page scrolls sideways, the edit included.
  - T329 / A6 and T272: a skeleton while loading, the alert on a load error with no empty state under it; no page
    prints an exception's text; a failed save says what failed and only a real duplicate reads "already exists".
  - T325 (its AdoptedOn half): an adoption is dated by the South African day (today the UTC day).
  - A picker offers exactly what the command it feeds accepts (DESIGN.md's picker rule): the adoption picker leaves out
    the version KGK has actively adopted; the profile's offers the pin and the active adoption (T304, built).
  - Every dialog names its target, its trigger is an outline button, it opens on Cancel, and the result takes the
    focus; a refusal keeps what was typed. One <h1>, then <h2> in order. People and institutions by name.
  - Done since the old brief, keep them: T304 (the kept pin, the move's replay and its result, the pickers); T291 item
    4 (Home's card counts what KGK adopted, observed 1.21); T239 (row actions named for their row); T222/T223 (the
    item pickers leave out what the curriculum holds; a national and a local item never share an EPA); T158/T196 (a
    paused EPA's item marked "(inactive: not in force)"; a pause keeps earned credit).

QUESTIONS THE DESIGN MUST ANSWER:
  1. (Mine) Does adoption merge into Curricula, so that an institution has one page listing the versions it trains
     against and the versions it may adopt, each row with its state for KGK and an Adopt action? And how does the
     Administrator adopt for an institution (today a page her menu does not list)?
  2. (Mine) An EPA outside her scope, typed by id: not found, or read-only with its College named? And a national EPA
     in her adopted discipline: no action ("Set by the College") or a read-only View?
  3. How "the College's, read-only" is told from "KGK's own, editable" on the items table and the EPA list, the same
     way on both (and on My progress, T357).
  4. Rebuilding one trainee's progress is not built (the command takes a trainee): offer it on the profile, or is
     T304's replay at a move or a start change enough?
  5. (Mine) T301's words and default: "Open" / "Held back", a new version held back.
  6. (Mine) Adopting a newer version asks first, naming what it supersedes and how many registrars stay on the old one?
  7. (Mine) The adoption picker: leave out the version already adopted; offer an older version (going back)?
  8. (Mine) A move asks first, saying it cannot be undone and that his evidence is counted again?
  9. (Mine) A superseded version stays readable to KGK while any registrar is on it, not only while it holds KGK's
     own item?
  10. (Mine) What a curriculum row says to KGK: its state for KGK (Adopted, Superseded, ‹N› registrars) or the College's
      Open/Held back?
  11. A local EPA's deactivate words: KGK's trainees, not "every institution".
  12. (Mine) Create EPA only with an adopted discipline; the commands refuse an unadopted one.
  13. The scales page for a reader: its words; every scale, or those KGK's curricula use?
  14. The items page's own-item add and edit: in the table, in a note panel, or a form page of its own?
  15. (Mine) T242: the College's add of an EPA KGK holds as its own item stays refused, naming KGK?
  16. (Mine) The rebuild's button and failure: an outline trigger? the failure without the exception's text.
  17. (Mine) What the build folds in.

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
  (EpaOptionLabel), a paused one with "(no longer in use)" where a trainee reads it.

ASK:
  - Round 1: 2–3 structures as wireframes for screen 1 (Prof Mbatha at 1.19, 1.20 and 6.32–6.33; devadmin choosing
    KGK) and for screen 2 (Prof Mbatha at 1.21, 6.26 and 6.33, and editing KGK's item at 390), each with its reasoning
    and its step count; 2–3 variations at full fidelity each for screen 3 (1.19, 1.21, 6.25 and the out-of-scope id)
    and for screen 4 (6.34, 6.36); ONE proposal at fidelity each for screen 5's three pages. At 1280 and 390 px. With
    them, two boards of their own: a Spec board (FIRST, whether the screenshots arrived; then every component each
    variation uses, NEW marked; the DESIGN.md rules each changes; everything PROPOSED; the data a page would need that
    it does not read today, marked DATA) and a Steps board (each variation's journeys counted, presses + fields typed,
    at 1280 and 390, and which of the 16 Expects below it would change). Stop and wait for my pick.
  - Round 2: the chosen designs with every screen and state above, at 1280 and 390 px, every confirmation, and where
    the focus lands after each action; with a Spec board (every NEW and changed component, its classes, the DESIGN.md
    text each changes, the tokens, the DATA, and every message by kind with its words) and a Steps board (each of the
    16 steps and what its Expect will say once built, with each journey's count: adopting a newer version; adding
    KGK's own EPA and item; moving a registrar; rebuilding progress).
  - Name every design-system component you use, and mark anything else NEW. Say which DESIGN.md rule a variation
    changes (§ Table system's "A row the caller cannot change" and its owner mark; the picker rule; § Button system's
    destructive action; § Form system's refusals; § Page-level patterns; the NavMenu's owner table).
  - Mark PROPOSED anything not built: a route, an action, a confirmation, a refusal, a rule change, a role newly
    allowed.
  - Flag edge cases: an institution adopting a second discipline (Demo IM Core) beside Paediatrics; a College version
    closed after KGK adopted it (KGK keeps it); a superseded version with no registrar left on it; a move while the
    registrar's completion is being credited elsewhere (the move waits, T304); KGK's own EPA whose discipline KGK no
    longer adopts; two institutions' own EPAs with the same code on the Administrator's list; an EPA title of 150
    characters; a registrar on 11.1 when KGK supersedes 11.2 in turn with a 11.3.
  - Run an accessibility review against WCAG 2.1 AA: one h1, headings in order, every table captioned, every row
    action named for its row, the owner mark in words (never colour alone), focus after every action and
    confirmation, refusals tied to their fields (aria-describedby), targets ≥ 24 px (44 at 390).

ATTACHED: act-1/1.19-2-epas-empty.png, act-1/1.19-3-epa-create-no-subspeciality.png, act-1/1.20-1-adopted.png,
  act-1/1.21-1-institution-epas.png, act-1/1.21-3-institution-items.png, act-1/1.22-1-scales-read-only.png,
  act-6/6.25-1-kgk001-saved.png, act-6/6.26-3-own-item-added.png, act-6/6.32-1-adopted-11.2.png,
  act-6/6.33-1-mbatha-curricula-list.png, act-6/6.36-1-ndlovu-moved-to-11.2.png and act-6/6.38-4-rebuilt.png, all
  from 2026-10-30 on the shell as built. Every capture dated 2026-09-26 is an older frame (a Title Case sidebar with
  Logout, an email in the top bar): read its body only. No attached capture shows a password.
```

---

## 2. The journey

| Step | Page | Who does | They must be able to see |
|---|---|---|---|
| 1.19 | `/admin/curricula` → `/admin/epas` | Prof Mbatha (InstitutionalAdmin) | "No curricula yet" and where to adopt; "No EPAs yet". **EPAs offers Create EPA and "Create the first EPA…", whose form has an empty Sub-speciality list (T291, observed)** |
| 1.20 | `/admin/adoptions` | Prof Mbatha | no institution picker; Adopt off until chosen; "Curriculum adopted." and the row (11.1, `D`, Active); the second Adopt refused; the supersede note. **The picker offers what Adopt refuses (new); `<h3>` sections** |
| 1.21 | `/admin/epas` → `/admin/curricula` → `/admin/curricula/{Id:int}/items` → `/` | Prof Mbatha | 15 PAED EPAs; Curricula one row with Items only; the items read-only, "Set by the College", the own-item form saying KGK must first create a local EPA; Home's card 1 and 1 (flow 12's, done). **Edit on all 15 national EPAs (T291 item 1)** |
| 1.22 | `/admin/entrustment-scales` | Prof Mbatha | two scales, no Create, no actions column. **"Define and maintain…" (T326)** |
| 6.9 | `/admin/entrustment-scales` | Prof Mbatha | three scales (CNSA 6, CPSA 6, O-R 5); read-only; nothing sideways at 390 |
| 6.22 | `/admin/epas` → `/admin/curricula` → `/admin/curricula/{Id:int}/items` → `/admin/entrustment-decisions` | Prof Mbatha | PAED-012 Inactive; its item "(inactive: not in force)"; Dr Molefe's sixteen STARs (flow 09's). **Edit on every national EPA, and Create EPA (T291)** |
| 6.25 | `/admin/epas` → `/admin/epas/new` → `/admin/epas/{Id:int}` → `/admin/epas` | Prof Mbatha | only Paediatrics / Paediatrics offered; KGK-001 saved, Active, hers. **Its page reads "Edit EPA"; the list puts KGK-001 first under the College's name, unmarked, with Edit on all 16 (T291)** |
| 6.26 | `/admin/curricula` → `/admin/curricula/{Id:int}/items` | Prof Mbatha | Items only; the own-item form, KGK-001 alone, the suggested scale, Add off until a rung; "Curriculum item added."; the row "Your institution's own item" with Edit and Remove |
| 6.32 | `/admin/adoptions` | Prof Mbatha | open versions offered, Neonatology not; 11.2 Active, 11.1 Superseded. **Adopt asks nothing and says nothing about the registrars left on 11.1 (new)** |
| 6.33 | `/admin/curricula` → `/admin/curricula/{Id:int}/items` | Prof Mbatha | 11.2 and 11.1, Items only; 11.2's 16 national rows, no KGK row until added. **Both read "Active": the College's flag, not KGK's state (new)** |
| 6.34 | `/admin/trainees` → `/admin/trainees/edit` | Prof Mbatha | "Anele Dlamini's programme"; the picker 11.1 and 11.2, 11.1 chosen; "Trainee profile saved."; still on 11.1 (T304, done) |
| 6.35 | `/portfolio/progress` | Dr Ndlovu (Trainee) | sixteen rows on 11.1, PAED-016 not among them (flow 05's) |
| 6.36 | `/admin/trainees` → `/admin/trainees/edit` | Prof Mbatha | "Trainee profile saved. 1 completion was checked against 11.2, and 1 counts towards it."; reopened, 11.2 alone. **No question before a move that cannot be undone (new)** |
| 6.37 | `/portfolio/progress` → `/activities/new` | Dr Ndlovu | seventeen rows, PAED-002 "1 of 3 this semester" at once; the Mini-CEX picker offers PAED-016 (flows 05 and 03's) |
| 6.38 | `/admin/curricula` → `/admin/curriculum-progress` | devadmin (Administrator) | the outline "Curriculum progress"; what a rebuild does; "Rebuild curriculum progress?" opening on Cancel; five figures, Stale tallies removed 0. **A failure prints the exception's text (T272)** |
| 6.39 | `/portfolio/progress` | Dr Ndlovu | the counts carried over; PAED-011 "0 of 2 in 2026"; "Once a year · 7 EPAs" |

## 3. States to design

Each screenshot is under `design/baseline/`. How to reach each is in `states.md` (the line numbers below are its rows on
2026-11-02). The step captures are from the T387 replay (2026-10-30), on the shell as built; the `states/` captures of
the six catalogue pages are from 2026-09-26, on the old frame: use the step capture where there is one, and the old one
for its body only.

| Page | State | Screenshot | Who | Reached at (`states.md` line) |
|---|---|---|---|---|
| `/admin/curricula` | None, an institution | `act-1/1.19-1-curricula-empty.png` (`states/curricula-list--empty.png`) | Prof Mbatha | Step 1.19 (:777) |
| `/admin/curricula` | Adopted, items only | `act-1/1.21-2-institution-curricula.png` (`states/curricula-list--institution.png`) | Prof Mbatha | Step 1.21 (:779) |
| `/admin/curricula` | Two versions, an institution | `act-6/6.33-1-mbatha-curricula-list.png` (`states/curricula-list--institution-versions.png`) | Prof Mbatha | Step 6.33 (:781) |
| `/admin/curricula` | The Administrator's header | `act-6/6.38-1-curricula-header.png` | devadmin | Step 6.38 |
| `/admin/curricula` | Loading; narrow | `states/curricula-list--loading.png`, `states/curricula-list--narrow.png` | Dr Kruger | held read; Step A.7.10 (:782, :783) |
| `/admin/adoptions` | Nothing adopted | `act-1/1.20-0-adoptions-before.png` (`states/adoptions-list--empty.png`) | Prof Mbatha | Step 1.20, before Adopt (:840) |
| `/admin/adoptions` | Adopted | `act-1/1.20-1-adopted.png` (`states/adoptions-list--adopted.png`) | Prof Mbatha | Step 1.20 (:841) |
| `/admin/adoptions` | Adopted twice, refused | `act-1/1.20-2-adopt-refused.png` (`states/adoptions-list--refused.png`) | Prof Mbatha | Step 1.20 (:842) |
| `/admin/adoptions` | Superseded | `act-6/6.32-1-adopted-11.2.png` (`states/adoptions-list--superseded.png`) | Prof Mbatha | Step 6.32 (:843) |
| `/admin/adoptions` | An administrator, no institution; KGK chosen | `states/adoptions-list--choose-institution.png`, `states/adoptions-list--administrator.png` | devadmin | typed (:844, :845) |
| `/admin/adoptions` | Loading; narrow | `states/adoptions-list--loading.png`, `states/adoptions-list--narrow.png` | Prof Mbatha | held read; Step 6.32 at 390 (:846, :847) |
| `/admin/epas` | None, an institution | `act-1/1.19-2-epas-empty.png` (`states/epas-list--empty.png`) | Prof Mbatha | Step 1.19 (:757) |
| `/admin/epas` | The adopted fifteen | `act-1/1.21-1-institution-epas.png` (`states/epas-list--institution.png`) | Prof Mbatha | Step 1.21 (:759) |
| `/admin/epas` | One inactive | `act-6/6.22-1-mbatha-epa-list.png` | Prof Mbatha | Step 6.22 |
| `/admin/epas` | With a local EPA | `act-6/6.25-2-epa-list-with-kgk001.png` (`states/epas-list--local.png`) | Prof Mbatha | Step 6.25 (:761) |
| `/admin/epas` | Loading; load error; narrow | `states/epas-list--loading.png`, `states/epas-list--load-error.png`, `states/epas-list--narrow.png` | Dr Kruger | held read; held 35 s; Step A.7.10 (:762–764) |
| `/admin/epas/new` | Create, an institution | `act-1/1.19-3-epa-create-no-subspeciality.png` (nothing adopted), `states/epa-edit--create-local.png` | Prof Mbatha | Steps 1.19, 6.25 (:770) |
| `/admin/epas/{Id:int}` | KGK-001 saved | `act-6/6.25-1-kgk001-saved.png` | Prof Mbatha | Step 6.25 |
| `/admin/epas/{Id:int}` | Deactivate dialog (the College's words) | `states/epa-edit--deactivate-dialog.png` | Dr Kruger | Step 6.17 (:767) |
| `/admin/epas/{Id:int}` | A local EPA deactivated | `states/epa-edit--local-deactivated.png` | Prof Mbatha | scratch (:774) |
| `/admin/epas/{Id:int}` | Out of scope | `states/epa-edit--not-found.png` (**the defect: a full editable form**) | Prof Mbatha | typed, the Demo EPA-001's id (:775) |
| `/admin/curricula/{Id:int}/items` | Read-only to an institution | `act-1/1.21-3-institution-items.png` (`states/curriculum-items-edit--institution.png`) | Prof Mbatha | Step 1.21 (:792) |
| `/admin/curricula/{Id:int}/items` | A paused EPA | `act-6/6.22-2-mbatha-items-paused.png` | Prof Mbatha | Step 6.22 |
| `/admin/curricula/{Id:int}/items` | The own-item form | `act-6/6.26-2-add-form-before-rung.png` (`states/curriculum-items-edit--own-form.png`) | Prof Mbatha | Step 6.26 (:795) |
| `/admin/curricula/{Id:int}/items` | An own item added | `act-6/6.26-3-own-item-added.png` (`states/curriculum-items-edit--own-added.png`) | Prof Mbatha | Step 6.26 (:796) |
| `/admin/curricula/{Id:int}/items` | 11.2, KGK's item added | `act-6/6.33-2-own-item-on-11.2.png` | Prof Mbatha | Step 6.33 |
| `/admin/curricula/{Id:int}/items` | A row being edited (the College's) | `states/curriculum-items-edit--row-edit.png` | Dr Kruger | Step 6.30 (:797) |
| `/admin/curricula/{Id:int}/items` | Remove dialog; removed | `states/curriculum-items-edit--remove-dialog.png`, `states/curriculum-items-edit--removed.png` | Prof Mbatha | after 6.33; scratch (:798, :799) |
| `/admin/curricula/{Id:int}/items` | Out of scope | `states/curriculum-items-edit--not-found.png` | Prof Mbatha | typed, the Demo IM Core curriculum (:800) |
| `/admin/curricula/{Id:int}/items` | Loading; narrow | `states/curriculum-items-edit--loading.png`, `states/curriculum-items-edit--narrow.png` | Dr Kruger | held read; Step A.7.10 (:801, :802) |
| `/admin/entrustment-scales` | Read-only | `act-1/1.22-1-scales-read-only.png`, `act-6/6.9-1-mbatha-reads-scales.png` (`states/entrustment-scales-list--read-only.png`) | Prof Mbatha | Steps 1.22, 6.9 (:804) |
| `/admin/entrustment-scales` | Loading; narrow | `states/entrustment-scales-list--loading.png`, `act-6/6.9-2-mbatha-reads-scales-390.png` (`states/entrustment-scales-list--narrow.png`) | Prof Mbatha | held read; Step 6.9 at 390 (:807, :808) |
| `/admin/trainees/edit` | Saved on 11.1 | `act-6/6.34-3-profile-saved.png` | Prof Mbatha | Step 6.34 |
| `/admin/trainees/edit` | Moved to a new version | `act-6/6.36-1-ndlovu-moved-to-11.2.png` (`states/trainee-profile-edit--moved.png`) | Prof Mbatha | Step 6.36 (:999) |
| `/admin/curriculum-progress` | Before; dialog; rebuilt | `act-6/6.38-2-curriculum-progress-page.png`, `act-6/6.38-3-rebuild-dialog.png`, `act-6/6.38-4-rebuilt.png` (`states/curriculum-progress-rebuild--default.png`, `--dialog`, `--result`) | devadmin | Step 6.38 (:1039–1041) |
| `/admin/curriculum-progress` | Failed; narrow | `states/curriculum-progress-rebuild--failed.png`, `states/curriculum-progress-rebuild--narrow.png` | devadmin | scratch; Step 6.38 at 390 (:1042, :1043) |
| `/portfolio/progress` | On 11.1, before the move | `act-6/6.35-1-ndlovu-progress.png` (2026-10-27) | Dr Ndlovu | Step 6.35 |
| `/portfolio/progress` | On 11.2 after the move; after the rebuild | `act-6/6.37-1-ndlovu-progress.png` (`states/my-progress--after-move.png`), `states/my-progress--after-rebuild.png` | Dr Ndlovu | Steps 6.37, 6.39 (:389, :390) |

Not captured, so describe them in words (`states.md`; `coverage.md` § Flows and states not played):
- Never built: a held-back version refused at adoption (T301); the adoption's confirmation (Q6); a move's confirmation
  (Q8); a read-only national EPA or a single not-found for an out-of-scope one (Q2); the own-EPA mark on the EPA list
  (T291 C17); a single trainee's rebuild (Q4; `coverage.md:203`).
- Named but never taken: `epa-edit--local-reactivated` (BRIEF § 10; `states.md:774`'s second half).
- Unplayed by the story: the Administrator adopting for an institution (`coverage.md:198`); an InstitutionalAdmin
  deactivating and reactivating a local EPA (`coverage.md:202`, browser-checked in T196); T242's College add
  (`coverage.md:205`).
- Load errors: Adoptions and an EPA's page draw the exception's text above an empty body (code); the items page's
  `LoadError` slot (uncaptured).

**Belongs to other flows:** flow 05: My progress (6.35, 6.37, 6.39) and its KGK-001 badge (T357). Flow 03: the
Mini-CEX's EPA picker (`act-6/6.37-2…`). Flow 09: the STAR register (`act-6/6.22-3…`). Flows 11 and 13: Trainees and
the profile's layout (`act-6/6.34-0…`, `-2…`, `-4…`). Flow 12: Home's Curriculum card (`act-1/1.21-4…`). Flow 16: the
College's EPA, curriculum, clone and items pages (`states/epa-edit--edit.png` and its siblings, `curricula-list--college`,
`curriculum-items-edit--college`, `--no-items`, `--inactive`), and T326's empty-items branch (6.13).

## 4. Attach

Paths are relative to `design/baseline/`. The twelve key screenshots were read from their step's Actual and four were
opened on 2026-11-02 (6.25-2, 6.32-1, 6.33-1, 6.36-1); none shows a password or a one-time link. The profile capture
shows the registrar's scenario address, which is the page's own record.

**Key screenshots (attach these first, with § 1):**
1. `act-1/1.19-2-epas-empty.png`: "No EPAs yet" with Create EPA before anything is adopted
2. `act-1/1.19-3-epa-create-no-subspeciality.png`: the create form with nothing to choose
3. `act-1/1.20-1-adopted.png`: "Curriculum adopted.", the two `<h3>` sections, the picker
4. `act-1/1.21-1-institution-epas.png`: Edit on every national EPA
5. `act-1/1.21-3-institution-items.png`: the read-only items under the standing notice, the own-item form's empty state
6. `act-1/1.22-1-scales-read-only.png`: "Define and maintain…" to a reader
7. `act-6/6.25-1-kgk001-saved.png`: KGK-001's page, "Edit EPA", Back to EPAs
8. `act-6/6.26-3-own-item-added.png`: "Your institution's own item" with Edit and Remove beside fifteen "Set by the College"
9. `act-6/6.32-1-adopted-11.2.png`: 11.2 Active, 11.1 Superseded, nothing asked
10. `act-6/6.33-1-mbatha-curricula-list.png`: both versions "Active"
11. `act-6/6.36-1-ndlovu-moved-to-11.2.png`: the move's result on flow 13's profile page
12. `act-6/6.38-4-rebuilt.png`: the rebuild's five figures

**Where KGK adopts and keeps its curricula:** `act-1/1.19-1-curricula-empty.png`, `act-1/1.20-0-adoptions-before.png`,
`act-1/1.20-2-adopt-refused.png`, `act-1/1.21-2-institution-curricula.png`, `act-6/6.38-1-curricula-header.png`, and
(old frame) `states/adoptions-list--choose-institution.png`, `states/adoptions-list--administrator.png`,
`states/adoptions-list--loading.png`, `states/adoptions-list--narrow.png`, `states/curricula-list--narrow.png`.

**EPAs:** `act-6/6.22-1-mbatha-epa-list.png`, `act-6/6.25-2-epa-list-with-kgk001.png`, and (old frame)
`states/epas-list--loading.png`, `states/epas-list--load-error.png`, `states/epa-edit--deactivate-dialog.png`,
`states/epa-edit--local-deactivated.png`, `states/epa-edit--not-found.png` (the defect, not the state).

**A curriculum's items:** `act-6/6.22-2-mbatha-items-paused.png`, `act-6/6.26-2-add-form-before-rung.png`,
`act-6/6.33-2-own-item-on-11.2.png`, and (old frame) `states/curriculum-items-edit--row-edit.png`,
`states/curriculum-items-edit--remove-dialog.png`, `states/curriculum-items-edit--removed.png`,
`states/curriculum-items-edit--not-found.png`, `states/curriculum-items-edit--narrow.png`.

**The consequences:** `act-6/6.9-1-mbatha-reads-scales.png`, `act-6/6.9-2-mbatha-reads-scales-390.png`,
`act-6/6.34-3-profile-saved.png`, `act-6/6.35-1-ndlovu-progress.png`, `act-6/6.37-1-ndlovu-progress.png`,
`act-6/6.38-2-curriculum-progress-page.png`, `act-6/6.38-3-rebuild-dialog.png`, and (old frame)
`states/curriculum-progress-rebuild--failed.png`, `states/curriculum-progress-rebuild--narrow.png`.

That is 46 files.

**Do not attach:** the captures of defects T304 removed (`act-6/6.34-1-profile-save-refused.png`, `states/trainee-profile-edit--curriculum-refused.png`, `states/my-progress--before-rebuild.png`); the duplicates (`act-6/6.26-1-curricula-list-items-only.png` is `act-1/1.21-2…`'s, `act-6/6.39-1-ndlovu-progress.png` is 6.37's); other flows' pages (`act-1/1.21-4-mbatha-home-adopted.png`, flow 12's; `act-6/6.22-3-molefe-stars-paused.png`, flow 09's; `act-6/6.34-0-trainees.png`, `act-6/6.34-2-profile-reopened.png`, `act-6/6.34-4-profile-opened.png`, flows 11 and 13's; `act-6/6.37-2-ndlovu-picker.png`, flow 03's); the `states/` captures a step capture replaces (`curricula-list--empty`, `--institution`, `--institution-versions`; `adoptions-list--empty`, `--adopted`, `--refused`, `--superseded`; `epas-list--empty`, `--institution`, `--local`; `epa-edit--create-local`; `curriculum-items-edit--institution`, `--own-form`, `--own-added`; `entrustment-scales-list--read-only`, `--narrow`; `curriculum-progress-rebuild--default`, `--dialog`, `--result`; `trainee-profile-edit--moved`; `my-progress--after-move`, `--after-rebuild`).

## 5. Known problems this design must solve

The evidence column is for the operator and for Claude Code. Attach only what § 4 lists. Code is cited at `a87041ac`.
"Observed" is seen in a capture or a replay's Actual; "code" is read in the source and not replayed; "inferred" is
neither.

| Task | What it means for the design | Evidence |
|---|---|---|
| **T291** (P3, queued; items 1 and 4 here, with its 1.19 widening, C17 and the sweep; items 2, 3, 6 and 8 are flows 16, 17 and 18's) | Q2, Q3, Q12. **Item 1, still true:** `EpasList.razor:50` offers Edit on every row; `UpdateEpa` admits only the College for a national EPA (`A/Epas/UpdateEpa.cs:98–104`; observed 1.21, 6.22, 6.25: Edit on all 16). `EpaDto` carries no owner and no "may change" flag (`A/Epas/EpaDto.cs:5–16`), so the page cannot read the rule. **C17, still true:** KGK-001 is unmarked (observed 6.25). **1.19 widening, still true:** Create EPA is unconditional (`EpasList.razor:10–13`) and the empty state invites "Create the first EPA…" (`:17`), while the form's list holds only adopted disciplines (`A/Institutions/Queries/GetSubSpecialitiesList/GetSubSpecialitiesListQueryHandler.cs:32–41`; observed 1.19). **The commands, still true:** `A/Epas/CreateEpa.cs:68–80` checks an InstitutionalAdmin only against her own institution, so it accepts her own EPA in any discipline; `UpdateEpa.cs:116–121` moves one the same way. **The sweep, still true:** `GetEpaByIdQuery` admits any InstitutionalAdmin to every national EPA (`A/Epas/GetEpas.cs:178–186`), wider than the list's rule (`:91–117`), so the Demo EPA-001 opens as a full form with Save and Deactivate (`EpaEdit.razor:94–100`). **Item 4, done** (`e53d4ae7`, flow 12): Home's card counts what KGK adopted (observed 1.21: 1 and 1). | `act-1/1.21-1…`, `act-6/6.25-2…`, `act-1/1.19-2…`, `-3…`, `states/epa-edit--not-found.png` |
| **T301** (P3, queued) | Q5, Q7. Still true: `AdoptCurriculumCommand` loads the curriculum with no open check (`A/Adoptions/AdoptCurriculum.cs:47–52`); only the picker filters (`A/Adoptions/GetAdoptableCurricula.cs:45`). The flag's label and a new version's default are the College's pages (flow 16); here, the refusal and the words an institution reads ("Open" / "Held back"). | `act-6/6.32-1…` (Neonatology left out by the picker only) |
| **T304** (P2, **done** `95d4e950`, `a27983fd`) | Re-based on what was built. Done: an unchanged curriculum keeps its pin (observed 6.34: saved, profile 3 on adoption 1); a move replays in the same save and says so, its first sentence bold (`A/Trainees/UpdateTraineeProfile.cs:54–61`, `W/Admin/Trainees/TraineeProfileEdit.razor:482–483`; observed 6.36); the picker offers the pin and the active adoption and is asked again after a save (`TraineeProfileEdit.razor:236`, `:500`; observed 6.34, 6.36); 6.37 reads his counts at once and 6.38 removes nothing stale (observed). **Still open on these pages:** a move cannot be undone (a move back is refused, `UpdateTraineeProfile.cs:109–111`, `:181`) and nothing asks first (Q8); Q4's single-trainee rebuild. The old brief's refusal and zeros are dropped. | `act-6/6.36-1…`, `act-6/6.34-3…` |
| **T326** (P3, queued; the scales half) | Q13. Still true: one subtitle for both roles (`EntrustmentScalesList.razor:11`; observed 1.22). New beside it, the same kind (A10): Curricula's "Manage curriculum versions…" and EPAs' "Maintain the entrustable professional activities…" speak to an editor (`CurriculaList.razor:9`, `EpasList.razor:8`; observed 6.33). T326's empty-items branch (6.13) is flow 16's. | `act-1/1.22-1…`, `act-6/6.33-1…` |
| **T323** (P3, queued; the item editor's half) | Q14. Still true: the edit row is a `<td colspan="9">` inside the table's scroll container (`CurriculumItemsEdit.razor:189–190`), so at 390 px the form scrolls sideways (T323's A.7.10). None of the four lists stacks (no `Stack`/`StackWide`). | `states/curriculum-items-edit--narrow.png`, `states/adoptions-list--narrow.png` |
| **T329** (A6; EpasList named; the others the same kind) | Still true: EPAs' `IsLoading` waits on four reads with no error guard (`EpasList.razor:17`, `:75–78`); Adoptions and an EPA's page have no `LoadError` and draw `exception.Message` above an empty body (`AdoptionsList.razor:25`, `:135–138`; `EpaEdit.razor:33`, `:204–207`); the items page and every action print `exception.Message` (`CurriculumItemsEdit.razor:590`, `:675`, `:920`). | `states/epas-list--load-error.png`, `states/adoptions-list--loading.png` |
| **T272** (P3, queued; CreateEpa and the rebuild) | Q16. Still true: every `DbUpdateException` on create reads "An EPA with the same code already exists for this sub-speciality." (`CreateEpa.cs:100–103`); the rebuild's failure prints the exception (`CurriculumProgressRebuild.razor:13`, `:114–117`; observed in T295's states sweep). | `states/curriculum-progress-rebuild--failed.png` |
| **T325** (P3, queued; its AdoptedOn half) | Still true: `AdoptedOn = DateOnly.FromDateTime(DateTime.UtcNow)` (`AdoptCurriculum.cs:77`), so an adoption between 00:00 and 02:00 SAST is dated the day before. | — (code) |
| **T242** (P3, queued; a decision) | Q15. Recommended (c): the College's add stays refused while KGK holds that EPA as its own item, naming KGK. Nothing on her pages changes unless (a) or (b) is picked. | — |
| **T357** (P3, queued; flow 05's) | Q3. My progress marks KGK-001 with a badge ("Kgosi Kgari Teaching Hospital's own"; observed 6.37), where DESIGN.md's T211 rule marks an own item with `.muted .text-sm` words. The EPA list's mark chosen here should be the one T357 adopts. | `act-6/6.37-1…` |
| **New: the adoption picker offers what Adopt refuses** (observed 1.20) | Q7. `GetAdoptableCurriculaQuery` lists every open version (`GetAdoptableCurricula.cs:44–58`), the one KGK has actively adopted included; Adopt then refuses it (`AdoptCurriculum.cs:63–66`). DESIGN.md's picker rule (DESIGN.md:966) asks a picker to offer exactly what its command accepts. 1.20's second press tests the refusal and changes with Q7. | `act-1/1.20-2…` |
| **New: Adopt supersedes with no question** (observed 6.32) | Q6. `AdoptionsList.razor:58`, `:177` send at once; the only word is the standing note (`:60`). Nothing says how many registrars stay on the superseded version, or that KGK's own items are not carried to the new one (6.33's Expect). | `act-6/6.32-1…` |
| **New: Curricula's Status is the College's flag** (observed 6.33) | Q10. `CurriculaList.razor:59` prints `IsActive` (the College's release, T301), so KGK's superseded 11.1 reads "Active" beside 11.2. KGK's own state (adopted, superseded, registrars on it) is not on the page. | `act-6/6.33-1…` |
| **New: a superseded version is readable only while it holds KGK's own item** (code; inferred consequence) | Q9. `A/Curricula/CurriculumAdminScope.cs:19–27`: the second arm opens a superseded version only if it holds an item of the institution's own. Dr Dlamini stays on 11.1, so had KGK added no item there, Prof Mbatha could no longer open the items Dr Dlamini is measured against (inferred: not played). | — |
| **New: the Administrator's menu has no Curriculum adoptions** (code) | Q1. `Nav/NavItems.cs:142–149` (DESIGN.md:586): she reaches `/admin/adoptions` only by its address or the nothing-adopted links on Invite and Admit, which name the InstitutionalAdmin's task. Her adoption for an institution is unplayed (`coverage.md:198`). | `states/adoptions-list--choose-institution.png` |
| **New: a local EPA's deactivate words** (code) | Q11. The dialog and the Active help say it stops being a target "for every institution that uses it" (`EpaEdit.razor:112`, `:155–160`), true of a national EPA only; KGK-001 is KGK's alone. | `states/epa-edit--local-deactivated.png` |
| **New: titles, Back links and the EPA's dash** (code; observed 6.25) | An EPA's page is "Edit EPA" for every EPA, with Back to EPAs (`EpaEdit.razor:8–14`); the items page is "Curriculum items" with Back to curricula (`CurriculumItemsEdit.razor:10–21`), the trail repeated, which flows 12 and 13 dropped. The items table writes "PAED-001 - Title" with a hyphen (`:111`), where `EpaOptionLabel` writes an em dash (`A/Epas/EpaOptionLabel.cs:20`). | `act-6/6.25-1…`, `act-1/1.21-3…` |
| **New: an EPA's refusals at the page's top** (code) | An EPA's page shows a `ValidationSummary` and every server refusal at the top (`EpaEdit.razor:21–31`, `:37`); flow 11's rule puts a refusal under its field, focused. The DataAnnotations messages name the model property (T324's validation half). | `states/epa-edit--invalid.png` is flow 16's |
| **New: the EPA list can fail to render** (code; inferred) | `EpasList.razor:39–40` looks up each row's sub-speciality with `Single` in the caller's adopted disciplines; an own EPA in a discipline KGK no longer adopts (possible through the commands, T291's sweep) would throw while rendering (inferred: not reached in the story). The list also reads institutions it never uses (`:71`). | — |
| **New: headings** (code) | `<h3>` sections under the `<h1>` with no `<h2>` (`AdoptionsList.razor:44`, `:63`; `CurriculumItemsEdit.razor:76`, `:329`). | `act-1/1.20-1…` |

**Dropped from the old brief (done since, or decided):**
- T304's refusal of an unchanged save and the moved registrar's zeros (done, `95d4e950`, `a27983fd`; observed 6.34,
  6.37); `states/trainee-profile-edit--curriculum-refused` and `states/my-progress--before-rebuild` are no longer
  states (`states.md` no longer lists them).
- T291 item 4 (done, `e53d4ae7`; observed 1.21).
- The old brief's "scales read-only to her … the College's": scales are the Administrator's, platform-wide
  (`src/Wombat.Domain/Epas/EntrustmentScale.cs`: no College; `GetEpas.cs:192–212` unscoped).
- The old CONTENT's dates (11.1 adopted 2026-09-26) and rebuild figures (Stale tallies removed 1), replaced by the
  T387 replay's; its LAYOUT line (the pre-shell frame); its question 3 (the single-trainee rebuild) is Q4.

## 6. Questions for the operator

Each with a recommendation. The canvas answers 3, 4, 11, 13 and 14 in its variations; 1, 2, 5–10, 12 and 15–17 are
the operator's, and 1 and 14 decide the two structures, so they are answered before round 1.

**Decided 2026-11-02: the operator took Q1–Q17 as recommended**, and with them Q18–Q21 of the round-1 assessment
(`15-institution-adopts-catalogue/round-1-assessment.md` § 8) as it recommends, which refine Q1 (Q18: the
Administrator adopts from an "An institution's curricula" section on her Curricula), Q2's words (Q19: DESIGN.md's
Unavailable pattern, no id shown), Q8's words (Q20: no completions count before the move, the result gives it) and Q7
(Q21: an older version stays adoptable, the boards' P5 dropped, so the move's words are M2's conditional ones). The
round-1 picks are accepted: A1 (with A2's per-version line), B2 (with B1's borrowings), C2's list with C1's page for
KGK's own EPA and the shared national record, and D1; E, F and G confirmed with their corrections. The assessment's
M1–M6 and S1–S16 apply, and the nits in one pass. Round 2's ask is `15-institution-adopts-catalogue/round-2-ask.txt`.

1. **Q1. (The operator's; the old brief's question 1) Does Curriculum adoptions merge into Curricula?** Today an
   institution's Curricula lists only what it has adopted (Items only), and Curriculum adoptions holds the picker and
   the history; the Administrator's menu has neither link to adoptions. **Decided 2026-11-02: as recommended, the Administrator adopting from her Curricula's "An institution's curricula" section (Q18).** *Recommendation:* merge, for the
   InstitutionalAdmin: one page, "Curricula", in two parts: the versions KGK trains against (each with its state for
   KGK, Q10, and Items), and the College versions open to KGK that it has not adopted (each with Adopt, Q6), the
   adoption history folded into the rows (adopted on, superseded on). The "Curriculum adoptions" menu item goes and
   `/admin/adoptions` sends to Curricula (PROPOSED). The Administrator keeps her College-wide Curricula and adopts for
   an institution from that institution's page (flow 18's; until then from Curricula's header, as "Curriculum
   progress" is reached) (PROPOSED). Round 1 draws the merge and today's two pages so the pick is made on the boards.
2. **Q2. (The operator's; T291's sweep) An EPA outside her scope, typed by id: not found, or read-only with its College
   named?** **Decided 2026-11-02: as recommended, with Q19's words: DESIGN.md's Unavailable pattern, showing no id.** *Recommendation:* not found, as flow 14's Unavailable state (a page's own "EPA not found" / "There is no
   EPA with this id that you can open.", PROPOSED words): `GetEpaById` narrows to the list's rule (the national EPAs of
   the disciplines KGK has actively adopted, plus KGK's own), so the list and the page agree (T211, T056), and
   `states.md:775` stands. A national EPA inside her scope opens **read-only**, reached by a View on its row: its code,
   title, description, required knowledge and skills, category, status and "Set by the College of Paediatricians of
   South Africa", no Save and no Deactivate (PROPOSED: the page reads the EPA's "may change" flag, DATA). The
   description and the knowledge are on no other page she can open.
3. **Q3. How "the College's" is told from "KGK's own".** **Decided 2026-11-02: as recommended.** *Recommendation:* one rule on both tables, DESIGN.md's T211
   paragraph: KGK's own EPA or item marked under its title in `.muted .text-sm` words ("Your institution's own EPA",
   "Your institution's own item"; to the Administrator "Kgosi Kgari Teaching Hospital's own EPA"), the national row's
   actions cell saying why it has no Edit ("Set by the College") or offering View (Q2), and the standing notice above
   the table. Words, never colour or a badge alone; T357 adopts the same words for My progress.
4. **Q4. (The old brief's question 3) A single trainee's rebuild.** **Decided 2026-11-02: as recommended.** *Recommendation:* not built: T304 replays a moved
   registrar or a changed start in the same save, which covers 6.37, and the Administrator's rebuild stays for the
   rest. BRIEF § 7 B10 keeps it as future; nothing on the profile offers it.
5. **Q5. (The operator's) T301.** **Decided 2026-11-02: as recommended.** *Recommendation:* as T301: the version's flag is its release for adoption, "Open" /
   "Held back"; a new or cloned version starts held back; Adopt refuses a held-back one before anything is written
   ("This curriculum version is not open for adoption."); an adoption already made is unaffected. Fold it into this
   build (the refusal and the words are here; the College's box is drawn in flow 16 with T301's words). 6.13, 6.29
   and 6.31's Expects (flow 16's) change with it.
6. **Q6. (The operator's) Adopting a newer version asks first.** **Decided 2026-11-02: as recommended.** *Recommendation:* yes, a ConfirmDialog only when an
   adoption supersedes one: "Adopt Paediatric EPA Curriculum (11.2) for Kgosi Kgari Teaching Hospital?"; it supersedes
   11.1 for every new admission; ‹N› registrars stay on 11.1 until each is moved on their profile (DATA: a count of
   the active profiles pinned to the adoption); KGK's own items are not carried over and are added again on 11.2; its
   confirm "Adopt 11.2", its trigger the row's Adopt. A first adoption of a discipline asks nothing. 6.32's Expect
   changes (PROPOSED).
7. **Q7. (The operator's) The adoption picker.** **Decided 2026-11-02: as recommended: an older version stays adoptable under Q6's dialog (Q21; the boards' P5 dropped).** *Recommendation:* it offers only open versions KGK has not actively
   adopted, so "This institution has already adopted this curriculum version." stays in the command for a stale page
   only; 1.20's second press becomes a check that 11.1 is no longer offered. An older version than the active one
   (going back) stays adoptable, under Q6's dialog, which says it supersedes the newer version.
8. **Q8. (The operator's) A move asks first.** **Decided 2026-11-02: as recommended, with no completions count before the save (Q20) and M2's conditional words in place of "cannot be moved back" (Q21).** *Recommendation:* yes: when Save profile would change the curriculum, a
   ConfirmDialog "Move Sipho Ndlovu to Paediatric EPA Curriculum (11.2)?": his ‹N› completions are counted again
   against 11.2 and his figures change at once; he cannot be moved back to 11.1, which KGK has superseded; its confirm
   "Move and save", Cancel keeping the form as typed. A sentence under the picker says the same before Save (PROPOSED).
   A save that keeps the curriculum asks nothing (6.34). 6.36's Expect changes.
9. **Q9. (The operator's) A superseded version stays readable while registrars are on it.** **Decided 2026-11-02: as recommended.** *Recommendation:* yes:
   `CurriculumAdminScope`'s second arm opens a superseded version to KGK while any KGK profile (active or ended) is
   pinned to it or it holds a KGK item (PROPOSED rule); with neither, it leaves her list.
10. **Q10. (The operator's) What a curriculum row says to KGK.** **Decided 2026-11-02: as recommended.** *Recommendation:* its state for KGK in words:
    "Adopted ‹date›" for the active one; "Superseded ‹date› · ‹N› registrars still on it" for an old one (DATA); the
    College's Open/Held back only on the College's and the Administrator's lists (T301).
11. **Q11. A local EPA's deactivate words.** **Decided 2026-11-02: as recommended.** *Recommendation:* the dialog and the Active help name whose trainees it
    stops measuring: "…for Kgosi Kgari Teaching Hospital's trainees" on KGK's own EPA, "…for every institution that
    has adopted it" on a national one, as the item's Remove dialog already does (`CurriculumItemsEdit.razor:771–776`).
    The dialog's title names the EPA ("Deactivate KGK-001?").
12. **Q12. (The operator's; T291's 1.19 widening and its command half) Create EPA.** **Decided 2026-11-02: as recommended.** *Recommendation:* Create EPA and
    the empty state's invitation only when the caller has an adopted discipline (for her) or a College (for the
    College); otherwise the empty state reads "Your institution has not adopted a curriculum yet, so there is no
    discipline to add an EPA to. Adopt one under Curricula." with the link (PROPOSED words). `CreateEpa` and
    `UpdateEpa` refuse an institution's own EPA in a discipline it has not actively adopted (PROPOSED refusal).
13. **Q13. The scales page for a reader.** **Decided 2026-11-02: as recommended.** *Recommendation:* every scale (they are platform-wide; 6.9's three stand),
    under "The rating ladders that assessment forms and committee reviews use. A Wombat administrator sets them; they
    are shown here for reference." (T326; PROPOSED words). No mark for which ones KGK's curricula use.
14. **Q14. The items page's own-item add and edit.** **Decided 2026-11-02: as recommended.** *Recommendation:* the list is the page (a captioned, stacked table
    with the standing notice), and adding or editing KGK's item opens a form page of its own
    (`/admin/curricula/{Id}/items/new` and `/admin/curricula/{Id}/items/{ItemId}`, PROPOSED routes; flow 11's "the
    list is the page and its create form a page of its own"), which ends T323's edit row in a cell at 390. The College's
    items page (flow 16) would follow the same shape; flow 16 confirms it. Round 1 draws it beside an in-place note
    panel (flow 12's) and today's edit row.
15. **Q15. (The operator's) T242.** **Decided 2026-11-02: as recommended.** *Recommendation:* (c): the College's national add stays refused while an
    institution holds that EPA as its own item, and the refusal names the institution; nothing on KGK's pages changes.
16. **Q16. (The operator's) The rebuild.** **Decided 2026-11-02: as recommended.** *Recommendation:* the trigger stays a primary button (a rebuild loses
    nothing and is safe to repeat; it is not a destructive action), the dialog opens on Cancel as built; a failure
    reads "The rebuild failed, and nothing was changed. Try again, or come back in a few minutes." with no exception's
    text (PROPOSED words; T272); "Semester tallies written" keeps its name.
17. **Q17. (The operator's) What the build folds in.** **Decided 2026-11-02: as recommended.** *Recommendation:* fold T291 items 1 and 4's page half (done),
    C17, the 1.19 widening, the sweep (with Q2) and its command rule (Q12); T301 (Q5); T326's scales subtitle and the
    same words on Curricula and EPAs; these pages' halves of T323 (the item editor), T329 (A6), T272 (CreateEpa, the
    rebuild) and T325 (AdoptedOn); Q6–Q11; the new faults above. Leave T242 (a decision only), T357 (flow 05's, to take
    Q3's words), T381 (a changed item EPA after an end), T171, T139 and T291 items 2, 3, 6 and 8 (flows 16, 17 and 18)
    as their own.

## 7. Acceptance

A flow is done when BRIEF.md § 9's four checks hold. For this flow:

- **Replay the whole runbook** on a fresh database (the flow's steps are 1.19–1.22, 6.9, 6.22, 6.25, 6.26 and
  6.32–6.39; § 9): every Expect must hold, the refusals' exact words included. **Rewrite the steps that change:** 1.19
  (no Create EPA she cannot complete; the adopt link), 1.20 (Q7: the adopted version no longer offered; Q1's page),
  1.21 and 6.22 (no Edit on national EPAs; View or "Set by the College"), 1.22 and 6.9 (the subtitle), 6.25 (the
  owner mark; KGK-001's page title), 6.26 and 6.33 (Q1's page; Q14's form), 6.32 (Q6's dialog; Q10's row; T301),
  6.36 (Q8's dialog); and T301's 6.13, 6.29 and 6.31 (flow 16's steps, whose Expects T301 changes); search the whole
  runbook (BRIEF.md § 9 item 7) for the words that change ("Curriculum adoptions" in 2.x's invitation and admission
  steps among them, if Q1 merges the page). Record passwords only in `pwd_DO_NOT_COMMIT.txt`.
- **Checks the design adds:** 1.19, nothing offered she cannot complete; 6.25, KGK-001 marked and the only row she may
  change; the Demo EPA-001 typed by id reads not found (Q2); a held-back version is not offered and its adoption by
  request is refused (T301); 6.32, the dialog's count of registrars on 11.1 (Dr Dlamini, Dr Mahlangu, Dr Ndlovu: 3);
  6.36, the move's dialog; 390 and 768 with nothing scrolling sideways, the item's edit included; no exception's text
  on any failed read or save.
- **The tests pass** (`dotnet test tests/Wombat.Web.Tests/Wombat.Web.Tests.csproj`, never with `--no-build`), with all
  six suites green. They must include the curriculum pages' tests in `tests/Wombat.Web.Tests/Admin/`
  (`CurriculumItemsScopeTests`, `CurriculumItemsEpaPickerTests`, `CurriculumItemsRemoveTests`,
  `CurriculaListScopeTests`, `CurriculumItemsEditLayoutTests`) with new ones for the owner mark, the adoption dialog,
  the move dialog and not found; the handler tests T291, T301 and Q2/Q9/Q12 bring; `Navigation/ActiveNavItemTests` and
  `NavMenuAuthorizationTests` (if Q1 changes a menu); `Hosting/PageTitleTests`; `Accessibility/RowNamesTests`,
  `ActionFocusTests`, `AccessibleNamesTests`; `Design/*`, including `NarrowLayoutTests`, `DefinedClassTests`,
  `TableColumnClassTests`, `ContrastTests`; the scenario guard in `Scenario/`.
- **Re-capture** the states in § 3 (the old-frame ones, and the new ones on scratch copies, as `states.md` says) and
  the steps' own captures, keeping their names; `check_baseline_paths.py` at missing 0. Compare them with the chosen
  artboards.
- **Browser check** at 1280, 768 and 390 px as Prof Mbatha, `devadmin` and Dr Ndlovu.

## 8. The runbook steps, verbatim (sent with § 1, in the same message)

These are pasted from `execution/knowledge/scenario-paediatrics/` (`act-1-setup.md`, `act-6-catalogue.md`; Role,
Route, Do and Expect only), as flow 14's replay left them (T387, 2026-10-30; the Gaps it recorded are 1.19's, 1.21's,
6.22's and 6.25's T291, and 1.22's T326; 6.35 was not played in order). `D` is the replay day, and `J` the latest 15
January on or before it (README § How to play).

```text
Step 1.19 — Before the adoption, KGK reads no catalogue (act-1-setup.md)
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/curricula → /admin/epas
Do: Open Curricula, then EPAs, before adopting anything.
Expect: Curricula shows the empty state "No curricula yet": "Your institution has not adopted a curriculum yet. Adopt
  one on the Curriculum adoptions page before admitting trainees." It offers no Create curriculum (T211).
  EPAs shows "No EPAs yet". An InstitutionalAdmin reads the national EPAs of the disciplines her institution has
  adopted, plus its own EPAs, and there are none of either.

Step 1.20 — Adopt curriculum 11.1 for KGK (act-1-setup.md)
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/adoptions
Do: Open Curriculum adoptions. Under Adopt a curriculum, choose `College of Paediatricians of South Africa —
  Paediatrics / Paediatrics: Paediatric EPA Curriculum (11.1)` and press Adopt. Then choose the same curriculum again
  and press Adopt a second time.
Expect: The page has no institution picker, because hers is KGK. Current adoptions first reads "No curriculum adoptions
  yet for this institution."
  - The picker also offers the Demo `IM Core Curriculum (2026.1)`, which she leaves alone.
  - Adopt stays disabled until a curriculum is chosen.
  - The first Adopt reads "Curriculum adopted." It adds a row: `College of Paediatricians of South Africa`,
    Paediatrics, Paediatrics, `Paediatric EPA Curriculum`, `11.1`, adopted on `D`, Active.
  - The second Adopt is refused: "This institution has already adopted this curriculum version."
  The page notes that re-adopting a newer version would supersede this adoption.

Step 1.21 — What the adoption opens to KGK (act-1-setup.md)
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/epas → /admin/curricula → /admin/curricula/{Id:int}/items → /
Do: Read each page again, then go Home. Change nothing.
Expect: Each page now shows the adopted catalogue:
  - EPAs lists the 15 PAED EPAs and nothing else. No row offers Edit, because a national EPA is the College's to
    change: `UpdateEpa` refuses her (T291 item 1).
  - Curricula has the one row, which offers Items but no Edit. The header offers no Create curriculum.
  - The items page warns that the College sets the curriculum's national items, so they are read only here, and that
    KGK's own items apply only to KGK's trainees. Its 15 rows match Step 1.18, and each reads "Set by the College".
  - The form below the items is headed "Add your institution's own item". It says every EPA KGK could add is already on
    the curriculum: to add an item of its own, KGK must first create a local EPA of Paediatrics.
  - Home's Curriculum card reads "1" over "speciality adopted" and "1" over "sub-speciality adopted", with "Paediatrics
    / Paediatrics, from the CPSA's Paediatric EPA Curriculum (11.1)." (T291 item 4, T376).

Step 1.22 — The ladder, read-only to KGK (act-1-setup.md)
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/entrustment-scales
Do: Open Entrustment scales and read the list.
Expect: The list shows `CPSA Paediatric Entrustment Scale v11.1` at 6 levels and `O-R Scale` at 5, each with its
  description. It offers no Create scale and has no actions column, because only an Administrator changes a scale
  (T057, T239).

Step 6.9 — Prof Mbatha reads the scales, and changes none (act-6-catalogue.md)
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/entrustment-scales
Do: Read the list.
Expect: Three scales, with their level counts: the CPSA ladder at 6, the O-R Scale at 5 and the CNSA scale at 6. There
  is no Create scale, and no column of Edit and Delete: a column no row offers is not rendered (T239).

Step 6.22 — Prof Mbatha sees the pause, and cannot lift it (act-6-catalogue.md)
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/epas → /admin/curricula → /admin/curricula/{Id:int}/items → /admin/entrustment-decisions
Do: Read the EPA list, then 11.1's items. Then open Entrustment decisions, filter by Dr Molefe's name and press Show.
  Revoke nothing.
Expect:
  - **The EPA list.** PAED-012 reads Inactive. No national EPA offers her Edit: only its College changes one (T211's
    rule; `UpdateEpa`).
  - **11.1's items.** PAED-012's row is marked "(inactive: not in force)", under the same notice. Every row reads "Set
    by the College".
  - **Dr Molefe's STARs.** Her fifteen from Act 5 still read Active, beside Act 4's PAED-010, Superseded (the status
    filter starts on All): "… · 16 shown". Each row is headed by her name, its EPA in the next column. The PAED-012 row
    names its EPA "PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams (no longer
    in use)" (T255): a pause leaves a STAR standing.

Step 6.25 — Prof Mbatha creates a KGK EPA (act-6-catalogue.md)
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/epas → /admin/epas/new → /admin/epas/{Id:int} → /admin/epas
Do: Create an EPA:
  - sub-speciality: Paediatrics / Paediatrics;
  - code: `KGK-001`;
  - title: "Running a paediatric outreach clinic at a district hospital";
  - description: "Plans and runs a monthly outreach clinic at a North West district hospital, with its referral
    follow-up.";
  - category: Elective.
  Save.
Expect:
  - The sub-speciality list offers only Paediatrics / Paediatrics, the discipline KGK has adopted.
  - Once saved, the page is KGK-001's, Active. KGK's own EPA is hers to change (`UpdateEpa`).
  - The EPA list shows KGK-001 beside the 15 national EPAs, and offers Edit on KGK-001 alone.

Step 6.26 — Prof Mbatha adds KGK's own item to 11.1 (act-6-catalogue.md)
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/curricula → /admin/curricula/{Id:int}/items
Do: Open 11.1's items, and add KGK's own item:
  - EPA: KGK-001;
  - target: 1 per academic year;
  - no decision cadence, decided by the trainee's general panel;
  - the suggested scale, minimum level 3a, and no year-by-year minima;
  - completion window: 12;
  - tools: Direct observation only.
Expect:
  - **The curricula list.** It offers her Items, and no Edit and no Create curriculum.
  - **The Add form.** It is headed "Add your institution's own item". Its EPA picker offers KGK-001 alone, because the
    College's items hold all 15 national EPAs (T222). The scale is the CPSA ladder, suggested because every item on
    the curriculum is pinned to it. Add item stays off until a minimum rung is chosen.
  - **After the add.** The page confirms it, and the form gives way to a note that every EPA KGK could add is already
    on the curriculum.
  - **The new row.** It reads "Your institution's own item": 1 per academic year, No decision cadence, 3a, Direct
    observation, with Edit and Remove. The national rows still read "Set by the College".

Step 6.32 — Prof Mbatha adopts 11.2 (act-6-catalogue.md)
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/adoptions
Do: Adopt "College of Paediatricians of South Africa — Paediatrics / Paediatrics: Paediatric EPA Curriculum (11.2)".
Expect:
  - The picker offers the two active Paediatric versions and the Demo IM Core curriculum. It does not offer the
    inactive Neonatology curriculum.
  - The adoption is confirmed. Current adoptions reads 11.2 Active and 11.1 Superseded, as the page's note says a newer
    version supersedes the current adoption.
  - The registrars already on 11.1 stay on it.

Step 6.33 — Prof Mbatha adds KGK's own item to 11.2 (act-6-catalogue.md)
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/curricula → /admin/curricula/{Id:int}/items
Do: Read the curricula list. Open 11.2's items, and add KGK's own item on KGK-001 with the values of Step 6.26.
Expect:
  - **The list.** It shows 11.2, and still 11.1, because 11.1 holds KGK's own item (T211, T223). Both offer Items only.
  - **11.2's items.** It holds the College's 16 rows, PAED-016 and PAED-011's new target among them, all "Set by the
    College". It has no KGK item, because a new version carries no institution's own items.
  - **The add.** The picker offers KGK-001 alone. Once added, the row reads "Your institution's own item".

Step 6.34 — Prof Mbatha saves Dr Dlamini's profile while she stays on 11.1 (act-6-catalogue.md)
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/trainees → /admin/trainees/edit
Do: Open Dr Dlamini's profile from Current trainees (Edit), which is on Paediatric EPA Curriculum (11.1). Move the expected completion date six
  months later than it reads, leave the curriculum as it is, and save the profile. Then reopen the profile.
Expect:
  - The profile is "Anele Dlamini's programme", with "Export Anele Dlamini's portfolio" in its header, End this
    programme below Profile details, and no Back to trainees (T379).
  - Trainees' "Completed and closed", "2 ended", lists the latest end first: Lerato Molefe, Completed (Act 5's `D`),
    then Pieter du Plessis, Withdrawn (Act 5's `D−2`), each with View.
  - The curriculum picker offers 11.1 and 11.2, with 11.1 chosen.
  - The save reads "Trainee profile saved."
  - Reopened, the profile shows the new date on 11.1. By SQL, profile 3 stays on curriculum 2 and adoption 1: she stays
    on the version she was admitted to (`CurriculumAdminScope`, T211).

Step 6.35 — Dr Ndlovu notes his figures before he moves (act-6-catalogue.md)
Role: Trainee — Dr Sipho Ndlovu
Route: /portfolio/progress
Do: Read the page, and note each row's count for its current period.
Expect: Sixteen rows in Your EPAs, "Each semester · 10 EPAs" and "Once a year · 6 EPAs": the 15 national EPAs of 11.1,
  and KGK-001 among the six. PAED-016 is not among them.

Step 6.36 — Prof Mbatha moves Dr Ndlovu to 11.2 (act-6-catalogue.md)
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/trainees → /admin/trainees/edit
Do: Open Dr Ndlovu's profile from Current trainees (Edit). Choose Paediatric EPA Curriculum (11.2), keep his programme start (`J`, from Act 2) and
  his expected completion as they read, and save the profile.
Expect: The profile is "Sipho Ndlovu's programme", with "Export Sipho Ndlovu's portfolio" in its header, End this
  programme below Profile details, and no Back to trainees (T379). The save reads "Trainee profile saved. N
  completion(s) was/were checked against 11.2, and M count(s) towards it." The profile reads 11.2. By SQL his PAED-002
  tally is on 11.2 and none is left on 11.1. Reopened, the picker offers 11.2 alone. From now on, his completions
  credit 11.2's items.

Step 6.37 — Dr Ndlovu on 11.2, straight after the move (act-6-catalogue.md)
Role: Trainee — Dr Sipho Ndlovu
Route: /portfolio/progress → /activities/new → /activities/new?type=mini_cex_cpsa
Do: Read the page. Then start a Mini-CEX (Paediatrics) and open its EPA picker. Leave without saving.
Expect:
  - **The rows.** Seventeen, in "Each semester · 10 EPAs" and "Once a year · 7 EPAs": 11.2's 16 national items (PAED-016
    once a year, and PAED-011 at 2 per academic year, among them) and KGK-001. Nothing notes the move and nothing marks
    a row as new: KGK-001's badge is the only mark (T355, Q9).
  - **The counts.** His evidence counts on 11.2 at once: PAED-002 reads "1 of 3 this semester", as in Step 6.35, and
    PAED-016 and KGK-001 read "0 of 1 in 2026".
  - **The picker.** The Mini-CEX picker offers ten EPAs, PAED-016 among them.

Step 6.38 — devadmin rebuilds curriculum progress (act-6-catalogue.md)
Role: Administrator — devadmin
Route: /admin/curricula → /admin/curriculum-progress
Do: From Curricula in the menu, open Curriculum progress from the header and read it. Press Rebuild progress, and
  confirm.
Expect:
  - **The way in.** Curricula offers the Administrator an outline "Curriculum progress" in its header, where Home's
    Maintenance card used to link it (T335, flow 01). On the page Curricula stays lit, and the trail reads Home ›
    Curricula › Curriculum progress.
  - **The page.** It says that a rebuild credits every completed activity again against today's curriculum, judges only
    whether an EPA was active as of each completion, and runs as one transaction.
  - **The question.** The button asks first: "Rebuild curriculum progress?".
  - **The result.** The page says progress was rebuilt, with five figures: activities re-read, curriculum items
    credited, semester tallies written, stale tallies removed, and completions re-stamped. Stale tallies removed is 0:
    the move left nothing stale (T304).

Step 6.39 — Dr Ndlovu's evidence counts on 11.2 (act-6-catalogue.md)
Role: Trainee — Dr Sipho Ndlovu
Route: /portfolio/progress
Do: Read the page.
Expect: Each of the 15 EPAs carried over from 11.1 reads the count he noted in Step 6.35 for its current period.
  PAED-016 and KGK-001 read "0 of 1 in 2026". PAED-011's row reads "0 of 2 in 2026": it measures its count against a
  target of 2 per academic year. The rows are "Each semester · 10 EPAs" and "Once a year · 7 EPAs", and This period's
  yearly figure is "… of 7" over "EPAs met in 2026".
```
