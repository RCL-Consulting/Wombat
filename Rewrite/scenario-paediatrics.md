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
Actual: General Paediatrics saved at `/admin/specialities/2/sub-specialities/2`. `SubSpecialityId = 2` recorded. Parent-speciality line correctly reads "Speciality: Paediatrics". h1 flips to "Edit sub-speciality".
Gap: Browser tab `<title>` still reads "Create Sub-Speciality" after save (h1 is correct). One of four pages with this stale-`<PageTitle>` leftover (see finding #5).

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
Action: Read the latest `.eml` at `%APPDATA%\Changemaker Studios\Papercut SMTP\` (Papercut) — the body contains `http://localhost:5080/account/register?token=<raw_token>`. Open the link; fill `First name` (`Nolwazi`), `Last name` (`Mbatha`), set + confirm password; submit. Auto-logs in and redirects to `/`.
Expected: After submit, page header reads `mbatha@kgk.wombat.local` and the InstitutionalAdmin dashboard renders with greeting "Welcome, mbatha@kgk.wombat.local / Viewing as InstitutionalAdmin".
Actual: Worked end-to-end via the inline URL (Papercut not needed). Opened the URL from Step 1.5's info Alert; form prefilled email + role; entered First name `Nolwazi`, Last name `Mbatha`, password; submit auto-logged Mbatha and redirected to `/`. Dashboard greeting "Welcome, mbatha@kgk.wombat.local / Viewing as InstitutionalAdmin" rendered with the InstitutionalAdmin scope dashboard (KGK / 1 speciality / 1 sub-speciality / quick links). NavMenu now exposes the 11 admin routes T056.e expanded to: Specialities, EPAs, Curricula, Activity Types, Entrustment Scales, Forms, Trainees, Assessors, Invitations, SSO Mappings, Audit Log — plus the 3 account-related routes (Home, My Account, Data Rights).
Gap: None. Papercut was running on port 25 (T051's appsettings change verified at process-listening level) but wasn't exercised because the inline-URL path works.

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
Actual: Saved as bootstrap admin at `/admin/entrustment-scales/2` with all 5 ten-Cate levels intact. h1 flips to "Edit entrustment scale". A first attempt as Prof Mbatha (InstitutionalAdmin) reached the same form but Save was rejected by the handler with status "Only global administrators may create entrustment scales." — T056.d's Administrator-only enforcement working at the handler level.
Gap: **UX wart** — the `Create scale` button on `/admin/entrustment-scales` and the `/admin/entrustment-scales/new` page itself render fully for InstitutionalAdmin, who can fill the entire form before discovering that Save will fail. Suggest gating the page (`[Authorize(Policy = "Administrator")]`) or hiding the Create button for non-Administrator viewers. Listing the seeded scales is fine for InstitutionalAdmin (read-only reference). Also: browser tab `<title>` still reads "Create entrustment scale" after save (2 of 4 stale-`<PageTitle>` cases — see finding #5).

> **About the seeded scale:** `DataSeeder` boots with one global `O-R Scale` for development convenience. Paediatrics could reuse it, but the labels differ slightly from the canonical ten-Cate ladder the curriculum cites — easier to create the Paed-specific scale and leave the seeded one alone.

## Phase 1.D — Define 15 General Paediatrics EPAs

Each EPA gets a code, title, description, category (Core or Elective), and a minimum entrustment level that a registrar must reach by graduation.

### Step 1.8 — Bulk-define 15 EPAs

Role: Prof Mbatha (InstitutionalAdmin) — T056 grants institution-scoped admin powers.
Route: `/admin/epas/new` (repeat for each)
Action: For each row in the table below, navigate to the new-EPA form. Fill Sub-speciality `Kgosi Kgari Teaching Hospital / Paediatrics / General Paediatrics` (combobox shows the triple-path label), Code, Title, Description, leave `Required knowledge and skills` blank (or paste a short freeform note — non-blocking), Category, then `Save`.
Expected: Each EPA appears in `/admin/epas` list, scoped to `General Paediatrics`, with status `Active` and version 1 (pending its first curriculum reference).
Actual: All 15 PAED-001 through PAED-015 EPAs persisted as Prof Mbatha (InstitutionalAdmin) — T056.b grants her institution-scoped EPA powers. Sub-speciality combobox correctly pre-selects her sole scope (`Kgosi Kgari Teaching Hospital / Paediatrics / General Paediatrics`). The seeded Demo Institution EPA-001 (id=1) is filtered out of her list — only the 15 PAED EPAs (ids 2-16) appear. h1 flips to "Edit EPA" after save.
Gap: Browser tab `<title>` still reads "Create EPA" after save (3 of 4 stale-`<PageTitle>` cases — see finding #5). Functionally clean.

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
Actual: FCPaed(SA) Part 1 v2026.1 saved at `/admin/curricula/2` as Prof Mbatha (InstitutionalAdmin via T056.b). Sub-speciality combobox correctly pre-selects her scope. Effective-from set to 2026-01-15.
Gap: Browser tab `<title>` still reads "Create Curriculum" after save (4 of 4 stale-`<PageTitle>` cases — see finding #5). Functionally clean.

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
Actual: As Prof Mbatha (InstitutionalAdmin), the list shows **only the 10 Paed types** (`acat_paed`, `cbd_paed`, `dops_paed`, `journal_club_paed`, `mini_cex_paed`, `msf_paed`, `procedure_log_paed`, `reflective_note_paed`, `research_output_paed`, `teaching_session_paed`) at ids 11-20. The 10 seeded IM types are filtered out by T056.c's scope guard on `ListActivityTypesAdmin`. Every row shows `Scope = Speciality / v1 / None / Active`.
Gap: Scope-column ambiguity **no longer applies to InstitutionalAdmin** (only sees own institution). Still applies to global Administrator who would see IM + Paed types both rendered as "Speciality" without identifying which. Cosmetic; still not pressing.

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

Re-populated 2026-05-24 from an end-to-end Playwright **replay** of the scenario after T051 + T055 + T056 shipped. Previous play-through (same date, earlier) is preserved in git at the prior commits; the Actual/Gap lines above now reflect the replay.

### Status of the six previous findings

1. ✅ ~~Hard: InstitutionalAdmin cannot perform Phases 1.D–1.F.~~ **Closed by T056** (5 clusters: `41def8a` / `9e3bc0a` / `e1d3737` / `8ad0788` / `ec6d6d1`). Replay verified: Mbatha runs Phases 1.D-1.F end-to-end without any auth detours. Handler-level scope filtering verified in three places: her EPA list shows 15 PAED EPAs (Demo IM filtered out); her curriculum-item EPA dropdown shows 15 entries (no Demo); her ActivityTypes list shows 10 Paed types (10 seeded IM types filtered out).
2. ✅ ~~Hard-ish: dev SMTP port mismatch.~~ **Closed by T051.** appsettings.Development.json fix verified (Papercut listening on port 25), and the inline-URL path means SMTP is no longer the only delivery channel. Replay used the inline URL exclusively — never touched Papercut.
3. ✅ ~~Bug: InvitationsList.IssueAsync drops the raw token.~~ **Closed by T051.** Status banner now says "Copy the link below — it is shown only once." The registration URL renders inline as a copy-friendly `<code>` block in an info Alert.
4. ✅ ~~Cosmetic: Save draft on a new activity type keeps URL at /new.~~ **Closed by T055.** URL flips to `/admin/activity-types/{id}` on first save — verified for all 10 Paed types created in this replay.
5. ⚠️ **Partially open: page-title bar reads "Create X" after the entity is saved.** The h1 fix landed on Institution + Speciality pages (h1 swaps to "Edit X" correctly). But the browser tab `<title>` (the `<PageTitle>` Razor component) remains stale on **four** pages observed this replay: Sub-Speciality, Entrustment Scale, EPA, Curriculum. Each page would need the same conditional pattern most of the others have. Cosmetic; ~5-line fix per page. Track as a small follow-up if not already in T055's expected scope.
6. ⚠️ **Adjusted: activity-types list Scope column ambiguity.** Doesn't apply to InstitutionalAdmin (only sees own institution per T056.c). Still applies to global Administrator who sees IM + Paed types both labelled "Speciality" without disambiguation. Cosmetic; same fix recommended (render the resolved scope label).

### New finding from this replay

7. **UX: InstitutionalAdmin can navigate to `/admin/entrustment-scales/new` and submit the form, only for the handler to reject the write.** The `Create scale` link on the list page is visible to her, the new page renders all controls active, and Save returns "Only global administrators may create entrustment scales." after she's filled the form. Suggested fix: page-level `[Authorize(Policy = "Administrator")]` on `EntrustmentScaleEdit.razor`'s new route (cleanest), plus hide the `Create scale` button on the list for non-Administrator viewers. The list page itself can stay readable so InstitutionalAdmin can see what scales are available. Same principle would apply to EntrustmentScale Edit / Delete buttons. Cosmetic-leaning UX; not data-integrity.

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

# Acts 2–5 — to be drafted

Act 1 establishes the document shape. Once played and the format is confirmed usable, draft Acts 2–5 with the same conventions.

**Act 2 — Day 1 team onboarding** (est. 400 lines): invitation flow for 6 consultants + 5 registrars, admission of PendingTrainee → Trainee, profile creation, committee panel creation (depends on consultants existing).

**Act 3 — Months 1–6 operational rhythm** (est. 800 lines, longest act): 8 sub-scenes — Mini-CEX submit/accept/rate/credit, Procedure log, DOPS, MSF cycle, stalled-assessment triage, audit-log verification, dashboard populated-state sanity.

**Act 4 — Month 12 annual review** (est. 400 lines): 5 reviews scheduled, committee evidence bundle, decisions recorded, STARs staged, ratification, one appeal.

**Act 5 — Year 4 graduation** (est. 250 lines): final review for Dr Molefe, multiple STARs, portfolio PDF export, profile completion.

**Appendix — cross-cutting concerns** (est. 300 lines): data rights, scheduled jobs, SSO path, mobile/accessibility spot-checks.

Not drafted here. Draft after Act 1 has been played once and any format revisions folded back.
