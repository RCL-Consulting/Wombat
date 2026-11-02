# Act 6 — After graduation: the catalogue is maintained

**Scenario date.** In the story, early 2027, in the weeks after Dr Molefe's graduation (Act 5). The story's dates are
narrative only: every date a step types is relative to the replay day `D` (README, "The clock").

**Who acts.**
- devadmin (Administrator) keeps the College records and the entrustment scales, and rebuilds progress.
- Dr Anton Kruger (CollegeAdmin, CPSA) keeps the national catalogue.
- Prof Nolwazi Mbatha (InstitutionalAdmin, KGK) keeps KGK's adoption and KGK's own additions.
- Dr Dlamini, Dr Mahlangu and Dr Ndlovu (Trainees), Dr Molefe (a former trainee), Dr du Plessis (withdrawn in Act 5,
  with no role) and Dr Patel (Assessor) act only to show what each catalogue change does to the people it
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
Actual (2026-11-02, T392 replay, wombat_scenario_t392): D = 2026-11-02. The list read two rows (CPSA, Demo College).
  An empty save shows "The Name field is required." and "The ShortCode field is required."; with short code CPSA the
  save was refused with "A college with the same name or short code already exists." and stayed on
  /admin/colleges/new. With CNSA it opened /admin/colleges/4, "Edit college", Active. The list reads three rows, all
  Active, each with Edit and Specialities: CNSA, CPSA, Demo College.
Gap: F-6.1a, T324 (still: the required-field message names the model property, "The ShortCode field is required.", not
  the label "Short code"; the Expect itself is met). known-task.

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
Actual (2026-11-02, T392 replay, wombat_scenario_t392): CPSA is /admin/colleges/2 (name, short code CPSA, Active as
  seeded). With only the description changed, Save showed "College saved."; the list's CPSA row reads the new
  description, Active, name and short code unchanged. Deactivate was not pressed. After the restart played after Step
  6.14a (the stop script printed "Nothing listens on 5180." while the app answered, T391, so the process was stopped
  by id after checking its command line, then started with the script; the old log kept as
  wombat_scenario_t392-20261102-202932.log, 40 Stub emails) the description stands (SQL).
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
Actual (2026-11-02, T392 replay, wombat_scenario_t392): CNSA's Specialities opened /admin/colleges/4/specialities,
  "College: College of Neurologists of South Africa (CNSA)", "No specialities yet / Create the first speciality for
  this college." Creating Neurology ("Specialist training in Neurology.") opened /admin/colleges/4/specialities/3,
  "Edit speciality", the same College line, with "Manage sub-specialities", Active. The list has one row: Neurology,
  Active.
Gap: none

### Step 6.4 — devadmin adds the Adult Neurology sub-speciality
Role: Administrator — devadmin
Route: /admin/colleges/{CollegeId:int}/specialities/{Id:int} → /admin/specialities/{SpecialityId:int}/sub-specialities → /admin/specialities/{SpecialityId:int}/sub-specialities/new → /admin/specialities/{SpecialityId:int}/sub-specialities/{Id:int} → /admin/specialities/{SpecialityId:int}/sub-specialities
Do: From Neurology's page, manage its sub-specialities and create `Adult Neurology`, with the description "Adult
  neurology specialist training programme."
Expect: The list is empty before the create and has one row after it. The new sub-speciality's edit page offers a
  default entrustment scale, which reads "No default — offer every scale". Its choices are the two existing scales.
Note: An Administrator, or the College's own CollegeAdmin. Step 6.7 sets the default.
Actual (2026-11-02, T392 replay, wombat_scenario_t392): /admin/specialities/3/sub-specialities: "No sub-specialities
  yet / Create the first sub-speciality under this speciality." Creating Adult Neurology opened
  /admin/specialities/3/sub-specialities/3: "Default entrustment scale" reads "No default — offer every scale", its
  other choices the CPSA ladder and the O-R Scale. The list has one row, Adult Neurology, Active.
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
Actual (2026-11-02, T392 replay, wombat_scenario_t392): With level 4 also "Indirect supervision" the save was refused:
  "Level labels must be unique within a scale." and stayed on /new. Corrected, it opened the new scale's edit page
  (/admin/entrustment-scales/3), levels 1 Observe only to 5 Unsupervised practice in order. The list shows three
  scales: CNSA 5, CPSA ladder 6, O-R Scale 5.
Gap: none

### Step 6.6 — devadmin adds a sixth level
Role: Administrator — devadmin
Route: /admin/entrustment-scales → /admin/entrustment-scales/{Id:int} → /admin/entrustment-scales
Do: Open the CNSA scale, add a sixth level, `Supervises others` ("Supervises juniors performing the activity."), and
  save.
Expect: The save is confirmed, and the page lists six levels, 1 to 6, with `Supervises others` at 6. The list shows 6
  levels.
Note: Administrator only.
Actual (2026-11-02, T392 replay, wombat_scenario_t392): With a sixth level "Supervises others", Save showed
  "Entrustment scale saved." and the page lists 1 to 6, Supervises others at 6. The list reads 6 levels.
Gap: none

### Step 6.7 — devadmin makes it Adult Neurology's default
Role: Administrator — devadmin
Route: /admin/specialities/{SpecialityId:int}/sub-specialities/{Id:int}
Do: Open Adult Neurology, set its default entrustment scale to the CNSA Neurology Entrustment Scale, and save.
Expect: The save is confirmed, and reopening the page shows the scale selected. Its help says that committee STARs for
  this programme's trainees are limited to that scale's levels.
Note: An Administrator, or the College's CollegeAdmin.
Actual (2026-11-02, T392 replay, wombat_scenario_t392): The default offers No default, the CNSA scale, the CPSA ladder
  and the O-R Scale. Choosing the CNSA scale and saving showed "Sub-speciality saved."; reopened, it is selected, with
  "Committee STARs for this programme's trainees are limited to this scale's levels."
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
Actual (2026-11-02, T392 replay, wombat_scenario_t392): The CNSA row's Delete sent the command at once: no dialog
  opened. It was refused: "This entrustment scale is the default scale of the sub-speciality "Adult Neurology"
  (Neurology), so it cannot be deleted. Change that sub-speciality's default entrustment scale to another scale, or to
  no default, first." The scale stays, 6 levels.
Gap: F-6.8a, T264 (still: Delete does not ask first, against DESIGN.md § Button system; the refusal text matches the
  Expect). known-task.

### Step 6.9 — Prof Mbatha reads the scales, and changes none
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/entrustment-scales
Do: Read the list.
Expect: Under the subtitle "The rating ladders that assessment forms and committee reviews use. A Wombat
  administrator sets them; they are shown here for reference." (T392, Q13), three scales, with their level counts: the
  CPSA ladder at 6, the O-R Scale at 5 and the CNSA scale at 6. There is no Create scale, and no column of Edit and
  Delete: a column no row offers is not rendered (T239).
Note: An InstitutionalAdmin reads the scales; only an Administrator changes one.
Actual (2026-11-02, T392 replay, wombat_scenario_t392): Prof Mbatha (Entrustment scales lit) reads, under the subtitle
  "The rating ladders that assessment forms and committee reviews use. A Wombat administrator sets them; they are
  shown here for reference.", three scales under Name, Description and Levels only: CNSA 6, CPSA ladder 6, O-R Scale
  5. No Create scale, no Actions column, no link or button in the page body. At 390 px the page does not scroll
  sideways (scrollWidth 390).
Gap: none (T326's scales half no longer occurs: the reader's subtitle says an administrator sets them)

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
Actual (2026-11-02, T392 replay, wombat_scenario_t392): Signed in, Home ("College admin · Semester 2, 2026") has one
  card, National catalogue, with Specialities, EPAs and Curricula; the menu offers Home, Specialities, EPAs,
  Curricula, Activity types and My data rights. /admin/colleges/2 and /admin/entrustment-scales/new each landed on
  /access-denied?ReturnUrl=…, "You cannot open this page", "Your role (College admin) does not open this page.", Go to
  Home; no menu item lit. Nothing changed.
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
Actual (2026-11-02, T392 replay, wombat_scenario_t392): Specialities opened /admin/colleges/2/specialities, "College:
  College of Paediatricians of South Africa (CPSA)", one row, Paediatrics. Edit opened
  /admin/colleges/2/specialities/2 (Back to specialities, Manage sub-specialities, Cancel; no colleges link); the new
  description saved with "Speciality saved." and the list shows it, Active. The list still offers him "Back to
  colleges" (/admin/colleges). After the restart played after Step 6.14a the description stands (SQL).
Gap: F-6.11a, T291 (still: the specialities list offers a CollegeAdmin "Back to colleges", a page he is refused; the
  Note anticipates it). known-task.

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
Actual (2026-11-02, T392 replay, wombat_scenario_t392): /admin/specialities/2/sub-specialities had one row,
  Paediatrics. Creating Neonatology opened /admin/specialities/2/sub-specialities/4 with "No default — offer every
  scale"; choosing the CPSA ladder and saving showed "Sub-speciality saved.", and it stays selected when reopened. The
  list holds Neonatology and Paediatrics, both Active; Paediatrics keeps the CPSA ladder (outcome SQL).
Gap: none

### Step 6.13 — Dr Kruger starts a Neonatology curriculum, and holds it back
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
Note: The College's CollegeAdmin, or an Administrator. Active decides only whether an institution may adopt the
  curriculum (`GetAdoptableCurricula`).
Actual (2026-11-02, T392 replay, wombat_scenario_t392): The form selects Paediatrics / Neonatology first. Effective
  from 2026-11-02 (D), no end, the create opened /admin/curricula/3, "Edit curriculum", with Active unticked (SQL:
  IsActive false) and a "Clone as new version" card (version "1.0-copy", from 2026-11-03), Manage items in the header.
  The list reads Neonatology EPA Curriculum (1.0), Paediatrics / Neonatology, 2026-11-02, 0 items, "Held back" (11.1
  "Open"). Its items page, "Items of Neonatology EPA Curriculum (1.0)" (h1 and tab), subtitled "College of
  Paediatricians of South Africa · Paediatrics / Neonatology · effective 2026-11-02", reads "Items (0)" with no table,
  and in place of the add link: "Every national EPA of Neonatology is already on this curriculum, as a national item
  or as an institution's own item, so there is none left to add. To add an item, first add a national EPA of
  Neonatology, …". No sideways scroll at 1280. (6.13-2 not retaken: nothing is unticked now.)
Gap: F-6.13a, T326 (still: the empty state says every national EPA of Neonatology is already on the curriculum, though
  Neonatology has none; the held-back start and the title are met). known-task.

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
Actual (2026-11-02, T392 replay, wombat_scenario_t392): The EPA list's PAED-006 row offers the code link and "Edit
  PAED-006 — …"; Edit opened /admin/epas/7, h1 and tab "PAED-006 — Managing long-term health conditions (LTHCs)". With
  only the title changed, Save showed "EPA saved." (the focus on it), the h1 now "… in children", Active stays ticked
  and Deactivate… is offered. The EPA list reads "PAED-006 — Managing long-term health conditions (LTHCs) in children
  | College of Paediatricians of South Africa | Paediatrics / Paediatrics | Active", and 11.1's items read the new
  title. After the restart played after Step 6.14a the title stands by SQL (the boot logged "0 republished, 22
  unchanged").
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
Actual (2026-11-02, T392 replay, wombat_scenario_t392): Activity types in his menu, lit. The list has 22 rows: the ten
  Demo rows offer View; the twelve *_cpsa rows offer Edit named for the row ("Edit Mini-CEX (Paediatrics)"), msf_cpsa
  and learner_feedback_cpsa included. No KGK Teaching Session Log. New activity type offered. "Edit Multi-Source
  Feedback (Paediatrics)" (/admin/activity-types/21) opened with Save draft, Publish (off) and every editor control
  (Add section, Edit, Up, Down, Delete, Add field), and no read-only notice; nothing pressed. New activity type opened
  "New activity type" on Form with the default draft (Details section, Title field). Scope offers Speciality and
  Sub-speciality, starting on Speciality with Paediatrics alone; Sub-speciality offers "Paediatrics / Neonatology" and
  "Paediatrics / Paediatrics". Save draft was accepted: /admin/activity-types/24, "Edit CPSA Case Presentation Log",
  with no "Draft saved." (Save draft, Discard draft, Publish). The list has 23 rows, the new one "Speciality ·
  Paediatrics", v0, "Draft saved", Active, Edit. Prof Mbatha's list has 23 rows without it (KGK Teaching Session Log
  among them), and /admin/activity-types/24 reads "The activity type could not be found." Then the app was restarted
  (see Step 6.2).
Gap: F-6.14aa, T291 (still: the first Save draft shows no "Draft saved."). F-6.14ab, T334 (still: msf_cpsa and
  learner_feedback_cpsa offer the College Edit, and msf_cpsa opens with no notice and every editor live; not saved).
  known-task.

## Phase 6.D — An EPA paused and restored

The College takes PAED-012 out of use while it revises it. By D48:
- **Deactivating pauses credit, and does not refuse filing.** Pickers and progress leave the EPA out, and the credit it
  has earned is kept.
- **A completion during the pause credits nothing.** Reactivating the EPA credits those completions.

### Step 6.15 — Dr Dlamini reads her progress before the pause
Role: Trainee — Dr Anele Dlamini
Route: /portfolio/progress → /portfolio/progress/{EpaId:int}
Do: Read the page, then open PAED-012's row. Note these, for Steps 6.19 and 6.24:
  - PAED-012's count this semester (`n`);
  - on its page, its count at the minimum level when observed, and the level its training year line names;
  - the count under "EPAs met this semester" (`m`).
Expect: PAED-012 is a row of "Each semester · 10 EPAs". PAED-006's row carries the title corrected in Step 6.14,
  "PAED-006 — Managing long-term health conditions (LTHCs) in children". This period reads "… of `m`" over "EPAs met
  this semester". PAED-012's page (`/portfolio/progress/{EpaId}`) reads "3 a semester" under its h1, its count `n`, its
  count at the minimum level when observed (no such line while `n` is 0), and "Training year 3: level …, the minimum
  each encounter is judged against and your STAR's target."
Note: A trainee reads only their own progress.
Actual (2026-11-02, T392 replay, wombat_scenario_t392): PAED-012 is a row of "Each semester · 10 EPAs": "0 of 3 this
  semester", "3 more by 2026-11-30", so n = 0. PAED-006's row reads "PAED-006 — Managing long-term health conditions
  (LTHCs) in children". This period reads "1 of 10" over "EPAs met this semester", so m = 1 ("0 of 5" over "EPAs met
  in 2026"; "Once a year · 5 EPAs"). PAED-012's link opened /portfolio/progress/13: under its h1 "3 a semester ·
  Decided each semester · Exit level 5", Observations "0 of 3 this semester", "3 more by 2026-11-30.", no
  at-the-minimum line, and "Training year 3: level 4, the minimum each encounter is judged against and your STAR's
  target." (the level: 4).
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
Actual (2026-11-02, T392 replay, wombat_scenario_t392): Log an activity lists seven rated types, none of them CPSA
  Case Presentation Log (Step 6.14a). Mini-CEX (Paediatrics) (/activities/new?type=mini_cex_cpsa): the EPA picker
  offers nine, PAED-001 to 004, 006 (corrected title), 007, 008, 012 and 013. Filed with the Do's values, encounter
  2026-11-02 (D); Submit to Mohammed Patel opened /activities/25: "Submitted. It is now Requested. It is in Mohammed
  Patel's Activity inbox."; "With Mohammed Patel since 2026-11-02 20:30 SAST.". Activity id 25. No Stub email was
  written.
Gap: none

### Step 6.17 — Dr Kruger pauses PAED-012
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
Note: D48, T158, T196. The College's CollegeAdmin or an Administrator deactivates a national EPA; a local EPA is its
  institution's. Unticking Active and saving asks the same question.
Actual (2026-11-02, T392 replay, wombat_scenario_t392): PAED-012 is /admin/epas/13 (found by its row's "Edit PAED-012
  — …"). Deactivate… opened "Deactivate PAED-012?": "It can no longer be chosen for a new activity, and it stops being
  a target on running programmes' progress pages and dashboards, for every institution that has adopted it. A
  programme that has ended keeps it on its record, marked no longer in use. The progress it has earned is kept. An
  activity completed against it while it is inactive counts towards nothing until it is reactivated. You can
  reactivate it on this page.", Cancel and Deactivate, the focus on Cancel; Escape closed it and returned the focus to
  Deactivate… (button#epa-deactivate). Reopened (focus on Cancel again) and confirmed: "EPA deactivated." takes the
  focus, Active unticked, Deactivate… gone (Save left). The EPA list reads PAED-012 Inactive. 11.1's items mark it
  "PAED-012 — … (inactive: not in force)" under "PAED-012 is inactive, so its item is not in force: no trainee can
  choose it for a new activity, its credit is paused, and no running programme's progress page or dashboard shows it
  (a programme that has ended keeps it on its record). Reactivating an EPA on its own page brings its item back, and
  credits what was completed against it meanwhile."
Gap: none

### Step 6.18 — Dr Patel completes the Mini-CEX during the pause
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
Note: Only the nominated assessor completes it (`field:assessor_user_id`). T231.
Actual (2026-11-02, T392 replay, wombat_scenario_t392): Dr Patel's inbox reads "2 waiting, 1 overdue": du Plessis's
  Portfolio and Logbook Review (Paediatrics) · PAED-015 · 2026-11-01 first, Overdue, 8 days (since 2026-10-25), then
  "Mini-CEX (Paediatrics) · PAED-012 · 2026-11-02", from Anele Dlamini. The inbox's EPA cell, About and the Request
  section read "PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams (no longer in
  use)". Rated 4 on the rung picker (1, 2, 3a, 3b, 4, 5); Complete: "Completed. 1 more waits for you.", with the
  review's row, Open the next and Back to Activity inbox; nothing left editable. "Done. You completed it on 2026-11-02
  20:31 SAST." and "Rated 4. Its credit to PAED-012 waits while the EPA is paused."; About's Credit None with "This
  activity's EPA is paused: its credit waits."; the Complete row's Credit None.
Gap: none

### Step 6.19 — Dr Dlamini during the pause
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
Note: T158: progress, pickers and dashboards list only EPAs in force. T231 and T255 mark what was recorded against an
  EPA that is not.
Actual (2026-11-02, T392 replay, wombat_scenario_t392): My progress: "Each semester · 9 EPAs" without PAED-012, "1 of
  9" over "EPAs met this semester" (m − 1). "No longer in use · 1 EPA": "PAED-012 — … (no longer in use)", a link to
  /portfolio/progress/13, the mark a span.paused-mark at weight 400, beside "Paused by the College. It is not a target
  while it is paused. Its ratings and the credit it had earned are kept, and what is completed on it meanwhile is
  credited if it is restored." Its page: h1 and tab "PAED-012 — … (no longer in use)" (muted mark, weight 400), the
  same sentence as an alert, no count under Observations; Entrustment "While PAED-012 is paused it is not in your
  standing against Annexure A."; "Rating trajectory (no longer in use)" charts the 2026-11-02 rating at 4; Activities
  on this EPA lists the Mini-CEX with "PAED-012 (no longer in use)", Completed, None. My activities: the Mini-CEX with
  "PAED-012 (no longer in use)", Credit "None" as text (the row's only link is the activity). Its status card: "Rated
  4. Its credit to PAED-012 waits while the EPA is paused.", no count; "Open My progress" (aria "Open My progress at
  PAED-012") landed on /portfolio/progress/13. The Mini-CEX picker offers eight: PAED-001 to 004, 006, 007, 008 and
  013. Left without saving.
Gap: none

### Step 6.20 — Dr Mahlangu's picker leaves PAED-012 out too
Role: Trainee — Dr Nomsa Mahlangu
Route: /activities/new → /activities/new?type=mini_cex_cpsa
Do: Start a Mini-CEX (Paediatrics) and open its EPA picker. Leave without saving.
Expect: Eight EPAs: PAED-001 to PAED-004, PAED-006 to PAED-008, and PAED-013. PAED-012 is not offered.
Note: The pause is national. It reaches every registrar on every curriculum that holds PAED-012.
Actual (2026-11-02, T392 replay, wombat_scenario_t392): Dr Mahlangu's Mini-CEX picker offers eight: PAED-001 to 004,
  006 to 008, and 013. Left without saving.
Gap: none

### Step 6.21 — Dr Molefe's record during the pause
Role: Former trainee (no role; a trainee record) — Dr Lerato Molefe
Route: /portfolio/progress → /portfolio/progress/{EpaId:int}
Do: Read her record. Then open PAED-012's page from its card.
Expect: The record opens with its notice that she completed her programme and that the page is read-only, under the
  line "Your record:" with My authorisations and My activities (T379, as in Step 5.20).
  - Her record reads the list her programme ended with (T312), so the pause changes nothing on it but a mark: PAED-012
    keeps its card, headed "PAED-012 (no longer in use)" (the code a link, the mark muted beside it, T255), with every
    period as Act 5 left it.
  - Her "Entrustment against Annexure A" panel keeps the PAED-012 row, marked "(no longer in use)", and reads as in Step
    5.20: "15 at or above · 0 below · 0 with no decision, of 15 EPAs", the exit rule 15 of 15. Its opening says
    "STAR decisions are shown as the programme left them, and ratings as they stand today." (T312's wording).
  - No chart is on My progress (T355). PAED-012's page is headed "PAED-012 — … (no longer in use)", with no alert that
    it would be credited if restored: it lists its periods as her record does, and its Entrustment panel shows her
    STAR as Act 5 issued it, with Open My authorisations (T379). Its rating trajectory, headed "Rating trajectory (no
    longer in use)", still charts her ratings on it (T255).
  - PAED-006's card carries the title corrected in Step 6.14. Her other cards and periods are as Act 5 left them.
Note: The TraineeOrFormerTrainee policy admits her without the Trainee role (T252).
Actual (2026-11-02, T392 replay, wombat_scenario_t392): Played during the pause (after Step 6.19). The notice reads
  "You completed your programme on 2026-11-02. This page is your record of it and is read-only: …", and "Your record:"
  with My authorisations and My activities sits under it (y 174, then 273). PAED-012 keeps its card, headed by the
  link "PAED-012" (to /portfolio/progress/13) and "(no longer in use)" in span.paused-mark (weight 400),
  "Communicating with and counselling patients, caregivers and healthcare teams" under it; every period as Act 5 left
  it (Semester 2, 2026 "2 of 3 so far; 2 at the minimum level when observed", then 0 of 3 back to Semester 1, 2023).
  No PAED-016 or KGK-001 card (neither exists yet). PAED-006 reads the corrected title. Standing: "15 at or above · 0
  below · 0 with no decision, of 15 EPAs", exit rule "15 of 15 EPAs at their exit level by STAR decision (level 5: 9
  of 9 · level 4: 6 of 6). Every EPA is at its exit level."; the opening reads "… STAR decisions are shown as the
  programme left them, and ratings as they stand today."; the PAED-012 row is marked "(no longer in use)", STAR 5. No
  chart on My progress (its only svgs are the two link icons). PAED-012's page: h1 "PAED-012 — … (no longer in use)",
  no paused/credited-if-restored alert, periods as her record, Entrustment "5 / At or above / Issued 2026-11-02 / Exit
  level 5 · reached" with "Open My authorisations", "Rating trajectory (no longer in use)" charting 2026-10-26 and
  10-28, both 5 ("2 ratings in the 2026 academic year, from Sarah Botha and Fatima Khumalo."). No sideways scroll at
  1280.
Gap: none

### Step 6.22 — Prof Mbatha sees the pause, and cannot lift it
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/epas → /admin/curricula → /admin/curricula/{Id:int}/items → /admin/entrustment-decisions
Do: Read the EPA list, then 11.1's items. Then open Entrustment decisions, filter by Dr Molefe's name and press Show.
  Revoke nothing.
Expect:
  - **The EPA list.** PAED-012 reads Inactive. No national EPA offers her Edit: each reads "Set by the College."
    instead, because only its College changes one (T211's rule; `UpdateEpa`; T392).
  - **11.1's items.** PAED-012's row is marked "(inactive: not in force)", under the same notice. Every row offers View
    beside "Set by the College." (T392, P2).
  - **Dr Molefe's STARs.** Her fifteen from Act 5 still read Active, beside Act 4's PAED-010, Superseded (the status
    filter starts on All): "… · 16 shown". Each row is headed by her name, its EPA in the next column. The PAED-012 row
    names its EPA "PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams (no longer
    in use)" (T255): a pause leaves a STAR standing.
Note: An InstitutionalAdmin changes only KGK's own EPAs and items.
Actual (2026-11-02, T392 replay, wombat_scenario_t392): Her EPA list, subtitled "The EPAs of the curricula Kgosi Kgari
  Teaching Hospital has adopted. The College's are read only; your institution's own are yours to change.", reads 15
  national rows, PAED-012 Inactive; no Edit anywhere, each row's action cell reads "Set by the College." and its code
  is a link to the record; Create EPA offered. Curricula: "Curricula you train against" holds 11.1 ("15 national · 0
  your own", "Adopted on 2026-11-02", Items only); Open for adoption offers only IM Core (2026.1), with Adopt. 11.1's
  items mark PAED-012 "(inactive: not in force)" under the same notice; all 15 rows offer View ("View PAED-012")
  beside "Set by the College.". Entrustment decisions (from Home, lit), status starts on All; filtered by "Molefe" and
  Show: "16 shown", the trainee a row header (16 th), 15 Active (PAED-010 at 5, review #7, among them) and Act 4's
  PAED-010 at 4, expiring 2026-11-22, Superseded ("Superseded by STAR #18, issued 2026-11-02."). The PAED-012 row
  reads "PAED-012 — … (no longer in use)", 5, Active, review #1. Nothing revoked (15 Revoke buttons, no dialog
  opened).
Gap: none (T291 no longer occurs: no national EPA offers her Edit, and every item offers View)

### Step 6.23 — Dr Kruger restores PAED-012
Role: CollegeAdmin — Dr Anton Kruger
Route: /admin/epas → /admin/epas/{Id:int} → /admin/epas
Do: Open PAED-012, tick Active and save.
Expect: Nothing asks first. The page reads "EPA reactivated. 1 activity completed against it while it was inactive now
  counts towards progress." Deactivate… is offered again (T392, S11). The EPA list reads PAED-012 Active.
Note: D48: reactivating credits the paused completions itself, so no rebuild is needed. The College's CollegeAdmin, or
  an Administrator.
Actual (2026-11-02, T392 replay, wombat_scenario_t392): PAED-012 (/admin/epas/13) read Active unticked, Save only;
  ticked and saved, nothing asked (no dialog): "EPA reactivated. 1 activity completed against it while it was inactive
  now counts towards progress." takes the focus; Deactivate… offered again beside Save; the list reads PAED-012
  Active. By SQL Dlamini now has a PAED-012 2026 S2 row (1 counted, ["25:complete"], updated 20:34), and Molefe's row
  (2 counted, ["15:complete", "16:complete"], updated 18:24, before the pause at about 20:31) is untouched.
Gap: none

### Step 6.24 — Dr Dlamini's credit is counted
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
Note: D48.
Actual (2026-11-02, T392 replay, wombat_scenario_t392): PAED-012's row is back in "Each semester · 10 EPAs": "1 of 3
  this semester" (n + 1), "2 more by 2026-11-30"; no "No longer in use" group; "1 of 10" over "EPAs met this semester"
  (m). Its page: "At the minimum level when observed: 1 of 1", "Last encounter 2026-11-02"; h1, tab and "Rating
  trajectory" carry no mark. My activities: no mark, Credit "1 item" linking /portfolio/progress/13. /activities/25:
  "Rated 4. Credited 1 item to PAED-012. PAED-012: 1 of 3 this semester.", About Credit "1 item", the Complete row's
  Credit "1 item". By SQL Molefe's PAED-012 2026 S2 row (2 counted, activities 15 and 16, last updated 18:24, before
  the pause) is unchanged through the pause and the restore.
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
  - **The form.** "Create EPA", subtitled "An EPA of your institution's own. It applies only to Kgosi Kgari Teaching
    Hospital's trainees." The sub-speciality list offers only Paediatrics / Paediatrics, the discipline KGK has
    adopted, already chosen, with the help "The disciplines your institution has adopted." (T392, R2-C04).
  - **Once saved.** Save opens the EPA's own page by `?created`: "EPA saved." heads it and takes the focus, and the token
    leaves the address (T392, F1, R3-C08). The page is titled "KGK-001 — Running a paediatric outreach clinic at a
    district hospital", its crumb KGK-001, with no Back to EPAs. It is Active, and its help names Kgosi Kgari Teaching
    Hospital's trainees; it offers Cancel, Deactivate… and Save. KGK's own EPA is hers to change (`UpdateEpa`).
  - **The EPA list.** KGK-001 comes first, "Your institution's own EPA" under its title, beside the 15 national EPAs,
    and its Edit is the only one: every national row reads "Set by the College." (R3-C09).
Note: An InstitutionalAdmin creates only KGK's own EPAs (`CreateEpa`); a College or an Administrator creates national
  ones.
Actual (2026-11-02, T392 replay, wombat_scenario_t392): "Create EPA", subtitled "An EPA of your institution's own. It
  applies only to Kgosi Kgari Teaching Hospital's trainees."; the sub-speciality list offers only "Paediatrics /
  Paediatrics", already chosen, with "The disciplines your institution has adopted."; category Core or Elective. Save
  went to /admin/epas/17?created and the page took the token out (the address reads /admin/epas/17): "EPA saved."
  heads it with the focus on it. Tab and h1 "KGK-001 — Running a paediatric outreach clinic at a district hospital",
  the crumb Home › EPAs › KGK-001, no Back to EPAs (its links Home, EPAs, Cancel). Active, its help naming "Kgosi
  Kgari Teaching Hospital's trainees"; Cancel, Deactivate… and Save. A reload says nothing of the save. The EPA list
  has 16 rows: KGK-001 first with "Your institution's own EPA" under its title and Edit, the only Edit (1); each of
  the 15 national rows reads "Set by the College.".
Gap: none (T291 no longer occurs: Edit is offered on KGK-001 alone, and the list marks it as KGK's own)

### Step 6.26 — Prof Mbatha adds KGK's own item to 11.1
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/curricula → /admin/curricula/{Id:int}/items → /admin/curricula/{Id:int}/items/new → /admin/curricula/{Id:int}/items → /admin/curricula
Do: Open 11.1's items, press "Add your institution's own item", and add KGK's own item:
  - EPA: KGK-001;
  - target: 1 per academic year;
  - no decision cadence, decided by the trainee's general panel;
  - the suggested scale, minimum level 3a, and no year-by-year minima;
  - completion window: 12;
  - tools: Direct observation only.
  Then open Curricula again.
Expect:
  - **The curricula list.** It offers her Items, and no Edit and no Create curriculum.
  - **The items page.** "Add your institution's own item" stands beside "Items (15)": KGK-001 is an EPA KGK could add.
  - **The add page.** It is a page of its own (T392, Q14), headed "Add your institution's own item" and subtitled "On
    Paediatric EPA Curriculum (11.1). It applies only to Kgosi Kgari Teaching Hospital's trainees." Its EPA picker
    offers KGK-001 alone, because the College's items hold all 15 national EPAs (T222). The scale is the CPSA ladder,
    "Suggested because every item on this curriculum is pinned to it." Add item stays off until a minimum rung is
    chosen. Cancel stands before Add item.
  - **After the add.** Back on the list, "Curriculum item added." heads it and takes the focus, and the token leaves the
    address (F1, R3-B06). "Items (16)", and the add link gives way to the note that every EPA KGK could add is already
    on the curriculum.
  - **The new row.** Last, after the national rows (decision 3): KGK-001 over "Your institution's own item", "1 per
    academic year" over "No decision cadence", 3a, "—" by training year, Direct observation, with Edit and Remove…
    ("Edit KGK-001", "Remove KGK-001"). The national rows still offer View beside "Set by the College.".
  - **Curricula again.** 11.1's Items read "15 national · 1 your own".
Note: The owning InstitutionalAdmin, and only on a curriculum the institution has adopted (T223).
Actual (2026-11-02, T392 replay, wombat_scenario_t392): Her curricula list shows 11.1 ("15 national · 0 your own",
  "Adopted on 2026-11-02") with Items only; no Edit, no Create curriculum (Open for adoption offers IM Core with
  Adopt). 11.1's items read "Items (15)" with "Add your institution's own item" beside it. It opened
  /admin/curricula/2/items/new, h1 "Add your institution's own item", subtitled "On Paediatric EPA Curriculum (11.1).
  It applies only to Kgosi Kgari Teaching Hospital's trainees."; the picker offers KGK-001 alone (chosen); the scale
  is the CPSA ladder, "Suggested because every item on this curriculum is pinned to it."; Add item was aria-disabled
  ("Adding is off until every minimum is a rung on CPSA Paediatric Entrustment Scale v11.1.") until 3a was chosen;
  Cancel before Add item. Added with the Do's values, it went to …/items?added=17, the page took the token out
  (…/items), "Curriculum item added." heads it with the focus; "Items (16)", and the add link gave way to "Every EPA
  your institution could add is already on this curriculum: each national EPA of Paediatrics, and each of your
  institution's own EPAs of Paediatrics. …". KGK-001's row is last: "Your institution's own item", "1 per academic
  year" over "No decision cadence", 3a (CPSA ladder), "—", Direct observation, Edit and Remove… ("Edit KGK-001",
  "Remove KGK-001"); the 15 national rows offer View beside "Set by the College.". Remove… (not confirmed) asks
  "Remove KGK-001 from Paediatric EPA Curriculum (11.1)?" with Cancel focused; Escape returned the focus to Remove…. A
  reload says nothing of the add. Curricula again: 11.1's Items "15 national · 1 your own".
Gap: none

### Step 6.27 — Dr Dlamini is measured against KGK-001
Role: Trainee — Dr Anele Dlamini
Route: /portfolio/progress → /portfolio/progress/{EpaId:int} → /activities/new →
  /activities/new?type=direct_observation_cpsa
Do: Read her progress, then KGK-001's page from its row. Then start a Direct Observation (Paediatrics) and open its EPA
  picker. Leave without saving.
Expect: A KGK-001 row appears first in "Once a year · 6 EPAs": "KGK-001 — Running a paediatric outreach clinic at a
  district hospital", with the badge "Kgosi Kgari Teaching Hospital's own", "0 of 1 in 2026" over "1 more by
  2026-11-30", and a dash where the other rows say when a committee decides them. This period reads "0 of 6" over "EPAs
  met in 2026", one EPA more. KGK-001's page reads "1 a year · Not in the College's exit rule" under its h1, and "2025
  academic year: 0 of 1, 1 short": a target is read live, into periods that have already closed, as the items page
  warned. Its level reads "Minimum 3a", and Entrustment "No STAR yet." and "Not in the College's exit rule." The Direct
  Observation picker offers ten EPAs, KGK-001 among them.
Note: A trainee reads only their own progress. An institution's own item measures only that institution's registrars.
Actual (2026-11-02, T392 replay, wombat_scenario_t392): KGK-001 leads "Once a year · 6 EPAs": "KGK-001 — Running a
  paediatric outreach clinic at a district hospital", badge "Kgosi Kgari Teaching Hospital's own", "0 of 1 in 2026"
  over "1 more by 2026-11-30", and a dash ("— No decision cadence") under Committee decides. This period "0 of 6" over
  "EPAs met in 2026" (was 5); the semester figure still "1 of 10". Its link opened /portfolio/progress/17: "1 a year ·
  Not in the College's exit rule" under the h1 (badge beside it), "0 of 1 in 2026", "2025 academic year: 0 of 1, 1
  short", "Minimum 3a", Entrustment "No STAR yet." and "Not in the College's exit rule.". The Direct Observation
  (Paediatrics) picker offers ten EPAs, KGK-001 first. Left unsaved.
Gap: none

## Phase 6.F — A new curriculum version

A curriculum's targets are read live, so the College puts a new EPA and a changed target into a new version rather than
into the one registrars are measured against. There is no draft state. A new or cloned version starts held back (Active
unticked), so no institution can adopt it until the College ticks Active (T392, F5, T301).

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
Actual (2026-11-02, T392 replay, wombat_scenario_t392): Create EPA offers "Paediatrics / Neonatology" (chosen first)
  and "Paediatrics / Paediatrics"; category Core or Elective. Saved in Paediatrics / Paediatrics, it opened
  /admin/epas/18, "PAED-016 — Transitioning adolescents with long-term conditions to adult care", "EPA saved."
  focused, Active, Deactivate… and Save. His EPA list holds 16 rows, PAED-001 to PAED-016, no KGK-001. 11.1's add page
  would offer PAED-016 (Step 6.29: Add item stands on 11.1): it is on no curriculum.
Gap: none

### Step 6.29 — Dr Kruger clones 11.1 as 11.2, and holds it back
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
Note: The College's CollegeAdmin, or an Administrator. A clone copies every cell of the College's items: target,
  period, minima, scale pin, tools and decision. It copies none of an institution's own items
  (`Curriculum.CloneAsNewVersion`).
Actual (2026-11-02, T392 replay, wombat_scenario_t392): 11.1's items show him 15 rows and no KGK-001, "Items (15)"
  with "Add item" beside it (to /admin/curricula/2/items/new). On 11.1's page the clone card starts at "11.1-copy"
  (from 2026-01-02). Cloned as 11.2 from 2026-11-02 (D), no end: /admin/curricula/4, "Edit curriculum", version 11.2,
  Active unticked (the focus on the h1). The list reads Paediatric EPA Curriculum (11.2), 2026-11-02, 15 items, "Held
  back", beside 11.1 "Open" and Neonatology 1.0 "Held back". While held back, Prof Mbatha's Open for adoption did not
  offer 11.2 (checked after Step 6.30: IM Core only).
Gap: none (T301 no longer occurs: the clone starts held back)

### Step 6.30 — Dr Kruger edits 11.2's items
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
Note: The College's CollegeAdmin, or an Administrator.
Actual (2026-11-02, T392 replay, wombat_scenario_t392): 11.2's items ("College of Paediatricians of South Africa ·
  Paediatrics / Paediatrics · effective 2026-11-02") hold 15 rows identical to 11.1's (row text compared, equal),
  "Items (15)" with "Add item". Add item opened /admin/curricula/4/items/new, h1 "Add item", subtitled "On Paediatric
  EPA Curriculum (11.2). It applies to every institution that has adopted it."; the picker offers PAED-016 alone,
  chosen; the CPSA ladder suggested; Add item aria-disabled until a rung was chosen; Cancel before Add item. Added
  with the Do's values: …/items?added=33, the token taken out (…/items), "Curriculum item added." at the top with the
  focus; "Items (16)", PAED-016 last ("1 per academic year", "Decided each academic year", 4, 2 / 3a / 3b / 4, CBD,
  Mini-CEX, MSF, Edit and Remove…), and in place of the add link "Every national EPA of Paediatrics is already on this
  curriculum, as a national item or as an institution's own item, so there is none left to add. …". Edit PAED-011
  opened /admin/curricula/4/items/28, "Edit PAED-011 — Managing population health challenges", the same subtitle, its
  EPA a select offering PAED-011 alone; target 2 and Save item: …/items?saved=28, then …/items, "Curriculum item
  saved." at the top, focused; the row reads "2 per academic year". A reload after each says nothing of the result. No
  sideways scroll at 1280. 11.1's PAED-011 still reads 1 (outcome SQL).
Gap: none

### Step 6.31 — Dr Kruger publishes 11.2
Role: CollegeAdmin — Dr Anton Kruger
Route: /admin/curricula → /admin/curricula/{Id:int} → /admin/curricula
Do: Open 11.2, tick Active and save.
Expect: The save is confirmed, and this is the only opening: a version starts held back (T392, F5). The curricula
  list shows 11.1 and 11.2 Open, with 11.2 at 16 items, and the Neonatology curriculum still Held back (T9).
Note: The College's CollegeAdmin, or an Administrator.
Actual (2026-11-02, T392 replay, wombat_scenario_t392): 11.2 (/admin/curricula/4) read Active unticked; ticked and
  Save: "Curriculum saved." (focused), nothing asked. The list reads Neonatology EPA Curriculum (1.0) 0 items "Held
  back", Paediatric EPA Curriculum (11.2) 16 items "Open" (from 2026-11-02) and 11.1 15 items "Open".
Gap: none

## Phase 6.G — KGK takes up the new version

### Step 6.32 — Prof Mbatha adopts 11.2
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/curricula
Do: Open Curricula. Under Open for adoption, press Adopt… on Paediatric EPA Curriculum (11.2), read the question, and
  confirm with Adopt 11.2.
Expect:
  - **Open for adoption** offers 11.2, over "Newer than the version your institution has adopted (11.1).", with Adopt…,
    and the Demo IM Core curriculum, with Adopt. It does not offer the held-back Neonatology curriculum, nor 11.1, which
    KGK has adopted (T392, Q7).
  - **The question.** Adopt… asks first (Q6, R3-A03): "Adopt Paediatric EPA Curriculum (11.2) for Kgosi Kgari Teaching
    Hospital?", with "It supersedes Paediatric EPA Curriculum (11.1) for every new admission to Paediatrics /
    Paediatrics at Kgosi Kgari Teaching Hospital.", "The 3 registrars on 11.1 stay on it until each is moved on their
    profile." and "Your institution's own item on 11.1, KGK-001, is not carried over: add it again on 11.2." Cancel
    takes the focus first; the confirm is Adopt 11.2.
  - **The result.** "Paediatric EPA Curriculum (11.2) adopted. New admissions to Paediatrics / Paediatrics join it.
    Paediatric EPA Curriculum (11.1) is superseded; its 3 registrars stay on it until each is moved." heads the page,
    its first sentence bold, and takes the focus (R3-A05).
  - **Curricula you train against** reads 11.2 "Adopted on `D`" and 11.1 "Superseded on `D`" over "3 registrars still
    on it": the registrars already on 11.1 stay on it. Open for adoption offers 11.1 again, over "Older than the version
    your institution has adopted (11.2).", with Adopt… (Q21).
Note: An InstitutionalAdmin for her own institution, or an Administrator for any institution (T091).
Actual (2026-11-02, T392 replay, wombat_scenario_t392): Curricula: "Curricula you train against" held 11.1 ("15
  national · 1 your own", "Adopted on 2026-11-02"); Open for adoption offered Paediatric EPA Curriculum (11.2) over
  "Newer than the version your institution has adopted (11.1).", with Adopt… ("Adopt Paediatric EPA Curriculum
  (11.2)…"), and IM Core Curriculum (2026.1) with Adopt; not the held-back Neonatology curriculum, nor 11.1. Adopt…
  asked "Adopt Paediatric EPA Curriculum (11.2) for Kgosi Kgari Teaching Hospital?", with "It supersedes Paediatric
  EPA Curriculum (11.1) for every new admission to Paediatrics / Paediatrics at Kgosi Kgari Teaching Hospital.", "The
  3 registrars on 11.1 stay on it until each is moved on their profile." and "Your institution's own item on 11.1,
  KGK-001, is not carried over: add it again on 11.2.", Cancel and Adopt 11.2, the focus on Cancel; Escape returned
  the focus to Adopt… (button#adopt-4). Reopened and confirmed with Adopt 11.2: "Paediatric EPA Curriculum (11.2)
  adopted. New admissions to Paediatrics / Paediatrics join it. Paediatric EPA Curriculum (11.1) is superseded; its 3
  registrars stay on it until each is moved." heads the page, its first sentence in strong, with the focus. Curricula
  you train against reads 11.2 "Adopted on 2026-11-02" ("16 national · 0 your own") and 11.1 "Superseded on
  2026-11-02" over "3 registrars still on it"; Open for adoption offers 11.1 over "Older than the version your
  institution has adopted (11.2).", with Adopt…, and IM Core.
Gap: none

### Step 6.33 — Prof Mbatha adds KGK's own item to 11.2
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/curricula → /admin/curricula/{Id:int}/items → /admin/curricula/{Id:int}/items/new → /admin/curricula/{Id:int}/items → /admin/curricula
Do: Read the curricula list. Open 11.2's items, and add KGK's own item on KGK-001 with the values of Step 6.26. Then
  read the curricula list again.
Expect: In the Do's order (T392, S20):
  - **The list.** It shows 11.2, Items "16 national · 0 your own", "Adopted on `D`", and still 11.1, "15 national · 1
    your own", "Superseded on `D`" over "3 registrars still on it" (T211, T223, R3-A05). Both offer Items only.
  - **11.2's items.** Subtitled "College of Paediatricians of South Africa · Paediatrics / Paediatrics · effective `D` ·
    adopted by your institution on `D`", it holds the College's 16 rows, PAED-016 and PAED-011's "2 per academic year"
    among them, each with View beside "Set by the College.". It has no KGK item, and says why: "A new version carries
    no institution's own items. Your item on 11.1 stays there." "Add your institution's own item" stands beside "Items
    (16)" (R3-B07).
  - **The add.** The picker offers KGK-001 alone. Back on the list, "Curriculum item added." takes the focus; KGK-001's
    row, last, reads "Your institution's own item", under "Items (17)", and the new-version line is gone.
  - **The list again.** 11.2's Items read "16 national · 1 your own"; 11.1 still reads "3 registrars still on it"
    (R3-A06a).
Note: The owning InstitutionalAdmin. An institution adds its own items again to each version it adopts (T091).
Actual (2026-11-02, T392 replay, wombat_scenario_t392): The list shows 11.2, Items "16 national · 0 your own",
  "Adopted on 2026-11-02", and 11.1, "15 national · 1 your own", "Superseded on 2026-11-02" over "3 registrars still
  on it", each with Items only. 11.2's items, subtitled "College of Paediatricians of South Africa · Paediatrics /
  Paediatrics · effective 2026-11-02 · adopted by your institution on 2026-11-02", hold the College's 16 rows, each
  with View beside "Set by the College.", PAED-016 and PAED-011 at "2 per academic year" among them, no KGK row, and
  "A new version carries no institution's own items. Your item on 11.1 stays there." under "Items (16)" with "Add your
  institution's own item" beside it. The add page ("On Paediatric EPA Curriculum (11.2). It applies only to Kgosi
  Kgari Teaching Hospital's trainees.") offers KGK-001 alone. With Step 6.26's values: …/items?added=34, the token
  taken out, "Curriculum item added." focused; KGK-001's row last, "Your institution's own item", under "Items (17)",
  and the new-version line gone. A reload says nothing of the add. The list again: 11.2 "16 national · 1 your own";
  11.1 still "3 registrars still on it".
Gap: none

### Step 6.34 — Prof Mbatha saves Dr Dlamini's profile while she stays on 11.1
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/trainees → /admin/trainees/edit
Do: Open Dr Dlamini's profile from Current trainees (Edit), which is on Paediatric EPA Curriculum (11.1). Move the expected completion date six
  months later than it reads, leave the curriculum as it is, and save the profile. Then reopen the profile.
Expect:
  - The profile is "Anele Dlamini's programme", with "Export Anele Dlamini's portfolio" in its header, End this
    programme below Profile details, and no Back to trainees (T379).
  - Trainees' "Completed and closed", "2 ended", lists the latest end first: Lerato Molefe, Completed (Act 5's `D`),
    then Pieter du Plessis, Withdrawn (Act 5's `D−2`), each with View.
  - The curriculum picker offers 11.1 and 11.2, with 11.1 chosen. Under it: "Anele Dlamini is on Paediatric EPA
    Curriculum (11.1), which your institution superseded on `D`. Keeping it keeps Anele Dlamini on 11.1." (T392,
    R2-D01).
  - Save profile asks nothing, and the save reads "Trainee profile saved."
  - Reopened, the profile shows the new date on 11.1. By SQL, profile 3 stays on curriculum 2 and adoption 1: she stays
    on the version she was admitted to (`CurriculumAdminScope`, T211).
Note: An InstitutionalAdmin, or an Administrator. Only a move is judged against KGK's active adoption: an unchanged
  curriculum keeps its pinned adoption, superseded or not (`UpdateTraineeProfile`, T304).
Actual (2026-11-02, T392 replay, wombat_scenario_t392): Trainees (lit) reads "Waiting to be admitted" ("Nobody is
  waiting to be admitted."), "Current trainees" ("3 current": Dlamini, Mahlangu, Ndlovu, each on Paediatric EPA
  Curriculum (11.1), with Programme start, Training year, Expected completion and Edit) and "Completed and closed" ("2
  ended", "Programmes that have ended, the latest end first. An ended record is read-only."): Lerato Molefe
  "2023-01-15 to 2026-11-02", "Completed 2026-11-02", then Pieter du Plessis "2025-01-15 to 2026-10-31", "Withdrawn
  2026-10-31", each with View. Edit on Anele Dlamini opened /admin/trainees/edit?id=3, "Anele Dlamini's programme"
  (tab and h1), "Export Anele Dlamini's portfolio" in the header, Profile details, then "End this programme" below;
  its links Home, Trainees, Export and Cancel only: no Back to trainees. The picker offers "Paediatric EPA Curriculum
  (11.1)" and "(11.2)", 11.1 chosen, under it "Anele Dlamini is on Paediatric EPA Curriculum (11.1), which your
  institution superseded on 2026-11-02. Keeping it keeps Anele Dlamini on 11.1."; start 2024-01-15, expected
  completion 2028-01-14. Moved to 2028-07-14, Save profile (so named) asked nothing: "Trainee profile saved."
  (focused). Reopened, it reads 2028-07-14 on 11.1. By SQL profile 3 is on curriculum 2, adoption 1,
  ExpectedCompletionDate 2028-07-14.
Gap: none

### Step 6.35 — Dr Ndlovu notes his figures before he moves
Role: Trainee — Dr Sipho Ndlovu
Route: /portfolio/progress
Do: Read the page, and note each row's count for its current period.
Expect: Sixteen rows in Your EPAs, "Each semester · 10 EPAs" and "Once a year · 6 EPAs": the 15 national EPAs of 11.1,
  and KGK-001 among the six. PAED-016 is not among them.
Note: Count his tallies too, as `t`: `SELECT count(*) FROM "CurriculumItemProgresses" p JOIN "CurriculumItems" i ON
  i."Id" = p."CurriculumItemId" JOIN "Curricula" c ON c."Id" = i."CurriculumId" JOIN "AspNetUsers" u ON u."Id" =
  p."TraineeUserId" WHERE u."Email" = 'ndlovu@kgk.wombat.local' AND c."Version" = '11.1';`
Actual (2026-11-02, T392 replay, wombat_scenario_t392): Played before the move this time. "Each semester · 10 EPAs"
  (PAED-001 to 007, 010, 012, 015) and "Once a year · 6 EPAs" (KGK-001 first, with its badge, then PAED-008, 009, 011,
  013, 014): sixteen rows, the 15 national EPAs of 11.1 and KGK-001; no PAED-016. Current-period counts: PAED-002 "1
  of 3 this semester"; every other row 0 (each semester row "0 of n this semester", each yearly row "0 of 1 in 2026").
  This period "0 of 10" over "EPAs met this semester", "0 of 6" over "EPAs met in 2026". The Note's query gives t = 1.
Gap: none

### Step 6.36 — Prof Mbatha moves Dr Ndlovu to 11.2
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/trainees → /admin/trainees/edit
Do: Open Dr Ndlovu's profile from Current trainees (Edit). Choose Paediatric EPA Curriculum (11.2), keep his programme start (`J`, from Act 2) and
  his expected completion as they read, and press Save profile and move…; read the question, and confirm with Move
  and save.
Expect: The profile is "Sipho Ndlovu's programme", with "Export Sipho Ndlovu's portfolio" in its header, End this
  programme below Profile details, and no Back to trainees (T379).
  - **The line.** With 11.2 chosen, the line under the picker reads "When you save, Sipho Ndlovu moves to Paediatric
    EPA Curriculum (11.2). Sipho Ndlovu's completed activities are counted again against 11.2 in the same save, so
    Sipho Ndlovu's progress page reads 11.2's targets at once. Sipho Ndlovu cannot be moved back to 11.1 while 11.2 is
    the version your institution has adopted.", and the save reads "Save profile and move…" (T392, Q8, R2-D02).
  - **The question.** It asks first: "Move Sipho Ndlovu to Paediatric EPA Curriculum (11.2)?", with "Sipho Ndlovu's
    completed activities are counted again against 11.2 as the profile saves, and from then on they credit 11.2's
    items." and "Sipho Ndlovu cannot be moved back to 11.1 while 11.2 is the version your institution has adopted."
    Cancel takes the focus first; the confirm is Move and save (R3-D03).
  - **The save** reads "Trainee profile saved. N completion(s) was/were checked against 11.2, and M count(s) towards
    it.", its first sentence bold. The profile reads 11.2. By SQL his PAED-002 tally is on 11.2 and none is left on
    11.1. Reopened, the picker offers 11.2 alone, under "The version your institution has adopted for Paediatrics /
    Paediatrics." From now on, his completions credit 11.2's items.
Note: An InstitutionalAdmin, or an Administrator, and only into the version KGK has adopted (`TraineeAdoptionResolver`).
  The move runs under the trainee credit lock, and replays his credit against 11.2 in the same save (T304).
Actual (2026-11-02, T392 replay, wombat_scenario_t392): From Current trainees, Edit on Sipho Ndlovu opened
  /admin/trainees/edit?id=6, "Sipho Ndlovu's programme", with "Export Sipho Ndlovu's portfolio" in the header, End
  this programme below Profile details, and Home, Trainees, Export and Cancel as its only links (no Back to trainees);
  11.1, start 2026-01-15 (J), expected completion 2030-01-14. With 11.2 chosen the line under the picker reads "When
  you save, Sipho Ndlovu moves to Paediatric EPA Curriculum (11.2). Sipho Ndlovu's completed activities are counted
  again against 11.2 in the same save, so Sipho Ndlovu's progress page reads 11.2's targets at once. Sipho Ndlovu
  cannot be moved back to 11.1 while 11.2 is the version your institution has adopted.", and the submit reads "Save
  profile and move…". It asked "Move Sipho Ndlovu to Paediatric EPA Curriculum (11.2)?", "Sipho Ndlovu's completed
  activities are counted again against 11.2 as the profile saves, and from then on they credit 11.2's items." and
  "Sipho Ndlovu cannot be moved back to 11.1 while 11.2 is the version your institution has adopted.", Cancel and Move
  and save, the focus on Cancel; Escape returned the focus to Save profile and move… (11.2 still chosen). Reopened and
  Move and save: "Trainee profile saved. 1 completion was checked against 11.2, and 1 counts towards it." (first
  sentence in strong, focused); the select reads 11.2. Reopened, the picker offers "Paediatric EPA Curriculum (11.2)"
  alone, under "The version your institution has adopted for Paediatrics / Paediatrics.". By SQL profile 6 is on
  curriculum 4, adoption 2, dates unchanged; his one tally (PAED-002) is on 11.2, none left on 11.1.
Gap: none (T304's remainder no longer occurs: the move asks first)

### Step 6.37 — Dr Ndlovu on 11.2, straight after the move
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
Note: The move replays his credit in its own save (Step 6.36), so no rebuild is needed (T304).
Actual (2026-11-02, T392 replay, wombat_scenario_t392): Seventeen rows: "Each semester · 10 EPAs" and "Once a year · 7
  EPAs" (KGK-001, PAED-008, 009, 011, 013, 014, 016). Nothing notes the move or marks a row new; KGK-001's "Kgosi
  Kgari Teaching Hospital's own" is the only mark. PAED-002 reads "1 of 3 this semester" at once, as in Step 6.35;
  PAED-011 "0 of 2 in 2026", PAED-016 and KGK-001 "0 of 1 in 2026", every other 0. "0 of 10" this semester, "0 of 7"
  in 2026, "0 at or above · 0 below · 17 with no decision, of 17 EPAs". The Mini-CEX picker offers ten EPAs (PAED-001
  to 004, 006 to 008, 012, 013, 016), PAED-016 among them. Left unsaved. Read before the rebuild (Step 6.38).
Gap: none

### Step 6.38 — devadmin rebuilds curriculum progress
Role: Administrator — devadmin
Route: /admin/curricula → /admin/curriculum-progress
Do: From Curricula in the menu, open Curriculum progress from the header and read it. Press Rebuild progress, and
  confirm.
Expect:
  - **The way in.** Curricula offers the Administrator an outline "Curriculum progress" in its header, where Home's
    Maintenance card used to link it (T335, flow 01), over its two sections, All curricula (each version's Status Open
    or Held back) and An institution's curricula (T392, Q18). On the page Curricula stays lit, and the trail reads Home
    › Curricula › Curriculum progress.
  - **The page.** It says that a rebuild credits every completed activity again against today's curriculum, judges only
    whether an EPA was active as of each completion, and runs as one transaction.
  - **The question.** The button asks first: "Rebuild curriculum progress?".
  - **The result.** "Curriculum progress was rebuilt.", in bold, takes the focus, with five figures: activities
    re-read, curriculum items credited, semester tallies written, stale tallies removed, and completions re-stamped.
    Stale tallies removed is 0: the move left nothing stale (T304).
Note: Administrator only (the page's policy, and `RebuildCurriculumProgressCommand`). It is safe to run more than once.
Actual (2026-11-02, T392 replay, wombat_scenario_t392): Curricula (from the menu), subtitled "Every College's
  curricula; and, for one institution, what it trains against and may adopt.", offers an outline "Curriculum progress"
  (btn btn-outline) beside Create curriculum, over "All curricula" (Neonatology 1.0 "Held back", 11.2 17 items "Open",
  11.1 16 "Open", IM Core "Open") and "An institution's curricula" (an Institution select and Show). It opened
  /admin/curriculum-progress, "Curriculum progress · Wombat", the trail Home › Curricula › Curriculum progress,
  Curricula the lit item in the main menu. The page says a rebuild "throws the stored tallies away and credits every
  completed activity again", "credits each activity against the curriculum as it is today", "Only whether an EPA was
  active is judged as of when the activity was completed", and "It runs as one transaction, so if it fails, nothing
  changes". Rebuild progress asked "Rebuild curriculum progress?" ("Every trainee's curriculum progress is
  recalculated from their completed activities, … if it fails nothing changes."), the focus on Cancel; Escape returned
  the focus to Rebuild progress. Reopened and confirmed: "Curriculum progress was rebuilt." in strong, with the focus,
  and Activities re-read 15, Curriculum items credited 14, Semester tallies written 9, Stale tallies removed 0,
  Completions re-stamped 15. By SQL Dlamini's and Molefe's PAED-012 rows keep their counts and activities (rewritten
  at 20:43, same content); Ndlovu's PAED-002 tally stays on 11.2.
Gap: none

### Step 6.39 — Dr Ndlovu's evidence counts on 11.2
Role: Trainee — Dr Sipho Ndlovu
Route: /portfolio/progress
Do: Read the page.
Expect: Each of the 15 EPAs carried over from 11.1 reads the count he noted in Step 6.35 for its current period.
  PAED-016 and KGK-001 read "0 of 1 in 2026". PAED-011's row reads "0 of 2 in 2026": it measures its count against a
  target of 2 per academic year. The rows are "Each semester · 10 EPAs" and "Once a year · 7 EPAs", and This period's
  yearly figure is "… of 7" over "EPAs met in 2026".
Note: A trainee reads only their own progress.
Actual (2026-11-02, T392 replay, wombat_scenario_t392): The page reads exactly as in Step 6.37 (text identical, diff
  empty): for the 15 EPAs carried over, PAED-002 "1 of 3 this semester" and every other 0, the counts noted in Step
  6.35; PAED-016 and KGK-001 "0 of 1 in 2026", PAED-011 "0 of 2 in 2026". "Each semester · 10 EPAs", "Once a year · 7
  EPAs", "0 of 7" over "EPAs met in 2026".
Gap: none

### Step 6.40 — Dr Molefe's record after the rebuild
Role: Former trainee (no role; a trainee record) — Dr Lerato Molefe
Route: /portfolio/progress
Do: Read her record, card by card, against what she read in Act 5 (Step 5.20).
Expect:
  - Her fifteen national cards are unchanged, PAED-012 among them, now without its "(no longer in use)" mark, and each
    period reads what Act 5 recorded: the rebuild replays a completed profile's evidence as it was (Act 5's handoff).
  - PAED-006's card carries the corrected title. There is no PAED-016 card: her profile is on 11.1.
  - There is no KGK-001 card: KGK added its item after she graduated, and her record reads the list her programme
    ended with (T312). Her standing reads "15 at or above · 0 below · 0 with no decision, of 15 EPAs", as in Step
    5.20.
Note: The TraineeOrFormerTrainee policy admits her without the Trainee role (T252).
Actual (2026-11-02, T392 replay, wombat_scenario_t392): Notice "You completed your programme on 2026-11-02. …",
  read-only, "Your record:" under it. Against Step 6.21's text (taken in the pause, diffed) the only differences are
  PAED-012's two "(no longer in use)" marks gone (its card heading and its Entrustment row): 15 cards, PAED-012 among
  them, each period as Act 5 left it. PAED-006 carries the corrected title. No PAED-016 card and no KGK-001 card, no
  paused mark. Standing "15 at or above · 0 below · 0 with no decision, of 15 EPAs"; exit rule "15 of 15 EPAs at their
  exit level by STAR decision (level 5: 9 of 9 · level 4: 6 of 6). Every EPA is at its exit level." By SQL her
  PAED-012 row (2 counted, activities 15 and 16) is unchanged by the pause, the restore and the rebuild (rewritten
  with the same content).
Gap: none

### Step 6.41 — Dr du Plessis's record after the rebuild
Role: Former trainee (no role; a trainee record) — Dr Pieter du Plessis
Route: /portfolio/progress → /activities/mine
Do: Read his record and his activities.
Expect:
  - My progress still opens with the notice that his programme ended on the day Act 5 recorded, "Your programme ended on
    <that day, as `YYYY-MM-DD`>. This page is your record of it and is read-only: …" (T355, D1).
  - PAED-002's period holding that day still reads "no target (your programme ended part-way through)", with the count
    Act 5 Step 5.28 read: the Mini-CEX observed after his last day still counts towards nothing (T281).
  - No KGK-001 card has appeared on his record either, as on Dr Molefe's: his standing still counts the 15 EPAs his
    programme ended with, "… 15 with no decision, of 15 EPAs" (T312).
  - My activities lists that Mini-CEX as Completed, its Credit None. A note row "Observed after your last day in the
    programme, <that day>, so it counts towards nothing." stands under it, under the cancelled DOPS and under the open
    Portfolio and Logbook Review, each observed after his last day, and under no other row (T379, as in Step 5.28).
Note: Withdrawal took his Trainee role away (Act 5, T290); he reads his own record through the trainee record, as Dr
  Molefe does.
Actual (2026-11-02, T392 replay, wombat_scenario_t392): By SQL du Plessis (and Molefe) hold no role; profile 4
  inactive. My progress opens "Your programme ended on 2026-10-31. This page is your record of it and is read-only:
  …", "Your record:" with My activities under it. PAED-002's "Semester 2, 2026" reads "no target (your programme ended
  part-way through) · 0 recorded", as Step 5.28 read. No KGK-001 or PAED-016 card; standing "0 at or above · 0 below ·
  15 with no decision, of 15 EPAs". My activities lists the Mini-CEX ("Mini-CEX (Paediatrics) · PAED-002 ·
  2026-11-01", to David Naidoo) Done, Completed, Credit None, with "Observed after your last day in the programme,
  2026-10-31, so it counts towards nothing." under it, under the cancelled DOPS (Closed, Cancelled, "no date") and
  under the open Portfolio and Logbook Review (2026-11-01, Awaiting review); no note under the Case-Based Discussion
  (2026-10-28, Requested) or the three KGK Teaching Session Logs. By SQL the after-the-end Mini-CEX's completion reads
  CreditedItemCount 0.
Gap: none

## Outcome state

Replay check (2026-11-02, T392 replay, wombat_scenario_t392): match. The 14 queries read as written: CNSA, CPSA and
  DEMO-C active; Adult Neurology on the CNSA scale (6 levels), Neonatology and Paediatrics on the CPSA ladder; 16 of
  16 national CPSA EPAs active; PAED-006 retitled; KGK-001 KGK's only own EPA; Neonatology 1.0 inactive 0/0, 11.1
  active 15/1, 11.2 active 16/1; PAED-011 1 on 11.1 and 2 on 11.2; KGK 11.1 superseded, 11.2 active; Dlamini and
  Mahlangu 11.1 active, du Plessis and Molefe 11.1 inactive, Ndlovu 11.2 active; Ndlovu's tallies 11.2: 1 (t = 1),
  none on 11.1; activity 25 (Step 6.16) credited 1; du Plessis's after-the-end query returns 0; Molefe and du Plessis
  hold no role. cpsa_case_presentation_log is a draft with no version (activity type 24). KGK's Curricula reads 11.2
  "Adopted on 2026-11-02" and 11.1 "Superseded on 2026-11-02" over "2 registrars still on it". Snapshot
  recovery/scenario-t392-post-act6.dump.

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
    | Paediatric EPA Curriculum 11.1 | Open | 15 national, plus KGK's own |
    | Paediatric EPA Curriculum 11.2 | Open | 16 national (PAED-011 at 2 per academic year), plus KGK's own |
    | Neonatology EPA Curriculum 1.0 | Held back | none |

    Status is Curricula's word for the College's Active flag (T392, T9): Open is released for adoption.

  - one activity type of the College's own, `CPSA Case Presentation Log` (`cpsa_case_presentation_log`), scoped to the
    Paediatrics speciality, a draft with no published version (Step 6.14a). The twelve `*_cpsa` instruments are as
    seeded, and none has a draft. (Added with Step 6.14a on 2026-09-26, after the T295 replay, so the
    `scenario-post-act6` snapshot does not hold it.)

**KGK:**
- **Its own EPA:** KGK-001, with an item of KGK's own on each Paediatric version.
- **Adoptions:** 11.2 Active and 11.1 Superseded, as KGK's Curricula says it (T392, R3-A06b): 11.2 "Adopted on `D`"; 11.1
  "Superseded on `D`" over "2 registrars still on it" (Dr Dlamini and Dr Mahlangu; an ended profile is not counted).
- **Registrars:**
  - Dr Ndlovu is on 11.2, and his progress is rebuilt there.
  - Dr Dlamini and Dr Mahlangu stay on 11.1: a save that leaves the curriculum as it is keeps the pinned, superseded
    adoption (Step 6.34).
  - Dr Molefe's completed record and Dr du Plessis's withdrawn one keep what Act 5 recorded, and neither shows KGK-001:
    each reads the list its programme ended with (T312; Steps 6.40 and 6.41).
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
    stay pinned to 11.1's superseded adoption when saved unchanged (Step 6.34);
  - three current trainees (Dr Dlamini, Dr Mahlangu and Dr Ndlovu), a graduate (Dr Molefe) and a withdrawn registrar
    (Dr du Plessis, with no role), as Act 5 handed them on;
  - KGK-001, and KGK's own item on each version;
  - PAED-006's corrected title;
  - one rebuild already run.
- **Personal data.** None of Act 6's catalogue rows belongs to a person, so an erasure in the appendix touches none of
  them.
- **Snapshot.** `pg_dump -Fc` the database to `recovery/scenario-post-act6.dump`.
