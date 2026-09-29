# Act 6 — After graduation: the catalogue is maintained

**Scenario date.** In the story, early 2027, in the weeks after Dr Molefe's graduation (Act 5). The story's dates are
narrative only: every date a step types is relative to the replay day `D` (README, "The clock").

**Who acts.**
- devadmin (Administrator) keeps the College records and the entrustment scales, and rebuilds progress.
- Dr Anton Kruger (CollegeAdmin, CPSA) keeps the national catalogue.
- Prof Nolwazi Mbatha (InstitutionalAdmin, KGK) keeps KGK's adoption and KGK's own additions.
- Dr Dlamini, Dr Mahlangu and Dr Ndlovu (Trainees), Dr Molefe (a former trainee), Dr du Plessis (withdrawn in Act 5,
  still holding Trainee) and Dr Patel (Assessor) act only to show what each catalogue change does to the people it
  measures.

**Why.** A national catalogue is not written once. The College:
- corrects its wording;
- takes an EPA out of use while it revises it, then restores it;
- grows a new sub-speciality;
- publishes a new curriculum version.

An institution then adopts the new version when it chooses, and keeps its own additions. The operator:
- adds a second College;
- keeps the scales the ladders are built on;
- can rebuild every trainee's progress from their evidence.

**Starting state.** What Acts 1–5 left (snapshot `recovery/scenario-post-act5.dump`):
- **Colleges:** CPSA, as seeded, and the Demo College.
- **Scales:** the six-rung CPSA Paediatric Entrustment Scale v11.1, and the Demo O-R Scale.
- **The CPSA catalogue, as seeded:**
  - the Paediatrics speciality, with one sub-speciality, Paediatrics, whose default scale is the CPSA ladder;
  - PAED-001 to PAED-015, all active;
  - Paediatric EPA Curriculum 11.1, with its 15 national items.
- **KGK's adoption:** one, of 11.1, active. KGK has no EPA and no curriculum item of its own.
- **The registrars:**
  - Dr Dlamini, Dr Mahlangu and Dr Ndlovu are the current trainees, all on 11.1, each holding the credited evidence
    Acts 3 and 4 recorded.
  - Dr Molefe's programme is complete: she holds no Trainee role, reads her record read-only, and holds 15 active
    STARs, one per EPA (Act 5).
  - Dr du Plessis has withdrawn: his profile on 11.1 is deactivated with the last day Act 5 recorded, he keeps the
    Trainee role, and his Mini-CEX observed after that day credits nothing (Act 5, Steps 5.24 to 5.28).
- **Dr Patel** is an active Assessor at KGK.

**Goal.** Every catalogue page is played by the role that owns it, and each change is followed to the pages it changes:
- a College, a speciality, two sub-specialities and a scale are created;
- CPSA's record, a speciality and an EPA's wording are corrected;
- the College drafts an activity type of its own in the builder (T300, D52), and leaves it unpublished;
- an EPA is paused and restored, and the credit earned in the pause is counted (D48);
- KGK adds an EPA and a curriculum item of its own;
- the College drafts, edits and publishes curriculum 11.2 with a new EPA, and KGK adopts it;
- one registrar moves to 11.2, and progress is rebuilt;
- the graduate's and the withdrawn registrar's records, and the graduate's STARs, come through the pause and the
  rebuild as Act 5 left them (Act 5's handoff).

It also shows where each role is refused.

## Phase 6.A — The College records

Only an Administrator creates a College or changes a College's own record. The College's CollegeAdmin keeps what the
College owns: its specialities, sub-specialities, EPAs and curricula.

### Step 6.1 — devadmin creates a second national College
Role: Administrator — devadmin
Route: /admin/colleges → /admin/colleges/new → /admin/colleges/{Id:int} → /admin/colleges
Do: Create the College of Neurologists of South Africa, with the description "Constituent College of the CMSA; owns
  the national Neurology catalogue." Save it first with the short code `CPSA` by mistake, then correct the code to
  `CNSA` and save again.
Expect: The first save is refused: "A college with the same name or short code already exists." Nothing is created.
  The second save opens the new College's edit page, Active. The colleges list reads three rows, each Active and each
  offering Edit and Specialities: the new College, the College of Paediatricians of South Africa and the Demo College.
Note: Administrator only. The College pages and `CreateCollege` admit no one else.
Actual (2026-09-28, T339 replay, wombat_scenario_t339): Colleges listed CPSA and the Demo College. Create college
  (trail Home › Colleges › Create college): an empty save shows "The Name field is required." and "The ShortCode field
  is required."; with short code CPSA the save was refused with "A college with the same name or short code already
  exists." and stayed on /admin/colleges/new, nothing created. With CNSA it opened /admin/colleges/4, "Edit college",
  Active ticked. The list reads three rows, all Active, each with Edit and Specialities ("Edit College of …",
  "Specialities of …"): CNSA, CPSA, Demo College.
Gap: F-6.1a, T324 (still: the required-field message names the model property, "The ShortCode field is required.", not
  the label "Short code"; the Expect itself is met).

### Step 6.2 — devadmin corrects CPSA's description, and nothing else
Role: Administrator — devadmin
Route: /admin/colleges → /admin/colleges/{Id:int} → /admin/colleges
Do: Open CPSA's record and note its id from the address, for Step 6.10. Change the description to "Constituent
  College of the Colleges of Medicine of South Africa; owns the national Paediatric EPA catalogue and its versions."
  Leave the name, the short code and Active as they are, and save.
Expect: The save is confirmed, and the list shows the new description, with CPSA still Active. The name and short code
  are unchanged, so every CPSA page still reads "College: College of Paediatricians of South Africa (CPSA)". A restart
  neither re-seeds nor restores the record: the seeder finds its College by seed key (T221).
Note: Administrator only (`UpdateCollege`: a CollegeAdmin keeps the catalogue, not the College's own record). The red
  Deactivate beside Save does not ask first yet, against DESIGN.md's rule (T264). Do not press it.
Actual (2026-09-28, T339 replay, wombat_scenario_t339): CPSA is /admin/colleges/2 (id 2, for Step 6.10): name, short
  code CPSA and Active as seeded. With only the description changed, Save showed "College saved."; the list's CPSA row
  reads the new description, Active. By SQL the row keeps its name, CPSA, IsActive true and SeedKey cpsa. Deactivate
  was not pressed. The restart claim was not played (restarting the shared replay app would cut the later acts' log);
  the SeedKey is on the row.
Gap: none

### Step 6.3 — devadmin adds the Neurology speciality
Role: Administrator — devadmin
Route: /admin/colleges → /admin/colleges/{CollegeId:int}/specialities → /admin/colleges/{CollegeId:int}/specialities/new → /admin/colleges/{CollegeId:int}/specialities/{Id:int} → /admin/colleges/{CollegeId:int}/specialities
Do: From the new College's row, open its specialities, and create `Neurology` with the description "Specialist training
  in Neurology."
Expect: Before the create, the list says the College has no specialities yet and invites the first. After it:
  - the edit page reads "College: College of Neurologists of South Africa (CNSA)", Active, and offers "Manage
    sub-specialities";
  - the list has one row, Neurology, Active.
Note: An Administrator, or the College's own CollegeAdmin. CNSA has none.
Actual (2026-09-28, T339 replay, wombat_scenario_t339): Specialities on CNSA's row opened
  /admin/colleges/4/specialities, "College: College of Neurologists of South Africa (CNSA)", with "No specialities yet
  / Create the first speciality for this college." Creating Neurology opened /admin/colleges/4/specialities/3, "Edit
  speciality", "College: College of Neurologists of South Africa (CNSA)", Active ticked, with "Manage
  sub-specialities". The list has one row: Neurology, "Specialist training in Neurology.", Active.
Gap: none

### Step 6.4 — devadmin adds the Adult Neurology sub-speciality
Role: Administrator — devadmin
Route: /admin/colleges/{CollegeId:int}/specialities/{Id:int} → /admin/specialities/{SpecialityId:int}/sub-specialities → /admin/specialities/{SpecialityId:int}/sub-specialities/new → /admin/specialities/{SpecialityId:int}/sub-specialities/{Id:int} → /admin/specialities/{SpecialityId:int}/sub-specialities
Do: From Neurology's page, manage its sub-specialities and create `Adult Neurology`, with the description "Adult
  neurology specialist training programme."
Expect: The list is empty before the create and has one row after it. The new sub-speciality's edit page offers a
  default entrustment scale, which reads "No default — offer every scale". Its choices are the two existing scales.
Note: An Administrator, or the College's own CollegeAdmin. Step 6.7 sets the default.
Actual (2026-09-28, T339 replay, wombat_scenario_t339): Manage sub-specialities opened
  /admin/specialities/3/sub-specialities, "Speciality: Neurology", "No sub-specialities yet / Create the first
  sub-speciality under this speciality." Creating Adult Neurology opened /admin/specialities/3/sub-specialities/3,
  "Edit sub-speciality": "Default entrustment scale" reads "No default — offer every scale", its other choices the
  CPSA Paediatric Entrustment Scale v11.1 and the O-R Scale. The list has one row, Adult Neurology, Active.
Gap: none

## Phase 6.B — Entrustment scales

Scales are global: any curriculum item, sub-speciality or form may use any scale. Only an Administrator creates, edits or
deletes one (T057). An InstitutionalAdmin reads them. A CollegeAdmin chooses a sub-speciality's default scale from them.

### Step 6.5 — devadmin creates the Neurology ladder
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
Note: Administrator only. The create and edit pages, and every scale command, admit no one else.
Actual (2026-09-28, T339 replay, wombat_scenario_t339): Create entrustment scale opens with two level rows; three Add
  level presses gave five. With level 4 also "Indirect supervision" the save was refused: "Level labels must be unique
  within a scale." and stayed on /new. With level 4 "Distant supervision" it opened /admin/entrustment-scales/3,
  levels 1 Observe only to 5 Unsupervised practice in order. The list shows three scales: CNSA Neurology Entrustment
  Scale 5, CPSA ladder 6, O-R Scale 5 (by SQL three scales, so the refused save created none).
Gap: none

### Step 6.6 — devadmin adds a sixth level
Role: Administrator — devadmin
Route: /admin/entrustment-scales → /admin/entrustment-scales/{Id:int} → /admin/entrustment-scales
Do: Open the CNSA scale, add a sixth level, `Supervises others` ("Supervises juniors performing the activity."), and
  save.
Expect: The save is confirmed, and the page lists six levels, 1 to 6, with `Supervises others` at 6. The list shows 6
  levels.
Note: Administrator only.
Actual (2026-09-28, T339 replay, wombat_scenario_t339): Add level gave a sixth row; with "Supervises others" and its
  description, Save showed "Entrustment scale saved." and the page lists 1 to 6, Supervises others at 6 (DB Orders 1-6
  in that order). The list reads 6 levels.
Gap: none

### Step 6.7 — devadmin makes it Adult Neurology's default
Role: Administrator — devadmin
Route: /admin/specialities/{SpecialityId:int}/sub-specialities/{Id:int}
Do: Open Adult Neurology, set its default entrustment scale to the CNSA Neurology Entrustment Scale, and save.
Expect: The save is confirmed, and reopening the page shows the scale selected. Its help says that committee STARs for
  this programme's trainees are limited to that scale's levels.
Note: An Administrator, or the College's CollegeAdmin.
Actual (2026-09-28, T339 replay, wombat_scenario_t339): On /admin/specialities/3/sub-specialities/3 the default offers
  No default, the CNSA scale, the CPSA ladder and the O-R Scale. Choosing the CNSA scale and saving showed
  "Sub-speciality saved." (DefaultEntrustmentScaleId 3). Reopened, the CNSA scale is selected, with "Committee STARs
  for this programme's trainees are limited to this scale's levels."
Gap: none

### Step 6.8 — devadmin cannot delete a scale in use
Role: Administrator — devadmin
Route: /admin/entrustment-scales
Do: Delete the CNSA Neurology Entrustment Scale from the list.
Expect: Delete asks first, naming the scale (DESIGN.md § Button system: a destructive action opens a ConfirmDialog).
  Once confirmed, it is refused: "This entrustment scale is the default scale of the sub-speciality "Adult Neurology"
  (Neurology), so it cannot be deleted. Change that sub-speciality's default entrustment scale to another scale, or to
  no default, first." The scale stays, with 6 levels.
Note: Administrator only. The refusal names the first reference it finds, in this order: an MSF question, a pinned
  curriculum item, scored progress, a sub-speciality's default, a published form's binding, an entrustment decision.
  Until T264 lands, the list's Delete does not ask first.
Actual (2026-09-28, T339 replay, wombat_scenario_t339): The CNSA row's Delete ("Delete CNSA Neurology Entrustment
  Scale") sent the command at once: no dialog. It was refused: "This entrustment scale is the default scale of the
  sub-speciality "Adult Neurology" (Neurology), so it cannot be deleted. Change that sub-speciality's default
  entrustment scale to another scale, or to no default, first." The scale stays, 6 levels in the list and in the DB.
Gap: F-6.8a, T264 (still: Delete does not ask first, against DESIGN.md § Button system). The refusal text matches the
  Expect.

### Step 6.9 — Prof Mbatha reads the scales, and changes none
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/entrustment-scales
Do: Read the list.
Expect: Three scales, with their level counts: the CPSA ladder at 6, the O-R Scale at 5 and the CNSA scale at 6. There
  is no Create scale, and no column of Edit and Delete: a column no row offers is not rendered (T239).
Note: An InstitutionalAdmin reads the scales; only an Administrator changes one.
Actual (2026-09-28, T339 replay, wombat_scenario_t339): Prof Mbatha (Entrustment scales lit) reads three scales under
  Name, Description and Levels only: CNSA 6, CPSA ladder 6, O-R Scale 5. No Create scale, no link or button in the
  page, no Actions column. At 390 px the page does not scroll sideways (scrollWidth 390).
Gap: none

## Phase 6.C — The College keeps its catalogue

### Step 6.10 — Dr Kruger is refused what only an Administrator may do
Role: CollegeAdmin — Dr Anton Kruger
Route: /account/login → /account/login/submit → / → /admin/colleges/{Id:int} → /access-denied → /admin/entrustment-scales/new → /access-denied
Do: Sign in. From the dashboard, type the address of CPSA's own College record (the id from Step 6.2), then the
  address for creating an entrustment scale.
Expect: The dashboard has one card, National catalogue, with Specialities, EPAs and Curricula. His menu offers neither
  of the pages he types. Each address lands on "You cannot open this page" ("Your role (College admin) does not open
  this page."), with Go to Home, and nothing changes.
Note: A CollegeAdmin keeps the College's specialities, EPAs and curricula. The College's own record and the scales
  belong to an Administrator (`UpdateCollege`, `CreateEntrustmentScale`).
Actual (2026-09-28, T339 replay, wombat_scenario_t339): The sign-in page ("Sign in · Wombat": Email, Password with
  Show, Remember me, Sign in, "Forgotten your password?") took him to /: "Home", "College admin · Semester 2, 2026",
  one card, National catalogue, linking Specialities, EPAs and Curricula. His menu reads Home, Specialities, EPAs,
  Curricula, Activity types, My data rights. /admin/colleges/2 and /admin/entrustment-scales/new each landed on
  /access-denied?ReturnUrl=…, "You cannot open this page", "Your role (College admin) does not open this page.", "If
  you need it for your work, ask the platform administrator.", Go to Home; nothing lit. Nothing changed (3 scales;
  CPSA's description as Step 6.2 left it).
Gap: none

### Step 6.11 — Dr Kruger edits the Paediatrics speciality
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
Note: The College's CollegeAdmin, or an Administrator. The seeder finds the speciality by seed key, so a restart keeps
  the edit (T221). Until T291 item 2 lands, the list still shows him "Back to colleges".
Actual (2026-09-28, T339 replay, wombat_scenario_t339): Specialities in his menu opened /admin/colleges/2/specialities
  (trail Home › Specialities › College of Paediatricians of South Africa): "College: College of Paediatricians of
  South Africa (CPSA)", one row, Paediatrics. Edit opened /admin/colleges/2/specialities/2; the new description saved
  with "Speciality saved." and the list shows it, Active. The edit page offers only Back to specialities, Manage
  sub-specialities and Cancel, but the list still offers him "Back to colleges" (/admin/colleges), which Step 6.10
  showed refuses him.
Gap: F-6.11a, T291 (still: the specialities list offers a CollegeAdmin "Back to colleges", a page he is refused; the
  Note anticipates it).

### Step 6.12 — Dr Kruger creates the Neonatology sub-speciality
Role: CollegeAdmin — Dr Anton Kruger
Route: /admin/colleges/{CollegeId:int}/specialities/{Id:int} → /admin/specialities/{SpecialityId:int}/sub-specialities → /admin/specialities/{SpecialityId:int}/sub-specialities/new → /admin/specialities/{SpecialityId:int}/sub-specialities/{Id:int} → /admin/specialities/{SpecialityId:int}/sub-specialities
Do: From Paediatrics, manage its sub-specialities and create `Neonatology`, with the description "Certificate training
  in Neonatology." On its edit page, set the default entrustment scale to the CPSA Paediatric Entrustment Scale v11.1,
  and save.
Expect: The list holds Neonatology and Paediatrics, both Active. The default is saved and stays selected when the page
  is reopened. The Paediatrics sub-speciality keeps its own default, the CPSA ladder. Nothing about KGK's programme
  changes.
Note: The College's CollegeAdmin, or an Administrator.
Actual (2026-09-28, T339 replay, wombat_scenario_t339): Manage sub-specialities opened
  /admin/specialities/2/sub-specialities, "Speciality: Paediatrics", one row. Creating Neonatology opened
  /admin/specialities/2/sub-specialities/4 with No default; choosing the CPSA ladder and saving showed "Sub-speciality
  saved.", and it stays selected when reopened. The list holds Neonatology and Paediatrics, both Active. By SQL both
  default to scale 2; KGK's one adoption and every KGK profile (curriculum 2, adoption 1) are unchanged.
Gap: none

### Step 6.13 — Dr Kruger starts a Neonatology curriculum, and holds it back
Role: CollegeAdmin — Dr Anton Kruger
Route: /admin/curricula → /admin/curricula/new → /admin/curricula/{Id:int} → /admin/curricula → /admin/curricula/{Id:int}/items
Do: Create `Neonatology EPA Curriculum`, version `1.0`, in Paediatrics / Neonatology, effective from `D`, with no end.
  The form selects Neonatology first. Untick Active and save, so that no institution can adopt it yet. Then open its
  items.
Expect:
  - The create opens the curriculum's edit page, with Active ticked and a card for cloning it.
  - After Active is unticked, the save is confirmed. The curricula list shows the curriculum Inactive, with 0 items.
  - Its items page has no rows. In place of the Add form, it says there is nothing to add, and that a national EPA of
    Neonatology must be added first.
Note: The College's CollegeAdmin, or an Administrator. Active decides only whether an institution may adopt the
  curriculum (`GetAdoptableCurricula`).
Actual (2026-09-28, T339 replay, wombat_scenario_t339): Curricula listed 11.1 alone. The create form preselects
  "Paediatrics / Neonatology", effective from 2026-09-28, and has no Active box. The create opened /admin/curricula/3,
  "Edit curriculum", Active ticked, with the "Clone as new version" card (1.0-copy). Unticked and saved: "Curriculum
  saved."; the list reads Neonatology EPA Curriculum 1.0, Paediatrics / Neonatology, 0 items, Inactive. Its items page
  has an Existing items table with headers and no rows, and in place of the Add form: "Every national EPA of
  Neonatology is already on this curriculum, as a national item or as an institution's own item, so there is none left
  to add. To add an item, first add a national EPA of Neonatology, …"
Gap: F-6.13a, T326 (still: the empty state says every national EPA of Neonatology is already on the curriculum, though
  Neonatology has none; the empty table shows headers with no "no items yet" line).

### Step 6.14 — Dr Kruger corrects an EPA's wording
Role: CollegeAdmin — Dr Anton Kruger
Route: /admin/epas → /admin/epas/{Id:int} → /admin/epas
Do: Open PAED-006. Correct its title from "Managing long-term health conditions (LTHCs)" to "Managing long-term health
  conditions (LTHCs) in children". Change nothing else, and save.
Expect: The save is confirmed, and the EPA stays Active. The EPA list shows the new title. The title is read live
  wherever the EPA is named: 11.1's items, the activity pickers and every registrar's progress card (Step 6.15). An
  EPA's wording has no versions.
Note: A national EPA's College's CollegeAdmin, or an Administrator (`UpdateEpa`). A restart does not bring back the
  seeded title (T221).
Actual (2026-09-28, T339 replay, wombat_scenario_t339): PAED-006 is /admin/epas/7. With the title changed and nothing
  else, Save showed "EPA saved.", Active stays ticked and Deactivate is offered; the row keeps its description,
  IsActive and SeedKey cpsa:paediatrics:epa:PAED-006. The EPA list reads the new title, Active, and 11.1's items name
  it "PAED-006 - Managing long-term health conditions (LTHCs) in children". Pickers and progress are checked in Steps
  6.15-6.16. The restart claim was not played.
Gap: none

### Step 6.14a — Dr Kruger drafts a College activity type
Role: CollegeAdmin — Dr Anton Kruger
Route: /admin/activity-types → /admin/activity-types/{ActivityTypeId:int} → /admin/activity-types → /admin/activity-types/new → /admin/activity-types/{ActivityTypeId:int} → /admin/activity-types
Do: Open Activity types from his menu and read the list. Open `Multi-Source Feedback (Paediatrics)` and change nothing.
  Back in the list, press New activity type, open the Metadata tab and read what Scope offers, under Sub-speciality
  too. Then enter:
  - Key `cpsa_case_presentation_log` and Name `CPSA Case Presentation Log`;
  - Scope Speciality, with `Paediatrics`;
  - This tool is `Not a WBA instrument`;
  - Description `A registrar's log of a case they presented at a departmental meeting. Not rated, and credits
    nothing.`;
  - Active on.
  Press Save draft, publish nothing, and go back to the list.
Expect:
  - **The list.** Activity types is in his menu, and lit (T300, D52). The list has 22 rows: the twelve `*_cpsa`
    instruments and the ten Demo types. The College writes its disciplines' types, so each `*_cpsa` row offers Edit,
    named for its row ("Edit Mini-CEX (Paediatrics)"). The exceptions are `msf_cpsa` and `learner_feedback_cpsa`: the
    MSF and learner-feedback releases write their data, so they offer View (T334). The Demo types belong to the Demo
    College, so they offer View. KGK Teaching Session Log is not listed, because it is an institution's own type. New
    activity type is offered.
  - **A system-managed type.** Multi-Source Feedback (Paediatrics) opens read-only, with a notice that the system writes
    it, and no Save draft, Discard draft, Publish or editor controls (T334).
  - **The new type.** It opens headed "New activity type", on Form, with the default draft. Scope offers Speciality and
    Sub-speciality only, never Global or Institution. It starts on Speciality with `Paediatrics`, the College's one
    speciality; CNSA's Neurology is not offered. Sub-speciality offers `Paediatrics / Neonatology` (Step 6.12) and
    `Paediatrics / Paediatrics`.
  - **The save.** Save draft is accepted. The page moves to the type's own address, headed "Edit CPSA Case
    Presentation Log", and reads "Draft saved." (T291 item 3). In the list the type reads `Speciality · Paediatrics`,
    with no published version, Draft "Draft saved", Active and Edit.
  - **Who sees the draft.** It is unpublished, so no registrar is offered it. Prof Mbatha's list leaves it out, and its
    address reads "could not be found" to her: a type she may neither write nor open is not listed (T300, T211).
Note: A College's CollegeAdmin, or an Administrator (`ActivityTypeScopeGuard`, D52). Save no draft on a seeded `*_cpsa`
  type. A seeded type with an operator's draft stops receiving seed updates until the draft is published or discarded.
  Once anyone but the seeder publishes one, the seed refresher leaves it for good (T103, D52).
Actual (2026-09-28, T339 replay, wombat_scenario_t339): Activity types is in his menu, lit. The list has 22 rows: the
  ten Demo rows ("Speciality · General Medicine") offer View; the twelve *_cpsa rows ("Speciality · Paediatrics", v1,
  None, Active) offer Edit named for the row, msf_cpsa and learner_feedback_cpsa included. No KGK Teaching Session
  Log; New activity type offered. "Edit Multi-Source Feedback (Paediatrics)" opened /admin/activity-types/21 with no
  notice, Save draft, Publish (disabled), Add section and Edit/Up/Down/Delete; nothing pressed. New activity type
  opened "New activity type" on Form with the Details section. Metadata's Scope offers Speciality and Sub-speciality,
  starting on Speciality with Paediatrics alone; Sub-speciality offers "Paediatrics / Neonatology" and "Paediatrics /
  Paediatrics". With the Do's values Save draft was accepted: /admin/activity-types/24, "Edit CPSA Case Presentation
  Log", Save draft, Discard draft and Publish, and no "Draft saved.". By SQL it is Scope Speciality, ScopeId 2,
  Version 0, no tool key, the only draft. The list has 23 rows, the new one "Speciality · Paediatrics", v0, "Draft
  saved", Active, Edit. Prof Mbatha's list has 23 rows without it (KGK Teaching Session Log among them), and
  /admin/activity-types/24 reads "The activity type could not be found."
Gap: F-6.14aa, T291 (still: the first Save draft shows no "Draft saved."). F-6.14ab, T334 (still: msf_cpsa and
  learner_feedback_cpsa offer the College Edit, and msf_cpsa opens with no notice and every editor live; not saved).

## Phase 6.D — An EPA paused and restored

The College takes PAED-012 out of use while it revises it. By D48:
- **Deactivating pauses credit, and does not refuse filing.** Pickers and progress leave the EPA out, and the credit it
  has earned is kept.
- **A completion during the pause credits nothing.** Reactivating the EPA credits those completions.

### Step 6.15 — Dr Dlamini reads her progress before the pause
Role: Trainee — Dr Anele Dlamini
Route: /portfolio/progress
Do: Read the page. Note these, for Steps 6.19 and 6.24:
  - PAED-012's count this semester (`n`);
  - its count at the minimum level when observed;
  - the rung it names as the minimum now;
  - the "Semester targets" line's count of EPAs (`m`).
Expect: PAED-012 is among the "Each semester" cards, with a target of 3 per semester. PAED-006's card carries the title
  corrected in Step 6.14. The "This period" card reads "Semester targets: … of `m` EPAs met this semester".
Note: A trainee reads only their own progress.
Actual (2026-09-28, T339 replay, wombat_scenario_t339): PAED-012's "Each semester" card reads "0 of 3 this semester",
  "3 more by 30 November 2026.", "Target: 3 per semester (6 a year). Minimum now 4." and no "at the minimum level"
  line: n = 0, 0 at the minimum, minimum now 4. "Semester targets 1 of 10 EPAs met this semester", so m = 10.
  PAED-006's card and Annexure A row read the corrected title.
Gap: none

### Step 6.16 — Dr Dlamini files a Mini-CEX on PAED-012
Role: Trainee — Dr Anele Dlamini
Route: /activities/new → /activities/new?type=mini_cex_cpsa → /activities/{ActivityId:int}
Do: File a Mini-CEX (Paediatrics) and submit it:
  - EPA: PAED-012;
  - assessor: Dr Patel;
  - encounter date: `D`;
  - setting: Outpatient clinic;
  - presenting problem: "Counselling the parents of a toddler newly diagnosed with type 1 diabetes";
  - complexity: Moderate.
  Note the activity's id from its address.
Expect: The EPA picker offers nine EPAs for the Mini-CEX, PAED-012 among them. Submit to Mohammed Patel reads
  "Submitted. It is now Requested. It is in Mohammed Patel's Activity inbox."
Note: The trainee files it. The assessor must be an active Assessor at KGK (T102).
Actual (2026-09-28, T339 replay, wombat_scenario_t339): Log an activity, Mini-CEX (Paediatrics): the EPA picker offers
  nine, PAED-001 to 004, 006 (corrected title), 007, 008, 012 and 013. Filed on PAED-012 with Mohammed Patel,
  encounter 2026-09-28, Outpatient clinic, the problem as given and Moderate; Submit opened /activities/25:
  "Submitted. It is now Requested.", history Create and Submit by Anele Dlamini. Activity id 25. No mail was written
  (Stub email lines stayed at 30).
Gap: none

### Step 6.17 — Dr Kruger pauses PAED-012
Role: CollegeAdmin — Dr Anton Kruger
Route: /admin/epas → /admin/epas/{Id:int} → /admin/epas → /admin/curricula → /admin/curricula/{Id:int}/items
Do: Open PAED-012 and press Deactivate. Read the question and confirm. Then open 11.1's items.
Expect:
  - **The question.** Deactivate asks first, "Deactivate this EPA?". The dialog says that the EPA:
    - can no longer be chosen for a new activity;
    - stops being a target on every progress page and dashboard, for every institution that uses it;
    - keeps the progress it has earned;
    - credits nothing for an activity completed against it while it is inactive, until it is reactivated.
  - **After confirming.** The page says the EPA is deactivated. Active is unticked, and Deactivate is gone. The EPA list
    reads PAED-012 Inactive.
  - **11.1's items.** PAED-012's row is marked "(inactive: not in force)". A notice says its credit is paused and that
    reactivating it credits what was completed meanwhile.
Note: D48, T158, T196. The College's CollegeAdmin or an Administrator deactivates a national EPA; a local EPA is its
  institution's. Unticking Active and saving asks the same question.
Actual (2026-09-28, T339 replay, wombat_scenario_t339): PAED-012 is /admin/epas/13. Deactivate opened "Deactivate this
  EPA?": "It can no longer be chosen for a new activity, and it stops being a target on every progress page and
  dashboard, for every institution that uses it. The progress it has earned is kept. An activity completed against it
  while it is inactive counts towards nothing until it is reactivated. …", focus on Cancel. Confirmed: "EPA
  deactivated.", Active unticked, only Save and Cancel left. The EPA list reads PAED-012 Inactive. 11.1's items mark
  it "(inactive: not in force)" under "PAED-012 is inactive, so its item is not in force: … its credit is paused, …
  Reactivating an EPA on its own page brings its item back, and credits what was completed against it meanwhile."
Gap: none

### Step 6.18 — Dr Patel completes the Mini-CEX during the pause
Role: Assessor — Dr Mohammed Patel
Route: /activities/inbox → /activities/{ActivityId:int}
Do: Open Dr Dlamini's Mini-CEX from the inbox. Rate it at the rung that Dr Dlamini's PAED-012 card named as the minimum
  now (Step 6.15). Write the three feedback fields, and complete it.
Expect:
  - The inbox row and the request's EPA both read "PAED-012 — Communicating with and counselling patients, caregivers
    and healthcare teams (no longer in use)".
  - The completion is not refused (D48): "Completed.". The activity is read-only.
  - The status card reads "Done. You completed it on …" and "Rated <the rung>. Its credit to PAED-012 waits while the
    EPA is paused." About's Credit reads None, with "This activity's EPA is paused: its credit waits." under it. In
    its history, the completion's Credit reads None.
Note: Only the nominated assessor completes it (`field:assessor_user_id`). T231.
Actual (2026-09-28, T339 replay, wombat_scenario_t339): Dr Patel's inbox row and the activity's EPA read "PAED-012 —
  Communicating with and counselling patients, caregivers and healthcare teams (no longer in use)". Rated 4 (the list
  offers 1, 2, 3a, 3b, 4, 5) with the three feedback fields, Complete was not refused: State Completed, no field
  enabled, no action left. The page warns "This activity is complete, but it counted towards no curriculum
  requirement. … or was not in use at the time, …"; the history's Complete row has Credit None (CreditedItemCount 0),
  and Dlamini has no PAED-012 progress row.
Gap: none

### Step 6.19 — Dr Dlamini during the pause
Role: Trainee — Dr Anele Dlamini
Route: /portfolio/progress → /activities/mine → /activities/new → /activities/new?type=mini_cex_cpsa
Do: Read her progress and her activities. Then start a Mini-CEX (Paediatrics) and open its EPA picker. Leave without
  saving.
Expect:
  - **Progress.** No PAED-012 card, and the semester targets line counts `m − 1` EPAs. The rating trajectory for
    PAED-012 still charts every rating on it, the one from Step 6.18 included, under the heading "PAED-012 — … (no
    longer in use)" (T255).
  - **My activities.** The Mini-CEX carries "PAED-012 (no longer in use)" under its name, and its Credit reads None.
  - **The picker.** It offers eight EPAs, without PAED-012.
Note: T158: progress, pickers and dashboards list only EPAs in force. T231 and T255 mark what was recorded against an
  EPA that is not.
Actual (2026-09-28, T339 replay, wombat_scenario_t339): My progress has no PAED-012 card and reads "Semester targets 1
  of 9 EPAs met this semester" (m − 1); the trajectory "PAED-012 — … (no longer in use)" charts 1 observation,
  2026-09-28 at 4; the MSF line counts 14 EPAs. My activities lists the Mini-CEX with the same marker, Completed,
  Credited None. The Mini-CEX picker offers eight: PAED-001 to 004, 006, 007, 008 and 013. Left without saving.
Gap: none

### Step 6.20 — Dr Mahlangu's picker leaves PAED-012 out too
Role: Trainee — Dr Nomsa Mahlangu
Route: /activities/new → /activities/new?type=mini_cex_cpsa
Do: Start a Mini-CEX (Paediatrics) and open its EPA picker. Leave without saving.
Expect: Eight EPAs: PAED-001 to PAED-004, PAED-006 to PAED-008, and PAED-013. PAED-012 is not offered.
Note: The pause is national. It reaches every registrar on every curriculum that holds PAED-012.
Actual (2026-09-28, T339 replay, wombat_scenario_t339): Dr Mahlangu's Mini-CEX (Paediatrics) picker offers eight:
  PAED-001 to 004, 006 to 008, and 013; no PAED-012. Left without saving.
Gap: none

### Step 6.21 — Dr Molefe's record during the pause
Role: Former trainee (no role; a trainee record) — Dr Lerato Molefe
Route: /portfolio/progress
Do: Read her record.
Expect: The record opens with its notice that she completed her programme and that the page is read-only.
  - It lists no PAED-012 card while PAED-012 is paused, as every progress page leaves out an EPA not in force (T158).
  - Her "Entrustment against Annexure A" panel has no PAED-012 row either: the standing reads only items in force.
  - Any rating trajectory she has on PAED-012 still charts, headed "(no longer in use)" (T255).
  - PAED-006's card carries the title corrected in Step 6.14. Her other cards and periods are as Act 5 left them.
Note: The TraineeOrFormerTrainee policy admits her without the Trainee role (T252).
Actual (2026-09-28, T339 replay, wombat_scenario_t339): Signed in with no role (menu Home, My progress, My data
  rights), /portfolio/progress opens "You completed your programme on 28 September 2026. This page is your record of
  it and is read-only …". No PAED-012 card; "Entrustment against Annexure A" reads "14 at or above · 0 below · 0 with
  no decision, of 14 EPAs" and "14 of 14 EPAs at their exit level by STAR decision (level 5: 8 of 8 · level 4: 6 of
  6)", no PAED-012 row (Act 5 left 15 of 15). Trajectories PAED-001, PAED-010 and "PAED-012 — … (no longer in use)" (2
  observations). PAED-006 reads the corrected title; the other periods read as Act 5 left them (Semester 2, 2026 no
  target, 3 recorded on PAED-001, 1 on 010).
Gap: F-6.21a, T312 (still: the page matches the Expect, but a completed programme's record is rewritten by a catalogue
  change made after it ended: 15 of 15 becomes 14 of 14 and PAED-012's periods vanish; a product question).

### Step 6.22 — Prof Mbatha sees the pause, and cannot lift it
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/epas → /admin/curricula → /admin/curricula/{Id:int}/items → /admin/entrustment-decisions
Do: Read the EPA list, then 11.1's items. Then open Entrustment decisions and filter by Dr Molefe's name. Revoke
  nothing.
Expect:
  - **The EPA list.** PAED-012 reads Inactive. No national EPA offers her Edit: only its College changes one (T211's
    rule; `UpdateEpa`).
  - **11.1's items.** PAED-012's row is marked "(inactive: not in force)", under the same notice. Every row reads "Set
    by the College".
  - **Dr Molefe's STARs.** Her fifteen from Act 5 still read Active, beside Act 4's PAED-010, Superseded (the status
    filter starts on All). The PAED-012 row names its EPA "PAED-012 — Communicating
    with and counselling patients, caregivers and healthcare teams (no longer in use)" (T255): a pause leaves a STAR
    standing.
Note: An InstitutionalAdmin changes only KGK's own EPAs and items. Until T291 item 1 lands, the EPA list still offers
  her Edit on every row.
Actual (2026-09-28, T339 replay, wombat_scenario_t339): Her EPA list reads PAED-012 Inactive but offers her Edit on
  all 15 national EPAs, and Create EPA. Her curricula list shows 11.1 with Items only. 11.1's items mark PAED-012
  "(inactive: not in force)" under the same notice, beside "The College sets this curriculum's national items, so they
  are read only here. …"; all 15 rows read "Set by the College", and the form is "Add your institution's own item".
  Entrustment decisions from Home's quick links, status on All, Trainee "Molefe", Apply filters: 16 rows, 15 Active
  issued 2026-09-28 with no expiry and Act 4's PAED-010 at 4, expiring 2026-10-18, Superseded. The PAED-012 row reads
  "PAED-012 — … (no longer in use)", level 5, Active. Nothing revoked.
Gap: F-6.22a, T291 (still: the EPA list offers an InstitutionalAdmin Edit on every national EPA, which UpdateEpa
  refuses on save; the Note anticipates it).

### Step 6.23 — Dr Kruger restores PAED-012
Role: CollegeAdmin — Dr Anton Kruger
Route: /admin/epas → /admin/epas/{Id:int} → /admin/epas
Do: Open PAED-012, tick Active and save.
Expect: Nothing asks first. The page reads "EPA reactivated. 1 activity completed against it while it was inactive now
  counts towards progress." Deactivate is offered again. The EPA list reads PAED-012 Active.
Note: D48: reactivating credits the paused completions itself, so no rebuild is needed. The College's CollegeAdmin, or
  an Administrator.
Actual (2026-09-28, T339 replay, wombat_scenario_t339): PAED-012 read Active unticked. With Active ticked, Save asked
  nothing and read "EPA reactivated. 1 activity completed against it while it was inactive now counts towards
  progress."; Deactivate is offered again, DeactivatedOn is cleared, and the list reads PAED-012 Active. Dlamini now
  has a PAED-012 2026 S2 row (1 counted, ["25:complete"]), and activity 25's completion reads CreditedItemCount 1.
Gap: none

### Step 6.24 — Dr Dlamini's credit is counted
Role: Trainee — Dr Anele Dlamini
Route: /portfolio/progress → /activities/mine
Do: Read her progress and her activities.
Expect:
  - PAED-012's card is back, with `n + 1` this semester, and one more at the minimum level when observed than in Step
    6.15. The semester targets line counts `m` EPAs again.
  - The trajectory heading and My activities no longer carry "(no longer in use)", and the Mini-CEX's Credit reads
    1 item.
  - The credit PAED-012 had earned before the pause was kept, not cancelled. Dr Dlamini holds none from Acts 3-4
    (`n` is 0 in Step 6.15), so this is checked on Dr Molefe's PAED-012 progress from Act 3: its row is unchanged
    through the pause (by SQL, or on her record in Step 6.40).
Note: D48.
Actual (2026-09-28, T339 replay, wombat_scenario_t339): PAED-012's card is back: "1 of 3 this semester" (n + 1), "At
  the minimum level when observed: 1 of 1. Last encounter date: 2026-09-28." (0 in Step 6.15). "Semester targets 1 of
  10 EPAs met this semester" (m); MSF line 15 EPAs. The trajectory heading has no marker; My activities lists the
  Mini-CEX without it, Completed, Credited "1 item"; /activities/25 no longer warns and its Complete row reads "1
  item". By SQL Molefe's PAED-012 2026 S2 row (2 counted, activities 15 and 16, updated 15:44, before the pause) is
  unchanged.
Gap: none

## Phase 6.E — KGK's own additions

An institution adopts the national core as published and cannot change it. It may add EPAs and curriculum items of its
own, which measure only its own registrars (T091, "national core + local extras").

### Step 6.25 — Prof Mbatha creates a KGK EPA
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
Note: An InstitutionalAdmin creates only KGK's own EPAs (`CreateEpa`); a College or an Administrator creates national
  ones. Until T291 item 1 lands, the list offers her Edit on the national rows too.
Actual (2026-09-28, T339 replay, wombat_scenario_t339): Create EPA: the sub-speciality list offers only "Paediatrics /
  Paediatrics"; category Core or Elective. Saved as EPA id 17 (KGK's, Elective); the page became /admin/epas/17,
  Active ticked, Deactivate and Save offered. The EPA list shows 16 rows, KGK-001 first, and offers Edit on all 16.
  KGK-001's College column reads "College of Paediatricians of South Africa"; the columns are Code, Title, College,
  Speciality, Sub-speciality, Status, Actions, and nothing marks it as KGK's own.
Gap: F-6.25a, T291 (still: Edit is offered on all 15 national EPAs, which UpdateEpa refuses; the Note anticipates it).
  F-6.25b, T291 (still: the list does not tell an institution's own EPA from a national one).

### Step 6.26 — Prof Mbatha adds KGK's own item to 11.1
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
Note: The owning InstitutionalAdmin, and only on a curriculum the institution has adopted (T223).
Actual (2026-09-28, T339 replay, wombat_scenario_t339): Her curricula list shows 11.1 alone with Items only: no Edit,
  no Create curriculum. The form is headed "Add your institution's own item"; its picker offers KGK-001 alone; the
  scale is the CPSA ladder, "Suggested because every item on this curriculum is pinned to it."; Add item was disabled
  ("Adding is off until every minimum is a rung on CPSA Paediatric Entrustment Scale v11.1.") until 3a was chosen.
  Added: "Curriculum item added.", and the form gave way to "Every EPA your institution could add is already on this
  curriculum: …". The new row reads "Your institution's own item", 1 per academic year, No decision cadence, CPSA
  ladder, 3a, window 12, Direct observation, Edit and Remove; the 15 national rows read "Set by the College".
Gap: none

### Step 6.27 — Dr Dlamini is measured against KGK-001
Role: Trainee — Dr Anele Dlamini
Route: /portfolio/progress → /activities/new → /activities/new?type=direct_observation_cpsa
Do: Read her progress. Then start a Direct Observation (Paediatrics) and open its EPA picker. Leave without saving.
Expect: A KGK-001 card appears under "Once a year", reading 0 of 1 for the current year, and the yearly targets line
  counts one more EPA. Its line for the year before reads "0 of 1, 1 short": a target is read live, into periods that
  have already closed, as the items page warned. The Direct Observation picker offers ten EPAs, KGK-001 among them.
Note: A trainee reads only their own progress. An institution's own item measures only that institution's registrars.
Actual (2026-09-28, T339 replay, wombat_scenario_t339): A KGK-001 card leads "Once a year": "0 of 1 in 2026", "1 more
  by 30 November 2026.", "2025 academic year: 0 of 1, 1 short". "Yearly targets 0 of 6 EPAs met in 2026" (was 5);
  "Semester targets 1 of 10" unchanged; MSF line 16 EPAs; Annexure A lists KGK-001 as "The institution's own EPA; not
  in the exit rule". The Direct Observation (Paediatrics) picker offers ten EPAs, KGK-001 first. Left unsaved.
Gap: none

## Phase 6.F — A new curriculum version

A curriculum's targets are read live, so the College puts a new EPA and a changed target into a new version rather than
into the one registrars are measured against. There is no draft state. A new version can be adopted while it is Active,
so the College holds it back by unticking Active until it is ready.

### Step 6.28 — Dr Kruger adds a national EPA
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
Note: The College's CollegeAdmin, or an Administrator. 11.1 would accept PAED-016 too, and it would then apply at once
  to every registrar on 11.1.
Actual (2026-09-28, T339 replay, wombat_scenario_t339): Create EPA's sub-speciality list offers "Paediatrics /
  Neonatology" (first) and "Paediatrics / Paediatrics". Saved in Paediatrics / Paediatrics as EPA id 18 (national,
  Core); the page became /admin/epas/18, Active ticked. His EPA list holds 16 rows, PAED-001 to PAED-016, and no
  KGK-001. By SQL PAED-016 is on no curriculum item.
Gap: none

### Step 6.29 — Dr Kruger clones 11.1 as 11.2, and holds it back
Role: CollegeAdmin — Dr Anton Kruger
Route: /admin/curricula → /admin/curricula/{Id:int}/items → /admin/curricula/{Id:int} → /admin/curricula
Do: Read 11.1's items. Then, on 11.1's page, clone it: new version `11.2`, effective from `D`, with no end. On the new
  version's page, untick Active and save.
Expect:
  - 11.1's items still show him 15 rows: KGK's own item is not shown to the College (T211).
  - The clone form starts with the version "11.1-copy".
  - Once cloned, the page is 11.2's, with Active ticked: a clone is adoptable the moment it exists.
  - After Active is unticked and saved, the list shows 11.2 Inactive, with 15 items.
Note: The College's CollegeAdmin, or an Administrator. A clone copies every cell of the College's items: target,
  period, minima, scale pin, tools and decision. It copies none of an institution's own items
  (`Curriculum.CloneAsNewVersion`).
Actual (2026-09-28, T339 replay, wombat_scenario_t339): 11.1's items show him 15 national rows and no KGK-001; its
  picker offers PAED-016 alone. The clone card starts at "11.1-copy" (from 2026-01-02). Cloned as 11.2 from
  2026-09-28, no end: the page became /admin/curricula/4 with no confirmation line, Version 11.2, Active ticked.
  Unticked and saved: "Curriculum saved."; the list shows 11.2 Inactive, 15 items. By SQL curriculum 4's 15 items
  equal 11.1's national items in every cell, and it has no own item.
Gap: none

### Step 6.30 — Dr Kruger edits 11.2's items
Role: CollegeAdmin — Dr Anton Kruger
Route: /admin/curricula → /admin/curricula/{Id:int}/items
Do: On 11.2's items:
  1. Add PAED-016:
     - target: 1 per academic year;
     - decided each academic year, by the trainee's general panel;
     - the suggested scale, and minimum level 4;
     - year-by-year minima: 2, 3a, 3b and 4 for years 1 to 4;
     - tools: Mini-CEX, CBD and MSF.
  2. Edit PAED-011: set its target to 2 per academic year, and change nothing else.
Expect:
  - **Before the edits.** The 15 rows match 11.1's, cell for cell.
  - **The add.** The form is headed "Add item", and its EPA picker offers PAED-016 alone. After the add, the
    confirmation heads a note that every national EPA of Paediatrics is already on the curriculum. The table has 16
    rows.
  - **The edit.** PAED-011's edit row opens beneath its row. Once saved, the row reads "2 per academic year". 11.1's
    PAED-011 still reads 1 (outcome SQL).
Note: The College's CollegeAdmin, or an Administrator.
Actual (2026-09-28, T339 replay, wombat_scenario_t339): Before the edits 11.2's 15 rendered rows are identical to
  11.1's. The form is headed "Add item", its picker offers PAED-016 alone, the CPSA ladder suggested. After the add:
  "Curriculum item added." and "Every national EPA of Paediatrics is already on this curriculum, …"; 16 rows, PAED-016
  reading 1 per academic year, Decided each academic year, CPSA ladder, 4, Year 1: 2, Year 2: 3a, Year 3: 3b, Year 4:
  4, window 12, CBD, Mini-CEX, MSF. PAED-011's edit row opened directly beneath its row; saved with target 2:
  "Curriculum item saved.", the row reads "2 per academic year", nothing else changed. 11.1's PAED-011 still has
  RequiredCount 1 (SQL).
Gap: none

### Step 6.31 — Dr Kruger publishes 11.2
Role: CollegeAdmin — Dr Anton Kruger
Route: /admin/curricula → /admin/curricula/{Id:int} → /admin/curricula
Do: Open 11.2, tick Active and save.
Expect: The save is confirmed. The curricula list shows 11.1 and 11.2 Active, with 11.2 at 16 items, and the
  Neonatology curriculum still Inactive.
Note: The College's CollegeAdmin, or an Administrator.
Actual (2026-09-28, T339 replay, wombat_scenario_t339): On /admin/curricula/4, Active ticked and Save: "Curriculum
  saved.", nothing asked. The list reads Neonatology EPA Curriculum 1.0 Inactive (0 items), 11.2 Active (16 items,
  from 2026-09-28) and 11.1 Active (15).
Gap: none

## Phase 6.G — KGK takes up the new version

### Step 6.32 — Prof Mbatha adopts 11.2
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/adoptions
Do: Adopt "College of Paediatricians of South Africa — Paediatrics / Paediatrics: Paediatric EPA Curriculum (11.2)".
Expect:
  - The picker offers the two active Paediatric versions and the Demo IM Core curriculum. It does not offer the
    inactive Neonatology curriculum.
  - The adoption is confirmed. Current adoptions reads 11.2 Active and 11.1 Superseded, as the page's note says a newer
    version supersedes the current adoption.
  - The registrars already on 11.1 stay on it.
Note: An InstitutionalAdmin for her own institution, or an Administrator for any institution (T091).
Actual (2026-09-28, T339 replay, wombat_scenario_t339): Curriculum adoptions: the picker offers CPSA's 11.2 and 11.1
  and "Demo College — General Medicine / General Internal Medicine: IM Core Curriculum (2026.1)", not Neonatology 1.0.
  Adopt asked nothing and read "Curriculum adopted."; Current adoptions reads 11.2 Active and 11.1 Superseded, as the
  note ("Re-adopting a newer version of a discipline you already follow supersedes the current adoption for that
  discipline.") says. By SQL all five KGK profiles are still on curriculum 2, adoption 1.
Gap: none

### Step 6.33 — Prof Mbatha adds KGK's own item to 11.2
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/curricula → /admin/curricula/{Id:int}/items
Do: Read the curricula list. Open 11.2's items, and add KGK's own item on KGK-001 with the values of Step 6.26.
Expect:
  - **The list.** It shows 11.2, and still 11.1, because 11.1 holds KGK's own item (T211, T223). Both offer Items only.
  - **11.2's items.** It holds the College's 16 rows, PAED-016 and PAED-011's new target among them, all "Set by the
    College". It has no KGK item, because a new version carries no institution's own items.
  - **The add.** The picker offers KGK-001 alone. Once added, the row reads "Your institution's own item".
Note: The owning InstitutionalAdmin. An institution adds its own items again to each version it adopts (T091).
Actual (2026-09-28, T339 replay, wombat_scenario_t339): Her curricula list shows 11.2 (16 items) and 11.1 (16: 15
  national and KGK's own), both Active, each with Items only. 11.2's items hold the College's 16 rows, all "Set by the
  College", PAED-016 among them and PAED-011 at "2 per academic year", and no KGK row. The form is "Add your
  institution's own item", picker KGK-001 alone; with Step 6.26's values: "Curriculum item added."; the new row reads
  "Your institution's own item", 1 per academic year, No decision cadence, 3a, window 12, Direct observation, Edit and
  Remove. 17 rows.
Gap: none

### Step 6.34 — Prof Mbatha cannot save Dr Dlamini's profile while she stays on 11.1
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/trainees → /admin/trainees/edit
Do: Open Dr Dlamini's profile, which is on Paediatric EPA Curriculum (11.1). Move the expected completion date six
  months later than it reads, leave the curriculum as it is, and save the profile. Then reopen the profile.
Expect:
  - The curriculum picker offers 11.1 and 11.2, with 11.1 chosen.
  - The save is refused: "Trainees must be admitted into the curriculum version this institution has adopted."
  - Nothing changes: the reopened profile shows the old date, and Dr Dlamini stays on 11.1, as re-adopting leaves
    registrars on the version they were admitted to (`CurriculumAdminScope`, T211).
Note: An InstitutionalAdmin, or an Administrator. Every profile save re-checks its curriculum against KGK's active
  adoption, now 11.2 (`UpdateTraineeProfile`, `TraineeAdoptionResolver`; T091's hard gate).
Actual (2026-09-28, T339 replay, wombat_scenario_t339): Trainees → Edit on Anele Dlamini opened
  /admin/trainees/edit?id=3: Paediatric EPA Curriculum (11.1) selected, (11.2) also offered, start 2024-01-15,
  expected completion 2028-01-14. Moved to 2028-07-14, Save profile was refused: "Trainees must be admitted into the
  curriculum version this institution has adopted." Reopened, it reads 2028-01-14 and 11.1; by SQL profile 3 is
  unchanged (curriculum 2, adoption 1).
Gap: F-6.34a, T304 (still: as the Expect says, but once KGK re-adopts no profile left on 11.1 can be saved at all,
  even for its expected completion date alone, although registrars are meant to stay pinned to their version).

### Step 6.35 — Dr Ndlovu notes his figures before he moves
Role: Trainee — Dr Sipho Ndlovu
Route: /portfolio/progress
Do: Read the page, and note each card's count for its current period.
Expect: Sixteen cards: the 15 national EPAs of 11.1, and KGK-001. PAED-016 is not among them.
Note: Count his tallies too, as `t`: `SELECT count(*) FROM "CurriculumItemProgresses" p JOIN "CurriculumItems" i ON
  i."Id" = p."CurriculumItemId" JOIN "Curricula" c ON c."Id" = i."CurriculumId" JOIN "AspNetUsers" u ON u."Id" =
  p."TraineeUserId" WHERE u."Email" = 'ndlovu@kgk.wombat.local' AND c."Version" = '11.1';`
Actual (2026-09-28, T339 replay, wombat_scenario_t339): Sixteen cards: ten "Each semester" (PAED-001 to 007, 010, 012,
  015) and six "Once a year" (KGK-001, PAED-008, 009, 011, 013, 014); no PAED-016. Every current-period count is 0
  except PAED-002, "1 of 3 this semester"; PAED-011 reads "0 of 1 in 2026". "Semester targets 0 of 10", "Yearly
  targets 0 of 6". t = 1 (PAED-002, 2026 S2, activity 3).
Gap: none

### Step 6.36 — Prof Mbatha moves Dr Ndlovu to 11.2
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/trainees → /admin/trainees/edit
Do: Open Dr Ndlovu's profile. Choose Paediatric EPA Curriculum (11.2), keep his programme start (`J`, from Act 2) and
  his expected completion as they read, and save the profile.
Expect: The save is confirmed, and the profile reads 11.2. From now on, his completions credit 11.2's items.
Note: An InstitutionalAdmin, or an Administrator, and only into the version KGK has adopted (`TraineeAdoptionResolver`).
Actual (2026-09-28, T339 replay, wombat_scenario_t339): His profile (/admin/trainees/edit?id=6) read 11.1, start
  2026-01-15 (J), expected completion 2030-01-14; with 11.2 chosen and the dates kept, Save profile read "Trainee
  profile saved." and the select reads 11.2. By SQL profile 6 is on curriculum 4, adoption 2, dates unchanged; his one
  tally is still the 11.1 row.
Gap: none

### Step 6.37 — Dr Ndlovu on 11.2, before progress is rebuilt
Role: Trainee — Dr Sipho Ndlovu
Route: /portfolio/progress → /activities/new → /activities/new?type=mini_cex_cpsa
Do: Read the page. Then start a Mini-CEX (Paediatrics) and open its EPA picker. Leave without saving.
Expect:
  - **The cards.** Seventeen: 11.2's 16 national items (PAED-016 once a year, and PAED-011 at 2 per academic year,
    among them) and KGK-001.
  - **The counts.** His encounters before the move are counted in 11.1's tallies, which this page no longer reads. Until
    Step 6.38, each card shows only what 11.2 holds for him, which is nothing yet.
  - **The picker.** The Mini-CEX picker offers ten EPAs, PAED-016 among them.
Note: Moving a registrar replays none of his credit. The Administrator's rebuild does (Step 6.38).
Actual (2026-09-28, T339 replay, wombat_scenario_t339): Seventeen cards: ten "Each semester" and seven "Once a year"
  (KGK-001, PAED-008, 009, 011, 013, 014, 016); PAED-011 reads "0 of 2 in 2026" (Target: 2 per academic year),
  PAED-016 "0 of 1 in 2026". Every card reads 0, PAED-002 included (1 in Step 6.35); "Yearly targets 0 of 7", MSF line
  17 EPAs, exit rule 16. Nothing on the page says his counts await a rebuild. The Mini-CEX picker offers ten EPAs,
  PAED-016 among them. Left unsaved (newest activity still 25).
Gap: F-6.37a, T304 (still: as the Expect says, but a moved registrar's cards are left at zero with no word to anyone:
  the save read only "Trainee profile saved.", his page does not say his evidence is not yet counted, and only the
  Administrator's global rebuild refills it).

### Step 6.38 — devadmin rebuilds curriculum progress
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
    credited, semester tallies written, stale tallies removed, and completions re-stamped. Stale tallies removed is at
    least `t` from Step 6.35.
Note: Administrator only (the page's policy, and `RebuildCurriculumProgressCommand`). It is safe to run more than once.
Actual (2026-09-28, T339 replay, wombat_scenario_t339): Curricula (from the menu) offers an outline "Curriculum
  progress" (btn-outline) beside Create curriculum in its header. It opened /admin/curriculum-progress, "Curriculum
  progress · Wombat", Curricula still lit, trail Home › Curricula › Curriculum progress. The page says a rebuild
  "credits each activity against the curriculum as it is today", "Only whether an EPA was active is judged as of when
  the activity was completed", and "It runs as one transaction". Rebuild progress asked "Rebuild curriculum
  progress?"; confirmed: "Curriculum progress was rebuilt." with Activities re-read 15, Curriculum items credited 14,
  Semester tallies written 9, Stale tallies removed 1 (t = 1), Completions re-stamped 15. By SQL Ndlovu's PAED-002
  tally is now on 11.2; every CreditedItemCount is unchanged (activity 24: 0, 25: 1).
Gap: none

### Step 6.39 — Dr Ndlovu's evidence counts on 11.2
Role: Trainee — Dr Sipho Ndlovu
Route: /portfolio/progress
Do: Read the page.
Expect: Each of the 15 EPAs carried over from 11.1 reads the count he noted in Step 6.35 for its current period.
  PAED-016 and KGK-001 read 0. PAED-011's card measures its count against a target of 2 per academic year.
Note: A trainee reads only their own progress.
Actual (2026-09-28, T339 replay, wombat_scenario_t339): Each of the 15 carried-over EPAs reads what Step 6.35 noted:
  PAED-002 "1 of 3 this semester", every other 0. PAED-016 and KGK-001 read "0 of 1 in 2026", PAED-011 "0 of 2 in
  2026". Seventeen cards; "Semester targets 0 of 10", "Yearly targets 0 of 7".
Gap: none

### Step 6.40 — Dr Molefe's record after the rebuild
Role: Former trainee (no role; a trainee record) — Dr Lerato Molefe
Route: /portfolio/progress
Do: Read her record, card by card, against what she read in Act 5 (Step 5.20).
Expect:
  - Her fifteen national cards are back, PAED-012 among them, and each period reads what Act 5 recorded: the rebuild
    replays a completed profile's evidence as it was (Act 5's handoff).
  - PAED-006's card carries the corrected title. There is no PAED-016 card: her profile is on 11.1.
  - A KGK-001 card has appeared under "Once a year", counting none of her encounters: each closed year of her
    programme reads "0 of 1, 1 short". KGK's own item on 11.1 is read live, into a record that has ended.
Note: The TraineeOrFormerTrainee policy admits her without the Trainee role (T252).
Actual (2026-09-28, T339 replay, wombat_scenario_t339): "You completed your programme on 28 September 2026. …",
  read-only, no bars. Sixteen cards: the 15 national, PAED-012 back, Semester 2, 2026 "no target (your programme ended
  part-way through)" with 3 recorded on PAED-001, 1 on 010, 2 on 012 and 0 elsewhere, as Step 5.20 read them. PAED-006
  carries the corrected title; no PAED-016 card. KGK-001 leads "Once a year": 2026 no target, 0 recorded; 2025, 2024
  and 2023 "0 of 1, 1 short". Standing "15 at or above · 0 below · 1 with no decision, of 16 EPAs"; exit rule 15 of
  15, KGK-001 "The institution's own EPA; not in the exit rule".
Gap: F-6.40a, T312 (still: the page matches the Expect, but a completed record gains an item KGK added after she
  graduated, with a shortfall in each closed year and "1 with no decision"; the same read feeds the PDF; a product
  question).

### Step 6.41 — Dr du Plessis's record after the rebuild
Role: Trainee — Dr Pieter du Plessis
Route: /portfolio/progress → /activities/mine
Do: Read his record and his activities.
Expect:
  - My progress still opens with the notice that his programme ended on the day Act 5 recorded, and is read-only.
  - PAED-002's period holding that day still reads "no target (your programme ended part-way through)", with the
    count Act 5 Step 5.28 read: the Mini-CEX observed after his last day still counts towards nothing (T281).
  - A KGK-001 card has appeared on his record too, as on Dr Molefe's.
  - My activities lists that Mini-CEX as Completed, its Credit None.
Note: He keeps the Trainee role after withdrawing (Act 5), so he reads his own record like any trainee.
Actual (2026-09-28, T339 replay, wombat_scenario_t339): My progress still opens "Your programme ended on 26 September
  2026. This page is your record of it and is read-only …", training year 2, no bars. PAED-002 "Semester 2, 2026 no
  target (your programme ended part-way through) · 0 recorded", as Step 5.28 read. A KGK-001 card has appeared: 2026
  no target, 0 recorded; "2025 academic year: 0 of 1, 1 short"; standing "16 with no decision, of 16 EPAs". My
  activities lists the Mini-CEX (PAED-002, 2026-09-27) Completed, Credited None; activity 24's completion still
  CreditedItemCount 0.
Gap: none (the KGK-001 card on an ended record is F-6.40a)

## Outcome state

Replay check (2026-09-28, T339 replay, wombat_scenario_t339): match. All 13 checks read as written: CNSA, CPSA and
  DEMO-C active; Adult Neurology on the CNSA scale (6 levels), Neonatology and Paediatrics on the CPSA ladder; 16 of
  16 national CPSA EPAs active; PAED-006 retitled; KGK-001 KGK's only own EPA; Neonatology 1.0 inactive 0/0, 11.1
  active 15/1, 11.2 active 16/1; PAED-011 1 on 11.1 and 2 on 11.2; KGK 11.1 superseded, 11.2 active; Dlamini and
  Mahlangu 11.1 active, du Plessis and Molefe 11.1 inactive, Ndlovu 11.2 active; Ndlovu's tallies 11.2: 1 (t = 1),
  none on 11.1; activity 25 credited 1; du Plessis's activity 24 credited 0. cpsa_case_presentation_log is the only
  draft. Snapshot recovery/scenario-t339-post-act6.dump.

**The catalogue:**
- **Colleges:** three, each Active:
  - CNSA, with the Neurology speciality and the Adult Neurology sub-speciality, but no CollegeAdmin and no curriculum;
  - CPSA, with its corrected description;
  - the Demo College.
- **Scales:** the CPSA ladder, the O-R Scale, and the six-level CNSA Neurology Entrustment Scale. The CNSA scale is
  Adult Neurology's default, and cannot be deleted while it is.
- **The CPSA catalogue:**
  - the Paediatrics speciality, with its new description;
  - the sub-specialities Paediatrics and Neonatology, both defaulting to the CPSA ladder;
  - national EPAs PAED-001 to PAED-016, all active, PAED-006 retitled and PAED-012 restored;
  - three curricula:

    | Curriculum | Status | Items |
    |---|---|---|
    | Paediatric EPA Curriculum 11.1 | Active | 15 national, plus KGK's own |
    | Paediatric EPA Curriculum 11.2 | Active | 16 national (PAED-011 at 2 per academic year), plus KGK's own |
    | Neonatology EPA Curriculum 1.0 | Inactive | none |

  - one activity type of the College's own, `CPSA Case Presentation Log` (`cpsa_case_presentation_log`), scoped to the
    Paediatrics speciality, a draft with no published version (Step 6.14a). The twelve `*_cpsa` instruments are as
    seeded, and none has a draft. (Added with Step 6.14a on 2026-09-26, after the T295 replay, so the
    `scenario-post-act6` snapshot does not hold it.)

**KGK:**
- **Its own EPA:** KGK-001, with an item of KGK's own on each Paediatric version.
- **Adoptions:** 11.2 Active and 11.1 Superseded.
- **Registrars:**
  - Dr Ndlovu is on 11.2, and his progress is rebuilt there.
  - Dr Dlamini and Dr Mahlangu stay on 11.1. Neither profile can be saved while KGK's active adoption is 11.2 (Step
    6.34).
  - Dr Molefe's completed record and Dr du Plessis's withdrawn one keep what Act 5 recorded, and each now also shows
    KGK-001 (Steps 6.40 and 6.41).
- **The Mini-CEX from Step 6.16:** completed, and credited once PAED-012 was restored.

**SQL checks** (identifiers as EF creates them):

```sql
-- Three Colleges, all active: CNSA, CPSA, DEMO-C
SELECT "ShortCode", "IsActive" FROM "Colleges" ORDER BY "ShortCode";

-- Adult Neurology -> CNSA Neurology Entrustment Scale; Neonatology and Paediatrics -> CPSA Paediatric Entrustment Scale v11.1
SELECT s."Name", sc."Name" FROM "SubSpecialities" s LEFT JOIN "EntrustmentScales" sc ON sc."Id" = s."DefaultEntrustmentScaleId"
WHERE s."Name" IN ('Adult Neurology', 'Neonatology', 'Paediatrics') ORDER BY s."Name";

-- 6
SELECT count(*) FROM "EntrustmentLevels" l JOIN "EntrustmentScales" s ON s."Id" = l."ScaleId"
WHERE s."Name" = 'CNSA Neurology Entrustment Scale';

-- 16 national CPSA EPAs, all active and none paused
SELECT count(*) FILTER (WHERE e."IsActive" AND e."DeactivatedOn" IS NULL), count(*) FROM "Epas" e
JOIN "SubSpecialities" s ON s."Id" = e."SubSpecialityId" JOIN "Specialities" p ON p."Id" = s."SpecialityId"
JOIN "Colleges" c ON c."Id" = p."CollegeId" WHERE c."ShortCode" = 'CPSA' AND e."OwningInstitutionId" IS NULL;

-- 'Managing long-term health conditions (LTHCs) in children'
SELECT "Title" FROM "Epas" WHERE "Code" = 'PAED-006' AND "OwningInstitutionId" IS NULL;

-- KGK-001, KGK's only EPA of its own
SELECT e."Code" FROM "Epas" e JOIN "Institutions" n ON n."Id" = e."OwningInstitutionId" WHERE n."ShortCode" = 'KGK';

-- Neonatology EPA Curriculum 1.0: inactive, 0, 0 | 11.1: active, 15, 1 | 11.2: active, 16, 1
SELECT c."Name", c."Version", c."IsActive",
       count(i."Id") FILTER (WHERE i."OwningInstitutionId" IS NULL) AS national,
       count(i."Id") FILTER (WHERE i."OwningInstitutionId" IS NOT NULL) AS own
FROM "Curricula" c LEFT JOIN "CurriculumItems" i ON i."CurriculumId" = c."Id"
WHERE c."Name" IN ('Paediatric EPA Curriculum', 'Neonatology EPA Curriculum')
GROUP BY c."Id", c."Name", c."Version", c."IsActive" ORDER BY c."Name", c."Version";

-- PAED-011: 1 on 11.1, 2 on 11.2
SELECT c."Version", i."RequiredCount" FROM "CurriculumItems" i JOIN "Epas" e ON e."Id" = i."EpaId"
JOIN "Curricula" c ON c."Id" = i."CurriculumId"
WHERE e."Code" = 'PAED-011' AND c."Name" = 'Paediatric EPA Curriculum' ORDER BY c."Version";

-- KGK: 11.1 superseded (false), 11.2 active (true)
SELECT c."Version", a."IsActive" FROM "InstitutionCurriculumAdoptions" a JOIN "Institutions" n ON n."Id" = a."InstitutionId"
JOIN "Curricula" c ON c."Id" = a."CurriculumId" WHERE n."ShortCode" = 'KGK' ORDER BY c."Version";

-- dlamini, mahlangu: 11.1 active; duplessis, molefe: 11.1 not active; ndlovu: 11.2 active
SELECT u."Email", c."Version", p."IsActive" FROM "TraineeProfiles" p JOIN "AspNetUsers" u ON u."Id" = p."UserId"
JOIN "Curricula" c ON c."Id" = p."CurriculumId" WHERE u."Email" LIKE '%@kgk.wombat.local' ORDER BY u."Email";

-- Dr Ndlovu's tallies: none on 11.1, and t (Step 6.35) on 11.2
SELECT c."Version", count(*) FROM "CurriculumItemProgresses" p JOIN "CurriculumItems" i ON i."Id" = p."CurriculumItemId"
JOIN "Curricula" c ON c."Id" = i."CurriculumId" JOIN "AspNetUsers" u ON u."Id" = p."TraineeUserId"
WHERE u."Email" = 'ndlovu@kgk.wombat.local' GROUP BY c."Version";

-- 1: the paused completion was credited when PAED-012 was restored
SELECT "CreditedItemCount" FROM "ActivityTransitions"
WHERE "ActivityId" = <the id noted in Step 6.16> AND "TransitionKey" = 'complete';

-- 0: after the rebuild, du Plessis's Mini-CEX observed after his last day still credits nothing (Act 5, T281)
SELECT t."CreditedItemCount" FROM "ActivityTransitions" t JOIN "Activities" a ON a."Id" = t."ActivityId"
JOIN "AspNetUsers" u ON u."Id" = a."SubjectUserId" JOIN "TraineeProfiles" p ON p."UserId" = u."Id"
WHERE u."Email" = 'duplessis@kgk.wombat.local' AND a."ObservedOn" > p."DeactivatedOn" AND t."TransitionKey" = 'complete';
```

## Handoff to the appendix

- **Order.** The appendix runs next, and last.
- **What Act 6 leaves finished.** Nothing is paused or half-done:
  - PAED-012 is active;
  - 11.2 is published and adopted by KGK;
  - the CNSA scale is in use as Adult Neurology's default.
- **What Act 6 leaves deliberately incomplete:**
  - the Neonatology curriculum is inactive and empty;
  - CNSA has no CollegeAdmin and no curriculum;
  - the College's `CPSA Case Presentation Log` is a draft, never published, so no registrar is offered it (Step 6.14a).
- **What the appendix inherits:**
  - two Paediatric versions in use at KGK: Dr Ndlovu on 11.2, Dr Dlamini and Dr Mahlangu on 11.1, whose profiles
    cannot be saved while 11.2 is KGK's active adoption (Step 6.34);
  - three current trainees (Dr Dlamini, Dr Mahlangu and Dr Ndlovu), a graduate (Dr Molefe) and a withdrawn registrar
    (Dr du Plessis, still holding Trainee), as Act 5 handed them on;
  - KGK-001, and KGK's own item on each version;
  - PAED-006's corrected title;
  - one rebuild already run.
- **Personal data.** None of Act 6's catalogue rows belongs to a person, so an erasure in the appendix touches none of
  them.
- **Snapshot.** `pg_dump -Fc` the database to `recovery/scenario-post-act6.dump`.
