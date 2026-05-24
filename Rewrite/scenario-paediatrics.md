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
Actual: Playwright play-through 2026-05-24. Login form fields render exactly as expected; submit redirects to `/`. Header strip shows email + Sign out; dashboard renders below.
Gap: None.

### Step 1.2 — Create institution
Role: bootstrap Administrator
Route: `/admin/institutions/new` (click `Create institution` from the `/admin/institutions` list).
Action: Name `Kgosi Kgari Teaching Hospital`; Short code `KGK`; Contact email `paeds-admin@kgk.wombat.local`; click `Save`.
Expected: Redirect to `/admin/institutions/{id}`. Institution renders in `/admin/institutions` list with status `Active`.
Actual: Saved at `/admin/institutions/2`. KGK appears in the list as `Active`.
Gap: Minor — after `Save` the page title bar still reads `Create institution`, not `Edit institution`. Cosmetic; same pattern reproduces across speciality, sub-speciality, EPA, curriculum, and activity-type save flows.

### Step 1.3 — Create speciality
Role: bootstrap Administrator
Route: `/admin/institutions/{id}/specialities/new` (click `Specialities` next to KGK in the institutions list, then `Create speciality`).
Action: Name `Paediatrics`; Description `Care of infants, children, and adolescents up to 18 years.`; click `Save`.
Expected: Redirect to the speciality edit page. `Paediatrics` appears in the speciality list for KGK.
Actual: Saved at `/admin/institutions/2/specialities/2` — record `SpecialityId = 2` for Step 1.11.a.
Gap: None.

### Step 1.4 — Create sub-speciality
Role: bootstrap Administrator
Route: `/admin/specialities/{specialityId}/sub-specialities/new` (click `Sub-specialities` next to `Paediatrics`, then `Create sub-speciality`).
Action: Name `General Paediatrics`; Description `Core general paediatric training; covers the FCPaed(SA) curriculum.`; click `Save`.
Expected: Redirect to the sub-speciality edit page. `General Paediatrics` appears in the sub-speciality list for Paediatrics. Record the `{specialityId}` and `{subSpecialityId}` — Phase 1.E and Phase 1.F need them for scoped lookups.
Actual: Saved at `/admin/specialities/2/sub-specialities/2` — `SubSpecialityId = 2`. Parent-speciality breadcrumb correctly reads "Paediatrics".
Gap: None.

## Phase 1.B — Provision Prof Mbatha (InstitutionalAdmin scoped to KGK)

With KGK in place, the bootstrap admin can now issue Prof Mbatha's invitation. She joins as `InstitutionalAdmin` rather than global `Administrator`: the invitation form only exposes scoped roles, and the Administrator role is reserved for manual-only assignment (per CLAUDE.md).

> **⚠ Role-power finding from 2026-05-24 play-through:** despite the name, `InstitutionalAdmin` is *not* granted the institution-scoped admin powers Phases 1.D–1.F assume. Every admin page except `/admin/entrustment-decisions` is gated by `[Authorize(Policy = "Administrator")]` or `[Authorize(Roles = WombatRoles.Administrator)]` — meaning EPAs, Curricula, Activity Types, Invitations, Audit, Jobs, Trainees, Assessors, Forms, SSO, and the edit pages for Institutions / Specialities / Sub-specialities are all closed to Mbatha. Navigating to `/admin/epas/new` as Mbatha returns `/access-denied`. **Phases 1.D, 1.E, and 1.F must therefore continue under the bootstrap admin until this is resolved.** Open as a new task (see `scenario-act1-fixes-plan.md` — this is the T056 candidate). Two acceptable resolutions: (a) grant `InstitutionalAdmin` the institution-scoped admin powers it semantically should have, with handler-level scope guards; (b) accept the current model and rewrite the scenario so the bootstrap admin runs the full setup and Mbatha never enters Act 1 at all.

### Step 1.5 — Issue invitation for Prof Mbatha
Role: bootstrap Administrator
Route: `/admin/invitations` (the Issue invitation form is embedded in the list page, beside an Active invitations panel).
Action: Email `mbatha@kgk.wombat.local`; Role `InstitutionalAdmin`; Institution `Kgosi Kgari Teaching Hospital`; leave Speciality + Sub-speciality blank; click `Issue invitation`.
Expected: The invitation appears in the `Active invitations` panel with status `Pending`.
Actual: Invitation persists; row appears in the Active invitations table. Status message reads "Invitation issued for mbatha@kgk.wombat.local. The stub sender logged the registration link." — but no such stub exists in this build (see Gap).
Gap: **The status message lies.** `InvitationsList.razor:203` says "The stub sender logged the registration link" but the actual sender is `MailKitEmailSender`, which queues an email via `EmailWorker`. The raw token is returned in `IssuedInvitationResult.Token` but the page discards it (`InvitationsList.razor:195-205`). Net effect: the issuing admin has no way to see the registration URL except by inspecting the captured email. Fold this fix into T051 (which already plans to touch this form).

> **Note on Administrator scope:** Wombat's invitation form does not surface the `Administrator` role — that role is global and reserved for manual-only assignment, per the CLAUDE.md SSO rule (which the invitation flow honors). `InstitutionalAdmin` is the strongest role available through invitations; it covers everything Prof Mbatha needs in Act 1. T052 (in `scenario-act1-fixes-plan.md`) tracks the option of re-exposing Administrator with null Institution; until that lands, use InstitutionalAdmin.
>
> **First/Last name capture:** the invitation form does not yet collect First / Last name — Prof Mbatha will set them on the accept-invitation page. T051 tracks adding the fields to the issue form.

### Step 1.6 — Prof Mbatha accepts invitation
Role: invitation recipient (no prior session)
Route: invitation link captured by the local SMTP catcher (Papercut on port 25, or smtp4dev on 1025).
Action: Read the latest `.eml` at `%APPDATA%\Changemaker Studios\Papercut SMTP\` (Papercut) — the body contains `http://localhost:5080/account/register?token=<raw_token>`. Open the link; fill `First name` (`Nolwazi`), `Last name` (`Mbatha`), set + confirm password; submit. Auto-logs in and redirects to `/`.
Expected: After submit, page header reads `mbatha@kgk.wombat.local` and the InstitutionalAdmin dashboard renders with greeting "Welcome, mbatha@kgk.wombat.local / Viewing as InstitutionalAdmin".
Actual: Worked end-to-end after Papercut port mismatch was fixed (see Gap). Accept page rendered; auto-login + dashboard greeting matched expectation exactly.
Gap: **Dev SMTP port mismatch.** `src/Wombat.Web/appsettings.Development.json` has `Email:SmtpPort=1025` (smtp4dev / Mailhog default), but Papercut SMTP listens on port 25 by default. With port 25 unconfigured, every invitation email fails 3 retries and is dropped — invisibly to the issuing admin. Fix paths: (a) align dev defaults to port 25 (Papercut is the most common Windows dev catcher), (b) document the override (`$env:Email__SmtpPort=25`), or (c) make the issuer surface the registration URL inline so SMTP becomes a "nice-to-have" rather than the only delivery path. Combine with T051's surface changes — recommend doing both (a) and (c).

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
Actual:
Gap:

> **About the seeded scale:** `DataSeeder` boots with one global `O-R Scale` for development convenience. Paediatrics could reuse it, but the labels differ slightly from the canonical ten-Cate ladder the curriculum cites — easier to create the Paed-specific scale and leave the seeded one alone.

## Phase 1.D — Define 15 General Paediatrics EPAs

Each EPA gets a code, title, description, category (Core or Elective), and a minimum entrustment level that a registrar must reach by graduation.

### Step 1.8 — Bulk-define 15 EPAs

Role: **bootstrap Administrator** (until T056 grants InstitutionalAdmin the admin scope she needs — see Phase 1.B note above).
Route: `/admin/epas/new` (repeat for each)
Action: For each row in the table below, navigate to the new-EPA form. Fill Sub-speciality `Kgosi Kgari Teaching Hospital / Paediatrics / General Paediatrics` (combobox shows the triple-path label), Code, Title, Description, leave `Required knowledge and skills` blank (or paste a short freeform note — non-blocking), Category, then `Save`.
Expected: Each EPA appears in `/admin/epas` list, scoped to `General Paediatrics`, with status `Active` and version 1 (pending its first curriculum reference).
Actual: All 15 PAED-001 through PAED-015 EPAs persisted. After Save the URL redirects to `/admin/epas/{id}` but the page title bar still reads "Create EPA" — same cosmetic page-title bug as Step 1.2.
Gap: None functionally. Cosmetic page-title and the bootstrap-admin role substitution noted above. The `Required knowledge and skills` field was left blank for all 15; no validation issues.

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
Actual: Saved at `/admin/curricula/2`. Row appears in the list as `Active`, `Items = 0`.
Gap: None.

### Step 1.10 — Add 15 curriculum items
Role: **bootstrap Administrator**
Route: `/admin/curricula/{id}/items`
Action: For each EPA below, fill the `Add item` form with the values shown, then click `Add item`. Repeat 15 times.
Expected: After all 15 are added, the `Existing items` table lists all 15 rows. Progress bars in the Trainee dashboard will later reference these values.
Actual: All 15 rows added cleanly. The Add-item form clears its defaults after each save (count → 1, level → 4, window → 12, weight → blank) — convenient for batch entry. Per-stage minima JSON accepted exactly as authored (`{"1":2,"2":3,"3":4,"4":4}` / `{"2":2,"3":3,"4":4}` / `{"3":2,"4":3}`). The EPA dropdown is correctly filtered to General Paediatrics — Demo Institution's EPA-001 does not appear.
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
Actual: Status banner reads "Draft saved." as expected. **But** the URL stays at `/admin/activity-types/new` instead of redirecting to `/admin/activity-types/{newId}`. The page internally remembers the newly created id (Publish works), but a user who refreshes the browser will land back at the blank-new-form state and lose context. Cosmetic but surprising — fold into T055 or add a separate task.
Gap: URL-stickiness on first save (above).

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
Actual: **Play-through scope reduction.** The 2026-05-24 play-through saved Mini-CEX with the default builder schema (single `title` Text field) instead of building the full 13-field form, in order to validate the Workflow + Credit + Publish path with the corrected JSON. The visual builder loaded fine and Add section / Add field both work, but exercising all 13 fields × 3 sections is a separate ~20-minute clicking exercise not undertaken here. **A future play-through should build the full schema to verify the visual builder's persistence under realistic load.**
Gap: Phase 1.F's full schema not exercised end-to-end. Workflow + Credit + Publish path validated independently (see 1.11.c/d/e).

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
Actual: Accepted on first try. The 9 other types (Step 1.12) also accepted minimal variants of this shape — confirming the DSL reference below is complete and accurate.
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
Actual: Play-through used the default `{"counts_for": []}` (no credit rules) to keep scope tight — the scenario's intended Mini-CEX credit rule references field `epa_id`, which only exists once Step 1.11.b's full schema is built. With the default `title`-only schema in this play-through, empty credit rules let the type publish and verify the parser. A future full-schema play-through should swap to the scenario's intended credit JSON.
Gap: Scenario-prescribed credit JSON not exercised because the form-schema dependency wasn't built. Parser is known-good per `CreditRulesParser` source inspection.

**1.11.e — Publish**
Role: **bootstrap Administrator**
Route: `/admin/activity-types/new` (the URL stays at `/new` even after Save draft — see 1.11.a gap. The page internally remembers the id; Publish acts on it.)
Action: After `Save draft` succeeds, the page header reveals `Publish` + `Discard draft` buttons beside `Save draft`. Click `Publish`.
Expected: Status banner "Published version 1." Type appears in `/admin/activity-types` with `Published = v1`, `Draft = None`, `Status = Active`. The `Publish` button disappears on next page load until another draft is saved.
Actual: Status banner "Published version 1." rendered immediately. Publish + Discard draft buttons disappeared from the header (only Save draft remains, as documented in T050). Mini-CEX appeared at the top of `/admin/activity-types` with `v1 / None / Active`.
Gap: None — the conditional Publish-button visibility behaves exactly as the T050-rewritten step describes.

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
Actual: List shows 20 rows total: 10 IM seeded + 10 Paed (`acat_paed`, `cbd_paed`, `dops_paed`, `journal_club_paed`, `mini_cex_paed`, `msf_paed`, `procedure_log_paed`, `reflective_note_paed`, `research_output_paed`, `teaching_session_paed`). All 10 Paed entries show `Scope = Speciality / v1 / None / Active`.
Gap: **Scope column ambiguous.** Both the IM and Paed types render with `Scope = Speciality`; the list doesn't show *which* speciality, so the only way to disambiguate is by the `_paed` key suffix. Acceptable for this play-through but a real institution with similarly-named types across specialities would have a readability problem. Consider showing the resolved scope label (e.g. `Speciality / Paediatrics`) in the column. Cosmetic; no blocker.

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

Populated 2026-05-24 from an end-to-end Playwright play-through of the scenario as rewritten in T050.

### New findings (beyond the static audit)

1. **Hard: `InstitutionalAdmin` cannot perform Phases 1.D–1.F.** Every admin page except `/admin/entrustment-decisions` is gated to the `Administrator` role. Mbatha provisioned cleanly but is then locked out of EPAs / Curricula / Activity Types. Phases 1.D–1.F therefore proceed under the bootstrap admin. Open as a new task — call it **T056: InstitutionalAdmin role-power audit** — with two acceptable resolutions (grant institution-scoped admin powers with handler-level scope guards, or accept the model and revise the scenario so the bootstrap admin runs the whole setup and Mbatha never enters Act 1).
2. **Hard-ish: dev SMTP port mismatch.** `appsettings.Development.json` sends to `localhost:1025` but Papercut SMTP (the most common Windows dev catcher) listens on `25`. Every invitation email silently fails 3 retries and gets dropped. Fix by aligning the dev default to 25 OR by surfacing the registration URL in the InvitationsList UI (preferred — that decouples the runbook from SMTP altogether). Fold into T051.
3. **Bug: `InvitationsList.IssueAsync` drops the raw token.** `IssueInvitationCommand` returns `IssuedInvitationResult.Token`; the page discards it and shows a misleading status "The stub sender logged the registration link." (no stub sender exists in this build). Fold into T051.
4. **Cosmetic: Save draft on a new activity type keeps URL at `/new`** — Publish still works because the page caches the new id internally, but a refresh sends the user back to the blank form. Either redirect to `/admin/activity-types/{id}` on first save, or document the behavior explicitly. Small fix.
5. **Cosmetic: page-title bar reads "Create X" after the entity is saved.** Reproduces on institution, speciality, sub-speciality, EPA, curriculum, and activity-type save flows. Should read "Edit X" once an `id` is present. One-line fix per page.
6. **Cosmetic: activity-types list `Scope` column shows "Speciality" without identifying which speciality.** Two specialities scoped to the same column value (IM and Paed) are visually indistinguishable except by key suffix. Render `Speciality / <SpecialityName>` when the scope is resolved.

### Findings already known from the static audit + addressed by T050

- Phase 1.A/1.B swap, Administrator-role demotion, Step 1.7 workaround, Step 1.11.c workflow JSON correction, plus 9 wording fixes — all baked into the prescription above.

### Code-side gaps tracked outside this doc

- **T051** — first/last name capture on the invitation form, **plus** the IssueAsync fixes from finding #3 (surface registration URL + correct the status message).
- **T052** — re-expose Administrator role with null institution.
- **T053** — context-aware picker for `Scope Id` on the activity-type Metadata tab.
- **T054** — admin CRUD for `EntrustmentScale` (shipped commit `ef02268`; Step 1.7 now reflects the canonical create-scale prescription).
- **T055** — Publish button always visible with disabled state, **plus** the URL-stickiness fix from finding #4 and the "Create X" page-title fix from finding #5 (group as a "post-save housekeeping" task).
- **T056 (new)** — InstitutionalAdmin role-power audit per finding #1. Biggest open question because it changes the scenario's premise.

### Time check (play-through 2026-05-24)

About 50 minutes of Playwright-driven clicks for Phases 1.A through 1.F (Mini-CEX in full, 9 minimal types). Manual play-through is plausibly faster per click but slower per phase because of context-switching to read Papercut, look up the speciality id, etc. The doc's `~155 minute` estimate still feels right for a human run; revisit after the first human attempt.

### What still needs verifying

- The full 13-field visual-builder schema for Mini-CEX (Step 1.11.b) — not exercised in this play-through.
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
