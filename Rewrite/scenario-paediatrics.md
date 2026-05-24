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
3. The Paediatric entrustment ladder is in place (seeded global scale reused until T054).
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
Actual:
Gap:

### Step 1.2 — Create institution
Role: bootstrap Administrator
Route: `/admin/institutions/new` (click `Create institution` from the `/admin/institutions` list).
Action: Name `Kgosi Kgari Teaching Hospital`; Short code `KGK`; Contact email `paeds-admin@kgk.wombat.local`; click `Save`.
Expected: Redirect to `/admin/institutions/{id}`. Institution renders in `/admin/institutions` list with status `Active`.
Actual:
Gap:

### Step 1.3 — Create speciality
Role: bootstrap Administrator
Route: `/admin/institutions/{id}/specialities/new` (click `Specialities` next to KGK in the institutions list, then `Create speciality`).
Action: Name `Paediatrics`; Description `Care of infants, children, and adolescents up to 18 years.`; click `Save`.
Expected: Redirect to the speciality edit page. `Paediatrics` appears in the speciality list for KGK.
Actual:
Gap:

### Step 1.4 — Create sub-speciality
Role: bootstrap Administrator
Route: `/admin/specialities/{specialityId}/sub-specialities/new` (click `Sub-specialities` next to `Paediatrics`, then `Create sub-speciality`).
Action: Name `General Paediatrics`; Description `Core general paediatric training; covers the FCPaed(SA) curriculum.`; click `Save`.
Expected: Redirect to the sub-speciality edit page. `General Paediatrics` appears in the sub-speciality list for Paediatrics. Record the `{specialityId}` and `{subSpecialityId}` — Phase 1.E and Phase 1.F need them for scoped lookups.
Actual:
Gap:

## Phase 1.B — Provision Prof Mbatha (InstitutionalAdmin scoped to KGK)

With KGK in place, the bootstrap admin can now issue Prof Mbatha's invitation. She joins as `InstitutionalAdmin` rather than global `Administrator`: the invitation form only exposes scoped roles, and the Administrator role is reserved for manual-only assignment (per CLAUDE.md). `InstitutionalAdmin` carries the full set of institution-scoped admin powers needed for the rest of Act 1.

### Step 1.5 — Issue invitation for Prof Mbatha
Role: bootstrap Administrator
Route: `/admin/invitations` (the Issue invitation form is embedded in the list page, beside an Active invitations panel).
Action: Email `mbatha@kgk.wombat.local`; Role `InstitutionalAdmin`; Institution `Kgosi Kgari Teaching Hospital`; leave Speciality + Sub-speciality blank; click `Issue invitation`.
Expected: The invitation appears in the `Active invitations` panel with status `Pending`.
Actual:
Gap:

> **Note on Administrator scope:** Wombat's invitation form does not surface the `Administrator` role — that role is global and reserved for manual-only assignment, per the CLAUDE.md SSO rule (which the invitation flow honors). `InstitutionalAdmin` is the strongest role available through invitations; it covers everything Prof Mbatha needs in Act 1. T052 (in `scenario-act1-fixes-plan.md`) tracks the option of re-exposing Administrator with null Institution; until that lands, use InstitutionalAdmin.
>
> **First/Last name capture:** the invitation form does not yet collect First / Last name — Prof Mbatha will set them on the accept-invitation page. T051 tracks adding the fields to the issue form.

### Step 1.6 — Prof Mbatha accepts invitation
Role: invitation recipient (no prior session)
Route: invitation link from the email pipeline. In dev with no SMTP wired, copy the token from the `Invitations` table or the dev log line that prints accept-URLs at issue time.
Action: Open the link; fill `First name` (`Nolwazi`), `Last name` (`Mbatha`), set + confirm password; submit. Then log in as `mbatha@kgk.wombat.local`.
Expected: Submit redirects to `/account/login`. After login, the page header reads `mbatha@kgk.wombat.local` and the InstitutionalAdmin dashboard renders.
Actual:
Gap:

> **Dev-mode note:** `DevUserSeeder` does not create Prof Mbatha. The invitation flow above is the canonical path. If it requires SMTP that is not wired, fall back to issuing the invitation, copying the token from the database, and navigating directly to `/account/accept-invitation/{token}` (or the equivalent route surfaced by `InvitationsList`). Capture any rough edges in the `Gap:` slot.

## Phase 1.C — Entrustment scale (workaround until T054)

Wombat's `EntrustmentScale` entity holds the institution's rating ladder. The rewrite has not yet built an admin CRUD surface for it; the only writer today is `DataSeeder` (`src/Wombat.Infrastructure/Persistence/DataSeeder.cs:113-133`), which seeds a single 5-level scale at boot. T054 (in `scenario-act1-fixes-plan.md`) tracks building the admin surface; until it ships, Paediatrics reuses the seeded scale.

### Step 1.7 — Adopt the seeded entrustment scale
Role: bootstrap Administrator (no action; reference only)
Route: n/a (no admin surface to visit)
Action: None in this revision. Note in the scenario log that Paediatrics adopts the seeded `5-level` scale: `Observe only` / `Direct supervision` / `Indirect supervision` / `Independent` / `Supervises others`. These map closely enough to the ten-Cate ladder the runbook originally specified to support the rest of Act 1 without surprises.
Expected: `GetEntrustmentScalesListQuery` returns one scale with 5 ordered levels. Form/activity-type editors that bind to a scale will pick this one up by default.
Actual:
Gap: **Open — tracked by T054.** When T054 lands, restore the original Step 1.7 (create `Paed General Entrustment Scale` with the 5 ten-Cate labels) and treat the seeded scale as a development seed only.

## Phase 1.D — Define 15 General Paediatrics EPAs

Each EPA gets a code, title, description, category (Core or Elective), and a minimum entrustment level that a registrar must reach by graduation.

### Step 1.8 — Bulk-define 15 EPAs

Role: InstitutionalAdmin (Prof Mbatha from here on unless noted)
Route: `/admin/epas/new` (repeat for each)
Action: For each row in the table below, navigate to the new-EPA form. Fill Sub-speciality `Kgosi Kgari Teaching Hospital / Paediatrics / General Paediatrics` (combobox shows the triple-path label), Code, Title, Description, leave `Required knowledge and skills` blank (or paste a short freeform note — non-blocking), Category, then `Save`.
Expected: Each EPA appears in `/admin/epas` list, scoped to `General Paediatrics`, with status `Active` and version 1 (pending its first curriculum reference).
Actual:
Gap:

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
Role: InstitutionalAdmin
Route: `/admin/curricula/new`
Action: Sub-speciality `Kgosi Kgari Teaching Hospital / Paediatrics / General Paediatrics`; Name `FCPaed(SA) Part 1`; Version `2026.1`; Effective from `2026-01-15`; Effective to leave empty; click `Save`.
Expected: Redirect to `/admin/curricula/{id}`. Curriculum row appears in `/admin/curricula` with status `Active`, 0 items.
Actual:
Gap:

### Step 1.10 — Add 15 curriculum items
Role: InstitutionalAdmin
Route: `/admin/curricula/{id}/items`
Action: For each EPA below, fill the `Add item` form with the values shown, then click `Add item`. Repeat 15 times.
Expected: After all 15 are added, the `Existing items` table lists all 15 rows. Progress bars in the Trainee dashboard will later reference these values.
Actual:
Gap:

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
Action: Key `mini_cex_paed`; Name `Mini-CEX (Paediatrics)`; Scope `Speciality`; Scope Id `<the integer SpecialityId from Step 1.3>` (the field is a numeric spinbutton — read the ID off the URL of `/admin/institutions/{instId}/specialities/{specId}` in another tab; T053 will replace this with a picker); Description `Brief (~20-minute) observed clinical encounter rated on six domains.`; Active checkbox on. Click `Save draft`.
Expected: Draft saved; status banner "Draft saved." Metadata persists across tabs.
Actual:
Gap:

**1.11.b — Form tab**
Action: The builder loads with a default `details` section containing a single `title` Text field — delete both before starting. Then click `Add section` and edit: Key `encounter`, Title `Encounter details`. Add fields (via `Add field` inside the section):
- Key `epa_id`, Label `EPA`, Type `EPA`, Required on.
- Key `assessor_user_id`, Label `Assessor`, Type `User`, Required on.
- Key `setting`, Label `Clinical setting`, Type `Choice`, Options `Inpatient\nOutpatient\nEmergency\nHigh care\nNeonatal`, Required on.
- Key `patient_age_months`, Label `Patient age (months)`, Type `Number`, Required on.
- Key `presenting_complaint`, Label `Presenting complaint`, Type `Long text`, Required on.
Click `Add section` again for the ratings: Key `ratings`, Title `Clinical performance ratings`. Add six fields each Type `Scale`, Required on, Catalogue key left blank (the seeded scale is picked up automatically; T054 will revisit this when per-institution scales become possible):
- `history_taking` / History taking
- `examination` / Physical examination
- `clinical_reasoning` / Clinical reasoning
- `communication` / Communication
- `professionalism` / Professionalism
- `overall_level` / Overall entrustment level
Click `Add section`: Key `feedback`, Title `Feedback`. Add field Key `narrative`, Label `Narrative feedback (strengths and next steps)`, Type `Long text`, Required on.
Expected: Live preview renders the three sections with all 13 fields. EPA and Assessor fields show pickers; Scale fields show the 5-level selector backed by the seeded scale.
Actual:
Gap:

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
Actual:
Gap:

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
Actual:
Gap:

**1.11.e — Publish**
Role: InstitutionalAdmin
Route: `/admin/activity-types/{id}` (still on the same edit page after Step 1.11.d).
Action: After `Save draft` succeeds, the page header reveals `Publish` + `Discard draft` buttons beside `Save draft`. Click `Publish`.
Expected: Status banner "Published version 1." Type appears in `/admin/activity-types` with `Published = v1`, `Draft = None`, `Status = Active`. The `Publish` button disappears on next page load until another draft is saved.
Actual:
Gap:

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
Role: InstitutionalAdmin
Route: `/admin/activity-types`
Action: Scroll the list (or filter by name in the Search box). Confirm 10 Paed entries each with `Scope = Speciality`, `Published = v1`, `Draft = None`, `Status = Active`. (Note: `Published`, `Draft`, and `Status` are separate columns, not a concatenated string.)
Expected: 10 rows. Names match the table above.
Actual:
Gap:

## Act 1 outcome state

After Act 1 completes cleanly, the database contains:
- 1 institution (`Kgosi Kgari Teaching Hospital`).
- 1 speciality (`Paediatrics`).
- 1 sub-speciality (`General Paediatrics`).
- 1 entrustment scale — the seeded global 5-level one, reused by Paediatrics until T054 ships.
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
| 1.C: Entrustment scale (workaround — no action) | 0 |
| 1.D: 15 EPAs | 25 (≈90s each) |
| 1.E: Curriculum + 15 items | 20 |
| 1.F: 10 activity types | 90 (15 min × 6 once the pattern is known + worked example time) |
| **Total** | **~155 minutes** — revise in the plan after the next play-through. Phase 1.F's 90-minute estimate is conservative until the actor-DSL translations in Step 1.12 are validated; expect a 10–20-minute overhead the first time the parser rejects a transition. |

## Act 1 findings summary

*(blank by design; populate inline in each step's `Actual:` and `Gap:` lines when the act is played.)*

The prescription above already reflects the doc-side corrections from the 2026-05-24 Playwright audit (commit `c07b71a`). Code-side gaps not yet closed are tracked by T051–T055 in `Rewrite/scenario-act1-fixes-plan.md`:

- **T051** — first/last name capture on the invitation form (cosmetic; Phase 1.B works without it).
- **T052** — re-expose Administrator role with null institution (would let Phase 1.B grant Prof Mbatha global Administrator; not required for Act 1).
- **T053** — context-aware picker for `Scope Id` (would remove a Step 1.11.a friction).
- **T054** — admin CRUD for `EntrustmentScale` (would replace the Phase 1.C workaround with the original prescription).
- **T055** — Publish button always visible with disabled state (cosmetic; covered by 1.11.e wording).

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
