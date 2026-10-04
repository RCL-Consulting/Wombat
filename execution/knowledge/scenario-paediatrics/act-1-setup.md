# Act 1 — Day 0: institutional setup

**Scenario date:** Monday 12 January 2026. The date is narrative only: every date a step types or reads is relative to
the replay day `D` (README § The clock).

**Who acts:**
- The Administrator: the platform operator, signed in as `devadmin@wombat.local` on dev (T292). Elsewhere the bootstrap
  Administrator plays these steps.
- Dr Anton Kruger: the CPSA's CollegeAdmin, from Phase 1.C.
- Prof Nolwazi Mbatha: KGK's InstitutionalAdmin, from Phase 1.C.

**Why:** The College owns the catalogue (T091), and `PaediatricCatalogueSeeder` has already written the one it
published, v11.1 (T159). So nobody authors a catalogue here: the College checks it. An institution trains against the
catalogue only after it adopts a curriculum version. Only an Administrator can create an institution, and only an
Administrator can invite the first person to run a College or an institution. After those invitations, the College's
and KGK's own people do everything.

**Starting state:** a fresh database, started as README § How to play says. No earlier act has run. The seeders have
written:
- **The Demo world** (`DataSeeder`): the Demo College, the Demo Institution, their Internal Medicine catalogue, the
  five-level `O-R Scale` and ten generic activity types. It is ignored throughout.
- **The CPSA's catalogue** (`PaediatricCatalogueSeeder`): the College, the Paediatrics speciality and its Paediatrics
  sub-speciality, the six-rung ladder, PAED-001 to PAED-015, curriculum 11.1 with its 15 items, and the twelve `*_cpsa`
  instruments.
- **Eight dev accounts** (`DevUserSeeder`):
  - `trainee@`, `assessor@`, `committee@`, `committee2@`, `coordinator@` and `instadmin@wombat.local`, all at the Demo
    Institution;
  - `collegeadmin@wombat.local`, a CPSA CollegeAdmin that this runbook never uses;
  - `devadmin@wombat.local`, a global Administrator (T292).
- **The bootstrap Administrator**, but only where `AdminSeeder` is configured.

KGK does not exist yet. There are no invitations and no activities.

**Goal:** by the end of Day 0:
1. The Administrator has confirmed the seeded CPSA and its ladder, created KGK, and invited Dr Kruger and Prof Mbatha.
   Both have registered.
2. Dr Kruger has checked the CPSA's catalogue and changed nothing:
   - the Paediatrics speciality and its Paediatrics sub-speciality, which defaults to the ladder;
   - PAED-001 to PAED-015;
   - curriculum 11.1 and its 15 items.
3. KGK has adopted curriculum 11.1, so Act 2 can admit registrars into it.
4. Prof Mbatha has checked the twelve `*_cpsa` instruments and changed none. She has built one KGK type in the builder
   and published it: `KGK Teaching Session Log` (`kgk_teaching_log`).

## Phase 1.A — The Administrator confirms the College and its ladder

The catalogue's College-level records are the Administrator's pages: the Colleges list and the scale editor admit
nobody else.

### Step 1.1 — The Administrator signs in
Role: Administrator — the platform operator (`devadmin@wombat.local`)
Route: /account/login → /
Do: Sign in with `devadmin@wombat.local` and the dev password that `DevUserSeeder` gives it.
Expect: Home is headed "Home", with "Administrator · Semester N, YYYY" under it, naming the current semester, and its
  header offers no action. No switch is offered, in the sidebar or on Home, since devadmin holds one role. The dashboard
  has two cards:
  - System health, which shows the database connection as healthy;
  - Users across institutions, which counts the seeded accounts (8, or 9 where the bootstrap Administrator exists).
  There is no Maintenance card: Curriculum progress is linked from the Curricula page's header (T335).
  The sidebar reads "Acting as Administrator", with no switch, over his menu, grouped: Home; Platform: Scheduled jobs,
  Audit log, SSO mappings, Data rights requests; Organisations: Institutions, Colleges; People: Users, Invitations;
  Catalogue: EPAs, Curricula, Activity types, Entrustment scales; Reviews: Decisions due, Committee reviews, Decision
  panels; then My data rights under a rule (DESIGN.md § The NavMenu). Home is lit. The top bar names him "Demo
  Administrator", with Sign out beside it.
Note: The sign-in page offers no institutional sign-in, because dev configures no SSO provider (appendix).
Actual (2026-10-04, T355 replay, wombat_scenario_t355): As expected: the sign-in page (no SSO button; Show beside
  Password) landed on Home, tab "Home · Wombat", "Administrator · Semester 2, 2026", no header action, no switch;
  System health (Database connection) and Users across institutions 9 (the bootstrap admin exists); no Maintenance
  card. The sidebar reads "Acting as Administrator" over Home; Platform; Organisations; People; Catalogue; Reviews with
  the listed links, then My data rights; Home is lit. The top bar reads "Demo Administrator" with Sign out. System
  health still lists "Email queue (T012)" and "Last nightly job (T024)".
Gap: [F-1.1a, T327] still: System health shows "Email queue (T012)" and "Last nightly job (T024)" with a hard-coded
  warning dot that reads no data, and prints internal task ids to the operator.

### Step 1.2 — The CPSA is among the Colleges
Role: Administrator — the platform operator
Route: /admin/colleges
Do: Open Colleges from the nav and read the list. Create nothing.
Expect: The list has two rows, each Active and each offering Edit and Specialities:
  - `Demo College` (`DEMO-C`);
  - `College of Paediatricians of South Africa` (`CPSA`), described as "Constituent College of the Colleges of Medicine
    of South Africa; owns the national Paediatric EPA catalogue."
  The header offers Create college. No College is named "College of Paediatricians" alone.
Actual (2026-10-04, T355 replay, wombat_scenario_t355): As expected: two rows, CPSA then Demo College (`DEMO-C`), each
  Active with Edit and Specialities named for their row; the CPSA description matches word for word. Create college in
  the header; Colleges lit.
Gap: none

### Step 1.3 — The CPSA's own record
Role: Administrator — the platform operator
Route: /admin/colleges → /admin/colleges/{Id:int}
Do: Open the CPSA's Edit and read the record. Leave by Back to colleges without saving. Record the CPSA's id as
  `{CollegeId}`.
Expect: The page is headed "Edit college", and the browser tab reads "Edit college · Wombat" (T190). Name, Short
  code and Description hold Step 1.2's values, and Active is ticked. Deactivate and Save are offered. Leaving saves
  nothing, and the list is unchanged.
Actual (2026-10-04, T355 replay, wombat_scenario_t355): As expected: /admin/colleges/2, headed "Edit college", tab
  "Edit college · Wombat"; the three fields hold Step 1.2's values, Active ticked; Cancel, Deactivate and Save offered.
  Back to colleges left the list unchanged. `{CollegeId}` = 2.
Gap: none

### Step 1.4 — The CPSA ladder has six rungs
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
Note: Scales are platform records (T057). Only an Administrator opens one. An InstitutionalAdmin reads the list
  (Step 1.22), and a CollegeAdmin is admitted to neither page (Step 1.15).
Actual (2026-10-04, T355 replay, wombat_scenario_t355): As expected: two rows, CPSA ladder 6 levels and O-R Scale 5,
  Create scale, Edit and Delete on each. /admin/entrustment-scales/2 lists Order 1-6 labelled 1, 2, 3a, 3b, 4, 5, from
  "Not entrusted to act…" to "Supervises others…". Cancel returned to the list unsaved. `{ScaleId}` = 2.
Gap: none

### Step 1.5 — Invite Dr Kruger as the CPSA's CollegeAdmin
Role: Administrator — the platform operator
Route: /admin/invitations
Do: Under Issue invitation, enter `kruger@cmsa.wombat.local` and choose the role CollegeAdmin. Press Issue invitation
  before choosing a College. Then choose `College of Paediatricians of South Africa` and issue the invitation.
Expect: Before the issue, Active invitations reads "No active invitations".
  - The Role picker offers the eight roles an invitation can carry, and never Administrator.
  - Choosing CollegeAdmin replaces the Institution picker with a College picker, which offers the Demo College and the
    CPSA. Speciality and Sub-speciality stay disabled.
  - The first press is refused: "A college is required for a college administrator."
  - The second press reads "Invitation issued for kruger@cmsa.wombat.local. Its email is being sent. Copy the link below
    — it is shown only once." (T283's wording). The registration link is shown beneath it, and it expires in 14 days.
  - The new row reads CollegeAdmin, with College `College of Paediatricians of South Africa`, no institution, an expiry
    of `D+14` and a Delivery of "Being sent".
  - The application log holds a stub email "Your Wombat invitation", tagged `invitation, role:CollegeAdmin`. It invites
    the reader to register as CollegeAdmin, with the same link and its expiry date. The log never names the address
    (T282), so the role tells this mail from Mbatha's.
Note: Only an Administrator is offered the CollegeAdmin role (T093). With `Email__SmtpHost` unset, nothing ever reports
  the mail as sent. After an hour the row reads "Not delivered." and offers Resend (T283). That state belongs to
  `states.md`.
Actual (2026-10-04, T355 replay, wombat_scenario_t355): As expected: "No active invitations"; eight roles, no
  Administrator; CollegeAdmin swaps in a College picker (CPSA, Demo College), Speciality and Sub-speciality disabled;
  the first press refused with the quoted text; the second shows T283's wording word for word, the link and "It expires
  in 14 days". Row: CollegeAdmin, CPSA, no institution, expires 2026-10-18 (D+14), "Being sent". The log holds "Stub
  email … (tags: invitation, role:CollegeAdmin)", subject "Your Wombat invitation", inviting the reader to register as
  CollegeAdmin, with the same link (compared) and "This link expires on 2026-10-18"; it never names the address.
Gap: none

## Phase 1.B — The Administrator creates KGK and invites Prof Mbatha

### Step 1.6 — Create Kgosi Kgari Teaching Hospital
Role: Administrator — the platform operator
Route: /admin/institutions → /admin/institutions/new → /admin/institutions/{Id:int}
Do: Open Institutions and read the list. Then press Create institution and enter:
  - Name `Kgosi Kgari Teaching Hospital`;
  - Short code `KGK`;
  - Contact email `paeds-admin@kgk.wombat.local`.
  Save. Record KGK's id as `{InstitutionId}`.
Expect: The list holds one row, the `Demo Institution` (`DEMO`), Active, offering Edit only. The create form asks for
  Name, Short code and Contact email, and has no Status field. Save lands on KGK's own record, headed "Edit
  institution", with the browser tab reading "Edit institution · Wombat" (T190). There, Active is ticked and
  Deactivate is offered. Back on the list, KGK is a second row: Active, and created today.
Note: This step used to be an SQL stand-in (T159). Played through the page, the save now writes KGK's audit row, which
  the stand-in never did.
Actual (2026-10-04, T355 replay, wombat_scenario_t355): As expected: one row, Demo Institution (DEMO), Active, Edit
  only; the form asks Name, Short code, Contact email, no Status. Save landed on /admin/institutions/2, "Edit
  institution", tab "Edit institution · Wombat", Active ticked, Deactivate offered; the list then shows KGK Active,
  created 2026-10-04 13:05. `{InstitutionId}` = 2. The list's subtitle still reads "Maintain institution records and
  drill into their speciality structure." An empty Save (states.md) still reads "The Name field is required." and "The
  ShortCode field is required."
Gap: [F-1.6a, T326] still: the Institutions list's subtitle promises a speciality drill-down that has not existed since
  T091 (each row offers Edit only). [F-1.6b, T324] still: the create form's empty-save message names the code property:
  "The ShortCode field is required." rather than "Short code".

### Step 1.7 — Invite Prof Mbatha as KGK's InstitutionalAdmin
Role: Administrator — the platform operator
Route: /admin/invitations
Do: Invite `mbatha@kgk.wombat.local` as InstitutionalAdmin at `Kgosi Kgari Teaching Hospital`, with no speciality.
Expect: InstitutionalAdmin is the form's default role. The Institution picker offers the Demo Institution and KGK.
  Speciality and Sub-speciality stay disabled for this role. The status line and the link read as in Step 1.5.
  Active invitations now holds two rows: Kruger's, and Mbatha's, which reads InstitutionalAdmin at `Kgosi Kgari Teaching
  Hospital` with no College. The log holds a second stub email, tagged `role:InstitutionalAdmin`, which invites the
  reader to register as InstitutionalAdmin.
Actual (2026-10-04, T355 replay, wombat_scenario_t355): As expected: InstitutionalAdmin preselected; Institution offers
  Demo Institution and KGK; Speciality and Sub-speciality disabled; the same status line and link; the row reads
  InstitutionalAdmin, KGK, no College, expires 2026-10-18, "Being sent", above Kruger's; the log's stub email is tagged
  role:InstitutionalAdmin and invites the reader to register as InstitutionalAdmin, with the page's link (compared).
  Replay slip: the script was run twice, so a second Mbatha invitation was issued (an extra IssueInvitationCommand
  success); the Administrator revoked the older one (id 2) on this page, and Step 1.10 used the newer (id 3).
Gap: none

## Phase 1.C — Dr Kruger and Prof Mbatha register

### Step 1.8 — Dr Kruger registers
Role: Anonymous — Dr Anton Kruger, invited
Route: /account/register → /
Do: In a fresh browser session, open the link from the CollegeAdmin stub email in the application log (the link Step
  1.5 showed). Enter:
  - First name `Anton` and Last name `Kruger`;
  - a password, twice, that meets the policy: at least 12 characters, with an upper-case and a lower-case letter, a
    digit and a symbol.
  Press Register. Write the password to `pwd_DO_NOT_COMMIT.txt`.
Expect: The page reads "Registering kruger@cmsa.wombat.local as CollegeAdmin.", and the address is shown but cannot be
  edited. Register signs Kruger in and lands on Home, with "College admin · Semester N, YYYY" under its heading. The
  dashboard has one card, National catalogue, with Specialities, EPAs and Curricula. The sidebar reads "Acting as College
  admin" over Home, Specialities, EPAs, Curricula and Activity types, then My data rights: the College writes its
  disciplines' activity types in the builder (T300, D52). The top bar names him "Anton Kruger".
Actual (2026-10-04, T355 replay, wombat_scenario_t355): As expected: "Registering kruger@cmsa.wombat.local as
  CollegeAdmin.", Email disabled; both password fields carry a Show toggle and the policy list. Register landed on
  Home, "College admin · Semester 2, 2026", one National catalogue card with Specialities, EPAs and Curricula; the
  sidebar reads "Acting as College admin" over Home, Specialities, EPAs, Curricula, Activity types, then My data
  rights; the top bar reads "Anton Kruger". The password was kept in the replay's own untracked file.
Gap: none

### Step 1.9 — A used link is refused
Role: Anonymous — anyone who holds Kruger's link
Route: /account/register
Do: In another fresh session, open Kruger's link again.
Expect: The page reads "This invitation has already been used." and offers no form. The page refuses up front whatever
  no input could put right (T285).
Actual (2026-10-04, T355 replay, wombat_scenario_t355): As expected: in a fresh session the link shows "Complete
  registration" and "This invitation has already been used.", with no form and no input.
Gap: none

### Step 1.10 — Prof Mbatha registers
Role: Anonymous — Prof Nolwazi Mbatha, invited
Route: /account/register → /
Do: Register as in Step 1.8, from the InstitutionalAdmin stub email in the log, with First name `Nolwazi` and Last name
  `Mbatha`. Write her password to `pwd_DO_NOT_COMMIT.txt`.
Expect: The page reads "Registering mbatha@kgk.wombat.local as InstitutionalAdmin." Register lands her on Home, with
  "Institutional admin · Semester N, YYYY" under its heading and "Invite a person" in its header, which opens the
  invitations page. The dashboard has three cards:
  - Users, which lists InstitutionalAdmin 1;
  - Specialities & sub-specialities, which reads 0 and 0, because KGK has adopted nothing yet;
  - Quick links: Users, Invitations, Curriculum adoptions and Entrustment decisions.
  The sidebar reads "Acting as Institutional admin" over her menu, grouped: Home; People: Invitations, Trainees,
  Assessors, Users; Curriculum: Curriculum adoptions, Curricula, EPAs, Activity types, Entrustment scales; Reviews:
  Decisions due, Committee reviews, Decision panels; Access and audit: SSO mappings, Audit log; then My data rights.
Note: The specialities card is meant to count what the institution has adopted (T291 item 4). Until that fix lands, it
  counts the whole national catalogue: 2 and 2.
Actual (2026-10-04, T355 replay, wombat_scenario_t355): "Registering mbatha@kgk.wombat.local as InstitutionalAdmin."
  Replay slip: a script error registered her with the names "x" and "x"; she corrected them to Nolwazi Mbatha on
  /account/profile ("Name saved."), and Register's own landing page was not observed. Home then reads "Institutional
  admin · Semester 2, 2026", with "Invite a person" (/admin/invitations) in its header. Users lists InstitutionalAdmin
  1; Specialities & sub-specialities reads 2 and 2; Quick links: Users, Invitations, Curriculum adoptions, Entrustment
  decisions. The sidebar reads "Acting as Institutional admin", grouped exactly as the Expect says, then My data
  rights. The password was kept in the replay's own untracked file.
Gap: [F-1.10a, T291] still (T291 item 4): the specialities card counts the national catalogue (2 and 2), not KGK's
  adoptions (0 and 0).

### Step 1.11 — Both invitations are spent
Role: Administrator — the platform operator
Route: /admin/invitations → /
Do: Reload the invitations list, then go Home.
Expect: Active invitations reads "No active invitations", because a used invitation leaves the list. The dashboard's
  count of registered users is two higher than in Step 1.1.
Actual (2026-10-04, T355 replay, wombat_scenario_t355): As expected: Active invitations reads "No active invitations"
  (the revoked duplicate is gone too); Home's Users across institutions reads 11 (9 in Step 1.1).
Gap: none

## Phase 1.D — Dr Kruger checks the national catalogue

The College governs the catalogue, and institutions adopt it. A CollegeAdmin reads and edits only his own College's
records (T091, T093). Kruger saves nothing in this phase.

### Step 1.12 — The Paediatrics speciality
Role: CollegeAdmin — Dr Anton Kruger
Route: /admin/specialities → /admin/colleges/{CollegeId:int}/specialities
Do: Open Specialities from the nav and read the page. Record the Paediatrics speciality's id as `{SpecialityId}`.
Expect: He is redirected to his own College's specialities, subtitled "College: College of Paediatricians of South
  Africa (CPSA)". The page has one row: `Paediatrics`, "Specialist training in Paediatrics.", Active, offering Edit and
  Sub-specialities. The Demo College's speciality is not listed. The header offers Create speciality. It offers no link
  to the Colleges list, which does not admit him (T291 item 2). His menu's Specialities stays lit on it (T331), and the
  trail above the header reads Home › Specialities › College of Paediatricians of South Africa.
Actual (2026-10-04, T355 replay, wombat_scenario_t355): Specialities in the nav redirected to
  /admin/colleges/2/specialities, subtitled "College: College of Paediatricians of South Africa (CPSA)"; one row,
  Paediatrics, "Specialist training in Paediatrics.", Active, Edit and Sub-specialities; no Demo speciality; Create
  speciality. Specialities stays lit, and the trail reads Home › Specialities › College of Paediatricians of South
  Africa. The header still offers "Back to colleges" (/admin/colleges). `{SpecialityId}` = 2.
Gap: [F-1.12a, T291] still (T291 item 2): "Back to colleges" is shown to a CollegeAdmin, whom /admin/colleges does not
  admit.

### Step 1.13 — The Paediatrics sub-speciality
Role: CollegeAdmin — Dr Anton Kruger
Route: /admin/specialities/{SpecialityId:int}/sub-specialities
Do: Open Sub-specialities on the Paediatrics row. Record the sub-speciality's id as `{SubSpecialityId}`.
Expect: The page is subtitled "Speciality: Paediatrics". It has one row: `Paediatrics`, "General paediatric specialist
  training programme.", Active, offering Edit. The header offers Back to specialities and Create sub-speciality. The
  sub-speciality shares its speciality's name, and there is no "General Paediatrics".
Actual (2026-10-04, T355 replay, wombat_scenario_t355): As expected: subtitled "Speciality: Paediatrics"; one row,
  Paediatrics, "General paediatric specialist training programme.", Active, Edit; Back to specialities and Create
  sub-speciality. `{SubSpecialityId}` = 2.
Gap: none

### Step 1.14 — The sub-speciality defaults to the CPSA ladder
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
Actual (2026-10-04, T355 replay, wombat_scenario_t355): As expected: "Edit sub-speciality", tab "Edit sub-speciality ·
  Wombat", "Speciality: Paediatrics"; Default entrustment scale on the CPSA ladder, with "No default — offer every
  scale" and O-R Scale; the help text as quoted; Active ticked; Cancel, Deactivate, Save. Cancel returned to the list
  unsaved.
Gap: none

### Step 1.15 — The College does not open the scale pages
Role: CollegeAdmin — Dr Anton Kruger
Route: /admin/entrustment-scales → /access-denied
Do: Type the address of the entrustment scales list.
Expect: The page reads "You cannot open this page" and "Your role (College admin) does not open this page.", then "If
  you need it for your work, ask the platform administrator.", with Go to Home (T335).
  The scales list admits only an Administrator or an InstitutionalAdmin, and Kruger's menu offers no Entrustment scales.
  He reads the ladder by name where the College uses it: the sub-speciality's default (Step 1.14) and each item's Scale
  column (Step 1.18).
Actual (2026-10-04, T355 replay, wombat_scenario_t355): As expected:
  /access-denied?ReturnUrl=%2Fadmin%2Fentrustment-scales, tab "You cannot open this page · Wombat", reads "You cannot
  open this page", "Your role (College admin) does not open this page.", "If you need it for your work, ask the
  platform administrator.", with Go to Home (/). His menu has no Entrustment scales.
Gap: none

### Step 1.16 — The 15 v11.1 EPAs
Role: CollegeAdmin — Dr Anton Kruger
Route: /admin/epas
Do: Open EPAs and read the list. Create nothing.
Expect: The list has exactly 15 rows, PAED-001 to PAED-015, with the titles in the table below. Every row reads:
  - College `College of Paediatricians of South Africa`;
  - Speciality `Paediatrics` and Sub-speciality `Paediatrics`;
  - Active, with Edit.
  The Demo `EPA-001` is not listed, because a CollegeAdmin reads only his own College's EPAs. The header offers Create
  EPA.
Actual (2026-10-04, T355 replay, wombat_scenario_t355): As expected: exactly 15 rows, PAED-001 to PAED-015 with the
  table's titles word for word, each CPSA, Paediatrics / Paediatrics, Active, Edit; no EPA-001; Create EPA in the
  header.
Gap: none

**The 15 EPAs** (`src/Wombat.Infrastructure/Persistence/Seeds/paediatric-epa-v11.1.json`; the list does not show the
domain, which is given here for reference):

| Code | Title | Domain |
|---|---|---|
| PAED-001 | Providing paediatric emergency care to children | Ambulatory, Emergency and Critical Care in Children |
| PAED-002 | Managing common paediatric presentations | Ambulatory, Emergency and Critical Care in Children |
| PAED-003 | Providing intensive care to children | Ambulatory, Emergency and Critical Care in Children |
| PAED-004 | Managing common neonatal conditions | Care for the Newborn |
| PAED-005 | Providing neonatal care in intensive and high-care settings | Care for the Newborn |
| PAED-006 | Managing long-term health conditions (LTHCs) | Chronic, Developmental and Preventive Care |
| PAED-007 | Maintaining and promoting the health and well-being of children | Chronic, Developmental and Preventive Care |
| PAED-008 | Evaluating and managing neurodevelopmental and behavioural presentations in children | Chronic, Developmental and Preventive Care |
| PAED-009 | Providing care for an adolescent patient | Chronic, Developmental and Preventive Care |
| PAED-010 | Leading and operating within a clinical team | Leadership, Population and Systems Health |
| PAED-011 | Managing population health challenges | Leadership, Population and Systems Health |
| PAED-012 | Communicating with and counselling patients, caregivers and healthcare teams | Ethics, Communication, Education and Palliative Care |
| PAED-013 | Managing child protection, legal and ethical aspects of children at risk | Ethics, Communication, Education and Palliative Care |
| PAED-014 | Providing palliative and end-of-life care | Ethics, Communication, Education and Palliative Care |
| PAED-015 | Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research | Ethics, Communication, Education and Palliative Care |

### Step 1.17 — Curriculum 11.1
Role: CollegeAdmin — Dr Anton Kruger
Route: /admin/curricula
Do: Open Curricula and read the list. Record the curriculum's id as `{CurriculumId}`.
Expect: The list has one row: `Paediatric EPA Curriculum`, version `11.1`, College `College of Paediatricians of South
  Africa`, Speciality and Sub-speciality `Paediatrics`, effective `2026-01-01`, 15 items, Active, offering Edit and
  Items. The header offers Create curriculum. The Demo `IM Core Curriculum` is not listed.
Actual (2026-10-04, T355 replay, wombat_scenario_t355): As expected: one row, Paediatric EPA Curriculum, 11.1, CPSA,
  Paediatrics / Paediatrics, 2026-01-01, 15 items, Active, Edit and Items; Create curriculum; no IM Core Curriculum.
  `{CurriculumId}` = 2.
Gap: none

### Step 1.18 — The 15 items and their targets
Role: CollegeAdmin — Dr Anton Kruger
Route: /admin/curricula/{Id:int}/items
Do: Open Items on the curriculum's row. Read the Existing items table against the table below. Add, edit and remove
  nothing.
Expect: The page is subtitled "Paediatric EPA Curriculum (11.1)" and offers Back to curricula and Back to curriculum.
  - A warning says that a target and its period are read live, in every institution that has adopted the curriculum,
    including periods that have already closed. It also says that a tool list is checked when an activity is filed and
    submitted, and never takes back credit already earned.
  - The table has 15 rows, which match the table below. Every row reads Scale `CPSA Paediatric Entrustment Scale v11.1`,
    Completion window (months) `12` and Weight `—`, and every row offers Edit and Remove.
  - Target and decision holds the target above the decision, for example "3 per semester" above "Decided each
    semester".
  - The Add item form offers no EPA. It says "Every national EPA of Paediatrics is already on this curriculum, …", and
    explains how an EPA would be freed.
Actual (2026-10-04, T355 replay, wombat_scenario_t355): As expected: subtitled "Paediatric EPA Curriculum (11.1)", Back
  to curricula and Back to curriculum; the live-target and tool-list warning; 15 rows matching the table cell by cell,
  the target above the decision, each on the CPSA ladder, window 12, weight "—", Edit and Remove. Add item offers no
  EPA and reads "Every national EPA of Paediatrics is already on this curriculum, …" with how to free one.
Gap: none

**The 15 curriculum items.** The targets are from Annexure B (T130, D39), the minima and tools from Annexure A (T122),
and the decisions from T131:

| EPA | Target | Decision | Minimum level | Minimum by training year (1 / 2 / 3 / 4) | Tools |
|---|---|---|---|---|---|
| PAED-001 | 3 per semester | Decided each semester | 5 | 3a / 3b / 4 / 5 | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise |
| PAED-002 | 3 per semester | Decided each semester | 5 | 3a / 3b / 4 / 5 | CBD, CCA, Chart-stimulated recall, Clinical audit, Direct observation, DOPS, Mini-CEX, MSF, RCA |
| PAED-003 | 3 per semester | Decided each academic year | 5 | 3a / 3b / 4 / 5 | CBD, CCA, Clinical audit, DOPS, Mini-CEX, MSF, RCA, Reflective exercise |
| PAED-004 | 3 per semester | Decided each semester, by the Neonatal team Clinical Competency Committee | 5 | 3a / 3b / 4 / 5 | CBD, CCA, Direct observation, DOPS, Mini-CEX, MSF |
| PAED-005 | 3 per semester | Decided each semester, by the Neonatal team Clinical Competency Committee | 5 | 3a / 3b / 4 / 5 | CBD, DOPS, MSF |
| PAED-006 | 2 per semester | Decided each academic year | 5 | 3a / 3b / 4 / 5 | CCA, Direct observation, DOPS, Mini-CEX, MSF |
| PAED-007 | 1 per semester | Decided each academic year | 5 | 3a / 3b / 4 / 5 | Direct observation, DOPS, Mini-CEX, MSF |
| PAED-008 | 1 per academic year | Decided each academic year, as opportunity allows | 4 | 2 / 3a / 3b / 4 | CBD, DOPS, Mini-CEX, MSF, Reflective exercise |
| PAED-009 | 1 per academic year | Decided each academic year, as opportunity allows | 4 | 2 / 3a / 3b / 4 | CBD, Direct observation, MSF |
| PAED-010 | 3 per semester | Decided each semester | 5 | 3a / 3b / 4 / 5 | Direct observation, MSF |
| PAED-011 | 1 per academic year | Decided each academic year | 4 | 2 / 3a / 3b / 4 | CBD, Direct observation, MSF |
| PAED-012 | 3 per semester | Decided each semester | 5 | 3a / 3b / 4 / 5 | CBD, Chart-stimulated recall, Mini-CEX, MSF |
| PAED-013 | 1 per academic year | Decided each academic year, as opportunity allows | 4 | 2 / 3a / 3b / 4 | CBD, Direct observation, Mini-CEX, MSF |
| PAED-014 | 1 per academic year | Decided each academic year | 4 | 2 / 3a / 3b / 4 | CBD, MSF, Reflective exercise |
| PAED-015 | 1 per semester | Decided each academic year | 4 | 2 / 3a / 3b / 4 | CBD, Direct observation, Learner feedback, MSF, Portfolio and logbook review |

How to read the table:
- **The periods.** Ten EPAs carry a target per semester, 25 observations a semester in all (Annexure B's own total).
  The other five carry a target of one per academic year.
- **Targets count in their current period only.** A registrar's dashboard reads "1 of 3 this semester" (or "in
  2026" for a yearly target), and a target never counts over the whole programme (T130).
- **Reaching the level.** An encounter at or above the training year's minimum counts as reached, and every encounter
  counts towards the target (T071, T073).
- **Labels and ranks.** A rung is written as the College prints it. The minima are stored as ranks, so `4` is stored
  as 5.
- **Tools.** The tool list decides which instruments may be filed against the EPA (T122, D20).

## Phase 1.E — KGK adopts curriculum 11.1

This phase is the T091 hinge. Until KGK adopts a curriculum version, its admins read no catalogue, its invitation form
offers no speciality, and nobody can be admitted (T092).

### Step 1.19 — Before the adoption, KGK reads no catalogue
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/curricula → /admin/epas
Do: Open Curricula, then EPAs, before adopting anything.
Expect: Curricula shows the empty state "No curricula yet": "Your institution has not adopted a curriculum yet. Adopt
  one on the Curriculum adoptions page before admitting trainees." It offers no Create curriculum (T211).
  EPAs shows "No EPAs yet". An InstitutionalAdmin reads the national EPAs of the disciplines her institution has
  adopted, plus its own EPAs, and there are none of either.
Actual (2026-10-04, T355 replay, wombat_scenario_t355): Curricula as expected: "No curricula yet" with the quoted text,
  no Create curriculum. EPAs reads "No EPAs yet" with "Create the first EPA to start building curricula and assessment
  forms.", and the header offers Create EPA, whose form (/admin/epas/new, left unsaved) has an empty Sub-speciality
  picker.
Gap: [F-1.19a, T291] still: before any adoption the EPAs page offers an InstitutionalAdmin Create EPA, and its empty
  state invites her to "Create the first EPA", but the form's Sub-speciality picker offers nothing, so the page links
  to a form she cannot complete (T211's rule).

### Step 1.20 — Adopt curriculum 11.1 for KGK
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
Actual (2026-10-04, T355 replay, wombat_scenario_t355): As expected: no institution picker; "No curriculum adoptions
  yet for this institution."; the picker offers the CPSA curriculum and the Demo IM Core Curriculum (2026.1); Adopt
  disabled until chosen. First Adopt: "Curriculum adopted.", row CPSA / Paediatrics / Paediatrics / Paediatric EPA
  Curriculum / 11.1 / 2026-10-04 / Active. Second Adopt refused: "This institution has already adopted this curriculum
  version." The supersede note is shown.
Gap: none

### Step 1.21 — What the adoption opens to KGK
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
  - Home's Specialities & sub-specialities card reads 1 and 1 (T291 item 4).
Actual (2026-10-04, T355 replay, wombat_scenario_t355): EPAs lists the 15 PAED EPAs only, but every row still offers
  Edit (and the header Create EPA). Curricula: one row with Items only, no Edit, no Create curriculum. Items: the
  College-sets-national-items warning, 15 rows matching Step 1.18, each "Set by the College"; the form is headed "Add
  your institution's own item" and says every EPA KGK could add is on the curriculum, so first create a local EPA of
  Paediatrics. Home's card reads 2 and 2.
Gap: [F-1.21a, T291] still (T291 item 1): /admin/epas offers Edit on all 15 national EPAs to an InstitutionalAdmin,
  whose save UpdateEpa refuses. [F-1.21b, T291] still (T291 item 4, as F-1.10a): the card reads 2 and 2, not 1 and 1.

### Step 1.22 — The ladder, read-only to KGK
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/entrustment-scales
Do: Open Entrustment scales and read the list.
Expect: The list shows `CPSA Paediatric Entrustment Scale v11.1` at 6 levels and `O-R Scale` at 5, each with its
  description. It offers no Create scale and has no actions column, because only an Administrator changes a scale
  (T057, T239).
Actual (2026-10-04, T355 replay, wombat_scenario_t355): As expected: two rows (CPSA ladder 6, O-R Scale 5) with
  descriptions; no Create scale and no Actions column. The subtitle still reads "Define and maintain the rating ladders
  used by assessment forms and committee reviews."
Gap: [F-1.22a, T326] still: the read-only scales list tells an InstitutionalAdmin to "Define and maintain" ladders she
  cannot change.

### Step 1.23 — KGK's own record
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/institutions/{Id:int} → /not-found
Do: Open KGK's record at `/admin/institutions/{InstitutionId}` by typing its address, since nothing in her nav leads
  there. Read it and change nothing. Then open the Demo Institution's id the same way.
Expect: KGK's page is headed "Edit institution" and shows KGK's name, the short code `KGK`, the contact email, and
  that KGK is active. Save is offered for the name, short code and contact email.
  She is offered nothing that would deactivate KGK, neither a Deactivate nor an Active she can untick, and no link
  (Back or Cancel) to the Institutions list: deactivating an institution and listing them belong to an Administrator
  alone (`DeactivateInstitutionCommand`; DESIGN.md § Table system, T211).
  The Demo Institution's id sends her to Page not found, not "You cannot open this page". The product never confirms
  that a record outside her institution exists (CLAUDE.md § InstitutionalAdmin scope-aware powers).
Actual (2026-10-04, T355 replay, wombat_scenario_t355): As expected: typed, /admin/institutions/2 is headed "Edit
  institution", "Back to home" beside the heading, KGK's name, `KGK` and the contact email, Status "Active" as text
  with "Set by a global administrator.", then Cancel (to /) and Save; no checkbox, no Deactivate, no link to the
  Institutions list. Nothing was saved. /admin/institutions/1 lands on /not-found, "Page not found". Typed,
  /admin/institutions/new still renders "Create institution" (Name, Short code, Contact email, Cancel, Save), headed by
  "Back to home" (not submitted).
Gap: [F-1.23c, T291] still: typed by her, /admin/institutions/new renders the "Create institution" form, which
  CreateInstitutionCommand always refuses her.

## Phase 1.F — The College's instruments, and one KGK type from the builder

Each activity type bundles three things: a form schema, a workflow and credit rules (CUSTOMIZATION.md). The College's
instruments are seeded as the twelve `*_cpsa` types, scoped to the Paediatrics speciality. Prof Mbatha reads them and
changes none: they open read-only to her, because the College writes them (T300, D52; Dr Kruger does so in Step
6.14a). KGK's own type is built in the builder, whose tabs are Metadata, Form, Workflow and Credit.

### Step 1.24 — The twelve CPSA instruments are published
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/activity-types
Do: Open Activity types, search `cpsa` and read the list. Then clear the search.
Expect: The search leaves the 12 rows in the table below. Each reads Scope `Speciality · Paediatrics`, Published `v1`,
  Draft `None` and Active, and each offers View, named for its row ("View Mini-CEX (Paediatrics)"), not Edit: the
  College's instruments are the College's, and she may not write them (T300, D52; T239 names each row's action). With
  the search cleared there are 22 rows: these twelve and the ten Demo types, which also offer View and whose Scope
  names the Demo speciality, `Speciality · General Medicine` (T291 item 5). New activity type is still offered, since
  she may create a type in her own institution.
  Two of the twelve are system-managed (T162, T164): `msf_cpsa` and `learner_feedback_cpsa`. A released MSF or
  learner-feedback campaign writes them, and nobody is offered them when filing an activity.
Actual (2026-10-04, T355 replay, wombat_scenario_t355): As expected: `cpsa` leaves the 12 rows of the table, each
  "Speciality · Paediatrics", v1, None, Active and View, named "View <name>" ("View Mini-CEX (Paediatrics)"); cleared,
  22 rows, the ten Demo rows each View and "Speciality · General Medicine"; New activity type offered. Ids:
  mini_cex_cpsa 11, dops_cpsa 12, cbd_cpsa 13, direct_observation_cpsa 14, cca_cpsa 15, rca_cpsa 16,
  chart_stimulated_recall_cpsa 17, reflective_exercise_cpsa 18, clinical_audit_cpsa 19, portfolio_review_cpsa 20,
  msf_cpsa 21, learner_feedback_cpsa 22.
Gap: none

The twelve instruments come from `ActivityTypeSeedCatalogue`, which sets each one's key, name and instrument. No two
seeded types share a name: the ten Demo types are named without "(Paediatrics)".

| Key | Name | Instrument | Rating |
|---|---|---|---|
| `mini_cex_cpsa` | Mini-CEX (Paediatrics) | Mini-CEX | `overall_level`, on the v11.1 ladder |
| `dops_cpsa` | DOPS (Paediatrics) | DOPS | the same |
| `cbd_cpsa` | Case-Based Discussion (Paediatrics) | CBD | the same |
| `direct_observation_cpsa` | Direct Observation (Paediatrics) | Direct observation | the same |
| `cca_cpsa` | Clinical Case Analysis (Paediatrics) | CCA | the same |
| `rca_cpsa` | Random Case Analysis (Paediatrics) | RCA | the same |
| `chart_stimulated_recall_cpsa` | Chart-Stimulated Recall (Paediatrics) | Chart-stimulated recall | the same |
| `reflective_exercise_cpsa` | Reflective Exercise (Paediatrics) | Reflective exercise | unrated; credits nothing (D6, D7) |
| `clinical_audit_cpsa` | Clinical Audit (Paediatrics) | Clinical audit | unrated; credits nothing |
| `portfolio_review_cpsa` | Portfolio and Logbook Review (Paediatrics) | Portfolio and logbook review | unrated; credits nothing |
| `msf_cpsa` | Multi-Source Feedback (Paediatrics) | MSF | system-managed; credits nothing (D8) |
| `learner_feedback_cpsa` | Learner Feedback (Paediatrics) | Learner feedback | system-managed; credits nothing (D35) |

### Step 1.25 — A College instrument is read-only to KGK
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/activity-types/{ActivityTypeId:int}
Do: Open `Mini-CEX (Paediatrics)` from its View and read each tab, opening the `overall_level` field to read its
  settings. Change nothing, save nothing, and leave by Back to list.
Expect: The page is headed "Mini-CEX (Paediatrics)", not "Edit …", and the browser tab reads "Mini-CEX (Paediatrics) ·
  Wombat" (T190). It opens on the Form tab, under a standing information notice that is there on every visit (no alert
  role): "Set by the College that owns Paediatrics. You can read this activity type here, but not change it." (T300;
  the wording is a product decision.)
  - **Form settings** read, as text, Encounter date field `observed_on`, Entrustment rating field `overall_level` and
    EPA field `epa_id`.
  - **Sections.** Request holds the EPA, the Assessor (a User field), the date observed, the clinical setting, the
    presenting problem and the case complexity. Entrustment holds the `overall_level` rating. Feedback holds three
    long-text fields. Each section and field offers View, which shows its settings as text; `overall_level`'s
    Entrustment scale reads `CPSA Paediatric Entrustment Scale v11.1`, the ladder its seed key binds (T271).
  - **Metadata**, as text: Key `mini_cex_cpsa`, Name, Scope `Speciality · Paediatrics`, This tool is `Mini-CEX`, the
    description, and Status Active.
  - **Workflow**, as a code block. Draft → Requested → Completed, with Declined and Cancelled. The named assessor
    (`field:assessor_user_id`) completes or declines.
  - **Credit**, as a code block. One directive: it matches on the EPA field, counts 1, and judges the level reached by
    what `overall_level` records.
  She is offered nothing that would change the type: no Save draft, Discard draft or Publish, and no Add, Up, Down or
  Delete. The College's instruments are the College's (T211's rule; T300, D52). Any save or publish she sent would be
  refused by `ActivityTypeScopeGuard`: "You do not have permission to modify activity types in that speciality."
Note: Do not save a draft on a seeded type, even as an Administrator or the College (D52): a seeded type with an
  operator's draft stops receiving seed updates (T103).
Actual (2026-10-04, T355 replay, wombat_scenario_t355): View opened /admin/activity-types/11, headed "Mini-CEX
  (Paediatrics)", tab "Mini-CEX (Paediatrics) · Wombat", on Form, under the notice "Set by the College that owns
  Paediatrics. You can read this activity type here, but not change it." (no role). Form settings as text: observed_on
  / overall_level / epa_id. Sections Request (EPA, Assessor, Date observed, Clinical setting, Presenting problem, Case
  complexity), Entrustment (overall_level) and Feedback (three long texts), each with View; overall_level's View reads
  Entrustment scale "seed:cpsa:scale:v11.1". Metadata as text: Key mini_cex_cpsa, the name, "Speciality · Paediatrics",
  This tool is "Mini-CEX", the description, Active. Workflow (draft → requested → completed, declined, cancelled;
  complete and decline by field:assessor_user_id) and Credit (one directive on epa_id, amount 1, minimum_level_field
  overall_level) are code blocks. The header offers only Back to list: no Save draft, Discard draft, Publish, Add, Up,
  Down or Delete. Nothing saved; Back to list.
Gap: [F-1.25b, T271] still: the read-only field view prints the raw seed key "seed:cpsa:scale:v11.1" where the ladder's
  name belongs.

### Step 1.26 — Start KGK's teaching log
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/activity-types → /admin/activity-types/new → /admin/activity-types/{ActivityTypeId:int}
Do: Press New activity type, open the Metadata tab and enter:
  - Key `kgk_teaching_log` and Name `KGK Teaching Session Log`;
  - Scope Institution, with `Kgosi Kgari Teaching Hospital`, which is how it opens: leave it untouched;
  - This tool is `Not a WBA instrument`;
  - Description `A registrar's log of a teaching session they delivered at KGK. Not rated, and credits nothing.`;
  - Active on.
  Press Save draft. Record the type's id as `{ActivityTypeId}`.
Expect: The builder opens on the Form tab, headed "New activity type", with the browser tab reading "New activity type
  · Wombat" (T300, T190). It starts with a default draft:
  - one Details section holding a required Title text field;
  - a workflow from draft to submitted;
  - the credit rules `{"counts_for": []}`.
  Scope offers her only what she may save: Institution, with KGK the only institution, and a new type starts there,
  never Global (T300, D52; DESIGN.md: a picker offers exactly what its command accepts). So the first Save draft is
  accepted without Scope being touched. This tool is offers `Not a WBA instrument` and the twelve instruments.
  Save draft moves the page to the type's own address and reads "Draft saved." (T291 item 3). The page is then headed
  "Edit KGK Teaching Session Log", and the browser tab says the same: "Edit KGK Teaching Session Log · Wombat"
  (T190).
Note: The key and name cannot collide with a seed, since every seeded key is a bare family or ends `_cpsa`. The type
  has no instrument key, so no tool list restricts which EPA it is filed against (D21, D45).
Actual (2026-10-04, T355 replay, wombat_scenario_t355): New activity type opened /admin/activity-types/new on Form,
  headed "New activity type", tab "New activity type · Wombat", with the default draft (Details section, required Title
  text field; draft → submitted; `{"counts_for": []}`). Scope offers Institution only, chosen, and Institution KGK
  only, chosen; This tool is offers "Not a WBA instrument" and the twelve. With Scope untouched the first Save draft
  was accepted: /admin/activity-types/23, headed "Edit KGK Teaching Session Log", tab "Edit KGK Teaching Session Log ·
  Wombat", Metadata still shown, Discard draft and Publish offered, and no "Draft saved.". `{ActivityTypeId}` = 23.
Gap: [F-1.26c, T291] still (T291 item 3): the first Save draft shows no "Draft saved.".

### Step 1.27 — The form, and a duplicate key refused
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/activity-types/{ActivityTypeId:int}
Do: On the Form tab, edit the Details section: key `session`, title `Teaching session`. Edit its Title field: key
  `topic`, label `Topic`, type Text, required. Then add five fields to the section:
  - `epa_id`, labelled `EPA`, type EPA, required;
  - `delivered_on`, labelled `Date delivered`, type Date, required;
  - `audience`, labelled `Audience`, type Choice, required, with the options `Interns`, `Medical students`, `Nursing
    staff` and `Registrars`, one per line;
  - `objectives`, labelled `Learning objectives`, type Long text, required;
  - `supervisor_user_id`, labelled `Supervising consultant`, type User, not required, leaving Names a on its default.
  Give Audience the key `topic` by mistake, and press Save draft. Then correct its key to `audience`. Under Form
  settings, choose Encounter date field `delivered_on`, Entrustment rating field "None — this form asserts no
  entrustment level" and EPA field `epa_id`. Press Save draft again.
Expect: The field types offered are Text, Long text, Number, Date, Choice, Multi-choice, Scale, User, EPA, Likert,
  Procedure reference, File and Signature.
  - **The User field** shows Names a in place of Options. It defaults to Assessor, and also offers InstitutionalAdmin,
    Coordinator, CommitteeMember and Trainee. The people it offers come from the directory (T102).
  - **The mistaken save** is refused, and the refusal names both fields: "Field key 'topic' is used by 2 fields ('Topic'
    in 'Teaching session', 'Audience' in 'Teaching session'); give each field its own key."
  - **The corrected save** reads "Draft saved." The Form settings pickers offer only fields of the matching type.
  - **The live preview** shows one section, Teaching session, with six inputs: a text box, an EPA picker, a date
    input, a choice of four, a long-text box and a person picker. The first five are required.
  - **The EPA picker** offers the 15 PAED EPAs and not the Demo `EPA-001`, following the form's own rule (T291 item 6).
  - **The person picker** offers nobody in the preview. Whom it offers depends on the registrar filing it: an active
    Assessor at their institution, never the registrar themselves.
Actual (2026-10-04, T355 replay, wombat_scenario_t355): The Type picker offers the thirteen types listed. The User
  field shows "Names a" in place of Options: "Assessor (default)", InstitutionalAdmin, Coordinator, CommitteeMember,
  Trainee. The mistaken save was refused with the quoted text word for word. The corrected save read "Draft saved.";
  Encounter date offered only delivered_on, Entrustment rating only None, EPA field only epa_id. The preview, under "*
  marks a field you must fill in.", shows one group, Teaching session, with the six inputs, the first five starred. Its
  EPA picker lists the Demo EPA-001 above the 15 PAED EPAs. Supervising consultant offers only "Select…". The field
  list still labels types by enum name ("· Epa", "· LongText").
Gap: [F-1.27a, T291] still (T291 item 6): the preview's EPA picker lists the Demo EPA-001 beside the PAED EPAs (so does
  the Mini-CEX preview in Step 1.25). [F-1.27b, T324] still: the builder's field list prints raw enum names ("Epa",
  "LongText") where its own Type picker reads "EPA" and "Long text" (low).

### Step 1.28 — The workflow, and a `field:` rule refused
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/activity-types/{ActivityTypeId:int}
Do: On the Workflow tab, replace the Workflow JSON with the workflow below, but with the cancel transition's actor
  written as `subject|field:topic`. Press Save draft. Then restore the actor to `subject|creator` and press Save draft
  again.
Expect: The first save is refused: "Transition 'cancel' actor names 'field:topic', but 'topic' is not a User field; a
  field: rule must name a User field." (T102). The second save reads "Draft saved."
  In the saved workflow, the registrar, or whoever filed the log for them, either logs the session or cancels the
  draft. Logging checks the whole form (`validation: all`), and cancelling checks formats only (`draft`, T105).
Actual (2026-10-04, T355 replay, wombat_scenario_t355): As expected: the first save was refused with the quoted text
  word for word; with the actor restored to `subject|creator` the second read "Draft saved.", and the Workflow JSON
  holds draft → logged (log, all) and draft → cancelled (cancel, draft), both `subject|creator`.
Gap: none

The workflow for Step 1.28. The DSL, including `editable_by` and the actor grammar (`subject`, `creator`, `role:`,
`scope:`, `field:`), is described in CUSTOMIZATION.md:

```json
{
  "version": 1,
  "initial_state": "draft",
  "states": [
    { "key": "draft", "label": "Draft" },
    { "key": "logged", "label": "Logged", "terminal": true },
    { "key": "cancelled", "label": "Cancelled" }
  ],
  "transitions": [
    { "key": "log", "from": "draft", "to": "logged", "actor": "subject|creator", "validation": "all" },
    { "key": "cancel", "from": "draft", "to": "cancelled", "actor": "subject|creator", "validation": "draft" }
  ]
}
```

### Step 1.29 — The credit rules
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/activity-types/{ActivityTypeId:int}
Do: Open the Credit tab. Leave the Credit rules JSON as `{"counts_for": []}`.
Expect: The type credits nothing. The save accepts an EPA field with no credit directive (T137,
  `EvidenceEpa.EnsureCreditAgrees`). Each log is stamped with its EPA, which activity lists show.
  A type that credits nothing is held to one encounter-date rule only: the date may not be after today (T160). It is
  not held to the programme start, and it gets no 14-day late-filing warning (`EncounterDatePolicy.CanCredit`, D15).
Actual (2026-10-04, T355 replay, wombat_scenario_t355): The Credit tab reads `{"counts_for": []}`, left unchanged. The
  saves of Steps 1.27 and 1.28 were accepted with the EPA field set and no credit directive. The encounter-date
  behaviour is for Act 2 onward to observe.
Gap: none

### Step 1.30 — Publish version 1
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/activity-types/{ActivityTypeId:int}
Do: Press Publish.
Expect: The page reads "Published version 1." Discard draft disappears, and Publish is disabled, with the hint "Save a
  draft to publish." The Key can no longer be changed. There are no publish warnings, since there is no earlier version
  to compare against.
Actual (2026-10-04, T355 replay, wombat_scenario_t355): As expected: "Published version 1."; Discard draft gone;
  Publish disabled, its hint "Save a draft to publish." carried as the button's title (tooltip), not on-page text; Key
  disabled on Metadata; no publish warnings. Save draft stays offered.
Gap: none

### Step 1.31 — KGK's type in the list
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/activity-types
Do: Press Back to list and read the list. Then search `KGK`.
Expect: The list has 23 rows, and no two share a name:
  - the ten Demo types, whose Scope reads `Speciality · General Medicine` (T291 item 5);
  - the twelve `*_cpsa` types;
  - `KGK Teaching Session Log`, whose Scope reads `Institution · Kgosi Kgari Teaching Hospital`, with `v1`, `None` and
    Active.
  Her own type's row offers Edit, and every other row View (T300). The search leaves that last row only. Which of these
  types a KGK registrar is offered is checked in Act 2.
Actual (2026-10-04, T355 replay, wombat_scenario_t355): 23 rows, no two names alike: the ten Demo types ("Speciality ·
  General Medicine"), the twelve `*_cpsa`, and KGK Teaching Session Log, "Institution · Kgosi Kgari Teaching Hospital",
  v1, None, Active; it alone offers Edit ("Edit KGK Teaching Session Log"), the other 22 View. `KGK` leaves that row
  only. `zzz` (states.md) still reads "No activity types" and "Create the first activity type to start the builder." At
  390 px the page is 390 px wide with no sideways scroll.
Gap: [F-1.31b, T326] still: a search that matches nothing shows the empty-catalogue state, "No activity types … Create
  the first activity type", though 23 types exist; it should say that nothing matches the search.

## Act 1 outcome state

Replay check (2026-10-04, T355 replay, wombat_scenario_t355): match but for one replay slip; 3 successful Save drafts.
The slip: Step 1.7 was issued twice, so Invitations holds a third row (Mbatha's first, revoked and unused) and
IssueInvitationCommand succeeded 3 times, not 2. Every other query is as expected.

Beside the ignored Demo College, Demo Institution and their catalogue, the database now holds:
- **The CPSA's seeded catalogue, unchanged:**
  - the College (`CPSA`, seed key `cpsa`);
  - the Paediatrics speciality and its Paediatrics sub-speciality, which defaults to the v11.1 ladder;
  - the ladder itself: `1`, `2`, `3a`, `3b`, `4`, `5`, at ranks 1 to 6;
  - 15 national EPAs;
  - curriculum 11.1, effective 2026-01-01, with 15 national items.
- **KGK** (`Kgosi Kgari Teaching Hospital`), active, created by the Administrator, with no seed key.
- **Two new accounts:** Dr Kruger, CollegeAdmin of the CPSA, and Prof Mbatha, InstitutionalAdmin of KGK, and KGK's only
  user. Both of their invitations are used.
- **One adoption:** KGK adopted `Paediatric EPA Curriculum` 11.1, and the adoption is active.
- **13 activity types in KGK's reach:**
  - the twelve `*_cpsa` types, scoped to the Paediatrics speciality, two of them system-managed;
  - `kgk_teaching_log`, scoped to KGK, at version 1, with no draft.

There are still no activities, no trainee or assessor profiles and no committee panel: those need the people Act 2
invites.

Run these queries with psql against the replay's database. The comment under each query is the expected result.

```sql
SELECT "Name", "ShortCode", "IsActive" FROM "Colleges" WHERE "SeedKey" = 'cpsa';
  -- College of Paediatricians of South Africa | CPSA | t
SELECT s."Name", e."Name" AS default_scale FROM "SubSpecialities" s
  LEFT JOIN "EntrustmentScales" e ON e."Id" = s."DefaultEntrustmentScaleId"
  WHERE s."SeedKey" = 'cpsa:paediatrics:paediatrics';
  -- Paediatrics | CPSA Paediatric Entrustment Scale v11.1
SELECT string_agg(l."Label", ' ' ORDER BY l."Order") FROM "EntrustmentLevels" l
  JOIN "EntrustmentScales" e ON e."Id" = l."ScaleId" WHERE e."SeedKey" = 'cpsa:scale:v11.1';
  -- 1 2 3a 3b 4 5
SELECT count(*) FROM "Epas"
  WHERE "SeedKey" LIKE 'cpsa:paediatrics:epa:%' AND "OwningInstitutionId" IS NULL AND "IsActive";
  -- 15
SELECT i."QuotaPeriod", i."RequiredCount", count(*) FROM "CurriculumItems" i
  JOIN "Curricula" c ON c."Id" = i."CurriculumId"
  WHERE c."SeedKey" = 'cpsa:paediatrics:curriculum:v11.1' AND i."OwningInstitutionId" IS NULL
  GROUP BY 1, 2 ORDER BY 1, 2;
  -- 0 | 1 | 5   (0 = per academic year)
  -- 1 | 1 | 2   (1 = per semester)
  -- 1 | 2 | 1
  -- 1 | 3 | 7
SELECT "Name", "ShortCode", "ContactEmail", "IsActive", "SeedKey" IS NULL AS unseeded
  FROM "Institutions" WHERE "ShortCode" = 'KGK';
  -- Kgosi Kgari Teaching Hospital | KGK | paeds-admin@kgk.wombat.local | t | t
SELECT u."Email", u."FirstName", u."LastName", r."Name" AS role, i."ShortCode" AS institution, c."ShortCode" AS college
  FROM "AspNetUsers" u
  JOIN "AspNetUserRoles" ur ON ur."UserId" = u."Id"
  JOIN "AspNetRoles" r ON r."Id" = ur."RoleId"
  LEFT JOIN "Institutions" i ON i."Id" = u."InstitutionId"
  LEFT JOIN "Colleges" c ON c."Id" = u."CollegeId"
  WHERE u."Email" IN ('kruger@cmsa.wombat.local', 'mbatha@kgk.wombat.local') ORDER BY u."Email";
  -- kruger@cmsa.wombat.local | Anton | Kruger | CollegeAdmin | (null) | CPSA
  -- mbatha@kgk.wombat.local | Nolwazi | Mbatha | InstitutionalAdmin | KGK | (null)
SELECT count(*) FROM "AspNetUsers" u JOIN "Institutions" i ON i."Id" = u."InstitutionId" WHERE i."ShortCode" = 'KGK';
  -- 1
SELECT "Email", "TargetRole", "UsedOn" IS NOT NULL AS used, "RevokedOn" IS NULL AS not_revoked
  FROM "Invitations" ORDER BY "Id";
  -- kruger@cmsa.wombat.local | CollegeAdmin | t | t
  -- mbatha@kgk.wombat.local | InstitutionalAdmin | t | t
SELECT i."ShortCode", c."Name", c."Version", a."AdoptedOn" = CURRENT_DATE AS today, a."IsActive"
  FROM "InstitutionCurriculumAdoptions" a
  JOIN "Institutions" i ON i."Id" = a."InstitutionId" JOIN "Curricula" c ON c."Id" = a."CurriculumId";
  -- KGK | Paediatric EPA Curriculum | 11.1 | t | t   (one row; AdoptedOn is the UTC date of Step 1.20)
SELECT "Key", "Scope", "SystemManaged", "WbaToolKey", "Version", "StagingSchemaJson" IS NULL AS no_draft
  FROM "ActivityTypes" WHERE right("Key", 5) = '_cpsa' OR "Key" = 'kgk_teaching_log' ORDER BY "Key";
  -- 13 rows, all Version 1 with no draft. Scope is 2 (Speciality) on the twelve seeded types and 1 (Institution) on
  -- kgk_teaching_log. SystemManaged is set on msf_cpsa and learner_feedback_cpsa only, and WbaToolKey is null on
  -- kgk_teaching_log only.
SELECT count(*) FROM "ActivityTypes";
  -- 23
SELECT t."ScopeId" = i."Id" AS at_kgk,
       t."SchemaJson"->>'observation_date_field' AS date_field,
       t."SchemaJson"->>'evidence_epa_field' AS epa_field,
       t."SchemaJson"->>'rated_level_field' IS NULL AS unrated,
       jsonb_array_length(t."SchemaJson"->'sections'->0->'fields') AS fields,
       t."CreditRulesJson"->'counts_for' AS counts_for,
       (SELECT count(*) FROM "ActivityTypeVersions" v WHERE v."ActivityTypeId" = t."Id") AS versions
  FROM "ActivityTypes" t, "Institutions" i WHERE t."Key" = 'kgk_teaching_log' AND i."ShortCode" = 'KGK';
  -- t | delivered_on | epa_id | t | 6 | [] | 1
SELECT "Action", "Success", count(*) FROM "AuditEntries"
  WHERE "Action" IN ('IssueInvitationCommand', 'CreateInstitutionCommand', 'AcceptInvitationCommand',
                     'AdoptCurriculumCommand', 'SaveActivityTypeDraftCommand', 'PublishActivityTypeDraftCommand')
  GROUP BY 1, 2 ORDER BY 1, 2;
  -- AcceptInvitationCommand | t | 2
  -- AdoptCurriculumCommand | f | 1          (Step 1.20's second Adopt)
  -- AdoptCurriculumCommand | t | 1
  -- CreateInstitutionCommand | t | 1
  -- IssueInvitationCommand | f | 1          (Step 1.5's first press)
  -- IssueInvitationCommand | t | 2
  -- PublishActivityTypeDraftCommand | t | 1
  -- SaveActivityTypeDraftCommand | f | 2    (Steps 1.27 and 1.28's refusals)
  -- SaveActivityTypeDraftCommand | t | n    (one per successful Save draft: at least 3)
```

After the queries, `pg_dump -Fc` the database to `recovery/scenario-post-act1.dump`.

## Handoff to Act 2

Act 2 runs on Prof Mbatha's account and the accounts she invites. It needs no Administrator and no CollegeAdmin. From
Act 1 it needs:
- **Mbatha's account** at KGK. She issues every Act 2 invitation.
- **KGK's active adoption of 11.1.** Because of it, her invitation form offers the Paediatrics speciality and
  sub-speciality (T092), and admission offers curriculum 11.1.
- **The ids this act recorded:**

  | Id | Is | Recorded in |
  |---|---|---|
  | `{CollegeId}` | the CPSA | Step 1.3 |
  | `{ScaleId}` | `CPSA Paediatric Entrustment Scale v11.1`, which the STAR and rating pickers keep to | Step 1.4 |
  | `{InstitutionId}` | KGK | Step 1.6 |
  | `{SpecialityId}` | Paediatrics, the speciality | Step 1.12 |
  | `{SubSpecialityId}` | Paediatrics, the sub-speciality | Step 1.13 |
  | `{CurriculumId}` | `Paediatric EPA Curriculum` 11.1 | Step 1.17 |
  | `{ActivityTypeId}` | `kgk_teaching_log` | Step 1.26 |

  Also record the ids of the ten `*_cpsa` types a registrar files, for Act 3. The list's Edit links carry them.
- **What the teaching log asks for.** Its Supervising consultant field is optional, and it names an active Assessor at
  the registrar's institution. From Act 2 on, a KGK registrar's log may name one of the consultants Act 2 creates, or
  leave the field blank.

Kruger returns in Act 6 to maintain the catalogue. The Administrator returns in Act 6 and the appendix.
