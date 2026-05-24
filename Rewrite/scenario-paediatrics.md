# Wombat end-to-end scenario: Paediatrics at Kgosi Kgari

A time-phased runbook that walks a realistic ZA paediatric training programme through Wombat, from Day 0 (empty install) to a year-4 registrar's graduation. The document has two jobs:

1. **Primary — test scenario.** Catch integration gaps that unit tests miss. Every step has an expected outcome and a gap slot; findings are the document's output.
2. **Secondary — training-material seed.** With a second pass (concept boxes, role-sliced entry points, troubleshooting blocks) it promotes to end-user documentation. The runbook is structured to convert well but does not try to be both at once.

## How to read this document

- Linear. Start at Act 1, work forward. Later acts assume earlier acts completed successfully.
- Against a clean dev install: admin bootstrap + `DataSeeder` (creating the `Demo Institution`) + `DevUserSeeder`. The Demo Institution is ignored throughout — its activity types are speciality-scoped to Internal Medicine and will not appear in the Paediatric users' selectors, so there's no interference.
- Every step uses a consistent six-line block (see Conventions). The `Gap:` line is empty until the step is played and a finding captured. The `Actual:` line is empty until played.
- Play the document front to back with Wombat running at `http://localhost:5080`. Login credentials for the bootstrap admin in `pwd_DO_NOT_COMMIT.txt`.

## Conventions

**Step format** — every step is six lines:

```
### Step X.Y — short label
Role: [who is doing this]
Route: /path/to/page
Action: [what to click, fill, select]
Expected: [what should happen / what you should see]
Actual:
Gap:
```

**Click paths** — literal. "Click `New institution` → type `Kgosi Kgari Teaching Hospital` in `Name` → click `Save`." Not "create an institution".

**Dev-data choices** (baked into the scenario):
- *Curriculum content:* synthetic CanMEDS-derived (recognisable, defensible, not claiming accredited).
- *Entrustment scale:* 5-level ten Cate / Chen family (Observation only → Can supervise others).
- *Fresh install at Act 1; accumulate thereafter.* Acts 2+ depend on state Act 1 built. A "reset to end-of-Act-N" script is flagged as out-of-scope.
- *One document, not per-act files.* Navigable by heading. Can split if any single act passes ~600 lines.

**Terminology shorthand used throughout:**
- *EPA* — Entrustable Professional Activity; a chunk of work a trainee must be able to perform. Wombat models them as `Epa` entities.
- *Curriculum item* — an EPA's requirement within a specific curriculum: how many times, at what entrustment level, by what point in training.
- *Activity type* — Wombat's schema-driven assessment definition: form + workflow + credit rules. Lives in the Activity platform (`/admin/activity-types`).
- *Activity* — a specific instance: "Dr Molefe's Mini-CEX on 2026-03-12".
- *STAR* — Statement of Awarded Responsibility; the formal certification that a committee has entrusted a trainee with an EPA at a given level.
- *WBA* — Workplace-based assessment; the category of activities that assessors rate (Mini-CEX, CbD, DOPS, ACAT, MSF).

## Cast

**The department** (all hypothetical, all fit in one real ZA teaching-hospital paed department):

| Role in Wombat | Real-world role | Person | Notes |
|---|---|---|---|
| Administrator (global) | Bootstrap admin | (seeded) `admin@wombat.local` | Used once in Act 1 to provision Prof Mbatha. |
| Administrator (scoped institutional) | Head of Department | Prof Nolwazi Mbatha | Creates institution and programme structure. |
| Coordinator | Programme Coordinator | Dr Pieter Smit | Day-to-day admin: onboarding, stalled-activity triage, committee scheduling. |
| CommitteeMember | Committee chair (senior consultant + chairs reviews) | Dr Thandi Zulu | Also an Assessor. |
| Assessor (+ CommitteeMember) | Senior consultants | Dr David Naidoo, Dr Sarah Botha | Both rate WBAs and sit on the review committee. |
| Assessor | Consultants | Dr Mohammed Patel, Dr Fatima Khumalo | Rate WBAs only. |
| CommitteeMember (external) | External examiner | Dr John van Rensburg (Stellenbosch) | Committee only; never rates WBAs. |
| Trainee (year 4) | Final-year registrar | Dr Lerato Molefe | Graduates in Act 5. |
| Trainee (year 3) | Third-year registrar | Dr Anele Dlamini | Advances in Act 4 committee. |
| Trainee (year 2) | Second-year registrar | Dr Pieter du Plessis | Advances in Act 4. |
| Trainee (year 1) | First-year registrars | Dr Nomsa Mahlangu, Dr Sipho Ndlovu | Both start programme fresh in Act 2. |

## Institution & programme

- **Institution:** Kgosi Kgari Teaching Hospital, Mafikeng, North West Province, ZA.
- **Department:** Paediatrics.
- **Sub-speciality:** General Paediatrics.
- **Programme:** FCPaed(SA) Part 1 + Part 2, 4-year, CMSA-modelled (synthetic).
- **Scenario clock:** 2026-01-15 (Act 1) → 2029-12-15 (Act 5).
- **Notional registrar intake:** 2 new registrars every January. The five active registrars in the cast reflect cohorts from Jan-2023, Jan-2024, Jan-2025, and Jan-2026.

---

# Act 1 — Day 0: Institutional setup

**Date in scenario:** Monday 2026-01-12.
**Who:** Prof Nolwazi Mbatha, alone, after hours, 17:00–18:30.
**Why:** Hospital IT has just provisioned Wombat for the Paediatrics Department. Before the registrars return for the new year, Prof Mbatha needs to stand up the institution's structure, enter the curriculum, and publish the assessment types her consultants will use.

**Starting state:**
- Wombat running at `http://localhost:5080`.
- The bootstrap admin (`admin@wombat.local`) exists from `AdminSeeder`.
- `DataSeeder` has created the `Demo Institution` with its Internal Medicine speciality and 10 activity types. **Ignored** throughout — it lives in a separate institution + speciality scope and does not interfere.
- No Paediatrics data exists.

**Act 1 goal:**
1. Institution, speciality, and sub-speciality exist.
2. 15 General Paediatrics EPAs are defined.
3. Curriculum "FCPaed(SA) Part 1 — 2026.1" is published with 15 curriculum items.
4. The Paediatric entrustment scale is defined.
5. 10 activity types are published and ready for registrars to submit against.
6. Prof Mbatha's consultant/registrar colleagues are ready to receive invitations in Act 2.

## Phase 1.A — Provision Prof Mbatha as scoped admin

Prof Mbatha needs an Administrator account of her own. The bootstrap `admin@wombat.local` is global; she should have a named identity so audit entries attribute her actions correctly.

### Step 1.1 — Login as bootstrap admin
Role: bootstrap Administrator
Route: `/account/login`
Action: Email `admin@wombat.local`, password from `pwd_DO_NOT_COMMIT.txt`. Click `Sign in`.
Expected: Redirect to `/` (AdministratorDashboard). Welcome banner reads "Welcome, admin@wombat.local / Viewing as Administrator".
Actual: Login form has Email + Password + Remember me + Sign in. Submit → `/` loads as Dashboard. Header reads `admin@wombat.local` (link to `/account/profile`) and a `Sign out` button — no "Viewing as Administrator" subtitle.
Gap: Wording mismatch only — scenario's expected welcome string ("Welcome, admin@wombat.local / Viewing as Administrator") is not how the dashboard header is composed today. Functionally login + role landing is correct.

### Step 1.2 — Issue invitation for Prof Mbatha
Role: bootstrap Administrator
Route: `/admin/invitations/new`
Action: Email `mbatha@kgk.wombat.local`; First name `Nolwazi`; Last name `Mbatha`; Target role `Administrator`; Institution — leave blank (global admin, see Step 1.3 note below); click `Send invitation`.
Expected: Invitation row appears in `/admin/invitations` list with status `Pending`.
Actual: **Route `/admin/invitations/new` does not exist.** The "Issue invitation" form is embedded directly inside `/admin/invitations` (list page), beside an "Active invitations" panel. Form fields: Email (required), Role (combobox), Institution (required combobox), Speciality (disabled until institution chosen), Sub-speciality (disabled). **No First name / Last name fields.** Submit button is labelled `Issue invitation`, not `Send invitation`.
Gap: **Three hard gaps that break the step as written:**
1. Route is `/admin/invitations` (form embedded), not `/admin/invitations/new`.
2. Role combobox options: `InstitutionalAdmin / SpecialityAdmin / SubSpecialityAdmin / Coordinator / CommitteeMember / Assessor / Trainee` — **`Administrator` is not selectable.** Confirms the CLAUDE.md note that global Administrator is manual-only and cannot be assigned via SSO; the invitation UI honors the same rule.
3. Institution is required on every invitation — you cannot "leave blank for global admin". So **Phase 1.A and Phase 1.B must swap order**: Prof Mbatha must (a) be created against an existing institution as `InstitutionalAdmin`, or (b) created by a different mechanism (admin Identity provisioning).
**Recommended scenario rewrite:** demote Prof Mbatha to `InstitutionalAdmin`, create the institution as bootstrap admin first (Phase 1.B), then issue the invitation. The CLAUDE.md note that "the Administrator role cannot be assigned via SSO — always requires explicit manual assignment" applies to the invitation flow too.

> **Note on Administrator scope:** Wombat's `Administrator` role is global per CLAUDE.md. Prof Mbatha is the *departmental* lead but her Wombat role is Administrator because she needs to define the institutional structure and curriculum. In a multi-institution deployment this would be `InstitutionalAdmin` instead; here we use global Administrator so the scenario exercises the full admin surface.

### Step 1.3 — Prof Mbatha accepts invitation
Role: PendingTrainee (invitation recipient flow)
Route: invitation link from email (in dev: check logs or copy token from DB)
Action: Open the link, fill `First name`, `Last name`, set password, confirm, submit.
Expected: Redirect to login. Log in as `mbatha@kgk.wombat.local`. Lands on `/` as Administrator.
Actual: Not exercised (blocked upstream by Step 1.2 — no invitation can be issued for an Administrator role).
Gap: Not auditable until 1.2 sequencing is fixed. Recommend playing this with the `InstitutionalAdmin` role swap above; the accept-invitation page itself is reachable and the flow is plausibly functional.

> **Dev-mode note:** `DevUserSeeder` does not create Prof Mbatha. Either use the invitation flow above (exercises email-less invitation tokens), or extend `DevUserSeeder` to add her. **Scenario choice:** use the invitation flow — it validates Wombat's canonical onboarding. If the flow fails or requires SMTP that's not wired, fall back to the bootstrap admin and note the gap.

## Phase 1.B — Institutional structure

### Step 1.4 — Create institution
Role: Administrator (Prof Mbatha from here on unless noted)
Route: `/admin/institutions/new`
Action: Name `Kgosi Kgari Teaching Hospital`; Short code `KGK`; Contact email `paeds-admin@kgk.wombat.local`; click `Save`.
Expected: Redirect to `/admin/institutions/{id}`. Institution renders in `/admin/institutions` list with status `Active`.
Actual: Route exists. Form fields exactly match: Name (required), Short code (required), Contact email (optional). Save button labelled `Save`. The list page's create-button is labelled `Create institution` (not `New institution`).
Gap: None — clean.

### Step 1.5 — Create speciality
Role: Administrator
Route: `/admin/institutions/{id}/specialities/new` (follow the `Manage specialities` link from the institution detail page)
Action: Name `Paediatrics`; Description `Care of infants, children, and adolescents up to 18 years.`; click `Save`.
Expected: Redirect to the speciality edit page. `Paediatrics` appears in the speciality list for Kgosi Kgari.
Actual: Route exists. Fields: Name (required), Description (optional). Save button present.
Gap: Minor — link from the institutions list is labelled `Specialities`, not `Manage specialities`. There's no link labelled `Manage specialities` anywhere; scenario wording needs a small edit.

### Step 1.6 — Create sub-speciality
Role: Administrator
Route: `/admin/specialities/{specialityId}/sub-specialities/new`
Action: Name `General Paediatrics`; Description `Core general paediatric training; covers the FCPaed(SA) curriculum.`; click `Save`.
Expected: Redirect to the sub-speciality edit page. `General Paediatrics` appears in the sub-speciality list for Paediatrics.
Actual: Route exists. Fields: Name (required), Description (optional). Save button present. Page heading "Create sub-speciality" + parent-speciality breadcrumb correct.
Gap: None — clean.

## Phase 1.C — Entrustment scale

Wombat's `EntrustmentScale` entity holds the institution's rating ladder. Paediatrics will use a 5-level ten Cate-derived scale.

### Step 1.7 — Create entrustment scale
Role: Administrator
Route: `/admin/entrustment-scales/new` *(verify this route exists; it may be `/admin/scales/new` or embedded in the Forms surface — see gap)*
Action: Name `Paed General Entrustment Scale`; Description `Default 5-level entrustment scale for FCPaed(SA) Part 1.`; add five levels in order:
1. Order 1, Label `Observation only`, Description `Trainee observes; does not participate actively.`
2. Order 2, Label `Direct supervision`, Description `Trainee performs with assessor physically present.`
3. Order 3, Label `Indirect supervision`, Description `Trainee performs independently; assessor available nearby.`
4. Order 4, Label `Unsupervised`, Description `Trainee performs unaided; assessor reviews outcomes.`
5. Order 5, Label `Can supervise others`, Description `Trainee is competent to teach and supervise junior colleagues.`
Expected: Scale appears in the scales list with 5 levels. The scale is selectable from any assessment form or activity type that references entrustment.
Actual: **No admin route exists for entrustment-scale creation.** `/admin/entrustment-scales`, `/admin/scales`, and similar candidates all 404. `EntrustmentScale` entity (`Wombat.Domain.Epas.EntrustmentScale`) and `EntrustmentScales` table exist; `GetEntrustmentScalesListQuery` is consumed by `FormEdit.razor` and `ReviewDetail.razor` (read-only dropdowns). The only writer in the codebase is `DataSeeder.cs:113-133`, which seeds a single 5-level scale at boot.
Gap: **Hard gap confirmed.** Prof Mbatha cannot define her own Paed-specific scale through the UI. Three options:
1. **Reuse the seeded scale** — already 5 levels, ten-Cate-flavored, labels close enough; Paediatrics adopts the global one. Scenario step becomes a no-op.
2. **Extend `DataSeeder`** to write the Paed-specific scale (developer task, out of scope for an operator runbook).
3. **Build an admin surface** — new task. Probably warranted; the rewrite plan never explicitly closed this gap. Recommend opening a backlog item: "Admin CRUD for `EntrustmentScale` + `EntrustmentLevel`".
**Scenario impact:** strike Step 1.7 from the runbook until option 3 lands, and document option 1 as the operator workaround in the meantime.

## Phase 1.D — Define 15 General Paediatrics EPAs

Each EPA gets a code, title, description, category (Core or Elective), and a minimum entrustment level that a registrar must reach by graduation.

### Step 1.8 — Bulk-define 15 EPAs

Role: Administrator
Route: `/admin/epas/new` (repeat for each)
Action: For each row in the table below, navigate to the new-EPA form, fill Sub-speciality (`General Paediatrics`), Code, Title, Description, Category, then `Save`.
Expected: Each EPA appears in `/admin/epas` list, scoped to `General Paediatrics`, with status `Active` and version 1 (pending its first curriculum reference).
Actual: Route exists. Form fields: Sub-speciality (required combobox showing `Institution / Speciality / Sub-speciality` triple-path label), Code (required), Title (required), Description (optional), **Required knowledge and skills** (optional — extra field not in scenario), Category (required combobox: Core / Elective). Save button present.
Gap: Minor — scenario doesn't mention the `Required knowledge and skills` field. Leave blank or use for the freeform description; non-blocking.

**The 15 EPAs:**

| Code | Title | Category | Domain (CanMEDS) |
|---|---|---|---|
| PAED-001 | Clerk, assess and present an acute general paediatric admission | Core | Medical Expert |
| PAED-002 | Lead paediatric resuscitation (basic & advanced) | Core | Medical Expert |
| PAED-003 | Perform growth and developmental assessment | Core | Medical Expert |
| PAED-004 | Assess and stabilise a neonate | Core | Medical Expert |
| PAED-005 | Manage common paediatric infections (pneumonia, gastro, UTI) | Core | Medical Expert |
| PAED-006 | Follow up children with chronic conditions (asthma, epilepsy, T1DM) | Core | Medical Expert / Scholar |
| PAED-007 | Assess and manage a child with severe acute malnutrition | Core | Medical Expert / Health Advocate |
| PAED-008 | Conduct a structured adolescent consultation | Core | Communicator |
| PAED-009 | Identify and respond to child safeguarding concerns | Core | Health Advocate / Professional |
| PAED-010 | Perform a lumbar puncture in an infant or child | Core | Medical Expert |
| PAED-011 | Obtain IV access in an infant | Core | Medical Expert |
| PAED-012 | Communicate a serious diagnosis to a child and caregivers | Core | Communicator |
| PAED-013 | Lead a multi-disciplinary case discussion | Core | Collaborator / Leader |
| PAED-014 | Present critically appraised literature at a journal club | Elective | Scholar |
| PAED-015 | Design and deliver a quality improvement project | Elective | Scholar / Leader |

(Full descriptions live inline in the EPA records; the table shows headline only for scannability. The scenario treats the Description field as freeform.)

> **Deferred decision note:** whether to also stamp per-stage minimum levels on each EPA is a CMSA call. In this scenario we store the minimum level on the *curriculum item*, not the EPA directly — the EPA's own minimum-level field is left at its default. This aligns with T034's "stage-indexed supervision levels" design.

## Phase 1.E — Create curriculum and its items

### Step 1.9 — Create the curriculum
Role: Administrator
Route: `/admin/curricula/new`
Action: Sub-speciality `General Paediatrics`; Name `FCPaed(SA) Part 1`; Version `2026.1`; Effective from `2026-01-15`; Effective to leave empty; click `Save`.
Expected: Redirect to `/admin/curricula/{id}`. Curriculum row appears in `/admin/curricula` with status `Active`, 0 items.
Actual: Route exists. Fields: Sub-speciality (required combobox), Name (required), Version (required), Effective from (date, defaults to today), Effective to (date, optional). Save button present.
Gap: None — clean. Note: list page columns include `Items` count and `Status`, both match the scenario expectation.

### Step 1.10 — Add 15 curriculum items
Role: Administrator
Route: `/admin/curricula/{id}/items`
Action: For each EPA below, fill the `Add item` form with the values shown, then click `Add item`. Repeat.
Expected: After all 15 are added, the `Existing items` table lists all 15 rows. Progress bars in the Trainee dashboard will later reference these values.
Actual: Route exists. `Add item` form has all 6 fields the scenario relies on: EPA (required combobox), Required count (spinbutton, default 1), Minimum level (spinbutton, default 4), Per-stage minima (JSON textbox with placeholder `{"1":2,"2":3,"3":4}` — exactly the syntax the scenario prescribes), Window months (spinbutton, default 12), Weight (spinbutton). `Existing items` table already shows the seeded IM-Core entry above the form.
Gap: None — clean.

**The 15 curriculum items:**

| EPA | Required count | Final-year min level | Per-stage min levels (yr 1/2/3/4) | Window (months) | Weight |
|---|---|---|---|---|---|
| PAED-001 | 30 | 4 (Unsupervised) | 2 / 3 / 4 / 4 | 12 | 3.0 |
| PAED-002 | 8 | 4 | — / 2 / 3 / 4 | 24 | 4.0 |
| PAED-003 | 15 | 4 | 2 / 3 / 4 / 4 | 12 | 2.0 |
| PAED-004 | 20 | 4 | 2 / 3 / 4 / 4 | 24 | 3.0 |
| PAED-005 | 25 | 4 | 2 / 3 / 4 / 4 | 12 | 2.5 |
| PAED-006 | 20 | 4 | — / 2 / 3 / 4 | 24 | 2.5 |
| PAED-007 | 10 | 4 | — / 2 / 3 / 4 | 24 | 3.0 |
| PAED-008 | 12 | 4 | 2 / 3 / 4 / 4 | 24 | 2.0 |
| PAED-009 | 6 | 4 | — / 2 / 3 / 4 | 48 | 3.0 |
| PAED-010 | 10 | 4 | — / 2 / 3 / 4 | 24 | 3.5 |
| PAED-011 | 30 | 4 | 2 / 3 / 4 / 4 | 12 | 1.5 |
| PAED-012 | 8 | 4 | — / 2 / 3 / 4 | 24 | 3.5 |
| PAED-013 | 6 | 4 | — / 2 / 3 / 4 | 24 | 2.0 |
| PAED-014 | 4 | 3 | — / — / 2 / 3 | 48 | 1.5 |
| PAED-015 | 1 | 3 | — / — / 2 / 3 | 48 | 2.0 |

Per-stage minima are entered as the JSON the form accepts, e.g. for PAED-001: `{"1":2,"2":3,"3":4,"4":4}`. Entries with a dash indicate not required at that stage.

Window months reflect how long a rating counts toward the requirement before it expires. Weight drives dashboard ordering (higher = shown first).

## Phase 1.F — Build the 10 activity types

Each activity type bundles three jsonb blobs: form schema, workflow state machine, credit rules. Prof Mbatha builds them via the visual Activity-Type Builder (`/admin/activity-types/{id}` — tabs: Metadata, Form, Workflow, Credit).

We fully specify one activity type (Mini-CEX) as a worked example, then summarise the other nine.

### Step 1.11 — Build Mini-CEX (worked example)
Role: Administrator
Route: `/admin/activity-types/new`

**1.11.a — Metadata tab**
Action: Key `mini_cex_paed`; Name `Mini-CEX (Paediatrics)`; Scope `Speciality`; Scope Id `<Paediatrics speciality id>`; Description `Brief (~20-minute) observed clinical encounter rated on six domains.`; Active checkbox on. Click `Save draft`.
Expected: Draft saved; status banner "Draft saved." Metadata persists across tabs.
Actual: Tab present. Fields: Key (required), Name (required), Scope (required combobox: Global / Institution / Speciality / SubSpeciality), Scope Id (numeric spinbutton), Description (optional), Active (checkbox, default on). `Save draft` button visible at top of page.
Gap: **Scope Id is a raw integer spinbutton, not a picker.** Prof Mbatha has to memorize or look up the numeric `SpecialityId` of her newly-created Paediatrics speciality. The speciality list pages don't display IDs in the URL bar in a way she can read off the breadcrumb easily — she'd need to inspect a URL on the edit page. UX friction; consider replacing with a context-aware picker that filters by selected Scope.

**1.11.b — Form tab**
Action: Click `Add section` → edit the section: Key `encounter`, Title `Encounter details`. Add fields (via `Add field` inside the section):
- Key `epa_id`, Label `EPA`, Type `Epa`, Required on.
- Key `assessor_user_id`, Label `Assessor`, Type `User`, Required on.
- Key `setting`, Label `Clinical setting`, Type `Choice`, Options `Inpatient\nOutpatient\nEmergency\nHigh care\nNeonatal`, Required on.
- Key `patient_age_months`, Label `Patient age (months)`, Type `Number`, Required on.
- Key `presenting_complaint`, Label `Presenting complaint`, Type `LongText`, Required on.
Click `Add section` again for the ratings: Key `ratings`, Title `Clinical performance ratings`. Add six fields each Type `Scale`, Required on, Catalogue key `paed-entrust-5`:
- `history_taking` / History taking
- `examination` / Physical examination
- `clinical_reasoning` / Clinical reasoning
- `communication` / Communication
- `professionalism` / Professionalism
- `overall_level` / Overall entrustment level
Click `Add section`: Key `feedback`, Title `Feedback`. Add field Key `narrative`, Label `Narrative feedback (strengths and next steps)`, Type `LongText`, Required on.
Expected: Live preview renders the three sections with all 13 fields. EPA and Assessor fields show pickers; Scale fields show the 5-level selector.
Actual: Builder loads with a default `details` section + single `title` Text field; Add section + Add field both present. Section editor (Section key + Title) and Field editor (Field key, Label, Type combobox, Help text, Options, Catalogue key, Required) all present. Field Type options: Text, **Long text** (label has a space; scenario writes `LongText`), Number, Date, Choice, Multi-choice, Scale, User, EPA, Likert, Procedure reference, File, Signature — every scenario-referenced type exists. Live preview pane renders to the right.
Gap: Cosmetic — scenario says Type `LongText`; UI option label is `Long text`. Catalogue key field is a plain textbox, so `paed-entrust-5` will only be meaningful if an `EntrustmentScale` exists with that catalogue key (linked to Step 1.7's gap).

**1.11.c — Workflow tab**
Action: Paste the following into the `Workflow JSON` textarea:
```json
{
  "initial": "draft",
  "states": [
    { "key": "draft",     "label": "Draft",      "terminal": false },
    { "key": "submitted", "label": "Submitted",  "terminal": false },
    { "key": "rated",     "label": "Rated",      "terminal": false },
    { "key": "completed", "label": "Completed",  "terminal": true  }
  ],
  "transitions": [
    { "key": "submit",   "from": ["draft"],     "to": "submitted", "actor_roles": ["Trainee"] },
    { "key": "accept",   "from": ["submitted"], "to": "rated",     "actor_field": "assessor_user_id" },
    { "key": "complete", "from": ["rated"],     "to": "completed", "actor_field": "assessor_user_id" },
    { "key": "recall",   "from": ["submitted"], "to": "draft",     "actor_roles": ["Trainee"] }
  ]
}
```
Expected: Validation passes. Save draft. Tab turns clean.
Actual: Tab present, single `Workflow JSON` textbox. Default placeholder uses a different schema than the scenario specifies. Checked `Wombat.Domain.Activities.Workflow.WorkflowParser` (whitelisted property names: `version`, `initial_state`, `states`, `transitions`; transition keys: `key`, `from`, `to`, `actor`, `requires_note`, `requires_fields`) and `ActorRuleParser` (actor grammar is a DSL string, not a roles array).
Gap: **Scenario JSON would be rejected by the parser.** Key differences:
- Root: scenario uses `initial`, parser requires `initial_state` and a top-level `version` integer.
- Transition `from`: scenario wraps in arrays; parser also accepts strings (so the array form is fine for `from`).
- Transition actor: scenario uses `actor_roles: ["Trainee"]` and `actor_field: "assessor_user_id"`; parser requires a single `actor` field whose value is a DSL string like `"role:Trainee"`, `"field:assessor_user_id"`, `"subject"`, `"creator"`, `"scope:<name>"`, combined with `+` (all) or `|` (any).

**Corrected Mini-CEX workflow JSON for the scenario:**
```json
{
  "version": 1,
  "initial_state": "draft",
  "states": [
    { "key": "draft",     "label": "Draft" },
    { "key": "submitted", "label": "Submitted" },
    { "key": "rated",     "label": "Rated" },
    { "key": "completed", "label": "Completed", "terminal": true }
  ],
  "transitions": [
    { "key": "submit",   "from": "draft",     "to": "submitted", "actor": "role:Trainee" },
    { "key": "accept",   "from": "submitted", "to": "rated",     "actor": "field:assessor_user_id" },
    { "key": "complete", "from": "rated",     "to": "completed", "actor": "field:assessor_user_id" },
    { "key": "recall",   "from": "submitted", "to": "draft",     "actor": "role:Trainee" }
  ]
}
```
Replace the example in the scenario with this version before playing. Apply the same `actor: "role:..."` / `actor: "field:..."` translation to the nine summarised types in Step 1.12.

**1.11.d — Credit tab**
Action: Paste:
```json
{
  "counts_for": [
    {
      "curriculum_item_match": { "epa_field": "epa_id" },
      "minimum_level_field": "overall_level",
      "amount": 1
    }
  ]
}
```
Expected: Validation passes. Save draft.
Actual: Tab present, single `Credit rules JSON` textbox. Default value `{ "counts_for": [] }`. Verified `CreditRulesParser` accepts exactly the keys the scenario uses (`counts_for`, `curriculum_item_match`, `epa_field` / `curriculum_item_id` / `curriculum_item_field`, `amount`, `minimum_level_field`, `minimum_level_fixed`).
Gap: None — the scenario JSON is valid as-written.

**1.11.e — Publish**
Role: Administrator
Route: `/admin/activity-types/{id}`
Action: Click `Publish`.
Expected: Status banner "Published version 1." Type appears in `/admin/activity-types` as `v1 / None (no draft) / Active`.
Actual: `Publish` button is in the page header beside `Save draft`, **but it only renders when `_editor.HasDraft == true`** (`ActivityTypeEdit.razor:14-18`). The flow is therefore: edit any field → `Save draft` → Publish button appears → click. Re-visiting the page after a clean publish hides the button. There's also a `Discard draft` companion that appears under the same condition.
Gap: Cosmetic only — scenario wording implies Publish is always visible. Either reword the step to "after Save draft, click the now-visible `Publish` button," or surface the button unconditionally with a disabled state when there's nothing to publish.

### Step 1.12 — Build the other 9 activity types (summary)

For each of the nine remaining types, repeat Step 1.11 structure (Metadata → Form → Workflow → Credit → Publish). Minimal specs:

| Key | Name | Schema focus | Workflow delta from Mini-CEX | Credit rule |
|---|---|---|---|---|
| `cbd_paed` | Case-based Discussion | Similar 6 ratings + case summary longtext | Same 4-state workflow | Same: one credit per completion meeting min level |
| `acat_paed` | Acute Care Assessment Tool | Acute scenario summary + 8-criteria rating matrix | Same | Same |
| `dops_paed` | Direct Observation of Procedural Skills | Procedure code (Choice), indication, complications, 5-step rating | Same | Same |
| `procedure_log_paed` | Procedure Log | Procedure code, supervision level (Choice), self-rated competence | `draft → logged (terminal)`; no assessor | Credits only when supervision-level meets min |
| `msf_paed` | Multi-Source Feedback | Self-rating + 8 invitee questions (LongText each) | `draft → open → closing → closed (terminal)` over 28 days | One credit on `closed` state |
| `reflective_note_paed` | Reflective Note | STAR structure (situation/task/action/result) | `draft → submitted (terminal)` | No curriculum credit by default; weights PAED-012 if linked |
| `journal_club_paed` | Journal Club Presentation | Article citation, summary, critique, slides reference | `draft → submitted → reviewed (terminal)` | Credits PAED-014 |
| `research_output_paed` | Research Output | Type (poster/paper/protocol), citation, role, reflective summary | `draft → submitted → verified (terminal)` | Credits PAED-015 |
| `teaching_session_paed` | Teaching Session | Audience, topic, duration, learning objectives, feedback summary | `draft → delivered (terminal)` | Credits PAED-013 weight when reviewed |

Each worked through the builder UI exactly as Mini-CEX; ~15 minutes per type once Prof Mbatha has the first one done. Total time on this phase: ~2.5 hours.

> **Gap hook (confirmed live):** the builder's Workflow tab rejects the JSON shape the scenario originally wrote — see Step 1.11.c's `Actual:` block for the corrected schema. The Credit tab's JSON shape is accepted as-written. The Form-tab visual builder is the only surface that exercises every field type, but JSON-driven types are validated separately at save time.

> **Seeded overlap:** `DataSeeder` already publishes 10 activity types against `General Internal Medicine` (ACAT, CbD, DOPS, Journal Club, Mini-CEX, Procedure Log, QI Project, Reflective Note, Research Output, Teaching Session). Scenario's intended set substitutes `MSF` for `QI Project` and prepends `_paed` to every key. Because the seeded types are speciality-scoped to General Internal Medicine, they will not surface in Paediatric users' selectors — no interference. Operator still has to build all 10 Paed types from scratch.

### Step 1.13 — Verify all 10 activity types are published
Role: Administrator
Route: `/admin/activity-types`
Action: Scroll through list. Confirm 10 entries with `v1 / Active` status and `Speciality` scope. Optionally filter by name.
Expected: 10 rows. Names match the table above.
Actual: List page has columns Name / Key / Scope / Published / Draft / Status, a Search textbox, and a `New activity type` link. Seeded rows render as `<Name> / <key> / Speciality / v1 / None / Active / Edit`. The "Edit" link is the only per-row action.
Gap: None — list semantics match. Note: the `Published` column shows `v1` (not the scenario's `v1 / None (no draft) / Active`); `Draft` and `Status` are separate columns. Scenario wording can be tightened but the data is all present.

## Act 1 outcome state

After Act 1 completes cleanly, the database contains:
- 1 institution (`Kgosi Kgari Teaching Hospital`).
- 1 speciality (`Paediatrics`).
- 1 sub-speciality (`General Paediatrics`).
- 1 entrustment scale (`Paed General Entrustment Scale`, 5 levels) — assuming Step 1.7 worked; see gap.
- 15 EPAs scoped to General Paediatrics.
- 1 curriculum (`FCPaed(SA) Part 1` v2026.1) with 15 items.
- 10 published activity types scoped to Paediatrics speciality.
- 1 Administrator user (Prof Mbatha) via the invitation flow (or fallback to bootstrap admin).

Nothing has been asked of consultants or registrars yet. No activities exist. No committee panel exists (panels depend on users, which are onboarded in Act 2).

## Act 1 time estimate

| Phase | Est. minutes |
|---|---|
| 1.A: Prof Mbatha provisioned | 10 |
| 1.B: Institution + speciality + sub-speciality | 10 |
| 1.C: Entrustment scale | 5 |
| 1.D: 15 EPAs | 25 (≈90s each) |
| 1.E: Curriculum + 15 items | 20 |
| 1.F: 10 activity types | 90 (15 min × 6 once the pattern is known + worked example time) |
| **Total** | **~160 minutes** (the target "90 minutes" in the plan was optimistic; revise in the plan after playing) |

## Act 1 findings summary

Populated 2026-05-24 from a Playwright route-and-surface audit (no full play-through; UX-friction findings still need a human pass).

### Hard gaps (block the step as written)

1. **Step 1.2 — Invitation flow cannot issue an `Administrator` role.** Route is `/admin/invitations` (form embedded in list page), not `/admin/invitations/new`; Role combobox does not include Administrator; Institution is required. Phase 1.A and Phase 1.B must swap — institution exists before invitation. Demote Prof Mbatha to `InstitutionalAdmin` for the scenario to be playable as authored.
2. **Step 1.7 — No admin UI for `EntrustmentScale`.** Entity, table, and read-side query all exist; the only writer is `DataSeeder`. Operator must either reuse the seeded scale or wait for an admin-CRUD surface to be built. Open as a new backlog item.
3. **Step 1.11.c — Workflow JSON schema in the scenario is rejected by `WorkflowParser`.** Corrected JSON in the step's `Actual:` block; the same translation (`actor_roles` → `actor: "role:..."`, `actor_field` → `actor: "field:..."`, top-level `initial` → `initial_state`, add `version: 1`) applies to all 9 summarised types in Step 1.12.

### Route mismatches (rewrite the step's `Route:` line)

- 1.2: `/admin/invitations/new` → `/admin/invitations` (form embedded).
- 1.5: "Manage specialities" link → labelled `Specialities` on the institutions list, not `Manage specialities`.
- 1.11.e: Publish button is in the page header alongside Save draft (visible only when there's an unsaved draft) — not a separate route action.

### UX friction (cosmetic; non-blocking)

- Login banner wording differs from scenario ("Welcome, X / Viewing as Administrator" doesn't exist; header just shows the email).
- Field-type label in builder is `Long text` (with space), scenario uses `LongText`.
- Metadata tab's `Scope Id` is a raw integer spinbutton — operator must look up the numeric ID. Strong UX win if replaced with a context-aware picker (filter by selected Scope).
- Activity-type list status column reads `v1 / None / Active` across three columns, not the single concatenated string the scenario expected.

### Missing features (warrant a backlog item)

- **Admin CRUD for `EntrustmentScale` + `EntrustmentLevel`** (driven by Step 1.7).
- **First name / Last name fields on invitation** — currently email-only; the accept-invitation flow probably collects them, but it would be friendlier on the invitation side too.
- **Allow null / `Global` Institution on invitation** for `Administrator` role only, gated by the existing manual-only rule.

### Time-estimate revisions

- Audit took ~15 minutes total, vs. the ~160 minutes the runbook estimates for a manual play-through. The estimates in §"Act 1 time estimate" are still the right shape for a human run; no change needed there. Phase 1.F's 90-minute estimate is conservative — the visual builder is well-paced, but the JSON tabs (Workflow, Credit) take longer than 15 min/type because of the schema-translation work above.

### What still needs a human play-through

The audit only walks routes and inspects form fields against the spec — it does not exercise data persistence, conditional visibility, validation errors, or real cross-page flow. Phase 1.D's 15 EPA bulk add, Phase 1.E's 15 curriculum items, and Phase 1.F's three JSON pastes per type are all still untested end-to-end. The findings above clear the obvious blockers so a human pass would catch real persistence / validation regressions, not paper cuts.

## Handoff into Act 2

Act 2 will need from Act 1:
- The Paediatrics institution, speciality, and sub-speciality IDs (record in the findings summary when played).
- The curriculum ID for FCPaed(SA) Part 1 v2026.1.
- The 10 activity type IDs (for reference when Act 3 has registrars submit specific types).
- Prof Mbatha's UserId (she'll be the one issuing consultant invitations in Act 2).

---

# Acts 2–5 — to be drafted

Act 1 establishes the document shape. Once played and the format is confirmed usable, draft Acts 2–5 with the same conventions.

**Act 2 — Day 1 team onboarding** (est. 400 lines): invitation flow for 6 consultants + 5 registrars, admission of PendingTrainee → Trainee, profile creation, committee panel creation (depends on consultants existing).

**Act 3 — Months 1–6 operational rhythm** (est. 800 lines, longest act): 8 sub-scenes — Mini-CEX submit/accept/rate/credit, Procedure log, DOPS, MSF cycle, stalled-assessment triage, audit-log verification, dashboard populated-state sanity.

**Act 4 — Month 12 annual review** (est. 400 lines): 5 reviews scheduled, committee evidence bundle, decisions recorded, STARs staged, ratification, one appeal.

**Act 5 — Year 4 graduation** (est. 250 lines): final review for Dr Molefe, multiple STARs, portfolio PDF export, profile completion.

**Appendix — cross-cutting concerns** (est. 300 lines): data rights, scheduled jobs, SSO path, mobile/accessibility spot-checks.

Not drafted here. Draft after Act 1 has been played once and any format revisions folded back.
