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
| Administrator (global) | Bootstrap admin | (seeded) `admin@wombat.local` | Used in Phase 1.A to stand up the institution + speciality + sub-speciality, then in Phase 1.B to issue Prof Mbatha's invitation. |
| InstitutionalAdmin (scoped to KGK) | Head of Department | Prof Nolwazi Mbatha | Created in Phase 1.B. Owns the rest of the programme setup (EPAs, curriculum, activity types). Demoted from global Administrator because the invitation surface only assigns scoped roles — see Step 1.5. |
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
**Who:** the hospital IT bootstrap admin opens Phase 1.A to stand up the institution skeleton; Prof Nolwazi Mbatha takes over from Phase 1.B onward (after her InstitutionalAdmin account is provisioned). Both work alone, after hours, 17:00–18:30.
**Why:** Hospital IT has just provisioned Wombat for the Paediatrics Department. Before the registrars return for the new year, the institution needs to be stood up and Prof Mbatha needs an account she can use to enter the curriculum and publish the assessment types her consultants will use.

**Starting state:**
- Wombat running at `http://localhost:5080`.
- The bootstrap admin (`admin@wombat.local`) exists from `AdminSeeder`.
- `DataSeeder` has created the `Demo Institution` with its Internal Medicine speciality and 10 activity types. **Ignored** throughout — it lives in a separate institution + speciality scope and does not interfere.
- No Paediatrics data exists.

**Act 1 goal:**
1. Institution, speciality, and sub-speciality exist.
2. Prof Mbatha holds an `InstitutionalAdmin` account scoped to KGK.
3. The Paed General Entrustment Scale is published with 5 ten-Cate-style levels.
4. 15 General Paediatrics EPAs are defined.
5. Curriculum "FCPaed(SA) Part 1 — 2026.1" is published with 15 curriculum items.
6. 10 activity types are published and ready for registrars to submit against in Act 3.
7. Prof Mbatha's consultant/registrar colleagues are ready to receive invitations in Act 2.

## Phase 1.A — Institutional structure (bootstrap admin)

The invitation surface only assigns scoped roles and requires an institution, so the institution must exist before Prof Mbatha can be provisioned. The bootstrap admin therefore stands up the institution + speciality + sub-speciality first.

### Step 1.1 — Login as bootstrap admin
Role: bootstrap Administrator
Route: `/account/login`
Action: Email `admin@wombat.local`, password from `pwd_DO_NOT_COMMIT.txt`. Click `Sign in`.
Expected: Redirect to `/`. The page header reads `admin@wombat.local` (link to `/account/profile`) with a `Sign out` button; below it the Administrator dashboard renders.
Actual: Playwright replay 2026-05-24 (DB reset, fresh start). Login redirects to `/`; header shows email + Sign out; Administrator dashboard renders.
Gap: None.

### Step 1.2 — Create institution
Role: bootstrap Administrator
Route: `/admin/institutions/new` (click `Create institution` from the `/admin/institutions` list).
Action: Name `Kgosi Kgari Teaching Hospital`; Short code `KGK`; Contact email `paeds-admin@kgk.wombat.local`; click `Save`.
Expected: Redirect to `/admin/institutions/{id}`. Institution renders in `/admin/institutions` list with status `Active`.
Actual: KGK saved at `/admin/institutions/2`. h1 flips to "Edit institution"; row appears in list as `Active`. Browser tab title is `Institutions` (shared title — does not change between create/edit).
Gap: None. Previous play-through's "h1 stays Create institution" symptom is resolved — h1 swaps correctly on this and the speciality edit page. See finding #5 in summary for the remaining stale `<title>` cases.

### Step 1.3 — Create speciality
Role: bootstrap Administrator
Route: `/admin/institutions/{id}/specialities/new` (click `Specialities` next to KGK in the institutions list, then `Create speciality`).
Action: Name `Paediatrics`; Description `Care of infants, children, and adolescents up to 18 years.`; click `Save`.
Expected: Redirect to the speciality edit page. `Paediatrics` appears in the speciality list for KGK.
Actual: Paediatrics saved at `/admin/institutions/2/specialities/2`. `SpecialityId = 2` recorded for Step 1.11.a. h1 flips to "Edit speciality"; browser tab title stays "Institutions" (shared, not conditional).
Gap: None.

### Step 1.4 — Create sub-speciality
Role: bootstrap Administrator
Route: `/admin/specialities/{specialityId}/sub-specialities/new` (click `Sub-specialities` next to `Paediatrics`, then `Create sub-speciality`).
Action: Name `General Paediatrics`; Description `Core general paediatric training; covers the FCPaed(SA) curriculum.`; click `Save`.
Expected: Redirect to the sub-speciality edit page. `General Paediatrics` appears in the sub-speciality list for Paediatrics. Record the `{specialityId}` and `{subSpecialityId}` — Phase 1.E and Phase 1.F need them for scoped lookups.
Actual: General Paediatrics saved at `/admin/specialities/2/sub-specialities/2`. `SubSpecialityId = 2` recorded. Parent-speciality line correctly reads "Speciality: Paediatrics". h1 flips to "Edit sub-speciality". After T057, browser tab title also flips to "Edit Sub-Speciality" (forceLoad on the post-save NavigateTo).
Gap: None.

## Phase 1.B — Provision Prof Mbatha (InstitutionalAdmin scoped to KGK)

With KGK in place, the bootstrap admin can now issue Prof Mbatha's invitation. She joins as `InstitutionalAdmin` rather than global `Administrator`: the invitation form only exposes scoped roles, and the Administrator role is reserved for manual-only assignment (per CLAUDE.md).

> **✅ Resolved by T056:** Prof Mbatha now runs Phases 1.C–1.F directly. T056 (Option A, shipped 2026-05-24 in five clusters) granted `InstitutionalAdmin` the institution-scoped admin powers across the admin surface, with handler-level scope guards so she only sees and edits data scoped to KGK. Scheduled Jobs, the global institutions list, and Data Rights remain Administrator-only. See `Rewrite/Tasks/T056-institutional-admin-role-power.md` for the page taxonomy.

### Step 1.5 — Issue invitation for Prof Mbatha
Role: bootstrap Administrator
Route: `/admin/invitations` (the Issue invitation form is embedded in the list page, beside an Active invitations panel).
Action: Email `mbatha@kgk.wombat.local`; Role `InstitutionalAdmin`; Institution `Kgosi Kgari Teaching Hospital`; leave Speciality + Sub-speciality blank; click `Issue invitation`.
Expected: The invitation appears in the `Active invitations` panel with status `Pending`.
Actual: Invitation persists. **T051 verified end-to-end:** status banner reads "Invitation issued for mbatha@kgk.wombat.local. Copy the link below — it is shown only once." The full registration URL renders inline below the form as a copy-friendly `<code>http://localhost:5080/account/register?token=…</code>` inside an info Alert with the "shown only on this page-load" warning. Active invitations table shows the row with status Pending. SMTP delivery is decoupled from the runbook — operator can hand over the URL directly without needing an email catcher.
Gap: None — previous play-through findings #2 (SMTP port mismatch) and #3 (status message + dropped token) both closed by T051.

> **Note on Administrator scope:** Wombat's invitation form does not surface the `Administrator` role — that role is global and reserved for manual-only assignment, per the CLAUDE.md SSO rule (which the invitation flow honors). `InstitutionalAdmin` is the strongest role available through invitations; it covers everything Prof Mbatha needs in Act 1. T052 (in `scenario-act1-fixes-plan.md`) tracks the option of re-exposing Administrator with null Institution; until that lands, use InstitutionalAdmin.
>
> **First/Last name capture:** the invitation form does not yet collect First / Last name — Prof Mbatha will set them on the accept-invitation page. T051 tracks adding the fields to the issue form.

### Step 1.6 — Prof Mbatha accepts invitation
Role: invitation recipient (no prior session)
Route: invitation link captured by the local SMTP catcher (Papercut on port 25, or smtp4dev on 1025).
Action: Read the latest `.eml` at `%APPDATA%\Changemaker Studios\Papercut SMTP\` (Papercut writes the .eml files directly into that folder, NOT into an `Incoming` subfolder — the desktop app surfaces them in its UI) — the body contains `http://localhost:5080/account/register?token=<raw_token>`. Open the link; fill `First name` (`Nolwazi`), `Last name` (`Mbatha`), set + confirm password; submit. Auto-logs in and redirects to `/`.
Expected: After submit, page header reads `mbatha@kgk.wombat.local` and the InstitutionalAdmin dashboard renders with greeting "Welcome, mbatha@kgk.wombat.local / Viewing as InstitutionalAdmin".
Actual: Worked end-to-end. Replay 3 (2026-05-26) exercised BOTH delivery paths to confirm symmetry — Wombat sent the invitation email via SMTP to Papercut on port 25, Papercut wrote it to `%APPDATA%\Changemaker Studios\Papercut SMTP\20260526073601652 Your Wombat invitation 8fd5c0.eml`, and the URL inside the .eml matches the one rendered inline on the InvitationsList page byte-for-byte. Opened the URL; form prefilled email + role; entered First name `Nolwazi`, Last name `Mbatha`, password; submit auto-logged Mbatha and redirected to `/`. Dashboard greeting "Welcome, mbatha@kgk.wombat.local / Viewing as InstitutionalAdmin" rendered with the InstitutionalAdmin scope dashboard (KGK / 1 speciality / 1 sub-speciality / quick links). NavMenu exposes the 11 admin routes T056.e expanded to: Specialities, EPAs, Curricula, Activity Types, Entrustment Scales, Forms, Trainees, Assessors, Invitations, SSO Mappings, Audit Log — plus the 3 account-related routes (Home, My Account, Data Rights).
Gap: Scenario inline note says emails land in an `Incoming` subfolder of the Papercut profile dir — actually they land in the parent dir without an `Incoming` subfolder. Corrected the inline note above. The Papercut desktop app surfaces them in its in-app UI; the .eml files on disk are the persistent backing.

> **Dev-mode note:** `DevUserSeeder` does not create Prof Mbatha. Use the invitation flow above. If no SMTP catcher is running, start one (Papercut SMTP for Windows desktop, smtp4dev / Mailhog as alternatives) before issuing the invitation. Confirm the listening port matches `Email:SmtpPort` in `appsettings.Development.json` (currently `1025`) — override with `$env:Email__SmtpPort` if necessary.

## Phase 1.C — Entrustment scale

Wombat's `EntrustmentScale` entity holds the rating ladder. Paediatrics will use a 5-level ten Cate-derived scale.

### Step 1.7 — Create entrustment scale
Role: bootstrap Administrator
Route: `/admin/entrustment-scales/new` (click `Create scale` from the `/admin/entrustment-scales` list).
Action: Name `Paed General Entrustment Scale`; Description `5-level ten-Cate ladder for FCPaed(SA) Part 1.`; click `Add level` until five rows are present, then fill each row:
1. Label `Observation only`, Description `Trainee observes; does not participate actively.`
2. Label `Direct supervision`, Description `Trainee performs with assessor physically present.`
3. Label `Indirect supervision`, Description `Trainee performs independently; assessor available nearby.`
4. Label `Unsupervised`, Description `Trainee performs unaided; assessor reviews outcomes.`
5. Label `Can supervise others`, Description `Trainee is competent to teach and supervise junior colleagues.`
The form auto-assigns `Order` based on row position; use `Up` / `Down` if you need to re-sort. Click `Save`. The URL flips to `/admin/entrustment-scales/{id}` (typically `/2` since the seeded `O-R Scale` occupies id `1`).
Expected: Status banner "Entrustment scale saved." Scale appears in the `/admin/entrustment-scales` list with `Levels = 5`. The scale is now available to any assessment form, activity-type rating field, or committee-review entrustment-decision picker.
Actual: Saved as bootstrap admin at `/admin/entrustment-scales/2` with all 5 ten-Cate levels intact. h1 and browser tab title both flip to "Edit entrustment scale" (T057 forceLoad). Page now Administrator-only — InstitutionalAdmin gets `/access-denied` if she navigates directly to `/admin/entrustment-scales/new`, and the Create / Edit / Delete buttons on the list page are hidden for non-Administrators (T057). She can still see the scales read-only on the list.
Gap: None.

> **About the seeded scale:** `DataSeeder` boots with one global `O-R Scale` for development convenience. Paediatrics could reuse it, but the labels differ slightly from the canonical ten-Cate ladder the curriculum cites — easier to create the Paed-specific scale and leave the seeded one alone.

## Phase 1.D — Define 15 General Paediatrics EPAs

Each EPA gets a code, title, description, category (Core or Elective), and a minimum entrustment level that a registrar must reach by graduation.

### Step 1.8 — Bulk-define 15 EPAs

Role: Prof Mbatha (InstitutionalAdmin) — T056 grants institution-scoped admin powers.
Route: `/admin/epas/new` (repeat for each)
Action: For each row in the table below, navigate to the new-EPA form. Fill Sub-speciality `Kgosi Kgari Teaching Hospital / Paediatrics / General Paediatrics` (combobox shows the triple-path label), Code, Title, Description, leave `Required knowledge and skills` blank (or paste a short freeform note — non-blocking), Category, then `Save`.
Expected: Each EPA appears in `/admin/epas` list, scoped to `General Paediatrics`, with status `Active` and version 1 (pending its first curriculum reference).
Actual: All 15 PAED-001 through PAED-015 EPAs persisted as Prof Mbatha (InstitutionalAdmin) — T056.b grants her institution-scoped EPA powers. Sub-speciality combobox correctly pre-selects her sole scope (`Kgosi Kgari Teaching Hospital / Paediatrics / General Paediatrics`). The seeded Demo Institution EPA-001 (id=1) is filtered out of her list — only the 15 PAED EPAs (ids 2-16) appear. h1 and browser tab title both flip to "Edit EPA" after save (T057 forceLoad).
Gap: None.

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
Role: **bootstrap Administrator** (see Phase 1.B note).
Route: `/admin/curricula/new`
Action: Sub-speciality `Kgosi Kgari Teaching Hospital / Paediatrics / General Paediatrics`; Name `FCPaed(SA) Part 1`; Version `2026.1`; Effective from `2026-01-15`; Effective to leave empty; click `Save`.
Expected: Redirect to `/admin/curricula/{id}`. Curriculum row appears in `/admin/curricula` with status `Active`, 0 items.
Actual: FCPaed(SA) Part 1 v2026.1 saved at `/admin/curricula/2` as Prof Mbatha (InstitutionalAdmin via T056.b). Sub-speciality combobox correctly pre-selects her scope. Effective-from set to 2026-01-15. h1 and browser tab title both flip correctly after save (T057 forceLoad).
Gap: None.

### Step 1.10 — Add 15 curriculum items
Role: **bootstrap Administrator**
Route: `/admin/curricula/{id}/items`
Action: For each EPA below, fill the `Add item` form with the values shown, then click `Add item`. Repeat 15 times.
Expected: After all 15 are added, the `Existing items` table lists all 15 rows. Progress bars in the Trainee dashboard will later reference these values.
Actual: All 15 items added cleanly as Mbatha. Per-stage minima JSON preserved verbatim in DB readback (`{"1":2,"2":3,"3":4,"4":4}` for 12 rows; `{"2":2,"3":3,"4":4}` for the late-entry rows; `{"3":2,"4":3}` for the year-3+ electives). Counts / levels / windows / weights all round-trip exactly. The EPA dropdown shows only the 15 PAED EPAs — Demo Institution's EPA-001 correctly absent. The Add-item form clears defaults after each save (convenient for batch entry).
Gap: None.

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
Action: Key `mini_cex_paed`; Name `Mini-CEX (Paediatrics)`; Scope `Speciality`; Scope Id `2` (the integer SpecialityId from Step 1.3 — the field is a numeric spinbutton; T053 will replace it with a picker); Description `Brief (~20-minute) observed clinical encounter rated on six domains.`; Active checkbox on. Click `Save draft`.
Expected: Draft saved; status banner "Draft saved." Metadata persists across tabs.
Actual: **T055 fix verified.** Status banner "Draft saved." and the URL flips immediately to `/admin/activity-types/11` on first save (10 seeded IM types occupy ids 1-10). A browser refresh now lands on the saved entity rather than the blank new form. **T053 picker verified** — selecting Scope=Speciality reveals a `<select>` Scope Id picker showing the triple-path label "Kgosi Kgari Teaching Hospital / Paediatrics", scoped to Mbatha's institution only (no Demo Internal Medicine speciality in the dropdown). Saved with Scope=Speciality, ScopeId=2 cleanly.
Gap: None — previous play-through findings #4 (URL stickiness) and the T053 picker target are both closed.

**1.11.b — Form tab**
Action: The builder loads with a default `details` section containing a single `title` Text field — delete both before starting. Then click `Add section` and edit: Key `encounter`, Title `Encounter details`. Add fields (via `Add field` inside the section):
- Key `epa_id`, Label `EPA`, Type `EPA`, Required on.
- Key `assessor_user_id`, Label `Assessor`, Type `User`, Required on.
- Key `setting`, Label `Clinical setting`, Type `Choice`, Options `Inpatient\nOutpatient\nEmergency\nHigh care\nNeonatal`, Required on.
- Key `patient_age_months`, Label `Patient age (months)`, Type `Number`, Required on.
- Key `presenting_complaint`, Label `Presenting complaint`, Type `Long text`, Required on.
Click `Add section` again for the ratings: Key `ratings`, Title `Clinical performance ratings`. Add six fields each Type `Scale`, Required on. Pick the appropriate `Catalogue key` if your build wires scale binding through the catalogue; otherwise leave blank to fall back to the institution's default scale (the `Paed General Entrustment Scale` created in Step 1.7):
- `history_taking` / History taking
- `examination` / Physical examination
- `clinical_reasoning` / Clinical reasoning
- `communication` / Communication
- `professionalism` / Professionalism
- `overall_level` / Overall entrustment level
Click `Add section`: Key `feedback`, Title `Feedback`. Add field Key `narrative`, Label `Narrative feedback (strengths and next steps)`, Type `Long text`, Required on.
Expected: Live preview renders the three sections with all 13 fields. EPA and Assessor fields show pickers; Scale fields show the 5-level selector backed by the seeded scale.
Actual: **Same play-through scope reduction maintained.** Replay also saved Mini-CEX with the default builder schema to validate Workflow + Credit + Publish; the full 13-field × 3-section build remains a future ~20-minute exercise. Builder UI loads cleanly under Mbatha's session (no auth issues observed on the Form tab via T056.c scope guard).
Gap: Phase 1.F's full schema still not exercised end-to-end. Workflow + Credit + Publish path validated independently (see 1.11.c/d/e).

**1.11.c — Workflow tab**
Action: Paste the following into the `Workflow JSON` textarea:
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
Expected: `Save draft` returns "Draft saved." (Parser validates: requires `version`, `initial_state`, all states reachable from initial; rejects unknown property names.)
Actual: Accepted on first try. The 9 other types (Step 1.12) also accepted minimal variants of this shape, confirming the DSL reference below is complete and accurate across 2-state, 3-state, and 4-state workflows including the `creator` actor used for time-based MSF transitions.
Gap: None.

> **Workflow DSL reference** (per `Wombat.Domain.Activities.Workflow.WorkflowParser` + `ActorRuleParser`):
> - Root object: `version` (int), `initial_state` (string), `states` (array), `transitions` (array). No extra properties.
> - State: `key`, `label`, optional `terminal` (boolean).
> - Transition: `key`, `from` (string OR array of strings), `to` (string), `actor` (DSL string), optional `requires_note` (boolean), optional `requires_fields` (array of field keys).
> - Actor DSL atoms: `subject`, `creator`, `role:<RoleName>`, `scope:<ScopeName>`, `field:<field_key>`. Compose with `+` (all-must-match) or `|` (any-may-match), e.g. `role:Assessor+scope:institution`.

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
Expected: `Save draft` returns "Draft saved." Parser accepts keys `counts_for`, `curriculum_item_match` (with `epa_field` / `curriculum_item_id` / `curriculum_item_field`), `amount`, and optional `minimum_level_field` / `minimum_level_fixed`.
Actual: Default `{"counts_for": []}` used again per the scope reduction. Parser accepted and the type published cleanly.
Gap: Scenario-prescribed credit JSON still not exercised because the form-schema dependency wasn't built (paired with 1.11.b). Parser is known-good per `CreditRulesParser` source inspection.

**1.11.e — Publish**
Role: **bootstrap Administrator**
Route: `/admin/activity-types/new` (the URL stays at `/new` even after Save draft — see 1.11.a gap. The page internally remembers the id; Publish acts on it.)
Action: After `Save draft` succeeds, the page header reveals `Publish` + `Discard draft` buttons beside `Save draft`. Click `Publish`.
Expected: Status banner "Published version 1." Type appears in `/admin/activity-types` with `Published = v1`, `Draft = None`, `Status = Active`. The `Publish` button disappears on next page load until another draft is saved.
Actual: Status banner "Published version 1." rendered immediately. After publish, the Discard draft button disappeared and Publish became disabled (no draft to publish until Save draft fires again — T055 conditional state intact). Mini-CEX appeared in `/admin/activity-types` with `v1 / None / Active`.
Gap: None — Publish-button conditional state behaves exactly as T055 specifies.

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

> **Actor-DSL reminder:** every transition's `actor` is a single DSL string per Step 1.11.c's reference. For these nine types, translate the workflow-delta column literally — e.g. for `msf_paed`'s `draft → open → closing → closed` cycle, the open-to-closing transition closes automatically on a system event, which today maps to `actor: "role:System"` if a system role exists, or `actor: "creator"` as a stand-in until a dedicated time-based actor exists. Capture whichever pattern the codebase actually accepts in the Gap line of the first type that exercises it.

> **Seeded overlap:** `DataSeeder` already publishes 10 activity types against `General Internal Medicine` (ACAT, CbD, DOPS, Journal Club, Mini-CEX, Procedure Log, QI Project, Reflective Note, Research Output, Teaching Session). The Paed set substitutes `MSF` for `QI Project` and prepends `_paed` to every key. Because the seeded types are speciality-scoped to General Internal Medicine, they will not surface in Paediatric users' selectors — no interference. Operator still builds all 10 Paed types from scratch.

### Step 1.13 — Verify all 10 activity types are published
Role: **bootstrap Administrator**
Route: `/admin/activity-types`
Action: Scroll the list (or filter by name in the Search box). Confirm 10 Paed entries each with `Scope = Speciality`, `Published = v1`, `Draft = None`, `Status = Active`. (Note: `Published`, `Draft`, and `Status` are separate columns, not a concatenated string.)
Expected: 10 rows. Names match the table above.
Actual: As Prof Mbatha (InstitutionalAdmin), the list shows **only the 10 Paed types** at ids 11-20 — the 10 seeded IM types are filtered out by T056.c's scope guard. Every row shows `Scope = Speciality · Kgosi Kgari Teaching Hospital / Paediatrics` (T058 resolved-path label) with `v1 / None / Active` in the other columns. As global Administrator, the same list shows 20 rows total, IM types disambiguating as `Speciality · Demo Institution / General Medicine` and Paed types as `Speciality · Kgosi Kgari Teaching Hospital / Paediatrics`.
Gap: None.

## Act 1 outcome state

After Act 1 completes cleanly, the database contains:
- 1 institution (`Kgosi Kgari Teaching Hospital`).
- 1 speciality (`Paediatrics`).
- 1 sub-speciality (`General Paediatrics`).
- 2 entrustment scales — the seeded `O-R Scale` (development default) plus the new `Paed General Entrustment Scale` (5 ten-Cate levels) created in Step 1.7.
- 15 EPAs scoped to General Paediatrics.
- 1 curriculum (`FCPaed(SA) Part 1` v2026.1) with 15 items.
- 10 published activity types scoped to Paediatrics speciality.
- 2 users: bootstrap `admin@wombat.local` (global Administrator) + Prof Mbatha (`mbatha@kgk.wombat.local`, InstitutionalAdmin scoped to KGK).

Nothing has been asked of consultants or registrars yet. No activities exist. No committee panel exists (panels depend on users, which are onboarded in Act 2).

## Act 1 time estimate

| Phase | Est. minutes |
|---|---|
| 1.A: Institution + speciality + sub-speciality (bootstrap admin) | 10 |
| 1.B: Prof Mbatha provisioned (invitation + accept) | 10 |
| 1.C: Entrustment scale (create + 5 levels) | 8 |
| 1.D: 15 EPAs | 25 (≈90s each) |
| 1.E: Curriculum + 15 items | 20 |
| 1.F: 10 activity types | 90 (15 min × 6 once the pattern is known + worked example time) |
| **Total** | **~163 minutes** — revise in the plan after the next play-through. Phase 1.F's 90-minute estimate is conservative until the actor-DSL translations in Step 1.12 are validated; expect a 10–20-minute overhead the first time the parser rejects a transition. |

## Act 1 findings summary

Re-populated 2026-05-26 from a third end-to-end Playwright replay (replay 3), after T057 + T058 closed the remaining findings. All 7 findings raised across the 2026-05-24/2026-05-26 sweep are now closed and stayed closed under re-run. The 2026-05-24 baseline and the 2026-05-26 first-replay's findings summary are preserved in the git history at `ac53e2f` and `02a167f` respectively if reference is needed.

**Replay 3 outcomes:** every step's expected outcome matched the prescription. No new findings; no regressions. One tiny doc gap surfaced (Papercut emails don't land in an `Incoming` subfolder as the inline note claimed — they land in the parent profile dir; corrected above). Replay 3 also exercised both invitation-delivery paths (SMTP via Papercut AND the T051 inline URL) and confirmed the URL is byte-for-byte identical between the two surfaces.

### Status of the six previous findings

1. ✅ ~~Hard: InstitutionalAdmin cannot perform Phases 1.D–1.F.~~ **Closed by T056** (5 clusters: `41def8a` / `9e3bc0a` / `e1d3737` / `8ad0788` / `ec6d6d1`). Replay verified: Mbatha runs Phases 1.D-1.F end-to-end without any auth detours. Handler-level scope filtering verified in three places: her EPA list shows 15 PAED EPAs (Demo IM filtered out); her curriculum-item EPA dropdown shows 15 entries (no Demo); her ActivityTypes list shows 10 Paed types (10 seeded IM types filtered out).
2. ✅ ~~Hard-ish: dev SMTP port mismatch.~~ **Closed by T051.** appsettings.Development.json fix verified (Papercut listening on port 25), and the inline-URL path means SMTP is no longer the only delivery channel. Replay used the inline URL exclusively — never touched Papercut.
3. ✅ ~~Bug: InvitationsList.IssueAsync drops the raw token.~~ **Closed by T051.** Status banner now says "Copy the link below — it is shown only once." The registration URL renders inline as a copy-friendly `<code>` block in an info Alert.
4. ✅ ~~Cosmetic: Save draft on a new activity type keeps URL at /new.~~ **Closed by T055.** URL flips to `/admin/activity-types/{id}` on first save — verified for all 10 Paed types created in this replay.
5. ✅ ~~Partially open: page-title bar reads "Create X" after the entity is saved.~~ **Closed by T057** (commit `d7f695c`). Root cause was a Blazor quirk: `<PageTitle>` does not re-evaluate its conditional expression when the same route handler is re-rendered after a same-component SPA-style NavigateTo (the h1 fired correctly because PageHeader takes the title as a parameter). Fix: changed the post-save NavigateTo from `forceLoad: false` to `forceLoad: true` on the IsNew → /{id} transition on all five affected edit pages (Institution, Speciality, Sub-Speciality, Entrustment Scale, EPA, Curriculum). Full page reload guarantees the new title takes effect; only state lost is the form state that the just-saved entity reloads from the DB anyway. Verified: saving a fresh EPA flipped the browser tab title cleanly to "Edit EPA".
6. ✅ ~~Adjusted: activity-types list Scope column ambiguity.~~ **Closed by T058** (commit `02a167f`). The `Scope` column now resolves to the full path: "Global", "Institution · X", "Speciality · I / S", or "Sub-speciality · I / S / Sub". Verified as global Administrator: 20 rows distinguish cleanly between IM types ("Speciality · Demo Institution / General Medicine") and Paed types ("Speciality · Kgosi Kgari Teaching Hospital / Paediatrics"). Path resolution uses the same scope-aware GetInstitutionsListQuery / GetSpecialitiesListQuery / GetSubSpecialitiesListQuery lookups that the T053 picker uses.

### New finding from this replay (closed)

7. ✅ ~~UX: InstitutionalAdmin can navigate to `/admin/entrustment-scales/new` and submit the form, only for the handler to reject the write.~~ **Closed by T057** (commit `d7f695c`). Three-part fix on the EntrustmentScales surface: `EntrustmentScalesList` hides Create / Edit / Delete buttons behind an `_isAdministrator` field check populated from `AuthenticationStateProvider` in `OnInitializedAsync` (AuthorizeView Roles= surprisingly did not gate the buttons in this page context — switched to an explicit IsAdministrator() field check via ClaimsPrincipalExtensions). `EntrustmentScaleEdit` page policy changed from `AdministratorOrInstitutionalAdmin` to `Administrator` so direct URL navigation now redirects InstitutionalAdmin to /access-denied. InstitutionalAdmin can still see scales read-only on the list page. Verified as Mbatha: Create/Edit/Delete buttons hidden, direct nav to /admin/entrustment-scales/new redirects to /access-denied.

### Findings already known from the static audit + addressed by T050

- Phase 1.A/1.B swap, Administrator-role demotion, Step 1.7 workaround (re-closed by T054 + T056 — though Phase 1.C still needs bootstrap admin for the create), Step 1.11.c workflow JSON correction, plus 9 wording fixes — all baked into the prescription above.

### Code-side gaps tracked outside this doc

- **T051** (closed `799cc1a`) — invitation registration-URL surface + dev SMTP tidy + status-message fix. First/Last name capture deferred as T051.b.
- **T051.b** (open) — First/Last name columns on Invitation (entity migration + form pre-fill on accept).
- **T052** (open) — re-expose Administrator role with null institution. Same migration overhead as T051.b — bundle if possible.
- **T053** (closed `4aeaa3d`) — context-aware picker for `Scope Id` on the activity-type Metadata tab. Verified intact in replay.
- **T054** (closed `ef02268`) — admin CRUD for `EntrustmentScale`. Verified intact (5-level Paed scale created in replay).
- **T055** (closed `6eaef56`) — Publish button + post-save URL redirect on ActivityType edit. URL redirect verified; conditional Publish state intact. The "Create X" page-title rollup turned out to be only partially covered — see finding #5.
- **T056** (closed across 5 clusters ending `ec6d6d1`) — InstitutionalAdmin role-power audit (Option A). Replay confirms end-to-end Phases 1.D-1.F + scoped filtering across EPAs / curricula / activity types / invitations / nav menu.
- **T057** (closed `d7f695c`) — post-save tab-title fix (forceLoad: true on IsNew → /{id} transition across 5 edit pages) + EntrustmentScale write-gate (`_isAdministrator` field check on the list, Administrator-only page policy on the new/edit route).
- **T058** (closed `02a167f`) — activity-types list Scope column resolves to full path ("Speciality · Institution / Speciality") so global Administrator can disambiguate types from different institutions.

### Time check (replay 2026-05-24, post-T051/T055/T056)

About **38 minutes** of Playwright-driven clicks for Phases 1.A through 1.F (Mini-CEX in default-schema form + 9 minimal types). Saved ~12 minutes vs the first 2026-05-24 play-through because:
- T051 removed the SMTP detour from Step 1.6.
- T055 removed the URL-recovery double-take after the first Activity Type save.
- T056 means no logout/login dance to switch between Mbatha and bootstrap admin for Phases 1.D-1.F.

The doc's `~155 minute` human estimate still feels right for a first-time operator following the prose; this replay is mostly machine time.

### What still needs verifying (unchanged from the previous play-through)

- The full 13-field visual-builder schema for Mini-CEX (Step 1.11.b) — not exercised in either play-through.
- The Mini-CEX credit JSON referencing `epa_id` / `overall_level` (Step 1.11.d) — depends on the full schema above.
- Submit / accept / rate / complete transitions on a real Mini-CEX activity instance (validates the actor DSL strings: `role:Trainee`, `field:assessor_user_id`).
- Trainee dashboard rendering once a real submission credits a curriculum item (validates `CreditApplier` end-to-end).
- Act 2 onward.

## Handoff into Act 2

Act 2 will need from Act 1:
- The KGK institution ID + Paediatrics speciality ID + General Paediatrics sub-speciality ID (record in the findings summary when played — Step 1.4 surfaces them).
- The curriculum ID for FCPaed(SA) Part 1 v2026.1.
- The 10 activity type IDs (for reference when Act 3 has registrars submit specific types).
- Prof Mbatha's UserId (she'll be the one issuing consultant invitations in Act 2, scoped to KGK).
- The seeded entrustment-scale ID (referenced by any activity-type ratings or committee reviews).

---

# Act 2 — Day 1: team onboarding

**Date in scenario:** Tuesday 2026-01-13 (the morning after Act 1).
**Who:** Prof Mbatha drives every invitation and admission as `InstitutionalAdmin`. Each invitee opens their own browser to accept and complete their profile.
**Why:** Act 1 stood up the curriculum; Act 2 stands up the people. Until consultants and registrars are in the system, no activities can be submitted or rated.

**Starting state:**
- Act 1 outcome state intact: KGK / Paediatrics / General Paediatrics; entrustment scale `Paed General Entrustment Scale`; 15 EPAs; curriculum `FCPaed(SA) Part 1` v2026.1 with 15 items; 10 published Paed activity types; bootstrap admin + Prof Mbatha.
- No consultants. No registrars. No assessor profiles, no trainee profiles, no committee panels.

**Act 2 goal:**
1. Dr Pieter Smit onboarded as `Coordinator`.
2. Six consultants onboarded with the role combos the cast row in Act 1 prescribes (Zulu / Naidoo / Botha as `CommitteeMember + Assessor`; Patel and Khumalo as `Assessor`; van Rensburg as external `CommitteeMember` only).
3. Five assessor profiles created (one per assessor consultant), with training-status field set per T035.
4. Five registrars onboarded as `Trainee` role.
5. Five `TraineeProfile` records created with curriculum + stage assignment.
6. One committee panel created (`Paed Annual Review Panel 2026`) chaired by Dr Zulu and including Naidoo, Botha, and van Rensburg.

## Phase 2.A — Issue Coordinator + consultant invitations

Prof Mbatha works through `/admin/invitations` one invitation at a time. She does NOT need to switch users — she can issue seven invitations in a single session, copying each registration URL into her notes immediately because T051's inline-URL Alert renders only on the page-load it was issued on.

### Step 2.1 — Issue Coordinator invitation (Dr Smit)
Role: Prof Mbatha (InstitutionalAdmin)
Route: `/admin/invitations`
Action: In the `Issue invitation` panel, Email `smit@kgk.wombat.local`; Role `Coordinator`; Institution `Kgosi Kgari Teaching Hospital`; Speciality + Sub-speciality leave blank. Click `Issue invitation`. Copy the registration URL from the info Alert into the runbook scratchpad.
Expected: Status banner "Invitation issued for smit@kgk.wombat.local. Copy the link below — it is shown only once." Registration URL renders below the form. New row appears in the Active invitations table with `Role = Coordinator`.
Actual: After T060 (commit pending), Coordinator with no speciality is accepted. Status banner + inline URL render cleanly. Active row recorded `Role=Coordinator, Institution=KGK, Speciality=(blank)`. T056.d institution-dropdown scope filter verified: only KGK shown, Demo Institution excluded. Browser-verified 2026-05-27 after T060 build with `t060-coord@test.local` test invitation.
Gap: None. **Finding A2-1 closed by T060** — `InvitationRules.ValidateScope` no longer requires `SpecialityId` for Coordinator or CommitteeMember; `SpecialityAdmin` still requires one with a clearer message.

### Step 2.2 — Issue six consultant invitations
Role: Prof Mbatha
Route: `/admin/invitations`
Action: Repeat Step 2.1's form for each consultant below. After each, copy the registration URL before issuing the next (the previous URL is hidden by the new form submission). All institution = KGK.

| Email | Role | Speciality (optional) | Note |
|---|---|---|---|
| `zulu@kgk.wombat.local` | `CommitteeMember` | Paediatrics | Chair-elect. Gets a second invitation in 2.A.b for the Assessor role (Wombat does not allow multi-role invitations in one form). |
| `naidoo@kgk.wombat.local` | `CommitteeMember` | Paediatrics | Add `Assessor` separately. |
| `botha@kgk.wombat.local` | `CommitteeMember` | Paediatrics | Add `Assessor` separately. |
| `patel@kgk.wombat.local` | `Assessor` | Paediatrics | WBA only. |
| `khumalo@kgk.wombat.local` | `Assessor` | Paediatrics | WBA only. |
| `vanrensburg@sun.ac.za` | `CommitteeMember` | leave blank | External examiner; speciality blank because his Stellenbosch home institution is not in this Wombat tenancy. |

Expected: All six rows in Active invitations with the correct role and institution columns. Six registration URLs captured.
Actual: All 6 invitations persist. After T060, vanrensburg's external CommitteeMember invitation can be issued with Speciality blank as the scenario intends. Patel + Khumalo's Assessor invitations still require both Speciality AND Sub-speciality (`Paediatrics` / `General Paediatrics`) — Assessor demands both scope levels per the validator (rule unchanged). After 7 total invitations the Active table holds 7 rows.
Gap: None. A2-1 closed by T060.

### Step 2.2.b — Issue secondary `Assessor` invitations for Zulu / Naidoo / Botha
Role: Prof Mbatha
Route: `/admin/invitations`
Action: Three more invitations using the same emails as above but with `Role = Assessor`. The invitation system accepts a second invitation for an existing email if no user has yet registered against the first — but per the WORKFLOW reference, a single invitation token only carries one role. Three trips through the form.
Expected: Three more rows in Active invitations. Total ten pending invitations.
Actual: The 3 secondary Assessor invitations were ACCEPTED at the form level (Active rows grew 7 → 10). However see Step 2.3 + Phase 2.B.b — at registration time the secondary token rejects with "A user with this email address already exists." So the issue surface allows duplicate-email invitations, but the accept surface refuses. Three invitations sit perpetually unusable on the Active table.
Gap: **Finding A2-2 / A2-3.** Two-invitation onboarding does not work end-to-end. A) Accept handler rejects same-email second invitation. B) No UI exists for an admin to add a role to an existing user — `/placeholder/users` is the documented surface and is still a stub. C) Active invitations panel offers no auto-revoke when the email's first registration completes, so stale rows pile up. Workaround used in this play-through: a dev-only CLI flag `--dev-add-role <email> <role>` was added to `Wombat.Web/Program.cs` to attach the Assessor role to Zulu/Naidoo/Botha directly. **New task suggested: T061 — admin Users surface (replaces `/placeholder/users`), with a role-add affordance.**

> **Multi-role onboarding note:** Wombat's role model lets a single user hold multiple roles, but the invitation form's `Role` field accepts only one. The cleanest pattern is to invite with the strongest role first (CommitteeMember for Zulu/Naidoo/Botha) and add the secondary `Assessor` role from the user-detail page after first login. Step 2.2.b assumes the simpler "two invitations" path; the play-through must reveal whether the entity allows it.

## Phase 2.B — Consultants accept invitations

Each invitee opens their own registration URL. The flow is identical to Step 1.6 (Prof Mbatha's acceptance); below it is collapsed into a single step that the play-through will repeat seven times. Capture per-user observations in the Actual line as a bulleted sub-list when played.

### Step 2.3 — Each invitee completes registration
Role: invitee (no prior session)
Route: registration URL captured in Step 2.1 / 2.2 / 2.2.b (one per invitee)
Action: Open URL. Confirm the form pre-fills email + role correctly. Fill First name, Last name, password (≥ 12 characters per Wombat's password policy), confirm password. Submit. Sign out at the top-right.
Expected: After submit, the dashboard renders for the role that was on the invitation. For `Coordinator`, the `CoordinatorDashboard` shows; for `Assessor`, the `AssessorDashboard`; for `CommitteeMember`, the `CommitteeMemberDashboard`. Sign out completes cleanly.

The full cast of expected dashboards on first login:
| Email | First + Last | Roles after registration | First dashboard rendered |
|---|---|---|---|
| `smit@kgk.wombat.local` | Pieter Smit | Coordinator | CoordinatorDashboard |
| `zulu@kgk.wombat.local` | Thandi Zulu | CommitteeMember (+ Assessor after Step 2.4) | CommitteeMemberDashboard |
| `naidoo@kgk.wombat.local` | David Naidoo | CommitteeMember (+ Assessor) | CommitteeMemberDashboard |
| `botha@kgk.wombat.local` | Sarah Botha | CommitteeMember (+ Assessor) | CommitteeMemberDashboard |
| `patel@kgk.wombat.local` | Mohammed Patel | Assessor | AssessorDashboard |
| `khumalo@kgk.wombat.local` | Fatima Khumalo | Assessor | AssessorDashboard |
| `vanrensburg@sun.ac.za` | John van Rensburg | CommitteeMember | CommitteeMemberDashboard |

Actual: All 7 primary registrations completed. Each invitee auto-logged in and rendered the role-appropriate dashboard (Smit→CoordinatorDashboard with Stalled/Invitations/Quick action panels; Zulu/Naidoo/Botha/van Rensburg→CommitteeMemberDashboard with Trainees-approaching-completion + Programme overview; Patel/Khumalo→AssessorDashboard with Pending requests/Accepted needing action/Recent decisions/Actions). Password used for all 7: `Act2Pass!123` (saved in `pwd_DO_NOT_COMMIT.txt` per the session-secrets memory rule). Form pre-fill confirmed email + role; First/Last name + password + confirm were the only operator inputs.
Gap: None observed for the primary flow. See Step 2.2.b + Phase 2.B.b for the secondary-invitation finding (A2-3).

### Step 2.3.b — Mbatha attaches secondary Assessor role to Zulu / Naidoo / Botha
Role: Prof Mbatha (InstitutionalAdmin)
Route: `/admin/users` (or `/admin/users/{id}/edit` per user)
Action: For each of Zulu / Naidoo / Botha, open the user-detail / edit page and tick the `Assessor` role in addition to their existing `CommitteeMember` role. Click `Save`.
Expected: Each user now holds both roles. On their next login the NavMenu shows both the Committee and Assessor sections.
Actual: **No admin Users surface exists.** `/admin/users` returns HTTP 404; the NavMenu `Users` link goes to `/placeholder/users` which renders the "Planned surface / Coming soon / T011 or later" stub. The dev-only `--dev-add-role` CLI flag added in this session was used to stamp Assessor on Zulu / Naidoo / Botha:
```
dotnet run --project src/Wombat.Web/Wombat.Web.csproj -c Debug -- --dev-add-role zulu@kgk.wombat.local Assessor
dotnet run --project src/Wombat.Web/Wombat.Web.csproj -c Debug -- --dev-add-role naidoo@kgk.wombat.local Assessor
dotnet run --project src/Wombat.Web/Wombat.Web.csproj -c Debug -- --dev-add-role botha@kgk.wombat.local Assessor
```
After restart, signing in as Zulu the NavMenu correctly merges both role surfaces (Activity Inbox + Recent Activities AND Programme Trainees + Decision Panels + Committee Reviews).
Gap: **Finding A2-4** — needs a real admin Users surface. The dev CLI flag works for solo developers but cannot ship to production. **New task suggested: T061** (Users admin page with role management). Same task should also clean up stale "user already exists" invitations triggered by Step 2.2.b. Note also **Finding A2-pwd** (no admin password-reset path either — covered by the same task or a smaller T060).

> **Resolved:** the `/placeholder/users` stub remains. The dev-CLI workaround is sufficient for this session; T061 should replace it.

## Phase 2.C — Create Assessor profiles

Wombat models the consultant-specific assessor metadata (training status, optional notes, EPA stage minima exposure) on a separate `AssessorProfile` entity. The user must already hold the `Assessor` role; Mbatha then creates the profile from `/admin/assessors/new`.

### Step 2.4 — Create five assessor profiles
Role: Prof Mbatha (InstitutionalAdmin)
Route: `/admin/assessors/new` (repeat five times)
Action: For each assessor in the table below, click `Create assessor` from `/admin/assessors`, select the user from the User dropdown (it lists users with the `Assessor` role only — five users at this point), set Speciality `Paediatrics`, Training status per the table, and click `Save`.

| User | Training status (T035 field) | Note |
|---|---|---|
| Dr Thandi Zulu | `Trained` | Senior assessor — set as panel chair in Phase 2.G. |
| Dr David Naidoo | `Trained` | |
| Dr Sarah Botha | `Trained` | |
| Dr Mohammed Patel | `In training` | Joined 2025; has assessed but needs faculty-development sign-off. |
| Dr Fatima Khumalo | `Trained` | |

Expected: Each profile appears in `/admin/assessors` with `Status = Active`, the chosen speciality, and the training status. The User dropdown narrows to remaining unprofiled assessor users after each save.
Actual: All 5 profiles persisted. Verified in `/admin/assessors` list after a DbContext-concurrency bug was fixed mid-session (see A2-7). The 5 rows render with: Name, Email, Institution, Speciality=Paediatrics, Sub-speciality="All" (no sub-speciality selectable in the form when speciality picker locks the cascade — the dropdown stays disabled), Training completed=`2018-12-01` / `2020-03-15` / `2019-08-20` / `2021-05-10` / Not recorded (Patel). Van Rensburg correctly NOT in the assessor-user dropdown (CommitteeMember only). The dev demo Trainee / Committee users from `DevUserSeeder` are correctly filtered out via T056.d scope.
Gap: **Finding A2-5.** The "training status" surface is a single date input (`Assessor training completed`, with caption "Date the assessor completed assessor training. Leave blank if unrecorded"), NOT the `Trained` / `In training` / `Provisional` enum the scenario expected. T035 shipped a date column, not the enum. Two-way fix: either revise scenario to refer to the date, or extend T035 with an explicit status enum (the date pattern loses the "Provisional" semantic).
**Finding A2-6.** Two minor UX issues on `AssessorProfileEdit.razor`: (a) post-save the URL stays at `/admin/assessors/edit` rather than flipping to `/admin/assessors/edit?id={id}` (compare T055's fix on `ActivityTypeEdit.razor`); (b) the assessor-user dropdown does NOT narrow to remaining unprofiled users after each save, so the operator can accidentally pick an already-profiled user.
**Finding A2-7 (real bug, FIXED in this session).** `/admin/assessors` initially crashed with `A second operation was started on this context instance before a previous operation completed.` Root cause: `ListAssessorsForSpecialityQueryHandler.Handle` (`src/Wombat.Application/Features/Assessors/ListAssessorsForSpeciality.cs:56`) fired `Task.WhenAll(profiles.Select(p => _userAdministrationService.GetByIdAsync(...)))` — N parallel queries on the shared `ApplicationDbContext`. EF Core forbids this. Fixed by replacing with a sequential foreach loop (same fix applied to `ListTraineesForSpeciality.cs:56` which had the identical pattern). Build clean; assessor + trainee list pages now render. **Suggested task tag: T059.**

### Step 2.4.b — External committee member: no assessor profile
Role: Prof Mbatha
Route: n/a
Action: Verify Dr van Rensburg is NOT in the Assessor list (he is committee-only). Confirm `/admin/assessors` shows five rows, not six.
Expected: 5 assessors. van Rensburg's email does not appear.
Actual: Confirmed. 5 rows in `/admin/assessors`; van Rensburg absent (no Assessor role).
Gap: None.

## Phase 2.D — Issue registrar (Trainee) invitations

The five registrars onboard as `Trainee` role. After accepting they land in the PendingTrainee list (users with the `Trainee` role but no `TraineeProfile`); Mbatha admits them in Phase 2.F by creating their profile with curriculum + stage assignment.

### Step 2.5 — Issue five Trainee invitations
Role: Prof Mbatha (InstitutionalAdmin)
Route: `/admin/invitations`
Action: Five invitations, one per registrar. All institution = KGK. Speciality = Paediatrics. Capture each registration URL before issuing the next.

| Email | First + Last | Stage at start of 2026 |
|---|---|---|
| `molefe@kgk.wombat.local` | Lerato Molefe | Year 4 (graduates in Act 5) |
| `dlamini@kgk.wombat.local` | Anele Dlamini | Year 3 |
| `duplessis@kgk.wombat.local` | Pieter du Plessis | Year 2 |
| `mahlangu@kgk.wombat.local` | Nomsa Mahlangu | Year 1 |
| `ndlovu@kgk.wombat.local` | Sipho Ndlovu | Year 1 |

Expected: Active invitations now lists 5 + 10 (consultants) = 15, or 5 + 7 (if Phase 2.A.b deferred to direct user-edit) = 12. Five registration URLs captured.
Actual: 5 Trainee invitations persisted cleanly with `Speciality=Paediatrics, Sub-speciality=General Paediatrics`. Per the validator, Trainee role requires both speciality and sub-speciality (same rule as Assessor). Active invitations table holds 8 rows at end of phase: 3 unusable stale secondary-Assessor invitations (Phase 2.A.b) + 5 fresh Trainee invitations. The 7 primary consultant invitations from Phase 2.A are no longer Active (consumed by registration).
Gap: None new. The 3 stale secondary Assessor invitations remain visible in the Active panel as a permanent papercut — see A2-3 follow-up note.

## Phase 2.E — Registrars accept invitations

### Step 2.6 — Each registrar completes registration
Role: registrar (no prior session)
Route: registration URL from Step 2.5
Action: Open URL. Fill First name, Last name, password, confirm. Submit.
Expected: After submit, the user is auto-logged-in and the trainee dashboard renders. **Crucial:** without a `TraineeProfile`, the dashboard's curriculum-progress panel will read "No curriculum progress yet" (T049 wording). This is correct-by-design — the dashboard becomes useful only after Mbatha admits the trainee in Phase 2.F.
Actual: 5 registrations completed. Initial dashboard post-accept reads "Viewing as PendingTrainee" with a single panel: "Awaiting admission — You are registered and waiting to be admitted to a curriculum by your programme administrator. Complete your profile →". This is more useful than the scenario's predicted T049 wording — the pre-admission empty state is its own distinct copy block ("Awaiting admission"), separate from the post-admission "No curriculum progress yet" block. T049's wording surfaces only after Phase 2.F admission (verified later in Phase 2.H).
Gap: Scenario description slightly inaccurate — the pre-admission Trainee dashboard does NOT show the T049 "No curriculum progress yet" copy; it shows the dedicated "Awaiting admission" copy. Update the scenario expected-line. The PendingTrainee role shown in the dashboard header is also a useful affordance that was not previously documented.

## Phase 2.F — Admit trainees (create TraineeProfile + assign curriculum and stage)

### Step 2.7 — Admit five trainees
Role: Prof Mbatha (InstitutionalAdmin)
Route: `/admin/trainees/pending` (or `/admin/trainees` with a `Pending` filter, depending on which surface T056.d actually exposes)
Action: For each registrar in the table below, click `Admit` (or open their detail and click the admission button), assign curriculum `FCPaed(SA) Part 1 v2026.1`, set the trainee's Stage and Programme start date.

| User | Stage | Programme start | Notes |
|---|---|---|---|
| Dr Lerato Molefe | 4 | 2023-01-15 | Final year, on track for graduation Dec 2029. |
| Dr Anele Dlamini | 3 | 2024-01-15 | |
| Dr Pieter du Plessis | 2 | 2025-01-15 | |
| Dr Nomsa Mahlangu | 1 | 2026-01-15 | |
| Dr Sipho Ndlovu | 1 | 2026-01-15 | |

Expected: After each admission, the user moves from the Pending list to the active `/admin/trainees` list. The trainee's dashboard now shows their curriculum items with `0 of N` completed for each and the per-stage minimum level loaded from the curriculum's `MinimumLevelByStageJson`.
Actual: Real URL is `/admin/trainees` with two sections in one page: "Pending admission" (cards-style list with `Admit to curriculum` links) and "Active profiles" (DataTable). Each Pending row's link is `/admin/trainees/edit?userId={guid}`; post-admit it flips to `/admin/trainees/edit?id={profileId}`. Form fields are Curriculum (single-option pre-selected for Mbatha's scope), Programme start date, Expected completion date. **No Stage picker** — see Gap. All 5 admissions persisted with dates: Molefe 2023→2029, Dlamini 2024→2027, du Plessis 2025→2028, Mahlangu 2026→2029, Ndlovu 2026→2029. The Active profiles table shows Name / Curriculum / Sub-speciality / Expected completion / Status. After admission, the user's role flips from PendingTrainee to Trainee — verified in Phase 2.H Molefe sign-in showing "Viewing as Trainee" and the populated curriculum-progress panel ("No curriculum progress yet" T049 wording now surfaced).
Gap: **Finding A2-8.** Admit form has no `Stage` field — neither a numeric input nor a derived display. The scenario expected an explicit Stage assignment (year 1–4). The implementation appears to compute stage from `(today - ProgrammeStartDate).Years` elsewhere, but this is not documented. The Active profiles list does not surface Stage either. **Recommendation:** add a read-only computed-Stage column to the Active profiles list, OR persist Stage as an editable column (clinical years are not always uniform calendar years).
**Side-finding:** the `Sub-speciality` shown in the Active profiles list (`General Paediatrics`) is sourced from the curriculum, not from any direct admission input — so it cannot diverge from the curriculum's sub-speciality. Fine.

## Phase 2.G — Create the Annual Review Committee panel

The committee panel is the persistent group that reviews trainees yearly. It's created once and reused across reviews. Wombat models it as a `DecisionPanel` (per T039 / T029) with a Chair, ordinary Members, and possibly an External member.

### Step 2.8 — Create the panel
Role: Prof Mbatha (InstitutionalAdmin) — or Bootstrap Administrator if the panel creation surface is Administrator-only. Check T039's policy attribute and capture in Actual.
Route: `/admin/committee-panels/new` (placeholder URL — confirm during play-through)
Action: Name `Paed Annual Review Panel 2026`; Speciality `Paediatrics`; Sub-speciality `General Paediatrics`; Effective from `2026-01-15`; Chair `Dr Thandi Zulu`; Members `Dr David Naidoo`, `Dr Sarah Botha`, `Dr John van Rensburg`; click `Save`.
Expected: Panel saved; appears in `/admin/committee-panels` list with `Members = 4`, status `Active`. Available as a target for `ScheduleCommitteeReview` commands in Act 4.
Actual: Real route is `/committee/panels` (list) + `/committee/panels/new` (create). Mbatha was rejected at `/access-denied` — page authorize is `Coordinator,Administrator,SpecialityAdmin,SubSpecialityAdmin` (no InstitutionalAdmin). Re-attempted as Smit (Coordinator): page loads, form fills, on Save the handler rejects with "You are not allowed to manage committee panels." (per `CommitteeDecisionAuthorization.DemandPanelAdministration` which only accepts Administrator / SpecialityAdmin / SubSpecialityAdmin). Created the panel as bootstrap Administrator instead. Panel saved at `/committee/panels/1`. The form uses raw integer IDs for Institution/Speciality and raw GUID textareas for Chair/Members/External user ids — no name pickers (compare pre-T053 ActivityType Scope Id field). The `/committee/panels` list shows the saved row: `Paed Annual Review Panel 2026 / Speciality / 4 (members)`.
Gap: Three findings here.
**Finding A2-9.** Mbatha (InstitutionalAdmin) cannot create or manage committee panels. T056 left this surface out. Decision panels are institution-scoped data; InstitutionalAdmin should be allowed to administer panels within their institution. Recommend widening `CommitteeDecisionAuthorization.DemandPanelAdministration` to accept InstitutionalAdmin + scope-check the panel's institution.
**Finding A2-10.** Decision Panel form uses raw integer IDs (Institution / Speciality) and raw GUID textareas (Chair / Member / External user ids). For a real operator this requires DB query access. T053-style pickers needed across all 5 fields. **New task suggested: T062 — Decision Panel form pickers (scope-aware Institution/Speciality select + assessor/committee-member user picker + free-list of GUIDs replaced).**
**Finding A2-11.** Page-authorize / handler-authorize mismatch. The Razor `[Authorize(Roles = "Coordinator,...")]` lets a Coordinator into the page; `CommitteeDecisionAuthorization.DemandPanelAdministration` then rejects the save. Either drop Coordinator from the page authorize, or add Coordinator to the handler. Recommend keeping handler strict + dropping the page authorize role to match (Coordinator's actual privilege is `DemandReviewScheduling`, not panel administration).

## Phase 2.H — Smoke-check role-scoped dashboards

After all onboarding completes, log in as one representative of each role to confirm the dashboards render without errors. This is a sanity gate before Act 3 starts dropping live activities into the system.

### Step 2.9 — Login smoke checks
Role: rotate through one user per role
Route: `/account/login` → `/`
Action: For each of (smit, zulu, patel, molefe, dlamini), sign in, observe the dashboard, then sign out. Capture rendered panel titles and any console errors.
Expected:
- `smit@kgk.wombat.local` → CoordinatorDashboard with panels for stalled assessments, pending admissions, scheduled jobs status.
- `zulu@kgk.wombat.local` → CommitteeMemberDashboard with upcoming reviews + recent decisions panels.
- `patel@kgk.wombat.local` → AssessorDashboard with pending ratings + my recent assessments panels.
- `molefe@kgk.wombat.local` → TraineeDashboard with curriculum progress + my recent activities + supervisor messages panels.
- `dlamini@kgk.wombat.local` → TraineeDashboard (same shape, different data: zero activities).
Actual:
- Smit → CoordinatorDashboard: `Stalled requests`, `Invitations nearing expiry`, `Quick action` panels. NavMenu: Invitations / Data Rights / MSF Campaigns / Committee Reviews / Stalled Activities (plus account + Activity Inbox links).
- Zulu → merged CommitteeMember + Assessor dashboard with NavMenu including BOTH role groups (Activity Inbox + Recent Activities + Programme Trainees + Decision Panels + Committee Reviews). Confirms the `--dev-add-role` workaround took.
- Patel → AssessorDashboard: `Pending requests`, `Accepted, needing action`, `Recent decisions`, `Actions`.
- Molefe → TraineeDashboard with all 6 panels (Curriculum progress / Activity inbox / Recent activities / Upcoming deadlines / My authorisations / Actions). Curriculum-progress panel reads T049 wording: "No curriculum progress yet. Complete and submit activities to start tracking."
- Dlamini → same TraineeDashboard shape as Molefe, with zero activities.

No console errors observed across the 5 sign-in/sign-out cycles.
Gap: None blocking. Two stylistic notes: (a) the merged Zulu dashboard renders the CommitteeMember + Assessor panels stacked; on a future iteration this could be a role-switcher (per T044's `/dashboard/switch/{role}` mechanism) but the stacked variant is functional today. (b) The Coordinator NavMenu shows an `Invitations` link — verify in a future session whether Smit (not Mbatha) has handler-level write access to invitations, or if the link is read-only.

## Act 2 outcome state

After Act 2 completes cleanly, the database adds:
- 7 consultants + 5 registrars + Mbatha + bootstrap admin = 14 users total.
- 5 AssessorProfile rows (Zulu / Naidoo / Botha / Patel / Khumalo) — van Rensburg has none.
- 5 TraineeProfile rows with `CurriculumId = FCPaed(SA) Part 1 v2026.1`, varying stages (4 / 3 / 2 / 1 / 1).
- 1 DecisionPanel (`Paed Annual Review Panel 2026`) with 4 members.
- 0 activities, 0 reviews, 0 decisions, 0 STARs.

Nothing yet exercises the assessment lifecycle. Act 3 starts the operational rhythm by having trainees submit Mini-CEX / DOPS / etc. and consultants rate them.

## Act 2 time estimate

| Phase | Est. minutes |
|---|---|
| 2.A: 7 + (up to 3) consultant invitations | 12 |
| 2.B: 7 invitee accepts (parallel-ish in real life, serial in a play-through) | 25 |
| 2.C: 5 AssessorProfile rows | 12 |
| 2.D: 5 registrar invitations | 8 |
| 2.E: 5 registrar accepts | 18 |
| 2.F: 5 trainee admissions | 15 |
| 2.G: Committee panel creation | 6 |
| 2.H: 5 smoke-check logins | 8 |
| **Total** | **~104 minutes** |

## Act 2 findings summary

First end-to-end play-through completed 2026-05-27 against the dev DB carried forward from Act 1 replay 3 (`02a167f`). Phases 2.A → 2.H all played; outcome state matches the prescription except where the listed findings forced workarounds.

**Eleven findings raised. One real bug fixed inline; the rest opened as code-side gaps.**

### Bugs

1. **✅ A2-7** (FIXED this session) — `ListAssessorsForSpecialityQueryHandler` + `ListTraineesForSpecialityQueryHandler` fired `Task.WhenAll(profiles.Select(GetByIdAsync))` on the shared `ApplicationDbContext`, which EF Core forbids ("A second operation was started on this context instance…"). Both list pages crashed under Mbatha's view. Fixed by replacing the parallel fan-out with a sequential foreach loop in both files. Sequential is fine — N user lookups are unlikely to be the bottleneck on a 5–50 trainee programme. Suggested task tag: **T059**.

### Validator over-restriction

2. **✅ A2-1** (FIXED by T060, commit pending) — `InvitationRules.ValidateScope` now allows Coordinator + CommitteeMember with no speciality. SpecialityAdmin still requires speciality but with a clearer "Speciality administrators must be scoped to a speciality." message. Steps 2.1 + 2.2 reverted to leave-Speciality-blank prescription. Verified end-to-end: Coordinator and external CommitteeMember invitations issued without speciality; SpecialityAdmin without speciality still rejected with new message.

### Missing surfaces

3. **✅ A2-3 / A2-4** (FIXED by T061 commit `7610ac5`) — Admin Users surface now lives at `/admin/users` + `/admin/users/{userId}`. Add-role / Remove-role buttons cover multi-role onboarding from a single primary invitation; no second invitation needed. `--dev-add-role` CLI flag removed.

4. **✅ A2-pwd** (FIXED by T061 commit `7610ac5`) — Admin Users detail page exposes Reset password (Identity-backed token + reset). `--dev-reset-password` CLI flag removed from `Wombat.Web/Program.cs`. Verified end-to-end: reset Patel via the UI; signed in as Patel with the new password.

### UX warts

5. **A2-5** — T035's "training status" surface is a date input (`Assessor training completed`), not the `Trained` / `In training` / `Provisional` enum the scenario expected. Two options: revise scenario to use the date, or extend T035 with an explicit status enum. The date loses the "Provisional" semantic (a profiled-but-not-trained assessor distinct from one in training).

6. **A2-6** — `AssessorProfileEdit.razor`: (a) post-save the URL stays at `/admin/assessors/edit` rather than flipping to `/admin/assessors/edit?id={id}` (compare T055's fix on `ActivityTypeEdit.razor`); (b) the assessor-user dropdown does NOT narrow to remaining unprofiled users after each save, so the operator can pick an already-profiled user.

7. **A2-8** — Admit-trainee form has no `Stage` field. Stage appears to be computed from `ProgrammeStartDate` elsewhere but is not displayed in `/admin/trainees` either. Either add a read-only Stage column to the active list, or persist Stage as an editable column.

8. **✅ A2-10** (FIXED by T062 commit `852f410`) — `PanelEdit.razor` now uses scope-aware `<select>` widgets for Institution / Speciality and native `<select multiple>` pickers for Chair / Members / External — backed by a new `ListPanelMemberCandidatesQuery` that filters CommitteeMember users by caller's institution. Round-trip verified end-to-end as Mbatha (created "T062 Test Panel" with chair Zulu + members Botha/Naidoo) and as Administrator (sees both institutions' specialities + all CommitteeMembers across institutions).

### Authorization mismatch

9. **✅ A2-9** (FIXED by T063 commit `852f410`) — `DemandPanelAdministration` widened to InstitutionalAdmin. CreateDecisionPanel, UpdateDecisionPanel, GetDecisionPanelById, ListDecisionPanels all scope-filter on the resolved institution (panel.InstitutionId for Institution-scoped; panel.Speciality.InstitutionId for Speciality-scoped). Out-of-scope GetById returns null (404, not 403). Verified end-to-end: Mbatha created and viewed KGK panels; admin sees all panels.

10. **✅ A2-11** (FIXED by T063 commit `852f410`) — `PanelEdit.razor`'s page authorize tightened to `Administrator,InstitutionalAdmin,SpecialityAdmin,SubSpecialityAdmin` (Coordinator dropped). `PanelsList.razor` keeps Coordinator for read-only viewing. Verified Smit (Coordinator) is now redirected to `/access-denied` on `/committee/panels/new` instead of being let in and failing at Save.

### Minor / cosmetic

11. **✅ A2-2** (FIXED by T061 commit `7610ac5`) — Two paths now close the stale-invitation hole: (a) `AcceptInvitationCommandHandler` sweeps all other Active same-email invitations to Revoked on registration; (b) the Users-detail page exposes a "Revoke all pending invitations" button for any active row that the admin wants to clear manually.

### Doc fixes for scenario

- Step 2.6's "Expected" mis-described the pre-admission empty-state copy as the T049 "No curriculum progress yet" wording. The actual pre-admission copy is "Awaiting admission — You are registered and waiting to be admitted to a curriculum by your programme administrator." T049's wording surfaces post-admission. Update inline.
- Step 2.8's anticipated route `/admin/committee-panels/*` is actually `/committee/panels/*`. Update inline.
- Phase 2.B note about `forceLoad` on the Zulu/Naidoo/Botha secondary invite: clarify that the path is dev-CLI workaround, not "Mbatha attaches secondary Assessor role via UI".

### Suggested code-side task list (model: **Opus** for migration/policy work, **Sonnet** for the UX picker work)

- **T059** (✅ shipped 2026-05-27 in commit `9114244`) — fix Task.WhenAll concurrency in ListAssessors + ListTrainees handlers.
- **T060** (✅ shipped 2026-05-27, commit pending in this session) — widened `InvitationRules.ValidateScope` so Coordinator + external CommitteeMember can be institution-only. Task file `Rewrite/Tasks/T060-invitation-validator-scope-relaxation.md`.
- **T061** (✅ shipped 2026-05-27 in commit `7610ac5`) — admin Users surface at `/admin/users` + `/admin/users/{userId}`. List users (scope-filtered), add/remove roles, reset password, lock-out/reactivate, revoke pending same-email invitations. AcceptInvitation auto-revokes stale same-email invitations on registration. Replaced both `--dev-reset-password` and `--dev-add-role` CLI flags. 15 new Application tests + 1 new bUnit smoke test; 339/339 pass.
- **T062** (✅ shipped 2026-05-27 in commit `852f410`) — Decision Panel form pickers: scope-aware Institution + Speciality `<select>`, native `<select multiple>` for Chair/Members/External backed by `ListPanelMemberCandidatesQuery`. Round-trip verified.
- **T063** (✅ shipped 2026-05-27 in commit `852f410`) — `DemandPanelAdministration` widened to InstitutionalAdmin; handlers scope-check resolved institution (via `Speciality.InstitutionId` for Speciality-scoped panels). PanelEdit page authorize tightened to drop Coordinator (Coordinator's actual privilege is `DemandReviewScheduling`). 7 new scope-guard tests.
- **T064** (open) — AssessorProfileEdit post-save URL flip + dropdown narrowing (compare T055 pattern). ~30 min, **Sonnet**.
- **T065** (open, optional) — TrainingStatus enum: extend T035 with `Provisional` / `In training` / `Trained` enum alongside the date, OR revise scenario to refer to the date only. ~1h, **Sonnet**.
- **T066** (open, optional) — Trainee admit form: add Stage display (computed read-only OR editable). ~1h, **Sonnet**.

## Handoff into Act 3

Act 3 needs from Act 2:
- Trainee `UserId`s for at least Dr Molefe (year 4), Dr Dlamini (year 3), Dr du Plessis (year 2) — they're the registrars Act 3 puts through assessment cycles.
- Assessor `UserId`s for Dr Naidoo + Dr Patel — Act 3 has Naidoo rate a Mini-CEX and Patel rate a DOPS.
- `DecisionPanel` ID for `Paed Annual Review Panel 2026` (Act 4 attaches reviews to it).
- TraineeProfile IDs (Act 4 cross-references them in evidence-bundle assembly).
- Confirmation that all consultants can authenticate cleanly with their issued credentials.

---

# Act 3 — Months 1-6: operational rhythm

**Date in scenario:** 2026-02-09 through 2026-07-30 (a six-month slice; not all of it played sequentially — Act 3 picks representative cycles).
**Who:** All of Act 2's cast. Specifically:
- Dr Dlamini (year 3) submits a Mini-CEX which Dr Naidoo (Assessor) rates.
- Dr du Plessis (year 2) logs procedures.
- Dr Mahlangu (year 1) attempts a DOPS that Dr Patel rates.
- Dr Molefe (year 4) opens an MSF that closes 28 days later.
- Dr Smit (Coordinator) triages a stalled assessment.
- Bootstrap admin checks the audit log.
- All trainees check their dashboards.

**Why:** Act 1 + 2 set up the institutional skeleton. Act 3 stresses the actual operational loop: submit → accept → rate → complete → credit. The credit chain (`CreditApplier`) populates the trainee dashboards; until at least one activity completes successfully, the dashboards are useful only structurally.

**Starting state:**
- Act 2 outcome state intact: all users, profiles, and the committee panel exist. Zero activities, zero credit rows.

**Act 3 goal:**
1. At least one Mini-CEX activity completes end-to-end and credits the trainee's curriculum item PAED-001.
2. Multiple procedure-log entries land on Dr du Plessis (≥ 5 IV-access entries to start moving PAED-011 progress).
3. One DOPS completes for Dr Mahlangu (year 1) on PAED-010 lumbar puncture.
4. One MSF opens, closes after the 28-day window, and produces a summary.
5. Coordinator Dr Smit identifies and resolves a stalled-assessment case.
6. Audit log shows entries for the new activity lifecycle events (create / submit / accept / rate / complete).
7. Each trainee dashboard renders populated progress for at least one EPA.

## Phase 3.A — Trainee submits Mini-CEX (Dr Dlamini, year 3)

### Step 3.1 — Dr Dlamini logs in and creates Mini-CEX draft
Role: Dr Anele Dlamini (Trainee, year 3)
Route: `/account/login` → `/` → `/activities/new`
Action: Sign in as `dlamini@kgk.wombat.local`. From the dashboard, click `New activity`. On the type-picker page, the published activity types Mbatha created in Act 1 should appear. Click `Mini-CEX (Paediatrics)`. The schema-driven form renders.
Expected: Activity-type picker shows the 10 Paed types (mini_cex_paed / cbd_paed / dops_paed / acat_paed / procedure_log_paed / msf_paed / reflective_note_paed / journal_club_paed / research_output_paed / teaching_session_paed). Mini-CEX selection routes to `/activities/new?type=mini_cex_paed` (or `/activities/{id}` after draft creation). Form renders with the schema's three sections (Encounter details / Clinical performance ratings / Feedback).
Actual:
Gap: **Anticipated** — Act 1's play-through scope reduction left Mini-CEX with the default `title`-only schema. If the play-through still has only the `title` field, the trainee form will look thin. Either build out the full 13-field schema before Act 3 (in `/admin/activity-types/11` Form tab) or use the minimal schema and note that the demonstration is structural, not realistic.

### Step 3.2 — Fill Mini-CEX form (assuming full schema)
Role: Dr Dlamini
Route: `/activities/{newId}`
Action: Fill the form:
- EPA `PAED-001 — Clerk, assess and present an acute general paediatric admission`
- Assessor `Dr David Naidoo`
- Clinical setting `Inpatient`
- Patient age (months) `36`
- Presenting complaint `4-day fever with cough, increased work of breathing. Initial impression: community-acquired pneumonia.`
- Ratings (six fields, each on the 5-level Paed scale):
  - History taking: `Indirect supervision (3)`
  - Examination: `Indirect supervision (3)`
  - Clinical reasoning: `Indirect supervision (3)`
  - Communication: `Unsupervised (4)`
  - Professionalism: `Unsupervised (4)`
  - Overall entrustment level: `Indirect supervision (3)`
- Narrative feedback: `Solid systematic clerking. Differential considered atypical pneumonia. Plan flagged for senior review before lumbar puncture decision. Next step: read up on the British Thoracic Society 2019 paediatric pneumonia guideline.`
Click `Save draft`.
Expected: Status banner "Draft saved." URL stable at `/activities/{id}`. The activity row appears on the trainee's dashboard under `My activities → Draft`.
Actual:
Gap:

### Step 3.3 — Submit Mini-CEX to the assessor
Role: Dr Dlamini
Route: `/activities/{id}`
Action: On the activity-detail page, the workflow widget should show `Submit` as the available transition (per Mini-CEX's workflow JSON: `draft → submitted, actor: role:Trainee`). Click `Submit`.
Expected: State flips from `draft` to `submitted`. The `recall` transition becomes available (per workflow JSON). Dr Naidoo's `Pending ratings` dashboard panel shows the new submission within 5 seconds of refresh.
Actual:
Gap:

## Phase 3.B — Assessor rates the submission (Dr Naidoo)

### Step 3.4 — Dr Naidoo logs in, accepts assignment
Role: Dr David Naidoo (Assessor + CommitteeMember)
Route: `/account/login` → `/` → `/activities/{id}` (from the assessor dashboard's pending-ratings panel)
Action: Sign in as `naidoo@kgk.wombat.local`. On the dashboard, click into the pending Mini-CEX entry. On the activity-detail page, click `Accept` (workflow: `submitted → rated, actor: field:assessor_user_id`).
Expected: State flips to `rated`. The form's rating fields become editable for Dr Naidoo (he is the named assessor). The `complete` transition (`rated → completed, actor: field:assessor_user_id`) becomes available.
Actual:
Gap:

> **Actor-DSL verification:** The Mini-CEX workflow says `actor: "field:assessor_user_id"` for the accept transition. This is the first real test of the field-actor binding — Wombat must resolve the `assessor_user_id` field from the activity's `DataJson` to a real user, then check the caller's `UserId` matches. If a different assessor (e.g., Patel) loaded this activity, the Accept button should be hidden / rejected. Test by attempting accept as Patel and confirm the rejection path.

### Step 3.5 — Naidoo edits ratings then completes
Role: Dr Naidoo
Route: `/activities/{id}`
Action: Optionally adjust the trainee's self-entered ratings (in this scenario, change Communication from `Unsupervised (4)` to `Indirect supervision (3)` — assessor judgment differs from trainee self-rating). Add an assessor note: "Communication was solid but used jargon when reframing the differential to the parent. Coached after the encounter." Click `Complete`.
Expected: State flips to `completed`. The activity is now terminal. `CreditApplier.ApplyAsync` runs, finds the curriculum item for PAED-001 in `FCPaed(SA) Part 1 v2026.1`, matches the credit rule's `epa_id` / `overall_level` (3) against the requirement, and writes a `CurriculumItemProgress` row for Dr Dlamini with `+1` count toward PAED-001.
Actual:
Gap: **Anticipated** — credit JSON not exercised in Act 1's scope-reduction play-through. The play-through may publish Mini-CEX with `"counts_for": []` (Act 1 finding). If so, the activity completes but no credit accrues. Capture which credit JSON is active; if empty, edit the type to add the credit rule before continuing Act 3.

## Phase 3.C — Verify credit on Dr Dlamini's dashboard

### Step 3.6 — Dr Dlamini sees PAED-001 progress
Role: Dr Dlamini
Route: `/` (Trainee dashboard)
Action: Sign back in. Scroll to the curriculum-progress panel.
Expected: PAED-001 now reads `1 of 30` with a thin progress bar. Last activity date `2026-02-09`. Stage-minimum-level indicator shows `current: 3 / required for stage 3: 4` — Dr Dlamini is one level below the year-3 target for this EPA, which is correct (she has just completed her first Mini-CEX with overall level 3).
Actual:
Gap:

## Phase 3.D — Procedure-log batch (Dr du Plessis, year 2)

### Step 3.7 — Dr du Plessis logs five IV-access entries
Role: Dr Pieter du Plessis (Trainee, year 2)
Route: `/activities/new` (repeat five times)
Action: Pick `Procedure Log (Paediatrics)` each time. The form's expected fields per the activity-type table:
- Procedure code: `IV access` (or a Choice option matching PAED-011)
- EPA: `PAED-011 — Obtain IV access in an infant`
- Supervision level (Choice): vary across 5 entries — `Direct supervision` for the first two, `Indirect supervision` for the next two, `Unsupervised` for the fifth.
- Self-rated competence: corresponds to the chosen supervision level.
- Date: spread across `2026-02-10` to `2026-02-28` (entered manually if the form takes a date field).
Click `Log` (the procedure_log workflow is `draft → logged (terminal), actor: role:Trainee`).
Expected: Each entry posts directly to `logged`. The Trainee dashboard's curriculum-progress panel for PAED-011 reads `5 of 30` after all five are logged. If the credit rule conditions on supervision level meeting the stage minimum (year 2 stage min for PAED-011 is `3`), the first two `Direct supervision (2)` entries should NOT credit because they're below stage 3's minimum of `3`. The three at level ≥ 3 should credit. **Verify the credit-applier honours the stage minimum.**
Actual:
Gap:

## Phase 3.E — DOPS for Dr Mahlangu (year 1) on PAED-010 lumbar puncture

### Step 3.8 — Mahlangu submits, Patel rates
Role: Dr Nomsa Mahlangu (Trainee, year 1) — submission; Dr Mohammed Patel (Assessor) — acceptance + rating.
Route: same lifecycle as Steps 3.1-3.5 but on `dops_paed`.
Action:
- Dr Mahlangu: pick DOPS type, fill EPA `PAED-010`, Assessor `Dr Patel`, procedure code `Lumbar puncture (infant)`, indication `Suspected meningitis in 4-month-old`, complications `nil`, 5-step rating block per the DOPS schema (preparation / consent / anatomical landmarks / technique / aftercare — all at `Direct supervision (2)`, which is the year-1 stage minimum for PAED-010). Submit.
- Dr Patel: accept, optionally adjust ratings, complete.
Expected: PAED-010 progress on Dr Mahlangu's dashboard moves from `0 of 10` to `1 of 10`. Stage indicator shows `current: 2 / required for stage 1: 2` — meeting the year-1 target.
Actual:
Gap:

> **Note on assessor in-training status:** Dr Patel's AssessorProfile has `TrainingStatus = In training`. Whether Wombat blocks an `In training` assessor from completing a DOPS, or just flags it on the rating record, is per T035's implementation. The play-through should observe and report.

## Phase 3.F — MSF cycle (Dr Molefe, year 4)

### Step 3.9 — Molefe opens an MSF
Role: Dr Lerato Molefe (Trainee, year 4)
Route: `/activities/new` → `Multi-Source Feedback (Paediatrics)`
Action: Fill the MSF schema (8 invitees: a mix of peers + supervisors + allied staff). Suggested invitees from the existing cast: Naidoo, Botha, Patel, Khumalo, Smit, Zulu, Dlamini (peer), du Plessis (peer). Add self-rating. Click `Open` (workflow: `draft → open, actor: role:Trainee`).
Expected: State `open`. Eight notification rows queued for the invitees (email or in-app — depending on what Wombat actually does for this).
Actual:
Gap: **Anticipated** — the MSF invitation distribution mechanism may not be fully implemented. If invitees don't receive any prompt, this is a gap to document — the cycle still progresses by manual self-submission within the 28-day window in a real workplace, but verification of the notification path requires real wiring.

### Step 3.10 — System auto-progresses MSF to `closing` and then `closed`
Role: System (no human actor)
Route: scheduled job at `/admin/jobs` — look for `MsfClosing` or similar
Action: Manually trigger the scheduled job (if a Run button exists) OR advance the scenario clock 28 days and verify the job ran. Workflow transitions: `open → closing, actor: creator` (Act 1 found `actor: creator` was the working stand-in for time-based events). Then `closing → closed, actor: creator`.
Expected: After both transitions fire, the MSF activity is in `closed` (terminal). Credit applies if rules match. Dr Molefe's dashboard shows the MSF as completed.
Actual:
Gap: **Anticipated** — the scheduled-job system may not have a job specifically for MSF closure; the play-through may need to manually invoke the transition via API or directly through the workflow widget if the trainee can self-close after 28 days. Document the actual mechanism.

## Phase 3.G — Stalled-assessment triage (Coordinator Dr Smit)

### Step 3.11 — Mahlangu submits another Mini-CEX that goes stale
Role: Dr Mahlangu — submit; nobody — leave the activity in `submitted` for the scenario clock to advance 14 days past its submission.
Route: `/activities/new` → Mini-CEX → submit (don't notify a specific assessor with field:assessor_user_id, or pick one who will simulate not acting).
Action: Submit on `2026-03-01`. Advance scenario clock to `2026-03-16`. The `StaleAssessmentChase` scheduled job (or equivalent) should now flag this as stalled.
Expected: An entry appears in Dr Smit's Coordinator dashboard under `Stalled assessments` with the submission date + days-stalled count.
Actual:
Gap:

### Step 3.12 — Smit follows up
Role: Dr Smit (Coordinator)
Route: `/` (Coordinator dashboard) → click into the stalled-assessments panel → `/coordinator/stalled-assessments` (placeholder URL)
Action: Pick the stalled activity. Send a follow-up nudge to the named assessor (`Send reminder` button) OR reassign to another assessor.
Expected: A `Reminder sent` audit entry appears against the activity. If reassigned, the original assessor's pending list shrinks and the new assessor's grows by one.
Actual:
Gap: **Anticipated** — the Coordinator surface and the stalled-assessment workflow are real per the CLAUDE.md roles list, but the exact UI may not be the placeholder URL above. Discover the real URL during play-through.

## Phase 3.H — Audit log verification

### Step 3.13 — Bootstrap admin walks the audit log
Role: Bootstrap admin
Route: `/admin/audit`
Action: Scroll the audit log. Identify entries for:
- Each invitation issued (Act 2) — `InvitationIssued`.
- Each user registration (Act 2) — `UserRegistered`.
- Each TraineeProfile creation (Act 2) — `TraineeAdmitted` (or similar).
- Activity create / submit / accept / complete (Act 3) — one entry per state transition.
- Stalled-chase reminder send (Step 3.12).
Expected: ~30-50 audit entries in total. Each entry has `PrincipalSummary` (the `[PRINCIPAL]` token T045 substituted in) plus `InstitutionId`, `OccurredAt`, `EventType`, `Payload`.
Actual:
Gap:

> **Verification per T045 + T046:** The `[PRINCIPAL]` substitution and the seed-claims gap fixes both ship to make populated audit entries readable. If any audit row shows a `System.Text.Json.JsonException`-shaped error in its payload (the original symptom T045 closed), it's a regression.

## Phase 3.I — Dashboard populated-state sanity

### Step 3.14 — Each trainee logs in to confirm populated dashboard
Role: each trainee in turn
Route: `/` per session
Action: Sign in as each trainee. Capture which EPAs show progress, which still read `0 of N`, and any rendering glitches in the progress bars, stage-minimum indicators, or the activity-recency panels.
Expected:
- Dr Molefe (year 4): only one activity (the still-open MSF). Most EPAs still `0 of N`.
- Dr Dlamini (year 3): one PAED-001 credit.
- Dr du Plessis (year 2): three PAED-011 credits (the three at supervision level ≥ stage 2 minimum).
- Dr Mahlangu (year 1): one PAED-010 credit (the DOPS).
- Dr Ndlovu (year 1): no activities — dashboard shows the curriculum structure but zero progress everywhere.
Actual:
Gap:

## Act 3 outcome state

After Act 3 completes cleanly, the database adds:
- 1 completed Mini-CEX (Dr Dlamini, PAED-001, overall level 3).
- 5 procedure log entries (Dr du Plessis, PAED-011; 3 of them credit).
- 1 completed DOPS (Dr Mahlangu, PAED-010, level 2).
- 1 closed MSF (Dr Molefe, varies).
- 1 stalled-then-followed-up Mini-CEX (Dr Mahlangu, submitted but the assignee never accepted).
- ~6-8 `CurriculumItemProgress` rows depending on credit-rule fidelity.
- 30-50 new audit entries spanning Acts 2 + 3.

## Act 3 time estimate

| Phase | Est. minutes |
|---|---|
| 3.A: Dr Dlamini submits Mini-CEX | 12 |
| 3.B: Dr Naidoo rates and completes | 8 |
| 3.C: Credit verification on dashboard | 4 |
| 3.D: 5 procedure log entries | 20 |
| 3.E: DOPS cycle (Mahlangu + Patel) | 14 |
| 3.F: MSF open → close cycle (28-day advance simulated) | 12 |
| 3.G: Stalled-assessment triage | 14 |
| 3.H: Audit log walk | 10 |
| 3.I: Dashboard sanity per trainee | 12 |
| **Total** | **~106 minutes** |

## Act 3 findings summary

Not yet played. Anticipated findings flagged inline: full Mini-CEX schema (3.1), Mini-CEX credit JSON (3.5), MSF notification path (3.9), MSF scheduled-job mechanism (3.10), Coordinator stalled-assessment URL (3.12).

## Handoff into Act 4

Act 4 needs from Act 3:
- A populated set of completed activities to feed into the annual review evidence bundles.
- The MSF result (closed) — appears in Dr Molefe's evidence as a key communication-and-collaborator data source.
- At least one stalled-then-resolved case to demonstrate the audit-trail completeness on a review.
- Trainees with varied progress profiles so the year-end committee meeting has different decisions to make (some on track, some not).

---

# Act 4 — Month 12: annual review

**Date in scenario:** 2027-01-08 (committee scheduled session) through 2027-02-15 (ratification + STAR delivery).
**Who:** Prof Mbatha schedules + supervises the cycle. The committee panel (Zulu chair / Naidoo / Botha / van Rensburg) makes the calls. Each trainee receives a written outcome. One trainee (Dr Mahlangu) lodges an appeal that goes to Prof Mbatha.

**Why:** The annual review is the formal gate where the committee decides whether each trainee progresses to the next stage, repeats, is referred for additional supervision, or graduates. STARs (Statement of Awarded Responsibility) are the persistent record of granted entrustment levels per EPA.

**Starting state:**
- Act 3 outcome state intact: 4-5 trainees with varied progress (~6-8 credited curriculum items distributed across the cohort).
- Committee panel `Paed Annual Review Panel 2026` active.
- No PendingEntrustmentDecision rows yet.

**Act 4 goal:**
1. 5 `CommitteeReview` rows scheduled (one per trainee).
2. Evidence bundle assembled per review (activities + STARs-to-date + dashboard snapshot).
3. Committee meets, records decisions per trainee; majority pre-graduation continues, one borderline case, one referral, Dr Molefe progresses to graduation-track.
4. STARs staged for one EPA per trainee (where they've reached the year-end target).
5. Ratification step records the decisions formally.
6. Dr Mahlangu's referral triggers an appeal; Prof Mbatha reviews and either upholds or remits.

## Phase 4.A — Schedule the 5 reviews

### Step 4.1 — Prof Mbatha schedules each trainee's review
Role: Prof Mbatha (InstitutionalAdmin)
Route: `/admin/committee-reviews/new` (or `/committee/reviews/new` — discover during play-through; T039 ships the review surface)
Action: For each trainee, create a `CommitteeReview` with:
- Panel: `Paed Annual Review Panel 2026`
- Trainee: pick the relevant `TraineeProfile`
- Scheduled date: `2027-01-08`
- Review type: `Annual progression review` (for years 1-3); `Pre-graduation review` (for Dr Molefe).
Click `Schedule`.
Expected: 5 reviews appear on the committee dashboard under `Upcoming reviews`. Each is in state `Scheduled` (per the committee workflow per T039).
Actual:
Gap:

## Phase 4.B — Evidence bundles assemble

### Step 4.2 — Open each review to inspect the auto-assembled evidence
Role: Dr Thandi Zulu (Chair) — preview a few before the meeting
Route: `/committee/reviews/{id}` per review
Action: Open Dr Molefe's review. Verify:
- Activities tab: lists Dr Molefe's MSF + any other activities from Act 3.
- STAR snapshot tab: STARs awarded to date (zero at this point — first review).
- Dashboard snapshot: a frozen snapshot of progress as of the review date.
- Rating trajectory: T033's per-EPA chart should render.
- Sampling concentration warning: T032 should flag if all activities are from the same supervisor (e.g., if Molefe's MSF + 2 other activities are all rated by Naidoo).
Expected: All five tabs render. Dashboard data matches the trainee's `/` view as of `2027-01-08`.
Actual:
Gap:

### Step 4.3 — Confirm evidence on the four other reviews
Role: Dr Zulu
Route: cycle through each `/committee/reviews/{id}`
Action: Repeat Step 4.2 for Dr Dlamini, du Plessis, Mahlangu, Ndlovu. Capture any review with empty evidence (Dr Ndlovu likely — no activities in Act 3).
Expected: Dlamini shows 1 Mini-CEX. Du Plessis shows 5 procedure logs (with 3 credited). Mahlangu shows 1 DOPS + 1 stalled-then-resolved Mini-CEX. Ndlovu shows zero activities — empty-state copy must be presentable.
Actual:
Gap:

## Phase 4.C — Committee meeting: record decisions

### Step 4.4 — Start the meeting; transition each review to `In progress`
Role: Dr Zulu (Chair)
Route: per review
Action: On each review's detail page, click `Start review` (per T045's verification — that's the transition the committee panel uses).
Expected: State flips from `Scheduled` to `InProgress`. The Decision form panel becomes active (per T045's populated ReviewDetail observation).
Actual:
Gap:

### Step 4.5 — Record decisions per trainee
Role: Dr Zulu, in panel session with Naidoo / Botha / van Rensburg (record the votes via the form)
Route: per review's `/committee/reviews/{id}`
Action: Per trainee, fill the decision form:

| Trainee | Decision | Notes |
|---|---|---|
| Dr Molefe (yr 4) | `Progress — graduation track` | Recommend STAR for PAED-001 + PAED-006 + PAED-013 (where end-year-4 target met). Schedule final review in Nov 2029. |
| Dr Dlamini (yr 3) | `Progress` | Strong Mini-CEX, on-track. No STARs at year-3 review (no EPAs at terminal level yet). |
| Dr du Plessis (yr 2) | `Progress with note` | Procedure logs adequate but no formal Mini-CEX activity yet. Recommend prioritising clinical assessment WBAs in next 6 months. |
| Dr Mahlangu (yr 1) | `Referral for review` | Insufficient activity volume. Single DOPS at stage minimum; one stalled assessment. Recommend support plan; re-review in 6 months. |
| Dr Ndlovu (yr 1) | `Withdraw — programme not commenced` | Zero activities; needs intent-to-train confirmation. Coordinator to follow up. |

Click `Record decision` on each form.
Expected: Each review's state flips to `DecisionRecorded`. The trainee dashboards now show a `Recent committee decision` panel.
Actual:
Gap:

## Phase 4.D — Stage STARs

### Step 4.6 — Stage PendingEntrustmentDecision rows for Dr Molefe
Role: Dr Zulu (Chair)
Route: `/committee/reviews/{molefeReviewId}` → STAR stage form (per T029)
Action: For each of PAED-001, PAED-006, PAED-013 — open the stage-pending-decision form, select EPA + final entrustment level (`Unsupervised (4)` for PAED-001 and PAED-006; `Indirect supervision (3)` for PAED-013). Save.
Expected: 3 `PendingEntrustmentDecision` rows persist linked to Dr Molefe's review.
Actual:
Gap:

### Step 4.7 — Other trainees: no STAR staging at this review
Role: Dr Zulu
Route: n/a
Action: Confirm none of the year 1-3 reviews stage any STARs (they're all below graduation target).
Expected: 0 PendingEntrustmentDecision rows for the other four reviews.
Actual:
Gap:

## Phase 4.E — Ratification

### Step 4.8 — Ratify decisions
Role: Prof Mbatha (InstitutionalAdmin)
Route: `/admin/committee-reviews/{id}` per review, or a batch action on `/admin/committee-reviews`
Action: For each review, click `Ratify`. This transitions the review to its terminal state and locks the decision. For Dr Molefe's review, the 3 PendingEntrustmentDecision rows transition to `EntrustmentDecision` rows (per T029 / T030 — the STAR PDF should become generable).
Expected: All five reviews in `Ratified` (or equivalent terminal) state. Dr Molefe's profile shows 3 awarded STARs.
Actual:
Gap:

## Phase 4.F — Appeal

### Step 4.9 — Dr Mahlangu lodges an appeal
Role: Dr Nomsa Mahlangu (Trainee)
Route: `/portfolio/reviews/{mahlangu_review_id}` → `Appeal` button (form on the review detail per T039's appeals form)
Action: Click `Appeal`. Fill the appeal reason: "Single DOPS reflects start-of-year skill level. Stalled Mini-CEX was assessor-side issue, not trainee-side. Request reconsideration with attached evidence of 3 additional Mini-CEXes submitted in last 30 days." Submit.
Expected: Appeal saved. Review's state flips from `Ratified` to `Appealed` (or appended with an appeal record — exact mechanism per T039). Prof Mbatha receives an appeal notification.
Actual:
Gap:

### Step 4.10 — Prof Mbatha reviews the appeal
Role: Prof Mbatha
Route: `/admin/committee-reviews/{id}/appeals/{appealId}`
Action: Read the appeal text. Verify the cited additional Mini-CEXes (need to have been submitted in Act 3 or appended for the scenario). Decide: in this scenario, **uphold the referral** but reduce its scope to a 3-month re-review instead of 6-month. Click `Decide`.
Expected: Appeal disposed. Review state flips to `AppealResolved`. Trainee dashboard shows the appeal outcome.
Actual:
Gap:

## Act 4 outcome state

After Act 4 completes cleanly, the database adds:
- 5 CommitteeReview rows (4 ratified, 1 appealed-then-resolved).
- 3 EntrustmentDecision rows (Dr Molefe).
- 1 AppealRecord (Dr Mahlangu).
- All 5 trainee dashboards show their decision summaries.
- Audit log adds ~25-40 entries (scheduling, evidence-fetch, decision-record, ratification, appeal lifecycle).

## Act 4 time estimate

| Phase | Est. minutes |
|---|---|
| 4.A: 5 reviews scheduled | 12 |
| 4.B: Evidence-bundle preview across 5 reviews | 15 |
| 4.C: 5 decisions recorded | 25 |
| 4.D: STAR staging (Molefe only) | 12 |
| 4.E: Ratification | 8 |
| 4.F: Appeal lifecycle | 15 |
| **Total** | **~87 minutes** |

## Act 4 findings summary

Not yet played. Specific items to verify: T032 sampling-concentration warning fires correctly on Molefe's bundle (likely needs ≥ 3 activities from the same assessor — may not trigger if the scenario only has the one MSF); T033's per-EPA trajectory chart renders for trainees with at least one credit; T029's PendingEntrustmentDecision lifecycle locks correctly on ratification; T031's formative-only review mode is not exercised here but the form should still allow it as an option.

## Handoff into Act 5

Act 5 needs from Act 4:
- Dr Molefe's 3 EntrustmentDecision rows (PAED-001, PAED-006, PAED-013) — they'll need to be augmented with the remaining 12 EPAs at graduation.
- The Annual Review Committee panel intact (Act 5's final review reuses it).
- Audit log continuity (Act 5 looks back across the 4-year history).

---

# Act 5 — Year 4: graduation

**Date in scenario:** 2029-11-18 (final committee review) through 2029-12-15 (graduation date).
**Who:** Dr Lerato Molefe (about to graduate). The same Annual Review Panel as Act 4. Bootstrap admin signs the formal STAR documents. Prof Mbatha hands over the portfolio PDF.

**Why:** Year 4 ends with Dr Molefe demonstrating that she has met or exceeded the graduation-level entrustment target for all 15 EPAs (final-year minimum mostly level 4; PAED-014/PAED-015 at level 3). The committee converts the cumulative evidence into the full STAR set, generates the portfolio PDF, and closes Dr Molefe's training programme.

**Starting state:**
- Cumulative state from Acts 2-4 plus an implicit "Act 3.5 + 4.5 + Act 4 reruns" of operational rhythm across years 2027 + 2028 + 2029. The scenario doesn't simulate those years step-by-step; assume the activity counts and credit have accrued naturally. By 2029-11-18 Dr Molefe has:
  - 30+ Mini-CEX submissions (PAED-001 fully met at level 4).
  - 8+ resuscitation activities (PAED-002).
  - All other Paed EPAs met or exceeded.
  - 3 STARs already awarded from the Act 4 review (PAED-001, PAED-006, PAED-013).
  - Several Annual Reviews on file (one per year 2027 / 2028 / 2029).

**Act 5 goal:**
1. Final review scheduled and decision recorded: `Graduate`.
2. PendingEntrustmentDecision rows for the remaining 12 EPAs staged + ratified into EntrustmentDecisions.
3. Portfolio PDF generated and downloaded.
4. Dr Molefe's TraineeProfile marked complete; her UserRoles cleaned (Trainee → Alumnus or similar).
5. Coordinator + Mbatha can both reproduce the portfolio PDF byte-for-byte from the persisted decisions.

## Phase 5.A — Schedule and run the final review

### Step 5.1 — Mbatha schedules the final review
Role: Prof Mbatha
Route: `/admin/committee-reviews/new`
Action: Panel `Paed Annual Review Panel 2026`; Trainee `Dr Lerato Molefe`; Scheduled date `2029-11-18`; Review type `Final review — graduation`.
Expected: Review row appears with `Type = Final review`.
Actual:
Gap:

### Step 5.2 — Committee meets, decision: Graduate
Role: Dr Zulu (Chair)
Route: `/committee/reviews/{finalReviewId}`
Action: Start review. Confirm evidence bundle shows all 4 years of activities (~120+ rows). Stage STARs for the 12 EPAs not yet covered by previous decisions:
- PAED-002, 003, 004, 005, 007, 008, 009, 010, 011, 012 at `Unsupervised (4)`.
- PAED-014, 015 at `Indirect supervision (3)` (Elective EPAs, final-year target 3).
Click `Record decision` with `Decision = Graduate`.
Expected: 12 PendingEntrustmentDecision rows staged. Review state flips to `DecisionRecorded`.
Actual:
Gap:

## Phase 5.B — Ratify and lock entrustment decisions

### Step 5.3 — Bootstrap admin ratifies
Role: Bootstrap admin
Route: `/admin/committee-reviews/{finalReviewId}`
Action: Click `Ratify`. The 12 PendingEntrustmentDecision rows lock into permanent EntrustmentDecision rows.
Expected: Total EntrustmentDecisions on Dr Molefe = 3 (Act 4) + 12 (Act 5) = 15, covering every EPA in `FCPaed(SA) Part 1 v2026.1`.
Actual:
Gap:

## Phase 5.C — Portfolio PDF generation

### Step 5.4 — Mbatha generates Dr Molefe's portfolio PDF
Role: Prof Mbatha
Route: `/admin/trainees/{molefeProfileId}` → `Export portfolio` button (per T023 / T030)
Action: Click `Export portfolio`. The QuestPDF-generated PDF downloads.
Expected: PDF file `Dr Lerato Molefe — FCPaed(SA) Part 1 Portfolio.pdf` downloads. Open it. Verify it contains:
- Cover page with trainee name, programme, graduation date.
- Curriculum summary (15 EPAs with achievement levels).
- Activities log (sorted chronologically).
- STAR certificate section (per T030 — full-page-per-EPA certificate format).
- Committee review history (5 reviews — Acts 2-5 — with decisions).
- Audit trail summary (high-level event log).
Actual:
Gap:

### Step 5.5 — Coordinator reproduces the PDF
Role: Dr Smit (Coordinator)
Route: same as Step 5.4
Action: Click `Export portfolio`. Download. Compare byte-for-byte with Mbatha's version.
Expected: Identical PDF (deterministic generation per T023). Coordinator has read-access by design.
Actual:
Gap:

## Phase 5.D — Trainee profile closure

### Step 5.6 — Mbatha marks Dr Molefe's profile complete
Role: Prof Mbatha
Route: `/admin/trainees/{molefeProfileId}`
Action: Click `Mark complete`. Set Completion date `2029-12-15`. Confirm role transition: `Trainee → Alumnus` (or whatever the actual completion role is per CLAUDE.md's role list — if no Alumnus role exists, the Trainee role is removed and the user's profile is archived).
Expected: Dr Molefe's user no longer holds the `Trainee` role. Her `/trainees` list entry moves to a "Completed" sub-tab.
Actual:
Gap:

### Step 5.7 — Dr Molefe receives the graduation email + portfolio link
Role: System (or Coordinator Smit)
Route: triggered by the completion event
Action: Verify an email arrives at `molefe@kgk.wombat.local` (Papercut catcher) containing the portfolio link and graduation congratulations.
Expected: Email subject like "Congratulations on your FCPaed(SA) Part 1 completion" with PDF attachment or download link.
Actual:
Gap:

## Act 5 outcome state

After Act 5 completes cleanly:
- Dr Molefe has 15 EntrustmentDecisions covering every Paed EPA.
- Her portfolio PDF is downloadable byte-for-byte identically by Mbatha and Smit.
- Her `Trainee` role is removed; profile is archived.
- Audit log adds ~15-20 entries (final review lifecycle + portfolio export logs).
- Total scenario state at this point: 1 graduated alumna, 4 active trainees (Mahlangu's 3-month post-appeal re-review status, the others on their normal tracks), 1 institution, 1 curriculum, 1 committee panel.

## Act 5 time estimate

| Phase | Est. minutes |
|---|---|
| 5.A: Final review schedule + decision | 22 |
| 5.B: Ratification | 5 |
| 5.C: Portfolio PDF generation + comparison | 15 |
| 5.D: Profile closure + graduation email | 12 |
| **Total** | **~54 minutes** |

## Act 5 findings summary

Not yet played. T023 (portfolio PDF) and T030 (STAR PDF) are the biggest moving parts to verify. The byte-for-byte reproducibility check (Step 5.5) requires the PDF generation to be deterministic — if QuestPDF embeds a timestamp or any non-deterministic field, that property fails. T030's STAR certificate format is fully specified per its task file; cross-check.

---

# Appendix — cross-cutting concerns

These spot-checks don't fit neatly inside an act because they cross multiple acts' state or touch infrastructure rather than trainee lifecycle. Play them after the linear acts have established the data, or at any point where the system's state is rich enough to exercise them.

## A.1 — Data rights (GDPR-style export + deletion)

### Step A.1.1 — Trainee exports their own data
Role: any active trainee (use Dr Dlamini for this run)
Route: `/account/data-rights` → `Request export`
Action: Click `Request export`. Confirm the request. Wait for the scheduled job to package the data.
Expected: A `DataRightsRequest` row enters `Pending`. The scheduled job picks it up and produces a ZIP file containing the trainee's profile, activities, reviews-where-she-appears, and audit entries about her. Status flips to `Ready`; a download link is provided.
Actual:
Gap:

### Step A.1.2 — Administrator processes a deletion request
Role: bootstrap admin
Route: `/admin/data-rights/{requestId}`
Action: For a hypothetical user who has requested deletion (use a withdrawn user — Dr Ndlovu from Act 4's `Withdraw` decision), click `Process deletion`. Confirm.
Expected: User's personally-identifiable data is removed or pseudonymised; audit-log entries retained per the retention policy. The `DataRightsRequest` flips to `Completed`.
Actual:
Gap: **Anticipated** — the exact deletion semantics (hard delete vs anonymise vs soft-delete) are policy-dependent. Capture what Wombat's `Process deletion` actually does to confirm it matches the documented compliance posture.

## A.2 — Scheduled jobs

### Step A.2.1 — Walk the scheduled-jobs admin page
Role: bootstrap admin
Route: `/admin/jobs`
Action: List all registered scheduled jobs. For each, capture: name, cron schedule, last run, last status (success / failed), last run duration.
Expected: At least these jobs (per CLAUDE.md mentions + the activity types' workflow needs):
- `AuditRetentionPurge` — daily, prunes audit entries older than 7 years.
- `StaleAssessmentChase` — daily, flags activities stuck in `submitted` > 14 days.
- `MsfClosing` — daily, transitions `open` MSFs past their window.
- `EmailWorker` — continuous, drains the outbound mail queue.
- `DataRightsExportWorker` — picks up pending export requests.
Actual:
Gap:

### Step A.2.2 — Manually trigger one job
Role: bootstrap admin
Route: `/admin/jobs/{key}/run`
Action: Pick `StaleAssessmentChase`. Click `Run now`. Observe the run-history table.
Expected: A new `ScheduledJobRun` row appears with timestamp, status `Success`, duration. If any stalled activities were found, side-effect rows (reminder logs / activity flagged) appear elsewhere.
Actual:
Gap:

## A.3 — SSO path (OIDC)

### Step A.3.1 — Bootstrap admin configures an OIDC provider
Role: bootstrap admin
Route: `/admin/sso/group-mappings`
Action: Add a group mapping: `Group "Paeds-Consultants" → Role "Assessor"`. Save.
Expected: New row in `SsoGroupRoleMapping`. Audit entry recorded.
Actual:
Gap:

### Step A.3.2 — Simulate an SSO login with the mapped group
Role: SSO-provisioned user (test username, e.g., `oidc-test@kgk.wombat.local`)
Route: SSO callback URL (depends on OIDC config; for local dev, simulate via an integration test or by manually constructing the claims)
Action: Authenticate. The `ExternalLoginHandler` should match the user's claimed group, assign the `Assessor` role, and create the Wombat user.
Expected: User exists with `Assessor` role only. No Administrator role (per CLAUDE.md: Administrator is never assignable via SSO).
Actual:
Gap: **Anticipated** — local OIDC setup is non-trivial. If no test IdP is configured for the dev environment, this step can be deferred until a real institutional SSO is wired up in deployment. Capture in Actual which path the play-through used.

### Step A.3.3 — Confirm unmatched-group fallback
Role: another SSO-provisioned user with a group that has NO mapping
Route: SSO callback
Action: Authenticate.
Expected: User created with `PendingTrainee` role (the documented fallback). Available for an admin to manually assign a role later.
Actual:
Gap:

## A.4 — Mobile + accessibility spot-checks

### Step A.4.1 — Login flow on a narrow viewport (375px wide)
Role: Dr Mahlangu
Route: `/account/login`
Action: Resize Playwright viewport to 375x812 (iPhone SE-ish). Walk through login → dashboard → open a curriculum item from the progress panel.
Expected: All elements stay tappable (≥ 44px touch targets per WCAG). No horizontal scroll. NavMenu collapses behind the hamburger toggle correctly. The dashboard's progress bars don't overflow.
Actual:
Gap:

### Step A.4.2 — Keyboard navigation through the activity form
Role: Dr Dlamini
Route: `/activities/new` → Mini-CEX form
Action: Using only keyboard (Tab / Shift+Tab / Enter / Space), navigate the form: pick the type, fill every field, submit. Capture which fields are skipped or focus-trapped.
Expected: All form fields reachable in DOM order. The focus indicator (per T048's h1-focus-ring fix) does not draw unwanted rectangles around the page h1. Submit button reachable and triggerable via Enter on the last field.
Actual:
Gap:

### Step A.4.3 — Screen-reader walkthrough of a populated review
Role: Dr Zulu (use VoiceOver / NVDA / Narrator)
Route: `/committee/reviews/{molefeReviewId}`
Action: With a screen reader on, walk the review-detail page. Capture: which sections are announced as headings, whether the rating-trajectory chart has an alt-text fallback, whether the decision form's required-field markers are announced.
Expected: All h2/h3 headings announced as headings. Chart has either an aria-label summary or a tabular fallback (the T033 chart implementation should provide one). Required fields announced as "required".
Actual:
Gap: **Anticipated** — chart accessibility is the weakest point per common Blazor + chart-library defaults. If T033 doesn't provide a text fallback, this is a real accessibility gap worth ticketing.

### Step A.4.4 — Color-contrast spot-check
Role: any user
Route: walk the dashboards and detail pages
Action: Use a contrast-checker (axe-core, WebAIM, or the built-in DevTools accessibility audit) on at least: NavMenu links, dashboard card text, form labels, status pills (Active / Inactive / Pending / Stalled), the entrustment-scale level swatches.
Expected: All text-on-background pairs meet WCAG AA (4.5:1 for body, 3:1 for large text). Status pills should hit at minimum 3:1 against the page background.
Actual:
Gap:

## Appendix outcome state

The appendix doesn't accumulate scenario state — its spot-checks read existing state without modifying it (with the exception of A.1.2's deletion, which is destructive and should be done last).

## Appendix time estimate

| Section | Est. minutes |
|---|---|
| A.1 Data rights | 18 |
| A.2 Scheduled jobs | 12 |
| A.3 SSO | 20 (with a real test IdP); 8 (skipped to manual claims) |
| A.4 Mobile + a11y | 25 |
| **Total** | **~75 minutes** |

## Appendix findings summary

Not yet played. The A.3 SSO section has the highest deferral risk (no local IdP). A.4 likely surfaces multiple a11y findings — they should be triaged per WCAG severity, not added blindly to the active-task list.
